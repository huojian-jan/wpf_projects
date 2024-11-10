// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.StickyColorSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class StickyColorSelector : ContentControl, IComponentConnector, IStyleConnector
  {
    private EscPopup _popup;
    internal ItemsControl ColorItems;
    private bool _contentLoaded;

    public StickyColorSelector()
    {
      this.InitializeComponent();
      this.ColorItems.ItemsSource = (IEnumerable) StickyColorViewModel.GetColorModels();
      this.ColorItems.Width = 440.0;
      this.Style = (Style) null;
    }

    public StickyColorSelector(string key, bool inPopup = true)
    {
      this.InitializeComponent();
      if (!Enum.TryParse<StickyColorKey>(key, true, out StickyColorKey _))
        key = StickyColorKey.Default.ToString();
      List<StickyColorViewModel> colorModels = StickyColorViewModel.GetColorModels();
      colorModels.RemoveAt(colorModels.Count - 1);
      foreach (StickyColorViewModel stickyColorViewModel in colorModels)
      {
        if (stickyColorViewModel.Key == key)
          stickyColorViewModel.Selected = true;
      }
      this.ColorItems.ItemsSource = (IEnumerable) colorModels;
      this.ColorItems.Width = 192.0;
      if (!inPopup)
        return;
      EscPopup escPopup = new EscPopup();
      escPopup.Child = (UIElement) this;
      escPopup.StaysOpen = false;
      escPopup.PopupAnimation = PopupAnimation.Fade;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.HorizontalOffset = -170.0;
      this._popup = escPopup;
      this._popup.Closed += (EventHandler) ((o, e) =>
      {
        EventHandler closed = this.Closed;
        if (closed == null)
          return;
        closed(o, e);
      });
    }

    public bool IsOpen
    {
      get
      {
        EscPopup popup = this._popup;
        return popup != null && popup.IsOpen;
      }
      set
      {
        if (this._popup == null)
          return;
        this._popup.IsOpen = value;
      }
    }

    public event EventHandler<string> ColorSelect;

    public event EventHandler Closed;

    private void OnItemSelect(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is StickyColorViewModel dataContext) || dataContext.Selected)
        return;
      this.IsOpen = false;
      if (dataContext.NeedPro && !ProChecker.CheckPro(ProType.StickyColor))
        return;
      EventHandler<string> colorSelect = this.ColorSelect;
      if (colorSelect != null)
        colorSelect((object) this, dataContext.Key);
      if (!(this.ColorItems.ItemsSource is List<StickyColorViewModel> itemsSource))
        return;
      foreach (StickyColorViewModel stickyColorViewModel in itemsSource)
        stickyColorViewModel.Selected = stickyColorViewModel.Key == dataContext.Key;
    }

    public void SetPlacementTarget(UIElement element)
    {
      if (this._popup == null)
        return;
      this._popup.PlacementTarget = element;
    }

    public void SetSelectedColor(string color)
    {
      if (!Enum.TryParse<StickyColorKey>(color, true, out StickyColorKey _))
        color = StickyColorKey.Default.ToString();
      if (!(this.ColorItems.ItemsSource is List<StickyColorViewModel> itemsSource))
        return;
      foreach (StickyColorViewModel stickyColorViewModel in itemsSource)
        stickyColorViewModel.Selected = stickyColorViewModel.Key == color;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/stickycolorselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.ColorItems = (ItemsControl) target;
      else
        this._contentLoaded = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemSelect);
    }
  }
}
