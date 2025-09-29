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
    List<string> initItems;

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

        if (subCellBind.TryGetValue(enumType, out Type cellType))
        {
            foreach (var value in array)
            {
                GameObject go = uiManager.Get_PoolUI(Defines.UI_PrefabType.MainSelect, parents);
                var cell = (SelectSubCell)go.AddComponent(cellType);

                // Enum �� ����
                cell.SetMyType(value);
            }
        }
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
    }

    void SelectJob(Defines.Jobs job)
    {
        selectJob = job;

        DataManager dm = GameManager.instance.Get_DataManager();
        ItemDataManager im = dm.itemDataManager;

        // 1. ������ �ʱ� ���� ������ ����Ʈ ����
        List<string> itemKeys = new List<string>(dm.racesDataManager.jobEquipList[selectJob]);

        // 2. ��ü ������ ������ ��������
        Dictionary<string, ItemBase> itemDatas = im.Get_ItemDatas();

        // 3. ���� ���õ� ���� ������ ��������
        RaceData raceData = dm.racesDataManager.raceDatas[selectRace];

        // 4. �г�Ƽ ��� HashSet ���� (�ߺ� ���� + ���� Ž��)
        HashSet<Defines.EquipmentType> restrictedEquipSet = new HashSet<Defines.EquipmentType>();

        foreach (PaneltyType penalty in raceData.paneltys)
        {
            if (penalty.restrictEquipment)
            {
                restrictedEquipSet.Add(penalty.equipType);
            }
        }

        // 5. ���� ��� ���� (���� ����)
        itemKeys.RemoveAll(key =>
            itemDatas.ContainsKey(key) &&
            itemDatas[key] is EquipItem equipItem &&
            restrictedEquipSet.Contains(equipItem.equipmentType)
        );

        // 6. ���� ������ ����Ʈ ����
        initItems = itemKeys;
    }
}
