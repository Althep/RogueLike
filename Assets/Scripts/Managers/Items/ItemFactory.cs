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
        floor = DungeonManager.instance.Get_Floor();

        equipRates = TierCalculator.GetTierProbabilities(floor,3,5, false);
        consumRates = TierCalculator.GetCoreTierWeights(floor,25,5);
        miscRates = TierCalculator.GetTierProbabilities(floor, 3, 5, true);
    }

    public ItemBase GetRandomItem()
    {
        ItemCategory type = Utils.Get_RandomType<ItemCategory>();
        if(type == ItemCategory.Misc)
        {
            Debug.Log("Misc 선택됨");
            int number = UnityEngine.Random.Range(0, 2);
            if(number == 0)
            {
                type = ItemCategory.Consumable;
            }
            else
            {
                type = ItemCategory.Equipment;
            }
        }
        int tier = GetRandomTier(type);
        string key = itemDataManager.GetRandomID(type, tier);
        Debug.Log($"Random Item ID {key}");
        return GetItemScript(key);
    }
    
    public int GetRandomTier(ItemCategory type)
    {
        switch (type)
        {
            case ItemCategory.Equipment:
                return Utils.GetRandomTier(equipRates);
            case ItemCategory.Consumable:
                return Utils.GetRandomTier(consumRates);
            case ItemCategory.Misc:
                return Utils.GetRandomTier(miscRates);
            default:
                return Utils.GetRandomTier(consumRates);
                
        }
        
    }
    public ItemCategory GetRandomCategory()
    {
        ItemCategory type = Utils.Get_RandomType<ItemCategory>();
        Debug.Log($"아이템 카테고리 결정 {type}");
        return type;

    }
    public void OnFloorChange()
    {
        floor = DungeonManager.instance.Get_Floor();
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
        
        // 아이템의 기본 속성 복사 (CopyBaseProperties 이용)
        // 참고: ItemBase.CopyBaseProperties()는 optionKeys만 복사하고 options 리스트는 초기화함
        
        switch (origin.category)
        {
            case Defines.ItemCategory.Equipment:
                item = new EquipItem();
                origin.CopyBaseProperties(item); // 리플렉션/명시적 복사 대신 함수로 안전하게 처리
                item.maxStack = 1;
                break;
            case Defines.ItemCategory.Consumable:
                item = new ConsumableItem();
                origin.CopyBaseProperties(item);
                item.maxStack = 100;
                break;
            case Defines.ItemCategory.Misc:
                item = new MiscItem();
                origin.CopyBaseProperties(item);
                item.maxStack = 100;
                break;
            default:
                Debug.Log("ItemCategoryError");
                return null;
        }
        
        // ? 핵심 리팩토링: Copy() 및 하위 루프 완전 제거 ?
        // 설계도(origin)에 기록된 optionKeys 문자열만 순회하며 풀러에게 단일 호출
        if (origin.optionKeys != null)
        {
            foreach(string optKey in origin.optionKeys)
            {
                // 풀러가 빈 껍데기를 가져와서 내부 부품까지 전부 싹 조립해서 반환해줍니다.
                ModifierOption newOption = ModifierManager.instance.Get_ModifierOption(optKey);
                
                if (newOption != null)
                {
                    // 조립된 옵션 객체를 실제 아이템 인스턴스의 options 리스트에 꽂아줌
                    item.options.Add(newOption);
                }
            }
        }
        
        return item;
    }
    public void MakeFieldItem()
    {
        GameObject go = poolManager.ObjectPool(TileType.Item);
        
    }
}
