using UnityEngine;

public class TileEntity : MapEntity
{
    public GameObject upperObj;
    public int spriteIndex = 0;
    private void Awake()
    {

    }


    void Init()
    {

    }
    public override MapEntity CopyToEmpty()
    {
        return new TileEntity();
    }

    public override void ResetData()
    {
        upperObj = null;
    }

    public override void Return()
    {
        PoolManager.instance.Return(GetMyType(), this.gameObject);
    }

    public override TileEntityData Get_SaveData()
    {
        Vector2Int posKey = Get_PosKey();
        TileEntityData data = new TileEntityData() { x = posKey.x, y = posKey.y, type = GetMyType() };


        return data;
    }
}

