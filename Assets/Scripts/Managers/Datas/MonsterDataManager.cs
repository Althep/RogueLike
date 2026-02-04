using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using static Defines;
public class MonsterDataManager : AsyncDataManager<MonsterDataManager>
{
    public Dictionary<string, MonsterStat> monsterStats = new Dictionary<string, MonsterStat>();
    
    public Dictionary<int, List<WeightKeyPair>> monsterKeyPairs = new Dictionary<int, List<WeightKeyPair>>();

    [SerializeField]string DataName = "";

    MonsterDataManager(){}

    public override async UniTask Init()
    {
        
    }

    public async UniTask SetUp(TextAsset myAsset)
    {
        List<Dictionary<string, object>> originData = Utils.TextAssetParse(myAsset);
        await SetMonsterData(originData);
    }
    public async UniTask SetMonsterData(List<Dictionary<string,object>> originData)
    {
        for(int i = 0; i< originData.Count; i++)
        {
            MonsterStat monsterStat = new MonsterStat();
            string name = null;
            string id = null;
            Utils.TrySetValue(originData[i], "Name", ref name);
            Utils.TrySetValue(originData[i], "MonsterID", ref id);
            monsterStat.SetID(id);
            if(id == null)
            {
                Debug.Log("몬스터 id 입력 오류 Null");
            }
            monsterStat.SetName(name);
            foreach(var key in originData[i].Keys)
            {
                if (key == "Name" || key == "MonsterID" || key == "Tier" /* 필요한 예외 키 */)
                {
                    continue;
                }
                if (Enum.TryParse<StatType>(key, out StatType stat))
                {
                    float value = 0;
                    Utils.TrySetValue(originData[i], key, ref value);
                    if (monsterStat.GetBase().ContainsKey(stat))
                    {
                        Debug.Log("");
                        continue;
                    }
                    monsterStat.SetBaseStat(stat, value);
                }
                else
                {
                    Debug.Log($"Can Convert Key : {key} Move To Next");
                    continue;
                }
            }
            if (monsterStats.ContainsKey(monsterStat.GetId()))
            {
                Debug.Log($" 몬스터 ID가 중복되었습니다: {monsterStat.GetId()}");
                continue;
            }

            if (i % 20 == 0)
            {
                await UniTask.Yield();
            }
            monsterStat.SetTileType(TileType.Monster);
            monsterStats.Add(monsterStat.GetId(), monsterStat);
            Debug.Log($" 몬스터 ID : {monsterStat.GetId()} 등록 완료");
        }
        
    }

    public MonsterStat GetMonsterStat(string key)
    {
        return monsterStats[key];
    }

    public MonsterStat GetCopyMonsterStat (MonsterEntity entity,string id)
    {
        MonsterStat origin = monsterStats[id];
        MonsterStat copied = origin.CopyMonsterStat();
        entity.SetMyStat(copied);
        return copied;

    }

    
}
