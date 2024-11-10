// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitLogControl
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
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitLogControl : UserControl, IComponentConnector
  {
    private bool _showEdit;
    internal HabitLogControl Root;
    internal TextBlock CheckInRecordHint;
    internal ItemsControl LogItems;
    private bool _contentLoaded;

    public HabitLogControl() => this.InitializeComponent();

    public async void Load(
      string habitId,
      DateTime startDate,
      DateTime endDate,
      List<HabitCheckInModel> checkIns,
      List<HabitRecordModel> records)
    {
      if (!(this.LogItems.ItemsSource is ObservableCollection<CheckInLogViewModel>))
        this.LogItems.ItemsSource = (IEnumerable) new ObservableCollection<CheckInLogViewModel>();
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      List<CheckInLogViewModel> checkInLogViewModelList = new List<CheckInLogViewModel>();
      if (habitById != null && records != null && records.Any<HabitRecordModel>())
      {
        List<HabitRecordModel> source = new List<HabitRecordModel>();
        HashSet<string> ids = new HashSet<string>();
        foreach (HabitRecordModel habitRecordModel in records.Where<HabitRecordModel>((Func<HabitRecordModel, bool>) (record => string.IsNullOrEmpty(record.Id) || !ids.Contains(record.Id))))
        {
          ids.Add(habitRecordModel.Id);
          source.Add(habitRecordModel);
        }
        HashSet<int> stamps = new HashSet<int>();
        List<HabitRecordModel> list = source.OrderByDescending<HabitRecordModel, int>((Func<HabitRecordModel, int>) (m => m.Stamp)).ToList<HabitRecordModel>();
        int startNum = DateUtils.GetDateNum(startDate);
        int endNum = DateUtils.GetDateNum(endDate);
        foreach (HabitRecordModel habitRecordModel in list.Where<HabitRecordModel>((Func<HabitRecordModel, bool>) (record => !stamps.Contains(record.Stamp) && record.Stamp >= startNum && record.Stamp <= endNum)))
        {
          HabitRecordModel record = habitRecordModel;
          stamps.Add(record.Stamp);
          HabitCheckInModel habitCheckInModel = checkIns.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (check => check.CheckinStamp == record.Stamp.ToString()));
          bool completed = habitCheckInModel != null && habitCheckInModel.Value >= habitCheckInModel.Goal;
          bool unCompleted = habitCheckInModel != null && habitCheckInModel.CheckStatus == 1;
          checkInLogViewModelList.Add(new CheckInLogViewModel(record, habitById, completed, unCompleted, this._showEdit));
        }
      }
      this.CheckInRecordHint.Visibility = checkInLogViewModelList.Any<CheckInLogViewModel>() ? Visibility.Collapsed : Visibility.Visible;
      checkInLogViewModelList.ForEach((Action<CheckInLogViewModel>) (d => d.LineVisibility = Visibility.Visible));
      if (checkInLogViewModelList.Count > 0)
        checkInLogViewModelList[checkInLogViewModelList.Count - 1].LineVisibility = Visibility.Collapsed;
      ItemsSourceHelper.SetHidableItemsSource<CheckInLogViewModel>(this.LogItems, checkInLogViewModelList);
    }

    public void OnShowEditChanged(bool showEdit)
    {
      this._showEdit = showEdit;
      if (!(this.LogItems.ItemsSource is ObservableCollection<CheckInLogViewModel> itemsSource))
        return;
      foreach (CheckInLogViewModel checkInLogViewModel in (Collection<CheckInLogViewModel>) itemsSource)
        checkInLogViewModel.ShowEdit = showEdit;
    }

    public void OnItemDeleted()
    {
      if (!(this.LogItems.ItemsSource is ObservableCollection<CheckInLogViewModel> itemsSource))
        return;
      this.CheckInRecordHint.Visibility = itemsSource.Any<CheckInLogViewModel>((Func<CheckInLogViewModel, bool>) (item => !item.IsHide)) ? Visibility.Collapsed : Visibility.Visible;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitlogcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (HabitLogControl) target;
          break;
        case 2:
          this.CheckInRecordHint = (TextBlock) target;
          break;
        case 3:
          this.LogItems = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
