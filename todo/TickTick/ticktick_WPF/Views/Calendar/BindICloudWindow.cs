// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.BindICloudWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.MarkDown;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class BindICloudWindow : Window, IOkCancelWindow, IComponentConnector
  {
    private SubscribeCalendar _parentCal;
    private bool _isReauthorize;
    private readonly BindCalendarAccountModel _account;
    internal Grid GetSepcificPassWordGrid;
    internal Run iCloudDesc;
    internal Run Period;
    internal Grid AccountPanel;
    internal Run UserNameRun;
    internal TextBox UsernameInput;
    internal TextBlock UsernameInputPlaceholder;
    internal PasswordBox PasswordInput;
    internal TextBlock PasswordInputPlaceholder;
    internal TextBox DescInput;
    internal TextBlock ErrorText;
    internal Run ErrorRun;
    internal Run OptionRun;
    internal Run PeriodRun;
    internal Button CancelButton;
    internal Button SaveButton;
    private bool _contentLoaded;

    public BindICloudWindow(SubscribeCalendar parent, BindCalendarAccountModel account = null)
    {
      this.InitializeComponent();
      this._parentCal = parent;
      this._account = account;
      this._isReauthorize = account != null;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      string text = Utils.GetString("iCloudDesc");
      if (text.Contains("{0}"))
      {
        List<string> stringList = text.SplitByStr("{0}");
        this.iCloudDesc.Text = stringList[0];
        this.Period.Text = stringList[1];
      }
      if (!this._isReauthorize)
        return;
      this.Title = string.Format(Utils.GetString("ReauthorizeAccount"), (object) "iCloud");
      this.ShowAccountPanel();
      this.UsernameInput.Text = this._account.Account;
      this.DescInput.Text = this._account.Description ?? string.Empty;
      this.PasswordInput.Focus();
    }

    private void ShowAccountPanel()
    {
      this.AccountPanel.Visibility = Visibility.Visible;
      this.SaveButton.Visibility = Visibility.Visible;
      this.GetSepcificPassWordGrid.Visibility = Visibility.Collapsed;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.ErrorRun.Text = string.Empty;
      this.PeriodRun.Text = string.Empty;
      this.OptionRun.Text = string.Empty;
      this.SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(this.UsernameInput.Text) && !string.IsNullOrEmpty(this.PasswordInput.Password);
    }

    private void ClearNameText(object sender, MouseButtonEventArgs e)
    {
      this.UsernameInput.Text = "";
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      if (sender is PasswordBox passwordBox)
        this.PasswordInputPlaceholder.Visibility = passwordBox.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
      this.SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(this.UsernameInput.Text) && !string.IsNullOrEmpty(this.PasswordInput.Password);
    }

    private void ClearDomainText(object sender, MouseButtonEventArgs e) => this.DescInput.Text = "";

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.Ok();

    private void OnHavePasswordClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowAccountPanel();
    }

    public void OnCancel() => this.Close();

    public void Ok()
    {
      if (string.IsNullOrWhiteSpace(this.UsernameInput.Text) || string.IsNullOrEmpty(this.PasswordInput.Password))
        return;
      this.ErrorRun.Text = string.Empty;
      this.PeriodRun.Text = string.Empty;
      this.OptionRun.Text = string.Empty;
      this.SetContentIsEnabled(false);
      if (this._isReauthorize)
        this.UpdateAccount();
      else
        this.BindAccount();
    }

    private async Task UpdateAccount()
    {
      BindICloudWindow bindIcloudWindow = this;
      BindAccountModel model = new BindAccountModel()
      {
        username = bindIcloudWindow.UsernameInput.Text.Trim(),
        password = bindIcloudWindow.PasswordInput.Password,
        desc = bindIcloudWindow.DescInput.Text.Trim()
      };
      string str = await Communicator.UpdateICloudAccount(bindIcloudWindow._account.Id, model);
      if (str.Contains("caldav_bind_faild") || str.Contains("false"))
      {
        bindIcloudWindow.ErrorRun.Text = Utils.GetString("CalendarBindFaild");
        bindIcloudWindow.PeriodRun.Text = string.Empty;
        bindIcloudWindow.OptionRun.Text = string.Empty;
        bindIcloudWindow.SetContentIsEnabled(true);
      }
      else
      {
        await BindCalendarAccountDao.DeleteCalendarsByAccountId(bindIcloudWindow._account?.Id);
        await CalendarService.DeleteEvent(bindIcloudWindow._account?.Id);
        bindIcloudWindow.PullEvent(bindIcloudWindow._account?.Id);
        bindIcloudWindow.SetContentIsEnabled(true);
        if (bindIcloudWindow._parentCal != null)
          await bindIcloudWindow._parentCal.LoadData(false);
        bindIcloudWindow.Close();
      }
    }

    private async Task PullEvent(string id)
    {
      await CalendarService.PullAccountCalendarsAndEvents(accountId: id);
      CalendarEventChangeNotifier.NotifyRemoteChanged();
    }

    private async Task BindAccount()
    {
      BindICloudWindow bindIcloudWindow = this;
      try
      {
        BindCalendarAccountModel calendar = await Communicator.BindICloudAccount(new BindAccountModel()
        {
          username = bindIcloudWindow.UsernameInput.Text.Trim(),
          password = bindIcloudWindow.PasswordInput.Password,
          desc = bindIcloudWindow.DescInput.Text.Trim()
        });
        if (calendar == null)
          throw new CustomException.CalendarBindException("calendar_bind_faild");
        await BindCalendarAccountDao.SaveBindCalendarAccount(calendar);
        if (bindIcloudWindow._parentCal != null)
          await bindIcloudWindow._parentCal.LoadData(false);
        ListViewContainer.ReloadProjectData();
        CalendarService.PullBindCalEvents(calendar.Id, new DateTime?(), new DateTime?(), Constants.BindAccountType.CalDAV);
        bindIcloudWindow.SetContentIsEnabled(true);
        bindIcloudWindow.Close();
        calendar = (BindCalendarAccountModel) null;
      }
      catch (CustomException.CalendarBindException ex)
      {
        bindIcloudWindow.OptionRun.Text = string.Empty;
        bindIcloudWindow.PeriodRun.Text = string.Empty;
        if (ex.Message == "calendar_bind_duplicate")
          bindIcloudWindow.ErrorRun.Text = Utils.GetString("CalendarBindDuplicate");
        else if (ex.Message == "calendar_bind_faild")
        {
          string text = Utils.GetString("iCloudPasswordError");
          if (!text.Contains("{0}"))
            return;
          List<string> stringList = text.SplitByStr("{0}");
          bindIcloudWindow.ErrorRun.Text = stringList[0];
          bindIcloudWindow.PeriodRun.Text = stringList[1];
          bindIcloudWindow.OptionRun.Text = Utils.GetString("AppSepcificPassword");
        }
        else
          bindIcloudWindow.ErrorRun.Text = !(ex.Message == "calendar_bind_exceed") ? ex.Message : string.Format(Utils.GetString("CalendarBindExceed"), (object) "iCloud");
      }
      finally
      {
        bindIcloudWindow.SetContentIsEnabled(true);
      }
    }

    private void SetContentIsEnabled(bool isEnabled)
    {
      if (isEnabled)
      {
        this.SaveButton.Content = (object) Utils.GetString("OK");
        this.SaveButton.IsEnabled = true;
        this.SaveButton.Opacity = 1.0;
        this.UsernameInput.IsEnabled = true;
        this.PasswordInput.IsEnabled = true;
        this.DescInput.IsEnabled = true;
        this.CancelButton.IsEnabled = true;
      }
      else
      {
        this.SaveButton.Content = (object) Utils.GetString("Subscribing");
        this.SaveButton.IsEnabled = false;
        this.SaveButton.Opacity = 0.699999988079071;
        this.CancelButton.IsEnabled = false;
        this.UsernameInput.IsEnabled = false;
        this.PasswordInput.IsEnabled = false;
        this.DescInput.IsEnabled = false;
      }
    }

    private void GetSepcificPassword(object sender, MouseButtonEventArgs e)
    {
      Utils.TryProcessStartUrl("https://appleid.apple.com");
    }

    private void OpenGuide(object sender, MouseButtonEventArgs e) => this.OpenGuide();

    private async Task OpenGuide()
    {
      string url = await Communicator.GetICloudGuideUrl();
      if (string.IsNullOrEmpty(url))
        url = Utils.IsZhCn() ? "https://support.apple.com/zh-cn/HT204397" : "https://support.apple.com/en-us/HT204397";
      Utils.TryProcessStartUrl(url);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/bindicloudwindow.xaml", UriKind.Relative));
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
          this.GetSepcificPassWordGrid = (Grid) target;
          break;
        case 3:
          this.iCloudDesc = (Run) target;
          break;
        case 4:
          ((ContentElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OpenGuide);
          break;
        case 5:
          this.Period = (Run) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnHavePasswordClick);
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.GetSepcificPassword);
          break;
        case 8:
          this.AccountPanel = (Grid) target;
          break;
        case 9:
          this.UserNameRun = (Run) target;
          break;
        case 10:
          this.UsernameInput = (TextBox) target;
          this.UsernameInput.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        case 11:
          this.UsernameInputPlaceholder = (TextBlock) target;
          break;
        case 12:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearNameText);
          break;
        case 13:
          this.PasswordInput = (PasswordBox) target;
          this.PasswordInput.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case 14:
          this.PasswordInputPlaceholder = (TextBlock) target;
          break;
        case 15:
          this.DescInput = (TextBox) target;
          break;
        case 16:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearDomainText);
          break;
        case 17:
          this.ErrorText = (TextBlock) target;
          break;
        case 18:
          this.ErrorRun = (Run) target;
          break;
        case 19:
          this.OptionRun = (Run) target;
          this.OptionRun.MouseLeftButtonUp += new MouseButtonEventHandler(this.GetSepcificPassword);
          break;
        case 20:
          this.PeriodRun = (Run) target;
          break;
        case 21:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 22:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
