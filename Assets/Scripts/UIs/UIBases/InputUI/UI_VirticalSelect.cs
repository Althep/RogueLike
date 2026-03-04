using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
public class UI_VirticalSelect : UI_InputUIBase
{
    public int selectedNumber = 0;
    UI_Base[] childs;
    UI_Base selectedButton;
    public GameObject selectPanel;

    private void Awake()
    {
        myInputType = Defines.InputType.VirticalUI; 
    }
    private void OnEnable()
    {
        SetButtons();
        if(InputManager.instance !=null)
        {
            InputManager.instance.ChangeContext(Defines.InputType.VirticalUI, this,true);
        }
    }

    private void OnDisable()
    {
        
    }
    void SetButtons()
    {
        childs = transform.GetComponentsInChildren<UI_Base>();
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
