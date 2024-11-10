// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.EmojiItemModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Views
{
  public class EmojiItemModel
  {
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("skin")]
    public List<string> Skin { get; set; }
  }
}
