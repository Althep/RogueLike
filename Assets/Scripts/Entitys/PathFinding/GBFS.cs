using UnityEngine;
using System;
using System.Collections.Generic;
public class GBFS :PathFinder
{
    MapLayer _navLayer;
    MapEntity _owner;

    Node closestNode;

    private readonly int[] dx = { 0, 0, -1, 1, -1, -1, 1, 1 };
    private readonly int[] dy = { 1, -1, 0, 0, 1, -1, 1, -1 };

    private readonly int moveCost = 10;

    readonly int maxSearch = 64;

    public GBFS(MapEntity owner)
    {
        _owner = owner;
        _navLayer = MapManager.instance.GetWeightMap();
    }

    public override List<Node> Get_Path(Vector2Int dest)
    {
        _path.Clear();
        closestNode = null;
        Vector2Int startPos = _owner.Get_PosKey();

        // 탐색 효율을 극대화하기 위해 visited 하나로 통합 관리
        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // GBFS는 G 비용이 필요 없으므로 0으로 설정합니다.
        Node startNode = new Node(startPos.x, startPos.y) { G = 0, H = GetChebyshevDistance(startPos, dest) };
        openList.Push(startNode);
        visited.Add(startPos);

        while (openList.Count > 0)
        {
            Node current = openList.Pop();
            Vector2Int currentPos = new Vector2Int(current.x, current.y);

            // 목적지 도달 시 즉시 경로 복기 및 반환
            if (current.x == dest.x && current.y == dest.y)
                return TracePath(current);

            // [수정 사항] 목적지가 막혔을 때를 대비한 최선책(가장 H가 낮은 노드) 추적
            // GBFS 특성상 G를 고려하지 않으므로 오직 H값만 비교합니다.
            if (closestNode == null || current.H < closestNode.H)
            {
                closestNode = current;
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2Int nextPos = new Vector2Int(current.x + dx[i], current.y + dy[i]);

                // 이미 예약(Open)되었거나 방문(Closed)한 곳은 완전히 스킵
                if (visited.Contains(nextPos)) continue;

                // 맵 경계 외부 및 장애물 검사 (255 이상은 통과 불가)
                // *주의: 실제 환경에 맞게 맵 배열 크기 경계 체크(Out of Bounds)를 추가하는 것을 권장합니다.
                byte weight = _navLayer[nextPos.x, nextPos.y];
                if (weight >= 255 && nextPos != dest) continue;

                // GBFS 핵심: G는 무조건 0으로 둡니다. (가중치 weight도 이동 비용이므로 무시)
                Node nextNode = new Node(nextPos.x, nextPos.y)
                {
                    G = 0,
                    H = GetChebyshevDistance(nextPos, dest),
                    parent = current
                };

                visited.Add(nextPos);
                openList.Push(nextNode);
            }
        }

        // 목적지 도달에 실패했다면, 그나마 가장 가까웠던 노드의 경로를 반환
        return TracePath(closestNode);
    }

    private int GetChebyshevDistance(Vector2Int a, Vector2Int b)
    {
        // x축 차이와 y축 차이 중 더 큰 값을 기준으로 거리를 계산합니다.
        return 10 * Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
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


    public override List<Node> Return_Path()
    {
        return _path;
    }
}
