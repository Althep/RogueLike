using UnityEngine;

public class WallEntity : MapEntity
{
    

    protected override void Init()
    {
        base.Init();
        myType = Defines.TileType.Wall;
    }



    public override MapEntity CopyToEmpty()
    {
        return new WallEntity();
    }

    public override void ResetData()
    {
        
    }
}
