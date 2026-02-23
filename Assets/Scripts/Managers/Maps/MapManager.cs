using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
using Cysharp.Threading.Tasks;

public class MapManager : MonoBehaviour
{
    private Dictionary<MapMakeType, MapMaker> _mapMaker;
    private Dictionary<MapMakeType, MapMaker> mapMakers => _mapMaker ??= new()
    {
        [MapMakeType.Divide] = new MapMaker_Divide(this),
        [MapMakeType.Saved] = new MapMaker_Saved(this),
        [MapMakeType.CellularAutomata] = new MapMaker_CellularAutomata(this),
        [MapMakeType.Prim] = new MapMaker_Prim(this),
        [MapMakeType.BSP] = new MapMaker_BSP(this)
    };
    Defines.MapMakeType makerType;
    MapMaker selectedMaker;
    [SerializeField] PoolManager poolManager;

    //public Dictionary<Vector2Int, Defines.TileType> tileData = new Dictionary<Vector2Int, Defines.TileType>(); //몬스터등을 포함한 맵데이터
    public Dictionary<Vector2Int, Defines.TileType> enviromentData = new Dictionary<Vector2Int, Defines.TileType>(); //타일과 벽, 문등을 포함한 맵데이터
    public Dictionary<Vector2Int, MapEntity> dynamicMapData = new Dictionary<Vector2Int, MapEntity>(); //몬스터, 캐릭터등 움직이는 오브젝트 데이터
    public Dictionary<Vector2Int, List<MapEntity>> interactiveMapData = new Dictionary<Vector2Int, List<MapEntity>>(); //문 아이템등 상호작용 오브젝트 데이터
    
    [SerializeField] List<MapEntity> tileObjects = new List<MapEntity>();

    public List<Vector2> tilePosList = new List<Vector2>();
    public List<Vector2> wallPosList = new List<Vector2>();
    public List<Vector2> doorPosList = new List<Vector2>();
    public List<Vector2> upStairList = new List<Vector2>();
    public List<Vector2> downStairList = new List<Vector2>();

    MonsterManager monsterManager;
    ItemManager itemManager;
    public MapLayer mapLayer;
    int _width;
    int _height;

    Vector2[] around = new Vector2[]
{
    new Vector2(1, 1),  new Vector2(1, 0),  new Vector2(1, -1),
    new Vector2(0, 1),                      new Vector2(0, -1), // (0,0) 제외, (0,1) 추가
    new Vector2(-1, 1), new Vector2(-1, 0), new Vector2(-1, -1)
};

    void Start()
    {

    }
    private void Init()
    {
        if(monsterManager == null)
        {
            monsterManager = GameManager.instance.Get_MonsterManager();
        }
        if(itemManager == null)
        {
            itemManager = GameManager.instance.Get_ItemManager();
        }
        mapLayer = new MapLayer(60, 60);
    }

    void Init_MapMaker()
    {
        //mapMaker.Add(Defines.MapMakeType.Divide, new MapMaker_Divide());
        if (poolManager == null)
        {
            GameManager.instance.Get_PoolManager();
        }
    }

    #region Gets
    void Get_RandomMapMaker()
    {
        List<MapMakeType> mapMakerType = Utils.Get_Enums<MapMakeType>(MapMakeType.Divide).ToList();
        if (mapMakerType.Contains(MapMakeType.Saved))
        {
            mapMakerType.Remove(MapMakeType.Saved);
        }
        if (mapMakerType.Contains(MapMakeType.Prim))
        {
            mapMakerType.Remove(MapMakeType.Prim);
        }
        int index = UnityEngine.Random.Range(0, mapMakerType.Count);
        selectedMaker = mapMakers[mapMakerType[index]];
        this.makerType = selectedMaker.makerType;
    }

    public List<Vector2> GetEmptyPosList()
    {
        List<Vector2> emptyPos = tilePosList;



        return emptyPos;
    }

    public List<Vector2> GetPotentialPos(Vector2 center, int range)
    {
        List<Vector2> potentials = tilePosList.Where(p =>
        p.x >= center.x - range && p.x <= center.x + range &&
        p.y >= center.y - range && p.y <= center.y + range
        ).ToList();
        return potentials;
    }
    public Vector2 GetRandomTilePos()
    {
        int randomIndex = UnityEngine.Random.Range(0, tilePosList.Count);
        Vector2 randomPos = tilePosList[randomIndex];
        return randomPos;
    }

    public Dictionary<Vector2Int,TileType> GetEnviromentData()
    {
        return enviromentData;
    }
    #endregion

    public void EntityMove(Vector2Int originPos, Vector2Int nextPos, Defines.TileType myType)
    {

    }
    #region MapMake
    public async UniTask MapMake()
    {
        float mapMakeStart = Time.realtimeSinceStartup;
        await SetMapData();
        Debug.Log($"[Profile] 오브젝트 배치 시간: {Time.realtimeSinceStartup - mapMakeStart}s");
        await MapObjSpawn();
        Vector2Int mapSize = selectedMaker.GetMapSize();
        _height = mapSize.y;
        _width = mapSize.x;
        mapLayer.Prepare(mapSize.x, mapSize.y);
        Debug.Log("던전 생성 완료");
    }
    public MapMaker Get_MapMaker()
    {
        return mapMakers[makerType];
    }
    public async UniTask SetMapData()
    {
        Debug.Log("던전 생성 시작");
        if (IsSaved())
        {
            enviromentData = MakeToSaveData();
        }
        else
        {
            Get_RandomMapMaker();
            enviromentData = await selectedMaker.MapMake();
        }


        AddObjToList();
    }
    void AddObjToList()
    {
        tilePosList.Clear();
        wallPosList.Clear();
        doorPosList.Clear();
        upStairList.Clear();
        downStairList.Clear();

        foreach (var kv in enviromentData)
        {
            Vector2 pos = (Vector2)kv.Key;
            switch (kv.Value)
            {
                case TileType.Tile: tilePosList.Add(pos); break;
                case TileType.Wall: wallPosList.Add(pos); break;
                case TileType.Door: doorPosList.Add(pos); break;
                case TileType.Upstair: upStairList.Add(pos); break;
                case TileType.DownStair: downStairList.Add(pos); break;
            }
        }
    }

    void TileListClear()
    {
        tilePosList.Clear();
        wallPosList.Clear();
        doorPosList.Clear();
    }
    public async UniTask MapObjSpawn()
    {
        if (poolManager == null)
            poolManager = GameManager.instance.Get_PoolManager();

        int count = 0;
        // 성능 최적화: 한 프레임에 처리할 개수 (컴퓨터 사양에 따라 50~100개 적당)
        const int yieldInterval = 50;

        foreach (Vector2Int pos in enviromentData.Keys)
        {
            TileType type = enviromentData[pos];
            GameObject go = poolManager.ObjectPool(type);
            go.transform.position = new Vector3(pos.x, pos.y);

            if (go.TryGetComponent<MapEntity>(out MapEntity tile))
            {
                if(tile is  DoorEntity door)
                {
                    AddMapData(pos,door);
                }
                tileObjects.Add(tile);
            }

            count++;
            // 일정 개수를 생성하면 다음 프레임으로 넘김
            if (count % yieldInterval == 0)
            {
                await UniTask.Yield();
            }
        }
    }
    public bool IsSaved()
    {
        return false;
    }
    Dictionary<Vector2Int, TileType> MakeToSaveData()
    {
        return new Dictionary<Vector2Int, TileType>();
    }
    #endregion 
    public async UniTask ReturnAll()
    {
        if (poolManager == null)
        {
            poolManager = GameManager.instance.Get_PoolManager();
        }
        foreach (MapEntity tile in tileObjects)
        {
            poolManager.Return(tile.myType, tile.gameObject);
        }
        tileObjects.Clear();
        enviromentData.Clear();
        TileListClear();
        await UniTask.Yield();
        Debug.Log("제거완료");
    }

    public List<Vector2Int> GetAroundTile(Vector2 target)
    {
        List<Vector2Int> final = new List<Vector2Int>();
        Vector2Int newTarget = new Vector2Int((int)target.x, (int)target.y);
        foreach (var p in around)
        {
            Vector2Int key = new Vector2Int((int)(target.x - p.x), (int)(target.y - p.y));
            if (enviromentData.ContainsKey(key))
            {
                final.Add(key);
            }
        }
        final.Add(newTarget);
        return final;
    }

    public List<Vector2Int> SetMonsterRandomPosition(List<LivingEntity> monsters)
    {
        Vector2 center = GetRandomTilePos();
        List<Vector2> potentials = GetPotentialPos(center, monsters.Count);
        int roofs = 0;
        while (potentials.Count < monsters.Count)
        {
            roofs++;
            potentials = GetPotentialPos(center, monsters.Count + roofs);
            if (roofs > 10)
            {
                Debug.Log("몬스터 젠 시킬 공간이 존재하지않음!");
                break;
            }
        }
        Utils.Shuffle(potentials);
        List<Vector2Int> shuffledPos = new List<Vector2Int>();
        for (int i = 0; i < monsters.Count && i < potentials.Count; i++)
        {
            shuffledPos.Add(Vector2Int.RoundToInt(potentials[i]));
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            if (i >= shuffledPos.Count) break;

            Debug.Log($"Suffled position : {shuffledPos[i]}");
            monsters[i].transform.position = new Vector3(shuffledPos[i].x, shuffledPos[i].y, -1);
            TileType tileType = monsters[i].GetMyType();
            Vector2Int shuppfleRandomPos = shuffledPos[i];
            AddMapData(shuppfleRandomPos, monsters[i]);
            tilePosList.Remove(shuppfleRandomPos);
        }

        return shuffledPos;
        
    }

    public GameObject SetEnviromentObjectPos(GameObject go,Vector2Int pos, TileType objType)
    {
        go.transform.position = (Vector2)pos;

        return go;
    }

    #region Entitys
    public bool CanMove(Vector2Int dest)
    {
        if(enviromentData.TryGetValue(dest ,out var tileType))
        {
            if(tileType == TileType.Wall || tileType == TileType.DeepWater)
            {
                return false;
            }
        }

        if (dynamicMapData.ContainsKey(dest))
        {
            return false;
        }

        if (interactiveMapData.TryGetValue(dest, out List<MapEntity> entities))
        {
            var door = entities.OfType<DoorEntity>().FirstOrDefault();
            if (door != null && !door.IsOpen())
            {
                return false; // 문이 있는데 닫혀있으면 못 감
            }
        }
        return true;
    }

    public void EntityMove(LivingEntity entity,Vector2Int pos,Vector2Int dest)
    {
        if (!dynamicMapData.ContainsKey(pos))
        {
            Debug.Log($"다이나믹 맵데이터에 포함되어 있지 않음! {entity.name} 포지션 : {pos}");
            return;
        }
        if (dynamicMapData.ContainsKey(dest))
        {
            Debug.LogWarning($"{dest} 위치에 이미 {dynamicMapData[dest].name}이 있습니다!");
        }
        dynamicMapData.Remove(pos);
        dynamicMapData[dest] = entity;


        mapLayer[pos.x, pos.y] = (byte)GetTypeCost(pos);
        mapLayer[dest.x, dest.y] = (byte)(GetTypeCost(dest));
    }
    
    public void AddMapData(Vector2Int pos, LivingEntity entity)
    {
        if (dynamicMapData.ContainsKey(pos))
        {
            Debug.Log("해당 위치에 이미 맵엔티티 저장되어있음!");
        }
        else
        {
            dynamicMapData.Add(pos, entity);
        }
    }

    public void AddMapData(Vector2Int pos, DoorEntity entity)
    {
        if (interactiveMapData.ContainsKey(pos))
        {
            Debug.Log("해당 위치에 이미 맵엔티티 저장되어있음!");
        }
        else
        {
            interactiveMapData.Add(pos, new List<MapEntity>());
        }
        if (!interactiveMapData[pos].Contains(entity))
        {
            interactiveMapData[pos].Add(entity);
        }
        
    }
    public void AddMapData(Vector2Int pos, ItemEntity entity)
    {
        if (interactiveMapData.ContainsKey(pos))
        {
            Debug.Log("해당 위치에 이미 맵엔티티 저장되어있음!");
        }
        else
        {
            interactiveMapData.Add(pos, new List<MapEntity>());
        }
        if (!interactiveMapData[pos].Contains(entity))
        {
            interactiveMapData[pos].Add(entity);
        }
    }
    public void OnItemPickUp(Vector2Int pos)
    {

    }
    #endregion
    #region MapLayer
    public int GetTypeCost(Vector2Int pos)
    {
        if (dynamicMapData.ContainsKey(pos))
        {
            return 255;
        }
        if (interactiveMapData.TryGetValue(pos, out var entities))
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] is DoorEntity door && !door.IsOpen())
                    return 255;
            }
        }
        TileType type = enviromentData[pos];
        switch (type)
        {
            case TileType.Wall:return 255;
            case TileType.ShallowWater: return 2;
            case TileType.DeepWater: return 255;
        }
        return 1;
    }
    public void CalculateInitialCost()
    {
        foreach(var kvp in enviromentData)
        {
            Vector2Int pos = kvp.Key;
            TileType type = kvp.Value;

            if (pos.x<0||pos.x>=_width||pos.y<0||pos.y<_height) continue;

            byte cost = 255;
            if (type == TileType.Tile) cost = 1;
            else if (type == TileType.ShallowWater) cost =2;
            mapLayer[pos.x, pos.y] = cost;
        }

        foreach (var kvp in interactiveMapData)
        {
            Vector2Int pos = kvp.Key;
            if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height) continue;

            byte cost = mapLayer[pos.x, pos.y];
            var entities = kvp.Value;
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] is DoorEntity door && !door.IsOpen())
                {
                    cost = 255;
                    break;
                }
            }
            mapLayer[pos.x, pos.y] = cost;
        }

        foreach (var kvp in dynamicMapData)
        {
            Vector2Int pos = kvp.Key;
            byte cost = 255;

            if (pos.x<0||pos.x>=_width||pos.y<0||pos.y<_height) continue;
            if (kvp.Value is PlayerEntity) cost = 10;

            mapLayer[pos.x, pos.y] = cost;
        }
    }
    #endregion
}
