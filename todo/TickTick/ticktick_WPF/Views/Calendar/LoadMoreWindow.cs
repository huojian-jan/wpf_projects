// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.LoadMoreWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
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
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Undo;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class LoadMoreWindow : MyWindow, IDragBarEvent, IToastShowWindow, IComponentConnector
  {
    private readonly IToastShowWindow _handler;
    private readonly bool _isLastRow;
    private List<CalendarDisplayModel> _models;
    private bool _closing;
    private FrameworkElement _context;
    internal ContentControl Container;
    internal TextBlock Title;
    internal StackPanel TaskPanel;
    private bool _contentLoaded;

    public LoadMoreWindow(
      DateTime date,
      FrameworkElement element,
      List<CalendarDisplayModel> models,
      bool isLastRow = true,
      string themeId = "",
      IToastShowWindow undo = null,
      bool locked = false)
    {
      if (!string.IsNullOrEmpty(themeId))
        ThemeUtil.SetTheme(themeId, (FrameworkElement) this);
      this.Date = date;
      this._context = element;
      this._models = models;
      this._isLastRow = isLastRow;
      this._handler = undo;
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      this.InitializeComponent();
      if (!locked)
        return;
      this.TaskPanel.IsEnabled = false;
    }

    private DateTime Date { get; }

    public IDragBarEvent DragBarEvent { get; set; }

    public bool OnSelection() => false;

    public void TryHideToast()
    {
    }

    public void OnDragStart(CalendarDisplayViewModel model, MouseEventArgs e, bool fromArrange)
    {
      this.DragBarEvent?.OnDragStart(model, e, false);
    }

    public void TaskDeleted(string taskId) => this._handler?.TaskDeleted(taskId);

    public void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels)
    {
      this._handler?.ToastDeleteRecUndo(undoModels);
    }

    public void Toast(FrameworkElement uiElement) => this._handler?.Toast(uiElement);

    public void ToastMoveProjectControl(string projectId, string taskName = null, MoveToastType moveType = MoveToastType.Move)
    {
      this._handler?.ToastMoveProjectControl(projectId, taskName);
    }

    public void TryToastString(object obj, string toastString)
    {
      this._handler?.TryToastString(obj, toastString);
    }

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks)
    {
      return await this._handler?.BatchDeleteTask(tasks);
    }

    public void TaskComplete(CloseUndoToast undo) => this._handler?.TaskComplete(undo);

    public event EventHandler Disappear;

    private void UnbindEvents()
    {
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      PomoNotifier.Changed -= new EventHandler<PomoChangeArgs>(this.OnTaskChanged);
    }

    private void BindEvents()
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      PomoNotifier.Changed += new EventHandler<PomoChangeArgs>(this.OnTaskChanged);
    }

    private async void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      if (e.DeletedChangedIds.Any() || e.UndoDeletedIds.Any())
      {
        this._models = await CalendarDisplayService.GetDisplayModelInDay(this.Date);
        this.LoadTasks(this._models);
      }
      if (e.StatusChangedIds.Any())
      {
        if (!LocalSettings.Settings.ShowCompletedInCal)
          this._models = await CalendarDisplayService.GetDisplayModelInDay(this.Date);
        this.LoadTasks(this._models);
      }
      if (e.DateChangedIds.Any())
      {
        this._models = await CalendarDisplayService.GetDisplayModelInDay(this.Date);
        this.LoadTasks(this._models);
      }
      if (!e.CheckItemChangedIds.Any())
        return;
      this._models = await CalendarDisplayService.GetDisplayModelInDay(this.Date);
      this.LoadTasks(this._models);
    }

    private void TryClose()
    {
      if (this._closing)
        return;
      this._closing = true;
      this.Close();
    }

    private async void OnTaskChanged(object sender, object e)
    {
      this._models = await CalendarDisplayService.GetDisplayModelInDay(this.Date);
      this.LoadTasks(this._models);
    }

    public async Task ShowPopup()
    {
      LoadMoreWindow loadMoreWindow = this;
      loadMoreWindow.Title.Text = loadMoreWindow.Date.ToString("yyyy-MM-dd");
      loadMoreWindow.LoadTasks(loadMoreWindow._models);
      loadMoreWindow.Show();
    }

    private void LoadTasks(List<CalendarDisplayModel> models)
    {
      this.TaskPanel.Children.Clear();
      models.Sort(new Comparison<CalendarDisplayModel>(GridGeoAssembler.CompareTasks));
      foreach (CalendarDisplayModel model1 in models)
      {
        CalendarDisplayViewModel model2 = CalendarDisplayViewModel.Build(model1);
        DateTime? nullable = model2.DueDate;
        if (nullable.HasValue)
        {
          nullable = model2.StartDate;
          if (nullable.HasValue)
          {
            nullable = model2.DueDate;
            DateTime date1 = nullable.Value;
            DateTime date2 = date1.Date;
            date1 = this.Date;
            DateTime date3 = date1.Date;
            if (date2 == date3)
            {
              nullable = model2.StartDate;
              date1 = nullable.Value;
              DateTime date4 = date1.Date;
              date1 = this.Date;
              DateTime date5 = date1.Date;
              if (date4 != date5)
              {
                nullable = model2.DueDate;
                DateTime dateTime = nullable.Value;
                nullable = model2.DueDate;
                date1 = nullable.Value;
                DateTime date6 = date1.Date;
                if ((dateTime - date6).TotalHours <= 5.0)
                  continue;
              }
            }
          }
        }
        this.AddTaskBar(model2);
      }
    }

    private void AddTaskBar(CalendarDisplayViewModel model)
    {
      TaskBar taskBar = new TaskBar();
      taskBar.HorizontalAlignment = HorizontalAlignment.Stretch;
      TaskBar element = taskBar;
      this.TaskPanel.Children.Add((UIElement) element);
      element.DataContext = (object) model;
      element.PopupClosed -= new EventHandler(this.OnPopupClosed);
      element.PopupClosed += new EventHandler(this.OnPopupClosed);
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      System.Windows.Point position = Mouse.GetPosition((IInputElement) this);
      if (position.X > this.ActualWidth - 10.0 || position.X < 10.0 || position.Y < 10.0 || position.Y > this.ActualHeight - 10.0)
        this.Close();
      else
        this.Activate();
    }

    protected override async void OnDeactivated(EventArgs e)
    {
      LoadMoreWindow relativeTo = this;
      // ISSUE: reference to a compiler-generated method
      relativeTo.\u003C\u003En__0(e);
      await Task.Delay(10);
      System.Windows.Point position = Mouse.GetPosition((IInputElement) relativeTo);
      if (position.X <= relativeTo.ActualWidth - 10.0 && position.X >= 10.0 && position.Y >= 10.0 && position.Y <= relativeTo.ActualHeight - 10.0 || relativeTo._closing)
        return;
      relativeTo._closing = true;
      try
      {
        relativeTo.Close();
      }
      catch (Exception ex)
      {
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      EventHandler disappear = this.Disappear;
      if (disappear != null)
        disappear((object) this, (EventArgs) null);
      base.OnClosing(e);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) => this.SetWindowLocation();

    private async Task SetWindowLocation()
    {
      LoadMoreWindow loadMoreWindow = this;
      Window parentWindow = Utils.GetParentWindow((DependencyObject) loadMoreWindow._context);
      if (parentWindow == null)
        return;
      System.Windows.Point point = loadMoreWindow._context.TranslatePoint(new System.Windows.Point(0.0, loadMoreWindow._context.ActualHeight), (UIElement) parentWindow);
      double num1 = parentWindow.GetActualLeft() + point.X - 4.0;
      double num2 = parentWindow.GetActualTop() + point.Y - loadMoreWindow.ActualHeight - 6.0;
      double left = num1;
      double top = num2;
      loadMoreWindow.Left = left;
      loadMoreWindow.Top = top;
      WindowHelper.MoveTo((Window) loadMoreWindow, (int) left, (int) top);
      loadMoreWindow.Left = left;
      loadMoreWindow.Top = top;
    }

    public void RemoveItem(TaskBar taskBar)
    {
      if (taskBar == null)
        return;
      this.TaskPanel.Children.Remove((UIElement) taskBar);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/loadmorewindow.xaml", UriKind.Relative));
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
          this.Container = (ContentControl) target;
          break;
        case 2:
          this.Title = (TextBlock) target;
          break;
        case 3:
          this.TaskPanel = (StackPanel) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
