using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UI_PopUpObj : UI_Base, IPopUpUI
{
    protected virtual void Awake()
    {
        Init();

    }
    private void Start()
    {

    }
    void Bind()
    {
        GameManager.instance.Bind_UI_PopUp(this);
    }

    public virtual void Init()
    {
        GameManager.instance.Bind_UI_PopUp(this);
        Debug.Log(this.gameObject.name);
        this.gameObject.SetActive(false);
        Debug.Log($"Object Name : {this.gameObject.name}");

    }


}
