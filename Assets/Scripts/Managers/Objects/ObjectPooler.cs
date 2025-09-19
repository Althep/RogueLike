using UnityEngine;
using System;
using System.Collections.Generic;
public class ObjectPooler 
{
    Dictionary<Defines.TileType, List<GameObject>> activePool = new Dictionary<Defines.TileType, List<GameObject>>();
    Dictionary<Defines.TileType, Queue<GameObject>> inactivePool = new Dictionary<Defines.TileType, Queue<GameObject>>();

    GameObject tilePrepab;
    GameObject wallPrefab;
    GameObject doorPrefab;
    GameObject shallowWaterPrefab;
    GameObject deepWaterPrefab;
    GameObject upStairPrefab;
    GameObject downStairPrefab;
    GameObject playerPrefab;
    GameObject monsterPrefab;
    GameObject itemPrefab;

    public ObjectPooler
        (
        GameObject tilePrepab,
        GameObject wallPrefab,
        GameObject doorPrefab,
        GameObject shallowWaterPrefab,
        GameObject deepWaterPrefab,
        GameObject upStairPrefab,
        GameObject downStairPrefab,
        GameObject playerPrefab,
        GameObject monsterPrefab,
        GameObject itemPrefab
        )
    {
        this.tilePrepab = tilePrepab;
        this.wallPrefab = wallPrefab;
        this.doorPrefab = doorPrefab;
        this.shallowWaterPrefab = shallowWaterPrefab;
        this.deepWaterPrefab = deepWaterPrefab;
        this.upStairPrefab = upStairPrefab;
        this.downStairPrefab = downStairPrefab;
        this.playerPrefab = playerPrefab;
        this.monsterPrefab = monsterPrefab;
        this.itemPrefab = itemPrefab;

    }

    public void Return(Defines.TileType type, GameObject target)
    {
        DictionaryCheck(type);

        if (activePool[type].Remove(target))
        {
            inactivePool[type].Enqueue(target);
        }

    }

    public GameObject Get(Defines.TileType type)
    {
        DictionaryCheck(type);
        GameObject go;
        if (inactivePool[type].Count>0)
        {
            go = inactivePool[type].Dequeue();
        }
        else
        {
            go = CreateNew(type);
        }

        activePool[type].Add(go);
        return go;
    }
    GameObject CreateNew(Defines.TileType type)
    {
        GameObject go;
        switch (type)
        {
            case Defines.TileType.Tile:
                go = UnityEngine.Object.Instantiate(tilePrepab);
                break;
            case Defines.TileType.Wall:
                go = UnityEngine.Object.Instantiate(wallPrefab);
                break;
            case Defines.TileType.Door:
                go = UnityEngine.Object.Instantiate(doorPrefab);
                break;
            case Defines.TileType.ShallowWater:
                go = UnityEngine.Object.Instantiate(shallowWaterPrefab);
                break;
            case Defines.TileType.DeepWater:
                go = UnityEngine.Object.Instantiate(deepWaterPrefab);
                break;
            case Defines.TileType.Upstair:
                go = UnityEngine.Object.Instantiate(upStairPrefab);
                break;
            case Defines.TileType.DownStair:
                go = UnityEngine.Object.Instantiate(downStairPrefab);
                break;
            case Defines.TileType.Player:
                go = UnityEngine.Object.Instantiate(playerPrefab);
                break;
            case Defines.TileType.Monster:
                go = UnityEngine.Object.Instantiate(monsterPrefab);
                break;
            case Defines.TileType.Item:
                go = UnityEngine.Object.Instantiate(itemPrefab);
                break;
            default:
                Debug.Log("TileType Error ! ");
                go = new GameObject();
                break;
        }
        return go;
    }

    void DictionaryCheck(Defines.TileType type)
    {
        if (!activePool.ContainsKey(type))
        {
            activePool.Add(type, new List<GameObject>());
        }
        if (!inactivePool.ContainsKey(type))
        {
            inactivePool.Add(type, new Queue<GameObject>());
        }
    }

    
}
