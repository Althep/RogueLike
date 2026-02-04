using UnityEngine;

public class DownStairEntity : MapEntity
{
    public int stairNumber;

    protected override void Init()
    {
        base.Init();
        myType = Defines.TileType.DownStair;
    }

    public override MapEntity CopyToEmpty()
    {
        return new DownStairEntity();
    }

    public override void ResetData()
    {
        stairNumber = 0;
    }
}
