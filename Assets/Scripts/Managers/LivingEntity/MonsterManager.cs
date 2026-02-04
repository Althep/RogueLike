using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
[System.Serializable]
public class MonsterManager : MonoBehaviour
{
    [SerializeField] MonsterSpawner monsterSpawner;
    [SerializeField] MapManager mapManager;
    [SerializeField] DataManager dataManager;
    [SerializeField] SpawnDataManager spawnDataManager;
    [SerializeField] MonsterDataManager monsterDataManager;
    [SerializeField] MonsterFactory monsterFactory;
    [SerializeField] DungeonManager dungeonManager;

    List<MapEntity> monsters = new List<MapEntity>();

    Dictionary<int, float> tierRates = new Dictionary<int, float>();
    float spawnPoint = 10;
    private async UniTask Awake()
    {
        if (monsterSpawner == null)
        {
            TierCalculator tier = GameManager.instance.GetDungeonManager().GetTierCalculator();
            monsterSpawner = new MonsterSpawner(tier, monsterFactory);
            monsterSpawner.Init();
        }
        await Init();
        //monsterFactory.Init();
    }

    public async UniTask Init()
    {
        if(mapManager == null)
        {
            mapManager = GameManager.instance.Get_MapManager();
        }
        if(dungeonManager == null)
        {
            dungeonManager = GameManager.instance.GetDungeonManager();
        }
        if(monsterDataManager == null)
        {
            monsterDataManager = await MonsterDataManager.CreateAsync();
        }
        if(spawnDataManager == null)
        {
            spawnDataManager = await SpawnDataManager.CreateAsync();
        }
        if(monsterFactory == null)
        {
            monsterFactory = new MonsterFactory();
        }
        if(monsterSpawner == null)
        {
            TierCalculator tier = GameManager.instance.GetDungeonManager().GetTierCalculator() ;
            monsterSpawner = new MonsterSpawner(tier,monsterFactory);
            monsterSpawner.Init();
        }
        if(dataManager == null)
        {
            dataManager = GameManager.instance.Get_DataManager();
        }
        monsterFactory.Init();
    }
    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        
        if(spawnDataManager == null)
        {
            spawnDataManager = await SpawnDataManager.CreateAsync();
        }
        if(monsterDataManager == null)
        {
            monsterDataManager = await MonsterDataManager.CreateAsync();
        }
        foreach(var asset in myAssets)
        {
            if (asset.name.Contains("Stat"))
            {
                Debug.Log("몬스터 스탯 데이터 리드");
                await monsterDataManager.SetUp(asset);
            }
            else if (asset.name.Contains("Spawn"))
            {
                Debug.Log("몬스터 스폰 데이터 리드");
                await spawnDataManager.SetUp(asset);
            }


        }
    }
    public async UniTask MonsterSpawnTest()
    {
        MonsterSpawnDataBundle bundle = monsterSpawner.GetRandoSpawnBundle();

        foreach (string key in bundle.spawnDatas.Keys)
        {
            monsterSpawner.MonsterSpawn(bundle.spawnDatas[key]);
        }
        spawnPoint -= bundle.spawnCost;

        await UniTask.Yield();
    }

    public void OnFloorChange()
    {
        int floor = dungeonManager.floor;
        tierRates = TierCalculator.GetTierProbabilities(floor, 3, 5, false);
        SpawnPointCalc(floor);
        monsterSpawner.OnFloorChange();
    }

    void SpawnPointCalc(int floor)
    {
        spawnPoint = floor * 10;
    }
    public Vector2 GetRandomPos()
    {
        List<Vector2> pos = mapManager.tilePosList.Select(v => (Vector2)v).ToList();

        HashSet<Vector2> occupiedPos = new HashSet<Vector2>(
            monsters.Select(m => (Vector2)m.transform.position)
        );

        List<Vector2> emptyPos = pos.Where(p => !occupiedPos.Contains(p)).ToList();

        if (emptyPos.Count == 0)
        {
            Debug.Log("빈공간 존재하지않음!");
            return Vector2.zero;
        }

        int index = UnityEngine.Random.Range(0, emptyPos.Count);
        return emptyPos[index];
    }

    public async UniTask MonsterSpawn()
    {
        List<MonsterSpawnDataBundle> bundleList = GetMonsterSpawnBundles();

        foreach(var bundle in bundleList)
        {
             List<MapEntity> faction = GetMonsters(bundle);
            mapManager.SetRandomPosition(faction);
            monsters.AddRange(faction);
        }


        await UniTask.Yield();
    }
    List<MapEntity> GetMonsters(MonsterSpawnDataBundle spawnData)
    {
        List<MapEntity> faction = new();
        foreach(var key in spawnData.spawnDatas.Keys)
        {
            List<MapEntity> monster = monsterSpawner.MonsterSpawn(spawnData.spawnDatas[key]);
            faction.AddRange(monster);
        }
        return faction;
    }
    public List<MonsterSpawnDataBundle> GetMonsterSpawnBundles()
    {
        List<MonsterSpawnDataBundle> bundleList = new List<MonsterSpawnDataBundle>();
        int maximumLoof = 40;
        int loofCount = 0;
        if (monsterSpawner == null)
        {
            Debug.Log("몬스터 스포너 널!");
        }
        while (spawnPoint > 0 || loofCount<maximumLoof)
        {
            MonsterSpawnDataBundle bundle = monsterSpawner.GetRandoSpawnBundle();
            
            spawnPoint -= bundle.spawnCost;
            bundleList.Add(bundle);
            loofCount++;
        }
        return bundleList;
    }
    public List<Vector2> GetMonsterPos()
    {
        List<Vector2> monsterPos = monsters.Select(p => (Vector2)p.transform.position).ToList();
        return monsterPos;
    }

    public void AddToMonsterList(MapEntity go)
    {
        if (!monsters.Contains(go))
        {
            monsters.Add(go);
        }
    }
    
    public int GetRandomTier()
    {
        int tier = 1;
        tier = Utils.GetRandomTier(tierRates);
        return tier;
    }

    public void OnFirstVisitFloor()
    {

    }
    #region GetManager
    public SpawnDataManager GetSpawnDataManager()
    {
        return spawnDataManager;
    }
    public MonsterFactory GetMonsterFactory()
    {
        if(monsterFactory == null)
        {
            monsterFactory = new MonsterFactory();
            monsterFactory.Init();
        }
        return monsterFactory;
    }
    #endregion

    
}
