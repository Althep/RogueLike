using UnityEngine;
using System;
using System.Collections.Generic;
public class Node : IComparable<Node>
{
    public int x, y;
    public int G;
    public int H;
    public int F => G + H;
    public Node parent;

    public Node(int x, int y) { this.x = x; this.y = y; }

    public int CompareTo(Node other) => F.CompareTo(other.F);
}

public class PriorityQueue<T> where T : IComparable<T>
{
    List<T> heap = new List<T>();

    public void Push(T data)
    {
        heap.Add(data);
        int now = heap.Count - 1;
        while (now > 0)
        {
            int next = (now - 1) / 2;
            if (heap[now].CompareTo(heap[next]) >= 0)
                break;

            T temp = heap[now];
            heap[now] = heap[next];
            heap[next] = temp;
            now = next;
        }
    }

    public T Pop()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("The heap is empty.");

        T ret = heap[0];
        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);

        int now = 0;
        while (true)
        {
            int left = now * 2 + 1;
            int right = now * 2 + 2;
            int next = now;

            if (left < heap.Count && heap[left].CompareTo(heap[next]) < 0)
                next = left;
            if (right < heap.Count && heap[right].CompareTo(heap[next]) < 0)
                next = right;

            if (next == now) break;

            (heap[now], heap[next]) = (heap[next], heap[now]);
            now = next;
        }

        return ret;
    }
    public void Clear() => heap.Clear();
    public int Count => heap.Count;
}
