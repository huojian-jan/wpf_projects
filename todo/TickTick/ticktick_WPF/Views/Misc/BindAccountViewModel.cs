// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.BindAccountViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class BindAccountViewModel : SelectableItemViewModel
  {
    public BindAccountViewModel(string id, string title, string kind)
    {
      this.Id = id;
      this.Icon = SubscribeCalendarHelper.GetCalendarProjectIconById(id);
      this.Title = title;
      this.Type = "bind_account";
      this.Selectable = true;
      this.CanMultiSelect = true;
    }
  }
}
