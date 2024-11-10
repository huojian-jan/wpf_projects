// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.TickDatePicker
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class TickDatePicker : UserControl, IComponentConnector
  {
    private const int DayColumns = 7;
    private const int DayRows = 6;
    private const int MonthColumns = 4;
    private const int MonthRows = 3;
    public static readonly DependencyProperty StartDateProperty = DependencyProperty.Register(nameof (StartDate), typeof (DateTime), typeof (TickDatePicker), new PropertyMetadata((object) DateTime.Today, new PropertyChangedCallback(TickDatePicker.OnStartDateChangeCallback)));
    public static readonly DependencyProperty ShowIndicatorProperty = DependencyProperty.Register(nameof (ShowIndicator), typeof (bool), typeof (TickDatePicker), new PropertyMetadata((object) false, new PropertyChangedCallback(TickDatePicker.OnStartDateChangeCallback)));
    public static readonly DependencyProperty CanMultiSelectProperty = DependencyProperty.Register(nameof (CanMultiSelect), typeof (bool), typeof (TickDatePicker), new PropertyMetadata((object) false, new PropertyChangedCallback(TickDatePicker.OnStartDateChangeCallback)));
    public static readonly DependencyProperty MiniModeProperty = DependencyProperty.Register(nameof (MiniMode), typeof (bool), typeof (TickDatePicker), new PropertyMetadata((object) false, new PropertyChangedCallback(TickDatePicker.OnMiniModeChangeCallback)));
    public static readonly DependencyProperty UseInSlideMenuProperty = DependencyProperty.Register(nameof (UseInSlideMenu), typeof (bool), typeof (TickDatePicker), new PropertyMetadata((object) false, new PropertyChangedCallback(TickDatePicker.OnUseInSlideMenuChangeCallback)));
    private bool _isStartDate;
    private bool _monthMode;
    private double _deltaInDays;
    private DatePickerViewModel _model;
    private DateTime _minDate;
    private DateTime? _maxDate;
    private List<DateTime> _selectedDays = new List<DateTime>();
    private Dictionary<DateTime, bool?> _fixedSelectedDays = new Dictionary<DateTime, bool?>();
    private bool _canClear;
    private DateTime _tabSelectedDate;
    private List<CalendarDisplayModel> _models = new List<CalendarDisplayModel>();
    internal TickDatePicker RootView;
    internal Grid TopGrid;
    internal TextBlock YearMonthButton;
    internal Run MonthRun;
    internal TextBlock YearButton;
    internal Grid NextOrLastGrid;
    internal Ellipse GotoCurrentBt;
    internal Border NextGrid;
    internal Grid MonthCellsGrid;
    internal StackPanel DayGrid;
    internal Grid WeekDaysGrid;
    internal ItemsControl DayCells;
    private bool _contentLoaded;

    public bool IsChooseDate { get; set; }

    private bool CanSwitchNext
    {
      get
      {
        int? nullable1 = this._model?.PivotDate.Year;
        ref DateTime? local1 = ref this._maxDate;
        int? nullable2 = local1.HasValue ? new int?(local1.GetValueOrDefault().Year) : new int?();
        if (!(nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() & nullable1.HasValue == nullable2.HasValue))
          return true;
        int? month = this._model?.PivotDate.Month;
        ref DateTime? local2 = ref this._maxDate;
        nullable1 = local2.HasValue ? new int?(local2.GetValueOrDefault().Month) : new int?();
        return !(month.GetValueOrDefault() == nullable1.GetValueOrDefault() & month.HasValue == nullable1.HasValue);
      }
    }

    private bool CanSwitchBefore
    {
      get
      {
        int? year1 = this._model?.PivotDate.Year;
        int year2 = this._minDate.Year;
        if (!(year1.GetValueOrDefault() == year2 & year1.HasValue))
          return true;
        int? month1 = this._model?.PivotDate.Month;
        int month2 = this._minDate.Month;
        return !(month1.GetValueOrDefault() == month2 & month1.HasValue);
      }
    }

    public TickDatePicker()
      : this(new DateTime?())
    {
    }

    public TickDatePicker(
      DateTime? selectedDate,
      DateTime? selectStart = null,
      DateTime? selectEnd = null,
      bool isStart = true,
      bool isMonthMode = false,
      DateTime? maxDate = null,
      DateTime? minDate = null)
    {
      this.InitializeComponent();
      this.SetData(selectedDate, selectStart, selectEnd, isStart, isMonthMode, maxDate, minDate);
    }

    public void SetData(
      DateTime? selectedDate,
      DateTime? selectStart = null,
      DateTime? selectEnd = null,
      bool isStart = true,
      bool isMonthMode = false,
      DateTime? maxDate = null,
      DateTime? minDate = null)
    {
      this._isStartDate = isStart;
      this._monthMode = isMonthMode;
      this._maxDate = maxDate;
      this._minDate = minDate.GetValueOrDefault();
      this.InitData(selectedDate, selectStart, selectEnd);
      if (!this.CanSwitchNext)
      {
        this.NextGrid.Cursor = Cursors.Arrow;
        this.NextGrid.MouseLeftButtonUp -= new MouseButtonEventHandler(this.NextClick);
      }
      this.InitCells();
      this.SetGotoCurrentEnable();
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnShowHolidayChanged), "EnableHoliday");
    }

    private void OnShowHolidayChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      foreach (DayCellViewModel dayCellViewModel in (Collection<DayCellViewModel>) itemsSource)
        dayCellViewModel.Data?.SetHoliday();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      if (!this.ShowIndicator)
        return;
      this.LoadTaskIndicator();
    }

    public DateTime? SelectStart => this._model.SelectStartDate;

    public DateTime? SelectEnd => this._model.SelectEndDate;

    private DateTime PivotDate => this._model.PivotDate;

    public bool AllowSelectMonth { get; set; } = true;

    public bool ShowInCal { get; set; }

    public bool ShowSelectedInCal { get; set; }

    public bool ShowIndicator
    {
      get => (bool) this.GetValue(TickDatePicker.ShowIndicatorProperty);
      set => this.SetValue(TickDatePicker.ShowIndicatorProperty, (object) value);
    }

    public bool CanMultiSelect
    {
      get => (bool) this.GetValue(TickDatePicker.CanMultiSelectProperty);
      set => this.SetValue(TickDatePicker.CanMultiSelectProperty, (object) value);
    }

    public DateTime? SelectedDate
    {
      get => this._model.SelectedDate;
      set
      {
        this._model.SelectedDate = value;
        this.ResetOnSelectedChanged();
      }
    }

    public string RepeatFlag { get; set; }

    public string RepeatFrom { get; set; }

    public List<string> ExDates { get; set; }

    public DateTime StartDate
    {
      get => (DateTime) this.GetValue(TickDatePicker.StartDateProperty);
      set => this.SetValue(TickDatePicker.StartDateProperty, (object) value);
    }

    public bool MiniMode
    {
      get => (bool) this.GetValue(TickDatePicker.MiniModeProperty);
      set => this.SetValue(TickDatePicker.MiniModeProperty, (object) value);
    }

    public bool UseInSlideMenu
    {
      get => (bool) this.GetValue(TickDatePicker.UseInSlideMenuProperty);
      set => this.SetValue(TickDatePicker.UseInSlideMenuProperty, (object) value);
    }

    public string TimeZone { get; set; }

    public event EventHandler<DateTime> SelectedDateChanged;

    public event EventHandler<DateTime> MonthSelected;

    public event EventHandler<DateTime> DateSelected;

    public event EventHandler<DateTime> StartDateChanged;

    private static void OnMiniModeChangeCallback(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is TickDatePicker tickDatePicker) || !(e.NewValue is bool newValue))
        return;
      tickDatePicker._model.MiniMode = newValue;
      tickDatePicker._model.RowCount = newValue ? 1 : 6;
      tickDatePicker.DayCells.Height = (double) (32 * tickDatePicker._model.RowCount);
      tickDatePicker.SetDayCells();
    }

    private static void OnUseInSlideMenuChangeCallback(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is TickDatePicker tickDatePicker) || !(e.NewValue is bool newValue))
        return;
      tickDatePicker._model.UseInSlideMenu = newValue;
      tickDatePicker._model.PrimaryTextColor = TickDatePicker.GetPrimaryTextColor(newValue);
      tickDatePicker._model.SecondaryTextColor = TickDatePicker.GetSecondaryTextColor(newValue);
      tickDatePicker._model.TertiaryTextColor = TickDatePicker.GetTertiaryTextColor(newValue);
      tickDatePicker.SetDayCells();
    }

    private static SolidColorBrush GetTertiaryTextColor(bool useInSlideMenu)
    {
      return ThemeUtil.GetColor(useInSlideMenu ? "ProjectMenuColorOpacity40" : "BaseColorOpacity40");
    }

    private static SolidColorBrush GetSecondaryTextColor(bool useInSlideMenu)
    {
      return ThemeUtil.GetColor(useInSlideMenu ? "ProjectMenuColorOpacity60" : "BaseColorOpacity60");
    }

    private static SolidColorBrush GetPrimaryTextColor(bool useInSlideMenu)
    {
      return ThemeUtil.GetColor(useInSlideMenu ? "ProjectMenuColorOpacity80" : "BaseColorOpacity80");
    }

    private static void OnStartDateChangeCallback(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is TickDatePicker sender) || !(e.NewValue is DateTime newValue))
        return;
      EventHandler<DateTime> startDateChanged = sender.StartDateChanged;
      if (startDateChanged == null)
        return;
      startDateChanged((object) sender, newValue);
    }

    public void InitEvents()
    {
      DataChangedNotifier.WeekStartFromChanged -= new EventHandler(this.ReSetDayCells);
      DataChangedNotifier.WeekStartFromChanged += new EventHandler(this.ReSetDayCells);
    }

    private async void ReSetDayCells(object sender, EventArgs e) => this.SetDayCells();

    public void SelectRange(DateTime? start, DateTime? end)
    {
      if (!start.HasValue || !end.HasValue)
      {
        this._model.SelectStartDate = new DateTime?();
        this._model.SelectEndDate = new DateTime?();
      }
      else
      {
        DateTime? nullable1 = start;
        DateTime? nullable2 = end;
        if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
        {
          this._model.SelectedDate = start;
          DatePickerViewModel model1 = this._model;
          nullable2 = new DateTime?();
          DateTime? nullable3 = nullable2;
          model1.SelectStartDate = nullable3;
          DatePickerViewModel model2 = this._model;
          nullable2 = new DateTime?();
          DateTime? nullable4 = nullable2;
          model2.SelectEndDate = nullable4;
        }
        else
        {
          DatePickerViewModel model = this._model;
          nullable2 = new DateTime?();
          DateTime? nullable5 = nullable2;
          model.SelectedDate = nullable5;
          this._model.SelectStartDate = start;
          this._model.SelectEndDate = end;
        }
      }
      this.SetDayCells(false);
    }

    public void SetInCalMode()
    {
      this.ShowInCal = true;
      this.TopGrid.Height = 44.0;
      this.YearMonthButton.FontSize = 14.0;
      this.MonthRun.FontWeight = FontWeights.Bold;
      this.WeekDaysGrid.Height = 28.0;
      foreach (UIElement child in this.WeekDaysGrid.Children)
      {
        if (child is TextBlock textBlock)
          textBlock.FontSize = 12.0;
      }
      Line line = new Line();
      line.X1 = 0.0;
      line.X2 = 1.0;
      line.Stretch = Stretch.Fill;
      line.StrokeThickness = 1.0;
      line.VerticalAlignment = VerticalAlignment.Bottom;
      line.Margin = new Thickness(-10.0, 4.0, -10.0, 12.0);
      Line element = line;
      element.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
      element.SetValue(Grid.ColumnSpanProperty, (object) 7);
      this.DayGrid.Children.Insert(1, (UIElement) element);
    }

    public async Task ReloadColor()
    {
      if (!(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      foreach (DayCellViewModel dayCellViewModel in (Collection<DayCellViewModel>) itemsSource)
        dayCellViewModel.Data?.NotifyColorChanged();
    }

    public async Task LoadTaskIndicator(bool reloadModels = true)
    {
      if (!(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> items))
      {
        items = (ObservableCollection<DayCellViewModel>) null;
      }
      else
      {
        if (reloadModels)
        {
          DateTime start = this.StartDate;
          DateTime end = start.AddDays(42.0);
          List<CalendarDisplayModel> models = await CalendarDisplayService.GetCalendarDisplayModels(start, end);
          if (LocalSettings.Settings.ShowRepeatCircles)
          {
            List<CalendarDisplayModel> repeatDisplayModel = await CalendarDisplayService.GetRepeatDisplayModel(start, end);
            if (repeatDisplayModel.Count > 0)
              models.AddRange((IEnumerable<CalendarDisplayModel>) repeatDisplayModel);
          }
          List<CalendarDisplayModel> repeatDisplayModels = await CalendarDisplayService.GetRepeatDisplayModels(start, end);
          if (repeatDisplayModels.Count > 0)
            models.AddRange((IEnumerable<CalendarDisplayModel>) repeatDisplayModels);
          this._models = models;
          models = (List<CalendarDisplayModel>) null;
        }
        if (this._models == null)
          items = (ObservableCollection<DayCellViewModel>) null;
        else if (!this._models.Any<CalendarDisplayModel>())
        {
          items = (ObservableCollection<DayCellViewModel>) null;
        }
        else
        {
          items.ToList<DayCellViewModel>().ForEach((Action<DayCellViewModel>) (model => model.Data.HasTasks = false));
          using (List<CalendarDisplayModel>.Enumerator enumerator = this._models.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              CalendarDisplayModel model = enumerator.Current;
              items.Where<DayCellViewModel>((Func<DayCellViewModel, bool>) (i =>
              {
                if (i.Data.HasTasks || model.Status != 0)
                  return false;
                DateTime? nullable = model.DueDate;
                DateTime dateTime;
                if (!nullable.HasValue)
                {
                  nullable = model.DisplayStartDate;
                  if (nullable.HasValue)
                  {
                    nullable = model.DisplayStartDate;
                    dateTime = nullable.Value;
                    if (dateTime.Date == i.Data.Date)
                      return true;
                  }
                }
                nullable = model.DisplayStartDate;
                if (nullable.HasValue)
                {
                  nullable = model.DisplayStartDate;
                  dateTime = nullable.Value;
                  if (dateTime.Date <= i.Data.Date)
                  {
                    nullable = model.DisplayDueDate;
                    if (nullable.HasValue)
                    {
                      nullable = model.DisplayDueDate;
                      dateTime = nullable.Value;
                      return dateTime.Date >= i.Data.Date;
                    }
                  }
                }
                return false;
              })).ToList<DayCellViewModel>().ForEach((Action<DayCellViewModel>) (item => item.Data.HasTasks = true));
            }
            items = (ObservableCollection<DayCellViewModel>) null;
          }
        }
      }
    }

    public List<DateTime> GetSelectedDays() => this._selectedDays.ToList<DateTime>();

    public void SetSelectedDays(List<DateTime> dayList)
    {
      this._selectedDays.Clear();
      dayList.ForEach((Action<DateTime>) (item => this._selectedDays.Add(item.Date)));
      this.TryUpdateViewDays();
    }

    public void SetFixedSelectedDay(DateTime time, bool? isSelected)
    {
      if (isSelected.HasValue)
        this._fixedSelectedDays[time.Date] = new bool?(isSelected.Value);
      else
        this._fixedSelectedDays.Remove(time.Date);
      this.TryUpdateViewDays();
    }

    public DateTime? GetHoverDate()
    {
      if (!(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return new DateTime?();
      return itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (item => item.Data.Hover))?.Data.Date;
    }

    public void SetHoverCell(int row, int column)
    {
      if (!(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      this._canClear = true;
      itemsSource.ToList<DayCellViewModel>().ForEach((Action<DayCellViewModel>) (model => model.Data.Hover = false));
      DayCellViewModel dayCellViewModel = itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (item => item.Row == row && item.Column == column));
      if (dayCellViewModel == null)
        return;
      dayCellViewModel.Data.Hover = true;
    }

    public void ClearHoverCell()
    {
      if (!this._canClear)
        return;
      if (this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource)
        itemsSource.ToList<DayCellViewModel>().ForEach((Action<DayCellViewModel>) (model => model.Data.Hover = false));
      this._canClear = false;
    }

    private void InitCells()
    {
      if (this._monthMode)
        this.SwitchMonthMode();
      else
        this.SetDayCells();
    }

    private void InitData(DateTime? selectedDate, DateTime? selectStart, DateTime? selectEnd)
    {
      this._model = new DatePickerViewModel()
      {
        PivotDate = selectedDate ?? DateTime.Now.Date,
        SelectedDate = selectedDate,
        SelectStartDate = selectStart,
        SelectEndDate = selectEnd
      };
      this.InitDelta();
      this.DataContext = (object) this._model;
    }

    private void ResetOnSelectedChanged()
    {
      if (!this._model.SelectedDate.HasValue)
        return;
      DateTime pivotDate = this._model.PivotDate;
      DateTime? selectedDate1 = this._model.SelectedDate;
      DateTime? selectedDate2;
      if ((selectedDate1.HasValue ? (pivotDate != selectedDate1.GetValueOrDefault() ? 1 : 0) : 1) != 0)
      {
        DatePickerViewModel model = this._model;
        selectedDate2 = this._model.SelectedDate;
        DateTime dateTime = selectedDate2.Value;
        model.PivotDate = dateTime;
      }
      DatePickerViewModel model1 = this._model;
      selectedDate2 = this._model.SelectedDate;
      DateTime date = selectedDate2.Value.Date;
      model1.PivotDate = date;
      if (!(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      this.ClearLastSelectedIfNeeds(this._model.PivotDate.Date);
      this.TryUpdateViewDays();
      DateTime monthStartDate = this.GetMonthStartDate(this._model.PivotDate);
      if (itemsSource[0].Data.Date != monthStartDate && !this.MiniMode)
        this.InitCells();
      DayCellViewModel dayCellViewModel = ((IEnumerable<DayCellViewModel>) this.DayCells.ItemsSource).FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (m => m.Data.Date == this._model.PivotDate.Date));
      if (dayCellViewModel == null)
        return;
      dayCellViewModel.Data.Selected = true;
    }

    private void TryUpdateViewDays()
    {
      if (!(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      foreach (DayCellViewModel dayCellViewModel in (Collection<DayCellViewModel>) itemsSource)
      {
        if (this.CanMultiSelect && this._selectedDays.Contains(dayCellViewModel.Data.Date))
          dayCellViewModel.Data.Selected = true;
        if (this._fixedSelectedDays.Any<KeyValuePair<DateTime, bool?>>() && this._fixedSelectedDays.ContainsKey(dayCellViewModel.Data.Date))
        {
          bool? fixedSelectedDay = this._fixedSelectedDays[dayCellViewModel.Data.Date];
          if (fixedSelectedDay.HasValue)
          {
            DayViewModel data = dayCellViewModel.Data;
            fixedSelectedDay = this._fixedSelectedDays[dayCellViewModel.Data.Date];
            int num = fixedSelectedDay.Value ? 1 : 0;
            data.Selected = num != 0;
            dayCellViewModel.Data.IsFixed = true;
          }
          else
          {
            dayCellViewModel.Data.IsFixed = false;
            this._fixedSelectedDays.Remove(dayCellViewModel.Data.Date);
          }
        }
      }
    }

    public void Reload()
    {
      this._model.UseInSlideMenu = this.UseInSlideMenu;
      this._model.PrimaryTextColor = TickDatePicker.GetPrimaryTextColor(this.UseInSlideMenu);
      this._model.SecondaryTextColor = TickDatePicker.GetSecondaryTextColor(this.UseInSlideMenu);
      this._model.TertiaryTextColor = TickDatePicker.GetTertiaryTextColor(this.UseInSlideMenu);
      this.SetDayCells();
    }

    public void SetDayCells(bool reloadIndicator = true, bool reset = false)
    {
      DateTime dateTime = this.GetMonthStartDate(this._model.PivotDate);
      if (this.MiniMode)
        dateTime = Utils.GetWeekStart(DateTime.Today);
      reloadIndicator = reloadIndicator || dateTime != this.StartDate;
      this.StartDate = dateTime;
      int num1 = 0;
      List<DayCellViewModel> dayCellViewModelList = new List<DayCellViewModel>();
      int weekStart = LocalSettings.Settings.WeekStartFrom == "Saturday" ? 6 : (LocalSettings.Settings.WeekStartFrom == "Monday" ? 1 : 0);
      for (int index1 = 0; index1 < this._model.RowCount; ++index1)
      {
        for (int index2 = 0; index2 < 7; ++index2)
        {
          DayViewModel dayViewModel1 = new DayViewModel(this.StartDate.AddDays((double) num1++), (FrameworkElement) this);
          this.InitShowMode(dayViewModel1);
          this.InitSelection(dayViewModel1, weekStart);
          this.InitSelected(dayViewModel1);
          this.InitLunarText(dayViewModel1);
          DayViewModel dayViewModel2 = dayViewModel1;
          int num2;
          if (this._maxDate.HasValue)
          {
            DateTime date = dayViewModel1.Date.Date;
            DateTime? maxDate = this._maxDate;
            if ((maxDate.HasValue ? (date <= maxDate.GetValueOrDefault() ? 1 : 0) : 0) == 0)
            {
              num2 = 0;
              goto label_8;
            }
          }
          num2 = dayViewModel1.Date.Date >= this._minDate ? 1 : 0;
label_8:
          dayViewModel2.CanSelect = num2 != 0;
          dayViewModel1.CanDoubleSelect = !this.ShowInCal;
          dayViewModel1.UseInSlideMenu = this.UseInSlideMenu;
          dayCellViewModelList.Add(new DayCellViewModel()
          {
            Row = index1,
            Column = index2,
            Data = dayViewModel1
          });
        }
      }
      if (reset || this.MiniMode)
        this.DayCells.ItemsSource = (IEnumerable) new ObservableCollection<DayCellViewModel>(dayCellViewModelList);
      else
        ItemsSourceHelper.SetHidableItemsSource<DayCellViewModel>(this.DayCells, dayCellViewModelList);
      if (this.ShowIndicator)
        this.LoadTaskIndicator(reloadIndicator);
      this.TryUpdateViewDays();
      this.SetGotoCurrentEnable();
    }

    private bool IsInSameWeek(DateTime dtmS, DateTime dtmE)
    {
      double totalDays = (dtmE - dtmS).TotalDays;
      int num = Convert.ToInt32((object) dtmE.DayOfWeek);
      if (num == 0)
        num = 7;
      return totalDays < 7.0 && totalDays < (double) num;
    }

    private void SetGotoCurrentEnable()
    {
    }

    private void InitLunarText(DayViewModel day)
    {
      AlternativeCalendar alternativeCalendar = LocalSettings.Settings.UserPreference.alternativeCalendar;
      if ((LocalSettings.Settings.EnableLunar ? 1 : (alternativeCalendar == null ? 0 : (alternativeCalendar.calendar == "lunar" ? 1 : 0))) != 0)
      {
        TickDatePicker.InitChineseLunar(day);
      }
      else
      {
        if (alternativeCalendar == null || !CalendarConverter.IsValidCalendarType(alternativeCalendar.calendar))
          return;
        CalendarDisplay calendarDisplay = new CalendarDisplay(day.Date, alternativeCalendar.calendar);
        day.LunarMonthFirstDay = calendarDisplay.Day == 1;
        day.LunarText = calendarDisplay.DisplayText();
      }
    }

    private static void InitChineseLunar(DayViewModel day)
    {
      LunarUtils.ChineseCalendar chineseCalendar = new LunarUtils.ChineseCalendar(day.Date);
      if (Utils.IsEmptyDate(chineseCalendar.Date))
        return;
      try
      {
        if (LocalSettings.Settings.EnableHoliday && !string.IsNullOrEmpty(chineseCalendar.ChineseCalendarHoliday))
        {
          DateTime date = day.Date;
          if (date.Month == 10)
          {
            date = day.Date;
            if (date.Day == 1)
              goto label_6;
          }
          string chineseCalendarHoliday = chineseCalendar.ChineseCalendarHoliday;
          if (chineseCalendarHoliday.Contains("除夕") || chineseCalendarHoliday.Contains("春节") || chineseCalendarHoliday.Contains("中秋") || chineseCalendarHoliday.Contains("端午"))
          {
            day.LunarText = chineseCalendarHoliday;
            return;
          }
        }
label_6:
        if (!string.IsNullOrEmpty(chineseCalendar.DateHoliday))
        {
          string dateHoliday = chineseCalendar.DateHoliday;
          day.LunarText = chineseCalendar.DateHoliday;
        }
        else if (!string.IsNullOrEmpty(chineseCalendar.ChineseCalendarHoliday))
        {
          string chineseCalendarHoliday = chineseCalendar.ChineseCalendarHoliday;
          day.LunarText = chineseCalendarHoliday;
        }
        else if (!string.IsNullOrEmpty(chineseCalendar.WeekDayHoliday))
        {
          day.LunarText = chineseCalendar.WeekDayHoliday;
        }
        else
        {
          if (!string.IsNullOrEmpty(chineseCalendar.ChineseTwentyFourDay))
          {
            string chineseTwentyFourDay = chineseCalendar.ChineseTwentyFourDay;
            if (LocalSettings.Settings.EnableLunar || LocalSettings.Settings.EnableHoliday && chineseTwentyFourDay.Contains("清明"))
            {
              day.LunarText = chineseTwentyFourDay;
              return;
            }
          }
          day.LunarText = chineseCalendar.ChineseDayString;
          if (chineseCalendar.ChineseDay != 1)
            return;
          day.LunarText = chineseCalendar.ChineseMonthString;
          day.LunarMonthFirstDay = true;
        }
      }
      catch (Exception ex)
      {
      }
    }

    public async void RenderRepeat()
    {
      DateTime startDate = !this.SelectedDate.HasValue || Utils.IsEmptyDate(this.SelectedDate) ? DateTime.Today : this.SelectedDate.Value;
      if (this.DayCells.ItemsSource == null || !(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      DateTime date = itemsSource[0].Data.Date;
      List<DateTime> validRepeatDates = RepeatUtils.GetValidRepeatDates(this.RepeatFlag, this.RepeatFrom, startDate, date.AddDays(-1.0), date.AddDays(43.0), this.ExDates, this.TimeZone);
      for (int index = 0; index < validRepeatDates.Count; ++index)
        validRepeatDates[index] = TimeZoneUtils.LocalToTargetTzTime(validRepeatDates[index], this.TimeZone);
      foreach (DayCellViewModel dayCellViewModel in (Collection<DayCellViewModel>) itemsSource)
      {
        DayCellViewModel item = dayCellViewModel;
        item.Data.IsRepeat = validRepeatDates.Exists((Predicate<DateTime>) (model => model.Date.Date == item.Data.Date));
      }
    }

    private void InitSelected(DayViewModel day)
    {
      day.TabBorderThickness = (double) (day.Date.Date == this._tabSelectedDate.Date ? 1 : 0);
      if (this.CanMultiSelect)
        return;
      DateTime? selectedDate = this._model.SelectedDate;
      if (!selectedDate.HasValue)
        return;
      DateTime date1 = day.Date;
      DateTime date2 = date1.Date;
      selectedDate = this._model.SelectedDate;
      date1 = selectedDate.Value;
      DateTime date3 = date1.Date;
      if (!(date2 == date3))
        return;
      day.Selected = true;
    }

    private void InitDelta()
    {
      if (!this._model.SelectStartDate.HasValue || !this._model.SelectEndDate.HasValue)
        return;
      this._deltaInDays = (this._model.SelectEndDate.Value.Date - this._model.SelectStartDate.Value.Date).TotalDays;
    }

    private void InitMonthRowsAndColumns()
    {
      for (int index = 0; index < 3; ++index)
        this.MonthCellsGrid.RowDefinitions.Add(new RowDefinition());
      for (int index = 0; index < 4; ++index)
        this.MonthCellsGrid.ColumnDefinitions.Add(new ColumnDefinition());
    }

    private void InitSelection(DayViewModel day, int weekStart)
    {
      if (!this._model.SelectStartDate.HasValue || !this._model.SelectEndDate.HasValue)
        return;
      DateTime date1 = this._model.SelectStartDate.Value.Date;
      DateTime date2 = this._model.SelectEndDate.Value;
      DateTime date3 = date2.Date;
      date2 = day.Date;
      DateTime date4 = date2.Date;
      if (date4 >= date1.Date && date4 <= date3.Date)
      {
        date2 = day.Date;
        bool flag1 = date2.DayOfWeek == (DayOfWeek) weekStart;
        date2 = day.Date;
        bool flag2 = date2.DayOfWeek == (DayOfWeek) ((weekStart + 6) % 7);
        if (date3 == date1)
          day.Selection = SelectionMode.Full;
        else if (date4 == date1)
          day.Selection = flag2 ? SelectionMode.Full : SelectionMode.Start;
        else if (date4 == date3)
          day.Selection = flag1 ? SelectionMode.Full : SelectionMode.End;
        else if (flag2)
          day.Selection = SelectionMode.End;
        else
          day.Selection = flag1 ? SelectionMode.Start : SelectionMode.Middle;
      }
      else
        day.Selection = SelectionMode.None;
    }

    private void InitShowMode(DayViewModel model)
    {
      DateTime dateTime1 = model.Date;
      int month1 = dateTime1.Month;
      dateTime1 = this._model.PivotDate;
      dateTime1 = dateTime1.Date;
      int month2 = dateTime1.Month;
      if (month1 == month2)
      {
        model.ShowMode = ShowMode.CurrentMonth;
      }
      else
      {
        DateTime dateTime2 = model.Date;
        int month3 = dateTime2.Month;
        dateTime2 = this._model.PivotDate;
        dateTime2 = dateTime2.Date;
        int month4 = dateTime2.Month;
        model.ShowMode = month3 >= month4 ? ShowMode.NextMonth : ShowMode.LastMonth;
      }
      DateTime dateTime3 = model.Date;
      DateTime date1 = dateTime3.Date;
      dateTime3 = DateTime.Now;
      DateTime date2 = dateTime3.Date;
      if (!(date1 == date2))
        return;
      model.ShowMode = ShowMode.Today;
    }

    private void OnDateChanged(DateTime date)
    {
      this._model.SelectedDate = new DateTime?(date);
      this.ClearLastSelectedIfNeeds(date);
      this.ResetOnSelectedChanged();
      this.TryAdjustSelection(date);
      this.RenderRepeat();
    }

    private void TryAdjustSelection(DateTime date)
    {
      if (this._model.SelectStartDate.HasValue && this._model.SelectEndDate.HasValue)
      {
        if (this._isStartDate)
        {
          DatePickerViewModel model1 = this._model;
          DateTime? nullable1 = this._model.SelectStartDate;
          DateTime? nullable2 = new DateTime?(DateUtils.SetDateOnly(nullable1.Value, date.Date));
          model1.SelectStartDate = nullable2;
          DatePickerViewModel model2 = this._model;
          nullable1 = this._model.SelectEndDate;
          DateTime original = nullable1.Value;
          nullable1 = this._model.SelectStartDate;
          DateTime modify = nullable1.Value;
          DateTime? nullable3 = new DateTime?(DateUtils.SetDateOnly(original, modify));
          model2.SelectEndDate = nullable3;
          DatePickerViewModel model3 = this._model;
          nullable1 = this._model.SelectEndDate;
          DateTime? nullable4 = new DateTime?(nullable1.Value.AddDays(this._deltaInDays));
          model3.SelectEndDate = nullable4;
        }
        else
        {
          DatePickerViewModel model4 = this._model;
          DateTime? nullable5 = this._model.SelectEndDate;
          DateTime? nullable6 = new DateTime?(DateUtils.SetDateOnly(nullable5.Value, date.Date));
          model4.SelectEndDate = nullable6;
          nullable5 = this._model.SelectStartDate;
          DateTime date1 = date.Date;
          if ((nullable5.HasValue ? (nullable5.GetValueOrDefault() > date1 ? 1 : 0) : 0) != 0)
          {
            DatePickerViewModel model5 = this._model;
            nullable5 = this._model.SelectStartDate;
            DateTime? nullable7 = new DateTime?(DateUtils.SetDateOnly(nullable5.Value, date.Date));
            model5.SelectStartDate = nullable7;
          }
        }
      }
      this.RenderSelection(date);
    }

    private void RenderSelection(DateTime date)
    {
      if (this.DayCells.ItemsSource == null || !(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      int weekStart = LocalSettings.Settings.WeekStartFrom == "Saturday" ? 6 : (LocalSettings.Settings.WeekStartFrom == "Monday" ? 1 : 0);
      foreach (DayCellViewModel dayCellViewModel in (Collection<DayCellViewModel>) itemsSource)
        this.InitSelection(dayCellViewModel.Data, weekStart);
    }

    private void ClearLastSelectedIfNeeds(DateTime selectedDate)
    {
      if (this.CanMultiSelect || this.DayCells.ItemsSource == null || !(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      foreach (DayCellViewModel dayCellViewModel in (Collection<DayCellViewModel>) itemsSource)
      {
        if (dayCellViewModel.Data.Selected && dayCellViewModel.Data.Date != selectedDate.Date)
        {
          dayCellViewModel.Data.Selected = false;
          break;
        }
      }
    }

    private DateTime GetMonthStartDate(DateTime date)
    {
      if (Utils.IsEmptyDate(date))
        return DateTime.Today;
      DateTime dateTime1 = date.AddDays((double) (1 - date.Day));
      DateTime dateTime2 = dateTime1.AddDays((double) ((int) dateTime1.DayOfWeek * -1)).AddDays((double) Utils.GetWeekFromDiff(new DateTime?(dateTime1)));
      return dateTime2.Month == this.PivotDate.Month && dateTime2.Day != 1 ? dateTime2.AddDays(-7.0) : dateTime2;
    }

    private DateTime InitStartMonth()
    {
      return this._model.PivotDate.AddDays((double) (this._model.PivotDate.DayOfYear * -1 + 1));
    }

    private void OnClick(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void LastClick(object sender, MouseButtonEventArgs e) => this.NextMonthOrYear(-1);

    private void NextClick(object sender, MouseButtonEventArgs e) => this.NextMonthOrYear(1);

    private void NextMonthOrYear(int step)
    {
      if (this.YearMonthButton.Visibility == Visibility.Visible)
      {
        if (this._model.PivotDate.Year < 1970)
          this._model.PivotDate = DateTime.Today;
        this._model.PivotDate = this._model.PivotDate.AddMonths(step);
        if (!this.CanSwitchNext)
        {
          this.NextGrid.Cursor = Cursors.Arrow;
          this.NextGrid.MouseLeftButtonUp -= new MouseButtonEventHandler(this.NextClick);
        }
        else
        {
          this.NextGrid.Cursor = Cursors.Hand;
          this.NextGrid.MouseLeftButtonUp -= new MouseButtonEventHandler(this.NextClick);
          this.NextGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.NextClick);
        }
        this.SetDayCells();
      }
      else
      {
        this._model.PivotDate = this._model.PivotDate.AddYears(step);
        this.ClearMonthCells();
        this.SetMonthCells();
      }
      this.RenderRepeat();
      this.TryUpdateViewDays();
    }

    private void MonthClick(object sender, RoutedEventArgs e)
    {
      if (!this.AllowSelectMonth || this.MonthCellsGrid.Visibility == Visibility.Visible)
        return;
      this.SwitchMonthMode();
    }

    private void SwitchMonthMode()
    {
      this.DayGrid.Visibility = Visibility.Collapsed;
      this.MonthCellsGrid.Visibility = Visibility.Visible;
      this.YearButton.Visibility = Visibility.Visible;
      this.YearMonthButton.Visibility = Visibility.Collapsed;
      this.ClearMonthCells();
      this.SetMonthCells();
    }

    private void ClearMonthCells()
    {
      this.MonthCellsGrid.Children.Clear();
      this.MonthCellsGrid.RowDefinitions.Clear();
      this.MonthCellsGrid.ColumnDefinitions.Clear();
    }

    private void SetMonthCells()
    {
      this.InitMonthRowsAndColumns();
      DateTime dateTime1 = this.InitStartMonth();
      int num1 = 0;
      for (int index1 = 0; index1 < 3; ++index1)
      {
        for (int index2 = 0; index2 < 4; ++index2)
        {
          DateTime dateTime2 = dateTime1.AddMonths(num1++);
          int year1 = dateTime2.Year;
          DateTime pivotDate = this._model.PivotDate;
          int year2 = pivotDate.Year;
          int num2;
          if (year1 == year2)
          {
            int month1 = dateTime2.Month;
            pivotDate = this._model.PivotDate;
            int month2 = pivotDate.Month;
            num2 = month1 == month2 ? 1 : 0;
          }
          else
            num2 = 0;
          bool flag = num2 != 0;
          MonthCellControl element = new MonthCellControl(new MonthViewModel()
          {
            Date = dateTime2,
            Selected = flag
          });
          this.MonthCellsGrid.Children.Add((UIElement) element);
          Grid.SetRow((UIElement) element, index1);
          Grid.SetColumn((UIElement) element, index2);
          element.Select += new EventHandler<DateTime>(this.OnMonthSelected);
        }
      }
      this.SetGotoCurrentEnable();
    }

    private void OnMonthSelected(object sender, DateTime date)
    {
      this._model.PivotDate = this._model.PivotDate.AddDays((date.Date - this._model.PivotDate.Date).TotalDays);
      this.SetMonthSelected();
      EventHandler<DateTime> monthSelected = this.MonthSelected;
      if (monthSelected != null)
        monthSelected((object) this, this._model.PivotDate);
      if (!this._monthMode)
      {
        this.DayGrid.Visibility = Visibility.Visible;
        this.MonthCellsGrid.Visibility = Visibility.Collapsed;
        this.YearButton.Visibility = Visibility.Collapsed;
        this.YearMonthButton.Visibility = Visibility.Visible;
        this.SetDayCells();
      }
      this.RenderRepeat();
    }

    private void SetMonthSelected()
    {
      foreach (MonthCellControl child in this.MonthCellsGrid.Children)
      {
        MonthViewModel model = child.Model;
        MonthViewModel monthViewModel = model;
        DateTime dateTime = model.Date;
        int year1 = dateTime.Year;
        dateTime = this._model.PivotDate;
        int year2 = dateTime.Year;
        int num;
        if (year1 == year2)
        {
          dateTime = model.Date;
          int month1 = dateTime.Month;
          dateTime = this._model.PivotDate;
          int month2 = dateTime.Month;
          num = month1 == month2 ? 1 : 0;
        }
        else
          num = 0;
        monthViewModel.Selected = num != 0;
      }
    }

    private void GotoCurrentMonth(object sender, MouseButtonEventArgs e)
    {
      if (this.YearMonthButton.Visibility == Visibility.Visible)
      {
        this._model.PivotDate = DateTime.Today;
        if (!this.CanSwitchNext)
        {
          this.NextGrid.Cursor = Cursors.Arrow;
          this.NextGrid.MouseLeftButtonUp -= new MouseButtonEventHandler(this.NextClick);
        }
        else
        {
          this.NextGrid.Cursor = Cursors.Hand;
          this.NextGrid.MouseLeftButtonUp -= new MouseButtonEventHandler(this.NextClick);
          this.NextGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.NextClick);
        }
        if (!this.IsChooseDate)
        {
          this._model.SelectedDate = new DateTime?(DateTime.Today);
          EventHandler<DateTime> selectedDateChanged = this.SelectedDateChanged;
          if (selectedDateChanged != null)
            selectedDateChanged((object) this, DateTime.Today);
        }
        this.SetDayCells();
      }
      else
      {
        this._model.PivotDate = DateTime.Today;
        this.ClearMonthCells();
        this.SetMonthCells();
      }
      this.RenderRepeat();
    }

    public void TrySetDayCells()
    {
      if (this._monthMode)
        return;
      this.DayGrid.Visibility = Visibility.Visible;
      this.MonthCellsGrid.Visibility = Visibility.Collapsed;
      this.YearButton.Visibility = Visibility.Collapsed;
      this.YearMonthButton.Visibility = Visibility.Visible;
      this.SetDayCells();
    }

    private void OnMonthTextMouseMove(object sender, MouseEventArgs e)
    {
      if (this.AllowSelectMonth)
      {
        this.YearMonthButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.MonthClick);
        this.YearMonthButton.Cursor = Cursors.Hand;
      }
      this.YearMonthButton.MouseMove -= new MouseEventHandler(this.OnMonthTextMouseMove);
    }

    private void OnDayClicked(object sender, DayViewModel e)
    {
      if (!this.ShowInCal)
      {
        e.Selected = !this.CanMultiSelect || !e.Selected;
        if (this.IsChooseDate && !e.Selected)
        {
          e.Selected = true;
          return;
        }
        if (!e.Selected)
        {
          this._selectedDays.RemoveAll((Predicate<DateTime>) (item => item.Date == e.Date.Date));
          return;
        }
        this._selectedDays.Add(e.Date.Date);
        this.OnDateChanged(e.Date);
      }
      else
        this._model.SelectedDate = this.ShowSelectedInCal ? new DateTime?(e.Date) : new DateTime?();
      EventHandler<DateTime> selectedDateChanged = this.SelectedDateChanged;
      if (selectedDateChanged != null)
        selectedDateChanged((object) this, e.Date);
      this.SetGotoCurrentEnable();
    }

    private void OnDayDoubleClicked(object sender, DayViewModel e)
    {
      if (!e.Selected)
      {
        this._selectedDays.RemoveAll((Predicate<DateTime>) (item => item.Date == e.Date.Date));
      }
      else
      {
        this._selectedDays.Add(e.Date.Date);
        this.OnDateChanged(e.Date);
        EventHandler<DateTime> dateSelected = this.DateSelected;
        if (dateSelected == null)
          return;
        dateSelected((object) this, e.Date);
      }
    }

    public void TabSelectCurrent()
    {
      if (this.DayGrid.Visibility != Visibility.Visible || !(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      DayCellViewModel dayCellViewModel = itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (item => (item.Data.ShowMode == ShowMode.CurrentMonth || item.Data.ShowMode == ShowMode.Today) && item.Data.Date == this._tabSelectedDate)) ?? itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (item => (item.Data.ShowMode == ShowMode.CurrentMonth || item.Data.ShowMode == ShowMode.Today) && item.Data.Selected)) ?? itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (item => (item.Data.ShowMode == ShowMode.CurrentMonth || item.Data.ShowMode == ShowMode.Today) && item.Data.CanSelect)) ?? itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (item => item.Data.ShowMode == ShowMode.CurrentMonth || item.Data.ShowMode == ShowMode.Today));
      if (dayCellViewModel == null)
        return;
      dayCellViewModel.Data.TabBorderThickness = 1.0;
      this._tabSelectedDate = dayCellViewModel.Data.Date;
    }

    public void ClearTabSelected()
    {
      if (!(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      foreach (DayCellViewModel dayCellViewModel in (Collection<DayCellViewModel>) itemsSource)
        dayCellViewModel.Data.TabBorderThickness = 0.0;
      this._tabSelectedDate = new DateTime();
    }

    public void MoveTabSelectDate(int step = 1)
    {
      if (!this.DayGrid.IsVisible || !(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      DayCellViewModel dayCellViewModel1 = itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (i => Math.Abs(i.Data.TabBorderThickness - 1.0) < 0.1));
      if (dayCellViewModel1 == null)
      {
        this.TabSelectCurrent();
      }
      else
      {
        DateTime date = dayCellViewModel1.Data.Date.AddDays((double) step);
        DateTime monthStartDate = this.GetMonthStartDate(date);
        this._tabSelectedDate = date;
        if (monthStartDate.Date != itemsSource[0].Data.Date.Date)
        {
          this._model.PivotDate = date;
          this.SetDayCells();
        }
        else
        {
          DayCellViewModel dayCellViewModel2 = itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (i => i.Data.Date == date));
          if (dayCellViewModel2 == null)
            return;
          dayCellViewModel1.Data.TabBorderThickness = 0.0;
          dayCellViewModel2.Data.TabBorderThickness = 1.0;
        }
      }
    }

    public void SelectTabItem()
    {
      if (!this.DayGrid.IsVisible || !(this.DayCells.ItemsSource is ObservableCollection<DayCellViewModel> itemsSource))
        return;
      DayCellViewModel dayCellViewModel = itemsSource.FirstOrDefault<DayCellViewModel>((Func<DayCellViewModel, bool>) (i => Math.Abs(i.Data.TabBorderThickness - 1.0) < 0.1));
      if (dayCellViewModel == null || !dayCellViewModel.Data.CanSelect)
        return;
      this.OnDayClicked((object) null, dayCellViewModel.Data);
    }

    public void SetMinDate(DateTime? date) => this._minDate = date.GetValueOrDefault();

    public void SetPivotDate(DateTime monthDate)
    {
      if (monthDate.Month == this._model.PivotDate.Month && monthDate.Year == this._model.PivotDate.Year)
        return;
      this._model.PivotDate = monthDate;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/tickdatepicker.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.RootView = (TickDatePicker) target;
          this.RootView.Loaded += new RoutedEventHandler(this.OnLoaded);
          this.RootView.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
          break;
        case 2:
          this.TopGrid = (Grid) target;
          break;
        case 3:
          this.YearMonthButton = (TextBlock) target;
          this.YearMonthButton.MouseMove += new MouseEventHandler(this.OnMonthTextMouseMove);
          break;
        case 4:
          this.MonthRun = (Run) target;
          break;
        case 5:
          this.YearButton = (TextBlock) target;
          break;
        case 6:
          this.NextOrLastGrid = (Grid) target;
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.LastClick);
          break;
        case 8:
          this.GotoCurrentBt = (Ellipse) target;
          this.GotoCurrentBt.MouseLeftButtonUp += new MouseButtonEventHandler(this.GotoCurrentMonth);
          break;
        case 9:
          this.NextGrid = (Border) target;
          this.NextGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.NextClick);
          break;
        case 10:
          this.MonthCellsGrid = (Grid) target;
          break;
        case 11:
          this.DayGrid = (StackPanel) target;
          break;
        case 12:
          this.WeekDaysGrid = (Grid) target;
          break;
        case 13:
          this.DayCells = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
