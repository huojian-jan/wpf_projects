// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.KanbanContainer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
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
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Event;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Kanban.Item;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.TaskList;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public class KanbanContainer : UserControl, IKanban, IBatchEditable, IComponentConnector
  {
    private BatchTaskEditHelper _batchHelper;
    private string _switchedColumnId;
    private string _currentColumnId;
    private long _current;
    private bool _inOperation;
    private System.Windows.Point _startPoint;
    private readonly DelayActionHandler _delayLoadHandler = new DelayActionHandler(200);
    private readonly BlockingList<ColumnViewModel> _columnViewModels = new BlockingList<ColumnViewModel>();
    private bool _autoScroll;
    private int _offset;
    internal KanbanContainer Root;
    internal Popup DragTaskPopup;
    internal KanbanItemPopupView PopupTaskItem;
    internal Grid TemplateGuidePopup;
    internal Border MenuPathGrid;
    internal Image FoldImage;
    internal StackPanel OptionPanel;
    internal Image SortIcon;
    internal EscPopup ChooseSortTypePopup;
    internal Grid MoreGrid;
    internal EscPopup MorePopup;
    internal Grid TitleGrid;
    internal EmojiTitleEditor TitleEditor;
    internal Grid ShareGrid;
    internal Image ShareImage;
    internal ScrollViewer KanbanScroller;
    internal Grid KanbanGrid;
    internal KanbanColumnCanvas ColumnContainer;
    internal Grid AddColumnControlGrid;
    internal Border AddColumnControl;
    internal Border CloseAddButton;
    internal StackPanel EmptyGrid;
    internal EmptyImage EmptyImage;
    internal TextBlock EmptyText;
    internal TextBlock EmptyAddText;
    private bool _contentLoaded;

    public SectionAddTaskViewModel AddingModel { get; private set; }

    public KanbanViewModel ViewModel => this.DataContext as KanbanViewModel;

    public KanbanContainer(ProjectIdentity project)
    {
      this.InitializeComponent();
      this.DataContext = (object) new KanbanViewModel(project, this);
      this.InitBatchHelper();
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    public void ShowBatchOperationDialog() => this._batchHelper.ShowOperationDialog();

    public void RemoveSelectedId(string id)
    {
      List<string> selectedTaskIds = this._batchHelper.SelectedTaskIds;
      // ISSUE: explicit non-virtual call
      if ((selectedTaskIds != null ? (__nonvirtual (selectedTaskIds.Count) > 1 ? 1 : 0) : 0) != 0)
        return;
      this._batchHelper.SelectedTaskIds?.Remove(id);
      foreach (ColumnViewModel columnViewModel in this._columnViewModels.ToList())
        columnViewModel.RemoveSelectedId(id);
    }

    public void SetSelectedTaskIds(List<string> taskIds)
    {
      List<string> source = new List<string>();
      foreach (ColumnViewModel columnViewModel in this._columnViewModels.ToList())
        source.AddRange(columnViewModel.GetSelectedTask() ?? (IEnumerable<string>) new List<string>());
      this._batchHelper.SelectedTaskIds = source.Distinct<string>().ToList<string>();
    }

    public List<string> GetSelectedTaskIds() => this._batchHelper.SelectedTaskIds;

    public void ReloadList() => this.Reload(false, true);

    public UIElement BatchOperaPlacementTarget() => (UIElement) this.TitleEditor;

    public event EventHandler<DisplayItemModel> TaskRemoved;

    public event EventHandler<DisplayItemModel> TaskAdded;

    public event EventHandler<List<string>> SetSelected;

    public void SetInOperation() => this._inOperation = true;

    public void CancelOperation() => this._inOperation = false;

    public bool IsInDragging() => this.DragTaskPopup.IsOpen;

    public string GetProjectId() => this.ViewModel.Identity?.GetProjectId();

    public bool IsTaskPopupOpened() => this.DragTaskPopup.IsOpen;

    public void ClearSelected()
    {
      EventHandler<List<string>> setSelected = this.SetSelected;
      if (setSelected != null)
        setSelected((object) this, (List<string>) null);
      this._batchHelper.ClearSelectedTaskIds();
    }

    public DisplayItemModel GetDraggingTask()
    {
      return this.DragTaskPopup.IsOpen ? (DisplayItemModel) this.PopupTaskItem.DataContext : (DisplayItemModel) null;
    }

    public KanbanViewModel GetViewModel() => this.ViewModel;

    public void Toast(string text)
    {
      Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.TryToastString((object) null, text);
    }

    public async Task ReloadColumn(string columnId, bool force = false) => this.Reload();

    public void DropTaskInColumn(DisplayItemModel task, ColumnViewModel dropColumn)
    {
      this.GetColumnControlById(dropColumn.ColumnId)?.OnTaskDrop(task);
    }

    public int GetColumnCount()
    {
      BlockingList<ColumnViewModel> columnViewModels = this._columnViewModels;
      return columnViewModels == null ? 0 : columnViewModels.GetCount((Predicate<ColumnViewModel>) (m => !m.IsPinned));
    }

    public async void AddColumn(string columnId, HorizontalDirection direction)
    {
      KanbanContainer kanbanContainer = this;
      if ((long) kanbanContainer._columnViewModels.GetCount((Predicate<ColumnViewModel>) (c => !c.IsPinned)) >= LimitCache.GetLimitByKey(Constants.LimitKind.KanbanNumber))
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ExceedMaxSectionCountMessage"), MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) kanbanContainer);
        customerDialog.ShowDialog();
      }
      else
      {
        kanbanContainer.AddColumnControl.Visibility = Visibility.Collapsed;
        ColumnViewModel model1 = kanbanContainer._columnViewModels.FirstOrDefault((Func<ColumnViewModel, bool>) (m => m.ColumnId == columnId));
        if (model1 == null)
          return;
        ColumnModel model2 = new ColumnModel()
        {
          id = Utils.GetGuid(),
          userId = Utils.GetCurrentUserIdInt().ToString(),
          projectId = kanbanContainer.ViewModel.Identity.Id,
          createdTime = new DateTime?(DateTime.Now),
          modifiedTime = new DateTime?(DateTime.Now),
          name = string.Empty,
          syncStatus = "new"
        };
        int num = kanbanContainer._columnViewModels.IndexOf(model1);
        model2.sortOrder = direction != HorizontalDirection.Left ? (num != kanbanContainer._columnViewModels.Count - 1 ? new long?(model1.SortOrder / 2L + kanbanContainer._columnViewModels[num + 1].SortOrder / 2L) : new long?(model1.SortOrder + 268435456L)) : (num == 0 || num > 0 && kanbanContainer._columnViewModels[num - 1].IsPinned ? new long?(model1.SortOrder - 268435456L) : new long?(model1.SortOrder / 2L + kanbanContainer._columnViewModels[num - 1].SortOrder / 2L));
        int index = direction == HorizontalDirection.Right ? num + 1 : num;
        kanbanContainer.InsertColumn(index, new ColumnViewModel(model2)
        {
          NewAdd = true,
          Identity = new ColumnProjectIdentity(kanbanContainer.ViewModel.Identity, model2.id)
        });
      }
    }

    public async void DeleteColumn(string columnId, bool prompt)
    {
      KanbanContainer kanbanContainer = this;
      ColumnViewModel model;
      if (prompt)
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString(nameof (DeleteColumn)), Utils.GetString("DeleteColumnHint"), Utils.GetString("Delete"), Utils.GetString("Cancel"));
        customerDialog.Owner = Window.GetWindow((DependencyObject) kanbanContainer);
        bool? nullable = customerDialog.ShowDialog();
        bool flag = true;
        if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        {
          model = (ColumnViewModel) null;
          return;
        }
      }
      List<ColumnViewModel> list = kanbanContainer._columnViewModels.ToList();
      model = list.FirstOrDefault<ColumnViewModel>((Func<ColumnViewModel, bool>) (m => m.ColumnId == columnId));
      if (model != null)
      {
        ColumnViewModel primaryModel = list[0];
        list.Remove(model);
        kanbanContainer.SetupColumn((IEnumerable<ColumnViewModel>) list);
        UtilLog.Info("kanbanContainer.DeleteColumn : " + columnId);
        await ColumnDao.DeleteColumn(columnId);
        await TaskService.BatchDeleteTaskInColumn(model.ProjectId, columnId, primaryModel.ColumnId);
        SyncManager.TryDelaySync();
        primaryModel = (ColumnViewModel) null;
      }
      kanbanContainer.ShowAddColumn();
      model = (ColumnViewModel) null;
    }

    public void StartDragTask(string columnId, DisplayItemModel model)
    {
      if (!this.IsEnable())
        return;
      TaskDetailWindow.TryCloseWindow();
      TaskDragHelper.Register(columnId, model);
      this.PopupTaskItem.DataContext = (object) model;
      if (this.DragTaskPopup.IsOpen)
        return;
      this.DragTaskPopup.IsOpen = true;
      Mouse.OverrideCursor = Cursors.Hand;
    }

    private void MoveTaskPopup(MouseEventArgs e)
    {
      if (!this.IsEnable())
        return;
      System.Windows.Point position = e.GetPosition((IInputElement) this.MenuPathGrid);
      this.DragTaskPopup.HorizontalOffset = position.X - 147.0;
      this.DragTaskPopup.VerticalOffset = position.Y - 20.0;
    }

    private async void OnContainerMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.DragTaskPopup.IsOpen)
      {
        this.DragTaskPopup.IsOpen = false;
        Mouse.OverrideCursor = (Cursor) null;
        this.KanbanGrid.ReleaseMouseCapture();
        this.DropTask();
        e.Handled = true;
      }
      this._autoScroll = false;
    }

    private void DropTask()
    {
      if (!(this.PopupTaskItem.DataContext is DisplayItemModel dataContext))
        return;
      List<DisplayItemModel> batchModels = dataContext.BatchModels;
      // ISSUE: explicit non-virtual call
      if ((batchModels != null ? (__nonvirtual (batchModels.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        EventHandler<List<string>> batchTaskDrop = this.BatchTaskDrop;
        if (batchTaskDrop != null)
          batchTaskDrop((object) this, dataContext.DragTaskIds);
        this.OnBatchTaskDrop(dataContext);
      }
      else
        this.OnTaskDrop();
    }

    public async Task<List<string>> GetColumnNames()
    {
      List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(this.ViewModel.Identity.Id);
      return columnsByProjectId == null || !columnsByProjectId.Any<ColumnModel>() ? new List<string>() : columnsByProjectId.Select<ColumnModel, string>((Func<ColumnModel, string>) (column => column.name)).ToList<string>();
    }

    public async void Reload(bool forceReload = false, bool restoreSelect = true)
    {
      KanbanContainer kanbanContainer = this;
      if (kanbanContainer._inOperation && !forceReload || !kanbanContainer.IsVisible)
        return;
      await kanbanContainer.Load(kanbanContainer.ViewModel.Identity.Copy(kanbanContainer.ViewModel.Identity), forceReload, restoreSelect: restoreSelect);
      kanbanContainer.ShowAddColumn();
    }

    private bool IsEnable()
    {
      if (this.ViewModel.Identity is NormalProjectIdentity)
      {
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == this.ViewModel.Identity.Id));
        if (projectModel != null)
          return projectModel.IsEnable();
      }
      return true;
    }

    public event EventHandler<List<string>> BatchTaskDrop;

    private bool IsDefaultColumn(string columnId)
    {
      return this._columnViewModels.Count > 0 && this._columnViewModels[0].ColumnId == columnId;
    }

    private void TryLoad(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() => this.Reload(true, true)));
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.SyncDone -= new EventHandler<SyncResult>(this.OnSyncChanged);
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnThemeModeChanged);
      DataChangedNotifier.ProjectChanged -= new EventHandler(this.OnProjectChanged);
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      DataChangedNotifier.ProjectGroupChanged -= new EventHandler<ProjectGroupModel>(this.OnProjectGroupChanged);
      DataChangedNotifier.TagChanged -= new EventHandler<TagModel>(this.OnTagChanged);
      DataChangedNotifier.PomoChanged -= new EventHandler<string>(this.OnPomoChanged);
      DataChangedNotifier.TaskDefaultChanged -= new EventHandler(this.OnDefaultChanged);
      DataChangedNotifier.ProjectColumnChanged -= new EventHandler<string>(this.OnColumnChanged);
      DataChangedNotifier.HabitsChanged -= new EventHandler(this.HabitsChanged);
      DataChangedNotifier.HabitCheckInChanged -= new EventHandler<HabitCheckInModel>(this.HabitsChanged);
      DataChangedNotifier.EventArchivedChanged -= new EventHandler(this.OnEventChanged);
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      CalendarEventChangeNotifier.TitleChanged -= new EventHandler<TextExtra>(this.OnCalendarTitleChanged);
      CalendarEventChangeNotifier.SummaryChanged -= new EventHandler<TextExtra>(this.OnCalendarSummaryChanged);
      CalendarEventChangeNotifier.Deleted -= new EventHandler<string>(this.OnEventChanged);
      CalendarEventChangeNotifier.Restored -= new EventHandler<string>(this.OnEventChanged);
      CalendarEventChangeNotifier.Changed -= new EventHandler<CalendarEventModel>(this.OnEventChanged);
      CalendarEventChangeNotifier.RemoteChanged -= new EventHandler(this.OnEventChanged);
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "HideComplete");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowSubtasks");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowDetails");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnShowAddChanged), "KbShowAdd");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnColumnWidthChanged), "KbSize");
      this._delayLoadHandler.StopAndClear();
    }

    private void BindEvents()
    {
      DataChangedNotifier.SyncDone += new EventHandler<SyncResult>(this.OnSyncChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnThemeModeChanged);
      DataChangedNotifier.ProjectChanged += new EventHandler(this.OnProjectChanged);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      DataChangedNotifier.ProjectGroupChanged += new EventHandler<ProjectGroupModel>(this.OnProjectGroupChanged);
      DataChangedNotifier.TagChanged += new EventHandler<TagModel>(this.OnTagChanged);
      DataChangedNotifier.PomoChanged += new EventHandler<string>(this.OnPomoChanged);
      DataChangedNotifier.TaskDefaultChanged += new EventHandler(this.OnDefaultChanged);
      DataChangedNotifier.ProjectColumnChanged += new EventHandler<string>(this.OnColumnChanged);
      DataChangedNotifier.HabitsChanged += new EventHandler(this.HabitsChanged);
      DataChangedNotifier.HabitCheckInChanged += new EventHandler<HabitCheckInModel>(this.HabitsChanged);
      DataChangedNotifier.EventArchivedChanged += new EventHandler(this.OnEventChanged);
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      CalendarEventChangeNotifier.TitleChanged += new EventHandler<TextExtra>(this.OnCalendarTitleChanged);
      CalendarEventChangeNotifier.SummaryChanged += new EventHandler<TextExtra>(this.OnCalendarSummaryChanged);
      CalendarEventChangeNotifier.Deleted += new EventHandler<string>(this.OnEventChanged);
      CalendarEventChangeNotifier.Restored += new EventHandler<string>(this.OnEventChanged);
      CalendarEventChangeNotifier.Changed += new EventHandler<CalendarEventModel>(this.OnEventChanged);
      CalendarEventChangeNotifier.RemoteChanged += new EventHandler(this.OnEventChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "HideComplete");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowSubtasks");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowDetails");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnShowAddChanged), "KbShowAdd");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnColumnWidthChanged), "KbSize");
      this._delayLoadHandler.SetAction(new EventHandler(this.TryLoad));
      this.TitleEditor.SetMaxWidth(this.ActualWidth - 150.0);
      this.TitleEditor.SetCheckFunc(new Func<string, string>(this.CheckTitleValid));
    }

    private void OnColumnWidthChanged(object sender, PropertyChangedEventArgs e)
    {
      this.ColumnContainer.ColumnWidthChanged();
      if (this.KanbanScroller.HorizontalOffset + this.ActualWidth <= this.ColumnContainer.Width)
        return;
      this.KanbanScroller.ScrollToHorizontalOffset(this.ColumnContainer.Width - this.ActualWidth - 10.0);
    }

    private void OnShowAddChanged(object sender, PropertyChangedEventArgs e)
    {
      this.ColumnContainer.SetShowAddInColumn();
    }

    private void OnDisplayChanged(object sender, PropertyChangedEventArgs e)
    {
      this.Reload(true, true);
    }

    private string CheckTitleValid(string text) => this.ViewModel.Identity?.CheckTitleValid(text);

    private void OnTitleTextChanged(object sender, string e)
    {
      this.ViewModel.Identity?.SaveTitle(e);
    }

    private void OnTagChanged(object sender, TagModel e)
    {
      if (this.Visibility != Visibility.Visible || !(this.ViewModel.Identity is TagProjectIdentity identity1) || !(identity1.Tag == e?.name))
        return;
      TagModel tagByName = CacheManager.GetTagByName(identity1.Id);
      if (tagByName == null)
        return;
      TagProjectIdentity identity2 = new TagProjectIdentity(tagByName);
      if (tagByName.viewMode != "kanban")
      {
        Utils.FindParent<ListViewContainer>((DependencyObject) this)?.SelectProject((ProjectIdentity) identity2);
      }
      else
      {
        this.ViewModel.SetIdentity((ProjectIdentity) identity2);
        this.Reload(true, true);
      }
    }

    private void OnProjectGroupChanged(object sender, ProjectGroupModel e)
    {
      if (this.Visibility != Visibility.Visible || !(this.ViewModel.Identity is GroupProjectIdentity identity1) || !(identity1.Id == e?.id))
        return;
      ProjectGroupModel groupById = CacheManager.GetGroupById(identity1.Id);
      if (groupById == null)
        return;
      List<ProjectModel> projectsInGroup = CacheManager.GetProjectsInGroup(groupById.id);
      GroupProjectIdentity identity2 = new GroupProjectIdentity(groupById, projectsInGroup);
      if (groupById.viewMode != "kanban")
      {
        Utils.FindParent<ListViewContainer>((DependencyObject) this)?.SelectProject((ProjectIdentity) identity2);
      }
      else
      {
        this.ViewModel.SetIdentity((ProjectIdentity) identity2);
        this.Reload(true, true);
      }
    }

    private void OnFilterChanged(object sender, FilterChangeArgs e)
    {
      if (this.Visibility != Visibility.Visible || !(this.ViewModel.Identity is FilterProjectIdentity identity1) || !(identity1.Id == e?.Filter?.id))
        return;
      FilterModel filterById = CacheManager.GetFilterById(identity1.Id);
      if (filterById == null)
        return;
      FilterProjectIdentity identity2 = new FilterProjectIdentity(filterById);
      if (filterById.viewMode != "kanban")
      {
        Utils.FindParent<ListViewContainer>((DependencyObject) this)?.SelectProject((ProjectIdentity) identity2);
      }
      else
      {
        this.ViewModel.SetIdentity((ProjectIdentity) identity2);
        this.Reload(true, true);
      }
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      BlockingSet<string> changedIds = new BlockingSet<string>();
      changedIds.AddRange((IEnumerable<string>) e.SortOrderChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.DateChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.DeletedChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.BatchChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.PinChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.StatusChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.KindChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.ProjectChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.PriorityChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.TagChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.AssignChangedIds.Value);
      changedIds.AddRange((IEnumerable<string>) e.CheckItemChangedIds.Value);
      e.AddIds.AddRange(e.UndoDeletedIds);
      if (e.AddIds.Any())
      {
        List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this.ViewModel.Identity, e.AddIds.ToList());
        // ISSUE: explicit non-virtual call
        if ((matchedTasks != null ? (__nonvirtual (matchedTasks.Count) > 0 ? 1 : 0) : 0) != 0)
          goto label_3;
      }
      List<TaskBaseViewModel> sourceModels = this.ViewModel.SourceModels;
      if ((sourceModels != null ? (sourceModels.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m => changedIds.Contains(m.Id))) ? 1 : 0) : 0) == 0)
      {
        this._columnViewModels.Do((Action) (() => this._columnViewModels.Value.ForEach((Action<ColumnViewModel>) (c => c.GetListViewModel()?.CheckTaskChanged(e)))));
        return;
      }
label_3:
      this.Reload(true, true);
    }

    private void OnDefaultChanged(object sender, EventArgs e)
    {
      this.ColumnContainer.ReloadQuickAddView();
    }

    private void OnPomoChanged(object sender, string e) => this.Reload(true, true);

    private void OnProjectChanged(object sender, EventArgs e)
    {
      if (this.Visibility != Visibility.Visible)
        return;
      NormalProjectIdentity pid = this.ViewModel.Identity as NormalProjectIdentity;
      if (pid == null)
        return;
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == pid.Id));
      if (project == null)
        return;
      NormalProjectIdentity identity = new NormalProjectIdentity(project);
      if (project.viewMode != "kanban")
      {
        Utils.FindParent<ListViewContainer>((DependencyObject) this)?.SelectProject((ProjectIdentity) new NormalProjectIdentity(project));
      }
      else
      {
        this.ViewModel.SetIdentity((ProjectIdentity) identity);
        this.Reload(true, true);
      }
    }

    private void HabitsChanged(object sender, object e)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (!(this.ViewModel.Identity is TodayProjectIdentity) && !(this.ViewModel.Identity is WeekProjectIdentity) || !LocalSettings.Settings.HabitInToday)
          return;
        this.Reload(true, true);
      }));
    }

    private void OnEventChanged(object sender, object e)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (!(this.ViewModel.Identity is SmartProjectIdentity) && !(this.ViewModel.Identity is FilterProjectIdentity))
          return;
        this.Reload(true, true);
      }));
    }

    private void OnCalendarSummaryChanged(object sender, TextExtra e)
    {
      List<TaskBaseViewModel> sourceModels = this.ViewModel.SourceModels;
      List<TaskBaseViewModel> list = sourceModels != null ? sourceModels.ToList<TaskBaseViewModel>() : (List<TaskBaseViewModel>) null;
      if (list == null)
        return;
      foreach (TaskBaseViewModel taskBaseViewModel in list)
      {
        if (taskBaseViewModel.EntityId == e.Id)
          taskBaseViewModel.Content = e.Text;
      }
    }

    private void OnCalendarTitleChanged(object sender, TextExtra e)
    {
      List<TaskBaseViewModel> sourceModels = this.ViewModel.SourceModels;
      List<TaskBaseViewModel> list = sourceModels != null ? sourceModels.ToList<TaskBaseViewModel>() : (List<TaskBaseViewModel>) null;
      if (list == null)
        return;
      foreach (TaskBaseViewModel taskBaseViewModel in list)
      {
        if (taskBaseViewModel.EntityId == e.Id)
          taskBaseViewModel.Title = e.Text;
      }
    }

    private void OnColumnChanged(object sender, string e)
    {
      if (!(e == this.ViewModel.Identity?.Id))
        return;
      this.Reload(true, true);
    }

    private void OnThemeModeChanged(object sender, EventArgs e)
    {
      if (!this.IsVisible)
        return;
      this.Reload(true, true);
    }

    private void OnSyncChanged(object sender, SyncResult changed)
    {
      if (!changed.ColumnChanged && !changed.RemoteTasksChanged || this.Visibility != Visibility.Visible)
        return;
      this.Reload(true, true);
    }

    private void InitBatchHelper()
    {
      this._batchHelper = new BatchTaskEditHelper((IBatchEditable) this);
    }

    private void OnBatchTaskChanged(object sender, List<string> taskIds)
    {
      this.Reload(false, true);
      SyncManager.TryDelaySync();
    }

    private void InitView(ProjectModel project)
    {
      if (project == null)
      {
        this.ShareGrid.Visibility = Visibility.Collapsed;
        this.OptionPanel.Visibility = Visibility.Visible;
        this.AddColumnControl.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.SetShareImage(project);
        this.AddColumnControl.Visibility = Visibility.Visible;
      }
    }

    public async Task Load(
      ProjectIdentity identity,
      bool forceReload = false,
      bool needPull = false,
      string taskId = null,
      bool restoreSelect = true)
    {
      if (this.DragTaskPopup.IsOpen || this.ColumnDragging || this.HasNewAddColumnEditing)
        return;
      if (this.ViewModel.Identity?.Id != identity.Id)
      {
        this.KanbanScroller.ScrollToHorizontalOffset(0.0);
        this.RemoveAddingTaskModel();
      }
      if (this.ViewModel.Identity?.Id != identity.Id || this.ViewModel.Identity is NormalProjectIdentity identity1 && identity is NormalProjectIdentity normalProjectIdentity1 && identity1.Project?.permission != normalProjectIdentity1.Project?.permission)
        forceReload = true;
      if (!forceReload)
        return;
      if (!restoreSelect)
        this._batchHelper.ClearSelectedTaskIds();
      this.InitView(identity is NormalProjectIdentity normalProjectIdentity2 ? normalProjectIdentity2.Project : (ProjectModel) null);
      this._batchHelper.ProjectIdentity = identity;
      this.ViewModel.SetIdentity(identity);
      this.TitleEditor.SetText(this.ViewModel.Name);
      this.TitleEditor.SetEnable(this.ViewModel.Identity is NormalProjectIdentity identity2 && identity2.CanEdit && identity2.Project != null && !identity2.Project.Isinbox || this.ViewModel.Identity is GroupProjectIdentity || this.ViewModel.Identity is FilterProjectIdentity);
      await this.ViewModel.LoadTasks(true, needPull);
      if (string.IsNullOrEmpty(taskId))
        return;
      this.NavigateTask(taskId);
    }

    private void SetShareImage(ProjectModel project)
    {
      if (((int) project.closed ?? (TeamDao.IsTeamExpired(project.teamId) ? 1 : 0)) != 0)
      {
        this.ShareGrid.Visibility = Visibility.Collapsed;
        this.OptionPanel.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.ShareGrid.Visibility = project.userCount < 2 ? Visibility.Collapsed : Visibility.Visible;
        this.OptionPanel.Visibility = Visibility.Visible;
        this.ShareImage.DataContext = (object) "";
        this.ShareImage.DataContext = (object) project.id;
        switch (project.permission)
        {
          case "read":
            Grid shareGrid1 = this.ShareGrid;
            System.Windows.Controls.ToolTip toolTip1 = new System.Windows.Controls.ToolTip();
            toolTip1.Content = (object) Utils.GetString("ReadOnly");
            shareGrid1.ToolTip = (object) toolTip1;
            break;
          case "comment":
            Grid shareGrid2 = this.ShareGrid;
            System.Windows.Controls.ToolTip toolTip2 = new System.Windows.Controls.ToolTip();
            toolTip2.Content = (object) Utils.GetString("CanComment");
            shareGrid2.ToolTip = (object) toolTip2;
            break;
          default:
            Grid shareGrid3 = this.ShareGrid;
            System.Windows.Controls.ToolTip toolTip3 = new System.Windows.Controls.ToolTip();
            toolTip3.Content = (object) Utils.GetString("Editable");
            shareGrid3.ToolTip = (object) toolTip3;
            break;
        }
      }
    }

    private bool CheckIfColumnsChanged(List<ColumnModel> left, List<ColumnModel> right)
    {
      if (left == null != (right == null))
        return true;
      if (left == null)
        left = new List<ColumnModel>();
      if (right == null)
        right = new List<ColumnModel>();
      if (left.Count != right.Count)
        return true;
      for (int index = 0; index < left.Count; ++index)
      {
        ColumnModel columnModel1 = left[index];
        ColumnModel columnModel2 = right[index];
        if (columnModel1.id != columnModel2.id || columnModel1.projectId != columnModel2.projectId || columnModel1.deleted != columnModel2.deleted || columnModel1.name != columnModel2.name)
          return true;
        long? sortOrder1 = columnModel1.sortOrder;
        long? sortOrder2 = columnModel2.sortOrder;
        if (!(sortOrder1.GetValueOrDefault() == sortOrder2.GetValueOrDefault() & sortOrder1.HasValue == sortOrder2.HasValue))
          return true;
      }
      return false;
    }

    private void ShowAddColumn()
    {
      if (this.AddColumnControl.Visibility == Visibility.Visible || this.HasNewAddColumnEditing)
        return;
      this.AddColumnControl.Visibility = Visibility.Visible;
    }

    private void OnContainerMouseMove(object sender, MouseEventArgs e)
    {
      if (this.DragTaskPopup.IsOpen && e.LeftButton == MouseButtonState.Released)
      {
        this.DragTaskPopup.IsOpen = false;
        Mouse.OverrideCursor = (Cursor) null;
        this.KanbanGrid.ReleaseMouseCapture();
        this.DropTask();
      }
      else
      {
        this._autoScroll = false;
        if (this.DragTaskPopup.IsOpen || this.ColumnDragging)
          this.ShiftKanbanOnScrollEdge(e.GetPosition((IInputElement) this).X);
        if (!this.DragTaskPopup.IsOpen)
          return;
        this.MoveTaskPopup(e);
        System.Windows.Point position = Mouse.GetPosition((IInputElement) Application.Current?.MainWindow);
        Utils.FindParent<TaskView>((DependencyObject) this)?.OnTaskDragging((object) this, new DragMouseEvent(position.X, position.Y));
        ItemDragNotifier.NotifyMouseMove(e);
      }
    }

    private void ShiftKanbanOnScrollEdge(double offset)
    {
      this._offset = (int) offset;
      if (offset >= -10.0 && offset <= 10.0)
      {
        this.AutoScrollContainer(true, this._offset);
      }
      else
      {
        if (offset < this.ActualWidth - 5.0)
          return;
        this.AutoScrollContainer(false, this._offset);
      }
    }

    private async void AutoScrollContainer(bool isLeft, int offset)
    {
      this._autoScroll = true;
      if (isLeft)
      {
        this.KanbanScroller.ScrollToHorizontalOffset(this.KanbanScroller.HorizontalOffset - 4.0);
        while (this._autoScroll && this.KanbanScroller.HorizontalOffset > 0.0 && this._offset == offset)
        {
          await Task.Delay(60);
          this.KanbanScroller.ScrollToHorizontalOffset(this.KanbanScroller.HorizontalOffset - 1.0);
        }
      }
      else
      {
        this.KanbanScroller.ScrollToHorizontalOffset(this.KanbanScroller.HorizontalOffset + 4.0);
        while (this._autoScroll && this.KanbanScroller.HorizontalOffset < this.KanbanScroller.ScrollableWidth && this._offset == offset)
        {
          await Task.Delay(60);
          this.KanbanScroller.ScrollToHorizontalOffset(this.KanbanScroller.HorizontalOffset + 1.0);
        }
      }
    }

    private async void OnBatchTaskDrop(DisplayItemModel model)
    {
      this.DragTaskPopup.IsOpen = false;
      if (this._columnViewModels.Count > 0)
      {
        ColumnViewModel dropColumn = this._columnViewModels.FirstOrDefault((Func<ColumnViewModel, bool>) (item => item.CanDrop));
        if (dropColumn != null)
        {
          if (!string.IsNullOrEmpty(dropColumn.ColumnId))
            await this.BatchDropOnAnotherColumn(model, dropColumn);
        }
        else
        {
          ColumnViewModel columnViewModel = this._columnViewModels.FirstOrDefault((Func<ColumnViewModel, bool>) (item => item.ColumnId == model.ColumnId));
          if (columnViewModel != null && columnViewModel.MouseOver)
            await this.BatchDropOnSameColumn(model);
          this.Reload(true);
        }
      }
      await Task.Delay(100);
    }

    private async Task BatchDropOnSameColumn(DisplayItemModel model)
    {
      KanbanColumnView columnById = this.ColumnContainer.GetColumnById(model.ColumnId);
      if (columnById == null)
        return;
      await columnById.OnBatchTaskDrop(model);
    }

    private async Task BatchDropOnAnotherColumn(DisplayItemModel model, ColumnViewModel dropColumn)
    {
      if (dropColumn.ColumnId.StartsWith("tag:"))
        return;
      if (dropColumn.ColumnId.StartsWith("date:"))
      {
        int num1 = await TaskService.BatchSetDate(model.DragTaskIds, dropColumn.GetDate()) ? 1 : 0;
      }
      else if (dropColumn.ColumnId.StartsWith("priority:"))
      {
        int num2 = await TaskService.BatchSetPriority(model.DragTaskIds, dropColumn.GetPriority()) ? 1 : 0;
      }
      if (dropColumn.ColumnId.StartsWith("project:"))
      {
        ProjectModel projectById = CacheManager.GetProjectById(dropColumn.GetProject());
        if (projectById != null)
          await TaskService.BatchMoveParentTaskProject(model.DragTaskIds, projectById);
      }
      else if (dropColumn.ColumnId.StartsWith("assign:"))
      {
        string assignee = dropColumn.GetAssignee();
        if (!string.IsNullOrEmpty(assignee) && assignee != "-1")
        {
          List<TaskBaseViewModel> tasksByIds = TaskCache.GetTasksByIds(model.DragTaskIds);
          List<string> list = tasksByIds != null ? tasksByIds.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (m => m.ProjectId)).Distinct<string>().ToList<string>() : (List<string>) null;
          // ISSUE: explicit non-virtual call
          if (list != null && __nonvirtual (list.Count) > 0)
          {
            foreach (string projectId in list)
            {
              List<ShareUserModel> projectUsers = AvatarHelper.GetProjectUsers(projectId);
              if (projectUsers == null || projectUsers.Count == 0 || projectUsers.All<ShareUserModel>((Func<ShareUserModel, bool>) (a => (a.userId.ToString() ?? "") != assignee)))
              {
                this.Toast(Utils.GetString("ChangeAssigneeError"));
                this.Reload(true);
                return;
              }
            }
          }
        }
        await TaskService.BatchSetAssignee(model.DragTaskIds, assignee);
      }
      else
        await TaskService.BatchSetColumn(model.DragTaskIds, dropColumn.ColumnId);
      foreach (string treeTopId in TaskDao.GetTreeTopIds(model.DragTaskIds, model.ProjectId))
        await TaskDao.UpdateParent(treeTopId, string.Empty);
      this.Reload(true);
      SyncManager.TryDelaySync();
    }

    private async Task OnTaskDrop()
    {
      KanbanContainer kanbanContainer = this;
      // ISSUE: explicit non-virtual call
      __nonvirtual (kanbanContainer.CancelOperation());
      TaskDragHelper.OnTaskDrop((IEnumerable<ColumnViewModel>) kanbanContainer._columnViewModels.Value, (IKanban) kanbanContainer);
    }

    private void OnAddColumn()
    {
      if (this.AddColumnControl.Visibility != Visibility.Visible)
      {
        this.ColumnContainer.ChildrenList.FirstOrDefault((Func<KanbanColumnView, bool>) (item =>
        {
          if (item == null)
            return false;
          ColumnViewModel model = item.GetModel();
          return model != null && model.NewAdd;
        }))?.TryFocus();
      }
      else
      {
        ColumnViewModel columnViewModel = this._columnViewModels.Value.LastOrDefault<ColumnViewModel>();
        if (columnViewModel == null)
          return;
        ProjectModel projectById = CacheManager.GetProjectById(this.ViewModel.Identity?.GetProjectId());
        if (projectById != null)
        {
          projectById.ShowAddColumn = true;
          ProjectDao.TryUpdateProject(projectById);
        }
        this.ViewModel.ShowAdd = true;
        this.AddColumn(columnViewModel.ColumnId, HorizontalDirection.Right);
        this.KanbanScroller.ScrollToRightEnd();
      }
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (!Utils.IfShiftPressed())
        return;
      ScrollViewer scrollViewer = (ScrollViewer) sender;
      scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - (double) e.Delta);
      e.Handled = true;
    }

    private void MoreGridClick(object sender, MouseButtonEventArgs e)
    {
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      bool flag = false;
      types.Add(new CustomMenuItemViewModel((object) "showComplete", Utils.GetString(LocalSettings.Settings.HideComplete ? "ShowCompleted" : "HideCompleted"), Utils.GetImageSource(LocalSettings.Settings.HideComplete ? "showCompletedDrawingImage" : "HideCompletedDrawingImage")));
      types.Add(new CustomMenuItemViewModel((object) "showDetail", Utils.GetString(LocalSettings.Settings.ShowDetails ? "HideDetails" : "ShowDetails"), Utils.GetImageSource(LocalSettings.Settings.ShowDetails ? "HideDetailsDrawingImage" : "showDetailsDrawingImage")));
      types.Add(new CustomMenuItemViewModel((object) "showCountDown", Utils.GetString(LocalSettings.Settings.ShowCountDown ? "ShowTaskDate" : "ShowCountDown"), Utils.GetImageSource(LocalSettings.Settings.ShowCountDown ? "TimeDrawingLine" : "SwitchCountDownDrawingImage")));
      types.Add(new CustomMenuItemViewModel((object) "DisplaySetting", Utils.GetString("DisplaySetting"), Utils.GetIcon("IcDisplaySetting")));
      if (this.ViewModel.Enable && this.ViewModel.SortOption.groupBy == "sortOrder")
      {
        flag = true;
        types.Add(new CustomMenuItemViewModel((object) "addSection", Utils.GetString("AddSection"), Utils.GetImageSource("AddDrawingImage")));
      }
      if (this.ViewModel.Identity is NormalProjectIdentity)
      {
        ProjectModel projectById = CacheManager.GetProjectById(this.ViewModel.Identity.Id);
        if (projectById != null)
        {
          if (projectById.userCount <= 1 && !projectById.Isinbox)
          {
            flag = true;
            types.Add(new CustomMenuItemViewModel((object) "share", Utils.GetString("Share"), Utils.GetImageSource("cooperationDrawingImage")));
          }
          if (projectById.IsProjectPermit())
          {
            flag = true;
            types.Add(new CustomMenuItemViewModel((object) "listActivities", Utils.GetString("ListActivitiesPro"), Utils.GetImageSource("ProjectActivitiesDrawingImage")));
          }
        }
      }
      if (flag)
        types.Insert(4, new CustomMenuItemViewModel((object) null));
      List<string> switchViewModes = this.ViewModel.Identity.GetSwitchViewModes();
      SwitchListViewControl topTabControl = switchViewModes != null ? new SwitchListViewControl() : (SwitchListViewControl) null;
      if (switchViewModes != null)
      {
        topTabControl.ViewSelected += new EventHandler<string>(this.OnSwitchView);
        topTabControl.SetButtonStatus(new bool?(false), new bool?(true), switchViewModes.Contains("timeline") ? new bool?(false) : new bool?());
      }
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.MorePopup, (ITabControl) topTabControl);
      customMenuList.Operated += new EventHandler<object>(this.OnMoreItemSelected);
      customMenuList.Show();
    }

    private void OnMoreItemSelected(object sender, object e)
    {
      if (!(e is string str) || str == null)
        return;
      switch (str.Length)
      {
        case 5:
          if (!(str == "share"))
            break;
          this.OnShareClick((object) null, (RoutedEventArgs) null);
          break;
        case 10:
          switch (str[0])
          {
            case 'a':
              if (!(str == "addSection"))
                return;
              this.OnAddColumn();
              return;
            case 's':
              if (!(str == "showDetail"))
                return;
              this.OnShowDetailClick();
              return;
            default:
              return;
          }
        case 12:
          if (!(str == "showComplete"))
            break;
          this.OnHideCompleteClick();
          break;
        case 13:
          if (!(str == "showCountDown"))
            break;
          this.CountDownOrTaskDateSwitch();
          break;
        case 14:
          switch (str[0])
          {
            case 'D':
              if (!(str == "DisplaySetting"))
                return;
              this.ShowDisplaySetting();
              return;
            case 'l':
              if (!(str == "listActivities"))
                return;
              this.OnListActivityClick();
              return;
            default:
              return;
          }
      }
    }

    private void ShowDisplaySetting()
    {
      KanbanDisplaySettingWindow displaySettingWindow = new KanbanDisplaySettingWindow(UserDao.IsPro() && this.ViewModel.Identity is TodayProjectIdentity || this.ViewModel.Identity is TomorrowProjectIdentity || this.ViewModel.Identity is WeekProjectIdentity || this.ViewModel.Identity is FilterProjectIdentity identity && identity.Filter.ContainsDate());
      displaySettingWindow.Owner = Window.GetWindow((DependencyObject) this);
      displaySettingWindow.ShowDialog();
    }

    private async void SelectOrderClick(object sender, MouseButtonEventArgs e)
    {
      KanbanContainer kanbanContainer = this;
      if (kanbanContainer.ViewModel.Identity == null)
        return;
      ProjectIdentity identity = kanbanContainer.ViewModel.Identity;
      List<SortTypeViewModel> projectSortTypeModels;
      if (!(identity is GroupProjectIdentity groupProjectIdentity))
      {
        TagProjectIdentity tagProjectIdentity = identity as TagProjectIdentity;
        if (tagProjectIdentity == null)
        {
          switch (identity)
          {
            case NormalProjectIdentity normalProjectIdentity:
              projectSortTypeModels = SortOptionHelper.GetNormalProjectSortTypeModels(normalProjectIdentity.Project.IsShareList(), normalProjectIdentity.Project.IsNote);
              break;
            case FilterProjectIdentity filterProjectIdentity:
              projectSortTypeModels = SortOptionHelper.GetSmartProjectSortTypeModels(filterProjectIdentity.OnlyNote);
              break;
            default:
              projectSortTypeModels = SortOptionHelper.GetSmartProjectSortTypeModels();
              break;
          }
        }
        else
          projectSortTypeModels = SortOptionHelper.GetTagProjectSortTypeModels(CacheManager.GetTags().Any<TagModel>((Func<TagModel, bool>) (t => t.parent == tagProjectIdentity.Tag)));
      }
      else
        projectSortTypeModels = SortOptionHelper.GetGroupProjectSortTypeModels(groupProjectIdentity.DisplayKind, groupProjectIdentity.ContainsShare);
      SortTypeSelector sortTypeSelector = new SortTypeSelector(kanbanContainer.ViewModel.Identity, projectSortTypeModels, kanbanContainer.ViewModel.Identity.SortOption, true, kanbanContainer.ChooseSortTypePopup);
      sortTypeSelector.SortOptionSelect += new EventHandler<SortOption>(kanbanContainer.OnSortOptionSelect);
      sortTypeSelector.ResetSortOrder += new EventHandler<int>(kanbanContainer.OnResetDateOrderClick);
      sortTypeSelector.Show();
    }

    private async void OnResetDateOrderClick(object sender, int e)
    {
      this.ChooseSortTypePopup.IsOpen = false;
      await TaskSortOrderService.DeleteAllSortOrderBySortOptionInListId(this.ViewModel.Identity.SortOption, this.ViewModel.Identity.GetSortProjectId());
      DataChangedNotifier.NotifySortOptionChanged(this.ViewModel.Identity.CatId);
      SyncManager.TryDelaySync();
    }

    private async void OnSortOptionSelect(object sender, SortOption sortOption)
    {
      if (this.ViewModel?.Identity == null)
        return;
      UtilLog.Info(string.Format("Kanban.Om SetSortType {0},type {1}", (object) this.ViewModel.Identity.Id, (object) sortOption));
      (await ProjectTaskDataProvider.GetProjectData(this.ViewModel.Identity)).SaveSortOption(sortOption);
      this.ViewModel.Identity.SortOption = sortOption;
      this.Load(this.ViewModel.Identity, true);
      SyncManager.TryDelaySync(1000);
    }

    private async void OnHideCompleteClick()
    {
      LocalSettings.Settings.HideComplete = !LocalSettings.Settings.HideComplete;
      LocalSettings.Settings.Save();
      SettingsHelper.PushLocalSettings();
      UserActCollectUtils.AddClickEvent("tasklist", "om", "show_hide_completed");
    }

    private void OnListActivityClick()
    {
      if (!ProChecker.CheckPro(ProType.ListActivities) || !(this.ViewModel?.Identity is NormalProjectIdentity identity))
        return;
      ProjectActivityWindow projectActivityWindow = new ProjectActivityWindow(identity.Id);
      projectActivityWindow.Owner = Window.GetWindow((DependencyObject) this);
      projectActivityWindow.Show();
      UserActCollectUtils.AddClickEvent("tasklist", "om", "project_activities");
    }

    private async void OnShowDetailClick()
    {
      LocalSettings.Settings.ShowDetails = !LocalSettings.Settings.ShowDetails;
      LocalSettings.Settings.Save();
      UserActCollectUtils.AddClickEvent("tasklist", "om", "show_hide_details");
    }

    private void OnShareClick(object sender, RoutedEventArgs e)
    {
      UserActCollectUtils.AddClickEvent("tasklist", "om", "collaboration");
      this.ShareProject();
    }

    private async void ShareProject()
    {
      KanbanContainer kanbanContainer = this;
      ShareProjectDialog.TryShowShareDialog(kanbanContainer.ViewModel.Identity.Id, Window.GetWindow((DependencyObject) kanbanContainer));
    }

    private async void NavigateTask(string taskId)
    {
      await Task.Delay(200);
      List<ColumnViewModel> list = this._columnViewModels.ToList();
      for (int index = 0; index < list.Count; ++index)
      {
        ColumnViewModel columnModel = list[index];
        ColumnViewModel columnViewModel = columnModel;
        TaskBaseViewModel taskBaseViewModel;
        if (columnViewModel == null)
        {
          taskBaseViewModel = (TaskBaseViewModel) null;
        }
        else
        {
          List<TaskBaseViewModel> sourceItems = columnViewModel.SourceItems;
          taskBaseViewModel = sourceItems != null ? sourceItems.FirstOrDefault<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m => m.Id == taskId)) : (TaskBaseViewModel) null;
        }
        if (taskBaseViewModel != null)
        {
          int offset = index * 282;
          double horizontalOffset = this.KanbanScroller.HorizontalOffset;
          if (horizontalOffset + this.KanbanScroller.ActualWidth < (double) (offset + 282))
            this.KanbanScroller.ScrollToHorizontalOffset((double) (offset + 282) - this.KanbanScroller.ActualWidth + 50.0);
          else if (horizontalOffset > (double) offset)
            this.KanbanScroller.ScrollToHorizontalOffset((double) offset);
          await Task.Delay(150);
          KanbanColumnView columnControlById = this.GetColumnControlById(columnModel.ColumnId);
          if (columnControlById == null)
            return;
          columnControlById.TryShowTaskDetail(taskId);
          return;
        }
        columnModel = (ColumnViewModel) null;
      }
      TaskDetailWindows.ShowTaskWindows(taskId);
    }

    private async void MenuGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      KanbanContainer child = this;
      e.Handled = true;
      await Task.Delay(1);
      Utils.FindParent<ListViewContainer>((DependencyObject) child)?.ShowProjectMenu();
    }

    private void HideTemplateGuide(object sender, MouseButtonEventArgs e)
    {
      this.HideTemplateGuide();
    }

    private void HideTemplateGuide() => this.TemplateGuidePopup.Visibility = Visibility.Collapsed;

    public void OnScroll(int offset)
    {
      this.KanbanScroller.ScrollToHorizontalOffset(this.KanbanScroller.HorizontalOffset + (double) offset);
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        if (!(this._startPoint != new System.Windows.Point()) || this._startPoint.Y <= 25.0)
          return;
        System.Windows.Point position = e.GetPosition((IInputElement) this.KanbanScroller);
        if (Math.Abs(position.X - this._startPoint.X) > 0.0)
          this.KanbanScroller.ScrollToHorizontalOffset(this.KanbanScroller.HorizontalOffset - position.X + this._startPoint.X);
        this._startPoint = position;
      }
      else
        this._startPoint = new System.Windows.Point();
    }

    private void CountDownOrTaskDateSwitch()
    {
      LocalSettings.Settings.ShowCountDown = !LocalSettings.Settings.ShowCountDown;
      UserActCollectUtils.AddClickEvent("tasklist", "om", "show_hide_countdown");
    }

    private void OnContainerMouseDown(object sender, MouseButtonEventArgs e)
    {
      this.ClearSelected();
    }

    public void SetFoldMenuIcon(bool hideMenu)
    {
      this.FoldImage.SetResourceReference(Image.SourceProperty, hideMenu ? (object) "ShowMenuDrawingImage" : (object) "HideMenuDrawingImage");
    }

    public void HideFoldMenuIcon()
    {
      this.MenuPathGrid.Visibility = Visibility.Collapsed;
      this.TitleGrid.Margin = new Thickness(20.0, 27.0, 0.0, 0.0);
    }

    public async Task BatchPinTask()
    {
      List<string> selectedTaskIds = this.GetSelectedTaskIds();
      List<TaskBaseViewModel> tasksByIds = TaskCache.GetTasksByIds(selectedTaskIds);
      await TaskService.BatchStarTaskOrNote(selectedTaskIds, this.ViewModel.Identity.CatId, tasksByIds.Count <= 0 || !tasksByIds.All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.IsPinned)));
    }

    public void BatchOpenSticky() => TaskStickyWindow.ShowTaskSticky(this.GetSelectedTaskIds());

    public async Task TryBatchSetDate(DateTime? date)
    {
      int num = await TaskService.BatchSetDate(this.GetSelectedTaskIds(), date) ? 1 : 0;
    }

    public async Task TryBatchSetPriority(int priority)
    {
      int num = await TaskService.BatchSetPriority(this.GetSelectedTaskIds(), priority) ? 1 : 0;
    }

    private void OnKanbanScrollerSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.ColumnContainer.SetScrollOffset(this.KanbanScroller.HorizontalOffset, this.ActualWidth);
      this.ColumnContainer.SetItemMaxHeight();
      this.TitleEditor.SetMaxWidth(this.ActualWidth - 150.0);
    }

    private void OnStartDrag(object sender, MouseButtonEventArgs e)
    {
      if (this.AddingModel != null || this.ColumnMouseOver() || this.AddColumnControl.IsMouseOver)
        return;
      this.ClearSelected();
      this._startPoint = e.GetPosition((IInputElement) this.KanbanScroller);
      Mouse.OverrideCursor = Cursors.Hand;
      this.KanbanGrid.CaptureMouse();
    }

    private void OnStopDrag(object sender, MouseButtonEventArgs e)
    {
      if (!(this._startPoint != new System.Windows.Point()))
        return;
      this._startPoint = new System.Windows.Point();
      Mouse.OverrideCursor = (Cursor) null;
      this.KanbanGrid.ReleaseMouseCapture();
    }

    public bool ColumnDragging => this.ColumnContainer.IsDragging;

    public bool HasNewAddColumnEditing
    {
      get => this._columnViewModels.Exists((Predicate<ColumnViewModel>) (model => model.Editing));
    }

    public void InsertColumn(int index, ColumnViewModel model)
    {
      this.ColumnContainer.Insert(index, model);
    }

    public void DeleteColumn(KanbanColumnView column)
    {
      this.ColumnContainer.RemoveItem(column.DataContext as ColumnViewModel);
      this.ShowAddColumn();
    }

    public async Task SetupColumn(IEnumerable<ColumnViewModel> data)
    {
      this._columnViewModels.Clear();
      foreach (ColumnViewModel model in data.Select<ColumnViewModel, ColumnViewModel>((Func<ColumnViewModel, ColumnViewModel>) (model => model.Clone())))
      {
        model.TaskCount = model.SourceItems.Count<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (s => s.Status == 0));
        this._columnViewModels.Add(model);
      }
      this.ColumnContainer.SetItemModels(this._columnViewModels);
      if (this._columnViewModels.Count == 0)
      {
        this.ShowEmptyImage();
      }
      else
      {
        this.KanbanScroller.Visibility = Visibility.Visible;
        this.EmptyGrid.Visibility = Visibility.Collapsed;
      }
    }

    private async Task ShowEmptyImage()
    {
      this.KanbanScroller.Visibility = Visibility.Collapsed;
      this.EmptyGrid.Visibility = Visibility.Visible;
      SortProjectData projectData = await ProjectTaskDataProvider.GetProjectData(this.ViewModel.Identity);
      this.EmptyImage.Image.Source = (ImageSource) projectData.GetEmptyImage();
      this.EmptyImage.Path.Data = projectData.GetEmptyPath();
      this.EmptyImage.Path.Margin = projectData.GetEmptyMargin();
      TextBlock textBlock = this.EmptyText;
      textBlock.Text = await projectData.GetEmptyTitle();
      textBlock = (TextBlock) null;
      this.EmptyAddText.Text = Utils.GetString(this.ViewModel.Identity.IsNote ? "AddaNote" : "AddaTask");
    }

    public KanbanColumnView GetColumnControlById(string columnId)
    {
      return this.ColumnContainer.GetColumnById(columnId);
    }

    public bool ColumnMouseOver()
    {
      foreach (KanbanColumnView child in this.ColumnContainer.Children)
      {
        if (child.IsMouseOver || child.IsPopupOpen())
          return true;
      }
      return false;
    }

    private async void OnAddColumnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.ViewModel.Identity is NormalProjectIdentity identity)
      {
        if (this.CloseAddButton.IsMouseOver)
        {
          ProjectModel projectById = CacheManager.GetProjectById(identity.Id);
          if (projectById != null)
          {
            projectById.ShowAddColumn = false;
            int num = await ProjectDao.TryUpdateProject(projectById);
          }
          this.ViewModel.ShowAdd = false;
          Utils.Toast(Utils.GetString("KanbanClosedAddColumnTips"));
          return;
        }
        if (this.AddColumnControl.IsMouseOver)
          this.OnAddColumn();
      }
      e.Handled = true;
    }

    private void OnSwitchView(object sender, string e)
    {
      Mouse.Capture((IInputElement) null);
      this.MorePopup.IsOpen = false;
      if (!(e == "list") && !(e == "timeline"))
        return;
      UserActCollectUtils.AddClickEvent("list_mode", "switch", e);
      if (e == "timeline" && !ProChecker.CheckPro(ProType.TimeLine, (Window) App.Window))
        return;
      this.ViewModel.Identity.SwitchViewMode(e);
    }

    public void ReloadDataAndColumnQuickAddView()
    {
      this.ColumnContainer.ReloadQuickAddView();
      this.Reload(true, true);
    }

    public void TabSelect()
    {
    }

    public void BatchDeleteTask()
    {
      List<string> selectedTaskIds = this._batchHelper.SelectedTaskIds;
      // ISSUE: explicit non-virtual call
      if ((selectedTaskIds != null ? (__nonvirtual (selectedTaskIds.Count) > 1 ? 1 : 0) : 0) == 0)
        return;
      TaskService.BatchDeleteTaskByIds(this._batchHelper.SelectedTaskIds);
    }

    private void OnAddTaskClick(object sender, MouseButtonEventArgs e)
    {
      Utils.FindParent<ListViewContainer>((DependencyObject) this)?.ShowAddTask();
    }

    public void Clear()
    {
      this.ViewModel.SourceModels = new List<TaskBaseViewModel>();
      this.ViewModel.Name = string.Empty;
      this._columnViewModels.Do((Action) (() => this._columnViewModels.Value.ForEach((Action<ColumnViewModel>) (v => v.Clear()))));
    }

    public void Dispose()
    {
      this.TitleEditor.SetText(string.Empty);
      this.ViewModel.SourceModels = new List<TaskBaseViewModel>();
      this._columnViewModels.Do((Action) (() => this._columnViewModels.Value.ForEach((Action<ColumnViewModel>) (v => v.Dispose()))));
      this.ViewModel.Dispose();
      this.ColumnContainer.Dispose();
    }

    public void StartRemoveAddModel() => this.AddingModel?.SetInvalid();

    public void RemoveAddingTaskModel() => this.AddingModel = (SectionAddTaskViewModel) null;

    public SectionAddTaskViewModel GetAddingTaskModel(ProjectIdentity identity, Section section)
    {
      if (identity is ColumnProjectIdentity columnProjectIdentity && section != null)
        this.AddingModel = new SectionAddTaskViewModel()
        {
          ColumnId = columnProjectIdentity.ColumnId,
          SectionId = section.SectionId
        };
      return this.AddingModel;
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      this.ColumnContainer.SetScrollOffset(this.KanbanScroller.HorizontalOffset, this.ActualWidth);
    }

    public void ClearEvent()
    {
      this.BatchTaskDrop = (EventHandler<List<string>>) null;
      this.TaskRemoved = (EventHandler<DisplayItemModel>) null;
      this.TaskAdded = (EventHandler<DisplayItemModel>) null;
      this.SetSelected = (EventHandler<List<string>>) null;
    }

    private void OnMenuPathMouseEnter(object sender, MouseEventArgs e)
    {
      Utils.FindParent<ListViewContainer>((DependencyObject) this)?.TryShowMenuOnHover((UIElement) this.MenuPathGrid);
    }

    public void ReloadIdentity()
    {
      this.ViewModel.SetIdentity(this.ViewModel.Identity.Copy(this.ViewModel.Identity));
      this.Reload(true, true);
    }

    public async void ExpandOrFoldAllTask()
    {
      bool allOpen = true;
      List<TaskListViewModel> taskListVms = new List<TaskListViewModel>();
      foreach (ColumnViewModel columnViewModel in this._columnViewModels.Value)
      {
        TaskListViewModel tM = new TaskListViewModel((ProjectIdentity) columnViewModel.Identity)
        {
          ListDisplayType = TaskListDisplayType.Kanban
        };
        tM.SourceModels = columnViewModel.SourceItems;
        TaskListViewModel taskListViewModel = tM;
        taskListViewModel.Items = new ObservableCollection<DisplayItemModel>(await tM.BuildItems(true, columnViewModel.SourceItems));
        taskListViewModel = (TaskListViewModel) null;
        taskListVms.Add(tM);
        allOpen = allOpen && tM.IsAllTaskOpen();
        tM = (TaskListViewModel) null;
      }
      foreach (TaskListViewModel taskListViewModel in taskListVms)
        await taskListViewModel.OpenOrCloseAllTasksInKanban(!allOpen);
      this.Reload(true);
      taskListVms = (List<TaskListViewModel>) null;
    }

    public async void ExpandOrFoldAllSection()
    {
      bool allOpen = true;
      List<TaskListViewModel> taskListVms = new List<TaskListViewModel>();
      foreach (ColumnViewModel columnViewModel in this._columnViewModels.Value)
      {
        TaskListViewModel tM = new TaskListViewModel((ProjectIdentity) columnViewModel.Identity)
        {
          ListDisplayType = TaskListDisplayType.Kanban
        };
        tM.SourceModels = columnViewModel.SourceItems;
        TaskListViewModel taskListViewModel = tM;
        taskListViewModel.Items = new ObservableCollection<DisplayItemModel>(await tM.BuildItems(true, columnViewModel.SourceItems));
        taskListViewModel = (TaskListViewModel) null;
        taskListVms.Add(tM);
        allOpen = allOpen && tM.IsAllSectionOpen();
        tM = (TaskListViewModel) null;
      }
      foreach (TaskListViewModel taskListViewModel in taskListVms)
        await taskListViewModel.OpenOrCloseAllSections(taskListViewModel.Items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.IsSection)).ToList<DisplayItemModel>(), !allOpen);
      this.Reload(true);
      taskListVms = (List<TaskListViewModel>) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/kanban/kanbancontainer.xaml", UriKind.Relative));
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
          this.Root = (KanbanContainer) target;
          this.Root.PreviewMouseMove += new MouseEventHandler(this.OnContainerMouseMove);
          this.Root.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnContainerMouseUp);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnContainerMouseDown);
          break;
        case 3:
          this.DragTaskPopup = (Popup) target;
          break;
        case 4:
          this.PopupTaskItem = (KanbanItemPopupView) target;
          break;
        case 5:
          this.TemplateGuidePopup = (Grid) target;
          this.TemplateGuidePopup.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.HideTemplateGuide);
          break;
        case 6:
          this.MenuPathGrid = (Border) target;
          this.MenuPathGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.MenuGrid_MouseLeftButtonUp);
          this.MenuPathGrid.MouseEnter += new MouseEventHandler(this.OnMenuPathMouseEnter);
          break;
        case 7:
          this.FoldImage = (Image) target;
          break;
        case 8:
          this.OptionPanel = (StackPanel) target;
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectOrderClick);
          break;
        case 10:
          this.SortIcon = (Image) target;
          break;
        case 11:
          this.ChooseSortTypePopup = (EscPopup) target;
          break;
        case 12:
          this.MoreGrid = (Grid) target;
          this.MoreGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoreGridClick);
          break;
        case 13:
          this.MorePopup = (EscPopup) target;
          break;
        case 14:
          this.TitleGrid = (Grid) target;
          break;
        case 15:
          this.TitleEditor = (EmojiTitleEditor) target;
          break;
        case 16:
          this.ShareGrid = (Grid) target;
          this.ShareGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnShareClick);
          break;
        case 17:
          this.ShareImage = (Image) target;
          break;
        case 18:
          this.KanbanScroller = (ScrollViewer) target;
          this.KanbanScroller.PreviewMouseWheel += new MouseWheelEventHandler(this.OnMouseWheel);
          this.KanbanScroller.SizeChanged += new SizeChangedEventHandler(this.OnKanbanScrollerSizeChanged);
          this.KanbanScroller.ScrollChanged += new ScrollChangedEventHandler(this.OnScrollChanged);
          break;
        case 19:
          this.KanbanGrid = (Grid) target;
          this.KanbanGrid.MouseMove += new MouseEventHandler(this.OnDragMove);
          this.KanbanGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnStartDrag);
          this.KanbanGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnStopDrag);
          break;
        case 20:
          this.ColumnContainer = (KanbanColumnCanvas) target;
          break;
        case 21:
          this.AddColumnControlGrid = (Grid) target;
          this.AddColumnControlGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddColumnMouseUp);
          break;
        case 22:
          this.AddColumnControl = (Border) target;
          break;
        case 23:
          this.CloseAddButton = (Border) target;
          break;
        case 24:
          this.EmptyGrid = (StackPanel) target;
          break;
        case 25:
          this.EmptyImage = (EmptyImage) target;
          break;
        case 26:
          this.EmptyText = (TextBlock) target;
          break;
        case 27:
          this.EmptyAddText = (TextBlock) target;
          this.EmptyAddText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddTaskClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
