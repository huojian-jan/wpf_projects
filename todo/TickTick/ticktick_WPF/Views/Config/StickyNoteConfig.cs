// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.StickyNoteConfig
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class StickyNoteConfig : UserControl, IComponentConnector, IStyleConnector
  {
    internal ScrollViewer Scroller;
    internal Run LearnMoreText;
    internal StickyColorSelector ColorSelector;
    internal Slider OpacitySlider;
    internal CustomSimpleComboBox FontSizeComboBox;
    internal CheckBox PinDefault;
    internal CheckBox HideInTaskBarCheckbox;
    internal CheckBox ResetAfterSortCheckBox;
    internal ItemsControl StickySpaceItems;
    private bool _contentLoaded;

    public StickyNoteConfig()
    {
      this.InitializeComponent();
      this.InitControl();
      this.Loaded += (RoutedEventHandler) ((o, e) => this.Scroller.ScrollToTop());
    }

    private void InitControl()
    {
      this.InitFontSize();
      this.InitStickySpacing();
      this.InitColor();
      this.InitOpacity();
      this.InitPin();
      this.InitSort();
      this.InitHideInTask();
    }

    private void InitOpacity()
    {
      if (LocalSettings.Settings.ExtraSettings.StickyOpacity < 0.0 || LocalSettings.Settings.ExtraSettings.StickyOpacity > 100.0)
        return;
      this.OpacitySlider.Value = LocalSettings.Settings.ExtraSettings.StickyOpacity / 10.0;
      this.OpacitySlider.ToolTip = (object) (((int) (this.OpacitySlider.Value * 10.0)).ToString() + "%");
      this.OpacitySlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.OnSlideValueChanged);
      this.OpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.OnSlideValueChanged);
    }

    private void InitStickySpacing()
    {
      List<StickySpacingSelectViewModel> parent = new List<StickySpacingSelectViewModel>();
      parent.Add(new StickySpacingSelectViewModel(Utils.GetString("none"), 1, 0, parent));
      parent.Add(new StickySpacingSelectViewModel(Utils.GetString("Normal"), 5, 20, parent));
      parent.Add(new StickySpacingSelectViewModel(Utils.GetString("Large"), 7, 30, parent));
      parent.Add(new StickySpacingSelectViewModel(Utils.GetString("ExtraLarge"), 9, 40, parent));
      this.StickySpaceItems.ItemsSource = (IEnumerable) parent;
    }

    private void InitHideInTask()
    {
      this.HideInTaskBarCheckbox.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.StickyHideInTaskBar);
      // ISSUE: method pointer
      this.HideInTaskBarCheckbox.Checked += new RoutedEventHandler((object) this, __methodptr(\u003CInitHideInTask\u003Eg__OnCheckedChanged\u007C4_0));
      // ISSUE: method pointer
      this.HideInTaskBarCheckbox.Unchecked += new RoutedEventHandler((object) this, __methodptr(\u003CInitHideInTask\u003Eg__OnCheckedChanged\u007C4_0));
    }

    private void InitSort()
    {
      this.ResetAfterSortCheckBox.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.ResetStickyWhenAlign);
      // ISSUE: method pointer
      this.ResetAfterSortCheckBox.Checked += new RoutedEventHandler((object) this, __methodptr(\u003CInitSort\u003Eg__OnCheckedChanged\u007C5_0));
      // ISSUE: method pointer
      this.ResetAfterSortCheckBox.Unchecked += new RoutedEventHandler((object) this, __methodptr(\u003CInitSort\u003Eg__OnCheckedChanged\u007C5_0));
    }

    private void InitPin()
    {
      this.PinDefault.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.StickyDefaultPin);
      // ISSUE: method pointer
      this.PinDefault.Checked += new RoutedEventHandler((object) this, __methodptr(\u003CInitPin\u003Eg__OnPinDefaultChanged\u007C6_0));
      // ISSUE: method pointer
      this.PinDefault.Unchecked += new RoutedEventHandler((object) this, __methodptr(\u003CInitPin\u003Eg__OnPinDefaultChanged\u007C6_0));
    }

    private void InitColor()
    {
      this.ColorSelector.SetSelectedColor(LocalSettings.Settings.ExtraSettings.StickyDefaultColor);
      this.ColorSelector.ColorSelect += (EventHandler<string>) ((sender, color) => LocalSettings.Settings.ExtraSettings.StickyDefaultColor = color);
    }

    private void InitFontSize()
    {
      int stickyFont = LocalSettings.Settings.ExtraSettings.StickyFont;
      this.FontSizeComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("Small"),
        Utils.GetString("Normal"),
        Utils.GetString("Large"),
        Utils.GetString("ExtraLarge")
      };
      this.FontSizeComboBox.SelectedIndex = LocalSettings.Settings.ExtraSettings.StickyFont;
    }

    private void OnFontSizeChanged(object sender, SimpleComboBoxViewModel e)
    {
      string data = "normal";
      switch (this.FontSizeComboBox.SelectedIndex)
      {
        case 0:
          data = "small";
          break;
        case 1:
          data = "normal";
          break;
        case 2:
          data = "large";
          break;
        case 3:
          data = "extra_large";
          break;
      }
      UserActCollectUtils.AddClickEvent("sticky_note", "font_size", data);
      LocalSettings.Settings.ExtraSettings.StickyFont = this.FontSizeComboBox.SelectedIndex;
      App.SetStickyFontSize();
    }

    private void OnSlideValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      LocalSettings.Settings.ExtraSettings.StickyOpacity = e.NewValue * 10.0;
      this.OpacitySlider.ToolTip = (object) (((int) (e.NewValue * 10.0)).ToString() + "%");
      ticktick_WPF.Notifier.GlobalEventManager.OnStickyOpacityChanged();
    }

    private void OnSpaceItemSelected(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is StickySpacingSelectViewModel dataContext))
        return;
      string data = "normal";
      switch (dataContext.Spacing)
      {
        case 0:
          data = "none";
          break;
        case 20:
          data = "normal";
          break;
        case 30:
          data = "large";
          break;
        case 40:
          data = "extra_large";
          break;
      }
      UserActCollectUtils.AddClickEvent("sticky_note", "default_grid_spacing", data);
      LocalSettings.Settings.ExtraSettings.StickySpacing = dataContext.Spacing;
      dataContext.SetSelected();
    }

    private void LearnMoreClick(object sender, MouseButtonEventArgs e)
    {
      Utils.TryProcessStartUrl("https://help.dida365.com/articles/6986508444417130496");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/stickynoteconfig.xaml", UriKind.Relative));
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
          this.Scroller = (ScrollViewer) target;
          break;
        case 2:
          this.LearnMoreText = (Run) target;
          this.LearnMoreText.MouseLeftButtonUp += new MouseButtonEventHandler(this.LearnMoreClick);
          break;
        case 3:
          this.ColorSelector = (StickyColorSelector) target;
          break;
        case 4:
          this.OpacitySlider = (Slider) target;
          break;
        case 5:
          this.FontSizeComboBox = (CustomSimpleComboBox) target;
          break;
        case 6:
          this.PinDefault = (CheckBox) target;
          break;
        case 7:
          this.HideInTaskBarCheckbox = (CheckBox) target;
          break;
        case 8:
          this.ResetAfterSortCheckBox = (CheckBox) target;
          break;
        case 9:
          this.StickySpaceItems = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 10)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSpaceItemSelected);
    }
  }
}
