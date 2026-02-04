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
    string equipMentsDataName = "EquipMents";
    #region DataInit

    ItemDataManager()
    {
        
    }

    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        if(modifierFactory == null)
        {

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
            
        }
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
        ItemDataSort();
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
                    Debug.Log("티어 키 포함되지않음 ItemDataManager GetRandomID , 이큅");
                    return Utils.GetRandomIDToWeight(equipPair[1]);
                }
                return Utils.GetRandomIDToWeight(equipPair[tier]);
            case ItemCategory.Consumable:
                if (!consumPair.ContainsKey(tier))
                {
                    Debug.Log("티어 키 포함되지않음 ItemDataManager GetRandomID , 컨슈머블");
                    return Utils.GetRandomIDToWeight(consumPair[1]);
                }
                return Utils.GetRandomIDToWeight(consumPair[tier]);
            case ItemCategory.Misc:
                if (!consumPair.ContainsKey(tier))
                {
                    Debug.Log("티어 키 포함되지않음 ItemDataManager GetRandomID , 미스크");
                    return Utils.GetRandomIDToWeight(miscPair[1]);
                }
                return Utils.GetRandomIDToWeight(miscPair[tier]);
            default:
                Debug.Log("아이템 타입 잘못돼었음 ! ItemDataManager GetRandomID");
                return Utils.GetRandomIDToWeight(consumPair[tier]);
        }
    }
    void ItemDataSort()
    {
        foreach (var key in itemDatas.Keys)
        {
            if(itemDatas[key] is EquipItem equip)
            {
                AddToSorted(equip);
            }
            if(itemDatas[key] is ConsumableItem consum)
            {
                AddToSorted(consum);
            }
            if(itemDatas[key] is MiscItem misc)
            {
                AddToSorted(misc);
            }
        }
        
    }
    async UniTask Read_EquipDatas(List<Dictionary<string,object>> originData)
    {
        
        for (int i = 0; i < originData.Count; i++)
        {
            string ID = null;
            Utils.TrySetValue<string>(originData[i], "ID", ref ID);
            string Name = null;
            string modifierId = null;
            Utils.TrySetValue<string>(originData[i], "ModifierID", ref modifierId);
            Utils.TrySetValue<string>(originData[i], "Name", ref Name);
            Debug.Log($"Item Name {Name} ID : {ID}");
            int tier = 0;
            float weight = 0f;
            Utils.TrySetValue<int>(originData[i], "Tier", ref tier);
            Utils.TrySetValue(originData[i], "Weight", ref weight);
            if (!itemDatas.ContainsKey(Name))
            {
                Debug.Log($"{Name} , {ID}");
                EquipItem equip = new EquipItem();
                equip.id = Name;
                equip.tier = tier;
                equip.category = ItemCategory.Equipment;
                Utils.TrySetValue<string>(originData[i], "Name", ref equip.name);
                Utils.TryConvertEnum<EquipmentType>(originData[i], "ItemCategory", ref equip.equipmentType);
                Utils.TryConvertEnum<SlotType>(originData[i], "SlotType", ref equip.slot);
                itemDatas.Add(Name, equip);
                equip.weight = weight;
                equip.options = new List<Modifier>();
            }
            if(modifierFactory == null)
            {
                Debug.Log("ModifierFactory Null");
                continue;
            }
            if(modifierId == null)
            {
                Debug.Log("ModifierID Null");
                continue;
            }
            bool exist = itemDatas[Name].options.Any(d => d.id == ID);
            if (!exist)
            {
                Modifier modi = modifierFactory.CreateModifier(modifierId);

                /*
                StatModifier stat = new StatModifier();
                Utils.TrySetValue<string>(originData[i], "ID", ref stat.id);
                Utils.TryConvertEnum<ModifierTriggerType>(originData[i], "TriggerType", ref stat.triggerType);
                Utils.TrySetValue<bool>(originData[i], "IsMulti", ref stat.isMulti);
                Utils.TrySetValue<float>(originData[i], "Value", ref stat.value);
                Utils.TrySetValue<int>(originData[i], "Priority", ref stat.priority);
                Utils.TryConvertEnum<StatType>(originData[i], "StatType", ref stat.stat);
                itemDatas[Name].options.Add(stat);
                modifierPooler.GetModifier(stat.modifierType, stat.id);
                */
            }
            Debug.Log($"{Name} 등록 완료");
            await Utils.WaitYield(i);

            //Utils.TrySetValue<string>(temp[i], "ID", ref equip.id);


        }
    }


    async UniTask ReadConsumData(List<Dictionary<string,object>> originData)
    {
        for(int i = 0; i < originData.Count; i++)
        {

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
        string id = equip.id;
        int tier = equip.tier;
        if (!equipToTier.ContainsKey(tier))
        {
            equipToTier.Add(tier, new List<string>());
        }
        if (!equipPair.ContainsKey(tier))
        {
            equipPair.Add(tier, new List<WeightKeyPair>());
        }
        if (!equipToTier[tier].Contains(id))
        {
            equipToTier[tier].Add(id);
            WeightKeyPair pair = new WeightKeyPair() { key = id, cumulativeWeight = equip.weight};
            equipPair[tier].Add(pair);
        }
    }
    public void AddToSorted(ConsumableItem consum)
    {
        string id = consum.id;
        int tier = consum.tier;
        if (!ConsumToTiers.ContainsKey(tier))
        {
            ConsumToTiers.Add(tier, new List<string>());
        }
        if (!ConsumToTiers[tier].Contains(id))
        {
            ConsumToTiers[tier].Add(id);
            WeightKeyPair pair = new WeightKeyPair() { key = id, cumulativeWeight = consum.weight };
            consumPair[tier].Add(pair);
        }
    }
    public void AddToSorted(MiscItem misc)
    {

        string id = misc.id;
        int tier = misc.tier;
        if (!miscToTier.ContainsKey(tier))
        {
            miscToTier.Add(tier, new List<string>());
        }
        if (!miscToTier[tier].Contains(id))
        {
            miscToTier[tier].Add(id);
            WeightKeyPair pair = new WeightKeyPair() { key = id, cumulativeWeight = misc.weight };
            miscPair[tier].Add(pair);
        }

    }
    #endregion

}
