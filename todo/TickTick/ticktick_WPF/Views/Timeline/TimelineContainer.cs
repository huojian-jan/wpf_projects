// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineContainer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineContainer : UserControl, IComponentConnector
  {
    private readonly TimelineViewModel _model = new TimelineViewModel();
    private ProjectIdentity _projectIdentity;
    private bool _loaded;
    private bool _dayWidthChanging;
    private readonly DispatcherTimer _moveViewTimer = new DispatcherTimer(DispatcherPriority.Render);
    private static readonly Dictionary<string, DateTime> _projectDays = new Dictionary<string, DateTime>();
    private bool _projectScrollChanged;
    private bool _addTaskMouseDown;
    private bool _moved;
    private System.Windows.Point _prePoint;
    private bool _ignoreMouseUp;
    private CancellationTokenSource _ignoreMouseUpTaskToken;
    private int _line;
    private bool _batchMouseDown;
    private System.Windows.Point _batchStartPoint;
    private TimelineCellViewModel _currentModel;
    private double _offset = -1.0;
    private FrameworkElement _hoveredCell;
    private double _initWidth;
    internal TimelineContainer Root;
    internal TimelineNavBar TimelineNavBar;
    internal ScrollViewer YearScroll;
    internal StackPanel OptionPanel;
    internal Border SwitchRangeBorder;
    internal EscPopup SelectRangePopup;
    internal StackPanel SelectRangeStackPanel;
    internal ScrollViewer DayLineScroll;
    internal TimelineHeader Header;
    internal TimelineHeaderHover HoverHeader;
    internal Grid PrePageGrid;
    internal Grid NextPageGrid;
    internal ScrollViewer MainScroll;
    internal Grid MainGrid;
    internal TimelineBackground TimelineBackground;
    internal TimelineVirtualizedCanvas CellCanvas;
    internal Border BatchSelectBorder;
    internal TimelineCellFloating TimelineFloating;
    internal TimelineArrange ArrangePanel;
    internal Grid GroupBorder;
    internal ScrollViewer ColumnScroll;
    internal TimelineToolTip ToolTipControl;
    internal Popup ColumnDragPopup;
    internal Grid ColumnDropLine;
    private bool _contentLoaded;

    public TimelineContainer()
    {
      this.InitializeComponent();
      TimelineViewModel model = this._model;
      DateTime today = DateTime.Today;
      DateTime startDate = today.AddDays(-1.0);
      today = DateTime.Today;
      DateTime endDate = today.AddDays(1.0);
      model.MoveSpan(startDate, endDate);
      this.DataContext = (object) this._model;
      this.Loaded += new RoutedEventHandler(this.OnContainerLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnContainerUnloaded);
      this._moveViewTimer.Interval = TimeSpan.FromMilliseconds(20.0);
      this._moveViewTimer.Tick += new EventHandler(this.OnMoveViewTimerOnElapsed);
    }

    private void OnThemeChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "ThemeId") || this._model == null)
        return;
      this._model.IsDark = ThemeKey.IsDarkTheme(LocalSettings.Settings.ThemeId);
    }

    private void OnContainerUnloaded(object sender, RoutedEventArgs e)
    {
      this.SaveProjectPos(true);
      DataChangedNotifier.ProjectChanged -= new EventHandler(this.OnProjectChanged);
      DataChangedNotifier.ProjectColumnChanged -= new EventHandler<string>(this.OnColumnChanged);
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      DataChangedNotifier.TagChanged -= new EventHandler<TagModel>(this.OnTagChanged);
      DataChangedNotifier.ProjectGroupChanged -= new EventHandler<ProjectGroupModel>(this.OnProjectGroupChanged);
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.TasksChanged));
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "HideComplete");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnDisplayChanged), "ShowSubtasks");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnThemeChanged), "ThemeId");
      this._model.ClearCell();
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChanged), "");
      ticktick_WPF.Notifier.GlobalEventManager.TimelineSetChanged -= new EventHandler<string>(this.OnSettingsChanged);
    }

    private void OnDisplayChanged(object sender, PropertyChangedEventArgs e)
    {
      this._model?.OnLocalSettingsChanged(e.PropertyName);
    }

    private async void OnContainerLoaded(object sender, RoutedEventArgs e)
    {
      TimelineContainer timelineContainer = this;
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(timelineContainer.TasksChanged));
      DataChangedNotifier.ProjectChanged += new EventHandler(timelineContainer.OnProjectChanged);
      DataChangedNotifier.ProjectColumnChanged += new EventHandler<string>(timelineContainer.OnColumnChanged);
      DataChangedNotifier.TagChanged += new EventHandler<TagModel>(timelineContainer.OnTagChanged);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(timelineContainer.OnFilterChanged);
      DataChangedNotifier.ProjectGroupChanged += new EventHandler<ProjectGroupModel>(timelineContainer.OnProjectGroupChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) timelineContainer._model, new EventHandler<PropertyChangedEventArgs>(timelineContainer.OnPropertyChanged), "");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(timelineContainer.OnDisplayChanged), "HideComplete");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(timelineContainer.OnDisplayChanged), "ShowSubtasks");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(timelineContainer.OnThemeChanged), "ThemeId");
      ticktick_WPF.Notifier.GlobalEventManager.TimelineSetChanged += new EventHandler<string>(timelineContainer.OnSettingsChanged);
    }

    private void OnSettingsChanged(object sender, string e) => this._model.OnSettingChanged(e);

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "ProjectTitle":
          this.TimelineNavBar.SetTitle(this._model.ProjectTitle);
          break;
        case "ProjectEnable":
          this.TimelineNavBar.SetTitleEnable(this._model.ProjectIdentity is NormalProjectIdentity projectIdentity && this._model.ProjectEnable && projectIdentity.Project != null && !projectIdentity.Project.Isinbox || this._model.ProjectIdentity is GroupProjectIdentity || this._model.ProjectIdentity is FilterProjectIdentity);
          break;
      }
    }

    private async void TasksChanged(object sender, TasksChangeEventArgs e)
    {
      TimelineContainer timelineContainer = this;
      if (!timelineContainer.IsVisible)
        return;
      timelineContainer._model?.OnTasksChanged(e);
    }

    private async void OnProjectGroupChanged(object sender, ProjectGroupModel e)
    {
      TimelineContainer child = this;
      if (!(child._model.ProjectIdentity is GroupProjectIdentity group))
        group = (GroupProjectIdentity) null;
      else if (!(group.CatId == e.id))
      {
        group = (GroupProjectIdentity) null;
      }
      else
      {
        ProjectGroupModel groupModel = CacheManager.GetGroupById(group.GroupId);
        if (groupModel != null && groupModel.viewMode == "list")
        {
          await Task.Delay(500);
          if (!child.IsVisible)
          {
            group = (GroupProjectIdentity) null;
          }
          else
          {
            group.SetGroup(groupModel);
            ListViewContainer parent = Utils.FindParent<ListViewContainer>((DependencyObject) child);
            if (parent == null)
            {
              group = (GroupProjectIdentity) null;
            }
            else
            {
              parent.SelectProject((ProjectIdentity) group);
              group = (GroupProjectIdentity) null;
            }
          }
        }
        else
        {
          if (groupModel?.Timeline != null)
          {
            if (!child._model.TimelineSortOption.Equal(groupModel.Timeline.sortOption))
              child._model.TimelineSortOption = groupModel.Timeline.sortOption;
            if (groupModel.Timeline.Range != child._model.TimelineRange)
            {
              DateTime day = child.GetDay();
              child._model.TimelineRange = groupModel.Timeline?.Range;
              await child.GotoDay(day);
            }
            if (groupModel.name != child._model.ProjectTitle)
              child._model.SetProjectTitle(groupModel.name);
          }
          groupModel = (ProjectGroupModel) null;
          group = (GroupProjectIdentity) null;
        }
      }
    }

    private async void OnTagChanged(object sender, TagModel e)
    {
      TimelineContainer child = this;
      if (!(child._model.ProjectIdentity is TagProjectIdentity projectIdentity))
        return;
      TagModel tag = CacheManager.GetTagByName(projectIdentity.Id);
      if (tag == null)
        return;
      if (tag.viewMode == "list")
      {
        await Task.Delay(500);
        if (!child.IsVisible)
          return;
        Utils.FindParent<ListViewContainer>((DependencyObject) child)?.SelectProject((ProjectIdentity) new TagProjectIdentity(tag));
      }
      else
      {
        if (tag.Timeline != null)
        {
          if (tag.Timeline.sortOption != null && !child._model.TimelineSortOption.Equal(tag.Timeline.sortOption))
            child._model.TimelineSortOption = tag.Timeline.sortOption;
          if (tag.Timeline.Range != child._model.TimelineRange)
          {
            DateTime day = child.GetDay();
            child._model.TimelineRange = tag.Timeline?.Range;
            await child.GotoDay(day);
          }
          if (tag.GetDisplayName() != child._model.ProjectTitle)
            child._model.SetProjectTitle(tag.GetDisplayName());
        }
        tag = (TagModel) null;
      }
    }

    private async void OnFilterChanged(object sender, FilterChangeArgs e)
    {
      TimelineContainer child = this;
      if (e.RuleChanged && !string.IsNullOrEmpty(e.Filter.id))
        TimelineContainer._projectDays.Remove(e.Filter.id);
      if (!string.IsNullOrEmpty(e.deleteId))
        TimelineContainer._projectDays.Remove(e.deleteId);
      if (!(child._model.ProjectIdentity?.Id == e.Filter?.id))
        return;
      FilterModel filter = CacheManager.GetFilterById(e.Filter?.id);
      if (filter != null && filter.viewMode == "list" && UserDao.IsPro())
      {
        await Task.Delay(500);
        if (!child.IsVisible)
          return;
        Utils.FindParent<ListViewContainer>((DependencyObject) child)?.SelectProject((ProjectIdentity) new FilterProjectIdentity(filter));
      }
      else
      {
        if (e.RuleChanged)
        {
          if (filter != null)
            child.LoadProjectInternal((ProjectIdentity) new FilterProjectIdentity(filter));
        }
        else if (filter?.Timeline != null)
        {
          if (filter.Timeline.sortOption != null && !child._model.TimelineSortOption.Equal(filter.Timeline.sortOption))
            child._model.TimelineSortOption = filter.Timeline.sortOption;
          if (filter.Timeline.Range != child._model.TimelineRange)
          {
            DateTime day = child.GetDay();
            child._model.TimelineRange = filter.Timeline?.Range;
            await child.GotoDay(day);
          }
          if (filter.name != child._model.ProjectTitle)
            child._model.SetProjectTitle(filter.name);
        }
        filter = (FilterModel) null;
      }
    }

    public async void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      e.Handled = true;
      if (Utils.IfCtrlPressed())
      {
        if (this._dayWidthChanging)
          return;
        this.ScrollChangeDayWidth(e.Delta >= 0 || this._model.TimelineRangeIndex >= 9 ? (e.Delta <= 0 || this._model.TimelineRangeIndex <= 1 ? 0 : -1) : 1);
      }
      else if (Utils.IfShiftPressed())
        this.MainScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset - (double) e.Delta);
      else
        this.MainScroll.ScrollToVerticalOffset(this.MainScroll.VerticalOffset - (double) e.Delta);
    }

    private async Task ScrollChangeDayWidth(int addIndex)
    {
      this._dayWidthChanging = true;
      if (addIndex != 0)
      {
        try
        {
          if (this.MainScroll.ActualWidth <= 0.0)
            return;
          TimelineInlineMarginConverter.IgnoreLeftOutSide = true;
          System.Windows.Point hoverPoint = Mouse.GetPosition((IInputElement) this.MainScroll);
          DateTime day = this._model.StartDate.AddDays((double) (int) ((this.MainScroll.HorizontalOffset + hoverPoint.X) / this._model.OneDayWidth));
          await this._model.SetRangIndex(this._model.TimelineRangeIndex + addIndex);
          await this.GotoDay(day, hoverPoint: hoverPoint);
          await this._model.UpdateCellLineAsync();
          this.TryCloseToolTips(true);
          await Task.Delay(100);
          TimelineInlineMarginConverter.IgnoreLeftOutSide = false;
          hoverPoint = new System.Windows.Point();
        }
        catch (Exception ex)
        {
          UtilLog.Warn("TimelineContainer.ChangDayWidthError " + ex.Message);
        }
      }
      await Task.Delay(150);
      this._dayWidthChanging = false;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (this._batchMouseDown)
        this.ShowBatchSelectBox(e);
      else if (e.LeftButton != MouseButtonState.Pressed)
      {
        this._addTaskMouseDown = false;
        this.StopMoveView();
      }
      else
        this.TryMoveCanvas(e);
    }

    private void TryMoveCanvas(MouseEventArgs e)
    {
      if (this.ToolTipControl.IsVisible)
      {
        this.MoveToolTips(e.GetPosition((IInputElement) this));
      }
      else
      {
        if (!this._addTaskMouseDown)
          return;
        System.Windows.Point position = e.GetPosition((IInputElement) this);
        double num1 = this._prePoint.X - position.X;
        double num2 = this._prePoint.Y - position.Y;
        if (Math.Abs(num2) < 2.0 && Math.Abs(num1) < 2.0)
          return;
        this.MainScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset + num1);
        if (num1 == 0.0 || Math.Abs(num2 / num1) > 0.6)
          this.MainScroll.ScrollToVerticalOffset(this.MainScroll.VerticalOffset + num2);
        this._prePoint = position;
        this._moved = true;
      }
    }

    public void DelayEndIgnoreMouseUp()
    {
      if (this._ignoreMouseUpTaskToken != null)
      {
        CancellationTokenSource mouseUpTaskToken = this._ignoreMouseUpTaskToken;
        this._ignoreMouseUpTaskToken = (CancellationTokenSource) null;
        mouseUpTaskToken.Cancel();
        mouseUpTaskToken.Dispose();
      }
      this._ignoreMouseUpTaskToken = new CancellationTokenSource();
      Task.Delay(500, this._ignoreMouseUpTaskToken.Token).ContinueWith((Action<Task>) (t =>
      {
        if (t.Status != TaskStatus.RanToCompletion)
          return;
        this._ignoreMouseUp = false;
      }), this._ignoreMouseUpTaskToken.Token);
    }

    public void StartIgnoreMouseUp()
    {
      this._ignoreMouseUpTaskToken?.Cancel();
      this._ignoreMouseUp = true;
    }

    private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      if (this._batchMouseDown)
        this.OnBatchMouseUp();
      else
        this.OnMoveMouseUp(sender, e);
    }

    private void OnMoveMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!this._ignoreMouseUp && !this._moved && this._model.ClearBatchSelect())
        Clear();
      else if (!PopupStateManager.CanShowAddPopup() || this._model.GroupEditing)
      {
        Clear();
      }
      else
      {
        if (sender is Grid && !this._moved && this._addTaskMouseDown && !this._ignoreMouseUp)
        {
          System.Windows.Point position = e.GetPosition((IInputElement) this.MainScroll);
          int dayOffsetByPoint = this.GetDayOffsetByPoint(position);
          DateTime dateTime = this._model.StartDate.AddDays((double) dayOffsetByPoint);
          DateTime? nullable = new DateTime?();
          if (this._model.NewTaskDefaultDays > 1)
            nullable = new DateTime?(dateTime.AddDays((double) this._model.NewTaskDefaultDays));
          int line = this.GetLineByPoint(position);
          int num = line;
          if (this._model.GroupDictModels.Any<KeyValuePair<int, bool>>())
            num = Math.Min(num, this._model.GroupDictModels.Keys.Max());
          bool flag;
          if (this._model.GroupDictModels.TryGetValue(num, out flag) && !flag)
          {
            Clear();
            return;
          }
          string projectId = this._model.ProjectIdentity?.GetProjectId();
          TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
          ProjectIdentity projectIdentity1 = this._model.ProjectIdentity;
          if ((projectIdentity1 != null ? (projectIdentity1.CanAddTask() ? 1 : 0) : 0) == 0)
          {
            Clear();
            return;
          }
          if (string.IsNullOrEmpty(projectId))
            projectId = defaultSafely.ProjectId;
          this._line = line;
          TaskBaseViewModel displayModel = new TaskBaseViewModel()
          {
            Id = Utils.GetGuid(),
            Kind = "TEXT",
            Title = string.Empty,
            Priority = defaultSafely.Priority,
            StartDate = new DateTime?(dateTime),
            DueDate = nullable,
            IsAllDay = new bool?(true)
          };
          if (this._model.ProjectIdentity is FilterProjectIdentity projectIdentity2)
          {
            displayModel.Priority = projectIdentity2.GetPriority();
            displayModel.SetTags(projectIdentity2.GetTags());
          }
          if (this._model.ProjectIdentity is TagProjectIdentity projectIdentity3)
            displayModel.SetTags(projectIdentity3.GetTags());
          else
            displayModel.SetTags(defaultSafely.Tags);
          switch (this._model.TimelineSortOption.groupBy)
          {
            case "tag":
              TimelineGroupViewModel timelineGroupViewModel1 = this._model.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= this._line));
              if (timelineGroupViewModel1 != null && !string.IsNullOrEmpty(timelineGroupViewModel1.Id) && timelineGroupViewModel1.Id != "!tag")
              {
                displayModel.SetTags(new List<string>()
                {
                  timelineGroupViewModel1.Title
                });
                break;
              }
              displayModel.SetTags(new List<string>());
              break;
            case "priority":
              TimelineGroupViewModel timelineGroupViewModel2 = this._model.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= this._line));
              if (timelineGroupViewModel2?.Id == "note")
              {
                displayModel.Kind = "NOTE";
                displayModel.Priority = 0;
                break;
              }
              int result;
              if (int.TryParse(timelineGroupViewModel2?.Id ?? "", out result))
              {
                displayModel.Priority = result;
                break;
              }
              break;
            case "assignee":
              TimelineGroupViewModel timelineGroupViewModel3 = this._model.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= this._line));
              if (timelineGroupViewModel3 != null)
              {
                displayModel.Assignee = timelineGroupViewModel3.Id;
                if (!string.IsNullOrEmpty(displayModel.Assignee) && displayModel.Assignee != "-1" && this._model.ProjectIdentity is GroupProjectIdentity projectIdentity4)
                {
                  string id = ProjectDao.GetAssignProjectInGroup(projectIdentity4.GroupId, displayModel.Assignee)?.id;
                  if (id != null)
                  {
                    projectId = id;
                    break;
                  }
                  break;
                }
                break;
              }
              break;
            case "sortOrder":
              TimelineGroupViewModel timelineGroupViewModel4 = this._model.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= line));
              if (timelineGroupViewModel4 != null)
              {
                displayModel.ColumnId = timelineGroupViewModel4.Id;
                break;
              }
              break;
            case "project":
              TimelineGroupViewModel timelineGroupViewModel5 = this._model.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= line));
              if (timelineGroupViewModel5 != null)
              {
                projectId = timelineGroupViewModel5.Id;
                break;
              }
              break;
          }
          ProjectModel projectById = CacheManager.GetProjectById(projectId);
          if (projectById == null || !projectById.IsEnable())
          {
            Clear();
            this.TryToastString(Utils.GetString("NoEditingPermission"));
            return;
          }
          displayModel.ProjectId = projectById.id;
          displayModel.Color = projectById.color;
          displayModel.SortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(projectId, isTop: new bool?(false));
          if (projectById.IsNote || this._model.ProjectIdentity is FilterProjectIdentity projectIdentity5 && projectIdentity5.FilterNote())
            displayModel.Kind = "NOTE";
          if (displayModel.IsNote)
          {
            displayModel.DueDate = new DateTime?();
            displayModel.Priority = 0;
          }
          this._model.AddCell(new TimelineCellViewModel(this._model, displayModel)
          {
            Line = line,
            Left = (double) dayOffsetByPoint * this._model.OneDayWidth,
            Operation = TimelineCellOperation.None,
            IsNew = true
          }, true);
        }
        Clear();
      }

      void Clear()
      {
        if (this._ignoreMouseUp)
          this._ignoreMouseUp = false;
        if (this._moved)
          UserActCollectUtils.AddClickEvent("timeline", "view_action", "drag_to_view");
        this._moved = false;
        this._addTaskMouseDown = false;
        if (!this._model.GroupEditing)
          return;
        this._model.SetGroupEditing(false);
      }
    }

    private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
    {
      if (sender is UIElement element)
        Mouse.Capture((IInputElement) element);
      if (!Utils.IfCtrlPressed())
      {
        if (PopupStateManager.CanShowAddPopup())
        {
          this._addTaskMouseDown = true;
          this._prePoint = e.GetPosition((IInputElement) this);
        }
      }
      else if (sender.Equals((object) this.MainGrid))
      {
        this.MainGrid.CaptureMouse();
        this._batchMouseDown = true;
        this.StartMoveView();
        this._batchStartPoint = e.GetPosition((IInputElement) this.MainGrid);
        this._prePoint = this._batchStartPoint;
      }
      this.TimelineFloating.Visibility = Visibility.Collapsed;
    }

    private async void OnMoveViewTimerOnElapsed(object s, EventArgs eventArgs)
    {
      ScrollViewer mainScroll = this.MainScroll;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) mainScroll);
      if (position.Y > mainScroll.ActualHeight)
        mainScroll.ScrollToVerticalOffset(mainScroll.VerticalOffset + (position.Y - mainScroll.ActualHeight));
      else if (position.Y < 0.0)
        mainScroll.ScrollToVerticalOffset(mainScroll.VerticalOffset + position.Y);
      double moveWidth = 0.0;
      double val2 = 30.0;
      if (this._model.ShowGroup && this.GroupBorder != null)
        val2 = Math.Max(this.GroupBorder.ActualWidth, val2);
      if (position.X < val2)
        moveWidth = Math.Min(100.0, (position.X - val2) / 10.0 * 2.0);
      else if (position.X > mainScroll.ActualWidth - 30.0)
        moveWidth = Math.Min(100.0, (position.X - mainScroll.ActualWidth + 30.0) / 10.0 * 2.0);
      if (moveWidth == 0.0)
      {
        mainScroll = (ScrollViewer) null;
      }
      else
      {
        if (this.TimelineFloating.Visibility == Visibility.Visible)
          await this.TimelineFloating.OnDragMoveOutSide();
        mainScroll.ScrollToHorizontalOffset(mainScroll.HorizontalOffset + moveWidth);
        mainScroll = (ScrollViewer) null;
      }
    }

    public void StartMoveView() => this._moveViewTimer.Start();

    public void StopMoveView() => this._moveViewTimer.Stop();

    private void ShowBatchSelectBox(MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this.MainGrid);
      position.X = Math.Min(Math.Max(0.0, position.X), this.MainGrid.ActualWidth);
      position.Y = Math.Min(Math.Max(0.0, position.Y), this.MainGrid.ActualHeight);
      double num1 = this._prePoint.X - position.X;
      if (Math.Abs(this._prePoint.Y - position.Y) < 2.0 && Math.Abs(num1) < 2.0)
        return;
      this._prePoint = position;
      if (this.BatchSelectBorder.Visibility == Visibility.Collapsed)
      {
        this.BatchSelectBorder.Visibility = Visibility.Visible;
        this.BatchSelectBorder.Margin = new Thickness(this._batchStartPoint.X, this._batchStartPoint.Y, 0.0, 0.0);
      }
      double num2 = position.X - this._batchStartPoint.X;
      double num3 = position.Y - this._batchStartPoint.Y;
      if (num2 < 0.0 || num3 < 0.0)
        this.BatchSelectBorder.Margin = new Thickness(num2 < 0.0 ? Math.Max(0.0, this._batchStartPoint.X + num2) : this._batchStartPoint.X, num3 < 0.0 ? Math.Max(0.0, this._batchStartPoint.Y + num3) : this._batchStartPoint.Y, 0.0, 0.0);
      this.BatchSelectBorder.Width = Math.Abs(num2);
      this.BatchSelectBorder.Height = Math.Abs(num3);
      if (this.BatchSelectBorder.Width <= 0.0)
        return;
      TimelineViewModel model = this._model;
      Thickness margin = this.BatchSelectBorder.Margin;
      double left = margin.Left;
      margin = this.BatchSelectBorder.Margin;
      double top = margin.Top;
      double width = this.BatchSelectBorder.Width;
      double height = this.BatchSelectBorder.Height;
      Rect rect = new Rect(left, top, width, height);
      model.TryBatchSelectOnMove(rect);
    }

    private void OnBatchMouseUp()
    {
      this._batchMouseDown = false;
      this.StopMoveView();
      this.BatchSelectBorder.Visibility = Visibility.Collapsed;
      this._model.ClearTempBatchSelect();
      this.BatchSelectBorder.Width = 0.0;
      this.BatchSelectBorder.Height = 0.0;
      this.BatchSelectBorder.Margin = new Thickness(0.0);
    }

    public async void ShowMenuPopup(TimelineCellViewModel model)
    {
      TimelineContainer ele = this;
      if (!model.BatchSelected)
        ele._model.ClearBatchSelect();
      else if (ele._model.BatchSelect)
      {
        ele.ShowBatchOperationDialog();
        ele.StartIgnoreMouseUp();
        return;
      }
      if (model.DisplayModel.IsEvent)
      {
        CalendarOperationDialog calendarOperationDialog = new CalendarOperationDialog(new EventArchiveArgs(model.DisplayModel));
        calendarOperationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((obj, kv) => this._model.RemoveCell(model, true));
        calendarOperationDialog.SetPlaceTarget((UIElement) ele);
        calendarOperationDialog.Show();
        ele.StartIgnoreMouseUp();
      }
      else if (!model.Editable)
        ;
      else
      {
        ele._currentModel = model;
        OperationExtra taskAccessInfo = await TaskOperationHelper.GetTaskAccessInfo(model.Id, false);
        if (taskAccessInfo == null)
          ;
        else
        {
          TaskOperationDialog taskOperationDialog = new TaskOperationDialog(taskAccessInfo, (UIElement) ele.TimelineNavBar.FoldGrid);
          taskOperationDialog.TimeClear += new EventHandler(ele.OnOperationTimeClear);
          taskOperationDialog.AbandonOrReopen += new EventHandler(ele.OnOperationAbandonOrReopen);
          taskOperationDialog.PrioritySelect += new EventHandler<int>(ele.OnOperationPrioritySelect);
          taskOperationDialog.ProjectSelect += new EventHandler<SelectableItemViewModel>(ele.OnOperationProjectSelect);
          taskOperationDialog.QuickDateSelect += new EventHandler<DateTime>(ele.OnOperationQuickDateSelect);
          taskOperationDialog.TimeSelect += new EventHandler<TimeData>(ele.OnOperationTimeSelect);
          taskOperationDialog.Deleted += new EventHandler(ele.OnOperationDeleted);
          taskOperationDialog.AssigneeSelect += new EventHandler<AvatarInfo>(ele.OnOperationAssigneeSelect);
          taskOperationDialog.TagsSelect += new EventHandler<TagSelectData>(ele.OnOperationTagsSelect);
          taskOperationDialog.Copied += new EventHandler(ele.OnOperationCopied);
          taskOperationDialog.LinkCopied += new EventHandler(ele.OnOperationLinkCopied);
          taskOperationDialog.SwitchTaskOrNote += new EventHandler(ele.OnOperationSwitchTaskOrNote);
          taskOperationDialog.Disappear += new EventHandler<bool>(ele.OnOperationDisappear);
          taskOperationDialog.SkipCurrentRecurrence += new EventHandler(ele.OnOperationSkipCurrentRecurrence);
          taskOperationDialog.Toast += new EventHandler<string>(ele.OnOperationToast);
          taskOperationDialog.CompleteDateChanged += (EventHandler<DateTime>) (async (o, date) =>
          {
            await TaskService.ChangeCompleteDate(model.Id, date);
            SyncManager.TryDelaySync();
          });
          taskOperationDialog.Show();
          ele.StartIgnoreMouseUp();
          TaskDetailWindow.TryCloseWindow(true);
        }
      }
    }

    private async void ShowBatchOperationDialog()
    {
      List<string> selectedIds = this._model.GetSelectedTaskIds();
      if (selectedIds.Count == 0)
        ;
      else
      {
        List<TaskModel> tasks = (List<TaskModel>) null;
        List<TaskDetailItemModel> checkItems = (List<TaskDetailItemModel>) null;
        tasks = await TaskDao.GetTaskAndChildrenInBatch(TaskDao.GetTreeTopIds(selectedIds, string.Empty));
        checkItems = await TaskDetailItemDao.GetCheckItemsInTaskIds((ICollection<string>) selectedIds);
        BatchTaskEditHelper batchHelper = new BatchTaskEditHelper((IBatchEditable) null)
        {
          ProjectIdentity = this._projectIdentity,
          SelectedTaskIds = selectedIds,
          UseInList = false
        };
        batchHelper.ShowOrHideOperation += (EventHandler<bool>) (async (sender, b) =>
        {
          if (b)
            return;
          this.DelayEndIgnoreMouseUp();
          batchHelper.Dispose();
        });
        batchHelper.CanUndo += (EventHandler<bool>) (async (sender, e) =>
        {
          await Task.Delay(200);
          bool show = false;
          if (this._projectIdentity is FilterProjectIdentity)
          {
            List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this._projectIdentity, selectedIds);
            List<TaskModel> taskModelList = tasks;
            // ISSUE: explicit non-virtual call
            if ((taskModelList != null ? (__nonvirtual (taskModelList.Count) > 0 ? 1 : 0) : 0) != 0 && matchedTasks.Count < selectedIds.Count)
              show = true;
          }
          if (!show && !e)
            return;
          UndoToast uiElement = new UndoToast((UndoController) new TaskUndo((TaskModel) null, string.Empty, Utils.GetString("TaskHasBeenFiltered"), tasks, checkItems));
          uiElement.SetVisible(show);
          Utils.FindParent<IToastShowWindow>((DependencyObject) this).Toast((FrameworkElement) uiElement);
        });
        batchHelper.ShowOperationDialog();
      }
    }

    private void OnOperationToast(object sender, string e) => Utils.Toast(e);

    private async void OnOperationSkipCurrentRecurrence(object sender, EventArgs e)
    {
      await this.UpdateTaskWithUndo(Skip(), this._currentModel.Id, this._currentModel.DisplayModel.Kind == "CHECKLIST");
      this._model.UpdateCellTime(this._currentModel);
      await this._model.UpdateCellLineAsync();

      async Task Skip()
      {
        await Task.Yield();
        TaskModel taskModel = await TaskService.SkipCurrentRecurrence(this._currentModel.Id, this._currentModel.DisplayModel.Kind == "CHECKLIST");
      }
    }

    private async void OnOperationSwitchTaskOrNote(object sender, EventArgs e)
    {
      await this.UpdateTaskWithUndo(Skip(), this._currentModel.Id, this._currentModel.DisplayModel.Kind == "CHECKLIST");
      this._model.UpdateCellTime(this._currentModel);
      await this._model.UpdateCellLineAsync();

      async Task Skip()
      {
        await Task.Delay(this._currentModel.DisplayModel.Kind == "CHECKLIST" ? 100 : 30);
        await TaskService.SwitchTaskOrNote(this._currentModel.Id);
      }
    }

    private async void OnOperationLinkCopied(object sender, EventArgs e)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(this._currentModel.Id);
      if (thinTaskById == null)
        return;
      TaskUtils.CopyTaskLink(thinTaskById.id, thinTaskById.projectId, thinTaskById.title);
    }

    private async void OnOperationCopied(object sender, EventArgs e)
    {
      TaskModel taskModel = await TaskService.CopyTask(this._currentModel.Id);
    }

    private async void OnOperationTagsSelect(object sender, TagSelectData tags)
    {
      await this.UpdateTaskWithUndo(SetTags(), this._currentModel.Id);
      if (this._model.GroupByEnum == Constants.SortType.tag)
      {
        await this._model.UpdateGroupAsync();
        await this._model.UpdateCellLineAsync();
      }
      this._model.UpdateCellTime(this._currentModel);
      await this._model.UpdateCellLineAsync();

      async Task SetTags()
      {
        await Task.Yield();
        await TaskService.SetTags(this._currentModel.Id, tags.OmniSelectTags);
      }
    }

    private async void OnOperationAssigneeSelect(object sender, AvatarInfo assignee)
    {
      await this.UpdateTaskWithUndo(SetAssignee(), this._currentModel.Id);
      if (this._model.GroupByEnum != Constants.SortType.assignee)
        return;
      await this._model.UpdateCellLineAsync();

      async Task SetAssignee()
      {
        await Task.Yield();
        await TaskService.SetAssignee(this._currentModel.Id, assignee.UserId);
      }
    }

    private async void OnOperationProjectSelect(object sender, SelectableItemViewModel e)
    {
      (string projectId, string columnId) = e.GetProjectAndColumnId();
      ProjectModel project = CacheManager.GetProjectById(projectId);
      if (project == null)
      {
        filter = (FilterProjectIdentity) null;
      }
      else
      {
        if (this._currentModel.DisplayModel.ProjectId == projectId)
        {
          if (string.IsNullOrEmpty(columnId))
          {
            filter = (FilterProjectIdentity) null;
            return;
          }
          if (this._currentModel.DisplayModel.ColumnId == columnId)
          {
            filter = (FilterProjectIdentity) null;
            return;
          }
        }
        this._model.RemoveCell(this._currentModel);
        await this._model.UpdateCellLineAsync();
        if (this._model.ProjectIdentity is FilterProjectIdentity filter)
        {
          TaskModel task = await TaskDao.GetTaskById(this._currentModel.Id);
          List<TaskModel> children = await TaskDao.GetAllSubTasksById(this._currentModel.Id, this._currentModel.DisplayModel.ProjectId);
          await Commit();
          if (TaskViewModelHelper.GetMatchedTasks((ProjectIdentity) filter, new List<string>()
          {
            task.id
          }).Count == 0)
            this.TryToastTaskChangeUndo(task, children);
          task = (TaskModel) null;
          children = (List<TaskModel>) null;
          filter = (FilterProjectIdentity) null;
        }
        else
        {
          await Commit();
          if (TaskViewModelHelper.GetMatchedTasks(this._model.ProjectIdentity, new List<string>()
          {
            this._currentModel.Id
          }).Count != 0)
          {
            filter = (FilterProjectIdentity) null;
          }
          else
          {
            IToastShowWindow toastWindow = Utils.GetToastWindow();
            if (toastWindow == null)
            {
              filter = (FilterProjectIdentity) null;
            }
            else
            {
              toastWindow.ToastMoveProjectControl(project.ProjectId);
              filter = (FilterProjectIdentity) null;
            }
          }
        }
      }

      async Task Commit()
      {
        string id = this._currentModel.Id;
        string projectId = project.ProjectId;
        string str = columnId;
        bool? isTop = new bool?();
        string columnId = str;
        await TaskService.MoveProject(id, projectId, isTop, columnId);
        if (!string.IsNullOrEmpty(columnId))
          await TaskService.SaveTaskColumnId(this._currentModel.Id, columnId);
        SyncManager.TryDelaySync();
      }
    }

    private async void OnOperationAbandonOrReopen(object sender, EventArgs e)
    {
      int status = this._currentModel.DisplayModel.Status == -1 ? 0 : -1;
      TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(this._currentModel.Id, status, status != 0, Utils.GetToastWindow());
      if (!this._model.HideCompleted || status == 0)
        return;
      this._model.RemoveCell(this._currentModel);
    }

    private async void OnOperationPrioritySelect(object sender, int e)
    {
      await this.UpdateTaskWithUndo(SetPriority(), this._currentModel.Id);
      this._model.UpdateCellTime(this._currentModel);
      await this._model.UpdateCellLineAsync();

      async Task SetPriority()
      {
        await Task.Yield();
        await TaskService.SetPriority(this._currentModel.Id, e);
      }
    }

    private void OnOperationDisappear(object sender, bool e)
    {
      if (sender is TaskOperationDialog taskOperationDialog)
      {
        taskOperationDialog.TimeClear -= new EventHandler(this.OnOperationTimeClear);
        taskOperationDialog.AbandonOrReopen -= new EventHandler(this.OnOperationAbandonOrReopen);
        taskOperationDialog.PrioritySelect -= new EventHandler<int>(this.OnOperationPrioritySelect);
        taskOperationDialog.ProjectSelect -= new EventHandler<SelectableItemViewModel>(this.OnOperationProjectSelect);
        taskOperationDialog.QuickDateSelect -= new EventHandler<DateTime>(this.OnOperationQuickDateSelect);
        taskOperationDialog.TimeSelect -= new EventHandler<TimeData>(this.OnOperationTimeSelect);
        taskOperationDialog.Deleted -= new EventHandler(this.OnOperationDeleted);
        taskOperationDialog.AssigneeSelect -= new EventHandler<AvatarInfo>(this.OnOperationAssigneeSelect);
        taskOperationDialog.TagsSelect -= new EventHandler<TagSelectData>(this.OnOperationTagsSelect);
        taskOperationDialog.Copied -= new EventHandler(this.OnOperationCopied);
        taskOperationDialog.LinkCopied -= new EventHandler(this.OnOperationLinkCopied);
        taskOperationDialog.SwitchTaskOrNote -= new EventHandler(this.OnOperationSwitchTaskOrNote);
        taskOperationDialog.Disappear -= new EventHandler<bool>(this.OnOperationDisappear);
        taskOperationDialog.SkipCurrentRecurrence -= new EventHandler(this.OnOperationSkipCurrentRecurrence);
        taskOperationDialog.Toast -= new EventHandler<string>(this.OnOperationToast);
      }
      SyncManager.TryDelaySync();
      this.DelayEndIgnoreMouseUp();
    }

    private async void OnOperationDeleted(object sender, EventArgs e)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(this._currentModel.Id);
      if (thinTaskById == null)
        return;
      thinTaskById.deleted = 1;
      thinTaskById.modifiedTime = new DateTime?(DateTime.Now);
      await TaskService.UpdateTaskOnDeletedChanged(thinTaskById);
      UndoToast uiElement = new UndoToast();
      uiElement.InitTaskUndo(this._currentModel.Id, this._currentModel.Title);
      Utils.GetToastWindow().Toast((FrameworkElement) uiElement);
    }

    private async void OnOperationQuickDateSelect(object sender, DateTime e)
    {
      if (sender is TaskOperationDialog taskOperationDialog)
        taskOperationDialog.Dismiss();
      DateTime? endDate = this._currentModel.EndDate;
      if (endDate.HasValue)
      {
        TimeSpan timeSpan = endDate.GetValueOrDefault() - this._currentModel.StartDate;
        this._currentModel.TrySetStartDate(new DateTime?(e));
        this._currentModel.EndDate = new DateTime?(e.Add(timeSpan));
      }
      else
        this._currentModel.TrySetStartDate(new DateTime?(e));
      await this.UpdateTaskWithUndo(QuickSetDate(), this._currentModel.Id);
      this._model.UpdateCellTime(this._currentModel);
      await this._model.UpdateCellLineAsync();

      async Task QuickSetDate()
      {
        await Task.Yield();
        await this._currentModel.CommitDate((TimeData) null);
      }
    }

    private async void OnOperationTimeSelect(object sender, TimeData e)
    {
      if (sender is TaskOperationDialog taskOperationDialog)
        taskOperationDialog.Dismiss();
      await this.UpdateTaskWithUndo(SetTime(), this._currentModel.Id);

      async Task SetTime()
      {
        await Task.Yield();
        DateTime? startDate = e.StartDate;
        if (startDate.HasValue)
        {
          DateTime start = startDate.GetValueOrDefault();
          this._currentModel.TrySetStartDate(new DateTime?(start));
          this._currentModel.EndDate = e.DueDate;
          this._currentModel.IsAllDay = ((int) e.IsAllDay ?? 1) != 0;
          await this._currentModel.CommitDate(e);
          this._model.UpdateCellTime(this._currentModel);
          await this._model.UpdateCellLineAsync();
        }
        else
          await this._currentModel.ClearDate();
      }
    }

    private async void OnOperationTimeClear(object sender, EventArgs e)
    {
      if (sender is TaskOperationDialog taskOperationDialog)
        taskOperationDialog.Dismiss();
      await this.UpdateTaskWithUndo(this._currentModel.ClearDate(), this._currentModel.Id);
      this._model.UpdateCellTime(this._currentModel);
      await this._model.UpdateCellLineAsync();
    }

    public int GetDayOffsetByPoint(System.Windows.Point point)
    {
      return (int) ((this.MainScroll.HorizontalOffset + point.X) / this._model.OneDayWidth);
    }

    public int GetLineByPoint(System.Windows.Point point)
    {
      return (int) ((this.MainScroll.VerticalOffset + point.Y) / this._model.OneLineHeight);
    }

    private DateTime GetDay()
    {
      return this.MainScroll.ActualWidth <= 0.0 ? DateTime.MinValue : this._model.StartDate.AddDays((double) (int) Math.Round((this.MainScroll.HorizontalOffset + this.MainScroll.ActualWidth / 2.0) / this._model.OneDayWidth, 0, MidpointRounding.AwayFromZero));
    }

    private async Task GotoDay(DateTime day, bool smooth = false, bool resetRange = false, System.Windows.Point hoverPoint = default (System.Windows.Point))
    {
      TimelineContainer timelineContainer = this;
      double dayWidth = timelineContainer._model.OneDayWidth;
      int num1 = (int) ((timelineContainer.IsLoaded ? timelineContainer.MainScroll.ViewportWidth : timelineContainer._initWidth) / dayWidth / 2.0 + 1.0);
      DateTime dateTime1 = day.AddDays((double) (-1 * num1));
      DateTime dateTime2;
      DateTime dateTime3;
      timelineContainer._model.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime2, out dateTime3);
      DateTime dateTime4 = dateTime2;
      DateTime dateTime5 = dateTime3;
      if (resetRange)
      {
        await timelineContainer._model.MoveSpan(day.AddDays((double) (num1 * -2)), day.AddDays((double) (num1 * 2)));
      }
      else
      {
        if (dateTime4 > dateTime1)
          dateTime4 = day.AddDays((double) (num1 * -1));
        if (dateTime5 < day.AddDays((double) num1))
          dateTime5 = day.AddDays((double) num1);
        int num2 = num1 * 2;
        int days = (dateTime5 - dateTime4).Days;
        if (days < num2)
        {
          int num3 = (num2 - days + 5) / 2;
          if (num3 > 0)
          {
            dateTime4 = dateTime4.AddDays((double) (-1 * num3));
            dateTime5 = dateTime5.AddDays((double) num3);
          }
        }
        if (dateTime4 < timelineContainer._model.StartDate || dateTime5 > timelineContainer._model.EndDate)
        {
          dateTime3 = dateTime4.AddDays((double) (1 - dateTime4.DayOfWeek));
          DateTime startDate = dateTime3.AddDays(-28.0);
          await timelineContainer._model.MoveSpan(startDate, dateTime5.AddDays(28.0));
        }
      }
      double offset = (day - timelineContainer._model.StartDate).TotalDays * dayWidth - (hoverPoint != new System.Windows.Point() ? hoverPoint.X - 0.5 * dayWidth : timelineContainer.MainScroll.ActualWidth / 2.0);
      if (smooth)
      {
        if (Math.Abs(offset - timelineContainer.MainScroll.HorizontalOffset) < 4.0)
          return;
        timelineContainer.SmoothScrollAsync(offset - timelineContainer.MainScroll.HorizontalOffset);
      }
      else
      {
        timelineContainer.MainScroll.ScrollToHorizontalOffset(offset);
        if (Math.Abs(timelineContainer.MainScroll.HorizontalOffset - offset) <= 1.0)
          return;
        for (int retry = 0; timelineContainer.MainScroll.ScrollableWidth < offset && retry < 40; ++retry)
          await Task.Delay(10);
        timelineContainer.MainScroll.ScrollToHorizontalOffset(offset);
      }
    }

    private async void OnContainerSizeChanged()
    {
      if (this._offset <= 0.0)
        return;
      for (int retry = 0; this.MainScroll.ScrollableWidth < this._offset && retry < 400; ++retry)
        await Task.Delay(1);
      if (this.MainScroll.ScrollableWidth < this._offset)
      {
        this._offset = -1.0;
      }
      else
      {
        this.MainScroll.ScrollToHorizontalOffset(this._offset);
        this._offset = -1.0;
      }
    }

    private async void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (e.HorizontalChange != 0.0)
      {
        if (!this._projectScrollChanged && Mouse.Captured != null)
          this._projectScrollChanged = true;
        DateTime startDate1;
        DateTime dateTime1;
        if (e.HorizontalChange > 0.0 && e.HorizontalOffset + e.ViewportWidth + 10.0 >= this.MainScroll.ExtentWidth)
        {
          this._model.StartEndTuple.Deconstruct<DateTime, DateTime>(out startDate1, out dateTime1);
          await this._model.MoveSpan(startDate1, dateTime1.AddDays(60.0));
          if (Mouse.Captured is Thumb captured && string.IsNullOrEmpty(captured.Name))
            Mouse.Capture((IInputElement) null);
        }
        if (this._offset < 0.0 && e.HorizontalChange < 0.0 && e.ExtentWidthChange >= 0.0 && e.HorizontalOffset < 10.0)
        {
          this._model.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out startDate1);
          DateTime dateTime2 = dateTime1;
          DateTime endDate = startDate1;
          DateTime startDate2 = dateTime2.AddDays((double) (1 - dateTime2.DayOfWeek)).AddDays(-70.0);
          this._offset = 70.0 * this._model.OneDayWidth;
          await this._model.MoveSpan(startDate2, endDate);
          this._batchStartPoint = new System.Windows.Point(this._batchStartPoint.X + this._offset, this._batchStartPoint.Y);
          this.OnContainerSizeChanged();
          if (Mouse.Captured is Thumb captured && string.IsNullOrEmpty(captured.Name))
            Mouse.Capture((IInputElement) null);
        }
        this._model.XOffset = this.MainScroll.HorizontalOffset;
        this.DayLineScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset);
        this.YearScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset);
      }
      if (e.VerticalChange == 0.0)
        return;
      this.ColumnScroll.ScrollToVerticalOffset(this.MainScroll.VerticalOffset);
    }

    private async void OnGotoTodayMouseUp(object sender, MouseButtonEventArgs e)
    {
      TimelineContainer timelineContainer = this;
      UserActCollectUtils.AddClickEvent("timeline", "action_bar", "back_to_today");
      await timelineContainer.GotoDay(DateTime.Today.AddDays((timelineContainer.ActualWidth / 2.0 - (timelineContainer._model.ShowGroup ? timelineContainer._model.GroupWidth : 0.0) - 1.5 * TimelineConstants.GetOneDayWidth(-1)) / timelineContainer._model.OneDayWidth), true);
      timelineContainer._projectScrollChanged = true;
    }

    private void OnPrePageMouseUp(object sender, MouseButtonEventArgs e)
    {
      UserActCollectUtils.AddClickEvent("timeline", "view_action", "button_to_view");
      this.SmoothScrollAsync(this.MainScroll.ViewportWidth / 3.0 * -2.0);
    }

    private void OnNextPageMouseUp(object sender, MouseButtonEventArgs e)
    {
      UserActCollectUtils.AddClickEvent("timeline", "view_action", "button_to_view");
      this.SmoothScrollAsync(this.MainScroll.ViewportWidth / 3.0 * 2.0);
    }

    internal void RemoveHoverHeader()
    {
      this._model.HoverStartEndTuple = (Tuple<DateTime, DateTime>) null;
    }

    private void SmoothScrollAsync(double offset)
    {
      if (offset == 0.0)
        return;
      this._model.GotoBtnEnabled = false;
      int sign = offset > 0.0 ? 1 : -1;
      offset = Math.Abs(offset);
      double pos = 0.0;
      DispatcherTimer scrollTimer = new DispatcherTimer(DispatcherPriority.Render)
      {
        Interval = TimeSpan.FromMilliseconds(8.0)
      };
      scrollTimer.Tick += (EventHandler) ((s, e) =>
      {
        double val2 = EaseOutQuad(pos, 0.0, offset, 1000.0);
        pos += 8.0;
        double num = Math.Max(1.0, val2);
        this.MainScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset + (double) sign * num);
        offset -= num;
        if (offset > 0.0)
          return;
        scrollTimer.Stop();
      });
      scrollTimer.Start();
      this._model.GotoBtnEnabled = true;

      static double EaseOutQuad(double t, double b, double c, double d)
      {
        return -c * (Math.Pow(t / d - 1.0, 4.0) - 1.0) + b;
      }
    }

    public void SetFoldMenuIcon(bool isHide)
    {
      this.TimelineNavBar.FoldImage.SetResourceReference(Image.SourceProperty, isHide ? (object) "ShowMenuDrawingImage" : (object) "HideMenuDrawingImage");
    }

    public void HideFoldMenuIcon()
    {
      this.TimelineNavBar.FoldGrid.Visibility = Visibility.Collapsed;
      this.TimelineNavBar.TitleGrid.Margin = new Thickness(20.0, 3.0, 0.0, 0.0);
    }

    private void SaveProjectPos(bool force = false)
    {
      if (!force && !this._projectScrollChanged || this._model.ProjectIdentity == null)
        return;
      DateTime day = this.GetDay();
      TimelineContainer._projectDays[this._model.ProjectIdentity.CatId] = day;
    }

    public async Task LoadProject(ProjectIdentity projectProject, string taskId = null)
    {
      TimelineContainer timelineContainer = this;
      timelineContainer.SaveProjectPos();
      timelineContainer._offset = -1.0;
      timelineContainer._projectScrollChanged = false;
      timelineContainer._projectIdentity = projectProject;
      timelineContainer.Dispatcher.InvokeAsync<Task>((Func<Task>) (async () => await this.LoadProjectInternal(projectProject, taskId)));
      if (projectProject.SortOption.groupBy == "sortOrder")
        ColumnBatchHandler.MergeWithServer(projectProject.GetProjectId());
      timelineContainer.Header.InvalidateVisual();
      timelineContainer.HoverHeader.InvalidateVisual();
      timelineContainer.TimelineBackground.InvalidateVisual();
    }

    private async Task LoadProjectInternal(ProjectIdentity projectProject, string taskId = null)
    {
      TimelineContainer timelineContainer = this;
      ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader closeLoader;
      TimelineCellViewModel cellModel;
      if (projectProject == null)
      {
        closeLoader = (ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader) null;
        cellModel = (TimelineCellViewModel) null;
      }
      else
      {
        bool samePid = timelineContainer._model.ProjectIdentity?.Id == projectProject.Id;
        DateTime oldDay;
        if (TimelineContainer._projectDays.TryGetValue(projectProject.CatId, out oldDay) & samePid && string.IsNullOrEmpty(taskId))
        {
          await timelineContainer._model.SetProjectIdentity(projectProject);
          await timelineContainer.GotoDay(oldDay, resetRange: true);
          closeLoader = (ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader) null;
          cellModel = (TimelineCellViewModel) null;
        }
        else
        {
          await timelineContainer._model.SetProjectIdentity(projectProject);
          if (!string.IsNullOrEmpty(taskId))
            timelineContainer._model.Editing = true;
          await Task.Delay(10);
          while (timelineContainer._model.AvailableReset)
            await Task.Delay(10);
          DateTime gotoDay = DateTime.Today;
          bool hasOldDay = false;
          TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
          if (taskById != null && taskById.StartDate.HasValue && taskById.StartDate.Value.Year > 1970)
          {
            gotoDay = taskById.StartDate.Value.Date;
            hasOldDay = true;
            timelineContainer._projectScrollChanged = true;
          }
          else if (oldDay != DateTime.MinValue)
          {
            hasOldDay = true;
            gotoDay = oldDay;
          }
          else
          {
            DateTime? rentCellDate = timelineContainer._model.GetRentCellDate();
            if (rentCellDate.HasValue)
              gotoDay = rentCellDate.GetValueOrDefault();
          }
          for (int retry = 0; timelineContainer.MainScroll.ActualWidth == 0.0 && retry < 100; ++retry)
            await Task.Delay(4);
          if (timelineContainer.MainScroll.ActualWidth > 0.0)
          {
            if (!hasOldDay)
              gotoDay = gotoDay.AddDays(timelineContainer.ActualWidth / 4.0 / timelineContainer._model.OneDayWidth);
            await timelineContainer.GotoDay(gotoDay, resetRange: true);
            if (!samePid)
              timelineContainer.MainScroll.ScrollToTop();
          }
          closeLoader = new ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader();
          if (await closeLoader.NeedPullCompletedTasks(projectProject))
          {
            if (await closeLoader.TryLoadCompletedTasks(projectProject))
              timelineContainer.OnCompletedLoaded();
          }
          if (!string.IsNullOrEmpty(taskId))
          {
            cellModel = timelineContainer._model.GetCellModel(taskId);
            if (cellModel != null)
            {
              timelineContainer._model.Editing = true;
              if (cellModel.Top > timelineContainer.MainScroll.ActualHeight + timelineContainer.MainScroll.VerticalOffset)
                timelineContainer.MainScroll.ScrollToVerticalOffset(cellModel.Top - timelineContainer.MainScroll.ActualHeight + 150.0);
              await Task.Delay(100);
              timelineContainer.CellCanvas.TryShowDetailWindow(cellModel);
              closeLoader = (ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader) null;
              cellModel = (TimelineCellViewModel) null;
              return;
            }
          }
          timelineContainer._model.Editing = false;
          closeLoader = (ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader) null;
          cellModel = (TimelineCellViewModel) null;
        }
      }
    }

    private void OnMainScrollSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this._model.ViewWidth = e.NewSize.Width;
    }

    private void OnResizeColumnDragDelta(object sender, DragDeltaEventArgs e)
    {
      this._model.GroupWidth = this.GroupBorder.Width + e.HorizontalChange;
    }

    public void MoveToolTips(System.Windows.Point point)
    {
      if (this._hoveredCell != null)
        point.Y = this._hoveredCell.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this).Y + this._hoveredCell.ActualHeight;
      this.ToolTipControl.Margin = new Thickness(point.X - this.ToolTipControl.ActualWidth / 2.0, point.Y, 0.0, 0.0);
    }

    public void OpenToolTips(FrameworkElement cell, bool showDateTip = false)
    {
      if (cell == null)
        return;
      this._hoveredCell = cell;
      if (cell.DataContext is TimelineCellViewModel dataContext)
      {
        if (dataContext.Operation.Contain(TimelineCellOperation.Edit))
        {
          this.TryCloseToolTips();
          return;
        }
        this.ToolTipControl.DataContext = (object) dataContext;
        this.ToolTipControl.ShowTip(showDateTip);
      }
      this.ToolTipControl.Margin = new Thickness(Mouse.GetPosition((IInputElement) this).X - this.ToolTipControl.ActualWidth / 2.0, cell.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this).Y + cell.ActualHeight + 3.0, 0.0, 0.0);
      this.ToolTipControl.Visibility = Visibility.Visible;
    }

    public void TryCloseToolTips(bool force = false)
    {
      if (this.ToolTipControl.Visibility != Visibility.Visible)
        return;
      if (force)
        this.ToolTipControl.Visibility = Visibility.Collapsed;
      if (this.ToolTipControl.IsMouseOver || this._hoveredCell != null && this._hoveredCell.IsMouseOver && this._hoveredCell.DataContext is TimelineCellViewModel dataContext && !dataContext.Operation.Contain(TimelineCellOperation.Edit))
        return;
      this.ToolTipControl.Visibility = Visibility.Hidden;
    }

    private void OnToolTipPopupMouseLeave(object sender, MouseEventArgs e)
    {
      this.TryCloseToolTips();
    }

    public void OnTouchPadHorizontalScroll(int offset)
    {
      if (this.TimelineFloating.IsMouseOver)
        this.TimelineFloating.DealWithMainScrollMove(true, -1 * offset);
      this.MainScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset + (double) offset);
    }

    private void OnSwitchRangeMouseUp(object sender, MouseButtonEventArgs args)
    {
      this.SelectRangePopup.IsOpen = !this.SelectRangePopup.IsOpen;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "day", Utils.GetString("Day"), (Geometry) null);
      menuItemViewModel1.Selected = this._model.TimelineRange == "day";
      types.Add(menuItemViewModel1);
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "week", Utils.GetString("Week"), (Geometry) null);
      menuItemViewModel2.Selected = this._model.TimelineRange == "week";
      types.Add(menuItemViewModel2);
      CustomMenuItemViewModel menuItemViewModel3 = new CustomMenuItemViewModel((object) "month", Utils.GetString("Month"), (Geometry) null);
      menuItemViewModel3.Selected = this._model.TimelineRange == "month";
      types.Add(menuItemViewModel3);
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.SelectRangePopup);
      customMenuList.Operated += (EventHandler<object>) (async (o, e) =>
      {
        if (!(e is string data2))
          return;
        DateTime day = this.GetDay();
        this._model.TimelineRange = data2;
        UserActCollectUtils.AddClickEvent("timeline", "view_zoom", data2);
        await this.GotoDay(day);
        await this._model.UpdateCellLineAsync();
      });
      customMenuList.Show();
    }

    private async void OnSwitchRangeClick(object sender, EventArgs e)
    {
      if (sender is FrameworkElement frameworkElement && frameworkElement.DataContext is TimelineViewModel dataContext && frameworkElement.Tag is string tag)
      {
        DateTime day = this.GetDay();
        dataContext.TimelineRange = tag;
        UserActCollectUtils.AddClickEvent("timeline", "view_zoom", tag);
        await this.GotoDay(day);
        await this._model.UpdateCellLineAsync();
      }
      this.SelectRangePopup.IsOpen = false;
    }

    public void KeyDown(Key key)
    {
      switch (key)
      {
        case Key.Left:
          this.MainScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset - this.ActualWidth / 10.0);
          break;
        case Key.Up:
          this.MainScroll.ScrollToVerticalOffset(Math.Max(0.0, this.MainScroll.VerticalOffset - this._model.OneDayWidth));
          break;
        case Key.Right:
          this.MainScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset + this.ActualWidth / 10.0);
          break;
        case Key.Down:
          this.MainScroll.ScrollToVerticalOffset(this.MainScroll.VerticalOffset + this._model.OneDayWidth);
          break;
        case Key.D:
        case Key.M:
        case Key.W:
          if (!Utils.IfCtrlPressed() || !Utils.IfShiftPressed())
          {
            this.QuickSwitchRange(key);
            break;
          }
          break;
      }
      this.TimelineFloating.Close();
    }

    private async void QuickSwitchRange(Key key)
    {
      string str = "day";
      switch (key)
      {
        case Key.M:
          str = "month";
          break;
        case Key.W:
          str = "week";
          break;
      }
      int defaultWidthIndex = TimelineConstants.GetRangeDefaultWidthIndex(str);
      if (this._model.TimelineRangeIndex == defaultWidthIndex)
        return;
      DateTime day = this.GetDay();
      this._model.SetRangIndex(defaultWidthIndex);
      UserActCollectUtils.AddClickEvent("timeline", "view_zoom", str);
      await this.GotoDay(day);
      await this._model.UpdateCellLineAsync();
      this.TryCloseToolTips(true);
    }

    public void SwitchArrangeAndFloatingIndex(bool floatingAbove)
    {
      if (floatingAbove)
        this.SetZIndexAbove((FrameworkElement) this.TimelineFloating, (FrameworkElement) this.ArrangePanel);
      else
        this.SetZIndexAbove((FrameworkElement) this.ArrangePanel, (FrameworkElement) this.TimelineFloating);
    }

    private void SetZIndexAbove(FrameworkElement a, FrameworkElement b)
    {
      int zindex1 = Panel.GetZIndex((UIElement) a);
      int zindex2 = Panel.GetZIndex((UIElement) b);
      if (zindex1 > zindex2)
        return;
      if (zindex1 == zindex2)
      {
        Panel.SetZIndex((UIElement) a, zindex2 + 1);
      }
      else
      {
        Panel.SetZIndex((UIElement) a, zindex2);
        Panel.SetZIndex((UIElement) b, zindex1);
      }
    }

    public void ShowDragColumnPopup(TimelineGroupViewModel model, MouseEventArgs e)
    {
      this._model?.OnDragColumn();
      this.ColumnDragPopup.DataContext = (object) model;
      this.SetColumnDragPopupPosition(e.GetPosition((IInputElement) this));
      this.ColumnDragPopup.IsOpen = true;
      this.RegisterDragEvent();
    }

    private void SetColumnDragPopupPosition(System.Windows.Point point)
    {
      this.ColumnDragPopup.HorizontalOffset = point.X - 30.0;
      this.ColumnDragPopup.VerticalOffset = point.Y - 10.0;
    }

    private void RegisterDragEvent()
    {
      Window window = Window.GetWindow((DependencyObject) this);
      if (window == null)
        return;
      window.CaptureMouse();
      window.MouseMove -= new MouseEventHandler(this.OnDragColumnMouseMove);
      window.MouseMove += new MouseEventHandler(this.OnDragColumnMouseMove);
      window.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnDragColumnDrop);
      window.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDragColumnDrop);
    }

    private async void OnDragColumnDrop(object sender, MouseButtonEventArgs e)
    {
      TimelineContainer timelineContainer = this;
      if (sender is Window window)
      {
        window.ReleaseMouseCapture();
        window.MouseMove -= new MouseEventHandler(timelineContainer.OnDragColumnMouseMove);
        window.MouseLeftButtonUp -= new MouseButtonEventHandler(timelineContainer.OnDragColumnDrop);
      }
      timelineContainer.ColumnDragPopup.IsOpen = false;
      try
      {
        if (!timelineContainer.ColumnDropLine.IsVisible)
          return;
        timelineContainer.ColumnDropLine.Visibility = Visibility.Collapsed;
        int index = ((int) timelineContainer.ColumnDropLine.Margin.Top + (int) timelineContainer.MainScroll.VerticalOffset + 4) / 40;
        if (timelineContainer.ColumnDragPopup.DataContext is TimelineGroupViewModel model)
        {
          ColumnModel columnById = await ColumnDao.GetColumnById(model.Id);
          if (columnById == null)
            return;
          TimelineGroupViewModel groupModel1 = index <= 0 ? (TimelineGroupViewModel) null : timelineContainer._model.GroupModels[index - 1];
          TimelineGroupViewModel groupModel2 = index >= timelineContainer._model.GroupModels.Count ? (TimelineGroupViewModel) null : timelineContainer._model.GroupModels[index];
          if (groupModel1 == null && groupModel2 == null || groupModel1?.Id == model.Id || groupModel2?.Id == model.Id)
            return;
          long num = groupModel1 == null ? groupModel2.SortOrder - 268435456L : (groupModel2 == null ? groupModel1.SortOrder + 268435456L : MathUtil.LongAvg(groupModel1.SortOrder, groupModel2.SortOrder));
          columnById.sortOrder = new long?(num);
          await ColumnDao.UpdateColumn(columnById);
          DataChangedNotifier.NotifyColumnChanged(timelineContainer._projectIdentity?.Id);
          SyncManager.TryDelaySync();
        }
        model = (TimelineGroupViewModel) null;
      }
      finally
      {
        timelineContainer._model?.ReloadColumns();
      }
    }

    private void OnDragColumnMouseMove(object sender, MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      this.SetColumnDragPopupPosition(position);
      int count = this._model.GroupModels.Count;
      int num1 = (int) position.Y - (int) sbyte.MaxValue + (int) this.MainScroll.VerticalOffset;
      if (num1 > 0 && (double) num1 < (double) count * 40.0)
      {
        this.ColumnDropLine.Visibility = Visibility.Visible;
        int num2 = num1 / 40;
        if ((double) (num1 % 40) > 20.0)
          ++num2;
        this.ColumnDropLine.Margin = new Thickness(0.0, 40.0 * (double) num2 - 4.0 - (double) (int) this.MainScroll.VerticalOffset, 0.0, 0.0);
      }
      else
        this.ColumnDropLine.Visibility = Visibility.Collapsed;
    }

    private async void OnProjectChanged(object sender, EventArgs e)
    {
      TimelineContainer timelineContainer = this;
      ProjectModel project;
      if (timelineContainer.Visibility != Visibility.Visible)
      {
        project = (ProjectModel) null;
      }
      else
      {
        string catId = timelineContainer._model.ProjectIdentity?.CatId;
        if (string.IsNullOrEmpty(catId))
        {
          project = (ProjectModel) null;
        }
        else
        {
          project = CacheManager.GetProjectById(catId);
          if (project == null)
            project = (ProjectModel) null;
          else if (project.viewMode == "kanban")
          {
            await Task.Delay(500);
            if (!timelineContainer.IsVisible)
            {
              project = (ProjectModel) null;
            }
            else
            {
              DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new NormalProjectIdentity(project));
              project = (ProjectModel) null;
            }
          }
          else if (project.viewMode == "list")
          {
            await Task.Delay(500);
            if (!timelineContainer.IsVisible)
            {
              project = (ProjectModel) null;
            }
            else
            {
              DataChangedNotifier.NotifyProjectViewModeChanged((ProjectIdentity) new NormalProjectIdentity(project));
              project = (ProjectModel) null;
            }
          }
          else if (project.Timeline == null)
          {
            project = (ProjectModel) null;
          }
          else
          {
            if (project.Timeline.sortOption != null && project.Timeline.sortOption.Equal(timelineContainer._model.TimelineSortOption))
              timelineContainer._model.TimelineSortOption = project.Timeline.sortOption;
            if (project.Timeline.Range != timelineContainer._model.TimelineRange)
            {
              DateTime day = timelineContainer.GetDay();
              timelineContainer._model.TimelineRange = project.Timeline?.Range;
              await timelineContainer.GotoDay(day);
            }
            if (!(project.name != timelineContainer._model.ProjectTitle))
            {
              project = (ProjectModel) null;
            }
            else
            {
              timelineContainer._model.SetProjectTitle(project.name);
              project = (ProjectModel) null;
            }
          }
        }
      }
    }

    private void OnColumnChanged(object sender, string e)
    {
      if (!(this._model.ProjectIdentity?.GetTimelineModel().SortType == Constants.SortType.sortOrder.ToString()) || !(this._model.ProjectIdentity?.CatId == e))
        return;
      this._model.ReloadColumns();
    }

    private void OnCompletedLoaded()
    {
      if (!(this._model.ProjectIdentity is NormalProjectIdentity projectIdentity) || this._model.AvailableModels.Count != 0)
        return;
      if (TimelineContainer._projectDays.ContainsKey(projectIdentity.Project.id))
        TimelineContainer._projectDays.Remove(projectIdentity.Project.id);
      this._projectScrollChanged = false;
      this.LoadProject((ProjectIdentity) new NormalProjectIdentity(projectIdentity.Project));
    }

    public void TryToastTaskChangeUndo(
      TaskModel task,
      List<TaskModel> children = null,
      List<TaskDetailItemModel> checkItems = null)
    {
      if (task == null)
        return;
      if (TaskViewModelHelper.GetMatchedTasks(this._projectIdentity, new List<string>()
      {
        task.id
      }).Count != 0)
        return;
      Utils.FindParent<IToastShowWindow>((DependencyObject) this).Toast((FrameworkElement) new UndoToast((UndoController) new TaskUndo(task, string.Empty, Utils.GetString("TaskHasBeenFiltered"), children, checkItems)));
    }

    private async Task UpdateTaskWithUndo(Task t, string taskId, bool undoCheckItem = false)
    {
      if (this._projectIdentity is FilterProjectIdentity)
      {
        TaskModel task = await TaskDao.GetTaskById(taskId);
        List<TaskDetailItemModel> checkItems = (List<TaskDetailItemModel>) null;
        if (undoCheckItem)
          checkItems = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
        await t;
        this.TryToastTaskChangeUndo(task, checkItems: checkItems);
        task = (TaskModel) null;
        checkItems = (List<TaskDetailItemModel>) null;
      }
      else
        await t;
    }

    public void TryToastString(string toast)
    {
      Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.TryToastString((object) null, toast);
    }

    public async void Reload()
    {
      TimelineContainer timelineContainer = this;
      if (!timelineContainer.IsVisible || timelineContainer._batchMouseDown)
        return;
      timelineContainer._model.Reload();
      timelineContainer.Header.InvalidateVisual();
      timelineContainer.HoverHeader.InvalidateVisual();
      timelineContainer.TimelineBackground.InvalidateVisual();
    }

    public async void ToggleGroupsOpen() => this._model.ToggleGroupsOpen();

    public async void DelaySetHitVisible()
    {
      TimelineContainer timelineContainer = this;
      timelineContainer.IsHitTestVisible = false;
      await Task.Delay(100);
      timelineContainer.IsHitTestVisible = true;
    }

    public void SetInitWidth(double initWidth) => this._initWidth = initWidth;

    public void Dispose() => this._model.AvailableModels.ClearEvents();

    public TimelineViewModel GetViewModel() => this._model;

    public void ReloadSortOption() => this._model.OnSortOptionChanged();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/timeline/timelinecontainer.xaml", UriKind.Relative));
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
          this.Root = (TimelineContainer) target;
          break;
        case 2:
          this.TimelineNavBar = (TimelineNavBar) target;
          break;
        case 3:
          this.YearScroll = (ScrollViewer) target;
          break;
        case 4:
          this.OptionPanel = (StackPanel) target;
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnGotoTodayMouseUp);
          break;
        case 6:
          this.SwitchRangeBorder = (Border) target;
          this.SwitchRangeBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSwitchRangeMouseUp);
          break;
        case 7:
          this.SelectRangePopup = (EscPopup) target;
          break;
        case 8:
          this.SelectRangeStackPanel = (StackPanel) target;
          break;
        case 9:
          ((UIElement) target).MouseDown += new MouseButtonEventHandler(this.OnMouseLeftDown);
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseLeftUp);
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnMouseMove);
          ((UIElement) target).PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel);
          break;
        case 10:
          this.DayLineScroll = (ScrollViewer) target;
          break;
        case 11:
          this.Header = (TimelineHeader) target;
          break;
        case 12:
          this.HoverHeader = (TimelineHeaderHover) target;
          break;
        case 13:
          this.PrePageGrid = (Grid) target;
          this.PrePageGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnPrePageMouseUp);
          break;
        case 14:
          this.NextPageGrid = (Grid) target;
          this.NextPageGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnNextPageMouseUp);
          break;
        case 15:
          this.MainScroll = (ScrollViewer) target;
          this.MainScroll.SizeChanged += new SizeChangedEventHandler(this.OnMainScrollSizeChanged);
          this.MainScroll.ScrollChanged += new ScrollChangedEventHandler(this.OnScrollChanged);
          this.MainScroll.PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel);
          break;
        case 16:
          this.MainGrid = (Grid) target;
          this.MainGrid.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseLeftDown);
          this.MainGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseLeftUp);
          this.MainGrid.MouseMove += new MouseEventHandler(this.OnMouseMove);
          break;
        case 17:
          this.TimelineBackground = (TimelineBackground) target;
          break;
        case 18:
          this.CellCanvas = (TimelineVirtualizedCanvas) target;
          break;
        case 19:
          this.BatchSelectBorder = (Border) target;
          break;
        case 20:
          this.TimelineFloating = (TimelineCellFloating) target;
          break;
        case 21:
          this.ArrangePanel = (TimelineArrange) target;
          break;
        case 22:
          this.GroupBorder = (Grid) target;
          break;
        case 23:
          ((Thumb) target).DragDelta += new DragDeltaEventHandler(this.OnResizeColumnDragDelta);
          break;
        case 24:
          this.ColumnScroll = (ScrollViewer) target;
          this.ColumnScroll.PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel);
          break;
        case 25:
          this.ToolTipControl = (TimelineToolTip) target;
          break;
        case 26:
          this.ColumnDragPopup = (Popup) target;
          break;
        case 27:
          this.ColumnDropLine = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
