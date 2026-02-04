using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using static Defines;
public interface IItemEffect
{
    public Modifier GetMyModifier();
    public void Excute(LivingEntity entity);
}
public interface IItemAction
{
    void Execute(LivingEntity entity);
}
public abstract class ModifierAction : IPoolScript
{
    public string effectID;
    public string effectName;
    public string stringValue;
    public float value;
    public bool isMulti;
    protected PoolScriptType poolType;
    public ModifierTriggerType modifierTrigger;
    public ActionEffectType actionEffectType;
    public int minTime;
    public int maxTime;
    protected ModifierManager mm = GameManager.instance.Get_ModifierManager();
    public virtual Modifier GetMyModifier()
    {
        return null;
    }
    public virtual void Excute(LivingEntity entity)
    {

    }

    public abstract void SetScriptType(PoolScriptType type);
    public abstract PoolScriptType GetScriptType();
    public abstract void SetModifier();
    public abstract void Reset();
}

public class StatAdd : ModifierAction
{
    public StatType statType;
    public StatAdd()
    {
        actionEffectType = ActionEffectType.StatAdd;
    }
    public override Modifier GetMyModifier()
    {
        return null;
    }
    public override void Excute(LivingEntity entity)
    {
        Utils.StringToEnum(stringValue, ref statType);
        entity.AddingStat(statType, value);

    }

    public override void SetScriptType(PoolScriptType type)
    {
        poolType = type;
    }

    public override PoolScriptType GetScriptType()
    {
        return poolType;
    }

    public override void Reset()
    {
        effectID = null;
        effectName = null;
        stringValue = null;
        value = 0f;
        isMulti = false;
    }

    public override void SetModifier()
    {
        Utils.StringToEnum<StatType>(stringValue,ref statType);
    }

    
}


public class BuffEffect : ModifierAction
{
    public StatType statType;
    public BuffModifier modifier;
    public ModifierTriggerType trigger;
    public StatusEffect statusEffect;
    public int duration;

    public BuffEffect()
    {
        actionEffectType = ActionEffectType.BuffEffect;
    }

    public override void Excute(LivingEntity target)
    {
        if(modifier == null)
        {
            SetModifier();
        }
        modifier.duration = SetDuration();
        target.AddModifier(modifier);
        EventManager.instance.AddBuff(target, modifier, duration);
    }
    public int SetDuration()
    {
        return UnityEngine.Random.Range(minTime, maxTime);
    }
    public override PoolScriptType GetScriptType()
    {
        return poolType;
    }

    public override void Reset()
    {
        effectID = null;
        effectName = null;
        stringValue = null;
        value = 0f;
        isMulti = false;
        modifier = null;
        statusEffect = null;
    }

    public override void SetModifier()
    {
        modifier = (BuffModifier)mm.GetModifierPooler().GetModifier(ModifierType.BuffModifier, effectID);
    }

    public override void SetScriptType(PoolScriptType type)
    {
        poolType = type;

        modifier = (BuffModifier)mm.GetModifierPooler().GetModifier(ModifierType.BuffModifier, effectID);


    }
}

public class SlotBlock : ModifierAction
{
    protected SlotType targetSlot;
    public override PoolScriptType GetScriptType()
    {
        return poolType;
    }

    public override void Reset()
    {
        effectID = null;
        effectName = null;
        stringValue = null;
        value = 0;
        isMulti = false;
        modifierTrigger = ModifierTriggerType.Passive;
        minTime = 0;
        maxTime = 0;

    }

    public override void SetModifier()
    {
        Utils.StringToEnum(stringValue, ref targetSlot);
    }

    public override void SetScriptType(PoolScriptType type)
    {
        throw new NotImplementedException();
    }
}


