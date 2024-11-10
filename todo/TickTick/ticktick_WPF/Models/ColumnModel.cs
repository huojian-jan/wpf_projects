// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ColumnModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class ColumnModel : BaseModel
  {
    [Indexed]
    public string id { get; set; }

    public string userId { get; set; }

    [Indexed]
    public string projectId { get; set; }

    public string name { get; set; }

    public long? sortOrder { get; set; }

    public int deleted { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? createdTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? modifiedTime { get; set; }

    public string etag { get; set; }

    public string syncStatus { get; set; }
  }
}
