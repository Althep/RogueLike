using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class ItemSelectPanel : SelectSubPanel
{
    public Dictionary<SlotType, string> selectItems = new Dictionary<SlotType, string>();
    
    public Dictionary<SlotType, List<string>> startEquips = new Dictionary<SlotType, List<string>>();
    
    public List<string> startItems = new List<string>();

    Dictionary<SlotType, ItemSlotListCell> itemScrolls = new Dictionary<SlotType, ItemSlotListCell>();
    JobSelectPanel jobSelectPanel;
    RaceSelectPanel raceSelectPanel;
    
    public override void OnAwake()
    {
        base.OnAwake();
        //InitMenu();
    }

    public void OnEnable()
    {
        
    }
    public void OnDisable()
    {
        ReturnMyPools();
    }
    public void Init()
    {
        OnAwake();
        InitMenu();
    }
    protected override void InitMenu()
    {
        base.InitMenu();
        if(jobSelectPanel == null)
        {
            jobSelectPanel = selectPanel.jobSelect;
        }
        if(raceSelectPanel == null)
        {
            raceSelectPanel = selectPanel.raceSelect;
        }
        MakePanels();


    }
    public void MakePanels()
    {
        startEquips = FilterItems();
        foreach(SlotType slot in startEquips.Keys)
        {
            foreach(string value in startEquips[slot])
            {
                Debug.Log($"Start Item Keys {value}");
            }
        }
        foreach (SlotType slot in startEquips.Keys)
        {
            AddItemSlotList(slot);
        }
    }
    
    void AddItemSlotList(SlotType slot)
    {
        GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.ItemSelect, myContents);
        if(go.TryGetComponent<ItemSlotListCell>(out ItemSlotListCell cell))
        {
            if(cell is IPoolUI && !myPools.Contains(cell))
            {
                cell.enabled = true;
                myPools.Add(cell);
            }
            else
            {
                Debug.Log("ItemSlotListCell IPoolUI 아님");
            }
            cell.Init(slot, startEquips[slot]);
            cell.SetMyType(slot, this);
        }
        else
        {
            cell = go.AddComponent<ItemSlotListCell>();
            if(cell is IPoolUI)
            {
                myPools.Add(cell);
            }
            cell.Init(slot, startEquips[slot]);
            cell.SetMyType(slot, this);
        }
        if (!itemScrolls.ContainsKey(slot))
        {
            itemScrolls.Add(slot, cell);
        }
        else
        {
            itemScrolls[slot] = cell;
        }
        
    }

    Dictionary<SlotType, List<string>> FilterItems()
    {
        Jobs selectJob = jobSelectPanel.selectJob;
        Races selectRace = raceSelectPanel.selectRace;
        DataManager dm = GameManager.instance.Get_DataManager();
        ItemDataManager im = dm.itemDataManager;
        ModifierController checker = selectPanel.checker;
        // 1. 직업별 초기 지급 아이템 리스트 복사
        Dictionary<SlotType, StartData> jobItems = dm.startDataManager.startDatas[selectJob];
        Dictionary<SlotType, List<string>> selects = new Dictionary<SlotType, List<string>>();
        // 2. 전체 아이템 데이터 가져오기
        Dictionary<string, ItemBase> itemDatas = im.Get_ItemDatas();

        // 3. 현재 선택된 종족 데이터 가져오기
        if(selectJob == Jobs.Default)
        {
            Debug.Log("JobInit Error Change To Warrior");
            selectJob = Jobs.Warrior;
        }
        
        RaceData raceData = dm.startDataManager.raceDatas[selectRace];

        List<Modifier> modifiers = raceData.modifiers;




        foreach (SlotType slot in jobItems.Keys)
        {
            StartData data = jobItems[slot];

            List<string> itemName = data.names;

            foreach (string key in itemName)
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

    public override void Select<T>(T t, GameObject go)
    {
        base.Select(t, go);
    }
    public void SelectItem(string key, SlotType slot,GameObject go)
    {
        
        if (!selectItems.ContainsKey(slot))
        {
            selectItems.Add(slot, key);
        }
        else
        {
            selectItems[slot] = key;
        }
    }
    public override void ReturnMyPools()
    {
        base.ReturnMyPools();
        itemScrolls.Clear();
    }
}
