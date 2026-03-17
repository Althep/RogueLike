using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Defines;

public class ModifierContext
{
    public ModifierTriggerType triggerType; // 발동조건
    public Dictionary<StatType,float> stats; // 각 능력치
    public Dictionary<StatType, float> multifle;
    public List<ModifierAction> modifierActions = new List<ModifierAction>();
    public List<StatusEffect> statusEffect = new List<StatusEffect>();
    public ModifierType ModifierType;
    public DamageType damageType = DamageType.Physical;
    private static readonly StatType[] allStatTypes = Utils.Get_Enums<StatType>();
    public bool isDirty;

    public ModifierContext()
    {
        
    }
    public void InitContext(ModifierTriggerType trigger)
    {
        triggerType = trigger;
        StatType[] types = Utils.Get_Enums<StatType>();
        if (stats == null)
        {
            stats = new Dictionary<StatType, float>();
        }
        if (multifle == null)
        {
            multifle = new Dictionary<StatType, float>();
        }
        foreach (StatType type in types)
        {
            stats.Add(type, 0);
        }
        foreach (StatType type in types)
        {
            multifle.Add(type, 0);
        }
    }
    public void Clear()
    {
        for (int i = 0; i < allStatTypes.Length; i++)
        {
            StatType key = allStatTypes[i];

            stats[key] = 0;
            multifle[key] = 0;
        }
        damageType = DamageType.Physical;
    }
}