// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.CacheBase`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace ticktick_WPF.Cache
{
  public abstract class CacheBase<T> : ICacheable
  {
    private readonly ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();

    public abstract Task Load();

    public async Task Clear() => this._cache.Clear();

    public ConcurrentDictionary<string, T> GetData() => this._cache;

    public void Update(string key, T data)
    {
      if ((object) data == null || key == null)
        return;
      this._cache[key] = data;
    }

    public void Delete(string key)
    {
      if (key == null || !this._cache.ContainsKey(key))
        return;
      this._cache.TryRemove(key, out T _);
    }

    protected void AssembleData(IEnumerable<T> data, Func<T, string> getId)
    {
      this._cache.Clear();
      foreach (T obj in data)
      {
        string key = getId(obj);
        if (!string.IsNullOrEmpty(key) && !this._cache.ContainsKey(key))
          this._cache.TryAdd(key, obj);
      }
    }
  }
}
