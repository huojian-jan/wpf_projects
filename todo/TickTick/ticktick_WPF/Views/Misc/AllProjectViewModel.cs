// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.AllProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class AllProjectViewModel : SelectableItemViewModel
  {
    public AllProjectViewModel()
    {
      this.Title = Utils.GetString("All");
      this.ShowIcon = false;
      this.CanMultiSelect = false;
      this.IsBold = true;
      this.Id = "all";
    }
  }
}
