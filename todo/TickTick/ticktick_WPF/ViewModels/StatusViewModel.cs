// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.StatusViewModel
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
  public class StatusViewModel : FilterListDisplayModel<string>
  {
    private static readonly Dictionary<string, string> StatusTypeMap = new Dictionary<string, string>()
    {
      {
        "uncompleted",
        Utils.GetString("Uncompleted")
      },
      {
        "inProgress",
        Utils.GetString("SummaryInProgress")
      },
      {
        "completed",
        Utils.GetString("Completed")
      },
      {
        "wontDo",
        Utils.GetString("Abandoned")
      }
    };

    public StatusViewModel() => this.ConditionName = "status";

    public List<string> Values
    {
      get => this.ValueList;
      set
      {
        this.ValueList = value;
        this.OnPropertyChanged(nameof (Values));
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
            displayList = displayList + StatusViewModel.StatusTypeMap[types[index]] + " " + Utils.GetString("or") + " ";
          else
            displayList += StatusViewModel.StatusTypeMap[types[index]];
        }
      }
      return displayList;
    }

    public override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new PriorityEditDialog();
    }

    public override string ToCardText() => StatusViewModel.ToDisplayList(this.ValueList);

    public override Rule<string> GetParseRule() => (Rule<string>) new StatusRule();
  }
}
