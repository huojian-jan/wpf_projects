// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.SummaryFilterViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Filter;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Views.Summary
{
  public class SummaryFilterViewModel : BaseViewModel
  {
    private string _selectedTemplateId;
    private bool _showTemplate;
    private List<string> _moreConditions;
    private NormalFilterViewModel _filter;
    private DateFilter _dateFilter;
    private List<string> _selectedTags;
    private List<string> _selectedProjectGroupIds;
    private List<string> _selectedProjectIds;
    private List<string> _selectedPriorities;
    private List<string> _selectedStatus;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private string _selectedProjectDisplayText;
    private string _selectedTagDisplayText;
    private string _selectedAssignTypeText;
    private string _selectedPriorityDisplayText;
    private string _selectedStatusTypeText;
    private SummarySortType _sortBy;
    private bool _showCompleteDate;
    private bool _showDetail;
    private bool _showPomo;
    private bool _showProgress;
    private bool _showBelongList;

    public string Name => this.GetSelectedTemplateName();

    public string SelectedTemplateId
    {
      get => this._selectedTemplateId;
      set
      {
        this._selectedTemplateId = value;
        this.OnPropertyChanged(nameof (SelectedTemplateId));
        this.OnPropertyChanged("Name");
      }
    }

    public bool ShowTemplate
    {
      get => this._showTemplate;
      set
      {
        this._showTemplate = value;
        this.OnPropertyChanged(nameof (ShowTemplate));
      }
    }

    public bool ShowMoreConditions => this._moreConditions.Count > 0;

    public List<string> MoreConditions
    {
      get => this._moreConditions;
      set
      {
        this._moreConditions = value;
        this.OnPropertyChanged(nameof (MoreConditions));
        this.OnPropertyChanged("ShowMoreConditions");
        this.OnPropertyChanged("ShowPriority");
        this.OnPropertyChanged("ShowAssignee");
      }
    }

    private string GetSelectedTemplateName()
    {
      return this.GetSelectedTemplate() == null ? Utils.GetString("Summary") : this.GetSelectedTemplate().name;
    }

    public NormalFilterViewModel Filter
    {
      get => this._filter;
      set
      {
        this._filter = value;
        this.OnPropertyChanged(nameof (Filter));
      }
    }

    private static SummarySortType TryParseSortType(string sortName)
    {
      SummarySortType result;
      return !Enum.TryParse<SummarySortType>(sortName, true, out result) ? SummarySortType.progress : result;
    }

    public async void SaveTemplate(SummaryTemplateViewModel model, Action<SummaryTemplate> handler)
    {
      handler(this.GetSelectedTemplate(model.Id));
      ticktick_WPF.Models.SummaryTemplates summaryTemplates = LocalSettings.Settings.UserPreference.summaryTemplates;
      SummaryTemplate summaryTemplate1;
      if (summaryTemplates == null)
      {
        summaryTemplate1 = (SummaryTemplate) null;
      }
      else
      {
        List<SummaryTemplate> templates = summaryTemplates.templates;
        summaryTemplate1 = templates != null ? templates.FirstOrDefault<SummaryTemplate>((Func<SummaryTemplate, bool>) (it => it.id == model.Id)) : (SummaryTemplate) null;
      }
      SummaryTemplate summaryTemplate2 = summaryTemplate1;
      if (summaryTemplate2 == null)
        return;
      handler(summaryTemplate2);
      summaryTemplates.mtime = Utils.GetNowTimeStampInMills();
      await SettingsHelper.PushLocalPreference();
    }

    public async void AddTemplate(string name, SummaryFilterViewModel viewModel)
    {
      ticktick_WPF.Models.SummaryTemplates summaryTemplates = LocalSettings.Settings.UserPreference.summaryTemplates;
      if (summaryTemplates == null)
      {
        LocalSettings.Settings.UserPreference.summaryTemplates = new ticktick_WPF.Models.SummaryTemplates();
        summaryTemplates = LocalSettings.Settings.UserPreference.summaryTemplates;
      }
      long num = 0;
      if (summaryTemplates.templates != null && summaryTemplates.templates.Count > 0)
        num = summaryTemplates.templates.Max<SummaryTemplate>((Func<SummaryTemplate, long>) (it => it.sortOrder)) + 1L;
      NormalFilterViewModel normalFilterViewModel = new NormalFilterViewModel()
      {
        Projects = viewModel.SelectedProjectIds,
        Groups = viewModel.SelectedProjectGroupIds,
        Tags = viewModel.SelectedTags,
        Priorities = viewModel.SelectedPriorities.Select<string, int>(new Func<string, int>(int.Parse)).ToList<int>(),
        Status = viewModel.SelectedStatus,
        Assignees = viewModel.Assignees,
        DueDates = new List<string>()
        {
          SummaryFilterViewModel.Filter2Rule(this.DateFilter, this.StartDate, this.EndDate)
        },
        CompletedTimes = new List<string>()
        {
          SummaryFilterViewModel.Filter2Rule(this.DateFilter, this.StartDate, this.EndDate)
        },
        Version = 6
      };
      SummaryTemplate summaryTemplate = new SummaryTemplate()
      {
        id = Utils.GetGuid(),
        name = name,
        rule = normalFilterViewModel.ToRule(false),
        sortType = viewModel.SortBy.ToString(),
        displayItems = viewModel.DisplayItems.Select<SummaryDisplayItemViewModel, SummaryDisplayItem>((Func<SummaryDisplayItemViewModel, SummaryDisplayItem>) (it => it.ToItemModel())).ToList<SummaryDisplayItem>(),
        sortOrder = num
      };
      if (summaryTemplates.templates == null)
        summaryTemplates.templates = new List<SummaryTemplate>();
      summaryTemplates.templates.Add(summaryTemplate);
      summaryTemplates.mtime = Utils.GetNowTimeStampInMills();
      this.SelectedTemplateId = summaryTemplate.id;
      LocalSettings.Settings.ExtraSettings.SelectedSummaryTemplateId = this.SelectedTemplateId;
      await SettingsHelper.PushLocalPreference();
    }

    private string GetSelectedRule()
    {
      return new NormalFilterViewModel()
      {
        Projects = this.SelectedProjectIds,
        Groups = this.SelectedProjectGroupIds,
        Tags = this.SelectedTags,
        Priorities = this.SelectedPriorities.Select<string, int>(new Func<string, int>(int.Parse)).ToList<int>(),
        Status = this.SelectedStatus,
        Assignees = this.Assignees,
        DueDates = new List<string>()
        {
          SummaryFilterViewModel.Filter2Rule(this.DateFilter, this.StartDate, this.EndDate)
        },
        CompletedTimes = new List<string>()
        {
          SummaryFilterViewModel.Filter2Rule(this.DateFilter, this.StartDate, this.EndDate)
        },
        Version = 6
      }.ToRule(false);
    }

    public void SaveTemplate(string name = null, bool sync = false)
    {
      this.SaveSelectedTemplate((Action<SummaryTemplate>) (template =>
      {
        template.name = name ?? this.GetSelectedTemplateName();
        template.rule = this.GetSelectedRule();
        template.sortType = this.SortBy.ToString();
        template.displayItems = this.DisplayItems.Select<SummaryDisplayItemViewModel, SummaryDisplayItem>((Func<SummaryDisplayItemViewModel, SummaryDisplayItem>) (it => it.ToItemModel())).ToList<SummaryDisplayItem>();
      }), sync);
    }

    public async void SaveSelectedTemplate(Action<SummaryTemplate> handler, bool sync = false)
    {
      SummaryFilterViewModel summaryFilterViewModel = this;
      handler(summaryFilterViewModel.GetSelectedTemplate(summaryFilterViewModel.SelectedTemplateId));
      if (summaryFilterViewModel.SelectedTemplateId == "defaultId")
      {
        SummaryTemplate summaryTemplate = JsonConvert.DeserializeObject<SummaryTemplate>(LocalSettings.Settings.ExtraSettings.DefaultSummaryTemplate);
        summaryTemplate.rule = summaryFilterViewModel.GetSelectedRule();
        summaryTemplate.sortType = summaryFilterViewModel.SortBy.ToString();
        summaryTemplate.displayItems = summaryFilterViewModel.DisplayItems.Select<SummaryDisplayItemViewModel, SummaryDisplayItem>((Func<SummaryDisplayItemViewModel, SummaryDisplayItem>) (it => it.ToItemModel())).ToList<SummaryDisplayItem>();
        LocalSettings.Settings.ExtraSettings.DefaultSummaryTemplate = JsonConvert.SerializeObject((object) summaryTemplate);
      }
      else
      {
        if (!sync)
          return;
        ticktick_WPF.Models.SummaryTemplates summaryTemplates = LocalSettings.Settings.UserPreference.summaryTemplates;
        SummaryTemplate summaryTemplate1;
        if (summaryTemplates == null)
        {
          summaryTemplate1 = (SummaryTemplate) null;
        }
        else
        {
          List<SummaryTemplate> templates = summaryTemplates.templates;
          // ISSUE: reference to a compiler-generated method
          summaryTemplate1 = templates != null ? templates.FirstOrDefault<SummaryTemplate>(new Func<SummaryTemplate, bool>(summaryFilterViewModel.\u003CSaveSelectedTemplate\u003Eb__26_1)) : (SummaryTemplate) null;
        }
        SummaryTemplate summaryTemplate2 = summaryTemplate1;
        if (summaryTemplate2 == null)
          return;
        handler(summaryTemplate2);
        summaryTemplates.mtime = Utils.GetNowTimeStampInMills();
        await SettingsHelper.PushLocalPreference();
      }
    }

    private SummaryTemplate GetSelectedTemplate()
    {
      if (!string.IsNullOrEmpty(this.SelectedTemplateId))
      {
        SummaryTemplate selectedTemplate = this.SummaryTemplates.FirstOrDefault<SummaryTemplate>((Func<SummaryTemplate, bool>) (it => it.id == this.SelectedTemplateId));
        if (selectedTemplate != null)
          return selectedTemplate;
      }
      return this.SummaryTemplates.First<SummaryTemplate>();
    }

    private SummaryTemplate GetSelectedTemplate(string id)
    {
      if (!string.IsNullOrEmpty(id))
      {
        SummaryTemplate selectedTemplate = this.SummaryTemplates.FirstOrDefault<SummaryTemplate>((Func<SummaryTemplate, bool>) (it => it.id == id));
        if (selectedTemplate != null)
          return selectedTemplate;
      }
      return this.SummaryTemplates.First<SummaryTemplate>();
    }

    public List<SummaryTemplate> SummaryTemplates { get; set; }

    public string ProItemsText => this.GetProItemsText();

    public void NotifyProItems() => this.OnPropertyChanged("ProItemsText");

    private string GetProItemsText()
    {
      if (UserDao.IsPro2())
        return string.Empty;
      List<string> list = this.DisplayItems.Where<SummaryDisplayItemViewModel>((Func<SummaryDisplayItemViewModel, bool>) (it => it.Enabled && it.IsProItem)).Select<SummaryDisplayItemViewModel, string>((Func<SummaryDisplayItemViewModel, string>) (it => it.Name)).ToList<string>();
      if (list.Count <= 0)
        return string.Empty;
      string str = "";
      if (Utils.IsEn())
        str = " ";
      return Utils.GetString("use_pro_summary_style") + str + string.Join(", ", (IEnumerable<string>) list);
    }

    public ObservableCollection<SummaryDisplayItemViewModel> DisplayItems { get; set; }

    public bool ShowPriority => !this.MoreConditions.Contains("priority");

    public bool ShowAssignee
    {
      get => !this.MoreConditions.Contains("assignee") && SummaryFilterViewModel.HasShareList();
    }

    private static bool HasShareList()
    {
      return CacheManager.GetProjects().Any<ProjectModel>((Func<ProjectModel, bool>) (it => it.IsShareList()));
    }

    public bool WithHabit
    {
      get
      {
        return (this._selectedProjectIds.Contains("Habit2e4c103c57ef480997943206") || this.IsProjectCategoryAll()) && this.IsNonProjectRuleAll();
      }
    }

    public bool WithCalendar
    {
      get
      {
        return (this._selectedProjectIds.Contains("Calendar5959a2259161d16d23a4f272") || this.IsProjectCategoryAll()) && this.IsNonProjectRuleAll();
      }
    }

    private bool IsProjectCategoryAll()
    {
      return this._selectedProjectIds.IsNullOrEmpty<string>() && this._selectedProjectGroupIds.IsNullOrEmpty<string>();
    }

    private bool IsNonProjectRuleAll()
    {
      return this.IsStatusAll() && this.IsPriorityAll() && this.IsTagAll() && this.IsAssigneeAll();
    }

    private bool IsStatusAll()
    {
      return this._selectedStatus.Count == 0 || this._selectedStatus.Count == 4;
    }

    private bool IsAssigneeAll() => this.Assignees.Count == 0;

    private bool IsTagAll() => this._selectedTags.Count == 0;

    private bool IsPriorityAll()
    {
      return this._selectedPriorities.Count == 0 || this._selectedPriorities.Count == 4;
    }

    public static string Filter2Rule(DateFilter filter, DateTime? start = null, DateTime? end = null)
    {
      string str = "thisweek";
      switch (filter)
      {
        case DateFilter.Today:
          str = "today";
          break;
        case DateFilter.Tomorrow:
          str = "tomorrow";
          break;
        case DateFilter.Yesterday:
          str = "offset(-1D)";
          break;
        case DateFilter.ThisWeek:
          str = "thisweek";
          break;
        case DateFilter.NextWeek:
          str = "nextweek";
          break;
        case DateFilter.LastWeek:
          str = "offset(-1W)";
          break;
        case DateFilter.ThisMonth:
          str = "thismonth";
          break;
        case DateFilter.LastMonth:
          str = "offset(-1M)";
          break;
      }
      if (filter == DateFilter.Custom && start.HasValue && end.HasValue)
      {
        TimeSpan timeSpan = start.Value.Date - DateTime.Today.Date;
        int totalDays1 = (int) timeSpan.TotalDays;
        DateTime today = end.Value;
        DateTime date1 = today.Date;
        today = DateTime.Today;
        DateTime date2 = today.Date;
        timeSpan = date1 - date2;
        int totalDays2 = (int) timeSpan.TotalDays;
        str = string.Format("span({0}~{1})", (object) totalDays1, (object) totalDays2);
      }
      return str;
    }

    private static DateFilter ParseDateFilter(string rule)
    {
      DateFilter dateFilter;
      if (rule != null)
      {
        switch (rule.Length)
        {
          case 5:
            if (rule == "today")
            {
              dateFilter = DateFilter.Today;
              goto label_21;
            }
            else
              break;
          case 8:
            switch (rule[1])
            {
              case 'e':
                if (rule == "nextweek")
                {
                  dateFilter = DateFilter.NextWeek;
                  goto label_21;
                }
                else
                  break;
              case 'h':
                if (rule == "thisweek")
                {
                  dateFilter = DateFilter.ThisWeek;
                  goto label_21;
                }
                else
                  break;
              case 'o':
                if (rule == "tomorrow")
                {
                  dateFilter = DateFilter.Tomorrow;
                  goto label_21;
                }
                else
                  break;
            }
            break;
          case 9:
            if (rule == "thismonth")
            {
              dateFilter = DateFilter.ThisMonth;
              goto label_21;
            }
            else
              break;
          case 11:
            switch (rule[9])
            {
              case 'D':
                if (rule == "offset(-1D)")
                {
                  dateFilter = DateFilter.Yesterday;
                  goto label_21;
                }
                else
                  break;
              case 'M':
                if (rule == "offset(-1M)")
                {
                  dateFilter = DateFilter.LastMonth;
                  goto label_21;
                }
                else
                  break;
              case 'W':
                if (rule == "offset(-1W)")
                {
                  dateFilter = DateFilter.LastWeek;
                  goto label_21;
                }
                else
                  break;
            }
            break;
        }
      }
      dateFilter = DateFilter.ThisWeek;
label_21:
      if (rule.Contains("span"))
        dateFilter = DateFilter.Custom;
      return dateFilter;
    }

    public void RefreshDate(string customRule = "")
    {
      if (this.DateFilter != DateFilter.Custom)
      {
        (DateTime, DateTime) tuple = SearchDateTextConverter.ConvertDateFilter2Span(this.DateFilter);
        this.StartDate = new DateTime?(tuple.Item1);
        this.EndDate = new DateTime?(tuple.Item2);
      }
      else if (!string.IsNullOrEmpty(customRule))
      {
        List<FilterDatePair> filterDatePairs = FilterUtils.GetFilterDatePairs((IEnumerable<string>) new List<string>()
        {
          customRule
        });
        if (filterDatePairs == null || filterDatePairs.Count <= 0)
          return;
        FilterDatePair filterDatePair = filterDatePairs.FirstOrDefault<FilterDatePair>();
        if (filterDatePair == null || !filterDatePair.Start.HasValue || !filterDatePair.End.HasValue)
          return;
        this.StartDate = new DateTime?(filterDatePair.Start.Value);
        this.EndDate = new DateTime?(filterDatePair.End.Value.AddDays(-1.0));
      }
      else
      {
        DateTime? nullable = this.StartDate;
        this.StartDate = new DateTime?(nullable ?? DateTime.Today);
        nullable = this.EndDate;
        this.EndDate = new DateTime?((nullable ?? this.StartDate).Value);
      }
    }

    public void LoadTemplate()
    {
      SummaryTemplate summaryTemplate = this.SummaryTemplates.FirstOrDefault<SummaryTemplate>((Func<SummaryTemplate, bool>) (it => it.id == this.SelectedTemplateId));
      if (summaryTemplate == null)
        return;
      if (summaryTemplate.displayItems != null)
        this.DisplayItems = new ObservableCollection<SummaryDisplayItemViewModel>(summaryTemplate.displayItems.Select<SummaryDisplayItem, SummaryDisplayItemViewModel>((Func<SummaryDisplayItem, SummaryDisplayItemViewModel>) (it => new SummaryDisplayItemViewModel(it))));
      this._sortBy = SummaryFilterViewModel.TryParseSortType(summaryTemplate.sortType);
      this._filter = Parser.ToNormalModel(summaryTemplate.rule);
      this.DateFilter = DateFilter.ThisWeek;
      string str = "";
      if (this._filter.DueDates != null && this._filter.DueDates.Count > 0)
      {
        str = this._filter.DueDates[0];
        this.DateFilter = SummaryFilterViewModel.ParseDateFilter(str);
      }
      this.RefreshDate(str);
      this.SelectedPriorities = this._filter.Priorities.Select<int, string>((Func<int, string>) (it => it.ToString())).ToList<string>();
      this.SelectedTags = this._filter.Tags;
      this.SelectedStatus = this._filter.Status;
      this.SelectedProjectIds = this._filter.Projects;
      this.SelectedProjectGroupIds = this._filter.Groups;
      this.SelectedProjectDisplayText = ProjectExtra.FormatDisplayText(this._selectedProjectIds, this._selectedProjectGroupIds);
      this.SelectedTagDisplayText = TagCardViewModel.ToNormalDisplayText(this._selectedTags);
      this.SelectedPriorityDisplayText = TaskPriorityTypeEditDialog.FormatDisplayText(this._selectedPriorities);
      this.SelectedStatusDisplayText = TaskStatusTypeEditDialog.FormatDisplayText(this._selectedStatus);
      this.Assignees = this._filter.Assignees;
      this.SetAssignDisplayText(this._filter.Assignees);
      List<string> stringList = new List<string>();
      if (this._filter.Priorities.Count == 0)
        stringList.Add("priority");
      if (this._filter.Assignees.Count == 0 && SummaryFilterViewModel.HasShareList())
        stringList.Add("assignee");
      this.MoreConditions = stringList;
    }

    public static void TryInitDefaultTemplate()
    {
      if (LocalSettings.Settings.ExtraSettings.SummaryFilterImported)
        return;
      SummaryFilterModel summaryFilter = LocalSettings.Settings.SummaryFilter;
      List<string> stringList = summaryFilter.SelectedProjectIds;
      if (summaryFilter.SelectedProjectIds.Count == 0 && summaryFilter.SelectedProjectGroupIds.Count == 0)
        stringList = new List<string>()
        {
          "ProjectAll2e4c103c57ef480997943206"
        };
      NormalFilterViewModel normalFilterViewModel = new NormalFilterViewModel()
      {
        Projects = stringList,
        Groups = summaryFilter.SelectedProjectGroupIds,
        Tags = summaryFilter.SelectedTags,
        Priorities = summaryFilter.SelectedPriorities.Select<string, int>(new Func<string, int>(int.Parse)).ToList<int>(),
        Assignees = summaryFilter.Assigns,
        DueDates = new List<string>()
        {
          SummaryFilterViewModel.Filter2Rule(summaryFilter.DateFilter, summaryFilter.StartDate, summaryFilter.EndDate)
        },
        CompletedTimes = new List<string>()
        {
          SummaryFilterViewModel.Filter2Rule(summaryFilter.DateFilter, summaryFilter.StartDate, summaryFilter.EndDate)
        },
        Version = 6
      };
      List<SummaryDisplayItemModel> source = SummaryFilterViewModel.InitDefaultDisplayItems();
      source.First<SummaryDisplayItemModel>((Func<SummaryDisplayItemModel, bool>) (it => it.Key == "detail")).Enabled = summaryFilter.ShowDetail;
      source.First<SummaryDisplayItemModel>((Func<SummaryDisplayItemModel, bool>) (it => it.Key == "focus")).Enabled = summaryFilter.ShowPomo;
      source.First<SummaryDisplayItemModel>((Func<SummaryDisplayItemModel, bool>) (it => it.Key == "progress")).Enabled = summaryFilter.ShowProgress;
      source.First<SummaryDisplayItemModel>((Func<SummaryDisplayItemModel, bool>) (it => it.Key == "project")).Enabled = summaryFilter.ShowBelongList;
      source.First<SummaryDisplayItemModel>((Func<SummaryDisplayItemModel, bool>) (it => it.Key == "completedTime")).Enabled = summaryFilter.ShowCompleteDate;
      SummaryTemplate summaryTemplate = new SummaryTemplate()
      {
        id = "defaultId",
        name = Utils.GetString("Summary"),
        rule = normalFilterViewModel.ToRule(false),
        sortType = summaryFilter.SortBy.ToString(),
        displayItems = source.Select<SummaryDisplayItemModel, SummaryDisplayItem>((Func<SummaryDisplayItemModel, SummaryDisplayItem>) (it => new SummaryDisplayItem()
        {
          key = it.Key,
          sortOrder = it.SortOrder,
          style = it.Style,
          enabled = it.Enabled
        })).ToList<SummaryDisplayItem>()
      };
      LocalSettings.Settings.ExtraSettings.SummaryFilterImported = true;
      LocalSettings.Settings.ExtraSettings.DefaultSummaryTemplate = JsonConvert.SerializeObject((object) summaryTemplate);
      LocalSettings.Settings.Save();
    }

    public void SetModel(SummaryFilterModel model)
    {
      if (model == null)
        return;
      this.DateFilter = model.DateFilter;
      this._selectedTags = model.SelectedTags ?? this._selectedTags;
      this._selectedProjectIds = model.SelectedProjectIds ?? this._selectedProjectIds;
      this._selectedProjectGroupIds = model.SelectedProjectGroupIds ?? this._selectedProjectGroupIds;
      this._selectedStatus = model.SelectedStatus ?? this._selectedStatus;
      this._selectedPriorities = model.SelectedPriorities ?? this._selectedPriorities;
      this.StartDate = model.StartDate ?? this._startDate;
      this.EndDate = model.EndDate ?? this._endDate;
      this.Assignees = model.Assigns;
      this._sortBy = model.SortBy;
      this._showCompleteDate = model.ShowCompleteDate;
      this._showDetail = model.ShowDetail;
      this._showPomo = model.ShowPomo;
      this._showProgress = model.ShowProgress;
      this._showBelongList = model.ShowBelongList;
      this.SelectedProjectDisplayText = ProjectExtra.FormatDisplayText(this._selectedProjectIds, this._selectedProjectGroupIds);
      this.SelectedTagDisplayText = TagCardViewModel.ToNormalDisplayText(this._selectedTags);
      this.SelectedPriorityDisplayText = TaskPriorityTypeEditDialog.FormatDisplayText(this._selectedPriorities);
      this.SelectedStatusDisplayText = TaskStatusTypeEditDialog.FormatDisplayText(this._selectedStatus);
      this.SetAssignDisplayText(this.Assignees);
      if (!model.DisplayItems.IsNullOrEmpty<SummaryDisplayItemModel>())
        this.DisplayItems = new ObservableCollection<SummaryDisplayItemViewModel>(model.DisplayItems.Select<SummaryDisplayItemModel, SummaryDisplayItemViewModel>((Func<SummaryDisplayItemModel, SummaryDisplayItemViewModel>) (item => new SummaryDisplayItemViewModel(item))).OrderBy<SummaryDisplayItemViewModel, long>((Func<SummaryDisplayItemViewModel, long>) (it => it.SortOrder)).ToList<SummaryDisplayItemViewModel>());
      else
        this.DisplayItems = new ObservableCollection<SummaryDisplayItemViewModel>(SummaryFilterViewModel.InitDefaultDisplayItems().Select<SummaryDisplayItemModel, SummaryDisplayItemViewModel>((Func<SummaryDisplayItemModel, SummaryDisplayItemViewModel>) (m => new SummaryDisplayItemViewModel(m))).ToList<SummaryDisplayItemViewModel>());
    }

    public void SetupDefaultItems()
    {
      this.DisplayItems = new ObservableCollection<SummaryDisplayItemViewModel>(SummaryFilterViewModel.InitDefaultDisplayItems().Select<SummaryDisplayItemModel, SummaryDisplayItemViewModel>((Func<SummaryDisplayItemModel, SummaryDisplayItemViewModel>) (m => new SummaryDisplayItemViewModel(m))).ToList<SummaryDisplayItemViewModel>());
    }

    public static List<SummaryDisplayItemModel> InitDefaultDisplayItems()
    {
      return new List<SummaryDisplayItemModel>()
      {
        new SummaryDisplayItemModel("status", 0L, "", false),
        new SummaryDisplayItemModel("progress", 1L, "", true),
        new SummaryDisplayItemModel("completedTime", 2L, "date", true),
        new SummaryDisplayItemModel("dueDate", 3L, "date", false),
        new SummaryDisplayItemModel("title", 4L, "", true),
        new SummaryDisplayItemModel("parentTask", 5L, "", false),
        new SummaryDisplayItemModel("tag", 6L, "", false),
        new SummaryDisplayItemModel("focus", 7L, "", false),
        new SummaryDisplayItemModel("project", 8L, "project", false),
        new SummaryDisplayItemModel("detail", 9L, "text", false)
      };
    }

    public void Save() => this.ToFilterModel().Save();

    public SummaryFilterModel ToFilterModel()
    {
      return new SummaryFilterModel()
      {
        DateFilter = this._dateFilter,
        SelectedTags = this._selectedTags,
        SelectedProjectIds = this._selectedProjectIds,
        SelectedProjectGroupIds = this._selectedProjectGroupIds,
        SelectedPriorities = this._selectedPriorities,
        SelectedStatus = this._selectedStatus,
        Assigns = this.Assignees,
        StartDate = this._startDate,
        EndDate = this._endDate,
        SortBy = this._sortBy,
        ShowCompleteDate = this._showCompleteDate,
        ShowBelongList = this._showBelongList,
        ShowDetail = this._showDetail,
        ShowPomo = this._showPomo,
        ShowProgress = this._showProgress,
        DisplayItems = this.DisplayItems.Select<SummaryDisplayItemViewModel, SummaryDisplayItemModel>((Func<SummaryDisplayItemViewModel, SummaryDisplayItemModel>) (vm => vm.ToModel())).ToList<SummaryDisplayItemModel>()
      };
    }

    public SummarySortType SortBy
    {
      get => this._sortBy;
      set => this._sortBy = value;
    }

    public bool ShowCompleteDate => this.IsDisplayItemEnabled("completedTime");

    public bool ShowTaskDate => this.IsDisplayItemEnabled("dueDate");

    public bool ShowTag => this.IsDisplayItemEnabled("tag");

    public bool ShowProject => this.IsDisplayItemEnabled("project");

    public bool ShowStatus => this.IsDisplayItemEnabled("status");

    public bool ShowDetail => this.IsDisplayItemEnabled("detail");

    public bool ShowProgress => this.IsDisplayItemEnabled("progress");

    public bool ShowParent => this.IsDisplayItemEnabled("parentTask");

    public bool ShowColumn => this.ShowProject && this.GetStyleByKey("project") == "column";

    public bool ShowTaskTime => this.ShowTaskDate && this.GetStyleByKey("dueDate") == "time";

    public bool ParentPrefix => this.ShowParent && this.IsParentBeforeTitle();

    public bool ShowCompletedTime
    {
      get => this.ShowCompleteDate && this.GetStyleByKey("completedTime") == "time";
    }

    private string GetStyleByKey(string key)
    {
      SummaryDisplayItemViewModel displayItemViewModel = this.DisplayItems.FirstOrDefault<SummaryDisplayItemViewModel>((Func<SummaryDisplayItemViewModel, bool>) (t => t.Key == key));
      return displayItemViewModel != null ? displayItemViewModel.Style ?? displayItemViewModel.SupportedStyles.FirstOrDefault<string>() : (string) null;
    }

    private bool IsParentBeforeTitle()
    {
      SummaryDisplayItemViewModel displayItemViewModel1 = this.DisplayItems.FirstOrDefault<SummaryDisplayItemViewModel>((Func<SummaryDisplayItemViewModel, bool>) (t => t.Key == "parentTask"));
      SummaryDisplayItemViewModel displayItemViewModel2 = this.DisplayItems.FirstOrDefault<SummaryDisplayItemViewModel>((Func<SummaryDisplayItemViewModel, bool>) (t => t.Key == "title"));
      return displayItemViewModel1 == null || displayItemViewModel2 == null || displayItemViewModel1.SortOrder < displayItemViewModel2.SortOrder;
    }

    public bool ShowClearSummaryOption()
    {
      return this.DateFilter != DateFilter.ThisWeek || this.SelectedProjectIds.Count != 1 || !(this.SelectedProjectIds[0] == "ProjectAll2e4c103c57ef480997943206") || this.SelectedProjectGroupIds.Count > 0 || this.SelectedPriorities.Count > 0 || this.SelectedTags.Count > 0 || this.SelectedStatus.Count > 0 || this.Assignees.Count > 0;
    }

    private bool IsDisplayItemEnabled(string key)
    {
      SummaryDisplayItemViewModel displayItemViewModel = this.DisplayItems.FirstOrDefault<SummaryDisplayItemViewModel>((Func<SummaryDisplayItemViewModel, bool>) (t => t.Key == key));
      return displayItemViewModel != null && displayItemViewModel.Enabled;
    }

    public List<string> SelectedProjectIds
    {
      get => this._selectedProjectIds;
      set
      {
        this._selectedProjectIds = value;
        this.OnPropertyChanged(nameof (SelectedProjectIds));
      }
    }

    public List<string> SelectedPriorities
    {
      get => this._selectedPriorities;
      set
      {
        this._selectedPriorities = value;
        this.OnPropertyChanged(nameof (SelectedPriorities));
      }
    }

    public List<string> SelectedStatus
    {
      get => this._selectedStatus;
      set
      {
        this._selectedStatus = value;
        this.OnPropertyChanged(nameof (SelectedStatus));
      }
    }

    public List<string> SelectedProjectGroupIds
    {
      get => this._selectedProjectGroupIds;
      set
      {
        this._selectedProjectGroupIds = value;
        this.OnPropertyChanged(nameof (SelectedProjectGroupIds));
      }
    }

    public List<string> Assignees { get; set; }

    public void SetAssignDisplayText(List<string> assignees)
    {
      if (assignees == null || assignees.Count == 0)
        this.SelectedAssignTypeText = Utils.GetString("AssignTo");
      else
        this.SelectedAssignTypeText = AssigneeCardViewModel.GetAssigneeValueText(assignees);
    }

    public string SelectedAssignTypeText
    {
      get => this._selectedAssignTypeText;
      set
      {
        this._selectedAssignTypeText = value;
        this.OnPropertyChanged(nameof (SelectedAssignTypeText));
      }
    }

    public string SelectedTagDisplayText
    {
      get => this._selectedTagDisplayText;
      set
      {
        this._selectedTagDisplayText = value;
        this.OnPropertyChanged(nameof (SelectedTagDisplayText));
      }
    }

    public List<string> SelectedTags
    {
      get => this._selectedTags;
      set
      {
        this._selectedTags = value;
        this.OnPropertyChanged(nameof (SelectedTags));
      }
    }

    public DateFilter DateFilter
    {
      get => this._dateFilter;
      set
      {
        this._dateFilter = value;
        this.OnPropertyChanged(nameof (DateFilter));
      }
    }

    public DateTime? StartDate
    {
      get => this._startDate;
      set
      {
        this._startDate = value;
        this.OnPropertyChanged(nameof (StartDate));
      }
    }

    public DateTime? EndDate
    {
      get => this._endDate;
      set
      {
        this._endDate = value;
        this.OnPropertyChanged(nameof (EndDate));
      }
    }

    public string SelectedProjectDisplayText
    {
      get => this._selectedProjectDisplayText;
      set
      {
        this._selectedProjectDisplayText = value;
        this.OnPropertyChanged(nameof (SelectedProjectDisplayText));
      }
    }

    public string SelectedStatusDisplayText
    {
      get => this._selectedStatusTypeText;
      set
      {
        this._selectedStatusTypeText = value;
        this.OnPropertyChanged(nameof (SelectedStatusDisplayText));
      }
    }

    public string SelectedPriorityDisplayText
    {
      get => this._selectedPriorityDisplayText;
      set
      {
        this._selectedPriorityDisplayText = value;
        this.OnPropertyChanged(nameof (SelectedPriorityDisplayText));
      }
    }

    public bool ShowPomo
    {
      get => this.IsDisplayItemEnabled("focus");
      set
      {
        this._showPomo = value;
        this.OnPropertyChanged(nameof (ShowPomo));
      }
    }

    public async void DeleteTemplate(string modelId)
    {
      SummaryTemplate summaryTemplate1 = this.SummaryTemplates.FirstOrDefault<SummaryTemplate>((Func<SummaryTemplate, bool>) (it => it.id == modelId));
      if (summaryTemplate1 == null)
        return;
      this.SummaryTemplates.Remove(summaryTemplate1);
      ticktick_WPF.Models.SummaryTemplates summaryTemplates = LocalSettings.Settings.UserPreference.summaryTemplates;
      SummaryTemplate summaryTemplate2;
      if (summaryTemplates == null)
      {
        summaryTemplate2 = (SummaryTemplate) null;
      }
      else
      {
        List<SummaryTemplate> templates = summaryTemplates.templates;
        summaryTemplate2 = templates != null ? templates.FirstOrDefault<SummaryTemplate>((Func<SummaryTemplate, bool>) (it => it.id == modelId)) : (SummaryTemplate) null;
      }
      SummaryTemplate summaryTemplate3 = summaryTemplate2;
      if (summaryTemplate3 == null)
        return;
      summaryTemplates.templates.Remove(summaryTemplate3);
      summaryTemplates.mtime = Utils.GetNowTimeStampInMills();
      LocalSettings.Settings.UserPreference.summaryTemplates = summaryTemplates;
      LocalSettings.Settings.Save();
      await SettingsHelper.PushLocalPreference();
    }

    public void RemoveProItems()
    {
      foreach (SummaryDisplayItemViewModel displayItem in (Collection<SummaryDisplayItemViewModel>) this.DisplayItems)
      {
        if (displayItem.IsProItem && displayItem.Enabled)
          displayItem.Enabled = false;
      }
      this.NotifyProItems();
    }

    public SummaryFilterViewModel()
    {
      DateTime dateTime1 = DateTime.Today;
      dateTime1 = dateTime1.AddDays((double) ((int) DateTime.Today.DayOfWeek * -1));
      this._startDate = new DateTime?(dateTime1.AddDays((double) Utils.GetWeekFromDiff()));
      DateTime dateTime2 = DateTime.Today;
      dateTime2 = dateTime2.AddDays((double) ((int) DateTime.Today.DayOfWeek * -1));
      dateTime2 = dateTime2.AddDays((double) Utils.GetWeekFromDiff());
      this._endDate = new DateTime?(dateTime2.AddDays(7.0));
      this._selectedProjectDisplayText = Utils.GetString("AllList");
      this._selectedTagDisplayText = Utils.GetString("AllTags");
      this._selectedAssignTypeText = Utils.GetString("AssignTo");
      this._selectedPriorityDisplayText = Utils.GetString("all_priorities");
      this._selectedStatusTypeText = Utils.GetString("AllStatus");
      this._showCompleteDate = true;
      this._showProgress = true;
      // ISSUE: reference to a compiler-generated field
      this.\u003CDisplayItems\u003Ek__BackingField = new ObservableCollection<SummaryDisplayItemViewModel>();
      // ISSUE: explicit constructor call
      base.\u002Ector();
    }
  }
}
