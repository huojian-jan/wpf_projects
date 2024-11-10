// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.AdvancedFilterViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class AdvancedFilterViewModel : BaseViewModel
  {
    public int Version = 1;

    public List<CardViewModel> CardList { get; set; } = new List<CardViewModel>();

    public string ToRule(bool isSave)
    {
      List<CardViewModel> list = this.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (x => x.Type == CardType.Filter || x.Type == CardType.LogicAnd || x.Type == CardType.LogicOr)).ToList<CardViewModel>();
      int val2 = this.WithNotLogic(list) ? 2 : 1;
      foreach (CardViewModel cardViewModel in list)
      {
        string data = (string) null;
        string conditionName = cardViewModel.ConditionName;
        if (conditionName != null)
        {
          switch (conditionName.Length)
          {
            case 3:
              if (conditionName == "tag")
              {
                data = "tag";
                break;
              }
              break;
            case 7:
              if (conditionName == "dueDate")
              {
                data = "date";
                Rule<string> rule = cardViewModel is FilterListDisplayModel<string> listDisplayModel ? listDisplayModel.ToRule() : (Rule<string>) null;
                List<string> source = rule?.or ?? rule?.not;
                if (source != null)
                {
                  if (source.Any<string>((Func<string, bool>) (d => d.StartsWith("offset"))))
                  {
                    val2 = Math.Max(6, val2);
                    break;
                  }
                  if (source.Any<string>((Func<string, bool>) (d => d.StartsWith("span"))))
                  {
                    val2 = Math.Max(4, val2);
                    break;
                  }
                  if (source.Any<string>((Func<string, bool>) (date => date.EndsWith("daysfromtoday") || date == "recurring")))
                  {
                    val2 = Math.Max(3, val2);
                    break;
                  }
                  break;
                }
                break;
              }
              break;
            case 8:
              switch (conditionName[0])
              {
                case 'a':
                  if (conditionName == "assignee")
                  {
                    data = "assignee";
                    Rule<string> rule = cardViewModel is FilterListDisplayModel<string> listDisplayModel ? listDisplayModel.ToRule() : (Rule<string>) null;
                    if (rule?.or != null && rule.or.Any<string>((Func<string, bool>) (assign => assign != "me" && assign != "other" && assign != "noassignee")))
                    {
                      val2 = Math.Max(3, val2);
                      break;
                    }
                    break;
                  }
                  break;
                case 'k':
                  if (conditionName == "keywords")
                  {
                    data = "content";
                    val2 = Math.Max(3, val2);
                    break;
                  }
                  break;
                case 'p':
                  if (conditionName == "priority")
                  {
                    data = "priority";
                    break;
                  }
                  break;
                case 't':
                  if (conditionName == "taskType")
                  {
                    data = "type";
                    val2 = Math.Max(3, val2);
                    break;
                  }
                  break;
              }
              break;
            case 11:
              if (conditionName == "listOrGroup")
              {
                data = "project";
                break;
              }
              break;
          }
        }
        if (isSave && !string.IsNullOrEmpty(data))
          UserActCollectUtils.AddClickEvent("edit_filter", "conditions", data);
      }
      switch (list.Count)
      {
        case 1:
          AdvancedRuleModel advanced1 = new AdvancedRuleModel()
          {
            and = new List<object>()
          };
          AdvancedFilterViewModel.AddRule(list[0], advanced1, true);
          advanced1.version = val2;
          return JsonConvert.SerializeObject((object) advanced1);
        case 3:
          AdvancedRuleModel advanced2 = new AdvancedRuleModel();
          if (list[1].Type == CardType.LogicAnd)
          {
            advanced2.and = new List<object>();
            AdvancedFilterViewModel.AddRule(list[0], advanced2, true);
            AdvancedFilterViewModel.AddRule(list[2], advanced2, true);
          }
          else
          {
            advanced2.or = new List<object>();
            AdvancedFilterViewModel.AddRule(list[0], advanced2, false);
            AdvancedFilterViewModel.AddRule(list[2], advanced2, false);
          }
          advanced2.version = val2;
          return JsonConvert.SerializeObject((object) advanced2);
        case 5:
          AdvancedRuleModel advanced3 = new AdvancedRuleModel();
          if (AdvancedFilterViewModel.IsSameLogic(list[1], list[3]))
          {
            if ((list[1].Type == CardType.LogicAnd ? 0 : 1) == 0)
            {
              advanced3.and = new List<object>();
              AdvancedFilterViewModel.AddRule(list[0], advanced3, true);
              AdvancedFilterViewModel.AddRule(list[2], advanced3, true);
              AdvancedFilterViewModel.AddRule(list[4], advanced3, true);
            }
            else
            {
              advanced3.or = new List<object>();
              AdvancedFilterViewModel.AddRule(list[0], advanced3, false);
              AdvancedFilterViewModel.AddRule(list[2], advanced3, false);
              AdvancedFilterViewModel.AddRule(list[4], advanced3, false);
            }
          }
          else
          {
            int num = list[1].Type == CardType.LogicAnd ? 0 : 1;
            LogicType logicType = list[3].Type == CardType.LogicAnd ? LogicType.And : LogicType.Or;
            BaseRuleModel baseRuleModel = new BaseRuleModel();
            if (num == 0)
            {
              baseRuleModel.and = new List<object>();
              AdvancedFilterViewModel.AddRule(list[0], ref baseRuleModel.and);
              AdvancedFilterViewModel.AddRule(list[2], ref baseRuleModel.and);
            }
            else
            {
              baseRuleModel.or = new List<object>();
              AdvancedFilterViewModel.AddRule(list[0], ref baseRuleModel.or);
              AdvancedFilterViewModel.AddRule(list[2], ref baseRuleModel.or);
            }
            if (logicType == LogicType.And)
            {
              advanced3.and = new List<object>()
              {
                (object) baseRuleModel
              };
              AdvancedFilterViewModel.AddRule(list[4], ref advanced3.and);
            }
            else
            {
              advanced3.or = new List<object>()
              {
                (object) baseRuleModel
              };
              AdvancedFilterViewModel.AddRule(list[4], ref advanced3.or);
            }
          }
          advanced3.version = val2;
          return JsonConvert.SerializeObject((object) advanced3);
        default:
          return string.Empty;
      }
    }

    private bool WithNotLogic(List<CardViewModel> valid)
    {
      if (valid.Any<CardViewModel>())
      {
        foreach (CardViewModel cardViewModel in valid)
        {
          if (cardViewModel.LogicType == LogicType.Not)
            return true;
        }
      }
      return false;
    }

    private static bool IsSameLogic(CardViewModel left, CardViewModel right)
    {
      if (left.Type == CardType.LogicAnd && right.Type == CardType.LogicAnd)
        return true;
      return left.Type == CardType.LogicOr && right.Type == CardType.LogicOr;
    }

    private static void AddRule(CardViewModel card, ref List<object> condList)
    {
      if (card.ConditionName == "priority")
      {
        Rule<int> rule = card is FilterListDisplayModel<int> listDisplayModel ? listDisplayModel.ToRule() : (Rule<int>) null;
        condList.Add((object) rule);
      }
      else
      {
        Rule<string> rule = card is FilterListDisplayModel<string> listDisplayModel ? listDisplayModel.ToRule() : (Rule<string>) null;
        condList.Add((object) rule);
      }
    }

    private static void AddRule(CardViewModel card, AdvancedRuleModel advanced, bool isAnd)
    {
      if (card.ConditionName == "priority")
      {
        Rule<int> rule = card is FilterListDisplayModel<int> listDisplayModel ? listDisplayModel.ToRule() : (Rule<int>) null;
        if (isAnd)
          advanced.and.Add((object) rule);
        else
          advanced.or.Add((object) rule);
      }
      else
      {
        Rule<string> rule = card is FilterListDisplayModel<string> listDisplayModel ? listDisplayModel.ToRule() : (Rule<string>) null;
        if (isAnd)
          advanced.and.Add((object) rule);
        else
          advanced.or.Add((object) rule);
      }
    }

    public bool ConditionValid()
    {
      List<CardViewModel> list1 = this.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>();
      List<CardViewModel> list2 = this.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.LogicAnd || card.Type == CardType.LogicOr)).ToList<CardViewModel>();
      if (list1.Count == 1 && list2.Count == 0 || list1.Count == 2 && list2.Count == 1)
        return true;
      return list1.Count == 3 && list2.Count == 2;
    }

    public bool CheckKeywordValid()
    {
      CardViewModel cardViewModel = this.CardList.FirstOrDefault<CardViewModel>((Func<CardViewModel, bool>) (card => card is KeywordsViewModel));
      return cardViewModel == null || !(cardViewModel is KeywordsViewModel keywordsViewModel) || !string.IsNullOrEmpty(keywordsViewModel.Keyword);
    }

    public bool OnlyNote()
    {
      if (this.CardList == null || !this.CardList.Any<CardViewModel>() || this.CardList.Any<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.LogicOr)))
        return false;
      foreach (CardViewModel card in this.CardList)
      {
        if (card is TaskTypeViewModel taskTypeViewModel)
          return taskTypeViewModel.IsNote;
      }
      return false;
    }
  }
}
