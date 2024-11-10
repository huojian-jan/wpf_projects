// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.TaskListRefreshHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public static class TaskListRefreshHelper
  {
    public static async Task<List<TaskBaseViewModel>> InitData(
      ProjectIdentity identity,
      bool withChild = true)
    {
      return identity == null ? new List<TaskBaseViewModel>() : await Task.Run<List<TaskBaseViewModel>>((Func<Task<List<TaskBaseViewModel>>>) (async () => await ProjectTaskDataProvider.GetDisplayModels(identity, withChild)));
    }

    public static async Task<List<DisplayItemModel>> GetSortedDisplayItems(
      ProjectIdentity identity,
      List<TaskBaseViewModel> displayModels,
      bool showLoadMore = false,
      List<ColumnModel> columns = null,
      ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader completedTaskLoader = null)
    {
      if (identity == null)
        return new List<DisplayItemModel>();
      if (displayModels == null)
        return new List<DisplayItemModel>();
      ObservableCollection<DisplayItemModel> models = TaskListRefreshHelper.BuildDisplayItemModels(identity, (IEnumerable<TaskBaseViewModel>) displayModels);
      completedTaskLoader?.GetCompletedLimit();
      if (!(identity is ColumnProjectIdentity columnProjectIdentity))
      {
        SortOption sortOption = identity.SortOption;
      }
      else
        columnProjectIdentity.GetRealSortOption();
      ProjectIdentity identity1 = identity;
      int num = showLoadMore ? 1 : 0;
      List<ColumnModel> columns1 = columns;
      ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader closedLoader = completedTaskLoader;
      bool? showComplete = new bool?();
      return (await SortHelper.Sort(models, identity1, showLoadMore: num != 0, columns: columns1, closedLoader: closedLoader, showComplete: showComplete)).ToList<DisplayItemModel>();
    }

    private static ObservableCollection<DisplayItemModel> BuildDisplayItemModels(
      ProjectIdentity identity,
      IEnumerable<TaskBaseViewModel> models)
    {
      bool canDrag = identity.CanDrag;
      ObservableCollection<DisplayItemModel> observableCollection = new ObservableCollection<DisplayItemModel>();
      if (models != null)
      {
        foreach (TaskBaseViewModel model1 in models)
        {
          DisplayItemModel model2;
          switch (model1.Type)
          {
            case DisplayType.Event:
              model2 = new DisplayItemModel(model1);
              break;
            case DisplayType.Habit:
              model2 = new DisplayItemModel(model1);
              break;
            default:
              bool isKanbanMode = false;
              bool showProject = !(identity is NormalProjectIdentity);
              if (identity is ColumnProjectIdentity columnProjectIdentity)
              {
                isKanbanMode = true;
                showProject = !(columnProjectIdentity.Project is NormalProjectIdentity);
              }
              model2 = new DisplayItemModel(model1, showProject, isKanbanMode);
              break;
          }
          if (!canDrag || model1.IsCourse && identity.SortOption.groupBy != "dueDate")
            model2.ShowDragBar = false;
          model2.InTomorrow = identity is TomorrowProjectIdentity;
          model2.InMatrix = identity is MatrixQuadrantIdentity;
          TaskItemLoadHelper.LoadShowReminder(model2);
          observableCollection.Add(model2);
        }
      }
      return observableCollection;
    }

    public static bool CheckChanged(
      List<TaskBaseViewModel> newModels,
      List<TaskBaseViewModel> models)
    {
      if (newModels.Count != models.Count)
        return true;
      Dictionary<string, TaskBaseViewModel> dictionary = new Dictionary<string, TaskBaseViewModel>();
      foreach (TaskBaseViewModel newModel in newModels)
      {
        if (!string.IsNullOrEmpty(newModel.Id))
          dictionary[newModel.Id] = newModel;
      }
      foreach (TaskBaseViewModel model1 in models)
      {
        if (string.IsNullOrEmpty(model1.Id) || !dictionary.ContainsKey(model1.Id))
          return true;
        if (model1.IsCourse || model1.IsEvent || model1.Type == DisplayType.Habit)
        {
          TaskBaseViewModel model2 = dictionary[model1.Id];
          if (!model1.IsPropertiesEqual(model2))
            return true;
        }
      }
      return false;
    }
  }
}
