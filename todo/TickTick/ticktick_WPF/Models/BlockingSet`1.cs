// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.BlockingSet`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Models
{
  public class BlockingSet<T>
  {
    private readonly HashSet<T> _models = new HashSet<T>();

    public HashSet<T> Value
    {
      get
      {
        lock (this._models)
          return this._models;
      }
    }

    public BlockingSet()
    {
    }

    public BlockingSet(IEnumerable<T> models) => this._models = new HashSet<T>(models);

    public List<T> ToList()
    {
      lock (this._models)
        return this._models.ToList<T>();
    }

    public int Count
    {
      get
      {
        lock (this._models)
          return this._models.Count;
      }
    }

    public bool Add(T model)
    {
      lock (this._models)
        return this._models.Add(model);
    }

    public void AddRange(IEnumerable<T> models, bool checkNull = false)
    {
      if (models == null)
        return;
      lock (this._models)
      {
        foreach (T model in models)
        {
          if (!checkNull || (object) model != null)
            this._models.Add(model);
        }
      }
    }

    public void AddRange(BlockingSet<T> set) => this.AddRange((IEnumerable<T>) set?._models);

    public List<TResult> Select<TResult>(Func<T, TResult> match)
    {
      lock (this._models)
        return this._models.Select<T, TResult>(match).ToList<TResult>();
    }

    public List<T> Where(Predicate<T> match)
    {
      lock (this._models)
        return this._models.Where<T>((Func<T, bool>) (item => match(item))).ToList<T>();
    }

    public List<T> RemoveAll(Predicate<T> match)
    {
      lock (this._models)
      {
        List<T> list = this._models.Where<T>((Func<T, bool>) (item => match(item))).ToList<T>();
        this.RemoveAll((IEnumerable<T>) list);
        return list;
      }
    }

    private void RemoveAll(IEnumerable<T> removeModels)
    {
      lock (this._models)
      {
        foreach (T removeModel in removeModels)
          this._models.Remove(removeModel);
      }
    }

    public void Clear()
    {
      lock (this._models)
        this._models.Clear();
    }

    public T FirstOrDefault(Func<T, bool> func)
    {
      lock (this._models)
        return this._models.FirstOrDefault<T>(func);
    }

    public T FirstOrDefault()
    {
      lock (this._models)
        return this._models.FirstOrDefault<T>();
    }

    public bool Contains(T m)
    {
      lock (this._models)
        return this._models.Contains(m);
    }

    public bool Remove(T m)
    {
      lock (this._models)
        return this._models.Remove(m);
    }

    public bool Any()
    {
      lock (this._models)
        return this._models.Any<T>();
    }

    public BlockingSet<T> Copy()
    {
      lock (this._models)
        return new BlockingSet<T>((IEnumerable<T>) this.ToList());
    }
  }
}
