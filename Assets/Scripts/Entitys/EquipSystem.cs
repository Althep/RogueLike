using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class EquipSystem
{
    Dictionary<SlotType, EquipItem> equipments = new Dictionary<SlotType, EquipItem>();

    private ModifierController modifierController;

    public void InitSystem(ModifierController mc)
    {
        modifierController = mc;
    }

    public void ItemEquip(EquipItem target)
    {
        SlotType slot = target.slot;
        UnEquipItem(target.slot); // 기존 장비 해제 
        equipments[slot] = target; // 장비 착용

        // 새 장비 모디파이어 적용
        foreach (ModifierOption option in target.options) modifierController.AddEquipment(option);
        foreach (ModifierOption option in target.addOptions) modifierController.AddEquipment(option);
    }

    public void UnEquipItem(SlotType slot)
    {
        if (equipments.ContainsKey(slot) && equipments[slot] != null)
        {
            EquipItem origin = equipments[slot];
            foreach (ModifierOption modi in origin.options) modifierController.RemoveEquipment(modi);
            foreach (ModifierOption modi in origin.addOptions) modifierController.RemoveEquipment(modi);
        }
    } 

    public StatType Get_WeaponAttribute()
    {
        if (equipments.ContainsKey(SlotType.MainHand) && equipments[SlotType.MainHand] is Weapon weapon)
        {
            return weapon.attributeStat;
        }
        return StatType.Str;
    }

    public void ClearEquips()
    {
        equipments.Clear();
    }


    public List<ItemSaveData> SaveEquips()
    {
        List<ItemSaveData> saveData = new List<ItemSaveData>();
        foreach(var equip in equipments.Values)
        {
            ItemSaveData data = equip.SaveData();
            saveData.Add(data);
        }

        return saveData;
    }
    
}
