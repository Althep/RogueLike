using UnityEngine;
using System;
using System.Collections.Generic;
public class ItemDataManager 
{
    Dictionary<string, ItemBase> itemDatas;



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
}
