using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static Defines;

public class AddOptionData
{
    public string optionKey;
    public string optionCategory;
    public List<AddOptionModifier> addOptions = new List<AddOptionModifier>();
    public int tier;
    public int weight;
    public EquipCategory targetType;
    public ItemSubType targetSubType;
}
public class AddOptionModifier
{
    public string id;
    public string optionName;
    
    public bool isMulti;
    public int priority;
    public float value;
    /*
    public int tier;
    public int weight;
    public EquipCategory targetType;
    public ItemSubType targetSubType;*/
}
public class RandomOptionManager : AsyncDataManager<RandomOptionManager>
{

    //Dictionary<string, AddOptionData> optionDatas = new Dictionary<string, AddOptionData>();
    Dictionary<string, AddOptionData> addOptionDatas = new Dictionary<string, AddOptionData>();

    public async override UniTask Init()
    {
        
    }

    public async UniTask SetUp(TextAsset asset)
    {
        List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);


        for(int i = 0; i<originData.Count; i++)
        {
            Dictionary<string, object> data = originData[i];
            AddOptionModifier newMod = new AddOptionModifier();
            string optionName = data["OptionName"].ToString();
            if (!addOptionDatas.ContainsKey(optionName))
            {
                AddOptionData newOption = new AddOptionData();
                newOption.optionKey = optionName;
                addOptionDatas.Add(optionName, newOption);
            }
            AddOptionData optionData = addOptionDatas[optionName];
            int weight = 0;
            Utils.TrySetValue(data, "ID", ref newMod.id);
            Debug.Log($"무작위 옵션 추가 {newMod.id}");
            Utils.TrySetValue(data, "OptionName", ref newMod.optionName);
            Utils.TrySetValue(data, "IsMulti", ref newMod.isMulti);
            Utils.TrySetValue(data, "Priority", ref newMod.priority);
            Utils.TrySetValue(data, "Value", ref newMod.value);
            Utils.TrySetValue(data, "Tier", ref optionData.tier);
            Utils.TrySetValue(data, "Weight", ref weight);
            Utils.TryConvertEnum(data, "TargetType", ref optionData.targetType);
            Utils.TryConvertEnum(data, "TargetSubType", ref optionData.targetSubType);

            optionData.weight += weight;
            addOptionDatas[optionName].addOptions.Add(newMod);

        }
        await UniTask.Yield();
    }

    public AddOptionData GetOptionByID(string id)
    {
        if (addOptionDatas.TryGetValue(id, out AddOptionData data))
        {
            return data;
        }
        Debug.LogWarning($"옵션 ID를 찾을 수 없습니다: {id}");
        return null;
    }

    public int Get_OptionCount(ItemRarity rarity)
    {
        int optionCount = 0;
        switch (rarity)
        {
            case ItemRarity.Normal:
                optionCount = 0;
                break;
            case ItemRarity.Magic:
                optionCount = 1;
                break;
            case ItemRarity.Rare:
                optionCount = UnityEngine.Random.Range(2, 6);
                break;
            case ItemRarity.Unique:
                break;
            default:
                break;
        }
        Debug.Log($"옵션 갯수 {optionCount}");
        return optionCount;

    }
    /// <summary>
    /// 조건에 맞는 랜덤 옵션을 뽑습니다. (기존 로직 유지)
    /// </summary>
    public AddOptionData GetRandomOption(EquipCategory itemType, ItemSubType itemSubType, int maxTier, HashSet<string> excludeOptioCategory)
    {
        Debug.Log($"[옵션뽑기 요청] 카테고리: {itemType}, 서브타입: {itemSubType}, 티어: {maxTier}");
        Debug.Log($"[옵션 DB 상태] 현재 로드된 전체 옵션 개수: {addOptionDatas.Count}개");
        // ?? optionDict.Values 를 통해 딕셔너리의 데이터들만 순회하며 필터링합니다.
        var validPool = addOptionDatas.Values.Where(opt =>
            (opt.targetType == EquipCategory.None || opt.targetType == itemType) &&
            (opt.targetSubType == ItemSubType.None || opt.targetSubType == itemSubType) &&
            opt.tier <= maxTier &&
            (!excludeOptioCategory.Contains(opt.optionCategory))
        ).ToList();

        if (validPool.Count == 0) return null;

        int totalWeight = validPool.Sum(opt => opt.weight);
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var option in validPool)
        {
            cumulativeWeight += option.weight;
            if (randomValue < cumulativeWeight)
            {
                Debug.Log($"선택 된 아이템 옵션 : {option.optionKey}");
                return option;
            }
        }

        return validPool.Last();
    }
}
