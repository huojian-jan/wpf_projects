// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.MyNotificationActivator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  [ClassInterface(ClassInterfaceType.None)]
  [ComSourceInterfaces(typeof (NotificationActivator.INotificationActivationCallback))]
  [Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3")]
  [ComVisible(true)]
  public class MyNotificationActivator : NotificationActivator
  {
    public override void OnActivated(
      string invokedArgs,
      NotificationUserInput userInput,
      string appUserModelId)
    {
      UtilLog.Info(invokedArgs + "  " + appUserModelId);
    }
  }
}
