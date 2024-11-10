// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.InviteSelectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class InviteSelectViewModel : BaseViewModel
  {
    private bool _preDelete;

    public string Email { get; set; }

    public string DisplayName { get; set; }

    public bool PreDelete
    {
      get => this._preDelete;
      set
      {
        this._preDelete = value;
        this.OnPropertyChanged(nameof (PreDelete));
      }
    }

    public bool IsInput { get; set; }
  }
}
