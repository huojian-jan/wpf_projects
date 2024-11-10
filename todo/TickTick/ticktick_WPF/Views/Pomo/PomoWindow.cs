// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Lock;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo.MiniFocus;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoWindow : UserControl, IMiniFocus, IComponentConnector
  {
    private bool _popupOpened;
    private PomoWindow.DragSizeMode _sizeMode;
    private double _width = 194.0;
    internal PomoWindow Root;
    internal Grid Container;
    internal Border BottomBorder;
    internal Grid MainContainer;
    internal ContentControl MorePopupTarget;
    internal StackPanel OperationPanel;
    internal Border ExpandBtn;
    internal Path ExpandIcon;
    internal Border MoreIcon;
    internal Border CloseBorder;
    internal Path CloseIcon;
    internal ClockControl TimePanel;
    internal StackPanel GotPomoPanel;
    internal Image GotPomo;
    internal TextBlock GotPomoText;
    internal Popup ModifySpanPopup;
    internal DockPanel FilterPanel;
    internal EmjTextBlock FocusTitle;
    internal Path TitlePath;
    internal Popup FilterPopup;
    internal Grid OptionGrid;
    internal PomoProgressBar ProgressBar;
    internal Grid OpStart;
    internal Path LeftOption;
    internal Border OptionSplite;
    internal Border OpStop;
    internal Line SplitLine;
    internal MiniFocusStatisticsView StatisticsView;
    internal Border SizeChangeBorder;
    private bool _contentLoaded;

    private FocusConfig _config => TickFocusManager.Config;

    private PomoStatus Status => this._config.Status;

    private bool _isPomo => this._config.Type == 0;

    private PomoFilterControl FocusTaskFilter { get; set; }

    public PomoWindow()
    {
      this.InitializeComponent();
      this.Unloaded += (RoutedEventHandler) ((o, e) =>
      {
        this.MouseLeave -= new MouseEventHandler(this.OnMouseLeave);
        DelayActionHandlerCenter.RemoveAction("HidePomoWindowOption");
        if (TickFocusManager.SaveAfterClose)
          LocalSettings.Settings.PomoLocalSetting.OpenWidget = false;
        UtilLog.Info("ClosePomoWindow openWidget : " + LocalSettings.Settings.PomoLocalSetting.OpenWidget.ToString());
      });
      this.SetStatisticsVisible();
      this.SetWidth(LocalSettings.Settings.PomoLocalSetting.MiniWindowWidth);
    }

    private void InitTaskFocusModel(FocusViewModel model = null)
    {
      this.SetTitle();
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) model, new EventHandler<PropertyChangedEventArgs>(this.OnModelTitleChanged), "Title");
    }

    private void SetTitle()
    {
      if (this.Status == PomoStatus.Relaxing)
      {
        this.FocusTitle.Text = Utils.GetString("Relaxing");
        this.TitlePath.Visibility = Visibility.Collapsed;
        this.FocusTitle.SetResourceReference(TextBlock.ForegroundProperty, (object) "PomoGreen");
      }
      else
      {
        this.FocusTitle.Text = string.IsNullOrEmpty(this._config.FocusVModel.Title) ? Utils.GetString("Focus") : this._config.FocusVModel.Title;
        this.FocusTitle.SetResourceReference(TextBlock.ForegroundProperty, string.IsNullOrEmpty(this._config.FocusVModel.Title) ? (object) "BaseColorOpacity60" : (object) "BaseColorOpacity100");
        this.TitlePath.Visibility = Visibility.Visible;
      }
    }

    private void OnModelTitleChanged(object sender, PropertyChangedEventArgs e) => this.SetTitle();

    private void InitPomo()
    {
      this.OnStatusChanged();
      this.TimePanel.ShowHour = !TickFocusManager.IsPomo;
      this.ProgressBar.IsStrokeMode = TickFocusManager.IsPomo;
      this.SetCountText();
    }

    public double GetLeftMargin() => 10.0;

    public double GetActualWidth() => this._width;

    public double GetExtraWidth() => 0.0;

    public double GetActualHeight()
    {
      return (double) ((LocalSettings.Settings.PomoExpand ? 55 : 0) + 72) * this._width / 194.0;
    }

    public void SetMoving(bool b)
    {
    }

    public bool CanDragMove()
    {
      return !this._popupOpened && !this.StatisticsView.TitleMouseOver && !this.SizeChangeBorder.IsMouseOver;
    }

    public bool CanHide() => !this._popupOpened && !this.StatisticsView.PopupOpened;

    public void SetHideStyle(bool isHide)
    {
    }

    public void OnWindowStartHide()
    {
    }

    public void OnFocusTypeChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        this.ProgressBar.IsStrokeMode = TickFocusManager.IsPomo;
        this.ProgressBar.Reset();
        this.TimePanel.ShowHour = !TickFocusManager.IsPomo;
        this.SetCountText();
        this.StatisticsView.SetTitleEnable();
        this.StatisticsView.SetData();
      }));
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (Math.Abs(this.OperationPanel.Opacity - 1.0) <= 0.1)
        return;
      DelayActionHandlerCenter.TryDoAction("ShowPomoWindowOption", (EventHandler) ((o, args) => this.Dispatcher.Invoke(new Action(this.ShowOptionStory))));
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (this._popupOpened)
        return;
      this.HideOptionStory();
    }

    private void ShowOptionStory()
    {
      this.OperationPanel.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(), 1.0, 90));
    }

    private void HideOptionStory()
    {
      DelayActionHandlerCenter.RemoveAction("ShowPomoWindowOption");
      this.OperationPanel.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 90));
    }

    public void OnStatusChanged()
    {
      this.Dispatcher?.Invoke((Action) (() =>
      {
        this.SetTimePanel();
        this.SetOptionStatus();
      }));
    }

    private void SetOptionStatus()
    {
      bool flag1 = this.Status == PomoStatus.Relaxing;
      bool flag2 = this.Status == PomoStatus.WaitingRelax;
      bool flag3 = this.Status == PomoStatus.Pause;
      this.OpStart.Visibility = flag1 ? Visibility.Collapsed : Visibility.Visible;
      this.OpStop.Visibility = flag2 | flag1 | flag3 ? Visibility.Visible : Visibility.Collapsed;
      this.OptionSplite.Visibility = flag3 | flag2 ? Visibility.Visible : Visibility.Collapsed;
      this.TimePanel.Visibility = this.Status == PomoStatus.WaitingRelax ? Visibility.Collapsed : Visibility.Visible;
      double num = (flag3 | flag2 ? 16.0 : 20.0) * this._width / 194.0;
      this.LeftOption.Width = num;
      this.LeftOption.Height = num;
      this.LeftOption.Data = Utils.GetIcon(this.Status == PomoStatus.Working ? "IcPomoPause" : "IcPomoStart");
      this.ProgressBar.SetResourceReference(PomoProgressBar.TopColorProperty, flag1 | flag2 ? (object) "PomoGreen" : (object) "PrimaryColor");
      this.LeftOption.SetResourceReference(Shape.FillProperty, flag1 | flag2 ? (object) "PomoGreen" : (object) "PrimaryColor");
      this.FilterPanel.Cursor = flag1 ? Cursors.Arrow : Cursors.Hand;
      this.SetTitle();
      switch (this.Status)
      {
        case PomoStatus.Working:
          this.OpStart.ToolTip = (object) Utils.GetString("Pause");
          break;
        case PomoStatus.Relaxing:
          this.OpStop.ToolTip = (object) Utils.GetString("SkipRelax");
          break;
        case PomoStatus.WaitingWork:
          this.ProgressBar.Reset();
          this.OpStart.ToolTip = (object) Utils.GetString(this._isPomo ? "StartPomo" : "StartTiming");
          break;
        case PomoStatus.WaitingRelax:
          this.OpStart.ToolTip = (object) Utils.GetString("BeginRelax");
          this.OpStop.ToolTip = (object) Utils.GetString("SkipRelax");
          break;
        case PomoStatus.Pause:
          this.OpStart.ToolTip = (object) Utils.GetString("Continue");
          this.OpStop.ToolTip = (object) Utils.GetString("End");
          break;
      }
    }

    private void SetTimePanel()
    {
      if (this._isPomo && this.Status == PomoStatus.WaitingWork)
      {
        this.TimePanel.ToolTip = (object) Utils.GetString("ClickToEdit");
        this.TimePanel.Cursor = Cursors.Hand;
      }
      else
      {
        this.TimePanel.ToolTip = (object) null;
        this.TimePanel.Cursor = Cursors.Arrow;
      }
      this.TimePanel.SetResourceReference(ClockControl.ForegroundProperty, this.Status == PomoStatus.Relaxing ? (object) "PomoGreen" : (object) "BaseColorOpacity100");
    }

    public void Init()
    {
      this.InitTaskFocusModel(this._config.FocusVModel);
      this.InitPomo();
      this.GotPomo.Source = (ImageSource) ImageUtils.GetResourceImage("pack://application:,,,/Assets/get_pomo.png", 80);
    }

    public void SetOpacity(double opacity)
    {
    }

    public void SetCountText()
    {
      int second = (int) Math.Round(this._config.CurrentSeconds, 0, MidpointRounding.AwayFromZero);
      this.ProgressBar.Angle = this._config.GetDisplayAngle(false);
      if (!this.ProgressBar.IsStrokeMode && TickFocusManager.Config.CurrentSeconds >= 30.0)
        this.ProgressBar.HideLeftMask();
      this.TimePanel.SetTime(second);
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
      Utils.FindParent<MiniFocusWindow>((DependencyObject) this)?.Close();
    }

    private void OnDropClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      FocusTimer.Drop();
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (this.ModifySpanPopup.IsOpen || this.Status != PomoStatus.WaitingWork && this.Status != PomoStatus.WaitingRelax)
            break;
          this.OnOptionButtonClick((object) null, (RoutedEventArgs) null);
          break;
        case Key.Escape:
          if (!this._popupOpened)
            break;
          this.FilterPopup.IsOpen = false;
          break;
        case Key.O:
          this.ShowFilterPopup();
          break;
      }
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e) => this.ShowOperation();

    private void OnRightClick(object sender, MouseButtonEventArgs e) => this.ShowOperation();

    private void ShowOperation()
    {
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = (UIElement) this.MorePopupTarget;
      escPopup.Placement = PlacementMode.Right;
      escPopup.HorizontalOffset = -2.0;
      escPopup.VerticalOffset = -8.0;
      escPopup.StaysOpen = false;
      this._popupOpened = true;
      escPopup.Closed += (EventHandler) ((sender, args) =>
      {
        this._popupOpened = false;
        if (this.IsMouseOver)
          return;
        this.HideOptionStory();
      });
      escPopup.Child = (UIElement) new MiniFocusMoreControl((Popup) escPopup, Window.GetWindow((DependencyObject) this), FocusWindowType.Normal);
      DelayActionHandlerCenter.RemoveAction("HidePomoWindowOption");
      escPopup.IsOpen = true;
    }

    private async void OnTimeAreaClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._isPomo || this.Status != PomoStatus.WaitingWork && this.Status != PomoStatus.WaitingRelax)
        return;
      this._popupOpened = true;
      this.ModifySpanPopup.Child = (UIElement) new PomoModitySpanConfig(this.ModifySpanPopup);
      this.ModifySpanPopup.IsOpen = true;
    }

    private void OnOptionButtonClick(object sender, RoutedEventArgs e)
    {
      switch (this.Status)
      {
        case PomoStatus.Working:
          FocusTimer.Pause(DateTime.Now);
          break;
        case PomoStatus.Relaxing:
          FocusOptionUploader.AddOption(FocusOption.endBreak, DateTime.Now, true);
          break;
        case PomoStatus.WaitingWork:
          FocusTimer.BeginTimer();
          break;
        case PomoStatus.WaitingRelax:
          FocusTimer.BeginTimer();
          break;
        case PomoStatus.Pause:
          FocusTimer.Continue(new DateTime?(DateTime.Now));
          break;
      }
    }

    private async void OnFilterClick(object sender, MouseButtonEventArgs e)
    {
      PomoWindow pomoWindow = this;
      if (pomoWindow.Status == PomoStatus.Relaxing)
        return;
      if (await AppLockCache.GetAppLocked())
        AppUnlockWindow.TryUnlockApp(new Action(pomoWindow.ShowFilterPopup));
      else
        pomoWindow.ShowFilterPopup();
    }

    private async void ShowFilterPopup()
    {
      PomoWindow pomoWindow1 = this;
      await Task.Delay(100);
      pomoWindow1._popupOpened = true;
      if (pomoWindow1.FocusTaskFilter == null)
      {
        PomoWindow pomoWindow2 = pomoWindow1;
        PomoFilterControl pomoFilterControl = new PomoFilterControl(pomoWindow1.FilterPopup, showDetail: true);
        pomoFilterControl.DataContext = (object) TickFocusManager.Config.FocusVModel;
        pomoWindow2.FocusTaskFilter = pomoFilterControl;
      }
      pomoWindow1.FilterPopup.IsOpen = true;
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      this._popupOpened = false;
      if (this.IsMouseOver)
        return;
      this.OperationPanel.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 120));
    }

    private void OnExpandClick(object sender, MouseButtonEventArgs e)
    {
      LocalSettings.Settings.PomoExpand = !LocalSettings.Settings.PomoExpand;
      this.SetStatisticsVisible();
      Utils.FindParent<MiniFocusWindow>((DependencyObject) this)?.SetSideDisplay();
    }

    private void SetStatisticsVisible()
    {
      this.StatisticsView.Visibility = !LocalSettings.Settings.PomoExpand ? Visibility.Collapsed : Visibility.Visible;
      this.ExpandIcon.Data = Utils.GetIcon(LocalSettings.Settings.PomoExpand ? "IcCollapse" : "IcExpand");
      this.ExpandBtn.ToolTip = (object) Utils.GetString(LocalSettings.Settings.PomoExpand ? "Collapse" : "Expand");
    }

    public void ResetStatistics() => this.StatisticsView.SetData();

    private void DoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (this.FilterPanel.IsMouseOver || this.OptionGrid.IsMouseOver || this.OperationPanel.IsMouseOver || this.StatisticsView.IsMouseOver)
        return;
      LocalSettings.Settings.PomoExpand = !LocalSettings.Settings.PomoExpand;
      this.SetStatisticsVisible();
    }

    private void SetWidth(double width)
    {
      if (width < 194.0)
        return;
      this._width = width;
      LocalSettings.Settings.PomoLocalSetting.MiniWindowWidth = this._width;
      double rate = this._width / 194.0;
      double textRate = 1.0 + (rate - 1.0) * 0.87;
      this.BottomBorder.CornerRadius = new CornerRadius(10.0 * rate);
      this.MainContainer.Width = 194.0 * rate;
      this.MainContainer.Height = 72.0 * rate;
      this.OperationPanel.Height = 10.0 * textRate;
      this.OperationPanel.Margin = new Thickness(0.0, 6.0 * textRate, 0.0, 0.0);
      this.ExpandBtn.Width = 14.0 * textRate;
      this.ExpandBtn.Margin = new Thickness(0.0, 0.0, 5.0 * textRate, 0.0);
      this.ExpandIcon.Height = 10.0 * textRate;
      this.ExpandIcon.Width = 10.0 * textRate;
      this.CloseBorder.Margin = new Thickness(5.0 * textRate, 0.0, 6.0 * textRate, 0.0);
      this.CloseIcon.Height = 8.0 * textRate;
      this.CloseIcon.Width = 10.0 * textRate;
      this.TimePanel.Height = 24.0 * rate;
      this.TimePanel.Margin = new Thickness(73.0 * rate, 34.0 * rate, 4.0 * rate, 0.0);
      this.TimePanel.FontSize = 19.0 * rate;
      this.GotPomoPanel.Margin = new Thickness(35.0 * rate, 0.0, 0.0, 0.0);
      this.GotPomo.Width = 42.0 * rate;
      this.GotPomo.Height = 42.0 * rate;
      this.GotPomoText.FontSize = 18.0 * rate;
      this.FilterPanel.Margin = new Thickness(74.0 * rate, 17.0 * rate, 8.0 * rate, 11.0 * rate);
      this.FocusTitle.FontSize = textRate * 11.0;
      this.FocusTitle.Height = textRate * 15.0;
      this.FocusTitle.MaxWidth = 96.0 * rate;
      this.TitlePath.Width = 12.0 * textRate;
      this.TitlePath.Height = 12.0 * textRate;
      this.ProgressBar.Width = 48.0 * rate;
      this.ProgressBar.Height = 48.0 * rate;
      this.ProgressBar.StrokeThickness = 3.0 * rate;
      this.ProgressBar.Margin = new Thickness(15.0 * rate, 10.0 * rate, 10.0 * rate, 10.0 * rate);
      double num = (this.Status == PomoStatus.Pause | this.Status == PomoStatus.WaitingRelax ? 16.0 : 20.0) * this._width / 194.0;
      this.LeftOption.Width = num;
      this.LeftOption.Height = num;
      this.OptionSplite.Height = 10.0 * rate;
      this.SplitLine.Width = 188.0 * rate;
      this.StatisticsView.SetSize(rate, textRate);
    }

    private async void OnSizeChangeMouseDown(object sender, MouseButtonEventArgs e)
    {
      PomoWindow child = this;
      Window window;
      if (Utils.FindParent<MiniFocusWindow>((DependencyObject) child).AutoHide)
        window = (Window) null;
      else if (child.SizeChangeBorder.IsMouseCaptured)
      {
        window = (Window) null;
      }
      else
      {
        window = Window.GetWindow((DependencyObject) child);
        System.Windows.Point position = e.GetPosition((IInputElement) child.SizeChangeBorder);
        child._sizeMode = child.GetPointMode(position);
        child.SizeChangeBorder.CaptureMouse();
        child.SizeChangeBorder.MouseLeftButtonUp += new MouseButtonEventHandler(child.ReleaseSizeChange);
        System.Windows.Point previousPoint = child.GetMousePosition();
        while (child.SizeChangeBorder.IsMouseCaptured)
        {
          await Task.Delay(20);
          System.Windows.Point cursorPosition = NativeUtils.GetCursorPosition();
          System.Windows.Point previousPoint1 = previousPoint;
          previousPoint = cursorPosition;
          child.ChangeSize(window, previousPoint1, cursorPosition);
        }
        window = (Window) null;
      }
    }

    private System.Windows.Point GetMousePosition() => NativeUtils.GetCursorPosition();

    private void ReleaseSizeChange(object sender, MouseButtonEventArgs e)
    {
      this.SizeChangeBorder.MouseLeftButtonUp -= new MouseButtonEventHandler(this.ReleaseSizeChange);
      this.SizeChangeBorder.ReleaseMouseCapture();
    }

    private void ChangeSize(Window window, System.Windows.Point previousPoint, System.Windows.Point point)
    {
      System.Windows.Point point1 = new System.Windows.Point(point.X - previousPoint.X, point.Y - previousPoint.Y);
      if (window == null || Math.Abs(point1.X) < 2.0 && Math.Abs(point1.Y) < 2.0)
        return;
      bool flag1 = (this._sizeMode & PomoWindow.DragSizeMode.Left) == PomoWindow.DragSizeMode.Left;
      bool flag2 = (this._sizeMode & PomoWindow.DragSizeMode.Right) == PomoWindow.DragSizeMode.Right;
      bool flag3 = (this._sizeMode & PomoWindow.DragSizeMode.Top) == PomoWindow.DragSizeMode.Top;
      bool flag4 = (this._sizeMode & PomoWindow.DragSizeMode.Bottom) == PomoWindow.DragSizeMode.Bottom;
      double actualWidth = this.Container.ActualWidth;
      double actualHeight = this.Container.ActualHeight;
      int val1_1 = 194;
      int val1_2 = 291;
      double num1 = window.Left + 10.0;
      double top = window.Top;
      double num2 = num1 + actualWidth;
      double num3 = top + actualHeight;
      double num4 = flag1 ? -1.0 * point1.X : (flag2 ? point1.X : 0.0);
      double num5 = flag3 ? -1.0 * point1.Y : (flag4 ? point1.Y : 0.0);
      if (num4 != 0.0 && num5 != 0.0 && (num4 > 0.0 && num5 < 0.0 || num4 < 0.0 && num5 > 0.0))
        num5 = 0.0;
      if (flag1 && (previousPoint.X > num1 + 8.0 && point.X > num1 + 8.0 || previousPoint.X < num1 - 8.0 && point.X < num1 - 8.0) || flag2 && (previousPoint.X > num2 + 8.0 && point.X > num2 + 8.0 || previousPoint.X < num2 - 8.0 && point.X < num2 - 8.0))
        num4 = 0.0;
      if (flag3 && (previousPoint.Y > top + 8.0 && point.Y > top + 8.0 || previousPoint.Y < top - 8.0 && point.Y < top - 8.0) || flag4 && (previousPoint.Y > num3 + 8.0 && point.Y > num3 + 8.0 || previousPoint.Y < num3 - 8.0 && point.Y < num3 - 8.0))
        num5 = 0.0;
      double num6 = num5 * actualWidth / actualHeight;
      double num7 = Math.Abs(num4) > Math.Abs(num6) ? num4 : num6;
      if (num7 == 0.0)
        return;
      double val2 = actualWidth + num7;
      double width = Math.Min((double) val1_2, Math.Max((double) val1_1, val2));
      if (Math.Abs(width - actualWidth) < 1.0 && (Math.Abs(width - (double) val1_1) < 0.1 || Math.Abs(width - (double) val1_2) < 0.1))
        return;
      if (flag1)
        window.Left = num2 - width - 10.0;
      if (flag3)
        window.Top = num3 - width * actualHeight / actualWidth;
      this.SetWidth(width);
    }

    private PomoWindow.DragSizeMode GetPointMode(System.Windows.Point point)
    {
      if (Utils.FindParent<MiniFocusWindow>((DependencyObject) this).AutoHide)
      {
        this.SizeChangeBorder.Cursor = Cursors.Arrow;
        return PomoWindow.DragSizeMode.None;
      }
      bool flag1 = point.Y <= 8.0;
      bool flag2 = point.X <= 8.0;
      bool flag3 = point.Y >= this.SizeChangeBorder.ActualHeight - 8.0;
      bool flag4 = point.X >= this.SizeChangeBorder.ActualWidth - 8.0;
      if (flag1 & flag2 || flag3 & flag4)
      {
        this.SizeChangeBorder.Cursor = Cursors.SizeNWSE;
        return !flag1 ? PomoWindow.DragSizeMode.Bottom | PomoWindow.DragSizeMode.Right : PomoWindow.DragSizeMode.Top | PomoWindow.DragSizeMode.Left;
      }
      if (flag1 & flag4 || flag3 & flag2)
      {
        this.SizeChangeBorder.Cursor = Cursors.SizeNESW;
        return !flag1 ? PomoWindow.DragSizeMode.Bottom | PomoWindow.DragSizeMode.Left : PomoWindow.DragSizeMode.Top | PomoWindow.DragSizeMode.Right;
      }
      if (flag1 | flag3)
      {
        this.SizeChangeBorder.Cursor = Cursors.SizeNS;
        return !flag1 ? PomoWindow.DragSizeMode.Bottom : PomoWindow.DragSizeMode.Top;
      }
      if (!(flag4 | flag2))
        return PomoWindow.DragSizeMode.None;
      this.SizeChangeBorder.Cursor = Cursors.SizeWE;
      return !flag2 ? PomoWindow.DragSizeMode.Right : PomoWindow.DragSizeMode.Left;
    }

    private void OnBorderMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Released)
        return;
      int pointMode = (int) this.GetPointMode(e.GetPosition((IInputElement) this.SizeChangeBorder));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/pomowindow.xaml", UriKind.Relative));
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
          this.Root = (PomoWindow) target;
          this.Root.KeyDown += new KeyEventHandler(this.OnWindowKeyDown);
          this.Root.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
          this.Root.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
          this.Root.MouseDoubleClick += new MouseButtonEventHandler(this.DoubleClick);
          break;
        case 2:
          this.Container = (Grid) target;
          this.Container.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
          break;
        case 3:
          this.BottomBorder = (Border) target;
          break;
        case 4:
          this.MainContainer = (Grid) target;
          break;
        case 5:
          this.MorePopupTarget = (ContentControl) target;
          break;
        case 6:
          this.OperationPanel = (StackPanel) target;
          break;
        case 7:
          this.ExpandBtn = (Border) target;
          this.ExpandBtn.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnExpandClick);
          break;
        case 8:
          this.ExpandIcon = (Path) target;
          break;
        case 9:
          this.MoreIcon = (Border) target;
          this.MoreIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
          break;
        case 10:
          this.CloseBorder = (Border) target;
          this.CloseBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
          break;
        case 11:
          this.CloseIcon = (Path) target;
          break;
        case 12:
          this.TimePanel = (ClockControl) target;
          break;
        case 13:
          this.GotPomoPanel = (StackPanel) target;
          break;
        case 14:
          this.GotPomo = (Image) target;
          break;
        case 15:
          this.GotPomoText = (TextBlock) target;
          break;
        case 16:
          this.ModifySpanPopup = (Popup) target;
          this.ModifySpanPopup.Closed += new EventHandler(this.OnPopupClosed);
          break;
        case 17:
          this.FilterPanel = (DockPanel) target;
          this.FilterPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFilterClick);
          break;
        case 18:
          this.FocusTitle = (EmjTextBlock) target;
          this.FocusTitle.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFilterClick);
          break;
        case 19:
          this.TitlePath = (Path) target;
          break;
        case 20:
          this.FilterPopup = (Popup) target;
          this.FilterPopup.Closed += new EventHandler(this.OnPopupClosed);
          break;
        case 21:
          this.OptionGrid = (Grid) target;
          break;
        case 22:
          this.ProgressBar = (PomoProgressBar) target;
          break;
        case 23:
          this.OpStart = (Grid) target;
          this.OpStart.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOptionButtonClick);
          break;
        case 24:
          this.LeftOption = (Path) target;
          break;
        case 25:
          this.OptionSplite = (Border) target;
          break;
        case 26:
          this.OpStop = (Border) target;
          this.OpStop.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDropClick);
          break;
        case 27:
          this.SplitLine = (Line) target;
          break;
        case 28:
          this.StatisticsView = (MiniFocusStatisticsView) target;
          break;
        case 29:
          this.SizeChangeBorder = (Border) target;
          this.SizeChangeBorder.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnSizeChangeMouseDown);
          this.SizeChangeBorder.MouseMove += new MouseEventHandler(this.OnBorderMouseMove);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [Flags]
    private enum DragSizeMode
    {
      None = 0,
      Top = 1,
      Bottom = 2,
      Left = 4,
      Right = 8,
    }
  }
}
