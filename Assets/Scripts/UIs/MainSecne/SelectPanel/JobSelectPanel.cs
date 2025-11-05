using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class JobSelectPanel : SelectSubPanel
{
    public Jobs selectJob;
    public List<Jobs> jobs = new List<Jobs>();

    private void Awake()
    {
        
    }

    public override void OnAwake()
    {
        base.OnAwake();
        InitMenu();
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
        Races selectRace = selectPanel.raceSelect.selectRace;
        base.InitMenu();
        DataManager dm = GameManager.instance.Get_DataManager();
        List<Jobs> jobList = dm.startDataManager.raceJobList[selectRace];
        foreach(Jobs job in jobList)
        {
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
            selectViewObj.transform.SetParent(go.transform);
        }
        //ItemSelect∆–≥Œ √ ±‚»≠ « ø‰
    }



}
