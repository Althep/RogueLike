using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class InventoryData
{
    [SerializeField]
    public List<ItemBase> inventory = new List<ItemBase>();
    public int maxInventory = 40;
    private ItemManager itemManager;
    //public InventoryItemPanel inventoryUI;
    int gold;

    

    public void AddinInventory(ItemBase item)
    {
        Debug.Log("Added Item Invoke");
        if(itemManager == null)
        {
            itemManager = GameManager.instance.Get_ItemManager();
        }
        DataManager dataManager = GameManager.instance.Get_DataManager();
        Dictionary<string, ItemBase> itemData = dataManager.Get_ItemData();
        if(itemData == null)
        {
            Debug.Log("ItemData Null");
        }
        Debug.Log($"item name : {item.name}");
        /*
        if (item == null || !itemData.ContainsKey(item.name))
        {
            Debug.Log("Item Id isn't Contain or item is Null");
            return;
        }*/
        ItemBase addedItem = OnGetCountCheck(item);
        if (addedItem == null)
        {
            Debug.Log("Can't make item script");
            return;
        }
        if (addedItem != item)
        {
            Debug.Log("script isn't same!");
            AddinInventory(item);
        }
        //addedItem.itemCount = item.itemCount;
        if ((item.id != addedItem.id && addedItem.itemCount <=0)||(item.id == addedItem.id && addedItem.itemCount <=0))
        {
            Debug.Log("Item Count <= 0 In inventoryData AddinVentoryFcuntion");
            Debug.Log($"item count : {item.itemCount} addedItemCount : {addedItem.itemCount}");
            return;
        }


        if (item == addedItem)
        {
            Debug.Log("Same Script");
            AddNewItem(addedItem);
        }
        else//같은 아이템이 있거나 획득아이템의 갯수가 최대스택보다 높을경우
        {
            int index = -1;
            index = CheckSameItem(addedItem);
            if (index != -1)
            {
                int prevItemCount = addedItem.itemCount;
                inventory[index].AddStack(addedItem, addedItem.itemCount);
                if (prevItemCount!= addedItem.itemCount)
                {
                    AddinInventory(addedItem);
                }
                else
                {
                    Debug.Log("Item Get Loof Stoped");
                    return;
                }
            }
            else
            {
                AddNewItem(addedItem);
                Debug.Log("More than max Stack");
            }

        }
        //EventManager.instance.OnInventoryUpdate.Invoke();

    }
    public ItemBase OnGetCountCheck(ItemBase item)
    {
        ItemBase getItem;
        if(itemManager == null)
        {
            GameManager.instance.Get_ItemManager();
        }
        
        if (item.itemCount > item.maxStack)
        {
            getItem = itemManager.ItemMake(item.id);
            getItem.itemCount = getItem.maxStack;
            item.itemCount -= getItem.maxStack;
            return getItem;
        }
        else
        {
            getItem = item;
        }
        return getItem;
    }
    public ItemBase OnGetItemCheck(ItemBase item)
    {
        int index = CheckSameItem(item);
        if (itemManager == null)
        {
            GameManager.instance.Get_ItemManager();
        }
        
        if (index == -1)//인벤토리에 같은 아이템이 있는지 체크
        {//같은아이템 존재X
            if (InventorySlotCheck())//인벤토리 여분 확인
            {
                //item = OnGetCountCheck(item); //아이템 갯수확인
                return item;
            }

            else
            {
                return null;
            }
        }
        else
        {//같은 아이템O
            int amount = item.itemCount;
            int rest = 0;
            amount = inventory[index].maxStack - inventory[index].itemCount;
            ItemBase getItem = itemManager.ItemMake(item.id);
            getItem.itemCount = amount;
            rest = item.itemCount - amount;
            item.itemCount = rest;
            if (amount > 0)
            {
                Debug.Log("work!");
            }
            else
            {
                Debug.Log("InventoryDataOnGetItemCheck Error");
                return null;
            }
            return getItem;

        }

    }

    bool InventorySlotCheck()
    {
        bool CanGet = true;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null)
            {
                CanGet = true;
                return CanGet;
            }
        }
        if (inventory.Count >= maxInventory)
            CanGet = false;

        return CanGet;
    }

    int CheckSameItem(ItemBase item)
    {
        //없을 시 -1
        int index = -1;
        for (int i = 0; i<inventory.Count; i++)
        {
            if (inventory[i]!=null &&
                inventory[i].id == item.id &&
                inventory[i].itemCount<inventory[i].maxStack)
            {
                return index = i;
            }

        }

        return index;
    }

    void AddNewItem(ItemBase item)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = DeepCopyItemData(item);
                inventory[i].itemCount = item.itemCount;
                Debug.Log("AddItem!");
                item.itemCount = 0;
                return;
            }
        }
        inventory.Add(DeepCopyItemData(item));


        inventory[inventory.Count - 1].itemCount = item.itemCount;
        item.itemCount = 0;
        //EventManager.instance.OnInventoryUpdate.Invoke();
    }


    ItemBase DeepCopyItemData(ItemBase original)
    {
        if (itemManager == null)
        {
            GameManager.instance.Get_ItemManager();
        }
        var item = itemManager.ItemMake(original.id);
        item.itemCount = original.itemCount;
        return item;
    }

    public void InventorySlotSwap(int first, int second)
    {
        Debug.Log("InventorySlotSwap");

        if (inventory[first] == null)
            return;

        if (inventory[second] == null)
        {
            inventory[second] = inventory[first];
            inventory[first] = null;
        }
        else if (inventory[first].id == inventory[second].id)
        {
            // 같은 아이템이면 스택을 합침
            int transferableAmount = inventory[second].maxStack - inventory[second].itemCount;
            int moveAmount = Mathf.Min(transferableAmount, inventory[first].itemCount);

            inventory[second].itemCount += moveAmount;
            inventory[first].itemCount -= moveAmount;

            if (inventory[first].itemCount == 0)
                inventory[first] = null;
        }
        else
        {
            // 다른 아이템이면 위치만 교환
            (inventory[first], inventory[second]) = (inventory[second], inventory[first]);
        }

        //EventManager.instance.OnInventoryUpdate.Invoke();
    }
    public void DecreaseItemCount(ItemBase item, int count)
    {
        var targetItem = inventory.FirstOrDefault(i => i == item);
        if (targetItem != null)
        {
            if (targetItem.itemCount < count)
            {
                count = targetItem.itemCount;
            }

            targetItem.itemCount -= count;
        }
        if (targetItem.itemCount == 0)
        {
            inventory.Remove(targetItem);
        }
    }
}
