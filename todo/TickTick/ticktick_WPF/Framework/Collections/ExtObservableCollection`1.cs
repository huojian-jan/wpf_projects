// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Framework.Collections.ExtObservableCollection`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

#nullable disable
namespace ticktick_WPF.Framework.Collections
{
  public class ExtObservableCollection<T> : ObservableCollection<T>
  {
    protected override void ClearItems()
    {
      List<T> changedItems = new List<T>((IEnumerable<T>) this);
      base.ClearItems();
      this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList) changedItems));
    }
  }
}
