// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SearchHistoryModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SearchHistoryModel : BaseModel
  {
    public string userId { get; set; }

    public string keyText { get; set; }

    public string tags { get; set; }

    public long sortOrder { get; set; }
  }
}
