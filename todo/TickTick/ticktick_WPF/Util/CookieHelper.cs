// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.CookieHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

#nullable disable
namespace ticktick_WPF.Util
{
  public class CookieHelper
  {
    private const int InternetCookieHttponly = 8192;
    private const int INTERNET_OPTION_END_BROWSER_SESSION = 42;

    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetGetCookieEx(
      string url,
      string cookieName,
      StringBuilder cookieData,
      ref int size,
      int dwFlags,
      IntPtr lpReserved);

    public static CookieContainer GetUriCookieContainer(Uri uri)
    {
      CookieContainer uriCookieContainer = (CookieContainer) null;
      int size = 131072;
      StringBuilder cookieData = new StringBuilder(size);
      if (!CookieHelper.InternetGetCookieEx(uri.ToString(), (string) null, cookieData, ref size, 8192, IntPtr.Zero))
      {
        if (size < 0)
          return (CookieContainer) null;
        cookieData = new StringBuilder(size);
        if (!CookieHelper.InternetGetCookieEx(uri.ToString(), (string) null, cookieData, ref size, 8192, IntPtr.Zero))
          return (CookieContainer) null;
      }
      if (cookieData.Length > 0)
      {
        uriCookieContainer = new CookieContainer();
        uriCookieContainer.SetCookies(uri, cookieData.ToString().Replace(';', ','));
      }
      return uriCookieContainer;
    }

    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(
      IntPtr hInternet,
      int dwOption,
      IntPtr lpBuffer,
      int lpdwBufferLength);

    public static void ClearCookie()
    {
      CookieHelper.InternetSetOption(IntPtr.Zero, 42, IntPtr.Zero, 0);
    }

    [DllImport("wininet.dll", EntryPoint = "InternetSetCookieExW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool InternetSetCookieEx(
      [In] string lpszUrl,
      [In] string lpszCookieName,
      [In] string lpszCookieData,
      uint dwFlags,
      [In] IntPtr dwReserved);

    public static void SetCookie(string uri, string cookie)
    {
      string lpszUrl = uri;
      string str1 = cookie;
      DateTime dateTime = DateTime.Now;
      dateTime = dateTime.AddDays(20.0);
      string str2 = dateTime.ToString("ddd, dd-MMM-yyyy 00:00:00 GMT", (IFormatProvider) CultureInfo.CreateSpecificCulture("en-US"));
      string lpszCookieData = str1 + "; expires = " + str2;
      IntPtr zero = IntPtr.Zero;
      CookieHelper.InternetSetCookieEx(lpszUrl, (string) null, lpszCookieData, 0U, zero);
    }

    public static void SetCookieOutDate(string uri, string cookie)
    {
      string lpszUrl = uri;
      string str1 = cookie;
      DateTime dateTime = DateTime.Now;
      dateTime = dateTime.AddDays(-1.0);
      string str2 = dateTime.ToString("ddd, dd-MMM-yyyy 00:00:00 GMT", (IFormatProvider) CultureInfo.CreateSpecificCulture("en-US"));
      string lpszCookieData = str1 + "; expires = " + str2;
      IntPtr zero = IntPtr.Zero;
      CookieHelper.InternetSetCookieEx(lpszUrl, (string) null, lpszCookieData, 0U, zero);
    }
  }
}
