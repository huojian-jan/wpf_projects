// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.BindAccountWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class BindAccountWindow : Window, IOkCancelWindow, IComponentConnector
  {
    private readonly SubscribeCalendar _parent;
    private readonly BindCalendarAccountModel _account;
    private string _domain;
    private string _username;
    private string _description;
    private readonly bool _isReauthorize;
    private readonly Constants.BindAccountType _type;
    private bool _needDomain;
    internal Run UserNameRun;
    internal TextBox UsernameInput;
    internal TextBlock UsernameInputPlaceholder;
    internal PasswordBox PasswordInput;
    internal TextBlock PasswordInputPlaceholder;
    internal Run DomainStarRun;
    internal TextBox DomainInput;
    internal TextBlock ServerInputPlaceholder;
    internal TextBox DescriptionInput;
    internal TextBlock DescriptionInputPlaceholder;
    internal TextBlock ErrorText;
    internal TextBlock OperationGuide;
    internal Button CancelButton;
    internal Button SaveButton;
    private bool _contentLoaded;

    public BindAccountWindow(SubscribeCalendar parent, Constants.BindAccountType type)
    {
      this.DataContext = (object) this;
      this.InitializeComponent();
      this._parent = parent;
      this._type = type;
      this.OperationGuide.Visibility = type == Constants.BindAccountType.CalDAV ? Visibility.Visible : Visibility.Collapsed;
    }

    public BindAccountWindow(
      BindCalendarAccountModel account,
      bool isReauthorize = true,
      SubscribeCalendar parent = null)
    {
      this.DataContext = (object) this;
      this.InitializeComponent();
      switch (account.Kind)
      {
        case "caldav":
          this._type = Constants.BindAccountType.CalDAV;
          break;
        case "exchange":
          this._type = Constants.BindAccountType.Exchange;
          break;
      }
      this.Title = string.Format(Utils.GetString("ReauthorizeAccount"), (object) this._type.ToString());
      this._account = account;
      this._parent = parent;
      this._isReauthorize = isReauthorize;
      this.DomainInput.Text = account.Domain;
      this.UsernameInput.Text = account.Username;
      this.DescriptionInput.Text = account.Description;
      this.OperationGuide.Visibility = account.Kind == "caldav" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      switch (this._type)
      {
        case Constants.BindAccountType.CalDAV:
          this.Title = Utils.GetString("AddCalDav");
          this._needDomain = true;
          break;
        case Constants.BindAccountType.Exchange:
          this.Title = Utils.GetString("AddExchange");
          this.UserNameRun.Text = Utils.GetString("Account");
          this.DomainStarRun.Text = "";
          this.DescriptionInputPlaceholder.Text = Utils.GetString("MyExchangeAccount");
          this._needDomain = false;
          break;
      }
      if (!this._isReauthorize)
        return;
      this.Title = string.Format(Utils.GetString("ReauthorizeAccount"), (object) this._type.ToString());
    }

    private async void UpdateAccount()
    {
      BindAccountWindow bindAccountWindow = this;
      BindAccountModel model = new BindAccountModel()
      {
        domain = bindAccountWindow._domain,
        username = bindAccountWindow._username,
        password = bindAccountWindow.PasswordInput.Password,
        desc = bindAccountWindow._description
      };
      string str1;
      if (bindAccountWindow._type == Constants.BindAccountType.CalDAV)
        str1 = await Communicator.UpdateCalDavAccount(bindAccountWindow._account.Id, model);
      else
        str1 = await Communicator.UpdateExchangeAccount(bindAccountWindow._account.Id, model);
      string str2 = str1;
      if (str2.Contains("calendar_bind_faild") || str2.Contains("false"))
      {
        bindAccountWindow.ErrorText.Text = Utils.GetString("CalendarBindFaild");
        bindAccountWindow.SetContentIsEnabled(true);
      }
      else
      {
        bindAccountWindow.PullEvent();
        bindAccountWindow.SetContentIsEnabled(true);
        if (bindAccountWindow._parent != null)
          await bindAccountWindow._parent.LoadData(false);
        bindAccountWindow.Close();
      }
    }

    private async Task PullEvent()
    {
      await CalendarService.PullBindCalEvents(this._account.Id, new DateTime?(), new DateTime?(), this._type);
      CalendarEventChangeNotifier.NotifyRemoteChanged();
    }

    private async void BindAccount()
    {
      BindAccountWindow bindAccountWindow = this;
      try
      {
        BindAccountModel model = new BindAccountModel()
        {
          domain = bindAccountWindow._domain,
          username = bindAccountWindow._username,
          password = bindAccountWindow.PasswordInput.Password,
          desc = bindAccountWindow._description
        };
        BindCalendarAccountModel calendarAccountModel;
        if (bindAccountWindow._type == Constants.BindAccountType.CalDAV)
          calendarAccountModel = await Communicator.BindCalDavAccount(model);
        else
          calendarAccountModel = await Communicator.BindExchangeAccount(model);
        BindCalendarAccountModel calendar = calendarAccountModel;
        if (calendar == null)
          throw new CustomException.CalendarBindException("caldav_bind_faild");
        await BindCalendarAccountDao.SaveBindCalendarAccount(calendar);
        if (bindAccountWindow._parent != null)
          await bindAccountWindow._parent.LoadData(false);
        ListViewContainer.ReloadProjectData();
        CalendarService.PullBindCalEvents(calendar.Id, new DateTime?(), new DateTime?(), bindAccountWindow._type);
        bindAccountWindow.SetContentIsEnabled(true);
        bindAccountWindow.Close();
        calendar = (BindCalendarAccountModel) null;
      }
      catch (CustomException.CalendarBindException ex)
      {
        bindAccountWindow.ErrorText.Text = !(ex.Message == "calendar_bind_duplicate") ? (!(ex.Message == "calendar_bind_faild") ? (!(ex.Message == "calendar_bind_exceed") ? ex.Message : string.Format(Utils.GetString("CalendarBindExceed"), (object) bindAccountWindow._type.ToString())) : Utils.GetString("CalendarBindFaild")) : Utils.GetString("CalendarBindDuplicate");
      }
      finally
      {
        bindAccountWindow.SetContentIsEnabled(true);
      }
    }

    private void SetContentIsEnabled(bool isEnabled)
    {
      if (isEnabled)
      {
        this.SaveButton.Content = (object) Utils.GetString("OK");
        this.SaveButton.IsEnabled = true;
        this.SaveButton.Opacity = 1.0;
        this.DomainInput.IsEnabled = !this._isReauthorize;
        this.UsernameInput.IsEnabled = true;
        this.PasswordInput.IsEnabled = true;
        this.DescriptionInput.IsEnabled = true;
        this.CancelButton.IsEnabled = true;
      }
      else
      {
        this.SaveButton.Content = (object) Utils.GetString("Subscribing");
        this.SaveButton.IsEnabled = false;
        this.SaveButton.Opacity = 0.699999988079071;
        this.CancelButton.IsEnabled = false;
        this.DomainInput.IsEnabled = false;
        this.UsernameInput.IsEnabled = false;
        this.PasswordInput.IsEnabled = false;
        this.DescriptionInput.IsEnabled = false;
      }
    }

    public async void Ok()
    {
      if (this._needDomain && string.IsNullOrEmpty(this._domain) || string.IsNullOrEmpty(this._username) || string.IsNullOrEmpty(this.PasswordInput.Password))
        return;
      if (this._needDomain)
      {
        if (!this.CheckDomain())
        {
          this.SaveButton.IsEnabled = false;
          this.ErrorText.Text = Utils.GetString("CalDavServerError");
          return;
        }
      }
      else
      {
        ErrorModel errorModel = await Communicator.CheckExchangeAccount(this._username);
        if (!string.IsNullOrEmpty(errorModel?.errorCode))
        {
          this.SaveButton.IsEnabled = false;
          this.ErrorText.Text = errorModel.errorMessage;
          return;
        }
      }
      this.ErrorText.Text = string.Empty;
      this.SetContentIsEnabled(false);
      if (this._isReauthorize)
        this.UpdateAccount();
      else
        this.BindAccount();
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    public void OnCancel() => this.Close();

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.Ok();

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.OnCancel();

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      if (sender is PasswordBox passwordBox)
      {
        if (passwordBox.Password.Length > 0)
          this.PasswordInputPlaceholder.Visibility = Visibility.Collapsed;
        else
          this.PasswordInputPlaceholder.Visibility = Visibility.Visible;
      }
      this.SaveButton.IsEnabled = (!this._needDomain || !string.IsNullOrEmpty(this._domain)) && !string.IsNullOrEmpty(this._username) && !string.IsNullOrEmpty(this.PasswordInput.Password);
    }

    private void OnDomainTextChanged(object sender, TextChangedEventArgs e)
    {
      this._domain = this.DomainInput.Text;
      this.ErrorText.Text = string.Empty;
      this.SaveButton.IsEnabled = (!this._needDomain || !string.IsNullOrEmpty(this._domain)) && !string.IsNullOrEmpty(this._username) && !string.IsNullOrEmpty(this.PasswordInput.Password);
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this._description = this.DescriptionInput.Text;
      this._username = this.UsernameInput.Text;
      this.ErrorText.Text = string.Empty;
      this.SaveButton.IsEnabled = (!this._needDomain || !string.IsNullOrEmpty(this._domain)) && !string.IsNullOrEmpty(this._username) && !string.IsNullOrEmpty(this.PasswordInput.Password);
    }

    private bool CheckDomain()
    {
      if (string.IsNullOrEmpty(this._domain))
        return false;
      return new Regex("^[a-zA-Z0-9][a-zA-Z0-9-]{1,61}[a-zA-Z0-9](?:\\.[a-zA-Z]{2,})").IsMatch(this._domain) || new Regex("(https?):\\/\\/[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]").IsMatch(this._domain);
    }

    protected override void OnClosed(EventArgs eventArgs)
    {
      try
      {
        if (this.Owner == null)
          return;
        this.Owner.Activate();
      }
      catch (Exception ex)
      {
      }
    }

    private void ClearDomainText(object sender, MouseButtonEventArgs e)
    {
      this.DomainInput.Text = string.Empty;
    }

    private void ClearNameText(object sender, MouseButtonEventArgs e)
    {
      this.UsernameInput.Text = string.Empty;
    }

    private void ClearDescText(object sender, MouseButtonEventArgs e)
    {
      this.DescriptionInput.Text = string.Empty;
    }

    private void OpenGuide(object sender, MouseButtonEventArgs e)
    {
      Utils.TryProcessStartUrl(Utils.IsDida() ? "https://help.dida365.com/articles/6950000554735042560#caldav-%E6%97%A5%E5%8E%86%E5%90%8C%E6%AD%A5" : "https://help.ticktick.com/articles/7055781593733922816#caldav-calendar-sync");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/bindaccountwindow.xaml", UriKind.Relative));
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
          this.UserNameRun = (Run) target;
          break;
        case 3:
          this.UsernameInput = (TextBox) target;
          this.UsernameInput.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        case 4:
          this.UsernameInputPlaceholder = (TextBlock) target;
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearNameText);
          break;
        case 6:
          this.PasswordInput = (PasswordBox) target;
          this.PasswordInput.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case 7:
          this.PasswordInputPlaceholder = (TextBlock) target;
          break;
        case 8:
          this.DomainStarRun = (Run) target;
          break;
        case 9:
          this.DomainInput = (TextBox) target;
          this.DomainInput.TextChanged += new TextChangedEventHandler(this.OnDomainTextChanged);
          break;
        case 10:
          this.ServerInputPlaceholder = (TextBlock) target;
          break;
        case 11:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearDomainText);
          break;
        case 12:
          this.DescriptionInput = (TextBox) target;
          this.DescriptionInput.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        case 13:
          this.DescriptionInputPlaceholder = (TextBlock) target;
          break;
        case 14:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearDescText);
          break;
        case 15:
          this.ErrorText = (TextBlock) target;
          break;
        case 16:
          this.OperationGuide = (TextBlock) target;
          break;
        case 17:
          ((ContentElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OpenGuide);
          break;
        case 18:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 19:
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
