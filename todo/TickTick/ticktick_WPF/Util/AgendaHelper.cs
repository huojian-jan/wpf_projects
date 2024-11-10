// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.AgendaHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class AgendaHelper
  {
    private static readonly Dictionary<string, TaskAttendModel> AttendDict = new Dictionary<string, TaskAttendModel>();

    public static bool CanAccessAgenda(AgendaHelper.IAgenda agenda)
    {
      return agenda.GetAttendId() == agenda.GetTaskId();
    }

    public static async Task<TaskAttendModel> GetAgendaAttendModel(string attendId, string taskId)
    {
      if (string.IsNullOrEmpty(attendId))
        return (TaskAttendModel) null;
      if (AgendaHelper.AttendDict.ContainsKey(attendId))
        return AgendaHelper.AttendDict[attendId];
      TaskAttendModel taskAttendModelById = await AttendModelDao.GetTaskAttendModelById(attendId);
      if (taskAttendModelById == null)
        return await AgendaHelper.GetRemoteModel(attendId, taskId);
      if (AgendaHelper.AttendDict.ContainsKey(attendId))
        AgendaHelper.AttendDict[attendId] = taskAttendModelById;
      else
        AgendaHelper.AttendDict.Add(attendId, taskAttendModelById);
      return taskAttendModelById;
    }

    public static async Task<TaskAttendModel> GetRemoteModel(string attendId, string taskId)
    {
      TaskAttendModel model = await Communicator.LoadTaskAttendInfo(attendId, taskId);
      if (model == null)
        return (TaskAttendModel) null;
      if (AgendaHelper.AttendDict.ContainsKey(attendId))
        AgendaHelper.AttendDict[attendId] = model;
      else
        AgendaHelper.AttendDict.Add(attendId, model);
      int num = await AttendModelDao.SaveTaskAttendModel(attendId, model) ? 1 : 0;
      return model;
    }

    public interface IAgenda
    {
      string GetTaskId();

      string GetAttendId();
    }
  }
}
