// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ArchivedDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class ArchivedDao : BaseDao<ArchivedEventModel>
  {
    public static async Task AddArchivedModel(string key, ArchiveKind kind, DateTime? start = null)
    {
      int num = await App.Connection.InsertAsync((object) new ArchivedEventModel()
      {
        UserId = LocalSettings.Settings.LoginUserId,
        Key = key,
        StartTime = start,
        Kind = (int) kind
      });
    }

    public static async Task<bool> ExistArchivedModel(string key)
    {
      return await App.Connection.Table<ArchivedEventModel>().Where((Expression<Func<ArchivedEventModel, bool>>) (cal => cal.Key == key && cal.UserId == LocalSettings.Settings.LoginUserId)).CountAsync() > 0;
    }

    public static async Task RemoveArchivedModel(string key)
    {
      List<ArchivedEventModel> listAsync = await App.Connection.Table<ArchivedEventModel>().Where((Expression<Func<ArchivedEventModel, bool>>) (cal => cal.Key == key)).ToListAsync();
      if (!listAsync.Any<ArchivedEventModel>())
        return;
      foreach (ArchivedEventModel archivedEventModel in listAsync)
        archivedEventModel.SyncStatus = -1;
      int num = await App.Connection.UpdateAllAsync((IEnumerable) listAsync);
    }

    public static string GenerateKey(string id, DateTime? startDate)
    {
      id = ArchivedDao.GetOriginalId(id);
      return id + "_time_" + (startDate.HasValue ? startDate.GetValueOrDefault().Ticks : 0L).ToString();
    }

    public static async Task<List<string>> GetArchivedKeys(ArchiveKind kind = ArchiveKind.Event)
    {
      return (await App.Connection.QueryAsync<EventKeyModel>(string.Format("select key from ArchivedEventModel where kind = {0} and SyncStatus != -1 and userid = '{1}'", (object) (int) kind, (object) LocalSettings.Settings.LoginUserId))).Select<EventKeyModel, string>((Func<EventKeyModel, string>) (model => model.Key)).ToList<string>();
    }

    public static async Task<List<ArchivedEventModel>> GetArchivedModels(
      ArchiveKind kind,
      bool needSync = false)
    {
      string sql = string.Format("select * from ArchivedEventModel where Kind = {0} and Userid = '{1}'", (object) (int) kind, (object) LocalSettings.Settings.LoginUserId);
      if (needSync)
        sql += string.Format(" and SyncStatus != {0}", (object) 2);
      return await App.Connection.QueryAsync<ArchivedEventModel>(sql);
    }

    public static string GetOriginalId(string id)
    {
      if (string.IsNullOrEmpty(id) || !id.Contains("_time_"))
        return id;
      int length = id.IndexOf("_time_", StringComparison.Ordinal);
      return id.Substring(0, length);
    }
  }
}
