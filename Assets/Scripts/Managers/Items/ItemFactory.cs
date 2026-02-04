using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class ItemFactory 
{
    ItemDataManager itemDataManager;
    ModifierPooler pooler;
    EffectManager effcetManager;
    PoolManager poolManager;
    DungeonManager dm;
    Dictionary<int, float> equipRates;
    Dictionary<int, float> consumRates;
    Dictionary<int, float> miscRates;
    public int itemCounts;
    
    int floor;
    public ItemFactory()
    {
        Init();
    }

    public void CalcuateItemRates()
    {
        floor = dm.floor;

        equipRates = TierCalculator.GetTierProbabilities(floor,3,5, false);
        consumRates = TierCalculator.GetTierProbabilities(floor, 3, 5, true);
        miscRates = TierCalculator.GetTierProbabilities(floor, 3, 5, true);
    }

    public ItemBase GetRandomItem()
    {
        ItemCategory type = Utils.Get_RandomType<ItemCategory>();
        int tier = GetRandomTier(type);
        string key = itemDataManager.GetRandomID(type, tier);
        return GetItemScript(key);
    }
    
    public int GetRandomTier(ItemCategory type)
    {
        switch (type)
        {
            case ItemCategory.Equipment:
                Debug.Log("selected Equip");
                return Utils.GetRandomTier(equipRates);
            case ItemCategory.Consumable:
                Debug.Log("Selected Consum");
                return Utils.GetRandomTier(consumRates);
            case ItemCategory.Misc:
                Debug.Log("Selected Misc");
                return Utils.GetRandomTier(miscRates);
            default:
                Debug.Log("입력 타입이 적절하지않음!");
                return Utils.GetRandomTier(consumRates);
                
        }
        
    }
    public ItemCategory GetRandomCategory()
    {
        ItemCategory type = Utils.Get_RandomType<ItemCategory>();
        Debug.Log($"랜덤 카테고리 선택 {type}");
        return type;

    }
    public void OnFloorChange()
    {
        floor = dm.floor;
        equipRates = TierCalculator.GetTierProbabilities(floor, 3, 5, false);
        consumRates = TierCalculator.GetTierProbabilities(floor, 3, 5, true);
    }

    public void Init()
    {
        if (itemDataManager == null)
        {
            itemDataManager = GameManager.instance.Get_DataManager().itemDataManager;
        }
        if(pooler == null)
        {
            pooler = GameManager.instance.Get_ModifierManager().GetModifierPooler();
        }
        if(effcetManager == null)
        {
            effcetManager = GameManager.instance.Get_EffectManager();
        }
        if(poolManager == null)
        {
            poolManager = GameManager.instance.Get_PoolManager();
        }
        if(dm == null)
        {
            dm = GameManager.instance.GetDungeonManager();
        }

    }

    public ItemBase GetItemScript(string name)
    {
        ItemBase origin = itemDataManager.Get_ItemDatas()[name];
        ItemBase item;
        switch (origin.category)
        {

            case Defines.ItemCategory.Equipment:
                item = new EquipItem();
                if(origin is EquipItem)
                {
                    item.id = origin.id;
                    item.name = origin.name;
                    item.maxStack = 1;
                    item.category = ItemCategory.Equipment;
                }
                break;
            case Defines.ItemCategory.Consumable:
                item = new ConsumableItem();
                if(origin is ConsumableItem)
                {
                    item.id = origin.id;
                    item.name = origin.name;
                    item.maxStack = 100;
                    item.category = ItemCategory.Consumable;
                }
                break;
            case Defines.ItemCategory.Misc:
                item = new MiscItem();
                if(origin is MiscItem)
                {
                    item.id = origin.id;
                    item.name = origin.name;
                    item.maxStack = 100;
                    item.category = ItemCategory.Misc;
                }
                break;
            default:
                Debug.Log("ItemCategoryError");
                return null;
        }
        foreach (Modifier modi in origin.options)
        {
            ModifierType modType = modi.modifierType;

            // Pool에서 해당 타입 객체 가져오기
            PoolScriptType poolType = (PoolScriptType)Enum.Parse(typeof(PoolScriptType), modType.ToString());
            IPoolScript pooled = pooler.GetModifier(modType,origin.id);

            if (pooled is Modifier newOne)
            {
                
                modi.Copy(newOne); // origin 데이터를 풀에서 꺼낸 객체에 덮어쓰기
                item.options.Add(newOne);
            }
            else
            {
                Debug.LogError($"Pool returned wrong type for {modType}");
            }
        }
        
        return item;
    }
    public void MakeFieldItem()
    {
        GameObject go = poolManager.ObjectPool(TileType.Item);
        
    }
}
