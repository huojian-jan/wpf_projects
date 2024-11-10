// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchExtra
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchExtra
  {
    public string SearchKey;
    public List<string> Tags;
    public string SearchId;

    public SearchExtra()
    {
    }

    public SearchExtra(SearchHistoryModel model)
    {
      this.SearchKey = model.keyText;
      string tags = model.tags;
      List<string> stringList;
      if (tags == null)
        stringList = (List<string>) null;
      else
        stringList = ((IEnumerable<string>) tags.Split('#')).ToList<string>();
      if (stringList == null)
        stringList = new List<string>();
      this.Tags = stringList;
      this.Tags?.Remove("");
    }

    public bool Empty()
    {
      if (!string.IsNullOrWhiteSpace(this.SearchKey))
        return false;
      return this.Tags == null || this.Tags.Count == 0;
    }
  }
}
