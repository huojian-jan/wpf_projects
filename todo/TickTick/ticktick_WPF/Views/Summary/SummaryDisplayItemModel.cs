// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.SummaryDisplayItemModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Summary
{
  public class SummaryDisplayItemModel : BaseViewModel
  {
    public string Key { get; set; }

    public long SortOrder { get; set; }

    public string Style { get; set; }

    public bool Enabled { get; set; }

    public SummaryDisplayItemModel()
    {
    }

    public SummaryDisplayItemModel(string key, long sortOrder, string style, bool enabled)
    {
      this.Key = key;
      this.SortOrder = sortOrder;
      this.Style = style;
      this.Enabled = enabled;
    }
  }
}
