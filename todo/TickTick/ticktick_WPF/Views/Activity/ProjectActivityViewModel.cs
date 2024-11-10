// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Activity.ProjectActivityViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Activity
{
  public class ProjectActivityViewModel
  {
    public string Title { get; set; }

    public string User { get; set; }

    public string When { get; set; }

    public SolidColorBrush Color { get; set; }

    public Geometry Icon { get; set; }

    public string TaskId { get; set; }

    public string TaskTitle { get; set; }

    public bool IsSection { get; set; }

    public DateTime Date { get; set; }

    public ProjectActivityViewModel(string date)
    {
      this.IsSection = true;
      this.Title = date;
    }

    public ProjectActivityViewModel(
      ProjectModifyModel model,
      string userName,
      IReadOnlyDictionary<string, TaskDao.TitleModel> titles)
    {
      this.Color = ProjectActivityViewModel.GetColor(model.action);
      this.Icon = ProjectActivityViewModel.GetIcon(model.action);
      bool? isNote = new bool?();
      if (model.taskIds != null && model.taskIds.Any<string>())
      {
        this.TaskId = model.taskIds[0];
        TaskDao.TitleModel titleModel;
        bool flag = titles.TryGetValue(this.TaskId, out titleModel);
        this.TaskTitle = flag ? titleModel.Title : Utils.GetString("Unknown");
        isNote = flag ? new bool?(titleModel.Kind == "NOTE") : new bool?();
      }
      this.Title = ProjectActivityViewModel.GetTitle(model, isNote);
      DateTime result;
      if (DateTime.TryParse(model.when, out result))
      {
        this.When = DateUtils.FormatCommentTime(result);
        this.Date = result;
      }
      this.User = userName;
      if (model.whoProfile == null || model.whoProfile.isMyself)
        return;
      this.User = string.IsNullOrEmpty(model.whoProfile.name) ? model.whoProfile.username : model.whoProfile.name;
    }

    private static Geometry GetIcon(string action)
    {
      if (action != null)
      {
        switch (action.Length)
        {
          case 4:
            if (action == "P_IN")
              return Utils.GetIcon("IcActionMoveIn");
            goto label_30;
          case 5:
            if (action == "P_OUT")
              return Utils.GetIcon("IcActionMoveOut");
            goto label_30;
          case 6:
            if (action == "T_DONE")
              return Utils.GetIcon("IcActionCompleted");
            goto label_30;
          case 8:
            switch (action[4])
            {
              case 'D':
                if (action == "T_UNDONE")
                  return Utils.GetIcon("IcActionUndone");
                goto label_30;
              case 'E':
                switch (action)
                {
                  case "P_CREATE":
                    return Utils.GetIcon("IcActionCreate");
                  case "T_CREATE":
                    return Utils.GetIcon("IcActionCreate");
                  default:
                    goto label_30;
                }
              case 'L':
                if (action == "T_DELETE")
                  return Utils.GetIcon("IcActionDelete");
                goto label_30;
              case 'N':
                if (action == "T_CANCEL")
                  break;
                goto label_30;
              case 'O':
                if (action == "T_REOPEN")
                  return Utils.GetIcon("IcActionUndone");
                goto label_30;
              case 'S':
                if (action == "T_ASSIGN")
                  return Utils.GetIcon("IcActionAssign");
                goto label_30;
              default:
                goto label_30;
            }
            break;
          case 12:
            switch (action[0])
            {
              case 'P':
                if (action == "P_SHARE_JOIN")
                  return Utils.GetIcon("IcActionJoin");
                goto label_30;
              case 'T':
                if (action == "T_TRASH_BACK")
                  return Utils.GetIcon("IcActionUndone");
                goto label_30;
              default:
                goto label_30;
            }
          case 14:
            if (action == "P_SHARE_DELETE")
              return Utils.GetIcon("IcActionQuit");
            goto label_30;
          case 15:
            if (action == "T_REPEAT_CANCEL")
              break;
            goto label_30;
          default:
            goto label_30;
        }
        return Utils.GetIcon("IcCalAbandonedIndicator");
      }
label_30:
      return Utils.GetIcon("IcActionEdit");
    }

    private static SolidColorBrush GetColor(string action)
    {
      if (action != null)
      {
        switch (action.Length)
        {
          case 4:
            if (action == "P_IN")
              break;
            goto label_21;
          case 5:
            if (action == "P_OUT")
              goto label_17;
            else
              goto label_21;
          case 6:
            if (action == "T_DONE")
              return ThemeUtil.GetColor("CompleteTaskColor");
            goto label_21;
          case 8:
            switch (action[4])
            {
              case 'D':
                if (action == "T_UNDONE")
                  goto label_19;
                else
                  goto label_21;
              case 'E':
                if (action == "T_CREATE" || action == "P_CREATE")
                  break;
                goto label_21;
              case 'L':
                if (action == "T_DELETE")
                  goto label_17;
                else
                  goto label_21;
              case 'N':
                if (action == "T_CANCEL")
                  goto label_20;
                else
                  goto label_21;
              case 'O':
                if (action == "T_REOPEN")
                  goto label_19;
                else
                  goto label_21;
              default:
                goto label_21;
            }
            break;
          case 12:
            switch (action[0])
            {
              case 'P':
                if (action == "P_SHARE_JOIN")
                  break;
                goto label_21;
              case 'T':
                if (action == "T_TRASH_BACK")
                  goto label_19;
                else
                  goto label_21;
              default:
                goto label_21;
            }
            break;
          case 14:
            if (action == "P_SHARE_DELETE")
              goto label_17;
            else
              goto label_21;
          case 15:
            if (action == "T_REPEAT_CANCEL")
              goto label_20;
            else
              goto label_21;
          default:
            goto label_21;
        }
        return ThemeUtil.GetColor("CreateTaskColor");
label_17:
        return ThemeUtil.GetColor("TextRedColor");
label_19:
        return ThemeUtil.GetColor("UndoTaskColor");
label_20:
        return ThemeUtil.GetColor("AbandonedColor");
      }
label_21:
      return ThemeUtil.GetColor("EditTaskColor");
    }

    private static string GetTitle(ProjectModifyModel model, bool? isNote)
    {
      string str = "";
      string action = model.action;
      if (action != null)
      {
        switch (action.Length)
        {
          case 4:
            if (action == "P_IN")
              return string.Format(Utils.GetString("ActionMoveIn"), !isNote.HasValue ? (object) "" : (object) Utils.GetString(isNote.Value ? "Notes" : "Task"));
            break;
          case 5:
            switch (action[0])
            {
              case 'P':
                if (action == "P_OUT")
                  return string.Format(Utils.GetString("ActionMoveOut"), !isNote.HasValue ? (object) "" : (object) Utils.GetString(isNote.Value ? "Notes" : "Task"));
                break;
              case 'T':
                if (action == "T_DUE")
                  return Utils.GetString("ActionSetDate");
                break;
            }
            break;
          case 6:
            if (action == "T_DONE")
              return Utils.GetString("ActionCompleteTask");
            break;
          case 7:
            switch (action[0])
            {
              case 'P':
                if (action == "P_TITLE")
                  return Utils.GetString("ActionListTitle");
                break;
              case 'T':
                if (action == "T_TITLE")
                  return Utils.GetString("ActionModifyTitle");
                break;
            }
            break;
          case 8:
            switch (action[4])
            {
              case 'D':
                if (action == "T_UNDONE")
                  return Utils.GetString("ActionUnCompleteTask");
                break;
              case 'E':
                switch (action)
                {
                  case "P_CREATE":
                    return Utils.GetString("ActionCreateProject");
                  case "T_CREATE":
                    return string.Format(Utils.GetString("ActionCreate"), !isNote.HasValue ? (object) "" : (object) Utils.GetString(isNote.Value ? "Notes" : "Task"));
                }
                break;
              case 'L':
                switch (action)
                {
                  case "P_DELETE":
                    return Utils.GetString("ActionDeleteProject");
                  case "T_DELETE":
                    return string.Format(Utils.GetString("ActionDelete"), !isNote.HasValue ? (object) "" : (object) Utils.GetString(isNote.Value ? "Notes" : "Task"));
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
                  return Utils.GetString("ActionSetDate");
                break;
              case 'S':
                if (action == "T_ASSIGN")
                  return Utils.GetString("ActionAssignTask");
                break;
            }
            break;
          case 9:
            if (action == "T_CONTENT")
              return Utils.GetString("ActionModifyContent");
            break;
          case 10:
            if (action == "T_PROGRESS")
              return string.Format(Utils.GetString("ActionSetProgress"), (object) str);
            break;
          case 12:
            switch (action[0])
            {
              case 'P':
                if (action == "P_SHARE_JOIN")
                  return Utils.GetString("ActionJoinIn");
                break;
              case 'T':
                if (action == "T_TRASH_BACK")
                  return string.Format(Utils.GetString("ActionRestore"), !isNote.HasValue ? (object) "" : (object) Utils.GetString(isNote.Value ? "Notes" : "Task"));
                break;
            }
            break;
          case 14:
            if (action == "P_SHARE_DELETE")
              return Utils.GetString("ActionQuit");
            break;
          case 15:
            if (action == "T_REPEAT_CANCEL")
              return Utils.GetString("ActionRepeatAbandoned");
            break;
        }
      }
      return string.Empty;
    }
  }
}
