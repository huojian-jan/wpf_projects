// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Month.MultiWeekDayControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Month
{
  public class MultiWeekDayControl : Grid
  {
    private static readonly SolidColorBrush _jpHolidayColor = ThemeUtil.GetColorInString("#FF0000");
    private static readonly SolidColorBrush _holidayColor = ThemeUtil.GetColorInString("#5DCA94");
    private bool _pressed;
    private MultiWeekControl _parent;
    private IToastShowWindow _parentWindow;
    private Path _path;
    private Image _image;
    private TextBlock _dateText;
    private TextBlock _lunarText;
    private Border _flashBorder;

    public MultiWeekDayControl()
    {
      Path path = new Path();
      path.Height = 24.0;
      path.Margin = new Thickness(2.0, 2.0, 0.0, 0.0);
      path.HorizontalAlignment = HorizontalAlignment.Left;
      path.VerticalAlignment = VerticalAlignment.Top;
      path.Cursor = Cursors.Hand;
      this._path = path;
      this._path.SetBinding(FrameworkElement.WidthProperty, "IconWidth");
      this._path.MouseEnter += new MouseEventHandler(this.OnPathMouseOverChanged);
      this._path.MouseLeave += new MouseEventHandler(this.OnPathMouseOverChanged);
      this._path.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDateClick);
      this._path.SetBinding(Path.DataProperty, "EllipseGeometry");
      this.Children.Add((UIElement) this._path);
      Image image = new Image();
      image.Width = 12.0;
      image.Height = 12.0;
      image.HorizontalAlignment = HorizontalAlignment.Left;
      image.VerticalAlignment = VerticalAlignment.Top;
      this._image = image;
      this.Children.Add((UIElement) this._image);
      TextBlock textBlock1 = new TextBlock();
      textBlock1.IsHitTestVisible = false;
      textBlock1.Width = 28.0;
      textBlock1.TextAlignment = TextAlignment.Center;
      textBlock1.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock1.VerticalAlignment = VerticalAlignment.Top;
      this._dateText = textBlock1;
      this._dateText.SetResourceReference(TextBlock.FontSizeProperty, (object) "MonthDayTextFontSize");
      this._dateText.SetResourceReference(FrameworkElement.MarginProperty, (object) "MonthDayTextMargin");
      this._dateText.SetBinding(FrameworkElement.WidthProperty, "IconWidth");
      this.Children.Add((UIElement) this._dateText);
      TextBlock textBlock2 = new TextBlock();
      textBlock2.FontSize = 11.0;
      textBlock2.IsHitTestVisible = false;
      textBlock2.Margin = new Thickness(30.0, 7.0, 8.0, 0.0);
      textBlock2.HorizontalAlignment = HorizontalAlignment.Right;
      textBlock2.VerticalAlignment = VerticalAlignment.Top;
      textBlock2.TextAlignment = TextAlignment.Left;
      this._lunarText = textBlock2;
      this.Children.Add((UIElement) this._lunarText);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.MouseEnter += new MouseEventHandler(this.OnCellMouseEnter);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnCellLeftDown);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCellLeftUp);
    }

    private void OnDateClick(object sender, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0 || !(this.DataContext is MonthDayViewModel dataContext))
        return;
      e.Handled = true;
      Utils.FindParent<CalendarControl>((DependencyObject) this)?.NavigateDate(dataContext.Date, CalendarDisplayMode.Week);
    }

    private void OnPathMouseOverChanged(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is MonthDayViewModel dataContext) || dataContext.IsToday)
        return;
      if (this._path.IsMouseOver)
        this._path.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor10");
      else
        this._path.Fill = (Brush) Brushes.Transparent;
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is MonthDayViewModel oldValue)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChanged), "");
      if (!(e.NewValue is MonthDayViewModel newValue))
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChanged), "");
      this.SetData(newValue);
    }

    private void SetData(MonthDayViewModel model)
    {
      this.SetBackGround(model);
      if (model.IsToday)
      {
        this._dateText.Foreground = (Brush) Brushes.White;
        this._path.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor");
      }
      else
      {
        this._dateText.SetResourceReference(TextBlock.ForegroundProperty, model.IsCurrentMonth ? (object) "BaseColorOpacity100_80" : (object) "BaseColorOpacity40");
        if (this._path.IsMouseOver)
          this._path.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor10");
        else
          this._path.Fill = (Brush) Brushes.Transparent;
      }
      TextBlock dateText = this._dateText;
      DateTime date = model.Date;
      string str;
      if (date.Day != 1)
      {
        date = model.Date;
        str = date.Day.ToString();
      }
      else
        str = DateUtils.FormatShortMonthDay(model.Date);
      dateText.Text = str;
      this._lunarText.Text = DateUtils.GetLunarText(model.Date, LocalSettings.Settings.ShowWeek);
      this._lunarText.Margin = new Thickness(model.IconWidth + 6.0, 7.0, 8.0, 0.0);
      if (HolidayManager.IsHolidayText(this._lunarText.Text))
        this._lunarText.Foreground = Utils.IsJp() ? (Brush) MultiWeekDayControl._jpHolidayColor : (Brush) MultiWeekDayControl._holidayColor;
      else
        this._lunarText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
      if (model.IsRestDay)
        this._image.SetResourceReference(Image.SourceProperty, (object) "RestDrawingImage");
      else if (model.IsWorkDay)
        this._image.SetResourceReference(Image.SourceProperty, (object) "WorkDrawingImage");
      else
        this._image.Source = (ImageSource) null;
      this._image.Opacity = model.WorkRestOpacity;
      if (this._image.Source == null)
        return;
      this._image.Margin = new Thickness(model.IconWidth - 3.0, 1.0, 0.0, 0.0);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "State":
          this.SetBackGround(this.DataContext as MonthDayViewModel);
          break;
        case "Date":
          this.SetData(this.DataContext as MonthDayViewModel);
          break;
        case "IsCurrentMonth":
          if (!(this.DataContext is MonthDayViewModel dataContext))
            break;
          if (dataContext.IsToday)
            this._dateText.Foreground = (Brush) Brushes.White;
          else
            this._dateText.SetResourceReference(TextBlock.ForegroundProperty, dataContext.IsCurrentMonth ? (object) "BaseColorOpacity100_80" : (object) "BaseColorOpacity40");
          this._image.Opacity = dataContext.WorkRestOpacity;
          break;
      }
    }

    private void SetBackGround(MonthDayViewModel model)
    {
      if (model == null || model.State == MonthCellState.Normal)
      {
        if (model != null && model.IsWeekend)
          this.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity2");
        else
          this.Background = (Brush) Brushes.Transparent;
      }
      else
        this.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity5");
    }

    private void OnCellLeftDown(object sender, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0 || this._path.IsMouseOver || !(this.DataContext is MonthDayViewModel dataContext))
        return;
      this.GetParent()?.SetFirstDate(dataContext.Date);
    }

    private void OnCellMouseEnter(object sender, MouseEventArgs e)
    {
      MultiWeekControl parent = this.GetParent();
      if (parent != null && parent.IsDragging() || PopupStateManager.CheckAddPopup())
        return;
      e.Handled = true;
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        if (!(this.DataContext is MonthDayViewModel dataContext) || parent == null)
          return;
        parent.SetSelection(dataContext.Date, this);
      }
      else
        parent?.ClearSelection();
    }

    private async void OnCellLeftUp(object sender, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0)
        return;
      e.Handled = true;
      MultiWeekControl parent = this.GetParent();
      if (parent != null)
      {
        KeyValuePair<DateTime, DateTime> selectDateSpan = parent.GetSelectDateSpan();
        DateTime key = selectDateSpan.Key;
        DateTime stop = selectDateSpan.Value;
        this.ShowAddTaskPopup((MouseEventArgs) e, key, stop);
      }
      else
      {
        if (!this._pressed)
          return;
        this._pressed = false;
        DateTime date = this.GetModel().Date;
        this.ShowAddTaskPopup((MouseEventArgs) e, date, date);
      }
    }

    private MonthDayViewModel GetModel()
    {
      return this.DataContext != null && this.DataContext is MonthDayViewModel dataContext ? dataContext : (MonthDayViewModel) null;
    }

    public void ShowAddTaskPopup(MouseEventArgs e, DateTime start, DateTime stop)
    {
      if (!PopupStateManager.CanShowAddPopup() || !(this.DataContext is MonthDayViewModel dataContext))
        return;
      if (Utils.IsEmptyDate(start))
      {
        start = dataContext.Date;
        stop = dataContext.Date;
      }
      dataContext.State = MonthCellState.Selected;
      TaskDetailViewModel model = TaskDetailViewModel.BuildInitModel((TaskBaseViewModel) null);
      if (start == stop)
      {
        model.SourceViewModel.StartDate = new DateTime?(start);
      }
      else
      {
        model.SourceViewModel.StartDate = new DateTime?(start);
        model.SourceViewModel.DueDate = new DateTime?(stop.Date.AddDays(1.0));
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
      taskDetailPopup.Show(model, string.Empty, new TaskWindowDisplayArgs((UIElement) this, this.ActualWidth, false, 0));
      this.GetParent()?.GetParent()?.SetEditting(true);
    }

    private async void OnTaskAdded(object sender, string taskId)
    {
      MultiWeekDayControl multiWeekDayControl = this;
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.TaskSaved -= new EventHandler<string>(multiWeekDayControl.OnTaskAdded);
      string e = await CalendarUtils.CheckAddTaskCanShown(taskId);
      if (string.IsNullOrEmpty(e))
        return;
      multiWeekDayControl.GetParentWindow()?.TryToastString((object) null, e);
    }

    private IToastShowWindow GetParentWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.FindParent<IToastShowWindow>((DependencyObject) this);
      return this._parentWindow;
    }

    private void OnDetailClosed(object sender, string e)
    {
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.Disappear -= new EventHandler<string>(this.OnDetailClosed);
      if (this.DataContext != null && this.DataContext is MonthDayViewModel dataContext)
        dataContext.State = MonthCellState.Normal;
      this.GetParent()?.ClearSelection();
      this.GetParent()?.GetParent()?.SetEditting(false);
    }

    private CalendarControl GetCalendarParent()
    {
      return Utils.FindParent<CalendarControl>((DependencyObject) this);
    }

    private MultiWeekControl GetParent()
    {
      this._parent = this._parent ?? Utils.FindParent<MultiWeekControl>((DependencyObject) this);
      return this._parent;
    }

    public void ClearDropTarget()
    {
      MonthDayViewModel model = this.GetModel();
      if (model == null || model.State == MonthCellState.Normal)
        return;
      model.State = MonthCellState.Normal;
    }

    public void SetDropTarget()
    {
      MonthDayViewModel model = this.GetModel();
      if (model == null || model.State == MonthCellState.Drop)
        return;
      model.State = MonthCellState.Drop;
    }

    public bool IsDragHover(MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      double x = position.X;
      double y = position.Y;
      return x > 0.0 && x < this.ActualWidth && y > 0.0 && y < this.ActualHeight;
    }

    public async void ShowLoadMore(MouseEventArgs e, bool locked)
    {
      await this.ShowLoadMoreWindow(e, locked);
    }

    private async Task ShowLoadMoreWindow(MouseEventArgs e, bool locked)
    {
      MultiWeekDayControl multiWeekDayControl = this;
      if (!PopupStateManager.CanShowLoadMorePopup())
        return;
      MonthDayViewModel model = multiWeekDayControl.GetModel();
      if (model != null)
      {
        List<CalendarDisplayModel> displayModelInDay = await CalendarDisplayService.GetDisplayModelInDay(model.Date, true);
        LoadMoreWindow loadMoreWindow = new LoadMoreWindow(model.Date, (FrameworkElement) multiWeekDayControl, displayModelInDay, themeId: multiWeekDayControl.GetWidgetThemeId(), undo: Utils.FindParent<IToastShowWindow>((DependencyObject) multiWeekDayControl), locked: locked);
        loadMoreWindow.Width = multiWeekDayControl.ActualWidth + 20.0;
        loadMoreWindow.DragBarEvent = (IDragBarEvent) multiWeekDayControl.GetParent();
        loadMoreWindow.Disappear -= new EventHandler(multiWeekDayControl.OnLoadMoreClosed);
        loadMoreWindow.Disappear += new EventHandler(multiWeekDayControl.OnLoadMoreClosed);
        PopupStateManager.OnLoadMorePopupOpened();
        multiWeekDayControl.GetParent()?.GetParent()?.SetEditting(true);
        loadMoreWindow.ShowPopup();
      }
      model = (MonthDayViewModel) null;
    }

    private string GetWidgetThemeId()
    {
      CalendarWidget parent = Utils.FindParent<CalendarWidget>((DependencyObject) this);
      return parent != null ? parent.ThemeId : string.Empty;
    }

    private void OnLoadMoreClosed(object sender, EventArgs e)
    {
      this.GetParent()?.GetParent()?.SetEditting(false);
      PopupStateManager.OnLoadMorePopupClosed();
    }

    public void ClearData()
    {
      this.DataContext = (object) null;
      this._parent = (MultiWeekControl) null;
      this._parentWindow = (IToastShowWindow) null;
    }

    public async Task Flash(bool delay)
    {
      MultiWeekDayControl multiWeekDayControl = this;
      Border border1 = new Border();
      border1.Opacity = 0.0;
      border1.IsHitTestVisible = false;
      Border border = border1;
      multiWeekDayControl.RemoveFlashBorder();
      border.SetResourceReference(Border.BackgroundProperty, (object) "BaseColorOpacity10");
      border.SetValue(Panel.ZIndexProperty, (object) -5);
      multiWeekDayControl._flashBorder = border;
      multiWeekDayControl.Children.Add((UIElement) multiWeekDayControl._flashBorder);
      int i;
      for (i = 0; i < 10; ++i)
      {
        await Task.Delay(delay ? 20 : 5);
        border.Opacity = (double) i / 10.0;
      }
      for (i = 0; i < 25; ++i)
      {
        await Task.Delay(20);
        border.Opacity = 1.0 - (double) i / 25.0;
      }
      multiWeekDayControl.Children.Remove((UIElement) border);
      if (multiWeekDayControl._flashBorder != border)
      {
        border = (Border) null;
      }
      else
      {
        multiWeekDayControl.RemoveFlashBorder();
        border = (Border) null;
      }
    }

    private void RemoveFlashBorder()
    {
      this.Children.Remove((UIElement) this._flashBorder);
      this._flashBorder = (Border) null;
    }
  }
}
