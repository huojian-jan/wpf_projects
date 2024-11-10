// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ArchivedEventDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class ArchivedEventDao : ArchivedDao
  {
    public static async Task RemoveArchivedEvent(string key)
    {
      List<ArchivedEventModel> listAsync = await App.Connection.Table<ArchivedEventModel>().Where((Expression<Func<ArchivedEventModel, bool>>) (cal => cal.Key == key)).ToListAsync();
      if (!listAsync.Any<ArchivedEventModel>())
        return;
      foreach (ArchivedEventModel archivedEventModel in listAsync)
        archivedEventModel.SyncStatus = -1;
      int num = await App.Connection.UpdateAllAsync((IEnumerable) listAsync);
      DataChangedNotifier.NotifyEventArchivedChanged();
    }

    public static string GetRealId(string id)
    {
      if (!string.IsNullOrEmpty(id) && id.Contains("@"))
      {
        int length = id.IndexOf("@", StringComparison.Ordinal);
        id = id.Substring(0, length);
      }
      return id;
    }

    public static async Task AddArchivedModel(EventArchiveArgs args)
    {
      string eventKey = ArchivedEventDao.GenerateEventKey(args);
      int num = await App.Connection.InsertAsync((object) new ArchivedEventModel()
      {
        UserId = LocalSettings.Settings.LoginUserId,
        Key = eventKey,
        StartTime = args.StartTime,
        Kind = 0,
        Title = args.Title
      });
      DataChangedNotifier.NotifyEventArchivedChanged();
    }

    public static string GenerateOriginEventKey(
      CalendarEventModel model,
      DateTime? startDate,
      DateTime? dueDate,
      bool isAllDay)
    {
      if (isAllDay)
      {
        startDate = startDate?.Date;
        dueDate = dueDate?.Date;
      }
      long num1 = 1;
      long num2;
      if (!string.IsNullOrEmpty(model.Id))
      {
        long num3 = 31L * (31L * (31L * (long) ArchivedDao.GetOriginalId(model.Id).GetHashCode() + (!startDate.HasValue ? 0L : (long) startDate.GetHashCode())) + (!dueDate.HasValue ? 0L : (long) dueDate.GetHashCode()));
        string repeatFlag = model.RepeatFlag;
        long hashCode = repeatFlag != null ? (long) repeatFlag.GetHashCode() : 0L;
        num2 = num3 + hashCode;
      }
      else
      {
        if (!string.IsNullOrEmpty(model.Title))
          num1 = 31L * num1 + (long) model.Title.GetHashCode();
        long num4 = 31L * (31L * (31L * num1 + (!startDate.HasValue ? 0L : (long) startDate.GetHashCode())) + (!dueDate.HasValue ? 0L : (long) dueDate.GetHashCode()));
        string repeatFlag = model.RepeatFlag;
        long hashCode = repeatFlag != null ? (long) repeatFlag.GetHashCode() : 0L;
        num2 = num4 + hashCode;
      }
      return num2.ToString() + string.Empty;
    }

    public static string GenerateEventKey(EventArchiveArgs args)
    {
      DateTime? endTime = args.EndTime;
      DateTime? nullable1 = args.StartTime;
      if ((endTime.HasValue == nullable1.HasValue ? (endTime.HasValue ? (endTime.GetValueOrDefault() == nullable1.GetValueOrDefault() ? 1 : 0) : 1) : 0) == 0)
      {
        nullable1 = args.EndTime;
        if (nullable1.HasValue)
          goto label_4;
      }
      nullable1 = args.StartTime;
      if (nullable1.HasValue && args.IsAllDay)
      {
        EventArchiveArgs eventArchiveArgs = args;
        nullable1 = args.StartTime;
        DateTime? nullable2 = new DateTime?(nullable1.Value.AddDays(1.0));
        eventArchiveArgs.EndTime = nullable2;
      }
label_4:
      if (args.IsAllDay)
      {
        EventArchiveArgs eventArchiveArgs1 = args;
        nullable1 = args.StartTime;
        ref DateTime? local1 = ref nullable1;
        DateTime valueOrDefault;
        DateTime? nullable3;
        if (!local1.HasValue)
        {
          nullable3 = new DateTime?();
        }
        else
        {
          valueOrDefault = local1.GetValueOrDefault();
          nullable3 = new DateTime?(valueOrDefault.Date);
        }
        eventArchiveArgs1.StartTime = nullable3;
        EventArchiveArgs eventArchiveArgs2 = args;
        nullable1 = args.EndTime;
        ref DateTime? local2 = ref nullable1;
        DateTime? nullable4;
        if (!local2.HasValue)
        {
          nullable4 = new DateTime?();
        }
        else
        {
          valueOrDefault = local2.GetValueOrDefault();
          nullable4 = new DateTime?(valueOrDefault.Date);
        }
        eventArchiveArgs2.EndTime = nullable4;
      }
      args.Id = ArchivedEventDao.GetRealId(args.Id);
      long hashCode1 = (long) ArchivedEventDao.GetHashCode(31, 0, ArchivedEventDao.GetBytesFromString(!string.IsNullOrEmpty(args.Id) ? ArchivedDao.GetOriginalId(args.Id) : args.Title));
      nullable1 = args.StartTime;
      long hashCode2 = (long) ArchivedEventDao.GetHashCode(31, 0, !nullable1.HasValue ? (byte[]) null : ArchivedEventDao.GetBytesFromString(Utils.GetTimeStamp(args.StartTime).ToString() + string.Empty));
      long num1 = hashCode1 + hashCode2;
      nullable1 = args.EndTime;
      long hashCode3 = (long) ArchivedEventDao.GetHashCode(31, 0, !nullable1.HasValue ? (byte[]) null : ArchivedEventDao.GetBytesFromString(Utils.GetTimeStamp(args.EndTime).ToString() + string.Empty));
      long num2 = num1 + hashCode3;
      string repeatFlag = args.RepeatFlag;
      if ((repeatFlag != null ? (repeatFlag.Length > 0 ? 1 : 0) : 0) != 0 && !args.RepeatFlag.StartsWith("RRULE:"))
        args.RepeatFlag = "RRULE:" + args.RepeatFlag;
      return (num2 + (long) ArchivedEventDao.GetHashCode(31, 0, ArchivedEventDao.GetBytesFromString(args.RepeatFlag))).ToString() + string.Empty;
    }

    public static int GetHashCode(int prime, int start, byte[] bytes)
    {
      if (bytes == null)
        return start;
      int hashCode = start;
      foreach (byte num in bytes)
        hashCode = hashCode * prime + (int) num;
      return hashCode;
    }

    public static byte[] GetBytesFromString(string str)
    {
      return string.IsNullOrEmpty(str) ? (byte[]) null : new ASCIIEncoding().GetBytes(str.ToCharArray());
    }
  }
}
