using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using static Defines;
public class RoomNode
{
    public int xMin, yMin, xMax, yMax; // 분할된 영역의 경계
    public Vector2Int RoomCenter;      // 생성될 방의 중심 좌표
    public RoomNode ChildA;            // 분할된 자식 노드 A (재귀 연결에 사용)
    public RoomNode ChildB;            // 분할된 자식 노드 B
    public bool IsLeaf;                // 더 이상 분할되지 않는 최종 노드인가?

    // 생성자 (Constructor)
    public RoomNode(int x1, int y1, int x2, int y2)
    {
        xMin = x1; yMin = y1; xMax = x2; yMax = y2;
        RoomCenter = Vector2Int.zero;
        ChildA = default;
        ChildB = default;
        IsLeaf = true;
    }
}
public class MapMaker_BSP : MapMaker
{
    int divideMaxDepth = 5;
    int minNodeSize = 10;
    int corridorWidth = 3;

    private RoomNode rootNode;
    private List<RoomNode> leafNodes;
    protected Vector2Int[] directions = {
    new Vector2Int(0, 1),  // 상
    new Vector2Int(0, -1), // 하
    new Vector2Int(-1, 0), // 좌
    new Vector2Int(1, 0)   // 우
};
    public MapMaker_BSP(MapManager mapManager)
    {
        this.mapManager = mapManager;
        makerType = Defines.MapMakeType.BSP;
    }

    public async override UniTask<Dictionary<Vector2Int, Defines.TileType>> MapMake()
    {
        Set_MapSize();
        TileType[,] mapArray = new TileType[xSize, ySize];

        // 1. 벽으로 초기화
        InitMapWall(mapArray);

        // 2. BSP 분할
        rootNode = new RoomNode(0, 0, xSize, ySize);
        leafNodes = new List<RoomNode>();
        Dividing(rootNode, 0);

        // 3. 방 및 통로 생성 (배열 기반)
        await GenerateRoomsAndCorridors(mapArray);

        // 4. BFS 후처리 (고립된 방 연결 및 작은 방 제거)
        await PostProcessBFS(mapArray);

        MapBorder(mapArray);
        MakeRandomStair(mapArray);

        // 마지막에만 변환
        return ArrayToDictionary(mapArray);
    }

    // --- [BSP 분할 로직은 동일하되 효율적 관리] ---
    void Dividing(RoomNode node, int depth)
    {
        if (depth >= divideMaxDepth || (node.xMax - node.xMin <= minNodeSize * 2) || (node.yMax - node.yMin <= minNodeSize * 2))
        {
            leafNodes.Add(node);
            return;
        }

        int width = node.xMax - node.xMin;
        int height = node.yMax - node.yMin;
        bool splitVertical = width > height;

        int splitPos = splitVertical ? node.xMin + width / 2 : node.yMin + height / 2;

        node.IsLeaf = false;
        if (splitVertical)
        {
            node.ChildA = new RoomNode(node.xMin, node.yMin, splitPos, node.yMax);
            node.ChildB = new RoomNode(splitPos, node.yMin, node.xMax, node.yMax);
        }
        else
        {
            node.ChildA = new RoomNode(node.xMin, node.yMin, node.xMax, splitPos);
            node.ChildB = new RoomNode(node.xMin, splitPos, node.xMax, node.yMax);
        }

        Dividing(node.ChildA, depth + 1);
        Dividing(node.ChildB, depth + 1);
    }

    // --- [방 생성 및 통로 연결 최적화] ---
    async UniTask GenerateRoomsAndCorridors(TileType[,] mapArray)
    {
        int yieldCount = 0;
        foreach (var node in leafNodes)
        {
            int roomXMin = UnityEngine.Random.Range(node.xMin + 2, node.xMax - 4);
            int roomYMin = UnityEngine.Random.Range(node.yMin + 2, node.yMax - 4);
            int roomXMax = UnityEngine.Random.Range(roomXMin + 4, node.xMax - 2);
            int roomYMax = UnityEngine.Random.Range(roomYMin + 4, node.yMax - 2);

            for (int x = roomXMin; x <= roomXMax; x++)
            {
                for (int y = roomYMin; y <= roomYMax; y++)
                {
                    mapArray[x, y] = TileType.Tile;
                    if (++yieldCount % 500 == 0) await UniTask.Yield();
                }
            }
            node.RoomCenter = new Vector2Int((roomXMin + roomXMax) / 2, (roomYMin + roomYMax) / 2);
        }

        ConnectNodes(rootNode, mapArray);
    }

    void ConnectNodes(RoomNode node, TileType[,] mapArray)
    {
        if (node.IsLeaf) return;

        ConnectNodes(node.ChildA, mapArray);
        ConnectNodes(node.ChildB, mapArray);

        CreateWideCorridor(mapArray, node.ChildA.RoomCenter, node.ChildB.RoomCenter);
        node.RoomCenter = node.ChildA.RoomCenter;
    }

    void CreateWideCorridor(TileType[,] mapArray, Vector2Int start, Vector2Int end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x)
        {
            x += (x < end.x) ? 1 : -1;
            for (int dy = -(corridorWidth / 2); dy <= (corridorWidth / 2); dy++)
            {
                int targetY = y + dy;
                if (x >= 0 && x < xSize && targetY >= 0 && targetY < ySize)
                    mapArray[x, targetY] = TileType.Tile;
            }
        }

        while (y != end.y)
        {
            y += (y < end.y) ? 1 : -1;
            for (int dx = -(corridorWidth / 2); dx <= (corridorWidth / 2); dx++)
            {
                int targetX = x + dx;
                if (targetX >= 0 && targetX < xSize && y >= 0 && y < ySize)
                    mapArray[targetX, y] = TileType.Tile;
            }
        }
    }

    // --- [핵심: BFS 기반 고립 구역 후처리] ---
    async UniTask PostProcessBFS(TileType[,] mapArray)
    {
        bool[,] visited = new bool[xSize, ySize];
        List<List<Vector2Int>> allRegions = new List<List<Vector2Int>>();

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (mapArray[x, y] == TileType.Tile && !visited[x, y])
                {
                    List<Vector2Int> newRegion = await GetRegionBFS(x, y, mapArray, visited);
                    allRegions.Add(newRegion);
                }
            }
        }

        // 방이 여러 개로 나뉘어 있다면 가장 큰 방 기준으로 나머지 연결
        if (allRegions.Count > 1)
        {
            allRegions.Sort((a, b) => b.Count.CompareTo(a.Count)); // 크기순 정렬

            for (int i = 1; i < allRegions.Count; i++)
            {
                // 현재 방의 첫 타일과 메인 방(0번)의 첫 타일을 연결
                CreateWideCorridor(mapArray, allRegions[i][0], allRegions[0][0]);
                await UniTask.Yield();
            }
        }
    }

    async UniTask<List<Vector2Int>> GetRegionBFS(int startX, int startY, TileType[,] mapArray, bool[,] visited)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            tiles.Add(curr);

            foreach (var dir in directions) // 상하좌우
            {
                int nX = curr.x + dir.x;
                int nY = curr.y + dir.y;

                if (nX >= 0 && nX < xSize && nY >= 0 && nY < ySize)
                {
                    if (!visited[nX, nY] && mapArray[nX, nY] == TileType.Tile)
                    {
                        visited[nX, nY] = true;
                        queue.Enqueue(new Vector2Int(nX, nY));
                    }
                }
            }
            if (tiles.Count % 1000 == 0) await UniTask.Yield();
        }
        return tiles;
    }
}