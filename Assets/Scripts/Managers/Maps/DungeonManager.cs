using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;
    MapManager mapManager;
    MonsterManager monsterManager;
    ItemManager itemManager;
    List<int> visitiedFloor = new List<int>();
    TierCalculator tierCalculator;
    PoolManager poolManager;
    //PlayerController playerController;
    GameObject playerObj;
    int floor;
    bool isVisited = false;
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        if(DungeonManager.instance == null)
        {
            DungeonManager.instance = this;
        }
        if(mapManager == null)
        {
            mapManager = GameManager.instance.Get_MapManager();
        }
        if(monsterManager == null)
        {
            monsterManager = GameManager.instance.Get_MonsterManager();
        }
        if(tierCalculator == null)
        {
            tierCalculator = new TierCalculator();
        }
        if(poolManager == null)
        {
            poolManager = GameManager.instance.Get_PoolManager();
        }
        if(itemManager == null)
        {
            itemManager = GameManager.instance.Get_ItemManager();
        }

    }

    public TierCalculator GetTierCalculator()
    {
        return tierCalculator;
    }

    public async UniTask ChangeFloor(int changes)
    {
        floor+=changes;
        Debug.Log("floor Changed");
        await OnFloorChange();
    }
    
    public async UniTask OnFloorChange()
    {
        if(floor <= 0)
        {
            Debug.Log("Game End");
            return;
        }
        if (IsVisited(floor))
        {
            await VisitedFloor();
        }
        else
        {
            await VisitNewFloor();
        }

    }
    bool IsVisited(int floor)
    {
        if (visitiedFloor.Contains(floor))
        {
            isVisited = true;
        }
        else
        {
            isVisited = false;
        }
        return isVisited;
    }
    public int Get_Floor()
    {
        return floor;
    }
    public bool Get_Visitied()
    {
        return isVisited;
    }
    public async UniTask VisitNewFloor()
    {
        if (visitiedFloor.Contains(floor))
        {
            Debug.Log("Floor Error Floor Already Visited");
            return;
        }
        await GenerateDungeon();
        visitiedFloor.Add(floor);
        monsterManager.OnFloorChange();
        await itemManager.OnFloorChange();
        mapManager.CalculateInitialCost();
    }
    async UniTask VisitedFloor()
    {
        
    }

    public async UniTask GenerateDungeon()
    {
        //OnFloorChange();
        float startTime = Time.realtimeSinceStartup;
        await poolManager.PreWarmObjects();
        Debug.Log($"[Profile] 데이터 생성 시간: {Time.realtimeSinceStartup - startTime}s");

        float spawnStart = Time.realtimeSinceStartup;
        await mapManager.MapMake();
        Debug.Log($"[Profile] 오브젝트 배치 시간: {Time.realtimeSinceStartup - spawnStart}s");

        //await itemManager.OnFloorChange();
        //await itemManager.OnVisitNewFloor();

        await monsterManager.MonsterSpawn();

        if(floor == 1)
        {
            PlayerController.instance.Set_Player_UpStair(0);
        }
        Debug.Log($"[Profile] 총 소요 시간: {Time.realtimeSinceStartup - startTime}s");
    }
}
