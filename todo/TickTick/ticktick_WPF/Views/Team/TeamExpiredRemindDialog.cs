// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Team.TeamExpiredRemindDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Team
{
  public class TeamExpiredRemindDialog : Window, IComponentConnector
  {
    private List<TeamModel> _teams;
    internal TextBlock TitleText;
    internal TextBlock RemindText;
    internal Button UploadButton;
    internal Button CloseButton;
    private bool _contentLoaded;

    public TeamExpiredRemindDialog(List<TeamModel> teams)
    {
      this._teams = teams;
      this.InitializeComponent();
      this.Closing += (CancelEventHandler) ((sender, e) => this.Owner?.Activate());
    }

    private async void OnUploadClick(object sender, RoutedEventArgs e)
    {
      TeamExpiredRemindDialog expiredRemindDialog = this;
      if (expiredRemindDialog._teams.Count == 1)
      {
        string signOnToken = await Communicator.GetSignOnToken();
        Utils.TryProcessStartUrl(string.Format(BaseUrl.GetApiDomain() + "/sign/autoSignOn?token={0}&dest={1}", (object) signOnToken, (object) string.Format(Utils.GetString("RenewTeamUrl"), (object) expiredRemindDialog._teams[0].id)));
      }
      expiredRemindDialog.Close();
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      if (this._teams == null)
        return;
      if (this._teams.Count == 1)
      {
        this.TitleText.Text = string.Format(Utils.GetString("TeamOutDate"), (object) this._teams[0].name);
        this.RemindText.Text = string.Format(Utils.GetString("TeamOutDateText"), (object) this._teams[0].name);
      }
      if (this._teams.Count <= 1)
        return;
      string str = "\"" + this._teams[0].name + "\"";
      for (int index = 1; index < this._teams.Count; ++index)
        str = str + ",\"" + this._teams[index].name + "\"";
      this.TitleText.Text = string.Format(Utils.GetString("TeamsOutDate"), (object) this._teams.Count);
      this.RemindText.Text = string.Format(Utils.GetString("TeamsOutDateText"), (object) str);
      this.CloseButton.Visibility = Visibility.Visible;
      this.UploadButton.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/team/teamexpiredreminddialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnWindowLoaded);
          break;
        case 2:
          this.TitleText = (TextBlock) target;
          break;
        case 3:
          this.RemindText = (TextBlock) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnCloseClick);
          break;
        case 5:
          this.UploadButton = (Button) target;
          this.UploadButton.Click += new RoutedEventHandler(this.OnUploadClick);
          break;
        case 6:
          this.CloseButton = (Button) target;
          this.CloseButton.Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
