// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Diff
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util
{
  public class Diff
  {
    public Operation operation;
    public string text;

    public Diff(Operation operation, string text)
    {
      this.operation = operation;
      this.text = text;
    }

    public override string ToString()
    {
      string str = this.text.Replace('\n', '¶');
      return "Diff(" + this.operation.ToString() + ",\"" + str + "\")";
    }

    public override bool Equals(object obj)
    {
      return obj != null && obj is Diff diff && diff.operation == this.operation && diff.text == this.text;
    }

    public bool Equals(Diff obj)
    {
      return obj != null && obj.operation == this.operation && obj.text == this.text;
    }

    public override int GetHashCode() => this.text.GetHashCode() ^ this.operation.GetHashCode();
  }
}
