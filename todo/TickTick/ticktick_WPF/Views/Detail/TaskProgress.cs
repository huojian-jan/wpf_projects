// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskProgress
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskProgress : BaseViewModel
  {
    private int _percent;
    private int _previewPercent;
    private bool _showPointer;
    private double _width;

    public double Width
    {
      get => this._width;
      set
      {
        this._width = value;
        this.OnPropertyChanged(nameof (Width));
      }
    }

    public int PreviewPercent
    {
      get => this._previewPercent;
      set
      {
        this._previewPercent = value;
        this.OnPropertyChanged(nameof (PreviewPercent));
      }
    }

    public int Percent
    {
      get => this._percent;
      set
      {
        this._percent = value;
        this.OnPropertyChanged(nameof (Percent));
      }
    }

    public bool ShowPointer
    {
      get => this._showPointer;
      set
      {
        this._showPointer = value;
        this.OnPropertyChanged(nameof (ShowPointer));
      }
    }
  }
}
