using UnityEngine;
using System;
using System.Collections.Generic;
public abstract class ItemBase 
{
    public string id;
    public string name;
    public int itemCount;
    public int maxStack;
    public Defines.ItemCategory category;
    public List<Modifier> options;
    public int AddStack(ItemBase item, int amount)
    {
        int overflow = 0;
        if (!CanStack(amount))
        {
            overflow = itemCount + amount - maxStack;
            itemCount = maxStack;
            item.itemCount = overflow;
            Debug.Log("itemCount = overflow;");
        }
        else
        {
            itemCount += amount;
            item.itemCount = 0;
            Debug.Log("itemCount = 0;");
        }
        Debug.Log("1111111111111111111111111");
        return overflow;
    }

    public bool CanStack(int amount)
    {
        return category == Defines.ItemCategory.Consumable;
    }

    public int ReturnRest(int amount)
    {
        int rest = 0;

        if (itemCount + amount > maxStack)
        {
            rest = itemCount + amount - maxStack;
        }

        return rest;
    }
    public abstract Enum GetSpecificType();
}
