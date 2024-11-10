// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ListDiff.EmptyList`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.ListDiff
{
  internal class EmptyList<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
  {
    public static readonly EmptyList<T> Instance = new EmptyList<T>();

    public IEnumerator<T> GetEnumerator()
    {
      yield break;
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    public int Count => 0;

    public T this[int index] => throw new IndexOutOfRangeException();
  }
}
