// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.MainWindowManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Eisenhower;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.TabBar;
using ticktick_WPF.Views.Widget;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class MainWindowManager
  {
    private static bool _tokenOutDateHandled;
    private static bool _resignIn;
    private static MainWindow _window;
    private static object _o = new object();
    private static bool _windowInited;

    public static void InitWindow()
    {
      if (MainWindowManager._window != null)
        return;
      Application.Current.Dispatcher.Invoke((Action) (() => MainWindowManager._window = new MainWindow()));
    }

    public static MainWindow Window => MainWindowManager._window;

    public static MatrixContainer MainMatrix => MainWindowManager.Window.MatrixContainer;

    public static ListViewContainer MainListView => MainWindowManager.Window.ListView;

    public static LeftMenuBar LeftBar => MainWindowManager.Window.LeftTabBar;

    public static CalendarControl MainCal => MainWindowManager.Window.MainCalendar;

    public static HabitContainer MainHabit => MainWindowManager.Window.HabitView;

    public static ImageSource BackImageSource { get; set; }

    internal static void TrySelectFilter(FilterModel filter)
    {
      if (LocalSettings.Settings.InSearch || filter == null)
        return;
      MainWindowManager.MainListView.SelectProject((ProjectIdentity) new FilterProjectIdentity(filter));
    }

    public static void CheckSelectedModule()
    {
      Utils.RunOnUiThread(Application.Current.Dispatcher, (Action) (() =>
      {
        MainWindowManager.Window.LeftTabBar.SetItems();
        bool flag = false;
        switch (LocalSettings.Settings.MainWindowDisplayModule)
        {
          case 2:
            if (!LocalSettings.Settings.ShowMatrix)
            {
              flag = true;
              break;
            }
            break;
          case 3:
            if (!LocalSettings.Settings.ShowHabit)
            {
              flag = true;
              break;
            }
            break;
          case 4:
            if (!LocalSettings.Settings.EnableFocus)
            {
              flag = true;
              break;
            }
            break;
        }
        if (!flag)
          return;
        MainWindowManager.Window.SwitchModule(DisplayModule.Task);
      }));
    }

    public static ProjectIdentity GetSelectedProject()
    {
      if (LocalSettings.Settings.MainWindowDisplayModule == 2)
        return (ProjectIdentity) null;
      return MainWindowManager.MainListView?.GetSelectedProject();
    }

    public static void HandleLogout()
    {
      MainWindowManager._tokenOutDateHandled = true;
      MainWindowManager._resignIn = true;
      MainWindowManager.Window.ClearChildren();
      SettingDialog.CloseInstance();
      MainWindowManager.Window.LeftTabBar.Reset();
      TickFocusManager.MainFocus = (FocusView) null;
    }

    public static bool IsReSignIn()
    {
      if (!MainWindowManager._resignIn)
        return false;
      MainWindowManager._resignIn = false;
      return true;
    }

    public static void BeginSyncStory()
    {
      ThreadUtil.DetachedRunOnUiThread((Action) (() => MainWindowManager.LeftBar?.StartSyncStory()));
    }

    public static void TokenOutDate()
    {
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        if (MainWindowManager._tokenOutDateHandled)
          return;
        MainWindowManager._tokenOutDateHandled = true;
        int num = (int) MessageBox.Show(Utils.GetString("LoginOutDate"), Utils.GetString("LoginOutDate"), MessageBoxButton.OK);
        MainWindowManager.Window?.ForceLogout();
      }));
    }

    public static async Task HandleSyncFinished(SyncResult syncResult)
    {
      if (syncResult.SyncType == 1)
      {
        NotificationUnreadCount notificationCount1 = syncResult.NotificationCount;
        if ((notificationCount1 != null ? (notificationCount1.Activity > 0 ? 1 : 0) : 0) == 0)
        {
          NotificationUnreadCount notificationCount2 = syncResult.NotificationCount;
          if ((notificationCount2 != null ? (notificationCount2.Notification > 0 ? 1 : 0) : 0) == 0)
          {
            MainWindowManager.LeftBar.HideNotificationIndicator();
            goto label_5;
          }
        }
        MainWindowManager.LeftBar.ShowNotificationIndicator(syncResult.NotificationCount);
      }
label_5:
      if (syncResult.TeamChanged)
        MainWindowManager.LeftBar.LoadUserInfo(true);
      MainWindowManager.MainListView?.OnSyncFinished(syncResult);
      DataChangedNotifier.NotifyAutoSyncDone(syncResult);
      MainWindowManager.LeftBar.RefreshHead();
      if (syncResult.RemoteTasksChanged || syncResult.RemoteProjectsChanged)
        ReminderCalculator.AssembleReminders();
      App.Instance.TryDismissRemindersOnSync();
    }

    private static void ReloadAll()
    {
      Application.Current?.Dispatcher.Invoke((Action) (() =>
      {
        if (MainWindowManager.Window.ShowList)
        {
          TaskCountCache.SetNeedLoad();
          MainWindowManager.MainListView?.ReloadView();
          MainWindowManager.MainListView?.LoadSavedProject();
        }
        if (MainWindowManager.Window.ShowCalendar)
          MainWindowManager.MainCal?.Reload(true);
        if (!MainWindowManager.Window.ShowMatrix)
          return;
        MainWindowManager.MainMatrix?.LoadTask();
      }));
    }

    public static void InitWindowSettingOnLogin()
    {
      MainWindowManager.Window.InitRestoreLocation();
      MainWindowManager.Window.CheckLoginStatus();
      MainWindowManager.LeftBar.RefreshHead();
      TickFocusManager.SetFocusType(!(LocalSettings.Settings.PomoType == FocusConstance.Focus) ? 1 : 0);
      TickFocusManager.LoadSavedFocus();
      MainWindowManager.LeftBar.LoadUserInfo();
      MainWindowManager.CheckSelectedModule();
      MainWindowManager._windowInited = true;
      MainWindowManager._tokenOutDateHandled = false;
      WindowManager.AppLockOrExit = false;
    }

    public static void SetSyncErrorStatus(NetworkError errorType)
    {
      MainWindowManager.LeftBar?.SetNetworkStatus(errorType);
    }

    public static void ShowShortCutPanel() => MainWindowManager.Window.ShowShortCutPanel();

    public static void DoOperation(SearchOperationViewModel model)
    {
      switch (model.Type)
      {
        case SearchOperationType.AddTask:
          MainWindowManager.Window.InputCommand();
          break;
        case SearchOperationType.GlobalAddTask:
          QuickAddWindow.ShowOrHideQuickAddWindow();
          break;
        case SearchOperationType.ShowOrHideApp:
          MainWindowManager.Window.HideWindow();
          break;
        case SearchOperationType.OpenOrCloseFocusWindow:
          TickFocusManager.HideOrShowFocusWidget(true);
          break;
        case SearchOperationType.NewSticky:
          TaskStickyWindow.CreateNewStickyStatic();
          break;
        case SearchOperationType.Task:
          MainWindowManager.Window.SwitchModule(DisplayModule.Task);
          LocalSettings.Settings.InSearch = false;
          break;
        case SearchOperationType.Calendar:
          MainWindowManager.Window.SwitchModule(DisplayModule.Calendar);
          LocalSettings.Settings.InSearch = false;
          break;
        case SearchOperationType.Matrix:
          MainWindowManager.Window.SwitchModule(DisplayModule.Matrix);
          LocalSettings.Settings.InSearch = false;
          break;
        case SearchOperationType.Habit:
          MainWindowManager.Window.SwitchModule(DisplayModule.Habit);
          LocalSettings.Settings.InSearch = false;
          break;
        case SearchOperationType.SearchTask:
          MainWindowManager.Window.SearchCommand();
          break;
        case SearchOperationType.GoSettings:
          MainWindowManager.LeftBar.ShowSettingDialog();
          break;
        case SearchOperationType.GoAll:
          MainWindowManager.Window.JumpSmartProject(SmartListType.All);
          break;
        case SearchOperationType.GoToday:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Today);
          break;
        case SearchOperationType.GoTomorrow:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Tomorrow);
          break;
        case SearchOperationType.GoNext7Day:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Week);
          break;
        case SearchOperationType.GoAssignToMe:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Assign);
          break;
        case SearchOperationType.GoInbox:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Inbox);
          break;
        case SearchOperationType.GoCompleted:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Completed);
          break;
        case SearchOperationType.GoAbandoned:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Abandoned);
          break;
        case SearchOperationType.GoTrash:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Trash);
          break;
        case SearchOperationType.GoSummary:
          MainWindowManager.Window.JumpSmartProject(SmartListType.Summary);
          break;
        case SearchOperationType.HelpCenter:
          MainWindowManager.GotoHelpCenter();
          break;
        case SearchOperationType.ShowShortCut:
          MainWindowManager.Window.ShowShortCutPanel();
          break;
        case SearchOperationType.GoProject:
          MainWindowManager.Window.NavigateProject("project", model.PtfId);
          break;
        case SearchOperationType.GoTag:
          MainWindowManager.Window.NavigateProject("tag", model.PtfId);
          break;
        case SearchOperationType.GoFilter:
          MainWindowManager.Window.NavigateProject("filter", model.PtfId);
          break;
        case SearchOperationType.GoSearch:
          SearchHelper.ClearTaskSearchModels();
          MainWindowManager.Window.OnSearch(new SearchExtra()
          {
            SearchKey = model.SearchText
          }, false);
          break;
      }
    }

    private static void GotoHelpCenter()
    {
      Utils.TryProcessStartUrl(Utils.IsDida() ? "https://help.dida365.com/" : "https://help.ticktick.com/");
    }

    public static void SetCalendarProStatus()
    {
      MainWindowManager.Window.Dispatcher.Invoke((Action) (() =>
      {
        if (MainWindowManager.MainCal != null && MainWindowManager.MainCal.Visibility == Visibility.Visible)
          MainWindowManager.MainCal.ShowProToast();
        CalendarWidgetHelper.CheckProEnable();
      }));
    }

    public static void OnDataInited() => MainWindowManager.ReloadAll();
  }
}
