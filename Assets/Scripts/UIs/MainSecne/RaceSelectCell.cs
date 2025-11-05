using UnityEngine;
using UnityEngine.EventSystems;
public class RaceSelectCell : SelectSubCell
{
    public Defines.Races race;
    public override void SetMyType<T>(T type,SelectSubPanel subpanel)
    {
        base.SetMyType<T>(type,subpanel);
        if(type is Defines.Races race)
        {
            this.race = race;
        }
    }
    public override void AddButtonFunction()
    {
        AddUIEvent(this.gameObject, ButtonFunction, Defines.UIEvents.Click);
    }
    public override void ButtonFunction(PointerEventData pev)
    {
        selectPanel.Select(race,this.gameObject);
    }
    public override void Return()
    {
        base.Return();
    }
}
