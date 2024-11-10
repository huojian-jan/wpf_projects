// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.DateParser
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public static class DateParser
  {
    public static bool CanParse = true;

    static DateParser()
    {
      if (App.Ci.DateTimeFormat.ShortDatePattern.IndexOf("d", StringComparison.Ordinal) >= App.Ci.DateTimeFormat.ShortDatePattern.IndexOf("M", StringComparison.Ordinal))
        return;
      try
      {
        Parser.setIsUsOrUkDateFormat(false);
      }
      catch (Exception ex)
      {
      }
    }

    public static IPaserDueDate Parse(string content, DateTime? baseDate)
    {
      if (ticktick_WPF.Util.DateParser.DateParser.CanParse)
      {
        try
        {
          DayOfWeek result;
          Enum.TryParse<DayOfWeek>(LocalSettings.Settings.WeekStartFrom, out result);
          Parser.setStartDay((int) (result + 1));
          Parser.setIs24DateFormat(LocalSettings.Settings.TimeFormat == "24Hour");
          string s = baseDate?.ToString("yyyy-MM-dd'T'HH:mm:ss") ?? string.Empty;
          IntPtr coTaskMemAnsi1 = Marshal.StringToCoTaskMemAnsi(content);
          IntPtr coTaskMemAnsi2 = Marshal.StringToCoTaskMemAnsi(s);
          string name = Thread.CurrentThread.CurrentCulture.Name;
          IntPtr coTaskMemAnsi3 = Marshal.StringToCoTaskMemAnsi(name);
          IntPtr ptr = Parser.parse(coTaskMemAnsi1, Encoding.Default.GetByteCount(content), coTaskMemAnsi2, Encoding.Default.GetByteCount(s), UserDao.IsPro(), coTaskMemAnsi3, Encoding.Default.GetByteCount(name));
          string stringAnsi = Marshal.PtrToStringAnsi(ptr);
          ParseDueDate parseDueDate = stringAnsi == null ? (ParseDueDate) null : JsonConvert.DeserializeObject<ParseDueDate>(stringAnsi);
          if (parseDueDate != null)
            parseDueDate.Text = content;
          Marshal.FreeCoTaskMem(coTaskMemAnsi1);
          Marshal.FreeCoTaskMem(coTaskMemAnsi2);
          Marshal.FreeCoTaskMem(coTaskMemAnsi3);
          Marshal.FreeCoTaskMem(ptr);
          return (IPaserDueDate) parseDueDate;
        }
        catch (Exception ex)
        {
          if (ex.Message.Contains("DllNotFoundException"))
            ticktick_WPF.Util.DateParser.DateParser.CanParse = false;
        }
      }
      return (IPaserDueDate) OldDateParser.Parse(content);
    }

    private static string StringToUTF8(string str)
    {
      try
      {
        return Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(str)));
      }
      catch (Exception ex)
      {
        return str;
      }
    }
  }
}
