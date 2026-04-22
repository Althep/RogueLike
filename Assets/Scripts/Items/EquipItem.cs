using UnityEngine;
using System;
using System.Collections.Generic;
public class EquipItem : ItemBase
{
    public Defines.EquipmentType equipmentType;
    public Defines.SlotType slot;
    public List<Modifier> addOptions;
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
        List<ModifierSaveData> myModifiers = new List<ModifierSaveData>();
        List<ModifierSaveData> addOptions = new List<ModifierSaveData>(); // 장비의 추가 옵션
        for (int i = 0; i<options.Count; i++)
        {
            myModifiers.Add(options[i].SaveData());
        }
        
        for (int i = 0; i<addOptions.Count; i++)
        {
            addOptions.Add(this.addOptions[i].SaveData());
        }

        itemSaveData.itemModifiers = myModifiers;
        itemSaveData.addOptionsData = addOptions;
        return itemSaveData;
    }
}
