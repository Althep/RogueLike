using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MapMaker 
{
    protected int minSize = 40;
    protected int maxSize = 60;
    public int length;
    public int height;

    public virtual Dictionary<Vector2Int, Defines.TileType> MapMake()
    {
        Set_MapHeight();
        Set_MapLength();
        return new Dictionary<Vector2Int, Defines.TileType>();
    }
    protected Dictionary<Vector2Int,Defines.TileType> InitMap (Dictionary<Vector2Int,Defines.TileType> mapData)
    {
        for(int i = 0; i<length; i++)
        {
            for(int j = 0; j<height; j++)
            {
                Vector2Int newKey = new Vector2Int(i, j);
                mapData.Add(newKey, Defines.TileType.Tile);
            }
        }
        return mapData;
    }
    protected Vector2Int Set_MapSize()
    {
        return new Vector2Int(Set_MapLength(), Set_MapHeight());
    }

    int Set_MapLength()
    {
        length = 10;
        length = UnityEngine.Random.Range(minSize, maxSize);


        return length;
    }
    int Set_MapHeight()
    {
        height = 10;
        height =  UnityEngine.Random.Range(minSize, maxSize);
        return height;
    }


    
}
