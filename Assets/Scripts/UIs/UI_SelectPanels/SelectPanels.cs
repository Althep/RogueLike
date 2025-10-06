using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static Defines;

public class SelectPanels : UI_Base
{

    [SerializeField] GameObject raceSelectPanel;
    [SerializeField] GameObject jobSelectPanel;
    [SerializeField] GameObject weaponSelectPanel;
    [SerializeField] GameObject confirmPanel;

    Defines.Races selectRace = Defines.Races.Elf;
    Defines.Jobs selectJob;
    List<Defines.Jobs> jobList = new List<Defines.Jobs>();
    List<string> startItems;
    string selectItem;
    UIManager uiManager;

    Dictionary<Type, Type> subCellBind = new Dictionary<Type, Type>()
    {
        {typeof(Defines.Jobs),typeof(JobSelectCell) },
        {typeof(Defines.Races),typeof(RaceSelectCell) },
        {typeof(Defines.WeaponType),typeof(WeaponSelectCell) }
    };

    private void Awake()
    {
        
    }
    private void OnEnable()
    {
        SelectSpecies(Defines.Races.Elf);
    }

    void Init()
    {
        uiManager = GameManager.instance.Get_UIManager();
        AddUIEvent(confirmPanel, StartButton, Defines.UIEvents.Click);
    }

    void Set_My_Selects<T>(T[] array, GameObject parents) where T : Enum
    {
        Type enumType = typeof(T);
        //T[] array = Utils.Get_Enums(type);

        if (subCellBind.TryGetValue(enumType, out Type cellType))
        {
            foreach (var value in array)
            {
                GameObject go = uiManager.Get_PoolUI(Defines.UI_PrefabType.MainSelect, parents);
                var cell = (SelectSubCell)go.AddComponent(cellType);

                // Enum �� ����
                cell.SetMyType(value,this);
                cell.AddButtonFunction();
            }
        }
    }

    public void Set_Job(Defines.Jobs job)
    {
        selectJob = job;
    }
    public void Set_Race(Defines.Races race)
    {
        selectRace = race;
    }
    public void Set_Weapon(string value)
    {
        selectItem = value;
    }
    void PanelUpdate()
    {
        
    }

    void StartButton(PointerEventData pev)
    {

    }

    void SelectSpecies(Defines.Races sp)
    {
        selectRace = sp;
        DataManager dm = GameManager.instance.Get_DataManager();
        jobList = dm.racesDataManager.raceJobList[selectRace];
        Jobs[] jobs = jobList.ToArray();

        Set_My_Selects<Jobs>(jobs, jobSelectPanel);
        
    }
    #region ���⼱�ü���
    void SelectJob(Defines.Jobs job)
    {
        startItems.Clear();
        List<string> selects = FilterWeapons(job);
        Type enumType = typeof(Defines.WeaponType);

        foreach(var key in selects)
        {
            GameObject go = uiManager.Get_PoolUI(Defines.UI_PrefabType.MainSelect, weaponSelectPanel);
            WeaponSelectCell subcell = Utils.GetOrAddComponent<WeaponSelectCell>(go);
            subcell.SetMyKey(key);
            subcell.AddButtonFunction();
        }
    }

    List<string> FilterWeapons(Defines.Jobs job)
    {
        selectJob = job;

        DataManager dm = GameManager.instance.Get_DataManager();
        ItemDataManager im = dm.itemDataManager;

        // 1. ������ �ʱ� ���� ������ ����Ʈ ����
        List<string> jobItems = new List<string>(dm.racesDataManager.startItems[selectJob]);
        List<string> selects = new List<string>();
        // 2. ��ü ������ ������ ��������
        Dictionary<string, ItemBase> itemDatas = im.Get_ItemDatas();

        // 3. ���� ���õ� ���� ������ ��������
        RaceData raceData = dm.racesDataManager.raceDatas[selectRace];

        LivingEntity playerEntity = GameManager.instance.Get_PlayerObj().GetComponent<LivingEntity>();

        playerEntity.Set_RaceData(raceData);

        foreach(string key in jobItems)
        {
            if(itemDatas[key] is Weapon)
            {//�����ϰ�� ���ø���Ʈ�� �߰�
                if (!playerEntity.IsRistricted(itemDatas[key]))
                {
                    selects.Add(key);
                }
            }
            else
            {//���Ⱑ �ƴҰ�� �ʱ������ ����Ʈ�� �߰�
                if (!playerEntity.IsRistricted(itemDatas[key]))
                {
                    startItems.Add(key);
                }
            }
        }

        return selects;
        // 6. ���� ������ ����Ʈ ����
        //return startItems = itemKeys;
    }
    #endregion
}
