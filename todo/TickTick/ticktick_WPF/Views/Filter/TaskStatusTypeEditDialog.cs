// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.TaskStatusTypeEditDialog
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
  public class TaskStatusTypeEditDialog : TaskTypeEditDialog
  {
    private static readonly Dictionary<string, string> Map = new Dictionary<string, string>()
    {
      {
        "all",
        Utils.GetString("AllStatus")
      },
      {
        "completed",
        Utils.GetString("Completed")
      },
      {
        "inProgress",
        Utils.GetString("SummaryInProgress")
      },
      {
        "uncompleted",
        Utils.GetString("Uncompleted")
      },
      {
        "wontDo",
        Utils.GetString("Abandoned")
      }
    };

    public TaskStatusTypeEditDialog(ICollection<string> selected)
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
        Title = Utils.GetString("AllStatus"),
        Selected = selected.Count == 0,
        IsAllItem = true
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "completed",
        Title = Utils.GetString("Completed"),
        Selected = selected.Contains("completed")
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "inProgress",
        Title = Utils.GetString("SummaryInProgress"),
        Selected = selected.Contains("inProgress")
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "uncompleted",
        Title = Utils.GetString("Uncompleted"),
        Selected = selected.Contains("uncompleted")
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "wontDo",
        Title = Utils.GetString("Abandoned"),
        Selected = selected.Contains("wontDo")
      });
      conditionViewModel.ItemsSource = observableCollection;
      conditionViewModel.IsAllSelected = false;
      conditionViewModel.SupportedLogic = new List<LogicType>();
      this.ViewModel = conditionViewModel;
    }

    public static string FormatDisplayText(List<string> status)
    {
      List<string> list = TaskStatusTypeEditDialog.Map.Keys.Where<string>((Func<string, bool>) (p => status.Contains(p.ToString()))).Select<string, string>((Func<string, string>) (it => TaskStatusTypeEditDialog.Map[it])).ToList<string>();
      return list.Count <= 0 ? Utils.GetString("AllStatus") : string.Join(", ", (IEnumerable<string>) list);
    }
  }
}
