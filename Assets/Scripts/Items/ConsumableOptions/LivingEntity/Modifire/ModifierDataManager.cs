using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public class ModifierOptionData
{
    public string optionKey;
    List<ModifierData> modDatas = new List<ModifierData>();

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
    //공용 정보
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
    public ItemTargetType itemTargetType;//구체적 타입 or 장비타입전체 전ex)경갑,중갑 후ex)갑옷,무기
    public bool unEquipable;
    // 아이템 및 버프, 액션 전용 변수들도 여기에 통합해서 선언합니다.
    // (어차피 런타임 인스턴스가 아니라 초기 로딩 시에만 파싱되는 순수 데이터이므로 메모리 부담이 없습니다)


    //public int specificType;



    // CSV 행(Row) 데이터를 통째로 받아 스스로의 빈칸을 채우는 파싱 함수
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
    // ★ 핵심: 1개의 ID 키가 여러 개의 '설계도 데이터(ModifierData)' 리스트를 가집니다.
    private Dictionary<string, ModifierOptionData> modifierDataGroup = new();
    private Dictionary<string, List<string>> optionIds = new();
    private Dictionary<string, Modifier> modifierBases = new();
    ModifierFactory modFactory;
    public override async UniTask Init() { /* 초기화 */ }

    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        foreach (var asset in myAssets)
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
            ModifierOptionData data;
            if (!modifierDataGroup.ContainsKey(id))
            {
                data = new ModifierOptionData();
            }
            data = modifierDataGroup[id];

            data.ParseFromCSV(row); // (이전 답변에서 만든 다용도 파싱 함수)

            if (!optionIds.ContainsKey(optionKey))
            {
                optionIds[optionKey] = new List<string>();
            }
            optionIds[optionKey].Add(id);
            await Utils.WaitYield(i);
        }
        Debug.Log($"총 {modifierDataGroup.Count}종류의 모디파이어 옵션 그룹 로딩 완료!");
    }

    // 외부에서 특정 옵션 ID에 묶인 모든 설계도 리스트를 요청할 때 사용
    public ModifierOptionData GetModifierData(string id)
    {
        return modifierDataGroup.GetValueOrDefault(id);
    }

    public List<string> Get_ModifierOptionKeys(string id)
    {
        return optionIds[id];
    }

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
                Debug.LogError("Id 중복");
                continue;
            }

            if (!row.TryGetValue("ModifierType", out object modTypeObj))
            {
                Debug.LogError("모드타입 X");
                continue;
            }

            ModifierType modType = (ModifierType)modTypeObj;
            Modifier mod = modFactory.CreateNewModifierToType(modType);
            mod.SetBaseData(originData[i]);
            modifierBases.Add(id, mod);
        }
    }
}