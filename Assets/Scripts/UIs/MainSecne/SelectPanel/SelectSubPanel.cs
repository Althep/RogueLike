using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using static Defines;
public class SelectSubPanel : UI_Base
{
    public GameObject selectViewObj;
    protected GameObject myContents;
    Dictionary<string, GameObject> mysubCells = new Dictionary<string, GameObject>();

    protected UIManager uiManager;

    protected SelectPanels selectPanel;

    public List<IPoolUI> myPools = new List<IPoolUI>();

    private void Awake()
    {
        OnAwake();
    }

    public virtual void OnAwake()
    {
        if (uiManager == null)
        {
            uiManager = GameManager.instance.Get_UIManager();
        }
        if(selectPanel == null)
        {
            selectPanel = GameObject.Find("SelectPanel").transform.GetComponent<SelectPanels>() ;
        }
        if(myContents == null)
        {
            myContents = transform.GetComponent<ScrollRect>().transform.GetChild(0).gameObject;
        }
        SetContents();
        InitMenu();
    }
    

    protected void SetContents()
    {
        ScrollRect scroll = this.transform.GetComponent<ScrollRect>();
        myContents = scroll.gameObject.transform.GetChild(0).gameObject;
    }

    protected virtual void InitMenu()
    {

    }

    public virtual void ReturnMyPools()
    {
        foreach(var ui in myPools)
        {
            ui.Return();
        }
        myPools.Clear();
    }

    public virtual void Select<T>(T t , GameObject go) where T : Enum
    {
        

    }
}
