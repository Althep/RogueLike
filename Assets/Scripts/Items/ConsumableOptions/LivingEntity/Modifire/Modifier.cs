using static Defines;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class Modifier : IPoolScript
{
    public PoolScriptType poolType;
    public string id;
    public ModifierTriggerType triggerType;
    public StatType stat;
    public int priority;
    public bool isMulti;
    public float value;
    public ModifierType modifierType;
    public virtual void Apply(ModifierContext context)
    {

    }

    public PoolScriptType GetScriptType()
    {
        return poolType;
    }

    public void SetScriptType(PoolScriptType type)
    {
        poolType = type;
    }
    public virtual void Copy(Modifier newOne)
    {
        newOne.poolType = this.poolType;
        newOne.id = this.id;
        newOne.triggerType = this.triggerType;
        newOne.stat = this.stat;
        newOne.priority = this.priority;
        newOne.isMulti = this.isMulti;
        newOne.value = this.value;
    }

    public virtual void Reset()
    {
        id = null;
        triggerType = ModifierTriggerType.Passive;
        stat = StatType.Accurancy;
        priority = 0;
        isMulti = false;
        value = 0;
    }
}

public class StatModifier : Modifier
{
    public StatModifier(string id, StatType stat, int value, float multifle, bool isMulty, int priority = 50)
    {
        this.id = id;
        this.stat = stat;
        this.value = value;
        this.isMulti = isMulty;
        this.priority = priority;
        modifierType = ModifierType.StatModifier;
    }
    public StatModifier()
    {
        modifierType = ModifierType.StatModifier;
    }
    public void Set(string id, StatType stat, int value, float multifle, bool isMulty, int priority = 50)
    {
        this.id = id;
        this.stat = stat;
        this.value = value;
        this.isMulti = isMulty;
        this.priority = priority;
    }
   public override void Apply( ModifierContext context)
    {
        if (isMulti)
        {
            if (!context.multifle.ContainsKey(this.stat))
            {
                context.multifle.Add(stat, 0f);
            }
            context.multifle[stat] += value;
        }
        else
        {
            if (!context.stats.ContainsKey(this.stat))
            {
                context.stats.Add(stat, 0);
            }
            context.stats[stat] += value;
        }
    }
    
    public override void Copy(Modifier newOne)
    {
        base.Copy(newOne);
        if(newOne is StatModifier stat)
        {
            stat.modifierType = ModifierType.StatModifier;
        }
    }
    public override void Reset()
    {
        base.Reset();
    }

}

public class ItemModifier : Modifier 
{
    public ItemTargetType itemTargetType;
    public ItemCategory itemCategory;
    public Enum specificType;
    public bool unEquipable;
    public ItemModifier(string name, ItemTargetType targetType, ItemCategory category, float value, int priority,bool unEquipable)
    {
        this.id = name;
        this.itemTargetType = targetType;
        itemCategory = category;
        this.unEquipable = unEquipable;
        modifierType = ModifierType.ItemModifier;
    }
    public ItemModifier()
    {
        modifierType = ModifierType.ItemModifier;
    }
    public void Set(string name, ItemTargetType targetType, ItemCategory category, float value, int priority, bool unEquipable)
    {
        this.id = name;
        this.itemTargetType = targetType;
        itemCategory = category;
        this.unEquipable = unEquipable;
    }
    public override void Apply(ModifierContext context)
    {
        if (isMulti)
        {
            if (!context.multifle.ContainsKey(stat))
            {
                context.multifle.Add(stat, 0f);
            }
            context.multifle[stat] += value;
        }
        else
        {
            if (!context.stats.ContainsKey(stat))
            {
                context.stats.Add(stat, 0);
            }
            context.stats[stat] += (int)value;
        }
    }
    public override void Copy(Modifier newOne)
    {
        base.Copy(newOne);
        if(newOne is ItemModifier item)
        {
            item.itemTargetType = itemTargetType;
            item.itemCategory = itemCategory;
            item.specificType = specificType;
            item.unEquipable = unEquipable;
        }
    }
    public override void Reset()
    {
        base.Reset();
        itemTargetType = ItemTargetType.Category;
        itemCategory = ItemCategory.Consumable;
    }

}

public class BuffModifier : Modifier
{
    public int duration;
    public StatusEffect statusEffect;
    public BuffModifier()
    {
        modifierType = ModifierType.BuffModifier;
    }
    public override void Apply(ModifierContext context)
    {
        if(statusEffect != null)
        {
            if (context.statusEffect.Contains(statusEffect))
            {

            }
            else
            {
                context.statusEffect.Add(statusEffect);
            }
        }
    }
    public override void Copy(Modifier newOne)
    {
        base.Copy(newOne);
        if(newOne is BuffModifier buff)
        {
            buff.duration = duration;
        }
    }
    public override void Reset()
    {
        base.Reset();
        duration = 0;
    }
}

public class DamageModifier : Modifier
{
    public DamageType damageType;
    public float evokeRate;
    bool canEvoke = false;
    public DamageModifier()
    {
        modifierType = ModifierType.DamageModifier;
    }
    public override void Apply(ModifierContext context)
    {
        if (CanEvoke())
        {
            if (isMulti)
            {
                if (!context.multifle.ContainsKey(stat))
                {
                    context.multifle.Add(stat, 0f);
                }
                context.multifle[stat] += value;
            }
            else
            {
                if (!context.stats.ContainsKey(stat))
                {
                    context.stats.Add(stat, 0);
                }
                context.stats[stat] += value;
            }
        }
    }

    public bool CanEvoke()
    {
        canEvoke = false;
        if (evokeRate >= UnityEngine.Random.Range(0, 101))
        {
            return canEvoke = true;
        }
        return canEvoke;
    }
    public override void Copy(Modifier newOne)
    {
        base.Copy(newOne);
        if(newOne is DamageModifier damage)
        {
            damage.damageType = damageType;
            damage.evokeRate = evokeRate;
        }
    }
    public override void Reset()
    {
        base.Reset();
        damageType = DamageType.Physical;
        evokeRate = 0;
    }
}
public class ActionModifier:Modifier
{
    ModifierAction action;

    public override void Apply(ModifierContext context)
    {
        List<ModifierAction> effects = context.modifierActions;

        if(!effects.Any(item => item.effectName == action.effectName))
        {
            context.modifierActions.Add(action);
        }
        else
        {
            //버프 시간 갱신등의 아이템 효과 갱신 필요
        }
    }

    public void SetAction(ModifierAction action)
    {
        this.action = action;
    }
}