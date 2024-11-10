// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.InitConditionCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class InitConditionCard : ConditionCard
  {
    private readonly Dictionary<CondType, string> _conditionDict = new Dictionary<CondType, string>()
    {
      {
        CondType.Lists,
        Utils.GetString("lists")
      },
      {
        CondType.Tag,
        Utils.GetString("tag")
      },
      {
        CondType.Date,
        Utils.GetString("date")
      },
      {
        CondType.Priority,
        Utils.GetString("priority")
      },
      {
        CondType.Assignee,
        Utils.GetString("assignee")
      },
      {
        CondType.KeyWords,
        Utils.GetString("ContentInclude")
      },
      {
        CondType.TaskType,
        Utils.GetString("TaskType")
      }
    };

    public InitConditionCard(CardViewModel viewModel)
      : base(viewModel)
    {
    }

    protected override void OnCardClick() => this.ShowSelectConditionDialog();

    private void ShowSelectConditionDialog()
    {
      List<ListItemData> listItems = new List<ListItemData>();
      foreach (CondType leftCondType in this.GetLeftCondTypes())
        listItems.Add(new ListItemData((object) leftCondType, this._conditionDict[leftCondType]));
      this.ShowDropdownDialog(listItems, (EventHandler<ListItemData>) (async (x, item) =>
      {
        InitConditionCard initConditionCard = this;
        CondType key = (CondType) item.Key;
        CardViewModel cardViewModel = (CardViewModel) null;
        switch (key)
        {
          case CondType.Lists:
            cardViewModel = (CardViewModel) new ProjectOrGroupCardViewModel();
            break;
          case CondType.Tag:
            cardViewModel = (CardViewModel) new TagCardViewModel();
            break;
          case CondType.Date:
            cardViewModel = (CardViewModel) new DateCardViewModel();
            break;
          case CondType.Priority:
            cardViewModel = (CardViewModel) new PriorityCardViewModel();
            break;
          case CondType.Assignee:
            cardViewModel = (CardViewModel) new AssigneeCardViewModel();
            break;
          case CondType.KeyWords:
            cardViewModel = (CardViewModel) new KeywordsViewModel()
            {
              IsNewAdd = true
            };
            break;
          case CondType.TaskType:
            cardViewModel = (CardViewModel) new TaskTypeViewModel();
            break;
        }
        if (cardViewModel == null)
          return;
        cardViewModel.ShowDropdown = true;
        initConditionCard.FireEvent(new ChangeData()
        {
          Type = ChangeType.Filter,
          From = initConditionCard.ViewModel,
          To = cardViewModel
        });
      }));
    }
  }
}
