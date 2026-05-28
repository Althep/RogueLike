using UnityEngine;
using System;
using System.Collections.Generic;
public class MonsterBaseState 
{
    protected PathFinder pathFinder;

    public MonsterBaseState(PathFinder pathFinder)
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

    public virtual List<Node> Get_Path()
    {
        return pathFinder.Return_Path();
    }
}
