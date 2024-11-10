// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TaskUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Undo;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class TaskUtils
  {
    private static readonly Regex TaskUrlWithTitleReg = new Regex("^" + BaseUrl.GetDomainUrl() + "/webapp#p/(.*)/tasks/(\\S+) (.*)");
    private static readonly Regex TaskUrlWithTitleRegExtra = new Regex("^" + BaseUrl.GetDomainUrl() + "/webapp/#p/(.*)/tasks/(\\S+) (.*)");
    private static readonly Regex TaskUrlReg = new Regex("^" + BaseUrl.GetDomainUrl() + "/webapp#p/(.*)/tasks/(.*)");
    private static readonly Regex TaskUrlRegExtra = new Regex("^" + BaseUrl.GetDomainUrl() + "/webapp/#p/(.*)/tasks/(.*)");
    public const string ValidUrl = "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]?\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]\\.[^\\s]{2,})";
    private static readonly Regex LinkRegex = new Regex("(\\[.*\\])(\\(.*\\))");
    public static readonly Regex UrlRegex = new Regex("(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]?\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]\\.[^\\s]{2,})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static ProjectTask ParseTaskUrlWithoutTitle(string url)
    {
      System.Text.RegularExpressions.Match match1 = TaskUtils.TaskUrlReg.Match(url);
      if (match1.Success)
        return new ProjectTask()
        {
          Title = match1.Groups[3].ToString(),
          TaskId = match1.Groups[2].ToString(),
          ProjectId = match1.Groups[1].ToString()
        };
      System.Text.RegularExpressions.Match match2 = TaskUtils.TaskUrlRegExtra.Match(url);
      if (!match2.Success)
        return (ProjectTask) null;
      return new ProjectTask()
      {
        Title = match2.Groups[3].ToString(),
        TaskId = match2.Groups[2].ToString(),
        ProjectId = match2.Groups[1].ToString()
      };
    }

    public static ProjectTask ParseTaskUrl(string url)
    {
      System.Text.RegularExpressions.Match match1 = TaskUtils.TaskUrlWithTitleReg.Match(url);
      if (match1.Success)
        return new ProjectTask()
        {
          Title = match1.Groups[3].ToString(),
          TaskId = match1.Groups[2].ToString(),
          ProjectId = match1.Groups[1].ToString()
        };
      System.Text.RegularExpressions.Match match2 = TaskUtils.TaskUrlWithTitleRegExtra.Match(url);
      if (!match2.Success)
        return (ProjectTask) null;
      return new ProjectTask()
      {
        Title = match2.Groups[3].ToString(),
        TaskId = match2.Groups[2].ToString(),
        ProjectId = match2.Groups[1].ToString()
      };
    }

    public static void CopyTaskLink(string taskId, string projectId, string title = "")
    {
      try
      {
        Clipboard.SetText(TaskUtils.GetCopyLink(projectId, taskId, title));
        TaskUtils.Toast("Copied");
      }
      catch (Exception ex)
      {
      }
    }

    public static string GetCopyLink(
      string projectId,
      string taskId,
      string title = "",
      bool isMarkdownStyle = false)
    {
      if (projectId.Contains("inbox"))
        projectId = "inbox";
      if (string.IsNullOrEmpty(title))
        title = Utils.GetString("MyTask");
      return isMarkdownStyle ? "[" + title + "](" + BaseUrl.GetDomainUrl() + "/webapp/#p/" + projectId + "/tasks/" + taskId + ")" : BaseUrl.GetDomainUrl() + "/webapp/#p/" + projectId + "/tasks/" + taskId;
    }

    public static async Task<TaskModel> TryLoadTask(
      string taskId,
      string projectId,
      bool checkDeleted = false)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById != null)
      {
        if (!checkDeleted || thinTaskById.deleted == 0)
          return thinTaskById;
        TaskUtils.Toast("CannotFindLastTask");
        return (TaskModel) null;
      }
      if (Utils.IsNetworkAvailable())
      {
        Mouse.OverrideCursor = Cursors.Wait;
        if (projectId.Contains("inboxId"))
          projectId = Utils.GetInboxId();
        TaskModel remoteTask = await Communicator.GetTask(taskId, projectId);
        Mouse.OverrideCursor = (Cursor) null;
        if (remoteTask != null && !string.IsNullOrEmpty(remoteTask.id))
        {
          await TaskDao.BatchCreateTaskFromRemote(new List<TaskModel>()
          {
            remoteTask
          }, true);
          if (!checkDeleted || remoteTask.deleted == 0)
            return remoteTask;
          TaskUtils.Toast("CannotFindLastTask");
          return (TaskModel) null;
        }
        TaskUtils.Toast("NoTaskFound");
        return (TaskModel) null;
      }
      TaskUtils.Toast("NoNetwork");
      return (TaskModel) null;
    }

    public static string EscapeMarkDownUrlContent(string content)
    {
      if (string.IsNullOrEmpty(content))
        return string.Empty;
      string str = content;
      MatchCollection matchCollection = TaskUtils.LinkRegex.Matches(content);
      if (matchCollection.Count <= 0)
        return content;
      foreach (object obj in matchCollection)
      {
        if (obj is System.Text.RegularExpressions.Match match)
        {
          string oldValue = match.Groups[2].ToString();
          str = str.Replace(oldValue, string.Empty);
        }
      }
      return str;
    }

    private static void Toast(string key)
    {
      string str = Utils.GetString(key);
      Utils.Toast(str);
      App.Window?.TryToastString((object) null, str);
    }

    public static string EscapeLinkContent(string content)
    {
      try
      {
        return !string.IsNullOrEmpty(content) ? Regex.Replace(content, "(?<=\\[.*\\])\\([^)]*\\)", "") : content;
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
        return content;
      }
    }

    public static string GetLinkTitle(string url)
    {
      HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
      httpWebRequest.CookieContainer = new CookieContainer();
      Stream stream = httpWebRequest.GetResponse() is HttpWebResponse response1 ? response1.GetResponseStream() : (Stream) null;
      if (stream == null)
        return (string) null;
      if (response1.ContentEncoding.ToLower().Contains("gzip"))
        stream = (Stream) new GZipStream(stream, CompressionMode.Decompress);
      else if (response1.ContentEncoding.ToLower().Contains("deflate"))
        stream = (Stream) new DeflateStream(stream, CompressionMode.Decompress);
      StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
      string end = streamReader.ReadToEnd();
      streamReader.Close();
      stream.Close();
      response1.Close();
      string pattern1 = "charset=([\"']?)([a-zA-z0-9\\-_]+)(\\1)[^>]*?>";
      System.Text.RegularExpressions.Match match = Regex.Match(end, pattern1, RegexOptions.IgnoreCase);
      if (match.Success)
      {
        Group group = match.Groups[2];
        if (group.Value.ToLower() != "utf-8")
        {
          Stream responseStream = WebRequest.Create(url.ToLower()).GetResponse() is HttpWebResponse response2 ? response2.GetResponseStream() : (Stream) null;
          if (responseStream != null)
            end = new StreamReader(responseStream, Encoding.GetEncoding(group.Value)).ReadToEnd();
        }
      }
      string pattern2 = "\\<title\\b[^>]*\\>\\s*(?<Title>[\\s\\S]*?)\\</title\\>";
      if (url.StartsWith("https://mp.weixin.qq.com"))
        pattern2 = "property=\"og:title\" content=\"(?<Title>.*?)\"";
      return WebUtility.HtmlDecode(Regex.Match(end, pattern2, RegexOptions.IgnoreCase).Groups["Title"].Value.Replace("\r\n", " ").Replace("\r", " ").Trim());
    }

    public static void TryGetTaskLinkTitle(TaskModel task)
    {
      if (((int) LocalSettings.Settings.UserPreference?.GeneralConfig?.urlParseEnabled ?? 1) == 0)
        return;
      System.Text.RegularExpressions.Match urlMatch = TaskUtils.UrlRegex.Match(task.title);
      if (!urlMatch.Success)
        return;
      System.Text.RegularExpressions.Match match = TaskUtils.LinkRegex.Match(task.title);
      if (match.Success && match.Index <= urlMatch.Index)
        return;
      new Thread(new ThreadStart(Function)).Start();

      void Function()
      {
        try
        {
          string title = TaskUtils.GetLinkTitle(urlMatch.Value);
          if (string.IsNullOrEmpty(title))
            return;
          Application.Current?.Dispatcher?.InvokeAsync((Action) (() => TaskService.ReplaceTitleLink(task.id, urlMatch.Value, title)));
        }
        catch (Exception ex)
        {
        }
      }
    }

    public static string ReplaceAttachmentTextInString(string text, bool withTypeName = true)
    {
      if (string.IsNullOrEmpty(text))
        return text;
      MatchCollection matchCollection = MarkDownEditor.AttachmentRegex.Matches(text.ToLower());
      if (matchCollection.Count > 0)
      {
        Dictionary<int, System.Text.RegularExpressions.Match> dictionary = new Dictionary<int, System.Text.RegularExpressions.Match>();
        for (int i = 0; i < matchCollection.Count; ++i)
        {
          System.Text.RegularExpressions.Match match = matchCollection[i];
          dictionary[match.Index] = match;
        }
        foreach (int num in (IEnumerable<int>) dictionary.Keys.ToList<int>().OrderByDescending<int, int>((Func<int, int>) (k => k)))
        {
          System.Text.RegularExpressions.Match match = dictionary[num];
          string[] strArray = match.Groups[2].Value.Split('/');
          if (num + match.Value.Length <= text.Length)
          {
            text = text.Remove(num, match.Value.Length);
            if (withTypeName)
            {
              string str = strArray.Length == 2 ? AttachmentProvider.GetFileTypeName(strArray[1]) : Utils.GetString("Image").ToLower();
              text = text.Insert(num, "[" + str + "]");
            }
          }
        }
      }
      return text;
    }

    public static string ReplaceAttachmentNameInString(string text)
    {
      if (string.IsNullOrEmpty(text))
        return text;
      MatchCollection matchCollection = MarkDownEditor.AttachmentRegex.Matches(text.ToLower());
      if (matchCollection.Count > 0)
      {
        Dictionary<int, System.Text.RegularExpressions.Match> dictionary = new Dictionary<int, System.Text.RegularExpressions.Match>();
        for (int i = 0; i < matchCollection.Count; ++i)
        {
          System.Text.RegularExpressions.Match match = matchCollection[i];
          dictionary[match.Index] = match;
        }
        foreach (int num in (IEnumerable<int>) dictionary.Keys.ToList<int>().OrderByDescending<int, int>((Func<int, int>) (k => k)))
        {
          System.Text.RegularExpressions.Match match = dictionary[num];
          string[] strArray = match.Groups[2].Value.Split('/');
          if (num + match.Value.Length <= text.Length)
          {
            text = text.Remove(num, match.Value.Length);
            string str;
            if (strArray.Length == 2)
            {
              str = AttachmentCache.GetFileName(strArray[0]);
              if (string.IsNullOrEmpty(str))
                str = strArray[1];
            }
            else
              str = Utils.GetString("Image").ToLower();
            text = text.Insert(num, "[" + str + "]");
          }
        }
      }
      return text;
    }

    public static bool CheckDate(
      TaskBaseViewModel task,
      List<FilterDatePair> startEndPairs,
      bool inTodayOrWeek = false,
      LogicType type = LogicType.Or,
      bool inCal = false,
      bool showOutDateNote = false)
    {
      // ISSUE: explicit non-virtual call
      if (startEndPairs == null || __nonvirtual (startEndPairs.Count) <= 0)
        return true;
      bool flag = false;
      DateTime? startDate = task.StartDate;
      DateTime? nullable1 = task.DueDate;
      if (startDate.HasValue && nullable1.HasValue && task.IsAllDay.GetValueOrDefault() && startDate.Value == nullable1.Value)
        nullable1 = new DateTime?(startDate.Value.Date.AddDays(1.0));
      if (task.Kind == "NOTE" && !showOutDateNote && (!nullable1.HasValue && startDate.HasValue && startDate.Value < DateTime.Today || nullable1.HasValue && nullable1.Value <= DateTime.Today))
        return false;
      foreach (FilterDatePair startEndPair in startEndPairs)
      {
        DateTime? start = startEndPair.Start;
        DateTime? end = startEndPair.End;
        if (!start.HasValue && !end.HasValue)
        {
          if (startEndPair.isRepeat)
          {
            flag = flag || !string.IsNullOrEmpty(task.RepeatFlag);
            continue;
          }
          if (startEndPair.IsNoDate != Utils.IsEmptyDate(startDate))
            continue;
        }
        DateTime? nullable2;
        DateTime? nullable3;
        if (!start.HasValue && end.HasValue)
        {
          if (startDate.HasValue)
          {
            if (!nullable1.HasValue)
            {
              nullable2 = startDate;
              nullable3 = end;
              if ((nullable2.HasValue & nullable3.HasValue ? (nullable2.GetValueOrDefault() >= nullable3.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                continue;
            }
            nullable3 = nullable1;
            nullable2 = end;
            if ((nullable3.HasValue & nullable2.HasValue ? (nullable3.GetValueOrDefault() > nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
              continue;
          }
          else
            continue;
        }
        if (start.HasValue && !end.HasValue)
        {
          if (startDate.HasValue)
          {
            if (!nullable1.HasValue)
            {
              nullable2 = startDate;
              nullable3 = start;
              if ((nullable2.HasValue & nullable3.HasValue ? (nullable2.GetValueOrDefault() < nullable3.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                continue;
            }
            if (nullable1.HasValue)
            {
              nullable3 = nullable1;
              nullable2 = start;
              if ((nullable3.HasValue & nullable2.HasValue ? (nullable3.GetValueOrDefault() <= nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                continue;
            }
          }
          else
            continue;
        }
        if (!Utils.IsEmptyDate(start) && !Utils.IsEmptyDate(end))
        {
          if ((!Utils.IsEmptyDate(startDate) || task.Status != 0) && (!(inTodayOrWeek | inCal) || !Utils.IsEmptyDate(task.CompletedTime) || task.Status == 0))
          {
            if (inTodayOrWeek)
            {
              if (task.Status == 0)
              {
                nullable2 = startDate;
                nullable3 = end;
                if ((nullable2.HasValue & nullable3.HasValue ? (nullable2.GetValueOrDefault() >= nullable3.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                  continue;
              }
              if (task.Status != 0)
              {
                nullable3 = task.CompletedTime;
                nullable2 = start;
                if ((nullable3.HasValue & nullable2.HasValue ? (nullable3.GetValueOrDefault() < nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                  continue;
              }
            }
            else
            {
              if (startDate.HasValue)
              {
                nullable2 = startDate;
                nullable3 = end;
                if ((nullable2.HasValue & nullable3.HasValue ? (nullable2.GetValueOrDefault() >= nullable3.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                {
                  if (nullable1.HasValue)
                  {
                    nullable3 = nullable1;
                    nullable2 = start;
                    if ((nullable3.HasValue & nullable2.HasValue ? (nullable3.GetValueOrDefault() <= nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                      goto label_35;
                  }
                  if (!nullable1.HasValue)
                  {
                    nullable2 = startDate;
                    nullable3 = start;
                    if ((nullable2.HasValue & nullable3.HasValue ? (nullable2.GetValueOrDefault() < nullable3.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                      goto label_38;
                  }
                  else
                    goto label_38;
                }
              }
label_35:
              if (inCal && Utils.IsEmptyDate(startDate) && task.Status != 0 && !Utils.IsEmptyDate(task.CompletedTime))
              {
                nullable3 = task.CompletedTime;
                nullable2 = start;
                if ((nullable3.HasValue & nullable2.HasValue ? (nullable3.GetValueOrDefault() >= nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                {
                  nullable2 = task.CompletedTime;
                  nullable3 = end;
                  if ((nullable2.HasValue & nullable3.HasValue ? (nullable2.GetValueOrDefault() < nullable3.GetValueOrDefault() ? 1 : 0) : 0) == 0)
                    continue;
                }
                else
                  continue;
              }
              else
                continue;
            }
          }
          else
            continue;
        }
label_38:
        flag = true;
        break;
      }
      return type != LogicType.Or ? !flag : flag;
    }

    public static bool IsAvatarUrlEmpty(string avatarUrl)
    {
      return string.IsNullOrEmpty(avatarUrl) || avatarUrl == "-1";
    }

    public static string ToastOnOpenSticky(string taskId)
    {
      TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
      return taskById == null || !taskById.Editable ? Utils.GetString("NoEditingPermission") : string.Empty;
    }

    public static void PostPoneTasks(TaskListView listView)
    {
      if (listView?.ViewModel?.SourceModels == null)
        return;
      List<TaskBaseViewModel> list = listView.ViewModel.SourceModels.ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Editable && t.Status == 0 && t.OutDate())).ToList<TaskBaseViewModel>();
      if (LocalSettings.Settings.ShowSubtasks)
        list.AddRange((IEnumerable<TaskBaseViewModel>) TaskDetailItemCache.GetAllOutDateItems());
      PostponeUndoModel controller = new PostponeUndoModel();
      if (!controller.InitData(list))
        return;
      controller.Do();
      App.Window?.Toast((FrameworkElement) new UndoToast((UndoController) controller));
    }
  }
}
