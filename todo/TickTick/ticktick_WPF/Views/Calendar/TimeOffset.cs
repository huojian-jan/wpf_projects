// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TimeOffset
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TimeOffset : BaseViewModel
  {
    private bool _foldExpanded;
    private bool _showBottomHandle;
    private bool _showTopHandle;
    private bool _isFirst;

    public int Offset { get; set; }

    public bool Folded { get; set; }

    public bool ShowDivider { get; set; }

    public bool IsFirst
    {
      get => this._isFirst;
      set
      {
        this._isFirst = value;
        this.OnPropertyChanged(nameof (IsFirst));
      }
    }

    public bool ShowBottomHandle
    {
      get => this._showBottomHandle;
      set
      {
        this._showBottomHandle = value;
        this.OnPropertyChanged(nameof (ShowBottomHandle));
      }
    }

    public bool ShowTopHandle
    {
      get => this._showTopHandle;
      set
      {
        this._showTopHandle = value;
        this.OnPropertyChanged(nameof (ShowTopHandle));
      }
    }

    public bool FoldExpanded
    {
      get => this._foldExpanded;
      set
      {
        this._foldExpanded = value;
        this.OnPropertyChanged(nameof (FoldExpanded));
      }
    }
  }
}
