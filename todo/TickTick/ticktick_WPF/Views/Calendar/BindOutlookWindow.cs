// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.BindOutlookWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.MainListView;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class BindOutlookWindow : Window, IComponentConnector
  {
    private SubscribeCalendar _parent;
    private string _originAccount;
    internal WebBrowser WebBrowser;
    private bool _contentLoaded;

    public BindOutlookWindow(SubscribeCalendar parent = null)
    {
      this.InitializeComponent();
      WinInetHelper.SupressCookiePersist();
      this._parent = parent;
      this.WebBrowser.Navigating += (NavigatingCancelEventHandler) ((o, ea) =>
      {
        string str = ea.Uri.ToString();
        if (!str.StartsWith("https://" + BaseUrl.GetDomain() + "/calendar/bind/outlook?code=") || !str.EndsWith("&state=outlook"))
          return;
        this.BindOutlook(str.Replace("https://" + BaseUrl.GetDomain() + "/calendar/bind/outlook?code=", "").Replace("&state=outlook", ""));
        this.Close();
      });
      this.WebBrowser.Navigate(BaseUrl.GetApiDomain() + "/pub/api/v2/calendar/bind/outlook");
    }

    protected override void OnClosed(EventArgs e) => base.OnClosed(e);

    private async void BindOutlook(string code)
    {
      BindCalendarAccountModel calendar = await Communicator.SubscribeOutlook(code);
      if (calendar != null)
      {
        if (!string.IsNullOrEmpty(calendar.Id))
          await BindCalendarAccountDao.DeleteBindAccountById(calendar.Id);
        if (!string.IsNullOrEmpty(this._originAccount))
          await BindCalendarAccountDao.DeleteBindAccountById(this._originAccount);
        await SubscribeCalendarHelper.SaveBindAccount(calendar);
        this._parent?.LoadData(false);
        ListViewContainer.ReloadProjectData();
        calendar = (BindCalendarAccountModel) null;
      }
      else
      {
        Utils.Toast(Utils.GetString("VerificationFailedHaveTry"));
        calendar = (BindCalendarAccountModel) null;
      }
    }

    public void SetOriginAccount(string accountId) => this._originAccount = accountId;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/bindoutlookwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.WebBrowser = (WebBrowser) target;
      else
        this._contentLoaded = true;
    }
  }
}
