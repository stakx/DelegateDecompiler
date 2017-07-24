#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System;

/// <summary>
/// A stack data structure that is based on a fixed-size array
/// and allows popping several values at once.
/// </summary>
internal sealed class ArrayStack<T>
{
    private T[] items;
    private int capacity;
    private int count;

    public ArrayStack(int capacity)
    {
        this.capacity = capacity;
        this.items = new T[capacity];
    }

    public T Peek()
    {
        return this.items[this.count - 1];
    }

    public T Pop()
    {
        return this.items[--this.count];
    }

    public T[] Pop(int count)
    {
        var copy = new T[count];
        this.count -= count;
        Array.Copy(this.items, this.count, copy, 0, count);
        return copy;
    }

    public void Push(T value)
    {
        this.items[this.count++] = value;
    }
}
