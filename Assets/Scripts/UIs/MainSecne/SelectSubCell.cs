using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
public class SelectSubCell : UI_Base
{
    protected SelectPanels selectPanel;
    public virtual void SetMyType<T>(T type,SelectPanels selectPanel) where T : Enum
    {
        this.selectPanel = selectPanel;
    }

    public virtual void AddButtonFunction()
    {
        AddUIEvent(this.gameObject, ButtonFunction, Defines.UIEvents.Click);
    }

    public virtual void ButtonFunction(PointerEventData pev)
    {

    }
}
