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

    public ModifierSaveData SaveData()
    {
        ModifierSaveData saveData = new ModifierSaveData {value = value, modifierId = id };

        return saveData;
    }

    public virtual void Apply(LivingEntity entity)
    {

    }

    public virtual void Return()
    {
        Reset();
        ModifierManager.instance.Return_Modifier(this);
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
        ModifierContext context = entity.GetContext(triggerType);
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
        int applyDuration = (this.duration > 0) ? this.duration : UnityEngine.Random.Range(minTime, maxTime + 1);

        EventManager.instance.AddBuff(entity, this, applyDuration);

        this.duration = 0;
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
        ModifierContext context = entity.GetContext(triggerType);
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
        if (evokeRate >= UnityEngine.Random.Range(0f, 100f))
        {
            return canEvoke = true;
        }
        return canEvoke = false;
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
    public ModifierAction action;
    public string effectName;
    public string stringValue;
    public int minValue;
    public int maxValue;
    public override void Apply(LivingEntity entity)
    {
        ModifierContext context = entity.GetContext(triggerType);
        List<ModifierAction> effects = context.modifierActions;

        bool isAlreadyExist = false;

        // 1. 리스트를 딱 한 번만 돌면서 확인합니다.
        for (int i = 0; i < effects.Count; i++)
        {
            // 내 effectName과 똑같은 버프를 찾았다면?
            if (effects[i] is BuffAction buff && buff.effectName == this.effectName)
            {
                // [수정] 기존 버프의 시간이 아닌, '새로 들어온 이 모디파이어'의 시간으로 갱신!
                int newDuration = UnityEngine.Random.Range(this.minValue, this.maxValue + 1);
                buff.SetDuration(newDuration);

                isAlreadyExist = true; // 찾았다고 깃발(플래그)을 듦
                break; // 이미 갱신했으니 남은 리스트는 더 볼 필요 없이 탈출!
            }
        }

        // 2. 만약 리스트를 다 뒤졌는데도 못 찾았다면(새로운 효과라면) 추가합니다.
        if (!isAlreadyExist)
        {
            if (action != null)
            {
                context.modifierActions.Add(action);
            }
        }
    }

    public void SetConsumableFlag()
    {
        if (action != null)
        {
            action.isConsumable = true;
        }
    }

    public void SetAction(ModifierAction action)
    {
        this.action = action;
        action.stringValue = stringValue;
    }

    public override void Copy(Modifier newOne)
    {
        base.Copy(newOne);
        if (newOne is ActionModifier act)
        {
            act.action = this.action;
            act.effectName = this.effectName;
            act.stringValue = this.stringValue;
            act.minValue = this.minValue;
            act.maxValue = this.maxValue;
        }
    }

    public override void Reset()
    {
        base.Reset();
        action = null;
        effectName = null;
        stringValue = null;
        minValue = 0;
        maxValue = 0;
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
        ModifierContext context = entity.GetContext(triggerType);
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
