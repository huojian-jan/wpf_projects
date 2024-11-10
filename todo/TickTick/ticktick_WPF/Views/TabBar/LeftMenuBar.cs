// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TabBar.LeftMenuBar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.TabBar
{
  public class LeftMenuBar : UserControl, IComponentConnector
  {
    public DateTime LoadTime;
    private string _pic;
    private NetworkError _errorType;
    public List<TabBarItemViewModel> TabBarItems;
    private bool _dragChanged;
    internal LeftMenuBar Root;
    internal Path Avatar;
    internal ImageBrush HeadimgImage;
    internal Image HeadProImage;
    internal Image HeadFreeImage;
    internal EscPopup MenuPopup;
    internal ItemsControl TopTabBar;
    internal Popup DragPopup;
    internal Path PopupPath;
    internal Image NetworkErrorImage;
    internal System.Windows.Controls.ToolTip SyncToolTip;
    internal Path SyncPath;
    internal Grid NotificationButton;
    internal Path NotificationDot;
    internal Border NotificationDotBorder;
    internal TextBlock NotificationDotText;
    internal EscPopup NotificationPopup;
    internal NotificationControl NotificationPanel;
    internal Grid MoreGrid;
    internal Path MorePath;
    internal Ellipse UpdateEllipse;
    internal EscPopup MorePopup;
    private bool _contentLoaded;

    public static bool CanUpdate { get; set; }

    public string SelectedModule
    {
      get
      {
        List<TabBarItemViewModel> tabBarItems = this.TabBarItems;
        if (tabBarItems == null)
          return (string) null;
        return tabBarItems.FirstOrDefault<TabBarItemViewModel>((Func<TabBarItemViewModel, bool>) (t => t.Selected))?.Module;
      }
      set
      {
        this.TabBarItems?.ForEach((Action<TabBarItemViewModel>) (item => item.Selected = item.Module == (LocalSettings.Settings.InSearch ? "Search" : value)));
      }
    }

    public LeftMenuBar()
    {
      this.InitializeComponent();
      DataChangedNotifier.IsDarkChanged += new EventHandler(this.OnIsDarkChanged);
      this.SetTabBarItems("List");
    }

    private void OnIsDarkChanged(object sender, EventArgs e) => this.RefreshHead();

    public async Task LoadUserInfo(bool force = false)
    {
      Task.Run((Func<Task>) (async () =>
      {
        await UserManager.CheckUserP(force);
        this.Dispatcher.Invoke<Task>((Func<Task>) (async () =>
        {
          await UserManager.PullUserInfo();
          this.LoadTime = DateTime.Now;
          this.RefreshHead();
          this.RefreshIcon();
          this.RefreshToolTip();
        }));
      }));
    }

    private void RefreshIcon() => LocalSettings.Settings.NotifyPropertyChanged("ShowMatrix");

    public void RefreshToolTip()
    {
      if (!(this.TopTabBar.ItemsSource is ObservableCollection<TabBarItemViewModel> source))
        source = new ObservableCollection<TabBarItemViewModel>();
      List<TabBarItemViewModel> list = source.Where<TabBarItemViewModel>((Func<TabBarItemViewModel, bool>) (item => item.Show)).ToList<TabBarItemViewModel>();
      list.Sort((Comparison<TabBarItemViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      int num = 0;
      foreach (TabBarItemViewModel barItemViewModel in list)
      {
        string empty = string.Empty;
        if (barItemViewModel.Module == "Search")
          empty = new HotkeyModel(LocalSettings.Settings.ShortCutModel.SearchTask).ToString();
        else if (!KeyBindingManager.HasCtrlNumKeyBinding())
        {
          empty = new HotkeyModel(LocalSettings.Settings.ShortCutModel.GetPropertyValue(string.Format("Tab0{0}", (object) (num + 1)))).ToString();
          ++num;
        }
        barItemViewModel.ToolTip = string.IsNullOrEmpty(empty) ? barItemViewModel.Name : barItemViewModel.Name + " (" + empty + ")";
      }
    }

    public async void RefreshHead()
    {
      try
      {
        UserInfoModel userInfo = await UserManager.GetUserInfo();
        if (userInfo != null)
          this.HeadimgImage.ImageSource = (ImageSource) await AvatarHelper.GetAvatarByUrlAsync(string.IsNullOrEmpty(userInfo.picture) ? "avatar-new.png" : userInfo.picture);
        if (this._pic != userInfo?.picture || string.IsNullOrEmpty(userInfo?.picture))
        {
          this._pic = userInfo?.picture;
          UtilLog.Info(string.Format("LeftMenu.RefreshHead : {0} ,isPro {1}", (object) userInfo?.picture, (object) LocalSettings.Settings.IsPro));
        }
        this.SetProCrown();
        userInfo = (UserInfoModel) null;
      }
      catch (Exception ex)
      {
      }
    }

    private void SetProCrown()
    {
      if (LocalSettings.Settings.IsPro)
      {
        this.HeadProImage.Visibility = Visibility.Visible;
        this.HeadFreeImage.Visibility = Visibility.Hidden;
      }
      else
      {
        this.HeadProImage.Visibility = Visibility.Hidden;
        this.HeadFreeImage.Visibility = Visibility.Visible;
      }
    }

    private void SetTabBarItems(string module)
    {
      this.TopTabBar.ItemsSource = (IEnumerable) new ObservableCollection<TabBarItemViewModel>();
      this.TabBarItems = new List<TabBarItemViewModel>()
      {
        new TabBarItemViewModel("Task", module),
        new TabBarItemViewModel("Calendar", module),
        new TabBarItemViewModel("Matrix", module),
        new TabBarItemViewModel("Habit", module),
        new TabBarItemViewModel("Pomo", module),
        new TabBarItemViewModel("Search", module)
      };
      this.SetItems();
      this.TopTabBar.ItemsSource = (IEnumerable) new ObservableCollection<TabBarItemViewModel>(this.TabBarItems);
    }

    public void SetItems()
    {
      List<TabBarModel> bars = LocalSettings.Settings.UserPreference?.desktopTabBars?.bars ?? TabBarModel.InitTabBars();
      foreach (TabBarItemViewModel tabBarItem in this.TabBarItems)
      {
        if (tabBarItem.CanHide())
          tabBarItem.Show = TabBarModel.IsActive(tabBarItem.Module, bars);
        tabBarItem.SortOrder = TabBarModel.GetTabBarSort(tabBarItem.Module, bars);
      }
      List<TabBarItemViewModel> list = this.TabBarItems.Where<TabBarItemViewModel>((Func<TabBarItemViewModel, bool>) (t => t.Show)).ToList<TabBarItemViewModel>();
      list.Sort((Comparison<TabBarItemViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      foreach (TabBarItemViewModel barItemViewModel in list)
        barItemViewModel.SortOrder = (long) list.IndexOf(barItemViewModel);
      this.TopTabBar.Height = (double) (list.Count * 48);
      this.RefreshToolTip();
      UtilLog.Info(string.Format("SetTabItems settings is null : {0}, {1}", (object) (LocalSettings.Settings.UserPreference?.desktopTabBars?.bars == null), (object) JsonConvert.SerializeObject((object) bars)));
      UtilLog.Info(this.TabBarItems.Aggregate<TabBarItemViewModel, string>(string.Empty, (Func<string, TabBarItemViewModel, string>) ((current, item) => current + string.Format("{0}, {1}, {2} | ", (object) item.Name, (object) item.Show, (object) item.SortOrder))));
    }

    private void HeadClick(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      this.InitMenuOptions();
    }

    private void InitMenuOptions()
    {
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "Settings", Utils.GetString("Settings"), Utils.GetImageSource("SettingsDrawingImage")),
        new CustomMenuItemViewModel((object) "Statistics", Utils.GetString("Statistics"), Utils.GetImageSource("StatisticsDrawingImage")),
        new CustomMenuItemViewModel((object) "Premium", Utils.GetString("Premium"), Utils.GetImageSource("PremiumDrawingImage")),
        new CustomMenuItemViewModel((object) "Logout", Utils.GetString("PublicLogout"), Utils.GetImageSource("LogoutDrawingImage"))
      };
      if (CacheManager.GetTeams().Count > 0)
        types.Insert(3, new CustomMenuItemViewModel((object) "ManageTeam", Utils.GetString("ManageTeam"), Utils.GetImageSource("ManageTeamDrawingImage")));
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.MenuPopup);
      customMenuList.Operated += new EventHandler<object>(this.OnActionSelected);
      customMenuList.Show();
    }

    private void ShowMoreOptions()
    {
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "OpenOnWeb", Utils.GetString("ViewOnWeb"), Utils.GetImageSource("OpenOnWebDrawingImage")),
        new CustomMenuItemViewModel((object) "CheckUpdate", Utils.GetString("PublicCheckUpdate"), Utils.GetImageSource("CheckUpdateDrawingImage"))
        {
          ExtraIcon = LeftMenuBar.CanUpdate ? Utils.GetIcon("UpgradeIcon") : (Geometry) null,
          ExtraIconColor = ThemeUtil.GetColorInString("#E03131"),
          ExtraIconAngle = 90,
          ExtraIconSize = 18
        },
        new CustomMenuItemViewModel((object) "Shortcut", Utils.GetString("Shortcut"), Utils.GetImageSource("ShortcutDrawingImage")),
        new CustomMenuItemViewModel((object) "HelpCenter", Utils.GetString("HelpCenter"), Utils.GetIcon("IcHelp")),
        new CustomMenuItemViewModel((object) "Feedback", Utils.GetString("PublicFeedback"), Utils.GetImageSource("FeedbackDrawingImage")),
        new CustomMenuItemViewModel((object) "Changelog", Utils.GetString("ChangeLog"), Utils.GetImageSource("ChangelogDrawingImage"))
      }, (Popup) this.MorePopup);
      customMenuList.Operated += new EventHandler<object>(this.OnActionSelected);
      customMenuList.Show();
    }

    private async void OnActionSelected(object sender, object e)
    {
      await Task.Delay(50);
      if (!(e is string str))
        return;
      if (str != null)
      {
        switch (str.Length)
        {
          case 4:
            if (str == "Sync")
            {
              UserActCollectUtils.AddClickEvent("tab_bar", "click", "sync");
              break;
            }
            break;
          case 6:
            if (str == "Logout")
            {
              UserActCollectUtils.AddClickEvent("tab_bar", "user", "signout");
              this.Logout();
              break;
            }
            break;
          case 7:
            if (str == "Premium")
            {
              UserActCollectUtils.AddClickEvent("tab_bar", "user", "premium");
              Utils.StartUpgrade("account_info");
              break;
            }
            break;
          case 8:
            switch (str[2])
            {
              case 'e':
                if (str == "Feedback")
                {
                  UserActCollectUtils.AddClickEvent("tab_bar", "three_dots", "feedback");
                  this.OpenTicket();
                  break;
                }
                break;
              case 'o':
                if (str == "Shortcut")
                {
                  UserActCollectUtils.AddClickEvent("tab_bar", "three_dots", "shortcut");
                  MainWindowManager.ShowShortCutPanel();
                  break;
                }
                break;
              case 't':
                if (str == "Settings")
                {
                  UserActCollectUtils.AddClickEvent("tab_bar", "user", "settings");
                  this.ShowSettingDialog();
                  break;
                }
                break;
            }
            break;
          case 9:
            switch (str[0])
            {
              case 'C':
                if (str == "Changelog")
                {
                  UserActCollectUtils.AddClickEvent("tab_bar", "three_dots", "changelog");
                  this.OpenChangeLog();
                  break;
                }
                break;
              case 'O':
                if (str == "OpenOnWeb")
                {
                  UserActCollectUtils.AddClickEvent("tab_bar", "three_dots", "home");
                  LeftMenuBar.ViewOnWeb();
                  break;
                }
                break;
            }
            break;
          case 10:
            switch (str[0])
            {
              case 'H':
                if (str == "HelpCenter")
                {
                  Utils.TryProcessStartUrl(Utils.IsDida() ? "https://help.dida365.com/" : "https://help.ticktick.com/");
                  break;
                }
                break;
              case 'M':
                if (str == "ManageTeam")
                {
                  List<TeamModel> teams = CacheManager.GetTeams();
                  if (teams.Count <= 0)
                    return;
                  Utils.TryProcessStartUrlWithToken("/webapp/#settings/team/members/" + teams[0].id);
                  return;
                }
                break;
              case 'S':
                if (str == "Statistics")
                {
                  UserActCollectUtils.AddClickEvent("tab_bar", "user", "statistics");
                  Utils.TryProcessStartUrlWithToken("/webapp/#statistics/overview?enablePomo=" + (LocalSettings.Settings.EnableFocus ? "true" : "false"));
                  break;
                }
                break;
            }
            break;
          case 11:
            if (str == "CheckUpdate")
            {
              UserActCollectUtils.AddClickEvent("tab_bar", "three_dots", "check_update");
              this.CheckUpdate();
              break;
            }
            break;
        }
      }
      UtilLog.Info("LeftMenu.MoreAction : " + str);
    }

    private void OnMoreClick(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      UserActCollectUtils.AddClickEvent("tab_bar", "click", "three_dots");
      this.ShowMoreOptions();
    }

    public void ShowSettingDialog()
    {
      if (string.IsNullOrEmpty(LocalSettings.Settings.LoginUserId))
      {
        new LoginDialog().ShowDialog();
      }
      else
      {
        SettingsHelper.PullRemoteSettings();
        SettingDialog.ShowSettingDialog(owner: Window.GetWindow((DependencyObject) this));
      }
    }

    private async void SyncButtonClick()
    {
      App.Window.SyncButtonClick();
      UserActCollectUtils.AddClickEvent("tab_bar", "click", "sync");
    }

    private void Logout() => App.Window.Logout();

    private void OpenChangeLog()
    {
      Utils.FindParent<MainWindow>((DependencyObject) this).OpenChangeLog();
    }

    private static void ViewOnWeb() => MainWindow.ViewOnWeb();

    private void CheckUpdate() => App.Window.CheckUpdate();

    private void OpenTicket() => this.ShowFeedBackDialog();

    private async void ShowFeedBackDialog()
    {
      if (InternetExplorerBrowserEmulation.IsVersion11())
      {
        NavigateWebBrowserWindow webBrowserWindow = new NavigateWebBrowserWindow("/v2/tickets/");
        webBrowserWindow.Owner = (Window) App.Window;
        webBrowserWindow.Show();
      }
      else
        this.JumpWebFeedBack();
    }

    private async void JumpWebFeedBack()
    {
      string url = BaseUrl.GetDomain() + "/sign/autoSignOn?token={0}&dest={1}";
      url = string.Format(url, (object) await Communicator.GetSignOnToken(), (object) "/v2/tickets/");
      Utils.TryProcessStartUrl(url);
      url = (string) null;
    }

    public void Reset()
    {
      try
      {
        this.HeadimgImage.ImageSource = (ImageSource) AvatarHelper.GetDefaultAvatar();
      }
      catch (Exception ex)
      {
        this.HeadimgImage.ImageSource = (ImageSource) null;
      }
      this.HeadProImage.Visibility = Visibility.Hidden;
      this.HeadFreeImage.Visibility = Visibility.Visible;
    }

    private void OnItemClick(object sender, TabBarItemViewModel e)
    {
      switch (e.Module)
      {
        case "Task":
          UserActCollectUtils.AddClickEvent("tab_bar", "click", "task");
          break;
        case "Calendar":
          UserActCollectUtils.AddClickEvent("tab_bar", "click", "calendar");
          EventArchiveSyncService.PullArchivedModels();
          CourseArchiveSyncService.PullArchivedModels();
          break;
        case "Matrix":
          SettingsHelper.PullRemotePreference();
          UserActCollectUtils.AddClickEvent("tab_bar", "click", "matrix");
          break;
        case "Habit":
          UserActCollectUtils.AddClickEvent("tab_bar", "click", "habit");
          break;
        case "Search":
          App.Window.StartSearch();
          UserActCollectUtils.AddClickEvent("tab_bar", "click", "search");
          break;
      }
      UtilLog.Info("LeftMenu.SwitchView : " + e.Module);
      DisplayModule result;
      if (!Enum.TryParse<DisplayModule>(e.Module, true, out result))
        return;
      bool inSearch = LocalSettings.Settings.InSearch;
      LocalSettings.Settings.InSearch = false;
      App.Window.SwitchModule(result, inSearch);
    }

    private void NotificationButtonClick(object sender, MouseButtonEventArgs e)
    {
      UtilLog.Info("LeftMenu.NotificationClick");
      UserActCollectUtils.AddClickEvent("tab_bar", "click", "notification");
      if (Utils.IsNetworkAvailable())
      {
        this.NotificationPopup.IsOpen = !this.NotificationPopup.IsOpen;
        if (!this.NotificationPopup.IsOpen)
          return;
        this.NotificationPanel.GetNotification();
      }
      else
        Utils.Toast(Utils.GetString("NoNetwork"));
    }

    public void ShowNotificationIndicator(NotificationUnreadCount notificationCount)
    {
      int num = notificationCount.Activity + notificationCount.Notification;
      if (num <= 0)
        return;
      string str = "99+";
      if (num <= 99)
        str = num.ToString();
      this.NotificationDotText.Text = str;
      this.NotificationDotBorder.Visibility = Visibility.Visible;
      this.NotificationDot.Data = Utils.GetIcon("NotificationCutPath");
    }

    public void HideNotificationIndicator()
    {
      this.NotificationDot.Data = Utils.GetIcon("NotificationPath");
      this.NotificationDotBorder.Visibility = Visibility.Collapsed;
    }

    private void TryMoveWindow(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      App.Window.DragMove();
    }

    private void OnSyncClick(object sender, MouseButtonEventArgs e)
    {
      UtilLog.Info("LeftMenu.SyncClick");
      this.SyncButtonClick();
    }

    public void StartSyncStory()
    {
      try
      {
        if (!(this.FindResource((object) "SyncStory") is Storyboard resource))
          return;
        resource.Begin();
      }
      catch
      {
      }
    }

    public void SetUpgradeDisplay(int level)
    {
      LeftMenuBar.CanUpdate = level >= 0;
      if (level >= 1)
      {
        this.MorePath.Data = Utils.GetIconData("LeftBarOMCut");
        this.UpdateEllipse.Visibility = Visibility.Visible;
      }
      else
      {
        this.UpdateEllipse.Visibility = Visibility.Collapsed;
        this.MorePath.Data = Utils.GetIconData("LeftBarOM");
      }
    }

    public void SetNetworkStatus(NetworkError errorType)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        try
        {
          if (this._errorType == errorType)
            return;
          this._errorType = errorType;
          this.Dispatcher.Invoke((Action) (() =>
          {
            switch (errorType)
            {
              case NetworkError.None:
                this.NetworkErrorImage.Visibility = Visibility.Collapsed;
                this.SyncToolTip.Content = (object) Utils.GetString("Sync");
                break;
              case NetworkError.NoNetwork:
                this.NetworkErrorImage.Visibility = Visibility.Visible;
                this.NetworkErrorImage.SetResourceReference(Image.SourceProperty, (object) "NoNetworkDrawingImage");
                this.SyncToolTip.Content = (object) Utils.GetString("LeftBarNoNetwork");
                break;
              case NetworkError.CheckProxy:
                this.NetworkErrorImage.Visibility = Visibility.Visible;
                this.NetworkErrorImage.SetResourceReference(Image.SourceProperty, (object) "NoConnectionDrawingImage");
                this.SyncToolTip.Content = (object) Utils.GetString("LeftBarCheckProxy");
                break;
              case NetworkError.Error:
                this.NetworkErrorImage.Visibility = Visibility.Visible;
                this.NetworkErrorImage.SetResourceReference(Image.SourceProperty, (object) "NoConnectionDrawingImage");
                this.SyncToolTip.Content = (object) Utils.GetString("LeftBarNoConnection");
                break;
            }
          }));
        }
        catch (Exception ex)
        {
          UtilLog.Info(ex.Message);
        }
      }));
    }

    public void TryDragModel(TabBarItemViewModel model)
    {
      if (model == null)
        return;
      this.DragPopup.Tag = (object) model;
      this.PopupPath.Data = model.Icon;
      this.PopupPath.Opacity = model.Selected ? 1.0 : 0.4;
      this.PopupPath.SetResourceReference(Shape.FillProperty, model.Selected ? (object) "LeftBarSelectedIconColor" : (object) "LeftBarColorOpacity100");
      model.Dragging = true;
      this.CaptureMouse();
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.ReleaseDrag);
      this.MouseMove += new MouseEventHandler(this.DragMove);
      this._dragChanged = false;
    }

    private void DragMove(object sender, MouseEventArgs e)
    {
      this.DragPopup.IsOpen = true;
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      System.Windows.Point position1 = e.GetPosition((IInputElement) this.TopTabBar);
      this.DragPopup.HorizontalOffset = position1.X - 36.0;
      this.DragPopup.VerticalOffset = position1.Y - 36.0;
      ContentPresenter pointVisibleItem = Utils.GetMousePointVisibleItem<ContentPresenter>(e, (FrameworkElement) this.TopTabBar);
      if (pointVisibleItem == null || !(pointVisibleItem.DataContext is TabBarItemViewModel dataContext) || !(this.DragPopup.Tag is TabBarItemViewModel tag) || dataContext.Equals((object) tag) || Math.Abs((double) pointVisibleItem.GetValue(Canvas.TopProperty) - (double) (dataContext.SortOrder * 48L)) > 5.0 || !(this.TopTabBar.ItemsSource is ObservableCollection<TabBarItemViewModel> itemsSource))
        return;
      List<TabBarItemViewModel> list = itemsSource.Where<TabBarItemViewModel>((Func<TabBarItemViewModel, bool>) (i => i.Show)).ToList<TabBarItemViewModel>();
      list.Sort((Comparison<TabBarItemViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      System.Windows.Point position2 = e.GetPosition((IInputElement) pointVisibleItem);
      list.Remove(tag);
      int num = list.IndexOf(dataContext);
      if (num >= 0)
        list.Insert(position2.Y > 20.0 ? num + 1 : num, tag);
      else
        list.Insert(0, tag);
      this._dragChanged = true;
      foreach (TabBarItemViewModel barItemViewModel in list)
        barItemViewModel.SortOrder = (long) list.IndexOf(barItemViewModel);
    }

    private void ReleaseDrag(object sender, MouseButtonEventArgs e)
    {
      this.ReleaseMouseCapture();
      this.MouseLeftButtonUp -= new MouseButtonEventHandler(this.ReleaseDrag);
      this.MouseMove -= new MouseEventHandler(this.DragMove);
      this.DragPopup.IsOpen = false;
      if (this.DragPopup.Tag is TabBarItemViewModel tag)
      {
        tag.Dragging = false;
        this.DragPopup.Tag = (object) null;
      }
      if (!this._dragChanged || !(this.TopTabBar.ItemsSource is ObservableCollection<TabBarItemViewModel> itemsSource))
        return;
      List<TabBarItemViewModel> list = itemsSource.Where<TabBarItemViewModel>((Func<TabBarItemViewModel, bool>) (i => i.Show)).ToList<TabBarItemViewModel>();
      list.Sort((Comparison<TabBarItemViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      List<(string, long)> idToOrders = new List<(string, long)>();
      foreach (TabBarItemViewModel barItemViewModel in list)
        idToOrders.Add((barItemViewModel.Module.ToUpper(), barItemViewModel.SortOrder));
      LocalSettings.Settings.SaveTabBarOrder(idToOrders);
      SettingsHelper.PushLocalPreference();
      this.RefreshToolTip();
    }

    private void OnTestClick(object sender, MouseButtonEventArgs e)
    {
      App.Window.ShowNewUserGuide();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tabbar/leftmenubar.xaml", UriKind.Relative));
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
          this.Root = (LeftMenuBar) target;
          break;
        case 2:
          this.Avatar = (Path) target;
          break;
        case 3:
          this.HeadimgImage = (ImageBrush) target;
          break;
        case 4:
          this.HeadProImage = (Image) target;
          break;
        case 5:
          this.HeadFreeImage = (Image) target;
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.HeadClick);
          break;
        case 7:
          this.MenuPopup = (EscPopup) target;
          break;
        case 8:
          this.TopTabBar = (ItemsControl) target;
          break;
        case 9:
          this.DragPopup = (Popup) target;
          break;
        case 10:
          this.PopupPath = (Path) target;
          break;
        case 11:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.TryMoveWindow);
          break;
        case 12:
          this.NetworkErrorImage = (Image) target;
          break;
        case 13:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSyncClick);
          break;
        case 14:
          this.SyncToolTip = (System.Windows.Controls.ToolTip) target;
          break;
        case 15:
          this.SyncPath = (Path) target;
          break;
        case 16:
          this.NotificationButton = (Grid) target;
          this.NotificationButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.NotificationButtonClick);
          break;
        case 17:
          this.NotificationDot = (Path) target;
          break;
        case 18:
          this.NotificationDotBorder = (Border) target;
          break;
        case 19:
          this.NotificationDotText = (TextBlock) target;
          break;
        case 20:
          this.NotificationPopup = (EscPopup) target;
          break;
        case 21:
          this.NotificationPanel = (NotificationControl) target;
          break;
        case 22:
          this.MoreGrid = (Grid) target;
          break;
        case 23:
          this.MorePath = (Path) target;
          break;
        case 24:
          this.UpdateEllipse = (Ellipse) target;
          break;
        case 25:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnMoreClick);
          break;
        case 26:
          this.MorePopup = (EscPopup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
