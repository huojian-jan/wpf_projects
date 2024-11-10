// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.ColorItemSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector
{
  public class ColorItemSelector : UserControl, IComponentConnector
  {
    public static readonly List<string> MoreColorList = new List<string>()
    {
      "#F68F8A",
      "#F8C3B2",
      "#FCDEAA",
      "#C2DED0",
      "#63C5D2",
      "#6FAAF3",
      "#C09EEB",
      "#F678AC",
      "#D27762",
      "#F7C989",
      "#DCDE6B",
      "#A7C9B9",
      "#7BCAD3",
      "#86A9DE",
      "#AD94C7",
      "#C77B9B",
      "#BE7471",
      "#DD882E",
      "#E3CE7B",
      "#76A692",
      "#5DAF9C",
      "#4682B4",
      "#838BB0",
      "#CB8A90",
      "#EC6666",
      "#F2B04B",
      "#FFD966",
      "#DDE358",
      "#5DD1A8",
      "#4AA6EF",
      "#CF66F7",
      "#ED70A5",
      "#EF2D24",
      "#FF7D5D",
      "#FFD457",
      "#02DBB2",
      "#34BAC5",
      "#8C5EFA",
      "#CE89FE",
      "#F964C2"
    };
    private readonly List<ItemColorViewModel> _colorItems = new List<ItemColorViewModel>();
    private readonly ObservableCollection<ItemColorViewModel> _customItems = new ObservableCollection<ItemColorViewModel>();
    private string _selectedColor;
    internal ItemsControl McrColorItems;
    internal ItemsControl MrdColorItems;
    internal ItemsControl RccColorItems;
    internal ItemsControl ClassicColorItems;
    internal ItemsControl MphColorItems;
    internal ItemsControl CustomItems;
    private bool _contentLoaded;

    public event EventHandler<string> ColorSelect;

    public ColorItemSelector()
    {
      this.InitializeComponent();
      ObservableCollection<ItemColorViewModel> collection1 = new ObservableCollection<ItemColorViewModel>();
      ObservableCollection<ItemColorViewModel> collection2 = new ObservableCollection<ItemColorViewModel>();
      ObservableCollection<ItemColorViewModel> collection3 = new ObservableCollection<ItemColorViewModel>();
      ObservableCollection<ItemColorViewModel> collection4 = new ObservableCollection<ItemColorViewModel>();
      ObservableCollection<ItemColorViewModel> collection5 = new ObservableCollection<ItemColorViewModel>();
      this.McrColorItems.ItemsSource = (IEnumerable) collection1;
      this.MrdColorItems.ItemsSource = (IEnumerable) collection2;
      this.RccColorItems.ItemsSource = (IEnumerable) collection3;
      this.ClassicColorItems.ItemsSource = (IEnumerable) collection4;
      this.MphColorItems.ItemsSource = (IEnumerable) collection5;
      this.CustomItems.ItemsSource = (IEnumerable) this._customItems;
      for (int index = 0; index < ColorItemSelector.MoreColorList.Count; ++index)
      {
        ItemColorViewModel itemColorViewModel = new ItemColorViewModel()
        {
          Color = ColorItemSelector.MoreColorList[index]
        };
        switch (index / 8)
        {
          case 0:
            collection1.Add(itemColorViewModel);
            break;
          case 1:
            collection2.Add(itemColorViewModel);
            break;
          case 2:
            collection3.Add(itemColorViewModel);
            break;
          case 3:
            collection4.Add(itemColorViewModel);
            break;
          case 4:
            collection5.Add(itemColorViewModel);
            break;
        }
      }
      this._customItems.Add(new ItemColorViewModel()
      {
        IsAddCustom = true
      });
      List<string> colors = LocalSettings.Settings.UserPreference?.RecentlyColors?.colors;
      if (colors != null)
      {
        foreach (string str in colors)
          this._customItems.Add(new ItemColorViewModel()
          {
            Color = str
          });
      }
      this._colorItems.AddRange((IEnumerable<ItemColorViewModel>) collection1);
      this._colorItems.AddRange((IEnumerable<ItemColorViewModel>) collection2);
      this._colorItems.AddRange((IEnumerable<ItemColorViewModel>) collection3);
      this._colorItems.AddRange((IEnumerable<ItemColorViewModel>) collection4);
      this._colorItems.AddRange((IEnumerable<ItemColorViewModel>) collection5);
    }

    public void SetItemSelected(string selectedColor)
    {
      foreach (ItemColorViewModel colorItem in this._colorItems)
        colorItem.Selected = selectedColor == colorItem.Color;
      foreach (ItemColorViewModel customItem in (Collection<ItemColorViewModel>) this._customItems)
        customItem.Selected = selectedColor == customItem.Color;
      this._selectedColor = selectedColor;
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is ColorSelectItem colorSelectItem) || !(colorSelectItem.DataContext is ItemColorViewModel dataContext))
        return;
      e.Handled = true;
      if (!dataContext.IsAddCustom && dataContext.Selected)
      {
        this._colorItems.ForEach((Action<ItemColorViewModel>) (m => m.Selected = false));
        foreach (ItemColorViewModel customItem in (Collection<ItemColorViewModel>) this._customItems)
          customItem.Selected = false;
        EventHandler<string> colorSelect = this.ColorSelect;
        if (colorSelect == null)
          return;
        colorSelect((object) null, "transparent");
      }
      else if (dataContext.IsAddCustom)
      {
        EscPopup escPopup = new EscPopup();
        escPopup.PlacementTarget = (UIElement) colorSelectItem;
        escPopup.StaysOpen = false;
        escPopup.Placement = PlacementMode.Top;
        escPopup.VerticalOffset = 10.0;
        escPopup.HorizontalOffset = -4.0;
        ColorPicker colorPicker = new ColorPicker((Popup) escPopup, this._selectedColor);
        escPopup.Child = (UIElement) colorPicker;
        colorPicker.ColorSelected += (EventHandler<string>) ((o, brush) => this.OnCustomColorSelect(brush));
        escPopup.IsOpen = true;
      }
      else
      {
        this._colorItems.ForEach((Action<ItemColorViewModel>) (m => m.Selected = false));
        foreach (ItemColorViewModel customItem in (Collection<ItemColorViewModel>) this._customItems)
          customItem.Selected = customItem.Color == dataContext.Color;
        dataContext.Selected = true;
        this._selectedColor = dataContext.Color;
        EventHandler<string> colorSelect = this.ColorSelect;
        if (colorSelect == null)
          return;
        colorSelect((object) null, dataContext.Color);
      }
    }

    private void OnCustomColorSelect(string color)
    {
      foreach (ItemColorViewModel customItem in (Collection<ItemColorViewModel>) this._customItems)
        customItem.Selected = false;
      ItemColorViewModel exist = this._customItems.FirstOrDefault<ItemColorViewModel>((Func<ItemColorViewModel, bool>) (m => m.Color == color));
      if (exist != null)
      {
        this._customItems.Remove(exist);
        this._customItems.Insert(1, exist);
      }
      else
      {
        exist = new ItemColorViewModel() { Color = color };
        this._customItems.Insert(1, exist);
        if (this._customItems.Count > 8)
          this._customItems.RemoveAt(8);
      }
      exist.Selected = true;
      this._colorItems.ForEach((Action<ItemColorViewModel>) (m => m.Selected = m.Color == exist.Color));
      this._selectedColor = exist.Color;
      EventHandler<string> colorSelect = this.ColorSelect;
      if (colorSelect == null)
        return;
      colorSelect((object) null, exist.Color);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/colorselector/coloritemselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.McrColorItems = (ItemsControl) target;
          break;
        case 2:
          this.MrdColorItems = (ItemsControl) target;
          break;
        case 3:
          this.RccColorItems = (ItemsControl) target;
          break;
        case 4:
          this.ClassicColorItems = (ItemsControl) target;
          break;
        case 5:
          this.MphColorItems = (ItemsControl) target;
          break;
        case 6:
          this.CustomItems = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
