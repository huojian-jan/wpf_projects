// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskDetailWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Kanban.Item;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.TaskList.Item;
using ticktick_WPF.Views.Time;
using ticktick_WPF.Views.Undo;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskDetailWindow : MyWindow, IToastShowWindow, IComponentConnector
  {
    private static TaskDetailWindow _window;
    private readonly Stack<ProjectTask> _navigateStack = new Stack<ProjectTask>();
    private bool _isInOperation;
    private double _lastTop;
    public IToastShowWindow DependentWindow;
    private bool _isHided = true;
    private bool _closed;
    public UIElement Target;
    private bool _isNavigate;
    private DateTime _operationTime;
    private static TaskDetailWindow _navigateWindow;
    private Dictionary<string, KeyBinding> _keyBindings = new Dictionary<string, KeyBinding>();
    internal TaskDetailWindow Root;
    internal ContentControl Control;
    internal Grid OperationPanel;
    internal TextBlock TitleText;
    internal TaskDetailPopupView Detail;
    internal Grid ToastGrid;
    private bool _contentLoaded;

    public TaskDetailWindow()
    {
      this.InitializeComponent();
      Utils.SetWindowChrome((Window) this, new Thickness(5.0));
      this.InitWindowEvent();
      this.BindEvents();
      this.Closed += (EventHandler) ((s, e) =>
      {
        this.UnbindEvents();
        this._closed = true;
        if (TaskDetailWindow._window != this)
          return;
        TaskDetailWindow._window = (TaskDetailWindow) null;
      });
      this.Loaded += new RoutedEventHandler(this.OnWindowLoaded);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnWindowLoaded);
      this.ResetPosition();
    }

    private async void ResetPosition()
    {
      TaskDetailWindow taskDetailWindow = this;
      double left = taskDetailWindow.Left;
      double top = taskDetailWindow.Top;
      WindowHelper.MoveTo((Window) taskDetailWindow, (int) taskDetailWindow.Left, (int) taskDetailWindow.Top);
      taskDetailWindow.Left = left;
      taskDetailWindow.Top = top;
      Matrix? transform = PresentationSource.FromVisual((Visual) taskDetailWindow)?.CompositionTarget?.TransformFromDevice;
      await Task.Delay(1000);
      System.Windows.Point defaultPoint = new System.Windows.Point(SystemParameters.PrimaryScreenWidth / 2.0, SystemParameters.PrimaryScreenHeight / 2.0);
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      System.Windows.Point pomoLocationSafely = WidgetLocationHelper.GetPomoLocationSafely(taskDetailWindow.Left, taskDetailWindow.Top, __nonvirtual (taskDetailWindow.Width), __nonvirtual (taskDetailWindow.Height), transform, defaultPoint);
      taskDetailWindow.Left = pomoLocationSafely.X;
      taskDetailWindow.Top = pomoLocationSafely.Y;
    }

    public bool ShowInCal
    {
      get
      {
        return this.Target != null && !(this.Target is TaskListItem) && !(this.Target is KanbanItemView) && !(this.Target is WidgetTaskItem);
      }
    }

    private void UnbindEvents()
    {
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      DataChangedNotifier.MainWindowHidden -= new EventHandler(this.OnMainWindowHidden);
      this.RemoveKeyBinding();
    }

    private void BindEvents()
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      DataChangedNotifier.MainWindowHidden += new EventHandler(this.OnMainWindowHidden);
      this.InitShortcut();
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      if (e.DeletedChangedIds.Any() && e.DeletedChangedIds.Contains(this.Detail.TaskId) || e.UndoDeletedIds.Any() && e.UndoDeletedIds.Contains(this.Detail.TaskId))
        this.TryClose();
      if (!e.KindChangedIds.Any() || !e.KindChangedIds.Contains(this.Detail.TaskId))
        return;
      this.TitleText.Text = Utils.GetString(TaskCache.GetTaskById(this.Detail.TaskId)?.Kind == "NOTE" ? "NoteDetail" : "TaskDetail");
    }

    public event EventHandler<ProjectTask> NavigateTask;

    private void InitWindowEvent()
    {
      this.Detail.ShowUndoOnTaskDeleted -= new EventHandler<string>(this.TaskDeleted);
      this.Detail.ShowUndoOnTaskDeleted += new EventHandler<string>(this.TaskDeleted);
      this.Detail.CheckItemsDeleted -= new EventHandler<TaskDetailItemModel>(this.CheckItemsDeleted);
      this.Detail.CheckItemsDeleted += new EventHandler<TaskDetailItemModel>(this.CheckItemsDeleted);
      this.Detail.NavigateBack -= new EventHandler(this.OnNavigateBack);
      this.Detail.NavigateBack += new EventHandler(this.OnNavigateBack);
      this.Detail.EscKeyUp -= new EventHandler(this.OnEscKeyUp);
      this.Detail.EscKeyUp += new EventHandler(this.OnEscKeyUp);
    }

    private void OnEscKeyUp(object sender, EventArgs e)
    {
      if (!this._isInOperation && !this.Detail.MorePopupOpen)
      {
        this.Detail.SetPopupShowing(false);
        this._isHided = false;
        this.TryClose();
      }
      else
        this._isInOperation = false;
    }

    private async void OnNavigateBack(object sender, EventArgs e)
    {
      if (this._navigateStack.Count <= 0)
        return;
      if (this._navigateStack.Count <= 1)
        this.SetNavigateBackDisabled();
      this.Navigate(this._navigateStack.Pop(), true, this._navigateStack.Count > 1);
    }

    private void SetNavigateBackDisabled() => this.Detail.SetBackIconEnable(false);

    private async void OnNavigateTask(object sender, ProjectTask task)
    {
      await this.OnNavigateTask(task);
    }

    public async Task OnNavigateTask(ProjectTask task)
    {
      TaskDetailWindow taskDetailWindow = this;
      if (await TaskUtils.TryLoadTask(task.TaskId, task.ProjectId) == null)
        return;
      if (taskDetailWindow.OperationPanel.Visibility == Visibility.Collapsed)
      {
        taskDetailWindow.InitNavigateWindow();
        await taskDetailWindow.Navigate(task, canBack: false);
      }
      else
      {
        taskDetailWindow._navigateStack.Push(new ProjectTask()
        {
          TaskId = taskDetailWindow.Detail.TaskId,
          ProjectId = taskDetailWindow.Detail.ProjectId
        });
        await taskDetailWindow.Navigate(task);
      }
      taskDetailWindow.Activate();
      if (taskDetailWindow.WindowState != WindowState.Minimized)
        return;
      taskDetailWindow.WindowState = WindowState.Normal;
    }

    private void SetNavigateEnabled() => this.Detail.SetBackIconEnable(true);

    private void TrySaveOnEnter(object sender, EventArgs e) => this.TryClose();

    public void TryToastString(object sender, string e) => this.ToastString(sender, e);

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks)
    {
      return await TaskService.BatchDeleteTasks(tasks, undoGrid: this.ToastGrid);
    }

    public void TaskComplete(CloseUndoToast undo)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, (FrameworkElement) undo);
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
    }

    public void ToastMoveProjectControl(string projectId, string taskName = null, MoveToastType moveType = MoveToastType.Move)
    {
    }

    public void Toast(FrameworkElement uiElement)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, uiElement);
    }

    public async void TaskDeleted(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.deleted = 1;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnDeletedChanged(task);
        UndoToast undoToast = new UndoToast();
        undoToast.InitTaskUndo(taskId, task.title);
        this.ShowUndoToast(undoToast);
        task = (TaskModel) null;
      }
    }

    private async Task ShowUndoToast(UndoToast undoToast)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, (FrameworkElement) undoToast);
    }

    private void ToastString(object sender, string e)
    {
      WindowToastHelper.ToastString(this.ToastGrid, e);
    }

    private void CheckItemsDeleted(object sender, TaskDetailItemModel e)
    {
      UndoToast undoToast = new UndoToast();
      undoToast.InitSubtaskUndo(e);
      this.ShowUndoToast(undoToast);
    }

    private async void TaskDeleted(object sender, string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(taskId, task.projectId);
        // ISSUE: explicit non-virtual call
        if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
        {
          subTasksByIdAsync.Add(task);
          IToastShowWindow dependentWindow = this.DependentWindow;
          if (dependentWindow == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            dependentWindow.BatchDeleteTask(subTasksByIdAsync);
            task = (TaskModel) null;
          }
        }
        else
        {
          IToastShowWindow dependentWindow = this.DependentWindow;
          if (dependentWindow == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            dependentWindow.TaskDeleted(taskId);
            task = (TaskModel) null;
          }
        }
      }
    }

    public event EventHandler<string> Disappear;

    public event EventHandler<string> TaskSaved;

    public async void ShowInNavigate(string taskId, Window target)
    {
      TaskDetailWindow taskDetailWindow = this;
      TaskDetailViewModel model = await TaskDetailViewModel.Build(taskId);
      if (model == null)
        return;
      taskDetailWindow.TitleText.Text = Utils.GetString(model.Kind == "NOTE" ? "NoteDetail" : "TaskDetail");
      taskDetailWindow.InitNavigateWindow();
      double height = (App.Window.Height - 384.0) / 2.0;
      double targetWidth = (App.Window.Width - 520.0) / 2.0;
      taskDetailWindow.Show(model, string.Empty, new TaskWindowDisplayArgs((UIElement) target, targetWidth, new System.Windows.Point(), height));
      taskDetailWindow.Owner = (Window) null;
    }

    private void InitNavigateWindow()
    {
      this._isNavigate = true;
      this.Width = 560.0;
      this.Height = 424.0;
      this.MinWidth = 404.0;
      this.Control.MaxHeight = 10000.0;
      this.Detail.MaxHeight = 10000.0;
      this.ShowInTaskbar = true;
      this.MaxHeight = 10000.0;
      this.Detail.SetNavigate();
      this.OperationPanel.Visibility = Visibility.Visible;
      this.SizeToContent = SizeToContent.Manual;
    }

    public async void Show(
      TaskDetailViewModel model,
      string itemId,
      TaskWindowDisplayArgs args,
      System.Windows.Point point = default (System.Windows.Point),
      bool show = true,
      bool canParse = false,
      ProjectIdentity parentIdentity = null)
    {
      TaskDetailWindow element = this;
      element.Target = args.Target;
      if (element.Target == null)
      {
        args.Point = PopupLocationCalculator.GetMousePoint(false);
        element.Owner = (Window) null;
      }
      else
      {
        try
        {
          Window window = Window.GetWindow((DependencyObject) element.Target);
          if (window != null && window.Topmost)
            element.Owner = !(window is LoadMoreWindow) ? window : (Window) null;
          else
            element.Owner = (Window) null;
          if (window is WidgetWindow widgetWindow)
            element.Resources = widgetWindow.Resources;
          else
            element.Resources = Application.Current.Resources;
          element.Detail.SetTheme((bool) element.FindResource((object) "IsDarkTheme"));
        }
        catch (Exception ex)
        {
          element.Owner = (Window) null;
        }
      }
      element.Topmost = true;
      model.Mode = 1;
      element.InitDetailEvents();
      element.Detail.SetInQuadrant(args.QuadrantLevel);
      element.Detail.ParentIdentity = parentIdentity;
      element.Detail.SetTitleParseEnable(canParse, false);
      if (point == new System.Windows.Point() && element.Target is Window target)
        point = new System.Windows.Point(target.Left + args.TargetWidth, target.Top);
      if (!element._isNavigate)
      {
        element.SizeToContent = SizeToContent.Height;
        element.Detail.MaxHeight = 480.0;
        element.OperationPanel.Visibility = Visibility.Collapsed;
        element.Detail.Margin = new Thickness(-4.0, 0.0, -4.0, 0.0);
      }
      if (model.IsNewAdd)
        PopupStateManager.OnAddPopupOpened();
      else
        PopupStateManager.OnViewPopupOpened();
      await element.TryShowWindow(model, itemId, args.AddHeight, point, show);
      Keyboard.ClearFocus();
      await Task.Delay(200);
      if (model.IsNewAdd || args.FocusTitle)
      {
        element.Detail.TryFocusTitle();
      }
      else
      {
        FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
        Keyboard.Focus((IInputElement) element);
      }
    }

    private async Task TryShowWindow(
      TaskDetailViewModel model,
      string itemId,
      double addHeight,
      System.Windows.Point point,
      bool show = true)
    {
      TaskDetailWindow taskDetailWindow = this;
      taskDetailWindow.Detail.ScrollToTop();
      taskDetailWindow.Detail.Navigate(model, itemId);
      await Task.Delay(10);
      // ISSUE: explicit non-virtual call
      __nonvirtual (taskDetailWindow.BeginAnimation(Window.TopProperty, (AnimationTimeline) null));
      // ISSUE: explicit non-virtual call
      __nonvirtual (taskDetailWindow.BeginAnimation(Window.LeftProperty, (AnimationTimeline) null));
      taskDetailWindow.Top = point.Y + addHeight;
      taskDetailWindow.Left = point.X;
      taskDetailWindow.Control.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      taskDetailWindow.Control.Opacity = 1.0;
      if (show && !taskDetailWindow.TryShow())
        return;
      taskDetailWindow._isHided = false;
    }

    private void InitDetailEvents()
    {
      this.Detail.ShowDialog -= new EventHandler(this.OnStartOperation);
      this.Detail.ShowDialog += new EventHandler(this.OnStartOperation);
      this.Detail.CloseDialog -= new EventHandler<bool>(this.OnStopOperation);
      this.Detail.CloseDialog += new EventHandler<bool>(this.OnStopOperation);
      this.Detail.ActionPopOpened -= new EventHandler(this.OnStartOperation);
      this.Detail.ActionPopOpened += new EventHandler(this.OnStartOperation);
      this.Detail.ActionPopClosed -= new EventHandler<bool>(this.OnStopOperation);
      this.Detail.ActionPopClosed += new EventHandler<bool>(this.OnStopOperation);
      this.Detail.EnterImmersive -= new EnterImmersiveDelegate(this.OnEnterImmersive);
      this.Detail.EnterImmersive += new EnterImmersiveDelegate(this.OnEnterImmersive);
      this.Detail.TagClick -= new EventHandler<string>(this.OnTagClick);
      this.Detail.TagClick += new EventHandler<string>(this.OnTagClick);
      this.Detail.NavigateTask -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      this.Detail.NavigateTask += new EventHandler<ProjectTask>(this.OnNavigateTask);
      this.Detail.ForceHideWindow -= new EventHandler(this.OnForceHideWindow);
      this.Detail.ForceHideWindow += new EventHandler(this.OnForceHideWindow);
    }

    private void OnForceHideWindow(object sender, EventArgs e)
    {
      this._isInOperation = false;
      this.TryHideWindow();
    }

    private void OnEnterImmersive(string id, int caret)
    {
      App.Window?.OnEnterImmersive(id, caret);
      this.TryHideWindow();
    }

    private void OnMainWindowHidden(object sender, EventArgs e) => this.TryHideWindow();

    private void OnTagClick(object sender, string e) => this.TryClose();

    private async Task BeforeClose()
    {
      TaskDetailWindow sender = this;
      await Task.Delay(100);
      sender.Detail.TryClosePopup();
      sender.DependentWindow = (IToastShowWindow) null;
      sender.Target = (UIElement) null;
      await sender.TrySaveTask();
      sender._isHided = true;
      sender.Detail.RecordLabelTime();
      EventHandler<string> disappear = sender.Disappear;
      if (disappear == null)
        return;
      disappear((object) sender, sender.Detail.DataContext is TaskDetailViewModel dataContext ? dataContext.TaskId : (string) null);
    }

    private async void OnStopOperation(object sender, bool e)
    {
      TaskDetailWindow taskDetailWindow = this;
      if (!taskDetailWindow.Detail.PopupShowing)
      {
        await Task.Delay(100);
        if ((DateTime.Now - taskDetailWindow._operationTime).TotalMilliseconds < 100.0)
          return;
        if (e && !ModifyRepeatDialog.Showing)
        {
          taskDetailWindow.Activate();
          // ISSUE: explicit non-virtual call
          __nonvirtual (taskDetailWindow.Focus());
        }
      }
      taskDetailWindow._isInOperation = false;
    }

    private void OnStartOperation(object sender, EventArgs e)
    {
      this._isInOperation = true;
      this._operationTime = DateTime.Now;
    }

    public async Task TryClose(bool needActivateOwner = true)
    {
      TaskDetailWindow taskDetailWindow = this;
      if (taskDetailWindow._isHided)
        return;
      await taskDetailWindow.BeforeClose();
      taskDetailWindow.Close();
      if (!needActivateOwner)
        return;
      taskDetailWindow.Owner?.Activate();
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
      TaskDetailWindow taskDetailWindow = this;
      taskDetailWindow._closed = true;
      taskDetailWindow.Detail.RemoveKeyBinding();
      await taskDetailWindow.BeforeClose();
      if (taskDetailWindow == TaskDetailWindow._window)
        TaskDetailWindow._window = new TaskDetailWindow();
      // ISSUE: reference to a compiler-generated method
      taskDetailWindow.\u003C\u003En__0(e);
    }

    protected override async void OnActivated(EventArgs e)
    {
      TaskDetailWindow taskDetailWindow = this;
      // ISSUE: reference to a compiler-generated method
      taskDetailWindow.\u003C\u003En__1(e);
      await Task.Delay(100);
      if (!taskDetailWindow.IsActive)
        return;
      taskDetailWindow.Detail.SetPopupShowing(false);
      taskDetailWindow._isInOperation = false;
    }

    private async void OnDeactivated(object sender, EventArgs e)
    {
      bool canClose = this.GetCanClose();
      await Task.Delay(20);
      int num = this.Detail.PopupShowing ? 1 : 0;
      bool flag = this.Detail.DataContext is TaskDetailViewModel dataContext && dataContext.IsNewAdd;
      if (num != 0)
        return;
      if (flag)
        this.TryHideWindow(true);
      else if (this._isNavigate | canClose)
        this.TryHideWindow(true);
      else
        await this.TrySaveTask();
    }

    private bool GetCanClose()
    {
      if (this.DependentWindow != null && this.DependentWindow is FrameworkElement dependentWindow)
      {
        IShowTaskDetailWindow pointVisibleItem = Utils.GetMousePointVisibleItem<IShowTaskDetailWindow>((MouseEventArgs) null, dependentWindow);
        if (pointVisibleItem != null && !pointVisibleItem.Equals((object) this.Target))
          return false;
      }
      return true;
    }

    private async Task TryHideWindow(bool force = false)
    {
      TaskDetailWindow taskDetailWindow = this;
      if (((taskDetailWindow._isInOperation ? 0 : (!taskDetailWindow._isHided ? 1 : 0)) | (force ? 1 : 0)) == 0)
        return;
      taskDetailWindow._isInOperation = false;
      await taskDetailWindow.BeforeClose();
      taskDetailWindow.Close();
      if (taskDetailWindow != TaskDetailWindow._window)
        return;
      TaskDetailWindow._window = new TaskDetailWindow();
    }

    public async Task TrySaveTask(bool force = false)
    {
      TaskDetailWindow sender = this;
      if (!(sender.Detail.DataContext is TaskDetailViewModel model1))
      {
        model1 = (TaskDetailViewModel) null;
      }
      else
      {
        await sender.Detail.TryParseUrl();
        bool dateSaved = await sender.Detail.TrySaveParseDate();
        if (!(sender.NeedSaveNew(model1) | force))
        {
          model1 = (TaskDetailViewModel) null;
        }
        else
        {
          if (string.IsNullOrEmpty(model1.TaskId))
            model1.SourceViewModel.Id = Utils.GetGuid();
          TaskModel task = await TaskDetailViewModel.ToTaskModel(model1);
          if (task != null)
          {
            TaskDetailItemModel[] items1 = model1.Items;
            if ((items1 != null ? (items1.Length != 0 ? 1 : 0) : 0) != 0 && ((IEnumerable<TaskDetailItemModel>) model1.Items).All<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (item => item.status != 0)))
            {
              task.status = 2;
              task.completedTime = new DateTime?(DateTime.Now);
            }
            if (await TaskDao.GetThinTaskById(task.id) != null)
            {
              TaskModel taskModel1 = await TaskService.SaveTask(task);
            }
            else
            {
              PopupStateManager.OnAddPopupClosed(false);
              task.createdTime = new DateTime?(DateTime.Now);
              TaskModel taskModel2 = await TaskService.AddTask(task);
              model1.IsNewAdd = false;
              if (!dateSaved)
                await TaskService.SaveTaskReminders(task);
              TaskDetailWindow._window = sender;
            }
            TaskDetailItemModel[] items2 = model1.Items;
            if ((items2 != null ? (items2.Length != 0 ? 1 : 0) : 0) != 0)
            {
              TaskDetailItemModel[] taskDetailItemModelArray = model1.Items;
              for (int index = 0; index < taskDetailItemModelArray.Length; ++index)
              {
                TaskDetailItemModel model = taskDetailItemModelArray[index];
                model.TaskServerId = task.id;
                await TaskDetailItemDao.InsertChecklistItem(model);
              }
              taskDetailItemModelArray = (TaskDetailItemModel[]) null;
            }
            EventHandler<string> taskSaved = sender.TaskSaved;
            if (taskSaved != null)
              taskSaved((object) sender, model1.TaskId);
          }
          task = (TaskModel) null;
          model1 = (TaskDetailViewModel) null;
        }
      }
    }

    private bool NeedSaveNew(TaskDetailViewModel model)
    {
      if (!model.IsNewAdd)
        return false;
      bool flag = !string.IsNullOrEmpty(model.Title) || !string.IsNullOrEmpty(model.TaskContent) || !string.IsNullOrEmpty(model.Desc) || !this.Detail.IsBlank;
      List<CheckItemViewModel> checklistItems = this.Detail.GetChecklistItems();
      if (checklistItems != null)
      {
        flag = flag || !this.IsItemsEmpty(checklistItems);
        if (flag)
        {
          List<TaskDetailItemModel> taskDetailItemModelList = new List<TaskDetailItemModel>();
          foreach (CheckItemViewModel checkItemViewModel in checklistItems)
          {
            TaskDetailItemModel taskDetailItemModel = new TaskDetailItemModel()
            {
              id = string.IsNullOrEmpty(checkItemViewModel.Id) ? Utils.GetGuid() : checkItemViewModel.Id,
              TaskId = 0,
              title = checkItemViewModel.Title,
              sortOrder = checkItemViewModel.SortOrder,
              completedTime = new DateTime?(),
              startDate = checkItemViewModel.StartDate,
              isAllDay = checkItemViewModel.IsAllDay,
              snoozeReminderTime = checkItemViewModel.SnoozeReminderTime,
              status = checkItemViewModel.Status
            };
            if (taskDetailItemModel.status != 0)
              taskDetailItemModel.completedTime = new DateTime?(DateTime.Now);
            taskDetailItemModelList.Add(taskDetailItemModel);
          }
          model.Items = taskDetailItemModelList.ToArray();
        }
      }
      return flag;
    }

    private bool IsItemsEmpty(List<CheckItemViewModel> items)
    {
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      return (items == null || __nonvirtual (items.Count) <= 1) && (items == null || __nonvirtual (items.Count) != 1 || string.IsNullOrEmpty(items[0].Title));
    }

    public async void FocusTitle()
    {
      await Task.Delay(50);
      this.Detail.TryFocusTitle(false);
    }

    public void EnterCommand()
    {
      if (this._isInOperation || !(this.Detail.DataContext is TaskDetailViewModel dataContext) || !dataContext.IsNewAdd)
        return;
      this.TryClose();
    }

    public async void ReLoad()
    {
      TaskDetailWindow taskDetailWindow = this;
      taskDetailWindow.Activate();
      if (!(taskDetailWindow.Detail.DataContext is TaskDetailViewModel model))
      {
        model = (TaskDetailViewModel) null;
      }
      else
      {
        await Task.Delay(100);
        taskDetailWindow.Detail.Navigate(model.TaskId);
        model = (TaskDetailViewModel) null;
      }
    }

    private async Task Navigate(ProjectTask projectTask, bool isBack = false, bool canBack = true)
    {
      TaskModel taskModel = await TaskUtils.TryLoadTask(projectTask.TaskId, projectTask.ProjectId, isBack);
      if (taskModel == null)
        return;
      this.TitleText.Text = Utils.GetString(taskModel.kind == "NOTE" ? "NoteDetail" : "TaskDetail");
      if (canBack)
        this.SetNavigateEnabled();
      this.Detail.Navigate(projectTask.TaskId);
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => this.TryClose();

    private void OnNormalButtonClick(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Normal;
      this.Top = this._lastTop;
    }

    private void OnMaxButtonClick(object sender, RoutedEventArgs e)
    {
      this._lastTop = this.Top;
      this.WindowState = WindowState.Maximized;
    }

    private void OnMinButtonClick(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Minimized;
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      this.DragMove();
    }

    public void ReLoad(string taskId, string itemId, TaskModifyType type)
    {
      if (!(taskId == this.Detail.TaskId))
        return;
      this.ReLoad();
    }

    public static void TryCloseWindow(bool force = false)
    {
      if (!PopupStateManager.IsViewPopOpened())
        return;
      TaskDetailWindow._window?.TryClose();
    }

    public static void SetCanClose(bool value)
    {
      TaskDetailWindow._window?.Detail.SetPopupShowing(!value);
    }

    private void OnWindowStateChanged(object sender, EventArgs e)
    {
      if (this._isNavigate)
        return;
      this.Close();
      TaskDetailWindow._window = new TaskDetailWindow();
    }

    private void OnWindowKeyUp(object sender, KeyEventArgs e)
    {
      if (!object.Equals((object) FocusManager.GetFocusedElement((DependencyObject) this), (object) this))
        return;
      switch (e.Key)
      {
        case Key.Return:
          this.Detail.TryFocusTitle();
          break;
        case Key.Escape:
          if (!this._isInOperation)
          {
            this.TryClose();
            break;
          }
          this._isInOperation = false;
          break;
        case Key.Delete:
          this.Detail.OnDelete();
          break;
      }
    }

    public void Clear()
    {
      this.DependentWindow = (IToastShowWindow) null;
      this.Target = (UIElement) null;
      this._isHided = true;
      EventHandler<string> disappear = this.Disappear;
      if (disappear != null)
        disappear((object) this, (string) null);
      PopupStateManager.OnViewPopupClosed();
      PopupStateManager.OnAddPopupClosed(false);
      this.Detail.Navigate(new TaskDetailViewModel(new TaskBaseViewModel()));
    }

    public bool TryShow()
    {
      try
      {
        this.Show();
        this.Activate();
      }
      catch (Exception ex)
      {
        TaskDetailWindow._window = new TaskDetailWindow();
        PopupStateManager.OnViewPopupClosed();
        PopupStateManager.OnAddPopupClosed(false);
        return false;
      }
      return true;
    }

    public void ToastInParent(string toast)
    {
      this.DependentWindow?.TryToastString((object) null, toast);
    }

    public static async Task NavigateProjectTask(ProjectTask task, IToastShowWindow window)
    {
      if (await TaskUtils.TryLoadTask(task.TaskId, task.ProjectId) == null)
        return;
      if (TaskDetailWindow._navigateWindow == null)
      {
        TaskDetailWindow._navigateWindow = new TaskDetailWindow();
        TaskDetailWindow._navigateWindow.ShowInNavigate(task.TaskId, window as Window);
        TaskDetailWindow._navigateWindow.DependentWindow = window;
        TaskDetailWindow._navigateWindow.Closed += (EventHandler) ((o, e) => TaskDetailWindow._navigateWindow = (TaskDetailWindow) null);
      }
      else
        TaskDetailWindow._navigateWindow.OnNavigateTask(task);
    }

    public void RemoveKeyBinding()
    {
      lock (this._keyBindings)
      {
        foreach (KeyValuePair<string, KeyBinding> keyBinding in this._keyBindings)
        {
          KeyBindingManager.RemoveKeyBinding(keyBinding.Key, keyBinding.Value);
          keyBinding.Value.CommandParameter = (object) null;
          keyBinding.Value.CommandTarget = (IInputElement) null;
        }
        this.InputBindings.Clear();
        this._keyBindings.Clear();
      }
    }

    private void InitShortcut()
    {
      AddKeyBinding("OpenSticky", GetKeyBinding(DetailWindowCommands.OpenAsSticky));

      void AddKeyBinding(string key, KeyBinding kb)
      {
        lock (this._keyBindings)
        {
          this._keyBindings[key] = kb;
          KeyBindingManager.TryAddKeyBinding(key, kb);
        }
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

    public void OpenAsSticky() => this.Detail.OpenAsSticky();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskdetailwindow.xaml", UriKind.Relative));
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
          this.Root = (TaskDetailWindow) target;
          break;
        case 2:
          this.Control = (ContentControl) target;
          break;
        case 3:
          this.OperationPanel = (Grid) target;
          this.OperationPanel.MouseMove += new MouseEventHandler(this.OnDragMove);
          break;
        case 4:
          this.TitleText = (TextBlock) target;
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnMinButtonClick);
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnMaxButtonClick);
          break;
        case 7:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnNormalButtonClick);
          break;
        case 8:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        case 9:
          this.Detail = (TaskDetailPopupView) target;
          break;
        case 10:
          this.ToastGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
