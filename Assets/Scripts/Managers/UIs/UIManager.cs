using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    Dictionary<string, GameObject> popUpObjs = new Dictionary<string, GameObject>();
    Dictionary<string, string> buttonBind = new Dictionary<string, string>();//버튼이름 키 , 열릴 오브젝트 밸류 , 이부분 CSV로 매핑처리 필요

    List<GameObject> opened_UIs = new List<GameObject>();

    UI_ConfirmPanel confirmPanel;

    [SerializeField]UIPooler uiPooler;

    private void Awake()
    {
        Init();
    }


    private void Init()
    {
        if(uiPooler == null)
        {
            uiPooler = GameObject.Find("UIPooler").transform.GetComponent<UIPooler>();
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
            GameObject target = popUpObjs[targetName];
            if (target.activeSelf)
            {
                target.SetActive(false);
                Debug.Log("Setactive False");

            }
            else
            {
                target.SetActive(true);
                Debug.Log("SetActive True");

            }
            AddorRemoveAtList(target);
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

    void AddorRemoveAtList(GameObject go)
    {
        if (opened_UIs.Contains(go))
        {
            opened_UIs.Remove(go);
        }
        else
        {
            opened_UIs.Add(go);
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
}
