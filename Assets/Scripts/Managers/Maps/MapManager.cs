using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
public class MapManager : MonoBehaviour
{
    Dictionary<Defines.MapMakeType,MapMaker> mapMaker = new Dictionary<Defines.MapMakeType, MapMaker>();
    Defines.MapMakeType makerType;
    public Dictionary<Vector2Int, Defines.TileType> mapData = new Dictionary<Vector2Int, Defines.TileType>(); //타일과 벽, 문등을 포함한 맵데이터
    public Dictionary<Vector2Int, Defines.TileType> tileData = new Dictionary<Vector2Int, Defines.TileType>(); //몬스터등을 포함한 맵데이터
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
    
    MapMaker Get_RandomMapMaker()
    {
        makerType = Utils.Get_RandomType<Defines.MapMakeType>();

        return mapMaker[makerType];

    }

    void MapMake()
    {
        mapData = Get_RandomMapMaker().MapMake();
    }

    void Set_NewMapData()
    {
        MapMaker maker = Get_RandomMapMaker();
        mapData = maker.MapMake();
    }
    
    public void EntityMove(Vector2Int originPos, Vector2Int nextPos,Defines.TileType myType)
    {
        tileData[originPos] = mapData[originPos];
        tileData[nextPos] = myType;
    }

    public MapMaker Get_MapMaker()
    {
        return mapMaker[makerType];
    }
    
}
