using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
public abstract class MapEntity : MonoBehaviour, ITargetable
{
    public Defines.TileType myType;

    protected bool isDead = false;
    protected FogOfWar fogOfWar;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected SpriteRenderer[] equipRenders;
    [SerializeField] protected Vector2 myPos;
    [SerializeField] protected Vector2Int posKey;
    [SerializeField] protected GameObject myUIs;
    [SerializeField] protected EntityUIController myUIController;
    string tileName;

    public bool IsValid => this.gameObject.activeInHierarchy && !isDead;

    public Vector2Int GridPos => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

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

        if (myUIController == null && myUIs !=null)
        {
            myUIController = myUIs.GetComponent<EntityUIController>();
        }
    }
    public void SetMyPos(Vector2 pos)
    {
        myPos = pos;
        posKey = new Vector2Int(Mathf.RoundToInt(myPos.x), Mathf.RoundToInt((myPos.y)));
    }

    public void SetMyTileType(Defines.TileType type)
    {
        myType = type;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public FogOfWar GetFOW()
    {
        if (fogOfWar == null)
        {
            fogOfWar = new FogOfWar(this.gameObject);
        }
        return fogOfWar;
    }

    public virtual void Return()
    {
        Vector2 pos = transform.position;
        Vector2Int posKey = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        //MapManager.instance.RemoveMapData(posKey,this);
    }

    public abstract MapEntity CopyToEmpty();
    public abstract void ResetData();
    public Defines.TileType GetMyType()
    {
        return myType;
    }
    public virtual Vector2Int Get_PosKey()
    {

        posKey = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));


        return posKey;
    }

    public virtual SpecialObjectData Get_SaveData()
    {
        return new SpecialObjectData();
    }
}
