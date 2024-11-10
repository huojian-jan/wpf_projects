// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.NotificationEntityTypeEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Collections.ObjectModel;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class NotificationEntityTypeEditDialog : TaskTypeEditDialog
  {
    public NotificationEntityTypeEditDialog(List<string> selected)
      : base((ICollection<string>) selected)
    {
    }

    protected override bool CanSave() => this.GetSelectedValues().Count > 0 || base.CanSave();

    protected override void InitData(ICollection<string> selected)
    {
      FilterConditionViewModel conditionViewModel = new FilterConditionViewModel();
      conditionViewModel.Type = CondType.TaskType;
      ObservableCollection<FilterListItemViewModel> observableCollection = new ObservableCollection<FilterListItemViewModel>();
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "task",
        Title = Utils.GetString("Task"),
        Icon = "IcNormalProject",
        Selected = selected.Contains("task"),
        ShowIcon = true
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "habit",
        Title = Utils.GetString("Habits"),
        Icon = "IcLineHabit",
        Selected = selected.Contains("habit"),
        ShowIcon = true
      });
      conditionViewModel.ItemsSource = observableCollection;
      conditionViewModel.IsAllSelected = false;
      conditionViewModel.SupportedLogic = new List<LogicType>();
      this.ViewModel = conditionViewModel;
    }
  }
}
