// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoSettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using NHotkey.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.CustomControl;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoSettings : Window, IComponentConnector
  {
    private const string KeyDailyPomoGoals = "DailyPomoGoals";
    public const string KeyPomoDuration = "PomoDuration";
    public const string KeyShortBreakDuration = "ShortBreakDuration";
    public const string KeyLongBreakDuration = "LongBreakDuration";
    private const string KeyLongBreakEvery = "LongBreakEvery";
    private const string KeyAutoPomoTimes = "AutoPomoTimes";
    public static readonly Dictionary<string, Tuple<int, int, int>> PomoSettingsData = new Dictionary<string, Tuple<int, int, int>>()
    {
      {
        "DailyPomoGoals",
        new Tuple<int, int, int>(4, 1, 20)
      },
      {
        "PomoDuration",
        new Tuple<int, int, int>(25, 5, 180)
      },
      {
        "ShortBreakDuration",
        new Tuple<int, int, int>(5, 1, 60)
      },
      {
        "LongBreakDuration",
        new Tuple<int, int, int>(15, 1, 60)
      },
      {
        "LongBreakEvery",
        new Tuple<int, int, int>(4, 1, 60)
      },
      {
        nameof (AutoPomoTimes),
        new Tuple<int, int, int>(4, 2, 10)
      }
    };
    private bool _changed;
    private bool _hotKeyChanged;
    private static PomoSettings _instance;
    internal ScrollViewer Scroller;
    internal StackPanel SettingsPanel;
    internal Path ProIcon;
    internal CheckBox AutoSyncCheckBox;
    internal TextBox PomoDurationText;
    internal TextBox ShortBreakDurationText;
    internal TextBox LongBreakDurationText;
    internal TextBox LongBreakEveryText;
    internal TextBlock PomoCount;
    internal CheckBox AutoNextPomo;
    internal CheckBox AutoBreak;
    internal Grid AutoTimesGrid;
    internal TextBox AutoPomoTimes;
    internal CustomSimpleComboBox FocusEndSoundComboBox;
    internal CustomSimpleComboBox BreakEndSoundComboBox;
    internal Image NormalImage;
    internal Grid DetailedSelect;
    internal Image CircleImage;
    internal Grid CircleSelect;
    internal Image MiniImage;
    internal Grid RectSelect;
    internal CustomSimpleComboBox MiniModeThemeComboBox;
    internal Slider OpacitySlider;
    internal CheckBox AutoShowWidget;
    internal HotkeyControl PomoHotkey;
    private bool _contentLoaded;

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    public static void ShowInstance(Window owner, bool scrollToMini = false, bool centerScreen = false)
    {
      if (PomoSettings._instance != null && PomoSettings._instance.Owner != owner)
      {
        PomoSettings._instance.Close();
        PomoSettings._instance = (PomoSettings) null;
      }
      if (PomoSettings._instance == null)
      {
        PomoSettings pomoSettings = new PomoSettings();
        pomoSettings.Owner = owner;
        PomoSettings._instance = pomoSettings;
        if (centerScreen)
          PomoSettings._instance.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        PomoSettings._instance.Closing += (CancelEventHandler) ((o, e) => PomoSettings._instance = (PomoSettings) null);
      }
      PomoSettings._instance.Show();
      if (!scrollToMini)
        return;
      PomoSettings._instance.Scroller.ScrollToBottom();
    }

    public PomoSettings()
    {
      this.InitializeComponent();
      this.InitSettings();
      this.LoadRemoteAndInit();
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
      this.PomoCount.Text = Utils.GetString(nameof (PomoCount))?.ToLower();
      this.Closed += new EventHandler(this.OnClosed);
      this.Loaded += (RoutedEventHandler) ((o, e) =>
      {
        bool valueOrDefault = (this.FindResource((object) "IsDarkTheme") as bool?).GetValueOrDefault();
        bool flag = !Utils.IsZhCn();
        string str = "pack://application:,,,/Assets/ImageSource/" + (valueOrDefault ? "dark" : "light") + "/";
        this.NormalImage.Source = (ImageSource) ImageUtils.GetResourceImage(str + "NormalFocus" + (flag ? "_en" : "_cn") + ".png", 0);
        this.MiniImage.Source = (ImageSource) ImageUtils.GetResourceImage(str + "MiniFocus.png", 0);
        this.CircleImage.Source = (ImageSource) ImageUtils.GetResourceImage(str + "CircleFocus.png", 0);
      });
    }

    private void OnClosed(object sender, EventArgs e)
    {
      if (this._changed)
      {
        PomoSettings.SyncRemoteSettings();
        TickFocusManager.ResetPomoSetting();
      }
      if (this._hotKeyChanged)
        SettingsHelper.PushLocalSettings();
      RemindSoundPlayer.Stop();
      this.Owner?.Activate();
    }

    private async void LoadRemoteAndInit()
    {
      if (!await PomoSettings.LoadRemoteSettings())
        return;
      this.InitSettings();
    }

    public static async Task<bool> LoadRemoteSettings()
    {
      PomoConfigModel remotePomoConfig = await Communicator.GetRemotePomoConfig();
      if (remotePomoConfig == null || !string.IsNullOrEmpty(remotePomoConfig.errorId))
        return false;
      LocalSettings settings = LocalSettings.Settings;
      settings.PomoDuration = remotePomoConfig.pomoDuration == 0 ? 25 : (remotePomoConfig.pomoDuration < 5 ? 5 : remotePomoConfig.pomoDuration);
      settings.ShortBreakDuration = remotePomoConfig.shortBreakDuration == 0 ? 5 : remotePomoConfig.shortBreakDuration;
      settings.LongBreakDuration = remotePomoConfig.longBreakDuration == 0 ? 15 : remotePomoConfig.longBreakDuration;
      settings.ShowDailyDuration = remotePomoConfig.focusDuration > 0;
      settings.DailyFocusDuration = settings.ShowDailyDuration ? remotePomoConfig.focusDuration : settings.DailyFocusDuration;
      settings.ShowDailyPomo = remotePomoConfig.pomoGoal > 0;
      settings.DailyPomoGoals = settings.ShowDailyPomo ? remotePomoConfig.pomoGoal : settings.DailyPomoGoals;
      settings.LongBreakEvery = remotePomoConfig.longBreakInterval == 0 ? 4 : remotePomoConfig.longBreakInterval;
      settings.AutoBreak = remotePomoConfig.autoBreak;
      settings.AutoNextPomo = remotePomoConfig.autoPomo;
      settings.LightsOn = remotePomoConfig.lightsOn;
      return true;
    }

    public static async void SyncRemoteSettings()
    {
      PomoConfigModel config = new PomoConfigModel();
      LocalSettings settings = LocalSettings.Settings;
      config.id = settings.LoginUserId;
      config.pomoDuration = settings.PomoDuration < 5 ? 5 : settings.PomoDuration;
      config.shortBreakDuration = settings.ShortBreakDuration;
      config.longBreakDuration = settings.LongBreakDuration;
      config.pomoGoal = settings.ShowDailyPomo ? settings.DailyPomoGoals : 0;
      config.focusDuration = settings.ShowDailyDuration ? settings.DailyFocusDuration : 0;
      config.longBreakInterval = settings.LongBreakEvery;
      config.autoBreak = settings.AutoBreak;
      config.autoPomo = settings.AutoNextPomo;
      config.lightsOn = settings.LightsOn;
      await Communicator.UpdateRemotePomoConfig(config);
    }

    private void InitSettings()
    {
      PomoSettings.SetPomoSetting("PomoDuration", this.PomoDurationText);
      PomoSettings.SetPomoSetting("ShortBreakDuration", this.ShortBreakDurationText);
      PomoSettings.SetPomoSetting("LongBreakDuration", this.LongBreakDurationText);
      PomoSettings.SetPomoSetting("LongBreakEvery", this.LongBreakEveryText);
      PomoSettings.SetPomoSetting("AutoPomoTimes", this.AutoPomoTimes);
      this.AutoNextPomo.IsChecked = new bool?(LocalSettings.Settings.AutoNextPomo);
      this.AutoBreak.IsChecked = new bool?(LocalSettings.Settings.AutoBreak);
      this.AutoShowWidget.IsChecked = new bool?(LocalSettings.Settings.PomoLocalSetting.AutoShowWidget);
      this.PomoHotkey.SetHotkey(LocalSettings.Settings.PomoShortcut);
      this.PomoHotkey.HotkeyChanged += new EventHandler(this.OnHotKeyChanged);
      this.PomoHotkey.HotkeyClear += new EventHandler(this.OnHotKeyClear);
      this.PomoHotkey.SetNormalMode();
      if (LocalSettings.Settings.PomoWindowOpacity >= 0.0 && LocalSettings.Settings.PomoWindowOpacity <= 1.0)
      {
        this.OpacitySlider.Value = LocalSettings.Settings.PomoWindowOpacity * 10.0;
        this.OpacitySlider.ToolTip = (object) (((int) (this.OpacitySlider.Value * 10.0)).ToString() + "%");
        this.OpacitySlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.OnSlideValueChanged);
        this.OpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.OnSlideValueChanged);
      }
      this.InitFocusEndSound();
      this.InitBreakEndSound();
      this.InitMiniModeThemeComboBox();
      this.AutoTimesGrid.Visibility = !LocalSettings.Settings.AutoNextPomo || !LocalSettings.Settings.AutoBreak ? Visibility.Collapsed : Visibility.Visible;
      this.AutoSyncCheckBox.IsChecked = new bool?(LocalSettings.Settings.FocusKeepInSync);
      this.SetSelectedDisplayType();
    }

    private void SetSelectedDisplayType()
    {
      bool flag1 = LocalSettings.Settings.PomoLocalSetting.DisplayType == "Circle";
      bool flag2 = LocalSettings.Settings.PomoLocalSetting.DisplayType == "Mini";
      this.DetailedSelect.Visibility = flag1 | flag2 ? Visibility.Collapsed : Visibility.Visible;
      this.CircleSelect.Visibility = flag1 ? Visibility.Visible : Visibility.Collapsed;
      this.RectSelect.Visibility = flag2 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnHotKeyClear(object sender, EventArgs e)
    {
      LocalSettings.Settings.PomoShortcut = string.Empty;
      HotkeyManager.Current.Remove("PomoShortcut");
      this._hotKeyChanged = true;
    }

    private void OnHotKeyChanged(object sender, EventArgs e)
    {
      LocalSettings.Settings.PomoShortcut = this.PomoHotkey.CurrentHotkey.ToString().Replace(" ", "");
      HotkeyManager.Current.Remove("PomoShortcut");
      HotKeyUtils.ResignHotKey("PomoShortcut");
      this._hotKeyChanged = true;
    }

    private static void SetPomoSetting(string key, TextBox settingText)
    {
      settingText.Text = LocalSettings.Settings[key].ToString();
    }

    private async void OnSettingsTextChanged(object sender, TextChangedEventArgs e)
    {
      TextBox textBox = (TextBox) sender;
      string key;
      Tuple<int, int, int> tuple;
      if (textBox.Tag == null)
      {
        textBox = (TextBox) null;
        key = (string) null;
        tuple = (Tuple<int, int, int>) null;
      }
      else
      {
        this._changed = true;
        key = textBox.Tag.ToString();
        if (!PomoSettings.PomoSettingsData.ContainsKey(key))
        {
          textBox = (TextBox) null;
          key = (string) null;
          tuple = (Tuple<int, int, int>) null;
        }
        else
        {
          tuple = PomoSettings.PomoSettingsData[key];
          await Task.Delay(600);
          int result;
          int.TryParse(textBox.Text, out result);
          if (result > tuple.Item3)
          {
            textBox.Text = tuple.Item3.ToString();
            textBox.SelectAll();
          }
          if (result < tuple.Item2)
          {
            textBox.Text = tuple.Item2.ToString();
            textBox.SelectAll();
          }
          PomoSettings.SavePomoSettings(key, textBox.Text);
          textBox = (TextBox) null;
          key = (string) null;
          tuple = (Tuple<int, int, int>) null;
        }
      }
    }

    private static void SavePomoSettings(string key, string value)
    {
      int result;
      if (!int.TryParse(value, out result))
        return;
      LocalSettings.Settings[key] = (object) result;
    }

    private void OnSettingsKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Up && e.Key != Key.Down)
        return;
      TextBox textBox = (TextBox) sender;
      if (textBox.Tag == null)
        return;
      string key = textBox.Tag.ToString();
      if (!PomoSettings.PomoSettingsData.ContainsKey(key))
        return;
      Tuple<int, int, int> tuple = PomoSettings.PomoSettingsData[key];
      textBox.TextChanged -= new TextChangedEventHandler(this.OnSettingsTextChanged);
      int result;
      if (int.TryParse(textBox.Text, out result))
      {
        int num1 = tuple.Item2;
        int num2 = tuple.Item3;
        int num3 = e.Key == Key.Up ? result + 1 : result - 1;
        if (num3 >= num1 && num3 <= num2)
        {
          textBox.Text = num3.ToString();
          textBox.SelectAll();
          PomoSettings.SavePomoSettings(key, textBox.Text);
        }
      }
      textBox.TextChanged += new TextChangedEventHandler(this.OnSettingsTextChanged);
    }

    private void AutoSyncClick(object sender, RoutedEventArgs e)
    {
      if (!UserDao.IsPro())
      {
        ProChecker.CheckPro(ProType.FocusAutoSync, Window.GetWindow((DependencyObject) this));
        Mouse.Capture((IInputElement) null);
        e.Handled = true;
      }
      else
      {
        PomoSettings.NotifyCheckBoxChanged(sender, e);
        bool keepInSync = this.AutoSyncCheckBox.IsChecked.HasValue && this.AutoSyncCheckBox.IsChecked.Value;
        LocalSettings.Settings.SaveFocusKeepInSync(keepInSync);
        if (!keepInSync)
          return;
        UserActCollectUtils.AddClickEvent("focus", "focus_settings", "focus_sync");
        WebSocketService.CheckFocusSocket();
      }
    }

    private void AutoPomoClick(object sender, MouseButtonEventArgs e)
    {
      this._changed = true;
      PomoSettings.NotifyCheckBoxChanged(sender, (RoutedEventArgs) e);
      LocalSettings.Settings.AutoNextPomo = this.AutoNextPomo.IsChecked.HasValue && this.AutoNextPomo.IsChecked.Value;
      this.AutoTimesGrid.Visibility = !LocalSettings.Settings.AutoNextPomo || !LocalSettings.Settings.AutoBreak ? Visibility.Collapsed : Visibility.Visible;
    }

    private void AutoBreakClick(object sender, MouseButtonEventArgs e)
    {
      this._changed = true;
      PomoSettings.NotifyCheckBoxChanged(sender, (RoutedEventArgs) e);
      LocalSettings.Settings.AutoBreak = this.AutoBreak.IsChecked.HasValue && this.AutoBreak.IsChecked.Value;
      this.AutoTimesGrid.Visibility = !LocalSettings.Settings.AutoNextPomo || !LocalSettings.Settings.AutoBreak ? Visibility.Collapsed : Visibility.Visible;
    }

    private static void NotifyCheckBoxChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      CheckBox checkBox1 = (CheckBox) sender;
      CheckBox checkBox2 = (CheckBox) sender;
      bool? nullable;
      if (checkBox2 == null)
      {
        nullable = new bool?();
      }
      else
      {
        bool? isChecked = checkBox2.IsChecked;
        nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      }
      checkBox1.IsChecked = nullable;
      e.Handled = true;
    }

    private void OnSlideValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      LocalSettings.Settings.PomoWindowOpacity = e.NewValue / 10.0;
      this.OpacitySlider.ToolTip = (object) (((int) (e.NewValue * 10.0)).ToString() + "%");
      ticktick_WPF.Notifier.GlobalEventManager.NotifyPomoOpacityChanged(LocalSettings.Settings.PomoWindowOpacity);
    }

    private void OnShortCutClick(object sender, MouseButtonEventArgs e)
    {
      this.PomoHotkey.TryFocus();
      e.Handled = true;
    }

    private void OnThemeChecked(object sender, RoutedEventArgs e)
    {
      if (!(sender is RadioButton radioButton) || !(radioButton.Tag is string tag))
        return;
      int num = tag != LocalSettings.Settings.PomoWindowTheme ? 1 : 0;
    }

    private void InitFocusEndSound()
    {
      this.FocusEndSoundComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("none"),
        Utils.GetString("Default"),
        Utils.GetString("HarpSound"),
        Utils.GetString("DrumSound"),
        Utils.GetString("BeepSound"),
        Utils.GetString("BlocksSound"),
        Utils.GetString("ChimesSound"),
        Utils.GetString("CrystalSound"),
        Utils.GetString("LadderSound"),
        Utils.GetString("LatticeSound"),
        Utils.GetString("LeapSound"),
        Utils.GetString("MatrixSound"),
        Utils.GetString("PulseSound"),
        Utils.GetString("Spiral")
      };
      FocusRemindSound result;
      if (Enum.TryParse<FocusRemindSound>(LocalSettings.Settings.PomoLocalSetting.FocusEndSound, out result))
        this.FocusEndSoundComboBox.SelectedIndex = (int) result;
      else
        this.FocusEndSoundComboBox.SelectedIndex = LocalSettings.Settings.EnableRingtone ? 1 : 0;
    }

    private void OnFocusEndSoundChanged(object sender, SimpleComboBoxViewModel e)
    {
      LocalSettings.Settings.PomoLocalSetting.FocusEndSound = ((FocusRemindSound) this.FocusEndSoundComboBox.SelectedIndex).ToString();
      RemindSoundPlayer.PlayFocusRemindSound(false, true);
    }

    private void InitBreakEndSound()
    {
      this.BreakEndSoundComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("none"),
        Utils.GetString("Default"),
        Utils.GetString("HarpSound"),
        Utils.GetString("DrumSound"),
        Utils.GetString("BeepSound"),
        Utils.GetString("BlocksSound"),
        Utils.GetString("ChimesSound"),
        Utils.GetString("CrystalSound"),
        Utils.GetString("LadderSound"),
        Utils.GetString("LatticeSound"),
        Utils.GetString("LeapSound"),
        Utils.GetString("MatrixSound"),
        Utils.GetString("PulseSound"),
        Utils.GetString("Spiral")
      };
      FocusRemindSound result;
      if (Enum.TryParse<FocusRemindSound>(LocalSettings.Settings.PomoLocalSetting.BreakEndSound, out result))
        this.BreakEndSoundComboBox.SelectedIndex = (int) result;
      else
        this.BreakEndSoundComboBox.SelectedIndex = LocalSettings.Settings.EnableRingtone ? 1 : 0;
    }

    private void OnBreakEndSoundChanged(object sender, SimpleComboBoxViewModel e)
    {
      LocalSettings.Settings.PomoLocalSetting.BreakEndSound = ((FocusRemindSound) this.BreakEndSoundComboBox.SelectedIndex).ToString();
      RemindSoundPlayer.PlayFocusRemindSound(true, true);
    }

    private void AutoShowWidgetClick(object sender, MouseButtonEventArgs e)
    {
      PomoSettings.NotifyCheckBoxChanged(sender, (RoutedEventArgs) e);
      LocalSettings.Settings.PomoLocalSetting.AutoShowWidget = this.AutoShowWidget.IsChecked.HasValue && this.AutoShowWidget.IsChecked.Value;
    }

    private void InitMiniModeThemeComboBox()
    {
      this.MiniModeThemeComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("DarkColor"),
        Utils.GetString("LightColor")
      };
      this.MiniModeThemeComboBox.SelectedIndex = !(LocalSettings.Settings.PomoWindowTheme == "dark") ? 1 : 0;
    }

    private void OnMiniModeThemeChanged(object sender, SimpleComboBoxViewModel e)
    {
      LocalSettings.Settings.PomoWindowTheme = this.MiniModeThemeComboBox.SelectedIndex == 0 ? "dark" : "light";
    }

    private void OnDisplayTypeClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.Tag is string tag) || !(tag != LocalSettings.Settings.PomoLocalSetting.DisplayType) || tag != "Normal" && !ProChecker.CheckPro(ProType.FocusMiniStyle, Window.GetWindow((DependencyObject) this)))
        return;
      LocalSettings.Settings.PomoLocalSetting.DisplayType = tag;
      LocalSettings.Settings.Save(true);
      TickFocusManager.OnDisplayTypeChanged();
      this.SetSelectedDisplayType();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/pomosettings.xaml", UriKind.Relative));
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
          this.SettingsPanel = (StackPanel) target;
          break;
        case 3:
          this.ProIcon = (Path) target;
          break;
        case 4:
          this.AutoSyncCheckBox = (CheckBox) target;
          this.AutoSyncCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.AutoSyncClick);
          break;
        case 5:
          this.PomoDurationText = (TextBox) target;
          this.PomoDurationText.KeyUp += new KeyEventHandler(this.OnSettingsKeyUp);
          this.PomoDurationText.TextChanged += new TextChangedEventHandler(this.OnSettingsTextChanged);
          break;
        case 6:
          this.ShortBreakDurationText = (TextBox) target;
          this.ShortBreakDurationText.KeyUp += new KeyEventHandler(this.OnSettingsKeyUp);
          this.ShortBreakDurationText.TextChanged += new TextChangedEventHandler(this.OnSettingsTextChanged);
          break;
        case 7:
          this.LongBreakDurationText = (TextBox) target;
          this.LongBreakDurationText.KeyUp += new KeyEventHandler(this.OnSettingsKeyUp);
          this.LongBreakDurationText.TextChanged += new TextChangedEventHandler(this.OnSettingsTextChanged);
          break;
        case 8:
          this.LongBreakEveryText = (TextBox) target;
          this.LongBreakEveryText.KeyUp += new KeyEventHandler(this.OnSettingsKeyUp);
          this.LongBreakEveryText.TextChanged += new TextChangedEventHandler(this.OnSettingsTextChanged);
          break;
        case 9:
          this.PomoCount = (TextBlock) target;
          break;
        case 10:
          this.AutoNextPomo = (CheckBox) target;
          this.AutoNextPomo.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.AutoPomoClick);
          break;
        case 11:
          this.AutoBreak = (CheckBox) target;
          this.AutoBreak.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.AutoBreakClick);
          break;
        case 12:
          this.AutoTimesGrid = (Grid) target;
          break;
        case 13:
          this.AutoPomoTimes = (TextBox) target;
          this.AutoPomoTimes.KeyUp += new KeyEventHandler(this.OnSettingsKeyUp);
          this.AutoPomoTimes.TextChanged += new TextChangedEventHandler(this.OnSettingsTextChanged);
          break;
        case 14:
          this.FocusEndSoundComboBox = (CustomSimpleComboBox) target;
          break;
        case 15:
          this.BreakEndSoundComboBox = (CustomSimpleComboBox) target;
          break;
        case 16:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDisplayTypeClick);
          break;
        case 17:
          this.NormalImage = (Image) target;
          break;
        case 18:
          this.DetailedSelect = (Grid) target;
          break;
        case 19:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDisplayTypeClick);
          break;
        case 20:
          this.CircleImage = (Image) target;
          break;
        case 21:
          this.CircleSelect = (Grid) target;
          break;
        case 22:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDisplayTypeClick);
          break;
        case 23:
          this.MiniImage = (Image) target;
          break;
        case 24:
          this.RectSelect = (Grid) target;
          break;
        case 25:
          this.MiniModeThemeComboBox = (CustomSimpleComboBox) target;
          break;
        case 26:
          this.OpacitySlider = (Slider) target;
          break;
        case 27:
          this.AutoShowWidget = (CheckBox) target;
          this.AutoShowWidget.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.AutoShowWidgetClick);
          break;
        case 28:
          this.PomoHotkey = (HotkeyControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
