using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public class MapManager : MonoBehaviour
{
    public Dictionary<Defines.MapMakeType,MapMaker> mapMaker = new Dictionary<Defines.MapMakeType, MapMaker>();

    public Dictionary<Vector2Int, Defines.TileType> mapData = new Dictionary<Vector2Int, Defines.TileType>(); //Ÿ�ϰ� ��, ������ ������ �ʵ�����
    public Dictionary<Vector2Int, Defines.TileType> tileData = new Dictionary<Vector2Int, Defines.TileType>(); //���͵��� ������ �ʵ�����
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init_MapMaker()
    {
        mapMaker.Add(Defines.MapMakeType.Divide, new MapMaker_Divide());
        
    }
    
    MapMaker Get_MapMaker()
    {
        Defines.MapMakeType makerType = Utils.Get_RandomType<Defines.MapMakeType>();

        return mapMaker[makerType];

    }

    void MapMake()
    {
        mapData = Get_MapMaker().MapMake();
    }

    void Set_NewMapData()
    {
        MapMaker maker = Get_MapMaker();
        mapData = maker.MapMake();
    }
    
    public void EntityMove(Vector2Int originPos, Vector2Int nextPos,Defines.TileType myType)
    {
        tileData[originPos] = mapData[originPos];
        tileData[nextPos] = myType;
    }
}
