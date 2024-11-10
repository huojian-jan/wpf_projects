// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.JavaScriptTicketOptions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Runtime.InteropServices;
using System.Windows;
using ticktick_WPF.Util;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views
{
  [ComVisible(true)]
  public class JavaScriptTicketOptions
  {
    private Window _instance;

    public JavaScriptTicketOptions(Window instance) => this._instance = instance;

    public void toastForTicketWithMessage(string message) => Utils.Toast(message);

    public void uploadLogsForTicketWithID(string ticketId, string tt)
    {
      Communicator.UploadLogs(TicketLogUtils.GetRecentLogs(), ticketId, UserManager.GetToken());
    }

    public string getXDeviceInfoForTicket() => Utils.GetDeviceInfo();

    public void setDefaultMatrixRule(string ruleType)
    {
      if (!(this._instance is NavigateWebBrowserWindow instance))
        return;
      instance.DoAction(true, ruleType);
    }

    public void Dispose() => this._instance = (Window) null;
  }
}
