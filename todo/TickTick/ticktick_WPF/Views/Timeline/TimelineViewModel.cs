// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineViewModel : BaseViewModel
  {
    public static readonly TimelineViewModel Instance = new TimelineViewModel();
    private ClosedTaskWithFilterLoader _closedLoader;
    private readonly SemaphoreSlim _updateLineSemaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _updateArrangeSemaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _updateGroupSemaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _updateLeftSemaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _cellLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _resetAllSemaphoreSlim = new SemaphoreSlim(1, 1);
    private bool _hasGroup;
    private bool _isArranging;
    private bool _isSetting;
    private int _lineCount;
    private double _xOffset;
    private double _yearNameWidth = double.PositiveInfinity;
    private string _yearName;
    private Visibility _arrangeEmptyTaskVisibility = Visibility.Collapsed;
    private bool _isDark = ThemeKey.IsDarkTheme(LocalSettings.Settings.ThemeId);
    private Dictionary<int, bool> _groupDictModels = new Dictionary<int, bool>();
    private double _groupWidth = 140.0;
    private bool _gotoBtnEnabled = true;
    private double _viewWidth;
    private Tuple<DateTime, DateTime> _hoverStartEndTuple;
    private bool _availableReset;
    private bool _itemEditing;
    private string _uid;
    private ObservableCollection<TimelineDisplayBase> _arrangeModels = new ObservableCollection<TimelineDisplayBase>();
    private readonly HashSet<string> _foldIds = new HashSet<string>();
    private readonly BlockingSet<TimelineCellViewModel> _batchSelectedModels = new BlockingSet<TimelineCellViewModel>();
    private readonly BlockingSet<TimelineCellViewModel> _tempBatchSelectedModels = new BlockingSet<TimelineCellViewModel>();
    private double _checkedWidth;
    private double _checkedHeight;

    public TimelineViewModel()
    {
      this._uid = Utils.GetGuid();
      DataChangedNotifier.TimelineSettingsChanged += (EventHandler) ((s, e) =>
      {
        this.OnPropertyChanged(nameof (ColorType));
        this.OnPropertyChanged(nameof (ShowWeek));
        this.OnPropertyChanged(nameof (HeadHeight));
        foreach (TimelineCellViewModel timelineCellViewModel in this.CellModels.ToList())
          timelineCellViewModel.NotifyColorChanged();
      });
      this.IsDark = ThemeKey.IsDarkTheme(LocalSettings.Settings.ThemeId);
      DataChangedNotifier.SyncDone += new EventHandler<SyncResult>(this.OnSyncChanged);
      DataChangedNotifier.ProjectChanged += new EventHandler(this.OnProjectChanged);
      DataChangedNotifier.TagChanged += new EventHandler<TagModel>(this.OnTagChanged);
      CalendarEventChangeNotifier.Changed += new EventHandler<CalendarEventModel>(this.Reload);
    }

    private void Reload(object sender, CalendarEventModel e)
    {
      DelayActionHandlerCenter.TryDoAction(this._uid + "ReloadTimeline", (EventHandler) ((o, args) => ThreadUtil.DetachedRunOnUiBackThread(new Action(this.Reload))));
    }

    private async void OnTagChanged(object sender, TagModel e)
    {
      if (this.TimelineSortOption.groupBy == Constants.SortType.tag.ToString())
      {
        await this.UpdateGroupAsync();
        await this.UpdateCellLineAsync();
      }
      if (this.ColorType != TimelineColorType.Tag)
        return;
      foreach (TimelineCellViewModel timelineCellViewModel in this.CellModels.ToList())
        timelineCellViewModel.NotifyColorChanged();
    }

    private async void OnProjectChanged(object sender, EventArgs e)
    {
      if (this.ProjectIdentity == null)
        return;
      await Task.Delay(300);
      this.Reload();
    }

    private async void OnSyncChanged(object sender, SyncResult e)
    {
      if (e.ColumnChanged && this.TimelineSortOption.groupBy == Constants.SortType.sortOrder.ToString())
      {
        await this.UpdateGroupAsync();
        await this.UpdateCellLineAsync();
      }
      if (!(this.ProjectIdentity is NormalProjectIdentity))
        return;
      await this.UpdateSortArrangeAsync();
    }

    public async Task OnTasksChanged(TasksChangeEventArgs e)
    {
      TimelineViewModel parent = this;
      bool changed = false;
      bool changeLine = false;
      if (e.TagChangedIds.Any() && parent.GroupByEnum == Constants.SortType.tag)
        changed = true;
      if (e.AssignChangedIds.Any() && parent.GroupByEnum == Constants.SortType.assignee)
        changed = true;
      if (e.ProjectChangedIds.Any() && parent.GroupByEnum == Constants.SortType.project)
        changed = true;
      if ((e.PriorityChangedIds.Any() || e.KindChangedIds.Any()) && parent.GroupByEnum == Constants.SortType.priority)
        changed = true;
      if (e.BatchChangedIds.Any())
        changed = true;
      BlockingSet<string> blockingSet = e.BatchChangedIds.Copy();
      blockingSet.AddRange(e.ProjectChangedIds);
      blockingSet.AddRange(e.AddIds);
      blockingSet.AddRange(e.AssignChangedIds);
      blockingSet.AddRange(e.KindChangedIds);
      blockingSet.AddRange(e.TagChangedIds);
      blockingSet.AddRange(e.PriorityChangedIds);
      blockingSet.AddRange(e.DeletedChangedIds);
      blockingSet.AddRange(e.UndoDeletedIds);
      blockingSet.AddRange(e.DateChangedIds);
      if (parent.HideCompleted)
        blockingSet.AddRange(e.StatusChangedIds);
      List<string> list1 = blockingSet.ToList();
      DateTime startDate = parent.StartDate;
      double oneDayWidth = parent.OneDayWidth;
      List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(parent.ProjectIdentity, list1);
      // ISSUE: reference to a compiler-generated method
      matchedTasks?.RemoveAll(new Predicate<TaskBaseViewModel>(parent.\u003COnTasksChanged\u003Eb__7_0));
      foreach (string str in list1)
      {
        string id = str;
        TimelineCellViewModel model1 = parent.FirstOrDefaultCell((Func<TimelineCellViewModel, bool>) (c => c?.Id == id));
        TimeSpan timeSpan;
        if (model1 == null)
        {
          TaskBaseViewModel displayModel = matchedTasks != null ? matchedTasks.FirstOrDefault<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Id == id)) : (TaskBaseViewModel) null;
          if (displayModel != null)
          {
            TimelineCellViewModel model2 = new TimelineCellViewModel(parent, displayModel);
            parent.AddCell(model2, true);
            timeSpan = model2.StartDate.Date - startDate;
            double left = (double) timeSpan.Days * oneDayWidth;
            model2.SetLeft(left, true);
            changed = true;
          }
        }
        else
        {
          TaskBaseViewModel taskBaseViewModel = matchedTasks != null ? matchedTasks.FirstOrDefault<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Id == id)) : (TaskBaseViewModel) null;
          if (taskBaseViewModel == null)
          {
            parent.RemoveCell(model1);
            changed = true;
          }
          else
          {
            double num;
            if (taskBaseViewModel.StartDate.HasValue)
            {
              timeSpan = taskBaseViewModel.StartDate.Value.Date - startDate;
              num = (double) timeSpan.Days * oneDayWidth;
            }
            else
              num = double.NegativeInfinity;
            double left = num;
            if ((left < 0.0 || model1.Left < 0.0) && Math.Abs(left - model1.Left) > 1.0)
              changed = true;
            if (model1.SetLeft(left, true) || e.KindChangedIds.Contains(id))
              changeLine = true;
            if (e.DateChangedIds.Contains(id) || e.BatchChangedIds.Contains(id))
              parent.UpdateCellAvailable(model1);
          }
        }
      }
      if (parent.CanShowCheckItem && LocalSettings.Settings.ShowSubtasks)
      {
        HashSet<string> source = e.CheckItemChangedIds.Value;
        e.AddIds.AddRange(e.StatusChangedIds);
        e.AddIds.AddRange(e.ProjectChangedIds);
        e.AddIds.AddRange(e.TagChangedIds);
        e.AddIds.AddRange(e.AssignChangedIds);
        e.AddIds.AddRange(e.PriorityChangedIds);
        e.AddIds.AddRange(e.PriorityChangedIds);
        e.AddIds.AddRange(e.DeletedChangedIds);
        e.AddIds.AddRange(e.UndoDeletedIds);
        e.AddIds.AddRange(e.KindChangedIds);
        e.AddIds.AddRange(e.BatchChangedIds);
        List<string> list2 = e.AddIds.ToList();
        if (list2.Any<string>())
        {
          foreach (string taskId in list2)
          {
            TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
            if (taskById?.CheckItems != null && taskById.CheckItems.Count > 0)
            {
              foreach (TaskBaseViewModel taskBaseViewModel in taskById.CheckItems.Value)
                source.Add(taskBaseViewModel.Id);
            }
          }
        }
        List<TaskBaseViewModel> matchedItems = TaskViewModelHelper.GetMatchedItems(parent.ProjectIdentity, source.ToList<string>());
        if (parent.HideCompleted && matchedItems != null)
          matchedItems.RemoveAll((Predicate<TaskBaseViewModel>) (t => t.Status != 0));
        foreach (string str in source)
        {
          string id = str;
          TimelineCellViewModel model3 = parent.FirstOrDefaultCell((Func<TimelineCellViewModel, bool>) (c => c?.Id == id));
          TimeSpan timeSpan;
          if (model3 == null)
          {
            TaskBaseViewModel displayModel = matchedItems != null ? matchedItems.FirstOrDefault<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Id == id)) : (TaskBaseViewModel) null;
            if (displayModel != null)
            {
              TimelineCellViewModel model4 = new TimelineCellViewModel(parent, displayModel);
              parent.AddCell(model4, true);
              timeSpan = model4.StartDate.Date - startDate;
              double left = (double) timeSpan.Days * oneDayWidth;
              model4.SetLeft(left, true);
              changed = true;
            }
          }
          else if ((matchedItems != null ? (matchedItems.All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Id != id)) ? 1 : 0) : 1) != 0)
          {
            parent.RemoveCell(model3);
            changed = true;
          }
          else
          {
            timeSpan = model3.StartDate.Date - startDate;
            double left = (double) timeSpan.Days * oneDayWidth;
            if (model3.SetLeft(left, true))
              changeLine = true;
          }
        }
      }
      if (changed)
        await parent.UpdateGroupAsync();
      if (!(changeLine | changed))
        return;
      await parent.UpdateCellLineAsync();
      if (!parent.BatchSelect)
        return;
      parent.OnPropertyChanged("HoverStartEndTuples");
    }

    public void ResetGroupHideModels(bool drag = false)
    {
      Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
      foreach (TimelineGroupViewModel groupModel in (Collection<TimelineGroupViewModel>) this.GroupModels)
      {
        groupModel.IsOpen = !drag && groupModel.IsOpen;
        dictionary[groupModel.Line] = groupModel.IsOpen;
      }
      this.GroupDictModels = dictionary;
    }

    public async Task ResetAllAsync(string taskId = null)
    {
      Task task = await Application.Current.Dispatcher.InvokeAsync<Task>((Func<Task>) (async () =>
      {
        await this._resetAllSemaphoreSlim.WaitAsync();
        try
        {
          while (this.Editing || this.Hovering)
            await Task.Delay(100);
          this.AvailableReset = true;
          await this.ResetAllResetModelsInternal();
          await this.SetAllModelsOptAndUpdateAvailableTask(this.StartDate, this.EndDate);
          this.AvailableReset = false;
          await this.UpdateGroupAsync();
          await this.UpdateCellLeftAsync(false);
          await this.UpdateCellLineAsync(false);
          await this.UpdateSortArrangeAsync();
          this.UpdateYearModel(true, true);
          this.AvailableModels.NotifyItemsChanged(this.AvailableModels.Value);
        }
        finally
        {
          this._resetAllSemaphoreSlim.Release();
        }
      }));
    }

    private void UpdateModelOpt(
      TimelineCellViewModel cell,
      DateTime startDate,
      DateTime endDate,
      bool ignoreEdit = false)
    {
      if (cell.DisplayModel == null)
      {
        cell.Operation = TimelineCellOperation.Hide;
      }
      else
      {
        TimelineCellOperation timelineCellOperation = TimelineCellOperation.None;
        DateTime? nullable = cell.DisplayModel.StartDate;
        if (!nullable.HasValue)
        {
          nullable = cell.DisplayModel.DueDate;
          if (!nullable.HasValue)
            goto label_5;
        }
        if (cell.DisplayModel.Deleted == 0)
        {
          DateTime startDate1 = cell.StartDate;
          nullable = cell.EndDate;
          ref DateTime? local = ref nullable;
          DateTime checkEnd = local.HasValue ? local.GetValueOrDefault().Date : cell.StartDate.AddDays(1.0);
          DateTime spanStart = startDate;
          DateTime spanEnd = endDate;
          if (DateUtils.CheckDateInSpan(startDate1, checkEnd, spanStart, spanEnd))
          {
            if (cell.Operation.Contain(TimelineCellOperation.Hide))
            {
              timelineCellOperation = TimelineCellOperation.None;
              goto label_10;
            }
            else
              goto label_10;
          }
          else
          {
            timelineCellOperation = TimelineCellOperation.Hide;
            goto label_10;
          }
        }
label_5:
        timelineCellOperation = TimelineCellOperation.Hide;
label_10:
        if (cell.Operation == TimelineCellOperation.Fold && timelineCellOperation == TimelineCellOperation.None || !ignoreEdit && !cell.Operation.IsNormalStatus() || cell.Operation == timelineCellOperation)
          return;
        cell.Operation = timelineCellOperation;
      }
    }

    private async Task SetAllModelsOptAndUpdateAvailableTask(DateTime startDate, DateTime endDate)
    {
      TimelineViewModel timelineViewModel = this;
      await timelineViewModel._cellLock.WaitAsync();
      try
      {
        foreach (TimelineCellViewModel cell in timelineViewModel.CellModels.Value)
          timelineViewModel.UpdateModelOpt(cell, startDate, endDate);
        List<TimelineCellViewModel> list = timelineViewModel.CellModels.Where((Predicate<TimelineCellViewModel>) (c => c.DisplayModel != null && c.DisplayModel.StartDate.HasValue && c.Operation != TimelineCellOperation.Hide)).ToList<TimelineCellViewModel>();
        List<TimelineCellViewModel> timelineCellViewModelList = new List<TimelineCellViewModel>();
        foreach (TimelineCellViewModel timelineCellViewModel in timelineViewModel.AvailableModels.Value)
        {
          if (!list.Contains(timelineCellViewModel))
            timelineCellViewModelList.Add(timelineCellViewModel);
        }
        if (timelineCellViewModelList.Count == timelineViewModel.AvailableModels.Count)
        {
          timelineViewModel.AvailableModels.Clear();
        }
        else
        {
          // ISSUE: reference to a compiler-generated method
          timelineCellViewModelList.ForEach(new Action<TimelineCellViewModel>(timelineViewModel.\u003CSetAllModelsOptAndUpdateAvailableTask\u003Eb__11_1));
        }
        // ISSUE: reference to a compiler-generated method
        IEnumerable<TimelineCellViewModel> models = list.Where<TimelineCellViewModel>(new Func<TimelineCellViewModel, bool>(timelineViewModel.\u003CSetAllModelsOptAndUpdateAvailableTask\u003Eb__11_2));
        timelineViewModel.AvailableModels.AddRange(models);
      }
      finally
      {
        timelineViewModel._cellLock.Release();
      }
    }

    private async Task ResetAllResetModelsInternal()
    {
      TimelineViewModel parent = this;
      await parent._cellLock.WaitAsync();
      try
      {
        List<TaskBaseViewModel> taskBaseViewModelList = await TaskListRefreshHelper.InitData(parent.ProjectIdentity, false);
        if (parent.HideCompleted)
          taskBaseViewModelList = taskBaseViewModelList.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Status == 0)).ToList<TaskBaseViewModel>();
        if (parent.CheckTasksChanged(taskBaseViewModelList))
        {
          parent.ClearCell();
          foreach (TaskBaseViewModel displayModel in taskBaseViewModelList)
            parent.AddCell(new TimelineCellViewModel(parent, displayModel), false);
        }
        else
          parent.ClearBatchSelect();
      }
      finally
      {
        parent._cellLock.Release();
      }
    }

    private bool CheckTasksChanged(List<TaskBaseViewModel> tasks)
    {
      if (tasks == null || tasks.Count != this.CellModels.Count)
        return true;
      HashSet<string> currentIds = new HashSet<string>((IEnumerable<string>) this.CellModels.ToList().Select<TimelineCellViewModel, string>((Func<TimelineCellViewModel, string>) (t => t.Id)).ToList<string>());
      return tasks.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => !currentIds.Contains(t.Id)));
    }

    public async Task OnDragColumn()
    {
      foreach (TimelineGroupViewModel groupModel in (Collection<TimelineGroupViewModel>) this.GroupModels)
        groupModel.IsOpen = false;
      this.UpdateCellLineAsync();
    }

    public async Task ReloadColumns()
    {
      await this.UpdateGroupAsync();
      await this.UpdateCellLineAsync();
    }

    public async void AddNewColumn()
    {
      TimelineViewModel parent = this;
      if (!(parent.ProjectIdentity is NormalProjectIdentity projectIdentity) || projectIdentity.Project == null)
        return;
      string projectId = projectIdentity.Project.id;
      if (!parent.GroupModels.Any<TimelineGroupViewModel>())
      {
        List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(projectId);
        if (columnsByProjectId == null || !columnsByProjectId.Any<ColumnModel>())
        {
          if (await ProjectDao.GetProjectById(projectId) != null)
          {
            int num = await ColumnDao.TryInitColumns(projectId) ? 1 : 0;
          }
        }
        await parent.UpdateGroupAsync();
      }
      List<TimelineGroupViewModel> list = parent.GroupModels.ToList<TimelineGroupViewModel>();
      if ((long) list.Count >= LimitCache.GetLimitByKey(Constants.LimitKind.KanbanNumber))
      {
        new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ExceedMaxSectionCountMessage"), MessageBoxButton.OK).ShowDialog();
      }
      else
      {
        long num = list.Count > 0 ? list[0].SortOrder - 268435456L : 0L;
        list.Insert(0, new TimelineGroupViewModel(parent)
        {
          Parent = parent,
          ProjectId = projectId,
          CatId = projectId,
          Editable = true,
          IsOpen = true,
          Editing = true,
          SortOrder = num
        });
        parent.GroupModels.Clear();
        foreach (TimelineGroupViewModel timelineGroupViewModel in list)
          parent.GroupModels.Add(timelineGroupViewModel);
        parent.ShowGroup = true;
        await parent.UpdateCellLineAsync();
        projectId = (string) null;
      }
    }

    public async void AddNewColumn(TimelineGroupViewModel columnModel, bool below)
    {
      TimelineViewModel parent = this;
      List<TimelineGroupViewModel> list = parent.GroupModels.ToList<TimelineGroupViewModel>();
      if ((long) list.Count >= LimitCache.GetLimitByKey(Constants.LimitKind.KanbanNumber))
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ExceedMaxSectionCountMessage"), MessageBoxButton.OK);
        customerDialog.Owner = (Window) App.Window;
        customerDialog.ShowDialog();
      }
      else
      {
        int num1 = list.IndexOf(columnModel);
        TimelineGroupViewModel timelineGroupViewModel1 = below ? columnModel : (num1 < list.Count - 1 ? list[num1 + 1] : (TimelineGroupViewModel) null);
        TimelineGroupViewModel timelineGroupViewModel2 = !below ? columnModel : (num1 > 0 ? list[num1 - 1] : (TimelineGroupViewModel) null);
        if (timelineGroupViewModel1 == null && timelineGroupViewModel2 == null)
          return;
        long num2 = timelineGroupViewModel1 == null ? timelineGroupViewModel2.SortOrder - 268435456L : (timelineGroupViewModel2 == null ? timelineGroupViewModel1.SortOrder + 268435456L : MathUtil.LongAvg(timelineGroupViewModel1.SortOrder, timelineGroupViewModel2.SortOrder));
        int index = num1 < 0 ? (below ? list.Count : 0) : num1 + (below ? 1 : 0);
        list.Insert(index, new TimelineGroupViewModel(parent)
        {
          Parent = parent,
          ProjectId = columnModel.ProjectId,
          CatId = columnModel.ProjectId,
          Editable = true,
          IsOpen = true,
          Editing = true,
          SortOrder = num2
        });
        parent.GroupModels.Clear();
        foreach (TimelineGroupViewModel timelineGroupViewModel3 in list)
          parent.GroupModels.Add(timelineGroupViewModel3);
        parent.UpdateCellLineAsync();
      }
    }

    public void RemoveGroup(TimelineGroupViewModel columnModel)
    {
      List<TimelineGroupViewModel> list = this.GroupModels.ToList<TimelineGroupViewModel>();
      list.Remove(columnModel);
      this.GroupModels.Clear();
      foreach (TimelineGroupViewModel timelineGroupViewModel in list)
      {
        timelineGroupViewModel.IsOpen = list.Count < 2 || timelineGroupViewModel.IsOpen;
        this.GroupModels.Add(timelineGroupViewModel);
      }
      this.ShowGroup = list.Count > 1;
      this.UpdateCellLineAsync();
    }

    public async Task UpdateCellLineAsync(bool notify = true)
    {
      await this._updateLineSemaphoreSlim.WaitAsync();
      await this._updateGroupSemaphoreSlim.WaitAsync();
      try
      {
        if (this.ProjectIdentity == null)
          return;
        this.UpdateCellLineInternal(this.ProjectIdentity, this.GroupByEnum, this.OrderByEnum, notify);
      }
      finally
      {
        this._updateLineSemaphoreSlim.Release();
        this._updateGroupSemaphoreSlim.Release();
      }
    }

    public async Task UpdateGroupAsync()
    {
      await this._updateGroupSemaphoreSlim.WaitAsync();
      await this._cellLock.WaitAsync();
      try
      {
        if (this.ProjectIdentity == null)
          return;
        await this.UpdateGroupInternal(this.ProjectIdentity, this.GroupByEnum);
      }
      finally
      {
        this._updateGroupSemaphoreSlim.Release();
        this._cellLock.Release();
      }
    }

    private async Task UpdateGroupInternal(ProjectIdentity project, Constants.SortType sortType)
    {
      List<TimelineGroupViewModel> groupModels = await this.GetGroupModels(project, sortType, (IList<TimelineCellViewModel>) this.CellModels.Value.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (t => ((DateTime?) t.DisplayModel?.StartDate).HasValue)).ToList<TimelineCellViewModel>(), true);
      switch (sortType)
      {
        case Constants.SortType.none:
          this.ShowGroup = false;
          break;
        case Constants.SortType.sortOrder:
          this.ShowGroup = groupModels.Count > 1;
          break;
        default:
          this.ShowGroup = groupModels.Count > 0;
          this.ShowGroup = groupModels.Count > 0;
          break;
      }
      ItemsSourceHelper.CopyTo<TimelineGroupViewModel>(groupModels, this.GroupModels);
    }

    private void UpdateCellLineInternal(
      ProjectIdentity project,
      Constants.SortType groupBy,
      Constants.SortType sortType,
      bool notify = true)
    {
      int num;
      switch (groupBy)
      {
        case Constants.SortType.none:
        case Constants.SortType.dueDate:
          num = sortType == Constants.SortType.sortOrder ? TimelineViewModel.SortInternal(this.AvailableModels.ToList(), 0, notify: notify) : this.SortDueDate(this.AvailableModels.ToList(), 0, notify: notify);
          break;
        case Constants.SortType.priority:
          num = this.SortPriority(this.AvailableModels.ToList(), sortType, notify);
          break;
        case Constants.SortType.assignee:
          num = this.SortAssignee(this.AvailableModels.ToList(), sortType, notify);
          break;
        case Constants.SortType.tag:
          num = this.SortTag(this.AvailableModels.ToList(), sortType, notify);
          break;
        default:
          num = !(project is NormalProjectIdentity) ? this.SortProjects(this.AvailableModels.ToList(), sortType, notify) : (this.ShowGroup ? this.SortColumns(project, this.AvailableModels.ToList(), sortType, notify) : (sortType == Constants.SortType.sortOrder ? TimelineViewModel.SortInternal(this.AvailableModels.ToList(), 0, notify: notify) : this.SortDueDate(this.AvailableModels.ToList(), 0, notify: notify)));
          break;
      }
      this.LineCount = num + 3;
      this.ResetGroupHideModels();
    }

    private async Task<List<TimelineGroupViewModel>> GetGroupModels(
      ProjectIdentity project,
      Constants.SortType sortType,
      IList<TimelineCellViewModel> children,
      bool containsNoChild)
    {
      List<TimelineGroupViewModel> models = new List<TimelineGroupViewModel>();
      string identityId = project?.CatId;
      List<SectionStatusModel> openStatus = CacheManager.GetSectionStatus().Where<SectionStatusModel>((Func<SectionStatusModel, bool>) (item => identityId == item.Identity)).ToList<SectionStatusModel>();
      string arrangeIdentity = "Arrange_" + identityId;
      List<SectionStatusModel> arrangeOpenStatus = CacheManager.GetSectionStatus().Where<SectionStatusModel>((Func<SectionStatusModel, bool>) (item => arrangeIdentity == item.Identity)).ToList<SectionStatusModel>();
      switch (sortType)
      {
        case Constants.SortType.none:
        case Constants.SortType.dueDate:
          return models;
        case Constants.SortType.sortOrder:
          await GetColumns();
          goto case Constants.SortType.none;
        case Constants.SortType.project:
          GetProjectSections();
          goto case Constants.SortType.none;
        case Constants.SortType.priority:
          GetPrioritySection();
          goto case Constants.SortType.none;
        case Constants.SortType.assignee:
          GetAssigneeSection();
          goto case Constants.SortType.none;
        case Constants.SortType.tag:
          GetTagSections();
          goto case Constants.SortType.none;
        default:
          if (this.ProjectIdentity is NormalProjectIdentity)
          {
            await GetColumns();
            goto case Constants.SortType.none;
          }
          else
          {
            GetProjectSections();
            goto case Constants.SortType.none;
          }
      }

      async Task GetColumns()
      {
        string projectId = this.ProjectIdentity?.GetProjectId();
        if (children.Any<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => string.IsNullOrEmpty(c.DisplayModel.ColumnId))))
          await TaskService.CheckColumnEmptyTask(projectId);
        List<ColumnModel> columns = await ColumnDao.GetColumnsByProjectId(projectId);
        columns?.ForEach((Action<ColumnModel>) (c =>
        {
          List<TimelineGroupViewModel> timelineGroupViewModelList = models;
          TimelineGroupViewModel timelineGroupViewModel = new TimelineGroupViewModel(this);
          timelineGroupViewModel.Title = c.name;
          timelineGroupViewModel.Id = c.id;
          timelineGroupViewModel.Parent = this;
          timelineGroupViewModel.IsOpen = columns.Count <= 1 || openStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != c.id));
          timelineGroupViewModel.IsArrangeOpen = arrangeOpenStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != c.id));
          timelineGroupViewModel.ProjectId = projectId;
          timelineGroupViewModel.CatId = projectId;
          timelineGroupViewModel.SortOrder = c.sortOrder.GetValueOrDefault();
          ProjectModel projectById = CacheManager.GetProjectById(projectId);
          timelineGroupViewModel.Editable = projectById != null && projectById.IsEnable();
          timelineGroupViewModelList.Add(timelineGroupViewModel);
        }));
        if (containsNoChild)
          ;
        else
        {
          List<TimelineGroupViewModel> alone = new List<TimelineGroupViewModel>();
          models.Where<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (m => children.All<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => c.DisplayModel.ColumnId != m.Id)))).ToList<TimelineGroupViewModel>().ForEach((Action<TimelineGroupViewModel>) (m => alone.Add(m)));
          alone.ForEach((Action<TimelineGroupViewModel>) (a => models.Remove(a)));
        }
      }

      void GetProjectSections()
      {
        List<ProjectModel> list;
        if (this.ProjectIdentity is GroupProjectIdentity & containsNoChild)
        {
          list = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.groupId == this.ProjectIdentity.CatId)).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).ToList<ProjectModel>();
        }
        else
        {
          HashSet<string> projectIds = new HashSet<string>(children.Select<TimelineCellViewModel, string>((Func<TimelineCellViewModel, string>) (c => c.DisplayModel.IsEvent ? "Calendar5959a2259161d16d23a4f272" : c.DisplayModel.ProjectId)));
          list = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => projectIds.Contains(p.id))).ToList<ProjectModel>();
          list.Sort((Comparison<ProjectModel>) ((a, b) => a.CompareTo(b)));
          if (projectIds.Contains("Calendar5959a2259161d16d23a4f272"))
            list.Add(new ProjectModel()
            {
              id = "Calendar5959a2259161d16d23a4f272",
              name = Utils.GetString("Calendar")
            });
        }
        foreach (ProjectModel projectModel in list)
        {
          ProjectModel p = projectModel;
          List<TimelineGroupViewModel> timelineGroupViewModelList = models;
          TimelineGroupViewModel timelineGroupViewModel = new TimelineGroupViewModel(this);
          timelineGroupViewModel.Parent = this;
          timelineGroupViewModel.Id = p.id;
          timelineGroupViewModel.Title = p.name;
          timelineGroupViewModel.ProjectId = p.id;
          timelineGroupViewModel.CatId = this.ProjectIdentity.CatId;
          timelineGroupViewModel.IsOpen = openStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != p.id));
          timelineGroupViewModel.IsArrangeOpen = arrangeOpenStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != p.id));
          timelineGroupViewModelList.Add(timelineGroupViewModel);
        }
      }

      void GetPrioritySection()
      {
        if (children.Any<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => !c.DisplayModel.IsNote && c.DisplayModel.Priority == 5)))
          AddGroup("5", Utils.GetString("PriorityHigh"));
        if (children.Any<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => !c.DisplayModel.IsNote && c.DisplayModel.Priority == 3)))
          AddGroup("3", Utils.GetString("PriorityMedium"));
        if (children.Any<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => !c.DisplayModel.IsNote && c.DisplayModel.Priority == 1)))
          AddGroup("1", Utils.GetString("PriorityLow"));
        if (children.Any<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => !c.DisplayModel.IsNote && c.DisplayModel.Priority == 0)))
          AddGroup("0", Utils.GetString("PriorityNull"));
        if (!children.Any<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => c.DisplayModel.IsNote)))
          return;
        AddGroup("note", Utils.GetString("Notes"));
      }

      void AddGroup(string id, string title)
      {
        List<TimelineGroupViewModel> timelineGroupViewModelList = models;
        TimelineGroupViewModel timelineGroupViewModel = new TimelineGroupViewModel(this);
        timelineGroupViewModel.Parent = this;
        timelineGroupViewModel.Id = id;
        timelineGroupViewModel.Title = title;
        timelineGroupViewModel.ProjectId = this.ProjectIdentity?.GetProjectId();
        timelineGroupViewModel.CatId = this.ProjectIdentity?.CatId;
        timelineGroupViewModel.IsOpen = openStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != title));
        timelineGroupViewModel.IsArrangeOpen = arrangeOpenStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != title));
        timelineGroupViewModelList.Add(timelineGroupViewModel);
      }

      void GetTagSections()
      {
        List<TagModel> tags = TagDataHelper.GetTags();
        tags.Sort((Comparison<TagModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
        List<TagModel> tagModelList = new List<TagModel>();
        HashSet<string> stringSet = new HashSet<string>();
        foreach (TimelineCellViewModel child in (IEnumerable<TimelineCellViewModel>) children)
        {
          string str = Utils.GetString("NoTags");
          if (string.IsNullOrEmpty(child.DisplayModel.Tag) || child.DisplayModel.Tag == "[]")
          {
            if (!stringSet.Contains("!tag"))
            {
              TagModel tagModel = new TagModel()
              {
                name = "!tag",
                label = str,
                sortOrder = 9223372036854775806
              };
              tagModelList.Add(tagModel);
              stringSet.Add(tagModel.name);
            }
          }
          else
          {
            List<string> taskTags = TagSerializer.ToTags(child.DisplayModel.Tag.ToLower());
            TagModel tagModel = tags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => taskTags.Contains(t.name)));
            if (tagModel != null && tagModel.name != null && !stringSet.Contains(tagModel.name))
            {
              tagModelList.Add(tagModel);
              stringSet.Add(tagModel.name);
            }
          }
        }
        tagModelList.Sort((Comparison<TagModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
        foreach (TagModel tagModel in tagModelList)
        {
          TagModel tag = tagModel;
          List<TimelineGroupViewModel> timelineGroupViewModelList = models;
          TimelineGroupViewModel timelineGroupViewModel = new TimelineGroupViewModel(this);
          timelineGroupViewModel.Parent = this;
          timelineGroupViewModel.Id = tag.name;
          timelineGroupViewModel.Title = tag.GetDisplayName();
          timelineGroupViewModel.ProjectId = this.ProjectIdentity?.GetProjectId();
          timelineGroupViewModel.CatId = this.ProjectIdentity?.CatId;
          timelineGroupViewModel.IsOpen = openStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != tag.name));
          timelineGroupViewModel.IsArrangeOpen = arrangeOpenStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != tag.name));
          timelineGroupViewModelList.Add(timelineGroupViewModel);
        }
      }

      void GetAssigneeSection()
      {
        List<AvatarViewModel> source = !(this.ProjectIdentity is GroupProjectIdentity projectIdentity) ? AvatarHelper.GetProjectAvatarsFromCache(this.ProjectIdentity?.CatId, true) : AvatarHelper.GetProjectAvatarsFromCacheInGroup(projectIdentity.Id);
        Dictionary<string, TimelineGroupViewModel> dictionary1 = new Dictionary<string, TimelineGroupViewModel>();
        foreach (TimelineCellViewModel child in (IEnumerable<TimelineCellViewModel>) children)
        {
          string userId = child.DisplayModel.GetAssignee();
          if (userId != "-1" && !dictionary1.ContainsKey(userId))
          {
            AvatarViewModel user = source.FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (u => (u.UserId ?? "") == userId));
            if (user != null)
            {
              Dictionary<string, TimelineGroupViewModel> dictionary2 = dictionary1;
              string key = userId;
              TimelineGroupViewModel timelineGroupViewModel = new TimelineGroupViewModel(this);
              timelineGroupViewModel.Title = user.Name;
              timelineGroupViewModel.Id = user.UserId;
              timelineGroupViewModel.ProjectId = this.ProjectIdentity?.GetProjectId();
              timelineGroupViewModel.CatId = this.ProjectIdentity?.CatId;
              timelineGroupViewModel.Parent = this;
              timelineGroupViewModel.IsOpen = true;
              timelineGroupViewModel.IsArrangeOpen = arrangeOpenStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != user.UserId));
              timelineGroupViewModel.AvatarUrl = user.AvatarUrl;
              timelineGroupViewModel.IsAvatar = true;
              timelineGroupViewModel.ShowArrow = Visibility.Collapsed;
              timelineGroupViewModel.SortOrder = user.UserId == LocalSettings.Settings.LoginUserId ? long.MinValue : (long) source.IndexOf(user);
              dictionary2.Add(key, timelineGroupViewModel);
            }
          }
        }
        models.AddRange((IEnumerable<TimelineGroupViewModel>) dictionary1.Values.ToList<TimelineGroupViewModel>().OrderBy<TimelineGroupViewModel, long>((Func<TimelineGroupViewModel, long>) (m => m.SortOrder)));
        string str = Utils.GetString("NotAssigned");
        List<TimelineGroupViewModel> timelineGroupViewModelList = models;
        TimelineGroupViewModel timelineGroupViewModel1 = new TimelineGroupViewModel(this);
        timelineGroupViewModel1.Title = str;
        timelineGroupViewModel1.Id = "-1";
        timelineGroupViewModel1.ProjectId = this.ProjectIdentity?.GetProjectId();
        timelineGroupViewModel1.CatId = this.ProjectIdentity?.CatId;
        timelineGroupViewModel1.Parent = this;
        timelineGroupViewModel1.IsAvatar = true;
        timelineGroupViewModel1.IsOpen = true;
        timelineGroupViewModel1.IsArrangeOpen = arrangeOpenStatus.All<SectionStatusModel>((Func<SectionStatusModel, bool>) (s => s.Name != "-1"));
        timelineGroupViewModel1.ShowArrow = Visibility.Collapsed;
        timelineGroupViewModelList.Add(timelineGroupViewModel1);
      }
    }

    public async Task MoveSpan(DateTime startDate, DateTime endDate, bool updateModels = true)
    {
      TimelineViewModel timelineViewModel = this;
      if (Utils.IsEmptyDate(startDate) || Utils.IsEmptyDate(startDate))
        return;
      startDate = startDate.Date;
      endDate = endDate.Date;
      bool changed = false;
      startDate = startDate.AddDays((double) ((int) (startDate.DayOfWeek - 1) * -1));
      endDate = endDate.AddDays((double) ((int) endDate.DayOfWeek * -1)).AddDays(7.0);
      if (timelineViewModel.StartDate != startDate || timelineViewModel.EndDate != endDate)
      {
        timelineViewModel.StartEndTuple = new Tuple<DateTime, DateTime>(startDate, endDate);
        timelineViewModel.OnPropertyChanged("StartEndTuple");
        timelineViewModel.OnPropertyChanged("Width");
      }
      if (timelineViewModel.StartDate != startDate)
      {
        timelineViewModel.StartDate = startDate;
        changed = true;
        timelineViewModel.OnPropertyChanged("StartDate");
        if (updateModels)
          await timelineViewModel.UpdateCellLeftAsync();
      }
      if (timelineViewModel.EndDate != endDate)
      {
        timelineViewModel.EndDate = endDate;
        changed = true;
        timelineViewModel.OnPropertyChanged("EndDate");
      }
      if (!changed || timelineViewModel.ProjectIdentity == null)
        return;
      await timelineViewModel.SetAllModelsOptAndUpdateAvailableTask(startDate, endDate);
      foreach (TimelineCellViewModel timelineCellViewModel in timelineViewModel.AvailableModels.ToList())
        timelineCellViewModel.SetWidth();
      if (updateModels)
      {
        await timelineViewModel.UpdateGroupAsync();
        await timelineViewModel.UpdateCellLineAsync();
      }
      timelineViewModel.UpdateYearModel(true, true);
      timelineViewModel.Fetch();
    }

    private void UpdateYearModel(bool updateModels, bool updateLeft)
    {
      DateTime startDate = this.StartDate;
      DateTime endDate = this.EndDate;
      double oneDayWidth = this.OneDayWidth;
      string timelineRange = this.TimelineRange;
      double xoffset = this.XOffset;
      int year = DateTime.Now.Year;
      lock (this.YearModels)
      {
        List<TimelineYearViewModel> list = this.YearModels.ToList<TimelineYearViewModel>();
        if (updateModels)
        {
          list.Clear();
          if (timelineRange == "year")
          {
            for (DateTime dateTime = new DateTime(startDate.Year, 1, 1); dateTime.Year < endDate.Year; dateTime = dateTime.AddYears(1))
              list.Add(new TimelineYearViewModel()
              {
                Left = (double) (dateTime - startDate).Days * oneDayWidth,
                YearName = dateTime.Year.ToString()
              });
          }
          else
          {
            for (DateTime date = new DateTime(startDate.Year, startDate.Month, 1); date < endDate; date = date.AddMonths(1))
              list.Add(new TimelineYearViewModel()
              {
                Left = (double) (date - startDate).Days * oneDayWidth,
                YearName = date.Year == year ? DateUtils.FormatMonth(date) : date.ToString("Y")
              });
          }
          ItemsSourceHelper.CopyTo<TimelineYearViewModel>(list, this.YearModels);
        }
        if (!updateLeft)
          return;
        double yearNameXOffset = xoffset + 10.0;
        TimelineYearViewModel timelineYearViewModel1 = list.LastOrDefault<TimelineYearViewModel>((Func<TimelineYearViewModel, bool>) (i => i.Left < yearNameXOffset));
        if (timelineYearViewModel1 == null)
          return;
        foreach (TimelineYearViewModel timelineYearViewModel2 in list)
          timelineYearViewModel2.Visibility = Visibility.Visible;
        int num1 = list.IndexOf(timelineYearViewModel1);
        if (num1 != -1 && num1 < list.Count - 1)
        {
          double num2 = list[num1 + 1].Left - yearNameXOffset;
          this.YearNameWidth = num2 > 0.0 ? num2 : double.NaN;
        }
        else
          this.YearNameWidth = double.NaN;
        timelineYearViewModel1.Visibility = Visibility.Collapsed;
        this.YearName = timelineYearViewModel1.YearName;
      }
    }

    private void Fetch()
    {
      if (this.ProjectIdentity == null)
        return;
      this._closedLoader?.LoadTasksInSpan(this.StartDate, this.EndDate < DateTime.Now ? this.EndDate : DateTime.Now, this.ProjectIdentity is NormalProjectIdentity projectIdentity ? projectIdentity.Id : "all", true);
    }

    public async Task UpdateCellLeftAsync(bool notify = true)
    {
      await this._updateLeftSemaphoreSlim.WaitAsync();
      await this._cellLock.WaitAsync();
      try
      {
        this.UpdateCellModelsLeftInternal((IEnumerable<TimelineCellViewModel>) this.CellModels.Value, notify);
      }
      finally
      {
        this._updateLeftSemaphoreSlim.Release();
        this._cellLock.Release();
      }
    }

    private void UpdateCellModelsLeftInternal(
      IEnumerable<TimelineCellViewModel> cellModels,
      bool notify)
    {
      DateTime startDate = this.StartDate;
      double oneDayWidth = this.OneDayWidth;
      foreach (TimelineCellViewModel cellModel in cellModels)
        cellModel.SetLeft((double) (cellModel.StartDate.Date - startDate).Days * oneDayWidth, notify);
    }

    private TimelineModel GetTimelineModel() => this.ProjectIdentity?.GetTimelineModel();

    public void ClearCell()
    {
      this._batchSelectedModels.Clear();
      this.OnPropertyChanged("HoverStartEndTuples");
      this.CellModels.Clear();
      this.AvailableModels.Clear();
      this.ClearArrangeModels();
    }

    public async Task RemoveCell(TimelineCellViewModel model, bool setPos = false)
    {
      this.CellModels.Remove(model);
      this.AvailableModels.Remove(model);
      if (model.BatchSelected)
        this.ChangeBatchSelectStatus(model);
      await this.UpdateCellToArrange(model, true);
      if (!setPos)
        return;
      await this.UpdateGroupAsync();
      await this.UpdateCellLineAsync();
    }

    public void AddCell(TimelineCellViewModel model, bool checkAva)
    {
      this.CellModels.Add(model);
      if (!checkAva)
        return;
      this.UpdateModelOpt(model, this.StartDate, this.EndDate);
      if (model.Operation != TimelineCellOperation.Hide && this.AvailableModels.All((Func<TimelineCellViewModel, bool>) (m => m.Id != model.Id)))
        this.AvailableModels.Add(model);
      this.UpdateCellToArrange(model);
    }

    public void UpdateCellTime(TimelineCellViewModel model, bool ignoreEdit = false, bool forceCheck = false)
    {
      if (this.ProjectIdentity is FilterProjectIdentity | forceCheck)
      {
        DisplayType? type = model.DisplayModel?.Type;
        if (type.HasValue)
        {
          switch (type.GetValueOrDefault())
          {
            case DisplayType.Task:
            case DisplayType.Note:
              List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, new List<string>()
              {
                model.Id
              });
              matchedTasks?.RemoveAll((Predicate<TaskBaseViewModel>) (t => this.HideCompleted && t.Status != 0));
              if (matchedTasks == null || matchedTasks.Count == 0)
              {
                this.RemoveCell(model, true);
                return;
              }
              break;
            case DisplayType.CheckItem:
              List<TaskBaseViewModel> matchedItems = TaskViewModelHelper.GetMatchedItems(this.ProjectIdentity, new List<string>()
              {
                model.Id
              });
              matchedItems?.RemoveAll((Predicate<TaskBaseViewModel>) (t => this.HideCompleted && t.Status != 0));
              if (matchedItems == null || matchedItems.Count == 0)
              {
                this.RemoveCell(model, true);
                return;
              }
              break;
          }
        }
      }
      this.UpdateCellAvailable(model, ignoreEdit);
    }

    private void UpdateCellAvailable(TimelineCellViewModel model, bool ignoreEdit = false)
    {
      this.UpdateModelOpt(model, this.StartDate, this.EndDate, ignoreEdit);
      this.UpdateCellToArrange(model);
      if (model.Operation == TimelineCellOperation.Hide || Utils.IsEmptyDate(model.StartDate))
      {
        this.AvailableModels.Remove(model);
      }
      else
      {
        if (this.AvailableModels.All((Func<TimelineCellViewModel, bool>) (m => m.Id != model.Id)))
          this.AvailableModels.Add(model);
        model.SetLeft();
      }
    }

    public bool ContainsCell(TimelineCellViewModel model) => this.CellModels.Contains(model);

    public bool AnyCell(Func<TimelineCellViewModel, bool> predicate)
    {
      return this.CellModels.Value.Any<TimelineCellViewModel>(predicate);
    }

    public TimelineCellViewModel FirstOrDefaultCell(Func<TimelineCellViewModel, bool> predicate)
    {
      return this.CellModels.FirstOrDefault(predicate);
    }

    public IEnumerable<TimelineCellViewModel> WhereCell(Func<TimelineCellViewModel, bool> predicate)
    {
      return this.CellModels.Value.Where<TimelineCellViewModel>(predicate);
    }

    public DateTime? GetRentCellDate()
    {
      DateTime? rentCellDate1 = new DateTime?();
      foreach (TimelineCellViewModel timelineCellViewModel in this.CellModels.Value)
      {
        DateTime? rentCellDate2 = timelineCellViewModel.DisplayModel.StartDate;
        if (rentCellDate2.HasValue)
        {
          rentCellDate2 = timelineCellViewModel.DisplayModel.StartDate;
          DateTime dateTime = rentCellDate2.Value;
          rentCellDate2 = timelineCellViewModel.DisplayModel.DueDate;
          if (rentCellDate2.HasValue)
          {
            rentCellDate2 = timelineCellViewModel.DisplayModel.DueDate;
            DateTime today1 = DateTime.Today;
            if ((rentCellDate2.HasValue ? (rentCellDate2.GetValueOrDefault() > today1 ? 1 : 0) : 0) != 0)
            {
              rentCellDate2 = timelineCellViewModel.DisplayModel.StartDate;
              DateTime today2 = DateTime.Today;
              if ((rentCellDate2.HasValue ? (rentCellDate2.GetValueOrDefault() <= today2 ? 1 : 0) : 0) != 0)
              {
                rentCellDate2 = new DateTime?(DateTime.Today);
                return rentCellDate2;
              }
            }
            DateTime? dueDate = timelineCellViewModel.DisplayModel.DueDate;
            DateTime today3 = DateTime.Today;
            if ((dueDate.HasValue ? (dueDate.GetValueOrDefault() < today3 ? 1 : 0) : 0) != 0)
            {
              dueDate = timelineCellViewModel.DisplayModel.DueDate;
              dateTime = dueDate.Value;
            }
          }
          if (!rentCellDate1.HasValue)
          {
            rentCellDate1 = new DateTime?(dateTime.Date);
          }
          else
          {
            TimeSpan timeSpan = DateTime.Today - rentCellDate1.Value;
            double num1 = Math.Abs(timeSpan.TotalDays);
            timeSpan = DateTime.Today - dateTime.Date;
            double num2 = Math.Abs(timeSpan.TotalDays);
            if (num1 > num2 || Math.Abs(num1 - num2) < 0.1 && dateTime.Date > rentCellDate1.Value.Date)
              rentCellDate1 = new DateTime?(dateTime.Date);
          }
        }
      }
      return rentCellDate1;
    }

    public ObservableCollection<TimelineYearViewModel> YearModels { get; } = new ObservableCollection<TimelineYearViewModel>();

    public BlockingList<TimelineCellViewModel> CellModels { get; } = new BlockingList<TimelineCellViewModel>();

    public TimeLineItemList AvailableModels { get; } = new TimeLineItemList();

    public ObservableCollection<TimelineGroupViewModel> GroupModels { get; } = new ObservableCollection<TimelineGroupViewModel>();

    public Dictionary<int, bool> GroupDictModels
    {
      get => this._groupDictModels;
      set
      {
        this._groupDictModels = value;
        this.OnPropertyChanged(nameof (GroupDictModels));
      }
    }

    public double GroupWidthMin => 100.0;

    public double GroupWidthMax { get; set; }

    public double ViewWidth
    {
      get => this._viewWidth;
      set
      {
        this.ChangeAndNotify<double>(ref this._viewWidth, value, nameof (ViewWidth));
        if (this._viewWidth <= 100.0)
          return;
        this.GroupWidthMax = this._viewWidth / 3.0;
        this.OnPropertyChanged("GroupWidthMax");
        if (this.GroupWidth <= this.GroupWidthMax)
          return;
        this.GroupWidth = this.GroupWidthMax;
      }
    }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public Tuple<DateTime, DateTime> StartEndTuple { get; private set; } = new Tuple<DateTime, DateTime>(new DateTime(), new DateTime());

    public Tuple<DateTime, DateTime> HoverStartEndTuple
    {
      get => this._hoverStartEndTuple;
      set
      {
        this._hoverStartEndTuple = value;
        this.OnPropertyChanged("HoverStartEndTuples");
      }
    }

    public List<Tuple<DateTime, DateTime>> HoverStartEndTuples
    {
      get
      {
        if (this._batchSelectedModels.Count == 0)
        {
          if (this._hoverStartEndTuple == null)
            return (List<Tuple<DateTime, DateTime>>) null;
          return new List<Tuple<DateTime, DateTime>>()
          {
            this._hoverStartEndTuple
          };
        }
        HashSet<DateTime> source = new HashSet<DateTime>();
        foreach (TimelineCellViewModel timelineCellViewModel in this._batchSelectedModels.ToList())
        {
          HashSet<DateTime> dateTimeSet1 = source;
          DateTime dateTime = timelineCellViewModel.StartDate;
          DateTime date1 = dateTime.Date;
          dateTimeSet1.Add(date1);
          DateTime? endDate = timelineCellViewModel.EndDate;
          if (endDate.HasValue)
          {
            HashSet<DateTime> dateTimeSet2 = source;
            DateTime date2;
            if (!timelineCellViewModel.IsAllDay)
            {
              endDate = timelineCellViewModel.EndDate;
              dateTime = endDate.Value;
              date2 = dateTime.Date;
            }
            else
            {
              endDate = timelineCellViewModel.EndDate;
              dateTime = endDate.Value;
              dateTime = dateTime.AddSeconds(-1.0);
              date2 = dateTime.Date;
            }
            dateTimeSet2.Add(date2);
          }
        }
        return new List<Tuple<DateTime, DateTime>>()
        {
          new Tuple<DateTime, DateTime>(source.Min<DateTime>(), source.Max<DateTime>())
        };
      }
    }

    public ProjectIdentity ProjectIdentity { get; private set; }

    public bool ProjectEnable
    {
      get
      {
        return !(this.ProjectIdentity is NormalProjectIdentity projectIdentity) || projectIdentity.Project.IsEnable();
      }
    }

    public async Task SetProjectIdentity(ProjectIdentity value)
    {
      TimelineViewModel timelineViewModel = this;
      int num;
      if (!(timelineViewModel.TimelineRange != value?.GetTimelineModel()?.Range))
      {
        int timelineRangeIndex = timelineViewModel.TimelineRangeIndex;
        int? dayWidthIndex = value?.GetTimelineModel()?.DayWidthIndex;
        int valueOrDefault = dayWidthIndex.GetValueOrDefault();
        num = !(timelineRangeIndex == valueOrDefault & dayWidthIndex.HasValue) ? 1 : 0;
      }
      else
        num = 1;
      timelineViewModel.ProjectIdentity = value;
      timelineViewModel._closedLoader = new ClosedTaskWithFilterLoader();
      if (num != 0)
      {
        timelineViewModel.OnTimelineRangeChanged();
        await Task.Delay(50);
      }
      await timelineViewModel.ResetAllAsync();
      timelineViewModel.SetProjectTitle(timelineViewModel.ProjectIdentity?.GetDisplayTitle());
      timelineViewModel.OnPropertyChanged("ShowShareGrid");
      timelineViewModel.OnPropertyChanged("ShowShareIcon");
      timelineViewModel.OnPropertyChanged("CanShowActivity");
      timelineViewModel.OnPropertyChanged("CanShowCheckItem");
      timelineViewModel.OnPropertyChanged("TimelinePermission");
      timelineViewModel.OnPropertyChanged("TimelineSortOption");
      timelineViewModel.OnPropertyChanged("ProjectEnable");
    }

    public string ProjectTitle { get; private set; }

    public void SetProjectTitle(string title)
    {
      this.ProjectTitle = title;
      this.OnPropertyChanged("ProjectTitle");
    }

    public TimelineColorType ColorType
    {
      get
      {
        TimelineColorType result;
        return Enum.TryParse<TimelineColorType>(LocalSettings.Settings.TimelineSettingsModel?.Color ?? string.Empty, true, out result) ? result : TimelineColorType.List;
      }
      set
      {
        string color = LocalSettings.Settings.TimelineSettingsModel.Color;
        string name = value.GetName();
        string str = name;
        if (!(color != str))
          return;
        LocalSettings.Settings.TimelineSettingsModel.Color = name;
        LocalSettings.Settings.TimelineSettingsModel.mtime = Utils.GetNowTimeStampInMills();
        LocalSettings.Settings.Save();
        SettingsHelper.PushLocalPreference();
        ticktick_WPF.Notifier.GlobalEventManager.NotifyTimelineSetChanged("Color");
      }
    }

    public double HeadHeight => !this.ShowWeek ? 28.0 : 42.0;

    public bool ShowWeek
    {
      get
      {
        TimelineSettingsModel timelineSettingsModel = LocalSettings.Settings.TimelineSettingsModel;
        return timelineSettingsModel != null && timelineSettingsModel.ShowWeek;
      }
      set
      {
        if (LocalSettings.Settings.UserPreference == null)
          return;
        bool? showWeek = LocalSettings.Settings.TimelineSettingsModel?.ShowWeek;
        bool flag = value;
        if (showWeek.GetValueOrDefault() == flag & showWeek.HasValue)
          return;
        if (LocalSettings.Settings.UserPreference.Timeline == null)
          LocalSettings.Settings.UserPreference.Timeline = new TimelineSettingsModel();
        LocalSettings.Settings.UserPreference.Timeline.ShowWeek = value;
        LocalSettings.Settings.UserPreference.Timeline.mtime = Utils.GetNowTimeStampInMills();
        LocalSettings.Settings.Save();
        SettingsHelper.PushLocalPreference();
        ticktick_WPF.Notifier.GlobalEventManager.NotifyTimelineSetChanged(nameof (ShowWeek));
      }
    }

    public double GroupWidth
    {
      get => this._groupWidth;
      set
      {
        if (this.GroupWidthMin > value || value > this.GroupWidthMax)
          return;
        this._groupWidth = value;
        this.OnPropertyChanged(nameof (GroupWidth));
      }
    }

    public bool ShowGroup
    {
      get => this._hasGroup;
      set
      {
        this._hasGroup = value;
        this.OnPropertyChanged(nameof (ShowGroup));
      }
    }

    public bool IsArranging
    {
      get => this._isArranging;
      set
      {
        this._isArranging = value;
        if (this._isArranging)
          this.SetArrangeModels();
        this.OnPropertyChanged(nameof (IsArranging));
      }
    }

    public bool IsSetting
    {
      get => this._isSetting;
      set
      {
        this._isSetting = value;
        this.OnPropertyChanged(nameof (IsSetting));
      }
    }

    public int TimelineRangeIndex
    {
      get
      {
        TimelineModel timelineModel = this.GetTimelineModel();
        if (timelineModel == null || timelineModel.DayWidthIndex <= 0)
          return TimelineConstants.GetRangeDefaultWidthIndex(timelineModel?.Range ?? TimelineConstants.RangeDefault);
        return TimelineConstants.GetDayWidthRange(timelineModel.DayWidthIndex) != timelineModel.Range ? TimelineConstants.GetRangeDefaultWidthIndex(timelineModel.Range) : timelineModel.DayWidthIndex;
      }
    }

    public async Task SetRangIndex(int index)
    {
      TimelineModel timelineModel = this.GetTimelineModel();
      if (timelineModel == null || timelineModel.DayWidthIndex == index)
        return;
      timelineModel.DayWidthIndex = index;
      if (TimelineConstants.GetDayWidthRange(index) != timelineModel.Range)
        timelineModel.Range = TimelineConstants.GetDayWidthRange(index);
      this.ProjectIdentity?.CommitTimeline(timelineModel);
      this.OnTimelineRangeChanged();
      this.UpdateYearModel(true, true);
      await this.UpdateCellLeftAsync();
      foreach (TimelineCellViewModel timelineCellViewModel in this.AvailableModels.ToList())
        timelineCellViewModel.SetWidth();
    }

    public string TimelineRange
    {
      get => this.GetTimelineModel()?.Range ?? TimelineConstants.RangeDefault;
      set
      {
        int defaultWidthIndex = TimelineConstants.GetRangeDefaultWidthIndex(value);
        TimelineModel timelineModel = this.GetTimelineModel();
        if (timelineModel != null && (timelineModel.Range != value || timelineModel.DayWidthIndex != defaultWidthIndex))
        {
          timelineModel.Range = value;
          timelineModel.DayWidthIndex = defaultWidthIndex;
          this.ProjectIdentity?.CommitTimeline(timelineModel);
          foreach (TimelineCellViewModel timelineCellViewModel in this.AvailableModels.ToList())
            timelineCellViewModel.SetWidth();
        }
        this.OnTimelineRangeChanged();
        this.UpdateYearModel(true, true);
        this.UpdateCellLeftAsync();
      }
    }

    private void OnTimelineRangeChanged()
    {
      this.OnPropertyChanged("TimelineRange");
      this.OnPropertyChanged("TimelineRangeIndex");
      this.OnPropertyChanged("OneDayWidth");
      this.OnPropertyChanged("Width");
      this.OnPropertyChanged("NewTaskDefaultDays");
      this.OnPropertyChanged("MinChangeDays");
      this.OnPropertyChanged("NeedsSplitLine");
    }

    public Constants.SortType OrderByEnum
    {
      get
      {
        Constants.SortType result;
        if (!Enum.TryParse<Constants.SortType>(this.TimelineSortOption.orderBy, out result))
          result = Constants.SortType.sortOrder;
        return result;
      }
    }

    public Constants.SortType GroupByEnum
    {
      get
      {
        Constants.SortType result;
        if (!Enum.TryParse<Constants.SortType>(this.TimelineSortOption.groupBy, out result))
          result = Constants.SortType.sortOrder;
        return result;
      }
    }

    public bool ShowShareGrid
    {
      get
      {
        if (!(this.ProjectIdentity is NormalProjectIdentity projectIdentity))
          return false;
        ProjectModel project = projectIdentity.Project;
        return project != null && project.IsShareList();
      }
    }

    public bool ShowShareIcon
    {
      get
      {
        if (!(this.ProjectIdentity is NormalProjectIdentity projectIdentity))
          return false;
        ProjectModel project = projectIdentity.Project;
        return (project != null ? (project.IsShareList() ? 1 : 0) : 0) == 0;
      }
    }

    public bool CanShowActivity
    {
      get
      {
        if (!(this.ProjectIdentity is NormalProjectIdentity projectIdentity))
          return false;
        ProjectModel project = projectIdentity.Project;
        return project != null && project.IsEnable();
      }
    }

    public bool CanShowCheckItem
    {
      get
      {
        return this.ProjectIdentity is FilterProjectIdentity projectIdentity && projectIdentity.Filter.ContainsDate();
      }
    }

    public string TimelinePermission
    {
      get
      {
        return this.ProjectIdentity is NormalProjectIdentity projectIdentity && projectIdentity.Project != null ? projectIdentity.Project.permission : string.Empty;
      }
    }

    public SortOption TimelineSortOption
    {
      get
      {
        SortOption sortOption = this.GetTimelineModel()?.GetSortOption();
        if (sortOption != null)
          return sortOption;
        return new SortOption()
        {
          groupBy = this.ProjectIdentity is NormalProjectIdentity ? "sortOrder" : "project",
          orderBy = "sortOrder"
        };
      }
      set
      {
        TimelineModel timelineModel = this.GetTimelineModel();
        if (timelineModel == null || value.Equal(timelineModel.sortOption))
          return;
        timelineModel.sortOption = value;
        if (timelineModel.sortOption != null)
          timelineModel.SortType = timelineModel.sortOption.groupBy == "none" ? timelineModel.sortOption.orderBy : timelineModel.sortOption.groupBy;
        this.ProjectIdentity?.CommitTimeline(timelineModel);
        DataChangedNotifier.NotifySortOptionChanged(this.ProjectIdentity?.CatId);
      }
    }

    private async void UpdateTimelineSortType()
    {
      await this.UpdateGroupAsync();
      await this.UpdateCellLeftAsync();
      await this.UpdateCellLineAsync();
      await this.UpdateSortArrangeAsync();
    }

    public double OneDayWidth => TimelineConstants.GetOneDayWidth(this.TimelineRangeIndex);

    public double MinChangeDays => (double) TimelineConstants.GetMinChangeDays(this.TimelineRange);

    public bool NeedsSplitLine => TimelineConstants.GetNeedsSplitLine(this.TimelineRange);

    public int NewTaskDefaultDays => TimelineConstants.GetNewTaskDefaultDays(this.TimelineRange);

    public double OneLineHeight => 40.0;

    public double Height => this.OneLineHeight * (double) this.LineCount;

    public double Width
    {
      get
      {
        DateTime dateTime1;
        DateTime dateTime2;
        this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out dateTime2);
        DateTime dateTime3 = dateTime1;
        return (double) (dateTime2 - dateTime3).Days * this.OneDayWidth;
      }
    }

    public int LineCount
    {
      get => this._lineCount;
      set
      {
        this._lineCount = value;
        this.OnPropertyChanged(nameof (LineCount));
        this.OnPropertyChanged("Height");
      }
    }

    public double XOffset
    {
      get => this._xOffset;
      set
      {
        this._xOffset = value;
        this.UpdateYearModel(false, true);
        DelayActionHandlerCenter.TryDoAction(this._uid + "TimelineXOffset", (EventHandler) ((sender, args) => ThreadUtil.DetachedRunOnUiThread((Action) (() => this.OnPropertyChanged(nameof (XOffset))))), 200);
      }
    }

    public double YearNameWidth
    {
      get => this._yearNameWidth;
      set => this.ChangeAndNotify<double>(ref this._yearNameWidth, value, nameof (YearNameWidth));
    }

    public string YearName
    {
      get => this._yearName;
      set
      {
        if (!(this._yearName != value))
          return;
        this._yearName = value;
        this.OnPropertyChanged(nameof (YearName));
      }
    }

    public bool HideCompleted
    {
      get => LocalSettings.Settings.HideComplete;
      set
      {
        LocalSettings.Settings.HideComplete = value;
        LocalSettings.Settings.Save();
        SettingsHelper.PushLocalSettings();
      }
    }

    public bool IsOverDue
    {
      get => LocalSettings.Settings.ExtraSettings.TimelineArrangeOverDue;
      set
      {
        LocalSettings.Settings.ExtraSettings.TimelineArrangeOverDue = value;
        this.OnPropertyChanged(nameof (IsOverDue));
        this.SetArrangeModels();
        LocalSettings.Settings.Save(true);
      }
    }

    public Visibility ArrangeEmptyTaskVisibility
    {
      get => this._arrangeEmptyTaskVisibility;
      set
      {
        this.ChangeAndNotify<Visibility>(ref this._arrangeEmptyTaskVisibility, value, nameof (ArrangeEmptyTaskVisibility));
      }
    }

    public bool IsDark
    {
      get => this._isDark;
      set
      {
        this._isDark = value;
        this.OnPropertyChanged(nameof (IsDark));
        foreach (TimelineCellViewModel timelineCellViewModel in this.CellModels.ToList())
          timelineCellViewModel.NotifyColorChanged();
      }
    }

    public bool GotoBtnEnabled
    {
      get => this._gotoBtnEnabled;
      set
      {
        this._gotoBtnEnabled = value;
        this.OnPropertyChanged(nameof (GotoBtnEnabled));
      }
    }

    public bool GroupEditing { get; private set; }

    public bool AvailableReset
    {
      get => this._availableReset;
      set => this.ChangeAndNotify<bool>(ref this._availableReset, value, nameof (AvailableReset));
    }

    public bool Editing { get; set; }

    public bool Hovering { get; set; }

    public bool ShowNoteInArrange
    {
      get => LocalSettings.Settings.ExtraSettings.TimelineShowNote;
      set
      {
        LocalSettings.Settings.ExtraSettings.TimelineShowNote = value;
        this.SetArrangeModels();
        LocalSettings.Settings.Save(true);
      }
    }

    public bool ShowParentInArrange
    {
      get => LocalSettings.Settings.ExtraSettings.TimelineShowParent;
      set
      {
        LocalSettings.Settings.ExtraSettings.TimelineShowParent = value;
        this.UpdateSortArrangeAsync();
        LocalSettings.Settings.Save(true);
      }
    }

    public bool CheckTaskMatched(string taskId)
    {
      if (!(this.ProjectIdentity is FilterProjectIdentity))
        return true;
      List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, new List<string>()
      {
        taskId
      });
      matchedTasks?.RemoveAll((Predicate<TaskBaseViewModel>) (t => this.HideCompleted && t.Status != 0));
      if (matchedTasks != null && matchedTasks.Count != 0)
        return true;
      TimelineCellViewModel model = this.CellModels.FirstOrDefault((Func<TimelineCellViewModel, bool>) (m => m.Id == taskId));
      if (model != null)
        this.RemoveCell(model);
      return false;
    }

    public void NotifyItemPosChanged(TimelineCellViewModel cellModel)
    {
      this.AvailableModels.OnItemPosChanged(cellModel);
    }

    public void Reload()
    {
      if (this._batchSelectedModels.Any() || this.ProjectIdentity == null)
        return;
      this.SetProjectIdentity(this.ProjectIdentity.Copy(this.ProjectIdentity));
    }

    public async void SetGroupEditing(bool value)
    {
      if (!value)
        await Task.Delay(200);
      this.GroupEditing = value;
    }

    public void SetItemEditing(bool value) => this._itemEditing = value;

    public TimelineCellViewModel GetCellModel(string taskId)
    {
      return this.CellModels.Where((Predicate<TimelineCellViewModel>) (c => c.Id == taskId)).FirstOrDefault<TimelineCellViewModel>();
    }

    public void OnSettingChanged(string name)
    {
      switch (name)
      {
        case "Color":
          this.OnPropertyChanged("ColorType");
          using (List<TimelineCellViewModel>.Enumerator enumerator = this.CellModels.ToList().GetEnumerator())
          {
            while (enumerator.MoveNext())
              enumerator.Current.NotifyColorChanged();
            break;
          }
        case "ShowWeek":
          this.OnPropertyChanged("ShowWeek");
          this.OnPropertyChanged("HeadHeight");
          break;
      }
    }

    public void OnLocalSettingsChanged(string name)
    {
      switch (name)
      {
        case "HideComplete":
          this.ResetAllAsync();
          this.OnPropertyChanged("HideCompleted");
          break;
        case "ShowSubtasks":
          this.ResetAllAsync();
          break;
      }
    }

    public void OnSortOptionChanged()
    {
      this.OnPropertyChanged("TimelineSortOption");
      this.UpdateTimelineSortType();
    }

    public async void ToggleGroupsOpen()
    {
      List<TimelineGroupViewModel> list = this.GroupModels.Where<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (g => g.ShowArrow == Visibility.Visible)).ToList<TimelineGroupViewModel>();
      if (!this.ShowGroup || list.Count <= 0)
        return;
      bool allOpen = list.All<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (g => g.IsOpen));
      list.ForEach((Action<TimelineGroupViewModel>) (g => g.ToggleOpen(!allOpen)));
      await this.UpdateCellLineAsync();
    }

    public ObservableCollection<TimelineDisplayBase> ArrangeModels
    {
      get => this._arrangeModels;
      set
      {
        this._arrangeModels = value;
        this.OnPropertyChanged(nameof (ArrangeModels));
      }
    }

    private List<TimelineDisplayBase> OverDueModels { get; } = new List<TimelineDisplayBase>();

    private List<TimelineDisplayBase> NoDateModels { get; } = new List<TimelineDisplayBase>();

    private bool IsCellOverDue(TimelineCellViewModel model)
    {
      if (!(this.ProjectIdentity is NormalProjectIdentity) && !model.Editable || model.DisplayModel == null)
        return false;
      DateTime? startDate = model.DisplayModel.StartDate;
      DateTime? nullable1 = model.DisplayModel.DueDate;
      DateTime dateTime;
      if (model.IsAllDay)
      {
        DateTime? nullable2;
        if (!nullable1.HasValue)
        {
          nullable2 = new DateTime?();
        }
        else
        {
          dateTime = nullable1.GetValueOrDefault();
          nullable2 = new DateTime?(dateTime.AddDays(-1.0));
        }
        nullable1 = nullable2;
      }
      DateTime? nullable3 = nullable1 ?? startDate;
      dateTime = DateTime.Today;
      return nullable3.HasValue && nullable3.GetValueOrDefault() < dateTime;
    }

    private bool IsCellNoDate(TimelineCellViewModel model)
    {
      return (this.ProjectIdentity is NormalProjectIdentity || model.Editable) && !((DateTime?) model.DisplayModel?.DueDate).HasValue && !((DateTime?) model.DisplayModel?.StartDate).HasValue;
    }

    private void ClearArrangeModels()
    {
      this.NoDateModels.Clear();
      this.OverDueModels.Clear();
      this.ArrangeModels.Clear();
      this.UpdateArrangeEmptyTaskVisibility();
    }

    private void RemoveCellInArrangeCollection(
      List<TimelineDisplayBase> collection,
      TimelineCellViewModel cellModel)
    {
      int index = collection.IndexOf((TimelineDisplayBase) cellModel) - 1;
      collection.Remove((TimelineDisplayBase) cellModel);
      if (index <= -1 || index >= collection.Count || !(collection[index] is TimelineGroupViewModel) || collection.Count != index + 1 && (index + 1 >= collection.Count || !(collection[index] is TimelineGroupViewModel) || !(collection[index + 1] is TimelineGroupViewModel)))
        return;
      collection.RemoveAt(index);
    }

    private async Task UpdateCellToArrange(TimelineCellViewModel cellModel, bool remove = false)
    {
      await this._updateArrangeSemaphoreSlim.WaitAsync();
      bool flag = false;
      try
      {
        flag = await this.UpdateCellToArrangeInternal(cellModel, remove);
      }
      finally
      {
        this._updateArrangeSemaphoreSlim.Release();
      }
      if (!flag)
      {
        await this.UpdateSortArrangeAsync();
      }
      else
      {
        if (!this._isArranging)
          return;
        this.SetArrangeModels();
      }
    }

    private async Task<bool> UpdateCellToArrangeInternal(
      TimelineCellViewModel cellModel,
      bool remove)
    {
      if (cellModel?.DisplayModel == null)
        return true;
      remove = remove || cellModel.DisplayModel.Deleted != 0;
      if (cellModel.Status != 0)
      {
        this.RemoveCellInArrangeCollection(this.OverDueModels, cellModel);
        this.RemoveCellInArrangeCollection(this.NoDateModels, cellModel);
        return true;
      }
      bool flag1 = this.IsCellOverDue(cellModel);
      bool flag2 = this.IsCellNoDate(cellModel);
      if (!flag1 | remove)
        this.RemoveCellInArrangeCollection(this.OverDueModels, cellModel);
      if (!flag2 | remove)
        this.RemoveCellInArrangeCollection(this.NoDateModels, cellModel);
      if (((flag1 ? 0 : (!flag2 ? 1 : 0)) | (remove ? 1 : 0)) != 0)
        return true;
      List<TimelineDisplayBase> timelineDisplayBaseList = flag1 ? this.OverDueModels : this.NoDateModels;
      int num = timelineDisplayBaseList.IndexOf((TimelineDisplayBase) cellModel);
      if (!(this.ProjectIdentity is NormalProjectIdentity))
      {
        if (num > 0)
        {
          for (int index = num; index > 0; --index)
          {
            if (timelineDisplayBaseList[index] is TimelineGroupViewModel timelineGroupViewModel && timelineGroupViewModel.Id == cellModel.DisplayModel.ProjectId)
              return true;
          }
        }
        return false;
      }
      if (num > 0)
      {
        int index;
        for (index = num; index > 0; --index)
        {
          if (timelineDisplayBaseList[index] is TimelineGroupViewModel timelineGroupViewModel && timelineGroupViewModel.Id == cellModel.DisplayModel.ColumnId)
            return true;
        }
        if (string.IsNullOrEmpty(cellModel.DisplayModel.ColumnId) && index == 0)
          return true;
      }
      return false;
    }

    public async Task UpdateSortArrangeAsync(bool updateGroup = true)
    {
      TimelineViewModel timelineViewModel = this;
      await timelineViewModel._cellLock.WaitAsync();
      await timelineViewModel._updateArrangeSemaphoreSlim.WaitAsync();
      try
      {
        List<TimelineCellViewModel> avaModels = timelineViewModel.CellModels.Where((Predicate<TimelineCellViewModel>) (c => c.DisplayModel != null && c.DisplayModel.IsTaskOrNote && c.Status == 0)).ToList<TimelineCellViewModel>();
        Constants.SortType sortType = timelineViewModel.ProjectIdentity is NormalProjectIdentity ? Constants.SortType.sortOrder : Constants.SortType.project;
        List<TimelineCellViewModel> overDueModels = avaModels.Where<TimelineCellViewModel>(new Func<TimelineCellViewModel, bool>(timelineViewModel.IsCellOverDue)).ToList<TimelineCellViewModel>();
        List<TimelineGroupViewModel> timelineGroupViewModelList1;
        if (updateGroup)
          timelineGroupViewModelList1 = await timelineViewModel.GetGroupModels(timelineViewModel.ProjectIdentity, sortType, (IList<TimelineCellViewModel>) overDueModels, false);
        else
          timelineGroupViewModelList1 = timelineViewModel.OverDueModels.OfType<TimelineGroupViewModel>().ToList<TimelineGroupViewModel>();
        List<TimelineGroupViewModel> groups1 = timelineGroupViewModelList1;
        timelineViewModel.UpdateSortArrangeInternal(sortType, overDueModels, groups1, timelineViewModel.OverDueModels);
        List<TimelineCellViewModel> noDateModels = avaModels.Where<TimelineCellViewModel>(new Func<TimelineCellViewModel, bool>(timelineViewModel.IsCellNoDate)).ToList<TimelineCellViewModel>();
        List<TimelineGroupViewModel> timelineGroupViewModelList2;
        if (updateGroup)
          timelineGroupViewModelList2 = await timelineViewModel.GetGroupModels(timelineViewModel.ProjectIdentity, sortType, (IList<TimelineCellViewModel>) noDateModels, false);
        else
          timelineGroupViewModelList2 = timelineViewModel.NoDateModels.OfType<TimelineGroupViewModel>().ToList<TimelineGroupViewModel>();
        List<TimelineGroupViewModel> groups2 = timelineGroupViewModelList2;
        timelineViewModel.UpdateSortArrangeInternal(sortType, noDateModels, groups2, timelineViewModel.NoDateModels);
        if (timelineViewModel._isArranging)
          timelineViewModel.SetArrangeModels();
        avaModels = (List<TimelineCellViewModel>) null;
        overDueModels = (List<TimelineCellViewModel>) null;
        noDateModels = (List<TimelineCellViewModel>) null;
      }
      finally
      {
        timelineViewModel._updateArrangeSemaphoreSlim.Release();
        timelineViewModel._cellLock.Release();
      }
    }

    private void UpdateArrangeEmptyTaskVisibility()
    {
      this.ArrangeEmptyTaskVisibility = this.ArrangeModels.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateSortArrangeInternal(
      Constants.SortType sortType,
      List<TimelineCellViewModel> cells,
      List<TimelineGroupViewModel> groups,
      List<TimelineDisplayBase> collection)
    {
      collection.Clear();
      if (sortType == Constants.SortType.tag)
        cells.Sort((IComparer<TimelineCellViewModel>) new SortHelper.TimelineTagComparer());
      else
        cells.Sort(new Comparison<TimelineCellViewModel>(TimelineViewModel.TimelineCellViewModelCompare));
      switch (sortType)
      {
        case Constants.SortType.sortOrder:
          if (groups.Any<TimelineGroupViewModel>())
          {
            using (List<TimelineGroupViewModel>.Enumerator enumerator = groups.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                TimelineGroupViewModel group = enumerator.Current;
                collection.Add((TimelineDisplayBase) group);
                if (group.IsArrangeOpen)
                {
                  IEnumerable<TimelineDisplayBase> sortedResult = GetSortedResult(cells.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => item.DisplayModel.ColumnId == group.Id)).ToList<TimelineCellViewModel>());
                  collection.AddRange(sortedResult);
                }
              }
              break;
            }
          }
          else
          {
            IEnumerable<TimelineDisplayBase> sortedResult = GetSortedResult(cells);
            collection.AddRange(sortedResult);
            break;
          }
        case Constants.SortType.project:
          using (List<TimelineGroupViewModel>.Enumerator enumerator = groups.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              TimelineGroupViewModel group = enumerator.Current;
              collection.Add((TimelineDisplayBase) group);
              if (group.IsArrangeOpen)
              {
                IEnumerable<TimelineDisplayBase> sortedResult = GetSortedResult(cells.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => item.DisplayModel.ProjectId == group.Id)).ToList<TimelineCellViewModel>());
                collection.AddRange(sortedResult);
              }
            }
            break;
          }
      }

      IEnumerable<TimelineDisplayBase> GetSortedResult(List<TimelineCellViewModel> children)
      {
        List<TimelineCellViewModel> result = new List<TimelineCellViewModel>();
        List<TaskBaseViewModel> list = children.Select<TimelineCellViewModel, TaskBaseViewModel>((Func<TimelineCellViewModel, TaskBaseViewModel>) (m => m.DisplayModel)).ToList<TaskBaseViewModel>();
        Dictionary<string, TimelineCellViewModel> cDict = new Dictionary<string, TimelineCellViewModel>();
        foreach (TimelineCellViewModel child in children)
        {
          cDict[child.Id] = child;
          child.IsOpen = !this._foldIds.Contains(child.Id);
          child.ParentItem = (TimelineCellViewModel) null;
          child.ChildItems.Clear();
        }
        foreach (Node<TaskBaseViewModel> node1 in TaskNodeUtils.GetTaskNodeTree(list))
        {
          if (!node1.HasParent)
          {
            AddChild(node1);
            foreach (Node<TaskBaseViewModel> node2 in node1.GetAllChildrenNode())
              AddChild(node2);
          }
        }
        return (IEnumerable<TimelineDisplayBase>) result;

        void AddChild(Node<TaskBaseViewModel> node)
        {
          TimelineCellViewModel timelineCellViewModel1 = cDict[node.NodeId];
          TimelineCellViewModel timelineCellViewModel2;
          if (!string.IsNullOrEmpty(node.ParentId) && cDict.TryGetValue(node.ParentId, out timelineCellViewModel2))
          {
            timelineCellViewModel1.ParentItem = timelineCellViewModel2;
            timelineCellViewModel2.ChildItems.Add(timelineCellViewModel1);
            timelineCellViewModel2.IsParent = LocalSettings.Settings.ExtraSettings.TimelineShowParent;
          }
          if (timelineCellViewModel1.AllParentItemOpen())
            result.Add(timelineCellViewModel1);
          timelineCellViewModel1.Level = this.ShowParentInArrange ? node.GetLevel(0) : 0;
        }
      }
    }

    private void SetArrangeModels()
    {
      List<TimelineDisplayBase> source = this.IsOverDue ? this.OverDueModels : this.NoDateModels;
      if (this.ShowNoteInArrange)
        ItemsSourceHelper.CopyTo<TimelineDisplayBase>(source, this.ArrangeModels);
      else
        ItemsSourceHelper.CopyTo<TimelineDisplayBase>(source.Where<TimelineDisplayBase>((Func<TimelineDisplayBase, bool>) (m => m.Type != DisplayType.Note)).ToList<TimelineDisplayBase>(), this.ArrangeModels);
      this.UpdateArrangeEmptyTaskVisibility();
    }

    public void ToggleArrangeItemOpen(TimelineCellViewModel model)
    {
      model.IsOpen = !model.IsOpen;
      if (model.IsOpen)
        this._foldIds.Remove(model.Id);
      else
        this._foldIds.Add(model.Id);
      this.UpdateSortArrangeAsync();
    }

    public bool BatchSelect => this._batchSelectedModels.Count > 0;

    public void BatchSelectInRect(Rect rect)
    {
      foreach (TimelineCellViewModel timelineCellViewModel in this.AvailableModels.Value)
      {
        if ((timelineCellViewModel.DisplayModel.IsTaskOrNote || timelineCellViewModel.DisplayModel.Type == DisplayType.Agenda) && timelineCellViewModel.Editable && !timelineCellViewModel.Operation.Contain(TimelineCellOperation.Fold) && new Rect(timelineCellViewModel.Left, timelineCellViewModel.Top, timelineCellViewModel.Width, 40.0).IntersectsWith(rect) && !timelineCellViewModel.Operation.Contain(TimelineCellOperation.BatchSelect))
          timelineCellViewModel.Operation = timelineCellViewModel.Operation.Add(TimelineCellOperation.BatchSelect);
      }
    }

    public void TryBatchSelectOnMove(Rect rect)
    {
      if (Math.Abs(rect.Width - this._checkedWidth) < 5.0 && Math.Abs(rect.Height - this._checkedHeight) < 5.0)
        return;
      this._checkedWidth = rect.Width;
      this._checkedHeight = rect.Height;
      foreach (TimelineCellViewModel timelineCellViewModel in this.AvailableModels.Value)
      {
        if ((timelineCellViewModel.DisplayModel.IsTaskOrNote || timelineCellViewModel.DisplayModel.Type == DisplayType.Agenda) && timelineCellViewModel.Editable && !timelineCellViewModel.Operation.Contain(TimelineCellOperation.Fold))
        {
          if (new Rect(timelineCellViewModel.Left, timelineCellViewModel.Top, timelineCellViewModel.Width, 40.0).IntersectsWith(rect))
          {
            if (!timelineCellViewModel.Operation.Contain(TimelineCellOperation.BatchSelect))
            {
              timelineCellViewModel.Operation = timelineCellViewModel.Operation.Add(TimelineCellOperation.BatchSelect);
              this._tempBatchSelectedModels.Add(timelineCellViewModel);
            }
          }
          else if (this._tempBatchSelectedModels.Contains(timelineCellViewModel))
          {
            timelineCellViewModel.Operation = timelineCellViewModel.Operation.Remove(TimelineCellOperation.BatchSelect);
            this._tempBatchSelectedModels.Remove(timelineCellViewModel);
          }
        }
      }
    }

    public bool ClearBatchSelect()
    {
      if (!this._batchSelectedModels.Any())
        return false;
      foreach (TimelineCellViewModel timelineCellViewModel in this._batchSelectedModels.ToList())
      {
        timelineCellViewModel.Operation = timelineCellViewModel.Operation.Remove(TimelineCellOperation.BatchSelect);
        timelineCellViewModel.DragStatus = 0;
      }
      this._batchSelectedModels.Clear();
      return true;
    }

    public void ChangeBatchSelectStatus(TimelineCellViewModel model)
    {
      if (!model.BatchSelected && (!model.DisplayModel.IsTaskOrNote && model.DisplayModel.Type != DisplayType.Agenda || !model.Editable || model.Operation.Contain(TimelineCellOperation.Fold)))
        return;
      if (model.Operation.Contain(TimelineCellOperation.BatchSelect))
      {
        model.Operation = model.Operation.Remove(TimelineCellOperation.BatchSelect);
        model.DragStatus = 0;
      }
      else
        model.Operation = model.Operation.Add(TimelineCellOperation.BatchSelect);
    }

    public void OnItemBatchSelectChanged(TimelineCellViewModel model)
    {
      int num = model.Operation.Contain(TimelineCellOperation.BatchSelect) ? 1 : 0;
      bool flag = this._batchSelectedModels.Contains(model);
      if (num != 0 && !flag)
        this._batchSelectedModels.Add(model);
      if (num == 0 & flag)
      {
        this._batchSelectedModels.Remove(model);
        model.DragStatus = 0;
      }
      DelayActionHandlerCenter.TryDoAction("NotifyBatchSelectDateChanged", (EventHandler) ((sender, args) => ThreadUtil.DetachedRunOnUiThread((Action) (() => this.OnPropertyChanged("HoverStartEndTuples")))), 100);
    }

    public async void ShowBatchOperationDialog(Action hideAction)
    {
    }

    public List<string> GetSelectedTaskIds()
    {
      return this._batchSelectedModels.Select<string>((Func<TimelineCellViewModel, string>) (m => m.Id));
    }

    public void OnBatchDrag(int spanDays)
    {
      foreach (TimelineCellViewModel timelineCellViewModel in this._batchSelectedModels.ToList())
      {
        DateTime dateTime = timelineCellViewModel.StartDate;
        timelineCellViewModel.TrySetStartDate(new DateTime?(dateTime.AddDays((double) spanDays)));
        DateTime? endDate = timelineCellViewModel.EndDate;
        ref DateTime? local = ref endDate;
        DateTime? nullable;
        if (!local.HasValue)
        {
          nullable = new DateTime?();
        }
        else
        {
          dateTime = local.GetValueOrDefault();
          nullable = new DateTime?(dateTime.AddDays((double) spanDays));
        }
        timelineCellViewModel.EndDate = nullable;
        timelineCellViewModel.SetLeft();
      }
      this.OnPropertyChanged("HoverStartEndTuples");
    }

    public async void SetBatchDragging(int status, bool delay = false)
    {
      if (delay)
        await Task.Delay(1);
      foreach (TimelineCellViewModel timelineCellViewModel in this._batchSelectedModels.ToList())
        timelineCellViewModel.DragStatus = status;
    }

    public int GetSelectedTaskCounts() => this._batchSelectedModels.Count;

    public void ClearTempBatchSelect()
    {
      this._tempBatchSelectedModels.Clear();
      this._checkedWidth = 0.0;
      this._checkedHeight = 0.0;
    }

    public bool IsBatchSelectOverGroup()
    {
      if (this._batchSelectedModels.Count <= 1)
        return false;
      string str1 = (string) null;
      foreach (TimelineCellViewModel timelineCellViewModel in this._batchSelectedModels.ToList())
      {
        TimelineCellViewModel model = timelineCellViewModel;
        string str2 = this.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= model.Line))?.Id ?? string.Empty;
        if (str1 == null)
          str1 = str2;
        if (str2 != str1)
          return true;
      }
      return false;
    }

    public async Task OnBatchDragDrop(int? line)
    {
      TimelineViewModel timelineViewModel = this;
      int num;
      if (line.HasValue)
      {
        string groupBy1 = timelineViewModel.TimelineSortOption.groupBy;
        Constants.SortType sortType = Constants.SortType.sortOrder;
        string str1 = sortType.ToString();
        if (!(groupBy1 == str1))
        {
          string groupBy2 = timelineViewModel.TimelineSortOption.groupBy;
          sortType = Constants.SortType.project;
          string str2 = sortType.ToString();
          num = groupBy2 == str2 ? 1 : 0;
        }
        else
          num = 1;
      }
      else
        num = 0;
      List<string> selectedIds = timelineViewModel.GetSelectedTaskIds();
      List<string> ids = num != 0 ? TaskDao.GetTreeTopIds(selectedIds, string.Empty) : selectedIds;
      Stopwatch sw = new Stopwatch();
      sw.Start();
      if (line.HasValue)
        await timelineViewModel.CommitBatchGroup(ids, line.Value);
      HashSet<string> matchedIds = (HashSet<string>) null;
      if (timelineViewModel.ProjectIdentity is FilterProjectIdentity)
      {
        List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(timelineViewModel.ProjectIdentity, selectedIds);
        // ISSUE: reference to a compiler-generated method
        matchedTasks?.RemoveAll(new Predicate<TaskBaseViewModel>(timelineViewModel.\u003COnBatchDragDrop\u003Eb__273_0));
        matchedIds = new HashSet<string>((matchedTasks != null ? matchedTasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (m => m.Id)) : (IEnumerable<string>) null) ?? (IEnumerable<string>) new List<string>());
      }
      foreach (TimelineCellViewModel model in timelineViewModel._batchSelectedModels.ToList())
      {
        if (matchedIds != null && !matchedIds.Contains(model.Id))
          timelineViewModel.RemoveCell(model);
        else
          await model.CommitDate((TimeData) null);
      }
      sw.Stop();
      selectedIds = (List<string>) null;
      sw = (Stopwatch) null;
      matchedIds = (HashSet<string>) null;
    }

    public async Task CommitBatchGroup(List<string> ids, int line)
    {
      string groupBy = this.TimelineSortOption.groupBy;
      TimelineGroupViewModel group = this.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= line));
      if (group == null)
        ;
      else
      {
        switch (groupBy)
        {
          case "tag":
            if (group.IsOpen)
              break;
            this.UpdateGroupAsync();
            break;
          case "priority":
            int result;
            if (int.TryParse(group.Id ?? "", out result))
            {
              int num = await TaskService.BatchSetPriority(ids, result) ? 1 : 0;
              goto case "tag";
            }
            else
              goto case "tag";
          case "project":
            List<TaskBaseViewModel> andChildrenInBatch = TaskCache.GetTaskAndChildrenInBatch(ids);
            ProjectModel projectById = CacheManager.GetProjectById(group.Id);
            // ISSUE: explicit non-virtual call
            if (projectById != null && projectById.IsEnable() && andChildrenInBatch != null && __nonvirtual (andChildrenInBatch.Count) > 0 && andChildrenInBatch[0].ProjectId != group.Id)
            {
              await TaskService.BatchMoveProject(andChildrenInBatch.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (s => s.Id)).ToList<string>(), new MoveProjectArgs(projectById));
              goto case "tag";
            }
            else
              goto case "tag";
          case "assignee":
            TaskBaseViewModel taskById1 = TaskCache.GetTaskById(ids[0]);
            if (taskById1 != null && taskById1.Assignee != group.Id)
            {
              if (!string.IsNullOrEmpty(group.Id) && group.Id != "-1")
              {
                List<TaskBaseViewModel> tasksByIds = TaskCache.GetTasksByIds(ids);
                List<string> list = tasksByIds != null ? tasksByIds.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (m => m.ProjectId)).Distinct<string>().ToList<string>() : (List<string>) null;
                // ISSUE: explicit non-virtual call
                if (list != null && __nonvirtual (list.Count) > 0)
                {
                  foreach (string projectId in list)
                  {
                    List<ShareUserModel> projectUsers = AvatarHelper.GetProjectUsers(projectId);
                    if (projectUsers == null || projectUsers.Count == 0 || projectUsers.All<ShareUserModel>((Func<ShareUserModel, bool>) (a => (a.userId.ToString() ?? "") != group.Id)))
                    {
                      Utils.Toast(Utils.GetString("ChangeAssigneeError"));
                      return;
                    }
                  }
                }
              }
              await TaskService.BatchSetAssignee(ids, group.Id);
              goto case "tag";
            }
            else
              goto case "tag";
          default:
            foreach (string id in ids)
            {
              TaskBaseViewModel taskById2 = TaskCache.GetTaskById(id);
              if (taskById2 != null && taskById2.ColumnId != group.Id)
              {
                if (!string.IsNullOrEmpty(taskById2.ParentId))
                  await TaskDao.UpdateParent(id, string.Empty);
                await TaskService.BatchSaveTasksColumnId(ids, group.Id, group.ProjectId);
              }
            }
            goto case "tag";
        }
      }
    }

    public void ResetBatchDates()
    {
      foreach (TimelineCellViewModel timelineCellViewModel in this._batchSelectedModels.ToList())
      {
        timelineCellViewModel.TrySetStartDate(timelineCellViewModel.DisplayModel.StartDate);
        timelineCellViewModel.SetLeft();
        timelineCellViewModel.EndDate = timelineCellViewModel.DisplayModel.DueDate;
      }
    }

    private static int SortInternal(
      List<TimelineCellViewModel> cells,
      int startLine,
      Constants.SortType sortType = Constants.SortType.sortOrder,
      bool notify = true)
    {
      if (cells == null)
        return 0;
      if (sortType == Constants.SortType.tag)
        cells.Sort((IComparer<TimelineCellViewModel>) new SortHelper.TimelineTagComparer());
      else
        cells.Sort(new Comparison<TimelineCellViewModel>(TimelineViewModel.TimelineCellViewModelCompare));
      int line = startLine;
      while (cells.Any<TimelineCellViewModel>())
      {
        TimelineCellViewModel timelineCellViewModel1 = cells.First<TimelineCellViewModel>();
        timelineCellViewModel1.SetLine(line, notify);
        timelineCellViewModel1.MaxWidth = double.PositiveInfinity;
        while (true)
        {
          if (timelineCellViewModel1 != null)
          {
            TimelineCellViewModel cellViewModel = timelineCellViewModel1;
            cells.Remove(timelineCellViewModel1);
            TimelineCellViewModel timelineCellViewModel2 = cells.FirstOrDefault<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c =>
            {
              DateTime? endDate = cellViewModel.EndDate;
              if (!endDate.HasValue)
                return cellViewModel.StartDate.Date < c.StartDate.Date;
              DateTime valueOrDefault = endDate.GetValueOrDefault();
              return cellViewModel.IsAllDay || valueOrDefault == valueOrDefault.Date ? valueOrDefault.AddSeconds(-1.0).Date < c.StartDate.Date : valueOrDefault.Date < c.StartDate.Date;
            }));
            if (timelineCellViewModel2 != null)
            {
              timelineCellViewModel1.MaxWidth = Math.Max(timelineCellViewModel2.Left - timelineCellViewModel1.Left, 6.0);
              timelineCellViewModel2.SetLine(line, notify);
              cells.Remove(timelineCellViewModel2);
            }
            else
              timelineCellViewModel1.MaxWidth = double.PositiveInfinity;
            timelineCellViewModel1 = timelineCellViewModel2;
          }
          else
            break;
        }
        ++line;
      }
      return line - startLine;
    }

    private static int TimelineCellViewModelCompare(
      TimelineCellViewModel a,
      TimelineCellViewModel b)
    {
      DateTime startDate1 = a.StartDate;
      DateTime date1 = startDate1.Date;
      startDate1 = b.StartDate;
      DateTime date2 = startDate1.Date;
      if (date1 != date2)
        return a.StartDate.CompareTo(b.StartDate);
      if (Math.Abs(b.Width - a.Width) > 0.01)
        return b.Width.CompareTo(a.Width);
      if (a.IsAllDay != b.IsAllDay)
        return a.IsAllDay.CompareTo(b.IsAllDay);
      DateTime startDate2 = a.StartDate;
      TimeSpan timeOfDay1 = startDate2.TimeOfDay;
      ref TimeSpan local1 = ref timeOfDay1;
      startDate2 = b.StartDate;
      TimeSpan timeOfDay2 = startDate2.TimeOfDay;
      int num = local1.CompareTo(timeOfDay2);
      if (num != 0)
        return num;
      if (a.DisplayModel.Priority != b.DisplayModel.Priority)
        return b.DisplayModel.Priority.CompareTo(a.DisplayModel.Priority);
      if (a.DisplayModel.Type != b.DisplayModel.Type && (a.DisplayModel.IsCheckItem || b.DisplayModel.IsCheckItem))
      {
        DateTime? createdTime = a.DisplayModel.CreatedTime;
        if (createdTime.HasValue)
        {
          createdTime = b.DisplayModel.CreatedTime;
          if (createdTime.HasValue)
          {
            createdTime = a.DisplayModel.CreatedTime;
            DateTime dateTime1 = createdTime.Value;
            ref DateTime local2 = ref dateTime1;
            createdTime = b.DisplayModel.CreatedTime;
            DateTime dateTime2 = createdTime.Value;
            return local2.CompareTo(dateTime2);
          }
        }
      }
      if (a.DisplayModel.ProjectOrder != b.DisplayModel.ProjectOrder)
        return a.DisplayModel.ProjectOrder.CompareTo(b.DisplayModel.ProjectOrder);
      return a.SortOrder != b.SortOrder ? a.SortOrder.CompareTo(b.SortOrder) : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
    }

    private int SortColumns(
      ProjectIdentity project,
      List<TimelineCellViewModel> availableTask,
      Constants.SortType orderBy,
      bool notify)
    {
      int num1 = 0;
      List<string> list = this.GroupModels.Select<TimelineGroupViewModel, string>((Func<TimelineGroupViewModel, string>) (g => g.Id)).ToList<string>();
      foreach (TimelineGroupViewModel groupModel in (Collection<TimelineGroupViewModel>) this.GroupModels)
      {
        TimelineGroupViewModel column = groupModel;
        if (!(column.ProjectId != project.GetProjectId()))
        {
          int num2 = 1;
          List<TimelineCellViewModel> tasks = availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => item.DisplayModel.ColumnId == column.Id)).ToList<TimelineCellViewModel>();
          if (num1 == 0)
          {
            foreach (TimelineCellViewModel timelineCellViewModel in availableTask)
            {
              if (string.IsNullOrEmpty(timelineCellViewModel.DisplayModel.ColumnId) || !list.Contains(timelineCellViewModel.DisplayModel.ColumnId))
              {
                TaskService.SaveTaskColumnId(timelineCellViewModel.Id, column.Id, true);
                tasks.Add(timelineCellViewModel);
              }
            }
          }
          availableTask.RemoveAll((Predicate<TimelineCellViewModel>) (c => tasks.Contains(c)));
          if (!column.IsOpen)
            tasks.ForEach((Action<TimelineCellViewModel>) (t => t.Operation = TimelineCellOperation.Fold));
          else if (tasks.Any<TimelineCellViewModel>())
          {
            tasks.ForEach((Action<TimelineCellViewModel>) (t =>
            {
              if (!t.Operation.IsNormalStatus())
                return;
              t.Operation = TimelineCellOperation.None;
            }));
            num2 += orderBy == Constants.SortType.sortOrder ? TimelineViewModel.SortInternal(tasks, num1, notify: notify) : this.SortDueDate(tasks, num1, notify: notify);
          }
          column.Height = (double) num2 * this.OneLineHeight;
          column.Line = num1;
          num1 += num2;
        }
      }
      return num1;
    }

    private int SortTag(
      List<TimelineCellViewModel> availableTask,
      Constants.SortType sortType,
      bool notify = true)
    {
      int num1 = 0;
      Dictionary<string, long> sortedTagDict = TagDataHelper.GetTagSortDict();
      foreach (TimelineGroupViewModel groupModel in (Collection<TimelineGroupViewModel>) this.GroupModels)
      {
        TimelineGroupViewModel gp = groupModel;
        List<TimelineCellViewModel> tasks = gp.Id == "!tag" ? availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => item.DisplayModel.Tags == null || item.DisplayModel.Tags.Length == 0)).ToList<TimelineCellViewModel>() : availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => item.DisplayModel.Tags != null && ((IEnumerable<string>) item.DisplayModel.Tags).Any<string>() && TagDataHelper.GetPrimaryTag(sortedTagDict, (IList<string>) item.DisplayModel.Tags, (ICollection<string>) null) == gp.Id)).ToList<TimelineCellViewModel>();
        int num2 = 1;
        availableTask.RemoveAll((Predicate<TimelineCellViewModel>) (t => tasks.Contains(t)));
        if (!gp.IsOpen)
          tasks.ForEach((Action<TimelineCellViewModel>) (t => t.Operation = TimelineCellOperation.Fold));
        else if (tasks.Any<TimelineCellViewModel>())
        {
          tasks.ForEach((Action<TimelineCellViewModel>) (t =>
          {
            if (t.Operation != TimelineCellOperation.Fold)
              return;
            t.Operation = TimelineCellOperation.None;
          }));
          num2 += sortType == Constants.SortType.sortOrder ? TimelineViewModel.SortInternal(tasks, num1, Constants.SortType.tag, notify) : this.SortDueDate(tasks, num1, Constants.SortType.tag, notify);
        }
        gp.Height = (double) num2 * this.OneLineHeight;
        gp.Line = num1;
        num1 += num2;
      }
      return num1;
    }

    private int SortPriority(
      List<TimelineCellViewModel> availableTask,
      Constants.SortType sortType,
      bool notify = true)
    {
      int num1 = 0;
      foreach (TimelineGroupViewModel groupModel in (Collection<TimelineGroupViewModel>) this.GroupModels)
      {
        TimelineGroupViewModel gp = groupModel;
        List<TimelineCellViewModel> tasks = gp.Id == "note" ? availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => item.DisplayModel.IsNote)).ToList<TimelineCellViewModel>() : availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => !item.DisplayModel.IsNote && item.DisplayModel.Priority.ToString() == gp.Id)).ToList<TimelineCellViewModel>();
        int num2 = 1;
        availableTask.RemoveAll((Predicate<TimelineCellViewModel>) (t => tasks.Contains(t)));
        if (!gp.IsOpen)
          tasks.ForEach((Action<TimelineCellViewModel>) (t => t.Operation = TimelineCellOperation.Fold));
        else if (tasks.Any<TimelineCellViewModel>())
        {
          tasks.ForEach((Action<TimelineCellViewModel>) (t =>
          {
            if (t.Operation != TimelineCellOperation.Fold)
              return;
            t.Operation = TimelineCellOperation.None;
          }));
          num2 += sortType == Constants.SortType.sortOrder ? TimelineViewModel.SortInternal(tasks, num1, notify: notify) : this.SortDueDate(tasks, num1, notify: notify);
        }
        gp.Height = (double) num2 * this.OneLineHeight;
        gp.Line = num1;
        num1 += num2;
      }
      return num1;
    }

    private int SortProjects(
      List<TimelineCellViewModel> availableTask,
      Constants.SortType orderBy,
      bool notify = true)
    {
      int num1 = 0;
      foreach (TimelineGroupViewModel groupModel in (Collection<TimelineGroupViewModel>) this.GroupModels)
      {
        TimelineGroupViewModel gp = groupModel;
        List<TimelineCellViewModel> tasks = availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (item => item.DisplayModel.IsEvent ? gp.Id == "Calendar5959a2259161d16d23a4f272" : item.DisplayModel.ProjectId == gp.Id)).ToList<TimelineCellViewModel>();
        int num2 = 1;
        availableTask.RemoveAll((Predicate<TimelineCellViewModel>) (t => tasks.Contains(t)));
        if (!gp.IsOpen)
          tasks.ForEach((Action<TimelineCellViewModel>) (t => t.Operation = TimelineCellOperation.Fold));
        else if (tasks.Any<TimelineCellViewModel>())
        {
          tasks.ForEach((Action<TimelineCellViewModel>) (t =>
          {
            if (t.Operation != TimelineCellOperation.Fold)
              return;
            t.Operation = TimelineCellOperation.None;
          }));
          num2 += orderBy == Constants.SortType.sortOrder ? TimelineViewModel.SortInternal(tasks, num1, notify: notify) : this.SortDueDate(tasks, num1, notify: notify);
        }
        gp.Height = (double) num2 * this.OneLineHeight;
        gp.Line = num1;
        num1 += num2;
      }
      return num1;
    }

    private int SortDueDate(
      List<TimelineCellViewModel> availableTask,
      int start,
      Constants.SortType groupBy = Constants.SortType.sortOrder,
      bool notify = true)
    {
      int num = 0;
      if (groupBy == Constants.SortType.tag)
        availableTask.Sort((IComparer<TimelineCellViewModel>) new SortHelper.TimelineTagComparer());
      else
        availableTask.Sort(new Comparison<TimelineCellViewModel>(TimelineViewModel.TimelineCellViewModelCompare));
      foreach (TimelineCellViewModel timelineCellViewModel in availableTask)
      {
        timelineCellViewModel.SetLine(start + num, notify);
        if (timelineCellViewModel.Operation.IsNormalStatus())
          timelineCellViewModel.Operation = TimelineCellOperation.None;
        timelineCellViewModel.MaxWidth = double.PositiveInfinity;
        ++num;
      }
      return num;
    }

    private int SortAssignee(
      List<TimelineCellViewModel> availableTask,
      Constants.SortType orderBy,
      bool notify = true)
    {
      int num1 = 0;
      foreach (TimelineGroupViewModel groupModel in (Collection<TimelineGroupViewModel>) this.GroupModels)
      {
        TimelineGroupViewModel group = groupModel;
        int num2 = 1;
        List<TimelineCellViewModel> list;
        if (group.Id == "-1")
        {
          list = availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => c.DisplayModel.GetAssignee() == group.Id)).ToList<TimelineCellViewModel>();
          list.AddRange(availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => this.GroupModels.All<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (g => g.Id != c.DisplayModel.GetAssignee())))));
        }
        else
          list = availableTask.Where<TimelineCellViewModel>((Func<TimelineCellViewModel, bool>) (c => c.DisplayModel.GetAssignee() == group.Id)).ToList<TimelineCellViewModel>();
        if (list.Any<TimelineCellViewModel>())
        {
          list.ForEach((Action<TimelineCellViewModel>) (t =>
          {
            if (!t.Operation.IsNormalStatus())
              return;
            t.Operation = TimelineCellOperation.None;
          }));
          num2 += orderBy == Constants.SortType.sortOrder ? TimelineViewModel.SortInternal(list, num1, notify: notify) : this.SortDueDate(list, num1, notify: notify);
        }
        group.Height = (double) num2 * this.OneLineHeight;
        group.Line = num1;
        num1 += num2;
      }
      return num1;
    }
  }
}
