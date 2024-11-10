// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarDetailControl
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Time;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarDetailControl : UserControl, IComponentConnector, IStyleConnector
  {
    private string _originContent;
    private string _originTitle;
    internal CalendarDetailControl Root;
    internal ScrollViewer ScrollViewer;
    internal Grid BackGrid;
    internal DetailTextBox TitleText;
    internal TextBlock TitleTextHint;
    internal Grid DateGrid;
    internal StackPanel ReminderText;
    internal Run ReminderRun;
    internal Run SnoozeRun;
    internal DetailTextBox LocationText;
    internal TextBlock LocationTextHint;
    internal Grid DescriptionGrid;
    internal DetailTextBox NewContentText;
    internal TextBlock NewContentTextHint;
    internal StackPanel CalendarMovePanel;
    internal EscPopup SetProjectPopup;
    internal Grid MoreGrid;
    internal EscPopup MorePopup;
    private bool _contentLoaded;

    public event EventHandler InOperate;

    public event EventHandler StopOperate;

    public event EventHandler HideDetail;

    public event EventHandler<bool> EventArchiveChanged;

    public string EventId => this.GetModel()?.Id;

    public CalendarDetailControl()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    public bool Editable
    {
      get => this.DataContext is CalendarDetailViewModel dataContext && dataContext.Editable;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.TitleText.LinkTextChange += new EventHandler(this.OnTitleChanged);
      CalendarEventChangeNotifier.TitleChanged += new EventHandler<TextExtra>(this.OnEventTitleChanged);
      this.ScrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(this.OnMouseScrollOnDetail);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      this.TitleText.LinkTextChange -= new EventHandler(this.OnTitleChanged);
      CalendarEventChangeNotifier.TitleChanged -= new EventHandler<TextExtra>(this.OnEventTitleChanged);
      this.ScrollViewer.PreviewMouseWheel -= new MouseWheelEventHandler(this.OnMouseScrollOnDetail);
    }

    private void OnMouseScrollOnDetail(object sender, MouseWheelEventArgs e)
    {
      if (e == null)
        return;
      this.ScrollViewer.ScrollToVerticalOffset(this.ScrollViewer.VerticalOffset - (double) e.Delta / 2.0);
    }

    private void OnEventTitleChanged(object sender, TextExtra extra)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null || !(extra.Id == model.Id) || !(model.Title != extra.Text))
        return;
      this.TitleText.LinkTextChange -= new EventHandler(this.OnTitleChanged);
      model.Title = extra.Text;
      this.TitleText.SetText(extra.Text);
      this.TitleText.LinkTextChange += new EventHandler(this.OnTitleChanged);
    }

    private void OnBackClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler hideDetail = this.HideDetail;
      if (hideDetail == null)
        return;
      hideDetail((object) this, (EventArgs) null);
    }

    private async void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null)
        return;
      Grid moreGrid = this.MoreGrid;
      DateTime? nullable1;
      int num;
      if (!model.Editable)
      {
        nullable1 = model.DueDate;
        DateTime today1 = DateTime.Today;
        if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() > today1 ? 1 : 0) : 0) == 0)
        {
          nullable1 = model.StartDate;
          DateTime today2 = DateTime.Today;
          if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() >= today2 ? 1 : 0) : 0) == 0)
          {
            num = 2;
            goto label_6;
          }
        }
      }
      num = 0;
label_6:
      moreGrid.Visibility = (Visibility) num;
      this.TitleText.SetTextOffset(model.Title, true, true);
      this.TitleTextHint.Visibility = string.IsNullOrEmpty(this.TitleText.Text) ? Visibility.Visible : Visibility.Collapsed;
      this.NewContentText.SetText(model.Content);
      this.NewContentTextHint.Visibility = string.IsNullOrEmpty(this.NewContentText.Text) ? Visibility.Visible : Visibility.Collapsed;
      this.LocationText.SetText(model.Location);
      this.LocationTextHint.Visibility = string.IsNullOrEmpty(this.LocationText.Text) ? Visibility.Visible : Visibility.Collapsed;
      if (model.IsAllDay)
      {
        nullable1 = model.DueDate;
        if (nullable1.HasValue)
        {
          nullable1 = model.StartDate;
          if (nullable1.HasValue)
          {
            nullable1 = model.StartDate;
            DateTime? dueDate = model.DueDate;
            if ((nullable1.HasValue == dueDate.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == dueDate.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
            {
              CalendarDetailViewModel calendarDetailViewModel = model;
              dueDate = model.DueDate;
              DateTime? nullable2 = new DateTime?(dueDate.Value.AddDays(1.0));
              calendarDetailViewModel.DueDate = nullable2;
            }
          }
        }
      }
      this._originTitle = model.Title;
      this._originContent = model.Content;
      if (model.IsNew)
      {
        this.TitleText.CanParseDate = true;
        this.MoreGrid.Visibility = Visibility.Collapsed;
      }
      else
        this.TitleText.CanParseDate = false;
      this.ReminderRun.Text = ReminderUtils.GetCalendarReminderText((IEnumerable<int>) model.Reminders, model.IsAllDay);
      this.ReminderText.Visibility = string.IsNullOrEmpty(this.ReminderRun.Text) ? Visibility.Collapsed : Visibility.Visible;
      string snoozeText = await model.GetSnoozeText();
      this.SnoozeRun.Text = string.IsNullOrEmpty(snoozeText) ? string.Empty : "  " + snoozeText;
    }

    private void OnTextLostFocus(object sender, RoutedEventArgs e) => SyncManager.TryDelaySync();

    private void OnLocationChanged(object sender, EventArgs e)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (string.IsNullOrEmpty(this.LocationText.Text))
        this.LocationTextHint.Visibility = Visibility.Visible;
      else
        this.LocationTextHint.Visibility = Visibility.Collapsed;
      if (model == null || !(model.Location != this.LocationText.Text))
        return;
      model.Location = this.LocationText.Text;
      CalendarService.SaveEventLocation(model.Id, model.Location);
    }

    private void OnCalendarContentChanged(object sender, EventArgs e)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (string.IsNullOrEmpty(this.NewContentText.Text))
        this.NewContentTextHint.Visibility = Visibility.Visible;
      else
        this.NewContentTextHint.Visibility = Visibility.Collapsed;
      if (model == null || !(this.NewContentText.Text != this._originContent))
        return;
      CalendarEventChangeNotifier.NotifySummaryChanged(model.Id, this.NewContentText.Text);
      CalendarService.SaveEventContent(model.Id, this.NewContentText.Text);
    }

    private async void OnTitleChanged(object sender, EventArgs e)
    {
      CalendarDetailControl calendarDetailControl = this;
      if (string.IsNullOrEmpty(calendarDetailControl.TitleText.Text))
        calendarDetailControl.TitleTextHint.Visibility = Visibility.Visible;
      else
        calendarDetailControl.TitleTextHint.Visibility = Visibility.Collapsed;
      CalendarEventChangeNotifier.TitleChanged -= new EventHandler<TextExtra>(calendarDetailControl.OnEventTitleChanged);
      CalendarDetailViewModel model = calendarDetailControl.GetModel();
      if (model != null && calendarDetailControl.TitleText.Text != calendarDetailControl._originTitle && !model.IsNew)
        await CalendarService.SaveEventTitle(model.Id, calendarDetailControl.TitleText.Text);
      CalendarEventChangeNotifier.TitleChanged += new EventHandler<TextExtra>(calendarDetailControl.OnEventTitleChanged);
    }

    private CalendarDetailViewModel GetModel()
    {
      return this.DataContext != null && this.DataContext is CalendarDetailViewModel dataContext ? dataContext : (CalendarDetailViewModel) null;
    }

    private void OnDateClick(object sender, MouseButtonEventArgs e)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null || !model.Editable || !model.StartDate.HasValue)
        return;
      if (!model.DueDate.HasValue)
        model.DueDate = new DateTime?(model.IsAllDay ? model.StartDate.Value.AddDays(1.0) : model.StartDate.Value.AddHours(1.0));
      TimeData timeData = new TimeData()
      {
        StartDate = model.StartDate,
        DueDate = model.DueDate,
        IsAllDay = new bool?(model.IsAllDay),
        RepeatFlag = model.RepeatFlag,
        IsDefault = false
      };
      if (model.Reminders != null && model.Reminders.Any<int>())
      {
        List<TaskReminderModel> taskReminderModelList = new List<TaskReminderModel>();
        foreach (int reminder in model.Reminders)
          taskReminderModelList.Add(new TaskReminderModel()
          {
            trigger = TriggerUtils.ReminderToTrigger(reminder)
          });
        timeData.Reminders = taskReminderModelList;
      }
      bool showRemind = CalendarEventDao.GetCalendarAccount(model.CalendarId)?.Kind != "icloud";
      SetDateDialog dialog = SetDateDialog.GetDialog();
      dialog.ClearEventHandle();
      dialog.Save += new EventHandler<TimeData>(this.OnTimeSelect);
      dialog.Hided += new EventHandler(this.OnPopupClosed);
      dialog.Show(timeData, new SetDateDialogArgs(true, target: (UIElement) this.DateGrid, vOffset: -10.0, placement: PlacementMode.Bottom, showQuickDate: false, showRemind: showRemind));
      EventHandler inOperate = this.InOperate;
      if (inOperate != null)
        inOperate((object) this, (EventArgs) null);
      e.Handled = true;
    }

    private async void OnTimeSelect(object sender, TimeData timeData)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null)
      {
        model = (CalendarDetailViewModel) null;
      }
      else
      {
        TimeData timeData1 = timeData;
        DateTime? nullable1;
        int num;
        if (timeData1 == null)
        {
          num = 0;
        }
        else
        {
          nullable1 = timeData1.StartDate;
          num = nullable1.HasValue ? 1 : 0;
        }
        if (num == 0)
        {
          model = (CalendarDetailViewModel) null;
        }
        else
        {
          model.IsAllDay = ((int) timeData.IsAllDay ?? 1) != 0;
          if (model.IsAllDay)
          {
            CalendarDetailViewModel calendarDetailViewModel1 = model;
            nullable1 = timeData.StartDate;
            DateTime? nullable2 = new DateTime?(nullable1.Value.Date);
            calendarDetailViewModel1.StartDate = nullable2;
            CalendarDetailViewModel calendarDetailViewModel2 = model;
            nullable1 = timeData.DueDate;
            DateTime dateTime;
            if (!nullable1.HasValue)
            {
              DateTime date = model.StartDate.Value;
              date = date.Date;
              dateTime = date.AddDays(1.0);
            }
            else
              dateTime = nullable1.GetValueOrDefault();
            DateTime? nullable3 = new DateTime?(dateTime);
            calendarDetailViewModel2.DueDate = nullable3;
          }
          else
          {
            model.StartDate = timeData.StartDate;
            model.DueDate = timeData.DueDate;
          }
          model.RepeatFlag = timeData.RepeatFlag;
          model.RepeatText = RRuleUtils.RRule2String(string.Empty, model.RepeatFlag, model.StartDate);
          if (timeData.Reminders.Any<TaskReminderModel>())
          {
            List<int> intList = new List<int>();
            foreach (TaskReminderModel reminder in timeData.Reminders)
              intList.Add(TriggerUtils.TriggerToReminder(reminder.trigger));
            model.Reminders = intList;
          }
          else
            model.Reminders = new List<int>();
          timeData.StartDate = model.StartDate;
          timeData.DueDate = model.DueDate;
          if (!model.IsNew)
          {
            await CalendarService.SaveEventTime(model.Id, timeData);
            this.NotifyEventChanged();
            ReminderDelayDao.DeleteByIdAsync(model.Id, "calendar");
          }
          this.ReminderRun.Text = ReminderUtils.GetCalendarReminderText((IEnumerable<int>) model.Reminders, model.IsAllDay);
          this.ReminderText.Visibility = string.IsNullOrEmpty(this.ReminderRun.Text) ? Visibility.Collapsed : Visibility.Visible;
          this.SnoozeRun.Text = string.Empty;
          model = (CalendarDetailViewModel) null;
        }
      }
    }

    private void OnAttendeeClick(object sender, MouseButtonEventArgs e)
    {
      Utils.Toast(Utils.GetString("EditingIsNotSupportedYet"));
    }

    private async void OnCalendarDelete()
    {
      CalendarDetailControl child = this;
      CalendarDetailViewModel model = child.GetModel();
      if (model != null)
      {
        string id = ArchivedDao.GetOriginalId(model.Id);
        await CalendarService.DeleteEvent(id);
        CalendarEventModel eventById = await CalendarEventDao.GetEventById(id);
        if (eventById != null)
        {
          UndoToast uiElement = new UndoToast();
          uiElement.InitEventUndo(eventById.EventId, model.Title);
          Utils.GetToastWindow().Toast((FrameworkElement) uiElement);
        }
        id = (string) null;
      }
      ListViewContainer parent = Utils.FindParent<ListViewContainer>((DependencyObject) child);
      if (parent == null)
      {
        model = (CalendarDetailViewModel) null;
      }
      else
      {
        parent.TryExtractDetail();
        model = (CalendarDetailViewModel) null;
      }
    }

    private void OnSelectCalendarClick(object sender, MouseButtonEventArgs e)
    {
      if (this.SetProjectPopup.IsOpen)
        return;
      e.Handled = true;
      CalendarDetailViewModel model = this.GetModel();
      if (model == null || !model.Editable)
        return;
      if (!model.Organizer)
      {
        Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.TryToastString((object) null, Utils.GetString("NotOrganizerToMove"));
      }
      else
      {
        BindCalendarModel bindCalendarModel = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == model.CalendarId));
        if (bindCalendarModel == null)
          return;
        ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.SetProjectPopup, new ProjectExtra()
        {
          SubscribeCalendars = new List<string>()
          {
            model.CalendarId
          }
        }, new ProjectSelectorExtra()
        {
          isCalendarMode = true,
          batchMode = false,
          accountId = bindCalendarModel.AccountId
        });
        projectOrGroupPopup.ItemSelect -= new EventHandler<SelectableItemViewModel>(this.CalendarItemSelect);
        projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.CalendarItemSelect);
        projectOrGroupPopup.Show();
      }
    }

    private async void CalendarItemSelect(object sender, SelectableItemViewModel selectableModel)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null)
        return;
      SubscribeCalendarViewModel calendar = selectableModel as SubscribeCalendarViewModel;
      if (calendar == null)
        return;
      this.SetProjectPopup.IsOpen = false;
      BindCalendarAccountModel calendarAccountModel = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (item => item.Id == calendar.AccountId));
      if ((calendarAccountModel != null ? (calendarAccountModel.IsCalDav() ? 1 : 0) : 0) == 0)
      {
        model.CalendarId = calendar.Id;
        model.CalendarName = calendar.Title;
        await CalendarService.MoveCalendar(model.Id, calendar.Id);
        this.NotifyEventChanged();
      }
      else
      {
        await CalendarService.MoveCalendar(model.Id, calendar.Id);
        SyncManager.TryDelaySync();
      }
    }

    private async void NotifyEventChanged()
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null)
        return;
      CalendarEventChangeNotifier.NotifyEventChanged(model.Data);
      SyncManager.TryDelaySync();
    }

    private void OnExpandClick(object sender, MouseButtonEventArgs e)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null)
        return;
      model.AttendeeExpand = !model.AttendeeExpand;
      model.DisplayAttendees = !model.AttendeeExpand ? model.Attendees : model.Attendees.Take<CalendarAttendeeModel>(5).ToList<CalendarAttendeeModel>();
    }

    private void OnPopupOpened(object sender, EventArgs e)
    {
      EventHandler inOperate = this.InOperate;
      if (inOperate == null)
        return;
      inOperate((object) this, (EventArgs) null);
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      EventHandler stopOperate = this.StopOperate;
      if (stopOperate == null)
        return;
      stopOperate((object) this, (EventArgs) null);
    }

    private void OnConferenceClick(object sender, MouseButtonEventArgs e)
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null)
        return;
      Utils.TryProcessStartUrl(model.ConferenceUri);
    }

    public void ClearEvent() => this.HideDetail = (EventHandler) null;

    public void ShowBackMenu() => this.BackGrid.Visibility = Visibility.Visible;

    public void HideBackMenu() => this.BackGrid.Visibility = Visibility.Collapsed;

    private async void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      CalendarDetailControl element = this;
      CalendarDetailViewModel model = element.GetModel();
      if (model == null)
        ;
      else
      {
        EventArchiveArgs args = new EventArchiveArgs(model.Data);
        args.Id = ArchivedDao.GetOriginalId(args.Id);
        string archiveKey = ArchivedEventDao.GenerateEventKey(args);
        bool archived = await ArchivedDao.ExistArchivedModel(archiveKey);
        List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
        if (model.Editable)
          types.Add(new CustomMenuItemViewModel((object) "delete", Utils.GetString("Delete"), Utils.GetImageSource("DeleteDrawingLine", (FrameworkElement) element)));
        types.Add(new CustomMenuItemViewModel((object) "archive", Utils.GetString(archived ? "CancelArchive" : "Archive"), Utils.GetImageSource(archived ? "CancelArchiveDrawingImage" : "ArchiveDrawingImage", (FrameworkElement) element)));
        CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) element.MorePopup);
        customMenuList.Operated += (EventHandler<object>) (async (o, key) =>
        {
          if (key.ToString() == "delete")
            this.OnCalendarDelete();
          if (key.ToString() == "archive")
          {
            if (archived)
              await ArchivedEventDao.RemoveArchivedEvent(archiveKey);
            else
              await ArchivedEventDao.AddArchivedModel(args);
            await SearchHelper.OnEventArchiveChanged(model.Data.Id);
            EventArchiveSyncService.PushLocalArchivedModels();
            EventHandler<bool> eventArchiveChanged = this.EventArchiveChanged;
            if (eventArchiveChanged != null)
              eventArchiveChanged((object) this, !archived);
          }
          if (ABTestManager.IsNewRemindCalculate())
            EventReminderCalculator.RecalEventReminders(await CalendarEventDao.GetEventByEventIdOrId(model.Data.Id));
          else
            ReminderCalculator.AssembleReminders();
        });
        customMenuList.Show();
      }
    }

    public async Task<bool> IsArchived()
    {
      CalendarDetailViewModel model = this.GetModel();
      if (model == null)
        return false;
      EventArchiveArgs args = new EventArchiveArgs(model.Data);
      args.Id = ArchivedDao.GetOriginalId(args.Id);
      return await ArchivedDao.ExistArchivedModel(ArchivedEventDao.GenerateEventKey(args));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calendardetailcontrol.xaml", UriKind.Relative));
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
          this.Root = (CalendarDetailControl) target;
          this.Root.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          break;
        case 2:
          this.ScrollViewer = (ScrollViewer) target;
          break;
        case 3:
          this.BackGrid = (Grid) target;
          this.BackGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBackClick);
          break;
        case 4:
          this.TitleText = (DetailTextBox) target;
          break;
        case 5:
          this.TitleTextHint = (TextBlock) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnConferenceClick);
          break;
        case 7:
          this.DateGrid = (Grid) target;
          this.DateGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDateClick);
          break;
        case 8:
          this.ReminderText = (StackPanel) target;
          break;
        case 9:
          this.ReminderRun = (Run) target;
          break;
        case 10:
          this.SnoozeRun = (Run) target;
          break;
        case 12:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnExpandClick);
          break;
        case 13:
          this.LocationText = (DetailTextBox) target;
          break;
        case 14:
          this.LocationTextHint = (TextBlock) target;
          break;
        case 15:
          this.DescriptionGrid = (Grid) target;
          break;
        case 16:
          this.NewContentText = (DetailTextBox) target;
          break;
        case 17:
          this.NewContentTextHint = (TextBlock) target;
          break;
        case 18:
          this.CalendarMovePanel = (StackPanel) target;
          this.CalendarMovePanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSelectCalendarClick);
          break;
        case 19:
          this.SetProjectPopup = (EscPopup) target;
          break;
        case 20:
          this.MoreGrid = (Grid) target;
          this.MoreGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
          break;
        case 21:
          this.MorePopup = (EscPopup) target;
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
      if (connectionId != 11)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAttendeeClick);
    }
  }
}
