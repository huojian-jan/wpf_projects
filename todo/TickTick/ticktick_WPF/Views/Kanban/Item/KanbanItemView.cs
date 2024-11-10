// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.Item.KanbanItemView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.MarkDown.Colorizer;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Kanban.Item
{
  public class KanbanItemView : Border, ITaskOperation, IShowTaskDetailWindow
  {
    private KanbanItemController _controller;
    private bool _previewLeftClick;
    private bool _isWaitingDoubleClick;
    private DateTime _lastClickTime;
    private bool _previewRightClick;
    private System.Windows.Point _startPoint;
    private Grid _root;
    private TaskCheckIcon _checkIcon;
    private Border _bottomRect;
    private readonly Grid _container;
    private Rectangle _dragRect;
    private Border _titleBorder;
    private StackPanel _timeProjectPanel;
    private KanbanItemIcons _itemIcons;
    private Border _foldIcon;
    private TextBlock _contentText;
    private ListItemTagsControl _tagPanel;
    private ListItemProjectLabel _projectLabel;
    private Ellipse _avatarImage;
    private RowDefinition _firstRow;

    protected bool InPopup { get; set; }

    public KanbanItemView()
    {
      TaskCheckIcon taskCheckIcon = new TaskCheckIcon();
      taskCheckIcon.Width = 14.0;
      taskCheckIcon.VerticalAlignment = VerticalAlignment.Top;
      this._checkIcon = taskCheckIcon;
      this._bottomRect = new Border();
      this._container = new Grid();
      this._titleBorder = new Border();
      StackPanel stackPanel = new StackPanel();
      stackPanel.Orientation = Orientation.Horizontal;
      stackPanel.Margin = new Thickness(4.0, 1.0, 12.0, 0.0);
      this._timeProjectPanel = stackPanel;
      KanbanItemIcons kanbanItemIcons = new KanbanItemIcons();
      kanbanItemIcons.Margin = new Thickness(0.0, 0.0, 2.0, 4.0);
      this._itemIcons = kanbanItemIcons;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.InitUi();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.ClearBinding());
    }

    private void InitUi()
    {
      this.MinHeight = 42.0;
      this.Cursor = Cursors.Hand;
      this.Child = (UIElement) this._bottomRect;
      this._bottomRect.Child = (UIElement) this._root;
      this._root.Children.Add((UIElement) this._container);
      this._firstRow = new RowDefinition()
      {
        Height = GridLength.Auto
      };
      this._firstRow.SetResourceReference(RowDefinition.MaxHeightProperty, (object) "KanbanTitleMaxHeight");
      this._container.RowDefinitions.Add(this._firstRow);
      this._container.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._container.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._container.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._container.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(34.0)
      });
      this._container.ColumnDefinitions.Add(new ColumnDefinition());
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDragMouseUp);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnLeftMouseDown);
      this.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
      this.MouseRightButtonDown += new MouseButtonEventHandler(this.OnRightMouseDown);
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      this.MouseMove += new MouseEventHandler(this.OnItemDrag);
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      this.RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
      this.InitCheckIcon();
      this.InitIcons();
      this.InitTitle();
      this.SetResourceReference(Border.BackgroundProperty, (object) "TextInverseColor");
      this.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity10");
      this.Effect = (Effect) new DropShadowEffect()
      {
        BlurRadius = 3.0,
        ShadowDepth = 3.0,
        Direction = 270.0,
        Opacity = 0.04
      };
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (this._tagPanel == null || Math.Abs(e.NewSize.Width - e.PreviousSize.Width) <= 10.0 || !(this.DataContext is DisplayItemModel dataContext))
        return;
      int maxCount = LocalSettings.Settings.ExtraSettings.KbSize == 0 ? 1 : 2;
      dataContext.SetDisplayTags(maxCount);
    }

    private void InitIcons()
    {
      this._timeProjectPanel.SetValue(Grid.ColumnProperty, (object) 1);
      this._timeProjectPanel.SetValue(Grid.RowProperty, (object) 3);
      this._timeProjectPanel.Children.Add((UIElement) this._itemIcons);
      this._itemIcons.SizeChanged += new SizeChangedEventHandler(this.OnIconSizeChanged);
      this._container.Children.Add((UIElement) this._timeProjectPanel);
    }

    private void InitTitle()
    {
      this._titleBorder.SetValue(Grid.ColumnProperty, (object) 1);
      this._titleBorder.VerticalAlignment = VerticalAlignment.Top;
      this._titleBorder.Margin = new Thickness(8.0, 12.0, 12.0, -2.0);
      this._container.Children.Add((UIElement) this._titleBorder);
    }

    private void InitCheckIcon()
    {
      this._checkIcon.SetResourceReference(FrameworkElement.MarginProperty, (object) "KanbanItemCheckBoxMargin");
      this._checkIcon.MouseRightButtonUp += new MouseButtonEventHandler(this.OnCheckBoxRightMouseUp);
      this._checkIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
      this._container.Children.Add((UIElement) this._checkIcon);
    }

    private void SetProject(DisplayItemModel model)
    {
      if (model.ShowProject)
      {
        if (this._projectLabel == null)
        {
          ListItemProjectLabel itemProjectLabel = new ListItemProjectLabel();
          itemProjectLabel.Margin = new Thickness(2.0, 2.0, 0.0, 6.0);
          this._projectLabel = itemProjectLabel;
          this._projectLabel.ProjectTitle.MaxWidth = double.PositiveInfinity;
          this._projectLabel.IsHitTestVisible = !model.IsCourse;
          this._timeProjectPanel.Children.Add((UIElement) this._projectLabel);
        }
        this._projectLabel.Opacity = model.Status == 0 ? 1.0 : 0.4;
      }
      else
      {
        this._timeProjectPanel.Children.Remove((UIElement) this._projectLabel);
        this._projectLabel = (ListItemProjectLabel) null;
      }
    }

    private void SetTagPanel(DisplayItemModel model)
    {
      if (model.ShowTag)
      {
        if (this._tagPanel == null)
        {
          ListItemTagsControl listItemTagsControl = new ListItemTagsControl();
          listItemTagsControl.Margin = new Thickness(3.0, 1.0, 0.0, 2.0);
          this._tagPanel = listItemTagsControl;
          this._tagPanel.TagDelete += new EventHandler<string>(this.OnTagDelete);
          this._tagPanel.SetValue(Grid.RowProperty, (object) 2);
          this._tagPanel.SetValue(Grid.ColumnProperty, (object) 1);
          this._container.Children.Add((UIElement) this._tagPanel);
        }
        this._tagPanel.Opacity = model.Status == 0 ? 1.0 : 0.4;
      }
      else
      {
        if (this._tagPanel != null)
          this._tagPanel.TagDelete -= new EventHandler<string>(this.OnTagDelete);
        this._container.Children.Remove((UIElement) this._tagPanel);
        this._tagPanel = (ListItemTagsControl) null;
      }
    }

    private void SetTitle(string title, int status)
    {
      if (string.IsNullOrEmpty(title))
      {
        if (this._titleBorder.Child is TextBlock)
          return;
        TextBlock textBlock1 = new TextBlock();
        textBlock1.Margin = new Thickness(0.0, 0.0, 0.0, 6.0);
        textBlock1.Text = Utils.GetString("NoTitle");
        textBlock1.TextDecorations = LocalSettings.Settings.ShowCompleteLine != 1 || status == 0 ? (TextDecorationCollection) null : DeleteLineColorizer.HintStrikethrough;
        TextBlock textBlock2 = textBlock1;
        textBlock2.SetResourceReference(Control.FontSizeProperty, (object) "Font14");
        textBlock2.SetResourceReference(Control.ForegroundProperty, (object) "BaseColorOpacity60");
        this._titleBorder.Child = (UIElement) textBlock2;
      }
      else
      {
        if (!(this._titleBorder.Child is TaskTitleBox))
        {
          TaskTitleBox taskTitleBox1 = new TaskTitleBox();
          taskTitleBox1.Margin = new Thickness(0.0, 0.0, 4.0, 0.0);
          taskTitleBox1.IsHitTestVisible = false;
          taskTitleBox1.ReadOnly = true;
          taskTitleBox1.VerticalAlignment = VerticalAlignment.Top;
          taskTitleBox1.RenderLink = true;
          taskTitleBox1.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
          TaskTitleBox taskTitleBox2 = taskTitleBox1;
          taskTitleBox2.SetWordWrap(true);
          taskTitleBox2.SetBinding(LinkTextEditBox.TextStatusProperty, "Status");
          taskTitleBox2.SetResourceReference(Control.FontSizeProperty, (object) "Font14");
          this._titleBorder.Child = (UIElement) taskTitleBox2;
          taskTitleBox2.LineSpacing = 4.0;
        }
        TaskTitleBox child = (TaskTitleBox) this._titleBorder.Child;
        if (child.Text != title)
          child.SetText(title);
        child.SetTextForeground();
      }
    }

    private void SetContentText(DisplayItemModel model)
    {
      if (LocalSettings.Settings.ShowDetails)
        DisplayItemModel.AssembleModelContent(model);
      if (model.ShowContent)
      {
        if (this._contentText == null)
        {
          TextBlock textBlock = new TextBlock();
          textBlock.TextWrapping = TextWrapping.NoWrap;
          textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
          textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
          textBlock.Margin = new Thickness(8.0, 2.0, 0.0, 4.0);
          this._contentText = textBlock;
          this._contentText.SetResourceReference(TextBlock.FontSizeProperty, (object) "Font12");
          this._contentText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
          this._contentText.SetValue(Grid.RowProperty, (object) 1);
          this._contentText.SetValue(Grid.ColumnProperty, (object) 1);
          this._container.Children.Add((UIElement) this._contentText);
        }
        this._contentText.Opacity = model.Status == 0 ? 1.0 : 0.6;
        string text = model.Content;
        if (text.Length > 200)
          text = text.Substring(0, 200);
        string str = TaskUtils.ReplaceAttachmentTextInString(text).Replace("\r\n", " ").Replace("\n", "  ");
        if (str.Length > 50)
          str = str.Substring(0, 50);
        this._contentText.Text = str;
      }
      else
      {
        this._container.Children.Remove((UIElement) this._contentText);
        this._contentText = (TextBlock) null;
      }
    }

    private void SetFoldIcon(DisplayItemModel model)
    {
      if (model.HasChildren)
      {
        if (this._foldIcon == null)
        {
          Border border = new Border();
          border.Width = 14.0;
          border.Height = 14.0;
          border.Cursor = Cursors.Hand;
          border.VerticalAlignment = VerticalAlignment.Top;
          border.HorizontalAlignment = HorizontalAlignment.Left;
          this._foldIcon = border;
          this._foldIcon.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_60");
          this._foldIcon.SetResourceReference(FrameworkElement.MarginProperty, (object) "KanbanItemFoldIconMargin");
          this._foldIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFoldClick);
          this._container.Children.Add((UIElement) this._foldIcon);
          Path arrow = UiUtils.GetArrow(12.0, 0.0, "BaseColorOpacity100");
          arrow.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
          this._foldIcon.Child = (UIElement) arrow;
        }
        this._foldIcon.Child.RenderTransform = (Transform) new RotateTransform(model.IsOpen ? 0.0 : -90.0);
      }
      else
      {
        if (this._foldIcon != null)
        {
          this._container.Children.Remove((UIElement) this._foldIcon);
          this._foldIcon.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnFoldClick);
          this._foldIcon = (Border) null;
        }
        this._checkIcon.SetResourceReference(FrameworkElement.MarginProperty, (object) "KanbanItemCheckBoxMargin");
      }
    }

    private void SetDraggingBorder(bool show, int level)
    {
      if (show)
      {
        this._container.Opacity = 0.0;
        if (this._dragRect == null)
        {
          Rectangle rectangle = new Rectangle();
          rectangle.Fill = (Brush) ThemeUtil.GetColor("BaseColorOpacity5");
          rectangle.Stroke = (Brush) ThemeUtil.GetColor("TaskDragBorderColor");
          rectangle.StrokeThickness = 1.0;
          rectangle.RadiusX = 3.0;
          rectangle.RadiusY = 3.0;
          this._dragRect = rectangle;
          this._root.Children.Add((UIElement) this._dragRect);
        }
        this._dragRect.Margin = level > 0 ? new Thickness((double) (16 + level * 24), 4.0, 4.0, 4.0) : new Thickness(0.0);
      }
      else
      {
        this._container.Opacity = 1.0;
        if (this._dragRect == null)
          return;
        this._root.Children.Remove((UIElement) this._dragRect);
        this._dragRect = (Rectangle) null;
      }
    }

    public void SetContainerMargin(DisplayItemModel model)
    {
      this._container.Margin = new Thickness(this.InPopup ? 0.0 : (double) (model.Level * 24), 0.0, 0.0, 4.0);
    }

    private void SetCornerRadius(DisplayItemModel model)
    {
      int num1 = model.ShowTopMargin ? 1 : 0;
      int num2 = model.ShowBottomMargin ? 1 : 0;
      this.CornerRadius = new CornerRadius((double) (num1 * 6), (double) (num1 * 6), (double) (num2 * 6), (double) (num2 * 6));
      this._bottomRect.CornerRadius = this.CornerRadius;
      this.BorderThickness = new Thickness(0.5, (double) num1 * 0.5, 0.5, (double) num2 * 0.5);
      this.Margin = new Thickness(12.0, model.ShowTopMargin ? 2.0 : 0.0, 12.0, model.ShowBottomMargin ? 6.0 : 0.0);
    }

    private void SetBackGround(DisplayItemModel model)
    {
      if (model == null)
        return;
      this._bottomRect.SetResourceReference(Border.BackgroundProperty, model.Dragging ? (object) "KanbanTaskItemBackground" : (model.Selected ? (object) "ItemSelectedColor" : (model.InOperation || this.IsMouseOver ? (object) "BaseColorOpacity3" : (object) "KanbanTaskItemBackground")));
    }

    private void SetAvatar(DisplayItemModel model)
    {
      if (model.Avatar != null)
      {
        if (this._avatarImage == null)
        {
          Ellipse ellipse = new Ellipse();
          ellipse.StrokeThickness = 1.0;
          ellipse.Height = 20.0;
          ellipse.Width = 20.0;
          ellipse.VerticalAlignment = VerticalAlignment.Top;
          ellipse.HorizontalAlignment = HorizontalAlignment.Right;
          ellipse.Margin = new Thickness(0.0, 10.0, 8.0, 0.0);
          this._avatarImage = ellipse;
          this._avatarImage.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity5");
          this._avatarImage.SetValue(Grid.RowProperty, (object) 0);
          this._avatarImage.SetValue(Grid.ColumnProperty, (object) 1);
          this._container.Children.Add((UIElement) this._avatarImage);
          this._titleBorder.Margin = new Thickness(8.0, 12.0, 30.0, 0.0);
        }
        Ellipse avatarImage = this._avatarImage;
        ImageBrush imageBrush = new ImageBrush();
        imageBrush.ImageSource = (ImageSource) model.Avatar;
        imageBrush.Stretch = Stretch.Fill;
        avatarImage.Fill = (Brush) imageBrush;
        this._avatarImage.ToolTip = (object) AvatarHelper.GetCacheUserName(model.Assignee, model.ProjectId);
      }
      else
      {
        this._container.Children.Remove((UIElement) this._avatarImage);
        this._avatarImage = (Ellipse) null;
        this._titleBorder.Margin = new Thickness(8.0, 12.0, 12.0, 0.0);
      }
    }

    private async void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      KanbanItemView element = this;
      DisplayItemModel newValue = e.NewValue as DisplayItemModel;
      DisplayItemModel oldValue = e.OldValue as DisplayItemModel;
      element.SetModel((object) oldValue, (object) newValue);
      if (newValue == null || string.IsNullOrEmpty(newValue.Id))
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(element.OnTaskPropertyChanged), string.Empty);
      if (element._controller == null)
      {
        element._controller = new KanbanItemController((UIElement) element, newValue);
        element._itemIcons.SetController((DisplayItemController) element._controller);
      }
      else
        element._controller.Reset((UIElement) element, newValue);
      element._checkIcon.SetIconColor(newValue.Type, newValue.Priority, newValue.Status);
      element.SetTitle(newValue.Title, newValue.Status);
      element.SetBackGround(newValue);
      newValue.SetIcon();
      element.SetFoldIcon(newValue);
      element.SetCornerRadius(newValue);
      newValue.TrySetAvatar();
      element.SetAvatar(newValue);
      element.SetContentText(newValue);
      element.SetTagPanel(newValue);
      element.SetDraggingBorder(newValue.Dragging, newValue.Level);
      element.SetContainerMargin(newValue);
      element.SetProject(newValue);
      element.SetItemIconsMaxWidth(newValue);
    }

    private void SetModel(object oldVal, object newVal)
    {
      DisplayItemModel oldModel = oldVal as DisplayItemModel;
      DisplayItemModel newModel = newVal as DisplayItemModel;
      Task.Run((Action) (() =>
      {
        newModel?.SetPropertyChangedEvent();
        if (oldModel != null)
          PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldModel, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), string.Empty);
        if (newModel == null)
          return;
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newModel, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), string.Empty);
      }));
    }

    private void OnIconSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || this._projectLabel == null)
        return;
      this._projectLabel.ProjectTitle.MaxWidth = Math.Max(20.0, (double) (200 - (dataContext.ShowProjectColor ? -20 : 0)) - Math.Min(e.NewSize.Width, this._itemIcons.MaxWidth));
    }

    private void ClearBinding()
    {
      if (this.DataContext is DisplayItemModel dataContext)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) dataContext, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), string.Empty);
      if (this._tagPanel != null)
      {
        this._tagPanel.TagDelete -= new EventHandler<string>(this.OnTagDelete);
        this._container.Children.Remove((UIElement) this._tagPanel);
        this._tagPanel = (ListItemTagsControl) null;
      }
      if (this._contentText != null)
      {
        this._container.Children.Remove((UIElement) this._contentText);
        this._contentText = (TextBlock) null;
      }
      this._itemIcons.Children.Clear();
      this._controller?.Clear();
      this._controller = (KanbanItemController) null;
      this._titleBorder.Child = (UIElement) null;
    }

    private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      string propertyName = e.PropertyName;
      if (propertyName == null)
        return;
      switch (propertyName.Length)
      {
        case 5:
          switch (propertyName[0])
          {
            case 'L':
              if (!(propertyName == "Level"))
                return;
              this.SetContainerMargin(dataContext);
              this.SetDraggingBorder(dataContext.Dragging, dataContext.Level);
              return;
            case 'T':
              if (!(propertyName == "Title"))
                return;
              this.SetTitle(dataContext.Title, dataContext.Status);
              return;
            default:
              return;
          }
        case 6:
          switch (propertyName[0])
          {
            case 'A':
              if (!(propertyName == "Avatar"))
                return;
              this.SetAvatar(dataContext);
              return;
            case 'I':
              if (!(propertyName == "IsOpen"))
                return;
              this.SetFoldIcon(dataContext);
              return;
            default:
              return;
          }
        case 7:
          if (!(propertyName == "Content"))
            return;
          this.SetContentText(dataContext);
          return;
        case 8:
          switch (propertyName[0])
          {
            case 'D':
              if (!(propertyName == "Dragging"))
                return;
              this.SetBackGround(dataContext);
              this.SetDraggingBorder(dataContext.Dragging, dataContext.Level);
              return;
            case 'P':
              if (!(propertyName == "Priority"))
                return;
              this._checkIcon.SetIconColor(dataContext.Type, dataContext.Priority, dataContext.Status);
              return;
            case 'S':
              if (!(propertyName == "Selected"))
                return;
              goto label_54;
            default:
              return;
          }
        case 9:
          if (!(propertyName == "ShowIcons"))
            return;
          this._itemIcons.ResetIcons();
          return;
        case 10:
          return;
        case 11:
          switch (propertyName[0])
          {
            case 'D':
              if (!(propertyName == "DisplayTags"))
                return;
              this.SetTagPanel(dataContext);
              return;
            case 'I':
              if (!(propertyName == "InOperation"))
                return;
              goto label_54;
            case 'P':
              if (!(propertyName == "ProjectName"))
                return;
              break;
            default:
              return;
          }
          break;
        case 12:
          return;
        case 13:
          if (!(propertyName == "ShowTopMargin"))
            return;
          goto label_53;
        case 14:
          return;
        case 15:
          return;
        case 16:
          switch (propertyName[4])
          {
            case 'B':
              if (!(propertyName == "ShowBottomMargin"))
                return;
              goto label_53;
            case 'P':
              if (!(propertyName == "ShowProjectColor"))
                return;
              break;
            default:
              return;
          }
          break;
        default:
          return;
      }
      this.SetItemIconsMaxWidth(dataContext);
      return;
label_53:
      this.SetCornerRadius(dataContext);
      this.SetContainerMargin(dataContext);
      return;
label_54:
      this.SetBackGround(dataContext);
    }

    private void SetItemIconsMaxWidth(DisplayItemModel model)
    {
      if (model.ShowProject)
      {
        double width = Utils.MeasureString(model.ProjectName, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0).Width;
        this._itemIcons.MaxWidth = Math.Max(124.0, (double) (200 - (model.ShowProjectColor ? 20 : 0)) - width);
        if (this._projectLabel == null)
          return;
        this._projectLabel.ProjectTitle.MaxWidth = Math.Max(20.0, (double) (200 - (model.ShowProjectColor ? -20 : 0)) - Math.Min(this._itemIcons.ActualWidth, this._itemIcons.MaxWidth));
      }
      else
        this._itemIcons.MaxWidth = double.PositiveInfinity;
    }

    private void OnFoldClick(object sender, MouseButtonEventArgs e)
    {
      this._controller?.OnOpenPathClick(sender, e);
      e.Handled = true;
    }

    private void OnCheckBoxRightMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this._controller?.OnCheckBoxRightMouseUp((UIElement) this._checkIcon);
    }

    private void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!(this.DataContext is DisplayItemModel dataContext) || dataContext.IsToggling)
        return;
      dataContext.IsToggling = true;
      this._controller?.OnCheckBoxClick(dataContext);
    }

    private async void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._previewRightClick)
        return;
      this._previewRightClick = false;
      bool flag = this._controller != null;
      if (flag)
        flag = !await this._controller?.ShowOperationDialogSafely();
      if (!flag)
        return;
      TaskDetailPopup window = await this._controller.ShowTaskDetail();
      await Task.Delay(100);
      window?.TryShow();
      window = (TaskDetailPopup) null;
    }

    private void OnTagDelete(object sender, string e) => this._controller?.OnTagDelete(e.ToLower());

    private void OnItemDrag(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || !this._previewLeftClick || !(this.DataContext is DisplayItemModel dataContext) || dataContext.IsHabit || !dataContext.Enable)
        return;
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      if (Math.Abs(position.X - this._startPoint.X) <= 4.0 && Math.Abs(position.Y - this._startPoint.Y) <= 4.0)
        return;
      this._previewLeftClick = false;
      this._controller?.OnKanbanItemDrag();
    }

    private async void OnDragMouseUp(object sender, MouseButtonEventArgs e)
    {
      KanbanItemView kanbanItemView = this;
      if (!(kanbanItemView.DataContext is DisplayItemModel model))
        model = (DisplayItemModel) null;
      else if (kanbanItemView._controller == null)
        model = (DisplayItemModel) null;
      else if (model.IsEvent)
      {
        kanbanItemView.OpenEventDetailWindow(model);
        model = (DisplayItemModel) null;
      }
      else if (model.IsCourse)
      {
        kanbanItemView.ShowCourseDetail(model);
        model = (DisplayItemModel) null;
      }
      else if (model.IsHabit)
      {
        kanbanItemView.ShowHabitDetail(model, e);
        model = (DisplayItemModel) null;
      }
      else
      {
        bool flag = Utils.IfCtrlPressed() || Utils.IfShiftPressed();
        if (((kanbanItemView._previewLeftClick ? 1 : (model.Selected ? 1 : 0)) | (flag ? 1 : 0)) == 0)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          kanbanItemView._isWaitingDoubleClick = false;
          if (flag)
          {
            kanbanItemView._controller.OnBatchSelectMouseUp();
            kanbanItemView._previewLeftClick = false;
            model = (DisplayItemModel) null;
          }
          else
          {
            bool firstClick = (DateTime.Now - kanbanItemView._lastClickTime).TotalMilliseconds > 300.0;
            kanbanItemView._lastClickTime = DateTime.Now;
            if (firstClick)
            {
              kanbanItemView._isWaitingDoubleClick = true;
              model.Selected = true;
              TaskDetailPopup window = await kanbanItemView._controller.ShowDetailWindow();
              if (window != null)
                window.Opacity = 0.0;
              await Task.Delay(150);
              if (!kanbanItemView._isWaitingDoubleClick)
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
            if (!firstClick)
            {
              model.Selected = false;
              TaskDetailWindows.ShowTaskWindows(model.Id);
            }
            kanbanItemView._previewLeftClick = false;
            model = (DisplayItemModel) null;
          }
        }
      }
    }

    private async void OpenEventDetailWindow(DisplayItemModel model)
    {
      KanbanItemView target = this;
      string entityId = model.EntityId;
      if (string.IsNullOrEmpty(entityId))
        return;
      CalendarEventModel eventById = await CalendarEventDao.GetEventById(ArchivedDao.GetOriginalId(entityId));
      if (eventById == null)
        return;
      eventById.DueStart = model.StartDate;
      eventById.DueEnd = model.DueDate;
      model.Selected = true;
      CalendarDetailWindow calendarDetailWindow = new CalendarDetailWindow();
      calendarDetailWindow.Disappear -= new EventHandler<string>(target.OnDetailClosed);
      calendarDetailWindow.Disappear += new EventHandler<string>(target.OnDetailClosed);
      calendarDetailWindow.Show((UIElement) target, target.ActualWidth + 6.0, 0.0, false, new CalendarDetailViewModel(eventById));
    }

    private async void ShowHabitDetail(DisplayItemModel model, MouseButtonEventArgs e)
    {
      KanbanItemView target = this;
      HabitModel habitById = await HabitDao.GetHabitById(model.Habit?.Id);
      if (habitById == null)
        ;
      else
      {
        HabitCheckInWindow window = new HabitCheckInWindow(habitById, model.Status, model.StartDate, target.GetToastWindow());
        window.Show((UIElement) target, target.ActualWidth, 0.0, false);
        window.OnAction += new EventHandler(target.OnCheckInAction);
        window.Closed += (EventHandler) ((sender, args) => window.OnAction -= new EventHandler(this.OnCheckInAction));
        model.Selected = true;
      }
    }

    private void OnCheckInAction(object sender, EventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      dataContext.Selected = false;
    }

    private IToastShowWindow GetToastWindow()
    {
      return Utils.FindParent<IToastShowWindow>((DependencyObject) this);
    }

    private async void ShowCourseDetail(DisplayItemModel model)
    {
      KanbanItemView target = this;
      if (model.Course == null)
        ;
      else
      {
        CourseDetailViewModel viewModel = await CourseDetailViewModel.Build(model.Course);
        model.Selected = true;
        CourseDetailWindow courseDetailWindow = new CourseDetailWindow(viewModel);
        courseDetailWindow.Closed += (EventHandler) (async (obj, e) =>
        {
          model.Selected = false;
          await Task.Delay(200);
        });
        courseDetailWindow.Show((UIElement) target, target.ActualWidth, false);
      }
    }

    private void OnDetailClosed(object sender, string e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      dataContext.Selected = false;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      this._previewLeftClick = false;
      this._previewRightClick = false;
      this.SetBackGround(this.DataContext as DisplayItemModel);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      this.SetBackGround(this.DataContext as DisplayItemModel);
    }

    public void SetPriority(int priority) => this._controller?.SetPriority(priority);

    public void SetDate(string key) => this._controller?.SetDate(key);

    public void ClearDate() => this._controller?.ClearDate();

    public void SelectDate(bool relative) => this._controller?.SelectDate();

    public void ToggleTaskCompleted()
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      this._controller?.ToggleItemCompleted(dataContext);
    }

    public void Delete() => this._controller?.DeleteTask();

    public bool ParsingDate() => false;

    public bool IsNewAdd() => false;

    public void PinOrUnpinTask()
    {
    }

    public void OpenSticky()
    {
    }

    public TaskListView GetParentList() => this._controller?.GetTaskListView();

    private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this._checkIcon.IsMouseOver)
        return;
      ListItemTagsControl tagPanel = this._tagPanel;
      // ISSUE: explicit non-virtual call
      if ((tagPanel != null ? (__nonvirtual (tagPanel.IsMouseOver) ? 1 : 0) : 0) != 0 || !(this.DataContext is DisplayItemModel dataContext))
        return;
      if (dataContext.IsEvent || dataContext.IsCourse)
      {
        KanbanColumnView parent = Utils.FindParent<KanbanColumnView>((DependencyObject) this);
        if (parent != null && !parent.CanDragEvent())
          return;
      }
      if (dataContext.Enable)
        this._startPoint = e.GetPosition((IInputElement) this);
      this._previewLeftClick = true;
    }

    private void OnRightMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._previewRightClick = true;
    }

    public async Task SelectItem(bool focusTitle)
    {
      KanbanItemView kanbanItemView = this;
      if (!(kanbanItemView.DataContext is DisplayItemModel dataContext) || kanbanItemView._controller == null)
        return;
      TaskDetailPopup window = await kanbanItemView._controller.SelectKanbanItem(dataContext, focusTitle);
      await Task.Delay(100);
      window?.TryShow();
      window = (TaskDetailPopup) null;
    }

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      if (Mouse.LeftButton != MouseButtonState.Pressed)
        return;
      KanbanContainer parent = Utils.FindParent<KanbanContainer>((DependencyObject) this);
      // ISSUE: explicit non-virtual call
      if ((parent != null ? (__nonvirtual (parent.IsInDragging()) ? 1 : 0) : 0) == 0)
        return;
      e.Handled = true;
    }
  }
}
