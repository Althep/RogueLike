using UnityEngine;
using System;
using System.Collections.Generic;
public interface IMapMaker
{
    public Dictionary<Vector2,Defines.TileType> MapMake();

    protected Vector2 MapSize();
}
