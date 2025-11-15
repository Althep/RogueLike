using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using static Defines;
public class ItemSelectCell : SelectSubCell
{
    [SerializeField] string itemKey;
    ItemSlotListCell myParents;
    SlotType slot;
    public override void SetMyType<T>(T type, SelectSubPanel panel)
    {
        base.SetMyType(type, panel);
        if(type is SlotType slot)
        {
            this.slot = slot;
        }
    }

    public void SetMyKey(string key)
    {
        itemKey = key;
    }
    public void SetMyParents(ItemSlotListCell myParents)
    {
        this.myParents = myParents;
        Debug.Log("SetParents");
    }
    

    public override void ButtonFunction(PointerEventData pev)
    {
        Debug.Log($"[ItemSelectCell] selectPanel Type = {selectPanel?.GetType().Name}");
        Function();



    }
    public void Function()
    {
        if (selectPanel is ItemSelectPanel panel)
        {
            panel.SelectItem(itemKey, slot, this.gameObject);
            myParents.Select(itemKey, this.gameObject);
            Debug.Log("111");
        }
    }
}
