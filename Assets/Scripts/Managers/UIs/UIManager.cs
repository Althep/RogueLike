using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using static Defines;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
    Stack<UI_Base> uiStacks = new Stack<UI_Base>();
    
    //List<GameObject> opened_UIs = new List<GameObject>();
    InputUIManager inputUIManager;
    UI_ConfirmPanel confirmPanel;
    UIDataManager uidataManager;
    StringKeyManager _SKManager;
    [SerializeField]UIPooler uiPooler;

    private void Awake()
    {
        Init();
    }


    private void Init()
    {
        if(instance == null)
        {
            instance = this;
        }
        if(uiPooler == null)
        {
            uiPooler = GameObject.Find("UIPooler").transform.GetComponent<UIPooler>();
        }
        if(inputUIManager == null)
        {
            inputUIManager = transform.GetComponent<InputUIManager>();
        }
        if(_SKManager == null)
        {
            _SKManager = transform.GetComponent<StringKeyManager>();
        }
        if(uiPooler == null)
        {
            GameObject go = new GameObject();
            uiPooler = Utils.GetOrAddComponent<UIPooler>(go);
            go.name = "UIPooler";
            GameObject Managers = GameObject.Find("Managers");
            go.transform.SetParent(Managers.transform);
        }
        if(uidataManager == null)
        {
            uidataManager = GameManager.instance.Get_DataManager().uiDataManager;
        }
    }
    public UIPooler Get_UIPooler()
    {
        return uiPooler;
    }
    public InputUIManager GetInputUIManager()
    {
        return inputUIManager;
    }

    public void BindPopUp(UI_PopUpObj target)
    {

    }
    public void Pop_Up_UI(string name)
    {
        GameObject go = uidataManager.GetTargetToName(name);
        if(go != null)
        {
            go.SetActive(true);
        }
    }
    public void OpenConfirmPanel(Action confirmAction)
    {
        if(confirmPanel == null)
        {

        }
        confirmPanel.Open(confirmAction);
    }

    public GameObject Get_PoolUI(UIDefines.UI_PrefabType type,GameObject parents)
    {
        return uiPooler.Get(type, parents);
    }

    public void Return_PoolUI(UIDefines.UI_PrefabType type,IPoolUI ui)
    {
        uiPooler.Return(type, ui);
    }

    public StringKeyManager GetStringKeyManager()
    {
        if(_SKManager == null)
        {
            _SKManager = transform.GetComponent<StringKeyManager>();
        }
        return _SKManager;
    }
}
