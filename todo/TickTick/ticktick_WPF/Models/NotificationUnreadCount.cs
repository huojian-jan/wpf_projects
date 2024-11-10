// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.NotificationUnreadCount
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class NotificationUnreadCount : BaseViewModel
  {
    private int _activity;
    private int _notification;

    [JsonProperty(PropertyName = "activity")]
    public int Activity
    {
      get => this._activity;
      set
      {
        this._activity = value;
        this.OnPropertyChanged(nameof (Activity));
      }
    }

    [JsonProperty(PropertyName = "notification")]
    public int Notification
    {
      get => this._notification;
      set
      {
        this._notification = value;
        this.OnPropertyChanged(nameof (Notification));
      }
    }

    public NotificationUnreadCount()
    {
    }

    public NotificationUnreadCount(int activity, int notification)
    {
      this.Activity = activity;
      this.Notification = notification;
    }
  }
}
