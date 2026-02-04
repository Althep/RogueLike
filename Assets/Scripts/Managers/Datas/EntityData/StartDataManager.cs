using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using static Defines;

public struct StartData
{
    public SlotType slot;
    public List<string> names;


}

public class StartDataManager : AsyncDataManager<StartDataManager>
{
    public Dictionary<Races, RaceData> raceDatas = new Dictionary<Races, RaceData>();

    public Dictionary<Races, List<Defines.Jobs>> raceJobList = new Dictionary<Defines.Races, List<Defines.Jobs>>();

    public Dictionary<Jobs, Dictionary<SlotType, StartData>> startDatas = new Dictionary<Jobs, Dictionary<SlotType, StartData>>();

    public Dictionary<Races, List<StatModifier>> raceTrait_Stat = new();
    public Dictionary<Jobs, List<StatModifier>> jobTrait_Stat = new();
    public Dictionary<Races, List<ItemModifier>> raceTrait_Item = new();
    public Dictionary<Races, List<Jobs>> bandJobDatas = new();
    
    CSVReader reader;
    DataManager dataManager;

    ModifierManager modifierManager;

    ModifierPooler modifierPooler;
    StartDataManager()
    {

    }

    public async UniTask SetUp(List<TextAsset> myAssets)
    {
        Debug.Log($"스타트 데이터 매니저 셋업! 에셋 카운트 : {myAssets.Count}");
        foreach (var asset in myAssets)
        {
            List<Dictionary<string, object>> originData = Utils.TextAssetParse(asset);
            Debug.Log($"{asset.name} 데이터 리드!");
            if (asset.name.Contains("RaceTrait_Item"))
            {
                await ReadRaceData_Item(originData);
                Debug.Log("종족 별 아이템 데이터 리드!");
            }
            else if (asset.name.Contains("BanedJob"))
            {
                await ReadBandJob(originData);
                Debug.Log("밴데이터 리드!");
            }
            else if (asset.name.Contains("JobStat"))
            {
                await ReadJobStats(originData);
                Debug.Log("직업 스탯 데이터 리드!");
            }
            else if (asset.name.Contains("RaceTrait_Stat"))
            {
                await ReadRaceData_Stat(originData);
                Debug.Log("종족 별 스탯 데이터 리드!");
            }
            else if (asset.name.Contains("StartItems"))
            {
                await ReadStartItems(originData);
                Debug.Log("시작 아이템 데이터 리드!");
            }
        }
        Debug.Log("StartSetUp Done!");
    }
    public override async UniTask Init()
    {
        if (dataManager == null)
        {
            dataManager = GameManager.instance.Get_DataManager();
        }
        if (reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
        if(modifierPooler == null)
        {
            modifierPooler = GameManager.instance.Get_ModifierManager().GetModifierPooler();
        }
    }

    public async UniTask ReadRaceData_Stat(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            Races race = Races.Default;
            if (Utils.StringToEnum<Races>(originData[i]["Race"].ToString(), ref race))
            {
                if (race == Races.Default)
                {
                    Debug.Log("Race Convert Error!!");
                    continue;
                }
            }
            if (!raceDatas.ContainsKey(race))
            {
                raceDatas.Add(race, new RaceData());
            }
            RaceData racedata = raceDatas[race];
            string id = originData[i]["ID"].ToString();
            if (!racedata.modifiers.Any(m => m.id == id))
            {
                StatModifier modi = new StatModifier();
                modi.id = id;
                Utils.TryConvertEnum<ModifierTriggerType>(originData[i], "ModifierTigger", ref modi.triggerType);
                Utils.TryConvertEnum<StatType>(originData[i], "StatType", ref modi.stat);
                Utils.TrySetValue<int>(originData[i], "Priority", ref modi.priority);
                Utils.TrySetValue<bool>(originData[i], "isMuti", ref modi.isMulti);
                Utils.TrySetValue<float>(originData[i], "Value", ref modi.value);
                racedata.modifiers.Add(modi);
            }
            await Utils.WaitYield(i);
        }

    }
    public async UniTask ReadRaceData_Item(List<Dictionary<string, object>> originData)
    {

        for (int i = 0; i < originData.Count; i++)
        {
            Races race = Races.Default;
            if (Utils.StringToEnum<Races>(originData[i]["Race"].ToString(), ref race))
            {
                if (race == Races.Default)
                {
                    Debug.Log("Race Convert Error!!");
                    continue;
                }
            }
            if (!raceDatas.ContainsKey(race))
            {
                raceDatas.Add(race, new RaceData());
            }
            RaceData racedata = raceDatas[race];
            string id = originData[i]["ID"].ToString();
            if (!racedata.modifiers.Any(m => m.id == id))
            {
                ItemModifier modi = new ItemModifier();
                modi.id = id;
                Utils.TryConvertEnum<ModifierTriggerType>(originData[i], "ModifierTigger", ref modi.triggerType);
                Utils.TryConvertEnum<StatType>(originData[i], "StatType", ref modi.stat);
                Utils.TrySetValue<int>(originData[i], "Priority", ref modi.priority);
                Utils.TrySetValue<bool>(originData[i], "isMuti", ref modi.isMulti);
                Utils.TrySetValue<float>(originData[i], "Value", ref modi.value);
                Utils.TryConvertEnum<ItemTargetType>(originData[i], "ItemTargetType", ref modi.itemTargetType);
                Utils.TryConvertEnum<ItemCategory>(originData[i], "ItemCategory", ref modi.itemCategory);
                Utils.TrySetValue<bool>(originData[i], "UnEquipable", ref modi.unEquipable);
                Enum specificType = Utils.Get_ItemSpecificType(originData[i]["SpecificType"].ToString());
                modi.specificType = specificType;
                //Utils.TryConvertEnum<> string=> 구체적 타입 변경 필요
                racedata.modifiers.Add(modi);
            }

            await Utils.WaitYield(i);
        }
    }
    public async UniTask ReadBandJob(List<Dictionary<string, object>> originData)
    {
        Debug.Log("ReadBandJob");
        Jobs[] jobs = (Jobs[])Utils.Get_Enums<Jobs>(Jobs.Default);
        Races[] races = (Races[])Utils.Get_Enums<Races>(Races.Default);
        for (int i = 0; i < originData.Count; i++)
        {
            Races race = Utils.ConvertToEnum<Races>(originData[i]["groupKey"].ToString());
            if (!bandJobDatas.ContainsKey(race))
            {
                bandJobDatas.Add(race, new List<Jobs>());
            }
            Jobs job = Utils.ConvertToEnum<Jobs>(originData[i]["value"].ToString());
            if(job != Jobs.Default)
            {
                bandJobDatas[race].Add(job);
            }
        }
        foreach(var race in bandJobDatas.Keys)
        {
            List<Jobs> bannedJobs = bandJobDatas[race];
            var bannedSet = new HashSet<Jobs>(bannedJobs);
            raceJobList[race] = jobs.Where(job => !bannedSet.Contains(job)).ToList();
        }
    }


    public async UniTask ReadStartItems(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            Jobs job = Jobs.Assassin;
            string item = null;
            SlotType slot = SlotType.MainHand;
            if (Utils.StringToEnum<Jobs>(originData[i]["Job"].ToString(), ref job))
            {
                if (!startDatas.ContainsKey(job))
                {
                    startDatas.Add(job, new Dictionary<SlotType, StartData>());
                }
            }
            else
            {
                Debug.Log("Job Convert Error");
                continue;
            }
            if (Utils.StringToEnum<SlotType>(originData[i]["Slot"].ToString(), ref slot))
            {
                if (!startDatas[job].ContainsKey(slot))
                {
                    StartData data = new StartData();
                    data.names = new List<string>();
                    data.slot = slot;
                    startDatas[job].Add(slot, data);
                }
            }
            else
            {
                Debug.Log($"Slot Convert Error ID : {originData[i]["Item"].ToString()} {originData[i]["Slot"].ToString()}");
                continue;
            }
            if (Utils.TrySetValue<string>(originData[i], "Item", ref item))
            {

                if (!startDatas[job][slot].names.Contains(item))
                {
                    startDatas[job][slot].names.Add(item);
                }

            }
            else
            {
                Debug.Log("Item Convert Error");
                continue;
            }

            await Utils.WaitYield(i);
        }
    }
    public async UniTask ReadJobStats(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            Jobs job = Utils.ConvertToEnum<Jobs>( originData[i]["Job"].ToString());
            string id = originData[i]["ID"].ToString();
            Debug.Log($"스탯 데이터 매니저 직업 스탯 리딩중 ID : {id}");
            StatModifier stat = (StatModifier)modifierPooler.GetModifier(ModifierType.StatModifier, id);
            if (!jobTrait_Stat.ContainsKey(job))
            {
                jobTrait_Stat.Add(job, new List<StatModifier>());
            }
            jobTrait_Stat[job].Add(stat);
            await Utils.WaitYield(i);
        }

    }

    public async UniTask ReadStatTrait(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            StatModifier stat = new StatModifier();
            Races race = Races.Default;
            Utils.TryConvertEnum<Races>(originData[i], "Race", ref race);
            if (race == Races.Default)
            {
                Debug.Log($"Race Convert Error {originData[i]["Race"]}");
                return;
            }
            if (!raceTrait_Stat.ContainsKey(race))
            {
                raceTrait_Stat.Add(race, new List<StatModifier>());
            }
            raceTrait_Stat[race].Add(stat);
            stat.id = originData[i]["ID"].ToString();
            Utils.TryConvertEnum<ModifierTriggerType>(originData[i], "ModifierTrigger", ref stat.triggerType);
            Utils.TryConvertEnum<StatType>(originData[i], "StatType", ref stat.stat);
            Utils.TrySetValue<int>(originData[i], "Priority", ref stat.priority);
            Utils.TrySetValue<bool>(originData[i], "isMulti", ref stat.isMulti);
            Utils.TrySetValue<float>(originData[i], "Value", ref stat.value);

            await Utils.WaitYield(i);
        }
    }

    public async UniTask ReadItemTrait(List<Dictionary<string, object>> originData)
    {
        for (int i = 0; i < originData.Count; i++)
        {
            ItemModifier im = new ItemModifier();
            Races race = Races.Default;
            Utils.TryConvertEnum<Races>(originData[i], "Race", ref race);
            if (race == Races.Default)
            {
                Debug.Log("Race Convert Error");
                return;
            }
            if (!raceTrait_Item.ContainsKey(race))
            {
                raceTrait_Item.Add(race, new List<ItemModifier>());
            }
            raceTrait_Item[race].Add(im);
            Utils.TrySetValue<string>(originData[i], "ID", ref im.id);
            Utils.TrySetValue<bool>(originData[i], "isMulti", ref im.isMulti);
            Utils.TrySetValue<float>(originData[i], "Value", ref im.value);
            Utils.TrySetValue<bool>(originData[i], "UnEquipable", ref im.unEquipable);
            Utils.TrySetValue<int>(originData[i], "Priority", ref im.priority);
            Utils.TryConvertEnum<StatType>(originData[i], "StatType", ref im.stat);
            Utils.TryConvertEnum<ItemTargetType>(originData[i], "ItemTargetType", ref im.itemTargetType);
            Utils.TryConvertEnum<ModifierTriggerType>(originData[i], "ModifierTrigger", ref im.triggerType);
            Utils.TryConvertEnum<ItemCategory>(originData[i], "ItemCategory", ref im.itemCategory);
            im.specificType = Utils.Get_ItemSpecificType(originData[i]["SpecificType"].ToString());
            if (im.specificType == null)
            {
                Debug.Log($"Scpecific Type Error! {originData[i]["SpecificType"].ToString()}");
            }

            await Utils.WaitYield(i);
        }
    }

}
