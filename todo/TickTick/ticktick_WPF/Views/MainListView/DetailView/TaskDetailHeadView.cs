// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.DetailView.TaskDetailHeadView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.MainListView.DetailView
{
  public class TaskDetailHeadView : Grid
  {
    private EscPopup _popup;
    private readonly Border _backBt;
    private readonly TaskCheckIcon _checkIcon;
    private readonly Grid _dateGrid;
    private TextBlock _dateText;
    private StackPanel _remindTimePanel;
    private StackPanel _rightIcons;
    private AssignIcon _assignIcon;
    private TaskProgressControl _progress;
    private bool _showPopupMouseDown;

    public TaskDetailHeadView()
    {
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.HorizontalOffset = 45.0;
      escPopup.VerticalOffset = 25.0;
      escPopup.Placement = PlacementMode.Left;
      this._popup = escPopup;
      Border border = new Border();
      border.Margin = new Thickness(0.0, 0.0, 8.0, 0.0);
      border.Cursor = Cursors.Hand;
      border.Visibility = Visibility.Collapsed;
      border.HorizontalAlignment = HorizontalAlignment.Left;
      border.Background = (Brush) Brushes.Transparent;
      border.ToolTip = (object) Utils.GetString("Close");
      this._backBt = border;
      TaskCheckIcon taskCheckIcon = new TaskCheckIcon();
      taskCheckIcon.Margin = new Thickness(3.0, 0.0, 12.0, 0.0);
      taskCheckIcon.VerticalAlignment = VerticalAlignment.Center;
      taskCheckIcon.HorizontalAlignment = HorizontalAlignment.Left;
      taskCheckIcon.Cursor = Cursors.Hand;
      this._checkIcon = taskCheckIcon;
      Grid grid = new Grid();
      grid.Height = 40.0;
      grid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
      this._dateGrid = grid;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.ColumnDefinitions.Add(new ColumnDefinition());
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition());
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(8.0)
      });
      this.InitUi();
    }

    private void InitUi()
    {
      this._backBt.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBackClick);
      this.Children.Add((UIElement) this._backBt);
      this._checkIcon.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
      this._checkIcon.PreviewMouseRightButtonUp += new MouseButtonEventHandler(this.CheckBoxRightMouseUp);
      this._checkIcon.SetValue(Grid.ColumnProperty, (object) 1);
      this._checkIcon.SetBinding(UIElement.VisibilityProperty, "ShowCheckIcon");
      this._checkIcon.SetSize(16);
      this.Children.Add((UIElement) this._checkIcon);
      Border border = new Border();
      border.Height = 14.0;
      border.Width = 4.0;
      border.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
      border.Margin = new Thickness(28.0, 0.0, 8.0, 0.0);
      Border element1 = border;
      element1.SetValue(Grid.ColumnProperty, (object) 1);
      element1.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity20");
      element1.SetBinding(UIElement.VisibilityProperty, "ShowCheckIcon");
      this.Children.Add((UIElement) element1);
      this.InitDateGrid();
      this.InitRightIcons();
      StackPanel stackPanel = new StackPanel();
      stackPanel.Margin = new Thickness(28.0, -8.0, 5.0, 4.0);
      stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
      this._remindTimePanel = stackPanel;
      this._remindTimePanel.SetValue(Grid.ColumnProperty, (object) 2);
      this._remindTimePanel.SetValue(Grid.RowProperty, (object) 1);
      this.Children.Add((UIElement) this._remindTimePanel);
      this.InitRemindText();
      TaskProgressControl taskProgressControl = new TaskProgressControl();
      taskProgressControl.VerticalAlignment = VerticalAlignment.Bottom;
      taskProgressControl.Margin = new Thickness(-19.0, 0.0, -19.0, 4.0);
      this._progress = taskProgressControl;
      this._progress.ProgressChanged += new TaskProgressDelegate(this.OnProgressChanged);
      this._progress.SetValue(Panel.ZIndexProperty, (object) 1000);
      this._progress.SetValue(Grid.ColumnSpanProperty, (object) 4);
      this._progress.SetValue(Grid.RowSpanProperty, (object) 3);
      this._progress.SetBinding(UIElement.IsEnabledProperty, "Enable");
      this._progress.SetBinding(UIElement.VisibilityProperty, "ProgressVisibility");
      this.Children.Add((UIElement) this._progress);
      Line line = new Line();
      line.X1 = 0.0;
      line.X2 = 1.0;
      line.Stretch = Stretch.Fill;
      line.VerticalAlignment = VerticalAlignment.Bottom;
      line.Margin = new Thickness(-20.0, 0.0, -20.0, 4.0);
      line.StrokeThickness = 1.0;
      Line element2 = line;
      element2.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
      element2.SetValue(Grid.ColumnSpanProperty, (object) 4);
      element2.SetValue(Grid.RowProperty, (object) 2);
      this.Children.Add((UIElement) element2);
      this._popup.Opened += new EventHandler(this.PopupOpened);
      this._popup.Closed += new EventHandler(this.PopupClosed);
    }

    private void InitRemindText()
    {
      TextBlock textBlock1 = new TextBlock();
      textBlock1.FontSize = 11.0;
      textBlock1.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock1.TextTrimming = TextTrimming.CharacterEllipsis;
      textBlock1.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      textBlock1.Background = (Brush) Brushes.Transparent;
      TextBlock element1 = textBlock1;
      element1.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
      element1.SetBinding(TextBlock.TextProperty, "RemindTimeText");
      element1.SetBinding(UIElement.VisibilityProperty, "ShowSnoozeText");
      this._remindTimePanel.Children.Add((UIElement) element1);
      TextBlock textBlock2 = new TextBlock();
      textBlock2.FontSize = 11.0;
      textBlock2.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock2.TextTrimming = TextTrimming.CharacterEllipsis;
      textBlock2.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      textBlock2.Background = (Brush) Brushes.Transparent;
      TextBlock element2 = textBlock2;
      element2.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
      element2.SetBinding(TextBlock.TextProperty, "TimeZoneText");
      element2.SetBinding(UIElement.VisibilityProperty, "ShowTimeZoneText");
      this._remindTimePanel.Children.Add((UIElement) element2);
    }

    public void SetIconColor(int priority, int status)
    {
      this._checkIcon.SetIconColor(DisplayType.Task, priority, status);
    }

    private void InitRightIcons()
    {
      StackPanel stackPanel = new StackPanel()
      {
        Orientation = Orientation.Horizontal
      };
      stackPanel.SetBinding(UIElement.IsEnabledProperty, "Enable");
      stackPanel.SetValue(Grid.ColumnProperty, (object) 3);
      this._rightIcons = stackPanel;
      this.Children.Add((UIElement) this._rightIcons);
    }

    public void SetRightIcons(bool newAdd, bool isNote)
    {
      this._rightIcons.Children.Clear();
      if (isNote)
      {
        if (newAdd)
          return;
        Border border = new Border();
        border.Width = 24.0;
        border.Cursor = Cursors.Hand;
        Border element = border;
        element.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_60");
        element.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnShowPopupMouseDown);
        element.PreviewMouseLeftButtonUp += (MouseButtonEventHandler) ((o, e) => this.ShowNoteMessage(o));
        Path path = UiUtils.CreatePath("IcNoteMessage", "BaseColorOpacity100", "");
        path.Width = 18.0;
        path.VerticalAlignment = VerticalAlignment.Center;
        path.Stretch = Stretch.Uniform;
        element.Child = (UIElement) path;
        this._rightIcons.Children.Insert(0, (UIElement) element);
      }
      else
      {
        HoverIconButton hoverIconButton = new HoverIconButton();
        hoverIconButton.ImageOpacity = 1.0;
        hoverIconButton.Margin = new Thickness(2.0, 0.0, -1.0, 0.0);
        HoverIconButton element = hoverIconButton;
        element.SetBinding(HoverIconButton.ImageSourceProperty, "PriorityImage");
        element.SetResourceReference(FrameworkElement.ToolTipProperty, (object) "priority");
        element.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnShowPopupMouseDown);
        element.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnPriorityClick);
        this._rightIcons.Children.Insert(0, (UIElement) element);
      }
    }

    private void InitDateGrid()
    {
      this._dateGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this._dateGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this._dateGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this._dateGrid.SetValue(Grid.ColumnProperty, (object) 2);
      this.Children.Add((UIElement) this._dateGrid);
      this._dateGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectDateClick);
      this._dateGrid.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnSelectTimeMouseDown);
      this._dateGrid.SetBinding(UIElement.IsEnabledProperty, "Enable");
      Border border = new Border();
      border.Margin = new Thickness(0.0, 7.0, 0.0, 7.0);
      border.Cursor = Cursors.Hand;
      Border element1 = border;
      element1.SetValue(Grid.ColumnSpanProperty, (object) 3);
      element1.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle");
      this._dateGrid.Children.Add((UIElement) element1);
      Path path = new Path();
      path.Width = 18.0;
      path.Height = 18.0;
      path.Stretch = Stretch.Uniform;
      path.IsHitTestVisible = false;
      path.Margin = new Thickness(4.0, 0.0, 0.0, 0.0);
      path.Data = Utils.GetIcon("IcCalendarThin");
      Path element2 = path;
      element2.SetBinding(Shape.FillProperty, "DateIconColor");
      this._dateGrid.Children.Add((UIElement) element2);
      TextBlock textBlock = new TextBlock();
      textBlock.Margin = new Thickness(4.0, 0.0, 4.0, 0.0);
      textBlock.Background = (Brush) Brushes.Transparent;
      textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      textBlock.IsHitTestVisible = false;
      this._dateText = textBlock;
      this._dateText.SetBinding(TextBlock.ForegroundProperty, "DateIconColor");
      this._dateText.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body01");
      this._dateText.SetBinding(TextBlock.TextProperty, "DateText");
      this._dateText.SetValue(Grid.ColumnProperty, (object) 1);
      this._dateGrid.Children.Add((UIElement) this._dateText);
      Image image = new Image();
      image.Width = 14.0;
      image.Height = 14.0;
      image.Cursor = Cursors.Hand;
      image.Margin = new Thickness(0.0, 4.0, 4.0, 4.0);
      image.Opacity = 0.4;
      Image element3 = image;
      element3.SetBinding(FrameworkElement.ToolTipProperty, "RepeatText");
      element3.SetValue(ToolTipService.InitialShowDelayProperty, (object) 500);
      element3.SetValue(ToolTipService.BetweenShowDelayProperty, (object) 0);
      element3.SetValue(Grid.ColumnProperty, (object) 2);
      element3.SetResourceReference(Image.SourceProperty, (object) "RepeatDrawingImage");
      element3.SetResourceReference(FrameworkElement.StyleProperty, (object) "Icon01");
      element3.SetBinding(UIElement.VisibilityProperty, "ShowRepeatIcon");
      this._dateGrid.Children.Add((UIElement) element3);
    }

    private ContentControl GetNoteMessageContent()
    {
      if (!(this.DataContext is TaskDetailViewModel dataContext))
        return (ContentControl) null;
      ContentControl noteMessageContent = new ContentControl();
      noteMessageContent.SetResourceReference(FrameworkElement.StyleProperty, (object) "PopupContentStyle");
      StackPanel stackPanel = new StackPanel();
      noteMessageContent.Content = (object) stackPanel;
      Style resource1 = (Style) this.FindResource((object) "Tag05");
      Style resource2 = (Style) this.FindResource((object) "Tag01");
      TextBlock textBlock1 = new TextBlock();
      textBlock1.Text = Utils.GetString("Words");
      textBlock1.LineHeight = 16.0;
      textBlock1.Margin = new Thickness(16.0, 4.0, 16.0, 0.0);
      textBlock1.Style = resource1;
      TextBlock element1 = textBlock1;
      TextBlock textBlock2 = new TextBlock();
      textBlock2.Text = Utils.IntToStringWithDivide(string.IsNullOrEmpty(dataContext.TaskContent) ? 0 : dataContext.TaskContent.Length);
      textBlock2.LineHeight = 16.0;
      textBlock2.Margin = new Thickness(16.0, 2.0, 16.0, 8.0);
      textBlock2.Style = resource2;
      textBlock2.FontSize = 11.0;
      TextBlock element2 = textBlock2;
      TextBlock textBlock3 = new TextBlock();
      textBlock3.Text = Utils.GetString("CreatedAt");
      textBlock3.LineHeight = 16.0;
      textBlock3.Margin = new Thickness(16.0, 4.0, 16.0, 0.0);
      textBlock3.Style = resource1;
      TextBlock element3 = textBlock3;
      TextBlock textBlock4 = new TextBlock();
      DateTime? createDate = dataContext.CreateDate;
      ref DateTime? local1 = ref createDate;
      textBlock4.Text = local1.HasValue ? local1.GetValueOrDefault().ToString("D", (IFormatProvider) App.Ci) : (string) null;
      textBlock4.LineHeight = 16.0;
      textBlock4.Margin = new Thickness(16.0, 2.0, 16.0, 8.0);
      textBlock4.Style = resource2;
      textBlock4.FontSize = 11.0;
      TextBlock element4 = textBlock4;
      TextBlock textBlock5 = new TextBlock();
      textBlock5.Text = Utils.GetString("ModifiedAt");
      textBlock5.LineHeight = 16.0;
      textBlock5.Margin = new Thickness(16.0, 4.0, 16.0, 0.0);
      textBlock5.Style = resource1;
      TextBlock element5 = textBlock5;
      TextBlock textBlock6 = new TextBlock();
      DateTime? modifiedDate = dataContext.ModifiedDate;
      ref DateTime? local2 = ref modifiedDate;
      textBlock6.Text = local2.HasValue ? local2.GetValueOrDefault().ToString("D", (IFormatProvider) App.Ci) : (string) null;
      textBlock6.LineHeight = 16.0;
      textBlock6.Margin = new Thickness(16.0, 2.0, 16.0, 8.0);
      textBlock6.Style = resource2;
      textBlock6.FontSize = 11.0;
      TextBlock element6 = textBlock6;
      stackPanel.Children.Add((UIElement) element1);
      stackPanel.Children.Add((UIElement) element2);
      stackPanel.Children.Add((UIElement) element3);
      stackPanel.Children.Add((UIElement) element4);
      stackPanel.Children.Add((UIElement) element5);
      stackPanel.Children.Add((UIElement) element6);
      return noteMessageContent;
    }

    private TaskDetailView GetParent() => Utils.FindParent<TaskDetailView>((DependencyObject) this);

    private void OnShowPopupMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._showPopupMouseDown = true;
    }

    private void PopupClosed(object sender, EventArgs e)
    {
      this.GetParent()?.PopupClosed(sender, e);
    }

    private void PopupOpened(object sender, EventArgs e)
    {
      this.GetParent()?.PopupOpened(sender, e);
    }

    private void OnPriorityClick(object sender, MouseButtonEventArgs e)
    {
      TaskDetailView parent = this.GetParent();
      if ((parent != null ? (parent.CheckEnable() ? 1 : 0) : 0) == 0)
        return;
      if (this._showPopupMouseDown && this.DataContext is TaskDetailViewModel dataContext)
      {
        SetPriorityDialog setPriorityDialog = new SetPriorityDialog(dataContext.Priority);
        this._popup.Child = (UIElement) setPriorityDialog;
        this._popup.PlacementTarget = (UIElement) (sender as FrameworkElement);
        // ISSUE: method pointer
        setPriorityDialog.PrioritySelect += new EventHandler<int>((object) this, __methodptr(\u003COnPriorityClick\u003Eg__OnPrioritySelected\u007C22_0));
        this._popup.IsOpen = true;
      }
      this._showPopupMouseDown = false;
    }

    private void ShowNoteMessage(object sender)
    {
      if (this._showPopupMouseDown)
      {
        this._popup.Child = (UIElement) this.GetNoteMessageContent();
        this._popup.PlacementTarget = (UIElement) (sender as FrameworkElement);
        this._popup.IsOpen = true;
      }
      this._showPopupMouseDown = false;
    }

    private void SelectDateClick(object sender, MouseButtonEventArgs e)
    {
      if (this._showPopupMouseDown)
        this.GetParent()?.SelectDateClick(e);
      this._showPopupMouseDown = false;
    }

    private void OnSetAssigneeMouseUp(object sender, MouseButtonEventArgs e)
    {
      TaskDetailView parent = this.GetParent();
      if (parent == null || !parent.CheckEnable() || !this._showPopupMouseDown)
        return;
      this._showPopupMouseDown = false;
      if (!(this.DataContext is TaskDetailViewModel dataContext))
        return;
      if (Utils.IsNetworkAvailable())
      {
        this._popup.PlacementTarget = (UIElement) (sender as FrameworkElement);
        SetAssigneeDialog setAssigneeDialog = new SetAssigneeDialog(dataContext.ProjectId, (Popup) this._popup, dataContext.Assignee);
        parent.AddActionEvent("action", "assignee");
        setAssigneeDialog.AssigneeSelect += (EventHandler<AvatarInfo>) (async (o, avatar) => await parent.SetAssignee(avatar.UserId, false));
        setAssigneeDialog.Show();
      }
      else
        parent.TryToast(Utils.GetString("NoNetwork"));
    }

    private void OnSelectTimeMouseDown(object sender, MouseButtonEventArgs e)
    {
      TaskDetailView parent = this.GetParent();
      if (parent != null && parent.DatePopupShow)
        parent.DatePopupShow = false;
      else
        this._showPopupMouseDown = true;
    }

    private void OnBackClick(object sender, MouseButtonEventArgs e)
    {
      this.GetParent()?.OnBackClick();
    }

    private void CheckBoxRightMouseUp(object sender, MouseButtonEventArgs e)
    {
      this.GetParent()?.OnCheckBoxMouseRightUp((UIElement) this._checkIcon);
    }

    private void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      this.GetParent()?.OnCheckBoxClick();
    }

    public void SetBackIcon(bool show, bool isNavigate = false)
    {
      if (show)
      {
        Path path = UiUtils.CreatePath(isNavigate ? "NavigateBackPath" : "IcClose", "BaseColorOpacity40", "Path01");
        path.Height = 11.0;
        path.Width = 11.0;
        this._backBt.Child = (UIElement) path;
        this._backBt.Visibility = Visibility.Visible;
      }
      else
      {
        this._backBt.Child = (UIElement) null;
        this._backBt.Visibility = Visibility.Collapsed;
      }
    }

    private void OnProgressChanged(int pointerProgress, int currentProgress)
    {
      Utils.FindParent<TaskDetailView>((DependencyObject) this)?.ProgressClick(pointerProgress, currentProgress);
    }

    public async Task SetProgress(int progress, bool withAnim = false)
    {
      this._progress.SetProgress(progress, withAnim);
    }

    public void CheckProgress()
    {
      if (!(this.DataContext is TaskDetailViewModel dataContext))
        return;
      int progress1 = this._progress.GetProgress();
      int? progress2 = dataContext.Progress;
      int valueOrDefault1 = progress2.GetValueOrDefault();
      if (progress1 == valueOrDefault1 & progress2.HasValue)
        return;
      TaskProgressControl progress3 = this._progress;
      progress2 = dataContext.Progress;
      int valueOrDefault2 = progress2.GetValueOrDefault();
      progress3.SetProgress(valueOrDefault2);
    }

    public async Task SetAvatarVisible(bool showAssign, string assignee = null, string projectId = null)
    {
      TaskDetailHeadView taskDetailHeadView1 = this;
      if (showAssign)
      {
        string avatarUrl = (string) null;
        if (!string.IsNullOrEmpty(assignee) && assignee != "-1")
          avatarUrl = await AvatarHelper.GetAvatarUrl(assignee, projectId);
        if (taskDetailHeadView1._assignIcon == null)
        {
          TaskDetailHeadView taskDetailHeadView2 = taskDetailHeadView1;
          AssignIcon assignIcon = new AssignIcon();
          assignIcon.Cursor = Cursors.Hand;
          assignIcon.Margin = new Thickness(0.0, 0.0, 4.0, 0.0);
          taskDetailHeadView2._assignIcon = assignIcon;
          taskDetailHeadView1._assignIcon.MouseLeftButtonUp += new MouseButtonEventHandler(taskDetailHeadView1.OnSetAssigneeMouseUp);
          taskDetailHeadView1._assignIcon.MouseLeftButtonDown += new MouseButtonEventHandler(taskDetailHeadView1.OnShowPopupMouseDown);
        }
        if (!taskDetailHeadView1._rightIcons.Children.Contains((UIElement) taskDetailHeadView1._assignIcon))
          taskDetailHeadView1._rightIcons.Children.Insert(0, (UIElement) taskDetailHeadView1._assignIcon);
        taskDetailHeadView1._assignIcon.ToolTip = string.IsNullOrEmpty(assignee) ? (object) Utils.GetString("AssignTo") : (object) AvatarHelper.GetCacheUserName(assignee, projectId);
        taskDetailHeadView1._assignIcon.SetAvatar(avatarUrl);
      }
      else
      {
        if (taskDetailHeadView1._assignIcon == null)
          return;
        taskDetailHeadView1._rightIcons.Children.Remove((UIElement) taskDetailHeadView1._assignIcon);
        taskDetailHeadView1._assignIcon = (AssignIcon) null;
      }
    }

    public UIElement GetDateDropTarget() => (UIElement) this._dateText;
  }
}
