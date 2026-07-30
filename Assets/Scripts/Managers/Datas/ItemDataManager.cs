using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using static Defines;
public class ItemDataManager : AsyncDataManager<ItemDataManager>
{
    Dictionary<string, ItemBase> itemDatas = new Dictionary<string, ItemBase>();

    Dictionary<int, List<string>> equipToTier = new Dictionary<int, List<string>>();
    Dictionary<int,List<WeightKeyPair>> equipPair = new Dictionary<int,List<WeightKeyPair>>();
    Dictionary<int, List<string>> ConsumToTiers = new Dictionary<int, List<string>>();
    Dictionary<int, List<WeightKeyPair>> consumPair = new Dictionary<int, List<WeightKeyPair>>();
    Dictionary<int, List<string>> miscToTier = new Dictionary<int, List<string>>();
    Dictionary<int, List<WeightKeyPair>> miscPair = new Dictionary<int, List<WeightKeyPair>>();
    ModifierFactory modifierFactory;
    ModifierPooler modifierPooler;
    EffectDataManager effectDataManager;
    RandomOptionManager randomOptionManager;
    string equipMentsDataName = "EquipMents";
    #region DataInit

    ItemDataManager()
    {
        
    }

    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        if(effectDataManager == null)
        {
            effectDataManager = await EffectDataManager.CreateAsync();
        }
        if(modifierFactory == null)
        {

        }
        if(randomOptionManager == null)
        {
            randomOptionManager = await RandomOptionManager.CreateAsync();
        }
        foreach(var asset in myAssets)
        {
            List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);
            if (asset.name.Contains("Equip"))
            {
                await Read_EquipDatas(originData);
            }
            else if (asset.name.Contains("Consum"))
            {
                await ReadConsumData(originData);
            }
            else if (asset.name.Contains("Misc"))
            {
                await ReadMiscData(originData);
            }
            else if (asset.name.Contains("AddOption"))
            {
                await randomOptionManager.SetUp(asset);
            }
            
        }
        ItemDataSort();
    }
    public override async UniTask Init()
    {
        if (modifierPooler == null)
        {
            modifierPooler = GameManager.instance.Get_ModifierManager().GetModifierPooler();
        }
        if (modifierFactory == null)
        {
            modifierFactory = await ModifierFactory.CreateAsync();
        }
        if (modifierPooler == null)
        {
            modifierPooler = GameManager.instance.Get_ModifierManager().GetModifierPooler();
        }
        
    }
    public int GetRandomTier(Dictionary<int,float> rates)
    {
        return Utils.GetRandomTier(rates);
    }
    public string GetRandomID(ItemCategory type, int tier)
    {
        switch (type)
        {
            case ItemCategory.Equipment:
                if (!equipPair.ContainsKey(tier))
                {
                    return Utils.GetRandomIDToWeight(equipPair[1]);
                }
                return Utils.GetRandomIDToWeight(equipPair[tier]);
            case ItemCategory.Consumable:
                if (!consumPair.ContainsKey(tier))
                {
                    return Utils.GetRandomIDToWeight(consumPair[1]);
                }
                return Utils.GetRandomIDToWeight(consumPair[tier]);
            case ItemCategory.Misc:
                if (!consumPair.ContainsKey(tier))
                {
                    return Utils.GetRandomIDToWeight(miscPair[1]);
                }
                return Utils.GetRandomIDToWeight(miscPair[tier]);
            default:
                return Utils.GetRandomIDToWeight(consumPair[tier]);
        }
    }
    void ItemDataSort()
    {
        foreach (var key in itemDatas.Keys)
        {
            switch (itemDatas[key].category)
            {
                case ItemCategory.Equipment:
                    EquipItem equip = itemDatas[key] as EquipItem;
                    AddToSorted(equip);
                    break;
                case ItemCategory.Consumable:
                    ConsumableItem consum = itemDatas[key] as ConsumableItem;
                    AddToSorted(consum);
                    break;
                case ItemCategory.Misc:
                    MiscItem misc = itemDatas[key] as MiscItem;
                    AddToSorted(misc);
                    break;
                default:
                    break;
            }
        }
        KeypairSort();
    }
    async UniTask Read_EquipDatas(List<Dictionary<string,object>> originData)
    {
        /*
        장비 파싱 로직도 향후 이 패턴으로 optionKey 문자열만 추가하도록 수정하면 됩니다.
        */
    }

    async UniTask ReadConsumData(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {

            string id = null;
            string optionKey = null;
            string name = null;
            Utils.TrySetValue<string>(originData[i], "ID", ref id);
            Utils.TrySetValue<string>(originData[i], "Name", ref name);
            Utils.TrySetValue<string>(originData[i], "OptionKey", ref optionKey);

            if (string.IsNullOrEmpty(name))
            {
                await Utils.WaitYield(i);
                continue;
            }

            if (!itemDatas.ContainsKey(name))
            {
                ConsumableItem consum = new ConsumableItem();
                consum.id = name;             
                consum.name = name;           
                consum.category = ItemCategory.Consumable;

                // 데이터 파싱
                Utils.TryConvertEnum<ConsumableType>(originData[i], "ItemType", ref consum.consumableType);
                Utils.TrySetValue<int>(originData[i], "Tier", ref consum.tier);
                Utils.TrySetValue<float>(originData[i], "Weight", ref consum.weight);

                // 리팩토링 핵심: 풀링 객체가 아닌 단순 문자열 식별자만 보관
                consum.optionKeys = new List<string>();

                itemDatas.Add(name, consum);
            }

            // ModifierType은 팩토리/데이터 로드에서 이미 optionKey로 묶어 처리하므로
            // 여기서는 단순히 해당 템플릿의 optionKeys에 문자열만 삽입해줍니다.
            if (!string.IsNullOrEmpty(optionKey))
            {
                if (!itemDatas[name].optionKeys.Contains(optionKey))
                {
                    itemDatas[name].optionKeys.Add(optionKey);
                }
            }

            await Utils.WaitYield(i);
        }
    }
    async UniTask ReadMiscData(List<Dictionary<string,object>> originData)
    {
        for(int i = 0; i < originData.Count; i++)
        {

            await Utils.WaitYield(i);
        }
    }


    #endregion
    public Dictionary<string, ItemBase> Get_ItemDatas()
    {
        return itemDatas;
    }

    public Dictionary<int,List<string>> Get_EquipToTire()
    {
        return equipToTier;
    }


    #region Sort
    public void AddToSorted(EquipItem equip)
    {
        string name = equip.name;
        int tier = equip.tier;
        if (!equipToTier.ContainsKey(tier))
        {
            equipToTier.Add(tier, new List<string>());
        }
        if (!equipPair.ContainsKey(tier))
        {
            equipPair.Add(tier, new List<WeightKeyPair>());
        }
        if (!equipToTier[tier].Contains(name))
        {
            equipToTier[tier].Add(name);
        }
    }
    public void AddToSorted(ConsumableItem consum)
    {
        string name = consum.name;
        int tier = consum.tier;
        if (!ConsumToTiers.ContainsKey(tier))
        {
            ConsumToTiers.Add(tier, new List<string>());
        }
        if (!ConsumToTiers[tier].Contains(name))
        {
            ConsumToTiers[tier].Add(name);
        }
    }
    public void AddToSorted(MiscItem misc)
    {

        string name = misc.name;
        int tier = misc.tier;
        if (!miscToTier.ContainsKey(tier))
        {
            miscToTier.Add(tier, new List<string>());
        }
        if (!miscToTier[tier].Contains(name))
        {
            miscToTier[tier].Add(name);
        }

    }
    #endregion
    public void KeypairSort()
    {
        foreach(var tier in equipToTier.Keys)
        {
            float weightsum = 0;
            foreach(var key in equipToTier[tier])
            {
                ItemBase item = itemDatas[key];
                if (!equipPair.ContainsKey(tier))
                {
                    equipPair.Add(tier, new List<WeightKeyPair>());
                }
                WeightKeyPair keypair = new WeightKeyPair();
                keypair.key = key;
                weightsum+= item.weight;
                keypair.cumulativeWeight = weightsum;
                equipPair[tier].Add(keypair);
            }
        }

        foreach (var tier in ConsumToTiers.Keys)
        {
            float weightsum = 0;
            foreach (var key in ConsumToTiers[tier])
            {
                ItemBase item = itemDatas[key];
                if (!consumPair.ContainsKey(tier))
                {
                    consumPair.Add(tier, new List<WeightKeyPair>());
                }
                WeightKeyPair keypair = new WeightKeyPair();
                keypair.key = key;
                weightsum+= item.weight;
                keypair.cumulativeWeight = weightsum;
                consumPair[tier].Add(keypair);
            }
        }

        foreach (var tier in miscToTier.Keys)
        {
            float weightsum = 0;
            foreach (var key in miscToTier[tier])
            {
                ItemBase item = itemDatas[key];
                if (!miscPair.ContainsKey(tier))
                {
                    miscPair.Add(tier, new List<WeightKeyPair>());
                }
                WeightKeyPair keypair = new WeightKeyPair();
                keypair.key = key;
                weightsum+= item.weight;
                keypair.cumulativeWeight = weightsum;
                miscPair[tier].Add(keypair);

            }
        }
    }
}
