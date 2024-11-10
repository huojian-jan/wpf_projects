// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.DayCellSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class DayCellSelector : UserControl, IComponentConnector
  {
    private bool _monthMode;
    private int _dayOfMonth;
    private DayOfWeek _day;
    public static readonly DependencyProperty ItemSelectedProperty = DependencyProperty.Register(nameof (ItemSelected), typeof (bool), typeof (DayCellSelector), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    public static readonly DependencyProperty DayTextProperty = DependencyProperty.Register(nameof (DayText), typeof (string), typeof (DayCellSelector), new PropertyMetadata((object) string.Empty, (PropertyChangedCallback) null));
    internal DayCellSelector RootView;
    internal Rectangle HoverBackground;
    internal Rectangle SelectedBackground;
    internal TextBlock Content;
    internal Rectangle TabSelectedRect;
    private bool _contentLoaded;

    public bool TabSelected => this.TabSelectedRect.Visibility == Visibility.Visible;

    public event EventHandler<bool> SelectedChanged;

    public DayOfWeek Day
    {
      get => this._day;
      set
      {
        this._day = value;
        this.SetValue(DayCellSelector.DayTextProperty, (object) this.GetDayText());
      }
    }

    public bool ItemSelected
    {
      get => (bool) this.GetValue(DayCellSelector.ItemSelectedProperty);
      set => this.SetValue(DayCellSelector.ItemSelectedProperty, (object) value);
    }

    public int DayOfMonth
    {
      get => this._dayOfMonth;
      set
      {
        this._dayOfMonth = value;
        this.SetValue(DayCellSelector.DayTextProperty, (object) this.GetDayText());
      }
    }

    public bool MonthMode
    {
      get => this._monthMode;
      set
      {
        this._monthMode = value;
        this.SetValue(DayCellSelector.DayTextProperty, (object) this.GetDayText());
      }
    }

    private string GetDayText()
    {
      if (!this.MonthMode)
        return Utils.GetWeekLabel(this.Day);
      return this.DayOfMonth != -1 ? this.DayOfMonth.ToString() : Utils.GetString("LastDay");
    }

    public string DayText
    {
      get => (string) this.GetValue(DayCellSelector.DayTextProperty);
      set => this.SetValue(DayCellSelector.DayTextProperty, (object) value);
    }

    public DayCellSelector(double width = 27.0)
    {
      this.InitializeComponent();
      this.Width = width;
      this.HoverBackground.Width = width;
      this.SelectedBackground.Width = width;
      this.TabSelectedRect.Width = width;
    }

    private void OnCellClick(object sender, MouseButtonEventArgs e) => this.Select();

    public void Select()
    {
      this.ItemSelected = !this.ItemSelected;
      EventHandler<bool> selectedChanged = this.SelectedChanged;
      if (selectedChanged == null)
        return;
      selectedChanged((object) this, this.ItemSelected);
    }

    public void SetTabSelected(bool tabSelect)
    {
      this.TabSelectedRect.Visibility = tabSelect ? Visibility.Visible : Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/daycellselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.RootView = (DayCellSelector) target;
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCellClick);
          break;
        case 3:
          this.HoverBackground = (Rectangle) target;
          break;
        case 4:
          this.SelectedBackground = (Rectangle) target;
          break;
        case 5:
          this.Content = (TextBlock) target;
          break;
        case 6:
          this.TabSelectedRect = (Rectangle) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
