using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using static Defines;

public class MapMaker_Saved : MapMaker
{
    // 로드된 데이터를 MapObjSpawn 등 다른 곳에서 디테일하게 꺼내 쓸 수 있도록 캐싱
    public MapSaveData loadedData;

    public MapMaker_Saved(MapManager manager) 
    {
        makerType = Defines.MapMakeType.Saved;
    }

    public override async UniTask<Dictionary<Vector2Int, TileType>> MapMake()
    {
        Dictionary<Vector2Int, TileType> envData = new Dictionary<Vector2Int, TileType>();

        MapSaveData loadedData = SaveDataManager.instance.Get_FloorSaveData(DungeonManager.instance.Get_Floor()).mapData;

        // 1. JSON 역직렬화

        int w = loadedData.width;

        // 2. 바닥 타일 데이터 복구 (-1이 아니면 Tile로 취급)
        for (int i = 0; i < loadedData.tileIds.Length; i++)
        {
            if (loadedData.tileIds[i] != -1)
            {
                int x = i % w;
                int y = i / w;
                envData[new Vector2Int(x, y)] = TileType.Tile;
            }
        }

        // 3. 벽 데이터 복구 (타일 위에 덮어쓰거나 추가)
        for (int i = 0; i < loadedData.wallIds.Length; i++)
        {
            if (loadedData.wallIds[i] != -1)
            {
                int x = i % w;
                int y = i / w;
                envData[new Vector2Int(x, y)] = TileType.Wall;
            }
        }

        // 4. 특수 오브젝트 (문, 계단 등) 데이터 복구
        if (loadedData.specialObjs != null)
        {
            foreach (var special in loadedData.specialObjs)
            {
                envData[new Vector2Int(special.x, special.y)] = special.tileType;
            }
        }

        // 비동기 흐름 제어 (맵 메이커 인터페이스 규격용)
        await UniTask.Yield();

        return envData;
    }

}