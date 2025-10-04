using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public class ModifierManager
{
    private Dictionary<ActionType, List<Modifier>> modifiers = new Dictionary<ActionType, List<Modifier>>();

    private ModifierContext modifireContext;

    public void AddModifier(ActionType type,Modifier modifier)
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

    public float ApplyModifiers(ActionType type, ModifierContext context)
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



    public float Calculate(ActionType type,ModifierContext context)
    {
        context.ModifiedValue = context.BaseValue;

        // 우선순위 기준으로 정렬 후 순차적 적용
        foreach(var mod in modifiers[type])
        {
            mod.Apply(context);
        }

        return context.ModifiedValue;
    }
}