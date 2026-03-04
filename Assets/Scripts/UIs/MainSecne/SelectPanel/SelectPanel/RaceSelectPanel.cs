using System.Collections.Generic;
using UnityEngine;
using static Defines;

public class RaceSelectPanel : SelectSubPanel
{
    public Defines.Races selectRace;
    private SelectPanels _parent;

    public void SetParentController(SelectPanels parent) => _parent = parent;

    protected override void InitMenu()
    {
        ReturnMyPools();
        Defines.Races[] races = Utils.Get_Enums<Defines.Races>(Defines.Races.Default);

        foreach (var race in races)
        {
            if (race == Defines.Races.Default) continue;
            AddRaceButton(race);
        }
    }

    public override void Select<T>(T race, GameObject go)
    {
        if (race is Defines.Races r)
        {
            selectRace = r;
            MoveSelectView(go);
            _parent.OnRaceSelected(r); // 부모에게 보고
        }
    }

    public override List<List<UI_Base>> GetGridData()
    {
        List<UI_Base> row = new List<UI_Base>();
        foreach (var pool in myPools) row.Add(pool as UI_Base);
        return new List<List<UI_Base>> { row };
    }

    public void AddRaceButton(Races race)
    {
        GameObject go = uiManager.Get_PoolUI(UIDefines.UI_PrefabType.MainSelect, myContents);
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
    }
    // 그리드 데이터 생성 (1행 N열 구조)
    
}