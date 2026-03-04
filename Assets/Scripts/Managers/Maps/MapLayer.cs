using System;

public class MapLayer
{
    private byte[] _data;
    private int _width;
    private int _height; 

    public MapLayer(int width, int height)
    {
        _width=width;
        _height=height;
        _data = new byte[width * height];
    }

    public void Prepare(int w, int h)
    {
        if (_data == null || _data.Length < w * h)
            _data = new byte[w * h];

        _width = w;
        _height = h;
        Array.Fill(_data, (byte)255);
    }

    public byte this[int x, int y]
    {
        get
        {
            // 이 안전장치 하나가 나중에 원인 모를 크래시를 막아줍니다.
            if (x < 0 || x >= _width || y < 0 || y >= _height) return 255;
            return _data[y * _width + x];
        }
        set
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
                _data[y * _width + x] = value;
        }
    }
}