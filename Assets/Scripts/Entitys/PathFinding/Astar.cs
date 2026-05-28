using UnityEngine;
using System;
using System.Collections.Generic;

public class Astar:PathFinder
{
    private MapLayer _navLayer;
    private MapEntity _owner;

    // 방향 데이터 (8방향: 상하좌우 + 대각선)
    private readonly int[] dx = { 0, 0, -1, 1, -1, -1, 1, 1 };
    private readonly int[] dy = { 1, -1, 0, 0, 1, -1, 1, -1 };
    private readonly int moveCost = 10;

    Node closestNode;
    public Astar(MapEntity owner)
    {
        _owner = owner;
        _navLayer = GameManager.instance.Get_MapManager().GetWeightMap();
    }

    public override List<Node> Get_Path(Vector2Int dest)
    {
        _path.Clear();
        closestNode = null;
        Vector2Int startPos = _owner.Get_PosKey();

        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        Node startNode = new Node(startPos.x, startPos.y) { G = 0, H = GetChebyshevDistance(startPos, dest) };
        openList.Push(startNode);
        allNodes[startPos] = startNode;

        while (openList.Count > 0)
        {
            Node current = openList.Pop();
            Vector2Int currentPos = new Vector2Int(current.x, current.y);

            if (current.x == dest.x && current.y == dest.y)
                return TracePath(current);

            closedList.Add(currentPos);

            // [수정 사항 B] 목적지가 막혔을 때 옆으로 퍼지게 만드는 측면 우회 로직
            // 거리가 더 가깝거나, 거리는 같은데 더 많이 걸어서(G가 커서) 새로운 빈칸을 찾은 경우 갱신
            if (closestNode == null || current.H < closestNode.H || (current.H == closestNode.H && current.G >= closestNode.G))
            {
                closestNode = current;
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2Int nextPos = new Vector2Int(current.x + dx[i], current.y + dy[i]);

                if (closedList.Contains(nextPos)) continue;

                byte weight = _navLayer[nextPos.x, nextPos.y];
                if (weight >= 255 && nextPos != dest) continue;

                int newG = current.G + moveCost + weight;

                // 의미 없는 else if 문을 제거하고 깔끔하게 원상복구
                if (!allNodes.ContainsKey(nextPos) || newG < allNodes[nextPos].G)
                {
                    Node nextNode = new Node(nextPos.x, nextPos.y)
                    {
                        G = newG,
                        H = GetChebyshevDistance(nextPos, dest),
                        parent = current
                    };

                    allNodes[nextPos] = nextNode;
                    openList.Push(nextNode);
                }
            }
        }
        return TracePath(closestNode);
    }
    private int GetChebyshevDistance(Vector2Int a, Vector2Int b)
    {
        // x축 차이와 y축 차이 중 더 큰 값을 기준으로 거리를 계산합니다.
        return 10 * Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
    /*
    // [수정 사항 A] 대각선 빈 공간을 미세하게 선호하도록 보정된 체비쇼프 거리
    private int GetChebyshevDistance(Vector2Int a, Vector2Int b)
    {
        int diffX = Mathf.Abs(a.x - b.x);
        int diffY = Mathf.Abs(a.y - b.y);

        // 기본 10단위 거리에, Min값을 미세하게 빼주어 대각선 이동 시 H점수 혜택을 줌
        return (10 * Mathf.Max(diffX, diffY)) - Mathf.Min(diffX, diffY);
    }
    */
    public override  List<Node> Return_Path()
    {
        return _path;
    }
    private List<Node> TracePath(Node lastNode)
    {
        Node temp = lastNode;
        while (temp != null)
        {
            _path.Add(temp);
            temp = temp.parent;
        }
        _path.Reverse();
        return _path;
    }

}
