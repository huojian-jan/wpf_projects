// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Resource.AttachmentInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Resource
{
  public class AttachmentInfo
  {
    public readonly string Value;
    public readonly string Kind;
    public readonly string Url;
    public readonly int Offset;
    public bool Duplicate;

    public AttachmentInfo(string value, string kind, string url, int offset)
    {
      this.Value = value;
      this.Kind = kind;
      this.Url = url;
      this.Offset = offset;
    }
  }
}
