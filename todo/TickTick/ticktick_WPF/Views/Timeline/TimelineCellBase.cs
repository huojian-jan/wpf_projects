// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineCellBase
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineCellBase : UserControl, IShowTaskDetailWindow
  {
    protected TimelineCellViewModel cellModel;
    private TimelineCellViewModel _detailData;
    private TimelineContainer _container;
    private bool _previewLeftClick;
    private bool _isWaitingDoubleClick;
    private DateTime _lastClickTime;

    public TimelineCellBase()
    {
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
    }

    protected TimelineViewModel timelineModel
    {
      get => this.GetContainer()?.GetViewModel() ?? TimelineViewModel.Instance;
    }

    protected TimelineContainer Container => this.GetContainer();

    private TimelineContainer GetContainer()
    {
      if (this._container == null)
        this._container = Utils.FindParent<TimelineContainer>((DependencyObject) this);
      return this._container;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue is TimelineCellViewModel newValue)
      {
        this.cellModel = newValue;
        newValue.SetColorAndAvatar();
      }
      else
        this.cellModel = (TimelineCellViewModel) null;
    }

    protected bool ModelInvalid()
    {
      return this.timelineModel == null || this.cellModel == null || this.Container == null;
    }

    protected void OnCellMouseLeave(object sender, MouseEventArgs e)
    {
      this.Container?.TimelineFloating?.TryClose(this.cellModel);
    }

    protected async void OnCellMouseEnter(object sender, MouseEventArgs e)
    {
      TimelineCellBase ui = this;
      if (ui.cellModel?.DisplayModel == null)
        return;
      TimelineCellViewModel cellModel = ui.cellModel;
      if ((cellModel != null ? (cellModel.IsNew ? 1 : 0) : 1) != 0)
        return;
      ui.Container?.TimelineFloating.Open(ui.cellModel, ui);
    }

    public async Task OpenDetailWindow(bool inArrange = false)
    {
      if (this.ModelInvalid())
        return;
      TimelineCellViewModel cellModel = this.cellModel;
      this.timelineModel.ClearBatchSelect();
      if (cellModel.DisplayModel.Type == DisplayType.Event)
        this.ShowEventDetail(cellModel);
      else
        this.TryShowTaskDetail(cellModel);
      this.Container?.TryCloseToolTips();
      this.Container?.StartIgnoreMouseUp();
    }

    private async void TryShowTaskDetail(TimelineCellViewModel model)
    {
      this._isWaitingDoubleClick = false;
      bool firstClick = (DateTime.Now - this._lastClickTime).TotalMilliseconds > 300.0;
      this._lastClickTime = DateTime.Now;
      TaskDetailPopup window = (TaskDetailPopup) null;
      if (firstClick)
      {
        this._isWaitingDoubleClick = true;
        window = await this.ShowTaskDetail(model);
        await Task.Delay(160);
        if (!this._isWaitingDoubleClick)
        {
          model.Operation = model.Operation.Remove(TimelineCellOperation.Edit);
          TaskDetailPopup taskDetailPopup = window;
          if (taskDetailPopup == null)
          {
            window = (TaskDetailPopup) null;
            return;
          }
          taskDetailPopup.Clear();
          window = (TaskDetailPopup) null;
          return;
        }
      }
      if (firstClick)
        window?.TryShow();
      else
        TaskDetailWindows.ShowTaskWindows(model.Id);
      this._previewLeftClick = false;
      window = (TaskDetailPopup) null;
    }

    protected async Task<TaskDetailPopup> ShowTaskDetail(TimelineCellViewModel model, bool byMouse = true)
    {
      TimelineCellBase target = this;
      model.Operation = model.Operation.Add(TimelineCellOperation.Edit);
      target.timelineModel.Editing = true;
      TaskDetailPopup popup = new TaskDetailPopup();
      MainWindow window = App.Window;
      if (window != null)
      {
        if (PopupStateManager.LastTarget == target && popup.Detail.TaskId == model.Id)
        {
          model.Operation = model.Operation.Remove(TimelineCellOperation.Edit);
          return (TaskDetailPopup) null;
        }
        popup.DependentWindow = (IToastShowWindow) window;
      }
      popup.Disappear -= new EventHandler<string>(target.OnWindowDisappear);
      popup.Disappear += new EventHandler<string>(target.OnWindowDisappear);
      int num = await popup.Show(model.DisplayModel.GetTaskId(), model.DisplayModel.Type == DisplayType.CheckItem ? model.DisplayModel.Id : (string) null, new TaskWindowDisplayArgs((UIElement) target, 0.0, new System.Windows.Point(10.0, 10.0), 0.0), target.timelineModel.ProjectIdentity, !byMouse) ? 1 : 0;
      UserActCollectUtils.AddClickEvent("timeline", "task_action", "task_detail");
      if (num == 0)
      {
        model.Operation = model.Operation.Remove(TimelineCellOperation.Edit);
        target.timelineModel.Editing = false;
        return (TaskDetailPopup) null;
      }
      target._detailData = model;
      UndoHelper.NeedToastFilteredTaskId = model.Id;
      return popup;
    }

    private async Task ShowEventDetail(TimelineCellViewModel model)
    {
      TimelineCellBase target = this;
      string id = model.Id;
      if (string.IsNullOrEmpty(id))
        return;
      model.Operation = model.Operation.Add(TimelineCellOperation.Edit);
      target.timelineModel.Editing = true;
      CalendarEventModel eventById = await CalendarEventDao.GetEventById(ArchivedDao.GetOriginalId(id));
      if (eventById != null)
      {
        eventById.DueStart = new DateTime?(model.StartDate);
        eventById.DueEnd = model.EndDate;
        CalendarDetailWindow calendarDetailWindow = new CalendarDetailWindow();
        calendarDetailWindow.Disappear -= new EventHandler<string>(target.OnDetailClosed);
        calendarDetailWindow.Disappear += new EventHandler<string>(target.OnDetailClosed);
        Mouse.GetPosition((IInputElement) target.Container).Y = 36.0;
        calendarDetailWindow.Show((UIElement) target, 40.0, 10.0, true, new CalendarDetailViewModel(eventById));
      }
      else
      {
        model.Operation = model.Operation.Remove(TimelineCellOperation.Edit);
        target.timelineModel.Editing = false;
      }
    }

    private void OnDetailClosed(object sender, string e)
    {
      if (sender is CalendarDetailWindow calendarDetailWindow)
        calendarDetailWindow.Disappear -= new EventHandler<string>(this.OnDetailClosed);
      TimelineCellViewModel cellModel = this.cellModel;
      if (this.ModelInvalid())
        return;
      this.Container?.DelayEndIgnoreMouseUp();
      cellModel.Operation = cellModel.Operation.Remove(TimelineCellOperation.Edit);
      this.timelineModel.Editing = false;
    }

    private async void OnWindowDisappear(object sender, string e)
    {
      TimelineCellBase timelineCellBase = this;
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.Disappear -= new EventHandler<string>(timelineCellBase.OnWindowDisappear);
      TimelineCellViewModel detailData = timelineCellBase._detailData;
      timelineCellBase._detailData = (TimelineCellViewModel) null;
      if (detailData == null)
        return;
      if (detailData.DisplayModel.Deleted == 0 && UndoHelper.NeedToastFilteredTaskId == e && !timelineCellBase.timelineModel.CheckTaskMatched(e))
        timelineCellBase.Container?.TryToastString(Utils.GetString("TaskHasBeenFiltered"));
      detailData.Operation = detailData.Operation.Remove(TimelineCellOperation.Edit);
      timelineCellBase.Container?.DelayEndIgnoreMouseUp();
      if (timelineCellBase.timelineModel == null)
        return;
      timelineCellBase.timelineModel.UpdateCellTime(detailData, forceCheck: true);
      await timelineCellBase.timelineModel.UpdateCellLineAsync();
      timelineCellBase.timelineModel.Editing = false;
    }
  }
}
