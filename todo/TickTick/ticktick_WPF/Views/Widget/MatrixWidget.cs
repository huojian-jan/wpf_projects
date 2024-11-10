// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.MatrixWidget
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
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
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Eisenhower;
using ticktick_WPF.Views.Undo;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class MatrixWidget : UserControl, IToastShowWindow, IWidgetChild, IComponentConnector
  {
    private WidgetSettings _currentWidgetSettings;
    private WidgetWindow _parentWindow;
    internal Grid UndoToastGrid;
    internal MatrixContainer MatrixContainer;
    internal Border NotProBorder;
    private bool _contentLoaded;

    private WidgetViewModel Model => (WidgetViewModel) this.DataContext;

    public string ThemeId => this.Model.ThemeId;

    public MatrixWidget(WidgetViewModel model)
    {
      this.DataContext = (object) model;
      this.InitializeComponent();
      this.InitEvents(model);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => this.MatrixContainer.LoadTask();

    private void InitEvents(WidgetViewModel model)
    {
      this.MatrixContainer.SetWidgetMode(model);
      this.MatrixContainer.MoreAction += new EventHandler<WidgetMoreAction>(this.OnMoreAction);
      this.MatrixContainer.SyncGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSyncClick);
      this.MatrixContainer.DragPanel.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnDragBarMouseDown);
      this.MatrixContainer.DragPanel.MouseMove += new MouseEventHandler(this.OnDragMove);
      this.MatrixContainer.UnLockGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnUnlockWidgetClick);
      this.MatrixContainer.Background = (Brush) Brushes.Transparent;
    }

    public void BindSettingEvent(WidgetSettings settings)
    {
      this._currentWidgetSettings = settings;
      if (settings == null)
        return;
      settings.Closed -= new EventHandler(this.OnSettingsClosed);
      settings.Closed += new EventHandler(this.OnSettingsClosed);
      settings.DisplayOptionChanged -= new EventHandler<string>(this.OnDisplayOptionChanged);
      settings.DisplayOptionChanged += new EventHandler<string>(this.OnDisplayOptionChanged);
      settings.OpacityChanged -= new EventHandler<float>(this.OnOpacityChanged);
      settings.OpacityChanged += new EventHandler<float>(this.OnOpacityChanged);
    }

    public void RemoveSettingEvent(WidgetSettings settings)
    {
      if (settings == null)
        return;
      settings.Closed -= new EventHandler(this.OnSettingsClosed);
      settings.DisplayOptionChanged -= new EventHandler<string>(this.OnDisplayOptionChanged);
      settings.OpacityChanged -= new EventHandler<float>(this.OnOpacityChanged);
    }

    private WidgetWindow GetParentWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.FindParent<WidgetWindow>((DependencyObject) this);
      return this._parentWindow;
    }

    private void OnOpacityChanged(object sender, float opacity)
    {
      this.MatrixContainer.SetQuadrantBackOpacity(this.Model.ThemeId == "dark" ? 1.0 : (double) opacity);
      this.GetParentWindow()?.EnableBlur((double) opacity > 0.0);
    }

    private async void OnDisplayOptionChanged(object sender, string option)
    {
      if (option == null)
        return;
      switch (option)
      {
        case "top":
        case "embed":
        case "bottom":
          this.GetParentWindow()?.SetTopMost();
          break;
        case "light":
        case "dark":
          this.GetParentWindow()?.SetTheme(option);
          this.MatrixContainer.SetQuadrantBackOpacity(this.Model.ThemeId == "dark" ? 1.0 : (double) this.Model.Opacity);
          break;
      }
    }

    private async void OnSettingsClosed(object sender, EventArgs e)
    {
      await this.Model.SaveSingleModel();
    }

    private void OnDragBarMouseDown(object sender, MouseButtonEventArgs e)
    {
      this.GetParentWindow()?.OnDragBarMouseDown(sender, e);
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
      this.GetParentWindow()?.OnDragMove(sender, e);
    }

    private void OnUnlockWidgetClick(object sender, MouseButtonEventArgs e)
    {
      this.LockWidget(false);
    }

    private void LockWidget(bool isLocked)
    {
      this.GetParentWindow().ResizeMode = isLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
      this.Model.IsLocked = isLocked;
      this.Model.SaveSingleModel();
      this.MatrixContainer.SetLocked(isLocked);
    }

    private void OnMoreAction(object sender, WidgetMoreAction e)
    {
      switch (e)
      {
        case WidgetMoreAction.Sync:
          this.OnSyncClick((object) null, (MouseButtonEventArgs) null);
          break;
        case WidgetMoreAction.Setting:
          this.BindSettingEvent(CalendarConfigHelper.TryShowSettings(this.Model));
          break;
        case WidgetMoreAction.Lock:
          this.LockWidget(true);
          this._currentWidgetSettings?.Close();
          break;
        case WidgetMoreAction.Exit:
          MatrixWidgetHelper.CloseWidget();
          break;
      }
    }

    private void OnSyncClick(object sender, MouseButtonEventArgs e)
    {
      this.MatrixContainer.MorePopup.IsOpen = false;
      this.MatrixContainer.BeginSyncStory();
      if (!Utils.IsNetworkAvailable())
        this.TryToastString((object) null, Utils.GetString("NoNetwork"));
      SyncManager.Sync(1);
    }

    private void OnGridWidgetSizeChanged(object sender, SizeChangedEventArgs e)
    {
    }

    private void OnRenewClick(object sender, RoutedEventArgs e)
    {
      Utils.StartUpgrade("matrix_widget");
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      MatrixWidgetHelper.CloseWidget();
    }

    public void Reload() => this.MatrixContainer.LoadTask();

    public async void TaskDeleted(string taskId)
    {
      TaskModel task = await TaskDao.GetTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        task.deleted = TaskService.IsEmptyTask(task) ? 2 : 1;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnDeletedChanged(task);
        UndoToast undoToast = new UndoToast();
        undoToast.InitTaskUndo(taskId, task.title, task.deleted == 2);
        this.ShowUndoToast(undoToast);
        task = (TaskModel) null;
      }
    }

    public void TryToastString(object sender, string e)
    {
      WindowToastHelper.ToastString(this.UndoToastGrid, e, 400.0);
    }

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks)
    {
      return await TaskService.BatchDeleteTasks(tasks, undoGrid: this.UndoToastGrid);
    }

    public async void TaskComplete(CloseUndoToast undo)
    {
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, (FrameworkElement) undo);
    }

    public void TryHideToast()
    {
      if (this.UndoToastGrid.Children.Count <= 0)
        return;
      if (this.UndoToastGrid.Children[0] is UndoToast child)
        child.OnFinished();
      this.UndoToastGrid.Children[0].Visibility = Visibility.Collapsed;
    }

    public void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels)
    {
      if (undoModels == null || !undoModels.Any<TaskDeleteRecurrenceUndoEntity>())
        return;
      UndoToast undoToast = new UndoToast();
      undoToast.InitTaskUndo(undoModels[0]);
      this.ShowUndoToast(undoToast);
    }

    public void Toast(FrameworkElement uiElement)
    {
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, uiElement);
    }

    public void ToastMoveProjectControl(string projectId, string taskName = null, MoveToastType moveType = MoveToastType.Move)
    {
    }

    public async Task ShowUndoToast(UndoToast undoToast)
    {
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, (FrameworkElement) undoToast);
    }

    public bool IsEditing() => this.MatrixContainer.InOperation;

    public void Save() => this.Model.SaveSingleModel();

    public void CheckProEnable()
    {
      this.MatrixContainer.SetBlur();
      this.NotProBorder.Visibility = UserDao.IsPro() ? Visibility.Collapsed : Visibility.Visible;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/widget/matrixwidget.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).SizeChanged += new SizeChangedEventHandler(this.OnGridWidgetSizeChanged);
          break;
        case 2:
          this.UndoToastGrid = (Grid) target;
          break;
        case 3:
          this.MatrixContainer = (MatrixContainer) target;
          break;
        case 4:
          this.NotProBorder = (Border) target;
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnRenewClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
