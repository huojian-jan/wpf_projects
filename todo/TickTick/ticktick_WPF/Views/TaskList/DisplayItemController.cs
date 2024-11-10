// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.DisplayItemController
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.DateParser;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.CustomPopup;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Eisenhower;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList.Item;
using ticktick_WPF.Views.Time;
using ticktick_WPF.Views.Widget;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class DisplayItemController
  {
    protected UIElement Element;
    protected DisplayItemModel Model;
    private IToastShowWindow _parentWindow;
    private bool _canEdit = true;
    private bool _isInAnimation;
    private ITaskList _iTaskList;

    public DisplayItemController(UIElement element, DisplayItemModel model)
    {
      this.Model = model;
      this.Element = element;
    }

    protected ITaskList GetTaskList()
    {
      if (this._iTaskList == null)
        this._iTaskList = Utils.FindParent<ITaskList>((DependencyObject) this.Element);
      return this._iTaskList;
    }

    public void TitleGotFocus()
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      this.GetTaskList()?.TrySetTitleReadonly(model.Id);
    }

    public async void SelectItem(bool focusTitle = false, bool ignoreBatch = false)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      ignoreBatch = ignoreBatch || !model.CanBatchSelect;
      if (model.IsTaskOrNote)
        this.GetTaskList()?.SelectTask(model.TaskId, TaskSelectType.Click, ignoreBatch);
      else if (model.IsItem)
        this.GetTaskList()?.SelectSubtask(new IdExtra(model.Id, model.TaskId), TaskSelectType.Click);
      else
        this.GetTaskList()?.SelectItem(model.Id, model.Type);
    }

    public async Task<TaskDetailPopup> ShowDetailWindow(DisplayItemModel model, bool focusTitle)
    {
      if (model.IsTaskOrNote || model.IsItem)
      {
        TaskListView taskListView = this.GetTaskListView();
        if (taskListView != null)
          taskListView.SetSelected(new List<string>()
          {
            model.Id
          });
        return await this.ShowTaskDetail(focusTitle);
      }
      if (model.IsEvent)
        this.ShowCalDetail(model);
      return (TaskDetailPopup) null;
    }

    private async Task ShowCalDetail(DisplayItemModel model)
    {
      DisplayItemController displayItemController = this;
      string id = model.Id;
      if (string.IsNullOrEmpty(id))
        return;
      model.Selected = true;
      CalendarEventModel eventById = await CalendarEventDao.GetEventById(ArchivedDao.GetOriginalId(id));
      if (eventById == null)
        return;
      eventById.DueStart = model.StartDate;
      eventById.DueEnd = model.DueDate;
      CalendarDetailWindow calendarDetailWindow = new CalendarDetailWindow();
      calendarDetailWindow.Disappear -= new EventHandler<string>(displayItemController.OnWindowClosed);
      calendarDetailWindow.Disappear += new EventHandler<string>(displayItemController.OnWindowClosed);
      calendarDetailWindow.Show(displayItemController.Element, 0.0, 0.0, false, new CalendarDetailViewModel(eventById));
    }

    private void ToastUnableMessage(object sender, MouseButtonEventArgs e) => this.TryToastUnable();

    private bool TryToastUnable()
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return false;
      if (TeamDao.IsTeamExpired(model.TeamId))
      {
        this.GetParentWindow()?.TryToastString((object) null, Utils.GetString("TeamExpiredOperate"));
        return false;
      }
      if (model.Permission != null)
      {
        switch (model.Permission)
        {
          case "read":
            string str1 = Utils.GetString("ReadOnly");
            this.GetParentWindow()?.TryToastString((object) null, string.Format(Utils.GetString("NoPermissionToEdit"), (object) str1));
            return false;
          case "comment":
            string str2 = Utils.GetString("CanComment");
            this.GetParentWindow()?.TryToastString((object) null, string.Format(Utils.GetString("NoPermissionToEdit"), (object) str2));
            return false;
        }
      }
      return true;
    }

    public void OnCheckBoxClick(DisplayItemModel model, bool soundPlayed = false)
    {
      if (model.IsHabit)
      {
        model.IsToggling = false;
        if (this._isInAnimation)
          return;
        this.ToggleHabitCompleted(model);
      }
      else if (model.Enable)
      {
        this.ToggleItemCompleted(model, soundPlayed);
      }
      else
      {
        this.TryToastUnable();
        model.IsToggling = false;
      }
    }

    private async void ToggleHabitCompleted(DisplayItemModel model)
    {
      if (model.Status == 0)
      {
        try
        {
          await this.CheckInHabit();
        }
        catch (Exception ex)
        {
        }
      }
      else
        await this.ResetHabit();
    }

    private async Task ResetHabit()
    {
      DisplayItemModel model = this.Model;
      ITaskList taskList = this.GetTaskList();
      if (taskList == null)
      {
        model = (DisplayItemModel) null;
        taskList = (ITaskList) null;
      }
      else if (!this.GetTaskList().Editable())
      {
        model = (DisplayItemModel) null;
        taskList = (ITaskList) null;
      }
      else if (model?.Habit == null)
      {
        model = (DisplayItemModel) null;
        taskList = (ITaskList) null;
      }
      else
      {
        int num = await HabitService.ResetCheckInHabit(model.Id, DateTime.Today, model.Habit.Type) ? 1 : 0;
        taskList.ReLoad(model.Id);
        model = (DisplayItemModel) null;
        taskList = (ITaskList) null;
      }
    }

    private async Task CheckInHabit()
    {
      DisplayItemController displayItemController = this;
      DisplayItemModel model = displayItemController.Model;
      displayItemController._isInAnimation = true;
      if (displayItemController.GetTaskList() == null || !displayItemController.GetTaskList().Editable() || model?.Habit == null)
        return;
      if (model.Habit.IsBoolHabit())
        await displayItemController.CheckInBooleanHabit();
      else if (Math.Abs(model.Habit.Step + 1.0) <= 0.001)
        displayItemController.GetTaskListView()?.ManuallyRecordCheckIn(displayItemController.Element, model, new EventHandler<double>(displayItemController.OnCheckInPopupSave));
      else
        await displayItemController.CheckInRealHabit(model.Habit.Step);
      displayItemController._isInAnimation = false;
    }

    private void OnCheckInPopupSave(object sender, double e) => this.CheckInRealHabit(e);

    private async Task CheckInRealHabit(double step)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        model = (DisplayItemModel) null;
      else if (!model.IsHabit)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        if (model.Habit == null)
        {
          DisplayItemModel displayItemModel = model;
          displayItemModel.SetHabit(await HabitDao.GetHabitById(model.Id));
          displayItemModel = (DisplayItemModel) null;
          if (model.Habit == null)
          {
            model = (DisplayItemModel) null;
            return;
          }
        }
        model.SourceViewModel.Status = 2;
        Utils.PlayCompletionSound();
        if (model.HabitCheckIn == null)
        {
          model.SetHabitCheckIn(new HabitCheckInModel()
          {
            Goal = model.Habit.Goal,
            Value = step
          });
        }
        else
        {
          model.HabitCheckIn.Value += step;
          model.SetHabitCheckIn(model.HabitCheckIn);
        }
        this.GetTaskListView()?.ShowRecordPopup(this.Element, Math.Abs(step) < 1E-05 ? model.Habit.Goal : step, model.Habit.Unit);
        await HabitService.CheckInHabit(model.Id, DateTime.Today, step, delay: 200);
        await Task.Delay(200);
        model = (DisplayItemModel) null;
      }
    }

    private async Task CheckInBooleanHabit()
    {
      DisplayItemModel model = this.Model;
      ITaskList taskList;
      if (model == null)
      {
        model = (DisplayItemModel) null;
        taskList = (ITaskList) null;
      }
      else
      {
        taskList = this.GetTaskList();
        model.SourceViewModel.Status = 2;
        model.SourceViewModel.CompletedTime = new DateTime?(DateTime.Now);
        Utils.PlayCompletionSound();
        await Task.Delay(16);
        taskList?.AfterTaskChanged((DisplayItemModel) null);
        await HabitService.CheckInHabit(model.Id, DateTime.Today);
        ITaskList taskList1 = taskList;
        if (taskList1 == null)
        {
          model = (DisplayItemModel) null;
          taskList = (ITaskList) null;
        }
        else
        {
          taskList1.ReLoad((string) null);
          model = (DisplayItemModel) null;
          taskList = (ITaskList) null;
        }
      }
    }

    public async Task<bool> ShowOperationDialogSafely()
    {
      DisplayItemController displayItemController1 = this;
      try
      {
        DisplayItemController displayItemController = displayItemController1;
        DisplayItemModel model = displayItemController1.Model;
        if (model == null)
          return false;
        if (model.IsHabit && model.Status == 0)
        {
          List<OperationItemViewModel> types = new List<OperationItemViewModel>()
          {
            new OperationItemViewModel(ActionType.Skip)
          };
          if (LocalSettings.Settings.EnableFocus)
            types.Insert(0, new OperationItemViewModel(ActionType.StartFocus)
            {
              SubActions = new List<OperationItemViewModel>()
              {
                new OperationItemViewModel(ActionType.StartPomo)
                {
                  Enable = !TickFocusManager.Working || LocalSettings.Settings.PomoType == FocusConstance.Focus
                },
                new OperationItemViewModel(ActionType.StartTiming)
                {
                  Enable = !TickFocusManager.Working || LocalSettings.Settings.PomoType == FocusConstance.Timing
                }
              }
            });
          OperationDialog operationDialog = new OperationDialog(model.Id, types);
          operationDialog.SetPlaceTarget(displayItemController1.Element);
          operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) (async (obj, kv) =>
          {
            switch (kv.Value)
            {
              case ActionType.Skip:
                await HabitService.SkipHabit(model.Id);
                DataChangedNotifier.NotifyHabitSkip(model.Id);
                displayItemController.GetParentWindow()?.TryToastString((object) null, Utils.GetString("Skipped"));
                break;
              case ActionType.StartTiming:
                TickFocusManager.TryStartFocusHabit(kv.Key, false);
                break;
              case ActionType.StartPomo:
                TickFocusManager.TryStartFocusHabit(kv.Key, true);
                break;
            }
          });
          operationDialog.Show();
          return true;
        }
        if (model.IsEvent)
        {
          CalendarOperationDialog calendarOperationDialog = new CalendarOperationDialog(new EventArchiveArgs(model.SourceViewModel));
          // ISSUE: reference to a compiler-generated method
          calendarOperationDialog.Operated += new EventHandler<KeyValuePair<string, ActionType>>(displayItemController1.\u003CShowOperationDialogSafely\u003Eb__21_1);
          calendarOperationDialog.SetPlaceTarget(displayItemController1.Element);
          calendarOperationDialog.Show();
        }
        else if (model.IsCourse)
        {
          ArchiveOperationDialog archiveOperationDialog = new ArchiveOperationDialog(model.Id, ArchiveKind.Course);
          archiveOperationDialog.SetPlaceTarget(displayItemController1.Element);
          // ISSUE: reference to a compiler-generated method
          archiveOperationDialog.Operated += new EventHandler<KeyValuePair<string, ActionType>>(displayItemController1.\u003CShowOperationDialogSafely\u003Eb__21_2);
          archiveOperationDialog.Show();
        }
        else
        {
          if (!model.Enable)
          {
            if (!displayItemController1.TryToastUnable())
            {
              displayItemController1.SelectItem();
              return false;
            }
            if (model.Deleted != 0)
            {
              bool flag = TeamService.GetTeamId() == model.TeamId;
              string empty = string.Empty;
              List<OperationItemViewModel> types;
              if (!flag)
              {
                types = new List<OperationItemViewModel>()
                {
                  new OperationItemViewModel(ActionType.Restore),
                  new OperationItemViewModel(ActionType.DeleteForever)
                };
              }
              else
              {
                types = new List<OperationItemViewModel>();
                types.Add(new OperationItemViewModel(ActionType.Restore));
              }
              OperationDialog operationDialog = new OperationDialog(empty, types);
              operationDialog.SetPlaceTarget(displayItemController1.Element);
              operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) (async (o, e) =>
              {
                switch (e.Value)
                {
                  case ActionType.DeleteForever:
                    if (!new CustomerDialog(Utils.GetString("DeleteForever"), Utils.GetString("DeleteForeverHint"), Utils.GetString("Delete"), Utils.GetString("Cancel"))
                    {
                      Owner = (displayItemController.Element == null ? (Window) null : Window.GetWindow((DependencyObject) displayItemController.Element)),
                      Topmost = false
                    }.ShowDialog().GetValueOrDefault())
                      break;
                    await TaskService.DeleteTask(model.Id, 2);
                    break;
                  case ActionType.Restore:
                    await TaskService.BatchRestoreProject(new List<string>()
                    {
                      model.Id
                    });
                    break;
                }
              });
              operationDialog.Show();
            }
            return false;
          }
          await displayItemController1.ShowOperationDialog();
        }
        return true;
      }
      catch (Exception ex)
      {
      }
      return false;
    }

    protected virtual void OnEventArchived() => this.GetTaskList()?.OnItemArchived();

    private async Task ShowOperationDialog()
    {
      DisplayItemController displayItemController = this;
      ITaskList list = displayItemController.GetTaskList();
      if (list == null)
        ;
      else if (!list.Editable())
        ;
      else
      {
        DisplayItemModel model = displayItemController.Model;
        if (model == null)
          ;
        else if (!model.IsTaskOrNote)
          ;
        else if (string.IsNullOrEmpty(model.Id))
          ;
        else
        {
          bool? pinned = new bool?();
          if (model.IsTaskOrNote)
          {
            TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskId);
            pinned = new bool?(thinTaskById != null && thinTaskById.pinnedTimeStamp > 0L);
          }
          OperationExtra taskAccessInfo = await TaskOperationHelper.GetTaskAccessInfo(model.Id, list.CanAddSubTask() && model.Enable && (model.InDetail || model.Status == 0));
          if (taskAccessInfo == null)
            ;
          else
          {
            taskAccessInfo.ShowCopy = list.Copyable();
            taskAccessInfo.ShowCopyLink = taskAccessInfo.ShowCopy;
            taskAccessInfo.IsPinned = pinned;
            TaskOperationDialog dialog = new TaskOperationDialog(taskAccessInfo, displayItemController.Element);
            if (Window.GetWindow((DependencyObject) displayItemController.Element) is WidgetWindow window)
              dialog.Resources = window.Resources;
            dialog.CreateSubTask += new EventHandler(displayItemController.CreateSubTask);
            dialog.Copied += new EventHandler(displayItemController.CopyTask);
            dialog.LinkCopied += new EventHandler(displayItemController.CopyTaskLink);
            dialog.Deleted += new EventHandler(displayItemController.DeleteTask);
            dialog.AbandonOrReopen += new EventHandler(displayItemController.OnAbandonOrReopenTask);
            dialog.PrioritySelect += new EventHandler<int>(displayItemController.SetPriority);
            dialog.ProjectSelect += new EventHandler<SelectableItemViewModel>(displayItemController.OnProjectSelect);
            dialog.AssigneeSelect += new EventHandler<AvatarInfo>(displayItemController.OnAssigneeSelect);
            dialog.TagsSelect += new EventHandler<TagSelectData>(displayItemController.OnTagsSelect);
            dialog.SkipCurrentRecurrence += new EventHandler(displayItemController.OnSkipRecurrence);
            dialog.TimeClear += (EventHandler) ((arg, obj) =>
            {
              dialog.Dismiss();
              this.OnTimeClear();
              list.SetDetailInOperation(false);
            });
            dialog.TimeSelect += (EventHandler<TimeData>) ((arg, data) =>
            {
              dialog.Dismiss();
              this.SetReminderAndRepeat(data);
              UtilLog.Info("ListItem.SetDate id " + model.Id + " from:taskOperation");
              list.SetDetailInOperation(false);
            });
            dialog.QuickDateSelect += (EventHandler<DateTime>) ((arg, date) =>
            {
              dialog.Dismiss();
              this.SetStartDate(date);
              UtilLog.Info(string.Format("ListItem.SetDate id {0} value {1} from:taskOperation.QuickSet", (object) model.Id, (object) date));
              list.SetDetailInOperation(false);
            });
            dialog.CompleteDateChanged += (EventHandler<DateTime>) (async (o, date) =>
            {
              await TaskService.ChangeCompleteDate(model.TaskId, date);
              this.GetTaskList()?.ReLoad(model.TaskId);
              SyncManager.TryDelaySync();
            });
            dialog.SwitchTaskOrNote += new EventHandler(displayItemController.OnSwitchClick);
            dialog.Toast += new EventHandler<string>(displayItemController.OnTaskOperationToast);
            dialog.Starred += new EventHandler<bool>(displayItemController.OnStarredClick);
            dialog.Closed += (EventHandler) ((arg, e) =>
            {
              if (!model.InDetail)
                PopupStateManager.OnViewPopupClosed();
              model.InOperation = false;
            });
            dialog.Disappear += (EventHandler<bool>) ((arg, e) =>
            {
              list.SetDetailInOperation(false, e);
              this.GetTaskListView()?.SetMatrixInOperation(false);
            });
            list.SetDetailInOperation(true);
            dialog.Show();
            model.InOperation = true;
            if (!model.InDetail)
              PopupStateManager.OnViewPopupOpened();
            TaskListView taskListView = displayItemController.GetTaskListView();
            if (taskListView == null)
              ;
            else
              taskListView.SetMatrixInOperation(true);
          }
        }
      }
    }

    private void OnTaskOperationToast(object sender, string e) => this.GetTaskListView()?.Toast(e);

    private async void OnAbandonOrReopenTask(object sender, EventArgs e)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      UtilLog.Info("ListItem.AbandonOrReopenTask " + model.Id + ", currentStatus " + model.Id);
      int closeStatus = model.IsAbandoned ? 0 : -1;
      model.SourceViewModel.Status = closeStatus;
      TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(model.TaskId, closeStatus, closeStatus != 0, this.GetTaskListView()?.GetToastParent());
    }

    private async void OnStarredClick(object sender, bool isPin)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      string catId = this.GetTaskListView()?.GetIdentity()?.CatId;
      string id = model.Id;
      string projectId = catId;
      bool inDetail = model.InDetail;
      bool? isPin1 = new bool?();
      long? sortOrder = new long?();
      int num = inDetail ? 1 : 0;
      await TaskService.TogglesStarred(id, projectId, isPin1, sortOrder, num != 0);
    }

    private async void OnSwitchClick(object sender, EventArgs e)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        UtilLog.Info("ListItem.SwitchTaskNote " + model.Id + ", currentKind " + model.Kind);
        await TaskService.SwitchTaskOrNote(model.Id);
        this.GetTaskList()?.AfterTaskChanged(model, true);
        SyncManager.TryDelaySync();
        model = (DisplayItemModel) null;
      }
    }

    private async void CreateSubTask(object sender, EventArgs e)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      this.GetTaskList()?.CreateSubTask(model, true);
    }

    private async void CopyTaskLink(object sender, EventArgs e)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(this.Model?.Id);
      if (thinTaskById == null)
        return;
      TaskUtils.CopyTaskLink(thinTaskById.id, thinTaskById.projectId, thinTaskById.title);
    }

    private async void OnSkipRecurrence(object sender, EventArgs e)
    {
      TaskModel taskModel = await TaskService.SkipCurrentRecurrence(this.Model.Id, toastWindow: this.GetParentWindow());
    }

    private async void OnAssigneeSelect(object sender, AvatarInfo assignee)
    {
      this.OnAssigneeSelect(assignee);
    }

    private async Task OnAssigneeSelect(AvatarInfo assignee)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      model.SourceViewModel.Assignee = assignee.UserId;
      model.AvatarUrl = assignee.AvatarUrl;
      if (this.GetTaskList() == null)
        return;
      await TaskService.SetAssignee(model.Id, assignee.UserId);
    }

    private async void OnTagsSelect(object sender, TagSelectData data)
    {
      List<string> omniSelectTags = data.OmniSelectTags;
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        await TaskService.SetTags(model.Id, omniSelectTags);
        UtilLog.Info("ListItem.SetTags id " + this.Model?.Id + "  from:taskOperation");
        this.TryToastQuadrantChanged(model.Id);
        model = (DisplayItemModel) null;
      }
    }

    private async Task OnProjectSelect(ProjectModel project, bool canReload, string columnId = null)
    {
      DisplayItemModel model = this.Model;
      bool projectChanged = model?.ProjectId != project?.id;
      ITaskList taskList;
      if (model == null)
      {
        model = (DisplayItemModel) null;
        taskList = (ITaskList) null;
      }
      else if (project == null)
      {
        model = (DisplayItemModel) null;
        taskList = (ITaskList) null;
      }
      else
      {
        if (!projectChanged)
        {
          if (model.ColumnId == columnId)
          {
            model = (DisplayItemModel) null;
            taskList = (ITaskList) null;
            return;
          }
          if (string.IsNullOrEmpty(columnId))
          {
            model = (DisplayItemModel) null;
            taskList = (ITaskList) null;
            return;
          }
        }
        taskList = this.GetTaskList();
        List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(model.TaskId, model.ProjectId);
        // ISSUE: explicit non-virtual call
        if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
        {
          List<string> ids = subTasksByIdAsync.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>();
          ids.Add(model.TaskId);
          await TaskDao.RemoveTaskParentId(model.TaskId);
          UtilLog.Info("SetParent id " + model.TaskId + ",parent null, from:setProject");
          await TaskService.BatchMoveProject(ids, new MoveProjectArgs(project)
          {
            ColumnId = columnId
          });
          UtilLog.Info("SetChildrenProject ids " + ids.Join<string>(";") + ",project " + project.id + ", from:setProject");
          if (!canReload)
          {
            model = (DisplayItemModel) null;
            taskList = (ITaskList) null;
          }
          else
          {
            taskList.AfterTaskChanged(model);
            if (!projectChanged)
            {
              model = (DisplayItemModel) null;
              taskList = (ITaskList) null;
            }
            else
            {
              taskList.AfterTaskProjectChanged(model);
              model = (DisplayItemModel) null;
              taskList = (ITaskList) null;
            }
          }
        }
        else
        {
          if (taskList != null)
          {
            string id1 = model.Id;
            string id2 = project.id;
            string str = columnId;
            bool? isTop = new bool?();
            string columnId1 = str;
            await TaskService.MoveProject(id1, id2, isTop, columnId1);
            if (canReload)
            {
              taskList.AfterTaskChanged(model);
              if (projectChanged)
                taskList.AfterTaskProjectChanged(model);
            }
          }
          this.TryToastQuadrantChanged(model.Id);
          model = (DisplayItemModel) null;
          taskList = (ITaskList) null;
        }
      }
    }

    private async void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      DisplayItemModel model = this.Model;
      (string projectId, string columnId) = e.GetProjectAndColumnId();
      UtilLog.Info("ListItem.SetProject id " + model?.Id + ",value " + projectId + " from:taskOperation");
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      if (projectById == null)
        return;
      await this.OnProjectSelect(projectById, true, columnId);
    }

    private async void CopyTask(object sender, EventArgs e)
    {
      if (this.Model == null)
        return;
      this.GetTaskList()?.CopyTask(this.Model.Id);
    }

    private void DeleteTask(object sender, EventArgs e) => this.DeleteTask();

    public void DeleteTask()
    {
      if (this.Model == null)
        return;
      this.GetTaskList()?.DeleteTask(this.Model.Id, TaskDeleteType.DeleteButton);
    }

    public void DeleteSelectedTasks() => this.GetTaskList()?.DeleteSelectedTasks();

    private async void SetReminderAndRepeat(TimeData timeData)
    {
      TaskService.TryFixRepeatFlag(ref timeData);
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        TaskListView taskListView = this.GetTaskListView();
        if (taskListView != null && taskListView.ViewModel.InDetail && !string.IsNullOrEmpty(model.RepeatFlag) && model.Status == 0)
        {
          TaskDetailWindow parent = Utils.FindParent<TaskDetailWindow>((DependencyObject) taskListView);
          if ((parent != null ? (parent.ShowInCal ? 1 : 0) : 0) != 0)
          {
            ModifyRepeatHandler.TryUpdateDueDate(model.TaskId, model.StartDate, model.DueDate, timeData, 1, 1);
            model = (DisplayItemModel) null;
            return;
          }
        }
        if (this.GetTaskList() == null)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          await TaskService.SetDate(model.Id, timeData);
          this.TryToastQuadrantChanged(model.Id);
          model = (DisplayItemModel) null;
        }
      }
    }

    private async Task SetStartDate(DateTime startDate)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        model = (DisplayItemModel) null;
      else if (model.IsTaskOrNote)
      {
        TaskListView taskListView = this.GetTaskListView();
        if (taskListView != null && taskListView.ViewModel.InDetail && !string.IsNullOrEmpty(model.RepeatFlag) && model.Status == 0)
        {
          TaskDetailWindow parent = Utils.FindParent<TaskDetailWindow>((DependencyObject) taskListView);
          if ((parent != null ? (parent.ShowInCal ? 1 : 0) : 0) != 0)
          {
            TaskModel task = await TaskDao.GetThinTaskById(model.TaskId);
            if (task == null)
            {
              model = (DisplayItemModel) null;
              return;
            }
            if (!model.StartDate.HasValue)
            {
              model = (DisplayItemModel) null;
              return;
            }
            List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
            ModifyRepeatHandler.TryUpdateDueDateOnlyDate(model.TaskId, model.StartDate, model.DueDate, new TimeData()
            {
              StartDate = model.StartDate,
              DueDate = model.DueDate,
              IsAllDay = new bool?(!model.StartDate.HasValue || ((int) model.IsAllDay ?? 1) != 0),
              Reminders = remindersByTaskId,
              RepeatFrom = model.RepeatFrom,
              RepeatFlag = model.RepeatFlag,
              ExDates = task.exDate != null ? ((IEnumerable<string>) task.exDate).ToList<string>() : new List<string>()
            }, startDate, 1, 1);
            model = (DisplayItemModel) null;
            return;
          }
        }
        if (!string.IsNullOrEmpty(model.AttendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) model))
        {
          IToastShowWindow parentWindow = this.GetParentWindow();
          if (parentWindow == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            parentWindow.TryToastString((object) null, Utils.GetString("AttendeeSetDate"));
            model = (DisplayItemModel) null;
          }
        }
        else
        {
          TimeData timeData = new TimeData()
          {
            StartDate = model.StartDate,
            DueDate = model.DueDate,
            IsAllDay = model.IsAllDay,
            RepeatFlag = model.RepeatFlag,
            RepeatFrom = model.RepeatFrom
          };
          TaskService.ChangeStartDate(ref timeData, startDate);
          TaskModel taskModel = await TaskService.SetStartDate(model.Id, startDate);
          this.TryToastQuadrantChanged(model.Id);
          model = (DisplayItemModel) null;
        }
      }
      else if (!model.IsItem)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskId);
        if (!string.IsNullOrEmpty(thinTaskById.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) thinTaskById))
        {
          IToastShowWindow parentWindow = this.GetParentWindow();
          if (parentWindow == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            parentWindow.TryToastString((object) null, Utils.GetString("AttendeeSetDate"));
            model = (DisplayItemModel) null;
          }
        }
        else
        {
          TaskDetailItemModel taskDetailItemModel = await TaskService.SetSubtaskDate(model.TaskId, model.Id, startDate);
          model = (DisplayItemModel) null;
        }
      }
    }

    private async Task OnTimeClear()
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        model = (DisplayItemModel) null;
      else if (this.Element == null)
        model = (DisplayItemModel) null;
      else if (!string.IsNullOrEmpty(model.AttendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) model))
      {
        IToastShowWindow parentWindow = this.GetParentWindow();
        if (parentWindow == null)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          parentWindow.TryToastString((object) null, Utils.GetString("AttendeeSetDate"));
          model = (DisplayItemModel) null;
        }
      }
      else if (model.Type == DisplayType.Task || model.Type == DisplayType.Agenda || model.IsNote)
      {
        if (!await TaskOperationHelper.CheckIfTaskAllowClearDate(model.TaskId, (DependencyObject) this.Element))
          model = (DisplayItemModel) null;
        else if (this.GetTaskList() == null)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          await TaskService.ClearDate(model.Id);
          this.TryToastQuadrantChanged(model.Id);
          model = (DisplayItemModel) null;
        }
      }
      else if (model.Type != DisplayType.CheckItem)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskId);
        if (thinTaskById != null && !string.IsNullOrEmpty(thinTaskById.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) thinTaskById))
        {
          IToastShowWindow parentWindow = this.GetParentWindow();
          if (parentWindow == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            parentWindow.TryToastString((object) null, Utils.GetString("AttendeeSetDate"));
            model = (DisplayItemModel) null;
          }
        }
        else
        {
          await TaskService.SetCheckItemDate(model.Id, model.TaskId, new TimeDataModel()
          {
            StartDate = new DateTime?(),
            IsAllDay = new bool?()
          });
          model = (DisplayItemModel) null;
        }
      }
    }

    public async Task SetPriority(int priority, bool checkBatch = false)
    {
      if (checkBatch)
      {
        TaskListView taskListView = this.GetTaskListView();
        int num1;
        if (taskListView == null)
        {
          num1 = 0;
        }
        else
        {
          int? count = taskListView.GetSelectedTaskIds()?.Count;
          int num2 = 0;
          num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
        }
        if (num1 != 0)
          return;
      }
      if (this.Model == null || !this.Model.Enable || this.Model.IsNote)
        return;
      UtilLog.Info(string.Format("ListItem.SetPriority id {0},value {1} from:shortCut", (object) this.Model?.Id, (object) priority));
      await this.SetPriority(priority);
    }

    private async Task SetTag(string tag)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        string[] tags1 = model.Tags;
        List<string> tags2 = (tags1 != null ? ((IEnumerable<string>) tags1).ToList<string>() : (List<string>) null) ?? new List<string>();
        if (!tags2.Contains(tag))
          tags2.Add(tag);
        await TaskService.SetTags(model.TaskId, tags2);
        this.TryToastQuadrantChanged(model.Id);
        model = (DisplayItemModel) null;
      }
    }

    public async void SelectDate(FrameworkElement target = null, double hOffset = 100.0, double vOffset = 12.0)
    {
      DisplayItemController displayItemController = this;
      DisplayItemModel model = displayItemController.Model;
      TaskModel originalTask;
      if (model == null)
      {
        model = (DisplayItemModel) null;
        originalTask = (TaskModel) null;
      }
      else if (displayItemController.Element == null)
      {
        model = (DisplayItemModel) null;
        originalTask = (TaskModel) null;
      }
      else if (!model.Enable)
      {
        model = (DisplayItemModel) null;
        originalTask = (TaskModel) null;
      }
      else if (!string.IsNullOrEmpty(model.AttendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) model))
      {
        IToastShowWindow parentWindow = displayItemController.GetParentWindow();
        if (parentWindow == null)
        {
          model = (DisplayItemModel) null;
          originalTask = (TaskModel) null;
        }
        else
        {
          parentWindow.TryToastString((object) null, Utils.GetString("AttendeeSetDate"));
          model = (DisplayItemModel) null;
          originalTask = (TaskModel) null;
        }
      }
      else
      {
        originalTask = await TaskDao.GetThinTaskById(model.Id);
        if (originalTask == null)
        {
          model = (DisplayItemModel) null;
          originalTask = (TaskModel) null;
        }
        else
        {
          List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(model.Id);
          TimeData timeData = new TimeData()
          {
            StartDate = originalTask.startDate,
            DueDate = originalTask.dueDate,
            IsAllDay = new bool?(!originalTask.startDate.HasValue || ((int) originalTask.isAllDay ?? 1) != 0),
            Reminders = remindersByTaskId != null ? remindersByTaskId.ToList<TaskReminderModel>() : (List<TaskReminderModel>) null,
            RepeatFrom = originalTask.repeatFrom,
            RepeatFlag = originalTask.repeatFlag,
            HasTime = originalTask.startDate.HasValue,
            TimeZone = new TimeZoneViewModel(originalTask.Floating, originalTask.timeZone),
            IsDefault = !originalTask.startDate.HasValue
          };
          SetDateDialog dialog = SetDateDialog.GetDialog();
          dialog.ClearEventHandle();
          // ISSUE: reference to a compiler-generated method
          dialog.Clear += new EventHandler(displayItemController.\u003CSelectDate\u003Eb__45_0);
          // ISSUE: reference to a compiler-generated method
          dialog.Save += new EventHandler<TimeData>(displayItemController.\u003CSelectDate\u003Eb__45_1);
          // ISSUE: reference to a compiler-generated method
          dialog.Hided += new EventHandler(displayItemController.\u003CSelectDate\u003Eb__45_2);
          dialog.SkipRecurrence += new EventHandler(displayItemController.OnSkipRecurrence);
          dialog.Show(timeData, new SetDateDialogArgs(isNote: originalTask.kind == "NOTE", target: (UIElement) target ?? displayItemController.Element, hOffset: hOffset, vOffset: vOffset, canSkip: !TaskService.IsNonRepeatTask(originalTask)));
          ITaskList taskList = displayItemController.GetTaskList();
          if (taskList == null)
          {
            model = (DisplayItemModel) null;
            originalTask = (TaskModel) null;
          }
          else
          {
            taskList.SetDetailInOperation(true);
            model = (DisplayItemModel) null;
            originalTask = (TaskModel) null;
          }
        }
      }
    }

    public async void SetDate(string key)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        model = (DisplayItemModel) null;
      else if (!model.Enable)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        DateTime date = DateTime.Today;
        switch (key)
        {
          case "today":
            date = DateTime.Today;
            break;
          case "tomorrow":
            date = DateTime.Today.AddDays(1.0);
            break;
          case "nextweek":
            date = DateTime.Today.AddDays(7.0);
            break;
        }
        await this.SetStartDate(date);
        UtilLog.Info(string.Format("ListItem.SetDate id {0} value {1} from:shortCut", (object) model.Id, (object) date));
        this.TryToastQuadrantChanged(model.Id);
        model = (DisplayItemModel) null;
      }
    }

    public virtual async Task<TaskDetailPopup> ShowTaskDetail(bool focusTitle = false)
    {
      DisplayItemController displayItemController = this;
      DisplayItemModel model = displayItemController.Model;
      if (model == null || string.IsNullOrEmpty(model.TaskId) || displayItemController.Element == null)
        return (TaskDetailPopup) null;
      TaskDetailViewModel model1 = await TaskDetailViewModel.Build(model.GetTaskId());
      if (model1 == null)
        return (TaskDetailPopup) null;
      TaskDetailPopup taskDetailPopup = new TaskDetailPopup();
      IToastShowWindow parentWindow = displayItemController.GetParentWindow();
      if (parentWindow != null)
      {
        if (PopupStateManager.LastTarget == displayItemController.Element)
          return (TaskDetailPopup) null;
        taskDetailPopup.DependentWindow = parentWindow;
      }
      model.Selected = true;
      taskDetailPopup.Disappear -= new EventHandler<string>(displayItemController.OnWindowClosed);
      taskDetailPopup.Disappear += new EventHandler<string>(displayItemController.OnWindowClosed);
      taskDetailPopup.Detail.SubTaskChanged += new EventHandler(displayItemController.OnChildrenChanged);
      taskDetailPopup.Tag = (object) displayItemController.GetTaskList();
      taskDetailPopup.Detail.ShowUndoOnTaskDeleted += new EventHandler<string>(displayItemController.OnTaskDeleted);
      int qLevel = 0;
      if (model.InMatrix)
      {
        UserActCollectUtils.AddClickEvent("matrix", "matrix_action", "task_detail");
        QuadrantControl parent = Utils.FindParent<QuadrantControl>((DependencyObject) displayItemController.Element);
        qLevel = parent != null ? parent.Level : 0;
      }
      Point point = new Point();
      if (!focusTitle && model.InMatrix && displayItemController.Element is TaskListItem element1)
        point = element1.GetClickPoint();
      if (model.InMatrix)
        point = new Point(10.0, 10.0);
      TaskListView taskListView = displayItemController.GetTaskListView();
      if (taskListView != null)
      {
        taskListView.SetSelectedIdOnShowDetail(model.TaskId);
        taskListView.SetMatrixInOperation(true);
      }
      taskDetailPopup.Show(model1, string.Empty, new TaskWindowDisplayArgs(displayItemController.Element, displayItemController.Element is FrameworkElement element2 ? element2.ActualWidth : 262.0, point, focusTitle, qLevel: qLevel));
      return taskDetailPopup;
    }

    private IToastShowWindow GetParentWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.FindParent<IToastShowWindow>((DependencyObject) this.Element);
      return this._parentWindow;
    }

    private void OnTaskDeleted(object sender, string e)
    {
      if (this.Model == null)
        return;
      this.GetTaskList()?.RemoveItemById(e);
    }

    private void OnChildrenChanged(object sender, EventArgs e)
    {
      this.GetTaskList()?.ReLoad(this.Model?.Id);
    }

    private async void OnWindowClosed(object sender, string e)
    {
      DisplayItemController displayItemController = this;
      bool flag = Utils.IfShiftPressed() || Utils.IfCtrlPressed();
      if (sender is TaskDetailPopup taskDetailPopup)
      {
        taskDetailPopup.Detail.SubTaskChanged -= new EventHandler(displayItemController.OnChildrenChanged);
        taskDetailPopup.Detail.ShowUndoOnTaskDeleted -= new EventHandler<string>(displayItemController.OnTaskDeleted);
        taskDetailPopup.Disappear -= new EventHandler<string>(displayItemController.OnWindowClosed);
        if (taskDetailPopup.Tag is ITaskList tag)
        {
          if (!flag && tag is TaskListView taskListView1)
            taskListView1.RemoveSelected(e);
          if (tag is TaskListView taskListView2)
            taskListView2.SetMatrixInOperation(false);
        }
        taskDetailPopup.Tag = (object) null;
      }
      if (displayItemController.Model != null && !flag)
        displayItemController.Model.Selected = false;
      displayItemController.ReloadOnDetailClosed();
    }

    public virtual void ReloadOnDetailClosed()
    {
    }

    public async void ToggleItemCompleted(DisplayItemModel model, bool soundPlayed = false)
    {
      if (TaskDragHelpModel.DragHelp.IsDragging)
        return;
      if (!model.Enable)
        model.IsToggling = false;
      else if (model.Deleted != 0)
        model.IsToggling = false;
      else if (model.IsTask)
      {
        this.ToggleTaskCompleted(model, soundPlayed);
      }
      else
      {
        if (!model.IsItem)
          return;
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskId);
        if (!string.IsNullOrEmpty(thinTaskById.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) thinTaskById))
        {
          UtilLog.Info("ListItem.CompleteItem id " + model.Id + " not owner");
          this.GetParentWindow()?.TryToastString((object) null, Utils.GetString("OnlyOwnerCanCompleteSubtask"));
        }
        else
        {
          UtilLog.Info(string.Format("ListItem.CompleteItem id {0},currentStatus{1}", (object) model.Id, (object) model.Status));
          this.GetTaskList()?.CompleteCheckitem(model.Id, !soundPlayed);
          model.IsToggling = false;
        }
      }
    }

    private async void ToggleTaskCompleted(DisplayItemModel model, bool soundPlayed = false)
    {
      if (model == null)
        return;
      int status = model.Status;
      UtilLog.Info(string.Format("ListItem.CompleteTask id {0},currentStatus{1}", (object) model.Id, (object) model.Status));
      if (status == 0)
      {
        if (model.InMatrix)
          UserActCollectUtils.AddClickEvent("matrix", "matrix_action", "complete_task");
        this.CompleteTask(model, soundPlayed);
      }
      else
        this.UnCompleteTask(model);
    }

    private async void CompleteTask(DisplayItemModel model, bool soundPlayed = false)
    {
      ITaskList taskList = this.GetTaskList();
      if (taskList == null || !taskList.Editable())
      {
        model.IsToggling = false;
        taskList = (ITaskList) null;
      }
      else
      {
        model.SourceViewModel.Status = 2;
        model.SourceViewModel.CompletedTime = new DateTime?(DateTime.Now);
        model.SourceViewModel.CompletedUser = LocalSettings.Settings.LoginUserId;
        await Task.Delay(150);
        TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(model.Id, 2, true, taskList.GetToastParent(), playSound: !soundPlayed);
        await Task.Delay(10);
        model.IsToggling = false;
        if (!(taskList is TaskListControl taskListControl))
        {
          taskList = (ITaskList) null;
        }
        else
        {
          Window window = Window.GetWindow((DependencyObject) taskListControl);
          if ((window != null ? (window.Equals((object) App.Window) ? 1 : 0) : 0) == 0)
          {
            taskList = (ITaskList) null;
          }
          else
          {
            App.Window.TryFocus();
            taskList = (ITaskList) null;
          }
        }
      }
    }

    private async void UnCompleteTask(DisplayItemModel model)
    {
      ITaskList taskList = this.GetTaskList();
      if (taskList == null || !taskList.Editable())
      {
        model.IsToggling = false;
        taskList = (ITaskList) null;
      }
      else
      {
        model.SourceViewModel.Status = 0;
        model.SourceViewModel.CompletedTime = new DateTime?();
        await Task.Delay(100);
        TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(model.Id, 0);
        model.IsToggling = false;
        taskList.AfterTaskChanged((DisplayItemModel) null);
        taskList = (ITaskList) null;
      }
    }

    private async void SetPriority(object sender, int priority)
    {
      UtilLog.Info(string.Format("ListItem.SetPriority id {0},value {1} from:taskOperation", (object) this.Model?.Id, (object) priority));
      this.SetPriority(priority);
    }

    private async Task SetPriority(int priority)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        ITaskList taskList = this.GetTaskList();
        model.SourceViewModel.Priority = priority;
        if (taskList == null)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          await TaskService.SetPriority(model.TaskId, priority);
          this.TryToastQuadrantChanged(model.Id);
          model = (DisplayItemModel) null;
        }
      }
    }

    public async void ClearDate()
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        model = (DisplayItemModel) null;
      else if (!model.Enable)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        ITaskList list = this.GetTaskList();
        await this.OnTimeClear();
        await Task.Delay(300);
        list?.FocusItem(model.Id);
        list = (ITaskList) null;
        model = (DisplayItemModel) null;
      }
    }

    public async void OnTagDelete(string tag)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        List<string> list = ((IEnumerable<string>) model.Tags).ToList<string>();
        if (list.Contains(tag))
          list.Remove(tag);
        await TaskService.SetTags(model.TaskId, list);
        UtilLog.Info("ListItem.DeleteTag id " + this.Model?.Id + " tag " + tag);
        SyncManager.TryDelaySync();
        this.TryToastQuadrantChanged(model.Id);
        model = (DisplayItemModel) null;
      }
    }

    public void MoveUp()
    {
      if (Utils.IfShiftPressed() && !this.Model.InDetail)
        this.GetTaskList()?.BatchSelectOnMove(true);
      else
        this.GetTaskList()?.MoveUp(this.Model.TaskId, this.Model.Type == DisplayType.CheckItem ? this.Model.Id : "");
    }

    public void MoveDown()
    {
      if (Utils.IfShiftPressed() && !this.Model.InDetail)
        this.GetTaskList()?.BatchSelectOnMove(false);
      else
        this.GetTaskList()?.MoveDown(this.Model.TaskId, this.Model.Type == DisplayType.CheckItem ? this.Model.Id : "");
    }

    public void MoveUp(string taskId, string itemId = "")
    {
      if (Utils.IfShiftPressed() && !this.Model.InDetail)
        this.GetTaskList()?.BatchSelectOnMove(true);
      else
        this.GetTaskList()?.MoveUp(taskId, itemId);
    }

    public void MoveDown(string taskId, string itemId = "")
    {
      if (Utils.IfShiftPressed() && !this.Model.InDetail)
        this.GetTaskList()?.BatchSelectOnMove(false);
      else
        this.GetTaskList()?.MoveDown(taskId, itemId);
    }

    public void OnMergeText(string text)
    {
      if (!string.IsNullOrEmpty(text))
        return;
      this.GetTaskList()?.DeleteTask(this.Model.Id, TaskDeleteType.RemoveText);
    }

    public void OnDragMouseDown(object sender, MouseEventArgs e)
    {
      if (!this.Model.Enable)
        return;
      this.GetTaskList()?.StartDrag(this.Model, e);
    }

    public void TitleSelectionChanged(int caretIndex)
    {
      this.GetTaskList()?.SetTitleCaretIndex(caretIndex);
    }

    public void OnMultipleTextPaste(string e) => this.GetTaskList()?.MultipleTextPaste(e);

    public void SplitNewTask(SplitData data)
    {
      DisplayItemModel model = this.Model;
      if (model == null || !model.Enable || !model.IsTaskOrNote)
        return;
      this.GetTaskList()?.SplitDisplayItem(model.Id);
    }

    public void TitleTextChanged(string text)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      ITaskList taskList = this.GetTaskList();
      if (taskList == null)
        return;
      if (model.IsTaskOrNote)
        taskList.TaskTitleChanged(model.Id, text);
      else if (model.IsItem)
      {
        taskList.SubtaskTitleChanged(model.TaskId, model.Id, text);
      }
      else
      {
        if (!model.IsEvent)
          return;
        taskList.EventTitleChanged(model.Id, text);
      }
    }

    public void ShowSelectAssignPopup()
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      if (!model.Enable)
      {
        this.TryToastUnable();
      }
      else
      {
        SetAssigneeDialog setAssigneeDialog = new SetAssigneeDialog(this.Model?.ProjectId, assignee: this.Model?.Assignee);
        setAssigneeDialog.AssigneeSelect += new EventHandler<AvatarInfo>(this.OnAssigneeSelect);
        setAssigneeDialog.Show();
      }
    }

    public async Task OnInputSelected(QuickSetModel setModel, string text, bool canReload)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        string id = model.Id;
        await this.GetTaskList().TaskTitleChanged(id, text);
        switch (setModel.Type)
        {
          case QuickSetType.Priority:
            if (!model.IsNote)
              await this.SetPriority(setModel.Priority);
            UtilLog.Info(string.Format("ListItem.SetPriority id {0},value {1} from:quickSelect", (object) model.Id, (object) setModel.Priority));
            model = (DisplayItemModel) null;
            break;
          case QuickSetType.Project:
            UtilLog.Info("ListItem.SetProject id " + model.Id + ",value " + setModel.Project?.id + " from:quickSelect");
            await this.OnProjectSelect(setModel.Project, canReload);
            model = (DisplayItemModel) null;
            break;
          case QuickSetType.Tag:
            await this.SetTag(setModel.Tag.ToLower());
            UtilLog.Info("ListItem.SetTag id " + model.Id + ",value " + setModel.Tag + " from:quickSelect");
            model = (DisplayItemModel) null;
            break;
          case QuickSetType.Date:
            if (setModel.Date.HasValue)
              await this.SetStartDate(setModel.Date.Value);
            UtilLog.Info(string.Format("ListItem.SetDate id {0},value {1} from:quickSelect", (object) model.Id, (object) setModel.Date));
            model = (DisplayItemModel) null;
            break;
          case QuickSetType.Assign:
            await this.OnAssigneeSelect(new AvatarInfo(setModel.Avatar.UserId, setModel.Avatar.AvatarUrl));
            model = (DisplayItemModel) null;
            break;
          default:
            model = (DisplayItemModel) null;
            break;
        }
      }
    }

    public void OnOpenPathClick(object sender, MouseButtonEventArgs e)
    {
      Utils.FindParent<ISectionList>((DependencyObject) this.Element)?.OnTaskOpenClick(this.Model);
    }

    public void OnCheckBoxRightMouseUp(UIElement ui)
    {
      ui = ui ?? this.Element;
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      ui = ui ?? this.Element;
      ITaskList list = this.GetTaskList();
      if (!model.Enable)
      {
        this.TryToastUnable();
      }
      else
      {
        if (!model.IsTask)
          return;
        SetClosedStatusPopup instance = SetClosedStatusPopup.GetInstance(ui, model.IsCompleted, model.IsAbandoned, 3.0, -5.0);
        instance.Abandoned += (EventHandler) ((s, e) =>
        {
          UtilLog.Info("TaskList.AbandonTask " + model.Id + " from:CheckIconRightClick");
          TaskService.SetTaskStatus(model.Id, -1, true, list?.GetToastParent());
        });
        instance.Completed += (EventHandler) (async (s, e) =>
        {
          UtilLog.Info("TaskList.CompleteTask " + model.Id + " from:CheckIconRightClick");
          bool playSound = true;
          TaskListItem parent = Utils.FindParent<TaskListItem>((DependencyObject) ui);
          if (parent != null)
            playSound = !await parent.TryPlayCompleteStory(model);
          TaskService.SetTaskStatus(model.Id, 2, true, list?.GetToastParent(), playSound: playSound);
        });
        instance.Closed += (EventHandler) ((s, e) => list?.SetDetailInOperation(false));
        list?.SetDetailInOperation(true);
        instance.Show();
      }
    }

    public void NavigateTaskClick() => this.GetTaskList()?.OnNavigateTask(this.Model);

    public void OnLineVisibleChanged(DisplayItemModel model, bool visible)
    {
      this.GetTaskList()?.OnLineVisibleChanged(model, visible);
    }

    public TaskListView GetTaskListView() => this.GetTaskList() as TaskListView;

    public void SetParentInOperation(bool inOperate)
    {
      this.GetTaskList()?.SetDetailInOperation(inOperate);
    }

    public void Reset(UIElement element, DisplayItemModel model)
    {
      this.Element = element;
      this.Model = model;
    }

    public void Clear()
    {
      this.Element = (UIElement) null;
      this.Model = (DisplayItemModel) null;
      this._iTaskList = (ITaskList) null;
      this._parentWindow = (IToastShowWindow) null;
    }

    public void TitleLostFocus(bool reload) => this.GetTaskList()?.OnLostFocus(reload);

    public void SetQuickSetPopup(Popup popup) => this.GetTaskListView()?.SetQuickSetPopup(popup);

    public void SetParsedDate(TimeData timeData) => this.Model?.OnTimeParsed(timeData);

    public async void SetDateAndTitle(string text, IPaserDueDate parsedData)
    {
      DisplayItemModel model = this.Model;
      if (model == null || this.GetTaskList() == null)
        return;
      TimeData timeData = parsedData?.ToTimeData(true);
      if (timeData != null)
        await TaskService.SetDateAndTitle(model.Id, text, timeData);
      else
        this.TitleTextChanged(text);
    }

    public TimeData GetDefaultDate()
    {
      return this.GetTaskList()?.GetDefaultTimeData() ?? TimeData.BuildDefaultStartAndEnd();
    }

    public bool InProjectList()
    {
      TaskListView taskListView = this.GetTaskListView();
      return taskListView != null && !taskListView.ViewModel.InKanban && !taskListView.ViewModel.InDetail;
    }

    public ProjectIdentity GetProjectIdentify()
    {
      return this.GetTaskListView()?.ViewModel.ProjectIdentity;
    }

    private async Task TryToastQuadrantChanged(string taskId)
    {
      ITaskList taskList = this.GetTaskList();
      int quadrantLevel = taskList != null ? taskList.QuadrantLevel : 0;
      if (quadrantLevel == 0)
        return;
      string e = (await MatrixManager.GetTaskQuadrantChangeString(quadrantLevel, taskId)).Item2;
      if (string.IsNullOrEmpty(e))
        return;
      this.GetParentWindow()?.TryToastString((object) null, e);
    }

    public async Task<string> GetCompleteStoryText(DisplayItemModel model)
    {
      if (LocalSettings.Settings.ExtraSettings.LastCompleteTime < ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Today))
      {
        LocalSettings.Settings.ExtraSettings.LastCompleteTime = ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Today);
        if (TaskCache.GetAllTask().All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
        {
          if (t.Status != 2)
            return true;
          DateTime? completedTime = t.CompletedTime;
          DateTime today = DateTime.Today;
          return completedTime.HasValue && completedTime.GetValueOrDefault() < today;
        })))
          return Utils.GetString("FirstTaskDone");
      }
      TaskListView taskListView = this.GetTaskListView();
      return taskListView != null ? await taskListView.GetCompleteText(model) : (string) null;
    }
  }
}
