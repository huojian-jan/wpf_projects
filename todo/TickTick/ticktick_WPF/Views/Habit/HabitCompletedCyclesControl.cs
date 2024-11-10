// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitCompletedCyclesControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitCompletedCyclesControl : UserControl, IComponentConnector
  {
    private HabitModel _habit;
    internal Grid CompletedCyclesCurrentGrid;
    internal TextBlock MoreButton;
    internal EscPopup CompletedCyclesDetailPopup;
    internal ListView CompletedCyclesDetailList;
    private bool _contentLoaded;

    public HabitModel Habit
    {
      get => this._habit;
      set
      {
        this._habit = value;
        this.Reload();
      }
    }

    public HabitCompletedCyclesControl()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    private void BindEvent()
    {
      DataChangedNotifier.HabitCyclesChanged -= new EventHandler<string>(this.OnHabitCyclesChanged);
      DataChangedNotifier.HabitCyclesChanged += new EventHandler<string>(this.OnHabitCyclesChanged);
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.HabitCyclesChanged -= new EventHandler<string>(this.OnHabitCyclesChanged);
    }

    private async void OnHabitCyclesChanged(object sender, string habitId)
    {
      if (!(habitId == this._habit?.Id))
        return;
      this.Habit = await HabitDao.GetHabitById(habitId) ?? this.Habit;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Reload();
      this.BindEvent();
    }

    private async void Reload()
    {
      HabitCompletedCyclesControl completedCyclesControl = this;
      if (completedCyclesControl.Habit == null || !completedCyclesControl.IsLoaded)
        return;
      int? nullable = completedCyclesControl.Habit.TargetDays;
      int num = 0;
      if (nullable.GetValueOrDefault() <= num & nullable.HasValue)
        return;
      completedCyclesControl.CompletedCyclesCurrentGrid.Children.Clear();
      nullable = completedCyclesControl.Habit.CompletedCycles;
      int valueOrDefault = nullable.GetValueOrDefault();
      HabitCompletedCyclesItem element = new HabitCompletedCyclesItem(completedCyclesControl.Habit, valueOrDefault);
      if (completedCyclesControl.Habit == null || completedCyclesControl.Habit.CompletedCyclesList == null || completedCyclesControl.Habit.CompletedCyclesList.Count <= 1 || !completedCyclesControl.Habit.CompletedCyclesList[0].isComplete)
        completedCyclesControl.MoreButton.Visibility = Visibility.Collapsed;
      else
        completedCyclesControl.MoreButton.Visibility = Visibility.Visible;
      completedCyclesControl.CompletedCyclesCurrentGrid.Children.Add((UIElement) element);
    }

    private void OnClicked(object sender, MouseButtonEventArgs e)
    {
      List<HabitDetailCompletedCyclesViewModel> items = new List<HabitDetailCompletedCyclesViewModel>();
      for (int index = this.Habit.CompletedCyclesList.Count - 1; index >= 0; --index)
      {
        if (this.Habit.CompletedCyclesList[index].isComplete)
          items.Add(new HabitDetailCompletedCyclesViewModel(this.Habit, index));
      }
      ItemsSourceHelper.SetItemsSource<HabitDetailCompletedCyclesViewModel>((ItemsControl) this.CompletedCyclesDetailList, items);
      this.CompletedCyclesDetailPopup.IsOpen = true;
    }

    private void OnCloseClicked(object sender, MouseButtonEventArgs e)
    {
      this.CompletedCyclesDetailPopup.IsOpen = false;
    }

    public void Dispose()
    {
      DataChangedNotifier.HabitCyclesChanged -= new EventHandler<string>(this.OnHabitCyclesChanged);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitcompletedcyclescontrol.xaml", UriKind.Relative));
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
          this.CompletedCyclesCurrentGrid = (Grid) target;
          break;
        case 2:
          this.MoreButton = (TextBlock) target;
          this.MoreButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClicked);
          break;
        case 3:
          this.CompletedCyclesDetailPopup = (EscPopup) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClicked);
          break;
        case 5:
          this.CompletedCyclesDetailList = (ListView) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
