using UnityEngine;

public class GameManager : MonoBehaviour
{

    GameManager _instance;
    public static GameManager instance;
    DataManager dataManager;
    MapManager mapManager;
    MonsterManager monsterManager;
    UIManager uiManager;
    ItemManager itemManager;
    ItemFactory itemFactory;
    PoolManager poolManager;
    StringKeyManager stringKeyManager;
    GameObject playerObj;

    private void Awake()
    {
        Init();
    }
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
        DontDestroyOnLoad(this);
    }
    public GameObject Get_PlayerObj()
    {
        return playerObj;
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

    public StringKeyManager Get_StringKeyManager()
    {
        return stringKeyManager;
    }

    public UIManager Get_UIManager()
    {
        return uiManager;
    }
    #endregion
    #region Maps
    public void EntityMove(Vector2Int origin,Vector2Int target,Defines.TileType type)
    {
        mapManager.EntityMove(origin, target, type);
    }
    #endregion
    #region UIs
    public void Bind_UI_PopUp(UI_PopUpObj bindTarget)
    {
        uiManager.BindPopUp(bindTarget);
    }
    public void PopUpUI(string name)
    {
        uiManager.Pop_Up_UI(name);
    }
    #endregion
    #region QuickKeys

    public void Add_MoveKey()
    {

    }

    public void Add_QuickKey()
    {

    }

    #endregion
}
