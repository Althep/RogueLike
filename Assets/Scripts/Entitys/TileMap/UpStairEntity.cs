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
}
