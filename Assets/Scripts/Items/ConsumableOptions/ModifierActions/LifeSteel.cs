using UnityEngine;

public class LifeSteel : ModifierAction
{

    public float damage = 0;
    public void Set_Value(float value)
    {
        this.value = value;
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public override void Excute(LivingEntity entity)
    {
        CombatInfo info = CombatManager.instance.info;
        if(entity == info.attacker)
        {
            damage = info.damage;
            int healValue = Mathf.RoundToInt(damage*value);
            entity.AddingStat(Defines.StatType.HP, healValue);
        }
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override void SetValueToString()
    {
        throw new System.NotImplementedException();
    }


}
