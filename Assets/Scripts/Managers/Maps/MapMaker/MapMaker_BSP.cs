using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using static Defines;

public class RoomNode
{
    public int xMin, yMin, xMax, yMax;
    public Vector2Int RoomCenter;
    public RoomNode ChildA;
    public RoomNode ChildB;
    public bool IsLeaf;

    public RoomNode(int x1, int y1, int x2, int y2)
    {
        xMin = x1; yMin = y1; xMax = x2; yMax = y2;
        RoomCenter = Vector2Int.zero;
        IsLeaf = true;
    }
}

public class MapMaker_BSP : MapMaker
{
    // --- 설정값 조정 ---
    int divideMaxDepth = 6;       // 너무 깊으면 방이 너무 작아지므로 6 정도가 적당
    int minNodeSize = 8;          // 최소 노드 크기
    int corridorWidth = 2;        // 통로 너비 (규칙성을 줄이기 위해 약간 슬림하게 조정 가능)

    private RoomNode rootNode;
    private List<RoomNode> leafNodes;
    protected Vector2Int[] directions = {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0)
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

        InitMapWall(mapArray);

        rootNode = new RoomNode(0, 0, xSize, ySize);
        leafNodes = new List<RoomNode>();
        Dividing(rootNode, 0);

        await GenerateRoomsAndCorridors(mapArray);
        await PostProcessBFS(mapArray);

        MapBorder(mapArray);
        MakeRandomStair(mapArray);

        return ArrayToDictionary(mapArray);
    }

    // --- 1. 분할 로직 개선 (무작위성 강화) ---
    void Dividing(RoomNode node, int depth)
    {
        int width = node.xMax - node.xMin;
        int height = node.yMax - node.yMin;

        // 분할 중단 조건
        if (depth >= divideMaxDepth || (width < minNodeSize * 2 && height < minNodeSize * 2))
        {
            leafNodes.Add(node);
            return;
        }

        // 분할 방향 결정 (긴 쪽을 주로 쪼개되 10% 확률로 반대 방향 쪼개기)
        bool splitVertical = (width > height);
        if (width > height && (float)width / height >= 1.25f) splitVertical = true;
        else if (height > width && (float)height / width >= 1.25f) splitVertical = false;
        else splitVertical = UnityEngine.Random.value > 0.5f;

        // 분할 지점을 30% ~ 70% 사이에서 랜덤하게 결정 (규칙성 파괴의 핵심)
        float splitRatio = UnityEngine.Random.Range(0.35f, 0.65f);

        node.IsLeaf = false;
        if (splitVertical)
        {
            int splitPos = node.xMin + (int)(width * splitRatio);
            node.ChildA = new RoomNode(node.xMin, node.yMin, splitPos, node.yMax);
            node.ChildB = new RoomNode(splitPos, node.yMin, node.xMax, node.yMax);
        }
        else
        {
            int splitPos = node.yMin + (int)(height * splitRatio);
            node.ChildA = new RoomNode(node.xMin, node.yMin, node.xMax, splitPos);
            node.ChildB = new RoomNode(node.xMin, splitPos, node.xMax, node.yMax);
        }

        Dividing(node.ChildA, depth + 1);
        Dividing(node.ChildB, depth + 1);
    }

    // --- 2. 방 생성 로직 개선 (노드 내 무작위 배치) ---
    async UniTask GenerateRoomsAndCorridors(TileType[,] mapArray)
    {
        int yieldCount = 0;
        foreach (var node in leafNodes)
        {
            // 15% 확률로 방을 스킵 (더 복잡한 동선 유도)
            if (UnityEngine.Random.value < 0.15f) continue;

            int nodeW = node.xMax - node.xMin;
            int nodeH = node.yMax - node.yMin;

            // 방의 크기를 노드 크기의 60% ~ 90% 사이로 랜덤 결정
            int roomWidth = UnityEngine.Random.Range(minNodeSize - 2, nodeW - 2);
            int roomHeight = UnityEngine.Random.Range(minNodeSize - 2, nodeH - 2);

            // 노드 내에서 방을 구석이나 중앙 등 랜덤하게 배치
            int roomXMin = node.xMin + UnityEngine.Random.Range(1, nodeW - roomWidth);
            int roomYMin = node.yMin + UnityEngine.Random.Range(1, nodeH - roomHeight);
            int roomXMax = roomXMin + roomWidth;
            int roomYMax = roomYMin + roomHeight;

            // 최소 크기 안전장치
            if (roomXMax - roomXMin < 4) roomXMax = roomXMin + 4;
            if (roomYMax - roomYMin < 4) roomYMax = roomYMin + 4;

            for (int x = roomXMin; x < roomXMax; x++)
            {
                for (int y = roomYMin; y < roomYMax; y++)
                {
                    if (x > 0 && x < xSize - 1 && y > 0 && y < ySize - 1)
                    {
                        mapArray[x, y] = TileType.Tile;
                        if (++yieldCount % 500 == 0) await UniTask.Yield();
                    }
                }
            }
            // 방의 중심점을 실제 생성된 방의 중앙으로 갱신
            node.RoomCenter = new Vector2Int((roomXMin + roomXMax) / 2, (roomYMin + roomYMax) / 2);
        }

        ConnectNodes(rootNode, mapArray);
    }

    void ConnectNodes(RoomNode node, TileType[,] mapArray)
    {
        if (node.IsLeaf) return;

        ConnectNodes(node.ChildA, mapArray);
        ConnectNodes(node.ChildB, mapArray);

        // 자식 노드가 방을 생성하지 않았을 경우를 대비해 유효한 Center 전파
        if (node.ChildA.RoomCenter == Vector2Int.zero) node.ChildA.RoomCenter = FindLeafCenter(node.ChildA);
        if (node.ChildB.RoomCenter == Vector2Int.zero) node.ChildB.RoomCenter = FindLeafCenter(node.ChildB);

        CreateWideCorridor(mapArray, node.ChildA.RoomCenter, node.ChildB.RoomCenter);
        node.RoomCenter = node.ChildA.RoomCenter;
    }

    // 방이 없는 노드일 경우 자식의 센터를 찾아오는 헬퍼 함수
    Vector2Int FindLeafCenter(RoomNode node)
    {
        if (node.IsLeaf) return new Vector2Int((node.xMin + node.xMax) / 2, (node.yMin + node.yMax) / 2);
        return FindLeafCenter(node.ChildA);
    }

    // --- 3. 통로 생성 로직 개선 (꺾임 지점 랜덤화) ---
    void CreateWideCorridor(TileType[,] mapArray, Vector2Int start, Vector2Int end)
    {
        if (start == Vector2Int.zero || end == Vector2Int.zero) return;

        Vector2Int current = start;
        // X 이동 후 Y 이동할지, Y 이동 후 X 이동할지 랜덤 결정 (직선성 방지)
        bool xFirst = UnityEngine.Random.value > 0.5f;

        if (xFirst)
        {
            MoveX( current.x, end.x, current.y, mapArray);
            MoveY( current.y, end.y, current.x, mapArray);
        }
        else
        {
            MoveY( current.y, end.y, current.x, mapArray);
            MoveX( current.x, end.x, current.y, mapArray);
        }
    }

    void MoveX(int currentX, int targetX, int y, TileType[,] mapArray)
    {
        while (currentX != targetX)
        {
            currentX += (currentX < targetX) ? 1 : -1;
            for (int dy = -(corridorWidth / 2); dy <= (corridorWidth / 2); dy++)
            {
                int ty = y + dy;
                if (currentX > 0 && currentX < xSize - 1 && ty > 0 && ty < ySize - 1)
                    mapArray[currentX, ty] = TileType.Tile;
            }
        }
    }

    void MoveY( int currentY, int targetY, int x, TileType[,] mapArray)
    {
        while (currentY != targetY)
        {
            currentY += (currentY < targetY) ? 1 : -1;
            for (int dx = -(corridorWidth / 2); dx <= (corridorWidth / 2); dx++)
            {
                int tx = x + dx;
                if (tx > 0 && tx < xSize - 1 && currentY > 0 && currentY < ySize - 1)
                    mapArray[tx, currentY] = TileType.Tile;
            }
        }
    }

    // --- BFS 후처리 (기존 로직 유지) ---
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
                    if (newRegion.Count > 5) // 너무 작은 파편 제거
                        allRegions.Add(newRegion);
                    else
                        foreach (var p in newRegion) mapArray[p.x, p.y] = TileType.Wall;
                }
            }
        }

        if (allRegions.Count > 1)
        {
            allRegions.Sort((a, b) => b.Count.CompareTo(a.Count));
            for (int i = 1; i < allRegions.Count; i++)
            {
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
            if (tiles.Count % 1000 == 0) await UniTask.Yield();
        }
        return tiles;
    }
}