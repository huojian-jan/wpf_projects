// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitDayCell
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
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitDayCell : UserControl, IComponentConnector
  {
    internal HabitDayCell Root;
    internal Grid DayCellGrid;
    private bool _contentLoaded;

    public HabitDayCell() => this.InitializeComponent();

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (this.DataContext == null || !(this.DataContext is DayViewModel dataContext))
        return;
      dataContext.Hover = true;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (this.DataContext == null || !(this.DataContext is DayViewModel dataContext))
        return;
      dataContext.Hover = false;
    }

    private async void OnClick(object sender, MouseButtonEventArgs e)
    {
      HabitDayCell child = this;
      e.Handled = true;
      if (child.DataContext == null || !(child.DataContext is HabitDayViewModel dataContext) || dataContext.Date > DateTime.Today)
        return;
      if (dataContext.Date < DateTime.Today.AddDays(-90.0))
        Utils.Toast(Utils.GetString("CheckInExpiredDateHint"));
      else if (!dataContext.Habit.IsBoolHabit())
      {
        Utils.FindParent<HabitMonthGrid>((DependencyObject) child)?.ManuallyRecord(dataContext.Date, true);
      }
      else
      {
        HabitUtils.BuildHabitRepeatInfo(dataContext.Habit.RepeatRule);
        if (dataContext.Completed || dataContext.UnCompleted)
        {
          dataContext.Completed = false;
          dataContext.UnCompleted = false;
          int num = await HabitService.ResetCheckInHabit(dataContext.HabitId, dataContext.Date, dataContext.Habit.Type) ? 1 : 0;
        }
        else
        {
          dataContext.Completed = true;
          await HabitService.CheckInHabit(dataContext.HabitId, dataContext.Date);
        }
      }
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (this.DataContext == null)
        return;
      HabitDayViewModel model = this.DataContext as HabitDayViewModel;
      if (model == null)
        return;
      if (model.Date < DateTime.Today.AddDays(-90.0))
      {
        Utils.Toast(Utils.GetString("CheckInExpiredDateHint"));
      }
      else
      {
        if (model.Date > DateTime.Today)
          return;
        if (model.Completed || model.UnCompleted)
        {
          model.Hover = true;
          OperationDialog operationDialog = new OperationDialog(model.HabitId, new OperationItemViewModel(ActionType.EditHabitLog));
          operationDialog.SetPlaceTarget((UIElement) this);
          operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) (async (obj, arg) =>
          {
            HabitUtils.ShowHabitRecordWindow(model.HabitId, model.Habit.Name, model.Date, unCompleted: model.UnCompleted);
            model.Hover = false;
          });
          operationDialog.Closed += (EventHandler) ((obj, arg) => model.Hover = false);
          operationDialog.Show();
        }
        else
        {
          model.Hover = true;
          OperationDialog operationDialog = new OperationDialog(model.HabitId, new List<OperationItemViewModel>()
          {
            new OperationItemViewModel(ActionType.UnComplete),
            new OperationItemViewModel(ActionType.EditHabitLog)
          });
          operationDialog.SetPlaceTarget((UIElement) this);
          operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((obj, arg) =>
          {
            if (arg.Value == ActionType.UnComplete)
              this.UncompleteHabit(model);
            else
              HabitUtils.ShowHabitRecordWindow(model.HabitId, model.Habit.Name, model.Date, unCompleted: !model.Completed);
            model.Hover = false;
          });
          operationDialog.Closed += (EventHandler) ((obj, arg) => model.Hover = false);
          operationDialog.Show();
        }
      }
    }

    private async void UncompleteHabit(HabitDayViewModel model)
    {
      model.UnCompleted = true;
      await HabitService.UncompleteHabit(model.HabitId, model.Date);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitdaycell.xaml", UriKind.Relative));
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
          this.Root = (HabitDayCell) target;
          this.Root.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
          this.Root.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
          break;
        case 2:
          this.DayCellGrid = (Grid) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
          ((UIElement) target).MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
