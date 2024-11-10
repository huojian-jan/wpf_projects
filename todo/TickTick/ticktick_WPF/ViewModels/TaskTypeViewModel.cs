// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TaskTypeViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Views.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TaskTypeViewModel : FilterListDisplayModel<string>
  {
    private static readonly Dictionary<string, string> TaskTypeMap = new Dictionary<string, string>()
    {
      {
        "task",
        Utils.GetString("Task")
      },
      {
        "note",
        Utils.GetString("Notes")
      }
    };

    public TaskTypeViewModel() => this.ConditionName = "taskType";

    public List<string> Values
    {
      get => this.ValueList;
      set
      {
        this.ValueList = value;
        this.OnPropertyChanged(nameof (Values));
      }
    }

    public bool IsNote
    {
      get
      {
        List<string> values = this.Values;
        return (values != null ? (__nonvirtual (values.Count) == 1 ? 1 : 0) : 0) != 0 && this.Values[0] == "note";
      }
    }

    public static string ToDisplayList(List<string> types)
    {
      string displayList = string.Empty;
      if (types != null && types.Count > 0)
      {
        for (int index = 0; index < types.Count; ++index)
        {
          if (index < types.Count - 1)
            displayList = displayList + TaskTypeViewModel.TaskTypeMap[types[index]] + " " + Utils.GetString("or") + " ";
          else
            displayList += TaskTypeViewModel.TaskTypeMap[types[index]];
        }
      }
      return displayList;
    }

    public override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new PriorityEditDialog();
    }

    public override string ToCardText()
    {
      switch (this.ValueList.Count)
      {
        case 0:
          return Utils.GetString("TaskType");
        case 1:
          return TaskTypeViewModel.TaskTypeMap[this.ValueList[0]];
        default:
          return TaskTypeViewModel.ToDisplayList(this.ValueList);
      }
    }

    public override Rule<string> GetParseRule() => (Rule<string>) new TaskTypeRule();
  }
}
