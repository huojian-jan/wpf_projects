// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Twitter.Tweet
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;

#nullable disable
namespace ticktick_WPF.Util.Twitter
{
  public class Tweet
  {
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    public string CreatedString
    {
      get
      {
        TimeSpan timeSpan = DateTime.Now - this.CreatedAt;
        if (timeSpan < new TimeSpan(1, 0, 0))
          return ((int) timeSpan.TotalMinutes).ToString() + "min";
        return timeSpan < new TimeSpan(24, 0, 0) ? ((int) timeSpan.TotalMinutes / 60).ToString() + "h" : this.CreatedAt.ToString("M");
      }
    }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("retweet_count")]
    public int RetweetCount { get; set; }

    [JsonProperty("favorite_count")]
    public int FavouriteCount { get; set; }

    [JsonProperty("favorited")]
    public bool IsFavourited { get; set; }

    [JsonProperty("retweeted")]
    public bool IsRetweeted { get; set; }

    [JsonProperty("user")]
    public User User { get; set; }
  }
}
