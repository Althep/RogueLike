using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static Defines;

public struct StartData
{
    public SlotType slot;
    public List<string> names;


}

public class StartDataManager
{
    public Dictionary<Races, RaceData> raceDatas = new Dictionary<Races, RaceData>();

    public Dictionary<Races, List<Defines.Jobs>> raceJobList = new Dictionary<Defines.Races, List<Defines.Jobs>>();

    public Dictionary<Jobs, Dictionary<SlotType,StartData>> startDatas = new Dictionary<Jobs, Dictionary<SlotType, StartData>>();

    string dataPath = "StartingData\\";

    CSVReader reader;
    public void Init()
    {
        if (reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
        ReadRaceData_Item();
        ReadRaceData_Stat();
        ReadBandJob();
        ReadStartItems();
        
    }
    public void ReadRaceData_Stat()
    {
        string path = dataPath + "RaceTrait_Stat";
        if(reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
        List<Dictionary<string, object>> temp = reader.Read(path);

        for(int i = 0; i < temp.Count; i++)
        {
            Races race = Races.Default;
            if(Utils.StringToEnum<Races>(temp[i]["Race"].ToString(),ref race))
            {
                if(race == Races.Default)
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
            string id = temp[i]["ID"].ToString();
            if(!racedata.modifiers.Any( m =>m.id == id))
            {
                StatModifier modi = new StatModifier();
                modi.id = id;
                Utils.TryConvertEnum<ModifierTriggerType>(temp[i], "ModifierTigger",ref modi.triggerType);
                Utils.TryConvertEnum<StatType>(temp[i], "StatType", ref modi.stat);
                Utils.TrySetValue<int>(temp[i], "Priority", ref modi.priority);
                Utils.TrySetValue<bool>(temp[i], "isMuti", ref modi.isMulti);
                Utils.TrySetValue<float>(temp[i], "Value", ref modi.value);
                racedata.modifiers.Add(modi);
            }
            
        }

    }
    public void ReadRaceData_Item()
    {
        string path = dataPath + "RaceTrait_Item";
        if (reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
        List<Dictionary<string, object>> temp = reader.Read(path);

        for (int i = 0; i < temp.Count; i++)
        {
            Races race = Races.Default;
            if (Utils.StringToEnum<Races>(temp[i]["Race"].ToString(), ref race))
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
            string id = temp[i]["ID"].ToString();
            if (!racedata.modifiers.Any(m => m.id == id))
            {
                ItemModifier modi = new ItemModifier();
                modi.id = id;
                Utils.TryConvertEnum<ModifierTriggerType>(temp[i], "ModifierTigger", ref modi.triggerType);
                Utils.TryConvertEnum<StatType>(temp[i], "StatType", ref modi.stat);
                Utils.TrySetValue<int>(temp[i], "Priority", ref modi.priority);
                Utils.TrySetValue<bool>(temp[i], "isMuti", ref modi.isMulti);
                Utils.TrySetValue<float>(temp[i], "Value", ref modi.value);
                Utils.TryConvertEnum<ItemTargetType>(temp[i], "ItemTargetType", ref modi.itemTargetType);
                Utils.TryConvertEnum<ItemCategory>(temp[i], "ItemCategory", ref modi.itemCategory);
                Utils.TrySetValue<bool>(temp[i], "UnEquipable", ref modi.unEquipable);
                Enum specificType = Utils.Get_ItemSpecificType(temp[i]["SpecificType"].ToString());
                modi.specificType = specificType;
                //Utils.TryConvertEnum<> string=> 구체적 타입 변경 필요
                racedata.modifiers.Add(modi);
            }

        }
    }
    public void ReadBandJob()
    {
        Jobs[] jobs = (Jobs[])Utils.Get_Enums<Jobs>(Jobs.Default);
        Races[] races = (Races[])Utils.Get_Enums<Races>(Races.Default);
        ModifierManager mm = GameManager.instance.Get_DataManager().modifierManager;
        foreach (Races race in races)
        {
            if (!mm.bandJobDatas.TryGetValue(race, out var bannedJobs))
            {
                bannedJobs = new List<Jobs>();
            }
            var bannedSet = new HashSet<Jobs>(bannedJobs);
            bannedSet.Add(Jobs.Default);
            raceJobList[race] = jobs.Where(job => !bannedSet.Contains(job))
                .ToList();

            foreach(var bandJ in bannedSet)
            {
                Debug.Log($"Race :{race} Baned Job :{bandJ}");
            }
            
        }

    }

    public void ReadStartItems()
    {
        string path = dataPath+ "Start_Items";
        List<Dictionary<string, object>> temp = reader.Read(path);

        for (int i = 0; i < temp.Count; i++)
        {
            Jobs job = Jobs.Assassin;
            string item = null;
            SlotType slot = SlotType.MainHand;
            if (Utils.StringToEnum<Jobs>(temp[i]["Job"].ToString(), ref job) )
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
            if (Utils.StringToEnum<SlotType>(temp[i]["Slot"].ToString(), ref slot))
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
                Debug.Log($"Slot Convert Error ID : {temp[i]["Item"].ToString()} {temp[i]["Slot"].ToString()}");
                continue;
            }
            if (Utils.TrySetValue<string>(temp[i], "Item", ref item))
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
            
        }
    }
}
