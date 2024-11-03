using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularBuffer<T> where T : new()
{
    T[] buffer;
    int bufferSize;

    public CircularBuffer(int bufferSize)
    {
        this.bufferSize = bufferSize;
        buffer = new T[bufferSize];

        for (int i = 0; i < bufferSize; i++)
        {
            buffer[i] = new();
        }
    }

    public void Add(T item, int index) => buffer[index % bufferSize] = item;
    public ref T Get(int index) => ref buffer[index % bufferSize];
    public void Clear() => buffer = new T[bufferSize];
}
