// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.PriorityCardViewModel
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
  public class PriorityCardViewModel : FilterListDisplayModel<int>
  {
    private static readonly Dictionary<int, string> PriorityMap = new Dictionary<int, string>()
    {
      {
        0,
        Utils.GetString("PriorityNull")
      },
      {
        1,
        Utils.GetString("PriorityLow")
      },
      {
        3,
        Utils.GetString("PriorityMedium")
      },
      {
        5,
        Utils.GetString("PriorityHigh")
      }
    };

    public PriorityCardViewModel() => this.ConditionName = "priority";

    public List<int> Values
    {
      get => this.ValueList;
      set
      {
        this.ValueList = value;
        this.Content = PriorityCardViewModel.ToDisplayList(this.ValueList);
        this.OnPropertyChanged(nameof (Values));
      }
    }

    private static string ToDisplayList(List<int> priorities)
    {
      string displayList = string.Empty;
      if (priorities != null && priorities.Count > 0)
      {
        for (int index = 0; index < priorities.Count; ++index)
        {
          if (index < priorities.Count - 1)
            displayList = displayList + PriorityCardViewModel.PriorityMap[priorities[index]] + " " + Utils.GetString("or") + " ";
          else
            displayList += PriorityCardViewModel.PriorityMap[priorities[index]];
        }
      }
      return displayList;
    }

    public override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new PriorityEditDialog();
    }

    public override Rule<int> GetParseRule() => (Rule<int>) new PriorityRule();

    public override string ToCardText() => PriorityCardViewModel.ToCardText(this.ValueList);

    public static string ToCardText(List<int> priorities)
    {
      switch (priorities.Count)
      {
        case 0:
          return Utils.GetString("priority");
        case 1:
          return PriorityCardViewModel.PriorityMap[priorities[0]];
        default:
          return PriorityCardViewModel.ToDisplayList(priorities);
      }
    }
  }
}
