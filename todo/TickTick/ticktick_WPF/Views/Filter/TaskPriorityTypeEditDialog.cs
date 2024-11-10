// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.TaskPriorityTypeEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class TaskPriorityTypeEditDialog : TaskTypeEditDialog
  {
    private static readonly Dictionary<string, string> Map = new Dictionary<string, string>()
    {
      {
        "all",
        Utils.GetString("all_priorities")
      },
      {
        "5",
        Utils.GetString("high")
      },
      {
        "3",
        Utils.GetString("medium")
      },
      {
        "1",
        Utils.GetString("low")
      },
      {
        "0",
        Utils.GetString("none")
      }
    };

    public TaskPriorityTypeEditDialog(ICollection<string> selected)
      : base(selected)
    {
    }

    protected override void InitData(ICollection<string> selected)
    {
      FilterConditionViewModel conditionViewModel = new FilterConditionViewModel();
      conditionViewModel.Type = CondType.TaskType;
      ObservableCollection<FilterListItemViewModel> observableCollection = new ObservableCollection<FilterListItemViewModel>();
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "all",
        Title = TaskPriorityTypeEditDialog.Map["all"],
        Selected = selected.Count == 0,
        IsAllItem = true
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "5",
        Title = TaskPriorityTypeEditDialog.Map["5"],
        Selected = selected.Contains("5")
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "3",
        Title = TaskPriorityTypeEditDialog.Map["3"],
        Selected = selected.Contains("3")
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "1",
        Title = TaskPriorityTypeEditDialog.Map["1"],
        Selected = selected.Contains("1")
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "0",
        Title = TaskPriorityTypeEditDialog.Map["0"],
        Selected = selected.Contains("0")
      });
      conditionViewModel.ItemsSource = observableCollection;
      conditionViewModel.IsAllSelected = false;
      conditionViewModel.SupportedLogic = new List<LogicType>();
      this.ViewModel = conditionViewModel;
    }

    public static string FormatDisplayText(List<string> priorities)
    {
      List<string> list = TaskPriorityTypeEditDialog.Map.Keys.Where<string>((Func<string, bool>) (p => priorities.Contains(p.ToString()))).Select<string, string>((Func<string, string>) (it => TaskPriorityTypeEditDialog.Map[it])).ToList<string>();
      return list.Count <= 0 ? Utils.GetString("all_priorities") : string.Join(", ", (IEnumerable<string>) list);
    }
  }
}
