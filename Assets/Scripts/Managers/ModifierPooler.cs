using UnityEngine;
using System.Collections.Generic;

public class ModifierPooler
{
    // ModifierType을 무시하고 id(문자열)를 키값으로 사용하는 객체 풀링 딕셔너리
    public Dictionary<string, Queue<IPoolScript>> inactiveModifiers = new Dictionary<string, Queue<IPoolScript>>();
    public Dictionary<string, List<IPoolScript>> activeModifiers = new Dictionary<string, List<IPoolScript>>();

    public Dictionary<string, Queue<IPoolScript>> inactiveModifierOptions = new();
    public Dictionary<string, List<IPoolScript>> activeModifierOptions = new();

    private ModifierManager modifierManager;
    private ModifierFactory modifierFactory;
    private ModifierDataManager modifierDataManager;

    private bool prewarmDone;
    private bool growWarned;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private class PoolStat
    {
        public int rentCount;
        public int returnCount;
        public int createCount;
        public int peakActive;
    }
    private Dictionary<string, PoolStat> modifierStats = new Dictionary<string, PoolStat>();
#endif

    public void Set_ModifierManager(ModifierManager MM, ModifierFactory MF,ModifierDataManager MD)
    {
        modifierManager = MM;
        modifierFactory = MF;
        modifierDataManager = MD;
    }

    public ModifierOption GetModifierOption(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[ModifierPooler] 유효하지 않은 옵션 ID");
            return null;
        }
        if (!inactiveModifierOptions.ContainsKey(id))
        {
            inactiveModifierOptions[id] = new Queue<IPoolScript>();
        }
        if (!activeModifierOptions.ContainsKey(id))
        {
            activeModifierOptions[id] = new List<IPoolScript>();
        }
        if (inactiveModifierOptions[id].Count == 0)
        {
            CreateNewOption(id);
        }

        // 풀에서 옵션 껍데기 객체를 꺼냅니다 (OnGet)
        IPoolScript value = inactiveModifierOptions[id].Dequeue();
        ModifierOption option = value as ModifierOption;
        
        // [Fix] Reset()에 의해 optionKey가 초기화될 수 있으므로 다시 할당
        option.optionKey = id;
        
        // ?? [중요] 재사용 시 상태 초기화 및 연쇄 조립
        // 풀에서 꺼낸 ModifierOption은 이전 반납 과정에서 하위 모디파이어가 모두 비워져 있으므로,
        // 팩토리의 AssembleOption을 통해 풀러에서 다시 개별 모디파이어들을 가져와 채워줍니다.
        if(modifierFactory == null)
        {
            modifierFactory = modifierManager.GetModifierFactory();
        }
        modifierFactory.AssembleOption(option, id);

        activeModifierOptions[id].Add(value);

        return option;
    }

    // Rents by value row. The pool key is the base id (modifierId) of the value row,
    // and every value field is injected here, so callers never touch SetModifierValue.
    public Modifier RentByValue(ModifierValueData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[ModifierPooler] RentByValue: value data is null");
            return null;
        }
        Modifier mod = RentBase(data.modifierId);
        if (mod == null)
        {
            return null;
        }
        mod.SetModifierValue(data);
        return mod;
    }

    public Modifier RentByValueId(string valueId)
    {
        if (modifierDataManager == null)
        {
            modifierDataManager = modifierManager.modifierDataManager;
        }
        return RentByValue(modifierDataManager.Get_ModifierValueData(valueId));
    }

    // Rents without value data (only the base template is applied). Temporary compatibility path.
    [System.Obsolete("RentBaseOnly injects no value data. Use RentByValue / RentByValueId.")]
    public Modifier RentBaseOnly(string baseID)
    {
        return RentBase(baseID);
    }

    // baseID = ID column of Mod_*.csv. This is the pool key. No value data is injected here.
    private Modifier RentBase(string baseID)
    {
        if (string.IsNullOrEmpty(baseID))
        {
            Debug.LogWarning("[ModifierPooler] invalid base modifier id");
            return null;
        }
        if(modifierDataManager == null)
        {
            modifierDataManager = modifierManager.modifierDataManager;
        }
        Modifier baseMod = modifierDataManager.Get_ModifierBase(baseID);
        if (baseMod == null)
        {
            return null;
        }

        EnsurePool(baseID, 4, out Queue<IPoolScript> inactive, out List<IPoolScript> active);

        if (inactive.Count == 0)
        {
            CreateNewBase(baseID);
        }

        IPoolScript value = inactive.Dequeue();
        Modifier mod = value as Modifier;
        mod.isInPool = false;
        // Reset() keeps baseID, but the base data is always re-applied on rent.
        mod.id = baseID;
        mod.baseID = baseID;
        mod.Set_BaseModData(baseMod);

        active.Add(value);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        PoolStat stat = GetStat(baseID);
        stat.rentCount++;
        if (active.Count > stat.peakActive) stat.peakActive = active.Count;
#endif
        return mod;
    }

    IPoolScript CreateNewBase(string baseID)
    {
        if(modifierDataManager == null)
        {
            modifierDataManager = modifierManager.modifierDataManager;
        }
        if(modifierFactory == null)
        {
            modifierFactory = ModifierFactory._instance != null ? ModifierFactory._instance : modifierManager.GetModifierFactory();
        }
        Modifier newMod = modifierFactory.CreateNewModifier(baseID);
        newMod.baseID = baseID;
        newMod.isInPool = true;
        inactiveModifiers[baseID].Enqueue(newMod);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GetStat(baseID).createCount++;
#endif
        if (prewarmDone && !growWarned)
        {
            growWarned = true;
            Debug.LogWarning("[ModifierPooler] pool grew after prewarm: " + baseID);
        }
        return newMod;
    }

    IPoolScript CreateNewOption(string id)
    {
        if(modifierFactory == null)
        {
            modifierFactory = modifierManager.GetModifierFactory();
        }

        // 순수하게 빈 껍데기(ModifierOption)만 생성해서 큐에 넣습니다.
        // 세부 조립(AssembleOption)은 풀에서 꺼내어질 때(GetModifierOption) 수행됩니다.
        ModifierOption newOption = new ModifierOption();
        newOption.optionKey = id;

        if (!inactiveModifierOptions.ContainsKey(id))
        {
            inactiveModifierOptions[id] = new Queue<IPoolScript>();
        }
        inactiveModifierOptions[id].Enqueue(newOption);
        return newOption;
    }

    public void ReturnModifierOption(IPoolScript script)
    {
        ModifierOption modOption = script as ModifierOption;
        if (modOption == null) return;

        string id = modOption.optionKey;

        // ?? [중요] 연쇄 해제 (Cascading Release) 로직 구현
        // ModifierOption이 반납될 때, 가지고 있던 개별 Modifier들을 절대 버리지 않고
        // 먼저 ModifierPooler로 하나씩 안전하게 반납합니다.
        if (modOption.myMods != null)
        {
            for(int i = 0; i < modOption.myMods.Count; i++)
            {
                ReturnModifier(modOption.myMods[i]);
            }
            // 하위 객체 반납이 끝난 후 리스트를 비워 메모리 참조를 끊습니다.
            modOption.myMods.Clear();
        }

        // 활성 리스트에서 제거
        if (activeModifierOptions.ContainsKey(id))
        {
            activeModifierOptions[id].Remove(script);
        }
        
        // 비활성 큐(풀)에 반납
        if (!inactiveModifierOptions.ContainsKey(id))
        {
            inactiveModifierOptions[id] = new Queue<IPoolScript>();
        }
        inactiveModifierOptions[id].Enqueue(script);
    }

    public void ReturnModifier(IPoolScript script)
    {
        Modifier modifier = script as Modifier;
        if (modifier == null) return;

        // The pool key (baseID) must be read BEFORE Reset(). Reset() must never clear baseID/id.
        string key = modifier.baseID;
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[ModifierPooler] ReturnModifier: baseID is empty (object not created by the pool): " + modifier.GetType().Name);
            return;
        }
        if (modifier.isInPool)
        {
            Debug.LogWarning("[ModifierPooler] ReturnModifier: duplicate return ignored: " + key);
            return;
        }

        // Only enqueue when the object was really removed from the active list (blocks duplicate / foreign returns).
        if (!activeModifiers.TryGetValue(key, out List<IPoolScript> activeList) || !activeList.Remove(script))
        {
            Debug.LogWarning("[ModifierPooler] ReturnModifier: duplicate or foreign return ignored: " + key);
            return;
        }

        modifier.Reset();
        modifier.isInPool = true;

        if (!inactiveModifiers.TryGetValue(key, out Queue<IPoolScript> inactiveQueue))
        {
            inactiveQueue = new Queue<IPoolScript>();
            inactiveModifiers[key] = inactiveQueue;
        }
        inactiveQueue.Enqueue(script);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GetStat(key).returnCount++;
#endif
    }

    // ---------------------------------------------------------------
    // Prewarm
    // ---------------------------------------------------------------
    public void Prewarm(string baseID, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(baseID)) return;
        if (modifierDataManager == null)
        {
            modifierDataManager = modifierManager.modifierDataManager;
        }
        if (!modifierDataManager.HasModifierBase(baseID))
        {
            Debug.LogWarning("[ModifierPooler] Prewarm: unknown base id " + baseID);
            return;
        }

        EnsurePool(baseID, count, out Queue<IPoolScript> inactive, out List<IPoolScript> active);
        while (inactive.Count < count)
        {
            CreateNewBase(baseID);
        }
    }

    public void PrewarmAll(IReadOnlyDictionary<string, int> plan)
    {
        if (plan == null) return;
        foreach (KeyValuePair<string, int> pair in plan)
        {
            Prewarm(pair.Key, pair.Value);
        }
        prewarmDone = true;
    }

    // Builds the default plan from the loaded data and prewarms every base.
    public void PrewarmFromData(ModifierDataManager dataManager)
    {
        if (dataManager == null) return;
        modifierDataManager = dataManager;
        PrewarmAll(dataManager.Get_PrewarmPlan());
    }

    private void EnsurePool(string baseID, int capacity, out Queue<IPoolScript> inactive, out List<IPoolScript> active)
    {
        if (!inactiveModifiers.TryGetValue(baseID, out inactive))
        {
            inactive = new Queue<IPoolScript>(capacity);
            inactiveModifiers[baseID] = inactive;
        }
        if (!activeModifiers.TryGetValue(baseID, out active))
        {
            active = new List<IPoolScript>(capacity);
            activeModifiers[baseID] = active;
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private PoolStat GetStat(string baseID)
    {
        if (!modifierStats.TryGetValue(baseID, out PoolStat stat))
        {
            stat = new PoolStat();
            modifierStats[baseID] = stat;
        }
        return stat;
    }

    // Logs rent/return/create/peak counters per base and checks the pool invariants.
    public void DumpStats()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[ModifierPooler] stats (base: rent/return/create/peakActive | inactive/active)");
        foreach (KeyValuePair<string, PoolStat> pair in modifierStats)
        {
            PoolStat s = pair.Value;
            int inactiveCount = inactiveModifiers.TryGetValue(pair.Key, out Queue<IPoolScript> q) ? q.Count : 0;
            int activeCount = activeModifiers.TryGetValue(pair.Key, out List<IPoolScript> l) ? l.Count : 0;
            bool ok = (s.createCount == inactiveCount + activeCount) && (s.rentCount - s.returnCount == activeCount);
            sb.Append(pair.Key).Append(": ").Append(s.rentCount).Append('/').Append(s.returnCount).Append('/')
              .Append(s.createCount).Append('/').Append(s.peakActive).Append(" | ")
              .Append(inactiveCount).Append('/').Append(activeCount).AppendLine(ok ? string.Empty : "  <-- INVARIANT BROKEN");
        }
        Debug.Log(sb.ToString());
    }
#else
    public void DumpStats() { }
#endif
}

