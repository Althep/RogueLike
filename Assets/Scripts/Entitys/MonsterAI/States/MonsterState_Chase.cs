using UnityEngine;
using System;
using System.Collections.Generic;
public class MonsterState_Chase : MonsterBaseState
{
    public MonsterState_Chase(Astar pathFinder) : base(pathFinder)
    {
        this.pathFinder = pathFinder;
    }


    public override Vector2Int Get_Destination(Vector2Int myPos, Vector2Int target)
    {
        List<Node> nods = pathFinder.Get_Path(target);
        if(nods.Count < 2)
        {
            Debug.Log($"Node Count 0 return myPos {myPos}");
            return myPos;
        }
        else
        {
            Debug.Log($"My Pos {myPos} return pos {nods[1].x },{nods[1].y} ");
            return new Vector2Int(nods[1].x,nods[1].y);
        }
    }
}
