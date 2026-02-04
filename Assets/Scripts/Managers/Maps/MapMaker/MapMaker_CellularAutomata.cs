using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static Defines;

public class Room
{
    public List<Vector2Int> tiles = new List<Vector2Int>();

    // 이 방의 중앙 위치 (복도 연결 시 사용)
    public Vector2Int center;

    public Room(List<Vector2Int> roomTiles)
    {
        tiles = roomTiles;
        CalculateCenter();
    }

    // 방의 중심점을 계산합니다.
    void CalculateCenter()
    {
        if (tiles.Count == 0) return;

        int totalX = 0;
        int totalY = 0;
        foreach (var pos in tiles)
        {
            totalX += pos.x;
            totalY += pos.y;
        }
        center = new Vector2Int(totalX / tiles.Count, totalY / tiles.Count);
    }
}

public class MapMaker_CellularAutomata : MapMaker
{
    // === 설정 변수 ===
    int initialWallPercent = 55; // 55는 너무 꽉 막힐 수 있어 45~50 권장
    int wallThreshold = 5;
    int simulationSteps = 3; // 보통 5회면 충분히 다듬어집니다.

    protected Vector2Int[] directions = {
    new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1),
    new Vector2Int(0, -1),                         new Vector2Int(0, 1),
    new Vector2Int(1, -1),  new Vector2Int(1, 0),  new Vector2Int(1, 1)
};
    public MapMaker_CellularAutomata(MapManager mapManager)
    {
        makerType = MapMakeType.CellularAutomata;
        this.mapManager = mapManager;
    }

    public async override UniTask<Dictionary<Vector2Int, Defines.TileType>> MapMake()
    {
        Set_MapSize();

        // 1. 배열 2개 생성 (더블 버퍼링용)
        TileType[,] mapA = new TileType[xSize, ySize];
        TileType[,] mapB = new TileType[xSize, ySize];

        // 2. 초기 랜덤 채우기
        RandomFillMapArray(mapA);

        // 3. 셀룰러 오토마타 시뮬레이션
        TileType[,] currentSource = mapA;
        TileType[,] currentDest = mapB;

        for (int i = 0; i < simulationSteps; i++)
        {
            await SmoothMapAsync(currentSource, currentDest);

            // 스왑 (A <-> B)
            TileType[,] temp = currentSource;
            currentSource = currentDest;
            currentDest = temp;

            await UniTask.Yield();
        }

        // 4. 후처리 (BFS 기반 구역 정리 및 연결)
        // 최종 결과는 currentSource에 있습니다.
        await PostProcessMapAsync(currentSource);

        MapBorder(currentSource);
        MakeRandomStair(currentSource);

        // 마지막에만 딕셔너리로 변환
        return ArrayToDictionary(currentSource);
    }

    void RandomFillMapArray(TileType[,] mapArray)
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (x == 0 || x == xSize - 1 || y == 0 || y == ySize - 1)
                    mapArray[x, y] = TileType.Wall;
                else
                    mapArray[x, y] = (UnityEngine.Random.Range(0, 100) < initialWallPercent) ? TileType.Wall : TileType.Tile;
            }
        }
    }

    async UniTask SmoothMapAsync(TileType[,] source, TileType[,] dest)
    {
        for (int x = 0; x < xSize; x++)
        {
            // 성능을 위해 100줄마다 한 번씩만 양보
            if (x % 100 == 0) await UniTask.Yield();

            for (int y = 0; y < ySize; y++)
            {
                // 테두리는 고정
                if (x == 0 || x == xSize - 1 || y == 0 || y == ySize - 1)
                {
                    dest[x, y] = TileType.Wall;
                    continue;
                }

                int wallCount = GetSurroundingWallCount(source, x, y);

                if (wallCount > wallThreshold) dest[x, y] = TileType.Wall;
                else if (wallCount < wallThreshold) dest[x, y] = TileType.Tile;
                else dest[x, y] = source[x, y];
            }
        }
    }

    int GetSurroundingWallCount(TileType[,] map, int gridX, int gridY)
    {
        int wallCount = 0;
        for (int nX = gridX - 1; nX <= gridX + 1; nX++)
        {
            for (int nY = gridY - 1; nY <= gridY + 1; nY++)
            {
                if (nX == gridX && nY == gridY) continue;

                // 인덱스 범위 체크 (최적화: 맵 테두리를 항상 벽으로 뒀으므로 사실 생략 가능)
                if (nX >= 0 && nX < xSize && nY >= 0 && nY < ySize)
                {
                    if (map[nX, nY] == TileType.Wall) wallCount++;
                }
                else wallCount++; // 맵 밖은 벽으로 간주
            }
        }
        return wallCount;
    }

    async UniTask PostProcessMapAsync(TileType[,] mapArray)
    {
        bool[,] visited = new bool[xSize, ySize];
        List<List<Vector2Int>> roomList = new List<List<Vector2Int>>();

        // 1. 모든 독립된 방 찾기 (BFS)
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (mapArray[x, y] == TileType.Tile && !visited[x, y])
                {
                    List<Vector2Int> newRoom = await GetRegionTiles(x, y, mapArray, visited);

                    // 너무 작은 방은 그냥 벽으로 메움 (청소 단계)
                    if (newRoom.Count < 15)
                    {
                        foreach (var tile in newRoom) mapArray[tile.x, tile.y] = TileType.Wall;
                    }
                    else
                    {
                        roomList.Add(newRoom);
                    }
                }
            }
        }

        // 2. 방들끼리 연결 (가까운 방 순서대로 정렬하여 연결)
        if (roomList.Count > 1)
        {
            // 첫 번째 방을 기준으로 가장 가까운 방들을 순차적으로 연결
            for (int i = 0; i < roomList.Count - 1; i++)
            {
                // 중심점끼리 L자 복도 생성
                CreateCorridor(mapArray, GetCenter(roomList[i]), GetCenter(roomList[i + 1]));
                await UniTask.Yield();
            }
        }
    }

    async UniTask<List<Vector2Int>> GetRegionTiles(int startX, int startY, TileType[,] mapArray, bool[,] visited)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            tiles.Add(curr);

            // directions는 부모 클래스에 상하좌우 4방향으로 정의되어 있어야 함
            foreach (var dir in directions)
            {
                int nX = curr.x + dir.x;
                int nY = curr.y + dir.y;

                if (nX >= 0 && nX < xSize && nY >= 0 && nY < ySize && !visited[nX, nY] && mapArray[nX, nY] == TileType.Tile)
                {
                    visited[nX, nY] = true;
                    queue.Enqueue(new Vector2Int(nX, nY));
                }
            }
            if (tiles.Count % 500 == 0) await UniTask.Yield();
        }
        return tiles;
    }

    Vector2Int GetCenter(List<Vector2Int> tiles)
    {
        if (tiles.Count == 0) return Vector2Int.zero;
        long tx = 0, ty = 0;
        foreach (var t in tiles) { tx += t.x; ty += t.y; }
        return new Vector2Int((int)(tx / tiles.Count), (int)(ty / tiles.Count));
    }

    void CreateCorridor(TileType[,] mapArray, Vector2Int start, Vector2Int end)
    {
        Vector2Int curr = start;
        // X 이동
        while (curr.x != end.x)
        {
            curr.x += (curr.x < end.x) ? 1 : -1;
            mapArray[curr.x, curr.y] = TileType.Tile;
        }
        // Y 이동
        while (curr.y != end.y)
        {
            curr.y += (curr.y < end.y) ? 1 : -1;
            mapArray[curr.x, curr.y] = TileType.Tile;
        }
    }
}