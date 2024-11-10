// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.MultiDayAllDayView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar.Month;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class MultiDayAllDayView : Grid
  {
    public DateTime ScrollStartDate;
    private MultiDayView _parent;
    private List<AllDayView> _dayViews;
    private ScrollViewer _scroll;
    private List<WeekEventModel> _events;
    private MultiDayAllDayItems _itemsContainer;
    private bool _mouseOnItem;
    private bool _dragging;
    private DateTime _dragStartDate;
    private DateTime _currentDragDate;

    public double CurrentOffset
    {
      get
      {
        ScrollViewer scroll = this._scroll;
        return scroll == null ? 4000.0 : scroll.HorizontalOffset;
      }
    }

    public event EventHandler<int> MoveNextOrLast;

    public MultiDayAllDayView(MultiDayView parent)
    {
      ScrollViewer scrollViewer = new ScrollViewer();
      scrollViewer.Margin = new Thickness(0.0, 69.0, 0.0, 0.0);
      scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
      scrollViewer.CanContentScroll = false;
      this._scroll = scrollViewer;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this._parent = parent;
      this.Margin = new Thickness(60.0, 0.0, 0.0, 0.0);
      this.ClipToBounds = true;
      this._scroll.SetResourceReference(FrameworkElement.StyleProperty, (object) "for_scrollviewer");
      this._scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.Children.Add((UIElement) this._scroll);
      ContentControl contentControl1 = new ContentControl();
      contentControl1.Width = 8000.0;
      ContentControl contentControl2 = contentControl1;
      MultiDayAllDayItems multiDayAllDayItems = new MultiDayAllDayItems(parent);
      multiDayAllDayItems.VerticalAlignment = VerticalAlignment.Top;
      multiDayAllDayItems.RenderTransform = (Transform) new TranslateTransform()
      {
        X = 0.0
      };
      this._itemsContainer = multiDayAllDayItems;
      this._itemsContainer.Height = 200.0;
      contentControl2.Content = (object) this._itemsContainer;
      this._scroll.Content = (object) contentControl2;
      this._scroll.SetValue(Panel.ZIndexProperty, (object) 10);
      this._scroll.ScrollToHorizontalOffset(4000.0);
      this._scroll.SizeChanged += new SizeChangedEventHandler(this.OnScrollSizeChanged);
      this._scroll.ScrollChanged += new ScrollChangedEventHandler(this.OnScrollChanged);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnCellMouseDown);
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCellMouseUp);
      this.PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      await Task.Delay(50);
      if (Math.Abs(this._scroll.HorizontalOffset - 4000.0) <= 1.0)
        return;
      this._scroll.ScrollToHorizontalOffset(4000.0);
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      this._itemsContainer.SetScrollOffset(this._scroll.VerticalOffset, true);
    }

    private void OnScrollSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this._itemsContainer.SetSize(e.NewSize);
    }

    public void SetData(List<MonthDayViewModel> models)
    {
      MultiDayView parent = this._parent;
      double num = parent != null ? parent.DayViewWidth : 0.0;
      for (int index = 0; index < models.Count; ++index)
      {
        if (this._dayViews.Count > index)
        {
          AllDayView dayView = this._dayViews[index];
          dayView.DataContext = (object) models[index];
          dayView.RenderTransform = (Transform) new TranslateTransform()
          {
            X = ((double) (index - 1) * num)
          };
        }
      }
    }

    public void SetEvents(List<WeekEventModel> events, bool resetOffset = true)
    {
      this._events = events != null ? events.ToList<WeekEventModel>() : (List<WeekEventModel>) null;
      if (resetOffset)
      {
        MultiDayView parent = this._parent;
        double dayWidth = parent != null ? parent.DayViewWidth : 0.0;
        if (this._itemsContainer.RenderTransform is TranslateTransform renderTransform1)
        {
          renderTransform1.X = 0.0;
          for (int index = 0; index < this._dayViews.Count; ++index)
          {
            if (this._dayViews[index].RenderTransform is TranslateTransform renderTransform)
              renderTransform.X = (double) (index - 1) * dayWidth;
          }
        }
        this._itemsContainer.SetScrollOffset(this._scroll.VerticalOffset);
        this._itemsContainer.SetSize(this._scroll.RenderSize);
        this._itemsContainer.SetDayWidth(dayWidth);
      }
      this._itemsContainer.SetItemsSource(events);
    }

    public void TryShowAddWindow(DateTime dateTime)
    {
      foreach (AllDayView dayView in this._dayViews)
      {
        if (dayView.DataContext is MonthDayViewModel dataContext && dataContext.Date == dateTime)
        {
          dataContext.State = MonthCellState.Selected;
          this._dragStartDate = dateTime;
          this._currentDragDate = dateTime;
          this.ShowAddTaskPopup(dayView);
          break;
        }
      }
    }

    public void SetDisplayDays()
    {
      MultiDayView parent1 = this._parent;
      int num1 = (parent1 != null ? parent1.DisplayDays : 7) + 2;
      MultiDayView parent2 = this._parent;
      double num2 = parent2 != null ? parent2.DayViewWidth : 0.0;
      if (num1 > this._dayViews.Count)
      {
        int count = this._dayViews.Count;
        for (int index = 0; index < num1 - count; ++index)
        {
          AllDayView allDayView = new AllDayView();
          allDayView.RenderTransform = (Transform) new TranslateTransform();
          allDayView.Width = num2;
          allDayView.HorizontalAlignment = HorizontalAlignment.Left;
          AllDayView element = allDayView;
          this._dayViews.Add(element);
          this.Children.Add((UIElement) element);
        }
      }
      else
      {
        List<AllDayView> allDayViewList = new List<AllDayView>();
        for (int index = num1; index < this._dayViews.Count; ++index)
          allDayViewList.Add(this._dayViews[index]);
        allDayViewList.ForEach((Action<AllDayView>) (d =>
        {
          this._dayViews.Remove(d);
          this.Children.Remove((UIElement) d);
        }));
      }
      for (int index = 0; index < this._dayViews.Count; ++index)
      {
        AllDayView dayView = this._dayViews[index];
        dayView.RenderTransform = (Transform) new TranslateTransform()
        {
          X = ((double) (index - 1) * num2)
        };
        dayView.Width = num2;
      }
    }

    public void ScrollToTop() => this._scroll?.ScrollToTop();

    public void NotifyEventTitleChanged(string eventId, string text)
    {
      List<WeekEventModel> events = this._events;
      if (events == null)
        return;
      events.Where<WeekEventModel>((Func<WeekEventModel, bool>) (model => model.Data.EventId == eventId)).ToList<WeekEventModel>().ForEach((Action<WeekEventModel>) (item => item.Data.SourceViewModel.Title = text));
    }

    public void SetDragStart(DateTime date)
    {
      this._dragging = true;
      this._dragStartDate = date;
    }

    public bool IsDragging() => this._dragging;

    public DateTime GetDragStart() => this._dragStartDate;

    public DateTime GetCurrentDrag() => this._currentDragDate;

    public void SetCurrentDragDate(DateTime date)
    {
      if (!this._dragging)
        return;
      this._currentDragDate = date;
      this.RenderSelected();
    }

    private void RenderSelected()
    {
      DateTime dateTime1 = this._dragStartDate;
      DateTime dateTime2 = this._dragStartDate;
      if (!Utils.IsEmptyDate(this._currentDragDate))
      {
        dateTime1 = this._currentDragDate.Date > this._dragStartDate ? this._dragStartDate : this._currentDragDate;
        dateTime2 = this._currentDragDate.Date < this._dragStartDate ? this._dragStartDate : this._currentDragDate;
      }
      foreach (FrameworkElement dayView in this._dayViews)
      {
        if (dayView.DataContext is MonthDayViewModel dataContext)
        {
          DateTime date = dataContext.Date;
          dataContext.State = !(date.Date >= dateTime1) || !(dataContext.Date <= dateTime2) ? MonthCellState.Normal : MonthCellState.Selected;
        }
      }
    }

    public void CancelDrag()
    {
      this._dragging = false;
      foreach (FrameworkElement dayView in this._dayViews)
      {
        if (dayView.DataContext is MonthDayViewModel dataContext)
          dataContext.State = MonthCellState.Normal;
      }
    }

    private void OnCellMouseDown(object sender, MouseButtonEventArgs e)
    {
      MultiDayView parent = this._parent;
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0 || Utils.GetMousePointVisibleItem<TaskBar>((MouseEventArgs) e, (FrameworkElement) this) != null)
        return;
      if (this._scroll.IsMouseOver)
        this._scroll.IsHitTestVisible = false;
      AllDayView pointVisibleItem = Utils.GetMousePointVisibleItem<AllDayView>((MouseEventArgs) e, (FrameworkElement) this);
      if (pointVisibleItem == null || !PopupStateManager.CanShowAddPopup() || pointVisibleItem.IsDateMouseOver)
        return;
      this.MouseMove += new MouseEventHandler(this.OnCellMouseMove);
      this.CaptureMouse();
      e.Handled = true;
      this.SetDragStart(pointVisibleItem.GetDate());
      this.SetCurrentDragDate(new DateTime());
      PopupStateManager.StartSelection();
    }

    private void OnCellMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.IsMouseCaptured)
        this.ReleaseMouseCapture();
      this._scroll.IsHitTestVisible = true;
      this.MouseMove -= new MouseEventHandler(this.OnCellMouseMove);
      if (!PopupStateManager.IsInSelection() || !PopupStateManager.CanShowAddPopup())
        return;
      AllDayView pointVisibleItem = Utils.GetMousePointVisibleItem<AllDayView>((MouseEventArgs) e, (FrameworkElement) this);
      if (pointVisibleItem != null && pointVisibleItem.DataContext is MonthDayViewModel dataContext1 && (dataContext1.State == MonthCellState.Selected || dataContext1.Date == this._dragStartDate))
      {
        e.Handled = true;
        dataContext1.State = MonthCellState.Selected;
        this.ShowAddTaskPopup(pointVisibleItem);
      }
      else
      {
        for (int index = this._dayViews.Count - 1; index >= 0; --index)
        {
          AllDayView dayView = this._dayViews[index];
          if (dayView.DataContext is MonthDayViewModel dataContext && dataContext.State == MonthCellState.Selected)
          {
            this.ShowAddTaskPopup(dayView);
            e.Handled = true;
            break;
          }
        }
      }
    }

    private void OnCellMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed && PopupStateManager.IsInSelection())
        PopupStateManager.StopSelection();
      if (!PopupStateManager.IsInSelection() || !PopupStateManager.CanShowAddPopup())
        return;
      AllDayView pointVisibleItem = Utils.GetMousePointVisibleItem<AllDayView>(e, (FrameworkElement) this);
      if (pointVisibleItem == null)
        return;
      this.SetCurrentDragDate(pointVisibleItem.GetDate());
    }

    private async void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (!PopupStateManager.CanShowAddPopup())
        e.Handled = true;
      if (!Utils.IfShiftPressed())
        return;
      this._parent?.OnShiftMouseWheel(e.Delta);
      e.Handled = true;
    }

    public void ShowAddTaskPopup(AllDayView dayView)
    {
      if (!PopupStateManager.CanShowAddPopup())
        return;
      TaskDetailViewModel model = TaskDetailViewModel.BuildInitModel((TaskBaseViewModel) null);
      DateTime dragStartDate = this._dragStartDate;
      DateTime currentDragDate = this._currentDragDate;
      if (dragStartDate == currentDragDate)
        model.SourceViewModel.StartDate = new DateTime?(dragStartDate);
      else if (dragStartDate > currentDragDate)
      {
        model.SourceViewModel.StartDate = new DateTime?(currentDragDate);
        model.SourceViewModel.DueDate = new DateTime?(dragStartDate.Date.AddDays(1.0));
      }
      else
      {
        model.SourceViewModel.StartDate = new DateTime?(dragStartDate);
        model.SourceViewModel.DueDate = new DateTime?(currentDragDate.Date.AddDays(1.0));
      }
      string defaultAddProjectId = CalendarUtils.GetCalendarDefaultAddProjectId();
      model.SourceViewModel.ProjectId = defaultAddProjectId;
      TaskDetailPopup taskDetailPopup = new TaskDetailPopup();
      IToastShowWindow parentWindow = this.GetParentWindow();
      if (parentWindow != null)
      {
        if (PopupStateManager.LastTarget == this)
          return;
        taskDetailPopup.DependentWindow = parentWindow;
      }
      taskDetailPopup.Disappear -= new EventHandler<string>(this.OnDetailClosed);
      taskDetailPopup.Disappear += new EventHandler<string>(this.OnDetailClosed);
      taskDetailPopup.TaskSaved += new EventHandler<string>(this.OnTaskAdded);
      taskDetailPopup.Show(model, string.Empty, new TaskWindowDisplayArgs((UIElement) dayView, dayView.ActualWidth, PopupLocationCalculator.GetMousePoint(dayView.ActualWidth < 350.0), 0.0));
      this._parent?.SetEditing(true);
    }

    private void OnDetailClosed(object sender, string e)
    {
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.Disappear -= new EventHandler<string>(this.OnDetailClosed);
      foreach (FrameworkElement dayView in this._dayViews)
      {
        if (dayView.DataContext is MonthDayViewModel dataContext)
          dataContext.State = MonthCellState.Normal;
      }
      this.CancelDrag();
      this._parent?.SetEditing(false);
    }

    private async void OnTaskAdded(object sender, string taskId)
    {
      MultiDayAllDayView multiDayAllDayView = this;
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.TaskSaved -= new EventHandler<string>(multiDayAllDayView.OnTaskAdded);
      string e = await CalendarUtils.CheckAddTaskCanShown(taskId);
      if (string.IsNullOrEmpty(e))
        return;
      multiDayAllDayView.GetParentWindow()?.TryToastString((object) null, e);
    }

    private IToastShowWindow GetParentWindow()
    {
      return Utils.FindParent<IToastShowWindow>((DependencyObject) this);
    }

    public void ClearDragging()
    {
      this._events.ForEach((Action<WeekEventModel>) (e => e.Data.Dragging = false));
    }

    public void ClearItems(bool onlyData)
    {
      this._itemsContainer.Clear();
      this._events?.Clear();
    }

    public void OnTouchScroll(double offset, bool showWeekend)
    {
      if (this._itemsContainer.RenderTransform is TranslateTransform renderTransform1)
      {
        renderTransform1.X += offset;
        this._itemsContainer.SetScrollOffset(this._scroll.VerticalOffset, true);
      }
      bool flag = false;
      MultiDayView parent1 = this._parent;
      double num1 = parent1 != null ? parent1.DayViewWidth : this.ActualWidth;
      MultiDayView parent2 = this._parent;
      int displayDays = parent2 != null ? parent2.DisplayDays : 0;
      foreach (AllDayView dayView in this._dayViews)
      {
        if (dayView.RenderTransform is TranslateTransform renderTransform2)
        {
          double num2 = renderTransform2.X + offset;
          DateTime date1 = dayView.GetDate();
          DateTime date2 = date1;
          if (num2 <= -1.5 * num1)
          {
            num2 += (double) (displayDays + 2) * num1;
            flag = true;
            date2 = date1.AddDays((double) (displayDays + 2));
            if (!showWeekend)
              date2 = date2.AddDays(date1.DayOfWeek == DayOfWeek.Friday || date1.DayOfWeek == DayOfWeek.Thursday ? 4.0 : 2.0);
          }
          if (num2 >= ((double) displayDays + 0.5) * num1 + 2.0)
          {
            num2 -= (double) (displayDays + 2) * num1;
            flag = true;
            date2 = date1.AddDays((double) (-2 - displayDays));
            if (!showWeekend)
              date2 = date2.AddDays(date1.DayOfWeek == DayOfWeek.Monday || date1.DayOfWeek == DayOfWeek.Tuesday ? -4.0 : -2.0);
          }
          if (date2 != date1 && dayView.DataContext is MonthDayViewModel dataContext)
            dataContext.SetDate(date2);
          dayView.Offset = num2;
          renderTransform2.X = num2;
        }
      }
      if (!flag)
        return;
      this._dayViews.Sort((Comparison<AllDayView>) ((a, b) => a.Offset.CompareTo(b.Offset)));
      DateTime? startDate = this.GetStartDate();
      if (!startDate.HasValue)
        return;
      DateTime currentMonthDate = DateUtils.GetCurrentMonthDate(startDate.Value, startDate.Value.AddDays((double) (displayDays - 1)));
      foreach (AllDayView dayView in this._dayViews)
        dayView.SetMonth(currentMonthDate);
    }

    public DateTime? GetStartDate()
    {
      return this._dayViews.Count > 1 ? new DateTime?(this._dayViews[1].GetDate()) : new DateTime?();
    }

    public double GetCurrentOffset()
    {
      return this._itemsContainer.RenderTransform is TranslateTransform renderTransform ? -1.0 * renderTransform.X : 0.0;
    }

    public void SetScrollMaxHeight(double newSizeHeight)
    {
      this._scroll.MaxHeight = newSizeHeight - 70.0;
    }
  }
}
