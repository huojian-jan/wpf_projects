// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoFilterControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Search;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoFilterControl : UserControl, IComponentConnector, IStyleConnector
  {
    private ProjectOrGroupPopup _popup;
    private ProjectIdentity _projectIdentity;
    public bool ShowComplete;
    private readonly Popup _parentPopup;
    private bool _inFocus;
    private bool _showDetail;
    private HashSet<string> _linkedIds;
    private List<TimerModel> _timers;
    private List<PomoTask> _recent;
    private string _selectedGroupTitle = "Task";
    private bool _loadTimers;
    private bool _loadMoreTask;
    private bool _loadMoreHabit;
    private bool _loadMoreTimer;
    private List<HabitModel> _habits;
    private static List<PomoTask> _remoteRecentTasks;
    private int[] _tabIndex = new int[3];
    private List<HabitCheckInModel> _checkIns;
    internal PomoFilterControl Root;
    internal Grid Container;
    internal Grid DetailGrid;
    internal TaskDetailPomoSummaryControl PomoSummary;
    internal Button CompleteButton;
    internal Grid ListGrid;
    internal GroupTitle GroupTitle;
    internal Grid SearchGrid;
    internal EmojiEditor SearchText;
    internal Path SearchIcon;
    internal Border ExitSearchIcon;
    internal Border ClearIcon;
    internal StackPanel ProjectName;
    internal EscPopup ProjectFilterPopup;
    internal Grid SelectTaskPanel;
    internal ListView TaskList;
    internal Grid EmptyPanel;
    internal Border ToastBorder;
    internal TextBlock ToastText;
    private bool _contentLoaded;

    public event EventHandler<DisplayItemModel> ItemSelected;

    public PomoFilterControl(
      Popup popup,
      bool inFocus = true,
      bool showDetail = false,
      List<string> linkedIds = null,
      bool loadTimers = true)
    {
      this._loadTimers = loadTimers;
      this._parentPopup = popup;
      this._showDetail = showDetail;
      this._linkedIds = linkedIds == null ? new HashSet<string>() : new HashSet<string>((IEnumerable<string>) linkedIds);
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this._inFocus = inFocus;
      popup.Child = (UIElement) this;
      this.SearchText.EditBox.GotFocus += new RoutedEventHandler(this.OnSearchFocus);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      if (this._showDetail && this.Data != null && !string.IsNullOrEmpty(this.Data.FocusId) && TickFocusManager.Working)
      {
        PomoTaskDetailModel taskModel = await PomoTaskDetailModel.GetDetailModel(this.Data.ObjId);
        if (taskModel != null)
        {
          PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(this.Data.ObjId);
          if (LocalSettings.Settings.EnableFocus && pomoByTaskId != null)
          {
            if (pomoByTaskId.count <= 0 && pomoByTaskId.StopwatchDuration + pomoByTaskId.PomoDuration < 30L)
            {
              List<object[]> focuses = pomoByTaskId.focuses;
              // ISSUE: explicit non-virtual call
              if ((focuses != null ? (__nonvirtual (focuses.Count) > 0 ? 1 : 0) : 0) == 0 && pomoByTaskId.estimatedPomo <= 0 && pomoByTaskId.EstimatedDuration <= 0L)
                goto label_6;
            }
            this.PomoSummary.SetData(pomoByTaskId);
            this.PomoSummary.Visibility = Visibility.Visible;
            goto label_7;
          }
label_6:
          this.PomoSummary.Visibility = Visibility.Collapsed;
label_7:
          this.DetailGrid.Visibility = Visibility.Visible;
          this.ListGrid.Visibility = Visibility.Collapsed;
          this.DetailGrid.DataContext = (object) taskModel;
          this.CompleteButton.Content = TickFocusManager.IsPomo ? (object) Utils.GetString("PublicComplete") : (object) Utils.GetString("CompleteTimingTask");
          return;
        }
        taskModel = (PomoTaskDetailModel) null;
      }
      this.LoadRecentTask();
      this.TryLoadTaskList();
    }

    private async Task LoadRecentTask()
    {
      PomoFilterControl._remoteRecentTasks = await Communicator.GetRecentFocusTasks();
      if (PomoFilterControl._remoteRecentTasks == null || PomoFilterControl._remoteRecentTasks.Count <= 0)
        return;
      if (!this.GroupTitle.Titles.StartsWith("Recent"))
      {
        this.TryLoadTaskList();
      }
      else
      {
        if (!(this._selectedGroupTitle == "Recent"))
          return;
        this.LoadRecent();
      }
    }

    private async Task TryLoadTaskList()
    {
      PomoFilterControl pomoFilterControl1 = this;
      pomoFilterControl1.ListGrid.Visibility = Visibility.Visible;
      pomoFilterControl1.DetailGrid.Visibility = Visibility.Collapsed;
      List<TabBarModel> list1 = (LocalSettings.Settings.UserPreference?.desktopTabBars?.bars ?? TabBarModel.InitTabBars()).ToList<TabBarModel>().Where<TabBarModel>((Func<TabBarModel, bool>) (t => t.name.ToUpper() == "TASK" || t.name.ToUpper() == "HABIT" || t.name.ToUpper() == "POMO")).ToList<TabBarModel>();
      list1.Sort((Comparison<TabBarModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
      pomoFilterControl1._tabIndex = new int[3];
      for (int index = 0; index < list1.Count; ++index)
      {
        if (list1[index].name.ToUpper() == "TASK")
          pomoFilterControl1._tabIndex[index] = 1;
        if (list1[index].name.ToUpper() == "HABIT")
          pomoFilterControl1._tabIndex[index] = 2;
        if (list1[index].name.ToUpper() == "POMO")
          pomoFilterControl1._tabIndex[index] = 3;
      }
      List<PomoTask> recentPomoTasks = await PomoTaskDao.GetRecentPomoTasks(6);
      pomoFilterControl1._recent = recentPomoTasks;
      List<PomoTask> remoteRecentTasks = PomoFilterControl._remoteRecentTasks;
      int num;
      if (remoteRecentTasks == null)
      {
        List<PomoTask> recent = pomoFilterControl1._recent;
        num = recent != null ? (recent.Any<PomoTask>((Func<PomoTask, bool>) (r => !string.IsNullOrEmpty(r.GetId()))) ? 1 : 0) : 0;
      }
      else
        num = remoteRecentTasks.Any<PomoTask>((Func<PomoTask, bool>) (r => !string.IsNullOrEmpty(r.GetId()))) ? 1 : 0;
      string titles = num != 0 ? "Recent" : "";
      List<HabitModel> habitModelList;
      if (LocalSettings.Settings.ShowHabit)
        habitModelList = await HabitService.GetHabitsbyStatus(0);
      else
        habitModelList = (List<HabitModel>) null;
      pomoFilterControl1._habits = habitModelList;
      PomoFilterControl pomoFilterControl2 = pomoFilterControl1;
      List<HabitModel> habits1 = pomoFilterControl1._habits;
      List<HabitModel> list2 = habits1 != null ? habits1.Where<HabitModel>((Func<HabitModel, bool>) (h => !h.IsSkipToday())).ToList<HabitModel>() : (List<HabitModel>) null;
      pomoFilterControl2._habits = list2;
      List<HabitModel> habits2 = pomoFilterControl1._habits;
      // ISSUE: explicit non-virtual call
      if ((habits2 != null ? (__nonvirtual (habits2.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        List<HabitCheckInModel> checkInsInSpan = await HabitCheckInDao.GetCheckInsInSpan(DateTime.Today, DateTime.Today.AddDays(1.0));
        pomoFilterControl1._checkIns = checkInsInSpan;
      }
      List<TimerModel> timerModelList;
      if (LocalSettings.Settings.EnableFocus && pomoFilterControl1._loadTimers)
        timerModelList = (await TimerDao.GetDisplayTimersAsync()).Where<TimerModel>((Func<TimerModel, bool>) (t => t.Status == 0)).OrderBy<TimerModel, long>((Func<TimerModel, long>) (a => a.SortOrder)).ToList<TimerModel>();
      else
        timerModelList = (List<TimerModel>) null;
      pomoFilterControl1._timers = timerModelList;
      for (int index = 0; index < 3; ++index)
      {
        switch (pomoFilterControl1._tabIndex[index])
        {
          case 1:
            titles = titles + (!string.IsNullOrEmpty(titles) ? "|" : "") + "Task";
            break;
          case 2:
            List<HabitModel> habits3 = pomoFilterControl1._habits;
            // ISSUE: explicit non-virtual call
            if ((habits3 != null ? (__nonvirtual (habits3.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              titles = titles + (!string.IsNullOrEmpty(titles) ? "|" : "") + "Habit";
              break;
            }
            break;
          case 3:
            List<TimerModel> timers = pomoFilterControl1._timers;
            // ISSUE: explicit non-virtual call
            if ((timers != null ? (__nonvirtual (timers.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              titles = titles + (!string.IsNullOrEmpty(titles) ? "|" : "") + "CommonTimer";
              break;
            }
            break;
        }
      }
      pomoFilterControl1.GroupTitle.Titles = titles;
      pomoFilterControl1.GroupTitle.SelectedTitle = pomoFilterControl1._selectedGroupTitle;
      pomoFilterControl1._selectedGroupTitle = pomoFilterControl1.GroupTitle.SelectedTitle;
      pomoFilterControl1.DetailGrid.DataContext = (object) null;
      if (pomoFilterControl1.Data != null)
      {
        pomoFilterControl1._projectIdentity = pomoFilterControl1.Data.Identity;
        pomoFilterControl1.ReloadProjectIdentity(pomoFilterControl1._projectIdentity);
      }
      if (string.IsNullOrEmpty(pomoFilterControl1.SearchText.Text))
      {
        pomoFilterControl1.GroupTitle.Visibility = pomoFilterControl1.GroupTitle.Titles.Contains("|") ? Visibility.Visible : Visibility.Collapsed;
        pomoFilterControl1.SearchIcon.Visibility = Visibility.Visible;
        pomoFilterControl1.ProjectName.Visibility = Visibility.Visible;
        pomoFilterControl1.ExitSearchIcon.Visibility = Visibility.Collapsed;
        pomoFilterControl1.LoadDisplayItems();
        titles = (string) null;
      }
      else
      {
        pomoFilterControl1.GroupTitle.Visibility = Visibility.Collapsed;
        pomoFilterControl1.SearchIcon.Visibility = Visibility.Collapsed;
        pomoFilterControl1.ProjectName.Visibility = Visibility.Collapsed;
        pomoFilterControl1.ExitSearchIcon.Visibility = Visibility.Visible;
        await Task.Delay(150);
        pomoFilterControl1.SearchText.Focus();
        pomoFilterControl1.OnSearchTextChanged((object) null, (EventArgs) null);
        titles = (string) null;
      }
    }

    public void SetTheme(string theme)
    {
      theme = theme == "dark" ? "dark" : "light";
      ThemeUtil.SetTheme(theme, (FrameworkElement) this);
    }

    private FocusViewModel Data => this.DataContext as FocusViewModel;

    private void OnFilterClick(object sender, MouseButtonEventArgs e)
    {
      ProjectExtra data = new ProjectExtra();
      if (this._projectIdentity is NormalProjectIdentity)
        data.ProjectIds.Add(this._projectIdentity.Id);
      if (this._projectIdentity is TagProjectIdentity)
        data.Tags.Add(this._projectIdentity.Id);
      if (this._projectIdentity is FilterProjectIdentity)
        data.FilterIds.Add(this._projectIdentity.Id);
      if (this._projectIdentity is SmartProjectIdentity)
        data.SmartIds.Add(this._projectIdentity.Id);
      if (this._popup == null)
      {
        this._popup = new ProjectOrGroupPopup((Popup) this.ProjectFilterPopup, data, new ProjectSelectorExtra()
        {
          showAll = false,
          showFilters = true,
          showTags = true,
          showSmartProjects = true,
          onlyShowPermission = true,
          batchMode = false,
          canSelectGroup = false,
          showSmartAll = false,
          showFilterGroup = false,
          showListGroup = true
        });
        this._popup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnFilterItemSelect);
        this._popup.Show();
      }
      else
      {
        this.ProjectFilterPopup.IsOpen = true;
        this._popup.SetData(data);
      }
    }

    private async void OnFilterItemSelect(object sender, SelectableItemViewModel project)
    {
      if (project is ticktick_WPF.Views.Misc.ProjectGroupViewModel)
        return;
      this.ProjectFilterPopup.IsOpen = false;
      await this.LoadProjectIdentity(project, project.Type);
      this.LoadTasks();
    }

    private async Task LoadProjectIdentity(SelectableItemViewModel project, string type)
    {
      switch (type)
      {
        case "normal":
          this._projectIdentity = ProjectIdHelper.GetProjectIdentity(project.Id);
          break;
        case "smart":
          this._projectIdentity = ProjectIdentity.CreateSmartIdentity(project.Id);
          break;
        case "filter":
          this._projectIdentity = ProjectIdHelper.GetFilterIdentity(project.Id);
          break;
        case "tag":
          this._projectIdentity = ProjectIdHelper.GetTagIdentity(project.Id);
          break;
        default:
          this._projectIdentity = type == null ? (ProjectIdentity) ProjectIdentity.CreateInboxProject() : this.GetDefaultProject();
          break;
      }
      if (this._projectIdentity == null)
        this._projectIdentity = this.GetDefaultProject();
      this.Data.Identity = this._projectIdentity;
    }

    private async Task ReloadProjectIdentity(ProjectIdentity identity)
    {
      this._projectIdentity = identity is NormalProjectIdentity ? ProjectIdHelper.GetProjectIdentity(identity.Id) : (identity is SmartProjectIdentity ? ProjectIdentity.CreateSmartIdentity(identity.Id) : (identity is FilterProjectIdentity ? ProjectIdHelper.GetFilterIdentity(identity.Id) : (identity is TagProjectIdentity ? ProjectIdHelper.GetTagIdentity(identity.Id) : this.GetDefaultProject())));
      if (this._projectIdentity == null)
        this._projectIdentity = this.GetDefaultProject();
      this.Data.Identity = this._projectIdentity;
    }

    private ProjectIdentity GetDefaultProject()
    {
      return ProjectIdentity.CreateSmartIdentity("_special_id_today");
    }

    private async void LoadTasks()
    {
      ObservableCollection<DisplayItemModel> displayModels = this.BuildDisplayItemModels((IEnumerable<TaskBaseViewModel>) (await ProjectTaskDataProvider.GetDisplayModels(this._projectIdentity)).Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m => !m.IsNote && m.Editable)).ToList<TaskBaseViewModel>());
      List<ColumnModel> columnModelList;
      if (this._projectIdentity.SortOption.groupBy == "sortOrder")
        columnModelList = await ColumnDao.GetColumnsByProjectId(this._projectIdentity.GetProjectId());
      else
        columnModelList = new List<ColumnModel>();
      List<ColumnModel> columns = columnModelList;
      displayModels = await SortHelper.Sort(displayModels, this._projectIdentity, columns: columns);
      DisplayItemModel displayItemModel1;
      foreach (DisplayItemModel displayItemModel2 in (Collection<DisplayItemModel>) displayModels)
      {
        if (displayItemModel2.Selected)
          displayItemModel1 = displayItemModel2;
      }
      this.ReloadTask(displayModels.ToList<DisplayItemModel>());
      if (displayItemModel1 == null)
      {
        displayModels = (ObservableCollection<DisplayItemModel>) null;
      }
      else
      {
        this.TaskList.ScrollIntoView((object) displayItemModel1);
        displayModels = (ObservableCollection<DisplayItemModel>) null;
      }
    }

    private ObservableCollection<DisplayItemModel> BuildDisplayItemModels(
      IEnumerable<TaskBaseViewModel> models)
    {
      ObservableCollection<DisplayItemModel> observableCollection = new ObservableCollection<DisplayItemModel>();
      foreach (TaskBaseViewModel model in models)
      {
        if (model.Editable)
        {
          switch (model.Type)
          {
            case DisplayType.CheckItem:
            case DisplayType.Event:
            case DisplayType.Note:
            case DisplayType.Course:
              continue;
            case DisplayType.Habit:
              model.StartDate = new DateTime?();
              model.DueDate = new DateTime?();
              break;
          }
          DisplayItemModel displayItemModel = new DisplayItemModel(model);
          displayItemModel.Selected = displayItemModel.IsTask && displayItemModel.TaskId == this.Data.FocusId || displayItemModel.IsHabit && displayItemModel.Habit.Id == this.Data.FocusId;
          displayItemModel.DisplayColor = this.GetPriorityColor(displayItemModel.Priority).ToString();
          displayItemModel.Linked = this._linkedIds.Contains(displayItemModel.Id);
          observableCollection.Add(displayItemModel);
        }
      }
      return observableCollection;
    }

    private SolidColorBrush GetPriorityColor(int modelPriority)
    {
      string str;
      switch (modelPriority)
      {
        case 0:
          str = "BaseColorOpacity40";
          break;
        case 1:
          str = "PriorityLowColor";
          break;
        case 3:
          str = "PriorityMiddleColor";
          break;
        case 5:
          str = "PriorityHighColor";
          break;
        default:
          str = "BaseColorOpacity40";
          break;
      }
      return this.FindResource((object) str) is SolidColorBrush resource ? resource : ThemeUtil.GetColor(str);
    }

    private void ReloadTask(List<DisplayItemModel> tasks)
    {
      if (tasks == null || tasks.Count <= 0)
      {
        this.EmptyPanel.Visibility = Visibility.Visible;
        this.SelectTaskPanel.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.EmptyPanel.Visibility = Visibility.Collapsed;
        this.SelectTaskPanel.Visibility = Visibility.Visible;
        this.TaskList.ItemsSource = (IEnumerable) new ObservableCollection<DisplayItemModel>(tasks);
      }
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is DisplayItemModel dataContext))
        return;
      if (!dataContext.Linked)
      {
        this._parentPopup.IsOpen = false;
        if (this._inFocus)
        {
          UserActCollectUtils.AddClickEvent("focus", TickFocusManager.GetActCType(), string.IsNullOrEmpty(TickFocusManager.Config.FocusVModel.FocusId) ? "select_task" : "switch_task");
          TickFocusManager.OnFocusIdSelected(dataContext.Id, dataContext.Type == DisplayType.Timer ? 2 : (dataContext.IsHabit ? 1 : 0));
        }
        else
        {
          this.Data.SetFocusId(dataContext.Id);
          EventHandler<DisplayItemModel> itemSelected = this.ItemSelected;
          if (itemSelected == null)
            return;
          itemSelected((object) null, dataContext);
        }
      }
      else
        this.Toast(Utils.GetString(dataContext.IsHabit ? "HabitLinked" : "TaskLinked"));
    }

    private void CheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is CheckBox checkBox))
        return;
      checkBox.Tag = (object) "1";
    }

    private async void OnItemCheckChanged(object sender, RoutedEventArgs e)
    {
      if (!(sender is CheckBox checkBox) || !(checkBox.Tag is string tag) || !(tag == "1") || !(checkBox.DataContext is PomoSubtaskDetailModel dataContext))
        return;
      checkBox.Tag = (object) "0";
      dataContext.Status = dataContext.Status == 0 ? 1 : 0;
      await this.NotifyItemStatusChanged(dataContext);
    }

    private async Task NotifyItemStatusChanged(PomoSubtaskDetailModel model)
    {
      if (model.Status == 1)
        model.CompleteDate = new DateTime?(DateTime.Now);
      await TaskDetailItemService.CompleteCheckItem(model.Id, false);
      if (!(this.DetailGrid.DataContext is PomoTaskDetailModel dataContext))
        return;
      ObservableCollection<PomoSubtaskDetailModel> items = dataContext.Items;
      // ISSUE: explicit non-virtual call
      if ((items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      dataContext.SortItems();
    }

    private async void OnCompleteClick(object sender, RoutedEventArgs e)
    {
      if (!(this.DetailGrid.DataContext is PomoTaskDetailModel dataContext))
        return;
      if (!TickFocusManager.IsPomo)
        FocusTimer.TryStopTiming(false);
      TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(dataContext.TaskId, 2);
      SyncManager.TryDelaySync();
      this._parentPopup.IsOpen = false;
    }

    private void OnSelectTaskClick(object sender, RoutedEventArgs e) => this.TryLoadTaskList();

    private void OnOpenPathClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!(sender is Grid grid) || !(grid.DataContext is DisplayItemModel dataContext))
        return;
      List<DisplayItemModel> childrenModels = dataContext.GetChildrenModels(false);
      ObservableCollection<DisplayItemModel> models = this.TaskList.ItemsSource as ObservableCollection<DisplayItemModel>;
      if (models == null || childrenModels == null)
        return;
      if (dataContext.IsOpen)
      {
        foreach (DisplayItemModel displayItemModel in childrenModels)
          models.Remove(displayItemModel);
        dataContext.IsOpen = false;
      }
      else
      {
        int i = 1;
        int index = models.IndexOf(dataContext);
        childrenModels.ForEach((Action<DisplayItemModel>) (child =>
        {
          models.Insert(index + i, child);
          ++i;
        }));
        dataContext.IsOpen = true;
      }
    }

    public void ClearItemSelectedEvent()
    {
      this.ItemSelected = (EventHandler<DisplayItemModel>) null;
    }

    private void Toast(string toastText)
    {
      this.ToastBorder.Visibility = Visibility.Visible;
      this.ToastText.Text = toastText;
      this.ToastBorder.BeginStoryboard((Storyboard) this.FindResource((object) "ShowToast"));
    }

    private void OnToasted(object sender, EventArgs e)
    {
      this.ToastBorder.Visibility = Visibility.Collapsed;
    }

    private void OnClearClick(object sender, MouseButtonEventArgs e) => this.SearchText.Text = "";

    private void OnSearchTextChanged(object sender, EventArgs e)
    {
      this.ClearIcon.Visibility = string.IsNullOrEmpty(this.SearchText.Text) ? Visibility.Collapsed : Visibility.Visible;
      List<DisplayItemModel> displayModels = new List<DisplayItemModel>();
      if (!string.IsNullOrEmpty(this.SearchText.Text))
      {
        string keyword = this.SearchText.Text.ToLower().Trim();
        List<string> keys = ((IEnumerable<string>) keyword.Split(' ')).ToList<string>();
        keys.Remove("");
        for (int index = 0; index < 3; ++index)
        {
          switch (this._tabIndex[index])
          {
            case 1:
              AddTasks();
              break;
            case 2:
              AddHabits();
              break;
            case 3:
              AddTimers();
              break;
          }
        }

        void AddTasks()
        {
          List<TaskBaseViewModel> exactSearchTasks = new List<TaskBaseViewModel>();
          List<TaskBaseViewModel> fussySearchTasks = new List<TaskBaseViewModel>();
          foreach (TaskBaseViewModel taskBaseViewModel in TaskCache.GetAllTask())
          {
            if (taskBaseViewModel.Deleted == 0 && !(taskBaseViewModel.Kind == "NOTE") && taskBaseViewModel.Editable)
            {
              int num = SearchHelper.KeyWordMatch(taskBaseViewModel.Title.ToLower(), keyword, keys);
              if (num == 1)
                exactSearchTasks.Add(taskBaseViewModel);
              if (num == 2)
                fussySearchTasks.Add(taskBaseViewModel);
            }
          }
          List<TaskBaseViewModel> searchModels = TaskDisplayService.GetSearchModels(exactSearchTasks, fussySearchTasks);
          if (!searchModels.Any<TaskBaseViewModel>())
            return;
          Section section = new Section()
          {
            SectionId = "Task",
            Name = Utils.GetString("Task")
          };
          DisplayItemModel displayItemModel1 = new DisplayItemModel(new TaskBaseViewModel(section))
          {
            Section = section,
            IsOpen = true
          };
          displayModels.Add(displayItemModel1);
          foreach (TaskBaseViewModel vm in searchModels)
          {
            DisplayItemModel displayItemModel2 = new DisplayItemModel(vm);
            displayItemModel2.Selected = displayItemModel2.Id == this.Data.FocusId;
            displayItemModel2.DisplayColor = this.GetPriorityColor(displayItemModel2.Priority).ToString();
            displayItemModel1.Section.Children.Add(displayItemModel2);
            if (displayItemModel1.Section.Children.Count <= 5 || this._loadMoreTask)
              displayModels.Add(displayItemModel2);
          }
          if (displayItemModel1.Section.Children.Count > 5 && !this._loadMoreTask)
          {
            DisplayItemModel displayItemModel3 = new DisplayItemModel()
            {
              SourceViewModel = new TaskBaseViewModel()
              {
                Type = DisplayType.LoadMore
              },
              Parent = displayItemModel1
            };
            displayModels.Add(displayItemModel3);
          }
          displayModels.Add(DisplayItemModel.BuildSplit());
        }

        void AddHabits()
        {
          List<HabitModel> habits = this._habits;
          List<HabitModel> list = habits != null ? habits.Where<HabitModel>((Func<HabitModel, bool>) (habit => habit.SyncStatus != -1 && SearchHelper.KeyWordMatched(habit.Name, keyword, keys))).ToList<HabitModel>() : (List<HabitModel>) null;
          if (list == null || !list.Any<HabitModel>())
            return;
          list.Sort((Comparison<HabitModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
          Section section = new Section()
          {
            SectionId = "Habit",
            Name = Utils.GetString("Habit")
          };
          DisplayItemModel displayItemModel1 = new DisplayItemModel(new TaskBaseViewModel(section))
          {
            Section = section,
            IsOpen = true
          };
          displayModels.Add(displayItemModel1);
          List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
          foreach (HabitModel habitModel in list)
          {
            HabitModel habit = habitModel;
            List<HabitCheckInModel> checkIns = this._checkIns;
            HabitCheckInModel habitCheckInModel = checkIns != null ? checkIns.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id)) : (HabitCheckInModel) null;
            HabitBaseViewModel vm = new HabitBaseViewModel(habit);
            vm.StartDate = new DateTime?();
            DisplayItemModel displayItemModel2 = new DisplayItemModel((TaskBaseViewModel) vm);
            if (habitCheckInModel != null && (habitCheckInModel.Value >= habitCheckInModel.Goal || habitCheckInModel.CheckStatus == 1))
            {
              displayItemModel2.SourceViewModel.Status = 2;
              displayItemModel2.SourceViewModel.CompletedTime = habitCheckInModel.CheckinTime;
            }
            displayItemModel2.Selected = displayItemModel2.Id == this.Data.FocusId;
            displayItemModel2.DisplayColor = this.GetPriorityColor(displayItemModel2.Priority).ToString();
            displayItemModel1.Section.Children.Add(displayItemModel2);
            displayItemModelList.Add(displayItemModel2);
          }
          displayItemModelList.Sort((Comparison<DisplayItemModel>) ((a, b) => a.Status == b.Status && a.CompletedTime.HasValue && b.CompletedTime.HasValue ? b.CompletedTime.Value.CompareTo(a.CompletedTime.Value) : a.Status.CompareTo(b.Status)));
          for (int index = 0; index < displayItemModelList.Count; ++index)
          {
            if (index < 5 || this._loadMoreHabit)
              displayModels.Add(displayItemModelList[index]);
          }
          if (displayItemModel1.Section.Children.Count > 5 && !this._loadMoreHabit)
          {
            DisplayItemModel displayItemModel3 = new DisplayItemModel()
            {
              SourceViewModel = new TaskBaseViewModel()
              {
                Type = DisplayType.LoadMore
              },
              Parent = displayItemModel1
            };
            displayModels.Add(displayItemModel3);
          }
          displayModels.Add(DisplayItemModel.BuildSplit());
        }

        void AddTimers()
        {
          List<TimerModel> timers = this._timers;
          List<TimerModel> list = timers != null ? timers.Where<TimerModel>((Func<TimerModel, bool>) (timer => !timer.Deleted && timer.Status == 0 && SearchHelper.KeyWordMatched(timer.Name, keyword, keys))).ToList<TimerModel>() : (List<TimerModel>) null;
          if (list == null || !list.Any<TimerModel>())
            return;
          Section section = new Section()
          {
            SectionId = "CommonTimer",
            Name = Utils.GetString("CommonTimer")
          };
          DisplayItemModel displayItemModel1 = new DisplayItemModel(new TaskBaseViewModel(section))
          {
            Section = section,
            IsOpen = true
          };
          displayModels.Add(displayItemModel1);
          foreach (TimerModel timerModel in list)
          {
            DisplayItemModel displayItemModel2 = new DisplayItemModel(new TaskBaseViewModel()
            {
              Id = timerModel.Id,
              Title = timerModel.Name,
              Type = DisplayType.Timer,
              Priority = 0,
              Status = 0
            });
            displayItemModel2.Selected = displayItemModel2.Id == this.Data.FocusId;
            displayItemModel2.DisplayColor = this.GetPriorityColor(displayItemModel2.Priority).ToString();
            displayItemModel1.Section.Children.Add(displayItemModel2);
            if (displayItemModel1.Section.Children.Count <= 5 || this._loadMoreTimer)
              displayModels.Add(displayItemModel2);
          }
          if (displayItemModel1.Section.Children.Count > 5 && !this._loadMoreTimer)
          {
            DisplayItemModel displayItemModel3 = new DisplayItemModel()
            {
              SourceViewModel = new TaskBaseViewModel()
              {
                Type = DisplayType.LoadMore
              },
              Parent = displayItemModel1
            };
            displayModels.Add(displayItemModel3);
          }
          displayModels.Add(DisplayItemModel.BuildSplit());
        }
      }
      if (displayModels.Count > 0)
        displayModels.RemoveAt(displayModels.Count - 1);
      this.EmptyPanel.Visibility = Visibility.Collapsed;
      this.SelectTaskPanel.Visibility = Visibility.Visible;
      this.TaskList.ItemsSource = (IEnumerable) new ObservableCollection<DisplayItemModel>(displayModels);
    }

    private void OnSearchFocus(object sender, RoutedEventArgs e)
    {
      if (!this.SearchIcon.IsVisible)
        return;
      this.GroupTitle.Visibility = Visibility.Collapsed;
      this.SearchIcon.Visibility = Visibility.Collapsed;
      this.ProjectName.Visibility = Visibility.Collapsed;
      this.ExitSearchIcon.Visibility = Visibility.Visible;
      this.EmptyPanel.Visibility = Visibility.Collapsed;
      this.TaskList.ItemsSource = (IEnumerable) new ObservableCollection<DisplayItemModel>();
      this.SelectTaskPanel.Visibility = Visibility.Visible;
    }

    private void ExitSearchClick(object sender, MouseButtonEventArgs e)
    {
      this.SearchText.Text = string.Empty;
      this.TaskList.Focus();
      this.GroupTitle.Visibility = this.GroupTitle.Titles.Contains("|") ? Visibility.Visible : Visibility.Collapsed;
      this.SearchIcon.Visibility = Visibility.Visible;
      this.ProjectName.Visibility = Visibility.Visible;
      this.ExitSearchIcon.Visibility = Visibility.Collapsed;
      this.LoadDisplayItems();
    }

    private void OnGroupTitleSelected(object sender, GroupTitleViewModel e)
    {
      this.LoadDisplayItems();
      this._selectedGroupTitle = this.GroupTitle.SelectedTitle;
    }

    private void LoadDisplayItems()
    {
      switch (this.GroupTitle.SelectedTitle)
      {
        case "Recent":
          this.ProjectName.Visibility = Visibility.Collapsed;
          this.LoadRecent();
          break;
        case "Habit":
          this.ProjectName.Visibility = Visibility.Collapsed;
          this.LoadHabits();
          break;
        case "CommonTimer":
          this.ProjectName.Visibility = Visibility.Collapsed;
          this.LoadTimers();
          break;
        default:
          this.ProjectName.Visibility = Visibility.Visible;
          this.LoadTasks();
          break;
      }
    }

    private async Task LoadRecent()
    {
      PomoFilterControl pomoFilterControl = this;
      List<PomoTask> pomoTaskList1 = this._recent;
      if (pomoTaskList1 == null)
        pomoTaskList1 = await PomoTaskDao.GetRecentPomoTasks(6);
      List<PomoTask> pomoTaskList2 = pomoTaskList1;
      List<PomoTask> remoteRecentTasks = PomoFilterControl._remoteRecentTasks;
      if (remoteRecentTasks != null)
        pomoTaskList2.AddRange(remoteRecentTasks.Where<PomoTask>((Func<PomoTask, bool>) (p => p.EndTime > DateTime.Today.AddDays(-6.0))));
      pomoTaskList2.Sort((Comparison<PomoTask>) ((a, b) => b.EndTime.CompareTo(a.EndTime)));
      List<string> taskIds = new List<string>();
      List<string> habitIds = new List<string>();
      List<string> timerIds = new List<string>();
      int[] numArray = new int[3];
      HashSet<string> stringSet = new HashSet<string>();
      foreach (PomoTask pomoTask in pomoTaskList2)
      {
        PomoTask r = pomoTask;
        if (taskIds.Count + habitIds.Count + timerIds.Count < 20)
        {
          if (!stringSet.Contains(r.GetId()))
          {
            stringSet.Add(r.GetId());
            if (this._loadTimers && !string.IsNullOrEmpty(r.TimerSid) && !timerIds.Contains(r.TimerSid))
            {
              List<TimerModel> timers = this._timers;
              if ((timers != null ? timers.FirstOrDefault<TimerModel>((Func<TimerModel, bool>) (t => t.Id == r.TimerSid)) : (TimerModel) null) != null)
              {
                timerIds.Add(r.TimerSid);
                if (numArray[2] == 0)
                {
                  numArray[2] = 1 + (numArray[0] > 0 ? 1 : 0) + (numArray[1] > 0 ? 1 : 0);
                  continue;
                }
                continue;
              }
            }
            if (!string.IsNullOrEmpty(r.TaskId) && !taskIds.Contains(r.TaskId))
            {
              if (this._loadTimers)
              {
                List<TimerModel> timers = this._timers;
                if ((timers != null ? timers.FirstOrDefault<TimerModel>((Func<TimerModel, bool>) (t => t.ObjId == r.TaskId)) : (TimerModel) null) != null)
                {
                  timerIds.Add(r.TimerSid);
                  if (numArray[2] == 0)
                  {
                    numArray[2] = 1 + (numArray[0] > 0 ? 1 : 0) + (numArray[1] > 0 ? 1 : 0);
                    continue;
                  }
                  continue;
                }
              }
              taskIds.Add(r.TaskId);
              if (!string.IsNullOrEmpty(r.TimerSid))
                stringSet.Add(r.TimerSid);
              if (numArray[0] == 0)
                numArray[0] = 1 + (numArray[1] > 0 ? 1 : 0) + (numArray[2] > 0 ? 1 : 0);
            }
            else if (LocalSettings.Settings.ShowHabit && !string.IsNullOrEmpty(r.HabitId) && !habitIds.Contains(r.HabitId))
            {
              if (this._loadTimers)
              {
                List<TimerModel> timers = this._timers;
                if ((timers != null ? timers.FirstOrDefault<TimerModel>((Func<TimerModel, bool>) (t => t.ObjId == r.HabitId)) : (TimerModel) null) != null)
                {
                  timerIds.Add(r.TimerSid);
                  if (numArray[2] == 0)
                  {
                    numArray[2] = 1 + (numArray[0] > 0 ? 1 : 0) + (numArray[1] > 0 ? 1 : 0);
                    continue;
                  }
                  continue;
                }
              }
              habitIds.Add(r.HabitId);
              if (!string.IsNullOrEmpty(r.TimerSid))
                stringSet.Add(r.TimerSid);
              if (numArray[1] == 0)
                numArray[1] = 1 + (numArray[0] > 0 ? 1 : 0) + (numArray[2] > 0 ? 1 : 0);
            }
          }
        }
        else
          break;
      }
      List<DisplayItemModel> displayModels = new List<DisplayItemModel>();
      for (int index1 = 1; index1 < 4; ++index1)
      {
        for (int index2 = 0; index2 < 3; ++index2)
        {
          if (numArray[index2] == index1)
          {
            switch (index2)
            {
              case 0:
                AddTasks();
                continue;
              case 1:
                AddHabits();
                continue;
              case 2:
                AddTimers();
                continue;
              default:
                continue;
            }
          }
        }
      }
      this.ReloadTask(displayModels);

      void AddTasks()
      {
        if (!taskIds.Any<string>())
          return;
        bool flag = false;
        Section section = new Section()
        {
          SectionId = "Task",
          Name = Utils.GetString("Task")
        };
        DisplayItemModel displayItemModel1 = new DisplayItemModel(new TaskBaseViewModel(section))
        {
          Section = section,
          IsOpen = true
        };
        foreach (string taskId in taskIds)
        {
          TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
          if (taskById != null && taskById.Deleted == 0)
          {
            DisplayItemModel displayItemModel2 = new DisplayItemModel(taskById);
            displayItemModel2.Selected = displayItemModel2.Id == this.Data.FocusId;
            displayItemModel2.DisplayColor = this.GetPriorityColor(displayItemModel2.Priority).ToString();
            displayItemModel2.Linked = this._linkedIds.Contains(displayItemModel2.Id);
            displayItemModel1.Section.Children.Add(displayItemModel2);
            if (!flag)
            {
              displayModels.Add(displayItemModel1);
              flag = true;
            }
            displayModels.Add(displayItemModel2);
          }
        }
      }

      void AddHabits()
      {
        if (!habitIds.Any<string>())
          return;
        bool flag = false;
        Section section = new Section()
        {
          SectionId = "Habit",
          Name = Utils.GetString("Habit")
        };
        DisplayItemModel displayItemModel1 = new DisplayItemModel(new TaskBaseViewModel(section))
        {
          Section = section,
          IsOpen = true
        };
        foreach (string habitId in habitIds)
        {
          string t = habitId;
          List<HabitModel> habits = this._habits;
          HabitModel habit = habits != null ? habits.FirstOrDefault<HabitModel>((Func<HabitModel, bool>) (h => h.Id == t)) : (HabitModel) null;
          if (habit != null && habit.Status == 0 && habit.SyncStatus != -1)
          {
            HabitBaseViewModel vm = new HabitBaseViewModel(habit);
            vm.StartDate = new DateTime?();
            DisplayItemModel displayItemModel2 = new DisplayItemModel((TaskBaseViewModel) vm);
            displayItemModel2.Selected = displayItemModel2.Id == this.Data.FocusId;
            displayItemModel2.Linked = this._linkedIds.Contains(displayItemModel2.Id);
            displayItemModel2.DisplayColor = this.GetPriorityColor(displayItemModel2.Priority).ToString();
            if (!flag)
            {
              displayModels.Add(displayItemModel1);
              flag = true;
            }
            List<HabitCheckInModel> checkIns = this._checkIns;
            HabitCheckInModel habitCheckInModel = checkIns != null ? checkIns.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id)) : (HabitCheckInModel) null;
            if (habitCheckInModel != null && (habitCheckInModel.Value >= habitCheckInModel.Goal || habitCheckInModel.CheckStatus == 1))
            {
              displayItemModel2.SourceViewModel.Status = 2;
              displayItemModel2.SourceViewModel.CompletedTime = habitCheckInModel.CheckinTime;
            }
            displayItemModel1.Section.Children.Add(displayItemModel2);
            displayModels.Add(displayItemModel2);
          }
        }
      }

      void AddTimers()
      {
        if (!timerIds.Any<string>())
          return;
        Section section = new Section()
        {
          SectionId = "CommonTimer",
          Name = Utils.GetString("CommonTimer")
        };
        bool flag = false;
        DisplayItemModel displayItemModel1 = new DisplayItemModel(new TaskBaseViewModel(section))
        {
          Section = section,
          IsOpen = true
        };
        foreach (string timerId in timerIds)
        {
          string id = timerId;
          TimerModel timerModel = this._timers.FirstOrDefault<TimerModel>((Func<TimerModel, bool>) (t => t.Id == id && t.Status == 0 && !t.Deleted));
          if (timerModel != null)
          {
            DisplayItemModel displayItemModel2 = new DisplayItemModel(new TaskBaseViewModel()
            {
              Id = timerModel.Id,
              Title = timerModel.Name,
              Type = DisplayType.Timer,
              Priority = 0,
              Status = 0,
              IsOpen = true
            })
            {
              Parent = displayItemModel1
            };
            displayItemModel2.Selected = displayItemModel2.Id == this.Data.FocusId;
            displayItemModel2.DisplayColor = this.GetPriorityColor(displayItemModel2.Priority).ToString();
            if (!flag)
            {
              displayModels.Add(displayItemModel1);
              flag = true;
            }
            displayItemModel1.Section.Children.Add(displayItemModel2);
            displayModels.Add(displayItemModel2);
          }
        }
      }
    }

    private async Task LoadTimers()
    {
      List<TimerModel> timerModelList1 = this._timers;
      if (timerModelList1 == null)
        timerModelList1 = (await TimerDao.GetDisplayTimersAsync()).Where<TimerModel>((Func<TimerModel, bool>) (t => t.Status == 0)).OrderBy<TimerModel, long>((Func<TimerModel, long>) (a => a.SortOrder)).ToList<TimerModel>();
      List<TimerModel> timerModelList2 = timerModelList1;
      List<DisplayItemModel> source = new List<DisplayItemModel>();
      foreach (TimerModel timerModel in timerModelList2)
      {
        DisplayItemModel displayItemModel = new DisplayItemModel(new TaskBaseViewModel()
        {
          Id = timerModel.Id,
          Title = timerModel.Name,
          Type = DisplayType.Timer,
          Priority = 0,
          Status = 0,
          SortOrder = timerModel.SortOrder
        });
        source.Add(displayItemModel);
      }
      foreach (DisplayItemModel displayItemModel in source)
      {
        displayItemModel.Selected = displayItemModel.Id == this.Data.FocusId;
        displayItemModel.DisplayColor = this.GetPriorityColor(displayItemModel.Priority).ToString();
      }
      this.ReloadTask(source.ToList<DisplayItemModel>());
    }

    private async Task LoadHabits()
    {
      Dictionary<string, long> sectionIdDict = (await HabitService.GetHabitSections()).OrderBy<HabitSectionModel, long>((Func<HabitSectionModel, long>) (s => s.SortOrder)).ToList<HabitSectionModel>().ToDictionaryEx<HabitSectionModel, string, long>((Func<HabitSectionModel, string>) (s => s.Id), (Func<HabitSectionModel, long>) (s => s.SortOrder));
      List<HabitModel> habitModelList1 = this._habits;
      if (habitModelList1 == null)
        habitModelList1 = await HabitService.GetHabitsbyStatus(0);
      List<HabitModel> habitModelList2 = habitModelList1;
      habitModelList2.Sort((Comparison<HabitModel>) ((a, b) =>
      {
        long num1 = string.IsNullOrEmpty(a.SectionId) || !sectionIdDict.ContainsKey(a.SectionId) ? long.MaxValue : sectionIdDict[a.SectionId];
        long num2 = string.IsNullOrEmpty(b.SectionId) || !sectionIdDict.ContainsKey(b.SectionId) ? long.MaxValue : sectionIdDict[b.SectionId];
        return num1 != num2 ? num1.CompareTo(num2) : a.SortOrder.CompareTo(b.SortOrder);
      }));
      List<DisplayItemModel> tasks = new List<DisplayItemModel>();
      foreach (HabitModel habitModel in habitModelList2)
      {
        HabitModel habit = habitModel;
        HabitBaseViewModel habitBaseViewModel = new HabitBaseViewModel(habit);
        habitBaseViewModel.StartDate = new DateTime?();
        HabitBaseViewModel vm = habitBaseViewModel;
        List<HabitCheckInModel> checkIns = this._checkIns;
        HabitCheckInModel habitCheckInModel = checkIns != null ? checkIns.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id)) : (HabitCheckInModel) null;
        if (habitCheckInModel != null && (habitCheckInModel.Value >= habitCheckInModel.Goal || habitCheckInModel.CheckStatus == 1))
        {
          vm.Status = 2;
          vm.CompletedTime = habitCheckInModel.CheckinTime;
        }
        DisplayItemModel displayItemModel = new DisplayItemModel((TaskBaseViewModel) vm);
        tasks.Add(displayItemModel);
      }
      tasks.Sort((Comparison<DisplayItemModel>) ((a, b) => a.Status == b.Status && a.CompletedTime.HasValue && b.CompletedTime.HasValue ? b.CompletedTime.Value.CompareTo(a.CompletedTime.Value) : a.Status.CompareTo(b.Status)));
      foreach (DisplayItemModel displayItemModel in tasks)
      {
        displayItemModel.Selected = displayItemModel.Habit.Id == this.Data.FocusId;
        displayItemModel.DisplayColor = this.GetPriorityColor(displayItemModel.Priority).ToString();
        displayItemModel.Linked = this._linkedIds.Contains(displayItemModel.Id);
      }
      this.ReloadTask(tasks);
    }

    private void OnLoadMoreClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is DisplayItemModel dataContext) || dataContext.Parent == null)
        return;
      DisplayItemModel parent = dataContext.Parent;
      ObservableCollection<DisplayItemModel> models = this.TaskList.ItemsSource as ObservableCollection<DisplayItemModel>;
      if (models == null || parent.Section.Children == null)
        return;
      List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
      foreach (DisplayItemModel child in parent.Section.Children)
      {
        displayItemModelList.Add(child);
        if (child.IsOpen && child.IsTask)
        {
          List<DisplayItemModel> childrenModels = child.GetChildrenModels(false);
          displayItemModelList.AddRange((IEnumerable<DisplayItemModel>) childrenModels);
        }
      }
      displayItemModelList.ForEach((Action<DisplayItemModel>) (child => models.Remove(child)));
      models.Remove(dataContext);
      int index1 = models.IndexOf(parent);
      if (index1 < 0 || models[index1] == null)
        return;
      for (int index2 = 0; index2 < displayItemModelList.Count; ++index2)
        models.Insert(index1 + 1 + index2, displayItemModelList[index2]);
    }

    private void OnSectionClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      DisplayItemModel section = frameworkElement.DataContext as DisplayItemModel;
      if (section == null || !section.IsSection)
        return;
      section.IsOpen = !section.IsOpen;
      ObservableCollection<DisplayItemModel> models = this.TaskList.ItemsSource as ObservableCollection<DisplayItemModel>;
      if (models == null || section.Section.Children == null)
        return;
      List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
      foreach (DisplayItemModel child in section.Section.Children)
      {
        displayItemModelList.Add(child);
        if (child.IsOpen && child.IsTask)
        {
          List<DisplayItemModel> childrenModels = child.GetChildrenModels(false);
          displayItemModelList.AddRange((IEnumerable<DisplayItemModel>) childrenModels);
        }
      }
      if (section.IsOpen)
      {
        int index1 = models.IndexOf(section);
        if (index1 >= 0 && models[index1] != null)
        {
          for (int index2 = 0; index2 < displayItemModelList.Count; ++index2)
            models.Insert(index1 + 1 + index2, displayItemModelList[index2]);
          if (this.NeedLoadMore(section))
          {
            DisplayItemModel displayItemModel = DisplayItemModel.BuildLoadMore();
            displayItemModel.Parent = section;
            models.Insert(index1 + 1 + displayItemModelList.Count, displayItemModel);
          }
        }
      }
      else
      {
        displayItemModelList.ForEach((Action<DisplayItemModel>) (child => models.Remove(child)));
        DisplayItemModel displayItemModel = models.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.IsLoadMore && m.Parent == section));
        if (displayItemModel != null)
          models.Remove(displayItemModel);
      }
      if (!(this.GroupTitle.SelectedTitle == "Task") || this._projectIdentity == null)
        return;
      string name = section.Title;
      if (section.Section is AssigneeSection section1)
        name = section1.Assingee;
      if (section.IsOpen)
        SectionStatusDao.OpenProjectSection(this._projectIdentity.Id, name);
      else
        SectionStatusDao.CloseProjectSection(this._projectIdentity.Id, name);
    }

    private bool NeedLoadMore(DisplayItemModel section)
    {
      if (!string.IsNullOrEmpty(this.SearchText.Text) && section.Section.Children.Count > 5)
      {
        switch (section.Section.SectionId)
        {
          case "Task":
            return this._loadMoreTask;
          case "Habit":
            return this._loadMoreHabit;
          case "CommonTimer":
            return this._loadMoreTimer;
        }
      }
      return false;
    }

    public void Open() => this._parentPopup.IsOpen = true;

    public Popup GetParentPopup() => this._parentPopup;

    public void ResetSelectTitle()
    {
      this.SearchText.Text = string.Empty;
      this._selectedGroupTitle = "Task";
      this._projectIdentity = (ProjectIdentity) new TodayProjectIdentity();
      this.Data.Identity = this._projectIdentity;
    }

    public void Show() => this._parentPopup.IsOpen = true;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/pomofiltercontrol.xaml", UriKind.Relative));
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
          this.Root = (PomoFilterControl) target;
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.OnToasted);
          break;
        case 3:
          this.Container = (Grid) target;
          break;
        case 4:
          this.DetailGrid = (Grid) target;
          break;
        case 5:
          this.PomoSummary = (TaskDetailPomoSummaryControl) target;
          break;
        case 7:
          this.CompleteButton = (Button) target;
          this.CompleteButton.Click += new RoutedEventHandler(this.OnCompleteClick);
          break;
        case 8:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSelectTaskClick);
          break;
        case 9:
          this.ListGrid = (Grid) target;
          break;
        case 10:
          this.GroupTitle = (GroupTitle) target;
          break;
        case 11:
          this.SearchGrid = (Grid) target;
          break;
        case 12:
          this.SearchText = (EmojiEditor) target;
          break;
        case 13:
          this.SearchIcon = (Path) target;
          break;
        case 14:
          this.ExitSearchIcon = (Border) target;
          this.ExitSearchIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.ExitSearchClick);
          break;
        case 15:
          this.ClearIcon = (Border) target;
          this.ClearIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClearClick);
          break;
        case 16:
          this.ProjectName = (StackPanel) target;
          break;
        case 17:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFilterClick);
          break;
        case 18:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFilterClick);
          break;
        case 19:
          this.ProjectFilterPopup = (EscPopup) target;
          break;
        case 20:
          this.SelectTaskPanel = (Grid) target;
          break;
        case 21:
          this.TaskList = (ListView) target;
          break;
        case 26:
          this.EmptyPanel = (Grid) target;
          break;
        case 27:
          this.ToastBorder = (Border) target;
          break;
        case 28:
          this.ToastText = (TextBlock) target;
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
      switch (connectionId)
      {
        case 6:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.CheckBoxClick);
          ((ToggleButton) target).Checked += new RoutedEventHandler(this.OnItemCheckChanged);
          ((ToggleButton) target).Unchecked += new RoutedEventHandler(this.OnItemCheckChanged);
          break;
        case 22:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 23:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenPathClick);
          break;
        case 24:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSectionClick);
          break;
        case 25:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLoadMoreClick);
          break;
      }
    }
  }
}
