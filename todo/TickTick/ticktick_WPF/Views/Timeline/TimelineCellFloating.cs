// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineCellFloating
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
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
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineCellFloating : TimelineCellBase, IComponentConnector
  {
    public static readonly DependencyProperty ArrangingProperty = DependencyProperty.Register(nameof (Arranging), typeof (bool), typeof (TimelineCellFloating), new PropertyMetadata((object) false));
    private bool _reopenHoverBorder;
    private bool _canSwitchLine;
    private TimelineCellBase _currentUi;
    private readonly TTAsyncLocker _locker = new TTAsyncLocker(1, 1);
    private DateTime _originStart;
    private DateTime? _originEnd;
    private double _originLeft;
    private int _originLine;
    private System.Windows.Point _originPoint;
    private System.Windows.Point _startPoint;
    private bool _startDrag;
    private int? _startDragLine;
    private bool _showTextTips;
    internal TimelineCellFloating Root;
    internal Grid Cell;
    internal Rectangle BackgroundBorder;
    internal Grid InfoTile;
    internal EmjTextBlock TitleText;
    internal TaskCircleProgress Progress;
    internal Border AvatarBorder;
    internal Thumb StartDateThumb;
    internal Thumb EndDateThumb;
    internal Popup TipsPopup;
    internal Border TipsPanel;
    private bool _contentLoaded;

    public bool Arranging
    {
      get => (bool) this.GetValue(TimelineCellFloating.ArrangingProperty);
      set => this.SetValue(TimelineCellFloating.ArrangingProperty, (object) value);
    }

    public TimelineCellFloating() => this.InitializeComponent();

    private async void OnEndDateThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
      TimelineCellFloating timelineCellFloating = this;
      if (!timelineCellFloating.CheckEditable())
        return;
      timelineCellFloating.timelineModel.ClearBatchSelect();
      await timelineCellFloating.UpdateModel();
      if (timelineCellFloating.cellModel == null)
        return;
      double left = Canvas.GetLeft((UIElement) timelineCellFloating.Cell);
      System.Windows.Point point = timelineCellFloating._currentUi.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) timelineCellFloating.Container?.MainScroll);
      double x = point.X;
      if (Math.Abs(left - x) > 0.1)
        Canvas.SetLeft((UIElement) timelineCellFloating.Cell, point.X);
      TimelineCellViewModel cellModel = timelineCellFloating.cellModel;
      cellModel.SetWidth();
      if (Math.Abs(cellModel.Width - timelineCellFloating.InfoTile.Width) <= 0.1)
        return;
      timelineCellFloating.InfoTile.Width = cellModel.Width;
      timelineCellFloating.Container?.MoveToolTips(new System.Windows.Point(point.X + cellModel.Width, 0.0));
    }

    private async void OnStartDateThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
      TimelineCellFloating timelineCellFloating = this;
      if (!timelineCellFloating.CheckEditable())
        return;
      timelineCellFloating.timelineModel.ClearBatchSelect();
      await timelineCellFloating.UpdateModel();
      if (timelineCellFloating.cellModel == null)
        return;
      double left1 = Canvas.GetLeft((UIElement) timelineCellFloating.Cell);
      double left2 = timelineCellFloating.cellModel.Left;
      double? horizontalOffset = timelineCellFloating.Container?.MainScroll.HorizontalOffset;
      double valueOrDefault = (horizontalOffset.HasValue ? new double?(left2 - horizontalOffset.GetValueOrDefault()) : new double?()).GetValueOrDefault();
      double num = valueOrDefault;
      if (Math.Abs(left1 - num) > 0.1)
      {
        Canvas.SetLeft((UIElement) timelineCellFloating.Cell, valueOrDefault);
        timelineCellFloating.Container?.MoveToolTips(new System.Windows.Point(valueOrDefault, 0.0));
      }
      TimelineCellViewModel cellModel = timelineCellFloating.cellModel;
      cellModel.SetWidth();
      if (Math.Abs(cellModel.Width - timelineCellFloating.InfoTile.Width) <= 0.1)
        return;
      timelineCellFloating.InfoTile.Width = cellModel.Width;
    }

    private void MoveFullDate(double xOffset)
    {
      if (this.ModelInvalid())
        return;
      Canvas.SetLeft((UIElement) this.Cell, Canvas.GetLeft((UIElement) this.Cell) + xOffset);
    }

    private async Task OnItemDragCompleted(bool openWindow)
    {
      TimelineCellFloating child = this;
      child.Container?.StopMoveView();
      child.TitleText.SetBinding(TextBox.TextProperty, "Title");
      TimelineCellViewModel currentCell = child.cellModel;
      TimelineViewModel parentModel = child.timelineModel;
      if (child.ModelInvalid())
        ;
      else if (child.IsCursorDisabled())
      {
        child.RestoreOriginProperty(currentCell);
        child.ResetCursor();
        currentCell.DragStatus = 0;
        child.timelineModel.SetBatchDragging(0, true);
        child.timelineModel.ResetBatchDates();
        child.Close();
      }
      else
      {
        child.ResetCursor();
        child.ResetHoverBorder();
        child.TipsPopup.IsOpen = false;
        if (openWindow)
        {
          parentModel.Hovering = false;
          if (Utils.IfCtrlPressed())
          {
            TaskDetailWindow.TryCloseWindow();
            parentModel.ChangeBatchSelectStatus(currentCell);
            bool hide = child.cellModel.BatchSelected || child.cellModel.DisplayModel.Type != DisplayType.Task && child.cellModel.DisplayModel.Type != DisplayType.Agenda || !child.cellModel.Editable;
            child.SetThumbVisibility(hide);
          }
          else if (child._currentUi == null)
            ;
          else
          {
            // ISSUE: explicit non-virtual call
            if (!__nonvirtual (child.IsMouseOver))
              ;
            else
              await child._currentUi?.OpenDetailWindow();
          }
        }
        else
        {
          if (parentModel != null && currentCell.DisplayModel.IsTaskOrNote)
          {
            currentCell.DragStatus = 0;
            List<TaskModel> tasks;
            if (parentModel.BatchSelect)
            {
              child.Close();
              List<string> selectedIds = parentModel.GetSelectedTaskIds();
              tasks = (List<TaskModel>) null;
              List<TaskDetailItemModel> checkItems = (List<TaskDetailItemModel>) null;
              tasks = await TaskDao.GetTaskAndChildrenInBatch(TaskDao.GetTreeTopIds(selectedIds, string.Empty));
              checkItems = await TaskDetailItemDao.GetCheckItemsInTaskIds((ICollection<string>) selectedIds);
              TimelineViewModel timelineViewModel = parentModel;
              int? line1;
              if (child._startDragLine.HasValue)
              {
                int? startDragLine = child._startDragLine;
                int line2 = currentCell.Line;
                if (!(startDragLine.GetValueOrDefault() == line2 & startDragLine.HasValue))
                {
                  line1 = new int?(currentCell.Line);
                  goto label_19;
                }
              }
              line1 = new int?();
label_19:
              await timelineViewModel.OnBatchDragDrop(line1);
              child.timelineModel?.SetBatchDragging(0, true);
              bool show = false;
              if (parentModel.ProjectIdentity is FilterProjectIdentity)
              {
                List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(parentModel.ProjectIdentity, selectedIds);
                List<TaskModel> taskModelList = tasks;
                // ISSUE: explicit non-virtual call
                if ((taskModelList != null ? (__nonvirtual (taskModelList.Count) > 0 ? 1 : 0) : 0) != 0 && matchedTasks.Count < selectedIds.Count)
                  show = true;
              }
              UndoToast uiElement = new UndoToast((UndoController) new TaskUndo((TaskModel) null, string.Empty, Utils.GetString("TaskHasBeenFiltered"), tasks, checkItems));
              uiElement.SetVisible(show);
              Utils.FindParent<IToastShowWindow>((DependencyObject) child).Toast((FrameworkElement) uiElement);
              selectedIds = (List<string>) null;
              tasks = (List<TaskModel>) null;
              checkItems = (List<TaskDetailItemModel>) null;
            }
            else
            {
              if (parentModel.ProjectIdentity is FilterProjectIdentity filter)
              {
                TaskModel task = await TaskDao.GetTaskById(currentCell.Id);
                if (task != null)
                {
                  tasks = await TaskDao.GetAllSubTasksById(currentCell.Id, currentCell.DisplayModel.ProjectId);
                  await Commit();
                  if (TaskViewModelHelper.GetMatchedTasks((ProjectIdentity) filter, new List<string>()
                  {
                    task.id
                  }).Count == 0)
                    child.Container?.TryToastTaskChangeUndo(task, tasks);
                  tasks = (List<TaskModel>) null;
                }
                task = (TaskModel) null;
              }
              else
                await Commit();
              filter = (FilterProjectIdentity) null;
            }
            if (parentModel.GroupByEnum == Constants.SortType.tag)
              await parentModel.UpdateGroupAsync();
            await parentModel.UpdateCellLineAsync();
            child._startDragLine = new int?();
            child.Container?.DelaySetHitVisible();
          }
          child.Close();
        }
      }

      async Task Commit()
      {
        await currentCell.CommitGroup();
        await currentCell.CommitDate((TimeData) null);
        parentModel.UpdateCellTime(currentCell, true);
      }
    }

    private async void OnItemDragCompleted(object sender, DragCompletedEventArgs e)
    {
      TimelineCellFloating timelineCellFloating1 = this;
      UserActCollectUtils.AddClickEvent("timeline", "task_action", "drag_task_length");
      TimelineCellFloating timelineCellFloating2 = timelineCellFloating1;
      int num;
      if (e.HorizontalChange != 0.0)
      {
        TimelineCellViewModel cellModel = timelineCellFloating1.cellModel;
        num = (cellModel != null ? (cellModel.Editable ? 1 : 0) : 0) == 0 ? 1 : 0;
      }
      else
        num = 1;
      await timelineCellFloating2.OnItemDragCompleted(num != 0);
    }

    private void UpdateCursor()
    {
      double x = Mouse.GetPosition((IInputElement) this).X;
      double num1 = this.timelineModel.IsArranging ? 215.0 : 0.0;
      double num2 = this.timelineModel.ShowGroup ? this.timelineModel.GroupWidth : 0.0;
      if (x > this.ActualWidth - num1 || x < num2)
        this.DisableCursor();
      else
        this.ResetCursor();
    }

    private bool IsCursorDisabled()
    {
      return this.InfoTile.Cursor == Cursors.No || this.StartDateThumb.Cursor == Cursors.No || this.EndDateThumb.Cursor == Cursors.No;
    }

    private void ResetCursor()
    {
      this.InfoTile.Cursor = Cursors.Hand;
      this.StartDateThumb.Cursor = Cursors.SizeWE;
      this.EndDateThumb.Cursor = Cursors.SizeWE;
    }

    private void DisableCursor()
    {
      this.InfoTile.Cursor = Cursors.No;
      this.StartDateThumb.Cursor = Cursors.No;
      this.EndDateThumb.Cursor = Cursors.No;
    }

    private void UpdateTipsPosition()
    {
    }

    private async Task UpdateModel()
    {
      TimelineCellFloating timelineCellFloating = this;
      if (timelineCellFloating.ModelInvalid() || !timelineCellFloating.cellModel.Editable)
        return;
      timelineCellFloating.UpdateTipsPosition();
      timelineCellFloating.UpdateCursor();
      await timelineCellFloating._locker.RunAsync(new Action(timelineCellFloating.UpdateModelInternal));
    }

    private void UpdateModelInternal()
    {
      System.Windows.Point position;
      DateTime dateTime1;
      DateTime? nullable1;
      DateTime dateTime2;
      if (this.cellModel.Operation.Contain(TimelineCellOperation.Start) || this.cellModel.Operation.Contain(TimelineCellOperation.Full))
      {
        double num1;
        if (!this.cellModel.Operation.Contain(TimelineCellOperation.Full))
        {
          position = Mouse.GetPosition((IInputElement) this.Container?.MainScroll);
          num1 = position.X;
        }
        else
          num1 = Canvas.GetLeft((UIElement) this.Cell);
        double num2 = num1;
        double? horizontalOffset = this.Container?.MainScroll.HorizontalOffset;
        int num3 = (int) Math.Round((horizontalOffset.HasValue ? new double?(num2 + horizontalOffset.GetValueOrDefault()) : new double?()).GetValueOrDefault() / this.cellModel.Parent.OneDayWidth, 0, MidpointRounding.AwayFromZero);
        DateTime dateTime3 = this.cellModel.Parent.StartDate;
        dateTime3 = dateTime3.AddDays((double) num3);
        ref DateTime local1 = ref dateTime3;
        dateTime1 = this.cellModel.StartDate;
        double hour = (double) dateTime1.Hour;
        DateTime dateTime4 = local1.AddHours(hour);
        ref DateTime local2 = ref dateTime4;
        dateTime1 = this.cellModel.StartDate;
        double minute = (double) dateTime1.Minute;
        DateTime dateTime5 = local2.AddMinutes(minute);
        int days = (dateTime5 - this.cellModel.StartDate).Days;
        if (days == 0)
          return;
        if (this.cellModel.Operation.Contain(TimelineCellOperation.Full))
        {
          if (this.cellModel.BatchSelected)
          {
            this.cellModel.Parent.OnBatchDrag(days);
            return;
          }
          DateTime? endDate = this.cellModel.EndDate;
          if (endDate.HasValue)
            this.cellModel.EndDate = new DateTime?(endDate.GetValueOrDefault().AddDays((double) days));
        }
        else
        {
          nullable1 = this.cellModel.DisplayModel.DueDate;
          int num4;
          if (nullable1.HasValue)
          {
            DateTime dateTime6 = dateTime5;
            nullable1 = this.cellModel.DisplayModel.DueDate;
            num4 = nullable1.HasValue ? (dateTime6 > nullable1.GetValueOrDefault() ? 1 : 0) : 0;
          }
          else
          {
            DateTime dateTime7 = dateTime5;
            nullable1 = this.cellModel.DisplayModel.StartDate;
            num4 = nullable1.HasValue ? (dateTime7 > nullable1.GetValueOrDefault() ? 1 : 0) : 0;
          }
          if (num4 != 0)
            return;
          nullable1 = this.cellModel.EndDate;
          if (nullable1.HasValue)
          {
            DateTime valueOrDefault = nullable1.GetValueOrDefault();
            if (dateTime5 >= valueOrDefault.AddDays(this.cellModel.IsAllDay ? -1.0 : 0.0))
            {
              TimelineCellViewModel cellModel = this.cellModel;
              nullable1 = new DateTime?();
              DateTime? nullable2 = nullable1;
              cellModel.EndDate = nullable2;
            }
          }
          else
          {
            TimelineCellViewModel cellModel = this.cellModel;
            DateTime dateTime8;
            if (!this.cellModel.IsAllDay)
            {
              dateTime8 = this.cellModel.StartDate;
            }
            else
            {
              dateTime2 = this.cellModel.StartDate.AddDays(1.0);
              dateTime8 = dateTime2.Date;
            }
            DateTime? nullable3 = new DateTime?(dateTime8);
            cellModel.EndDate = nullable3;
          }
        }
        this.cellModel.TrySetStartDate(new DateTime?(dateTime5));
        this.cellModel.SetLeft();
        this.UpdateHoverBorderPosition();
      }
      if (!this.cellModel.Operation.Contain(TimelineCellOperation.End))
        return;
      position = Mouse.GetPosition((IInputElement) this.InfoTile);
      double num = position.X / this.cellModel.Parent.OneDayWidth;
      if (num - (double) (int) num > 0.3)
        ++num;
      if (this.cellModel.IsAllDay)
      {
        if (Math.Abs(num) == 0.0)
          return;
      }
      else
        --num;
      if (num < 0.0)
        return;
      DateTime? nullable4;
      ref DateTime? local3 = ref nullable4;
      dateTime2 = this.cellModel.StartDate;
      dateTime2 = dateTime2.Date;
      DateTime dateTime9 = dateTime2.AddDays((double) (int) num);
      local3 = new DateTime?(dateTime9);
      nullable1 = this.cellModel.EndDate;
      if (nullable1.HasValue)
      {
        DateTime valueOrDefault = nullable1.GetValueOrDefault();
        DateTime? nullable5;
        if (!nullable4.HasValue)
        {
          nullable1 = new DateTime?();
          nullable5 = nullable1;
        }
        else
        {
          dateTime2 = nullable4.GetValueOrDefault();
          dateTime2 = dateTime2.AddHours((double) valueOrDefault.Hour);
          nullable5 = new DateTime?(dateTime2.AddMinutes((double) valueOrDefault.Minute));
        }
        nullable4 = nullable5;
      }
      else
      {
        DateTime? nullable6;
        if (!nullable4.HasValue)
        {
          nullable1 = new DateTime?();
          nullable6 = nullable1;
        }
        else
        {
          dateTime2 = nullable4.GetValueOrDefault();
          ref DateTime local4 = ref dateTime2;
          dateTime1 = this.cellModel.StartDate;
          double hour = (double) dateTime1.Hour;
          dateTime2 = local4.AddHours(hour);
          ref DateTime local5 = ref dateTime2;
          dateTime1 = this.cellModel.StartDate;
          double minute = (double) dateTime1.Minute;
          nullable6 = new DateTime?(local5.AddMinutes(minute));
        }
        nullable4 = nullable6;
      }
      DateTime? nullable7;
      if (!nullable4.HasValue)
      {
        nullable7 = new DateTime?();
      }
      else
      {
        dateTime1 = nullable4.GetValueOrDefault();
        nullable7 = new DateTime?(dateTime1.AddDays(this.cellModel.IsAllDay ? -1.0 : 0.0));
      }
      nullable1 = nullable7;
      dateTime2 = this.cellModel.StartDate;
      if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() <= dateTime2 ? 1 : 0) : 0) != 0)
        nullable4 = new DateTime?();
      this.cellModel.EndDate = nullable4;
      this.UpdateHoverBorderPosition();
    }

    private void UpdateHoverBorderPosition()
    {
      DateTime? endDate = this.cellModel.EndDate;
      ref DateTime? local = ref endDate;
      this.timelineModel.HoverStartEndTuple = new Tuple<DateTime, DateTime>(this.cellModel.StartDate.Date, local.HasValue ? local.GetValueOrDefault().AddSeconds(-1.0).Date : this.cellModel.StartDate.Date);
    }

    private void OnItemDragStarted(object sender, DragStartedEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.Tag is TimelineCellOperation tag) || this.ModelInvalid() || !this.cellModel.Editable)
        return;
      if (tag != TimelineCellOperation.Full)
        TaskDetailWindow.TryCloseWindow(true);
      else
        this.Container?.TryCloseToolTips(true);
      this.cellModel.Operation = this.cellModel.Operation.SetPos(tag);
      this.SetHoverBorder();
      this.Container?.StartMoveView();
      this.UpdateTipsPosition();
      this.timelineModel.Hovering = true;
    }

    private void OnMouseRightUp(object sender, MouseButtonEventArgs e)
    {
      TimelineCellViewModel cellModel = this.cellModel;
      if (this._startDrag || cellModel == null)
        return;
      if (cellModel.DisplayModel.IsTaskOrNote && !cellModel.Editable)
        this.Container?.TryToastString(Utils.GetString("NoEditingPermission"));
      else if (cellModel.DisplayModel.Type == DisplayType.CheckItem)
        this.Container?.TryToastString(Utils.GetString("OperationNotSupport"));
      else
        this.Container?.ShowMenuPopup(cellModel);
    }

    private void SaveOriginProperty(TimelineCellViewModel model)
    {
      this._originStart = model.StartDate;
      this._originEnd = model.EndDate;
      this._originLeft = model.Left;
      this._originLine = model.Line;
    }

    private void RestoreOriginProperty(TimelineCellViewModel model)
    {
      model.TrySetStartDate(new DateTime?(this._originStart));
      model.EndDate = this._originEnd;
      model.Left = this._originLeft;
      model.Line = this._originLine;
    }

    private void SetThumbVisibility(bool hide)
    {
      this.StartDateThumb.Visibility = hide ? Visibility.Hidden : Visibility.Visible;
      this.StartDateThumb.IsHitTestVisible = !hide;
      this.EndDateThumb.Visibility = hide ? Visibility.Hidden : Visibility.Visible;
      this.EndDateThumb.IsHitTestVisible = !hide;
    }

    public void Open(TimelineCellViewModel timelineCellViewModel, TimelineCellBase ui)
    {
      if (ui == null || timelineCellViewModel == null || timelineCellViewModel == this.DataContext)
        return;
      this._currentUi = ui;
      this.Arranging = false;
      this.RemoveAndResetDataContext(timelineCellViewModel);
      if (this.ModelInvalid())
      {
        Utils.Toast("Invalid Model");
      }
      else
      {
        if (this.cellModel.PlayingAnimation)
          return;
        this.SaveOriginProperty(timelineCellViewModel);
        this._canSwitchLine = this.timelineModel.TimelineSortOption.groupBy != "tag" && (!timelineCellViewModel.BatchSelected || !this.timelineModel.IsBatchSelectOverGroup());
        double width = Math.Max(this.timelineModel.OneDayWidth - 6.0, this.cellModel.Width);
        this.InfoTile.Width = width;
        this.SetHoverBorder();
        this.SetTextProgressAndAvatarWidth(timelineCellViewModel);
        System.Windows.Point point = ui.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this.Container?.MainScroll);
        Canvas.SetLeft((UIElement) this.Cell, point.X);
        Canvas.SetTop((UIElement) this.Cell, point.Y);
        this.Visibility = Visibility.Visible;
        DelayActionHandlerCenter.TryDoAction("ShowTimelineFloatingThumb", new EventHandler(ShowThumb), 200);

        void ShowThumb(object sender, EventArgs eventArgs)
        {
          this.Dispatcher.Invoke((Action) (() =>
          {
            if (this.cellModel == null)
              return;
            this.SetThumbVisibility(this.cellModel.BatchSelected || this.cellModel.DisplayModel.Type != DisplayType.Task && this.cellModel.DisplayModel.Type != DisplayType.Agenda || !this.cellModel.Editable);
            if (width >= 24.0 || this.cellModel.BatchSelected)
              return;
            double num = width <= 24.0 ? 5.0 : 0.0;
            width = Math.Max(width, 24.0);
            this.InfoTile.Width = width;
            Canvas.SetLeft((UIElement) this.Cell, point.X - num);
            this.SetTextProgressAndAvatarWidth(timelineCellViewModel);
          }));
        }
      }
    }

    public void OpenArrange(TimelineCellViewModel timelineCellViewModel)
    {
      this.Arranging = true;
      this.RemoveAndResetDataContext(timelineCellViewModel);
      this.SaveOriginProperty(timelineCellViewModel);
      if (this.ModelInvalid())
      {
        Utils.Toast("Invalid Model");
      }
      else
      {
        this.timelineModel.Hovering = true;
        bool hide = this.cellModel.DisplayModel.Kind == "NOTE";
        this.SetThumbVisibility(hide);
        this._canSwitchLine = true;
        this.Container.SwitchArrangeAndFloatingIndex(true);
        if (!this.cellModel.DisplayModel.StartDate.HasValue && !hide)
          this.InfoTile.Width = this.timelineModel.OneDayWidth * (double) this.timelineModel.NewTaskDefaultDays;
        else
          this.InfoTile.Width = Math.Max(this.timelineModel.OneDayWidth - 6.0, Math.Max(this.cellModel.Width, 24.0));
        System.Windows.Point position = Mouse.GetPosition((IInputElement) this.Container.MainScroll);
        position.X -= this.InfoTile.Width / 2.0;
        Canvas.SetLeft((UIElement) this.Cell, position.X);
        Canvas.SetTop((UIElement) this.Cell, position.Y - 20.0);
        this.Visibility = Visibility.Visible;
        this.SetTextProgressAndAvatarWidth(timelineCellViewModel);
        if (!this.cellModel.DisplayModel.StartDate.HasValue)
        {
          int dayOffsetByPoint = this.Container.GetDayOffsetByPoint(position);
          if (hide)
          {
            this.cellModel.TrySetStartDate(new DateTime?(this.timelineModel.StartDate.AddDays((double) dayOffsetByPoint)));
            this.cellModel.EndDate = new DateTime?();
          }
          else if (this.timelineModel.NewTaskDefaultDays > 1)
          {
            int num = this.timelineModel.NewTaskDefaultDays / 2;
            TimelineCellViewModel timelineCellViewModel1 = timelineCellViewModel;
            DateTime startDate = this.timelineModel.StartDate;
            DateTime? time = new DateTime?(startDate.AddDays((double) (dayOffsetByPoint - num)));
            timelineCellViewModel1.TrySetStartDate(time);
            TimelineCellViewModel timelineCellViewModel2 = timelineCellViewModel;
            startDate = timelineCellViewModel.StartDate;
            DateTime? nullable = new DateTime?(startDate.AddDays((double) this.timelineModel.NewTaskDefaultDays));
            timelineCellViewModel2.EndDate = nullable;
            timelineCellViewModel.IsAllDay = true;
          }
          else
            timelineCellViewModel.TrySetStartDate(new DateTime?(this.timelineModel.StartDate.AddDays((double) dayOffsetByPoint)));
        }
        MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
        e.RoutedEvent = Mouse.MouseDownEvent;
        this.OnFullMouseDown((object) this.InfoTile, e);
      }
    }

    private void SetTextProgressAndAvatarWidth(TimelineCellViewModel model)
    {
      if (model == null)
        return;
      int num1 = 14;
      double num2 = this.InfoTile.Width;
      bool flag1 = model.AvatarUrl != "-1" && !string.IsNullOrEmpty(model.AvatarUrl);
      bool flag2 = model.Progress > 0 && model.Status == 0;
      double num3 = string.IsNullOrEmpty(model.Title) ? 0.0 : Utils.MeasureString(model.Title, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0).Width;
      if (num3 < 70.0)
      {
        if (flag2 && num2 <= 40.0)
          flag2 = num2 - (double) num1 - num3 >= 16.0;
        if (flag1 && num2 <= 70.0)
          flag1 = num2 - (double) num1 - num3 - (flag2 ? 16.0 : 0.0) >= 22.0;
      }
      else
      {
        flag2 = flag2 && num2 > 40.0;
        flag1 = flag1 && num2 > 70.0;
      }
      int num4 = num1 + (flag2 ? 16 : 0) + (flag1 ? 22 : 0);
      if (!double.IsPositiveInfinity(num2))
      {
        num2 -= (double) num4;
        if (num2 < 6.0)
          num2 = 0.0;
      }
      this.TitleText.MaxWidth = !model.Inline ? Math.Min(num2, 120.0) : num2;
      this._showTextTips = this.TitleText.MaxWidth < num3;
      this.Progress.Visibility = flag2 ? Visibility.Visible : Visibility.Collapsed;
      this.AvatarBorder.Visibility = flag1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RemoveAndResetDataContext(TimelineCellViewModel newContext)
    {
      if (this.DataContext is TimelineCellViewModel dataContext && dataContext != newContext)
        dataContext.Operation = dataContext.Operation.Remove(TimelineCellOperation.Hover);
      this.DataContext = (object) newContext;
      newContext.Operation = newContext.Operation.Add(TimelineCellOperation.Hover);
    }

    public void TryClose(TimelineCellViewModel vm)
    {
      if (this.timelineModel != null && this.timelineModel.Hovering || this.DataContext != vm || this.IsMouseOver)
        return;
      this.Close();
    }

    public void Close()
    {
      TimelineViewModel timelineModel = this.timelineModel;
      if (this.DataContext is TimelineCellViewModel dataContext)
      {
        dataContext.Operation = dataContext.Operation.RemovePos() & dataContext.Operation.Remove(TimelineCellOperation.Hover);
        this.Container?.TryCloseToolTips();
        this.Container?.SwitchArrangeAndFloatingIndex(false);
        this.DataContext = (object) null;
      }
      this.ResetHoverBorder();
      this.Visibility = Visibility.Collapsed;
      this.TipsPopup.IsOpen = false;
      this._startDrag = false;
      this.StartDateThumb.Visibility = Visibility.Hidden;
      this.EndDateThumb.Visibility = Visibility.Hidden;
      if (timelineModel == null)
        return;
      timelineModel.Hovering = false;
    }

    private async void OnMouseLeave(object sender, MouseEventArgs e)
    {
      TimelineCellFloating timelineCellFloating = this;
      DelayActionHandlerCenter.RemoveAction("ShowTimelineFloatingThumb");
      if (timelineCellFloating.ModelInvalid())
        timelineCellFloating.Close();
      else if (timelineCellFloating.timelineModel.Editing)
      {
        timelineCellFloating.Close();
      }
      else
      {
        while (true)
        {
          TimelineViewModel timelineModel = timelineCellFloating.timelineModel;
          if ((timelineModel != null ? (timelineModel.Hovering ? 1 : 0) : 0) != 0)
            await Task.Delay(100);
          else
            break;
        }
        timelineCellFloating.Close();
      }
    }

    private void SetHoverBorder()
    {
      this._reopenHoverBorder = true;
      this.UpdateHoverBorderPosition();
      this.Container.HoverHeader.Visibility = Visibility.Visible;
    }

    private void ResetHoverBorder()
    {
      this._reopenHoverBorder = false;
      Task.Delay(150).ContinueWith((Action<Task>) (t =>
      {
        if (this._reopenHoverBorder)
          return;
        this.Dispatcher.Invoke((Action) (() => this.Container?.RemoveHoverHeader()));
      }));
    }

    private void OnTextMouseEnter(object sender, MouseEventArgs e)
    {
      if (!this._showTextTips)
        return;
      Task.Delay(200).ContinueWith((Action<Task>) (t => ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        if (!this.InfoTile.IsMouseOver || this.StartDateThumb.IsMouseOver || this.EndDateThumb.IsMouseOver || e.LeftButton == MouseButtonState.Pressed)
          return;
        this.Container?.OpenToolTips((FrameworkElement) this.Cell);
      }))));
    }

    private void OnTextMouseLeave(object sender, MouseEventArgs e)
    {
      this.Container?.TryCloseToolTips();
    }

    private void OnTextMouseMove(object sender, MouseEventArgs e)
    {
    }

    private void OnItemDragStartDateMouseEnter(object sender, MouseEventArgs e)
    {
      this.cellModel.Operation = this.cellModel.Operation.SetPos(TimelineCellOperation.Start);
      Task.Delay(20).ContinueWith((Action<Task>) (t => ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        if (!this.StartDateThumb.IsMouseOver)
          return;
        this.Container?.OpenToolTips((FrameworkElement) this.Cell, true);
      }))));
    }

    private void OnItemDragEndDateMouseEnter(object sender, MouseEventArgs e)
    {
      this.cellModel.Operation = this.cellModel.Operation.SetPos(TimelineCellOperation.End);
      Task.Delay(20).ContinueWith((Action<Task>) (t => ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        if (!this.EndDateThumb.IsMouseOver)
          return;
        this.Container?.OpenToolTips((FrameworkElement) this.Cell, true);
      }))));
    }

    private void OnItemDragStartDateEndDateMouseLeave(object sender, MouseEventArgs e)
    {
      this.Container?.TryCloseToolTips(true);
    }

    private void OnFloatingMouseWheel(object sender, MouseWheelEventArgs e)
    {
      this.DealWithMainScrollMove(Keyboard.IsKeyDown(Key.LeftShift), e.Delta);
      this.Container?.OnPreviewMouseWheel(sender, e);
    }

    public void DealWithMainScrollMove(bool hor, int offset)
    {
      if (hor)
        Canvas.SetLeft((UIElement) this.Cell, Canvas.GetLeft((UIElement) this.Cell) + (double) offset);
      else
        this.Close();
    }

    private void OnFullMouseDown(object sender, MouseButtonEventArgs e)
    {
      this.timelineModel.SetItemEditing(true);
      this.OnItemDragStarted(sender, (DragStartedEventArgs) null);
      this._startPoint = e.GetPosition((IInputElement) this);
      this._originPoint = this._startPoint;
      this._startDrag = true;
      if (this.cellModel != null)
        this._startDragLine = this.cellModel.BatchSelected ? new int?(this.cellModel.Line) : new int?();
      Mouse.Capture((IInputElement) this.InfoTile);
    }

    private async void OnFullMouseMove(object sender, MouseEventArgs e)
    {
      TimelineCellFloating relativeTo = this;
      if (!relativeTo._startDrag)
        return;
      System.Windows.Point newPoint = e.GetPosition((IInputElement) relativeTo);
      if (Math.Abs(newPoint.X - relativeTo._startPoint.X) < 3.0 && Math.Abs(newPoint.Y - relativeTo._startPoint.Y) < 3.0)
        return;
      TaskDetailWindow.TryCloseWindow(true);
      if (!relativeTo.CheckEditable())
      {
        relativeTo.Container?.StopMoveView();
        Mouse.Capture((IInputElement) null);
      }
      else
      {
        double xOffset = newPoint.X - relativeTo._startPoint.X;
        double num = newPoint.Y - relativeTo._startPoint.Y;
        if (relativeTo.ModelInvalid() || !relativeTo.cellModel.Editable)
          return;
        if (!relativeTo.cellModel.BatchSelected)
        {
          relativeTo.timelineModel.ClearBatchSelect();
          relativeTo.cellModel.DragStatus = 1;
        }
        else if (relativeTo.cellModel.DragStatus == 0)
        {
          relativeTo.timelineModel.SetBatchDragging(1);
          relativeTo.TitleText.Text = relativeTo.timelineModel.GetSelectedTaskCounts().ToString() + " " + Utils.GetString("CountTasks");
        }
        relativeTo.MoveFullDate(xOffset);
        if (relativeTo._canSwitchLine && num != 0.0)
        {
          Canvas.SetTop((UIElement) relativeTo.Cell, Canvas.GetTop((UIElement) relativeTo.Cell) + num);
          TimelineContainer container = relativeTo.Container;
          int line = container != null ? container.GetLineByPoint(Mouse.GetPosition((IInputElement) relativeTo)) : -1;
          if (relativeTo.cellModel.BatchSelected && relativeTo._startDragLine.HasValue)
          {
            TimelineGroupViewModel timelineGroupViewModel1 = relativeTo.cellModel.Parent.GroupModels.LastOrDefault<TimelineGroupViewModel>((Func<TimelineGroupViewModel, bool>) (c => c.Line <= line));
            // ISSUE: reference to a compiler-generated method
            TimelineGroupViewModel timelineGroupViewModel2 = relativeTo.cellModel.Parent.GroupModels.LastOrDefault<TimelineGroupViewModel>(new Func<TimelineGroupViewModel, bool>(relativeTo.\u003COnFullMouseMove\u003Eb__54_1));
            if (timelineGroupViewModel1 != null && timelineGroupViewModel1.Id != timelineGroupViewModel2?.Id)
            {
              relativeTo.timelineModel.SetBatchDragging(2);
              relativeTo.cellModel.DragStatus = 1;
              relativeTo.cellModel.Line = line;
            }
            else
            {
              relativeTo.timelineModel.SetBatchDragging(1);
              relativeTo.cellModel.DragStatus = 1;
              relativeTo.cellModel.Line = relativeTo._startDragLine.Value;
            }
          }
          else
          {
            relativeTo.cellModel.DragStatus = 1;
            relativeTo.cellModel.Line = line;
          }
        }
        if (xOffset != 0.0)
          await relativeTo.UpdateModel();
        relativeTo._startPoint = newPoint;
      }
    }

    private bool CheckEditable()
    {
      TimelineCellViewModel cellModel = this.cellModel;
      bool flag = true;
      if (this.ModelInvalid())
      {
        flag = false;
      }
      else
      {
        switch (cellModel.DisplayModel.Type)
        {
          case DisplayType.Task:
          case DisplayType.Note:
            if (!cellModel.Editable)
            {
              this.Container?.TryToastString(Utils.GetString("NoEditingPermission"));
              flag = false;
              break;
            }
            break;
          case DisplayType.CheckItem:
          case DisplayType.Event:
            this.Container?.TryToastString(Utils.GetString("OptionNotSupport"));
            flag = false;
            break;
        }
      }
      if (!flag)
      {
        Mouse.Capture((IInputElement) null);
        this._startDrag = false;
        this.timelineModel.Hovering = false;
      }
      return flag;
    }

    private async void OnFullMouseUp(object sender, MouseButtonEventArgs e)
    {
      TimelineCellFloating timelineCellFloating = this;
      Mouse.Capture((IInputElement) null);
      if (timelineCellFloating.cellModel == null)
        return;
      timelineCellFloating._startDrag = false;
      UserActCollectUtils.AddClickEvent("timeline", "task_action", timelineCellFloating.Arranging ? "drag_arrangement" : "drag_task_position");
      await timelineCellFloating.OnItemDragCompleted(timelineCellFloating._startPoint == timelineCellFloating._originPoint || timelineCellFloating.timelineModel != null && !timelineCellFloating.cellModel.Editable);
    }

    public async Task OnDragMoveOutSide()
    {
      TimelineCellFloating timelineCellFloating = this;
      TimelineCellViewModel model;
      if (timelineCellFloating.ModelInvalid())
      {
        model = (TimelineCellViewModel) null;
      }
      else
      {
        model = timelineCellFloating.cellModel;
        if (model == null)
          model = (TimelineCellViewModel) null;
        else if (!model.Editable)
        {
          model = (TimelineCellViewModel) null;
        }
        else
        {
          switch (model.Operation.GetPos())
          {
            case TimelineCellOperation.Start:
              await timelineCellFloating.UpdateModel();
              Grid cell = timelineCellFloating.Cell;
              double left = model.Left;
              double? horizontalOffset = timelineCellFloating.Container?.MainScroll.HorizontalOffset;
              double valueOrDefault = (horizontalOffset.HasValue ? new double?(left - horizontalOffset.GetValueOrDefault()) : new double?()).GetValueOrDefault();
              Canvas.SetLeft((UIElement) cell, valueOrDefault);
              model.SetWidth();
              timelineCellFloating.InfoTile.Width = model.Width;
              model = (TimelineCellViewModel) null;
              break;
            case TimelineCellOperation.Full:
              await timelineCellFloating.UpdateModel();
              model = (TimelineCellViewModel) null;
              break;
            case TimelineCellOperation.End:
              System.Windows.Point? nullable = timelineCellFloating._currentUi?.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) timelineCellFloating.Container?.MainScroll);
              if (!nullable.HasValue)
              {
                model = (TimelineCellViewModel) null;
                break;
              }
              Canvas.SetLeft((UIElement) timelineCellFloating.Cell, nullable.Value.X);
              await timelineCellFloating.UpdateModel();
              model.SetWidth();
              timelineCellFloating.InfoTile.Width = model.Width;
              model = (TimelineCellViewModel) null;
              break;
            default:
              model = (TimelineCellViewModel) null;
              break;
          }
        }
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/timeline/timelinecellfloating.xaml", UriKind.Relative));
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
          this.Root = (TimelineCellFloating) target;
          break;
        case 2:
          this.Cell = (Grid) target;
          this.Cell.MouseRightButtonUp += new MouseButtonEventHandler(this.OnMouseRightUp);
          this.Cell.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
          break;
        case 3:
          this.BackgroundBorder = (Rectangle) target;
          break;
        case 4:
          this.InfoTile = (Grid) target;
          this.InfoTile.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnFullMouseDown);
          this.InfoTile.MouseMove += new MouseEventHandler(this.OnFullMouseMove);
          this.InfoTile.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFullMouseUp);
          this.InfoTile.MouseEnter += new MouseEventHandler(this.OnTextMouseEnter);
          this.InfoTile.MouseLeave += new MouseEventHandler(this.OnTextMouseLeave);
          break;
        case 5:
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnTextMouseMove);
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnTextMouseEnter);
          ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnTextMouseLeave);
          break;
        case 6:
          this.TitleText = (EmjTextBlock) target;
          break;
        case 7:
          this.Progress = (TaskCircleProgress) target;
          break;
        case 8:
          this.AvatarBorder = (Border) target;
          break;
        case 9:
          this.StartDateThumb = (Thumb) target;
          this.StartDateThumb.DragDelta += new DragDeltaEventHandler(this.OnStartDateThumbDragDelta);
          this.StartDateThumb.DragCompleted += new DragCompletedEventHandler(this.OnItemDragCompleted);
          this.StartDateThumb.DragStarted += new DragStartedEventHandler(this.OnItemDragStarted);
          this.StartDateThumb.MouseEnter += new MouseEventHandler(this.OnItemDragStartDateMouseEnter);
          this.StartDateThumb.MouseLeave += new MouseEventHandler(this.OnItemDragStartDateEndDateMouseLeave);
          break;
        case 10:
          this.EndDateThumb = (Thumb) target;
          this.EndDateThumb.DragDelta += new DragDeltaEventHandler(this.OnEndDateThumbDragDelta);
          this.EndDateThumb.DragCompleted += new DragCompletedEventHandler(this.OnItemDragCompleted);
          this.EndDateThumb.DragStarted += new DragStartedEventHandler(this.OnItemDragStarted);
          this.EndDateThumb.MouseEnter += new MouseEventHandler(this.OnItemDragEndDateMouseEnter);
          this.EndDateThumb.MouseLeave += new MouseEventHandler(this.OnItemDragStartDateEndDateMouseLeave);
          break;
        case 11:
          this.TipsPopup = (Popup) target;
          break;
        case 12:
          this.TipsPanel = (Border) target;
          this.TipsPanel.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
