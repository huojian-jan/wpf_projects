// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.App
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Win32;
using SQLite;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Lock;
using ticktick_WPF.Views.MarkDown.SpellCheck;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Theme;
using ticktick_WPF.Views.Widget;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF
{
  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
  public class App : System.Windows.Application, IS2CMessages
  {
    private const int CheckReminderInterval = 5000;
    private const int CheckExtraInterval = 1000;
    private static Mutex _mutex;
    public static List<ReminderModel> ReminderList = new List<ReminderModel>();
    public static NotifyIcon NotifyIcon = new NotifyIcon();
    public static bool IsHideStart;
    private static bool _isInOperation;
    public static bool IsRegisterSuccess = false;
    public static bool IsProjectOrGroupDragging = false;
    private static bool _isUnhandledException;
    public static bool ProExpiredSyncError = false;
    private static bool _topmostCanceled;
    private static bool _reminderTimeZoneChanged;
    private System.Timers.Timer _checkReminderTimer;
    private static bool _windowShow;
    private readonly List<ReminderModel> _firedReminderModels = new List<ReminderModel>();
    private ServiceHost _clientHost;
    private Guid _clientId;
    private static DateTime _time;
    private NamedPipeServerStream _pipeServer;
    private string _pipeName = "TICKTICKAPP";
    private bool _contentLoaded;

    public bool TimeZoneChangeHandling { get; set; }

    public static CultureInfo Ci => GlobalSettings.Ci;

    public static ticktick_WPF.Views.MainWindow Window => MainWindowManager.Window;

    public App()
    {
      TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(this.TaskUnhandledException);
      System.Windows.Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(this.Current_DispatcherUnhandledException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.CurrentDomain_UnhandledException);
      SystemEvents.SessionSwitch += new SessionSwitchEventHandler(this.OnSessionSwitch);
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
      TickFocusManager.OnSessionChanged();
    }

    public static SQLiteAsyncConnection Connection => TickTickDao.Connection.DbConnection;

    public static App Instance { get; private set; }

    public void CommandInClient(string text)
    {
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      bool createdNew = false;
      Utils.LogActionTimes((Action) (() =>
      {
        App._mutex = new Mutex(true, "TickTick", out createdNew);
        AppPaths.Init(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Tick_Tick\\", AppDomain.CurrentDomain.BaseDirectory);
        this.CheckIsAdmin();
      }), 100, (string) null, "StartMutex");
      this._pipeName += Environment.UserName.GetHashCode().ToString();
      await this.StartUpSafely(e, createdNew);
    }

    public static void RegisterURIProtocol()
    {
      string str1 = "ticktick";
      string fileName = Process.GetCurrentProcess().MainModule?.FileName;
      if (string.IsNullOrEmpty(fileName))
      {
        UtilLog.Info("RegisterURIProtocol application Path should not be null or empty!");
      }
      else
      {
        string str2 = "\"" + fileName + "\" %1";
        RegistryKey registryKey1 = (RegistryKey) null;
        RegistryKey registryKey2 = (RegistryKey) null;
        RegistryKey registryKey3 = (RegistryKey) null;
        try
        {
          registryKey1 = Registry.CurrentUser?.OpenSubKey("Software", true)?.OpenSubKey("Classes", true);
          registryKey2 = registryKey1?.OpenSubKey(str1);
          if (registryKey2 != null)
          {
            registryKey3 = registryKey2.OpenSubKey("shell\\open\\command");
            if (registryKey3 != null)
            {
              object obj1 = registryKey3.GetValue("URL Protocol");
              object obj2 = registryKey3.GetValue("");
              if (str2.Equals(obj2) && str1.Equals(obj1))
                return;
            }
          }
          registryKey2 = registryKey2 ?? registryKey1?.CreateSubKey(str1);
          if (registryKey2 == null)
            return;
          object obj = registryKey2.GetValue("URL Protocol");
          if (!str1.Equals(obj))
            registryKey2.SetValue("URL Protocol", (object) str1);
          registryKey3 = registryKey3 ?? registryKey2.CreateSubKey("shell\\open\\command");
          registryKey3?.SetValue("", (object) str2);
        }
        catch (Exception ex)
        {
          UtilLog.Error("RegisterURIProtocol Exception, " + ex.Message);
        }
        finally
        {
          registryKey1?.Close();
          registryKey2?.Close();
          registryKey3?.Close();
        }
      }
    }

    public static bool IsAdmin => GlobalSettings.IsAdmin;

    private void CheckIsAdmin()
    {
      try
      {
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
          return;
        GlobalSettings.IsAdmin = true;
      }
      catch (Exception ex)
      {
      }
    }

    private async Task StartUpSafely(StartupEventArgs e, bool createdNew)
    {
      App app = this;
      string str1 = "Show";
      if (e.Args.Length != 0)
      {
        str1 = string.Empty;
        foreach (string str2 in e.Args)
        {
          if (str2 == "-hide")
            App.IsHideStart = true;
          str1 += str2;
        }
      }
      if (createdNew)
      {
        App.Instance = app;
        GlobalSettings.Ci = new CultureInfo("zh-CN");
        try
        {
          Utils.LoadDefaultLanguage();
          await Utils.LogTaskTimes(InitApp(), 300, "InitApp", "InitApp");
        }
        catch (Exception ex)
        {
          int num1 = (int) System.Windows.MessageBox.Show(Utils.GetString("CreatDbFailed"), Utils.GetString("PublicError"));
          int num2 = (int) System.Windows.MessageBox.Show(ExceptionUtils.BuildExceptionMessage(ex));
        }
        LocalSettings.Init();
        app.InitDll();
        await Utils.LogTaskTimes(App.TryGetLocalSettings());
        await Utils.LogTaskTimes(app.CheckTimeZoneOnStart());
        GlobalSettings.AppInstance = (System.Windows.Application) app;
        GlobalSettings.UseCustomWindowChrome = LocalSettings.Settings.Common.UseCustomWindowChrome;
        GlobalSettings.Ci = new CultureInfo(LocalSettings.Settings.UserChooseLanguage);
        Thread.CurrentThread.CurrentCulture = App.Ci;
        Thread.CurrentThread.CurrentUICulture = App.Ci;
        UtilLog.Info(string.Format("SetCi {0}", (object) App.Ci));
        Utils.LogActionTimes((Action) (() =>
        {
          Utils.SwitchLanguage();
          App.SetFontSize();
          TaskDllInitializer.Init();
          TTCalendarDllInitializer.Init();
        }), 1000, (string) null, "SetLanguageAndIcon");
        if (LocalSettings.Settings.LoginUserId == "")
        {
          App.FirstUse = true;
          new LoginDialog().Show();
        }
        else
        {
          bool show = true;
          if (App.IsHideStart)
            show = !App.IsHideStart && LocalSettings.Settings.Common.ShowWhenStart == 0 || LocalSettings.Settings.Common.ShowWhenStart == 1;
          app.ShowMainWindow(show, isStart: true);
          await Task.Delay(4000);
          JumpHelper.InitJumpList();
          JumpHelper.InitChangeEvents();
        }
        App.SetStickyFontSize();
        MemoryHelper.Cracker();
        app.InitReminderTimer();
        app.InitDateChangedEvent();
        app.InitTimeZoneChangedListener();
        AppIconUtils.SetAppIcons(LocalSettings.Settings.Common.AppIconKey, true);
        try
        {
          TicketLogUtils.RemoveTenDaysAgoLogs();
        }
        catch (Exception ex)
        {
        }
        app.InitPipe();
        app.SetBrowserVersion();
      }
      else
      {
        try
        {
          NamePipeService.SendPipeText(str1, app._pipeName);
        }
        catch (Exception ex)
        {
          UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
          app.OriginalPipe(str1);
        }
        finally
        {
          app.Shutdown();
        }
      }

      async Task InitApp()
      {
        try
        {
          ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }
        catch (Exception ex)
        {
        }
        App.SetAlignment();
        this.CheckNewUser();
        await TickDbHelper.CreateDbAsync();
        PipelineService.GetInstance().RegisterServiceHost();
      }
    }

    private void OriginalPipe(string arg)
    {
      try
      {
        this.RegisterClientHost();
        this.Dispatcher?.BeginInvoke((Delegate) (() => App.Register(this._clientId, arg)));
      }
      catch (Exception ex)
      {
        UtilLog.Warn("PipeRegisterFailed");
      }
    }

    private void InitDll()
    {
    }

    private async Task CheckTimeZoneOnStart()
    {
      TimeZoneInfo tz = TimeZoneInfo.Local;
      string localTimeZone = LocalSettings.Settings.LocalTimeZone;
      if (!string.IsNullOrEmpty(localTimeZone) && localTimeZone != tz.Id)
      {
        try
        {
          TimeZoneInfo systemTimeZoneById = TimeZoneInfo.FindSystemTimeZoneById(localTimeZone);
          await this.HandleTimeZoneChanged(tz, systemTimeZoneById, true);
          tz = (TimeZoneInfo) null;
        }
        catch
        {
          LocalSettings.Settings.LocalTimeZone = tz.Id;
          tz = (TimeZoneInfo) null;
        }
      }
      else if (!string.IsNullOrEmpty(localTimeZone))
      {
        tz = (TimeZoneInfo) null;
      }
      else
      {
        LocalSettings.Settings.LocalTimeZone = tz.Id;
        tz = (TimeZoneInfo) null;
      }
    }

    public async void Restart(bool delay = false)
    {
      App app = this;
      await LocalSettings.Settings.Save();
      if (delay)
        await Task.Delay(1000);
      App.ReleaseMutex();
      Process.Start(Assembly.GetExecutingAssembly().Location);
      app.Shutdown();
    }

    private void SetBrowserVersion()
    {
      if (InternetExplorerBrowserEmulation.IsBrowserEmulationSet())
        return;
      InternetExplorerBrowserEmulation.SetBrowserEmulationVersion();
    }

    public static void SetFontSize()
    {
      switch (LocalSettings.Settings.BaseFontSize)
      {
        case 16:
          App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
          {
            Source = new Uri("Resource\\FontResource\\Font_Middle.xaml", UriKind.Relative)
          });
          break;
        case 18:
          App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
          {
            Source = new Uri("Resource\\FontResource\\Font_Large.xaml", UriKind.Relative)
          });
          break;
        default:
          App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
          {
            Source = new Uri("Resource\\FontResource\\Font_Normal.xaml", UriKind.Relative)
          });
          break;
      }
    }

    public static void SetStickyFontSize()
    {
      int stickyFont = LocalSettings.Settings.ExtraSettings.StickyFont;
      ResourceDictionary resources = App.Instance.Resources;
      switch (stickyFont)
      {
        case 0:
          resources[(object) "StickyFont14"] = (object) 13.0;
          resources[(object) "StickyFont13"] = (object) 12.0;
          resources[(object) "StickyFont12"] = (object) 11.0;
          resources[(object) "StickyFont11"] = (object) 10.0;
          resources[(object) "StickyFont10"] = (object) 10.0;
          resources[(object) "StickyHeight30"] = (object) 28.0;
          resources[(object) "StickyHeight42"] = (object) 38.0;
          resources[(object) "StickyTopTitleMargin"] = (object) new Thickness(12.0, 3.0, 6.0, 0.0);
          resources[(object) "StickyCheckItemIconMargin"] = (object) new Thickness(3.0, 7.0, 0.0, 0.0);
          break;
        case 1:
          resources[(object) "StickyFont14"] = (object) 14.0;
          resources[(object) "StickyFont13"] = (object) 13.0;
          resources[(object) "StickyFont12"] = (object) 12.0;
          resources[(object) "StickyFont11"] = (object) 11.0;
          resources[(object) "StickyFont10"] = (object) 10.0;
          resources[(object) "StickyHeight30"] = (object) 30.0;
          resources[(object) "StickyHeight42"] = (object) 42.0;
          resources[(object) "StickyTopTitleMargin"] = (object) new Thickness(12.0, 2.0, 6.0, 0.0);
          resources[(object) "StickyCheckItemIconMargin"] = (object) new Thickness(3.0, 8.0, 0.0, 0.0);
          break;
        case 2:
          resources[(object) "StickyFont14"] = (object) 16.0;
          resources[(object) "StickyFont13"] = (object) 14.0;
          resources[(object) "StickyFont12"] = (object) 13.0;
          resources[(object) "StickyFont11"] = (object) 12.0;
          resources[(object) "StickyFont10"] = (object) 12.0;
          resources[(object) "StickyHeight30"] = (object) 34.0;
          resources[(object) "StickyHeight42"] = (object) 42.0;
          resources[(object) "StickyTopTitleMargin"] = (object) new Thickness(12.0, 2.0, 6.0, 0.0);
          resources[(object) "StickyCheckItemIconMargin"] = (object) new Thickness(3.0, 9.0, 0.0, 0.0);
          break;
        case 3:
          resources[(object) "StickyFont14"] = (object) 18.0;
          resources[(object) "StickyFont13"] = (object) 16.0;
          resources[(object) "StickyFont12"] = (object) 14.0;
          resources[(object) "StickyFont11"] = (object) 13.0;
          resources[(object) "StickyFont10"] = (object) 12.0;
          resources[(object) "StickyHeight30"] = (object) 36.0;
          resources[(object) "StickyHeight42"] = (object) 46.0;
          resources[(object) "StickyTopTitleMargin"] = (object) new Thickness(12.0, 1.0, 6.0, 0.0);
          resources[(object) "StickyCheckItemIconMargin"] = (object) new Thickness(3.0, 10.0, 0.0, 0.0);
          break;
      }
    }

    private static async Task TryGetLocalSettings()
    {
      int step = 0;
      try
      {
        CommonSettings common = await App.Connection.Table<CommonSettings>().FirstOrDefaultAsync();
        step = 1;
        if (common == null)
        {
          common = new CommonSettings();
          step = 2;
          int num = await App.Connection.InsertAsync((object) common);
        }
        step = 3;
        LocalSettings.Settings.Common = common;
        step = 4;
        if (!string.IsNullOrEmpty(common.LoginUserId))
        {
          step = 5;
          List<SettingsModel> listAsync = await App.Connection.Table<SettingsModel>().ToListAsync();
          step = 6;
          SettingsModel setting = listAsync != null ? listAsync.FirstOrDefault<SettingsModel>((Func<SettingsModel, bool>) (m => m?.UserId == common.LoginUserId)) : (SettingsModel) null;
          step = 7;
          if (setting == null)
          {
            setting = new SettingsModel()
            {
              UserId = common.LoginUserId
            };
            step = 8;
            int num = await App.Connection.InsertAsync((object) setting);
          }
          step = 9;
          LocalSettings.Settings.SettingsModel = setting;
          step = 10;
          Task.Run((Action) (() =>
          {
            LocalSettings.Settings.SetPreference();
            LocalSettings.Settings.SetSmartProjects(LocalSettings.Settings.SettingsModel.SmartProjects);
            SpellCheckUtils.TryInitSpellLanguage();
          }));
          setting = (SettingsModel) null;
        }
        step = 11;
        GlobalSettings.UserId = common.LoginUserId;
        step = 12;
      }
      catch (NullReferenceException ex)
      {
        UserActCollectUtils.SendException(new Exception("TryGetLocalSettings NullReferenceException " + step.ToString()), ExceptionType.UiThread);
        throw;
      }
    }

    private void LoadProBaseTheme(string themeId = "", string oldTheme = "")
    {
      if (string.IsNullOrEmpty(oldTheme) || ThemeKey.IsDarkTheme(oldTheme))
        this.Resources.MergedDictionaries.Add(new ResourceDictionary()
        {
          Source = new Uri("Resource/Themes/Light.xaml", UriKind.Relative)
        });
      this.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri("Resource/Themes/ProBase.xaml", UriKind.Relative)
      });
      try
      {
        App.SetMainBackground(themeId);
      }
      catch (Exception ex)
      {
      }
    }

    private void LoadDerivedTheme() => this.AddDictionary("Resource/Themes/ColorDerived.xaml");

    private static void SetMainBackground(string themeId = "")
    {
      if (string.IsNullOrEmpty(themeId))
        themeId = LocalSettings.Settings.ThemeId;
      string str = AppPaths.ThemeDir + themeId.ToLower() + ".png";
      Rect viewBox = new Rect(0.0, 0.0, 1.0, 1.0);
      if (themeId == "Custom")
      {
        if (!Directory.Exists(AppPaths.ThemeDir))
          Directory.CreateDirectory(AppPaths.ThemeDir);
        string[] files = Directory.GetFiles(AppPaths.ThemeDir, "custom*.*");
        if (files.Length != 0)
          str = files[0];
        viewBox = Utils.GetRectByString(LocalSettings.Settings.CustomThemeLocation);
        if (viewBox.Width <= 0.0 || viewBox.Height <= 0.0)
          viewBox = new Rect(0.0, 0.0, 1.0, 1.0);
      }
      UtilLog.Info("SetMainBackground " + str);
      if (System.IO.File.Exists(str) && new FileInfo(str).Length > 0L)
        App.Window.SetBackgroundImage(Utils.LoadBitmap(str), viewBox);
      else
        UtilLog.Warn("SetMainBackground " + str + " not Exist");
    }

    private void LoadColorBaseTheme(string oldTheme = "")
    {
      if (string.IsNullOrEmpty(oldTheme) || ThemeKey.IsDarkTheme(oldTheme))
        this.AddDictionary("Resource/Themes/Light.xaml");
      this.AddDictionary("Resource/Themes/ColorBase.xaml");
      App.Window?.HideBackground();
    }

    private void TryLoadSavedTheme()
    {
      if (string.IsNullOrEmpty(LocalSettings.Settings.ExtraSettings.AppTheme))
        LocalSettings.Settings.ExtraSettings.AppTheme = LocalSettings.Settings.ThemeId;
      LocalSettings.Settings.ThemeId = string.Empty;
      if (LocalSettings.Settings.ExtraSettings.UseSystemTheme)
        ThemeUtil.TrySetSystemTheme(true);
      else
        this.LoadTargetTheme(LocalSettings.Settings.ExtraSettings.AppTheme);
    }

    public void LoadTargetTheme(string themeId, bool fromDark = false)
    {
      if (LocalSettings.Settings.ExtraSettings.AppTheme == "Custom")
      {
        LocalSettings.Settings.LoadCustomTheme();
        this.SetCustomTheme(string.IsNullOrEmpty(LocalSettings.Settings.CustomThemeColor) ? "#4772FA" : LocalSettings.Settings.CustomThemeColor, fromDark);
      }
      else
        this.LoadTheme(LocalSettings.Settings.ThemeId, themeId);
    }

    private void SetCustomTheme(string color, bool fromDark)
    {
      ResourceDictionary rd = new ResourceDictionary();
      rd[(object) "ProjectHoverColor"] = ColorConverter.ConvertFromString("#1AFFFFFF");
      rd[(object) "ProjectSelectedBackground"] = (object) ThemeUtil.GetColorInString("#33FFFFFF");
      rd[(object) "TextBoxBackground"] = (object) ThemeUtil.GetAlphaColor("#000000", 4);
      rd[(object) "ProjectMenuBackGround"] = (object) ThemeUtil.GetColorInString("#1A000000");
      rd[(object) "ShowAreaBackground"] = (object) ThemeUtil.GetAlphaColor("#FFFFFF", (int) (LocalSettings.Settings.ShowAreaOpacity * 100.0));
      rd[(object) "LeftBarIconColor"] = ColorConverter.ConvertFromString("#FFFFFF");
      rd[(object) "ProjectMenuBaseColor"] = ColorConverter.ConvertFromString("#FFFFFF");
      rd[(object) "MainBackground"] = (object) ThemeUtil.GetColorInString("#000000");
      rd[(object) "KanbanContainerColor"] = (object) Brushes.Transparent;
      rd[(object) "MatrixContainerColor"] = (object) Brushes.Transparent;
      if (ColorConverter.ConvertFromString(color) is System.Windows.Media.Color color1)
      {
        rd[(object) "ColorPrimary"] = (object) color1;
        rd[(object) "MainWindow"] = (object) color1;
        rd[(object) "PrimaryColor"] = (object) new SolidColorBrush(color1);
        rd[(object) "DateColorPrimary"] = (object) new SolidColorBrush(color1);
      }
      if (ColorConverter.ConvertFromString("#CC" + color.Substring(color.Length - 6, 6)) is System.Windows.Media.Color color2)
      {
        rd[(object) "LeftBarBackColorTop"] = (object) color2;
        rd[(object) "LeftBarBackColorBottom"] = (object) color2;
      }
      if (ColorConverter.ConvertFromString("#28" + color.Substring(color.Length - 6, 6)) is System.Windows.Media.Color color3)
        rd[(object) "ItemSelected"] = (object) color3;
      if (ColorConverter.ConvertFromString("#14" + color.Substring(color.Length - 6, 6)) is System.Windows.Media.Color color4)
        rd[(object) "ItemHover"] = (object) color4;
      this.LoadTheme(fromDark ? "Dark" : "Blue", "Custom", rd);
    }

    private void AddDictionary(string url)
    {
      this.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri(url, UriKind.Relative)
      });
    }

    public void LoadTheme(string oldTheme = "", string newTheme = "", ResourceDictionary rd = null, bool isRetry = false)
    {
      try
      {
        string str = LocalSettings.Settings.ThemeId;
        if (newTheme != null && str != newTheme && !string.IsNullOrEmpty(newTheme))
          str = newTheme;
        LocalSettings.Settings.ThemeId = str;
        if (!ThemeUtil.ExistTheme(str))
        {
          LocalSettings.Settings.ThemeId = "White";
          str = LocalSettings.Settings.ThemeId;
        }
        if (!ThemeKey.IsDarkTheme(str))
        {
          if (string.IsNullOrEmpty(oldTheme) || ThemeKey.IsDarkTheme(oldTheme) || ThemeKey.IsPinkTheme(oldTheme) || ThemeKey.IsGreenTheme(oldTheme))
            this.AddDictionary("Resource/icons_light.xaml");
          if (ThemeKey.IsPinkTheme(str) || ThemeKey.IsGreenTheme(str))
            this.AddDictionary("Resource/icons_pink.xaml");
          this.AddDictionary("Resource/Themes/LightNecessaryColor.xaml");
        }
        else
          this.AddDictionary("Resource/icons_dark.xaml");
        if (ThemeKey.IsColorTheme(str))
          this.LoadColorBaseTheme(oldTheme);
        else if (!ThemeKey.IsDarkTheme(str))
          this.LoadProBaseTheme(str, oldTheme);
        else
          App.Window?.HideBackground();
        if (rd != null)
        {
          this.Resources.MergedDictionaries.Add(rd);
        }
        else
        {
          LocalSettings.Settings.ResetCustomTheme();
          this.AddDictionary("Resource/Themes/" + str + ".xaml");
        }
        if (!ThemeKey.IsDarkTheme(str))
          this.LoadDerivedTheme();
        this.SetProjectMenuTextShadowOpacity(ThemeKey.ShowProjectTextShadow(str));
        DataChangedNotifier.NotifyThemeModeChanged();
        if (string.IsNullOrEmpty(oldTheme) || (ThemeKey.IsDarkTheme(oldTheme) || !ThemeKey.IsDarkTheme(str)) && (!ThemeKey.IsDarkTheme(oldTheme) || ThemeKey.IsDarkTheme(str)))
          return;
        DataChangedNotifier.NotifyIsDarkChanged();
      }
      catch (Exception ex)
      {
        if (!isRetry)
          this.LoadTheme(oldTheme, newTheme, rd, true);
        else
          UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
      }
    }

    internal async void LoadFontResource()
    {
      App app = this;
      string appFontFamily = LocalSettings.Settings.ExtraSettings.AppFontFamily;
      string fontFamily = FontFamilyUtils.GetFontFamily(appFontFamily);
      FontFamily fontFamilyByKey = FontFamilyUtils.GetFontFamilyByKey(appFontFamily);
      LocalSettings.Settings.FontFamily = fontFamilyByKey;
      ticktick_WPF.Resource.Constants.DefaultMonoFont = "Consolas," + fontFamily;
      string uriString = "Resource/FontResource/FontFamilyNormal.xaml";
      switch (appFontFamily)
      {
        case "SourceHansansSC_CN":
          uriString = "Resource/FontResource/FontFamilySourceHanSans.xaml";
          break;
        case "975Maru_CN":
          uriString = "Resource/FontResource/FontFamily975Maru.xaml";
          break;
        case "Default_CN":
          uriString = "Resource/FontResource/FontFamilyYaHei.xaml";
          break;
        case "Yozai_CN":
          uriString = "Resource/FontResource/FontFamilyYozai.xaml";
          break;
      }
      app.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri(uriString, UriKind.Relative)
      });
    }

    public void SetProjectMenuTextShadowOpacity(bool showShadow)
    {
      LocalSettings.Settings.ProjectTextShadowOpacity = showShadow ? 0.4 : 0.0;
    }

    private void InitDateChangedEvent()
    {
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(App.OnDateChanged);
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(App.OnDateChanged);
    }

    private static void OnDateChanged(object sender, EventArgs e)
    {
      App.Window?.HandleDateChanged();
      App.Window?.SyncVersionStatus();
    }

    private void InitTimeZoneChangedListener()
    {
      SystemEvents.TimeChanged -= new EventHandler(this.OnTimeZoneChanged);
      SystemEvents.TimeChanged += new EventHandler(this.OnTimeZoneChanged);
    }

    private async void OnTimeZoneChanged(object sender, EventArgs e)
    {
      TimeZoneInfo.ClearCachedData();
      TimeZoneInfo local = TimeZoneInfo.Local;
      TimeZoneInfo timeZone = TimeZoneData.LocalTimeZoneModel?.TimeZone;
      TimeZoneData.LocalTimeZoneModel = new TimeZoneViewModel(local);
      await this.HandleTimeZoneChanged(local, timeZone);
    }

    private async Task HandleTimeZoneChanged(TimeZoneInfo newTz, TimeZoneInfo oldTz, bool isStart = false)
    {
      if (oldTz == null || oldTz.Id == newTz.Id)
        return;
      this.TimeZoneChangeHandling = true;
      try
      {
        if (!isStart)
          Utils.Toast(Utils.GetString("TimeZoneChangeRestart"));
        else
          App._reminderTimeZoneChanged = true;
        await TaskDao.HandleTimeZoneChanged(newTz, oldTz);
        await CalendarEventDao.HandleTimeZoneChanged(newTz, oldTz);
        LocalSettings.Settings.LocalTimeZone = newTz.Id;
        await UserActCollectUtils.OnDeviceDataChanged();
        if (isStart)
          return;
        LocalSettings.Settings.ExtraSettings.ReminderCalculated = false;
        if (!LocalSettings.Settings.EnableTimeZone && new CustomerDialog(Utils.GetString("TimeZoneChange"), Utils.GetString("TimeZoneChangeText"), Utils.GetString("Enable"), Utils.GetString("Cancel")).ShowDialog().GetValueOrDefault())
        {
          LocalSettings.Settings.EnableTimeZone = true;
          await SettingsHelper.PushLocalSettings();
        }
        await LocalSettings.Settings.Save();
        this.Restart(true);
      }
      finally
      {
        this.TimeZoneChangeHandling = false;
      }
    }

    public async Task ShowMainWindow(bool show = true, bool delay = false, bool isStart = false)
    {
      ABTestManager.AssignGroupOnLaunch();
      MainWindowManager.InitWindow();
      if (LocalSettings.Settings.NeedShowTutorial)
      {
        LocalSettings.Settings.NeedShowTutorial = false;
        App.Window.TryShowTutorial();
        await Task.Delay(500);
      }
      App._windowShow = true;
      ReminderBase.UseNewReminderCalculate = new bool?();
      this.InitData();
      this.LoadUserSettings();
      MainWindowManager.InitWindowSettingOnLogin();
      AppConfigManager.PerformPullAppConfig();
      AppConfigManager.StartPolling();
      if (show)
        await App.Window.TryShowMainWindow(delay: delay, isStart: isStart);
      UserActCollectUtils.StartPost();
    }

    private void LoadUserSettings()
    {
      App.SetMainBackground(LocalSettings.Settings.ThemeId);
      this.TryLoadSavedTheme();
      this.LoadSavedFont();
      ResourceUtils.SetKanbanColumnWidth();
    }

    private async void InitData()
    {
      await Task.Run((Func<Task>) (async () =>
      {
        await App.WarmCache();
        HolidayManager.TryPullRemoteHolidays();
        SyncManager.Init();
        FocusPieceHandler.LoadSavedFocus();
        TimeChangeNotifier.BeginTimer();
        ReminderCalculator.AssembleReminders();
        ReminderBase.InitReminders(App._reminderTimeZoneChanged);
        App.TryStartWidgets();
      }));
      SearchProjectHelper.InitModels();
      TickFocusManager.SaveAfterClose = true;
      if (!App._reminderTimeZoneChanged || LocalSettings.Settings.EnableTimeZone || !new CustomerDialog(Utils.GetString("TimeZoneChange"), Utils.GetString("TimeZoneChangeText"), Utils.GetString("OK"), Utils.GetString("Cancel")).ShowDialog().GetValueOrDefault())
        return;
      LocalSettings.Settings.EnableTimeZone = true;
      LocalSettings.Settings.Save();
      SettingsHelper.PushLocalSettings();
    }

    private void LoadSavedFont()
    {
      if (Utils.IsZhCn() && Utils.IsDida() || Utils.IsEn() && !Utils.IsDida())
      {
        this.LoadFontResource();
      }
      else
      {
        string familyName = "Segoe UI, Microsoft YaHei UI, Microsoft JhengHei";
        switch (App.Ci.ToString())
        {
          case "ja-JP":
            familyName = "Segoe UI, Yu Gothic UI, Microsoft JhengHei UI, Microsoft YaHei UI";
            break;
          case "ko-KR":
            familyName = "Segoe UI, Malgun Gothic, Microsoft JhengHei UI, Microsoft YaHei UI";
            break;
        }
        FontFamily fontFamily = new FontFamily(familyName);
        LocalSettings.Settings.FontFamily = fontFamily;
        ticktick_WPF.Resource.Constants.DefaultMonoFont = "Consolas," + familyName;
      }
    }

    private static async Task WarmCache()
    {
      await CacheManager.Init();
      MainWindowManager.OnDataInited();
      UserManager.Init();
      TaskDefaultDao.InitCache();
      UpgradeHelper.CheckUpgradePoint();
      NotificationHelper.InitProjectNtfOptions();
      HolidayManager.GetRecentHolidays();
      HabitSectionCache.SetSections();
      SmartListTaskFoldHelper.Load();
    }

    private static async void TryStartWidgets()
    {
      await Task.Delay(10);
      if (await App.CanShowWidget())
      {
        TaskStickyWindow.TryLoadSavedStickies();
        await Task.Delay(3000);
        App.Instance.Dispatcher.Invoke((Action) (() =>
        {
          CalendarWidgetHelper.TryLoadWidget();
          MatrixWidgetHelper.TryLoadWidget();
          ProjectWidgetsHelper.LoadWidgets();
        }));
      }
      AppLockModel model = await AppLockCache.GetModel();
      if (model == null || !model.Locked)
        IndependentWindow.Restore();
      if (!LocalSettings.Settings.PomoLocalSetting.OpenWidget)
        return;
      System.Windows.Application.Current?.Dispatcher.Invoke((Action) (() => TickFocusManager.HideOrShowFocusWidget(true)));
    }

    private static async Task<bool> CanShowWidget()
    {
      AppLockModel model = await AppLockCache.GetModel();
      return model == null || !model.Locked || !model.LockWidget;
    }

    public static bool IsInOperation() => App._isInOperation;

    public static async Task NavigateEvent(string eventId)
    {
      if (await AppLockCache.GetAppLocked())
        AppUnlockWindow.TryUnlockApp(new Action(Navigate));
      else
        Navigate();

      void Navigate()
      {
        App.Window.ShowWindow();
        App.Window.NavigateEvent(eventId);
      }
    }

    public static void NavigateCourse(string courseId)
    {
      try
      {
        App.Window.TryShowMainWindow();
        App.Window.NavigateCourse(courseId);
      }
      catch (Exception ex)
      {
      }
    }

    public static void NavigateHabit(string habitId, bool showRecord = false)
    {
      try
      {
        App.Window.TryShowMainWindow();
        App.Window.NavigateHabit(habitId, showRecord);
      }
      catch (Exception ex)
      {
      }
    }

    public static void ShowMainWindow(string taskId, string itemId = "", bool skiplock = false)
    {
      try
      {
        if (!string.IsNullOrEmpty(taskId))
          App.Window.UnlockAndNavigateTask(taskId, itemId);
        else
          App.Window.TryShowMainWindow(skiplock);
      }
      catch (Exception ex)
      {
      }
    }

    private void InitReminderTimer()
    {
      if (ABTestManager.IsNewRemindCalculate())
        return;
      this._checkReminderTimer = new System.Timers.Timer(5000.0);
      this._checkReminderTimer.Elapsed += new ElapsedEventHandler(this.CheckOnTimeReminders);
      this._checkReminderTimer.Start();
    }

    private void RegisterClientHost()
    {
      this._clientHost = new ServiceHost((object) this, Array.Empty<Uri>());
      this._clientId = Guid.NewGuid();
      UtilLog.Info("RegisterClientHost " + this._clientId.ToString());
      this._clientHost.AddServiceEndpoint(typeof (IS2CMessages), (System.ServiceModel.Channels.Binding) new NetNamedPipeBinding(), "net.pipe://localhost/Client_" + this._clientId.ToString());
      this._clientHost.Open();
    }

    private static void SetAlignment()
    {
      if (!SystemParameters.MenuDropAlignment)
        return;
      FieldInfo field = typeof (SystemParameters).GetField("_menuDropAlignment", BindingFlags.Static | BindingFlags.NonPublic);
      if (!(field != (FieldInfo) null))
        return;
      field.SetValue((object) null, (object) false);
    }

    private void TaskUnhandledException(object sender, UnobservedTaskExceptionEventArgs e)
    {
      UtilLog.Warn("TaskUnhandledException() " + e.Exception?.ToString());
      UserActCollectUtils.SendException((Exception) e.Exception, ExceptionType.Task);
      e.SetObserved();
    }

    private void Current_DispatcherUnhandledException(
      object sender,
      DispatcherUnhandledExceptionEventArgs e)
    {
      if (!App._isUnhandledException)
      {
        if (App.TryHandleException(e.Exception))
        {
          e.Handled = true;
          return;
        }
        if (e.Exception.StackTrace.Contains("LoadFontResource"))
        {
          LocalSettings.Settings.ExtraSettings.AppFontFamily = "Default_CN";
          LocalSettings.Settings.Save(true);
        }
        bool restart = false;
        if (e.Exception.StackTrace.Contains("WindowChrome") && GlobalSettings.UseCustomWindowChrome)
        {
          LocalSettings.Settings.Common.UseCustomWindowChrome = true;
          LocalSettings.Settings.Save(true);
          restart = true;
        }
        App._isUnhandledException = true;
        UserActCollectUtils.SendException(e.Exception, ExceptionType.UiThread);
        string str = (e.Exception.InnerException?.ToString() ?? string.Empty) + e.Exception.Message;
        if (str.Contains("Version=") && str.Contains("Culture="))
          str.Contains("PublicKeyToken=");
        Utils.RunOnUiThread(this.Dispatcher, (Action) (() => App.ShowExceptionWindow(e.Exception, restart)));
      }
      e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      if (!(e.ExceptionObject is Exception exceptionObject) || !(exceptionObject.GetType().ToString() != "System.Threading.ThreadAbortException") || !(exceptionObject.GetType().ToString() != "System.Threading.Tasks.TaskCanceledException"))
        return;
      UtilLog.Error("CurrentDomain_UnhandledException() " + exceptionObject?.ToString());
      UserActCollectUtils.SendException(exceptionObject, ExceptionType.OtherThread);
    }

    private static bool TryHandleException(Exception e)
    {
      if (!(e is COMException comException) || comException.ErrorCode != -2147221037 && comException.ErrorCode != -2147221040)
        return false;
      Utils.Toast("ClipBoard Error");
      return true;
    }

    private static async void ShowExceptionWindow(Exception e, bool restart = false)
    {
      switch (e)
      {
        case NotSupportedException _:
          break;
        case ConfigurationErrorsException _:
          break;
        default:
          LocalSettings.Settings.Save();
          await Task.Delay(100);
          new UnhandledExceptionWindow(e, restart).ShowDialog();
          break;
      }
    }

    public void StopReminderTimer() => this._checkReminderTimer?.Stop();

    public void StartReminderTimer() => this._checkReminderTimer?.Start();

    private async void CheckOnTimeReminders(object sender, ElapsedEventArgs e)
    {
      this._checkReminderTimer.Stop();
      List<ReminderModel> unfiredList = new List<ReminderModel>();
      DateTime checkReminderTime = DateTime.Now;
      foreach (ReminderModel reminderModel in new List<ReminderModel>((IEnumerable<ReminderModel>) App.ReminderList))
      {
        if (reminderModel != null)
        {
          DateTime? reminderTime = reminderModel.ReminderTime;
          if (reminderTime.HasValue && Math.Abs((checkReminderTime - reminderTime.Value).TotalMilliseconds) < 6000.0)
          {
            if (!this.IsReminderRecentFired(reminderModel))
            {
              this._firedReminderModels.Add(reminderModel);
              await ReminderBase.ShowReminderWindow(reminderModel);
            }
          }
          else
            unfiredList.Add(reminderModel);
        }
      }
      if (unfiredList.Count > 0)
        App.ReminderList = unfiredList;
      this._checkReminderTimer.Start();
      unfiredList = (List<ReminderModel>) null;
    }

    private bool IsTaskProjectMuted(ReminderModel reminder)
    {
      if (reminder.Type == 0 || reminder.Type == 1)
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(reminder.TaskId);
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == reminder.ProjectId));
        if (projectModel != null && !projectModel.IsValid())
          return true;
        bool flag = string.IsNullOrEmpty(taskById.Assignee) || taskById.Assignee == "-1";
        if ((flag && taskById.Creator != LocalSettings.Settings.LoginUserId || !flag && reminder.Assignee != Utils.GetCurrentUserIdInt().ToString()) && projectModel != null && projectModel.muted && projectModel.IsShareList())
          return true;
      }
      return false;
    }

    private bool IsReminderRecentFired(ReminderModel model)
    {
      return this._firedReminderModels.Any<ReminderModel>((Func<ReminderModel, bool>) (reminder =>
      {
        if (reminder.TaskId == model.TaskId && reminder.CheckItemId == model.CheckItemId && reminder.EventId == model.EventId)
        {
          DateTime? reminderTime1 = reminder.ReminderTime;
          DateTime? reminderTime2 = model.ReminderTime;
          if ((reminderTime1.HasValue == reminderTime2.HasValue ? (reminderTime1.HasValue ? (reminderTime1.GetValueOrDefault() == reminderTime2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
            return reminder.HabitId == model.HabitId;
        }
        return false;
      }));
    }

    public async void TryDismissRemindersOnSync()
    {
      List<ReminderModel> removes = new List<ReminderModel>();
      if (this._firedReminderModels.Any<ReminderModel>())
      {
        for (int i = 0; i < this._firedReminderModels.Count; ++i)
        {
          ReminderModel reminder = this._firedReminderModels[i];
          if (await ReminderCalculator.IsReminderExpired(reminder))
          {
            SystemToastUtils.RemoveToast(!string.IsNullOrEmpty(reminder.CheckItemId) ? reminder.CheckItemId : reminder.TaskId, reminder.ProjectId);
            removes.Add(reminder);
          }
          reminder = (ReminderModel) null;
        }
      }
      if (!removes.Any<ReminderModel>())
      {
        removes = (List<ReminderModel>) null;
      }
      else
      {
        using (List<ReminderModel>.Enumerator enumerator = removes.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            ReminderModel current = enumerator.Current;
            DateTime? reminderTime = current.ReminderTime;
            DateTime dateTime = DateTime.Now.AddMinutes(-1.0);
            if ((reminderTime.HasValue ? (reminderTime.GetValueOrDefault() <= dateTime ? 1 : 0) : 0) != 0)
              this._firedReminderModels.Remove(current);
          }
          removes = (List<ReminderModel>) null;
        }
      }
    }

    protected override void OnExit(ExitEventArgs e)
    {
      App.RemoveTrayIcon();
      try
      {
        HotKeyUtils.RemoveHotkeys();
      }
      catch (Exception ex)
      {
      }
      TaskStickyWindow.OnAppExit();
      UserActCollectUtils.OnAppExit();
      this._pipeServer?.Close();
      base.OnExit(e);
    }

    public async void ExitApplication()
    {
      App app = this;
      if (!await TickFocusManager.ExitPomo())
        return;
      App.RemoveTrayIcon();
      app.Shutdown();
    }

    private static void RemoveTrayIcon()
    {
      if (App.NotifyIcon == null)
        return;
      App.NotifyIcon.Visible = false;
      App.NotifyIcon.Dispose();
      App.NotifyIcon = (NotifyIcon) null;
    }

    private static void CloseChannel(ICommunicationObject channel)
    {
      try
      {
        channel.Close();
      }
      catch (Exception ex)
      {
      }
      finally
      {
        channel.Abort();
      }
    }

    private void InitPipe()
    {
      NamePipeService.InitPipe(this._pipeName, new Action<string>(this.HandlePipeCommand));
    }

    private void HandlePipeCommand(string command)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (command.StartsWith("ticktick://"))
          this.TickTickUri(command);
        else
          App.Window?.HandlePipelineMsg(command);
      }));
    }

    private void TickTickUri(string uri)
    {
      uri = uri.Substring(uri.IndexOf("//", StringComparison.Ordinal) + 2);
      uri = HttpUtility.UrlDecode(uri);
      if (!uri.Contains<char>('?'))
        return;
      UriNotifier.NotifyUri(uri.Substring(0, uri.IndexOf('?')), uri.Substring(uri.IndexOf('?') + 1));
    }

    private static void Register(Guid clientId, string command)
    {
      using (ChannelFactory<IC2SMessages> channelFactory = new ChannelFactory<IC2SMessages>((System.ServiceModel.Channels.Binding) new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/TickTick")))
      {
        IC2SMessages channel = channelFactory.CreateChannel();
        try
        {
          channel.Register(clientId);
          channel.DisplayCommandOnServer(command);
        }
        catch (Exception ex)
        {
          UtilLog.Warn(ExceptionUtils.BuildExceptionMessage(ex));
        }
        finally
        {
          App.CloseChannel(channel as ICommunicationObject);
        }
      }
    }

    public static void SelectTagProject(string tag) => App.Window.SelectTagProject(tag);

    public static void ClearReminders() => App.ReminderList.Clear();

    public static void CancelTopMost()
    {
      if (App.Window == null || !LocalSettings.Settings.MainWindowTopmost)
        return;
      App._topmostCanceled = true;
      LocalSettings.Settings.MainWindowTopmost = false;
    }

    public static void RestoreTopMost()
    {
      if (App.Window == null || !App._topmostCanceled)
        return;
      App._topmostCanceled = false;
      LocalSettings.Settings.MainWindowTopmost = true;
    }

    public static void RefreshIconMenu()
    {
      App.Instance.Dispatcher.Invoke(new Action(App.Window.LoadAppOptions));
    }

    public async void AddCalendarWidget()
    {
      if (!ProChecker.CheckPro(ProType.CalendarWidget))
        return;
      CalendarWidgetHelper.AddWidget();
    }

    public async void CloseCalendarWidget() => CalendarWidgetHelper.CloseWidget();

    public async void AddMatrixWidget()
    {
      if (!ProChecker.CheckPro(ProType.MatrixWidget))
        return;
      MatrixWidgetHelper.AddWidget();
    }

    public async void CloseMatrixWidget() => MatrixWidgetHelper.CloseWidget();

    public static async void AddProjectWidget() => ProjectWidgetsHelper.AddDefaultWidget();

    public static void ReloadProjects() => App.Window?.ReloadView();

    public static async Task Unlock()
    {
      await AppLockDao.SetAppLocked(false);
      App.Window.UnlockApp();
    }

    public static void ReleaseMutex()
    {
      System.Windows.Application.Current.Dispatcher.Invoke((Action) (() =>
      {
        try
        {
          App._mutex.ReleaseMutex();
          App._mutex.Dispose();
        }
        catch (Exception ex)
        {
        }
      }));
    }

    private async void OnAppExit(object sender, ExitEventArgs e)
    {
      PipelineService.GetInstance().CloseServiceHost();
      await LocalSettings.Settings.Save(true);
      await TaskCompletionRateDao.SaveToDb();
      if (Utils.IsWindows7())
        return;
      SystemToastUtils.ClearToastHistory();
    }

    private async void CheckNewUser()
    {
      try
      {
        string appName = "Dida";
        await Task.Delay(1);
        if (Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey(appName) == null)
        {
          string id = Utils.GetGuid();
          if (!System.IO.File.Exists(AppPaths.TickTickDbPath))
          {
            string str = await Communicator.RegNewUser(id);
          }
          Registry.CurrentUser.CreateSubKey("Software\\" + appName)?.SetValue("UId", (object) id);
          id = (string) null;
        }
        App.UId = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey(appName)?.GetValue("UId") as string;
        if (string.IsNullOrEmpty(App.UId))
          App.UId = Utils.GetGuid0();
        appName = (string) null;
      }
      catch (Exception ex)
      {
        App.UId = Utils.GetGuid0();
      }
    }

    public static bool FirstUse { get; set; }

    public static string UId { get; set; }

    public void SetNotifyIcon(bool isLock)
    {
      if (isLock)
      {
        App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconLocked;
      }
      else
      {
        AppIconKey result;
        System.Enum.TryParse<AppIconKey>(LocalSettings.Settings.Common.AppIconKey, out result);
        switch (result)
        {
          case AppIconKey.CClassic:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconCClassic;
            break;
          case AppIconKey.SClassic:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconSClassic;
            break;
          case AppIconKey.SClassicPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconSClassicPro;
            break;
          case AppIconKey.SGreenPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconSGreenPro;
            break;
          case AppIconKey.SOrangePro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconSOrangePro;
            break;
          case AppIconKey.SPinkPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconSPinkPro;
            break;
          case AppIconKey.SYellowPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconSYellowPro;
            break;
          case AppIconKey.SBlackPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconSBlackPro;
            break;
          case AppIconKey.WGreenPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconWGreenPro;
            break;
          case AppIconKey.PlanetPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconPlanetPro;
            break;
          case AppIconKey.GYellowPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconGYellowPro;
            break;
          case AppIconKey.GRedPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconGRedPro;
            break;
          case AppIconKey.GSiriPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconGSiriPro;
            break;
          case AppIconKey.GBluePro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconGBluePro;
            break;
          case AppIconKey.LRedPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconLRedPro;
            break;
          case AppIconKey.LYellowPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconLYellowPro;
            break;
          case AppIconKey.LGreenPro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconLGreenPro;
            break;
          case AppIconKey.ScratchBluePro:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconScratchBluePro;
            break;
          case AppIconKey.Christmas:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconChristmas;
            break;
          case AppIconKey.SpringFestival:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconSpringFestival;
            break;
          default:
            App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconDefault;
            break;
        }
      }
    }

    public void LoadTimelineStyle()
    {
      string source = "Styles\\TimelineStyle.xaml";
      if (this.Resources.MergedDictionaries.FirstOrDefault<ResourceDictionary>((Func<ResourceDictionary, bool>) (d => d.Source?.OriginalString != null && d.Source.OriginalString.Equals(source))) != null)
        return;
      this.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri(source, UriKind.Relative)
      });
    }

    public static bool GetWindowShow() => App._windowShow;

    public async void TryLockApp()
    {
      if (string.IsNullOrEmpty(await AppLockCache.GetLockPassword()))
      {
        new SetPasswordWindow(SetPasswordMode.FirstTry).ShowDialog();
        if (string.IsNullOrEmpty(await AppLockCache.GetLockPassword()))
          return;
        this.LockAndHide();
      }
      else
        this.LockAndHide();
    }

    public void LockAndHide()
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() =>
      {
        this.LockApp();
        App.Window.HideWindow(false);
      }));
    }

    public async Task LockApp()
    {
      AppLockDao.SetAppLocked(true);
      App.Window.SetLockedOption();
      JumpHelper.InitJumpList();
      if (App.NotifyIcon != null)
        App.NotifyIcon.Icon = ticktick_WPF.Properties.Resources.AppIconLocked;
      AppLockModel model = await AppLockCache.GetModel();
      WindowManager.AppLockOrExit = true;
      UtilLog.Info("AppLocked");
      if (model != null)
      {
        if (model.LockWidget)
        {
          CalendarWidgetHelper.HideWidget();
          MatrixWidgetHelper.HideWidget();
          ProjectWidgetsHelper.CloseAll();
          TaskStickyWindow.CloseAllStickies();
        }
        IndependentWindow.CloseAll();
        if (model.LockAfter)
          App.Window.StopLock();
      }
      TaskDetailWindows.CloseAll();
    }

    public static void CheckLastActiveTime()
    {
      int lastActiveTime = LocalSettings.Settings.ExtraSettings.LastActiveTime;
      int dateNum = ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Today);
      int num = dateNum;
      if (lastActiveTime == num)
        return;
      LocalSettings.Settings.ExtraSettings.LastActiveTime = dateNum;
      LocalSettings.Settings.Save();
      UserActCollectUtils.OnDeviceDataChanged();
    }

    public static async Task<bool> CheckTwitterLogin(bool logOut) => true;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      this.Exit += new ExitEventHandler(this.OnAppExit);
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/app.xaml", UriKind.Relative));
    }

    [STAThread]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public static void Main()
    {
      App app = new App();
      app.InitializeComponent();
      app.Run();
    }
  }
}
