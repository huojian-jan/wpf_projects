// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskDetailPopup
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
using System.Windows.Media.Animation;
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
  public class TaskDetailPopup : ContentControl, IToastShowWindow, IComponentConnector
  {
    private Popup _popup;
    private bool _isInOperation;
    private double _lastTop;
    public IToastShowWindow DependentWindow;
    public static UIElement LastTarget;
    private static TaskDetailPopup _lastPopup;
    public UIElement Target;
    private bool _isNavigate;
    private DateTime _operationTime;
    private string _taskId;
    private static TaskDetailWindow _navigateWindow;
    private Dictionary<string, KeyBinding> _keyBindings = new Dictionary<string, KeyBinding>();
    internal TaskDetailPopup Control;
    internal TaskDetailPopupView Detail;
    internal Grid ToastGrid;
    private bool _contentLoaded;

    public TaskDetailPopup()
    {
      this.InitializeComponent();
      this.FontWeight = FontWeights.Normal;
      this.FontSize = 14.0;
      Popup popup = new Popup();
      popup.Width = 420.0;
      popup.MinHeight = 320.0;
      popup.AllowsTransparency = true;
      popup.StaysOpen = false;
      popup.PopupAnimation = PopupAnimation.Fade;
      popup.Child = (UIElement) this;
      this._popup = popup;
      this._popup.PreviewKeyUp += new KeyEventHandler(this.OnWindowKeyUp);
      this.Detail.SetBackIconEnable(false);
      this.InitWindowEvent();
      this.BindEvents();
      this._popup.Opened += new EventHandler(this.OnOpened);
      this._popup.Closed += (EventHandler) ((s, e) =>
      {
        PopupStateManager.OnViewPopupClosed(false);
        PopupStateManager.OnAddPopupClosed(false);
        this.UnbindEvents();
        this.Detail.RemoveKeyBinding();
        this.OnClosed();
      });
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

    private async void OnOpened(object sender, EventArgs e)
    {
      await Task.Delay(50);
      HwndHelper.SetFocus(this._popup, !this._popup.StaysOpen);
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      if ((!e.DeletedChangedIds.Any() || !e.DeletedChangedIds.Contains(this.Detail.TaskId)) && (!e.UndoDeletedIds.Any() || !e.UndoDeletedIds.Contains(this.Detail.TaskId)))
        return;
      this.Close();
    }

    private void InitWindowEvent()
    {
      this.Detail.ShowUndoOnTaskDeleted -= new EventHandler<string>(this.TaskDeleted);
      this.Detail.ShowUndoOnTaskDeleted += new EventHandler<string>(this.TaskDeleted);
      this.Detail.CheckItemsDeleted -= new EventHandler<TaskDetailItemModel>(this.CheckItemsDeleted);
      this.Detail.CheckItemsDeleted += new EventHandler<TaskDetailItemModel>(this.CheckItemsDeleted);
      this.Detail.EscKeyUp -= new EventHandler(this.OnEscKeyUp);
      this.Detail.EscKeyUp += new EventHandler(this.OnEscKeyUp);
    }

    private void OnEscKeyUp(object sender, EventArgs e)
    {
      if (this.Detail.MorePopupOpen)
        return;
      this.Close();
    }

    private async void OnNavigateTask(object sender, ProjectTask task)
    {
      await Task.Delay(200);
      TaskDetailWindow taskDetailWindow = new TaskDetailWindow();
      if (!(this.DependentWindow is UIElement uiElement))
        uiElement = this.Target;
      taskDetailWindow.ShowInNavigate(task.TaskId, Window.GetWindow((DependencyObject) uiElement));
      taskDetailWindow.DependentWindow = this.DependentWindow;
      this._popup.IsOpen = false;
    }

    private void SetNavigateEnabled() => this.Detail.SetBackIconEnable(true);

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

    public async Task<bool> Show(
      string taskId,
      string itemId,
      TaskWindowDisplayArgs args,
      ProjectIdentity parentIdentity = null,
      bool show = true)
    {
      TaskDetailPopup taskDetailPopup1 = this;
      TaskDetailViewModel taskDetailViewModel = await TaskDetailViewModel.Build(taskId);
      if (taskDetailViewModel != null)
      {
        TaskDetailPopup taskDetailPopup2 = taskDetailPopup1;
        TaskDetailViewModel model = taskDetailViewModel;
        string itemId1 = itemId;
        TaskWindowDisplayArgs args1 = args;
        bool flag = show;
        ProjectIdentity projectIdentity = parentIdentity;
        System.Windows.Point point = new System.Windows.Point();
        int num = flag ? 1 : 0;
        ProjectIdentity parentIdentity1 = projectIdentity;
        taskDetailPopup2.Show(model, itemId1, args1, point, num != 0, parentIdentity: parentIdentity1);
      }
      return taskDetailViewModel != null;
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
      TaskDetailPopup element = this;
      element._taskId = model.TaskId;
      PopupStateManager.LastTarget = args.Target;
      element.Target = args.Target;
      element._popup.Placement = PlacementMode.Mouse;
      element._popup.PlacementTarget = element.Target;
      TaskDetailPopup._lastPopup = element;
      if (element.Target == null)
      {
        element._popup.Placement = PlacementMode.Mouse;
      }
      else
      {
        try
        {
          if (Window.GetWindow((DependencyObject) element.Target) is WidgetWindow window)
            element.Resources = window.Resources;
          else
            element.Resources = Application.Current.Resources;
          element.Detail.SetTheme((bool) element.FindResource((object) "IsDarkTheme"));
        }
        catch (Exception ex)
        {
        }
      }
      element.InitDetailEvents();
      element.Detail.SetInQuadrant(args.QuadrantLevel);
      element.Detail.ParentIdentity = parentIdentity;
      element.Detail.SetTitleParseEnable(canParse, false);
      if (point == new System.Windows.Point() && !(element.Target is Window))
      {
        TaskPopupArgs popupLocation = PopupLocationCalculator.GetPopupLocation(element.Target, args.TargetWidth, 420.0, args.Point != new System.Windows.Point(), args.AddHeight);
        if (!popupLocation.ByMouse)
        {
          element._popup.Placement = popupLocation.IsRight ? PlacementMode.Right : PlacementMode.Left;
          element._popup.HorizontalOffset = popupLocation.IsRight ? -3.0 : 3.0;
          element._popup.VerticalOffset = -5.0;
        }
      }
      if (!element._isNavigate)
      {
        element.Detail.MaxHeight = 480.0;
        element.Detail.Margin = new Thickness(-4.0, 0.0, -4.0, 0.0);
      }
      await element.TryShowWindow(model, itemId, point, show);
      Keyboard.ClearFocus();
      await Task.Delay(200);
      if (model.IsNewAdd || args.FocusTitle)
      {
        element.Detail.TryFocusTitle();
      }
      else
      {
        await Task.Delay(100);
        FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
        Keyboard.Focus((IInputElement) element);
      }
      if (!element._popup.IsOpen)
        return;
      HwndHelper.SetFocus((UIElement) element.Detail);
      if (model.IsNewAdd)
        PopupStateManager.OnAddPopupOpened();
      else
        PopupStateManager.OnViewPopupOpened();
    }

    private async Task TryShowWindow(
      TaskDetailViewModel model,
      string itemId,
      System.Windows.Point point,
      bool show = true)
    {
      this.Detail.ScrollToTop();
      this.Detail.Navigate(model, itemId);
      this.Control.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      this.Control.Opacity = 1.0;
      if (!show)
        return;
      this.TryShow();
    }

    private void InitDetailEvents()
    {
      this.Detail.ShowDialog -= new EventHandler(this.OnStartOperation);
      this.Detail.ShowDialog += new EventHandler(this.OnStartOperation);
      this.Detail.CloseDialog -= new EventHandler<bool>(this.OnStopOperation);
      this.Detail.CloseDialog += new EventHandler<bool>(this.OnStopOperation);
      this.Detail.EnterImmersive -= new EnterImmersiveDelegate(this.OnEnterImmersive);
      this.Detail.EnterImmersive += new EnterImmersiveDelegate(this.OnEnterImmersive);
      this.Detail.TagClick -= new EventHandler<string>(this.OnTagClick);
      this.Detail.TagClick += new EventHandler<string>(this.OnTagClick);
      this.Detail.NavigateTask -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      this.Detail.NavigateTask += new EventHandler<ProjectTask>(this.OnNavigateTask);
      this.Detail.ForceHideWindow -= new EventHandler(this.OnForceHideWindow);
      this.Detail.ForceHideWindow += new EventHandler(this.OnForceHideWindow);
    }

    private void OnForceHideWindow(object sender, EventArgs e) => this._popup.IsOpen = false;

    private void OnEnterImmersive(string id, int caret)
    {
      this._popup.IsOpen = false;
      App.Window?.OnEnterImmersive(id, caret);
    }

    private void OnMainWindowHidden(object sender, EventArgs e) => this._popup.IsOpen = false;

    private void OnTagClick(object sender, string e) => this.Close();

    private async Task OnClosed()
    {
      TaskDetailPopup sender = this;
      if (PopupStateManager.LastTarget == sender.Target)
        PopupStateManager.LastTarget = (UIElement) null;
      if (TaskDetailPopup._lastPopup == sender)
        TaskDetailPopup._lastPopup = (TaskDetailPopup) null;
      sender.Detail.TryClosePopup();
      sender.DependentWindow = (IToastShowWindow) null;
      sender.Target = (UIElement) null;
      sender.Detail.ClearEvents();
      EventHandler<string> disappear = sender.Disappear;
      if (disappear != null)
        disappear((object) sender, sender._taskId);
      await sender.TrySaveTask();
    }

    private async void OnStopOperation(object sender, bool e)
    {
      TaskDetailPopup taskDetailPopup = this;
      if (!taskDetailPopup.Detail.PopupShowing)
      {
        await Task.Delay(100);
        if ((DateTime.Now - taskDetailPopup._operationTime).TotalMilliseconds < 100.0)
          return;
        if (e && !ModifyRepeatDialog.Showing)
        {
          // ISSUE: explicit non-virtual call
          __nonvirtual (taskDetailPopup.Focus());
        }
      }
      taskDetailPopup._popup.StaysOpen = false;
    }

    private void OnStartOperation(object sender, EventArgs e)
    {
      this._popup.StaysOpen = true;
      this._operationTime = DateTime.Now;
    }

    public async Task Close(bool needActivateOwner = true) => this._popup.IsOpen = false;

    public async Task TrySaveTask(bool force = false)
    {
      TaskDetailPopup sender = this;
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
      if (!(this.Detail.DataContext is TaskDetailViewModel dataContext) || !dataContext.IsNewAdd)
        return;
      this.Close();
    }

    public async void ReLoad()
    {
      if (!(this.Detail.DataContext is TaskDetailViewModel model))
      {
        model = (TaskDetailViewModel) null;
      }
      else
      {
        await Task.Delay(100);
        this.Detail.Navigate(model.TaskId);
        model = (TaskDetailViewModel) null;
      }
    }

    private async Task Navigate(ProjectTask projectTask, bool isBack = false, bool canBack = true)
    {
      if (await TaskUtils.TryLoadTask(projectTask.TaskId, projectTask.ProjectId, isBack) == null)
        return;
      if (canBack)
        this.SetNavigateEnabled();
      this.Detail.Navigate(projectTask.TaskId);
    }

    public void ReLoad(string taskId, string itemId, TaskModifyType type)
    {
      if (!(taskId == this.Detail.TaskId))
        return;
      this.ReLoad();
    }

    private void OnWindowKeyUp(object sender, KeyEventArgs e)
    {
      if (!object.Equals((object) FocusManager.GetFocusedElement((DependencyObject) this), (object) this))
        return;
      switch (e.Key)
      {
        case Key.Return:
          if (!object.Equals((object) this, (object) Keyboard.FocusedElement))
            break;
          this.Detail.TryFocusTitle();
          break;
        case Key.Escape:
          if (this.Detail.PopupShowing)
            break;
          this.Close();
          break;
        case Key.Delete:
          if (!object.Equals((object) this, (object) Keyboard.FocusedElement))
            break;
          this.Detail.OnDelete();
          break;
      }
    }

    public void Clear()
    {
      this._popup.IsOpen = false;
      if (PopupStateManager.LastTarget == this.Target)
        PopupStateManager.LastTarget = (UIElement) null;
      if (TaskDetailPopup._lastPopup == this)
        TaskDetailPopup._lastPopup = (TaskDetailPopup) null;
      this.DependentWindow = (IToastShowWindow) null;
      this.Target = (UIElement) null;
      EventHandler<string> disappear = this.Disappear;
      if (disappear != null)
        disappear((object) this, this._taskId);
      PopupStateManager.OnViewPopupClosed();
    }

    public bool TryShow()
    {
      this._popup.IsOpen = true;
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
      if (TaskDetailPopup._navigateWindow == null)
      {
        TaskDetailPopup._navigateWindow = new TaskDetailWindow();
        TaskDetailPopup._navigateWindow.ShowInNavigate(task.TaskId, window as Window);
        TaskDetailPopup._navigateWindow.DependentWindow = window;
        TaskDetailPopup._navigateWindow.Closed += (EventHandler) ((o, e) => TaskDetailPopup._navigateWindow = (TaskDetailWindow) null);
      }
      else
        TaskDetailPopup._navigateWindow.OnNavigateTask(task);
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
      AddKeyBinding("OpenSticky", GetKeyBinding(DetailPopupCommands.OpenAsSticky));
      AddKeyBinding("CompleteTask", GetKeyBinding(DetailPopupCommands.ToggleCompleted));

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

    public void ToggleCompleted() => this.Detail.ToggleTaskCompleted();

    public static void SetCanClose(bool canClose)
    {
      if (TaskDetailPopup._lastPopup?._popup == null)
        return;
      TaskDetailPopup._lastPopup._popup.StaysOpen = !canClose;
    }

    public static void TryCloseWindow()
    {
      if (TaskDetailPopup._lastPopup?._popup == null)
        return;
      TaskDetailPopup._lastPopup._popup.IsOpen = false;
    }

    public bool IsFocus()
    {
      if (Keyboard.FocusedElement == null)
        return false;
      return object.Equals((object) Keyboard.FocusedElement, (object) this) || Utils.FindParent<TaskDetailPopup>((DependencyObject) (Keyboard.FocusedElement as UIElement)) != null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskdetailpopup.xaml", UriKind.Relative));
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
          this.Control = (TaskDetailPopup) target;
          break;
        case 2:
          this.Detail = (TaskDetailPopupView) target;
          break;
        case 3:
          this.ToastGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
