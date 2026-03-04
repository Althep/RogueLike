using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;

public class ItemSelectPanel : SelectSubPanel
{
    // 최종 선택된 아이템 저장 (슬롯타입별 아이템 키)
    public Dictionary<SlotType, string> selectItems = new Dictionary<SlotType, string>();

    // 필터링 결과: 슬롯별 가용 아이템 리스트
    public Dictionary<SlotType, List<string>> startEquips = new Dictionary<SlotType, List<string>>();

    // 소모품 등 기타 아이템 리스트
    public List<string> startItems = new List<string>();

    // 생성된 슬롯(행) 관리
    Dictionary<SlotType, ItemSlotListCell> itemScrolls = new Dictionary<SlotType, ItemSlotListCell>();

    private SelectPanels _parent;

    public void SetParentController(SelectPanels parent) => _parent = parent;

    public void Init()
    {
        OnAwake(); // 초기화
        InitMenu(); // 메뉴 생성
    }

    public override void OnAwake()
    {
        base.OnAwake(); //
    }

    public void OnDisable()
    {
        ReturnMyPools(); //
    }

    protected override void InitMenu()
    {
        base.InitMenu();
        MakePanels(); // 패널 생성 로직 시작
    }

    public void MakePanels()
    {
        ReturnMyPools(); // 기존 풀 회수
        itemScrolls.Clear();
        startItems.Clear();

        startEquips = FilterItems(); // 아이템 필터링 실행

        foreach (SlotType slot in startEquips.Keys)
        {
            AddItemSlotList(slot); // 각 슬롯별 가로 리스트 생성
        }
    }

    // --- 핵심 필터링 로직 ---
    Dictionary<SlotType, List<string>> FilterItems()
    {
        // 부모 컨트롤러를 통해 현재 선택된 종족/직업 획득
        Jobs selectJob = _parent.jobSelect.selectJob;
        Races selectRace = _parent.raceSelect.selectRace;

        DataManager dm = GameManager.instance.Get_DataManager();
        ItemDataManager im = dm.itemDataManager;
        ModifierController checker = _parent.checker; // 제약 조건 체크기

        if (selectJob == Jobs.Default) selectJob = Jobs.Warrior; // 예외 처리

        // 직업별 초기 데이터 가져오기
        Dictionary<SlotType, StartData> jobItems = dm.startDataManager.startDatas[selectJob];
        Dictionary<SlotType, List<string>> selects = new Dictionary<SlotType, List<string>>();
        Dictionary<string, ItemBase> itemDatas = im.Get_ItemDatas();

        foreach (SlotType slot in jobItems.Keys)
        {
            StartData data = jobItems[slot];
            List<string> itemName = data.names;

            foreach (string key in itemName)
            {
                if (!itemDatas.ContainsKey(key)) continue;

                if (itemDatas[key] is EquipItem equip)
                {
                    // 착용 제한 확인
                    if (!checker.IsRestricted(itemDatas[key], ModifierTriggerType.OnEquip))
                    {
                        if (!selects.ContainsKey(slot)) selects.Add(slot, new List<string>());
                        selects[slot].Add(key);
                    }
                }
                else
                {
                    // 사용 아이템 제약 확인
                    if (!checker.IsRestricted(itemDatas[key], ModifierTriggerType.OnUseItem))
                    {
                        startItems.Add(key);
                    }
                }
            }
        }
        return selects;
    }

    void AddItemSlotList(SlotType slot)
    {
        // UI 풀에서 아이템 슬롯 리스트(가로 행) 가져오기
        GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.ItemSelect, myContents);
        ItemSlotListCell cell = go.GetComponent<ItemSlotListCell>();

        if (cell == null) cell = go.AddComponent<ItemSlotListCell>();

        if (!myPools.Contains(cell)) myPools.Add(cell);

        cell.enabled = true;
        cell.Init(slot, startEquips[slot]); // 내부 아이템 셀들 생성
        cell.SetMyType(slot, this);

        if (!itemScrolls.ContainsKey(slot)) itemScrolls.Add(slot, cell);
        else itemScrolls[slot] = cell;
    }

    // --- GridUI 시스템을 위한 2차원 리스트 생성 ---
    public override List<List<UI_Base>> GetGridData()
    {
        List<List<UI_Base>> grid = new List<List<UI_Base>>();

        // itemScrolls에 담긴 각 슬롯(행)을 순회하며 내부 아이템(열)들을 가져옴
        foreach (var slotKvp in itemScrolls)
        {
            // ItemSlotListCell 내부에서 생성된 ItemSelectCell 리스트를 가져와서 한 행으로 추가
            List<UI_Base> rowCells = slotKvp.Value.GetItemCells();
            if (rowCells != null && rowCells.Count > 0)
            {
                grid.Add(rowCells);
            }
        }
        return grid;
    }

    public void SelectItem(string key, SlotType slot, GameObject go)
    {
        if (!selectItems.ContainsKey(slot)) selectItems.Add(slot, key);
        else selectItems[slot] = key;

        Debug.Log($"Item Selected in Slot {slot}: {key}");
    }

    public override void ReturnMyPools()
    {
        base.ReturnMyPools();
        itemScrolls.Clear(); //
    }
}