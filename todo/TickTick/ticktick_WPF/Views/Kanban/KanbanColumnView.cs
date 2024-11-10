// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.KanbanColumnView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Kanban.Item;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.TaskList;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public class KanbanColumnView : Grid, IKanbanColumn
  {
    private static LinearGradientBrush _emptyBrush = new LinearGradientBrush()
    {
      StartPoint = new System.Windows.Point(1.0, 0.0),
      EndPoint = new System.Windows.Point(1.0, 1.0)
    };
    private readonly Grid _titleGrid;
    private TaskListView _taskList;
    private KanbanContainer _kanban;
    private string _originalColumnName;
    private QuickAddView _quickAddView;
    private TextBox _editNameText;
    private Popup _errorPopup;
    private Rectangle _emptyBorder;
    private HoverIconButton _moreButton;
    private bool _popupOpen;
    private HoverIconButton _addButton;
    private Rectangle _draggingBorder;
    private Rectangle _dropBorder;
    private bool _canEnter;
    private TextBlock _postPoneText;

    public KanbanColumnView(ColumnViewModel model)
    {
      Grid grid = new Grid();
      grid.Background = (Brush) Brushes.Transparent;
      grid.Cursor = Cursors.Hand;
      grid.Height = 36.0;
      grid.VerticalAlignment = VerticalAlignment.Bottom;
      grid.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
      this._titleGrid = grid;
      this._canEnter = true;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.VerticalAlignment = VerticalAlignment.Stretch;
      this.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      this.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.DataContext = (object) model;
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(40.0)
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition());
      this.SetResourceReference(FrameworkElement.WidthProperty, (object) "KanbanColumnWidth");
      this.InitTitleGrid();
      this.SetShowAdd(false);
      this.InitView();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnload);
    }

    private void OnUnload(object sender, RoutedEventArgs e) => this.UnbindEvents();

    private void OnLoaded(object sender, RoutedEventArgs e) => this.BindEvents();

    static KanbanColumnView() => KanbanColumnView.SetEmptyBrushColor();

    public bool IsMouseDragOver
    {
      get
      {
        if (!this._titleGrid.IsMouseOver || this._addButton != null && this._addButton.IsMouseOver)
          return false;
        return this._moreButton == null || !this._moreButton.IsMouseOver;
      }
    }

    public bool DragMouseDown { get; set; }

    private void InitTitleGrid()
    {
      this._titleGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this._titleGrid.ColumnDefinitions.Add(new ColumnDefinition());
      this._titleGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this._titleGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this._titleGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      Border border = new Border();
      border.Margin = new Thickness(-4.0, 0.0, 0.0, 0.0);
      border.CornerRadius = new CornerRadius(4.0);
      border.VerticalAlignment = VerticalAlignment.Center;
      Border element1 = border;
      element1.SetBinding(UIElement.IsHitTestVisibleProperty, "Enable");
      element1.SetResourceReference(FrameworkElement.StyleProperty, (object) "HoverBorderStyle10");
      EmjTextBlock emjTextBlock1 = new EmjTextBlock();
      emjTextBlock1.MaxWidth = this.Width - 84.0;
      emjTextBlock1.ClipToBounds = true;
      emjTextBlock1.IsHitTestVisible = false;
      emjTextBlock1.TextWrapping = TextWrapping.Wrap;
      emjTextBlock1.Height = 18.0;
      emjTextBlock1.FontSize = 14.0;
      emjTextBlock1.Margin = new Thickness(4.0);
      emjTextBlock1.TextTrimming = TextTrimming.CharacterEllipsis;
      emjTextBlock1.FontWeight = FontWeights.Bold;
      EmjTextBlock emjTextBlock2 = emjTextBlock1;
      emjTextBlock2.SetBinding(EmjTextBlock.TextProperty, "Name");
      emjTextBlock2.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      element1.Child = (UIElement) emjTextBlock2;
      element1.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTitleClick);
      this._titleGrid.Children.Add((UIElement) element1);
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 12.0;
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.Margin = new Thickness(4.0, 1.0, 0.0, 0.0);
      TextBlock element2 = textBlock;
      element2.SetValue(Grid.ColumnProperty, (object) 1);
      element2.SetBinding(TextBlock.TextProperty, "TaskCount");
      element2.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
      this._titleGrid.Children.Add((UIElement) element2);
      this.Children.Add((UIElement) this._titleGrid);
    }

    private void OnTitleClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DragMouseDown)
        this.SetNewName(true);
      this.DragMouseDown = false;
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is ColumnViewModel oldValue)
      {
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnNotifyReload), "Reload");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnCanDropChanged), "CanDrop");
      }
      if (!(e.NewValue is ColumnViewModel newValue))
        return;
      this.ColumnId = newValue.ColumnId;
      this.SetTaskList(newValue);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnNotifyReload), "Reload");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnCanDropChanged), "CanDrop");
      this.SetDropBorder(newValue);
      this.SetMoreButton(newValue);
      this.SetAddButton(newValue);
      this.SetPostponeButton(newValue);
      this.InitView();
      this.SetShowAdd(false);
      if (!newValue.NewAdd)
        return;
      this.SetNewName(false);
    }

    public string ColumnId { get; set; }

    private void SetPostponeButton(ColumnViewModel model)
    {
      if (model.ColumnId == "date:-1" && (model.Identity.Project is TodayProjectIdentity || model.Identity.Project is WeekProjectIdentity || model.Identity.Project is AllProjectIdentity))
      {
        if (this._postPoneText != null)
          return;
        TextBlock textBlock = new TextBlock();
        textBlock.Text = Utils.GetString("Postpone");
        textBlock.FontSize = 12.0;
        textBlock.Background = (Brush) Brushes.Transparent;
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        this._postPoneText = textBlock;
        this._postPoneText.SetResourceReference(TextBlock.ForegroundProperty, (object) "ThemeBlue");
        this._postPoneText.MouseLeftButtonUp += new MouseButtonEventHandler(this.PostPoneClick);
        this._postPoneText.SetValue(Grid.ColumnProperty, (object) 2);
        this._titleGrid.Children.Add((UIElement) this._postPoneText);
      }
      else
      {
        if (this._postPoneText == null)
          return;
        this._titleGrid.Children.Remove((UIElement) this._postPoneText);
        this._postPoneText = (TextBlock) null;
      }
    }

    private void PostPoneClick(object sender, MouseButtonEventArgs e)
    {
      if (!new CustomerDialog(Utils.GetString("PostponeToTodayTitle"), Utils.GetString("PostponeToTodayMsg"), Utils.GetString("Postpone"), Utils.GetString("Cancel"), (Window) App.Window).ShowDialog().GetValueOrDefault())
        return;
      TaskUtils.PostPoneTasks(this._taskList);
    }

    private void OnCanDropChanged(object sender, PropertyChangedEventArgs e)
    {
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      this.SetDropBorder(model);
    }

    private void SetDropBorder(ColumnViewModel model)
    {
      if (model.CanDrop)
      {
        if (this._dropBorder != null)
          return;
        Rectangle rectangle = new Rectangle();
        rectangle.RadiusX = 6.0;
        rectangle.RadiusY = 6.0;
        rectangle.StrokeThickness = 1.0;
        this._dropBorder = rectangle;
        this._dropBorder.SetResourceReference(Shape.StrokeProperty, (object) "PrimaryColor");
        this._dropBorder.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor10");
        this._dropBorder.SetValue(Panel.ZIndexProperty, (object) -5);
        this._dropBorder.SetValue(Grid.RowSpanProperty, (object) 3);
        this.Children.Add((UIElement) this._dropBorder);
      }
      else
      {
        this.Children.Remove((UIElement) this._dropBorder);
        this._dropBorder = (Rectangle) null;
      }
    }

    private void SetAddButton(ColumnViewModel model)
    {
      if (model == null)
        return;
      if (!model.CanAdd || model.NewAdd || LocalSettings.Settings.ExtraSettings.KbShowAdd)
      {
        this._titleGrid.Children.Remove((UIElement) this._addButton);
        this._addButton = (HoverIconButton) null;
      }
      else
      {
        if (this._addButton != null)
          return;
        HoverIconButton hoverIconButton = new HoverIconButton();
        hoverIconButton.Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
        this._addButton = hoverIconButton;
        this._addButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "AddDrawingImage");
        this._addButton.SetResourceReference(FrameworkElement.ToolTipProperty, (object) "AddaTask");
        this._addButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.AddTaskClick);
        this._addButton.SetValue(Grid.ColumnProperty, (object) 3);
        this._titleGrid.Children.Add((UIElement) this._addButton);
      }
    }

    private void AddTaskClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.InitQuickAddView();
    }

    private void OnNotifyReload(object sender, PropertyChangedEventArgs e)
    {
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      this.SetTaskList(model);
    }

    private void SetMoreButton(ColumnViewModel model)
    {
      if (!model.Enable || model.NewAdd)
      {
        this._titleGrid.Children.Remove((UIElement) this._moreButton);
        this._moreButton = (HoverIconButton) null;
      }
      else
      {
        if (this._moreButton != null)
          return;
        HoverIconButton hoverIconButton = new HoverIconButton();
        hoverIconButton.Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
        this._moreButton = hoverIconButton;
        this._moreButton.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "MoreDrawingImage");
        this._moreButton.SetResourceReference(FrameworkElement.ToolTipProperty, (object) "More");
        this._moreButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoreGridClick);
        this._moreButton.SetValue(Grid.ColumnProperty, (object) 4);
        this._titleGrid.Children.Add((UIElement) this._moreButton);
      }
    }

    private void MoreGridClick(object sender, MouseButtonEventArgs e)
    {
      ColumnViewModel model = this.GetModel();
      ColumnViewModel columnViewModel = model;
      if ((columnViewModel != null ? (columnViewModel.Enable ? 1 : 0) : 1) == 0)
        return;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "rename", Utils.GetString("Rename"), Utils.GetImageSource("EditDrawingImage")),
        new CustomMenuItemViewModel((object) "addToLeft", Utils.GetString("AddColumnToTheLeft"), Utils.GetImageSource("AddToLeftDrawingImage")),
        new CustomMenuItemViewModel((object) "addToRight", Utils.GetString("AddColumnToTheRight"), Utils.GetImageSource("AddToRightDrawingImage"))
      };
      KanbanContainer kanban = this._kanban;
      // ISSUE: explicit non-virtual call
      if ((kanban != null ? (__nonvirtual (kanban.GetColumnCount()) > 1 ? 1 : 0) : 0) != 0)
        types.Add(new CustomMenuItemViewModel((object) "delete", Utils.GetString("Delete"), Utils.GetImageSource("DeleteDrawingLine")));
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.PlacementTarget = (UIElement) this._moreButton;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.VerticalOffset = -5.0;
      escPopup.HorizontalOffset = -102.0;
      EscPopup popup = escPopup;
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) popup);
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) customMenuList.SubPopup, new ProjectExtra()
      {
        ProjectIds = new List<string>() { model?.ProjectId }
      }, new ProjectSelectorExtra()
      {
        showAll = false,
        batchMode = false,
        canSelectGroup = false,
        CanSearch = true,
        onlyShowPermission = true,
        ShowColumn = false
      });
      types.Insert(3, new CustomMenuItemViewModel((object) "move", Utils.GetString("MoveTo"), Utils.GetIcon("IcMovetoLine"))
      {
        SubControl = (ITabControl) projectOrGroupPopup,
        NeedSubContentStyle = false
      });
      projectOrGroupPopup.ItemSelect += (EventHandler<SelectableItemViewModel>) ((o, viewModel) =>
      {
        popup.IsOpen = false;
        this.OnMoveColumnItemSelected(model, viewModel);
      });
      popup.Opened += (EventHandler) ((o, a) => this._popupOpen = true);
      popup.Closed += (EventHandler) ((o, a) => this._popupOpen = false);
      customMenuList.Operated += new EventHandler<object>(this.OnMoreItemSelected);
      customMenuList.Show();
    }

    private async void OnMoveColumnItemSelected(ColumnViewModel model, SelectableItemViewModel e)
    {
      await Task.Delay(50);
      string id = e.Id;
      if (!(id != model.ProjectId))
        return;
      TaskService.MoveColumnAsync(model.ColumnId, model.ProjectId, id);
    }

    private void OnMoreItemSelected(object sender, object e)
    {
      if (!(e is string str))
        return;
      switch (str)
      {
        case "rename":
          this.SetNewName(true);
          break;
        case "addToLeft":
          this._kanban?.AddColumn(this.GetModel()?.ColumnId, HorizontalDirection.Left);
          break;
        case "addToRight":
          this._kanban?.AddColumn(this.GetModel()?.ColumnId, HorizontalDirection.Right);
          break;
        case "delete":
          this._kanban?.DeleteColumn(this.GetModel()?.ColumnId, true);
          break;
      }
    }

    private void SetTaskList(ColumnViewModel model)
    {
      if (model.SourceItems.Count > 0)
      {
        if (this._taskList == null)
        {
          TaskListView taskListView = new TaskListView();
          taskListView.Margin = new Thickness(0.0, 6.0, 0.0, 8.0);
          taskListView.VerticalAlignment = VerticalAlignment.Top;
          this._taskList = taskListView;
          this._taskList.SetValue(Grid.RowProperty, (object) 2);
          this.Children.Add((UIElement) this._taskList);
          try
          {
            this._taskList.ViewModel.SetKanbanParent(this._kanban.GetViewModel());
          }
          catch (Exception ex)
          {
          }
          this._taskList.ItemsCountChanged += new EventHandler(this.OnTaskCountChanged);
        }
        if (model.Identity != null)
        {
          SectionAddTaskViewModel addingModel = this._kanban?.AddingModel;
          this._taskList.SetIdentity((ProjectIdentity) model.Identity, TaskListDisplayType.Kanban);
          this._taskList.SetAddingTask(false);
          this._taskList.SortItemsAndLoad(model.SourceItems, addingModel == null || !(addingModel.ColumnId == model.ColumnId) || addingModel.Invalid ? (SectionAddTaskViewModel) null : addingModel);
        }
        this._taskList.ViewModel.BatchEditor = (IBatchEditable) this._kanban;
        model.SetTaskListViewModel(this._taskList.ViewModel);
        this.Children.Remove((UIElement) this._emptyBorder);
        this._emptyBorder = (Rectangle) null;
        this.Height = double.NaN;
      }
      else
      {
        this.Children.Remove((UIElement) this._taskList);
        this._taskList = (TaskListView) null;
        model.SetTaskListViewModel((TaskListViewModel) null);
        if (this._taskList != null)
          this._taskList.ItemsCountChanged -= new EventHandler(this.OnTaskCountChanged);
        if (this._emptyBorder == null)
        {
          Rectangle rectangle = new Rectangle();
          rectangle.RadiusX = 6.0;
          rectangle.RadiusY = 6.0;
          rectangle.Margin = new Thickness(12.0, 8.0, 12.0, 0.0);
          rectangle.VerticalAlignment = VerticalAlignment.Stretch;
          rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
          rectangle.IsHitTestVisible = false;
          rectangle.Fill = (Brush) KanbanColumnView._emptyBrush;
          this._emptyBorder = rectangle;
          this._emptyBorder.SetValue(Grid.RowProperty, (object) 2);
          this.Children.Add((UIElement) this._emptyBorder);
        }
        if (double.IsInfinity(this.MaxHeight))
          return;
        this.Height = this.MaxHeight;
      }
    }

    public static void SetEmptyBrushColor()
    {
      Color color1 = LocalSettings.Settings.ThemeId == "Dark" ? Colors.White : (LocalSettings.Settings.ThemeId == "White" ? Colors.Black : ThemeUtil.GetColorValue("ColorPrimary"));
      double num = LocalSettings.Settings.ThemeId == "Dark" ? 0.02 : (LocalSettings.Settings.ThemeId == "White" ? 0.04 : 0.05);
      Color color2 = Color.FromArgb(byte.MaxValue, color1.R, color1.G, color1.B);
      Color color3 = Color.FromArgb((byte) 0, color1.R, color1.G, color1.B);
      KanbanColumnView._emptyBrush.Opacity = num;
      KanbanColumnView._emptyBrush.GradientStops.Clear();
      KanbanColumnView._emptyBrush.GradientStops.Add(new GradientStop(color2, 0.0));
      KanbanColumnView._emptyBrush.GradientStops.Add(new GradientStop(color3, 1.0));
    }

    public void UnbindEvents()
    {
      if (this._taskList != null)
        this._taskList.ItemsCountChanged -= new EventHandler(this.OnTaskCountChanged);
      ItemDragNotifier.MouseMove -= new EventHandler<MouseEventArgs>(this.OnDragMouseMove);
      if (this._kanban == null)
        return;
      this._kanban.SetSelected -= new EventHandler<List<string>>(this.OnSetSelected);
      this._kanban = (KanbanContainer) null;
    }

    private void BindEvents()
    {
      ItemDragNotifier.MouseMove += new EventHandler<MouseEventArgs>(this.OnDragMouseMove);
      this._kanban = Utils.FindParent<KanbanContainer>((DependencyObject) this);
      if (this._kanban == null)
        return;
      this._kanban.SetSelected -= new EventHandler<List<string>>(this.OnSetSelected);
      this._kanban.SetSelected += new EventHandler<List<string>>(this.OnSetSelected);
      if (this._taskList == null)
        return;
      this._taskList.ViewModel.BatchEditor = (IBatchEditable) this._kanban;
    }

    private void OnDragMouseMove(object sender, MouseEventArgs e)
    {
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      DisplayItemModel draggingTask = this._kanban?.GetDraggingTask();
      if (draggingTask == null || draggingTask.IsNote && model.ColumnId.StartsWith("priority:"))
        return;
      double x = e.GetPosition((IInputElement) this).X;
      if ((x < 0.0 ? 0 : (x <= this.ActualWidth ? 1 : 0)) != 0)
      {
        if (draggingTask.ColumnId != model.ColumnId)
        {
          model.CanDrop = draggingTask.IsTaskOrNote && draggingTask.ColumnId != "note" || draggingTask.IsItem && model.Identity.GetRealSortOption().groupBy == "dueDate";
        }
        else
        {
          model.CanDrop = false;
          this._taskList?.OnKanbanItemDragMove(draggingTask, e);
        }
        model.MouseOver = true;
      }
      else
      {
        model.MouseOver = false;
        model.CanDrop = false;
      }
    }

    private void OnSetSelected(object sender, List<string> e) => this._taskList?.SetSelected(e);

    private async void OnTaskCountChanged(object sender, EventArgs e)
    {
      await Task.Delay(100);
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      ColumnViewModel columnViewModel = model;
      TaskListView taskList = this._taskList;
      int num = taskList != null ? taskList.ViewModel.SourceModels.Count<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (i => i.Status == 0)) : 0;
      columnViewModel.TaskCount = num;
    }

    private void InitView(bool focus = false, bool resetQuickAdd = false)
    {
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      this._originalColumnName = model.Name;
      if (this._quickAddView == null || !resetQuickAdd && !(this._quickAddView.Model.ColumnId != model.ColumnId) && !string.IsNullOrEmpty(this._quickAddView.TitleText.EditBox.Text))
        return;
      this._quickAddView.Reset(AddTaskViewModel.Build((IProjectTaskDefault) new ColumnProjectIdentity((ProjectIdentity) model.Identity, model.ColumnId)), QuickAddView.Scenario.Kanban, focus);
    }

    private async void InitQuickAddView(bool withAnim = true, bool focus = true)
    {
      KanbanColumnView kanbanColumnView1 = this;
      ColumnViewModel model = kanbanColumnView1.GetModel();
      if (model == null)
        return;
      if (kanbanColumnView1._addButton != null)
        kanbanColumnView1._addButton.IsHitTestVisible = false;
      if (kanbanColumnView1._quickAddView == null)
      {
        int to = 108;
        KanbanColumnView kanbanColumnView2 = kanbanColumnView1;
        QuickAddView quickAddView = new QuickAddView((IProjectTaskDefault) new ColumnProjectIdentity(model.Identity.Project, model.ColumnId), QuickAddView.Scenario.Kanban);
        quickAddView.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
        quickAddView.MaxHeight = 0.0;
        kanbanColumnView2._quickAddView = quickAddView;
        kanbanColumnView1._quickAddView.TaskAdded += new EventHandler<TaskModel>(kanbanColumnView1.OnTaskAdded);
        kanbanColumnView1._quickAddView.BatchTaskAdded += new EventHandler<List<TaskModel>>(kanbanColumnView1.OnBatchTaskAdded);
        kanbanColumnView1._quickAddView.SetValue(Grid.RowProperty, (object) 1);
        kanbanColumnView1.Children.Add((UIElement) kanbanColumnView1._quickAddView);
        if (withAnim)
        {
          kanbanColumnView1._quickAddView.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(0.0), (double) to, 240));
        }
        else
        {
          kanbanColumnView1._quickAddView.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) null);
          kanbanColumnView1._quickAddView.MaxHeight = (double) to;
        }
        kanbanColumnView1._quickAddView.TitleText.EditBox.LostFocus += new RoutedEventHandler(kanbanColumnView1.OnQuickAddLostFocus);
        if (focus)
          kanbanColumnView1._quickAddView.FocusEnd();
      }
      if (!LocalSettings.Settings.ExtraSettings.KbShowAdd)
        return;
      kanbanColumnView1._quickAddView.TitleText.EditBox.LostFocus -= new RoutedEventHandler(kanbanColumnView1.OnQuickAddLostFocus);
      await Task.Delay(120);
      kanbanColumnView1._quickAddView?.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) null);
      kanbanColumnView1._quickAddView?.SetResourceReference(FrameworkElement.MaxHeightProperty, (object) 108);
    }

    private async void OnQuickAddLostFocus(object sender, RoutedEventArgs e)
    {
      await Task.Delay(100);
      if (this._quickAddView == null || !this._quickAddView.IsLostFocus())
        return;
      await this._quickAddView?.TryAddTaskOnLostFocus();
      this.HideAddView();
    }

    private void HideAddView()
    {
      QuickAddView addView = this._quickAddView;
      this._quickAddView = (QuickAddView) null;
      if (addView == null)
        return;
      addView.TaskAdded -= new EventHandler<TaskModel>(this.OnTaskAdded);
      addView.BatchTaskAdded -= new EventHandler<List<TaskModel>>(this.OnBatchTaskAdded);
      addView.TitleText.LostFocus -= new RoutedEventHandler(this.OnQuickAddLostFocus);
      DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(108.0), 0.0, 120);
      doubleAnimation.Completed += (EventHandler) ((o, a) =>
      {
        if (this._addButton != null)
          this._addButton.IsHitTestVisible = true;
        this.Children.Remove((UIElement) addView);
      });
      addView.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) doubleAnimation);
    }

    public void ResetQuickAddView(bool keepText = true)
    {
      QuickAddView quickAddView = this._quickAddView;
      if (!this.IsVisible || quickAddView == null)
        return;
      ColumnViewModel model = this.GetModel();
      if (model?.Identity == null)
        return;
      quickAddView.ResetView((IProjectTaskDefault) model.Identity, keepText: keepText);
    }

    private void OnBatchTaskAdded(object sender, List<TaskModel> e) => SyncManager.TryDelaySync();

    private void OnTaskAdded(object sender, TaskModel model)
    {
      this._kanban?.CancelOperation();
      List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this.GetModel().Identity.Project, new List<string>()
      {
        model.id
      });
      if (matchedTasks == null || matchedTasks.Count == 0)
      {
        if (CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.projectId)) == null)
          return;
        Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.ToastMoveProjectControl(model.projectId, moveType: MoveToastType.Add);
      }
      else
      {
        ColumnViewModel model1 = this.GetModel();
        if (model1 == null)
          return;
        model1.CanDrop = false;
        if (!(model.projectId == model1.ProjectId) || !(model.columnId == model1.ColumnId) && (!model1.IsPinned || model.pinnedTimeStamp <= 0L))
          return;
        ++model1.TaskCount;
      }
    }

    public void TryFocus() => this._editNameText.Focus();

    private async void SetNewName(bool rename)
    {
      KanbanColumnView kanbanColumnView1 = this;
      kanbanColumnView1._titleGrid.Visibility = Visibility.Collapsed;
      if (kanbanColumnView1._editNameText == null)
      {
        KanbanColumnView kanbanColumnView2 = kanbanColumnView1;
        TextBox textBox = new TextBox();
        textBox.Height = 36.0;
        textBox.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
        textBox.VerticalAlignment = VerticalAlignment.Bottom;
        kanbanColumnView2._editNameText = textBox;
        kanbanColumnView1.Children.Add((UIElement) kanbanColumnView1._editNameText);
        kanbanColumnView1._editNameText.SetResourceReference(FrameworkElement.StyleProperty, (object) "HintEditTextStyle");
        kanbanColumnView1._editNameText.LostFocus += new RoutedEventHandler(kanbanColumnView1.OnNameLostFocus);
        kanbanColumnView1._editNameText.TextChanged += new TextChangedEventHandler(kanbanColumnView1.OnNameTextChanged);
        kanbanColumnView1._editNameText.PreviewKeyUp += new KeyEventHandler(kanbanColumnView1.OnEditNameKeyUp);
        kanbanColumnView1._editNameText.PreviewKeyDown += new KeyEventHandler(kanbanColumnView1.OnEditNameKeyDown);
        kanbanColumnView1.SetEditing();
        kanbanColumnView1._editNameText.Focus();
      }
      if (rename)
      {
        kanbanColumnView1._editNameText.Text = kanbanColumnView1.GetModel()?.Name ?? string.Empty;
        kanbanColumnView1._editNameText.SelectAll();
      }
      else
      {
        await Task.Delay(20);
        kanbanColumnView1._editNameText.Text = string.Empty;
        kanbanColumnView1._editNameText.Focus();
        kanbanColumnView1._editNameText.SelectAll();
      }
    }

    private void OnEditNameKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.ImeProcessed)
        return;
      this._canEnter = false;
    }

    private void OnEditNameKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (!this._canEnter)
            break;
          goto case Key.Escape;
        case Key.Escape:
          this.OnNameLostFocus((object) null, (RoutedEventArgs) null);
          e.Handled = true;
          break;
      }
      this._canEnter = true;
    }

    private void SetEditing()
    {
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      model.Editing = true;
    }

    private async void OnNameLostFocus(object sender, RoutedEventArgs e)
    {
      KanbanColumnView column = this;
      ColumnViewModel model = column.GetModel();
      if (model == null)
        return;
      model.Editing = false;
      if (!model.NewAdd || !string.IsNullOrEmpty(column._editNameText?.Text))
      {
        await column.SaveColumnName();
      }
      else
      {
        // ISSUE: explicit non-virtual call
        if (!string.IsNullOrEmpty(column._editNameText?.Text) || __nonvirtual (column.IsMouseOver))
          return;
        column._kanban?.DeleteColumn(column);
      }
    }

    private async Task SaveColumnName()
    {
      KanbanColumnView kanbanColumnView = this;
      ColumnViewModel model = kanbanColumnView.GetModel();
      TextBox editText;
      KanbanContainer kanban;
      if (model == null)
      {
        model = (ColumnViewModel) null;
        editText = (TextBox) null;
        kanban = (KanbanContainer) null;
      }
      else
      {
        editText = kanbanColumnView._editNameText;
        kanban = kanbanColumnView._kanban ?? Utils.FindParent<KanbanContainer>((DependencyObject) kanbanColumnView);
        if (editText == null)
        {
          model = (ColumnViewModel) null;
          editText = (TextBox) null;
          kanban = (KanbanContainer) null;
        }
        else
        {
          string saveName = editText.Text.Trim();
          if (!NameUtils.IsValidColumnName(saveName))
          {
            editText.SelectAll();
            kanbanColumnView.SetErrorPopup(true);
            model = (ColumnViewModel) null;
            editText = (TextBox) null;
            kanban = (KanbanContainer) null;
          }
          else if (string.IsNullOrEmpty(saveName))
          {
            editText.Text = kanbanColumnView._originalColumnName;
            model.Name = kanbanColumnView._originalColumnName;
            editText.LostFocus -= new RoutedEventHandler(kanbanColumnView.OnNameLostFocus);
            editText.TextChanged -= new TextChangedEventHandler(kanbanColumnView.OnNameTextChanged);
            kanbanColumnView.Children.Remove((UIElement) kanbanColumnView._editNameText);
            kanbanColumnView._editNameText = (TextBox) null;
            kanbanColumnView._titleGrid.Visibility = Visibility.Visible;
            Utils.Toast(Utils.GetString("SectionNameCannotBeEmpty"));
            if (!kanbanColumnView.GetModel().NewAdd)
            {
              model = (ColumnViewModel) null;
              editText = (TextBox) null;
              kanban = (KanbanContainer) null;
            }
            else
            {
              KanbanContainer kanbanContainer = kanban;
              if (kanbanContainer == null)
              {
                model = (ColumnViewModel) null;
                editText = (TextBox) null;
                kanban = (KanbanContainer) null;
              }
              else
              {
                kanbanContainer.DeleteColumn(kanbanColumnView);
                model = (ColumnViewModel) null;
                editText = (TextBox) null;
                kanban = (KanbanContainer) null;
              }
            }
          }
          else
          {
            bool flag = saveName != model.Name;
            if (flag)
              flag = await kanbanColumnView.CheckIfColumnNameExisted(saveName);
            if (flag)
            {
              editText.Text = kanbanColumnView._originalColumnName;
              kanbanColumnView.GetModel().Name = kanbanColumnView._originalColumnName;
              editText.LostFocus -= new RoutedEventHandler(kanbanColumnView.OnNameLostFocus);
              editText.TextChanged -= new TextChangedEventHandler(kanbanColumnView.OnNameTextChanged);
              kanbanColumnView.Children.Remove((UIElement) kanbanColumnView._editNameText);
              kanbanColumnView._editNameText = (TextBox) null;
              kanbanColumnView._titleGrid.Visibility = Visibility.Visible;
              Utils.Toast(Utils.GetString("SectionNameExisted"));
              if (!kanbanColumnView.GetModel().NewAdd)
              {
                model = (ColumnViewModel) null;
                editText = (TextBox) null;
                kanban = (KanbanContainer) null;
              }
              else
              {
                KanbanContainer kanbanContainer = kanban;
                if (kanbanContainer == null)
                {
                  model = (ColumnViewModel) null;
                  editText = (TextBox) null;
                  kanban = (KanbanContainer) null;
                }
                else
                {
                  kanbanContainer.DeleteColumn(kanbanColumnView);
                  model = (ColumnViewModel) null;
                  editText = (TextBox) null;
                  kanban = (KanbanContainer) null;
                }
              }
            }
            else
            {
              if (model.NewAdd)
              {
                ColumnModel columnModel = await ColumnDao.AddColumn(saveName, model.ProjectId, model.SortOrder);
              }
              else
                await ColumnDao.SaveColumnName(saveName, model.ColumnId, model.NewAdd);
              kanbanColumnView._originalColumnName = saveName;
              kanbanColumnView.GetModel().Name = saveName;
              editText.LostFocus -= new RoutedEventHandler(kanbanColumnView.OnNameLostFocus);
              editText.TextChanged -= new TextChangedEventHandler(kanbanColumnView.OnNameTextChanged);
              kanbanColumnView.Children.Remove((UIElement) kanbanColumnView._editNameText);
              kanbanColumnView._editNameText = (TextBox) null;
              kanbanColumnView._titleGrid.Visibility = Visibility.Visible;
              if (model.NewAdd)
              {
                model.NewAdd = false;
                model.Editing = false;
                kanban?.Reload(true, true);
              }
              SyncManager.TryDelaySync();
              saveName = (string) null;
              model = (ColumnViewModel) null;
              editText = (TextBox) null;
              kanban = (KanbanContainer) null;
            }
          }
        }
      }
    }

    private void SetErrorPopup(bool show)
    {
      if (show)
      {
        if (this._errorPopup != null || this._editNameText == null)
          return;
        this._errorPopup = new Popup()
        {
          StaysOpen = true,
          PlacementTarget = (UIElement) this._editNameText,
          Placement = PlacementMode.Bottom,
          AllowsTransparency = true
        };
        ContentControl contentControl = new ContentControl();
        contentControl.SetResourceReference(FrameworkElement.StyleProperty, (object) "PopupContentStyle");
        TextBlock textBlock1 = new TextBlock();
        textBlock1.Text = Utils.GetString("SectionNotValid");
        textBlock1.FontSize = 11.0;
        textBlock1.Margin = new Thickness(6.0);
        TextBlock textBlock2 = textBlock1;
        textBlock2.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
        contentControl.Content = (object) textBlock2;
        this._errorPopup.Child = (UIElement) contentControl;
        this._errorPopup.IsOpen = true;
      }
      else
      {
        if (this._errorPopup == null)
          return;
        this._errorPopup.IsOpen = false;
        this._errorPopup = (Popup) null;
      }
    }

    private async Task<bool> CheckIfColumnNameExisted(string name)
    {
      return !(name == this._originalColumnName) && this._kanban != null && (await this._kanban.GetColumnNames()).Contains(name);
    }

    private void OnNameTextChanged(object sender, TextChangedEventArgs e)
    {
      if (string.IsNullOrEmpty(this._editNameText.Text.TrimEnd()))
        return;
      this.CheckIfColumnNameValid();
    }

    private void CheckIfColumnNameValid()
    {
      if (!NameUtils.IsValidColumnName(this._editNameText.Text))
      {
        this.SetErrorPopup(true);
        this._editNameText.SelectAll();
      }
      else
        this.SetErrorPopup(false);
    }

    public bool QuickAddPopupOpened()
    {
      QuickAddView quickAddView = this._quickAddView;
      return quickAddView != null && quickAddView.IsInOperation;
    }

    public async void OnTaskDrop(DisplayItemModel model)
    {
      ColumnViewModel model1 = this.GetModel();
      if (model1 == null || !(model.ColumnId == model1.ColumnId) || this._taskList == null)
        return;
      await this._taskList.OnTaskDrop(model);
      this.ReloadColumn();
    }

    public bool TaskChanged { get; set; }

    public string GetColumnId() => this.GetModel()?.ColumnId;

    public void ReloadColumn() => this._taskList?.ReLoad((string) null);

    public List<DisplayItemModel> RemoveTasks(List<string> selected, string except)
    {
      return this._taskList?.ViewModel.RemoveItemByIds(selected, except);
    }

    public void RemoveTaskChildren(DisplayItemModel model)
    {
      TaskListView taskList = this._taskList;
      if (taskList == null)
        return;
      TaskListViewModel viewModel = taskList.ViewModel;
      List<string> taskIds = new List<string>();
      taskIds.Add(model.Id);
      string id = model.Id;
      viewModel.RemoveItemByIds(taskIds, id);
    }

    public async Task OnBatchTaskDrop(DisplayItemModel dragModel)
    {
      if (this._taskList == null)
        return;
      await this._taskList.OnBatchTaskDrop(dragModel);
    }

    public async void TryShowTaskDetail(string taskModelId, bool focusTitle = false)
    {
      this.GetModel();
      if (this._kanban == null)
        ;
      else
      {
        await Task.Delay(120);
        TaskListView taskList = this._taskList;
        DisplayItemModel item = taskList != null ? taskList.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Id == taskModelId)) : (DisplayItemModel) null;
        if (item != null)
        {
          KanbanItemView singleVisualChildren = Utils.FindSingleVisualChildren<KanbanItemView>((DependencyObject) this._taskList?.GetItemByModel(item));
          if (singleVisualChildren != null)
          {
            singleVisualChildren.SelectItem(focusTitle);
            return;
          }
          this._taskList?.ScrollToItemById(item.Id);
          await Task.Delay(120);
          Utils.FindSingleVisualChildren<KanbanItemView>((DependencyObject) this._taskList?.GetItemByModel(item))?.SelectItem(focusTitle);
        }
        else
          TaskDetailWindows.ShowTaskWindows(taskModelId);
        item = (DisplayItemModel) null;
      }
    }

    public ColumnViewModel GetModel()
    {
      return this.DataContext != null && this.DataContext is ColumnViewModel dataContext ? dataContext : (ColumnViewModel) null;
    }

    public bool IsPopupOpen() => this.QuickAddPopupOpened();

    public async Task ExpandOrFoldAllTask(bool isOpen)
    {
      if (this._taskList == null)
        return;
      await this._taskList.ExpandOrFoldAllTask();
    }

    public void SetMaxHeight(double maxHeight)
    {
      this.MaxHeight = maxHeight;
      if (this._emptyBorder == null)
        return;
      this.Height = this.MaxHeight;
    }

    public void SetColumnDragging(bool dragging)
    {
      if (dragging)
      {
        if (this._draggingBorder != null)
          return;
        this._draggingBorder = new Rectangle()
        {
          RadiusX = 6.0,
          RadiusY = 6.0
        };
        this._draggingBorder.SetResourceReference(Shape.FillProperty, (object) "KanbanColumnBackground");
        this._draggingBorder.SetValue(Panel.ZIndexProperty, (object) -5);
        this._draggingBorder.SetValue(Grid.RowSpanProperty, (object) 3);
        this._draggingBorder.Effect = (Effect) new DropShadowEffect()
        {
          Opacity = 0.16,
          BlurRadius = 18.0,
          ShadowDepth = 0.0,
          Direction = 270.0
        };
        this.Children.Add((UIElement) this._draggingBorder);
      }
      else
      {
        this.Children.Remove((UIElement) this._draggingBorder);
        this._draggingBorder = (Rectangle) null;
      }
    }

    public bool CanDragEvent()
    {
      ColumnViewModel model = this.GetModel();
      return model != null && model.ColumnId != "calendar" && model.ColumnId != "course";
    }

    public void SetShowAdd(bool withAnim = true)
    {
      if (LocalSettings.Settings.ExtraSettings.KbShowAdd)
      {
        ColumnViewModel model = this.GetModel();
        if (model != null && model.CanAdd && !model.NewAdd)
          this.InitQuickAddView(withAnim, false);
        else
          this.HideAddView();
      }
      else if (this._quickAddView != null && this._quickAddView.IsLostFocus())
        this.HideAddView();
      this.SetAddButton(this.GetModel());
    }
  }
}
