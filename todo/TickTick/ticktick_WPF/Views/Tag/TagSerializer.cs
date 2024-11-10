// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagSerializer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public static class TagSerializer
  {
    public static List<string> ToTags(string content)
    {
      if (!string.IsNullOrEmpty(content))
      {
        try
        {
          List<string> source = ((JToken) JsonConvert.DeserializeObject(content)).ToObject<List<string>>();
          if (source != null)
          {
            if (source.Count > 0)
              return source.Select<string, string>((Func<string, string>) (tag => tag.Replace("#", string.Empty).Replace("＃", string.Empty).ToLower().Trim())).ToList<string>();
          }
        }
        catch (Exception ex)
        {
          return new List<string>();
        }
      }
      return new List<string>();
    }

    public static string ToJsonContent(List<string> tags)
    {
      if (tags == null)
        return string.Empty;
      List<string> stringList = new List<string>();
      foreach (string tag in tags)
      {
        if (!string.IsNullOrEmpty(tag) && !stringList.Contains(tag))
          stringList.Add(tag.Replace("#", string.Empty).Replace("＃", string.Empty).ToLower().Trim());
      }
      return JsonConvert.SerializeObject((object) stringList);
    }
  }
}
