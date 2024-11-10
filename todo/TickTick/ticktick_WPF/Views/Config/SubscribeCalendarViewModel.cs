// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.SubscribeCalendarViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class SubscribeCalendarViewModel : BaseViewModel
  {
    private bool _expand;
    private bool _isExpried;
    public readonly BindCalendarAccountModel Account;

    public bool Expand
    {
      get => this._expand;
      set
      {
        this._expand = value;
        this.OnPropertyChanged(nameof (Expand));
      }
    }

    public string CalendarId { get; set; }

    public string Kind { get; set; }

    public string Site { get; set; }

    public string ImageUrl
    {
      get
      {
        switch (this.Kind)
        {
          case "api":
            switch (this.Site)
            {
              case "outlook":
                return "../../Assets/img_Outlook.png";
              case "google":
                return "../../Assets/img_google.png";
              default:
                return "../../Assets/img_url.png";
            }
          case "url":
            return "../../Assets/img_url.png";
          case "caldav":
            return "../../Assets/img_calDAV.png";
          case "exchange":
            return "../../Assets/img_exchange.png";
          case "icloud":
            return "../../Assets/img_iCloud.png";
          default:
            return (string) null;
        }
      }
    }

    public SubscribeCalendarType SubType { get; set; }

    public string Title { get; set; }

    public string Color { get; set; }

    public string Content { get; set; }

    public int ShowStatus { get; set; }

    public List<SubscribeCalendarViewModel> Parent { get; set; }

    public string Tips { get; set; }

    public bool ShowTips { get; set; }

    public bool IsExpired
    {
      get => this._isExpried;
      set
      {
        this._isExpried = value;
        this.OnPropertyChanged(nameof (IsExpired));
      }
    }

    public SubscribeCalendarViewModel(BindCalendarAccountModel account)
    {
      this.Account = account;
      this.CalendarId = account.Id;
      this.SubType = SubscribeCalendarType.Account;
      if (this.Account.Site == "feishu")
      {
        this.Title = Utils.GetString("FeishuCalendar");
      }
      else
      {
        this.Title = !string.IsNullOrEmpty(account.Description) ? account.Description : account.Account;
        this.Content = !string.IsNullOrEmpty(account.Description) ? account.Description : this.Content;
      }
      this.Kind = account.Kind;
      this.Site = account.Site;
      this.IsExpired = account.Expired;
    }

    public SubscribeCalendarViewModel(CalendarSubscribeProfileModel profile)
    {
      this.CalendarId = profile.Id;
      this.SubType = SubscribeCalendarType.Url;
      this.Title = profile.Name;
      this.Color = profile.Color;
      this.Content = profile.Url;
      this._isExpried = profile.Expired;
      switch (profile.Show)
      {
        case "show":
          this.ShowStatus = 0;
          break;
        case "calendar":
          this.ShowStatus = 1;
          break;
        case "hidden":
          this.ShowStatus = 2;
          break;
      }
      this.Kind = "url";
    }
  }
}
