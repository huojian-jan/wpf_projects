// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.AllDayView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar.Month;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class AllDayView : Grid
  {
    private static readonly SolidColorBrush _jpHolidayColor = ThemeUtil.GetColorInString("#FF0000");
    private static readonly SolidColorBrush _holidayColor = ThemeUtil.GetColorInString("#5DCA94");
    private Path _path;
    private Image _image;
    private TextBlock _dateText;
    private TextBlock _lunarText;
    private Border _backBorder;
    private TextBlock _weekDayText;
    public double Offset;

    public AllDayView()
    {
      Border border = new Border();
      border.Margin = new Thickness(0.0, 40.0, 0.0, 0.0);
      border.HorizontalAlignment = HorizontalAlignment.Stretch;
      border.VerticalAlignment = VerticalAlignment.Stretch;
      this._backBorder = border;
      this.Children.Add((UIElement) this._backBorder);
      Path path = new Path();
      path.Width = 24.0;
      path.Height = 24.0;
      path.Margin = new Thickness(4.0, 44.0, 0.0, 0.0);
      path.HorizontalAlignment = HorizontalAlignment.Left;
      path.VerticalAlignment = VerticalAlignment.Top;
      path.Cursor = Cursors.Hand;
      this._path = path;
      this._path.MouseEnter += new MouseEventHandler(this.OnPathMouseOverChanged);
      this._path.MouseLeave += new MouseEventHandler(this.OnPathMouseOverChanged);
      this._path.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDayTextClick);
      this._path.SetBinding(Path.DataProperty, "EllipseGeometry");
      this.Children.Add((UIElement) this._path);
      Image image = new Image();
      image.Width = 12.0;
      image.Height = 12.0;
      image.Margin = new Thickness(23.0, 43.0, 0.0, 0.0);
      image.HorizontalAlignment = HorizontalAlignment.Left;
      image.VerticalAlignment = VerticalAlignment.Top;
      this._image = image;
      this.Children.Add((UIElement) this._image);
      TextBlock textBlock1 = new TextBlock();
      textBlock1.IsHitTestVisible = false;
      textBlock1.Width = 24.0;
      textBlock1.TextAlignment = TextAlignment.Center;
      textBlock1.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock1.VerticalAlignment = VerticalAlignment.Top;
      this._dateText = textBlock1;
      this._dateText.SetResourceReference(TextBlock.FontSizeProperty, (object) "MonthDayTextFontSize");
      this._dateText.SetResourceReference(FrameworkElement.MarginProperty, (object) "WeekDayTextMargin");
      this.Children.Add((UIElement) this._dateText);
      TextBlock textBlock2 = new TextBlock();
      textBlock2.FontSize = 12.0;
      textBlock2.IsHitTestVisible = false;
      textBlock2.Margin = new Thickness(0.0, 16.0, 0.0, 0.0);
      textBlock2.TextAlignment = TextAlignment.Center;
      textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock2.VerticalAlignment = VerticalAlignment.Top;
      this._weekDayText = textBlock2;
      this._weekDayText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
      this.Children.Add((UIElement) this._weekDayText);
      TextBlock textBlock3 = new TextBlock();
      textBlock3.FontSize = 12.0;
      textBlock3.IsHitTestVisible = false;
      textBlock3.Margin = new Thickness(30.0, 48.0, 12.0, 0.0);
      textBlock3.HorizontalAlignment = HorizontalAlignment.Right;
      textBlock3.VerticalAlignment = VerticalAlignment.Top;
      textBlock3.TextAlignment = TextAlignment.Left;
      this._lunarText = textBlock3;
      this.Children.Add((UIElement) this._lunarText);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataChanged);
    }

    public bool IsDateMouseOver => this._path != null && this._path.IsMouseOver;

    private void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
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
      string str1 = date.Day.ToString();
      dateText.Text = str1;
      TextBlock weekDayText = this._weekDayText;
      date = model.Date;
      string str2 = date.ToString("ddd", (IFormatProvider) App.Ci);
      weekDayText.Text = str2;
      this._lunarText.Text = DateUtils.GetLunarText(model.Date, false);
      if (HolidayManager.IsHolidayText(this._lunarText.Text))
        this._lunarText.Foreground = Utils.IsJp() ? (Brush) AllDayView._jpHolidayColor : (Brush) AllDayView._holidayColor;
      else
        this._lunarText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
      if (model.IsRestDay)
        this._image.SetResourceReference(Image.SourceProperty, (object) "RestDrawingImage");
      else if (model.IsWorkDay)
        this._image.SetResourceReference(Image.SourceProperty, (object) "WorkDrawingImage");
      else
        this._image.Source = (ImageSource) null;
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
          if (!(this.DataContext is MonthDayViewModel dataContext) || dataContext.IsToday)
            break;
          this._dateText.SetResourceReference(TextBlock.ForegroundProperty, dataContext.IsCurrentMonth ? (object) "BaseColorOpacity100_80" : (object) "BaseColorOpacity40");
          break;
      }
    }

    private void SetBackGround(MonthDayViewModel model)
    {
      if (model == null || model.State == MonthCellState.Normal)
        this._backBorder.Background = (Brush) Brushes.Transparent;
      else
        this._backBorder.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity5");
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

    public DateTime GetDate()
    {
      return this.DataContext is MonthDayViewModel dataContext ? dataContext.Date : DateTime.Today;
    }

    private void OnDayTextClick(object sender, MouseButtonEventArgs e)
    {
      CalendarControl parent = Utils.FindParent<CalendarControl>((DependencyObject) this);
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0)
        return;
      e.Handled = true;
      if (this.DataContext == null || !(this.DataContext is MonthDayViewModel dataContext))
        return;
      Utils.FindParent<CalendarControl>((DependencyObject) this)?.NavigateDate(dataContext.Date, CalendarDisplayMode.Day);
    }

    public void SetMonth(DateTime month)
    {
      if (!(this.DataContext is MonthDayViewModel dataContext))
        return;
      dataContext.SetCurrentMonth(month);
    }
  }
}
