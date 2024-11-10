// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Twitter.TwitterClient
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util.Twitter
{
  public class TwitterClient : BaseRequest
  {
    private readonly OAuthInfo oauth;

    public TwitterClient(OAuthInfo oauth)
      : base("https://api.twitter.com/1.1/statuses/", oauth)
    {
      this.oauth = oauth;
    }
  }
}
