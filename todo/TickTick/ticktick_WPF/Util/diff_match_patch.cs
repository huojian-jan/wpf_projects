// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.diff_match_patch
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Util
{
  public class diff_match_patch
  {
    public float Diff_Timeout = 1f;
    public short Diff_EditCost = 4;
    public short Diff_DualThreshold = 32;
    public float Match_Threshold = 0.5f;
    public int Match_Distance = 1000;
    public float Patch_DeleteThreshold = 0.5f;
    public short Patch_Margin = 4;
    private int Match_MaxBits = 32;
    private Regex BLANKLINEEND = new Regex("\\n\\r?\\n\\Z");
    private Regex BLANKLINESTART = new Regex("\\A\\r?\\n\\r?\\n");

    public List<Diff> diff_main(string text1, string text2) => this.diff_main(text1, text2, true);

    public List<Diff> diff_main(string text1, string text2, bool checklines)
    {
      if (text1 == text2)
        return new List<Diff>()
        {
          new Diff(Operation.EQUAL, text1)
        };
      int num1 = this.diff_commonPrefix(text1, text2);
      string text3 = text1.Substring(0, num1);
      text1 = text1.Substring(num1);
      text2 = text2.Substring(num1);
      int num2 = this.diff_commonSuffix(text1, text2);
      string text4 = text1.Substring(text1.Length - num2);
      text1 = text1.Substring(0, text1.Length - num2);
      text2 = text2.Substring(0, text2.Length - num2);
      List<Diff> diffs = this.diff_compute(text1, text2, checklines);
      if (text3.Length != 0)
        diffs.Insert(0, new Diff(Operation.EQUAL, text3));
      if (text4.Length != 0)
        diffs.Add(new Diff(Operation.EQUAL, text4));
      this.diff_cleanupMerge(diffs);
      return diffs;
    }

    protected List<Diff> diff_compute(string text1, string text2, bool checklines)
    {
      List<Diff> diffList1 = new List<Diff>();
      if (text1.Length == 0)
      {
        diffList1.Add(new Diff(Operation.INSERT, text2));
        return diffList1;
      }
      if (text2.Length == 0)
      {
        diffList1.Add(new Diff(Operation.DELETE, text1));
        return diffList1;
      }
      string str1 = text1.Length > text2.Length ? text1 : text2;
      string text3 = text1.Length > text2.Length ? text2 : text1;
      int length = str1.IndexOf(text3);
      if (length != -1)
      {
        Operation operation = text1.Length > text2.Length ? Operation.DELETE : Operation.INSERT;
        diffList1.Add(new Diff(operation, str1.Substring(0, length)));
        diffList1.Add(new Diff(Operation.EQUAL, text3));
        diffList1.Add(new Diff(operation, str1.Substring(length + text3.Length)));
        return diffList1;
      }
      string str2;
      string str3 = str2 = (string) null;
      string[] strArray = this.diff_halfMatch(text1, text2);
      if (strArray != null)
      {
        string text1_1 = strArray[0];
        string text1_2 = strArray[1];
        string text2_1 = strArray[2];
        string text2_2 = strArray[3];
        string text4 = strArray[4];
        List<Diff> diffList2 = this.diff_main(text1_1, text2_1, checklines);
        List<Diff> collection = this.diff_main(text1_2, text2_2, checklines);
        List<Diff> diffList3 = diffList2;
        diffList3.Add(new Diff(Operation.EQUAL, text4));
        diffList3.AddRange((IEnumerable<Diff>) collection);
        return diffList3;
      }
      if (checklines && (text1.Length < 100 || text2.Length < 100))
        checklines = false;
      List<string> lineArray = (List<string>) null;
      if (checklines)
      {
        object[] chars = this.diff_linesToChars(text1, text2);
        text1 = (string) chars[0];
        text2 = (string) chars[1];
        lineArray = (List<string>) chars[2];
      }
      List<Diff> diffs = this.diff_map(text1, text2);
      if (diffs == null)
      {
        diffs = new List<Diff>();
        diffs.Add(new Diff(Operation.DELETE, text1));
        diffs.Add(new Diff(Operation.INSERT, text2));
      }
      if (checklines)
      {
        this.diff_charsToLines((ICollection<Diff>) diffs, lineArray);
        this.diff_cleanupSemantic(diffs);
        diffs.Add(new Diff(Operation.EQUAL, string.Empty));
        int index1 = 0;
        int num1 = 0;
        int num2 = 0;
        string empty1 = string.Empty;
        string empty2 = string.Empty;
        for (; index1 < diffs.Count; ++index1)
        {
          switch (diffs[index1].operation)
          {
            case Operation.DELETE:
              ++num1;
              empty1 += diffs[index1].text;
              break;
            case Operation.INSERT:
              ++num2;
              empty2 += diffs[index1].text;
              break;
            case Operation.EQUAL:
              if (num1 >= 1 && num2 >= 1)
              {
                List<Diff> collection = this.diff_main(empty1, empty2, false);
                diffs.RemoveRange(index1 - num1 - num2, num1 + num2);
                int index2 = index1 - num1 - num2;
                diffs.InsertRange(index2, (IEnumerable<Diff>) collection);
                index1 = index2 + collection.Count;
              }
              num2 = 0;
              num1 = 0;
              empty1 = string.Empty;
              empty2 = string.Empty;
              break;
          }
        }
        diffs.RemoveAt(diffs.Count - 1);
      }
      return diffs;
    }

    protected object[] diff_linesToChars(string text1, string text2)
    {
      List<string> lineArray = new List<string>();
      Dictionary<string, int> lineHash = new Dictionary<string, int>();
      lineArray.Add(string.Empty);
      return new object[3]
      {
        (object) this.diff_linesToCharsMunge(text1, lineArray, lineHash),
        (object) this.diff_linesToCharsMunge(text2, lineArray, lineHash),
        (object) lineArray
      };
    }

    private string diff_linesToCharsMunge(
      string text,
      List<string> lineArray,
      Dictionary<string, int> lineHash)
    {
      int num1 = 0;
      int num2 = -1;
      StringBuilder stringBuilder = new StringBuilder();
      while (num2 < text.Length - 1)
      {
        num2 = text.IndexOf('\n', num1);
        if (num2 == -1)
          num2 = text.Length - 1;
        string key = text.JavaSubstring(num1, num2 + 1);
        num1 = num2 + 1;
        if (lineHash.ContainsKey(key))
        {
          stringBuilder.Append((char) lineHash[key]);
        }
        else
        {
          lineArray.Add(key);
          lineHash.Add(key, lineArray.Count - 1);
          stringBuilder.Append((char) (lineArray.Count - 1));
        }
      }
      return stringBuilder.ToString();
    }

    protected void diff_charsToLines(ICollection<Diff> diffs, List<string> lineArray)
    {
      foreach (Diff diff in (IEnumerable<Diff>) diffs)
      {
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < diff.text.Length; ++index)
          stringBuilder.Append(lineArray[(int) diff.text[index]]);
        diff.text = stringBuilder.ToString();
      }
    }

    protected List<Diff> diff_map(string text1, string text2)
    {
      DateTime dateTime = DateTime.Now + new TimeSpan(0, 0, (int) this.Diff_Timeout);
      int length1 = text1.Length;
      int length2 = text2.Length;
      int num1 = length1 + length2 - 1;
      bool flag1 = (int) this.Diff_DualThreshold * 2 < num1;
      List<HashSet<long>> v_map1 = new List<HashSet<long>>();
      List<HashSet<long>> v_map2 = new List<HashSet<long>>();
      Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
      Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
      dictionary1.Add(1, 0);
      dictionary2.Add(1, 0);
      long key1 = 0;
      Dictionary<long, int> dictionary3 = new Dictionary<long, int>();
      bool flag2 = false;
      bool flag3 = (length1 + length2) % 2 == 1;
      for (int index = 0; index < num1; ++index)
      {
        if ((double) this.Diff_Timeout > 0.0 && DateTime.Now > dateTime)
          return (List<Diff>) null;
        v_map1.Add(new HashSet<long>());
        for (int key2 = -index; key2 <= index; key2 += 2)
        {
          int num2 = key2 == -index || key2 != index && dictionary1[key2 - 1] < dictionary1[key2 + 1] ? dictionary1[key2 + 1] : dictionary1[key2 - 1] + 1;
          int num3 = num2 - key2;
          if (flag1)
          {
            key1 = this.diff_footprint(num2, num3);
            if (flag3 && dictionary3.ContainsKey(key1))
              flag2 = true;
            if (!flag3)
              dictionary3.Add(key1, index);
          }
          while (!flag2 && num2 < length1 && num3 < length2 && (int) text1[num2] == (int) text2[num3])
          {
            ++num2;
            ++num3;
            if (flag1)
            {
              key1 = this.diff_footprint(num2, num3);
              if (flag3 && dictionary3.ContainsKey(key1))
                flag2 = true;
              if (!flag3)
                dictionary3.Add(key1, index);
            }
          }
          if (dictionary1.ContainsKey(key2))
            dictionary1[key2] = num2;
          else
            dictionary1.Add(key2, num2);
          v_map1[index].Add(this.diff_footprint(num2, num3));
          if (num2 == length1 && num3 == length2)
            return this.diff_path1(v_map1, text1, text2);
          if (flag2)
          {
            List<HashSet<long>> range = v_map2.GetRange(0, dictionary3[key1] + 1);
            List<Diff> diffList = this.diff_path1(v_map1, text1.Substring(0, num2), text2.Substring(0, num3));
            diffList.AddRange((IEnumerable<Diff>) this.diff_path2(range, text1.Substring(num2), text2.Substring(num3)));
            return diffList;
          }
        }
        if (flag1)
        {
          v_map2.Add(new HashSet<long>());
          for (int key3 = -index; key3 <= index; key3 += 2)
          {
            int x = key3 == -index || key3 != index && dictionary2[key3 - 1] < dictionary2[key3 + 1] ? dictionary2[key3 + 1] : dictionary2[key3 - 1] + 1;
            int y = x - key3;
            key1 = this.diff_footprint(length1 - x, length2 - y);
            if (!flag3 && dictionary3.ContainsKey(key1))
              flag2 = true;
            if (flag3)
              dictionary3.Add(key1, index);
            while (!flag2 && x < length1 && y < length2 && (int) text1[length1 - x - 1] == (int) text2[length2 - y - 1])
            {
              ++x;
              ++y;
              key1 = this.diff_footprint(length1 - x, length2 - y);
              if (!flag3 && dictionary3.ContainsKey(key1))
                flag2 = true;
              if (flag3)
                dictionary3.Add(key1, index);
            }
            if (dictionary2.ContainsKey(key3))
              dictionary2[key3] = x;
            else
              dictionary2.Add(key3, x);
            v_map2[index].Add(this.diff_footprint(x, y));
            if (flag2)
            {
              List<Diff> diffList = this.diff_path1(v_map1.GetRange(0, dictionary3[key1] + 1), text1.Substring(0, length1 - x), text2.Substring(0, length2 - y));
              diffList.AddRange((IEnumerable<Diff>) this.diff_path2(v_map2, text1.Substring(length1 - x), text2.Substring(length2 - y)));
              return diffList;
            }
          }
        }
      }
      return (List<Diff>) null;
    }

    protected List<Diff> diff_path1(List<HashSet<long>> v_map, string text1, string text2)
    {
      LinkedList<Diff> source = new LinkedList<Diff>();
      int length1 = text1.Length;
      int length2 = text2.Length;
      Operation? nullable1 = new Operation?();
label_16:
      for (int index = v_map.Count - 2; index >= 0; --index)
      {
        Operation? nullable2;
        while (!v_map[index].Contains(this.diff_footprint(length1 - 1, length2)))
        {
          if (v_map[index].Contains(this.diff_footprint(length1, length2 - 1)))
          {
            --length2;
            nullable2 = nullable1;
            Operation operation = Operation.INSERT;
            if (nullable2.GetValueOrDefault() == operation & nullable2.HasValue)
              source.First<Diff>().text = text2[length2].ToString() + source.First<Diff>().text;
            else
              source.AddFirst(new Diff(Operation.INSERT, text2.Substring(length2, 1)));
            nullable1 = new Operation?(Operation.INSERT);
            goto label_16;
          }
          else
          {
            --length1;
            --length2;
            nullable2 = nullable1;
            Operation operation = Operation.EQUAL;
            if (nullable2.GetValueOrDefault() == operation & nullable2.HasValue)
              source.First<Diff>().text = text1[length1].ToString() + source.First<Diff>().text;
            else
              source.AddFirst(new Diff(Operation.EQUAL, text1.Substring(length1, 1)));
            nullable1 = new Operation?(Operation.EQUAL);
          }
        }
        --length1;
        nullable2 = nullable1;
        Operation operation1 = Operation.DELETE;
        if (nullable2.GetValueOrDefault() == operation1 & nullable2.HasValue)
          source.First<Diff>().text = text1[length1].ToString() + source.First<Diff>().text;
        else
          source.AddFirst(new Diff(Operation.DELETE, text1.Substring(length1, 1)));
        nullable1 = new Operation?(Operation.DELETE);
      }
      return source.ToList<Diff>();
    }

    protected List<Diff> diff_path2(List<HashSet<long>> v_map, string text1, string text2)
    {
      LinkedList<Diff> source = new LinkedList<Diff>();
      int length1 = text1.Length;
      int length2 = text2.Length;
      Operation? nullable1 = new Operation?();
label_16:
      for (int index = v_map.Count - 2; index >= 0; --index)
      {
        Operation? nullable2;
        while (!v_map[index].Contains(this.diff_footprint(length1 - 1, length2)))
        {
          if (v_map[index].Contains(this.diff_footprint(length1, length2 - 1)))
          {
            --length2;
            nullable2 = nullable1;
            Operation operation = Operation.INSERT;
            if (nullable2.GetValueOrDefault() == operation & nullable2.HasValue)
              source.Last<Diff>().text += text2[text2.Length - length2 - 1].ToString();
            else
              source.AddLast(new Diff(Operation.INSERT, text2.Substring(text2.Length - length2 - 1, 1)));
            nullable1 = new Operation?(Operation.INSERT);
            goto label_16;
          }
          else
          {
            --length1;
            --length2;
            nullable2 = nullable1;
            Operation operation = Operation.EQUAL;
            if (nullable2.GetValueOrDefault() == operation & nullable2.HasValue)
              source.Last<Diff>().text += text1[text1.Length - length1 - 1].ToString();
            else
              source.AddLast(new Diff(Operation.EQUAL, text1.Substring(text1.Length - length1 - 1, 1)));
            nullable1 = new Operation?(Operation.EQUAL);
          }
        }
        --length1;
        nullable2 = nullable1;
        Operation operation1 = Operation.DELETE;
        if (nullable2.GetValueOrDefault() == operation1 & nullable2.HasValue)
          source.Last<Diff>().text += text1[text1.Length - length1 - 1].ToString();
        else
          source.AddLast(new Diff(Operation.DELETE, text1.Substring(text1.Length - length1 - 1, 1)));
        nullable1 = new Operation?(Operation.DELETE);
      }
      return source.ToList<Diff>();
    }

    protected long diff_footprint(int x, int y) => ((long) x << 32) + (long) y;

    public int diff_commonPrefix(string text1, string text2)
    {
      int num = Math.Min(text1.Length, text2.Length);
      for (int index = 0; index < num; ++index)
      {
        if ((int) text1[index] != (int) text2[index])
          return index;
      }
      return num;
    }

    public int diff_commonSuffix(string text1, string text2)
    {
      int length1 = text1.Length;
      int length2 = text2.Length;
      int num = Math.Min(text1.Length, text2.Length);
      for (int index = 1; index <= num; ++index)
      {
        if ((int) text1[length1 - index] != (int) text2[length2 - index])
          return index - 1;
      }
      return num;
    }

    protected string[] diff_halfMatch(string text1, string text2)
    {
      string longtext = text1.Length > text2.Length ? text1 : text2;
      string shorttext = text1.Length > text2.Length ? text2 : text1;
      if (longtext.Length < 10 || shorttext.Length < 1)
        return (string[]) null;
      string[] strArray1 = this.diff_halfMatchI(longtext, shorttext, (longtext.Length + 3) / 4);
      string[] strArray2 = this.diff_halfMatchI(longtext, shorttext, (longtext.Length + 1) / 2);
      if (strArray1 == null && strArray2 == null)
        return (string[]) null;
      string[] strArray3 = strArray2 != null ? (strArray1 != null ? (strArray1[4].Length > strArray2[4].Length ? strArray1 : strArray2) : strArray2) : strArray1;
      if (text1.Length > text2.Length)
        return strArray3;
      return new string[5]
      {
        strArray3[2],
        strArray3[3],
        strArray3[0],
        strArray3[1],
        strArray3[4]
      };
    }

    private string[] diff_halfMatchI(string longtext, string shorttext, int i)
    {
      string str1 = longtext.Substring(i, longtext.Length / 4);
      int num = -1;
      string str2 = string.Empty;
      string str3 = string.Empty;
      string str4 = string.Empty;
      string str5 = string.Empty;
      string str6 = string.Empty;
      while (num < shorttext.Length && (num = shorttext.IndexOf(str1, num + 1)) != -1)
      {
        int length1 = this.diff_commonPrefix(longtext.Substring(i), shorttext.Substring(num));
        int length2 = this.diff_commonSuffix(longtext.Substring(0, i), shorttext.Substring(0, num));
        if (str2.Length < length2 + length1)
        {
          str2 = shorttext.Substring(num - length2, length2) + shorttext.Substring(num, length1);
          str3 = longtext.Substring(0, i - length2);
          str4 = longtext.Substring(i + length1);
          str5 = shorttext.Substring(0, num - length2);
          str6 = shorttext.Substring(num + length1);
        }
      }
      if (str2.Length < longtext.Length / 2)
        return (string[]) null;
      return new string[5]{ str3, str4, str5, str6, str2 };
    }

    public void diff_cleanupSemantic(List<Diff> diffs)
    {
      bool flag = false;
      Stack<int> intStack = new Stack<int>();
      string text = (string) null;
      int index = 0;
      int num1 = 0;
      int num2 = 0;
      for (; index < diffs.Count; ++index)
      {
        if (diffs[index].operation == Operation.EQUAL)
        {
          intStack.Push(index);
          num1 = num2;
          num2 = 0;
          text = diffs[index].text;
        }
        else
        {
          num2 += diffs[index].text.Length;
          if (text != null && text.Length <= num1 && text.Length <= num2)
          {
            diffs.Insert(intStack.Peek(), new Diff(Operation.DELETE, text));
            diffs[intStack.Peek() + 1].operation = Operation.INSERT;
            intStack.Pop();
            if (intStack.Count > 0)
              intStack.Pop();
            index = intStack.Count > 0 ? intStack.Peek() : -1;
            num1 = 0;
            num2 = 0;
            text = (string) null;
            flag = true;
          }
        }
      }
      if (flag)
        this.diff_cleanupMerge(diffs);
      this.diff_cleanupSemanticLossless(diffs);
    }

    public void diff_cleanupSemanticLossless(List<Diff> diffs)
    {
      for (int index = 1; index < diffs.Count - 1; ++index)
      {
        if (diffs[index - 1].operation == Operation.EQUAL && diffs[index + 1].operation == Operation.EQUAL)
        {
          string str1 = diffs[index - 1].text;
          string str2 = diffs[index].text;
          string two = diffs[index + 1].text;
          int num1 = this.diff_commonSuffix(str1, str2);
          if (num1 > 0)
          {
            string str3 = str2.Substring(str2.Length - num1);
            str1 = str1.Substring(0, str1.Length - num1);
            str2 = str3 + str2.Substring(0, str2.Length - num1);
            two = str3 + two;
          }
          string str4 = str1;
          string str5 = str2;
          string str6 = two;
          int num2 = this.diff_cleanupSemanticScore(str1, str2) + this.diff_cleanupSemanticScore(str2, two);
          while (str2.Length != 0 && two.Length != 0 && (int) str2[0] == (int) two[0])
          {
            str1 += str2[0].ToString();
            str2 = str2.Substring(1) + two[0].ToString();
            two = two.Substring(1);
            int num3 = this.diff_cleanupSemanticScore(str1, str2) + this.diff_cleanupSemanticScore(str2, two);
            if (num3 >= num2)
            {
              num2 = num3;
              str4 = str1;
              str5 = str2;
              str6 = two;
            }
          }
          if (diffs[index - 1].text != str4)
          {
            if (str4.Length != 0)
            {
              diffs[index - 1].text = str4;
            }
            else
            {
              diffs.RemoveAt(index - 1);
              --index;
            }
            diffs[index].text = str5;
            if (str6.Length != 0)
            {
              diffs[index + 1].text = str6;
            }
            else
            {
              diffs.RemoveAt(index + 1);
              --index;
            }
          }
        }
      }
    }

    private int diff_cleanupSemanticScore(string one, string two)
    {
      if (one.Length == 0 || two.Length == 0)
        return 5;
      int num = 0;
      if (!char.IsLetterOrDigit(one[one.Length - 1]) || !char.IsLetterOrDigit(two[0]))
      {
        ++num;
        if (char.IsWhiteSpace(one[one.Length - 1]) || char.IsWhiteSpace(two[0]))
        {
          ++num;
          if (char.IsControl(one[one.Length - 1]) || char.IsControl(two[0]))
          {
            ++num;
            if (this.BLANKLINEEND.IsMatch(one) || this.BLANKLINESTART.IsMatch(two))
              ++num;
          }
        }
      }
      return num;
    }

    public void diff_cleanupEfficiency(List<Diff> diffs)
    {
      bool flag1 = false;
      Stack<int> intStack = new Stack<int>();
      string text = string.Empty;
      int index = 0;
      bool flag2 = false;
      bool flag3 = false;
      bool flag4 = false;
      bool flag5 = false;
      for (; index < diffs.Count; ++index)
      {
        if (diffs[index].operation == Operation.EQUAL)
        {
          if (diffs[index].text.Length < (int) this.Diff_EditCost && flag4 | flag5)
          {
            intStack.Push(index);
            flag2 = flag4;
            flag3 = flag5;
            text = diffs[index].text;
          }
          else
          {
            intStack.Clear();
            text = string.Empty;
          }
          flag4 = flag5 = false;
        }
        else
        {
          if (diffs[index].operation == Operation.DELETE)
            flag5 = true;
          else
            flag4 = true;
          if (text.Length != 0 && (flag2 & flag3 & flag4 & flag5 || text.Length < (int) this.Diff_EditCost / 2 && (flag2 ? 1 : 0) + (flag3 ? 1 : 0) + (flag4 ? 1 : 0) + (flag5 ? 1 : 0) == 3))
          {
            diffs.Insert(intStack.Peek(), new Diff(Operation.DELETE, text));
            diffs[intStack.Peek() + 1].operation = Operation.INSERT;
            intStack.Pop();
            text = string.Empty;
            if (flag2 & flag3)
            {
              flag4 = flag5 = true;
              intStack.Clear();
            }
            else
            {
              if (intStack.Count > 0)
                intStack.Pop();
              index = intStack.Count > 0 ? intStack.Peek() : -1;
              flag4 = flag5 = false;
            }
            flag1 = true;
          }
        }
      }
      if (!flag1)
        return;
      this.diff_cleanupMerge(diffs);
    }

    public void diff_cleanupMerge(List<Diff> diffs)
    {
      diffs.Add(new Diff(Operation.EQUAL, string.Empty));
      int index1 = 0;
      int num1 = 0;
      int num2 = 0;
      string str1 = string.Empty;
      string str2 = string.Empty;
      while (index1 < diffs.Count)
      {
        switch (diffs[index1].operation)
        {
          case Operation.DELETE:
            ++num1;
            str1 += diffs[index1].text;
            ++index1;
            continue;
          case Operation.INSERT:
            ++num2;
            str2 += diffs[index1].text;
            ++index1;
            continue;
          case Operation.EQUAL:
            if (num1 != 0 || num2 != 0)
            {
              if (num1 != 0 && num2 != 0)
              {
                int num3 = this.diff_commonPrefix(str2, str1);
                if (num3 != 0)
                {
                  if (index1 - num1 - num2 > 0 && diffs[index1 - num1 - num2 - 1].operation == Operation.EQUAL)
                  {
                    diffs[index1 - num1 - num2 - 1].text += str2.Substring(0, num3);
                  }
                  else
                  {
                    diffs.Insert(0, new Diff(Operation.EQUAL, str2.Substring(0, num3)));
                    ++index1;
                  }
                  str2 = str2.Substring(num3);
                  str1 = str1.Substring(num3);
                }
                int num4 = this.diff_commonSuffix(str2, str1);
                if (num4 != 0)
                {
                  diffs[index1].text = str2.Substring(str2.Length - num4) + diffs[index1].text;
                  str2 = str2.Substring(0, str2.Length - num4);
                  str1 = str1.Substring(0, str1.Length - num4);
                }
              }
              if (num1 == 0)
                diffs.Splice<Diff>(index1 - num1 - num2, num1 + num2, new Diff(Operation.INSERT, str2));
              else if (num2 == 0)
                diffs.Splice<Diff>(index1 - num1 - num2, num1 + num2, new Diff(Operation.DELETE, str1));
              else
                diffs.Splice<Diff>(index1 - num1 - num2, num1 + num2, new Diff(Operation.DELETE, str1), new Diff(Operation.INSERT, str2));
              index1 = index1 - num1 - num2 + (num1 != 0 ? 1 : 0) + (num2 != 0 ? 1 : 0) + 1;
            }
            else if (index1 != 0 && diffs[index1 - 1].operation == Operation.EQUAL)
            {
              diffs[index1 - 1].text += diffs[index1].text;
              diffs.RemoveAt(index1);
            }
            else
              ++index1;
            num2 = 0;
            num1 = 0;
            str1 = string.Empty;
            str2 = string.Empty;
            continue;
          default:
            continue;
        }
      }
      if (diffs[diffs.Count - 1].text.Length == 0)
        diffs.RemoveAt(diffs.Count - 1);
      bool flag = false;
      for (int index2 = 1; index2 < diffs.Count - 1; ++index2)
      {
        if (diffs[index2 - 1].operation == Operation.EQUAL && diffs[index2 + 1].operation == Operation.EQUAL)
        {
          if (diffs[index2].text.EndsWith(diffs[index2 - 1].text))
          {
            diffs[index2].text = diffs[index2 - 1].text + diffs[index2].text.Substring(0, diffs[index2].text.Length - diffs[index2 - 1].text.Length);
            diffs[index2 + 1].text = diffs[index2 - 1].text + diffs[index2 + 1].text;
            diffs.Splice<Diff>(index2 - 1, 1);
            flag = true;
          }
          else if (diffs[index2].text.StartsWith(diffs[index2 + 1].text))
          {
            diffs[index2 - 1].text += diffs[index2 + 1].text;
            diffs[index2].text = diffs[index2].text.Substring(diffs[index2 + 1].text.Length) + diffs[index2 + 1].text;
            diffs.Splice<Diff>(index2 + 1, 1);
            flag = true;
          }
        }
      }
      if (!flag)
        return;
      this.diff_cleanupMerge(diffs);
    }

    public int diff_xIndex(List<Diff> diffs, int loc)
    {
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      int num4 = 0;
      Diff diff1 = (Diff) null;
      foreach (Diff diff2 in diffs)
      {
        if (diff2.operation != Operation.INSERT)
          num1 += diff2.text.Length;
        if (diff2.operation != Operation.DELETE)
          num2 += diff2.text.Length;
        if (num1 > loc)
        {
          diff1 = diff2;
          break;
        }
        num3 = num1;
        num4 = num2;
      }
      return diff1 != null && diff1.operation == Operation.DELETE ? num4 : num4 + (loc - num3);
    }

    public string diff_prettyHtml(List<Diff> diffs)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int num = 0;
      foreach (Diff diff in diffs)
      {
        string str = diff.text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\n", "&para;<BR>");
        switch (diff.operation)
        {
          case Operation.DELETE:
            stringBuilder.Append("<DEL STYLE=\"background:#FFE6E6;\" TITLE=\"i=").Append(num).Append("\">").Append(str).Append("</DEL>");
            break;
          case Operation.INSERT:
            stringBuilder.Append("<INS STYLE=\"background:#E6FFE6;\" TITLE=\"i=").Append(num).Append("\">").Append(str).Append("</INS>");
            break;
          case Operation.EQUAL:
            stringBuilder.Append("<SPAN TITLE=\"i=").Append(num).Append("\">").Append(str).Append("</SPAN>");
            break;
        }
        if (diff.operation != Operation.DELETE)
          num += diff.text.Length;
      }
      return stringBuilder.ToString();
    }

    public string diff_text1(List<Diff> diffs)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (Diff diff in diffs)
      {
        if (diff.operation != Operation.INSERT)
          stringBuilder.Append(diff.text);
      }
      return stringBuilder.ToString();
    }

    public string diff_text2(List<Diff> diffs)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (Diff diff in diffs)
      {
        if (diff.operation != Operation.DELETE)
          stringBuilder.Append(diff.text);
      }
      return stringBuilder.ToString();
    }

    public int diff_levenshtein(List<Diff> diffs)
    {
      int num = 0;
      int val1 = 0;
      int val2 = 0;
      foreach (Diff diff in diffs)
      {
        switch (diff.operation)
        {
          case Operation.DELETE:
            val2 += diff.text.Length;
            continue;
          case Operation.INSERT:
            val1 += diff.text.Length;
            continue;
          case Operation.EQUAL:
            num += Math.Max(val1, val2);
            val1 = 0;
            val2 = 0;
            continue;
          default:
            continue;
        }
      }
      return num + Math.Max(val1, val2);
    }

    public string diff_toDelta(List<Diff> diffs)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (Diff diff in diffs)
      {
        switch (diff.operation)
        {
          case Operation.DELETE:
            stringBuilder.Append("-").Append(diff.text.Length).Append("\t");
            continue;
          case Operation.INSERT:
            stringBuilder.Append("+").Append(diff.text).Replace('+', ' ').Append("\t");
            continue;
          case Operation.EQUAL:
            stringBuilder.Append("=").Append(diff.text.Length).Append("\t");
            continue;
          default:
            continue;
        }
      }
      string delta = stringBuilder.ToString();
      if (delta.Length != 0)
        delta = diff_match_patch.unescapeForEncodeUriCompatability(delta.Substring(0, delta.Length - 1));
      return delta;
    }

    public List<Diff> diff_fromDelta(string text1, string delta)
    {
      List<Diff> diffList = new List<Diff>();
      int startIndex = 0;
      string str1 = delta;
      string[] separator = new string[1]{ "\t" };
      foreach (string str2 in str1.Split(separator, StringSplitOptions.None))
      {
        if (str2.Length != 0)
        {
          string str3 = str2.Substring(1);
          switch (str2[0])
          {
            case '+':
              string text2 = str3.Replace("+", "%2b");
              diffList.Add(new Diff(Operation.INSERT, text2));
              continue;
            case '-':
            case '=':
              int int32;
              try
              {
                int32 = Convert.ToInt32(str3);
              }
              catch (FormatException ex)
              {
                throw new ArgumentException("Invalid number in diff_fromDelta: " + str3, (Exception) ex);
              }
              if (int32 < 0)
                throw new ArgumentException("Negative number in diff_fromDelta: " + str3);
              string text3;
              try
              {
                text3 = text1.Substring(startIndex, int32);
                startIndex += int32;
              }
              catch (ArgumentOutOfRangeException ex)
              {
                throw new ArgumentException("Delta length (" + startIndex.ToString() + ") larger than source text length (" + text1.Length.ToString() + ").", (Exception) ex);
              }
              if (str2[0] == '=')
              {
                diffList.Add(new Diff(Operation.EQUAL, text3));
                continue;
              }
              diffList.Add(new Diff(Operation.DELETE, text3));
              continue;
            default:
              throw new ArgumentException("Invalid diff operation in diff_fromDelta: " + str2[0].ToString());
          }
        }
      }
      if (startIndex != text1.Length)
        throw new ArgumentException("Delta length (" + startIndex.ToString() + ") smaller than source text length (" + text1.Length.ToString() + ").");
      return diffList;
    }

    public int match_main(string text, string pattern, int loc)
    {
      loc = Math.Max(0, Math.Min(loc, text.Length));
      if (text == pattern)
        return 0;
      if (text.Length == 0)
        return -1;
      return loc + pattern.Length <= text.Length && text.Substring(loc, pattern.Length) == pattern ? loc : this.match_bitap(text, pattern, loc);
    }

    protected int match_bitap(string text, string pattern, int loc)
    {
      Dictionary<char, int> dictionary = this.match_alphabet(pattern);
      double val2 = (double) this.Match_Threshold;
      int x1 = text.IndexOf(pattern, loc);
      if (x1 != -1)
      {
        val2 = Math.Min(this.match_bitapScore(0, x1, loc, pattern), val2);
        int x2 = text.LastIndexOf(pattern, Math.Min(loc + pattern.Length, text.Length));
        if (x2 != -1)
          val2 = Math.Min(this.match_bitapScore(0, x2, loc, pattern), val2);
      }
      int num1 = 1 << pattern.Length - 1;
      int num2 = -1;
      int num3 = pattern.Length + text.Length;
      int[] numArray1 = new int[0];
      for (int e = 0; e < pattern.Length; ++e)
      {
        int num4 = 0;
        int num5;
        for (num5 = num3; num4 < num5; num5 = (num3 - num4) / 2 + num4)
        {
          if (this.match_bitapScore(e, loc + num5, loc, pattern) <= val2)
            num4 = num5;
          else
            num3 = num5;
        }
        num3 = num5;
        int num6 = Math.Max(1, loc - num5 + 1);
        int num7 = Math.Min(loc + num5, text.Length) + pattern.Length;
        int[] numArray2 = new int[num7 + 2];
        numArray2[num7 + 1] = (1 << e) - 1;
        for (int index = num7; index >= num6; --index)
        {
          int num8 = text.Length <= index - 1 || !dictionary.ContainsKey(text[index - 1]) ? 0 : dictionary[text[index - 1]];
          numArray2[index] = e != 0 ? (numArray2[index + 1] << 1 | 1) & num8 | (numArray1[index + 1] | numArray1[index]) << 1 | 1 | numArray1[index + 1] : (numArray2[index + 1] << 1 | 1) & num8;
          if ((numArray2[index] & num1) != 0)
          {
            double num9 = this.match_bitapScore(e, index - 1, loc, pattern);
            if (num9 <= val2)
            {
              val2 = num9;
              num2 = index - 1;
              if (num2 > loc)
                num6 = Math.Max(1, 2 * loc - num2);
              else
                break;
            }
          }
        }
        if (this.match_bitapScore(e + 1, loc, loc, pattern) <= val2)
          numArray1 = numArray2;
        else
          break;
      }
      return num2;
    }

    private double match_bitapScore(int e, int x, int loc, string pattern)
    {
      float num1 = (float) e / (float) pattern.Length;
      int num2 = Math.Abs(loc - x);
      if (this.Match_Distance != 0)
        return (double) num1 + (double) num2 / (double) this.Match_Distance;
      return num2 != 0 ? 1.0 : (double) num1;
    }

    protected Dictionary<char, int> match_alphabet(string pattern)
    {
      Dictionary<char, int> dictionary = new Dictionary<char, int>();
      char[] charArray = pattern.ToCharArray();
      foreach (char key in charArray)
      {
        if (!dictionary.ContainsKey(key))
          dictionary.Add(key, 0);
      }
      int num1 = 0;
      foreach (char key in charArray)
      {
        int num2 = dictionary[key] | 1 << pattern.Length - num1 - 1;
        dictionary[key] = num2;
        ++num1;
      }
      return dictionary;
    }

    protected void patch_addContext(Patch patch, string text)
    {
      if (text.Length == 0)
        return;
      string str = text.Substring(patch.start2, patch.length1);
      int num1 = 0;
      for (; text.IndexOf(str) != text.LastIndexOf(str) && str.Length < this.Match_MaxBits - (int) this.Patch_Margin - (int) this.Patch_Margin; str = text.JavaSubstring(Math.Max(0, patch.start2 - num1), Math.Min(text.Length, patch.start2 + patch.length1 + num1)))
        num1 += (int) this.Patch_Margin;
      int num2 = num1 + (int) this.Patch_Margin;
      string text1 = text.JavaSubstring(Math.Max(0, patch.start2 - num2), patch.start2);
      if (text1.Length != 0)
        patch.diffs.Insert(0, new Diff(Operation.EQUAL, text1));
      string text2 = text.JavaSubstring(patch.start2 + patch.length1, Math.Min(text.Length, patch.start2 + patch.length1 + num2));
      if (text2.Length != 0)
        patch.diffs.Add(new Diff(Operation.EQUAL, text2));
      patch.start1 -= text1.Length;
      patch.start2 -= text1.Length;
      patch.length1 += text1.Length + text2.Length;
      patch.length2 += text1.Length + text2.Length;
    }

    public List<Patch> patch_make(string text1, string text2)
    {
      List<Diff> diffs = this.diff_main(text1, text2, true);
      if (diffs.Count > 2)
      {
        this.diff_cleanupSemantic(diffs);
        this.diff_cleanupEfficiency(diffs);
      }
      return this.patch_make(text1, diffs);
    }

    public List<Patch> patch_make(List<Diff> diffs)
    {
      return this.patch_make(this.diff_text1(diffs), diffs);
    }

    public List<Patch> patch_make(string text1, string text2, List<Diff> diffs)
    {
      return this.patch_make(text1, diffs);
    }

    public List<Patch> patch_make(string text1, List<Diff> diffs)
    {
      List<Patch> patchList = new List<Patch>();
      if (diffs.Count == 0)
        return patchList;
      Patch patch = new Patch();
      int num1 = 0;
      int num2 = 0;
      string text = text1;
      string str = text1;
      foreach (Diff diff in diffs)
      {
        if (patch.diffs.Count == 0 && diff.operation != Operation.EQUAL)
        {
          patch.start1 = num1;
          patch.start2 = num2;
        }
        switch (diff.operation)
        {
          case Operation.DELETE:
            patch.length1 += diff.text.Length;
            patch.diffs.Add(diff);
            str = str.Substring(0, num2) + str.Substring(num2 + diff.text.Length);
            break;
          case Operation.INSERT:
            patch.diffs.Add(diff);
            patch.length2 += diff.text.Length;
            str = str.Substring(0, num2) + diff.text + str.Substring(num2);
            break;
          case Operation.EQUAL:
            if (diff.text.Length <= 2 * (int) this.Patch_Margin && patch.diffs.Count<Diff>() != 0 && diff != diffs.Last<Diff>())
            {
              patch.diffs.Add(diff);
              patch.length1 += diff.text.Length;
              patch.length2 += diff.text.Length;
            }
            if (diff.text.Length >= 2 * (int) this.Patch_Margin && patch.diffs.Count != 0)
            {
              this.patch_addContext(patch, text);
              patchList.Add(patch);
              patch = new Patch();
              text = str;
              num1 = num2;
              break;
            }
            break;
        }
        if (diff.operation != Operation.INSERT)
          num1 += diff.text.Length;
        if (diff.operation != Operation.DELETE)
          num2 += diff.text.Length;
      }
      if (patch.diffs.Count != 0)
      {
        this.patch_addContext(patch, text);
        patchList.Add(patch);
      }
      return patchList;
    }

    public List<Patch> patch_deepCopy(List<Patch> patches)
    {
      List<Patch> patchList = new List<Patch>();
      foreach (Patch patch1 in patches)
      {
        Patch patch2 = new Patch();
        foreach (Diff diff1 in patch1.diffs)
        {
          Diff diff2 = new Diff(diff1.operation, diff1.text);
          patch2.diffs.Add(diff2);
        }
        patch2.start1 = patch1.start1;
        patch2.start2 = patch1.start2;
        patch2.length1 = patch1.length1;
        patch2.length2 = patch1.length2;
        patchList.Add(patch2);
      }
      return patchList;
    }

    public object[] patch_apply(List<Patch> patches, string text)
    {
      if (patches.Count == 0)
        return new object[2]
        {
          (object) text,
          (object) new bool[0]
        };
      patches = this.patch_deepCopy(patches);
      string str1 = this.patch_addPadding(patches);
      text = str1 + text + str1;
      this.patch_splitMax(patches);
      int index = 0;
      int num1 = 0;
      bool[] flagArray = new bool[patches.Count];
      foreach (Patch patch in patches)
      {
        int loc1 = patch.start2 + num1;
        string str2 = this.diff_text1(patch.diffs);
        int num2 = -1;
        int num3;
        if (str2.Length > this.Match_MaxBits)
        {
          num3 = this.match_main(text, str2.Substring(0, this.Match_MaxBits), loc1);
          if (num3 != -1)
          {
            num2 = this.match_main(text, str2.Substring(str2.Length - this.Match_MaxBits), loc1 + str2.Length - this.Match_MaxBits);
            if (num2 == -1 || num3 >= num2)
              num3 = -1;
          }
        }
        else
          num3 = this.match_main(text, str2, loc1);
        if (num3 == -1)
        {
          flagArray[index] = false;
          num1 -= patch.length2 - patch.length1;
        }
        else
        {
          flagArray[index] = true;
          num1 = num3 - loc1;
          string text2 = num2 != -1 ? text.JavaSubstring(num3, Math.Min(num2 + this.Match_MaxBits, text.Length)) : text.JavaSubstring(num3, Math.Min(num3 + str2.Length, text.Length));
          if (str2 == text2)
          {
            text = text.Substring(0, num3) + this.diff_text2(patch.diffs) + text.Substring(num3 + str2.Length);
          }
          else
          {
            List<Diff> diffs = this.diff_main(str2, text2, false);
            if (str2.Length > this.Match_MaxBits && (double) this.diff_levenshtein(diffs) / (double) str2.Length > (double) this.Patch_DeleteThreshold)
            {
              flagArray[index] = false;
            }
            else
            {
              this.diff_cleanupSemanticLossless(diffs);
              int loc2 = 0;
              foreach (Diff diff in patch.diffs)
              {
                if (diff.operation != Operation.EQUAL)
                {
                  int num4 = this.diff_xIndex(diffs, loc2);
                  if (diff.operation == Operation.INSERT)
                    text = text.Insert(num3 + num4, diff.text);
                  else if (diff.operation == Operation.DELETE)
                    text = text.Substring(0, num3 + num4) + text.Substring(num3 + this.diff_xIndex(diffs, loc2 + diff.text.Length));
                }
                if (diff.operation != Operation.DELETE)
                  loc2 += diff.text.Length;
              }
            }
          }
        }
        ++index;
      }
      text = text.JavaSubstring(str1.Length, text.Length - str1.Length);
      return new object[2]
      {
        (object) text,
        (object) flagArray
      };
    }

    public string patch_addPadding(List<Patch> patches)
    {
      int patchMargin = (int) this.Patch_Margin;
      string empty = string.Empty;
      for (int index = 1; index <= patchMargin; ++index)
        empty += ((char) index).ToString();
      foreach (Patch patch in patches)
      {
        patch.start1 += patchMargin;
        patch.start2 += patchMargin;
      }
      Patch patch1 = patches.First<Patch>();
      List<Diff> diffs1 = patch1.diffs;
      if (diffs1.Count == 0 || diffs1.First<Diff>().operation != Operation.EQUAL)
      {
        diffs1.Insert(0, new Diff(Operation.EQUAL, empty));
        patch1.start1 -= patchMargin;
        patch1.start2 -= patchMargin;
        patch1.length1 += patchMargin;
        patch1.length2 += patchMargin;
      }
      else if (patchMargin > diffs1.First<Diff>().text.Length)
      {
        Diff diff = diffs1.First<Diff>();
        int num = patchMargin - diff.text.Length;
        diff.text = empty.Substring(diff.text.Length) + diff.text;
        patch1.start1 -= num;
        patch1.start2 -= num;
        patch1.length1 += num;
        patch1.length2 += num;
      }
      Patch patch2 = patches.Last<Patch>();
      List<Diff> diffs2 = patch2.diffs;
      if (diffs2.Count == 0 || diffs2.Last<Diff>().operation != Operation.EQUAL)
      {
        diffs2.Add(new Diff(Operation.EQUAL, empty));
        patch2.length1 += patchMargin;
        patch2.length2 += patchMargin;
      }
      else if (patchMargin > diffs2.Last<Diff>().text.Length)
      {
        Diff diff = diffs2.Last<Diff>();
        int length = patchMargin - diff.text.Length;
        diff.text += empty.Substring(0, length);
        patch2.length1 += length;
        patch2.length2 += length;
      }
      return empty;
    }

    public void patch_splitMax(List<Patch> patches)
    {
      for (int index = 0; index < patches.Count; ++index)
      {
        if (patches[index].length1 > this.Match_MaxBits)
        {
          Patch patch1 = patches[index];
          patches.Splice<Patch>(index--, 1);
          int matchMaxBits = this.Match_MaxBits;
          int start1 = patch1.start1;
          int start2 = patch1.start2;
          string text1 = string.Empty;
          while (patch1.diffs.Count != 0)
          {
            Patch patch2 = new Patch();
            bool flag = true;
            patch2.start1 = start1 - text1.Length;
            patch2.start2 = start2 - text1.Length;
            if (text1.Length != 0)
            {
              patch2.length1 = patch2.length2 = text1.Length;
              patch2.diffs.Add(new Diff(Operation.EQUAL, text1));
            }
            while (patch1.diffs.Count != 0 && patch2.length1 < matchMaxBits - (int) this.Patch_Margin)
            {
              Operation operation = patch1.diffs[0].operation;
              string text2 = patch1.diffs[0].text;
              switch (operation)
              {
                case Operation.DELETE:
                  if (patch2.diffs.Count == 1 && patch2.diffs.First<Diff>().operation == Operation.EQUAL && text2.Length > 2 * matchMaxBits)
                  {
                    patch2.length1 += text2.Length;
                    start1 += text2.Length;
                    flag = false;
                    patch2.diffs.Add(new Diff(operation, text2));
                    patch1.diffs.RemoveAt(0);
                    continue;
                  }
                  break;
                case Operation.INSERT:
                  patch2.length2 += text2.Length;
                  start2 += text2.Length;
                  patch2.diffs.Add(patch1.diffs.First<Diff>());
                  patch1.diffs.RemoveAt(0);
                  flag = false;
                  continue;
              }
              string text3 = text2.Substring(0, Math.Min(text2.Length, matchMaxBits - patch2.length1 - (int) this.Patch_Margin));
              patch2.length1 += text3.Length;
              start1 += text3.Length;
              if (operation == Operation.EQUAL)
              {
                patch2.length2 += text3.Length;
                start2 += text3.Length;
              }
              else
                flag = false;
              patch2.diffs.Add(new Diff(operation, text3));
              if (text3 == patch1.diffs[0].text)
                patch1.diffs.RemoveAt(0);
              else
                patch1.diffs[0].text = patch1.diffs[0].text.Substring(text3.Length);
            }
            string str = this.diff_text2(patch2.diffs);
            text1 = str.Substring(Math.Max(0, str.Length - (int) this.Patch_Margin));
            string text4 = this.diff_text1(patch1.diffs).Length <= (int) this.Patch_Margin ? this.diff_text1(patch1.diffs) : this.diff_text1(patch1.diffs).Substring(0, (int) this.Patch_Margin);
            if (text4.Length != 0)
            {
              patch2.length1 += text4.Length;
              patch2.length2 += text4.Length;
              if (patch2.diffs.Count != 0 && patch2.diffs[patch2.diffs.Count - 1].operation == Operation.EQUAL)
                patch2.diffs[patch2.diffs.Count - 1].text += text4;
              else
                patch2.diffs.Add(new Diff(Operation.EQUAL, text4));
            }
            if (!flag)
              patches.Splice<Patch>(++index, 0, patch2);
          }
        }
      }
    }

    public string patch_toText(List<Patch> patches)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (Patch patch in patches)
        stringBuilder.Append((object) patch);
      return stringBuilder.ToString();
    }

    public List<Patch> patch_fromText(string textline)
    {
      List<Patch> patchList = new List<Patch>();
      if (textline.Length == 0)
        return patchList;
      LinkedList<string> source = new LinkedList<string>((IEnumerable<string>) new List<string>((IEnumerable<string>) textline.Split(new string[1]
      {
        "\n"
      }, StringSplitOptions.None)));
      Regex regex = new Regex("^@@ -(\\d+),?(\\d*) \\+(\\d+),?(\\d*) @@$");
label_25:
      while (source.Count != 0)
      {
        Match match = regex.Match(source.First<string>());
        if (!match.Success)
          throw new ArgumentException("Invalid patch string: " + source.First<string>());
        Patch patch = new Patch();
        patchList.Add(patch);
        patch.start1 = Convert.ToInt32(match.Groups[1].Value);
        if (match.Groups[2].Length == 0)
        {
          --patch.start1;
          patch.length1 = 1;
        }
        else if (match.Groups[2].Value == "0")
        {
          patch.length1 = 0;
        }
        else
        {
          --patch.start1;
          patch.length1 = Convert.ToInt32(match.Groups[2].Value);
        }
        patch.start2 = Convert.ToInt32(match.Groups[3].Value);
        if (match.Groups[4].Length == 0)
        {
          --patch.start2;
          patch.length2 = 1;
        }
        else if (match.Groups[4].Value == "0")
        {
          patch.length2 = 0;
        }
        else
        {
          --patch.start2;
          patch.length2 = Convert.ToInt32(match.Groups[4].Value);
        }
        source.RemoveFirst();
        while (source.Count != 0)
        {
          char ch;
          try
          {
            ch = source.First<string>()[0];
          }
          catch (IndexOutOfRangeException ex)
          {
            source.RemoveFirst();
            continue;
          }
          string text = source.First<string>().Substring(1).Replace("+", "%2b");
          switch (ch)
          {
            case ' ':
              patch.diffs.Add(new Diff(Operation.EQUAL, text));
              break;
            case '+':
              patch.diffs.Add(new Diff(Operation.INSERT, text));
              break;
            case '-':
              patch.diffs.Add(new Diff(Operation.DELETE, text));
              break;
            case '@':
              goto label_25;
            default:
              throw new ArgumentException("Invalid patch mode '" + ch.ToString() + "' in: " + text);
          }
          source.RemoveFirst();
        }
      }
      return patchList;
    }

    public static string unescapeForEncodeUriCompatability(string str)
    {
      return str.Replace("%21", "!").Replace("%7e", "~").Replace("%27", "'").Replace("%28", "(").Replace("%29", ")").Replace("%3b", ";").Replace("%2f", "/").Replace("%3f", "?").Replace("%3a", ":").Replace("%40", "@").Replace("%26", "&").Replace("%3d", "=").Replace("%2b", "+").Replace("%24", "$").Replace("%2c", ",").Replace("%23", "#");
    }
  }
}
