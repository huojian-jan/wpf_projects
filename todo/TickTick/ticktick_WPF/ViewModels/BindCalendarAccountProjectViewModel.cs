// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.BindCalendarAccountProjectViewModel
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
  public class BindCalendarAccountProjectViewModel : ProjectItemViewModel
  {
    public readonly BindCalendarAccountModel Account;

    public BindCalendarAccountProjectViewModel(BindCalendarAccountModel account)
    {
      if (account.Expired)
      {
        this.InfoIcon = Utils.GetIcon("IcExpired");
        this.Count = 0;
        this.InfoIconBrush = (Brush) ThemeUtil.GetColorInString("#FFB000");
      }
      this.Account = account;
      this.Id = account.Id;
      this.Title = string.IsNullOrEmpty(account.Description) ? account.Account : account.Description;
      if (account.Site == "feishu")
        this.Title = Utils.GetString("FeishuCalendar");
      this.Icon = SubscribeCalendarHelper.GetCalendarProjectIcon(account);
      this.CanDrag = false;
      this.CanDrop = false;
      this.IsPtfItem = true;
      this.ListType = ProjectListType.Calendar;
    }

    public override string GetSaveIdentity() => "bind_account:" + this.Account.Id;

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      BindCalendarAccountProjectViewModel projectViewModel = this;
      string saveIdentity = projectViewModel.GetSaveIdentity();
      bool windowShowing = !string.IsNullOrEmpty(saveIdentity) && TaskListWindow.Windows.ContainsKey(saveIdentity);
      List<ContextAction> contextActions1;
      List<ContextAction> contextActions2;
      if (projectViewModel.Account.Expired)
      {
        contextActions2 = new List<ContextAction>();
        contextActions2.Add(new ContextAction(ContextActionKey.Reauthorize));
        contextActions1 = contextActions2;
        contextActions1.Add(new ContextAction(await ProjectPinSortOrderService.CheckIsProjectPinned(projectViewModel.Account.Id, 9) ? ContextActionKey.Unpin : ContextActionKey.Pin));
        contextActions2.Add(new ContextAction(windowShowing ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow));
        contextActions2.Add(new ContextAction(ContextActionKey.Unsubscribe));
        return (IEnumerable<ContextAction>) contextActions2;
      }
      contextActions1 = new List<ContextAction>();
      contextActions1.Add(new ContextAction(ContextActionKey.Edit));
      contextActions2 = contextActions1;
      contextActions2.Add(new ContextAction(await ProjectPinSortOrderService.CheckIsProjectPinned(projectViewModel.Account.Id, 9) ? ContextActionKey.Unpin : ContextActionKey.Pin));
      contextActions1.Add(new ContextAction(windowShowing ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow));
      contextActions1.Add(new ContextAction(ContextActionKey.Unsubscribe));
      return (IEnumerable<ContextAction>) contextActions1;
    }

    public override async void LoadCount()
    {
      BindCalendarAccountProjectViewModel projectViewModel = this;
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
      return (ProjectIdentity) new BindAccountCalendarProjectIdentity(this.Account);
    }

    public override PtfType GetPtfType() => PtfType.Subscribe;
  }
}
