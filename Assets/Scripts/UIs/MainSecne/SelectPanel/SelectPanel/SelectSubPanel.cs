using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using static Defines;
public class SelectSubPanel : UI_Base
{

    [SerializeField]protected GameObject myContents;
    Dictionary<string, GameObject> mysubCells = new Dictionary<string, GameObject>();

    protected UIManager uiManager;

    protected SelectPanels selectPanel;

    public List<IPoolUI> myPools = new List<IPoolUI>();
    [SerializeField] protected GameObject selectedView;
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
            SetContents();
        }
        
        //InitMenu();
    }
    

    protected void SetContents()
    {
        ScrollRect scroll = this.transform.GetComponent<ScrollRect>();
        myContents = scroll.content.gameObject;
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
    public void FindSelectedObj()
    {

    }
    public void SetParentController(UI_GridSelect gridPanel)
    {

    }
    public virtual List<List<UI_Base>> GetGridData()
    {
        List<List<UI_Base>> data = new List<List<UI_Base>>();
        return data;
    }

    public virtual void Select<T>(T t , GameObject go) where T : Enum
    {
        

    }

    protected virtual void MoveSelectView(GameObject go)
    {
        if(selectedView !=null)
        {
            selectedView.transform.SetParent(go.transform);
            selectedView.transform.localPosition = Vector3.zero;
        }
    }

}
