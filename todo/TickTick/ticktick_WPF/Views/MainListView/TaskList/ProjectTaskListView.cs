// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.TaskList.ProjectTaskListView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Completed;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Filter;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Print;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.Widget;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MainListView.TaskList
{
  public class ProjectTaskListView : Grid, IBatchEditable
  {
    private Border _trashTitle;
    private bool _canHoverShow;
    private SearchFilterControl _searchFilterControl;
    private CompletedFilterControl _completeFilterControl;
    private CompletedFilterControl _abandonFilterControl;
    private readonly DelayActionHandler _delayLoadHandler;
    private BatchTaskEditHelper _batchHelper;
    private TaskView _parent;
    private Border _menuFoldBorder;
    private StackPanel _titlePanel;
    private EmojiTitleEditor _title;
    private ContentControl _addTaskContent;
    private ContentControl _listContent;
    private StackPanel _listEmpty;
    private EmptyImage _emptyImage;
    private TaskListView _taskList;
    private HoverIconButton _shareButton;
    private HoverIconButton _orderButton;
    private HoverIconButton _moreButton;
    private string _emptyShow;
    private bool _hideFoldMenu;

    private bool _isPersonTrash
    {
      get
      {
        return !(this.ViewModel.ProjectIdentity is TrashProjectIdentity projectIdentity) || projectIdentity.IsPerson;
      }
    }

    public void SwitchTitle(bool isTrash)
    {
      if (isTrash)
      {
        this._titlePanel.Visibility = Visibility.Collapsed;
        this._trashTitle.Visibility = Visibility.Visible;
        if (this._trashTitle.Child == null)
          this.InitTrashTitle();
        this.SetTrashMode();
      }
      else
      {
        this._titlePanel.Visibility = Visibility.Visible;
        this._trashTitle.Visibility = Visibility.Collapsed;
      }
    }

    private void SetTrashMode()
    {
      bool flag = UserManager.IsTeamUser();
      this._trashTitle.IsHitTestVisible = flag;
      if (!(this._trashTitle.Child is StackPanel child1))
        return;
      foreach (object child2 in child1.Children)
      {
        if (!(child2 is Path path))
        {
          if (child2 is TextBlock textBlock)
          {
            string str = Utils.GetString(!flag ? "Trash" : (this._isPersonTrash ? "PersonalTrash" : "TeamTrash"));
            textBlock.Text = str;
          }
        }
        else
        {
          int num = flag ? 0 : 2;
          path.Visibility = (Visibility) num;
        }
      }
    }

    private void InitTrashTitle()
    {
      if (!this.Children.Contains((UIElement) this._trashTitle))
        this.Children.Add((UIElement) this._trashTitle);
      StackPanel stackPanel1 = new StackPanel();
      stackPanel1.Orientation = Orientation.Horizontal;
      stackPanel1.Margin = new Thickness(6.0);
      StackPanel stackPanel2 = stackPanel1;
      this._trashTitle.SetResourceReference(FrameworkElement.StyleProperty, (object) "HoverBorderStyle");
      this._trashTitle.Child = (UIElement) stackPanel2;
      this._trashTitle.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTrashTitleClick);
      TextBlock textBlock = new TextBlock();
      textBlock.IsHitTestVisible = false;
      textBlock.FontSize = 20.0;
      textBlock.FontWeight = FontWeights.Bold;
      TextBlock element = textBlock;
      element.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      Path arrow = UiUtils.GetArrow(16.0, 0.0, "BaseColorOpacity60");
      arrow.IsHitTestVisible = false;
      arrow.Margin = new Thickness(4.0, 0.0, 0.0, 0.0);
      stackPanel2.Children.Add((UIElement) element);
      stackPanel2.Children.Add((UIElement) arrow);
    }

    private void OnTrashTitleClick(object sender, MouseButtonEventArgs e)
    {
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "PersonalTrash", Utils.GetString("PersonalTrash"), (Geometry) null);
      menuItemViewModel1.Selected = this._isPersonTrash;
      types.Add(menuItemViewModel1);
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "TeamTrash", Utils.GetString("TeamTrash"), (Geometry) null);
      menuItemViewModel2.Selected = !this._isPersonTrash;
      types.Add(menuItemViewModel2);
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.PlacementTarget = (UIElement) this._trashTitle;
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) escPopup);
      customMenuList.Operated += new EventHandler<object>(this.OnActionSelected);
      customMenuList.Show();
    }

    private void OnActionSelected(object sender, object e)
    {
      if (!(e is string str))
        return;
      bool flag = this._isPersonTrash;
      switch (str)
      {
        case "PersonalTrash":
          flag = true;
          break;
        case "TeamTrash":
          flag = false;
          break;
      }
      if (flag == this._isPersonTrash || !(this.ViewModel.ProjectIdentity is TrashProjectIdentity projectIdentity))
        return;
      projectIdentity.IsPerson = flag;
      this.TryLoadTrashData();
      this.ReloadTask(true);
      LocalSettings.Settings.Save(true);
      this.SetTrashMode();
      Utils.FindParent<TaskView>((DependencyObject) this)?.HideDetail();
      this._moreButton.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public event TaskSelectDelegate TaskSelect;

    public event EventHandler<List<string>> BatchTaskChanged;

    public event EventHandler<TaskModel> TaskAdded;

    public event EventHandler<string> EventAdded;

    public event EventHandler<List<string>> BatchTaskSelected;

    public event EventHandler OnSearchFilterChanged;

    public event EventHandler<DragMouseEvent> TaskDragging;

    public event EventHandler<string> TaskDrop;

    public event EventHandler<List<string>> BatchTaskDrop;

    public event EventHandler<string> OnProjectActivityClick;

    public Border MenuPathBorder => this._menuFoldBorder;

    public TaskListViewModel ViewModel
    {
      get
      {
        return this.DataContext is TaskListViewModel dataContext ? dataContext : new TaskListViewModel();
      }
    }

    public ProjectTaskListView(TaskView parent)
    {
      Border border1 = new Border();
      border1.Margin = new Thickness(42.0, 26.0, 0.0, 12.0);
      border1.VerticalAlignment = VerticalAlignment.Top;
      border1.HorizontalAlignment = HorizontalAlignment.Left;
      this._trashTitle = border1;
      this._canHoverShow = true;
      this._delayLoadHandler = new DelayActionHandler(200);
      Border border2 = new Border();
      border2.Cursor = Cursors.Hand;
      border2.Margin = new Thickness(20.0, 36.0, 0.0, 12.0);
      border2.HorizontalAlignment = HorizontalAlignment.Left;
      border2.VerticalAlignment = VerticalAlignment.Top;
      border2.Background = (Brush) Brushes.Transparent;
      this._menuFoldBorder = border2;
      StackPanel stackPanel1 = new StackPanel();
      stackPanel1.Margin = new Thickness(38.0, 27.0, 0.0, 12.0);
      stackPanel1.VerticalAlignment = VerticalAlignment.Top;
      stackPanel1.Orientation = Orientation.Horizontal;
      this._titlePanel = stackPanel1;
      this._title = new EmojiTitleEditor();
      ContentControl contentControl = new ContentControl();
      contentControl.Margin = new Thickness(0.0, 8.0, 0.0, 8.0);
      this._addTaskContent = contentControl;
      this._listContent = new ContentControl();
      StackPanel stackPanel2 = new StackPanel();
      stackPanel2.VerticalAlignment = VerticalAlignment.Center;
      this._listEmpty = stackPanel2;
      this._taskList = new TaskListView();
      HoverIconButton hoverIconButton1 = new HoverIconButton();
      hoverIconButton1.HorizontalAlignment = HorizontalAlignment.Right;
      hoverIconButton1.VerticalAlignment = VerticalAlignment.Top;
      hoverIconButton1.Margin = new Thickness(0.0, 31.0, 48.0, 12.0);
      hoverIconButton1.ToolTip = (object) Utils.GetString("Sort");
      this._orderButton = hoverIconButton1;
      HoverIconButton hoverIconButton2 = new HoverIconButton();
      hoverIconButton2.HorizontalAlignment = HorizontalAlignment.Right;
      hoverIconButton2.VerticalAlignment = VerticalAlignment.Top;
      hoverIconButton2.Margin = new Thickness(0.0, 31.0, 20.0, 12.0);
      this._moreButton = hoverIconButton2;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this._parent = parent;
      this.InitBatchHelper();
      this.DataContext = (object) new TaskListViewModel();
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition());
      this.InitFoldIcon();
      this.InitTitle();
      this.InitTopButtons();
      this.InitAddContent();
      this.InitListView();
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnPreviewMouseLeftButtonUp);
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.SyncDone -= new EventHandler<SyncResult>(this.OnSyncChanged);
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnThemeModeChanged);
      DataChangedNotifier.TaskDefaultChanged -= new EventHandler(this.RestartQuickAdd);
      DataChangedNotifier.ProjectColumnChanged -= new EventHandler<string>(this.OnColumnChanged);
      DataChangedNotifier.PomoChanged -= new EventHandler<string>(this.NotifyReload);
      DataChangedNotifier.CalendarChanged -= new EventHandler(this.OnCalendarChanged);
      DataChangedNotifier.ProjectChanged -= new EventHandler(this.OnProjectChanged);
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      DataChangedNotifier.TagChanged -= new EventHandler<TagModel>(this.OnTagChanged);
      DataChangedNotifier.ProjectGroupChanged -= new EventHandler<ProjectGroupModel>(this.OnGroupChanged);
      DataChangedNotifier.ListSectionOpenChanged -= new EventHandler<(string, SortOption)>(this.OnListSectionOpenChanged);
      ClosedTaskWithFilterLoader.CompletionLoader.Loaded -= new EventHandler<bool>(this.LoadedMoreReload);
      ClosedTaskWithFilterLoader.AbandonedLoader.Loaded -= new EventHandler<bool>(this.LoadedMoreReload);
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "HideComplete");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowSubtasks");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowDetails");
      this._delayLoadHandler.StopAndClear();
    }

    private void BindEvents()
    {
      DataChangedNotifier.SyncDone += new EventHandler<SyncResult>(this.OnSyncChanged);
      DataChangedNotifier.CalendarChanged += new EventHandler(this.OnCalendarChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnThemeModeChanged);
      DataChangedNotifier.TaskDefaultChanged += new EventHandler(this.RestartQuickAdd);
      DataChangedNotifier.ProjectColumnChanged += new EventHandler<string>(this.OnColumnChanged);
      DataChangedNotifier.ProjectChanged += new EventHandler(this.OnProjectChanged);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      DataChangedNotifier.TagChanged += new EventHandler<TagModel>(this.OnTagChanged);
      DataChangedNotifier.ProjectGroupChanged += new EventHandler<ProjectGroupModel>(this.OnGroupChanged);
      DataChangedNotifier.PomoChanged += new EventHandler<string>(this.NotifyReload);
      DataChangedNotifier.ListSectionOpenChanged += new EventHandler<(string, SortOption)>(this.OnListSectionOpenChanged);
      ClosedTaskWithFilterLoader.CompletionLoader.Loaded += new EventHandler<bool>(this.LoadedMoreReload);
      ClosedTaskWithFilterLoader.AbandonedLoader.Loaded += new EventHandler<bool>(this.LoadedMoreReload);
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "HideComplete");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowSubtasks");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowDetails");
      this._delayLoadHandler.SetAction(new EventHandler(this.DelayLoad));
    }

    private void OnDisplayChanged(object sender, PropertyChangedEventArgs e) => this.ReloadData();

    private async void OnTagChanged(object sender, TagModel e)
    {
      ProjectTaskListView projectTaskListView = this;
      if (!projectTaskListView.IsVisible || !(projectTaskListView.ViewModel?.ProjectIdentity is TagProjectIdentity projectIdentity))
        return;
      TagModel tagModel = CacheManager.GetTagByName(projectIdentity.Tag);
      if (tagModel != null && (tagModel.viewMode == "timeline" && UserDao.IsPro() || tagModel.viewMode == "kanban"))
      {
        await Task.Delay(500);
        if (!projectTaskListView.IsVisible)
          return;
        DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new TagProjectIdentity(tagModel));
      }
      else if (tagModel != null)
        projectTaskListView.LoadProject((ProjectIdentity) new TagProjectIdentity(tagModel));
      tagModel = (TagModel) null;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (e.NewSize.Width <= 150.0)
        return;
      this._title.SetMaxWidth(e.NewSize.Width - 150.0);
    }

    private void OnListSectionOpenChanged(object sender, (string, SortOption) e)
    {
      ProjectIdentity projectIdentity = this.ViewModel.ProjectIdentity;
      if (projectIdentity == null || object.Equals(sender, (object) this._taskList) || !(projectIdentity.CatId == e.Item1) || !projectIdentity.SortOption.Equal(e.Item2))
        return;
      this._taskList?.ReLoad((string) null);
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      this._taskList?.OnTasksChanged(e);
    }

    private void DelayLoad(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() => this._taskList?.LoadAsync(ignoreFocus: true)));
    }

    private void OnCalendarChanged(object sender, EventArgs e)
    {
      if (!(this.ViewModel.ProjectIdentity is SubscribeCalendarProjectIdentity) && !(this.ViewModel.ProjectIdentity is BindAccountCalendarProjectIdentity))
        return;
      this.LoadProject(this.ViewModel?.ProjectIdentity);
    }

    private void OnColumnChanged(object sender, string e)
    {
      if (!(this.ViewModel.ProjectIdentity is NormalProjectIdentity projectIdentity) || !(projectIdentity.Project.id == e) || !this.ViewModel.ProjectIdentity.SortOption.ContainsSortType(Constants.SortType.sortOrder.ToString()))
        return;
      this.ReloadData();
    }

    private void NotifyReload(object sender, object e) => this.ReloadData();

    private void RestartQuickAdd(object sender, EventArgs e)
    {
      this.LoadQuickAddView(keepText: true);
    }

    private async void OnGroupChanged(object sender, ProjectGroupModel e)
    {
      ProjectTaskListView projectTaskListView = this;
      if (!projectTaskListView.IsVisible)
        group = (GroupProjectIdentity) null;
      else if (!(e?.id == projectTaskListView.ViewModel?.ProjectIdentity?.CatId))
        group = (GroupProjectIdentity) null;
      else if (!(projectTaskListView.ViewModel?.ProjectIdentity is GroupProjectIdentity group))
      {
        group = (GroupProjectIdentity) null;
      }
      else
      {
        ProjectGroupModel groupModel = CacheManager.GetGroupById(group.GroupId);
        if (groupModel != null && (groupModel.viewMode == "timeline" && UserDao.IsPro() || groupModel.viewMode == "kanban"))
        {
          await Task.Delay(500);
          if (!projectTaskListView.IsVisible)
          {
            group = (GroupProjectIdentity) null;
            return;
          }
          group.SetGroup(groupModel);
          DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) group);
        }
        else if (groupModel != null)
        {
          group.SetGroup(groupModel);
          projectTaskListView.LoadProject((ProjectIdentity) group);
        }
        groupModel = (ProjectGroupModel) null;
        group = (GroupProjectIdentity) null;
      }
    }

    private async void OnProjectChanged(object sender, EventArgs e)
    {
      ProjectTaskListView projectTaskListView = this;
      ProjectModel project;
      if (!projectTaskListView.IsVisible)
      {
        project = (ProjectModel) null;
      }
      else
      {
        // ISSUE: reference to a compiler-generated method
        project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(projectTaskListView.\u003COnProjectChanged\u003Eb__63_0));
        if (project?.viewMode == "kanban")
        {
          await Task.Delay(500);
          if (!projectTaskListView.IsVisible)
          {
            project = (ProjectModel) null;
          }
          else
          {
            DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new NormalProjectIdentity(project));
            project = (ProjectModel) null;
          }
        }
        else if (project?.kind != Constants.ProjectKind.NOTE.ToString() && project?.viewMode == "timeline" && UserDao.IsPro())
        {
          await Task.Delay(500);
          if (!projectTaskListView.IsVisible)
          {
            project = (ProjectModel) null;
          }
          else
          {
            DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new NormalProjectIdentity(project));
            project = (ProjectModel) null;
          }
        }
        else if (projectTaskListView.ViewModel?.ProjectIdentity is GroupProjectIdentity projectIdentity)
        {
          ProjectGroupModel groupById = CacheManager.GetGroupById(projectIdentity.GroupId);
          if (groupById == null)
          {
            project = (ProjectModel) null;
          }
          else
          {
            List<ProjectModel> projectsInGroup = CacheManager.GetProjectsInGroup(projectIdentity.GroupId);
            projectTaskListView.ViewModel.ProjectIdentity = (ProjectIdentity) new GroupProjectIdentity(groupById, projectsInGroup);
            projectTaskListView.LoadQuickAddView(keepText: true);
            project = (ProjectModel) null;
          }
        }
        else if (project == null)
        {
          project = (ProjectModel) null;
        }
        else
        {
          projectTaskListView.LoadProject((ProjectIdentity) new NormalProjectIdentity(project));
          projectTaskListView.LoadQuickAddView(keepText: true);
          project = (ProjectModel) null;
        }
      }
    }

    private async void OnFilterChanged(object sender, FilterChangeArgs e)
    {
      ProjectTaskListView projectTaskListView = this;
      if (!projectTaskListView.IsVisible || !(e.Filter?.id == projectTaskListView.ViewModel?.ProjectIdentity.Id) || !(projectTaskListView.ViewModel?.ProjectIdentity is FilterProjectIdentity projectIdentity))
        return;
      FilterModel filter = CacheManager.GetFilterById(projectIdentity.Id);
      if (filter != null && (filter.viewMode == "timeline" && UserDao.IsPro() || filter.viewMode == "kanban"))
      {
        await Task.Delay(500);
        if (!projectTaskListView.IsVisible)
          return;
        DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new FilterProjectIdentity(filter));
      }
      else
      {
        if (filter != null)
        {
          FilterProjectIdentity taskDefault = new FilterProjectIdentity(filter);
          projectTaskListView.LoadProject((ProjectIdentity) taskDefault);
          if (e.RuleChanged)
            projectTaskListView.GetQuickAddView()?.ResetView((IProjectTaskDefault) taskDefault, keepText: true);
        }
        filter = (FilterModel) null;
      }
    }

    private void OnThemeModeChanged(object sender, EventArgs e)
    {
      if (this.ViewModel?.ProjectData == null || !this.IsVisible)
        return;
      this.ReloadData(forceFocus: false);
    }

    private void LoadedMoreReload(object sender, bool e)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (e)
          this.ReloadData(true);
        else
          this._taskList.HideLoadMore();
      }));
    }

    private void OnSyncChanged(object sender, SyncResult e)
    {
      if (!e.RemoteTasksChanged || this.Visibility != Visibility.Visible)
        return;
      this.ReloadData();
    }

    private void InitFoldIcon()
    {
      Image image1 = new Image();
      image1.Width = 18.0;
      image1.Height = 18.0;
      image1.Stretch = Stretch.Uniform;
      image1.IsHitTestVisible = false;
      Image image2 = image1;
      this._menuFoldBorder.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle60_100");
      image2.SetResourceReference(Image.SourceProperty, (object) "HideMenuDrawingImage");
      this._menuFoldBorder.Child = (UIElement) image2;
      this._menuFoldBorder.MouseEnter += new MouseEventHandler(this.OnShowMenuMouseEnter);
      this._menuFoldBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.MenuBorderMouseLeftButtonUp);
      this.Children.Add((UIElement) this._menuFoldBorder);
    }

    private void InitTitle()
    {
      this._titlePanel.Children.Add((UIElement) this._title);
      this._title.TextChanged += new EventHandler<string>(this.OnTitleTextChanged);
      this.Children.Add((UIElement) this._titlePanel);
      this._title.SetCheckFunc(new Func<string, string>(this.CheckTitleValid));
      this._titlePanel.SetValue(Panel.ZIndexProperty, (object) 1000);
      this._titlePanel.SetValue(Grid.RowSpanProperty, (object) 3);
    }

    private string CheckTitleValid(string text)
    {
      return this.ViewModel?.ProjectIdentity?.CheckTitleValid(text);
    }

    private void OnTitleTextChanged(object sender, string e)
    {
      this.ViewModel.ProjectIdentity?.SaveTitle(e);
      this.ViewModel.Title = e;
    }

    private void InitTopButtons()
    {
      this._orderButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "SortDrawingImage");
      this._orderButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectOrderClick);
      this.Children.Add((UIElement) this._orderButton);
      this._moreButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "MoreDrawingImage");
      this._moreButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoreButtonClick);
      this.Children.Add((UIElement) this._moreButton);
    }

    private void InitAddContent()
    {
      this._addTaskContent.SetValue(Grid.RowProperty, (object) 1);
      this.Children.Add((UIElement) this._addTaskContent);
    }

    private void InitListView()
    {
      this._listContent.SetValue(Grid.RowProperty, (object) 2);
      this._listContent.Content = (object) this._taskList;
      this._taskList.DataContext = (object) (this.DataContext as TaskListViewModel);
      this.Children.Add((UIElement) this._listContent);
      this._listEmpty.SetValue(Grid.RowProperty, (object) 0);
      this._listEmpty.SetValue(Grid.RowSpanProperty, (object) 3);
      this.Children.Add((UIElement) this._listEmpty);
      this._taskList.ItemSelect += new TaskListView.SelectDelegate(this.OnItemSelect);
      this._taskList.ItemsCountChanged += new EventHandler(this.OnTaskCountChanged);
      this._taskList.BatchSelect += new EventHandler<List<string>>(this.OnBatchTaskSelected);
      this._taskList.DragOver += new EventHandler<DragMouseEvent>(this.OnTaskDragging);
      this._taskList.DragDropped += new EventHandler<string>(this.OnTaskDrop);
      this._taskList.BatchDragDropped += new EventHandler<List<string>>(this.OnBatchTaskDrop);
      this._taskList.MoveUpCaret += new EventHandler(this.OnListCaretMoveUp);
    }

    private void InitBatchHelper()
    {
      this._batchHelper = new BatchTaskEditHelper((IBatchEditable) this);
    }

    public async Task LoadProject(
      ProjectIdentity projectIdentity,
      bool forceRefresh = false,
      bool forceLoad = false)
    {
      Task task = await this.Dispatcher.InvokeAsync<Task>((Func<Task>) (async () => await this.LoadProjectAsync(projectIdentity, forceRefresh, forceLoad)));
    }

    private async Task LoadProjectAsync(
      ProjectIdentity projectIdentity,
      bool forceRefresh = false,
      bool forceLoad = false)
    {
      ProjectTaskListView editor = this;
      TaskListView taskList = editor._taskList;
      if (taskList == null)
      {
        await Task.Delay(100);
        taskList = editor._taskList;
        if (taskList == null)
        {
          taskList = (TaskListView) null;
          return;
        }
      }
      if (projectIdentity == null)
      {
        taskList = (TaskListView) null;
      }
      else
      {
        bool projectChanged = editor.ViewModel.ProjectIdentity.CatId != projectIdentity.CatId || editor.ViewModel.ProjectIdentity.CanEdit != projectIdentity.CanEdit;
        if (projectIdentity is TrashProjectIdentity trashProjectIdentity && editor.ViewModel.ProjectIdentity is TrashProjectIdentity projectIdentity1)
          trashProjectIdentity.IsPerson = projectIdentity1.IsPerson;
        if (!(editor.DataContext is TaskListViewModel taskListViewModel1))
        {
          taskListViewModel1 = new TaskListViewModel(projectIdentity);
          editor.DataContext = (object) taskListViewModel1;
        }
        else
          taskListViewModel1.SetIdentity(projectIdentity);
        taskListViewModel1.SetBatchEditor((IBatchEditable) editor);
        editor._title.SetText(taskListViewModel1.Title);
        editor._title.SetMaxWidth(editor.ActualWidth - 150.0);
        editor._title.SetEnable(projectIdentity is NormalProjectIdentity normalProjectIdentity && normalProjectIdentity.CanEdit && normalProjectIdentity.Project != null && !normalProjectIdentity.Project.Isinbox || projectIdentity is GroupProjectIdentity || projectIdentity is FilterProjectIdentity);
        if (forceRefresh)
          taskList.ScrollToTop();
        editor._batchHelper.ProjectIdentity = projectIdentity;
        TaskListViewModel taskListViewModel = taskListViewModel1;
        taskListViewModel.ProjectData = await ProjectTaskDataProvider.GetProjectData(projectIdentity);
        taskListViewModel = (TaskListViewModel) null;
        editor.SetShareIcon();
        if (projectChanged || editor._addTaskContent.Content == null)
          editor.LoadQuickAddView();
        editor.UpdateView();
        if (projectChanged)
        {
          taskList.SelectedId = "";
          taskList.ScrollToTop();
          taskList.SectionEditing = false;
        }
        if (forceLoad)
          taskList.SetTitleCaretIndex(-1);
        LocalSettings.Settings.InSearch = editor.ViewModel.ProjectData is SearchListData;
        switch (editor.ViewModel.ProjectData)
        {
          case SearchListData _:
            editor.InitSearchData();
            LocalSettings.Settings.InSearch = true;
            taskList.ScrollToTop();
            break;
          case CompleteListData _:
            editor.InitCompletedData();
            break;
          case AbandonedListData _:
            editor.InitAbandonedData();
            break;
          case TrashListData _:
            if (projectChanged)
            {
              editor.TryLoadTrashData();
              editor.Children.Remove((UIElement) editor._completeFilterControl);
              editor._completeFilterControl = (CompletedFilterControl) null;
              editor.Children.Remove((UIElement) editor._abandonFilterControl);
              editor._abandonFilterControl = (CompletedFilterControl) null;
              editor.Children.Remove((UIElement) editor._searchFilterControl);
              editor._searchFilterControl = (SearchFilterControl) null;
              break;
            }
            break;
          default:
            editor.Children.Remove((UIElement) editor._completeFilterControl);
            editor._completeFilterControl = (CompletedFilterControl) null;
            editor.Children.Remove((UIElement) editor._abandonFilterControl);
            editor._abandonFilterControl = (CompletedFilterControl) null;
            editor.Children.Remove((UIElement) editor._searchFilterControl);
            editor._searchFilterControl = (SearchFilterControl) null;
            break;
        }
        editor.UpdateLayout();
        await editor.LoadTaskAsync(forceLoad);
        editor.TryLoadClosedProjectData(editor.ViewModel.ProjectData);
        taskList.TryLoadCompletedTasks(editor.ViewModel.ProjectIdentity);
        taskList = (TaskListView) null;
      }
    }

    private async void TryLoadTrashData()
    {
      bool flag = !TrashSyncService.Next(this._isPersonTrash).HasValue;
      if (flag)
        flag = await this.ViewModel.TrashTaskLoader.TryLoadTrashTasks(this._isPersonTrash);
      if (!flag)
        return;
      this.ReloadList();
    }

    private void UpdateView()
    {
      SortProjectData projectData = this.ViewModel.ProjectData;
      if (projectData == null)
        return;
      this.SwitchTitle(projectData.IsTrash);
      HoverIconButton orderButton = this._orderButton;
      int num;
      if (!projectData.IsCompleted && !projectData.IsProjectClosed && !(projectData is SearchListData) && !(projectData is PreviewListData) && !projectData.IsTeamExpired)
      {
        switch (projectData)
        {
          case BindAccountCalendarListData _:
          case SubscribeCalendarListData _:
          case TrashListData _:
          case DateListData _:
            break;
          default:
            if (!(this.ViewModel.ProjectIdentity is SummaryProjectIdentity))
            {
              num = 0;
              goto label_6;
            }
            else
              break;
        }
      }
      num = 2;
label_6:
      orderButton.Visibility = (Visibility) num;
      bool flag1 = !projectData.IsProjectClosed && !projectData.IsCompleted && !(projectData is SearchListData) && !(projectData is PreviewListData) && !projectData.IsTeamExpired && !(projectData is TrashListData) && !(this.ViewModel.ProjectIdentity is SummaryProjectIdentity);
      bool flag2 = projectData is TrashListData && this._isPersonTrash;
      bool flag3 = projectData.IsCompleted || projectData is SubscribeCalendarListData || projectData is BindAccountCalendarListData;
      this._moreButton.MouseLeftButtonUp -= new MouseButtonEventHandler(this.MoreButtonClick);
      this._moreButton.MouseLeftButtonUp -= new MouseButtonEventHandler(this.ShowPrintPopup);
      this._moreButton.MouseLeftButtonUp -= new MouseButtonEventHandler(this.ClearMouseLeftButtonUp);
      if (flag1 && !flag3)
      {
        this._moreButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "MoreDrawingImage");
        this._moreButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoreButtonClick);
        this._moreButton.ToolTip = (object) Utils.GetString("More");
      }
      else if (flag2)
      {
        this._moreButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "ClearTrashDrawingImage");
        this._moreButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearMouseLeftButtonUp);
        this._moreButton.ToolTip = (object) Utils.GetString("ClearTrash");
      }
      else if (flag3)
      {
        this._moreButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "PrintDrawingImage");
        this._moreButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowPrintPopup);
        this._moreButton.ToolTip = (object) Utils.GetString("Print");
      }
      this._titlePanel.Visibility = this.ViewModel.ProjectData is SearchListData || this.ViewModel.ProjectData.IsTrash ? Visibility.Collapsed : Visibility.Visible;
      this._moreButton.Visibility = flag1 || flag2 || flag3 ? Visibility.Visible : Visibility.Collapsed;
      this._menuFoldBorder.Visibility = this.ViewModel.ProjectData is SearchListData || this._hideFoldMenu ? Visibility.Collapsed : Visibility.Visible;
    }

    private void SetShareIcon()
    {
      if ((!this.ViewModel.ProjectData.ShowShare || !this.ViewModel.ProjectData.ShowAssignSort || this.ViewModel.ProjectData.IsProjectClosed ? 0 : (!this.ViewModel.ProjectData.IsTeamExpired ? 1 : 0)) != 0)
      {
        if (this._shareButton == null)
        {
          HoverIconButton hoverIconButton = new HoverIconButton();
          hoverIconButton.HorizontalAlignment = HorizontalAlignment.Left;
          hoverIconButton.VerticalAlignment = VerticalAlignment.Center;
          hoverIconButton.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
          hoverIconButton.ImageWidth = 18.0;
          this._shareButton = hoverIconButton;
          this._shareButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShareMouseLeftButtonUp);
          this._titlePanel.Children.Add((UIElement) this._shareButton);
        }
        if (!(this.ViewModel?.ProjectData is NormalListData projectData))
          return;
        switch (projectData.Project.permission)
        {
          case "read":
            HoverIconButton shareButton1 = this._shareButton;
            System.Windows.Controls.ToolTip toolTip1 = new System.Windows.Controls.ToolTip();
            toolTip1.Content = (object) Utils.GetString("ReadOnly");
            shareButton1.ToolTip = (object) toolTip1;
            this._shareButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "ReadOnlyDrawingImage");
            break;
          case "comment":
            HoverIconButton shareButton2 = this._shareButton;
            System.Windows.Controls.ToolTip toolTip2 = new System.Windows.Controls.ToolTip();
            toolTip2.Content = (object) Utils.GetString("CanComment");
            shareButton2.ToolTip = (object) toolTip2;
            this._shareButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "CanCommentDrawingImage");
            break;
          default:
            HoverIconButton shareButton3 = this._shareButton;
            System.Windows.Controls.ToolTip toolTip3 = new System.Windows.Controls.ToolTip();
            toolTip3.Content = (object) Utils.GetString("Editable");
            shareButton3.ToolTip = (object) toolTip3;
            this._shareButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "CanEditDrawingImage");
            break;
        }
      }
      else
      {
        this._titlePanel.Children.Remove((UIElement) this._shareButton);
        this._shareButton = (HoverIconButton) null;
      }
    }

    private void ShareMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (string.IsNullOrEmpty(this.ViewModel?.ProjectData.DefaultProjectModel.id))
        return;
      this.ShareProject(this.ViewModel.ProjectData.DefaultProjectModel.id);
    }

    private void LoadQuickAddView(bool focus = false, bool keepText = false)
    {
      ProjectIdentity projectIdentity = this.ViewModel?.ProjectIdentity;
      if (projectIdentity == null)
        return;
      if (string.IsNullOrEmpty(this.ViewModel?.ProjectData?.AddTaskHint))
      {
        this._addTaskContent.Visibility = Visibility.Collapsed;
        this._addTaskContent.Content = (object) null;
      }
      else
        this._addTaskContent.Visibility = Visibility.Visible;
      if (projectIdentity is BindAccountCalendarProjectIdentity calendarProjectIdentity)
      {
        if (calendarProjectIdentity.Account == null || calendarProjectIdentity.Account.Expired || SubscribeCalendarHelper.GetDefaultCalendar(calendarProjectIdentity.Account.Id) == null)
          return;
        if (this._addTaskContent.Content is CalendarAddView content)
        {
          content.ResetView((IProjectTaskDefault) projectIdentity, focus);
          content.SetAccountKind(calendarProjectIdentity.Account.Kind);
        }
        else
        {
          CalendarAddView calendarAddView = new CalendarAddView(calendarProjectIdentity.Account, focus);
          calendarAddView.CalendarEventAdded += new EventHandler<CalendarEventModel>(this.OnEventAdded);
          this._addTaskContent.Content = (object) calendarAddView;
        }
      }
      else if (this._addTaskContent.Content is QuickAddView content1 && !(content1 is CalendarAddView))
      {
        content1.ResetView((IProjectTaskDefault) projectIdentity, focus, keepText);
      }
      else
      {
        QuickAddView quickAddView = new QuickAddView((IProjectTaskDefault) projectIdentity, focus: focus);
        quickAddView.TitleText.MoveDown += new EventHandler(this.OnQuickAddMoveDown);
        quickAddView.HideGuide += new EventHandler(this.OnHideGuide);
        quickAddView.TaskAdded += new EventHandler<TaskModel>(this.OnTaskQuickAdded);
        this._addTaskContent.Content = (object) quickAddView;
      }
    }

    private void OnTaskQuickAdded(object sender, TaskModel task)
    {
      UtilLog.Info("TaskList.AddTask " + task.id + " from:quickAdd");
      if (this.ViewModel.ProjectIdentity is FilterProjectIdentity projectIdentity)
      {
        List<TaskBaseViewModel> tasksMatchedFilter = TaskService.GetTasksMatchedFilter(projectIdentity.Filter, new List<TaskBaseViewModel>()
        {
          new TaskBaseViewModel(task)
        });
        if (tasksMatchedFilter == null || tasksMatchedFilter.Count == 0)
        {
          Utils.GetToastWindow()?.ToastMoveProjectControl(task.projectId, task.title, MoveToastType.Add);
          return;
        }
      }
      if (sender == null || !sender.Equals((object) this.GetQuickAddView()))
        return;
      this._taskList.SelectedId = task.id;
      EventHandler<TaskModel> taskAdded = this.TaskAdded;
      if (taskAdded == null)
        return;
      taskAdded((object) this, task);
    }

    private void OnQuickAddMoveDown(object sender, EventArgs e)
    {
      if (!this._taskList.IsVisible)
        return;
      this._taskList.TryFocusFirstItem();
    }

    private void OnEventAdded(object sender, CalendarEventModel e)
    {
      this._taskList.SelectedId = e.Id;
      this._taskList.LoadAsync(true);
      EventHandler<string> eventAdded = this.EventAdded;
      if (eventAdded == null)
        return;
      eventAdded((object) this, e.Id);
    }

    public void NavigateTask(string taskId)
    {
      this._taskList.SetItemSelected(this.ViewModel.Items.ToList<DisplayItemModel>(), taskId);
    }

    private async void ClearMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      bool? nullable = new CustomerDialog(Utils.GetString("ClearTrash"), Utils.GetString("SureClearTrash"), Utils.GetString("OK"), Utils.GetString("Cancel")).ShowDialog();
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
        return;
      await TaskService.BatchClearTrash(await TaskDao.GetDeletedIds());
      this._taskList.LoadAsync();
    }

    private async void TryLoadClosedProjectData(SortProjectData projectDataModel)
    {
      ProjectModel project = projectDataModel is NormalListData normalListData ? normalListData.Project : (ProjectModel) null;
      if (project == null || !project.closed.HasValue || !project.closed.Value)
        return;
      await SyncManager.PullProjectTasks(project.id);
      this.ReloadData();
    }

    public async void TryLoadCompletedTasks(ProjectIdentity identity)
    {
      ProjectTaskListView projectTaskListView = this;
      TaskListViewModel vm = projectTaskListView.DataContext as TaskListViewModel;
      bool flag = vm != null;
      if (flag)
        flag = await vm.CompletedTaskLoader.NeedPullCompletedTasks(identity);
      if (!flag)
        vm = (TaskListViewModel) null;
      else if (!await vm.CompletedTaskLoader.TryLoadCompletedTasks(identity))
      {
        vm = (TaskListViewModel) null;
      }
      else
      {
        projectTaskListView._taskList.LoadAsync(true);
        vm = (TaskListViewModel) null;
      }
    }

    public void ReloadTask(bool forceReload = false, string taskId = "", bool forceFocus = true)
    {
      if (!string.IsNullOrEmpty(taskId))
        this._taskList.SelectedId = taskId;
      this.LoadTaskAsync(forceReload, forceFocus);
    }

    private async Task LoadTaskAsync(bool forceReload = false, bool forceFocus = true)
    {
      await this._taskList.LoadAsync(forceReload, setSelect: forceFocus);
    }

    private void OnItemSelect(ListItemSelectModel model)
    {
      TaskSelectDelegate taskSelect = this.TaskSelect;
      if (taskSelect == null)
        return;
      taskSelect(model);
    }

    private void OnTaskCountChanged(object sender, EventArgs e)
    {
      this.RefreshEmptyView(this.ViewModel.Items.Any<DisplayItemModel>() ? 1 : 0);
    }

    private void RefreshEmptyView(int count)
    {
      this._listEmpty.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
      if (count == 0)
      {
        this.SetEmptyView(this.DataContext is TaskListViewModel dataContext ? dataContext.ProjectData : (SortProjectData) null);
      }
      else
      {
        this._emptyShow = (string) null;
        this._emptyImage = (EmptyImage) null;
        this._listEmpty.Children.Clear();
      }
      this.SetDetailImageVisible(count);
    }

    private void SetDetailImageVisible(int count)
    {
      this._parent?.SetEmptyImageVisible(count > 0 ? Visibility.Visible : Visibility.Collapsed, true, this.ViewModel.ProjectIdentity.IsNote);
    }

    private async void SetEmptyView(SortProjectData projectDataModel)
    {
      ProjectTaskListView projectTaskListView = this;
      if (projectDataModel == null || projectTaskListView._emptyShow == projectDataModel.ProjectIdentity?.Id)
        return;
      projectTaskListView._emptyShow = projectDataModel.ProjectIdentity?.Id;
      if (projectTaskListView._emptyImage == null)
      {
        projectTaskListView._emptyImage = new EmptyImage();
        projectTaskListView._listEmpty.Children.Insert(0, (UIElement) projectTaskListView._emptyImage);
      }
      projectTaskListView._listEmpty.Children.RemoveRange(1, projectTaskListView._listEmpty.Children.Count - 1);
      projectTaskListView._emptyImage.Image.Source = (ImageSource) projectDataModel.GetEmptyImage();
      projectTaskListView._emptyImage.Path.Data = projectDataModel.GetEmptyPath();
      projectTaskListView._emptyImage.Path.Margin = projectDataModel.GetEmptyMargin();
      if (projectDataModel.ShowFilterExpired())
      {
        TextBlock textBlock = new TextBlock();
        textBlock.FontSize = 14.0;
        textBlock.HorizontalAlignment = HorizontalAlignment.Center;
        TextBlock element = textBlock;
        element.Inlines.Add((Inline) new Run()
        {
          Text = (Utils.GetString("FilterExpiredHead") + " ")
        });
        Run run1 = new Run();
        run1.Text = Utils.GetString("FilterExpiredCenter");
        run1.Background = (Brush) Brushes.Transparent;
        run1.Cursor = Cursors.Hand;
        Run run2 = run1;
        run2.SetResourceReference(TextElement.ForegroundProperty, (object) "PrimaryColor");
        run2.MouseLeftButtonUp += new MouseButtonEventHandler(projectTaskListView.OnExpiredFilterClick);
        element.Inlines.Add((Inline) run2);
        element.Inlines.Add((Inline) new Run()
        {
          Text = (" " + Utils.GetString("FilterExpiredTail"))
        });
        element.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
        projectTaskListView._listEmpty.Children.Add((UIElement) element);
      }
      else if (projectDataModel.ShowCalendarExpired())
      {
        TextBlock textBlock = new TextBlock();
        textBlock.Text = Utils.GetString("Reauthorize");
        textBlock.Margin = new Thickness(10.0, 10.0, 10.0, 0.0);
        textBlock.FontSize = 14.0;
        textBlock.HorizontalAlignment = HorizontalAlignment.Center;
        textBlock.Cursor = Cursors.Hand;
        TextBlock element = textBlock;
        element.SetResourceReference(TextBlock.ForegroundProperty, (object) "TextAccentColor");
        element.MouseLeftButtonUp += new MouseButtonEventHandler(projectTaskListView.OnExpiredCalendarClick);
        projectTaskListView._listEmpty.Children.Add((UIElement) element);
      }
      else
      {
        TextBlock textBlock1 = new TextBlock();
        TextBlock textBlock2 = textBlock1;
        textBlock2.Text = await projectDataModel.GetEmptyTitle();
        textBlock1.FontSize = 16.0;
        textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
        TextBlock element1 = textBlock1;
        textBlock2 = (TextBlock) null;
        textBlock1 = (TextBlock) null;
        element1.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
        projectTaskListView._listEmpty.Children.Add((UIElement) element1);
        TextBlock textBlock = new TextBlock();
        textBlock.Text = projectDataModel.GetEmptyContent();
        textBlock.FontSize = 13.0;
        textBlock.HorizontalAlignment = HorizontalAlignment.Center;
        textBlock.TextWrapping = TextWrapping.Wrap;
        textBlock.Margin = new Thickness(10.0, 10.0, 10.0, 0.0);
        TextBlock element2 = textBlock;
        element2.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
        projectTaskListView._listEmpty.Children.Add((UIElement) element2);
      }
    }

    private void OnExpiredCalendarClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TaskListViewModel dataContext))
        return;
      if (dataContext.ProjectData is BindAccountCalendarListData projectData1)
      {
        if (projectData1.Account.IsBindAccountPassword())
        {
          BindAccountWindow bindAccountWindow = new BindAccountWindow(projectData1.Account);
          bindAccountWindow.Owner = Utils.GetParentWindow((DependencyObject) this);
          bindAccountWindow.ShowDialog();
        }
        else if (projectData1.Account.Site == "outlook")
        {
          BindOutlookWindow bindOutlookWindow = new BindOutlookWindow();
          bindOutlookWindow.SetOriginAccount(projectData1.Account?.Id);
          bindOutlookWindow.ShowDialog();
        }
        else if (projectData1.Account.Site == "google")
        {
          BindGoogleAccount.GetInstance().Start(projectData1.Account?.Id);
        }
        else
        {
          if (!(projectData1.Account.Kind == "icloud"))
            return;
          new BindICloudWindow((SubscribeCalendar) null, projectData1.Account).ShowDialog();
        }
      }
      else
      {
        if (!(dataContext.ProjectData is SubscribeCalendarListData projectData))
          return;
        EditUrlWindow editUrlWindow = new EditUrlWindow(new ticktick_WPF.Views.Config.SubscribeCalendarViewModel(projectData.Profile));
        editUrlWindow.Owner = Window.GetWindow((DependencyObject) this);
        editUrlWindow.ShowDialog();
      }
    }

    private void OnExpiredFilterClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TaskListViewModel dataContext) || !(dataContext.ProjectIdentity is FilterProjectIdentity projectIdentity))
        return;
      FilterEditDialog filterEditDialog = new FilterEditDialog(projectIdentity.Filter);
      filterEditDialog.Owner = Window.GetWindow((DependencyObject) this);
      filterEditDialog.ShowDialog();
    }

    private void OnBatchTaskSelected(object sender, List<string> taskIds)
    {
      EventHandler<List<string>> batchTaskSelected = this.BatchTaskSelected;
      if (batchTaskSelected == null)
        return;
      batchTaskSelected((object) this, taskIds);
    }

    private void OnTaskDrop(object sender, string taskId)
    {
      EventHandler<string> taskDrop = this.TaskDrop;
      if (taskDrop == null)
        return;
      taskDrop((object) this, taskId);
    }

    private void OnBatchTaskDrop(object sender, List<string> taskIds)
    {
      EventHandler<List<string>> batchTaskDrop = this.BatchTaskDrop;
      if (batchTaskDrop == null)
        return;
      batchTaskDrop((object) this, taskIds);
    }

    private void OnTaskDragging(object sender, DragMouseEvent e)
    {
      EventHandler<DragMouseEvent> taskDragging = this.TaskDragging;
      if (taskDragging == null)
        return;
      taskDragging((object) this, e);
    }

    private void OnListCaretMoveUp(object sender, EventArgs e)
    {
      if (this._addTaskContent.Visibility != Visibility.Visible || !(this._addTaskContent.Content is QuickAddView content))
        return;
      content.FocusText();
    }

    public void FocusQuickAdd() => this.LoadQuickAddView(true, true);

    private async void SelectOrderClick(object sender, MouseButtonEventArgs e)
    {
      ProjectTaskListView projectTaskListView = this;
      if (projectTaskListView.ViewModel?.ProjectIdentity == null || projectTaskListView.ViewModel?.ProjectData == null)
        return;
      ProjectIdentity projectIdentity = projectTaskListView.ViewModel?.ProjectIdentity;
      List<SortTypeViewModel> projectSortTypeModels;
      if (!(projectIdentity is GroupProjectIdentity groupProjectIdentity))
      {
        TagProjectIdentity tagProjectIdentity = projectIdentity as TagProjectIdentity;
        if (tagProjectIdentity == null)
        {
          switch (projectIdentity)
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
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = (UIElement) projectTaskListView._orderButton;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.HorizontalOffset = -156.0;
      escPopup.VerticalOffset = -5.0;
      escPopup.StaysOpen = false;
      escPopup.PopupAnimation = PopupAnimation.Fade;
      EscPopup popup = escPopup;
      SortTypeSelector sortTypeSelector = new SortTypeSelector(projectTaskListView.ViewModel.ProjectIdentity, projectSortTypeModels, projectTaskListView.ViewModel.SortOption, false, popup, (List<CustomMenuItemViewModel>) null);
      sortTypeSelector.ResetSortOrder += new EventHandler<int>(projectTaskListView.OnResetSortOrder);
      sortTypeSelector.SortOptionSelect += new EventHandler<SortOption>(projectTaskListView.OnSortOptionSelect);
      sortTypeSelector.Show();
    }

    private async void OnSortOptionSelect(object sender, SortOption sortOption)
    {
      if (this.ViewModel.ProjectData == null)
        return;
      this.ViewModel.ProjectData.SortOption = sortOption;
      this.ViewModel.ProjectIdentity.SortOption = this.ViewModel.ProjectData.SortOption;
      this.ViewModel.ProjectData.SaveSortOption(this.ViewModel.ProjectData.SortOption);
      UtilLog.Info(string.Format("TaskList.Om SetSortType {0},Id {1},type {2}", (object) this.ViewModel.ProjectIdentity, (object) this.ViewModel.ProjectIdentity.Id, (object) this.ViewModel.ProjectData.SortOption));
      await this._taskList.LoadAsync();
      SyncManager.TryDelaySync(1000);
    }

    private async void OnResetSortOrder(object sender, int e)
    {
      await TaskSortOrderService.DeleteAllSortOrderBySortOptionInListId(this.ViewModel.ProjectData.ProjectIdentity.SortOption, this.ViewModel.ProjectData.ProjectIdentity.GetSortProjectId());
      UserActCollectUtils.AddClickEvent("tasklist", "sort_note", "reset_tag");
      DataChangedNotifier.NotifySortOptionChanged(this.ViewModel.ProjectData.ProjectIdentity.CatId);
      SyncManager.TryDelaySync();
    }

    private async void MoreButtonClick(object sender, MouseButtonEventArgs e)
    {
      ProjectTaskListView projectTaskListView = this;
      if (!(projectTaskListView.DataContext is TaskListViewModel dataContext))
        return;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      if (!(dataContext.ProjectIdentity is BindAccountCalendarProjectIdentity))
      {
        List<CustomMenuItemViewModel> menuItemViewModelList = types;
        CustomMenuItemViewModel menuItemViewModel = new CustomMenuItemViewModel((object) "showComplete", Utils.GetString(LocalSettings.Settings.HideComplete ? "ShowCompleted" : "HideCompleted"), Utils.GetImageSource(LocalSettings.Settings.HideComplete ? "showCompletedDrawingImage" : "HideCompletedDrawingImage"));
        menuItemViewModel.ShowSelected = false;
        menuItemViewModelList.Add(menuItemViewModel);
      }
      List<CustomMenuItemViewModel> menuItemViewModelList1 = types;
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "showDetail", Utils.GetString(LocalSettings.Settings.ShowDetails ? "HideDetails" : "ShowDetails"), Utils.GetImageSource(LocalSettings.Settings.ShowDetails ? "HideDetailsDrawingImage" : "showDetailsDrawingImage"));
      menuItemViewModel1.ShowSelected = false;
      menuItemViewModelList1.Add(menuItemViewModel1);
      List<CustomMenuItemViewModel> menuItemViewModelList2 = types;
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "showCountDown", Utils.GetString(LocalSettings.Settings.ShowCountDown ? "ShowTaskDate" : "ShowCountDown"), Utils.GetImageSource(LocalSettings.Settings.ShowCountDown ? "TimeDrawingLine" : "SwitchCountDownDrawingImage"));
      menuItemViewModel2.ShowSelected = false;
      menuItemViewModelList2.Add(menuItemViewModel2);
      if ((!UserDao.IsPro() || !(dataContext.ProjectIdentity is TodayProjectIdentity)) && !(dataContext.ProjectIdentity is TomorrowProjectIdentity) && !(dataContext.ProjectIdentity is WeekProjectIdentity) && !(dataContext.ProjectIdentity is DateProjectIdentity) && (!(dataContext.ProjectIdentity is FilterProjectIdentity projectIdentity1) || !projectIdentity1.Filter.ContainsDate()))
      {
        SortProjectData projectData = dataContext.ProjectData;
        if ((projectData != null ? (projectData.IsCompleted ? 1 : 0) : 0) == 0)
          goto label_7;
      }
      List<CustomMenuItemViewModel> menuItemViewModelList3 = types;
      CustomMenuItemViewModel menuItemViewModel3 = new CustomMenuItemViewModel((object) "showCheckItem", Utils.GetString(LocalSettings.Settings.ShowSubtasks ? "HideSubtask" : "ShowSubtask"), Utils.GetImageSource(LocalSettings.Settings.ShowSubtasks ? "HideSubtaskDrawingImage" : "showsubtaskDrawingImage"));
      menuItemViewModel3.ShowSelected = false;
      menuItemViewModelList3.Add(menuItemViewModel3);
label_7:
      CustomMenuItemViewModel menuItemViewModel4 = new CustomMenuItemViewModel((object) null);
      types.Add(menuItemViewModel4);
      int num = 1;
      if (dataContext.ProjectIdentity is NormalProjectIdentity projectIdentity2 && projectIdentity2.Project.IsEnable() && projectIdentity2.SortOption.groupBy == "sortOrder")
      {
        ++num;
        List<CustomMenuItemViewModel> menuItemViewModelList4 = types;
        CustomMenuItemViewModel menuItemViewModel5 = new CustomMenuItemViewModel((object) "addSection", Utils.GetString("AddSection"), Utils.GetImageSource("AddDrawingImage"));
        menuItemViewModel5.ShowSelected = false;
        menuItemViewModelList4.Add(menuItemViewModel5);
      }
      if (dataContext.ProjectData != null && dataContext.ProjectData.ShowShare && !dataContext.ProjectData.ShowAssignSort)
      {
        ++num;
        List<CustomMenuItemViewModel> menuItemViewModelList5 = types;
        CustomMenuItemViewModel menuItemViewModel6 = new CustomMenuItemViewModel((object) "share", Utils.GetString("Share"), Utils.GetImageSource("cooperationDrawingImage"));
        menuItemViewModel6.ShowSelected = false;
        menuItemViewModelList5.Add(menuItemViewModel6);
      }
      if (dataContext.ProjectIdentity is NormalProjectIdentity projectIdentity3 && (string.IsNullOrEmpty(projectIdentity3.Project.teamId) || projectIdentity3.Project.permission != "read" && projectIdentity3.Project.permission != "comment"))
      {
        ++num;
        List<CustomMenuItemViewModel> menuItemViewModelList6 = types;
        CustomMenuItemViewModel menuItemViewModel7 = new CustomMenuItemViewModel((object) "listActivities", Utils.GetString("ListActivitiesPro"), Utils.GetImageSource("ProjectActivitiesDrawingImage"));
        menuItemViewModel7.ShowSelected = false;
        menuItemViewModelList6.Add(menuItemViewModel7);
      }
      List<CustomMenuItemViewModel> menuItemViewModelList7 = types;
      CustomMenuItemViewModel menuItemViewModel8 = new CustomMenuItemViewModel((object) "", Utils.GetString("Print"), Utils.GetImageSource("PrintDrawingImage"));
      List<CustomMenuItemViewModel> menuItemViewModelList8 = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel9 = new CustomMenuItemViewModel((object) "print", Utils.GetString("PrintTitle"), (Geometry) null);
      menuItemViewModel9.ImageWidth = 0.0;
      menuItemViewModel9.ShowSelected = false;
      menuItemViewModelList8.Add(menuItemViewModel9);
      CustomMenuItemViewModel menuItemViewModel10 = new CustomMenuItemViewModel((object) "printDetail", Utils.GetString("PrintTitleAndContent"), (Geometry) null);
      menuItemViewModel10.ImageWidth = 0.0;
      menuItemViewModel10.ShowSelected = false;
      menuItemViewModelList8.Add(menuItemViewModel10);
      menuItemViewModel8.SubActions = menuItemViewModelList8;
      menuItemViewModelList7.Add(menuItemViewModel8);
      if (num == 1)
        types.Remove(menuItemViewModel4);
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.PlacementTarget = (UIElement) projectTaskListView._moreButton;
      escPopup.HorizontalOffset = -118.0;
      escPopup.VerticalOffset = -5.0;
      escPopup.PopupAnimation = PopupAnimation.Fade;
      EscPopup popup = escPopup;
      List<string> switchViewModes = dataContext.ProjectIdentity.GetSwitchViewModes();
      SwitchListViewControl topTabControl = switchViewModes != null ? new SwitchListViewControl() : (SwitchListViewControl) null;
      if (switchViewModes != null)
      {
        topTabControl.ViewSelected += (EventHandler<string>) ((o, a) =>
        {
          popup.IsOpen = false;
          this.OnSwitchView(a);
        });
        topTabControl.SetButtonStatus(new bool?(true), switchViewModes.Contains("kanban") ? new bool?(false) : new bool?(), switchViewModes.Contains("timeline") ? new bool?(false) : new bool?());
      }
      popup.Closed += (EventHandler) ((o, arg) => Window.GetWindow((DependencyObject) this)?.Activate());
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) popup, (ITabControl) topTabControl);
      customMenuList.MinWidth = 172.0;
      customMenuList.Operated += (EventHandler<object>) ((o, operation) =>
      {
        popup.IsOpen = false;
        this.OnMoreItemSelected(operation);
      });
      customMenuList.Show();
    }

    private void OnMoreItemSelected(object e)
    {
      if (!(e is string tag) || tag == null)
        return;
      switch (tag.Length)
      {
        case 5:
          switch (tag[0])
          {
            case 'p':
              if (!(tag == "print"))
                return;
              this.OnPrintClick(false);
              return;
            case 's':
              if (!(tag == "share"))
                return;
              break;
            default:
              return;
          }
          break;
        case 6:
          return;
        case 7:
          return;
        case 8:
          return;
        case 9:
          return;
        case 10:
          switch (tag[0])
          {
            case 'a':
              if (!(tag == "addSection"))
                return;
              this.AddSectionClick();
              return;
            case 's':
              if (!(tag == "showDetail"))
                return;
              break;
            default:
              return;
          }
          break;
        case 11:
          if (!(tag == "printDetail"))
            return;
          this.OnPrintClick(true);
          return;
        case 12:
          if (!(tag == "showComplete"))
            return;
          break;
        case 13:
          switch (tag[5])
          {
            case 'h':
              if (!(tag == "showCheckItem"))
                return;
              break;
            case 'o':
              if (!(tag == "showCountDown"))
                return;
              break;
            default:
              return;
          }
          break;
        case 14:
          if (!(tag == "listActivities"))
            return;
          this.ShowProjectActivityClick();
          return;
        default:
          return;
      }
      this.ShowOrHideClick(tag);
    }

    private void OnPrintClick(bool detail)
    {
      this.ShowPrintPreview(detail);
      UserActCollectUtils.AddClickEvent("tasklist", "om", detail ? "print_title_content" : "print_title");
    }

    public async void ShowPrintPreview(bool isDetail)
    {
      List<DisplayItemModel> models = this.ViewModel.Items.ToList<DisplayItemModel>();
      if (!models.Any<DisplayItemModel>())
      {
        models = (List<DisplayItemModel>) null;
      }
      else
      {
        List<TaskModel> tasks = new List<TaskModel>();
        if (isDetail)
          tasks = await TaskDao.GetThinTasksInBatch(models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.Type == DisplayType.Task || model.Type == DisplayType.Agenda || model.Type == DisplayType.Note)).Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (model => model.Id)).ToList<string>());
        TaskListPrintViewModel printModel = new TaskListPrintViewModel()
        {
          ListName = this.ViewModel.ProjectData.GetTitle(),
          IsNormal = this.ViewModel.ProjectData is NormalListData
        };
        foreach (DisplayItemModel displayItemModel in models)
        {
          DisplayItemModel model = displayItemModel;
          TaskPrintViewModel viewModel = new TaskPrintViewModel(model);
          if (isDetail && (model.Type == DisplayType.Task || model.Type == DisplayType.Agenda || model.Type == DisplayType.Note))
          {
            TaskModel taskModel = tasks.FirstOrDefault<TaskModel>((Func<TaskModel, bool>) (t => t.id == model.TaskId));
            viewModel.Content = TaskUtils.ReplaceAttachmentTextInString(taskModel?.content);
            viewModel.Desc = taskModel?.desc;
            if (model.Kind == "CHECKLIST" || model.Type == DisplayType.Agenda)
            {
              List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(model.Id);
              // ISSUE: explicit non-virtual call
              if (checkItemsByTaskId != null && __nonvirtual (checkItemsByTaskId.Count) > 0)
              {
                checkItemsByTaskId.Sort((Comparison<TaskDetailItemModel>) ((a, b) =>
                {
                  if (a.status != 0 && b.status == 0)
                    return 1;
                  if (a.status == 0 && b.status != 0)
                    return -1;
                  return a.status != 0 && b.status != 0 && a.completedTime.HasValue && b.completedTime.HasValue ? b.completedTime.Value.CompareTo(a.completedTime.Value) : a.sortOrder.CompareTo(b.sortOrder);
                }));
                viewModel.SubtaskPrintViewModels = checkItemsByTaskId.Select<TaskDetailItemModel, SubtaskPrintViewModel>((Func<TaskDetailItemModel, SubtaskPrintViewModel>) (i => new SubtaskPrintViewModel(i))).ToList<SubtaskPrintViewModel>();
              }
            }
          }
          if (isDetail && model.Type == DisplayType.Event)
            viewModel.Content = model.Content;
          printModel.TaskPrintViewModels.Add(viewModel);
          viewModel = (TaskPrintViewModel) null;
        }
        PrintPreviewWindow window = new PrintPreviewWindow(printModel, isDetail);
        await Task.Delay(300);
        window.Show();
        window.Activate();
        tasks = (List<TaskModel>) null;
        printModel = (TaskListPrintViewModel) null;
        window = (PrintPreviewWindow) null;
        models = (List<DisplayItemModel>) null;
      }
    }

    private void ShowProjectActivityClick()
    {
      if (!(this.DataContext is TaskListViewModel dataContext) || !(dataContext.ProjectIdentity is NormalProjectIdentity projectIdentity))
        return;
      UserActCollectUtils.AddClickEvent("tasklist", "om", "project_activities");
      EventHandler<string> projectActivityClick = this.OnProjectActivityClick;
      if (projectActivityClick == null)
        return;
      projectActivityClick((object) this, projectIdentity.Project.id);
    }

    private void AddSectionClick()
    {
      UserActCollectUtils.AddClickEvent("tasklist", "om", "add_section");
      if (!(this.DataContext is TaskListViewModel dataContext) || !(dataContext.SortOption.groupBy == Constants.SortType.sortOrder.ToString()))
        return;
      this._taskList.AddNewSectionAtLast();
      this._listEmpty.Visibility = Visibility.Collapsed;
    }

    public void TryReloadProject(string projectId)
    {
      if (!(this.ViewModel?.ProjectIdentity.Id == projectId))
        return;
      this.ReloadData(true);
    }

    private void ShowOrHideClick(string tag)
    {
      switch (tag)
      {
        case "share":
          if (this.DataContext is TaskListViewModel dataContext && dataContext.ProjectIdentity.GetProjectId() != null)
            this.ShareProject(dataContext.ProjectIdentity.GetProjectId());
          UserActCollectUtils.AddClickEvent("tasklist", "om", "collaboration");
          break;
        case "showComplete":
          LocalSettings.Settings.HideComplete = !LocalSettings.Settings.HideComplete;
          ProjectWidgetsHelper.Reload();
          LocalSettings.Settings.Save();
          SettingsHelper.PushLocalSettings();
          UserActCollectUtils.AddClickEvent("tasklist", "om", "show_hide_completed");
          UtilLog.Info(string.Format("TaskList.Om ShowComplete {0}", (object) !LocalSettings.Settings.HideComplete));
          break;
        case "showCheckItem":
          LocalSettings.Settings.ShowSubtasks = !LocalSettings.Settings.ShowSubtasks;
          LocalSettings.Settings.NotifyPropertyChanged("ProjectNum");
          ProjectWidgetsHelper.Reload();
          LocalSettings.Settings.Save();
          SettingsHelper.PushLocalSettings();
          UserActCollectUtils.AddClickEvent("tasklist", "om", "show_hide_checklist");
          UtilLog.Info(string.Format("TaskList.Om ShowCheckItem {0}", (object) LocalSettings.Settings.ShowSubtasks));
          break;
        case "showDetail":
          LocalSettings.Settings.ShowDetails = !LocalSettings.Settings.ShowDetails;
          UserActCollectUtils.AddClickEvent("tasklist", "om", "show_hide_details");
          break;
        case "showCountDown":
          UserActCollectUtils.AddClickEvent("tasklist", "om", "show_hide_countdown");
          LocalSettings.Settings.ShowCountDown = !LocalSettings.Settings.ShowCountDown;
          break;
      }
    }

    private void ShareProject(string projectId)
    {
      ShareProjectDialog.TryShowShareDialog(projectId, Window.GetWindow((DependencyObject) this));
    }

    private async void OnSwitchView(string e)
    {
      string str = e;
      if (!(str == "kanban") && !(str == "timeline"))
        return;
      UserActCollectUtils.AddClickEvent("list_mode", "switch", e);
      if (e == "timeline" && !ProChecker.CheckPro(ProType.TimeLine, (Window) App.Window) || this.ViewModel.ProjectIdentity == null)
        return;
      await this.ViewModel.ProjectIdentity?.SwitchViewMode(e);
    }

    public void SetFoldMenuIcon(bool hideMenu)
    {
      if (!(this._menuFoldBorder.Child is Image child))
        return;
      DependencyProperty sourceProperty = Image.SourceProperty;
      string name = hideMenu ? "ShowMenuDrawingImage" : "HideMenuDrawingImage";
      child.SetResourceReference(sourceProperty, (object) name);
    }

    public void HideFoldMenuIcon()
    {
      this._hideFoldMenu = true;
      this._menuFoldBorder.Visibility = Visibility.Collapsed;
      this._titlePanel.Margin = new Thickness(20.0, 27.0, 0.0, 12.0);
      this._trashTitle.Margin = new Thickness(22.0, 29.0, 0.0, 12.0);
    }

    public DisplayItemModel SetSelectedId(string id, bool scroll = false)
    {
      if (this._taskList != null)
      {
        this._taskList.SelectedId = id;
        if (scroll)
          this._taskList?.ScrollToItemById(id);
      }
      return this.ViewModel.SetSelectedTaskId(id);
    }

    public void OnDetailTaskNavigated(string taskId)
    {
      this._taskList?.OnDetailTaskNavigated(taskId);
    }

    public void BatchSelectOnMove(bool up) => this._taskList?.BatchSelectOnMove(up);

    public void TryFocusSelectedItem() => this._taskList?.TryFocusSelectedItem();

    public IEnumerable<DisplayItemModel> GetItems()
    {
      return this.DataContext is TaskListViewModel dataContext ? (IEnumerable<DisplayItemModel>) dataContext.Items : (IEnumerable<DisplayItemModel>) null;
    }

    public void ResetSearchFilterControl() => this._searchFilterControl?.Reset();

    public void ReSearch()
    {
      if (!(this.ViewModel.ProjectIdentity is SearchProjectIdentity))
        return;
      this.ReloadData(true);
    }

    private async void SearchFilterChanged(object sender, EventArgs e)
    {
      ProjectTaskListView projectTaskListView = this;
      if (!(projectTaskListView.DataContext is TaskListViewModel dataContext) || !(dataContext.ProjectIdentity is SearchProjectIdentity))
        return;
      await projectTaskListView.LoadProject((ProjectIdentity) new SearchProjectIdentity());
    }

    public async Task ForbidShowMenu()
    {
      this._canHoverShow = false;
      await Task.Delay(300);
      this._canHoverShow = true;
    }

    private void OnShowMenuMouseEnter(object sender, MouseEventArgs e)
    {
      if (!this._canHoverShow)
        return;
      Utils.FindParent<ListViewContainer>((DependencyObject) this)?.TryShowMenuOnHover((UIElement) this.MenuPathBorder);
    }

    private async void MenuBorderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      ProjectTaskListView child = this;
      e.Handled = true;
      await Task.Delay(1);
      Utils.FindParent<ListViewContainer>((DependencyObject) child)?.ShowProjectMenu();
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      Utils.FindParent<ListViewContainer>((DependencyObject) this)?.HideProjectMenu();
    }

    public void ReloadDataAndQuickAddView()
    {
      if (!this.IsVisible)
        return;
      this.LoadQuickAddView(true, true);
      this.ReloadData();
    }

    public void OnTaskCopied(string copyId) => this._taskList.SelectedId = copyId;

    public void ReloadOnBatchTaskChanged(object sender, List<string> taskIds) => this.ReloadData();

    private QuickAddView GetQuickAddView() => this._addTaskContent.Content as QuickAddView;

    public async Task ReloadData(bool forceReload = false, string taskId = "", bool forceFocus = true)
    {
      TaskListView taskList = this._taskList;
      if (taskList == null)
      {
        await Task.Delay(100);
        taskList = this._taskList;
        if (taskList == null)
          return;
      }
      if (!string.IsNullOrEmpty(taskId))
        taskList.SelectedId = taskId;
      taskList.LoadAsync(forceReload, ignoreFocus: true, setSelect: forceFocus);
    }

    public bool ExistTask(string taskId)
    {
      return this.DataContext is TaskListViewModel dataContext && dataContext.SourceModels != null && dataContext.SourceModels.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.Id == taskId));
    }

    public bool CanPrint()
    {
      if (!(this.DataContext is TaskListViewModel dataContext))
        return false;
      switch (dataContext.ProjectIdentity)
      {
        case TrashProjectIdentity _:
        case SearchProjectIdentity _:
        case AbandonedProjectIdentity _:
          return false;
        case NormalProjectIdentity normalProjectIdentity:
          return normalProjectIdentity.Project.IsValid();
        default:
          return true;
      }
    }

    private void ShowPrintPopup(object sender, MouseButtonEventArgs e)
    {
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      types.Add(new CustomMenuItemViewModel((object) "print", Utils.GetString("PrintTitle"), (DrawingImage) null)
      {
        ImageWidth = 0.0
      });
      types.Add(new CustomMenuItemViewModel((object) "printDetail", Utils.GetString("PrintTitleAndContent"), (DrawingImage) null)
      {
        ImageWidth = 0.0
      });
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.PlacementTarget = (UIElement) this._moreButton;
      escPopup.HorizontalOffset = -5.0;
      escPopup.VerticalOffset = -5.0;
      EscPopup popup = escPopup;
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) popup);
      customMenuList.Operated += (EventHandler<object>) ((o, operation) =>
      {
        popup.IsOpen = false;
        this.OnMoreItemSelected(operation);
      });
      customMenuList.Show();
    }

    public async Task TryBatchSetPriority(int priority)
    {
      int num = await TaskService.BatchSetPriority(this._taskList.GetSelectedTaskIds(), priority) ? 1 : 0;
    }

    public void BatchOpenSticky()
    {
      TaskStickyWindow.ShowTaskSticky(this._taskList.GetSelectedTaskIds());
    }

    public async Task TryBatchSetDate(DateTime? date)
    {
      int num = await TaskService.BatchSetDate(this._taskList.GetSelectedTaskIds(), date) ? 1 : 0;
    }

    public async Task BatchPinTask()
    {
      List<string> selectedTaskIds = this._taskList.GetSelectedTaskIds();
      List<TaskBaseViewModel> tasksByIds = TaskCache.GetTasksByIds(selectedTaskIds);
      await TaskService.BatchStarTaskOrNote(selectedTaskIds, this.ViewModel?.ProjectIdentity?.CatId, tasksByIds.Count <= 0 || !tasksByIds.All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.IsPinned)));
    }

    public void DeleteSelectedTasks() => this._taskList.DeleteSelectedTasks();

    public void ExpandOrFoldAllTask(bool isOpen) => this._taskList.ExpandOrFoldAllTask();

    public void ExpandOrFoldAllSection() => this._taskList.ExpandOrFoldAllSection();

    public void TabSelectItem()
    {
      if (!(this._addTaskContent.Content is QuickAddView content))
        return;
      content.TabSelectItem();
    }

    public void RemoveTabSelect() => this.GetQuickAddView()?.ClearTabSelect();

    private void InitSearchData()
    {
      if (this._searchFilterControl == null)
      {
        SearchFilterControl searchFilterControl = new SearchFilterControl();
        searchFilterControl.Margin = new Thickness(0.0, 15.0, 20.0, 0.0);
        this._searchFilterControl = searchFilterControl;
        this._searchFilterControl.SetValue(Grid.RowProperty, (object) 0);
        this.Children.Add((UIElement) this._searchFilterControl);
        this._searchFilterControl.FilterChanged += new EventHandler(this.SearchFilterChanged);
      }
      this.Children.Remove((UIElement) this._abandonFilterControl);
      this._abandonFilterControl = (CompletedFilterControl) null;
      this.Children.Remove((UIElement) this._completeFilterControl);
      this._completeFilterControl = (CompletedFilterControl) null;
    }

    private void InitCompletedData()
    {
      if (this._completeFilterControl == null)
      {
        CompletedFilterControl completedFilterControl = new CompletedFilterControl();
        completedFilterControl.Margin = new Thickness(10.0, -12.0, 0.0, 5.0);
        this._completeFilterControl = completedFilterControl;
        this._completeFilterControl.SetValue(Grid.RowProperty, (object) 1);
        this.Children.Add((UIElement) this._completeFilterControl);
        this._completeFilterControl.FilterChanged += new EventHandler<ClosedFilterViewModel>(this.ClosedFilterChanged);
      }
      this.Children.Remove((UIElement) this._abandonFilterControl);
      this._abandonFilterControl = (CompletedFilterControl) null;
      this.Children.Remove((UIElement) this._searchFilterControl);
      this._searchFilterControl = (SearchFilterControl) null;
    }

    private void InitAbandonedData()
    {
      if (this._abandonFilterControl == null)
      {
        CompletedFilterControl completedFilterControl = new CompletedFilterControl(true);
        completedFilterControl.Margin = new Thickness(10.0, -12.0, 0.0, 5.0);
        this._abandonFilterControl = completedFilterControl;
        this._abandonFilterControl.SetValue(Grid.RowProperty, (object) 1);
        this.Children.Add((UIElement) this._abandonFilterControl);
        this._abandonFilterControl.FilterChanged += new EventHandler<ClosedFilterViewModel>(this.ClosedFilterChanged);
      }
      this.Children.Remove((UIElement) this._completeFilterControl);
      this._completeFilterControl = (CompletedFilterControl) null;
      this.Children.Remove((UIElement) this._searchFilterControl);
      this._searchFilterControl = (SearchFilterControl) null;
    }

    private void ClosedFilterChanged(object sender, ClosedFilterViewModel filter)
    {
      if (this.ViewModel?.ProjectData is CompleteListData)
      {
        CompletedProjectIdentity.Filter = filter;
        ClosedTaskWithFilterLoader.CompletionLoader.Reset();
        this.ReloadData();
      }
      if (!(this.ViewModel?.ProjectData is AbandonedListData))
        return;
      AbandonedProjectIdentity.Filter = filter;
      ClosedTaskWithFilterLoader.AbandonedLoader.Reset();
      this.ReloadData();
    }

    public bool GetQuickAddPosition()
    {
      QuickAddView quickAddView = this.GetQuickAddView();
      if (!quickAddView.IsVisible)
        return false;
      quickAddView?.FocusText();
      return true;
    }

    private void HideTemplateGuide(object sender, MouseButtonEventArgs e)
    {
      this.HideTemplateGuide();
    }

    private void OnHideGuide(object sender, EventArgs e) => this.HideTemplateGuide();

    private void HideTemplateGuide()
    {
    }

    public void ShowBatchOperationDialog() => this._batchHelper.ShowOperationDialog();

    public void SetSelectedTaskIds(List<string> taskIds)
    {
      this._batchHelper.SelectedTaskIds = taskIds;
    }

    public void RemoveSelectedId(string id)
    {
      this._batchHelper.SelectedTaskIds?.Remove(id);
      this._taskList?.RemoveSelectedId(id);
    }

    public List<string> GetSelectedTaskIds() => this._batchHelper.SelectedTaskIds;

    public void ReloadList() => this.ReloadData();

    public UIElement BatchOperaPlacementTarget() => (UIElement) this._titlePanel;

    public DisplayItemModel GetSelectedItem() => this.ViewModel.GetSelectedItem();

    public void Dispose()
    {
      this.ViewModel.Title = string.Empty;
      this._parent = (TaskView) null;
      this.TaskSelect = (TaskSelectDelegate) null;
      this.EventAdded = (EventHandler<string>) null;
      this.BatchTaskSelected = (EventHandler<List<string>>) null;
      this.TaskDragging = (EventHandler<DragMouseEvent>) null;
      this.BatchTaskDrop = (EventHandler<List<string>>) null;
      this.TaskAdded = (EventHandler<TaskModel>) null;
      this.OnSearchFilterChanged = (EventHandler) null;
      this.BatchTaskChanged = (EventHandler<List<string>>) null;
      this.TaskDrop = (EventHandler<string>) null;
      this.OnProjectActivityClick = (EventHandler<string>) null;
      this.Children.Clear();
      this._taskList?.Dispose();
      this._taskList = (TaskListView) null;
    }

    public void OnCheckItemDragDrop(string id) => this._taskList.OnCheckItemDrop(id);

    public string GetSelectedId() => this._taskList.SelectedId;

    public bool InSearch()
    {
      SearchFilterControl searchFilterControl = this._searchFilterControl;
      return searchFilterControl != null && searchFilterControl.Visibility == Visibility.Visible;
    }

    public void ReloadIdentity()
    {
      this.ViewModel?.SetIdentity(this.ViewModel.ProjectIdentity.Copy(this.ViewModel.ProjectIdentity));
      this._taskList?.LoadAsync();
    }

    public void ToggleSelectedTaskCompleted() => this._taskList?.ToggleSelectedItemCompleted();
  }
}
