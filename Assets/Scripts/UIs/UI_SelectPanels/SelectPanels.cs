using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using static Defines;

public class SelectPanels : UI_Base
{

    public RaceSelectPanel raceSelect;
    public JobSelectPanel jobSelect;
    public ItemSelectPanel itemSelect;

    [SerializeField] GameObject raceSelectPanel;
    [SerializeField] GameObject jobSelectPanel;
    [SerializeField] GameObject itemSelectPanel;
    [SerializeField] UI_ConfirmPanel confirmPanel;
    Dictionary<SlotType,GameObject> itemSelectPanels = new Dictionary<SlotType, GameObject>();
    Dictionary<SlotType, List<SelectSubCell>> itemSubPanels = new Dictionary<SlotType, List<SelectSubCell>>();

    UIManager uiManager;

    public ModifierController checker = new ModifierController();

    Dictionary<Type, Type> subCellBind = new Dictionary<Type, Type>()
    {
        {typeof(Defines.Jobs),typeof(JobSelectCell) },
        {typeof(Defines.Races),typeof(RaceSelectCell) },
        {typeof(Defines.SlotType),typeof(ItemSlotListCell) }
    };

    

    private void Awake()
    {
        Init();
        confirmPanel.Open(ConfirmFunction);
    }
    private void OnEnable()
    {
        
    }

    void Init()
    {
        uiManager = GameManager.instance.Get_UIManager();
        //AddUIEvent(confirmPanel, StartButton, Defines.UIEvents.Click);

        raceSelect = raceSelectPanel.transform.GetComponent<RaceSelectPanel>();
        jobSelect = jobSelectPanel.transform.GetComponent<JobSelectPanel>();
    }

    public void ConfirmFunction()
    {
        Dictionary<SlotType, string> selectedItem = itemSelect.selectItems;
        Debug.Log("ÄÁÆß Æã¼Ç!");
        foreach(string key in selectedItem.Values)
        {
            ItemBase item = GameManager.instance.Get_ItemManager().Get_ItemFactory().ItemMake(key);
        }
    }

    public void LoadDungeonScene()
    {
        
    }

}
