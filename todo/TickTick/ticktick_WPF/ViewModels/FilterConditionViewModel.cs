// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.FilterConditionViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class FilterConditionViewModel : BaseViewModel
  {
    private bool _canSelectLogic;
    private bool _isAllSelected = true;
    private LogicType _logic = LogicType.Or;
    private bool _showAll;
    private List<LogicType> _supportedLogic;
    private CondType _type;
    private Geometry _allData = Utils.GetIconData("IcAllProject");
    private int _allPathWidth = 16;

    public CondType Type
    {
      get => this._type;
      set
      {
        this._type = value;
        this.OnPropertyChanged(nameof (Type));
      }
    }

    public LogicType Logic
    {
      get => this._logic;
      set
      {
        this._logic = value;
        this.OnPropertyChanged(nameof (Logic));
      }
    }

    public bool CanSelectLogic
    {
      get => this._canSelectLogic;
      set
      {
        this._canSelectLogic = value;
        this.OnPropertyChanged(nameof (CanSelectLogic));
      }
    }

    public bool WithTagsSelected { get; set; }

    public bool WithoutTagsSelected { get; set; }

    public bool AllLogicEnabled { get; set; } = true;

    public bool ShowAll
    {
      get => this._showAll;
      set
      {
        this._showAll = value;
        this.OnPropertyChanged(nameof (ShowAll));
      }
    }

    public ObservableCollection<FilterListItemViewModel> ItemsSource { get; set; }

    public bool IsAllSelected
    {
      get => this._isAllSelected;
      set
      {
        this._isAllSelected = value;
        this.OnPropertyChanged(nameof (IsAllSelected));
      }
    }

    public List<LogicType> SupportedLogic
    {
      get => this._supportedLogic;
      set
      {
        this._supportedLogic = value;
        this.OnPropertyChanged(nameof (SupportedLogic));
      }
    }

    public Geometry AllData
    {
      get => this._allData;
      set
      {
        this._allData = value;
        this.OnPropertyChanged(nameof (AllData));
      }
    }

    public Visibility AllIconVisible { get; set; }

    public int AllPathWidth
    {
      get => this._allPathWidth;
      set
      {
        this._allPathWidth = value;
        this.OnPropertyChanged(nameof (AllPathWidth));
      }
    }

    public bool ShowLogic
    {
      get => this._supportedLogic.Count > 0 && !this.ShowAll;
      set => this.OnPropertyChanged(nameof (ShowLogic));
    }
  }
}
