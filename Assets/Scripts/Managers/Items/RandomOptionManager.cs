using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static Defines;

public class AddOptionData
{
    public string id;
    public string optionName;
    public bool isMulti;
    public int priority;
    public float value;
    public int tier;
    public int weight;
    public EquipCategory targetType;
    public ItemSubType targetSubType;
}
public class RandomOptionManager : AsyncDataManager<RandomOptionManager>
{

    Dictionary<string, AddOptionData> optionDatas = new Dictionary<string, AddOptionData>();


    public async override UniTask Init()
    {
        
    }

    public async UniTask SetUp(TextAsset asset)
    {
        List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);


        for(int i = 0; i<originData.Count; i++)
        {
            Dictionary<string, object> data = originData[i];

            AddOptionData newOption = new AddOptionData();
            
            Utils.TrySetValue(data, "ID", ref newOption.id);
            Debug.Log($"무작위 옵션 추가 {newOption.id}");
            Utils.TrySetValue(data, "OptionName", ref newOption.optionName);
            Utils.TrySetValue(data, "IsMulti", ref newOption.isMulti);
            Utils.TrySetValue(data, "Priority", ref newOption.priority);
            Utils.TrySetValue(data, "Value", ref newOption.value);
            Utils.TrySetValue(data, "Tier", ref newOption.tier);
            Utils.TrySetValue(data, "Weight", ref newOption.weight);
            Utils.TryConvertEnum(data, "TargetType", ref newOption.targetType);
            Utils.TryConvertEnum(data, "TargetSubType", ref newOption.targetSubType);
            optionDatas[newOption.id] = newOption;
        }
        await UniTask.Yield();
    }

    public AddOptionData GetOptionByID(string id)
    {
        if (optionDatas.TryGetValue(id, out AddOptionData data))
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
    public AddOptionData GetRandomOption(EquipCategory itemType, ItemSubType itemSubType, int maxTier, HashSet<string> excludeOptionNames)
    {
        Debug.Log($"[옵션뽑기 요청] 카테고리: {itemType}, 서브타입: {itemSubType}, 티어: {maxTier}");
        Debug.Log($"[옵션 DB 상태] 현재 로드된 전체 옵션 개수: {optionDatas.Count}개");
        // ?? optionDict.Values 를 통해 딕셔너리의 데이터들만 순회하며 필터링합니다.
        var validPool = optionDatas.Values.Where(opt =>
            (opt.targetType == EquipCategory.None || opt.targetType == itemType) &&
            (opt.targetSubType == ItemSubType.None || opt.targetSubType == itemSubType) &&
            opt.tier <= maxTier &&
            (!excludeOptionNames.Contains(opt.optionName) || opt.isMulti)
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
                Debug.Log($"선택 된 아이템 옵션 : {option.id}");
                return option;
            }
        }

        return validPool.Last();
    }
}
