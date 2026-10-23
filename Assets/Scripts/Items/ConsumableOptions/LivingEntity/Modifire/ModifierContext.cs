using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Defines;

public class ModifierContext
{
    public LivingEntity owner;
    public ModifierTriggerType triggerType; // 발동조건
    public Dictionary<StatType,float> stats; // 각 능력치
    public Dictionary<StatType, float> multiplier;
    public List<ModifierAction> modifierActions = new List<ModifierAction>();
    public List<StatusEffect> statusEffect = new List<StatusEffect>();
    public ModifierType ModifierType;
    public DamageType damageType = DamageType.Physical;
    private static readonly StatType[] allStatTypes = Utils.Get_Enums<StatType>();
    public ModifierContext(LivingEntity owner)
    {
        this.owner = owner;
    }
    public void InitContext(ModifierTriggerType trigger)
    {
        triggerType = trigger;
        StatType[] types = Utils.Get_Enums<StatType>();
        if (stats == null)
        {
            stats = new Dictionary<StatType, float>();
        }
        if (multiplier == null)
        {
            multiplier = new Dictionary<StatType, float>();
        }
        foreach (StatType type in types)
        {
            stats.Add(type, 0);
        }
        foreach (StatType type in types)
        {
            multiplier.Add(type, 0);
        }
    }
    public void ExecuteModifierActions()
    {
        for(int i = 0; i<modifierActions.Count; i++)
        {
            if (owner == null)
                return;
            modifierActions[i].Excute(owner);
        }
    }
    public LivingEntity GetOwner()
    {
        return owner;
    }
    public void Clear()
    {
        for (int i = 0; i < allStatTypes.Length; i++)
        {
            StatType key = allStatTypes[i];
            stats[key] = 0;
            multiplier[key] = 0;
        }
        damageType = DamageType.Physical;

        // 이 두 줄이 반드시 추가되어야 합니다!
        modifierActions.Clear();
        statusEffect.Clear();
    }
}
