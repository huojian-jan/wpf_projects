// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ShowCountDownHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ShowCountDownHelper
  {
    private static async Task<ShowCountDownModel> GetShowCountDownModel(string projectId)
    {
      List<ShowCountDownModel> listAsync = await App.Connection.Table<ShowCountDownModel>().Where((Expression<Func<ShowCountDownModel, bool>>) (e => e.UserId == LocalSettings.Settings.LoginUserId && e.ProjectId == projectId)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
        return listAsync[0];
      ShowCountDownModel model = new ShowCountDownModel()
      {
        UserId = LocalSettings.Settings.LoginUserId,
        ProjectId = projectId
      };
      int num = await App.Connection.InsertAsync((object) model);
      return model;
    }

    public static async Task<bool> GetShowCountDown(string projectId)
    {
      return (await ShowCountDownHelper.GetShowCountDownModel(projectId)).ShowCountDown;
    }

    public static async void SetShowCountDown(string projectId, bool showCountDown)
    {
      ShowCountDownModel showCountDownModel = await ShowCountDownHelper.GetShowCountDownModel(projectId);
      showCountDownModel.ShowCountDown = showCountDown;
      int num = await App.Connection.UpdateAsync((object) showCountDownModel);
      ProjectWidgetsHelper.OnShowCountDownChanged(projectId, showCountDown);
    }
  }
}
