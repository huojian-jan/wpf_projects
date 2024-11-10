// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Activity.TaskActivityViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Activity
{
  public class TaskActivityViewModel : BaseViewModel
  {
    private bool _fold = true;

    public string Title { get; set; }

    public string Description { get; set; }

    public string User { get; set; }

    public string When { get; set; }

    public SolidColorBrush Color { get; set; }

    public Geometry Icon { get; set; }

    public int MaxDescLength { get; set; }

    public bool Fold
    {
      get => this._fold;
      set
      {
        this._fold = value;
        this.OnPropertyChanged(nameof (Fold));
      }
    }

    public TaskActivityViewModel(
      TaskModifyModel model,
      string userName,
      bool isAgendaCopy,
      bool taskFloating,
      string kind)
    {
      if (isAgendaCopy && model.action != "T_CREATE")
        userName = Utils.GetString("Owner");
      this.Title = TaskActivityViewModel.GetTitle(model, userName, isAgendaCopy, model.kind ?? kind);
      this.Description = TaskActivityViewModel.GetDescription(model, taskFloating, kind);
      this.User = userName;
      this.Color = TaskActivityViewModel.GetColor(model.action);
      this.Icon = TaskActivityViewModel.GetIcon(model.action, model.kind);
      DateTime result;
      if (DateTime.TryParse(model.when, out result))
        this.When = DateUtils.FormatCommentTime(result);
      if (model.whoProfile == null || model.whoProfile.isMyself)
        return;
      this.User = string.IsNullOrEmpty(model.whoProfile.name) ? model.whoProfile.username : model.whoProfile.name;
    }

    private static Geometry GetIcon(string action, string kind)
    {
      if (action != null)
      {
        switch (action.Length)
        {
          case 6:
            switch (action[2])
            {
              case 'D':
                switch (action)
                {
                  case "T_DONE":
                    return Utils.GetIcon("IcActionCompleted");
                  case "C_DONE":
                    return Utils.GetIcon("IcActionSubtaskDone");
                  default:
                    goto label_40;
                }
              case 'K':
                if (action == "T_KIND")
                  return !(kind == "TEXT") ? Utils.GetIcon("IcActionList") : Utils.GetIcon("IcActionText");
                goto label_40;
              case 'M':
                if (action == "T_MOVE")
                  return Utils.GetIcon("IcActionMoveIn");
                goto label_40;
              default:
                goto label_40;
            }
          case 8:
            switch (action[4])
            {
              case 'D':
                switch (action)
                {
                  case "T_UNDONE":
                    return Utils.GetIcon("IcActionUndone");
                  case "C_UNDONE":
                    return Utils.GetIcon("IcActionSubtaskUndone");
                  default:
                    goto label_40;
                }
              case 'E':
                switch (action)
                {
                  case "T_CREATE":
                    return Utils.GetIcon("IcActionCreate");
                  case "C_CREATE":
                    return Utils.GetIcon("IcActionSubtaskCreate");
                  default:
                    goto label_40;
                }
              case 'L':
                switch (action)
                {
                  case "T_DELETE":
                    return Utils.GetIcon("IcActionDelete");
                  case "C_DELETE":
                    return Utils.GetIcon("IcActionSubtaskDel");
                  default:
                    goto label_40;
                }
              case 'N':
                if (action == "T_CANCEL")
                  break;
                goto label_40;
              case 'O':
                if (action == "T_REOPEN")
                  return Utils.GetIcon("IcActionUndone");
                goto label_40;
              case 'S':
                if (action == "T_ASSIGN")
                  return Utils.GetIcon("IcActionAssign");
                goto label_40;
              default:
                goto label_40;
            }
            break;
          case 9:
            if (action == "C_CONTENT")
              return Utils.GetIcon("IcActionSubtaskEdit");
            goto label_40;
          case 10:
            switch (action[2])
            {
              case 'A':
                if (action == "T_ADD_FILE")
                  return Utils.GetIcon("IcActionAddFile");
                goto label_40;
              case 'D':
                if (action == "T_DEL_FILE")
                  return Utils.GetIcon("IcActionDelete");
                goto label_40;
              case 'R':
                if (action == "C_REMINDER")
                  return Utils.GetIcon("IcActionSubTime");
                goto label_40;
              default:
                goto label_40;
            }
          case 12:
            if (action == "T_TRASH_BACK")
              return Utils.GetIcon("IcActionUndone");
            goto label_40;
          case 15:
            if (action == "T_REPEAT_CANCEL")
              break;
            goto label_40;
          default:
            goto label_40;
        }
        return Utils.GetIcon("IcCalAbandonedIndicator");
      }
label_40:
      return Utils.GetIcon("IcActionEdit");
    }

    private static SolidColorBrush GetColor(string action)
    {
      if (action != null)
      {
        switch (action.Length)
        {
          case 6:
            switch (action[2])
            {
              case 'D':
                if (action == "T_DONE" || action == "C_DONE")
                  return ThemeUtil.GetColor("CompleteTaskColor");
                goto label_22;
              case 'K':
                if (action == "T_KIND")
                  break;
                goto label_22;
              case 'M':
                if (action == "T_MOVE")
                  break;
                goto label_22;
              default:
                goto label_22;
            }
            break;
          case 8:
            switch (action[4])
            {
              case 'D':
                if (action == "T_UNDONE" || action == "C_UNDONE")
                  goto label_19;
                else
                  goto label_22;
              case 'E':
                if (action == "T_CREATE" || action == "C_CREATE")
                  break;
                goto label_22;
              case 'L':
                if (action == "T_DELETE" || action == "C_DELETE")
                  goto label_20;
                else
                  goto label_22;
              case 'N':
                if (action == "T_CANCEL")
                  goto label_21;
                else
                  goto label_22;
              case 'O':
                if (action == "T_REOPEN")
                  goto label_19;
                else
                  goto label_22;
              default:
                goto label_22;
            }
            break;
          case 10:
            switch (action[2])
            {
              case 'A':
                if (action == "T_ADD_FILE")
                  break;
                goto label_22;
              case 'D':
                if (action == "T_DEL_FILE")
                  goto label_20;
                else
                  goto label_22;
              default:
                goto label_22;
            }
            break;
          case 12:
            if (action == "T_TRASH_BACK")
              goto label_19;
            else
              goto label_22;
          case 15:
            if (action == "T_REPEAT_CANCEL")
              goto label_21;
            else
              goto label_22;
          default:
            goto label_22;
        }
        return ThemeUtil.GetColor("CreateTaskColor");
label_19:
        return ThemeUtil.GetColor("UndoTaskColor");
label_20:
        return ThemeUtil.GetColor("TextRedColor");
label_21:
        return ThemeUtil.GetColor("AbandonedColor");
      }
label_22:
      return ThemeUtil.GetColor("EditTaskColor");
    }

    private static string GetDescription(TaskModifyModel model, bool taskFloating, string kind)
    {
      string action = model.action;
      if (action != null)
      {
        switch (action.Length)
        {
          case 5:
            if (action == "T_DUE")
            {
              DateTime result1;
              DateTime.TryParse(model.startDate, out result1);
              DateTime result2;
              DateTime.TryParse(model.startDateBefore, out result2);
              DateTime result3;
              DateTime.TryParse(model.dueDate != model.startDate ? model.dueDate : string.Empty, out result3);
              DateTime result4;
              DateTime.TryParse(model.dueDateBefore != model.startDateBefore ? model.dueDateBefore : string.Empty, out result4);
              return TaskActivityViewModel.GetTimeDescription(result2, result1, result4, result3, model);
            }
            goto label_42;
          case 6:
            switch (action[0])
            {
              case 'C':
                if (action == "C_DONE")
                  goto label_31;
                else
                  goto label_42;
              case 'T':
                if (action == "T_MOVE")
                {
                  string format = Utils.GetString(kind != "NOTE" ? "ActionMoveTaskMessage" : "ActionMoveNoteMessage");
                  string str1 = CacheManager.GetProjectById(model.fromProjectId)?.name ?? Utils.GetString("UnknownProject");
                  string str2 = CacheManager.GetProjectById(model.toProjectId)?.name ?? Utils.GetString("UnknownProject");
                  string str3 = str1;
                  string str4 = str2;
                  return string.Format(format, (object) str3, (object) str4);
                }
                goto label_42;
              default:
                goto label_42;
            }
          case 7:
            if (action == "T_TITLE")
              return model.title;
            goto label_42;
          case 8:
            switch (action[2])
            {
              case 'C':
                switch (action)
                {
                  case "T_CREATE":
                    return model.title;
                  case "C_CREATE":
                    goto label_31;
                  default:
                    goto label_42;
                }
              case 'D':
                if (action == "C_DELETE")
                  goto label_31;
                else
                  goto label_42;
              case 'P':
                if (action == "T_PARENT")
                  return Utils.GetString("SetParentActivity") + ":" + model.parent;
                goto label_42;
              case 'R':
                if (action == "T_REPEAT")
                  break;
                goto label_42;
              case 'U':
                if (action == "C_UNDONE")
                  goto label_31;
                else
                  goto label_42;
              default:
                goto label_42;
            }
            break;
          case 9:
            switch (action[0])
            {
              case 'C':
                if (action == "C_CONTENT")
                {
                  string description = string.IsNullOrEmpty(model.desc) ? "" : model.desc + "\r\n";
                  List<string> itemTitles = model.itemTitles;
                  // ISSUE: explicit non-virtual call
                  if ((itemTitles != null ? (__nonvirtual (itemTitles.Count) > 0 ? 1 : 0) : 0) != 0)
                  {
                    for (int index = 0; index < model.itemTitles.Count; ++index)
                      description = description + "-" + model.itemTitles[index] + (index == model.itemTitles.Count - 1 ? "" : "\r\n");
                  }
                  return description;
                }
                goto label_42;
              case 'T':
                if (action == "T_CONTENT")
                  return model.content;
                goto label_42;
              default:
                goto label_42;
            }
          case 10:
            switch (action[2])
            {
              case 'A':
                if (action == "T_ADD_FILE")
                  break;
                goto label_42;
              case 'D':
                if (action == "T_DEL_FILE")
                  break;
                goto label_42;
              case 'R':
                if (action == "C_REMINDER")
                {
                  DateTime result5;
                  DateTime.TryParse(model.itemReminderDate, out result5);
                  DateTime result6;
                  DateTime.TryParse(model.itemReminderDateBefore, out result6);
                  string str5 = Utils.IsEmptyDate(result6) ? Utils.GetString("NoReminder") : DateUtils.GetTaskActivityTimeString(result6, new DateTime?(), new bool?(model.isAllDayBefore), model.itemTimeZone, taskFloating);
                  string str6 = Utils.IsEmptyDate(result5) ? Utils.GetString("NoReminder") : DateUtils.GetTaskActivityTimeString(result5, new DateTime?(), new bool?(model.isAllDay), model.itemTimeZone, taskFloating);
                  return str5 != str6 ? str5 + " → " + str6 + "\r\n" + model.description : string.Empty;
                }
                goto label_42;
              default:
                goto label_42;
            }
            return model.attachments != null && model.attachments.Any<ModifyAttachmentModel>() ? model.attachments[0].fileName : string.Empty;
          case 15:
            if (action == "T_REPEAT_CANCEL")
              break;
            goto label_42;
          case 16:
            if (action == "T_COMPLETED_TIME")
            {
              DateTime result;
              DateTime.TryParse(model.completedTime, out result);
              string str = DateUtils.FormatShortDate(result);
              return string.Format(Utils.GetString("ActionChangeCpDateDesc"), (object) str);
            }
            goto label_42;
          default:
            goto label_42;
        }
        DateTime result7;
        DateTime.TryParse(model.startDate, out result7);
        DateTime result8;
        DateTime.TryParse(model.startDateBefore, out result8);
        DateTime result9;
        DateTime.TryParse(model.dueDate != model.startDate ? model.dueDate : string.Empty, out result9);
        DateTime result10;
        DateTime.TryParse(model.dueDateBefore != model.startDateBefore ? model.dueDateBefore : string.Empty, out result10);
        return TaskActivityViewModel.GetRepeatDescription(result8, result7, result10, result9, model);
label_31:
        return model.description;
      }
label_42:
      return string.Empty;
    }

    private static string GetRepeatDescription(
      DateTime startBefore,
      DateTime start,
      DateTime dueBefore,
      DateTime due,
      TaskModifyModel model)
    {
      string activityTimeString1 = DateUtils.GetTaskActivityTimeString(startBefore, new DateTime?(dueBefore), new bool?(model.isAllDayBefore), model.timeZoneBefore, model.isFloatingBefore);
      string activityTimeString2 = DateUtils.GetTaskActivityTimeString(start, new DateTime?(due), new bool?(model.isAllDay), model.timeZone, model.isFloating);
      return string.Format(Utils.GetString("RepeatCycleTo"), (object) activityTimeString1, (object) activityTimeString2);
    }

    private static string GetTimeDescription(
      DateTime startBefore,
      DateTime start,
      DateTime dueBefore,
      DateTime due,
      TaskModifyModel model)
    {
      if (Utils.IsEmptyDate(start))
        return Utils.GetString("ActionClearDate");
      if (Utils.IsEmptyDate(startBefore))
      {
        string activityTimeString = DateUtils.GetTaskActivityTimeString(start, new DateTime?(due), new bool?(model.isAllDay), model.timeZone, model.isFloating);
        return string.Format(Utils.GetString("ActionModifyDate"), (object) string.Empty, (object) activityTimeString);
      }
      string activityTimeString1 = DateUtils.GetTaskActivityTimeString(startBefore, new DateTime?(dueBefore), new bool?(model.isAllDayBefore), model.timeZoneBefore, model.isFloatingBefore);
      string activityTimeString2 = DateUtils.GetTaskActivityTimeString(start, new DateTime?(due), new bool?(model.isAllDay), model.timeZone, model.isFloating);
      return string.Format(Utils.GetString("ActionModifyDate"), (object) activityTimeString1, (object) activityTimeString2);
    }

    private static string GetTitle(
      TaskModifyModel model,
      string username,
      bool isAgendaCopy,
      string kind)
    {
      string action = model.action;
      if (action != null)
      {
        switch (action.Length)
        {
          case 5:
            if (action == "T_DUE")
              return Utils.GetString("ActionSetDate");
            break;
          case 6:
            switch (action[2])
            {
              case 'D':
                switch (action)
                {
                  case "T_DONE":
                    return Utils.GetString("ActionCompleteTask");
                  case "C_DONE":
                    return Utils.GetString("ActionDoneItem");
                }
                break;
              case 'K':
                if (action == "T_KIND")
                  return !(model.kind == "TEXT") ? Utils.GetString("ActionSwitchToChecklist") : Utils.GetString("ActionSwitchToText");
                break;
              case 'M':
                if (action == "T_MOVE")
                  return Utils.GetString(kind != "NOTE" ? "ActionMoveTask" : "ActionMoveNote");
                break;
            }
            break;
          case 7:
            if (action == "T_TITLE")
              return Utils.GetString("ActionModifyTitle");
            break;
          case 8:
            switch (action[4])
            {
              case 'D':
                switch (action)
                {
                  case "T_UNDONE":
                    return Utils.GetString("ActionUnCompleteTask");
                  case "C_UNDONE":
                    return Utils.GetString("ActionUndoneItem");
                }
                break;
              case 'E':
                switch (action)
                {
                  case "T_CREATE":
                    return Utils.GetString(isAgendaCopy ? "ActionSaveAgenda" : (kind == "NOTE" ? "ActionCreateNote" : "ActionCreateTask"));
                  case "C_CREATE":
                    return Utils.GetString("ActionCreateItem");
                }
                break;
              case 'L':
                switch (action)
                {
                  case "T_DELETE":
                    return Utils.GetString(kind == "NOTE" ? "ActionDeleteNote" : "ActionDeleteTask");
                  case "C_DELETE":
                    return Utils.GetString("ActionDeleteItem");
                }
                break;
              case 'N':
                if (action == "T_CANCEL")
                  return Utils.GetString("ActionAbandonedTask");
                break;
              case 'O':
                if (action == "T_REOPEN")
                  return Utils.GetString("ActionReopenTask");
                break;
              case 'P':
                if (action == "T_REPEAT")
                  return Utils.GetString("ActionRepeat");
                break;
              case 'R':
                if (action == "T_PARENT")
                  return Utils.GetString("SetParentActivity");
                break;
              case 'S':
                if (action == "T_ASSIGN")
                {
                  if (model.assigneeProfile == null)
                    return Utils.GetString("ActionCancelAssign");
                  string str = !model.assigneeProfile.isMyself ? (string.IsNullOrEmpty(model.assigneeProfile.name) ? model.assigneeProfile.username : model.assigneeProfile.name) : username;
                  return string.Format(Utils.GetString(kind == "NOTE" ? "ActionAssignNote" : "ActionAssignTask"), (object) str);
                }
                break;
            }
            break;
          case 9:
            switch (action[6])
            {
              case 'A':
                if (action == "T_AS_TASK")
                  return Utils.GetString("ActionConvertToTask");
                break;
              case 'E':
                switch (action)
                {
                  case "T_CONTENT":
                    return Utils.GetString("ActionModifyContent");
                  case "C_CONTENT":
                    return Utils.GetString("ActionModifyDesc");
                }
                break;
              case 'O':
                if (action == "T_AS_NOTE")
                  return Utils.GetString("ActionConvertToNote");
                break;
            }
            break;
          case 10:
            switch (action[2])
            {
              case 'A':
                if (action == "T_ADD_FILE")
                  return Utils.GetString("ActionAddFile");
                break;
              case 'D':
                if (action == "T_DEL_FILE")
                  return Utils.GetString("ActionDeleteFile");
                break;
              case 'P':
                if (action == "T_PROGRESS")
                {
                  string str = model.progress + "%";
                  return string.Format(Utils.GetString("ActionSetProgress"), (object) str);
                }
                break;
              case 'R':
                if (action == "C_REMINDER")
                  return Utils.GetString("ActionModifyItemDue");
                break;
            }
            break;
          case 12:
            if (action == "T_TRASH_BACK")
              return Utils.GetString(kind == "NOTE" ? "ActionRestoreNote" : "ActionRestoreTask");
            break;
          case 15:
            switch (action[2])
            {
              case 'D':
                if (action == "T_DELETE_PARENT")
                  return Utils.GetString("DelParentActivity");
                break;
              case 'R':
                if (action == "T_REPEAT_CANCEL")
                  return Utils.GetString("ActionRepeatAbandoned");
                break;
            }
            break;
          case 16:
            if (action == "T_COMPLETED_TIME")
              return Utils.GetString("ActionChangeCpDate");
            break;
        }
      }
      return string.Empty;
    }
  }
}
