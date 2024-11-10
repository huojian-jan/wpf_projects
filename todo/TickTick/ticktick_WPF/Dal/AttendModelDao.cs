// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.AttendModelDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class AttendModelDao : BaseDao<AttendModel>
  {
    public static async Task<TaskAttendModel> GetTaskAttendModelById(string attendId)
    {
      if (!string.IsNullOrEmpty(attendId))
      {
        AttendModel attendModelById = await AttendModelDao.GetAttendModelById(attendId);
        if (attendModelById != null && !string.IsNullOrEmpty(attendModelById.json))
          return JsonConvert.DeserializeObject<TaskAttendModel>(attendModelById.json);
      }
      return (TaskAttendModel) null;
    }

    private static async Task<AttendModel> GetAttendModelById(string attendId)
    {
      return await App.Connection.Table<AttendModel>().Where((Expression<Func<AttendModel, bool>>) (model => model.attendId == attendId)).FirstOrDefaultAsync();
    }

    public static async Task<bool> SaveTaskAttendModel(string attendId, TaskAttendModel model)
    {
      if (model != null)
      {
        AttendModel attendModelById = await AttendModelDao.GetAttendModelById(attendId);
        if (attendModelById == null)
        {
          int num = await App.Connection.InsertAsync((object) new AttendModel()
          {
            attendId = attendId,
            json = JsonConvert.SerializeObject((object) model)
          });
          return true;
        }
        string str = JsonConvert.SerializeObject((object) model);
        if (attendModelById.json != str)
        {
          attendModelById.json = str;
          int num = await App.Connection.UpdateAsync((object) attendModelById);
          return true;
        }
      }
      return false;
    }
  }
}
