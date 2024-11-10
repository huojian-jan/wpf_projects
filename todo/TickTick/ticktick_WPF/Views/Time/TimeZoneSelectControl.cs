// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.TimeZoneSelectControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class TimeZoneSelectControl : UserControl, IComponentConnector, IStyleConnector
  {
    public static readonly DependencyProperty TabSelectedProperty = DependencyProperty.Register(nameof (TabSelected), typeof (bool), typeof (TimeZoneSelectControl), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    private static List<TimeZoneViewModel> _models;
    private readonly TimeZoneViewModel _localTimeZone = TimeZoneData.LocalTimeZoneModel;
    private TimeZoneViewModel _oldTimeZone = new TimeZoneViewModel();
    private static readonly TimeZoneViewModel FloatModel = TimeZoneViewModel.GetFloatModel();
    private TimeZoneViewModel _selectedTimeZone;
    private List<TimeZoneViewModel> _displayItems;
    internal TimeZoneSelectControl Root;
    internal Border BackBorder;
    internal TextBlock SelectedTimeZone;
    internal Path DropArrow;
    internal Popup OptionPopup;
    internal ContentControl DropContent;
    internal RadioButton TimeZoneRadio;
    internal Grid SelectTimeZoneGrid;
    internal TextBlock SelectedTimeZoneText;
    internal EscPopup TimeZonePopup;
    internal TextBox SearchText;
    internal Button ClearButton;
    internal ListView TimeZoneItems;
    internal RadioButton KeepTimeRadio;
    private bool _contentLoaded;

    public bool TabSelected
    {
      get => (bool) this.GetValue(TimeZoneSelectControl.TabSelectedProperty);
      set => this.SetValue(TimeZoneSelectControl.TabSelectedProperty, (object) value);
    }

    public event EventHandler<TimeZoneViewModel> OnTimeZoneChanged;

    public TimeZoneSelectControl(TimeZoneViewModel model = null)
    {
      this.InitializeComponent();
      this.GetTimeZoneModels();
      this.SetData(model);
    }

    public void SetData(TimeZoneViewModel model)
    {
      this._selectedTimeZone = model ?? this._localTimeZone;
      this.SelectedTimeZone.Text = TimeZoneViewModel.GetShortName(this._selectedTimeZone.DisplayName);
      this.SelectedTimeZone.ToolTip = (object) this._selectedTimeZone.DisplayName;
    }

    private void GetTimeZoneModels()
    {
      if (TimeZoneSelectControl._models != null)
        return;
      TimeZoneSelectControl._models = new List<TimeZoneViewModel>();
      foreach (TimeZoneInfo tz in TimeZoneInfo.GetSystemTimeZones().ToList<TimeZoneInfo>())
      {
        try
        {
          TimeZoneSelectControl._models.Add(new TimeZoneViewModel()
          {
            TimeZone = tz,
            DisplayName = TimeZoneUtils.GetTimeZoneDisplayName(tz),
            TimeZoneName = TimeZoneUtils.GetTimeZoneName(tz),
            IsFloat = false
          });
        }
        catch (Exception ex)
        {
        }
      }
      TimeZoneSelectControl._models.Sort((Comparison<TimeZoneViewModel>) ((a, b) =>
      {
        int num = a.TimeZone.BaseUtcOffset.CompareTo(b.TimeZone.BaseUtcOffset);
        return num == 0 ? string.Compare(a.DisplayName, b.DisplayName, new CultureInfo(2052), CompareOptions.None) : num;
      }));
    }

    private void ShowPopupClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.ShowPopup();
    }

    public void ShowPopup()
    {
      this.SelectedTimeZoneText.Text = this._selectedTimeZone.IsFloat ? this._localTimeZone.DisplayName : this._selectedTimeZone.DisplayName;
      this.SelectedTimeZoneText.IsEnabled = !this._selectedTimeZone.IsFloat;
      this.OptionPopup.IsOpen = true;
      this.TimeZoneRadio.IsChecked = new bool?(!this._selectedTimeZone.IsFloat);
      this.KeepTimeRadio.IsChecked = new bool?(this._selectedTimeZone.IsFloat);
      this.SelectedTimeZoneText.Opacity = this._selectedTimeZone.IsFloat ? 0.36 : 0.85;
      this._oldTimeZone = this._selectedTimeZone;
    }

    private void TimeZoneFloatClick(object sender, RoutedEventArgs routedEventArgs)
    {
      this.SelectedTimeZoneText.Text = this._localTimeZone.DisplayName;
      this.SelectedTimeZoneText.ToolTip = (object) this._localTimeZone.DisplayName;
      this._selectedTimeZone = TimeZoneSelectControl.FloatModel;
      this.SelectedTimeZoneText.Opacity = 0.36;
    }

    private void SelectTimeZoneCheck(object sender, RoutedEventArgs e)
    {
      if (!this._selectedTimeZone.IsFloat)
        return;
      this._selectedTimeZone = this._localTimeZone;
      this.SelectedTimeZoneText.Text = this._localTimeZone.DisplayName;
      this.SelectedTimeZoneText.ToolTip = (object) this._localTimeZone.DisplayName;
      this.SelectedTimeZoneText.Opacity = 0.85;
    }

    private async void SelectTimeZoneClick(object sender, MouseButtonEventArgs e)
    {
      this.TimeZonePopup.IsOpen = true;
      List<TimeZoneViewModel> displayModels = this.GetDisplayModels();
      this._displayItems = displayModels;
      ItemsSourceHelper.SetItemsSource<TimeZoneViewModel>((ItemsControl) this.TimeZoneItems, displayModels);
      this.TimeZoneItems.ScrollIntoView((object) displayModels[0]);
      this.SearchText.Text = "";
      this.SearchText.Focus();
      TimeZoneSelectControl.SetFocus(this.GetHwnd((Popup) this.TimeZonePopup));
    }

    public void SetDropContentWidth(double width) => this.DropContent.Width = width;

    private List<TimeZoneViewModel> GetDisplayModels()
    {
      List<TimeZoneViewModel> displayModels = new List<TimeZoneViewModel>();
      List<TimeZoneViewModel> list = TimeZoneSelectControl._models.ToList<TimeZoneViewModel>();
      TimeZoneViewModel timeZoneViewModel1 = list.FirstOrDefault<TimeZoneViewModel>((Func<TimeZoneViewModel, bool>) (f => f.TimeZoneName == this._localTimeZone.TimeZoneName));
      if (timeZoneViewModel1 != null)
      {
        displayModels.Add(timeZoneViewModel1);
        list.Remove(timeZoneViewModel1);
      }
      TimeZoneViewModel timeZoneViewModel2 = list.FirstOrDefault<TimeZoneViewModel>((Func<TimeZoneViewModel, bool>) (f => f.TimeZoneName == this._selectedTimeZone?.TimeZoneName));
      if (timeZoneViewModel2 != null)
      {
        displayModels.Add(timeZoneViewModel2);
        list.Remove(timeZoneViewModel2);
      }
      string recentTimezone = LocalSettings.Settings.ExtraSettings.RecentTimezone;
      List<string> stringList;
      if (recentTimezone == null)
        stringList = (List<string>) null;
      else
        stringList = ((IEnumerable<string>) recentTimezone.Split(';')).ToList<string>();
      if (stringList == null)
        stringList = new List<string>();
      foreach (string str in stringList)
      {
        string re = str;
        if (displayModels.Count < 4)
        {
          TimeZoneViewModel timeZoneViewModel3 = list.FirstOrDefault<TimeZoneViewModel>((Func<TimeZoneViewModel, bool>) (f => f.TimeZoneName == re));
          if (timeZoneViewModel3 != null)
          {
            displayModels.Add(timeZoneViewModel3);
            list.Remove(timeZoneViewModel3);
          }
        }
      }
      displayModels.Add(new TimeZoneViewModel()
      {
        IsSplit = true
      });
      displayModels.AddRange((IEnumerable<TimeZoneViewModel>) list);
      displayModels.ForEach((Action<TimeZoneViewModel>) (m => m.Selected = m.TimeZoneName == this._selectedTimeZone?.TimeZoneName));
      return displayModels;
    }

    private void KnowMoreClick(object sender, MouseButtonEventArgs e)
    {
      string url = "https://help.dida365.com/faqs/6609623323343060992";
      try
      {
        Utils.TryProcessStartUrl(url);
        this.OptionPopup.IsOpen = false;
      }
      catch (Exception ex)
      {
        this.OptionPopup.IsOpen = false;
      }
    }

    private void OnTimeZoneSelect(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Border border) || !(border.DataContext is TimeZoneViewModel dataContext))
        return;
      this.SelectedTimeZoneText.Text = dataContext.DisplayName;
      this.SelectedTimeZoneText.ToolTip = (object) dataContext.DisplayName;
      this._selectedTimeZone = dataContext;
      this.TimeZonePopup.IsOpen = false;
      this.SaveRecent(dataContext.TimeZoneName);
    }

    private void SaveRecent(string timeZone)
    {
      string recentTimezone = LocalSettings.Settings.ExtraSettings.RecentTimezone;
      List<string> stringList;
      if (recentTimezone == null)
        stringList = (List<string>) null;
      else
        stringList = ((IEnumerable<string>) recentTimezone.Split(';')).ToList<string>();
      if (stringList == null)
        stringList = new List<string>();
      List<string> items = stringList;
      if (items.Contains(timeZone))
        items.Remove(timeZone);
      items.Insert(0, timeZone);
      if (items.Count > 4)
        items.RemoveRange(4, items.Count - 4);
      LocalSettings.Settings.ExtraSettings.RecentTimezone = items.Join<string>(";");
    }

    private void NotifyTimeZoneChanged()
    {
      if (this._selectedTimeZone.IsFloat == this._oldTimeZone.IsFloat && this._selectedTimeZone.TimeZone.Equals(this._oldTimeZone.TimeZone))
        return;
      EventHandler<TimeZoneViewModel> onTimeZoneChanged = this.OnTimeZoneChanged;
      if (onTimeZoneChanged == null)
        return;
      onTimeZoneChanged((object) null, this._selectedTimeZone);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!string.IsNullOrEmpty(this.SearchText.Text))
      {
        List<TimeZoneViewModel> displayItems = this._displayItems;
        ItemsSourceHelper.SetItemsSource<TimeZoneViewModel>((ItemsControl) this.TimeZoneItems, displayItems != null ? displayItems.Where<TimeZoneViewModel>((Func<TimeZoneViewModel, bool>) (m => m.TimeZone != null && m.DisplayName.ToLower().Contains(this.SearchText.Text.Trim().ToLower()) && !m.TimeZone.Equals(this._selectedTimeZone.TimeZone))).ToList<TimeZoneViewModel>() : (List<TimeZoneViewModel>) null);
        this.ClearButton.Visibility = Visibility.Visible;
      }
      else
      {
        ItemsSourceHelper.SetItemsSource<TimeZoneViewModel>((ItemsControl) this.TimeZoneItems, this._displayItems);
        this.ClearButton.Visibility = Visibility.Collapsed;
      }
    }

    [DllImport("User32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    private IntPtr GetHwnd(Popup popup)
    {
      return ((HwndSource) PresentationSource.FromVisual((Visual) popup.Child)).Handle;
    }

    private void OnTopTimezoneClick(object sender, MouseButtonEventArgs e)
    {
      this.TimeZonePopup.IsOpen = false;
    }

    private void ClearText(object sender, RoutedEventArgs e) => this.SearchText.Text = "";

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.OptionPopup.IsOpen = false;

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this.NotifyTimeZoneChanged();
      this.SelectedTimeZone.Text = TimeZoneViewModel.GetShortName(this._selectedTimeZone.DisplayName);
      this.SelectedTimeZone.ToolTip = (object) this._selectedTimeZone.DisplayName;
      this._oldTimeZone = this._selectedTimeZone;
      this.OptionPopup.IsOpen = false;
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      this._selectedTimeZone = this._oldTimeZone;
    }

    public bool ClosePopup()
    {
      if (!this.OptionPopup.IsOpen)
        return false;
      this.OptionPopup.IsOpen = false;
      return true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/timezoneselectcontrol.xaml", UriKind.Relative));
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
          this.Root = (TimeZoneSelectControl) target;
          break;
        case 2:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ShowPopupClick);
          break;
        case 3:
          this.BackBorder = (Border) target;
          break;
        case 4:
          this.SelectedTimeZone = (TextBlock) target;
          break;
        case 5:
          this.DropArrow = (Path) target;
          break;
        case 6:
          this.OptionPopup = (Popup) target;
          this.OptionPopup.Closed += new EventHandler(this.OnPopupClosed);
          break;
        case 7:
          this.DropContent = (ContentControl) target;
          break;
        case 8:
          this.TimeZoneRadio = (RadioButton) target;
          this.TimeZoneRadio.Click += new RoutedEventHandler(this.SelectTimeZoneCheck);
          break;
        case 9:
          this.SelectTimeZoneGrid = (Grid) target;
          this.SelectTimeZoneGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectTimeZoneClick);
          break;
        case 10:
          this.SelectedTimeZoneText = (TextBlock) target;
          break;
        case 11:
          this.TimeZonePopup = (EscPopup) target;
          break;
        case 12:
          this.SearchText = (TextBox) target;
          this.SearchText.TextChanged += new TextChangedEventHandler(this.OnSearchTextChanged);
          break;
        case 13:
          this.ClearButton = (Button) target;
          this.ClearButton.Click += new RoutedEventHandler(this.ClearText);
          break;
        case 14:
          this.TimeZoneItems = (ListView) target;
          break;
        case 16:
          this.KeepTimeRadio = (RadioButton) target;
          this.KeepTimeRadio.Click += new RoutedEventHandler(this.TimeZoneFloatClick);
          break;
        case 17:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.KnowMoreClick);
          break;
        case 18:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 19:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 15)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTimeZoneSelect);
    }
  }
}
