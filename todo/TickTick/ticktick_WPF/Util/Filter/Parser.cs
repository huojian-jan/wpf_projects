// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Filter.Parser
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Util.Filter
{
  public static class Parser
  {
    public static int GetFilterRuleType(string rule)
    {
      if (string.IsNullOrEmpty(rule))
        return 0;
      try
      {
        return JsonConvert.DeserializeObject<RuleModel>(rule).type;
      }
      catch (Exception ex)
      {
        return 0;
      }
    }

    public static AdvancedFilterViewModel ToAdvanceModel(string rule)
    {
      AdvancedFilterViewModel advanceModel = new AdvancedFilterViewModel();
      if (string.IsNullOrEmpty(rule))
      {
        advanceModel.CardList = FilterConditionProvider.BuildInitData();
      }
      else
      {
        List<CardViewModel> cardViewModelList = new List<CardViewModel>();
        List<CardViewModel> logicList = new List<CardViewModel>();
        AdvancedRuleModel advancedRuleModel;
        try
        {
          advancedRuleModel = JsonConvert.DeserializeObject<AdvancedRuleModel>(rule);
        }
        catch (Exception ex)
        {
          advancedRuleModel = new AdvancedRuleModel();
        }
        if (advancedRuleModel == null)
          advancedRuleModel = new AdvancedRuleModel();
        if (advancedRuleModel.and != null)
        {
          logicList.Add(new CardViewModel()
          {
            Type = CardType.LogicAnd
          });
          Parser.ParseModelList(advancedRuleModel.and, cardViewModelList, logicList, advancedRuleModel.version);
        }
        else if (advancedRuleModel.or != null)
        {
          logicList.Add(new CardViewModel()
          {
            Type = CardType.LogicOr
          });
          Parser.ParseModelList(advancedRuleModel.or, cardViewModelList, logicList, advancedRuleModel.version);
        }
        advanceModel.Version = advancedRuleModel.version;
        switch (cardViewModelList.Count)
        {
          case 1:
            advanceModel.CardList.AddRange((IEnumerable<CardViewModel>) cardViewModelList);
            advanceModel.CardList.Add(new CardViewModel()
            {
              Type = CardType.InitLogic
            });
            advanceModel.CardList.Add(new CardViewModel()
            {
              Type = CardType.InitFilter
            });
            advanceModel.CardList.Add(new CardViewModel()
            {
              Type = CardType.AddMore
            });
            break;
          case 2:
            advanceModel.CardList.Add(cardViewModelList[0]);
            advanceModel.CardList.Add(logicList[0]);
            advanceModel.CardList.Add(cardViewModelList[1]);
            advanceModel.CardList.Add(new CardViewModel()
            {
              Type = CardType.AddMore
            });
            break;
          case 3:
            CardViewModel cardViewModel1 = new CardViewModel()
            {
              Type = logicList[0].Type
            };
            CardViewModel cardViewModel2 = new CardViewModel()
            {
              Type = logicList[0].Type
            };
            if (logicList.Count > 1)
              cardViewModel1 = new CardViewModel()
              {
                Type = logicList[1].Type
              };
            advanceModel.CardList.Add(cardViewModelList[0]);
            advanceModel.CardList.Add(cardViewModel1);
            advanceModel.CardList.Add(cardViewModelList[1]);
            advanceModel.CardList.Add(cardViewModel2);
            advanceModel.CardList.Add(cardViewModelList[2]);
            break;
        }
      }
      return advanceModel;
    }

    private static void ParseModelList(
      List<object> modelList,
      List<CardViewModel> conditionList,
      List<CardViewModel> logicList,
      int version)
    {
      if (modelList == null)
        return;
      foreach (object model in modelList)
      {
        CardViewModel cardViewModel = (CardViewModel) null;
        ListRuleModel listRuleModel1;
        try
        {
          listRuleModel1 = JsonConvert.DeserializeObject<ListRuleModel>(model.ToString());
        }
        catch (Exception ex)
        {
          listRuleModel1 = new ListRuleModel();
        }
        if (listRuleModel1.conditionName == null)
        {
          if (listRuleModel1.or != null)
          {
            logicList.Add(new CardViewModel()
            {
              Type = CardType.LogicOr
            });
            Parser.ParseModelList(listRuleModel1.or, conditionList, logicList, version);
          }
          else
          {
            logicList.Add(new CardViewModel()
            {
              Type = CardType.LogicAnd
            });
            Parser.ParseModelList(listRuleModel1.and, conditionList, logicList, version);
          }
        }
        else
        {
          LogicType logicType = LogicType.Or;
          if (listRuleModel1.or != null)
            logicType = LogicType.Or;
          else if (listRuleModel1.and != null)
            logicType = LogicType.And;
          else if (listRuleModel1.not != null)
            logicType = LogicType.Not;
          List<object> values = (listRuleModel1.or ?? listRuleModel1.and) ?? listRuleModel1.not;
          string conditionName = listRuleModel1.conditionName;
          if (conditionName != null)
          {
            switch (conditionName.Length)
            {
              case 3:
                if (conditionName == "tag")
                {
                  TagCardViewModel tagCardViewModel1 = new TagCardViewModel();
                  tagCardViewModel1.LogicType = logicType;
                  TagCardViewModel tagCardViewModel2 = tagCardViewModel1;
                  List<string> stringList1 = new List<string>();
                  List<string> stringList2 = Parser.GetStringList(values);
                  List<TagModel> tags = CacheManager.GetTags();
                  foreach (string str in stringList2)
                  {
                    if (!stringList1.Contains(str))
                      stringList1.Add(str);
                    string parent = str;
                    IEnumerable<TagModel> source = tags.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == parent));
                    List<string> list = source != null ? source.Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>() : (List<string>) null;
                    stringList1.AddRange((IEnumerable<string>) list);
                  }
                  tagCardViewModel2.Values = stringList1;
                  cardViewModel = (CardViewModel) tagCardViewModel2;
                  break;
                }
                break;
              case 7:
                if (conditionName == "dueDate")
                {
                  DateCardViewModel dateCardViewModel = new DateCardViewModel();
                  dateCardViewModel.LogicType = logicType;
                  dateCardViewModel.Values = Parser.GetStringList(values);
                  dateCardViewModel.Version = version;
                  cardViewModel = (CardViewModel) dateCardViewModel;
                  break;
                }
                break;
              case 8:
                switch (conditionName[0])
                {
                  case 'a':
                    if (conditionName == "assignee")
                    {
                      AssigneeCardViewModel assigneeCardViewModel = new AssigneeCardViewModel();
                      assigneeCardViewModel.LogicType = logicType;
                      assigneeCardViewModel.Values = Parser.GetStringList(values);
                      assigneeCardViewModel.Version = version;
                      cardViewModel = (CardViewModel) assigneeCardViewModel;
                      break;
                    }
                    break;
                  case 'k':
                    if (conditionName == "keywords")
                    {
                      List<string> stringList = Parser.GetStringList(values);
                      KeywordsViewModel keywordsViewModel = new KeywordsViewModel();
                      keywordsViewModel.LogicType = logicType;
                      // ISSUE: explicit non-virtual call
                      keywordsViewModel.Keyword = stringList == null || __nonvirtual (stringList.Count) <= 0 ? string.Empty : stringList[0];
                      cardViewModel = (CardViewModel) keywordsViewModel;
                      break;
                    }
                    break;
                  case 'p':
                    if (conditionName == "priority")
                    {
                      PriorityCardViewModel priorityCardViewModel = new PriorityCardViewModel();
                      priorityCardViewModel.LogicType = logicType;
                      priorityCardViewModel.Values = Parser.GetIntList(values);
                      cardViewModel = (CardViewModel) priorityCardViewModel;
                      break;
                    }
                    break;
                  case 't':
                    if (conditionName == "taskType")
                    {
                      TaskTypeViewModel taskTypeViewModel = new TaskTypeViewModel();
                      taskTypeViewModel.LogicType = logicType;
                      taskTypeViewModel.Values = Parser.GetStringList(values);
                      cardViewModel = (CardViewModel) taskTypeViewModel;
                      break;
                    }
                    break;
                }
                break;
              case 11:
                if (conditionName == "listOrGroup")
                {
                  cardViewModel = (CardViewModel) new ProjectOrGroupCardViewModel();
                  cardViewModel.LogicType = logicType;
                  if (values != null && values.Count > 0)
                  {
                    using (List<object>.Enumerator enumerator = values.GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        object current = enumerator.Current;
                        ListRuleModel listRuleModel2;
                        try
                        {
                          listRuleModel2 = JsonConvert.DeserializeObject<ListRuleModel>(current.ToString());
                        }
                        catch (Exception ex)
                        {
                          listRuleModel2 = new ListRuleModel();
                        }
                        switch (listRuleModel2.conditionName)
                        {
                          case "list":
                            if (listRuleModel2.or != null && listRuleModel2.or.Any<object>())
                            {
                              ((ProjectOrGroupCardViewModel) cardViewModel).Values = Parser.GetStringList(listRuleModel2.or);
                              continue;
                            }
                            continue;
                          case "group":
                            if (listRuleModel2.or != null && listRuleModel2.or.Any<object>())
                            {
                              ((ProjectOrGroupCardViewModel) cardViewModel).GroupIds = Parser.GetStringList(listRuleModel2.or);
                              continue;
                            }
                            continue;
                          default:
                            continue;
                        }
                      }
                      break;
                    }
                  }
                  else
                    break;
                }
                else
                  break;
            }
          }
          if (cardViewModel != null)
            conditionList.Add(cardViewModel);
        }
      }
    }

    private static List<string> GetStringList(List<object> values)
    {
      List<string> stringList = new List<string>();
      if (values != null && values.Any<object>())
      {
        foreach (object obj in values)
          stringList.Add(obj.ToString());
      }
      return stringList;
    }

    private static List<int> GetIntList(List<object> values)
    {
      List<int> intList = new List<int>();
      if (values != null && values.Any<object>())
      {
        foreach (object obj in values)
          intList.Add(int.Parse(obj.ToString()));
      }
      return intList;
    }

    public static NormalFilterViewModel ToNormalModel(string rule)
    {
      if (string.IsNullOrEmpty(rule))
        return new NormalFilterViewModel();
      if (!rule.StartsWith("{") && !rule.EndsWith("}"))
        return Parser.ParseLegacyRule(rule);
      NormalFilterViewModel normalModel = new NormalFilterViewModel();
      NormalRuleModel normalRuleModel;
      try
      {
        normalRuleModel = JsonConvert.DeserializeObject<NormalRuleModel>(rule);
      }
      catch (Exception ex)
      {
        normalRuleModel = new NormalRuleModel();
      }
      if (normalRuleModel.and == null || normalRuleModel.and.Count == 0)
        return normalModel;
      normalModel.Version = normalRuleModel.version;
      foreach (object obj1 in normalRuleModel.and)
      {
        if (obj1 != null)
        {
          string rule1 = obj1.ToString();
          if (rule1.Contains("keywords") || !rule1.Contains("priority") && !rule1.Contains("listOrGroup"))
          {
            StringRule ruleModelSafely = Parser.GetRuleModelSafely<StringRule>(rule1);
            switch (ruleModelSafely.conditionName)
            {
              case "assignee":
                normalModel.Assignees = ruleModelSafely.or;
                break;
              case "tag":
                normalModel.Tags = ruleModelSafely.or;
                break;
              case "dueDate":
                normalModel.DueDates = ruleModelSafely.or;
                break;
              case "keywords":
                normalModel.Keywords = ruleModelSafely.or;
                break;
              case "taskType":
                normalModel.TaskTypes = ruleModelSafely.or;
                break;
              case "status":
                normalModel.Status = ruleModelSafely.or;
                break;
            }
          }
          if (rule1.Contains("priority"))
          {
            IntRule ruleModelSafely = Parser.GetRuleModelSafely<IntRule>(rule1);
            normalModel.Priorities = ruleModelSafely.or;
          }
          if (rule1.Contains("date"))
          {
            ListOrGroupRule ruleModelSafely1 = Parser.GetRuleModelSafely<ListOrGroupRule>(rule1);
            if (ruleModelSafely1.or != null && ruleModelSafely1.or.Count != 0)
            {
              foreach (object obj2 in ruleModelSafely1.or)
              {
                if (obj2.ToString().Contains("dueDate"))
                {
                  StringRule ruleModelSafely2 = Parser.GetRuleModelSafely<StringRule>(obj2.ToString());
                  normalModel.DueDates = ruleModelSafely2.or;
                }
                else if (obj2.ToString().Contains("completedTime"))
                {
                  StringRule ruleModelSafely3 = Parser.GetRuleModelSafely<StringRule>(obj2.ToString());
                  normalModel.CompletedTimes = ruleModelSafely3.or;
                }
              }
            }
            else
              continue;
          }
          if (rule1.Contains("listOrGroup"))
          {
            ListOrGroupRule ruleModelSafely4 = Parser.GetRuleModelSafely<ListOrGroupRule>(rule1);
            if (ruleModelSafely4.or != null && ruleModelSafely4.or.Count != 0)
            {
              foreach (object obj3 in ruleModelSafely4.or)
              {
                if (obj3.ToString().Contains("list"))
                {
                  StringRule ruleModelSafely5 = Parser.GetRuleModelSafely<StringRule>(obj3.ToString());
                  normalModel.Projects = ruleModelSafely5.or;
                }
                else if (obj3.ToString().Contains("group"))
                {
                  StringRule ruleModelSafely6 = Parser.GetRuleModelSafely<StringRule>(obj3.ToString());
                  normalModel.Groups = ruleModelSafely6.or;
                }
              }
            }
          }
        }
      }
      return normalModel;
    }

    private static NormalFilterViewModel ParseLegacyRule(string rule)
    {
      NormalFilterViewModel legacyRule = new NormalFilterViewModel();
      string[] strArray = Regex.Split(rule, "AND");
      if (strArray.Length == 0)
        return legacyRule;
      foreach (string str in strArray)
      {
        string filter = str.Trim();
        if (filter.StartsWith("tag"))
          legacyRule.Tags = Parser.ParseItems(filter, "tag");
        else if (filter.StartsWith("dueDate"))
          legacyRule.DueDates = Parser.ParseItems(filter, "dueDate");
        else if (filter.StartsWith("assignee"))
          legacyRule.Assignees = Parser.ParseItems(filter, "assignee");
        else if (filter.StartsWith("list"))
          legacyRule.Projects = Parser.ParseItems(filter, "list");
        else if (filter.StartsWith("group"))
          legacyRule.Groups = Parser.ParseItems(filter, "group");
        else if (filter.StartsWith("priority"))
          legacyRule.Priorities = Parser.ParsePriorities(filter);
      }
      return legacyRule;
    }

    private static List<int> ParsePriorities(string filter)
    {
      try
      {
        return ((IEnumerable<string>) filter.Substring("priority".Length + 1).Split(',')).Select<string, int>(new Func<string, int>(int.Parse)).ToList<int>();
      }
      catch
      {
        return new List<int>();
      }
    }

    private static List<string> ParseItems(string filter, string category)
    {
      if (category.Length + 1 >= filter.Length)
        return new List<string>();
      return ((IEnumerable<string>) filter.Substring(category.Length + 1).Split(',')).Select<string, string>((Func<string, string>) (item => item.Trim())).ToList<string>();
    }

    private static T GetRuleModelSafely<T>(string rule) where T : new()
    {
      try
      {
        return JsonConvert.DeserializeObject<T>(rule);
      }
      catch
      {
        return new T();
      }
    }

    public static bool CheckFilterRuleExpired(string rule)
    {
      if (Parser.GetFilterRuleType(rule) == 0)
      {
        NormalFilterViewModel normalModel = Parser.ToNormalModel(rule);
        if (normalModel.Projects.Count > 0 || normalModel.Groups.Count > 0)
          return !Parser.CheckProjectOrGroupValid(normalModel.Projects, normalModel.Groups);
        return normalModel.Tags.Count > 0 && !Parser.CheckTagValid((IEnumerable<string>) normalModel.Tags);
      }
      AdvancedFilterViewModel advanceModel = Parser.ToAdvanceModel(rule);
      CardViewModel cardViewModel = advanceModel != null ? advanceModel.CardList.FirstOrDefault<CardViewModel>((Func<CardViewModel, bool>) (card => card.ConditionName == "tag")) : (CardViewModel) null;
      if (cardViewModel != null && !Parser.CheckTagValid((IEnumerable<string>) ((TagCardViewModel) cardViewModel).Values))
        return true;
      return (advanceModel != null ? advanceModel.CardList.FirstOrDefault<CardViewModel>((Func<CardViewModel, bool>) (card => card.ConditionName == "listOrGroup")) : (CardViewModel) null) is ProjectOrGroupCardViewModel groupCardViewModel && !Parser.CheckProjectOrGroupValid(groupCardViewModel.Values, groupCardViewModel.GroupIds);
    }

    public static bool CheckProjectOrGroupValid(List<string> projectIds, List<string> groupIds)
    {
      return projectIds.Count <= 0 && groupIds.Count <= 0 || projectIds.Contains("Calendar5959a2259161d16d23a4f272") || CacheManager.GetProjects().Count<ProjectModel>((Func<ProjectModel, bool>) (p => (projectIds.Contains(p.id) || groupIds.Contains(p.groupId)) && p.IsValid())) != 0;
    }

    private static bool CheckTagValid(IEnumerable<string> tags)
    {
      List<string> cachedTags = TagDataHelper.GetTags().Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>();
      cachedTags.Add("*withtags");
      cachedTags.Add("!tag");
      cachedTags.Add("*tag");
      return tags.All<string>((Func<string, bool>) (tag => cachedTags.Contains(tag)));
    }

    public static int GetFilterRuleVersion(string rule)
    {
      if (string.IsNullOrEmpty(rule))
        return 1;
      try
      {
        RuleModel ruleModel = JsonConvert.DeserializeObject<RuleModel>(rule);
        return ruleModel != null ? ruleModel.version : 1;
      }
      catch (Exception ex)
      {
        return 1;
      }
    }

    public static bool ContainsDate(string rule, bool onlyNormal = false)
    {
      if (!onlyNormal && Parser.GetFilterRuleType(rule) != 0)
        return Parser.ToAdvanceModel(rule).CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>().Any<CardViewModel>((Func<CardViewModel, bool>) (c => c is DateCardViewModel dateCardViewModel && dateCardViewModel.Values != null && dateCardViewModel.Values.Any<string>((Func<string, bool>) (v => v != "nodue" && v != "recurring"))));
      NormalFilterViewModel normalModel = Parser.ToNormalModel(rule);
      return normalModel.DueDates != null && normalModel.DueDates.Any<string>((Func<string, bool>) (d => d != "nodue" && d != "recurring"));
    }
  }
}
