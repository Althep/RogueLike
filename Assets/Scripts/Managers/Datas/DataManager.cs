using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public class DataManager 
{
    Dictionary<string, ItemBase> itemDatas = new Dictionary<string, ItemBase>();
    public CSVReader csvReader = new CSVReader();
    
    public StartDataManager startDataManager { get; private set; }
    public ModifierManager modifierManager;
    public ItemDataManager itemDataManager; 
    public void Init()
    {
        if(itemDataManager == null)
        {
            itemDataManager = new ItemDataManager();
        }
        if(startDataManager == null)
        {
            startDataManager = new StartDataManager();
        }
        if(modifierManager == null)
        {
            modifierManager = new();
        }
        itemDataManager.Init();
        modifierManager.Init();
        startDataManager.Init();
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
