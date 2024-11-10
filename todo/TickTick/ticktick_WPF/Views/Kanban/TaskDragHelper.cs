// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.TaskDragHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public static class TaskDragHelper
  {
    private static string _columnId;
    private static DisplayItemModel _model;

    public static void Register(string columnId, DisplayItemModel model)
    {
      TaskDragHelper._columnId = columnId;
      TaskDragHelper._model = model;
    }

    private static void UnRegister()
    {
      TaskDragHelper._columnId = string.Empty;
      TaskDragHelper._model = (DisplayItemModel) null;
    }

    public static async void OnTaskDrop(IEnumerable<ColumnViewModel> columns, IKanban kanban)
    {
      if (!await TaskDragHelper.TryDropOnProjectMenu(kanban))
      {
        if (await TaskDragHelper.TryDropOnSameColumn(columns, kanban))
          return;
        await TaskDragHelper.TryDropOnAnotherColumn(columns, kanban);
      }
      else
        kanban.Reload();
    }

    private static async Task<bool> TryDropOnSameColumn(
      IEnumerable<ColumnViewModel> columns,
      IKanban kanban)
    {
      ColumnViewModel dropColumn = TaskDragHelper.GetDropColumn(columns);
      if (dropColumn == null || !(dropColumn.ColumnId == TaskDragHelper._columnId))
        return false;
      kanban.DropTaskInColumn(TaskDragHelper._model, dropColumn);
      TaskDragHelper.UnRegister();
      return true;
    }

    private static async Task TryDropOnAnotherColumn(
      IEnumerable<ColumnViewModel> columns,
      IKanban kanban)
    {
      try
      {
        DisplayItemModel model = TaskDragHelper._model;
        if (kanban != null && model != null)
        {
          ColumnViewModel dropColumn = TaskDragHelper.GetDropColumn(columns);
          if (dropColumn != null && dropColumn.CanDrop && dropColumn.ColumnId != TaskDragHelper._columnId)
          {
            if (dropColumn.ColumnId == "note" || dropColumn.ColumnId == "habit" || dropColumn.ColumnId == "course" || dropColumn.ColumnId == "calendar" || dropColumn.ColumnId.StartsWith("tag:"))
              return;
            if (dropColumn.ColumnId.StartsWith("date:"))
            {
              DateTime? date = dropColumn.GetDate();
              if (model.IsItem)
              {
                if (date.HasValue)
                {
                  TaskDetailItemModel taskDetailItemModel = await TaskService.SetSubtaskDate(model.TaskId, model.Id, date.Value);
                }
                else
                  await TaskService.SetCheckItemDate(model.TaskId, model.Id, new TimeDataModel()
                  {
                    StartDate = new DateTime?(),
                    IsAllDay = new bool?()
                  });
              }
              else if (date.HasValue)
                await TaskService.SetDate(model.TaskId, date.Value);
              else
                await TaskService.ClearDate(model.TaskId);
            }
            else if (dropColumn.ColumnId.StartsWith("project:"))
              await TaskService.MoveProject(model.TaskId, dropColumn.GetProject());
            else if (dropColumn.ColumnId.StartsWith("priority:"))
              await TaskService.SetPriority(model.TaskId, dropColumn.GetPriority());
            else if (dropColumn.ColumnId.StartsWith("assign:"))
            {
              string assignee = dropColumn.GetAssignee();
              if (!string.IsNullOrEmpty(assignee) && assignee != "-1")
              {
                List<ShareUserModel> projectUsers = AvatarHelper.GetProjectUsers(TaskDragHelper._model.ProjectId);
                if (projectUsers == null || projectUsers.Count == 0 || projectUsers.All<ShareUserModel>((Func<ShareUserModel, bool>) (a => (a.userId.ToString() ?? "") != assignee)))
                {
                  kanban.Toast(Utils.GetString("ChangeAssigneeError"));
                  return;
                }
              }
              await TaskService.SetAssignee(model.TaskId, assignee);
            }
            else
              await TaskService.SaveTaskColumnId(model.TaskId, dropColumn.ColumnId);
            await TaskDao.UpdateParent(model.TaskId, string.Empty);
            TaskDragHelper.UnRegister();
            SyncManager.TryDelaySync();
          }
        }
        model = (DisplayItemModel) null;
      }
      catch (Exception ex)
      {
      }
      finally
      {
        kanban?.Reload(true);
      }
    }

    public static async Task ChangeOrderInType(
      string sortOrderType,
      DisplayItemModel front,
      DisplayItemModel next,
      List<DisplayItemModel> dragModels,
      List<DisplayItemModel> siblings,
      ProjectIdentity projectIdentity)
    {
      string catId;
      if (dragModels == null)
        catId = (string) null;
      else if (dragModels.Count == 0)
      {
        catId = (string) null;
      }
      else
      {
        catId = projectIdentity.CatId;
        long num1;
        if (front == null || !front.IsSection && front.Level == dragModels[0].Level)
        {
          DisplayItemModel displayItemModel = front;
          num1 = displayItemModel != null ? displayItemModel.SpecialOrder : long.MaxValue;
        }
        else
          num1 = long.MaxValue;
        long frontOrder = num1;
        long num2;
        if (next == null || !next.IsSection && next.Level == dragModels[0].Level)
        {
          DisplayItemModel displayItemModel = next;
          num2 = displayItemModel != null ? displayItemModel.SpecialOrder : long.MaxValue;
        }
        else
          num2 = long.MaxValue;
        long nextOrder = num2;
        if (front != null && front.SpecialOrder == 0L)
        {
          List<SyncSortOrderModel> source = await TaskSortOrderService.BatchResetSortOrder(sortOrderType, catId, siblings.Select<DisplayItemModel, (string, int)>((Func<DisplayItemModel, (string, int)>) (i => (i.Id, EntityType.GetEntityTypeNum(i.Type)))).ToList<(string, int)>());
          SyncSortOrderModel syncSortOrderModel1 = source.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == front?.Id));
          SyncSortOrderModel syncSortOrderModel2 = source.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == next?.Id));
          frontOrder = syncSortOrderModel1 != null ? syncSortOrderModel1.SortOrder : (syncSortOrderModel2 != null ? syncSortOrderModel2.SortOrder - 268435456L : 0L);
          nextOrder = syncSortOrderModel2 != null ? syncSortOrderModel2.SortOrder : frontOrder + 268435456L;
        }
        else
        {
          if (frontOrder == long.MaxValue && nextOrder != long.MaxValue)
            frontOrder = nextOrder - 268435456L;
          if (nextOrder == long.MaxValue && frontOrder != long.MaxValue)
            nextOrder = frontOrder + 268435456L;
          if (nextOrder == long.MaxValue && frontOrder == long.MaxValue)
          {
            frontOrder = 0L;
            nextOrder = 0L;
          }
        }
        for (int i = 0; i < dragModels.Count; ++i)
        {
          long num3 = Math.Min(frontOrder, nextOrder) + Math.Abs(nextOrder - frontOrder) * (long) (i + 1) / (long) (dragModels.Count + 1);
          SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertOrUpdateAsync(sortOrderType, catId, dragModels[i].Id, EntityType.GetEntityTypeNum(dragModels[i].Type), new long?(num3));
        }
        catId = (string) null;
      }
    }

    public static async Task ChangeNewDate(
      Section section,
      DisplayItemModel model,
      string columnId)
    {
      if (section == null || model == null)
        return;
      DateTime? startDate = section.GetStartDate();
      if (startDate.HasValue)
      {
        if (model.IsTaskOrNote)
        {
          TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskId);
          if (!string.IsNullOrEmpty(thinTaskById?.attendId))
          {
            if (AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) thinTaskById))
              return;
            Utils.Toast(Utils.GetString("AttendeeSetDate"));
          }
          else
          {
            bool hasStartDate = model.StartDate.HasValue;
            await TaskService.SetDate(model.TaskId, startDate.Value, false, columnId);
            if (hasStartDate)
              return;
            await TaskService.SaveTaskReminders(new TaskModel()
            {
              id = model.TaskId,
              reminders = TimeData.GetDefaultAllDayReminders().ToArray()
            });
          }
        }
        else
        {
          if (!model.IsItem)
            return;
          TaskDetailItemModel taskDetailItemModel = await TaskService.SetSubtaskDate(model.TaskId, model.Id, startDate.Value);
        }
      }
      else
        await TaskService.ClearDate(model.TaskId, columnId);
    }

    private static int GetSortOrderType(DisplayItemModel model)
    {
      if (model.IsTaskOrNote)
        return 1;
      if (model.IsItem)
        return 2;
      return model.IsEvent ? 3 : 1;
    }

    private static void AddDeltaOrder(
      string date,
      string projectId,
      DisplayItemModel model,
      long order,
      ICollection<TaskSortOrderInDateModel> diffOrders)
    {
      TaskSortOrderInDateModel orderInDateModel = new TaskSortOrderInDateModel()
      {
        userid = LocalSettings.Settings.LoginUserId,
        modifiedTime = Utils.GetNowTimeStamp(),
        taskid = model.Id,
        projectid = projectId,
        date = date,
        type = TaskDragHelper.GetSortOrderType(model),
        sortOrder = order,
        status = 1
      };
      diffOrders.Add(orderInDateModel);
    }

    private static async Task SaveSectionSortOrders(
      List<DisplayItemModel> siblings,
      DisplayItemModel target,
      DisplayItemModel dragModel,
      string projectId,
      string date,
      IReadOnlyCollection<TaskSortOrderInDateModel> orders,
      bool below)
    {
      List<TaskSortOrderInDateModel> deltaOrders = new List<TaskSortOrderInDateModel>();
      long order1 = 0;
      if (orders != null && orders.Count > 0)
      {
        long order2 = orders.OrderByDescending<TaskSortOrderInDateModel, long>((Func<TaskSortOrderInDateModel, long>) (order => order.sortOrder)).First<TaskSortOrderInDateModel>().sortOrder + 268435456L;
        foreach (DisplayItemModel sibling in siblings)
        {
          DisplayItemModel model = sibling;
          if ((model.IsTaskOrNote || model.IsItem || model.IsEvent) && orders.All<TaskSortOrderInDateModel>((Func<TaskSortOrderInDateModel, bool>) (order => order.taskid != model.Id)))
          {
            TaskDragHelper.AddDeltaOrder(date, projectId, model, order2, (ICollection<TaskSortOrderInDateModel>) deltaOrders);
            order2 += 268435456L;
          }
        }
      }
      else
      {
        foreach (DisplayItemModel model in siblings.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.IsTaskOrNote || model.IsItem || model.IsEvent)))
        {
          TaskDragHelper.AddDeltaOrder(date, projectId, model, order1, (ICollection<TaskSortOrderInDateModel>) deltaOrders);
          order1 += 268435456L;
        }
      }
      if (deltaOrders.Count > 0)
        await TaskSortOrderInDateDao.BatchInsertOrderInDate(deltaOrders);
      List<TaskSortOrderInDateModel> source = new List<TaskSortOrderInDateModel>();
      if (orders != null)
        source.AddRange((IEnumerable<TaskSortOrderInDateModel>) orders);
      source.AddRange((IEnumerable<TaskSortOrderInDateModel>) deltaOrders);
      List<TaskSortOrderInDateModel> list = source.OrderBy<TaskSortOrderInDateModel, long>((Func<TaskSortOrderInDateModel, long>) (order => order.sortOrder)).ToList<TaskSortOrderInDateModel>();
      int index = target == null ? -1 : TaskDragHelper.GetOrderIndex(target, (IReadOnlyList<TaskSortOrderInDateModel>) list);
      long num;
      if (index >= 0)
      {
        long sortOrder = list[index].sortOrder;
        num = below ? (index >= list.Count - 1 ? sortOrder + 268435456L : (sortOrder + list[index + 1].sortOrder) / 2L) : (index <= 0 ? sortOrder - 268435456L : (sortOrder + list[index - 1].sortOrder) / 2L);
      }
      else
        num = list.Select<TaskSortOrderInDateModel, long>((Func<TaskSortOrderInDateModel, long>) (o => o.sortOrder)).ToList<long>().Min() - 268435456L;
      await TaskSortOrderInDateDao.UpdateOrInsertTaskSortOrderInDateAsync(new TaskSortOrderInDateModel()
      {
        userid = LocalSettings.Settings.LoginUserId,
        modifiedTime = Utils.GetNowTimeStamp(),
        taskid = dragModel.Id,
        projectid = projectId,
        date = date,
        type = TaskDragHelper.GetSortOrderType(dragModel),
        sortOrder = num,
        status = 1
      });
      deltaOrders = (List<TaskSortOrderInDateModel>) null;
    }

    private static int GetOrderIndex(
      DisplayItemModel target,
      IReadOnlyList<TaskSortOrderInDateModel> orders)
    {
      for (int index = 0; index < orders.Count; ++index)
      {
        if (orders[index].taskid == target.Id)
          return index;
      }
      return -1;
    }

    public static async Task ChangeTasksSortOrder(
      DisplayItemModel front,
      DisplayItemModel next,
      List<DisplayItemModel> models,
      string dropProjectId,
      List<DisplayItemModel> siblings,
      Section dropSection,
      ProjectIdentity projectIdentity)
    {
      if (models == null)
        ;
      else if (models.Count == 0)
        ;
      else
      {
        string projectId = dropProjectId;
        List<DisplayItemModel> source1 = siblings;
        List<string> list = source1 != null ? source1.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (i => i.Id)).ToList<string>() : (List<string>) null;
        List<TaskSortOrderInProjectModel> source2 = await ProjectSortOrderDao.BatchResetSortOrder(projectId, list);
        TaskSortOrderInProjectModel orderInProjectModel1 = source2.FirstOrDefault<TaskSortOrderInProjectModel>((Func<TaskSortOrderInProjectModel, bool>) (o => o.EntityId == front?.Id));
        TaskSortOrderInProjectModel orderInProjectModel2 = source2.FirstOrDefault<TaskSortOrderInProjectModel>((Func<TaskSortOrderInProjectModel, bool>) (o => o.EntityId == next?.Id));
        long frontOrder = orderInProjectModel1 != null ? orderInProjectModel1.SortOrder : (orderInProjectModel2 != null ? orderInProjectModel2.SortOrder - 268435456L : 0L);
        long nextOrder = orderInProjectModel2 != null ? orderInProjectModel2.SortOrder : frontOrder + 268435456L;
        for (int i = 0; i < models.Count; ++i)
          await ProjectSortOrderDao.UpdateSortOrderInProject(models[i].Id, Math.Min(frontOrder, nextOrder) + Math.Abs(nextOrder - frontOrder) * (long) (i + 1) / (long) (models.Count + 1), dropProjectId, EntityType.GetEntityType(models[i].Type));
      }
    }

    public static async Task ChangeOrderInPriority(
      Section dropSection,
      IEnumerable<DisplayItemModel> siblings,
      ProjectIdentity projectIdentity,
      List<DisplayItemModel> models,
      DisplayItemModel front,
      DisplayItemModel next)
    {
      if (models == null)
        ;
      else if (models.Count == 0)
        ;
      else
      {
        Section section = dropSection;
        int priority = section != null ? section.GetPriority() : 0;
        if (models.Count == 1)
        {
          DisplayItemModel model = models[0];
          if (model.IsEvent && priority != 0)
          {
            Utils.Toast(Utils.GetString("CannotDragEventToPrioritySection"));
            return;
          }
          if (model.IsItem && model.Priority != priority)
          {
            Utils.Toast(Utils.GetString("CannotDragSubTasksToPrioritySection"));
            return;
          }
          if (!model.IsTask && !model.IsItem && !model.IsEvent)
            return;
        }
        List<TaskSortOrderInPriorityModel> source = await TaskDragHelper.BatchResetSortOrderInPriority(siblings.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (s => s.Id)).ToList<string>(), priority, projectIdentity.CatId);
        TaskSortOrderInPriorityModel orderInPriorityModel1 = source.FirstOrDefault<TaskSortOrderInPriorityModel>((Func<TaskSortOrderInPriorityModel, bool>) (o => o.EntityId == front?.Id));
        TaskSortOrderInPriorityModel orderInPriorityModel2 = source.FirstOrDefault<TaskSortOrderInPriorityModel>((Func<TaskSortOrderInPriorityModel, bool>) (o => o.EntityId == next?.Id));
        long frontOrder = orderInPriorityModel1 != null ? orderInPriorityModel1.SortOrder : (orderInPriorityModel2 != null ? orderInPriorityModel2.SortOrder - 268435456L : 0L);
        long nextOrder = orderInPriorityModel2 != null ? orderInPriorityModel2.SortOrder : frontOrder + 268435456L;
        for (int i = 0; i < models.Count; ++i)
        {
          if (models[i].IsTask && models[i].Level == 0)
            await TaskService.SetPriority(models[i].Id, priority, notify: false);
          await TaskDragHelper.UpdateSortOrderInPriority(models[i].Id, Math.Min(frontOrder, nextOrder) + Math.Abs(nextOrder - frontOrder) * (long) (i + 1) / (long) (models.Count + 1), priority, projectIdentity.CatId, EntityType.GetEntityType(models[i].Type));
        }
      }
    }

    private static ColumnViewModel GetDropColumn(IEnumerable<ColumnViewModel> columns)
    {
      return columns == null ? (ColumnViewModel) null : columns.FirstOrDefault<ColumnViewModel>((Func<ColumnViewModel, bool>) (column => column.MouseOver));
    }

    private static async Task<bool> TryDropOnProjectMenu(IKanban kanban)
    {
      if (kanban is FrameworkElement child)
      {
        ListViewContainer listView = Utils.FindParent<ListViewContainer>((DependencyObject) child);
        if (listView != null)
        {
          IDroppable target = listView.ProjectList.GetTaskDropTarget();
          if (target != null && target.CanDrop)
          {
            await TaskDragHelper.SetTaskProperty(await TaskDao.GetThinTaskById(TaskDragHelper._model.TaskId), target, listView);
            listView.ProjectList.ClearDropSelected();
            TaskDragHelper.UnRegister();
            return true;
          }
          target = (IDroppable) null;
        }
      }
      return false;
    }

    public static async Task SetTaskProperty(
      TaskModel task,
      IDroppable droppable,
      ListViewContainer listView)
    {
      IToastShowWindow toastWindow;
      if (droppable == null)
        toastWindow = (IToastShowWindow) null;
      else if (task == null)
      {
        toastWindow = (IToastShowWindow) null;
      }
      else
      {
        toastWindow = listView?.GetToastWindow() ?? Utils.GetToastWindow();
        if (!string.IsNullOrEmpty(droppable.ProjectId) && task.projectId != droppable.ProjectId)
        {
          await TaskService.MoveProject(task.id, droppable.ProjectId);
          toastWindow?.ToastMoveProjectControl(droppable.ProjectId, task.title);
        }
        if (droppable.Priority != 0)
          await TaskService.SetPriority(task.id, droppable.Priority);
        DateTime? nullable = droppable.DefaultDate;
        if (nullable.HasValue)
        {
          if (!string.IsNullOrEmpty(task.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) task))
          {
            Utils.Toast(Utils.GetString("AttendeeSetDate"));
            toastWindow = (IToastShowWindow) null;
            return;
          }
          nullable = task.startDate;
          bool hasStartDate = nullable.HasValue;
          string id = task.id;
          nullable = droppable.DefaultDate;
          DateTime newDate = nullable.Value;
          bool? isAllDay = new bool?();
          await TaskService.SetDate(id, newDate, false, isAllDay: isAllDay);
          if (!hasStartDate)
            await TaskService.SaveTaskReminders(new TaskModel()
            {
              id = task.id,
              reminders = TimeData.GetDefaultAllDayReminders().ToArray()
            });
          if (droppable is SmartProjectViewModel projectViewModel && (projectViewModel.Id == "_special_id_today" || projectViewModel.Id == "_special_id_tomorrow"))
          {
            bool isToday = projectViewModel.Id == "_special_id_today";
            TaskModel thinTaskById = await TaskDao.GetThinTaskById(task.id);
            listView?.TryToastMoveControl(thinTaskById, isToday);
          }
        }
        if (droppable.IsCompleted)
        {
          if (task.kind == "NOTE")
          {
            toastWindow = (IToastShowWindow) null;
            return;
          }
          TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(task.id, 2);
        }
        if (droppable.IsAbandoned)
        {
          if (task.kind == "NOTE")
          {
            toastWindow = (IToastShowWindow) null;
            return;
          }
          TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(task.id, -1);
        }
        if (droppable.IsDeleted)
        {
          if (!string.IsNullOrEmpty(task.attendId))
          {
            if (await TaskOperationHelper.CheckIfAllowDeleteAgenda(task, (DependencyObject) App.Window))
              await TaskService.DeleteAgenda(task.id, task.projectId, task.attendId);
          }
          else
          {
            toastWindow?.TaskDeleted(task.id);
            listView?.TryExtractDetail();
          }
        }
        if (!droppable.Tags.Any<string>())
        {
          toastWindow = (IToastShowWindow) null;
        }
        else
        {
          List<string> tags = TagSerializer.ToTags(task.tag);
          tags.AddRange(droppable.Tags.Except<string>((IEnumerable<string>) tags));
          await TaskService.SetTags(task.id, tags);
          toastWindow = (IToastShowWindow) null;
        }
      }
    }

    public static async Task CheckChildrenLevelAndSortOrder(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      Dictionary<string, Node<TaskBaseViewModel>> taskTreeInProject = TaskCache.GetTaskTreeInProject(thinTaskById.projectId);
      if (!taskTreeInProject.ContainsKey(thinTaskById.id))
        return;
      await CheckLevel(taskTreeInProject[thinTaskById.id], TaskCache.GetTaskLevel(taskId));

      static async Task CheckLevel(Node<TaskBaseViewModel> node, int level)
      {
        if (level >= 4)
        {
          List<Node<TaskBaseViewModel>> children = node.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) == 0)
            return;
          await TaskDao.BatchUpdateParent(node.GetAllChildrenValue().Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>(), node.Value.ParentId);
        }
        else
        {
          List<Node<TaskBaseViewModel>> children = node.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) == 0)
            return;
          foreach (Node<TaskBaseViewModel> child in node.Children)
            await CheckLevel(child, level + 1);
        }
      }
    }

    private static async Task<List<TaskSortOrderInPriorityModel>> BatchResetSortOrderInPriority(
      List<string> ids,
      int priority,
      string catId)
    {
      List<TaskSortOrderInPriorityModel> result = new List<TaskSortOrderInPriorityModel>();
      if (ids == null || ids.Count == 0)
        return result;
      List<TaskSortOrderInPriorityModel> models = (await TaskSortOrderInPriorityDao.GetSortOrders(catId, new int?(priority), containDelete: true)).Where<TaskSortOrderInPriorityModel>((Func<TaskSortOrderInPriorityModel, bool>) (m => ids.Contains(m.EntityId))).ToList<TaskSortOrderInPriorityModel>();
      long sortOrder = 0;
      foreach (string id1 in ids)
      {
        string id = id1;
        TaskSortOrderInPriorityModel model = models.FirstOrDefault<TaskSortOrderInPriorityModel>((Func<TaskSortOrderInPriorityModel, bool>) (m => m.EntityId == id));
        if (model != null)
        {
          model.SortOrder = sortOrder;
          model.SyncStatus = 1;
          model.Deleted = 0;
          model.UserId = LocalSettings.Settings.LoginUserId;
          await TaskSortOrderInPriorityDao.UpdateAsync(model);
        }
        else
        {
          model = new TaskSortOrderInPriorityModel()
          {
            EntityId = id,
            EntityType = "task",
            Priority = priority,
            CatId = catId,
            SyncStatus = 0,
            SortOrder = sortOrder,
            UserId = LocalSettings.Settings.LoginUserId
          };
          await TaskSortOrderInPriorityDao.InsertAsync(model);
        }
        sortOrder += 268435456L;
        result.Add(model);
        model = (TaskSortOrderInPriorityModel) null;
      }
      return result;
    }

    private static async Task UpdateSortOrderInPriority(
      string id,
      long childOrder,
      int priority,
      string catId,
      string type = "task")
    {
      TaskSortOrderInPriorityModel model = (await TaskSortOrderInPriorityDao.GetSortOrders(catId, new int?(priority), id, true)).FirstOrDefault<TaskSortOrderInPriorityModel>();
      if (model != null)
      {
        model.SortOrder = childOrder;
        model.SyncStatus = 1;
        model.Deleted = 0;
        model.UserId = LocalSettings.Settings.LoginUserId;
        await TaskSortOrderInPriorityDao.UpdateAsync(model);
      }
      else
        await TaskSortOrderInPriorityDao.InsertAsync(new TaskSortOrderInPriorityModel()
        {
          EntityId = id,
          EntityType = type,
          Priority = priority,
          CatId = catId,
          SyncStatus = 0,
          SortOrder = childOrder,
          UserId = LocalSettings.Settings.LoginUserId
        });
    }

    public static async Task InitSortModels(
      string sortKey,
      string cId,
      List<DisplayItemModel> allModels)
    {
      List<SyncSortOrderModel> asyncList = await TaskSortOrderService.GetAsyncList(sortKey, cId, true);
      long num1 = 0;
      List<SyncSortOrderModel> result = new List<SyncSortOrderModel>();
      if (asyncList != null)
        result.AddRange((IEnumerable<SyncSortOrderModel>) asyncList);
      List<SyncSortOrderModel> syncSortOrderModelList = new List<SyncSortOrderModel>();
      List<SyncSortOrderModel> update = new List<SyncSortOrderModel>();
      foreach (DisplayItemModel allModel in allModels)
      {
        DisplayItemModel m = allModel;
        SyncSortOrderModel syncSortOrderModel1 = asyncList != null ? asyncList.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (s => s.EntityId == m.EntityId)) : (SyncSortOrderModel) null;
        int entityTypeNum = EntityType.GetEntityTypeNum(m.Type);
        if (syncSortOrderModel1 == null)
        {
          SyncSortOrderModel syncSortOrderModel2 = new SyncSortOrderModel(sortKey)
          {
            EntityId = m.EntityId,
            SortOrder = num1,
            GroupId = cId,
            Type = entityTypeNum,
            SyncStatus = 1
          };
          syncSortOrderModelList.Add(syncSortOrderModel2);
          result.Add(syncSortOrderModel2);
        }
        else
        {
          syncSortOrderModel1.SortOrder = num1;
          syncSortOrderModel1.SyncStatus = 1;
          syncSortOrderModel1.Type = entityTypeNum;
          update.Add(syncSortOrderModel1);
        }
        m.SpecialOrder = num1;
        num1 += 268435456L;
      }
      if (syncSortOrderModelList.Any<SyncSortOrderModel>())
      {
        int num2 = await SyncSortOrderDao.InsertAllAsync(syncSortOrderModelList);
      }
      if (update.Any<SyncSortOrderModel>())
      {
        int num3 = await SyncSortOrderDao.UpdateAllAsync(update);
      }
      TaskSortOrderService.SetSortModels(sortKey, cId, result);
      result = (List<SyncSortOrderModel>) null;
      update = (List<SyncSortOrderModel>) null;
    }

    public static async Task ChangeOrderInType(
      string sortOrderType,
      long? frontOrder,
      long? nextOrder,
      List<DisplayItemModel> dragModels,
      string catId)
    {
      long? nullable1 = frontOrder;
      long valueOrDefault1;
      if (!nullable1.HasValue)
      {
        long? nullable2 = nextOrder;
        valueOrDefault1 = (nullable2.HasValue ? new long?(nullable2.GetValueOrDefault() - 268435456L) : new long?()).GetValueOrDefault();
      }
      else
        valueOrDefault1 = nullable1.GetValueOrDefault();
      long front = valueOrDefault1;
      long? nullable3 = nextOrder;
      long valueOrDefault2;
      if (!nullable3.HasValue)
      {
        nullable1 = frontOrder;
        valueOrDefault2 = (nullable1.HasValue ? new long?(nullable1.GetValueOrDefault() + 268435456L) : new long?()).GetValueOrDefault();
      }
      else
        valueOrDefault2 = nullable3.GetValueOrDefault();
      long step = (valueOrDefault2 - front) / (long) (dragModels.Count + 1);
      for (int i = 0; i < dragModels.Count; ++i)
      {
        long num = front + step * (long) (i + 1);
        SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertOrUpdateAsync(sortOrderType, catId, dragModels[i].EntityId, EntityType.GetEntityTypeNum(dragModels[i].Type), new long?(num));
      }
    }
  }
}
