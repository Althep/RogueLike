using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using static Defines;
public class ModifierFactory 
{
    [SerializeField] string dataPath;
    [SerializeField] string statPath;
    [SerializeField] string itemPath;
    [SerializeField] string buffPath;
    [SerializeField] string damagePath;
    public Dictionary<string, Modifier> Modifiers = new Dictionary<string, Modifier>();
    CSVReader reader;

    public Modifier Get_Modifier(string id)
    {
        ModifierType type = Modifiers[id].modifierType;

        switch (type)
        {
            case ModifierType.StatModifier:
                StatModifier newStat = new StatModifier();
                Modifiers[id].Copy(newStat);
                return newStat;
            case ModifierType.ItemModifier:
                ItemModifier newItem = new ItemModifier();
                Modifiers[id].Copy(newItem);
                return newItem;
            case ModifierType.BuffModifier:
                BuffModifier newBuff = new BuffModifier();
                Modifiers[id].Copy(newBuff);
                return newBuff;
            case ModifierType.DamageModifier:
                DamageModifier newDamage = new DamageModifier();
                Modifiers[id].Copy(newDamage);
                return newDamage;
            default:
                Debug.Log("StatType Error in Get_Modifier_ModifierFactory");
                return null;
        }
    }

    public void ReadAllModifiers()
    {
        ReadStatModifier();
        ReadItemModifier();
        ReadBuffModifier();
        ReadDamageModifier();
    }

    public void ReadModifier(Dictionary<string,object> data , Modifier modifier)
    {
        Utils.TrySetValue<string>(data, "ID", ref modifier.id);
        Utils.TryConvertEnum<StatType>(data, "StatType", ref modifier.stat);
        Utils.TryConvertEnum<ModifierTriggerType>(data, "ModifierTrigger", ref modifier.triggerType);
        Utils.TryConvertEnum<StatType>(data, "StatType", ref modifier.stat);
        Utils.TrySetValue<int>(data, "Priority", ref modifier.priority);
        Utils.TrySetValue<bool>(data, "isMulti", ref modifier.isMulti);
        Utils.TrySetValue<float>(data, "Value", ref modifier.value);
    }
    public void ReadStatModifier()
    {
        if(reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
        string path = dataPath + statPath;
        var temp = reader.Read(path);
        for (int i = 0; i < temp.Count; i++)
        {
            string id = temp[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            StatModifier stat = new StatModifier();
            stat.poolType = PoolScriptType.StatModifier;
            stat.id = id;
            ReadModifier(temp[i], stat);
            Modifiers.Add(id, stat);
        }
    }

    public void ReadItemModifier()
    {
        if (reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
        string path = dataPath + itemPath;
        var temp = reader.Read(path);
        for (int i = 0; i < temp.Count; i++)
        {
            string id = temp[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            ItemModifier item = new ItemModifier();
            item.poolType = PoolScriptType.ItemModifier;
            item.id = id;
            ReadModifier(temp[i], item);
            Utils.TryConvertEnum<ItemTargetType>(temp[i], "ItemTargetType", ref item.itemTargetType);
            Utils.TryConvertEnum<ItemCategory>(temp[i], "ItemCategory", ref item.itemCategory);
            Utils.TrySetValue<bool>(temp[i], "UnEquipable", ref item.unEquipable);
            item.specificType = Utils.Get_ItemSpecificType(temp[i].ToString());
            Modifiers.Add(id, item);
        }
    }

    public void ReadBuffModifier()
    {
        if (reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
        string path = dataPath + buffPath;
        var temp = reader.Read(path);
        for (int i = 0; i < temp.Count; i++)
        {
            string id = temp[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            BuffModifier buff = new BuffModifier();
            buff.poolType = PoolScriptType.ItemModifier;
            buff.id = id;
            ReadModifier(temp[i], buff);
            Utils.TrySetValue<int>(temp[i], "Duration", ref buff.duration);
            Modifiers.Add(id, buff);
        }
    }
    public void ReadDamageModifier()
    {
        if (reader == null)
        {
            reader = GameManager.instance.Get_DataManager().csvReader;
        }
        string path = dataPath + damagePath;
        var temp = reader.Read(path);
        for (int i = 0; i < temp.Count; i++)
        {
            string id = temp[i]["ID"].ToString();
            if (Modifiers.ContainsKey(id))
            {
                continue;
            }
            DamageModifier damage = new DamageModifier();
            damage.poolType = PoolScriptType.ItemModifier;
            damage.id = id;
            ReadModifier(temp[i], damage);
            Utils.TryConvertEnum<DamageType>(temp[i], "DamageType", ref damage.damageType);
            Utils.TrySetValue<float>(temp[i], "EvokeRate", ref damage.evokeRate);
            Modifiers.Add(id, damage);
        }
    }

}
