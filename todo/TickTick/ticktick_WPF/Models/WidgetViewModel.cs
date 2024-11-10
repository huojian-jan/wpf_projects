// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.WidgetViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class WidgetViewModel : BaseViewModel
  {
    private readonly WidgetSettingModel _data;
    private readonly CalendarWidgetModel _singleData;
    public readonly string Id;
    public string Identity;
    public string Type;
    private string _themeId;
    private float _opacity;
    public string DisplayOption;
    private bool _autoHide;
    private bool _hideComplete;
    private double _left;
    private double _top;
    private double _width;
    private double _height;
    private string _collapseTitle;
    private bool _isLocked;
    private int _mode;

    public string SortType { get; set; }

    public string GroupType { get; set; }

    public bool IsCal => this._singleData != null && this._singleData.mode == 0;

    public bool IsMatrix => this._singleData != null && this._singleData.mode == 1;

    public bool IsSingleMode => this._singleData != null;

    public WidgetViewModel()
    {
    }

    public WidgetViewModel(WidgetSettingModel settingModel)
    {
      this._data = settingModel;
      this.Id = settingModel.id;
      this.Type = settingModel.type;
      this.Identity = settingModel.identity;
      this._themeId = settingModel.themeId;
      this._opacity = settingModel.opacity;
      this.DisplayOption = settingModel.displayOption;
      this._autoHide = settingModel.autoHide;
      this.SortType = settingModel.sortType;
      this.GroupType = settingModel.groupType;
      this._hideComplete = settingModel.hideComplete;
      this._isLocked = settingModel.isLocked;
      this._left = settingModel.left;
      this._top = settingModel.top;
      this._width = settingModel.width;
      this._height = settingModel.height;
      if (string.IsNullOrEmpty(this.SortType))
        this.SortType = "dueDate";
      if (this.GroupType != null)
        return;
      if (this.SortType == "title" || this.SortType == "createdTime" || this.SortType == "modifiedTime")
      {
        this.GroupType = "none";
      }
      else
      {
        this.GroupType = this.SortType;
        this.SortType = this.SortType == "sortOrder" ? "sortOrder" : "dueDate";
      }
    }

    public WidgetViewModel(CalendarWidgetModel settingModel)
    {
      this._singleData = settingModel;
      this.Id = settingModel.id;
      this._themeId = settingModel.themeId;
      this._opacity = settingModel.opacity;
      this.DisplayOption = settingModel.displayOption;
      this._autoHide = settingModel.autoHide;
      this._hideComplete = settingModel.hideComplete;
      this._isLocked = settingModel.isLocked;
      this._mode = settingModel.mode;
      this._left = settingModel.left;
      this._top = settingModel.top;
      this._width = settingModel.width;
      this._height = settingModel.height;
    }

    public async Task Save()
    {
      if (this.IsSingleMode)
      {
        await this.SaveSingleModel();
      }
      else
      {
        this._data.opacity = this._opacity;
        this._data.left = this._left;
        this._data.top = this._top;
        this._data.width = this._width;
        this._data.height = this._height;
        this._data.identity = this.Identity;
        this._data.type = this.Type;
        this._data.autoHide = this._autoHide;
        this._data.hideComplete = this._hideComplete;
        this._data.displayOption = this.DisplayOption;
        this._data.sortType = this.SortType;
        this._data.groupType = this.GroupType ?? this.SortType;
        this._data.themeId = this._themeId;
        this._data.isLocked = this._isLocked;
        await ProjectWidgetDao.UpdateWidget(this._data);
      }
    }

    public async Task SaveSingleModel()
    {
      this._singleData.opacity = this._opacity;
      this._singleData.left = this._left;
      this._singleData.top = this._top;
      this._singleData.width = this._width;
      this._singleData.height = this._height;
      this._singleData.autoHide = this._autoHide;
      this._singleData.hideComplete = this._hideComplete;
      this._singleData.displayOption = this.DisplayOption;
      this._singleData.themeId = this.ThemeId;
      this._singleData.mode = this._mode;
      this._singleData.isLocked = this._isLocked;
      await SingleWidgetDao.SaveWidget(this._singleData);
    }

    public float Opacity
    {
      get => this._opacity;
      set
      {
        this._opacity = value;
        this.OnPropertyChanged(nameof (Opacity));
      }
    }

    public double Left
    {
      get => this._left;
      set
      {
        this._left = value;
        this.OnPropertyChanged(nameof (Left));
      }
    }

    public double Top
    {
      get => this._top;
      set
      {
        this._top = value;
        this.OnPropertyChanged(nameof (Top));
      }
    }

    public double Width
    {
      get => this._width;
      set
      {
        this._width = value;
        this.OnPropertyChanged(nameof (Width));
      }
    }

    public double Height
    {
      get => this._height;
      set
      {
        this._height = value;
        this.OnPropertyChanged(nameof (Height));
      }
    }

    public bool AutoHide
    {
      get => this._autoHide;
      set
      {
        this._autoHide = value;
        this.OnPropertyChanged(nameof (AutoHide));
      }
    }

    public bool HideComplete
    {
      get => this._hideComplete;
      set
      {
        this._hideComplete = value;
        this.OnPropertyChanged(nameof (HideComplete));
      }
    }

    public string CollapseTitle
    {
      get => this._collapseTitle;
      set
      {
        this._collapseTitle = value;
        this.OnPropertyChanged(nameof (CollapseTitle));
      }
    }

    public string ThemeId
    {
      get => this._themeId;
      set
      {
        this._themeId = value;
        this.OnPropertyChanged(nameof (ThemeId));
      }
    }

    public bool IsLocked
    {
      get => this._isLocked;
      set
      {
        this._isLocked = value;
        this.OnPropertyChanged(nameof (IsLocked));
      }
    }
  }
}
