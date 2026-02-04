using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static Defines;
public class ModifierManager : MonoBehaviour
{

    public Dictionary<Races, List<StatModifier>> raceTrait_Stat = new Dictionary<Races, List<StatModifier>>();
    
    //public Dictionary<Races, List<Jobs>> bandJobDatas = new Dictionary<Races, List<Jobs>>();
    CSVReader reader;

    ModifierFactory modifierFactory;
    ModifierPooler modifierPooler;
    public StartDataManager startDataManager;
    public ModifierDataManager modifierDataManager;
    string bandJobDataName = "BanedJob";
    string statRaceDataName = "RaceTrait_Stat";
    string itemRaceDataName = "RaceTrait_Item";

    private ModifierManager()
    {

    }

    private async UniTask Awake()
    {
        await Init();

    }
    public async UniTask Init()
    {

        if (modifierFactory == null)
        {
            modifierFactory = await ModifierFactory.CreateAsync();
            //await modifierFactory.Init();
        }
        if (modifierPooler == null)
        {
            modifierPooler = new ModifierPooler();
            modifierPooler.Set_ModifierManager(this,modifierFactory);
        }
        if(modifierDataManager == null)
        {
            modifierDataManager = await ModifierDataManager.CreateAsync();
        }
        /*
        if(startDataManager == null)
        {
            startDataManager = new StartDataManager();
            startDataManager.Init();
        }*/
    }

    public ModifierFactory GetModifierFactory()
    {
        if (modifierFactory == null)
        {
            // 만약 Init이 끝나지 않았다면 여기서도 null이 반환될 수 있음
            Debug.LogWarning("Factory is not initialized yet!");
        }
        return modifierFactory;
    }
    public ModifierPooler GetModifierPooler()
    {
        if(modifierPooler == null)
        {
            modifierPooler = new ModifierPooler();
            modifierPooler.Set_ModifierManager(this,modifierFactory);
        }
        return modifierPooler;
    }
}