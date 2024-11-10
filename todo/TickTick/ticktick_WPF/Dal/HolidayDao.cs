// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.HolidayDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class HolidayDao : BaseDao<HolidayModel>
  {
    public static async Task SaveHolidays(List<HolidayModel> holidays)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) holidays);
    }

    public static async Task<List<HolidayModel>> GetRecentHolidays()
    {
      return await App.Connection.Table<HolidayModel>().ToListAsync();
    }

    public static async Task ClearHolidays()
    {
      int num = await App.Connection.ExecuteAsync("DELETE FROM HolidayModel WHERE 1=1");
    }
  }
}
