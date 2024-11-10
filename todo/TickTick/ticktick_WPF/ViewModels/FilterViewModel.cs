// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.FilterViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class FilterViewModel : BaseViewModel
  {
    private AdvancedFilterViewModel _advancedViewModel;
    private Constants.ModelEditType _editType;
    private FilterModel _filterModel;
    private FilterMode _mode;
    private string _name;
    private NormalFilterViewModel _normalViewModel;

    public FilterViewModel()
    {
      this._name = string.Empty;
      this._editType = Constants.ModelEditType.New;
      this._filterModel = new FilterModel();
      this._mode = FilterMode.Normal;
      this._normalViewModel = new NormalFilterViewModel();
      this._advancedViewModel = new AdvancedFilterViewModel();
    }

    public FilterViewModel(FilterModel filterModel)
    {
      this._name = filterModel.name;
      this._editType = string.IsNullOrEmpty(filterModel.id) ? Constants.ModelEditType.New : Constants.ModelEditType.Edit;
      this._filterModel = filterModel.Copy();
      this._mode = (FilterMode) Parser.GetFilterRuleType(filterModel.rule);
      if (this._mode == FilterMode.Normal)
      {
        this._normalViewModel = Parser.ToNormalModel(filterModel.rule);
        this._advancedViewModel = new AdvancedFilterViewModel();
      }
      else
      {
        this._normalViewModel = new NormalFilterViewModel();
        this._advancedViewModel = Parser.ToAdvanceModel(filterModel.rule);
      }
    }

    public FilterViewModel(SearchFilterModel searchFilter)
    {
      this._editType = Constants.ModelEditType.New;
      this._filterModel = new FilterModel();
      this._mode = FilterMode.Normal;
      this._normalViewModel = new NormalFilterViewModel(searchFilter);
      this._advancedViewModel = new AdvancedFilterViewModel();
    }

    public NormalFilterViewModel NormalViewModel
    {
      get => this._normalViewModel;
      set => this._normalViewModel = value;
    }

    public FilterMode Mode
    {
      get => this._mode;
      set
      {
        this._mode = value;
        this.OnPropertyChanged(nameof (Mode));
      }
    }

    public AdvancedFilterViewModel AdvancedViewModel
    {
      get => this._advancedViewModel;
      set => this._advancedViewModel = value;
    }

    public string Name
    {
      get => this._name;
      set
      {
        this._name = value;
        this.OnPropertyChanged(nameof (Name));
      }
    }

    public Constants.ModelEditType EditType
    {
      get => this._editType;
      set
      {
        this._editType = value;
        this.OnPropertyChanged(nameof (EditType));
      }
    }

    public FilterModel FilterModel
    {
      get => this._filterModel;
      set => this._filterModel = value;
    }

    public FilterModel GetFilterModel()
    {
      if (this.EditType == Constants.ModelEditType.New)
      {
        FilterModel filter = new FilterModel()
        {
          name = this._name,
          deleted = 0,
          syncStatus = 0,
          createdTime = DateTime.Now,
          modifiedTime = DateTime.Now,
          sortType = Constants.SortType.project.ToString()
        };
        this.SaveRule(filter);
        return filter;
      }
      this._filterModel.name = this._name;
      this.SaveRule(this._filterModel);
      return this._filterModel;
    }

    public async Task<FilterModel> Save()
    {
      if (this.EditType == Constants.ModelEditType.New)
      {
        FilterModel filterModel1 = new FilterModel();
        filterModel1.name = this._name;
        filterModel1.deleted = 0;
        filterModel1.syncStatus = 0;
        filterModel1.id = Utils.GetGuid();
        filterModel1.createdTime = DateTime.Now;
        filterModel1.modifiedTime = DateTime.Now;
        filterModel1.sortType = Constants.SortType.project.ToString();
        FilterModel filterModel2 = filterModel1;
        filterModel2.sortOrder = await FilterDao.GetNextFilterSortOrder();
        filterModel1.SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.project, false);
        this._filterModel = filterModel1;
        filterModel2 = (FilterModel) null;
        filterModel1 = (FilterModel) null;
        this.SaveRule(this._filterModel);
        await FilterDao.AddFilter(this._filterModel);
      }
      else
      {
        this._filterModel.name = this._name;
        this._filterModel.modifiedTime = DateTime.Now;
        this.SaveRule(this._filterModel);
        if (this._filterModel.syncStatus != 0)
          this._filterModel.syncStatus = 1;
        await FilterSyncJsonDao.TrySaveFilter(this._filterModel.id);
        await FilterDao.UpdateFilter(this._filterModel);
      }
      return this._filterModel;
    }

    private void SaveRule(FilterModel filter)
    {
      bool flag = false;
      if (this._mode == FilterMode.Normal)
      {
        string rule1 = this._normalViewModel.ToRule(true);
        if (string.IsNullOrEmpty(rule1))
        {
          string rule2 = this._advancedViewModel.ToRule(true);
          filter.rule = !string.IsNullOrEmpty(rule2) ? rule2 : rule1;
        }
        else
        {
          filter.rule = rule1;
          flag = true;
        }
      }
      else
        filter.rule = this._advancedViewModel.ToRule(true);
      UserActCollectUtils.AddClickEvent("edit_filter", "mode", flag ? "normal" : "advance");
      if ((flag ? (this._normalViewModel.OnlyNote() ? 1 : 0) : (this._advancedViewModel.OnlyNote() ? 1 : 0)) != 0)
      {
        if (this.EditType == Constants.ModelEditType.New || filter.SortOption == null)
        {
          filter.SortOption = new SortOption()
          {
            groupBy = "project",
            orderBy = "createdTime"
          };
        }
        else
        {
          if (filter.SortOption.groupBy == Constants.SortType.priority.ToString() || filter.SortOption.groupBy == Constants.SortType.dueDate.ToString())
            filter.SortOption.groupBy = "project";
          if (filter.SortOption.orderBy == Constants.SortType.priority.ToString() || filter.SortOption.orderBy == Constants.SortType.dueDate.ToString())
            filter.SortOption.orderBy = "createdTime";
        }
      }
      if ((flag ? (!this._normalViewModel.OnlyNote() ? 1 : 0) : (!this._advancedViewModel.OnlyNote() ? 1 : 0)) == 0)
        return;
      if (this.EditType == Constants.ModelEditType.New || filter.SortOption == null)
      {
        filter.SortOption = new SortOption()
        {
          groupBy = "project",
          orderBy = "dueDate"
        };
      }
      else
      {
        if (!(filter.SortOption.orderBy == Constants.SortType.createdTime.ToString()) && !(filter.SortOption.orderBy == Constants.SortType.modifiedTime.ToString()))
          return;
        filter.SortOption.orderBy = "dueDate";
      }
    }

    public static FilterTaskDefault CalculateTaskDefault(string filterRule, bool withEmptyProject = false)
    {
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      FilterTaskDefault filterTaskDefault1 = new FilterTaskDefault()
      {
        Priority = new int?(defaultSafely.Priority),
        DefaultDate = defaultSafely.GetDefaultDateTime()
      };
      if (Parser.GetFilterRuleType(filterRule) == 0)
      {
        FilterViewModel.GetDefaultInNormal(Parser.ToNormalModel(filterRule), filterTaskDefault1);
      }
      else
      {
        AdvancedFilterViewModel advanceModel = Parser.ToAdvanceModel(filterRule);
        List<CardViewModel> list = advanceModel.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>();
        if (list.Count > 0)
        {
          filterTaskDefault1.DefaultDate = TaskDefaultDao.GetDefaultSafely().GetDefaultDateTime();
          CardViewModel cardViewModel = list.FirstOrDefault<CardViewModel>((Func<CardViewModel, bool>) (f => f is PriorityCardViewModel));
          if (cardViewModel != null)
            filterTaskDefault1.Priority = FilterViewModel.CheckAdvancedFilterValid<PriorityCardViewModel>((IReadOnlyList<CardViewModel>) advanceModel.CardList) ? FilterViewModel.CalcDefaultPriority(cardViewModel is PriorityCardViewModel priorityCardViewModel ? (IReadOnlyCollection<int>) priorityCardViewModel.Values : (IReadOnlyCollection<int>) null) : new int?(0);
          filterTaskDefault1.ProjectModel = FilterViewModel.CalculateDefaultProject((IReadOnlyList<CardViewModel>) advanceModel.CardList);
          DateTime? defaultDate = FilterViewModel.CalculateDefaultDate((IReadOnlyList<CardViewModel>) advanceModel.CardList);
          if (defaultDate.HasValue)
            filterTaskDefault1.DefaultDate = defaultDate;
          bool flag = advanceModel.OnlyNote();
          filterTaskDefault1.OnlyNote = flag;
          FilterTaskDefault filterTaskDefault2 = filterTaskDefault1;
          int num;
          if (!flag)
          {
            ProjectModel projectModel = filterTaskDefault1.ProjectModel;
            num = projectModel != null ? (projectModel.IsNote ? 1 : 0) : 0;
          }
          else
            num = 1;
          filterTaskDefault2.IsNote = num != 0;
          filterTaskDefault1.DefaultTags = FilterViewModel.CalculateDefaultTag((IReadOnlyList<CardViewModel>) advanceModel.CardList);
        }
      }
      if (!withEmptyProject && filterTaskDefault1.ProjectModel == null)
      {
        ProjectModel projectModel = new ProjectModel()
        {
          id = TaskDefaultDao.GetDefaultSafely().ProjectId,
          name = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == TaskDefaultDao.GetDefaultSafely().ProjectId))?.name
        };
        filterTaskDefault1.ProjectModel = projectModel;
      }
      return filterTaskDefault1;
    }

    public static void GetDefaultInNormal(
      NormalFilterViewModel normal,
      FilterTaskDefault filterTaskDefault)
    {
      filterTaskDefault.Priority = FilterViewModel.CalcDefaultPriority((IReadOnlyCollection<int>) normal.Priorities);
      filterTaskDefault.DefaultTags = FilterViewModel.CalcDefaultTags(normal.Tags, LogicType.Or);
      DateTime? nullable = FilterViewModel.CalcDefaultDate((ICollection<string>) normal.DueDates);
      if (nullable.HasValue)
        filterTaskDefault.DefaultDate = nullable;
      filterTaskDefault.ProjectModel = FilterViewModel.CalcDefaultProject(normal.Projects, normal.Groups);
      filterTaskDefault.OnlyNote = normal.OnlyNote();
      FilterTaskDefault filterTaskDefault1 = filterTaskDefault;
      int num;
      if (!filterTaskDefault.OnlyNote)
      {
        ProjectModel projectModel = filterTaskDefault.ProjectModel;
        num = projectModel != null ? (projectModel.IsNote ? 1 : 0) : 0;
      }
      else
        num = 1;
      filterTaskDefault1.IsNote = num != 0;
    }

    private static ProjectModel CalculateDefaultProject(IReadOnlyList<CardViewModel> cards)
    {
      if (FilterViewModel.CheckAdvancedFilterValid<ProjectOrGroupCardViewModel>(cards) && cards != null && cards.Any<CardViewModel>())
      {
        foreach (CardViewModel card in (IEnumerable<CardViewModel>) cards)
        {
          if (card is ProjectOrGroupCardViewModel groupCardViewModel)
          {
            ProjectModel defaultProject = FilterViewModel.CalcDefaultProject(groupCardViewModel.Values, groupCardViewModel.GroupIds, groupCardViewModel.LogicType, false);
            if (defaultProject != null)
              return defaultProject;
          }
        }
      }
      return CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == TaskDefaultDao.GetDefaultSafely().ProjectId));
    }

    private static bool CheckAdvancedFilterValid<T>(IReadOnlyList<CardViewModel> cards)
    {
      if (cards.Count<CardViewModel>((Func<CardViewModel, bool>) (card => card is T)) > 1)
      {
        for (int index = 2; index < cards.Count; ++index)
        {
          if (cards[index] is T)
          {
            CardViewModel card = cards[index - 1];
            if (card != null && card.Type == CardType.LogicAnd)
              return false;
          }
        }
      }
      return true;
    }

    private static DateTime? CalculateDefaultDate(IReadOnlyList<CardViewModel> cards)
    {
      if (FilterViewModel.CheckAdvancedFilterValid<DateCardViewModel>(cards) && cards != null && cards.Any<CardViewModel>())
      {
        foreach (CardViewModel card in (IEnumerable<CardViewModel>) cards)
        {
          if (card is DateCardViewModel dateCardViewModel)
          {
            DateTime? defaultDate = FilterViewModel.CalcDefaultDate((ICollection<string>) dateCardViewModel.Values, dateCardViewModel.LogicType);
            if (defaultDate.HasValue)
              return defaultDate;
          }
        }
      }
      return new DateTime?();
    }

    private static List<string> CalculateDefaultTag(IReadOnlyList<CardViewModel> cards)
    {
      List<string> source = new List<string>();
      if (FilterViewModel.IsWithAndWithoutTags(cards))
        return new List<string>()
        {
          TagDataHelper.GetFirstValidTag()
        };
      for (int index = 0; index < cards.Count; ++index)
      {
        if (cards[index] is TagCardViewModel card1)
        {
          if (!source.Any<string>())
            source.AddRange((IEnumerable<string>) FilterViewModel.CalcDefaultTags(card1.Values, card1.LogicType));
          else if (index > 0)
          {
            CardViewModel card = cards[index - 1];
            if (card != null && card.Type == CardType.LogicAnd)
              source.AddRange((IEnumerable<string>) FilterViewModel.CalcDefaultTags(card1.Values, card1.LogicType));
          }
        }
      }
      return source.Distinct<string>().ToList<string>();
    }

    private static bool IsWithAndWithoutTags(IReadOnlyList<CardViewModel> cards)
    {
      List<string> stringList = new List<string>();
      for (int index = 0; index < cards.Count; ++index)
      {
        if (index != 0)
        {
          if (index > 0)
          {
            CardViewModel card = cards[index - 1];
            if (card == null || card.Type != CardType.LogicAnd)
              continue;
          }
          else
            continue;
        }
        if (cards[index] is TagCardViewModel card1)
          stringList.AddRange((IEnumerable<string>) card1.Values);
      }
      return stringList.Contains("*withtags") && stringList.Contains("!tag");
    }

    private static ProjectModel CalcDefaultProject(
      List<string> projectIds,
      List<string> groupIds,
      LogicType logicType = LogicType.Or,
      bool withDefault = true)
    {
      List<ProjectModel> projects = CacheManager.GetProjects();
      ProjectModel projectModel = projects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == TaskDefaultDao.GetDefaultSafely().ProjectId));
      if (!Parser.CheckProjectOrGroupValid(projectIds, groupIds) || projectIds.Contains(projectModel?.id) && logicType != LogicType.Not)
        return projectModel;
      List<ProjectModel> list1 = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (project => !project.Ishide && !project.delete_status && project.IsEnable())).ToList<ProjectModel>();
      if (logicType == LogicType.Not)
        return !string.IsNullOrEmpty(projectModel?.id) && !projectIds.Contains(projectModel.id) ? projectModel : list1.Where<ProjectModel>((Func<ProjectModel, bool>) (project => !projectIds.Contains(project.id))).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).ToList<ProjectModel>().FirstOrDefault<ProjectModel>();
      List<ProjectModel> list2 = list1.Where<ProjectModel>((Func<ProjectModel, bool>) (project => projectIds.Contains(project.id) || groupIds.Contains(project.groupId))).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).ToList<ProjectModel>();
      if (list2.Count > 0)
        return list2[0];
      return !withDefault ? (ProjectModel) null : projectModel;
    }

    private static int? CalcDefaultPriority(IReadOnlyCollection<int> priorities)
    {
      return priorities == null || priorities.Count == 0 ? new int?() : new int?(priorities.Max());
    }

    private static string GetSelectedTag(List<string> tags)
    {
      string str = tags.Where<string>((Func<string, bool>) (t => t != "!tag")).ToList<string>().FirstOrDefault<string>();
      if (str == null)
        return string.Empty;
      return !tags.Contains("*withtags") ? str : TagDataHelper.GetFirstValidTag();
    }

    private static List<string> CalcDefaultTags(List<string> tags, LogicType logic)
    {
      if (tags == null || tags.Count == 0)
        return TaskDefaultDao.GetDefaultSafely().Tags;
      List<string> source = new List<string>();
      if (logic == LogicType.Not)
      {
        if (tags.Count == 1 && tags[0] == "!tag")
        {
          source.Add(TagDataHelper.GetFirstValidTag());
        }
        else
        {
          if (tags.Count <= 1 || !tags.Contains("!tag"))
            return new List<string>();
          string str = TagDataHelper.GetTags().Select<TagModel, string>((Func<TagModel, string>) (item => item.name)).FirstOrDefault<string>((Func<string, bool>) (tag => !tags.Contains(tag)));
          if (str == null)
            return new List<string>();
          return new List<string>() { str };
        }
      }
      if (logic == LogicType.Or)
      {
        if (tags.Count == 1 && tags[0] == "!tag")
          return new List<string>();
        source.Add(FilterViewModel.GetSelectedTag(tags));
      }
      else
      {
        if (tags.Any<string>() && tags.Contains("!tag"))
          return new List<string>();
        foreach (string tag in tags)
          source.Add(tag == "*withtags" ? TagDataHelper.GetFirstValidTag() : tag);
        source = source.Distinct<string>().ToList<string>();
      }
      return source.Count <= 0 ? TaskDefaultDao.GetDefaultSafely().Tags : source.ToList<string>();
    }

    private static DateTime? CalcDefaultDate(ICollection<string> selectedDates, LogicType logic = LogicType.Or)
    {
      if (logic == LogicType.Not)
      {
        selectedDates.Remove("nodue");
        selectedDates.Remove("recurring");
        return selectedDates.Count == 0 ? new DateTime?(DateTime.Today) : new DateTime?();
      }
      List<DefaultDateModel> list1 = DefaultDateModel.GetDefaultDateMap().Where<DefaultDateModel>((Func<DefaultDateModel, bool>) (date => selectedDates.Contains(date.Key))).ToList<DefaultDateModel>();
      if (list1.Any<DefaultDateModel>((Func<DefaultDateModel, bool>) (d => d.DefaultDate.HasValue && d.DefaultDate.Value == DateTime.Today)))
        return new DateTime?(DateTime.Today);
      if (selectedDates.Contains("nextweek"))
      {
        int nextWeekDayDiff = DateUtils.GetNextWeekDayDiff();
        list1.Add(new DefaultDateModel()
        {
          Key = "nextweek",
          Priority = nextWeekDayDiff,
          DefaultDate = new DateTime?(DateTime.Today.AddDays((double) nextWeekDayDiff))
        });
      }
      if (selectedDates.FirstOrDefault<string>((Func<string, bool>) (date => date.EndsWith("days"))) != null)
        return new DateTime?(DateTime.Today);
      foreach (string str in selectedDates.Where<string>((Func<string, bool>) (date => date.StartsWith("offset"))).ToList<string>().Where<string>((Func<string, bool>) (offset => offset.Length > 8)).Select<string, string>((Func<string, string>) (offset => offset.Substring(7))).Select<string, string>((Func<string, string>) (val => val.Substring(0, val.Length - 1))))
      {
        int result;
        if (int.TryParse(str.Substring(0, str.Length - 1), out result))
        {
          DateTime dateTime;
          switch (str.Substring(str.Length - 1))
          {
            case "W":
              result = Utils.GetWeekStartDiff(DateTime.Today) + result * 7;
              break;
            case "M":
              dateTime = DateTime.Today;
              dateTime = dateTime.AddDays((double) (1 - DateTime.Today.Day));
              result = (int) (dateTime.AddMonths(result) - DateTime.Today).TotalDays;
              break;
          }
          List<DefaultDateModel> defaultDateModelList = list1;
          DefaultDateModel defaultDateModel = new DefaultDateModel();
          defaultDateModel.Key = string.Format("{0}dayslater", (object) 7);
          defaultDateModel.Priority = result;
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          defaultDateModel.DefaultDate = new DateTime?(dateTime.AddDays((double) result));
          defaultDateModelList.Add(defaultDateModel);
        }
      }
      string str1 = selectedDates.FirstOrDefault<string>((Func<string, bool>) (date => date.EndsWith("dayslater")));
      DateTime dateTime1;
      if (str1 != null)
      {
        int result;
        int.TryParse(str1.Replace("dayslater", string.Empty), out result);
        List<DefaultDateModel> defaultDateModelList = list1;
        DefaultDateModel defaultDateModel = new DefaultDateModel();
        defaultDateModel.Key = string.Format("{0}dayslater", (object) 7);
        defaultDateModel.Priority = result;
        dateTime1 = DateTime.Now;
        dateTime1 = dateTime1.Date;
        defaultDateModel.DefaultDate = new DateTime?(dateTime1.AddDays((double) result));
        defaultDateModelList.Add(defaultDateModel);
      }
      List<string> list2 = selectedDates.Where<string>((Func<string, bool>) (date => date.EndsWith("daysfromtoday"))).ToList<string>();
      if (list2.Count > 0)
      {
        foreach (string str2 in list2)
        {
          int result;
          if (int.TryParse(str2.Replace("daysfromtoday", string.Empty), out result))
          {
            if (result == 0)
              return new DateTime?(DateTime.Today);
            List<DefaultDateModel> defaultDateModelList = list1;
            DefaultDateModel defaultDateModel = new DefaultDateModel();
            defaultDateModel.Key = 7.ToString() + "daysfromtoday";
            defaultDateModel.Priority = result;
            dateTime1 = DateTime.Now;
            dateTime1 = dateTime1.Date;
            defaultDateModel.DefaultDate = new DateTime?(dateTime1.AddDays((double) result));
            defaultDateModelList.Add(defaultDateModel);
          }
        }
      }
      string rule = selectedDates.FirstOrDefault<string>((Func<string, bool>) (d => d.StartsWith("span")));
      if (rule != null)
      {
        (int? nullable1, int? nullable2) = FilterUtils.GetSpanPairInRule(rule);
        if (!nullable1.HasValue && !nullable2.HasValue)
          return new DateTime?(DateTime.Today);
        int num = nullable1.HasValue ? (nullable2.HasValue ? (nullable2.Value < 0 ? nullable2.Value : (nullable1.Value > 0 ? nullable1.Value : 0)) : (nullable1.Value <= 0 ? 0 : nullable1.Value)) : (nullable2.Value >= 0 ? 0 : nullable2.Value);
        if (num == 0)
          return new DateTime?(DateTime.Today);
        List<DefaultDateModel> defaultDateModelList = list1;
        DefaultDateModel defaultDateModel = new DefaultDateModel();
        defaultDateModel.Key = rule;
        defaultDateModel.Priority = num;
        dateTime1 = DateTime.Now;
        dateTime1 = dateTime1.Date;
        defaultDateModel.DefaultDate = new DateTime?(dateTime1.AddDays((double) num));
        defaultDateModelList.Add(defaultDateModel);
      }
      List<DefaultDateModel> list3 = list1.OrderBy<DefaultDateModel, int>((Func<DefaultDateModel, int>) (date => date.Priority)).ToList<DefaultDateModel>();
      DefaultDateModel defaultDateModel1 = list3.FirstOrDefault<DefaultDateModel>((Func<DefaultDateModel, bool>) (d => d.Priority >= 0));
      if (defaultDateModel1 != null && defaultDateModel1.DefaultDate.HasValue)
        return defaultDateModel1.DefaultDate;
      if (defaultDateModel1 == null || defaultDateModel1.Key == "nodue")
      {
        DefaultDateModel defaultDateModel2 = list3.LastOrDefault<DefaultDateModel>((Func<DefaultDateModel, bool>) (d => d.Priority < 0));
        if (defaultDateModel2 != null)
          return defaultDateModel2.DefaultDate;
      }
      return defaultDateModel1 != null ? defaultDateModel1.DefaultDate : TaskDefaultDao.GetDefaultSafely().GetDefaultDateTime();
    }
  }
}
