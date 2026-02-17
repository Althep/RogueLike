using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public abstract class StatusEffect:ModifierAction
{
    public ModifierTriggerType trigger;
    public EStatusEffect effect;
    public StatuseType stType;

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

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void SetValueToString()
    {
        throw new NotImplementedException();
    }
}

public class GetDamage : StatusEffect
{
    DamageType damageType;
    public GetDamage()
    {
        trigger = ModifierTriggerType.OnTurnEnd;
        effect = EStatusEffect.Poison;
        stType = StatuseType.Debuff;
    }
    public override void Excute(LivingEntity target)
    {
        SetValueToString();
        base.Excute(target);
        target.GetResistDamage(DamageType.Poison, (int)value);
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }



    public override void SetValueToString()
    {
        Utils.StringToEnum<DamageType>(stringValue, ref damageType);
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

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void SetValueToString()
    {
        throw new NotImplementedException();
    }
}