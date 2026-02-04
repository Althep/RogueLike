using System.Collections.Generic;
using UnityEngine; // Vector2Int, Random 짓수를 위해 필요합니다.
using System.Linq;
using Cysharp.Threading.Tasks;
using static Defines; // TileType 등을 포함하는 Defines 짓수를 가정합니다.

public class MapMaker_Prim : MapMaker
{
    // 이웃 탐색 방향: 미로 생성에서는 상하좌우 (4방향)만 사용합니다. 와자마에!
    private readonly Vector2Int[] directions = {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1)
    };

    public MapMaker_Prim(MapManager mapManager)
    {
        makerType = MapMakeType.Prim; // 적절한 MapMakeType으로 설정하십시오.
        this.mapManager = mapManager;
    }

    public async override UniTask<Dictionary<Vector2Int, Defines.TileType>> MapMake()
    {
        
        Set_MapSize(); // MapMaker의 xSize, ySize를 설정합니다.

        Dictionary<Vector2Int, Defines.TileType> mapData = new Dictionary<Vector2Int, Defines.TileType>();
        /*
        // 1. 맵 초기화: 맵 전체를 '벽(Wall)'로 채웁니다. 
        // 프림 짓수는 벽에서 시작해서 통로를 뚫어 나갑니다.
        await InitMap(mapData, Defines.TileType.Wall);

        // 2. 미로 생성 핵심 짓수 실행
        await GenerateMaze_Prim(mapData);

        MapBorder(mapData);
        MakeRandomStair(mapData);

        Debug.Log($"프림 알고리즘 기반 던전 생성 완료! 맵 크기: {xSize}x{ySize}");
        */
        return mapData;
    }

    // ====================================================================
    // | A. 헬퍼 짓수 (Helper Jutsu)                                       |
    // ====================================================================

    // 맵 경계 체크 짓수
    bool IsInBounds(Vector2Int pos)
    {
        // 맵 테두리(1칸)는 항상 벽으로 남겨두기 위해 1부터 xSize-2, ySize-2까지만 허용합니다.
        return pos.x > 0 && pos.x < xSize - 1 &&
               pos.y > 0 && pos.y < ySize - 1;
    }

    // InitMap 짓수 (MapMaker 상속 클래스에 없을 경우 추가하십시오)
    async UniTask InitMap(Dictionary<Vector2Int, Defines.TileType> mapData, Defines.TileType type)
    {
        int count = 0;
        int interval = 500;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                mapData[new Vector2Int(x, y)] = type;
                count++;
                if(count%interval == 0)
                {
                    await UniTask.Yield();
                }
            }
        }
    }


    // ====================================================================
    // | B. 프림 미로 생성 핵심 짓수 (Core Prim's Jutsu)                   |
    // ====================================================================

    public async UniTask GenerateMaze_Prim(Dictionary<Vector2Int, Defines.TileType> mapData)
    {
        // 1. 중복 검사를 위한 HashSet (Contains를 O(1)로 만듦)
        List<Vector2Int> frontier = new List<Vector2Int>();
        HashSet<Vector2Int> inFrontier = new HashSet<Vector2Int>();

        // 재사용할 리스트 (매번 new 하지 않음)
        List<Vector2Int> surroundingTiles = new List<Vector2Int>(4);

        Vector2Int startPos = new Vector2Int(
            UnityEngine.Random.Range(1, xSize - 1),
            UnityEngine.Random.Range(1, ySize - 1)
        );

        mapData[startPos] = Defines.TileType.Tile;

        // 시작점 주변 추가
        AddFrontier(startPos, mapData, frontier, inFrontier);

        int yieldCount = 0;

        while (frontier.Count > 0)
        {
            // 비동기 처리: 200번의 단계마다 한 번씩 프레임 양보
            if (yieldCount++ % 200 == 0) await UniTask.Yield();

            // A. 랜덤 선택 및 최적화된 삭제 (마지막 요소와 교체 후 제거)
            int randomIndex = UnityEngine.Random.Range(0, frontier.Count);
            Vector2Int currentWall = frontier[randomIndex];

            // RemoveAt(index) 대신 쓰는 최적화 기법 (Swap with last)
            frontier[randomIndex] = frontier[frontier.Count - 1];
            frontier.RemoveAt(frontier.Count - 1);
            inFrontier.Remove(currentWall);

            // B. 주변 바닥 타일 검사 (리스트 재사용)
            surroundingTiles.Clear();
            foreach (var dir in directions)
            {
                Vector2Int neighbor = currentWall + dir;
                if (IsInBounds(neighbor) && mapData[neighbor] == Defines.TileType.Tile)
                {
                    surroundingTiles.Add(neighbor);
                }
            }

            // C. 조건 충족 시 통로 확장
            if (surroundingTiles.Count == 1)
            {
                mapData[currentWall] = Defines.TileType.Tile;
                AddFrontier(currentWall, mapData, frontier, inFrontier);
            }
        }
    }

    // 프론티어 추가 로직 분리 및 최적화
    void AddFrontier(Vector2Int pos, Dictionary<Vector2Int, TileType> mapData, List<Vector2Int> frontier, HashSet<Vector2Int> inFrontier)
    {
        foreach (var dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (IsInBounds(neighbor) && mapData[neighbor] == Defines.TileType.Wall)
            {
                // HashSet 덕분에 Contains 검사가 거의 실시간으로 끝남
                if (!inFrontier.Contains(neighbor))
                {
                    frontier.Add(neighbor);
                    inFrontier.Add(neighbor);
                }
            }
        }
    }
}