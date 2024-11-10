// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ChecklistUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util
{
  public class ChecklistUtils
  {
    private const string LineSeperator = "\r\n";
    private const string DescriptionSeperator = "\r\n  \r\n";

    public static ChecklistExtra Text2Items(string content)
    {
      content = content?.Replace("\r\n", "\n").Replace("\n", "\r\n") ?? string.Empty;
      ChecklistExtra checklistExtra = new ChecklistExtra()
      {
        Description = string.Empty,
        ChecklistItems = new List<string>()
      };
      string[] strArray1 = content.Split(new string[1]
      {
        "\r\n  \r\n"
      }, StringSplitOptions.None);
      int num = 0;
      if (strArray1.Length >= 2)
      {
        checklistExtra.Description = strArray1[0];
        num = 1;
      }
      for (int index = num; index < strArray1.Length; ++index)
      {
        string[] strArray2 = strArray1[index].Split(new string[1]
        {
          "\r\n"
        }, StringSplitOptions.None);
        if (strArray2.Length != 0)
        {
          foreach (string str in strArray2)
          {
            if (!string.IsNullOrEmpty(str))
              checklistExtra.ChecklistItems.Add(str);
          }
        }
      }
      if (checklistExtra.ChecklistItems.Count == 0)
        checklistExtra.ChecklistItems.Add(string.Empty);
      return checklistExtra;
    }

    public static List<TaskDetailItemModel> BuildChecklist(
      int taskId,
      string taskSid,
      List<string> titles)
    {
      List<TaskDetailItemModel> taskDetailItemModelList = new List<TaskDetailItemModel>();
      int num = 0;
      foreach (string title in titles)
      {
        TaskDetailItemModel taskDetailItemModel = new TaskDetailItemModel()
        {
          id = Utils.GetGuid(),
          title = title,
          status = 0,
          TaskId = taskId,
          TaskServerId = taskSid,
          sortOrder = (long) num
        };
        num += 2048;
        taskDetailItemModelList.Add(taskDetailItemModel);
      }
      return taskDetailItemModelList;
    }

    public static string Items2Text(string description, List<string> checklist)
    {
      string empty = string.Empty;
      return string.IsNullOrEmpty(description) ? ChecklistUtils.AppendContent(description, (IEnumerable<string>) checklist, empty) : (checklist == null || checklist.Count == 0 ? description + "\r\n  \r\n" : ChecklistUtils.AppendContent(description, (IEnumerable<string>) checklist, empty));
    }

    private static string AppendContent(
      string description,
      IEnumerable<string> checklist,
      string content)
    {
      if (!string.IsNullOrEmpty(description))
      {
        content += description;
        if (!content.EndsWith("\r\n  \r\n"))
          content += "\r\n  \r\n";
      }
      content = checklist.Aggregate<string, string>(content, (Func<string, string, string>) ((current, item) => current + item + "\r\n"));
      if (content.EndsWith("\r\n"))
        content = content.Substring(0, content.Length - "\r\n".Length);
      return content;
    }
  }
}
