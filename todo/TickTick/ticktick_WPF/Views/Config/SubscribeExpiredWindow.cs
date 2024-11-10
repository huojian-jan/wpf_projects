// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.SubscribeExpiredWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.MainListView;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class SubscribeExpiredWindow : CustomerDialog
  {
    private readonly bool _multiMode;
    private bool _unsubscribeMode;
    private readonly BindCalendarAccountModel _account;
    private readonly CalendarSubscribeProfileModel _subscribe;

    public SubscribeExpiredWindow(
      List<BindCalendarAccountModel> accounts,
      List<CalendarSubscribeProfileModel> subscribes)
      : base(Utils.GetString("SubscriptionCalendarExpired"), "", Utils.GetString("Reauthorize"), Utils.GetString("Unsubscribe"))
    {
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      if ((accounts != null ? __nonvirtual (accounts.Count) : 0) + (subscribes != null ? __nonvirtual (subscribes.Count) : 0) > 1)
      {
        this._multiMode = true;
        string str = "";
        // ISSUE: explicit non-virtual call
        if (accounts != null && __nonvirtual (accounts.Count) > 0)
        {
          for (int index = 0; index < accounts.Count; ++index)
          {
            BindCalendarAccountModel account = accounts[index];
            str = str + (Utils.IsCn() ? "“" : "'") + (string.IsNullOrEmpty(account.Description) ? account.Account : account.Description) + (Utils.IsCn() ? "”" : "'");
            // ISSUE: explicit non-virtual call
            if (index < accounts.Count - 1 || subscribes != null && __nonvirtual (subscribes.Count) > 0)
              str += "、 ";
          }
        }
        // ISSUE: explicit non-virtual call
        if (subscribes != null && __nonvirtual (subscribes.Count) > 0)
        {
          for (int index = 0; index < subscribes.Count; ++index)
          {
            CalendarSubscribeProfileModel subscribe = subscribes[index];
            str = str + (Utils.IsCn() ? "“" : "'") + subscribe.Name + (Utils.IsCn() ? "”" : "'");
            if (index < subscribes.Count - 1)
              str += "、 ";
          }
        }
        this.ContentTextBlock.Text = string.Format(Utils.GetString("SubscriptionCalendarsExpiredDesc"), (object) str);
        this.CancelButton.Content = (object) Utils.GetString("GotIt");
      }
      else if (accounts != null && accounts.Count == 1)
      {
        this.ContentTextBlock.Text = string.Format(Utils.GetString("SubscriptionCalendarExpiredDesc"), string.IsNullOrEmpty(accounts[0].Description) ? (object) accounts[0].Account : (object) accounts[0].Description);
        this._account = accounts[0];
      }
      else
      {
        if (subscribes == null || subscribes.Count != 1)
          return;
        this.ContentTextBlock.Text = string.Format(Utils.GetString("SubscriptionCalendarExpiredDesc"), (object) subscribes[0].Name);
        this._subscribe = subscribes[0];
      }
    }

    protected override void Cancel()
    {
      if (!this._multiMode)
      {
        if (!this._unsubscribeMode)
        {
          this._unsubscribeMode = true;
          this.TitleTextBlock.Text = Utils.GetString("Unsubscribe");
          this.ContentTextBlock.Text = Utils.GetString("UnsubscribedConfirmContent");
          this.CancelButton.Content = (object) Utils.GetString("GotIt");
          this.OkButton.Content = (object) Utils.GetString("Unsubscribe");
        }
        else
          this.Close();
      }
      else
        this.Close();
    }

    protected override void OnOk()
    {
      this.Close();
      if (!this._multiMode && (this._account != null || this._subscribe != null))
      {
        if (this._account != null)
        {
          if (!this._unsubscribeMode)
            this.ReauthorizeAccount(this._account);
          else
            this.UnsubscribeAccount(this._account);
        }
        else if (!this._unsubscribeMode)
        {
          EditUrlWindow editUrlWindow = new EditUrlWindow(new SubscribeCalendarViewModel(this._subscribe));
          editUrlWindow.Owner = this.Owner;
          editUrlWindow.ShowDialog();
        }
        else
          SubscribeCalendarHelper.UnsubscribeCalendar(this._subscribe.Id);
      }
      else
        SettingDialog.ShowSettingDialog(SettingsType.Calendar);
    }

    private async void UnsubscribeAccount(BindCalendarAccountModel account)
    {
      await SubscribeCalendarHelper.UnbindCalendar(account.Id);
      ListViewContainer.ReloadProjectData();
    }

    private async void UnsubscribeCalendar(CalendarSubscribeProfileModel calendar)
    {
    }

    private void ReauthorizeAccount(BindCalendarAccountModel account)
    {
      if (account.IsBindAccountPassword())
      {
        BindAccountWindow bindAccountWindow = new BindAccountWindow(account);
        bindAccountWindow.Owner = this.Owner;
        bindAccountWindow.ShowDialog();
      }
      else if (account.Site == "outlook")
      {
        BindOutlookWindow bindOutlookWindow = new BindOutlookWindow();
        bindOutlookWindow.Owner = this.Owner;
        bindOutlookWindow.SetOriginAccount(account.Id);
        bindOutlookWindow.ShowDialog();
      }
      else if (account.Site == "google")
      {
        BindGoogleAccount.GetInstance().Start(account.Id);
      }
      else
      {
        if (!(account.Kind == "icloud"))
          return;
        BindICloudWindow bindIcloudWindow = new BindICloudWindow((SubscribeCalendar) null, account);
        bindIcloudWindow.Owner = this.Owner;
        bindIcloudWindow.ShowDialog();
      }
    }

    public static void TryShowWindow(
      List<BindCalendarAccountModel> expired,
      List<CalendarSubscribeProfileModel> subscribes = null)
    {
      SubscribeExpiredWindow subscribeExpiredWindow = new SubscribeExpiredWindow(expired, subscribes);
      try
      {
        subscribeExpiredWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        subscribeExpiredWindow.Owner = Utils.GetToastWindow() as Window;
        subscribeExpiredWindow.Show();
      }
      catch
      {
        subscribeExpiredWindow.Owner = (Window) null;
        subscribeExpiredWindow.Show();
      }
    }
  }
}
