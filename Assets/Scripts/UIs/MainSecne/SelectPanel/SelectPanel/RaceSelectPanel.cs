using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static Defines;

public class RaceSelectPanel : SelectSubPanel
{
    public Races selectRace;
    private SelectPanels _parent;
    List<UI_Base> gridData = new List<UI_Base>();

    public void SetParentController(SelectPanels parent) => _parent = parent;

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
        // InitMenu는 _parent(SelectPanels)에서 제어
    }

    public void OnDisable()
    {
        ReturnMyPools();
    }

    public void Init()
    {
        InitMenu();
    }

    protected override void InitMenu()
    {
        ReturnMyPools();
        Races[] races = Utils.Get_Enums<Races>(Races.Default);

        foreach (Races race in races)
        {
            if (race == Races.Default)
                continue;
            AddRaceButton(race);
        }
        _parent.RemoveGridData(gridData);
        _parent.AddGridData(gridData);
        if (myPools.Count > 0)
        {
            RaceSelectCell first = myPools[0].Get().GetComponent<RaceSelectCell>();
            if (first != null)
            {
                Select<Races>(first.race, first.gameObject);
                Debug.Log($"Selected {first.race}");
            }
        }
    }

    public void AddRaceButton(Races race)
    {
        GameObject go = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, myContents);
        if (go.TryGetComponent<RaceSelectCell>(out RaceSelectCell cell))
        {
            if (cell is IPoolUI && !myPools.Contains(cell))
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
            if (cell is IPoolUI)
            {
                myPools.Add(cell);
            }
        }
        cell.SetMyType<Races>(race, this);
        cell.AddButtonFunction();
        gridData.Add(cell);
    }

    public override void Select<T>(T race, GameObject go)
    {
        if (race is Races r)
        {
            selectRace = r;
            MoveSelectView(go);

            if (_parent != null)
                _parent.OnRaceSelected(r);
        }
        else
        {
            Debug.Log("RaceSelectPanel 에러");
        }
    }

    // SelectPanels에게 자신의 버튼 리스트를 반환
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