// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.DetailView.BatchDetailView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Time;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MainListView.DetailView
{
  public class BatchDetailView : ContentControl
  {
    private TaskDetailBatchControl _batchControl;
    private List<string> _selectedTaskIds;
    private TaskView _parent;
    private SetBatchTaskGridArgs _batchArgs;
    private TaskDetailViewModel _task;
    private TaskType _taskType;

    private ProjectIdentity _projectIdentity => this._parent?.ProjectIdentity;

    public event EventHandler<List<string>> TaskBatchDeleted;

    public BatchDetailView(TaskView parent)
    {
      TaskDetailBatchControl detailBatchControl = new TaskDetailBatchControl(this);
      detailBatchControl.Margin = new Thickness(0.0, 34.0, 0.0, 0.0);
      this._batchControl = detailBatchControl;
      this.Content = (object) this._batchControl;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
      this._parent = parent;
    }

    public async void OnBatchSelect(List<string> selected)
    {
      BatchDetailView batchDetailView = this;
      if (selected.Count < 1)
        return;
      TaskBaseViewModel baseModel = new TaskBaseViewModel()
      {
        Status = 0,
        StartDate = new DateTime?(),
        DueDate = new DateTime?(),
        IsAllDay = new bool?(true),
        Progress = 0,
        Kind = "TEXT"
      };
      TaskDetailViewModel model = new TaskDetailViewModel(baseModel)
      {
        IsOwner = true
      };
      List<TaskModel> tasks = await TaskDao.GetThinTasksInBatch(selected);
      int completedCount = 0;
      string projectId = (string) null;
      string columnId = (string) null;
      string assignee = (string) null;
      BatchData batchData = (BatchData) null;
      SetBatchTaskGridArgs batchGridArgs = new SetBatchTaskGridArgs();
      if (tasks.Count > 0)
      {
        batchData = new BatchData();
        TaskModel task = tasks[0];
        List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
        batchData.StartDate = task.startDate;
        batchData.DueDate = task.dueDate;
        batchData.IsAllDay = task.isAllDay;
        batchData.Reminders = remindersByTaskId.ToList<TaskReminderModel>();
        batchData.RepeatFlag = task.repeatFlag;
        batchData.RepeatFrom = task.repeatFrom;
        batchData.IsFloating = task.Floating;
        batchData.TimeZone = task.timeZone;
        baseModel.IsAllDay = task.isAllDay;
        baseModel.StartDate = task.startDate;
        baseModel.DueDate = task.dueDate;
        baseModel.RepeatFlag = task.repeatFlag;
        baseModel.RepeatFrom = task.repeatFrom;
        baseModel.ProjectName = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == task.projectId))?.name;
        baseModel.TimeZoneName = task.timeZone;
        baseModel.Priority = task.priority;
        columnId = task.columnId;
      }
      foreach (TaskModel task in tasks)
      {
        batchGridArgs.IsAllPinned = batchGridArgs.IsAllPinned && task.pinnedTimeStamp > 0L;
        batchGridArgs.IsAllCompleted = batchGridArgs.IsAllCompleted && task.isCompleted;
        batchGridArgs.HasAbandoned = batchGridArgs.HasAbandoned || task.isAbandoned;
        if (task.priority != model.Priority)
        {
          baseModel.Priority = 0;
          batchGridArgs.HasSamePriority = false;
        }
        if (task.columnId != columnId)
          columnId = "-1";
        if (batchGridArgs.HasSameTag && batchGridArgs.TagStr != task.tag)
          batchGridArgs.HasSameTag = false;
        DateTime? nullable1 = model.StartDate;
        DateTime? nullable2;
        bool? isAllDay1;
        bool? isAllDay2;
        if (nullable1.HasValue)
        {
          nullable1 = model.StartDate;
          nullable2 = task.startDate;
          if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
          {
            nullable2 = model.DueDate;
            nullable1 = task.dueDate;
            if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() != nullable1.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
            {
              isAllDay1 = model.IsAllDay;
              isAllDay2 = task.isAllDay;
              if (isAllDay1.GetValueOrDefault() == isAllDay2.GetValueOrDefault() & isAllDay1.HasValue == isAllDay2.HasValue || task.Floating != model.IsFloating || !(task.timeZone == model.TimeZoneName))
                goto label_18;
            }
          }
          TaskBaseViewModel taskBaseViewModel1 = baseModel;
          nullable1 = new DateTime?();
          DateTime? nullable3 = nullable1;
          taskBaseViewModel1.StartDate = nullable3;
          TaskBaseViewModel taskBaseViewModel2 = baseModel;
          nullable1 = new DateTime?();
          DateTime? nullable4 = nullable1;
          taskBaseViewModel2.DueDate = nullable4;
          batchGridArgs.HasSameDate = false;
        }
label_18:
        if (task.repeatFlag != model.RepeatFlag || task.repeatFrom != model.RepeatFrom)
        {
          baseModel.RepeatFlag = (string) null;
          baseModel.RepeatFrom = (string) null;
        }
        if (batchData != null)
        {
          if (batchData.IsDateUnified && !DateUtils.IsSameDate(task.startDate, batchData.StartDate))
            batchData.IsDateUnified = false;
          if (batchData.IsTimeUnified)
          {
            if (DateUtils.IsSameTime(task.startDate, batchData.StartDate))
            {
              nullable1 = task.dueDate;
              if (!nullable1.HasValue && !batchData.DueDate.HasValue)
                goto label_28;
            }
            nullable1 = batchData.StartDate;
            nullable2 = task.startDate;
            if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
            {
              nullable2 = batchData.DueDate;
              nullable1 = task.dueDate;
              if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() == nullable1.GetValueOrDefault() ? 1 : 0) : 1) : 0) == 0)
                goto label_29;
            }
            else
              goto label_29;
label_28:
            isAllDay2 = batchData.IsAllDay;
            isAllDay1 = task.isAllDay;
            if (isAllDay2.GetValueOrDefault() == isAllDay1.GetValueOrDefault() & isAllDay2.HasValue == isAllDay1.HasValue && task.Floating == batchData.IsFloating && task.timeZone == batchData.TimeZone)
              goto label_30;
label_29:
            batchData.IsTimeUnified = false;
          }
label_30:
          if (batchData.IsReminderUnified)
          {
            if (!TaskReminderDao.IsEquals(await TaskReminderDao.GetRemindersByTaskId(task.id), batchData.Reminders))
              batchData.IsReminderUnified = false;
          }
          if (batchData.IsRepeatUnified && (!(task.repeatFlag == batchData.RepeatFlag) || !(task.repeatFrom == batchData.RepeatFrom)))
            batchData.IsRepeatUnified = false;
        }
        if (task.deleted != 0)
        {
          batchGridArgs.IsDeleted = true;
          batchGridArgs.CanSwitchTaskOrNote = false;
        }
        if (task.status != 0)
        {
          ++completedCount;
          batchGridArgs.CanSwitchTaskOrNote = false;
        }
        if (TaskCache.IsParentTask(task.id) || TaskCache.IsChildTask(task.parentId, task.projectId))
          batchGridArgs.CanSwitchTaskOrNote = false;
        if (task.kind == "NOTE")
          batchGridArgs.HasNote = true;
        else
          batchGridArgs.HasTask = true;
        if (projectId == null || projectId == task.projectId)
        {
          projectId = task.projectId;
        }
        else
        {
          batchGridArgs.HasSameProject = false;
          projectId = "";
        }
        assignee = assignee == null || assignee == task.assignee ? task.assignee : "";
      }
      model.BatchData = batchData;
      baseModel.ProjectId = projectId;
      baseModel.Assignee = assignee;
      baseModel.ColumnId = columnId;
      if (completedCount == tasks.Count)
        baseModel.Status = 2;
      batchDetailView._taskType = !batchGridArgs.HasTask || !batchGridArgs.HasNote ? (!batchGridArgs.OnlyNote ? TaskType.Task : TaskType.Note) : TaskType.TaskAndNote;
      baseModel.Kind = batchGridArgs.OnlyNote ? "NOTE" : "TEXT";
      batchDetailView._task = model;
      batchDetailView.DataContext = (object) model;
      batchDetailView._selectedTaskIds = selected;
      batchDetailView._batchControl.SetAvatar(assignee, projectId);
      string[] strArray = Utils.GetString("YouHaveChosenTasks").Split('0');
      if (strArray.Length == 2)
      {
        batchDetailView._batchControl.BatchTaskText.Inlines.Clear();
        batchDetailView._batchControl.BatchTaskText.Inlines.Add(strArray[0].Substring(0, strArray[0].Length - 1));
        InlineCollection inlines = batchDetailView._batchControl.BatchTaskText.Inlines;
        Run run = new Run(selected.Count.ToString());
        run.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
        inlines.Add((Inline) run);
        batchDetailView._batchControl.BatchTaskText.Inlines.Add(strArray[1].Substring(1, strArray[1].Length - 1));
      }
      await batchDetailView.SetBatchTaskGridVisibility(batchGridArgs);
      baseModel = (TaskBaseViewModel) null;
      model = (TaskDetailViewModel) null;
      tasks = (List<TaskModel>) null;
      projectId = (string) null;
      columnId = (string) null;
      assignee = (string) null;
      batchData = (BatchData) null;
      batchGridArgs = (SetBatchTaskGridArgs) null;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      this.Content = (object) null;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      if (!this.IsVisible)
        return;
      BlockingSet<string> blockingSet = new BlockingSet<string>();
      blockingSet.AddRange(e.BatchChangedIds);
      blockingSet.AddRange(e.PinChangedIds);
      blockingSet.AddRange(e.StatusChangedIds);
      blockingSet.AddRange(e.DateChangedIds);
      blockingSet.AddRange(e.PriorityChangedIds);
      blockingSet.AddRange(e.ProjectChangedIds);
      blockingSet.AddRange(e.TagChangedIds);
      blockingSet.AddRange(e.KindChangedIds);
      if (blockingSet.Any())
      {
        this.CheckBatchSelectTasksChanged(blockingSet.Value);
      }
      else
      {
        if (e.PinChangedIds.Any())
        {
          List<string> selectedTaskIds = this._selectedTaskIds;
          if ((selectedTaskIds != null ? (selectedTaskIds.Any<string>(new Func<string, bool>(e.PinChangedIds.Contains)) ? 1 : 0) : 0) != 0)
          {
            List<TaskBaseViewModel> tasks = TaskCache.GetTasksByIds(this._selectedTaskIds.ToList<string>());
            if (tasks.Any<TaskBaseViewModel>())
              this.SetBatchStarGrid(tasks.All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.IsPinned == tasks[0].IsPinned)));
          }
        }
        if (!e.DeletedChangedIds.Any() || this._selectedTaskIds == null || !this._selectedTaskIds.Any<string>(new Func<string, bool>(e.DeletedChangedIds.Contains)))
          return;
        this._parent.HideDetail();
      }
    }

    private void SetBatchStarGrid(bool isPinned)
    {
      this._batchControl.BatchTaskStartPinIcon.Visibility = isPinned ? Visibility.Collapsed : Visibility.Visible;
      this._batchControl.BatchTaskStartUnPinIcon.Visibility = isPinned ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CheckBatchSelectTasksChanged(HashSet<string> ids)
    {
      if (this._selectedTaskIds == null || !this._selectedTaskIds.Any<string>(new Func<string, bool>(ids.Contains)))
        return;
      if (this._parent.ProjectIdentity is TrashProjectIdentity)
        this._parent.HideDetail();
      else
        this.OnBatchSelect(this._selectedTaskIds.ToList<string>());
    }

    public void SelectDateClick(MouseButtonEventArgs e) => this.ShowSelectDateDialog();

    public bool CheckEnable() => this._task.Enable;

    public void OnMoveProject(EscPopup popup, bool b)
    {
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) popup, new ProjectExtra()
      {
        ProjectIds = new List<string>()
        {
          this._task.ProjectId
        }
      }, new ProjectSelectorExtra()
      {
        showAll = false,
        batchMode = false,
        canSelectGroup = false,
        onlyShowPermission = true,
        CanSearch = true,
        ShowColumn = true,
        ColumnId = this._task.ColumnId
      });
      projectOrGroupPopup.ItemSelect += (EventHandler<SelectableItemViewModel>) ((o, e) => this.OnMoveProject(e));
      projectOrGroupPopup.Show();
    }

    private async void OnMoveProject(SelectableItemViewModel e)
    {
      if (e == null)
        return;
      (string str, string columnId) = e.GetProjectAndColumnId();
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == str));
      if (project == null)
        return;
      UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", "move_project");
      UtilLog.Info("TaskDetail.SetProject " + this._task?.TaskId + ", value " + project.id + " from:ProjectSelector");
      await this.BatchMoveProject(project, columnId);
      this.NotifyBatchChanged(true);
    }

    public void OnSetAssigneeMouseUp(bool inBatch, EscPopup popup)
    {
      if (Utils.IsNetworkAvailable())
      {
        SetAssigneeDialog setAssigneeDialog = new SetAssigneeDialog(this._task.ProjectId, (Popup) popup, this._task.Assignee);
        this.AddActionEvent("action", "assignee");
        setAssigneeDialog.AssigneeSelect += (EventHandler<AvatarInfo>) (async (o, avatar) => await this.SetAssignee(avatar.UserId, inBatch));
        setAssigneeDialog.Show();
      }
      else
        this.TryToast(Utils.GetString("NoNetwork"));
    }

    private async Task SetAssigneeImage(string assignee, string projectId)
    {
      if (!string.IsNullOrEmpty(assignee) && assignee != "-1")
      {
        string avatarUrl = await AvatarHelper.GetAvatarUrl(assignee, projectId);
        if (string.IsNullOrEmpty(avatarUrl))
          return;
        this._batchControl?.SetBatchAssignVisible(true, avatarUrl);
      }
      else
        this._batchControl?.SetBatchAssignVisible(true, (string) null);
    }

    private async Task SetAssignee(string assignee, bool inBatch)
    {
      this._task.SourceViewModel.Assignee = assignee;
      await TaskService.BatchSetAssignee(this._selectedTaskIds, assignee);
      if (this._parent?.GetSelectedProject() is AssignToMeProjectIdentity)
        this.NotifyBatchChanged(true);
      else
        this.SetAssigneeImage(assignee, this._task.ProjectId);
    }

    private async void OnDelete()
    {
      BatchDetailView child = this;
      Utils.FindParent<TaskView>((DependencyObject) child)?.HideDetail(true);
      UtilLog.Info("TaskDetail.DeleteTasks " + child._selectedTaskIds.Join<string>(";"));
      if (await TaskService.BatchDeleteTaskByIds(child._selectedTaskIds))
      {
        child.NotifyBatchDeleted();
        Utils.FindParent<ListViewContainer>((DependencyObject) child)?.HideDetail();
      }
      UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", "delete");
    }

    private async void OnSkipRecurrence(object sender, EventArgs e)
    {
      await TaskService.BatchSkipRecurrence(this._selectedTaskIds);
    }

    private async Task OnDateSelected(DateTime time)
    {
      await TaskService.BatchSetStartDate(this._selectedTaskIds, time);
      TaskChangeNotifier.NotifyBatchDateChanged(this._selectedTaskIds);
    }

    public async void OnRestoreProject()
    {
      await TaskService.BatchRestoreProject(this._selectedTaskIds);
    }

    public async void OnTag(Grid batchSetTagsButton)
    {
      UtilLog.Info("DetailShowSetTag");
      TagSelectData tagSelectData = await this.GetTagSelectData();
      UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", "tag");
      EscPopup escPopup = new EscPopup();
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.StaysOpen = false;
      escPopup.PlacementTarget = (UIElement) batchSetTagsButton;
      escPopup.HorizontalOffset = 20.0;
      escPopup.VerticalOffset = 0.0;
      EscPopup popup = escPopup;
      BatchSetTagControl batchSetTagControl = new BatchSetTagControl();
      batchSetTagControl.Close += (EventHandler) ((s, e) => popup.IsOpen = false);
      batchSetTagControl.TagsSelect += (EventHandler<TagSelectData>) ((s, e) =>
      {
        this.OnTagsSelected(e);
        popup.IsOpen = false;
      });
      popup.Child = (UIElement) batchSetTagControl;
      batchSetTagControl.Init(tagSelectData, true);
      popup.IsOpen = true;
    }

    private async Task<TagSelectData> GetTagSelectData()
    {
      if (this._selectedTaskIds == null || this._selectedTaskIds.Count <= 0)
        return new TagSelectData();
      ObservableCollection<TaskModel> tasksInTaskIds = await TaskDao.GetTasksInTaskIds(this._selectedTaskIds);
      return TagDataHelper.GetSelectTagData(tasksInTaskIds != null ? tasksInTaskIds.Select<TaskModel, List<string>>((Func<TaskModel, List<string>>) (task => TagSerializer.ToTags(task.tag))).ToList<List<string>>() : (List<List<string>>) null);
    }

    public async void OnTagsSelected(TagSelectData data)
    {
      UtilLog.Info(string.Format("TaskDetail.SetTasksTag taskCount {0} from:TagWindow", (object) this._selectedTaskIds?.Count));
      await TaskService.BatchSetTags(this._selectedTaskIds, data);
      await this.SetBatchTagsName();
    }

    public async void OnCopy()
    {
      BatchDetailView child = this;
      if (await ProChecker.CheckTaskLimit(child._task.ProjectId, child._selectedTaskIds.Count))
        return;
      UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", "duplicate");
      await TaskService.CopyTasks(child._selectedTaskIds);
      Utils.FindParent<ListViewContainer>((DependencyObject) child)?.HideDetail();
    }

    public void OnTasksDeleted(List<string> taskIds)
    {
      Utils.FindParent<ListViewContainer>((DependencyObject) this)?.HideDetail();
    }

    private async Task BatchClearDate()
    {
      BatchDetailView sender = this;
      await TaskService.BatchClearDate(sender._selectedTaskIds);
      sender._task.SetTimeData(new TimeData());
      sender._task.ExDates = (List<string>) null;
      TaskChangeNotifier.NotifyBatchDateChanged(sender._selectedTaskIds, (object) sender);
    }

    private async Task CompleteOrUndoneTask(bool? isComplete = null)
    {
      BatchDetailView batchDetailView = this;
      int num = batchDetailView._task.Status != 0 ? 0 : 2;
      isComplete = new bool?(((int) isComplete ?? (num == 2 ? 1 : 0)) != 0);
      if (isComplete.Value && batchDetailView._task.StartDate.HasValue && batchDetailView._task.DisplayStartDate.HasValue)
      {
        DateTime dateTime = batchDetailView._task.StartDate.Value;
        DateTime date1 = dateTime.Date;
        dateTime = batchDetailView._task.DisplayStartDate.Value;
        DateTime date2 = dateTime.Date;
        if (date1 != date2)
        {
          IToastShowWindow dependentWindow = Window.GetWindow((DependencyObject) batchDetailView) is TaskDetailWindow window ? window.DependentWindow : (IToastShowWindow) null;
          if (!await ModifyRepeatHandler.CompleteOrSkipRecurrence(batchDetailView._task.TaskId, batchDetailView._task.DisplayStartDate, dependentWindow))
            return;
        }
      }
      if (batchDetailView._selectedTaskIds == null || batchDetailView._selectedTaskIds.Count == 0)
        return;
      List<string> selectedTaskIds = batchDetailView._selectedTaskIds;
      UtilLog.Info(string.Format("TaskDetail.BatchCompleteTasks {0}, isComplete {1}", (object) (selectedTaskIds != null ? selectedTaskIds.Join<string>(";") : (string) null), (object) isComplete));
      await batchDetailView.BatchToggleCompleteTasks(batchDetailView._selectedTaskIds);
    }

    private async Task BatchSetDateAsync(TimeData model)
    {
      BatchDetailView sender = this;
      await TaskService.BatchSetDate(sender._selectedTaskIds, model);
      sender._task.SetTimeData(model);
      sender._task.ExDates = model.ExDates;
      TaskChangeNotifier.NotifyBatchDateChanged(sender._selectedTaskIds, (object) sender);
    }

    public async void PrioritySelect(bool inBatch, int priority)
    {
      string data = "priority_none";
      switch (priority)
      {
        case 0:
          data = "priority_none";
          break;
        case 1:
          data = "priority_low";
          break;
        case 3:
          data = "priority_medium";
          break;
        case 5:
          data = "priority_high";
          break;
      }
      UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", data);
      this.BatchSetPriority(priority);
    }

    private async void NotifyBatchDeleted()
    {
      BatchDetailView sender = this;
      sender._selectedTaskIds = (List<string>) null;
      EventHandler<List<string>> taskBatchDeleted = sender.TaskBatchDeleted;
      if (taskBatchDeleted == null)
        return;
      taskBatchDeleted((object) sender, sender._selectedTaskIds);
    }

    private async void NotifyBatchChanged(bool hideBatchGrid)
    {
      BatchDetailView child = this;
      if (!hideBatchGrid)
        return;
      child._selectedTaskIds = (List<string>) null;
      Utils.FindParent<ListViewContainer>((DependencyObject) child)?.HideDetail();
    }

    private async Task BatchMoveProject(ProjectModel project, string columnId = null)
    {
      List<TaskBaseViewModel> tasks = TaskCache.GetTaskAndChildrenInBatch(this._selectedTaskIds);
      await TaskDao.RemoveTaskParentIdInBatch(TaskNodeUtils.GetTaskNodeTree(tasks).Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (node => !node.HasParent)).Select<Node<TaskBaseViewModel>, string>((Func<Node<TaskBaseViewModel>, string>) (node => node.Value.Id)).ToList<string>());
      List<string> ids = tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>();
      await TaskService.BatchMoveProject(ids, new MoveProjectArgs(project)
      {
        ColumnId = columnId
      });
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(ids.FirstOrDefault<string>());
      TaskService.TryToastMoveControl(this._parent?.GetSelectedProject(), thinTaskById, project.id, true);
      tasks = (List<TaskBaseViewModel>) null;
      ids = (List<string>) null;
    }

    private async Task BatchToggleCompleteTasks(List<string> selectedTaskIds)
    {
      int num = await TaskService.BatchCompleteTasks(selectedTaskIds) ? 1 : 0;
      this.NotifyBatchChanged(false);
    }

    public async Task OnBatchDeleteTaskForever()
    {
      BatchDetailView batchDetailView = this;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DeleteForever"), Utils.GetString("BatchDeleteForeverHint"), Utils.GetString("Delete"), Utils.GetString("Cancel"));
      customerDialog.Owner = Window.GetWindow((DependencyObject) batchDetailView);
      customerDialog.Topmost = false;
      if (!customerDialog.ShowDialog().GetValueOrDefault())
        return;
      int num = await TaskService.BatchDeleteTaskByIds(batchDetailView._selectedTaskIds, false, 2) ? 1 : 0;
      batchDetailView.NotifyBatchDeleted();
      batchDetailView._parent?.TryExtractDetail();
    }

    private bool CheckUiTagEnable(object sender)
    {
      return ((sender is FrameworkElement frameworkElement ? frameworkElement.Tag : (object) null) as bool?).GetValueOrDefault();
    }

    public async void OnBatchCompleteMouseUp(object sender)
    {
      if (!this.CheckUiTagEnable(sender) || !this.CheckEnable())
        return;
      await this.CompleteOrUndoneTask();
    }

    public async void OnBatchPinClick(object sender, bool isPin)
    {
      if (!this.CheckUiTagEnable(sender))
        return;
      UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", isPin ? "pin" : "unpin");
      await TaskService.BatchStarTaskOrNote(this._selectedTaskIds, this._parent.ProjectIdentity?.CatId, isPin);
    }

    public async Task OnMergeMouseUp(object sender)
    {
      if (!this.CheckUiTagEnable(sender))
        return;
      List<string> selectedTaskIds = this._selectedTaskIds;
      // ISSUE: explicit non-virtual call
      bool flag = selectedTaskIds != null && __nonvirtual (selectedTaskIds.Count) > 0;
      TaskModel taskModel;
      if (flag)
      {
        taskModel = await TaskService.TryMergeTask(this._selectedTaskIds, this._selectedTaskIds[0], (IProjectTaskDefault) this._projectIdentity);
        flag = taskModel != null;
      }
      if (!flag)
        return;
      TaskChangeNotifier.NotifyTasksMerged(new List<string>((IEnumerable<string>) this._selectedTaskIds)
      {
        taskModel.id
      });
    }

    public async Task BatchSwitchTaskNoteMouseUp(object sender)
    {
      if (!this.CheckUiTagEnable(sender))
      {
        if (this._batchArgs.OnlyTask)
        {
          Utils.Toast(Utils.GetString("CannotConvertMultiLevelToNote"));
          return;
        }
        if (!this._batchArgs.OnlyNote || !this._batchArgs.OnlyTask)
        {
          Utils.Toast(Utils.GetString("CannotConvertMixedToNote"));
          return;
        }
      }
      await TaskService.BatchSwitchTaskOrNote(this._selectedTaskIds);
      UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", "convert_to_note_or_task");
      SyncManager.TryDelaySync();
      this.OnBatchSelect(this._selectedTaskIds.ToList<string>());
    }

    public async void OnBatchCopyTextMouseUp(object sender)
    {
      if (!this.CheckUiTagEnable(sender))
        return;
      List<TaskModel> thinTasksInBatch = await TaskDao.GetThinTasksInBatch(this._selectedTaskIds.ToList<string>());
      StringBuilder stringBuilder1 = new StringBuilder();
      foreach (TaskModel taskModel in thinTasksInBatch)
      {
        stringBuilder1.Append(string.IsNullOrEmpty(taskModel.title) ? Utils.GetString("NoTitle") : taskModel.title);
        if (!Utils.IsEmptyDate(taskModel.startDate))
        {
          bool flag = ((int) taskModel.isAllDay ?? 1) != 0;
          StringBuilder stringBuilder2 = stringBuilder1;
          DateTime start = taskModel.startDate.Value;
          DateTime? dueDate = taskModel.dueDate;
          ref DateTime? local = ref dueDate;
          DateTime? due = local.HasValue ? new DateTime?(local.GetValueOrDefault().AddDays(flag ? -1.0 : 0.0)) : new DateTime?();
          int num = flag ? 1 : 0;
          string str = "(" + DateUtils.FormatDateStringNoWord(start, due, num != 0) + ")";
          stringBuilder2.Append(str);
        }
        stringBuilder1.Append(Environment.NewLine);
      }
      if (stringBuilder1.Length - Environment.NewLine.Length >= 0)
        stringBuilder1.Remove(stringBuilder1.Length - Environment.NewLine.Length, Environment.NewLine.Length);
      try
      {
        Clipboard.SetDataObject((object) stringBuilder1.ToString(), true);
        Utils.Toast(Utils.GetString("CopyTextTips"));
      }
      catch (Exception ex)
      {
      }
    }

    public void OnBatchDelete(object sender)
    {
      if (!this.CheckUiTagEnable(sender))
        return;
      this.OnDelete();
    }

    public async Task SetBatchTaskGridVisibility(SetBatchTaskGridArgs args)
    {
      this._batchArgs = args;
      bool flag = this._parent?.GetSelectedProject() is TrashProjectIdentity;
      this._batchControl.BatchTaskMoveProjectButton.Visibility = flag ? Visibility.Collapsed : Visibility.Visible;
      this._batchControl.BatchTaskPriorityButton.Visibility = flag || args.HasNote ? Visibility.Collapsed : Visibility.Visible;
      this._batchControl.BatchTaskBlocks.Visibility = flag ? Visibility.Collapsed : Visibility.Visible;
      this._batchControl.BatchSetTagsButton.Visibility = flag ? Visibility.Collapsed : Visibility.Visible;
      this._batchControl.BatchSelectDateButton.Visibility = flag ? Visibility.Collapsed : Visibility.Visible;
      this._batchControl.BatchDeleteFromTrashButton.Visibility = Visibility.Collapsed;
      this._batchControl.BatchRestoreProjectButton.Visibility = Visibility.Collapsed;
      if (flag)
      {
        this._batchControl.BatchAssigneeGrid.Visibility = Visibility.Collapsed;
        this._batchControl.BatchCompleteOrUndoneBorder.Visibility = Visibility.Collapsed;
        if ((this._projectIdentity is TrashProjectIdentity projectIdentity ? (projectIdentity.IsPerson ? 1 : 0) : 1) != 0)
          this._batchControl.BatchDeleteFromTrashButton.Visibility = Visibility.Visible;
        this._batchControl.BatchRestoreProjectButton.Visibility = Visibility.Visible;
      }
      else
      {
        this._batchControl.BatchCompleteOrUndoneBorder.Tag = (object) true;
        this._batchControl.BatchTaskStarBorder.Tag = (object) args.CanPinOrUnPin;
        this._batchControl.BatchTaskMergeBorder.Tag = (object) args.CanMerge;
        this._batchControl.BatchTaskCopyBorder.Tag = (object) true;
        this._batchControl.BatchSwitchTaskNoteBorder.Tag = (object) (bool) (args.CanSwitchNote ? 1 : (args.CanSwitchTask ? 1 : 0));
        this._batchControl.BatchCopyTextBorder.Tag = (object) true;
        this._batchControl.BatchDeleteBorder.Tag = (object) args.CanDelete;
        await Task.Delay(50);
        await this.SetBatchTagsName();
        this.SetBatchProjectName(args.HasSameProject ? this._task.ProjectName : (string) null);
        this.SetBatchPriorityName(args.HasSamePriority);
        this.SetBatchStarGrid(args.IsAllPinned);
        this.SetBatchCompleteGrid(args.HasTask, args.IsAllCompleted);
        this.SetBatchSwitchTaskOrNote(args.OnlyTask);
      }
    }

    private async void ShowSelectDateDialog()
    {
      BatchDetailView batchDetailView = this;
      TimeData timeData1 = new TimeData();
      timeData1.StartDate = batchDetailView._task.DisplayStartDate;
      timeData1.DueDate = batchDetailView._task.DisplayDueDate;
      timeData1.IsAllDay = new bool?(!batchDetailView._task.DisplayStartDate.HasValue || ((int) batchDetailView._task.IsAllDay ?? 1) != 0);
      TaskReminderModel[] reminders = batchDetailView._task.Reminders;
      timeData1.Reminders = reminders != null ? ((IEnumerable<TaskReminderModel>) reminders).ToList<TaskReminderModel>() : (List<TaskReminderModel>) null;
      timeData1.RepeatFrom = batchDetailView._task.RepeatFrom;
      timeData1.RepeatFlag = batchDetailView._task.RepeatFlag;
      timeData1.ExDates = batchDetailView._task.ExDates;
      TimeData timeData = timeData1;
      if (!string.IsNullOrEmpty(batchDetailView._task.TimeZoneName))
        timeData.TimeZone = new TimeZoneViewModel(batchDetailView._task.IsFloating, batchDetailView._task.TimeZoneName);
      if (timeData.IsAllDay.HasValue && timeData.IsAllDay.Value && timeData.StartDate.HasValue && timeData.DueDate.HasValue)
      {
        DateTime dateTime = timeData.StartDate.Value;
        DateTime date1 = dateTime.Date;
        dateTime = timeData.DueDate.Value;
        DateTime date2 = dateTime.Date;
        if (date1 == date2)
          timeData.DueDate = new DateTime?();
      }
      if (batchDetailView._task.BatchData != null)
      {
        timeData.BatchData = batchDetailView._task.BatchData.Clone();
        timeData.IsDefault = !batchDetailView._task.BatchData.StartDate.HasValue && batchDetailView._task.BatchData.IsUnified;
      }
      if (timeData.StartDate.HasValue)
        timeData.IsDefault = false;
      bool canSkip = await TaskService.IsAllRepeatTask(batchDetailView._selectedTaskIds);
      bool isNote = batchDetailView._taskType == TaskType.Note || batchDetailView._taskType == TaskType.TaskAndNote && batchDetailView._parent?.GetSelectedProject()?.IsNote.GetValueOrDefault();
      SetDateDialog dialog = SetDateDialog.GetDialog();
      dialog.ClearEventHandle();
      // ISSUE: reference to a compiler-generated method
      dialog.Clear += new EventHandler(batchDetailView.\u003CShowSelectDateDialog\u003Eb__51_0);
      // ISSUE: reference to a compiler-generated method
      dialog.Save += new EventHandler<TimeData>(batchDetailView.\u003CShowSelectDateDialog\u003Eb__51_1);
      dialog.SkipRecurrence += new EventHandler(batchDetailView.OnSkipRecurrence);
      dialog.Show(timeData, new SetDateDialogArgs(isNote: isNote, target: (UIElement) batchDetailView._batchControl?.BatchSelectDateButton, hOffset: 24.0, placement: PlacementMode.Bottom, canSkip: canSkip));
      UtilLog.Info("DetailShowBatchSetDate");
      timeData = (TimeData) null;
    }

    private async Task SetBatchTagsName()
    {
      if (this._batchControl == null)
        return;
      TagSelectData tagSelectData = await this.GetTagSelectData();
      if (tagSelectData != null && tagSelectData.OmniSelectTags != null && tagSelectData.OmniSelectTags.Any<string>())
      {
        this._batchControl.BatchTagsContainer.Visibility = Visibility.Visible;
        this._batchControl.BatchTagsText.Visibility = Visibility.Collapsed;
        this._batchControl.BatchTagsContainer.Child = (UIElement) new TagDisplayControl((IReadOnlyCollection<string>) tagSelectData.OmniSelectTags, false)
        {
          CanClickTag = false
        };
      }
      else
      {
        this._batchControl.BatchTagsContainer.Child = (UIElement) null;
        this._batchControl.BatchTagsContainer.Visibility = Visibility.Collapsed;
        this._batchControl.BatchTagsText.Visibility = Visibility.Visible;
      }
    }

    private void SetBatchProjectName(string projectName = null)
    {
      if (this._batchControl == null)
        return;
      if (string.IsNullOrEmpty(projectName))
      {
        this._batchControl.BatchTaskMoveProjectButton.Title = Utils.GetString("MoveTo");
        this._batchControl.BatchTaskMoveProjectButton.ItemSelected = false;
      }
      else
      {
        this._batchControl.BatchTaskMoveProjectButton.Title = projectName;
        this._batchControl.BatchTaskMoveProjectButton.ItemSelected = true;
      }
    }

    private void SetBatchPriorityName(bool hasSamePriority)
    {
      if (!(this._batchControl?.PriorityText.Tag is string tag))
        return;
      if (string.IsNullOrEmpty(tag) || tag == Utils.GetString("PriorityNull") || !hasSamePriority)
      {
        this._batchControl.PriorityText.Text = Utils.GetString("priority");
        this._batchControl.PriorityText.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity80");
      }
      else
      {
        this._batchControl.PriorityText.Text = this._batchControl.PriorityText.Tag?.ToString();
        this._batchControl.PriorityText.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
      }
    }

    private void SetBatchSwitchTaskOrNote(bool isTaskToNote)
    {
      if (this._batchControl == null)
        return;
      if (isTaskToNote)
      {
        this._batchControl.SwitchTaskToNoteText.Visibility = Visibility.Visible;
        this._batchControl.SwitchNoteToTaskText.Visibility = Visibility.Collapsed;
        this._batchControl.SwitchPath.Data = Utils.GetIconData("IcSwitchNote");
      }
      else
      {
        this._batchControl.SwitchTaskToNoteText.Visibility = Visibility.Collapsed;
        this._batchControl.SwitchNoteToTaskText.Visibility = Visibility.Visible;
        this._batchControl.SwitchPath.Data = Utils.GetIconData("IcSwitchTask");
      }
    }

    private void SetBatchCompleteGrid(bool hasTask, bool isAllCompleted)
    {
      if (this._batchControl == null)
        return;
      this._batchControl.BatchCompleteOrUndoneBorder.Visibility = hasTask ? Visibility.Visible : Visibility.Collapsed;
      if (this._task == null)
        return;
      if (isAllCompleted)
      {
        this._batchControl.BatchTaskUndoneIcon.Visibility = Visibility.Visible;
        this._batchControl.BatchTaskDoneIcon.Visibility = Visibility.Collapsed;
      }
      else
      {
        this._batchControl.BatchTaskUndoneIcon.Visibility = Visibility.Collapsed;
        this._batchControl.BatchTaskDoneIcon.Visibility = Visibility.Visible;
      }
    }

    private async void BatchSetPriority(int priority)
    {
      int num = await TaskService.BatchSetPriority(this._selectedTaskIds, priority) ? 1 : 0;
      this._batchArgs.HasSamePriority = true;
      this._task.SourceViewModel.Priority = priority;
      this.SetBatchPriorityName(this._batchArgs.HasSamePriority);
    }

    public void BatchOpenSticky()
    {
      TaskStickyWindow.ShowTaskSticky(this._selectedTaskIds);
      UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", "open_as_sticky_note");
    }

    public void AddActionEvent(string ctype, string label)
    {
      UserActCollectUtils.AddClickEvent("task_detail", ctype, label);
    }

    public void TryToast(string getString)
    {
      Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.TryToastString((object) null, getString);
    }

    public void Dispose() => this._parent = (TaskView) null;

    public void OnCancel() => this._parent?.ClearBatchSelect();
  }
}
