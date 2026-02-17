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
public abstract class ModifierAction 
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

    public abstract void SetValueToString();
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
        SetValueToString();
        entity.AddingStat(statType, value);
    }

    public override void Reset()
    {
        effectID = null;
        effectName = null;
        stringValue = null;
        value = 0f;
        isMulti = false;
    }

    public override void SetValueToString()
    {
        Utils.StringToEnum(stringValue, ref statType);
    }
}

public class BuffAction : ModifierAction
{
    protected int duration;
    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public void SetDuration(int dur)
    {
        duration = dur;
    }
    public override void SetValueToString()
    {
        throw new NotImplementedException();
    }
}

public class StatBuff : BuffAction
{
    public StatType statType;
    public BuffModifier modifier;
    public ModifierTriggerType trigger;
    public StatusEffect statusEffect;

    public override Modifier GetMyModifier()
    {
        modifier = (BuffModifier)mm.GetModifierPooler().GetModifier(ModifierType.BuffModifier, effectID);
        return modifier;
    }
    public override void Excute(LivingEntity target)
    {
        if(modifier == null)
        {
            GetMyModifier();
        }
        target.AddModifier(modifier);
        EventManager.instance.AddBuff(target, modifier, duration);
    }
    public int SetDuration()
    {
        return UnityEngine.Random.Range(minTime, maxTime);
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

    public override void SetValueToString()
    {
        if(stringValue ==null)
        {
            return;
        }
        Utils.StringToEnum<StatType>(stringValue, ref statType);
    }
}

public class SlotBlock : ModifierAction
{
    protected SlotType targetSlot;

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

    public override void SetValueToString()
    {
        throw new NotImplementedException();
    }
}


