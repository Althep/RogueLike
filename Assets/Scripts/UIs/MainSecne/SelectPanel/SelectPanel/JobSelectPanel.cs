using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class JobSelectPanel : SelectSubPanel
{
    public Jobs selectJob;
    public List<Jobs> jobs = new List<Jobs>();
    public ItemSelectPanel itemselectPanel;
    private void Awake()
    {
        OnAwake();
        if(itemselectPanel == null)
        {
            itemselectPanel = selectPanel.itemSelect;
        }
    }

    public override void OnAwake()
    {
        base.OnAwake();
        InitMenu();
        itemselectPanel.OnAwake();
    }

    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        ReturnMyPools();
    }
    public void Init()
    {
        InitMenu();
    }
    protected override void InitMenu()
    {
        if (selectPanel == null)
        {
            selectPanel = GameObject.Find("SelectPanel").transform.GetComponent<SelectPanels>();
        }
        if (itemselectPanel == null)
        {
            itemselectPanel = selectPanel.itemSelect;
        }
        Races selectRace = selectPanel.raceSelect.selectRace;
        DataManager dm = GameManager.instance.Get_DataManager();
        List<Jobs> jobList = dm.startDataManager.raceJobList[selectRace];
        foreach(Jobs job in jobList)
        {
            if (job == Jobs.Default)
                continue;
            AddJobButton(job);
        }
        if (myPools[0] is JobSelectCell cell)
        {
            selectJob = cell.selectJob;
            Select(selectJob, cell.gameObject);
        }
    }
    void AddJobButton(Jobs job)
    {
        if(myContents == null)
        {
            SetContents();
        }
        GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, myContents);
        if(go.TryGetComponent<JobSelectCell>(out JobSelectCell cell))
        {
            if(cell is IPoolUI && !myPools.Contains(cell))
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
            if(cell is IPoolUI)
            {
                myPools.Add(cell);
            }
        }
        cell.SetMyType<Jobs>(job,this);
        cell.AddButtonFunction();
    }

    public override void Select<T>(T t, GameObject go)
    {
        base.Select(t, go);
        if(t is Jobs j)
        {
            selectJob = j;
            MoveSelectView(go);
        }
        //ItemSelect∆–≥Œ √ ±‚»≠ « ø‰
        itemselectPanel.ReturnMyPools();
        itemselectPanel.Init();
    }



}
