// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Eisenhower.MatrixContainer
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Widget;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Eisenhower
{
  public class MatrixContainer : UserControl, IComponentConnector
  {
    private readonly DelayActionHandler _delayLoadHandler = new DelayActionHandler(200);
    private const double QuadrantPadding = 6.0;
    private List<QuadrantControl> _quadrants;
    private Rect[] _quadrantLocations = new Rect[4];
    private System.Windows.Point _dragStartPosition;
    private System.Windows.Point _popupStartPoint;
    private QuadrantControl _dragQuadrant;
    private int _dragHoverIndex = -1;
    private int _dragStartIndex;
    private WidgetMorePopup _widgetMorePopup;
    private bool _clear;
    internal BlurEffect BlurEffect;
    internal RowDefinition HeadRow;
    internal TextBlock TitleName;
    internal Border DragPanel;
    internal HoverIconButton UnLockGrid;
    internal HoverIconButton SyncGrid;
    internal HoverIconButton MoreGrid;
    internal EscPopup MorePopup;
    internal Canvas Container;
    internal QuadrantControl Quadrant1;
    internal QuadrantControl Quadrant2;
    internal QuadrantControl Quadrant3;
    internal QuadrantControl Quadrant4;
    internal Popup DragQuadrantPopup;
    internal VisualBrush DragVisual;
    private bool _contentLoaded;

    public event EventHandler<WidgetMoreAction> MoreAction;

    public MatrixContainer()
    {
      this.InitializeComponent();
      this._delayLoadHandler.SetAction(new EventHandler(this.DelayLoad));
      this.Loaded += (RoutedEventHandler) ((o, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((o, e) => this.UnbindEvents());
      this.InitQuadrants();
    }

    public bool IsLocked { get; set; }

    public bool InOperation { get; set; }

    private void InitQuadrants()
    {
      this._quadrants = new List<QuadrantControl>()
      {
        this.Quadrant1,
        this.Quadrant2,
        this.Quadrant3,
        this.Quadrant4
      };
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.SetIdentity();
      this._quadrants.Sort((Comparison<QuadrantControl>) ((a, b) => ((long?) a.Quadrant?.sortOrder).GetValueOrDefault().CompareTo(((long?) b.Quadrant?.sortOrder).GetValueOrDefault())));
    }

    private void UnbindEvents()
    {
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
      DataChangedNotifier.TaskDefaultChanged -= new EventHandler(this.OnTaskDefaultChanged);
      DataChangedNotifier.MatrixQuadrantChanged -= new EventHandler<int>(this.OnMatrixQuadrantChanged);
      DataChangedNotifier.SyncDone -= new EventHandler<SyncResult>(this.OnSyncChanged);
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnThemeModeChanged);
      ticktick_WPF.Notifier.GlobalEventManager.QuadrantSortChanged -= new EventHandler(this.OnQuadrantSortChanged);
      this._delayLoadHandler.StopAndClear();
    }

    private void BindEvents()
    {
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
      DataChangedNotifier.MatrixQuadrantChanged += new EventHandler<int>(this.OnMatrixQuadrantChanged);
      DataChangedNotifier.TaskDefaultChanged += new EventHandler(this.OnTaskDefaultChanged);
      DataChangedNotifier.SyncDone += new EventHandler<SyncResult>(this.OnSyncChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnThemeModeChanged);
      ticktick_WPF.Notifier.GlobalEventManager.QuadrantSortChanged += new EventHandler(this.OnQuadrantSortChanged);
      this._delayLoadHandler.SetAction(new EventHandler(this.DelayLoad));
    }

    private void OnSyncChanged(object sender, SyncResult e)
    {
      if (!this.IsVisible || !e.RemoteProjectsChanged && !e.RemoteTasksChanged && !e.RemoteTagChanged)
        return;
      this.LoadTask();
    }

    private void OnQuadrantSortChanged(object sender, EventArgs e)
    {
      if (sender != null && sender.Equals((object) this))
        return;
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.SetIdentity();
      this._quadrants.Sort((Comparison<QuadrantControl>) ((a, b) => ((long?) a.Quadrant?.sortOrder).GetValueOrDefault().CompareTo(((long?) b.Quadrant?.sortOrder).GetValueOrDefault())));
      this.SetQuadrantLocation();
    }

    private void OnThemeModeChanged(object sender, EventArgs e)
    {
      if (!this.IsVisible)
        return;
      this.LoadTask();
    }

    private void OnTaskDefaultChanged(object sender, EventArgs e)
    {
      foreach (QuadrantControl quadrant in this._quadrants)
      {
        if (!quadrant.TaskList.Equals(sender))
          quadrant.SetIdentity();
      }
    }

    private void DelayLoad(object sender, EventArgs e)
    {
      Utils.RunOnBackgroundThread(this.Dispatcher, new Action(this.Load));
    }

    private void OnDayChanged(object sender, object e)
    {
      if (!this.IsVisible)
        return;
      this.LoadTask();
    }

    private void OnMatrixQuadrantChanged(object sender, int e)
    {
      Utils.RunOnBackgroundThread(this.Dispatcher, (Action) (() =>
      {
        switch (e)
        {
          case 1:
            this.Quadrant1.LoadAsync();
            break;
          case 2:
            this.Quadrant2.LoadAsync();
            break;
          case 3:
            this.Quadrant3.LoadAsync();
            break;
          case 4:
            this.Quadrant4.LoadAsync();
            break;
          default:
            foreach (QuadrantControl quadrant in this._quadrants)
              quadrant.SetIdentity();
            this._quadrants.Sort((Comparison<QuadrantControl>) ((a, b) => ((long?) a.Quadrant?.sortOrder).GetValueOrDefault().CompareTo(((long?) b.Quadrant?.sortOrder).GetValueOrDefault())));
            this.SetQuadrantLocation();
            UtilRun.WhenAllAsync(this.Quadrant1.LoadTaskAsync(), this.Quadrant2.LoadTaskAsync(), this.Quadrant3.LoadTaskAsync(), this.Quadrant4.LoadTaskAsync());
            break;
        }
      }));
    }

    private void Load() => this.LoadTask();

    public async Task LoadTask(bool restore = true)
    {
      await UtilRun.WhenAllAsync(this.Quadrant1.LoadAsync(restore), this.Quadrant2.LoadAsync(restore), this.Quadrant3.LoadAsync(restore), this.Quadrant4.LoadAsync(restore));
    }

    private void OnTaskDragOver(object sender, MouseEventArgs e)
    {
      QuadrantControl mousePointElement = Utils.GetMousePointElement<QuadrantControl>(e, (FrameworkElement) this);
      int level = 0;
      if (mousePointElement != null)
        level = sender == null || !mousePointElement.Equals(sender) ? mousePointElement.GetLevel() : -1;
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.SetTaskOver(level);
    }

    public async Task<bool> OnBatchTaskDrop(
      int previousLevel,
      List<string> taskIds,
      MouseEventArgs e)
    {
      MatrixContainer element = this;
      QuadrantControl dragQuadrant = (QuadrantControl) null;
      foreach (QuadrantControl quadrant in element._quadrants)
      {
        quadrant.OnTaskDragDrop();
        if (quadrant.Level == previousLevel)
          dragQuadrant = quadrant;
      }
      QuadrantControl dropQuadrant = Utils.GetMousePointElement<QuadrantControl>(e, (FrameworkElement) element);
      bool flag1 = false;
      if (dragQuadrant != null && dropQuadrant != null && !dragQuadrant.Equals((object) dropQuadrant))
      {
        UserActCollectUtils.AddClickEvent("matrix", "matrix_action", "drag_task");
        dragQuadrant.RemoveItems(taskIds);
        flag1 = await TaskService.BatchUpdateTaskQuadrantProperties(taskIds, dragQuadrant.GetRule(), dropQuadrant.GetRule(), element.GetToastParent());
        if (flag1)
        {
          dropQuadrant.LoadTaskAsync();
          dragQuadrant.LoadTaskAsync();
        }
      }
      bool flag2 = flag1;
      dragQuadrant = (QuadrantControl) null;
      dropQuadrant = (QuadrantControl) null;
      return flag2;
    }

    public void Toast(string toast) => this.GetToastParent()?.TryToastString((object) null, toast);

    private IToastShowWindow GetToastParent()
    {
      return Utils.FindParent<IToastShowWindow>((DependencyObject) this);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      QuadrantControl mousePointElement = Utils.GetMousePointElement<QuadrantControl>((MouseEventArgs) e, (FrameworkElement) this);
      if (mousePointElement == null)
        return;
      foreach (QuadrantControl quadrant in this._quadrants)
      {
        if (!quadrant.Equals((object) mousePointElement))
          quadrant.ClearSelected();
      }
    }

    internal bool AddButtonMoveOver()
    {
      return this._quadrants.Any<QuadrantControl>((Func<QuadrantControl, bool>) (quad => quad.AddButton.IsMouseOver));
    }

    public void Reload(string id)
    {
      if (string.IsNullOrEmpty(id))
      {
        this.LoadTask();
      }
      else
      {
        foreach (QuadrantControl quadrant in this._quadrants)
        {
          if (quadrant.Quadrant?.id == id)
            quadrant.LoadAsync();
        }
      }
    }

    public async Task BatchPinTask()
    {
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.BatchPinTask().ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    public void BatchOpenSticky()
    {
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.BatchOpenSticky();
    }

    public void TryBatchSetDate(DateTime? date)
    {
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.BatchSetDate(date).ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    public void TryBatchSetPriority(int priority)
    {
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.BatchSetPriority(priority).ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      if (Window.GetWindow((DependencyObject) this) is WidgetWindow)
      {
        if (this._widgetMorePopup == null)
        {
          this._widgetMorePopup = new WidgetMorePopup(new Action(this.OnMorePopupClosed));
          this._widgetMorePopup.SetPlaceTarget((UIElement) this.MoreGrid);
          this._widgetMorePopup.MoreAction += new EventHandler<WidgetMoreAction>(this.OnMoreMoreAction);
        }
        this._widgetMorePopup.Show(new System.Windows.Point(-5.0, -10.0));
        this.SetInOperation(true);
      }
      else
      {
        CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) new List<CustomMenuItemViewModel>()
        {
          new CustomMenuItemViewModel((object) "showComplete", Utils.GetString(LocalSettings.Settings.MatrixShowCompleted ? "HideCompleted" : "ShowCompleted"), Utils.GetImageSource(LocalSettings.Settings.HideComplete ? "showCompletedDrawingImage" : "HideCompletedDrawingImage"))
        }, (Popup) this.MorePopup);
        customMenuList.Operated += (EventHandler<object>) ((o, c) =>
        {
          LocalSettings.Settings.MatrixModel.SetShowComplete(!LocalSettings.Settings.MatrixShowCompleted);
          SettingsHelper.PushLocalPreference();
          DataChangedNotifier.NotifyMatrixQuadrantChanged(-1);
        });
        customMenuList.Show();
      }
    }

    private void OnMoreMoreAction(object sender, WidgetMoreAction e)
    {
      EventHandler<WidgetMoreAction> moreAction = this.MoreAction;
      if (moreAction == null)
        return;
      moreAction((object) this, e);
    }

    private void OnContainerSizeChanged(object sender, SizeChangedEventArgs e)
    {
      double width = e.NewSize.Width / 2.0 - 6.0;
      double height = e.NewSize.Height / 2.0 - 6.0;
      for (int index = 0; index < 4; ++index)
        this._quadrantLocations[index] = new Rect((double) (index % 2) * (width + 12.0), index < 2 ? 0.0 : height + 12.0, width, height);
      if (this.DragQuadrantPopup.IsOpen)
        return;
      this.SetQuadrantLocation();
    }

    private void SetQuadrantLocation()
    {
      for (int index = 0; index < 4; ++index)
      {
        QuadrantControl quadrant = this._quadrants[index];
        quadrant.Width = this._quadrantLocations[index].Width;
        quadrant.Height = this._quadrantLocations[index].Height;
        quadrant.BeginAnimation(Canvas.LeftProperty, (AnimationTimeline) null);
        quadrant.BeginAnimation(Canvas.TopProperty, (AnimationTimeline) null);
        quadrant.SetValue(Canvas.LeftProperty, (object) this._quadrantLocations[index].X);
        quadrant.SetValue(Canvas.TopProperty, (object) this._quadrantLocations[index].Y);
      }
    }

    public void StartDragQuadrant(QuadrantControl quadrant, MouseEventArgs args)
    {
      if (this.DragQuadrantPopup.IsOpen)
        return;
      this._dragStartIndex = this._quadrants.IndexOf(quadrant);
      if (this._dragStartIndex < 0)
        return;
      this._dragStartPosition = args.GetPosition((IInputElement) this.Container);
      this._dragQuadrant = quadrant;
      this._popupStartPoint = new System.Windows.Point((double) quadrant.GetValue(Canvas.LeftProperty), (double) quadrant.GetValue(Canvas.TopProperty));
      this.DragQuadrantPopup.HorizontalOffset = this._popupStartPoint.X;
      this.DragQuadrantPopup.VerticalOffset = this._popupStartPoint.Y;
      this.DragVisual.Visual = (Visual) quadrant;
      this.DragQuadrantPopup.IsOpen = true;
      quadrant.BeginAnimation(Canvas.LeftProperty, (AnimationTimeline) null);
      quadrant.SetValue(Canvas.LeftProperty, (object) (this.Container.ActualWidth * -1.0));
      this.SetupDragEvent();
    }

    private void SetupDragEvent()
    {
      this.CaptureMouse();
      this.MouseMove -= new MouseEventHandler(this.OnDragMouseMove);
      this.MouseMove += new MouseEventHandler(this.OnDragMouseMove);
      this.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnDragDrop);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDragDrop);
    }

    private void OnDragDrop(object sender, MouseButtonEventArgs e) => this.OnQuadrantDrop();

    private void OnDragMouseMove(object sender, MouseEventArgs e)
    {
      if (this.DragQuadrantPopup.IsOpen && e.LeftButton == MouseButtonState.Pressed)
      {
        System.Windows.Point position = e.GetPosition((IInputElement) this.Container);
        this.DragQuadrantPopup.HorizontalOffset = this._popupStartPoint.X + (position - this._dragStartPosition).X;
        this.DragQuadrantPopup.VerticalOffset = this._popupStartPoint.Y + (position - this._dragStartPosition).Y;
        for (int index = 0; index < 4; ++index)
        {
          Rect quadrantLocation = this._quadrantLocations[index];
          if (index != this._dragStartIndex && IsInRect(index, quadrantLocation, position))
          {
            if (index == this._dragHoverIndex)
              return;
            this.SetLocationAnimation(this._dragHoverIndex, this._dragHoverIndex);
            this._dragHoverIndex = index;
            this.TryMoveHoverQuadrant(index);
            return;
          }
        }
        this.SetLocationAnimation(this._dragHoverIndex, this._dragHoverIndex);
        this._dragHoverIndex = -1;
      }
      else
        this.OnQuadrantDrop();

      static bool IsInRect(int i, Rect rect, System.Windows.Point position)
      {
        return (position.X <= rect.Right + 6.0 || i % 2 != 0) && (position.X >= rect.Left - 6.0 || i % 2 != 1) && (position.Y <= rect.Bottom + 6.0 || i >= 2) && (position.Y >= rect.Top - 6.0 || i < 2);
      }
    }

    private async Task TryMoveHoverQuadrant(int index)
    {
      await Task.Delay(100);
      if (index != this._dragHoverIndex)
        return;
      this.SetLocationAnimation(index, this._dragStartIndex);
    }

    private void OnQuadrantDrop()
    {
      if (!this.DragQuadrantPopup.IsOpen)
        return;
      this.DragQuadrantPopup.IsOpen = false;
      if (this._dragHoverIndex >= 0)
      {
        this._quadrants[this._dragStartIndex] = this._quadrants[this._dragHoverIndex];
        this._quadrants[this._dragHoverIndex] = this._dragQuadrant;
        Dictionary<string, long> orders = new Dictionary<string, long>();
        for (int index = 0; index < 4; ++index)
        {
          QuadrantControl quadrant = this._quadrants[index];
          orders[quadrant.Quadrant.id] = (long) index * 268435456L;
        }
        MatrixManager.UpdateQuadrantsSortOrder(orders, (object) this);
      }
      this.SetQuadrantLocation();
      this.ReleaseMouseCapture();
      this.MouseMove -= new MouseEventHandler(this.OnDragMouseMove);
      this.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnDragDrop);
      this._dragStartIndex = -1;
      this._dragHoverIndex = -1;
      this._dragQuadrant = (QuadrantControl) null;
      this.DragVisual.Visual = (Visual) null;
    }

    private void SetLocationAnimation(int from, int to)
    {
      if (from < 0 || from == this._dragStartIndex)
        return;
      QuadrantControl quadrant = this._quadrants[from];
      Storyboard resource = (Storyboard) this.FindResource((object) "DragMoveStory");
      if (resource.Children[0] is DoubleAnimation child1)
      {
        child1.To = new double?(this._quadrantLocations[to].X);
        Storyboard.SetTarget((DependencyObject) child1, (DependencyObject) quadrant);
      }
      if (resource.Children[1] is DoubleAnimation child2)
      {
        child2.To = new double?(this._quadrantLocations[to].Y);
        Storyboard.SetTarget((DependencyObject) child2, (DependencyObject) quadrant);
      }
      resource.Begin();
    }

    public void BeginSyncStory()
    {
      (this.MoreGrid.Visibility == Visibility.Visible ? (Storyboard) this.FindResource((object) "UnlockedSyncStory") : (Storyboard) this.FindResource((object) "LockedSyncStory")).Begin();
      this.SyncGrid.IsHitTestVisible = false;
    }

    private void SyncStoryCompleted(object sender, EventArgs e)
    {
      if (this.MoreGrid.Visibility == Visibility.Visible)
        return;
      this.SyncGrid.IsHitTestVisible = true;
    }

    public void SetLocked(bool isLocked)
    {
      this.IsLocked = isLocked;
      this.DragPanel.IsEnabled = !isLocked;
      this.UnLockGrid.Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
      this.MoreGrid.Visibility = isLocked ? Visibility.Collapsed : Visibility.Visible;
      this.SyncGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      this.SyncGrid.Opacity = (double) (isLocked ? 1 : 0);
      this.SyncGrid.IsHitTestVisible = isLocked;
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.SetLocked(isLocked);
    }

    public void SetQuadrantBackOpacity(double opacity)
    {
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.SetBackBorderOpacity(opacity);
    }

    public void SetWidgetMode(WidgetViewModel model)
    {
      this.TitleName.Margin = new Thickness(0.0, 0.0, 0.0, 10.0);
      this.TitleName.FontSize = 18.0;
      this.HeadRow.Height = new GridLength(44.0);
      this.DragPanel.Cursor = Cursors.SizeAll;
      this.Quadrant1.SetWidgetBackColor((double) model.Opacity);
      this.Quadrant2.SetWidgetBackColor((double) model.Opacity);
      this.Quadrant3.SetWidgetBackColor((double) model.Opacity);
      this.Quadrant4.SetWidgetBackColor((double) model.Opacity);
    }

    public void SetBlur()
    {
      this.BlurEffect.Radius = UserDao.IsPro() ? 0.0 : 8.0;
      this.IsEnabled = UserDao.IsPro();
    }

    public void SetInOperation(bool inOperation) => this.InOperation = inOperation;

    private void OnMorePopupClosed() => this.SetInOperation(false);

    public void ToastMoveProjectControl(string taskProjectId, string taskTitle, MoveToastType type)
    {
      this.GetToastParent()?.ToastMoveProjectControl(taskProjectId, taskTitle, type);
    }

    public void BatchDeleteTask()
    {
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.BatchDeleteTask();
    }

    public void ExpandOrFoldAllSection()
    {
      bool flag = this._quadrants.All<QuadrantControl>((Func<QuadrantControl, bool>) (q => q.TaskList.IsAllSectionOpen()));
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.ToggleAllSection(new bool?(!flag));
    }

    public void ExpandOrFoldAllTask()
    {
      bool flag = this._quadrants.All<QuadrantControl>((Func<QuadrantControl, bool>) (q => q.TaskList.IsAllTaskOpen()));
      foreach (QuadrantControl quadrant in this._quadrants)
        quadrant.ExpandOrFoldAllTask(new bool?(!flag));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/eisenhower/matrixcontainer.xaml", UriKind.Relative));
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
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.SyncStoryCompleted);
          break;
        case 3:
          ((Timeline) target).Completed += new EventHandler(this.SyncStoryCompleted);
          break;
        case 4:
          this.BlurEffect = (BlurEffect) target;
          break;
        case 5:
          this.HeadRow = (RowDefinition) target;
          break;
        case 6:
          this.TitleName = (TextBlock) target;
          break;
        case 7:
          this.DragPanel = (Border) target;
          break;
        case 8:
          this.UnLockGrid = (HoverIconButton) target;
          break;
        case 9:
          this.SyncGrid = (HoverIconButton) target;
          break;
        case 10:
          this.MoreGrid = (HoverIconButton) target;
          break;
        case 11:
          this.MorePopup = (EscPopup) target;
          break;
        case 12:
          this.Container = (Canvas) target;
          this.Container.SizeChanged += new SizeChangedEventHandler(this.OnContainerSizeChanged);
          break;
        case 13:
          this.Quadrant1 = (QuadrantControl) target;
          break;
        case 14:
          this.Quadrant2 = (QuadrantControl) target;
          break;
        case 15:
          this.Quadrant3 = (QuadrantControl) target;
          break;
        case 16:
          this.Quadrant4 = (QuadrantControl) target;
          break;
        case 17:
          this.DragQuadrantPopup = (Popup) target;
          break;
        case 18:
          this.DragVisual = (VisualBrush) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
