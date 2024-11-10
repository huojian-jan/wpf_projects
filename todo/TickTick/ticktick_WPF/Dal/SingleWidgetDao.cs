// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.SingleWidgetDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class SingleWidgetDao
  {
    public static async Task CreateWidget(bool isCalendar)
    {
      int num = await App.Connection.InsertAsync((object) new CalendarWidgetModel()
      {
        id = Utils.GetGuid(),
        userId = LocalSettings.Settings.LoginUserId,
        themeId = "dark",
        displayOption = "embed",
        autoHide = true,
        hideComplete = false,
        opacity = 0.7f,
        width = 847.0,
        height = 622.0,
        left = 300.0,
        top = 100.0,
        mode = (!isCalendar ? 1 : 0)
      });
    }

    public static async Task DeleteWidget(int mode)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<CalendarWidgetModel> listAsync = await App.Connection.Table<CalendarWidgetModel>().Where((Expression<Func<CalendarWidgetModel, bool>>) (widget => widget.userId == userId && widget.mode == mode)).ToListAsync();
      if (listAsync == null || listAsync.Count <= 0)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task SaveWidget(CalendarWidgetModel settingModel)
    {
      int num = await App.Connection.UpdateAsync((object) settingModel);
    }

    public static async Task<CalendarWidgetModel> TryGetWidget(int mode)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<CalendarWidgetModel>().Where((Expression<Func<CalendarWidgetModel, bool>>) (widget => widget.userId == userId && widget.mode == mode)).FirstOrDefaultAsync();
    }
  }
}
