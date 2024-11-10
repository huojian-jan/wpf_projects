// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.SetHabitReminderControl
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class SetHabitReminderControl : UserControl, IComponentConnector, IStyleConnector
  {
    internal ItemsControl ReminderItems;
    internal EscPopup TimePopup;
    private bool _contentLoaded;

    public ObservableCollection<HabitReminderViewModel> Items
    {
      get => this.ReminderItems.ItemsSource as ObservableCollection<HabitReminderViewModel>;
    }

    public event EventHandler<string> Toast;

    public SetHabitReminderControl() => this.InitializeComponent();

    public void Init(string habitReminder)
    {
      List<HabitReminderViewModel> reminderViewModelList1 = new List<HabitReminderViewModel>();
      if (!string.IsNullOrEmpty(habitReminder))
      {
        string[] source = habitReminder.Split(',');
        if (((IEnumerable<string>) source).Any<string>())
        {
          foreach (string str in source)
          {
            if (str.Contains(":"))
            {
              string[] strArray = str.Split(':');
              int hour;
              int min;
              if (strArray.Length == 2 && int.TryParse(strArray[0], out hour) && int.TryParse(strArray[1], out min) && reminderViewModelList1.All<HabitReminderViewModel>((Func<HabitReminderViewModel, bool>) (i => i.Time.Hour != hour || i.Time.Minute != min)))
              {
                List<HabitReminderViewModel> reminderViewModelList2 = reminderViewModelList1;
                HabitReminderViewModel reminderViewModel = new HabitReminderViewModel();
                DateTime dateTime = DateTime.Today;
                dateTime = dateTime.AddHours((double) hour);
                reminderViewModel.Time = dateTime.AddMinutes((double) min);
                reminderViewModelList2.Add(reminderViewModel);
              }
            }
          }
        }
      }
      reminderViewModelList1.Sort((Comparison<HabitReminderViewModel>) ((l, r) => l.Time.CompareTo(r.Time)));
      reminderViewModelList1.Add(new HabitReminderViewModel()
      {
        IsAdd = true
      });
      SetHabitReminderControl.SortItems(reminderViewModelList1);
      this.ReminderItems.ItemsSource = (IEnumerable) new ObservableCollection<HabitReminderViewModel>(reminderViewModelList1);
    }

    private void AddNewReminderClick(object sender, RoutedEventArgs e)
    {
      if (this.Items.Count > 10)
      {
        EventHandler<string> toast = this.Toast;
        if (toast == null)
          return;
        toast((object) this, Utils.GetString("HabitReminderLimite"));
      }
      else
      {
        if (sender is Button button)
        {
          this.TimePopup.PlacementTarget = (UIElement) button;
          button.Background = (Brush) ThemeUtil.GetColor("PrimaryColor");
          if (button.Content is Path content)
            content.Fill = (Brush) Brushes.White;
        }
        this.TimePopup.IsOpen = true;
      }
    }

    private void AddReminder(object sender, DateTime time)
    {
      if (this.Items.Any<HabitReminderViewModel>((Func<HabitReminderViewModel, bool>) (i => !Utils.IsEmptyDate(i.Time) && DateUtils.IsEqualTime(time, i.Time))))
      {
        EventHandler<string> toast = this.Toast;
        if (toast != null)
          toast((object) this, Utils.GetString("HabitReminderExist"));
      }
      else
      {
        UserActCollectUtils.AddClickEvent("habit", "add_edit_habit", "add_reminder");
        if (this.TimePopup.PlacementTarget is Button placementTarget)
        {
          placementTarget.Background = (Brush) ThemeUtil.GetColor("BaseColorOpacity10_20");
          if (placementTarget.Content is Path content)
            content.Fill = (Brush) ThemeUtil.GetColor("BaseColorOpacity40");
        }
        this.Items.Insert(this.GetInsertIndex(time), new HabitReminderViewModel()
        {
          Time = time
        });
      }
      this.TimePopup.IsOpen = false;
    }

    private int GetInsertIndex(DateTime time)
    {
      if (this.Items.Count > 1)
      {
        for (int index = 0; index < this.Items.Count - 1; ++index)
        {
          if (this.Items[index].Time > time)
            return index;
        }
      }
      return Math.Max(0, this.Items.Count - 1);
    }

    private static void SortItems(List<HabitReminderViewModel> items)
    {
      items.Sort((Comparison<HabitReminderViewModel>) ((a, b) =>
      {
        if (b.IsAdd && !a.IsAdd)
          return -1;
        if (a.IsAdd && !b.IsAdd)
          return 1;
        return a.Time.Hour != b.Time.Hour ? a.Time.Hour.CompareTo(b.Time.Hour) : a.Time.Minute.CompareTo(b.Time.Minute);
      }));
    }

    private void OnReminderChanged(object sender, DateTime time)
    {
      if (!(sender is TimeInputControl child))
        return;
      Grid parent = Utils.FindParent<Grid>((DependencyObject) child);
      if (parent == null || !(parent.DataContext is HabitReminderViewModel dataContext))
        return;
      if (this.Items.Any<HabitReminderViewModel>((Func<HabitReminderViewModel, bool>) (i => DateUtils.IsEqualTime(time, i.Time))))
        child.SetTimeText(dataContext.Time);
      else
        dataContext.Time = time;
    }

    public string GetReminderString()
    {
      string str = string.Join(",", (IEnumerable<string>) this.Items.Where<HabitReminderViewModel>((Func<HabitReminderViewModel, bool>) (i => !i.IsAdd)).ToList<HabitReminderViewModel>().Select<HabitReminderViewModel, string>((Func<HabitReminderViewModel, string>) (r => r.Time.ToString("HH':'mm"))).ToList<string>());
      return !string.IsNullOrEmpty(str) ? str : (string) null;
    }

    private void DeleteReminder(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Border border) || !(border.DataContext is HabitReminderViewModel dataContext))
        return;
      this.Items.Remove(dataContext);
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      if (!(sender is Popup popup) || !(popup.PlacementTarget is Button placementTarget))
        return;
      placementTarget.Background = (Brush) ThemeUtil.GetColor("BaseColorOpacity10_20");
      if (!(placementTarget.Content is Path content))
        return;
      content.Fill = (Brush) ThemeUtil.GetColor("BaseColorOpacity40");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/sethabitremindercontrol.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 4)
          this.TimePopup = (EscPopup) target;
        else
          this._contentLoaded = true;
      }
      else
        this.ReminderItems = (ItemsControl) target;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
      {
        if (connectionId != 3)
          return;
        ((ButtonBase) target).Click += new RoutedEventHandler(this.AddNewReminderClick);
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.DeleteReminder);
    }
  }
}
