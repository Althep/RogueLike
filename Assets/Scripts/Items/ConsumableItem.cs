using System;
using UnityEngine;

public class ConsumableItem : ItemBase
{
    public Defines.ConsumableType consumableType;

    public override ItemBase Clone()
    {
        ConsumableItem newConsum = ItemManager.instance.ItemMake(name) as ConsumableItem;
        CopyBaseProperties(newConsum);
        newConsum.consumableType = consumableType;
        return newConsum;
    }

    public override Enum GetSpecificType()
    {
        return consumableType;
    }
}
