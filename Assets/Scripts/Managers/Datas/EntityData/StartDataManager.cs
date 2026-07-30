using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using static Defines;

public class JobStartData
{
    public Jobs job;
    public List<string> optionKeys = new();
    
    public void SetData(Dictionary<string,object> originData)
    {
        string newKey = null;
        Utils.TryConvertEnum(originData, "Job", ref job);
        if(Utils.TrySetValue(originData, "OptionKey", ref newKey))
        {
            if(!optionKeys.Contains(newKey))
            {
                optionKeys.Add(newKey);
            }
        }
    }
}

public class RaceModData
{
    public Races race;
    public List<string> optionKeys = new();

    public void SetData(Dictionary<string,object> origindata)
    {
        string newKey = null;
        Utils.TryConvertEnum(origindata, "Race",ref race);
        if(Utils.TrySetValue(origindata, "OptionKey", ref newKey))
        {
            if(!optionKeys.Contains(newKey))
            {
                optionKeys.Add(newKey);
            }
        }
    }
}

public class RaceBanedJobData
{
    public Races race;
    public List<Jobs> bandJobs = new();
    public void SetData(Dictionary<string,object> originData)
    {
        Jobs job = Jobs.Default;
        Utils.TryConvertEnum(originData, "Race", ref race);
        if(Utils.TryConvertEnum(originData, "BanedJob", ref job))
        {
            if(job != Jobs.Default && !bandJobs.Contains(job))
            {
                bandJobs.Add(job);
            }
        }
    }
}

public class StartItemData
{
    public Jobs job;
    public Dictionary<SlotType, List<string>> slotItems = new();

    public void SetData(Dictionary<string,object> originData)
    {
        string itemKey = null;
        Utils.TryConvertEnum(originData, "Job", ref job);
        SlotType slot = SlotType.Default;
        Utils.TryConvertEnum(originData, "Slot", ref slot);
        
        if(slot == SlotType.Default)
        {
            Debug.Log("Slot Type Error");
            return;
        }

        if(Utils.TrySetValue(originData, "Item", ref itemKey))
        {
            if (!slotItems.ContainsKey(slot))
            {
                slotItems.Add(slot, new List<string>());
            }
            if(!slotItems[slot].Contains(itemKey))
            {
                slotItems[slot].Add(itemKey);
            }
        }
    }
}

public class StartDataManager : AsyncDataManager<StartDataManager>
{
    public Dictionary<Races, RaceModData> raceModDatas = new();
    public Dictionary<Jobs, JobStartData> jobStartDatas = new();
    public Dictionary<Races, RaceBanedJobData> bandJobDatas = new();
    public Dictionary<Jobs, StartItemData> startItemDatas = new();

    // 동적으로 생성되는 종족별 선택 가능 직업 리스트
    public Dictionary<Races, List<Jobs>> raceJobList = new();

    public override async UniTask Init()
    {
        // 초기화 시 필요한 로직
    }

    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        Debug.Log($"StartDataManager 셋업 시작! 파일 카운트 : {myAssets.Count}");
        foreach (var asset in myAssets)
        {
            List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);
            Debug.Log($"{asset.name} 파일 파싱 중...");

            if (asset.name.Contains("RaceTrait_Item") || asset.name.Contains("RaceTrait_Stat"))
            {
                await Read_RaceModData(originData);
            }
            else if (asset.name.Contains("BanedJob"))
            {
                await Read_BandJob(originData);
            }
            else if (asset.name.Contains("JobStat"))
            {
                await Read_JobStartData(originData);
            }
            else if (asset.name.Contains("StartItems"))
            {
                await Read_StartItems(originData);
            }
        }
        
        // 데이터 파싱 완료 후 종족별 허용 직업 리스트 생성
        BuildRaceJobList();
        
        Debug.Log("StartDataManager 셋업 완료!");
    }

    public async UniTask Read_RaceModData(List<Dictionary<string,object>> originData)
    {
        for(int i = 0; i < originData.Count; i++)
        {
            Races race = Races.Default;
            if(!Utils.TryConvertEnum(originData[i], "Race", ref race) || race == Races.Default) continue;

            if (!raceModDatas.ContainsKey(race))
            {
                raceModDatas[race] = new RaceModData();
                raceModDatas[race].race = race;
            }
            raceModDatas[race].SetData(originData[i]);
            await Utils.WaitYield(i);
        }
    }

    public async UniTask Read_JobStartData(List<Dictionary<string,object>> originData)
    {
        for(int i = 0; i < originData.Count; i++)
        {
            Jobs job = Jobs.Default;
            if(!Utils.TryConvertEnum(originData[i], "Job", ref job) || job == Jobs.Default) continue;

            if (!jobStartDatas.ContainsKey(job))
            {
                jobStartDatas[job] = new JobStartData();
                jobStartDatas[job].job = job;
            }
            jobStartDatas[job].SetData(originData[i]);
            await Utils.WaitYield(i);
        }
    }

    public async UniTask Read_BandJob(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            // 주의: 기존 코드를 보니 밴 직업의 헤더가 groupKey와 value로 되어 있었습니다.
            // CSV 포맷이 맞춰졌다고 가정하고 호환성 처리
            Races race = Races.Default;
            if(originData[i].ContainsKey("groupKey"))
                Utils.TryConvertEnum(originData[i], "groupKey", ref race);
            else
                Utils.TryConvertEnum(originData[i], "Race", ref race);
                
            if (race == Races.Default) continue;

            if (!bandJobDatas.ContainsKey(race))
            {
                bandJobDatas.Add(race, new RaceBanedJobData());
                bandJobDatas[race].race = race;
            }

            Jobs bannedJob = Jobs.Default;
            if(originData[i].ContainsKey("value"))
                Utils.TryConvertEnum(originData[i], "value", ref bannedJob);
            else
                Utils.TryConvertEnum(originData[i], "BanedJob", ref bannedJob);

            if (bannedJob != Jobs.Default && !bandJobDatas[race].bandJobs.Contains(bannedJob))
            {
                bandJobDatas[race].bandJobs.Add(bannedJob);
            }

            await Utils.WaitYield(i);
        }
    }

    public async UniTask Read_StartItems(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            Jobs job = Jobs.Default;
            if(!Utils.TryConvertEnum(originData[i], "Job", ref job) || job == Jobs.Default) continue;

            if (!startItemDatas.ContainsKey(job))
            {
                startItemDatas.Add(job, new StartItemData());
                startItemDatas[job].job = job;
            }
            startItemDatas[job].SetData(originData[i]);

            await Utils.WaitYield(i);
        }
    }

    private void BuildRaceJobList()
    {
        Jobs[] allJobs = (Jobs[])Utils.Get_Enums<Jobs>();
        Races[] allRaces = (Races[])Utils.Get_Enums<Races>();

        raceJobList.Clear();

        foreach (var race in allRaces)
        {
            if (race == Races.Default) continue;

            List<Jobs> bannedJobs = bandJobDatas.ContainsKey(race) ? bandJobDatas[race].bandJobs : new List<Jobs>();
            var bannedSet = new HashSet<Jobs>(bannedJobs);

            raceJobList[race] = allJobs.Where(job => job != Jobs.Default && !bannedSet.Contains(job)).ToList();
        }
    }
}
