using UnityEngine;
using System;
using System.Collections.Generic;
public class PathFinder 
{
    protected List<Node> _path = new List<Node>();
    public virtual List<Node> Get_Path(Vector2Int Dest)
    {
        return null;
    }

    public virtual void Get_Node()
    {

    }

    public virtual List<Node> Return_Path()
    {
        return _path;
    }
}
