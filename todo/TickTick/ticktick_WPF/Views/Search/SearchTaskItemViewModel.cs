// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchTaskItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchTaskItemViewModel : SearchItemBaseModel
  {
    private string _projectName;
    public bool TogglingStatus;
    private string _attachmentContent;

    public override bool CanSelect { get; set; } = true;

    public Geometry Icon
    {
      get
      {
        return ThemeUtil.GetTaskIconGeometry(this.SourceModel.Type, this.Status, this.SourceModel.Kind, SubscribeCalendarHelper.GetCalendarType(this.SourceModel.CalendarId));
      }
    }

    public double IconWidth => this.SourceModel.Type == DisplayType.Course ? 15.0 : 14.0;

    public SearchTaskItemViewModel(TaskBaseViewModel task)
    {
      this.SourceModel = task;
      this.Status = task.Status;
    }

    public TaskBaseViewModel SourceModel { get; set; }

    public string CommentStr { get; set; }

    public string Content { get; set; }

    public int Status { get; set; }

    public void InitData()
    {
      this.SetComment();
      this.SetAttachment();
      this.SetContent();
    }

    private void SetAttachment()
    {
      if (this.SourceModel == null)
        return;
      this._attachmentContent = TaskUtils.ReplaceAttachmentNameInString(this.SourceModel.Content);
    }

    private void SetComment()
    {
      string str = string.Empty;
      List<CommentModel> taskComments = TaskCommentCache.GetTaskComments(this.SourceModel.Id);
      // ISSUE: explicit non-virtual call
      if (taskComments != null && __nonvirtual (taskComments.Count) > 0)
      {
        foreach (CommentModel commentModel in (IEnumerable<CommentModel>) taskComments.OrderByDescending<CommentModel, DateTime?>((Func<CommentModel, DateTime?>) (c => c.createdTime)).ThenByDescending<CommentModel, DateTime?>((Func<CommentModel, DateTime?>) (c => c.modifiedTime)))
        {
          Match match = SearchHelper.PreSearchRegex.Match(commentModel.title?.ToLower() ?? string.Empty);
          if (commentModel.title != null && match.Success)
          {
            str = match.Index > 8 ? "..." + commentModel.title.Substring(match.Index - 8).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ") : commentModel.title;
            break;
          }
        }
      }
      this.CommentStr = str;
    }

    private void SetContent()
    {
      string str = (string) null;
      if (this.SourceModel.Kind == "CHECKLIST")
      {
        Match match1 = SearchHelper.PreSearchRegex.Match(this.SourceModel.Desc?.ToLower() ?? string.Empty);
        if (this.SourceModel.Desc != null && match1.Success)
        {
          str = match1.Index > 8 ? "..." + this.SourceModel.Desc.Substring(match1.Index - 8).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ") : this.SourceModel.Desc;
        }
        else
        {
          List<TaskBaseViewModel> checkItemsByTaskId = TaskDetailItemCache.GetCheckItemsByTaskId(this.SourceModel.Id);
          if (checkItemsByTaskId != null)
          {
            checkItemsByTaskId.Sort((Comparison<TaskBaseViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
            foreach (TaskBaseViewModel taskBaseViewModel in checkItemsByTaskId)
            {
              Match match2 = SearchHelper.PreSearchRegex.Match(taskBaseViewModel.Title?.ToLower() ?? string.Empty);
              if (taskBaseViewModel.Title != null && match2.Success)
              {
                str = "- " + (match2.Index > 8 ? "..." + taskBaseViewModel.Title.Substring(match2.Index - 8) : taskBaseViewModel.Title);
                break;
              }
            }
            if (string.IsNullOrEmpty(str))
            {
              if (!string.IsNullOrEmpty(this.SourceModel.Desc))
              {
                str = this.SourceModel.Desc;
              }
              else
              {
                TaskBaseViewModel taskBaseViewModel = checkItemsByTaskId.FirstOrDefault<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (i => i.Status == 0)) ?? checkItemsByTaskId.FirstOrDefault<TaskBaseViewModel>();
                str = string.IsNullOrEmpty(taskBaseViewModel?.Title) ? string.Empty : "- " + taskBaseViewModel.Title;
              }
            }
          }
        }
      }
      else if (!string.IsNullOrEmpty(this._attachmentContent))
      {
        string attachmentContent = this._attachmentContent;
        Match match = SearchHelper.PreSearchRegex.Match(attachmentContent.ToLower());
        str = !match.Success || match.Index <= 11 ? attachmentContent : "..." + attachmentContent.Substring(match.Index - 10).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
      }
      else
        str = string.Empty;
      this.Content = str;
    }

    public void ResetIcon() => this.OnPropertyChanged("Icon");
  }
}
