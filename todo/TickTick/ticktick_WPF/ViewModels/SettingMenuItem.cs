// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.SettingMenuItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class SettingMenuItem : BaseViewModel
  {
    private bool _expanded;
    private bool _visible = true;
    private bool _isSelected;
    private bool _isSubItem;
    private bool _hasChildren;
    private bool _showNew;

    public SettingMenuItem() => this.IsSplit = true;

    public SettingMenuItem(string title, SettingsType type, SettingsType parent)
    {
      this.Title = Utils.GetString(title);
      this.Type = type;
      this.IsSubItem = true;
      this.ParentType = parent;
      this.Visible = false;
    }

    public SettingMenuItem(string title, SettingsType type)
    {
      this.Title = Utils.GetString(title);
      this.Type = type;
      this.Icon = Utils.GetIcon("SettingsIc" + type.ToString());
    }

    public string Title { get; set; }

    public Geometry Icon { get; set; }

    public SettingsType Type { get; set; }

    public bool IsSplit { get; set; }

    public bool IsSubItem
    {
      get => this._isSubItem;
      set
      {
        this._isSubItem = value;
        this.OnPropertyChanged(nameof (IsSubItem));
      }
    }

    public SettingsType ParentType { get; set; }

    public bool HasChildren
    {
      get => this._hasChildren;
      set
      {
        this._hasChildren = value;
        this.OnPropertyChanged(nameof (HasChildren));
      }
    }

    public List<SettingMenuItem> Children { get; set; }

    public bool Expanded
    {
      get => this._expanded;
      set
      {
        this._expanded = value;
        this.OnPropertyChanged(nameof (Expanded));
      }
    }

    public bool IsSelected
    {
      get => this._isSelected;
      set
      {
        if (value == this._isSelected)
          return;
        this._isSelected = value;
        if (value && this._showNew)
          SettingsMenuHelper.CheckNewFeature(this.Type);
        this.OnPropertyChanged(nameof (IsSelected));
      }
    }

    public bool Visible
    {
      get => this._visible;
      set
      {
        this._visible = value;
        this.OnPropertyChanged(nameof (Visible));
      }
    }

    public bool ShowNew
    {
      get => this._showNew;
      set
      {
        this._showNew = value;
        this.OnPropertyChanged(nameof (ShowNew));
      }
    }
  }
}
