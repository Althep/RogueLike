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
        if(uiPooler == null)
        {
            GameObject go = new GameObject();
            uiPooler = Utils.GetOrAddComponent<UIPooler>(go);
            go.name = "UIPooler";
            GameObject Managers = GameObject.Find("Managers");
            go.transform.SetParent(Managers.transform);
        }
    }

    public void BindPopUp(UI_PopUpObj target)
    {

    }
    public void Pop_Up_UI(string name)
    {

    }
    /*
    public void OpenUI(UI_InputUIBase OpenedUI, InputType inputType)
    {
        OpenedUI.gameObject.SetActive(true);
        inputUIStack.Push(OpenedUI);
        InputManager.instance.ChangeContext(inputType,OpenedUI);
        OpenedUI.transform.SetAsLastSibling();
    }
    public UI_Base CloseUI()
    {
        UI_InputUIBase closedUI = inputUIStack.Pop();
        GameObject targetobj = closedUI.gameObject;
        InputType inputType = closedUI.GetInputType();
        targetobj.SetActive(false);
        return closedUI;
    }
    public void Pop_Up_UI(string button_Name)
    {
        Debug.Log($"Button Name {button_Name}, OpenUI Name {buttonBind[button_Name]}");

        if (buttonBind.ContainsKey(button_Name))
        {
            string targetName = buttonBind[button_Name];
            if (!popUpObjs.ContainsKey(targetName))
            {
                Debug.Log("name Didn't Contain");
                return;
            }
            UI_Base target = popUpObjs[targetName];
            GameObject targetOBJ = target.gameObject;
            if (targetOBJ.activeSelf)
            {
                targetOBJ.SetActive(false);
                Debug.Log("Setactive False");

            }
            else
            {
                targetOBJ.SetActive(true);
                Debug.Log("SetActive True");

            }
            //AddorRemoveAtList(targetOBJ);
        }
    }

    public void BindPopUp(UI_PopUpObj uiObj)
    {
        string objName = uiObj.gameObject.name;
        if (!popUpObjs.ContainsKey(objName))
        {
            popUpObjs.Add(objName, uiObj.gameObject);
            Debug.Log($"PopUp obj Binded Objname : {objName} ");
        }
    }
    /*
    void AddorRemoveAtList(UI_Base ui)
    {
        if (uiStack.Contains(ui))
        {
            uiStack.Remove(ui);
        }
        else
        {
            opened_UIs.Add(ui);
        }
    }
    */
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
}
