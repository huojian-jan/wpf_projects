// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.ProjectMenuItemHoverModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class ProjectMenuItemHoverModel : BaseViewModel
  {
    public static ProjectMenuItemHoverModel Model = new ProjectMenuItemHoverModel();
    private PtfType _hoverType = PtfType.Null;
    private string _teamId = string.Empty;

    public string TeamId
    {
      get => this._teamId;
      set
      {
        this._teamId = value;
        this.OnPropertyChanged(nameof (TeamId));
      }
    }

    public PtfType HoverType
    {
      get => this._hoverType;
      set
      {
        if (this._hoverType == value)
          return;
        this._hoverType = value;
        this.OnPropertyChanged(nameof (HoverType));
      }
    }
  }
}
