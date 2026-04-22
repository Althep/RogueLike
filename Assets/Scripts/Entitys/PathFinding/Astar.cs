using UnityEngine;
using System;
using System.Collections.Generic;

public class Astar
{
    private List<Node> _path = new List<Node>();
    private MapLayer _navLayer;
    private GameObject _owner;

    // 방향 데이터 (8방향: 상하좌우 + 대각선)
    private readonly int[] dx = { 0, 0, -1, 1, -1, -1, 1, 1 };
    private readonly int[] dy = { 1, -1, 0, 0, 1, -1, 1, -1 };
    // 모든 이동 비용을 10으로 통일 (사용자 요청 사항)
    private readonly int moveCost = 10;

    public Astar(GameObject owner)
    {
        _owner = owner;
        // MapManager를 통해 이미 255로 초기화된 레이어를 가져옴
        _navLayer = GameManager.instance.Get_MapManager().GetWeightMap();
    }

    public List<Node> Get_Path(Vector2Int dest)
    {
        _path.Clear();

        Vector2Int startPos = new Vector2Int(Mathf.RoundToInt(_owner.transform.position.x), Mathf.RoundToInt(_owner.transform.position.y));

        // 도착지점이 벽(255)이라면 길을 찾을 수 없음
        //if (_navLayer[dest.x, dest.y] >= 255) return _path;

        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        Node startNode = new Node(startPos.x, startPos.y) { G = 0, H = GetManhattanDistance(startPos, dest) };
        openList.Push(startNode);
        allNodes[startPos] = startNode;

        while (openList.Count > 0)
        {
            Node current = openList.Pop();
            Vector2Int currentPos = new Vector2Int(current.x, current.y);

            if (current.x == dest.x && current.y == dest.y)
                return TracePath(current);

            closedList.Add(currentPos);

            for (int i = 0; i < 8; i++)
            {
                Vector2Int nextPos = new Vector2Int(current.x + dx[i], current.y + dy[i]);

                // 1. 이미 검사한 노드 스킵
                if (closedList.Contains(nextPos)) continue;

                // 2. MapLayer 인덱서를 통한 벽(255) 및 가중치 확인
                byte weight = _navLayer[nextPos.x, nextPos.y];
                if (weight >= 255 && nextPos != dest) continue;

                // 3. 비용 계산: 이전G + 이동비용(10) + 타일 가중치
                int newG = current.G + moveCost + weight;

                if (!allNodes.ContainsKey(nextPos) || newG < allNodes[nextPos].G)
                {
                    Node nextNode = new Node(nextPos.x, nextPos.y)
                    {
                        G = newG,
                        H = GetManhattanDistance(nextPos, dest),
                        parent = current
                    };

                    allNodes[nextPos] = nextNode;
                    openList.Push(nextNode);
                }
            }
        }
        return _path;
    }

    // 대각선 비용이 10일 때 가장 적합한 맨해튼 거리 휴리스틱
    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return 10 * (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y));
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