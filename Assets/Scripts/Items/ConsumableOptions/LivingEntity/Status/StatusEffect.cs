using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public abstract class StatusEffect:IPoolScript
{
    public ModifierTriggerType trigger;
    public EStatusEffect effect;
    public StatuseType stType;
    public string id;
    public virtual void Excute(LivingEntity target)
    {

    }

    public abstract PoolScriptType GetScriptType();
    public abstract void Reset();
    public abstract void SetScriptType(PoolScriptType type);
    
}

public class Confuse : StatusEffect
{
    public Confuse()
    {
        trigger = ModifierTriggerType.OnMove;
        effect = EStatusEffect.Confuse;
        stType = StatuseType.Debuff;
    }
    public override void Excute(LivingEntity target)
    {
        Vector2 pos = target.transform.position;
        List<Vector2Int> targets = GameManager.instance.Get_MapManager().GetAroundTile(pos);
        Vector2Int randomPos = targets[UnityEngine.Random.Range(0, targets.Count - 1)];
        target.SetDestination(randomPos);
    }

    public override PoolScriptType GetScriptType()
    {
        throw new NotImplementedException();
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void SetScriptType(PoolScriptType type)
    {
        throw new NotImplementedException();
    }
}

public class Poison : StatusEffect
{
    public Poison()
    {
        trigger = ModifierTriggerType.OnTurnEnd;
        effect = EStatusEffect.Poison;
        stType = StatuseType.Debuff;
    }
    public override void Excute(LivingEntity target)
    {
        base.Excute(target);

        //재생속도- 혹은 대미지
    }

    public override PoolScriptType GetScriptType()
    {
        throw new NotImplementedException();
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void SetScriptType(PoolScriptType type)
    {
        throw new NotImplementedException();
    }
}

public class StatEffect : StatusEffect
{
    public StatModifier modifier;

    public override PoolScriptType GetScriptType()
    {
        throw new NotImplementedException();
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void SetScriptType(PoolScriptType type)
    {
        throw new NotImplementedException();
    }

    public void Setting()
    {
        if((modifier.stat == StatType.AttackSpeed || modifier.stat == StatType.MoveSpeed || modifier.stat == StatType.SpellSpeed))
        {
            if (modifier.value <0)
            {
                stType = StatuseType.Buff;
            }
            else
            {
                stType = StatuseType.Debuff;
            }
        }
        else
        {
            if (modifier.value > 0)
            {
                stType = StatuseType.Buff;
            }
            else
            {
                stType = StatuseType.Debuff;
            }
        }
    }
}

public class Petrification : StatusEffect
{
    public Petrification()
    {
        trigger = ModifierTriggerType.StartMove;
        effect = EStatusEffect.Petrification;
        stType = StatuseType.Debuff;
    }
    public override void Excute(LivingEntity target)
    {
        Vector2Int pos = new Vector2Int( (int)target.transform.position.x,(int)target.transform.position.y);
        target.SetDestination(pos);
    }

    public override PoolScriptType GetScriptType()
    {
        throw new NotImplementedException();
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void SetScriptType(PoolScriptType type)
    {
        throw new NotImplementedException();
    }
}