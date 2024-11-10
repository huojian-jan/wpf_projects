// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.SyncWebSocket
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Pomo;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class SyncWebSocket : TickWebSocket
  {
    private PushModel _pushModel;

    public SyncWebSocket(string pushUrl)
      : base(pushUrl)
    {
    }

    protected override bool IsConnectId(string msg)
    {
      if (msg.StartsWith("{"))
        return false;
      return msg.Split('-').Length >= 4;
    }

    protected override async Task Register(string msg)
    {
      this._pushModel = await Communicator.RegisterPushService(this._pushModel?.id ?? string.Empty, msg);
      UtilLog.Info("SyncWebSocket.OnMessageReceived() RegisterPushService channelId=" + msg + ", " + this._pushModel?.ToString());
    }

    protected override async Task HandleMessage(string message)
    {
      WSReminderModel reminderModel = (WSReminderModel) null;
      JObject jobject = (JObject) null;
      try
      {
        reminderModel = JsonConvert.DeserializeObject<WSReminderModel>(message);
      }
      catch (Exception ex)
      {
      }
      try
      {
        jobject = JObject.Parse(message);
      }
      catch (Exception ex)
      {
      }
      JToken jtoken1;
      if (jobject != null && jobject.TryGetValue("type", out jtoken1))
      {
        UtilLog.Info("SyncWebSocket.HandleMessage()" + jtoken1?.ToString());
        string str = jtoken1.ToString();
        if (str != null)
        {
          switch (str.Length)
          {
            case 2:
              if (str == "sn")
                break;
              goto label_33;
            case 4:
              switch (str[2])
              {
                case 'a':
                  if (str == "team")
                    break;
                  goto label_33;
                case 's':
                  if (str == "test")
                  {
                    JToken jtoken2;
                    Communicator.NotifyPushArrive(new PushArriveModel()
                    {
                      id = jobject.TryGetValue("data", out jtoken2) ? jtoken2.ToString() : string.Empty,
                      model = Utils.GetWindowsVersion(),
                      osVersion = Utils.VersionNumToDouble().ToString() + string.Empty,
                      time = DateTime.Now
                    });
                    goto label_33;
                  }
                  else
                    goto label_33;
                default:
                  goto label_33;
              }
              break;
            case 5:
              switch (str[0])
              {
                case 'h':
                  if (str == "habit")
                  {
                    HabitSyncService.DelaySync();
                    goto label_33;
                  }
                  else
                    goto label_33;
                case 's':
                  if (str == "share")
                    break;
                  goto label_33;
                default:
                  goto label_33;
              }
              break;
            case 8:
              switch (str[0])
              {
                case 'c':
                  if (str == "calendar")
                  {
                    await CalendarService.PullSubscribeCalendars();
                    await CalendarService.PullAccountCalendarsAndEvents();
                    ListViewContainer.ReloadProjectData();
                    goto label_33;
                  }
                  else
                    goto label_33;
                case 'n':
                  if (str == "needSync")
                  {
                    SyncWebSocket.HandleNeedSync();
                    goto label_33;
                  }
                  else
                    goto label_33;
                default:
                  goto label_33;
              }
            case 9:
              if (str == "timetable")
              {
                ScheduleService.GetRemoteSchedules();
                goto label_33;
              }
              else
                goto label_33;
            case 10:
              if (str == "preference")
              {
                SettingsHelper.PullRemotePreference();
                goto label_33;
              }
              else
                goto label_33;
            case 12:
              if (str == "notification")
                break;
              goto label_33;
            case 13:
              if (str == "paymentUpdate")
                break;
              goto label_33;
            default:
              goto label_33;
          }
          SyncWebSocket.CheckNotification();
        }
      }
label_33:
      if (reminderModel?.data == null)
      {
        reminderModel = (WSReminderModel) null;
      }
      else
      {
        try
        {
          UtilLog.Info("SyncWebSocket.HandleMessage()" + reminderModel.data.type);
          ticktick_WPF.Notifier.GlobalEventManager.NotifyRemindHandled(reminderModel.data);
          switch (reminderModel.data.type)
          {
            case "calendar":
              CalendarEventModel eventById = await CalendarEventDao.GetEventById(reminderModel.data.id);
              if (eventById == null)
              {
                reminderModel = (WSReminderModel) null;
                break;
              }
              SystemToastUtils.RemoveToast(eventById.Id, eventById.CalendarId);
              reminderModel = (WSReminderModel) null;
              break;
            case "task":
              TaskModel thinTaskById1 = await TaskDao.GetThinTaskById(reminderModel.data.id);
              if (thinTaskById1 == null)
              {
                reminderModel = (WSReminderModel) null;
                break;
              }
              SystemToastUtils.RemoveToast(thinTaskById1.id, thinTaskById1.projectId);
              reminderModel = (WSReminderModel) null;
              break;
            case "checklist":
              TaskDetailItemModel checkitem = await TaskDetailItemDao.GetCheckItemById(reminderModel.data.id);
              if (checkitem != null)
              {
                TaskModel thinTaskById2 = await TaskDao.GetThinTaskById(checkitem.TaskServerId);
                if (thinTaskById2 != null)
                  SystemToastUtils.RemoveToast(checkitem.id, thinTaskById2.projectId);
              }
              checkitem = (TaskDetailItemModel) null;
              reminderModel = (WSReminderModel) null;
              break;
            case "habit":
              SystemToastUtils.RemoveHabitToast(reminderModel.data.id);
              reminderModel = (WSReminderModel) null;
              break;
            case "pomo":
              SystemToastUtils.RemoveToast("focus", "focus");
              TickFocusManager.HidePomoReminder();
              reminderModel = (WSReminderModel) null;
              break;
            default:
              reminderModel = (WSReminderModel) null;
              break;
          }
        }
        catch (Exception ex)
        {
          UtilLog.Error(ex);
          reminderModel = (WSReminderModel) null;
        }
      }
    }

    private static async void CheckNotification()
    {
      Utils.RunOnUiThread(Application.Current?.Dispatcher, new Action(SetNotificationIndication));

      static async void SetNotificationIndication()
      {
        NotificationUnreadCount notificationCount = await Communicator.GetNotificationCount();
        if (notificationCount == null || notificationCount.Activity <= 0 && notificationCount.Notification <= 0)
          return;
        App.Window.LeftTabBar.ShowNotificationIndicator(notificationCount);
      }
    }

    private static void HandleNeedSync()
    {
      Utils.RunOnUiThread(App.Window.Dispatcher, (Action) (() =>
      {
        SyncManager.TryDelaySync(1000);
        ReminderCalculator.AssembleReminders();
      }));
    }

    public override void SendHello()
    {
      this.SendTextAsync("hello").ContinueWith(new Action<Task>(UtilRun.LogTask));
    }
  }
}
