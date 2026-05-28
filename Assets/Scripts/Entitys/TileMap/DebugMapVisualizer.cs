using UnityEngine;
using UnityEngine.UI;

public class DebugMapVisualizer : MonoBehaviour
{
    public RawImage displayImage; // UI RawImage 연결
    private Texture2D _mapTexture;
    private Color[] _pixels;

    public void Initialize(int width, int height)
    {
        _mapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        _mapTexture.filterMode = FilterMode.Point; // 픽셀이 뭉개지지 않게 설정
        _pixels = new Color[width * height];
        displayImage.texture = _mapTexture;
    }

    public void UpdateVisualization(MapLayer navLayer, int width, int height)
    {
        if(navLayer == null) return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte weight = navLayer[x, y];

                // 가중치에 따른 색상 결정
                if (weight >= 255) _pixels[y * width + x] = Color.red;      // 벽 (Red)
                else if (weight > 0) _pixels[y * width + x] = Color.yellow; // 가중치 있음 (Yellow)
                else _pixels[y * width + x] = Color.black;                 // 길 (Black)
            }
        }
        if(_pixels.Length==0) return;
        _mapTexture.SetPixels(_pixels);
        _mapTexture.Apply();
    }
}