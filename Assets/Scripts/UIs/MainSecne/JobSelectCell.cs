using UnityEngine;
using UnityEngine.EventSystems;
public class JobSelectCell : SelectSubCell
{
    Defines.Jobs selectJob;


    public override void SetMyType<T>(T type,SelectPanels selectPanels)
    {
        base.SetMyType<T>(type, selectPanels);
        if(type is Defines.Jobs job)
        {
            selectJob = job;
        }
    }
    public override void AddButtonFunction()
    {
        AddUIEvent(this.gameObject, ButtonFunction, Defines.UIEvents.Click);
    }
    public override void ButtonFunction(PointerEventData evt)
    {
        selectPanel.Set_Job(selectJob);
    }
}
