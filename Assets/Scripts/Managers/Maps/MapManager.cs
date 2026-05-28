using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static Defines;
using Cysharp.Threading.Tasks;
using UnityEditor;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
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
    public Dictionary<Vector2Int, TileType> enviromentData = new Dictionary<Vector2Int, TileType>(); //타일과 벽, 문등을 포함한 맵데이터
    public MapEntity[,] bakedEnviromentData;
    public Dictionary<Vector2Int, MapEntity> dynamicMapData = new Dictionary<Vector2Int, MapEntity>(); //몬스터, 캐릭터등 움직이는 오브젝트 데이터
    public Dictionary<Vector2Int, List<MapEntity>> interactiveMapData = new Dictionary<Vector2Int, List<MapEntity>>(); //문 아이템등 상호작용 오브젝트 데이터
    public List<WallEntity> wallEntitys = new List<WallEntity>();
    public List<TileEntity> tileEntitys = new List<TileEntity>();
    public List<StairEntity> stairs = new List<StairEntity>();
    //public List<DownStairEntity> downStairs = new List<DownStairEntity>();
    public List<DoorEntity> doors = new List<DoorEntity>();
    [SerializeField] List<MapEntity> enviromentObjs = new List<MapEntity>();

    public List<Vector2Int> tilePosList = new List<Vector2Int>();
    public List<Vector2Int> wallPosList = new List<Vector2Int>();
    public List<Vector2Int> doorPosList = new List<Vector2Int>();
    public List<Vector2Int> upStairList = new List<Vector2Int>();
    public List<Vector2Int> downStairList = new List<Vector2Int>();

    MonsterManager monsterManager;
    ItemManager itemManager;

    public MapLayer mapLayer;
    int _width;
    int _height;

    public Action OnMapGenerateComplete;
    public Action<Vector2Int,Vector2Int> OnEntityPositionChanged;

    Vector2[] around = new Vector2[]
{
    new Vector2(1, 1),  new Vector2(1, 0),  new Vector2(1, -1),
    new Vector2(0, 1),                      new Vector2(0, -1), // (0,0) 제외, (0,1) 추가
    new Vector2(-1, 1), new Vector2(-1, 0), new Vector2(-1, -1)
};



    public void Awake()
    {
        Init();
    }
    void Start()
    {

    }

    private void Init()
    {
        if (monsterManager == null)
        {
            monsterManager = GameManager.instance.Get_MonsterManager();
        }
        if (itemManager == null)
        {
            itemManager = GameManager.instance.Get_ItemManager();
        }
        if(instance == null)
        {
            instance = this;
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
    public bool IsInMap(Vector2Int pos)
    {
        if (pos.x>_width-1||pos.y>_height-1||pos.x<0||pos.y<0)
        {
            return false;
        }
        return true;
    }
    public int Get_Width()
    {
        return _width;
    }
    public int Get_Height()
    {
        return _height;
    }
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
    public MapEntity Get_BakedEnvironmentEntity(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height)
        {
            return bakedEnviromentData[pos.x, pos.y];
        }
        // 맵 밖이거나 해당 칸에 오브젝트가 생성되지 않았다면 null 반환
        return null;
    }
    public List<Vector2Int> GetEmptyPosList()
    {
        List<Vector2Int> emptyPos = tilePosList;



        return emptyPos;
    }

    public List<Vector2Int> GetPotentialPos(Vector2 center, int range)
    {
        List<Vector2Int> potentials = tilePosList.Where(p =>
        p.x >= center.x - range && p.x <= center.x + range &&
        p.y >= center.y - range && p.y <= center.y + range
        ).ToList();
        return potentials;
    }
    public Vector2Int GetRandomTilePos()
    {
        int randomIndex = UnityEngine.Random.Range(0, tilePosList.Count);
        Vector2Int randomPos = tilePosList[randomIndex];
        return randomPos;
    }

    public Dictionary<Vector2Int, TileType> GetEnviromentData()
    {
        return enviromentData;
    }

    public StairEntity Try_GetStairEntity(Vector2Int pos)
    {
        StairEntity stairEntity = stairs.FirstOrDefault(s => s.Get_PosKey() == pos);

        return stairEntity;
    }

    public TileType GetTileType(Vector2Int posKey)
    {
        //미니맵 드로잉을 위한 타입 가져오기
        if (dynamicMapData.ContainsKey(posKey))//플레이어와 몬스터 우선
        {
            return dynamicMapData[posKey].GetMyType();
        }
        else if (interactiveMapData.ContainsKey(posKey))//아이템,문 가장 처음에 있던 오브젝트 기준
        {
            return interactiveMapData[posKey][0].GetMyType();
        }
        else//환경정보 반환
        {
            return enviromentData[posKey];
        }
    }
    #endregion

    
    #region MapMake
    public async UniTask MapMake()
    {
        Debug.Log($"[생성기] 맵을 만드는 매니저: {gameObject.name} / ID: {GetInstanceID()}");
        float mapMakeStart = Time.realtimeSinceStartup;
        await SetMapData();
        Debug.Log($"[Profile] 오브젝트 배치 시간: {Time.realtimeSinceStartup - mapMakeStart}s");
        Vector2Int mapSize = selectedMaker.GetMapSize();
        _height = mapSize.y;
        _width = mapSize.x;
        await MapObjSpawn();
        LeftDataUpdate();
        
        mapLayer.Prepare(mapSize.x, mapSize.y);
        CalculateInitialCost();
        OnMapGenerateComplete?.Invoke();
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
            makerType = MapMakeType.Saved;
            selectedMaker = mapMakers[makerType];
            enviromentData = await selectedMaker.MapMake();
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
            Vector2Int pos = kv.Key;
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
    void BakeEnviromentData()
    {

    }
    public async UniTask MapObjSpawn()
    {
        int upStairNunber = 0;
        int downStairNunber = 0;
        if (poolManager == null)
            poolManager = GameManager.instance.Get_PoolManager();

        int count = 0;
        const int yieldInterval = 50;
        bakedEnviromentData = new MapEntity[_width, _height];
        foreach (Vector2Int pos in enviromentData.Keys)
        {
            TileType type = enviromentData[pos];
            GameObject go = poolManager.ObjectPool(type);
            

            if (go.TryGetComponent<MapEntity>(out MapEntity tile))
            {

                switch (tile)
                {
                    case DoorEntity door: 
                        AddMapData(pos, door);
                        go.transform.position = new Vector3(pos.x, pos.y, -1); 
                        break;
                    case TileEntity tileTile: 
                        AddMapData(tileTile);
                        go.transform.position = new Vector3(pos.x, pos.y);
                        break;
                    case WallEntity wallTile: 
                        AddMapData(wallTile); 
                        go.transform.position = new Vector3(pos.x, pos.y, -1);
                        break;
                    case StairEntity stair : 
                        AddMapData(stair); 
                        go.transform.position = new Vector3(pos.x, pos.y, -1);
                        switch (stair.GetMyType())
                        {
                            case TileType.Upstair:
                                stair.stairNumber = upStairNunber;
                                upStairNunber++;
                                break;
                            case TileType.DownStair:
                                stair.stairNumber = downStairNunber;
                                downStairNunber++;
                                break;
                            default:
                                break;
                        }
                        break;
                }

                enviromentObjs.Add(tile);
                tile.SetMyPos(pos);
                if (pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height)
                {
                    bakedEnviromentData[pos.x, pos.y] = tile;
                }
                else
                {
                    // 어떤 이상한 좌표가 범위를 벗어났는지 콘솔에 찍어서 범인을 찾습니다.
                    Debug.LogWarning($"[배열 범위 이탈] 타일 좌표가 맵 크기를 벗어났습니다! pos: {pos}, width: {_width}, height: {_height}");
                }
            }

            count++;
            if (count % yieldInterval == 0)
            {
                await UniTask.Yield();
            }
        }
        //FogOfWarManager.Instance.InitMapFog(_width, _height);
    }
    public bool IsSaved()
    {
        return SaveDataManager.instance.IsSaved();
    }
    Dictionary<Vector2Int, TileType> MakeToSaveData()
    {
        return new Dictionary<Vector2Int, TileType>();
    }



    public async UniTask ReturnAllObjects()
    {
        for(int i = 0;i<enviromentObjs.Count; i++)
        {
            enviromentObjs[i].Return();
        }
        await UniTask.Yield();
        /*
        for(int i = 0; i<wallEntitys.Count; i++)
        {
            wallEntitys[i].Return();
        }
        for(int i =0; i<stairs.Count; i++)
        {
            stairs[i].Return();
        }
        for(int i = 0; i<doors.Count; i++)
        {
            doors[i].Return();
        }
        */
        enviromentObjs.Clear();
        dynamicMapData.Clear();
        wallEntitys.Clear();
        stairs.Clear();
        doors.Clear();
        enviromentData.Clear();
        interactiveMapData.Clear();
    }


    #endregion 
    

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

    public List<Vector2Int> Get_AroundOnlyTile(Vector2Int target)
    {
        List<Vector2Int> around = GetAroundTile(target);
        List<Vector2Int> final = new List<Vector2Int>();

        foreach(var p in around)
        {
            if(enviromentData[p] == TileType.Tile)
            {
                final.Add(p);
            }
        }

        return final;
    }

    public List<Vector2Int> GetCoordinatesInCircle(Vector2Int center, int radius)
    {
        List<Vector2Int> coordinatesInCircle = new List<Vector2Int>();
        int radiusSquared = radius * radius; // 원 넓이 계산

        for (int x = center.x - radius; x <= center.x + radius; x++)
        {
            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                int dx = x - center.x;
                int dy = y - center.y;

                if (dx * dx + dy * dy <= radiusSquared)
                {
                    coordinatesInCircle.Add(new Vector2Int(x, y));
                }
            }
        }

        return coordinatesInCircle;
    }

    public List<MonsterEntity> Get_MonstersInRadius(Vector2Int playerPos,int radius)
    {
        List<MonsterEntity> monsters = new List<MonsterEntity>();
        List<Vector2Int> circlePosList = GetCoordinatesInCircle(playerPos, radius);

        foreach(Vector2Int pos in circlePosList)
        {
            MonsterEntity entity = GetMonsterEntity(pos)as MonsterEntity;
            if(entity!= null)
            {
                monsters.Add(entity);
            }
        }

        return monsters;
    }

    public List<MapEntity> Get_DoorsInRadius(Vector2Int playerPos,int radius)
    {
        List<MapEntity> interacts = new List<MapEntity>();
        List<Vector2Int> circlePosList = GetCoordinatesInCircle(playerPos, radius);

        foreach(Vector2Int pos in circlePosList)
        {
            DoorEntity entity = GetDoorEntity(pos) as DoorEntity;
            if(entity != null)
            {
                interacts.Add(entity);
            }
        }

        return interacts;
    }

    public List<MapEntity> Get_EnviromentsInRadius(Vector2Int playerPos,int radius)
    {
        List<MapEntity> enviroments = new List<MapEntity>();
        List<Vector2Int> circlePosList = GetCoordinatesInCircle(playerPos, radius);

        foreach(Vector2Int pos in circlePosList)
        {
            MapEntity entity = bakedEnviromentData[pos.x,pos.y];
            if (entity!=null)
            {
                enviroments.Add(entity);
            }
        }
        return enviroments;

    }

    public List<Vector2Int> SetMonsterRandomPosition(List<MonsterEntity> monsters)
    {
        Vector2Int center = GetRandomTilePos();
        List<Vector2Int> potentials = GetPotentialPos(center, monsters.Count);
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

    public GameObject SetEnviromentObjectPos(GameObject go, Vector2Int pos, TileType objType)
    {
        go.transform.position = (Vector2)pos;

        return go;
    }

    #region Entitys

    public MonsterEntity GetMonsterEntity(Vector2Int targetPos)
    {
        if (dynamicMapData.ContainsKey(targetPos))
        {
            return dynamicMapData[targetPos]as MonsterEntity;
        }
        return null;
    }

    public DoorEntity GetDoorEntity(Vector2Int targetPos)
    {
        if (interactiveMapData.ContainsKey(targetPos))
        {
            List<MapEntity> targets = interactiveMapData[targetPos];

            for (int i = 0; i<targets.Count; i++)
            {
                if (targets[i].GetMyType() == TileType.Door)
                {
                    return targets[i] as DoorEntity;
                }
            }

        }
        return null;
    }
    public Vector2Int Get_UpStairPos(int index)
    {
        Mathf.Clamp(index, 0, upStairList.Count);
        return upStairList[index];
    }
    public Vector2Int Get_DownStairPos(int index)
    {
        Mathf.Clamp(index, 0, downStairList.Count);
        return downStairList[index];
    }
    public bool CanAcctack(Vector2Int dest)
    {
        bool canAttack = false;
        if (dynamicMapData.ContainsKey(dest))
        {
            canAttack = true;
        }
        return canAttack;
    }
    public bool CanInteract(Vector2Int dest)
    {
        bool canInteract = false;
        if (interactiveMapData.ContainsKey(dest))
            canInteract = true;

        return canInteract;
    }
    public bool CanMove(Vector2Int dest,LivingEntity entity)
    {
        if (enviromentData.TryGetValue(dest, out var tileType))
        {

            Debug.Log(tileType);
            if (tileType == TileType.Wall || tileType == TileType.DeepWater)
            {
                Debug.Log("타일타입 벽");
                return false;
            }
        }

        if (dynamicMapData.ContainsKey(dest))
        {
            if (dynamicMapData[dest]!=entity)
            {
                Debug.Log("다이나믹 엔티티 포함!");
                return false;
            }
            return true;
        }

        if (interactiveMapData.TryGetValue(dest, out List<MapEntity> entities))
        {
            var door = entities.OfType<DoorEntity>().FirstOrDefault();
            if (door != null && !door.IsOpen())
            {
                Debug.Log("문 잠김!");
                return false; // 문이 있는데 닫혀있으면 못 감
            }
        }
        return true;
    }
    public TileType Get_TargetType(Vector2Int targetPos)
    {
        if (dynamicMapData.ContainsKey(targetPos))
        {
            return dynamicMapData[targetPos].GetMyType();
        }
        if (interactiveMapData.ContainsKey(targetPos))
        {
            if (interactiveMapData[targetPos].Count> 1)
            {
                return interactiveMapData[targetPos][0].GetMyType();
            }
        }
        if (enviromentData.ContainsKey(targetPos))
        {
            return enviromentData[targetPos];
        }
        Debug.Log($"Tile Tpye Not Defined {targetPos} ");
        return TileType.DeepWater;
    }
    public void EntityMove(LivingEntity entity, Vector2Int origin, Vector2Int dest)
    {
        // 제자리 대기(이동 안 함)인 경우 무시
        if (origin == dest) return;

        if (!dynamicMapData.ContainsKey(origin))
        {
            Debug.Log($"다이나믹 맵데이터에 포함되어 있지 않음! {entity.name} 포지션 : {origin}");
            return;
        }

        // [핵심 방어선] 내가 가려는 목적지에 누군가 이미 있다면? 강제 종료!
        if (dynamicMapData.ContainsKey(dest))
        {
            Debug.LogWarning($"[뺑소니 방지] {dest} 위치에 이미 {dynamicMapData[dest].name}가 있어 {entity.name}의 이동이 취소되었습니다.");
            return;
        }

        dynamicMapData.Remove(origin);
        dynamicMapData[dest] = entity;

        mapLayer[origin.x, origin.y] = (byte)GetTypeCost(origin);
        mapLayer[dest.x, dest.y] = (byte)GetTypeCost(dest);

        OnEntityPositionChanged?.Invoke(origin,dest);
    }

    #region AddMapData
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

    public void AddMapData(TileEntity tile)
    {
        if (tileEntitys.Contains(tile))
        {
            Debug.Log("타일 엔티티 이미 저장 되어있음");
            return;
        }
        tileEntitys.Add(tile);
    }
    public void AddMapData(WallEntity wall)
    {
        if (wallEntitys.Contains(wall))
        {
            Debug.Log("벽엔티티 이미 저장되어있음");
            return;
        }
        wallEntitys.Add(wall);
    }
    public void AddMapData(Vector2Int pos, DoorEntity entity)
    {
        doors.Add(entity);
        if (!doors.Contains(entity))
        {
            doors.Add(entity);
            Debug.Log("문 리스트에 추가");
        }
        
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
    public void AddMapData(StairEntity entity)
    {
        if (stairs.Contains(entity))
        {
            Debug.Log("Stair Contained");
            return;
        }
        stairs.Add(entity);

    }

    public void RemoveMapEntity(Vector2Int pos, LivingEntity entity)
    {
        if (!dynamicMapData.ContainsKey(pos))
        {
            foreach(var key in dynamicMapData.Keys)
            {
                if (dynamicMapData[key] is MonsterEntity monster)
                {
                    Debug.Log($"ActualKey : {key} Entity {monster.id}");
                }
                
                if(entity == dynamicMapData[key])
                {
                    Debug.Log($"TargetKey {pos} ActualKey {key}");
                }
            }
            Debug.Log("Pos Don't Contained MonsterEntity");
            return;
        }
        else if (dynamicMapData[pos] != entity)
        {
            Debug.Log("Entity Not Contained");
            return;
        }
        dynamicMapData.Remove(pos);
        monsterManager.RemoveMonster(entity);
        poolManager.Return(entity.GetMyType(), entity.gameObject);
    }
    
    
    public void RemoveMapEntity(Vector2Int pos, DoorEntity entity)
    {
        if (!interactiveMapData.ContainsKey(pos))
        {
            Debug.Log("Pos Dindt't contained DoorEntity");
            return;
        }
        List<MapEntity> interacts = interactiveMapData[pos];
        bool isSame = false;
        for(int i = 0; i <interacts.Count; i++)
        {
            if (interacts[i] == entity)
                isSame = true;
        }
        if (!isSame)
        {
            Debug.Log("Not Contain Same Entity DoorEntity");
        }
        interactiveMapData.Remove(pos);
        poolManager.Return(entity.GetMyType(), entity.gameObject);
             
    }
    public void RemoveMapEntity(Vector2Int pos, ItemEntity entity)
    {
        if (!interactiveMapData.ContainsKey(pos))
        {
            Debug.Log("Pos Dind't Contained ItemEntity");
            return;
        }
        List<MapEntity> interacts = interactiveMapData[pos];
        bool isSame = false;
        for(int i = 0; i<interacts.Count; i++)
        {
            if (interacts[i] == entity)
                isSame = true;
        }
        if (!isSame)
        {
            Debug.Log("Not Contain Same Entity Item Entity");
            return;
        }
        interactiveMapData.Remove(pos);
        poolManager.Return(entity.GetMyType(), entity.gameObject);

    }


    #endregion
    public void OnItemPickUp(Vector2Int pos)
    {

    }
    #endregion
    #region MapLayer
    public MapLayer GetWeightMap()
    {
        return mapLayer;
    }

    public int GetTypeCost(Vector2Int pos)
    {
        if (dynamicMapData.TryGetValue(pos, out MapEntity entity))
        {
            if (entity is PlayerEntity)
                return 10;
            else
                return 255;
        }
        if (interactiveMapData.TryGetValue(pos, out var entities))
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] is DoorEntity door && !door.IsOpen())
                    return 2;
            }
        }
        if (enviromentData.TryGetValue(pos, out TileType type))
        {
            switch (type)
            {
                case TileType.Wall: return 255;
                case TileType.ShallowWater: return 2;
                case TileType.DeepWater: return 255;
            }
        }
        return 1;
    }
    public void CalculateInitialCost()
    {
        for(int i = 0; i<mapLayer.Get_Length();i++)
        {
            Vector2Int pos = Utils.IntToMapPos(i, _width, _height);
            mapLayer[pos.x, pos.y] = (byte)GetTypeCost(pos);
        }
    }
    #endregion

    #region FogOfWar
    public bool CheckLineOfSight(Vector2Int start, Vector2Int target)
    {
        // 시작점과 목표점이 같으면 당연히 보입니다.
        if (start == target) return true;

        Vector2 startPos = new Vector2(start.x, start.y);
        Vector2 targetPos = new Vector2(target.x, target.y);

        // --- DDA (Digital Differential Analyzer) 알고리즘 ---
        int dx = target.x - start.x;
        int dy = target.y - start.y;

        // [핵심 수정 1] 검사 해상도 2배 증가! 
        // 듬성듬성 검사해서 타일을 건너뛰는(스킵) 현상을 막기 위해 스텝을 2배로 늘립니다.
        int step = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) * 2;

        // 한 스텝마다 이동할 x, y의 실수 비율을 구합니다. (보폭이 절반으로 줄어 촘촘해짐)
        float xIncrement = (float)dx / step;
        float yIncrement = (float)dy / step;

        float currentX = start.x;
        float currentY = start.y;

        for (int i = 1; i < step; i++)
        {
            currentX += xIncrement;
            currentY += yIncrement;

            // [핵심 수정 2] C#의 이상한 반올림(RoundToInt) 대신, 기하학적으로 정확한 Floor 규칙 적용
            int checkX = Mathf.FloorToInt(currentX + 0.5f);
            int checkY = Mathf.FloorToInt(currentY + 0.5f);
            Vector2Int checkPos = new Vector2Int(checkX, checkY);

            // 타겟 좌표에 도달했다면 더 이상 벽 판정을 할 필요가 없습니다. (자기 자신 검사 방지)
            if (checkPos == target) break;

            // 해당 타일이 시야를 막는 오브젝트인지 확인
            TileType type = Get_TargetType(checkPos);

            if (type == TileType.Wall || (type == TileType.Door && !GetDoorEntity(checkPos).IsOpen()))
            {
                // 십자가 교차 판정 (정말로 마름모를 뚫었는가?)
                if (CheckLineCrossIntersection(startPos, targetPos, checkPos))
                {
                    return false; // 시야 차단!
                }
            }
        }

        return true; // 무사히 통과!
    }
    /// <summary>
    /// 마름모를 대체하는 십자가(+) 교차 판정 수학 공식
    /// </summary>
    private bool CheckLineCrossIntersection(Vector2 start, Vector2 end, Vector2Int tileCenter, float tileSize = 1f)
    {
        float halfSize = tileSize * 0.5f;

        // 1. 가로 선분 교차 검사
        if (!Mathf.Approximately(start.y, end.y))
        {
            float t = (tileCenter.y - start.y) / (end.y - start.y);

            if (t >= 0f && t <= 1f)
            {
                float x_int = start.x + t * (end.x - start.x);
                if (x_int >= tileCenter.x - halfSize && x_int <= tileCenter.x + halfSize) return true;
            }
        }

        // 2. 세로 선분 교차 검사
        if (!Mathf.Approximately(start.x, end.x))
        {
            // [핵심 수정 3] 혼재되어 있던 오타 수식 완벽 제거 및 정상화
            // 기울기 수식: $t = \frac{tileCenter.x - start.x}{end.x - start.x}$
            float t = (tileCenter.x - start.x) / (end.x - start.x);

            if (t >= 0f && t <= 1f)
            {
                float y_int = start.y + t * (end.y - start.y);
                if (y_int >= tileCenter.y - halfSize && y_int <= tileCenter.y + halfSize) return true;
            }
        }

        return false;
    }
    #endregion

    #region MapSave&Load

    public List<TileEntityData> TileSave()
    {
        List<TileEntityData> entityDatas = new List<TileEntityData>();

        for(int i =0; i<enviromentObjs.Count; i++)
        {
            Vector2Int posKey = enviromentObjs[i].Get_PosKey();
            entityDatas.Add(enviromentObjs[i].Get_SaveData());
        }

        return entityDatas;
    }

    public void LeftDataUpdate()
    {
        if(selectedMaker is MapMaker_Saved mapMaker)
        {
            Dictionary<Vector2Int, TileEntityData> left = mapMaker.leftData;

            for(int i = 0; i<doors.Count; i++)
            {
                DoorEntity door = doors[i];
                Vector2Int key = door.Get_PosKey();

                if (left.ContainsKey(key))
                {
                    TileEntityData data = left[key];
                    door.Set_OpenState(data.state);
                }
            }

            for(int i = 0; i<stairs.Count; i++)
            {
                StairEntity stair = stairs[i];

                Vector2Int key = stair.Get_PosKey();
                if (left.ContainsKey(key))
                {
                    TileEntityData data = left[key];
                    stair.stairNumber = data.stairNumber;
                }
            }
        }
        Debug.Log("TileData NotContained");
    }
    #endregion
    #region Debugs
    /*
    private void OnDrawGizmos() //맵 코스트 디버그
    {
        if(!Application.isPlaying || !mapLayer.IsEmpty())
        {

            for(int x = 0; x<_width; x++)
            {
                for(int y = 0; y<_height; y++)
                {
                    int cost = mapLayer[x, y];

                    Vector3 worldPos = new Vector3(x, y, 0);

                    if (cost>200)
                    {
                        Gizmos.color = new Color(1f, 0, 0, 0.4f);//이동 불가 지형 반투명 빨간색
                    }
                    else if (cost > 1) // 코스트가 높은 구역
                    {
                        Gizmos.color = new Color(1f, 0.9f, 0f, 0.4f); // 반투명 노란색
                    }
                    else // 이동 가능한 빈 타일 (기본 코스트 1이라고 가정)
                    {
                        Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // 반투명 초록색
                    }

                    Gizmos.DrawCube(worldPos, new Vector3(0.5f, 0.5f, 0.5f));
#if UNITY_EDITOR
                    // 2. 타일 위에 실제 코스트 숫자 텍스트 띄우기 (매우 유용함)
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.white;
                    style.alignment = TextAnchor.MiddleCenter;

                    // Scene 뷰에 텍스트 라벨 표시
                    Handles.Label(worldPos, cost.ToString(), style);
#endif
                }
            }


        }
    }*/
    #endregion
}
