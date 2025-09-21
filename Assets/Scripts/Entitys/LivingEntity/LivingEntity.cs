using UnityEngine;

public class LivingEntity : MapEntity
{
    EntityData myData;
    public float attackRange;

    public EntityData Get_MyData()
    {
        return myData;
    }
}
