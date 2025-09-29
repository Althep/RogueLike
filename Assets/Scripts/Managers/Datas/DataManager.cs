using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public class DataManager 
{
    Dictionary<string, ItemBase> itemDatas = new Dictionary<string, ItemBase>();

    
    

    public RacesDataManager racesDataManager { get; private set; }

    public ItemDataManager itemDataManager; 
    public void Init()
    {
        if(racesDataManager == null)
        {
            racesDataManager = new RacesDataManager();
            racesDataManager.Init();
        }
    }


    public Dictionary<string,ItemBase> Get_ItemData()
    {
        return itemDatas;
    }

    
    public Dictionary<string,ItemBase> Get_ItemDatass()
    {
        return itemDataManager.Get_ItemDatas();
    }
}
