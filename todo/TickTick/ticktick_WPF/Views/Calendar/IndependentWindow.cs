// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.IndependentWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Undo;
using ticktick_WPF.Views.Widget;
using TickTickDao;
using TickTickModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class IndependentWindow : MyWindow, IToastShowWindow, IKeyBinding, IComponentConnector
  {
    private DateTime _headerClickOldTime;
    protected bool _mouseDown;
    private bool _canDragToNormal;
    private static ImageBrush _backBrush;
    protected string Id;
    private DateTime _lastActiveTime;
    private Dictionary<string, KeyBinding> _keyBindings = new Dictionary<string, KeyBinding>();
    private bool _enableHandleMinMax;
    private bool _inPrimaryScreen;
    protected double InitLeft;
    protected double InitTop;
    internal IndependentWindow Window;
    internal Border WindowBackground;
    internal Grid Container;
    internal ColumnDefinition Column1;
    internal ColumnDefinition Column2;
    internal System.Windows.Controls.Button MinButton;
    internal System.Windows.Controls.Button MaxButton;
    internal System.Windows.Controls.Button NormalButton;
    internal System.Windows.Controls.Button CloseButton;
    internal Polygon X;
    internal Grid ToastGrid;
    private bool _contentLoaded;

    public IndependentWindow(WindowModel windowModel)
    {
      this.InitializeComponent();
      Utils.SetWindowChrome((System.Windows.Window) this, new Thickness(5.0, 5.0, 3.0, 5.0));
      this.Width = windowModel.Width;
      this.Height = windowModel.Height;
      this.InitLeft = windowModel.Left;
      this.InitTop = windowModel.Top;
      this.Left = windowModel.Left;
      this.Top = windowModel.Top;
      this.SetBackBrush();
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      this.LocationChanged += new EventHandler(this.OnLocationChanged);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.KeyUp += new System.Windows.Input.KeyEventHandler(this.OnWindowKeyUp);
      SystemEvents.SessionSwitch += new SessionSwitchEventHandler(this.OnSessionSwitch);
      this.Deactivated += new EventHandler(this.OnDeactivated);
      this._lastActiveTime = DateTime.Now;
    }

    private void OnDeactivated(object sender, EventArgs e) => this._lastActiveTime = DateTime.Now;

    private async void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
      await Task.Delay(3000);
      this.RefreshUi();
    }

    private void RefreshUi()
    {
      this.Width += 2.0;
      this.InvalidateVisual();
      this.Width -= 2.0;
      this.UpdateLayout();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
      base.OnSourceInitialized(e);
      ((HwndSource) PresentationSource.FromVisual((Visual) this))?.AddHook(new HwndSourceHook(this.HookProc));
    }

    private IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == 36 && !Utils.IsWindows7() && (this.WindowState != WindowState.Maximized || this._enableHandleMinMax))
      {
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

    private async void OnStateChanged(object sender, EventArgs e)
    {
      IndependentWindow independentWindow = this;
      if (independentWindow.WindowState == WindowState.Maximized && !independentWindow._inPrimaryScreen)
      {
        // ISSUE: explicit non-virtual call
        ((FrameworkElement) __nonvirtual (independentWindow.Content)).Margin = new Thickness(7.0, 5.0, 7.0, 5.0);
      }
      else
      {
        // ISSUE: explicit non-virtual call
        ((FrameworkElement) __nonvirtual (independentWindow.Content)).Margin = new Thickness(0.0);
      }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.InitShortcut();
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      this.ResetPosition();
    }

    private async void ResetPosition()
    {
      IndependentWindow independentWindow = this;
      WindowHelper.MoveTo((System.Windows.Window) independentWindow, (int) independentWindow.InitLeft, (int) independentWindow.InitTop);
      independentWindow.Left = independentWindow.InitLeft;
      independentWindow.Top = independentWindow.InitTop;
      Matrix? transform = PresentationSource.FromVisual((Visual) independentWindow)?.CompositionTarget?.TransformFromDevice;
      await Task.Delay(1000);
      System.Windows.Point defaultPoint = new System.Windows.Point(100.0, 100.0);
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      System.Windows.Point pomoLocationSafely = WidgetLocationHelper.GetPomoLocationSafely(independentWindow.Left, independentWindow.Top, __nonvirtual (independentWindow.Width), __nonvirtual (independentWindow.Height), transform, defaultPoint);
      independentWindow.Left = pomoLocationSafely.X;
      independentWindow.Top = pomoLocationSafely.Y;
    }

    protected override async void OnActivated(EventArgs e)
    {
      IndependentWindow independentWindow = this;
      if ((DateTime.Now - independentWindow._lastActiveTime).TotalMinutes > 10.0)
        independentWindow.RefreshUi();
      independentWindow._lastActiveTime = DateTime.Now;
      Utils.ToastWindow = (IToastShowWindow) independentWindow;
      // ISSUE: reference to a compiler-generated method
      independentWindow.\u003C\u003En__0(e);
    }

    private void OnLocationChanged(object sender, EventArgs e)
    {
      DelayActionHandlerCenter.TryDoAction(this.Id + "WindowLocationChange", (EventHandler) (async (o, args) =>
      {
        IndependentWindow independentWindow = this;
        // ISSUE: reference to a compiler-generated method
        independentWindow.Dispatcher.Invoke<Task>(new Func<Task>(independentWindow.\u003COnLocationChanged\u003Eb__16_1));
      }));
    }

    private async Task SaveLocation()
    {
      IndependentWindow independentWindow = this;
      if (independentWindow.WindowState != WindowState.Normal)
        return;
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      WindowModel windowModel = await WindowModelDao.SaveWindowLocation(independentWindow.Id, independentWindow.Left, independentWindow.Top, __nonvirtual (independentWindow.Width), __nonvirtual (independentWindow.Height));
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      DelayActionHandlerCenter.TryDoAction(this.Id + "WindowLocationChange", (EventHandler) ((o, args) => this.Dispatcher.Invoke<Task>((Func<Task>) (async () => this.SaveLocation()))));
      this.OnSizeChanged(e);
    }

    protected virtual void OnSizeChanged(SizeChangedEventArgs e)
    {
    }

    protected static async Task<WindowModel> GetNewWindowModel(
      string id,
      bool isProject = false,
      string data = null,
      System.Windows.Window win = null)
    {
      WindowModel windowModel = new WindowModel();
      windowModel.UserId = LocalSettings.Settings.LoginUserId;
      windowModel.Id = id;
      windowModel.Width = isProject ? 340.0 : MainWindowManager.Window.Width;
      windowModel.Height = isProject ? 550.0 : MainWindowManager.Window.Height;
      double? nullable1;
      if (!isProject)
      {
        nullable1 = new double?();
      }
      else
      {
        System.Windows.Window window = win;
        nullable1 = window != null ? new double?(window.GetActualLeft()) : new double?();
      }
      double? nullable2 = nullable1;
      windowModel.Left = (nullable2 ?? MainWindowManager.Window.GetActualLeft()) + 50.0;
      double? nullable3;
      if (!isProject)
      {
        nullable3 = new double?();
      }
      else
      {
        System.Windows.Window window = win;
        nullable3 = window != null ? new double?(window.GetActualTop()) : new double?();
      }
      nullable2 = nullable3;
      windowModel.Top = (nullable2 ?? MainWindowManager.Window.GetActualTop()) + 50.0;
      windowModel.Type = "independent";
      windowModel.Data = data;
      WindowModel model = windowModel;
      int num = await BaseDao<WindowModel>.InsertAsync(model);
      WindowModel newWindowModel = model;
      model = (WindowModel) null;
      return newWindowModel;
    }

    protected virtual void OnClosing()
    {
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.RemoveShortcut();
      this.OnClosing();
      if (!WindowManager.AppLockOrExit)
        WindowModelDao.CloseAsync(this.Id);
      if (this.Equals((object) Utils.ToastWindow))
        Utils.ToastWindow = (IToastShowWindow) null;
      base.OnClosing(e);
    }

    protected static void AddShowEvent(string data)
    {
      UserActCollectUtils.AddClickEvent("project_list_ui", "open_in_new_window", data);
    }

    private void HeaderGridButtonDown(object sender, MouseButtonEventArgs e)
    {
      this._mouseDown = true;
      if ((DateTime.Now - this._headerClickOldTime).TotalMilliseconds <= 300.0)
      {
        this.ToggleWindowState();
      }
      else
      {
        if (this.WindowState == WindowState.Maximized)
          this._canDragToNormal = true;
        if (e.LeftButton == MouseButtonState.Pressed)
        {
          this.DragMove();
          e.Handled = false;
        }
      }
      this._headerClickOldTime = DateTime.Now;
    }

    private void ToggleWindowState()
    {
      this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void StopDragMove(object sender, MouseButtonEventArgs e)
    {
      this._canDragToNormal = false;
    }

    private void OnDragMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this.WindowState != WindowState.Maximized || e.LeftButton != MouseButtonState.Pressed || !this._canDragToNormal)
        return;
      UIElement relativeTo = (UIElement) sender;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) relativeTo);
      System.Windows.Point point = relativeTo.PointToScreen(new System.Windows.Point(0.0, 0.0));
      CompositionTarget compositionTarget = PresentationSource.FromVisual((Visual) relativeTo)?.CompositionTarget;
      if (compositionTarget != null)
        point = compositionTarget.TransformFromDevice.Transform(point);
      this.Top = point.Y;
      double num = 0.0;
      double width = this.Width;
      if (position.X > width / 2.0)
        num = position.X - width / 2.0;
      this.Left = point.X + num;
      this.WindowState = WindowState.Normal;
      LocalSettings.Settings.Maxmized = false;
      this._canDragToNormal = false;
      try
      {
        this.DragMove();
      }
      catch (Exception ex)
      {
      }
      e.Handled = true;
    }

    private void MinButton_Click(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Minimized;
    }

    private void MaxButtonClick(object sender, RoutedEventArgs e) => this.ToggleWindowState();

    private void NormalButtonClick(object sender, RoutedEventArgs e) => this.ToggleWindowState();

    private void OnCloseButtonClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnClickButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      this.X.Fill = (System.Windows.Media.Brush) System.Windows.Media.Brushes.White;
    }

    private void OnClickButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      this.X.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity60");
    }

    public static void SetBackGround(ImageBrush brush)
    {
      IndependentWindow._backBrush = brush;
      CalendarWindow.Window?.SetBackBrush();
      MatrixWindow.Window?.SetBackBrush();
      HabitWindow.Window?.SetBackBrush();
      TaskWindow.Window?.SetBackBrush();
      FocusWindow.Window?.SetBackBrush();
      TaskListWindow.SetWindowsBackBrush();
    }

    public void SetBackBrush()
    {
      this.WindowBackground.Background = (System.Windows.Media.Brush) IndependentWindow._backBrush;
      this.WindowBackground.Visibility = IndependentWindow._backBrush == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public static async Task Restore()
    {
      System.Windows.Application.Current?.Dispatcher.Invoke((Action) (() =>
      {
        CalendarWindow.ShowWindow(false);
        MatrixWindow.ShowWindow(false);
        HabitWindow.ShowWindow(false);
        TaskWindow.ShowWindow(false);
        FocusWindow.ShowWindow(false);
        TaskListWindow.ShowSavedWindows();
      }));
    }

    public static void CloseAll()
    {
      CalendarWindow.Window?.Close();
      MatrixWindow.Window?.Close();
      HabitWindow.Window?.Close();
      TaskWindow.Window?.Close();
      FocusWindow.Window?.Close();
      TaskListWindow.CloseAllWindow();
    }

    public async void TaskDeleted(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task != null)
      {
        task.deleted = 1;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnDeletedChanged(task);
        UndoToast uiElement = new UndoToast();
        uiElement.InitTaskUndo(taskId, task.title);
        this.Toast((FrameworkElement) uiElement);
      }
      this.TryFocus();
      task = (TaskModel) null;
    }

    public void TryToastString(object sender, string e)
    {
      WindowToastHelper.ToastString(this.ToastGrid, e);
    }

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks)
    {
      TaskService.BatchDeleteTasks(tasks, undoGrid: this.ToastGrid);
      this.TryFocus();
      return true;
    }

    public void TaskComplete(CloseUndoToast undo)
    {
      this.Toast((FrameworkElement) undo);
      this.TryFocus();
    }

    private async void TryFocus()
    {
      IndependentWindow element = this;
      if (!PopupStateManager.CanShowAddPopup())
      {
        await Task.Delay(200);
        if (!PopupStateManager.CanShowAddPopup())
          return;
      }
      if (!element.IsActive)
        return;
      FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
      Keyboard.Focus((IInputElement) element);
    }

    public void TryHideToast()
    {
      if (this.ToastGrid.Children.Count <= 0)
        return;
      if (this.ToastGrid.Children[0] is UndoToast child)
        child.OnFinished();
      this.ToastGrid.Children[0].Visibility = Visibility.Collapsed;
    }

    public void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels)
    {
      if (undoModels == null || !undoModels.Any<TaskDeleteRecurrenceUndoEntity>())
        return;
      UndoToast undoToast = new UndoToast();
      undoToast.InitTaskUndo(undoModels[0]);
      this.ShowUndoToast(undoToast);
    }

    public async Task ShowUndoToast(UndoToast undoToast)
    {
      this.Toast((FrameworkElement) undoToast);
      this.TryFocus();
    }

    public void ToastMoveProjectControl(string projectId, string taskName = null, MoveToastType moveType = MoveToastType.Move)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, (FrameworkElement) new MoveToastControl(projectId, (INavigateProject) App.Window, taskName, moveType));
    }

    public void Toast(FrameworkElement uiElement)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, uiElement);
    }

    private void RemoveShortcut()
    {
      foreach (KeyValuePair<string, KeyBinding> keyBinding in this._keyBindings)
        KeyBindingManager.RemoveKeyBinding(keyBinding.Key, keyBinding.Value);
    }

    private void InitShortcut()
    {
      this._keyBindings.Add("SyncTask", GetKeyBinding(IndependentWindowCommand.SyncCommand));
      this._keyBindings.Add("AddTask", GetKeyBinding(IndependentWindowCommand.InputCommand));
      this._keyBindings.Add("Print", GetKeyBinding(IndependentWindowCommand.PrintCommand));
      this._keyBindings.Add("SetNoPriority", GetKeyBinding(IndependentWindowCommand.SetPriorityNoneCommand));
      this._keyBindings.Add("SetLowPriority", GetKeyBinding(IndependentWindowCommand.SetPriorityLowCommand));
      this._keyBindings.Add("SetMediumPriority", GetKeyBinding(IndependentWindowCommand.SetPriorityMediumCommand));
      this._keyBindings.Add("SetHighPriority", GetKeyBinding(IndependentWindowCommand.SetPriorityHighCommand));
      this._keyBindings.Add("ClearDate", GetKeyBinding(IndependentWindowCommand.ClearDateCommand));
      this._keyBindings.Add("SetToday", GetKeyBinding(IndependentWindowCommand.SetTodayCommand));
      this._keyBindings.Add("SetTomorrow", GetKeyBinding(IndependentWindowCommand.SetTomorrowCommand));
      this._keyBindings.Add("SetNextWeek", GetKeyBinding(IndependentWindowCommand.SetNextWeekCommand));
      this._keyBindings.Add("DeleteTask", GetKeyBinding(IndependentWindowCommand.DeleteCommand));
      this._keyBindings.Add("PinTask", GetKeyBinding(IndependentWindowCommand.PinTaskCommand));
      this._keyBindings.Add("OpenSticky", GetKeyBinding(IndependentWindowCommand.OpenStickyCommand));
      this._keyBindings.Add("OmListView", GetKeyBinding(IndependentWindowCommand.ListViewCommand));
      this._keyBindings.Add("OmKanbanView", GetKeyBinding(IndependentWindowCommand.KanbanViewCommand));
      this._keyBindings.Add("OmTimelineView", GetKeyBinding(IndependentWindowCommand.TimelineViewCommand));
      foreach (KeyValuePair<string, KeyBinding> keyBinding in this._keyBindings)
        KeyBindingManager.TryAddKeyBinding(keyBinding.Key, keyBinding.Value);

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

    public virtual void Print()
    {
    }

    public virtual void BatchSetPriorityCommand(int priority)
    {
    }

    public virtual void BatchSetDateCommand(DateTime? date)
    {
    }

    public virtual void BatchPinTaskCommand()
    {
    }

    public virtual void BatchOpenStickyCommand()
    {
    }

    public virtual void BatchDeleteCommand()
    {
    }

    private void OnWindowKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (!Utils.IfCtrlPressed() || e.Key != Key.Z)
        return;
      this.TryUndo();
    }

    public void TryUndo()
    {
      if (this.ToastGrid.Children.Count <= 0 || !(this.ToastGrid.Children[0] is UndoToast child) || child.Visibility != Visibility.Visible)
        return;
      child.OnUndo();
    }

    public static bool CheckCount(System.Windows.Window owner)
    {
      int num = 0;
      if (MatrixWindow.IsShowing)
        ++num;
      if (FocusWindow.IsShowing)
        ++num;
      if (TaskWindow.IsShowing)
        ++num;
      if (CalendarWindow.IsShowing)
        ++num;
      if (HabitWindow.IsShowing)
        ++num;
      if (num + TaskListWindow.Windows.Count < 5)
        return true;
      if (owner is IToastShowWindow toastShowWindow)
        toastShowWindow.TryToastString((object) null, Utils.GetString("IndependentWindowLimit"));
      return false;
    }

    public virtual void SwitchViewMode(string mode)
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/independentwindow.xaml", UriKind.Relative));
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
          this.Window = (IndependentWindow) target;
          break;
        case 2:
          this.WindowBackground = (Border) target;
          break;
        case 3:
          this.Container = (Grid) target;
          break;
        case 4:
          this.Column1 = (ColumnDefinition) target;
          break;
        case 5:
          this.Column2 = (ColumnDefinition) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.HeaderGridButtonDown);
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.StopDragMove);
          ((UIElement) target).MouseMove += new System.Windows.Input.MouseEventHandler(this.OnDragMove);
          break;
        case 7:
          this.MinButton = (System.Windows.Controls.Button) target;
          this.MinButton.Click += new RoutedEventHandler(this.MinButton_Click);
          break;
        case 8:
          this.MaxButton = (System.Windows.Controls.Button) target;
          this.MaxButton.Click += new RoutedEventHandler(this.MaxButtonClick);
          break;
        case 9:
          this.NormalButton = (System.Windows.Controls.Button) target;
          this.NormalButton.Click += new RoutedEventHandler(this.NormalButtonClick);
          break;
        case 10:
          this.CloseButton = (System.Windows.Controls.Button) target;
          this.CloseButton.Click += new RoutedEventHandler(this.OnCloseButtonClick);
          this.CloseButton.MouseEnter += new System.Windows.Input.MouseEventHandler(this.OnClickButtonMouseEnter);
          this.CloseButton.MouseLeave += new System.Windows.Input.MouseEventHandler(this.OnClickButtonMouseLeave);
          break;
        case 11:
          this.X = (Polygon) target;
          break;
        case 12:
          this.ToastGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
