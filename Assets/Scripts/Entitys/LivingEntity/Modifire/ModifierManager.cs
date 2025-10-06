using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public class ModifierManager
{
    private Dictionary<ModifierTriggerType, List<Modifier>> modifiers = new Dictionary<ModifierTriggerType, List<Modifier>>();

    

    public void AddModifier(ModifierTriggerType type,Modifier modifier)
    {
        if (!modifiers.ContainsKey(type))
        {
            modifiers.Add(type, new List<Modifier>());
        }
        modifiers[type].Add(modifier);
        modifiers[type].Sort((a,b)=>a.priority.CompareTo(b.priority));
    }

    public void RemoveModifier(Modifier modifier)
    {
        foreach(var type in modifiers.Keys)
        {
            modifiers[type].Remove(modifier);
        }
        
    }

    public float ApplyModifiers(ModifierTriggerType type, ModifierContext context)
    {
        if(!modifiers.TryGetValue(type, out var list))
        {
            return context.ModifiedValue;
        }
        float result = context.ModifiedValue;

        foreach(var modifier in list)
        {
            modifier.Apply(context);
            result = context.ModifiedValue;
        }
        return result;
    }

    public void ResetModifiers()
    {
        modifiers.Clear();
    }

    public float Calculate(ModifierTriggerType type,ModifierContext context)
    {
        context.ModifiedValue = context.BaseValue;

        // �켱���� �������� ���� �� ������ ����
        foreach(var mod in modifiers[type])
        {
            mod.Apply(context);
        }

        return context.ModifiedValue;
    }
}