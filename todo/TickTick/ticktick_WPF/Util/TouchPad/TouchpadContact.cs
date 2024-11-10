// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TouchPad.TouchpadContact
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Util.TouchPad
{
  public struct TouchpadContact : IEquatable<TouchpadContact>
  {
    public int ContactId { get; }

    public int X { get; }

    public int Y { get; }

    public TouchpadContact(int contactId, int x, int y)
    {
      int num1 = contactId;
      int num2 = x;
      int num3 = y;
      this.ContactId = num1;
      this.X = num2;
      this.Y = num3;
    }

    public override bool Equals(object obj) => obj is TouchpadContact other && this.Equals(other);

    public bool Equals(TouchpadContact other)
    {
      return this.ContactId == other.ContactId && this.X == other.X && this.Y == other.Y;
    }

    public static bool operator ==(TouchpadContact a, TouchpadContact b) => a.Equals(b);

    public static bool operator !=(TouchpadContact a, TouchpadContact b) => !(a == b);

    public override int GetHashCode() => (this.ContactId, this.X, this.Y).GetHashCode();

    public override string ToString()
    {
      return string.Format("Contact ID:{0} Point:{1},{2}", (object) this.ContactId, (object) this.X, (object) this.Y);
    }
  }
}
