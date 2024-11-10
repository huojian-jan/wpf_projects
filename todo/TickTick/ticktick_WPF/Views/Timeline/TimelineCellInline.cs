// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineCellInline
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineCellInline : TimelineCellBase
  {
    private Path _icon;
    private Ellipse _avatar;
    private Grid _contentPanel;
    private Border _border;
    private Rectangle _rect;
    private bool _inline = true;
    private Grid _infoTile;
    private EmjTextBlock _emjEmjText;
    private int _dragStatus;
    private System.Windows.Point? _point;
    private DoubleAnimation _leftAni;
    private DoubleAnimation _topAni;
    private bool _topAnimating;
    private TaskCircleProgress _progress;

    private async void LoadStyle()
    {
      TimelineCellInline timelineCellInline1 = this;
      timelineCellInline1.Margin = new Thickness(0.0, 3.0, 0.0, 0.0);
      timelineCellInline1._infoTile = new Grid();
      // ISSUE: explicit non-virtual call
      __nonvirtual (timelineCellInline1.Content) = (object) timelineCellInline1._infoTile;
      timelineCellInline1._infoTile.Margin = new Thickness(2.0, 3.0, 2.0, 3.0);
      timelineCellInline1._infoTile.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      timelineCellInline1._infoTile.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      timelineCellInline1._infoTile.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      timelineCellInline1._infoTile.SetBinding(FrameworkElement.WidthProperty, (BindingBase) new Binding("Width"));
      timelineCellInline1._border = new Border();
      timelineCellInline1._border.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineInlineBorder");
      timelineCellInline1._rect = new Rectangle();
      timelineCellInline1._rect.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineItemRectStyle");
      timelineCellInline1._border.Child = (UIElement) timelineCellInline1._rect;
      timelineCellInline1._infoTile.Children.Add((UIElement) timelineCellInline1._border);
      timelineCellInline1._border.MouseEnter += new MouseEventHandler(timelineCellInline1.OnMouseEnter);
      timelineCellInline1._border.MouseLeave += new MouseEventHandler(timelineCellInline1.OnMouseLeave);
      timelineCellInline1._contentPanel = new Grid();
      timelineCellInline1._contentPanel.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      timelineCellInline1._contentPanel.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      timelineCellInline1._contentPanel.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      timelineCellInline1._contentPanel.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineItemStackPanelStyle");
      timelineCellInline1._infoTile.Children.Add((UIElement) timelineCellInline1._contentPanel);
      TimelineCellInline timelineCellInline2 = timelineCellInline1;
      EmjTextBlock emjTextBlock = new EmjTextBlock();
      emjTextBlock.IsHitTestVisible = false;
      timelineCellInline2._emjEmjText = emjTextBlock;
      timelineCellInline1._emjEmjText.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineItemEmjTextStyle");
      timelineCellInline1._contentPanel.Children.Add((UIElement) timelineCellInline1._emjEmjText);
      timelineCellInline1._contentPanel.MouseLeftButtonDown += new MouseButtonEventHandler(timelineCellInline1.OnTextMouseDown);
      timelineCellInline1._contentPanel.MouseLeftButtonUp += new MouseButtonEventHandler(timelineCellInline1.OnTextMouseUp);
      timelineCellInline1._contentPanel.MouseRightButtonUp += new MouseButtonEventHandler(timelineCellInline1.OnRightClick);
      timelineCellInline1._contentPanel.MouseEnter += new MouseEventHandler(timelineCellInline1.OnTextMouseEnter);
      timelineCellInline1._contentPanel.MouseLeave += new MouseEventHandler(timelineCellInline1.OnTextMouseLeave);
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      TimelineCellViewModel cellModel = this.cellModel;
      if (cellModel == null)
        return;
      if (cellModel.DisplayModel.IsTaskOrNote && !cellModel.Editable)
        this.Container?.TryToastString(Utils.GetString("NoEditingPermission"));
      else if (cellModel.DisplayModel.Type == DisplayType.CheckItem)
        this.Container?.TryToastString(Utils.GetString("OptionNotSupport"));
      else
        this.Container?.ShowMenuPopup(cellModel);
    }

    public TimelineCellInline()
    {
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnTimelineCellDataContextChanged);
      this.LoadStyle();
      this.SetBinding(UIElement.VisibilityProperty, (BindingBase) new Binding("Visibility"));
      this.SetBinding(FrameworkElement.HeightProperty, (BindingBase) new Binding("Parent.OneLineHeight"));
      this.SetBinding(FrameworkElement.MaxWidthProperty, (BindingBase) new Binding("MaxWidth"));
      this.ClipToBounds = true;
    }

    private void OnCellMouseLeftDown(object sender, MouseButtonEventArgs e)
    {
      this._point = new System.Windows.Point?(e.GetPosition((IInputElement) this));
    }

    private void OnCellMouseLeftUp(object sender, MouseButtonEventArgs e)
    {
      System.Windows.Point? point = this._point;
      if (point.HasValue && point.GetValueOrDefault() == e.GetPosition((IInputElement) this) && sender is FrameworkElement)
        this.OpenDetailWindow(true);
      this._point = new System.Windows.Point?();
    }

    private async void OnTimelineCellDataContextChanged(
      object sender,
      DependencyPropertyChangedEventArgs e)
    {
      TimelineCellInline target = this;
      bool oldInline = target._border != null && (int) target._border.GetValue(Grid.ColumnProperty) == 1;
      if (e.OldValue is TimelineCellViewModel oldValue)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(target.OnModelPropertyChanged), string.Empty);
      bool newInline = oldInline;
      if (e.NewValue is TimelineCellViewModel newVm)
      {
        newInline = newVm.Inline;
        target.CheckIcon(newVm);
        target.CheckTextProgressAndAvatar(newVm);
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newVm, new EventHandler<PropertyChangedEventArgs>(target.OnModelPropertyChanged), string.Empty);
        double left = newVm.Left;
        if (!double.IsInfinity(left))
          target.SetValue(Canvas.LeftProperty, (object) left);
        if (newVm.Top >= 0.0)
        {
          // ISSUE: explicit non-virtual call
          __nonvirtual (target.BeginAnimation(Canvas.TopProperty, (AnimationTimeline) null));
          target._topAnimating = false;
          target.SetValue(Canvas.TopProperty, (object) newVm.Top);
        }
        target.SetValue(Panel.ZIndexProperty, (object) 10);
        target.SetBatchDragStyle(newVm);
        if (newVm.IsNew)
        {
          UserActCollectUtils.AddClickEvent("timeline", "task_action", "add");
          TaskDetailPopup popup = new TaskDetailPopup();
          target.SetValue(Panel.ZIndexProperty, (object) 100);
          popup.TaskSaved -= new EventHandler<string>(target.OnTaskSaved);
          popup.TaskSaved += new EventHandler<string>(target.OnTaskSaved);
          popup.Disappear -= new EventHandler<string>(target.OnNewWindowDisappear);
          popup.Disappear += new EventHandler<string>(target.OnNewWindowDisappear);
          TaskDetailViewModel taskDetailViewModel = new TaskDetailViewModel(newVm.DisplayModel)
          {
            IsNewAdd = true,
            Reminders = newVm.IsAllDay ? TimeData.GetDefaultAllDayReminders().ToArray() : TimeData.GetDefaultTimeReminders().ToArray()
          };
          await Task.Delay(50);
          popup.Show(taskDetailViewModel, string.Empty, new TaskWindowDisplayArgs((UIElement) target, 3.0, new System.Windows.Point(), false, target.ActualHeight + 3.0, 0));
          popup.FocusTitle();
          newVm.Operation = newVm.Operation.Add(TimelineCellOperation.Edit);
          target.timelineModel.Editing = true;
          popup = (TaskDetailPopup) null;
          taskDetailViewModel = (TaskDetailViewModel) null;
        }
      }
      if (oldInline != newInline && target._border != null)
      {
        target._border.SetValue(Grid.ColumnProperty, (object) (newInline ? 1 : 0));
        target._border.SetValue(Grid.ColumnSpanProperty, (object) (newInline ? 2 : 1));
        target._infoTile.SetBinding(FrameworkElement.WidthProperty, newInline ? (BindingBase) new Binding("Width") : (BindingBase) new Binding());
      }
      target._inline = newInline;
      newVm = (TimelineCellViewModel) null;
    }

    private void CheckTextProgressAndAvatar(TimelineCellViewModel model)
    {
      if (model == null)
        return;
      int num1 = model.Inline ? 40 : 50;
      int num2 = model.Inline ? 70 : 80;
      double num3 = model.Inline ? 14.0 : this.timelineModel.OneDayWidth + 2.0;
      double num4 = model.Inline ? model.Width : model.MaxWidth;
      model.CheckAvatarUrl();
      bool showAvatar = model.AvatarUrl != "-1" && !string.IsNullOrEmpty(model.AvatarUrl);
      bool showProgress = model.Progress > 0 && model.Status == 0;
      string title = model.Title;
      if ((title != null ? (title.Length < 10 ? 1 : 0) : 0) != 0)
      {
        double width = Utils.MeasureString(model.Title, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0).Width;
        if (showProgress && num4 <= (double) num1)
          showProgress = num4 - num3 - width >= 16.0;
        if (showAvatar && num4 <= (double) num2)
          showAvatar = num4 - num3 - width - (showProgress ? 16.0 : 0.0) >= 22.0;
      }
      else
      {
        showProgress = showProgress && num4 > (double) num1;
        showAvatar = showAvatar && num4 > (double) num2;
      }
      double num5 = num3 + (showProgress ? 16.0 : 0.0) + (showAvatar ? 22.0 : 0.0);
      if (!double.IsPositiveInfinity(num4))
      {
        num4 -= num5;
        if (num4 < 6.0)
          num4 = 0.0;
      }
      double d = !model.Inline ? Math.Min(num4, 120.0) : num4;
      this._emjEmjText.MaxWidth = double.IsNaN(d) ? 100.0 : d;
      this._emjEmjText.TextTrimming = !model.Inline ? TextTrimming.CharacterEllipsis : TextTrimming.None;
      this.CheckProgress(model, showProgress);
      this.CheckAvatar(model, showAvatar);
    }

    private void SetBatchDragStyle(TimelineCellViewModel model)
    {
      if (this._dragStatus == model.DragStatus)
        return;
      switch (model.DragStatus)
      {
        case 0:
          this.Opacity = 1.0;
          this._rect.SetBinding(Shape.FillProperty, "BackgroundBrush");
          this._rect.SetBinding(Shape.StrokeThicknessProperty, "BorderThickness");
          this._border.SetResourceReference(Control.BackgroundProperty, (object) "TimelineContainerBackground");
          this._contentPanel.Visibility = Visibility.Visible;
          if (this._avatar != null)
            this._avatar.Visibility = Visibility.Visible;
          this.SetValue(Panel.ZIndexProperty, (object) 20);
          break;
        case 1:
          this.Opacity = 1.0;
          this._rect.Fill = (Brush) ThemeUtil.GetColor("BaseColorOpacity5");
          this._rect.StrokeThickness = 0.0;
          this._contentPanel.Visibility = Visibility.Collapsed;
          if (this._avatar != null)
            this._avatar.Visibility = Visibility.Collapsed;
          this._border.Background = (Brush) Brushes.Transparent;
          this.SetValue(Panel.ZIndexProperty, (object) 0);
          break;
        case 2:
          this.Opacity = 0.0;
          break;
      }
      this._dragStatus = model.DragStatus;
    }

    private void CheckIcon(TimelineCellViewModel model)
    {
      if (model.Icon != null && this._icon == null)
      {
        this._icon = new Path();
        this._icon.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineItemIconStyle");
        this._contentPanel?.Children.Insert(0, (UIElement) this._icon);
      }
      if (model.Icon != null || this._icon == null)
        return;
      this._contentPanel?.Children.Remove((UIElement) this._icon);
      this._icon = (Path) null;
    }

    private void CheckProgress(TimelineCellViewModel model, bool showProgress)
    {
      if (showProgress && this._progress == null)
      {
        TaskCircleProgress taskCircleProgress = new TaskCircleProgress();
        taskCircleProgress.Percentage = (double) model.Progress;
        taskCircleProgress.Radius = 4.5;
        taskCircleProgress.StrokeThickness = 1.0;
        taskCircleProgress.HorizontalAlignment = HorizontalAlignment.Right;
        taskCircleProgress.VerticalAlignment = VerticalAlignment.Center;
        taskCircleProgress.Margin = new Thickness(2.0, 0.0, -2.0, -1.0);
        taskCircleProgress.IsHitTestVisible = false;
        taskCircleProgress.Opacity = 0.4;
        this._progress = taskCircleProgress;
        this._progress.SetValue(Grid.ColumnProperty, (object) 2);
        this._progress.SetBinding(TaskCircleProgress.PercentageProperty, "Progress");
        this._progress.SetBinding(TaskCircleProgress.SegmentColorProperty, "ForegroundBrush");
        this._contentPanel?.Children.Add((UIElement) this._progress);
      }
      if (showProgress || this._progress == null)
        return;
      this._contentPanel?.Children.Remove((UIElement) this._progress);
      this._progress = (TaskCircleProgress) null;
    }

    private void CheckAvatar(TimelineCellViewModel model, bool showAvatar)
    {
      if (showAvatar && this._avatar == null)
      {
        this._avatar = new Ellipse();
        this._avatar.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineItemAvatarStyle");
        this._infoTile.Children.Add((UIElement) this._avatar);
      }
      if (showAvatar || this._avatar == null)
        return;
      this._infoTile.Children.Remove((UIElement) this._avatar);
      this._avatar = (Ellipse) null;
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender != this.cellModel)
        return;
      TimelineCellViewModel cellModel = this.cellModel;
      string propertyName = e.PropertyName;
      if (propertyName == null)
        return;
      switch (propertyName.Length)
      {
        case 3:
          if (!(propertyName == "Top"))
            return;
          double top = cellModel.Top;
          if (this._topAnimating && Math.Abs(((double?) this._topAni?.To).GetValueOrDefault() - top) < 0.1)
            return;
          double num = (double) this.GetValue(Canvas.TopProperty);
          if (!cellModel.BatchSelected && top >= 0.0 && num >= 0.0 && Math.Abs(top - num) > 0.1)
          {
            if (this._topAni == null)
            {
              DoubleAnimation doubleAnimation = new DoubleAnimation();
              doubleAnimation.Duration = (Duration) TimeSpan.FromMilliseconds(200.0);
              CubicEase cubicEase = new CubicEase();
              cubicEase.EasingMode = EasingMode.EaseOut;
              doubleAnimation.EasingFunction = (IEasingFunction) cubicEase;
              this._topAni = doubleAnimation;
              this._topAni.Completed += (EventHandler) ((o, args) => this._topAnimating = false);
            }
            this._topAni.From = new double?(num);
            this._topAni.To = new double?(top);
            this._topAnimating = true;
            this.BeginAnimation(Canvas.TopProperty, (AnimationTimeline) this._topAni);
            return;
          }
          if (cellModel.Top < 0.0)
            return;
          this.BeginAnimation(Canvas.TopProperty, (AnimationTimeline) null);
          this.SetValue(Canvas.TopProperty, (object) cellModel.Top);
          return;
        case 4:
          switch (propertyName[0])
          {
            case 'I':
              if (!(propertyName == "Icon"))
                return;
              this.CheckIcon(cellModel);
              return;
            case 'L':
              if (!(propertyName == "Left") || double.IsInfinity(cellModel.Left))
                return;
              this.SetValue(Canvas.LeftProperty, (object) cellModel.Left);
              return;
            default:
              return;
          }
        case 5:
          if (!(propertyName == "Width"))
            return;
          break;
        case 6:
          if (!(propertyName == "Inline"))
            return;
          this._border.SetValue(Grid.ColumnProperty, (object) (cellModel.Inline ? 1 : 0));
          this._border.SetValue(Grid.ColumnSpanProperty, (object) (cellModel.Inline ? 2 : 1));
          this._infoTile.SetBinding(FrameworkElement.WidthProperty, cellModel.Inline ? (BindingBase) new Binding("Width") : (BindingBase) new Binding());
          this._inline = cellModel.Inline;
          this.CheckTextProgressAndAvatar(cellModel);
          return;
        case 7:
          return;
        case 8:
          switch (propertyName[0])
          {
            case 'M':
              if (!(propertyName == "MaxWidth"))
                return;
              break;
            case 'P':
              if (!(propertyName == "Progress"))
                return;
              this.CheckTextProgressAndAvatar(cellModel);
              return;
            default:
              return;
          }
          break;
        case 9:
          if (!(propertyName == "AvatarUrl"))
            return;
          this.CheckTextProgressAndAvatar(cellModel);
          return;
        case 10:
          if (!(propertyName == "DragStatus"))
            return;
          this.SetBatchDragStyle(cellModel);
          return;
        case 11:
          return;
        case 12:
          return;
        case 13:
          if (!(propertyName == "BatchSelected"))
            return;
          this.SetValue(Panel.ZIndexProperty, (object) (cellModel.BatchSelected ? 20 : 10));
          return;
        default:
          return;
      }
      this.CheckTextProgressAndAvatar(cellModel);
    }

    private async void OnNewWindowDisappear(object sender, string e)
    {
      TimelineCellInline timelineCellInline = this;
      if (sender is TaskDetailWindow taskDetailWindow)
      {
        taskDetailWindow.Disappear -= new EventHandler<string>(timelineCellInline.OnNewWindowDisappear);
        taskDetailWindow.TaskSaved -= new EventHandler<string>(timelineCellInline.OnTaskSaved);
      }
      timelineCellInline.SetValue(Panel.ZIndexProperty, (object) 0);
      TimelineCellViewModel model = timelineCellInline.cellModel;
      if (timelineCellInline.ModelInvalid())
      {
        timelineCellInline.timelineModel.Editing = false;
        model = (TimelineCellViewModel) null;
      }
      else
      {
        model.Operation = model.Operation.Remove(TimelineCellOperation.Edit);
        if (model.IsNew)
        {
          timelineCellInline.timelineModel.RemoveCell(model);
          TaskCache.DeleteTask(model.Id);
        }
        else
        {
          model.ResetBaseVm(TaskCache.GetTaskById(model.Id));
          timelineCellInline.timelineModel.UpdateCellTime(model);
          if (timelineCellInline.timelineModel.GroupByEnum != Constants.SortType.sortOrder)
            await timelineCellInline.timelineModel.UpdateGroupAsync();
          await timelineCellInline.timelineModel.UpdateCellLineAsync();
          if (!timelineCellInline.timelineModel.CheckTaskMatched(model.Id))
            timelineCellInline.Container?.TryToastString(Utils.GetString("TaskHasBeenFiltered"));
        }
        timelineCellInline.timelineModel.Editing = false;
        model = (TimelineCellViewModel) null;
      }
    }

    private void OnTaskSaved(object sender, string e)
    {
      if (this.cellModel == null)
        return;
      this.cellModel.IsNew = false;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e) => this.OnCellMouseLeave(sender, e);

    private void OnMouseEnter(object sender, MouseEventArgs e) => this.OnCellMouseEnter(sender, e);

    private void OnTextMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this._inline)
        return;
      e.Handled = true;
    }

    private async void OnTextMouseUp(object sender, MouseButtonEventArgs e)
    {
      TimelineCellInline timelineCellInline = this;
      TimelineCellViewModel cellModel = timelineCellInline.cellModel;
      if (timelineCellInline._inline || cellModel == null)
        return;
      e.Handled = true;
      if (Utils.IfCtrlPressed())
        timelineCellInline.timelineModel.ChangeBatchSelectStatus(cellModel);
      else
        await timelineCellInline.OpenDetailWindow();
    }

    private void OnTextMouseLeave(object sender, MouseEventArgs e)
    {
      if (this._inline)
        return;
      this.Container?.TryCloseToolTips(true);
    }

    private void OnTextMouseEnter(object sender, MouseEventArgs e)
    {
      if (this._inline)
        return;
      FrameworkElement ui = sender as FrameworkElement;
      if (ui == null)
        return;
      Task.Delay(200).ContinueWith((Action<Task>) (t => ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        if (!ui.IsMouseOver)
          return;
        this.Container?.OpenToolTips((FrameworkElement) this);
      }))));
    }

    public void Hide() => this.Visibility = Visibility.Collapsed;

    public void ResetVisibility()
    {
      this.SetBinding(UIElement.VisibilityProperty, (BindingBase) new Binding("Visibility"));
    }

    public void SetModel(TimelineCellViewModel model)
    {
      if (this._emjEmjText != null)
        this._emjEmjText.Text = model.Title;
      this.Visibility = Visibility.Visible;
    }

    public void ShowDetail()
    {
      if (this.cellModel == null)
        return;
      this.ShowTaskDetail(this.cellModel, false);
    }
  }
}
