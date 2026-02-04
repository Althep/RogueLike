using System;
using System.Collections.Generic;

using static Defines;
using UnityEngine.TextCore.Text;

public class ModifierContext
{
    public ModifierTriggerType triggerType; // 발동조건
    public Dictionary<StatType,float> stats; // 각 능력치
    public Dictionary<StatType, float> multifle;
    public List<ModifierAction> modifierActions = new List<ModifierAction>();
    public List<StatusEffect> statusEffect = new List<StatusEffect>();
    public ModifierType ModifierType;
    public DamageType damageType = DamageType.Physical;
    public bool isDirty;
    public ModifierContext()
    {
        foreach(StatType types in Enum.GetValues(typeof(StatType)))
        {
            stats.Add(types, 0);
        }
        foreach (StatType types in Enum.GetValues(typeof(StatType)))
        {
            multifle.Add(types, 0);
        }
    }
    public void Clear()
    {
        foreach(StatType key in stats.Keys)
        {
            stats[key] = 0;
        }
        foreach(StatType key in multifle.Keys)
        {
            multifle[key] = 0;
        }
        damageType = DamageType.Physical;
        
    }
}