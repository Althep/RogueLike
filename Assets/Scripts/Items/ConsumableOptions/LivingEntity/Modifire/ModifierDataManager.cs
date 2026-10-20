
using static Defines;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
public class ModifierDataManager : AsyncDataManager<ModifierDataManager>
{
    // 옵션 그룹(ModifierGroupData) 단위 캐싱
    private Dictionary<string, ModifierGroupData> modifierOptionGroups = new();
    
    

    private Dictionary<string, List<string>> optionIds = new();

    // valueId -> ModifierValueData (all ModOption_ rows)
    private Dictionary<string, ModifierValueData> valueDatas = new();
    
    // Mod_ 파일을 읽어 저장하는 Base Modifier 인스턴스들
    private Dictionary<string, Modifier> modifierBases = new();
    
    ModifierFactory modFactory;

    public override async UniTask Init() { /* 초기화 */ }

    // myAssets는 어드레서블 등에서 Mod_ 파일과 ModOption_ 파일을 모두 포함해서 넘겨준다고 가정
    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        List<TextAsset> baseAssets = myAssets.Where(a => a.name.StartsWith("Mod_")).ToList();
        List<TextAsset> optionAssets = myAssets.Where(a => a.name.StartsWith("ModOption_")).ToList();

        // 1. Mod_ 파일 로드 (ModifierBases 채우기)
        foreach (var asset in baseAssets)
        {
            List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);
            ReadModifierBase(originData);
        }

        // 2. ModOption_ 파일 로드 (ModifierOptionData 및 ModifierData 채우기)
        foreach (var asset in optionAssets)
        {
            List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);
            await ReadAllModifierGroups(originData);
        }

        ValidateValueBases();
    }

    public void ReadModifierBase(List<Dictionary<string, object>> originData)
    {
        if (modFactory == null)
        {
            modFactory = ModifierFactory._instance;
        }
        for (int i = 0; i < originData.Count; i++)
        {
            var row = originData[i];
            if (!row.TryGetValue("ID", out object idObj))
            {
                Debug.LogError("ID 불러오기 실패");
                continue;
            }
            string id = idObj.ToString();
            if (modifierBases.ContainsKey(id))
            {
                Debug.LogError($"Id 중복: {id}");
                continue;
            }

            if (!row.TryGetValue("ModifierType", out object modTypeObj))
            {
                Debug.LogError($"모디파이어 타입 누락 (ID: {id})");
                continue;
            }

            ModifierType modType = Utils.ConvertToEnum<ModifierType>(modTypeObj.ToString());

            Modifier mod = modFactory.CreateNewModifierToType(modType);
            mod.SetBaseData(originData[i]);
            modifierBases.Add(id, mod);
        }
    }

    private async UniTask ReadAllModifierGroups(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            var row = originData[i];
            if (!row.TryGetValue("ID", out object idObj)) continue;
            if (!row.TryGetValue("OptionKey", out object optionKeyobj)) continue;
            
            string id = idObj.ToString();
            string optionKey = optionKeyobj.ToString();
            Debug.Log($"OptionKey: {optionKey}, ID: {id} 추가 시작");

            if(!modifierOptionGroups.ContainsKey(optionKey))
            {
                ModifierGroupData newGroup = new ModifierGroupData();
                modifierOptionGroups.Add(optionKey, newGroup);
                newGroup.ParseFromCSV(row);
            }

            ModifierGroupData optionGroup = modifierOptionGroups[optionKey];

            
            if (optionGroup.modDatas.Any(md => md.valueId == id))
            {
                Debug.LogError($"중복된 ModifierData ID: {id} 발견 (OptionKey: {optionKey})");
                continue;
            }

            ModifierValueData modValueData = new ModifierValueData();

            modValueData.ParseFromCsv(row);

            if(!optionGroup.valueKey.Contains(modValueData.optionKey))
            {
                Debug.LogError($"OptionKey 불일치: {optionGroup.valueKey} vs {modValueData.optionKey}");
            }

            optionGroup.AddModifierData(modValueData);

            if (valueDatas.ContainsKey(modValueData.valueId))
            {
                Debug.LogError($"Duplicate valueId in value index: {modValueData.valueId}");
            }
            else
            {
                valueDatas[modValueData.valueId] = modValueData;
            }

            // 3. OptionKey에 대한 ID 리스트 매핑 (호환성 유지용)
            if (!optionIds.ContainsKey(optionKey))
            {
                optionIds[optionKey] = new List<string>();
            }
            if (!optionIds[optionKey].Contains(id))
            {
                optionIds[optionKey].Add(id);
                Debug.Log($"OptionKey: {optionKey}, ID: {id} 추가 완료");
            }
            
            await Utils.WaitYield(i);
        }
        Debug.Log($"총 {modifierOptionGroups.Count}개의 모디파이어 옵션 그룹 로드 완료!");
    }


    // 옵션 그룹(ModifierOptionData) 조회
    public ModifierGroupData GetModifierGroupData(string optionKey)
    {
        return modifierOptionGroups.GetValueOrDefault(optionKey);
    }

    private static readonly List<string> emptyKeys = new List<string>();

    public List<string> Get_ModifierGroupKeys(string optionKey)
    {
        if (optionKey != null && optionIds.TryGetValue(optionKey, out List<string> keys))
        {
            return keys;
        }
        return emptyKeys;
    }

    // valueId -> ModifierValueData index lookup. Returns null when the id is unknown.
    public ModifierValueData Get_ModifierValueData(string valueId)
    {
        if (string.IsNullOrEmpty(valueId))
        {
            Debug.LogWarning("[ModifierDataManager] Get_ModifierValueData: empty valueId");
            return null;
        }
        if (valueDatas.TryGetValue(valueId, out ModifierValueData data))
        {
            return data;
        }
        Debug.LogWarning("[ModifierDataManager] Get_ModifierValueData: unknown valueId " + valueId);
        return null;
    }

    public bool HasModifierBase(string baseId)
    {
        return !string.IsNullOrEmpty(baseId) && modifierBases.ContainsKey(baseId);
    }

    // Default prewarm plan: base id -> object count. Bases that no value row uses get no entry (0).
    // Shared bases (many value rows) 16, Buff_* 4, everything else 8.
    public Dictionary<string, int> Get_PrewarmPlan()
    {
        Dictionary<string, int> usage = new Dictionary<string, int>();
        foreach (var pair in valueDatas)
        {
            string baseId = pair.Value.modifierId;
            if (string.IsNullOrEmpty(baseId)) continue;
            usage.TryGetValue(baseId, out int count);
            usage[baseId] = count + 1;
        }

        Dictionary<string, int> plan = new Dictionary<string, int>();
        foreach (var pair in modifierBases)
        {
            string baseId = pair.Key;
            if (!usage.ContainsKey(baseId)) continue;

            int size = 8;
            if (baseId.StartsWith("Buff_"))
            {
                size = 4;
            }
            else if (baseId == "Weapon_Damage" || baseId == "Weapon_AttackSpeed" || baseId == "Weapon_TwoHand"
                || baseId == "Armor_Def" || baseId.StartsWith("Job_") || baseId.StartsWith("Race_"))
            {
                size = 16;
            }
            plan[baseId] = size;
        }
        return plan;
    }

    // Called once after all Mod_/ModOption_ files are loaded. Reports value rows whose base is missing
    // and value ids that collide with base ids, each as a single aggregated warning.
    private void ValidateValueBases()
    {
        if (valueDatas.Count == 0) return;
        System.Text.StringBuilder missing = null;
        System.Text.StringBuilder overlap = null;
        int missingCount = 0;
        int overlapCount = 0;
        foreach (var pair in valueDatas)
        {
            ModifierValueData data = pair.Value;
            if (!modifierBases.ContainsKey(data.modifierId ?? string.Empty))
            {
                if (missing == null) missing = new System.Text.StringBuilder();
                missing.Append(data.valueId).Append('(').Append(data.modifierId).Append(") ");
                missingCount++;
            }
            if (modifierBases.ContainsKey(data.valueId))
            {
                if (overlap == null) overlap = new System.Text.StringBuilder();
                overlap.Append(data.valueId).Append(' ');
                overlapCount++;
            }
        }
        if (missing != null)
        {
            Debug.LogWarning("[ModifierDataManager] value rows without base modifier (skipped at assemble) " + missingCount + ": " + missing);
        }
        if (overlap != null)
        {
            Debug.LogWarning("[ModifierDataManager] valueId equals a baseId " + overlapCount + ": " + overlap);
        }
        Debug.Log("[ModifierDataManager] value index size " + valueDatas.Count);
    }

    // Mod_ 파일을 읽어서 베이스 모디파이어 팩토리에 인스턴스화하여 저장
    

    public Modifier Get_ModifierBase(string id)
    {
        if(modifierBases.TryGetValue(id, out Modifier mod))
        {
            return mod;
        }
        else
        {
            Debug.LogError($"ModifierDataManager: ID {id}에 해당하는 ModifierBase가 존재하지 않습니다.");
            return null;
        }
    }   
}
