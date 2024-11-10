// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.TaskService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.DateParser;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Time;
using ticktick_WPF.Views.Undo;
using TickTickDao;
using TickTickModels;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class TaskService
  {
    private static readonly BlockingList<string> TaskPullingProjectIds = new BlockingList<string>();
    private static BlockingSet<string> _checkItemPulledIds = new BlockingSet<string>();
    private static HashSet<string> _pulledAttachmentTaskId = new HashSet<string>();

    public static async Task BatchAbandonTasks(List<string> selectedTaskIds)
    {
      foreach (TaskModel originalTask in (await TaskService.GetSelectedTasks(selectedTaskIds)).Where<TaskModel>((Func<TaskModel, bool>) (t => t.kind != "NOTE")).ToList<TaskModel>())
        TaskService.SetTaskStatus(originalTask, -1);
    }

    public static async Task<bool> BatchCompleteTasks(
      List<string> selectedTaskIds,
      bool ignoreRepeat = false,
      bool justComplete = false)
    {
      List<TaskModel> list = (await TaskService.GetSelectedTasks(selectedTaskIds)).Where<TaskModel>((Func<TaskModel, bool>) (t => t.kind != "NOTE")).ToList<TaskModel>();
      int num1 = list.Count<TaskModel>((Func<TaskModel, bool>) (task => task.status == 0));
      int num2 = list.Count<TaskModel>((Func<TaskModel, bool>) (task => task.status != 0));
      bool changed = false;
      if (list.Count == num1 || list.Count == num2 && !justComplete)
      {
        int newStatus = num1 > num2 ? 2 : 0;
        changed = list.Count > 0;
        TaskCloseExtra[] taskCloseExtraArray = await Task.WhenAll<TaskCloseExtra>(list.Select<TaskModel, Task<TaskCloseExtra>>((Func<TaskModel, Task<TaskCloseExtra>>) (task => TaskService.SetTaskStatus(task, newStatus, false, ignoreRepeat))));
      }
      else
      {
        changed = list.Count > 0;
        TaskCloseExtra[] taskCloseExtraArray = await Task.WhenAll<TaskCloseExtra>(list.Where<TaskModel>((Func<TaskModel, bool>) (task => task.status == 0)).Select<TaskModel, Task<TaskCloseExtra>>((Func<TaskModel, Task<TaskCloseExtra>>) (task => TaskService.SetTaskStatus(task, 2, false, ignoreRepeat))));
      }
      return changed;
    }

    public static async Task BatchAddTag(List<string> taskIds, string tag)
    {
      List<TaskModel> selectedTasks = await TaskService.GetSelectedTasks(taskIds);
      TagExtra tagExt = new TagExtra();
      for (int index = 0; index < selectedTasks.Count; ++index)
      {
        TaskModel taskModel = selectedTasks[index];
        List<string> tags = new List<string>();
        if (!string.IsNullOrEmpty(taskModel.tag))
          tags = TagSerializer.ToTags(taskModel.tag);
        if (!tags.Contains(tag))
          tags.Add(tag);
        taskModel.tag = TagSerializer.ToJsonContent(tags);
        taskModel.tags = tags.ToArray();
        taskModel.modifiedTime = new DateTime?(DateTime.Now);
        tagExt.AddTaskTagsPair(taskModel.id, tags);
      }
      await TaskService.BatchSaveTask(selectedTasks, CheckMatchedType.CheckTag);
      TaskChangeNotifier.NotifyTaskTagsChanged(tagExt);
      tagExt = (TagExtra) null;
    }

    public static async Task BatchSetTags(List<string> taskIds, TagSelectData tags)
    {
      List<TaskModel> selectedTasks = await TaskService.GetSelectedTasks(taskIds);
      TagExtra tagExt = new TagExtra();
      for (int index = 0; index < selectedTasks.Count; ++index)
      {
        TaskModel taskModel = selectedTasks[index];
        if (!string.IsNullOrEmpty(taskModel.tag))
          taskModel.tags = TagSerializer.ToTags(taskModel.tag).ToArray();
        List<string> tags1 = new List<string>();
        tags1.AddRange((IEnumerable<string>) tags.OmniSelectTags);
        if (taskModel.tags != null && taskModel.tags.Length != 0)
        {
          List<string> list = ((IEnumerable<string>) taskModel.tags).ToList<string>();
          tags1.AddRange(list.Where<string>((Func<string, bool>) (tag => !tags.OmniSelectTags.Contains(tag) && tags.PartSelectTags.Contains(tag))));
        }
        taskModel.tag = TagSerializer.ToJsonContent(tags1);
        taskModel.tags = tags1.ToArray();
        taskModel.modifiedTime = new DateTime?(DateTime.Now);
        tagExt.AddTaskTagsPair(taskModel.id, tags1);
      }
      await TaskService.BatchSaveTask(selectedTasks, CheckMatchedType.CheckTag);
      TaskChangeNotifier.NotifyTaskTagsChanged(tagExt);
      tagExt = (TagExtra) null;
    }

    public static async Task BatchSetAssignee(List<string> taskIds, string assignee)
    {
      List<TaskModel> selectedTasks = await TaskService.GetSelectedTasks(taskIds);
      for (int index = 0; index < selectedTasks.Count; ++index)
      {
        TaskModel taskModel = selectedTasks[index];
        taskModel.assignee = assignee;
        taskModel.modifiedTime = new DateTime?(DateTime.Now);
      }
      await TaskService.BatchSaveTask(selectedTasks, CheckMatchedType.CheckAssign);
      TaskChangeNotifier.NotifyTaskAssigneeChanged(taskIds, assignee);
    }

    public static async Task<List<TaskModel>> BatchAddTasks(
      List<string> titles,
      string projectId,
      TimeData time = null,
      int priority = 0,
      List<string> tags = null,
      int status = 0,
      string assigneeId = "-1",
      string columnId = "",
      string parentId = null,
      bool isNote = false,
      DateTime? defaultDate = null,
      bool isPin = false,
      bool? addTop = null,
      long? targetSortOrder = null)
    {
      string str1 = Utils.GetCurrentUserIdInt().ToString();
      string localTimeZone = Utils.GetLocalTimeZone();
      List<TaskModel> models = new List<TaskModel>();
      List<TaskReminderModel> reminders = new List<TaskReminderModel>();
      bool flag = false;
      if (addTop.HasValue)
      {
        flag = addTop.Value;
      }
      else
      {
        TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
        if ((defaultSafely != null ? (defaultSafely.AddTo == 0 ? 1 : 0) : 0) != 0)
          flag = true;
      }
      if (titles == null || !titles.Any<string>())
        return (List<TaskModel>) null;
      List<ProjectModel> projects = CacheManager.GetProjects();
      List<ProjectModel> list1 = projects != null ? projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsEnable())).ToList<ProjectModel>() : (List<ProjectModel>) null;
      Dictionary<string, int> dictionary = new Dictionary<string, int>()
      {
        {
          Utils.GetString("PriorityHigh"),
          5
        },
        {
          Utils.GetString("PriorityMedium"),
          3
        },
        {
          Utils.GetString("PriorityLow"),
          1
        },
        {
          Utils.GetString("PriorityNull"),
          0
        }
      };
      for (int index = 0; index < titles.Count; ++index)
      {
        string projectId1 = projectId;
        long num1 = !flag ? 268435456L * (long) (index + 1) : (long) (titles.Count + 1 - index) * 268435456L * -1L;
        string title = titles[index]?.Replace("\n", " ").Replace("\r", " ").TrimEnd();
        if (!string.IsNullOrEmpty(title))
        {
          if (list1 != null && list1.Any<ProjectModel>())
          {
            List<string> list2 = list1.Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.name)).ToList<string>();
            string existName = TaskTitleUtils.GetLastMatchName(ref title, list2, "^", "~");
            if (existName != null)
              projectId1 = list1.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.name == existName))?.id;
          }
          if (string.IsNullOrEmpty(projectId1))
            projectId1 = TaskDefaultDao.GetDefaultSafely().ProjectId;
          long num2 = targetSortOrder ?? ProjectSortOrderDao.GetNewTaskSortOrderInProject(projectId1, parentId, new bool?(flag));
          List<AvatarViewModel> avatarsFromCache = AvatarHelper.GetProjectAvatarsFromCache(projectId1);
          string lastMatchName = isNote ? (string) null : TaskTitleUtils.GetLastMatchName(ref title, dictionary.Keys.ToList<string>(), "!", "！");
          int num3 = priority;
          if (lastMatchName != null)
            num3 = dictionary[lastMatchName];
          string a = assigneeId;
          if (avatarsFromCache != null && avatarsFromCache.Any<AvatarViewModel>())
          {
            List<string> list3 = avatarsFromCache.Select<AvatarViewModel, string>((Func<AvatarViewModel, string>) (u => u.Name)).ToList<string>();
            string existName = TaskTitleUtils.GetLastMatchName(ref title, list3, "@", "@");
            if (existName != null)
            {
              AvatarViewModel avatarViewModel = avatarsFromCache.FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (u => u.Name == existName));
              if (avatarViewModel != null)
                a = avatarViewModel.UserId;
            }
          }
          string str2 = title;
          List<string> tags1 = tags == null ? new List<string>() : tags.ToList<string>();
          List<string> list4 = ((IEnumerable<string>) title.Split(' ')).ToList<string>();
          if (list4.Count > 0)
          {
            List<string> items = new List<string>();
            foreach (string str3 in list4)
            {
              if (str3.Length > 1 && str3.Length <= 65 && (str3.StartsWith("#") || str3.StartsWith("＃")))
              {
                string exp = str3.Substring(1);
                if (NameUtils.IsValidName(exp))
                  tags1.Add(exp);
              }
              else
                items.Add(str3);
            }
            title = items.Join<string>(" ");
            if (!LocalSettings.Settings.KeepTagsInText)
              str2 = title;
          }
          List<string> source = new List<string>();
          TimeData timeData1 = time;
          if (LocalSettings.Settings.DateParsing)
          {
            IPaserDueDate paserDueDate = ticktick_WPF.Util.DateParser.DateParser.Parse(title, new DateTime?());
            if (paserDueDate != null)
            {
              source = paserDueDate.GetRecognizeStrings();
              TimeData timeData2 = paserDueDate.ToTimeData(!string.IsNullOrEmpty(title) && title.Contains(Utils.GetString("reminder")));
              if (timeData2 != null && !Utils.IsEmptyDate(timeData2.StartDate))
              {
                timeData1 = timeData2;
                if (time != null)
                  timeData1.TimeZone = time.TimeZone;
              }
            }
          }
          // ISSUE: explicit non-virtual call
          TaskModel model = new TaskModel()
          {
            id = Utils.GetGuid(),
            projectId = projectId1,
            title = str2.Trim(),
            userId = str1,
            timeZone = localTimeZone,
            sortOrder = num2 + num1,
            tag = tags1 == null || __nonvirtual (tags1.Count) <= 0 ? (string) null : TagSerializer.ToJsonContent(tags1),
            tags = tags1?.ToArray(),
            priority = num3,
            kind = isNote ? "NOTE" : "TEXT",
            status = status,
            columnId = columnId,
            parentId = parentId,
            createdTime = new DateTime?(DateTime.Now),
            creator = LocalSettings.Settings.LoginUserId
          };
          if (!string.Equals(a, "-1"))
            model.assignee = a;
          if (status != 0)
            model.completedTime = new DateTime?(DateTime.Now);
          if (timeData1 != null)
          {
            model.startDate = timeData1.StartDate;
            model.dueDate = isNote ? new DateTime?() : timeData1.DueDate;
            model.isAllDay = timeData1.IsAllDay;
            model.repeatFrom = timeData1.RepeatFrom;
            model.repeatFlag = timeData1.RepeatFlag;
            TaskModel taskModel = model;
            TimeZoneViewModel timeZone = timeData1.TimeZone;
            bool? nullable = new bool?(timeZone != null && timeZone.IsFloat);
            taskModel.isFloating = nullable;
            model.timeZone = timeData1.TimeZone?.TimeZoneName ?? TimeZoneData.LocalTimeZoneModel.TimeZoneName;
            if (timeData1.Reminders != null && timeData1.Reminders.Any<TaskReminderModel>())
              reminders.AddRange(timeData1.Reminders.Select<TaskReminderModel, TaskReminderModel>((Func<TaskReminderModel, TaskReminderModel>) (reminder => new TaskReminderModel()
              {
                id = Utils.GetGuid(),
                taskserverid = model.id,
                trigger = reminder.trigger
              })));
            if (LocalSettings.Settings.RemoveTimeText && source.Any<string>())
            {
              foreach (string oldValue in source)
                model.title = model.title.Replace(oldValue, string.Empty);
            }
          }
          models.Add(model);
        }
      }
      if (isPin)
      {
        for (int index = models.Count - 1; index >= 0; --index)
          models[index].pinnedTimeStamp = Utils.GetNowTimeStamp() - (long) index;
      }
      await TagService.CheckTaskTags(models);
      await TaskDao.BatchInsertTasks(models);
      if (reminders.Any<TaskReminderModel>())
        await TaskReminderDao.BatchAddTaskReminders((IEnumerable<TaskReminderModel>) reminders);
      List<string> ids = models.Select<TaskModel, string>((Func<TaskModel, string>) (model => model.id)).ToList<string>();
      await SyncStatusDao.BatchAddCreateTaskStatus((IEnumerable<string>) ids);
      if (!string.IsNullOrEmpty(parentId))
      {
        TaskDao.AddOrRemoveTaskChildIds(parentId, ids, true);
        await SyncStatusDao.BatchAddTaskParentStatus(ids);
      }
      return models;
    }

    public static async Task BatchClearDate(List<string> taskIds)
    {
      List<TaskModel> tasks = await TaskService.GetSelectedTasks(taskIds);
      for (int i = 0; i < tasks.Count; ++i)
      {
        TaskModel task = tasks[i];
        task.isAllDay = new bool?(true);
        task.startDate = new DateTime?();
        task.dueDate = new DateTime?();
        task.reminders = (TaskReminderModel[]) null;
        task.repeatFrom = string.Empty;
        task.repeatFlag = string.Empty;
        task.remindTime = new DateTime?();
        task.modifiedTime = new DateTime?(DateTime.Now);
        task.isFloating = new bool?(false);
        task.timeZone = TimeZoneData.LocalTimeZoneModel.TimeZoneName;
        int num = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
        await ReminderDelayDao.DeleteByIdAsync(task.id, "task");
        task = (TaskModel) null;
      }
      TaskChangeNotifier.NotifyBatchDateChanged(taskIds);
      await TaskService.BatchSaveTask(tasks, CheckMatchedType.CheckSmart);
      tasks = (List<TaskModel>) null;
    }

    public static async Task<bool> BatchDeleteTaskByIds(
      List<string> selectedTaskIds,
      bool showUndo = true,
      int status = 1)
    {
      List<TaskModel> tasks;
      if (status == 1)
        tasks = await TaskDao.GetTaskAndChildrenInBatch(selectedTaskIds);
      else
        tasks = (await TaskService.GetSelectedTasks(selectedTaskIds)).Where<TaskModel>((Func<TaskModel, bool>) (t => t.CheckEnable(true))).ToList<TaskModel>();
      return await TaskService.BatchDeleteTasks(tasks, showUndo, status);
    }

    public static async Task<bool> BatchDeleteTasks(
      List<TaskModel> tasks,
      bool showUndo = true,
      int status = 1,
      Grid undoGrid = null)
    {
      if (tasks.Count >= 50 && status == 1 && !new CustomerDialog(Utils.GetString("DeleteTask"), string.Format(Utils.GetString("BatchDeleteConfirm"), (object) tasks.Count.ToString()), Utils.GetString("Delete"), Utils.GetString("Cancel")).ShowDialog().GetValueOrDefault())
        return false;
      List<string> ids = new List<string>();
      List<string> deleteForever = new List<string>();
      for (int index = 0; index < tasks.Count; ++index)
      {
        TaskModel task = tasks[index];
        if (TaskService.IsEmptyTask(task))
        {
          deleteForever.Add(task.id);
          task.deleted = 2;
        }
        else
        {
          ids.Add(task.id);
          task.deleted = status;
        }
        task.modifiedTime = new DateTime?(DateTime.Now);
      }
      await TaskDao.BatchUpdateTasks(tasks, checkMatched: CheckMatchedType.All);
      if (status == 1)
      {
        if (!showUndo)
        {
          TaskChangeNotifier.NotifyTaskBatchDeletedChanged(ids);
          TaskChangeNotifier.NotifyTaskBatchDeletedChanged(deleteForever);
          await SyncStatusDao.BatchAddDeleteSyncStatus((IEnumerable<string>) ids);
          await SyncStatusDao.BatchAddDeleteForeverSyncStatus((IEnumerable<string>) deleteForever);
          SyncManager.TryDelaySync();
        }
        else
        {
          UndoToast uiElement = new UndoToast();
          if (ids.Count + deleteForever.Count == 1)
          {
            TaskModel taskModel = tasks.FirstOrDefault<TaskModel>();
            if (taskModel != null)
              uiElement.InitTaskUndo(taskModel.id, taskModel.title, taskModel.deleted == 2);
          }
          else
            uiElement.InitBatchTaskUndo((IEnumerable<string>) ids, (IEnumerable<string>) deleteForever);
          if (undoGrid == null)
          {
            undoGrid = App.Window.ToastGrid;
            App.Window.TryFocus();
          }
          WindowToastHelper.ShowAndHideToast(undoGrid, (FrameworkElement) uiElement);
        }
        UtilLog.Info("TaskBatchService.BatchDeleteTasks : " + ids.Join<string>(";"));
      }
      else
      {
        UtilLog.Info("TaskBatchService.BatchDeleteTasksForever : " + ids.Join<string>(";"));
        await SyncStatusDao.BatchAddDeleteForeverSyncStatus((IEnumerable<string>) ids);
        await SyncStatusDao.BatchAddDeleteForeverSyncStatus((IEnumerable<string>) deleteForever);
        TaskChangeNotifier.NotifyTaskBatchDeletedChanged(ids);
        SyncManager.TryDelaySync();
      }
      return true;
    }

    public static async Task BatchDeleteTaskInColumn(
      string projectId,
      string columnId,
      string primaryColumnId)
    {
      int num = await TaskService.BatchDeleteTasks(await TaskDao.GetTasksInProjectOrColumn(projectId, columnId, primaryColumnId == columnId), false) ? 1 : 0;
    }

    public static async Task BatchDeleteTaskInCustomSection(
      string projectId,
      string columnId,
      string primaryColumnId)
    {
      List<TaskModel> inProjectOrColumn = await TaskDao.GetTasksInProjectOrColumn(projectId, columnId, primaryColumnId == columnId);
      List<TaskModel> notDeletedTasks = inProjectOrColumn.Where<TaskModel>((Func<TaskModel, bool>) (t => t.pinnedTimeStamp > 0L || t.status != 0)).ToList<TaskModel>();
      HashSet<string> notDeletedTasksIds = new HashSet<string>(notDeletedTasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
      List<TaskModel> deletedTasks = new List<TaskModel>();
      foreach (TaskModel task in inProjectOrColumn)
      {
        if (notDeletedTasksIds.Contains(task.id))
        {
          if (!string.IsNullOrEmpty(task.parentId) && !notDeletedTasksIds.Contains(task.parentId))
          {
            task.parentId = string.Empty;
            await TaskDao.UpdateParent(task.id, string.Empty);
            UtilLog.Info("SetTaskParent empty " + task.id + " DeleteSection");
          }
          task.columnId = string.Empty;
        }
        else
          deletedTasks.Add(task);
      }
      await TaskDao.BatchUpdateTasks(notDeletedTasks, checkMatched: CheckMatchedType.CheckSmart);
      int num = await TaskService.BatchDeleteTasks(deletedTasks, false) ? 1 : 0;
      notDeletedTasks = (List<TaskModel>) null;
      notDeletedTasksIds = (HashSet<string>) null;
      deletedTasks = (List<TaskModel>) null;
    }

    public static async Task BatchClearTrash(List<string> trashIds)
    {
      List<TaskModel> selectedTasks = await TaskService.GetSelectedTasks(trashIds);
      for (int index = 0; index < selectedTasks.Count; ++index)
      {
        TaskModel taskModel = selectedTasks[index];
        taskModel.deleted = 2;
        taskModel.modifiedTime = new DateTime?(DateTime.Now);
      }
      await TaskDao.BatchUpdateTasks(selectedTasks);
      await SyncStatusDao.AddClearTrashSyncStatus();
      SyncManager.TryDelaySync();
    }

    public static async Task BatchMoveParentTaskProject(
      List<string> taskIds,
      ProjectModel project,
      bool keepAssignee = false)
    {
      List<TaskBaseViewModel> andChildrenInBatch = TaskCache.GetTaskAndChildrenInBatch(taskIds);
      // ISSUE: explicit non-virtual call
      if (andChildrenInBatch == null || __nonvirtual (andChildrenInBatch.Count) <= 0)
        return;
      await TaskService.BatchMoveProject(andChildrenInBatch.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>(), new MoveProjectArgs((string) null, project, keepAssignee)
      {
        Notify = false
      });
    }

    public static async Task BatchRestoreProject(List<string> taskIds)
    {
      List<string> ids = new List<string>((IEnumerable<string>) taskIds);
      ids = TaskCache.GetAllTaskAndSubTaskIdsByIds(ids);
      List<TaskModel> tasks = await TaskService.GetSelectedTasks(ids);
      IEnumerable<string> strings = tasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.projectId)).Distinct<string>();
      Dictionary<string, ProjectModel> prjs = new Dictionary<string, ProjectModel>();
      foreach (string id in strings)
      {
        Dictionary<string, ProjectModel> dictionary = prjs;
        string key = id;
        dictionary[key] = await ProjectDao.GetProjectById(id);
        dictionary = (Dictionary<string, ProjectModel>) null;
        key = (string) null;
      }
      string inbox = Utils.GetInboxId();
      foreach (TaskModel task in tasks)
      {
        task.modifiedTime = new DateTime?(DateTime.Now);
        task.deleted = 0;
        task.assignee = "-1";
        string projectId = task.projectId;
        if (prjs.ContainsKey(task.projectId) && prjs[task.projectId] == null)
          task.projectId = inbox;
        await SyncStatusDao.AddMoveOrRestoreProjectStatus(task.id, projectId, true);
        CommentDao.ChangeCommentProjectId(task, task.projectId);
      }
      await TaskDao.BatchUpdateTasks(tasks, checkMatched: CheckMatchedType.CheckProject);
      TaskChangeNotifier.NotifyTaskBatchDeletedChanged(ids);
      SyncManager.TryDelaySync();
      if (ids.Count == 1)
      {
        IToastShowWindow toastWindow = Utils.GetToastWindow();
        if (toastWindow == null)
        {
          ids = (List<string>) null;
          tasks = (List<TaskModel>) null;
          prjs = (Dictionary<string, ProjectModel>) null;
          inbox = (string) null;
        }
        else
        {
          toastWindow.ToastMoveProjectControl(prjs.Values.FirstOrDefault<ProjectModel>()?.id, moveType: MoveToastType.Restore);
          ids = (List<string>) null;
          tasks = (List<TaskModel>) null;
          prjs = (Dictionary<string, ProjectModel>) null;
          inbox = (string) null;
        }
      }
      else
      {
        Utils.Toast(Utils.GetString("TaskHasBeenRestoreToOrigin"));
        ids = (List<string>) null;
        tasks = (List<TaskModel>) null;
        prjs = (Dictionary<string, ProjectModel>) null;
        inbox = (string) null;
      }
    }

    public static async Task CheckColumnEmptyTask(string projectId)
    {
      List<TaskModel> tasksInProjectAsync = await TaskDao.GetTasksInProjectAsync(projectId);
      List<TaskModel> tasks = tasksInProjectAsync != null ? tasksInProjectAsync.Where<TaskModel>((Func<TaskModel, bool>) (t => string.IsNullOrEmpty(t.columnId))).ToList<TaskModel>() : (List<TaskModel>) null;
      if (tasks == null)
        tasks = (List<TaskModel>) null;
      else if (!tasks.Any<TaskModel>())
      {
        tasks = (List<TaskModel>) null;
      }
      else
      {
        string defaultColumnId = await ColumnDao.GetProjectDefaultColumnId(projectId);
        UtilLog.Info("ColumnEmptyTasksCount projectId " + projectId + " count " + tasks.Count.ToString());
        if (!string.IsNullOrEmpty(defaultColumnId))
        {
          foreach (TaskModel task in tasks)
          {
            task.columnId = defaultColumnId;
            task.modifiedTime = new DateTime?(DateTime.Now);
            await TaskService.UpdateTaskColumnId(task);
            await SyncStatusDao.AddModifySyncStatus(task.id);
          }
        }
        defaultColumnId = (string) null;
        tasks = (List<TaskModel>) null;
      }
    }

    public static async Task BatchMoveProject(List<string> taskIds, MoveProjectArgs args)
    {
      List<string> ids = new List<string>((IEnumerable<string>) taskIds);
      TaskDefaultModel model;
      List<TaskModel> tasks;
      if (await ProChecker.CheckTaskLimit(args.Project.id, ids.Count))
      {
        ids = (List<string>) null;
        model = (TaskDefaultModel) null;
        tasks = (List<TaskModel>) null;
      }
      else
      {
        long newSortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(args.Project.id);
        model = TaskDefaultDao.GetDefaultSafely();
        tasks = await TaskService.GetSelectedTasks(ids);
        bool changed = false;
        for (int i = 0; i < tasks.Count; ++i)
        {
          TaskModel task = tasks[i];
          bool flag1 = task.projectId == args.Project.id;
          bool flag2 = task.columnId == args.ColumnId || string.IsNullOrEmpty(args.ColumnId);
          if (!(flag1 & flag2))
          {
            changed = true;
            string projectId = task.projectId;
            task.projectId = args.Project.id;
            task.columnId = args.ColumnId;
            task.modifiedTime = new DateTime?(DateTime.Now);
            task.deleted = 0;
            string assignee = task.assignee;
            long sortOrder = task.sortOrder;
            if (!args.KeepAssignee)
              task.assignee = "-1";
            task.sortOrder = model.AddTo != 0 ? newSortOrder + (long) i : newSortOrder - (long) (tasks.Count - i);
            if (!flag1)
            {
              if (!string.IsNullOrEmpty(args.UndoId))
              {
                TaskDragUndoModel.AddDragModel(args.UndoId, task.id, (string) null, (string) null, projectId, args.Project.id, assignee, sortOrder);
              }
              else
              {
                await SyncStatusDao.AddMoveOrRestoreProjectStatus(task.id, projectId);
                await SyncStatusDao.AddModifySyncStatus(task.id);
              }
              CommentDao.ChangeCommentProjectId(task, args.Project.id);
              await TaskService.UpdateTaskOnProjectChanged(task, CheckMatchedType.None);
            }
            else
              await TaskService.UpdateTaskColumnId(task);
            task = (TaskModel) null;
          }
        }
        if (!changed)
        {
          ids = (List<string>) null;
          model = (TaskDefaultModel) null;
          tasks = (List<TaskModel>) null;
        }
        else
        {
          ProjectAndTaskIdsCache.OnTasksChanged(TaskCache.GetTasksByIds(ids), CheckMatchedType.CheckProject);
          if (!args.Notify)
          {
            ids = (List<string>) null;
            model = (TaskDefaultModel) null;
            tasks = (List<TaskModel>) null;
          }
          else
          {
            TaskChangeNotifier.NotifyTaskBatchChanged(ids);
            SyncManager.TryDelaySync();
            ids = (List<string>) null;
            model = (TaskDefaultModel) null;
            tasks = (List<TaskModel>) null;
          }
        }
      }
    }

    private static async Task BatchSaveTask(List<TaskModel> tasks, CheckMatchedType checkType = CheckMatchedType.None)
    {
      await TaskDao.BatchUpdateTasks(tasks, checkMatched: checkType);
      await SyncStatusDao.BatchAddModifySyncStatus(tasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
    }

    internal static async Task<bool> BatchUpdateTaskQuadrantProperties(
      List<string> taskIds,
      string previewRule,
      string nextRule,
      IToastShowWindow toastShowWindow)
    {
      List<TaskBaseViewModel> tasksViewModels = TaskCache.GetTasksByIds(taskIds);
      NormalFilterViewModel nextFilter = ticktick_WPF.Util.Filter.Parser.ToNormalModel(nextRule);
      List<string> matchedTaskIds = TaskService.GetTaskMatchedNormalFilter(nextFilter, tasksViewModels, true).Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>();
      tasksViewModels.RemoveAll((Predicate<TaskBaseViewModel>) (t => matchedTaskIds.Contains(t.Id)));
      if (tasksViewModels.Count == 0 && matchedTaskIds.Count > 0)
      {
        toastShowWindow?.TryToastString((object) null, Utils.GetString("TaskExisted"));
        return false;
      }
      NormalFilterViewModel previewFilter = ticktick_WPF.Util.Filter.Parser.ToNormalModel(previewRule);
      FilterViewModel.GetDefaultInNormal(previewFilter, new FilterTaskDefault());
      FilterTaskDefault nextDefault = new FilterTaskDefault();
      FilterViewModel.GetDefaultInNormal(nextFilter, nextDefault);
      if (nextFilter.Priorities.Any<int>() && (nextFilter.TaskTypes.Count == 1 && nextFilter.TaskTypes[0].ToLower() == "note" || tasksViewModels.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.IsNote)) && nextFilter.TaskTypes.Count != 1))
      {
        toastShowWindow?.TryToastString((object) null, Utils.GetString("NoteMoveFailed"));
        return false;
      }
      if (nextFilter.TaskTypes.Count == 1 && nextFilter.TaskTypes[0].ToLower() == "note" && tasksViewModels.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Status != 0 || TaskCache.IsParentTask(t.Id) || TaskCache.IsChildTask(t.ParentId, t.ProjectId))))
      {
        toastShowWindow?.TryToastString((object) null, Utils.GetString("CannotConvertMultiLevelToNote"));
        return false;
      }
      if (nextFilter.Projects.Any<string>() || nextFilter.Groups.Any<string>())
      {
        List<string> topIds = TaskDao.GetTreeTopIds(taskIds, (string) null);
        foreach (TaskBaseViewModel taskBaseViewModel in tasksViewModels)
        {
          ProjectModel projectById = CacheManager.GetProjectById(taskBaseViewModel.ProjectId);
          if (projectById != null)
          {
            List<string> projects = nextFilter.Projects;
            // ISSUE: explicit non-virtual call
            if ((projects != null ? (__nonvirtual (projects.Contains(projectById.id)) ? 1 : 0) : 0) == 0)
            {
              List<string> groups = nextFilter.Groups;
              // ISSUE: explicit non-virtual call
              if ((groups != null ? (__nonvirtual (groups.Contains(projectById.groupId)) ? 1 : 0) : 0) == 0 && topIds.Contains(taskBaseViewModel.Id) && taskBaseViewModel.ProjectId != nextDefault.ProjectModel.id)
                await TaskService.TryMoveProject(new MoveProjectArgs(taskBaseViewModel.Id, nextDefault.ProjectModel));
            }
          }
        }
        topIds = (List<string>) null;
      }
      List<TaskModel> tasks = new List<TaskModel>();
      string toastSwitchKind = string.Empty;
      foreach (TaskBaseViewModel taskBaseViewModel in tasksViewModels)
      {
        TaskBaseViewModel vm = taskBaseViewModel;
        TaskModel taskById = await TaskDao.GetTaskById(vm.Id);
        if (taskById != null)
        {
          tasks.Add(taskById);
          if (nextFilter.DueDates.Any<string>((Func<string, bool>) (dateRule => dateRule != "recurring")) && !TaskUtils.CheckDate(vm, FilterUtils.GetFilterDatePairs((IEnumerable<string>) nextFilter.DueDates)))
          {
            TimeData timeData = new TimeData();
            DateTime? defaultDate = nextDefault.DefaultDate;
            if (defaultDate.HasValue)
            {
              timeData = taskById.GetTimeData();
              ref TimeData local = ref timeData;
              defaultDate = nextDefault.DefaultDate;
              DateTime changeDate = defaultDate.Value;
              TaskService.ChangeStartDate(ref local, changeDate);
            }
            taskById.startDate = timeData.StartDate;
            taskById.dueDate = timeData.DueDate;
            taskById.repeatFlag = timeData.RepeatFlag;
            taskById.repeatFrom = timeData.RepeatFrom;
            taskById.isAllDay = timeData.IsAllDay;
          }
          if (nextFilter.Priorities.Any<int>() && !nextFilter.Priorities.Contains(taskById.priority))
            taskById.priority = nextDefault.Priority ?? TaskDefaultDao.GetDefaultSafely().Priority;
          if (nextFilter.TaskTypes.Count == 1)
          {
            string str = nextFilter.TaskTypes[0].ToLower() == "note" ? "NOTE" : "TEXT";
            if (taskById.kind == "NOTE" != (str == "NOTE"))
            {
              taskById.kind = str;
              if (string.IsNullOrEmpty(toastSwitchKind))
                toastSwitchKind = Utils.GetString(str == "NOTE" ? "ConvertedtoNote" : "ConvertedtoTask");
            }
          }
          if (nextFilter.Tags.Any<string>())
          {
            List<string> tags = TagSerializer.ToTags(taskById.tag);
            if (previewFilter.Tags.Any<string>())
            {
              if (previewFilter.Tags.Contains("*withtags"))
              {
                tags.Clear();
              }
              else
              {
                foreach (string tag1 in previewFilter.Tags)
                {
                  string tag = tag1;
                  tags.RemoveAll((Predicate<string>) (t => string.Equals(t, tag, StringComparison.CurrentCultureIgnoreCase)));
                }
              }
            }
            if (!nextFilter.Tags.Contains("*withtags") && nextFilter.Tags.Contains("!tag"))
              tags.Clear();
            foreach (string defaultTag in nextDefault.DefaultTags)
            {
              string tagName = defaultTag;
              if (tagName.StartsWith("#"))
                tagName = tagName.Substring(1);
              if (!tags.Any<string>((Func<string, bool>) (t => string.Equals(t, tagName, StringComparison.CurrentCultureIgnoreCase))))
                tags.Add(tagName);
            }
            taskById.tag = TagSerializer.ToJsonContent(tags);
          }
          vm = (TaskBaseViewModel) null;
        }
      }
      if (!string.IsNullOrEmpty(toastSwitchKind))
        toastShowWindow?.TryToastString((object) null, toastSwitchKind);
      List<string> changedIds = tasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>();
      await TaskDao.BatchUpdateTasks(tasks, checkMatched: CheckMatchedType.All);
      await SyncStatusDao.BatchAddModifySyncStatus((IEnumerable<string>) changedIds);
      TaskChangeNotifier.NotifyTaskBatchChanged(changedIds);
      SyncManager.TryDelaySync();
      return true;
    }

    public static async Task BatchSkipRecurrence(List<string> taskIds)
    {
      if (taskIds != null && taskIds.Any<string>())
      {
        foreach (string taskId in taskIds)
        {
          TaskModel taskModel = await TaskService.SkipCurrentRecurrence(taskId, false);
        }
      }
      TaskChangeNotifier.NotifyBatchDateChanged(taskIds);
    }

    public static async Task BatchSetStartDate(List<string> selectedTaskIds, DateTime date)
    {
      List<TaskModel> taskModels = new List<TaskModel>();
      for (int i = 0; i < selectedTaskIds.Count; ++i)
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(selectedTaskIds[i]);
        bool noDateTask = thinTaskById == null || !thinTaskById.startDate.HasValue;
        if (thinTaskById != null)
        {
          TaskModel task = await TaskService.ModifyTaskDate(thinTaskById, date);
          taskModels.Add(task);
          if (noDateTask && task.isAllDay.HasValue && task.isAllDay.Value)
          {
            task.reminders = TimeData.GetDefaultAllDayReminders().ToArray();
            task.exDates = string.Empty;
            await TaskService.SaveTaskReminders(task);
          }
        }
      }
      await TaskService.BatchSaveTask(taskModels, CheckMatchedType.CheckSmart);
      TaskChangeNotifier.NotifyBatchDateChanged(selectedTaskIds);
      ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(new TimeData()
      {
        StartDate = new DateTime?(date)
      });
      taskModels = (List<TaskModel>) null;
    }

    public static async Task UndoBatchDeletedTasks(List<string> taskIds)
    {
      List<TaskModel> selectedTasks = await TaskService.GetSelectedTasks(taskIds);
      if (selectedTasks == null || !selectedTasks.Any<TaskModel>())
        return;
      selectedTasks.ForEach((Action<TaskModel>) (task =>
      {
        task.deleted = 0;
        task.modifiedTime = new DateTime?(DateTime.Now);
      }));
      UtilLog.Info("TaskBatchService.UndoBatchDeletedTasks : " + taskIds.Count.ToString());
      await TaskDao.BatchUpdateTasks(selectedTasks, checkMatched: CheckMatchedType.All);
      await SyncStatusDao.BatchRemoveDeleteSyncStatus(taskIds);
      TaskChangeNotifier.NotifyBatchDeleteUndo(taskIds);
    }

    public static async Task<bool> BatchSetPriority(List<string> taskIds, int priority)
    {
      List<TaskModel> selectedTasks = await TaskService.GetSelectedTasks(taskIds);
      if (selectedTasks.Count == 0)
        return false;
      for (int index = 0; index < selectedTasks.Count; ++index)
      {
        TaskModel taskModel = selectedTasks[index];
        if (taskModel.kind != "NOTE")
        {
          taskModel.priority = priority;
          taskModel.modifiedTime = new DateTime?(DateTime.Now);
        }
      }
      await TaskService.BatchSaveTask(selectedTasks, CheckMatchedType.OnlyFilter);
      TaskChangeNotifier.NotifyTaskPriorityChanged((string) null, priority, taskIds);
      return true;
    }

    public static async Task<int> BatchSetCheckItemToToday(List<string> itemIds)
    {
      if (itemIds == null || itemIds.Count <= 0)
        return 0;
      List<TaskDetailItemModel> items = new List<TaskDetailItemModel>();
      List<string> taskList = new List<string>();
      DateTime today = DateTime.Today;
      foreach (string itemId in itemIds)
      {
        TaskDetailItemModel checklistItemById = await TaskDetailItemDao.GetChecklistItemById(itemId);
        if (checklistItemById != null)
        {
          DateTime? startDate = checklistItemById.startDate;
          if (startDate.HasValue)
          {
            TaskDetailItemModel taskDetailItemModel = checklistItemById;
            int year = today.Year;
            int month = today.Month;
            int day = today.Day;
            startDate = checklistItemById.startDate;
            DateTime dateTime = startDate.Value;
            int hour = dateTime.Hour;
            startDate = checklistItemById.startDate;
            dateTime = startDate.Value;
            int minute = dateTime.Minute;
            startDate = checklistItemById.startDate;
            dateTime = startDate.Value;
            int second = dateTime.Second;
            DateTime? nullable = new DateTime?(new DateTime(year, month, day, hour, minute, second));
            taskDetailItemModel.startDate = nullable;
            items.Add(checklistItemById);
            taskList.Add(checklistItemById.TaskServerId);
          }
        }
      }
      if (items.Any<TaskDetailItemModel>())
        await TaskDetailItemDao.BatchUpdateChecklists(items);
      if (taskList.Any<string>())
        await SyncStatusDao.BatchAddModifySyncStatus(taskList.Distinct<string>());
      return items.Count;
    }

    public static async Task BatchSetColumn(List<string> taskIds, string columnId)
    {
      foreach (TaskBaseViewModel taskBaseViewModel in TaskCache.GetTaskAndChildrenInBatch(taskIds))
      {
        TaskModel task = await TaskDao.GetTaskById(taskBaseViewModel.Id);
        if (task != null)
        {
          task.columnId = columnId;
          task.modifiedTime = new DateTime?(DateTime.Now);
          await TaskService.UpdateTaskColumnId(task);
          await SyncStatusDao.AddModifySyncStatus(task.id);
          task = (TaskModel) null;
        }
      }
    }

    public static async Task BatchSetCompleteDate(List<string> taskIds, DateTime date)
    {
      List<TaskModel> selectedTasks = await TaskService.GetSelectedTasks(taskIds);
      for (int index = 0; index < selectedTasks.Count; ++index)
      {
        TaskModel taskModel = selectedTasks[index];
        if (taskModel.status != 0)
        {
          taskModel.completedTime = new DateTime?(date);
          taskModel.modifiedTime = new DateTime?(DateTime.Now);
        }
      }
      await TaskService.BatchSaveTask(selectedTasks);
    }

    public static async Task BatchStarTaskOrNote(List<string> ids, string project, bool pin)
    {
      foreach (TaskModel taskModel in await TaskDao.GetThinTasksInBatch(ids))
      {
        if (taskModel.CheckEnable())
          await TaskService.TogglesStarred(taskModel.id, project ?? taskModel.projectId, new bool?(pin), notify: false);
      }
      TaskChangeNotifier.NotifyBatchTaskStarredChanged(new TaskPinExtra()
      {
        Ids = ids,
        IsPin = pin
      });
    }

    public static async Task BatchSwitchTaskOrNote(List<string> ids)
    {
      List<TaskModel> thinTasksInBatch = await TaskDao.GetThinTasksInBatch(ids);
      thinTasksInBatch.Any<TaskModel>((Func<TaskModel, bool>) (t => t.kind == "NOTE"));
      foreach (TaskModel task in thinTasksInBatch)
      {
        if (task.kind == "NOTE")
        {
          task.kind = "TEXT";
        }
        else
        {
          if (task.kind == "CHECKLIST")
          {
            task.content = ChecklistUtils.Items2Text(task.desc, (await TaskDetailItemDao.GetCheckItemsByTaskId(task.id)).OrderBy<TaskDetailItemModel, long>((Func<TaskDetailItemModel, long>) (c => c.sortOrder)).Select<TaskDetailItemModel, string>((Func<TaskDetailItemModel, string>) (s => s.title)).ToList<string>());
            TaskModel taskModel = task;
            taskModel.content = await AttachmentDao.AddAttachmentStrings(task.id, task.content);
            taskModel = (TaskModel) null;
            task.desc = "";
          }
          task.dueDate = new DateTime?();
          task.kind = "NOTE";
          task.status = 0;
          task.completedTime = new DateTime?();
          task.priority = 0;
          task.progress = new int?(0);
          task.repeatFlag = (string) null;
          task.repeatFrom = (string) null;
        }
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTask(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
      }
      TaskChangeNotifier.NotifyTaskKindChanged(ids);
    }

    public static async Task<bool> BatchSetDate(List<string> taskIds, DateTime? date)
    {
      List<string> stringList = taskIds;
      // ISSUE: explicit non-virtual call
      if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) == 0)
        return false;
      if (date.HasValue)
        await TaskService.BatchSetStartDate(taskIds, date.Value);
      else
        await TaskService.BatchClearDate(taskIds);
      return true;
    }

    public static async Task<bool> MoveColumnAsync(
      string columnId,
      string fromProjectId,
      string toProjectId,
      bool isSection = false)
    {
      ProjectModel toProject = CacheManager.GetProjectById(toProjectId);
      if (toProject == null)
        return false;
      List<ColumnModel> columns = await ColumnDao.CheckProjectColumns(toProjectId);
      List<TaskBaseViewModel> tasks = await TaskDao.GetDisplayTasksInColumn(fromProjectId, columnId, columnId == await ColumnDao.GetProjectDefaultColumnId(fromProjectId));
      int? count = columns?.Count;
      long? nullable1 = count.HasValue ? new long?((long) count.GetValueOrDefault()) : new long?();
      long limitByKey = LimitCache.GetLimitByKey(Constants.LimitKind.KanbanNumber);
      if (nullable1.GetValueOrDefault() >= limitByKey & nullable1.HasValue)
      {
        new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ExceedMaxSectionCountMessage"), MessageBoxButton.OK, Utils.GetToastWindow() as Window).ShowDialog();
        return false;
      }
      if (await ProChecker.CheckTaskLimit(toProjectId, tasks.Count))
        return false;
      ColumnModel columnById = await ColumnDao.GetColumnById(columnId);
      if (columnById != null)
      {
        columnById.projectId = toProjectId;
        ColumnModel columnModel = columnById;
        List<ColumnModel> source = columns;
        long? nullable2;
        if (source == null)
        {
          nullable1 = new long?();
          nullable2 = nullable1;
        }
        else
          nullable2 = source.Max<ColumnModel>((Func<ColumnModel, long?>) (c => c.sortOrder));
        nullable1 = nullable2;
        long? nullable3 = new long?(nullable1.GetValueOrDefault() + 268435456L);
        columnModel.sortOrder = nullable3;
        int num = await App.Connection.UpdateAsync((object) columnById);
      }
      tasks = TaskCache.GetTaskAndChildrenInBatch(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>(), projectId: fromProjectId);
      if (isSection)
        tasks.RemoveAll((Predicate<TaskBaseViewModel>) (t => t.IsPinned || t.Status != 0));
      List<string> ids = tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>();
      await TaskService.BatchMoveProject(ids, new MoveProjectArgs(toProject));
      await TaskService.BatchSetColumn(tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.ColumnId != columnId)).Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>(), columnId);
      await SyncStatusDao.AddColumnMoveProjectStatus(columnId, fromProjectId);
      DataChangedNotifier.NotifyColumnChanged(toProjectId);
      DataChangedNotifier.NotifyColumnChanged(fromProjectId);
      TaskChangeNotifier.NotifyTaskBatchChanged(ids);
      return true;
    }

    public static bool IsTaskMatchedFilter(
      NormalFilterViewModel normal,
      TaskBaseViewModel task,
      bool inAll = false)
    {
      NormalFilterViewModel normal1 = normal;
      List<TaskBaseViewModel> tasks = new List<TaskBaseViewModel>();
      tasks.Add(task);
      int num = inAll ? 1 : 0;
      List<TaskBaseViewModel> matchedNormalFilter = TaskService.GetTaskMatchedNormalFilter(normal1, tasks, num != 0);
      return matchedNormalFilter != null && matchedNormalFilter.Any<TaskBaseViewModel>();
    }

    public static List<TaskBaseViewModel> GetTaskMatchedNormalFilter(
      NormalFilterViewModel normal,
      List<TaskBaseViewModel> tasks,
      bool inAll = false,
      bool inCal = false)
    {
      if (normal == null)
        return new List<TaskBaseViewModel>();
      List<FilterDatePair> startEndPairs = (List<FilterDatePair>) null;
      if (normal.DueDates != null && normal.DueDates.Count > 0)
        startEndPairs = FilterUtils.GetFilterDatePairs((IEnumerable<string>) normal.DueDates);
      TaskType taskType = normal.TaskTypes.Count == 1 ? (normal.TaskTypes[0] != "task" ? TaskType.Note : TaskType.Task) : TaskType.TaskAndNote;
      List<string> keywords = normal.Keywords;
      // ISSUE: explicit non-virtual call
      string keyword = (keywords != null ? (__nonvirtual (keywords.Count) > 0 ? 1 : 0) : 0) != 0 ? normal.Keywords[0] : (string) null;
      return TaskViewModelHelper.GetTasks(tasks, normal.Groups, normal.Projects, normal.Tags, startEndPairs, inAll: inAll, assignTo: normal.Assignees, priorities: normal.Priorities, inCal: inCal, taskType: taskType, keyword: keyword).ToList<TaskBaseViewModel>();
    }

    public static List<TaskBaseViewModel> GetTasksMatchedFilter(
      FilterModel filter,
      List<TaskBaseViewModel> tasks,
      bool inCal = false)
    {
      if (filter == null)
        return new List<TaskBaseViewModel>();
      List<TaskBaseViewModel> tasksMatchedFilter;
      if (ticktick_WPF.Util.Filter.Parser.GetFilterRuleType(filter.rule) == 0)
      {
        tasksMatchedFilter = TaskService.GetTaskMatchedNormalFilter(ticktick_WPF.Util.Filter.Parser.ToNormalModel(filter.rule), tasks, inCal);
      }
      else
      {
        AdvancedFilterViewModel advanceModel = ticktick_WPF.Util.Filter.Parser.ToAdvanceModel(filter.rule);
        if (!TaskService.IsAdvanceModelValid(advanceModel.CardList))
          return new List<TaskBaseViewModel>();
        tasksMatchedFilter = TaskService.GetTaskMatchedAdvancedFilter(advanceModel, tasks, inCal);
      }
      return tasksMatchedFilter;
    }

    public static List<TaskBaseViewModel> GetCheckItemsMatchedFilter(
      FilterModel filter,
      List<TaskBaseViewModel> items)
    {
      if (filter == null)
        return new List<TaskBaseViewModel>();
      List<TaskBaseViewModel> itemsMatchedFilter;
      if (ticktick_WPF.Util.Filter.Parser.GetFilterRuleType(filter.rule) == 0)
      {
        itemsMatchedFilter = TaskService.GetItemMatchedNormalFilter(ticktick_WPF.Util.Filter.Parser.ToNormalModel(filter.rule), items);
      }
      else
      {
        AdvancedFilterViewModel advanceModel = ticktick_WPF.Util.Filter.Parser.ToAdvanceModel(filter.rule);
        if (!TaskService.IsAdvanceModelValid(advanceModel.CardList))
          return new List<TaskBaseViewModel>();
        itemsMatchedFilter = TaskService.GetItemsInAdvancedFilter(advanceModel, items);
      }
      return itemsMatchedFilter;
    }

    public static List<TaskBaseViewModel> GetItemMatchedNormalFilter(
      NormalFilterViewModel normal,
      List<TaskBaseViewModel> items)
    {
      if (normal == null)
        return new List<TaskBaseViewModel>();
      List<FilterDatePair> startEndPairs = (List<FilterDatePair>) null;
      if (normal.DueDates != null && normal.DueDates.Count > 0)
        startEndPairs = FilterUtils.GetFilterDatePairs((IEnumerable<string>) normal.DueDates);
      return TaskViewModelHelper.GetItems(items, normal.Groups, normal.Projects, normal.Tags, startEndPairs, assignTo: normal.Assignees, priorities: normal.Priorities).ToList<TaskBaseViewModel>();
    }

    private static List<TaskBaseViewModel> GetItemsInAdvancedFilter(
      AdvancedFilterViewModel advanced,
      List<TaskBaseViewModel> items)
    {
      List<CardViewModel> list1 = advanced.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>();
      List<CardViewModel> list2 = advanced.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.LogicAnd || card.Type == CardType.LogicOr)).ToList<CardViewModel>();
      DateCardViewModel dateCardViewModel = (DateCardViewModel) list1.FirstOrDefault<CardViewModel>((Func<CardViewModel, bool>) (c => c is DateCardViewModel));
      int num = dateCardViewModel?.Values == null ? 0 : (dateCardViewModel.Values.Any<string>((Func<string, bool>) (v => v != "nodue")) ? 1 : (dateCardViewModel.LogicType == LogicType.Not ? 1 : 0));
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      if (num != 0)
      {
        switch (list1.IndexOf((CardViewModel) dateCardViewModel))
        {
          case 0:
            flag1 = true;
            switch (list1.Count)
            {
              case 2:
                flag2 = list2[0].Type == CardType.LogicAnd;
                break;
              case 3:
                flag2 = list2[0].Type == CardType.LogicAnd;
                flag3 = list2[1].Type == CardType.LogicAnd;
                break;
            }
            break;
          case 1:
            flag2 = true;
            switch (list1.Count)
            {
              case 2:
                flag1 = list2[0].Type == CardType.LogicAnd;
                break;
              case 3:
                flag1 = list2[0].Type == CardType.LogicAnd;
                flag3 = list2[1].Type == CardType.LogicAnd;
                break;
            }
            break;
          case 2:
            flag3 = true;
            if (list1.Count == 3)
            {
              flag2 = list2[1].Type == CardType.LogicAnd;
              flag1 = flag2;
              break;
            }
            break;
        }
      }
      switch (list1.Count)
      {
        case 1:
          return !flag1 ? new List<TaskBaseViewModel>() : TaskService.GetSingleFilterMatchedItems(list1[0], items);
        case 2:
          List<TaskBaseViewModel> first1 = flag1 ? TaskService.GetSingleFilterMatchedItems(list1[0], items) : new List<TaskBaseViewModel>();
          List<TaskBaseViewModel> second1 = flag2 ? TaskService.GetSingleFilterMatchedItems(list1[1], items) : new List<TaskBaseViewModel>();
          if (list2.Count == 1)
          {
            switch (list2[0].Type)
            {
              case CardType.LogicAnd:
                return TaskService.GetInterSetOfModels(first1, second1);
              case CardType.LogicOr:
                return TaskService.GetUnionOfModels(first1, second1);
            }
          }
          else
            break;
          break;
        case 3:
          List<TaskBaseViewModel> first2 = flag1 ? TaskService.GetSingleFilterMatchedItems(list1[0], items) : new List<TaskBaseViewModel>();
          List<TaskBaseViewModel> second2 = flag2 ? TaskService.GetSingleFilterMatchedItems(list1[1], items) : new List<TaskBaseViewModel>();
          List<TaskBaseViewModel> second3 = flag3 ? TaskService.GetSingleFilterMatchedItems(list1[2], items) : new List<TaskBaseViewModel>();
          if (list2.Count == 2)
          {
            CardType type1 = list2[0].Type;
            CardType type2 = list2[1].Type;
            List<TaskBaseViewModel> first3 = (List<TaskBaseViewModel>) null;
            switch (type1)
            {
              case CardType.LogicAnd:
                first3 = TaskService.GetInterSetOfModels(first2, second2);
                break;
              case CardType.LogicOr:
                first3 = TaskService.GetUnionOfModels(first2, second2);
                break;
            }
            if (first3 == null)
              first3 = new List<TaskBaseViewModel>();
            if (type2 == CardType.LogicAnd)
              return TaskService.GetInterSetOfModels(first3, second3);
            if (type2 == CardType.LogicOr)
              return TaskService.GetUnionOfModels(first3, second3);
            break;
          }
          break;
      }
      return new List<TaskBaseViewModel>();
    }

    private static List<TaskBaseViewModel> GetSingleFilterMatchedItems(
      CardViewModel model,
      List<TaskBaseViewModel> items)
    {
      List<TaskBaseViewModel> filterMatchedItems = new List<TaskBaseViewModel>();
      switch (model)
      {
        case PriorityCardViewModel priorityCardViewModel:
          filterMatchedItems = TaskViewModelHelper.GetItems(items, priorities: priorityCardViewModel.Values, type: priorityCardViewModel.LogicType).ToList<TaskBaseViewModel>();
          break;
        case DateCardViewModel dateCardViewModel:
          List<FilterDatePair> filterDatePairs = FilterUtils.GetFilterDatePairs((IEnumerable<string>) dateCardViewModel.Values);
          filterMatchedItems = TaskViewModelHelper.GetItems(items, startEndPairs: filterDatePairs, type: dateCardViewModel.LogicType).ToList<TaskBaseViewModel>();
          break;
        case AssigneeCardViewModel assigneeCardViewModel:
          filterMatchedItems = TaskViewModelHelper.GetItems(items, assignTo: assigneeCardViewModel.Values, type: assigneeCardViewModel.LogicType).ToList<TaskBaseViewModel>();
          break;
        case ProjectOrGroupCardViewModel groupCardViewModel:
          filterMatchedItems = TaskViewModelHelper.GetItems(items, groupCardViewModel.GroupIds, groupCardViewModel.Values, type: groupCardViewModel.LogicType).ToList<TaskBaseViewModel>();
          break;
        case TagCardViewModel tagCardViewModel:
          filterMatchedItems = TaskViewModelHelper.GetItems(items, tags: tagCardViewModel.Values, type: tagCardViewModel.LogicType).ToList<TaskBaseViewModel>();
          break;
      }
      return filterMatchedItems;
    }

    private static List<TaskBaseViewModel> GetTaskMatchedAdvancedFilter(
      AdvancedFilterViewModel advanced,
      List<TaskBaseViewModel> tasks,
      bool inCal = false)
    {
      List<CardViewModel> list1 = advanced.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>();
      List<CardViewModel> list2 = advanced.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.LogicAnd || card.Type == CardType.LogicOr)).ToList<CardViewModel>();
      switch (list1.Count)
      {
        case 1:
          return TaskService.GetSingleFilterMatchedTasks(list1[0], tasks, inCal);
        case 2:
          List<TaskBaseViewModel> filterMatchedTasks1 = TaskService.GetSingleFilterMatchedTasks(list1[0], tasks, inCal);
          List<TaskBaseViewModel> filterMatchedTasks2 = TaskService.GetSingleFilterMatchedTasks(list1[1], tasks, inCal);
          if (list2.Count == 1)
          {
            switch (list2[0].Type)
            {
              case CardType.LogicAnd:
                return TaskService.GetInterSetOfModels(filterMatchedTasks1, filterMatchedTasks2);
              case CardType.LogicOr:
                return TaskService.GetUnionOfModels(filterMatchedTasks1, filterMatchedTasks2);
            }
          }
          else
            break;
          break;
        case 3:
          List<TaskBaseViewModel> filterMatchedTasks3 = TaskService.GetSingleFilterMatchedTasks(list1[0], tasks, inCal);
          List<TaskBaseViewModel> filterMatchedTasks4 = TaskService.GetSingleFilterMatchedTasks(list1[1], tasks, inCal);
          List<TaskBaseViewModel> filterMatchedTasks5 = TaskService.GetSingleFilterMatchedTasks(list1[2], tasks, inCal);
          if (list2.Count == 2)
          {
            CardType type1 = list2[0].Type;
            CardType type2 = list2[1].Type;
            List<TaskBaseViewModel> first = (List<TaskBaseViewModel>) null;
            switch (type1)
            {
              case CardType.LogicAnd:
                first = TaskService.GetInterSetOfModels(filterMatchedTasks3, filterMatchedTasks4);
                break;
              case CardType.LogicOr:
                first = TaskService.GetUnionOfModels(filterMatchedTasks3, filterMatchedTasks4);
                break;
            }
            if (first == null)
              first = new List<TaskBaseViewModel>();
            if (type2 == CardType.LogicAnd)
              return TaskService.GetInterSetOfModels(first, filterMatchedTasks5);
            if (type2 == CardType.LogicOr)
              return TaskService.GetUnionOfModels(first, filterMatchedTasks5);
            break;
          }
          break;
      }
      return new List<TaskBaseViewModel>();
    }

    private static List<TaskBaseViewModel> GetSingleFilterMatchedTasks(
      CardViewModel model,
      List<TaskBaseViewModel> tasks,
      bool inCal = false)
    {
      List<TaskBaseViewModel> filterMatchedTasks = new List<TaskBaseViewModel>();
      switch (model)
      {
        case PriorityCardViewModel priorityCardViewModel:
          filterMatchedTasks = TaskViewModelHelper.GetTasks(tasks, priorities: priorityCardViewModel.Values, type: priorityCardViewModel.LogicType, inCal: inCal).ToList<TaskBaseViewModel>();
          break;
        case DateCardViewModel dateCardViewModel:
          List<FilterDatePair> filterDatePairs = FilterUtils.GetFilterDatePairs((IEnumerable<string>) dateCardViewModel.Values);
          filterMatchedTasks = TaskViewModelHelper.GetTasks(tasks, startEndPairs: filterDatePairs, type: dateCardViewModel.LogicType, inCal: inCal).ToList<TaskBaseViewModel>();
          break;
        case AssigneeCardViewModel assigneeCardViewModel:
          filterMatchedTasks = TaskViewModelHelper.GetTasks(tasks, assignTo: assigneeCardViewModel.Values, type: assigneeCardViewModel.LogicType, inCal: inCal).ToList<TaskBaseViewModel>();
          break;
        case ProjectOrGroupCardViewModel groupCardViewModel:
          filterMatchedTasks = TaskViewModelHelper.GetTasks(tasks, groupCardViewModel.GroupIds, groupCardViewModel.Values, type: groupCardViewModel.LogicType, inCal: inCal).ToList<TaskBaseViewModel>();
          break;
        case TagCardViewModel tagCardViewModel:
          filterMatchedTasks = TaskViewModelHelper.GetTasks(tasks, tags: tagCardViewModel.Values, type: tagCardViewModel.LogicType, inCal: inCal).ToList<TaskBaseViewModel>();
          break;
        case TaskTypeViewModel taskTypeViewModel:
          List<string> values = taskTypeViewModel.Values;
          TaskType taskType = values == null || values.Count != 1 ? TaskType.TaskAndNote : (values[0] != "task" ? TaskType.Note : TaskType.Task);
          filterMatchedTasks = TaskViewModelHelper.GetTasks(tasks, taskType: taskType).ToList<TaskBaseViewModel>();
          break;
        case KeywordsViewModel keywordsViewModel:
          string keyword = keywordsViewModel.Keyword;
          filterMatchedTasks = TaskViewModelHelper.GetTasks(tasks, keyword: keyword).ToList<TaskBaseViewModel>();
          break;
      }
      return filterMatchedTasks;
    }

    public static async Task<TaskCloseExtra> SetTaskStatus(
      string taskId,
      int closeStatus,
      bool needUndo = false,
      IToastShowWindow undoWindow = null,
      bool inDetail = false,
      bool playSound = true)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return (TaskCloseExtra) null;
      if (!string.IsNullOrEmpty(thinTaskById.exDates))
        thinTaskById.exDate = ExDateSerilizer.ToArray(thinTaskById.exDates);
      return needUndo & inDetail && thinTaskById.status != 0 && closeStatus == 0 && CloseUndoHandler.TryUndoInDetail(thinTaskById.id) ? (TaskCloseExtra) null : await TaskService.SetTaskStatus(thinTaskById, closeStatus, playSound, needUndo: needUndo, undoWindow: undoWindow, toastUndo: !inDetail);
    }

    public static async Task<TaskCloseExtra> SetTaskStatus(
      TaskModel originalTask,
      int closeStatus,
      bool playSound = true,
      bool ignoreRepeat = false,
      bool needUndo = false,
      IToastShowWindow undoWindow = null,
      bool completeByItem = false,
      bool toastUndo = true)
    {
      if (closeStatus == 2)
        LocalSettings.Settings.ExtraSettings.LastCompleteTime = ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Today);
      if (playSound && originalTask.status == 0 && closeStatus != -1)
        Utils.PlayCompletionSound();
      UtilLog.Info(string.Format("TaskService.SetTaskStatus {0},{1}, {2} to {3}, repeatFlag {4}, ignoreRepeat {5}", (object) originalTask.id, (object) originalTask.startDate, (object) originalTask.status, (object) closeStatus, (object) originalTask.repeatFlag, (object) ignoreRepeat));
      TaskCloseExtra result = new TaskCloseExtra();
      if (Utils.IsEmptyDate(originalTask.startDate))
        originalTask.startDate = new DateTime?();
      originalTask.remindTime = new DateTime?();
      ProjectModel project = await ProjectDao.GetProjectById(originalTask.projectId);
      if (project != null && originalTask.status != closeStatus && project.viewMode == "kanban" && string.IsNullOrEmpty(originalTask.columnId))
        originalTask.columnId = await ColumnDao.GetProjectDefaultColumnId(originalTask.projectId);
      needUndo = needUndo && originalTask.status == 0;
      if (needUndo)
      {
        if (completeByItem)
          CloseUndoHandler.AddUndoOriginTask(originalTask.id, originalTask.Clone());
        else
          CloseUndoHandler.AddUndoModel(originalTask.id, originalTask.Clone());
      }
      bool isNonRepeat = TaskService.IsNonRepeatTask(originalTask);
      TaskModel repeatTask;
      if (isNonRepeat | ignoreRepeat && originalTask.status != closeStatus)
      {
        if (originalTask.status == 0)
          await TickFocusManager.NotifyPomoTaskChanged(originalTask.id, (string) null);
        originalTask.status = closeStatus;
        originalTask.modifiedTime = new DateTime?(DateTime.Now);
        if (originalTask.status != 0)
        {
          originalTask.completedTime = new DateTime?(DateTime.Now);
          originalTask.completedUserId = Utils.GetCurrentUserStr();
          await TaskService.TryCloseSubTask(originalTask.id, originalTask.projectId, needUndo, closeStatus);
        }
        else
        {
          originalTask.completedTime = new DateTime?();
          originalTask.completedUserId = (string) null;
          await TaskService.TryUnCompleteParentTask(originalTask.id);
        }
      }
      else
      {
        TaskModel newTask = new TaskModel(originalTask)
        {
          id = Utils.GetGuid(),
          status = closeStatus,
          repeatFlag = string.Empty,
          createdTime = new DateTime?(DateTime.Now),
          completedTime = new DateTime?(DateTime.Now),
          repeatTaskId = originalTask.id,
          sortOrder = ProjectSortOrderDao.GetNextTaskSortOrderInProject(originalTask.projectId, originalTask.sortOrder, originalTask.parentId),
          completedUserId = Utils.GetCurrentUserStr()
        };
        newTask.AddPinnedSecond();
        double diffInMinutes = 0.0;
        DateTime? nullable1 = originalTask.startDate;
        if (nullable1.HasValue)
        {
          nullable1 = originalTask.dueDate;
          if (nullable1.HasValue)
          {
            nullable1 = originalTask.dueDate;
            DateTime dateTime1 = nullable1.Value;
            nullable1 = originalTask.startDate;
            DateTime dateTime2 = nullable1.Value;
            diffInMinutes = (dateTime1 - dateTime2).TotalMinutes;
          }
        }
        int addForgotCycle = 1;
        originalTask.startDate = new DateTime?(RepeatUtils.GetNextRepeatDate(originalTask, ref addForgotCycle));
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(originalTask.id);
        DateTime? nullable2;
        if (thinTaskById != null)
        {
          nullable1 = thinTaskById.startDate;
          nullable2 = originalTask.startDate;
          if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0 && originalTask.repeatFrom != "1")
            return result;
        }
        originalTask.repeatFlag = RRuleUtils.GetNextCount(originalTask.repeatFlag, true);
        nullable2 = originalTask.dueDate;
        if (nullable2.HasValue)
        {
          nullable2 = originalTask.startDate;
          if (nullable2.HasValue)
          {
            TaskModel taskModel1 = originalTask;
            nullable2 = originalTask.startDate;
            DateTime? nullable3 = new DateTime?(nullable2.Value.AddMinutes(diffInMinutes));
            taskModel1.dueDate = nullable3;
          }
        }
        TaskModel taskModel2 = originalTask;
        nullable2 = new DateTime?();
        DateTime? nullable4 = nullable2;
        taskModel2.remindTime = nullable4;
        originalTask.status = 0;
        originalTask.progress = new int?();
        originalTask.modifiedTime = new DateTime?(DateTime.Now);
        repeatTask = await TaskDao.InsertTask(newTask);
        await ReminderDelayDao.DeleteByIdAsync(originalTask.id, "task");
        if (needUndo)
          CloseUndoHandler.AddUndoNewTasks(originalTask.id, new List<TaskModel>()
          {
            repeatTask
          });
        if (!needUndo)
          SyncStatusDao.AddSyncStatus(newTask.id, 4);
        if (!string.IsNullOrEmpty(repeatTask.parentId))
        {
          TaskDao.AddOrRemoveTaskChildIds(repeatTask.parentId, new List<string>()
          {
            repeatTask.id
          }, true);
          SyncStatusDao.AddSetParentSyncStatus(repeatTask.id, "");
        }
        await TaskService.TryCloseAndCopySubTask(originalTask, newTask, needUndo, closeStatus);
        await ProjectCopyManager.CopyChecklistItems(originalTask.id, repeatTask.id);
        double deltaInDays = 0.0;
        nullable2 = originalTask.startDate;
        if (nullable2.HasValue)
        {
          nullable2 = repeatTask.startDate;
          if (nullable2.HasValue)
          {
            nullable2 = originalTask.startDate;
            DateTime dateTime3 = nullable2.Value;
            nullable2 = repeatTask.startDate;
            DateTime dateTime4 = nullable2.Value;
            deltaInDays = (dateTime3 - dateTime4).TotalDays;
          }
        }
        await TaskService.UpdateTaskCheckItemsWithDeltaDate(originalTask.id, deltaInDays);
        if (originalTask.reminders != null)
        {
          List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(originalTask.id);
          if (originalTask.isAllDay.HasValue && originalTask.isAllDay.Value && remindersByTaskId.Any<TaskReminderModel>())
          {
            foreach (TaskReminderModel taskReminderModel in remindersByTaskId)
              taskReminderModel.trigger = TriggerUtils.ConvertLegacyTrigger(taskReminderModel.trigger);
          }
          originalTask.reminders = remindersByTaskId.ToArray();
        }
        TaskModel taskModel = repeatTask;
        taskModel.reminders = await Utils.CopyReminderItem(repeatTask, originalTask.reminders);
        taskModel = (TaskModel) null;
        taskModel = repeatTask;
        taskModel.Attachments = await Utils.CopyAttachmentItem(repeatTask, originalTask.id);
        taskModel = (TaskModel) null;
        taskModel = repeatTask;
        taskModel.FocusSummaries = await Utils.CopyPomoSummaries(repeatTask, originalTask.id);
        taskModel = (TaskModel) null;
        UtilLog.Info("TaskService.CleanPomosByTaskId: " + originalTask.id);
        await PomoSummaryDao.CleanPomosByTaskId(originalTask.id);
        await TaskService.UpdateTask(repeatTask);
        if (Utils.IsEmptyDate(originalTask.startDate))
        {
          originalTask.deleted = 1;
          await TaskDao.DeleteTaskToTrash(originalTask.id);
          await SyncStatusDao.AddTrashTaskStatus(originalTask.id);
        }
        result.RepeatTask = repeatTask;
        await TickFocusManager.NotifyPomoTaskChanged(originalTask.id, newTask.id);
        newTask = (TaskModel) null;
        repeatTask = (TaskModel) null;
      }
      result.OriginalTask = originalTask;
      if (originalTask.kind == "CHECKLIST")
      {
        repeatTask = originalTask;
        repeatTask.progress = new int?(await TaskService.CalculateProgress(originalTask.id));
        repeatTask = (TaskModel) null;
      }
      if (project != null)
      {
        result.OriginalTask.Color = project.color;
        if (result.RepeatTask != null)
          result.RepeatTask.Color = project.color;
      }
      await TaskService.UpdateTask(originalTask);
      if (!needUndo)
        await SyncStatusDao.AddSyncStatus(originalTask.id, 0);
      if (needUndo)
      {
        if (toastUndo)
          CloseUndoHandler.TryToast(undoWindow ?? (IToastShowWindow) App.Window, closeStatus);
        else
          CloseUndoHandler.TryStartTimer();
      }
      TaskChangeNotifier.NotifyTaskStatusChanged(result);
      UtilLog.Info(string.Format("SetTaskStatus : isRepeat {0}", (object) !isNonRepeat));
      return result;
    }

    public static async Task TryCloseAndCopySubTask(
      TaskModel originalTask,
      TaskModel newTask,
      bool needUndo,
      int closeStatus)
    {
      List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(originalTask.id, originalTask.projectId);
      List<TaskModel> taskModelList = new List<TaskModel>();
      List<TaskModel> updateTasks = new List<TaskModel>();
      List<TaskModel> needCopyTasks = taskModelList;
      List<TaskModel> updateTasks1 = updateTasks;
      TaskService.AssemblyRepeatTaskOnComplete(subTasksByIdAsync, needCopyTasks, updateTasks1);
      taskModelList.RemoveAll((Predicate<TaskModel>) (t => t.id == originalTask.id));
      Dictionary<string, string> dict = new Dictionary<string, string>();
      dict[originalTask.id] = newTask.id;
      List<TaskModel> copyTasks = new List<TaskModel>();
      TaskModel task;
      foreach (TaskModel taskModel in taskModelList)
      {
        task = taskModel;
        TaskModel copy = new TaskModel(task)
        {
          id = Utils.GetGuid()
        };
        if (copy.status == 0)
        {
          copy.status = closeStatus;
          copy.completedTime = new DateTime?(DateTime.Now);
          copy.completedUserId = Utils.GetCurrentUserIdInt().ToString();
        }
        copy.AddPinnedSecond();
        copyTasks.Add(copy);
        dict[task.id] = copy.id;
        await TaskService.CopyExtraData(task, copy);
        if (task.status != 0)
        {
          if (needUndo)
            CloseUndoHandler.AddUndoOriginTask(originalTask.id, task.Clone());
          task.status = 0;
          task.completedTime = new DateTime?();
          task.modifiedTime = new DateTime?(DateTime.Now);
          await TaskService.UpdateTaskOnStatusChanged(task);
          if (!needUndo)
            await SyncStatusDao.AddSyncStatus(task.id, 0);
        }
        task = (TaskModel) null;
      }
      foreach (TaskModel task1 in copyTasks)
      {
        if (!string.IsNullOrEmpty(task1.parentId) && dict.ContainsKey(task1.parentId))
        {
          string str = dict[task1.parentId];
          task1.parentId = str;
          TaskModel taskModel = await TaskDao.InsertTask(task1);
          if (needUndo)
            await SyncStatusDao.AddSyncStatus(taskModel.id, 4);
        }
      }
      foreach (TaskModel taskModel in copyTasks)
      {
        task = taskModel;
        if (!string.IsNullOrEmpty(task.parentId))
        {
          await SyncStatusDao.AddSetParentSyncStatus(task.id, "");
          await TaskDao.AddOrRemoveTaskChildIds(task.parentId, new List<string>()
          {
            task.id
          }, true);
        }
        task = (TaskModel) null;
      }
      if (needUndo)
        CloseUndoHandler.AddUndoNewTasks(originalTask.id, copyTasks);
      foreach (TaskModel taskModel in updateTasks)
      {
        if (!string.IsNullOrEmpty(taskModel.parentId) && dict.ContainsKey(taskModel.parentId))
        {
          CloseUndoHandler.AddUndoOriginTask(originalTask.id, taskModel.Clone());
          string parentId = dict[taskModel.parentId];
          await TaskDao.UpdateParent(taskModel.id, parentId);
        }
      }
      updateTasks = (List<TaskModel>) null;
      dict = (Dictionary<string, string>) null;
      copyTasks = (List<TaskModel>) null;
    }

    private static void AssemblyRepeatTaskOnComplete(
      List<TaskModel> subTasks,
      List<TaskModel> needCopyTasks,
      List<TaskModel> updateTasks)
    {
      if (subTasks == null)
        return;
      List<TaskModel> list = subTasks.Where<TaskModel>((Func<TaskModel, bool>) (t => t.status != 0 && !string.IsNullOrEmpty(t.repeatTaskId) && t.id != t.repeatTaskId && subTasks.Any<TaskModel>((Func<TaskModel, bool>) (ta => ta.id == t.repeatTaskId)))).ToList<TaskModel>();
      HashSet<TaskModel> collection = new HashSet<TaskModel>((IEnumerable<TaskModel>) list);
      subTasks.RemoveAll(new Predicate<TaskModel>(collection.Contains));
      foreach (TaskModel taskModel1 in list)
      {
        List<TaskModel> subTasks1 = TaskService.GetSubTasks(taskModel1.id, subTasks);
        if (subTasks1 != null && subTasks1.Any<TaskModel>())
        {
          foreach (TaskModel taskModel2 in subTasks1)
          {
            subTasks.Remove(taskModel2);
            collection.Add(taskModel2);
          }
        }
      }
      subTasks.RemoveAll(new Predicate<TaskModel>(collection.Contains));
      needCopyTasks.AddRange((IEnumerable<TaskModel>) subTasks);
      updateTasks.AddRange((IEnumerable<TaskModel>) collection);
    }

    private static async Task CopyExtraData(TaskModel task, TaskModel copy)
    {
      await ProjectCopyManager.CopyChecklistItems(task.id, copy.id);
      List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
      bool? isAllDay = task.isAllDay;
      if (isAllDay.HasValue)
      {
        isAllDay = task.isAllDay;
        if (isAllDay.Value && remindersByTaskId.Any<TaskReminderModel>())
        {
          foreach (TaskReminderModel taskReminderModel in remindersByTaskId)
            taskReminderModel.trigger = TriggerUtils.ConvertLegacyTrigger(taskReminderModel.trigger);
        }
      }
      TaskModel taskModel;
      if (remindersByTaskId != null)
      {
        task.reminders = remindersByTaskId.ToArray();
        taskModel = copy;
        taskModel.reminders = await Utils.CopyReminderItem(copy, task.reminders);
        taskModel = (TaskModel) null;
      }
      taskModel = copy;
      taskModel.Attachments = await Utils.CopyAttachmentItem(copy, task.id);
      taskModel = (TaskModel) null;
      taskModel = copy;
      taskModel.FocusSummaries = await Utils.CopyPomoSummaries(copy, task.id);
      taskModel = (TaskModel) null;
      await PomoSummaryDao.CleanPomosByTaskId(task.id);
    }

    private static async Task TryUnCompleteParentTask(string taskId)
    {
      List<TaskModel> parentsByTaskIdAsync = await TaskDao.GetParentsByTaskIdAsync(taskId);
      if (parentsByTaskIdAsync == null)
        return;
      foreach (TaskModel task in parentsByTaskIdAsync)
      {
        task.status = 0;
        task.completedTime = new DateTime?();
        task.completedUserId = (string) null;
        await TaskService.UpdateTaskOnStatusChanged(task);
        await SyncStatusDao.AddSyncStatus(task.id, 0);
      }
    }

    private static async Task TryCloseSubTask(
      string taskId,
      string projectId,
      bool needUndo,
      int closeStatus)
    {
      List<TaskModel> subTasks = await TaskService.GetAllSubTasksByIdAsync(taskId, projectId);
      if (subTasks == null)
      {
        subTasks = (List<TaskModel>) null;
      }
      else
      {
        subTasks = subTasks.Where<TaskModel>((Func<TaskModel, bool>) (s => s.status == 0)).ToList<TaskModel>();
        foreach (TaskModel task in subTasks)
        {
          if (needUndo)
            CloseUndoHandler.AddUndoOriginTask(taskId, task.Clone());
          await TickFocusManager.NotifyPomoTaskChanged(task.id, (string) null);
          task.status = closeStatus;
          task.completedTime = new DateTime?(DateTime.Now);
          task.completedUserId = Utils.GetCurrentUserStr();
        }
        await TaskDao.BatchUpdateTasks(subTasks, checkMatched: CheckMatchedType.All);
        foreach (TaskModel taskModel in subTasks)
        {
          if (!needUndo)
            await SyncStatusDao.AddSyncStatus(taskModel.id, 0);
        }
        subTasks = (List<TaskModel>) null;
      }
    }

    private static async Task UpdateTaskCheckItemsWithDeltaDate(
      string originalTaskId,
      double deltaInDays)
    {
      List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(originalTaskId);
      if (checkItemsByTaskId.Count <= 0)
        return;
      foreach (TaskDetailItemModel taskDetailItemModel1 in checkItemsByTaskId)
      {
        taskDetailItemModel1.status = 0;
        DateTime? startDate = taskDetailItemModel1.startDate;
        if (startDate.HasValue)
        {
          TaskDetailItemModel taskDetailItemModel2 = taskDetailItemModel1;
          startDate = taskDetailItemModel1.startDate;
          DateTime? nullable = new DateTime?(startDate.Value.AddDays(deltaInDays));
          taskDetailItemModel2.startDate = nullable;
        }
      }
      await TaskDetailItemDao.BatchUpdateChecklists(checkItemsByTaskId.ToList<TaskDetailItemModel>());
    }

    public static async Task<TaskModel> DeleteCheckItem(string itemId)
    {
      TaskDetailItemModel item = await TaskDetailItemDao.GetChecklistItemById(itemId);
      if (item != null)
      {
        await TaskDetailItemDao.DeleteById(itemId);
        ItemChangeNotifier.NotifyItemDeleted(item);
        CheckItemCompleteExtra itemCompleteExtra = await TaskService.SyncTaskChange(item);
        if (itemCompleteExtra != null)
          return itemCompleteExtra.OriginalTask;
      }
      return (TaskModel) null;
    }

    public static async Task ReminderCompleteCheckItem(string itemId)
    {
      TaskDetailItemModel item = await TaskDetailItemDao.GetChecklistItemById(itemId);
      if (item == null)
        item = (TaskDetailItemModel) null;
      else if (item.status != 0)
      {
        item = (TaskDetailItemModel) null;
      }
      else
      {
        item.status = 1;
        item.completedTime = new DateTime?(DateTime.Now);
        await TaskDetailItemDao.SaveChecklistItem(item);
        CheckItemCompleteExtra itemCompleteExtra = await TaskService.SyncTaskChange(item, true);
        ItemChangeNotifier.NotifyItemDateChanged(item);
        if (await TaskDao.GetThinTaskById(item.TaskServerId) == null)
        {
          item = (TaskDetailItemModel) null;
        }
        else
        {
          SyncManager.TryDelaySync();
          item = (TaskDetailItemModel) null;
        }
      }
    }

    public static async Task CompleteCheckItem(
      string itemId,
      bool handleTask = true,
      bool completeOnly = false,
      bool needUndo = false,
      IToastShowWindow window = null)
    {
      TaskDetailItemModel item = await TaskDetailItemDao.GetChecklistItemById(itemId);
      if (item == null)
        item = (TaskDetailItemModel) null;
      else if (completeOnly && item.status == 1)
      {
        item = (TaskDetailItemModel) null;
      }
      else
      {
        if (needUndo)
          CloseUndoHandler.AddUndoCheckItem(item.TaskServerId, item);
        item.status = item.status != 1 ? 1 : 0;
        item.completedTime = item.status == 1 ? new DateTime?(DateTime.Now) : new DateTime?();
        await TaskDetailItemDao.SaveChecklistItem(item);
        CheckItemCompleteExtra itemCompleteExtra = await TaskService.SyncTaskChange(item, handleTask, needUndo, window);
        ItemChangeNotifier.NotifyItemStatusChanged(item.id);
        item = (TaskDetailItemModel) null;
      }
    }

    public static async Task<CheckItemCompleteExtra> SyncTaskChange(
      TaskDetailItemModel item,
      bool handleTask = false,
      bool needUndo = false,
      IToastShowWindow window = null)
    {
      TaskModel task = await TaskDao.GetThinTaskById(item.TaskServerId);
      if (task == null)
        return (CheckItemCompleteExtra) null;
      List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(task.id);
      int progress = 0;
      string originKind = task.kind;
      if (checkItemsByTaskId.Count == 0)
      {
        task.kind = "TEXT";
        task.progress = new int?(0);
        progress = 0;
        task.content = ChecklistUtils.Items2Text(task.desc, new List<string>());
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTask(task);
        await SyncStatusDao.AddSyncStatus(item.TaskServerId, 0);
      }
      else
      {
        task.kind = "CHECKLIST";
        progress = TaskService.CalculateProgress((IReadOnlyCollection<TaskDetailItemModel>) checkItemsByTaskId);
        if (originKind != task.kind)
          await TaskService.UpdateTaskOnKindChanged(task);
        if (progress < 100 && progress >= 0)
          await TaskService.SyncProgress(task, progress);
        else if (progress == 100)
        {
          if (handleTask && TaskCache.CanTaskCompletedByCheckItem(task.id))
          {
            UtilLog.Info("CompleteTask id " + task.id + " from:CheckItem");
            TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(task, 2, false, needUndo: needUndo, undoWindow: window, completeByItem: true);
            return new CheckItemCompleteExtra()
            {
              ItemId = item.id,
              OriginalTask = taskCloseExtra.OriginalTask,
              RepeatTask = taskCloseExtra.RepeatTask
            };
          }
          await TaskService.SyncProgress(task, progress);
        }
      }
      if (originKind != task.kind)
        TaskChangeNotifier.NotifyTaskKindChanged(new List<string>()
        {
          task.id
        });
      return new CheckItemCompleteExtra()
      {
        ItemId = item.id,
        OriginalTask = task
      };
    }

    public static async Task SyncProgress(TaskModel task, int progress)
    {
      task.progress = new int?(progress);
      task.modifiedTime = new DateTime?(DateTime.Now);
      await TaskService.UpdateTaskOnProgressChanged(task);
      await SyncStatusDao.AddSyncStatus(task.id, 0);
    }

    public static async Task<int> CalculateProgress(string taskId)
    {
      return TaskService.CalculateProgress((IReadOnlyCollection<TaskDetailItemModel>) await TaskDetailItemDao.GetCheckItemsByTaskId(taskId));
    }

    public static int CalculateProgress(
      IReadOnlyCollection<TaskDetailItemModel> checkItems)
    {
      if (!checkItems.Any<TaskDetailItemModel>())
        return 0;
      int count = checkItems.Count;
      int num = checkItems.Count<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (item => item.status == 0));
      return (int) Math.Round((double) (count - num) * 1.0 / ((double) count * 1.0) * 100.0, 0, MidpointRounding.AwayFromZero);
    }

    public static async Task<List<TaskModel>> CopySubTasks(string oldTaskId, string newTaskId)
    {
      TaskBaseViewModel taskById = TaskCache.GetTaskById(oldTaskId);
      List<TaskModel> children = await TaskService.GetAllSubTasksByIdAsync(taskById.Id, taskById.ProjectId);
      List<TaskModel> copyChildren = new List<TaskModel>();
      Dictionary<string, string> dict = new Dictionary<string, string>();
      dict[oldTaskId] = newTaskId;
      foreach (TaskModel taskModel in children)
      {
        TaskModel child = taskModel;
        if (string.IsNullOrEmpty(child.repeatTaskId) || !(child.repeatTaskId != child.id) || !children.Any<TaskModel>((Func<TaskModel, bool>) (c => c.id == child.repeatTaskId)))
        {
          child._Id = 0;
          string oldId = child.id;
          child.id = Utils.GetGuid();
          child.repeatTaskId = string.Empty;
          dict[oldId] = child.id;
          child.AddPinnedSecond();
          List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(oldId);
          if (remindersByTaskId != null && remindersByTaskId.Any<TaskReminderModel>())
          {
            TaskReminderModel[] taskReminderModelArray = await Utils.CopyReminderItem(child, remindersByTaskId.ToArray());
          }
          AttachmentModel[] attachmentModelArray = await Utils.CopyAttachmentItem(child, oldId);
          await ProjectCopyManager.CopyChecklistItems(oldId, child.id, false);
          copyChildren.Add(child);
          oldId = (string) null;
        }
      }
      foreach (TaskModel taskModel in copyChildren)
      {
        if (!string.IsNullOrEmpty(taskModel.parentId) && dict.ContainsKey(taskModel.parentId))
          taskModel.parentId = dict[taskModel.parentId];
      }
      await TaskDao.BatchInsertTasks(copyChildren);
      List<TaskModel> taskModelList = copyChildren;
      children = (List<TaskModel>) null;
      copyChildren = (List<TaskModel>) null;
      dict = (Dictionary<string, string>) null;
      return taskModelList;
    }

    public static async Task UncheckAllSubTasks(TaskModel task)
    {
      List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(task.id, task.projectId);
      // ISSUE: explicit non-virtual call
      if (subTasksByIdAsync == null || __nonvirtual (subTasksByIdAsync.Count) <= 0)
        return;
      foreach (TaskModel child in subTasksByIdAsync)
      {
        bool flag = await TaskDao.UncheckAllCheckItems(child.id);
        DateTime? nullable1;
        if (child.status != 0)
        {
          child.status = 0;
          TaskModel taskModel = child;
          nullable1 = new DateTime?();
          DateTime? nullable2 = nullable1;
          taskModel.completedTime = nullable2;
          flag = true;
        }
        int? progress = child.progress;
        int num = 0;
        if (!(progress.GetValueOrDefault() == num & progress.HasValue))
        {
          child.progress = new int?(0);
          flag = true;
        }
        nullable1 = child.remindTime;
        if (nullable1.HasValue)
        {
          TaskModel taskModel = child;
          nullable1 = new DateTime?();
          DateTime? nullable3 = nullable1;
          taskModel.remindTime = nullable3;
          await ReminderDelayDao.DeleteByIdAsync(child.id, nameof (task));
          flag = true;
        }
        if (flag)
        {
          await TaskService.UpdateTaskOnStatusChanged(child);
          SyncStatusDao.AddModifySyncStatus(child.id);
        }
      }
    }

    public static async Task<bool> IsAllRepeatTask(List<string> taskIds)
    {
      if (taskIds == null || taskIds.Count == 0)
        return false;
      ObservableCollection<TaskModel> tasksInTaskIds = await TaskDao.GetTasksInTaskIds(taskIds);
      return tasksInTaskIds == null || !tasksInTaskIds.Any<TaskModel>() || !tasksInTaskIds.Any<TaskModel>((Func<TaskModel, bool>) (t => t.status == 0 && TaskService.IsNonRepeatTask(t)));
    }

    public static bool IsNonRepeatTask(TaskModel taskModel)
    {
      if (taskModel == null || taskModel.status != 0)
        return true;
      List<string> exDates = taskModel.exDate == null ? new List<string>() : ((IEnumerable<string>) taskModel.exDate).ToList<string>();
      return RepeatUtils.IsNonRepeatTask(taskModel.startDate, taskModel.isAllDay, taskModel.repeatFlag, taskModel.repeatFrom, taskModel.timeZone, taskModel.Floating, exDates);
    }

    public static SyncBean HandleTaskBeforeSave(SyncBean syncBean)
    {
      string str = Utils.GetCurrentUserIdInt().ToString();
      if (syncBean?.syncTaskBean != null)
      {
        SyncTaskBean syncTaskBean = syncBean.syncTaskBean;
        if (syncTaskBean.Add.Count > 0)
        {
          foreach (TaskModel task in syncTaskBean.Add)
          {
            TaskService.ClearDueDate(task);
            task.userId = str;
          }
        }
        if (syncTaskBean.Update.Count > 0)
        {
          foreach (TaskModel task in syncTaskBean.Update)
          {
            TaskService.ClearDueDate(task);
            task.userId = str;
          }
        }
      }
      return syncBean;
    }

    public static void ClearDueDate(TaskModel task)
    {
      if (task != null && task.startDate.HasValue && task.dueDate.HasValue)
      {
        if (task.isAllDay.HasValue && task.isAllDay.Value)
        {
          if (task.dueDate.Value.Date <= task.startDate.Value.Date)
            task.dueDate = new DateTime?();
        }
        else if (task.startDate.Value == task.dueDate.Value)
          task.dueDate = new DateTime?();
      }
      if (task?.items == null)
        return;
      foreach (TaskDetailItemModel taskDetailItemModel in task.items)
      {
        if (!string.IsNullOrEmpty(taskDetailItemModel.serverStartDate))
        {
          if (taskDetailItemModel.serverStartDate != "-1")
          {
            try
            {
              taskDetailItemModel.startDate = new DateTime?(DateTime.Parse(taskDetailItemModel.serverStartDate));
              if (!taskDetailItemModel.isAllDay.HasValue || !taskDetailItemModel.isAllDay.Value)
              {
                if (!task.Floating)
                  continue;
              }
              taskDetailItemModel.startDate = new DateTime?(TimeZoneUtils.LocalToTargetTzTime(taskDetailItemModel.startDate.Value, task.timeZone));
            }
            catch (Exception ex)
            {
            }
          }
        }
      }
    }

    public static async Task SkipSelectRecurrence(
      string taskId,
      DateTime dateTime,
      IToastShowWindow toastWindow = null)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        List<string> stringList = !string.IsNullOrEmpty(task.exDates) ? ((IEnumerable<string>) ExDateSerilizer.ToArray(task.exDates)).ToList<string>() : new List<string>();
        string str = dateTime.ToString("yyyyMMdd");
        if (!stringList.Contains(str))
          stringList.Add(str);
        task.exDates = ExDateSerilizer.ToString(stringList.ToArray());
        await TaskService.UpdateTaskOnTimeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        if (toastWindow == null)
          task = (TaskModel) null;
        else if (task.repeatFlag == null)
          task = (TaskModel) null;
        else if (!task.repeatFlag.Contains("COUNT"))
        {
          task = (TaskModel) null;
        }
        else
        {
          toastWindow.TryToastString((object) null, Utils.GetString("SkipCountToast"));
          task = (TaskModel) null;
        }
      }
    }

    public static async Task<TaskModel> SkipCurrentRecurrence(
      string taskId,
      bool notify = true,
      IToastShowWindow toastWindow = null,
      bool toast = true,
      bool handleCount = false)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task != null)
      {
        DateTime? originalStartDate = task.startDate;
        int addForgotCycle = 1;
        DateTime nextDate = RepeatUtils.GetNextRepeatDate(task, ref addForgotCycle);
        if (!Utils.IsEmptyDate(nextDate) && originalStartDate.HasValue)
        {
          task.startDate = new DateTime?(nextDate);
          task.modifiedTime = new DateTime?(DateTime.Now);
          task.progress = new int?();
          DateTime? nullable1 = task.dueDate;
          if (nullable1.HasValue)
          {
            nullable1 = task.dueDate;
            task.dueDate = new DateTime?(nextDate.AddMinutes((double) (int) (nullable1.Value - originalStartDate.Value).TotalMinutes));
          }
          if (task.repeatFlag != null && task.repeatFlag.Contains("FORGETTINGCURVE"))
            task.repeatFlag = RepeatUtils.GetNextEbbinghausCycle(task.repeatFlag, addForgotCycle);
          if (handleCount)
            task.repeatFlag = RRuleUtils.GetNextCount(task.repeatFlag, false);
          else if (toast && task.repeatFlag != null && task.repeatFlag.Contains("COUNT"))
            (toastWindow ?? (IToastShowWindow) App.Window).TryToastString((object) null, Utils.GetString("SkipCountToast"));
          List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
          if (checkItemsByTaskId != null && checkItemsByTaskId.Any<TaskDetailItemModel>())
          {
            int totalMinutes = (int) (nextDate - originalStartDate.Value).TotalMinutes;
            List<TaskDetailItemModel> taskDetailItemModelList = new List<TaskDetailItemModel>();
            foreach (TaskDetailItemModel taskDetailItemModel1 in checkItemsByTaskId)
            {
              bool flag = false;
              if (taskDetailItemModel1.status != 0)
              {
                taskDetailItemModel1.status = 0;
                flag = true;
              }
              nullable1 = taskDetailItemModel1.startDate;
              if (nullable1.HasValue)
              {
                TaskDetailItemModel taskDetailItemModel2 = taskDetailItemModel1;
                nullable1 = taskDetailItemModel1.startDate;
                DateTime? nullable2 = new DateTime?(nullable1.Value.AddMinutes((double) totalMinutes));
                taskDetailItemModel2.startDate = nullable2;
                flag = true;
              }
              if (flag)
                taskDetailItemModelList.Add(taskDetailItemModel1);
            }
            if (taskDetailItemModelList.Any<TaskDetailItemModel>())
              await TaskDetailItemDao.BatchUpdateChecklists(taskDetailItemModelList);
          }
          await TaskService.UpdateTaskOnTimeChanged(task);
          await SyncStatusDao.AddModifySyncStatus(task.id);
          if (notify)
            TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        }
        originalStartDate = new DateTime?();
      }
      TaskModel taskModel = task;
      task = (TaskModel) null;
      return taskModel;
    }

    public static async Task<TaskModel> SetStartDate(
      string taskId,
      DateTime newDate,
      bool checkWrongSet = true)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      TaskModel taskModel1 = task;
      bool noDateTask = taskModel1 == null || !taskModel1.startDate.HasValue;
      if (task != null)
      {
        task = await TaskService.ModifyTaskDate(task, newDate);
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnTimeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(taskId);
        if (noDateTask)
        {
          bool? isAllDay = task.isAllDay;
          if (isAllDay.HasValue)
          {
            isAllDay = task.isAllDay;
            if (isAllDay.Value)
            {
              task.reminders = TimeData.GetDefaultAllDayReminders().ToArray();
              task.exDates = string.Empty;
              await TaskService.SaveTaskReminders(task);
            }
          }
        }
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        if (checkWrongSet)
          ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(new TimeData()
          {
            StartDate = new DateTime?(newDate)
          });
      }
      TaskModel taskModel2 = task;
      task = (TaskModel) null;
      return taskModel2;
    }

    public static async Task SetTaskDateAfterDropHandle(
      string taskId,
      DateTime startDate,
      DateTime? dueDate,
      bool onlyEnd)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        if (!task.startDate.HasValue)
          task.isAllDay = new bool?(true);
        if (onlyEnd)
        {
          task.dueDate = dueDate;
        }
        else
        {
          task.startDate = new DateTime?(startDate);
          task.dueDate = dueDate;
        }
        task.modifiedTime = new DateTime?(DateTime.Now);
        task.exDates = string.Empty;
        await TaskService.UpdateTaskOnTimeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        task = (TaskModel) null;
      }
    }

    private static async Task<TaskModel> ModifyTaskDate(string taskId, DateTime newDate)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task != null)
      {
        TaskModel taskModel1 = await TaskService.ModifyTaskDate(task, newDate);
      }
      TaskModel taskModel2 = task;
      task = (TaskModel) null;
      return taskModel2;
    }

    public static async Task<TaskModel> ModifyTaskDate(TaskModel task, DateTime newDate)
    {
      if (task != null)
      {
        DateTime? nullable1 = task.startDate;
        if (!nullable1.HasValue)
        {
          task.startDate = new DateTime?(newDate);
          TaskModel taskModel = task;
          nullable1 = new DateTime?();
          DateTime? nullable2 = nullable1;
          taskModel.dueDate = nullable2;
          task.isAllDay = new bool?(true);
        }
        TimeData timeData = new TimeData()
        {
          StartDate = task.startDate,
          DueDate = task.dueDate,
          IsAllDay = task.isAllDay,
          RepeatFlag = task.repeatFlag,
          RepeatFrom = task.repeatFrom
        };
        TaskService.ChangeStartDate(ref timeData, newDate);
        task.startDate = timeData.StartDate;
        nullable1 = task.remindTime;
        if (nullable1.HasValue)
        {
          TaskModel taskModel = task;
          nullable1 = new DateTime?();
          DateTime? nullable3 = nullable1;
          taskModel.remindTime = nullable3;
          await ReminderDelayDao.DeleteByIdAsync(task.id, nameof (task));
        }
        task.dueDate = timeData.DueDate;
        task.modifiedTime = new DateTime?(DateTime.Now);
        task.exDates = string.Empty;
        timeData = (TimeData) null;
      }
      return task;
    }

    public static void ChangeStartDate(ref TimeData timeData, DateTime changeDate, bool changeTime = false)
    {
      if (timeData.StartDate.HasValue && !Utils.IsEmptyDate(timeData.StartDate) && (!timeData.DueDate.HasValue || Utils.IsEmptyDate(timeData.DueDate)) && !changeTime)
        timeData.StartDate = new DateTime?(ticktick_WPF.Util.DateUtils.SetDateOnly(timeData.StartDate.Value, changeDate));
      else if (timeData.StartDate.HasValue && !Utils.IsEmptyDate(timeData.StartDate) && timeData.DueDate.HasValue && !Utils.IsEmptyDate(timeData.DueDate))
      {
        double totalHours = (timeData.DueDate.Value - timeData.StartDate.Value).TotalHours;
        TimeData timeData1 = timeData;
        DateTime? startDate = timeData.StartDate;
        DateTime? nullable1 = new DateTime?(ticktick_WPF.Util.DateUtils.SetDateOnly(startDate.Value, changeDate));
        timeData1.StartDate = nullable1;
        TimeData timeData2 = timeData;
        startDate = timeData.StartDate;
        DateTime? nullable2 = new DateTime?(startDate.Value.AddHours(totalHours));
        timeData2.DueDate = nullable2;
      }
      else
        timeData.StartDate = new DateTime?(changeDate);
      TaskService.TryFixRepeatFlag(ref timeData);
    }

    public static void TryFixRepeatFlag(ref TimeData timeData)
    {
      timeData.RepeatFlag = TaskService.TryFixRepeatFlag(timeData.StartDate, timeData.RepeatFlag, timeData.RepeatFrom);
    }

    private static string TryFixRepeatFlag(
      DateTime? startDate,
      string repeatFlag,
      string repeatFrom)
    {
      if (repeatFrom != null && repeatFlag != null && startDate.HasValue && repeatFrom == "2" && repeatFlag.Contains("FREQ=MONTHLY") && repeatFlag.Contains("RRULE"))
      {
        if (repeatFlag.Contains("BYMONTHDAY"))
        {
          int startIndex = repeatFlag.IndexOf("BYMONTHDAY", StringComparison.Ordinal);
          int num1 = repeatFlag.IndexOf(";", startIndex, StringComparison.Ordinal);
          int length = num1 > 0 ? num1 - startIndex : repeatFlag.Length - startIndex;
          if (startIndex > 1)
          {
            int num2 = repeatFlag.IndexOf(";", StringComparison.Ordinal);
            if (num2 > 0)
            {
              if (num2 < startIndex)
              {
                --startIndex;
                ++length;
              }
              else
                ++length;
            }
          }
          string oldValue = repeatFlag.Substring(startIndex, length);
          repeatFlag = repeatFlag.Replace(oldValue, "");
        }
        repeatFlag = repeatFlag + ";BYMONTHDAY=" + startDate.Value.Day.ToString();
      }
      return repeatFlag;
    }

    public static async Task DeleteAgenda(string taskId, string projectId, string attendId)
    {
      await TaskService.DeleteTask(taskId);
      await Communicator.DeleteTaskAttend(projectId, taskId);
    }

    public static async Task DeleteTask(string taskId, int status = 1)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById != null)
      {
        thinTaskById.deleted = status;
        thinTaskById.attendId = (string) null;
        thinTaskById.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnDeletedChanged(thinTaskById);
        if (status == 1)
        {
          UtilLog.Info("TaskService.DeleteTask : " + taskId);
          TaskChangeNotifier.NotifyTaskDeleted(taskId);
          await SyncStatusDao.AddDeleteSyncStatus(taskId);
        }
        else
        {
          UtilLog.Info("TaskService.DeleteTaskForever : " + taskId);
          List<string> taskIds = new List<string>();
          taskIds.Add(taskId);
          TaskChangeNotifier.NotifyTaskDeleted(taskId);
          await SyncStatusDao.BatchAddDeleteForeverSyncStatus((IEnumerable<string>) taskIds);
        }
      }
      SyncManager.TryDelaySync();
    }

    public static async Task UndoDeletedTask(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        if (task.deleted == 2)
        {
          await SyncStatusDao.RemoveDeleteForeverSyncStatus(taskId);
        }
        else
        {
          int num = await SyncStatusDao.RemoveDeleteSyncStatus(taskId) ? 1 : 0;
        }
        task.deleted = 0;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnDeletedChanged(task);
        UtilLog.Info("TaskService.UndoDeletedTask : " + taskId);
        TaskChangeNotifier.NotifyDeleteUndo(taskId);
        task = (TaskModel) null;
      }
    }

    public static async Task UndoDeletedCheckItem(TaskDetailItemModel model)
    {
      if (model == null)
        return;
      await TaskDetailItemDao.InsertChecklistItem(model);
      if (await TaskDao.GetThinTaskById(model.TaskServerId) == null)
        return;
      CheckItemCompleteExtra itemCompleteExtra = await TaskService.SyncTaskChange(model);
      UtilLog.Info("TaskService.UndoDeletedCheckItem : " + model.id);
      ItemChangeNotifier.NotifyDeletedUndo(model);
    }

    public static List<TaskBaseViewModel> GetInterSetOfModels(
      List<TaskBaseViewModel> first,
      List<TaskBaseViewModel> second)
    {
      if (second == null)
        return first;
      List<string> interSetIds = first.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>().Intersect<string>((IEnumerable<string>) second.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>()).ToList<string>();
      return interSetIds.Count <= 0 ? new List<TaskBaseViewModel>() : first.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => interSetIds.Contains(t.Id))).ToList<TaskBaseViewModel>();
    }

    public static List<TaskBaseViewModel> GetUnionOfModels(
      List<TaskBaseViewModel> first,
      List<TaskBaseViewModel> second)
    {
      List<string> list1 = first.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>();
      List<string> exceptFirstIds = second.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>().Except<string>((IEnumerable<string>) list1).ToList<string>();
      List<TaskBaseViewModel> list2 = second.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => exceptFirstIds.Contains(t.Id))).ToList<TaskBaseViewModel>();
      return first.Union<TaskBaseViewModel>((IEnumerable<TaskBaseViewModel>) list2).ToList<TaskBaseViewModel>();
    }

    public static bool IsAdvanceModelValid(List<CardViewModel> cards)
    {
      if (cards.Count == 4)
      {
        CardViewModel card1 = cards[1];
        if ((card1 != null ? (card1.Type == CardType.LogicAnd ? 1 : 0) : 0) != 0 && cards[0] is DateCardViewModel card2 && cards[2] is DateCardViewModel card3 && (card2.Values.Contains("today") && card3.Values.Contains("tomorrow") || card3.Values.Contains("today") && card2.Values.Contains("tomorrow")))
          return false;
      }
      return true;
    }

    public static async Task SetAssignee(string taskId, string assignee, bool notify = true)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
        task = (TaskModel) null;
      else if (!(task.assignee != assignee))
      {
        task = (TaskModel) null;
      }
      else
      {
        task.assignee = assignee;
        task.modifiedTime = new DateTime?(DateTime.Now);
        Communicator.SetTaskAssign(new List<AssignModel>()
        {
          new AssignModel()
          {
            assignee = assignee,
            projectId = task.projectId,
            taskId = task.id
          }
        });
        await TaskService.UpdateTaskOnAssigneeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        if (!notify)
        {
          task = (TaskModel) null;
        }
        else
        {
          TaskChangeNotifier.NotifyTaskAssigneeChanged(new List<string>()
          {
            taskId
          }, assignee);
          task = (TaskModel) null;
        }
      }
    }

    public static async Task SetSortOrder(string taskId, long sortOrder, string columnId = "")
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.sortOrder = sortOrder;
        task.modifiedTime = new DateTime?(DateTime.Now);
        if (!string.IsNullOrEmpty(columnId))
          task.columnId = columnId;
        TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
        taskViewModel.SortOrder = task.sortOrder;
        taskViewModel.ColumnId = task.columnId;
        taskViewModel.ModifiedTime = task.modifiedTime;
        await TaskDao.UpdateTask(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        task = (TaskModel) null;
      }
    }

    public static async Task SetPriority(
      string taskId,
      int priority,
      string columnId = "",
      bool notify = true)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
        task = (TaskModel) null;
      else if (task.kind == "NOTE")
      {
        task = (TaskModel) null;
      }
      else
      {
        task.priority = priority;
        if (!string.IsNullOrEmpty(columnId))
          task.columnId = columnId;
        task.modifiedTime = new DateTime?(DateTime.Now);
        TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
        viewModel.Priority = task.priority;
        viewModel.ColumnId = task.columnId;
        viewModel.ModifiedTime = task.modifiedTime;
        await TaskDao.UpdateTask(task);
        ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.OnlyFilter);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        if (notify)
          TaskChangeNotifier.NotifyTaskPriorityChanged(taskId, priority);
        viewModel = (TaskBaseViewModel) null;
        task = (TaskModel) null;
      }
    }

    public static async Task CopyTasks(List<string> ids)
    {
      List<TaskModel> thinTasksInBatch = await TaskDao.GetThinTasksInBatch(TaskDao.GetTreeTopIds(ids, (string) null));
      List<string> newTaskIds;
      if (thinTasksInBatch == null)
      {
        newTaskIds = (List<string>) null;
      }
      else
      {
        thinTasksInBatch.Sort((Comparison<TaskModel>) ((a, b) => b.sortOrder.CompareTo(a.sortOrder)));
        newTaskIds = new List<string>();
        foreach (TaskModel taskModel1 in thinTasksInBatch)
        {
          TaskModel taskModel2 = await TaskService.CopyTask(taskModel1.id, true);
          if (taskModel2 != null)
            newTaskIds.Add(taskModel2.id);
        }
        TaskChangeNotifier.NotifyTasksCopied(newTaskIds);
        newTaskIds = (List<string>) null;
      }
    }

    public static async Task<TaskModel> CopyTask(
      string taskId,
      bool isDragCopy = false,
      bool notify = true,
      bool delayNotify = false)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
        return (TaskModel) null;
      long sortOrderInProject = ProjectSortOrderDao.GetNextTaskSortOrderInProject(task.projectId, task.sortOrder, task.parentId, true);
      TaskModel copy = new TaskModel(task)
      {
        title = task.title + (isDragCopy ? "" : " " + Utils.GetString("ProjectCopy")),
        sortOrder = sortOrderInProject
      };
      if (!string.IsNullOrEmpty(task.parentId))
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(task.parentId);
        if (thinTaskById != null && thinTaskById.status != 0)
        {
          TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(thinTaskById, thinTaskById.status, false);
        }
      }
      TaskModel copyModel = await ProjectCopyManager.CopyTask(copy, task.projectId, Utils.GetGuid());
      if (copyModel == null)
        return (TaskModel) null;
      List<string> stringList = await ProjectCopyManager.TryCopySubTasks(copy.id, copyModel.id, task.projectId, isDragCopy: isDragCopy);
      if (stringList.Any<string>())
        UtilLog.Info("CopyTaskChildren " + taskId + ",Children " + stringList.Join<string>(";"));
      AttachmentCache.ResetDictItems();
      if (notify)
      {
        stringList.Add(copyModel.id);
        TaskChangeNotifier.NotifyTaskCopied(stringList, delayNotify);
      }
      return copyModel;
    }

    public static async Task SetSnoozeTime(string taskId, DateTime snoozeTime)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.remindTime = new DateTime?(snoozeTime);
        task.modifiedTime = new DateTime?(DateTime.Now);
        TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
        taskViewModel.RemindTime = task.remindTime;
        taskViewModel.ModifiedTime = task.modifiedTime;
        await TaskDao.UpdateTask(task);
        TaskChangeNotifier.NotifyTaskDateChanged(taskId);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        task = (TaskModel) null;
      }
    }

    public static async Task TryMoveProject(MoveProjectArgs args)
    {
      TaskModel task = await TaskDao.GetThinTaskById(args.TaskId);
      List<TaskModel> subTasks;
      if (task == null)
      {
        task = (TaskModel) null;
        subTasks = (List<TaskModel>) null;
      }
      else
      {
        bool sameProject = task.projectId == args.Project.id;
        bool sameColumn = task.columnId == args.ColumnId || string.IsNullOrEmpty(args.ColumnId);
        if (sameColumn & sameProject)
        {
          task = (TaskModel) null;
          subTasks = (List<TaskModel>) null;
        }
        else
        {
          subTasks = await TaskService.GetAllSubTasksByIdAsync(args.TaskId, task.projectId);
          await TaskDao.RemoveTaskParentId(task.id);
          task.parentId = "";
          List<TaskModel> taskModelList = subTasks;
          // ISSUE: explicit non-virtual call
          if ((taskModelList != null ? (__nonvirtual (taskModelList.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            List<string> ids = subTasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>();
            ids.Add(task.id);
            if (!sameProject)
              await TaskService.BatchMoveProject(ids, args);
            if (!sameColumn)
              await TaskService.BatchSaveTasksColumnId(ids, args.ColumnId, args.Project.id);
            ids = (List<string>) null;
          }
          else
          {
            if (!sameProject)
            {
              TaskModel taskModel = await TaskService.MoveProject(args);
              if (taskModel != null)
                task = taskModel;
            }
            if (!sameColumn)
            {
              if (sameProject)
                task.sortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(task.projectId);
              task.columnId = args.ColumnId;
              await TaskService.UpdateTaskColumnId(task);
              TaskChangeNotifier.NotifyTaskProjectChanged(task);
            }
          }
          SyncManager.TryDelaySync(2000);
          task = (TaskModel) null;
          subTasks = (List<TaskModel>) null;
        }
      }
    }

    private static async Task<TaskModel> MoveProject(MoveProjectArgs args)
    {
      if (await ProChecker.CheckTaskLimit(args.Project.id))
        return (TaskModel) null;
      TaskModel task = await TaskDao.GetThinTaskById(args.TaskId);
      if (task != null && task.projectId != args.Project.id)
      {
        string fromProjectId = task.projectId;
        task.deleted = 0;
        task.projectId = args.Project.id;
        task.columnId = string.Empty;
        string oldAssign = task.assignee;
        long oldSort = task.sortOrder;
        if (!args.KeepAssignee)
          task.assignee = "-1";
        task.sortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(args.Project.id, isTop: args.IsTop);
        CommentDao.ChangeCommentProjectId(task, args.Project.id);
        await TaskService.UpdateTaskOnProjectChanged(task);
        if (!string.IsNullOrEmpty(args.UndoId))
        {
          TaskDragUndoModel.AddDragModel(args.UndoId, task.id, (string) null, (string) null, fromProjectId, args.Project.id, oldAssign, oldSort);
        }
        else
        {
          await SyncStatusDao.AddMoveOrRestoreProjectStatus(task.id, fromProjectId);
          if (oldAssign != task.assignee && !string.IsNullOrEmpty(oldAssign))
            await SyncStatusDao.AddModifySyncStatus(task.id);
        }
        if (args.Notify)
          TaskChangeNotifier.NotifyTaskProjectChanged(task);
        fromProjectId = (string) null;
        oldAssign = (string) null;
      }
      return task;
    }

    public static async Task MoveProject(
      string taskId,
      string projectId,
      bool? isTop = null,
      string columnId = null)
    {
      ProjectModel projectById = await ProjectDao.GetProjectById(projectId);
      if (projectById == null)
        return;
      await TaskService.TryMoveProject(new MoveProjectArgs(taskId, projectById)
      {
        IsTop = isTop,
        ColumnId = columnId
      });
    }

    public static async Task ClearAgendaDate(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.isAllDay = new bool?(true);
        task.startDate = new DateTime?();
        task.dueDate = new DateTime?();
        task.reminders = (TaskReminderModel[]) null;
        task.repeatFrom = string.Empty;
        task.repeatFlag = string.Empty;
        DateTime? nullable1 = task.remindTime;
        if (nullable1.HasValue)
        {
          TaskModel taskModel = task;
          nullable1 = new DateTime?();
          DateTime? nullable2 = nullable1;
          taskModel.remindTime = nullable2;
          await ReminderDelayDao.DeleteByIdAsync(task.id, "task");
        }
        task.exDates = string.Empty;
        task.modifiedTime = new DateTime?(DateTime.Now);
        task.attendId = string.Empty;
        task.isFloating = new bool?(false);
        task.timeZone = TimeZoneData.LocalTimeZoneModel.TimeZoneName;
        int num = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
        await TaskService.UpdateTaskOnTimeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        await Communicator.RemoveAgenda(task.projectId, task.id);
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        task = (TaskModel) null;
      }
    }

    public static async Task ClearDate(string taskId, string columnId = "")
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.isAllDay = new bool?(true);
        task.startDate = new DateTime?();
        task.dueDate = new DateTime?();
        task.reminders = (TaskReminderModel[]) null;
        task.repeatFrom = string.Empty;
        task.repeatFlag = string.Empty;
        DateTime? nullable1 = task.remindTime;
        if (nullable1.HasValue)
        {
          TaskModel taskModel = task;
          nullable1 = new DateTime?();
          DateTime? nullable2 = nullable1;
          taskModel.remindTime = nullable2;
          await ReminderDelayDao.DeleteByIdAsync(task.id, "task");
        }
        task.exDates = string.Empty;
        task.modifiedTime = new DateTime?(DateTime.Now);
        task.isFloating = new bool?(false);
        task.timeZone = TimeZoneData.LocalTimeZoneModel.TimeZoneName;
        if (!string.IsNullOrEmpty(columnId))
          task.columnId = columnId;
        if (!string.IsNullOrEmpty(task.attendId))
        {
          task.attendId = "";
          Communicator.RemoveAgenda(task.projectId, taskId);
        }
        int num = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
        await TaskService.UpdateTaskOnTimeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        task = (TaskModel) null;
      }
    }

    public static async Task BatchSetDate(List<string> taskIds, TimeData model)
    {
      List<TaskModel> tasks = await TaskService.GetSelectedTasks(taskIds);
      for (int i = 0; i < tasks.Count; ++i)
      {
        TaskModel task = tasks[i];
        DateTime? nullable1 = task.startDate;
        if (!nullable1.HasValue)
          task.isAllDay = new bool?(true);
        bool flag1 = task.kind == "NOTE";
        bool? isAllDay = task.isAllDay;
        int num1;
        if (!isAllDay.GetValueOrDefault())
        {
          isAllDay = model.IsAllDay;
          num1 = isAllDay.GetValueOrDefault() ? 1 : 0;
        }
        else
          num1 = 0;
        bool flag2 = num1 != 0;
        if (model.ChangedDateOnly)
        {
          if (model.IsTimeUnified)
          {
            task.startDate = model.StartDate;
            TaskModel taskModel = task;
            DateTime? nullable2;
            if (!flag1)
            {
              nullable2 = model.DueDate;
            }
            else
            {
              nullable1 = new DateTime?();
              nullable2 = nullable1;
            }
            taskModel.dueDate = nullable2;
          }
          else
          {
            DateTime? startDate = task.startDate;
            TaskModel taskModel1 = task;
            nullable1 = task.startDate;
            DateTime? nullable3;
            if (nullable1.HasValue)
            {
              nullable1 = model.StartDate;
              if (nullable1.HasValue)
              {
                nullable1 = task.startDate;
                DateTime original = nullable1.Value;
                nullable1 = model.StartDate;
                DateTime modify = nullable1.Value;
                nullable3 = new DateTime?(ticktick_WPF.Util.DateUtils.SetDateOnly(original, modify));
                goto label_17;
              }
            }
            nullable3 = model.StartDate;
label_17:
            taskModel1.startDate = nullable3;
            if (startDate.HasValue)
            {
              nullable1 = task.dueDate;
              if (nullable1.HasValue)
              {
                nullable1 = task.dueDate;
                double totalMinutes = (nullable1.Value - startDate.Value).TotalMinutes;
                TaskModel taskModel2 = task;
                nullable1 = task.startDate;
                ref DateTime? local = ref nullable1;
                DateTime? nullable4 = local.HasValue ? new DateTime?(local.GetValueOrDefault().AddMinutes(totalMinutes)) : new DateTime?();
                taskModel2.dueDate = nullable4;
              }
            }
          }
        }
        else
        {
          task.startDate = model.StartDate;
          TaskModel taskModel = task;
          DateTime? nullable5;
          if (!flag1)
          {
            nullable5 = model.DueDate;
          }
          else
          {
            nullable1 = new DateTime?();
            nullable5 = nullable1;
          }
          taskModel.dueDate = nullable5;
          task.isAllDay = model.IsAllDay;
          task.repeatFrom = flag1 ? (string) null : model.RepeatFrom;
          task.repeatFlag = flag1 ? (string) null : model.RepeatFlag;
          task.remindTime = task.remindTime;
          task.modifiedTime = new DateTime?(DateTime.Now);
          task.exDates = string.Empty;
          task.repeatFlag = TaskService.TryFixRepeatFlag(task.startDate, task.repeatFlag, task.repeatFrom);
          task.isFloating = new bool?(!flag2 && model.TimeZone.IsFloat);
          task.timeZone = flag2 ? TimeZoneData.LocalTimeZoneModel.TimeZoneName : model.TimeZone.TimeZoneName;
          int num2 = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
          List<TaskReminderModel> reminders = new List<TaskReminderModel>();
          if (model.Reminders != null)
          {
            foreach (TaskReminderModel reminder in model.Reminders)
              reminders.Add(new TaskReminderModel()
              {
                Taskid = task._Id,
                taskserverid = task.id,
                id = Utils.GetGuid(),
                trigger = reminder.trigger
              });
            await TaskReminderDao.InsertTaskReminders(reminders);
          }
          task.reminders = reminders.ToArray();
          reminders = (List<TaskReminderModel>) null;
        }
        task = (TaskModel) null;
      }
      await TaskService.BatchSaveTask(tasks, CheckMatchedType.CheckSmart);
      TaskChangeNotifier.NotifyBatchDateChanged(taskIds);
      ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(model);
      tasks = (List<TaskModel>) null;
    }

    public static async Task<TaskDetailItemModel> SetSubtaskDate(
      string taskId,
      string itemId,
      DateTime newDate)
    {
      TaskDetailItemModel item = await TaskDetailItemDao.GetChecklistItemById(itemId);
      if (item != null)
      {
        TaskDetailItemModel taskDetailItemModel = item;
        DateTime? startDate = item.startDate;
        DateTime dateTime;
        if (startDate.HasValue)
        {
          startDate = item.startDate;
          dateTime = ticktick_WPF.Util.DateUtils.SetDateOnly(startDate.Value, newDate);
        }
        else
          dateTime = newDate;
        DateTime? nullable = new DateTime?(dateTime);
        taskDetailItemModel.startDate = nullable;
        await TaskDetailItemDao.SaveChecklistItem(item);
        await SyncStatusDao.AddModifySyncStatus(taskId);
        ItemChangeNotifier.NotifyItemDateChanged(item);
      }
      TaskDetailItemModel taskDetailItemModel1 = item;
      item = (TaskDetailItemModel) null;
      return taskDetailItemModel1;
    }

    public static async Task SetCheckItemDate(string taskId, string itemId, TimeDataModel newDate)
    {
      TaskDetailItemModel item = await TaskDetailItemDao.GetChecklistItemById(itemId);
      if (item == null)
      {
        item = (TaskDetailItemModel) null;
      }
      else
      {
        item.startDate = newDate.StartDate;
        item.isAllDay = newDate.IsAllDay;
        item.snoozeReminderTime = new DateTime?();
        await TaskDetailItemDao.SaveChecklistItem(item);
        await SyncStatusDao.AddModifySyncStatus(taskId);
        ItemChangeNotifier.NotifyItemDateChanged(item);
        ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(new TimeData()
        {
          StartDate = newDate.StartDate,
          DueDate = newDate.DueDate
        });
        item = (TaskDetailItemModel) null;
      }
    }

    public static async Task SetDateTime(TimeDataModel time)
    {
      TaskModel task = await TaskDao.GetThinTaskById(time.TaskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.startDate = time.StartDate;
        task.dueDate = time.DueDate;
        task.isAllDay = time.IsAllDay;
        task.exDates = string.Empty;
        DateTime? nullable1 = task.remindTime;
        if (nullable1.HasValue)
        {
          TaskModel taskModel = task;
          nullable1 = new DateTime?();
          DateTime? nullable2 = nullable1;
          taskModel.remindTime = nullable2;
          await ReminderDelayDao.DeleteByIdAsync(task.id, "task");
        }
        if (task.isAllDay.GetValueOrDefault())
        {
          task.timeZone = TimeZoneData.LocalTimeZoneModel.TimeZoneName;
          task.isFloating = new bool?(false);
        }
        switch (time.HandleReminderMode)
        {
          case ReminderMode.Clear:
            int num = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
            break;
          case ReminderMode.SetDefault:
            await TaskService.SetDefaultReminder(time, task);
            break;
          case ReminderMode.KeepDay:
            await TaskService.KeepTaskDayReminder(time, task);
            break;
        }
        await TaskService.UpdateTaskOnTimeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        nullable1 = time.DueDate;
        if (nullable1.HasValue)
        {
          task = (TaskModel) null;
        }
        else
        {
          ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(new TimeData()
          {
            StartDate = time.StartDate,
            DueDate = time.DueDate
          });
          task = (TaskModel) null;
        }
      }
    }

    private static async Task KeepTaskDayReminder(TimeDataModel time, TaskModel task)
    {
      List<TaskReminderModel> original = await TaskReminderDao.GetRemindersByTaskId(task.id);
      if (original == null)
        original = (List<TaskReminderModel>) null;
      else if (original.Count <= 0)
      {
        original = (List<TaskReminderModel>) null;
      }
      else
      {
        int num1 = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
        List<string> source = new List<string>();
        foreach (TaskReminderModel taskReminderModel in original)
        {
          AdvanceDateModel advanceDateModel = new AdvanceDateModel(taskReminderModel.trigger, true);
          string str = string.Format("TRIGGER:-P{0}D", (object) advanceDateModel.AdvanceDays);
          if (advanceDateModel.AdvanceDays <= 0 && advanceDateModel.Hour <= 0 && advanceDateModel.Minute <= 0)
            str = "TRIGGER:PT0S";
          if (!source.Contains(str))
            source.Add(str);
        }
        if (source.Any<string>())
        {
          foreach (string str in source)
          {
            TaskReminderModel taskReminderModel = new TaskReminderModel();
            taskReminderModel.id = Utils.GetGuid();
            taskReminderModel._Id = task._Id;
            taskReminderModel.taskserverid = task.id;
            taskReminderModel.trigger = str;
            int num2 = await TaskReminderDao.SaveReminders(taskReminderModel);
          }
          original = (List<TaskReminderModel>) null;
        }
        else
        {
          await TaskService.SetDefaultReminder(time, task);
          original = (List<TaskReminderModel>) null;
        }
      }
    }

    private static async Task SetDefaultReminder(TimeDataModel time, TaskModel task)
    {
      int num1 = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
      bool? isAllDay = time.IsAllDay;
      List<TaskReminderModel> taskReminderModelList1;
      if (isAllDay.HasValue)
      {
        isAllDay = time.IsAllDay;
        if (isAllDay.Value)
        {
          taskReminderModelList1 = TimeData.GetDefaultAllDayReminders();
          goto label_5;
        }
      }
      taskReminderModelList1 = TimeData.GetDefaultTimeReminders();
label_5:
      List<TaskReminderModel> taskReminderModelList2 = taskReminderModelList1;
      if (taskReminderModelList2 == null)
        ;
      else if (taskReminderModelList2.Count <= 0)
        ;
      else
      {
        taskReminderModelList2.ForEach((Action<TaskReminderModel>) (reminder =>
        {
          reminder._Id = task._Id;
          reminder.taskserverid = task.id;
        }));
        foreach (TaskReminderModel taskReminderModel in taskReminderModelList2)
        {
          int num2 = await TaskReminderDao.SaveReminders(taskReminderModel);
        }
      }
    }

    public static async Task SetDateAndClearRepeat(
      string taskId,
      DateTime date,
      bool handleReminder = true,
      bool clearRepeat = false)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      if (!thinTaskById.startDate.HasValue)
        thinTaskById.isAllDay = new bool?(true);
      TimeData timeData = new TimeData()
      {
        StartDate = thinTaskById.startDate,
        DueDate = thinTaskById.dueDate,
        IsAllDay = thinTaskById.isAllDay,
        RepeatFlag = clearRepeat ? string.Empty : thinTaskById.repeatFlag,
        RepeatFrom = clearRepeat ? string.Empty : thinTaskById.repeatFrom,
        ExDates = (List<string>) null,
        TimeZone = new TimeZoneViewModel(thinTaskById.Floating, thinTaskById.timeZone)
      };
      DateTime changeDate = date;
      TaskService.ChangeStartDate(ref timeData, changeDate);
      await TaskService.SetDate(taskId, timeData, handleReminder);
    }

    public static async Task SetDate(
      string taskId,
      DateTime newDate,
      bool handleReminder = true,
      string columnId = "",
      bool? isAllDay = null)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      if (!thinTaskById.startDate.HasValue)
        thinTaskById.isAllDay = new bool?(true);
      TimeData timeData = new TimeData()
      {
        StartDate = thinTaskById.startDate,
        DueDate = thinTaskById.dueDate,
        IsAllDay = !isAllDay.HasValue ? thinTaskById.isAllDay : isAllDay,
        RepeatFlag = thinTaskById.repeatFlag,
        RepeatFrom = thinTaskById.repeatFrom,
        ExDates = (List<string>) null,
        TimeZone = new TimeZoneViewModel(thinTaskById.Floating, thinTaskById.timeZone)
      };
      TaskService.ChangeStartDate(ref timeData, newDate);
      await TaskService.SetDate(taskId, timeData, handleReminder, columnId);
    }

    public static TimeData GetChangedData(
      DateTime? startDate,
      DateTime? dueDate,
      bool? isAllDay,
      DateTime newDate,
      string repeatFlag,
      string repeatFrom,
      bool changeTime = false)
    {
      TimeData timeData = new TimeData()
      {
        StartDate = startDate,
        DueDate = dueDate,
        IsAllDay = isAllDay,
        RepeatFlag = repeatFlag,
        RepeatFrom = repeatFrom
      };
      TaskService.ChangeStartDate(ref timeData, newDate, changeTime);
      return timeData;
    }

    public static async Task SetDate(
      string taskId,
      TimeData model,
      bool handleReminder = true,
      string columnId = "",
      bool notify = true)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.isAllDay = model.IsAllDay;
        task.startDate = model.StartDate;
        task.dueDate = model.DueDate;
        task.reminders = model.Reminders?.ToArray();
        task.repeatFrom = model.RepeatFrom;
        task.repeatFlag = model.RepeatFlag;
        task.exDates = ExDateSerilizer.ToString(model.ExDates?.ToArray());
        task.isFloating = new bool?(model.TimeZone.IsFloat);
        task.timeZone = model.TimeZone.TimeZoneName;
        DateTime? nullable1 = task.remindTime;
        if (nullable1.HasValue)
        {
          TaskModel taskModel = task;
          nullable1 = new DateTime?();
          DateTime? nullable2 = nullable1;
          taskModel.remindTime = nullable2;
          await ReminderDelayDao.DeleteByIdAsync(task.id, "task");
        }
        task.modifiedTime = new DateTime?(DateTime.Now);
        if (!string.IsNullOrEmpty(columnId))
          task.columnId = columnId;
        if (handleReminder)
          await TaskService.SaveTaskReminders(task);
        await TaskService.UpdateTaskOnTimeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        if (notify)
          TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        nullable1 = model.DueDate;
        if (nullable1.HasValue)
        {
          task = (TaskModel) null;
        }
        else
        {
          ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(model);
          task = (TaskModel) null;
        }
      }
    }

    public static async Task SaveTaskReminders(TaskModel task)
    {
      int num1 = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
      if (task.reminders == null || task.reminders.Length == 0)
        return;
      HashSet<string> saved = new HashSet<string>();
      TaskReminderModel[] taskReminderModelArray = task.reminders;
      for (int index = 0; index < taskReminderModelArray.Length; ++index)
      {
        TaskReminderModel taskReminderModel = taskReminderModelArray[index];
        if (!saved.Contains(taskReminderModel.trigger))
        {
          saved.Add(taskReminderModel.trigger);
          taskReminderModel.Taskid = task._Id;
          taskReminderModel.taskserverid = task.id;
          int num2 = await TaskReminderDao.SaveReminders(taskReminderModel);
        }
      }
      taskReminderModelArray = (TaskReminderModel[]) null;
      saved = (HashSet<string>) null;
    }

    public static async Task SaveReminders(string taskId, List<TaskReminderModel> reminders)
    {
      int num1 = await TaskReminderDao.DeleteRemindersByTaskId(taskId) ? 1 : 0;
      if (reminders == null || reminders.Count <= 0)
        return;
      foreach (TaskReminderModel reminder in reminders)
      {
        reminder.taskserverid = taskId;
        int num2 = await TaskReminderDao.SaveReminders(reminder);
      }
    }

    private static async Task<List<TaskModel>> GetSelectedTasks(List<string> selectedTaskIds)
    {
      return selectedTaskIds != null && selectedTaskIds.Count > 0 ? await TaskDao.GetThinTasksInBatch(selectedTaskIds) : new List<TaskModel>();
    }

    public static async Task OnCheckItemChanged(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskDao.UpdateTask(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        SyncManager.TryDelaySync();
        task = (TaskModel) null;
      }
    }

    public static async Task SetTags(string taskId, List<string> tags)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.modifiedTime = new DateTime?(DateTime.Now);
        task.tags = tags.ToArray();
        task.tag = TagSerializer.ToJsonContent(tags);
        TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
        viewModel.ModifiedTime = task.modifiedTime;
        viewModel.Tags = task.tags;
        viewModel.Tag = task.tag;
        await TaskDao.UpdateTask(task);
        ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.CheckTag);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        await Task.Delay(50);
        TaskChangeNotifier.NotifyTaskTagsChanged(new TagExtra(taskId, tags));
        viewModel = (TaskBaseViewModel) null;
        task = (TaskModel) null;
      }
    }

    public static async void ReplaceTitleLink(string taskId, string url, string title)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.title = task.title.Replace(url, "[" + title + "](" + url + ")");
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnTitleChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        TaskChangeNotifier.NotifyTaskTextChanged(task.id);
        task = (TaskModel) null;
      }
    }

    public static async Task SaveTaskTitle(string taskId, string title)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
        task = (TaskModel) null;
      else if (!(task.title != title))
      {
        task = (TaskModel) null;
      }
      else
      {
        task.title = title;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnTitleChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        TaskChangeNotifier.NotifyTaskTextChanged(taskId);
        SyncManager.TryDelaySync(3000);
        task = (TaskModel) null;
      }
    }

    public static async Task SaveTaskContent(string taskId, string content)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.content = content;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnContentChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        task = (TaskModel) null;
      }
    }

    public static async Task<TaskModel> AddTaskFromTemplate(
      string templateId,
      AddTaskViewModel taskDefault = null,
      AddTaskExtraInfo extra = null)
    {
      if (taskDefault == null)
        taskDefault = new AddTaskViewModel((IProjectTaskDefault) ProjectIdentity.CreateInboxProject());
      TaskTemplateModel template = await TemplateDao.GetTemplateById(templateId);
      if (template == null)
        return (TaskModel) null;
      TaskModel task = await TaskService.AddTaskFromTemplate(template, taskDefault, extra);
      if (task.kind != "NOTE")
        await TaskService.AddTaskChildrenFromTemplate(template, task.id, taskDefault);
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == task.projectId));
      string str = projectModel == null ? Utils.GetString("Inbox") : projectModel.name;
      Utils.Toast(string.Format(Utils.GetString("AddTaskTo"), (object) str));
      TaskChangeNotifier.NotifyTaskAdded(task);
      return task;
    }

    private static async Task AddTaskChildrenFromTemplate(
      TaskTemplateModel template,
      string taskId,
      AddTaskViewModel taskDefault = null)
    {
      List<TaskTemplateModel> chidlren = template.Chidlren;
      // ISSUE: explicit non-virtual call
      if ((chidlren != null ? (__nonvirtual (chidlren.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      long sortOrder = 0;
      foreach (TaskTemplateModel childTemplate in template.Chidlren)
      {
        TaskModel taskModel = await TaskService.AddTaskFromTemplate(childTemplate, taskDefault, parentId: taskId, sort: new long?(sortOrder));
        sortOrder += 268435456L;
        await TaskService.AddTaskChildrenFromTemplate(childTemplate, taskModel.id, taskDefault);
      }
    }

    private static async Task<TaskModel> AddTaskFromTemplate(
      TaskTemplateModel model,
      AddTaskViewModel taskDefault = null,
      AddTaskExtraInfo extra = null,
      string parentId = null,
      long? sort = null)
    {
      List<string> tags = new List<string>();
      if (model.Tags != null && model.Tags.Any<string>())
        tags.AddRange((IEnumerable<string>) model.Tags);
      if (taskDefault != null && taskDefault.Tags.Any<string>() && string.IsNullOrEmpty(parentId))
      {
        tags.AddRange(taskDefault.Tags.Select<string, string>((Func<string, string>) (tag => tag.Replace("#", string.Empty))));
        tags = tags.Distinct<string>().ToList<string>();
      }
      bool isList = model.Items != null && model.Items.Any<string>();
      bool flag = model.Kind == TemplateKind.Note.ToString();
      TaskModel task = new TaskModel()
      {
        id = Utils.GetGuid(),
        title = model.Title,
        content = model.Content,
        kind = flag ? "NOTE" : (isList ? "CHECKLIST" : "TEXT"),
        desc = model.Desc,
        status = 0,
        projectId = Utils.GetInboxId(),
        modifiedTime = new DateTime?(DateTime.Now),
        createdTime = new DateTime?(DateTime.Now),
        sortOrder = sort.GetValueOrDefault()
      };
      if (taskDefault != null)
      {
        task.projectId = taskDefault.ProjectId;
        task.columnId = taskDefault.ColumnId;
        task.priority = flag || !string.IsNullOrEmpty(parentId) ? 0 : taskDefault.Priority;
        if (taskDefault.TimeData != null && string.IsNullOrEmpty(parentId))
        {
          task.startDate = taskDefault.TimeData.StartDate;
          task.dueDate = flag ? new DateTime?() : taskDefault.TimeData.DueDate;
          task.isAllDay = taskDefault.TimeData.IsAllDay;
          task.repeatFlag = flag ? (string) null : taskDefault.TimeData.RepeatFlag;
          task.repeatFrom = flag ? (string) null : taskDefault.TimeData.RepeatFrom;
          if (taskDefault.TimeData.Reminders != null && taskDefault.TimeData.Reminders.Count > 0)
          {
            task.reminders = taskDefault.TimeData.Reminders.ToArray();
            task.reminderCount = task.reminders?.Length.Value;
            TaskReminderModel[] taskReminderModelArray = task.reminders;
            for (int index = 0; index < taskReminderModelArray.Length; ++index)
            {
              TaskReminderModel taskReminderModel = taskReminderModelArray[index];
              taskReminderModel.id = Utils.GetGuid();
              taskReminderModel.taskserverid = task.id;
              int num = await TaskReminderDao.SaveReminders(taskReminderModel);
            }
            taskReminderModelArray = (TaskReminderModel[]) null;
          }
        }
      }
      if (extra != null)
      {
        if (!string.IsNullOrEmpty(extra.Title.Trim()))
          task.title = extra.Title.Trim();
        task.assignee = extra.AssigneeId;
        if (extra.Tags != null && extra.Tags.Any<string>())
        {
          tags.AddRange(extra.Tags.Select<string, string>((Func<string, string>) (tag => tag.Replace("#", string.Empty))));
          tags = tags.Distinct<string>().ToList<string>();
        }
      }
      if (tags.Any<string>())
      {
        List<string> localTags = CacheManager.GetTags().Select<TagModel, string>((Func<TagModel, string>) (tag => tag.name)).ToList<string>();
        List<string> list = tags.Where<string>((Func<string, bool>) (tag => !localTags.Contains(tag.ToLower()))).ToList<string>();
        if (list.Any<string>())
        {
          foreach (string tag1 in list)
          {
            TagModel tag2 = await TagService.TryCreateTag(tag1);
          }
        }
      }
      task.tag = TagSerializer.ToJsonContent(tags);
      task.parentId = parentId;
      if (taskDefault != null)
      {
        if (taskDefault.IsPin)
          task.pinnedTime = DateTime.Now.ToString(UtcDateTimeConverter.GetConverterValue(DateTime.Now));
        if (taskDefault.IsComplete)
        {
          task.status = 2;
          task.completedTime = new DateTime?(DateTime.Now);
          task.completedUserId = LocalSettings.Settings.LoginUserId;
        }
      }
      if (isList)
      {
        List<string> items = model.Items;
        // ISSUE: explicit non-virtual call
        if ((items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          long sortOrder = 0;
          foreach (string str in model.Items)
          {
            TaskDetailItemModel model1 = new TaskDetailItemModel();
            model1.id = Utils.GetGuid();
            model1.TaskId = 0;
            model1.TaskServerId = task.id;
            model1.title = str;
            model1.sortOrder = sortOrder;
            sortOrder += 268435456L;
            await TaskDetailItemDao.InsertChecklistItem(model1);
          }
        }
      }
      TaskModel taskModel = await TaskService.AddTask(task, !sort.HasValue, false);
      tags = (List<string>) null;
      task = (TaskModel) null;
      return taskModel;
    }

    public static async Task<TaskModel> AddTask(
      TaskModel task,
      bool setSortOrder = true,
      bool notify = true,
      object sender = null)
    {
      if (task == null)
        return (TaskModel) null;
      DateTime? nullable = task.createdTime;
      if (!nullable.HasValue)
        task.createdTime = new DateTime?(DateTime.Now);
      if (task.creator == null)
        task.creator = LocalSettings.Settings.LoginUserId;
      nullable = task.modifiedTime;
      if (!nullable.HasValue)
        task.modifiedTime = new DateTime?(DateTime.Now);
      if (string.IsNullOrEmpty(task.id))
        task.id = Utils.GetGuid();
      if (string.IsNullOrEmpty(task.projectId))
        task.projectId = TaskDefaultDao.GetDefaultSafely().ProjectId;
      if (string.IsNullOrEmpty(task.userId))
        task.userId = Utils.GetCurrentUserStr();
      if (string.IsNullOrEmpty(task.timeZone))
        task.timeZone = TimeZoneData.LocalTimeZoneModel.TimeZoneName;
      if (KanbanHelper.IsProjectInKanbanMode(task.projectId) && string.IsNullOrEmpty(task.columnId))
      {
        TaskModel taskModel = task;
        taskModel.columnId = await ColumnDao.GetProjectDefaultColumnId(task.projectId);
        taskModel = (TaskModel) null;
      }
      TaskModel taskModel1 = await TaskDao.InsertTask(task);
      await SyncStatusDao.AddCreateSyncStatus(task.id);
      UtilLog.Info("TaskService.AddTask : " + task.id);
      if (!string.IsNullOrEmpty(task.parentId))
      {
        TaskDao.AddOrRemoveTaskChildIds(task.parentId, new List<string>()
        {
          task.id
        }, true);
        await SyncStatusDao.AddSetParentSyncStatus(task.id, "");
      }
      if (notify)
        TaskChangeNotifier.NotifyTaskAdded(task);
      return task;
    }

    public static async Task<TaskModel> SaveTask(TaskModel task)
    {
      if (task == null)
        return (TaskModel) null;
      if (string.IsNullOrEmpty(task.userId))
        task.userId = Utils.GetCurrentUserIdInt().ToString();
      if (string.IsNullOrEmpty(task.timeZone))
        task.timeZone = Utils.GetLocalTimeZone();
      await TaskService.UpdateTask(task);
      await SyncStatusDao.AddModifySyncStatus(task.id);
      return task;
    }

    public static async Task DeletePomodoro(
      string pomoId,
      List<PomoTask> pomoTasks,
      bool isPomo,
      bool isDelete = false)
    {
      Dictionary<string, long> taskDict = new Dictionary<string, long>();
      foreach (PomoTask pomoTask in pomoTasks)
      {
        long totalSecond = Utils.GetTotalSecond(pomoTask.StartTime, pomoTask.EndTime);
        if (!string.IsNullOrEmpty(pomoTask.TaskId))
        {
          if (taskDict.ContainsKey(pomoTask.TaskId))
            taskDict[pomoTask.TaskId] += totalSecond;
          else
            taskDict.Add(pomoTask.TaskId, totalSecond);
        }
      }
      foreach (KeyValuePair<string, long> keyValuePair in taskDict)
      {
        KeyValuePair<string, long> kv = keyValuePair;
        TaskModel task = await TaskDao.GetThinTaskById(kv.Key);
        if (task != null)
        {
          long duration = -1L * kv.Value;
          int count = (Math.Abs(duration) >= 300L ? 1 : 0) * (isDelete ? -1 : 1);
          PomodoroSummaryModel pomo = await TaskService.DeletePomoSummary(pomoId, kv.Key, duration, isPomo, count);
          await PomoSummaryDao.SavePomoSummary(pomo);
          task.modifiedTime = new DateTime?(DateTime.Now);
          await TaskDao.UpdateTask(task);
          await SyncStatusDao.AddModifySyncStatus(pomo.taskId);
          task = (TaskModel) null;
          pomo = (PomodoroSummaryModel) null;
          kv = new KeyValuePair<string, long>();
        }
      }
      TaskChangeNotifier.NotifyTaskPomoAdded(taskDict.Keys.ToList<string>());
      taskDict = (Dictionary<string, long>) null;
    }

    private static async Task<PomodoroSummaryModel> DeletePomoSummary(
      string pomoId,
      string taskId,
      long duration,
      bool isPomo,
      int count)
    {
      PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(taskId);
      if (pomoByTaskId == null)
        return new PomodoroSummaryModel()
        {
          id = Utils.GetGuid(),
          userId = LocalSettings.Settings.LoginUserId,
          estimatedPomo = 0,
          EstimatedDuration = 0,
          taskId = taskId
        };
      List<object[]> focuses = pomoByTaskId.focuses;
      object[] objArray = focuses != null ? focuses.FirstOrDefault<object[]>((Func<object[], bool>) (f => f.Length == 3 && f[0] as string == pomoId)) : (object[]) null;
      if (objArray != null)
        pomoByTaskId.focuses.Remove(objArray);
      else if (isPomo)
      {
        pomoByTaskId.count += count;
        pomoByTaskId.count = pomoByTaskId.count > 0 ? pomoByTaskId.count : 0;
        pomoByTaskId.PomoDuration += duration;
        pomoByTaskId.PomoDuration = pomoByTaskId.PomoDuration > 0L ? pomoByTaskId.PomoDuration : 0L;
      }
      else
      {
        pomoByTaskId.StopwatchDuration += duration;
        pomoByTaskId.StopwatchDuration = pomoByTaskId.StopwatchDuration > 0L ? pomoByTaskId.StopwatchDuration : 0L;
      }
      return pomoByTaskId;
    }

    public static async Task UpdatePomodoro(string pomoId, List<PomoTask> pomoTasks, bool isPomo)
    {
      Dictionary<string, long> taskDict = new Dictionary<string, long>();
      Dictionary<string, long> habitDict = new Dictionary<string, long>();
      foreach (PomoTask pomoTask in pomoTasks)
      {
        if (!string.IsNullOrEmpty(pomoTask.TaskId))
        {
          long totalSecond = Utils.GetTotalSecond(pomoTask.StartTime, pomoTask.EndTime);
          if (taskDict.ContainsKey(pomoTask.TaskId))
            taskDict[pomoTask.TaskId] += totalSecond;
          else
            taskDict.Add(pomoTask.TaskId, totalSecond);
        }
        if (!string.IsNullOrEmpty(pomoTask.HabitId) && pomoTask.EndTime > DateTime.Today)
        {
          long totalSecond = Utils.GetTotalSecond(pomoTask.StartTime > DateTime.Today ? pomoTask.StartTime : DateTime.Today, pomoTask.EndTime);
          if (habitDict.ContainsKey(pomoTask.HabitId))
            habitDict[pomoTask.HabitId] += totalSecond;
          else
            habitDict.Add(pomoTask.HabitId, totalSecond);
        }
      }
      KeyValuePair<string, long> kv;
      foreach (KeyValuePair<string, long> keyValuePair in taskDict)
      {
        kv = keyValuePair;
        TaskModel task = await TaskDao.GetThinTaskById(kv.Key);
        if (task != null)
        {
          long duration = kv.Value;
          int count = Math.Abs(duration) >= 300L ? 1 : 0;
          PomodoroSummaryModel pomo = await TaskService.UpdatePomoSummary(pomoId, kv.Key, duration, isPomo, count);
          await PomoSummaryDao.SavePomoSummary(pomo);
          task.modifiedTime = new DateTime?(DateTime.Now);
          await TaskDao.UpdateTask(task);
          await SyncStatusDao.AddModifySyncStatus(pomo.taskId);
          task = (TaskModel) null;
          pomo = (PomodoroSummaryModel) null;
          kv = new KeyValuePair<string, long>();
        }
      }
      TaskChangeNotifier.NotifyTaskPomoAdded(taskDict.Keys.ToList<string>());
      foreach (KeyValuePair<string, long> keyValuePair in habitDict)
      {
        kv = keyValuePair;
        HabitModel habitById = await HabitDao.GetHabitById(kv.Key);
        if (habitById != null && !habitById.IsBoolHabit())
        {
          float num = 0.0f;
          if (FocusConstance.HourUnit.Contains(habitById.Unit.Trim().ToLower()))
            num = (float) kv.Value / 3600f;
          if (FocusConstance.MinuteUnit.Contains(habitById.Unit.Trim().ToLower()))
            num = (float) kv.Value / 60f;
          if ((double) num > 0.0)
            HabitService.CheckInHabit(habitById.Id, DateTime.Today, Math.Round((double) num, 2));
        }
        kv = new KeyValuePair<string, long>();
      }
      taskDict = (Dictionary<string, long>) null;
      habitDict = (Dictionary<string, long>) null;
    }

    private static async Task<PomodoroSummaryModel> GetPomoSummary(
      string taskId,
      long duration,
      bool isPomo,
      int count = 1)
    {
      PomodoroSummaryModel pomodoroSummaryModel = await PomoSummaryDao.GetPomoByTaskId(taskId);
      if (pomodoroSummaryModel == null)
        pomodoroSummaryModel = new PomodoroSummaryModel()
        {
          id = Utils.GetGuid(),
          userId = LocalSettings.Settings.LoginUserId,
          estimatedPomo = 0,
          EstimatedDuration = 0L,
          taskId = taskId
        };
      PomodoroSummaryModel pomoSummary = pomodoroSummaryModel;
      if (isPomo)
      {
        pomoSummary.count += count;
        pomoSummary.count = pomoSummary.count > 0 ? pomoSummary.count : 0;
        pomoSummary.PomoDuration += duration;
        pomoSummary.PomoDuration = pomoSummary.PomoDuration > 0L ? pomoSummary.PomoDuration : 0L;
      }
      else
      {
        pomoSummary.StopwatchDuration += duration;
        pomoSummary.StopwatchDuration = pomoSummary.StopwatchDuration > 0L ? pomoSummary.StopwatchDuration : 0L;
      }
      return pomoSummary;
    }

    private static async Task<PomodoroSummaryModel> UpdatePomoSummary(
      string pomoId,
      string taskId,
      long duration,
      bool isPomo,
      int count)
    {
      bool isDelete = duration < 0L;
      PomodoroSummaryModel pomodoroSummaryModel = await PomoSummaryDao.GetPomoByTaskId(taskId);
      if (pomodoroSummaryModel == null)
      {
        pomodoroSummaryModel = new PomodoroSummaryModel()
        {
          id = Utils.GetGuid(),
          userId = LocalSettings.Settings.LoginUserId,
          estimatedPomo = 0,
          EstimatedDuration = 0L,
          taskId = taskId
        };
        if (isDelete)
          return pomodoroSummaryModel;
      }
      List<object[]> focuses = pomodoroSummaryModel.focuses;
      object[] objArray = focuses != null ? focuses.FirstOrDefault<object[]>((Func<object[], bool>) (f => f.Length == 3 && f[0] as string == pomoId)) : (object[]) null;
      if (isDelete)
      {
        if (objArray != null)
          pomodoroSummaryModel.focuses.Remove(objArray);
        else if (isPomo)
        {
          pomodoroSummaryModel.count += count;
          pomodoroSummaryModel.count = pomodoroSummaryModel.count > 0 ? pomodoroSummaryModel.count : 0;
          pomodoroSummaryModel.PomoDuration += duration;
          pomodoroSummaryModel.PomoDuration = pomodoroSummaryModel.PomoDuration > 0L ? pomodoroSummaryModel.PomoDuration : 0L;
        }
        else
        {
          pomodoroSummaryModel.StopwatchDuration += duration;
          pomodoroSummaryModel.StopwatchDuration = pomodoroSummaryModel.StopwatchDuration > 0L ? pomodoroSummaryModel.StopwatchDuration : 0L;
        }
      }
      else if (objArray != null)
        objArray[2] = (object) duration;
      else
        pomodoroSummaryModel.AddFocuses(new object[3]
        {
          (object) pomoId,
          (object) (!isPomo ? 1 : 0),
          (object) duration
        });
      return pomodoroSummaryModel;
    }

    public static async Task SaveTaskPomoDuration(string focusId, string taskId, int duration)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      thinTaskById.modifiedTime = new DateTime?(DateTime.Now);
      await TaskDao.UpdateTask(thinTaskById);
      await PomoSummaryDao.SavePomoSummary(await TaskService.UpdatePomoSummary(focusId, taskId, (long) duration, true, 0));
      SyncStatusDao.AddModifySyncStatus(taskId);
    }

    public static async Task DeleteAllTasksInProject(string projectId)
    {
      List<TaskModel> tasks = await TaskDao.GetTasksInProjectAsync(projectId);
      if (tasks == null)
        tasks = (List<TaskModel>) null;
      else if (tasks.Count <= 0)
      {
        tasks = (List<TaskModel>) null;
      }
      else
      {
        foreach (TaskModel taskModel in tasks)
          taskModel.deleted = 1;
        await TaskDao.BatchUpdateTasks(tasks.ToList<TaskModel>(), checkMatched: CheckMatchedType.All);
        List<string> list = tasks.Select<TaskModel, string>((Func<TaskModel, string>) (task => task.id)).ToList<string>();
        TaskChangeNotifier.NotifyTaskBatchDeletedChanged(list);
        await SyncStatusDao.BatchAddDeleteSyncStatus((IEnumerable<string>) list);
        tasks = (List<TaskModel>) null;
      }
    }

    public static bool IsEmptyTask(TaskModel fullTaskModel, bool checkChildren = false)
    {
      if (!string.IsNullOrEmpty(fullTaskModel.title) || !string.IsNullOrEmpty(fullTaskModel.content) || fullTaskModel.items != null && ((IEnumerable<TaskDetailItemModel>) fullTaskModel.items).Any<TaskDetailItemModel>())
        return false;
      int result;
      int.TryParse(fullTaskModel.commentCount, out result);
      return result <= 0 && string.IsNullOrEmpty(fullTaskModel.desc);
    }

    public static async Task<TaskModel> TryMergeTask(
      List<string> taskIds,
      string primaryTaskId,
      IProjectTaskDefault taskDefault)
    {
      List<string> topIds = TaskDao.GetTreeTopIds(taskIds, (string) null);
      List<TaskModel> tasks = await TaskDao.GetTaskAndChildrenInBatch(topIds, false);
      List<TaskModel> source = tasks;
      TaskModel primaryTask = source != null ? source.FirstOrDefault<TaskModel>((Func<TaskModel, bool>) (task => task.id == primaryTaskId)) : (TaskModel) null;
      if (primaryTask == null)
        return (TaskModel) null;
      ProjectModel mergeProject = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == primaryTask.projectId));
      string content = string.Format(Utils.GetString("MergeHint"), (object) tasks.Count);
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("Merge"), content, Utils.GetString("Merge"), Utils.GetString("Cancel"));
      customerDialog.Owner = (Window) App.Window;
      bool? nullable = customerDialog.ShowDialog();
      if (!nullable.HasValue || !nullable.Value)
        return (TaskModel) null;
      bool needPin = primaryTask.pinnedTimeStamp > 0L;
      TimeData defaultTimeData = taskDefault.GetTimeData();
      TaskModel taskModel1 = new TaskModel();
      taskModel1.id = Utils.GetGuid();
      taskModel1.title = Utils.GetString("NewMergeTask");
      taskModel1.sortOrder = ProjectSortOrderDao.GetNextTaskSortOrderInProject(mergeProject.id, primaryTask.sortOrder);
      DateTime now;
      string str1;
      if (!needPin)
      {
        str1 = (string) null;
      }
      else
      {
        now = DateTime.Now;
        str1 = now.ToString(UtcDateTimeConverter.GetConverterValue(DateTime.Now));
      }
      taskModel1.pinnedTime = str1;
      taskModel1.columnId = primaryTask.columnId;
      taskModel1.startDate = (DateTime?) defaultTimeData?.StartDate;
      taskModel1.priority = taskDefault.GetPriority();
      taskModel1.tags = taskDefault.GetTags()?.ToArray();
      taskModel1.projectId = primaryTask.projectId;
      taskModel1.isAllDay = new bool?(true);
      taskModel1.timeZone = TimeZoneData.LocalTimeZoneModel.TimeZoneName;
      taskModel1.kind = "TEXT";
      TaskModel parent = taskModel1;
      parent.tag = TagSerializer.ToJsonContent(((IEnumerable<string>) parent.tags).ToList<string>());
      List<string> newChildIds = new List<string>();
      int i = 0;
      foreach (TaskModel task in tasks)
      {
        if (topIds.Contains(task.id) && !string.IsNullOrEmpty(task.parentId))
          await TaskDao.AddOrRemoveTaskChildIds(task.parentId, new List<string>()
          {
            task.id
          }, false);
        await SyncStatusDao.AddSetParentSyncStatus(task.id, task.parentId);
        task.parentId = parent.id;
        newChildIds.Add(task.id);
        task.childrenString = (string) null;
        task.childIds = (List<string>) null;
        task.columnId = parent.columnId;
        task.sortOrder = 268435456L * (long) i;
        ++i;
        await SyncStatusDao.AddModifySyncStatus(task.id);
        if (task.projectId != mergeProject.id)
        {
          await SyncStatusDao.AddMoveProjectStatus(task.id, task.projectId);
          task.projectId = mergeProject.id;
        }
        if (task.pinnedTimeStamp <= 0L & needPin)
        {
          TaskModel taskModel2 = task;
          now = DateTime.Now;
          string str2 = now.ToString(UtcDateTimeConverter.GetConverterValue(DateTime.Now));
          taskModel2.pinnedTime = str2;
        }
      }
      TaskModel taskModel3 = await TaskService.AddTask(parent);
      await TaskDao.BatchUpdateTasks(tasks, checkMatched: CheckMatchedType.All);
      await TaskDao.AddOrRemoveTaskChildIds(parent.id, newChildIds, true);
      List<TaskReminderModel> reminders = defaultTimeData?.Reminders;
      if (reminders != null)
      {
        foreach (TaskReminderModel taskReminderModel in reminders)
        {
          taskReminderModel.taskserverid = parent.id;
          int num = await TaskReminderDao.SaveReminders(taskReminderModel);
        }
      }
      return parent;
    }

    public static async Task<bool> MergeTasks(IEnumerable<TaskModel> tasks)
    {
      if (tasks == null)
        return false;
      HashSet<string> allTaskIdMap = TaskCache.GetAllTaskIdMap();
      List<TaskModel> newTaskList = new List<TaskModel>();
      List<TaskModel> updateTaskList = new List<TaskModel>();
      List<TaskReminderModel> reminderList = new List<TaskReminderModel>();
      List<TaskDetailItemModel> checkListItems = new List<TaskDetailItemModel>();
      List<AttachmentModel> attachments = new List<AttachmentModel>();
      List<PomodoroSummaryModel> pomos = new List<PomodoroSummaryModel>();
      List<SyncStatusModel> allSyncStatus = await SyncStatusDao.GetAllSyncStatus();
      IEnumerable<string> syncStatusEntityIds = allSyncStatus != null ? allSyncStatus.Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (s => s.EntityId)).Distinct<string>() : (IEnumerable<string>) null;
      int index = Utils.GetCurrentUserIdInt();
      string userId = index.ToString();
      foreach (TaskModel task in tasks)
      {
        if (string.IsNullOrEmpty(task.userId))
          task.userId = userId;
        if (!allTaskIdMap.Contains(task.id))
        {
          TaskDao.HandleServerDate(task);
          newTaskList.Add(task);
          if (task.reminders != null)
          {
            TaskReminderModel[] reminders = task.reminders;
            for (index = 0; index < reminders.Length; ++index)
            {
              TaskReminderModel taskReminderModel = reminders[index];
              taskReminderModel.Taskid = task._Id;
              taskReminderModel.taskserverid = task.id;
            }
            reminderList.AddRange((IEnumerable<TaskReminderModel>) task.reminders);
          }
          if (task.items != null)
          {
            TaskDetailItemModel[] items = task.items;
            for (index = 0; index < items.Length; ++index)
            {
              TaskDetailItemModel taskDetailItemModel = items[index];
              taskDetailItemModel.TaskId = task._Id;
              taskDetailItemModel.TaskServerId = task.id;
            }
            checkListItems.AddRange((IEnumerable<TaskDetailItemModel>) task.items);
          }
          if (task.Attachments != null)
          {
            AttachmentModel[] attachments1 = task.Attachments;
            for (index = 0; index < attachments1.Length; ++index)
            {
              AttachmentModel attachmentModel = attachments1[index];
              attachmentModel.taskId = task.id;
              attachmentModel.sync_status = 2.ToString();
            }
            attachments.AddRange((IEnumerable<AttachmentModel>) task.Attachments);
          }
          if (task.FocusSummaries != null)
          {
            PomodoroSummaryModel[] focusSummaries = task.FocusSummaries;
            for (index = 0; index < focusSummaries.Length; ++index)
              focusSummaries[index].taskId = task.id;
            pomos.AddRange((IEnumerable<PomodoroSummaryModel>) task.FocusSummaries);
          }
          List<string> childIds = task.childIds;
          // ISSUE: explicit non-virtual call
          if ((childIds != null ? (__nonvirtual (childIds.Count) > 0 ? 1 : 0) : 0) != 0)
            task.childrenString = JsonConvert.SerializeObject((object) task.childIds);
        }
        else
        {
          TaskModel taskById = await TaskDao.GetTaskById(task.id);
          if (taskById != null && taskById.deleted != task.deleted && (syncStatusEntityIds == null || !syncStatusEntityIds.Contains<string>(task.id)))
          {
            taskById.deleted = task.deleted;
            taskById.modifiedTime = task.modifiedTime;
            updateTaskList.Add(taskById);
          }
        }
      }
      if (newTaskList.Count > 0)
      {
        await TaskDao.BatchInsertTasks(newTaskList);
        TaskSyncedJsonBean bean = new TaskSyncedJsonBean();
        bean.Added.AddRange((IEnumerable<TaskModel>) newTaskList);
        await TaskSyncedJsonDao.SaveTaskSyncedJsons(bean);
      }
      if (updateTaskList.Count > 0)
        await TaskDao.BatchUpdateTasks(updateTaskList);
      if (attachments.Count > 0)
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) attachments);
      }
      if (reminderList.Count > 0)
      {
        int num2 = await App.Connection.InsertAllAsync((IEnumerable) reminderList);
      }
      if (checkListItems.Count > 0)
        await TaskDetailItemDao.BatchInsertChecklists(checkListItems);
      if (pomos.Count > 0)
      {
        int num3 = await App.Connection.InsertAllAsync((IEnumerable) pomos);
      }
      return newTaskList.Count > 0 || updateTaskList.Count > 0;
    }

    public static async Task<List<TaskDetailItemModel>> TaskToSubtask(
      List<TaskModel> fromTasks,
      string toTaskId,
      string targetItemId,
      bool topOrBottom)
    {
      if (fromTasks == null || fromTasks.Count == 0)
        return (List<TaskDetailItemModel>) null;
      TaskModel toTask = await TaskDao.GetThinTaskById(toTaskId);
      Dictionary<string, long> itemSortOrderInTask = await TaskDetailItemDao.GetCheckItemSortOrderInTask(new List<string>()
      {
        toTaskId
      });
      List<long> orders = itemSortOrderInTask.Values.ToList<long>();
      long limits = LimitCache.GetLimitByKey(Constants.LimitKind.SubtaskNumber);
      long secondOrder;
      long firstOrder;
      if (orders.Count == 0)
      {
        firstOrder = 0L;
        secondOrder = 268435456L;
      }
      else
      {
        firstOrder = itemSortOrderInTask.ContainsKey(targetItemId) ? itemSortOrderInTask[targetItemId] : itemSortOrderInTask.Values.Min();
        if (topOrBottom)
        {
          orders = orders.OrderByDescending<long, long>((Func<long, long>) (o => o)).ToList<long>();
          secondOrder = orders.FirstOrDefault<long>((Func<long, bool>) (o => o < firstOrder));
          if (secondOrder == orders.Min() && secondOrder == firstOrder || secondOrder >= firstOrder)
            secondOrder = firstOrder - 268435456L;
        }
        else
        {
          orders = orders.OrderBy<long, long>((Func<long, long>) (o => o)).ToList<long>();
          secondOrder = orders.FirstOrDefault<long>((Func<long, bool>) (o => o > firstOrder));
          if (secondOrder == orders.Max() && secondOrder == firstOrder || secondOrder <= firstOrder)
            secondOrder = firstOrder + 268435456L;
        }
      }
      if (firstOrder > secondOrder)
      {
        long num = firstOrder;
        firstOrder = secondOrder;
        secondOrder = num;
      }
      List<TaskDetailItemModel> subItems = new List<TaskDetailItemModel>();
      for (int i = 0; i < fromTasks.Count; ++i)
      {
        List<TaskDetailItemModel> checkItems = new List<TaskDetailItemModel>();
        TaskModel fromTask = fromTasks[i];
        long sortOrder = firstOrder + (secondOrder - firstOrder) * (long) (i + 1) / (long) (fromTasks.Count + 1);
        TaskDetailItemModel taskDetailItemModel = new TaskDetailItemModel()
        {
          id = Utils.GetGuid(),
          TaskId = toTask._Id,
          TaskServerId = toTaskId,
          title = fromTask.title,
          status = 0,
          sortOrder = sortOrder
        };
        checkItems.Add(taskDetailItemModel);
        if (fromTask.startDate.HasValue && UserDao.IsPro())
        {
          taskDetailItemModel.startDate = fromTask.startDate;
          taskDetailItemModel.isAllDay = fromTask.isAllDay;
        }
        if (fromTask.kind == "CHECKLIST")
        {
          if (!string.IsNullOrEmpty(fromTask.desc))
            checkItems.Add(new TaskDetailItemModel()
            {
              id = Utils.GetGuid(),
              TaskId = toTask._Id,
              TaskServerId = toTaskId,
              title = fromTask.desc,
              status = 0,
              sortOrder = sortOrder + 1L
            });
          List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(fromTasks[i].id);
          if (checkItemsByTaskId != null && checkItemsByTaskId.Any<TaskDetailItemModel>())
            checkItems.AddRange((IEnumerable<TaskDetailItemModel>) checkItemsByTaskId.OrderBy<TaskDetailItemModel, long>((Func<TaskDetailItemModel, long>) (item => item.sortOrder)).ToList<TaskDetailItemModel>().Select<TaskDetailItemModel, TaskDetailItemModel>((Func<TaskDetailItemModel, int, TaskDetailItemModel>) ((originalItem, index) => new TaskDetailItemModel()
            {
              id = Utils.GetGuid(),
              TaskServerId = toTaskId,
              title = originalItem.title,
              status = originalItem.status,
              sortOrder = sortOrder + (long) i + 1L,
              startDate = originalItem.startDate,
              isAllDay = originalItem.isAllDay
            })).ToList<TaskDetailItemModel>());
        }
        else if (!string.IsNullOrEmpty(fromTask.content))
          checkItems.Add(new TaskDetailItemModel()
          {
            id = Utils.GetGuid(),
            TaskId = toTask._Id,
            TaskServerId = toTaskId,
            title = fromTask.content,
            status = 0,
            sortOrder = sortOrder + 1L
          });
        subItems.AddRange((IEnumerable<TaskDetailItemModel>) checkItems);
        if ((long) (subItems.Count + orders.Count) > limits)
        {
          ProChecker.ShowUpgradeDialog(ProType.MoreSubTasks);
          return (List<TaskDetailItemModel>) null;
        }
        checkItems = (List<TaskDetailItemModel>) null;
      }
      await SyncStatusDao.AddModifySyncStatus(toTaskId);
      await TaskDetailItemDao.BatchInsertChecklists(subItems);
      int num1 = await TaskService.BatchDeleteTasks(fromTasks, false) ? 1 : 0;
      return subItems;
    }

    internal static async Task<int> GetQuadrantTaskBelong(string taskId, int previous)
    {
      TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
      if (taskById == null)
        return 0;
      List<QuadrantModel> list = LocalSettings.Settings.MatrixModel.quadrants.ToList<QuadrantModel>();
      string previousId = "quadrant" + previous.ToString();
      list.Sort((Comparison<QuadrantModel>) ((a, b) =>
      {
        if (a.id == previousId)
          return -1;
        return b.id == previousId && a.id != previousId ? 1 : 0;
      }));
      foreach (QuadrantModel quadrantModel in list)
      {
        List<TaskBaseViewModel> matchedNormalFilter = TaskService.GetTaskMatchedNormalFilter(ticktick_WPF.Util.Filter.Parser.ToNormalModel(quadrantModel.rule), new List<TaskBaseViewModel>()
        {
          taskById
        }, true);
        // ISSUE: explicit non-virtual call
        if ((matchedNormalFilter != null ? (__nonvirtual (matchedNormalFilter.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          int result;
          return int.TryParse(quadrantModel.id.Substring(8), out result) ? result : 0;
        }
      }
      return 0;
    }

    public static async Task<TaskModel> SubtaskToTask(
      string checkItemId,
      TaskPrimaryProperty model,
      string columnId = "")
    {
      TaskDetailItemModel checkitem = await TaskDetailItemDao.GetChecklistItemById(checkItemId);
      if (checkitem == null)
        return (TaskModel) null;
      TaskModel taskModel = new TaskModel()
      {
        id = Utils.GetGuid(),
        title = checkitem.title,
        projectId = model.ProjectId,
        status = model.TaskStatus,
        priority = model.Priority.GetValueOrDefault(),
        sortOrder = model.SortOrder,
        assignee = model.AssigneeId,
        kind = "TEXT",
        parentId = model.ParentId,
        columnId = columnId,
        createdTime = new DateTime?(DateTime.Now)
      };
      if (model.Tags != null && model.Tags.Count > 0)
        taskModel.tag = TagSerializer.ToJsonContent(model.Tags.ToList<string>());
      TimeData timeData = model.TimeData;
      if ((timeData != null ? (timeData.StartDate.HasValue ? 1 : 0) : 0) != 0)
      {
        taskModel.startDate = model.TimeData.StartDate;
        taskModel.isAllDay = model.TimeData.IsAllDay;
        taskModel.dueDate = model.TimeData.DueDate;
        taskModel.repeatFlag = model.TimeData.RepeatFlag;
        taskModel.repeatFrom = model.TimeData.RepeatFrom;
        taskModel.reminders = model.TimeData.Reminders?.ToArray();
      }
      if (model.TimeData?.TimeZone != null)
      {
        taskModel.isFloating = new bool?(model.TimeData.TimeZone.IsFloat);
        taskModel.timeZone = model.TimeData.TimeZone.TimeZoneName;
      }
      if (taskModel.status != 0)
        taskModel.completedTime = new DateTime?(DateTime.Now);
      TaskModel taskModel1 = await TaskService.AddTask(taskModel);
      await TaskDetailItemDao.DeleteById(checkitem.id);
      CheckItemCompleteExtra itemCompleteExtra = await TaskService.SyncTaskChange(checkitem);
      if (itemCompleteExtra != null)
        TaskService.UpdateTask(itemCompleteExtra.OriginalTask);
      ItemChangeNotifier.NotifyItemDeleted(checkitem);
      if (taskModel.reminders != null && ((IEnumerable<TaskReminderModel>) taskModel.reminders).Any<TaskReminderModel>() && !Utils.IsEmptyDate(taskModel.startDate))
      {
        List<TaskReminderModel> list = ((IEnumerable<TaskReminderModel>) taskModel.reminders).ToList<TaskReminderModel>();
        list.ForEach((Action<TaskReminderModel>) (reminder => reminder.taskserverid = taskModel.id));
        await TaskReminderDao.BatchAddTaskReminders((IEnumerable<TaskReminderModel>) list);
      }
      return taskModel;
    }

    public static async Task<TaskModel> SetTaskMultiProperty(TaskModel task, IDroppable droppable)
    {
      string projectId = task.projectId;
      if (!string.IsNullOrEmpty(droppable.ProjectId))
        task.projectId = droppable.ProjectId;
      task.priority = droppable.Priority;
      if (droppable.Tags != null && droppable.Tags.Any<string>())
        task.tag = TagSerializer.ToJsonContent(TagSerializer.ToTags(task.tag).Union<string>((IEnumerable<string>) droppable.Tags).ToList<string>());
      DateTime? nullable1 = droppable.DefaultDate;
      if (nullable1.HasValue)
      {
        TaskModel taskModel1 = task;
        nullable1 = droppable.DefaultDate;
        DateTime? nullable2 = new DateTime?(nullable1.Value);
        taskModel1.startDate = nullable2;
        task.reminders = TimeData.GetDefaultAllDayReminders().ToArray();
        task.isAllDay = new bool?(true);
        TaskModel taskModel2 = task;
        nullable1 = new DateTime?();
        DateTime? nullable3 = nullable1;
        taskModel2.dueDate = nullable3;
      }
      await TaskService.SetTaskProperties(task, projectId);
      return task;
    }

    public static async Task SetTaskProperties(TaskModel task, string fromProjectId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(task.id);
      if (thinTaskById == null)
        return;
      task._Id = thinTaskById._Id;
      await TaskService.UpdateTask(task);
      await SyncStatusDao.AddMoveProjectStatus(task.id, fromProjectId);
      await SyncStatusDao.AddModifySyncStatus(task.id);
      TaskChangeNotifier.NotifyTaskProjectChanged(task);
    }

    public static async Task SetItemTitle(string taskId, string subtaskId, string text)
    {
      TaskDetailItemModel checklistItemById = await TaskDetailItemDao.GetChecklistItemById(subtaskId);
      if (checklistItemById == null || !(checklistItemById.title != text))
        return;
      checklistItemById.title = text;
      await TaskDetailItemDao.TrySaveChecklistItem(checklistItemById, false);
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById != null)
      {
        thinTaskById.modifiedTime = new DateTime?(DateTime.Now);
        await TaskDao.UpdateTask(thinTaskById);
      }
      await SyncStatusDao.AddModifySyncStatus(taskId);
      SyncManager.TryDelaySync(3000);
    }

    public static async Task SaveTaskColumnId(string taskId, string columnId, bool checkEmpty = false)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
        ;
      else if (checkEmpty && !string.IsNullOrEmpty(task.columnId))
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(task.id);
        if (taskById == null)
          ;
        else
          taskById.ColumnId = task.columnId;
      }
      else
      {
        task.columnId = columnId;
        task.modifiedTime = new DateTime?(DateTime.Now);
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == task.projectId));
        if (projectModel != null && projectModel.sortType == Constants.SortType.sortOrder.ToString())
          task.sortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(task.projectId);
        await TaskService.UpdateTaskColumnId(task);
        List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(taskId, task.projectId);
        // ISSUE: explicit non-virtual call
        if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
        {
          foreach (TaskModel task1 in subTasksByIdAsync)
          {
            task1.columnId = columnId;
            task1.modifiedTime = new DateTime?(DateTime.Now);
            await TaskService.UpdateTaskColumnId(task1);
          }
        }
        DataChangedNotifier.NotifyColumnChanged(task.projectId);
      }
    }

    public static async Task BatchSaveTasksColumnId(
      List<string> taskIds,
      string columnId,
      string projectId)
    {
      List<TaskBaseViewModel> models = TaskCache.GetTaskAndChildrenInBatch(taskIds, projectId: projectId);
      long sortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(projectId, isTop: new bool?(false));
      models.Sort((Comparison<TaskBaseViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      List<TaskBaseViewModel> taskBaseViewModelList = models;
      // ISSUE: explicit non-virtual call
      if ((taskBaseViewModelList != null ? (__nonvirtual (taskBaseViewModelList.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        for (int i = 0; i < models.Count; ++i)
        {
          TaskModel thinTaskById = await TaskDao.GetThinTaskById(models[i].Id);
          if (thinTaskById != null)
          {
            thinTaskById.columnId = columnId;
            thinTaskById.modifiedTime = new DateTime?(DateTime.Now);
            if (string.IsNullOrEmpty(thinTaskById.parentId))
              thinTaskById.sortOrder = sortOrder + 268435456L / (long) models.Count * (long) i;
            await TaskService.UpdateTaskColumnId(thinTaskById);
          }
        }
      }
      TaskChangeNotifier.NotifyTaskBatchChanged(taskIds);
      models = (List<TaskBaseViewModel>) null;
    }

    public static async Task<bool> AdjustKanbanData(string projectId)
    {
      List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(projectId);
      if (columnsByProjectId != null && columnsByProjectId.Any<ColumnModel>())
      {
        string defaultColumnId = columnsByProjectId.Select<ColumnModel, string>((Func<ColumnModel, string>) (column => column.id)).ToList<string>()[0];
        List<TaskBaseViewModel> columnTaskInProject = TaskCache.GetEmptyColumnTaskInProject(projectId);
        if (columnTaskInProject != null && columnTaskInProject.Any<TaskBaseViewModel>())
        {
          foreach (TaskBaseViewModel taskBaseViewModel in columnTaskInProject)
          {
            TaskModel taskById = await TaskDao.GetTaskById(taskBaseViewModel.Id);
            taskById.columnId = defaultColumnId;
            taskById.modifiedTime = new DateTime?(DateTime.Now);
            TaskService.UpdateTaskColumnId(taskById);
          }
          SyncManager.TryDelaySync();
          return true;
        }
        defaultColumnId = (string) null;
      }
      return false;
    }

    public static async Task SaveCommentCount(string taskId, bool sync = true)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        List<CommentModel> commentsByTaskId = await CommentDao.GetCommentsByTaskId(taskId);
        TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
        string commentCount = task.commentCount;
        int count = commentsByTaskId.Count;
        string str1 = count.ToString();
        if (commentCount != str1)
        {
          TaskModel taskModel = task;
          count = commentsByTaskId.Count;
          string str2 = count.ToString();
          taskModel.commentCount = str2;
          task.modifiedTime = new DateTime?(DateTime.Now);
          taskViewModel.CommentCount = task.commentCount;
          taskViewModel.ModifiedTime = task.modifiedTime;
          await TaskDao.UpdateTask(task);
          TaskChangeNotifier.NotifyTaskTextChanged(task.id);
          if (sync)
            await SyncStatusDao.AddModifySyncStatus(task.id);
          SyncManager.TryDelaySync();
          task = (TaskModel) null;
        }
        else
        {
          taskViewModel.NotifyCommentsChanged();
          task = (TaskModel) null;
        }
      }
    }

    public static async Task<bool> PullTasksOfProject(string projectId)
    {
      if (TaskService.TaskPullingProjectIds.Contains(projectId))
        return false;
      TaskService.TaskPullingProjectIds.Add(projectId);
      try
      {
        List<string> addStatus = (await SyncStatusDao.GetSyncStatusByType(4)).Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (status => status.EntityId)).ToList<string>();
        List<string> moveStatus = (await SyncStatusDao.GetSyncStatusByType(2)).Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (status => status.EntityId)).ToList<string>();
        List<string> restoreStatus = (await SyncStatusDao.GetSyncStatusByType(7)).Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (status => status.EntityId)).ToList<string>();
        ObservableCollection<TaskModel> tasks = await Communicator.PullServerTasksByProjectId(projectId);
        if (tasks == null)
          return false;
        List<TaskModel> tasksInProjectAsync = await TaskDao.GetTasksInProjectAsync(projectId);
        HashSet<string> remoteIds = new HashSet<string>();
        List<TaskModel> needAdded = new List<TaskModel>();
        foreach (TaskModel taskModel in (Collection<TaskModel>) tasks)
        {
          remoteIds.Add(taskModel.id);
          if (!TaskCache.ExistTask(taskModel.id))
            needAdded.Add(taskModel);
        }
        List<TaskModel> list = tasksInProjectAsync != null ? tasksInProjectAsync.Where<TaskModel>((Func<TaskModel, bool>) (task => task.status == 0 && task.deleted == 0 && !remoteIds.Contains(task.id))).ToList<TaskModel>() : (List<TaskModel>) null;
        bool changed = false;
        if (list != null && list.Any<TaskModel>())
        {
          foreach (TaskModel taskModel in list)
          {
            if (!addStatus.Contains(taskModel.id) && !moveStatus.Contains(taskModel.id) && !restoreStatus.Contains(taskModel.id))
            {
              changed = true;
              await TaskDao.DeleteTaskInDb(taskModel.id);
            }
          }
        }
        if (needAdded.Count > 0)
        {
          changed = true;
          int num = await TaskService.MergeTasks((IEnumerable<TaskModel>) needAdded) ? 1 : 0;
        }
        return changed;
      }
      finally
      {
        TaskService.TaskPullingProjectIds.Remove(projectId);
      }
    }

    public static async Task ChangeCompleteDate(string taskId, DateTime date)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null || thinTaskById.status == 0)
        return;
      thinTaskById.completedTime = new DateTime?(date);
      thinTaskById.modifiedTime = new DateTime?(DateTime.Now);
      TaskModel taskModel = await TaskService.SaveTask(thinTaskById);
    }

    public static async Task TryLoadTaskChildren(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null || thinTaskById.deleted != 0)
        return;
      await TaskService.LoadTaskChildren(thinTaskById.id, thinTaskById.projectId);
    }

    private static async Task LoadTaskChildren(string taskId, string projectId, int count = 0)
    {
      if (count > 5)
        return;
      bool flag = false;
      Node<TaskBaseViewModel> taskNode = TaskCache.GetTaskNode(taskId, projectId);
      if (taskNode == null)
        return;
      List<Node<TaskBaseViewModel>> nodeList = taskNode?.GetAllChildrenNode() ?? new List<Node<TaskBaseViewModel>>();
      nodeList.Insert(0, taskNode);
      foreach (Node<TaskBaseViewModel> node in nodeList)
      {
        List<string> stringList = node.Value?.ChildrenIds == null ? (List<string>) null : JsonConvert.DeserializeObject<List<string>>(node.Value.ChildrenIds);
        if (stringList != null)
        {
          foreach (string str in stringList)
          {
            string id = str;
            if (node.Children == null || !node.Children.Exists((Predicate<Node<TaskBaseViewModel>>) (n => n.NodeId == id)))
            {
              if (TaskCache.ExistTask(id))
              {
                TaskBaseViewModel taskById = TaskCache.GetTaskById(id);
                if (taskById.ProjectId == projectId && taskById.Deleted == 0)
                  continue;
              }
              await TaskService.PullChildren(await TaskDao.GetTaskById(node.Value.Id));
              flag = true;
              break;
            }
          }
        }
      }
      if (!flag)
        return;
      await Task.Delay(50);
      await TaskService.LoadTaskChildren(taskId, projectId, ++count);
    }

    private static async Task PullChildren(TaskModel task)
    {
      TaskModel remote;
      if (task == null)
        remote = (TaskModel) null;
      else if (task.deleted != 0)
      {
        remote = (TaskModel) null;
      }
      else
      {
        remote = await Communicator.GetTask(task.id, task.projectId, true);
        if (remote == null)
          remote = (TaskModel) null;
        else if (!(remote.id == task.id))
        {
          remote = (TaskModel) null;
        }
        else
        {
          List<TaskModel> children = remote.children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            int num = await TaskService.MergeTasks((IEnumerable<TaskModel>) remote.children) ? 1 : 0;
          }
          task.parentId = remote.parentId;
          TaskModel taskModel = task;
          List<string> childIds = remote.childIds;
          // ISSUE: explicit non-virtual call
          string str = (childIds != null ? (__nonvirtual (childIds.Count) > 0 ? 1 : 0) : 0) != 0 ? JsonConvert.SerializeObject((object) remote.childIds) : (string) null;
          taskModel.childrenString = str;
          await TaskService.UpdateTaskParent(task);
          remote = (TaskModel) null;
        }
      }
    }

    public static async Task SwitchTaskOrNote(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        if (task.kind == "NOTE")
        {
          task.kind = "TEXT";
        }
        else
        {
          if (task.kind == "CHECKLIST")
          {
            List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
            task.content = ChecklistUtils.Items2Text(task.desc, checkItemsByTaskId != null ? checkItemsByTaskId.OrderBy<TaskDetailItemModel, long>((Func<TaskDetailItemModel, long>) (c => c.sortOrder)).Select<TaskDetailItemModel, string>((Func<TaskDetailItemModel, string>) (s => s.title)).ToList<string>() : (List<string>) null);
            TaskModel taskModel = task;
            taskModel.content = await AttachmentDao.AddAttachmentStrings(task.id, task.content);
            taskModel = (TaskModel) null;
            task.desc = "";
          }
          task.dueDate = new DateTime?();
          task.kind = "NOTE";
          task.status = 0;
          task.completedTime = new DateTime?();
          task.priority = 0;
          task.progress = new int?(0);
          task.repeatFlag = (string) null;
          task.repeatFrom = (string) null;
        }
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTask(task);
        await SyncStatusDao.AddModifySyncStatus(taskId);
        TaskChangeNotifier.NotifyTaskKindChanged(new List<string>()
        {
          taskId
        });
        task = (TaskModel) null;
      }
    }

    public static async Task GetRemoteTaskCheckItems(string projectId, string taskId)
    {
      UtilLog.Info("CheckItemsNotFound TaskId: " + taskId);
      List<SyncStatusModel> syncStatusByType = await SyncStatusDao.GetSyncStatusByType(4);
      TaskModel remote;
      if ((syncStatusByType != null ? new bool?(syncStatusByType.Any<SyncStatusModel>((Func<SyncStatusModel, bool>) (status => status.EntityId == taskId))) : new bool?()).GetValueOrDefault())
        remote = (TaskModel) null;
      else if (TaskService._checkItemPulledIds.Contains(taskId))
      {
        remote = (TaskModel) null;
      }
      else
      {
        TaskService._checkItemPulledIds.Add(taskId);
        remote = await Communicator.GetTask(taskId, projectId);
        if (remote == null)
        {
          SyncStatusDao.DeleteSyncStatus(taskId, 0);
          remote = (TaskModel) null;
        }
        else if (string.IsNullOrEmpty(remote.id))
        {
          TaskModel taskById = await TaskDao.GetTaskById(taskId);
          if (taskById == null)
          {
            remote = (TaskModel) null;
          }
          else
          {
            taskById.kind = "TEXT";
            await TaskService.UpdateTaskOnKindChanged(taskById);
            remote = (TaskModel) null;
          }
        }
        else
        {
          TaskDetailItemModel[] items = remote.items;
          if ((items != null ? (items.Length != 0 ? 1 : 0) : 0) != 0)
          {
            foreach (TaskDetailItemModel taskDetailItemModel in remote.items)
            {
              if (!string.IsNullOrEmpty(taskDetailItemModel.serverStartDate))
              {
                if (taskDetailItemModel.serverStartDate != "-1")
                {
                  try
                  {
                    taskDetailItemModel.startDate = new DateTime?(DateTime.Parse(taskDetailItemModel.serverStartDate));
                    bool? isAllDay = taskDetailItemModel.isAllDay;
                    if (isAllDay.HasValue)
                    {
                      isAllDay = taskDetailItemModel.isAllDay;
                      if (isAllDay.Value)
                        goto label_20;
                    }
                    if (!remote.Floating)
                      continue;
label_20:
                    taskDetailItemModel.startDate = new DateTime?(TimeZoneUtils.LocalToTargetTzTime(taskDetailItemModel.startDate.Value, remote.timeZone));
                  }
                  catch (Exception ex)
                  {
                  }
                }
              }
            }
            await TaskDao.SaveCheckItems(remote);
          }
          TaskModel taskById = await TaskDao.GetTaskById(taskId);
          if (taskById == null)
          {
            remote = (TaskModel) null;
          }
          else
          {
            taskById.kind = remote.kind;
            taskById.deleted = remote.deleted;
            taskById.projectId = remote.projectId;
            await TaskService.UpdateTask(taskById);
            remote = (TaskModel) null;
          }
        }
      }
    }

    public static async Task CheckAttachmentPath(
      string projectId,
      string taskId,
      List<AttachmentModel> local)
    {
      UtilLog.Info("AttachmentPath is null TaskId: " + taskId);
      TaskModel task;
      if (local == null)
        task = (TaskModel) null;
      else if (TaskService._pulledAttachmentTaskId.Contains(taskId))
      {
        task = (TaskModel) null;
      }
      else
      {
        TaskService._pulledAttachmentTaskId.Add(taskId);
        task = await Communicator.GetTask(taskId, projectId);
        if (task == null)
          task = (TaskModel) null;
        else if (string.IsNullOrEmpty(task.id))
        {
          task = (TaskModel) null;
        }
        else
        {
          AttachmentModel[] attachments = task.Attachments;
          if ((attachments != null ? (attachments.Length != 0 ? 1 : 0) : 0) == 0)
          {
            task = (TaskModel) null;
          }
          else
          {
            foreach (AttachmentModel attachmentModel1 in local)
            {
              AttachmentModel attachment = attachmentModel1;
              if (string.IsNullOrEmpty(attachment.path))
              {
                AttachmentModel attachmentModel2 = ((IEnumerable<AttachmentModel>) task.Attachments).FirstOrDefault<AttachmentModel>((Func<AttachmentModel, bool>) (a => a.id == attachment.id));
                if (attachmentModel2 != null)
                {
                  attachment.path = attachmentModel2.path;
                  await AttachmentDao.UpdateAttachment(attachment);
                }
              }
            }
            task = (TaskModel) null;
          }
        }
      }
    }

    public static async Task SetImageMode(string taskId, int imageMode)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      task.imgMode = imageMode;
      task.modifiedTime = new DateTime?(DateTime.Now);
      await TaskService.UpdateTaskOnImgModeChanged(task);
      await SyncStatusDao.AddModifySyncStatus(task.id);
      SyncManager.TryDelaySync();
      task = (TaskModel) null;
    }

    public static async Task TogglesStarred(
      string taskId,
      string projectId,
      bool? isPin = null,
      long? sortOrder = null,
      bool isInDetail = false,
      bool notify = true)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      TaskPinExtra notifyExt = new TaskPinExtra()
      {
        Ids = new List<string>(),
        IsPin = isPin.GetValueOrDefault()
      };
      DateTime modifyTime = DateTime.Now;
      if (task == null)
      {
        task = (TaskModel) null;
        notifyExt = (TaskPinExtra) null;
      }
      else if (!task.CheckEnable())
      {
        task = (TaskModel) null;
        notifyExt = (TaskPinExtra) null;
      }
      else
      {
        if (task.pinnedTimeStamp <= 0L || isPin.HasValue)
        {
          bool? nullable = isPin;
          bool flag = false;
          if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
          {
            notifyExt.IsPin = true;
            long pinnedTime = Utils.GetNowTimeStamp();
            task.pinnedTimeStamp = pinnedTime;
            if (string.IsNullOrEmpty(projectId))
              projectId = task.projectId ?? string.Empty;
            if (sortOrder.HasValue)
            {
              SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertOrUpdateAsync("taskPinned", projectId, taskId, sortOrder: sortOrder);
            }
            else
              await TaskSortOrderService.DeleteAllAsync("taskPinned", projectId, new List<string>()
              {
                taskId
              });
            notifyExt.Ids.Add(task.id);
            await ToggleSubTasks(notifyExt, await TaskService.GetAllSubTasksByIdAsync(task.id, task.projectId), modifyTime, pinnedTime);
            goto label_18;
          }
        }
        notifyExt.IsPin = false;
        task.pinnedTimeStamp = -1L;
        notifyExt.Ids.Add(task.id);
        await ToggleSubTasks(notifyExt, await TaskService.GetAllSubTasksByIdAsync(task.id, task.projectId), modifyTime, -1L);
label_18:
        task.modifiedTime = new DateTime?(modifyTime);
        await TaskService.UpdateTaskPinned(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        if (notify)
          TaskChangeNotifier.NotifyBatchTaskStarredChanged(notifyExt);
        SyncManager.TryDelaySync();
        task = (TaskModel) null;
        notifyExt = (TaskPinExtra) null;
      }

      static async Task ToggleSubTasks(
        TaskPinExtra notifyExtra,
        List<TaskModel> children,
        DateTime modifiedTime,
        long pinnedTimeStamp)
      {
        if (children == null || !children.Any<TaskModel>())
          return;
        foreach (TaskModel child in children)
        {
          child.pinnedTimeStamp = pinnedTimeStamp;
          child.modifiedTime = new DateTime?(modifiedTime);
          notifyExtra.Ids.Add(child.id);
          await TaskService.UpdateTaskPinned(child);
          await SyncStatusDao.AddModifySyncStatus(child.id);
        }
      }
    }

    public static async void UndoDeleteTaskRecurrence(TaskDeleteRecurrenceUndoEntity undoEntity)
    {
      TaskModel task = await TaskDao.GetThinTaskById(undoEntity.sid);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.status = undoEntity.taskStatus;
        task.startDate = new DateTime?(ModelsDao.TTCalendarToDateTime(undoEntity.startDate));
        task.dueDate = new DateTime?(ModelsDao.TTCalendarToDateTime(undoEntity.dueDate));
        task.remindTime = new DateTime?(ModelsDao.TTCalendarToDateTime(undoEntity.snoozeRemindTime));
        task.repeatFlag = undoEntity.repeatFlag;
        task.exDate = undoEntity.exDate?.ToArray();
        task.exDates = task.exDate == null ? string.Empty : ExDateSerilizer.ToString(task.exDate);
        await TaskService.UpdateTaskOnTimeChanged(task);
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        SyncStatusDao.AddModifySyncStatus(task.id);
        task = (TaskModel) null;
      }
    }

    public static async Task SkipRecurrenceByDate(
      string taskId,
      DateTime date,
      IToastShowWindow toastWindow = null)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        List<string> stringList = !string.IsNullOrEmpty(task.exDates) ? ((IEnumerable<string>) ExDateSerilizer.ToArray(task.exDates)).ToList<string>() : new List<string>();
        string str = date.ToString("yyyyMMdd");
        if (!stringList.Contains(str))
          stringList.Add(str);
        task.exDates = ExDateSerilizer.ToString(stringList.ToArray());
        await TaskService.UpdateTaskOnTimeChanged(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        if (toastWindow == null)
          task = (TaskModel) null;
        else if (task.repeatFlag == null)
          task = (TaskModel) null;
        else if (!task.repeatFlag.Contains("COUNT"))
        {
          task = (TaskModel) null;
        }
        else
        {
          toastWindow.TryToastString((object) null, Utils.GetString("SkipCountToast"));
          task = (TaskModel) null;
        }
      }
    }

    public static async Task SetDateAndTitle(string taskId, string text, TimeData model)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.isAllDay = model.IsAllDay;
        task.startDate = model.StartDate;
        task.dueDate = model.DueDate;
        task.reminders = model.Reminders?.ToArray();
        task.repeatFrom = model.RepeatFrom;
        task.repeatFlag = model.RepeatFlag;
        task.exDates = string.Empty;
        task.isFloating = new bool?(model.TimeZone.IsFloat);
        task.timeZone = model.TimeZone.TimeZoneName;
        if (task.remindTime.HasValue)
        {
          task.remindTime = new DateTime?();
          await ReminderDelayDao.DeleteByIdAsync(task.id, "task");
        }
        task.modifiedTime = new DateTime?(DateTime.Now);
        task.title = text;
        await TaskService.SaveTaskReminders(task);
        await TaskService.UpdateTask(task, CheckMatchedType.CheckSmart);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
        TaskChangeNotifier.NotifyTaskTextChanged(task.id);
        if (model.DueDate.HasValue)
        {
          task = (TaskModel) null;
        }
        else
        {
          ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(model);
          task = (TaskModel) null;
        }
      }
    }

    public static async Task UpdateTaskPomoSummary(
      string oldTaskId,
      string newTaskId,
      PomoTask pomoTask,
      List<PomoTask> pomoTasks,
      PomodoroModel pomoModel)
    {
      long oldTaskOriginDuration = 0;
      long newTaskOriginDuration = 0;
      foreach (PomoTask pomoTask1 in pomoTasks)
      {
        if (!string.IsNullOrEmpty(pomoTask1.TaskId))
        {
          if (pomoTask1.TaskId == oldTaskId)
            oldTaskOriginDuration += Utils.GetTotalSecond(pomoTask1.StartTime, pomoTask1.EndTime);
          else if (pomoTask1.TaskId == newTaskId)
            newTaskOriginDuration += Utils.GetTotalSecond(pomoTask1.StartTime, pomoTask1.EndTime);
        }
      }
      long duration = Utils.GetTotalSecond(pomoTask.StartTime, pomoTask.EndTime);
      long oldTaskDuration = oldTaskOriginDuration - duration;
      long newTaskDuration = newTaskOriginDuration + duration;
      TaskModel newTask = await TaskDao.GetThinTaskById(newTaskId);
      if (newTask != null && newTask.deleted == 0)
      {
        pomoTask.TaskId = newTask.id;
        pomoTask.HabitId = string.Empty;
        pomoTask.ProjectName = CacheManager.GetProjectById(newTask.projectId)?.name;
        pomoTask.TagString = newTask.tag;
        pomoTask.Title = newTask.title;
        PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(newTaskId);
        if (newTaskOriginDuration == 0L && pomoByTaskId == null)
          await PomoSummaryDao.SavePomoSummary(new PomodoroSummaryModel()
          {
            id = Utils.GetGuid(),
            userId = LocalSettings.Settings.LoginUserId,
            estimatedPomo = 0,
            EstimatedDuration = 0L,
            taskId = newTaskId,
            focuses = new List<object[]>()
            {
              new object[3]
              {
                (object) pomoModel.Id,
                (object) pomoModel.Type,
                (object) newTaskDuration
              }
            }
          });
        else if (pomoByTaskId != null)
        {
          List<object[]> focuses = pomoByTaskId.focuses;
          object[] objArray = focuses != null ? focuses.FirstOrDefault<object[]>((Func<object[], bool>) (f => f.Length == 3 && f[0] as string == pomoModel.Id)) : (object[]) null;
          if (objArray == null)
          {
            if (newTaskOriginDuration == 0L)
              pomoByTaskId.AddFocuses(new object[3]
              {
                (object) pomoModel.Id,
                (object) pomoModel.Type,
                (object) newTaskDuration
              });
            else if (pomoModel.Type == 0)
            {
              pomoByTaskId.PomoDuration += duration;
              if (newTaskOriginDuration < 300L && newTaskDuration >= 300L)
                ++pomoByTaskId.count;
            }
            else
              pomoByTaskId.StopwatchDuration += duration;
          }
          else
            objArray[2] = (object) newTaskDuration;
          await PomoSummaryDao.SavePomoSummary(pomoByTaskId);
        }
        newTask.modifiedTime = new DateTime?(DateTime.Now);
        await TaskDao.UpdateTask(newTask);
        await SyncStatusDao.AddModifySyncStatus(newTask.id);
      }
      TaskModel oldTask = await TaskDao.GetThinTaskById(oldTaskId);
      if (oldTask == null)
      {
        newTask = (TaskModel) null;
        oldTask = (TaskModel) null;
      }
      else
      {
        PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(oldTaskId);
        if (pomoByTaskId != null)
        {
          List<object[]> focuses = pomoByTaskId.focuses;
          object[] objArray = focuses != null ? focuses.FirstOrDefault<object[]>((Func<object[], bool>) (f => f.Length == 3 && f[0] as string == pomoModel.Id)) : (object[]) null;
          if (objArray == null)
          {
            if (pomoModel.Type == 0)
            {
              pomoByTaskId.PomoDuration -= duration;
              if (oldTaskOriginDuration >= 300L && oldTaskDuration < 300L)
                --pomoByTaskId.count;
            }
            else
              pomoByTaskId.StopwatchDuration -= duration;
          }
          else if (oldTaskDuration == 0L)
            pomoByTaskId.focuses.Remove(objArray);
          else
            objArray[2] = (object) Math.Max(0L, oldTaskDuration);
          pomoByTaskId.CheckNegative();
          await PomoSummaryDao.SavePomoSummary(pomoByTaskId);
        }
        oldTask.modifiedTime = new DateTime?(DateTime.Now);
        await TaskDao.UpdateTask(oldTask);
        await SyncStatusDao.AddModifySyncStatus(oldTask.id);
        newTask = (TaskModel) null;
        oldTask = (TaskModel) null;
      }
    }

    public static async Task UncheckTaskItem(string taskId)
    {
      TaskBaseViewModel taskViewModel = TaskCache.GetTaskById(taskId);
      if (taskViewModel != null)
      {
        int num = await TaskDetailItemService.UncheckDetailItemsByTaskId(taskId) ? 1 : 0;
        taskViewModel.Status = 0;
        taskViewModel.CompletedTime = new DateTime?();
        taskViewModel.CompletedUser = (string) null;
        taskViewModel.RemindTime = new DateTime?();
        taskViewModel.Progress = 0;
      }
      TaskModel task = await TaskDao.GetTaskById(taskId);
      if (task == null)
      {
        taskViewModel = (TaskBaseViewModel) null;
        task = (TaskModel) null;
      }
      else
      {
        task.status = 0;
        task.completedTime = new DateTime?();
        DateTime? nullable1 = task.remindTime;
        if (nullable1.HasValue)
        {
          TaskModel taskModel = task;
          nullable1 = new DateTime?();
          DateTime? nullable2 = nullable1;
          taskModel.remindTime = nullable2;
          await ReminderDelayDao.DeleteByIdAsync(task.id, "task");
        }
        task.progress = new int?(0);
        await TaskDao.UpdateTask(task);
        taskViewModel = (TaskBaseViewModel) null;
        task = (TaskModel) null;
      }
    }

    public static async Task UpdateTaskOnMoveUndo(TaskModel task, CheckMatchedType checkMatched = CheckMatchedType.None)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.Assignee = task.assignee;
      viewModel.ProjectId = task.projectId;
      viewModel.ParentId = task.parentId;
      viewModel.SortOrder = task.sortOrder;
      viewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, checkMatched);
      viewModel = (TaskBaseViewModel) null;
    }

    public static async Task UpdateTaskColumnId(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
      taskViewModel.ColumnId = task.columnId;
      taskViewModel.SortOrder = task.sortOrder;
      taskViewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
      await SyncStatusDao.AddModifySyncStatus(task.id);
    }

    public static async Task UpdateTaskParent(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.ParentId = task.parentId;
      viewModel.ChildrenIds = task.childrenString;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.All);
      viewModel = (TaskBaseViewModel) null;
    }

    public static async Task UpdateTaskProject(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.ProjectId = task.projectId;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.All);
      viewModel = (TaskBaseViewModel) null;
    }

    public static async Task UpdateTask(TaskModel task, CheckMatchedType check = CheckMatchedType.All)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.SetProperties(new TaskBaseViewModel(task));
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, check);
      viewModel = (TaskBaseViewModel) null;
    }

    public static async Task<TaskBaseViewModel> UpdateTaskOnCompleteUndo(TaskModel task)
    {
      if (task == null)
        return (TaskBaseViewModel) null;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.Status = task.status;
      viewModel.CompletedTime = task.completedTime;
      viewModel.CompletedUser = task.completedUserId;
      viewModel.StartDate = task.startDate;
      viewModel.DueDate = task.dueDate;
      viewModel.RepeatFlag = task.repeatFlag;
      viewModel.RepeatFrom = task.repeatFrom;
      viewModel.ExDates = task.exDates;
      viewModel.Progress = task.progress.GetValueOrDefault();
      viewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
      return viewModel;
    }

    private static async Task UpdateTaskOnProjectChanged(TaskModel task, CheckMatchedType check = CheckMatchedType.CheckProject)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.ProjectId = task.projectId;
      viewModel.Assignee = task.assignee;
      viewModel.Deleted = task.deleted;
      viewModel.SortOrder = task.sortOrder;
      viewModel.ColumnId = task.columnId;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, check);
      viewModel = (TaskBaseViewModel) null;
    }

    private static async Task UpdateTaskOnStatusChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.Status = task.status;
      viewModel.CompletedTime = task.completedTime;
      viewModel.CompletedUser = task.completedUserId;
      viewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.All);
      viewModel = (TaskBaseViewModel) null;
    }

    private static async Task UpdateTaskPinned(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
      taskViewModel.PinnedTime = task.pinnedTimeStamp;
      taskViewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
    }

    public static async Task ClearAssignByProjectId(string projectId, string userId)
    {
      List<TaskModel> assignTasksInProject = await TaskDao.GetAssignTasksInProject(projectId, userId);
      List<TaskBaseViewModel> vms = new List<TaskBaseViewModel>();
      if (assignTasksInProject != null && assignTasksInProject.Count != 0)
      {
        foreach (TaskModel task in assignTasksInProject)
        {
          task.assignee = "-1";
          TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
          taskViewModel.Assignee = "-1";
          vms.Add(taskViewModel);
          await TaskDao.UpdateTask(task);
          await SyncStatusDao.AddSyncStatus(task.id, 0);
        }
      }
      ProjectAndTaskIdsCache.OnTasksChanged(vms, CheckMatchedType.CheckAssign);
      vms = (List<TaskBaseViewModel>) null;
    }

    public static async Task UpdateTaskOnDeletedChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.Deleted = task.deleted;
      viewModel.AttendId = task.attendId;
      viewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.All);
      viewModel = (TaskBaseViewModel) null;
    }

    public static async Task UpdateTaskOnTimeChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.StartDate = task.startDate;
      viewModel.DueDate = task.dueDate;
      viewModel.IsAllDay = task.isAllDay;
      viewModel.RepeatFlag = task.repeatFlag;
      viewModel.ExDates = task.exDates;
      viewModel.RepeatFrom = task.repeatFrom;
      viewModel.RemindTime = task.remindTime;
      viewModel.IsFloating = task.isFloating.GetValueOrDefault();
      viewModel.TimeZoneName = task.timeZone;
      viewModel.ModifiedTime = task.modifiedTime;
      viewModel.AttendId = task.attendId;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.CheckSmart);
      ReminderCalculator.AssembleReminders();
      viewModel = (TaskBaseViewModel) null;
    }

    public static async Task UpdateTaskOnDetailChanged(TaskModel task, CheckMatchedType checkType)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, checkType);
      viewModel = (TaskBaseViewModel) null;
    }

    private static async Task UpdateTaskOnKindChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      viewModel.Kind = task.kind;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.All);
      viewModel = (TaskBaseViewModel) null;
    }

    private static async Task UpdateTaskOnProgressChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
      taskViewModel.Progress = task.progress.GetValueOrDefault();
      taskViewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
    }

    private static async Task UpdateTaskOnAssigneeChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel viewModel = TaskCache.SafeGetTaskViewModel(task);
      if (viewModel.Assignee != task.assignee)
        viewModel.Assignee = task.assignee;
      viewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
      ProjectAndTaskIdsCache.OnTaskChanged(viewModel, CheckMatchedType.CheckAssign);
      viewModel = (TaskBaseViewModel) null;
    }

    public static async Task FoldTask(string taskId, bool isOpen)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      thinTaskById.isOpen = isOpen;
      TaskCache.SafeGetTaskViewModel(thinTaskById).IsOpen = thinTaskById.isOpen;
      await TaskDao.UpdateTask(thinTaskById);
    }

    public static async Task UpdateTaskOnImgModeChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
      taskViewModel.ImageMode = task.imgMode;
      taskViewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
    }

    public static async Task UpdateTaskOnTitleChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
      taskViewModel.Title = task.title;
      taskViewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
    }

    public static async Task UpdateTaskOnContentChanged(TaskModel task)
    {
      if (task == null)
        return;
      TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
      taskViewModel.Content = task.content;
      taskViewModel.ModifiedTime = task.modifiedTime;
      await TaskDao.UpdateTask(task);
    }

    public static async Task<List<TaskModel>> GetAllSubTasksByIdAsync(
      string taskId,
      string projectId)
    {
      return TaskService.GetSubTasks(taskId, await TaskDao.GetTasksInProjectAsync(projectId));
    }

    public static List<TaskModel> GetSubTasks(
      string taskId,
      List<TaskModel> tasks,
      bool withDeleted = false)
    {
      if (tasks == null || string.IsNullOrEmpty(taskId))
        return (List<TaskModel>) null;
      Dictionary<string, TaskModel> taskDict = new Dictionary<string, TaskModel>();
      if (!withDeleted)
        tasks = tasks.Where<TaskModel>((Func<TaskModel, bool>) (t => t.deleted == 0)).ToList<TaskModel>();
      foreach (TaskModel task in tasks)
      {
        if (!string.IsNullOrEmpty(task.id) && !taskDict.ContainsKey(task.id))
          taskDict[task.id] = task;
      }
      return tasks.Where<TaskModel>((Func<TaskModel, bool>) (task => FindParent(task, new HashSet<string>()))).ToList<TaskModel>();

      bool FindParent(TaskModel task, HashSet<string> checkedIds)
      {
        for (; !(task.parentId == taskId); task = taskDict[task.parentId])
        {
          if (string.IsNullOrEmpty(task.parentId) || !taskDict.ContainsKey(task.parentId) || checkedIds.Contains(task.parentId))
            return false;
          checkedIds.Add(task.parentId);
        }
        return true;
      }
    }

    public static async Task UndoTask(TaskModel task, List<TaskDetailItemModel> checkItems = null)
    {
      TaskModel current;
      if (task == null)
      {
        current = (TaskModel) null;
      }
      else
      {
        current = await TaskDao.GetTaskById(task.id);
        if (current == null)
        {
          current = (TaskModel) null;
        }
        else
        {
          bool changed = false;
          if (task.projectId != current.projectId)
          {
            changed = true;
            current.projectId = task.projectId;
          }
          if (task.parentId != current.parentId)
          {
            changed = true;
            current.parentId = task.parentId;
          }
          DateTime? nullable1 = task.startDate;
          DateTime? nullable2 = current.startDate;
          if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0)
          {
            changed = true;
            current.startDate = task.startDate;
          }
          nullable2 = task.dueDate;
          nullable1 = current.dueDate;
          if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() != nullable1.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0)
          {
            changed = true;
            current.dueDate = task.dueDate;
          }
          bool? isAllDay1 = task.isAllDay;
          bool? isAllDay2 = current.isAllDay;
          if (!(isAllDay1.GetValueOrDefault() == isAllDay2.GetValueOrDefault() & isAllDay1.HasValue == isAllDay2.HasValue))
          {
            changed = true;
            current.isAllDay = task.isAllDay;
          }
          if (task.repeatFlag != current.repeatFlag)
          {
            changed = true;
            current.repeatFlag = task.repeatFlag;
          }
          if (task.repeatFrom != current.repeatFrom)
          {
            changed = true;
            current.repeatFrom = task.repeatFrom;
          }
          if (task.exDates != current.exDates)
          {
            changed = true;
            current.exDates = task.exDates;
          }
          if (task.exDates != current.exDates)
          {
            changed = true;
            current.exDates = task.exDates;
          }
          if (task.tag != current.tag)
          {
            changed = true;
            current.tag = task.tag;
          }
          if (task.priority != current.priority)
          {
            changed = true;
            current.priority = task.priority;
          }
          bool flag = task.kind != current.kind;
          if (task.kind != current.kind)
          {
            changed = true;
            current.kind = task.kind;
          }
          int? progress1 = task.progress;
          int? progress2 = current.progress;
          if (!(progress1.GetValueOrDefault() == progress2.GetValueOrDefault() & progress1.HasValue == progress2.HasValue))
          {
            changed = true;
            current.progress = task.progress;
          }
          if (task.assignee != current.assignee)
          {
            changed = true;
            current.assignee = task.assignee;
          }
          if (checkItems != null & flag && current.kind == "CHECKLIST")
          {
            List<TaskDetailItemModel> taskDetailItemModelList = await TaskDetailItemDao.DeleteCheckItemsByTaskId(current.id);
            foreach (TaskDetailItemModel item in checkItems)
            {
              item.TaskServerId = current.id;
              await TaskDetailItemDao.InsertChecklistItem(item);
              ItemChangeNotifier.NotifyDeletedUndo(item);
            }
          }
          if (!changed)
          {
            current = (TaskModel) null;
          }
          else
          {
            await TaskService.UpdateTask(current);
            TaskChangeNotifier.NotifyTaskBatchChanged(new List<string>()
            {
              task.id
            });
            SyncManager.TryDelaySync();
            current = (TaskModel) null;
          }
        }
      }
    }

    public static async Task<TaskModel> CreateNewTask()
    {
      TaskModel task = new TaskModel();
      TaskDefaultModel taskDefault = TaskDefaultDao.GetDefaultSafely();
      if (await ProChecker.CheckTaskLimit(taskDefault.projectId))
        return (TaskModel) null;
      task.startDate = taskDefault.GetDefaultDateTime();
      task.projectId = taskDefault.ProjectId;
      task.priority = taskDefault.Priority;
      task.isAllDay = new bool?(true);
      task = await TaskService.AddTask(task);
      if (!string.IsNullOrEmpty(taskDefault?.AllDayReminders))
      {
        string allDayReminders = taskDefault.AllDayReminders;
        char[] chArray = new char[1]{ ',' };
        foreach (TaskReminderModel buildReminder in Utils.BuildReminders((IReadOnlyCollection<string>) ((IEnumerable<string>) allDayReminders.Split(chArray)).ToList<string>()))
        {
          buildReminder.taskserverid = task.id;
          int num = await TaskReminderDao.SaveReminders(buildReminder);
        }
      }
      return task;
    }

    public static async Task TryToastMoveControl(
      ProjectIdentity identity,
      TaskModel task,
      string projectId,
      bool hideTitle = false)
    {
      if (identity == null || identity is CompletedProjectIdentity || task == null || MoveToastHelper.CheckTaskMatched(identity, task))
        return;
      Utils.GetToastWindow()?.ToastMoveProjectControl(projectId, hideTitle ? string.Empty : task.title);
    }

    public static async Task<bool> InEmptyTask(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById != null)
      {
        switch (thinTaskById.kind)
        {
          case "TEXT":
          case "NOTE":
            if (string.IsNullOrEmpty(thinTaskById.content))
              return true;
            break;
          case "CHECKLIST":
            if (!string.IsNullOrEmpty(thinTaskById.desc))
              return false;
            List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(thinTaskById.id);
            if (checkItemsByTaskId == null || checkItemsByTaskId.Count == 0 || checkItemsByTaskId.Count == 1 && string.IsNullOrEmpty(checkItemsByTaskId[0].title))
              return true;
            break;
        }
      }
      return false;
    }

    public static bool IsTeamTask(string taskId)
    {
      TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
      if (taskById == null)
        return false;
      ProjectModel projectById = CacheManager.GetProjectById(taskById.ProjectId);
      return TeamService.GetTeamId() == projectById?.teamId;
    }

    public static void DeleteTasksFromDb(List<TaskModel> needDelete)
    {
      foreach (TaskModel taskModel in needDelete)
      {
        App.Connection.DeleteAsync((object) taskModel);
        TaskDetailItemDao.DeleteCheckItemsByTaskId(taskModel.id);
        App.Connection.ExecuteAsync("DELETE FROM AttachmentModel WHERE taskId=?", (object) taskModel.id);
        App.Connection.ExecuteAsync("DELETE FROM ReminderModel WHERE TaskId=?", (object) taskModel.id);
      }
    }

    public static async Task PullGuideTask()
    {
      List<TaskModel> guideTask = await Communicator.GetGuideTask();
      // ISSUE: explicit non-virtual call
      if (guideTask == null || __nonvirtual (guideTask.Count) <= 0)
        return;
      long num = 0;
      foreach (TaskModel taskModel in guideTask)
      {
        taskModel.sortOrder = num;
        taskModel.userId = LocalSettings.Settings.LoginUserId;
        taskModel.projectId = Utils.GetInboxId();
        num += 268435456L;
      }
      await TaskDao.BatchInsertTasksAndItems(guideTask);
    }
  }
}
