using UnityEngine;
using System;
using System.Collections.Generic;
public class MonsterState_Idle : MonsterBaseState
{
    MapManager mapManager;
    public MonsterState_Idle(Astar pathFinder) : base(pathFinder)
    {
        this.pathFinder = pathFinder;
    }


    public override Vector2Int Get_Destination(Vector2Int myPos,Vector2Int target)
    {
        if(mapManager == null)
        {
            mapManager = GameManager.instance.Get_MapManager();
        }

        List<Vector2Int> arounds = mapManager.Get_AroundOnlyTile(myPos);
        if (arounds.Count == 0)
            return myPos;
        else
        {
            int index = UnityEngine.Random.Range(0, arounds.Count);
            return arounds[index];
        }
    }

}
