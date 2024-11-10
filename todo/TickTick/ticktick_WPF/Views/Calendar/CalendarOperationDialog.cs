// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarOperationDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.Views.Search;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarOperationDialog : OperationDialog
  {
    private readonly DateTime? _dueDate;
    private readonly string _eventId;
    private readonly DateTime? _startDate;
    private bool _isAllDay;
    private EventArchiveArgs _args;

    public CalendarOperationDialog(EventArchiveArgs args, bool cancel = false)
      : base(args.Id, cancel ? new OperationItemViewModel(ActionType.CancelArchive) : new OperationItemViewModel(ActionType.Archive))
    {
      this._eventId = args.Id;
      args.Id = ArchivedDao.GetOriginalId(args.Id);
      this._args = args;
    }

    protected override async void OnActionClick(OperationItemViewModel model)
    {
      if (model.Type == ActionType.CancelArchive)
      {
        await ArchivedEventDao.RemoveArchivedEvent(ArchivedEventDao.GenerateEventKey(this._args));
        UtilLog.Info("CancelArchiveEvent " + this._args.Id + "," + this._args.StartTime.ToString());
      }
      else
      {
        await ArchivedEventDao.AddArchivedModel(this._args);
        UtilLog.Info("ArchiveEvent " + this._args.Id + "," + this._args.StartTime.ToString());
      }
      if (ABTestManager.IsNewRemindCalculate())
        EventReminderCalculator.RecalEventReminders(await CalendarEventDao.GetEventByEventIdOrId(this._eventId));
      else
        ReminderCalculator.AssembleReminders();
      await SearchHelper.OnEventArchiveChanged(this._eventId);
      EventArchiveSyncService.PushLocalArchivedModels();
      base.OnActionClick(model);
    }
  }
}
