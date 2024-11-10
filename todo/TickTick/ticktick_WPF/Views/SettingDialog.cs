// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.SettingDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Setting;
using ticktick_WPF.Views.Theme;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views
{
  public class SettingDialog : Window, IComponentConnector, IStyleConnector
  {
    private readonly Dictionary<SettingsType, UserControl> _settingItems = new Dictionary<SettingsType, UserControl>();
    private static SettingDialog _instance;
    private bool _changed;
    internal SettingDialog SettingsWindow;
    internal ListView MenuListView;
    internal Grid SettingContainer;
    internal Border ToastBorder;
    internal TextBlock ToastText;
    private bool _contentLoaded;

    public static void ShowSettingDialog(SettingsType type = SettingsType.Account, Window owner = null)
    {
      if (SettingDialog._instance == null)
      {
        SettingDialog settingDialog = new SettingDialog(type);
        settingDialog.Owner = owner;
        SettingDialog._instance = settingDialog;
      }
      try
      {
        SettingDialog._instance.Show();
        SettingDialog._instance.Activate();
        SettingDialog._instance.ForceSelectType(type);
        SettingDialog._instance.SetMenuListViewIndex(type);
      }
      catch (Exception ex)
      {
        SettingDialog settingDialog = new SettingDialog(type);
        settingDialog.Owner = owner;
        SettingDialog._instance = settingDialog;
        SettingDialog._instance.Show();
      }
    }

    private void ForceSelectType(SettingsType type) => this.GetSettingControl(type);

    public static void ShowTemplateSettingDialog(
      Window owner = null,
      TemplateKind kind = TemplateKind.Task,
      AddTaskViewModel addModel = null)
    {
      if (SettingDialog._instance == null)
      {
        SettingDialog settingDialog = new SettingDialog(SettingsType.More);
        settingDialog.Owner = owner;
        SettingDialog._instance = settingDialog;
        if (owner == null)
          SettingDialog._instance.Topmost = true;
      }
      try
      {
        SettingDialog._instance.Show();
        SettingDialog._instance.Activate();
        SettingDialog._instance.SetTemplate(kind, addModel);
        SettingDialog._instance.SetMenuListViewIndex(SettingsType.More);
      }
      catch (Exception ex)
      {
        SettingDialog settingDialog = new SettingDialog(SettingsType.More);
        settingDialog.Owner = owner;
        SettingDialog._instance = settingDialog;
        SettingDialog._instance.Show();
      }
    }

    private void SetTemplate(TemplateKind kind = TemplateKind.Task, AddTaskViewModel addModel = null)
    {
      MoreSettingsConfig element = new MoreSettingsConfig(kind, addModel);
      this._settingItems[SettingsType.More] = (UserControl) element;
      this.SettingContainer.Children.Clear();
      this.SettingContainer.Children.Add((UIElement) element);
      element.DelayScrollToTemplate();
    }

    public SettingDialog(SettingsType type = SettingsType.Account)
    {
      this.InitializeComponent();
      this.SettingsWindow.DataContext = (object) new SettingViewModel(type);
      this.Closing += (CancelEventHandler) ((sender, e) =>
      {
        SettingDialog._instance = (SettingDialog) null;
        this.Owner?.Activate();
      });
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
      App.Window.ReleaseLock();
    }

    private async void SetMenuListViewIndex(SettingsType t)
    {
      await Task.Delay(50);
      if (!(this.MenuListView.ItemsSource is ObservableCollection<SettingMenuItem> itemsSource))
        return;
      foreach (SettingMenuItem settingMenuItem in (Collection<SettingMenuItem>) itemsSource)
        settingMenuItem.IsSelected = settingMenuItem.Type == t;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    protected override async void OnClosed(EventArgs e)
    {
      await this.SaveSettings();
      App.Window.LoadAppOptions();
      JumpHelper.InitJumpList();
      ProjectWidgetsHelper.Reload();
      if (LocalSettings.Settings.ExtraSettings.UseSystemTheme)
        ThemeUtil.TrySetSystemTheme(true);
      else
        App.Instance.LoadTargetTheme(LocalSettings.Settings.ExtraSettings.AppTheme);
      base.OnClosed(e);
    }

    private async Task SaveSettings()
    {
      await LocalSettings.Settings.Save();
      if (this._changed || this._settingItems.ContainsKey(SettingsType.Shortcuts))
        await SettingsHelper.PushLocalSettings();
      AppLockModel lockConfig = await AppLockDao.GetLockConfig();
      if (lockConfig != null && lockConfig.LockAfter)
        App.Window.TryLock();
      else
        App.Window.ReleaseLock();
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is SettingMenuItem dataContext))
        return;
      this.SelectItem(dataContext);
    }

    private void SelectItem(SettingMenuItem model)
    {
      if (!(this.MenuListView.ItemsSource is ObservableCollection<SettingMenuItem> itemsSource))
        return;
      foreach (SettingMenuItem settingMenuItem in (Collection<SettingMenuItem>) itemsSource)
        settingMenuItem.IsSelected = false;
      model.IsSelected = true;
      UserControl element;
      if (this._settingItems.TryGetValue(model.Type, out element))
      {
        this.SettingContainer.Children.Clear();
        this.SettingContainer.Children.Add((UIElement) element);
        if (!(element is ShortcutsConfig shortcutsConfig))
          return;
        shortcutsConfig.InitShortcuts();
      }
      else
        this.GetSettingControl(model.Type);
    }

    private void GetSettingControl(SettingsType selectedType)
    {
      UserControl element = (UserControl) null;
      if (this._settingItems.ContainsKey(selectedType))
        return;
      switch (selectedType)
      {
        case SettingsType.Account:
          element = (UserControl) new AccountInfo(this);
          break;
        case SettingsType.Theme:
          element = (UserControl) new AppearanceControl();
          break;
        case SettingsType.Calendar:
          element = (UserControl) new SubscribeCalendar();
          break;
        case SettingsType.Shortcuts:
          element = (UserControl) new ShortcutsConfig();
          break;
        case SettingsType.About:
          element = (UserControl) new AboutInfo();
          break;
        case SettingsType.SmartList:
          element = (UserControl) new SmartListConfig();
          break;
        case SettingsType.Notification:
          element = (UserControl) new ReminderConfig();
          break;
        case SettingsType.Share:
          element = (UserControl) new ShareConfig();
          break;
        case SettingsType.Widget:
          element = (UserControl) new WidgetInfo();
          break;
        case SettingsType.DateTime:
          element = (UserControl) new DateTimeConfig();
          break;
        case SettingsType.StickyNote:
          element = (UserControl) new StickyNoteConfig();
          break;
        case SettingsType.Feature:
          element = (UserControl) new FeatureConfig();
          break;
        case SettingsType.Premium:
          element = (UserControl) new PremiumConfig();
          break;
        case SettingsType.More:
          element = (UserControl) new MoreSettingsConfig();
          break;
        case SettingsType.Debug:
          element = (UserControl) new DebugSettings();
          break;
      }
      if (element == null || this._settingItems.ContainsKey(selectedType))
        return;
      this._settingItems.Add(selectedType, element);
      this.SettingContainer.Children.Clear();
      this.SettingContainer.Children.Add((UIElement) element);
    }

    public void Toast(string toastText)
    {
      this.ToastBorder.Visibility = Visibility.Visible;
      this.ToastText.Text = toastText;
      this.ToastBorder.BeginStoryboard((Storyboard) this.FindResource((object) "ShowToast"));
    }

    private void OnToasted(object sender, EventArgs e)
    {
      this.ToastBorder.Visibility = Visibility.Collapsed;
    }

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      if (!this.MenuListView.IsMouseOver)
        return;
      e.Handled = true;
    }

    public static void CloseInstance() => SettingDialog._instance?.Close();

    public static bool SettingShow() => SettingDialog._instance != null;

    private void OnDragMove(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      this.DragMove();
    }

    public void SetChanged() => this._changed = true;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/settingdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.SettingsWindow = (SettingDialog) target;
          break;
        case 3:
          ((Timeline) target).Completed += new EventHandler(this.OnToasted);
          break;
        case 4:
          this.MenuListView = (ListView) target;
          break;
        case 6:
          this.SettingContainer = (Grid) target;
          break;
        case 7:
          this.ToastBorder = (Border) target;
          break;
        case 8:
          this.ToastText = (TextBlock) target;
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnDragMove);
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
      if (connectionId != 2)
      {
        if (connectionId != 5)
          return;
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
      }
      else
        ((FrameworkElement) target).RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
    }
  }
}
