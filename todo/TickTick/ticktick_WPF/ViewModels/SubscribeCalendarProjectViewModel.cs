// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.SubscribeCalendarProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class SubscribeCalendarProjectViewModel : ProjectItemViewModel
  {
    public readonly CalendarSubscribeProfileModel Profile;

    public SubscribeCalendarProjectViewModel(CalendarSubscribeProfileModel profile)
    {
      this.Profile = profile;
      this.Id = profile.Id;
      this.Icon = Utils.GetIcon("IcSubscribeCalendar");
      this.Title = profile.Name;
      this.CanDrag = false;
      this.CanDrop = false;
      this.IsPtfItem = true;
      this.ListType = ProjectListType.Calendar;
      if (!profile.Expired)
        return;
      this.InfoIcon = Utils.GetIcon("IcExpired");
      this.Count = 0;
      this.InfoIconBrush = (Brush) ThemeUtil.GetColorInString("#FFB000");
    }

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      SubscribeCalendarProjectViewModel projectViewModel = this;
      string saveIdentity = projectViewModel.GetSaveIdentity();
      bool windowShowing = !string.IsNullOrEmpty(saveIdentity) && TaskListWindow.Windows.ContainsKey(saveIdentity);
      List<ContextAction> contextActions = new List<ContextAction>();
      contextActions.Add(new ContextAction(ContextActionKey.Edit));
      List<ContextAction> contextActionList = contextActions;
      contextActionList.Add(new ContextAction(await ProjectPinSortOrderService.CheckIsProjectPinned(projectViewModel.Profile.Id, 9) ? ContextActionKey.Unpin : ContextActionKey.Pin));
      contextActions.Add(new ContextAction(windowShowing ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow));
      contextActions.Add(new ContextAction(ContextActionKey.Unsubscribe));
      return (IEnumerable<ContextAction>) contextActions;
    }

    public override string GetSaveIdentity() => "subscribe_calendar:" + this.Profile.Id;

    public override async void LoadCount()
    {
      SubscribeCalendarProjectViewModel projectViewModel = this;
      if (LocalSettings.Settings.ExtraSettings.NumDisplayType == 2)
      {
        projectViewModel.Count = 0;
      }
      else
      {
        // ISSUE: reference to a compiler-generated method
        int num = await Task.Run<int>(new Func<Task<int>>(projectViewModel.\u003CLoadCount\u003Eb__4_0));
        projectViewModel.Count = num;
      }
    }

    public override ProjectIdentity GetIdentity()
    {
      return (ProjectIdentity) new SubscribeCalendarProjectIdentity(this.Profile);
    }

    public override PtfType GetPtfType() => PtfType.Subscribe;
  }
}
