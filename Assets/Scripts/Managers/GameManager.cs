using UnityEngine;
using Cysharp.Threading.Tasks;
public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null) return null;
            return _instance;
        }
    }
    [SerializeField] DataManager dataManager;
    [SerializeField] MapManager mapManager;
    [SerializeField] MonsterManager monsterManager;
    [SerializeField] UIManager uiManager;
    [SerializeField] ItemManager itemManager;
    [SerializeField] ItemFactory itemFactory;
    [SerializeField] PoolManager poolManager;
    [SerializeField] StringKeyManager stringKeyManager;
    [SerializeField] SceneController sceneController;
    [SerializeField] SpriteManager spriteManager;
    [SerializeField] DungeonManager dungeonManager;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject playerObj;
    [SerializeField] EventManager eventManager;
    [SerializeField] PlayerEntity playerEntity;
    [SerializeField] EffectManager effectManager;
    [SerializeField] ModifierManager modifierManager;
    private async UniTask Awake()
    {
        await Init();
    }
    async UniTask Init()
    {
        GameObject Managers = GameObject.Find("Managers");
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        if (dataManager == null)
        {
            dataManager = new DataManager();
            await dataManager.Init();
        }
        if(modifierManager == null)
        {
            modifierManager = GameObject.Find("ModifierManager").transform.GetComponent<ModifierManager>();
            await modifierManager.Init();
        }
        if(uiManager == null)
        {
            uiManager = GameObject.Find("UIManager").transform.GetComponent<UIManager>();
            if(uiManager == null)
            {
                GameObject go = new GameObject();
                go.name = "UIManager";
                uiManager = Utils.GetOrAddComponent<UIManager>(go);

            }
        }
        if(poolManager == null)
        {
            poolManager = GameObject.Find("ObjectPooler").transform.GetComponent<PoolManager>();
            if(poolManager == null)
            {
                GameObject go = new GameObject();
                go.name = "ObjectPooler";
                poolManager = Utils.GetOrAddComponent<PoolManager>(go);
            }
        }
        if(itemManager == null)
        {
            GameObject go = GameObject.Find("ItemManager");
            itemManager = Utils.GetOrAddComponent<ItemManager>(go);
            itemFactory = itemManager.Get_ItemFactory();
        }
        if(itemFactory == null)
        {
            itemFactory = itemManager.Get_ItemFactory();
        }
        if(sceneController == null)
        {
            sceneController = new SceneController();
        }
        if (spriteManager == null)
        {
            spriteManager = GameObject.Find("SpriteManager").transform.GetComponent<SpriteManager>();
        }
        if(playerObj == null)
        {
            playerObj = Instantiate(playerPrefab);
            playerObj.transform.SetParent(Managers.transform);
            playerEntity = playerObj.transform.GetComponent<PlayerEntity>();
        }
        if(monsterManager == null)
        {
            monsterManager = GameObject.Find("MonsterManager").transform.GetComponent<MonsterManager>();
            
        }
        if(dungeonManager == null)
        {
            dungeonManager = GameObject.Find("DungeonManager").transform.GetComponent<DungeonManager>();
        }
        if (eventManager == null)
        {
            eventManager = GameObject.Find("EventManager").transform.GetComponent<EventManager>();
        }
        if(effectManager == null)
        {
            effectManager = GameObject.Find("EffectManager").transform.GetComponent<EffectManager>();
        }
        await monsterManager.Init();
        DontDestroyOnLoad(Managers);
        
    }
    public EffectManager Get_EffectManager()
    {
        if(effectManager == null)
        {
            effectManager = GameObject.Find("EffectManager").transform.GetComponent<EffectManager>();
        }
        return effectManager;
    }
    public GameObject Get_PlayerObj()
    {
        if(playerObj == null)
        {
            playerObj = Instantiate(playerPrefab);
        }
        return playerObj;
    }
    public PlayerEntity Get_PlayerEntity()
    {
        if(playerEntity == null)
        {
            Get_PlayerObj().transform.GetComponent<PlayerEntity>();
        }
        return playerEntity;
    }
    #region Get_Managers
    public EventManager Get_EventManager()
    {
        return eventManager;
    }
    public DataManager Get_DataManager()
    {
        if(dataManager == null)
        {
            dataManager = new DataManager();
            dataManager.Init();
        }
        
        return dataManager;
    }
    public ModifierManager Get_ModifierManager()
    {
        if(modifierManager == null)
        {
            modifierManager = GameObject.Find("ModifierManager").transform.GetComponent<ModifierManager>();
        }

        return modifierManager;
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

    public PoolManager Get_PoolManager()
    {
        if (poolManager == null)
        {
            poolManager = GameObject.Find("ObjectPooler").transform.GetComponent<PoolManager>();
            if (poolManager == null)
            {
                GameObject go = new GameObject();
                go.name = "ObjectPooler";
                poolManager = Utils.GetOrAddComponent<PoolManager>(go);
            }
        }
        return poolManager;
    }
    public SceneController Get_SceneController()
    {
        return sceneController;
    }
    public DungeonManager GetDungeonManager()
    {
        return dungeonManager;
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
