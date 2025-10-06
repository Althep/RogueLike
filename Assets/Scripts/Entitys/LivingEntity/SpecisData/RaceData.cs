using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
using UnityEditorInternal.VersionControl;

public class RaceData
{
    Defines.Races race;

    public string disPlayName;

    public int bonusDice;

    public List<string> startItems;

    ModifierManager modifierManager = new ModifierManager();

    public ModifierContext modifireContext;
    public List<Modifier> modifiers = new List<Modifier>();



    public bool CanEquip()
    {
        foreach(Modifier modis in modifiers)
        {
            ModifierTriggerType type = modis.triggerType;
        }
        return true;
    }


    public bool IsRestricted(ItemBase item,ModifierContext context)
    {
        bool isRistriced = false;

        float value = modifierManager.ApplyModifiers(ModifierTriggerType.OnEquip, context);

        if (context.ModifiedValue>=0)
        {
            isRistriced = true;
        }

        return isRistriced;
    }


}
