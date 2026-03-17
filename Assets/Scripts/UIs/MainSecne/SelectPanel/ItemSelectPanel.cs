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
    private SelectPanels _parent;

    public void SetParentController(SelectPanels parent) => _parent = parent;

    public override void OnAwake()
    {
        base.OnAwake();
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
        MakePanels();
    }

    public void RefreshContents(Jobs job)
    {
        MakePanels();
    }


    public void MakePanels()
    {
        ReturnMyPools();
        itemScrolls.Clear();
        startItems.Clear();

        startEquips = FilterItems();
        /*
        foreach (SlotType slot in startEquips.Keys)
        {
            foreach (string value in startEquips[slot])
            {
                Debug.Log($"Start Item Keys {value}");
            }
        }
        */
        foreach (SlotType slot in startEquips.Keys)
        {
            AddItemSlotList(slot);
        }

        _parent.AddConfirmPanel();
    }

    void AddItemSlotList(SlotType slot)
    {
        GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.ItemSelect, myContents);
        if (go.TryGetComponent<ItemSlotListCell>(out ItemSlotListCell cell))
        {
            if (cell is IPoolUI && !myPools.Contains(cell))
            {
                cell.enabled = true;
                myPools.Add(cell);
            }
            else
            {
                Debug.Log("ItemSlotListCell IPoolUI 아님");
            }
            cell.SetParentController(_parent);
            cell.Init(slot, startEquips[slot]);
            cell.SetMyType(slot, this);
        }
        else
        {
            cell = go.AddComponent<ItemSlotListCell>();
            if (cell is IPoolUI)
            {
                myPools.Add(cell);
            }
            cell.SetParentController(_parent);
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
        Jobs selectJob = _parent.jobSelect.selectJob;
        Races selectRace = _parent.raceSelect.selectRace;
        DataManager dm = GameManager.instance.Get_DataManager();
        ItemDataManager im = dm.itemDataManager;
        ModifierController checker = _parent.checker;

        if (selectJob == Jobs.Default)
        {
            Debug.Log("JobInit Error Change To Warrior");
            selectJob = Jobs.Warrior;
        }

        RaceData raceData = dm.startDataManager.raceDatas[selectRace];
        List<Modifier> modifiers = raceData.modifiers;

        Dictionary<SlotType, StartData> jobItems = dm.startDataManager.startDatas[selectJob];
        Dictionary<SlotType, List<string>> selects = new Dictionary<SlotType, List<string>>();
        Dictionary<string, ItemBase> itemDatas = im.Get_ItemDatas();

        foreach (SlotType slot in jobItems.Keys)
        {
            StartData data = jobItems[slot];
            List<string> itemName = data.names;

            foreach (string key in itemName)
            {
                if (itemDatas[key] is EquipItem equip)
                {
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
                {
                    if (!checker.IsRestricted(itemDatas[key], ModifierTriggerType.OnUseItem))
                    {
                        startItems.Add(key);
                    }
                }
            }
        }

        return selects;
    }

    public override void Select<T>(T t, GameObject go)
    {
        base.Select(t, go);
    }

    public void SelectItem(string key, SlotType slot, GameObject go)
    {
        if (!selectItems.ContainsKey(slot))
        {
            selectItems.Add(slot, key);
        }
        else
        {
            selectItems[slot] = key;
        }
        //MoveSelectView(go); // 시각적 선택 표시
    }

    public override void ReturnMyPools()
    {
        base.ReturnMyPools();
        itemScrolls.Clear();
    }

    public List<UI_Base> GetCellList()
    {
        List<UI_Base> list = new List<UI_Base>();
        foreach (var pool in myPools)
        {
            if (pool is UI_Base uiBase) list.Add(uiBase);
        }
        return list;
    }
}