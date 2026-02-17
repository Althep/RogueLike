using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;

public class ModifierController
{
    

    private Dictionary<ModifierTriggerType, List<Modifier>> modifiers = new Dictionary<ModifierTriggerType, List<Modifier>>();

    private Dictionary<ModifierTriggerType, ModifierContext> contexts = new Dictionary<ModifierTriggerType, ModifierContext>();

    LivingEntity myEntity;

    bool isDirty;
    public void InitContext(LivingEntity entity)
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
        SetMyEntity(entity);
    }

    public void SetMyEntity(LivingEntity entity)
    {
        myEntity = entity;
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
        if (modifier == null) return;

        string id = modifier.id;

        foreach (var key in modifiers.Keys)
        {
            // RemoveAll은 내부적으로 최적화되어 있으며 추가 할당이 없습니다.
            modifiers[key].RemoveAll(m => m.id == id);
        }

        if (modifier.triggerType == ModifierTriggerType.Passive)
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
            modifier.Apply(myEntity);
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
            modifier.Apply(myEntity);
            //result = context.ModifiedValue;
        }
        List<Modifier> actionModifiers = modifiers[type].Where(m => m is ActionModifier).ToList();
        context = contexts[type];
        for(int i = 0; i < actionModifiers.Count; i++)
        {
            modifiers[type].Remove(actionModifiers[i]);
        }
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
    public Dictionary<ModifierTriggerType, List<Modifier>> GetModifiers()
    {
        return modifiers;
    }
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
                            modifier.Apply(myEntity);
                        }
                        break;
                    case ItemTargetType.Specific:
                        // 카테고리 및 세부 타입 일치 확인
                        if (itemModi.itemCategory == item.category &&
                            itemModi.specificType.Equals(item.GetSpecificType()))
                        {
                            modifier.Apply(myEntity);
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
