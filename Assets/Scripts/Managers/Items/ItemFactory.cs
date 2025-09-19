using UnityEngine;

public class ItemFactory 
{
    
    

    public ItemBase ItemMake(string id)
    {
        return new ItemBase();
    }
}
