using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;

public class InventoryData
{
    [SerializeField]
    public List<ItemBase> inventory = new List<ItemBase>();
    public int maxInventory = 40;
    private ItemManager itemManager;
    //public InventoryItemPanel inventoryUI;
    int gold;

    

    public bool AddingInventory(ItemBase item)
    {
        bool canGet = false;
        switch (item.category)
        {
            case Defines.ItemCategory.Equipment:
                if (InventoryCountCheck())
                {
                    AddInventory(item);
                   canGet = true;
                }
                break;
            case Defines.ItemCategory.Consumable:
                if (ConsumableIdCheck(item))
                {
                    Add_ExistConsumItem(item);
                    canGet = true;
                }
                else
                {
                    if (InventoryCountCheck())
                    {
                        AddInventory(item);
                        canGet = true;
                    }
                }
                break;
            case Defines.ItemCategory.Misc:
                if (InventoryCountCheck())
                {
                    AddInventory(item);
                    canGet = true;
                }
                break;
            default:
                break;
        }
        return canGet;
    }

    public void AddInventory(ItemBase item)
    {
        ItemBase copiedItem = item.Clone();

        inventory.Add(copiedItem);

    }


    public bool InventoryCountCheck()
    {
        if (inventory.Count+1<maxInventory)
        {
            return true;
        }
        return false;
    }

    public bool ConsumableIdCheck(ItemBase item)
    {
        if(inventory.Exists(i => i.id == item.id))
        {
            return true;
        }
        return false;
    }

    public void Add_ExistConsumItem(ItemBase item)
    {
        ItemBase target = inventory.First(i => i.id == item.id);
        target.itemCount+=item.itemCount;
    }
}
