// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.DatePickerViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class DatePickerViewModel : BaseViewModel
  {
    private DateTime _pivotDate;
    private DateTime? _selectedDate;
    private DateTime? _selectEndDate;
    private DateTime? _selectStartDate;
    private bool _miniMode;
    private int _rowCount = 6;
    private bool _useInSlideMenu;
    private SolidColorBrush _primaryTextColor = ThemeUtil.GetColor("BaseColorOpacity80");
    private SolidColorBrush _secondaryTextColor = ThemeUtil.GetColor("BaseColorOpacity60");
    private SolidColorBrush _tertiaryTextColor = ThemeUtil.GetColor("BaseColorOpacity40");

    public DateTime PivotDate
    {
      get => this._pivotDate;
      set
      {
        this._pivotDate = value;
        this.OnPropertyChanged(nameof (PivotDate));
      }
    }

    public DateTime? SelectedDate
    {
      get => this._selectedDate;
      set
      {
        this._selectedDate = value;
        this.OnPropertyChanged(nameof (SelectedDate));
      }
    }

    public DateTime? SelectStartDate
    {
      get => this._selectStartDate;
      set
      {
        this._selectStartDate = value;
        this.OnPropertyChanged(nameof (SelectStartDate));
      }
    }

    public DateTime? SelectEndDate
    {
      get => this._selectEndDate;
      set
      {
        this._selectEndDate = value;
        this.OnPropertyChanged(nameof (SelectEndDate));
      }
    }

    public bool MiniMode
    {
      get => this._miniMode;
      set
      {
        this._miniMode = value;
        this.OnPropertyChanged(nameof (MiniMode));
      }
    }

    public int RowCount
    {
      get => this._rowCount;
      set
      {
        this._rowCount = value;
        this.OnPropertyChanged(nameof (RowCount));
      }
    }

    public bool UseInSlideMenu
    {
      get => this._useInSlideMenu;
      set
      {
        this._useInSlideMenu = value;
        this.OnPropertyChanged(nameof (UseInSlideMenu));
      }
    }

    public SolidColorBrush PrimaryTextColor
    {
      get => this._primaryTextColor;
      set
      {
        this._primaryTextColor = value;
        this.OnPropertyChanged(nameof (PrimaryTextColor));
      }
    }

    public SolidColorBrush SecondaryTextColor
    {
      get => this._secondaryTextColor;
      set
      {
        this._secondaryTextColor = value;
        this.OnPropertyChanged(nameof (SecondaryTextColor));
      }
    }

    public SolidColorBrush TertiaryTextColor
    {
      get => this._tertiaryTextColor;
      set
      {
        this._tertiaryTextColor = value;
        this.OnPropertyChanged(nameof (TertiaryTextColor));
      }
    }
  }
}
