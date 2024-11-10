// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ImmersiveContent
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

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
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CheckList;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.DetailView;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class ImmersiveContent : UserControl, IComponentConnector
  {
    private readonly Stack<ProjectTask> _navigateStack = new Stack<ProjectTask>();
    private readonly ScaleTransform _scaleTransform = new ScaleTransform(1.0, 1.0);
    private bool _isPreparing;
    internal ImmersiveContent Root;
    internal ScrollViewer TaskDetailScrollViewer;
    internal Grid ImmersiveGrid;
    internal TaskDetailImmerseView TaskDetail;
    internal Grid EditorMenuControl;
    internal EditorMenu EditorMenu;
    private bool _contentLoaded;

    public ImmersiveContent()
    {
      this.InitializeComponent();
      this.InitEvent();
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      this.EditorMenu.EditorAction -= new EventHandler<string>(this.OnEditorAction);
      this.EditorMenu.ExitImmersive -= new EventHandler(this.OnExitClick);
      this.EditorMenu.NavigateBack -= new EventHandler(this.OnNavigateBack);
      this.TaskDetail.PreviewKeyDown -= new KeyEventHandler(this.OnDetailKeyDown);
      MarkDownEditor contentText = this.TaskDetail.GetContentText();
      if (contentText != null)
        contentText.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      this.TaskDetail.ScrollViewer.PreviewMouseWheel -= new MouseWheelEventHandler(ImmersiveContent.OnDetailScroll);
      this.TaskDetail.Dispose();
    }

    private void InitEvent()
    {
      this.EditorMenu.EditorAction -= new EventHandler<string>(this.OnEditorAction);
      this.EditorMenu.EditorAction += new EventHandler<string>(this.OnEditorAction);
      this.EditorMenu.ExitImmersive -= new EventHandler(this.OnExitClick);
      this.EditorMenu.ExitImmersive += new EventHandler(this.OnExitClick);
      this.EditorMenu.NavigateBack -= new EventHandler(this.OnNavigateBack);
      this.EditorMenu.NavigateBack += new EventHandler(this.OnNavigateBack);
      this.EditorMenu.MoreOptionGrid.Visibility = Visibility.Collapsed;
      this.TaskDetail.PreviewKeyDown -= new KeyEventHandler(this.OnDetailKeyDown);
      this.TaskDetail.PreviewKeyDown += new KeyEventHandler(this.OnDetailKeyDown);
      MarkDownEditor contentText = this.TaskDetail.GetContentText();
      if (contentText != null)
      {
        contentText.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnContentVerticalOffsetChanged);
        contentText.CaretVerticalOffsetChanged += new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      }
      this.TaskDetail.ScrollViewer.PreviewMouseWheel -= new MouseWheelEventHandler(ImmersiveContent.OnDetailScroll);
      this.AddScrollHandler();
      this.TaskDetail.NavigateTask -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      this.TaskDetail.NavigateTask += new EventHandler<ProjectTask>(this.OnNavigateTask);
      this.TaskDetail.EscKeyUp -= new EventHandler(this.OnEsc);
      this.TaskDetail.EscKeyUp += new EventHandler(this.OnEsc);
      this.TaskDetail.CheckItemsDeleted -= new EventHandler<TaskDetailItemModel>(this.OnCheckItemDeleted);
      this.TaskDetail.CheckItemsDeleted += new EventHandler<TaskDetailItemModel>(this.OnCheckItemDeleted);
    }

    private void OnCheckItemDeleted(object sender, TaskDetailItemModel model)
    {
      UndoToast uiElement = new UndoToast();
      uiElement.InitSubtaskUndo(model);
      Utils.FindParent<MainWindow>((DependencyObject) this)?.Toast((FrameworkElement) uiElement);
    }

    private void OnEsc(object sender, EventArgs e)
    {
      Utils.FindParent<IListViewParent>((DependencyObject) this)?.ExitImmersiveMode();
    }

    private void OnNavigateBack(object sender, EventArgs e)
    {
      if (this._navigateStack.Count > 0)
        this.ShowContent(this._navigateStack.Pop().TaskId);
      if (this._navigateStack.Count > 0)
        return;
      this.SetNavigateBackDisabled();
    }

    private void SetNavigateBackDisabled()
    {
      this.EditorMenu.NavigateBackBtn.IsEnabled = false;
      this.EditorMenu.NavigateBackBtn.Visibility = Visibility.Collapsed;
    }

    private async void OnNavigateTask(object sender, ProjectTask projectTask)
    {
      this._navigateStack.Push(new ProjectTask()
      {
        TaskId = this.TaskDetail.TaskId,
        ProjectId = this.TaskDetail.ProjectId
      });
      TaskModel task = await TaskUtils.TryLoadTask(projectTask.TaskId, projectTask.ProjectId);
      if (task == null)
        ;
      else
      {
        this.SetNavigateEnabled();
        this.ShowContent(projectTask.TaskId);
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == task.projectId));
        if (projectModel == null || projectModel.closed.HasValue && projectModel.closed.Value || task.deleted != 0 || task.kind == "CHECKLIST")
          this.EditorMenu.SetReadOnly(true);
        else
          this.EditorMenu.SetReadOnly(false);
      }
    }

    private void SetNavigateEnabled()
    {
      this.EditorMenu.NavigateBackBtn.IsEnabled = true;
      this.EditorMenu.NavigateBackBtn.Visibility = Visibility.Visible;
    }

    private async void ShowContent(string taskId)
    {
      Storyboard storyboard = new Storyboard();
      DoubleAnimation doubleAnimation1 = new DoubleAnimation();
      doubleAnimation1.AutoReverse = false;
      doubleAnimation1.FillBehavior = FillBehavior.HoldEnd;
      doubleAnimation1.From = new double?(1.0);
      doubleAnimation1.To = new double?(0.0);
      doubleAnimation1.Duration = (Duration) TimeSpan.FromMilliseconds(200.0);
      DoubleAnimation element1 = doubleAnimation1;
      DoubleAnimation doubleAnimation2 = new DoubleAnimation();
      doubleAnimation2.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(200.0));
      doubleAnimation2.AutoReverse = false;
      doubleAnimation2.FillBehavior = FillBehavior.HoldEnd;
      doubleAnimation2.From = new double?(0.0);
      doubleAnimation2.To = new double?(1.0);
      doubleAnimation2.Duration = (Duration) TimeSpan.FromMilliseconds(200.0);
      DoubleAnimation element2 = doubleAnimation2;
      storyboard.Children.Add((Timeline) element1);
      storyboard.Children.Add((Timeline) element2);
      Storyboard.SetTargetProperty((DependencyObject) element1, new PropertyPath((object) UIElement.OpacityProperty));
      Storyboard.SetTargetProperty((DependencyObject) element2, new PropertyPath((object) UIElement.OpacityProperty));
      storyboard.Begin((FrameworkElement) this.Root);
      await Task.Delay(200);
      this.TaskDetail.Navigate(taskId);
    }

    private void OnContentVerticalOffsetChanged(object sender, double offset)
    {
      double actualHeight = this.TaskDetail.GetTitleText().ActualHeight;
      double num1 = this.TaskDetailScrollViewer.VerticalOffset + this.TaskDetailScrollViewer.ActualHeight - actualHeight;
      double num2 = offset + 24.0 + 21.0 + 56.0;
      if (num2 > num1)
        this.TaskDetailScrollViewer.ScrollToVerticalOffset(num2 - this.TaskDetailScrollViewer.ActualHeight + actualHeight);
      double num3 = this.TaskDetailScrollViewer.VerticalOffset - actualHeight;
      if (offset + 24.0 - 21.0 < num3)
      {
        offset = offset + 24.0 - 21.0;
        this.TaskDetailScrollViewer.ScrollToVerticalOffset(offset + actualHeight - 21.0);
      }
      if (offset > 24.0)
        return;
      this.TaskDetailScrollViewer.ScrollToVerticalOffset(actualHeight - 24.0);
    }

    private void OnDetailKeyDown(object sender, KeyEventArgs e)
    {
      if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.None)
        return;
      switch (e.Key)
      {
        case Key.Up:
          this.Zoom(true);
          e.Handled = true;
          break;
        case Key.Down:
          this.Zoom(false);
          e.Handled = true;
          break;
      }
    }

    private static void OnDetailScroll(object sender, MouseWheelEventArgs e)
    {
      if (e.Handled)
        return;
      e.Handled = true;
      MouseWheelEventArgs mouseWheelEventArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
      mouseWheelEventArgs.RoutedEvent = UIElement.MouseWheelEvent;
      mouseWheelEventArgs.Source = sender;
      MouseWheelEventArgs e1 = mouseWheelEventArgs;
      if (!(((FrameworkElement) sender).Parent is UIElement parent))
        return;
      // ISSUE: explicit non-virtual call
      __nonvirtual (parent.RaiseEvent((RoutedEventArgs) e1));
    }

    private async void OnEditorAction(object sender, string action)
    {
      this.TaskDetail.SetContentStyle(action);
    }

    public async Task LoadData(string taskId)
    {
      this._isPreparing = true;
      this.ShowMenu();
      await this.TaskDetail.Navigate(taskId);
      await Task.Delay(1500);
      if (!this.EditorMenuControl.IsMouseOver)
        this.HideMenu(false);
      this._isPreparing = false;
    }

    private void OnExitClick(object sender, EventArgs eventArgs)
    {
      this._navigateStack.Clear();
      this.SetNavigateBackDisabled();
      this.ZoomNormal();
      Utils.FindParent<IListViewParent>((DependencyObject) this)?.ExitImmersiveMode();
    }

    private async void OnMenuMouseLeave(object sender, MouseEventArgs e)
    {
      if (this.EditorMenu.HeaderPopup.IsOpen)
        return;
      this.HideMenu();
    }

    private void OnMenuMouseEnter(object sender, MouseEventArgs e) => this.ShowMenu();

    private void HideMenu(bool checkPreparing = true)
    {
      if (checkPreparing && this._isPreparing)
        return;
      Storyboard storyboard = new Storyboard();
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.AutoReverse = false;
      doubleAnimation.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(500.0));
      doubleAnimation.FillBehavior = FillBehavior.HoldEnd;
      doubleAnimation.From = new double?(1.0);
      doubleAnimation.To = new double?(0.23999999463558197);
      doubleAnimation.Duration = (Duration) TimeSpan.FromMilliseconds(120.0);
      DoubleAnimation element = doubleAnimation;
      storyboard.Children.Add((Timeline) element);
      Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath((object) UIElement.OpacityProperty));
      storyboard.Begin((FrameworkElement) this.EditorMenuControl);
    }

    private void ShowMenu()
    {
      Storyboard storyboard = new Storyboard();
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.AutoReverse = false;
      doubleAnimation.FillBehavior = FillBehavior.HoldEnd;
      doubleAnimation.From = new double?(0.23999999463558197);
      doubleAnimation.To = new double?(1.0);
      doubleAnimation.Duration = (Duration) TimeSpan.FromMilliseconds(120.0);
      DoubleAnimation element = doubleAnimation;
      storyboard.Children.Add((Timeline) element);
      Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath((object) UIElement.OpacityProperty));
      storyboard.Begin((FrameworkElement) this.EditorMenuControl);
    }

    private void ZoomNormal()
    {
      this._scaleTransform.ScaleX = 1.0;
      this._scaleTransform.ScaleY = 1.0;
      this.TaskDetail.RenderTransform = (Transform) this._scaleTransform;
    }

    public void Zoom(bool zoomIn)
    {
      double scaleX = this._scaleTransform.ScaleX;
      double num1 = App.Window.ActualWidth / this.ImmersiveGrid.ActualWidth;
      this.TaskDetail.RenderTransformOrigin = new System.Windows.Point(0.5, 0.0);
      double num2 = !zoomIn ? scaleX - 0.05 : scaleX + 0.05;
      if (num2 < 0.5 || num2 > num1)
        return;
      this._scaleTransform.ScaleX = num2;
      this._scaleTransform.ScaleY = num2;
      this.TaskDetail.RenderTransform = (Transform) this._scaleTransform;
    }

    public void AddScrollHandler()
    {
      this.TaskDetail.ScrollViewer.PreviewMouseWheel -= new MouseWheelEventHandler(ImmersiveContent.OnDetailScroll);
      this.TaskDetail.ScrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(ImmersiveContent.OnDetailScroll);
    }

    public void KeepOffsetInView(object sender, double offset)
    {
      double num1 = 10.0;
      MarkDownEditor descText = this.TaskDetail.GetDescText();
      MarkDownEditor contentText = this.TaskDetail.GetContentText();
      DetailTextBox titleText = this.TaskDetail.GetTitleText();
      double num2 = titleText != null ? titleText.ActualHeight : 0.0;
      ChecklistControl checklist = this.TaskDetail.GetChecklist();
      if (object.Equals(sender, (object) contentText) || object.Equals(sender, (object) descText))
        num1 += num2;
      else if (object.Equals(sender, (object) checklist))
      {
        num1 += num2 + 18.0;
        if (descText != null)
          num1 += descText.ActualHeight;
      }
      double num3 = (double) this.FindResource((object) "Font14") - 14.0 + 2.0;
      double num4 = offset + num3 + 40.0;
      double num5 = this.TaskDetailScrollViewer.VerticalOffset + this.TaskDetailScrollViewer.ActualHeight - num1;
      double num6 = this.TaskDetailScrollViewer.VerticalOffset - num1;
      if (num5 >= num4 && offset >= num6)
        return;
      if (num4 > num5)
      {
        this.TaskDetailScrollViewer.ScrollToVerticalOffset(num4 - this.TaskDetailScrollViewer.ActualHeight + num1);
      }
      else
      {
        if (offset < num6)
          this.TaskDetailScrollViewer.ScrollToVerticalOffset(offset + num1 - 10.0);
        if (offset > 24.0 + num3)
          return;
        this.TaskDetailScrollViewer.ScrollToVerticalOffset(num1 - 24.0 - num3);
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/markdown/immersivecontent.xaml", UriKind.Relative));
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
          this.Root = (ImmersiveContent) target;
          break;
        case 2:
          this.TaskDetailScrollViewer = (ScrollViewer) target;
          break;
        case 3:
          this.ImmersiveGrid = (Grid) target;
          break;
        case 4:
          this.TaskDetail = (TaskDetailImmerseView) target;
          break;
        case 5:
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnMenuMouseEnter);
          ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnMenuMouseLeave);
          break;
        case 6:
          this.EditorMenuControl = (Grid) target;
          break;
        case 7:
          this.EditorMenu = (EditorMenu) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
