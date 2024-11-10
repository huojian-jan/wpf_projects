// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.ArchiveOperationDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Dal;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class ArchiveOperationDialog : OperationDialog
  {
    private readonly string _key;
    private ArchiveKind _kind;

    public ArchiveOperationDialog(string key, ArchiveKind kind, bool cancel = false)
      : base(key, cancel ? new OperationItemViewModel(ActionType.CancelArchive) : new OperationItemViewModel(ActionType.Archive))
    {
      this._key = key;
      this._kind = kind;
    }

    protected override async void OnActionClick(OperationItemViewModel model)
    {
      if (model.Type == ActionType.CancelArchive)
        await ArchivedDao.RemoveArchivedModel(this._key);
      else
        await ArchivedDao.AddArchivedModel(this._key, this._kind);
      if (ABTestManager.IsNewRemindCalculate())
        CourseReminderCalculator.InitCourseReminders();
      else
        ReminderCalculator.AssembleReminders();
      DataChangedNotifier.OnScheduleChanged();
      if (this._kind == ArchiveKind.Course)
        CourseArchiveSyncService.PushLocalArchivedModels();
      if (this._kind == ArchiveKind.Event)
        EventArchiveSyncService.PushLocalArchivedModels();
      base.OnActionClick(model);
    }
  }
}
