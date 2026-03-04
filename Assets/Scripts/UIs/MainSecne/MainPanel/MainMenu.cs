using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;
    public int selectNumber = 0;
    public Button[] buttons;
    public Button selectedMenu;
    public GameObject selectPanel;

    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnAwake()
    {
        SetButtons();
        if(instance == null)
        {
            instance = this;
        }
    }
    void SetButtons()
    {
        buttons = transform.GetComponentsInChildren<Button>();
    }
    public void ChangeSelection(int direction)
    {
        Debug.Log($"Change Selection {direction}");
        selectNumber+=direction;
        if (selectNumber<0)
        {
            selectNumber = buttons.Length-1;
        }
        else if (selectNumber>=buttons.Length)
        {
            selectNumber=0;
        }
        selectedMenu = buttons[selectNumber];
        GameObject selectedPanel = buttons[selectNumber].gameObject;
        selectPanel.transform.SetParent(selectedPanel.transform);
        selectPanel.transform.localPosition = Vector3.zero;
    }
    public void ExecuteSelectedMenu()
    {
        selectedMenu.onClick.Invoke();
    }

}
