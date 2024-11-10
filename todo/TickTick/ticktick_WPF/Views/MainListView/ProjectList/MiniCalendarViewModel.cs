// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.MiniCalendarViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class MiniCalendarViewModel : BaseViewModel
  {
    private bool _miniMode = true;

    public bool MiniMode
    {
      get => this._miniMode;
      set
      {
        this._miniMode = value;
        this.OnPropertyChanged(nameof (MiniMode));
      }
    }
  }
}
