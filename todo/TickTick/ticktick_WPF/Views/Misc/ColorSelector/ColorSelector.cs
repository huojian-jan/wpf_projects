// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.ColorSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector
{
  public class ColorSelector : UserControl, IComponentConnector
  {
    private List<ItemColorViewModel> _defaultColors = new List<ItemColorViewModel>();
    private static readonly List<string> ColorList = new List<string>()
    {
      "#FF6161",
      "#FFAC38",
      "#FFD324",
      "#E6EA49",
      "#35D870",
      "#4CA1FF",
      "#6E75F4"
    };
    internal ItemsControl DefaultColorItems;
    internal Grid CustomColorItem;
    internal Image CustomColorImage;
    internal Image CustomColorSelectedImage;
    internal Rectangle CustomSelectedColor;
    internal EscPopup ColorPopup;
    internal ContentControl MoreColorContainer;
    internal ColorItemSelector Selector;
    private bool _contentLoaded;

    public event EventHandler<string> ColorSelect;

    public ColorSelector()
    {
      this.InitializeComponent();
      this.Selector.ColorSelect += new EventHandler<string>(this.OnColorSelect);
      this._defaultColors.Add(new ItemColorViewModel()
      {
        NoColor = true
      });
      foreach (string color in ticktick_WPF.Views.Misc.ColorSelector.ColorSelector.ColorList)
        this._defaultColors.Add(new ItemColorViewModel()
        {
          Color = color
        });
      this.DefaultColorItems.ItemsSource = (IEnumerable) this._defaultColors;
    }

    private void OnColorSelect(object sender, string color)
    {
      if (string.IsNullOrEmpty(color))
        color = "transparent";
      this.SetSelectedColor(color);
      EventHandler<string> colorSelect = this.ColorSelect;
      if (colorSelect == null)
        return;
      colorSelect((object) null, color);
    }

    private void ShowColorSelector(object sender, MouseButtonEventArgs e)
    {
      this.Selector.SetItemSelected(this.CustomSelectedColor.Fill == Brushes.Transparent ? (string) null : ThemeUtil.GetNoAlphaColorString(this.CustomSelectedColor.Fill.ToString()));
      this.ColorPopup.IsOpen = true;
    }

    public string GetSelectedColor()
    {
      ItemColorViewModel itemColorViewModel = this._defaultColors.FirstOrDefault<ItemColorViewModel>((Func<ItemColorViewModel, bool>) (m => m.Selected));
      if (itemColorViewModel != null)
        return itemColorViewModel.Color;
      if (this.CustomSelectedColor.Fill == Brushes.Transparent)
        return "transparent";
      string selectedColor = this.CustomSelectedColor.Fill.ToString();
      if (selectedColor.StartsWith("#") && selectedColor.Length == 9)
        selectedColor = "#" + selectedColor.Substring(3);
      return selectedColor;
    }

    private string CheckColor(string colorString)
    {
      if (string.IsNullOrEmpty(colorString) || colorString.Length == 9 && colorString.StartsWith("#00") || colorString == "transparent")
        colorString = (string) null;
      if (!ColorUtils.IsAvailableColor(colorString))
        colorString = (string) null;
      return colorString;
    }

    public void SetSelectedColor(string colorString)
    {
      colorString = this.CheckColor(colorString);
      this._defaultColors.ForEach((Action<ItemColorViewModel>) (m => m.Selected = m.Color == colorString || m.NoColor && ThemeUtil.IsEmptyColor(colorString)));
      if (this._defaultColors.All<ItemColorViewModel>((Func<ItemColorViewModel, bool>) (m => !m.Selected)))
      {
        this.CustomSelectedColor.Fill = (Brush) ThemeUtil.GetColorInString(colorString);
        this.CustomColorImage.Visibility = Visibility.Collapsed;
        this.CustomColorSelectedImage.Visibility = Visibility.Visible;
      }
      else
      {
        this.CustomSelectedColor.Fill = (Brush) Brushes.Transparent;
        this.CustomColorImage.Visibility = Visibility.Visible;
        this.CustomColorSelectedImage.Visibility = Visibility.Collapsed;
      }
    }

    private void OnDefaultColorClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is ItemColorViewModel dataContext))
        return;
      if (!dataContext.Selected)
      {
        string str = dataContext.NoColor ? "transparent" : dataContext.Color;
        this.SetSelectedColor(str);
        EventHandler<string> colorSelect = this.ColorSelect;
        if (colorSelect == null)
          return;
        colorSelect((object) null, str);
      }
      else
      {
        string str = "transparent";
        this.SetSelectedColor(str);
        EventHandler<string> colorSelect = this.ColorSelect;
        if (colorSelect == null)
          return;
        colorSelect((object) null, str);
      }
    }

    private void OnColorPopupClosed(object sender, EventArgs e)
    {
      SettingsHelper.PushLocalPreference();
    }

    public static void TryAddClickEvent(string color)
    {
      string data = "";
      if (ThemeUtil.IsEmptyColor(color))
        data = "none";
      if (ticktick_WPF.Views.Misc.ColorSelector.ColorSelector.ColorList.Contains(color))
      {
        data = "preset";
      }
      else
      {
        int num = ColorItemSelector.MoreColorList.IndexOf(color);
        if (num >= 0)
        {
          switch (num / 8)
          {
            case 0:
              data = "macaron";
              break;
            case 1:
              data = "morandi";
              break;
            case 2:
              data = "rococo";
              break;
            case 3:
              data = "classic";
              break;
            case 4:
              data = "memphis";
              break;
          }
        }
      }
      if (string.IsNullOrEmpty(data))
        return;
      UserActCollectUtils.AddClickEvent("project_list_ui", "list_color", data);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/colorselector/colorselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.DefaultColorItems = (ItemsControl) target;
          break;
        case 2:
          this.CustomColorItem = (Grid) target;
          this.CustomColorItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowColorSelector);
          break;
        case 3:
          this.CustomColorImage = (Image) target;
          break;
        case 4:
          this.CustomColorSelectedImage = (Image) target;
          break;
        case 5:
          this.CustomSelectedColor = (Rectangle) target;
          break;
        case 6:
          this.ColorPopup = (EscPopup) target;
          break;
        case 7:
          this.MoreColorContainer = (ContentControl) target;
          break;
        case 8:
          this.Selector = (ColorItemSelector) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
