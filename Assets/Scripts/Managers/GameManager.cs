using UnityEngine;

public class GameManager : MonoBehaviour
{

    GameManager _instance;
    public static GameManager instance;
    DataManager dataManager;
    MapManager mapManager;
    MonsterManager monsterManager;
    ItemManager itemManager;
    ItemFactory itemFactory;
    PoolManager poolManager;
    void Init()
    {
        if(_instance == null)
        {
            _instance = transform.GetComponent<GameManager>();
        }
        if(instance == null)
        {
            instance = _instance;
        }
        if (dataManager == null)
        {
            dataManager = new DataManager();
            dataManager.Init();
        }
        if(poolManager == null)
        {
            poolManager = GameObject.Find("ObjectPooler").transform.GetComponent<PoolManager>();
        }
        if(itemManager == null)
        {
            itemManager = new ItemManager();
            itemFactory = itemManager.Get_ItemFactory();
        }
        if(itemFactory == null)
        {
            itemFactory = itemManager.Get_ItemFactory();
        }
    }

    

    #region Get_Managers

    public DataManager Get_DataManager()
    {
        if(dataManager == null)
        {
            dataManager = new DataManager();
            dataManager.Init();
        }
        
        return dataManager;
    }
    public ItemManager Get_ItemManager()
    {
        if(itemManager == null)
        {
            itemManager = new ItemManager();
            itemManager.Init();
        }
        return itemManager;
    }
    public MapManager Get_MapManager()
    {
        if(mapManager == null)
        {
            mapManager = new MapManager();

        }
        return mapManager;
    }

    public MonsterManager Get_MonsterManager()
    {
        return monsterManager;
    }
    #endregion
}
