// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.EventUpdateResult
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util
{
  public class EventUpdateResult
  {
    private Dictionary<string, Dictionary<string, string>> id2error = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, Dictionary<string, string>> id2etag = new Dictionary<string, Dictionary<string, string>>();

    [JsonProperty(PropertyName = "accountId")]
    public string AccountId { get; set; }

    [JsonProperty(PropertyName = "errorCode")]
    public string ErrorCode { get; set; }

    [JsonProperty(PropertyName = "id2etag")]
    public Dictionary<string, Dictionary<string, string>> Id2etag
    {
      get => this.id2etag;
      set => this.id2etag = value;
    }

    [JsonProperty(PropertyName = "id2error")]
    public Dictionary<string, Dictionary<string, string>> Id2error
    {
      get => this.id2error;
      set => this.id2error = value;
    }
  }
}
