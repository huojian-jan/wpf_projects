// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.ProjectListView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
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
using System.Windows.Media.Effects;
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
using ticktick_WPF.Util.Drag;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Filter;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.Widget;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class ProjectListView : Grid
  {
    private double _dragStartOffset;
    private DragStatus _dragStatus;
    private string _duplicateProjectId;
    private bool _canReload;
    private readonly ContentControl _pinViewContent;
    private PtfType _ptfType;
    private readonly ListView _projectList;
    private readonly Border _shadowBorder;
    private readonly Border _backBorder;
    private readonly MiniCalendar _miniCalendar;
    public ProjectItemViewModel SelectedItem;
    private static List<SyncSortOrderModel> _cache;
    private string _uid;
    private int _dragStartIndex;
    private int _currentIndex;
    private string _currentIdentity;
    private DragDirection _direction;
    private long _dragStartY;
    private ProjectItemViewModel _dragStartTarget;
    private ProjectItemViewModel _draggingModel;
    private int _hoverIndex;
    private ListBoxItem _dragHoverItem;
    private ProjectItemViewModel _hoverModel;
    private EmptySubViewModel _lastHoverLine;
    private ScrollViewer _listScrollViewer;
    private Popup _projectDragPopup;
    public bool TabSelected;
    private int _lastDropIndex;
    private ProjectItemViewModel _previousOverGroup;

    public event EventHandler ProjectSelected;

    private ObservableCollection<ProjectItemViewModel> ProjectItems
    {
      get
      {
        return this._projectList.ItemsSource is ObservableCollection<ProjectItemViewModel> itemsSource ? itemsSource : new ObservableCollection<ProjectItemViewModel>();
      }
    }

    public PtfType CurrentPtfType => this._ptfType;

    public ProjectListView()
    {
      ContentControl contentControl = new ContentControl();
      contentControl.MinHeight = 4.0;
      this._pinViewContent = contentControl;
      this._projectList = new ListView();
      this._shadowBorder = new Border();
      Border border = new Border();
      border.IsHitTestVisible = false;
      border.BorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);
      this._backBorder = border;
      this._miniCalendar = new MiniCalendar();
      this._dragStartIndex = -1;
      this._currentIndex = -1;
      this._currentIdentity = string.Empty;
      this._direction = DragDirection.Idle;
      this._dragStartY = -1L;
      this._hoverIndex = -1;
      EscPopup escPopup = new EscPopup();
      escPopup.HorizontalOffset = 10.0;
      escPopup.StaysOpen = true;
      escPopup.Placement = PlacementMode.Relative;
      this._projectDragPopup = (Popup) escPopup;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this._uid = Utils.GetGuid();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition());
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.InitBackground();
      this.Children.Add((UIElement) this._pinViewContent);
      this.InitProjectList();
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.MenuMouseDown);
      this.InitShadowBorder();
      this.Unloaded += (RoutedEventHandler) ((o, e) => this.UnbindEvents());
      this.Children.Add((UIElement) this._projectDragPopup);
      this.InitMiniCalendar();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      ProjectListView projectListView = this;
      projectListView.BindEvents();
      if (projectListView._projectList.Template.FindName("ScrollViewer", (FrameworkElement) projectListView._projectList) is ScrollViewer name)
      {
        name.ScrollChanged += new ScrollChangedEventHandler(projectListView.OnScrollChanged);
        name.PreviewMouseWheel += new MouseWheelEventHandler(projectListView.OnPreviewMouseWheel);
      }
      projectListView.Reload(true);
    }

    private void InitBackground()
    {
      this._backBorder.SetValue(Grid.RowSpanProperty, (object) 2);
      this._backBorder.SetResourceReference(Panel.BackgroundProperty, (object) "ProjectMenuBackGround");
      this.Children.Add((UIElement) this._backBorder);
      Line line = new Line();
      line.Y1 = 0.0;
      line.Y2 = 1.0;
      line.Stretch = Stretch.Fill;
      line.StrokeThickness = 1.0;
      line.HorizontalAlignment = HorizontalAlignment.Right;
      Line element = line;
      element.SetValue(Grid.RowSpanProperty, (object) 3);
      element.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
      this.Children.Add((UIElement) element);
    }

    private void InitProjectList()
    {
      this._projectList.SetValue(Grid.RowProperty, (object) 1);
      this._projectList.Margin = new Thickness(0.0, 16.0, 0.0, 10.0);
      this._projectList.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListViewStyle");
      this._projectList.SetResourceReference(ItemsControl.ItemContainerStyleProperty, (object) "ListViewItemContainerStyle");
      this._projectList.ItemTemplateSelector = (DataTemplateSelector) new ProjectItemTemplateSelector();
      this.Children.Add((UIElement) this._projectList);
    }

    private void InitShadowBorder()
    {
      this._shadowBorder.SetValue(Grid.RowProperty, (object) 1);
      this._shadowBorder.Height = 80.0;
      this._shadowBorder.IsHitTestVisible = false;
      this._shadowBorder.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
      this._shadowBorder.Visibility = Visibility.Collapsed;
      this._shadowBorder.VerticalAlignment = VerticalAlignment.Top;
      this._shadowBorder.SetResourceReference(UIElement.OpacityProperty, (object) "ProjectShadowOpacity");
      this._shadowBorder.Background = (Brush) this.FindResource((object) "ProjectTopShadowBrush");
      this._shadowBorder.RenderTransform = (Transform) new TranslateTransform()
      {
        Y = -6.0
      };
      this.Children.Add((UIElement) this._shadowBorder);
    }

    private void InitMiniCalendar()
    {
      this._miniCalendar.SetValue(Grid.RowProperty, (object) 2);
      this.Children.Add((UIElement) this._miniCalendar);
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnThemeModeChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnThemeModeChanged);
    }

    private void OnThemeModeChanged(object sender, EventArgs e)
    {
      this._miniCalendar.DayPicker.Reload();
    }

    private ProjectPinView GetPinView(bool create = false)
    {
      ProjectPinView pinView;
      if (this._pinViewContent.Content == null & create)
      {
        ProjectPinView projectPinView = new ProjectPinView();
        projectPinView.Visibility = Visibility.Collapsed;
        projectPinView.Margin = new Thickness(12.0, 16.0, 0.0, 0.0);
        pinView = projectPinView;
        this._pinViewContent.Content = (object) pinView;
      }
      else
        pinView = (ProjectPinView) this._pinViewContent.Content;
      return pinView;
    }

    public void SetProjectFoldBackground(bool show)
    {
      if (show)
      {
        Border border = new Border();
        border.ClipToBounds = true;
        border.SetResourceReference(Panel.BackgroundProperty, (object) "ShowProjectBackground");
        border.Effect = (Effect) new DropShadowEffect()
        {
          Color = Colors.Black,
          Opacity = 0.3,
          ShadowDepth = 4.0,
          Direction = 340.0,
          BlurRadius = 10.0
        };
        if (MainWindowManager.BackImageSource != null)
        {
          Image image1 = new Image();
          image1.HorizontalAlignment = HorizontalAlignment.Left;
          image1.VerticalAlignment = VerticalAlignment.Bottom;
          image1.Source = MainWindowManager.BackImageSource;
          image1.Stretch = Stretch.UniformToFill;
          Image image2 = image1;
          double num = (double) LocalSettings.Settings.ThemeImageBlurRadius / -2.0;
          image2.Margin = new Thickness(num - 50.0, num, num, num);
          image2.Effect = (Effect) new BlurEffect()
          {
            Radius = (double) LocalSettings.Settings.ThemeImageBlurRadius
          };
          border.Child = (UIElement) image2;
        }
        this._backBorder.Child = (UIElement) border;
      }
      else
        this._backBorder.Child = (UIElement) null;
    }

    private void UnbindEvents()
    {
      DragEventManager.CheckItemDragEvent -= new EventHandler<DragMouseEvent>(this.OnItemDragging);
      DragEventManager.CheckItemDrop -= new EventHandler<string>(this.OnCheckItemDrop);
      DataChangedNotifier.HabitCheckInChanged -= new EventHandler<HabitCheckInModel>(this.OnHabitCheckInChanged);
      DataChangedNotifier.TagChanged -= new EventHandler<TagModel>(this.OnTagChanged);
      DataChangedNotifier.TagTypeChanged -= new EventHandler(this.OnTagTypeChanged);
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      DataChangedNotifier.ProjectPinRemoteChanged -= new EventHandler(this.PinChanged);
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnNumDisplayChanged), "ProjectNum");
    }

    private void BindEvents()
    {
      DragEventManager.CheckItemDragEvent += new EventHandler<DragMouseEvent>(this.OnItemDragging);
      DragEventManager.CheckItemDrop += new EventHandler<string>(this.OnCheckItemDrop);
      DataChangedNotifier.HabitCheckInChanged += new EventHandler<HabitCheckInModel>(this.OnHabitCheckInChanged);
      DataChangedNotifier.TagChanged += new EventHandler<TagModel>(this.OnTagChanged);
      DataChangedNotifier.TagTypeChanged += new EventHandler(this.OnTagTypeChanged);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      DataChangedNotifier.ProjectPinRemoteChanged += new EventHandler(this.PinChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnNumDisplayChanged), "ProjectNum");
    }

    private void OnNumDisplayChanged(object sender, PropertyChangedEventArgs e)
    {
      this.LoadData(false);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      ProjectMenuItemHoverModel.Model.HoverType = PtfType.Null;
      ProjectMenuItemHoverModel.Model.TeamId = string.Empty;
    }

    private void OnFilterChanged(object sender, FilterChangeArgs e)
    {
      this.LoadData(false);
      this.LoadPinModels();
    }

    private void OnTagTypeChanged(object sender, EventArgs e)
    {
      if (!LocalSettings.Settings.AllTagOpened)
        return;
      this.LoadData(false);
    }

    private void OnTagChanged(object sender, TagModel e)
    {
      if (this._ptfType == PtfType.Tag && e != null && this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (item => item is TagProjectViewModel projectViewModel1 && projectViewModel1.TagModel.id == e.id)) is TagProjectViewModel projectViewModel2)
        projectViewModel2.TagModel = e;
      this.LoadPinModels();
    }

    private void OnHabitCheckInChanged(object sender, HabitCheckInModel e)
    {
      if (!(e.CheckinStamp == DateTime.Today.ToString("yyyyMMdd")))
        return;
      this.ReloadTodayOrWeekCount();
    }

    private void PinChanged(object sender, EventArgs e)
    {
      this.Dispatcher.Invoke<Task>(new Func<Task>(this.LoadPinModels));
    }

    private void ReloadTodayOrWeekCount() => this.LoadTodayOrWeekCount();

    public SmartListType? GetSelectedSmartListType()
    {
      if (!(this.SelectedItem is ticktick_WPF.ViewModels.SmartProjectViewModel selectedItem))
        return new SmartListType?();
      string str = selectedItem.Project.GetType().Name.Replace("Project", "");
      return str == "Inbox" ? new SmartListType?() : new SmartListType?((SmartListType) Enum.Parse(typeof (SmartListType), str));
    }

    public void SelectProject(ProjectIdentity projectId, bool invokeEvent = true)
    {
      this.SelectedItem = ProjectItemViewModel.BuildProject(projectId);
      this.SetSelectedItemHighlighted();
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == projectId.QueryId));
      if (projectItemViewModel != null)
        this._projectList.ScrollIntoView((object) projectItemViewModel);
      if (!invokeEvent)
        return;
      EventHandler projectSelected = this.ProjectSelected;
      if (projectSelected == null)
        return;
      projectSelected((object) this, (EventArgs) null);
    }

    public async void LoadTaskCount()
    {
      await Task.Delay(100);
      TaskCountCache.SetNeedLoad();
      this.LoadData(false);
    }

    private async void LoadTodayOrWeekCount()
    {
      bool loaded = false;
      Task.Run((Func<Task>) (async () =>
      {
        await TaskCountCache.LoadTodayOrWeekCount();
        loaded = true;
      }));
      int retry = 0;
      while (!loaded)
      {
        if (retry < 10)
        {
          await Task.Delay(50);
          ++retry;
        }
        else
          break;
      }
    }

    public async Task LoadData(bool refreshCount = true)
    {
      DelayActionHandlerCenter.TryDoAction(this._uid + "ProjectMenuLoadData", (EventHandler) ((o, e) => ThreadUtil.DetachedRunOnUiThread((Action) (async () =>
      {
        if (App.IsProjectOrGroupDragging || !this._canReload || this._projectDragPopup.IsOpen)
          return;
        ItemsSourceHelper.SetItemsSource<ProjectItemViewModel>((ItemsControl) this._projectList, await ProjectDataProvider.GetProjectData());
        this.InitSelected();
        if (refreshCount)
          this.ForceLoadCount();
        this.LoadPinModels();
        if (!LocalSettings.Settings.ExtraSettings.MiniCalendarEnabled)
          return;
        this._miniCalendar.LoadIndicator();
      }))));
    }

    public async Task LoadPinModels()
    {
      if (this._pinViewContent.Content == null)
      {
        List<SyncSortOrderModel> cache = ProjectListView._cache;
        // ISSUE: explicit non-virtual call
        if ((cache != null ? (__nonvirtual (cache.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          List<ProjectPinItemViewModel> data = ProjectPinItemViewModel.BuildModels(ProjectListView._cache);
          this.GetPinView(true).SetData(data);
        }
      }
      List<SyncSortOrderModel> async = await ProjectPinSortOrderService.GetAsync();
      ProjectListView._cache = async;
      if (async.Count > 0)
      {
        async.Sort((Comparison<SyncSortOrderModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
        List<ProjectPinItemViewModel> data = ProjectPinItemViewModel.BuildModels(async);
        if (data.Count > 0)
        {
          ProjectPinView pinView = this.GetPinView(true);
          pinView.SetData(data);
          if (pinView.IsEmpty())
            this._pinViewContent.Content = (object) null;
          else
            pinView.Visibility = Visibility.Visible;
        }
        else
          this._pinViewContent.Content = (object) null;
      }
      else
        this._pinViewContent.Content = (object) null;
      this._projectList.Margin = new Thickness(0.0, this._pinViewContent.Content == null ? 16.0 : 0.0, 0.0, 10.0);
    }

    private void LoadProjectList(bool refreshCount)
    {
      DelayActionHandlerCenter.TryDoAction("ProjectMenuLoadData", (EventHandler) ((o, e) => ThreadUtil.DetachedRunOnUiThread((Action) (async () =>
      {
        if (!this._canReload || this.ProjectDragging)
          return;
        this.Dispatcher.Invoke<Task>((Func<Task>) (async () => this.Reload(refreshCount)));
      }))));
    }

    private async Task Reload(bool refreshCount)
    {
      this.LoadPinModels();
      ItemsSourceHelper.SetItemsSource<ProjectItemViewModel>((ItemsControl) this._projectList, await ProjectDataProvider.GetProjectData());
      this.InitSelected();
      if (!refreshCount)
        return;
      this.ForceLoadCount();
    }

    public async void ForceLoadCount()
    {
      if (!this.TryRemoveAutoList() && !this.TryRemoveAutoTags())
        return;
      this.LoadData(false);
    }

    private bool TryRemoveAutoTags()
    {
      if (LocalSettings.Settings.SmartListTag == 2)
      {
        foreach (TagModel tag in CacheManager.GetTags())
        {
          if (ProjectListView.AutoCountChanged((IEnumerable<ProjectItemViewModel>) this.ProjectItems, tag.name))
            return true;
        }
      }
      return false;
    }

    private bool TryRemoveAutoList()
    {
      return LocalSettings.Settings.SmartListToday == 2 && ProjectListView.AutoCountChanged((IEnumerable<ProjectItemViewModel>) this.ProjectItems, "_special_id_today") || LocalSettings.Settings.SmartListTomorrow == 2 && ProjectListView.AutoCountChanged((IEnumerable<ProjectItemViewModel>) this.ProjectItems, "_special_id_tomorrow") || LocalSettings.Settings.SmartList7Day == 2 && ProjectListView.AutoCountChanged((IEnumerable<ProjectItemViewModel>) this.ProjectItems, "_special_id_week") || LocalSettings.Settings.SmartListForMe == 2 && ProjectListView.AutoCountChanged((IEnumerable<ProjectItemViewModel>) this.ProjectItems, "_special_id_assigned");
    }

    private static bool AutoCountChanged(IEnumerable<ProjectItemViewModel> projects, string key)
    {
      if (TaskCountCache.CountData.ContainsKey(key))
      {
        int num = TaskCountCache.CountData[key];
        ProjectItemViewModel projectItemViewModel = projects.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (project => project.Id == key));
        if (projectItemViewModel == null && num > 0 || num == 0 && projectItemViewModel != null)
          return true;
      }
      return false;
    }

    public async Task OnSyncFinished(SyncResult syncResult)
    {
      ProjectListView projectListView = this;
      if (projectListView.SelectedItem == null)
        return;
      ListViewContainer parent = Utils.FindParent<ListViewContainer>((DependencyObject) projectListView);
      if ((parent != null ? (parent.Mode == ListMode.Search ? 1 : 0) : 0) != 0)
        return;
      if (syncResult.RemoteProjectsChanged || syncResult.RemoteFiltersChanged || syncResult.TeamChanged || syncResult.RemoteGroupChanged || syncResult.RemoteTagChanged)
        projectListView.LoadData();
      if (syncResult.RemoteProjectsChanged || syncResult.RemoteFiltersChanged || syncResult.RemoteTagChanged)
      {
        if (projectListView.IsSelectedProjectChanged((IReadOnlyCollection<ProjectModel>) syncResult.UpdatedProjects))
        {
          if (projectListView.SelectedItem is NormalProjectViewModel projectVm)
          {
            ProjectModel projectById = await ProjectDao.GetProjectById(projectVm.Id);
            if (projectById != null && !projectById.IsEquals(projectVm.Project))
              projectListView.SelectedItem = (ProjectItemViewModel) new NormalProjectViewModel(projectById);
          }
          projectVm = (NormalProjectViewModel) null;
        }
        else if (projectListView.IsSelectedFilterChanged((IReadOnlyCollection<FilterModel>) syncResult.UpdatedFilters))
        {
          if (projectListView.SelectedItem is FilterProjectViewModel selectedItem1)
          {
            FilterModel filterById = CacheManager.GetFilterById(selectedItem1.Id);
            if (filterById != null)
              projectListView.SelectedItem = (ProjectItemViewModel) new FilterProjectViewModel(filterById);
          }
        }
        else if (projectListView.IsSelectedTagChanged(syncResult.UpdatedTags))
        {
          if (projectListView.SelectedItem is TagProjectViewModel selectedItem2)
          {
            TagModel tagByName = CacheManager.GetTagByName(selectedItem2.Id.ToLower());
            if (tagByName != null)
            {
              projectListView.SelectedItem = (ProjectItemViewModel) new TagProjectViewModel(tagByName);
              EventHandler projectSelected = projectListView.ProjectSelected;
              if (projectSelected != null)
                projectSelected((object) projectListView, (EventArgs) null);
            }
          }
        }
        else if (projectListView.IsSelectedTagDeleted(syncResult.DeletedTags))
          projectListView.SetAndSelectDefaultProject();
      }
      if (!syncResult.RemoteDataChanged)
        return;
      Utils.FindParent<ListViewContainer>((DependencyObject) projectListView)?.ReloadView();
    }

    private bool IsSelectedTagDeleted(List<TagModel> deletedTags)
    {
      return !string.IsNullOrEmpty(this.SelectedItem?.Id) && deletedTags != null && deletedTags.Any<TagModel>((Func<TagModel, bool>) (tag => tag.name == this.SelectedItem.Id.ToLower()));
    }

    private bool IsSelectedTagChanged(List<TagModel> tags)
    {
      return !string.IsNullOrEmpty(this.SelectedItem?.Id) && tags != null && tags.Any<TagModel>((Func<TagModel, bool>) (tag => tag.name == this.SelectedItem.Id.ToLower()));
    }

    private bool IsSelectedProjectChanged(IReadOnlyCollection<ProjectModel> updatedProjects)
    {
      return !string.IsNullOrEmpty(this.SelectedItem?.Id) && updatedProjects != null && updatedProjects.Count > 0 && updatedProjects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == this.SelectedItem.Id)) != null;
    }

    private bool IsSelectedFilterChanged(IReadOnlyCollection<FilterModel> updatedFilters)
    {
      if (this.SelectedItem != null)
      {
        FilterProjectViewModel filterProject = this.SelectedItem as FilterProjectViewModel;
        if (filterProject != null && updatedFilters != null && updatedFilters.Count > 0)
          return updatedFilters.FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == filterProject.Id)) != null;
      }
      return false;
    }

    private void InitSelected()
    {
      if (this.SelectedItem == null)
      {
        ListViewContainer parent = Utils.FindParent<ListViewContainer>((DependencyObject) this);
        if ((parent != null ? (parent.Mode != 0 ? 1 : 0) : 1) == 0)
          return;
        this.LoadSavedProject();
      }
      else
        this.SetSelectedItemHighlighted();
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (!(sender is ScrollViewer scrollViewer) || this.GetPinView() == null)
        return;
      if (scrollViewer.VerticalOffset > 0.0 && scrollViewer.ScrollableHeight > 0.0)
        this._shadowBorder.Visibility = Visibility.Visible;
      else
        this._shadowBorder.Visibility = Visibility.Collapsed;
    }

    public async void LoadSavedProject()
    {
      ProjectListView projectListView = this;
      string selectedProject = Utils.FindParent<IListViewParent>((DependencyObject) projectListView)?.GetSelectedProject();
      ProjectItemViewModel projectItemViewModel = ProjectItemViewModel.BuildProject(selectedProject);
      UtilLog.Info("LoadSavedProject : " + selectedProject);
      if (projectItemViewModel != null)
      {
        projectListView.SelectedItem = projectItemViewModel;
        projectListView.SetSelectedItemHighlighted();
        EventHandler projectSelected = projectListView.ProjectSelected;
        if (projectSelected == null)
          return;
        projectSelected((object) projectListView, (EventArgs) null);
      }
      else
        projectListView.SetAndSelectDefaultProject();
    }

    public void SelectGroupProject(string gId)
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == gId));
      if (projectItemViewModel != null)
      {
        this._projectList.ScrollIntoView((object) projectItemViewModel);
      }
      else
      {
        ProjectGroupModel groupById = CacheManager.GetGroupById(gId);
        projectItemViewModel = groupById != null ? (ProjectItemViewModel) new ticktick_WPF.ViewModels.ProjectGroupViewModel(groupById) : (ProjectItemViewModel) null;
      }
      if (projectItemViewModel == null)
        return;
      this.SelectedItem = projectItemViewModel;
      EventHandler projectSelected = this.ProjectSelected;
      if (projectSelected != null)
        projectSelected((object) this, (EventArgs) null);
      this.SetSelectedItemHighlighted();
    }

    public void SelectTagProject(string tag)
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == tag));
      if (projectItemViewModel != null)
      {
        this._projectList.ScrollIntoView((object) projectItemViewModel);
      }
      else
      {
        TagModel tag1 = CacheManager.GetTagByName(tag);
        if (tag1 == null)
          tag1 = new TagModel() { name = tag };
        projectItemViewModel = (ProjectItemViewModel) new TagProjectViewModel(tag1);
      }
      this.SelectedItem = projectItemViewModel;
      EventHandler projectSelected = this.ProjectSelected;
      if (projectSelected != null)
        projectSelected((object) this, (EventArgs) null);
      this.SetSelectedItemHighlighted();
    }

    public void SelectFilter(string id, bool ignoreEvent = false)
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == id));
      if (projectItemViewModel == null)
      {
        FilterModel filter = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == id));
        projectItemViewModel = filter != null ? (ProjectItemViewModel) new FilterProjectViewModel(filter) : (ProjectItemViewModel) null;
      }
      if (projectItemViewModel != null)
      {
        this.SelectedItem = projectItemViewModel;
        ProjectListView.LogRecentProject("filter", id);
      }
      if (!ignoreEvent)
      {
        EventHandler projectSelected = this.ProjectSelected;
        if (projectSelected != null)
          projectSelected((object) this, (EventArgs) null);
      }
      this.SetSelectedItemHighlighted();
    }

    public void SelectBindCalender(string calId)
    {
      BindCalendarModel bindCalendar = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == calId));
      if (bindCalendar == null)
        return;
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == calId));
      if (projectItemViewModel != null)
      {
        this._projectList.ScrollIntoView((object) projectItemViewModel);
      }
      else
      {
        BindCalendarAccountModel account = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (acc => acc.Id == bindCalendar.AccountId));
        if (account != null)
          projectItemViewModel = (ProjectItemViewModel) new BindCalendarAccountProjectViewModel(account);
      }
      if (projectItemViewModel != null)
      {
        this.SelectedItem = projectItemViewModel;
        EventHandler projectSelected = this.ProjectSelected;
        if (projectSelected != null)
          projectSelected((object) this, (EventArgs) null);
      }
      this.SetSelectedItemHighlighted();
    }

    public void SelectSubscribeCalender(string calId)
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == calId));
      if (projectItemViewModel != null)
      {
        this._projectList.ScrollIntoView((object) projectItemViewModel);
      }
      else
      {
        CalendarSubscribeProfileModel profile = CacheManager.GetSubscribeCalendars().FirstOrDefault<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Id == calId));
        if (profile != null)
          projectItemViewModel = (ProjectItemViewModel) new SubscribeCalendarProjectViewModel(profile);
      }
      if (projectItemViewModel != null)
      {
        this.SelectedItem = projectItemViewModel;
        EventHandler projectSelected = this.ProjectSelected;
        if (projectSelected != null)
          projectSelected((object) this, (EventArgs) null);
      }
      this.SetSelectedItemHighlighted();
    }

    public void SelectNormalProject(string projectId, string taskId = null)
    {
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      if (project == null)
        return;
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == projectId));
      if (projectItemViewModel != null)
        this._projectList.ScrollIntoView((object) projectItemViewModel);
      else
        projectItemViewModel = (ProjectItemViewModel) new NormalProjectViewModel(project);
      this.SelectedItem = projectItemViewModel;
      EventHandler projectSelected = this.ProjectSelected;
      if (projectSelected != null)
        projectSelected((object) this, (EventArgs) null);
      this.SetSelectedItemHighlighted();
    }

    public async Task OnProjectClick(ProjectItemViewModel model, bool manual)
    {
      ProjectListView projectListView = this;
      if (model is ClosedProjectGroupViewModel || model.Id == projectListView.SelectedItem?.Id)
        return;
      if (projectListView._projectList.ItemsSource is ObservableCollection<ProjectItemViewModel> itemsSource)
      {
        foreach (ProjectItemViewModel projectItemViewModel in (Collection<ProjectItemViewModel>) itemsSource)
        {
          if (projectItemViewModel.Id == model.Id)
            projectItemViewModel.Selected = true;
          else if (projectItemViewModel.Selected)
            projectItemViewModel.Selected = false;
        }
      }
      projectListView.SelectedItem = model;
      projectListView.GetPinView()?.SetSelectedProject(model);
      Utils.FindParent<IListViewParent>((DependencyObject) projectListView)?.SaveSelectedProject(model.GetSaveIdentity());
      EventHandler projectSelected = projectListView.ProjectSelected;
      if (projectSelected != null)
        projectSelected((object) projectListView, (EventArgs) null);
      string listType = "list";
      if (!(model is ticktick_WPF.ViewModels.SmartProjectViewModel projectViewModel))
      {
        if (!(model is NormalProjectViewModel))
        {
          if (!(model is ticktick_WPF.ViewModels.ProjectGroupViewModel))
          {
            if (!(model is TagProjectViewModel))
            {
              if (!(model is FilterProjectViewModel))
              {
                if (!(model is SubscribeCalendarProjectViewModel model2))
                {
                  if (model is BindCalendarAccountProjectViewModel model1)
                  {
                    projectListView.OnBindCalendarProjectClick(model1);
                    listType = "events";
                  }
                }
                else
                {
                  projectListView.OnSubscribeCalendarClick(model2);
                  listType = "events";
                }
              }
              else
              {
                listType = "filter";
                ProjectListView.LogRecentProject("filter", model.Id);
              }
            }
            else
            {
              listType = "tag";
              ProjectListView.LogRecentProject("tag", model.Id);
            }
          }
          else
          {
            ProjectGroupDao.CheckGroupGroupBy(model.Id);
            listType = "folder";
          }
        }
        else
        {
          ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.Id));
          if ((projectModel != null ? (projectModel.IsShareList() ? 1 : 0) : 0) != 0)
            AvatarHelper.ResetProjectAvatars(model.Id);
          ProjectListView.LogRecentProject("project", model.Id);
        }
      }
      else
      {
        if (manual)
          UserActCollectUtils.AddClickEvent("project_list_ui", "select", projectViewModel is ticktick_WPF.ViewModels.InboxProjectViewModel ? "inbox" : projectViewModel.Project.UserEventId);
        if ((projectViewModel.Project is TodayProject || projectViewModel.Project is WeekProject) && LocalSettings.Settings.HabitInToday)
          Task.Run((Action) (() => HabitSyncService.PullHabits(8)));
      }
      if (!manual)
        return;
      UtilLog.Info("Select " + model.GetType().Name + " " + model.Id);
      projectListView.CollectSelectEvent(model.ViewMode, listType, model.GetIdentity()?.SortOption);
      TaskCountCache.ReloadProjectTaskCount(model.GetIdentity());
    }

    private void CollectSelectEvent(string viewMode, string listType, SortOption sortOption)
    {
      UserActCollectUtils.AddClickEvent("project_list_ui", "select", listType);
      if (sortOption != null)
        UserActCollectUtils.AddSortOptionEvent("list_group_order", "sort_view", sortOption.groupBy, sortOption.orderBy);
      if (string.IsNullOrEmpty(viewMode))
        return;
      UserActCollectUtils.AddClickEvent("list_mode", "show", viewMode);
      if (!(viewMode == "timeline"))
        return;
      UserActCollectUtils.AddClickEvent("timeline", "view_list_type", listType);
    }

    private static void LogRecentProject(string type, string id)
    {
      string str = type + ":" + id;
      List<string> list = ((IEnumerable<string>) LocalSettings.Settings.RecentProjects.Split(',')).ToList<string>();
      if (!list.Contains(str))
      {
        list.Insert(0, str);
      }
      else
      {
        list.Remove(str);
        list.Insert(0, str);
      }
      LocalSettings.Settings.RecentProjects = string.Join(",", (IEnumerable<string>) list);
      JumpHelper.InitJumpList();
    }

    public void CollapseItem(ProjectItemViewModel model)
    {
      if (model is ticktick_WPF.ViewModels.ProjectGroupViewModel vm1)
      {
        vm1.Open = !vm1.Open;
        if (vm1 is ClosedProjectGroupViewModel)
        {
          LocalSettings.Settings.ClosedSectionStatus = vm1.Open.ToString();
        }
        else
        {
          vm1.Icon = Utils.GetIcon(vm1.Open ? "IcOpenedFolder" : "IcClosedFolder");
          if (vm1.ProjectGroup != null)
          {
            vm1.ProjectGroup.open = vm1.Open;
            ProjectGroupDao.TrySaveProjectGroup(vm1.ProjectGroup, true);
          }
        }
        this.OnOpenGroupClick((ProjectItemViewModel) vm1);
      }
      if (!(model is TagProjectViewModel vm2))
        return;
      vm2.Open = !vm2.Open;
      TagModel tagByName = CacheManager.GetTagByName(vm2.Id.ToLower());
      if (tagByName != null)
      {
        tagByName.collapsed = !vm2.Open;
        vm2.TagModel = tagByName;
        TagDao.UpdateTag(tagByName);
      }
      this.OnOpenGroupClick((ProjectItemViewModel) vm2);
    }

    private void OnOpenGroupClick(ProjectItemViewModel vm)
    {
      if (!(this._projectList.ItemsSource is ObservableCollection<ProjectItemViewModel> itemsSource))
        return;
      ProjectItemViewModel projectItemViewModel = itemsSource.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == vm.Id));
      switch (projectItemViewModel)
      {
        case ticktick_WPF.ViewModels.ProjectGroupViewModel _:
label_4:
          if (projectItemViewModel.Open)
          {
            int num = itemsSource.IndexOf(projectItemViewModel);
            if (num <= 0 || projectItemViewModel.Children.Count <= 0)
              break;
            using (List<ProjectItemViewModel>.Enumerator enumerator = projectItemViewModel.Children.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                ProjectItemViewModel current = enumerator.Current;
                itemsSource.Insert(++num, current);
              }
              break;
            }
          }
          else
          {
            if (projectItemViewModel.Children.Count <= 0)
              break;
            using (List<ProjectItemViewModel>.Enumerator enumerator = projectItemViewModel.Children.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                ProjectItemViewModel current = enumerator.Current;
                itemsSource.Remove(current);
              }
              break;
            }
          }
        case TagProjectViewModel projectViewModel:
          if (!projectViewModel.IsParent)
            break;
          goto label_4;
      }
    }

    public async void OnAddProjectClick(string groupId, string teamId)
    {
      ProjectListView projectListView = this;
      if (await projectListView.CheckProjectOverLimit(CheckListLimiteType.Add))
        return;
      EditProjectDialog editProjectDialog = new EditProjectDialog(groupId, teamId);
      editProjectDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
      UserActCollectUtils.AddClickEvent("project_list_ui", "list", "add");
      editProjectDialog.OnProjectSaved += new EventHandler<ticktick_WPF.ViewModels.ProjectViewModel>(projectListView.OnProjectSaved);
      editProjectDialog.ShowDialog();
      editProjectDialog.OnProjectSaved -= new EventHandler<ticktick_WPF.ViewModels.ProjectViewModel>(projectListView.OnProjectSaved);
    }

    private async Task<bool> CheckProjectOverLimit(CheckListLimiteType type, bool isOpen = false)
    {
      ProjectListView projectListView = this;
      long projectLimitNumber = Utils.GetUserLimit(Constants.LimitKind.ProjectNumber);
      List<string> teamIds = CacheManager.GetTeams().Select<TeamModel, string>((Func<TeamModel, string>) (t => t.id)).ToList<string>();
      List<ProjectModel> list;
      if (!UserDao.IsPro())
        list = (await ProjectDao.GetAllProjects(false)).ToList<ProjectModel>();
      else
        list = (await ProjectDao.GetAllProjects(false, false)).Where<ProjectModel>((Func<ProjectModel, bool>) (p => !TeamDao.IsTeamExpired(p.teamId))).ToList<ProjectModel>();
      int count = list.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.teamId == null || teamIds.Contains(p.teamId))).ToList<ProjectModel>().Count;
      if (!UserDao.IsPro() & isOpen)
        --count;
      if ((long) count < projectLimitNumber)
      {
        UtilLog.Info(string.Format("AddProjectLimit : {0}, current {1}, isOpen {2}", (object) projectLimitNumber, (object) count, (object) isOpen));
        return false;
      }
      if (UserDao.IsPro())
      {
        string content = "";
        switch (type)
        {
          case CheckListLimiteType.Add:
            content = string.Format(Utils.GetString("ListsLimitAdd"), (object) count);
            break;
          case CheckListLimiteType.Copy:
            content = string.Format(Utils.GetString("ListsLimitCopy"), (object) count);
            break;
          case CheckListLimiteType.Open:
            content = string.Format(Utils.GetString("ListsLimitAdd"), (object) count);
            break;
        }
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("ProjectLimitTips"), content, MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
        customerDialog.ShowDialog();
        return true;
      }
      ProChecker.ShowUpgradeDialog(ProType.MoreLists);
      return true;
    }

    public void OnAddTagClick()
    {
      AddOrEditTagWindow addOrEditTagWindow = new AddOrEditTagWindow();
      addOrEditTagWindow.Owner = Window.GetWindow((DependencyObject) this);
      UserActCollectUtils.AddClickEvent("project_list_ui", "tag", "add");
      addOrEditTagWindow.TagSaved += (EventHandler<TagModel>) (async (arg, tag) =>
      {
        ProjectListView child = this;
        if (LocalSettings.Settings.SmartListTag == 2)
          child.ShowTagCreateHint();
        else if (tag != null)
        {
          UtilLog.Info("ProjectMenu.AddTag : " + tag.name);
          Utils.FindParent<ListViewContainer>((DependencyObject) child)?.SelectTagProject(tag.name);
        }
        ListViewContainer.ReloadProjectData();
      });
      addOrEditTagWindow.ShowDialog();
    }

    private async void ShowTagCreateHint()
    {
      ProjectListView projectListView = this;
      if (!LocalSettings.Settings.AutoTagUserIds.Contains(Utils.GetCurrentUserIdInt().ToString()))
      {
        await Task.Delay(100);
        LocalSettings.Settings.AutoTagUserIds = LocalSettings.Settings.AutoTagUserIds + ";" + Utils.GetCurrentUserIdInt().ToString();
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("ShowTags"), Utils.GetString("AutoShowTagsMessage"), Utils.GetString("GotIt"), string.Empty);
        customerDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
        customerDialog.ShowDialog();
      }
      else
        Utils.Toast(Utils.GetString("AutoTagAddHint"));
    }

    private void NotifyGroupOpenChanged(ProjectItemViewModel projectModel)
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == projectModel.Id));
      switch (projectItemViewModel)
      {
        case ticktick_WPF.ViewModels.ProjectGroupViewModel _:
label_3:
          if (projectItemViewModel.Open)
          {
            int num = this.ProjectItems.IndexOf(projectItemViewModel);
            if (num <= 0 || projectItemViewModel.Children.Count <= 0)
              break;
            using (List<ProjectItemViewModel>.Enumerator enumerator = projectItemViewModel.Children.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                ProjectItemViewModel current = enumerator.Current;
                this.ProjectItems.Insert(++num, current);
              }
              break;
            }
          }
          else
          {
            if (projectItemViewModel.Children.Count <= 0)
              break;
            using (List<ProjectItemViewModel>.Enumerator enumerator = projectItemViewModel.Children.GetEnumerator())
            {
              while (enumerator.MoveNext())
                this.ProjectItems.Remove(enumerator.Current);
              break;
            }
          }
        case TagProjectViewModel projectViewModel:
          if (!projectViewModel.IsParent)
            break;
          goto label_3;
      }
    }

    public async void DeleteFilter(FilterModel filter)
    {
      ProjectListView sender = this;
      if (filter == null)
        return;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString(nameof (DeleteFilter)), string.Format(Utils.GetString("DeleteFilterHint"), (object) filter.name), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) sender);
      bool? nullable = customerDialog.ShowDialog();
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
        return;
      filter.syncStatus = 1;
      filter.deleted = 1;
      UtilLog.Info("ProjectMenu.DeleteFilter : " + filter.id);
      await FilterDao.UpdateFilter(filter);
      SyncSortOrderDao.OnFilterDeleted(filter.id);
      sender.OnFilterDeleted((object) sender, filter.id);
    }

    public async void UnsubscribeCalendar(CalendarSubscribeProfileModel calendar)
    {
      ProjectListView projectListView = this;
      if (calendar == null)
        return;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("Unsubscribe"), Utils.GetString("CancelSubscribeMessage"), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
      bool? nullable = customerDialog.ShowDialog();
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
        return;
      UtilLog.Info("ProjectMenu.UnsubCal : id" + calendar.Id + ", name " + calendar.Name);
      await SubscribeCalendarHelper.UnsubscribeCalendar(calendar.Id);
      ListViewContainer.ReloadProjectData();
      projectListView.SetAndSelectDefaultProject();
      App.Window.TryReloadCalendar();
    }

    public async void EditBindCalendar(string accountId)
    {
      ProjectListView projectListView = this;
      if (string.IsNullOrEmpty(accountId))
        return;
      EditBindAccountWindow bindAccountWindow = await EditBindAccountWindow.Build(accountId);
      bindAccountWindow.Owner = Window.GetWindow((DependencyObject) projectListView);
      bindAccountWindow.ShowDialog();
      projectListView.TrySelectDefaultProject(accountId);
    }

    private void TrySelectDefaultProject(string accountId)
    {
      if (CacheManager.GetBindCalendars().Any<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.AccountId == accountId && cal.Show != "hidden")))
        return;
      this.SetAndSelectDefaultProject();
    }

    public async void UnbindCalendar(string accountId)
    {
      ProjectListView projectListView = this;
      if (string.IsNullOrEmpty(accountId))
        return;
      bool? nullable = new CustomerDialog(Utils.GetString("Unsubscribe"), Utils.GetString("CancelSubscribeMessage"), MessageBoxButton.OKCancel, Window.GetWindow((DependencyObject) projectListView)).ShowDialog();
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
        return;
      await SubscribeCalendarHelper.UnbindCalendar(accountId);
      ListViewContainer.ReloadProjectData();
      projectListView.SetAndSelectDefaultProject();
    }

    public async void EditSubscribeCalendar(CalendarSubscribeProfileModel calendar)
    {
      ProjectListView projectListView = this;
      if (calendar == null)
        return;
      EditUrlWindow editUrlWindow = new EditUrlWindow(new ticktick_WPF.Views.Config.SubscribeCalendarViewModel(await CalendarSubscribeProfileDao.GetProfileById(calendar.Id)));
      editUrlWindow.Owner = Window.GetWindow((DependencyObject) projectListView);
      editUrlWindow.ShowDialog();
      ListViewContainer.ReloadProjectData();
    }

    public void EditFilter(FilterModel filter)
    {
      if (!ProChecker.CheckPro(ProType.Filter) || filter == null)
        return;
      string inboxId = Utils.GetInboxId();
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == inboxId));
      if (projectModel != null)
        projectModel.sortOrder = long.MinValue;
      FilterEditDialog filterEditDialog = new FilterEditDialog(filter);
      filterEditDialog.Owner = (Window) App.Window;
      filterEditDialog.Owner = Window.GetWindow((DependencyObject) this);
      filterEditDialog.FilterSaved += (EventHandler<FilterModel>) ((o, f) =>
      {
        if (f == null)
          return;
        ListViewContainer.ReloadProjectData();
        ProjectWidgetsHelper.OnProjectChanged((ProjectIdentity) new FilterProjectIdentity(f));
        SyncManager.Sync();
      });
      filterEditDialog.ShowDialog();
    }

    private async void OnFilterDeleted(object sender, string filterId)
    {
      ListViewContainer.ReloadProjectData();
      if (this.SelectedItem?.Id == filterId)
      {
        ProjectWidgetsHelper.OnProjectChanged(this.SelectedItem?.GetIdentity());
        this.SetAndSelectDefaultProject();
      }
      SyncManager.Sync();
    }

    private int GetGroupIndex(string groupId)
    {
      for (int index = 0; index < this.ProjectItems.Count; ++index)
      {
        if (this.ProjectItems[index] is ticktick_WPF.ViewModels.ProjectGroupViewModel projectItem && projectItem.ProjectGroup.id == groupId)
          return index;
      }
      return -1;
    }

    public void OnRenameGroup(ProjectGroupModel group)
    {
      int groupIndex = this.GetGroupIndex(group.id);
      if (groupIndex < 0)
        return;
      ticktick_WPF.ViewModels.ProjectGroupViewModel projectItem = (ticktick_WPF.ViewModels.ProjectGroupViewModel) this.ProjectItems[groupIndex];
      if (projectItem == null)
        return;
      AddProjectGroupDialog projectGroupDialog = new AddProjectGroupDialog(projectItem.ProjectGroup);
      projectGroupDialog.Owner = (Window) MainWindowManager.Window;
      projectGroupDialog.ProjectGroupEdit += (EventHandler<ProjectGroupModel>) ((s, e) => ListViewContainer.ReloadProjectData());
      projectGroupDialog.ShowDialog();
    }

    public void OnDeleteProjectGroup(ProjectGroupModel projectGroup)
    {
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LeftMenuProjectDeleteGroupTitle"), string.Format(Utils.GetString("LeftMenuProjectDeleteGroupMessage"), (object) projectGroup.name), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) this);
      bool? nullable = customerDialog.ShowDialog();
      bool flag = true;
      if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        return;
      this.DeleteProjectGroup(projectGroup.id);
    }

    private async void DeleteProjectGroup(string groupId)
    {
      ObservableCollection<ProjectModel> projectInGroupList = await ProjectDao.GetProjectsInGroup(groupId);
      foreach (ProjectModel project in (Collection<ProjectModel>) projectInGroupList)
      {
        project.groupId = "NONE";
        int num = await ProjectDao.TryUpdateProject(project);
      }
      int num1 = await ProjectGroupDao.DeleteProjectGroupById(groupId);
      ListViewContainer.ReloadProjectData();
      foreach (ProjectModel project in (Collection<ProjectModel>) projectInGroupList)
      {
        project.groupId = "NONE";
        string str = await Communicator.UpdatePutProject(project);
      }
      UtilLog.Info("ProjectMenu.DeleteProjectGroup : " + groupId);
      int num2 = await Communicator.DeleteProjectGroup(groupId) ? 1 : 0;
      ProjectWidgetsHelper.OnProjectChanged((ProjectIdentity) new GroupProjectIdentity(new ProjectGroupModel()
      {
        id = groupId
      }, (List<ProjectModel>) null));
      if (!(this.SelectedItem?.Id == groupId))
      {
        projectInGroupList = (ObservableCollection<ProjectModel>) null;
      }
      else
      {
        this.SetAndSelectDefaultProject();
        projectInGroupList = (ObservableCollection<ProjectModel>) null;
      }
    }

    public void EditInboxProject()
    {
      new EditInboxDialog().ShowDialog();
      EventHandler projectSelected = this.ProjectSelected;
      if (projectSelected != null)
        projectSelected((object) this, (EventArgs) null);
      ProjectWidgetsHelper.OnProjectChanged(this.SelectedItem.GetIdentity());
      ListViewContainer.ReloadProjectData();
    }

    public async void EditProject(ProjectModel project)
    {
      ProjectListView projectListView = this;
      if (project == null)
        return;
      ProjectModel projectById = await ProjectDao.GetProjectById(project.id);
      if (projectById == null)
        return;
      EditProjectDialog editProjectDialog = new EditProjectDialog(new ticktick_WPF.ViewModels.ProjectViewModel(projectById));
      editProjectDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
      editProjectDialog.OnProjectSaved += new EventHandler<ticktick_WPF.ViewModels.ProjectViewModel>(projectListView.OnProjectSaved);
      editProjectDialog.ShowDialog();
      editProjectDialog.OnProjectSaved -= new EventHandler<ticktick_WPF.ViewModels.ProjectViewModel>(projectListView.OnProjectSaved);
    }

    public async void ShareProject(ProjectModel project)
    {
      ProjectListView projectListView = this;
      ShareProjectDialog.TryShowShareDialog(project.id, Window.GetWindow((DependencyObject) projectListView));
    }

    public async void CloseOrOpenProject(ProjectModel project)
    {
      ProjectListView projectListView = this;
      if (project == null)
        return;
      if (project.closed.HasValue && project.closed.Value)
      {
        if (await projectListView.CheckProjectOverLimit(CheckListLimiteType.Open, true))
          return;
      }
      bool? nullable1;
      if (!project.closed.HasValue || !project.closed.Value)
      {
        if (TaskDefaultDao.GetDefaultSafely().ProjectId == project.id)
        {
          ModifyDefaultProjectDialog defaultProjectDialog = new ModifyDefaultProjectDialog(isArchive: true);
          defaultProjectDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
          defaultProjectDialog.ShowDialog();
          return;
        }
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("ArchiveList"), string.Format(Utils.GetString("ArchiveConfirmMessage"), (object) project.name), MessageBoxButton.OKCancel);
        customerDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
        nullable1 = customerDialog.ShowDialog();
        bool flag = true;
        if (!(nullable1.GetValueOrDefault() == flag & nullable1.HasValue))
          return;
      }
      string id = project.id;
      CompletionLoadStatusDao.DeleteStatusByEntityId(id);
      ProjectModel originalProject = await ProjectDao.GetProjectById(id);
      if (originalProject != null)
      {
        nullable1 = originalProject.closed;
        if (!nullable1.HasValue)
          originalProject.closed = new bool?(false);
        ProjectModel projectModel = originalProject;
        nullable1 = originalProject.closed;
        bool? nullable2 = nullable1.HasValue ? new bool?(!nullable1.GetValueOrDefault()) : new bool?();
        projectModel.closed = nullable2;
        originalProject.sortOrder = ProjectDragHelper.GetNewSortOrderOnCloseChanged(originalProject);
        if (originalProject.sync_status == Constants.SyncStatus.SYNC_DONE.ToString())
          originalProject.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
        if (TaskStickyWindow.TryCloseInProject(project.id))
          Utils.Toast(Utils.GetString("StickyClosed"));
        int num = await ProjectDao.TryUpdateProject(originalProject);
        projectListView.SelectedItem = (ProjectItemViewModel) null;
        ListViewContainer.ReloadProjectData();
        SyncManager.TryDelaySync();
        nullable1 = originalProject.closed;
        if (nullable1.HasValue)
        {
          nullable1 = originalProject.closed;
          if (nullable1.Value)
          {
            SyncSortOrderDao.OnProjectDeletedOrClosed(project.id);
            goto label_22;
          }
        }
        SyncManager.PullProjectTasks(originalProject.id);
      }
label_22:
      UtilLog.Info(string.Format("ProjectMenu.CloseOrOpenProject : {0},{1}", (object) project.id, (object) project.closed));
      DataChangedNotifier.NotifyProjectChanged();
      originalProject = (ProjectModel) null;
    }

    public async Task DuplicateProject(ProjectModel project)
    {
      ProjectListView projectListView = this;
      if (await projectListView.CheckProjectOverLimit(CheckListLimiteType.Copy) || project == null)
        return;
      DuplicateProjectDialog duplicateProjectDialog1 = new DuplicateProjectDialog();
      duplicateProjectDialog1.Owner = Window.GetWindow((DependencyObject) projectListView);
      DuplicateProjectDialog duplicateProjectDialog2 = duplicateProjectDialog1;
      duplicateProjectDialog2.ShowDialog();
      if (duplicateProjectDialog2.Option == DuplicateProjectEnum.None)
        return;
      string originalName = project.name;
      int originalUserCount = project.userCount;
      project.name = project.name + " " + Utils.GetString("ProjectCopy");
      project.userCount = 1;
      ProjectIdentityModel identity = await Communicator.DuplicateProject(project.id, project, (int) duplicateProjectDialog2.Option);
      project.name = originalName;
      project.userCount = originalUserCount;
      long sortOrder = ProjectDragHelper.GetNewSortOrder(false, project.groupId, project.id, project.teamId);
      if (identity != null)
      {
        SyncManager.Sync();
        DataChangedNotifier.SyncDone += new EventHandler<SyncResult>(NavigateProjectOnAutoSyncDone);
      }
      UtilLog.Info("ProjectMenu.DuplicateProject : origin " + project.id + ",new " + identity?.id);
      originalName = (string) null;
      // ISSUE: variable of a compiler-generated type
      ProjectListView.\u003C\u003Ec__DisplayClass91_0 cDisplayClass910;

      async void NavigateProjectOnAutoSyncDone(object sender, SyncResult changed)
      {
        // ISSUE: method pointer
        DataChangedNotifier.SyncDone -= new EventHandler<SyncResult>((object) cDisplayClass910, __methodptr(\u003CDuplicateProject\u003Eg__NavigateProjectOnAutoSyncDone\u007C0));
        ProjectModel newProject = CacheManager.GetProjectById(identity.id);
        if (newProject == null)
        {
          newProject = (ProjectModel) null;
        }
        else
        {
          newProject.sortOrder = sortOrder;
          newProject.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          int num = await ProjectDao.TryUpdateProject(newProject);
          ListViewContainer.ReloadProjectData();
          App.Window.NavigateNormalProject(newProject.id);
          newProject = (ProjectModel) null;
        }
      }
    }

    public async Task DeleteOrExitProject(ProjectModel project, bool isDelete = true)
    {
      ProjectListView projectListView = this;
      if (project == null)
        ;
      else if (TaskDefaultDao.GetDefaultSafely().ProjectId == project.id)
      {
        ModifyDefaultProjectDialog defaultProjectDialog = new ModifyDefaultProjectDialog(true);
        defaultProjectDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
        defaultProjectDialog.ShowDialog();
      }
      else
      {
        CustomerDialog customerDialog = new CustomerDialog(isDelete ? string.Format(Utils.GetString("DeleteList_Title"), (object) project.name) : Utils.GetString("ExitProjectTitle"), isDelete ? Utils.GetString(project.IsShareList() ? "DeleteShareList_Content" : "DeleteList_Content") : string.Format(Utils.GetString("ExitProjectSummary"), (object) project.name), Utils.GetString("Delete"), Utils.GetString("Cancel"));
        customerDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
        bool? nullable = customerDialog.ShowDialog();
        bool flag = true;
        if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
          ;
        else
        {
          ProjectItemViewModel projectItemViewModel = projectListView.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p is NormalProjectViewModel projectViewModel && projectViewModel.ProjectId == project.id));
          if (projectItemViewModel != null)
            projectListView.ProjectItems.Remove(projectItemViewModel);
          if (TaskStickyWindow.TryCloseInProject(project.id))
            Utils.Toast(Utils.GetString("StickyClosed"));
          await ProjectDao.DeleteProjectById(project.id);
          UtilLog.Info("ProjectMenu.DeleteOrExitProject : " + project.id);
          ListViewContainer.ReloadProjectData();
          SyncManager.TryDelaySync();
          ProjectWidgetsHelper.OnProjectChanged(projectListView.SelectedItem.GetIdentity());
          SyncSortOrderDao.OnProjectDeletedOrClosed(project.id);
          if (!(projectListView.SelectedItem?.Id == project.id))
            ;
          else
            projectListView.SetAndSelectDefaultProject();
        }
      }
    }

    public async void EditTag(TagModel tag, bool canEditTitle)
    {
      ProjectListView projectListView = this;
      if (tag == null)
        return;
      AddOrEditTagWindow addOrEditTagWindow = new AddOrEditTagWindow(tag, canEditTitle);
      addOrEditTagWindow.Owner = Window.GetWindow((DependencyObject) projectListView);
      string originName = tag.name;
      addOrEditTagWindow.TagSaved += (EventHandler<TagModel>) (async (sender, newTag) =>
      {
        UtilLog.Info(string.Format("ProjectMenu.EditTag : origin {0},saved {1}", (object) originName, (object) newTag));
        if (newTag != null)
        {
          if (this.SelectedItem == null || this.SelectedItem.Id.ToLower() == originName && originName != newTag.name)
          {
            this.SelectedItem = (ProjectItemViewModel) new TagProjectViewModel(newTag);
            this.SetSelectedItemHighlighted();
            EventHandler projectSelected = this.ProjectSelected;
            if (projectSelected != null)
              projectSelected((object) this, (EventArgs) null);
          }
          else if (this.SelectedItem.GetIdentity().SortOption.ContainsSortType(Constants.SortType.tag.ToString()))
          {
            EventHandler projectSelected = this.ProjectSelected;
            if (projectSelected != null)
              projectSelected((object) this, (EventArgs) null);
          }
        }
        ListViewContainer.ReloadProjectData();
      });
      addOrEditTagWindow.ShowDialog();
    }

    public void MergeTag(TagModel tag)
    {
      MergeTagWindow mergeTagWindow1 = new MergeTagWindow(tag);
      mergeTagWindow1.Owner = Window.GetWindow((DependencyObject) this);
      MergeTagWindow mergeTagWindow2 = mergeTagWindow1;
      mergeTagWindow2.ShowDialog();
      if (string.IsNullOrEmpty(mergeTagWindow2.MergeTag))
        return;
      UtilLog.Info("ProjectMenu.MergeTag : from " + tag.name + ",to " + mergeTagWindow2.MergeTag);
      this.SelectTagProject(mergeTagWindow2.MergeTag);
      ListViewContainer.ReloadProjectData();
    }

    public async void DeleteTag(TagModel tag)
    {
      ProjectListView projectListView = this;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString(nameof (DeleteTag)), string.Format(Utils.GetString("DeleteTagMessage"), (object) tag.GetDisplayName()), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
      bool? nullable = customerDialog.ShowDialog();
      if (!nullable.HasValue || !nullable.Value)
        return;
      await TagService.DeleteTag(tag.name);
      await Communicator.DeleteTag(tag.name);
      UtilLog.Info("ProjectMenu.DeleteTag : " + tag.name);
      SyncSortOrderDao.OnTagDeleted(tag.name);
      ListViewContainer.ReloadProjectData();
      if (!(projectListView.SelectedItem?.Id == tag.name))
        return;
      projectListView.SetAndSelectDefaultProject();
    }

    private async Task OnBindCalendarProjectClick(BindCalendarAccountProjectViewModel model)
    {
      if (model?.Account == null)
        return;
      Task.Run((Func<Task>) (async () =>
      {
        EventArchiveSyncService.PullArchivedModelsAsync();
        this.PullRemoteCalendarEvents(model.Account.Id, model.Account.Kind, model.Account.Site);
      }));
    }

    private async Task PullRemoteCalendarEvents(string accountId, string kind, string site)
    {
      await CalendarService.PullAccountEvents(accountId, kind, site);
      this.LoadData();
    }

    private async Task OnSubscribeCalendarClick(SubscribeCalendarProjectViewModel model)
    {
      if (model?.Profile == null)
        return;
      Task.Run((Func<Task>) (async () =>
      {
        string calendarId = model.Profile.Id;
        if (string.IsNullOrEmpty(calendarId))
          calendarId = Utils.Base64Encode(model.Profile.Url);
        await SubscribeCalendarHelper.ParseUrlCalendar(calendarId, model.Profile.Url);
        EventArchiveSyncService.PullArchivedModelsAsync();
        this.Dispatcher.Invoke((Action) (() =>
        {
          ListViewContainer.ReloadProjectData();
          EventHandler projectSelected = this.ProjectSelected;
          if (projectSelected != null)
            projectSelected((object) this, (EventArgs) null);
          if (ABTestManager.IsNewRemindCalculate())
            EventReminderCalculator.InitAllEventsReminderTimes();
          else
            ReminderCalculator.AssembleReminders();
        }));
      }));
    }

    public void ShowOrHideProject(SmartProject project, ContextActionKey action)
    {
      string empty = string.Empty;
      string propertyName = string.Empty;
      int num1;
      switch (action)
      {
        case ContextActionKey.Show:
          num1 = 0;
          break;
        case ContextActionKey.ShowIfNotEmpty:
          num1 = 2;
          break;
        default:
          num1 = 1;
          break;
      }
      int num2 = num1;
      switch (project)
      {
        case AllProject _:
          propertyName = "SmartListAll";
          empty = Utils.GetString("All");
          break;
        case TodayProject _:
          propertyName = "SmartListToday";
          empty = Utils.GetString("Today");
          break;
        case TomorrowProject _:
          propertyName = "SmartListTomorrow";
          empty = Utils.GetString("Tomorrow");
          break;
        case WeekProject _:
          propertyName = "SmartList7Day";
          empty = Utils.GetString("Next7Day");
          break;
        case AssignProject _:
          propertyName = "SmartListForMe";
          empty = Utils.GetString("AssignToMe");
          break;
        case CompletedProject _:
          propertyName = "SmartListComplete";
          empty = Utils.GetString("Completed");
          break;
        case AbandonedProject _:
          propertyName = "SmartListAbandoned";
          empty = Utils.GetString("Abandoned");
          break;
        case TrashProject _:
          propertyName = "SmartListTrash";
          empty = Utils.GetString("Trash");
          break;
        case SummaryProject _:
          propertyName = "SmartListSummary";
          empty = Utils.GetString("Summary");
          break;
        case InboxProject _:
          if (action == ContextActionKey.Hide && LocalSettings.Settings.SmartListInbox != 1 && TaskDefaultDao.GetDefaultSafely().ProjectId == LocalSettings.Settings.InServerBoxId)
          {
            ModifyDefaultProjectDialog defaultProjectDialog = new ModifyDefaultProjectDialog();
            defaultProjectDialog.Owner = Window.GetWindow((DependencyObject) this);
            defaultProjectDialog.ShowDialog();
            return;
          }
          propertyName = "SmartListInbox";
          empty = Utils.GetString("Inbox");
          break;
      }
      if (!string.IsNullOrEmpty(propertyName))
      {
        LocalSettings.Settings[propertyName] = (object) num2;
        ListViewContainer.ReloadProjectData();
      }
      if (action != ContextActionKey.Hide)
      {
        if (action != ContextActionKey.ShowIfNotEmpty)
          return;
        Utils.Toast(string.Format(Utils.GetString("AutoSmartProjectTip"), (object) empty));
      }
      else
      {
        Utils.Toast(string.Format(Utils.GetString("HideHint"), (object) empty));
        this.SetAndSelectDefaultProject();
      }
    }

    public async void OnAddFilterClick()
    {
      if (!ProChecker.CheckPro(ProType.Filter))
        return;
      UserActCollectUtils.AddClickEvent("project_list_ui", "filter", "add");
      await this.AddFilter();
    }

    private async Task AddFilter()
    {
      ProjectListView projectListView = this;
      ObservableCollection<ProjectModel> projectsWithoutClosed = await ProjectDao.GetProjectsWithoutClosed();
      string inboxId = Utils.GetInboxId();
      Func<ProjectModel, bool> predicate = (Func<ProjectModel, bool>) (p => p.id == inboxId);
      ProjectModel projectModel = projectsWithoutClosed.FirstOrDefault<ProjectModel>(predicate);
      if (projectModel != null)
        projectModel.sortOrder = long.MinValue;
      FilterEditDialog filterEditDialog = new FilterEditDialog();
      filterEditDialog.Owner = Window.GetWindow((DependencyObject) projectListView);
      filterEditDialog.FilterSaved += new EventHandler<FilterModel>(projectListView.OnFilterAdded);
      filterEditDialog.ShowDialog();
    }

    private void OnFilterAdded(object sender, FilterModel filter)
    {
      if (filter == null)
        return;
      ListViewContainer.ReloadProjectData();
      FilterProjectIdentity filterProjectIdentity = new FilterProjectIdentity(filter);
      filterProjectIdentity.SortOption = filter.GetSortOption();
      FilterProjectIdentity projectId = filterProjectIdentity;
      ListViewContainer parent = Utils.FindParent<ListViewContainer>((DependencyObject) this);
      if ((parent != null ? (parent.Mode != 0 ? 1 : 0) : 1) != 0)
        this.SelectProject((ProjectIdentity) projectId);
      SyncManager.Sync();
    }

    public async Task OnPtfAllClick(ProjectItemViewModel model)
    {
      if (!(this._projectList.ItemsSource is ObservableCollection<ProjectItemViewModel> itemsSource))
        return;
      ProjectItemViewModel section = itemsSource.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p.Id == model.Id));
      int num = itemsSource.IndexOf(section);
      if (section == null || num < 0 || this.ProjectDragging)
        return;
      section.Open = !section.Open;
      if (section.Open)
      {
        if (!(section is TeamGroupViewModel))
        {
          if (!(section is PtfAllViewModel ptfAllViewModel))
            return;
          switch (ptfAllViewModel.Type)
          {
            case PtfType.Project:
              ProjectDataProvider.AddNormalProjects((IList<ProjectItemViewModel>) itemsSource, num + 1);
              LocalSettings.Settings.AllProjectOpened = true;
              break;
            case PtfType.Tag:
              if (ptfAllViewModel.InSubSection)
              {
                LocalSettings.Settings.AllShareTagOpened = true;
                await ProjectDataProvider.AddShareTagProject((IList<ProjectItemViewModel>) itemsSource, num + 1);
                break;
              }
              await ProjectDataProvider.AddTagProject((IList<ProjectItemViewModel>) itemsSource, num + 1);
              LocalSettings.Settings.AllTagOpened = true;
              break;
            case PtfType.Filter:
              ProjectDataProvider.AddFilterProject((IList<ProjectItemViewModel>) itemsSource, num + 1);
              LocalSettings.Settings.AllFilterOpened = true;
              break;
            case PtfType.Subscribe:
              ProjectDataProvider.AddSubscribeCalendar((IList<ProjectItemViewModel>) itemsSource, num + 1);
              LocalSettings.Settings.AllSubscribeOpened = true;
              break;
          }
        }
        else
        {
          foreach (ProjectItemViewModel child1 in section.Children)
          {
            itemsSource.Insert(++num, child1);
            if (child1.Children != null && child1.Children.Any<ProjectItemViewModel>() && child1.Open)
            {
              foreach (ProjectItemViewModel child2 in child1.Children)
                itemsSource.Insert(++num, child2);
            }
          }
        }
      }
      else
      {
        IEnumerable<ProjectItemViewModel> source = (IEnumerable<ProjectItemViewModel>) null;
        if (!(section is TeamGroupViewModel))
        {
          if (section is PtfAllViewModel ptfAllViewModel)
          {
            switch (ptfAllViewModel.Type)
            {
              case PtfType.Project:
                source = itemsSource.Where<ProjectItemViewModel>(new Func<ProjectItemViewModel, bool>(ProjectDataProvider.IsProjectCategory));
                LocalSettings.Settings.AllProjectOpened = false;
                break;
              case PtfType.Tag:
                if (ptfAllViewModel.InSubSection)
                {
                  LocalSettings.Settings.AllShareTagOpened = false;
                  source = itemsSource.Where<ProjectItemViewModel>(new Func<ProjectItemViewModel, bool>(ProjectDataProvider.IsShareTagCategory));
                  break;
                }
                source = itemsSource.Where<ProjectItemViewModel>(new Func<ProjectItemViewModel, bool>(ProjectDataProvider.IsTagCategory));
                LocalSettings.Settings.AllTagOpened = false;
                break;
              case PtfType.Filter:
                source = itemsSource.Where<ProjectItemViewModel>(new Func<ProjectItemViewModel, bool>(ProjectDataProvider.IsFilterCategory));
                LocalSettings.Settings.AllFilterOpened = false;
                break;
              case PtfType.Subscribe:
                source = itemsSource.Where<ProjectItemViewModel>(new Func<ProjectItemViewModel, bool>(ProjectDataProvider.IsSubscribeCategory));
                LocalSettings.Settings.AllSubscribeOpened = false;
                break;
            }
          }
        }
        else
          source = itemsSource.Where<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (m => !(m is TeamGroupViewModel) && m.TeamId == section.TeamId));
        if (source == null)
          return;
        foreach (ProjectItemViewModel projectItemViewModel in source.ToList<ProjectItemViewModel>())
          itemsSource.Remove(projectItemViewModel);
      }
    }

    public ProjectIdentity GetSelectedProject()
    {
      if (this.SelectedItem != null)
        return this.SelectedItem.GetIdentity();
      string selectedProject = Utils.FindParent<IListViewParent>((DependencyObject) this)?.GetSelectedProject();
      this.SelectedItem = ProjectItemViewModel.BuildProject(selectedProject);
      return ProjectIdentity.BuildProject(selectedProject);
    }

    public void SetSelectedItemHighlighted()
    {
      this.ClearLastSelected();
      ProjectItemViewModel selectedItem1 = this.GetSelectedItem();
      if (selectedItem1 != null)
        selectedItem1.Selected = true;
      else if (this.SelectedItem is BindCalendarAccountProjectViewModel selectedItem3)
      {
        if (CacheManager.GetAccountCalById(selectedItem3.Id) != null)
          return;
        this.SetAndSelectDefaultProject();
      }
      else
      {
        if (!(this.SelectedItem is SubscribeCalendarProjectViewModel selectedItem2) || CacheManager.GetSubscribeDict().ContainsKey(selectedItem2.Id))
          return;
        this.SetAndSelectDefaultProject();
      }
    }

    public void SetAndSelectDefaultProject()
    {
      ProjectModel projectById = CacheManager.GetProjectById(TaskDefaultDao.GetDefaultSafely().ProjectId);
      this.SelectedItem = projectById == null || projectById.Isinbox ? (ProjectItemViewModel) new ticktick_WPF.ViewModels.InboxProjectViewModel((SmartProject) new InboxProject()) : (ProjectItemViewModel) new NormalProjectViewModel(projectById);
      Utils.FindParent<IListViewParent>((DependencyObject) this)?.SaveSelectedProject(this.SelectedItem?.GetSaveIdentity());
      this.SetSelectedItemHighlighted();
      EventHandler projectSelected = this.ProjectSelected;
      if (projectSelected == null)
        return;
      projectSelected((object) this, (EventArgs) null);
    }

    private void ClearLastSelected()
    {
      foreach (ProjectItemViewModel projectItem in (Collection<ProjectItemViewModel>) this.ProjectItems)
      {
        if (projectItem.Selected)
          projectItem.Selected = false;
      }
    }

    private ProjectItemViewModel GetSelectedItem()
    {
      if (this.ProjectItems == null || this.ProjectItems.Count <= 0 || this.SelectedItem == null)
        return (ProjectItemViewModel) null;
      ticktick_WPF.ViewModels.ProjectGroupViewModel group = this.SelectedItem as ticktick_WPF.ViewModels.ProjectGroupViewModel;
      return group != null ? this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p is ticktick_WPF.ViewModels.ProjectGroupViewModel && !string.IsNullOrEmpty(p.Id) && p.Id == group.Id)) : this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => !string.IsNullOrEmpty(p.Id) && p.Id == this.SelectedItem.Id));
    }

    private async void OnProjectSaved(object sender, ticktick_WPF.ViewModels.ProjectViewModel project)
    {
      ProjectListView sender1 = this;
      ListViewContainer.ReloadProjectData();
      if (project == null)
        return;
      ProjectModel projectById = await ProjectDao.GetProjectById(project.id);
      if (sender1.SelectedItem?.Id == project.id || project.IsNew)
      {
        sender1.SelectedItem = (ProjectItemViewModel) new NormalProjectViewModel(projectById);
        sender1.SetSelectedItemHighlighted();
      }
      EventHandler projectSelected = sender1.ProjectSelected;
      if (projectSelected != null)
        projectSelected((object) sender1, (EventArgs) null);
      ProjectWidgetsHelper.OnProjectChanged((ProjectIdentity) new NormalProjectIdentity(projectById));
      SyncManager.Sync();
    }

    public void ClearDropSelected()
    {
      if (this.ProjectItems == null || !this.ProjectItems.Any<ProjectItemViewModel>())
        return;
      this.ProjectItems.ToList<ProjectItemViewModel>().ForEach((Action<ProjectItemViewModel>) (model => model.DropSelected = false));
    }

    private void ClearLastSelected(ProjectItemViewModel selectedItem)
    {
      foreach (ProjectItemViewModel projectItem in (Collection<ProjectItemViewModel>) this.ProjectItems)
      {
        if (projectItem != selectedItem && projectItem.Selected)
          projectItem.Selected = false;
      }
    }

    public void SelectInbox()
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (item => SpecialListUtils.IsInboxProject(item.Id)));
      if (projectItemViewModel != null)
        projectItemViewModel.Selected = true;
      this.OnProjectClick((ProjectItemViewModel) new ticktick_WPF.ViewModels.InboxProjectViewModel((SmartProject) new InboxProject()), false);
    }

    public void SelectToday()
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (item => SpecialListUtils.IsTodayProject(item.Id)));
      if (projectItemViewModel != null)
        projectItemViewModel.Selected = true;
      this.OnProjectClick((ProjectItemViewModel) new ticktick_WPF.ViewModels.SmartProjectViewModel((SmartProject) new TodayProject()), false);
    }

    public bool SelectSmartList(SmartListType smartType)
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (item => SpecialListUtils.IsTargetSmartList(smartType, item.Id)));
      this.OnProjectClick((ProjectItemViewModel) ticktick_WPF.ViewModels.SmartProjectViewModel.BuildModel(smartType), false);
      if (projectItemViewModel == null)
        return false;
      projectItemViewModel.Selected = true;
      this._projectList.ScrollIntoView((object) projectItemViewModel);
      return true;
    }

    public void SelectTrash()
    {
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (item => SpecialListUtils.IsTrashProject(item.Id)));
      if (projectItemViewModel == null)
        return;
      this.OnProjectClick((ProjectItemViewModel) new ticktick_WPF.ViewModels.SmartProjectViewModel((SmartProject) new TrashProject()), false);
      projectItemViewModel.Selected = true;
    }

    public void SetSelectedItem(ProjectIdentity identity)
    {
      this.SelectedItem = ProjectItemViewModel.BuildProject(identity);
      this.SetSelectedItemHighlighted();
    }

    private void OnGroupNameTextButtonUp(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
    }

    public async void OnPinClick(string id, int type, bool isPinned)
    {
      if (isPinned)
      {
        ProjectPinSortOrderModel pinSortOrderModel = await ProjectPinSortOrderService.Insert(id, type);
        this.LoadPinModels();
      }
      else
      {
        await ProjectPinSortOrderService.Delete(id, type);
        this.LoadPinModels();
      }
    }

    public void OnLogOut() => this.SelectedItem = (ProjectItemViewModel) null;

    public bool TrySelectSmartProject(SmartListType smartListType)
    {
      this.SelectedItem = (ProjectItemViewModel) null;
      return this.SelectSmartList(smartListType);
    }

    public bool IsNoteProjectSelected()
    {
      return this.SelectedItem?.GetIdentity()?.IsNote.GetValueOrDefault();
    }

    private void OnItemBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      if (Mouse.LeftButton != MouseButtonState.Pressed)
        return;
      e.Handled = true;
    }

    public void ReSelectCurrent()
    {
      if (!(this.SelectedItem is NormalProjectViewModel selectedItem))
        return;
      ProjectModel projectById = CacheManager.GetProjectById(selectedItem.Id);
      if (projectById != null && !projectById.delete_status)
      {
        EventHandler projectSelected = this.ProjectSelected;
        if (projectSelected == null)
          return;
        projectSelected((object) this, (EventArgs) null);
      }
      else
      {
        if (selectedItem.Project.userid == LocalSettings.Settings.LoginUserId)
          Utils.Toast(Utils.GetString("ProjectNotFoundTips"));
        this.SelectInbox();
      }
    }

    public void SetItemCount(ProjectIdentity identity, int count)
    {
      string id = identity is SmartProjectIdentity smartProjectIdentity ? smartProjectIdentity.QueryId : identity.Id;
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (i => i.Id == id));
      if (projectItemViewModel == null)
        return;
      projectItemViewModel.Count = count;
    }

    public void NavigateProject(string type, string id)
    {
      ProjectIdentity projectId = (ProjectIdentity) null;
      if (type != null)
      {
        switch (type.Length)
        {
          case 3:
            if (type == "tag")
            {
              this.SelectTagProject(id.ToLower());
              return;
            }
            break;
          case 4:
            if (type == "date")
            {
              projectId = (ProjectIdentity) new DateProjectIdentity(id);
              break;
            }
            break;
          case 5:
            switch (type[0])
            {
              case 'g':
                if (type == "group")
                {
                  projectId = ProjectIdHelper.GetGroupIdentity(id);
                  break;
                }
                break;
              case 's':
                if (type == "smart")
                {
                  projectId = (ProjectIdentity) SmartProjectIdentity.BuildSmartProject(id);
                  break;
                }
                break;
            }
            break;
          case 6:
            if (type == "filter")
            {
              projectId = ProjectIdHelper.GetFilterIdentity(id);
              break;
            }
            break;
          case 7:
            if (type == "project")
            {
              projectId = ProjectIdHelper.GetProjectIdentity(id);
              break;
            }
            break;
          case 8:
            if (type == "calendar")
            {
              projectId = ProjectIdHelper.GetCalendarIdentity(id);
              break;
            }
            break;
        }
      }
      if (projectId == null)
        return;
      this.SelectProject(projectId);
    }

    public async Task DuplicateFilter(FilterModel filter)
    {
      filter = CacheManager.GetFilterById(filter?.id);
      if (filter == null)
        return;
      FilterModel filter1 = filter.Copy();
      filter1.id = Utils.GetGuid();
      filter1.name = filter1.name + " " + Utils.GetString("ProjectCopy");
      filter1.syncStatus = 0;
      filter1.sortOrder = ProjectDragHelper.CalculateInsertFilterSortOrder(false, filter.id);
      await FilterDao.AddFilter(filter1);
      SyncManager.Sync();
    }

    public void ShowOrHidePtf(PtfAllViewModel model, int index)
    {
      if (model.IsTag)
        LocalSettings.Settings.SmartListTag = index;
      if (model.IsFilter)
        LocalSettings.Settings.ShowCustomSmartList = index;
      ListViewContainer.ReloadProjectData();
    }

    public bool ProjectDragging { get; set; }

    private void MenuMouseDown(object sender, MouseButtonEventArgs e)
    {
      ProjectPinView pinView = this.GetPinView();
      // ISSUE: explicit non-virtual call
      if ((pinView != null ? (__nonvirtual (pinView.IsMouseOver) ? 1 : 0) : 0) != 0)
      {
        this._dragStartTarget = (ProjectItemViewModel) null;
      }
      else
      {
        this.MouseMove -= new MouseEventHandler(this.MenuMouseMove);
        this._dragStartY = (long) e.GetPosition((IInputElement) App.Window).Y;
        this._dragStartTarget = this.GetDragTarget((MouseEventArgs) e);
        this.MouseMove += new MouseEventHandler(this.MenuMouseMove);
      }
    }

    private async void MenuMouseMove(object sender, MouseEventArgs e)
    {
      ProjectListView relativeTo = this;
      double x = e.GetPosition((IInputElement) relativeTo).X;
      long y = (long) e.GetPosition((IInputElement) App.Window).Y;
      double actualWidth = relativeTo.ActualWidth;
      if (e.LeftButton != MouseButtonState.Pressed || relativeTo._dragStatus != DragStatus.Dragging && Math.Abs(y - relativeTo._dragStartY) <= 2L)
        return;
      if (relativeTo._dragStartTarget != null)
      {
        relativeTo.TryStartDrag(e);
        relativeTo.TryMoveItem(e);
      }
      else
      {
        if (relativeTo.SelectedItem is NormalProjectViewModel selectedItem && TeamDao.IsTeamExpired(selectedItem.Project.teamId))
          Utils.Toast(Utils.GetString("TeamExpiredOperate"));
        // ISSUE: explicit non-virtual call
        relativeTo.MouseMove -= new MouseEventHandler(relativeTo.MenuMouseMove);
      }
    }

    private void TryStartDrag(MouseEventArgs e)
    {
      if (this._dragStartY == -1L)
        return;
      long num = Math.Abs((long) e.GetPosition((IInputElement) App.Window).Y - this._dragStartY);
      if (this._dragStatus != DragStatus.Dragging && num <= 5L)
        return;
      this.OnDragStart(e);
    }

    private void TryMoveItem(MouseEventArgs e)
    {
      this.TryShowLine();
      ListBoxItem pointListBoxItem = this.GetMousePointListBoxItem(e);
      if (pointListBoxItem == null)
        return;
      if (pointListBoxItem.DataContext is ProjectItemViewModel dataContext)
      {
        if (!this.CheckHoverModelEnable(dataContext))
          return;
        this.TryShowChildLine(dataContext);
        this._hoverIndex = this.ProjectItems.IndexOf(dataContext);
        if (!object.Equals((object) this._dragHoverItem, (object) pointListBoxItem))
          this._dragHoverItem = pointListBoxItem;
        if (this._hoverIndex == this._currentIndex)
          return;
        if (this._hoverModel != dataContext)
        {
          if (this._hoverModel != null)
          {
            this._hoverModel.DropSelected = false;
            this._hoverModel.Hover = false;
          }
          this._hoverModel = dataContext;
        }
        System.Windows.Point position = e.GetPosition((IInputElement) this._dragHoverItem);
        if (!(this._hoverModel is EmptySubViewModel) && position.Y > this._dragHoverItem.ActualHeight / 3.0 && position.Y < this._dragHoverItem.ActualHeight * 2.0 / 3.0)
        {
          if (!this.CanDropCreateGroup(this._draggingModel, this._hoverModel))
            return;
          this._hoverModel.DropSelected = true;
        }
        else if (this._hoverIndex < this._currentIndex && (position.Y > 0.0 && position.Y < this._dragHoverItem.ActualHeight / 3.0 || this._hoverModel is EmptySubViewModel && position.Y > 0.0 && position.Y < this._dragHoverItem.ActualHeight * 3.0 / 4.0) && ProjectDragHelper.CanSwapItems(this._draggingModel, this._hoverModel, false))
        {
          this._previousOverGroup = (ProjectItemViewModel) null;
          this.SwapItems(this._hoverIndex, this._draggingModel);
          this._currentIndex = this._hoverIndex;
          this._hoverModel.DropSelected = false;
        }
        else if (this._hoverIndex > this._currentIndex && (position.Y > this._dragHoverItem.ActualHeight * 2.0 / 3.0 && position.Y < this._dragHoverItem.ActualHeight || this._hoverModel is EmptySubViewModel && position.Y > this._dragHoverItem.ActualHeight / 4.0 && position.Y < this._dragHoverItem.ActualHeight) && ProjectDragHelper.CanSwapItems(this._draggingModel, this._hoverModel, true))
        {
          this._previousOverGroup = (ProjectItemViewModel) null;
          this.SwapItems(this._hoverIndex, this._draggingModel);
          this._currentIndex = this._hoverIndex;
          this._hoverModel.DropSelected = false;
        }
        else
        {
          this._previousOverGroup = (ProjectItemViewModel) null;
          this._hoverModel.DropSelected = false;
        }
      }
      else
        this.ClearDropSelected();
    }

    private ListBoxItem GetMousePointListBoxItem(MouseEventArgs e)
    {
      HitTestResult hitTestResult = VisualTreeHelper.HitTest((Visual) this._projectList, e.GetPosition((IInputElement) this._projectList));
      return hitTestResult != null ? Utils.FindParent<ListBoxItem>(hitTestResult.VisualHit) : (ListBoxItem) null;
    }

    private bool CheckHoverModelEnable(ProjectItemViewModel hoverModel)
    {
      switch (this._draggingModel)
      {
        case ticktick_WPF.ViewModels.ProjectGroupViewModel _:
        case NormalProjectViewModel _:
          if (this._draggingModel.TeamId != hoverModel.TeamId)
            return false;
          if (this._draggingModel is NormalProjectViewModel draggingModel && !draggingModel.Project.IsValid())
            return hoverModel is NormalProjectViewModel projectViewModel1 && !projectViewModel1.Project.IsValid();
          switch (hoverModel)
          {
            case ClosedProjectGroupViewModel _:
              return false;
            case NormalProjectViewModel projectViewModel2:
              return projectViewModel2.Project.IsValid();
            case ticktick_WPF.ViewModels.ProjectGroupViewModel _:
              return true;
            case EmptySubViewModel emptySubViewModel1:
              return !emptySubViewModel1.IsEmptyTag;
          }
          break;
        case ticktick_WPF.ViewModels.SmartProjectViewModel _:
          return hoverModel is ticktick_WPF.ViewModels.SmartProjectViewModel;
        case TagProjectViewModel _:
          switch (hoverModel)
          {
            case TagProjectViewModel _:
              return true;
            case EmptySubViewModel emptySubViewModel2:
              return emptySubViewModel2.IsEmptyTag;
            default:
              return false;
          }
        case FilterProjectViewModel _:
          return hoverModel is FilterProjectViewModel;
        case PtfAllViewModel _:
          return hoverModel is PtfAllViewModel;
      }
      return false;
    }

    private void TryShowLine()
    {
      try
      {
        if (!(this._draggingModel is NormalProjectViewModel) && !(this._draggingModel is ticktick_WPF.ViewModels.ProjectGroupViewModel) && !(this._draggingModel is TagProjectViewModel))
          return;
        if (this.ProjectItems.Count > this._currentIndex - 1 && this._currentIndex > 0 && this.ProjectItems[this._currentIndex - 1] is EmptySubViewModel projectItem2)
        {
          if (this._lastHoverLine == null)
            this._lastHoverLine = projectItem2;
          else if (!this._lastHoverLine.Equals((object) projectItem2))
          {
            this._lastHoverLine.Hover = false;
            this._lastHoverLine = projectItem2;
          }
          this._lastHoverLine.Hover = true;
        }
        else if (this.ProjectItems.Count > this._currentIndex + 1 && this.ProjectItems[this._currentIndex + 1] is EmptySubViewModel projectItem1)
        {
          if (this._lastHoverLine == null)
            this._lastHoverLine = projectItem1;
          else if (!this._lastHoverLine.Equals((object) projectItem1))
          {
            this._lastHoverLine.Hover = false;
            this._lastHoverLine = projectItem1;
          }
          this._lastHoverLine.Hover = true;
        }
        else
        {
          if (this._lastHoverLine == null)
            return;
          this._lastHoverLine.Hover = false;
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void TryShowChildLine(ProjectItemViewModel hoverModel)
    {
      if (!(this._draggingModel is ticktick_WPF.ViewModels.ProjectGroupViewModel) && (!(this._draggingModel is TagProjectViewModel draggingModel) || !draggingModel.IsParent))
        return;
      string lineParent = "";
      switch (hoverModel)
      {
        case ticktick_WPF.ViewModels.ProjectGroupViewModel projectGroupViewModel:
          lineParent = projectGroupViewModel.Id;
          break;
        case ticktick_WPF.ViewModels.SubProjectViewModel projectViewModel1:
          lineParent = projectViewModel1.Project.groupId;
          break;
        case TagProjectViewModel projectViewModel2:
          if (projectViewModel2.IsParent || projectViewModel2.IsSubItem)
          {
            lineParent = projectViewModel2.IsParent ? projectViewModel2.Title : projectViewModel2.TagModel.parent;
            break;
          }
          break;
      }
      if (string.IsNullOrEmpty(lineParent))
        return;
      if (this._lastHoverLine != null)
      {
        if (this._lastHoverLine.ParentId != lineParent)
          this._lastHoverLine.Hover = false;
        else if (this._lastHoverLine.Hover)
          return;
      }
      if (!(this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p is EmptySubViewModel emptySubViewModel1 && emptySubViewModel1.ParentId == lineParent)) is EmptySubViewModel emptySubViewModel2))
        return;
      this._lastHoverLine = emptySubViewModel2;
      this._lastHoverLine.Hover = true;
    }

    private bool CanDropCreateGroup(ProjectItemViewModel model, ProjectItemViewModel target)
    {
      switch (model)
      {
        case NormalProjectViewModel _:
          if (this._hoverIndex >= 0 && this._hoverIndex < this.ProjectItems.Count)
          {
            switch (target)
            {
              case NormalProjectViewModel _:
                if (!(target is ticktick_WPF.ViewModels.SubProjectViewModel))
                  return true;
                break;
              case ticktick_WPF.ViewModels.ProjectGroupViewModel groupVm1:
                if (!groupVm1.Open)
                {
                  this.OnGroupDragOver((ProjectItemViewModel) groupVm1);
                  return false;
                }
                break;
            }
          }
          else
            break;
          break;
        case TagProjectViewModel projectViewModel:
          if (!projectViewModel.IsParent && this._hoverIndex >= 0 && this._hoverIndex < this.ProjectItems.Count && target is TagProjectViewModel groupVm2 && !groupVm2.IsSubItem && projectViewModel.InSubSection == groupVm2.InSubSection)
          {
            if (!groupVm2.IsParent)
              return true;
            if (groupVm2.IsParent && !groupVm2.Open)
            {
              this.OnGroupDragOver((ProjectItemViewModel) groupVm2);
              return false;
            }
            break;
          }
          break;
      }
      return false;
    }

    private ProjectItemViewModel GetDragTarget(MouseEventArgs e)
    {
      if (this.ActualWidth - e.GetPosition((IInputElement) this).X <= 16.0)
        return (ProjectItemViewModel) null;
      ListBoxItem pointListBoxItem = this.GetMousePointListBoxItem(e);
      if (pointListBoxItem == null)
        return (ProjectItemViewModel) null;
      if (!(pointListBoxItem.DataContext is ProjectItemViewModel dataContext) || !dataContext.CanDrag)
        return (ProjectItemViewModel) null;
      this._dragStartIndex = this.ProjectItems.IndexOf(dataContext);
      this._currentIndex = this._dragStartIndex;
      return dataContext;
    }

    private double GetDragVerticalOffset(MouseEventArgs e)
    {
      return e.GetPosition((IInputElement) this._projectList).Y - 24.0;
    }

    private void OnDragStart(MouseEventArgs e)
    {
      switch (this._dragStatus)
      {
        case DragStatus.Waiting:
          this.LogDragStartLocation(e);
          break;
        case DragStatus.Trying:
          this.StartDragging(e);
          break;
        case DragStatus.Dragging:
          this.UpdateDraggingLocation(e);
          break;
      }
    }

    private void LogDragStartLocation(MouseEventArgs e)
    {
      this._dragStartOffset = this.GetDragVerticalOffset(e);
      this._dragStatus = DragStatus.Trying;
    }

    private void UpdateDraggingLocation(MouseEventArgs e)
    {
      this._projectDragPopup.VerticalOffset = this.GetDragVerticalOffset(e);
    }

    private void StartDragging(MouseEventArgs e)
    {
      if (Math.Abs(this.GetDragVerticalOffset(e) - this._dragStartOffset) < 5.0)
        return;
      this._draggingModel = this._dragStartTarget;
      if (this._draggingModel == null)
        return;
      this._draggingModel.DragSelected = true;
      if (this._draggingModel is ticktick_WPF.ViewModels.ProjectGroupViewModel draggingModel1 && draggingModel1.ProjectGroup.open)
      {
        draggingModel1.Icon = Utils.GetIcon("IcClosedFolder");
        draggingModel1.ProjectGroup.open = false;
        ProjectGroupDao.TrySaveProjectGroup(draggingModel1.ProjectGroup, true);
        draggingModel1.Open = false;
        this.NotifyGroupOpenChanged((ProjectItemViewModel) draggingModel1);
      }
      if (this._draggingModel is TagProjectViewModel draggingModel2 && draggingModel2.Open)
      {
        draggingModel2.Open = false;
        draggingModel2.TagModel.collapsed = true;
        this.NotifyGroupOpenChanged((ProjectItemViewModel) draggingModel2);
      }
      if (this._draggingModel is PtfAllViewModel draggingModel3)
        this.FoldAllPtfSection(draggingModel3);
      this.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(this.ProjectMenuMouseUp);
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ProjectMenuMouseUp);
      Mouse.Capture((IInputElement) this._projectList);
      this._projectDragPopup.DataContext = (object) this._draggingModel;
      this._projectDragPopup.VerticalOffset = this.GetDragVerticalOffset(e);
      this._projectDragPopup.PlacementTarget = (UIElement) this._projectList;
      this._projectDragPopup.IsOpen = true;
      if (this._projectDragPopup.Child == null)
      {
        Popup projectDragPopup = this._projectDragPopup;
        ProjectDragPopupContent dragPopupContent = new ProjectDragPopupContent();
        dragPopupContent.Width = this.ActualWidth - 20.0;
        projectDragPopup.Child = (UIElement) dragPopupContent;
      }
      App.IsProjectOrGroupDragging = true;
      this._dragStatus = DragStatus.Dragging;
    }

    private void FoldAllPtfSection(PtfAllViewModel ptf)
    {
      List<ProjectItemViewModel> list = this.ProjectItems.ToList<ProjectItemViewModel>();
      list.RemoveAll(new Predicate<ProjectItemViewModel>(ProjectDataProvider.IsProjectCategory));
      list.RemoveAll(new Predicate<ProjectItemViewModel>(ProjectDataProvider.IsTagCategory));
      list.RemoveAll(new Predicate<ProjectItemViewModel>(ProjectDataProvider.IsFilterCategory));
      list.RemoveAll(new Predicate<ProjectItemViewModel>(ProjectDataProvider.IsSubscribeCategory));
      ptf.Open = false;
      switch (ptf.Type)
      {
        case PtfType.Project:
          LocalSettings.Settings.AllProjectOpened = false;
          break;
        case PtfType.Tag:
          LocalSettings.Settings.AllTagOpened = false;
          break;
        case PtfType.Filter:
          LocalSettings.Settings.AllFilterOpened = false;
          break;
        case PtfType.Subscribe:
          LocalSettings.Settings.AllSubscribeOpened = false;
          break;
      }
      ItemsSourceHelper.SetItemsSource<ProjectItemViewModel>((ItemsControl) this._projectList, list);
    }

    private void SwapItems(int index, ProjectItemViewModel model)
    {
      if (this.ProjectItems.Count <= index)
        return;
      this.ProjectItems.Remove(model);
      this.ProjectItems.Insert(index, model);
    }

    private void ProjectMenuMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this._projectDragPopup.IsOpen)
        e.Handled = true;
      this._dragStartTarget = (ProjectItemViewModel) null;
      this.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(this.ProjectMenuMouseUp);
      Mouse.Capture((IInputElement) null);
      this.OnDrop();
    }

    private async void OnDrop()
    {
      ProjectListView sender = this;
      sender._projectDragPopup.IsOpen = false;
      App.IsProjectOrGroupDragging = false;
      sender._dragStatus = DragStatus.Waiting;
      bool needReload = false;
      PtfAllViewModel draggingPtf = (PtfAllViewModel) null;
      try
      {
        if (sender._lastHoverLine != null)
        {
          sender._lastHoverLine.Hover = false;
          sender._lastHoverLine = (EmptySubViewModel) null;
        }
        if (sender._draggingModel == null)
        {
          sender.ResetDrag();
          draggingPtf = (PtfAllViewModel) null;
          return;
        }
        if (sender._hoverModel != null && sender._hoverModel.DropSelected && sender._draggingModel != null && !(sender._draggingModel is PtfAllViewModel))
        {
          sender._draggingModel.DragSelected = false;
          if (sender._draggingModel is TagProjectViewModel draggingModel2 && sender._hoverModel is TagProjectViewModel hoverModel2)
          {
            await sender.TryCreatTagGroup(draggingModel2, hoverModel2);
            UtilLog.Info("ProjectMenu.DragDrop : newTag");
          }
          else if (sender._hoverModel is ticktick_WPF.ViewModels.ProjectGroupViewModel hoverModel1 && sender._draggingModel is NormalProjectViewModel draggingModel1)
          {
            if (sender._hoverModel is ClosedProjectGroupViewModel)
            {
              sender.ResetDrag();
              draggingPtf = (PtfAllViewModel) null;
              return;
            }
            ProjectModel project = draggingModel1.Project;
            if (project != null)
            {
              string str = string.Format("originGid {0},originOrder {1}", (object) project.groupId, (object) project.sortOrder);
              project.groupId = hoverModel1.Id;
              project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
              project.sortOrder = ProjectDragHelper.GetNewSortOrder(hoverModel1.Id, project.teamId);
              UtilLog.Info(string.Format("ProjectMenu.DragDrop : project {0},group {1},order {2},{3}", (object) project.id, (object) hoverModel1.Id, (object) project.sortOrder, (object) str));
              int num = await ProjectDao.TryUpdateProject(project);
            }
            sender.ProjectItems.Remove(sender._draggingModel);
            needReload = true;
          }
          else
            needReload = !await sender.CreateNewProjectGroupOnDrop(sender._hoverModel);
        }
        else if (sender._hoverModel != null)
        {
          if (sender._dragStartIndex == sender._currentIndex && !(sender._draggingModel is PtfAllViewModel))
          {
            sender._draggingModel.DragSelected = false;
            draggingPtf = (PtfAllViewModel) null;
            return;
          }
          ProjectItemViewModel draggingModel = sender._draggingModel;
          switch (draggingModel)
          {
            case ticktick_WPF.ViewModels.SmartProjectViewModel _:
              sender.SaveSmartProjectSortOrder(sender._hoverModel);
              needReload = true;
              break;
            case FilterProjectViewModel filterProjectViewModel:
              await sender.SaveFilterSortOrder(sender._hoverModel, filterProjectViewModel);
              break;
            case TagProjectViewModel tag:
              if (!tag.IsParent)
                await sender.SaveTagSortOrder(tag, sender._hoverModel);
              else
                await sender.SortTagParent(tag);
              needReload = true;
              break;
            default:
              if (!(draggingModel is NormalProjectViewModel normal))
              {
                switch (draggingModel)
                {
                  case ticktick_WPF.ViewModels.ProjectGroupViewModel group:
                    await sender.SaveProjectGroupSortOrder(sender._hoverModel, group);
                    needReload = true;
                    break;
                  case PtfAllViewModel ptfAllViewModel:
                    draggingPtf = ptfAllViewModel;
                    bool isTeam = draggingPtf.IsTeam;
                    await sender.SaveProjectSectionSortOrder(isTeam);
                    needReload = true;
                    break;
                }
              }
              else
              {
                int num1 = sender.ProjectItems.IndexOf(sender._hoverModel);
                ProjectItemViewModel hoverModel = sender._hoverModel;
                await sender.SaveProjectSortOrder(num1 < sender._currentIndex, normal.Project, hoverModel);
                NormalProjectViewModel projectViewModel = normal;
                ProjectModel project = normal.Project;
                int num2 = project != null ? (project.InGroup() ? 1 : 0) : 0;
                projectViewModel.IsSubItem = num2 != 0;
                needReload = true;
                break;
              }
              break;
          }
          normal = (NormalProjectViewModel) null;
        }
        sender._draggingModel.DragSelected = false;
        SyncManager.Sync();
      }
      catch (Exception ex)
      {
        needReload = true;
      }
      ProjectIdentity identity = sender.SelectedItem?.GetIdentity();
      if (identity?.SortOption != null && (sender._draggingModel is NormalProjectViewModel && identity.SortOption.ContainsSortType(Constants.SortType.project.ToString()) || sender._draggingModel is TagProjectViewModel && identity.SortOption.ContainsSortType(Constants.SortType.tag.ToString())))
      {
        EventHandler projectSelected = sender.ProjectSelected;
        if (projectSelected != null)
          projectSelected((object) sender, (EventArgs) null);
      }
      sender.ResetDrag();
      if (needReload)
      {
        await Task.Delay(10);
        ListViewContainer.ReloadProjectData();
      }
      if (draggingPtf == null)
      {
        draggingPtf = (PtfAllViewModel) null;
      }
      else
      {
        sender.ScrollToPtf(draggingPtf);
        draggingPtf = (PtfAllViewModel) null;
      }
    }

    private async void ScrollToPtf(PtfAllViewModel draggingPtf)
    {
      await Task.Delay(100);
      ProjectItemViewModel projectItemViewModel = this.ProjectItems.FirstOrDefault<ProjectItemViewModel>((Func<ProjectItemViewModel, bool>) (p => p is PtfAllViewModel ptfAllViewModel && ptfAllViewModel.Type == draggingPtf.Type));
      if (projectItemViewModel == null)
        ;
      else
        this._projectList.ScrollIntoView((object) projectItemViewModel);
    }

    private async Task SaveProjectSectionSortOrder(bool dragTeam)
    {
      List<PtfAllViewModel> list = this.ProjectItems.OfType<PtfAllViewModel>().ToList<PtfAllViewModel>();
      string empty = string.Empty;
      foreach (PtfAllViewModel ptfAllViewModel in list)
        empty += ptfAllViewModel.Title;
      UtilLog.Info(string.Format("ProjectMenu.DragDrop : projectSection {0} , {1}", (object) empty, (object) dragTeam));
      if (dragTeam)
        list.RemoveAll((Predicate<PtfAllViewModel>) (p => p.IsProject && !p.IsTeam));
      else
        list.RemoveAll((Predicate<PtfAllViewModel>) (p => p.IsProject && p.IsTeam));
      ProjectDataProvider.SetNewSortOrders((IEnumerable<PtfAllViewModel>) list);
    }

    private void ResetDrag()
    {
      this._dragStartTarget = (ProjectItemViewModel) null;
      this._draggingModel = (ProjectItemViewModel) null;
      this._dragStartY = -1L;
      this._currentIndex = -1;
      this._dragStartIndex = -1;
      this._direction = DragDirection.Idle;
      this._currentIdentity = string.Empty;
      this._projectDragPopup.Child = (UIElement) null;
    }

    private async Task TryCreatTagGroup(
      TagProjectViewModel dragTagProject,
      TagProjectViewModel dropTagProject)
    {
      ProjectListView projectListView = this;
      if (dropTagProject.IsSubItem)
        ;
      else if (dropTagProject.IsParent)
      {
        if (dropTagProject.Open)
          ;
        else
        {
          dropTagProject.DropSelected = false;
          dragTagProject.TagModel.parent = dropTagProject.TagModel.name;
          dragTagProject.TagModel.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(true, dropTagProject.TagModel.name);
          dragTagProject.IsSubItem = true;
          projectListView.ProjectItems.Remove((ProjectItemViewModel) dragTagProject);
          dropTagProject.Children.Insert(0, (ProjectItemViewModel) dragTagProject);
          TagModel tagByName = CacheManager.GetTagByName(dragTagProject.TagModel.name);
          if (tagByName == null)
            ;
          else
          {
            tagByName.parent = dragTagProject.TagModel.parent;
            tagByName.sortOrder = dragTagProject.TagModel.sortOrder;
            tagByName.status = tagByName.status != 0 ? 1 : 0;
            await TagDao.UpdateTag(tagByName);
          }
        }
      }
      else
      {
        TagModel tagParent = await TagDao.CreateTag("");
        if (tagParent == null)
          ;
        else
        {
          tagParent.name = Utils.GetString("NewTag");
          int val1 = projectListView.ProjectItems.IndexOf((ProjectItemViewModel) dropTagProject);
          ObservableCollection<ProjectItemViewModel> projectItems = projectListView.ProjectItems;
          int index = Math.Min(val1, projectListView._currentIndex);
          TagProjectViewModel projectViewModel = new TagProjectViewModel(tagParent);
          projectViewModel.IsParent = true;
          projectViewModel.IsNew = true;
          projectViewModel.InSubSection = dragTagProject.InSubSection;
          projectItems.Insert(index, (ProjectItemViewModel) projectViewModel);
          dropTagProject.IsSubItem = true;
          dropTagProject.DropSelected = false;
          dragTagProject.IsSubItem = true;
          tagParent.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(false, dropTagProject.TagModel.name);
          tagParent.sortType = Constants.SortType.tag.ToString();
          tagParent.SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.tag, false);
          projectListView._canReload = false;
          tagParent.type = dropTagProject.InSubSection ? 2 : 1;
          projectListView.Dispatcher.Invoke<Task>((Func<Task>) (async () =>
          {
            await Task.Delay(10);
            tagParent.name = "";
            this.TryAddTagParent(tagParent, dropTagProject.TagModel, dragTagProject.TagModel);
          }));
        }
      }
    }

    private async Task SortTagParent(TagProjectViewModel tag)
    {
      List<TagModel> tags = CacheManager.GetTags();
      List<TagModel> list = tags != null ? tags.Where<TagModel>((Func<TagModel, bool>) (t => t.name == tag.Id || t.parent == tag.Id)).OrderBy<TagModel, long>((Func<TagModel, long>) (t => t.sortOrder)).ToList<TagModel>() : (List<TagModel>) null;
      if (list == null)
        return;
      long num = 268435456L / (long) list.Count;
      if (this._hoverModel == null)
        return;
      ProjectItemViewModel projectItem1 = this.ProjectItems[this._currentIndex - 1];
      switch (projectItem1)
      {
        case TagProjectViewModel _:
        case EmptySubViewModel _:
          TagProjectViewModel projectViewModel1 = projectItem1 is TagProjectViewModel projectViewModel2 ? projectViewModel2 : this.ProjectItems[this._currentIndex - 2] as TagProjectViewModel;
          if (projectViewModel1 != null)
          {
            if (this.ProjectItems.Count > this._currentIndex + 1 && this.ProjectItems[this._currentIndex + 1] is TagProjectViewModel projectItem2)
              num = (projectItem2.TagModel.sortOrder - projectViewModel1.TagModel.sortOrder) / (long) (list.Count + 1);
            for (int index = 0; index < list.Count; ++index)
            {
              string str = string.Format("originOrder {0}", (object) list[index].sortOrder);
              list[index].sortOrder = projectViewModel1.TagModel.sortOrder + (long) (index + 1) * num;
              if (list[index].status != 0)
                list[index].status = 1;
              if (string.IsNullOrEmpty(list[index].parent) && list.Count > 1)
                list[index].collapsed = true;
              UtilLog.Info(string.Format("ProjectMenu.DragDrop : tag {0},order {1},{2}", (object) list[index].name, (object) list[index].sortOrder, (object) str));
              TagDao.UpdateTag(list[index]);
            }
            DataChangedNotifier.NotifyTagChanged(tag.TagModel);
            return;
          }
          break;
      }
      long tagMinSort = ProjectDragHelper.GetTagMinSort();
      for (int index = list.Count - 1; index >= 0; --index)
      {
        string str = string.Format("originOrder {0}", (object) list[index].sortOrder);
        list[index].sortOrder = tagMinSort - (long) (list.Count - index) * num;
        if (list[index].status != 0)
          list[index].status = 1;
        if (string.IsNullOrEmpty(list[index].parent))
          list[index].collapsed = true;
        UtilLog.Info(string.Format("ProjectMenu.DragDrop : tag {0},order {1},{2}", (object) list[index].name, (object) list[index].sortOrder, (object) str));
        TagDao.UpdateTag(list[index]);
      }
      DataChangedNotifier.NotifyTagChanged(tag.TagModel);
    }

    private void TryAddTagParent(TagModel tagParent, TagModel dropModel, TagModel dragModel)
    {
      AddOrEditTagWindow addOrEditTagWindow = new AddOrEditTagWindow(tagParent, isCreat: true);
      addOrEditTagWindow.Owner = Window.GetWindow((DependencyObject) this);
      addOrEditTagWindow.TagSaved += (EventHandler<TagModel>) (async (sender, tag) =>
      {
        TagModel tagByName1 = CacheManager.GetTagByName(dropModel.name);
        if (tagByName1 != null)
        {
          tagByName1.parent = tag.name;
          tagByName1.status = tagByName1.status != 0 ? 1 : 0;
          await TagDao.UpdateTag(tagByName1);
        }
        TagModel tagByName2 = CacheManager.GetTagByName(dragModel.name);
        if (tagByName2 != null)
        {
          tagByName2.parent = tag.name;
          tagByName2.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(true, dropModel.name);
          tagByName2.status = tagByName2.status != 0 ? 1 : 0;
          await TagDao.UpdateTag(tagByName2);
        }
        this._canReload = true;
        ListViewContainer.ReloadProjectData();
        SyncManager.TryDelaySync();
      });
      addOrEditTagWindow.Cancel += (EventHandler) (async (sender, e) =>
      {
        this._canReload = true;
        this.LoadData();
      });
      addOrEditTagWindow.ShowDialog();
    }

    private async Task SaveProjectGroupSortOrder(
      ProjectItemViewModel dropItem,
      ticktick_WPF.ViewModels.ProjectGroupViewModel group)
    {
      ProjectGroupModel projectGroupById = await ProjectGroupDao.GetProjectGroupById(group.ProjectGroup.id);
      if (projectGroupById == null)
        return;
      string id = string.Empty;
      switch (dropItem)
      {
        case NormalProjectViewModel projectViewModel:
          id = projectViewModel.Project.id;
          break;
        case ticktick_WPF.ViewModels.ProjectGroupViewModel projectGroupViewModel:
          id = projectGroupViewModel.ProjectGroup.id;
          break;
        case EmptySubViewModel emptySubViewModel:
          id = emptySubViewModel.ParentId;
          break;
      }
      if (string.IsNullOrEmpty(id))
        return;
      bool upDown = this.ProjectItems.IndexOf(dropItem) < this._currentIndex;
      projectGroupById.sortOrder = new long?(ProjectDragHelper.GetNewSortOrder(upDown, id, group.TeamId));
      if (projectGroupById.sync_status != Constants.SyncStatus.SYNC_NEW.ToString())
        projectGroupById.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
      await ProjectGroupDao.TrySaveProjectGroup(projectGroupById);
    }

    private async Task SaveProjectSortOrder(
      bool upDown,
      ProjectModel project,
      ProjectItemViewModel target)
    {
      project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == project.id)) ?? project;
      string origin = string.Format("originGid {0},originOrder {1}", (object) project.groupId, (object) project.sortOrder);
      string oldGroupId;
      switch (target)
      {
        case ticktick_WPF.ViewModels.SubProjectViewModel projectViewModel1:
          long sortOrder1 = project.sortOrder;
          oldGroupId = project.groupId;
          if (projectViewModel1.Project.closed.GetValueOrDefault())
          {
            project.sortOrder = ProjectDragHelper.GetNewSortOrderInClosed(upDown, projectViewModel1.Project.id, projectViewModel1.TeamId) ?? project.sortOrder;
          }
          else
          {
            project.groupId = projectViewModel1.Project.groupId;
            project.sortOrder = ProjectDragHelper.GetNewSortOrder(upDown, projectViewModel1.Project.groupId, projectViewModel1.Project.id, projectViewModel1.TeamId);
          }
          long sortOrder2 = project.sortOrder;
          if (sortOrder1 != sortOrder2 || oldGroupId != project.groupId)
          {
            project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
            int num = await ProjectDao.TryUpdateProject(project);
          }
          if (oldGroupId != project.groupId && project.IsShareList())
          {
            ProjectGroupDao.CheckGroupGroupBy(oldGroupId);
            break;
          }
          break;
        case NormalProjectViewModel projectViewModel2:
          project.groupId = "NONE";
          project.sortOrder = ProjectDragHelper.GetNewSortOrder(upDown, projectViewModel2.Id, projectViewModel2.TeamId);
          project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          int num1 = await ProjectDao.TryUpdateProject(project);
          break;
        case ticktick_WPF.ViewModels.ProjectGroupViewModel projectGroupViewModel:
          oldGroupId = project.groupId;
          if (projectGroupViewModel.Open)
          {
            if (upDown)
            {
              project.groupId = projectGroupViewModel.ProjectGroup.id;
              project.sortOrder = ProjectDragHelper.GetNewSortOrder(true, projectGroupViewModel.ProjectGroup.id, false, projectGroupViewModel.TeamId);
              project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
            }
            else
            {
              project.groupId = "NONE";
              project.sortOrder = ProjectDragHelper.GetNewSortOrder(false, projectGroupViewModel.ProjectGroup.id, false, projectGroupViewModel.TeamId);
              project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
            }
          }
          else
          {
            project.groupId = "NONE";
            project.sortOrder = ProjectDragHelper.GetNewSortOrder(upDown, projectGroupViewModel.Id, projectGroupViewModel.TeamId);
            project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          }
          int num2 = await ProjectDao.TryUpdateProject(project);
          if (oldGroupId != project.groupId && project.IsShareList())
          {
            ProjectGroupDao.CheckGroupGroupBy(oldGroupId);
            break;
          }
          break;
        case EmptySubViewModel emptySubViewModel:
          project.groupId = upDown ? "NONE" : emptySubViewModel.ProjectGroup.id;
          project.sortOrder = ProjectDragHelper.GetNewSortOrder(upDown, emptySubViewModel.ProjectGroup.id, true, emptySubViewModel.TeamId);
          project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          int num3 = await ProjectDao.TryUpdateProject(project);
          break;
      }
      UtilLog.Info(string.Format("ProjectMenu.DragDrop : projectSort {0},group {1},order {2},{3}", (object) project.id, (object) project.groupId, (object) project.sortOrder, (object) origin));
      origin = (string) null;
    }

    private async Task SaveTagSortOrder(TagProjectViewModel tag, ProjectItemViewModel target)
    {
      if (tag.TagModel.status != 0)
        tag.TagModel.status = 1;
      string originParent = tag.TagModel.parent;
      int num = this.ProjectItems.IndexOf(target);
      string str = string.Format("originParent {0},originOrder {1}", (object) tag.TagModel.parent, (object) tag.TagModel.sortOrder);
      switch (target)
      {
        case EmptySubViewModel emptySubViewModel:
          if (num > this._currentIndex)
          {
            if (this.ProjectItems[this._currentIndex - 1] is TagProjectViewModel projectItem)
            {
              tag.IsSubItem = true;
              tag.TagModel.parent = emptySubViewModel.ParentId;
              tag.TagModel.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(true, projectItem.TagModel.name);
              break;
            }
            break;
          }
          if (num < this._currentIndex && this.ProjectItems[this._currentIndex - 2] is TagProjectViewModel projectItem1)
          {
            tag.IsSubItem = false;
            tag.TagModel.parent = "";
            tag.TagModel.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(true, projectItem1.TagModel.name);
            break;
          }
          break;
        case TagProjectViewModel projectViewModel:
          if (projectViewModel.IsParent)
          {
            tag.TagModel.parent = num > this._currentIndex ? "" : projectViewModel.TagModel.name;
            tag.IsSubItem = num < this._currentIndex;
            tag.TagModel.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(num < this._currentIndex, projectViewModel.TagModel.name);
            break;
          }
          tag.IsSubItem = projectViewModel.IsSubItem;
          tag.TagModel.parent = string.IsNullOrEmpty(projectViewModel.TagModel.parent) ? "" : projectViewModel.TagModel.parent;
          tag.TagModel.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(num < this._currentIndex, projectViewModel.TagModel.name);
          break;
      }
      UtilLog.Info(string.Format("ProjectMenu.DragDrop : tag {0},parent{1},order {2},{3}", (object) tag.Id, (object) tag.TagModel.parent, (object) tag.TagModel.sortOrder, (object) str));
      await TagDao.UpdateTag(tag.TagModel);
      if (!string.IsNullOrEmpty(originParent) && originParent != tag.TagModel.parent)
        TagDao.CheckParent(originParent);
      DataChangedNotifier.NotifyTagChanged(tag.TagModel);
      originParent = (string) null;
    }

    private async Task SaveFilterSortOrder(
      ProjectItemViewModel dropItem,
      FilterProjectViewModel filterProjectViewModel)
    {
      if (!(dropItem is FilterProjectViewModel projectViewModel))
        return;
      int num = this.ProjectItems.IndexOf((ProjectItemViewModel) projectViewModel);
      FilterModel filter = filterProjectViewModel.Filter;
      long sortOrder = filter.sortOrder;
      filter.sortOrder = ProjectDragHelper.CalculateInsertFilterSortOrder(num < this._currentIndex, projectViewModel.Filter.id);
      UtilLog.Info(string.Format("ProjectMenu.DragDrop : filter {0},order {1},originOrder {2}", (object) filter.id, (object) filter.sortOrder, (object) sortOrder));
      if (filter.syncStatus != 0)
        filter.syncStatus = 1;
      await FilterDao.UpdateFilter(filter);
    }

    private void SaveSmartProjectSortOrder(ProjectItemViewModel dropItem)
    {
      if (!(dropItem is ticktick_WPF.ViewModels.SmartProjectViewModel))
        return;
      for (int index = 0; index < this.ProjectItems.Count; ++index)
      {
        if (this.ProjectItems[index] is ticktick_WPF.ViewModels.SmartProjectViewModel projectItem)
          projectItem.Project.SortOrder = index * 1000;
      }
    }

    private async Task<bool> CreateNewProjectGroupOnDrop(ProjectItemViewModel targetModel)
    {
      ProjectGroupModel projectGroup = await this.CreateNewProjectGroup(targetModel);
      if (projectGroup == null)
        return true;
      int index = Math.Min(this.ProjectItems.Count, Math.Min(this._hoverIndex, this._currentIndex));
      ticktick_WPF.ViewModels.ProjectGroupViewModel group = new ticktick_WPF.ViewModels.ProjectGroupViewModel(projectGroup)
      {
        IsNew = true
      };
      this.ProjectItems.Insert(index, (ProjectItemViewModel) group);
      int num = await this.DowngradeProject(index, projectGroup, group) ? 1 : 0;
      UserActCollectUtils.AddClickEvent("project_list_ui", "folder", "add");
      this.OnRenameGroup(projectGroup);
      return num != 0;
    }

    private async Task<bool> DowngradeProject(
      int index,
      ProjectGroupModel projectGroup,
      ticktick_WPF.ViewModels.ProjectGroupViewModel group)
    {
      if (this.ProjectItems.Count > index + 2)
      {
        ProjectItemViewModel projectItem1 = this.ProjectItems[index + 1];
        ProjectItemViewModel projectItem2 = this.ProjectItems[index + 2];
        if (projectItem1 is NormalProjectViewModel projectViewModel1 && projectItem2 is NormalProjectViewModel projectViewModel2)
        {
          ProjectModel project = projectViewModel1.Project;
          ProjectModel nextProject = projectViewModel2.Project;
          this.ProjectItems[index + 1] = (ProjectItemViewModel) new ticktick_WPF.ViewModels.SubProjectViewModel(project);
          this.ProjectItems[index + 2] = (ProjectItemViewModel) new ticktick_WPF.ViewModels.SubProjectViewModel(nextProject);
          group.Children.Add(this.ProjectItems[index + 1]);
          group.Children.Add(this.ProjectItems[index + 2]);
          if (project != null)
          {
            project.groupId = projectGroup.id;
            project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
            int num = await ProjectDao.TryUpdateProject(project);
          }
          if (nextProject != null)
          {
            nextProject.groupId = projectGroup.id;
            nextProject.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
            int num = await ProjectDao.TryUpdateProject(nextProject);
          }
          return true;
        }
      }
      return false;
    }

    private async Task<ProjectGroupModel> CreateNewProjectGroup(ProjectItemViewModel targetModel)
    {
      if (targetModel == null)
        return (ProjectGroupModel) null;
      string identity = string.Empty;
      string teamId = string.Empty;
      if (!(targetModel is ticktick_WPF.ViewModels.ProjectGroupViewModel projectGroupViewModel))
      {
        if (targetModel is NormalProjectViewModel projectViewModel)
        {
          identity = projectViewModel.Project.id;
          teamId = projectViewModel.Project.teamId;
        }
      }
      else
      {
        identity = projectGroupViewModel.ProjectGroup.id;
        teamId = projectGroupViewModel.ProjectGroup.teamId;
      }
      long insertSortOrder = ProjectDragHelper.CalculateInsertSortOrder(false, identity, teamId);
      ProjectGroupModel projectGroup = new ProjectGroupModel()
      {
        id = Utils.GetGuid(),
        name = Utils.GetString("NewFolder"),
        open = true,
        teamId = teamId,
        sync_status = Constants.SyncStatus.SYNC_NEW.ToString(),
        userId = int.Parse(LocalSettings.Settings.LoginUserId),
        sortOrder = new long?(insertSortOrder),
        sortType = "project",
        SortOption = new SortOption()
        {
          groupBy = "project",
          orderBy = "dueDate"
        }
      };
      await ProjectGroupDao.TrySaveProjectGroup(projectGroup);
      return projectGroup;
    }

    public void OnTaskDragging(DragMouseEvent position) => this.SetTaskDragTarget(position);

    private void SetTaskDragTarget(DragMouseEvent position)
    {
      Rect rect = new Rect(this._projectList.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) Application.Current?.MainWindow), this._projectList.TranslatePoint(new System.Windows.Point(this._projectList.ActualWidth, this._projectList.ActualHeight), (UIElement) Application.Current?.MainWindow));
      System.Windows.Point point = new System.Windows.Point(position.X, position.Y);
      ProjectItemViewModel projectItemViewModel = (ProjectItemViewModel) null;
      if (rect.Contains(point))
      {
        int dropProjectIndex = this.GetDropProjectIndex(new System.Windows.Point(position.X, position.Y));
        if (dropProjectIndex >= 0 && dropProjectIndex < this.ProjectItems.Count)
          projectItemViewModel = this.ProjectItems[dropProjectIndex];
        if (dropProjectIndex != this._lastDropIndex)
        {
          if (this._lastDropIndex >= 0 && this._lastDropIndex < this.ProjectItems.Count)
            this.ProjectItems[this._lastDropIndex].DropSelected = false;
          if (projectItemViewModel != null && projectItemViewModel.CanDrop)
          {
            if (position.Data is DisplayItemModel data && data.IsSection && !(projectItemViewModel is NormalProjectViewModel))
            {
              this._lastDropIndex = -1;
              return;
            }
            projectItemViewModel.DropSelected = true;
          }
          this._lastDropIndex = dropProjectIndex;
        }
      }
      else if (this._lastDropIndex >= 0 && this._lastDropIndex < this.ProjectItems.Count)
      {
        this.ProjectItems[this._lastDropIndex].DropSelected = false;
        this._lastDropIndex = -1;
      }
      if (projectItemViewModel is ticktick_WPF.ViewModels.ProjectGroupViewModel groupVm && !groupVm.Open)
        this.OnGroupDragOver((ProjectItemViewModel) groupVm);
      else
        this._previousOverGroup = (ProjectItemViewModel) null;
    }

    private async Task OnGroupDragOver(ProjectItemViewModel groupVm)
    {
      if (groupVm == this._previousOverGroup)
        return;
      this._previousOverGroup = groupVm;
      await Task.Delay(240);
      if (groupVm != this._previousOverGroup)
        return;
      this._previousOverGroup = (ProjectItemViewModel) null;
      if (groupVm.Open)
        return;
      groupVm.Open = true;
      this.OnOpenGroupClick(groupVm);
    }

    private IDroppable GetDroppable()
    {
      if (this._lastDropIndex >= 0 && this._lastDropIndex < this.ProjectItems.Count && this.ProjectItems[this._lastDropIndex].DropSelected)
      {
        this.ProjectItems[this._lastDropIndex].DropSelected = false;
        if (this.ProjectItems[this._lastDropIndex] is IDroppable projectItem && projectItem.CanDrop)
          return projectItem;
      }
      return (IDroppable) null;
    }

    private int GetDropProjectIndex(System.Windows.Point point)
    {
      for (int index = 0; index < this.ProjectItems.Count; ++index)
      {
        ListViewItem listViewItem = Utils.GetListViewItem(this._projectList, index);
        if (listViewItem != null)
        {
          System.Windows.Point point1 = listViewItem.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) Application.Current?.MainWindow);
          System.Windows.Point point2 = listViewItem.TranslatePoint(new System.Windows.Point(listViewItem.ActualWidth, listViewItem.ActualHeight), (UIElement) Application.Current?.MainWindow);
          point2.X = point2.X;
          if (new Rect(point1, point2).Contains(point))
            return index;
        }
      }
      return 0;
    }

    public async Task<bool> OnBatchTaskDropped(List<string> taskIds)
    {
      ProjectListView child = this;
      IDroppable droppable = child.GetDroppable();
      if (droppable == null)
        return false;
      Utils.FindParent<ListViewContainer>((DependencyObject) child)?.HideDetail();
      if (!string.IsNullOrEmpty(droppable.ProjectId))
      {
        ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == droppable.ProjectId));
        if (project != null)
        {
          List<TaskBaseViewModel> tasks = TaskCache.GetTaskAndChildrenInBatch(taskIds);
          await TaskDao.RemoveTaskParentIdInBatch(TaskNodeUtils.GetTaskNodeTree(tasks).Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (node => !node.HasParent)).Select<Node<TaskBaseViewModel>, string>((Func<Node<TaskBaseViewModel>, string>) (node => node.Value.Id)).ToList<string>());
          List<string> ids = tasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>();
          await TaskService.BatchMoveProject(ids, new MoveProjectArgs(project));
          TaskModel thinTaskById = await TaskDao.GetThinTaskById(ids.FirstOrDefault<string>());
          TaskService.TryToastMoveControl(child.SelectedItem.GetIdentity(), thinTaskById, project.id, true);
          tasks = (List<TaskBaseViewModel>) null;
          ids = (List<string>) null;
        }
        project = (ProjectModel) null;
      }
      if (droppable.Priority != 0)
      {
        int num1 = await TaskService.BatchSetPriority(taskIds, droppable.Priority) ? 1 : 0;
      }
      DateTime? nullable = droppable.DefaultDate;
      if (nullable.HasValue)
      {
        foreach (string taskId1 in taskIds)
        {
          TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId1);
          int num2;
          if (taskById == null)
          {
            num2 = 0;
          }
          else
          {
            nullable = taskById.StartDate;
            num2 = nullable.HasValue ? 1 : 0;
          }
          bool hasStartDate = num2 != 0;
          string taskId2 = taskId1;
          nullable = droppable.DefaultDate;
          DateTime newDate = nullable.Value;
          bool? isAllDay = new bool?();
          await TaskService.SetDate(taskId2, newDate, false, isAllDay: isAllDay);
          if (!hasStartDate)
            await TaskService.SaveTaskReminders(new TaskModel()
            {
              id = taskId1,
              reminders = TimeData.GetDefaultAllDayReminders().ToArray()
            });
        }
        List<string> stringList = taskIds;
        ticktick_WPF.ViewModels.SmartProjectViewModel smart;
        // ISSUE: explicit non-virtual call
        if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          smart = droppable as ticktick_WPF.ViewModels.SmartProjectViewModel;
          if (smart != null && (smart.Id == "_special_id_tomorrow" || smart.Id == "_special_id_today"))
            App.Window.TryToastMoveControl(await TaskDao.GetThinTaskById(taskIds[0]), smart.Id == "_special_id_today", taskIds.Count > 1);
        }
        smart = (ticktick_WPF.ViewModels.SmartProjectViewModel) null;
      }
      if (droppable.Tags != null && droppable.Tags.Any<string>())
        await TaskService.BatchAddTag(taskIds, droppable.Tags[0]);
      if (droppable.IsCompleted)
      {
        int num3 = await TaskService.BatchCompleteTasks(taskIds) ? 1 : 0;
      }
      if (droppable.IsAbandoned)
        await TaskService.BatchAbandonTasks(taskIds);
      if (droppable.IsDeleted)
      {
        int num4 = await TaskService.BatchDeleteTaskByIds(taskIds, false) ? 1 : 0;
      }
      return true;
    }

    public IDroppable GetTaskDropTarget() => this.GetDroppable();

    public async Task<bool> OnDragTaskDropped(string taskId)
    {
      ProjectListView child = this;
      if (child.Visibility != Visibility.Visible)
        return false;
      IDroppable droppable = child.GetDroppable();
      if (droppable != null && droppable.CanDrop)
      {
        Utils.FindParent<ListViewContainer>((DependencyObject) child)?.HideDetail();
        TaskModel task = await TaskDao.GetThinTaskById(taskId);
        if (task != null)
        {
          if (droppable.Multiple)
          {
            await ProjectListView.SetTaskMultiProperty(task, droppable);
          }
          else
          {
            if (droppable.Tags != null && droppable.Tags.Any<string>())
              Utils.Toast(string.Format(Utils.GetString("MoveTaskToTag"), (object) task.title, (object) TagDataHelper.GetTagDisplayName(droppable.Tags[0])));
            else if (!string.IsNullOrEmpty(droppable.ProjectId))
            {
              ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == droppable.ProjectId));
              if (projectModel != null && task.projectId == projectModel.id && (droppable is NormalProjectViewModel || droppable is ticktick_WPF.ViewModels.InboxProjectViewModel))
                return false;
            }
            await TaskDragHelper.SetTaskProperty(task, droppable, Utils.FindParent<ListViewContainer>((DependencyObject) child));
          }
          return !string.IsNullOrEmpty(droppable.ProjectId) && droppable.ProjectId != task.projectId;
        }
        task = (TaskModel) null;
      }
      return false;
    }

    private static async Task SetTaskMultiProperty(TaskModel task, IDroppable droppable)
    {
      string projectId = task.projectId;
      task.projectId = droppable.ProjectId;
      task.priority = droppable.Priority;
      task.tags = droppable.Tags.ToArray();
      DateTime? nullable1 = droppable.DefaultDate;
      if (nullable1.HasValue)
      {
        TaskModel taskModel1 = task;
        nullable1 = droppable.DefaultDate;
        DateTime? nullable2 = new DateTime?(nullable1.Value);
        taskModel1.startDate = nullable2;
        task.reminders = TimeData.GetDefaultAllDayReminders().ToArray();
        task.isAllDay = new bool?(true);
        TaskModel taskModel2 = task;
        nullable1 = new DateTime?();
        DateTime? nullable3 = nullable1;
        taskModel2.dueDate = nullable3;
      }
      await TaskService.SetTaskProperties(task, projectId);
    }

    public async void OnKanbanBatchTaskDropped(List<string> taskIds)
    {
      int num = await this.OnBatchTaskDropped(taskIds) ? 1 : 0;
    }

    private async void OnCheckItemDrop(object sender, string checkItemId)
    {
      IDroppable droppable = this.GetDroppable();
      if (droppable == null)
        droppable = (IDroppable) null;
      else if (!droppable.CanDrop)
      {
        droppable = (IDroppable) null;
      }
      else
      {
        TaskDetailItemModel subTask = await TaskDetailItemDao.GetChecklistItemById(checkItemId);
        if (subTask != null)
        {
          if (droppable.IsCompleted)
            await TaskDetailItemService.CompleteCheckItem(checkItemId);
          else if (droppable.IsDeleted)
          {
            TaskModel taskModel = await TaskService.DeleteCheckItem(checkItemId);
          }
          else
          {
            TaskPrimaryProperty dropProperty = SubtaskToTaskHelper.GetProjectDefaultProperty((IProjectTaskDefault) this.ProjectItems[this._lastDropIndex].GetIdentity());
            TaskModel primaryTask = await TaskDao.GetTaskById(subTask.TaskServerId);
            TimeData timeData1 = new TimeData();
            if (!Utils.IsEmptyDate(subTask.startDate))
            {
              timeData1.StartDate = subTask.startDate;
              timeData1.IsAllDay = subTask.isAllDay;
              timeData1.RepeatFlag = primaryTask.repeatFlag;
              timeData1.RepeatFrom = primaryTask.repeatFrom;
              bool? isAllDay = subTask.isAllDay;
              if (isAllDay.HasValue)
              {
                isAllDay = subTask.isAllDay;
                if (isAllDay.Value)
                {
                  timeData1.Reminders = new List<TaskReminderModel>()
                  {
                    new TaskReminderModel() { trigger = "TRIGGER:P0DT9H0M0S" }
                  };
                  goto label_16;
                }
              }
              timeData1.Reminders = new List<TaskReminderModel>()
              {
                new TaskReminderModel() { trigger = "TRIGGER:PT0S" }
              };
            }
            else if (!Utils.IsEmptyDate(primaryTask.startDate))
            {
              timeData1.StartDate = primaryTask.startDate;
              timeData1.DueDate = primaryTask.dueDate;
              timeData1.IsAllDay = primaryTask.isAllDay;
              timeData1.RepeatFlag = primaryTask.repeatFlag;
              timeData1.RepeatFrom = primaryTask.repeatFrom;
              TimeData timeData2 = timeData1;
              timeData2.Reminders = await TaskReminderDao.GetRemindersByTaskId(primaryTask.id);
              timeData2 = (TimeData) null;
            }
label_16:
            TaskPrimaryProperty taskProperty = new TaskPrimaryProperty();
            taskProperty.Priority = new int?(primaryTask.priority);
            taskProperty.ProjectId = primaryTask.projectId;
            taskProperty.Tags = TagSerializer.ToTags(primaryTask.tag);
            taskProperty.TimeData = timeData1;
            TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
            TaskPrimaryProperty property = await SubtaskToTaskHelper.MergeTaskProperty(taskProperty, new TaskPrimaryProperty()
            {
              Priority = new int?(defaultSafely.Priority),
              ProjectId = Utils.GetInboxId(),
              TimeData = TimeData.BuildFromDefault(defaultSafely)
            }, dropProperty);
            if (!await ProChecker.CheckTaskLimit(property.ProjectId))
            {
              TaskModel task = await TaskService.SubtaskToTask(checkItemId, property);
            }
            dropProperty = (TaskPrimaryProperty) null;
            primaryTask = (TaskModel) null;
            timeData1 = (TimeData) null;
            property = (TaskPrimaryProperty) null;
          }
        }
        subTask = (TaskDetailItemModel) null;
        droppable = (IDroppable) null;
      }
    }

    private void OnItemDragging(object sender, DragMouseEvent mouseEvent)
    {
      this.SetTaskDragTarget(mouseEvent);
    }

    public async Task<bool> OnDragSectionDropped(string dragSectionId, string projectId)
    {
      ProjectListView projectListView = this;
      if (projectListView.Visibility != Visibility.Visible)
        return false;
      IDroppable droppable = projectListView.GetDroppable();
      return droppable != null && droppable.CanDrop && droppable is NormalProjectViewModel projectViewModel && !(projectId == projectViewModel.Id) && await TaskService.MoveColumnAsync(dragSectionId, projectId, projectViewModel.Id, true);
    }
  }
}
