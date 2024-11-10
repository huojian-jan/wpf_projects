// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TouchPad.TouchpadContactCreator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util.TouchPad
{
  internal class TouchpadContactCreator
  {
    public int? ContactId { get; set; }

    public int? X { get; set; }

    public int? Y { get; set; }

    public bool TryCreate(out TouchpadContact contact)
    {
      if (this.ContactId.HasValue && this.X.HasValue && this.Y.HasValue)
      {
        contact = new TouchpadContact(this.ContactId.Value, this.X.Value, this.Y.Value);
        return true;
      }
      contact = new TouchpadContact();
      return false;
    }

    public void Clear()
    {
      this.ContactId = new int?();
      this.X = new int?();
      this.Y = new int?();
    }
  }
}
