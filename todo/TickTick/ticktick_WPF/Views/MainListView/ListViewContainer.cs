// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ListViewContainer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Event;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.ProjectList;
using ticktick_WPF.Views.Search;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MainListView
{
  public class ListViewContainer : Grid, INavigateProject
  {
    public ListMode Mode;
    private readonly ContentControl _projectContent;
    private TaskView _taskView;
    private Border _parentBorder;
    private static int ThreeFrameWidth = 912;
    private const int TwoFrameWidth = 582;
    private const int ProjectMenuMiniWidth = 230;
    private ColumnDefinition _firstColumn;
    private ColumnDefinition _secondColumn;
    private MainProjectStatus _menuStatus;
    private GridSplitter _leftSplit;
    private bool _needHideDetail;
    private ProjectListView _projectList;
    private static ConcurrentDictionary<string, ListViewContainer> _listViews = new ConcurrentDictionary<string, ListViewContainer>();

    public ListViewContainer(Border parentBorder)
    {
      ContentControl contentControl = new ContentControl();
      contentControl.RenderTransform = (Transform) new TranslateTransform();
      this._projectContent = contentControl;
      this._taskView = new TaskView();
      this._menuStatus = MainProjectStatus.Show;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this._parentBorder = parentBorder;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
      this.SizeChanged += new SizeChangedEventHandler(this.OnListViewSizeChanged);
      this.InitGridColumns();
      this._projectList = new ProjectListView();
      this.ClipToBounds = true;
      this._projectContent.SetValue(Grid.ColumnProperty, (object) 0);
      this.Children.Add((UIElement) this._projectContent);
      this.HideDetail();
      this._projectList.ProjectSelected += new EventHandler(this.OnProjectSelected);
      this._projectContent.Content = (object) this._projectList;
      this._taskView.KanbanBatchTaskDrop += new EventHandler<List<string>>(this.OnKanbanBatchTaskDropped);
      this._taskView.BatchTaskDrop += new EventHandler<List<string>>(this.OnBatchTaskDropped);
      this._taskView.TaskDragging += new EventHandler<DragMouseEvent>(this.OnTaskDragging);
      this._taskView.SetValue(Grid.ColumnProperty, (object) 1);
      this.Children.Add((UIElement) this._taskView);
    }

    public ProjectIdentity ProjectIdentity => this._projectList?.GetSelectedProject();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      CalendarEventChangeNotifier.Deleted += new EventHandler<string>(this.OnEventDeleted);
      DataChangedNotifier.CalendarChanged += new EventHandler(this.OnCalendarChanged);
      DataChangedNotifier.ViewModeChanged += new EventHandler<ProjectIdentity>(this.OnProjectViewModeChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnThemeChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnSmartProjectsChanged), "SmartProjects");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnProjectNumChanged), "ProjectNum");
      this.InitColumnSize();
    }

    public void Clear()
    {
      try
      {
        if (this._parentBorder != null)
        {
          if (object.Equals((object) this._parentBorder.Child, (object) this))
            this._parentBorder.Child = (UIElement) null;
          this._parentBorder = (Border) null;
        }
        if (this._projectList != null)
        {
          this._projectList.ProjectSelected -= new EventHandler(this.OnProjectSelected);
          this._projectList.Children.Clear();
          this._projectList = (ProjectListView) null;
        }
        if (this._taskView != null)
        {
          this._taskView.KanbanBatchTaskDrop -= new EventHandler<List<string>>(this.OnKanbanBatchTaskDropped);
          this._taskView.BatchTaskDrop -= new EventHandler<List<string>>(this.OnBatchTaskDropped);
          this._taskView.TaskDragging -= new EventHandler<DragMouseEvent>(this.OnTaskDragging);
          this._taskView?.Clear();
          this._taskView = (TaskView) null;
        }
        this.Children.Clear();
      }
      catch (Exception ex)
      {
        UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
      }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      DataChangedNotifier.CalendarChanged -= new EventHandler(this.OnCalendarChanged);
      CalendarEventChangeNotifier.Deleted -= new EventHandler<string>(this.OnEventDeleted);
      DataChangedNotifier.ViewModeChanged -= new EventHandler<ProjectIdentity>(this.OnProjectViewModeChanged);
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnThemeChanged);
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnSmartProjectsChanged), "SmartProjects");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnProjectNumChanged), "ProjectNum");
    }

    private void OnThemeChanged(object sender, EventArgs e) => this._projectList?.LoadData(false);

    private void OnProjectNumChanged(object sender, PropertyChangedEventArgs e)
    {
      TaskCountCache.Clear();
      this._projectList?.LoadData();
    }

    private void OnSmartProjectsChanged(object sender, PropertyChangedEventArgs e)
    {
      this._projectList?.LoadData();
    }

    private void OnProjectSelected(object sender, EventArgs e)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this.Mode = ListMode.Normal;
        ThemeUtil.TryClearImageCached(true);
        this.HideProjectMenu();
        ProjectIdentity selectedProject = this._projectList.GetSelectedProject();
        if (selectedProject == null)
          return;
        this.OnProjectSelect(selectedProject);
      }));
    }

    private void OnProjectSelect(ProjectIdentity identity, string taskId = null)
    {
      this._taskView?.OnProjectSelect(identity, taskId, true);
      this._taskView?.SetFoldMenuIcon(!this.MenuShow || this.MenuAutoHide);
    }

    private void OnEventDeleted(object sender, string eventId) => this.HideDetail();

    private void OnCalendarChanged(object sender, EventArgs e) => this._projectList?.LoadData();

    public bool IsProjectMenuVisible() => this.MenuShow && !this.MenuAutoHide;

    public IToastShowWindow GetToastWindow()
    {
      return Utils.FindParent<IToastShowWindow>((DependencyObject) this);
    }

    public void TryToast(string getString)
    {
      this.GetToastWindow()?.TryToastString((object) null, getString);
    }

    public void SelectProject(ProjectIdentity identity)
    {
      this._projectList?.SelectProject(identity);
    }

    public void LoadSavedProject() => this._projectList?.LoadSavedProject();

    public ProjectIdentity GetSelectedProject()
    {
      if (this.Mode == ListMode.Search)
        return (ProjectIdentity) new SearchProjectIdentity();
      return this._projectList?.SelectedItem?.GetIdentity();
    }

    public async void NavigateTask(TaskModel task)
    {
      if (task.deleted == 1)
      {
        this._projectList?.SelectTrash();
      }
      else
      {
        ProjectModel projectById = CacheManager.GetProjectById(task.projectId);
        if (projectById == null)
          return;
        this._projectList?.SelectProject((ProjectIdentity) new NormalProjectIdentity(projectById), false);
        this.OnProjectSelect((ProjectIdentity) new NormalProjectIdentity(projectById), task.id);
      }
    }

    public void ReloadView(bool hideDetail = false)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this._projectList?.LoadData();
        this._taskView?.ReloadView(hideDetail);
      }));
    }

    public void TryExtractDetail() => this._taskView?.TryExtractDetail();

    public void HideDetail() => this._taskView?.HideDetail();

    public async void StartSearch(SearchExtra searchExtra, bool restore)
    {
      this.SetMode(ListMode.Search);
      if (!restore)
      {
        SearchHelper.SearchFilter = new SearchFilterViewModel();
        this._taskView?.ResetSearchFilterControl();
      }
      SearchHelper.SearchKey = searchExtra.SearchKey;
      SearchHelper.Tags = searchExtra.Tags;
      SearchHelper.SearchFilter.SearchKey = searchExtra.SearchKey;
      SearchHelper.SearchFilter.SearchTags = searchExtra.Tags;
      SearchHelper.SearchFilter.Searched = false;
      if (this.MenuShow && this.ColumnStyle != ListViewColumnStyle.OneColumn)
      {
        this._firstColumn.MaxWidth = 0.0;
        this._firstColumn.MinWidth = 0.0;
      }
      if (this._taskView == null)
        return;
      await this._taskView.SwitchListAndLoad((ProjectIdentity) new SearchProjectIdentity(), string.IsNullOrEmpty(searchExtra.SearchId));
      await Task.Delay(100);
      this._taskView.SelectId(searchExtra.SearchId);
    }

    public async void StopSearch()
    {
      this.SetMode(ListMode.Normal);
      this.HideDetail();
      if (!this.MenuShow || this.MenuAutoHide)
        return;
      this._firstColumn.MinWidth = 212.0;
      this._firstColumn.MaxWidth = this.GetFirstColumnMaxWidth();
    }

    public async void OnWindowKeyUp(KeyEventArgs e) => this._taskView?.OnKeyUp(e);

    public void OnEsc()
    {
      if (LocalSettings.Settings.InSearch || this.ColumnStyle != ListViewColumnStyle.OneColumn)
        return;
      if (this.IsProjectMenuVisible())
      {
        this.HideProjectMenu();
      }
      else
      {
        TaskView taskView = this._taskView;
        if ((taskView != null ? (taskView.OnEsc() ? 1 : 0) : 0) != 0)
          return;
        this.ShowProjectMenu(false);
      }
    }

    public void TryPrint(bool isDetail) => this._taskView?.TryPrint(isDetail);

    public void TryBatchSetPriority(int priority) => this._taskView?.TryBatchSetPriority(priority);

    public void BatchOpenSticky() => this._taskView?.BatchOpenSticky();

    public void TryBatchSetDate(DateTime? date) => this._taskView?.TryBatchSetDate(date);

    public void BatchPinTask() => this._taskView?.BatchPinTask();

    public void BatchDeleteTask() => this._taskView?.BatchDeleteTask();

    public bool TabListAndDetail(IInputElement focusItem)
    {
      TaskView taskView = this._taskView;
      return taskView != null && taskView.TabListAndDetail(focusItem);
    }

    public async Task ExpandOrFoldAllTask(bool isOpen)
    {
      this._taskView?.ExpandOrFoldAllTask(isOpen);
    }

    public async Task ExpandOrFoldAllSection() => this._taskView?.ExpandOrFoldAllSection();

    public void TabSelectQuickAddItem() => this._taskView?.TabSelectItem();

    public void TabKanbanSelect() => this._taskView?.TabKanbanSelect();

    public void OnScroll(int offset) => this._taskView?.OnTouchScroll(offset);

    public async Task TryToastMoveControl(TaskModel task, bool isToday, bool hideTitle = false)
    {
      ListViewContainer navigate = this;
      if (task == null)
        return;
      if (!string.IsNullOrEmpty(task.parentId))
      {
        TaskView taskView = navigate._taskView;
        if ((taskView != null ? (taskView.CheckTaskInList(task.parentId) ? 1 : 0) : 0) != 0)
          return;
      }
      if (MoveToastHelper.CheckTaskMatched(navigate.GetSelectedProject(), task))
        return;
      navigate.GetToastWindow()?.Toast((FrameworkElement) new MoveToastControl(isToday, (INavigateProject) navigate, hideTitle ? string.Empty : task.title));
    }

    public void NavigateProjectById(string projectId)
    {
      this.SelectProject(ProjectIdHelper.GetProjectIdentity(projectId));
    }

    public void NavigateTodayProject()
    {
      this._projectList?.TrySelectSmartProject(SmartListType.Today);
    }

    public void NavigateTomorrowProject()
    {
      this._projectList?.TrySelectSmartProject(SmartListType.Tomorrow);
    }

    public void ShowAddTask()
    {
      Utils.FindParent<MainWindow>((DependencyObject) this)?.InputCommand();
    }

    public void SetMode(ListMode mode) => this.Mode = mode;

    public void ReloadTaskListAndSelect(bool forceReload = false, string taskId = "", bool forceFocus = true)
    {
      this._taskView?.ReloadTaskListAndSelect(forceReload, taskId, forceFocus);
    }

    public void ReSearch() => this._taskView?.ReSearch();

    public void HideDetailOnTasksDelete(List<string> taskIds)
    {
      this._taskView?.HideDetailOnTasksDelete(taskIds);
    }

    public TaskView GetTaskView() => this._taskView;

    private bool MenuAutoHide
    {
      get => (this._menuStatus & MainProjectStatus.AutoFold) == MainProjectStatus.AutoFold;
    }

    private bool MenuShow => (this._menuStatus & MainProjectStatus.Show) == MainProjectStatus.Show;

    public ListViewColumnStyle ColumnStyle { get; set; }

    private void InitGridColumns()
    {
      this._firstColumn = new ColumnDefinition()
      {
        MinWidth = 230.0,
        MaxWidth = 305.0,
        Width = new GridLength(230.0)
      };
      this._secondColumn = new ColumnDefinition();
      this.ColumnDefinitions.Add(this._firstColumn);
      this.ColumnDefinitions.Add(this._secondColumn);
      GridSplitter gridSplitter = new GridSplitter();
      gridSplitter.Width = 3.0;
      gridSplitter.IsTabStop = false;
      gridSplitter.Background = (Brush) Brushes.Transparent;
      gridSplitter.FocusVisualStyle = (Style) null;
      gridSplitter.HorizontalAlignment = HorizontalAlignment.Left;
      gridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
      this._leftSplit = gridSplitter;
      this._leftSplit.SetValue(Grid.ColumnProperty, (object) 1);
      this._leftSplit.SetValue(Panel.ZIndexProperty, (object) 1000);
      this.Children.Add((UIElement) this._leftSplit);
      this._leftSplit.DragCompleted += new DragCompletedEventHandler(this.OnSplitDragCompleted);
    }

    private void InitColumnSize()
    {
      double firstColumnMaxWidth = this.GetFirstColumnMaxWidth();
      IListViewParent parent = Utils.FindParent<IListViewParent>((DependencyObject) this);
      double num1 = parent != null ? parent.GetProjectWidth() : 0.0;
      double num2 = num1 < 230.0 ? 230.0 : (num1 > firstColumnMaxWidth ? firstColumnMaxWidth : num1);
      this._firstColumn.Width = new GridLength(num2 > 0.0 ? num2 : 230.0);
      this._secondColumn.Width = new GridLength(1.0, GridUnitType.Star);
      this.SetListViewSize();
      if (LocalSettings.Settings.ExtraSettings.MenuFold == !this.MenuShow)
        return;
      if (this.ColumnStyle != ListViewColumnStyle.OneColumn)
      {
        this.ShowProjectMenu(false);
      }
      else
      {
        this._menuStatus |= MainProjectStatus.Fold;
        this._menuStatus &= ~MainProjectStatus.Show;
        this._taskView?.SetFoldMenuIcon(!this.MenuShow || this.MenuAutoHide);
      }
    }

    private void OnListViewSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size size = e.NewSize;
      double width1 = size.Width;
      size = e.PreviousSize;
      double width2 = size.Width;
      if (Math.Abs(width1 - width2) <= 3.0)
        return;
      this.SetListViewSize();
    }

    private void SetListViewSize()
    {
      Window window = Window.GetWindow((DependencyObject) this);
      double width = window != null ? window.ActualWidth : 0.0;
      if (width <= 0.0)
        width = 1140.0;
      this._taskView?.ResetColumnWidth(width - this._firstColumn.ActualWidth);
      this.SetColumns(width);
    }

    private void SetColumns(double width)
    {
      if (width < 582.0)
        this.SetOneColumn();
      else if (width < (double) ListViewContainer.ThreeFrameWidth)
        this.SetTwoColumns();
      else
        this.SetThreeColumns();
    }

    private void FoldThirdColumn() => this._taskView?.FoldDetail();

    private void ExpandThirdColumn() => this._taskView?.ExpandDetail();

    private void SetOneColumn()
    {
      if (this.ColumnStyle == ListViewColumnStyle.OneColumn)
        return;
      this.FoldThirdColumn();
      this._taskView?.SetListColumnSpan(2, 0.0, 0.0);
      this.FoldMenu(true);
      this.SetProjectMenuStyle(true);
      this._leftSplit.Visibility = Visibility.Collapsed;
      this._projectList?.SetProjectFoldBackground(true);
      this.ColumnStyle = ListViewColumnStyle.OneColumn;
    }

    private void SetTwoColumns()
    {
      if (this.ColumnStyle == ListViewColumnStyle.TwoColumn)
        return;
      this.FoldThirdColumn();
      this.SetProjectMenuStyle(false);
      this._projectList?.SetProjectFoldBackground(false);
      this._leftSplit.Visibility = Visibility.Collapsed;
      this.ColumnStyle = ListViewColumnStyle.TwoColumn;
      if (!this.MenuShow || this.Mode != ListMode.Normal)
        return;
      this._firstColumn.BeginAnimation(ColumnDefinition.MinWidthProperty, (AnimationTimeline) null);
      this._firstColumn.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) null);
      this._firstColumn.MinWidth = 230.0;
      this._firstColumn.MaxWidth = 230.0;
    }

    private void SetThreeColumns()
    {
      if (this.MenuShow && this.Mode == ListMode.Normal)
      {
        double firstColumnMaxWidth = this.GetFirstColumnMaxWidth();
        if (Math.Abs(this._firstColumn.MaxWidth - firstColumnMaxWidth) > 4.0)
        {
          this._firstColumn.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) null);
          this._firstColumn.MaxWidth = firstColumnMaxWidth;
        }
      }
      if (this.ColumnStyle == ListViewColumnStyle.ThreeColumn)
        return;
      this.ExpandThirdColumn();
      this._taskView?.SetListColumnSpan(1, 340.0, 328.0);
      this.SetProjectMenuStyle(false);
      this._projectList?.SetProjectFoldBackground(false);
      this.ColumnStyle = ListViewColumnStyle.ThreeColumn;
      this._leftSplit.Visibility = Visibility.Visible;
      if (!this.MenuShow || this.Mode != ListMode.Normal)
        return;
      this._firstColumn.MinWidth = 230.0;
      this._firstColumn.MaxWidth = this.GetFirstColumnMaxWidth();
    }

    private double GetFirstColumnMaxWidth()
    {
      return Math.Max(Math.Min(this.ActualWidth - 750.0, (double) ((int) this.ActualWidth / 4)), 230.0);
    }

    private async void FoldMenu(bool auto)
    {
      if (auto)
      {
        this._menuStatus |= MainProjectStatus.AutoFold;
        this._menuStatus &= ~MainProjectStatus.AutoShow;
      }
      else
      {
        this._menuStatus |= MainProjectStatus.Fold;
        this._menuStatus &= ~MainProjectStatus.Show;
      }
      if (auto && this.ColumnStyle == ListViewColumnStyle.Init)
      {
        this._firstColumn.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) null);
        this._firstColumn.MaxWidth = 0.0;
        this._firstColumn.MinWidth = 0.0;
        this._projectContent.Visibility = Visibility.Collapsed;
        this._projectContent.RenderTransform = (Transform) new TranslateTransform()
        {
          X = -230.0
        };
      }
      else
        this.BeginProjectContentAnim(true);
      this._taskView?.SetFoldMenuIcon(!this.MenuShow || this.MenuAutoHide);
    }

    private async void ShowMenu()
    {
      this._menuStatus &= ~MainProjectStatus.Fold;
      this._menuStatus |= MainProjectStatus.Show;
      this.BeginProjectContentAnim(false);
      this.SetProjectMenuStyle(false);
      this._taskView?.SetFoldMenuIcon(!this.MenuShow || this.MenuAutoHide);
    }

    private void BeginProjectContentAnim(bool hide)
    {
      if (hide)
      {
        this._firstColumn.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) null);
        this._firstColumn.MaxWidth = this._firstColumn.ActualWidth;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(this._firstColumn.MaxWidth), 0.0, 240);
        this._firstColumn.MinWidth = 0.0;
        doubleAnimation.Completed += (EventHandler) ((o, args) =>
        {
          this._projectContent.Visibility = Visibility.Collapsed;
          this._projectContent.RenderTransform = (Transform) new TranslateTransform()
          {
            X = -230.0
          };
        });
        this._firstColumn.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) doubleAnimation);
      }
      else
      {
        this._projectContent.Visibility = Visibility.Visible;
        this._projectContent.RenderTransform = (Transform) new TranslateTransform()
        {
          X = 0.0
        };
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(this._firstColumn.MaxWidth), 500.0, 240);
        this._firstColumn.MinWidth = 0.0;
        doubleAnimation.Completed += (EventHandler) ((o, args) =>
        {
          this._firstColumn.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) null);
          this._firstColumn.MaxWidth = this.GetFirstColumnMaxWidth();
          this._firstColumn.MinWidth = 230.0;
        });
        this._firstColumn.BeginAnimation(ColumnDefinition.MaxWidthProperty, (AnimationTimeline) doubleAnimation);
      }
    }

    private void SetProjectMenuStyle(bool autoShow)
    {
      if (!autoShow)
      {
        if (!this.MenuShow || LocalSettings.Settings.MainWindowDisplayModule == 2 || LocalSettings.Settings.InSearch)
          return;
        this._menuStatus &= ~MainProjectStatus.AutoFold;
        this._menuStatus |= MainProjectStatus.AutoShow;
        this._projectContent.SetValue(Grid.ColumnSpanProperty, (object) 1);
        this._projectContent.Visibility = Visibility.Visible;
        TranslateTransform renderTransform = (TranslateTransform) this._projectContent.RenderTransform;
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
        renderTransform.X = 0.0;
        this._projectContent.MinWidth = 230.0;
        this._projectContent.MaxWidth = 10000.0;
        this._projectContent.HorizontalAlignment = HorizontalAlignment.Stretch;
        this.SetHideMenuIcon(false);
        if (!this.MenuShow && LocalSettings.Settings.MainWindowDisplayModule != 3 && this.Mode != ListMode.Search)
          this.ShowMenu();
        this._taskView?.SetFoldMenuIcon(!this.MenuShow || this.MenuAutoHide);
      }
      else
      {
        if (this.MenuAutoHide || LocalSettings.Settings.MainWindowDisplayModule == 2 || LocalSettings.Settings.InSearch)
          return;
        this.SetHideMenuIcon(true);
        this._projectContent.Visibility = Visibility.Visible;
        this._projectContent.SetValue(Grid.ColumnSpanProperty, (object) 2);
        this._projectContent.SetValue(Panel.ZIndexProperty, (object) 100);
        this._projectContent.MinWidth = 230.0;
        this._projectContent.MaxWidth = 230.0;
        this._projectContent.HorizontalAlignment = HorizontalAlignment.Left;
      }
    }

    private void SetHideMenuIcon(bool isHide) => this._taskView?.SetFoldMenuIcon(isHide);

    private void OnSplitDragCompleted(object sender, DragCompletedEventArgs e)
    {
      Utils.FindParent<IListViewParent>((DependencyObject) this)?.OnProjectWidthChanged(this._projectList.ActualWidth);
    }

    public async void HideProjectMenu()
    {
      ListViewContainer listViewContainer = this;
      if (listViewContainer._taskView != null && listViewContainer._taskView.IsMenuPathMouseOver() || listViewContainer.ActualWidth <= 0.0 || listViewContainer.ActualWidth >= 582.0 || listViewContainer.MenuAutoHide || listViewContainer.ColumnStyle != ListViewColumnStyle.OneColumn)
        return;
      // ISSUE: explicit non-virtual call
      listViewContainer.MouseMove -= new MouseEventHandler(listViewContainer.TryHideProject);
      listViewContainer.ShowOrHideProjectAnim(true);
      await Task.Delay(100);
      listViewContainer._menuStatus &= ~MainProjectStatus.AutoShow;
      listViewContainer._menuStatus |= MainProjectStatus.AutoFold;
      listViewContainer._taskView?.SetFoldMenuIcon(!listViewContainer.MenuShow || listViewContainer.MenuAutoHide);
    }

    private void ShowOrHideProjectAnim(bool hide)
    {
      if (hide)
      {
        if (!(this._projectContent?.RenderTransform is TranslateTransform renderTransform))
          return;
        this._projectContent.Visibility = Visibility.Visible;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), -230.0, 240);
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation);
      }
      else
      {
        if (!(this._projectContent?.RenderTransform is TranslateTransform renderTransform))
          return;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 240);
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation);
      }
    }

    public async void TryShowMenuOnHover(UIElement element)
    {
      ListViewContainer listViewContainer = this;
      await Task.Delay(150);
      if (!element.IsMouseOver || listViewContainer.ColumnStyle != ListViewColumnStyle.OneColumn || !listViewContainer.MenuAutoHide)
        return;
      listViewContainer.ShowProjectMenuStory();
      await Task.Delay(100);
      // ISSUE: explicit non-virtual call
      listViewContainer.MouseMove -= new MouseEventHandler(listViewContainer.TryHideProject);
      // ISSUE: explicit non-virtual call
      listViewContainer.MouseMove += new MouseEventHandler(listViewContainer.TryHideProject);
    }

    private void ShowProjectMenuStory()
    {
      this._menuStatus &= ~MainProjectStatus.AutoFold;
      this._menuStatus |= MainProjectStatus.AutoShow;
      this.SetProjectMenuStyle(true);
      this.ShowOrHideProjectAnim(false);
      this._taskView?.SetFoldMenuIcon(!this.MenuShow || this.MenuAutoHide);
    }

    private async void TryHideProject(object sender, MouseEventArgs e)
    {
      ListViewContainer listViewContainer = this;
      System.Windows.Point position = e.GetPosition((IInputElement) listViewContainer._projectContent);
      if (position.X > 0.0 && position.X < 230.0 && position.Y >= 0.0 && position.Y < listViewContainer.ActualHeight)
        return;
      Window window = Window.GetWindow((DependencyObject) listViewContainer);
      if (!listViewContainer.MenuAutoHide && window != null && window.IsActive)
      {
        await Task.Delay(150);
        if (!listViewContainer._projectContent.IsMouseOver && !listViewContainer.MenuAutoHide && window.IsActive)
          listViewContainer.HideProjectMenu();
      }
      window = (Window) null;
    }

    public void TryHideProjectMenu(double x)
    {
      if (x <= this._projectContent.ActualWidth)
        return;
      this.HideProjectMenu();
    }

    public void TryShowThreeColumns()
    {
      if (this.ActualWidth < (double) ListViewContainer.ThreeFrameWidth)
        return;
      this.SetThreeColumns();
    }

    public void ShowProjectMenu(bool manual = true)
    {
      if (this.ColumnStyle == ListViewColumnStyle.OneColumn)
        return;
      if (!this.MenuShow)
        this.ShowMenu();
      else
        this.FoldMenu(false);
      if (!manual)
        return;
      LocalSettings.Settings.ExtraSettings.MenuFold = !this.MenuShow;
    }

    public void OnWindowMouseDown(MouseButtonEventArgs e)
    {
      if (this.ColumnStyle == ListViewColumnStyle.OneColumn)
        this.TryHideProject((object) this, (MouseEventArgs) e);
      this._taskView?.TryFoldDetailOnMouseDown(e);
    }

    public ProjectListView ProjectList => this._projectList;

    public void OnSyncFinished(SyncResult syncResult)
    {
      this._projectList?.OnSyncFinished(syncResult);
    }

    public async Task<bool> OnDragTaskDroppedOnProject(string taskId)
    {
      return await this._projectList.OnDragTaskDropped(taskId);
    }

    public void SelectFilter(string filterId) => this._projectList?.SelectFilter(filterId);

    public async Task NavigateProject(string type, string id)
    {
      if (this._projectList == null)
        await Task.Delay(150);
      this._projectList?.NavigateProject(type, id);
    }

    public void SelectTagProject(string tag) => this._projectList?.SelectTagProject(tag.ToLower());

    public void SetProjectBackground(ImageBrush brush)
    {
      this._projectList?.SetProjectFoldBackground(brush != null);
    }

    private void OnProjectViewModeChanged(object sender, ProjectIdentity e)
    {
      if (!(e.Id == this._projectList?.GetSelectedProject()?.Id))
        return;
      this.OnProjectSelected((object) null, (EventArgs) null);
    }

    private void SetIdentityCount(ProjectIdentity identity, int count)
    {
      this.Dispatcher.Invoke((Action) (() => this._projectList?.SetItemCount(identity, count)));
    }

    private void OnKanbanBatchTaskDropped(object sender, List<string> e)
    {
      this._projectList?.OnKanbanBatchTaskDropped(e);
    }

    private void OnBatchTaskDropped(object sender, List<string> e)
    {
      if (!this.IsProjectMenuVisible())
        return;
      this._projectList?.OnBatchTaskDropped(e);
    }

    private void OnTaskDragging(object sender, DragMouseEvent e)
    {
      if (!this.IsProjectMenuVisible())
        return;
      this._projectList?.OnTaskDragging(e);
    }

    public async Task<bool> OnDragSectionDroppedOnProject(string dragSectionId, string projectId)
    {
      return await this._projectList.OnDragSectionDropped(dragSectionId, projectId);
    }

    public static ListViewContainer GetListView(string key, Border border)
    {
      if (!ListViewContainer._listViews.ContainsKey(key))
        ListViewContainer._listViews[key] = new ListViewContainer(border);
      return ListViewContainer._listViews[key];
    }

    public static void RemoveListView(string key)
    {
      ListViewContainer listViewContainer;
      if (!ListViewContainer._listViews.TryRemove(key, out listViewContainer))
        return;
      listViewContainer.Clear();
    }

    public static void ReloadCount()
    {
      foreach (ListViewContainer listViewContainer in ListViewContainer._listViews.Values.ToList<ListViewContainer>())
        listViewContainer._projectList?.LoadTaskCount();
    }

    public static void ReloadProjectData(bool refreshCount = true)
    {
      foreach (ListViewContainer listViewContainer in ListViewContainer._listViews.Values.ToList<ListViewContainer>())
        listViewContainer._projectList?.LoadData(refreshCount);
    }

    public static void ReloadTasks()
    {
      foreach (ListViewContainer listViewContainer in ListViewContainer._listViews.Values.ToList<ListViewContainer>())
        listViewContainer.ReloadView();
    }

    public static void OnProjectHide(PtfType ptfType)
    {
      foreach (ListViewContainer listViewContainer in ListViewContainer._listViews.Values.ToList<ListViewContainer>())
      {
        ProjectItemViewModel selectedItem = listViewContainer._projectList?.SelectedItem;
        if (selectedItem != null && selectedItem.GetPtfType() == ptfType)
          listViewContainer._projectList.SetAndSelectDefaultProject();
        listViewContainer._projectList?.LoadData(false);
      }
    }

    public static void OnProjectHide(SmartListType smartType)
    {
      foreach (ListViewContainer listViewContainer in ListViewContainer._listViews.Values.ToList<ListViewContainer>())
      {
        ProjectListView projectList = listViewContainer._projectList;
        SmartListType? nullable1;
        SmartListType? nullable2;
        if (projectList == null)
        {
          nullable1 = new SmartListType?();
          nullable2 = nullable1;
        }
        else
          nullable2 = projectList.GetSelectedSmartListType();
        SmartListType? nullable3 = nullable2;
        if (nullable3.HasValue)
        {
          nullable1 = nullable3;
          SmartListType smartListType = smartType;
          if (nullable1.GetValueOrDefault() == smartListType & nullable1.HasValue)
            listViewContainer._projectList.SetAndSelectDefaultProject();
        }
      }
    }

    public static async Task AcceptShareList(string projectid)
    {
      ObservableCollection<TaskModel> tasks = await Communicator.PullServerTasksByProjectId(projectid);
      if (tasks != null)
      {
        await TaskDao.BatchCreateTaskFromRemote(tasks.ToList<TaskModel>(), true);
        await TagService.CheckTaskTags(tasks.ToList<TaskModel>(), true);
        await Task.Delay(100);
      }
      ListViewContainer.ReloadProjectData();
      tasks = (ObservableCollection<TaskModel>) null;
    }

    public static void ResetIdentityCount(ProjectIdentity identity, int count)
    {
      foreach (ListViewContainer listViewContainer in ListViewContainer._listViews.Values.ToList<ListViewContainer>())
        listViewContainer.SetIdentityCount(identity, count);
    }
  }
}
