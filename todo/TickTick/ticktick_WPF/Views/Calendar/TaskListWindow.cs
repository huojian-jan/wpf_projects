// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TaskListWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.QuickAdd;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TaskListWindow : IndependentWindow, IListViewParent
  {
    public static ConcurrentDictionary<string, TaskListWindow> Windows = new ConcurrentDictionary<string, TaskListWindow>();
    private TaskView _taskView;
    private Border _immersiveBorder;
    private Border _containBorder;

    private TaskListWindow(WindowModel windowModel)
      : base(windowModel)
    {
      this.Id = windowModel.Id;
      this._immersiveBorder = new Border();
      this.Container.Children.Add((UIElement) this._immersiveBorder);
      this._containBorder = new Border();
      this.Container.Children.Add((UIElement) this._containBorder);
      this._taskView = new TaskView(false);
      this.MinWidth = 340.0;
      this.MinHeight = 400.0;
      this._containBorder.Child = (UIElement) this._taskView;
      this.Loaded += (RoutedEventHandler) (async (o, e) =>
      {
        TaskListWindow taskListWindow = this;
        taskListWindow._taskView.SetColumns();
        ProjectIdentity identity = ProjectIdentity.BuildProject(taskListWindow.Id, false);
        taskListWindow._taskView.OnProjectSelect(identity);
        ((HwndSource) PresentationSource.FromVisual((Visual) taskListWindow))?.AddHook(new HwndSourceHook(taskListWindow.Hook));
        // ISSUE: explicit non-virtual call
        __nonvirtual (taskListWindow.Title) = identity?.GetDisplayTitle();
      });
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnWindowMouseUp);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnWindowMouseDown);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnPreviewMouseDown);
      DataChangedNotifier.ViewModeChanged += new EventHandler<ProjectIdentity>(this.OnProjectViewModeChanged);
      DataChangedNotifier.ProjectChanged += new EventHandler(this.OnIdentityChanged);
      DataChangedNotifier.ProjectGroupChanged += new EventHandler<ProjectGroupModel>(this.OnIdentityChanged);
      DataChangedNotifier.TagChanged += new EventHandler<TagModel>(this.OnIdentityChanged);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(this.OnIdentityChanged);
    }

    private void OnIdentityChanged(object sender, object e)
    {
      ProjectIdentity projectIdentity = ProjectIdentity.BuildProject(this.Id, false);
      try
      {
        this.Title = projectIdentity?.GetDisplayTitle();
      }
      catch (Exception ex)
      {
        this.Title = Utils.GetString("Task");
      }
    }

    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg != 526)
        return IntPtr.Zero;
      this.OnMouseTilt(TaskListWindow.LOWORD(wParam));
      return (IntPtr) 1;
    }

    private static int HIWORD(IntPtr ptr) => ptr.ToInt32() >> 16 & (int) ushort.MaxValue;

    private static int LOWORD(IntPtr ptr) => ((int) ptr.ToInt64() >> 16) % 256;

    private void OnMouseTilt(int offset) => this._taskView?.OnTouchScroll(offset);

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._taskView?.TryFoldDetailOnMouseDown(e);
    }

    private void OnWindowMouseDown(object sender, MouseButtonEventArgs e) => this._mouseDown = true;

    private async void OnWindowMouseUp(object sender, MouseButtonEventArgs e)
    {
      TaskListWindow element = this;
      if (!element._mouseDown)
        return;
      element._mouseDown = false;
      TaskView taskView = element._taskView;
      if ((taskView != null ? (taskView.DetailMouseOver() ? 1 : 0) : 0) != 0 || Utils.GetMousePointVisibleItem<QuickAddView>((MouseEventArgs) e, (FrameworkElement) element) != null || Utils.GetMousePointVisibleItem<TextBox>((MouseEventArgs) e, (FrameworkElement) element) != null)
        return;
      await Task.Delay(100);
      if (!element.IsActive || e.Handled)
        return;
      element.TryFocus();
    }

    public async Task TryFocus()
    {
      TaskListWindow element = this;
      await Task.Delay(10);
      Keyboard.ClearFocus();
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

    private void OnProjectViewModeChanged(object sender, ProjectIdentity e)
    {
      if (!(e.Id == this._taskView?.ProjectIdentity?.Id))
        return;
      this._taskView?.OnProjectSelect(ProjectIdentity.BuildProject(this.Id, false));
    }

    public static async Task ShowSavedWindows()
    {
      foreach (WindowModel windowModel in await WindowModelDao.GetAllAsync())
      {
        if (!windowModel.Closed && windowModel.Id.Contains(":"))
        {
          if (ProjectIdentity.BuildProject(windowModel.Id, false) == null)
            break;
          if (TaskListWindow.Windows.ContainsKey(windowModel.Id))
          {
            TaskListWindow window = TaskListWindow.Windows[windowModel.Id];
            window.Show();
            window.Activate();
            window.Reload();
          }
          else
          {
            TaskListWindow taskListWindow = new TaskListWindow(windowModel);
            taskListWindow.Show();
            taskListWindow.SetBackBrush();
            TaskListWindow.Windows.TryAdd(windowModel.Id, taskListWindow);
          }
        }
      }
    }

    public static async Task ShowWindow(string projectId, bool force = true, System.Windows.Window win = null)
    {
      ProjectIdentity identity = ProjectIdentity.BuildProject(projectId, false);
      WindowModel windowModel;
      if (identity == null)
      {
        identity = (ProjectIdentity) null;
        windowModel = (WindowModel) null;
      }
      else
      {
        windowModel = await WindowModelDao.GetModelByIdAsync(projectId);
        if (TaskListWindow.Windows.ContainsKey(projectId))
        {
          TaskListWindow window = TaskListWindow.Windows[projectId];
          window.Show();
          window.Activate();
          window.Reload();
          identity = (ProjectIdentity) null;
          windowModel = (WindowModel) null;
        }
        else
        {
          IndependentWindow.AddShowEvent(TaskListWindow.GetEventName(identity));
          if (windowModel == null)
            windowModel = await IndependentWindow.GetNewWindowModel(projectId, true, win: win);
          else
            await WindowModelDao.OpenWindow(windowModel);
          TaskListWindow taskListWindow = new TaskListWindow(windowModel);
          taskListWindow.Show();
          taskListWindow.SetBackBrush();
          TaskListWindow.Windows.TryAdd(projectId, taskListWindow);
          identity = (ProjectIdentity) null;
          windowModel = (WindowModel) null;
        }
      }
    }

    private static string GetEventName(ProjectIdentity identity)
    {
      switch (identity)
      {
        case WeekProjectIdentity _:
          return "n7d";
        case AssignToMeProjectIdentity _:
          return "assign_to_me";
        case AbandonedProjectIdentity _:
          return "won_t_do";
        case CompletedProjectIdentity _:
          return "completed";
        case TrashProjectIdentity _:
          return "trash";
        case AllProjectIdentity _:
          return "all";
        case TodayProjectIdentity _:
          return "today";
        case TomorrowProjectIdentity _:
          return "tomorrow";
        case NormalProjectIdentity normalProjectIdentity:
          ProjectModel project = normalProjectIdentity.Project;
          return (project != null ? (project.Isinbox ? 1 : 0) : 0) == 0 ? "list" : "inbox";
        case TagProjectIdentity _:
          return "tag";
        case GroupProjectIdentity _:
          return "folder";
        case FilterProjectIdentity _:
          return "filter";
        case SubscribeCalendarProjectIdentity _:
        case BindAccountCalendarProjectIdentity _:
          return "events";
        default:
          return (string) null;
      }
    }

    private void Reload() => this._taskView?.ReloadView();

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
      if (e.NewSize.Width > 800.0)
      {
        this._taskView?.ExpandDetail();
        this._taskView?.SetListColumnSpan(1, 340.0, 328.0);
      }
      else
      {
        this._taskView?.FoldDetail();
        this._taskView?.SetListColumnSpan(2, 0.0, 0.0);
      }
      if (Math.Abs(e.NewSize.Width - e.PreviousSize.Width) <= 3.0 || this.ActualWidth <= 0.0)
        return;
      this._taskView?.ResetColumnWidth(this.ActualWidth);
    }

    protected override void OnClosing()
    {
      DataChangedNotifier.ViewModeChanged -= new EventHandler<ProjectIdentity>(this.OnProjectViewModeChanged);
      this.Container.Children.Clear();
      TaskListWindow.Windows.TryRemove(this.Id, out TaskListWindow _);
    }

    public override void Print() => this._taskView?.TryPrint(false);

    public override void SwitchViewMode(string mode)
    {
      this._taskView?.ProjectIdentity?.SwitchViewMode(mode);
    }

    public static async void OpenOrCloseWindow(string id, System.Windows.Window w)
    {
      if (string.IsNullOrEmpty(id))
        return;
      TaskListWindow taskListWindow;
      if (TaskListWindow.Windows.TryRemove(id, out taskListWindow))
      {
        taskListWindow.Close();
      }
      else
      {
        if (!IndependentWindow.CheckCount(w))
          return;
        await Task.Delay(100);
        TaskListWindow.ShowWindow(id, win: w);
      }
    }

    public override void BatchSetPriorityCommand(int priority)
    {
      this._taskView?.TryBatchSetPriority(priority);
    }

    public override void BatchSetDateCommand(DateTime? date)
    {
      this._taskView?.TryBatchSetDate(date);
    }

    public override void BatchPinTaskCommand() => this._taskView?.BatchPinTask();

    public override void BatchOpenStickyCommand() => this._taskView?.BatchOpenSticky();

    public override void BatchDeleteCommand() => this._taskView?.BatchDeleteTask();

    public static void CloseAllWindow()
    {
      foreach (System.Windows.Window window in TaskListWindow.Windows.Values.ToList<TaskListWindow>())
        window.Close();
    }

    public void ExitImmersiveMode()
    {
      if (this._immersiveBorder.Visibility != Visibility.Visible)
        return;
      this._immersiveBorder.Visibility = Visibility.Collapsed;
      this._immersiveBorder.Child = (UIElement) null;
      this._containBorder.Visibility = Visibility.Visible;
    }

    public async void EnterImmersiveMode(string taskId, int caretOffset = -1)
    {
      ImmersiveContent immersiveContent = new ImmersiveContent();
      immersiveContent.Margin = new Thickness(0.0, 24.0, 0.0, 12.0);
      immersiveContent.VerticalAlignment = VerticalAlignment.Stretch;
      ImmersiveContent editor = immersiveContent;
      this._immersiveBorder.Child = (UIElement) editor;
      this._immersiveBorder.Visibility = Visibility.Visible;
      this._containBorder.Visibility = Visibility.Collapsed;
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

    public void SetMinSize(int width, int height)
    {
      this.MinWidth = (double) (width - 50);
      this.MinHeight = (double) height;
    }

    public async Task NavigateTask(ProjectTask task)
    {
      TaskListWindow target = this;
      if (await TaskUtils.TryLoadTask(task.TaskId, task.ProjectId) == null)
        return;
      TaskDetailWindow taskDetailWindow = new TaskDetailWindow();
      taskDetailWindow.ShowInNavigate(task.TaskId, (System.Windows.Window) target);
      taskDetailWindow.DependentWindow = (IToastShowWindow) target;
    }

    public void OnProjectWidthChanged(double width)
    {
    }

    public void OnDetailWidthChanged(double width)
    {
    }

    public void SaveSelectedProject(string saveProjectId)
    {
    }

    public double GetProjectWidth() => 0.0;

    public double GetDetailWidth() => 0.0;

    public string GetSelectedProject() => "";

    public static void SetWindowsBackBrush()
    {
      foreach (IndependentWindow independentWindow in (IEnumerable<TaskListWindow>) TaskListWindow.Windows.Values)
        independentWindow.SetBackBrush();
    }
  }
}
