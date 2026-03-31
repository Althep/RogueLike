using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;

public class JobSelectPanel : SelectSubPanel
{
    public Jobs selectJob;
    public List<Jobs> jobs = new List<Jobs>();
    private SelectPanels _parent;

    public void SetParentController(SelectPanels parent) => _parent = parent;

    List<UI_Base> gridData = new List<UI_Base>();
    private void Awake()
    {
        OnAwake();
    }

    public override void OnAwake()
    {
        base.OnAwake();
    }

    private void OnDisable()
    {
        ReturnMyPools();
    }

    // _parentø°º≠ ¡æ¡∑¿Ã πŸ≤ ∂ß∏∂¥Ÿ »£√‚µ 
    public void RefreshContents(Races selectRace)
    {
        _parent.RemoveGridData(gridData);
        gridData.Clear();
        ReturnMyPools();
        DataManager dm = GameManager.instance.Get_DataManager();
        if (dm.startDataManager.raceJobList.ContainsKey(selectRace))
        {
            List<Jobs> jobList = dm.startDataManager.raceJobList[selectRace];
            foreach (Jobs job in jobList)
            {
                if (job == Jobs.Default)
                    continue;
                AddJobButton(job);
            }
        }

        if (myPools.Count > 0 && myPools[0] is JobSelectCell cell)
        {
            selectJob = cell.selectJob;
            Select(selectJob, cell.gameObject);
        }
        AddingGridData();
        _parent.OnJobSelected(selectJob);
        
    }

    void AddJobButton(Jobs job)
    {
        if (myContents == null)
        {
            SetContents();
        }
        
        GameObject go = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, myContents);
        if (go.TryGetComponent<JobSelectCell>(out JobSelectCell cell))
        {
            if (cell is IPoolUI && !myPools.Contains(cell))
            {
                myPools.Add(cell);
                cell.enabled = true;
            }
            else
            {
                Debug.Log("JobSelectCell IPoolUI ª©∏‘¿Ω");
            }
        }
        else
        {
            cell = go.AddComponent<JobSelectCell>();
            if (cell is IPoolUI)
            {
                myPools.Add(cell);
            }
        }
        gridData.Add(cell);
        cell.SetMyType<Jobs>(job, this);
        cell.AddButtonFunction();
    }

    public void AddingGridData()
    {
        Debug.Log($"JobPanel Adding gridData parents {_parent.GetGridCount()} Grid cOunt : {gridData.Count}");
        _parent.AddGridData(gridData);
    }
    public override void Select<T>(T t, GameObject go)
    {
        base.Select(t, go);
        if (t is Jobs j)
        {
            selectJob = j;
            MoveSelectView(go);

            if (_parent != null)
                _parent.OnJobSelected(j);
        }
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