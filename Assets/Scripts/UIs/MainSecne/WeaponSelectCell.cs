using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class WeaponSelectCell : SelectSubCell
{
    Defines.WeaponType myType;
    string itemKey;
    public override void SetMyType<T>(T type,SelectPanels selectPanels)
    {
        base.SetMyType<T>(type, selectPanels);
       if(type is Defines.WeaponType weaponType)
        {
            myType = weaponType;
        }
    }
    public void SetMyKey(string key)
    {
        itemKey = key;
    }
    public override void AddButtonFunction()
    {
        AddUIEvent(this.gameObject, ButtonFunction, Defines.UIEvents.Click);
    }
    public override void ButtonFunction(PointerEventData pev)
    {
        selectPanel.Set_Weapon(itemKey);
    }
}
