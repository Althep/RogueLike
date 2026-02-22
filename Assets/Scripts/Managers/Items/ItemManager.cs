using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using static Defines;
public class ItemManager :MonoBehaviour
{
    [SerializeField]ItemFactory itemFactory;
    [SerializeField]PlayerEntity playerEntity;
    MapManager mapManager;
    PoolManager poolManager;
    int itemGenPerTile = 70;
    public Dictionary<Vector2Int, List<ItemEntity>> fieldItems = new();
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
        if(mapManager == null)
        {
            mapManager = GameManager.instance.Get_MapManager();
        }
    }

    public GameObject MakeItem()
    {
        GameObject go = poolManager.ObjectPool(Defines.TileType.Item);
        ItemEntity entity = go.transform.GetComponent<ItemEntity>();
        entity.item = itemFactory.GetRandomItem();
        return go;
    }
    public GameObject MakeRandomItem(Vector2 pos)
    {
        GameObject go = poolManager.ObjectPool(Defines.TileType.Item);
        ItemEntity entity = go.GetComponent<ItemEntity>();
        entity.item = itemFactory.GetRandomItem();
        entity.id = entity.item.name;
        go.transform.position = pos;
        Vector2Int keyPos = new Vector2Int((int)pos.x, (int)pos.y);
        if (!fieldItems.ContainsKey(keyPos))
        {
            fieldItems.Add(keyPos, new List<ItemEntity>());
        }
        fieldItems[keyPos].Add(entity);
        return go;
    }
    public async UniTask OnVisitNewFloor()
    {
        int tileCount = mapManager.GetEmptyPosList().Count;
        int itemCount = tileCount/itemGenPerTile;
        
        for(int i = 0; i < itemCount; i++)
        {
            Vector2 targetPos = mapManager.GetRandomTilePos();
            GameObject go = MakeRandomItem(targetPos);
            await Utils.WaitYield(i);
        }
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
        itemFactory.OnFloorChange();
    }
}
