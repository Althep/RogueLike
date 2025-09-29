using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    Dictionary<string, GameObject> popUpObjs = new Dictionary<string, GameObject>();
    Dictionary<string, string> buttonBind = new Dictionary<string, string>();//��ư�̸� Ű , ���� ������Ʈ ��� , �̺κ� CSV�� ����ó�� �ʿ�

    List<GameObject> opened_UIs = new List<GameObject>();

    UI_ConfirmPanel confirmPanel;

    UIPooler uiPooler;
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

    public GameObject Get_PoolUI(Defines.UI_PrefabType type,GameObject parents)
    {
        return uiPooler.Get(type, parents);
    }
}
