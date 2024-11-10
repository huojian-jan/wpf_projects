// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SelectTimeSpanViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SelectTimeSpanViewModel : BaseViewModel
  {
    private DateTime? _startDate;
    private DateTime? _endDate;

    public DateTime? StartDate
    {
      get => this._startDate;
      set
      {
        this._startDate = value;
        this.OnPropertyChanged(nameof (StartDate));
      }
    }

    public DateTime? EndDate
    {
      get => this._endDate;
      set
      {
        this._endDate = value;
        this.OnPropertyChanged(nameof (EndDate));
      }
    }
  }
}
