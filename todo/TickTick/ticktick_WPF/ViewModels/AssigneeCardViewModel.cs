// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.AssigneeCardViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Views.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class AssigneeCardViewModel : FilterListDisplayModel<string>
  {
    private static readonly Dictionary<string, string> AssigneeMap = new Dictionary<string, string>()
    {
      {
        "me",
        Utils.GetString("Me")
      },
      {
        "other",
        Utils.GetString("AssignedToOthers")
      },
      {
        "noassignee",
        Utils.GetString("NotAssigned")
      },
      {
        "anyone",
        Utils.GetString("Assigned")
      }
    };

    public AssigneeCardViewModel() => this.ConditionName = "assignee";

    public List<string> Values
    {
      get => this.ValueList;
      set
      {
        this.ValueList = value;
        this.OnPropertyChanged(nameof (Values));
      }
    }

    private static string GetValueTitle(string val)
    {
      if (AssigneeCardViewModel.AssigneeMap.ContainsKey(val))
        return AssigneeCardViewModel.AssigneeMap[val];
      return AvatarHelper.GetUserModelById(val)?.displayName;
    }

    public static string GetAssigneeValueText(List<string> assignees)
    {
      switch (assignees.Count)
      {
        case 0:
          return Utils.GetString("assignee");
        case 1:
          return AssigneeCardViewModel.GetValueTitle(assignees[0]);
        default:
          string assigneeValueText = string.Empty;
          int num = Math.Min(3, assignees.Count);
          for (int index = 0; index < num; ++index)
          {
            string valueTitle = AssigneeCardViewModel.GetValueTitle(assignees[index]);
            if (index < num - 1)
              assigneeValueText = assigneeValueText + valueTitle + " " + Utils.GetString("or") + " ";
            else
              assigneeValueText += valueTitle;
          }
          if (assignees.Count > 3)
            assigneeValueText = string.Format(Utils.GetString("AssignDisplayMessage"), (object) assigneeValueText, (object) (assignees.Count - 3));
          return assigneeValueText;
      }
    }

    public override string ToCardText()
    {
      return AssigneeCardViewModel.GetAssigneeValueText(this.ValueList);
    }

    public override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new AssigneeEditDialog(this.Version);
    }

    public override Rule<string> GetParseRule() => (Rule<string>) new AssigneeRule();

    public static string ToNormalDisplayText(List<string> assignees, int displayNum = 3)
    {
      return AssigneeCardViewModel.ToAssigneeText(assignees, ", ", displayNum);
    }

    private static string ToAssigneeText(List<string> assignees, string divider, int displayNum = 3)
    {
      if (assignees.Count == 0)
        return Utils.GetString("All");
      return assignees.Count <= displayNum ? AssigneeCardViewModel.DisplayAssigneeText((IReadOnlyList<string>) assignees, divider) : string.Format(Utils.GetString("AssignDisplayMessage"), (object) AssigneeCardViewModel.DisplayAssigneeText((IReadOnlyList<string>) assignees.Take<string>(displayNum).ToList<string>(), divider), (object) (assignees.Count - displayNum));
    }

    private static string DisplayAssigneeText(IReadOnlyList<string> assignees, string divider)
    {
      string str = string.Empty;
      if (assignees != null && assignees.Count > 0)
      {
        for (int index = 0; index < assignees.Count; ++index)
          str = index >= assignees.Count - 1 ? str + AssigneeCardViewModel.GetValueTitle(assignees[index]) : str + AssigneeCardViewModel.GetValueTitle(assignees[index]) + divider;
      }
      return str;
    }
  }
}
