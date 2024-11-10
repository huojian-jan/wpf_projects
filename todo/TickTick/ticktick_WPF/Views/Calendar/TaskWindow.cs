// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TaskWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.QuickAdd;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TaskWindow : IndependentWindow, IListViewParent
  {
    public static TaskWindow Window;
    public static string Name = DisplayModule.Task.ToString();
    private ListViewContainer _listView;
    private WindowModel _model;
    private Border _immersiveBorder;
    private Border _containBorder;

    public static bool IsShowing => TaskWindow.Window != null;

    private TaskWindow(WindowModel windowModel)
      : base(windowModel)
    {
      this.Id = TaskWindow.Name;
      this._model = windowModel;
      this._immersiveBorder = new Border();
      this.Container.Children.Add((UIElement) this._immersiveBorder);
      this._containBorder = new Border();
      this.Container.Children.Add((UIElement) this._containBorder);
      this._listView = ListViewContainer.GetListView(nameof (TaskWindow), this._containBorder);
      this.MinWidth = 400.0;
      this.MinHeight = 462.0;
      this._containBorder.Child = (UIElement) this._listView;
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnWindowMouseUp);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnWindowMouseDown);
      this.Title = Utils.GetString("Task");
      this.Loaded += new RoutedEventHandler(this.OnWindowLoaded);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      ((HwndSource) PresentationSource.FromVisual((Visual) this))?.AddHook(new HwndSourceHook(this.Hook));
    }

    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg != 526)
        return IntPtr.Zero;
      this.OnMouseTilt(TaskWindow.LOWORD(wParam));
      return (IntPtr) 1;
    }

    private static int HIWORD(IntPtr ptr) => ptr.ToInt32() >> 16 & (int) ushort.MaxValue;

    private static int LOWORD(IntPtr ptr) => ((int) ptr.ToInt64() >> 16) % 256;

    private void OnMouseTilt(int offset) => this._listView?.OnScroll(offset);

    private void OnWindowMouseDown(object sender, MouseButtonEventArgs e) => this._mouseDown = true;

    private async void OnWindowMouseUp(object sender, MouseButtonEventArgs e)
    {
      TaskWindow element = this;
      if (!element._mouseDown)
        return;
      element._mouseDown = false;
      if (element._listView?.GetTaskView()?.DetailMouseOver().GetValueOrDefault() || Utils.GetMousePointVisibleItem<QuickAddView>((MouseEventArgs) e, (FrameworkElement) element) != null || Utils.GetMousePointVisibleItem<TextBox>((MouseEventArgs) e, (FrameworkElement) element) != null)
        return;
      await Task.Delay(100);
      if (!element.IsActive || e.Handled)
        return;
      element.TryFocus();
    }

    public async Task TryFocus()
    {
      TaskWindow element = this;
      await Task.Delay(10);
      Keyboard.ClearFocus();
      if (!PopupStateManager.CanShowAddPopup())
      {
        await Task.Delay(300);
        if (!PopupStateManager.CanShowAddPopup())
          return;
      }
      if (!element.IsActive)
        return;
      FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
      Keyboard.Focus((IInputElement) element);
    }

    public static async Task ShowWindow(bool force = true)
    {
      WindowModel windowModel = await WindowModelDao.GetModelByIdAsync(TaskWindow.Name);
      if (!force)
      {
        if (windowModel == null)
        {
          windowModel = (WindowModel) null;
          return;
        }
        if (windowModel.Closed)
        {
          windowModel = (WindowModel) null;
          return;
        }
      }
      if (TaskWindow.Window != null)
      {
        TaskWindow.Window.Show();
        TaskWindow.Window.Activate();
        TaskWindow.Window.Reload();
        windowModel = (WindowModel) null;
      }
      else
      {
        if (windowModel == null || windowModel.Closed)
          IndependentWindow.AddShowEvent("tasks");
        if (windowModel == null)
        {
          windowModel = await IndependentWindow.GetNewWindowModel(TaskWindow.Name, data: JsonConvert.SerializeObject((object) new TaskWindow.TaskWindowSet()
          {
            ProjectWidth = LocalSettings.Settings.ProjectPanelWidth,
            DetailWidth = LocalSettings.Settings.DetailListDivide,
            SaveProject = LocalSettings.Settings.SelectProjectId
          }));
          BaseDao<WindowModel>.UpdateAsync(windowModel);
        }
        else
        {
          windowModel.Closed = false;
          await WindowModelDao.OpenWindow(windowModel);
        }
        TaskWindow.Window = new TaskWindow(windowModel);
        TaskWindow.Window.Show();
        TaskWindow.Window.SetBackBrush();
        windowModel = (WindowModel) null;
      }
    }

    private void Reload() => this._listView?.ReloadView();

    protected override void OnClosing()
    {
      this.Container.Children.Clear();
      ListViewContainer.RemoveListView(nameof (TaskWindow));
      TaskWindow.Window = (TaskWindow) null;
    }

    public override void Print() => this._listView?.TryPrint(false);

    public override void SwitchViewMode(string mode)
    {
      this._listView?.ProjectIdentity?.SwitchViewMode(mode);
    }

    public static void OpenOrCloseWindow(System.Windows.Window owner)
    {
      if (TaskWindow.Window != null)
      {
        TaskWindow.Window.Close();
      }
      else
      {
        if (!IndependentWindow.CheckCount(owner))
          return;
        TaskWindow.ShowWindow();
      }
    }

    public async void OnProjectWidthChanged(double width)
    {
      WindowModel windowModel = await WindowModelDao.GetModelByIdAsync(TaskWindow.Name);
      if (windowModel == null)
      {
        windowModel = (WindowModel) null;
      }
      else
      {
        TaskWindow.TaskWindowSet taskWindowSet = string.IsNullOrEmpty(windowModel.Data) ? new TaskWindow.TaskWindowSet() : JsonConvert.DeserializeObject<TaskWindow.TaskWindowSet>(windowModel.Data) ?? new TaskWindow.TaskWindowSet();
        taskWindowSet.ProjectWidth = width;
        windowModel.Data = JsonConvert.SerializeObject((object) taskWindowSet);
        int num = await BaseDao<WindowModel>.UpdateAsync(windowModel);
        this._model = windowModel;
        windowModel = (WindowModel) null;
      }
    }

    public async void OnDetailWidthChanged(double width)
    {
      WindowModel windowModel = await WindowModelDao.GetModelByIdAsync(TaskWindow.Name);
      if (windowModel == null)
      {
        windowModel = (WindowModel) null;
      }
      else
      {
        TaskWindow.TaskWindowSet taskWindowSet = string.IsNullOrEmpty(windowModel.Data) ? new TaskWindow.TaskWindowSet() : JsonConvert.DeserializeObject<TaskWindow.TaskWindowSet>(windowModel.Data) ?? new TaskWindow.TaskWindowSet();
        taskWindowSet.DetailWidth = width;
        windowModel.Data = JsonConvert.SerializeObject((object) taskWindowSet);
        int num = await BaseDao<WindowModel>.UpdateAsync(windowModel);
        this._model = windowModel;
        windowModel = (WindowModel) null;
      }
    }

    public void SetMinSize(int width, int height)
    {
      this.MinWidth = (double) (width - 50);
      this.MinHeight = (double) height;
    }

    public async void SaveSelectedProject(string saveProjectId)
    {
      WindowModel windowModel = await WindowModelDao.GetModelByIdAsync(TaskWindow.Name);
      if (windowModel == null)
      {
        windowModel = (WindowModel) null;
      }
      else
      {
        TaskWindow.TaskWindowSet taskWindowSet = string.IsNullOrEmpty(windowModel.Data) ? new TaskWindow.TaskWindowSet() : JsonConvert.DeserializeObject<TaskWindow.TaskWindowSet>(windowModel.Data) ?? new TaskWindow.TaskWindowSet();
        taskWindowSet.SaveProject = saveProjectId;
        windowModel.Data = JsonConvert.SerializeObject((object) taskWindowSet);
        int num = await BaseDao<WindowModel>.UpdateAsync(windowModel);
        this._model = windowModel;
        windowModel = (WindowModel) null;
      }
    }

    public double GetProjectWidth()
    {
      return (string.IsNullOrEmpty(this._model?.Data) ? new TaskWindow.TaskWindowSet() : JsonConvert.DeserializeObject<TaskWindow.TaskWindowSet>(this._model?.Data) ?? new TaskWindow.TaskWindowSet()).ProjectWidth;
    }

    public double GetDetailWidth()
    {
      return (string.IsNullOrEmpty(this._model?.Data) ? new TaskWindow.TaskWindowSet() : JsonConvert.DeserializeObject<TaskWindow.TaskWindowSet>(this._model?.Data) ?? new TaskWindow.TaskWindowSet()).DetailWidth;
    }

    public string GetSelectedProject()
    {
      return (string.IsNullOrEmpty(this._model?.Data) ? new TaskWindow.TaskWindowSet() : JsonConvert.DeserializeObject<TaskWindow.TaskWindowSet>(this._model?.Data) ?? new TaskWindow.TaskWindowSet()).SaveProject;
    }

    public async Task NavigateTask(ProjectTask task)
    {
      TaskWindow target = this;
      if (await TaskUtils.TryLoadTask(task.TaskId, task.ProjectId) == null)
        return;
      TaskDetailWindow taskDetailWindow = new TaskDetailWindow();
      taskDetailWindow.ShowInNavigate(task.TaskId, (System.Windows.Window) target);
      taskDetailWindow.DependentWindow = (IToastShowWindow) target;
    }

    public void ExitImmersiveMode()
    {
      if (this._immersiveBorder.Visibility != Visibility.Visible)
        return;
      this._immersiveBorder.Visibility = Visibility.Collapsed;
      this._immersiveBorder.Child = (UIElement) null;
      this._containBorder.Visibility = Visibility.Visible;
    }

    public async void EnterImmersiveMode(string taskId, int caretOffset = -1)
    {
      ImmersiveContent immersiveContent = new ImmersiveContent();
      immersiveContent.Margin = new Thickness(0.0, 24.0, 0.0, 12.0);
      immersiveContent.VerticalAlignment = VerticalAlignment.Stretch;
      ImmersiveContent editor = immersiveContent;
      this._immersiveBorder.Child = (UIElement) editor;
      this._immersiveBorder.Visibility = Visibility.Visible;
      this._containBorder.Visibility = Visibility.Collapsed;
      await editor.LoadData(taskId);
      MarkDownEditor contentText = editor.TaskDetail.GetContentText();
      if (contentText == null)
        editor = (ImmersiveContent) null;
      else if (caretOffset < 0)
        editor = (ImmersiveContent) null;
      else if (caretOffset >= contentText.EditBox.Text.Length)
      {
        editor = (ImmersiveContent) null;
      }
      else
      {
        contentText.EditBox.CaretOffset = caretOffset;
        contentText.FocusEditBox();
        contentText.EditBox.ScrollToEnd();
        editor = (ImmersiveContent) null;
      }
    }

    public override void BatchSetPriorityCommand(int priority)
    {
      this._listView?.TryBatchSetPriority(priority);
    }

    public override void BatchSetDateCommand(DateTime? date)
    {
      this._listView?.TryBatchSetDate(date);
    }

    public override void BatchPinTaskCommand() => this._listView?.BatchPinTask();

    public override void BatchOpenStickyCommand() => this._listView?.BatchOpenSticky();

    public override void BatchDeleteCommand() => this._listView?.BatchDeleteTask();

    private class TaskWindowSet
    {
      public double ProjectWidth { get; set; } = 256.0;

      public double DetailWidth { get; set; } = 1.0;

      public string SaveProject { get; set; }
    }
  }
}
