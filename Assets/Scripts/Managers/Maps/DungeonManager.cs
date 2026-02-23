using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
public class DungeonManager : MonoBehaviour
{
    MapManager mapManager;
    MonsterManager monsterManager;
    ItemManager itemManager;
    List<int> visitiedFloor = new List<int>();
    TierCalculator tierCalculator;
    PoolManager poolManager;
    GameObject playerObj;
    public int floor;

    private void Awake()
    {
        Init();
    }
    private void Init()
    {
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

    void OnFloorChange()
    {
        floor++;
        monsterManager.OnFloorChange();
        itemManager.OnFloorChange();
        mapManager.CalculateInitialCost();
    }

    public async UniTask GenerateDungeon()
    {
        OnFloorChange();
        float startTime = Time.realtimeSinceStartup;
        await poolManager.PreWarmObjects();
        Debug.Log($"[Profile] 데이터 생성 시간: {Time.realtimeSinceStartup - startTime}s");

        float spawnStart = Time.realtimeSinceStartup;
        await mapManager.MapMake();
        Debug.Log($"[Profile] 오브젝트 배치 시간: {Time.realtimeSinceStartup - spawnStart}s");


        await itemManager.OnVisitNewFloor();

        await monsterManager.MonsterSpawn();
        Debug.Log($"[Profile] 총 소요 시간: {Time.realtimeSinceStartup - startTime}s");
    }
}
