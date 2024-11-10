// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KotlinUtils.Attachment
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util.KotlinUtils
{
  public struct Attachment
  {
    public long uniqueId;
    public string id;

    public Attachment(int uid, string id)
    {
      this.uniqueId = (long) uid;
      this.id = id;
    }
  }
}
