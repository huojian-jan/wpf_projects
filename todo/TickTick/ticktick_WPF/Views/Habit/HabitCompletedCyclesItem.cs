// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitCompletedCyclesItem
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
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitCompletedCyclesItem : UserControl, IComponentConnector
  {
    internal Image RightImage;
    private bool _contentLoaded;

    public HabitCompletedCyclesItem()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnControlLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnControlUnloaded);
    }

    public HabitCompletedCyclesItem(HabitModel habit, int cycle)
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnControlLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnControlUnloaded);
      int? targetDays = habit.TargetDays;
      int num = 0;
      this.Visibility = targetDays.GetValueOrDefault() > num & targetDays.HasValue ? Visibility.Visible : Visibility.Collapsed;
      this.DataContext = (object) new HabitDetailCompletedCyclesViewModel(habit, cycle);
    }

    private void OnControlUnloaded(object sender, RoutedEventArgs e) => this.BindEvents();

    private void OnControlLoaded(object sender, RoutedEventArgs e) => this.UnbindEvents();

    private void BindEvents()
    {
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnThemeModeChanged);
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnThemeModeChanged);
    }

    private async void OnThemeModeChanged(object sender, EventArgs e)
    {
      if (!(this.DataContext is HabitDetailCompletedCyclesViewModel dataContext))
        return;
      dataContext.UpdateIcon();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitcompletedcyclesitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.RightImage = (Image) target;
      else
        this._contentLoaded = true;
    }
  }
}
