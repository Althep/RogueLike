using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Defines;
public class ModifierManager
{
    
    public Dictionary<Races,List<StatModifier>> raceTrait_Stat = new Dictionary<Races,List<StatModifier>>();
    public Dictionary<Races, List<ItemModifier>> raceTrait_Item = new Dictionary<Races, List<ItemModifier>>();
    public Dictionary<Races, List<Jobs>> bandJobDatas = new Dictionary<Races, List<Jobs>>();
    CSVReader reader;
    [SerializeField]string dataPath = "StartingData\\";
    [SerializeField] string jobPath = "BanedJob";
    [SerializeField] string statTraitPath = "RaceTrait_Stat";
    [SerializeField] string ItemTraitPath = "RaceTrait_Item";

    public ModifierFactory modifierFactory;

    public ModifierPooler modifierPooler;

    public StartDataManager startDataManager;
    public ModifierManager()
    {
        Init();
    }

    public void Init()
    {
        ReadDatas();
        if(modifierFactory == null)
        {
            modifierFactory = new ModifierFactory();
            
        }
        if(modifierPooler == null)
        {
            modifierPooler = new ModifierPooler();
            modifierPooler.Set_ModifierManager(this);
        }
        if(startDataManager == null)
        {
            startDataManager = new StartDataManager();
            startDataManager.Init();
        }
    }
    public void SetCSVReader()
    {
        if(reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
    }

    public void ReadDatas()
    {
        SetCSVReader();
        ReadJobDatas();
        ReadStatTrait();
        ReadItemTrait();
    }

    public void ReadJobDatas()
    {
        string path = dataPath + "BanedJob";
        var temp = reader.Read(path);
        for(int i = 0; i < temp.Count; i++)
        {
            string groupKey = temp[i]["groupKey"].ToString();
            string data = temp[i]["value"].ToString();
            Jobs value = Jobs.Default;
            Races race = Races.Default;
            Utils.TryConvertEnum<Races>(temp[i],"groupKey", ref race);
            Utils.TryConvertEnum<Jobs>(temp[i],"value",ref value);
            if(value == Jobs.Default) // 변환 실패 했을 경우
            {
                Debug.Log("JobError");
            }
            if(race == Races.Default)
            {
                Debug.Log("Race Error");
            }
            if (!bandJobDatas.ContainsKey(race))
            {
                bandJobDatas.Add(race, new List<Jobs>());
                bandJobDatas[race].Add(Jobs.Default);
            }
            if (!bandJobDatas[race].Contains(value))
            {
                bandJobDatas[race].Add(value);
            }
        }

    }

    public void ReadStatTrait()
    {
        string path = dataPath + "RaceTrait_Stat";
        var temp = reader.Read(path);
        for(int i = 0; i < temp.Count; i++)
        {
            StatModifier stat = new StatModifier();
            Races race = Races.Default;
            Utils.TryConvertEnum<Races>(temp[i],"Race", ref race);
            if(race == Races.Default)
            {
                Debug.Log($"Race Convert Error {temp[i]["Race"]}");
                return;
            }
            if (!raceTrait_Stat.ContainsKey(race))
            {
                raceTrait_Stat.Add(race, new List<StatModifier>());
            }
            raceTrait_Stat[race].Add(stat);
            stat.id = temp[i]["ID"].ToString();
            Utils.TryConvertEnum<ModifierTriggerType>(temp[i], "ModifierTrigger", ref stat.triggerType);
            Utils.TryConvertEnum<StatType>(temp[i],"StatType", ref stat.stat);
            Utils.TrySetValue<int>(temp[i], "Priority",ref stat.priority);
            Utils.TrySetValue<bool>(temp[i], "isMulti", ref stat.isMulti);
            Utils.TrySetValue<float>(temp[i], "Value", ref stat.value);
        }
    }

    public void ReadItemTrait()
    {
        string path = dataPath + "RaceTrait_Item";
        var temp = reader.Read(path);
        for(int i = 0; i < temp.Count; i++)
        {
            ItemModifier im = new ItemModifier();
            Races race = Races.Default;
            Utils.TryConvertEnum<Races>(temp[i], "Race", ref race);
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
            Utils.TrySetValue<string>(temp[i],"ID",ref im.id);
            Utils.TrySetValue<bool>(temp[i],"isMulti", ref im.isMulti);
            Utils.TrySetValue<float>(temp[i],"Value", ref im.value);
            Utils.TrySetValue<bool>(temp[i],"UnEquipable", ref im.unEquipable);
            Utils.TrySetValue<int>(temp[i],"Priority", ref im.priority);
            Utils.TryConvertEnum<StatType>(temp[i],"StatType", ref im.stat);
            Utils.TryConvertEnum<ItemTargetType>(temp[i],"ItemTargetType", ref im.itemTargetType);
            Utils.TryConvertEnum<ModifierTriggerType>(temp[i],"ModifierTrigger", ref im.triggerType);
            Utils.TryConvertEnum<ItemCategory>(temp[i],"ItemCategory", ref im.itemCategory);
            im.specificType = Utils.Get_ItemSpecificType(temp[i]["SpecificType"].ToString());
            if(im.specificType == null)
            {
                Debug.Log($"Scpecific Type Error! {temp[i]["SpecificType"].ToString()}");
            }
        }
    }

}