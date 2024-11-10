// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.BindCalendarViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.ObjectModel;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class BindCalendarViewModel : BaseViewModel
  {
    public string Id { get; set; }

    public string Name { get; set; }

    public string Color { get; set; }

    public ObservableCollection<ComboBoxViewModel> StatusItems { get; set; }
  }
}
