// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.BatchTaskEditHelper
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
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Tag;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class BatchTaskEditHelper
  {
    private string _projectId;
    public ProjectIdentity ProjectIdentity;
    public bool UseInList = true;
    private int _quadrantLevel;
    private IBatchEditable _editor;
    private bool _canBatchUndo;
    private OperationExtra _args;

    public List<string> SelectedTaskIds { get; set; } = new List<string>();

    public event EventHandler<bool> ShowOrHideOperation;

    public event EventHandler<bool> CanUndo;

    public BatchTaskEditHelper(IBatchEditable editor, int quadrantLevel = 0)
    {
      this._quadrantLevel = quadrantLevel;
      this._editor = editor;
    }

    public async void ShowOperationDialog()
    {
      BatchTaskEditHelper sender = this;
      ObservableCollection<TaskModel> tasks = await TaskDao.GetTasksInTaskIds(sender.SelectedTaskIds);
      string columnId;
      TimeData timeData;
      BatchData batchData;
      if (tasks == null)
      {
        tasks = (ObservableCollection<TaskModel>) null;
        columnId = (string) null;
        timeData = (TimeData) null;
        batchData = (BatchData) null;
      }
      else if (tasks.Count == 0)
      {
        tasks = (ObservableCollection<TaskModel>) null;
        columnId = (string) null;
        timeData = (TimeData) null;
        batchData = (BatchData) null;
      }
      else
      {
        DateTime? completeDate = new DateTime?();
        bool showCompeteDate = false;
        bool existTask = false;
        bool existNote = false;
        sender._projectId = (string) null;
        columnId = (string) null;
        timeData = new TimeData();
        batchData = (BatchData) null;
        int priority = -1;
        bool isPinned = true;
        TaskModel task1;
        if (tasks.Count > 0)
        {
          batchData = new BatchData();
          task1 = tasks[0];
          if (task1 != null)
          {
            List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task1.id);
            batchData.StartDate = task1.startDate;
            batchData.DueDate = task1.dueDate;
            batchData.IsAllDay = task1.isAllDay;
            batchData.Reminders = remindersByTaskId.ToList<TaskReminderModel>();
            batchData.RepeatFlag = task1.repeatFlag;
            batchData.RepeatFrom = task1.repeatFrom;
            batchData.IsFloating = task1.Floating;
            batchData.TimeZone = task1.timeZone;
            timeData.StartDate = task1.startDate;
            timeData.DueDate = task1.dueDate;
            timeData.IsAllDay = task1.isAllDay;
            timeData.TimeZone = new TimeZoneViewModel(task1.Floating, task1.timeZone);
            batchData.Assign = task1.assignee;
            priority = task1.priority;
            columnId = task1.columnId;
            showCompeteDate = task1.status == 2;
            completeDate = showCompeteDate ? task1.completedTime : new DateTime?();
          }
          task1 = (TaskModel) null;
        }
        foreach (TaskModel taskModel in (Collection<TaskModel>) tasks)
        {
          task1 = taskModel;
          isPinned = isPinned && task1.pinnedTimeStamp > 0L;
          if (priority != -1 && task1.priority != priority)
            priority = -1;
          if (task1.columnId != columnId)
            columnId = "-1";
          if (batchData.Assign != task1.assignee)
            batchData.Assign = (string) null;
          DateTime? nullable1 = timeData.StartDate;
          DateTime? nullable2;
          bool? isAllDay1;
          bool? isAllDay2;
          if (nullable1.HasValue)
          {
            nullable1 = timeData.StartDate;
            nullable2 = task1.startDate;
            if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
            {
              nullable2 = timeData.DueDate;
              nullable1 = task1.dueDate;
              if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() != nullable1.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
              {
                isAllDay1 = timeData.IsAllDay;
                isAllDay2 = task1.isAllDay;
                if (isAllDay1.GetValueOrDefault() == isAllDay2.GetValueOrDefault() & isAllDay1.HasValue == isAllDay2.HasValue || task1.Floating != timeData.TimeZone.IsFloat || !(task1.timeZone == timeData.TimeZone.TimeZoneName))
                  goto label_22;
              }
            }
            TimeData timeData1 = timeData;
            nullable1 = new DateTime?();
            DateTime? nullable3 = nullable1;
            timeData1.StartDate = nullable3;
            TimeData timeData2 = timeData;
            nullable1 = new DateTime?();
            DateTime? nullable4 = nullable1;
            timeData2.DueDate = nullable4;
            timeData.IsAllDay = new bool?(true);
            timeData.TimeZone = TimeZoneData.LocalTimeZoneModel;
          }
label_22:
          if (batchData != null)
          {
            if (batchData.IsDateUnified && !DateUtils.IsSameDate(task1.startDate, batchData.StartDate))
              batchData.IsDateUnified = false;
            if (batchData.IsTimeUnified)
            {
              if (DateUtils.IsSameTime(task1.startDate, batchData.StartDate))
              {
                nullable1 = task1.dueDate;
                if (!nullable1.HasValue && !batchData.DueDate.HasValue)
                  goto label_30;
              }
              nullable1 = batchData.StartDate;
              nullable2 = task1.startDate;
              if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
              {
                nullable2 = batchData.DueDate;
                nullable1 = task1.dueDate;
                if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() == nullable1.GetValueOrDefault() ? 1 : 0) : 1) : 0) == 0)
                  goto label_31;
              }
              else
                goto label_31;
label_30:
              isAllDay2 = batchData.IsAllDay;
              isAllDay1 = task1.isAllDay;
              if (isAllDay2.GetValueOrDefault() == isAllDay1.GetValueOrDefault() & isAllDay2.HasValue == isAllDay1.HasValue && task1.Floating == batchData.IsFloating && task1.timeZone == batchData.TimeZone)
                goto label_32;
label_31:
              batchData.IsTimeUnified = false;
            }
label_32:
            if (batchData.IsReminderUnified)
            {
              if (!TaskReminderDao.IsEquals(await TaskReminderDao.GetRemindersByTaskId(task1.id), batchData.Reminders))
                batchData.IsReminderUnified = false;
            }
            if (batchData.IsRepeatUnified && (!(task1.repeatFlag == batchData.RepeatFlag) || !(task1.repeatFrom == batchData.RepeatFrom)))
              batchData.IsRepeatUnified = false;
          }
          sender._projectId = sender._projectId == null || sender._projectId == task1.projectId ? task1.projectId : "";
          showCompeteDate = showCompeteDate && task1.status == 2;
          if (showCompeteDate)
          {
            if (completeDate.HasValue)
            {
              nullable1 = task1.completedTime;
              if (nullable1.HasValue)
              {
                nullable1 = task1.completedTime;
                nullable2 = completeDate;
                if ((nullable1.HasValue & nullable2.HasValue ? (nullable1.GetValueOrDefault() > nullable2.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                  goto label_42;
              }
              else
                goto label_42;
            }
            completeDate = task1.completedTime;
            goto label_43;
          }
label_42:
          completeDate = new DateTime?();
label_43:
          if (!existTask && task1.kind == "TEXT")
            existTask = true;
          if (!existNote && task1.kind == "NOTE")
            existNote = true;
          task1 = (TaskModel) null;
        }
        timeData.BatchData = batchData;
        timeData.IsDefault = batchData != null && !batchData.StartDate.HasValue && batchData.IsUnified;
        TaskType kind = existNote ? (existTask ? TaskType.TaskAndNote : TaskType.Note) : TaskType.Task;
        bool flag1 = await TaskService.IsAllRepeatTask(sender.SelectedTaskIds);
        bool flag2 = kind == TaskType.Note || kind == TaskType.Task && !TaskCache.ExistParentTask(sender.SelectedTaskIds) && tasks.All<TaskModel>((Func<TaskModel, bool>) (task => task.status == 0 && string.IsNullOrEmpty(task.parentId)));
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == this._projectId));
        bool flag3 = projectModel != null && projectModel.IsShareList();
        sender._args = new OperationExtra()
        {
          InBatch = true,
          Priority = priority,
          ProjectId = sender._projectId,
          ColumnId = columnId,
          TimeModel = timeData,
          Tags = BatchTaskEditHelper.GetTagSelectData(tasks),
          ShowCopy = true,
          ShowCopyLink = false,
          ShowMerge = sender.UseInList,
          ShowAssignTo = flag3,
          ShowSkip = flag1,
          CompleteTime = completeDate,
          TaskType = kind,
          CanSwitch = flag2,
          ShowSwitch = true,
          Assignee = batchData.Assign,
          FailedSwitchTips = kind == TaskType.TaskAndNote ? Utils.GetString("CannotConvertMixedToNote") : Utils.GetString("CannotConvertMultiLevelToNote"),
          IsPinned = sender.UseInList ? new bool?(isPinned) : new bool?(),
          ShowCopyText = true
        };
        OperationExtra args = sender._args;
        ProjectIdentity projectIdentity = sender.ProjectIdentity;
        int num = projectIdentity != null ? (projectIdentity.IsNote ? 1 : 0) : 0;
        args.InNoteProject = num != 0;
        TaskOperationDialog dialog = new TaskOperationDialog(sender._args, sender._editor?.BatchOperaPlacementTarget() ?? sender._editor as UIElement);
        dialog.TimeClear += (EventHandler) ((arg, obj) =>
        {
          this._canBatchUndo = true;
          this.BatchClearDate();
          dialog.Dismiss();
        });
        dialog.TimeSelect += (EventHandler<TimeData>) ((arg, data) =>
        {
          this._canBatchUndo = true;
          this.BatchSetDate(data);
          dialog.Dismiss();
        });
        dialog.QuickDateSelect += (EventHandler<DateTime>) ((arg, date) =>
        {
          this._canBatchUndo = true;
          this.BatchSetStartDate(date);
          dialog.Dismiss();
        });
        dialog.PrioritySelect += new EventHandler<int>(sender.BatchSetPriority);
        dialog.Deleted += new EventHandler(sender.BatchDeleteTask);
        dialog.ProjectSelect += new EventHandler<SelectableItemViewModel>(sender.BatchSetProject);
        dialog.AssigneeSelect += new EventHandler<AvatarInfo>(sender.BatchSetAssignee);
        dialog.Merge += new EventHandler(sender.MergeTasks);
        dialog.TagsSelect += new EventHandler<TagSelectData>(sender.BatchSetTags);
        dialog.CompleteDateChanged += new EventHandler<DateTime>(sender.BatchSetCompleteDate);
        dialog.SkipCurrentRecurrence += new EventHandler(sender.BatchSkipRecurrence);
        dialog.SwitchTaskOrNote += new EventHandler(sender.OnSwitchTaskOrNoteClick);
        dialog.OpenSticky += new EventHandler(sender.OnOpenSticky);
        dialog.Toast += new EventHandler<string>(sender.OnTaskOperationToast);
        dialog.Starred += new EventHandler<bool>(sender.OnBatchStarred);
        dialog.Copied += new EventHandler(sender.OnBatchCopied);
        dialog.TextCopied += new EventHandler(sender.OnBatchTextCopied);
        dialog.Closed += (EventHandler) ((o, e) =>
        {
          EventHandler<bool> canUndo = this.CanUndo;
          if (canUndo != null)
            canUndo((object) this, this._canBatchUndo);
          EventHandler<bool> showOrHideOperation = this.ShowOrHideOperation;
          if (showOrHideOperation == null)
            return;
          showOrHideOperation((object) this, false);
        });
        dialog.Show();
        EventHandler<bool> showOrHideOperation1 = sender.ShowOrHideOperation;
        if (showOrHideOperation1 == null)
        {
          tasks = (ObservableCollection<TaskModel>) null;
          columnId = (string) null;
          timeData = (TimeData) null;
          batchData = (BatchData) null;
        }
        else
        {
          showOrHideOperation1((object) sender, true);
          tasks = (ObservableCollection<TaskModel>) null;
          columnId = (string) null;
          timeData = (TimeData) null;
          batchData = (BatchData) null;
        }
      }
    }

    private void OnOpenSticky(object sender, EventArgs e)
    {
      TaskStickyWindow.ShowTaskSticky(this.SelectedTaskIds.ToList<string>());
    }

    private void OnTaskOperationToast(object sender, string e) => Utils.Toast(e);

    private void OnBatchStarred(object sender, bool e)
    {
      TaskService.BatchStarTaskOrNote(new List<string>((IEnumerable<string>) this.SelectedTaskIds), this.ProjectIdentity?.CatId, e);
    }

    private async void OnBatchTextCopied(object sender, EventArgs e)
    {
      List<TaskModel> thinTasksInBatch = await TaskDao.GetThinTasksInBatch(this.SelectedTaskIds);
      if (thinTasksInBatch == null || thinTasksInBatch.Count == 0)
        return;
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

    private async void OnBatchCopied(object sender, EventArgs e)
    {
      List<string> ids = new List<string>((IEnumerable<string>) this.SelectedTaskIds);
      if (await ProChecker.CheckTaskLimit(this._projectId, ids.Count))
      {
        ids = (List<string>) null;
      }
      else
      {
        await TaskService.CopyTasks(ids);
        ids = (List<string>) null;
      }
    }

    private async void OnSwitchTaskOrNoteClick(object sender, EventArgs e)
    {
      List<string> ids = new List<string>((IEnumerable<string>) this.SelectedTaskIds);
      this._canBatchUndo = true;
      UtilLog.Info("BatchSwitchTaskNote " + this.SelectedTaskIds.Join<string>(";"));
      await TaskService.BatchSwitchTaskOrNote(ids);
      SyncManager.TryDelaySync();
      this._canBatchUndo = true;
    }

    private async void BatchSetCompleteDate(object sender, DateTime date)
    {
      await TaskService.BatchSetCompleteDate(this.SelectedTaskIds.ToList<string>(), date);
      this._editor?.ReloadList();
      SyncManager.TryDelaySync();
    }

    private async void BatchSkipRecurrence(object sender, EventArgs e)
    {
      await TaskService.BatchSkipRecurrence(this.SelectedTaskIds.ToList<string>());
    }

    private async void BatchSetTags(object sender, TagSelectData data)
    {
      this._canBatchUndo = true;
      await TaskService.BatchSetTags(this.SelectedTaskIds.ToList<string>(), data);
    }

    private async void MergeTasks(object sender, EventArgs e) => await this.MergeTasks();

    private async Task MergeTasks()
    {
      List<string> selectIds = this.SelectedTaskIds.ToList<string>();
      bool flag = selectIds.Count > 0;
      TaskModel taskModel;
      if (flag)
      {
        taskModel = await TaskService.TryMergeTask(selectIds, selectIds[0], (IProjectTaskDefault) this.ProjectIdentity);
        flag = taskModel != null;
      }
      if (!flag)
      {
        selectIds = (List<string>) null;
      }
      else
      {
        List<string> taskIds = new List<string>((IEnumerable<string>) selectIds);
        UtilLog.Info("MergeTasks newTask " + taskModel.id);
        taskIds.Add(taskModel.id);
        TaskChangeNotifier.NotifyTasksMerged(taskIds);
        selectIds = (List<string>) null;
      }
    }

    private async void BatchSetAssignee(object sender, AvatarInfo assignee)
    {
      if (!string.IsNullOrEmpty(assignee.UserId) && assignee.UserId != "-1")
      {
        List<TaskBaseViewModel> tasksByIds = TaskCache.GetTasksByIds(this.SelectedTaskIds.ToList<string>());
        List<string> list = tasksByIds != null ? tasksByIds.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (m => m.ProjectId)).Distinct<string>().ToList<string>() : (List<string>) null;
        // ISSUE: explicit non-virtual call
        if (list != null && __nonvirtual (list.Count) > 0)
        {
          foreach (string projectId in list)
          {
            List<ShareUserModel> projectUsers = AvatarHelper.GetProjectUsers(projectId);
            if (projectUsers == null || projectUsers.Count == 0 || projectUsers.All<ShareUserModel>((Func<ShareUserModel, bool>) (a => (a.userId.ToString() ?? "") != assignee.UserId)))
            {
              Utils.Toast(Utils.GetString("ChangeAssigneeError"));
              return;
            }
          }
        }
      }
      this._canBatchUndo = true;
      await TaskService.BatchSetAssignee(this.SelectedTaskIds.ToList<string>(), assignee.UserId);
    }

    private async void BatchSetProject(object sender, SelectableItemViewModel e)
    {
      (string projectId, string columnId) = e.GetProjectAndColumnId();
      ProjectModel project = CacheManager.GetProjectById(projectId);
      List<string> ids;
      if (project == null)
      {
        columnId = (string) null;
        project = (ProjectModel) null;
        ids = (List<string>) null;
      }
      else
      {
        List<TaskBaseViewModel> andChildrenInBatch = TaskCache.GetTaskAndChildrenInBatch(this.SelectedTaskIds.ToList<string>());
        ids = andChildrenInBatch.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>();
        if (this._args?.ProjectId != projectId || !string.IsNullOrEmpty(this._args?.ColumnId) && !(this._args?.ColumnId == columnId))
        {
          await TaskDao.RemoveTaskParentIdInBatch(TaskNodeUtils.GetTaskNodeTree(andChildrenInBatch).Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (node => !node.HasParent)).Select<Node<TaskBaseViewModel>, string>((Func<Node<TaskBaseViewModel>, string>) (node => node.Value.Id)).ToList<string>());
          await TaskService.BatchMoveProject(ids, new MoveProjectArgs(project)
          {
            ColumnId = columnId
          });
          TaskModel thinTaskById = await TaskDao.GetThinTaskById(ids.FirstOrDefault<string>());
          if (this._quadrantLevel == 0)
          {
            TaskService.TryToastMoveControl(this.ProjectIdentity, thinTaskById, project.id, true);
          }
          else
          {
            string message = (await MatrixManager.GetTaskQuadrantChangeString(this._quadrantLevel, thinTaskById.id)).Item2;
            if (!string.IsNullOrEmpty(message))
              Utils.Toast(message);
          }
        }
        if (string.IsNullOrEmpty(columnId))
        {
          columnId = (string) null;
          project = (ProjectModel) null;
          ids = (List<string>) null;
        }
        else if (!(this._args?.ColumnId != columnId))
        {
          columnId = (string) null;
          project = (ProjectModel) null;
          ids = (List<string>) null;
        }
        else
        {
          await TaskService.BatchSetColumn(ids, columnId);
          DataChangedNotifier.NotifyColumnChanged(project.id);
          columnId = (string) null;
          project = (ProjectModel) null;
          ids = (List<string>) null;
        }
      }
    }

    private async void BatchSetStartDate(DateTime date)
    {
      await TaskService.BatchSetStartDate(this.SelectedTaskIds.ToList<string>(), date);
    }

    private async void BatchSetDate(TimeData timeData)
    {
      await TaskService.BatchSetDate(this.SelectedTaskIds.ToList<string>(), timeData);
    }

    private async void BatchDeleteTask(object sender, EventArgs e)
    {
      int num = await TaskService.BatchDeleteTaskByIds(this.SelectedTaskIds.ToList<string>()) ? 1 : 0;
    }

    private static TagSelectData GetTagSelectData(ObservableCollection<TaskModel> tasks)
    {
      List<List<string>> data = new List<List<string>>();
      if (tasks != null && tasks.Any<TaskModel>())
        data.AddRange(tasks.Cast<TaskModel>().Select<TaskModel, List<string>>((Func<TaskModel, List<string>>) (task => !string.IsNullOrEmpty(task.tag) ? TagSerializer.ToTags(task.tag).ToList<string>() : new List<string>())));
      return TagDataHelper.GetSelectTagData(data);
    }

    private async void BatchClearDate()
    {
      await TaskService.BatchClearDate(this.SelectedTaskIds.ToList<string>());
    }

    private async void BatchSetPriority(object sender, int priority)
    {
      this._canBatchUndo = true;
      int num = await TaskService.BatchSetPriority(this.SelectedTaskIds.ToList<string>(), priority) ? 1 : 0;
    }

    public void ClearSelectedTaskIds() => this.SelectedTaskIds = new List<string>();

    public void Dispose()
    {
      this.ShowOrHideOperation = (EventHandler<bool>) null;
      this.CanUndo = (EventHandler<bool>) null;
    }
  }
}
