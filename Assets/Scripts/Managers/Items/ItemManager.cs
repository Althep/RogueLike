using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public class ItemManager :MonoBehaviour
{
    [SerializeField]ItemFactory itemFactory;
    [SerializeField]PlayerEntity playerEntity;
    PoolManager poolManager;
    public List<GameObject> items;
    private void Awake()
    {
        Init();
    }
    public void Init()
    {
        if(itemFactory == null)
        {
            itemFactory =  new ItemFactory();
        }
        if(playerEntity == null)
        {
            playerEntity = GameManager.instance.Get_PlayerEntity();
        }
        if(poolManager == null)
        {
            poolManager = GameManager.instance.Get_PoolManager();
        }
    }

    public GameObject MakeItem()
    {
        GameObject go = poolManager.ObjectPool(Defines.TileType.Item);
        ItemEntity entity = go.transform.GetComponent<ItemEntity>();
        ItemCategory type = itemFactory.GetRandomCategory();
        int tier = itemFactory.GetRandomTier(type);

        return go;
    }

    public ItemFactory Get_ItemFactory()
    {
        if(itemFactory == null)
        {
            itemFactory = new ItemFactory();
        }
        return itemFactory;
    }

    public ItemBase ItemMake(string id)
    {
        return itemFactory.GetItemScript(id);
    }

    public void AddStartItems(List<string> itemNames)
    {
        /*
        if(playerEntity == null)
        {
            playerEntity = GameManager.instance.Get_PlayerEntity();
        }
        InventoryData inventory = playerEntity.GetInventory();

        for(int i = 0; i < itemNames.Count; i++)
        {
            ItemBase item = itemFactory.GetItemScript(itemNames[i]);
            //inventory.AddinInventory(item);
        }
        */
    }

    public void OnFloorChange()
    {

    }
}
