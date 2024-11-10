// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.ArrangeTaskPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class ArrangeTaskPanel : UserControl, IDragBarEvent, IComponentConnector
  {
    public const int ByList = 0;
    public const int ByTag = 1;
    public const int ByPriority = 2;
    public const int ShowNoDate = 0;
    public const int ShowOverDue = 1;
    private static ConcurrentDictionary<string, (DateTime?, TimeData)> _undoArrangeDict = new ConcurrentDictionary<string, (DateTime?, TimeData)>();
    public bool InDisplay;
    private List<CalendarDisplayViewModel> _viewModels = new List<CalendarDisplayViewModel>();
    private bool _inAssemble;
    private static ProjectExtra _projectFilter;
    private DateTime _startDragTime;
    internal StackPanel TopPanel;
    internal StackPanel DateFilterPanel;
    internal GroupTitle2 SwitchTitle;
    internal Grid ListGrid;
    internal Grid ListFilter;
    internal StackPanel FilterTextBorder;
    internal EmjTextBlock FilterText;
    internal EscPopup ProjectFilterPopup;
    internal EscPopup TagFilterPopup;
    internal Grid ItemsGrid;
    internal ListView TaskItems;
    internal Grid EmptyTaskGrid;
    internal Border MaskBorder;
    private bool _contentLoaded;

    public event EventHandler OnClose;

    public ArrangeTaskPanel()
    {
      this.InitializeComponent();
      this.ListFilter.Visibility = LocalSettings.Settings.ArrangeDisplayType != 2 ? Visibility.Visible : Visibility.Collapsed;
      ArrangeSectionStatusDao.InitClosedSectionNames();
      ArrangeTaskPanel._projectFilter = ProjectExtra.Deserialize(LocalSettings.Settings.ArrangeTaskFilterData);
      this.SwitchTitle.SetSelectedIndex(LocalSettings.Settings.ArrangeDisplayType);
      this.SetFilterText();
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    private void BindEvents()
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(this.ReloadFilters);
      DataChangedNotifier.ProjectChanged += new EventHandler(this.ReloadProjects);
      ticktick_WPF.Notifier.GlobalEventManager.ArrangeFilterTypeChanged += new EventHandler(this.OnArrangeFilterChanged);
      ticktick_WPF.Notifier.GlobalEventManager.ArrangeTaskSortTypeChanged += new EventHandler(this.OnSortTypeChanged);
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      if (!this.IsVisible)
        return;
      if (e.AddIds.Any() || e.StatusChangedIds.Any() || e.TagChangedIds.Any() || e.DeletedChangedIds.Any() || e.DateChangedIds.Any() || e.ProjectChangedIds.Any() || e.KindChangedIds.Any() || e.PriorityChangedIds.Any() || e.UndoDeletedIds.Any())
      {
        this.TryReloadTasks();
      }
      else
      {
        if (!e.PriorityChangedIds.Any() || !(LocalSettings.Settings.CellColorType.ToLower() == "priority") && LocalSettings.Settings.ArrangeDisplayType != 2)
          return;
        this.TryReloadTasks();
      }
    }

    private void UnbindEvents()
    {
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(this.ReloadFilters);
      DataChangedNotifier.ProjectChanged -= new EventHandler(this.ReloadProjects);
      ticktick_WPF.Notifier.GlobalEventManager.ArrangeFilterTypeChanged -= new EventHandler(this.OnArrangeFilterChanged);
      ticktick_WPF.Notifier.GlobalEventManager.ArrangeTaskSortTypeChanged -= new EventHandler(this.OnSortTypeChanged);
    }

    private void OnArrangeFilterChanged(object sender, EventArgs e) => this.TryReloadTasks();

    private async void ReloadProjects(object sender, EventArgs eventArgs)
    {
      ArrangeTaskPanel arrangeTaskPanel = this;
      List<string> projectIds = ArrangeTaskPanel._projectFilter.ProjectIds;
      // ISSUE: explicit non-virtual call
      if ((projectIds != null ? (__nonvirtual (projectIds.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      List<string> list = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => ArrangeTaskPanel._projectFilter.ProjectIds.Contains(p.id))).ToList<ProjectModel>().Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsEnable())).Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.id)).ToList<string>();
      if (list.Count >= ArrangeTaskPanel._projectFilter.ProjectIds.Count)
        return;
      ArrangeTaskPanel._projectFilter.ProjectIds = list;
      LocalSettings.Settings.ArrangeTaskFilterData = ProjectExtra.Serialize(ArrangeTaskPanel._projectFilter);
      if (!arrangeTaskPanel.IsVisible || LocalSettings.Settings.ArrangeDisplayType != 0)
        return;
      arrangeTaskPanel.FilterText.Text = ProjectExtra.FormatDisplayText(LocalSettings.Settings.ArrangeTaskFilterData, true);
      arrangeTaskPanel.TryReloadTasks();
    }

    private void ReloadFilters(object sender, FilterChangeArgs e)
    {
      List<string> filterIds = ArrangeTaskPanel._projectFilter.FilterIds;
      // ISSUE: explicit non-virtual call
      if ((filterIds != null ? (__nonvirtual (filterIds.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      string filterId = ArrangeTaskPanel._projectFilter.FilterIds[0];
      if (filterId == e.deleteId)
      {
        if (LocalSettings.Settings.ArrangeDisplayType == 0)
          this.FilterText.Text = Utils.GetString("AllList");
        ArrangeTaskPanel._projectFilter.FilterIds = new List<string>();
        this.SaveArrangeFilter();
        if (LocalSettings.Settings.ArrangeDisplayType != 0)
          return;
        this.TryReloadTasks();
      }
      else
      {
        if (!(filterId == e.Filter?.id))
          return;
        FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == filterId && f.deleted != 1));
        if (filterModel == null || LocalSettings.Settings.ArrangeDisplayType != 0)
          return;
        this.FilterText.Text = filterModel.name;
        this.TryReloadTasks();
      }
    }

    private void SaveArrangeFilter()
    {
      LocalSettings.Settings.ArrangeTaskFilterData = ProjectExtra.Serialize(ArrangeTaskPanel._projectFilter);
    }

    public void TryReloadTasks() => this.LoadTask();

    private async void SetFilterText()
    {
      if (LocalSettings.Settings.ArrangeDisplayType == 0)
      {
        this.FilterText.Text = ProjectExtra.FormatDisplayText(LocalSettings.Settings.ArrangeTaskFilterData, true);
      }
      else
      {
        if (LocalSettings.Settings.ArrangeDisplayType != 1)
          return;
        this.FilterText.Text = TagCardViewModel.ToNormalDisplayText(ArrangeTaskPanel._projectFilter.Tags, 1);
      }
    }

    private void OnFilterProject(object sender, MouseButtonEventArgs e)
    {
      if (LocalSettings.Settings.ArrangeDisplayType == 0)
      {
        ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.ProjectFilterPopup, ArrangeTaskPanel._projectFilter, new ProjectSelectorExtra()
        {
          showFilters = true,
          onlyShowPermission = true
        });
        projectOrGroupPopup.Save += new EventHandler<ProjectExtra>(this.OnProjectSelect);
        projectOrGroupPopup.Show();
      }
      else
      {
        if (LocalSettings.Settings.ArrangeDisplayType != 1)
          return;
        TagSearchFilterControl searchFilterControl = new TagSearchFilterControl(ArrangeTaskPanel._projectFilter.Tags);
        searchFilterControl.Cancel -= new EventHandler(this.OnTagFilterCancel);
        searchFilterControl.Cancel += new EventHandler(this.OnTagFilterCancel);
        searchFilterControl.Save -= new EventHandler<List<string>>(this.OnTagFilterSelected);
        searchFilterControl.Save += new EventHandler<List<string>>(this.OnTagFilterSelected);
        this.TagFilterPopup.Child = (UIElement) searchFilterControl;
        this.TagFilterPopup.IsOpen = true;
      }
    }

    private void OnTagFilterSelected(object sender, List<string> tags)
    {
      this.TagFilterPopup.IsOpen = false;
      ArrangeTaskPanel._projectFilter.Tags = tags ?? new List<string>();
      this.SaveArrangeFilter();
      this.LoadTask();
      this.SetFilterText();
    }

    private void OnTagFilterCancel(object sender, EventArgs e)
    {
      this.TagFilterPopup.IsOpen = false;
    }

    private void OnProjectSelect(object sender, ProjectExtra data)
    {
      data.Tags = ArrangeTaskPanel._projectFilter.Tags;
      ArrangeTaskPanel._projectFilter = data;
      this.SaveArrangeFilter();
      this.LoadTask();
      this.SetFilterText();
    }

    public async void NotifyModelChanged()
    {
      ArrangeTaskPanel arrangeTaskPanel = this;
      if (!arrangeTaskPanel.IsVisible)
        return;
      await Task.Delay(100);
      arrangeTaskPanel.TryReloadTasks();
    }

    private async Task LoadTask()
    {
      ProjectExtra projectFilter;
      if (this._inAssemble)
        projectFilter = (ProjectExtra) null;
      else if (!this.InDisplay)
      {
        projectFilter = (ProjectExtra) null;
      }
      else
      {
        this._inAssemble = true;
        this._viewModels.Clear();
        projectFilter = new ProjectExtra();
        if (LocalSettings.Settings.ArrangeDisplayType == 0)
        {
          projectFilter.ProjectIds = ArrangeTaskPanel._projectFilter.ProjectIds;
          projectFilter.GroupIds = ArrangeTaskPanel._projectFilter.GroupIds;
          projectFilter.FilterIds = ArrangeTaskPanel._projectFilter.FilterIds;
        }
        if (LocalSettings.Settings.ArrangeDisplayType == 1)
          projectFilter.Tags = ArrangeTaskPanel._projectFilter.Tags;
        List<TaskBaseViewModel> calendarArrangeTasks = await TaskDisplayService.GetCalendarArrangeTasks(projectFilter);
        if (calendarArrangeTasks != null && calendarArrangeTasks.Count > 0)
        {
          if (LocalSettings.Settings.ArrangeDisplayType == 1)
          {
            this.GetViewModelsByTag(calendarArrangeTasks, projectFilter.Tags);
          }
          else
          {
            foreach (KeyValuePair<string, List<TaskBaseViewModel>> keyValuePair in LocalSettings.Settings.ArrangeDisplayType == 2 ? this.GetTaskDictByPriority((IEnumerable<TaskBaseViewModel>) calendarArrangeTasks) : this.GetTaskDictByProject(calendarArrangeTasks))
            {
              CalendarDisplayViewModel section = new CalendarDisplayViewModel()
              {
                SourceViewModel = new TaskBaseViewModel()
                {
                  Type = DisplayType.Section,
                  Title = keyValuePair.Key
                }
              };
              if (LocalSettings.Settings.ArrangeDisplayType == 0 && keyValuePair.Key != Utils.GetString("Pinned"))
                section.SourceViewModel.Title = CacheManager.GetProjectById(keyValuePair.Key)?.name;
              bool folded = ArrangeSectionStatusDao.CheckSectionClosed(LocalSettings.Settings.ArrangeDisplayType, section.Title);
              section.IsOpened = !folded;
              this._viewModels.Add(section);
              List<TaskBaseViewModel> models = keyValuePair.Value;
              if (keyValuePair.Key == Utils.GetString("Pinned"))
              {
                List<SyncSortOrderModel> asyncList = await TaskSortOrderService.GetAsyncList("taskPinned", "all");
                Dictionary<string, long> pinOrderDict = new Dictionary<string, long>();
                foreach (SyncSortOrderModel syncSortOrderModel in asyncList)
                  pinOrderDict[syncSortOrderModel.EntityId] = syncSortOrderModel.SortOrder;
                models.Sort((Comparison<TaskBaseViewModel>) ((left, right) => (pinOrderDict.ContainsKey(left.Id) ? pinOrderDict[left.Id] : 0L).CompareTo(pinOrderDict.ContainsKey(right.Id) ? pinOrderDict[right.Id] : 0L)));
              }
              else if (LocalSettings.Settings.ArrangeTaskDateType == 1)
                models.Sort((Comparison<TaskBaseViewModel>) ((a, b) => a.StartDate.HasValue && b.StartDate.HasValue ? DateSortHelper.CompareTaskByDate(a, b, false) : 0));
              if (LocalSettings.Settings.ArrangeDisplayType == 0)
              {
                List<CalendarDisplayViewModel> list = models.Select<TaskBaseViewModel, CalendarDisplayViewModel>(new Func<TaskBaseViewModel, CalendarDisplayViewModel>(CalendarDisplayViewModel.Build)).ToList<CalendarDisplayViewModel>();
                if (LocalSettings.Settings.ExtraSettings.ShowParentInCalArrange)
                {
                  Dictionary<string, CalendarDisplayViewModel> dictionary = new Dictionary<string, CalendarDisplayViewModel>();
                  foreach (CalendarDisplayViewModel displayViewModel in list)
                  {
                    dictionary[displayViewModel.TaskId] = displayViewModel;
                    displayViewModel.IsOpened = !ArrangeSectionStatusDao.CheckSectionClosed(LocalSettings.Settings.ArrangeDisplayType, displayViewModel.TaskId);
                  }
                  foreach (CalendarDisplayViewModel displayViewModel1 in list)
                  {
                    string parentId = displayViewModel1.SourceViewModel.ParentId;
                    CalendarDisplayViewModel displayViewModel2;
                    if (!string.IsNullOrEmpty(parentId) && dictionary.TryGetValue(parentId, out displayViewModel2))
                    {
                      displayViewModel1.Parent = displayViewModel2;
                      displayViewModel2.Children.Add(displayViewModel1);
                    }
                  }
                  foreach (CalendarDisplayViewModel displayViewModel in list)
                  {
                    if (displayViewModel.Parent == null)
                    {
                      section.Children.Add(displayViewModel);
                      if (!folded)
                      {
                        List<CalendarDisplayViewModel> children = displayViewModel.GetChildren();
                        this._viewModels.Add(displayViewModel);
                        this._viewModels.AddRange((IEnumerable<CalendarDisplayViewModel>) children);
                      }
                    }
                  }
                }
                else
                {
                  foreach (CalendarDisplayViewModel displayViewModel in list)
                  {
                    section.Children.Add(displayViewModel);
                    if (!folded)
                      this._viewModels.Add(displayViewModel);
                  }
                }
              }
              else
              {
                foreach (CalendarDisplayViewModel displayViewModel in models.Select<TaskBaseViewModel, CalendarDisplayViewModel>(new Func<TaskBaseViewModel, CalendarDisplayViewModel>(CalendarDisplayViewModel.Build)))
                {
                  section.Children.Add(displayViewModel);
                  if (!folded)
                    this._viewModels.Add(displayViewModel);
                }
              }
              section = (CalendarDisplayViewModel) null;
              models = (List<TaskBaseViewModel>) null;
            }
          }
        }
        this._inAssemble = false;
        this.EmptyTaskGrid.Visibility = !this._viewModels.Any<CalendarDisplayViewModel>() ? Visibility.Visible : Visibility.Collapsed;
        this.SetViewModels();
        projectFilter = (ProjectExtra) null;
      }
    }

    private Dictionary<string, List<TaskBaseViewModel>> GetTaskDictByPriority(
      IEnumerable<TaskBaseViewModel> tasks)
    {
      Dictionary<int, List<TaskBaseViewModel>> dictionary = new Dictionary<int, List<TaskBaseViewModel>>();
      foreach (TaskBaseViewModel task in tasks)
      {
        int key = task.Kind == "NOTE" ? -1 : task.Priority;
        if (dictionary.ContainsKey(key))
          dictionary[key].Add(task);
        else
          dictionary[key] = new List<TaskBaseViewModel>()
          {
            task
          };
      }
      IOrderedEnumerable<int> orderedEnumerable = dictionary.Keys.OrderByDescending<int, int>((Func<int, int>) (k => k));
      Dictionary<string, List<TaskBaseViewModel>> taskDictByPriority = new Dictionary<string, List<TaskBaseViewModel>>();
      foreach (int num in (IEnumerable<int>) orderedEnumerable)
      {
        string priorityName = this.GetPriorityName(num);
        IOrderedEnumerable<TaskBaseViewModel> source = dictionary[num].OrderBy<TaskBaseViewModel, long>((Func<TaskBaseViewModel, long>) (t => t.ProjectOrder)).ThenBy<TaskBaseViewModel, long>((Func<TaskBaseViewModel, long>) (t => t.SortOrder));
        taskDictByPriority[priorityName] = source.ToList<TaskBaseViewModel>();
      }
      return taskDictByPriority;
    }

    private void GetViewModelsByTag(List<TaskBaseViewModel> tasks, List<string> tagFilters)
    {
      List<TagModel> tags = CacheManager.GetTags();
      tags.Sort((Comparison<TagModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
      string key1 = Utils.GetString("NoTagsTitle");
      Dictionary<string, List<TaskBaseViewModel>> dictionary = tags.ToDictionary<TagModel, string, List<TaskBaseViewModel>>((Func<TagModel, string>) (tag => tag.name), (Func<TagModel, List<TaskBaseViewModel>>) (tag => new List<TaskBaseViewModel>()));
      dictionary.Add(key1, new List<TaskBaseViewModel>());
      foreach (TaskBaseViewModel task in tasks)
      {
        if (string.IsNullOrEmpty(task.Tag))
        {
          dictionary[key1].Add(task);
        }
        else
        {
          List<string> taskTags = TagSerializer.ToTags(task.Tag);
          List<string> stringList = taskTags;
          // ISSUE: explicit non-virtual call
          if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            string key2 = dictionary.Keys.FirstOrDefault<string>((Func<string, bool>) (k => (tagFilters == null || tagFilters.Count == 0 || tagFilters.Contains(k)) && taskTags.Contains(k)));
            if (key2 != null)
              dictionary[key2].Add(task);
            else
              dictionary.Add(taskTags[0], new List<TaskBaseViewModel>()
              {
                task
              });
          }
          else
            dictionary[key1].Add(task);
        }
      }
      foreach (KeyValuePair<string, List<TaskBaseViewModel>> keyValuePair in dictionary)
      {
        List<TaskBaseViewModel> taskBaseViewModelList = keyValuePair.Value;
        // ISSUE: explicit non-virtual call
        if ((taskBaseViewModelList != null ? (__nonvirtual (taskBaseViewModelList.Count) > 0 ? 1 : 0) : 0) != 0 && (tagFilters == null || tagFilters.Count == 0 || tagFilters.Contains(keyValuePair.Key) || keyValuePair.Key == key1))
        {
          CalendarDisplayViewModel displayViewModel1 = new CalendarDisplayViewModel()
          {
            SourceViewModel = new TaskBaseViewModel()
            {
              Type = DisplayType.Section,
              Title = TagDataHelper.GetTagDisplayName(keyValuePair.Key)
            }
          };
          this._viewModels.Add(displayViewModel1);
          List<TaskBaseViewModel> list = keyValuePair.Value.OrderBy<TaskBaseViewModel, long>((Func<TaskBaseViewModel, long>) (task => task.ProjectOrder)).ThenBy<TaskBaseViewModel, long>((Func<TaskBaseViewModel, long>) (task => task.SortOrder)).ToList<TaskBaseViewModel>();
          if (LocalSettings.Settings.ArrangeTaskDateType == 1)
            list.Sort((Comparison<TaskBaseViewModel>) ((a, b) => a.StartDate.HasValue && b.StartDate.HasValue ? DateSortHelper.CompareTaskByDate(a, b, false) : 0));
          bool flag = ArrangeSectionStatusDao.CheckSectionClosed(LocalSettings.Settings.ArrangeDisplayType, displayViewModel1.Title);
          foreach (CalendarDisplayViewModel displayViewModel2 in list.Select<TaskBaseViewModel, CalendarDisplayViewModel>(new Func<TaskBaseViewModel, CalendarDisplayViewModel>(CalendarDisplayViewModel.Build)))
          {
            displayViewModel1.Children.Add(displayViewModel2);
            if (!flag)
              this._viewModels.Add(displayViewModel2);
            else
              displayViewModel1.IsOpened = false;
          }
        }
      }
    }

    private void SetViewModels()
    {
      ItemsSourceHelper.SetItemsSource<CalendarDisplayViewModel>((ItemsControl) this.TaskItems, this._viewModels);
    }

    private string GetPriorityName(int taskPriority)
    {
      switch (taskPriority)
      {
        case -1:
          return Utils.GetString("Notes");
        case 0:
          return Utils.GetString("PriorityNull");
        case 1:
          return Utils.GetString("PriorityLow");
        case 3:
          return Utils.GetString("PriorityMedium");
        case 5:
          return Utils.GetString("PriorityHigh");
        default:
          return Utils.GetString("PriorityNull");
      }
    }

    private Dictionary<string, List<TaskBaseViewModel>> GetTaskDictByProject(
      List<TaskBaseViewModel> tasks)
    {
      Dictionary<string, List<TaskBaseViewModel>> dictionary1 = new Dictionary<string, List<TaskBaseViewModel>>();
      string pinned = Utils.GetString("Pinned");
      HashSet<string> stringSet = new HashSet<string>();
      foreach (TaskBaseViewModel task in tasks)
      {
        string key = task.IsPinned ? pinned : task.ProjectId;
        if (task.IsPinned)
          stringSet.Add(task.Id);
        if (dictionary1.ContainsKey(key))
          dictionary1[key].Add(task);
        else
          dictionary1[key] = new List<TaskBaseViewModel>()
          {
            task
          };
      }
      List<string> list1 = dictionary1.Keys.ToList<string>();
      list1.Sort((Comparison<string>) ((a, b) =>
      {
        if (a == pinned)
          return -1;
        if (b == pinned)
          return 1;
        ProjectModel projectById1 = CacheManager.GetProjectById(a);
        ProjectModel projectById2 = CacheManager.GetProjectById(b);
        return projectById1 == null ? 1 : projectById1.CompareTo(projectById2);
      }));
      Dictionary<string, List<TaskBaseViewModel>> taskDictByProject = new Dictionary<string, List<TaskBaseViewModel>>();
      foreach (string key in list1)
      {
        List<TaskBaseViewModel> list2 = dictionary1[key];
        if (key != pinned && LocalSettings.Settings.ExtraSettings.ShowParentInCalArrange)
        {
          Dictionary<string, TaskBaseViewModel> dictionary2 = new Dictionary<string, TaskBaseViewModel>();
          foreach (TaskBaseViewModel taskBaseViewModel in list2)
            dictionary2[taskBaseViewModel.Id] = taskBaseViewModel;
          using (List<TaskBaseViewModel>.Enumerator enumerator = list2.GetEnumerator())
          {
label_23:
            while (enumerator.MoveNext())
            {
              string parentId = enumerator.Current.ParentId;
              while (true)
              {
                if (!string.IsNullOrEmpty(parentId) && !dictionary2.ContainsKey(parentId) && !stringSet.Contains(parentId))
                {
                  TaskBaseViewModel taskById = TaskCache.GetTaskById(parentId);
                  if (taskById != null)
                  {
                    dictionary2[taskById.Id] = taskById;
                    parentId = taskById.ParentId;
                  }
                  else
                    goto label_23;
                }
                else
                  goto label_23;
              }
            }
          }
          list2 = dictionary2.Values.ToList<TaskBaseViewModel>();
        }
        List<TaskBaseViewModel> list3 = list2.OrderBy<TaskBaseViewModel, long>((Func<TaskBaseViewModel, long>) (t => t.SortOrder)).ToList<TaskBaseViewModel>();
        taskDictByProject[key] = list3.ToList<TaskBaseViewModel>();
      }
      return taskDictByProject;
    }

    public bool OnSelection() => false;

    public async void OnDragStart(
      CalendarDisplayViewModel model,
      MouseEventArgs e,
      bool fromArrange)
    {
      Utils.FindParent<CalendarControl>((DependencyObject) this)?.GetDragEvent()?.OnDragStart(model, e, true);
    }

    public void NotifyModelDrop()
    {
      List<CalendarDisplayViewModel> viewModels = this._viewModels;
      if (viewModels == null)
        return;
      viewModels.Where<CalendarDisplayViewModel>((Func<CalendarDisplayViewModel, bool>) (item => item.Dragging)).ToList<CalendarDisplayViewModel>().ForEach((Action<CalendarDisplayViewModel>) (item => item.Dragging = false));
    }

    public async Task OnSectionStatusChanged(bool isOpened, CalendarDisplayViewModel model)
    {
      if (this._viewModels == null)
        return;
      List<CalendarDisplayViewModel> models = new List<CalendarDisplayViewModel>((IEnumerable<CalendarDisplayViewModel>) this._viewModels);
      int index = models.IndexOf(model);
      if (index >= 0)
      {
        List<CalendarDisplayViewModel> children = model.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          if (isOpened)
            model.Children?.ForEach((Action<CalendarDisplayViewModel>) (child =>
            {
              models.Insert(++index, child);
              child.GetChildren()?.ForEach((Action<CalendarDisplayViewModel>) (c => models.Insert(++index, c)));
            }));
          else
            model.Children?.ForEach((Action<CalendarDisplayViewModel>) (child =>
            {
              models.Remove(child);
              child.GetChildren()?.ForEach((Action<CalendarDisplayViewModel>) (c => models.Remove(c)));
            }));
        }
      }
      this._viewModels = models;
      ItemsSourceHelper.SetItemsSource<CalendarDisplayViewModel>((ItemsControl) this.TaskItems, this._viewModels);
    }

    public void TryResetTagFilterText()
    {
      if (!this.IsVisible || LocalSettings.Settings.ArrangeDisplayType != 1)
        return;
      this.FilterText.Text = TagCardViewModel.ToNormalDisplayText(ArrangeTaskPanel._projectFilter.Tags, 1);
    }

    public static void ResetArrangeFilter(ProjectExtra filter)
    {
      ArrangeTaskPanel._projectFilter = filter;
    }

    public bool IsFilterSelected()
    {
      List<string> filterIds = ArrangeTaskPanel._projectFilter.FilterIds;
      return filterIds != null && filterIds.Any<string>();
    }

    private void OnFilterDateClick(object sender, MouseButtonEventArgs e)
    {
      new CalArrangeDateFilterPopup((FrameworkElement) this.DateFilterPanel).Show();
    }

    private void OnSortTypeChanged(object sender, GroupTitleViewModel e)
    {
      if (LocalSettings.Settings.ArrangeDisplayType == e.Index)
        return;
      LocalSettings.Settings.ArrangeDisplayType = e.Index;
      UserActCollectUtils.AddClickEvent("calendar", "arrangement_category", e.Index == 0 ? "list" : (e.Index == 1 ? "tag" : "priority"));
      this.SetFilterText();
      this.ListFilter.Visibility = LocalSettings.Settings.ArrangeDisplayType != 2 ? Visibility.Visible : Visibility.Collapsed;
      this.LoadTask();
    }

    private void OnSortTypeChanged(object sender, EventArgs e)
    {
      if (this.SwitchTitle.GetSelectedIndex() == LocalSettings.Settings.ArrangeDisplayType)
        return;
      this.SwitchTitle.SetSelectedIndex(LocalSettings.Settings.ArrangeDisplayType);
      this.SetFilterText();
      this.ListFilter.Visibility = LocalSettings.Settings.ArrangeDisplayType != 2 ? Visibility.Visible : Visibility.Collapsed;
      this.LoadTask();
    }

    public void ClearItems()
    {
      this.TaskItems.ItemsSource = (IEnumerable) null;
      this._viewModels.Clear();
      ArrangeTaskPanel._undoArrangeDict.Clear();
    }

    public void SetInDisplay(bool show)
    {
      this.InDisplay = true;
      if (show)
        this.LoadTask();
      else
        this.ClearItems();
    }

    public void RemoveItem(CalendarDisplayViewModel model)
    {
    }

    public void OnPopupMove(System.Windows.Point position, double width)
    {
      if (this.Opacity <= 0.0)
        return;
      if (position.X < width)
        this.MaskBorder.Visibility = Visibility.Collapsed;
      else
        this.MaskBorder.Visibility = new Rect(width, 100.0, this.ActualWidth, this.ActualHeight).Contains(position) ? Visibility.Visible : Visibility.Collapsed;
    }

    public async Task OnDragDrop(TimeDataModel origin)
    {
      ArrangeTaskPanel arrangeTaskPanel = this;
      if (arrangeTaskPanel.Opacity <= 0.0 || !arrangeTaskPanel.MaskBorder.IsVisible)
        return;
      if (LocalSettings.Settings.ArrangeTaskDateType != 0)
      {
        DateTime? startDate = origin.StartDate;
        DateTime today = DateTime.Today;
        if ((startDate.HasValue ? (startDate.GetValueOrDefault() >= today ? 1 : 0) : 0) == 0)
          goto label_18;
      }
      string id = origin.TaskId;
      DateTime dateTime;
      if (origin.RepeatFrom != "1" && !string.IsNullOrEmpty(origin.RepeatFlag) && !origin.RepeatFlag.Contains("COUNT=1"))
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(id);
        if (thinTaskById != null && origin.StartDate.HasValue && thinTaskById.startDate.HasValue)
        {
          DateTime date1 = thinTaskById.startDate.Value.Date;
          dateTime = origin.StartDate.Value;
          DateTime date2 = dateTime.Date;
          if (date1 != date2)
          {
            TimeData model = new TimeData(thinTaskById, (List<TaskReminderModel>) null);
            List<string> exDates1 = model.ExDates;
            dateTime = origin.StartDate.Value;
            dateTime = dateTime.Date;
            string str1 = dateTime.ToString("yyyyMMdd");
            if (!exDates1.Contains(str1))
            {
              List<string> exDates2 = model.ExDates;
              dateTime = origin.StartDate.Value;
              dateTime = dateTime.Date;
              string str2 = dateTime.ToString("yyyyMMdd");
              exDates2.Add(str2);
              model.RepeatFlag = RRuleUtils.GetNextCount(model.RepeatFlag, false);
              await TaskService.SetDate(id, model, false);
            }
          }
          else
          {
            TaskModel taskModel = await TaskService.SkipCurrentRecurrence(id, toast: false, handleCount: true);
          }
          id = (await TaskService.CopyTask(id, true, delayNotify: true))?.id;
        }
      }
      if (LocalSettings.Settings.ArrangeTaskDateType == 0)
      {
        await TaskService.ClearDate(id);
      }
      else
      {
        string taskId = id;
        dateTime = DateTime.Today;
        DateTime date = dateTime.AddDays(-1.0);
        int num = origin.RepeatFrom != "1" ? 1 : 0;
        await TaskService.SetDateAndClearRepeat(taskId, date, false, num != 0);
      }
      id = (string) null;
label_18:
      arrangeTaskPanel.MaskBorder.Visibility = Visibility.Collapsed;
    }

    private IToastShowWindow GetToastWindow()
    {
      return Utils.FindParent<IToastShowWindow>((DependencyObject) this);
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e)
    {
      Utils.FindParent<CalendarControl>((DependencyObject) this)?.ShowOrHideArrange();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/arrangetaskpanel.xaml", UriKind.Relative));
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
          this.TopPanel = (StackPanel) target;
          break;
        case 2:
          this.DateFilterPanel = (StackPanel) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFilterDateClick);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
          break;
        case 5:
          this.SwitchTitle = (GroupTitle2) target;
          break;
        case 6:
          this.ListGrid = (Grid) target;
          break;
        case 7:
          this.ListFilter = (Grid) target;
          break;
        case 8:
          this.FilterTextBorder = (StackPanel) target;
          this.FilterTextBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFilterProject);
          break;
        case 9:
          this.FilterText = (EmjTextBlock) target;
          break;
        case 10:
          this.ProjectFilterPopup = (EscPopup) target;
          break;
        case 11:
          this.TagFilterPopup = (EscPopup) target;
          break;
        case 12:
          this.ItemsGrid = (Grid) target;
          break;
        case 13:
          this.TaskItems = (ListView) target;
          break;
        case 14:
          this.EmptyTaskGrid = (Grid) target;
          break;
        case 15:
          this.MaskBorder = (Border) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
