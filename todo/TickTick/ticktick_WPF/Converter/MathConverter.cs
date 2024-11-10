// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.MathConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class MathConverter : IValueConverter
  {
    private static readonly char[] _allOperators = new char[7]
    {
      '+',
      '-',
      '*',
      '/',
      '%',
      '(',
      ')'
    };
    private static readonly List<string> _grouping = new List<string>()
    {
      "(",
      ")"
    };
    private static readonly List<string> _operators = new List<string>()
    {
      "+",
      "-",
      "*",
      "/",
      "%"
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string mathEquation = (parameter as string).Replace(" ", "").Replace("@VALUE", value.ToString());
      List<double> numbers = new List<double>();
      foreach (string str in mathEquation.Split(MathConverter._allOperators))
      {
        double db;
        if (str != string.Empty && MathUtil.TryParseString2Double(str, out db))
          numbers.Add(db);
      }
      try
      {
        this.EvaluateMathString(ref mathEquation, ref numbers, 0);
      }
      catch (Exception ex)
      {
      }
      return (object) numbers[0];
    }

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      return (object) null;
    }

    private void EvaluateMathString(ref string mathEquation, ref List<double> numbers, int index)
    {
      for (string nextToken1 = MathConverter.GetNextToken(mathEquation); nextToken1 != string.Empty; nextToken1 = MathConverter.GetNextToken(mathEquation))
      {
        mathEquation = mathEquation.Remove(0, nextToken1.Length);
        if (MathConverter._grouping.Contains(nextToken1))
        {
          switch (nextToken1)
          {
            case "(":
              this.EvaluateMathString(ref mathEquation, ref numbers, index);
              break;
            case ")":
              return;
          }
        }
        if (MathConverter._operators.Contains(nextToken1))
        {
          string nextToken2 = MathConverter.GetNextToken(mathEquation);
          if (nextToken2 == "(")
            this.EvaluateMathString(ref mathEquation, ref numbers, index + 1);
          double db;
          MathUtil.TryParseString2Double(nextToken2, out db);
          if (numbers.Count <= index + 1 || db != numbers[index + 1] && !(nextToken2 == "("))
            break;
          switch (nextToken1)
          {
            case "+":
              numbers[index] = numbers[index] + numbers[index + 1];
              break;
            case "-":
              numbers[index] = numbers[index] - numbers[index + 1];
              break;
            case "*":
              numbers[index] = numbers[index] * numbers[index + 1];
              break;
            case "/":
              numbers[index] = numbers[index] / numbers[index + 1];
              break;
            case "%":
              numbers[index] = numbers[index] % numbers[index + 1];
              break;
          }
          numbers.RemoveAt(index + 1);
        }
      }
    }

    private static string GetNextToken(string mathEquation)
    {
      if (mathEquation == string.Empty)
        return string.Empty;
      string nextToken = "";
      foreach (char ch in mathEquation)
      {
        if (((IEnumerable<char>) MathConverter._allOperators).Contains<char>(ch))
          return !(nextToken == "") ? nextToken : ch.ToString();
        nextToken += ch.ToString();
      }
      return nextToken;
    }
  }
}
