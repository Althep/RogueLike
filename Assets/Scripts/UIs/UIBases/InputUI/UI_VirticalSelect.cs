using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
public class UI_VirticalSelect : UI_InputUIBase
{
    public int selectedNumber = 0;
    [SerializeField]UI_Base[] childs;
    [SerializeField]UI_Base selectedButton;
    public GameObject selectPanel;

    private void Awake()
    {
        myInputType = Defines.InputType.VirticalUI;
        if (InputManager.instance!=null)
        {
            Set_Input();
        }
    }
    private void OnEnable()
    {
        SetButtons();
        Debug.Log("1111111111");
        if(InputManager.instance !=null)
        {
            Set_Input();
        }
    }

    private void OnDisable()
    {
        
    }
    void SetButtons()
    {
        childs = transform.GetComponentsInChildren<UI_Base>().Where(c => c.gameObject!=this.gameObject).ToArray();
        selectedButton = childs[0];
    }

    public void ChangeSelection(int direction)
    {
        Debug.Log($"Change Selection {direction}");
        selectedNumber+=direction;
        if (selectedNumber<0)
        {
            selectedNumber = childs.Length-1;
        }
        else if (selectedNumber>=childs.Length)
        {
            selectedNumber=0;
        }
        selectedButton = childs[selectedNumber];
        GameObject selectedPanel = childs[selectedNumber].gameObject;
        selectPanel.transform.SetParent(selectedPanel.transform);
        selectPanel.transform.localPosition = Vector3.zero;
    }
    public void ExecuteSelectedMenu()
    {
        selectedButton.Excute();
    }
    public void CancleMenu()
    {

    }
}
