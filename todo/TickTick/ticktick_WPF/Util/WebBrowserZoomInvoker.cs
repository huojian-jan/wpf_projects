// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.WebBrowserZoomInvoker
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Navigation;

#nullable disable
namespace ticktick_WPF.Util
{
  public class WebBrowserZoomInvoker
  {
    private const int LOGPIXELSX = 88;
    private const int LOGPIXELSY = 90;
    private static readonly int OLECMDEXECOPT_DODEFAULT = 0;
    private static readonly int OLECMDID_OPTICAL_ZOOM = 63;
    public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static readonly string EmptyHTMLFilePath = Path.Combine(WebBrowserZoomInvoker.AppDataPath, "Empty.html");
    public static readonly string EmptyHTML = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"><style>body {overflow-y: hidden;}v\\:* {behavior:url(#default#VML);}o\\:* {behavior:url(#default#VML);}w\\:* {behavior:url(#default#VML);}.shape {behavior:url(#default#VML);}</style></head><body></body></html>";

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr ptr);

    [DllImport("user32.dll")]
    public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateDC(
      string lpszDriver,
      string lpszDevice,
      string lpszOutput,
      long lpInitData);

    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware();

    private static PointF GetCurrentDIPScale()
    {
      PointF currentDipScale = new PointF(1f, 1f);
      try
      {
        WebBrowserZoomInvoker.SetProcessDPIAware();
        IntPtr dc = WebBrowserZoomInvoker.GetDC(IntPtr.Zero);
        int deviceCaps1 = WebBrowserZoomInvoker.GetDeviceCaps(dc, 88);
        int deviceCaps2 = WebBrowserZoomInvoker.GetDeviceCaps(dc, 90);
        currentDipScale.X = (float) deviceCaps1 / 96f;
        currentDipScale.Y = (float) deviceCaps2 / 96f;
        WebBrowserZoomInvoker.ReleaseDC(IntPtr.Zero, dc);
        return currentDipScale;
      }
      catch (Exception ex)
      {
      }
      return currentDipScale;
    }

    private static void SetZoom(System.Windows.Controls.WebBrowser webbrowser, int zoom)
    {
      try
      {
        if (webbrowser == null)
          return;
        foreach (FieldInfo field in webbrowser.GetType().GetFields())
          ;
        FieldInfo field1 = webbrowser.GetType().GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
        if (!((FieldInfo) null != field1))
          return;
        object target = field1.GetValue((object) webbrowser);
        if (target == null)
          return;
        object[] args = new object[4]
        {
          (object) WebBrowserZoomInvoker.OLECMDID_OPTICAL_ZOOM,
          (object) WebBrowserZoomInvoker.OLECMDEXECOPT_DODEFAULT,
          (object) zoom,
          (object) IntPtr.Zero
        };
        target.GetType().InvokeMember("ExecWB", BindingFlags.InvokeMethod, (Binder) null, target, args);
      }
      catch (Exception ex)
      {
      }
    }

    private static void SetZoom(System.Windows.Forms.WebBrowser webbrowser, int zoom)
    {
      try
      {
        if (webbrowser == null)
          return;
        foreach (FieldInfo field in webbrowser.GetType().GetFields())
          ;
        FieldInfo field1 = webbrowser.GetType().GetField("axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
        if (!((FieldInfo) null != field1))
          return;
        object target = field1.GetValue((object) webbrowser);
        if (target == null)
          return;
        object[] args = new object[4]
        {
          (object) WebBrowserZoomInvoker.OLECMDID_OPTICAL_ZOOM,
          (object) WebBrowserZoomInvoker.OLECMDEXECOPT_DODEFAULT,
          (object) zoom,
          (object) IntPtr.Zero
        };
        target.GetType().InvokeMember("ExecWB", BindingFlags.InvokeMethod, (Binder) null, target, args);
      }
      catch (Exception ex)
      {
      }
    }

    public static void AddZoomInvoker(System.Windows.Controls.WebBrowser browser)
    {
      if (!WebBrowserZoomInvoker.NeedZoom())
        return;
      WebBrowserZoomInvoker.InitEmptyHTML();
      browser.LoadCompleted += new LoadCompletedEventHandler(WebBrowserZoomInvoker.browser_LoadCompleted);
    }

    public static void AddZoomInvoker(System.Windows.Forms.WebBrowser browser)
    {
      if (!WebBrowserZoomInvoker.NeedZoom())
        return;
      WebBrowserZoomInvoker.InitEmptyHTML();
      browser.Navigated += new WebBrowserNavigatedEventHandler(WebBrowserZoomInvoker.browser_LoadCompleted);
      browser.Navigate(new Uri(WebBrowserZoomInvoker.EmptyHTMLFilePath));
    }

    private static void browser_LoadCompleted(object sender, NavigationEventArgs e)
    {
      if (!(sender is System.Windows.Controls.WebBrowser webbrowser))
        return;
      webbrowser.LoadCompleted -= new LoadCompletedEventHandler(WebBrowserZoomInvoker.browser_LoadCompleted);
      PointF currentDipScale = WebBrowserZoomInvoker.GetCurrentDIPScale();
      if (100 == (int) ((double) currentDipScale.X * 100.0))
        return;
      WebBrowserZoomInvoker.SetZoom(webbrowser, (int) ((double) currentDipScale.X * (double) currentDipScale.Y * 100.0));
    }

    private static void browser_LoadCompleted(object sender, WebBrowserNavigatedEventArgs e)
    {
      if (!(sender is System.Windows.Forms.WebBrowser webbrowser))
        return;
      webbrowser.Navigated -= new WebBrowserNavigatedEventHandler(WebBrowserZoomInvoker.browser_LoadCompleted);
      PointF currentDipScale = WebBrowserZoomInvoker.GetCurrentDIPScale();
      if (100 == (int) ((double) currentDipScale.X * 100.0))
        return;
      WebBrowserZoomInvoker.SetZoom(webbrowser, (int) ((double) currentDipScale.X * (double) currentDipScale.Y * 100.0));
    }

    private static void InitEmptyHTML()
    {
      try
      {
        if (File.Exists(WebBrowserZoomInvoker.EmptyHTMLFilePath))
          return;
        using (FileStream fileStream = new FileStream(WebBrowserZoomInvoker.EmptyHTMLFilePath, FileMode.Create))
          fileStream.Write(Encoding.UTF8.GetBytes(WebBrowserZoomInvoker.EmptyHTML), 0, Encoding.UTF8.GetByteCount(WebBrowserZoomInvoker.EmptyHTML));
        File.SetAttributes(WebBrowserZoomInvoker.EmptyHTMLFilePath, FileAttributes.ReadOnly | FileAttributes.Hidden);
      }
      catch (Exception ex)
      {
      }
    }

    private static bool NeedZoom()
    {
      return 100 != (int) ((double) WebBrowserZoomInvoker.GetCurrentDIPScale().X * 100.0);
    }
  }
}
