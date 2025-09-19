using UnityEngine;

public class ItemManager 
{
    ItemFactory itemFactory;
    public void Init()
    {
        if(itemFactory == null)
        {
            itemFactory =  new ItemFactory();
        }
    }

    public ItemFactory Get_ItemFactory()
    {
        return itemFactory;
    }
    public ItemBase ItemMake(string id)
    {
        return itemFactory.ItemMake(id);
    }
}
