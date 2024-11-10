// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ProjectWidgetDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class ProjectWidgetDao
  {
    public static async Task CreateInboxWidget()
    {
      SortOption projectSortOption = SmartProjectService.GetSmartProjectSortOption("inbox", false);
      int num = await App.Connection.InsertAsync((object) new WidgetSettingModel()
      {
        id = Utils.GetGuid(),
        type = "normal",
        userId = LocalSettings.Settings.LoginUserId,
        identity = Utils.GetInboxId(),
        sortType = projectSortOption.orderBy,
        groupType = projectSortOption.groupBy,
        themeId = "dark",
        displayOption = "embed",
        autoHide = true,
        hideComplete = false,
        opacity = 0.7f,
        width = 424.0,
        height = 546.0,
        left = 100.0,
        top = 100.0
      });
    }

    public static async Task<WidgetSettingModel> GetWidgetById(string widgetId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return (await App.Connection.Table<WidgetSettingModel>().ToListAsync()).Where<WidgetSettingModel>((Func<WidgetSettingModel, bool>) (widget => widget.id == widgetId && widget.userId == userId)).FirstOrDefault<WidgetSettingModel>();
    }

    public static async Task<List<WidgetSettingModel>> GetWidgets()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<WidgetSettingModel>().Where((Expression<Func<WidgetSettingModel, bool>>) (widget => widget.userId == userId)).ToListAsync();
    }

    public static async Task DeleteWidgetById(string id)
    {
      List<WidgetSettingModel> listAsync = await App.Connection.Table<WidgetSettingModel>().Where((Expression<Func<WidgetSettingModel, bool>>) (v => v.id == id)).ToListAsync();
      if (listAsync == null || listAsync.Count <= 0)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task UpdateWidget(WidgetSettingModel settingModel)
    {
      int num = await App.Connection.UpdateAsync((object) settingModel);
    }
  }
}
