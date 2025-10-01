using UnityEngine;

public class ItemBase 
{
    public string id;
    public int itemCount;
    public int maxStack;
    public Defines.ItemCategory category;

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
        return itemCount + amount < maxStack;
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
}
