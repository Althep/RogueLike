using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
public class UI_ItemInfo : UI_Base
{
    [SerializeField] Image itemIcon;
    [SerializeField] UI_StringKeyController itemName;
    [SerializeField] UI_StringKeyController itemDesc;
    [SerializeField] List<UI_StringKeyValueController> itemAddOptions;
    [SerializeField] ItemBase selectedItem;
    [SerializeField] GameObject itemOptionPanel;
    List<UI_StringKeyValueController> optionCells = new();

    private void OnEnable()
    {
        if (selectedItem == null)
            return;
    }

    private void OnDisable()
    {
        selectedItem = null;
        ReturnItemOptions();
    }

    public void OnSelectItem(ItemBase item)
    {
        Set_SelectedItem(item);
        if (selectedItem == null)
            return;
        if(selectedItem.name == null)
        {
            Debug.Log($"{selectedItem.id} name is null");
            return;
        }
        itemName.Set_MyKey(selectedItem.name);
        itemDesc.Set_MyKey(selectedItem.name+"_Desc");
        Set_BaseItemOptions();
        Set_AddItemOptions();
    }


    public void Set_BaseItemOptions()
    {
        foreach(var option in selectedItem.options)
        {
            GameObject go = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.ItemOption,itemOptionPanel);
            UI_StringKeyValueController keyController = go.transform.GetComponentInChildren<UI_StringKeyValueController>();
            
            if(keyController == null)
            {
                Debug.Log("keyController Null");
                continue;
            }
            if(option == null)
            {
                Debug.Log("Modi null");
                continue;
            }
            foreach(Modifier mod in option.myMods)
            {
                keyController.Add_MyValue(mod.value.ToString());
            }
            
            keyController.Set_MyKey($"{option.optionKey}_Desc");
            optionCells.Add(keyController);
        }
    }

    public void Set_AddItemOptions()
    {
        if (selectedItem is EquipItem equip && equip.addOptions.Count>0)
        {
            foreach(ModifierOption option in equip.addOptions)
            {
                GameObject go = UIManager.instance.Get_PoolUI(UIDefines.UI_PrefabType.ItemOption, itemOptionPanel);
                UI_StringKeyValueController keyController = go.transform.GetComponent<UI_StringKeyValueController>();
                foreach(var mod in option.myMods)
                {
                    keyController.Add_MyValue(mod.value.ToString());
                }
                
                keyController.Set_MyKey($"{option.optionKey}_Desc");
                optionCells.Add(keyController);
            }
        }
    }

    public void ReturnItemOptions()
    {
        for(int i = 0; i<optionCells.Count; i++)
        {
            optionCells[i].Return();
        }
    }

    public void Set_SelectedItem(ItemBase item)
    {
        selectedItem = item;
    }
}
