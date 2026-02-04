using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public abstract class MapEntity:MonoBehaviour
{
    public Defines.TileType myType;

    protected FogOfWar fogOfWar;

    
    private void Awake()
    {
        Init();
    }
    protected virtual void Init()
    {
        if (fogOfWar == null)
        {
            fogOfWar = new FogOfWar(this.gameObject);
        }
    }


    public void SetMyTileType(Defines.TileType type)
    {
        myType = type;
    }


    public FogOfWar GetFOW()
    {
        if(fogOfWar == null)
        {
            fogOfWar = new FogOfWar(this.gameObject);
        }
        return fogOfWar;
    }

    public void Return()
    {
        
    }

    public abstract MapEntity CopyToEmpty();
    public abstract void ResetData();
    public Defines.TileType GetMyType() 
    {
        return myType;
    }
}
