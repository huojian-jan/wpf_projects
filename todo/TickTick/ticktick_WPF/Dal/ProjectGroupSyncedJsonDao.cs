// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ProjectGroupSyncedJsonDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class ProjectGroupSyncedJsonDao
  {
    public static async Task TrySaveProjectGroup(string groupId)
    {
      if (await ProjectGroupSyncedJsonDao.GetSavedJson(groupId) != null)
        return;
      ProjectGroupModel projectGroupById = await ProjectGroupDao.GetProjectGroupById(groupId);
      if (projectGroupById == null)
        return;
      int num = await App.Connection.InsertAsync((object) new ProjectGroupSyncedJsonModel()
      {
        GroupId = groupId,
        UserId = Utils.GetCurrentUserIdInt().ToString(),
        JsonString = JsonConvert.SerializeObject((object) projectGroupById)
      });
    }

    public static async Task<ProjectGroupSyncedJsonModel> GetSavedJson(string groupId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<ProjectGroupSyncedJsonModel>().Where((Expression<Func<ProjectGroupSyncedJsonModel, bool>>) (model => model.UserId == userId && model.GroupId == groupId)).FirstOrDefaultAsync();
    }

    public static async Task BatchDeleteGroups(List<string> groupIds)
    {
      if (groupIds == null || !groupIds.Any<string>())
        return;
      foreach (string groupId in groupIds)
      {
        ProjectGroupSyncedJsonModel savedJson = await ProjectGroupSyncedJsonDao.GetSavedJson(groupId);
        if (savedJson != null)
        {
          int num = await App.Connection.DeleteAsync((object) savedJson);
        }
      }
    }
  }
}
