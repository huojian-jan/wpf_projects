// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.NormalFilterViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Dal;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class NormalFilterViewModel : BaseViewModel
  {
    private List<string> _assignees = new List<string>();
    private string _displayProjectText = string.Empty;
    private List<string> _dueDates = new List<string>();
    private List<string> _groups = new List<string>();
    private List<int> _priorities = new List<int>();
    private List<string> _projects = new List<string>();
    private List<string> _tags = new List<string>();
    private List<string> _status = new List<string>();
    private List<string> _taskTypes = new List<string>();
    private List<string> _completedTimes = new List<string>();
    public int Version = 1;

    public NormalFilterViewModel()
    {
    }

    public NormalFilterViewModel(SearchFilterModel searchFilter)
    {
      if (!string.IsNullOrWhiteSpace(searchFilter.Key))
        this.Keywords.Add(searchFilter.Key);
      List<string> assignees = searchFilter.Assignees;
      // ISSUE: explicit non-virtual call
      if ((assignees != null ? (__nonvirtual (assignees.Count) > 0 ? 1 : 0) : 0) != 0)
        this._assignees.AddRange((IEnumerable<string>) searchFilter.Assignees);
      List<int> priorities = searchFilter.Priorities;
      // ISSUE: explicit non-virtual call
      if ((priorities != null ? (__nonvirtual (priorities.Count) > 0 ? 1 : 0) : 0) != 0)
        this._priorities.AddRange((IEnumerable<int>) searchFilter.Priorities);
      List<string> tags = searchFilter.Tags;
      // ISSUE: explicit non-virtual call
      if ((tags != null ? (__nonvirtual (tags.Count) > 0 ? 1 : 0) : 0) != 0)
        this._tags.AddRange((IEnumerable<string>) searchFilter.Tags);
      List<string> projectIds = searchFilter.ProjectIds;
      // ISSUE: explicit non-virtual call
      if ((projectIds != null ? (__nonvirtual (projectIds.Count) > 0 ? 1 : 0) : 0) != 0)
        this._projects.AddRange((IEnumerable<string>) searchFilter.ProjectIds);
      List<string> groupIds = searchFilter.GroupIds;
      // ISSUE: explicit non-virtual call
      if ((groupIds != null ? (__nonvirtual (groupIds.Count) > 0 ? 1 : 0) : 0) != 0)
        this._groups.AddRange((IEnumerable<string>) searchFilter.GroupIds);
      switch (searchFilter.TaskType)
      {
        case TaskType.Task:
          this._taskTypes.Add("task");
          break;
        case TaskType.Note:
          this._taskTypes.Add("note");
          break;
      }
      switch (searchFilter.DateFilter)
      {
        case DateFilter.ThisWeek:
          this._dueDates.Add("thisweek");
          break;
        case DateFilter.ThisMonth:
          this._dueDates.Add("thismonth");
          break;
      }
    }

    public List<int> Priorities
    {
      get => this._priorities ?? new List<int>();
      set
      {
        this._priorities = value;
        this.OnPropertyChanged(nameof (Priorities));
      }
    }

    public List<string> TaskTypes
    {
      get => this._taskTypes ?? new List<string>();
      set
      {
        this._taskTypes = value;
        this.OnPropertyChanged(nameof (TaskTypes));
      }
    }

    public List<string> Status
    {
      get => this._status ?? new List<string>();
      set
      {
        this._status = value;
        this.OnPropertyChanged(nameof (Status));
      }
    }

    public List<string> Tags
    {
      get => this._tags ?? new List<string>();
      set
      {
        this._tags = TagDao.GetValidAndChildren(value);
        this.OnPropertyChanged(nameof (Tags));
      }
    }

    public List<string> DueDates
    {
      get => this._dueDates ?? new List<string>();
      set
      {
        this._dueDates = value;
        this.OnPropertyChanged(nameof (DueDates));
      }
    }

    public List<string> CompletedTimes
    {
      get => this._completedTimes ?? new List<string>();
      set
      {
        this._completedTimes = value;
        this.OnPropertyChanged(nameof (CompletedTimes));
      }
    }

    public List<string> Projects
    {
      get => this._projects ?? new List<string>();
      set
      {
        this._projects = value;
        this.OnPropertyChanged(nameof (Projects));
      }
    }

    public List<string> Groups
    {
      get => this._groups ?? new List<string>();
      set
      {
        this._groups = value;
        this.OnPropertyChanged(nameof (Groups));
      }
    }

    public List<string> Assignees
    {
      get => this._assignees ?? new List<string>();
      set
      {
        this._assignees = value;
        this.OnPropertyChanged(nameof (Assignees));
      }
    }

    public string DisplayProjectText
    {
      get => this._displayProjectText;
      set
      {
        this._displayProjectText = value;
        this.OnPropertyChanged(nameof (DisplayProjectText));
      }
    }

    public List<string> Keywords { get; set; } = new List<string>();

    public bool Empty()
    {
      return !this.Priorities.Any<int>() && !this.Tags.Any<string>() && !this.DueDates.Any<string>() && !this.Projects.Any<string>() && !this.Groups.Any<string>() && !this.Assignees.Any<string>() && !this.Keywords.Any<string>() && !this.TaskTypes.Any<string>();
    }

    public void NotifyPriorityChanged() => this.OnPropertyChanged("Priorities");

    public void NotifyTaskTypeChanged() => this.OnPropertyChanged("TaskTypes");

    public void NotifyAssigneeChanged() => this.OnPropertyChanged("Assignees");

    public bool OnlyNote()
    {
      List<string> taskTypes = this._taskTypes;
      // ISSUE: explicit non-virtual call
      return (taskTypes != null ? (__nonvirtual (taskTypes.Count) == 1 ? 1 : 0) : 0) != 0 && this._taskTypes.Contains("note");
    }

    public string ToRule(bool isSave)
    {
      NormalRuleModel normalRuleModel = new NormalRuleModel()
      {
        and = new List<object>()
      };
      if (this.Projects.Count > 0 || this.Groups.Count > 0)
      {
        ListOrGroupRule listOrGroupRule = new ListOrGroupRule()
        {
          or = new List<object>()
        };
        if (this.Projects.Count > 0)
        {
          ListRule listRule1 = new ListRule();
          listRule1.or = this.Projects.Where<string>((Func<string, bool>) (p => !string.IsNullOrEmpty(p))).ToList<string>();
          ListRule listRule2 = listRule1;
          listOrGroupRule.or.Add((object) listRule2);
        }
        if (this.Groups.Count > 0)
        {
          GroupRule groupRule1 = new GroupRule();
          groupRule1.or = this.Groups.Where<string>((Func<string, bool>) (g => !string.IsNullOrEmpty(g))).ToList<string>();
          GroupRule groupRule2 = groupRule1;
          listOrGroupRule.or.Add((object) groupRule2);
        }
        normalRuleModel.and.Add((object) listOrGroupRule);
      }
      if (this.Tags.Count > 0)
      {
        TagRule tagRule1 = new TagRule();
        tagRule1.or = this.Tags;
        TagRule tagRule2 = tagRule1;
        normalRuleModel.and.Add((object) tagRule2);
      }
      if (this.Assignees.Count > 0)
      {
        AssigneeRule assigneeRule1 = new AssigneeRule();
        assigneeRule1.or = this.Assignees;
        AssigneeRule assigneeRule2 = assigneeRule1;
        normalRuleModel.and.Add((object) assigneeRule2);
        if (this.Assignees.Any<string>((Func<string, bool>) (assignee => assignee != "me" && assignee != "other" && assignee != "noassignee")))
          normalRuleModel.version = 3;
      }
      if (this.Priorities.Count > 0)
      {
        PriorityRule priorityRule1 = new PriorityRule();
        priorityRule1.or = this.Priorities;
        PriorityRule priorityRule2 = priorityRule1;
        normalRuleModel.and.Add((object) priorityRule2);
      }
      if (this.Status.Count > 0)
      {
        StatusRule statusRule1 = new StatusRule();
        statusRule1.or = this.Status;
        StatusRule statusRule2 = statusRule1;
        normalRuleModel.and.Add((object) statusRule2);
      }
      if (this.TaskTypes.Count > 0)
      {
        TaskTypeRule taskTypeRule1 = new TaskTypeRule();
        taskTypeRule1.or = this.TaskTypes;
        TaskTypeRule taskTypeRule2 = taskTypeRule1;
        normalRuleModel.and.Add((object) taskTypeRule2);
        normalRuleModel.version = 3;
      }
      if (this.Keywords.Count > 0)
      {
        KeywordsRule keywordsRule1 = new KeywordsRule();
        keywordsRule1.or = this.Keywords;
        KeywordsRule keywordsRule2 = keywordsRule1;
        normalRuleModel.and.Add((object) keywordsRule2);
        normalRuleModel.version = 3;
      }
      if (this.DueDates.Count > 0)
      {
        if (this.CompletedTimes.Count > 0)
        {
          DateRule dateRule = new DateRule()
          {
            or = new List<object>()
          };
          DueDateRule dueDateRule1 = new DueDateRule();
          dueDateRule1.or = this._dueDates;
          DueDateRule dueDateRule2 = dueDateRule1;
          dateRule.or.Add((object) dueDateRule2);
          CompletedTimeRule completedTimeRule1 = new CompletedTimeRule();
          completedTimeRule1.or = this._completedTimes;
          CompletedTimeRule completedTimeRule2 = completedTimeRule1;
          dateRule.or.Add((object) completedTimeRule2);
          normalRuleModel.and.Add((object) dateRule);
        }
        else
        {
          DueDateRule dueDateRule3 = new DueDateRule();
          dueDateRule3.or = this.DueDates;
          DueDateRule dueDateRule4 = dueDateRule3;
          normalRuleModel.and.Add((object) dueDateRule4);
          if (this.DueDates.Any<string>((Func<string, bool>) (d => d.StartsWith("offset"))))
            normalRuleModel.version = 6;
          else if (this.DueDates.Any<string>((Func<string, bool>) (d => d.StartsWith("span"))))
            normalRuleModel.version = 4;
          else if (this.DueDates.Any<string>((Func<string, bool>) (d => d.EndsWith("daysfromtoday") || d == "recurring")))
            normalRuleModel.version = 3;
        }
      }
      if (normalRuleModel.and.Count <= 0)
        return string.Empty;
      if (isSave)
      {
        foreach (object obj in normalRuleModel.and)
        {
          string data = (string) null;
          switch (obj)
          {
            case ListOrGroupRule _:
              data = "project";
              break;
            case TagRule _:
              data = "tag";
              break;
            case DueDateRule _:
              data = "date";
              break;
            case AssigneeRule _:
              data = "assignee";
              break;
            case PriorityRule _:
              data = "priority";
              break;
            case TaskTypeRule _:
              data = "type";
              break;
            case KeywordsRule _:
              data = "content";
              break;
          }
          if (!string.IsNullOrEmpty(data))
            UserActCollectUtils.AddClickEvent("edit_filter", "conditions", data);
        }
      }
      return JsonConvert.SerializeObject((object) normalRuleModel);
    }
  }
}
