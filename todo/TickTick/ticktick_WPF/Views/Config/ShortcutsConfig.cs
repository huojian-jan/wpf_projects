// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.ShortcutsConfig
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class ShortcutsConfig : UserControl, IComponentConnector
  {
    private string _addTaskHotKeyString;
    private string _lockHotKeyString;
    private string _openOrCloseHotKeyString;
    private string _pinHotKeyString;
    internal StackPanel TitlePanel;
    internal Border HelpBorder;
    internal ListView ShortcutList;
    private bool _contentLoaded;

    public ShortcutsConfig(bool inSettings = true)
    {
      this.InitializeComponent();
      this.InitShortcuts();
      if (inSettings)
        this.TitlePanel.Visibility = Visibility.Collapsed;
      else
        this.TitlePanel.Visibility = Visibility.Visible;
      this.Loaded += (RoutedEventHandler) ((sender, args) => this.ShortcutList.ScrollIntoView((object) (this.ShortcutList.ItemsSource as IEnumerable<ShortcutViewModel>).FirstOrDefault<ShortcutViewModel>()));
    }

    public void InitShortcuts()
    {
      this.ShortcutList.ItemsSource = (IEnumerable) ShortcutViewModel.BuildShortcuts();
    }

    private void OnClearShortcut(object sender, EventArgs e)
    {
      if (!(sender is HotkeyControl hotkeyControl) || !(hotkeyControl.DataContext is ShortcutViewModel dataContext))
        return;
      dataContext.Shortcut = hotkeyControl.CurrentHotkey.ToString().Replace(" ", "");
      dataContext.ClearShortCut();
      SettingsHelper.ShortCutChanged = true;
    }

    private void OnShortcutChanged(object sender, EventArgs e)
    {
      if (!(sender is HotkeyControl hotkeyControl) || !(hotkeyControl.DataContext is ShortcutViewModel dataContext))
        return;
      dataContext.SetShortCut(hotkeyControl.CurrentHotkey.ToRealString().Replace(" ", ""));
    }

    private void OnIndicatorClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Grid grid) || !(grid.Tag is string tag) || !(LocalSettings.Settings[tag] is bool setting))
        return;
      LocalSettings.Settings[tag] = (object) !setting;
    }

    private void ResetAllShortcut(object sender, RoutedEventArgs e)
    {
      LocalSettings.Settings.ShortcutAddTask = "Alt+Shift+A";
      LocalSettings.Settings.ShortcutOpenOrClose = "Ctrl+Shift+E";
      LocalSettings.Settings.ShortcutPin = "";
      LocalSettings.Settings.LockShortcut = "Ctrl+Shift+L";
      LocalSettings.Settings.PomoShortcut = "Ctrl+Alt+P";
      LocalSettings.Settings.CreateStickyShortCut = "";
      LocalSettings.Settings.ShowHideStickyShortCut = "";
      LocalSettings.Settings.ShortCutModel.Reset();
      this.InitShortcuts();
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.Toast(Utils.GetString("SettingsRestored"));
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (Utils.GetMousePointElement<HotkeyControl>((MouseEventArgs) e, (FrameworkElement) this.ShortcutList) != null)
        return;
      Window window = Window.GetWindow((DependencyObject) this);
      if (window == null)
        return;
      FocusManager.SetFocusedElement((DependencyObject) window, (IInputElement) window);
      Keyboard.Focus((IInputElement) window);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/shortcutsconfig.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.TitlePanel = (StackPanel) target;
          break;
        case 2:
          this.HelpBorder = (Border) target;
          break;
        case 3:
          this.ShortcutList = (ListView) target;
          this.ShortcutList.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ResetAllShortcut);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
