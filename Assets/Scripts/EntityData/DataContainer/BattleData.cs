using UnityEngine;

public class BattleData : IDataContainer
{
    public LivingEntity attacker;
    public LivingEntity defender;
    public int value;

    BattleData(LivingEntity myEntity)
    {

    }

    public void Reset()
    {
        defender = null;
        value = 0;
    }
}
