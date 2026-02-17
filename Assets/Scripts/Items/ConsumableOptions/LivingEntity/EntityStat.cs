using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class EntityStat
{
    protected TileType tileType;
    protected Dictionary<StatType, float> baseStat = new();
    protected Dictionary<StatType, float> finalStat = new();
    protected int level;
    protected string entityId;
    protected string entityName;

    public void SetBaseStat(StatType type, float value)
    {
        if (baseStat.ContainsKey(type))
        {
            baseStat[type] += value;
        }
        else
        {
            baseStat.Add(type, value);
        }

    }
    public void SetID(string id)
    {
        entityId = id;
    }
    public void SetName(string Name)
    {
        entityName = Name;
    }
    public void SetTileType(TileType type)
    {
        tileType = type;
    }
    public TileType GetTileType()
    {
        return tileType;
    }
    public string GetId()
    {
        return entityId;
    }
    public string GetName()
    {
        return entityName;
    }
    public int GetLevel()
    {
        return level;
    }
    public Dictionary<StatType,float> GetBase()
    {
        return baseStat;
    }
    public Dictionary<StatType,float> GetFinal()
    {
        return finalStat;
    }
    public float GetBaseStat(StatType stat)
    {
        return baseStat[stat];
    }
    
    public virtual EntityStat CopyStat() 
    {
        EntityStat newStat = new EntityStat();
        foreach(StatType stat in baseStat.Keys)
        {
            newStat.SetBaseStat(stat, baseStat[stat]);
        }
        newStat.level = level;
        newStat.SetName(entityName);
        newStat.SetID(entityId);
        newStat.SetTileType(tileType);
        return newStat;
    }

    public int GetDamageToResist(DamageType type,int damage)
    {
        switch (type)
        {
            case DamageType.Physical:
                break;
            case DamageType.Fire:
                damage = (int)(damage* (1/ GetFinal()[StatType.FireResist]));
                break;
            case DamageType.Ice:
                damage = (int)(damage* (1/ GetFinal()[StatType.IceResist]));
                break;
            case DamageType.Magic:
                damage = (int)(damage* (1/ GetFinal()[StatType.MagicResist]));
                break;
            case DamageType.Thunder:
                damage = (int)(damage* (1/ GetFinal()[StatType.ThunderResist]));
                break;
            case DamageType.Poison:
                break;
            default:
                break;
        }
        return damage;
    }
}
