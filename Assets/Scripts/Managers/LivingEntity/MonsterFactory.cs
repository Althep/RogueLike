using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public class MonsterFactory 
{
    MonsterDataManager monsterDatamanager;
    MonsterManager monsterManager;
    PoolManager poolManager;

    
    public void Init()
    {
        if (poolManager == null)
        {
            poolManager = GameManager.instance.Get_PoolManager();
        }
        if (monsterManager == null)
        {
            monsterManager = GameManager.instance.Get_MonsterManager();
        }
        if(monsterDatamanager == null)
        {
            monsterDatamanager = GameManager.instance.Get_DataManager().GetMonsterDataManager();
        }
    }
    public MonsterStat GetMonsterStat(string key)
    {
        if(monsterDatamanager == null)
        {
            monsterDatamanager = GameManager.instance.Get_DataManager().monsterDataManager;
        }
        return monsterDatamanager.monsterStats[key];
    }


    public void GetMonsters(MonsterSpawnDataBundle monsterBundle)
    {
        if(monsterBundle.id == null || monsterBundle == null)
        {
            Debug.Log("몬스터 번들 잘못됨");
            return;
        }
        Dictionary<string, MonsterSpawnData> spawnBundle = monsterBundle.spawnDatas;
        foreach(string id in spawnBundle.Keys)
        {
            int monsterCount = GetMonsterCount(spawnBundle[id]);
            for(int i = 0; i < monsterCount; i++)
            {
                GetMonster(spawnBundle[id].monsterID);
            }
        }
    }

    public int GetMonsterCount(MonsterSpawnData data)
    {
        int randomCount = UnityEngine.Random.Range(data.minCount, data.maxCount + 1);
        return randomCount;
    }

    public GameObject GetMonster(string id)
    {
        Init();
        GameObject monsterObj = poolManager.ObjectPool(Defines.TileType.Monster);
        MonsterEntity entity = monsterObj.transform.GetComponent<MonsterEntity>();
        monsterManager.AddToMonsterList(entity);
        entity.ResetData();
        monsterDatamanager.GetCopyMonsterStat(entity, id);
        return monsterObj;
    }
}
