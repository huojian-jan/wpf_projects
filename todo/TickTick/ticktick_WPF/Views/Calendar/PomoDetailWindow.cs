// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.PomoDetailWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class PomoDetailWindow : EscPopup, IComponentConnector, IStyleConnector
  {
    private PomoFilterControl _pomoFilter;
    private PomoDisplayViewModel _model;
    private CalendarDisplayViewModel _calModel;
    internal PomoDetailWindow Root;
    internal ContentControl Control;
    internal Grid Container;
    internal ScrollViewer ScrollViewer;
    internal ItemsControl TaskPomoItems;
    internal EmojiEditor NoteEditor;
    internal TextBlock LengthText;
    internal Popup FilterPopup;
    internal Border DeleteBorder;
    internal Border ToastBorder;
    internal TextBlock ToastText;
    internal Popup DeletePopup;
    private bool _contentLoaded;

    public PomoDetailWindow(PomoDisplayViewModel model, CalendarDisplayViewModel calModel)
    {
      this.InitializeComponent();
      this._model = model;
      this._calModel = calModel;
      this.DataContext = (object) this._model;
      this.NoteEditor.Text = model.Note;
      this.NoteEditor.ReadOnly = !model.Enable;
      this.Closed += (EventHandler) ((o, e) =>
      {
        PopupStateManager.OnViewPopupClosed();
        if (Utils.IsEqualString(this._model.Note, this.NoteEditor.Text))
          return;
        PomoService.SaveNote(this._model.Id, this.NoteEditor.Text);
      });
    }

    private async void OnDeleteMouseUp(object sender, MouseButtonEventArgs e)
    {
      this.DeletePopup.IsOpen = true;
    }

    private void OnShowLinkPopupMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement element) || !(element.DataContext is PomoTaskDisplayViewModel dataContext))
        return;
      this.ShowLinkPopup(element, dataContext);
    }

    private void ShowLinkPopup(FrameworkElement element, PomoTaskDisplayViewModel pomoTask)
    {
      if (this._pomoFilter == null)
      {
        PomoFilterControl pomoFilterControl = new PomoFilterControl(this.FilterPopup, false);
        pomoFilterControl.ShowComplete = true;
        pomoFilterControl.DataContext = (object) new FocusViewModel(pomoTask.GetId());
        this._pomoFilter = pomoFilterControl;
        this.FilterPopup.Child = (UIElement) this._pomoFilter;
      }
      this._pomoFilter.ClearItemSelectedEvent();
      this._pomoFilter.ItemSelected += (EventHandler<DisplayItemModel>) (async (obj, model) =>
      {
        TimerModel timerByIdOrObjId = await TimerDao.GetTimerByIdOrObjId(model.Id);
        pomoTask.Title = model.IsHabit ? model.Habit.Name : model.Title;
        pomoTask.HabitId = model.IsHabit ? model.Id : (timerByIdOrObjId == null || !(timerByIdOrObjId.ObjType == "habit") ? (string) null : timerByIdOrObjId.ObjId);
        pomoTask.TaskId = model.IsTaskOrNote ? model.Id : (timerByIdOrObjId == null || !(timerByIdOrObjId.ObjType == "task") ? (string) null : timerByIdOrObjId.ObjId);
        pomoTask.Bind = true;
        this.FilterPopup.IsOpen = false;
        this.CheckTitleChanged(pomoTask);
        PomoService.RebindTaskIdOfPomoTask(pomoTask.PomoId, pomoTask.StartTime, pomoTask.TaskId, pomoTask.HabitId, timerByIdOrObjId);
      });
      this.FilterPopup.PlacementTarget = (UIElement) element;
      this.FilterPopup.IsOpen = true;
    }

    private void CheckTitleChanged(PomoTaskDisplayViewModel pomoTask)
    {
      if (!(this.DataContext is PomoDisplayViewModel dataContext) || this._calModel == null)
        return;
      TaskBaseViewModel taskById;
      if (dataContext.PomoTaskModels.Count <= 1)
      {
        this._calModel.SourceViewModel.Title = pomoTask.Title;
        taskById = TaskCache.GetTaskById(pomoTask.TaskId);
      }
      else
      {
        long maxDuration = dataContext.PomoTaskModels.Max<PomoTaskDisplayViewModel>((Func<PomoTaskDisplayViewModel, long>) (m => m.Duration));
        PomoTaskDisplayViewModel displayViewModel = dataContext.PomoTaskModels.FirstOrDefault<PomoTaskDisplayViewModel>((Func<PomoTaskDisplayViewModel, bool>) (p => p.Duration == maxDuration));
        if (displayViewModel?.Title != pomoTask.Title)
          return;
        this._calModel.SourceViewModel.Title = pomoTask.Title + "...";
        taskById = TaskCache.GetTaskById(displayViewModel?.TaskId);
      }
      TaskBaseViewModel vm = new TaskBaseViewModel()
      {
        Type = DisplayType.Pomo,
        Id = this._calModel.SourceViewModel.Id,
        Title = this._calModel.SourceViewModel.Title,
        IsAllDay = new bool?(false),
        StartDate = this._calModel.SourceViewModel.StartDate,
        DueDate = this._calModel.SourceViewModel.DueDate,
        CompletedTime = this._calModel.SourceViewModel.CompletedTime
      };
      this._calModel.SetSourceModel(vm, true);
      vm.SetDependenceModel(taskById, "Color", "Tag", "Priority");
      vm.Color = taskById?.Color;
      vm.Tag = taskById?.Tag;
      vm.Priority = taskById != null ? taskById.Priority : 0;
    }

    public async void Show(UIElement target, double targetWidth, bool showByMouse)
    {
      PomoDetailWindow pomoDetailWindow = this;
      pomoDetailWindow.PlacementTarget = target;
      TaskPopupArgs popupLocation = PopupLocationCalculator.GetPopupLocation(target, targetWidth, 460.0, showByMouse, 0.0);
      if (!popupLocation.ByMouse)
      {
        pomoDetailWindow.Placement = popupLocation.IsRight ? PlacementMode.Right : PlacementMode.Left;
        pomoDetailWindow.HorizontalOffset = popupLocation.IsRight ? -6.0 : 6.0;
        pomoDetailWindow.VerticalOffset = -8.0;
      }
      else
        pomoDetailWindow.Placement = PlacementMode.Mouse;
      PopupStateManager.OnViewPopupOpened();
      pomoDetailWindow.IsOpen = true;
      await Task.Delay(200);
      if (!pomoDetailWindow.IsOpen)
        return;
      PopupStateManager.OnViewPopupOpened();
    }

    private async Task ShowTaskDetail(FrameworkElement element, PomoTaskDisplayViewModel pomoTask)
    {
      PomoDetailWindow pomoDetailWindow = this;
      if (string.IsNullOrEmpty(pomoTask.TaskId))
        return;
      if (!await pomoDetailWindow.CheckTaskExist(pomoTask.TaskId) || PopupStateManager.LastTarget == element)
        return;
      TaskDetailPopup taskDetailPopup = new TaskDetailPopup();
      taskDetailPopup.DependentWindow = (IToastShowWindow) null;
      pomoDetailWindow.StaysOpen = true;
      taskDetailPopup.Disappear -= new EventHandler<string>(pomoDetailWindow.OnDetailClosed);
      taskDetailPopup.Disappear += new EventHandler<string>(pomoDetailWindow.OnDetailClosed);
      taskDetailPopup.Show(pomoTask.TaskId, (string) null, new TaskWindowDisplayArgs((UIElement) element, 30.0, new System.Windows.Point(0.0, 30.0), 0.0));
    }

    private async void OnTitleClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement element) || !(element.DataContext is PomoTaskDisplayViewModel dataContext))
        return;
      if (dataContext.Bind)
      {
        if (!string.IsNullOrEmpty(dataContext.TaskId))
        {
          await this.ShowTaskDetail(element, dataContext);
        }
        else
        {
          if (string.IsNullOrEmpty(dataContext.HabitId))
            return;
          int num = await this.CheckHabitExist(dataContext.HabitId) ? 1 : 0;
        }
      }
      else
        this.ShowLinkPopup(element, dataContext);
    }

    private async Task<bool> CheckHabitExist(string habitId)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById != null && habitById.SyncStatus != -1)
        return true;
      this.Toast(Utils.GetString("HabitDeleted"));
      return false;
    }

    private async Task<bool> CheckTaskExist(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById != null && thinTaskById.deleted == 0)
        return true;
      this.Toast(Utils.GetString("TaskDeleted"));
      return false;
    }

    private async void OnDetailClosed(object sender, string e)
    {
      PomoDetailWindow pomoDetailWindow = this;
      if (pomoDetailWindow.PlacementTarget != null)
        Window.GetWindow((DependencyObject) pomoDetailWindow.PlacementTarget)?.Activate();
      pomoDetailWindow.StaysOpen = false;
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.Disappear -= new EventHandler<string>(pomoDetailWindow.OnDetailClosed);
      await Task.Delay(150);
      PopupStateManager.OnViewPopupOpened();
    }

    public void Toast(string toastText)
    {
      this.ToastBorder.Visibility = Visibility.Visible;
      this.ToastText.Text = toastText;
      this.ToastBorder.BeginStoryboard((Storyboard) this.FindResource((object) "ShowToast"));
    }

    private void OnToasted(object sender, EventArgs e)
    {
      this.ToastBorder.Visibility = Visibility.Collapsed;
    }

    private void CancelBtnClick(object sender, RoutedEventArgs e)
    {
      this.DeletePopup.IsOpen = false;
    }

    private async void OkBtnClick(object sender, RoutedEventArgs e)
    {
      PomoDetailWindow pomoDetailWindow = this;
      pomoDetailWindow.DeletePopup.IsOpen = false;
      if (!(pomoDetailWindow.DataContext is PomoDisplayViewModel dataContext))
        return;
      await PomoService.DeleteById(dataContext.Id);
      pomoDetailWindow.Close();
    }

    private void OnNoteChanged(object sender, EventArgs e)
    {
      if (this.NoteEditor.Text.Length >= 400)
      {
        this.LengthText.Visibility = Visibility.Visible;
        this.LengthText.Text = this.NoteEditor.Text.Length.ToString() + "/500";
      }
      else
        this.LengthText.Visibility = Visibility.Collapsed;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.NoteEditor.IsMouseOver)
        return;
      FocusManager.SetFocusedElement((DependencyObject) this, (IInputElement) this);
      Keyboard.Focus((IInputElement) this);
    }

    public void SetCustomStyle()
    {
    }

    public void OnCancel() => this.IsOpen = false;

    public void Ok()
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/pomodetailwindow.xaml", UriKind.Relative));
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
          this.Root = (PomoDetailWindow) target;
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.OnToasted);
          break;
        case 3:
          this.Control = (ContentControl) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
          break;
        case 5:
          this.Container = (Grid) target;
          break;
        case 6:
          this.ScrollViewer = (ScrollViewer) target;
          break;
        case 7:
          this.TaskPomoItems = (ItemsControl) target;
          break;
        case 11:
          this.NoteEditor = (EmojiEditor) target;
          break;
        case 12:
          this.LengthText = (TextBlock) target;
          break;
        case 13:
          this.FilterPopup = (Popup) target;
          break;
        case 14:
          this.DeleteBorder = (Border) target;
          this.DeleteBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteMouseUp);
          break;
        case 15:
          this.ToastBorder = (Border) target;
          break;
        case 16:
          this.ToastText = (TextBlock) target;
          break;
        case 17:
          this.DeletePopup = (Popup) target;
          break;
        case 18:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OkBtnClick);
          break;
        case 19:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CancelBtnClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 8:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTitleClick);
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTitleClick);
          break;
        case 10:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowLinkPopupMouseUp);
          break;
      }
    }
  }
}
