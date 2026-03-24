using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    public Tilemap fogTilemap;
    public TileBase fogTileAsset;      // 까만 타일
    public TileBase visitedTileAsset;  // 회색 타일

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

        // 조건문 삭제! 비어있든 까만색이든 무조건 '방문한 회색 타일'로 덮어버립니다.
        fogTilemap.SetTile(pos, visitedTileAsset);
    }
}