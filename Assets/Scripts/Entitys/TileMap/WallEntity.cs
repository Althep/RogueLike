using UnityEngine;

public class WallEntity : MapEntity
{
    public int spriteIndex = 0;

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
