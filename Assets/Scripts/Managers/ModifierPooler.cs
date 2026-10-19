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

    public Modifier GetModifier(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[ModifierPooler] 유효하지 않은 모디파이어 ID입니다!");
            return null;
        }
        if(modifierDataManager == null)
        {
            modifierDataManager = modifierManager.modifierDataManager;
        }
        ModifierData modData = modifierDataManager.GetModifierData(id);
        
        string baseID = modData.modifierId;
        // 1. 비활성 큐와 활성 리스트가 없으면 초기화
        if (!inactiveModifiers.ContainsKey(baseID))
        {
            inactiveModifiers[baseID] = new Queue<IPoolScript>();
        }
        if (!activeModifiers.ContainsKey(baseID))
        {
            activeModifiers[baseID] = new List<IPoolScript>();
        }

        // 2. 큐가 비어 있으면 새로 생성해서 큐에 삽입
        if (inactiveModifiers[baseID].Count == 0)
        {
            CreateNewBase(baseID);
        }

        // 3. 큐에서 하나 꺼내서 활성 리스트로 이동
        IPoolScript value = inactiveModifiers[baseID].Dequeue();
        Modifier mod = value as Modifier;
        // [Fix] ReturnModifier에서 Reset() 호출로 인해 데이터가 날아가므로 재초기화
        if (modifierDataManager == null)
        {
            modifierDataManager = modifierManager.modifierDataManager;
        }
        if (modifierDataManager != null)
        {
            if (modData != null)
            {
                mod.SetData(modData);
            }
            Modifier baseMod = modifierDataManager.Get_ModifierBase(modData.modifierId);
            if (baseMod != null)
            {
                mod.Set_BaseModData(baseMod);
            }   
        }

        activeModifiers[baseID].Add(value);

        return mod;
    }

    IPoolScript CreateNewBase(string baseID)
    {
        if(modifierDataManager == null)
        {
            modifierDataManager = modifierManager.modifierDataManager;
        }   
        Modifier newMod = modifierFactory.CreateNewModifier(baseID); //새로운 모디파이어 생성
        //ModifierData modData = modifierDataManager.GetModifierData(id); //데이터 가져오기
        Modifier baseMod = modifierDataManager.Get_ModifierBase(baseID);
        //newMod.SetData(modData); // 데이터세팅
        newMod.Set_BaseModData(baseMod); // 베이스 모디파이어 세팅
        inactiveModifiers[baseID].Enqueue(newMod); // 방금 생성한 객체를 큐에 바로 적재
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

        string id = modifier.baseID;

        // 1. 활성 리스트에서 제거
        if (activeModifiers.ContainsKey(id))
        {
            activeModifiers[id].Remove(script);
        }

        // 2. 비활성 큐에 반환 (큐가 없으면 생성하여 보관)
        if (!inactiveModifiers.ContainsKey(id))
        {
            inactiveModifiers[id] = new Queue<IPoolScript>();
        }
        modifier.Reset(); // ?? 풀 반납 전 내부 수치 초기화
        inactiveModifiers[id].Enqueue(script);
    }
}

