// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.LockSettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using NHotkey.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.Lock;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class LockSettings : UserControl, IComponentConnector
  {
    private string _cacheShortcut;
    private AppLockModel _model;
    internal CheckBox StartOnWindowsCheckBox;
    internal Grid StartToTrayGrid;
    internal CheckBox StartOnWindowsMiniCheckBox;
    internal TextBlock SetPasswordButton;
    internal TextBlock ClearPasswordButton;
    internal StackPanel LockSettingsPanel;
    internal CheckBox MinLockCheckBox;
    internal CheckBox LockAfterCheckBox;
    internal CountSpinner CountSpinner;
    internal CheckBox LockWidgetCheckBox;
    internal HotkeyControl LockHotkey;
    private bool _contentLoaded;

    public LockSettings()
    {
      this.InitializeComponent();
      this.InitData();
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => this.SaveLock();

    private async void InitData()
    {
      LockSettings lockSettings1 = this;
      AppLockModel lockConfig = await AppLockDao.GetLockConfig();
      LockSettings lockSettings2 = lockSettings1;
      AppLockModel appLockModel = lockConfig;
      if (appLockModel == null)
        appLockModel = new AppLockModel()
        {
          LockInterval = 5,
          MinLock = true
        };
      lockSettings2._model = appLockModel;
      lockSettings1.DataContext = (object) new LockViewModel(lockSettings1._model);
      lockSettings1._cacheShortcut = LocalSettings.Settings.LockShortcut;
      await lockSettings1.InitPasswordButton();
      lockSettings1.LockHotkey.SetHotkey(lockSettings1._cacheShortcut);
      lockSettings1.LockHotkey.HotkeyChanged += new EventHandler(lockSettings1.OnHotKeyChanged);
      lockSettings1.LockHotkey.HotkeyClear += new EventHandler(lockSettings1.OnHotKeyClear);
      lockSettings1.LockHotkey.SetNormalMode();
      lockSettings1.StartOnWindowsCheckBox.IsChecked = new bool?(ShortcutUtils.IsShortcutExit());
      if (!lockSettings1.StartOnWindowsCheckBox.IsChecked.Value)
        lockSettings1.StartToTrayGrid.Visibility = Visibility.Collapsed;
      lockSettings1.StartOnWindowsMiniCheckBox.IsChecked = new bool?(LocalSettings.Settings.Common.ShowWhenStart == 0 && !string.IsNullOrWhiteSpace(ShortcutUtils.GetShutcutArgs()) || LocalSettings.Settings.Common.ShowWhenStart == 2);
    }

    private void StartOnWindowsCheckBoxPreviewMouseLeftButtonUp(
      object sender,
      MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      CheckBox checkBox1 = (CheckBox) sender;
      CheckBox checkBox2 = (CheckBox) sender;
      bool? isChecked;
      bool? nullable;
      if (checkBox2 == null)
      {
        nullable = new bool?();
      }
      else
      {
        isChecked = checkBox2.IsChecked;
        nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      }
      checkBox1.IsChecked = nullable;
      e.Handled = true;
      CheckBox checkBox3 = (CheckBox) sender;
      int num;
      if (checkBox3 == null)
      {
        num = 0;
      }
      else
      {
        isChecked = checkBox3.IsChecked;
        bool flag = true;
        num = isChecked.GetValueOrDefault() == flag & isChecked.HasValue ? 1 : 0;
      }
      if (num != 0)
      {
        if (!ShortcutUtils.CreatShortCut(arg: "-hide"))
          return;
        this.StartToTrayGrid.Visibility = Visibility.Visible;
      }
      else
      {
        this.StartToTrayGrid.Visibility = Visibility.Collapsed;
        ShortcutUtils.DeleteShutCut(name: "滴答清单");
        ShortcutUtils.DeleteShutCut(name: "TickTick");
      }
    }

    private void StartOnWindowsMiniCheckBoxPreviewMouseLeftButtonUp(
      object sender,
      MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      CheckBox checkBox1 = (CheckBox) sender;
      CheckBox checkBox2 = (CheckBox) sender;
      bool? isChecked;
      bool? nullable;
      if (checkBox2 == null)
      {
        nullable = new bool?();
      }
      else
      {
        isChecked = checkBox2.IsChecked;
        nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      }
      checkBox1.IsChecked = nullable;
      e.Handled = true;
      CommonSettings common = LocalSettings.Settings.Common;
      CheckBox checkBox3 = (CheckBox) sender;
      int num1;
      if (checkBox3 == null)
      {
        num1 = 0;
      }
      else
      {
        isChecked = checkBox3.IsChecked;
        bool flag = true;
        num1 = isChecked.GetValueOrDefault() == flag & isChecked.HasValue ? 1 : 0;
      }
      int num2 = num1 != 0 ? 2 : 1;
      common.ShowWhenStart = num2;
    }

    private void OnHotKeyClear(object sender, EventArgs e)
    {
      this._cacheShortcut = string.Empty;
      LocalSettings.Settings.LockShortcut = string.Empty;
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
      HotkeyManager.Current.Remove("LockShortcut");
    }

    private void OnHotKeyChanged(object sender, EventArgs e)
    {
      this._cacheShortcut = this.LockHotkey.CurrentHotkey.ToString().Replace(" ", "");
      LocalSettings.Settings.LockShortcut = this._cacheShortcut;
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
      HotkeyManager.Current.Remove("LockShortcut");
      HotKeyUtils.ResignHotKey("LockShortcut");
    }

    private async Task InitPasswordButton()
    {
      string lockPassword = await AppLockCache.GetLockPassword();
      this.SetPasswordButton.Text = Utils.GetString(!string.IsNullOrEmpty(lockPassword) ? "ChangePassword" : "SetPassword");
      this.ClearPasswordButton.Visibility = !string.IsNullOrEmpty(lockPassword) ? Visibility.Visible : Visibility.Collapsed;
      this.SetLockPanelEnabled(!string.IsNullOrEmpty(lockPassword));
    }

    private void SetLockPanelEnabled(bool isEnabled)
    {
      this.LockSettingsPanel.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
    }

    public async Task SaveLock()
    {
      AppLockModel model = new AppLockModel()
      {
        MinLock = this.MinLockCheckBox.IsChecked.GetValueOrDefault(),
        LockAfter = this.LockAfterCheckBox.IsChecked.GetValueOrDefault(),
        LockWidget = this.LockWidgetCheckBox.IsChecked.GetValueOrDefault(),
        LockInterval = this.CountSpinner.Count
      };
      if (this._model != null && this._model.Equal(model))
        return;
      this._model = model;
      await AppLockDao.SaveLockConfig(model);
    }

    private async void SetPasswordClick(object sender, MouseButtonEventArgs e)
    {
      LockSettings lockSettings = this;
      SetPasswordWindow setPasswordWindow = new SetPasswordWindow(string.IsNullOrEmpty(await AppLockCache.GetLockPassword()) ? SetPasswordMode.Init : SetPasswordMode.Modify);
      setPasswordWindow.Owner = Window.GetWindow((DependencyObject) lockSettings);
      setPasswordWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      setPasswordWindow.ShowDialog();
      await lockSettings.InitPasswordButton();
    }

    private async void ClearPasswordClick(object sender, MouseButtonEventArgs e)
    {
      LockSettings lockSettings = this;
      SetPasswordWindow setPasswordWindow = new SetPasswordWindow(SetPasswordMode.Clear);
      setPasswordWindow.Owner = Window.GetWindow((DependencyObject) lockSettings);
      setPasswordWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      setPasswordWindow.ShowDialog();
      await lockSettings.InitPasswordButton();
    }

    private void OnLockMinClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox minLockCheckBox = this.MinLockCheckBox;
      bool? isChecked = this.MinLockCheckBox.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      minLockCheckBox.IsChecked = nullable;
    }

    private void OnAutoLockClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox lockAfterCheckBox = this.LockAfterCheckBox;
      bool? isChecked = this.LockAfterCheckBox.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      lockAfterCheckBox.IsChecked = nullable;
    }

    private void OnLockWidgetClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox lockWidgetCheckBox = this.LockWidgetCheckBox;
      bool? isChecked = this.LockWidgetCheckBox.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      lockWidgetCheckBox.IsChecked = nullable;
    }

    private void OnShortCutClick(object sender, MouseButtonEventArgs e)
    {
      this.LockHotkey.TryFocus();
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/locksettings.xaml", UriKind.Relative));
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
          this.StartOnWindowsCheckBox = (CheckBox) target;
          this.StartOnWindowsCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.StartOnWindowsCheckBoxPreviewMouseLeftButtonUp);
          break;
        case 2:
          this.StartToTrayGrid = (Grid) target;
          break;
        case 3:
          this.StartOnWindowsMiniCheckBox = (CheckBox) target;
          this.StartOnWindowsMiniCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.StartOnWindowsMiniCheckBoxPreviewMouseLeftButtonUp);
          break;
        case 4:
          this.SetPasswordButton = (TextBlock) target;
          this.SetPasswordButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetPasswordClick);
          break;
        case 5:
          this.ClearPasswordButton = (TextBlock) target;
          this.ClearPasswordButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearPasswordClick);
          break;
        case 6:
          this.LockSettingsPanel = (StackPanel) target;
          break;
        case 7:
          this.MinLockCheckBox = (CheckBox) target;
          break;
        case 8:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLockMinClick);
          break;
        case 9:
          this.LockAfterCheckBox = (CheckBox) target;
          break;
        case 10:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAutoLockClick);
          break;
        case 11:
          this.CountSpinner = (CountSpinner) target;
          break;
        case 12:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAutoLockClick);
          break;
        case 13:
          this.LockWidgetCheckBox = (CheckBox) target;
          break;
        case 14:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLockWidgetClick);
          break;
        case 15:
          this.LockHotkey = (HotkeyControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
