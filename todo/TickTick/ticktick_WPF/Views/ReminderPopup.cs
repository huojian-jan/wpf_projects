// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.ReminderPopup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Remind;
using TickTickDao;
using TickTickModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views
{
  public class ReminderPopup : MyWindow, IRemindPop, IComponentConnector
  {
    private readonly ReminderModel _reminder = new ReminderModel();
    private TaskModel _task;
    private int _currentIndex = -1;
    internal Border ContainerBorder;
    internal Image WindowIcon;
    internal TextBlock TimeText;
    internal Image RepeatIcon;
    internal Border FocusIcon;
    internal ScrollViewer Viewer;
    internal EmjTextBlock TitleTextBlock;
    internal StackPanel ContentPanel;
    internal Grid TaskOperationPanel;
    internal CustomComboBox ChooseRemindLaterComboBox;
    internal Button CompleteButton;
    internal Button CloseButton;
    private bool _contentLoaded;

    public ReminderPopup() => this.InitializeComponent();

    public ReminderPopup(ReminderModel reminder)
    {
      this.Left = SystemParameters.WorkArea.Width - 324.0;
      this.Top = SystemParameters.WorkArea.Height - 180.0;
      this.InitializeComponent();
      this._reminder = reminder;
      DateTime? nullable = reminder.ReminderTime;
      DateTime dateTime = nullable ?? DateTime.Now;
      int num = dateTime.Hour * 60 + dateTime.Minute;
      string key = num >= 540 ? (num >= 780 ? (num >= 1020 ? (num >= 1200 ? (!reminder.CanDelayTomorrow() ? "" : "TomorrowMorning") : "TodayNight") : "TodayEvening") : "TodayAfternoon") : "TodayMorning";
      ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
      observableCollection.Add(new ComboBoxViewModel((object) "15m", "15 " + Utils.GetString("PublicMinutes"), 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) "30m", "30 " + Utils.GetString("PublicMinutes"), 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) "1h", "1 " + Utils.GetString("PublicHour"), 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) "3h", "3 " + Utils.GetString("PublicHours"), 32.0));
      ObservableCollection<ComboBoxViewModel> items = observableCollection;
      if (!string.IsNullOrEmpty(key))
        items.Add(new ComboBoxViewModel((object) key, Utils.GetString(key), 32.0));
      if (reminder.CanDelayTomorrow())
        items.Add(new ComboBoxViewModel((object) "Tomorrow", Utils.GetString("Tomorrow"), 32.0));
      this.ChooseRemindLaterComboBox.Init<ComboBoxViewModel>(items);
      this.LoadData();
      DataChangedNotifier.SyncDone += new EventHandler<SyncResult>(this.OnSyncDone);
      ticktick_WPF.Notifier.GlobalEventManager.RemindHandled += new EventHandler<RemindMessage>(this.OnRemindHandled);
      this.SetWindowIcon();
      nullable = reminder.StartDate;
      DateTime date = nullable ?? reminder.ReminderTime ?? DateTime.Now;
      this.TimeText.Text = ((int) reminder.IsAllDay ?? 1) != 0 ? ticktick_WPF.Util.DateUtils.FormatSmartDateShortString(date) : ticktick_WPF.Util.DateUtils.FormatSmartDateShortString(date) + ", " + ticktick_WPF.Util.DateUtils.FormatHourMinute(date);
      this.Visibility = Visibility.Hidden;
      this.ChooseRemindLaterComboBox.SetPopupPosition(PlacementMode.Top, -5, 2);
    }

    private void SetWindowIcon()
    {
      this.WindowIcon.Source = (ImageSource) AppIconUtils.GetIconImage();
    }

    private void OnRemindHandled(object sender, RemindMessage e)
    {
      if (!this.CheckRemindMatched(this._reminder, e))
        return;
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.TryClose));
    }

    private bool CheckRemindMatched(ReminderModel reminder, RemindMessage remindMessage)
    {
      if (remindMessage.type == "habit" || !reminder.ReminderTime.HasValue || (reminder.ReminderTime.Value - remindMessage.remindTime).Minutes > 0)
        return false;
      switch (remindMessage.type)
      {
        case "calendar":
          return reminder.EventId == remindMessage.id;
        case "course":
          return reminder.Id == remindMessage.id;
        case "checklist":
          return reminder.CheckItemId == remindMessage.id;
        case "task":
          return reminder.TaskId == remindMessage.id;
        default:
          return false;
      }
    }

    private async void OnSyncDone(object sender, SyncResult result)
    {
      if (!await ReminderCalculator.IsReminderExpired(this._reminder))
        return;
      this.TryClose();
    }

    private async Task LoadData()
    {
      bool flag1 = LocalSettings.Settings.ExtraSettings.RemindDetail == 2;
      if (!flag1)
      {
        bool flag2 = LocalSettings.Settings.ExtraSettings.RemindDetail == 1;
        if (flag2)
          flag2 = await AppLockCache.GetAppLocked();
        flag1 = flag2;
      }
      bool hideDetail = flag1;
      if (hideDetail)
        this.TaskOperationPanel.Visibility = Visibility.Collapsed;
      if (this._reminder.Type == 2)
        this.LoadEventData(hideDetail);
      else if (this._reminder.Type == 8)
        this.LoadCourseData();
      else
        await this.LoadTaskData(hideDetail);
      if (!string.IsNullOrEmpty(this.TitleTextBlock.Text))
        return;
      this.TitleTextBlock.Opacity = 0.6;
      this.TitleTextBlock.Text = Utils.GetString("NoTitle");
    }

    private void LoadEventData(bool hideDetail)
    {
      this.CompleteButton.Visibility = Visibility.Collapsed;
      this.CloseButton.Visibility = Visibility.Visible;
      if (hideDetail)
      {
        this.TitleTextBlock.Text = Utils.GetString("ReminderWhenLock");
        this.ContentPanel.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.TitleTextBlock.Text = this._reminder.Title;
        this.RepeatIcon.Visibility = string.IsNullOrEmpty(this._reminder.RepeatFlag) ? Visibility.Collapsed : Visibility.Visible;
        if (string.IsNullOrWhiteSpace(this._reminder.Content))
          this.ContentPanel.Visibility = Visibility.Collapsed;
        else
          this.AddContentText(new List<string>()
          {
            this._reminder.Content
          });
      }
    }

    private void AddContentText(List<string> texts, int highlightIndex = -1)
    {
      for (int index = 0; index < texts.Count; ++index)
      {
        string text = texts[index];
        if (index != 0 || !string.IsNullOrWhiteSpace(text))
        {
          EmjTextBlock emjTextBlock = new EmjTextBlock();
          emjTextBlock.TextWrapping = TextWrapping.Wrap;
          emjTextBlock.FontSize = 14.0;
          emjTextBlock.LineHeight = 20.0;
          emjTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
          emjTextBlock.VerticalAlignment = VerticalAlignment.Top;
          emjTextBlock.Text = text;
          emjTextBlock.Margin = new Thickness(0.0, 0.0, 0.0, index == 0 ? 2.0 : 0.0);
          EmjTextBlock element1 = emjTextBlock;
          element1.SetResourceReference(Control.ForegroundProperty, (object) "BaseSolidColorOpacity50");
          if (index == highlightIndex)
          {
            Grid element2 = new Grid();
            element1.SetResourceReference(TextBlock.ForegroundProperty, (object) "PrimaryColor");
            element2.Children.Add((UIElement) element1);
            Border border = new Border();
            border.CornerRadius = new CornerRadius(4.0);
            border.RenderTransform = (Transform) new TranslateTransform(0.0, -1.0);
            border.Margin = new Thickness(0.0);
            border.VerticalAlignment = VerticalAlignment.Stretch;
            border.HorizontalAlignment = HorizontalAlignment.Stretch;
            Border element3 = border;
            element3.SetResourceReference(Control.BackgroundProperty, (object) "PrimaryColor10");
            element2.Children.Add((UIElement) element3);
            this.ContentPanel.Children.Add((UIElement) element2);
            this.UpdateLayout();
            this.Viewer.ScrollToVerticalOffset(element2.TranslatePoint(new System.Windows.Point(), (UIElement) this.Viewer).Y);
          }
          else
            this.ContentPanel.Children.Add((UIElement) element1);
        }
      }
    }

    private void LoadCourseData()
    {
      this.CompleteButton.Visibility = Visibility.Collapsed;
      this.CloseButton.Visibility = Visibility.Visible;
      this.TitleTextBlock.Text = this._reminder.Title;
      if (string.IsNullOrWhiteSpace(this._reminder.Content))
        this.ContentPanel.Visibility = Visibility.Collapsed;
      else
        this.AddContentText(new List<string>()
        {
          this._reminder.Content
        });
    }

    private async Task LoadTaskData(bool hideDetail)
    {
      ReminderPopup reminderPopup = this;
      // ISSUE: reference to a compiler-generated method
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(reminderPopup.\u003CLoadTaskData\u003Eb__13_0));
      if (!hideDetail)
      {
        ProjectModel projectModel = project;
        if ((projectModel != null ? (projectModel.IsProjectPermit() ? 1 : 0) : 1) != 0)
          goto label_3;
      }
      reminderPopup.TaskOperationPanel.Visibility = Visibility.Collapsed;
label_3:
      TaskModel task = await TaskDao.GetThinTaskById(reminderPopup._reminder.TaskId);
      if (task != null)
      {
        reminderPopup._task = task;
        if (hideDetail)
        {
          reminderPopup.TitleTextBlock.Text = Utils.GetString("ReminderWhenLock");
          reminderPopup.ContentPanel.Visibility = Visibility.Collapsed;
          project = (ProjectModel) null;
          task = (TaskModel) null;
          return;
        }
        reminderPopup.TitleTextBlock.Text = task.title;
        reminderPopup._reminder.Title = task.title;
        if (task.kind == "CHECKLIST")
        {
          List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(reminderPopup._reminder.TaskId);
          List<string> texts = new List<string>();
          texts.Add(task.desc?.Trim() ?? string.Empty);
          int highlightIndex = -1;
          // ISSUE: explicit non-virtual call
          if (checkItemsByTaskId != null && __nonvirtual (checkItemsByTaskId.Count) > 0)
          {
            checkItemsByTaskId.Sort((Comparison<TaskDetailItemModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
            for (int index = 0; index < checkItemsByTaskId.Count; ++index)
            {
              TaskDetailItemModel taskDetailItemModel = checkItemsByTaskId[index];
              texts.Add("- " + taskDetailItemModel.title.Trim());
              if (taskDetailItemModel.id == reminderPopup._reminder.CheckItemId)
                highlightIndex = index + 1;
            }
          }
          if (reminderPopup._reminder.Type == 1)
            reminderPopup.CompleteButton.Content = (object) Utils.GetString("CompleteCheckItem");
          reminderPopup.AddContentText(texts, highlightIndex);
        }
        else
        {
          reminderPopup._reminder.Content = TaskUtils.ReplaceAttachmentTextInString(task.content);
          if (string.IsNullOrWhiteSpace(reminderPopup._reminder.Content))
            reminderPopup.ContentPanel.Visibility = Visibility.Collapsed;
          else
            reminderPopup.AddContentText(new List<string>()
            {
              reminderPopup._reminder.Content
            });
        }
        if (!string.IsNullOrEmpty(reminderPopup._task.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) reminderPopup._task))
        {
          reminderPopup.CloseButton.Visibility = Visibility.Visible;
          reminderPopup.CompleteButton.Visibility = Visibility.Collapsed;
        }
        if (reminderPopup._task.kind == "NOTE")
        {
          reminderPopup.CloseButton.Visibility = Visibility.Visible;
          reminderPopup.CompleteButton.Visibility = Visibility.Collapsed;
        }
      }
      if (LocalSettings.Settings.EnableFocus)
      {
        Border focusIcon = reminderPopup.FocusIcon;
        ProjectModel projectModel = project;
        int num = (projectModel != null ? (projectModel.IsProjectPermit() ? 1 : 0) : 0) == 0 || hideDetail || !(reminderPopup._task.kind != "NOTE") || reminderPopup._reminder.Type == 1 ? 2 : 0;
        focusIcon.Visibility = (Visibility) num;
      }
      reminderPopup.RepeatIcon.Visibility = string.IsNullOrEmpty(reminderPopup._task.repeatFlag) ? Visibility.Collapsed : Visibility.Visible;
      project = (ProjectModel) null;
      task = (TaskModel) null;
    }

    private void RemindLaterButtonClick(object sender, RoutedEventArgs e)
    {
      this.ChooseRemindLaterComboBox.IsOpen = true;
    }

    private async void CompleteClick(object sender, RoutedEventArgs e)
    {
      if (this._reminder != null)
      {
        App.Instance.StopReminderTimer();
        switch (this._reminder.Type)
        {
          case 0:
            TaskModel thinTaskById = await TaskDao.GetThinTaskById(this._reminder.TaskId);
            if (thinTaskById != null && thinTaskById.status == 0 && this._task != null && thinTaskById.status == 0)
            {
              TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(this._task, 2);
              break;
            }
            break;
          case 1:
            await TaskDetailItemService.CompleteCheckItem(this._reminder.CheckItemId, completeOnly: true);
            break;
        }
        this.AddClickEvent("done");
        SyncManager.TryDelaySync();
        App.Instance.StartReminderTimer();
      }
      this.TryClose();
    }

    private async void ReminderSelectionChanged(object sender, ComboBoxViewModel e)
    {
      ReminderModel reminder = this._reminder.Copy();
      DateTime? reminderTime;
      if (e.Value is string str)
      {
        string data = str;
        if (str != null)
        {
          switch (str.Length)
          {
            case 2:
              switch (str[0])
              {
                case '1':
                  if (str == "1h")
                  {
                    reminder.ReminderTime = new DateTime?(DateTime.Now.AddHours(1.0));
                    break;
                  }
                  break;
                case '3':
                  if (str == "3h")
                  {
                    reminder.ReminderTime = new DateTime?(DateTime.Now.AddHours(3.0));
                    break;
                  }
                  break;
              }
              break;
            case 3:
              switch (str[0])
              {
                case '1':
                  if (str == "15m")
                  {
                    reminder.ReminderTime = new DateTime?(DateTime.Now.AddMinutes(15.0));
                    break;
                  }
                  break;
                case '3':
                  if (str == "30m")
                  {
                    reminder.ReminderTime = new DateTime?(DateTime.Now.AddMinutes(30.0));
                    break;
                  }
                  break;
              }
              break;
            case 8:
              if (str == "Tomorrow")
              {
                data = "tomorrow";
                reminder.ReminderTime = new DateTime?(DateTime.Now.AddDays(1.0));
                break;
              }
              break;
            case 10:
              if (str == "TodayNight")
              {
                data = "smart_time";
                ReminderModel reminderModel = reminder;
                reminderTime = reminder.ReminderTime;
                DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddHours(20.0));
                reminderModel.ReminderTime = nullable;
                break;
              }
              break;
            case 12:
              switch (str[5])
              {
                case 'E':
                  if (str == "TodayEvening")
                  {
                    data = "smart_time";
                    ReminderModel reminderModel = reminder;
                    reminderTime = reminder.ReminderTime;
                    DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddHours(17.0));
                    reminderModel.ReminderTime = nullable;
                    break;
                  }
                  break;
                case 'M':
                  if (str == "TodayMorning")
                  {
                    data = "smart_time";
                    ReminderModel reminderModel = reminder;
                    reminderTime = reminder.ReminderTime;
                    DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddHours(9.0));
                    reminderModel.ReminderTime = nullable;
                    break;
                  }
                  break;
              }
              break;
            case 14:
              if (str == "TodayAfternoon")
              {
                data = "smart_time";
                ReminderModel reminderModel = reminder;
                reminderTime = reminder.ReminderTime;
                DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddHours(13.0));
                reminderModel.ReminderTime = nullable;
                break;
              }
              break;
            case 15:
              if (str == "TomorrowMorning")
              {
                data = "smart_time";
                ReminderModel reminderModel = reminder;
                reminderTime = reminder.ReminderTime;
                DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddDays(1.0).AddHours(9.0));
                reminderModel.ReminderTime = nullable;
                break;
              }
              break;
          }
        }
        UserActCollectUtils.AddClickEvent("reminder_data", "snooze", data);
      }
      App.ReminderList.Add(reminder);
      this.AddClickEvent("snooze");
      if (reminder.Type == 0)
      {
        reminderTime = reminder.ReminderTime;
        if (reminderTime.HasValue)
        {
          string taskId = reminder.TaskId;
          reminderTime = reminder.ReminderTime;
          DateTime snoozeTime = reminderTime.Value;
          await TaskService.SetSnoozeTime(taskId, snoozeTime);
        }
      }
      else if (reminder.Type == 1)
      {
        TaskDetailItemModel checklistItemById = await TaskDetailItemDao.GetChecklistItemById(reminder.CheckItemId);
        if (checklistItemById != null)
        {
          reminderTime = reminder.ReminderTime;
          if (reminderTime.HasValue)
          {
            TaskDetailItemModel taskDetailItemModel = checklistItemById;
            reminderTime = reminder.ReminderTime;
            DateTime? nullable = new DateTime?(reminderTime.Value);
            taskDetailItemModel.snoozeReminderTime = nullable;
            await TaskDetailItemDao.SaveChecklistItem(checklistItemById);
            await SyncStatusDao.AddSyncStatus(reminder.TaskId, 0);
          }
        }
      }
      if (reminder.Type != 1)
        await ReminderDelayDao.AddModelAsync(new ReminderDelayModel()
        {
          UserId = LocalSettings.Settings.LoginUserId,
          ObjId = reminder.GetObjId(),
          Type = reminder.GetObjType(),
          RemindTime = this._reminder.StartDate,
          NextTime = reminder.ReminderTime,
          SyncStatus = 0
        });
      SyncManager.TryDelaySync();
      this.TryClose();
      reminder = (ReminderModel) null;
    }

    private void AddClickEvent(string label)
    {
      string ctype = "task_reminder_dialog";
      if (this._reminder.Type == 2)
        ctype = "calendar_reminder_dialog";
      else if (this._reminder.Type == 8)
        ctype = "timetable_reminder_dialog";
      UserActCollectUtils.AddClickEvent("reminder", ctype, label);
    }

    private void OnPopupClick(object sender, MouseButtonEventArgs e)
    {
      this.AddClickEvent("click_content");
      if (this._reminder.Type == 2)
        App.NavigateEvent(this._reminder.EventId);
      else
        App.ShowMainWindow(this._reminder.TaskId);
      this.TryClose();
    }

    private void OnCloseIconClick(object sender, MouseButtonEventArgs e)
    {
      this.AddClickEvent("x_btn");
      this.TryClose();
    }

    private void OnFocusClick(object sender, MouseButtonEventArgs e)
    {
      if (!string.IsNullOrEmpty(this._reminder.TaskId))
      {
        this.AddClickEvent("start_focus");
        if (!TickFocusManager.ConfirmSwitch(this._reminder.TaskId, this._reminder.Title, Window.GetWindow((DependencyObject) this)))
          return;
        TickFocusManager.StartFocus(this._reminder.TaskId, checkTimer: true);
      }
      this.TryClose();
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Mouse.LeftButton != MouseButtonState.Pressed)
        return;
      this.LocationChanged -= new EventHandler(this.OnLocationChanged);
      this.LocationChanged += new EventHandler(this.OnLocationChanged);
      this.DragMove();
    }

    private void OnLocationChanged(object sender, EventArgs e)
    {
      ReminderWindowManager.OnWindowMoved((IRemindPop) this);
    }

    public bool IsSameReminder(ReminderModel reminder)
    {
      if (reminder.Id != this._reminder.Id)
        return false;
      return !string.IsNullOrEmpty(reminder.TaskId) ? this._reminder.TaskId == reminder.TaskId && this._reminder.CheckItemId == reminder.CheckItemId : !string.IsNullOrEmpty(reminder.EventId) && this._reminder.EventId == reminder.EventId;
    }

    public void SetDisplayStyle(int index)
    {
      bool flag = true;
      if (this._currentIndex < 0 && index == 2)
      {
        this._currentIndex = index - 1;
        flag = false;
      }
      int num1 = this._currentIndex < 0 ? (index > 0 ? index + 1 : index) : this._currentIndex;
      if (this.ContainerBorder.RenderTransform is ScaleTransform renderTransform)
      {
        if (flag)
        {
          DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(1.0 - 0.05 * (double) num1), 1.0 - 0.05 * (double) index, 180);
          renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) doubleAnimation);
          renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) doubleAnimation);
        }
        else
        {
          renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) null);
          renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) null);
          renderTransform.ScaleX = 1.0 - 0.05 * (double) index;
          renderTransform.ScaleY = 1.0 - 0.05 * (double) index;
        }
      }
      double num2 = SystemParameters.WorkArea.Height - 180.0;
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(flag ? num2 - (double) (8 * num1) : num2 - (double) (8 * index) + 2.0), num2 - (double) (8 * index), 180);
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) doubleAnimation1);
      this._currentIndex = index;
      this.Opacity = 1.0;
    }

    public void TryHide()
    {
      this._currentIndex = -1;
      this.Opacity = 0.0;
      if (!(this.ContainerBorder.RenderTransform is ScaleTransform renderTransform))
        return;
      renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) null);
      renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) null);
      renderTransform.ScaleX = 0.9;
      renderTransform.ScaleY = 0.9;
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) null);
      this.Top = SystemParameters.WorkArea.Height - 196.0;
    }

    public void TryClose()
    {
      Communicator.NotifyCloseReminder(new RemindMessage(this._reminder));
      this.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      DataChangedNotifier.SyncDone -= new EventHandler<SyncResult>(this.OnSyncDone);
      ticktick_WPF.Notifier.GlobalEventManager.RemindHandled -= new EventHandler<RemindMessage>(this.OnRemindHandled);
      base.OnClosing(e);
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
      this.AddClickEvent("got_it_btn");
      this.TryClose();
    }

    public void ShowWindow()
    {
      IntPtr handle = new WindowInteropHelper((Window) this).Handle;
      this.Opacity = 1.0;
      this.Visibility = Visibility.Visible;
      if (handle != IntPtr.Zero)
      {
        this.Topmost = false;
        this.Topmost = true;
        NativeUtils.ShowWindow(handle, 4);
      }
      else
        this.Show();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/reminderpopup.xaml", UriKind.Relative));
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
          this.ContainerBorder = (Border) target;
          break;
        case 2:
          this.WindowIcon = (Image) target;
          break;
        case 3:
          this.TimeText = (TextBlock) target;
          break;
        case 4:
          this.RepeatIcon = (Image) target;
          break;
        case 5:
          ((UIElement) target).PreviewMouseDown += new MouseButtonEventHandler(this.OnPreviewMouseDown);
          break;
        case 6:
          this.FocusIcon = (Border) target;
          this.FocusIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFocusClick);
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseIconClick);
          break;
        case 8:
          this.Viewer = (ScrollViewer) target;
          this.Viewer.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnPopupClick);
          break;
        case 9:
          this.TitleTextBlock = (EmjTextBlock) target;
          break;
        case 10:
          this.ContentPanel = (StackPanel) target;
          break;
        case 11:
          this.TaskOperationPanel = (Grid) target;
          break;
        case 12:
          this.ChooseRemindLaterComboBox = (CustomComboBox) target;
          break;
        case 13:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.RemindLaterButtonClick);
          break;
        case 14:
          this.CompleteButton = (Button) target;
          this.CompleteButton.Click += new RoutedEventHandler(this.CompleteClick);
          break;
        case 15:
          this.CloseButton = (Button) target;
          this.CloseButton.Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
