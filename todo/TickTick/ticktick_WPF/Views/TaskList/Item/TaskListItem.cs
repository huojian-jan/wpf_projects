// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.Item.TaskListItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Tag;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.TaskList.Item
{
  public class TaskListItem : Canvas, ITaskOperation, IShowTaskDetailWindow
  {
    private DisplayItemController _controller;
    private bool _startDrag;
    private bool _mouseDown;
    private System.Windows.Point _startPoint;
    private DisplayItemModel _model;
    private ListItemTagsControl _tagPanel;
    private TaskItemPomoSummaryControl _pomoSummary;
    private ListItemProjectLabel _projectLabel;
    private CommentSearchLabel _commentLabel;
    private DateTime _lastClickTime;
    private bool _isWaitingDoubleClick;
    private System.Windows.Point _firstClickPoint;
    private bool _isLocked;
    private int _startLeft;
    private Border _dragBorder;
    private Border _moreBorder;
    private Border _backBorder;
    private static readonly Geometry DragBarIcon = Utils.GetIcon(nameof (DragBarIcon));
    private static readonly Geometry MoreIcon = Utils.GetIcon("IcThreeDots");
    private Canvas _itemContainer;
    private Rectangle _colorRect;
    private Rectangle _avatar;
    private Line _bottomLine;
    public ListItemContent TitleContent;
    private TextBlock _noteTimeText;
    private TextBlock ContentText;
    private TaskItemIcons Icons = new TaskItemIcons();
    private int _tagMaxCount = 2;
    private bool _showDetail;
    private double _originWidth;
    private CompleteStory _completeStoryView;
    private TextBlock _csTextBlock;
    private Border _csBackBorder;
    private bool _unloaded;

    private bool InMatrix { get; set; }

    private bool InPreview { get; set; }

    private bool InList { get; set; }

    public TaskListItem()
    {
      this.InList = true;
      this.InitControl();
      this.Unloaded += (RoutedEventHandler) ((o, e) => this.RemoveBinding());
      this.Loaded += (RoutedEventHandler) ((o, e) => this.ReloadData());
      this.ClipToBounds = true;
    }

    public TaskListItem(bool inMatrix, bool inPreview)
    {
      this.InMatrix = inMatrix;
      this.InPreview = inPreview;
      this.InitControl();
    }

    private void LoadContent()
    {
      Canvas canvas = new Canvas();
      canvas.Background = (Brush) Brushes.Transparent;
      this._itemContainer = canvas;
      this._itemContainer.ClipToBounds = true;
      this._itemContainer.SetBinding(FrameworkElement.HeightProperty, (BindingBase) new Binding()
      {
        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof (TaskListItem), 1),
        Path = new PropertyPath((object) FrameworkElement.ActualHeightProperty)
      });
      this.Children.Add((UIElement) this._itemContainer);
      Canvas.SetLeft((UIElement) this._itemContainer, 12.0);
      this._itemContainer.MouseEnter += new MouseEventHandler(this.OnContentMouseEnter);
      this._itemContainer.MouseLeave += new MouseEventHandler(this.OnContentMouseLeave);
      this.TitleContent = new ListItemContent();
      this.TitleContent.Width = this._itemContainer.Width - 4.0;
      this._itemContainer.Children.Add((UIElement) this.TitleContent);
      this._itemContainer.SetValue(Panel.ZIndexProperty, (object) 100);
      Canvas.SetLeft((UIElement) this.TitleContent, 0.0);
      if (this.InMatrix || this.InPreview)
        return;
      this.TitleContent.TitleTextBox.SetupSearchRender(true, false);
    }

    private void LoadIcons()
    {
      this._itemContainer.Children.Add((UIElement) this.Icons);
      this.Icons.SizeChanged += new SizeChangedEventHandler(this.OnIconsSizeChanged);
      this.Icons.SetResourceReference(Canvas.TopProperty, (object) "Double10Add3");
    }

    private void OnIconsSizeChanged(object sender, SizeChangedEventArgs e) => this.SetItemsLeft();

    private void SetTitleWidth(double extraWidth = 0.0)
    {
      this.TitleContent.Width = Math.Max(0.0, this._itemContainer.Width - extraWidth);
    }

    private void ShowOrHideDragBar(bool show)
    {
      DisplayItemModel model = this.GetModel();
      if (model == null)
        return;
      show = show && !this._isLocked;
      if (show && !TaskDragHelpModel.DragHelp.IsDragging && model.Enable && model.ShowDragBar && !model.IsHabit && !this.InMatrix)
      {
        this.ShowDragBar();
      }
      else
      {
        if (this._dragBorder == null)
          return;
        try
        {
          this.Children.Remove((UIElement) this._dragBorder);
          this._dragBorder = (Border) null;
        }
        catch (Exception ex)
        {
        }
      }
    }

    private DisplayItemModel GetModel() => this.DataContext as DisplayItemModel;

    private void ShowDragBar()
    {
      if (this._dragBorder == null)
      {
        Border border = new Border();
        border.Width = 18.0;
        border.HorizontalAlignment = HorizontalAlignment.Left;
        border.Cursor = Cursors.SizeAll;
        this._dragBorder = border;
        this._dragBorder.SetValue(Panel.ZIndexProperty, (object) 10);
        this._dragBorder.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnDragMouseDown);
        this._dragBorder.MouseMove += new MouseEventHandler(this.OnMouseMove);
        this._dragBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDragMouseUp);
        this._dragBorder.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_100");
        Path path1 = new Path();
        path1.Width = 12.0;
        path1.Height = 12.0;
        path1.HorizontalAlignment = HorizontalAlignment.Center;
        path1.VerticalAlignment = VerticalAlignment.Center;
        path1.Stretch = Stretch.Uniform;
        Path path2 = path1;
        path2.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity100_80");
        path2.Data = TaskListItem.DragBarIcon;
        this._dragBorder.Child = (UIElement) path2;
      }
      this._dragBorder.Height = this.Height;
      if (!this.Children.Contains((UIElement) this._dragBorder))
        this.Children.Add((UIElement) this._dragBorder);
      Canvas.SetTop((UIElement) this._dragBorder, 0.0);
      Canvas.SetLeft((UIElement) this._dragBorder, (double) this._startLeft);
    }

    private void ShowOrHideMoreIcon(bool show)
    {
      DisplayItemModel model = this.GetModel();
      if (model == null)
        return;
      show = show && !this._isLocked;
      if (show && !TaskDragHelpModel.DragHelp.IsDragging && model.Enable && model.HitVisible && !this.InMatrix)
      {
        this.ShowMoreIcon();
      }
      else
      {
        if (this._moreBorder == null)
          return;
        this.Children.Remove((UIElement) this._moreBorder);
        this._moreBorder = (Border) null;
      }
    }

    private void ShowMoreIcon()
    {
      if (this._moreBorder == null)
      {
        Border border = new Border();
        border.Width = 10.0;
        border.HorizontalAlignment = HorizontalAlignment.Right;
        border.Cursor = Cursors.Hand;
        this._moreBorder = border;
        this._moreBorder.SetValue(Panel.ZIndexProperty, (object) 10);
        this._moreBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OperationClick);
        this._moreBorder.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_100");
        Path path1 = new Path();
        path1.Width = 8.0;
        path1.Height = 8.0;
        path1.HorizontalAlignment = HorizontalAlignment.Center;
        path1.VerticalAlignment = VerticalAlignment.Center;
        path1.Stretch = Stretch.Uniform;
        Path path2 = path1;
        path2.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity100_80");
        path2.Data = TaskListItem.MoreIcon;
        this._moreBorder.Child = (UIElement) path2;
        this.Children.Add((UIElement) this._moreBorder);
        Canvas.SetTop((UIElement) this._moreBorder, 0.0);
      }
      this._moreBorder.Height = this.Height;
      Canvas.SetLeft((UIElement) this._moreBorder, this.ActualWidth - 16.0);
    }

    private void OnContentMouseEnter(object sender, MouseEventArgs e) => this.ShowBackground();

    private void OnContentMouseLeave(object sender, MouseEventArgs e) => this.ShowBackground();

    private void ShowBackground()
    {
      DisplayItemModel model = this.GetModel();
      if (model == null)
        return;
      string name = string.Empty;
      if (!TaskDragHelpModel.DragHelp.IsDragging)
      {
        if (model.InOperation || this._itemContainer.IsMouseOver || model.Dragging)
          name = "BaseColorOpacity3";
        if (model.Selected)
          name = "ItemSelectedColor";
      }
      else if (model.Dragging)
        name = "BaseColorOpacity3";
      if (!string.IsNullOrEmpty(name) && !this._isLocked)
      {
        if (this._backBorder == null)
        {
          this._backBorder = new Border();
          this._backBorder.SetBinding(FrameworkElement.HeightProperty, (BindingBase) new Binding()
          {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof (TaskListItem), 1),
            Path = new PropertyPath((object) FrameworkElement.ActualHeightProperty)
          });
        }
        if (!this.Children.Contains((UIElement) this._backBorder))
          this.Children.Add((UIElement) this._backBorder);
        this._backBorder.SetResourceReference(Panel.BackgroundProperty, (object) name);
        this._backBorder.CornerRadius = new CornerRadius(model.Dragging ? 4.0 : 0.0);
        this._backBorder.Width = Math.Max(0.0, this.ActualWidth - (double) this._startLeft - 36.0);
        Canvas.SetLeft((UIElement) this._backBorder, (double) (this._startLeft + 18));
        Canvas.SetTop((UIElement) this._backBorder, 0.0);
        this._backBorder.CornerRadius = name == "ItemSelectedColor" ? new CornerRadius(model.ShowTopMargin ? 6.0 : 0.0, model.ShowTopMargin ? 6.0 : 0.0, model.ShowBottomMargin ? 6.0 : 0.0, model.ShowBottomMargin ? 6.0 : 0.0) : new CornerRadius(6.0);
      }
      else if (this._backBorder != null)
        this.Children.Remove((UIElement) this._backBorder);
      this.ShowBottomLine();
    }

    private async void ShowColorRect()
    {
      TaskListItem taskListItem1 = this;
      await Task.Delay(50);
      DisplayItemModel model = taskListItem1.GetModel();
      if (taskListItem1.InMatrix || string.IsNullOrEmpty(model?.Color))
        return;
      if (taskListItem1._colorRect == null)
      {
        TaskListItem taskListItem2 = taskListItem1;
        Rectangle rectangle = new Rectangle();
        rectangle.Width = 3.0;
        taskListItem2._colorRect = rectangle;
        taskListItem1._colorRect.SetBinding(FrameworkElement.HeightProperty, (BindingBase) new Binding()
        {
          RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof (TaskListItem), 1),
          Path = new PropertyPath((object) FrameworkElement.ActualHeightProperty)
        });
        Canvas.SetLeft((UIElement) taskListItem1._colorRect, 18.0);
        taskListItem1._colorRect.SetBinding(Shape.FillProperty, "Color");
        taskListItem1._colorRect.SetValue(Panel.ZIndexProperty, (object) 100);
        taskListItem1.Children.Add((UIElement) taskListItem1._colorRect);
      }
      taskListItem1._colorRect.Opacity = !model.ShowProject ? 0.0 : (model.Status == 0 ? 1.0 : 0.36);
      taskListItem1._colorRect.Visibility = model.Dragging ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ShowBottomLine()
    {
      DisplayItemModel model = this.GetModel();
      if (model == null || !model.Dragging && !model.Selected && !this._itemContainer.IsMouseOver)
      {
        if (this._bottomLine != null)
          return;
        Line line = new Line();
        line.Width = Math.Max(0.0, this._itemContainer.Width - 40.0);
        line.StrokeThickness = 1.0;
        line.X1 = 0.0;
        line.X2 = 1.0;
        line.Stretch = Stretch.Fill;
        this._bottomLine = line;
        this._bottomLine.Margin = new Thickness(0.0, -0.5, 0.0, 0.0);
        this._bottomLine.SetBinding(Canvas.TopProperty, (BindingBase) new Binding()
        {
          RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof (TaskListItem), 1),
          Path = new PropertyPath((object) FrameworkElement.ActualHeightProperty)
        });
        this._bottomLine.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
        this._itemContainer.Children.Add((UIElement) this._bottomLine);
        Canvas.SetLeft((UIElement) this._bottomLine, 36.0);
      }
      else
      {
        if (this._bottomLine == null || !this._itemContainer.Children.Contains((UIElement) this._bottomLine))
          return;
        this._itemContainer.Children.Remove((UIElement) this._bottomLine);
        this._bottomLine = (Line) null;
      }
    }

    private async void ShowAvatar()
    {
      TaskListItem taskListItem1 = this;
      DisplayItemModel model = taskListItem1.GetModel();
      string avatarUrl = model?.AvatarUrl;
      if (!string.IsNullOrEmpty(avatarUrl))
      {
        if (taskListItem1._avatar == null)
        {
          TaskListItem taskListItem2 = taskListItem1;
          Rectangle rectangle1 = new Rectangle();
          rectangle1.Width = 18.0;
          rectangle1.Height = 18.0;
          rectangle1.RadiusX = 9.0;
          rectangle1.RadiusY = 9.0;
          taskListItem2._avatar = rectangle1;
          taskListItem1._avatar.MouseLeftButtonUp += new MouseButtonEventHandler(taskListItem1.OnAvatarMouseUp);
          taskListItem1._avatar.Cursor = Cursors.Hand;
          taskListItem1._itemContainer.Children.Add((UIElement) taskListItem1._avatar);
          taskListItem1._avatar.SetResourceReference(Canvas.TopProperty, (object) "Double11Add2");
        }
        Canvas.SetLeft((UIElement) taskListItem1._avatar, taskListItem1._itemContainer.ActualWidth - 32.0);
        taskListItem1._avatar.Visibility = Visibility.Visible;
        taskListItem1._avatar.Opacity = model.Status == 0 ? 1.0 : 0.4;
        Rectangle rectangle = taskListItem1._avatar;
        ImageBrush imageBrush1 = new ImageBrush();
        ImageBrush imageBrush2 = imageBrush1;
        imageBrush2.ImageSource = (ImageSource) await AvatarHelper.GetAvatarByUrlAsync(avatarUrl);
        rectangle.Fill = (Brush) imageBrush1;
        rectangle = (Rectangle) null;
        imageBrush2 = (ImageBrush) null;
        imageBrush1 = (ImageBrush) null;
        taskListItem1._avatar.ToolTip = (object) AvatarHelper.GetCacheUserName(model.Assignee, model.ProjectId);
        model = (DisplayItemModel) null;
      }
      else if (taskListItem1._avatar == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        taskListItem1._avatar.Visibility = Visibility.Collapsed;
        model = (DisplayItemModel) null;
      }
    }

    private void ShowCreateTimeText()
    {
      DisplayItemModel model = this.GetModel();
      if (this._showDetail && model != null && model.NoteDisplayDate.HasValue)
      {
        if (this._noteTimeText == null)
        {
          this._noteTimeText = new TextBlock()
          {
            FontSize = 12.0
          };
          this._noteTimeText.Margin = new Thickness(0.0, 2.0, 0.0, 0.0);
          this._noteTimeText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
          this._itemContainer.Children.Add((UIElement) this._noteTimeText);
          Canvas.SetLeft((UIElement) this._noteTimeText, 38.0);
        }
        this._noteTimeText.Text = DateUtils.FormatFullDate(model.NoteDisplayDate.Value);
      }
      else
      {
        if (this._noteTimeText == null)
          return;
        this._itemContainer.Children.Remove((UIElement) this._noteTimeText);
        this._noteTimeText = (TextBlock) null;
      }
    }

    private void InitControl()
    {
      this.Name = "ItemRoot";
      this.SetResourceReference(FrameworkElement.HeightProperty, (object) "Height40");
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.Background = (Brush) Brushes.Transparent;
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
      this.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
      this.MouseMove += new MouseEventHandler(this.OnItemMouseMove);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
      this.LoadContent();
      this.LoadIcons();
      if (!this.InPreview)
        return;
      this.SetLocked(true);
    }

    protected override Size MeasureOverride(Size constraint)
    {
      if (Math.Abs(this._originWidth - constraint.Width) > 1.0)
      {
        double num = Math.Min(10000.0, Math.Max(0.0, constraint.Width - (double) this._startLeft - 36.0));
        this._originWidth = constraint.Width;
        this._itemContainer.Width = num;
        if (this._backBorder != null)
          this._backBorder.Width = num;
        this.SetItemChildrenLeft();
      }
      return base.MeasureOverride(constraint);
    }

    private void SetItemChildrenLeft()
    {
      DisplayItemModel model = this.GetModel();
      int num = this._showDetail ? 4 : (this._originWidth > 500.0 ? 2 : (this._originWidth >= 350.0 ? 1 : 0));
      if (this._tagMaxCount != num)
      {
        this._tagMaxCount = num;
        model?.SetDisplayTags(this._tagMaxCount);
      }
      if (this._bottomLine != null)
        this._bottomLine.Width = Math.Max(0.0, this._itemContainer.Width - 40.0);
      this.SetItemsLeft();
    }

    private void SetItemsLeft()
    {
      if (this._avatar != null)
        Canvas.SetLeft((UIElement) this._avatar, this._itemContainer.Width - 32.0);
      double num1 = !string.IsNullOrEmpty(this.GetModel()?.AvatarUrl) ? 34.0 : 8.0;
      Canvas.SetLeft((UIElement) this.Icons, this._itemContainer.Width - this.Icons.ActualWidth - num1);
      double extraWidth = num1 + (this.Icons.ActualWidth > 0.0 ? this.Icons.ActualWidth + 4.0 : 0.0);
      if (this._projectLabel != null)
      {
        if (!this._showDetail)
        {
          Canvas.SetLeft((UIElement) this._projectLabel, this._itemContainer.Width - this._projectLabel.ActualWidth - extraWidth - 2.0);
          extraWidth += this._projectLabel.ActualWidth > 0.0 ? this._projectLabel.ActualWidth + 0.0 : 0.0;
        }
        else
        {
          ListItemProjectLabel projectLabel = this._projectLabel;
          ListItemTagsControl tagPanel = this._tagPanel;
          double val1 = (tagPanel != null ? tagPanel.ActualWidth : 0.0) + 40.0;
          double num2 = this._itemContainer.Width - this._projectLabel.ActualWidth;
          TaskItemPomoSummaryControl pomoSummary = this._pomoSummary;
          double num3 = pomoSummary != null ? pomoSummary.ActualWidth : 0.0;
          double val2 = num2 - num3 - 8.0;
          double length = Math.Min(val1, val2);
          Canvas.SetLeft((UIElement) projectLabel, length);
        }
      }
      if (this._showDetail && this._pomoSummary != null)
      {
        TaskItemPomoSummaryControl pomoSummary = this._pomoSummary;
        ListItemTagsControl tagPanel = this._tagPanel;
        double num4 = tagPanel != null ? tagPanel.ActualWidth : 0.0;
        ListItemProjectLabel projectLabel = this._projectLabel;
        double num5 = projectLabel != null ? projectLabel.ActualWidth : 0.0;
        double length = Math.Min(num4 + num5 + 40.0, this._itemContainer.Width - this._pomoSummary.ActualWidth - 8.0);
        Canvas.SetLeft((UIElement) pomoSummary, length);
      }
      if (this._tagPanel != null)
      {
        if (!this._showDetail)
        {
          Canvas.SetLeft((UIElement) this._tagPanel, this._itemContainer.Width - this._tagPanel.ActualWidth - extraWidth);
          extraWidth += this._tagPanel.ActualWidth > 0.0 ? this._tagPanel.ActualWidth + 2.0 : 0.0;
          this._tagPanel.MaxWidth = this._itemContainer.Width;
        }
        else
        {
          Canvas.SetLeft((UIElement) this._tagPanel, 36.0);
          ListItemTagsControl tagPanel = this._tagPanel;
          double width = this._itemContainer.Width;
          ListItemProjectLabel projectLabel = this._projectLabel;
          double num6 = projectLabel != null ? projectLabel.ActualWidth : 0.0;
          double num7 = width - num6;
          TaskItemPomoSummaryControl pomoSummary = this._pomoSummary;
          double num8 = pomoSummary != null ? pomoSummary.ActualWidth : 0.0;
          double num9 = Math.Max(0.0, num7 - num8 - 50.0);
          tagPanel.MaxWidth = num9;
        }
      }
      this.SetTitleWidth(extraWidth);
      if (this.ContentText == null)
        return;
      this.ContentText.Width = Math.Max(0.0, this._itemContainer.Width - 44.0);
    }

    private void SetLocked(bool locked)
    {
      if (this._isLocked == locked)
        return;
      this._isLocked = locked;
      foreach (UIElement child in this.Children)
      {
        if (!child.Equals((object) this.TitleContent))
          child.IsHitTestVisible = !locked;
      }
      this.TitleContent.TitleTextBox.IsHitTestVisible = !locked;
      this.TitleContent.CheckIcon.IsHitTestVisible = !locked;
    }

    public void SetPriority(int priority) => this._controller?.SetPriority(priority);

    public void SetDate(string key)
    {
      this.TitleContent.TryClearParseDate();
      this._controller?.SetDate(key);
    }

    public void ClearDate()
    {
      this.TitleContent.TitleTextBox.SetCanParseDate(false);
      this._controller?.ClearDate();
    }

    public void SelectDate(bool relative) => this._controller?.SelectDate();

    public async void ToggleTaskCompleted()
    {
      TaskListItem taskListItem = this;
      if (!(taskListItem.DataContext is DisplayItemModel model))
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        taskListItem.TitleContent.RemoveFocus();
        bool soundPlayed = await taskListItem.TryPlayCompleteStory(model);
        DisplayItemController controller = taskListItem._controller;
        if (controller == null)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          controller.ToggleItemCompleted(model, soundPlayed);
          model = (DisplayItemModel) null;
        }
      }
    }

    public void Delete()
    {
      this.TitleContent.RemoveFocus();
      this._controller?.DeleteSelectedTasks();
    }

    public TaskListView GetParentList() => this._controller?.GetTaskListView();

    public bool ParsingDate() => this.TitleContent.TitleTextBox.ParsingDate;

    public bool IsNewAdd()
    {
      return this.DataContext is DisplayItemModel dataContext && dataContext.IsNewAdd;
    }

    public async void PinOrUnpinTask()
    {
      TaskListItem taskListItem = this;
      if (!(taskListItem.DataContext is DisplayItemModel dataContext))
        return;
      // ISSUE: explicit non-virtual call
      string catId = __nonvirtual (taskListItem.GetParentList())?.GetIdentity()?.CatId;
      string id = dataContext.Id;
      string projectId = catId;
      bool inDetail = dataContext.InDetail;
      bool? isPin = new bool?();
      long? sortOrder = new long?();
      int num = inDetail ? 1 : 0;
      await TaskService.TogglesStarred(id, projectId, isPin, sortOrder, num != 0);
    }

    public void OpenSticky()
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      TaskStickyWindow.ShowTaskSticky(new List<string>()
      {
        dataContext.TaskId
      });
    }

    private async void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.OnDataChanged(e.OldValue, e.NewValue);
    }

    private async void OnDataChanged(object oldValue, object newValue)
    {
      TaskListItem element = this;
      element.SetModel(oldValue, newValue);
      element.RemoveCompleteStory();
      if (!(newValue is DisplayItemModel model))
        model = (DisplayItemModel) null;
      else if (string.IsNullOrEmpty(model.Id))
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        element.SetStartLeft(model);
        if (element._controller == null)
        {
          element._controller = new DisplayItemController((UIElement) element, model);
          element.Icons.SetController(element._controller);
        }
        element._showDetail = !element.InMatrix && (LocalSettings.Settings.ShowDetails || LocalSettings.Settings.InSearch) && !model.IsHabit;
        if (model.DropHover)
          element.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity5");
        else
          element.Background = (Brush) Brushes.Transparent;
        element._controller.Reset((UIElement) element, model);
        element._itemContainer.Visibility = model.Dragging ? Visibility.Collapsed : Visibility.Visible;
        element.Icons.IsHitTestVisible = model.Enable && !model.IsHabit && !model.IsCourse;
        if (element.InMatrix)
        {
          TaskListItem taskListItem = element;
          // ISSUE: explicit non-virtual call
          TaskListView parentList = __nonvirtual (element.GetParentList());
          int num = parentList != null ? (parentList.IsLocked ? 1 : 0) : 0;
          taskListItem.SetLocked(num != 0);
        }
        element.TitleContent.TitleTextBox.IsHitTestVisible = !element.InMatrix && !model.IsHabit && !model.IsCourse;
        element.ShowAvatar();
        element.InitContent(model);
        element.SetContentText();
        element.TryGetSearchComment();
        element.ShowCreateTimeText();
        element.ShowBackground();
        await Task.Delay(25);
        element.SetTagAndProject(model);
        element.ShowColorRect();
        element.SetShowDetailHeight();
        if (model.ShowReminder.HasValue)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          TaskItemLoadHelper.LoadShowReminder(model);
          model = (DisplayItemModel) null;
        }
      }
    }

    private void InitContent(DisplayItemModel model)
    {
      if (!this._showDetail || !string.IsNullOrEmpty(model.Content))
        return;
      if (model.IsTaskOrNote)
        DisplayItemModel.AssembleModelContent(model);
      else if (model.IsItem && model.SourceViewModel.OwnerTask != null)
      {
        model.Content = Utils.GetString("FromTask") + model.SourceViewModel.OwnerTask.Title;
      }
      else
      {
        if (!model.IsCourse)
          return;
        model.Content = model.SourceViewModel.Content;
      }
    }

    private async Task TryGetSearchComment()
    {
      DisplayItemModel model = this.GetModel();
      if (model == null)
        return;
      if (this._showDetail && LocalSettings.Settings.InSearch && !string.IsNullOrEmpty(SearchHelper.SearchKey))
      {
        string content = string.Empty;
        List<CommentModel> taskComments = TaskCommentCache.GetTaskComments(model.TaskId);
        // ISSUE: explicit non-virtual call
        if (taskComments != null && __nonvirtual (taskComments.Count) > 0)
        {
          foreach (CommentModel commentModel in (IEnumerable<CommentModel>) taskComments.OrderByDescending<CommentModel, DateTime?>((Func<CommentModel, DateTime?>) (c => c.createdTime)).ThenByDescending<CommentModel, DateTime?>((Func<CommentModel, DateTime?>) (c => c.modifiedTime)))
          {
            Match match = SearchHelper.SearchRegex.Match(commentModel.title?.ToLower() ?? string.Empty);
            if (commentModel.title != null && match.Success)
            {
              content = match.Index > 8 ? "..." + commentModel.title.Substring(match.Index - 8).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ") : commentModel.title;
              break;
            }
          }
        }
        if (!string.IsNullOrEmpty(content))
        {
          if (this._commentLabel == null)
          {
            this._commentLabel = new CommentSearchLabel();
            this._itemContainer.Children.Add((UIElement) this._commentLabel);
          }
          this.SetTaskContent(content, this._commentLabel.CommentText);
          this._commentLabel.Visibility = Visibility.Visible;
          Canvas.SetLeft((UIElement) this._commentLabel, 38.0);
          this.SetShowDetailHeight();
          model.SearchComment = true;
          return;
        }
      }
      model.SearchComment = false;
      if (this._commentLabel == null)
        return;
      this._itemContainer.Children.Remove((UIElement) this._commentLabel);
      this._commentLabel = (CommentSearchLabel) null;
    }

    private void SetShowDetailHeight()
    {
      DisplayItemModel model = this.GetModel();
      if (model == null)
        return;
      if (!this._showDetail)
      {
        this.SetResourceReference(FrameworkElement.HeightProperty, (object) "Height40");
        this.SetItemChildrenLeft();
      }
      else
      {
        int num1 = 40;
        int num2 = 4;
        if (!string.IsNullOrWhiteSpace(model.Content) && this.ContentText != null)
        {
          this.ContentText.SetResourceReference(Canvas.TopProperty, (object) "ItemContentTop");
          num1 += 20;
          num2 += 2;
        }
        int num3;
        if (this._noteTimeText != null)
        {
          TextBlock noteTimeText = this._noteTimeText;
          DependencyProperty topProperty = Canvas.TopProperty;
          string name;
          if (num2 != 4)
          {
            num3 = num1 - 8;
            name = "Double" + num3.ToString() + "Add" + num2.ToString();
          }
          else
          {
            num3 = num1 - 8;
            name = "Height" + num3.ToString();
          }
          noteTimeText.SetResourceReference(topProperty, (object) name);
          num1 += 20;
        }
        if (model.ShowTag && this._tagPanel != null || this._projectLabel != null || this._pomoSummary != null)
        {
          string str;
          if (num2 != 4)
          {
            num3 = num1 - 8;
            str = "Double" + num3.ToString() + "Add" + num2.ToString();
          }
          else
          {
            num3 = num1 - 8;
            str = "Height" + num3.ToString();
          }
          string name = str;
          this._tagPanel?.SetResourceReference(Canvas.TopProperty, (object) name);
          this._projectLabel?.SetResourceReference(Canvas.TopProperty, (object) name);
          this._pomoSummary?.SetResourceReference(Canvas.TopProperty, (object) name);
          num1 += 20;
          num2 += 2;
        }
        if (this._commentLabel != null)
        {
          string str;
          if (num2 != 4)
          {
            num3 = num1 - 8;
            str = "Double" + num3.ToString() + "Add" + num2.ToString();
          }
          else
          {
            num3 = num1 - 8;
            str = "Height" + num3.ToString();
          }
          string name = str;
          this._commentLabel.SetResourceReference(Canvas.TopProperty, (object) name);
          num1 += 20;
          num2 += 4;
        }
        this.SetResourceReference(FrameworkElement.HeightProperty, num2 == 4 ? (object) ("Height" + num1.ToString()) : (object) ("Double" + num1.ToString() + "Add" + num2.ToString()));
        this.SetItemChildrenLeft();
      }
    }

    private void RemoveBinding()
    {
      DisplayItemModel model = this.GetModel();
      if (model != null)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) model, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), string.Empty);
      if (this._tagPanel != null)
      {
        this._itemContainer.Children.Remove((UIElement) this._tagPanel);
        this._tagPanel.TagDelete -= new EventHandler<string>(this.OnTagDelete);
        this._tagPanel = (ListItemTagsControl) null;
      }
      if (this._projectLabel != null)
      {
        this._itemContainer.Children.Remove((UIElement) this._projectLabel);
        this._projectLabel = (ListItemProjectLabel) null;
      }
      if (this._pomoSummary != null)
      {
        this._itemContainer.Children.Remove((UIElement) this._pomoSummary);
        this._pomoSummary = (TaskItemPomoSummaryControl) null;
      }
      this.Icons.Children.Clear();
      this._controller?.Clear();
      this._controller = (DisplayItemController) null;
      this._unloaded = true;
    }

    private void ReloadData()
    {
      if (!this._unloaded)
        return;
      DisplayItemModel model = this.GetModel();
      this.OnDataChanged((object) model, (object) model);
      this.Icons.ResetIcons();
    }

    private void SetModel(object oldVal, object newVal)
    {
      DisplayItemModel oldModel = oldVal as DisplayItemModel;
      DisplayItemModel newModel = newVal as DisplayItemModel;
      Task.Run((Action) (() =>
      {
        newModel?.SetPropertyChangedEvent();
        if (oldModel != null)
          PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldModel, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), string.Empty);
        if (newModel == null)
          return;
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newModel, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), string.Empty);
      }));
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (!object.Equals(sender, this.DataContext))
        {
          PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) (sender as DisplayItemModel), new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), string.Empty);
        }
        else
        {
          DisplayItemModel model = this.GetModel();
          if (model == null)
            return;
          string propertyName = e.PropertyName;
          if (propertyName == null)
            return;
          switch (propertyName.Length)
          {
            case 4:
              switch (propertyName[1])
              {
                case 'a':
                  if (!(propertyName == "Tags"))
                    return;
                  model.SetDisplayTags(this._tagMaxCount);
                  this.SetTagPanel();
                  return;
                case 'y':
                  if (!(propertyName == "Type"))
                    return;
                  goto label_77;
                default:
                  return;
              }
            case 5:
              switch (propertyName[0])
              {
                case 'L':
                  if (!(propertyName == "Level"))
                    return;
                  this.SetStartLeft(model);
                  return;
                case 'T':
                  if (!(propertyName == "Title"))
                    return;
                  this.OnTaskTitleChanged();
                  return;
                default:
                  return;
              }
            case 6:
              switch (propertyName[0])
              {
                case 'E':
                  if (!(propertyName == "Enable"))
                    return;
                  goto label_77;
                case 'S':
                  if (!(propertyName == "Status"))
                    return;
                  double num = model.Status == 0 ? 1.0 : 0.4;
                  if (this._tagPanel != null)
                    this._tagPanel.Opacity = num;
                  if (this._projectLabel != null)
                    this._projectLabel.Opacity = num;
                  if (this._avatar == null)
                    return;
                  this._avatar.Opacity = num;
                  return;
                default:
                  return;
              }
            case 7:
              if (!(propertyName == "Content"))
                return;
              this.SetContentText();
              this.SetShowDetailHeight();
              return;
            case 8:
              switch (propertyName[1])
              {
                case 'e':
                  if (!(propertyName == "Selected"))
                    return;
                  goto label_69;
                case 'h':
                  if (!(propertyName == "ShowPomo"))
                    return;
                  this.SetPomoSummary();
                  this.SetShowDetailHeight();
                  return;
                case 'r':
                  if (!(propertyName == "Dragging"))
                    return;
                  this.SetStartLeft(model);
                  this._itemContainer.Visibility = model.Dragging ? Visibility.Collapsed : Visibility.Visible;
                  this.ShowOrHideMoreIcon(this._itemContainer.IsMouseOver);
                  this.ShowBackground();
                  return;
                default:
                  return;
              }
            case 9:
              switch (propertyName[0])
              {
                case 'A':
                  if (!(propertyName == "AvatarUrl"))
                    return;
                  this.ShowAvatar();
                  this.SetItemsLeft();
                  return;
                case 'D':
                  if (!(propertyName == "DropHover"))
                    return;
                  if (model.DropHover)
                  {
                    this.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity5");
                    return;
                  }
                  this.Background = (Brush) Brushes.Transparent;
                  return;
                case 'S':
                  if (!(propertyName == "ShowIcons"))
                    return;
                  this.Icons.ResetIcons();
                  return;
                default:
                  return;
              }
            case 10:
              return;
            case 11:
              switch (propertyName[0])
              {
                case 'D':
                  if (!(propertyName == "DisplayTags"))
                    return;
                  this.SetShowDetailHeight();
                  return;
                case 'I':
                  if (!(propertyName == "InOperation"))
                    return;
                  goto label_69;
                case 'S':
                  if (!(propertyName == "ShowComment"))
                    return;
                  this.TryGetSearchComment();
                  this.SetShowDetailHeight();
                  return;
                default:
                  return;
              }
            case 12:
              return;
            case 13:
              if (!(propertyName == "ShowTopMargin"))
                return;
              break;
            case 14:
              return;
            case 15:
              return;
            case 16:
              if (!(propertyName == "ShowBottomMargin"))
                return;
              break;
            default:
              return;
          }
          if (this._backBorder == null || !this._backBorder.IsVisible)
            return;
          this._backBorder.CornerRadius = new CornerRadius(model.ShowTopMargin ? 6.0 : 0.0, model.ShowTopMargin ? 6.0 : 0.0, model.ShowBottomMargin ? 6.0 : 0.0, model.ShowBottomMargin ? 6.0 : 0.0);
          return;
label_69:
          this.ShowBackground();
          return;
label_77:
          this.Icons.IsHitTestVisible = model.Enable && !model.IsHabit;
        }
      }));
    }

    private void SetStartLeft(DisplayItemModel model)
    {
      this._startLeft = model.Level * 20 + (model.Dragging ? 10 : 0);
      Canvas.SetLeft((UIElement) this._itemContainer, (double) (this._startLeft + 18));
      this._itemContainer.Width = Math.Max(0.0, this.ActualWidth - (double) this._startLeft - 36.0);
      if (this._backBorder != null)
      {
        this._backBorder.Width = Math.Max(0.0, this.ActualWidth - (double) this._startLeft - 36.0);
        Canvas.SetLeft((UIElement) this._backBorder, (double) (this._startLeft + 18));
      }
      this.SetItemsLeft();
      this.ShowOrHideDragBar(this._itemContainer.IsMouseOver);
    }

    private void OnTaskTitleChanged()
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || this.TitleContent.TitleTextBox.KeyboardFocused || !(dataContext.Title != this.TitleContent.TitleTextBox.Text))
        return;
      this.TitleContent.SetHintText(string.IsNullOrEmpty(dataContext.Title));
      this.TitleContent.TitleTextBox.SetTextOffset(dataContext.Title, true, true);
    }

    private void SetTagAndProject(DisplayItemModel model)
    {
      model.SetDisplayTags(this._tagMaxCount);
      this.SetProjectLabel();
      this.SetTagPanel();
      this.SetPomoSummary();
    }

    private void SetProjectLabel()
    {
      DisplayItemModel model = this.GetModel();
      if (model != null && model.ShowProject)
      {
        if (this._projectLabel == null)
        {
          this._projectLabel = new ListItemProjectLabel();
          this._projectLabel.SizeChanged += (SizeChangedEventHandler) ((o, e) => this.SetItemsLeft());
          this._itemContainer.Children.Insert(1, (UIElement) this._projectLabel);
        }
        this._projectLabel.Opacity = model.Status == 0 ? 1.0 : 0.4;
        this._projectLabel.SetResourceReference(Canvas.TopProperty, !this._showDetail ? (object) "Double8_12_13" : (object) "Height38");
        this._projectLabel.IsHitTestVisible = !model.IsCourse;
      }
      else
      {
        if (this._projectLabel == null)
          return;
        this._itemContainer.Children.Remove((UIElement) this._projectLabel);
        this._projectLabel = (ListItemProjectLabel) null;
      }
    }

    private void SetPomoSummary()
    {
      DisplayItemModel model = this.GetModel();
      if (this._showDetail && model != null && model.ShowPomo)
      {
        if (this._pomoSummary == null)
        {
          TaskItemPomoSummaryControl pomoSummaryControl = new TaskItemPomoSummaryControl();
          pomoSummaryControl.Margin = new Thickness(6.0, 4.0, 0.0, 0.0);
          this._pomoSummary = pomoSummaryControl;
          this._itemContainer.Children.Add((UIElement) this._pomoSummary);
        }
        this._pomoSummary.SetData(model.PomoSummary);
      }
      else
      {
        if (this._pomoSummary == null)
          return;
        this._itemContainer.Children.Remove((UIElement) this._pomoSummary);
        this._pomoSummary = (TaskItemPomoSummaryControl) null;
      }
    }

    private void SetTagPanel()
    {
      DisplayItemModel model = this.GetModel();
      if (model != null && model.ShowTag)
      {
        if (this._tagPanel == null)
        {
          this._tagPanel = new ListItemTagsControl();
          this._tagPanel.TagDelete += new EventHandler<string>(this.OnTagDelete);
          this._tagPanel.SizeChanged += (SizeChangedEventHandler) ((o, e) => this.SetItemsLeft());
          this._itemContainer.Children.Add((UIElement) this._tagPanel);
        }
        this._tagPanel.Opacity = model.Status == 0 ? 1.0 : 0.4;
        this._tagPanel.SetResourceReference(Canvas.TopProperty, !this._showDetail ? (object) "Double9_12_13" : (object) "Height38");
      }
      else if (this._tagPanel != null)
      {
        this._itemContainer.Children.Remove((UIElement) this._tagPanel);
        this._tagPanel.TagDelete -= new EventHandler<string>(this.OnTagDelete);
        this._tagPanel = (ListItemTagsControl) null;
      }
      this.SetShowDetailHeight();
    }

    private async void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      TaskListItem taskListItem = this;
      if (!(taskListItem.DataContext is DisplayItemModel model))
        model = (DisplayItemModel) null;
      else if (!model.HitVisible)
        model = (DisplayItemModel) null;
      else if (taskListItem.TitleContent.IsIconMouseOver)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        Border moreBorder = taskListItem._moreBorder;
        // ISSUE: explicit non-virtual call
        if ((moreBorder != null ? (__nonvirtual (moreBorder.IsMouseOver) ? 1 : 0) : 0) != 0)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          int num;
          if (!taskListItem.Icons.IsMouseOver)
          {
            ListItemProjectLabel projectLabel = taskListItem._projectLabel;
            // ISSUE: explicit non-virtual call
            if ((projectLabel != null ? (__nonvirtual (projectLabel.IsMouseOver) ? 1 : 0) : 0) == 0)
            {
              ListItemTagsControl tagPanel = taskListItem._tagPanel;
              // ISSUE: explicit non-virtual call
              if ((tagPanel != null ? (__nonvirtual (tagPanel.IsMouseOver) ? 1 : 0) : 0) == 0)
              {
                Rectangle avatar = taskListItem._avatar;
                num = (avatar != null ? (avatar.IsMouseOver ? 1 : 0) : 0) == 0 ? 1 : 0;
                goto label_15;
              }
            }
          }
          num = 0;
label_15:
          if (num == 0)
            model = (DisplayItemModel) null;
          else if (!taskListItem._mouseDown && !model.Selected && !taskListItem.InMatrix)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            taskListItem._mouseDown = false;
            bool ctrlOrShiftDown = Utils.IfCtrlPressed() || Utils.IfShiftPressed();
            if (taskListItem.InList | ctrlOrShiftDown)
            {
              taskListItem.SelectItem(model, ctrlOrShiftDown);
              if (ctrlOrShiftDown)
              {
                model = (DisplayItemModel) null;
                return;
              }
            }
            double totalMilliseconds = (DateTime.Now - taskListItem._lastClickTime).TotalMilliseconds;
            if (totalMilliseconds < 2000.0)
              UtilLog.Info("ListItem.OnItemClick : " + totalMilliseconds.ToString());
            bool firstClick = totalMilliseconds > 300.0;
            taskListItem._lastClickTime = DateTime.Now;
            if (firstClick)
            {
              taskListItem._isWaitingDoubleClick = true;
              model.Selected = true;
              if (taskListItem.InMatrix)
              {
                TaskDetailPopup window = await taskListItem.ShowItemDetailWindow(model);
                if (window != null)
                  window.Opacity = 0.0;
                await Task.Delay(150);
                if (!taskListItem._isWaitingDoubleClick)
                {
                  TaskDetailPopup taskDetailPopup = window;
                  if (taskDetailPopup == null)
                  {
                    model = (DisplayItemModel) null;
                    return;
                  }
                  taskDetailPopup.Clear();
                  model = (DisplayItemModel) null;
                  return;
                }
                if (window != null)
                  window.Opacity = 1.0;
                window = (TaskDetailPopup) null;
              }
            }
            taskListItem._isWaitingDoubleClick = false;
            if (firstClick)
              model = (DisplayItemModel) null;
            else if (taskListItem.TitleContent.TitleTextBox.SelectionLength != 0)
            {
              model = (DisplayItemModel) null;
            }
            else
            {
              model.Selected = !taskListItem.InMatrix;
              TaskDetailWindows.ShowTaskWindows(model.TaskId);
              model = (DisplayItemModel) null;
            }
          }
        }
      }
    }

    private async Task<TaskDetailPopup> ShowItemDetailWindow(DisplayItemModel model)
    {
      TaskDetailPopup taskDetailPopup;
      if (this._controller != null)
        taskDetailPopup = await this._controller.ShowDetailWindow(model, false);
      else
        taskDetailPopup = (TaskDetailPopup) null;
      return taskDetailPopup;
    }

    private async void SelectItem(DisplayItemModel model, bool ctrlOrShiftDown = false)
    {
      this._controller?.SelectItem();
      if (model.InMatrix || ctrlOrShiftDown || this.TitleContent.TitleTextBox.CurrentFocused)
        return;
      await Task.Delay(5);
      this.TitleContent.TitleTextBox.FocusEnd();
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      this._controller?.ShowOperationDialogSafely();
      e.Handled = true;
    }

    private void OnTagDelete(object sender, string tag)
    {
      DisplayItemModel model = this.GetModel();
      if (model == null || !model.Enable)
        return;
      this._controller?.OnTagDelete(tag.ToLower());
    }

    private void OperationClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      this._controller?.ShowOperationDialogSafely();
    }

    private void OnAvatarMouseUp(object sender, MouseEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      this._controller?.ShowSelectAssignPopup();
    }

    private void NavigateTaskClick(object sender, RoutedEventArgs e)
    {
      this._controller?.NavigateTaskClick();
    }

    private void OnDragMouseDown(object sender, MouseButtonEventArgs e) => this._startDrag = true;

    private void OnItemMouseMove(object sender, MouseEventArgs e)
    {
      if (this.TitleContent.TitleTextBox.SelectionPopupOpened)
        return;
      if (this.DataContext is DisplayItemModel dataContext && (!dataContext.ShowDragBar || !dataContext.Enable || dataContext.IsHabit || dataContext.IsSection))
      {
        this._startDrag = false;
      }
      else
      {
        if (!this.TitleContent.TitleTextBox.CurrentFocused && this._startDrag && e.LeftButton == MouseButtonState.Pressed)
        {
          System.Windows.Point position = e.GetPosition((IInputElement) this);
          if (Math.Abs(position.X - this._startPoint.X) <= 5.0 && Math.Abs(position.Y - this._startPoint.Y) <= 2.0)
            return;
          this._controller?.OnDragMouseDown(sender, e);
        }
        this._startDrag = false;
      }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (this._startDrag && e.LeftButton == MouseButtonState.Pressed)
        this._controller?.OnDragMouseDown(sender, e);
      this._startDrag = false;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this._dragBorder != null && this._dragBorder.IsMouseOver || this.TitleContent.IsIconMouseOver)
        return;
      Rectangle avatar = this._avatar;
      if ((avatar != null ? (avatar.IsMouseOver ? 1 : 0) : 0) != 0 || this._moreBorder != null && this._moreBorder.IsMouseOver)
        return;
      if (!Utils.IfCtrlPressed() && !Utils.IfShiftPressed())
      {
        this._startPoint = e.GetPosition((IInputElement) this);
        this._startDrag = true;
      }
      this._mouseDown = true;
    }

    private void OnDragMouseUp(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void SetContentText()
    {
      if (!this._showDetail)
      {
        if (this.ContentText == null)
          return;
        this._itemContainer.Children.Remove((UIElement) this.ContentText);
        this.ContentText = (TextBlock) null;
      }
      else if (this.InMatrix || !LocalSettings.Settings.ShowDetails || !(this.DataContext is DisplayItemModel dataContext) || dataContext.IsHabit || string.IsNullOrWhiteSpace(dataContext.Content))
      {
        if (this.ContentText == null)
          return;
        this.ContentText.Inlines.Clear();
        this.ContentText.Visibility = Visibility.Collapsed;
      }
      else
      {
        if (this.ContentText == null)
        {
          this.ContentText = new TextBlock();
          this.ContentText.SetResourceReference(TextBlock.FontSizeProperty, (object) "Font12");
          this.ContentText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100_80");
          this.ContentText.Opacity = 0.4;
          this.ContentText.TextWrapping = TextWrapping.NoWrap;
          this.ContentText.TextTrimming = TextTrimming.CharacterEllipsis;
          this._itemContainer.Children.Add((UIElement) this.ContentText);
          Canvas.SetLeft((UIElement) this.ContentText, 38.0);
        }
        this.ContentText.Visibility = Visibility.Visible;
        this.SetTaskContent(dataContext.Content, this.ContentText);
      }
    }

    private void SetTaskContent(string content, TextBlock block)
    {
      block.Inlines.Clear();
      string str = content;
      string text1 = (str.Length > 400 ? str.Substring(0, 399) : str).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("    ", " ");
      if (LocalSettings.Settings.InSearch && !string.IsNullOrEmpty(SearchHelper.SearchKey) && !string.IsNullOrEmpty(content) && !this.InPreview)
      {
        MatchCollection matchCollection = SearchHelper.SearchRegex.Matches(text1.ToLower());
        if (matchCollection.Count > 0)
        {
          for (int i = 0; i < matchCollection.Count; ++i)
          {
            int startIndex = i > 0 ? matchCollection[i - 1].Index + matchCollection[i - 1].Length : 0;
            string text2 = text1.Substring(startIndex, matchCollection[i].Index - startIndex);
            block.Inlines.Add(text2);
            InlineCollection inlines = block.Inlines;
            Run run = new Run(text1.Substring(matchCollection[i].Index, matchCollection[i].Length));
            run.Background = SearchHelper.GetSearchHighlightColor();
            inlines.Add((Inline) run);
          }
          int startIndex1 = matchCollection[matchCollection.Count - 1].Index + matchCollection[matchCollection.Count - 1].Length;
          string text3 = text1.Substring(startIndex1, text1.Length - startIndex1);
          block.Inlines.Add(text3);
        }
        else
          block.Inlines.Add(text1);
      }
      else
      {
        string text4 = TaskUtils.ReplaceAttachmentTextInString(text1);
        block.Inlines.Add(text4);
      }
      block.Visibility = Visibility.Visible;
    }

    public async void ShowDetailWindow(bool focus)
    {
      TaskListItem taskListItem = this;
      if (!(taskListItem.DataContext is DisplayItemModel dataContext) || taskListItem._controller == null)
        return;
      TaskDetailPopup taskDetailPopup = await taskListItem._controller.ShowDetailWindow(dataContext, focus);
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      this.ShowOrHideDragBar(true);
      this.ShowOrHideMoreIcon(true);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      this.ShowOrHideDragBar(false);
      this.ShowOrHideMoreIcon(false);
    }

    public System.Windows.Point GetClickPoint() => this._firstClickPoint;

    public async Task<bool> TryPlayCompleteStory(DisplayItemModel model)
    {
      if (this.InMatrix || !model.IsTask && !model.IsItem)
        return false;
      bool flag = model.Status == 0 && this._controller != null;
      string completeStoryText;
      if (flag)
      {
        completeStoryText = await this._controller.GetCompleteStoryText(model);
        flag = completeStoryText != null;
      }
      if (!flag)
        return false;
      await this.PlayCompleteStory(completeStoryText);
      return true;
    }

    private async Task PlayCompleteStory(string text)
    {
      TaskListItem taskListItem1 = this;
      bool flag = true;
      if (text == Utils.GetString("FirstTaskDone"))
      {
        flag = LocalSettings.Settings.ExtraSettings.CpltStoryTimes < 2;
        ++LocalSettings.Settings.ExtraSettings.CpltStoryTimes;
      }
      double length = ((double) taskListItem1.FindResource((object) "Height40") - 40.0) / 2.0;
      taskListItem1._completeStoryView = new CompleteStory();
      taskListItem1.Children.Add((UIElement) taskListItem1._completeStoryView);
      Canvas.SetLeft((UIElement) taskListItem1._completeStoryView, (double) (taskListItem1._startLeft + 19));
      Canvas.SetTop((UIElement) taskListItem1._completeStoryView, length);
      DoubleAnimation doubleAnimation1 = new DoubleAnimation();
      doubleAnimation1.Duration = new Duration(TimeSpan.FromMilliseconds(300.0));
      doubleAnimation1.From = new double?(-40.0);
      doubleAnimation1.To = new double?(0.0);
      DoubleAnimation animation1 = doubleAnimation1;
      taskListItem1._csTextBlock = new TextBlock()
      {
        Text = text,
        FontSize = 14.0
      };
      taskListItem1._csTextBlock.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      TranslateTransform translateTransform1 = new TranslateTransform()
      {
        Y = -40.0
      };
      taskListItem1._csTextBlock.RenderTransform = (Transform) translateTransform1;
      taskListItem1.Children.Add((UIElement) taskListItem1._csTextBlock);
      Canvas.SetLeft((UIElement) taskListItem1._csTextBlock, (double) (taskListItem1._startLeft + 56));
      Canvas.SetTop((UIElement) taskListItem1._csTextBlock, length + 11.0);
      DoubleAnimation doubleAnimation2 = new DoubleAnimation();
      doubleAnimation2.Duration = new Duration(TimeSpan.FromMilliseconds(300.0));
      doubleAnimation2.From = new double?(0.0);
      doubleAnimation2.To = new double?(Math.Max(0.0, taskListItem1.ActualWidth - (double) taskListItem1._startLeft - 24.0));
      DoubleAnimation animation2 = doubleAnimation2;
      TaskListItem taskListItem2 = taskListItem1;
      Border border = new Border();
      border.Width = 0.0;
      border.Height = taskListItem1.ActualHeight;
      border.Opacity = 0.2;
      taskListItem2._csBackBorder = border;
      taskListItem1._csBackBorder.SetResourceReference(Panel.BackgroundProperty, (object) "TickYellow");
      Canvas.SetLeft((UIElement) taskListItem1._csBackBorder, (double) (taskListItem1._startLeft + 12));
      taskListItem1.Children.Add((UIElement) taskListItem1._csBackBorder);
      TranslateTransform translateTransform2 = new TranslateTransform();
      taskListItem1._itemContainer.RenderTransform = (Transform) translateTransform2;
      DoubleAnimation doubleAnimation3 = new DoubleAnimation();
      doubleAnimation3.Duration = new Duration(TimeSpan.FromMilliseconds(300.0));
      doubleAnimation3.From = new double?(0.0);
      doubleAnimation3.To = new double?(taskListItem1._itemContainer.ActualHeight);
      DoubleAnimation animation3 = doubleAnimation3;
      Utils.PlayCompletionSound();
      translateTransform2.BeginAnimation(TranslateTransform.YProperty, (AnimationTimeline) animation3);
      if (flag)
        translateTransform1.BeginAnimation(TranslateTransform.YProperty, (AnimationTimeline) animation1);
      taskListItem1._csBackBorder.BeginAnimation(FrameworkElement.WidthProperty, (AnimationTimeline) animation2);
      taskListItem1._completeStoryView.PlayStory();
      await Task.Delay(flag ? 900 : 500);
      taskListItem1.RemoveCompleteStory();
    }

    private async Task RemoveCompleteStory()
    {
      TaskListItem taskListItem = this;
      if (taskListItem._csTextBlock == null)
        return;
      taskListItem.Children.Remove((UIElement) taskListItem._completeStoryView);
      taskListItem.Children.Remove((UIElement) taskListItem._csTextBlock);
      taskListItem.Children.Remove((UIElement) taskListItem._csBackBorder);
      taskListItem._itemContainer.RenderTransform = (Transform) null;
      taskListItem._completeStoryView = (CompleteStory) null;
      taskListItem._csTextBlock = (TextBlock) null;
      taskListItem._csBackBorder = (Border) null;
    }
  }
}
