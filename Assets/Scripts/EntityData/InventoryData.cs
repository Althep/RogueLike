using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;

public class InventoryData
{
    [SerializeField]

    public int maxInventory = 40;
    private ItemManager itemManager;
    public ItemBase[] inventory = new ItemBase[40];
    UI_Inventory inventoryUI;

    int gold;



    public bool TryAddInventory(ItemBase item)
    {
        bool canGet = false;
        int index = Get_EmptyInventoryIndex();
        switch (item.category)
        {
            case Defines.ItemCategory.Equipment:
                if (InventoryCountCheck(index))
                {
                    AddInventory(item, index);
                    canGet = true;
                }
                break;
            case Defines.ItemCategory.Consumable:
                index = ConsumableIdCheck(item);
                if (index !=-1)
                {
                    Add_ExistConsumItem(item, index);
                    canGet = true;
                }
                else
                {
                    index = Get_EmptyInventoryIndex();
                    if (InventoryCountCheck(index))
                    {
                        AddInventory(item, index);
                        canGet = true;
                    }
                }
                break;
            case Defines.ItemCategory.Misc:
                if (InventoryCountCheck(index))
                {
                    AddInventory(item, index);
                    canGet = true;
                }
                break;
            default:
                break;
        }
        return canGet;
    }

    void AddInventory(ItemBase item, int index)
    {
        ItemBase copiedItem = item.Clone();

        inventory[index] = copiedItem;
        UpdateInventory(index);
    }


    int Get_EmptyInventoryIndex()
    {
        for (int i = 0; i<inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                return i;
            }
        }
        Debug.Log("Count over!");
        return -1;
    }

    bool InventoryCountCheck(int index)
    {
        if (index!=-1)
        {
            return true;
        }
        return false;
    }
    int ConsumableIdCheck(ItemBase item)
    {
        for (int i = 0; i<inventory.Length; i++)
        {
            if (inventory[i]== null)
            {
                continue;
            }
            if (inventory[i].id == item.id)
            {
                return i;
            }
        }
        return -1;
    }

    void Add_ExistConsumItem(ItemBase item, int index)
    {
        ItemBase target = inventory.First(i => i.id == item.id);
        target.itemCount+=item.itemCount;
        UpdateInventory(index);
    }

    public void Set_InventoryUI(UI_Inventory inventorySc)
    {
        inventoryUI = inventorySc;
    }
    public void UpdateInventory(int index)
    {
        if (inventoryUI == null)
        {
            return;
        }
        inventoryUI.UpdateItemSlot(index);
    }
}
