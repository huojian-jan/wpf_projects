// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Patch
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Text;

#nullable disable
namespace ticktick_WPF.Util
{
  public class Patch
  {
    public List<Diff> diffs = new List<Diff>();
    public int start1;
    public int start2;
    public int length1;
    public int length2;

    public override string ToString()
    {
      string str1 = this.length1 != 0 ? (this.length1 != 1 ? (this.start1 + 1).ToString() + "," + this.length1.ToString() : Convert.ToString(this.start1 + 1)) : this.start1.ToString() + ",0";
      string str2 = this.length2 != 0 ? (this.length2 != 1 ? (this.start2 + 1).ToString() + "," + this.length2.ToString() : Convert.ToString(this.start2 + 1)) : this.start2.ToString() + ",0";
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("@@ -").Append(str1).Append(" +").Append(str2).Append(" @@\n");
      foreach (Diff diff in this.diffs)
      {
        switch (diff.operation)
        {
          case Operation.DELETE:
            stringBuilder.Append('-');
            break;
          case Operation.INSERT:
            stringBuilder.Append('+');
            break;
          case Operation.EQUAL:
            stringBuilder.Append(' ');
            break;
        }
        stringBuilder.Append(diff.text.Replace('+', ' ')).Append("\n");
      }
      return diff_match_patch.unescapeForEncodeUriCompatability(stringBuilder.ToString());
    }
  }
}
