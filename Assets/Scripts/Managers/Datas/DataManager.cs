using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public class DataManager 
{
    Dictionary<string, ItemBase> itemDatas = new Dictionary<string, ItemBase>();



    public void Init()
    {

    }


    public Dictionary<string,ItemBase> Get_ItemData()
    {
        return itemDatas;
    }
}
