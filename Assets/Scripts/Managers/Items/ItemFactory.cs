using UnityEngine;
using System;
using static Defines;
public class ItemFactory 
{
    ItemDataManager itemDataManager;
     ModifierPooler pooler;

    public ItemBase ItemMake(string name)
    {
        ItemBase origin = itemDataManager.Get_ItemDatas()[name];
        ItemBase item;
        switch (origin.category)
        {
            case Defines.ItemCategory.Equipment:
                item = new EquipItem();
                if(origin is EquipItem)
                {
                    
                }
                break;
            case Defines.ItemCategory.Consumable:
                item = new ConsumableItem();
                break;
            case Defines.ItemCategory.Misc:
                item = new MiscItem();
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
}
