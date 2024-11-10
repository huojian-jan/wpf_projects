// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ListDiff.Diff`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util.ListDiff
{
  public class Diff<T>
  {
    public Operation Operation { get; }

    public IReadOnlyList<T> Items { get; set; }

    public Diff(Operation operation, IReadOnlyList<T> items)
    {
      this.Operation = operation;
      this.Items = items;
    }

    public override string ToString()
    {
      string str = string.Join("", this.Items.Select<T, string>((Func<T, string>) (t => t.ToString()))).Replace('\n', '¶');
      return "Diff(" + this.Operation.ToString() + ",\"" + str + "\")";
    }

    public override bool Equals(object obj) => obj is Diff<T> diff && this.Equals(diff);

    public bool Equals(Diff<T> obj)
    {
      return obj != null && obj.Operation == this.Operation && obj.Items.SequenceEqual<T>((IEnumerable<T>) this.Items);
    }

    public override int GetHashCode() => this.Items.GetHashCode() ^ this.Operation.GetHashCode();
  }
}
