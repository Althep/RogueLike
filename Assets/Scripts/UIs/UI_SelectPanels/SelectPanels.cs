using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static Defines;

public class SelectPanels : UI_Base
{

    public RaceSelectPanel raceSelect;
    public JobSelectPanel jobSelect;


    [SerializeField] GameObject raceSelectPanel;
    [SerializeField] GameObject jobSelectPanel;

    Dictionary<SlotType,GameObject> itemSelectPanels = new Dictionary<SlotType, GameObject>();
    Dictionary<SlotType, List<SelectSubCell>> itemSubPanels = new Dictionary<SlotType, List<SelectSubCell>>();

    UIManager uiManager;

    public ModifierController checker = new ModifierController();

    Dictionary<Type, Type> subCellBind = new Dictionary<Type, Type>()
    {
        {typeof(Defines.Jobs),typeof(JobSelectCell) },
        {typeof(Defines.Races),typeof(RaceSelectCell) },
        {typeof(Defines.SlotType),typeof(ItemSlotListCell) }
    };

    

    private void Awake()
    {
        Init();
    }
    private void OnEnable()
    {
        
    }

    void Init()
    {
        uiManager = GameManager.instance.Get_UIManager();
        //AddUIEvent(confirmPanel, StartButton, Defines.UIEvents.Click);

        raceSelect = raceSelectPanel.transform.GetComponent<RaceSelectPanel>();
        jobSelect = jobSelectPanel.transform.GetComponent<JobSelectPanel>();
    }
    /*
    void Set_My_Selects<T>(T[] array, GameObject parents) where T : Enum
    {
        Type enumType = typeof(T);
        //T[] array = Utils.Get_Enums(type);

        if (subCellBind.TryGetValue(enumType, out Type cellType))
        {
            foreach (var value in array)
            {
                GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, parents);
                var cell = (SelectSubCell)go.AddComponent(cellType);
                if (value.ToString() == "Default")
                    continue;
                // Enum 값 전달
                //cell.SetMyType(value,this);
                cell.AddButtonFunction();
                
            }
        }
    }
    void InitRaces()
    {
        Races[] races = Utils.Get_Enums<Races>(Races.Default);
        Set_My_Selects<Races>(races, raceSelectPanel);
        
    }
    public void Set_Job(Defines.Jobs job)
    {
        selectJob = job;
        Debug.Log("1111");
        SelectJob();
    }
    public void Set_Race(Defines.Races race)
    {
        selectRace = race;
        SelectRace(race);
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

    void SelectRace(Defines.Races race)
    {
        selectRace = race;
        DataManager dm = GameManager.instance.Get_DataManager();
        jobList = dm.startDataManager.raceJobList[selectRace];
        Jobs[] jobs = jobList.ToArray();
        Debug.Log("SElect Race");
        Set_My_Selects<Jobs>(jobs, jobSelectPanel);
        
    }
    #region 무기선택세팅
    void SelectJob()
    {
        
        startItems.Clear();
        Dictionary<SlotType,List<string>> selects = FilterItems(selectJob);
        Debug.Log("asd");
        foreach(var slot in selects.Keys)
        {
            for(int i = 0; i < selects[slot].Count; i++)
            {
                Debug.Log($"시작 리스트 내의 아이템 이름 {selects[slot][i]}");
            }
            
            
        }
        foreach(var slot in selects.Keys)
        {
            
            List<string> items = selects[slot];
            if (!itemSelectPanels.ContainsKey(slot))
            {
                GameObject go = GameManager.instance.Get_UIManager().Get_PoolUI(UIDefines.UI_PrefabType.ItemSelect, itemContentsPanel);
                itemSelectPanels.Add(slot, go);
            }
            for (int i = 0; i < items.Count; i++)
            {
                Debug.Log($"Item Name : {items[i]}");
                GameObject contens = itemSelectPanels[slot].transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, contens);
                ItemSlotListCell subcell = Utils.GetOrAddComponent<ItemSlotListCell>(go);
                subcell.SetMyKey(items[i]);
                subcell.AddButtonFunction();
                if (!itemSubPanels.ContainsKey(slot))
                {
                    itemSubPanels.Add(slot, new List<SelectSubCell>());
                }
                itemSubPanels[slot].Add(subcell);
            }
            
            
        }
    }

    Dictionary<SlotType,List<string>> FilterItems(Defines.Jobs job)
    {
        selectJob = job;

        DataManager dm = GameManager.instance.Get_DataManager();
        ItemDataManager im = dm.itemDataManager;

        // 1. 직업별 초기 지급 아이템 리스트 복사
        Dictionary<SlotType, StartData> jobItems = dm.startDataManager.startDatas[job];
        Dictionary<SlotType,List<string>> selects = new Dictionary<SlotType, List<string>>();
        // 2. 전체 아이템 데이터 가져오기
        Dictionary<string, ItemBase> itemDatas = im.Get_ItemDatas();

        // 3. 현재 선택된 종족 데이터 가져오기
        RaceData raceData = dm.startDataManager.raceDatas[selectRace];

        List<Modifier> modifiers = raceData.modifiers;



        
        foreach(SlotType slot in jobItems.Keys)
        {
            StartData data = jobItems[slot];

            List<string> itemName = data.names;
            
            foreach(string key in itemName)
            {
                if (itemDatas[key] is EquipItem equip)
                {//무기일경우 선택리스트에 추가
                    if (!checker.IsRestricted(itemDatas[key], ModifierTriggerType.OnEquip))
                    {
                        if (!selects.ContainsKey(slot))
                        {
                            selects.Add(slot, new List<string>());
                        }
                        selects[slot].Add(key);
                        
                    }
                }
                else
                {//무기가 아닐경우 초기아이템 리스트에 추가
                    if (!checker.IsRestricted(itemDatas[key], ModifierTriggerType.OnUseItem))
                    {
                        startItems.Add(key);
                    }
                }
            }
            
        }

        return selects;
        // 6. 최종 아이템 리스트 저장
        //return startItems = itemKeys;
    }
    #endregion
    */

}
