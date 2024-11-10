// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.CalendarGroupViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class CalendarGroupViewModel : SelectableItemViewModel
  {
    public CalendarGroupViewModel()
    {
      this.Id = "#allSubscribe";
      this.Title = Utils.GetString("SubscribeCalendar");
      this.Selectable = false;
      this.ShowIcon = false;
      this.ShowIndent = true;
      this.IsSectionGroup = true;
      this.Open = true;
      this.IsParent = true;
      this.IsBold = true;
    }
  }
}
