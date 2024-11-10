// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SetTimeControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class SetTimeControl : UserControl, IComponentConnector
  {
    internal Grid ChooseTimePopupGrid;
    internal Calendar Calendar;
    private bool _contentLoaded;

    public event EventHandler<DateTime> DateSelected;

    public SetTimeControl()
      : this(DateTime.Now)
    {
    }

    public SetTimeControl(DateTime dateTime)
    {
      this.InitializeComponent();
      this.Calendar.SelectedDate = new DateTime?(dateTime);
      this.Calendar.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(this.OnDateSelected);
      this.DataContext = (object) new SelectDateViewModel()
      {
        SelectedDate = dateTime
      };
    }

    private void OnDateSelected(object sender, SelectionChangedEventArgs e)
    {
      if (!this.Calendar.SelectedDate.HasValue)
        return;
      EventHandler<DateTime> dateSelected = this.DateSelected;
      if (dateSelected == null)
        return;
      dateSelected((object) this, this.Calendar.SelectedDate.Value);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/settimecontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.Calendar = (Calendar) target;
        else
          this._contentLoaded = true;
      }
      else
        this.ChooseTimePopupGrid = (Grid) target;
    }
  }
}
