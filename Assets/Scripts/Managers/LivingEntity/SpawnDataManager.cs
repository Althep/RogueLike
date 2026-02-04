using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using static Defines;
[System.Serializable]
public struct MonsterSpawnData
{
    public string monsterID;
    public int minCount;
    public int maxCount;
}
public class MonsterSpawnDataBundle
{
    public string id;
    public Dictionary<string, MonsterSpawnData> spawnDatas = new Dictionary<string, MonsterSpawnData>();
    //public List<string> monsterID = new List<string>();
    public int tier;
    public float weight;
    public float spawnCost;
    public SpawnType spawnType;
}

public class SpawnDataManager : AsyncDataManager<SpawnDataManager>
{
    public SpawnDataManager spawnDataManager;

    public Dictionary<int, List<MonsterSpawnDataBundle>> spawnDatas = new Dictionary<int, List<MonsterSpawnDataBundle>>();
    public Dictionary<int, List<WeightKeyPair>> spawnWeightPairs = new Dictionary<int, List<WeightKeyPair>>();

    public override async UniTask Init()
    {
        await UniTask.Yield();
    }

    protected SpawnDataManager() { }
    
    public async UniTask SetUp(TextAsset myAsset)
    {
        List<Dictionary<string, object>> originData = Utils.TextAssetParse(myAsset);
        await SetSpawnData(originData);

        foreach(var tier in spawnDatas.Keys)
        {
            List<MonsterSpawnDataBundle> bundles = spawnDatas[tier];

            foreach(var bundle in bundles)
            {
                Debug.Log($" 스폰 데이터 번들 아이디 : {bundle.id}");
                foreach(var name in bundle.spawnDatas.Keys)
                {
                    Debug.Log($"스폰 데이터 몬스터 ID {name}");
                }
            }


        }
    }


    public async UniTask SetSpawnData(List<Dictionary<string,object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            Dictionary<string, object> temp = originData[i];
            int tier = 1;
            string id = null;
            string monsterID = null;
            Utils.TrySetValue(temp, "Tier",ref tier);
            Utils.TrySetValue(temp, "ID", ref id);
            Utils.TrySetValue(temp, "MonsterID", ref monsterID);
            if(id == null)
            {
                Debug.Log($"Skpping Spawn data At Index {i} due to invalid");
                continue;
            }
            if (!spawnDatas.ContainsKey(tier))
            {
                spawnDatas.Add(tier, new List<MonsterSpawnDataBundle>());
            }
            MonsterSpawnDataBundle spData = spawnDatas[tier].FirstOrDefault(x=> x.spawnDatas.Keys.Contains(id));
            if(spData != null)
            {
                spData.id = id;
                spData.tier = tier;
                string newID = null;
                Utils.TrySetValue(temp, "MonsterID", ref newID);
                if(!string.IsNullOrEmpty(newID))
                {
                    MonsterSpawnData spawnData = new MonsterSpawnData();
                    spawnData.monsterID = newID;
                    Utils.TrySetValue(temp, "MinCount",ref spawnData.minCount);
                    Utils.TrySetValue(temp, "MaxCount", ref spawnData.maxCount);
                    float spawnCost = 0;
                    float weight = 0;
                    Utils.TrySetValue(temp, "SpawnCost", ref spawnCost);
                    Utils.TrySetValue(temp, "Weight", ref weight);
                    spData.spawnCost += spawnCost;
                    spData.weight += weight;
                    spData.spawnDatas.Add(newID,spawnData);
                }
                else
                {
                    Debug.Log("NewId is Empty or Null");
                }
            }
            else
            {
                MonsterSpawnDataBundle bundle = new MonsterSpawnDataBundle();
                MonsterSpawnData newData = new MonsterSpawnData();
                bundle.id = id;
                bundle.tier = tier;
                Utils.TrySetValue(temp, "SpawnCost",ref bundle.spawnCost);
                Utils.TrySetValue(temp, "MinCount",ref newData.minCount);
                Utils.TrySetValue(temp, "MaxCount", ref newData.maxCount);
                Utils.TrySetValue(temp, "MonsterID", ref monsterID);
                newData.monsterID = monsterID;
                if(monsterID == null)
                {
                    Debug.Log($"Error! Monster ID is Null");
                    continue;
                }
                Utils.TrySetValue(temp,"Weight", ref bundle.weight);
                Utils.TryConvertEnum(temp, "GroupType", ref bundle.spawnType);
                bundle.spawnDatas.Add(monsterID, newData);
                spawnDatas[tier].Add(bundle);
            }
            if (i % 20 == 0)
            {
                await UniTask.Yield();
            }
        }
        SortSpawnDatas();
    }

    public void SortSpawnDatas()
    {
        foreach(var tier in spawnDatas.Keys)
        {
            List<MonsterSpawnDataBundle> bundleList = spawnDatas[tier];

            for(int i = 0; i<bundleList.Count; i++)
            {
                MonsterSpawnDataBundle bundle = bundleList[i];

                if (!spawnWeightPairs.ContainsKey(tier))
                {
                    spawnWeightPairs.Add(tier, new List<WeightKeyPair>());
                }

                bool exists = spawnWeightPairs[tier].Any(x => x.key == bundle.id);
                if (!exists)
                {
                    WeightKeyPair newPair = new WeightKeyPair();
                    newPair.key = bundle.id;
                    newPair.cumulativeWeight = bundle.weight;
                    spawnWeightPairs[tier].Add(newPair);
                }
            }
        }
    }

    public MonsterSpawnDataBundle GetRandomBundle(int tier)
    {
        string key = Utils.GetRandomIDToWeight(spawnWeightPairs[tier]);
        MonsterSpawnDataBundle bundle = spawnDatas[tier].FirstOrDefault(x => x.id == key);
        if(bundle.id == null)
        {
            Debug.Log("몬스터 스폰 번들을 찾지 못함! SapwnDataManager , GetRandomBundle");
            return null;
        }
        return bundle;
    }

    
}
