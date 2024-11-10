// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.EventUndo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Dal;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class EventUndo : UndoController
  {
    private readonly string _eventId;
    private readonly string _eventTitle;

    public EventUndo(string eventId, string eventTitle)
    {
      this._eventId = eventId;
      this._eventTitle = eventTitle;
    }

    public override string GetTitle() => this._eventTitle;

    public override string GetContent() => Utils.GetString("Deleted");

    public override async void Undo()
    {
      await CalendarService.UndoDeleteEvent(this._eventId);
      CalendarEventChangeNotifier.NotifyEventRestored(this._eventId);
    }

    public override async void Finished()
    {
      await SyncStatusDao.AddEventDeletedSyncStatus(this._eventId);
      SyncManager.Sync();
    }
  }
}
