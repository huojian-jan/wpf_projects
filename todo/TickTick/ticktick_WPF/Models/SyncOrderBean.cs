// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SyncOrderBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SyncOrderBean
  {
    [JsonProperty("orderByType")]
    public Dictionary<string, Dictionary<string, SortOrderData>> OrderByType { get; set; }

    public SyncOrderBean GetTaskOrderBean()
    {
      SyncOrderBean taskOrderBean = new SyncOrderBean();
      taskOrderBean.OrderByType = new Dictionary<string, Dictionary<string, SortOrderData>>();
      foreach (string key in this.OrderByType.Keys.ToList<string>())
      {
        if (key.StartsWith("taskBy#"))
          taskOrderBean.OrderByType[key] = this.OrderByType[key];
      }
      return taskOrderBean;
    }

    public SyncOrderBean GetOtherOrderBean()
    {
      SyncOrderBean otherOrderBean = new SyncOrderBean();
      otherOrderBean.OrderByType = new Dictionary<string, Dictionary<string, SortOrderData>>();
      foreach (string key in this.OrderByType.Keys.ToList<string>())
      {
        if (!key.StartsWith("taskBy#"))
          otherOrderBean.OrderByType[key] = this.OrderByType[key];
      }
      return otherOrderBean;
    }
  }
}
