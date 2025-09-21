using System.Collections.Generic;
using UnityEngine;
using static Defines;

public class MapMaker_Divide : MapMaker
{
    int maxCount = 5;
    int dividingMin = 80;
    int dividingMax = 120;
    public override Dictionary<Vector2Int, Defines.TileType> MapMake()
    {
        Set_MapSize();
        Dictionary<Vector2Int, Defines.TileType> mapData = new Dictionary<Vector2Int, Defines.TileType>();
        InitMap(mapData);
        Dividing(0, 0, xSize, ySize, 0, mapData);
        return mapData;
    }

    int Get_AroumdHalf(int target)
    {
        int value = target;

        value = target/2;


        return value; 
    }

    public void Dividing(int startX, int startY,int endX,int endY,int count,Dictionary<Vector2Int,Defines.TileType> mapData)
    {
        if(count < maxCount && endX - startX>5 && endY-startY>5)
        {
            if((endX - startX) > (endY-startY))
            {
                count++;
                int divided = ((startX+endX)*UnityEngine.Random.Range(dividingMin, dividingMax)) / 200;
                divided = RerollDivied(startX,endX,divided);
                for(int y = startY; y<endY; y++)
                {
                    Vector2Int pos = new Vector2Int(divided, y);
                    if (mapData.ContainsKey(pos))
                    {
                        mapData[pos] = Defines.TileType.Wall;
                    }
                    else
                    {
                        mapData.Add(pos, Defines.TileType.Wall);
                    }
                }
                int door = UnityEngine.Random.Range(startY, endY);
                Vector2Int doorPos = new Vector2Int(divided, door);
                if (mapData.ContainsKey(doorPos))
                {
                    mapData[doorPos] = Defines.TileType.Door;
                }
                else
                {
                    mapData.Add(doorPos, Defines.TileType.Door);
                }
                if(divided - startX > 3)
                {
                    Dividing(startX, startY, divided, endY, count,mapData);
                }
                if(endX - divided > 3)
                {
                    Dividing(divided, startY, endX, endY, count,mapData);
                }
            }
            else
            {
                count++;
                int divided = ((startY + endY) * UnityEngine.Random.Range(dividingMin, dividingMax)) / 200;
                divided = RerollDivied(startY, endY, divided);
                for (int x = startX; x < endX; x++)
                {
                    Vector2Int pos = new Vector2Int(x, divided);
                    if (mapData.ContainsKey(pos))
                    {
                        mapData[pos] = Defines.TileType.Wall;
                    }
                    else
                    {
                        mapData.Add(pos, Defines.TileType.Wall);
                    }
                }
                int door = UnityEngine.Random.Range(startX, endX);
                Vector2Int doorPos = new Vector2Int(door, divided);
                if (mapData.ContainsKey(doorPos))
                {
                    mapData[doorPos] =  TileType.Door;
                }
                else
                {
                    mapData.Add(doorPos, TileType.Door);
                }
                
                if (divided - startY > 3)
                {
                    Dividing(startX, startY, endX, divided, count,mapData);
                }
                if (endY - divided > 3)
                {
                    Dividing(startX, divided, endX, endY, count,mapData);
                }
            }
        }
    }
    void TwistDungeon(Dictionary<Vector2Int,Defines.TileType> mapData)
    {
        int constant;
        foreach(Vector2Int key in mapData.Keys)
        {
            switch (mapData[key])
            {
                case TileType.Tile:
                    constant = 10;
                    if (UnityEngine.Random.Range(0, 100)<constant)
                    {
                        mapData[key] = TileType.Wall;
                    }
                    break;
                case TileType.Wall:
                    constant = 30;
                    if(UnityEngine.Random.Range(0, 100)<constant)
                    {
                        mapData[key] = TileType.Tile;
                    }
                    break;
                case TileType.Door:
                    break;
                case TileType.ShallowWater:
                    break;
                case TileType.DeepWater:
                    break;
                case TileType.Upstair:
                    break;
                case TileType.DownStair:
                    break;
                case TileType.Player:
                    break;
                case TileType.Monster:
                    break;
                case TileType.Item:
                    break;
                default:
                    break;
            }
        }
    }
    int RerollDivied(int start, int end, int divide)
    {
        if(start>=divide || end<=divide|| start+1 == divide || end-1 == divide)
        {
            return (start+end)/2;
        }
        else
        {
            return divide;
        }
    }
}
