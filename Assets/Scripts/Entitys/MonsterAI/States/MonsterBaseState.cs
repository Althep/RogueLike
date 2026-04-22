using UnityEngine;

public class MonsterBaseState 
{
    protected Astar pathFinder;

    public MonsterBaseState(Astar pathFinder)
    {
        this.pathFinder=pathFinder;
    }

    public virtual void Enter()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void Exit()
    {

    }

    public virtual Vector2Int Get_Destination(Vector2Int myPos, Vector2Int target)
    {
        return target;
    }
}
