// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.BlockingList`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Models
{
  public class BlockingList<T>
  {
    private readonly List<T> _models = new List<T>();

    public event EventHandler<ListItemChangeArgs<T>> ItemsChange;

    public BlockingList()
    {
    }

    public BlockingList(IEnumerable<T> models) => this._models = models.ToList<T>();

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

    public List<T> Value => this._models;

    public T this[int index]
    {
      get
      {
        lock (this._models)
          return this._models[index];
      }
      set
      {
        lock (this._models)
        {
          T model = this._models[index];
          this._models[index] = value;
          EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
          if (itemsChange == null)
            return;
          itemsChange((object) this, new ListItemChangeArgs<T>()
          {
            Action = ListChangeAction.Replace,
            Items = new List<T>() { value },
            OldItems = new List<T>() { model }
          });
        }
      }
    }

    public void Add(T model)
    {
      lock (this._models)
      {
        this._models.Add(model);
        EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
        if (itemsChange == null)
          return;
        itemsChange((object) this, new ListItemChangeArgs<T>()
        {
          Action = ListChangeAction.Add,
          Items = new List<T>() { model }
        });
      }
    }

    public void AddRange(IEnumerable<T> models)
    {
      lock (this._models)
      {
        this._models.AddRange(models);
        EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
        if (itemsChange == null)
          return;
        itemsChange((object) this, new ListItemChangeArgs<T>()
        {
          Action = ListChangeAction.Add,
          Items = models.ToList<T>()
        });
      }
    }

    public T RemoveAt(int index)
    {
      lock (this._models)
      {
        if (this._models.Count <= index)
          return default (T);
        T model = this._models[index];
        this._models.RemoveAt(index);
        EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
        if (itemsChange != null)
          itemsChange((object) this, new ListItemChangeArgs<T>()
          {
            Action = ListChangeAction.Remove,
            Items = new List<T>() { model }
          });
        return model;
      }
    }

    public List<TResult> SelectList<TResult>(Func<T, TResult> match)
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
        List<T> all = this._models.FindAll(match);
        this._models.RemoveAll(match);
        EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
        if (itemsChange != null)
          itemsChange((object) this, new ListItemChangeArgs<T>()
          {
            Action = ListChangeAction.Remove,
            Items = all
          });
        return all;
      }
    }

    public void Clear()
    {
      lock (this._models)
      {
        EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
        if (itemsChange != null)
          itemsChange((object) this, new ListItemChangeArgs<T>()
          {
            Action = ListChangeAction.Clear,
            Items = this._models.ToList<T>()
          });
        this._models.Clear();
      }
    }

    public void Foreach(Action<T> func)
    {
      lock (this._models)
        this._models.ForEach(func);
    }

    public T FirstOrDefault(Func<T, bool> func)
    {
      lock (this._models)
        return this._models.FirstOrDefault<T>(func);
    }

    public bool Contains(T m)
    {
      lock (this._models)
        return this._models.Contains(m);
    }

    public void Remove(T m)
    {
      lock (this._models)
      {
        this._models.Remove(m);
        EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
        if (itemsChange == null)
          return;
        itemsChange((object) this, new ListItemChangeArgs<T>()
        {
          Action = ListChangeAction.Remove,
          Items = new List<T>() { m }
        });
      }
    }

    public bool All(Func<T, bool> func)
    {
      lock (this._models)
        return this._models.All<T>(func);
    }

    public int IndexOf(T model)
    {
      lock (this._models)
        return this._models.IndexOf(model);
    }

    public void NotifyItemChanged(T model)
    {
      if (!this.Contains(model))
        return;
      EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
      if (itemsChange == null)
        return;
      itemsChange((object) this, new ListItemChangeArgs<T>()
      {
        Action = ListChangeAction.Change,
        Items = new List<T>() { model }
      });
    }

    public void NotifyItemsChanged(List<T> models)
    {
      EventHandler<ListItemChangeArgs<T>> itemsChange = this.ItemsChange;
      if (itemsChange == null)
        return;
      itemsChange((object) this, new ListItemChangeArgs<T>()
      {
        Action = ListChangeAction.Change,
        Items = models
      });
    }

    public void ClearEvents() => this.ItemsChange = (EventHandler<ListItemChangeArgs<T>>) null;

    public bool Exists(Predicate<T> match)
    {
      lock (this._models)
        return this._models.Any<T>((Func<T, bool>) (item => match(item)));
    }

    public int GetCount(Predicate<T> match)
    {
      lock (this._models)
        return this._models.Count<T>((Func<T, bool>) (item => match(item)));
    }

    public void Do(Action action)
    {
      lock (this._models)
        action();
    }

    public void Insert(int index, T model)
    {
      lock (this._models)
        this._models.Insert(index, model);
    }

    public void Sort(Func<T, T, int> func)
    {
      lock (this._models)
        this._models.Sort((Comparison<T>) ((a, b) => func(a, b)));
    }
  }
}
