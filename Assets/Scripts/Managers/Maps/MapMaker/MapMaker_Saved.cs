using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using static Defines;

public class MapMaker_Saved : MapMaker
{
    // 로드된 데이터를 MapObjSpawn 등 다른 곳에서 디테일하게 꺼내 쓸 수 있도록 캐싱
    public Dictionary<Vector2Int, TileEntityData> leftData;

    public MapMaker_Saved(MapManager manager) 
    {
        makerType = Defines.MapMakeType.Saved;
    }

    public override async UniTask<Dictionary<Vector2Int, TileType>> MapMake()
    {
        leftData = new Dictionary<Vector2Int, TileEntityData>();
        Dictionary<Vector2Int, TileType> envData = new Dictionary<Vector2Int, TileType>();

        FloorSaveData loadedData = SaveDataManager.instance.Get_FloorSaveData(DungeonManager.instance.Get_Floor());
        List<TileEntityData> mapSaveData = loadedData.tileDatas;
        // 1. JSON 역직렬화

        xSize = loadedData.width;
        ySize = loadedData.height;
        // 2. 바닥 타일 데이터 복구 (-1이 아니면 Tile로 취급)

        for(int i = 0; i<mapSaveData.Count; i++)
        {
            int x = mapSaveData[i].x;
            int y = mapSaveData[i].y;
            Vector2Int pos = new Vector2Int(x, y);
            envData[pos] = TileType.Tile;
            if (mapSaveData[i].type == TileType.Tile)
                continue;

            envData[pos] = mapSaveData[i].type;

            switch (mapSaveData[i].type)
            {
                case TileType.Door:
                    leftData.Add(pos, mapSaveData[i]);
                    break;
                case TileType.Upstair:
                    leftData.Add(pos, mapSaveData[i]);
                    break;
                case TileType.DownStair:
                    leftData.Add(pos, mapSaveData[i]);
                    break;
                default:
                    break;
            }

        }
        
        // 비동기 흐름 제어 (맵 메이커 인터페이스 규격용)
        await UniTask.Yield();

        return envData;
    }
    
}