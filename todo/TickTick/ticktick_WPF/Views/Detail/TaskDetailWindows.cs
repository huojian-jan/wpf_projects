// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskDetailWindows
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
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.Undo;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskDetailWindows : MyWindow, IToastShowWindow, IComponentConnector
  {
    private string _taskId;
    public static List<TaskDetailWindows> OpenedWindows = new List<TaskDetailWindows>();
    private WindowState _originState;
    private bool _closed;
    internal TaskDetailWindows Root;
    internal ContentControl Control;
    internal Grid OperationPanel;
    internal TaskDetailPopupView Detail;
    internal Grid ToastGrid;
    private bool _contentLoaded;

    public TaskDetailWindows()
    {
      this.InitializeComponent();
      Utils.SetWindowChrome((Window) this, new Thickness(5.0));
      this.InitWindowEvent();
      this.InitGlobalEvent();
      this.Loaded += new RoutedEventHandler(this.OnWindowLoaded);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnWindowLoaded);
      this.ResetPosition();
      this.InitKeyBinding();
    }

    private async void ResetPosition()
    {
      TaskDetailWindows taskDetailWindows = this;
      WindowHelper.MoveToCenter((Window) taskDetailWindows);
      Matrix? transform = PresentationSource.FromVisual((Visual) taskDetailWindows)?.CompositionTarget?.TransformFromDevice;
      await Task.Delay(1000);
      System.Windows.Point defaultPoint = new System.Windows.Point(SystemParameters.PrimaryScreenWidth / 2.0, SystemParameters.PrimaryScreenHeight / 2.0);
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      System.Windows.Point pomoLocationSafely = WidgetLocationHelper.GetPomoLocationSafely(taskDetailWindows.Left, taskDetailWindows.Top, __nonvirtual (taskDetailWindows.Width), __nonvirtual (taskDetailWindows.Height), transform, defaultPoint);
      taskDetailWindows.Left = pomoLocationSafely.X;
      taskDetailWindows.Top = pomoLocationSafely.Y;
    }

    private void InitGlobalEvent()
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      if ((!e.DeletedChangedIds.Any() || !e.DeletedChangedIds.Contains(this.Detail.TaskId)) && (!e.UndoDeletedIds.Any() || !e.UndoDeletedIds.Contains(this.Detail.TaskId)))
        return;
      this.TryClose();
    }

    private void InitWindowEvent()
    {
      this.Detail.ShowUndoOnTaskDeleted -= new EventHandler<string>(this.TaskDeleted);
      this.Detail.ShowUndoOnTaskDeleted += new EventHandler<string>(this.TaskDeleted);
      this.Detail.EscKeyUp -= new EventHandler(this.OnEscKeyUp);
      this.Detail.EscKeyUp += new EventHandler(this.OnEscKeyUp);
      this.Detail.NavigateTask -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      this.Detail.NavigateTask += new EventHandler<ProjectTask>(this.OnNavigateTask);
      this.Detail.TaskNavigated -= new EventHandler<string>(this.OnTaskNavigated);
      this.Detail.TaskNavigated += new EventHandler<string>(this.OnTaskNavigated);
      this.Detail.TagClick -= new EventHandler<string>(this.OnTagClick);
      this.Detail.TagClick += new EventHandler<string>(this.OnTagClick);
      this.Detail.NotifyCloseWindow -= new EventHandler(this.OnNotifyClose);
      this.Detail.NotifyCloseWindow += new EventHandler(this.OnNotifyClose);
      this.InitDetailEvents();
    }

    private void InitKeyBinding()
    {
      HotKeyUtils hotKeyUtils = new HotKeyUtils(LocalSettings.Settings.ShortCutModel.CompleteTask.Replace(" ", ""));
      KeyBinding keyBinding = new KeyBinding();
      keyBinding.Command = (ICommand) new RelayCommand((Action<object>) (o => ((TaskDetailWindows) o).CompletedTaskCommand()));
      keyBinding.Key = hotKeyUtils.Key;
      keyBinding.Modifiers = hotKeyUtils.Modifiers;
      keyBinding.CommandParameter = (object) this;
      this.InputBindings.Add((InputBinding) keyBinding);
    }

    private async void OnNotifyClose(object sender, EventArgs e) => this.TryClose();

    private void OnTagClick(object sender, string e) => this.TryClose();

    private void OnTaskNavigated(object sender, string e) => this._taskId = e;

    private async void OnNavigateTask(object sender, ProjectTask pt)
    {
      if (await TaskDetailWindows.ShowTaskWindows(pt.TaskId))
        return;
      WindowToastHelper.ToastString(this.ToastGrid, Utils.GetString("NoTaskFound"));
    }

    private void OnEscKeyUp(object sender, EventArgs e)
    {
      if (this.Detail.Mode == Constants.DetailMode.Editor)
      {
        this.Detail.Mode = Constants.DetailMode.Page;
        this.Detail.SetEditMenuMode(false);
        this.Detail.Reload();
      }
      else
      {
        if (!this.IsActive || this.Detail.PopupShowing)
          return;
        this.TryClose();
      }
    }

    public void TryHideToast()
    {
      if (this.ToastGrid.Children.Count <= 0)
        return;
      if (this.ToastGrid.Children[0] is UndoToast child)
        child.OnFinished();
      this.ToastGrid.Children[0].Visibility = Visibility.Collapsed;
    }

    public void TryToastString(object sender, string e) => this.ToastString(sender, e);

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks)
    {
      TaskDetailWindows taskDetailWindows = this;
      // ISSUE: reference to a compiler-generated method
      return tasks != null && await TaskService.BatchDeleteTasks(tasks, undoGrid: tasks.Any<TaskModel>(new Func<TaskModel, bool>(taskDetailWindows.\u003CBatchDeleteTask\u003Eb__18_0)) ? (Grid) null : taskDetailWindows.ToastGrid);
    }

    public void TaskComplete(CloseUndoToast undo)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, (FrameworkElement) undo);
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
        this.ShowUndoToast(undoToast, taskId);
        task = (TaskModel) null;
      }
    }

    public void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels)
    {
    }

    public void ToastMoveProjectControl(
      string projectProjectId,
      string taskName = null,
      MoveToastType moveType = MoveToastType.Move)
    {
    }

    public void Toast(FrameworkElement uiElement)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, uiElement);
    }

    private async Task ShowUndoToast(UndoToast undoToast, string taskId = null)
    {
      WindowToastHelper.ShowAndHideToast(taskId == this._taskId ? (Grid) null : this.ToastGrid, (FrameworkElement) undoToast);
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
      List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(taskId, task.projectId);
      // ISSUE: explicit non-virtual call
      if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
      {
        subTasksByIdAsync.Add(task);
        this.BatchDeleteTask(subTasksByIdAsync);
        task = (TaskModel) null;
      }
      else
      {
        this.TaskDeleted(taskId);
        task = (TaskModel) null;
      }
    }

    public static async Task<bool> ShowTaskWindows(string taskId)
    {
      TaskDetailWindow.TryCloseWindow();
      TaskBaseViewModel taskModel = TaskCache.GetTaskById(taskId);
      if (taskModel == null)
        return false;
      TaskDetailWindows exist = TaskDetailWindows.OpenedWindows.FirstOrDefault<TaskDetailWindows>((Func<TaskDetailWindows, bool>) (w => w._taskId == taskId));
      if (exist != null && exist._closed)
      {
        TaskDetailWindows.OpenedWindows.Remove(exist);
        exist = (TaskDetailWindows) null;
      }
      if (exist == null)
      {
        OpenNew();
      }
      else
      {
        try
        {
          exist.Detail.Navigate(taskId);
          if (!string.IsNullOrEmpty(taskModel.Title))
            exist.Title = taskModel.Title;
          exist.Show();
          await Task.Delay(10);
          exist.Activate();
        }
        catch (Exception ex)
        {
          TaskDetailWindows.OpenedWindows.Remove(exist);
          OpenNew();
        }
      }
      return true;

      async Task OpenNew()
      {
        await Task.Delay(50);
        TaskDetailWindows window = new TaskDetailWindows();
        window._taskId = taskId;
        window.Detail.Navigate(taskId);
        if (!string.IsNullOrEmpty(taskModel.Title))
          window.Title = taskModel.Title;
        window.Show();
        TaskDetailWindows.OpenedWindows.Add(window);
        await Task.Delay(100);
        if (window._closed)
        {
          window = (TaskDetailWindows) null;
        }
        else
        {
          window.Visibility = Visibility.Visible;
          window.Activate();
          window = (TaskDetailWindows) null;
        }
      }
    }

    private void InitDetailEvents()
    {
      this.Detail.EnterImmersive -= new EnterImmersiveDelegate(this.OnEnterImmersive);
      this.Detail.EnterImmersive += new EnterImmersiveDelegate(this.OnEnterImmersive);
      this.Detail.ExitImmersive -= new EventHandler(this.OnExitImmersive);
      this.Detail.ExitImmersive += new EventHandler(this.OnExitImmersive);
    }

    private void OnExitImmersive(object sender, EventArgs e)
    {
      this.Detail.Mode = Constants.DetailMode.Popup;
      this.Detail.SetEditMenuMode(false);
      this.Detail.SetImmerse();
      this.Detail.Reload();
    }

    private async void OnEnterImmersive(string id, int caret)
    {
      this.Detail.Mode = Constants.DetailMode.Editor;
      this.Detail.Reload();
      this.Detail.SetImmerse();
      await Task.Delay(400);
      this.Detail.SetEditMenuMode(true);
    }

    private async Task BeforeClose()
    {
      this.Detail.TryClosePopup();
      await this.TrySaveTask();
    }

    public async Task TryClose() => this.Close();

    protected override async void OnClosing(CancelEventArgs e)
    {
      TaskDetailWindows taskDetailWindows = this;
      taskDetailWindows._closed = true;
      taskDetailWindows.Detail.RemoveKeyBinding();
      TaskDetailWindows.OpenedWindows.Remove(taskDetailWindows);
      await taskDetailWindows.BeforeClose();
      // ISSUE: reference to a compiler-generated method
      taskDetailWindows.\u003C\u003En__0(e);
    }

    public async Task TrySaveTask(bool force = false)
    {
      if (!(this.Detail.DataContext is TaskDetailViewModel model1))
      {
        model1 = (TaskDetailViewModel) null;
      }
      else
      {
        bool dateSaved = await this.Detail.TrySaveParseDate();
        if (!(this.NeedSaveNew(model1) | force))
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
            TaskDetailItemModel[] items = model1.Items;
            if ((items != null ? (items.Length != 0 ? 1 : 0) : 0) != 0)
            {
              TaskDetailItemModel[] taskDetailItemModelArray = model1.Items;
              for (int index = 0; index < taskDetailItemModelArray.Length; ++index)
              {
                TaskDetailItemModel model = taskDetailItemModelArray[index];
                model.TaskServerId = task.id;
                await TaskDetailItemDao.InsertChecklistItem(model);
              }
              taskDetailItemModelArray = (TaskDetailItemModel[]) null;
              if (((IEnumerable<TaskDetailItemModel>) model1.Items).All<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (item => item.status != 0)))
              {
                task.status = 2;
                task.completedTime = new DateTime?(DateTime.Now);
              }
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
              if (dateSaved)
                await TaskService.SaveTaskReminders(task);
            }
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

    public void EnterCommand()
    {
    }

    public void CompletedTaskCommand() => this.Detail.ToggleTaskCompleted();

    public async void ReLoad()
    {
      TaskDetailWindows taskDetailWindows = this;
      taskDetailWindows.Activate();
      if (!(taskDetailWindows.Detail.DataContext is TaskDetailViewModel model))
      {
        model = (TaskDetailViewModel) null;
      }
      else
      {
        await Task.Delay(100);
        taskDetailWindows.Detail.Navigate(model.TaskId);
        model = (TaskDetailViewModel) null;
      }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => this.TryClose();

    private void OnNormalButtonClick(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Normal;
    }

    private void OnMaxButtonClick(object sender, RoutedEventArgs e)
    {
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

    private void OnStateChanged(object sender, EventArgs e)
    {
      if (this.WindowState == WindowState.Maximized && this._originState != WindowState.Minimized)
      {
        this.Hide();
        this.Show();
      }
      this._originState = this.WindowState;
    }

    public static void CloseAll()
    {
      for (int index = 0; index < TaskDetailWindows.OpenedWindows.Count; ++index)
        TaskDetailWindows.OpenedWindows[index].DelayClose();
    }

    private async void DelayClose()
    {
      await Task.Delay(200);
      this.TryClose();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskdetailwindows.xaml", UriKind.Relative));
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
          this.Root = (TaskDetailWindows) target;
          break;
        case 2:
          this.Control = (ContentControl) target;
          break;
        case 3:
          this.OperationPanel = (Grid) target;
          this.OperationPanel.MouseMove += new MouseEventHandler(this.OnDragMove);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnMinButtonClick);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnMaxButtonClick);
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnNormalButtonClick);
          break;
        case 7:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        case 8:
          this.Detail = (TaskDetailPopupView) target;
          break;
        case 9:
          this.ToastGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
