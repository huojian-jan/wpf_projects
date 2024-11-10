// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TaskPomoSetDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using TickTickDao;
using TickTickModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class TaskPomoSetDialog : UserControl, ITabControl, IComponentConnector
  {
    private int _originalEstimatedCount;
    private string _taskId;
    private PomodoroSummaryModel _taskPomo;
    private static int _defaultIndex;
    private string _eventType;
    private string _eventCtype;
    private int _tabIndex;
    internal TaskPomoSetDialog Root;
    internal ContentControl Container;
    internal TextBox EmptyBox;
    internal StackPanel TaskPomoPanel;
    internal OptionCheckBox StartPomoButton;
    internal OptionCheckBox StartTimingButton;
    internal OptionCheckBox EstimateButton;
    internal StackPanel EstimatePanel;
    internal CustomComboBox EstimateTypeComboBox;
    internal Grid EstimatePomoGrid;
    internal TextBox PomoCount;
    internal Grid EstimateDurationGrid;
    internal TextBox HourCount;
    internal TextBox MinuteCount;
    internal Grid SaveButtonPanel;
    internal Button SaveButton;
    internal Button CancelButton;
    private bool _contentLoaded;

    public TaskPomoSetDialog()
    {
      this.InitializeComponent();
      CustomComboBox estimateTypeComboBox = this.EstimateTypeComboBox;
      ObservableCollection<ComboBoxViewModel> items = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) 0, Utils.GetString("EstimatedPomo"), 32.0);
      comboBoxViewModel.Selected = true;
      items.Add(comboBoxViewModel);
      items.Add(new ComboBoxViewModel((object) 1, Utils.GetString("EstimatedDurationPro"), 32.0));
      estimateTypeComboBox.Init<ComboBoxViewModel>(items, (ComboBoxViewModel) null);
    }

    public TaskPomoSetDialog(bool inSticky)
    {
      this.InitializeComponent();
      CustomComboBox estimateTypeComboBox = this.EstimateTypeComboBox;
      ObservableCollection<ComboBoxViewModel> items = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) 0, Utils.GetString("EstimatedPomo"), 32.0);
      comboBoxViewModel.Selected = true;
      items.Add(comboBoxViewModel);
      items.Add(new ComboBoxViewModel((object) 1, Utils.GetString("EstimatedDurationPro"), 32.0));
      estimateTypeComboBox.Init<ComboBoxViewModel>(items, (ComboBoxViewModel) null);
      if (!inSticky)
        return;
      this.Container.Style = (Style) null;
      this.FontSize = 12.0;
      this.StartPomoButton.Height = 30.0;
      this.StartTimingButton.Height = 30.0;
      this.EstimateButton.Height = 30.0;
      this.EstimatePomoGrid.Height = 30.0;
    }

    public event EventHandler<bool> Closed;

    public async void InitData(
      string taskId,
      bool force = false,
      string eventType = null,
      string eventCtype = null,
      bool onEnter = false)
    {
      TaskPomoSetDialog taskPomoSetDialog = this;
      taskPomoSetDialog.TaskPomoPanel.Visibility = Visibility.Visible;
      taskPomoSetDialog.EstimatePanel.Visibility = Visibility.Collapsed;
      taskPomoSetDialog._eventType = eventType;
      taskPomoSetDialog._eventCtype = eventCtype;
      if (onEnter)
      {
        taskPomoSetDialog.StartPomoButton.HoverSelected = true;
        taskPomoSetDialog.EstimateButton.HoverSelected = false;
        taskPomoSetDialog.StartTimingButton.HoverSelected = false;
      }
      if (!(taskPomoSetDialog._taskId != taskId | force))
        return;
      taskPomoSetDialog._taskId = taskId;
      PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(taskPomoSetDialog._taskId);
      taskPomoSetDialog._taskPomo = pomoByTaskId;
      if (taskPomoSetDialog._taskPomo != null)
      {
        if (taskPomoSetDialog._taskPomo.estimatedPomo > 0)
        {
          taskPomoSetDialog.PomoCount.Text = taskPomoSetDialog._taskPomo.estimatedPomo.ToString() ?? "";
          taskPomoSetDialog.EstimateTypeComboBox.SetSelected((object) 0);
        }
        else if (taskPomoSetDialog._taskPomo.EstimatedDuration > 0L)
        {
          long num = taskPomoSetDialog._taskPomo.EstimatedDuration / 60L;
          taskPomoSetDialog.HourCount.Text = (num / 60L).ToString() ?? "";
          taskPomoSetDialog.MinuteCount.Text = (num % 60L).ToString() ?? "";
          taskPomoSetDialog.EstimateTypeComboBox.SetSelected((object) 1);
        }
        else
        {
          taskPomoSetDialog.HourCount.Text = "0";
          taskPomoSetDialog.MinuteCount.Text = "0";
          taskPomoSetDialog.PomoCount.Text = "0";
          taskPomoSetDialog.EstimateTypeComboBox.SetSelected((object) 0);
        }
      }
      else
      {
        taskPomoSetDialog.HourCount.Text = "0";
        taskPomoSetDialog.MinuteCount.Text = "0";
        taskPomoSetDialog.PomoCount.Text = "0";
        taskPomoSetDialog.EstimateTypeComboBox.SetSelected((object) TaskPomoSetDialog._defaultIndex);
        taskPomoSetDialog._taskPomo = new PomodoroSummaryModel()
        {
          id = Utils.GetGuid(),
          taskId = taskId,
          userId = LocalSettings.Settings.LoginUserId
        };
      }
      taskPomoSetDialog.StartPomoButton.Text = Utils.GetString("StartPomo");
      taskPomoSetDialog.StartTimingButton.Text = Utils.GetString("StartTiming");
      if (TickFocusManager.Working)
      {
        taskPomoSetDialog.StartPomoButton.IsEnabled = LocalSettings.Settings.PomoType == FocusConstance.Focus;
        taskPomoSetDialog.StartTimingButton.IsEnabled = !taskPomoSetDialog.StartPomoButton.IsEnabled;
      }
      else
      {
        taskPomoSetDialog.StartPomoButton.IsEnabled = true;
        taskPomoSetDialog.StartTimingButton.IsEnabled = true;
      }
      taskPomoSetDialog.StartPomoButton.Opacity = taskPomoSetDialog.StartPomoButton.IsEnabled ? 1.0 : 0.56;
      taskPomoSetDialog.StartTimingButton.Opacity = taskPomoSetDialog.StartTimingButton.IsEnabled ? 1.0 : 0.56;
      if (taskPomoSetDialog.EstimateTypeComboBox.SelectedIndex == 1)
      {
        taskPomoSetDialog.EstimatePomoGrid.Visibility = Visibility.Collapsed;
        taskPomoSetDialog.EstimateDurationGrid.Visibility = Visibility.Visible;
      }
      else
      {
        taskPomoSetDialog.EstimatePomoGrid.Visibility = Visibility.Visible;
        taskPomoSetDialog.EstimateDurationGrid.Visibility = Visibility.Collapsed;
      }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      await this.TrySaveAndClose();
    }

    private async Task TrySaveAndClose()
    {
      TaskPomoSetDialog sender = this;
      TaskModel task = await TaskDao.GetThinTaskById(sender._taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        TaskPomoSetDialog._defaultIndex = sender.EstimateTypeComboBox.SelectedIndex;
        if (sender.EstimatePomoGrid.Visibility == Visibility.Visible)
        {
          int result;
          int.TryParse(sender.PomoCount.Text, out result);
          sender._taskPomo.estimatedPomo = result;
          sender._taskPomo.EstimatedDuration = 0L;
        }
        else if (sender.EstimateDurationGrid.Visibility == Visibility.Visible)
        {
          int result1;
          int.TryParse(sender.HourCount.Text, out result1);
          int result2;
          int.TryParse(sender.MinuteCount.Text, out result2);
          int num = Math.Min(5999999, result1 * 60 + result2) * 60;
          sender._taskPomo.EstimatedDuration = (long) num;
          sender._taskPomo.estimatedPomo = 0;
        }
        await PomoSummaryDao.SavePomoSummary(sender._taskPomo);
        DataChangedNotifier.NotifyPomoChanged(sender._taskId);
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskDao.UpdateTask(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        SyncManager.Sync();
        EventHandler<bool> closed = sender.Closed;
        if (closed == null)
        {
          task = (TaskModel) null;
        }
        else
        {
          closed((object) sender, false);
          task = (TaskModel) null;
        }
      }
    }

    private async void OnCancelClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      this.EstimatePanel.Visibility = Visibility.Collapsed;
      this.TaskPomoPanel.Visibility = Visibility.Visible;
    }

    private void OnEstimatePreviewInput(object sender, TextCompositionEventArgs e)
    {
      if (sender == null || !(sender is TextBox textBox))
        return;
      bool flag = int.TryParse(textBox.Text.Insert(textBox.SelectionStart, e.Text), out int _);
      e.Handled = !flag;
    }

    private async void OnPomoCountChanged(object sender, TextChangedEventArgs e)
    {
      if (!(sender is TextBox textBox))
      {
        textBox = (TextBox) null;
      }
      else
      {
        await Task.Delay(10);
        int result;
        int.TryParse(textBox.Text, out result);
        if (result > 60)
        {
          textBox.Text = 60.ToString() ?? "";
          textBox.SelectAll();
        }
        if (result > 0)
        {
          textBox = (TextBox) null;
        }
        else
        {
          textBox.Text = "0";
          textBox.SelectAll();
          textBox = (TextBox) null;
        }
      }
    }

    private async void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!(sender is TextBox textBox))
      {
        textBox = (TextBox) null;
      }
      else
      {
        await Task.Delay(10);
        int result;
        int.TryParse(textBox.Text, out result);
        if (result > 99999)
        {
          textBox.Text = 99999.ToString() ?? "";
          textBox.SelectAll();
        }
        if (result > 0)
        {
          textBox = (TextBox) null;
        }
        else
        {
          textBox.Text = "0";
          textBox.SelectAll();
          textBox = (TextBox) null;
        }
      }
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (this._tabIndex >= 0)
            break;
          this.TrySaveAndClose();
          break;
        case Key.Escape:
          EventHandler<bool> closed = this.Closed;
          if (closed == null)
            break;
          closed((object) this, false);
          break;
      }
    }

    public static bool ConfirmSwitch(string title, bool isPomo, Window owner = null)
    {
      List<Inline> content = new List<Inline>();
      string[] strArray = Utils.GetString(isPomo ? "SwitchTimingTips" : "SwitchPomoTips").Split('0');
      if (strArray.Length == 2)
      {
        content.Add((Inline) new Run(strArray[0].Substring(0, strArray[0].Length - 1)));
        List<Inline> inlineList = content;
        Run run = new Run(title);
        run.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
        inlineList.Add((Inline) run);
        content.Add((Inline) new Run(strArray[1].Substring(1, strArray[1].Length - 1)));
      }
      bool? nullable1 = new CustomerDialog("", content, Utils.GetString("Ok"), Utils.GetString("Cancel"), owner).ShowDialog();
      if (!nullable1.HasValue)
        return false;
      bool? nullable2 = nullable1;
      bool flag = false;
      return !(nullable2.GetValueOrDefault() == flag & nullable2.HasValue);
    }

    private async void OnStartFocusClick(object sender, EventArgs e) => this.StartFocus(true);

    private async void StartFocus(bool isPomo)
    {
      TaskPomoSetDialog sender = this;
      if (!string.IsNullOrEmpty(sender._taskId))
      {
        if (TickFocusManager.IsFocusing(sender._taskId))
        {
          Utils.Toast(Utils.GetString("TaskIsFocused"));
          EventHandler<bool> closed = sender.Closed;
          if (closed == null)
            return;
          closed((object) sender, true);
          return;
        }
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(sender._taskId);
        if (thinTaskById != null)
        {
          if (!TickFocusManager.ConfirmSwitch(thinTaskById.id, thinTaskById.title))
            return;
          if (TickFocusManager.Status == PomoStatus.WaitingWork)
            UserActCollectUtils.AddClickEvent("focus", "start_from", sender._eventCtype == "cm_single_task" ? "cm_single_task" : (sender._eventCtype == "task_detail_pomo" ? "task_detail_pomo" : "task_detail"));
          if (!TickFocusManager.Working)
          {
            TickFocusManager.SetFocusType(!isPomo ? 1 : 0);
            if (isPomo)
            {
              TimerModel timerByObjId = await TimerDao.GetTimerByObjId(thinTaskById.id);
              if (timerByObjId != null && timerByObjId.Type == "pomodoro")
                TickFocusManager.Config.SetPomoSeconds(new int?(timerByObjId.PomodoroTime));
            }
          }
          TickFocusManager.StartFocus(sender._taskId);
          UserActCollectUtils.AddClickEvent(sender._eventType, sender._eventCtype, "start_focus");
        }
      }
      EventHandler<bool> closed1 = sender.Closed;
      if (closed1 == null)
        return;
      closed1((object) sender, true);
    }

    private async void OnStartTimingClick(object sender, EventArgs e) => this.StartFocus(false);

    private void OnEstimateClick(object sender, EventArgs e)
    {
      UserActCollectUtils.AddClickEvent(this._eventType, this._eventCtype, "estimated_pomo");
      this.EstimatePanel.Visibility = Visibility.Visible;
      this.TaskPomoPanel.Visibility = Visibility.Collapsed;
      if (this.EstimatePomoGrid.Visibility == Visibility.Visible)
      {
        this.PomoCount.SelectAll();
        this.PomoCount.Focus();
      }
      if (this.EstimateDurationGrid.Visibility != Visibility.Visible)
        return;
      this.HourCount.SelectAll();
      this.HourCount.Focus();
    }

    private void OnEstimateTypeChanged(object sender, ComboBoxViewModel e)
    {
      if (e.Value is 0)
      {
        this.EstimatePomoGrid.Visibility = Visibility.Visible;
        this.EstimateDurationGrid.Visibility = Visibility.Collapsed;
        this.PomoCount.SelectAll();
        this.PomoCount.Focus();
      }
      else
      {
        this.EstimatePomoGrid.Visibility = Visibility.Collapsed;
        this.EstimateDurationGrid.Visibility = Visibility.Visible;
        this.HourCount.SelectAll();
        this.HourCount.Focus();
      }
    }

    public bool HandleTab(bool shift)
    {
      if (this.EstimatePanel.IsVisible)
      {
        if (this.EstimateTypeComboBox.IsOpen)
        {
          this.EstimateTypeComboBox.UpDownSelect(shift);
          return false;
        }
        if (this._tabIndex < 0)
        {
          this._tabIndex = 0;
          this.EstimateTypeComboBox.TabSelected = true;
          this.EmptyBox.Focus();
        }
        else
        {
          this._tabIndex += shift ? -1 : 1;
          this._tabIndex += 5;
          this._tabIndex %= 5;
          this.EstimateTypeComboBox.TabSelected = this._tabIndex == 0;
          switch (this._tabIndex)
          {
            case 1:
              if (this.EstimateTypeComboBox.SelectedIndex == 0)
              {
                this.PomoCount.Focus();
                break;
              }
              this.HourCount.Focus();
              break;
            case 2:
              if (this.EstimateTypeComboBox.SelectedIndex == 0)
              {
                this.HandleTab(shift);
                break;
              }
              this.MinuteCount.Focus();
              break;
            default:
              this.EmptyBox.Focus();
              break;
          }
          UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._tabIndex == 3);
          UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._tabIndex == 4);
        }
      }
      else
        this.UpDownSelect(shift);
      return true;
    }

    public bool HandleEnter()
    {
      if (this.TaskPomoPanel.IsVisible)
      {
        if (this.StartPomoButton.HoverSelected)
          this.StartFocus(true);
        else if (this.StartTimingButton.HoverSelected)
          this.StartFocus(false);
        else if (this.EstimateButton.HoverSelected)
        {
          this.OnEstimateClick((object) null, (EventArgs) null);
          this._tabIndex = -1;
          this.HandleTab(true);
        }
      }
      else if (this.EstimatePanel.IsVisible)
      {
        switch (this._tabIndex)
        {
          case 0:
            this.EstimateTypeComboBox.HandleEnter();
            break;
          case 3:
            this.TrySaveAndClose();
            break;
          case 4:
            this.EstimatePanel.Visibility = Visibility.Collapsed;
            this.TaskPomoPanel.Visibility = Visibility.Visible;
            break;
        }
      }
      return true;
    }

    public bool HandleEsc()
    {
      if (!this.EstimateTypeComboBox.IsOpen)
        return false;
      this.EstimateTypeComboBox.Close();
      return true;
    }

    public bool UpDownSelect(bool isUp)
    {
      if (!this.TaskPomoPanel.IsVisible)
        return false;
      if (this.StartPomoButton.HoverSelected)
      {
        this.StartPomoButton.HoverSelected = false;
        if (isUp && this.EstimateButton.IsVisible)
          this.EstimateButton.HoverSelected = true;
        else
          this.StartTimingButton.HoverSelected = true;
      }
      else if (this.StartTimingButton.HoverSelected)
      {
        this.StartTimingButton.HoverSelected = false;
        if (!isUp && this.EstimateButton.IsVisible)
          this.EstimateButton.HoverSelected = true;
        else
          this.StartPomoButton.HoverSelected = true;
      }
      else if (this.EstimateButton.HoverSelected)
      {
        this.EstimateButton.HoverSelected = false;
        if (isUp)
          this.StartTimingButton.HoverSelected = true;
        else
          this.StartPomoButton.HoverSelected = true;
      }
      else
        this.StartPomoButton.HoverSelected = true;
      return true;
    }

    public bool LeftRightSelect(bool isLeft) => false;

    public bool IsInputFocus()
    {
      return this.HourCount.IsFocused || this.MinuteCount.IsFocused || this.PomoCount.IsFocused;
    }

    public bool CheckMouseMove()
    {
      if (this.EstimateTypeComboBox.IsOpen)
        return true;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) this);
      return position.X >= -5.0 && position.X <= this.ActualWidth + 10.0 && position.Y >= -5.0 && position.Y <= this.ActualHeight + 10.0;
    }

    private void OnTextGotFocus(object sender, RoutedEventArgs e)
    {
      if (object.Equals(sender, (object) this.PomoCount) || object.Equals(sender, (object) this.HourCount))
        this._tabIndex = 1;
      if (object.Equals(sender, (object) this.MinuteCount))
        this._tabIndex = 2;
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, false);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, false);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/taskpomosetdialog.xaml", UriKind.Relative));
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
          this.Root = (TaskPomoSetDialog) target;
          break;
        case 2:
          this.Container = (ContentControl) target;
          break;
        case 3:
          this.EmptyBox = (TextBox) target;
          break;
        case 4:
          this.TaskPomoPanel = (StackPanel) target;
          break;
        case 5:
          this.StartPomoButton = (OptionCheckBox) target;
          break;
        case 6:
          this.StartTimingButton = (OptionCheckBox) target;
          break;
        case 7:
          this.EstimateButton = (OptionCheckBox) target;
          break;
        case 8:
          this.EstimatePanel = (StackPanel) target;
          break;
        case 9:
          this.EstimateTypeComboBox = (CustomComboBox) target;
          break;
        case 10:
          this.EstimatePomoGrid = (Grid) target;
          break;
        case 11:
          this.PomoCount = (TextBox) target;
          this.PomoCount.GotFocus += new RoutedEventHandler(this.OnTextGotFocus);
          this.PomoCount.PreviewTextInput += new TextCompositionEventHandler(this.OnEstimatePreviewInput);
          this.PomoCount.TextChanged += new TextChangedEventHandler(this.OnPomoCountChanged);
          this.PomoCount.KeyDown += new KeyEventHandler(this.OnTextKeyDown);
          break;
        case 12:
          this.EstimateDurationGrid = (Grid) target;
          break;
        case 13:
          this.HourCount = (TextBox) target;
          this.HourCount.GotFocus += new RoutedEventHandler(this.OnTextGotFocus);
          this.HourCount.PreviewTextInput += new TextCompositionEventHandler(this.OnEstimatePreviewInput);
          this.HourCount.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          this.HourCount.KeyDown += new KeyEventHandler(this.OnTextKeyDown);
          break;
        case 14:
          this.MinuteCount = (TextBox) target;
          this.MinuteCount.GotFocus += new RoutedEventHandler(this.OnTextGotFocus);
          this.MinuteCount.PreviewTextInput += new TextCompositionEventHandler(this.OnEstimatePreviewInput);
          this.MinuteCount.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          this.MinuteCount.KeyDown += new KeyEventHandler(this.OnTextKeyDown);
          break;
        case 15:
          this.SaveButtonPanel = (Grid) target;
          break;
        case 16:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 17:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
