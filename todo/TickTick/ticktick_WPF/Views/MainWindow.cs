// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Editing;
using KotlinModels;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.TouchPad;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Eisenhower;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Lock;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.MainListView.ProjectList;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.NewUser;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Summary;
using ticktick_WPF.Views.TabBar;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.TaskList.Item;
using ticktick_WPF.Views.Theme;
using ticktick_WPF.Views.Undo;
using ticktick_WPF.Views.Update;
using ticktick_WPF.Views.User;
using ticktick_WPF.Views.Widget;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views
{
  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
  public class MainWindow : 
    MyWindow,
    IListViewParent,
    IKeyBinding,
    IThemeable,
    IToastShowWindow,
    INavigateProject,
    IComponentConnector
  {
    private bool _goDown;
    private bool _viewDown;
    private bool _tabMenuFirst = true;
    private bool _thirdColumnFolded;
    private bool _projectFloat;
    private bool _needHideDetail;
    private const int WM_MOUSEHWHEEL = 526;
    private const int WM_MOUSEMOVE = 512;
    private static bool _isSpliteDragging;
    private static bool _checkShowTutorial;
    private static bool _proDialogShowed;
    private static bool _updateVersionShowed;
    private static bool _versionOutdated;
    private static DateTime _lastSyncTime = DateTime.Now;
    private bool _canDragToNormal;
    private System.Windows.Point _dragStartPoint;
    private string _currentMode = "list";
    private bool _goToPremium;
    private DateTime _headerClickOldTime = DateTime.Now;
    private DateTime? _lastShowUpdateTime;
    private DateTime? _lastStateChangeSyncTime;
    private Storyboard _leftProjectMenu;
    private System.Timers.Timer _lockHandler;
    private TaskDetailWindow _navigateWindow;
    private bool _needShowTemplateGuide;
    private System.Windows.Controls.ContextMenu _notifyIconMenu;
    private DateTime? _refreshSizeTime;
    private bool _upgradeShowing;
    private double _width = -1.0;
    private DateTime? _calendarDate;
    public CalendarControl MainCalendar;
    public HabitContainer HabitView;
    public SummaryControl Summary;
    public MatrixContainer MatrixContainer;
    private SearchDialog SearchDialog;
    private SearchOperationDialog SearchOperationDialog;
    private bool _hideMenu;
    private MainProjectStatus _menuStatus = MainProjectStatus.Show;
    private double _listDetailScale;
    public bool IsLoaded;
    private bool _showed;
    private DisplayModule _previousModule;
    private bool _ignoreFocus;
    private bool _mouseDown;
    private bool _inPrimaryScreen;
    private bool _enableHandleMinMax = true;
    private int _saveModule = -1;
    internal MainWindow Window;
    internal Grid Container;
    internal Border WindowBackground;
    internal ImageBrush WindowBackgroundImage;
    internal Border CalendarBorder;
    internal Border FocusBorder;
    internal Border HabitBorder;
    internal Border EisenhoverBorder;
    internal Border ListBorder;
    internal LeftMenuBar LeftTabBar;
    internal ContentControl TutorialContent;
    internal StackPanel ResizeButtons;
    internal System.Windows.Controls.Button UpgradeButton;
    internal System.Windows.Controls.Button MinButton;
    internal System.Windows.Controls.Button MaxButton;
    internal System.Windows.Controls.Button NormalButton;
    internal System.Windows.Controls.Button CloseButton;
    internal Polygon X;
    internal Border ImmersiveGrid;
    internal Border CoachMarks;
    internal Grid MaskPanel;
    internal Border ShortCutMask;
    internal Grid ToastGrid;
    private bool _contentLoaded;

    private void LoadCalendar()
    {
      this.FocusBorder.Visibility = Visibility.Collapsed;
      this.CalendarBorder.Visibility = Visibility.Visible;
      this.EisenhoverBorder.Visibility = Visibility.Collapsed;
      this.ListBorder.Visibility = Visibility.Collapsed;
      this.HabitBorder.Visibility = Visibility.Collapsed;
      if (this.MainCalendar == null)
        this.InitCalendar();
      else
        this._calendarDate = new DateTime?(this.MainCalendar.HeadView.StartDate);
      this.SetMainWindowsMinSize(false);
      CalendarEventLoader.Reset();
      this.MainCalendar.SetBlur();
      this.MainCalendar.GotoDate(this._calendarDate, true);
      this.MainCalendar.ArrangePanel.TryReloadTasks();
      this.MainCalendar.GetFocus();
      this.MainCalendar.CollectUserEvent();
    }

    private void InitCalendar()
    {
      this.MainCalendar = new CalendarControl(true);
      this.CalendarBorder.Child = (UIElement) this.MainCalendar;
    }

    public async Task LoadMatrix()
    {
      this.FocusBorder.Visibility = Visibility.Collapsed;
      this.CalendarBorder.Visibility = Visibility.Collapsed;
      this.EisenhoverBorder.Visibility = Visibility.Visible;
      this.ListBorder.Visibility = Visibility.Collapsed;
      this.HabitBorder.Visibility = Visibility.Collapsed;
      if (this.MatrixContainer == null)
        this.InitMatrix();
      this.SetMainWindowsMinSize(false);
      await Task.Delay(100);
      this.MatrixContainer?.LoadTask(false);
    }

    private void InitMatrix()
    {
      this.MatrixContainer = new MatrixContainer();
      this.EisenhoverBorder.Child = (UIElement) this.MatrixContainer;
    }

    private void SwitchFocus()
    {
      this.CalendarBorder.Visibility = Visibility.Collapsed;
      this.EisenhoverBorder.Visibility = Visibility.Collapsed;
      this.ListBorder.Visibility = Visibility.Collapsed;
      this.FocusBorder.Visibility = Visibility.Visible;
      this.HabitBorder.Visibility = Visibility.Collapsed;
      TickFocusManager.InitFocusControl();
      this.FocusBorder.Child = (UIElement) TickFocusManager.MainFocus;
      this.MinWidth = 450.0;
      this.MinHeight = 600.0;
    }

    private void SwitchHabit()
    {
      this.FocusBorder.Visibility = Visibility.Collapsed;
      this.CalendarBorder.Visibility = Visibility.Collapsed;
      this.EisenhoverBorder.Visibility = Visibility.Collapsed;
      this.ListBorder.Visibility = Visibility.Collapsed;
      this.HabitBorder.Visibility = Visibility.Visible;
      this.HabitView = new HabitContainer();
      this.HabitBorder.Child = (UIElement) this.HabitView;
      this.SetMainWindowsMinSize(true);
      this.HabitView.HideDetail();
      this.HabitView.LoadHabits();
    }

    private void LoadTaskView()
    {
      this.FocusBorder.Visibility = Visibility.Collapsed;
      this.CalendarBorder.Visibility = Visibility.Collapsed;
      this.EisenhoverBorder.Visibility = Visibility.Collapsed;
      this.HabitBorder.Visibility = Visibility.Collapsed;
      this.ListBorder.Visibility = Visibility.Visible;
      if (this.ListBorder.Child == null)
      {
        this.ListView = ListViewContainer.GetListView(nameof (MainWindow), this.ListBorder);
        this.ListBorder.Child = (UIElement) this.ListView;
      }
      bool hideDetail = this.ListView.Mode == ListMode.Search;
      if (this.ListView.Mode == ListMode.Search && !LocalSettings.Settings.InSearch)
        this.ListView.StopSearch();
      if (LocalSettings.Settings.InSearch)
        return;
      this.ListView.ReloadView(hideDetail);
    }

    public void OnProjectWidthChanged(double width)
    {
      LocalSettings.Settings.ProjectPanelWidth = width;
    }

    public void OnDetailWidthChanged(double width)
    {
      LocalSettings.Settings.DetailListDivide = width;
    }

    public void SaveSelectedProject(string saveProjectId)
    {
      LocalSettings.Settings.SelectProjectId = saveProjectId;
    }

    public double GetProjectWidth() => LocalSettings.Settings.ProjectPanelWidth;

    public double GetDetailWidth() => LocalSettings.Settings.DetailListDivide;

    public string GetSelectedProject() => LocalSettings.Settings.SelectProjectId;

    public void SetMinSize(int width, int height)
    {
      this.MinWidth = (double) width;
      this.MinHeight = (double) height;
    }

    public void InputCommand()
    {
      KeyBindingCommand.InputCommand((object) this, this.ListView?.GetSelectedProject());
    }

    public void CloseCommand() => this.OnCloseButtonClick((object) null, (RoutedEventArgs) null);

    public void ExitImmersiveCommand()
    {
      this.ExitImmersiveMode();
      this.ListView?.OnEsc();
    }

    public void JumpCalendar()
    {
      if (LocalSettings.Settings.MainWindowDisplayModule == 1)
        return;
      this.SwitchModule(DisplayModule.Calendar);
    }

    public void JumpSmartProject(SmartListType smartListType)
    {
      this.SwitchModule(DisplayModule.Task);
      ListViewContainer listView = this.ListView;
      if ((listView != null ? (listView.Mode == ListMode.Search ? 1 : 0) : 0) != 0)
        this.StopSearch();
      this.ListView?.ProjectList?.TrySelectSmartProject(smartListType);
    }

    public void JumpHabit()
    {
      if (!LocalSettings.Settings.ShowHabit || LocalSettings.Settings.MainWindowDisplayModule == 3)
        return;
      this.SwitchModule(DisplayModule.Habit);
    }

    public void JumpTab(int index)
    {
      if (index < 1 || index > 9)
        return;
      UserActCollectUtils.AddShortCutEvent("navigate", "switch_the_tab_bar");
      List<TabBarModel> list = (LocalSettings.Settings.UserPreference?.desktopTabBars?.bars ?? TabBarModel.InitTabBars()).Where<TabBarModel>((Func<TabBarModel, bool>) (t => t.status == "active" && t.name != "SEARCH")).ToList<TabBarModel>();
      list.Sort((Comparison<TabBarModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
      DisplayModule result;
      if (list.Count < index || !System.Enum.TryParse<DisplayModule>(list[index - 1].name, true, out result))
        return;
      this.SwitchModule(result);
    }

    public void SearchCommand() => this.StartSearch();

    public void SearchOperationCommand()
    {
      UserActCollectUtils.AddShortCutEvent("general", "open_command_menu");
      this.ShowSearchOperation();
    }

    public void Print(bool isDetail = false) => this.ListView?.TryPrint(isDetail);

    private ITaskOperation GetFocusedTaskItem()
    {
      switch (FocusManager.GetFocusedElement((DependencyObject) this))
      {
        case ITaskOperation focusedTaskItem:
          return focusedTaskItem;
        case TextArea child:
          ITaskOperation parent = Utils.FindParent<ITaskOperation>((DependencyObject) child);
          if (parent != null)
            return parent;
          break;
      }
      return (ITaskOperation) null;
    }

    public void BatchSetPriorityCommand(int priority)
    {
      this.ListView?.TryBatchSetPriority(priority);
      if (this.MatrixContainer == null || !this.MatrixContainer.IsVisible)
        return;
      this.MatrixContainer.TryBatchSetPriority(priority);
    }

    public void BatchOpenStickyCommand()
    {
      this.ListView?.BatchOpenSticky();
      if (this.MatrixContainer == null || !this.MatrixContainer.IsVisible)
        return;
      this.MatrixContainer.BatchOpenSticky();
    }

    public void BatchSetDateCommand(DateTime? date)
    {
      this.ListView?.TryBatchSetDate(date);
      if (this.MatrixContainer == null || !this.MatrixContainer.IsVisible)
        return;
      this.MatrixContainer.TryBatchSetDate(date);
    }

    public void BatchPinTaskCommand()
    {
      this.ListView?.BatchPinTask();
      if (this.MatrixContainer == null || !this.MatrixContainer.IsVisible)
        return;
      this.MatrixContainer.BatchPinTask();
    }

    public void SelectDate(bool relative) => this.GetFocusedTaskItem()?.SelectDate(relative);

    public void ToggleTaskCompleted()
    {
      this.ListView?.GetTaskView()?.GetTaskListView()?.ToggleSelectedTaskCompleted();
    }

    public void BatchDeleteCommand()
    {
      this.ListView?.BatchDeleteTask();
      if (this.MatrixContainer == null || !this.MatrixContainer.IsVisible)
        return;
      this.MatrixContainer.BatchDeleteTask();
    }

    public void TaskDelete()
    {
      ITaskOperation focusedTaskItem = this.GetFocusedTaskItem();
      if (focusedTaskItem != null)
        focusedTaskItem.Delete();
      else
        this.BatchDeleteCommand();
    }

    private void InitShortcut()
    {
      KeyBindingManager.TryAddKeyBinding("SyncTask", GetKeyBinding(MainWindowCommands.SyncCommand));
      KeyBindingManager.TryAddKeyBinding("SearchTask", GetKeyBinding(MainWindowCommands.SearchCommand));
      KeyBindingManager.TryAddKeyBinding("AddTask", GetKeyBinding(MainWindowCommands.InputCommand));
      KeyBindingManager.TryAddKeyBinding("JumpAll", GetKeyBinding(MainWindowCommands.JumpAllList));
      KeyBindingManager.TryAddKeyBinding("JumpToday", GetKeyBinding(MainWindowCommands.JumpTodayList));
      KeyBindingManager.TryAddKeyBinding("JumpTomorrow", GetKeyBinding(MainWindowCommands.JumpTomorrowList));
      KeyBindingManager.TryAddKeyBinding("JumpWeek", GetKeyBinding(MainWindowCommands.JumpWeekList));
      KeyBindingManager.TryAddKeyBinding("JumpAssign", GetKeyBinding(MainWindowCommands.JumpAssignMeList));
      KeyBindingManager.TryAddKeyBinding("JumpInbox", GetKeyBinding(MainWindowCommands.JumpInboxList));
      KeyBindingManager.TryAddKeyBinding("JumpComplete", GetKeyBinding(MainWindowCommands.JumpCompleteList));
      KeyBindingManager.TryAddKeyBinding("JumpTrash", GetKeyBinding(MainWindowCommands.JumpTrashList));
      KeyBindingManager.TryAddKeyBinding("JumpSummary", GetKeyBinding(MainWindowCommands.JumpSummary));
      KeyBindingManager.TryAddKeyBinding("Print", GetKeyBinding(MainWindowCommands.PrintCommand));
      KeyBindingManager.TryAddKeyBinding("PrintDetail", GetKeyBinding(MainWindowCommands.PrintDetailCommand));
      KeyBindingManager.TryAddKeyBinding("SetNoPriority", GetKeyBinding(MainWindowCommands.SetPriorityNoneCommand));
      KeyBindingManager.TryAddKeyBinding("SetLowPriority", GetKeyBinding(MainWindowCommands.SetPriorityLowCommand));
      KeyBindingManager.TryAddKeyBinding("SetMediumPriority", GetKeyBinding(MainWindowCommands.SetPriorityMediumCommand));
      KeyBindingManager.TryAddKeyBinding("SetHighPriority", GetKeyBinding(MainWindowCommands.SetPriorityHighCommand));
      KeyBindingManager.TryAddKeyBinding("ClearDate", GetKeyBinding(MainWindowCommands.ClearDateCommand));
      KeyBindingManager.TryAddKeyBinding("SetToday", GetKeyBinding(MainWindowCommands.SetTodayCommand));
      KeyBindingManager.TryAddKeyBinding("SetTomorrow", GetKeyBinding(MainWindowCommands.SetTomorrowCommand));
      KeyBindingManager.TryAddKeyBinding("SetNextWeek", GetKeyBinding(MainWindowCommands.SetNextWeekCommand));
      KeyBindingManager.TryAddKeyBinding("SetDate", GetKeyBinding(MainWindowCommands.SelectDateCommand));
      KeyBindingManager.TryAddKeyBinding("CompleteTask", GetKeyBinding(MainWindowCommands.CompleteCommand));
      KeyBindingManager.TryAddKeyBinding("DeleteTask", GetKeyBinding(MainWindowCommands.DeleteCommand));
      KeyBindingManager.TryAddKeyBinding("ExpandAllTask", GetKeyBinding(MainWindowCommands.ExpandAllTaskCommand));
      KeyBindingManager.TryAddKeyBinding("ExpandAllSection", GetKeyBinding(MainWindowCommands.ExpandAllSectionCommand));
      KeyBindingManager.TryAddKeyBinding("PinTask", GetKeyBinding(MainWindowCommands.PinTaskCommand));
      KeyBindingManager.TryAddKeyBinding("JumpAbandon", GetKeyBinding(MainWindowCommands.JumpAbandonList));
      KeyBindingManager.TryAddKeyBinding("OpenSetting", GetKeyBinding(MainWindowCommands.OpenSettingCommand));
      KeyBindingManager.TryAddKeyBinding("JumpCalendar", GetKeyBinding(MainWindowCommands.JumpCalendar));
      KeyBindingManager.TryAddKeyBinding("JumpHabit", GetKeyBinding(MainWindowCommands.JumpHabitList));
      KeyBindingManager.TryAddKeyBinding("OpenSticky", GetKeyBinding(MainWindowCommands.OpenStickyCommand));
      KeyBindingManager.TryAddKeyBinding("OmListView", GetKeyBinding(MainWindowCommands.ListViewCommand));
      KeyBindingManager.TryAddKeyBinding("OmKanbanView", GetKeyBinding(MainWindowCommands.KanbanViewCommand));
      KeyBindingManager.TryAddKeyBinding("OmTimelineView", GetKeyBinding(MainWindowCommands.TimelineViewCommand));
      bool flag = KeyBindingManager.HasCtrlNumKeyBinding();
      for (int index = 1; index <= 9; ++index)
      {
        if (!flag)
          LocalSettings.Settings.ShortCutModel.SetPropertyValue(string.Format("Tab0{0}", (object) index), "Ctrl+D" + index.ToString());
        KeyBindingManager.TryAddKeyBinding(string.Format("Tab0{0}", (object) index), GetKeyBinding(GetCommand(string.Format("Tab0{0}Command", (object) index))));
      }

      static ICommand GetCommand(string commandName)
      {
        return typeof (MainWindowCommands).GetField(commandName).GetValue((object) null) as ICommand;
      }

      KeyBinding GetKeyBinding(ICommand command)
      {
        if (command == null)
          return (KeyBinding) null;
        KeyBinding keyBinding1 = new KeyBinding(command, new KeyGesture(Key.None));
        keyBinding1.CommandParameter = (object) this;
        KeyBinding keyBinding2 = keyBinding1;
        this.InputBindings.Add((InputBinding) keyBinding2);
        return keyBinding2;
      }
    }

    public bool TryTabListAndDetail()
    {
      IInputElement focusedElement = FocusManager.GetFocusedElement((DependencyObject) this);
      if (focusedElement == null || focusedElement.Equals((object) this))
        return false;
      ListViewContainer listView = this.ListView;
      return listView != null && listView.TabListAndDetail(focusedElement);
    }

    public void ToggleAppPinOption()
    {
      if (LocalSettings.Settings.MainWindowTopmost)
      {
        this.ToggleOptionItem(AppOption.Pin, Visibility.Collapsed);
        this.ToggleOptionItem(AppOption.Unpin, Visibility.Visible);
      }
      else
      {
        this.ToggleOptionItem(AppOption.Pin, Visibility.Visible);
        this.ToggleOptionItem(AppOption.Unpin, Visibility.Collapsed);
      }
    }

    public void ShowOrHideWindow()
    {
      if (!(LocalSettings.Settings["LoginUserId"].ToString() != ""))
        return;
      if (this.IsActive && this.Visibility == Visibility.Visible)
        this.HideWindow();
      else
        this.TryShowMainWindow();
    }

    public void TryUndo()
    {
      if (this.ToastGrid.Children.Count <= 0 || !(this.ToastGrid.Children[0] is UndoToast child) || child.Visibility != Visibility.Visible)
        return;
      child.OnUndo();
    }

    public async void ExpandOrFoldAllTask(bool isOpen)
    {
      this.ListView?.ExpandOrFoldAllTask(isOpen);
      this.MatrixContainer?.ExpandOrFoldAllTask();
    }

    public async void ExpandOrFoldAllSection()
    {
      this.ListView?.ExpandOrFoldAllSection();
      this.MatrixContainer?.ExpandOrFoldAllSection();
    }

    private void DoOperationOnKeyUp(Key e)
    {
      if (e == Key.Oem2 && Utils.IfShiftPressed())
      {
        this.ShowShortCutPanel();
      }
      else
      {
        if (this._viewDown && e >= Key.A && e <= Key.Z)
          this.DoViewAction(e);
        this._viewDown = e == Key.V && !this._goDown;
        if (this._goDown && e >= Key.A && e <= Key.Z && !Utils.IfShiftPressed() && !Utils.IfCtrlPressed())
          this.DoGoAction(e);
        this._goDown = e == Key.G && !this._goDown && !Utils.IfShiftPressed() && !Utils.IfCtrlPressed();
      }
    }

    private void DoViewAction(Key key)
    {
      if (LocalSettings.Settings.MainWindowDisplayModule != 0)
        return;
      ProjectIdentity selectedProject = this.ListView?.GetSelectedProject();
      string viewMode = string.Empty;
      switch (key)
      {
        case Key.K:
          UserActCollectUtils.AddShortCutEvent("switch_views", "kanban_view");
          viewMode = "kanban";
          break;
        case Key.L:
          UserActCollectUtils.AddShortCutEvent("switch_views", "list_view");
          viewMode = "list";
          break;
        case Key.T:
          UserActCollectUtils.AddShortCutEvent("switch_views", "timeline_view");
          viewMode = "timeline";
          break;
      }
      if (string.IsNullOrEmpty(viewMode))
        return;
      selectedProject.SwitchViewMode(viewMode);
    }

    private void DoGoAction(Key key)
    {
      switch (key)
      {
        case Key.A:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_all");
          this.JumpSmartProject(SmartListType.All);
          break;
        case Key.B:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_summary");
          this.JumpSmartProject(SmartListType.Summary);
          break;
        case Key.C:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_completed");
          this.JumpSmartProject(SmartListType.Completed);
          break;
        case Key.G:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_trash");
          this.JumpSmartProject(SmartListType.Trash);
          break;
        case Key.I:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_inbox");
          this.JumpSmartProject(SmartListType.Inbox);
          break;
        case Key.M:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_assign");
          this.JumpSmartProject(SmartListType.Assign);
          break;
        case Key.N:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_7D");
          this.JumpSmartProject(SmartListType.Week);
          break;
        case Key.R:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_tom");
          this.JumpSmartProject(SmartListType.Tomorrow);
          break;
        case Key.S:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_settings");
          this.LeftTabBar.ShowSettingDialog();
          break;
        case Key.T:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_tod");
          this.JumpSmartProject(SmartListType.Today);
          break;
        case Key.W:
          UserActCollectUtils.AddShortCutEvent("navigate", "go_to_wont_do");
          this.JumpSmartProject(SmartListType.Abandoned);
          break;
      }
    }

    public void OnTabKeyUp()
    {
      if (this.MaskPanel.IsVisible || this.ListView == null || !this.ListView.IsVisible)
        return;
      if (!this.TryTabListAndDetail())
        this.ListView.TabSelectQuickAddItem();
      else
        this.ListView.TabKanbanSelect();
    }

    public void SwitchViewMode(string mode)
    {
      this.ListView?.GetSelectedProject()?.SwitchViewMode(mode);
    }

    private void OnMainWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (this._showed)
        this.NotifyWindowSizeChanged(e);
      this.ListBorder.Width = Math.Max(300.0, this.Container.ActualWidth - 50.0);
    }

    public void InitRestoreLocation()
    {
      this.RestoreWindowSize();
      this.RestoreLastPosition();
    }

    private void RestoreWindowSize()
    {
      this.SizeChanged -= new SizeChangedEventHandler(this.OnMainWindowSizeChanged);
      this.SizeChanged += new SizeChangedEventHandler(this.OnMainWindowSizeChanged);
      if (LocalSettings.Settings.Maxmized)
      {
        this.WindowState = WindowState.Maximized;
        this.DelaySetSize();
      }
      else
        this.SafeSetWindowSize();
    }

    private void SafeSetWindowSize(bool refresh = false, bool delayRefresh = false)
    {
      if (MainWindow._isSpliteDragging)
        return;
      double mainWindowWidth = LocalSettings.Settings.MainWindowWidth;
      double mainWindowHeight = LocalSettings.Settings.MainWindowHeight;
      try
      {
        this.Width = mainWindowWidth;
        this.Height = mainWindowHeight;
      }
      catch (Exception ex)
      {
        this.Width = 1020.0;
        this.Height = 680.0;
      }
      if (!refresh)
        return;
      this.RefreshUi(delayRefresh ? 1000 : 0);
    }

    private async void RefreshUi(int delayRefresh = 0)
    {
      MainWindow mainWindow = this;
      if (delayRefresh > 0)
        await Task.Delay(delayRefresh);
      mainWindow.Width += 2.0;
      mainWindow.InvalidateVisual();
      mainWindow.Width -= 2.0;
      mainWindow.UpdateLayout();
    }

    private async Task RestoreLastPosition()
    {
      MainWindow mainWindow = this;
      double left = LocalSettings.Settings.WindowLeft;
      double top = LocalSettings.Settings.WindowTop;
      mainWindow.Left = left;
      mainWindow.Top = top;
      if (left > SystemParameters.PrimaryScreenWidth || left < -1.0 * LocalSettings.Settings.MainWindowWidth / 2.0 || top < -1.0 * LocalSettings.Settings.MainWindowHeight / 2.0 || top > SystemParameters.PrimaryScreenHeight)
      {
        WindowHelper.MoveTo((System.Windows.Window) mainWindow, (int) left, (int) top);
        mainWindow.Left = left;
        mainWindow.Top = top;
      }
      await Task.Delay(2000);
      CompositionTarget compositionTarget = PresentationSource.FromVisual((Visual) mainWindow)?.CompositionTarget;
      if (compositionTarget == null)
        return;
      Matrix transformFromDevice = compositionTarget.TransformFromDevice;
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      System.Windows.Point pomoLocationSafely = WidgetLocationHelper.GetPomoLocationSafely(mainWindow.Left, mainWindow.Top, __nonvirtual (mainWindow.Width), __nonvirtual (mainWindow.Height), new Matrix?(transformFromDevice), new System.Windows.Point(100.0, 100.0));
      if (Math.Abs(pomoLocationSafely.X - left) <= 1.0 || Math.Abs(pomoLocationSafely.Y - top) <= 1.0)
        return;
      mainWindow.Left = pomoLocationSafely.X;
      mainWindow.Top = pomoLocationSafely.Y;
    }

    private void NotifyWindowSizeChanged(SizeChangedEventArgs e)
    {
      if (this.WindowState == WindowState.Maximized)
        return;
      this.DelaySaveLocation();
    }

    private void DelaySaveLocation()
    {
      DelayActionHandlerCenter.TryDoAction("MainWindowSaveLocation", new EventHandler(this.SaveLocation));
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e) => this._mouseDown = true;

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this.SearchDialog != null && this.SearchDialog.Showing)
        this.SearchDialog.TryClose();
      if (this.SearchOperationDialog != null && this.SearchOperationDialog.Showing)
        this.SearchOperationDialog.Close();
      this.ListView?.OnWindowMouseDown(e);
      this.HabitView?.OnWindowMouseDown(e);
      if (this.FocusBorder.Visibility != Visibility.Visible)
        return;
      TickFocusManager.MainFocus?.TryFoldDetail();
    }

    public void SetMainWindowsMinSize(bool isList)
    {
      if (isList)
      {
        this.MinWidth = 390.0;
        this.MinHeight = 400.0;
      }
      else
      {
        this.MinWidth = 630.0;
        this.MinHeight = 462.0;
      }
    }

    public bool ShowCalendar => LocalSettings.Settings.MainWindowDisplayModule == 1;

    public bool ShowList => LocalSettings.Settings.MainWindowDisplayModule == 0;

    public bool ShowHabit => LocalSettings.Settings.MainWindowDisplayModule == 3;

    public bool ShowMatrix => LocalSettings.Settings.MainWindowDisplayModule == 2;

    public bool ShowFocus => LocalSettings.Settings.MainWindowDisplayModule == 4;

    private bool MenuAutoHide
    {
      get => (this._menuStatus & MainProjectStatus.AutoFold) == MainProjectStatus.AutoFold;
    }

    private bool MenuShow => (this._menuStatus & MainProjectStatus.Show) == MainProjectStatus.Show;

    public ListViewContainer ListView { get; set; }

    static MainWindow() => MainWindow._updateVersionShowed = false;

    public MainWindow()
    {
      this.InitializeComponent();
      Utils.SetWindowChrome((System.Windows.Window) this, new Thickness(5.0, 5.0, 3.0, 5.0));
      this.OnWindowInitialized();
    }

    private async void OnWindowInitialized()
    {
      MainWindow mainWindow = this;
      mainWindow.Loaded += new RoutedEventHandler(mainWindow.OnMainWindowLoaded);
      await Task.Delay(10);
      mainWindow.InitViews();
      mainWindow.InitShortcut();
    }

    public async Task HandlePipelineMsg(string text)
    {
      if (await AppLockCache.GetAppLocked())
        AppUnlockWindow.TryUnlockApp(new Action(Navigate));
      else
        Navigate();

      void Navigate()
      {
        string str = text;
        if (str != null)
        {
          switch (str.Length)
          {
            case 4:
              if (str == "Show")
              {
                this.TryShowMainWindow();
                return;
              }
              goto label_37;
            case 8:
              if (str == "add_task")
              {
                this.OpenAddTaskWindow();
                return;
              }
              goto label_37;
            case 15:
              switch (str[10])
              {
                case '_':
                  if (str == "other_show_pomo")
                    break;
                  goto label_37;
                case 'i':
                  if (str == "task_show_inbox")
                  {
                    this.SwitchModule(DisplayModule.Task);
                    this.ListView?.ProjectList.SelectInbox();
                    this.ShowWindow();
                    return;
                  }
                  goto label_37;
                case 't':
                  if (str == "task_show_today")
                  {
                    this.SwitchModule(DisplayModule.Task);
                    this.ListView?.ProjectList.SelectToday();
                    this.ShowWindow();
                    return;
                  }
                  goto label_37;
                default:
                  goto label_37;
              }
              break;
            case 16:
              if (str == "other_start_pomo")
                break;
              goto label_37;
            case 17:
              if (str == "other_show_timing")
                goto label_36;
              else
                goto label_37;
            case 18:
              switch (str[0])
              {
                case 'o':
                  if (str == "other_start_timing")
                    goto label_36;
                  else
                    goto label_37;
                case 't':
                  if (str == "task_show_calendar")
                  {
                    this.ShowWindow();
                    this.SwitchModule(DisplayModule.Calendar);
                    return;
                  }
                  goto label_37;
                default:
                  goto label_37;
              }
            case 21:
              if (str == "other_add_list_widget")
              {
                App.AddProjectWidget();
                return;
              }
              goto label_37;
            case 23:
              switch (str[6])
              {
                case 'a':
                  if (str == "other_add_matrix_widget")
                  {
                    App.Instance.AddMatrixWidget();
                    return;
                  }
                  goto label_37;
                case 'o':
                  if (str == "other_open_focus_widget")
                  {
                    TickFocusManager.HideOrShowFocusWidget(true);
                    return;
                  }
                  goto label_37;
                default:
                  goto label_37;
              }
            case 25:
              if (str == "other_add_calendar_widget")
              {
                App.Instance.AddCalendarWidget();
                return;
              }
              goto label_37;
            case 26:
              if (str == "other_remove_matrix_widget")
              {
                MatrixWidgetHelper.CloseWidget();
                return;
              }
              goto label_37;
            case 28:
              if (str == "other_remove_calendar_widget")
              {
                CalendarWidgetHelper.CloseWidget();
                return;
              }
              goto label_37;
            default:
              goto label_37;
          }
          UserActCollectUtils.AddClickEvent("focus", "start_from", "dock");
          TickFocusManager.BeginFocus(true, true);
          return;
label_36:
          UserActCollectUtils.AddClickEvent("focus", "start_from", "dock");
          TickFocusManager.BeginFocus(false, true);
          return;
        }
label_37:
        this.NavigateProject(text);
      }
    }

    private void NavigateProject(string text)
    {
      if (text != null && text.Contains(":"))
      {
        string[] strArray = text.Split(':');
        if (strArray.Length == 2)
          this.NavigateProject(strArray[0], strArray[1]);
      }
      this.ShowWindow();
    }

    public void NavigateTodayProject()
    {
      this.TryShowMainWindow();
      this.SwitchModule(DisplayModule.Task);
      this.ListView?.ProjectList.TrySelectSmartProject(SmartListType.Today);
    }

    public void NavigateTomorrowProject()
    {
      this.TryShowMainWindow();
      this.SwitchModule(DisplayModule.Task);
      this.ListView?.ProjectList.TrySelectSmartProject(SmartListType.Tomorrow);
    }

    private void OpenAddTaskWindow() => QuickAddWindow.ShowQuickAddWindow();

    public string GetThemeId() => "light";

    public async void TaskDeleted(string taskId)
    {
      TaskModel taskById = await TaskDao.GetTaskById(taskId);
      if (taskById != null)
      {
        List<TaskModel> taskModels = new List<TaskModel>()
        {
          taskById
        };
        List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(taskById.id, taskById.projectId);
        if (subTasksByIdAsync != null && subTasksByIdAsync.Any<TaskModel>())
          taskModels.AddRange((IEnumerable<TaskModel>) subTasksByIdAsync);
        int num = await TaskService.BatchDeleteTasks(taskModels) ? 1 : 0;
        taskModels = (List<TaskModel>) null;
      }
      this.TryFocus();
    }

    public void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels)
    {
      if (undoModels == null || !undoModels.Any<TaskDeleteRecurrenceUndoEntity>())
        return;
      UndoToast uiElement = new UndoToast();
      uiElement.InitTaskUndo(undoModels[0]);
      this.Toast((FrameworkElement) uiElement);
    }

    public void Toast(FrameworkElement uiElement)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, uiElement);
      this.TryFocus();
    }

    public void TryToastString(object obj, string toastString)
    {
      WindowToastHelper.ToastString(this.ToastGrid, toastString);
    }

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks)
    {
      MainWindow mainWindow1 = this;
      if (!await TaskService.BatchDeleteTasks(tasks))
        return false;
      MainWindow mainWindow2 = mainWindow1;
      List<TaskModel> source = tasks;
      List<string> list = source != null ? source.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>() : (List<string>) null;
      mainWindow2.HideDetailOnTasksDelete(list);
      return true;
    }

    public void TaskComplete(CloseUndoToast undo)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, (FrameworkElement) undo);
      this.TryFocus();
    }

    public void TryHideToast()
    {
      if (this.ToastGrid.Children.Count <= 0)
        return;
      if (this.ToastGrid.Children[0] is UndoToast child)
        child.OnFinished();
      this.ToastGrid.Children[0].Visibility = Visibility.Collapsed;
    }

    private void HideDetailOnTasksDelete(List<string> taskIds)
    {
      if (taskIds == null || taskIds.Count == 0)
        return;
      this.ListView?.HideDetailOnTasksDelete(taskIds);
    }

    public event EventHandler<SearchExtra> Search;

    private async void DelaySetSize()
    {
      await Task.Delay(200);
      this.SafeSetWindowSize(true, true);
    }

    private async void InitViews()
    {
      MainWindow mainWindow = this;
      HotKeyUtils.ResignHotKey();
      mainWindow.InitNotifyIcon();
      MainWindow.CheckResetPassword();
      // ISSUE: explicit non-virtual call
      __nonvirtual (mainWindow.Title) = Utils.GetAppName();
      SystemEvents.SessionSwitch -= new SessionSwitchEventHandler(mainWindow.OnPowerModeChanged);
      SystemEvents.SessionSwitch += new SessionSwitchEventHandler(mainWindow.OnPowerModeChanged);
      SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(mainWindow.OnSystemUserPreferenceChanged);
      SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(mainWindow.OnSystemUserPreferenceChanged);
    }

    private void OnSystemUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
      if (!LocalSettings.Settings.ExtraSettings.UseSystemTheme)
        return;
      ThemeUtil.TrySetSystemTheme();
    }

    private async void OnPowerModeChanged(object sender, SessionSwitchEventArgs e)
    {
      MainWindow mainWindow = this;
      App.Instance.SetNotifyIcon(false);
      await Task.Delay(3000);
      mainWindow.RefreshUi();
      mainWindow.UpdateLayout();
      await Task.Delay(2000);
      mainWindow.RefreshUi();
      mainWindow.UpdateLayout();
    }

    private static void CheckResetPassword()
    {
      if (!LocalSettings.Settings.NeedResetPassword || !(LocalSettings.Settings.NeedResetUserId == LocalSettings.Settings.LoginUserId))
        return;
      LocalSettings.Settings.NeedResetPassword = false;
      LocalSettings.Settings.NeedResetUserId = string.Empty;
      new SetPasswordWindow(SetPasswordMode.Reset).ShowDialog();
    }

    private async void InitNotifyIcon()
    {
      MainWindow mainWindow = this;
      bool appLocked = await AppLockCache.GetAppLocked(true);
      App.NotifyIcon.Text = Utils.GetAppName();
      App.Instance.SetNotifyIcon(appLocked);
      App.NotifyIcon.Visible = true;
      App.NotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(mainWindow.OnAppIconClick);
      System.Windows.Forms.MenuItem[] menuItems = new System.Windows.Forms.MenuItem[0];
      mainWindow._notifyIconMenu = (System.Windows.Controls.ContextMenu) mainWindow.FindResource((object) "NotifyIconMenu");
      App.NotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(menuItems);
      App.NotifyIcon.ContextMenu.Collapse += new EventHandler(mainWindow.OnMenuClosed);
      App.NotifyIcon.ContextMenu.Popup += new EventHandler(mainWindow.OnMenuShowed);
      mainWindow.LoadAppOptions();
    }

    private void OnMenuShowed(object sender, EventArgs e)
    {
      this.LoadOptionShortcut();
      this._notifyIconMenu.IsOpen = true;
    }

    private void OnMenuClosed(object sender, EventArgs e) => this._notifyIconMenu.IsOpen = false;

    public async void LoadAppOptions()
    {
      AppLockModel model = await AppLockCache.GetModel();
      if (model != null && model.Locked)
      {
        this.SetLockedOption();
      }
      else
      {
        this.ToggleWidgetItems();
        if (!LocalSettings.Settings.MainWindowTopmost)
        {
          this.ToggleOptionItem(AppOption.Pin, Visibility.Visible);
          this.ToggleOptionItem(AppOption.Unpin, Visibility.Collapsed);
        }
        else
        {
          this.ToggleOptionItem(AppOption.Unpin, Visibility.Visible);
          this.ToggleOptionItem(AppOption.Pin, Visibility.Collapsed);
        }
        this.ToggleOptionItem(AppOption.Lock, Visibility.Visible);
        this.ToggleOptionItem(AppOption.Unlock, Visibility.Collapsed);
        this.ToggleSeparator(AppOptionSeparator.First, LocalSettings.Settings.EnableFocus ? Visibility.Visible : Visibility.Collapsed);
        this.ToggleSeparator(AppOptionSeparator.Second, Visibility.Visible);
      }
      this.SetPomoOption(model != null && model.Locked);
    }

    private void SetPomoOption(bool locked)
    {
      this.ToggleOptionItem(AppOption.StartFocus, !LocalSettings.Settings.EnableFocus || locked ? Visibility.Collapsed : Visibility.Visible);
      if (TickFocusManager.Working)
      {
        this.SetOptionTitleAndEnable(AppOption.StartPomo, Utils.GetString("ShowPomo"), LocalSettings.Settings.PomoType == FocusConstance.Focus);
        this.SetOptionTitleAndEnable(AppOption.StartTiming, Utils.GetString("ShowTiming"), LocalSettings.Settings.PomoType == FocusConstance.Timing);
      }
      else
      {
        this.SetOptionTitleAndEnable(AppOption.StartPomo, Utils.GetString("StartPomo"));
        this.SetOptionTitleAndEnable(AppOption.StartTiming, Utils.GetString("StartTiming"));
      }
    }

    public void LoadOptionShortcut()
    {
      this.SetOptionShortcut(AppOption.ShowFocusWidget, LocalSettings.Settings.PomoShortcut);
      this.SetOptionShortcut(AppOption.Lock, LocalSettings.Settings.LockShortcut);
      this.SetOptionShortcut(AppOption.Unlock, LocalSettings.Settings.LockShortcut);
      this.SetOptionShortcut(AppOption.Pin, LocalSettings.Settings.ShortcutPin);
      this.SetOptionShortcut(AppOption.Unpin, LocalSettings.Settings.ShortcutPin);
    }

    private async void OnOptionClick(object sender, MouseButtonEventArgs e)
    {
      this._notifyIconMenu.IsOpen = false;
      if (!(sender is AppOptionItem appOptionItem) || !(appOptionItem.Tag is AppOption tag))
        return;
      switch (tag)
      {
        case AppOption.AddListWidget:
          App.AddProjectWidget();
          break;
        case AppOption.AddCalendarWidget:
          App.Instance.AddCalendarWidget();
          break;
        case AppOption.CloseCalendarWidget:
          App.Instance.CloseCalendarWidget();
          break;
        case AppOption.AddMatrixWidget:
          App.Instance.AddMatrixWidget();
          break;
        case AppOption.CloseMatrixWidget:
          App.Instance.CloseMatrixWidget();
          break;
        case AppOption.StartFocus:
          TickFocusManager.HideOrShowFocusWidget(true);
          break;
        case AppOption.StartPomo:
          UserActCollectUtils.AddClickEvent("focus", "start_from", "dock");
          TickFocusManager.BeginFocus(true);
          break;
        case AppOption.StartTiming:
          UserActCollectUtils.AddClickEvent("focus", "start_from", "dock");
          TickFocusManager.BeginFocus(false);
          break;
        case AppOption.Pin:
          this.ToggleOptionItem(tag, Visibility.Collapsed);
          this.ToggleOptionItem(AppOption.Unpin, Visibility.Visible);
          LocalSettings.Settings.MainWindowTopmost = true;
          break;
        case AppOption.Unpin:
          this.ToggleOptionItem(tag, Visibility.Collapsed);
          this.ToggleOptionItem(AppOption.Pin, Visibility.Visible);
          LocalSettings.Settings.MainWindowTopmost = false;
          break;
        case AppOption.Lock:
          App.Instance.TryLockApp();
          break;
        case AppOption.Unlock:
          this.TryShowMainWindow();
          break;
        case AppOption.Exit:
          this.SaveSettingsBeforeExit();
          await LocalSettings.Settings.Save();
          App.Instance.ExitApplication();
          break;
      }
    }

    private void SaveSettingsBeforeExit() => SmartListTaskFoldHelper.Save();

    private AppOptionItem GetAppOption(AppOption key)
    {
      System.Windows.Controls.ContextMenu notifyIconMenu = this._notifyIconMenu;
      if ((notifyIconMenu != null ? (notifyIconMenu.HasItems ? 1 : 0) : 0) != 0)
      {
        foreach (object obj in (IEnumerable) this._notifyIconMenu.Items)
        {
          if (obj is AppOptionItem menu && menu.Tag is AppOption tag)
          {
            if (tag == key)
              return menu;
            AppOptionItem appOption = this.GetAppOption((System.Windows.Controls.MenuItem) menu, key);
            if (appOption != null)
              return appOption;
          }
        }
      }
      return (AppOptionItem) null;
    }

    private AppOptionItem GetAppOption(System.Windows.Controls.MenuItem menu, AppOption key)
    {
      if (menu != null && menu.HasItems)
      {
        foreach (object obj in (IEnumerable) menu.Items)
        {
          if (obj is AppOptionItem menu1 && menu1.Tag is AppOption tag)
          {
            if (tag == key)
              return menu1;
            AppOptionItem appOption = this.GetAppOption((System.Windows.Controls.MenuItem) menu1, key);
            if (appOption != null)
              return appOption;
          }
        }
      }
      return (AppOptionItem) null;
    }

    private void SetOptionShortcut(AppOption key, string shortcut)
    {
      AppOptionItem appOption = this.GetAppOption(key);
      if (appOption == null)
        return;
      appOption.Shortcut = shortcut;
    }

    private void SetOptionTitleAndEnable(AppOption key, string title, bool enable = true)
    {
      AppOptionItem appOption = this.GetAppOption(key);
      if (appOption == null)
        return;
      appOption.Title = title;
      appOption.IsEnabled = enable;
    }

    private void ToggleOptionItem(AppOption key, Visibility visibility)
    {
      AppOptionItem appOption = this.GetAppOption(key);
      if (appOption == null)
        return;
      appOption.Visibility = visibility;
    }

    private void ToggleSeparator(AppOptionSeparator key, Visibility visibility)
    {
      if (this._notifyIconMenu?.Items == null)
        return;
      foreach (object obj in (IEnumerable) this._notifyIconMenu.Items)
      {
        if (obj is Separator separator && separator.Tag is AppOptionSeparator tag && tag == key)
          separator.Visibility = visibility;
      }
    }

    private void SaveLocation(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() =>
      {
        if (this.WindowState == WindowState.Maximized)
          return;
        LocalSettings.Settings.WindowLeft = this.Left;
        LocalSettings.Settings.WindowTop = this.Top;
        if (this.ActualWidth >= this.MinWidth)
          LocalSettings.Settings.MainWindowWidth = this.ActualWidth;
        if (this.ActualHeight <= 0.0 || this.ActualWidth <= 0.0)
          return;
        LocalSettings.Settings.MainWindowHeight = this.ActualHeight;
      }));
    }

    public void OnEnterImmersive(string taskId, int caretIndex)
    {
      this.EnterImmersiveMode(taskId, caretIndex);
    }

    public async Task NavigateTask(ProjectTask task)
    {
      MainWindow mainWindow = this;
      if (await TaskUtils.TryLoadTask(task.TaskId, task.ProjectId) == null)
        return;
      if (mainWindow._navigateWindow == null)
      {
        mainWindow._navigateWindow = new TaskDetailWindow();
        mainWindow._navigateWindow.ShowInNavigate(task.TaskId, (System.Windows.Window) App.Window);
        mainWindow._navigateWindow.DependentWindow = (IToastShowWindow) mainWindow;
      }
      else
        mainWindow._navigateWindow?.OnNavigateTask(task);
      mainWindow._ignoreFocus = true;
      await Task.Delay(200);
      mainWindow._ignoreFocus = false;
    }

    public void NavigateCourse(string courseId)
    {
      if (string.IsNullOrEmpty(courseId))
        return;
      this.SwitchModule(DisplayModule.Task);
      this.ListView?.NavigateTodayProject();
      this.ListView?.GetTaskView()?.TaskSelect(new ListItemSelectModel(courseId, string.Empty, DisplayType.Course, TaskSelectType.Click));
    }

    public async void NavigateHabit(string habitId, bool showRecord = false)
    {
      if (string.IsNullOrEmpty(habitId))
        return;
      if (await HabitDao.GetHabitById(habitId) == null)
        return;
      this.SwitchModule(DisplayModule.Habit);
      this.HabitView?.NavigateItem(habitId);
      if (!showRecord)
        return;
      this.HabitView?.CheckInHabit(habitId);
    }

    public async void NavigateEvent(string eventId)
    {
      if (string.IsNullOrEmpty(eventId))
        return;
      CalendarEventModel model = await CalendarEventDao.GetEventByEventId(eventId);
      if (model == null)
        model = await CalendarEventDao.GetEventById(eventId);
      if (model != null)
      {
        CalendarSubscribeProfileModel profile = CacheManager.GetSubscribeCalendars().FirstOrDefault<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Id == model.CalendarId));
        if (profile != null)
        {
          this.SwitchModule(DisplayModule.Task);
          this.ListView?.SelectProject((ProjectIdentity) new SubscribeCalendarProjectIdentity(profile));
          await Task.Delay(100);
          this.ListView?.GetTaskView()?.NavigateEvent(eventId);
        }
        else
        {
          BindCalendarModel bindCalendar = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == model.CalendarId));
          if (bindCalendar != null)
          {
            BindCalendarAccountModel account = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (acc => acc.Id == bindCalendar.AccountId));
            if (account != null)
            {
              this.SwitchModule(DisplayModule.Task);
              this.ListView?.SelectProject((ProjectIdentity) new BindAccountCalendarProjectIdentity(account));
              await Task.Delay(100);
              this.ListView?.GetTaskView()?.NavigateEvent(eventId);
            }
          }
        }
      }
    }

    public async Task NavigateNormalProject(string projectId)
    {
      MainWindow mainWindow = this;
      mainWindow.SwitchModule(DisplayModule.Task);
      mainWindow.ListView?.SelectProject(ProjectIdHelper.GetProjectIdentity(projectId));
      if (await AppLockCache.GetAppLocked())
        AppUnlockWindow.TryUnlockApp(new Action(mainWindow.ShowWindow));
      else
        mainWindow.ShowWindow();
    }

    public async Task NavigateFilter(string filterId)
    {
      MainWindow mainWindow = this;
      mainWindow.SwitchModule(DisplayModule.Task);
      mainWindow.ListView?.SelectFilter(filterId);
      if (await AppLockCache.GetAppLocked())
        AppUnlockWindow.TryUnlockApp(new Action(mainWindow.ShowWindow));
      else
        mainWindow.ShowWindow();
    }

    public async Task NavigateProject(string type, string id)
    {
      MainWindow mainWindow = this;
      if (LocalSettings.Settings.InSearch)
        App.Window.StopSearch(false);
      mainWindow.SwitchModule(DisplayModule.Task);
      mainWindow.ListView?.NavigateProject(type, id);
      if (await AppLockCache.GetAppLocked())
        AppUnlockWindow.TryUnlockApp(new Action(mainWindow.ShowWindow));
      else
        mainWindow.ShowWindow();
    }

    public async void NavigateTask(string taskId, string itemId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null || thinTaskById.deleted == 2)
        return;
      this.SwitchModule(DisplayModule.Task);
      this.ListView?.NavigateTask(thinTaskById);
    }

    public void ReloadView(bool restore = false)
    {
      if (restore)
      {
        this.SwitchDisplayModule();
      }
      else
      {
        if (!this.ShowList)
          return;
        ListViewContainer listView = this.ListView;
        if ((listView != null ? (listView.Mode == ListMode.Normal ? 1 : 0) : 0) == 0)
          return;
        this.ListView?.ReloadView();
      }
    }

    public void Reload() => this.SwitchDisplayModule();

    private void SwitchDisplayModule()
    {
      DisplayModule windowDisplayModule = (DisplayModule) LocalSettings.Settings.MainWindowDisplayModule;
      this.LeftTabBar.SelectedModule = windowDisplayModule.ToString();
      ProChecker.TryCloseCalendarProDialog();
      switch (windowDisplayModule)
      {
        case DisplayModule.Task:
          this.LoadTaskView();
          break;
        case DisplayModule.Calendar:
          this.LoadCalendar();
          if (!UserDao.IsUserValid())
          {
            ProChecker.ShowUpgradeDialog(ProType.CalendarView, (System.Windows.Window) App.Window);
            break;
          }
          UserActCollectUtils.AddClickEvent("calendar", "view_show", LocalSettings.Settings.CalendarDisplaySettings.MainCal.GetSwitchText());
          break;
        case DisplayModule.Matrix:
          this.LoadMatrix();
          break;
        case DisplayModule.Habit:
          this.SwitchHabit();
          break;
        case DisplayModule.Pomo:
          this.SwitchFocus();
          break;
      }
    }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
      this.IsLoaded = true;
      this.Loaded -= new RoutedEventHandler(this.OnMainWindowLoaded);
      ++this.Width;
      this.CheckUpdateStart();
      UserActCollectUtils.AddClickEvent("dotnet", "version", Utils.GetDotNetVersion());
      PresentationSource presentationSource = PresentationSource.FromVisual((Visual) this);
      ((HwndSource) presentationSource)?.AddHook(new HwndSourceHook(this.Hook));
      if (TouchPadHelper.TouchpadHelper.Exists())
        TouchPadHelper.TouchpadHelper.RegisterInput(((HwndSource) presentationSource).Handle);
      this.OnWindowLoaded();
    }

    private async Task OnWindowLoaded()
    {
      AvatarHelper.GetAllProjectAvatars();
      LimitCache.InitLimitCache();
      this.LeftTabBar.SelectedModule = ((DisplayModule) LocalSettings.Settings.MainWindowDisplayModule).ToString();
      this.CheckVersionValid();
    }

    private async void CheckVersionValid(bool showPrompt = true)
    {
      if (UserDao.IsPro())
        return;
      await Task.Delay(2000);
      if (MainWindow._updateVersionShowed)
        return;
      if (!(UserManager.ProExpired() & showPrompt))
        return;
      try
      {
        if (!(DateTime.ParseExact(LocalSettings.Settings.LastProRemindeTime, "yyyy MM dd", (IFormatProvider) null) < DateTime.Today))
          return;
        this.ShowProExpired();
      }
      catch (Exception ex)
      {
        this.ShowProExpired();
      }
    }

    public async Task TryShowTutorial()
    {
      string abTestGroupResult = await ABTestManager.GetABTestGroupResult("newuser3_202403");
      if (!ABTestManager.IsAGroup(abTestGroupResult))
      {
        await TaskService.PullGuideTask();
        this.ShowTutorial();
      }
      else
      {
        this.ShowNewUserGuide();
        string str = await ProjectDao.PullGuideProject(abTestGroupResult);
        if (string.IsNullOrEmpty(str))
          return;
        LocalSettings.Settings.SelectProjectId = "project:" + str;
        this.ListView?.ProjectList?.LoadSavedProject();
      }
    }

    public void ShowNewUserGuide()
    {
      WindowUtils.SetWindowChromeBorderThickness((System.Windows.Window) this, new Thickness(0.0));
      this.ResizeMode = ResizeMode.NoResize;
      this.ResizeButtons.Visibility = Visibility.Hidden;
      this.WindowState = WindowState.Normal;
      this.Width = 1140.0;
      this.Height = 780.0;
      NewUserGuide element = new NewUserGuide();
      element.SetValue(Grid.ColumnSpanProperty, (object) 2);
      element.SetValue(Grid.RowSpanProperty, (object) 2);
      element.SetValue(System.Windows.Controls.Panel.ZIndexProperty, (object) 5);
      this.Container.Children.Add((UIElement) element);
    }

    public void RemoveNewUserGuide(NewUserGuide guide)
    {
      this.LeftTabBar.SetItems();
      this.Container.Children.Remove((UIElement) guide);
      WindowUtils.SetWindowChromeBorderThickness((System.Windows.Window) this, new Thickness(5.0, 5.0, 2.0, 5.0));
      this.ResizeMode = ResizeMode.CanResize;
      this.ResizeButtons.Visibility = Visibility.Visible;
    }

    private void ShowProExpired()
    {
      if (this._upgradeShowing)
        return;
      if (!this._lastShowUpdateTime.HasValue)
      {
        this._lastShowUpdateTime = new DateTime?(DateTime.Now);
      }
      else
      {
        if ((DateTime.Now - this._lastShowUpdateTime.Value).TotalSeconds < 10.0)
          return;
        this._lastShowUpdateTime = new DateTime?(DateTime.Now);
      }
      this._upgradeShowing = true;
      this.ShowProExpiredDialog();
      this._upgradeShowing = false;
    }

    private async void ShowProExpiredDialog()
    {
      List<ProjectModel> projects;
      if (UserDao.IsPro())
        projects = (List<ProjectModel>) null;
      else if (UserManager.IsTeamActive())
        projects = (List<ProjectModel>) null;
      else if (LocalSettings.Settings.DontShowProWindow)
      {
        projects = (List<ProjectModel>) null;
      }
      else
      {
        projects = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => !p.closed.HasValue || !p.closed.Value)).ToList<ProjectModel>();
        ListOverLimitsModel listOverLimitsModel1 = new ListOverLimitsModel();
        listOverLimitsModel1.ProjectNum = projects.Count;
        ListOverLimitsModel listOverLimitsModel2 = listOverLimitsModel1;
        listOverLimitsModel2.TaskNum = await ProjectDao.GetTaskOverLimitProjectNum();
        listOverLimitsModel1.ShareNum = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList())).ToList<ProjectModel>().Count;
        ListOverLimitsModel model = listOverLimitsModel1;
        listOverLimitsModel2 = (ListOverLimitsModel) null;
        listOverLimitsModel1 = (ListOverLimitsModel) null;
        new ProReminderWindow(model).Show();
        MainWindow._proDialogShowed = true;
        LocalSettings.Settings.LastProRemindeTime = DateTime.Today.ToString("yyyy MM dd");
        projects = (List<ProjectModel>) null;
      }
    }

    private void InitLock()
    {
      this._lockHandler = new System.Timers.Timer(5000.0);
      this._lockHandler.Elapsed += new ElapsedEventHandler(this.CheckLocked);
    }

    private async void CheckLocked(object sender, ElapsedEventArgs e)
    {
      AppLockModel model = await AppLockCache.GetModel();
      if (model == null)
        model = (AppLockModel) null;
      else if (!await AppLockCache.IsLockValid())
      {
        model = (AppLockModel) null;
      }
      else
      {
        int num = model.LockInterval * 1000 * 60;
        if ((long) IdleTimeFinder.GetIdleTime() < (long) num)
        {
          model = (AppLockModel) null;
        }
        else
        {
          App.Instance.LockAndHide();
          model = (AppLockModel) null;
        }
      }
    }

    public void ReleaseLock()
    {
      this.StopLock();
      this._lockHandler = (System.Timers.Timer) null;
    }

    public async void TryLock()
    {
      if (SettingDialog.SettingShow())
        return;
      if (this._lockHandler == null)
        this.InitLock();
      this._lockHandler.Stop();
      this._lockHandler.Start();
    }

    public void StopLock() => this._lockHandler?.Stop();

    public void SetLockedOption()
    {
      this.ToggleOptionItem(AppOption.AddListWidget, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.AddCalendarWidget, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.CloseCalendarWidget, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.AddMatrixWidget, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.CloseMatrixWidget, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.Lock, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.Unlock, Visibility.Visible);
      this.ToggleOptionItem(AppOption.Pin, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.Unpin, Visibility.Collapsed);
      this.ToggleSeparator(AppOptionSeparator.First, Visibility.Collapsed);
      this.ToggleSeparator(AppOptionSeparator.Second, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.StartFocus, Visibility.Collapsed);
    }

    public async void UnlockApp()
    {
      this.ToggleWidgetItems();
      this.ToggleOptionItem(AppOption.Unlock, Visibility.Collapsed);
      this.ToggleOptionItem(AppOption.Lock, Visibility.Visible);
      JumpHelper.InitJumpList();
      this.SetPomoOption(false);
      if (LocalSettings.Settings.MainWindowTopmost)
      {
        this.ToggleOptionItem(AppOption.Pin, Visibility.Collapsed);
        this.ToggleOptionItem(AppOption.Unpin, Visibility.Visible);
      }
      else
      {
        this.ToggleOptionItem(AppOption.Pin, Visibility.Visible);
        this.ToggleOptionItem(AppOption.Unpin, Visibility.Collapsed);
      }
      this.ToggleSeparator(AppOptionSeparator.First, LocalSettings.Settings.EnableFocus ? Visibility.Visible : Visibility.Collapsed);
      this.ToggleSeparator(AppOptionSeparator.Second, Visibility.Visible);
      AppLockModel model = await AppLockCache.GetModel();
      WindowManager.AppLockOrExit = false;
      if (model != null)
      {
        if (model.LockWidget)
        {
          CalendarWidgetHelper.TryLoadWidget();
          MatrixWidgetHelper.TryLoadWidget();
          ProjectWidgetsHelper.LoadWidgets();
          TaskStickyWindow.TryLoadSavedStickies();
        }
        IndependentWindow.Restore();
        if (model.LockAfter)
          this.TryLock();
      }
      App.Instance.SetNotifyIcon(false);
    }

    private void ToggleWidgetItems()
    {
      this.ToggleOptionItem(AppOption.AddListWidget, Visibility.Visible);
      if (CalendarWidgetHelper.CanAddProject())
      {
        this.ToggleOptionItem(AppOption.AddCalendarWidget, Visibility.Visible);
        this.ToggleOptionItem(AppOption.CloseCalendarWidget, Visibility.Collapsed);
      }
      else
      {
        this.ToggleOptionItem(AppOption.AddCalendarWidget, Visibility.Collapsed);
        this.ToggleOptionItem(AppOption.CloseCalendarWidget, Visibility.Visible);
      }
      if (MatrixWidgetHelper.CanAddWidget())
      {
        this.ToggleOptionItem(AppOption.AddMatrixWidget, Visibility.Visible);
        this.ToggleOptionItem(AppOption.CloseMatrixWidget, Visibility.Collapsed);
      }
      else
      {
        this.ToggleOptionItem(AppOption.AddMatrixWidget, Visibility.Collapsed);
        this.ToggleOptionItem(AppOption.CloseMatrixWidget, Visibility.Visible);
      }
    }

    public async Task TryFocus()
    {
      MainWindow element = this;
      await Task.Delay(10);
      Keyboard.ClearFocus();
      if (element._ignoreFocus)
        return;
      if (element.ShowCalendar && !LocalSettings.Settings.InSearch)
      {
        element.MainCalendar?.GetFocus();
      }
      else
      {
        if (!PopupStateManager.CanShowAddPopup())
        {
          await Task.Delay(300);
          if (!PopupStateManager.CanShowAddPopup())
            return;
        }
        if (!element.IsActive)
          return;
        FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
        Keyboard.Focus((IInputElement) element);
      }
    }

    public async Task TryShowMainWindow(bool force = false, bool delay = false, bool isStart = false)
    {
      MainWindow mainWindow = this;
      if (force)
        await mainWindow.ShowWindow(delay);
      else if (await AppLockCache.GetAppLocked(isStart))
        AppUnlockWindow.TryUnlockApp(new Action(mainWindow.ShowWindow));
      else
        mainWindow.ShowWindow();
    }

    public async Task UnlockAndNavigateTask(string taskId, string itemId)
    {
      if (await AppLockCache.GetAppLocked())
        AppUnlockWindow.TryUnlockApp(new Action(Navigate));
      else
        Navigate();

      void Navigate()
      {
        this.ShowWindow();
        this.NavigateTask(taskId, itemId);
      }
    }

    public async Task ShowUndoToast(UndoToast undoToast)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, (FrameworkElement) undoToast);
      this.TryFocus();
    }

    public void HandleNeedUpgradeClient()
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.ForceExit));
    }

    private void ForceExit()
    {
      if (MainWindow._versionOutdated)
        return;
      MainWindow._versionOutdated = true;
      int num = (int) System.Windows.MessageBox.Show(Utils.GetString("NeedUpgradeVersion"), Utils.GetString("UpdateNow"), MessageBoxButton.OK);
      App.Instance.ExitApplication();
    }

    public async Task CheckLoginStatus()
    {
      this.LeftTabBar.LoadUserInfo();
      await Task.Delay(2000);
      if (await TeamDao.IsTeamPro())
        this.UpgradeButton.Visibility = Visibility.Collapsed;
      else if (UserManager.IsFreeUser())
      {
        this.UpgradeButton.Visibility = Visibility.Visible;
      }
      else
      {
        switch (await UserManager.GetProExpireDays())
        {
          case 0:
          case 1:
          case 2:
          case 3:
          case 4:
          case 5:
          case 6:
          case 7:
            if (UserDao.IsPro())
            {
              this.UpgradeButton.Visibility = Visibility.Visible;
              goto label_10;
            }
            else
              break;
        }
        this.UpgradeButton.Visibility = Visibility.Collapsed;
      }
label_10:
      MainWindowManager.SetCalendarProStatus();
      MatrixWidgetHelper.CheckProEnable();
    }

    protected override async void OnActivated(EventArgs e)
    {
      MainWindow mainWindow = this;
      // ISSUE: reference to a compiler-generated method
      mainWindow.\u003C\u003En__0(e);
      MainWindow.CancelOperation();
      if (App.NotifyIcon != null)
        App.NotifyIcon.Visible = true;
      Utils.ToastWindow = (IToastShowWindow) mainWindow;
      if (mainWindow.WindowState == WindowState.Maximized)
        mainWindow.ListView?.TryShowThreeColumns();
      mainWindow.CheckUserStatus();
    }

    public async Task CheckUserStatus()
    {
      if (!(this.LeftTabBar.LoadTime < DateTime.Now.AddMinutes(-10.0)))
        return;
      await this.CheckLoginStatus();
      App.CheckLastActiveTime();
    }

    protected override void OnDeactivated(EventArgs e)
    {
      MainWindow.CancelOperation();
      base.OnDeactivated(e);
      LocalSettings.Settings.Save();
      this.CheckToLock();
    }

    private async void CheckToLock()
    {
      AppLockModel model = await AppLockCache.GetModel();
      if (model == null || !model.LockAfter)
        return;
      this.TryLock();
    }

    private static void CancelOperation() => App.IsProjectOrGroupDragging = false;

    protected override async void OnClosing(CancelEventArgs e)
    {
      MainWindow mainWindow = this;
      if (mainWindow.Visibility == Visibility.Visible)
      {
        mainWindow.Hide();
        e.Cancel = true;
        if (!await AppLockCache.IsMinLock())
          return;
        App.Instance.LockApp();
      }
      else
      {
        if (App.NotifyIcon == null)
          return;
        App.NotifyIcon.MouseClick -= new System.Windows.Forms.MouseEventHandler(mainWindow.OnAppIconClick);
      }
    }

    public void ShowWindow() => this.ShowWindow(false);

    private async Task ShowWindow(bool delay)
    {
      MainWindow mainWindow = this;
      try
      {
        bool needReload = !mainWindow.ShowInTaskbar;
        int num = LocalSettings.Settings.Maxmized ? 1 : (mainWindow.WindowState == WindowState.Maximized ? 1 : 0);
        WindowState state = LocalSettings.Settings.Maxmized ? WindowState.Maximized : mainWindow.WindowState;
        if (num != 0 && mainWindow.Visibility != Visibility.Visible)
        {
          mainWindow.WindowState = WindowState.Minimized;
          await Task.Delay(10);
        }
        mainWindow.Show();
        if (!mainWindow.ShowInTaskbar)
        {
          mainWindow.ShowInTaskbar = true;
          await Task.Delay(50);
        }
        mainWindow.Visibility = Visibility.Visible;
        mainWindow.Activate();
        if (state == WindowState.Maximized && !mainWindow._showed)
          await mainWindow.CheckTaskBarAutoHide();
        mainWindow.WindowState = state == WindowState.Minimized ? WindowState.Normal : state;
        mainWindow.DelaySetSize();
        mainWindow.TryFocus();
        if (!mainWindow.IsLoaded || mainWindow.LeftTabBar.SelectedModule != ((DisplayModule) LocalSettings.Settings.MainWindowDisplayModule).ToString())
          mainWindow.SwitchDisplayModule();
        if (MainWindowManager.IsReSignIn())
          mainWindow.OnWindowLoaded();
        if (!mainWindow._showed | needReload)
        {
          mainWindow._showed = true;
          mainWindow.ReloadView(true);
        }
        if (!delay)
          return;
        await Task.Delay(300);
        mainWindow.Activate();
      }
      catch (Exception ex)
      {
      }
    }

    private async Task CheckTaskBarAutoHide()
    {
      Screen screen = Screen.FromHandle(new WindowInteropHelper((System.Windows.Window) this).Handle);
      System.Drawing.Rectangle rectangle = screen.WorkingArea;
      System.Drawing.Size size1 = rectangle.Size;
      rectangle = screen.Bounds;
      System.Drawing.Size size2 = rectangle.Size;
      if (size1 != size2 || !screen.Primary)
        return;
      await Task.Delay(400);
    }

    public async void HideWindow(bool checkLock = true)
    {
      MainWindow mainWindow = this;
      mainWindow.Hide();
      await Task.Delay(1);
      mainWindow.ShowInTaskbar = false;
      DataChangedNotifier.NotifyMainWindowHidden();
      MemoryHelper.FlushMemory();
      if (!checkLock)
        return;
      if (!await AppLockCache.IsMinLock())
        return;
      App.Instance.LockApp();
    }

    private void OnAppIconClick(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      switch (e.Button)
      {
        case MouseButtons.Left:
          this.TryShowMainWindow();
          break;
        case MouseButtons.Right:
          this._notifyIconMenu.IsOpen = true;
          break;
      }
    }

    private void MaxButtonClick(object sender, RoutedEventArgs e) => this.ToggleWindowState();

    private void ToggleWindowState()
    {
      this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
      LocalSettings.Settings.Maxmized = this.WindowState == WindowState.Maximized;
      this.SafeSetWindowSize();
    }

    private void NormalButtonClick(object sender, RoutedEventArgs e) => this.ToggleWindowState();

    private void MinButton_Click(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Minimized;
    }

    private async void OnCloseButtonClick(object sender, RoutedEventArgs e) => this.HideWindow();

    private async void HeaderGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      MainWindow mainWindow = this;
      mainWindow._mouseDown = true;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) sender);
      mainWindow.ListView?.TryHideProjectMenu(position.X);
      if ((DateTime.Now - mainWindow._headerClickOldTime).TotalMilliseconds <= 300.0)
      {
        if (mainWindow.ResizeMode != ResizeMode.NoResize)
          mainWindow.ToggleWindowState();
      }
      else
      {
        if (mainWindow.WindowState == WindowState.Maximized)
          mainWindow._dragStartPoint = position;
        if (e.LeftButton == MouseButtonState.Pressed)
        {
          mainWindow.DragMove();
          e.Handled = false;
        }
      }
      mainWindow._headerClickOldTime = DateTime.Now;
    }

    public async void SyncButtonClick()
    {
      this.CheckLoginStatus();
      this.CheckProValid();
      AppConfigManager.PerformPullAppConfig();
      SyncManager.Sync(1);
    }

    private async void CheckProValid()
    {
      if (!UserManager.IsTeamUser() && CacheManager.GetTeams().Any<TeamModel>())
        this.ListView?.ReloadView();
      this.CheckVersionValid(false);
    }

    public async void OpenChangeLog()
    {
      Task.Run((Func<Task>) (async () =>
      {
        MainWindow mainWindow1 = this;
        MainWindow mainWindow = mainWindow1;
        ReleaseNoteModel releaseNote = await Utils.GetReleaseNote();
        string key = App.Ci.ToString() == "zh-CN" ? "zh_cn" : "en";
        NoteItemModel note = releaseNote != null ? releaseNote.data.FirstOrDefault<NoteItemModel>((Func<NoteItemModel, bool>) (item => item.lang == key)) : (NoteItemModel) null;
        UpdateModel updateModel = await UpdateHelper.CheckUpdate();
        if (note != null && updateModel != null)
        {
          string version = Utils.ToVersion(updateModel.versionNum);
          string message = note?.content;
          mainWindow1.Dispatcher.Invoke((Action) (() =>
          {
            new ChangeLogWindow(version, message)
            {
              Owner = ((System.Windows.Window) closure_3)
            }.Show();
          }));
          note = (NoteItemModel) null;
        }
        else
        {
          UtilLog.Info((note == null ? "note is null," : "") + (updateModel == null ? "updateModel is null" : ""));
          note = (NoteItemModel) null;
        }
      }));
    }

    public static async void ViewOnWeb()
    {
      string urlTemple = BaseUrl.GetDomainUrl() + "/sign/autoSignOn?token={0}";
      Utils.TryProcessStartUrl(string.Format(urlTemple, (object) await Communicator.GetSignOnToken()));
      urlTemple = (string) null;
    }

    public async void CheckUpdate()
    {
      MainWindow mainWindow = this;
      if (!Utils.IsNetworkAvailable())
      {
        Utils.Toast(Utils.GetString("NoNetwork"));
      }
      else
      {
        // ISSUE: reference to a compiler-generated method
        Task.Run(new Func<Task>(mainWindow.\u003CCheckUpdate\u003Eb__217_0));
      }
    }

    private async void ShowNewVersionWindow(UpdateModel update)
    {
      Task.Run((Func<Task>) (async () =>
      {
        ReleaseNoteModel releaseNote = await Utils.GetReleaseNote();
        string key = App.Ci.ToString() == "zh-CN" ? "zh_cn" : "en";
        NoteItemModel noteItemModel = releaseNote != null ? releaseNote.data.FirstOrDefault<NoteItemModel>((Func<NoteItemModel, bool>) (item => item.lang == key)) : (NoteItemModel) null;
        NewVersonViewModel model = new NewVersonViewModel()
        {
          OldVersion = System.Windows.Application.ResourceAssembly.GetName().Version.ToString(),
          NewVersion = Utils.ToVersion(update.versionNum),
          ReleaseNote = noteItemModel?.content,
          DownloadPath = update.downLoadUri,
          SizeModel = releaseNote?.size
        };
        this.Dispatcher.Invoke((Action) (() =>
        {
          App.CancelTopMost();
          new NewVersionWindow(model).Show();
          MainWindow._updateVersionShowed = true;
          App.RestoreTopMost();
        }));
      }));
    }

    private async void CheckUpdateStart()
    {
      MainWindow mainWindow = this;
      if (!Utils.IsNetworkAvailable())
        return;
      UpdateHelper.TryDeleteOriginPackage();
      if (ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Today) < LocalSettings.Settings.GrayVersionDate)
        return;
      // ISSUE: reference to a compiler-generated method
      Task.Run(new Func<Task>(mainWindow.\u003CCheckUpdateStart\u003Eb__219_0));
    }

    private async void BackDownLoad(string uri, string path)
    {
      MainWindow mainWindow = this;
      if (!Utils.IsNetworkAvailable())
        return;
      using (WebClient webClient = new WebClient())
      {
        int num;
        if (num != 0 && !Directory.Exists(AppPaths.PackageDir))
          Directory.CreateDirectory(AppPaths.PackageDir);
        try
        {
          webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(mainWindow.OnDownloadChanged);
          webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(mainWindow.OnDownloadCompleted);
          await webClient.DownloadFileTaskAsync(uri, path);
        }
        catch (Exception ex)
        {
        }
      }
    }

    private async void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      MainWindow mainWindow = this;
      bool appLocked = await AppLockCache.GetAppLocked();
      App.NotifyIcon.Text = Utils.GetAppName();
      App.NotifyIcon.BalloonTipTitle = Utils.GetString("DownloadCompleted");
      App.NotifyIcon.BalloonTipText = Utils.GetString("ClickToUpdate");
      App.NotifyIcon.BalloonTipClicked += new EventHandler(mainWindow.ShowUpdateWindow);
      App.NotifyIcon.ShowBalloonTip(20);
      App.Instance.SetNotifyIcon(appLocked);
      LeftMenuBar.CanUpdate = true;
    }

    private void ShowUpdateWindow(object sender, EventArgs e)
    {
    }

    private void OnDownloadChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      if (App.NotifyIcon == null)
        return;
      App.NotifyIcon.Text = string.Format(Utils.GetString("DownloadingPackage"), (object) e?.ProgressPercentage);
    }

    public async Task Logout()
    {
      if (!await App.CheckTwitterLogin(true))
        return;
      await this.ForceLogout();
    }

    public async Task ForceLogout()
    {
      MainWindow mainWindow = this;
      await SyncManager.Sync();
      if (!await LoginManager.Logout())
        return;
      mainWindow.Hide();
      new LoginDialog().Show();
    }

    public void OnSearch(SearchExtra searchExtra, bool restore)
    {
      if (!LocalSettings.Settings.InSearch)
        this._saveModule = LocalSettings.Settings.MainWindowDisplayModule;
      LocalSettings.Settings.InSearch = true;
      this.SwitchModule(DisplayModule.Task, true);
      this.ListView?.StartSearch(searchExtra, restore);
      this.TryFocus();
    }

    private void WindowLocationChanged(object sender, EventArgs e) => this.DelaySaveLocation();

    public async void SelectTagProject(string tag)
    {
      MainWindow mainWindow = this;
      await Task.Delay(120);
      mainWindow.SwitchModule(DisplayModule.Task);
      mainWindow.ListView?.SelectTagProject(tag.ToLower());
      // ISSUE: explicit non-virtual call
      __nonvirtual (mainWindow.ExitImmersiveMode());
      if (await AppLockCache.GetAppLocked())
        AppUnlockWindow.TryUnlockApp(new Action(mainWindow.ShowWindow));
      else
        mainWindow.ShowWindow();
    }

    public void StartSearch() => this.StartSearch(false);

    public void StartSearch(bool reopen)
    {
      if (this.SearchOperationDialog != null && this.SearchOperationDialog.Showing)
        this.SearchOperationDialog.Close();
      if (this.SearchDialog != null && this.SearchDialog.Showing)
        return;
      this.LeftTabBar.SelectedModule = "Search";
      SearchDialog searchDialog = new SearchDialog(reopen);
      searchDialog.Owner = (System.Windows.Window) this;
      this.SearchDialog = searchDialog;
      this.SearchDialog?.Show();
    }

    private void ShowSearchOperation()
    {
      if (this.SearchDialog != null && this.SearchDialog.Showing)
        this.SearchDialog.Close();
      if (this.SearchOperationDialog != null && this.SearchOperationDialog.Showing)
        return;
      SearchOperationDialog searchOperationDialog = new SearchOperationDialog();
      searchOperationDialog.Owner = (System.Windows.Window) this;
      this.SearchOperationDialog = searchOperationDialog;
      this.SearchOperationDialog?.Show();
    }

    private async void OnWindowKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      MainWindow element = this;
      if (element.CalendarBorder.Visibility == Visibility.Visible && !element.ImmersiveGrid.IsVisible)
        element.MainCalendar?.OnKeyUp(sender, e);
      IInputElement focusedElement = FocusManager.GetFocusedElement((DependencyObject) element);
      if (Utils.IfCtrlPressed() && e.Key == Key.Z)
      {
        element.TryUndo();
      }
      else
      {
        if (focusedElement == null || focusedElement.Equals((object) element) || focusedElement is ScrollViewer || Utils.FindParent<ProjectListView>(focusedElement as DependencyObject) != null)
        {
          element.DoOperationOnKeyUp(e.Key);
          element.ListView?.OnWindowKeyUp(e);
        }
        if (e.Key != Key.Escape)
          return;
        await Task.Delay(50);
        if (e.Handled || !element.MaskPanel.IsVisible)
          return;
        element.CloseShortCut();
      }
    }

    private void UpgradeProClick(object sender, RoutedEventArgs e) => Utils.StartUpgrade();

    public void HandleDateChanged()
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (async () =>
      {
        ProjectAndTaskIdsCache.ResetIds();
        TaskStickyWindow.ReloadStickies();
      }));
    }

    public void SyncVersionStatus()
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() => this.CheckVersionValid(false)));
    }

    public void HideBackground()
    {
      this.WindowBackground.Visibility = Visibility.Collapsed;
      MainWindowManager.BackImageSource = (ImageSource) null;
      IndependentWindow.SetBackGround((ImageBrush) null);
    }

    public void SetBackgroundImage(BitmapImage background, Rect viewBox)
    {
      MainWindowManager.BackImageSource = (ImageSource) background;
      this.WindowBackgroundImage.ImageSource = (ImageSource) background;
      this.WindowBackgroundImage.Viewbox = viewBox;
      IndependentWindow.SetBackGround(this.WindowBackgroundImage);
      if (this.WindowBackground.Visibility == Visibility.Visible)
        return;
      this.WindowBackground.Visibility = Visibility.Visible;
    }

    public void FinishTutorial()
    {
      this.TutorialContent.Content = (object) null;
      this.ListView?.GetTaskView()?.FocusQuickAdd();
    }

    private void ShowTutorial() => this.TutorialContent.Content = (object) new TutorialPopup();

    private async void OnWindowMouseUp(object sender, MouseButtonEventArgs e)
    {
      MainWindow element = this;
      if (element.MaskPanel.IsVisible || !element._mouseDown)
        return;
      element._mouseDown = false;
      if (!element.ShowCalendar || LocalSettings.Settings.InSearch)
      {
        if (element.ListView?.GetTaskView()?.DetailMouseOver().GetValueOrDefault() || element.Summary != null && !element.Summary.RemoveInputFocus() || Utils.GetMousePointVisibleItem<QuickAddView>((System.Windows.Input.MouseEventArgs) e, (FrameworkElement) element) != null || Utils.GetMousePointVisibleItem<System.Windows.Controls.TextBox>((System.Windows.Input.MouseEventArgs) e, (FrameworkElement) element) != null || element.MatrixContainer != null && element.MatrixContainer.IsVisible && (element.MatrixContainer.AddButtonMoveOver() || Utils.GetMousePointElement<TaskListItem>((System.Windows.Input.MouseEventArgs) e, (FrameworkElement) element.MatrixContainer) != null))
          return;
        await Task.Delay(100);
        if (!element.IsActive || e.Handled)
          return;
        element.TryFocus();
      }
      else
      {
        await Task.Delay(100);
        if (!element.IsActive || e.Handled)
          return;
        element.MainCalendar?.GetFocus();
        TaskOperationDialog.TryCloseLastOne();
      }
    }

    private void OnClickButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      this.X.Fill = (System.Windows.Media.Brush) System.Windows.Media.Brushes.White;
    }

    private void OnClickButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      this.X.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity60");
    }

    public async void EnterImmersiveMode(string taskId, int caretOffset = -1)
    {
      MainWindow mainWindow = this;
      mainWindow.TryShowMainWindow();
      mainWindow.MinWidth = 595.0;
      ImmersiveContent immersiveContent = new ImmersiveContent();
      immersiveContent.Margin = new Thickness(0.0, 24.0, 0.0, 12.0);
      immersiveContent.VerticalAlignment = VerticalAlignment.Stretch;
      ImmersiveContent editor = immersiveContent;
      mainWindow.ImmersiveGrid.Child = (UIElement) editor;
      mainWindow.ImmersiveGrid.Visibility = Visibility.Visible;
      await editor.LoadData(taskId);
      MarkDownEditor contentText = editor.TaskDetail.GetContentText();
      if (contentText == null)
        editor = (ImmersiveContent) null;
      else if (caretOffset < 0)
        editor = (ImmersiveContent) null;
      else if (caretOffset >= contentText.EditBox.Text.Length)
      {
        editor = (ImmersiveContent) null;
      }
      else
      {
        contentText.EditBox.CaretOffset = caretOffset;
        contentText.FocusEditBox();
        contentText.EditBox.ScrollToEnd();
        editor = (ImmersiveContent) null;
      }
    }

    public void ExitImmersiveMode()
    {
      if (this.ImmersiveGrid.Visibility != Visibility.Visible)
        return;
      this.MinWidth = 370.0;
      this.ImmersiveGrid.Visibility = Visibility.Collapsed;
      this.ImmersiveGrid.Child = (UIElement) null;
    }

    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      switch (msg)
      {
        case 36:
          if (Utils.IsWindows7() || this.WindowState == WindowState.Maximized && !this._enableHandleMinMax)
            return IntPtr.Zero;
          if (this.WindowState == WindowState.Maximized)
          {
            this._enableHandleMinMax = false;
            DelaySetEnableHandleMinMax();
          }
          else
            this._enableHandleMinMax = true;
          Screen screen = Screen.FromRectangle(new System.Drawing.Rectangle((int) this.Left, (int) this.Top, (int) this.Width, (int) this.Height));
          if (screen.WorkingArea.Size != screen.Bounds.Size || !screen.Primary)
          {
            SetInPrimary(false);
            return IntPtr.Zero;
          }
          if (Mouse.LeftButton == MouseButtonState.Pressed && this.WindowState == WindowState.Normal)
            return IntPtr.Zero;
          if (WindowSizing.WmGetMinMaxInfo(hwnd, lParam, this.MinWidth, this.MinHeight))
          {
            SetInPrimary(true);
            handled = true;
            return IntPtr.Zero;
          }
          SetInPrimary(false);
          break;
        case (int) byte.MaxValue:
          TouchpadContact[] input = TouchPadHelper.TouchpadHelper.ParseInput(lParam);
          if (input != null && ((IEnumerable<TouchpadContact>) input).Any<TouchpadContact>((Func<TouchpadContact, bool>) (c => c.ContactId == 1)))
          {
            this.OnDoubleFingerTouch();
            break;
          }
          break;
        case 526:
          this.OnMouseTilt(MainWindow.LOWORD(wParam));
          return (IntPtr) 1;
      }
      return IntPtr.Zero;

      void SetInPrimary(bool inPrimary)
      {
        this._inPrimaryScreen = inPrimary;
        if (!inPrimary && this.WindowState == WindowState.Maximized)
        {
          ((FrameworkElement) this.Content).Margin = new Thickness(7.0, 5.0, 7.0, 5.0);
        }
        else
        {
          if (!inPrimary)
            return;
          ((FrameworkElement) this.Content).Margin = new Thickness(0.0);
        }
      }

      async void DelaySetEnableHandleMinMax()
      {
        await Task.Delay(500);
        this._enableHandleMinMax = true;
      }
    }

    private static int HIWORD(IntPtr ptr) => ptr.ToInt32() >> 16 & (int) ushort.MaxValue;

    private static int LOWORD(IntPtr ptr) => ((int) ptr.ToInt64() >> 16) % 256;

    private void OnMouseTilt(int offset)
    {
      if (this.ShowCalendar)
      {
        if (!this.IsActive)
          return;
        this.MainCalendar?.OnTouchScroll(offset);
      }
      else
      {
        if (this.ListView == null || !this.ListView.IsVisible)
          return;
        this.ListView.OnScroll(offset);
      }
    }

    private void OnDoubleFingerTouch()
    {
      if (!this.ShowCalendar)
        return;
      this.MainCalendar?.OnDoubleFingerTouch();
    }

    private void StopDragMove(object sender, MouseButtonEventArgs e)
    {
      this._dragStartPoint = new System.Windows.Point();
    }

    private void OnDragMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this.WindowState != WindowState.Maximized || e.LeftButton != MouseButtonState.Pressed || !(this._dragStartPoint != new System.Windows.Point()))
        return;
      UIElement relativeTo = (UIElement) sender;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) relativeTo);
      if ((position - this._dragStartPoint).Length < 4.0)
        return;
      System.Windows.Point point = relativeTo.PointToScreen(new System.Windows.Point(0.0, 0.0));
      CompositionTarget compositionTarget = PresentationSource.FromVisual((Visual) relativeTo)?.CompositionTarget;
      if (compositionTarget != null)
        point = compositionTarget.TransformFromDevice.Transform(point);
      this.Top = point.Y;
      double num = 0.0;
      double db;
      if (!MathUtil.TryParseString2Double(LocalSettings.Settings.MainWindowWidth.ToString((IFormatProvider) CultureInfo.InvariantCulture), out db))
        db = 1020.0;
      if (position.X > db / 2.0)
        num = position.X - db / 2.0;
      this.Left = point.X + num;
      this.WindowState = WindowState.Normal;
      LocalSettings.Settings.Maxmized = false;
      this._dragStartPoint = new System.Windows.Point();
      try
      {
        this.DragMove();
      }
      catch (Exception ex)
      {
      }
      e.Handled = true;
    }

    public void HandleOnNeedPro()
    {
    }

    public void TryReloadCalendar()
    {
      if (this.CalendarBorder.Visibility != Visibility.Visible)
        return;
      this.MainCalendar?.Reload(reloadArrange: false);
    }

    public bool IsProjectMenuVisible() => this.MenuShow && !this.MenuAutoHide;

    private async void OnStateChanged(object sender, EventArgs e)
    {
      MainWindow mainWindow = this;
      if (mainWindow.WindowState == WindowState.Maximized && !mainWindow._inPrimaryScreen)
      {
        // ISSUE: explicit non-virtual call
        ((FrameworkElement) __nonvirtual (mainWindow.Content)).Margin = new Thickness(7.0, 5.0, 7.0, 5.0);
      }
      else
      {
        // ISSUE: explicit non-virtual call
        ((FrameworkElement) __nonvirtual (mainWindow.Content)).Margin = new Thickness(0.0);
      }
      if (mainWindow.WindowState != WindowState.Normal)
        return;
      mainWindow.SafeSetWindowSize();
    }

    public void ToastMoveProjectControl(string projectId, string taskName = null, MoveToastType moveType = MoveToastType.Move)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, (FrameworkElement) new MoveToastControl(projectId, (INavigateProject) this, taskName, moveType));
    }

    public async void TryToastMoveControl(TaskModel task, bool isToday, bool hideTitle = false)
    {
      MainWindow navigate = this;
      if (task == null || !string.IsNullOrEmpty(task.parentId) && navigate.ListView?.GetTaskView()?.CheckTaskInList(task.parentId).GetValueOrDefault() || MoveToastHelper.CheckTaskMatched(navigate.ListView?.GetSelectedProject(), task))
        return;
      WindowToastHelper.ShowAndHideToast(navigate.ToastGrid, (FrameworkElement) new MoveToastControl(isToday, (INavigateProject) navigate, hideTitle ? string.Empty : task.title));
    }

    public void NavigateProjectById(string projectId)
    {
      this.TryShowMainWindow();
      this.NavigateNormalProject(projectId);
    }

    public void SwitchModule(DisplayModule module, bool force = false)
    {
      if ((DisplayModule) LocalSettings.Settings.MainWindowDisplayModule != module | force)
      {
        this._previousModule = (DisplayModule) LocalSettings.Settings.MainWindowDisplayModule;
        LocalSettings.Settings.MainWindowDisplayModule = (int) module;
        if (this._previousModule != module)
          this.ClearPreviousViewData();
        this.SwitchDisplayModule();
        ThemeUtil.TryClearImageCached(true);
      }
      else
      {
        if (module != DisplayModule.Pomo)
          return;
        TimerSyncService.PullRemoteTimers();
      }
    }

    private async void ClearPreviousViewData()
    {
      switch (this._previousModule)
      {
        case DisplayModule.Task:
          this.ListBorder.Child = (UIElement) null;
          ListViewContainer.RemoveListView(nameof (MainWindow));
          this.ListView = (ListViewContainer) null;
          break;
        case DisplayModule.Calendar:
          this._calendarDate = this.MainCalendar?.HeadView?.StartDate;
          if (!CalendarWindow.IsShowing)
          {
            CalendarControl mainCalendar = this.MainCalendar;
            if (mainCalendar != null)
            {
              mainCalendar.ClearData();
              break;
            }
            break;
          }
          break;
        case DisplayModule.Matrix:
          this.MatrixContainer = (MatrixContainer) null;
          this.EisenhoverBorder.Child = (UIElement) null;
          if (MatrixWindow.IsShowing)
            break;
          break;
        case DisplayModule.Habit:
          this.HabitBorder.Child = (UIElement) null;
          this.HabitView = (HabitContainer) null;
          break;
        case DisplayModule.Pomo:
          this.FocusBorder.Child = (UIElement) null;
          TickFocusManager.SetFocusInitPanel();
          TickFocusManager.MainFocus = (FocusView) null;
          break;
      }
      await Task.Delay(2000);
      GC.Collect();
    }

    public void ResetLeftSelected()
    {
      this.LeftTabBar.SelectedModule = ((DisplayModule) LocalSettings.Settings.MainWindowDisplayModule).ToString();
    }

    public void SetCustomTheme(BitmapImage image, Rect rect)
    {
      if (image != null)
      {
        this.WindowBackground.Visibility = Visibility.Visible;
        this.WindowBackgroundImage.ImageSource = (ImageSource) image;
      }
      this.WindowBackgroundImage.Viewbox = rect;
    }

    public void SwitchPreviousModule() => this.SwitchModule(this._previousModule);

    public async void BeginCoach(UIElement ui)
    {
      this.CoachMarks.Child = ui;
      this.CoachMarks.IsHitTestVisible = false;
      this.CoachMarks.Visibility = Visibility.Hidden;
      await Task.Delay(500);
      this.CoachMarks.IsHitTestVisible = true;
      this.CoachMarks.Visibility = Visibility.Visible;
    }

    public void EndCoach()
    {
      this.CoachMarks.Child = (UIElement) null;
      this.CoachMarks.Visibility = Visibility.Collapsed;
    }

    public async Task ShowShortCutPanel()
    {
      UserActCollectUtils.AddShortCutEvent("general", "shortcuts");
      await SettingsHelper.PullRemoteSettings();
      Border border1 = new Border();
      ShortcutsConfig shortcutsConfig = new ShortcutsConfig(false);
      shortcutsConfig.Width = this.ShortCutMask.ActualWidth <= 72.0 || this.ShortCutMask.ActualWidth >= 512.0 ? 440.0 : this.ShortCutMask.ActualWidth - 72.0;
      border1.Child = (UIElement) shortcutsConfig;
      border1.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
      border1.Margin = new Thickness(36.0);
      border1.CornerRadius = new CornerRadius(10.0);
      Border border2 = border1;
      border2.SetResourceReference(System.Windows.Controls.Control.BackgroundProperty, (object) "PopupBackground");
      this.ShortCutMask.Child = (UIElement) border2;
      this.MaskPanel.Visibility = Visibility.Visible;
    }

    private void CloseShortCut(object sender, MouseButtonEventArgs e)
    {
      UIElement child = this.ShortCutMask.Child;
      // ISSUE: explicit non-virtual call
      if ((child != null ? (__nonvirtual (child.IsMouseOver) ? 1 : 0) : 0) != 0)
        return;
      this.CloseShortCut();
    }

    private async Task CloseShortCut()
    {
      this.ShortCutMask.Child = (UIElement) null;
      this.MaskPanel.Visibility = Visibility.Collapsed;
      if (SettingsHelper.ShortCutChanged)
      {
        await SettingsHelper.PushLocalSettings();
        SettingsHelper.ShortCutChanged = false;
      }
      this.TryFocus();
    }

    public void ClearListView()
    {
      this.ListBorder.Child = (UIElement) null;
      this.ListView = (ListViewContainer) null;
      ListViewContainer.RemoveListView(nameof (MainWindow));
      this.LeftTabBar.SelectedModule = (string) null;
    }

    public void ClearChildren() => this.ClearListView();

    public void StopSearch(bool forceSwitch = true)
    {
      LocalSettings.Settings.InSearch = false;
      if (this._saveModule >= 0)
      {
        this.SwitchModule((DisplayModule) this._saveModule, true);
        this._saveModule = -1;
      }
      else
      {
        if (!forceSwitch)
          return;
        this.SwitchDisplayModule();
      }
    }

    private async void OnWindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key != Key.Escape || !LocalSettings.Settings.InSearch)
        return;
      await Task.Delay(50);
      if (e.Handled || this.ListView?.GetTaskView()?.TaskDetailFocus().GetValueOrDefault())
        return;
      this.StopSearch();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/mainwindow.xaml", UriKind.Relative));
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
          this.Window = (MainWindow) target;
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.WindowBackground = (Border) target;
          break;
        case 4:
          this.WindowBackgroundImage = (ImageBrush) target;
          break;
        case 5:
          this.CalendarBorder = (Border) target;
          break;
        case 6:
          this.FocusBorder = (Border) target;
          break;
        case 7:
          this.HabitBorder = (Border) target;
          break;
        case 8:
          this.EisenhoverBorder = (Border) target;
          break;
        case 9:
          this.ListBorder = (Border) target;
          break;
        case 10:
          this.LeftTabBar = (LeftMenuBar) target;
          break;
        case 11:
          this.TutorialContent = (ContentControl) target;
          break;
        case 12:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.HeaderGrid_MouseLeftButtonDown);
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.StopDragMove);
          ((UIElement) target).MouseMove += new System.Windows.Input.MouseEventHandler(this.OnDragMove);
          break;
        case 13:
          this.ResizeButtons = (StackPanel) target;
          break;
        case 14:
          this.UpgradeButton = (System.Windows.Controls.Button) target;
          this.UpgradeButton.Click += new RoutedEventHandler(this.UpgradeProClick);
          break;
        case 15:
          this.MinButton = (System.Windows.Controls.Button) target;
          this.MinButton.Click += new RoutedEventHandler(this.MinButton_Click);
          break;
        case 16:
          this.MaxButton = (System.Windows.Controls.Button) target;
          this.MaxButton.Click += new RoutedEventHandler(this.MaxButtonClick);
          break;
        case 17:
          this.NormalButton = (System.Windows.Controls.Button) target;
          this.NormalButton.Click += new RoutedEventHandler(this.NormalButtonClick);
          break;
        case 18:
          this.CloseButton = (System.Windows.Controls.Button) target;
          this.CloseButton.Click += new RoutedEventHandler(this.OnCloseButtonClick);
          this.CloseButton.MouseEnter += new System.Windows.Input.MouseEventHandler(this.OnClickButtonMouseEnter);
          this.CloseButton.MouseLeave += new System.Windows.Input.MouseEventHandler(this.OnClickButtonMouseLeave);
          break;
        case 19:
          this.X = (Polygon) target;
          break;
        case 20:
          this.ImmersiveGrid = (Border) target;
          break;
        case 21:
          this.CoachMarks = (Border) target;
          break;
        case 22:
          this.MaskPanel = (Grid) target;
          this.MaskPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.CloseShortCut);
          break;
        case 23:
          this.ShortCutMask = (Border) target;
          break;
        case 24:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.CloseShortCut);
          break;
        case 25:
          this.ToastGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
