using UnityEngine;

public static class Defines
{
    public enum UIEvents
    {
        Click,
        DragStart,
        Drag,
        DragEnd
    }

    public enum TileType
    {
        Tile,
        Wall,
        Door,
        ShallowWater,
        DeepWater,
        Upstair,
        DownStair,
        Player,
        Monster,
        Item
    }
    
    public enum MapMakeType
    {
        Divide
    }

    public enum DungeonTheme
    {
        RuinedDungeon
    }

    public enum VisibleState
    {
        Visible,
        Discovered,
        Hidden
    }

}
