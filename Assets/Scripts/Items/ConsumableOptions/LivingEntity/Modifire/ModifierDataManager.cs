using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;

public class ModifierOptionData
{
    public string optionKey;
    public List<ModifierData> modDatas = new List<ModifierData>();

    public void ParseFromCSV(Dictionary<string,object> row)
    {
        Utils.TrySetValue(row, "OptionKey", ref optionKey);
    }
}

public class ModifierData
{
    //베이스 정보
    public ModifierType modifierType;
    public StatType stat;
    public ModifierTriggerType triggerType;
    //옵션 정보
    public string id;
    public string modifierId;
    public string optionKey;
    public bool isMulti;
    public float value;
    public int priority;
    public string descKey;
    public int keyOder;

    //액션 모디파이어 정보
    public string stringValue,effectName;
    //버프 모디파이어 정보

    public int minDuration, maxDuration;
    public BuffCategory buffCategory; //버프,디버프
    public BuffType buffType;//액션 추가형, 스탯추가형
    //아이템 모디파이어 정보
    public ItemCategory itemCategory;
    public ItemTargetType itemTargetType;//전체에 타겟 or 특정카테고리에 만ex)방어구,무기 단ex)투구,갑옷
    public bool unEquipable;
    // 아이템을 낄 때나, 액션 등을 발동할때 로직에서 확인해서 사용합니다.
    // (아이템 모듈등 자신이 아닌 초기 로드 시에 파싱되는 기본 데이터이므로 메모리 누수를 방지합니다)


    //public int specificType;



    // CSV 행(Row) 데이터를 통째로 받아 자신에게 맞는 빈칸을 채우는 파싱 함수
    public void ParseFromCsv(Dictionary<string, object> row)
    {
        Utils.TrySetValue(row, "ID", ref id);
        Utils.TrySetValue(row, "ModifierID", ref modifierId);
        Utils.TrySetValue(row, "OptionKey", ref optionKey);
        Utils.TrySetValue(row, "IsMulti", ref isMulti);
        Utils.TrySetValue(row, "Value", ref value);
        Utils.TrySetValue(row, "Priority", ref priority);
        Utils.TrySetValue(row, "DescKey", ref descKey);
        Utils.TrySetValue(row, "KeyOder", ref keyOder);
        //액션
        Utils.TrySetValue(row, "StringValue", ref stringValue);
        Utils.TrySetValue(row, "effectName", ref effectName);
        //버프
        Utils.TrySetValue(row, "MinDuration", ref minDuration);
        Utils.TrySetValue(row, "MaxDuration", ref maxDuration);
        Utils.TryConvertEnum(row, "BuffCategory", ref buffCategory);
        Utils.TryConvertEnum(row, "BuffType", ref buffType);
        //아이템
        Utils.TryConvertEnum(row, "ItemCategory",ref itemCategory);
        Utils.TryConvertEnum(row, "ItemTargetType",ref itemTargetType);
        Utils.TrySetValue(row, "UnEquipable", ref unEquipable);
    }
}

public class ModifierDataManager : AsyncDataManager<ModifierDataManager>
{
    // 옵션 그룹(ModifierOptionData) 단위 캐싱
    private Dictionary<string, ModifierOptionData> modifierOptionGroups = new();
    
    // 개별 ModifierData 캐싱 (ID 기준) - ModifierPooler 등에서 개별 접근 시 사용
    private Dictionary<string, ModifierData> modifierDatas = new();
    
    private Dictionary<string, List<string>> optionIds = new();
    
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
            
            // 1. 개별 ModifierData 파싱 및 저장
            ModifierData modData = new ModifierData();
            modData.ParseFromCsv(row);
            
            // 베이스 모디파이어(Mod_ 파일에서 로드됨)의 정보를 병합
            if (!string.IsNullOrEmpty(modData.modifierId) && modifierBases.ContainsKey(modData.modifierId))
            {
                Modifier baseMod = modifierBases[modData.modifierId];
                modData.modifierType = baseMod.modifierType;
                modData.stat = baseMod.stat;
                modData.triggerType = baseMod.triggerType;
            }

            if (!modifierDatas.ContainsKey(id))
            {
                modifierDatas.Add(id, modData);
            }

            // 2. OptionKey 기준으로 ModifierOptionData 그룹핑
            if (!modifierOptionGroups.ContainsKey(optionKey))
            {
                ModifierOptionData newOptionGroup = new ModifierOptionData();
                newOptionGroup.ParseFromCSV(row); // OptionKey 세팅
                modifierOptionGroups.Add(optionKey, newOptionGroup);
            }
            modifierOptionGroups[optionKey].modDatas.Add(modData);

            // 3. OptionKey에 대한 ID 리스트 매핑 (호환성 유지용)
            if (!optionIds.ContainsKey(optionKey))
            {
                optionIds[optionKey] = new List<string>();
            }
            if (!optionIds[optionKey].Contains(id))
            {
                optionIds[optionKey].Add(id);
            }
            
            await Utils.WaitYield(i);
        }
        Debug.Log($"총 {modifierOptionGroups.Count}개의 모디파이어 옵션 그룹 로드 완료!");
    }

    // 개별 ModifierData 조회 (ModifierFactory나 ModifierPooler에서 호출)
    public ModifierData GetModifierData(string id)
    {
        return modifierDatas.GetValueOrDefault(id);
    }

    // 옵션 그룹(ModifierOptionData) 조회
    public ModifierOptionData GetModifierOptionGroup(string optionKey)
    {
        return modifierOptionGroups.GetValueOrDefault(optionKey);
    }

    public List<string> Get_ModifierOptionKeys(string optionKey)
    {
        return optionIds.GetValueOrDefault(optionKey, new List<string>());
    }

    // Mod_ 파일을 읽어서 베이스 모디파이어 팩토리에 인스턴스화하여 저장
    public void ReadModifierBase(List<Dictionary<string,object>> originData)
    {
        if(modFactory == null)
        {
            modFactory = ModifierFactory._instance;
        }
        for(int i = 0; i< originData.Count; i++)
        {
            var row = originData[i];
            if(!row.TryGetValue("ID", out object idObj))
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

            ModifierType modType = (ModifierType)modTypeObj;
            Modifier mod = modFactory.CreateNewModifierToType(modType);
            mod.SetBaseData(originData[i]);
            modifierBases.Add(id, mod);
        }
    }
}
