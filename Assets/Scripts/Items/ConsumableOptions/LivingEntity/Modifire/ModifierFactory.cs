using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Cysharp.Threading.Tasks;
using static Defines;
public class ModifierFactory
{

    ModifierDataManager modifierDataManager;
    public static ModifierFactory _instance;
    ModifierFactory()
    {
    }
    
    public async UniTask Init()
    {
        if (modifierDataManager == null)
        {
            modifierDataManager = await ModifierDataManager.CreateAsync();
        }
    }
    public static async UniTask<ModifierFactory> CreateAsync()
    {
        if (_instance == null)
        {
            _instance = new ModifierFactory(); // 즉시 할당
            _instance.modifierDataManager = await ModifierDataManager.CreateAsync();
        }
        return _instance;
    }
    public Modifier CreateModifier(string id)
    {
        Init().Forget();
        Modifier target = modifierDataManager.GetModifier(id);
        return GetCopyModifier(target);
    }

    Modifier GetCopyModifier(Modifier modifier)
    {
        if(modifier == null)
        {
            Debug.Log("Modifier Null! ModifierFactory_GetCopyModifier");
            return null;
        }
        ModifierType type = modifier.modifierType;
        switch (type)
        {
            case ModifierType.StatModifier:
                StatModifier newStat = new StatModifier();
                if (modifier is StatModifier)
                {
                    modifier.Copy(newStat);
                }
                return newStat;
            case ModifierType.ItemModifier:
                ItemModifier newItem = new ItemModifier();
                if (modifier is ItemModifier)
                {
                    modifier.Copy(newItem);
                }
                return newItem;
            case ModifierType.BuffModifier:
                BuffModifier newBuff = new BuffModifier();
                if (modifier is BuffModifier)
                {
                    modifier.Copy(newBuff);
                }
                return newBuff;
            case ModifierType.DamageModifier:
                DamageModifier newDamage = new DamageModifier();
                if (modifier is DamageModifier)
                {
                    modifier.Copy(newDamage);
                }
                return newDamage;
            case ModifierType.ActionModifier:
                ActionModifier newAction = new ActionModifier();
                if (modifier is ActionModifier)
                {
                    modifier.Copy(newAction);
                }
                return newAction;
            default:
                Debug.Log("StatType Error in GetEmptyModifier_ModifierFactory");
                return null;
        }
    }
}
