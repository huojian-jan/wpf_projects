// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TimeViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public class TimeViewModel : BaseViewModel
  {
    private bool _selected;
    private bool _suggested;
    private bool _showSpanText;

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public bool Suggested
    {
      get => this._suggested;
      set
      {
        this._suggested = value;
        this.OnPropertyChanged(nameof (Suggested));
      }
    }

    public string DisplayText { get; set; }

    public string TimeSpanText { get; set; }

    public bool ShowSpanText
    {
      get => this._showSpanText;
      set
      {
        this._showSpanText = value;
        this.OnPropertyChanged(nameof (ShowSpanText));
      }
    }

    public DateTime Value { get; set; }
  }
}
