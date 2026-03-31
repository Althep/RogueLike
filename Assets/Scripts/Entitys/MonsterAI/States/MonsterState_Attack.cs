using UnityEngine;

public class MonsterState_Attack : MonsterBaseState
{
    public MonsterState_Attack(Astar pathFinder) : base(pathFinder)
    {
        this.pathFinder = pathFinder;
    }

    public override Vector2Int Get_Destination(Vector2Int myPos, Vector2Int target)
    {
        return myPos;
    }
}
