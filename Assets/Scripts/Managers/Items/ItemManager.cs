using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using static Defines;
public class ItemManager :MonoBehaviour
{
    public static ItemManager instance;
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
        if(instance == null)
        {
            instance = this;
        }
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
        Vector3 targetPos = new Vector3(pos.x, pos.y, -1);
        go.transform.position = targetPos;
        
        Vector2Int keyPos = new Vector2Int((int)pos.x, (int)pos.y);
        if (!fieldItems.ContainsKey(keyPos))
        {
            fieldItems.Add(keyPos, new List<ItemEntity>());
        }
        mapManager.AddMapData(keyPos, entity);
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

    public bool GroundCheck(Vector2Int pos)
    {
        if (fieldItems.ContainsKey(pos))
        {
            return true;
        }
        return false;
    }
    public List<ItemEntity> Get_GroundItems(Vector2Int pos)
    {
        if (fieldItems.ContainsKey(pos))
        {
            return fieldItems[pos];
        }
        return null;
    }
}
