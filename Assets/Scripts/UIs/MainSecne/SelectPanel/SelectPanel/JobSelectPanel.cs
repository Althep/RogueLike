using System.Collections.Generic;
using UnityEngine;
using static Defines;

public class JobSelectPanel : SelectSubPanel
{
    public Defines.Jobs selectJob;
    private SelectPanels _parent;

    public void SetParentController(SelectPanels parent) => _parent = parent;

    public void RefreshContents(Defines.Races race)
    {
        ReturnMyPools();
        DataManager dm = GameManager.instance.Get_DataManager();
        List<Defines.Jobs> jobList = dm.startDataManager.raceJobList[race];

        foreach (var job in jobList)
        {
            if (job == Defines.Jobs.Default) continue;
            AddJobButton(job);
        }
    }

    public override void Select<T>(T t, GameObject go)
    {
        if (t is Defines.Jobs j)
        {
            selectJob = j;
            MoveSelectView(go);
            _parent.OnJobSelected(j); // ºÎ¸ð¿¡°Ô º¸°í
        }
    }

    public List<List<UI_Base>> GetGridData()
    {
        List<UI_Base> row = new List<UI_Base>();
        foreach (var pool in myPools) row.Add(pool as UI_Base);
        return new List<List<UI_Base>> { row };
    }

    void AddJobButton(Jobs job)
    {
        if (myContents == null)
        {
            SetContents();
        }
        GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, myContents);
        if (go.TryGetComponent<JobSelectCell>(out JobSelectCell cell))
        {
            if (cell is IPoolUI && !myPools.Contains(cell))
            {
                myPools.Add(cell);
                cell.enabled = true;
            }
            else
            {
                Debug.Log("JobSelectCell IPoolUI »©¸ÔÀ½");
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
        cell.SetMyType<Jobs>(job, this);
        cell.AddButtonFunction();
    }
}