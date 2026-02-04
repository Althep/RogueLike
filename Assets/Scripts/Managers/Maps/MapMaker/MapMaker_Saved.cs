using UnityEngine;
using System;
using System.Collections.Generic;
using static Defines;
public class MapMaker_Saved : MapMaker
{
    public MapMaker_Saved(MapManager mapManager)
    {
        makerType = MapMakeType.Saved;
        this.mapManager = mapManager;
    }
}
