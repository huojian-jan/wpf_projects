// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Eisenhower.QuadrantControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
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
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Eisenhower
{
  public class QuadrantControl : Border, IBatchEditable, IComponentConnector
  {
    public static readonly DependencyProperty InOptionProperty = DependencyProperty.Register(nameof (InOption), typeof (bool), typeof (QuadrantControl), new PropertyMetadata((object) false));
    private int _level = 1;
    private MatrixQuadrantIdentity _identity;
    private bool _taskDragOver;
    private BatchTaskEditHelper _batchHelper;
    private bool _previewDrag;
    private bool _isLocked;
    private MatrixContainer _matrix;
    private bool _omMouseDown;
    internal QuadrantControl Root;
    internal Border BackBorder;
    internal Border HoverBorder;
    internal ColumnDefinition OptionColumn;
    internal Grid IconGrid;
    internal Path Icon;
    internal EmjTextBlock Emoji;
    internal EmjTextBlock Title;
    internal HoverIconButton AddButton;
    internal HoverIconButton MoreButton;
    internal EscPopup MorePopup;
    internal TaskListView TaskList;
    internal TextBlock EmptyText;
    private bool _contentLoaded;

    public QuadrantControl()
    {
      this.InitializeComponent();
      this.Loaded += (RoutedEventHandler) ((o, e) =>
      {
        TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
        this.TaskList.DragOver += new EventHandler<DragMouseEvent>(this.OnTaskDragOver);
      });
      this.Unloaded += (RoutedEventHandler) ((o, e) =>
      {
        TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
        this.TaskList.DragOver -= new EventHandler<DragMouseEvent>(this.OnTaskDragOver);
      });
      this._batchHelper = new BatchTaskEditHelper((IBatchEditable) this, this._level);
      this._batchHelper.ShowOrHideOperation += new EventHandler<bool>(this.OnBatchOperationChanged);
      this.TaskList.ViewModel.BatchEditor = (IBatchEditable) this;
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      this.TaskList.OnTasksChanged(e);
    }

    private void OnBatchOperationChanged(object sender, bool e) => this.SetMatrixInOperation(e);

    private void ReloadData(object sender, List<string> e) => this.LoadAsync();

    public bool InOption
    {
      get => (bool) this.GetValue(QuadrantControl.InOptionProperty);
      set => this.SetValue(QuadrantControl.InOptionProperty, (object) value);
    }

    public event EventHandler<MouseEventArgs> TaskDragOver;

    public QuadrantModel Quadrant => this._identity?.Quadrant;

    public int Level
    {
      get => this._level;
      set
      {
        this._level = value;
        this.OnLevelSet();
      }
    }

    private void OnLevelSet()
    {
      if (this._level < 1 || this._level > 4)
        return;
      string key1 = "MatrixColorQuadrant" + this._level.ToString();
      string index = "MatrixIconQuadrant" + this._level.ToString();
      string key2 = "MatrixTitleQuadrant" + this._level.ToString();
      SolidColorBrush color = ThemeUtil.GetColor(key1);
      this.Icon.Data = Utils.GetIcon(index);
      this.Icon.Fill = (Brush) color;
      this.Title.Foreground = (Brush) color;
      this.Title.Text = Utils.GetString(key2);
      this.TaskList.QuadrantLevel = this._level;
    }

    private void OnAddTaskClick(object sender, MouseButtonEventArgs e)
    {
      if (!PopupStateManager.CanShowAddPopup())
        return;
      UserActCollectUtils.AddClickEvent("matrix", "matrix_action", "add");
      TaskDetailViewModel model = TaskDetailViewModel.BuildInitModel((TaskBaseViewModel) null);
      if (this._identity != null)
      {
        model.SourceViewModel.Tag = TagSerializer.ToJsonContent(this._identity.GetTags());
        model.SourceViewModel.Priority = this._identity.IsNote ? 0 : this._identity.GetPriority();
        model.SourceViewModel.ProjectId = this._identity.GetProjectId();
        model.SourceViewModel.ProjectName = this._identity.GetProjectName();
        TimeData timeData = this._identity.GetTimeData();
        model.SetTimeData(timeData);
        model.SourceViewModel.Kind = this._identity.GetIsNote() ? "NOTE" : "TEXT";
      }
      model.IsNewAdd = true;
      TaskDetailPopup taskDetailPopup = new TaskDetailPopup();
      IToastShowWindow parent = Utils.FindParent<IToastShowWindow>((DependencyObject) this);
      if (parent != null)
      {
        if (PopupStateManager.LastTarget == this)
          return;
        taskDetailPopup.DependentWindow = parent;
      }
      Grid target = sender as Grid;
      taskDetailPopup.TaskSaved += new EventHandler<string>(this.OnTaskAdded);
      taskDetailPopup.Disappear += new EventHandler<string>(this.OnDetailWindowClosed);
      taskDetailPopup.Show(model, string.Empty, new TaskWindowDisplayArgs((UIElement) target, 25.0, false, this._level), canParse: true);
      this.InOption = true;
      this.SetMatrixInOperation(true);
    }

    private void OnDetailWindowClosed(object sender, string e)
    {
      this.InOption = false;
      this.SetMatrixInOperation(false);
      if (!(sender is TaskDetailWindow taskDetailWindow))
        return;
      taskDetailWindow.Disappear -= new EventHandler<string>(this.OnDetailWindowClosed);
    }

    private async void OnTaskAdded(object sender, string taskId)
    {
      QuadrantControl quadrantControl = this;
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.TaskSaved -= new EventHandler<string>(quadrantControl.OnTaskAdded);
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(thinTaskById);
      List<TaskBaseViewModel> matchedNormalFilter1 = TaskService.GetTaskMatchedNormalFilter(Parser.ToNormalModel(quadrantControl._identity.Quadrant.rule), new List<TaskBaseViewModel>()
      {
        taskViewModel
      }, true);
      // ISSUE: explicit non-virtual call
      if ((matchedNormalFilter1 != null ? (__nonvirtual (matchedNormalFilter1.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        quadrantControl.LoadAsync();
      }
      else
      {
        foreach (QuadrantModel quadrant in LocalSettings.Settings.MatrixModel.quadrants)
        {
          if (quadrant.id != quadrantControl._identity.Quadrant.id)
          {
            List<TaskBaseViewModel> matchedNormalFilter2 = TaskService.GetTaskMatchedNormalFilter(Parser.ToNormalModel(quadrant.rule), new List<TaskBaseViewModel>()
            {
              taskViewModel
            }, true);
            // ISSUE: explicit non-virtual call
            if ((matchedNormalFilter2 != null ? (__nonvirtual (matchedNormalFilter2.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              int result;
              int.TryParse(quadrant.id.Substring(8), out result);
              string toast = string.Format(Utils.GetString("AddedToProject"), (object) MatrixManager.GetQuadrantNameByLevel(result));
              quadrantControl.GetMatrixParent()?.Reload(quadrant.id);
              quadrantControl.GetMatrixParent()?.Toast(toast);
              return;
            }
          }
        }
        quadrantControl.GetMatrixParent()?.ToastMoveProjectControl(thinTaskById.projectId, thinTaskById.title, MoveToastType.Add);
      }
    }

    private MatrixContainer GetMatrixParent()
    {
      return this._matrix ?? (this._matrix = Utils.FindParent<MatrixContainer>((DependencyObject) this));
    }

    internal void OnTaskDragDrop()
    {
      if (!this._taskDragOver)
        return;
      this._taskDragOver = false;
      this.HoverBorder.BorderThickness = new Thickness(0.0);
    }

    internal string GetRule() => this._identity.Quadrant.rule;

    internal void SetTaskOver(int level)
    {
      this._taskDragOver = level == this._level;
      this.HoverBorder.BorderThickness = new Thickness((double) (this._taskDragOver ? 1 : 0));
    }

    internal int GetLevel() => this._level;

    private async void OnOMClick(object sender, MouseButtonEventArgs e)
    {
      QuadrantControl element = this;
      if (!element._omMouseDown || element._identity == null)
        return;
      element._omMouseDown = false;
      List<SortTypeViewModel> projectSortTypeModels = SortOptionHelper.GetSmartProjectSortTypeModels(element._identity.IsNote);
      new List<CustomMenuItemViewModel>().AddRange(projectSortTypeModels.Select<SortTypeViewModel, CustomMenuItemViewModel>((Func<SortTypeViewModel, CustomMenuItemViewModel>) (sortType => new CustomMenuItemViewModel((object) sortType.Id, sortType.Title, (DrawingImage) null))));
      List<CustomMenuItemViewModel> extraItem = new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "edit", Utils.GetString("Edit"), Utils.GetImageSource("EditDrawingImage", (FrameworkElement) element))
      };
      SortTypeSelector sortTypeSelector = new SortTypeSelector((ProjectIdentity) element._identity, projectSortTypeModels, element._identity.SortOption, popup: element.MorePopup, extraItem: extraItem);
      sortTypeSelector.ResetSortOrder += new EventHandler<int>(element.OnResetSortOrder);
      sortTypeSelector.SortOptionSelect += new EventHandler<SortOption>(element.OnSortOptionSelect);
      sortTypeSelector.OptionSelect += new EventHandler<string>(element.OnOptionSelect);
      sortTypeSelector.Show();
      element.SetMatrixInOperation(false);
      element.InOption = true;
    }

    private void OnOptionSelect(object sender, string e)
    {
      if (!(e == "edit"))
        return;
      EditQuadrantWindow editQuadrantWindow = new EditQuadrantWindow(this._level);
      editQuadrantWindow.Owner = Window.GetWindow((DependencyObject) this);
      editQuadrantWindow.ShowDialog();
    }

    internal void ClearSelected() => this.TaskList.SetSelected((List<string>) null);

    private void OnSortOptionSelect(object o, SortOption sort)
    {
      LocalSettings.Settings.SaveMatrixQuadrant(this._level, (string) null, sort, (string) null);
      this._identity.SortOption = sort;
      DataChangedNotifier.NotifyMatrixQuadrantChanged(this._level);
    }

    private async void OnResetSortOrder(object o, int e)
    {
      await TaskSortOrderService.DeleteAllSortOrderBySortOptionInListId(this._identity.SortOption, this._identity.CatId);
      DataChangedNotifier.NotifyMatrixQuadrantChanged(this._level);
    }

    public async Task LoadAsync(bool restore = true)
    {
      this.SetIdentity();
      await this.LoadTaskAsync(restore);
    }

    public async Task LoadTaskAsync(bool restore = true)
    {
      await this.TaskList.LoadAsync(true, restoreSelected: restore);
    }

    private void OnTaskDragOver(object sender, DragMouseEvent e)
    {
      EventHandler<MouseEventArgs> taskDragOver = this.TaskDragOver;
      if (taskDragOver == null)
        return;
      taskDragOver((object) this, e.MouseArg);
    }

    private void OnTaskCountChanged(object sender, EventArgs e)
    {
      this.EmptyText.Visibility = !this.TaskList.DisplayModels.Any<DisplayItemModel>() ? Visibility.Visible : Visibility.Collapsed;
      this.EmptyText.Text = Utils.GetString(Parser.GetFilterRuleVersion(this._identity.Quadrant.rule) > 6 ? "FilterVersionUpdate" : "NoTask");
    }

    public void RemoveItems(List<string> taskIds) => this.TaskList.RemoveItemByIds(taskIds);

    private void OnMorePopupClosed(object sender, EventArgs e)
    {
      this.InOption = false;
      this.SetMatrixInOperation(false);
    }

    public void ShowBatchOperationDialog() => this._batchHelper.ShowOperationDialog();

    public void SetSelectedTaskIds(List<string> taskIds)
    {
      this._batchHelper.SelectedTaskIds = taskIds;
    }

    public void RemoveSelectedId(string id)
    {
      this._batchHelper.SelectedTaskIds?.Remove(id);
      this.TaskList.RemoveSelectedId(id);
    }

    public List<string> GetSelectedTaskIds() => this._batchHelper.SelectedTaskIds;

    public void ReloadList() => this.LoadTaskAsync();

    public UIElement BatchOperaPlacementTarget() => (UIElement) this.Title;

    public void SetIdentity()
    {
      QuadrantModel quadrantByLevel = MatrixManager.GetQuadrantByLevel(this._level);
      this.SetTitle(quadrantByLevel.name);
      this._identity = new MatrixQuadrantIdentity(quadrantByLevel);
      this._batchHelper.ProjectIdentity = (ProjectIdentity) this._identity;
      this.TaskList.SetIdentity((ProjectIdentity) this._identity, TaskListDisplayType.Matrix);
    }

    private void SetTitle(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        this.Title.Text = Utils.GetString("MatrixTitleQuadrant" + this._level.ToString());
        this.Title.Foreground = (Brush) ThemeUtil.GetColor("MatrixColorQuadrant" + this._level.ToString());
        this.Emoji.Text = string.Empty;
        this.Icon.Visibility = Visibility.Visible;
      }
      else
      {
        string emojiIcon = EmojiHelper.GetEmojiIcon(name);
        if (!string.IsNullOrEmpty(emojiIcon) && name.StartsWith(emojiIcon))
        {
          this.Title.Text = name.Remove(0, emojiIcon.Length);
          this.Emoji.Text = emojiIcon;
          this.Icon.Visibility = Visibility.Collapsed;
          this.Title.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
        }
        else
        {
          this.Title.Text = name;
          this.Emoji.Text = string.Empty;
          this.Icon.Visibility = Visibility.Visible;
          this.Title.Foreground = (Brush) ThemeUtil.GetColor("MatrixColorQuadrant" + this._level.ToString());
        }
      }
    }

    public async Task BatchPinTask()
    {
      List<string> selectedTaskIds = this.GetSelectedTaskIds();
      List<TaskBaseViewModel> tasksByIds = TaskCache.GetTasksByIds(selectedTaskIds);
      await TaskService.BatchStarTaskOrNote(selectedTaskIds, this._identity.CatId, tasksByIds.Count <= 0 || !tasksByIds.All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t => t.IsPinned)));
    }

    public void BatchOpenSticky() => TaskStickyWindow.ShowTaskSticky(this.GetSelectedTaskIds());

    public async Task BatchSetDate(DateTime? date)
    {
      if (!await TaskService.BatchSetDate(this.GetSelectedTaskIds(), date))
        return;
      this.LoadAsync();
    }

    public async Task BatchSetPriority(int priority)
    {
      if (!await TaskService.BatchSetPriority(this.GetSelectedTaskIds(), priority))
        return;
      this.LoadAsync();
    }

    public void BatchDeleteTask()
    {
      List<string> selectedTaskIds = this._batchHelper.SelectedTaskIds;
      // ISSUE: explicit non-virtual call
      if ((selectedTaskIds != null ? (__nonvirtual (selectedTaskIds.Count) > 1 ? 1 : 0) : 0) == 0)
        return;
      TaskService.BatchDeleteTaskByIds(this._batchHelper.SelectedTaskIds);
    }

    private void OnHeadMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this.AddButton.IsMouseOver || this.MoreButton.IsMouseOver)
        return;
      this._previewDrag = true;
    }

    private void OnHeadMouseMove(object sender, MouseEventArgs e)
    {
      if (!this._isLocked && this._previewDrag && e.LeftButton == MouseButtonState.Pressed)
        Utils.FindParent<MatrixContainer>((DependencyObject) this)?.StartDragQuadrant(this, e);
      this._previewDrag = false;
    }

    public void SetLocked(bool isLocked)
    {
      this._isLocked = isLocked;
      this.AddButton.Visibility = isLocked ? Visibility.Collapsed : Visibility.Visible;
      this.TaskList.IsLocked = isLocked;
      this.TaskList.LoadAsync(true);
    }

    public void SetBackBorderOpacity(double opacity)
    {
      this.BackBorder.Opacity = Math.Max(Math.Min(opacity, 1.0), 0.2);
    }

    public void SetWidgetBackColor(double opacity)
    {
      this.BackBorder.SetResourceReference(Border.BackgroundProperty, (object) "WidgetQuadrantBackground");
      this.BackBorder.Opacity = Math.Max(Math.Min(opacity, 1.0), 0.2);
    }

    public void SetMatrixInOperation(bool inOperation)
    {
      Utils.FindParent<MatrixContainer>((DependencyObject) this)?.SetInOperation(inOperation);
    }

    private void OnOmMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this.MorePopup.IsOpen)
        return;
      this._omMouseDown = true;
    }

    public void ToggleAllSection(bool? open) => this.TaskList.ExpandOrFoldAllSection(open);

    public void ExpandOrFoldAllTask(bool? open) => this.TaskList.ExpandOrFoldAllTask(open);

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/eisenhower/quadrantcontrol.xaml", UriKind.Relative));
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
          this.Root = (QuadrantControl) target;
          break;
        case 2:
          this.BackBorder = (Border) target;
          break;
        case 3:
          this.HoverBorder = (Border) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnHeadMouseDown);
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnHeadMouseMove);
          break;
        case 5:
          this.OptionColumn = (ColumnDefinition) target;
          break;
        case 6:
          this.IconGrid = (Grid) target;
          break;
        case 7:
          this.Icon = (Path) target;
          break;
        case 8:
          this.Emoji = (EmjTextBlock) target;
          break;
        case 9:
          this.Title = (EmjTextBlock) target;
          break;
        case 10:
          this.AddButton = (HoverIconButton) target;
          break;
        case 11:
          this.MoreButton = (HoverIconButton) target;
          break;
        case 12:
          this.MorePopup = (EscPopup) target;
          break;
        case 13:
          this.TaskList = (TaskListView) target;
          break;
        case 14:
          this.EmptyText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
