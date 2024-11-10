// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.MiniCalendar
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
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class MiniCalendar : UserControl, IComponentConnector
  {
    private readonly MiniCalendarViewModel _viewModel;
    internal Grid Root;
    internal Grid DayPickerGrid;
    internal TickDatePicker DayPicker;
    private bool _contentLoaded;

    public MiniCalendar()
    {
      this.InitializeComponent();
      this._viewModel = new MiniCalendarViewModel();
      this.DataContext = (object) this._viewModel;
      this.DayPicker.UseInSlideMenu = true;
      this.DayPicker.AllowSelectMonth = true;
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
    }

    private void OnDayChanged(object sender, EventArgs e)
    {
      this.DayPicker.SetDayCells(reset: true);
    }

    private void OnDaySelected(object sender, DateTime date)
    {
      Utils.FindParent<ProjectListView>((DependencyObject) this)?.NavigateProject(nameof (date), date.ToString("yyyyMMdd"));
      this._viewModel.MiniMode = this.DayPicker.MiniMode;
      this.DayPicker.SelectedDate = new DateTime?(date);
    }

    private void OnCollapsedClick(object sender, MouseButtonEventArgs e)
    {
      this.DayPicker.MiniMode = !this.DayPicker.MiniMode;
      this._viewModel.MiniMode = this.DayPicker.MiniMode;
    }

    public void LoadIndicator() => this.DayPicker.LoadTaskIndicator();

    public void Reload() => this.DayPicker.MiniMode = this._viewModel.MiniMode;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/mainlistview/projectlist/minicalendar.xaml", UriKind.Relative));
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
          this.Root = (Grid) target;
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCollapsedClick);
          break;
        case 3:
          this.DayPickerGrid = (Grid) target;
          break;
        case 4:
          this.DayPicker = (TickDatePicker) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
