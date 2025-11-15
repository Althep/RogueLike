using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static Defines;
public class RaceSelectPanel : SelectSubPanel
{
    public Races selectRace;
    JobSelectPanel jobselect;

    private void Awake()
    {
        OnAwake();
    }

    public override void OnAwake()
    {
        base.OnAwake();
        
    }

    
    public void OnEnable()
    {
        InitMenu();
    }
    public void OnDisable()
    {
        ReturnMyPools();
        
    }
    
    protected override void InitMenu()
    {
        Races[] races = Utils.Get_Enums<Races>(Races.Default);
        jobselect = selectPanel.jobSelect;
        foreach (Races race in races)
        {
            if (race == Races.Default)
                continue;
            AddRaceButton(race);
        }
        RaceSelectCell first = myPools[0].Get().transform.GetComponent<RaceSelectCell>();
        if(first != null)
        {
            Select<Races>(first.race,first.gameObject);
            Debug.Log($"Selected {first.race}");
        }
        
    }

    #region 버튼
    public void AddRaceButton(Races race)
    {
        GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, myContents);
        if(go.TryGetComponent<RaceSelectCell>(out RaceSelectCell cell))
        {
            if(cell is IPoolUI && !myPools.Contains(cell))
            {
                cell.enabled = true;
                myPools.Add(cell);
            }
            else
            {
                Debug.Log("RaceSelectCell IPoolUI 빼먹음");
            }
        }
        else
        {
            cell = go.AddComponent<RaceSelectCell>();
            if(cell is IPoolUI)
            {
                myPools.Add(cell);
            }
        }
        cell.SetMyType<Races>(race,this);
        cell.AddButtonFunction();
    }
    public override void Select<T>(T race,GameObject go)
    {
        if(race is Races r)
        {
            selectRace = r;
            MoveSelectView(go);
        }
        else
        {
            Debug.Log("RaceSElectPanel 에러");
        }
        //Job패널 초기화
        jobselect.ReturnMyPools();
        jobselect.OnAwake();

    }

    
    #endregion
}
