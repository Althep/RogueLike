using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
public class SelectSubCell : UI_Base,IPoolUI
{
    protected SelectSubPanel selectPanel;

    protected UI_StringKeyController myTextController;
    UIManager uiManager;

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        uiManager = GameManager.instance.Get_UIManager();
    }
    public virtual void SetMyType<T>(T type,SelectSubPanel panel) where T : Enum
    {
        selectPanel = panel;
        /*
        if(myTextController == null)
        {
            myTextController = this.gameObject.transform.GetComponentInChildren<UI_StringKeyController>();
        }
        //myTextController.Set_MyKey(type.ToString());
        */
    }

    public virtual void AddButtonFunction()
    {
        AddUIEvent(this.gameObject, ButtonFunction, Defines.UIEvents.Click);
    }

    public virtual void ButtonFunction(PointerEventData pev)
    {

    }

    public void Init()
    {
        throw new NotImplementedException();
    }

    public void EnableFunction()
    {
        throw new NotImplementedException();
    }

    public void DisableFunction()
    {
        throw new NotImplementedException();
    }

    public GameObject Get()
    {
        throw new NotImplementedException();
    }

    public virtual void Return()
    {
        uiManager.Return_PoolUI(UIDefines.UI_PrefabType.MainSelect, this);

        // 중복 파생 방지
        var allCells = GetComponents<SelectSubCell>();
        foreach (var c in allCells)
        {
            c.enabled = false;
        }
            
    }
}
