// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.StickySpacingSelectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class StickySpacingSelectViewModel : BaseViewModel
  {
    private bool _selected;
    public int Spacing;
    private readonly List<StickySpacingSelectViewModel> _parentList;

    public string Title { get; set; }

    public int DisplayMargin { get; set; }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public StickySpacingSelectViewModel(
      string title,
      int margin,
      int spacing,
      List<StickySpacingSelectViewModel> parent)
    {
      this.Title = title;
      this.Spacing = spacing;
      this.DisplayMargin = margin;
      this._parentList = parent;
      this._selected = LocalSettings.Settings.ExtraSettings.StickySpacing == spacing;
    }

    public void SetSelected()
    {
      this._parentList?.ForEach((Action<StickySpacingSelectViewModel>) (m => m.Selected = false));
      this.Selected = true;
    }
  }
}
