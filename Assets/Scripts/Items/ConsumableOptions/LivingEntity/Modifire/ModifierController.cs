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
    public void InitAllContext(LivingEntity entity)
    {
        Debug.Log("Init Context");
        ModifierTriggerType[] triggers = Utils.Get_Enums<ModifierTriggerType>();
        Debug.Log($"Trigger Count ! :    {triggers.Length}");
        foreach (ModifierTriggerType type in triggers)
        {
            Debug.Log($"Init context Trigger : {type}");
            if (contexts.ContainsKey(type))
            {
                Debug.Log("Trigger Contained");
                continue;
            }
            else
            {
                contexts.Add(type, new ModifierContext());
                contexts[type].InitContext(type);
            }
        }
        SetMyEntity(entity);
    }
    public ModifierContext Get_Context(ModifierTriggerType trigger)
    {
        //딕셔너리 초기화 오래걸리니 지연 초기화 사용
        if (contexts.TryGetValue(trigger, out ModifierContext existingContext))
        {
            existingContext.Clear();
            return existingContext;
        }

        ModifierContext newContext = new ModifierContext();
        newContext.InitContext(trigger);
        contexts.Add(trigger, newContext);
        return newContext;
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
            Get_Context(ModifierTriggerType.Passive);
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
            Get_Context(ModifierTriggerType.Passive);
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
    public ModifierContext ApplyModifiers(ModifierTriggerType type)
    {
        ModifierContext context = Get_Context(type);
        if (type == ModifierTriggerType.Passive)
        {
            return context;
        }
        if (!modifiers.TryGetValue(type, out var list))
        {
            return context;
        }
        
        foreach (var modifier in list)
        {
            modifier.Apply(myEntity);
            //result = context.ModifiedValue;
        }
        List<Modifier> actionModifiers = modifiers[type].Where(m => m is ActionModifier).ToList();

        for(int i = 0; i < actionModifiers.Count; i++)
        {
            modifiers[type].Remove(actionModifiers[i]);
        }
        return context;
    }

    public void ResetModifiers()
    {
        modifiers.Clear();
    }

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

        var context = Get_Context(trigger);
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
