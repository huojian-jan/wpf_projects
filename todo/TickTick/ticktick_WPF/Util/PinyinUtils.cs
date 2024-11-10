// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.PinyinUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class PinyinUtils
  {
    private static Dictionary<string, string> _pinyin;
    private static ConcurrentDictionary<string, Pinyin> _text2Pinyin = new ConcurrentDictionary<string, Pinyin>();

    public static Pinyin ToPinyin(string text)
    {
      if (string.IsNullOrWhiteSpace(text))
        return new Pinyin(string.Empty, string.Empty, new List<int>());
      Pinyin pinyin1;
      if (PinyinUtils._text2Pinyin.TryGetValue(text, out pinyin1))
        return pinyin1;
      if (PinyinUtils._pinyin == null)
        PinyinUtils.InitData();
      text = EmojiData.MatchMultiple.Replace(text, "");
      List<int> indexes = new List<int>();
      if (PinyinUtils._pinyin == null || PinyinUtils._pinyin.Count <= 0)
        return new Pinyin(string.Empty, string.Empty, new List<int>());
      StringBuilder stringBuilder1 = new StringBuilder();
      StringBuilder stringBuilder2 = new StringBuilder();
      int num = 0;
      foreach (char ch in text)
      {
        indexes.Add(num);
        if (PinyinUtils._pinyin.ContainsKey(ch.ToString()))
        {
          string str = PinyinUtils._pinyin[ch.ToString()];
          stringBuilder1.Append(str);
          stringBuilder2.Append(str.Substring(0, 1));
          num += str.Length;
        }
        else
        {
          stringBuilder1.Append(ch);
          stringBuilder2.Append(ch);
          ++num;
        }
      }
      Pinyin pinyin2 = new Pinyin(stringBuilder1.ToString(), stringBuilder2.ToString(), indexes);
      PinyinUtils._text2Pinyin.TryAdd(text, pinyin2);
      return pinyin2;
    }

    private static void InitData()
    {
      string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      if (directoryName == null)
        return;
      string path = Path.Combine(directoryName, "pinyin.txt");
      if (!File.Exists(path))
        return;
      PinyinUtils._pinyin = new Dictionary<string, string>();
      try
      {
        foreach (string readAllLine in File.ReadAllLines(path))
        {
          char[] chArray = new char[1]{ ' ' };
          string[] strArray = readAllLine.Split(chArray);
          if (strArray.Length == 2 && !PinyinUtils._pinyin.ContainsKey(strArray[0]))
            PinyinUtils._pinyin.Add(strArray[0], strArray[1]);
        }
      }
      catch
      {
      }
    }
  }
}
