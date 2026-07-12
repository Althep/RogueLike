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
        Debug.Log("아이템 데이터 정렬 시작!");
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
        
        for (int i = 0; i < originData.Count; i++)
        {
            string ID = null;
            Utils.TrySetValue<string>(originData[i], "ID", ref ID);
            string Name = null;
            string modifierId = null;
            string modifierOptionKey = null;
            Utils.TrySetValue<string>(originData[i], "ModifierID", ref modifierId);
            Utils.TrySetValue<string>(originData[i],"OptionKey",ref modifierOptionKey);
            Utils.TrySetValue<string>(originData[i], "Name", ref Name);
            Debug.Log($"Item Name {Name} ID : {ID}");
            int tier = 0;
            float weight = 0f;
            string tooltipKey = null;
            Utils.TrySetValue<int>(originData[i], "Tier", ref tier);
            Utils.TrySetValue(originData[i], "Weight", ref weight);
            Utils.TrySetValue(originData[i], "TooltipKey", ref tooltipKey);
            if (!itemDatas.ContainsKey(Name))
            {
                Debug.Log($"{Name} , {ID}");
                EquipItem equip = new EquipItem();
                equip.id = Name;
                equip.tier = tier;
                equip.category = ItemCategory.Equipment;
                equip.tooltipKey = tooltipKey;
                Utils.TrySetValue<string>(originData[i], "Name", ref equip.name);
                Utils.TryConvertEnum<EquipmentType>(originData[i], "ItemCategory", ref equip.equipmentType);
                Utils.TryConvertEnum<SlotType>(originData[i], "SlotType", ref equip.slot);
                Utils.TryConvertEnum(originData[i], "EquipmentCategory", ref equip.equipmentType);
                Utils.TryConvertEnum(originData[i], "ItemSubType", ref equip.itemSubType);
                itemDatas.Add(Name, equip);
                equip.weight = weight;
                equip.options = new List<ModifierOption>();
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
            bool existOption = itemDatas[Name].options.Any(k => k.optionKey == modifierOptionKey);
            ModifierOption modOption;
            if (!existOption)
            {
                modOption = new ModifierOption();
                itemDatas[Name].options.Add(modOption);
            }
            modOption = itemDatas[Name].options.FirstOrDefault(m => m.optionKey == modifierOptionKey);
            bool existModifier = modOption.myMods.Any(m => m.id == ID);
            if (!existModifier)
            {
                Modifier mod = modifierFactory.CreateNewInstance(modifierId);
                Utils.TrySetValue(originData[i], "Value", ref mod.value);
                Debug.Log($"{Name} 모디파이어 ID : {modifierId} 등록 값 : {mod.value} 타입 {mod.modifierType} 스탯 타입{mod.stat} 모디파이어 ID {mod.id}");
                modOption.myMods.Add(mod);
            }

            Debug.Log($"{Name} 등록 완료");
            await Utils.WaitYield(i);

            //Utils.TrySetValue<string>(temp[i], "ID", ref equip.id);


        }
    }

    /*
    async UniTask ReadConsumData(List<Dictionary<string,object>> originData)
    {
        for(int i = 0; i<originData.Count; i++)
        {
            string id = originData[i]["ID"].ToString();
            string name = originData[i]["Name"].ToString();
            if (!itemDatas.ContainsKey(name))
            {
                ConsumableItem consum = new();
                consum.id = name;
                //consum.name = name;
                Utils.TryConvertEnum<ConsumableType>(originData[i], "ItemType", ref consum.consumableType);
                Utils.TrySetValue(originData[i], "Tier", ref consum.tier);
                Utils.TrySetValue(originData[i], "Weight", ref consum.weight);
                consum.category = ItemCategory.Consumable;
                itemDatas.Add(name, consum);
            }
            ModifierType type = Utils.ConvertToEnum<ModifierType>(originData[i]["ModifierType"].ToString());
            Modifier mod = modifierPooler.GetModifier(type,id);
            itemDatas[name].options.Add(mod);

            await Utils.WaitYield(i);
        }
        
    }
    */
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

                // 추가 데이터 파싱
                Utils.TryConvertEnum<ConsumableType>(originData[i], "ItemType", ref consum.consumableType);
                Utils.TrySetValue<int>(originData[i], "Tier", ref consum.tier);
                Utils.TrySetValue<float>(originData[i], "Weight", ref consum.weight);

                consum.options = new List<ModifierOption>();

                itemDatas.Add(name, consum);
            }

            // 2. Modifier 데이터 추가 부분
            string modifierTypeStr = null;
            if (Utils.TrySetValue<string>(originData[i], "ModifierType", ref modifierTypeStr))
            {
                ModifierType type = Utils.ConvertToEnum<ModifierType>(modifierTypeStr);

                if (modifierPooler != null)
                {
                    ModifierOption option = modifierPooler.GetModifierOption(optionKey);
                    
                    if (option != null)
                    {
                        itemDatas[name].options.Add(option);
                    }
                }
                else
                {
                    Debug.LogWarning($"ModifierPooler가 Null입니다! 아이템: {name}");
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
            Debug.Log($"티어 정렬 {tier}, 이름 {name} 등록 완료");
            //WeightKeyPair pair = new WeightKeyPair() { key = id, cumulativeWeight = equip.weight};
            //equipPair[tier].Add(pair);
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
            Debug.Log($"티어 정렬 {tier}, 이름 {name} 등록 완료");
            //WeightKeyPair pair = new WeightKeyPair() { key = id, cumulativeWeight = consum.weight };
            //consumPair[tier].Add(pair);
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
            //WeightKeyPair pair = new WeightKeyPair() { key = id, cumulativeWeight = misc.weight };
            //miscPair[tier].Add(pair);
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
                Debug.Log($"장비아이템  {tier} 키페어 키 {keypair.key} 등록");
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
                Debug.Log($"소비아이템 {tier }키페어 키 : {keypair.key} 등록");
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
