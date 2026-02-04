using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using static Defines;



public class MapMaker 
{
    public MapMakeType makerType;
    protected int minSize = 40;
    protected int maxSize = 60;
    public int xSize;
    public int ySize;
    public MapManager mapManager;
    public int upStairCount = 3 ;
    public int downStairCount = 3 ;


    protected virtual void Init()
    {
        Set_MapSize();
    }

    public async virtual UniTask<Dictionary<Vector2Int,TileType>> MapMake()
    {
        var mapData = new TileType[Set_MapLength(), Set_MapHeight()];
        //x,y로 저장
        
        return ArrayToDictionary(mapData);
    }
    protected TileType[,] InitMap (TileType[,] mapData)
    {
        for(int y = 0; y< ySize; y++)
        {
            for(int x = 0; x< xSize; x++)
            {
                mapData[x, y] = TileType.Tile;
            }
        }
        return mapData;
    }
    protected TileType[,] MapBorder(TileType[,] mapData)
    {
        /*
        for(int x = 0; x < xSize; x++)
        {
            mapData[x,0] = TileType.Wall;
            mapData[x,ySize] = TileType.Wall;
        }
        for(int y = 0; y<ySize; ySize++)
        {
            mapData[0,y] = TileType.Wall;
            mapData[xSize,y] = TileType.Wall;
        }
        */
        return mapData;
    }
    protected TileType[,] InitMapWall(TileType[,] mapData)
    {
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                mapData[x,y] = TileType.Wall;
            }
        }
        return mapData;
    }

    protected void MakeRandomStair(TileType[,] mapArray)
    {
        int xSize = mapArray.GetLength(0);
        int ySize = mapArray.GetLength(1);

        // 1. 바닥 타일 위치 리스트 확보 (Vector2Int 사용)
        List<Vector2Int> tilePosList = new List<Vector2Int>(xSize * ySize / 2); // 미리 넉넉히 할당

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (mapArray[x, y] == TileType.Tile)
                {
                    tilePosList.Add(new Vector2Int(x, y));
                }
            }
        }

        int totalNeed = upStairCount + downStairCount;
        if (tilePosList.Count < totalNeed)
        {
            Debug.LogError("계단을 설치할 바닥 타일이 부족합니다!");
            return;
        }

        // 2. 피셔-예이츠 셔플 (Fisher-Yates Shuffle) 적용
        // 셔플을 하면 앞에서부터 필요한 만큼만 꺼내 쓰면 되므로 HashSet이 필요 없습니다.
        for (int i = 0; i < totalNeed; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, tilePosList.Count);
            Vector2Int temp = tilePosList[i];
            tilePosList[i] = tilePosList[randomIndex];
            tilePosList[randomIndex] = temp;
        }

        // 3. 계단 배치
        for (int i = 0; i < upStairCount; i++)
        {
            mapArray[tilePosList[i].x, tilePosList[i].y] = TileType.Upstair;
        }

        for (int i = upStairCount; i < totalNeed; i++)
        {
            mapArray[tilePosList[i].x, tilePosList[i].y] = TileType.DownStair;
        }
    }

    protected int[,] Set_MapSize()
    {
        return new int[Set_MapLength(), Set_MapHeight()];
    }

    int Set_MapLength()
    {
        xSize = UnityEngine.Random.Range(minSize, maxSize);
        return xSize;
    }
    int Set_MapHeight()
    {
        ySize =  UnityEngine.Random.Range(minSize, maxSize);
        return ySize;
    }

    protected Dictionary<Vector2Int, TileType> ArrayToDictionary(TileType[,] array)
    {
        var dict = new Dictionary<Vector2Int, TileType>();
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
                dict.Add(new Vector2Int(x, y), array[x,y]);
        return dict;
    }

}
