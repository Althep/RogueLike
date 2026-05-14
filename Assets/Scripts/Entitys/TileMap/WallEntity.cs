using UnityEngine;

public class WallEntity : MapEntity
{
    public int spriteIndex = 0;

    protected override void Init()
    {
        base.Init();
        myType = Defines.TileType.Wall;
    }




    public override void ResetData()
    {
        
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
