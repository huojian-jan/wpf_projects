// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.UriNotifier
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Notifier
{
  internal class UriNotifier
  {
    public static event EventHandler<UriModel> Uri;

    public static void NotifyUri(string path, string param)
    {
      UriModel e = new UriModel()
      {
        Path = path,
        Param = param,
        Parmas = new Dictionary<string, string>()
      };
      try
      {
        string str1 = param;
        char[] chArray1 = new char[1]{ '&' };
        foreach (string str2 in str1.Split(chArray1))
        {
          char[] chArray2 = new char[1]{ '=' };
          string[] strArray = str2.Split(chArray2);
          string key = strArray[0];
          string str3 = strArray[1];
          e.Parmas.Add(key, str3);
        }
      }
      catch (Exception ex)
      {
      }
      EventHandler<UriModel> uri = UriNotifier.Uri;
      if (uri == null)
        return;
      uri((object) null, e);
    }
  }
}
