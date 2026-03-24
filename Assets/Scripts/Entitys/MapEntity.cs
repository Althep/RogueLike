using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public abstract class MapEntity:MonoBehaviour
{
    public Defines.TileType myType;

    protected FogOfWar fogOfWar;
    [SerializeField]protected SpriteRenderer[] spriteRenderer;
    [SerializeField] Vector2 myPos;
    protected GameObject myUIs;
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
    public void SetMyPos(Vector2 pos)
    {
        myPos = pos;
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
