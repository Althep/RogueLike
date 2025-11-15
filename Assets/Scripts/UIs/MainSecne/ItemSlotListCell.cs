using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using static Defines;
public class ItemSlotListCell : SelectSubCell, IPoolUI
{
    string selectItemKey;
    public SlotType slot;
    public List<ItemSelectCell> cellList = new List<ItemSelectCell>();
    [SerializeField] List<IPoolUI> myPools = new List<IPoolUI>();
    public List<string> itemKeys = new List<string>();
    public GameObject myContents;
    public GameObject selectViewObj;

    protected override void OnAwake()
    {
        //base.OnAwake();
    }
    public override void SetMyType<T>(T type, SelectSubPanel selectPanels)
    {
        base.SetMyType<T>(type, selectPanels);
        if (type is SlotType s)
        {
            slot = s;
        }
        this.selectPanel = selectPanels;

    }
    public void SetMyKey(string key)
    {
        selectItemKey = key;
    }
    

    public void Init(SlotType slot, List<string> itemKeys)
    {
        if (uiManager == null)
        {
            uiManager = GameManager.instance.Get_UIManager();
        }
        if (myContents == null)
        {
            ScrollRect scroll = transform.GetComponent<ScrollRect>();
            myContents = scroll.content.gameObject;
        }
        this.slot = slot;
        this.itemKeys = itemKeys;
        foreach (string key in itemKeys)
        {
            GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, myContents);

            if (go.TryGetComponent<ItemSelectCell>(out ItemSelectCell cell))
            {
                if (cell is IPoolUI && !myPools.Contains(cell))
                {
                    cell.enabled = true;
                    myPools.Add(cell);
                }
            }
            else
            {
                cell = go.AddComponent<ItemSelectCell>();
                if (cell is IPoolUI)
                {
                    myPools.Add(cell);
                }
            }
            cell.SetMyType(slot, selectPanel as ItemSelectPanel);
            cell.SetMyKey(key);
            cell.SetMyParents(this);
            cell.AddButtonFunction();
            //cell.Init();
            Debug.Log($"[ItemSlotListCell] selectPanel Type = {selectPanel?.GetType().Name}");
        }

        ItemSelectCell first = myPools[0].Get().transform.GetComponent<ItemSelectCell>();
        if (first != null)
        {
            first.Function();
        }
    }

    public void Select(string key, GameObject go)
    {
        selectItemKey = key;
        selectViewObj.transform.SetParent(go.transform);
        selectViewObj.transform.localPosition = Vector3.zero;
        Debug.Log("Moved");
    }

    public override void Return()
    {
        foreach (var ui in myPools)
        {
            ui.Return();
        }
        myPools.Clear();
        if (uiManager == null)
        {
            uiManager = GameManager.instance.Get_UIManager();
        }
        uiManager.Return_PoolUI(UIDefines.UI_PrefabType.ItemSelect, this);

        // 중복 파생 방지
        var allCells = GetComponents<SelectSubCell>();
        foreach (var c in allCells)
        {
            c.enabled = false;
        }
    }
}
