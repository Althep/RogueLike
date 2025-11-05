using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using static Defines;
public class ItemSelectCell : SelectSubCell
{
    [SerializeField] string itemKey;
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

    public override void AddButtonFunction()
    {
        base.AddButtonFunction();
    }

    public override void ButtonFunction(PointerEventData pev)
    {
        if(selectPanel is ItemSelectPanel panel)
        {
            panel.SelectItem(itemKey, slot,this.gameObject);
        }
        
    }

}
