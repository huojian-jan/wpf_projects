// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.PomoTaskDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class PomoTaskDao
  {
    public static async Task<PomoTask[]> GetByPomoId(string pomoId)
    {
      List<PomoTask> listAsync = await App.Connection.Table<PomoTask>().Where((Expression<Func<PomoTask, bool>>) (p => p.PomoId == pomoId)).ToListAsync();
      listAsync?.ForEach((Action<PomoTask>) (t => t.Tags = TagSerializer.ToTags(t.TagString).ToArray()));
      return listAsync?.ToArray();
    }

    public static async Task<int> DeleteAllByPomoId(string pomoId)
    {
      return await App.Connection.ExecuteAsync("DELETE FROM PomoTask WHERE PomoId=?", (object) pomoId);
    }

    public static async Task UpdateAsync(PomoTask pomo)
    {
      int num = await App.Connection.UpdateAsync((object) pomo);
    }

    public static async Task DeleteAsync(PomoTask pomo)
    {
      int num = await App.Connection.DeleteAsync((object) pomo);
    }

    public static async Task InsertAsync(PomoTask pomo)
    {
      int num = await App.Connection.InsertAsync((object) pomo);
    }

    public static async Task InsertAllAsync(IEnumerable<PomoTask> pomos)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) pomos);
    }

    public static async Task<List<PomoTask>> GetTodayPomoTasks()
    {
      return await App.Connection.QueryAsync<PomoTask>("Select * FROM PomoTask WHERE EndTime > '" + DateTime.Today.ToString("yyyy'-'MM'-'dd' 00:00:00'") + "'");
    }

    public static async Task<List<PomoTask>> GetRecentPomoTasks(int days)
    {
      DateTime dateTime = DateTime.Today;
      dateTime = dateTime.AddDays((double) (-1 * days));
      return await App.Connection.QueryAsync<PomoTask>("Select * FROM PomoTask WHERE EndTime > '" + dateTime.ToString("yyyy'-'MM'-'dd' 00:00:00'") + "'");
    }

    public static async Task<List<PomoTask>> GetPomoTaskOfTimer(
      string timerId,
      string objId,
      bool isHabit,
      DateTime? start = null,
      DateTime? end = null,
      bool isNew = false,
      bool delete = false)
    {
      string str1 = "";
      if (start.HasValue && end.HasValue)
      {
        DateTime dateTime = start.Value;
        string str2 = dateTime.ToString("yyyy'-'MM'-'dd' 00:00:00'");
        dateTime = end.Value;
        string str3 = dateTime.ToString("yyyy'-'MM'-'dd' 23:59:59'");
        str1 = " and b.StartTime >= '" + str2 + "' and b.StartTime < '" + str3 + "'";
      }
      string str4 = delete ? " and a.SyncStatus = -1 and a.Etag is not null " : (isNew ? " and a.SyncStatus = 0 " : " and ( a.SyncStatus = 1 or a.SyncStatus = 2 )");
      string str5;
      if (string.IsNullOrEmpty(objId))
        str5 = " b.TimerSid = '" + timerId + "' ";
      else if (isHabit)
        str5 = "( b.TimerSid = '" + timerId + "' or b.HabitId = '" + objId + "' ) ";
      else
        str5 = " ( b.TimerSid = '" + timerId + "' or b.TaskId = '" + objId + "' ) ";
      return await App.Connection.QueryAsync<PomoTask>("Select b.StartTime,b.EndTime FROM PomodoroModel a,PomoTask b WHERE a.UserId = '" + LocalSettings.Settings.LoginUserId + "' and  a.Id = b.PomoId and " + str5 + " " + str4 + " " + str1 + " ");
    }
  }
}
