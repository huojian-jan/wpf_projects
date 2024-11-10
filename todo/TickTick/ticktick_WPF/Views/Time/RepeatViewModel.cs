// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.RepeatViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class RepeatViewModel : UpDownSelectViewModel
  {
    public string Title { get; set; }

    public string Value { get; set; }

    public string Desc { get; set; }

    public bool IsSplit { get; set; }

    public RepeatViewModel()
    {
      this.IsSplit = true;
      this.IsEnable = false;
    }

    public RepeatViewModel(string title, string desc, string value, bool selected)
    {
      this.Title = title;
      this.Desc = desc;
      this.Value = value;
      this.Selected = selected;
    }
  }
}
