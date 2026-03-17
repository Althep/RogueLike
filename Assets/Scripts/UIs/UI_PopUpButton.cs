using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class UI_PopUpButton : UI_Base
{
    string buttonName;
    private void Awake()
    {
        buttonName = this.gameObject.name;
        Init();
    }
    private void OnEnable()
    {

    }
    void Init()
    {

        buttonName = this.gameObject.name;
        AddUIEvent(this.gameObject, OnClickEvent, Defines.UIEvents.Click);
    }

    public override void Excute()
    {
        Debug.Log("PopUpUI!");
        GameManager.instance.PopUpUI(buttonName);
    }
    protected virtual void OnClickEvent(PointerEventData evt)
    {
        Excute();
    }
}
