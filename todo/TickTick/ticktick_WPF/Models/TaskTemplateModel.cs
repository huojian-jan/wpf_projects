// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskTemplateModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Config;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskTemplateModel : BaseModel
  {
    private List<string> _items;
    private List<string> _tags;
    private List<TaskTemplateModel> _children;

    public TaskTemplateModel()
    {
    }

    public TaskTemplateModel(TaskModel task, List<TaskDetailItemModel> checkItems)
    {
      this.Id = Utils.GetGuid();
      this.Title = task.title?.Trim();
      if (task.kind == "TEXT" || task.kind == "NOTE")
      {
        this.Content = TaskUtils.ReplaceAttachmentTextInString(task.content?.Trim(), false);
        this.Kind = task.kind == "NOTE" ? TemplateKind.Note.ToString() : TemplateKind.Task.ToString();
      }
      else
      {
        this.Desc = task.desc?.Trim();
        if (checkItems != null)
        {
          List<TaskDetailItemModel> list = checkItems.ToList<TaskDetailItemModel>();
          list.Sort((Comparison<TaskDetailItemModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
          this.Items = list.Select<TaskDetailItemModel, string>((Func<TaskDetailItemModel, string>) (i => i.title)).ToList<string>();
        }
        else
          this.Items = new List<string>() { "" };
        this.Kind = TemplateKind.Task.ToString();
      }
      this.Tags = task.tag == null ? (List<string>) null : JsonConvert.DeserializeObject<List<string>>(task.tag);
      this.CreatedTime = DateTime.Now;
      this.UserId = LocalSettings.Settings.LoginUserId;
    }

    [NotNull]
    [DefaultValue("\"Task\"")]
    [JsonIgnore]
    public string Kind { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("desc")]
    public string Desc { get; set; }

    [JsonProperty("createdTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime CreatedTime { get; set; }

    [JsonProperty("etag")]
    public string Etag { get; set; }

    [Ignore]
    [JsonProperty("children")]
    public List<TaskTemplateModel> Chidlren
    {
      get => this._children;
      set
      {
        this._children = value;
        this.ChidlrenToString = value != null ? JsonConvert.SerializeObject((object) value) : "[]";
      }
    }

    [JsonIgnore]
    public string ChidlrenToString { get; set; }

    [Ignore]
    [JsonProperty("items")]
    public List<string> Items
    {
      get => this._items;
      set
      {
        this._items = value;
        this.ItemsToString = value != null ? JsonConvert.SerializeObject((object) value) : "[]";
      }
    }

    [JsonIgnore]
    public string ItemsToString { get; set; }

    [Ignore]
    [JsonProperty("tags")]
    public List<string> Tags
    {
      get => this._tags;
      set
      {
        this._tags = value;
        this.TagsToString = value != null ? JsonConvert.SerializeObject((object) value) : "[]";
      }
    }

    [JsonIgnore]
    public string TagsToString { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    public bool IsNote => this.Kind == TemplateKind.Note.ToString();

    public static List<TaskTemplateModel> GetDefaultNoteTemp()
    {
      return new List<TaskTemplateModel>()
      {
        new TaskTemplateModel()
        {
          Id = Utils.GetGuid(),
          Title = Utils.GetString("ReviewTemplateTitle"),
          Content = Utils.GetString("ReviewTemplateContent"),
          Desc = (string) null,
          CreatedTime = DateTime.Now.AddMinutes(-6.0),
          Items = (List<string>) null,
          Tags = (List<string>) null,
          UserId = LocalSettings.Settings.LoginUserId,
          Kind = TemplateKind.Note.ToString()
        },
        new TaskTemplateModel()
        {
          Id = Utils.GetGuid(),
          Title = Utils.GetString("ReadingTemplateTitle"),
          Content = Utils.GetString("ReadingTemplateContent"),
          Desc = (string) null,
          CreatedTime = DateTime.Now.AddMinutes(-5.0),
          Items = (List<string>) null,
          Tags = (List<string>) null,
          UserId = LocalSettings.Settings.LoginUserId,
          Kind = TemplateKind.Note.ToString()
        },
        new TaskTemplateModel()
        {
          Id = Utils.GetGuid(),
          Title = Utils.GetString("MeetingTemplateTitle"),
          Content = Utils.GetString("MeetingTemplateContent"),
          Desc = (string) null,
          CreatedTime = DateTime.Now.AddMinutes(-4.0),
          Items = (List<string>) null,
          Tags = (List<string>) null,
          UserId = LocalSettings.Settings.LoginUserId,
          Kind = TemplateKind.Note.ToString()
        }
      };
    }

    public static List<TaskTemplateModel> GetDefaultTaskTemp()
    {
      return new List<TaskTemplateModel>()
      {
        new TaskTemplateModel()
        {
          Id = Utils.GetGuid(),
          Title = Utils.GetString("TripTemplateTitle"),
          Content = (string) null,
          Desc = (string) null,
          CreatedTime = DateTime.Now.AddMinutes(-3.0),
          Items = new List<string>()
          {
            Utils.GetString("TripTemplateItem01"),
            Utils.GetString("TripTemplateItem02"),
            Utils.GetString("TripTemplateItem03"),
            Utils.GetString("TripTemplateItem04"),
            Utils.GetString("TripTemplateItem05"),
            Utils.GetString("TripTemplateItem06"),
            Utils.GetString("TripTemplateItem07"),
            Utils.GetString("TripTemplateItem08"),
            Utils.GetString("TripTemplateItem09"),
            Utils.GetString("TripTemplateItem10"),
            Utils.GetString("TripTemplateItem11"),
            Utils.GetString("TripTemplateItem12"),
            Utils.GetString("TripTemplateItem13"),
            Utils.GetString("TripTemplateItem14"),
            Utils.GetString("TripTemplateItem15"),
            Utils.GetString("TripTemplateItem16"),
            Utils.GetString("TripTemplateItem17"),
            Utils.GetString("TripTemplateItem18")
          },
          Tags = (List<string>) null,
          UserId = LocalSettings.Settings.LoginUserId,
          Kind = TemplateKind.Task.ToString()
        },
        new TaskTemplateModel()
        {
          Id = Utils.GetGuid(),
          Title = Utils.GetString("DayTemplateTitle"),
          Content = Utils.GetString("DayTemplateContent"),
          Desc = (string) null,
          CreatedTime = DateTime.Now.AddMinutes(-2.0),
          Items = (List<string>) null,
          Tags = (List<string>) null,
          UserId = LocalSettings.Settings.LoginUserId,
          Kind = TemplateKind.Task.ToString()
        },
        new TaskTemplateModel()
        {
          Id = Utils.GetGuid(),
          Title = Utils.GetString("WorkTemplateTitle"),
          Content = (string) null,
          Desc = (string) null,
          CreatedTime = DateTime.Now.AddMinutes(-1.0),
          Items = new List<string>()
          {
            Utils.GetString("WorkTemplateItem01"),
            Utils.GetString("WorkTemplateItem02"),
            Utils.GetString("WorkTemplateItem03"),
            Utils.GetString("WorkTemplateItem04"),
            Utils.GetString("WorkTemplateItem05"),
            Utils.GetString("WorkTemplateItem06"),
            Utils.GetString("WorkTemplateItem07")
          },
          Tags = (List<string>) null,
          UserId = LocalSettings.Settings.LoginUserId,
          Kind = TemplateKind.Task.ToString()
        }
      };
    }

    public static async Task<TaskTemplateModel> Build(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
      if (task == null || string.IsNullOrEmpty(task.title) && string.IsNullOrEmpty(task.content) && string.IsNullOrEmpty(task.desc) && checkItemsByTaskId.All<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (t => string.IsNullOrEmpty(t.title))))
        return (TaskTemplateModel) null;
      TaskTemplateModel template = new TaskTemplateModel(task, checkItemsByTaskId);
      Node<TaskBaseViewModel> taskNode = TaskCache.GetTaskNode(task.id, task.projectId);
      TaskTemplateModel taskTemplateModel = template;
      taskTemplateModel.Chidlren = await TaskTemplateModel.GetTemplateChildren(template, taskNode);
      taskTemplateModel = (TaskTemplateModel) null;
      return template;
    }

    private static async Task<List<TaskTemplateModel>> GetTemplateChildren(
      TaskTemplateModel templateModel,
      Node<TaskBaseViewModel> node)
    {
      Node<TaskBaseViewModel> node1 = node;
      int num1;
      if (node1 == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = node1.Children?.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 == 0)
        return (List<TaskTemplateModel>) null;
      IOrderedEnumerable<Node<TaskBaseViewModel>> orderedEnumerable = node.Children.OrderBy<Node<TaskBaseViewModel>, long?>((Func<Node<TaskBaseViewModel>, long?>) (n => n.Value?.SortOrder));
      List<TaskTemplateModel> templateChildren = new List<TaskTemplateModel>();
      foreach (Node<TaskBaseViewModel> node2 in (IEnumerable<Node<TaskBaseViewModel>>) orderedEnumerable)
      {
        Node<TaskBaseViewModel> child = node2;
        TaskModel task = await TaskDao.GetTaskById(child.Value?.Id);
        if (task != null && (task.status == 0 || string.IsNullOrEmpty(task.repeatTaskId)))
        {
          TaskTemplateModel template = new TaskTemplateModel(task, await TaskDetailItemDao.GetCheckItemsByTaskId(child.NodeId));
          TaskTemplateModel taskTemplateModel = template;
          taskTemplateModel._children = await TaskTemplateModel.GetTemplateChildren(template, child);
          taskTemplateModel = (TaskTemplateModel) null;
          templateChildren.Add(template);
          task = (TaskModel) null;
          template = (TaskTemplateModel) null;
          child = (Node<TaskBaseViewModel>) null;
        }
      }
      return templateChildren;
    }
  }
}
