// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarSideBar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Time;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarSideBar : UserControl, IComponentConnector
  {
    private ItemSelection _filter;
    private bool _isAll;
    internal Grid DayPickerGrid;
    internal TickDatePicker DayPicker;
    internal Grid FilterGrid;
    private bool _contentLoaded;

    public SolidColorBrush DayColor { get; set; }

    public bool IsAll => this._isAll;

    private void ResetFilterStatus(ProjectExtra data) => this._isAll = data.IsAll;

    public event EventHandler<DateTime> DateSelected;

    public CalendarSideBar()
    {
      this.InitializeComponent();
      this.InitFilter();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      this.DayPicker.SetInCalMode();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.ResetFilterStatus(ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData));
      this.BindEvents();
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.ProjectChanged -= new EventHandler(this.OnProjectChanged);
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
    }

    private void BindEvents()
    {
      DataChangedNotifier.ProjectChanged += new EventHandler(this.OnProjectChanged);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
    }

    private void OnDayChanged(object sender, EventArgs e) => this.DayPicker.SetDayCells();

    private void OnFilterChanged(object sender, FilterChangeArgs filterChangeArgs)
    {
      this.ReloadFilter();
    }

    private void OnProjectChanged(object sender, EventArgs e) => this.ReloadFilter();

    private void OnDaySelected(object sender, DateTime date)
    {
      EventHandler<DateTime> dateSelected = this.DateSelected;
      if (dateSelected == null)
        return;
      dateSelected((object) this, date);
    }

    public void SetSelectedRange(DateTime? start, DateTime? end)
    {
      this.DayPicker.SelectRange(start, end);
    }

    private void InitFilter()
    {
      if (this._filter == null)
        this._filter = new ItemSelection((Popup) null)
        {
          OriginalData = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData),
          ShowAll = true,
          ShowFilters = true,
          ShowTags = true,
          ShowSmartProjects = false,
          ShowCalendars = true,
          BatchMode = true,
          CanSelectGroup = true,
          OnlyShowPermission = false,
          ShowSmartAll = true,
          ShowFilterGroup = true,
          IsCalFilter = true,
          ShowListGroup = true
        };
      this._filter.ItemSelect -= new EventHandler<SelectableItemViewModel>(this.OnFilterItemSelect);
      this._filter.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnFilterItemSelect);
      this.FilterGrid.Children.Add((UIElement) this._filter);
    }

    private void OnFilterItemSelect(object sender, SelectableItemViewModel e)
    {
      ProjectExtra selectedData = this._filter.GetSelectedData();
      LocalSettings.Settings.CalendarFilterData = ProjectExtra.Serialize(selectedData);
      DataChangedNotifier.NotifyCalendarProjectFilterChanged((object) this);
      this.ResetFilterStatus(selectedData);
      UtilLog.Info(LocalSettings.Settings.CalendarFilterData);
    }

    public void ReloadFilter()
    {
      this._filter.ItemSelect -= new EventHandler<SelectableItemViewModel>(this.OnFilterItemSelect);
      this._filter.OriginalData = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      this._filter.LoadData();
      this._filter.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnFilterItemSelect);
    }

    public bool CheckHoverDayCell(double vOffset)
    {
      if (this.Opacity == 0.0)
        return false;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) this.DayPicker.DayCells);
      double x = position.X;
      position = Mouse.GetPosition((IInputElement) this.DayPicker.DayCells);
      double num = position.Y - vOffset;
      if (x > 0.0 && x < this.DayPicker.ActualWidth && num > 0.0 && num < this.DayPicker.ActualHeight)
      {
        int column = (int) (x / 32.0);
        this.DayPicker.SetHoverCell((int) (num / 32.0), column);
        return true;
      }
      this.DayPicker.ClearHoverCell();
      return false;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      double num = this.ActualHeight - this.DayPickerGrid.ActualHeight;
      if (this._filter == null)
        return;
      if (num != double.NaN && num > 250.0)
        this._filter.SelectableItems.MaxHeight = num;
      else
        this._filter.SelectableItems.MaxHeight = 250.0;
    }

    public void SetMonth(DateTime monthDate) => this.DayPicker.SetPivotDate(monthDate);

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calendarsidebar.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
          break;
        case 2:
          this.DayPickerGrid = (Grid) target;
          break;
        case 3:
          this.DayPicker = (TickDatePicker) target;
          break;
        case 4:
          this.FilterGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
