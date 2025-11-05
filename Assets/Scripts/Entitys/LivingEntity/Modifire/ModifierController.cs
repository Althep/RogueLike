using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;

public class ModifierController
{
    private Dictionary<ModifierTriggerType, List<Modifier>> modifiers = new Dictionary<ModifierTriggerType, List<Modifier>>();

    private Dictionary<ModifierTriggerType, ModifierContext> contexts = new Dictionary<ModifierTriggerType, ModifierContext>();

    bool isDirty;
    public void InitContext()
    {
        foreach (ModifierTriggerType type in Enum.GetValues(typeof(ModifierTriggerType)))
        {
            contexts[type] = new ModifierContext
            {
                triggerType = type,
                stats = new Dictionary<StatType, float>(),
                multifle = new Dictionary<StatType, float>()
            };
        }
    }

    public void AddModifier(Modifier modifier)
    {
        ModifierTriggerType type = modifier.triggerType;
        if (!modifiers.ContainsKey(type))
        {
            modifiers.Add(type, new List<Modifier>());
        }
        modifiers[type].Add(modifier);
        modifiers[type].Sort((a, b) => a.priority.CompareTo(b.priority));
        if(type == ModifierTriggerType.Passive)
        {
            contexts[ModifierTriggerType.Passive].Clear();
            ApplyPassiveModifiers();
        }
    }

    public void RemoveModifier(Modifier modifier)
    {
        foreach (var type in modifiers.Keys)
        {
            modifiers[type].Remove(modifier);
        }
        if(modifier.triggerType == ModifierTriggerType.Passive)
        {
            contexts[ModifierTriggerType.Passive].Clear();
            ApplyPassiveModifiers();
        }
    }

    void ApplyPassiveModifiers()
    {
        if(!modifiers.TryGetValue(ModifierTriggerType.Passive, out var list))
        {
            return;
        }
        foreach(var modifier in list)
        {
            modifier.Apply(contexts[ModifierTriggerType.Passive]);
        }
        
    }
    public ModifierContext ApplyModifiers(ModifierTriggerType type,ModifierContext context)
    {
        if(type == ModifierTriggerType.Passive)
        {
            return contexts[ModifierTriggerType.Passive];
        }
        contexts[type].Clear();
        if (!modifiers.TryGetValue(type, out var list))
        {
            return contexts[type];
        }
        
        foreach (var modifier in list)
        {
            modifier.Apply(contexts[type]);
            //result = context.ModifiedValue;
        }
        context = contexts[type];
        return contexts[type];
    }

    public void ResetModifiers()
    {
        modifiers.Clear();
    }
    /*
    public float Calculate(ModifierTriggerType type, ModifierContext context)
    {
        context.ModifiedValue = context.BaseValue;

        // 우선순위 기준으로 정렬 후 순차적 적용
        foreach (var mod in modifiers[type])
        {
            mod.Apply(context);
        }

        return context.ModifiedValue;
    }
    */

    public bool CanEquip(ModifierTriggerType trigger)
    {
        foreach (Modifier modis in modifiers[trigger])
        {
            ModifierTriggerType type = modis.triggerType;
        }
        return true;
    }

    public bool IsRestricted(ItemBase item, ModifierTriggerType trigger)
    {
        if (!modifiers.TryGetValue(trigger, out var list))
            return false;

        var context = contexts[trigger];
        context.Clear();

        foreach (var modifier in list)
        {
            if (modifier is ItemModifier itemModi)
            {
                switch (itemModi.itemTargetType)
                {
                    case ItemTargetType.Category:
                        if(itemModi.itemCategory == item.category)
                        {
                            modifier.Apply(context);
                        }
                        break;
                    case ItemTargetType.Specific:
                        // 카테고리 및 세부 타입 일치 확인
                        if (itemModi.itemCategory == item.category &&
                            itemModi.specificType.Equals(item.GetSpecificType()))
                        {
                            modifier.Apply(context);
                        }
                        break;
                    default:
                        break;
                }
                
            }
        }

        // 제한 조건 확인
        foreach (var kvp in context.multifle)
        {
            if (kvp.Value <= -1f)
                return true;
        }

        return false;
    }
}
