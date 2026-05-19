using UnityEngine;
using UnityEngine.UI;
using static Defines;

public class MinimapManager : MonoBehaviour
{
    public RawImage minimapImage; // UI 캔버스에 올려둔 빈 RawImage
    private Texture2D mapTexture;

    private int mapWidth = 50;  // 맵 가로 사이즈
    private int mapHeight = 50; // 맵 세로 사이즈

    private void OnEnable()
    {
        // 이벤트 이름과 참조(Instance)를 통일하여 안전하게 구독합니다.
        if (MapManager.instance != null)
        {
            MapManager.instance.OnMapGenerateComplete += InitializeMinimap; // 최초 1회 전체 그리기
            MapManager.instance.OnEntityPositionChanged += UpdatePartialMap;  // 몹 이동 시 (2픽셀 갱신 + Apply 포함)
        }

        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.OnFogUpdateComplete += ApplyTexture;   // 다 찍고 한 번에 반영
            FogOfWarManager.Instance.OnFogTileChanged += UpdateSinglePixel; // 점만 찍기 (Apply 없음)
        }
    }

    private void OnDisable()
    {
        // OnEnable과 완벽히 동일한 경로로 구독을 해제해야 메모리 누수가 발생하지 않습니다.
        if (MapManager.instance != null)
        {
            MapManager.instance.OnMapGenerateComplete -= InitializeMinimap;
            MapManager.instance.OnEntityPositionChanged -= UpdatePartialMap;
        }

        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.OnFogUpdateComplete -= ApplyTexture;
            FogOfWarManager.Instance.OnFogTileChanged -= UpdateSinglePixel;
        }
    }

    public void Init()
    {
        // 매니저 초기화 시 필요한 로직 (현재는 비워둠)
    }

    private void InitializeMinimap()
    {
        int width = MapManager.instance.Get_Width();
        int height = MapManager.instance.Get_Height();
        InitMinimap(width, height); // 텍스처 생성 함수 호출
    }

    public void InitMinimap(int width, int height)
    {
        mapWidth = width;
        mapHeight = height;

        // 1. 맵 크기와 똑같은 사이즈의 텍스처를 생성합니다.
        mapTexture = new Texture2D(mapWidth, mapHeight, TextureFormat.RGBA32, false);

        // [핵심] 픽셀이 번지지 않고 레트로한 도트 느낌이 날카롭게 살도록 필터 모드를 설정합니다.
        mapTexture.filterMode = FilterMode.Point;

        // 2. 생성한 텍스처를 UI RawImage에 연결합니다.
        minimapImage.texture = mapTexture;

        // 3. 초기 맵 그리기
        DrawFullMap();
    }

    public void DrawFullMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Color tileColor = GetTileColor(x, y);

                // 픽셀의 색상을 변경합니다. (아직 화면에 적용되진 않음)
                mapTexture.SetPixel(x, y, tileColor);
            }
        }

        // [중요] 모든 점을 다 찍은 후, 마지막에 딱 한 번 Apply를 호출해야 화면에 쫙 뿌려집니다!
        ApplyTexture();
    }

    private void UpdatePartialMap(Vector2Int fromPos, Vector2Int toPos)
    {
        // 1. 엔티티가 떠난 자리(과거 위치)를 다시 계산해서 칠합니다. 
        Color fromColor = GetTileColor(fromPos.x, fromPos.y);
        mapTexture.SetPixel(fromPos.x, fromPos.y, fromColor);

        // 2. 엔티티가 도착한 자리(현재 위치)를 다시 계산해서 칠합니다.
        Color toColor = GetTileColor(toPos.x, toPos.y);
        mapTexture.SetPixel(toPos.x, toPos.y, toColor);

        // 3. 점 두 개 다 찍었으니 화면에 적용!
        ApplyTexture();
    }

    private void UpdateSinglePixel(Vector2Int pos)
    {
        Color color = GetTileColor(pos.x, pos.y);
        mapTexture.SetPixel(pos.x, pos.y, color);
    }

    // [추가됨] FoW 갱신이 끝났을 때 한 번에 도장을 쾅 찍어주는 함수
    private void ApplyTexture()
    {
        if (mapTexture != null)
        {
            mapTexture.Apply();
        }
    }

    private Color GetTileColor(int x, int y)
    {
        // 1. 한 번도 안 간 곳은 투명 처리
        if (!FogOfWarManager.Instance.IsExplored(x, y))
            return Color.clear;

        // 2. 맵 매니저님, 여기 제일 위에 있는 게 뭡니까?
        TileType type = MapManager.instance.GetTileType(new Vector2Int(x, y));

        // 3. 안개 속 몬스터 숨기기 처리
        if (type == TileType.Monster && !FogOfWarManager.Instance.IsCurrentlyVisible(x, y))
        {
            // 몬스터가 안개 속에 있다면, 몬스터 대신 바닥 색을 반환합니다.
            return new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        // 4. 색상 칠하기
        switch (type)
        {
            case TileType.Player: return Color.green;
            case TileType.Monster: return Color.red;
            case TileType.Tile: return new Color(0.3f, 0.3f, 0.3f, 1f);
            case TileType.Wall: return Color.gray;
            case TileType.Door: return new Color(0.6f, 0.3f, 0f, 1f);
            case TileType.Upstair: return Color.cyan;
            case TileType.DownStair: return Color.magenta;
            default: return Color.clear;
        }
    }
}