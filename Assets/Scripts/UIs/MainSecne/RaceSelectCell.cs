using UnityEngine;
using UnityEngine.EventSystems;
public class RaceSelectCell : SelectSubCell
{
    Defines.Races selectRace;

    public override void SetMyType<T>(T type,SelectPanels selectPanels)
    {
        base.SetMyType<T>(type, selectPanels);
        if(type is Defines.Races race)
        {
            selectRace = race;
        }
    }
    public override void AddButtonFunction()
    {
        AddUIEvent(this.gameObject, ButtonFunction, Defines.UIEvents.Click);
    }
    public override void ButtonFunction(PointerEventData pev)
    {
        selectPanel.Set_Race(selectRace);
    }
}
