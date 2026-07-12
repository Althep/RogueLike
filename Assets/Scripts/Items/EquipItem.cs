using UnityEngine;
using System;
using System.Collections.Generic;
public class EquipItem : ItemBase
{
    public Defines.EquipmentType equipmentType;
    public Defines.EquipCategory equipCategory;
    public Defines.ItemSubType itemSubType;
    public Defines.SlotType slot;
    public Defines.ItemRarity rarity;
    public List<ModifierOption> addOptions = new List<ModifierOption>();

    
    public override ItemBase Clone()
    {
        EquipItem newEquip = ItemManager.instance.ItemMake(id) as EquipItem;
        CopyBaseProperties(newEquip);
        newEquip.equipmentType = equipmentType;
        newEquip.slot = slot;
        return newEquip;
    }

    public override Enum GetSpecificType()
    {
        return equipmentType;
    }

    public override ItemSaveData SaveData()
    {
        ItemSaveData itemSaveData = new ItemSaveData();

        List<ModifierOptionSaveData> modifierOptionSaveData = new List<ModifierOptionSaveData>();

        for(int i = 0; i<addOptions.Count; i++)
        {
            ModifierOptionSaveData modSaved = addOptions[i].SaveData();
            itemSaveData.addOptionsData.Add(modSaved);
        }
        //List<ModifierSaveData> addOptions = new List<ModifierSaveData>(); // 장비의 추가 옵션
        return itemSaveData;
        /*
        for (int i = 0; i<addOptions.Count; i++)
        {
            addOptions.Add(this.addOptions[i].SaveData());
        }
        itemSaveData.itemCount = itemCount;
        itemSaveData.itemId = id;
        itemSaveData.addOptionsData = addOptions;
        return itemSaveData;*/
    }
}
