using UnityEngine;

public class DownStairEntity : StairEntity
{
    

    protected override void Init()
    {
        base.Init();
        myType = Defines.TileType.DownStair;
    }



    public override void ResetData()
    {
        stairNumber = 0;
    }

    public override TileEntityData Get_SaveData()
    {
        Vector2Int posKey = Get_PosKey();
        TileEntityData data = new TileEntityData() { x = posKey.x, y = posKey.y, stairNumber = this.stairNumber, type = GetMyType() };


        return data;
    }

    public override async void Interact()
    {
        PlayerEntity player = GameManager.instance.Get_PlayerEntity();
        player.IneractStair(GetMyType(), stairNumber);
        SaveDataManager.instance.SaveAllFloorDatas();
        await DungeonManager.instance.ChangeFloor(1);
    }

    public override void Return()
    {
        PoolManager.instance.Return(GetMyType(), this.gameObject);
    }
}
