// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.WebBrowserHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;

#nullable disable
namespace ticktick_WPF.Util
{
  public class WebBrowserHelper
  {
    public static void SetErrorSilent(WebBrowser browser)
    {
      browser.Navigating += (NavigatingCancelEventHandler) ((s, e) =>
      {
        FieldInfo field = typeof (WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == (FieldInfo) null)
          return;
        object target = field.GetValue((object) browser);
        target?.GetType().InvokeMember("Silent", BindingFlags.SetProperty, (Binder) null, target, new object[1]
        {
          (object) true
        });
      });
    }
  }
}
