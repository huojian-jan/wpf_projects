// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.ThemeBaseModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class ThemeBaseModel : BaseViewModel
  {
    private bool _selected;

    public string Key { get; set; }

    public List<ThemeBaseModel> Parent { get; set; }

    public string Name { get; set; }

    public SolidColorBrush Color { get; set; }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public void SetSelected()
    {
      this.Parent.Where<ThemeBaseModel>((Func<ThemeBaseModel, bool>) (item => item.Selected && item.Key != this.Key)).ToList<ThemeBaseModel>().ForEach((Action<ThemeBaseModel>) (item => item.Selected = false));
      this.Selected = true;
    }
  }
}
