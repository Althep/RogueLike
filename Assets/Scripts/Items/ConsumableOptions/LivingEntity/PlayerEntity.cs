using UnityEngine;
using System;
using System.Collections.Generic;
public class PlayerEntity : LivingEntity
{
    InventoryData inventory = new InventoryData();



    public InventoryData GetInventory()
    {
        return inventory;
    }

    
    
}
