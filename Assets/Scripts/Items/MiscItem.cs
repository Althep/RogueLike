using System;
using UnityEngine;
using static Defines;

public class MiscItem : ItemBase
{
    public Defines.MiscType miscType;

    public override ItemBase Clone()
    {
        MiscItem newMisc = ItemManager.instance.ItemMake(id) as MiscItem;
        CopyBaseProperties(newMisc);
        newMisc.miscType = miscType;
        return newMisc;
    }

    public override Enum GetSpecificType()
    {
        return miscType;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
