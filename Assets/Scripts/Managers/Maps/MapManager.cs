using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
using Cysharp.Threading.Tasks;


[System.Serializable]
public struct TileDataEntry
{
    public Vector2Int Position;
    public Defines.TileType Type;
}


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
    public Dictionary<Vector2Int, Defines.TileType> originMapData = new Dictionary<Vector2Int, Defines.TileType>(); //타일과 벽, 문등을 포함한 맵데이터
    public Dictionary<Vector2Int, Defines.TileType> tileData = new Dictionary<Vector2Int, Defines.TileType>(); //몬스터등을 포함한 맵데이터
    [SerializeField] List<MapEntity> tileObjects = new List<MapEntity>();
    [SerializeField] List<TileDataEntry> inspectorTileData = new List<TileDataEntry>();

    public List<Vector2> tilePosList = new List<Vector2>();
    public List<Vector2> wallPosList = new List<Vector2>();
    public List<Vector2> doorPosList = new List<Vector2>();
    public List<Vector2> upStairList = new List<Vector2>();
    public List<Vector2> downStairList = new List<Vector2>();

    MonsterManager monsterManager;
    ItemManager itemManager;
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
    }

    void Init_MapMaker()
    {
        //mapMaker.Add(Defines.MapMakeType.Divide, new MapMaker_Divide());
        if (poolManager == null)
        {
            GameManager.instance.Get_PoolManager();
        }
    }
    private void UpdateInspectorData()
    {
        inspectorTileData.Clear();
        foreach (var kvp in tileData)
        {
            inspectorTileData.Add(new TileDataEntry { Position = kvp.Key, Type = kvp.Value });
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

    public Dictionary<Vector2Int,TileType> GetTileData()
    {
        return tileData;
    }
    #endregion

    public void EntityMove(Vector2Int originPos, Vector2Int nextPos, Defines.TileType myType)
    {
        //tileData[originPos] = mapData[originPos];
        //tileData[nextPos] = myType;
    }
    #region MapMake
    public async UniTask MapMake()
    {
        float mapMakeStart = Time.realtimeSinceStartup;
        await SetMapData();
        Debug.Log($"[Profile] 오브젝트 배치 시간: {Time.realtimeSinceStartup - mapMakeStart}s");
        await MapObjSpawn();

        SetOriginMapData();
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
            tileData = MakeToSaveData();
        }
        else
        {
            Get_RandomMapMaker();
            tileData = await selectedMaker.MapMake();
        }

        UpdateInspectorData();
        AddObjToList();
    }
    void AddObjToList()
    {
        tilePosList.Clear();
        wallPosList.Clear();
        doorPosList.Clear();
        upStairList.Clear();
        downStairList.Clear();

        foreach (var kv in tileData)
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
    void SetOriginMapData()
    {
        foreach(var pos in tileData.Keys)
        {
            if (!originMapData.ContainsKey(pos))
            {
                originMapData.Add(pos, tileData[pos]);
            }
            else
            {
                originMapData[pos] = tileData[pos];
            }
            if(tileData[pos] ==TileType.Monster || tileData[pos] == TileType.Player || tileData[pos] == TileType.Item)
            {
                Debug.Log($"타일 타입 에러! {pos}의 타일 타입이 {tileData[pos]} 입니다!");
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

        foreach (Vector2Int pos in tileData.Keys)
        {
            TileType type = tileData[pos];
            GameObject go = poolManager.ObjectPool(type);
            go.transform.position = new Vector3(pos.x, pos.y);

            if (go.TryGetComponent<MapEntity>(out MapEntity tile))
            {
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
        tileData.Clear();
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
            if (tileData.ContainsKey(key))
            {
                final.Add(key);
            }
        }
        final.Add(newTarget);
        return final;
    }

    public List<Vector2Int> SetRandomPosition(List<MapEntity> objects)
    {

        Vector2 center = GetRandomTilePos();
        List<Vector2> potentials = GetPotentialPos(center, objects.Count);
        int roofs = 0;
        while (potentials.Count < objects.Count)
        {
            roofs++;
            potentials = GetPotentialPos(center, objects.Count + roofs);
            if (roofs > 10)
            {
                Debug.Log("몬스터 젠 시킬 공간이 존재하지않음!");
                break;
            }
        }
        Utils.Shuffle(potentials);
        List<Vector2Int> shuffledPos = new List<Vector2Int>();
        for (int i = 0; i < objects.Count && i < potentials.Count; i++)
        {
            shuffledPos.Add(Vector2Int.RoundToInt(potentials[i]));
        }

        for (int i = 0; i < objects.Count; i++)
        {
            if (i >= shuffledPos.Count) break;

            Debug.Log($"Suffled position : {shuffledPos[i]}");
            objects[i].transform.position = new Vector3(shuffledPos[i].x, shuffledPos[i].y, -1);
            TileType tileType = objects[i].GetMyType();
            Vector2Int suppfleRandomPos = shuffledPos[i];
            if (tileData.ContainsKey(shuffledPos[i]))
            {
                tileData[shuffledPos[i]] = tileType;
            }
        }

        return shuffledPos;
        
    }

    public GameObject SetObjectPos(GameObject go,Vector2Int pos, TileType objType)
    {
        go.transform.position = (Vector2)pos;
        tileData[pos] = objType;

        return go;
    }

    public void ObjectRemovePos(GameObject go)
    {
        Vector2Int pos = Vector2Int.RoundToInt(go.transform.position);
        tileData[pos] = originMapData[pos];
    }
}
