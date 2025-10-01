using UnityEngine;
using System;
using System.Collections.Generic;
public class ItemDataManager 
{
    Dictionary<string, ItemBase> itemDatas;

    Dictionary<Defines.EquipmentType, Dictionary<int, string>> EquipToTire;

    public void Init()
    {
        if(itemDatas == null)
        {
            InitItemData();
        }
    }

    void InitItemData()
    {
        itemDatas = new Dictionary<string, ItemBase>();
    }

    public Dictionary<string,ItemBase> Get_ItemDatas()
    {
        return itemDatas;
    }

    public Dictionary<Defines.EquipmentType,Dictionary<int,string>> Get_EquipToTire()
    {
        return EquipToTire;
    }
}
