using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class EntityStat 
{
    Dictionary<StatType, float> baseStat = new();
    Dictionary<StatType, float> finalStat = new();
    int level;


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
}
