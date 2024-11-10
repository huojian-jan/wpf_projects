// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.Display.TaskDisplayService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Completed;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Service.Display
{
  public static class TaskDisplayService
  {
    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInAll()
    {
      List<TaskBaseViewModel> tasks = TaskViewModelHelper.GetModel(inAll: true, withChild: true);
      List<TaskBaseViewModel> taskBaseViewModelList = tasks;
      DateTime date = DateTime.Now.Date;
      DateTime dateTime = DateTime.Now;
      dateTime = dateTime.Date;
      DateTime due = dateTime.AddDays(90.0);
      taskBaseViewModelList.AddRange((IEnumerable<TaskBaseViewModel>) (await TaskDisplayService.GetCalendarEventsInSpan(date, due)).Item2);
      taskBaseViewModelList = (List<TaskBaseViewModel>) null;
      List<TaskBaseViewModel> displayTaskInAll = tasks;
      tasks = (List<TaskBaseViewModel>) null;
      return displayTaskInAll;
    }

    public static async Task<(int, List<TaskBaseViewModel>)> GetCalendarEventsInSpan(
      DateTime start,
      DateTime due,
      bool onlyCount = false)
    {
      (int, List<TaskBaseViewModel>) modelsBetweenSpan = await CalendarEventDao.GetTaskDisplayModelsBetweenSpan(start, due, onlyCount: onlyCount);
      return (modelsBetweenSpan.Item1, modelsBetweenSpan.Item2);
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInToday()
    {
      List<TaskBaseViewModel> tasks = TaskViewModelHelper.GetModel(startEndPairs: new List<FilterDatePair>()
      {
        new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(1.0)))
      }, showCheckItem: (LocalSettings.Settings.ShowSubtasks ? 1 : 0) != 0, inTodayOrWeek: true, inAll: true, withChild: true);
      tasks.AddRange((IEnumerable<TaskBaseViewModel>) await TaskDisplayService.GetCalendarEventsInToday());
      tasks.AddRange((IEnumerable<TaskBaseViewModel>) TaskDisplayService.SortHabitDisplayModels(await TaskDisplayService.GetHabitsInToday()));
      if (LocalSettings.Settings.UserPreference?.TimeTable?.ShowInSmart.GetValueOrDefault())
        tasks.AddRange((IEnumerable<TaskBaseViewModel>) await TaskDisplayService.GetCourseViewModelsInSpan(DateTime.Today, DateTime.Today.AddDays(1.0)));
      List<TaskBaseViewModel> displayTaskInToday = tasks;
      tasks = (List<TaskBaseViewModel>) null;
      return displayTaskInToday;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInDate(string dateStamp)
    {
      DateTime exact = DateTime.ParseExact(dateStamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture);
      List<TaskBaseViewModel> tasks = TaskViewModelHelper.GetModel(startEndPairs: new List<FilterDatePair>()
      {
        new FilterDatePair(new DateTime?(exact), new DateTime?(exact.AddDays(1.0)))
      }, showCheckItem: (LocalSettings.Settings.ShowSubtasks ? 1 : 0) != 0, inAll: true, withChild: true);
      tasks.AddRange((IEnumerable<TaskBaseViewModel>) (await TaskDisplayService.GetCalendarEventsInSpan(exact, exact.AddDays(1.0))).Item2);
      List<TaskBaseViewModel> displayTaskInDate = tasks;
      tasks = (List<TaskBaseViewModel>) null;
      return displayTaskInDate;
    }

    public static List<HabitBaseViewModel> SortHabitDisplayModels(List<HabitBaseViewModel> models)
    {
      if (models.Count > 1)
      {
        Dictionary<string, HabitSectionModel> sectionDict = HabitSectionCache.GetSectionDict();
        models.Sort((Comparison<HabitBaseViewModel>) ((left, right) =>
        {
          long habitSectionOrder1 = GetHabitSectionOrder(left.Habit);
          long habitSectionOrder2 = GetHabitSectionOrder(right.Habit);
          return habitSectionOrder1 != habitSectionOrder2 ? habitSectionOrder1.CompareTo(habitSectionOrder2) : left.SortOrder.CompareTo(right.SortOrder);
        }));

        long GetHabitSectionOrder(HabitModel habit)
        {
          return habit == null || habit.SectionId == null || !sectionDict.ContainsKey(habit.SectionId) ? LocalSettings.Settings.HabitDefaultOrder : sectionDict[habit.SectionId].SortOrder;
        }
      }
      return models;
    }

    public static async Task<List<HabitBaseViewModel>> GetHabitsInToday()
    {
      List<HabitBaseViewModel> result = new List<HabitBaseViewModel>();
      if (LocalSettings.Settings.ShowHabit && LocalSettings.Settings.HabitInToday)
      {
        List<HabitModel> habits = await HabitDao.GetNeedCheckHabits();
        List<HabitCheckInModel> monthCheckIns = await HabitCheckInDao.GetCheckInsInSpan(DateTime.Today.AddDays(-30.0), DateTime.Today.AddDays(1.0));
        if (habits != null && habits.Any<HabitModel>())
        {
          foreach (HabitModel habitModel in habits)
          {
            HabitModel habit = habitModel;
            List<HabitCheckInModel> list = monthCheckIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id)).ToList<HabitCheckInModel>();
            HabitBaseViewModel model = new HabitBaseViewModel(habit);
            HabitCheckInModel habitCheckInModel = list.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (v => v.HabitId == habit.Id && v.CheckinStamp == DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)));
            if (habitCheckInModel != null)
            {
              model.HabitCheckIn = habitCheckInModel;
              if (habitCheckInModel.Value >= habitCheckInModel.Goal || habitCheckInModel.CheckStatus == 1)
              {
                model.CompletedTime = habitCheckInModel.CheckinTime;
                model.Status = 2;
              }
            }
            bool flag = model.Status != 0;
            if (!flag)
              flag = await HabitUtils.IsHabitValidInToday(habit, list);
            if (flag)
              result.Add(model);
            model = (HabitBaseViewModel) null;
          }
        }
        habits = (List<HabitModel>) null;
        monthCheckIns = (List<HabitCheckInModel>) null;
      }
      List<HabitBaseViewModel> habitsInToday = result;
      result = (List<HabitBaseViewModel>) null;
      return habitsInToday;
    }

    public static async Task<List<TaskBaseViewModel>> GetCalendarEventsInToday()
    {
      DateTime dateTime = DateTime.Now;
      DateTime date = dateTime.Date;
      dateTime = DateTime.Now;
      dateTime = dateTime.Date;
      DateTime due = dateTime.AddDays(1.0);
      return (await TaskDisplayService.GetCalendarEventsInSpan(date, due)).Item2;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInTomorrow()
    {
      List<FilterDatePair> startEndPairs = new List<FilterDatePair>();
      DateTime today = DateTime.Today;
      DateTime? start1 = new DateTime?(today.AddDays(1.0));
      today = DateTime.Today;
      DateTime? end1 = new DateTime?(today.AddDays(2.0));
      startEndPairs.Add(new FilterDatePair(start1, end1));
      List<TaskBaseViewModel> tasks = TaskViewModelHelper.GetModel(startEndPairs: startEndPairs, showCheckItem: (LocalSettings.Settings.ShowSubtasks ? 1 : 0) != 0, inAll: true, withChild: true);
      tasks.AddRange((IEnumerable<TaskBaseViewModel>) await TaskDisplayService.GetCalendarEventsInTomorrow());
      TimeTableModel timeTable = LocalSettings.Settings.UserPreference.TimeTable;
      if ((timeTable != null ? (timeTable.ShowInSmart ? 1 : 0) : 0) != 0)
      {
        today = DateTime.Today;
        DateTime start2 = today.AddDays(1.0);
        today = DateTime.Today;
        DateTime end2 = today.AddDays(2.0);
        tasks.AddRange((IEnumerable<TaskBaseViewModel>) await TaskDisplayService.GetCourseViewModelsInSpan(start2, end2));
      }
      List<TaskBaseViewModel> displayTaskInTomorrow = tasks;
      tasks = (List<TaskBaseViewModel>) null;
      return displayTaskInTomorrow;
    }

    public static async Task<List<TaskBaseViewModel>> GetCourseViewModelsInSpan(
      DateTime start,
      DateTime end)
    {
      return (await ScheduleService.GetCoursesInSpan(start, end, true)).Select<CourseDisplayModel, TaskBaseViewModel>((Func<CourseDisplayModel, TaskBaseViewModel>) (c => new TaskBaseViewModel(c))).ToList<TaskBaseViewModel>();
    }

    public static async Task<List<TaskBaseViewModel>> GetCalendarEventsInTomorrow()
    {
      DateTime dateTime = DateTime.Now;
      dateTime = dateTime.Date;
      DateTime start = dateTime.AddDays(1.0);
      dateTime = DateTime.Now;
      dateTime = dateTime.Date;
      DateTime due = dateTime.AddDays(2.0);
      return (await TaskDisplayService.GetCalendarEventsInSpan(start, due)).Item2;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInWeek()
    {
      List<TaskBaseViewModel> tasks = TaskViewModelHelper.GetModel(startEndPairs: new List<FilterDatePair>()
      {
        new FilterDatePair(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today.AddDays(7.0)))
      }, showCheckItem: (LocalSettings.Settings.ShowSubtasks ? 1 : 0) != 0, inTodayOrWeek: true, inAll: true, withChild: true);
      List<TaskBaseViewModel> events = await TaskDisplayService.GetCalendarEventsInWeek();
      List<HabitBaseViewModel> collection = TaskDisplayService.SortHabitDisplayModels(await TaskDisplayService.GetHabitsInToday());
      tasks.AddRange((IEnumerable<TaskBaseViewModel>) events);
      tasks.AddRange((IEnumerable<TaskBaseViewModel>) collection);
      TimeTableModel timeTable = LocalSettings.Settings.UserPreference.TimeTable;
      if ((timeTable != null ? (timeTable.ShowInSmart ? 1 : 0) : 0) != 0)
        tasks.AddRange((IEnumerable<TaskBaseViewModel>) await TaskDisplayService.GetCourseViewModelsInSpan(DateTime.Today, DateTime.Today.AddDays(7.0)));
      List<TaskBaseViewModel> displayTaskInWeek = tasks;
      tasks = (List<TaskBaseViewModel>) null;
      events = (List<TaskBaseViewModel>) null;
      return displayTaskInWeek;
    }

    public static async Task<List<TaskBaseViewModel>> GetCalendarEventsInWeek()
    {
      DateTime dateTime = DateTime.Now;
      DateTime date = dateTime.Date;
      dateTime = DateTime.Now;
      dateTime = dateTime.Date;
      DateTime due = dateTime.AddDays(7.0);
      return (await TaskDisplayService.GetCalendarEventsInSpan(date, due)).Item2;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInTag(string tag)
    {
      return TaskViewModelHelper.GetModel(tags: CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.name == tag || t.parent == tag)).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>(), showComplete: !LocalSettings.Settings.HideComplete, withChild: true);
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInSearch(
      string searchKey,
      SearchFilterViewModel filter,
      List<string> tags = null,
      bool local = false)
    {
      if (string.IsNullOrEmpty(searchKey))
      {
        List<string> stringList = tags;
        // ISSUE: explicit non-virtual call
        if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) == 0)
          goto label_33;
      }
      SearchFilterModel filterModel = filter.ToSearchFilter();
      List<TaskSearchModel> taskSearchModels = await SearchHelper.GetTaskSearchModels(filterModel, tags);
      searchKey = searchKey?.Trim().ToLower();
      if (!filter.Searched && !local)
      {
        SearchHelper.SearchFilter.Searched = true;
        SearchHelper.TrySearchFromService(searchKey, filterModel);
      }
      string str = searchKey;
      List<string> stringList1;
      if (str == null)
        stringList1 = (List<string>) null;
      else
        stringList1 = ((IEnumerable<string>) str.Split(' ')).ToList<string>();
      List<string> stringList2 = stringList1;
      stringList2?.Remove("");
      if (taskSearchModels.Count > 0)
      {
        List<TaskSearchModel> source = new List<TaskSearchModel>();
        HashSet<string> stringSet = (HashSet<string>) null;
        if (string.IsNullOrEmpty(searchKey))
          return TaskDisplayService.GetSearchModels(taskSearchModels.Select<TaskSearchModel, TaskBaseViewModel>((Func<TaskSearchModel, TaskBaseViewModel>) (m => m.SourceModel)).ToList<TaskBaseViewModel>(), new List<TaskBaseViewModel>());
        List<TaskSearchModel> list = taskSearchModels.Where<TaskSearchModel>((Func<TaskSearchModel, bool>) (item => item.Contains(searchKey))).ToList<TaskSearchModel>();
        if (list.Any<TaskSearchModel>())
          stringSet = new HashSet<string>((IEnumerable<string>) list.Select<TaskSearchModel, string>((Func<TaskSearchModel, string>) (m => m.Id)).ToList<string>());
        if (stringList2.Count > 0)
        {
          foreach (TaskSearchModel taskSearchModel in taskSearchModels)
          {
            if (stringSet == null || !stringSet.Contains(taskSearchModel.Id))
            {
              string text = taskSearchModel.GetText();
              bool flag = true;
              foreach (string old in stringList2)
              {
                if (text.Contains(old))
                {
                  Utils.ReplaceFirst(ref text, old, "");
                }
                else
                {
                  flag = false;
                  break;
                }
              }
              if (flag)
                source.Add(taskSearchModel);
            }
          }
        }
        if (list.Count > 0 || source.Count > 0)
          return TaskDisplayService.GetSearchModels(list.Select<TaskSearchModel, TaskBaseViewModel>((Func<TaskSearchModel, TaskBaseViewModel>) (m => m.SourceModel)).ToList<TaskBaseViewModel>(), source.Select<TaskSearchModel, TaskBaseViewModel>((Func<TaskSearchModel, TaskBaseViewModel>) (m => m.SourceModel)).ToList<TaskBaseViewModel>());
      }
      filterModel = (SearchFilterModel) null;
label_33:
      return new List<TaskBaseViewModel>();
    }

    public static List<TaskBaseViewModel> GetSearchModels(
      List<TaskBaseViewModel> exactSearchTasks,
      List<TaskBaseViewModel> fussySearchTasks)
    {
      HashSet<string> exactSearchIds = new HashSet<string>(exactSearchTasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)));
      List<TaskBaseViewModel> searchModels1 = exactSearchTasks;
      searchModels1.AddRange((IEnumerable<TaskBaseViewModel>) fussySearchTasks);
      searchModels1.Sort((Comparison<TaskBaseViewModel>) ((a, b) =>
      {
        if (a.Status == 0 && b.Status != 0)
          return -1;
        if (a.Status != 0 && b.Status == 0)
          return 1;
        if (exactSearchIds.Contains(a.Id) && !exactSearchIds.Contains(b.Id))
          return -1;
        if (!exactSearchIds.Contains(a.Id) && exactSearchIds.Contains(b.Id))
          return 1;
        if (a.Status == 0 && b.Status == 0)
        {
          if (!a.StartDate.HasValue && b.StartDate.HasValue)
            return 1;
          if (a.StartDate.HasValue && !b.StartDate.HasValue)
            return -1;
          if (a.StartDate.HasValue && b.StartDate.HasValue)
          {
            int searchModels2 = DateSortHelper.CompareTaskByDate(a, b, false);
            if (searchModels2 != 0)
              return searchModels2;
          }
        }
        if (a.Status != 0 && b.Status != 0 && a.CompletedTime.HasValue && b.CompletedTime.HasValue)
          return b.CompletedTime.Value.CompareTo(a.CompletedTime.Value);
        int searchModels3 = TaskDisplayService.ComparePriority(a.IsNote ? -1 : (a.IsEvent ? -2 : a.Priority), b.IsNote ? -1 : (b.IsEvent ? -2 : b.Priority));
        if (searchModels3 != 0)
          return searchModels3;
        if (a.IsEvent && b.IsEvent)
        {
          string title = a.Title;
          return title == null ? 0 : title.CompareTo(b.Title);
        }
        return a.ProjectOrder != b.ProjectOrder ? a.ProjectOrder.CompareTo(b.ProjectOrder) : a.SortOrder.CompareTo(b.SortOrder);
      }));
      return searchModels1;
    }

    private static int ComparePriority(int left, int right)
    {
      if (left > right)
        return -1;
      return left < right ? 1 : 0;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInAssigneeToMe(
      ProjectIdentity project)
    {
      return TaskViewModelHelper.GetModel(assignTo: new List<string>()
      {
        "me"
      }, withChild: true);
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTasksInAbandoned()
    {
      ClosedTaskWithFilterLoader completionLoader = ClosedTaskWithFilterLoader.AbandonedLoader;
      LoadStatus status = completionLoader.CompletedProjectStatus;
      ClosedFilterViewModel filter = AbandonedProjectIdentity.Filter;
      CompletedFilterData completedFilterData = await filter.GetCompletedFilterData();
      List<TaskBaseViewModel> closedModels = TaskViewModelHelper.GetClosedModels(completedFilterData.FromTime, completedFilterData.ToTime, completedFilterData.ProjectIds, status.Count, LocalSettings.Settings.ShowSubtasks, -1, !filter.ShowAll);
      if (closedModels.Count < 50 && filter.Changed)
      {
        filter.Changed = false;
        completionLoader.TryLoadTasks();
      }
      List<TaskBaseViewModel> tasksInAbandoned = closedModels;
      completionLoader = (ClosedTaskWithFilterLoader) null;
      status = (LoadStatus) null;
      filter = (ClosedFilterViewModel) null;
      return tasksInAbandoned;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTasksInCompleted()
    {
      ClosedTaskWithFilterLoader completionLoader = ClosedTaskWithFilterLoader.CompletionLoader;
      LoadStatus status = completionLoader.CompletedProjectStatus;
      ClosedFilterViewModel filter = CompletedProjectIdentity.Filter;
      CompletedFilterData completedFilterData = await filter.GetCompletedFilterData();
      List<TaskBaseViewModel> source = TaskViewModelHelper.GetClosedModels(completedFilterData.FromTime, completedFilterData.ToTime, completedFilterData.ProjectIds, status.Count, LocalSettings.Settings.ShowSubtasks, 2, !filter.ShowAll);
      if (source.Count < 50 && filter.Changed)
        completionLoader.TryLoadTasks();
      if (!filter.ShowAll)
        source = source.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m => m.CompletedUser == LocalSettings.Settings.LoginUserId)).ToList<TaskBaseViewModel>();
      List<TaskBaseViewModel> tasksInCompleted = source;
      completionLoader = (ClosedTaskWithFilterLoader) null;
      status = (LoadStatus) null;
      filter = (ClosedFilterViewModel) null;
      return tasksInCompleted;
    }

    private static async Task<List<TaskBaseViewModel>> GetSingleFilterDisplayTask(
      CardViewModel model,
      bool getCompleted,
      bool needLoadChecklist)
    {
      List<TaskBaseViewModel> taskBaseViewModelList = new List<TaskBaseViewModel>();
      switch (model)
      {
        case PriorityCardViewModel priorityCardViewModel:
          taskBaseViewModelList = TaskViewModelHelper.GetModel(showComplete: getCompleted, showCheckItem: needLoadChecklist, priorities: priorityCardViewModel.Values, type: priorityCardViewModel.LogicType);
          break;
        case DateCardViewModel dateCardViewModel:
          taskBaseViewModelList = TaskViewModelHelper.GetModel(startEndPairs: FilterUtils.GetFilterDatePairs((IEnumerable<string>) dateCardViewModel.Values), showComplete: getCompleted, showCheckItem: needLoadChecklist, type: dateCardViewModel.LogicType);
          break;
        case AssigneeCardViewModel assigneeCardViewModel:
          taskBaseViewModelList = TaskViewModelHelper.GetModel(showComplete: getCompleted, showCheckItem: needLoadChecklist, assignTo: assigneeCardViewModel.Values, type: assigneeCardViewModel.LogicType);
          break;
        case ProjectOrGroupCardViewModel groupCardViewModel:
          taskBaseViewModelList = TaskViewModelHelper.GetModel(groupCardViewModel.GroupIds, groupCardViewModel.Values, showComplete: getCompleted, showCheckItem: needLoadChecklist, type: groupCardViewModel.LogicType);
          break;
        case TagCardViewModel tagCardViewModel:
          taskBaseViewModelList = TaskViewModelHelper.GetModel(tags: tagCardViewModel.Values, showComplete: getCompleted, showCheckItem: needLoadChecklist, type: tagCardViewModel.LogicType);
          break;
        case TaskTypeViewModel taskTypeViewModel:
          List<string> values = taskTypeViewModel.Values;
          taskBaseViewModelList = TaskViewModelHelper.GetModel(showComplete: getCompleted, showCheckItem: needLoadChecklist, taskType: values == null || values.Count != 1 ? TaskType.TaskAndNote : (values[0] != "task" ? TaskType.Note : TaskType.Task));
          break;
        case KeywordsViewModel keywordsViewModel:
          taskBaseViewModelList = await TaskDisplayService.GetDisplayTaskInSearch(keywordsViewModel.Keyword, new SearchFilterViewModel()
          {
            StatusFilter = getCompleted ? StatusFilter.All : StatusFilter.Uncompleted,
            Searched = true
          });
          break;
      }
      return taskBaseViewModelList ?? new List<TaskBaseViewModel>();
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInFilter(
      string rule,
      bool getCompleted = true,
      bool getCheckItem = false,
      bool withChild = true,
      bool inCal = false,
      bool inAll = false)
    {
      if (rule == null)
        return new List<TaskBaseViewModel>();
      List<TaskBaseViewModel> tasks;
      if (Parser.GetFilterRuleType(rule) == 0)
      {
        tasks = await TaskDisplayService.GetDisplayTaskInNormalFilter(Parser.ToNormalModel(rule), getCompleted, getCheckItem, withChild, inCal, inAll);
      }
      else
      {
        AdvancedFilterViewModel advancedModel = Parser.ToAdvanceModel(rule);
        if (!TaskService.IsAdvanceModelValid(advancedModel.CardList))
          return new List<TaskBaseViewModel>();
        tasks = await TaskDisplayService.GetDisplayTaskInAdvancedFilter(advancedModel, getCompleted, getCheckItem);
        List<TaskBaseViewModel> inAdvancedFilter = await TaskDisplayService.GetDisplayEventsInAdvancedFilter(advancedModel, inCal);
        if (inAdvancedFilter != null && inAdvancedFilter.Any<TaskBaseViewModel>())
          tasks.AddRange((IEnumerable<TaskBaseViewModel>) inAdvancedFilter);
        if (withChild)
          tasks.AddRange((IEnumerable<TaskBaseViewModel>) TaskCache.GetPinCheckChildren(tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>()));
        advancedModel = (AdvancedFilterViewModel) null;
      }
      return tasks ?? new List<TaskBaseViewModel>();
    }

    public static async Task<int> GetDisplayEventsCountInFilter(FilterModel filter, bool inCal)
    {
      return Parser.GetFilterRuleType(filter.rule) == 0 ? (await TaskDisplayService.GetDisplayEventsInNormalFilter(Parser.ToNormalModel(filter.rule), inCal)).Item1 : (await TaskDisplayService.GetDisplayEventsInAdvancedFilter(Parser.ToAdvanceModel(filter.rule), inCal)).Count;
    }

    private static async Task<List<TaskBaseViewModel>> GetDisplayEventsInAdvancedFilter(
      AdvancedFilterViewModel advanced,
      bool inCal)
    {
      List<CardViewModel> conds = advanced.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>();
      List<CardViewModel> logics = advanced.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.LogicAnd || card.Type == CardType.LogicOr)).ToList<CardViewModel>();
      List<TaskBaseViewModel> first;
      switch (conds.Count)
      {
        case 1:
          return await TaskDisplayService.GetSingleFilterDisplayEvents(conds[0], inCal);
        case 2:
          first = await TaskDisplayService.GetSingleFilterDisplayEvents(conds[0], inCal);
          List<TaskBaseViewModel> filterDisplayEvents1 = await TaskDisplayService.GetSingleFilterDisplayEvents(conds[1], inCal);
          if (logics.Count == 1)
          {
            switch (logics[0].Type)
            {
              case CardType.LogicAnd:
                return TaskService.GetInterSetOfModels(first, filterDisplayEvents1);
              case CardType.LogicOr:
                return TaskService.GetUnionOfModels(first, filterDisplayEvents1);
            }
          }
          else
            break;
          break;
        case 3:
          first = await TaskDisplayService.GetSingleFilterDisplayEvents(conds[0], inCal);
          List<TaskBaseViewModel> second = await TaskDisplayService.GetSingleFilterDisplayEvents(conds[1], inCal);
          List<TaskBaseViewModel> filterDisplayEvents2 = await TaskDisplayService.GetSingleFilterDisplayEvents(conds[2], inCal);
          if (logics.Count == 2)
          {
            CardType type1 = logics[0].Type;
            CardType type2 = logics[1].Type;
            List<TaskBaseViewModel> first1 = (List<TaskBaseViewModel>) null;
            switch (type1)
            {
              case CardType.LogicAnd:
                first1 = TaskService.GetInterSetOfModels(first, second);
                break;
              case CardType.LogicOr:
                first1 = TaskService.GetUnionOfModels(first, second);
                break;
            }
            if (first1 == null)
              first1 = new List<TaskBaseViewModel>();
            switch (type2)
            {
              case CardType.LogicAnd:
                return TaskService.GetInterSetOfModels(first1, filterDisplayEvents2);
              case CardType.LogicOr:
                return TaskService.GetUnionOfModels(first1, filterDisplayEvents2);
            }
          }
          else
            break;
          break;
      }
      return new List<TaskBaseViewModel>();
    }

    private static async Task<(int, List<TaskBaseViewModel>)> GetDisplayEventsInNormalFilter(
      NormalFilterViewModel normal,
      bool inCal)
    {
      if (normal.Assignees.Any<string>() || normal.Tags.Any<string>() || normal.Priorities.Any<int>() || normal.TaskTypes.Count > 0 && normal.TaskTypes.All<string>((Func<string, bool>) (t => t == "note")) || normal.Projects.Count > 0 && normal.Projects.All<string>((Func<string, bool>) (t => t != "Calendar5959a2259161d16d23a4f272")))
        return (0, new List<TaskBaseViewModel>());
      List<string> dueDates = normal.DueDates;
      if (dueDates.Count == 0)
        dueDates.Add("all");
      return await TaskDisplayService.GetDisplayEventsInDates(dueDates, inCal, normal.Keywords.FirstOrDefault<string>());
    }

    private static async Task<List<TaskBaseViewModel>> GetSingleFilterDisplayEvents(
      CardViewModel model,
      bool inCal)
    {
      List<TaskBaseViewModel> taskBaseViewModelList = new List<TaskBaseViewModel>();
      switch (model)
      {
        case DateCardViewModel dateCardViewModel:
          taskBaseViewModelList = (await TaskDisplayService.GetDisplayEventsInDates(dateCardViewModel.Values, inCal, logicType: dateCardViewModel.LogicType)).Item2;
          break;
        case ProjectOrGroupCardViewModel groupCardViewModel:
          if (groupCardViewModel.Values.Contains("Calendar5959a2259161d16d23a4f272") && groupCardViewModel.LogicType != LogicType.Not)
          {
            taskBaseViewModelList = (await TaskDisplayService.GetDisplayEventsInDates(new List<string>()
            {
              "all"
            }, inCal)).Item2;
            break;
          }
          break;
        case TaskTypeViewModel taskTypeViewModel:
          List<string> values = taskTypeViewModel.Values;
          if ((values == null || values.Count != 1 ? 2 : (values[0] != "task" ? 1 : 0)) != 1)
          {
            taskBaseViewModelList = (await TaskDisplayService.GetDisplayEventsInDates(new List<string>()
            {
              "all"
            }, inCal)).Item2;
            break;
          }
          break;
        case KeywordsViewModel keywordsViewModel:
          taskBaseViewModelList = (await TaskDisplayService.GetDisplayEventsInDates(new List<string>()
          {
            "all"
          }, inCal, keywordsViewModel.Keyword)).Item2;
          break;
      }
      return taskBaseViewModelList ?? new List<TaskBaseViewModel>();
    }

    private static async Task<List<TaskBaseViewModel>> GetDisplayTaskInAdvancedFilter(
      AdvancedFilterViewModel advanced,
      bool getCompleted,
      bool withSubTask = false)
    {
      List<CardViewModel> conds = advanced.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>();
      List<CardViewModel> logics = advanced.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.LogicAnd || card.Type == CardType.LogicOr)).ToList<CardViewModel>();
      DateCardViewModel dateCardViewModel = (DateCardViewModel) conds.FirstOrDefault<CardViewModel>((Func<CardViewModel, bool>) (c => c is DateCardViewModel));
      int num = !withSubTask || dateCardViewModel?.Values == null ? 0 : (dateCardViewModel.Values.Any<string>((Func<string, bool>) (v => v != "nodue")) ? 1 : (dateCardViewModel.LogicType == LogicType.Not ? 1 : 0));
      bool needLoadChecklist = false;
      bool secondLoadChecklist = false;
      bool thirdLoadChecklist = false;
      if (num != 0)
      {
        switch (conds.IndexOf((CardViewModel) dateCardViewModel))
        {
          case 0:
            needLoadChecklist = true;
            switch (conds.Count)
            {
              case 2:
                secondLoadChecklist = logics[0].Type == CardType.LogicAnd;
                break;
              case 3:
                secondLoadChecklist = logics[0].Type == CardType.LogicAnd;
                thirdLoadChecklist = logics[1].Type == CardType.LogicAnd;
                break;
            }
            break;
          case 1:
            secondLoadChecklist = true;
            switch (conds.Count)
            {
              case 2:
                needLoadChecklist = logics[0].Type == CardType.LogicAnd;
                break;
              case 3:
                needLoadChecklist = logics[0].Type == CardType.LogicAnd;
                thirdLoadChecklist = logics[1].Type == CardType.LogicAnd;
                break;
            }
            break;
          case 2:
            thirdLoadChecklist = true;
            if (conds.Count == 3)
            {
              secondLoadChecklist = logics[1].Type == CardType.LogicAnd;
              needLoadChecklist = secondLoadChecklist;
              break;
            }
            break;
        }
      }
      List<TaskBaseViewModel> first;
      switch (conds.Count)
      {
        case 1:
          return await TaskDisplayService.GetSingleFilterDisplayTask(conds[0], getCompleted, needLoadChecklist);
        case 2:
          first = await TaskDisplayService.GetSingleFilterDisplayTask(conds[0], getCompleted, needLoadChecklist);
          List<TaskBaseViewModel> filterDisplayTask1 = await TaskDisplayService.GetSingleFilterDisplayTask(conds[1], getCompleted, secondLoadChecklist);
          if (logics.Count == 1)
          {
            switch (logics[0].Type)
            {
              case CardType.LogicAnd:
                return TaskService.GetInterSetOfModels(first, filterDisplayTask1);
              case CardType.LogicOr:
                return TaskService.GetUnionOfModels(first, filterDisplayTask1);
            }
          }
          else
            break;
          break;
        case 3:
          first = await TaskDisplayService.GetSingleFilterDisplayTask(conds[0], getCompleted, needLoadChecklist);
          List<TaskBaseViewModel> second = await TaskDisplayService.GetSingleFilterDisplayTask(conds[1], getCompleted, secondLoadChecklist);
          List<TaskBaseViewModel> filterDisplayTask2 = await TaskDisplayService.GetSingleFilterDisplayTask(conds[2], getCompleted, thirdLoadChecklist);
          if (logics.Count == 2)
          {
            CardType type1 = logics[0].Type;
            CardType type2 = logics[1].Type;
            List<TaskBaseViewModel> first1 = (List<TaskBaseViewModel>) null;
            switch (type1)
            {
              case CardType.LogicAnd:
                first1 = TaskService.GetInterSetOfModels(first, second);
                break;
              case CardType.LogicOr:
                first1 = TaskService.GetUnionOfModels(first, second);
                break;
            }
            if (first1 == null)
              first1 = new List<TaskBaseViewModel>();
            switch (type2)
            {
              case CardType.LogicAnd:
                return TaskService.GetInterSetOfModels(first1, filterDisplayTask2);
              case CardType.LogicOr:
                return TaskService.GetUnionOfModels(first1, filterDisplayTask2);
            }
          }
          else
            break;
          break;
      }
      return new List<TaskBaseViewModel>();
    }

    private static async Task<(int, List<TaskBaseViewModel>)> GetDisplayEventsInDates(
      List<string> dates,
      bool inCal,
      string keyWord = null,
      LogicType logicType = LogicType.Or,
      bool onlyCount = false)
    {
      DateTime dateTime = DateTime.Now;
      DateTime date = dateTime.Date;
      dateTime = DateTime.Now;
      dateTime = dateTime.Date;
      DateTime end = dateTime.AddDays(180.0);
      int num1 = inCal ? 1 : 0;
      List<string> dates1 = dates;
      string keyWord1 = keyWord;
      int num2 = (int) logicType;
      int num3 = onlyCount ? 1 : 0;
      (int, List<TaskBaseViewModel>) modelsBetweenSpan = await CalendarEventDao.GetTaskDisplayModelsBetweenSpan(date, end, num1 != 0, dates1, keyWord1, (LogicType) num2, num3 != 0);
      return (modelsBetweenSpan.Item1, modelsBetweenSpan.Item2);
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInNormalFilter(
      NormalFilterViewModel normal,
      bool getCompleted,
      bool getSubtask,
      bool withChild = true,
      bool inCal = false,
      bool inAll = false)
    {
      if (normal == null)
        return new List<TaskBaseViewModel>();
      List<FilterDatePair> startEndPairs = (List<FilterDatePair>) null;
      if (normal.DueDates != null && normal.DueDates.Count > 0)
      {
        startEndPairs = FilterUtils.GetFilterDatePairs((IEnumerable<string>) normal.DueDates);
        List<string> list = normal.DueDates.ToList<string>();
        list.Remove("nodue");
        list.Remove("recurring");
        if (list.Count == 0)
          getSubtask = false;
      }
      else
        getSubtask = false;
      TaskType taskType = normal.TaskTypes.Count == 1 ? (normal.TaskTypes[0] != "task" ? TaskType.Note : TaskType.Task) : TaskType.TaskAndNote;
      List<string> keywords = normal.Keywords;
      // ISSUE: explicit non-virtual call
      string keyword = (keywords != null ? (__nonvirtual (keywords.Count) > 0 ? 1 : 0) : 0) != 0 ? normal.Keywords[0] : (string) null;
      List<TaskBaseViewModel> tasks = TaskViewModelHelper.GetModel(normal.Groups, normal.Projects, normal.Tags, startEndPairs, getCompleted, getSubtask, inAll: inAll, assignTo: normal.Assignees, priorities: normal.Priorities, withChild: withChild, inCal: inCal, taskType: taskType, keyword: keyword);
      List<TaskBaseViewModel> taskBaseViewModelList = (await TaskDisplayService.GetDisplayEventsInNormalFilter(normal, inCal)).Item2;
      if (taskBaseViewModelList.Any<TaskBaseViewModel>())
        tasks.AddRange((IEnumerable<TaskBaseViewModel>) taskBaseViewModelList);
      return tasks;
    }

    public static async Task<List<TaskBaseViewModel>> GetCalendarArrangeTasks(
      ProjectExtra projectFilter)
    {
      FilterModel filterModel = (FilterModel) null;
      List<string> filterIds = projectFilter.FilterIds;
      // ISSUE: explicit non-virtual call
      if ((filterIds != null ? (__nonvirtual (filterIds.Count) > 0 ? 1 : 0) : 0) != 0)
        filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == projectFilter.FilterIds[0] && f.deleted != 1));
      List<TaskBaseViewModel> source;
      if (filterModel != null)
      {
        List<TaskBaseViewModel> displayTaskInFilter = await TaskDisplayService.GetDisplayTaskInFilter(filterModel.rule, false);
        List<string> hideProjectIds = new List<string>();
        if (projectFilter.IsAll)
          hideProjectIds.AddRange((IEnumerable<string>) CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => !p.inAll)).Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.id)).ToList<string>());
        hideProjectIds.AddRange((IEnumerable<string>) CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => !p.IsProjectPermit())).Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.id)).ToList<string>());
        source = displayTaskInFilter.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
        {
          if (LocalSettings.Settings.ArrangeTaskDateType == 1)
          {
            DateTime? nullable = t.StartDate;
            if (!nullable.HasValue)
              return false;
            nullable = t.DueDate;
            if (nullable.HasValue)
            {
              nullable = t.DueDate;
              if (nullable.Value > DateTime.Today)
                goto label_7;
            }
            nullable = t.DueDate;
            if (!nullable.HasValue)
            {
              nullable = t.StartDate;
              DateTime today = DateTime.Today;
              if ((nullable.HasValue ? (nullable.GetValueOrDefault() >= today ? 1 : 0) : 0) == 0)
                goto label_10;
            }
            else
              goto label_10;
label_7:
            return false;
          }
          if (t.StartDate.HasValue)
            return false;
label_10:
          return (!hideProjectIds.Any<string>() || !hideProjectIds.Contains(t.ProjectId) || t.Assignee == LocalSettings.Settings.LoginUserId) && t.Status == 0;
        })).ToList<TaskBaseViewModel>();
      }
      else
        source = TaskViewModelHelper.GetTasksInArrange(projectFilter);
      string inServerBoxId = LocalSettings.Settings.InServerBoxId;
      foreach (TaskBaseViewModel taskBaseViewModel in source)
      {
        if (taskBaseViewModel.ProjectId == inServerBoxId)
          taskBaseViewModel.ProjectOrder = long.MinValue;
      }
      if (!LocalSettings.Settings.ExtraSettings.ShowNoteInCalArrange)
        source = source.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => !t.IsNote)).ToList<TaskBaseViewModel>();
      return source;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTasksInTrash(bool isPerson)
    {
      return TaskViewModelHelper.GetTrashModels(isPerson);
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTaskInGroup(string groupId)
    {
      return TaskViewModelHelper.GetModel(new List<string>()
      {
        groupId
      }, showComplete: !LocalSettings.Settings.HideComplete);
    }

    public static List<TaskBaseViewModel> GetDisplayTasksInProject(
      string projectId,
      bool hideComplete)
    {
      List<TaskBaseViewModel> source = TaskCache.GetModelsInProject(projectId);
      if (hideComplete)
        source = source.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Status == 0)).ToList<TaskBaseViewModel>();
      return source;
    }
  }
}
