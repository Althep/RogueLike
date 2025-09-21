using UnityEngine;
using System;
using System.Collections.Generic;
public class Astar 
{
    public List<Node> path = new List<Node>();
    Dictionary<Vector2Int, Defines.TileType> tileData;
    MapMaker mapMaker;
    GameObject myObj;
    LivingEntity myEntity;

    Astar(GameObject myObj)
    {
        if(tileData == null)
        {
            tileData = GameManager.instance.Get_MapManager().tileData;
        }
        if(mapMaker == null)
        {
            mapMaker = GameManager.instance.Get_MapManager().Get_MapMaker();
        }
        if(this.myObj == null)
        {
            this.myObj = myObj;
        }
        if(myEntity == null)
        {
            myObj.transform.GetComponent<LivingEntity>();
        }
    }

    public void Get_Path(Vector2 Dest)
    {
        
        if (path.Count > 0)
        {
            path.Clear();
        }
        bool[,] closed;
        int[,] open;
        int[] dx = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = new int[] { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] cost = new int[] { 8, 10, 8, 10, 10, 8, 10, 8 };//{ 14, 10, 14, 10, 10, 14, 10, 14 };
        if (tileData == null)
        {
            Debug.Log("MapScript Null");
            tileData = GameManager.instance.Get_MapManager().tileData;
        }
        
        Node[,] parents = new Node[mapMaker.ySize, mapMaker.xSize];
        PriorityQueue<Node> q = new PriorityQueue<Node>();
        Node start = new Node();
        closed = new bool[mapMaker.ySize, mapMaker.xSize];
        open = new int[mapMaker.ySize, mapMaker.xSize];
        //int[,] cost = new int[mapMakeScript.ySize,mapMakeScript.xSize];
        Vector2 nowPos = new Vector2();
        for (int i = 0; i < mapMaker.ySize; i++)
        {
            for (int j = 0; j < mapMaker.xSize; j++)
            {
                open[i, j] = Int32.MaxValue;
            }
        }
        nowPos = myObj.transform.position;
        start.x = (int)nowPos.x;
        start.y = (int)nowPos.y;
        start.F = (int)(Vector2.Distance(Dest, nowPos) * 10);//((int)Math.Abs(Dest.x - start.x) + (int)Math.Abs(Dest.y - start.y))*10;
        start.G = 0;
        parents[start.y, start.x] = start;
        q.Push(start);
        while (q.Count > 0)
        {
            Node now = q.Pop();
            closed[now.y, now.x] = true;
            if (now.x == Dest.x && now.y == Dest.y)
            {
                GetPath(now, parents);
                break;
            }
            for (int i = 0; i < dx.Length; i++)
            {
                Vector2Int closePos = new Vector2Int();
                closePos.x = now.x + dx[i];
                closePos.y = now.y + dy[i];
                int f = (int)(Vector2.Distance(Dest, closePos) * 10) + cost[i];//((int)Math.Abs(Dest.x - closePos.x) + (int)Math.Abs(Dest.y - closePos.y)) * 10 + cost[i];//
                if (!tileData.ContainsKey(closePos))
                    continue;
                if (closed[(int)closePos.y, (int)closePos.x])
                    continue;
                if (f > open[(int)closePos.y, (int)closePos.x])
                    continue;
                if (tileData[closePos] != Defines.TileType.Tile && tileData[closePos] != Defines.TileType.Door && tileData[closePos] != Defines.TileType.Player)
                    continue;
                Node next = new Node()
                {
                    x = (int)closePos.x,
                    y = (int)closePos.y,
                    F = f,// ((int)Math.Abs(Dest.y-closePos.y) + (int)Math.Abs(Dest.x-closePos.x))*10+cost[i],
                    G = cost[i]
                };
                q.Push(next);
                //Debug.Log("Next : "+next.x +","+next.y);
                open[next.y, next.x] = next.F;
                parents[next.y, next.x] = now;
            }
        }
    }

    public void GetPath(Node LastNode, Node[,] parents)
    {
        while (LastNode.x != (int)myObj.transform.position.x || LastNode.y != (int)myObj.transform.position.y)
        {
            if (Vector2.Distance(myObj.transform.position, GameManager.instance.Get_PlayerObj().transform.position) <= myEntity.Get_MyData().attackRange)
            {
                LastNode = parents[LastNode.y, LastNode.x];
                path.Add(LastNode);
                break;
            }
            if (parents.Length == 0)
            {
                break;
            }
            LastNode = parents[LastNode.y, LastNode.x];
            path.Add(LastNode);
        }
        path.Reverse();
        /*
        for (int i = 0; i < path.Count - 1; i++)
        {
            drawingStart = new Vector2(path[i].x, path[i].y);
            drawingEnd = new Vector2(path[i + 1].x, path[i + 1].y);
            Debug.DrawLine(drawingStart, drawingEnd, Color.red, 30f);
            //Debug.Log("Path : "+"i : " + path[i]);
        }
        */
    }
}
