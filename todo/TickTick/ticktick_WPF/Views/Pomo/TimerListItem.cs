// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerListItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo.FocusStatistics;
using ticktick_WPF.Views.Pomo.MainFocus;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class TimerListItem : DockPanel
  {
    private System.Windows.Point _mouseDownPoint;
    public bool InPopup;
    private Image _iconImage;
    private readonly Border _optionIcon;
    private readonly TextBlock _durationText;
    private readonly EmjTextBlock _titleText;
    private PomoProgressBar _focusProgress;

    public TimerListItem()
    {
      EmjTextBlock emjTextBlock = new EmjTextBlock();
      emjTextBlock.TextWrapping = TextWrapping.Wrap;
      emjTextBlock.ClipToBounds = true;
      emjTextBlock.Height = 20.0;
      emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      this._titleText = emjTextBlock;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.Height = 54.0;
      this.Background = (Brush) Brushes.Transparent;
      this.InitIcon();
      this.InitOptionIcon();
      this.InitDurationText();
      this.InitTitleText();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      this.MouseMove += new MouseEventHandler(this.OnMouseMove);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
      this.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
      this.Unloaded += (RoutedEventHandler) ((o, e) => TickFocusManager.CurrentSecondChanged -= new FocusChange(this.OnFocusSecondChanged));
    }

    private void InitIcon()
    {
      this._iconImage.Width = 30.0;
      this._iconImage.Height = 30.0;
      this._iconImage.SetValue(DockPanel.DockProperty, (object) Dock.Left);
      this._iconImage.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
      this.Children.Add((UIElement) this._iconImage);
      Line line = new Line();
      line.X1 = 0.0;
      line.X2 = 1.0;
      line.Stretch = Stretch.Uniform;
      line.StrokeThickness = 1.0;
      line.Margin = new Thickness(0.0, 0.0, 20.0, 0.0);
      Line element = line;
      element.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
      element.SetValue(DockPanel.DockProperty, (object) Dock.Bottom);
      this.Children.Add((UIElement) element);
    }

    private void InitOptionIcon()
    {
      this._optionIcon.SetValue(DockPanel.DockProperty, (object) Dock.Right);
      this._optionIcon.Width = 22.0;
      this._optionIcon.Height = 22.0;
      this._optionIcon.Background = (Brush) Brushes.Transparent;
      this._optionIcon.Margin = new Thickness(0.0, 0.0, 20.0, 0.0);
      this._optionIcon.Cursor = Cursors.Hand;
      this.Children.Add((UIElement) this._optionIcon);
      this._optionIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOptionIconClick);
    }

    private void InitDurationText()
    {
      this._durationText.SetValue(DockPanel.DockProperty, (object) Dock.Right);
      this._durationText.MinWidth = 30.0;
      this._durationText.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body06");
      this._durationText.TextAlignment = TextAlignment.Right;
      this._durationText.Margin = new Thickness(0.0, 0.0, 6.0, 0.0);
      this.Children.Add((UIElement) this._durationText);
      this._durationText.SetBinding(TextBlock.TextProperty, "DurationText");
    }

    private void InitTitleText()
    {
      this._titleText.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body06");
      this._titleText.Margin = new Thickness(12.0, 0.0, 8.0, 0.0);
      this._titleText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100_80");
      this._titleText.FontSize = 14.0;
      this.Children.Add((UIElement) this._titleText);
      this._titleText.SetBinding(EmjTextBlock.TextProperty, "Title");
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is FocusTimerViewModel oldValue)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.VmPropertyChanged), "");
      if (!(e.NewValue is FocusTimerViewModel newValue))
        return;
      this._iconImage.Source = (ImageSource) HabitService.GetIcon(newValue.Icon, newValue.Color);
      if (newValue.Archive)
      {
        TickFocusManager.CurrentSecondChanged -= new FocusChange(this.OnFocusSecondChanged);
        this._optionIcon.Child = (UIElement) null;
        this._optionIcon.Visibility = Visibility.Collapsed;
        this._focusProgress = (PomoProgressBar) null;
      }
      else
      {
        this._optionIcon.Visibility = Visibility.Visible;
        this.SetOptionChild(newValue.Focusing);
      }
      this.SetItemBackground(newValue.Selected);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.VmPropertyChanged), "");
      this.Opacity = !newValue.Dragging || this.InPopup ? 1.0 : 0.0;
    }

    private void VmPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DataContext is FocusTimerViewModel dataContext))
        return;
      switch (e.PropertyName)
      {
        case "Dragging":
          this.Opacity = !dataContext.Dragging || this.InPopup ? 1.0 : 0.0;
          break;
        case "Focusing":
          if (dataContext.Archive)
            break;
          this.SetOptionChild(dataContext.Focusing);
          break;
        case "Selected":
          this.SetItemBackground(dataContext.Selected);
          break;
      }
    }

    private void SetItemBackground(bool selected)
    {
      if (!selected)
      {
        if (this.IsMouseOver)
          this.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity5");
        else
          this.Background = (Brush) Brushes.Transparent;
      }
      else
        this.SetResourceReference(Panel.BackgroundProperty, (object) "ItemSelectedColor");
    }

    private void SetOptionChild(bool focusing)
    {
      if (focusing)
      {
        PomoProgressBar pomoProgressBar = new PomoProgressBar();
        pomoProgressBar.TimingSize = 2;
        pomoProgressBar.Height = 18.0;
        pomoProgressBar.Width = 18.0;
        pomoProgressBar.Angle = 0.0;
        pomoProgressBar.StrokeThickness = 3.0;
        pomoProgressBar.IsStrokeMode = TickFocusManager.IsPomo;
        pomoProgressBar.VerticalAlignment = VerticalAlignment.Center;
        pomoProgressBar.HorizontalAlignment = HorizontalAlignment.Center;
        this._focusProgress = pomoProgressBar;
        this._focusProgress.SetResourceReference(PomoProgressBar.UnderColorProperty, (object) "BaseColorOpacity10");
        this._focusProgress.SetResourceReference(PomoProgressBar.TopColorProperty, (object) "PrimaryColor");
        TickFocusManager.CurrentSecondChanged -= new FocusChange(this.OnFocusSecondChanged);
        TickFocusManager.CurrentSecondChanged += new FocusChange(this.OnFocusSecondChanged);
        this._optionIcon.Child = (UIElement) this._focusProgress;
        this._focusProgress.Angle = TickFocusManager.Config.GetDisplayAngle(false);
        if (this._focusProgress.IsStrokeMode || TickFocusManager.Config.CurrentSeconds < 30.0)
          return;
        this._focusProgress.HideLeftMask();
      }
      else
      {
        Path path1 = new Path();
        path1.Data = Utils.GetIcon("IcPomoStart");
        path1.Width = 18.0;
        path1.Height = 18.0;
        Path path2 = path1;
        path2.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor");
        path2.SetResourceReference(FrameworkElement.StyleProperty, (object) "Path01");
        this._optionIcon.Child = (UIElement) path2;
        TickFocusManager.CurrentSecondChanged -= new FocusChange(this.OnFocusSecondChanged);
        this._focusProgress = (PomoProgressBar) null;
      }
    }

    private void OnFocusSecondChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        PomoProgressBar focusProgress = this._focusProgress;
        if (focusProgress == null)
          return;
        focusProgress.Angle = TickFocusManager.Config.GetDisplayAngle(false);
        if (focusProgress.IsStrokeMode || TickFocusManager.Config.CurrentSeconds < 30.0)
          return;
        focusProgress.HideLeftMask();
      }));
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is FocusTimerViewModel dataContext))
        return;
      EscPopup escPopup1 = new EscPopup();
      escPopup1.PlacementTarget = sender as UIElement;
      escPopup1.StaysOpen = false;
      escPopup1.Placement = PlacementMode.MousePoint;
      EscPopup escPopup2 = escPopup1;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "Archive", Utils.GetString(dataContext.Archive ? "Restore" : "Archive"), Utils.GetImageSource(dataContext.Archive ? "CancelArchiveDrawingImage" : "ArchiveDrawingImage")),
        new CustomMenuItemViewModel((object) "Delete", Utils.GetString("Delete"), Utils.GetImageSource("DeleteDrawingLine"))
      };
      if (!dataContext.Archive)
      {
        types.Insert(0, new CustomMenuItemViewModel((object) "Edit", Utils.GetString("Edit"), Utils.GetImageSource("EditDrawingImage")));
        types.Insert(1, new CustomMenuItemViewModel((object) "AddRecord", Utils.GetString("AddRecord"), Utils.GetIcon("IcAdd"))
        {
          ImageMargin = new Thickness(11.0, 0.0, 2.0, 0.0)
        });
      }
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) escPopup2);
      customMenuList.Operated += new EventHandler<object>(this.OnActionSelected);
      customMenuList.Show();
    }

    private async void OnActionSelected(object sender, object e)
    {
      TimerListItem timerListItem = this;
      if (!(timerListItem.DataContext is FocusTimerViewModel model))
      {
        model = (FocusTimerViewModel) null;
      }
      else
      {
        switch (e.ToString())
        {
          case "Edit":
            TimerModel timerById = await TimerDao.GetTimerById(model.Id);
            if (timerById == null)
            {
              model = (FocusTimerViewModel) null;
              break;
            }
            AddTimerWindow addTimerWindow = new AddTimerWindow(timerById);
            addTimerWindow.Owner = Window.GetWindow((DependencyObject) timerListItem);
            addTimerWindow.ShowDialog();
            model = (FocusTimerViewModel) null;
            break;
          case "AddRecord":
            TimerModel timer = await TimerDao.GetTimerById(model.Id);
            if (timer != null)
            {
              string objTitle = await TimerService.GetObjTitle(timer.ObjId, timer.ObjType);
              if (objTitle != null && timer.Name != objTitle)
              {
                timer.Name = objTitle;
                await TimerService.UpdateName(timer.Id, objTitle);
              }
              new AddFocusRecordWindow(timer).ShowDialog();
              UserActCollectUtils.AddClickEvent("timer", "timer_detail", "add_record");
            }
            timer = (TimerModel) null;
            model = (FocusTimerViewModel) null;
            break;
          case "Archive":
            if (model.Archive)
            {
              if (!await ProChecker.CheckTimerLimit(Window.GetWindow((DependencyObject) timerListItem)))
              {
                model = (FocusTimerViewModel) null;
                break;
              }
            }
            else if (!new CustomerDialog("", Utils.GetString("ArchiveTimerMessage"), Utils.GetString("Archive"), Utils.GetString("Cancel"), Window.GetWindow((DependencyObject) timerListItem)).ShowDialog().GetValueOrDefault())
            {
              model = (FocusTimerViewModel) null;
              break;
            }
            TimerService.ChangeArchiveStatus(model.Id);
            model = (FocusTimerViewModel) null;
            break;
          case "Delete":
            if (!new CustomerDialog("", Utils.GetString("DeleteTimerMessage"), Utils.GetString("Delete"), Utils.GetString("Cancel"), Window.GetWindow((DependencyObject) timerListItem)).ShowDialog().GetValueOrDefault())
            {
              model = (FocusTimerViewModel) null;
              break;
            }
            TimerService.DeleteTimer(model.Id);
            model = (FocusTimerViewModel) null;
            break;
          default:
            model = (FocusTimerViewModel) null;
            break;
        }
      }
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is FocusTimerViewModel dataContext) || dataContext.Archive)
        return;
      this._mouseDownPoint = e.GetPosition((IInputElement) this);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && this._mouseDownPoint != new System.Windows.Point())
      {
        if ((e.GetPosition((IInputElement) this) - this._mouseDownPoint).Length <= 4.0)
          return;
        this._mouseDownPoint = new System.Windows.Point();
        Utils.FindParent<FocusTimerListView>((DependencyObject) this)?.TryDragItem(this.DataContext as FocusTimerViewModel, e);
      }
      else
        this._mouseDownPoint = new System.Windows.Point();
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is FocusTimerViewModel dataContext))
        return;
      this.SetItemBackground(dataContext.Selected);
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is FocusTimerViewModel dataContext) || dataContext.Selected)
        return;
      this.SetItemBackground(dataContext.Selected);
    }

    private async void OnOptionIconClick(object sender, MouseButtonEventArgs e)
    {
      TimerListItem child = this;
      e.Handled = true;
      if (!(child.DataContext is FocusTimerViewModel dataContext))
        return;
      if (!await TickFocusManager.TryStartFocusTimer(dataContext.Id, Window.GetWindow((DependencyObject) child)))
        return;
      Utils.FindParent<MainFocusControl>((DependencyObject) child)?.ShowClockPanel();
      UserActCollectUtils.AddClickEvent("focus", "start_from_tab", "timer_list");
      UserActCollectUtils.AddClickEvent("focus", "start_from", "tab");
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is FocusTimerViewModel dataContext))
        return;
      Utils.FindParent<FocusTimerListView>((DependencyObject) this)?.OnItemClick(dataContext);
    }
  }
}
