using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static Defines;
public class ItemDataManager 
{
    Dictionary<string, ItemBase> itemDatas;

    Dictionary<Defines.EquipmentType, Dictionary<int, string>> EquipToTire;


    #region DataInit
    public void Init()
    {
        if(itemDatas == null)
        {
            InitItemData();
        }
        Read_EquipDatas();
    }

    void Read_EquipDatas()
    {
        CSVReader reader = GameManager.instance.Get_DataManager().csvReader;
        string path = "EquipMents";
        List<Dictionary<string, object>> temp = reader.Read(path);

        for(int i = 0; i < temp.Count; i++)
        {
            string ID = null;
            Utils.TrySetValue<string>(temp[i], "ID", ref ID);
            string Name = null;
            Utils.TrySetValue<string>(temp[i], "Name", ref Name);
            Debug.Log($"Item Name {Name} ID : {ID}");
            if (!itemDatas.ContainsKey(Name))
            {
                EquipItem equip = new EquipItem();
                equip.id = ID;
                Utils.TrySetValue<string>(temp[i], "Name", ref equip.name);
                Utils.TryConvertEnum<ItemCategory>(temp[i], "ItemCategory", ref equip.category);
                Utils.TryConvertEnum<SlotType>(temp[i], "SlotType", ref equip.slot);
                itemDatas.Add(Name, equip);
                equip.options = new List<Modifier>();
            }

            bool exist = itemDatas[Name].options.Any(d => d.id == ID);
            if (!exist)
            {
                StatModifier stat = new StatModifier();
                Utils.TryConvertEnum<ModifierTriggerType>(temp[i], "TriggerType", ref stat.triggerType);
                Utils.TrySetValue<bool>(temp[i], "IsMulti", ref stat.isMulti);
                Utils.TrySetValue<float>(temp[i], "Value", ref stat.value);
                Utils.TrySetValue<int>(temp[i], "Priority", ref stat.priority);
                Utils.TryConvertEnum<StatType>(temp[i], "StatType", ref stat.stat);
                itemDatas[Name].options.Add(stat);
            }
            
            

            //Utils.TrySetValue<string>(temp[i], "ID", ref equip.id);


        }
    }

    



    #endregion
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
