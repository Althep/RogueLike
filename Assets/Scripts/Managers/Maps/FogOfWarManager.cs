using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    public Tilemap fogTilemap;
    public TileBase fogTileAsset;      // 까만 타일
    public TileBase visitedTileAsset;  // 회색 타일

    public List<Vector2Int> visited = new List<Vector2Int>();

    private bool[,] exploredMap;

    public Action OnFogUpdateComplete;
    public Action<Vector2Int> OnFogTileChanged;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void InitMapFog(int width, int height)
    {
        exploredMap = new bool[width, height];
        fogTilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                fogTilemap.SetTile(pos, fogTileAsset);
                exploredMap[x, y] = false;
            }
        }
        OnFogUpdateComplete?.Invoke();
    }

    public void SetVisible(Vector2Int gridPos)
    {
        Vector3Int pos = new Vector3Int(gridPos.x, gridPos.y, 0);
        
        fogTilemap.SetTile(pos, null);
        
    }

    public void SetExplored(Vector2Int gridPos)
    {
        Vector3Int pos = new Vector3Int(gridPos.x, gridPos.y, 0);
        if (MapManager.instance.IsInMap(gridPos))
        {
            fogTilemap.SetTile(pos, visitedTileAsset);
            AddVisited(gridPos);
        }
    }
    void AddVisited(Vector2Int gridPos)
    {
        Debug.Log("방문추가");
        if (!visited.Contains(gridPos))
        {
            visited.Add(gridPos);
            exploredMap[gridPos.x, gridPos.y] = true;
            OnFogTileChanged?.Invoke(gridPos);
        }
        
    }
    public void OnFloorChange()
    {
        Clear_Visited();
        int _width = MapManager.instance.Get_Width();
        int _height = MapManager.instance.Get_Height();
        InitMapFog(_width, _height);
        if (DungeonManager.instance.Get_Visitied())
        {
            Set_VisitedTiles();
        }
    }
    public void Set_VisitedTiles()
    {
        visited = SaveDataManager.instance.Get_FloorSaveData(DungeonManager.instance.Get_Floor()).visited;
        Debug.Log($"방문 위치 수 {visited.Count}");
        for(int i = 0; i<visited.Count; i++)
        {
            SetExplored(visited[i]);
        }
    }

    public List<Vector2Int> Get_Visited()
    {
        return visited;
    }
    
    public void Clear_Visited()
    {
        visited= new List<Vector2Int>();
    }
    /// <summary>
    /// 미니맵에서 해당 타일이 한 번이라도 탐험되었는지(회색 타일 이상인지) 확인합니다.
    /// </summary>
    public bool IsExplored(int x, int y)
    {
        if (exploredMap == null) return false;
        if (x < 0 || x >= exploredMap.GetLength(0) || y < 0 || y >= exploredMap.GetLength(1)) return false;

        return exploredMap[x, y];
    }

    /// <summary>
    /// (선택) 미니맵에서 현재 내 시야에 몬스터가 들어와 있는지 판별할 때 사용합니다.
    /// 타일이 null(완전히 밝혀짐)이면 현재 시야에 있는 것으로 간주합니다.
    /// </summary>
    public bool IsCurrentlyVisible(int x, int y)
    {
        TileBase tile = fogTilemap.GetTile(new Vector3Int(x, y, 0));
        return tile == null; // 까만 타일도 아니고, 회색 타일도 아니면 현재 보고 있는 곳!
    }
}