// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ListItemData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class ListItemData : UpDownSelectViewModel
  {
    public object Key { get; set; }

    public string Value { get; set; }

    public ListItemData(object key, string value)
    {
      this.Key = key;
      this.Value = value;
    }
  }
}
