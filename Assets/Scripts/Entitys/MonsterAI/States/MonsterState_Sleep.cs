using UnityEngine;

public class MonsterState_Sleep : MonsterBaseState
{
    public MonsterState_Sleep(Astar pathFinder) : base(pathFinder)
    {
        this.pathFinder = pathFinder;
    }



    public override Vector2Int Get_Destination(Vector2Int myPos, Vector2Int target)
    {
        return Vector2Int.zero;
    }
}
