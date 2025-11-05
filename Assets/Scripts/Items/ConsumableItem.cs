using System;
using UnityEngine;

public class ConsumableItem : ItemBase
{
    public Defines.ConsumableType consumableType;

    public override Enum GetSpecificType()
    {
        return consumableType;
    }
}
