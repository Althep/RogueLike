using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public struct SpawnPosData
{
    public Vector2 target;
    public int spawnRange;
}

public class MonsterSpawner 
{
    PoolManager poolmanager;
    MonsterManager monsterManager;
    MonsterDataManager monsterDataManager;
    MonsterFactory monsterFactory;
    [SerializeField] TierCalculator tierCalculator;
    [SerializeField] DungeonManager dungeonManager;
    SpawnDataManager spawnDataManager;
    Dictionary<int, List<string>> monsterToTier = new Dictionary<int, List<string>>();
    Dictionary<int, float> monsterRatesToTier = new Dictionary<int, float>();

    int floor;

    public MonsterSpawner(TierCalculator tierCalculator,MonsterFactory monsterFactory)
    {
        this.tierCalculator = tierCalculator;
        if(monsterFactory == null)
        {
            this.monsterFactory = monsterFactory;
        }
        if(spawnDataManager == null)
        {
            spawnDataManager = GameManager.instance.Get_MonsterManager().GetSpawnDataManager();
        }
    }

    public void OnFloorChange()
    {
        floor = dungeonManager.floor;
        TierCalculator.GetTierProbabilities(floor, 3, 5, false);
    }
    public int GetRandomTier()
    {
        return Utils.GetRandomTier(monsterRatesToTier);
    }
    public void Init()
    {
        if(poolmanager == null)
        {
            poolmanager = GameManager.instance.Get_PoolManager();
        }
        if(monsterDataManager == null)
        {
            monsterDataManager = GameManager.instance.Get_DataManager().GetMonsterDataManager();
        }
        if(dungeonManager == null)
        {
            dungeonManager = GameManager.instance.GetDungeonManager();
        }
        if(monsterManager == null)
        {
            monsterManager = GameManager.instance.Get_MonsterManager();
        }
        if(spawnDataManager == null)
        {
            spawnDataManager = GameManager.instance.Get_MonsterManager().GetSpawnDataManager();
        }
        if(monsterFactory == null)
        {
            monsterFactory = monsterManager.GetMonsterFactory();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /*
    public GameObject MonsterSpawn()
    {
        if(poolmanager == null)
        {
            Init();
        }

        GameObject go = poolmanager.ObjectPool(Defines.TileType.Monster);


        return go;

    }
    */
    public List<MapEntity> MonsterSpawn(MonsterSpawnData spawnData)
    {
        string monsterId = spawnData.monsterID;
        int monsterCount = UnityEngine.Random.Range(spawnData.minCount, spawnData.maxCount + 1);
        List<MapEntity> monsters = new List<MapEntity>();
        if(monsterDataManager == null)
        {
            Debug.Log("MonsterDataManager Null");
            monsterDataManager = GameManager.instance.Get_DataManager().GetMonsterDataManager();
        }
        for(int i = 0; i < monsterCount; i++)
        {
            EntityStat originData = monsterDataManager.GetMonsterStat(monsterId);
            GameObject monster = poolmanager.ObjectPool(Defines.TileType.Monster);
            MonsterEntity monsterEntity = monster.transform.GetComponent<MonsterEntity>();
            if(monsterEntity != null)
            {
                EntityStat monsterStat = originData.CopyStat();
                monsterEntity.SetMyStat(monsterStat);
            }
            monsters.Add(monsterEntity);
        }
        return monsters;
    }


    public MonsterSpawnDataBundle GetRandoSpawnBundle()
    {
        if (spawnDataManager == null)
        {
            spawnDataManager = GameManager.instance.Get_MonsterManager().GetSpawnDataManager();
        }
        int tier = GetRandomTier();
        List<WeightKeyPair> keypairList = spawnDataManager.spawnWeightPairs[tier];
        string spawnId = Utils.GetRandomIDToWeight(keypairList);
        List<MonsterSpawnDataBundle> bundles= spawnDataManager.spawnDatas[tier];
        MonsterSpawnDataBundle selected = bundles.First(b => b.id == spawnId);
        if(selected == null)
        {
            Debug.Log("번들 키 잘못됨! ");
            return bundles[0];
        }
        return selected;
    }
}
