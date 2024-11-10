// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.AddFocusRecordWindow
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Time;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class AddFocusRecordWindow : Window, IOkCancelWindow, IComponentConnector
  {
    private int _focusType;
    private int _pomoNum = 1;
    private DateTime? _startTime;
    private DateTime? _endTime;
    private int? _minutes;
    private PomoTask _focusTask = new PomoTask();
    private int _pomoDuration;
    private PomoFilterControl _pomoFilter;
    internal Grid TaskSelectGrid;
    internal EmjTextBlock TaskTextBox;
    internal EscPopup TaskPopup;
    internal TextBlock StartTextBox;
    internal EscPopup StartPopup;
    internal TextBlock EndTextBox;
    internal EscPopup EndPopup;
    internal TextBlock FocusTypeTextBox;
    internal EscPopup TypePopup;
    internal StackPanel EstimatePanel;
    internal GroupTitle TypeSelector;
    internal Grid PomoGrid;
    internal NumInputTextBox PomoCount;
    internal Grid TimerGrid;
    internal NumInputTextBox HourText;
    internal NumInputTextBox MinuteText;
    internal EmojiEditor NoteEditor;
    internal TextBlock LengthText;
    internal Button SaveButton;
    internal Border ToastBorder;
    internal TextBlock ToastText;
    private bool _contentLoaded;

    public AddFocusRecordWindow(string type)
    {
      this.InitializeComponent();
      InputBindingCollection inputBindings1 = this.InputBindings;
      KeyBinding keyBinding1 = new KeyBinding(OkCancelWindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding1.CommandParameter = (object) this;
      inputBindings1.Add((InputBinding) keyBinding1);
      InputBindingCollection inputBindings2 = this.InputBindings;
      KeyBinding keyBinding2 = new KeyBinding(OkCancelWindowCommands.OkCommand, new KeyGesture(Key.Return));
      keyBinding2.CommandParameter = (object) this;
      inputBindings2.Add((InputBinding) keyBinding2);
      this.Title = Utils.GetString("AddFocusRecord");
      this.TaskTextBox.Text = Utils.GetString("SetTask");
      this.StartTextBox.Text = Utils.GetString("SetTime");
      this.EndTextBox.Text = Utils.GetString("SetTime");
      this._pomoDuration = Math.Max(5, LocalSettings.Settings.PomoDuration);
      if (type == "timing")
      {
        this.TypeSelector.SetSelectedIndex(1);
        this._focusType = 1;
        this.SetTypeValues();
        this.FocusTypeTextBox.Text = Utils.GetString("Timing") + " : 0 " + Utils.GetString("PublicMinute");
      }
      else
        this.FocusTypeTextBox.Text = Utils.GetString("PomoTimer2") + " : 0 " + Utils.GetString("EstimatePomoCount");
    }

    public AddFocusRecordWindow(TimerModel timer)
      : this(timer.Type)
    {
      bool flag = timer.ObjType == "habit";
      this.TaskTextBox.Text = timer.Name;
      this.TaskTextBox.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.TaskSelectGrid.IsEnabled = false;
      this.TaskSelectGrid.Opacity = 0.6;
      this._focusTask.TaskId = flag ? (string) null : timer.ObjId;
      this._focusTask.HabitId = flag ? timer.ObjId : (string) null;
      this._focusTask.Title = string.IsNullOrEmpty(timer.ObjId) ? (string) null : timer.Name;
      this._focusTask.TimerSid = timer.Id;
      this._focusTask.TimerName = timer.Name;
      if (!flag)
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(timer.ObjId);
        this._focusTask.ProjectName = taskById?.ProjectName;
        this._focusTask.Tags = taskById?.Tags;
        PomoTask focusTask = this._focusTask;
        List<string> tags1;
        if (taskById == null)
        {
          tags1 = (List<string>) null;
        }
        else
        {
          string[] tags2 = taskById.Tags;
          tags1 = tags2 != null ? ((IEnumerable<string>) tags2).ToList<string>() : (List<string>) null;
        }
        string jsonContent = TagSerializer.ToJsonContent(tags1);
        focusTask.TagString = jsonContent;
      }
      this._pomoDuration = Math.Max(5, timer.Type == "pomodoro" ? timer.PomodoroTime : LocalSettings.Settings.PomoDuration);
      if (!(timer.Type != "pomodoro"))
        return;
      this.TypeSelector.SetSelectedIndex(1);
      this._focusType = 1;
      this.SetTypeValues();
    }

    public void OnCancel()
    {
      if (this.EndPopup.IsOpen || this.StartPopup.IsOpen || this.TaskPopup.IsOpen || this.TypePopup.IsOpen)
        return;
      this.Close();
    }

    public void Ok()
    {
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      AddFocusRecordWindow focusRecordWindow = this;
      if (focusRecordWindow._endTime.HasValue && focusRecordWindow._startTime.HasValue)
      {
        double totalMinutes = (focusRecordWindow._endTime.Value - focusRecordWindow._startTime.Value).TotalMinutes;
        int count = focusRecordWindow._focusType == 0 ? focusRecordWindow._pomoNum : 1;
        double duration = focusRecordWindow._focusType == 0 ? (double) focusRecordWindow._pomoDuration : totalMinutes;
        if (string.IsNullOrEmpty(focusRecordWindow._focusTask.TimerSid))
        {
          TimerModel timerByObjId = await TimerDao.GetTimerByObjId(focusRecordWindow._focusTask.TaskId ?? focusRecordWindow._focusTask.HabitId);
          if (timerByObjId != null)
          {
            focusRecordWindow._focusTask.TimerSid = timerByObjId.Id;
            focusRecordWindow._focusTask.TimerName = focusRecordWindow._focusTask.Title;
          }
        }
        for (int i = 0; i < count; ++i)
        {
          PomodoroModel pomo = new PomodoroModel()
          {
            Id = Utils.GetGuid(),
            StartTime = focusRecordWindow._startTime.Value.AddMinutes((double) i * duration),
            EndTime = i == count - 1 ? focusRecordWindow._endTime.Value : focusRecordWindow._startTime.Value.AddMinutes((double) (i + 1) * duration),
            UserId = LocalSettings.Settings.LoginUserId,
            Added = true,
            Type = focusRecordWindow._focusType,
            Status = 1,
            Note = i == count - 1 ? focusRecordWindow.NoteEditor.Text : string.Empty
          };
          PomoTask pomoTask = focusRecordWindow._focusTask.Copy();
          pomoTask.PomoId = pomo.Id;
          pomoTask.StartTime = pomo.StartTime;
          pomoTask.EndTime = pomo.EndTime;
          pomo.Tasks = new PomoTask[1]{ pomoTask };
          LocalSettings.Settings.PomoLocalSetting.AddRecordType = focusRecordWindow._focusType == 0 ? "pomodoro" : "timing";
          LocalSettings.Settings.Save(true);
          await PomoService.SaveFocusModel(pomo);
        }
      }
      UserActCollectUtils.AddClickEvent("focus", "focus_tab", "add_focus_record");
      focusRecordWindow.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

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

    private void OnTaskClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (this._pomoFilter == null)
      {
        PomoFilterControl pomoFilterControl = new PomoFilterControl((Popup) this.TaskPopup, false);
        pomoFilterControl.ShowComplete = true;
        pomoFilterControl.DataContext = (object) new FocusViewModel();
        this._pomoFilter = pomoFilterControl;
      }
      this._pomoFilter.ClearItemSelectedEvent();
      this._pomoFilter.ItemSelected += (EventHandler<DisplayItemModel>) (async (obj, model) =>
      {
        TimerModel timerByIdOrObjId = await TimerDao.GetTimerByIdOrObjId(model.Id);
        this._focusTask.TimerSid = timerByIdOrObjId?.Id;
        this._focusTask.TimerName = timerByIdOrObjId?.Name;
        this.TaskTextBox.Text = model.IsHabit ? model.Habit.Name : model.Title;
        this.TaskTextBox.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
        this._focusTask.HabitId = model.IsHabit ? model.Habit.Id : (timerByIdOrObjId == null || !(timerByIdOrObjId.ObjType == "habit") ? (string) null : timerByIdOrObjId.ObjId);
        this._focusTask.Title = model.IsHabit ? model.Habit.Name : model.Title;
        TaskBaseViewModel taskBaseViewModel = model.IsTaskOrNote ? TaskCache.GetTaskById(model.Id) : TaskCache.GetTaskById(timerByIdOrObjId?.ObjId);
        this._focusTask.TaskId = taskBaseViewModel?.Id;
        this._focusTask.ProjectName = taskBaseViewModel?.ProjectName;
        this._focusTask.Tags = taskBaseViewModel?.Tags;
        this._focusTask.TagString = taskBaseViewModel?.Tag;
        this.TaskPopup.IsOpen = false;
      });
      this.TaskPopup.IsOpen = true;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Owner?.Activate();
      base.OnClosing(e);
    }

    private void OnStartTimeClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      DateTime? startTime1 = this._startTime;
      DateTime dateTime;
      DateTime time;
      if (!startTime1.HasValue)
      {
        ref DateTime? local = ref this._endTime;
        if (!local.HasValue)
        {
          dateTime = DateTime.Today.AddHours((double) DateTime.Now.Hour);
          time = dateTime.AddHours(-3.0);
        }
        else
        {
          dateTime = local.GetValueOrDefault();
          time = dateTime.AddMinutes((double) (-1 * this._pomoDuration));
        }
      }
      else
        time = startTime1.GetValueOrDefault();
      dateTime = DateTime.Today;
      dateTime = dateTime.AddDays(1.0);
      DateTime? maxDate = new DateTime?(dateTime.AddSeconds(-1.0));
      dateTime = DateTime.Today;
      DateTime? minDate = new DateTime?(dateTime.AddDays(-30.0));
      DateTimeSelector dateTimeSelector = new DateTimeSelector(time, maxDate, minDate);
      dateTimeSelector.TimeSelected += (EventHandler<DateTime>) ((o, t) =>
      {
        DateTime? startTime2 = this._startTime;
        this._startTime = new DateTime?(t);
        if (!this._endTime.HasValue && this._minutes.HasValue)
        {
          this._endTime = new DateTime?(t.AddMinutes((double) this._minutes.Value));
          this.SetEndText();
        }
        else if (startTime2.HasValue && this._endTime.HasValue)
        {
          double totalMinutes = (this._endTime.Value - startTime2.Value).TotalMinutes;
          this._endTime = new DateTime?(t.AddMinutes(totalMinutes));
          this.SetEndText();
        }
        this.SetStartText();
        this.SetFocusText();
        this.SetSaveButtonEnable();
        this.StartPopup.IsOpen = false;
      });
      dateTimeSelector.Cancel += (EventHandler) ((o, args) => this.StartPopup.IsOpen = false);
      this.StartPopup.Child = (UIElement) dateTimeSelector;
      this.StartPopup.IsOpen = true;
    }

    private void SetStartText()
    {
      if (!this._startTime.HasValue)
        return;
      this.StartTextBox.Text = this._startTime.Value.ToString("yyyy-MM-dd  ") + DateUtils.GetTimeText(this._startTime.Value);
      this.StartTextBox.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
    }

    private void SetEndText()
    {
      if (!this._endTime.HasValue)
        return;
      this.EndTextBox.Text = this._endTime.Value.ToString("yyyy-MM-dd  ") + DateUtils.GetTimeText(this._endTime.Value);
      this.EndTextBox.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      if (!this._startTime.HasValue)
        return;
      int totalMinutes = (int) (this._endTime.Value - this._startTime.Value).TotalMinutes;
      int num = totalMinutes / this._pomoDuration;
      if (totalMinutes - num * this._pomoDuration >= 5)
        ++num;
      this._pomoNum = num;
    }

    private void SetFocusText()
    {
      if (this._endTime.HasValue && this._startTime.HasValue)
        this._minutes = new int?((int) (this._endTime.Value - this._startTime.Value).TotalMinutes);
      if (!this._minutes.HasValue)
        return;
      int minutes = this._minutes.Value;
      if (this._focusType == 0)
        this.FocusTypeTextBox.Text = Utils.GetString("PomoTimer2") + " : " + this._pomoNum.ToString() + " " + Utils.GetString(this._pomoNum > 1 ? "EstimatePomoCounts" : "EstimatePomoCount");
      else
        this.FocusTypeTextBox.Text = Utils.GetString("Timing") + " : " + Utils.MinutesToHourMinuteText(minutes);
      this.FocusTypeTextBox.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
    }

    private void OnEndTimeClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      DateTime? endTime = this._endTime;
      DateTime dateTime;
      DateTime time;
      if (!endTime.HasValue)
      {
        ref DateTime? local = ref this._startTime;
        if (!local.HasValue)
        {
          time = DateTime.Today.AddHours((double) DateTime.Now.Hour);
        }
        else
        {
          dateTime = local.GetValueOrDefault();
          time = dateTime.AddMinutes((double) this._pomoDuration);
        }
      }
      else
        time = endTime.GetValueOrDefault();
      dateTime = DateTime.Today;
      dateTime = dateTime.AddDays(1.0);
      DateTime? maxDate = new DateTime?(dateTime.AddSeconds(-1.0));
      dateTime = DateTime.Today;
      DateTime? minDate = new DateTime?(dateTime.AddDays(-30.0));
      DateTimeSelector dateTimeSelector = new DateTimeSelector(time, maxDate, minDate);
      dateTimeSelector.TimeSelected += (EventHandler<DateTime>) ((o, t) =>
      {
        if (!this._startTime.HasValue && this._minutes.HasValue)
        {
          this._startTime = new DateTime?(t.AddMinutes((double) (-1 * this._minutes.Value)));
          this.SetStartText();
        }
        this._endTime = new DateTime?(t);
        this.SetEndText();
        this.SetFocusText();
        this.SetSaveButtonEnable();
        this.EndPopup.IsOpen = false;
      });
      dateTimeSelector.Cancel += (EventHandler) ((o, args) => this.EndPopup.IsOpen = false);
      this.EndPopup.Child = (UIElement) dateTimeSelector;
      this.EndPopup.IsOpen = true;
    }

    private void SetSaveButtonEnable()
    {
      if (!this._startTime.HasValue || !this._endTime.HasValue)
        this.SaveButton.IsEnabled = false;
      else if (this._endTime.Value > DateTime.Now)
      {
        this.Toast(Utils.GetString("AddFocusBeforeNow"));
        this.SaveButton.IsEnabled = false;
      }
      else if ((this._endTime.Value - this._startTime.Value).TotalMinutes < 5.0)
      {
        this.Toast(Utils.GetString("AddFocusMinMinutes"));
        this.SaveButton.IsEnabled = false;
      }
      else if (this._focusType == 1 && (this._endTime.Value - this._startTime.Value).TotalMinutes > 720.0)
      {
        this.Toast(Utils.GetString("AddFocusMaxDuration"));
        this.SaveButton.IsEnabled = false;
      }
      else if (this._focusType == 0 && this._pomoNum > 10)
      {
        this.Toast(Utils.GetString("AddFocusMaxCount"));
        this.SaveButton.IsEnabled = false;
      }
      else
        this.SaveButton.IsEnabled = true;
    }

    private void Toast(string text)
    {
      this.ToastBorder.Visibility = Visibility.Visible;
      this.ToastText.Text = text;
      this.ToastBorder.BeginStoryboard((Storyboard) this.FindResource((object) "ShowToast"));
    }

    private void OnToasted(object sender, EventArgs e)
    {
      this.ToastBorder.Visibility = Visibility.Collapsed;
    }

    private void OnTypeSaveClick(object sender, RoutedEventArgs e)
    {
      this._focusType = this.TypeSelector.GetSelectedIndex();
      if (this._focusType == 1)
      {
        int result1;
        int result2;
        if (int.TryParse(this.HourText.Text, out result1) && int.TryParse(this.MinuteText.Text, out result2))
        {
          int num = result1 * 60 + result2;
          if (num > 720)
            num = 720;
          if (num < 5)
            num = 5;
          this._minutes = new int?(num);
        }
      }
      else
      {
        int result;
        if (int.TryParse(this.PomoCount.Text, out result))
        {
          this._pomoNum = result;
          this._minutes = new int?(result * this._pomoDuration);
        }
      }
      if (this._minutes.HasValue)
      {
        if (this._startTime.HasValue)
          this._endTime = new DateTime?(this._startTime.Value.AddMinutes((double) this._minutes.Value));
        else if (this._endTime.HasValue)
          this._startTime = new DateTime?(this._endTime.Value.AddMinutes((double) (-1 * this._minutes.Value)));
      }
      this.SetStartText();
      this.SetEndText();
      this.SetFocusText();
      this.SetSaveButtonEnable();
      this.TypePopup.IsOpen = false;
    }

    private void OnTypeCancelClick(object sender, RoutedEventArgs e)
    {
      this.TypePopup.IsOpen = false;
    }

    private void OnTypeClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      this.TypeSelector.SetSelectedIndex(this._focusType);
      this.TypePopup.IsOpen = true;
      this.SetTypeValues();
    }

    private void SetTypeValues()
    {
      this.PomoGrid.Visibility = this._focusType == 0 ? Visibility.Visible : Visibility.Collapsed;
      this.TimerGrid.Visibility = this._focusType == 1 ? Visibility.Visible : Visibility.Collapsed;
      if (this._focusType == 1)
      {
        double num1;
        if (!this._startTime.HasValue || !this._endTime.HasValue)
        {
          num1 = (double) this._minutes.GetValueOrDefault();
        }
        else
        {
          DateTime? endTime = this._endTime;
          DateTime? startTime = this._startTime;
          num1 = (endTime.HasValue & startTime.HasValue ? new TimeSpan?(endTime.GetValueOrDefault() - startTime.GetValueOrDefault()) : new TimeSpan?()).Value.TotalMinutes;
        }
        double num2 = num1;
        NumInputTextBox hourText = this.HourText;
        int num3 = (int) num2 / 60;
        string str1 = num3.ToString() + string.Empty;
        hourText.Text = str1;
        NumInputTextBox minuteText = this.MinuteText;
        num3 = (int) num2 % 60;
        string str2 = num3.ToString() + string.Empty;
        minuteText.Text = str2;
      }
      else
        this.PomoCount.Text = this._pomoNum.ToString() ?? "";
    }

    private void OnTypeSelected(object sender, GroupTitleViewModel e)
    {
      this._focusType = e.Index;
      this.SetTypeValues();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/focusstatistics/addfocusrecordwindow.xaml", UriKind.Relative));
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
          ((Timeline) target).Completed += new EventHandler(this.OnToasted);
          break;
        case 2:
          this.TaskSelectGrid = (Grid) target;
          this.TaskSelectGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTaskClick);
          break;
        case 3:
          this.TaskTextBox = (EmjTextBlock) target;
          break;
        case 4:
          this.TaskPopup = (EscPopup) target;
          break;
        case 5:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnStartTimeClick);
          break;
        case 6:
          this.StartTextBox = (TextBlock) target;
          break;
        case 7:
          this.StartPopup = (EscPopup) target;
          break;
        case 8:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnEndTimeClick);
          break;
        case 9:
          this.EndTextBox = (TextBlock) target;
          break;
        case 10:
          this.EndPopup = (EscPopup) target;
          break;
        case 11:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTypeClick);
          break;
        case 12:
          this.FocusTypeTextBox = (TextBlock) target;
          break;
        case 13:
          this.TypePopup = (EscPopup) target;
          break;
        case 14:
          this.EstimatePanel = (StackPanel) target;
          break;
        case 15:
          this.TypeSelector = (GroupTitle) target;
          break;
        case 16:
          this.PomoGrid = (Grid) target;
          break;
        case 17:
          this.PomoCount = (NumInputTextBox) target;
          break;
        case 18:
          this.TimerGrid = (Grid) target;
          break;
        case 19:
          this.HourText = (NumInputTextBox) target;
          break;
        case 20:
          this.MinuteText = (NumInputTextBox) target;
          break;
        case 21:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnTypeSaveClick);
          break;
        case 22:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnTypeCancelClick);
          break;
        case 23:
          this.NoteEditor = (EmojiEditor) target;
          break;
        case 24:
          this.LengthText = (TextBlock) target;
          break;
        case 25:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 26:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 27:
          this.ToastBorder = (Border) target;
          break;
        case 28:
          this.ToastText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
