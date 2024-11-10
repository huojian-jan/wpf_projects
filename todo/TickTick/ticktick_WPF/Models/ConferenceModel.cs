// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ConferenceModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class ConferenceModel : BaseModel
  {
    [JsonProperty("entryPoints")]
    public List<EntryPointsModel> EntryPoints { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
  }
}
