using UnityEngine;

public class UpStairEntity : MapEntity
{
    public int stairNumer;

   

    protected override void Init()
    {
        base.Init();
        myType = Defines.TileType.Upstair;
    }


    public override MapEntity CopyToEmpty()
    {
        return new UpStairEntity();
    }

    public override void ResetData()
    {
        stairNumer = 0;
    }

    public override SpecialObjectData Get_SaveData()
    {
        SpecialObjectData saveData = new SpecialObjectData { x = posKey.x, y=posKey.y, tileType = this.GetMyType() };
        saveData.x = posKey.x;
        saveData.y = posKey.y;
        return saveData;
    }
}
