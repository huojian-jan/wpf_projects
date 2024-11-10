// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoReminderWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Remind;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoReminderWindow : MyWindow, IRemindPop, IComponentConnector
  {
    private int _currentIndex;
    internal Border ContainerBorder;
    internal Image WindowIcon;
    internal TextBlock TitleText;
    internal Grid OperationPanel;
    internal Button PositiveButton;
    internal Button CloseButton;
    private bool _contentLoaded;

    public PomoReminderWindow()
    {
      this.InitializeComponent();
      this.WindowIcon.Source = (ImageSource) AppIconUtils.GetIconImage();
      this.Left = SystemParameters.WorkArea.Width - 324.0;
      this.Top = SystemParameters.WorkArea.Height - 180.0;
    }

    public bool IsRelax
    {
      set
      {
        if (!value)
        {
          this.TitleText.SetResourceReference(TextBlock.TextProperty, (object) "TimeToFocus");
          this.PositiveButton.SetResourceReference(ContentControl.ContentProperty, (object) "StartFocus");
        }
        else
        {
          this.TitleText.SetResourceReference(TextBlock.TextProperty, (object) "TimeToRelax");
          this.PositiveButton.SetResourceReference(ContentControl.ContentProperty, (object) "Relax");
        }
        if (!(this.PositiveButton.Content is string content))
          return;
        UtilLog.Info("RelaxReminderText: " + this.TitleText.Text + " Button :" + content);
      }
    }

    public bool IsAutomatic
    {
      set
      {
        this.OperationPanel.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        if (!value)
          return;
        this.DelayCloseWindow();
      }
    }

    private async void DelayCloseWindow()
    {
      await Task.Delay(3000);
      this.TryClose();
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
      FocusTimer.Reset();
      this.Visibility = Visibility.Collapsed;
    }

    private void OnStartClick(object sender, RoutedEventArgs e)
    {
      if (TickFocusManager.Status == PomoStatus.WaitingWork)
        UserActCollectUtils.AddClickEvent("focus", "start_from", "reminder");
      if (TickFocusManager.Status == PomoStatus.WaitingWork || TickFocusManager.Status == PomoStatus.WaitingRelax)
        FocusTimer.BeginTimer();
      this.Visibility = Visibility.Collapsed;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => this.TryClose();

    public void SetIcon() => this.WindowIcon.Source = (ImageSource) AppIconUtils.GetIconImage();

    public bool IsSameReminder(ReminderModel reminder) => false;

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Mouse.LeftButton != MouseButtonState.Pressed)
        return;
      this.LocationChanged -= new EventHandler(this.OnLocationChanged);
      this.LocationChanged += new EventHandler(this.OnLocationChanged);
      this.DragMove();
    }

    private void OnLocationChanged(object sender, EventArgs e)
    {
      ReminderWindowManager.OnWindowMoved((IRemindPop) this);
    }

    public void SetDisplayStyle(int index)
    {
      bool flag = true;
      if (this._currentIndex < 0 && index == 2)
      {
        this._currentIndex = index - 1;
        flag = false;
      }
      int num1 = this._currentIndex < 0 ? (index > 0 ? index + 1 : index) : this._currentIndex;
      if (this.ContainerBorder.RenderTransform is ScaleTransform renderTransform)
      {
        if (flag)
        {
          DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(1.0 - 0.05 * (double) num1), 1.0 - 0.05 * (double) index, 180);
          renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) doubleAnimation);
          renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) doubleAnimation);
        }
        else
        {
          renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) null);
          renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) null);
          renderTransform.ScaleX = 1.0 - 0.05 * (double) index;
          renderTransform.ScaleY = 1.0 - 0.05 * (double) index;
        }
      }
      double num2 = SystemParameters.WorkArea.Height - 180.0;
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(flag ? num2 - (double) (8 * num1) : num2 - (double) (8 * index) + 2.0), num2 - (double) (8 * index), 180);
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) doubleAnimation1);
      this._currentIndex = index;
      this.Opacity = 1.0;
    }

    public void TryHide()
    {
      this._currentIndex = -1;
      this.Opacity = 0.0;
      if (!(this.ContainerBorder.RenderTransform is ScaleTransform renderTransform))
        return;
      renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) null);
      renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) null);
      renderTransform.ScaleX = 0.9;
      renderTransform.ScaleY = 0.9;
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) null);
      this.Top = SystemParameters.WorkArea.Height - 196.0;
    }

    public void TryClose() => this.Close();

    public void ShowWindow()
    {
      IntPtr handle = new WindowInteropHelper((Window) this).Handle;
      this.Opacity = 1.0;
      this.Visibility = Visibility.Visible;
      if (handle != IntPtr.Zero)
      {
        this.Topmost = false;
        this.Topmost = true;
        NativeUtils.ShowWindow(handle, 4);
      }
      else
        this.Show();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/pomoreminderwindow.xaml", UriKind.Relative));
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
          this.ContainerBorder = (Border) target;
          break;
        case 2:
          this.WindowIcon = (Image) target;
          break;
        case 3:
          ((UIElement) target).PreviewMouseDown += new MouseButtonEventHandler(this.OnPreviewMouseDown);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
          break;
        case 5:
          this.TitleText = (TextBlock) target;
          break;
        case 6:
          this.OperationPanel = (Grid) target;
          break;
        case 7:
          this.PositiveButton = (Button) target;
          this.PositiveButton.Click += new RoutedEventHandler(this.OnStartClick);
          break;
        case 8:
          this.CloseButton = (Button) target;
          this.CloseButton.Click += new RoutedEventHandler(this.OnExitClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
