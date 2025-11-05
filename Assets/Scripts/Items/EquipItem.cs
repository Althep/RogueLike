using UnityEngine;
using System;
using System.Collections.Generic;
public class EquipItem : ItemBase
{
    public Defines.EquipmentType equipmentType;
    public Defines.SlotType slot;

    public override Enum GetSpecificType()
    {
        return equipmentType;
    }
}
