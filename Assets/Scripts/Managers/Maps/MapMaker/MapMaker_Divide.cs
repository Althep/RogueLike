using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static Defines;

public class MapMaker_Divide : MapMaker
{
    int maxCount = 5;
    int dividingMin = 80;
    int dividingMax = 120;

    public MapMaker_Divide(MapManager mapManager)
    {
        makerType = MapMakeType.Divide;
        this.mapManager = mapManager;
    }
    public async override UniTask<Dictionary<Vector2Int, Defines.TileType>> MapMake()
    {
        Debug.Log("디바이딩 맵생성 실행 ! ");
        Set_MapSize();
        Debug.Log($"맵사이즈 세팅 X : {xSize} Y : {ySize}");
        TileType[,] mapData = new TileType[xSize,ySize];
        InitMap(mapData);
        await Dividing(0, 0, xSize, ySize, 0, mapData);
        TwistDungeon(mapData);
        MapBorder(mapData);
        
        MakeRandomStair(mapData);
        return ArrayToDictionary(mapData);
    }

    int Get_AroumdHalf(int target)
    {
        int value = target;

        value = target/2;


        return value; 
    }

    public async UniTask Dividing(int startX, int startY,int endX,int endY,int count,TileType[,] mapData)
    {
        int intervalCount = 500;
        if (count < maxCount && endX - startX>5 && endY-startY>5)
        {
            if((endX - startX) > (endY-startY))
            {
                //count++;
                int divided = ((startX+endX)*UnityEngine.Random.Range(dividingMin, dividingMax)) / 200;
                
                divided = RerollDivide(startX,endX,divided);
                
                for(int y = startY; y<endY; y++)
                {
                    mapData[divided, y] = TileType.Wall;
                    if(y%intervalCount == 0)
                    {
                        await UniTask.Yield();
                    }
                }

                int door = UnityEngine.Random.Range(startY, endY);
                Vector2Int doorPos = new Vector2Int(divided, door);
                mapData[doorPos.x, doorPos.y] = TileType.Door;

                

                if (divided - startX > 3)
                {
                    await Dividing(startX, startY, divided, endY, count+1,mapData);
                }
                if(endX - divided > 3)
                {
                    await Dividing(divided, startY, endX, endY, count+1,mapData);
                }
            }
            else
            {
                //count++;
                int divided = ((startY + endY) * UnityEngine.Random.Range(dividingMin, dividingMax)) / 200;
                divided = RerollDivide(startY, endY, divided);
                for (int x = startX; x < endX; x++)
                {
                    mapData[x, divided] = TileType.Wall;
                    if (x % intervalCount == 0)
                    {
                        await UniTask.Yield();
                    }
                }
                int door = UnityEngine.Random.Range(startX, endX);
                Vector2Int doorPos = new Vector2Int(door, divided);
                mapData[doorPos.x, doorPos.y] = TileType.Door;
                
                await UniTask.Yield();
                if (divided - startY > 3)
                {
                    await Dividing(startX, startY, endX, divided, count+1,mapData);
                }
                if (endY - divided > 3)
                {
                    await Dividing(startX, divided, endX, endY, count+1,mapData);
                }
            }
        }
    }
    void TwistDungeon(TileType[,] mapData)
    {
        int constant;

        for(int x = 0; x < mapData.GetLength(0); x++)
        {
            for(int y =0; y < mapData.GetLength(1); y++)
            {
                switch (mapData[x,y])
                {
                    case TileType.Tile:
                        constant = 10;
                        if (UnityEngine.Random.Range(0, 100) < constant)
                        {
                            mapData[x, y] = TileType.Wall;
                        }
                        break;
                    case TileType.Wall:
                        constant = 30;
                        if (UnityEngine.Random.Range(0, 100) < constant)
                        {
                            mapData[x, y] = TileType.Tile;
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
    }
    int RerollDivide(int start, int end, int divide)
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
