using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class ModifierPooler
{
    public Dictionary<ModifierType, Dictionary<string, Queue<IPoolScript>>> inactiveModifiers = new Dictionary<ModifierType, Dictionary<string, Queue<IPoolScript>>>();

    public Dictionary<ModifierType, Dictionary<string, List<IPoolScript>>> activeModifiers = new Dictionary<ModifierType, Dictionary<string, List<IPoolScript>>>();

    ModifierManager modifierManager;
    ModifierFactory modifierFactory;

    public void Set_ModifierManager(ModifierManager MM,ModifierFactory MF)
    {
        modifierManager = MM;
        modifierFactory = MF;
        modifierFactory = modifierManager.GetModifierFactory();
    }

    public Modifier GetModifier(ModifierType type, string id)
    {
        IPoolScript value;
        if (!inactiveModifiers.ContainsKey(type))
        {
            inactiveModifiers.Add(type, new Dictionary<string, Queue<IPoolScript>>());
            if (!inactiveModifiers[type].ContainsKey(id))
            {
                inactiveModifiers[type].Add(id, new Queue<IPoolScript>());
            }
        }
        if (!activeModifiers.ContainsKey(type))
        {
            activeModifiers.Add(type, new Dictionary<string, List<IPoolScript>>());
            if (!activeModifiers[type].ContainsKey(id))
            {
                activeModifiers[type].Add(id, new List<IPoolScript>());
            }
        }
        if (!inactiveModifiers.ContainsKey(type))
        {
            inactiveModifiers.Add(type, new Dictionary<string, Queue<IPoolScript>>());
        }
        if (!inactiveModifiers[type].ContainsKey(id))
        {
            inactiveModifiers[type].Add(id, new Queue<IPoolScript>());
        }
        if (inactiveModifiers[type][id].Count < 1)
        {
            CreateNew(type, id);
        }
        value = inactiveModifiers[type][id].Dequeue();
        if (activeModifiers[type] == null)
        {
            activeModifiers[type] = new Dictionary<string, List<IPoolScript>>();   
        }
        if (!activeModifiers[type].ContainsKey(id))
        {
            activeModifiers[type].Add(id, new List<IPoolScript>());
        }
        activeModifiers[type][id].Add(value);
        return value as Modifier;
    }

    public IPoolScript CreateNew(ModifierType type, string id)
    {
        IPoolScript modifier = null;
        if(modifierFactory == null)
        {
            Debug.Log("모디파이어 팩토리 널!");
            modifierFactory = modifierManager.GetModifierFactory();
        }
        switch (type)
        {
            case ModifierType.StatModifier:
                modifier = new StatModifier();
                if (modifier is StatModifier)
                {
                    modifierFactory.CreateModifier(id).Copy((StatModifier)modifier);
                }

                break;
            case ModifierType.ItemModifier:
                modifier = new ItemModifier();
                if (modifier is ItemModifier)
                {
                    modifierFactory.CreateModifier(id).Copy((ItemModifier)modifier);
                }
                break;
            case ModifierType.BuffModifier:
                modifier = new BuffModifier();
                if (modifier is BuffModifier)
                {
                    modifierFactory.CreateModifier(id).Copy((BuffModifier)modifier);
                }
                break;
            case ModifierType.DamageModifier:
                modifier = new DamageModifier();
                if (modifier is DamageModifier)
                {
                    modifierFactory.CreateModifier(id).Copy((DamageModifier)modifier);
                }
                break;
            case ModifierType.ActionModifier:
                modifier = new ActionModifier();
                if(modifier is ActionModifier)
                {
                    modifierFactory.CreateModifier(id).Copy((ActionModifier)modifier);
                }
                break;
            default:
                break;
        }

        //modifier.SetScriptType(type);
        if (!inactiveModifiers.ContainsKey(type))
        {
            inactiveModifiers.Add(type, new Dictionary<string, Queue<IPoolScript>>());
            if (!inactiveModifiers[type].ContainsKey(id))
            {
                inactiveModifiers[type].Add(id, new Queue<IPoolScript>());
            }
        }
        inactiveModifiers[type][id].Enqueue(modifier);
        return modifier;
    }

    public void ReturnModifier(IPoolScript script)
    {

        Modifier modifier = (Modifier)script;
        ModifierType type = modifier.modifierType;
        string id = modifier.id;
        activeModifiers[type][id].Remove(modifier);

        if (!inactiveModifiers.ContainsKey(type))
        {
            inactiveModifiers.Add(type, new Dictionary<string, Queue<IPoolScript>>());
            if (!inactiveModifiers[type].ContainsKey(id))
            {
                inactiveModifiers[type].Add(id, new Queue<IPoolScript>());
            }

        }
        inactiveModifiers[type][id].Enqueue(script);

    }

}
