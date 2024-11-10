// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineArrange
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
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineArrange : UserControl, IComponentConnector, IStyleConnector
  {
    private TimelineContainer _container;
    private TimelineCellViewModel _cellModel;
    internal TimelineArrange Root;
    internal StackPanel DateFilterPanel;
    private bool _contentLoaded;

    public TimelineArrange()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this._container = Utils.FindParent<TimelineContainer>((DependencyObject) this);
    }

    private void OnOpenSetMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      new TimelineArrangeFilterPopup(dataContext, (FrameworkElement) this.DateFilterPanel).Show();
    }

    private void OnDateSelected(object sender, object e)
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.IsOverDue = e as string == "overdue";
    }

    private async void OnColumnOpenCloseMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is TimelineGroupViewModel dataContext))
        return;
      dataContext.UpdateIsArrangeOpen(!dataContext.IsArrangeOpen);
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.IsArranging = false;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/timeline/timelinearrange.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (TimelineArrange) target;
          break;
        case 3:
          this.DateFilterPanel = (StackPanel) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenSetMouseUp);
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
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
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnColumnOpenCloseMouseUp);
    }
  }
}
