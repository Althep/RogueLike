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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void InitMapFog(int width, int height)
    {
        fogTilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                fogTilemap.SetTile(pos, fogTileAsset);
            }
        }
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

}