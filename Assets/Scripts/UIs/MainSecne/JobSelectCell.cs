using UnityEngine;
using UnityEngine.EventSystems;
using static Defines;
public class JobSelectCell : SelectSubCell
{
    public Defines.Jobs selectJob;

    public override void SetMyType<T>(T type, SelectSubPanel subpanel)
    {
        base.SetMyType<T>(type, subpanel);
        if (type is Defines.Jobs job)
        {
            if(job == Defines.Jobs.Default)
            {
                Debug.Log(" Cell Set Type Error Type is Default");
                return;
            }
            this.selectJob = job;
        }
    }
    public override void AddButtonFunction()
    {
        AddUIEvent(this.gameObject, ButtonFunction, Defines.UIEvents.Click);
    }
    public override void ButtonFunction(PointerEventData evt)
    {
        selectPanel.Select<Jobs>(selectJob,this.gameObject);
    }
}
