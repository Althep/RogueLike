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
        //throw new NotImplementedException();
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
            foreach (var key in originData[i].Keys)
            {
                // 1. 현재 어떤 Key를 검사하고 있는지 무조건 출력해봅니다.
                Debug.Log($"검사 중인 데이터 Key : [{key}]");

                if (key == "Name" || key == "MonsterID" || key == "Tier")
                {
                    continue;
                }

                if (Enum.TryParse<StatType>(key, out StatType stat))
                {
                    float value = 0;
                    Utils.TrySetValue(originData[i], key, ref value);
                    /*
                    if (monsterStat.GetBase().ContainsKey(stat))
                    {
                        // 2. 빈 로그 대신 명확한 이유를 출력하도록 수정합니다.
                        Debug.Log($"[중복 스킵] 몬스터 ID: {id}, 이미 {stat} 키가 존재합니다. 값 할당을 건너뜁니다.");
                        continue;

                        // 주의: 만약 0으로 초기화된 값을 실제 데이터로 '덮어씌워야' 한다면, 
                        // 이 if문과 continue를 지우고 값을 업데이트하도록 로직을 수정해야 합니다!
                    }
                    */
                    monsterStat.SetBaseStat(stat, value);
                    Debug.Log($"[성공] 몬스터 ID : {id} 몬스터 스탯 : {stat} 값 : {value} 등록 완료");
                }
                else
                {
                    Debug.Log($"[변환 실패] Can't Convert Key : {key} Move To Next");
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
