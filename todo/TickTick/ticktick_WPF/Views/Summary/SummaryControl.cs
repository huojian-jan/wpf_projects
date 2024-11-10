// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.SummaryControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.Filter;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Print;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Tag;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Summary
{
  public class SummaryControl : System.Windows.Controls.UserControl, IComponentConnector, IStyleConnector
  {
    private SummaryFilterViewModel _viewModel;
    private bool _isEditingTime;
    private string _projectIds;
    private DateTime _oldStart;
    private DateTime _oldEnd;
    private string _oldProjectIds;
    private AssigneeEditDialog _assignEditDialog;
    private SummaryTemplateViewModel _editingTemplate;
    internal Grid Container;
    internal StackPanel TitlePanel;
    internal Border MenuPathGrid;
    internal Image FoldImage;
    internal TextBlock TemplateName;
    internal Border ArrowGrid;
    internal EscPopup ChooseTemplatePopup;
    internal ItemsControl TemplatesItems;
    internal EscPopup TemplateOptionPopup;
    internal PopupPlacementBorder DateFilterText;
    internal EscPopup DateSelectPopup;
    internal PopupPlacementBorder ProjectOrGroupFilterText;
    internal EscPopup ProjectOrGroupFilterPopup;
    internal PopupPlacementBorder TagFilterText;
    internal EscPopup TagFilterPopup;
    internal PopupPlacementBorder StatusFilter;
    internal EscPopup StatusFilterPopup;
    internal PopupPlacementBorder PriorityFilter;
    internal EscPopup PriorityFilterPopup;
    internal PopupPlacementBorder AssignFilter;
    internal EscPopup AssignFilterPopup;
    internal PopupPlacementBorder MoreFilter;
    internal EscPopup MoreFilterPopup;
    internal ItemsControl MoreConditionPopup;
    internal Grid ClearFilterGrid;
    internal SummaryDisplayFilterControl DisplayFilter;
    internal EditorMenu TextEditorMenu;
    internal Grid SpliteLine;
    internal Grid ContentGrid;
    internal ScrollViewer ContentScrollViewer;
    internal Grid MdGrid;
    internal MarkDownEditor SummaryContent;
    internal System.Windows.Controls.Button CopyButton;
    internal System.Windows.Controls.Button ExportButton;
    internal System.Windows.Controls.Button InsertButton;
    internal Popup CopyPopup;
    internal Popup ExportPopup;
    private bool _contentLoaded;

    public event EventHandler<string> InsertSummary;

    public SummaryControl()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.LoadSummaryData();
      LocalSettings.Settings.OnPreferenceChanged += new EventHandler<string>(this.NotifySummaryChanged);
    }

    private void NotifySummaryChanged(object sender, string e)
    {
      ThreadUtil.DetachedRunOnUiThread((Action) (async () => this.LoadSummaryData()));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      LocalSettings.Settings.OnPreferenceChanged -= new EventHandler<string>(this.NotifySummaryChanged);
    }

    private static SummaryTemplate GetInitTemplate()
    {
      List<SummaryDisplayItem> list = SummaryFilterViewModel.InitDefaultDisplayItems().Select<SummaryDisplayItemModel, SummaryDisplayItem>((Func<SummaryDisplayItemModel, SummaryDisplayItem>) (it => new SummaryDisplayItem()
      {
        key = it.Key,
        enabled = it.Enabled,
        sortOrder = it.SortOrder,
        style = it.Style
      })).ToList<SummaryDisplayItem>();
      NormalFilterViewModel normalFilterViewModel = new NormalFilterViewModel()
      {
        Projects = new List<string>()
        {
          "ProjectAll2e4c103c57ef480997943206"
        },
        Groups = new List<string>(),
        DueDates = new List<string>() { "thisweek" },
        CompletedTimes = new List<string>() { "thisweek" },
        Version = 6
      };
      SummaryTemplate initTemplate = new SummaryTemplate()
      {
        id = "defaultId",
        name = Utils.GetString("Summary"),
        sortType = SummarySortType.progress.ToString(),
        displayItems = list,
        rule = normalFilterViewModel.ToRule(false)
      };
      LocalSettings.Settings.ExtraSettings.DefaultSummaryTemplate = JsonConvert.SerializeObject((object) initTemplate);
      LocalSettings.Settings.Save();
      return initTemplate;
    }

    private static SummaryTemplate GetDefaultTemplate()
    {
      if (string.IsNullOrEmpty(LocalSettings.Settings.ExtraSettings.DefaultSummaryTemplate))
        return SummaryControl.GetInitTemplate();
      try
      {
        return JsonConvert.DeserializeObject<SummaryTemplate>(LocalSettings.Settings.ExtraSettings.DefaultSummaryTemplate);
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
        return SummaryControl.GetInitTemplate();
      }
    }

    private void LoadSummaryData()
    {
      this._viewModel = new SummaryFilterViewModel()
      {
        DateFilter = DateFilter.ThisWeek
      };
      SummaryTemplates summaryTemplates = LocalSettings.Settings.UserPreference.summaryTemplates;
      List<SummaryTemplate> source1 = new List<SummaryTemplate>();
      if (summaryTemplates?.templates != null && summaryTemplates.templates.Count > 0)
        source1 = new List<SummaryTemplate>(LocalSettings.Settings.UserPreference.summaryTemplates.templates.Select<SummaryTemplate, SummaryTemplate>((Func<SummaryTemplate, SummaryTemplate>) (it => new SummaryTemplate()
        {
          id = it.id,
          name = it.name,
          sortOrder = it.sortOrder,
          sortType = it.sortType,
          displayItems = it.displayItems,
          rule = it.rule
        })));
      List<SummaryTemplate> source2 = (source1 != null ? source1.Where<SummaryTemplate>((Func<SummaryTemplate, bool>) (it => it.id != null)).GroupBy<SummaryTemplate, string>((Func<SummaryTemplate, string>) (item => item.id)).Select<IGrouping<string, SummaryTemplate>, SummaryTemplate>((Func<IGrouping<string, SummaryTemplate>, SummaryTemplate>) (group => group.First<SummaryTemplate>())).ToList<SummaryTemplate>() : (List<SummaryTemplate>) null) ?? new List<SummaryTemplate>();
      if (source2.Count == 0 || source2.All<SummaryTemplate>((Func<SummaryTemplate, bool>) (it => it.id != "defaultId")))
        source2.Insert(0, SummaryControl.GetDefaultTemplate());
      this._viewModel.ShowTemplate = source2.Count > 1;
      string selectedId = LocalSettings.Settings.ExtraSettings.SelectedSummaryTemplateId;
      if (string.IsNullOrEmpty(selectedId) || source1.Find((Predicate<SummaryTemplate>) (it => it.id == selectedId)) == null)
        selectedId = "defaultId";
      this._viewModel.SelectedTemplateId = selectedId;
      this._viewModel.SummaryTemplates = source2;
      this.InitEvent();
      this.SummaryContent.EditBox.Margin = new Thickness(15.0);
      this.SummaryContent.SetMarkRegexText(true, false);
      this.LoadData();
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      this.DataContext = (object) this._viewModel;
    }

    private void InitEvent()
    {
      this.TextEditorMenu.EditorAction -= new EventHandler<string>(this.OnEditorAction);
      this.TextEditorMenu.EditorAction += new EventHandler<string>(this.OnEditorAction);
      this.SummaryContent.RegisterCaretChanged();
      this.SummaryContent.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      this.SummaryContent.CaretVerticalOffsetChanged += new EventHandler<double>(this.OnContentVerticalOffsetChanged);
    }

    private void OnContentVerticalOffsetChanged(object sender, double offset)
    {
      double num1 = this.ContentScrollViewer.VerticalOffset + this.ContentScrollViewer.ActualHeight;
      double num2 = offset + 15.0;
      if (num2 > num1)
        this.ContentScrollViewer.ScrollToVerticalOffset(num2 - this.ContentScrollViewer.ActualHeight);
      double verticalOffset = this.ContentScrollViewer.VerticalOffset;
      if (offset - 21.0 < verticalOffset)
        this.ContentScrollViewer.ScrollToVerticalOffset(offset - 10.0);
      if (offset > 24.0)
        return;
      this.ContentScrollViewer.ScrollToVerticalOffset(0.0);
    }

    private async void OnEditorAction(object sender, string tag)
    {
      this.SetContentStyle(tag);
      await Task.Delay(100);
      this.SummaryContent.FocusEditBox();
    }

    private void SetContentStyle(string tag)
    {
      string[] strArray = tag.Split('_');
      string empty = string.Empty;
      if (strArray.Length > 1)
      {
        tag = strArray[0];
        empty = strArray[1];
      }
      if (tag == null)
        return;
      switch (tag.Length)
      {
        case 4:
          switch (tag[0])
          {
            case 'B':
              if (!(tag == "Bold"))
                return;
              this.SummaryContent.Bold();
              return;
            case 'C':
              if (!(tag == "Code"))
                return;
              this.SummaryContent.Code();
              return;
            case 'L':
              if (!(tag == "Link"))
                return;
              this.SummaryContent.ShowInsertLink(this.SummaryContent.EditBox.SelectedText, string.Empty);
              return;
            default:
              return;
          }
        case 5:
          if (!(tag == "Quote"))
            break;
          this.SummaryContent.Quote();
          break;
        case 6:
          if (!(tag == "Italic"))
            break;
          this.SummaryContent.Italic();
          break;
        case 8:
          switch (tag[7])
          {
            case '1':
              if (!(tag == "Heading1"))
                return;
              this.SummaryContent.InsertHeader(1);
              return;
            case '2':
              if (!(tag == "Heading2"))
                return;
              this.SummaryContent.InsertHeader(2);
              return;
            case '3':
              if (!(tag == "Heading3"))
                return;
              this.SummaryContent.InsertHeader(3);
              return;
            case 'e':
              if (!(tag == "DateTime"))
                return;
              this.SummaryContent.AddText(empty);
              return;
            default:
              return;
          }
        case 9:
          switch (tag[0])
          {
            case 'C':
              if (!(tag == "CheckItem"))
                return;
              this.SummaryContent.InsertCheckItem();
              return;
            case 'H':
              if (!(tag == "HighLight"))
                return;
              this.SummaryContent.Highlight();
              return;
            case 'S':
              if (!(tag == "SplitLine"))
                return;
              this.SummaryContent.InsertLine();
              return;
            case 'U':
              if (!(tag == "UnderLine"))
                return;
              this.SummaryContent.UnderLine();
              return;
            default:
              return;
          }
        case 10:
          if (!(tag == "BulletList"))
            break;
          this.SummaryContent.InsertUnOrderList();
          break;
        case 12:
          if (!(tag == "NumberedList"))
            break;
          this.SummaryContent.InsertNumberedList();
          break;
        case 13:
          if (!(tag == "StrikeThrough"))
            break;
          this.SummaryContent.StrokeLine();
          break;
      }
    }

    private async void SetSummary(object sender = null, EventArgs eventArgs = null)
    {
      SummaryControl summaryControl1 = this;
      summaryControl1._viewModel.NotifyProItems();
      summaryControl1._viewModel.SetAssignDisplayText(summaryControl1._viewModel.Assignees);
      summaryControl1.ClearFilterGrid.Visibility = summaryControl1._viewModel.ShowClearSummaryOption() ? Visibility.Visible : Visibility.Collapsed;
      DateTime? nullable = summaryControl1._viewModel.StartDate;
      if (nullable.HasValue)
      {
        nullable = summaryControl1._viewModel.EndDate;
        if (nullable.HasValue)
        {
          DateTime oldStart = summaryControl1._oldStart;
          nullable = summaryControl1._viewModel.StartDate;
          DateTime dateTime1 = nullable.Value;
          if (!(oldStart != dateTime1))
          {
            DateTime oldEnd = summaryControl1._oldEnd;
            nullable = summaryControl1._viewModel.EndDate;
            DateTime dateTime2 = nullable.Value;
            if (!(oldEnd != dateTime2) && !(summaryControl1._projectIds != summaryControl1._oldProjectIds))
              goto label_12;
          }
          List<TaskModel> closedTasks;
          if (string.IsNullOrEmpty(summaryControl1._projectIds))
          {
            nullable = summaryControl1._viewModel.StartDate;
            DateTime? from = new DateTime?(nullable.Value);
            nullable = summaryControl1._viewModel.EndDate;
            DateTime? to = new DateTime?(nullable.Value);
            closedTasks = await Communicator.GetClosedTasks("all", from, to, 1000);
          }
          else
          {
            string projectIds = summaryControl1._projectIds;
            nullable = summaryControl1._viewModel.StartDate;
            DateTime? from = new DateTime?(nullable.Value);
            nullable = summaryControl1._viewModel.EndDate;
            DateTime? to = new DateTime?(nullable.Value);
            closedTasks = await Communicator.GetClosedTasks(projectIds, from, to, 1000);
          }
          List<TaskModel> tasks1 = closedTasks;
          // ISSUE: explicit non-virtual call
          if (tasks1 != null && __nonvirtual (tasks1.Count) > 0)
          {
            int num = await TaskService.MergeTasks((IEnumerable<TaskModel>) tasks1) ? 1 : 0;
          }
          SummaryControl summaryControl2 = summaryControl1;
          nullable = summaryControl1._viewModel.StartDate;
          DateTime dateTime3 = nullable.Value;
          summaryControl2._oldStart = dateTime3;
          SummaryControl summaryControl3 = summaryControl1;
          nullable = summaryControl1._viewModel.EndDate;
          DateTime dateTime4 = nullable.Value;
          summaryControl3._oldEnd = dateTime4;
          summaryControl1._oldProjectIds = summaryControl1._projectIds;
        }
      }
label_12:
      List<TaskBaseViewModel> tasks = await TaskDao.GetSummaryTask(summaryControl1._viewModel.ToFilterModel());
      List<SummaryTaskViewModel> habits = await summaryControl1.LoadHabitData();
      List<SummaryTaskViewModel> calendarEvents = await summaryControl1.LoadCalendarData();
      summaryControl1.SetSummaryContent(tasks, (IReadOnlyCollection<SummaryTaskViewModel>) habits, (IReadOnlyCollection<SummaryTaskViewModel>) calendarEvents);
      tasks = (List<TaskBaseViewModel>) null;
      habits = (List<SummaryTaskViewModel>) null;
    }

    private async Task<List<SummaryTaskViewModel>> LoadCalendarData()
    {
      if (!this._viewModel.WithCalendar || !this._viewModel.StartDate.HasValue || !this._viewModel.EndDate.HasValue)
        return new List<SummaryTaskViewModel>();
      DateTime start = this._viewModel.StartDate.Value;
      DateTime? nullable1 = this._viewModel.EndDate;
      DateTime end1 = nullable1.Value.AddDays(1.0);
      List<string> subscribes = new List<string>();
      List<string> accounts = new List<string>();
      List<CalendarDisplayModel> calendarEvents = await CalendarDisplayService.GetCalendarDisplayModelsBetweenSpan(start, end1, subscribes, accounts);
      nullable1 = this._viewModel.StartDate;
      ref DateTime? local1 = ref nullable1;
      DateTime? nullable2 = this._viewModel.StartDate;
      DateTime defaultValue1 = nullable2.Value;
      DateTime valueOrDefault = local1.GetValueOrDefault(defaultValue1);
      nullable1 = this._viewModel.EndDate;
      ref DateTime? local2 = ref nullable1;
      nullable2 = this._viewModel.EndDate;
      DateTime defaultValue2 = nullable2.Value;
      DateTime end2 = local2.GetValueOrDefault(defaultValue2).AddDays(1.0);
      calendarEvents.AddRange((IEnumerable<CalendarDisplayModel>) await CalendarDisplayService.GetRepeatDisplayModels(valueOrDefault, end2, true));
      Dictionary<string, string> calendarId2Names = (await BindCalendarAccountDao.GetBindCalendars()).ToDictionary<BindCalendarModel, string, string>((Func<BindCalendarModel, string>) (it => it.Id), (Func<BindCalendarModel, string>) (it => it.Name));
      List<SummaryTaskViewModel> list = calendarEvents.Select<CalendarDisplayModel, SummaryTaskViewModel>((Func<CalendarDisplayModel, SummaryTaskViewModel>) (h => SummaryTaskViewModel.BuildCalendar(h, calendarId2Names, this._viewModel))).ToList<SummaryTaskViewModel>();
      if (this._viewModel.SelectedStatus != null && this._viewModel.SelectedStatus.Count > 0 && !this._viewModel.SelectedStatus.Contains("all"))
      {
        List<Func<SummaryTaskViewModel, bool>> conditionList = new List<Func<SummaryTaskViewModel, bool>>();
        foreach (string selectedStatu in this._viewModel.SelectedStatus)
        {
          switch (selectedStatu)
          {
            case "completed":
              conditionList.Add((Func<SummaryTaskViewModel, bool>) (it => it.Status == 2));
              continue;
            case "uncompleted":
              conditionList.Add((Func<SummaryTaskViewModel, bool>) (it => it.Status == 0));
              continue;
            default:
              continue;
          }
        }
        list = list.Where<SummaryTaskViewModel>((Func<SummaryTaskViewModel, bool>) (it => conditionList.Any<Func<SummaryTaskViewModel, bool>>((Func<Func<SummaryTaskViewModel, bool>, bool>) (condition => condition(it))))).ToList<SummaryTaskViewModel>();
      }
      return list;
    }

    private void AssembleGroupDate(SummaryTaskViewModel taskModel)
    {
      if (this._viewModel.SortBy == SummarySortType.completedTime)
      {
        taskModel.GroupDate = taskModel.CompletedDate ?? taskModel.StartDate;
      }
      else
      {
        if (this._viewModel.SortBy != SummarySortType.dueDate)
          return;
        taskModel.GroupDate = taskModel.StartDate ?? taskModel.CompletedDate;
      }
    }

    private async Task<List<SummaryTaskViewModel>> LoadHabitData()
    {
      List<SummaryTaskViewModel> habitModels = new List<SummaryTaskViewModel>();
      if (this._viewModel.WithHabit && this._viewModel.StartDate.HasValue && this._viewModel.EndDate.HasValue)
      {
        List<HabitModel> habits = (await HabitDao.GetAllHabits()).Where<HabitModel>((Func<HabitModel, bool>) (it => it.Status == 0)).ToList<HabitModel>();
        Dictionary<string, HabitModel> id2Habit = habits.ToDictionary<HabitModel, string, HabitModel>((Func<HabitModel, string>) (it => it.Id), (Func<HabitModel, HabitModel>) (it => it));
        Dictionary<string, string> sectionId2Name = (await HabitSectionDao.GetAllHabitSections()).ToDictionary<HabitSectionModel, string, string>((Func<HabitSectionModel, string>) (it => it.Id), (Func<HabitSectionModel, string>) (it => it.Name));
        List<HabitCheckInModel> checkIns = await HabitCheckInDao.GetCheckInsByIdsInSpan(habits.Select<HabitModel, string>((Func<HabitModel, string>) (it => it.Id)).ToList<string>(), this._viewModel.StartDate.Value, this._viewModel.EndDate.Value);
        DateTime? nullable = this._viewModel.StartDate;
        int checkStamp = int.Parse(nullable.Value.ToString("yyyyMMdd"));
        Dictionary<string, HabitRecordModel> habit2Note = new Dictionary<string, HabitRecordModel>();
        if (this._viewModel.ShowDetail)
          habit2Note = (await HabitRecordDao.GetHabitRecords(checkStamp)).ToDictionary<HabitRecordModel, string, HabitRecordModel>((Func<HabitRecordModel, string>) (it => it.HabitId + "_" + it.Stamp.ToString()), (Func<HabitRecordModel, HabitRecordModel>) (it => it));
        if (checkIns != null && checkIns.Count > 0)
        {
          foreach (HabitCheckInModel habitCheckIn in checkIns)
          {
            HabitModel habit;
            if (id2Habit.TryGetValue(habitCheckIn.HabitId, out habit) && habitCheckIn.Value > 0.0)
            {
              SummaryTaskViewModel taskModel = SummaryTaskViewModel.BuildHabit(habit, habitCheckIn, habit2Note, sectionId2Name, this._viewModel);
              this.AssembleGroupDate(taskModel);
              habitModels.Add(taskModel);
            }
          }
        }
        if (this._viewModel.WithHabit)
        {
          nullable = this._viewModel.StartDate;
          if (nullable.HasValue)
          {
            nullable = this._viewModel.EndDate;
            if (nullable.HasValue)
            {
              DateTime today1 = DateTime.Today;
              nullable = this._viewModel.StartDate;
              if ((nullable.HasValue ? (today1 >= nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0)
              {
                DateTime today2 = DateTime.Today;
                nullable = this._viewModel.EndDate;
                if ((nullable.HasValue ? (today2 <= nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                {
                  List<HabitModel> todayHabits = await HabitDao.GetNeedCheckHabits();
                  List<HabitCheckInModel> monthCheckIns = await HabitCheckInDao.GetCheckInsInSpan(DateTime.Today.AddDays(-30.0), DateTime.Today.AddDays(1.0));
                  foreach (HabitModel habitModel in todayHabits)
                  {
                    HabitModel habit = habitModel;
                    string todayStamp = DateTime.Today.ToString("yyyyMMdd");
                    List<HabitCheckInModel> habitCheckIns = monthCheckIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id)).ToList<HabitCheckInModel>();
                    if ((!await HabitUtils.IsHabitValidInToday(habit, habitCheckIns) ? 0 : (!habitCheckIns.Any<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id && c.CheckinStamp == todayStamp)) ? 1 : 0)) != 0)
                    {
                      SummaryTaskViewModel taskModel = SummaryTaskViewModel.BuildHabit(habit, sectionId2Name, this._viewModel);
                      this.AssembleGroupDate(taskModel);
                      habitModels.Add(taskModel);
                    }
                    habitCheckIns = (List<HabitCheckInModel>) null;
                  }
                  todayHabits = (List<HabitModel>) null;
                  monthCheckIns = (List<HabitCheckInModel>) null;
                }
              }
            }
          }
        }
        habits = (List<HabitModel>) null;
        id2Habit = (Dictionary<string, HabitModel>) null;
        sectionId2Name = (Dictionary<string, string>) null;
        checkIns = (List<HabitCheckInModel>) null;
      }
      if (this._viewModel.SelectedStatus != null && this._viewModel.SelectedStatus.Count > 0 && !this._viewModel.SelectedStatus.Contains("all"))
      {
        List<Func<SummaryTaskViewModel, bool>> conditionList = new List<Func<SummaryTaskViewModel, bool>>();
        foreach (string selectedStatu in this._viewModel.SelectedStatus)
        {
          switch (selectedStatu)
          {
            case "inProgress":
              conditionList.Add((Func<SummaryTaskViewModel, bool>) (it => it.Status == 0 && !string.IsNullOrEmpty(it.ProgressText)));
              continue;
            case "completed":
              conditionList.Add((Func<SummaryTaskViewModel, bool>) (it => it.Status == 2));
              continue;
            case "uncompleted":
              conditionList.Add((Func<SummaryTaskViewModel, bool>) (it => it.Status == 0 && string.IsNullOrEmpty(it.ProgressText)));
              continue;
            case "wontDo":
              conditionList.Add((Func<SummaryTaskViewModel, bool>) (it => it.Status == -1));
              continue;
            default:
              continue;
          }
        }
        habitModels = habitModels.Where<SummaryTaskViewModel>((Func<SummaryTaskViewModel, bool>) (it => conditionList.Any<Func<SummaryTaskViewModel, bool>>((Func<Func<SummaryTaskViewModel, bool>, bool>) (condition => condition(it))))).ToList<SummaryTaskViewModel>();
      }
      List<SummaryTaskViewModel> summaryTaskViewModelList = habitModels;
      habitModels = (List<SummaryTaskViewModel>) null;
      return summaryTaskViewModelList;
    }

    private async void SetSummaryContent(
      List<TaskBaseViewModel> tasks,
      IReadOnlyCollection<SummaryTaskViewModel> habits,
      IReadOnlyCollection<SummaryTaskViewModel> calendarEvents)
    {
      Dictionary<string, Node<TaskBaseViewModel>> uncompletedNodeDict = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Status == 0)).ToDictionary<TaskBaseViewModel, string, Node<TaskBaseViewModel>>((Func<TaskBaseViewModel, string>) (task => task.Id), (Func<TaskBaseViewModel, Node<TaskBaseViewModel>>) (task => (Node<TaskBaseViewModel>) new TaskNode(task)));
      Dictionary<string, Node<TaskBaseViewModel>> closedNodeDict = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Status != 0)).ToDictionary<TaskBaseViewModel, string, Node<TaskBaseViewModel>>((Func<TaskBaseViewModel, string>) (task => task.Id), (Func<TaskBaseViewModel, Node<TaskBaseViewModel>>) (task => (Node<TaskBaseViewModel>) new TaskNode(task)));
      if (this._viewModel.ShowColumn)
      {
        Dictionary<string, ColumnModel> dictionary = (await ColumnDao.GetAllColumnsAsync()).GroupBy<ColumnModel, string>((Func<ColumnModel, string>) (cm => cm.projectId)).Where<IGrouping<string, ColumnModel>>((Func<IGrouping<string, ColumnModel>, bool>) (group => group.Count<ColumnModel>() > 1)).SelectMany<IGrouping<string, ColumnModel>, ColumnModel>((Func<IGrouping<string, ColumnModel>, IEnumerable<ColumnModel>>) (group => (IEnumerable<ColumnModel>) group.ToList<ColumnModel>())).ToList<ColumnModel>().ToDictionary<ColumnModel, string>((Func<ColumnModel, string>) (it => it.id));
        foreach (TaskBaseViewModel task in tasks)
        {
          ColumnModel columnModel;
          dictionary.TryGetValue(task.ColumnId ?? string.Empty, out columnModel);
          task.ColumnName = columnModel != null ? columnModel.name : string.Empty;
        }
      }
      TaskNodeUtils.BuildNodeTree<TaskBaseViewModel>(uncompletedNodeDict);
      TaskNodeUtils.BuildNodeTree<TaskBaseViewModel>(closedNodeDict);
      IEnumerable<Node<TaskBaseViewModel>> nodes = uncompletedNodeDict.Values.Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (n => !n.HasParent));
      IEnumerable<Node<TaskBaseViewModel>> closedParentNode = closedNodeDict.Values.Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (n => !n.HasParent));
      if (!this._viewModel.StartDate.HasValue)
      {
        uncompletedNodeDict = (Dictionary<string, Node<TaskBaseViewModel>>) null;
        closedNodeDict = (Dictionary<string, Node<TaskBaseViewModel>>) null;
        closedParentNode = (IEnumerable<Node<TaskBaseViewModel>>) null;
      }
      else if (!this._viewModel.EndDate.HasValue)
      {
        uncompletedNodeDict = (Dictionary<string, Node<TaskBaseViewModel>>) null;
        closedNodeDict = (Dictionary<string, Node<TaskBaseViewModel>>) null;
        closedParentNode = (IEnumerable<Node<TaskBaseViewModel>>) null;
      }
      else
      {
        string content = "# " + this.GetTitle();
        List<SummaryTaskViewModel> models = new List<SummaryTaskViewModel>();
        foreach (Node<TaskBaseViewModel> node in nodes)
        {
          SummaryTaskViewModel summaryTaskViewModel = await SummaryTaskViewModel.Build(node, this._viewModel);
          if (this._viewModel.SortBy == SummarySortType.completedTime)
          {
            summaryTaskViewModel.GroupDate = summaryTaskViewModel.Status != 0 ? summaryTaskViewModel.CompletedDate : summaryTaskViewModel.StartDate;
            if (Utils.IsEmptyDate(summaryTaskViewModel.GroupDate))
              continue;
          }
          else if (this._viewModel.SortBy == SummarySortType.dueDate)
          {
            summaryTaskViewModel.GroupDate = summaryTaskViewModel.StartDate ?? summaryTaskViewModel.CompletedDate;
            if (Utils.IsEmptyDate(summaryTaskViewModel.GroupDate))
              continue;
          }
          models.Add(summaryTaskViewModel);
        }
        foreach (Node<TaskBaseViewModel> node in closedParentNode)
        {
          SummaryTaskViewModel summaryTaskViewModel = await SummaryTaskViewModel.Build(node, this._viewModel);
          if (this._viewModel.SortBy == SummarySortType.completedTime)
          {
            summaryTaskViewModel.GroupDate = summaryTaskViewModel.CompletedDate;
            if (Utils.IsEmptyDate(summaryTaskViewModel.GroupDate))
              continue;
          }
          else if (this._viewModel.SortBy == SummarySortType.dueDate)
          {
            summaryTaskViewModel.GroupDate = summaryTaskViewModel.StartDate ?? summaryTaskViewModel.CompletedDate;
            if (Utils.IsEmptyDate(summaryTaskViewModel.GroupDate))
              continue;
          }
          models.Add(summaryTaskViewModel);
        }
        if (habits != null && habits.Count > 0)
          models.AddRange((IEnumerable<SummaryTaskViewModel>) habits);
        if (calendarEvents != null && calendarEvents.Count > 0)
          models.AddRange((IEnumerable<SummaryTaskViewModel>) calendarEvents);
        if (models.Count == 0)
        {
          content = content + "\r\n" + Utils.GetString("NoMatchTask");
        }
        else
        {
          string str = content;
          content = str + await this.GetTaskSummary(models);
          str = (string) null;
        }
        this.SummaryContent.Text = content;
        content = (string) null;
        models = (List<SummaryTaskViewModel>) null;
        uncompletedNodeDict = (Dictionary<string, Node<TaskBaseViewModel>>) null;
        closedNodeDict = (Dictionary<string, Node<TaskBaseViewModel>>) null;
        closedParentNode = (IEnumerable<Node<TaskBaseViewModel>>) null;
      }
    }

    private string GetTitle(bool withSpace = true)
    {
      if (!this._viewModel.StartDate.HasValue || !this._viewModel.EndDate.HasValue)
        return "";
      string title = this._viewModel.StartDate.Value.ToString("m", (IFormatProvider) App.Ci);
      if (this._viewModel.StartDate.Value != this._viewModel.EndDate.Value)
        title = title + (withSpace ? " - " : "-") + this._viewModel.EndDate.Value.ToString("m", (IFormatProvider) App.Ci);
      return title;
    }

    private async Task<string> GetTaskSummary(List<SummaryTaskViewModel> models)
    {
      string content = "";
      string str1 = Utils.GetString("statistics_habit");
      switch (this._viewModel.SortBy)
      {
        case SummarySortType.progress:
          content = this.GetTaskContent(models, content);
          break;
        case SummarySortType.project:
          models.Sort((Comparison<SummaryTaskViewModel>) ((a, b) =>
          {
            if (a.Project.Isinbox && !b.Project.Isinbox)
              return -1;
            return !a.Project.Isinbox && b.Project.Isinbox ? 1 : a.Project.sortOrder.CompareTo(b.Project.sortOrder);
          }));
          content = await this.GetGroupTaskContent(models.GroupBy<SummaryTaskViewModel, string>((Func<SummaryTaskViewModel, string>) (m => m.Project.id)), content, true);
          break;
        case SummarySortType.completedTime:
        case SummarySortType.dueDate:
          foreach (SummaryTaskViewModel model in models)
          {
            this.AssembleGroupDate(model);
            SummaryTaskViewModel summaryTaskViewModel = model;
            DateTime? groupDate = model.GroupDate;
            ref DateTime? local1 = ref groupDate;
            string str2;
            if (!local1.HasValue)
            {
              str2 = (string) null;
            }
            else
            {
              DateTime valueOrDefault = local1.GetValueOrDefault();
              ref DateTime local2 = ref valueOrDefault;
              DateTime today = model.GroupDate.Value;
              int year1 = today.Year;
              today = DateTime.Today;
              int year2 = today.Year;
              string format = year1 == year2 ? "m" : "d";
              CultureInfo ci = App.Ci;
              str2 = local2.ToString(format, (IFormatProvider) ci);
            }
            summaryTaskViewModel.GroupName = str2;
          }
          models.Sort((Comparison<SummaryTaskViewModel>) ((a, b) => a.GroupDate.HasValue && b.GroupDate.HasValue ? a.GroupDate.Value.CompareTo(b.GroupDate.Value) : 0));
          content = await this.GetGroupTaskContent(models.GroupBy<SummaryTaskViewModel, string>((Func<SummaryTaskViewModel, string>) (m => m.GroupName)), content);
          break;
        case SummarySortType.priority:
          foreach (SummaryTaskViewModel model in models)
          {
            if (model.IsHabitModel())
            {
              model.GroupName = str1;
            }
            else
            {
              switch (model.Priority)
              {
                case 0:
                  model.GroupName = Utils.GetString("PriorityNull");
                  continue;
                case 1:
                  model.GroupName = Utils.GetString("PriorityLow");
                  continue;
                case 3:
                  model.GroupName = Utils.GetString("PriorityMedium");
                  continue;
                case 5:
                  model.GroupName = Utils.GetString("PriorityHigh");
                  continue;
                default:
                  continue;
              }
            }
          }
          models.Sort((Comparison<SummaryTaskViewModel>) ((a, b) => b.Priority.CompareTo(a.Priority)));
          content = await this.GetGroupTaskContent(models.GroupBy<SummaryTaskViewModel, string>((Func<SummaryTaskViewModel, string>) (m => m.GroupName)), content);
          break;
        case SummarySortType.assignee:
          string me = LocalSettings.Settings.LoginUserId;
          models.Sort((Comparison<SummaryTaskViewModel>) ((a, b) =>
          {
            if (a.IsHabitModel() && !b.IsHabitModel())
              return 1;
            if (!a.IsHabitModel() && b.IsHabitModel())
              return -1;
            if (a.Assignee != me && b.Assignee == me)
              return 1;
            if (a.Assignee == me && b.Assignee != me)
              return -1;
            if (a.Assignee != b.Assignee)
            {
              if (string.IsNullOrEmpty(a.Assignee) || a.Assignee == "-1")
                return 1;
              if (string.IsNullOrEmpty(b.Assignee) || b.Assignee == "-1")
                return -1;
            }
            return string.Compare(a.Assignee, b.Assignee, StringComparison.Ordinal);
          }));
          content = await this.GetGroupTaskContent(models.GroupBy<SummaryTaskViewModel, string>((Func<SummaryTaskViewModel, string>) (m =>
          {
            if (m.IsHabitModel())
              return "habit";
            return string.IsNullOrEmpty(m.Assignee) || m.Assignee == "-1" ? "noAssignee" : m.Assignee;
          })), content, isAssign: true);
          break;
        case SummarySortType.tag:
          string str3 = Utils.GetString("NoTags");
          foreach (SummaryTaskViewModel model in models)
          {
            if (model.Tags != null)
              model.PrimaryTag = CacheManager.GetPrimaryTag(model.Tags.ToArray());
            model.GroupName = !model.IsHabitModel() ? (model.PrimaryTag != null ? model.PrimaryTag.label ?? model.PrimaryTag.name : str3) : str1;
          }
          models.Sort((Comparison<SummaryTaskViewModel>) ((a, b) =>
          {
            if (a.IsHabitModel() && !b.IsHabitModel())
              return 1;
            if (!a.IsHabitModel() && b.IsHabitModel())
              return -1;
            if (a.PrimaryTag == null && b.PrimaryTag != null)
              return 1;
            if (a.PrimaryTag != null && b.PrimaryTag == null)
              return -1;
            return a.PrimaryTag != null && b.PrimaryTag != null ? a.PrimaryTag.sortOrder.CompareTo(b.PrimaryTag.sortOrder) : 0;
          }));
          content = await this.GetGroupTaskContent(models.GroupBy<SummaryTaskViewModel, string>((Func<SummaryTaskViewModel, string>) (m => m.GroupName)), content);
          break;
      }
      return content;
    }

    private async Task<string> GetGroupTaskContent(
      IEnumerable<IGrouping<string, SummaryTaskViewModel>> groups,
      string content,
      bool isProject = false,
      bool isAssign = false)
    {
      foreach (IGrouping<string, SummaryTaskViewModel> group1 in groups)
      {
        IGrouping<string, SummaryTaskViewModel> group = group1;
        if (isProject)
        {
          ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == group.Key));
          content = projectModel == null ? (!(group.Key == "habit") ? (!(group.Key == "calendar") ? content + "\r\n## " + group.Key : content + "\r\n## " + Utils.GetString("Calendar")) : content + "\r\n## " + Utils.GetString("statistics_habit")) : content + "\r\n## " + projectModel.name;
        }
        else if (isAssign)
        {
          if (group.Key == LocalSettings.Settings.LoginUserId)
            content = content + "\r\n## " + Utils.GetString("Me");
          else if (group.Key == "habit")
            content = content + "\r\n## " + Utils.GetString("statistics_habit");
          else if (group.Key != "noAssignee" && long.TryParse(group.Key, out long _))
          {
            List<AvatarViewModel> projectAvatars = await AvatarHelper.GetProjectAvatars(group.ToList<SummaryTaskViewModel>().FirstOrDefault<SummaryTaskViewModel>()?.Project.id, false, true);
            AvatarViewModel avatarViewModel = projectAvatars != null ? projectAvatars.FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (u => u.UserId == group.Key)) : (AvatarViewModel) null;
            content = avatarViewModel == null ? content + "\r\n## " + group.Key : content + "\r\n## " + avatarViewModel.Name;
          }
          else
            content = content + "\r\n## " + Utils.GetString("NotAssigned");
        }
        else
          content = content + "\r\n## " + group.Key;
        content = this.GetTaskContent(group.ToList<SummaryTaskViewModel>(), content, true);
      }
      return content;
    }

    private string GetClosedTaskContent(
      IEnumerable<SummaryTaskViewModel> unorderTasks,
      string title,
      bool withSpace)
    {
      string content = string.Empty;
      IOrderedEnumerable<SummaryTaskViewModel> source = unorderTasks.OrderByDescending<SummaryTaskViewModel, DateTime?>((Func<SummaryTaskViewModel, DateTime?>) (t => t.CompletedDate)).ThenBy<SummaryTaskViewModel, int>((Func<SummaryTaskViewModel, int>) (t => t.Priority)).ThenBy<SummaryTaskViewModel, long>((Func<SummaryTaskViewModel, long>) (t => t.SortOrder)).ThenBy<SummaryTaskViewModel, long>((Func<SummaryTaskViewModel, long>) (t => t.Project.sortOrder));
      if (source.Any<SummaryTaskViewModel>())
        content = content + "\r\n" + (withSpace ? "###    " : "## ") + title;
      foreach (SummaryTaskViewModel summaryTaskViewModel in (IEnumerable<SummaryTaskViewModel>) source)
      {
        content += summaryTaskViewModel.GetContent(withSpace);
        List<SummaryTaskViewModel> children = summaryTaskViewModel.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
          content = this.GetChildrenContent(summaryTaskViewModel.Children, content, withSpace);
      }
      if (source.Any<SummaryTaskViewModel>())
        content += "\r\n";
      return content;
    }

    private string GetTaskContent(
      List<SummaryTaskViewModel> models,
      string content,
      bool withSpace = false)
    {
      List<SummaryTaskViewModel> list1 = models.Where<SummaryTaskViewModel>((Func<SummaryTaskViewModel, bool>) (m => m.Status != 0)).ToList<SummaryTaskViewModel>();
      IEnumerable<SummaryTaskViewModel> unorderTasks1 = list1.Where<SummaryTaskViewModel>((Func<SummaryTaskViewModel, bool>) (m => m.Status != -1));
      IEnumerable<SummaryTaskViewModel> unorderTasks2 = list1.Where<SummaryTaskViewModel>((Func<SummaryTaskViewModel, bool>) (m => m.Status == -1));
      content += this.GetClosedTaskContent(unorderTasks1, Utils.GetString("Completed"), withSpace);
      content += this.GetClosedTaskContent(unorderTasks2, Utils.GetString("Abandoned"), withSpace);
      List<SummaryTaskViewModel> list2 = models.Where<SummaryTaskViewModel>((Func<SummaryTaskViewModel, bool>) (m => m.Status == 0 && m.Progress.GetValueOrDefault() > 0)).ToList<SummaryTaskViewModel>();
      if (list2.Count > 0)
      {
        this.SortUnCompleteTask(list2);
        List<SummaryTaskViewModel> list3 = list2.OrderByDescending<SummaryTaskViewModel, int?>((Func<SummaryTaskViewModel, int?>) (it => it.Progress)).ToList<SummaryTaskViewModel>();
        content = content + "\r\n" + (withSpace ? "###    " : "## ") + Utils.GetString("SummaryInProgress");
        content = list3.Aggregate<SummaryTaskViewModel, string>(content, (Func<string, SummaryTaskViewModel, string>) ((current, u) =>
        {
          current += u.GetContent(withSpace);
          List<SummaryTaskViewModel> children = u.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
            current = this.GetChildrenContent(u.Children, current, withSpace);
          return current;
        }));
        content += "\r\n";
      }
      List<SummaryTaskViewModel> list4 = models.Where<SummaryTaskViewModel>((Func<SummaryTaskViewModel, bool>) (m => m.Status == 0 && m.Progress.GetValueOrDefault() == 0)).ToList<SummaryTaskViewModel>();
      if (list4.Count > 0)
      {
        this.SortUnCompleteTask(list4);
        content = content + "\r\n" + (withSpace ? "###    " : "## ") + Utils.GetString("Uncompleted");
        content = list4.Aggregate<SummaryTaskViewModel, string>(content, (Func<string, SummaryTaskViewModel, string>) ((current, u) =>
        {
          current += u.GetContent(withSpace);
          List<SummaryTaskViewModel> children = u.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
            current = this.GetChildrenContent(u.Children, current, withSpace);
          return current;
        }));
        content += "\r\n";
      }
      return content;
    }

    private string GetChildrenContent(
      List<SummaryTaskViewModel> children,
      string content,
      bool withSpace)
    {
      this.SortUnCompleteTask(children);
      foreach (SummaryTaskViewModel child in children)
      {
        content += child.GetContent(withSpace);
        List<SummaryTaskViewModel> children1 = child.Children;
        // ISSUE: explicit non-virtual call
        if ((children1 != null ? (__nonvirtual (children1.Count) > 0 ? 1 : 0) : 0) != 0)
          content = this.GetChildrenContent(child.Children, content, withSpace);
      }
      return content;
    }

    private void SortUnCompleteTask(List<SummaryTaskViewModel> models)
    {
      models.Sort((Comparison<SummaryTaskViewModel>) ((a, b) =>
      {
        int num1 = this.CompareModelByTime(a, b);
        if (num1 != 0 && (this._viewModel.SortBy == SummarySortType.dueDate || this._viewModel.SortBy == SummarySortType.completedTime || this._viewModel.SortBy == SummarySortType.progress))
          return num1;
        if (this._viewModel.SortBy == SummarySortType.priority && a.Priority != b.Priority)
          return b.Priority.CompareTo(a.Priority);
        int valueOrDefault1 = a.Progress.GetValueOrDefault();
        int valueOrDefault2 = b.Progress.GetValueOrDefault();
        if (valueOrDefault1 != valueOrDefault2)
          return valueOrDefault2.CompareTo(valueOrDefault1);
        if (num1 != 0)
          return num1;
        if (a.Priority != b.Priority)
          return b.Priority.CompareTo(a.Priority);
        int num2 = this.CompareModelType(a, b);
        if (num2 != 0)
          return num2;
        return a.Project.sortOrder != b.Project.sortOrder ? a.Project.sortOrder.CompareTo(b.Project.sortOrder) : a.SortOrder.CompareTo(b.SortOrder);
      }));
    }

    private int GetTypeOrdinal(SummaryTaskViewModel model)
    {
      if (model.IsTaskModel())
        return 3;
      if (model.IsCalendarModel())
        return 2;
      return model.IsHabitModel() ? 1 : 3;
    }

    private int CompareModelType(SummaryTaskViewModel left, SummaryTaskViewModel right)
    {
      int typeOrdinal = this.GetTypeOrdinal(left);
      return this.GetTypeOrdinal(right).CompareTo(typeOrdinal);
    }

    private int CompareModelByTime(SummaryTaskViewModel left, SummaryTaskViewModel right)
    {
      if (!left.StartDate.HasValue && right.StartDate.HasValue)
        return 1;
      if (left.StartDate.HasValue && !right.StartDate.HasValue)
        return -1;
      return left.SourceViewModel != null && right.SourceViewModel != null ? DateSortHelper.CompareDate(left.SourceViewModel, right.SourceViewModel, true) : 0;
    }

    private static string GetDateSpanDesc((DateTime, DateTime) span)
    {
      return " (" + DateUtils.FormatShortMonthDay(span.Item1) + " - " + DateUtils.FormatShortMonthDay(span.Item2) + ")";
    }

    private void DateFilterClick(object sender, MouseButtonEventArgs e)
    {
      TimeFilterDialog timeFilterDialog = new TimeFilterDialog()
      {
        ItemsSource = new List<ListItemData>()
        {
          new ListItemData((object) DateFilter.Today, Utils.GetString("Today")),
          new ListItemData((object) DateFilter.Tomorrow, Utils.GetString("Tomorrow")),
          new ListItemData((object) DateFilter.Yesterday, Utils.GetString("PublicYesterday")),
          new ListItemData((object) DateFilter.ThisWeek, Utils.GetString("ThisWeek") + SummaryControl.GetDateSpanDesc(SearchDateTextConverter.ConvertDateFilter2Span(DateFilter.ThisWeek))),
          new ListItemData((object) DateFilter.NextWeek, Utils.GetString("NextWeek") + SummaryControl.GetDateSpanDesc(SearchDateTextConverter.ConvertDateFilter2Span(DateFilter.NextWeek))),
          new ListItemData((object) DateFilter.LastWeek, Utils.GetString("LastWeek") + SummaryControl.GetDateSpanDesc(SearchDateTextConverter.ConvertDateFilter2Span(DateFilter.LastWeek))),
          new ListItemData((object) DateFilter.ThisMonth, Utils.GetString("ThisMonth")),
          new ListItemData((object) DateFilter.LastMonth, Utils.GetString("LastMonth")),
          new ListItemData((object) DateFilter.Custom, Utils.GetString("Custom"))
        }
      };
      if (this._viewModel.DateFilter == DateFilter.Custom)
      {
        timeFilterDialog.SetStartDate(this._viewModel.StartDate);
        timeFilterDialog.SetEndDate(this._viewModel.EndDate);
      }
      timeFilterDialog.ItemsSource.ForEach((Action<ListItemData>) (item =>
      {
        if ((DateFilter) item.Key != this._viewModel.DateFilter)
          return;
        item.Selected = true;
      }));
      timeFilterDialog.OnFilterSelect += (EventHandler<DateFilterData>) ((obj, item) =>
      {
        this._viewModel.DateFilter = item.Type;
        if (item.StartDate.HasValue)
          this._viewModel.StartDate = new DateTime?(item.StartDate.Value);
        if (item.EndDate.HasValue)
          this._viewModel.EndDate = new DateTime?(item.EndDate.Value);
        this.RefreshDate();
        this._viewModel.SaveTemplate();
        this.DateSelectPopup.IsOpen = false;
        UserActCollectUtils.AddClickEvent("summary", "filter", "date");
        UserActCollectUtils.AddClickEvent("summary", "time_range", this.GetDateAnalyticsLabel());
        this.SetSummary();
      });
      timeFilterDialog.OnEndEditDate += (EventHandler) (async (obj, item) => this._isEditingTime = true);
      timeFilterDialog.OnCancel += (EventHandler) ((obj, arg) => this.DateSelectPopup.IsOpen = false);
      this.DateSelectPopup.Child = (UIElement) timeFilterDialog;
      this.DateSelectPopup.IsOpen = true;
      this.DateSelectPopup.Closed += (EventHandler) (async (obj, item) =>
      {
        if (!this._isEditingTime)
          return;
        await Task.Delay(1);
        this._isEditingTime = false;
      });
    }

    private string GetDateAnalyticsLabel()
    {
      switch (this._viewModel.DateFilter)
      {
        case DateFilter.Today:
          return "today";
        case DateFilter.Tomorrow:
          return "tomorrow";
        case DateFilter.Yesterday:
          return "yesterday";
        case DateFilter.ThisWeek:
          return "this_week";
        case DateFilter.NextWeek:
          return "next_week";
        case DateFilter.LastWeek:
          return "last_week";
        case DateFilter.ThisMonth:
          return "this_month";
        case DateFilter.LastMonth:
          return "last_month";
        case DateFilter.Custom:
          DateTime? nullable = this._viewModel.StartDate;
          if (nullable.HasValue)
          {
            nullable = this._viewModel.EndDate;
            if (nullable.HasValue)
            {
              DayOfWeek dayOfWeek = DateTime.Today.DayOfWeek;
              int num = dayOfWeek != DayOfWeek.Sunday ? (int) (dayOfWeek - 1) : 6;
              DateTime dateTime1 = DateTime.Today.AddDays((double) -num);
              DateTime dateTime2 = dateTime1.AddDays(4.0);
              nullable = this._viewModel.StartDate;
              DateTime dateTime3 = dateTime1;
              if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == dateTime3 ? 1 : 0) : 1) : 0) != 0)
              {
                nullable = this._viewModel.EndDate;
                DateTime dateTime4 = dateTime2;
                if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == dateTime4 ? 1 : 0) : 1) : 0) != 0)
                  return "this_mon_to_fri_custom";
              }
              DateTime dateTime5 = DateTime.Today.AddDays((double) (-num - 7));
              DateTime dateTime6 = dateTime5.AddDays(4.0);
              nullable = this._viewModel.StartDate;
              DateTime dateTime7 = dateTime5;
              if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == dateTime7 ? 1 : 0) : 1) : 0) != 0)
              {
                nullable = this._viewModel.EndDate;
                DateTime dateTime8 = dateTime6;
                if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == dateTime8 ? 1 : 0) : 1) : 0) != 0)
                  return "last_mon_to_fri_custom";
              }
              return "other_custom";
            }
          }
          return "other_custom";
        default:
          return "other_custom";
      }
    }

    private void RefreshDate() => this._viewModel.RefreshDate();

    private void ProjectOrGroupFilterClick(object sender, MouseButtonEventArgs e)
    {
      List<string> selectedProjectIds = this._viewModel.SelectedProjectIds;
      ProjectExtra data = new ProjectExtra()
      {
        ProjectIds = selectedProjectIds,
        GroupIds = this._viewModel.SelectedProjectGroupIds
      };
      bool flag1 = CacheManager.GetSubscribeCalendars().Any<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Show != "hidden")) || CacheManager.GetBindCalendars().Any<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Show != "hidden"));
      bool flag2 = flag1 || LocalSettings.Settings.ShowHabit;
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.ProjectOrGroupFilterPopup, data, new ProjectSelectorExtra()
      {
        showAll = flag1 || LocalSettings.Settings.ShowHabit,
        showCalendarCategory = true,
        showAllProjectCategory = flag1 || LocalSettings.Settings.ShowHabit || !flag2,
        showHabitCategory = LocalSettings.Settings.ShowHabit
      });
      projectOrGroupPopup.Save += new EventHandler<ProjectExtra>(this.OnProjectSelect);
      projectOrGroupPopup.Show();
    }

    private async void OnProjectSelect(object sender, ProjectExtra data)
    {
      if (!data.IsAll)
      {
        List<string> projectIds = data.ProjectIds;
        // ISSUE: explicit non-virtual call
        if ((projectIds != null ? (__nonvirtual (projectIds.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          this._projectIds = data.ProjectIds[0];
          for (int index = 0; index < data.ProjectIds.Count; ++index)
          {
            if (index != 0)
              this._projectIds = this._projectIds + "," + data.ProjectIds[index];
          }
        }
        this._viewModel.SelectedProjectIds = data.ProjectIds;
        this._viewModel.SelectedProjectGroupIds = data.GroupIds;
        this._viewModel.SelectedProjectDisplayText = ProjectExtra.FormatDisplayText(ProjectExtra.Serialize(data));
        UserActCollectUtils.AddClickEvent("summary", "filter", "list");
      }
      else
      {
        this._projectIds = (string) null;
        this._viewModel.SelectedProjectIds = new List<string>();
        this._viewModel.SelectedProjectGroupIds = new List<string>();
        this._viewModel.SelectedProjectDisplayText = Utils.GetString("All");
      }
      this.AnalyticsContentRange();
      this._viewModel.SaveTemplate();
      this.SetSummary();
    }

    private void AnalyticsContentRange()
    {
      List<string> selectedProjectIds1 = this._viewModel.SelectedProjectIds;
      // ISSUE: explicit non-virtual call
      if ((selectedProjectIds1 != null ? (__nonvirtual (selectedProjectIds1.Count) == 0 ? 1 : 0) : 0) != 0 && this._viewModel.SelectedProjectGroupIds.Count == 0)
        UserActCollectUtils.AddClickEvent("summary", "content_range", "all");
      if (this._viewModel.SelectedProjectIds != null && this._viewModel.SelectedProjectIds.Contains("Calendar5959a2259161d16d23a4f272"))
        UserActCollectUtils.AddClickEvent("summary", "content_range", "calendar");
      if (this._viewModel.SelectedProjectIds != null && this._viewModel.SelectedProjectIds.Contains("Habit2e4c103c57ef480997943206"))
        UserActCollectUtils.AddClickEvent("summary", "content_range", "habit");
      List<string> selectedProjectIds2 = this._viewModel.SelectedProjectIds;
      if ((selectedProjectIds2 != null ? (selectedProjectIds2.Count<string>((Func<string, bool>) (it => it != "Calendar5959a2259161d16d23a4f272" && it != "Habit2e4c103c57ef480997943206")) > 0 ? 1 : 0) : 0) == 0 && this._viewModel.SelectedProjectGroupIds.Count <= 0)
        return;
      UserActCollectUtils.AddClickEvent("summary", "content_range", "list");
    }

    private void TagFilterClick(object sender, MouseButtonEventArgs e)
    {
      TagSearchFilterControl searchFilterControl = new TagSearchFilterControl(this._viewModel.SelectedTags);
      searchFilterControl.Cancel -= new EventHandler(this.OnTagFilterCancel);
      searchFilterControl.Cancel += new EventHandler(this.OnTagFilterCancel);
      searchFilterControl.Save -= new EventHandler<List<string>>(this.OnTagFilterSelected);
      searchFilterControl.Save += new EventHandler<List<string>>(this.OnTagFilterSelected);
      this.TagFilterPopup.Child = (UIElement) searchFilterControl;
      this.TagFilterPopup.IsOpen = true;
    }

    private void OnTagFilterSelected(object sender, List<string> tags)
    {
      this.TagFilterPopup.IsOpen = false;
      this._viewModel.SelectedTags = tags;
      this._viewModel.SelectedTagDisplayText = TagCardViewModel.ToNormalDisplayText(tags);
      this._viewModel.SaveTemplate();
      UserActCollectUtils.AddClickEvent("summary", "filter", "tag");
      this.SetSummary();
    }

    private void OnTagFilterCancel(object sender, EventArgs e)
    {
      this.TagFilterPopup.IsOpen = false;
    }

    private void StatusFilterClick(object sender, MouseButtonEventArgs e)
    {
      TaskStatusTypeEditDialog statusTypeEditDialog = new TaskStatusTypeEditDialog((ICollection<string>) this._viewModel.SelectedStatus);
      statusTypeEditDialog.OnCancel += (EventHandler) ((s, arg) => this.StatusFilterPopup.IsOpen = false);
      statusTypeEditDialog.OnSave += (EventHandler<ticktick_WPF.ViewModels.FilterConditionViewModel>) ((s, model) =>
      {
        List<string> list = model.ItemsSource.Where<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (p => p.Value != (object) "all" && p.Selected)).Select<FilterListItemViewModel, string>((Func<FilterListItemViewModel, string>) (p => p.Value as string)).ToList<string>();
        this._viewModel.SelectedStatus = list.Count == 4 ? new List<string>() : list;
        this._viewModel.SelectedStatusDisplayText = TaskStatusTypeEditDialog.FormatDisplayText(this._viewModel.SelectedStatus);
        this._viewModel.SaveTemplate();
        UserActCollectUtils.AddClickEvent("summary", "filter", "status");
        this.StatusFilterPopup.IsOpen = false;
        this.SetSummary();
      });
      this.StatusFilterPopup.Child = (UIElement) statusTypeEditDialog;
      this.StatusFilterPopup.IsOpen = true;
    }

    private void PriorityFilterClick(object sender, MouseButtonEventArgs e)
    {
      TaskPriorityTypeEditDialog priorityTypeEditDialog = new TaskPriorityTypeEditDialog((ICollection<string>) this._viewModel.SelectedPriorities);
      priorityTypeEditDialog.OnCancel += (EventHandler) ((s, arg) => this.PriorityFilterPopup.IsOpen = false);
      priorityTypeEditDialog.OnSave += (EventHandler<ticktick_WPF.ViewModels.FilterConditionViewModel>) ((s, model) =>
      {
        List<string> list = model.ItemsSource.Where<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (p => p.Value != (object) "all" && p.Selected)).Select<FilterListItemViewModel, string>((Func<FilterListItemViewModel, string>) (p => p.Value as string)).ToList<string>();
        this._viewModel.SelectedPriorities = list.Count == 4 ? new List<string>() : list;
        this._viewModel.SelectedPriorityDisplayText = TaskPriorityTypeEditDialog.FormatDisplayText(this._viewModel.SelectedPriorities);
        this._viewModel.SaveTemplate();
        UserActCollectUtils.AddClickEvent("summary", "filter", "priority");
        this.PriorityFilterPopup.IsOpen = false;
        this.SetSummary();
      });
      this.PriorityFilterPopup.Child = (UIElement) priorityTypeEditDialog;
      this.PriorityFilterPopup.IsOpen = true;
    }

    private void AssignFilterClick(object sender, MouseButtonEventArgs e)
    {
      this._assignEditDialog = new AssigneeEditDialog(this._viewModel.Assignees ?? new List<string>(), true);
      this._assignEditDialog.OnSelectedAssigneeChanged += (EventHandler<List<string>>) ((s, assignees) => this._viewModel.Assignees = assignees);
      this.AssignFilterPopup.Closed -= new EventHandler(this.AssigneePopup_Closed);
      this.AssignFilterPopup.Closed += new EventHandler(this.AssigneePopup_Closed);
      this.AssignFilterPopup.Child = (UIElement) this._assignEditDialog;
      this._assignEditDialog.OnCancel += (EventHandler) ((s, arg) => this.AssignFilterPopup.IsOpen = false);
      this._assignEditDialog.OnSave += (EventHandler<ticktick_WPF.ViewModels.FilterConditionViewModel>) ((s, model) =>
      {
        this._viewModel.SaveTemplate();
        this.AssignFilterPopup.IsOpen = false;
        UserActCollectUtils.AddClickEvent("summary", "filter", "assignee");
        this.SetSummary();
      });
      this.AssignFilterPopup.IsOpen = true;
    }

    private void AssigneePopup_Closed(object sender, EventArgs e)
    {
      if (!this._assignEditDialog.IsSave)
        this._assignEditDialog.Restore();
      this._assignEditDialog.IsSave = false;
    }

    private async void MenuGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      SummaryControl child = this;
      e.Handled = true;
      await Task.Delay(1);
      Utils.FindParent<ListViewContainer>((DependencyObject) child)?.ShowProjectMenu();
    }

    public async void LoadData()
    {
      SummaryControl summaryControl = this;
      if (summaryControl._viewModel == null)
        return;
      summaryControl._viewModel.LoadTemplate();
      summaryControl.RefreshDate();
      summaryControl.DisplayFilter.FilterChanged -= new EventHandler(summaryControl.SetSummary);
      summaryControl.DisplayFilter.InitModel(summaryControl._viewModel);
      summaryControl.DisplayFilter.FilterChanged += new EventHandler(summaryControl.SetSummary);
      summaryControl.SetSummary();
    }

    public bool RemoveInputFocus() => !this.SummaryContent.IsMouseOver;

    private void OnCopyClick(object sender, RoutedEventArgs e) => this.CopyPopup.IsOpen = true;

    private void SaveImage()
    {
      string outputPath = "";
      SaveFileDialog saveFileDialog1 = new SaveFileDialog();
      saveFileDialog1.FileName = Utils.GetString("Summary") + "(" + this.GetTitle(false) + ")";
      saveFileDialog1.Filter = "Png Files|*.png";
      SaveFileDialog saveFileDialog2 = saveFileDialog1;
      if (saveFileDialog2.ShowDialog() == DialogResult.OK)
        outputPath = saveFileDialog2.FileName;
      if (LocalSettings.Settings.ThemeId == "Dark")
      {
        this.SummaryContent.SetTheme();
        this.SummaryContent.Background = (Brush) Brushes.White;
        this.SummaryContent.UpdateLayout();
        Utils.GetPicFromControl((FrameworkElement) this.SummaryContent, "png", outputPath);
        this.SummaryContent.ClearTheme();
        this.SummaryContent.Background = (Brush) Brushes.Transparent;
      }
      else
      {
        this.SummaryContent.Background = (Brush) Brushes.White;
        this.SummaryContent.UpdateLayout();
        Utils.GetPicFromControl((FrameworkElement) this.SummaryContent, "png", outputPath);
        this.SummaryContent.Background = (Brush) Brushes.Transparent;
      }
    }

    private void OnContentLostFocus(object sender, RoutedEventArgs e)
    {
      this.SummaryContent.UnRegisterInputHandler();
    }

    private void OnContentGotFocus(object sender, RoutedEventArgs e)
    {
      this.SummaryContent.RegisterInputHandler();
    }

    private void EditorOnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    private void OnMouseScroll(object sender, MouseWheelEventArgs e)
    {
      this.ContentScrollViewer.ScrollToVerticalOffset(this.ContentScrollViewer.VerticalOffset - (double) e.Delta / 2.0);
    }

    private void OnContentClick(object sender, MouseButtonEventArgs e)
    {
      this.SummaryContent.FocusEditBox();
    }

    private void OnClearClick(object sender, MouseButtonEventArgs e)
    {
      this._projectIds = (string) null;
      this._viewModel.DateFilter = DateFilter.ThisWeek;
      this._viewModel.SelectedProjectIds = new List<string>()
      {
        "ProjectAll2e4c103c57ef480997943206"
      };
      this._viewModel.SelectedProjectGroupIds = new List<string>();
      this._viewModel.SelectedProjectDisplayText = Utils.GetString("AllList");
      this._viewModel.SelectedTags = new List<string>();
      this._viewModel.SelectedTagDisplayText = Utils.GetString("AllTags");
      this._viewModel.Assignees = new List<string>();
      this._viewModel.SelectedAssignTypeText = Utils.GetString("AssignTo");
      this._viewModel.SelectedStatus = new List<string>();
      this._viewModel.SelectedPriorities = new List<string>();
      this._viewModel.SelectedPriorityDisplayText = Utils.GetString("all_priorities");
      this._viewModel.SelectedStatusDisplayText = Utils.GetString("AllStatus");
      this._viewModel.SaveTemplate();
      this.ClearFilterGrid.Visibility = Visibility.Collapsed;
      this.RefreshDate();
      this.SetSummary();
    }

    private void OnTemplateClick(object sender, MouseButtonEventArgs e)
    {
      this.CopyPopup.IsOpen = false;
      this.ExportPopup.IsOpen = false;
      if (!UserDao.IsPro())
        ProChecker.ShowUpgradeDialog(ProType.SummaryTemplate);
      else if (this._viewModel.SelectedTemplateId == "defaultId" && this._viewModel.SummaryTemplates.Count > 9)
      {
        Utils.Toast(Utils.GetString("summary_template_limit_hint"));
      }
      else
      {
        UserActCollectUtils.AddClickEvent("summary", "action", "save_template");
        this.ShowSaveTemplateDialog();
      }
    }

    private void ShowSaveTemplateDialog()
    {
      bool replace = this._viewModel.SelectedTemplateId != "defaultId";
      List<string> list = this._viewModel.SummaryTemplates.Select<SummaryTemplate, string>((Func<SummaryTemplate, string>) (it => it.name)).ToList<string>();
      EditTemplateWindow editTemplateWindow1 = new EditTemplateWindow(!replace ? string.Empty : this._viewModel.Name, list, true, Utils.GetString(replace ? "would_you_replace_template" : "save_template_desc"), true);
      editTemplateWindow1.Title = Utils.GetString(replace ? "replace_template" : "SaveAsTemplate");
      editTemplateWindow1.Owner = Window.GetWindow((DependencyObject) this);
      EditTemplateWindow editTemplateWindow2 = editTemplateWindow1;
      if (replace)
        editTemplateWindow2.OkButton.Content = (object) Utils.GetString("replace_action");
      editTemplateWindow2.Save += (EventHandler<string>) ((sender, title) =>
      {
        if (replace)
          this._viewModel.SaveTemplate(title, true);
        else
          this._viewModel.AddTemplate(title, this._viewModel);
        this.ShowTemplateTooltip();
        this.LoadSummaryData();
      });
      editTemplateWindow2.ShowDialog();
    }

    private async void ShowTemplateTooltip()
    {
      ExtraSettings settings = LocalSettings.Settings.ExtraSettings;
      if (settings.SummaryTemplateTooltipShown)
      {
        settings = (ExtraSettings) null;
      }
      else
      {
        System.Windows.Controls.ToolTip toolTip = new System.Windows.Controls.ToolTip();
        toolTip.Content = (object) Utils.GetString("summary_template_hint");
        toolTip.PlacementTarget = (UIElement) this.TemplateName;
        toolTip.Placement = PlacementMode.Bottom;
        toolTip.IsOpen = true;
        System.Windows.Controls.ToolTip templateTip = toolTip;
        templateTip.IsOpen = true;
        await Task.Delay(3000);
        templateTip.IsOpen = false;
        settings.SummaryTemplateTooltipShown = true;
        LocalSettings.Settings.Save();
        templateTip = (System.Windows.Controls.ToolTip) null;
        settings = (ExtraSettings) null;
      }
    }

    private void OnCopyTypeClick(object sender, MouseButtonEventArgs e)
    {
      this.CopyPopup.IsOpen = false;
      this.ExportPopup.IsOpen = false;
      if (!(sender is StackPanel stackPanel) || !(stackPanel.Tag is string tag))
        return;
      string text = this.SummaryContent.Text;
      switch (tag)
      {
        case "email":
        case "richtext":
          string html1 = HtmlConverter.MdToHtml(this.SummaryContent.HandleTabs());
          if (html1 != null)
          {
            HtmlConverter.Html2Rtf(html1);
            Utils.Toast(Utils.GetString("Copied"));
          }
          else
            Utils.Toast("error");
          if (tag == "email")
          {
            try
            {
              Process.Start("mailto:");
            }
            catch (Exception ex)
            {
            }
            UserActCollectUtils.AddClickEvent("summary", "action", "save_email");
            break;
          }
          UserActCollectUtils.AddClickEvent("summary", "action", "copy_rich_text");
          break;
        case "pdf":
          PrintPreviewWindow printPreviewWindow = new PrintPreviewWindow(text);
          UserActCollectUtils.AddClickEvent("summary", "action", "save_pdf");
          printPreviewWindow.Show();
          break;
        case "png":
          UserActCollectUtils.AddClickEvent("summary", "action", "save_image");
          this.SaveImage();
          break;
        default:
          string str = this.SummaryContent.HandleTabs();
          string html2 = HtmlConverter.MdToHtml(str);
          if (tag == "plaintext" && html2 != null)
          {
            UserActCollectUtils.AddClickEvent("summary", "action", "copy_plain_text");
            str = HtmlConverter.ConvertToPlainText(html2).Trim();
          }
          else
            UserActCollectUtils.AddClickEvent("summary", "action", "copy_md");
          if (string.IsNullOrEmpty(str))
            break;
          System.Windows.Clipboard.Clear();
          try
          {
            System.Windows.Forms.Clipboard.Clear();
            System.Windows.Forms.Clipboard.SetDataObject((object) str, false, 8, 250);
            Utils.Toast(Utils.GetString("Copied"));
            break;
          }
          catch (Exception ex)
          {
            break;
          }
      }
    }

    private void OnExportClick(object sender, RoutedEventArgs e) => this.ExportPopup.IsOpen = true;

    private void OnInsertClick(object sender, RoutedEventArgs e)
    {
      EventHandler<string> insertSummary = this.InsertSummary;
      if (insertSummary == null)
        return;
      insertSummary((object) this, this.SummaryContent.Text);
    }

    public void SetInsertMode()
    {
      this.InsertButton.Visibility = Visibility.Visible;
      this.TextEditorMenu.Visibility = Visibility.Collapsed;
      this.SpliteLine.Visibility = Visibility.Collapsed;
      this.CopyButton.Visibility = Visibility.Collapsed;
      this.ExportButton.Visibility = Visibility.Collapsed;
      this.HideFoldMenuIcon();
      this.ContentGrid.Margin = new Thickness(10.0);
    }

    public void SetFoldMenuIcon(bool hideMenu)
    {
      this.FoldImage.SetResourceReference(Image.SourceProperty, hideMenu ? (object) "ShowMenuDrawingImage" : (object) "HideMenuDrawingImage");
    }

    public void HideFoldMenuIcon() => this.MenuPathGrid.Visibility = Visibility.Collapsed;

    private void OnShowMenuMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      Utils.FindParent<ListViewContainer>((DependencyObject) this)?.TryShowMenuOnHover((UIElement) this.MenuPathGrid);
    }

    private void OnTemplateToggleClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._viewModel.ShowTemplate)
        return;
      this.TemplatesItems.ItemsSource = (IEnumerable) new ObservableCollection<SummaryTemplateViewModel>(this._viewModel.SummaryTemplates.Select<SummaryTemplate, SummaryTemplateViewModel>((Func<SummaryTemplate, SummaryTemplateViewModel>) (it => new SummaryTemplateViewModel()
      {
        Id = it.id,
        Name = it.name,
        IsSelected = this._viewModel.SelectedTemplateId == it.id
      })));
      this.ChooseTemplatePopup.IsOpen = true;
    }

    private void OnSelectTemplateClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      SummaryTemplateViewModel dataContext = (SummaryTemplateViewModel) frameworkElement.DataContext;
      dataContext.IsSelected = true;
      LocalSettings.Settings.ExtraSettings.SelectedSummaryTemplateId = dataContext.Id;
      LocalSettings.Settings.Save();
      this.ChooseTemplatePopup.IsOpen = false;
      this.LoadSummaryData();
    }

    private void MoreFilterClick(object sender, MouseButtonEventArgs e)
    {
      this.MoreConditionPopup.ItemsSource = (IEnumerable) new ObservableCollection<FilterConditionViewModel>(this._viewModel.MoreConditions.Select<string, FilterConditionViewModel>((Func<string, FilterConditionViewModel>) (it => new FilterConditionViewModel()
      {
        Id = it,
        Name = Utils.GetString(it)
      })));
      this.MoreFilterPopup.IsOpen = true;
    }

    private void OnFilterConditionClick(object sender, MouseButtonEventArgs e)
    {
      this.MoreFilterPopup.IsOpen = false;
      if (!(sender is FrameworkElement frameworkElement))
        return;
      string id = frameworkElement.DataContext is FilterConditionViewModel dataContext ? dataContext.Id : (string) null;
      List<string> moreConditions = this._viewModel.MoreConditions;
      moreConditions.Remove(id);
      this._viewModel.MoreConditions = moreConditions;
    }

    private void OnTemplateOptionClick(object sender, MouseButtonEventArgs e)
    {
      this.TemplateOptionPopup.PlacementTarget = sender as UIElement;
      this.TemplateOptionPopup.Placement = PlacementMode.Right;
      this.TemplateOptionPopup.IsOpen = true;
      if (!(sender is FrameworkElement frameworkElement))
        return;
      this._editingTemplate = frameworkElement.DataContext as SummaryTemplateViewModel;
    }

    private void OnTemplateEditClick(object s, MouseButtonEventArgs e)
    {
      this.TemplateOptionPopup.IsOpen = false;
      if (this._editingTemplate == null)
        return;
      EditTemplateWindow editTemplateWindow = new EditTemplateWindow(this._editingTemplate.Name, this._viewModel.SummaryTemplates.Select<SummaryTemplate, string>((Func<SummaryTemplate, string>) (it => it.name)).ToList<string>(), true, string.Empty, true);
      editTemplateWindow.Title = Utils.GetString("edit_template");
      editTemplateWindow.Owner = Window.GetWindow((DependencyObject) this);
      editTemplateWindow.Save += (EventHandler<string>) ((sender, title) =>
      {
        this._viewModel.SaveTemplate(this._editingTemplate, (Action<SummaryTemplate>) (template => template.name = title));
        this.LoadSummaryData();
      });
      editTemplateWindow.ShowDialog();
    }

    private void OnTemplateDeleteClick(object sender, MouseButtonEventArgs e)
    {
      this.DeleteTemplate();
    }

    private void DeleteTemplate()
    {
      if (this._editingTemplate != null)
        this._viewModel.DeleteTemplate(this._editingTemplate.Id);
      this.TemplatesItems.ItemsSource = (IEnumerable) new ObservableCollection<SummaryTemplateViewModel>(this._viewModel.SummaryTemplates.Select<SummaryTemplate, SummaryTemplateViewModel>((Func<SummaryTemplate, SummaryTemplateViewModel>) (it => new SummaryTemplateViewModel()
      {
        Id = it.id,
        Name = it.name,
        IsSelected = this._viewModel.SelectedTemplateId == it.id
      })));
      this.TemplateOptionPopup.IsOpen = false;
      this.ChooseTemplatePopup.IsOpen = false;
      this.LoadSummaryData();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/summary/summarycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Container = (Grid) target;
          break;
        case 2:
          this.TitlePanel = (StackPanel) target;
          break;
        case 3:
          this.MenuPathGrid = (Border) target;
          this.MenuPathGrid.MouseEnter += new System.Windows.Input.MouseEventHandler(this.OnShowMenuMouseEnter);
          this.MenuPathGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.MenuGrid_MouseLeftButtonUp);
          break;
        case 4:
          this.FoldImage = (Image) target;
          break;
        case 5:
          this.TemplateName = (TextBlock) target;
          this.TemplateName.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTemplateToggleClick);
          break;
        case 6:
          this.ArrowGrid = (Border) target;
          this.ArrowGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTemplateToggleClick);
          break;
        case 7:
          this.ChooseTemplatePopup = (EscPopup) target;
          break;
        case 8:
          this.TemplatesItems = (ItemsControl) target;
          break;
        case 11:
          this.TemplateOptionPopup = (EscPopup) target;
          break;
        case 12:
          this.DateFilterText = (PopupPlacementBorder) target;
          break;
        case 13:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.DateFilterClick);
          break;
        case 14:
          this.DateSelectPopup = (EscPopup) target;
          break;
        case 15:
          this.ProjectOrGroupFilterText = (PopupPlacementBorder) target;
          break;
        case 16:
          this.ProjectOrGroupFilterPopup = (EscPopup) target;
          break;
        case 17:
          this.TagFilterText = (PopupPlacementBorder) target;
          break;
        case 18:
          this.TagFilterPopup = (EscPopup) target;
          break;
        case 19:
          this.StatusFilter = (PopupPlacementBorder) target;
          break;
        case 20:
          this.StatusFilterPopup = (EscPopup) target;
          break;
        case 21:
          this.PriorityFilter = (PopupPlacementBorder) target;
          break;
        case 22:
          this.PriorityFilterPopup = (EscPopup) target;
          break;
        case 23:
          this.AssignFilter = (PopupPlacementBorder) target;
          break;
        case 24:
          this.AssignFilterPopup = (EscPopup) target;
          break;
        case 25:
          this.MoreFilter = (PopupPlacementBorder) target;
          break;
        case 26:
          this.MoreFilterPopup = (EscPopup) target;
          break;
        case 27:
          this.MoreConditionPopup = (ItemsControl) target;
          break;
        case 28:
          this.ClearFilterGrid = (Grid) target;
          this.ClearFilterGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClearClick);
          break;
        case 29:
          this.DisplayFilter = (SummaryDisplayFilterControl) target;
          break;
        case 30:
          this.TextEditorMenu = (EditorMenu) target;
          break;
        case 31:
          this.SpliteLine = (Grid) target;
          break;
        case 32:
          this.ContentGrid = (Grid) target;
          break;
        case 33:
          this.ContentScrollViewer = (ScrollViewer) target;
          this.ContentScrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(this.OnMouseScroll);
          break;
        case 34:
          this.MdGrid = (Grid) target;
          this.MdGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnContentClick);
          break;
        case 35:
          this.SummaryContent = (MarkDownEditor) target;
          break;
        case 36:
          this.CopyButton = (System.Windows.Controls.Button) target;
          this.CopyButton.Click += new RoutedEventHandler(this.OnCopyClick);
          break;
        case 37:
          this.ExportButton = (System.Windows.Controls.Button) target;
          this.ExportButton.Click += new RoutedEventHandler(this.OnExportClick);
          break;
        case 38:
          this.InsertButton = (System.Windows.Controls.Button) target;
          this.InsertButton.Click += new RoutedEventHandler(this.OnInsertClick);
          break;
        case 39:
          this.CopyPopup = (Popup) target;
          break;
        case 40:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCopyTypeClick);
          break;
        case 41:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCopyTypeClick);
          break;
        case 42:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCopyTypeClick);
          break;
        case 43:
          this.ExportPopup = (Popup) target;
          break;
        case 44:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTemplateClick);
          break;
        case 45:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCopyTypeClick);
          break;
        case 46:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCopyTypeClick);
          break;
        case 47:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCopyTypeClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 9)
      {
        if (connectionId != 10)
          return;
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSelectTemplateClick);
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTemplateOptionClick);
    }
  }
}
