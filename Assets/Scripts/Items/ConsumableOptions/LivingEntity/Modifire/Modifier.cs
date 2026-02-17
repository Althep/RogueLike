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
    public virtual void Apply(LivingEntity entity)
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
   public override void Apply(LivingEntity entity)
    {
        ModifierContext context = entity.GetContext();
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

public class BuffModifier : Modifier
{
    public int duration;
    public int minTime;
    public int maxTime;
    public string stringValue;
    public string effectName;
    public ActionEffectType actionType;
    public BuffCategory buffCategory;
    public BuffType buffType;
    ModifierAction myAction;
    public BuffModifier()
    {
        modifierType = ModifierType.BuffModifier;
    }
    public override void Apply(LivingEntity entity)
    {

        duration = UnityEngine.Random.Range(minTime, maxTime+1);
        EventManager.instance.AddBuff(entity, this, duration);
    }
    void GetStats()
    {

    }
    public override void Copy(Modifier newOne)
    {
        base.Copy(newOne);
        
        if(newOne is BuffModifier buff)
        {
            if(myAction != null)
            {
                buff.myAction = myAction;
            }

            buff.minTime = minTime;
            buff.maxTime = maxTime;
            buff.stringValue = stringValue;
            buff.actionType = actionType;
            buff.buffCategory = buffCategory;
        }
    }
    public override void Reset()
    {
        base.Reset();
        myAction = null;
        minTime = 0;
        maxTime = 0;
        stringValue = null;
        actionType = 0 ;
        buffCategory = 0;
        duration = 0;
    }
    public void SetMyAction(ModifierAction modAction)
    {
        myAction = modAction;
        myAction.value = value;
        myAction.stringValue = stringValue;
        myAction.SetValueToString();
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
    public override void Apply(LivingEntity entity)
    {
        ModifierContext context = entity.GetContext();
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
public class ActionModifier: Modifier
{
    ModifierAction action;
    public string effectName;
    public string stringValue;
    public int minValue;
    public int maxValue;    
    public override void Apply(LivingEntity entity)
    {
        ModifierContext context = entity.GetContext();
        List<ModifierAction> effects = context.modifierActions;
        int duration = 0;
        for(int i = 0; i<effects.Count; i++)
        {
            if(effects[i] is BuffAction buff)
            {
                duration = UnityEngine.Random.Range(buff.minTime,buff.maxTime+1);
                break;
            }
        }
        
        for(int i = 0; i<effects.Count; i++)
        {
            if (effects[i] is BuffAction buff)
            {
                buff.SetDuration(duration);
            }
        }

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
        action.stringValue = stringValue;
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
    public override void Apply(LivingEntity entity)
    {
        ModifierContext context = entity.GetContext();
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
