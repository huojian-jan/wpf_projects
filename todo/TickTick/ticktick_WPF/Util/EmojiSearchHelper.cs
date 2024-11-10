// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.EmojiSearchHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ticktick_WPF.Views.MarkDown;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class EmojiSearchHelper
  {
    private static readonly Dictionary<string, EmojiKeyLists> EmojiSearchDict = new Dictionary<string, EmojiKeyLists>();
    private static Dictionary<string, string> NumEmjDict = new Dictionary<string, string>()
    {
      {
        "0⃣",
        "0️⃣"
      },
      {
        "1⃣",
        "1️⃣"
      },
      {
        "2⃣",
        "2️⃣"
      },
      {
        "3⃣",
        "3️⃣"
      },
      {
        "4⃣",
        "4️⃣"
      },
      {
        "5⃣",
        "5️⃣"
      },
      {
        "6⃣",
        "6️⃣"
      },
      {
        "7⃣",
        "7️⃣"
      },
      {
        "8⃣",
        "8️⃣"
      },
      {
        "9⃣",
        "9️⃣"
      }
    };

    static EmojiSearchHelper() => EmojiSearchHelper.GetEmojiSearchDict();

    public static void Init()
    {
    }

    private static void GetEmojiSearchDict()
    {
      EmojiSearchHelper.LoadCiKeys();
      EmojiSearchHelper.LoadSearchDict(AppPaths.LocaleDir + "\\en-US\\emojiSearch.txt", "EnKeys");
      EmojiSearchHelper.LoadSearchDict(AppPaths.LocaleDir + "\\zh-CN\\emojiSearch.txt", "CnKeys");
    }

    private static async Task LoadCiKeys()
    {
      string filePath;
      if (Utils.IsCn())
      {
        filePath = (string) null;
      }
      else
      {
        string str = App.Ci.ToString();
        filePath = AppPaths.DataDir + "Locales\\" + str + "\\emojiSearch.txt";
        int num = await IOUtils.CheckResourceExist(AppPaths.DataDir + "Locales\\" + str + "\\", "emojiSearch.txt", "https://" + BaseUrl.PullDomain + "/emoji/v0/" + str.Substring(0, 2) + ".txt") ? 1 : 0;
        if (!File.Exists(filePath))
        {
          filePath = (string) null;
        }
        else
        {
          EmojiSearchHelper.LoadSearchDict(filePath, "CiKeys");
          filePath = (string) null;
        }
      }
    }

    private static void LoadSearchDict(string file, string listName)
    {
      if (!File.Exists(file))
        return;
      foreach (string text in File.ReadLines(file).ToList<string>())
      {
        List<string> stringList = text.SplitByStr("=>");
        if (stringList.Count == 2)
        {
          string key = stringList[0];
          if (EmojiSearchHelper.NumEmjDict.ContainsKey(key))
            key = EmojiSearchHelper.NumEmjDict[key];
          if (EmojiSearchHelper.EmojiSearchDict.ContainsKey(key))
          {
            EmojiSearchHelper.EmojiSearchDict[key].AddToList(stringList[1], listName);
          }
          else
          {
            EmojiKeyLists emojiKeyLists = new EmojiKeyLists();
            emojiKeyLists.AddToList(stringList[1], listName);
            EmojiSearchHelper.EmojiSearchDict.Add(key, emojiKeyLists);
          }
        }
      }
    }

    public static List<string> GetMatchedEmoji(string searchKey)
    {
      List<string> list = ((IEnumerable<string>) searchKey.Split(' ')).ToList<string>();
      List<string> matchedEmoji = new List<string>();
      foreach (KeyValuePair<string, EmojiKeyLists> keyValuePair in EmojiSearchHelper.EmojiSearchDict)
      {
        Match match = EmojiData.MatchOne.Match(keyValuePair.Key);
        if (match.Success && !(match.Groups[0].Value != keyValuePair.Key) && keyValuePair.Value.CheckKeys(list))
          matchedEmoji.Add(keyValuePair.Key);
      }
      return matchedEmoji;
    }
  }
}
