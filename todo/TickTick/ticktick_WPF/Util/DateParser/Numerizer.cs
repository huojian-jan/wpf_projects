// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.Numerizer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public class Numerizer
  {
    private static readonly Regex Dehalfer = new Regex("a half", RegexOptions.IgnoreCase);
    private static readonly Regex Dehaalfer = new Regex("(\\d+)(?: | and |-)*haAlf", RegexOptions.IgnoreCase);
    protected static Numerizer.DirectNum[] DirectNums = new List<Numerizer.DirectNum>()
    {
      new Numerizer.DirectNum("first", "1"),
      new Numerizer.DirectNum("second", "2"),
      new Numerizer.DirectNum("third", "3"),
      new Numerizer.DirectNum("fourth", "4"),
      new Numerizer.DirectNum("fifth", "5"),
      new Numerizer.DirectNum("sixth", "6"),
      new Numerizer.DirectNum("seventh", "7"),
      new Numerizer.DirectNum("eighth", "8"),
      new Numerizer.DirectNum("ninth", "9"),
      new Numerizer.DirectNum("tenth", "10"),
      new Numerizer.DirectNum("eleventh", "11"),
      new Numerizer.DirectNum("twelfth", "12"),
      new Numerizer.DirectNum("thirteenth", "13"),
      new Numerizer.DirectNum("fourteenth", "14"),
      new Numerizer.DirectNum("fifteenth", "15"),
      new Numerizer.DirectNum("sixteenth", "16"),
      new Numerizer.DirectNum("seventeenth", "17"),
      new Numerizer.DirectNum("eighteenth", "18"),
      new Numerizer.DirectNum("nineteenth", "19"),
      new Numerizer.DirectNum("twentieth", "20"),
      new Numerizer.DirectNum("thirtieth", "30"),
      new Numerizer.DirectNum("eleven", "11"),
      new Numerizer.DirectNum("twelve", "12"),
      new Numerizer.DirectNum("thirteen", "13"),
      new Numerizer.DirectNum("fourteen", "14"),
      new Numerizer.DirectNum("fifteen", "15"),
      new Numerizer.DirectNum("sixteen", "16"),
      new Numerizer.DirectNum("seventeen", "17"),
      new Numerizer.DirectNum("eighteen", "18"),
      new Numerizer.DirectNum("nineteen", "19"),
      new Numerizer.DirectNum("ninteen", "19"),
      new Numerizer.DirectNum("zero", "0"),
      new Numerizer.DirectNum("one", "1"),
      new Numerizer.DirectNum("two", "2"),
      new Numerizer.DirectNum("three", "3"),
      new Numerizer.DirectNum("four(\\W|$)", "4$1"),
      new Numerizer.DirectNum("five", "5"),
      new Numerizer.DirectNum("six(\\W|$)", "6$1"),
      new Numerizer.DirectNum("seven(\\W|$)", "7$1"),
      new Numerizer.DirectNum("eight(\\W|$)", "8$1"),
      new Numerizer.DirectNum("nine(\\W|$)", "9$1"),
      new Numerizer.DirectNum("ten", "10"),
      new Numerizer.DirectNum("\\ba\\b", "1")
    }.ToArray();
    protected static Numerizer.TenPrefix[] TenPrefixes = new List<Numerizer.TenPrefix>()
    {
      new Numerizer.TenPrefix("twenty", 20L),
      new Numerizer.TenPrefix("thirty", 30L),
      new Numerizer.TenPrefix("fourty", 40L),
      new Numerizer.TenPrefix("fifty", 50L),
      new Numerizer.TenPrefix("sixty", 60L),
      new Numerizer.TenPrefix("seventy", 70L),
      new Numerizer.TenPrefix("eighty", 80L),
      new Numerizer.TenPrefix("ninety", 90L),
      new Numerizer.TenPrefix("ninty", 90L)
    }.ToArray();
    protected static Numerizer.BigPrefix[] BigPrefixes = new List<Numerizer.BigPrefix>()
    {
      new Numerizer.BigPrefix("hundred", 100L),
      new Numerizer.BigPrefix("thousand", 1000L),
      new Numerizer.BigPrefix("million", 1000000L),
      new Numerizer.BigPrefix("billion", 1000000000L),
      new Numerizer.BigPrefix("trillion", 1000000000000L)
    }.ToArray();

    public static string Numerize(string str)
    {
      string input1 = str;
      string seed = Numerizer.Dehalfer.Replace(input1, "haAlf");
      string input2 = ((IEnumerable<Numerizer.DirectNum>) Numerizer.DirectNums).Aggregate<Numerizer.DirectNum, string>(seed, (Func<string, Numerizer.DirectNum, string>) ((current, dn) => dn.GetName().Replace(current, dn.GetNumber())));
      long num;
      foreach (Numerizer.TenPrefix tenPrefix in Numerizer.TenPrefixes)
      {
        Match match = tenPrefix.GetName().Match(input2);
        if (match.Success)
        {
          do
          {
            Regex name = tenPrefix.GetName();
            string input3 = input2;
            string replacement;
            if (!string.IsNullOrWhiteSpace(match.Groups[1].Value))
            {
              num = tenPrefix.GetNumber() + long.Parse(match.Groups[1].Value.Trim());
              replacement = num.ToString();
            }
            else
            {
              num = tenPrefix.GetNumber();
              replacement = num.ToString();
            }
            input2 = name.Replace(input3, replacement);
            match = tenPrefix.GetName().Match(input2);
          }
          while (match.Success);
        }
      }
      foreach (Numerizer.BigPrefix bigPrefix in Numerizer.BigPrefixes)
      {
        Match match = bigPrefix.GetName().Match(input2);
        if (match.Success)
        {
          do
          {
            Regex name = bigPrefix.GetName();
            string input4 = input2;
            string replacement;
            if (!string.IsNullOrWhiteSpace(match.Groups[1].Value))
            {
              num = bigPrefix.GetNumber() + long.Parse(match.Groups[1].Value.Trim());
              replacement = num.ToString();
            }
            else
            {
              num = bigPrefix.GetNumber();
              replacement = num.ToString();
            }
            input2 = name.Replace(input4, replacement);
            match = bigPrefix.GetName().Match(input2);
          }
          while (match.Success);
        }
      }
      Match match1 = Numerizer.Dehaalfer.Match(input2);
      if (match1.Success)
      {
        do
        {
          input2 = Numerizer.Dehaalfer.Replace(input2, (float.Parse(match1.Groups[1].Value.Trim()) + 0.5f).ToString((IFormatProvider) CultureInfo.InvariantCulture));
          match1 = Numerizer.Dehaalfer.Match(input2);
        }
        while (match1.Success);
      }
      return input2;
    }

    protected class DirectNum
    {
      private readonly Regex _name;
      private readonly string _number;

      public DirectNum(string name, string number)
      {
        this._name = new Regex(name, RegexOptions.IgnoreCase);
        this._number = number;
      }

      public Regex GetName() => this._name;

      public string GetNumber() => this._number;
    }

    protected class TenPrefix
    {
      private readonly Regex _name;
      private readonly long _number;

      public TenPrefix(string name, long number)
      {
        this._name = new Regex("(?:" + name + ")( *\\d(?=\\D|$))*", RegexOptions.IgnoreCase);
        this._number = number;
      }

      public Regex GetName() => this._name;

      public long GetNumber() => this._number;
    }

    protected class BigPrefix
    {
      private readonly Regex _name;
      private readonly long _number;

      public BigPrefix(string name, long number)
      {
        this._name = new Regex("(\\d*) *" + name, RegexOptions.IgnoreCase);
        this._number = number;
      }

      public Regex GetName() => this._name;

      public long GetNumber() => this._number;
    }
  }
}
