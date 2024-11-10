// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SqliteUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util
{
  public class SqliteUtils
  {
    public static async Task<List<T>> BatchGetModelsAsync<T>(
      string sql,
      List<string> ids,
      int step = 50)
      where T : new()
    {
      if (ids == null || ids.Count == 0)
        return new List<T>();
      int start = 0;
      BlockingList<T> result = new BlockingList<T>();
      List<Task> taskList = new List<Task>();
      do
      {
        taskList.Add(GetModels(start, Math.Min(ids.Count, start + step)));
        start += step;
      }
      while (ids.Count > start);
      await Task.WhenAll((IEnumerable<Task>) taskList);
      return result.Value;

      async Task GetModels(int start, int end)
      {
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = start; index < end; ++index)
          stringBuilder.Append("'").Append(ids[index]).Append("'").Append(",");
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        List<T> models = await App.Connection.QueryAsync<T>(string.Format(sql, (object) stringBuilder));
        if (models == null)
          return;
        result.AddRange((IEnumerable<T>) models);
      }
    }
  }
}
