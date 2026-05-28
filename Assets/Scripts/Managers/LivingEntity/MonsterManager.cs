using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor.EditorTools;

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

    List<MonsterEntity> monsters = new List<MonsterEntity>();

    Dictionary<int, float> tierRates = new Dictionary<int, float>();

    float spawnPoint = 10;

    public static MonsterManager Instance;
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
        if(MonsterManager.Instance == null)
        {
            MonsterManager.Instance = this;
        }
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
            if (asset.name.Contains("StatData"))
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

    public async UniTask OnFloorChange()
    {
        int floor = dungeonManager.Get_Floor();
        tierRates = TierCalculator.GetTierProbabilities(floor, 3, 5, false);
        if (SaveDataManager.instance.IsSaved())
        {
            LoadMonster();
        }
        else
        {
            SpawnPointCalc(floor);
            monsterSpawner.OnFloorChange();
            await MonsterSpawn();
        }
        
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
             List<MonsterEntity> faction = GetMonsters(bundle);
            mapManager.SetMonsterRandomPosition(faction);
            monsters.AddRange(faction);
        }


        await UniTask.Yield();
    }
    List<MonsterEntity> GetMonsters(MonsterSpawnDataBundle spawnData)
    {
        List<MonsterEntity> faction = new();
        foreach(var key in spawnData.spawnDatas.Keys)
        {
            List<MonsterEntity> monster = monsterSpawner.MonsterSpawn(spawnData.spawnDatas[key]);
            faction.AddRange(monster);
        }
        return faction;
    }

    List<MonsterEntity> Get_HeardMonsters(PlayerEntity player)
    {
        Vector2Int playerPos = player.Get_PosKey();
        float sound = player.CalculateContext(Defines.ModifierTriggerType.OnMove)[Defines.StatType.Sound];
        List<MonsterEntity> heardMonsters = monsters
            .Where(m => 
            {
            Vector2Int monsterPos = m.Get_PosKey();
            float dx = Mathf.Abs(monsterPos.x - playerPos.x);
            float dy = Mathf.Abs(monsterPos.y - playerPos.y);
            float maxDist = Mathf.Max(dx, dy);
            return maxDist < sound;
        }).ToList();

        return heardMonsters;
    }
    public void Set_HeardMonstersTarget()
    {
        PlayerEntity player = GameManager.instance.Get_PlayerEntity();
        List<MonsterEntity> hearedMonsters = Get_HeardMonsters(player);
        Vector2Int playerPos = player.Get_PosKey();
        for(int i = 0; i<hearedMonsters.Count; i++)
        {
            hearedMonsters[i].OnHeardTarget(playerPos);
        }
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
        if (!monsters.Contains(go) && go is MonsterEntity monster)
        {
            monsters.Add(monster);
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

    public async UniTask OnTurnEnd(float actPoint)
    {
        int ap = (int)actPoint;
        Set_HeardMonstersTarget();
        for (int i = 0; i <monsters.Count; i++)
        {
            monsters[i].Add_ActPoint(ap);
            await monsters[i].ReadyAction();
            Debug.Log($"Act Point to {monsters[i].gameObject.name}");
        }


    }


    public void RemoveMonster(LivingEntity entity)
    {
        if (!monsters.Contains(entity))
        {
            Debug.Log("Monster Don;t Contained");
            return;
        }
        if(entity is MonsterEntity monster)
        {
            monsters.Remove(monster);
        }
        else
        {
            Debug.Log("Entity is not Monster Entity");
        }
    }
    #region Save&Load
    public List<LivingEntitySaveData> SaveAllMonster()
    {
        List<LivingEntitySaveData> saveDatas = new List<LivingEntitySaveData>();
        for(int i = 0; i<monsters.Count; i++)
        {
            saveDatas.Add(monsters[i].SaveMonster());
        }

        return saveDatas;
    }

    public void LoadMonster()
    {
        List<LivingEntitySaveData> savedData = SaveDataManager.instance.Get_FloorSaveData(DungeonManager.instance.Get_Floor()).monsterDatas;

        if(savedData == null)
        {
            Debug.Log("");
            return;
        }

        for(int i = 0; i<savedData.Count; i++)
        {
            MonsterStat originData = monsterDataManager.GetMonsterStat(savedData[i].id);
            GameObject monster = PoolManager.instance.ObjectPool(Defines.TileType.Monster);
            MonsterEntity monsterEntity = monster.transform.GetComponent<MonsterEntity>();
            Vector2Int pos = new Vector2Int(savedData[i].x, savedData[i].y);
            monster.transform.position = new Vector3(pos.x, pos.y, -1);
            if (monsterEntity != null)
            {
                EntityStat monsterStat = originData.CopyStat();
                monsterEntity.id = savedData[i].id;
                monsterEntity.SetMyStat(monsterStat);
                monsterEntity.GetMyStat().SetBaseStat(Defines.StatType.HP, savedData[i].currentHp);

                if (savedData[i].itemdata!=null)
                {
                    for (int j = 0; j<savedData[i].itemdata.Count; j++)
                    {
                        ItemSaveData itemData = savedData[i].itemdata[j];
                        ItemBase item = ItemManager.instance.LoadItem(itemData);
                        AddItemToMonster(monsterEntity, item);
                    }
                }
                if (savedData[i].equipMentsData!=null)
                {

                    for(int j = 0; j<savedData[i].equipMentsData.Count; j++)
                    {
                        ItemSaveData itemData = savedData[i].equipMentsData[j];
                        ItemBase item = ItemManager.instance.LoadItem(itemData);
                        EquipItemToMonster(monsterEntity, item);
                    }
                }
                if (savedData[i].modifiers!=null)
                {
                    for(int j = 0; j<savedData[i].modifiers.Count; j++)
                    {
                        ModifierSaveData modifierData = savedData[i].modifiers[j];
                        Modifier modi = ModifierManager.instance.Get_Modifier(modifierData.modifierId);
                        monsterEntity.Add_Mutation(modi);
                    }
                }

                if (savedData[i].buffs!=null)
                {
                    for(int j = 0; j<savedData[i].buffs.Count; j++)
                    {
                        BuffSaveData buffData = savedData[i].buffs[j];
                        Modifier modi = ModifierManager.instance.Get_Modifier(buffData.modifierId);
                        int duration = buffData.leftDuration;
                        modi.value = buffData.value;

                        EventManager.instance.AddBuff(monsterEntity, modi, duration);
                    }
                }
            }
            else
            {
                Debug.Log("MonsterEntity null");
            }
            monsterEntity.Set_Visibility(false);
            monsters.Add(monsterEntity);
            mapManager.AddMapData(pos,monsterEntity);
        }
    }

    public void AddItemToMonster(MonsterEntity target , ItemBase item)
    {
        target.Add_Item(item);
    }

    public void EquipItemToMonster(MonsterEntity target, ItemBase equip)
    {

    }
    
    public void ReturnAllObjects()
    {
        for(int i = 0; i<monsters.Count; i++)
        {
            monsters[i].Return();
        }
        monsters.Clear();
    }
    #endregion
}
