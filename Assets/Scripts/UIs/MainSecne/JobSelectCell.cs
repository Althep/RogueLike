using UnityEngine;
using UnityEngine.EventSystems;
using static Defines;
public class JobSelectCell : SelectSubCell
{
    public Defines.Jobs selectJob;

    public override void AddButtonFunction()
    {
        AddUIEvent(this.gameObject, ButtonFunction, Defines.UIEvents.Click);
    }
    public override void ButtonFunction(PointerEventData evt)
    {
        selectPanel.Select<Jobs>(selectJob,this.gameObject);
    }
}
