// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ProjectOrGroupCardViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Views.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class ProjectOrGroupCardViewModel : FilterListDisplayModel<string>
  {
    public ProjectOrGroupCardViewModel() => this.ConditionName = "listOrGroup";

    public List<string> Values
    {
      get => this.ValueList;
      set
      {
        this.ValueList = value;
        this.OnPropertyChanged(nameof (Values));
      }
    }

    public List<string> GroupIds { get; set; } = new List<string>();

    public override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new ProjectOrGroupEditDialog();
    }

    public override Rule<string> ToRule()
    {
      List<ProjectModel> projects = CacheManager.GetProjects();
      ListOrGroupRule rule = (ListOrGroupRule) null;
      if (this.ValueList.Count > 0 || this.GroupIds.Count > 0)
      {
        List<string> stringList = new List<string>();
        List<string> projectIds = this.ValueList.Where<string>((Func<string, bool>) (p => !string.IsNullOrEmpty(p))).ToList<string>();
        List<string> groupIds = this.GroupIds.Where<string>((Func<string, bool>) (g => !string.IsNullOrEmpty(g))).ToList<string>();
        if (groupIds.Count > 0)
        {
          List<ProjectModel> list = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => projectIds.Contains(p.id))).ToList<ProjectModel>();
          stringList.AddRange(list.Where<ProjectModel>((Func<ProjectModel, bool>) (project => string.IsNullOrEmpty(project.groupId) || project.groupId == "NONE" || !groupIds.Contains(project.groupId))).Select<ProjectModel, string>((Func<ProjectModel, string>) (project => project.id)));
        }
        else
          stringList = projectIds;
        if (this.LogicType == LogicType.Not)
        {
          rule = new ListOrGroupRule()
          {
            not = new List<object>()
          };
          if (stringList.Count > 0)
          {
            ListRule listRule1 = new ListRule();
            listRule1.or = stringList;
            ListRule listRule2 = listRule1;
            rule.not.Add((object) listRule2);
          }
          if (groupIds.Count > 0)
          {
            GroupRule groupRule1 = new GroupRule();
            groupRule1.or = groupIds;
            GroupRule groupRule2 = groupRule1;
            rule.not.Add((object) groupRule2);
          }
        }
        else
        {
          rule = new ListOrGroupRule()
          {
            or = new List<object>()
          };
          if (stringList.Count > 0)
          {
            ListRule listRule3 = new ListRule();
            listRule3.or = stringList;
            ListRule listRule4 = listRule3;
            rule.or.Add((object) listRule4);
          }
          if (groupIds.Count > 0)
          {
            GroupRule groupRule3 = new GroupRule();
            groupRule3.or = groupIds;
            GroupRule groupRule4 = groupRule3;
            rule.or.Add((object) groupRule4);
          }
        }
      }
      return (Rule<string>) rule;
    }

    public override Rule<string> GetParseRule() => (Rule<string>) new ListRule();

    public override string ToCardText()
    {
      if (this.ValueList.Count == 0 && this.GroupIds.Count == 0)
        return Utils.GetString("lists");
      List<ProjectModel> projects = CacheManager.GetProjects();
      string str = this.LogicType == LogicType.Not ? Utils.GetString("Exclude") + " " : string.Empty;
      string divider = this.LogicType == LogicType.Not ? ", " : " " + Utils.GetString("or") + " ";
      Func<ProjectModel, bool> predicate = (Func<ProjectModel, bool>) (p => this.ValueList.Contains(p.id) || this.GroupIds.Contains(p.groupId));
      List<ProjectModel> list = projects.Where<ProjectModel>(predicate).ToList<ProjectModel>();
      if (this.ValueList.Contains("Calendar5959a2259161d16d23a4f272"))
        list.Add(new ProjectModel()
        {
          id = "Calendar5959a2259161d16d23a4f272",
          name = Utils.GetString("Calendar")
        });
      switch (list.Count)
      {
        case 0:
          return Utils.GetString("Expired");
        case 1:
          return str + list[0].name;
        default:
          string displayText = string.Empty;
          if (list.Count > 0)
          {
            if (list.Count <= 3)
              displayText = ProjectOrGroupCardViewModel.GetProjectFullText((IReadOnlyList<ProjectModel>) list, displayText, divider);
            else
              displayText = ProjectOrGroupCardViewModel.GetProjectFullText((IReadOnlyList<ProjectModel>) list.Take<ProjectModel>(3).ToList<ProjectModel>(), displayText, divider) + " " + Utils.GetString("or") + " " + string.Format(Utils.GetString("OtherProjects"), (object) (list.Count - 3));
          }
          return string.IsNullOrEmpty(str) ? displayText : str + "( " + displayText + " )";
      }
    }

    private static string GetProjectFullText(
      IReadOnlyList<ProjectModel> selectedProjects,
      string displayText,
      string divider = "")
    {
      for (int index = 0; index < selectedProjects.Count; ++index)
        displayText = index >= selectedProjects.Count - 1 ? displayText + selectedProjects[index].name : displayText + selectedProjects[index].name + divider;
      return displayText;
    }
  }
}
