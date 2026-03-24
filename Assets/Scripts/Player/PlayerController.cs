using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [SerializeField] GameObject Pre_Player;
    [SerializeField] GameObject playerObj;
    MapManager mapManager;
    PlayerEntity playerEntity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        OnAwake();
    }
    void OnAwake()
    {
        if(PlayerController.instance == null)
        {
            PlayerController.instance = this;
        }
        if(mapManager == null)
        {
            mapManager = GameManager.instance.Get_MapManager();
        }
    }

    public GameObject Get_PlayerObj()
    {
        if(playerObj == null)
        {
            playerObj = Instantiate(Pre_Player);
            GameManager.instance.SetMapObjectParent(playerObj);
            playerEntity = playerObj.GetComponent<PlayerEntity>();
        }

        return playerObj;
    }
    public PlayerEntity Get_PlayerEntity()
    {
        
        if(playerEntity == null)
        {
            if(playerObj == null)
            {
                Get_PlayerObj();
            }
            playerEntity = playerObj.GetComponent<PlayerEntity>();
        }
        return playerEntity;
    }
    public void Set_Player_UpStair(int stairNumber)
    {
        Vector2Int pos = mapManager.Get_UpStairPos(stairNumber);
        mapManager.AddMapData(pos, playerEntity);
        Set_PlayerPos(pos);
    }

    public void Set_Player_DownStair(int stairNumber)
    {
        Vector2Int pos = mapManager.Get_DownStairPos(stairNumber);
        mapManager.AddMapData(pos, playerEntity);
        Set_PlayerPos(pos);
    }
    public void Set_PlayerPos(Vector2Int pos)
    {
        playerObj.transform.position = (Vector2)pos;
        //playerEntity.UpdateFoV(pos, (int)playerEntity.GetEntityStat(Defines.ModifierTriggerType.OnMove)[Defines.StatType.Vision]);
    }
}
