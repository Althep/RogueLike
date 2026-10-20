using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;

public class ModifierValueData
{
    public string valueId;
    public string modifierId;
    public string optionKey;
    public bool isMulti;
    public float value;
    public int priority;
    public string descKey;
    public int keyOder;
    public void ParseFromCsv(Dictionary<string, object> row)
    {
        Utils.TrySetValue(row, "ID", ref valueId);
        Utils.TrySetValue(row, "ModifierID", ref modifierId);
        Utils.TrySetValue(row, "OptionKey", ref optionKey);
        Utils.TrySetValue(row, "IsMulti", ref isMulti);
        Utils.TrySetValue(row, "Value", ref value);
        Utils.TrySetValue(row, "Priority", ref priority);
        Utils.TrySetValue(row, "DescKey", ref descKey);
        Utils.TrySetValue(row, "KeyOder", ref keyOder);
    }
}
public class ModifierGroupData
{
    public string valueKey;
    public List<ModifierValueData> modDatas = new List<ModifierValueData>();
    public string descKey;
    public void ParseFromCSV(Dictionary<string, object> row)
    {
        Utils.TrySetValue(row, "OptionKey", ref valueKey);
        Utils.TrySetValue(row, "DescKey", ref descKey);
    }

    public void AddModifierData(ModifierValueData modValueData)
    {
        if (!modDatas.Contains(modValueData))
        {
            modDatas.Add(modValueData);
            Debug.Log($"ModifierOptionData: OptionKey {valueKey}에 ModifierData ID {modValueData.valueId} 추가 완료");
        }
    }
}
#region 주석처리
/*
public class ModifierBaseData
{
    public string id;
    public string stringValue;
    public bool isMulti;
    public int priority;
    public ModifierType modifierType;
    public StatType stat;
    public ModifierTriggerType triggerType;
    
    public ItemTargetType itemTargetType;//전체에 타겟 or 특정카테고리에 만ex)방어구,무기 단ex)투구,갑옷
    public ItemCategory itemCategory;
    public Enum itemSpecificType;

    public void ParseFromCSV(Dictionary<string, object> row)
    {
        Utils.TrySetValue(row, "ID", ref id);
        Utils.TrySetValue(row, "StringValue", ref stringValue);
        Utils.TrySetValue(row, "IsMulti", ref isMulti);
        Utils.TrySetValue(row, "Priority", ref priority);
        Utils.TryConvertEnum(row, "ModifierType", ref modifierType);
        Utils.TryConvertEnum(row, "Stat", ref stat);
        Utils.TryConvertEnum(row, "TriggerType", ref triggerType);
        switch (itemTargetType)
        {
            case ItemTargetType.Category:
                break;
            case ItemTargetType.Specific:
                itemSpecificType = Utils.Get_ItemSpecificType(row["ItemSpecificType"].ToString());
                break;
            default:
                break;
        }
        Utils.TryConvertEnum(row, "ItemCategory", ref itemTargetType);
    }
}
*/
#endregion
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
    public string stringValue, effectName;
    //버프 모디파이어 정보

    public int minDuration, maxDuration;
    public BuffCategory buffCategory; //버프,디버프
    public BuffType buffType;//액션 추가형, 스탯추가형
    //아이템 모디파이어 정보
    public ItemCategory itemCategory;
    public ItemTargetType itemTargetType;//전체에 타겟 or 특정카테고리에 만ex)방어구,무기 단ex)투구,갑옷

    public Enum itemSpecificType; // 특정 아이템 타입 (ItemTargetType이 Specific일 때만 의미 있음)
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
        Utils.TryConvertEnum(row, "ItemCategory", ref itemCategory);
        Utils.TryConvertEnum(row, "ItemTargetType", ref itemTargetType);
        if (itemTargetType == ItemTargetType.Specific)
        {
            itemSpecificType = Utils.Get_ItemSpecificType(row["ItemSpecificType"].ToString());
        }
        else
        {
            if (Utils.TryConvertEnum(row, "ItemSpecificType", ref itemCategory))
            {
                Debug.LogWarning($"ItemTargetType이 Specific이 아닌데 ItemSpecificType이 설정되어 있습니다. (ID: {id})");
            }
        }
        Utils.TrySetValue(row, "UnEquipable", ref unEquipable);
        Debug.Log($"ModifierData ParseFromCsv 완료: ID={id}, ModifierID={modifierId}, OptionKey={optionKey}, IsMulti={isMulti}, Value={value}, Priority={priority}, DescKey={descKey}, KeyOder={keyOder}");
    }

    public void SetValueForOption(Dictionary<string, object> originData)
    {
        Utils.TrySetValue(originData, "ID", ref id);
        Utils.TrySetValue(originData, "ModifierID", ref modifierId);
        Utils.TrySetValue(originData, "OptionKey", ref optionKey);
        Utils.TrySetValue(originData, "IsMulti", ref isMulti);
        Utils.TrySetValue(originData, "Value", ref value);
        Utils.TrySetValue(originData, "Priority", ref priority);
        Utils.TrySetValue(originData, "KeyOder", ref keyOder);

    }
}
