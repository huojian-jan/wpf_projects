// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskOperationDialog
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
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views
{
  public class TaskOperationDialog : UserControl, ITabControl, IComponentConnector
  {
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof (SelectedIndex), typeof (int), typeof (TaskOperationDialog), new PropertyMetadata((object) -1, (PropertyChangedCallback) null));
    public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(nameof (SelectedDate), typeof (string), typeof (TaskOperationDialog), new PropertyMetadata((object) string.Empty, (PropertyChangedCallback) null));
    public static readonly DependencyProperty SelectedPriorityProperty = DependencyProperty.Register(nameof (SelectedPriority), typeof (int), typeof (TaskOperationDialog), new PropertyMetadata((object) 0, (PropertyChangedCallback) null));
    private const string Today = "today";
    private const string Tomorrow = "tomorrow";
    private const string NextWeek = "nextweek";
    private const string Custom = "custom";
    private const string Clear = "clear";
    private readonly Popup _popup;
    private SelectDateDialog _dueDateDialog;
    private DateTime? _completeTime;
    private static Popup _lastOne;
    private bool _pomoPopupShow;
    private bool _changeDatePopupShow;
    private SetAssigneeDialog _assignDialog;
    private bool _assignPopupShow;
    private bool _setProjectPopupShow;
    private bool _tagPopupShow;
    private ProjectOrGroupPopup _projectOrGroupPopup;
    private BatchSetTagControl _batchSetTagCtrl;
    private readonly OperationExtra _extra;
    private System.Windows.Point _position;
    private readonly PopupLocationInfo _projectPopupTracker = new PopupLocationInfo();
    private readonly PopupLocationInfo _tagPopupTracker = new PopupLocationInfo();
    private readonly PopupLocationInfo _pomoPopupTracker = new PopupLocationInfo();
    private readonly PopupLocationInfo _assignPopupTracker = new PopupLocationInfo();
    private readonly List<PopupLocationInfo> _popupTrackers = new List<PopupLocationInfo>();
    internal TaskOperationDialog Root;
    internal StackPanel Container;
    internal StackPanel DateStack;
    internal Grid DatePanel;
    internal ColumnDefinition FourthColumn;
    internal ColumnDefinition SixthColumn;
    internal Border TodayGrid;
    internal Border TomorrowGrid;
    internal Border NextWeekGrid;
    internal Border SkipGrid;
    internal Border SetDateBorder;
    internal Border ClearDateBorder;
    internal Grid PriorityGrid;
    internal Border PriorityHighGrid;
    internal Border PriorityMediumGrid;
    internal Border PriorityLowGrid;
    internal Border PriorityNoGrid;
    internal OptionItemWithImageIcon CreateSubTaskButton;
    internal OptionItemWithImageIcon PinButton;
    internal OptionItemWithImageIcon AbandonedButton;
    internal OptionItemWithImageIcon ReopenButton;
    internal Grid MoveButton;
    internal OptionCheckBox ProjectItem;
    internal EscPopup SetProjectPopup;
    internal Grid AssignBtn;
    internal OptionCheckBox AssignItem;
    internal EscPopup SetAssigneePopup;
    internal Grid TagGrid;
    internal OptionCheckBox TagItem;
    internal EscPopup SetTagPopup;
    internal StackPanel PomoPanel;
    internal Grid PomoGrid;
    internal OptionCheckBox PomoItem;
    internal EscPopup TaskPomoPopup;
    internal TaskPomoSetDialog TaskPomoSetDialog;
    internal OptionItemWithImageIcon CopyButton;
    internal OptionItemWithImageIcon CopyLinkGrid;
    internal OptionItemWithImageIcon CopyTextGrid;
    internal OptionItemWithImageIcon MergeBtn;
    internal Grid ChangeCompleteDate;
    internal OptionItemWithImageIcon CompleteItem;
    internal EscPopup ChangeDatePopup;
    internal Line DivideLine;
    internal OptionItemWithImageIcon SwitchButton;
    private bool _contentLoaded;

    public int SelectedIndex
    {
      get => (int) this.GetValue(TaskOperationDialog.SelectedIndexProperty);
      set
      {
        this.SetValue(TaskOperationDialog.SelectedIndexProperty, (object) value);
        this.SetItemTabSelected();
      }
    }

    public string SelectedDate
    {
      get => (string) this.GetValue(TaskOperationDialog.SelectedDateProperty);
      set => this.SetValue(TaskOperationDialog.SelectedDateProperty, (object) value);
    }

    public int SelectedPriority
    {
      get => (int) this.GetValue(TaskOperationDialog.SelectedPriorityProperty);
      set => this.SetValue(TaskOperationDialog.SelectedPriorityProperty, (object) value);
    }

    public event EventHandler CreateSubTask;

    public event EventHandler Copied;

    public event EventHandler LinkCopied;

    public event EventHandler Deleted;

    public event EventHandler AbandonOrReopen;

    public event EventHandler TextCopied;

    public event EventHandler<int> PrioritySelect;

    public event EventHandler<AvatarInfo> AssigneeSelect;

    public event EventHandler<SelectableItemViewModel> ProjectSelect;

    public event EventHandler<TimeData> TimeSelect;

    public event EventHandler<DateTime> QuickDateSelect;

    public event EventHandler<TagSelectData> TagsSelect;

    public event EventHandler CustomDateSelect;

    public event EventHandler TimeClear;

    public event EventHandler SkipCurrentRecurrence;

    public event EventHandler Merge;

    public event EventHandler StartPomo;

    public event EventHandler Closed;

    public event EventHandler<string> Toast;

    public event EventHandler<DateTime> CompleteDateChanged;

    public event EventHandler<bool> Disappear;

    public event EventHandler SwitchTaskOrNote;

    public event EventHandler OpenSticky;

    public event EventHandler<bool> Starred;

    public TaskOperationDialog(OperationExtra extra, UIElement element = null)
    {
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.Placement = PlacementMode.Mouse;
      escPopup.VerticalOffset = 0.0;
      escPopup.HorizontalOffset = 5.0;
      escPopup.PlacementTarget = element;
      this._popup = (Popup) escPopup;
      if (TaskOperationDialog._lastOne != null)
        TaskOperationDialog._lastOne.IsOpen = false;
      TaskOperationDialog._lastOne = this._popup;
      this._extra = extra;
      TimeData timeModel = extra.TimeModel;
      if ((timeModel != null ? (timeModel.StartDate.HasValue ? 1 : 0) : 0) != 0)
        extra.TimeModel.IsDefault = false;
      this.InitializeComponent();
      if (!extra.ShowCopy)
        this.CopyButton.Visibility = Visibility.Collapsed;
      if (!extra.ShowCopyLink)
        this.CopyLinkGrid.Visibility = Visibility.Collapsed;
      if (!extra.ShowCreateSubTask || extra.TaskType != TaskType.Task)
        this.CreateSubTaskButton.Visibility = Visibility.Collapsed;
      if (extra.TaskType != TaskType.Task)
      {
        this.PriorityGrid.Visibility = Visibility.Collapsed;
        this.PomoPanel.Visibility = Visibility.Collapsed;
      }
      if (extra.TaskType == TaskType.Note)
      {
        this.SwitchButton.Content = Utils.GetString("ConvertToTask");
        this.SwitchButton.SetResourceReference(OptionItemWithImageIcon.ImageSourceProperty, (object) "SwitchTaskDrawingImage");
      }
      if (!extra.CanSwitch)
        this.SwitchButton.Visibility = extra.ShowSwitch ? Visibility.Visible : Visibility.Collapsed;
      if (!extra.ShowMerge || extra.TaskType != TaskType.Task)
        this.MergeBtn.Visibility = Visibility.Collapsed;
      if (!extra.ShowPomo || !LocalSettings.Settings.EnableFocus)
        this.PomoPanel.Visibility = Visibility.Collapsed;
      if (!extra.ShowDate)
        this.DateStack.Visibility = Visibility.Collapsed;
      this.SkipGrid.Visibility = !extra.ShowSkip ? Visibility.Collapsed : Visibility.Visible;
      this.FourthColumn.Width = extra.ShowSkip ? new GridLength(1.0, GridUnitType.Star) : new GridLength(0.0);
      this.SixthColumn.Width = !Utils.IsEmptyDate(extra.TimeModel.StartDate) ? new GridLength(1.0, GridUnitType.Star) : new GridLength(0.0);
      if (extra.ShowSkip && !Utils.IsEmptyDate(extra.TimeModel.StartDate))
        this.DatePanel.Width = 216.0;
      if (extra.ShowAssignTo)
        this.AssignBtn.Visibility = Visibility.Visible;
      if (extra.CompleteTime.HasValue)
      {
        this._completeTime = extra.CompleteTime;
        this.ChangeCompleteDate.Visibility = Visibility.Visible;
      }
      if (!extra.ShowCopy && !extra.CompleteTime.HasValue && (!extra.ShowMerge || extra.TaskType != TaskType.Task))
        this.DivideLine.Visibility = Visibility.Collapsed;
      if (extra.IsPinned.HasValue)
      {
        this.PinButton.Visibility = Visibility.Visible;
        this.PinButton.Content = Utils.GetString(extra.IsPinned.Value ? "Unpin" : "Pin");
        this.PinButton.Tag = (object) !extra.IsPinned.Value;
        this.PinButton.SetResourceReference(OptionItemWithImageIcon.ImageSourceProperty, extra.IsPinned.Value ? (object) "UnpinnedDrawingImage" : (object) "PinnedDrawingImage");
      }
      bool? isAbandoned = extra.IsAbandoned;
      if (isAbandoned.HasValue)
      {
        bool valueOrDefault = isAbandoned.GetValueOrDefault();
        this.AbandonedButton.Visibility = !valueOrDefault ? Visibility.Visible : Visibility.Collapsed;
        this.ReopenButton.Visibility = valueOrDefault ? Visibility.Visible : Visibility.Collapsed;
      }
      this.CopyTextGrid.Visibility = extra.ShowCopyText ? Visibility.Visible : Visibility.Collapsed;
      this._popup.Closed += new EventHandler(this.OnPopupClosed);
      this.Disappear += (EventHandler<bool>) (async (sender, args) =>
      {
        await Task.Delay(250);
        this.CreateSubTask = (EventHandler) null;
        this.Copied = (EventHandler) null;
        this.LinkCopied = (EventHandler) null;
        this.Deleted = (EventHandler) null;
        this.AbandonOrReopen = (EventHandler) null;
        this.PrioritySelect = (EventHandler<int>) null;
        this.AssigneeSelect = (EventHandler<AvatarInfo>) null;
        this.ProjectSelect = (EventHandler<SelectableItemViewModel>) null;
        this.TimeSelect = (EventHandler<TimeData>) null;
        this.QuickDateSelect = (EventHandler<DateTime>) null;
        this.TagsSelect = (EventHandler<TagSelectData>) null;
        this.CustomDateSelect = (EventHandler) null;
        this.TimeClear = (EventHandler) null;
        this.SkipCurrentRecurrence = (EventHandler) null;
        this.Merge = (EventHandler) null;
        this.StartPomo = (EventHandler) null;
        this.CompleteDateChanged = (EventHandler<DateTime>) null;
        this.SwitchTaskOrNote = (EventHandler) null;
        this.Disappear = (EventHandler<bool>) null;
        this.Starred = (EventHandler<bool>) null;
        this.Toast = (EventHandler<string>) null;
        this.OpenSticky = (EventHandler) null;
        this._popup.PlacementTarget = (UIElement) null;
        await Task.Delay(250);
        this.Closed = (EventHandler) null;
      });
      this._popupTrackers.Add(this._projectPopupTracker);
      this._popupTrackers.Add(this._pomoPopupTracker);
      this._popupTrackers.Add(this._tagPopupTracker);
      this._popupTrackers.Add(this._assignPopupTracker);
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Up:
          ++this.SelectedIndex;
          break;
        case Key.Down:
          --this.SelectedIndex;
          break;
      }
      this.SelectedIndex += 9;
      this.SelectedIndex /= 9;
    }

    public TaskOperationDialog() => this.InitializeComponent();

    private void OnPopupClosed(object sender, EventArgs e)
    {
      EventHandler closed = this.Closed;
      if (closed != null)
        closed((object) this, e);
      EventHandler<bool> disappear = this.Disappear;
      if (disappear != null)
        disappear((object) this, true);
      PopupStateManager.OnViewPopupClosed();
    }

    public void Show()
    {
      this._popup.Child = (UIElement) this;
      this._popup.IsOpen = true;
      PopupStateManager.OnViewPopupOpened();
    }

    public void Dismiss() => this._popup.IsOpen = false;

    private void OnInitialized(object sender, EventArgs e)
    {
      this.InitPriority();
      this.InitDate();
    }

    private void InitDate()
    {
      if (this._extra.TimeModel == null)
        this._extra.TimeModel = new TimeData();
      if (!this._extra.TimeModel.HasTime || !this._extra.TimeModel.StartDate.HasValue)
        return;
      DateTime dateTime1 = this._extra.TimeModel.StartDate.Value;
      DateTime date1 = dateTime1.Date;
      DateTime dateTime2 = DateTime.Now;
      DateTime date2 = dateTime2.Date;
      if (date1 == date2)
      {
        this.SelectedDate = "today";
      }
      else
      {
        DateTime date3 = dateTime1.Date;
        dateTime2 = DateTime.Now;
        dateTime2 = dateTime2.Date;
        DateTime dateTime3 = dateTime2.AddDays(1.0);
        if (date3 == dateTime3)
        {
          this.SelectedDate = "tomorrow";
        }
        else
        {
          DateTime date4 = dateTime1.Date;
          dateTime2 = DateTime.Now;
          dateTime2 = dateTime2.Date;
          DateTime dateTime4 = dateTime2.AddDays(7.0);
          if (!(date4 == dateTime4))
            return;
          this.SelectedDate = "nextweek";
        }
      }
    }

    private static void SetGridHighlight(Border grid)
    {
      grid.SetResourceReference(Control.BackgroundProperty, (object) "BaseColorOpacity5");
    }

    private void InitPriority() => this.SelectedPriority = this._extra.Priority;

    private void OnCopyClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      EventHandler copied = this.Copied;
      if (copied != null)
        copied((object) this, (EventArgs) null);
      UserActCollectUtils.AddClickEvent("tasklist", this._extra.InBatch ? "cm_batch_task" : (this._extra.TaskType == TaskType.Task ? "cm_single_task" : "cm_single_note"), "duplicate");
    }

    private void OnCopyLinkClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      EventHandler linkCopied = this.LinkCopied;
      if (linkCopied != null)
        linkCopied((object) this, (EventArgs) null);
      UserActCollectUtils.AddClickEvent("tasklist", this._extra.InBatch ? "cm_batch_task" : "cm_single_task", "copy_link");
    }

    private void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      EventHandler deleted = this.Deleted;
      if (deleted != null)
        deleted((object) this, (EventArgs) null);
      UserActCollectUtils.AddClickEvent("tasklist", this._extra.InBatch ? "cm_batch_task" : (this._extra.TaskType == TaskType.Task ? "cm_single_task" : "cm_single_note"), "delete");
    }

    private void PriorityGridClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      if (!(sender is Border border))
        return;
      this.OnPriorityClick(int.Parse(border.Tag.ToString()));
    }

    private void OnPriorityClick(int priority)
    {
      EventHandler<int> prioritySelect = this.PrioritySelect;
      if (prioritySelect != null)
        prioritySelect((object) this, priority);
      string data = (string) null;
      switch (priority)
      {
        case 0:
          data = "priority_none";
          break;
        case 1:
          data = "priority_low";
          break;
        case 3:
          data = "priority_medium";
          break;
        case 5:
          data = "priority_high";
          break;
      }
      UserActCollectUtils.AddClickEvent("tasklist", this._extra.InBatch ? "cm_batch_task" : "cm_single_task", data);
    }

    private async void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      TaskOperationDialog sender1 = this;
      UserActCollectUtils.AddClickEvent("tasklist", sender1._extra.InBatch ? "cm_batch_task" : (sender1._extra.TaskType == TaskType.Task ? "cm_single_task" : "cm_single_note"), "move_project");
      EventHandler<SelectableItemViewModel> projectSelect = sender1.ProjectSelect;
      if (projectSelect != null)
        projectSelect((object) sender1, e);
      await Task.Delay(250);
      sender1.CloseParentPopup();
    }

    private async void OnAssigneeSelect(object sender, AvatarInfo assignee)
    {
      TaskOperationDialog sender1 = this;
      await Task.Delay(250);
      sender1.CloseParentPopup();
      EventHandler<AvatarInfo> assigneeSelect = sender1.AssigneeSelect;
      if (assigneeSelect == null)
        return;
      assigneeSelect((object) sender1, assignee);
    }

    private void SetTimeClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Border border))
        return;
      this.OnTimeClick(border.Tag.ToString());
    }

    private void OnTimeClick(string cmd)
    {
      if (!this._extra.TimeModel.StartDate.HasValue && cmd != "custom")
        this._extra.TimeModel.StartDate = new DateTime?(DateTime.Now.Date);
      string ctype = this._extra.InBatch ? "cm_batch_task" : (this._extra.TaskType == TaskType.Task ? "cm_single_task" : "cm_single_note");
      switch (cmd)
      {
        case "today":
          UserActCollectUtils.AddClickEvent("tasklist", ctype, "date_today");
          this.OnStartDateSelect(DateTime.Today);
          break;
        case "tomorrow":
          UserActCollectUtils.AddClickEvent("tasklist", ctype, "date_tmr");
          this.OnStartDateSelect(DateTime.Today.AddDays(1.0));
          break;
        case "nextweek":
          UserActCollectUtils.AddClickEvent("tasklist", ctype, "date_n7d");
          this.OnStartDateSelect(DateTime.Today.AddDays(7.0));
          break;
        case "custom":
          UserActCollectUtils.AddClickEvent("tasklist", ctype, "date_other");
          this.SelectTimeOrReminder();
          break;
        case "clear":
          UserActCollectUtils.AddClickEvent("tasklist", ctype, "date_clear");
          this.CloseParentPopup();
          EventHandler timeClear = this.TimeClear;
          if (timeClear == null)
            break;
          timeClear((object) this, (EventArgs) null);
          break;
      }
    }

    private void OnStartDateSelect(DateTime date)
    {
      this.CloseParentPopup();
      EventHandler<DateTime> quickDateSelect = this.QuickDateSelect;
      if (quickDateSelect == null)
        return;
      quickDateSelect((object) this, date);
    }

    private async void SelectTimeOrReminder()
    {
      TaskOperationDialog sender1 = this;
      bool isNote = sender1._extra.TaskType == TaskType.Note || sender1._extra.TaskType == TaskType.TaskAndNote && sender1._extra.InNoteProject;
      SetDateDialog dia = SetDateDialog.GetDialog();
      dia.ClearEventHandle();
      dia.Clear += (EventHandler) ((sender, obj) =>
      {
        dia.TryClose();
        EventHandler timeClear = this.TimeClear;
        if (timeClear == null)
          return;
        timeClear((object) this, (EventArgs) null);
      });
      dia.Save += (EventHandler<TimeData>) ((sender, data) =>
      {
        dia.TryClose();
        EventHandler<TimeData> timeSelect = this.TimeSelect;
        if (timeSelect == null)
          return;
        timeSelect((object) this, data);
      });
      dia.Show(sender1._extra.TimeModel, new SetDateDialogArgs(isNote: isNote, target: (UIElement) sender1.DateStack, vOffset: -10.0, placement: PlacementMode.Right, showQuickDate: false));
      sender1._popup.Closed += (EventHandler) ((sender, obj) => dia.TryClose());
      EventHandler customDateSelect = sender1.CustomDateSelect;
      if (customDateSelect == null)
        return;
      customDateSelect((object) sender1, (EventArgs) null);
    }

    private void CloseParentPopup() => this._popup.IsOpen = false;

    private void OnMergeClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      EventHandler merge = this.Merge;
      if (merge != null)
        merge((object) this, (EventArgs) null);
      UserActCollectUtils.AddClickEvent("tasklist", this._extra.InBatch ? "cm_batch_task" : "cm_single_task", "merge");
    }

    private void SkipRecurrenceClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      UserActCollectUtils.AddClickEvent("tasklist", this._extra.InBatch ? "cm_batch_task" : "cm_single_task", "date_skip");
      EventHandler currentRecurrence = this.SkipCurrentRecurrence;
      if (currentRecurrence == null)
        return;
      currentRecurrence((object) this, (EventArgs) null);
    }

    private async void ClosePomoPopup(object sender, bool e)
    {
      this.TaskPomoPopup.IsOpen = false;
      await Task.Delay(250);
      this.CloseParentPopup();
    }

    private void ShowPopup(object sender, MouseEventArgs e)
    {
      if (this._popupTrackers.Any<PopupLocationInfo>((Func<PopupLocationInfo, bool>) (tracker => tracker.IsSafeShowing())))
        return;
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      if (Math.Abs(this._position.Y - position.Y) <= 4.0 && Math.Abs(this._position.X - position.X) <= 4.0)
        return;
      this._position = position;
      this.TryShowAssignPopup();
      this.TryShowSetProjectPopup();
      this.TryShowChangeDatePopup();
      this.TryShowPomoSetPopup();
      this.TryShowTagPopup();
    }

    private void TryShowTagPopup()
    {
      if (this.SetTagPopup.IsOpen)
        this._tagPopupTracker.Mark();
      if (!this.TagGrid.IsMouseOver && !this.SetTagPopup.IsMouseOver)
      {
        this.TryHideTagPopup();
      }
      else
      {
        if (this._tagPopupShow || this.SetTagPopup.IsOpen)
          return;
        this._tagPopupShow = true;
        this.DelayShowTagPopup();
      }
    }

    private async Task TryHideTagPopup()
    {
      this._tagPopupShow = false;
      if (!this.SetTagPopup.IsOpen)
        return;
      await Task.Delay(100);
      bool flag = this._tagPopupTracker.IsInSafeArea();
      if (!this.TagGrid.IsMouseOver && !this.SetTagPopup.IsMouseOver && !flag)
        this.SetTagPopup.IsOpen = false;
      else
        this._tagPopupShow = this.SetTagPopup.IsOpen;
    }

    private async Task DelayShowTagPopup(bool wait = true)
    {
      TaskOperationDialog taskOperationDialog = this;
      if (taskOperationDialog.SetTagPopup.IsOpen)
        taskOperationDialog._tagPopupTracker.Mark();
      bool isFirst = taskOperationDialog._batchSetTagCtrl == null;
      if (isFirst)
      {
        taskOperationDialog._batchSetTagCtrl = new BatchSetTagControl();
        taskOperationDialog._batchSetTagCtrl.Close += new EventHandler(taskOperationDialog.OnBatchSetTagClosed);
        // ISSUE: reference to a compiler-generated method
        taskOperationDialog._batchSetTagCtrl.TagsSelect += new EventHandler<TagSelectData>(taskOperationDialog.\u003CDelayShowTagPopup\u003Eb__136_0);
        taskOperationDialog.SetTagPopup.Child = (UIElement) taskOperationDialog._batchSetTagCtrl;
      }
      if (wait)
      {
        await Task.Delay(150);
        if (taskOperationDialog.TaskPomoPopup.IsOpen || taskOperationDialog.SetAssigneePopup.IsOpen || taskOperationDialog.SetProjectPopup.IsOpen || taskOperationDialog.ChangeDatePopup.IsOpen)
        {
          taskOperationDialog._tagPopupShow = false;
          return;
        }
      }
      if (!taskOperationDialog._tagPopupShow)
        return;
      taskOperationDialog._batchSetTagCtrl.Init(taskOperationDialog._extra.Tags?.Clone() ?? new TagSelectData(), isFirst);
      taskOperationDialog.SetTagPopup.IsOpen = true;
      taskOperationDialog._tagPopupTracker.Bind((Popup) taskOperationDialog.SetTagPopup);
    }

    private void OnBatchSetTagClosed(object sender, EventArgs e) => this.SetTagPopup.IsOpen = false;

    private void TryShowSetProjectPopup()
    {
      if (this.SetProjectPopup.IsOpen)
        this._projectPopupTracker.Mark();
      bool flag = this._projectOrGroupPopup != null && this._projectOrGroupPopup.ChildMouseOver();
      if (!this.MoveButton.IsMouseOver && !this.SetProjectPopup.IsMouseOver && !flag)
      {
        this.TryHideProjectPopup();
      }
      else
      {
        if (this._setProjectPopupShow || this.SetProjectPopup.IsOpen)
          return;
        this._setProjectPopupShow = true;
        this.DelayShowSetProjectPopup();
      }
    }

    private async Task TryHideProjectPopup()
    {
      this._setProjectPopupShow = false;
      if (!this.SetProjectPopup.IsOpen)
        return;
      await Task.Delay(100);
      bool flag1 = this._projectPopupTracker.IsInSafeArea();
      bool flag2 = this._projectOrGroupPopup != null && this._projectOrGroupPopup.ChildMouseOver();
      if (!this.MoveButton.IsMouseOver && !this.SetProjectPopup.IsMouseOver && !flag2 && !flag1)
      {
        this._projectOrGroupPopup?.ClosePopup();
        this.SetProjectPopup.IsOpen = false;
      }
      else
        this._setProjectPopupShow = this.SetProjectPopup.IsOpen;
    }

    private async void DelayShowSetProjectPopup()
    {
      await Task.Delay(150);
      if (this.TaskPomoPopup.IsOpen || this.SetAssigneePopup.IsOpen || this.SetTagPopup.IsOpen || this.ChangeDatePopup.IsOpen)
      {
        this._setProjectPopupShow = false;
      }
      else
      {
        if (!this._setProjectPopupShow)
          return;
        this.ShowProjectSelector();
      }
    }

    private void ShowProjectSelector()
    {
      if (this._projectOrGroupPopup == null)
      {
        this._projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.SetProjectPopup, new ProjectExtra()
        {
          ProjectIds = new List<string>()
          {
            this._extra.ProjectId
          }
        }, new ProjectSelectorExtra()
        {
          showAll = false,
          batchMode = false,
          canSelectGroup = false,
          onlyShowPermission = true,
          CanSearch = true,
          ShowColumn = true,
          ColumnId = this._extra.ColumnId
        });
        this._projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnProjectSelect);
      }
      this._projectOrGroupPopup.Show();
      this._projectOrGroupPopup.DelayFocus();
      this._projectPopupTracker.Bind(this._projectOrGroupPopup.GetProjectPopup());
    }

    private void TryShowAssignPopup()
    {
      if (!this.AssignBtn.IsMouseOver && !this.SetAssigneePopup.IsMouseOver)
      {
        this.TryHideAssignPopup();
      }
      else
      {
        if (this._assignPopupShow || this.SetAssigneePopup.IsOpen)
          return;
        this._assignPopupShow = true;
        this.DelayShowAssignPopup();
      }
    }

    private async Task TryHideAssignPopup()
    {
      this._assignPopupShow = false;
      if (!this.SetAssigneePopup.IsOpen)
        return;
      await Task.Delay(100);
      if (!this.AssignBtn.IsMouseOver && !this.SetAssigneePopup.IsMouseOver)
        this.SetAssigneePopup.IsOpen = false;
      else
        this._assignPopupShow = this.SetAssigneePopup.IsOpen;
    }

    private async void DelayShowAssignPopup()
    {
      await Task.Delay(150);
      if (this.TaskPomoPopup.IsOpen || this.SetProjectPopup.IsOpen || this.SetTagPopup.IsOpen || this.ChangeDatePopup.IsOpen)
      {
        this._assignPopupShow = false;
      }
      else
      {
        if (!this._assignPopupShow)
          return;
        this.ShowAssignPopup();
      }
    }

    private void ShowAssignPopup(bool onEnter = false)
    {
      if (this._assignDialog == null)
      {
        this._assignDialog = new SetAssigneeDialog(this._extra.ProjectId, (Popup) this.SetAssigneePopup, this._extra.Assignee);
        this._assignDialog.AssigneeSelect += new EventHandler<AvatarInfo>(this.OnAssigneeSelect);
        if (onEnter)
          this._assignDialog.UpDownSelect(true);
      }
      this._assignDialog.Show();
      this._assignPopupTracker.Bind(this._assignDialog.GetPopup());
    }

    private void TryShowPomoSetPopup()
    {
      if (this.TaskPomoPopup.IsOpen)
        this._pomoPopupTracker.Mark();
      if (!this.PomoGrid.IsMouseOver && !this.TaskPomoPopup.IsMouseOver)
      {
        this.TryHidePomoPopup();
      }
      else
      {
        if (this._pomoPopupShow || this.TaskPomoPopup.IsOpen)
          return;
        this._pomoPopupShow = true;
        this.DelayShowPomoPopup();
      }
    }

    private async Task TryHidePomoPopup()
    {
      this._pomoPopupShow = false;
      if (!this.TaskPomoPopup.IsOpen)
        return;
      await Task.Delay(100);
      bool flag = this._pomoPopupTracker.IsInSafeArea();
      if (!this.PomoGrid.IsMouseOver && !this.TaskPomoSetDialog.CheckMouseMove() && !flag)
        this.TaskPomoPopup.IsOpen = false;
      else
        this._pomoPopupShow = this.TaskPomoPopup.IsOpen;
    }

    private void TryShowChangeDatePopup()
    {
      if (!this.ChangeCompleteDate.IsMouseOver && !this.ChangeDatePopup.IsMouseOver)
      {
        this.TryHideChangeDatePopup();
      }
      else
      {
        if (this._changeDatePopupShow || this.ChangeDatePopup.IsOpen)
          return;
        this._changeDatePopupShow = true;
        this.DelayShowChangeDatePopup();
      }
    }

    private async Task TryHideChangeDatePopup()
    {
      this._changeDatePopupShow = false;
      if (this.ChangeDatePopup.IsOpen)
      {
        await Task.Delay(100);
        if (!this.ChangeCompleteDate.IsMouseOver && !this.ChangeDatePopup.IsMouseOver)
          this.ChangeDatePopup.IsOpen = false;
        else
          this._changeDatePopupShow = this.ChangeDatePopup.IsOpen;
      }
      else
        this._changeDatePopupShow = false;
    }

    private async void DelayShowChangeDatePopup()
    {
      await Task.Delay(150);
      if (this.TaskPomoPopup.IsOpen || this.SetProjectPopup.IsOpen || this.SetTagPopup.IsOpen || this.SetAssigneePopup.IsOpen)
      {
        this._changeDatePopupShow = false;
      }
      else
      {
        if (!this._changeDatePopupShow)
          return;
        this.ShowChangeDatePopup();
      }
    }

    private void ShowChangeDatePopup(bool onEnter = false)
    {
      if (this._dueDateDialog == null)
      {
        EscPopup changeDatePopup = this.ChangeDatePopup;
        DurationModel model = new DurationModel();
        model.SelectedDate = this._completeTime;
        model.SelectionStart = new DateTime?();
        model.SelectionEnd = new DateTime?();
        DateTime? maxDate = new DateTime?(DateTime.Today);
        DateTime? minDate = new DateTime?();
        this._dueDateDialog = new SelectDateDialog((Popup) changeDatePopup, model, false, maxDate, minDate, true);
        this._dueDateDialog.SelectDate += new EventHandler<DateTime>(this.SelectDueDate);
      }
      this._dueDateDialog.Show();
      this._dueDateDialog.SetCurrentTab(onEnter);
    }

    private async void DelayShowPomoPopup()
    {
      await Task.Delay(150);
      if (this.ChangeDatePopup.IsOpen || this.SetProjectPopup.IsOpen || this.SetTagPopup.IsOpen || this.SetAssigneePopup.IsOpen)
      {
        this._pomoPopupShow = false;
      }
      else
      {
        if (!this._pomoPopupShow)
          return;
        this.ShowPomoPopup();
      }
    }

    private void ShowPomoPopup(bool onEnter = false)
    {
      this.TaskPomoPopup.IsOpen = true;
      this.TaskPomoSetDialog.InitData(this._extra.TaskId, true, "tasklist", "cm_single_task", onEnter);
      this.TaskPomoSetDialog.Closed -= new EventHandler<bool>(this.ClosePomoPopup);
      this.TaskPomoSetDialog.Closed += new EventHandler<bool>(this.ClosePomoPopup);
      this._pomoPopupTracker.Bind((Popup) this.TaskPomoPopup);
    }

    private async void SelectDueDate(object sender, DateTime date)
    {
      EventHandler<DateTime> completeDateChanged = this.CompleteDateChanged;
      if (completeDateChanged != null)
        completeDateChanged((object) null, date);
      await Task.Delay(250);
      this.CloseParentPopup();
    }

    private void OnCreateSubTaskClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      EventHandler createSubTask = this.CreateSubTask;
      if (createSubTask != null)
        createSubTask((object) this, (EventArgs) null);
      UserActCollectUtils.AddClickEvent("tasklist", "cm_single_task", "add_subtask");
    }

    private void OnSwitchClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      if (!this._extra.CanSwitch)
      {
        EventHandler<string> toast = this.Toast;
        if (toast == null)
          return;
        toast((object) this, this._extra.FailedSwitchTips);
      }
      else
      {
        EventHandler switchTaskOrNote = this.SwitchTaskOrNote;
        if (switchTaskOrNote != null)
          switchTaskOrNote((object) null, (EventArgs) null);
        if (this._extra.InBatch)
          UserActCollectUtils.AddClickEvent("tasklist", "cm_batch_task", "convert_to_note_or_task");
        else
          UserActCollectUtils.AddClickEvent("tasklist", this._extra.TaskType == TaskType.Task ? "cm_single_task" : "cm_single_note", this._extra.TaskType == TaskType.Task ? "convert_to_note" : "convert_to_task");
      }
    }

    public static void TryCloseLastOne()
    {
      if (TaskOperationDialog._lastOne == null)
        return;
      TaskOperationDialog._lastOne.IsOpen = false;
    }

    private void OnPinClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      bool valueOrDefault = (this.PinButton.Tag as bool?).GetValueOrDefault();
      UserActCollectUtils.AddClickEvent("tasklist", this._extra.TaskType == TaskType.Task ? "cm_single_task" : "cm_single_note", valueOrDefault ? "pin" : "unpin");
      EventHandler<bool> starred = this.Starred;
      if (starred == null)
        return;
      starred((object) this, valueOrDefault);
    }

    private void OnAbandonOrReopenClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      EventHandler abandonOrReopen = this.AbandonOrReopen;
      if (abandonOrReopen == null)
        return;
      abandonOrReopen((object) this, (EventArgs) null);
    }

    private void OnCopyTextClick(object sender, MouseButtonEventArgs e)
    {
      this.CloseParentPopup();
      EventHandler textCopied = this.TextCopied;
      if (textCopied == null)
        return;
      textCopied((object) this, (EventArgs) null);
    }

    private async void OnStickyClick(object sender, MouseButtonEventArgs e)
    {
      TaskOperationDialog sender1 = this;
      sender1.CloseParentPopup();
      UserActCollectUtils.AddClickEvent("tasklist", sender1._extra.InBatch ? "cm_batch_task" : (sender1._extra.TaskType == TaskType.Task ? "cm_single_task" : "cm_single_note"), "open_as_sticky_note");
      if (sender1._extra.InBatch)
      {
        EventHandler openSticky = sender1.OpenSticky;
        if (openSticky == null)
          return;
        openSticky((object) sender1, (EventArgs) null);
      }
      else
      {
        string e1 = TaskUtils.ToastOnOpenSticky(sender1._extra?.TaskId);
        if (!string.IsNullOrEmpty(e1))
        {
          EventHandler<string> toast = sender1.Toast;
          if (toast == null)
            return;
          toast((object) sender1, e1);
        }
        else
          TaskStickyWindow.ShowTaskSticky(new List<string>()
          {
            sender1._extra?.TaskId
          });
      }
    }

    public bool HandleTab(bool shift)
    {
      if (this.TaskPomoPopup.IsOpen)
      {
        this.TaskPomoSetDialog.HandleTab(shift);
        return true;
      }
      if (this.SetTagPopup.IsOpen)
      {
        this._batchSetTagCtrl?.HandleTab(shift);
        return true;
      }
      if (this.ChangeDatePopup.IsOpen)
      {
        this._dueDateDialog?.SetCurrentTab(true);
        return true;
      }
      if (this.SetProjectPopup.IsOpen)
      {
        this._projectOrGroupPopup.UpDownSelect(shift);
        return false;
      }
      if (this.SetAssigneePopup.IsOpen)
      {
        this._assignDialog.UpDownSelect(shift);
        return false;
      }
      if (this.SelectedIndex < 0)
      {
        this.SelectedIndex = 0;
        return true;
      }
      this.SelectedIndex += shift ? 24 : 1;
      this.SelectedIndex %= 25;
      if (this.SelectedIndex >= 5 && this.SelectedIndex <= 8 && !this.PriorityGrid.IsVisible)
        this.SelectedIndex = shift ? 4 : 9;
      switch (this.SelectedIndex)
      {
        case 3:
          if (!this.SkipGrid.IsVisible)
            break;
          goto default;
        case 9:
          if (!this.CreateSubTaskButton.IsVisible)
            break;
          goto default;
        case 10:
          if (!this.PinButton.IsVisible)
            break;
          goto default;
        case 11:
          if (!this.AbandonedButton.IsVisible)
            break;
          goto default;
        case 12:
          if (!this.ReopenButton.IsVisible)
            break;
          goto default;
        case 14:
          if (!this.AssignBtn.IsVisible)
            break;
          goto default;
        case 16:
          if (!this.PomoPanel.IsVisible)
            break;
          goto default;
        case 17:
          if (!this.CopyButton.IsVisible)
            break;
          goto default;
        case 18:
          if (!this.CopyLinkGrid.IsVisible)
            break;
          goto default;
        case 19:
          if (!this.CopyTextGrid.IsVisible)
            break;
          goto default;
        case 20:
          if (!this.MergeBtn.IsVisible)
            break;
          goto default;
        case 21:
          if (!this.ChangeCompleteDate.IsVisible)
            break;
          goto default;
        case 23:
          if (this.SwitchButton.IsVisible)
            goto default;
          else
            break;
        default:
          return true;
      }
      return this.HandleTab(shift);
    }

    public bool HandleEnter()
    {
      if (this.ChangeDatePopup.IsOpen)
        this._dueDateDialog.EnterSelect();
      if (this.SetProjectPopup.IsOpen || this.SetAssigneePopup.IsOpen || this.SetTagPopup.IsOpen || this.TaskPomoPopup.IsOpen || this.ChangeDatePopup.IsOpen)
        return false;
      switch (this.SelectedIndex)
      {
        case 0:
          this.OnTimeClick("today");
          break;
        case 1:
          this.OnTimeClick("tomorrow");
          break;
        case 2:
          this.OnTimeClick("nextweek");
          break;
        case 3:
          this.SkipRecurrenceClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 4:
          this.OnTimeClick("custom");
          break;
        case 5:
        case 6:
        case 7:
          this.OnPriorityClick((7 - this.SelectedIndex) * 2 + 1);
          this.CloseParentPopup();
          break;
        case 8:
          this.OnPriorityClick(0);
          this.CloseParentPopup();
          break;
        case 9:
          this.OnCreateSubTaskClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 10:
          this.OnPinClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 11:
          this.OnAbandonOrReopenClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 12:
          this.OnAbandonOrReopenClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 13:
          this._setProjectPopupShow = true;
          this.ShowProjectSelector();
          break;
        case 14:
          this._assignPopupShow = true;
          this.ShowAssignPopup(true);
          break;
        case 15:
          this._tagPopupShow = true;
          this.DelayShowTagPopup();
          break;
        case 16:
          this._pomoPopupShow = true;
          this.ShowPomoPopup(true);
          break;
        case 17:
          this.OnCopyClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 18:
          this.OnCopyLinkClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 19:
          this.OnCopyTextClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 20:
          this.OnMergeClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 21:
          this._changeDatePopupShow = true;
          this.ShowChangeDatePopup(true);
          break;
        case 22:
          this.OnStickyClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 23:
          this.OnSwitchClick((object) null, (MouseButtonEventArgs) null);
          break;
        case 24:
          this.OnDeleteClick((object) null, (MouseButtonEventArgs) null);
          break;
      }
      return true;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      if (this.ChangeDatePopup.IsOpen)
      {
        this._dueDateDialog.HandleUpDown(isUp);
        return true;
      }
      if (this.SetProjectPopup.IsOpen)
      {
        this._projectOrGroupPopup.UpDownSelect(isUp);
        return true;
      }
      if (this.SetProjectPopup.IsOpen || this.SetAssigneePopup.IsOpen || this.SetTagPopup.IsOpen || this.TaskPomoPopup.IsOpen || this.ChangeDatePopup.IsOpen)
        return false;
      if (this.SelectedIndex < 0)
      {
        this.SelectedIndex = 0;
        return true;
      }
      if (this.SelectedIndex >= 0 && this.SelectedIndex <= 4)
        this.SelectedIndex = isUp ? -1 : (this.PriorityGrid.IsVisible ? Math.Min(this.SelectedIndex + 5, 8) : 9);
      else if (this.SelectedIndex >= 5 && this.SelectedIndex <= 8)
      {
        this.SelectedIndex = isUp ? this.SelectedIndex - 5 : 9;
        if (this.SelectedIndex == 3 && !this.SkipGrid.IsVisible)
          this.SelectedIndex = 4;
      }
      else if (this.SelectedIndex == 9)
        this.SelectedIndex = isUp ? (this.PriorityGrid.IsVisible ? 5 : 0) : 10;
      else
        this.SelectedIndex += isUp ? -1 : 1;
      this.SelectedIndex += 25;
      this.SelectedIndex %= 25;
      switch (this.SelectedIndex)
      {
        case 9:
          if (!this.CreateSubTaskButton.IsVisible)
            break;
          goto default;
        case 10:
          if (!this.PinButton.IsVisible)
            break;
          goto default;
        case 11:
          if (!this.AbandonedButton.IsVisible)
            break;
          goto default;
        case 12:
          if (!this.ReopenButton.IsVisible)
            break;
          goto default;
        case 14:
          if (!this.AssignBtn.IsVisible)
            break;
          goto default;
        case 16:
          if (!this.PomoPanel.IsVisible)
            break;
          goto default;
        case 17:
          if (!this.CopyButton.IsVisible)
            break;
          goto default;
        case 18:
          if (!this.CopyLinkGrid.IsVisible)
            break;
          goto default;
        case 19:
          if (!this.CopyTextGrid.IsVisible)
            break;
          goto default;
        case 20:
          if (!this.MergeBtn.IsVisible)
            break;
          goto default;
        case 21:
          if (!this.ChangeCompleteDate.IsVisible)
            break;
          goto default;
        case 23:
          if (this.SwitchButton.IsVisible)
            goto default;
          else
            break;
        default:
label_30:
          return true;
      }
      this.UpDownSelect(isUp);
      goto label_30;
    }

    public bool LeftRightSelect(bool isLeft)
    {
      if (this.ChangeDatePopup.IsOpen)
      {
        this._dueDateDialog.HandleLeftRight(isLeft);
        return true;
      }
      if (this.SetProjectPopup.IsOpen)
        return this._projectOrGroupPopup.LeftRightSelect(isLeft);
      if (!isLeft)
      {
        if (this.SetAssigneePopup.IsOpen || this.SetTagPopup.IsOpen || this.TaskPomoPopup.IsOpen)
          return false;
        int num = this.SetProjectPopup.IsOpen ? 1 : 0;
        switch (this.SelectedIndex)
        {
          case 13:
          case 14:
          case 15:
          case 16:
          case 21:
            this.HandleEnter();
            return false;
        }
      }
      else
      {
        if (this.TaskPomoPopup.IsOpen && !this.TaskPomoSetDialog.IsInputFocus())
        {
          this.TaskPomoPopup.IsOpen = false;
          return true;
        }
        if (this.SetAssigneePopup.IsOpen)
        {
          this.SetAssigneePopup.IsOpen = false;
          return true;
        }
      }
      if (this.SelectedIndex >= 0 && this.SelectedIndex <= 4)
      {
        this.SelectedIndex += isLeft ? -1 : 1;
        if (this.SelectedIndex == 3 && !this.SkipGrid.IsVisible)
          this.SelectedIndex += isLeft ? -1 : 1;
        this.SelectedIndex += 5;
        this.SelectedIndex %= 5;
      }
      else if (this.SelectedIndex >= 5 && this.SelectedIndex <= 8)
      {
        this.SelectedIndex += isLeft ? -1 : 1;
        if (this.SelectedIndex > 8)
          this.SelectedIndex = 5;
        if (this.SelectedIndex < 5)
          this.SelectedIndex = 8;
      }
      return false;
    }

    private void OnChildPopupOpened(object sender, EventArgs e)
    {
      if (object.Equals(sender, (object) this.SetProjectPopup))
        this.ProjectItem.HoverSelected = true;
      else if (object.Equals(sender, (object) this.SetAssigneePopup))
        this.AssignItem.HoverSelected = true;
      else if (object.Equals(sender, (object) this.SetTagPopup))
        this.TagItem.HoverSelected = true;
      else if (object.Equals(sender, (object) this.TaskPomoPopup))
      {
        this.PomoItem.HoverSelected = true;
      }
      else
      {
        if (!object.Equals(sender, (object) this.ChangeDatePopup))
          return;
        this.CompleteItem.HoverSelected = true;
      }
    }

    private void OnChildPopupClosed(object sender, EventArgs e)
    {
      if (object.Equals(sender, (object) this.SetProjectPopup))
        this.ProjectItem.HoverSelected = this.SelectedIndex == 13;
      else if (object.Equals(sender, (object) this.SetAssigneePopup))
        this.AssignItem.HoverSelected = this.SelectedIndex == 14;
      else if (object.Equals(sender, (object) this.SetTagPopup))
        this.TagItem.HoverSelected = this.SelectedIndex == 15;
      else if (object.Equals(sender, (object) this.TaskPomoPopup))
      {
        this.PomoItem.HoverSelected = this.SelectedIndex == 16;
      }
      else
      {
        if (!object.Equals(sender, (object) this.ChangeDatePopup))
          return;
        this.CompleteItem.HoverSelected = this.SelectedIndex == 21;
      }
    }

    private void SetItemTabSelected()
    {
      this.ProjectItem.HoverSelected = this.SelectedIndex == 13 || this.SetProjectPopup.IsOpen;
      this.AssignItem.HoverSelected = this.SelectedIndex == 14 || this.SetAssigneePopup.IsOpen;
      this.TagItem.HoverSelected = this.SelectedIndex == 15 || this.SetTagPopup.IsOpen;
      this.PomoItem.HoverSelected = this.SelectedIndex == 16 || this.TaskPomoPopup.IsOpen;
      this.CompleteItem.HoverSelected = this.SelectedIndex == 21 || this.ChangeDatePopup.IsOpen;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/taskoperationdialog.xaml", UriKind.Relative));
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
          this.Root = (TaskOperationDialog) target;
          this.Root.Initialized += new EventHandler(this.OnInitialized);
          break;
        case 2:
          this.Container = (StackPanel) target;
          this.Container.MouseMove += new MouseEventHandler(this.ShowPopup);
          break;
        case 3:
          this.DateStack = (StackPanel) target;
          break;
        case 4:
          this.DatePanel = (Grid) target;
          break;
        case 5:
          this.FourthColumn = (ColumnDefinition) target;
          break;
        case 6:
          this.SixthColumn = (ColumnDefinition) target;
          break;
        case 7:
          this.TodayGrid = (Border) target;
          this.TodayGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetTimeClick);
          break;
        case 8:
          this.TomorrowGrid = (Border) target;
          this.TomorrowGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetTimeClick);
          break;
        case 9:
          this.NextWeekGrid = (Border) target;
          this.NextWeekGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetTimeClick);
          break;
        case 10:
          this.SkipGrid = (Border) target;
          this.SkipGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SkipRecurrenceClick);
          break;
        case 11:
          this.SetDateBorder = (Border) target;
          this.SetDateBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetTimeClick);
          break;
        case 12:
          this.ClearDateBorder = (Border) target;
          this.ClearDateBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetTimeClick);
          break;
        case 13:
          this.PriorityGrid = (Grid) target;
          break;
        case 14:
          this.PriorityHighGrid = (Border) target;
          this.PriorityHighGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityGridClick);
          break;
        case 15:
          this.PriorityMediumGrid = (Border) target;
          this.PriorityMediumGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityGridClick);
          break;
        case 16:
          this.PriorityLowGrid = (Border) target;
          this.PriorityLowGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityGridClick);
          break;
        case 17:
          this.PriorityNoGrid = (Border) target;
          this.PriorityNoGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityGridClick);
          break;
        case 18:
          this.CreateSubTaskButton = (OptionItemWithImageIcon) target;
          break;
        case 19:
          this.PinButton = (OptionItemWithImageIcon) target;
          break;
        case 20:
          this.AbandonedButton = (OptionItemWithImageIcon) target;
          break;
        case 21:
          this.ReopenButton = (OptionItemWithImageIcon) target;
          break;
        case 22:
          this.MoveButton = (Grid) target;
          break;
        case 23:
          this.ProjectItem = (OptionCheckBox) target;
          break;
        case 24:
          this.SetProjectPopup = (EscPopup) target;
          break;
        case 25:
          this.AssignBtn = (Grid) target;
          break;
        case 26:
          this.AssignItem = (OptionCheckBox) target;
          break;
        case 27:
          this.SetAssigneePopup = (EscPopup) target;
          break;
        case 28:
          this.TagGrid = (Grid) target;
          break;
        case 29:
          this.TagItem = (OptionCheckBox) target;
          break;
        case 30:
          this.SetTagPopup = (EscPopup) target;
          break;
        case 31:
          this.PomoPanel = (StackPanel) target;
          break;
        case 32:
          this.PomoGrid = (Grid) target;
          break;
        case 33:
          this.PomoItem = (OptionCheckBox) target;
          break;
        case 34:
          this.TaskPomoPopup = (EscPopup) target;
          break;
        case 35:
          this.TaskPomoSetDialog = (TaskPomoSetDialog) target;
          break;
        case 36:
          this.CopyButton = (OptionItemWithImageIcon) target;
          break;
        case 37:
          this.CopyLinkGrid = (OptionItemWithImageIcon) target;
          break;
        case 38:
          this.CopyTextGrid = (OptionItemWithImageIcon) target;
          break;
        case 39:
          this.MergeBtn = (OptionItemWithImageIcon) target;
          break;
        case 40:
          this.ChangeCompleteDate = (Grid) target;
          break;
        case 41:
          this.CompleteItem = (OptionItemWithImageIcon) target;
          break;
        case 42:
          this.ChangeDatePopup = (EscPopup) target;
          break;
        case 43:
          this.DivideLine = (Line) target;
          break;
        case 44:
          this.SwitchButton = (OptionItemWithImageIcon) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
