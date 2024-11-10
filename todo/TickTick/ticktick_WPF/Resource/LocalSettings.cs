// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Resource.LocalSettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Summary;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Resource
{
  [Serializable]
  public class LocalSettings : INotifyPropertyChanged
  {
    public static readonly LocalSettings Settings = new LocalSettings();
    private bool _isChanged = true;
    public SettingsModel SettingsModel = new SettingsModel();
    public CommonSettings Common = new CommonSettings();
    public CustomThemeSetting CustomTheme = new CustomThemeSetting();
    private CalendarDisplaySettings _calendarDisplaySettings;
    private ProjectListOpenStatus _projectListOpenStatus;
    private ShortcutModel _shortcutModel;
    private PomoLocalSetting _pomoLocalSetting;
    private StatisticsModel _statisticsModel;
    private bool _inSearch;
    private SummaryFilterModel _summaryFilter;
    private List<SmartProjectConf> _smartProjects;
    private double _projectTextShadowOpacity;
    private bool _projectPinFolded = true;
    private FontFamily _fontFamily;
    private ExtraSettings _extraSettings;

    public UserPreferenceModel UserPreference { get; set; }

    public DesktopTabBar DesktopConfig => this.UserPreference?.desktopTabBars;

    public event EventHandler<string> OnPreferenceChanged;

    public void SetRemotePreference(UserPreferenceModel remote)
    {
      if (remote == null || remote.mtime == 0L)
      {
        if (remote == null || remote.mtime != 0L || this.DesktopConfig != null)
          return;
        this.InitDesktopTabBars(TabBarModel.InitTabBars());
        SettingsHelper.PushLocalPreference();
      }
      else
      {
        bool flag = false;
        if (remote.mtime > this.UserPreference.mtime)
          this.UserPreference.mtime = remote.mtime;
        this.SettingsModel.PreferrenceMTime = remote.mtime;
        if (remote.desktopTabBars == null)
        {
          if (this.DesktopConfig == null)
            this.InitDesktopTabBars(TabBarModel.InitTabBars());
          this.UserPreference.mtime = Utils.GetNowTimeStampInMills();
          flag = true;
        }
        long? mtime1 = this.DesktopConfig?.mtime;
        foreach (PropertyInfo property in typeof (UserPreferenceModel).GetProperties())
        {
          if (property.GetValue((object) this.UserPreference) is PreferenceBaseModel preferenceBaseModel)
            flag = flag || preferenceBaseModel.SetRemoteValue(property.GetValue((object) remote) as PreferenceBaseModel);
          else
            property.SetValue((object) this.UserPreference, property.GetValue((object) remote));
        }
        this._isChanged = true;
        this.Save();
        if (flag)
          SettingsHelper.PushLocalPreference();
        long? nullable = mtime1;
        long? mtime2 = this.UserPreference.desktopTabBars?.mtime;
        if (!(nullable.GetValueOrDefault() == mtime2.GetValueOrDefault() & nullable.HasValue == mtime2.HasValue))
          MainWindowManager.CheckSelectedModule();
        EventHandler<string> preferenceChanged = this.OnPreferenceChanged;
        if (preferenceChanged != null)
          preferenceChanged((object) this, (string) null);
        WebSocketService.CheckFocusSocket();
      }
    }

    public void ShowOrHideTabBar(string name, bool show)
    {
      if (this.UserPreference.desktopTabBars == null)
        this.UserPreference.desktopTabBars = new DesktopTabBar()
        {
          bars = TabBarModel.InitTabBars()
        };
      if (this.UserPreference.desktopTabBars.bars == null)
        this.UserPreference.desktopTabBars.bars = TabBarModel.InitTabBars();
      List<TabBarModel> source = this.DesktopConfig.bars ?? TabBarModel.InitTabBars();
      List<TabBarModel> bars = this.UserPreference.desktopTabBars.bars;
      TabBarModel bar = bars != null ? bars.FirstOrDefault<TabBarModel>((Func<TabBarModel, bool>) (b => b.name.ToLower() == name.ToLower())) : (TabBarModel) null;
      if (bar != null)
      {
        bar.status = show ? "active" : "inactive";
        if (show && source.Count<TabBarModel>((Func<TabBarModel, bool>) (b => b.sortOrder == bar.sortOrder)) > 1)
        {
          List<TabBarModel> list = source.Where<TabBarModel>((Func<TabBarModel, bool>) (b => b.status == "active" && b.name != bar.name)).ToList<TabBarModel>();
          for (int index = 0; index < list.Count; ++index)
            list[index].sortOrder = (long) index;
          bar.sortOrder = (long) list.Count;
        }
      }
      else
        this.UserPreference.desktopTabBars.bars.Add(new TabBarModel()
        {
          name = name.ToUpper(),
          sortOrder = source.Max<TabBarModel>((Func<TabBarModel, long>) (b => b.sortOrder)) + 1L,
          status = show ? "active" : "inactive"
        });
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.mtime = timeStampInMills;
      this.DesktopConfig.mtime = timeStampInMills;
      this.DesktopConfig.bars = source;
      this._isChanged = true;
      MainWindowManager.CheckSelectedModule();
      SettingsHelper.PushLocalPreference();
    }

    public bool ShowMatrix => TabBarModel.IsActive("Matrix", this.DesktopConfig?.bars);

    public MatrixModel MatrixModel => this.UserPreference?.matrix;

    public TimelineSettingsModel TimelineSettingsModel => this.UserPreference?.Timeline;

    public bool MatrixShowCompleted => (bool?) this.MatrixModel?.show_completed ?? true;

    public void SaveMatrixQuadrant(int level, string rule, SortOption sort, string name)
    {
      if (this.MatrixModel == null)
        this.UserPreference.matrix = MatrixModel.GetDefault();
      UtilLog.Info(string.Format("SaveMatrixQuadrant {0} {1}", (object) level, (object) rule));
      QuadrantModel quadrantModel = this.MatrixModel.quadrants.FirstOrDefault<QuadrantModel>((Func<QuadrantModel, bool>) (q => q.id == "quadrant" + level.ToString()));
      if (quadrantModel == null)
      {
        quadrantModel = QuadrantModel.GetDefault(level);
        this.MatrixModel.quadrants.Add(quadrantModel);
      }
      if (rule != null && quadrantModel.rule != rule)
      {
        quadrantModel.rule = rule;
        this.MatrixModel.version = 1;
      }
      if (sort != null)
      {
        quadrantModel.SortOption = sort;
        quadrantModel.sortType = sort.groupBy == "none" ? sort.orderBy : sort.groupBy;
      }
      if (name != null)
        quadrantModel.name = name == string.Empty ? (string) null : name;
      this.OnMatrixQuadrantChanged();
    }

    public void OnMatrixQuadrantChanged()
    {
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.MatrixModel.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
      SettingsHelper.PushLocalPreference();
      this.Save();
    }

    public void SetPreference()
    {
      string userPreference = LocalSettings.Settings.SettingsModel.UserPreference;
      if (!string.IsNullOrEmpty(userPreference))
      {
        try
        {
          this.UserPreference = JsonConvert.DeserializeObject<UserPreferenceModel>(userPreference);
        }
        catch (Exception ex)
        {
          UtilLog.Error(ex);
        }
      }
      this.UserPreference = this.UserPreference ?? new UserPreferenceModel();
      if (this.UserPreference.matrix == null || this.UserPreference.matrix.IsEmpty())
        this.UserPreference.matrix = MatrixModel.GetDefault();
      if (this.UserPreference.Timeline == null || string.IsNullOrEmpty(this.UserPreference.Timeline.Color))
        this.UserPreference.Timeline = new TimelineSettingsModel();
      if (this.UserPreference.desktop_conf != null)
        return;
      this.UserPreference.desktop_conf = new DesktopConfigModel();
    }

    public void SavePreference()
    {
      this.SettingsModel.UserPreference = JsonConvert.SerializeObject((object) this.UserPreference);
    }

    public bool ShowTimeTableInCal
    {
      get
      {
        if (!UserDao.IsPro())
          return false;
        TimeTableModel timeTable = this.UserPreference.TimeTable;
        return timeTable != null && timeTable.ShowInCal;
      }
    }

    public bool AutoAcceptShare
    {
      get
      {
        TeamConfig teamConfig = this.UserPreference.TeamConfig;
        return teamConfig == null || teamConfig.autoAcceptInvite;
      }
    }

    public void SetAutoAcceptShare(bool auto)
    {
      if (this.UserPreference.TeamConfig == null)
        this.UserPreference.TeamConfig = new TeamConfig();
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.TeamConfig.autoAcceptInvite = auto;
      this.UserPreference.TeamConfig.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
    }

    public void SetTimelineShowGuide()
    {
      if (this.UserPreference.Timeline == null)
        this.UserPreference.Timeline = new TimelineSettingsModel();
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.Timeline.ShowGuide = false;
      this.UserPreference.Timeline.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
    }

    public void SetUrlParseEnable(bool enable)
    {
      if (this.UserPreference.GeneralConfig == null)
        this.UserPreference.GeneralConfig = new GeneralConfig();
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.GeneralConfig.urlParseEnabled = enable;
      this.UserPreference.GeneralConfig.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
    }

    public void SetEmailRemindItems(List<string> items)
    {
      if (this.UserPreference.GeneralConfig == null)
        this.UserPreference.GeneralConfig = new GeneralConfig();
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.GeneralConfig.mtime = timeStampInMills;
      this.UserPreference.GeneralConfig.emailRemindItems = items;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
    }

    public void SetEmailReminder(bool enable)
    {
      if (this.UserPreference == null)
        return;
      if (this.UserPreference.GeneralConfig == null)
        this.UserPreference.GeneralConfig = new GeneralConfig();
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.GeneralConfig.emailRemindEnabled = enable;
      this.UserPreference.GeneralConfig.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
    }

    public void InitDesktopTabBars(List<TabBarModel> bars)
    {
      if (this.UserPreference == null)
        return;
      if (this.UserPreference.desktopTabBars == null)
        this.UserPreference.desktopTabBars = new DesktopTabBar();
      this.UserPreference.desktopTabBars.bars = bars;
      this.UserPreference.desktopTabBars.mtime = 1L;
      this.UserPreference.mtime = Utils.GetNowTimeStampInMills();
      this._isChanged = true;
    }

    public void SaveTabBarOrder(List<(string, long)> idToOrders)
    {
      if (this.UserPreference.desktopTabBars == null)
        this.UserPreference.desktopTabBars = new DesktopTabBar();
      if (this.UserPreference.desktopTabBars.bars == null || this.UserPreference.desktopTabBars.bars.Count == 0)
        this.UserPreference.desktopTabBars.bars = TabBarModel.InitTabBars();
      foreach ((string str, long num) in idToOrders)
      {
        TabBarModel tabBarModel = this.UserPreference.desktopTabBars.bars.FirstOrDefault<TabBarModel>((Func<TabBarModel, bool>) (b => b.name.ToLower() == str.ToLower()));
        if (tabBarModel != null)
        {
          tabBarModel.sortOrder = num;
          tabBarModel.status = "active";
        }
        else
          this.UserPreference.desktopTabBars.bars.Add(new TabBarModel()
          {
            name = str.ToUpper(),
            sortOrder = num,
            status = "active"
          });
      }
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.desktopTabBars.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
    }

    public void InitSmartProjectsOption(bool push = true)
    {
      if (this.UserPreference.SmartProjectsOption == null)
      {
        UserPreferenceModel userPreference = this.UserPreference;
        SmartProjectsOption smartProjectsOption = new SmartProjectsOption();
        smartProjectsOption.SmartProjects = SmartProjectService.InitSmartProjectOptions();
        smartProjectsOption.mtime = 1L;
        userPreference.SmartProjectsOption = smartProjectsOption;
        this.UserPreference.mtime = Utils.GetNowTimeStampInMills();
        this._isChanged = true;
      }
      if (!push)
        return;
      this.UserPreference.SmartProjectsOption.mtime = Utils.GetNowTimeStampInMills();
      SettingsHelper.PushLocalPreference();
    }

    public void SaveSmartProjectOptions(string name, SortOption option)
    {
      if (this.UserPreference?.SmartProjectsOption?.SmartProjects == null)
        this.InitSmartProjectsOption(false);
      if (this.UserPreference?.SmartProjectsOption?.SmartProjects == null)
        return;
      SmartProjectOption smartProjectByName = this.UserPreference.SmartProjectsOption?.GetSmartProjectByName(name);
      if (smartProjectByName != null)
        smartProjectByName.SortOption = option;
      else
        this.UserPreference.SmartProjectsOption.SmartProjects.Add(new SmartProjectOption()
        {
          Name = name,
          ViewMode = "list",
          SortOption = option
        });
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.SmartProjectsOption.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
      SettingsHelper.PushLocalPreference();
    }

    public void InitMatrixSortOption()
    {
      this.UserPreference.matrix?.InitSortOption();
      this._isChanged = true;
    }

    public void SaveSmartViewMode(string name, string viewMode)
    {
      if (this.UserPreference?.SmartProjectsOption?.SmartProjects == null)
        this.InitSmartProjectsOption(false);
      if (this.UserPreference?.SmartProjectsOption?.SmartProjects == null)
        return;
      SmartProjectOption smartProjectByName = this.UserPreference.SmartProjectsOption?.GetSmartProjectByName(name);
      if (smartProjectByName != null)
      {
        smartProjectByName.ViewMode = viewMode;
        if (viewMode == "kanban" && smartProjectByName.SortOption?.groupBy == "none")
          smartProjectByName.SortOption.groupBy = name == "inbox" ? "sortOrder" : "project";
      }
      else
      {
        List<SmartProjectOption> smartProjects = this.UserPreference.SmartProjectsOption.SmartProjects;
        SmartProjectOption smartProjectOption = new SmartProjectOption();
        smartProjectOption.Name = name;
        smartProjectOption.ViewMode = viewMode;
        SortOption sortOption;
        if (!(name == "inbox"))
        {
          sortOption = new SortOption()
          {
            orderBy = "dueDate",
            groupBy = "dueDate"
          };
        }
        else
        {
          sortOption = new SortOption();
          sortOption.orderBy = "sortOrder";
          sortOption.groupBy = "sortOrder";
        }
        smartProjectOption.SortOption = sortOption;
        smartProjects.Add(smartProjectOption);
      }
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.SmartProjectsOption.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
      SettingsHelper.PushLocalPreference();
    }

    public void SaveSmartProjectTimeline(string name, TimelineModel timeline)
    {
      if (this.UserPreference?.SmartProjectsOption?.SmartProjects == null)
        this.InitSmartProjectsOption(false);
      if (this.UserPreference?.SmartProjectsOption?.SmartProjects == null)
        return;
      SmartProjectOption smartProjectByName = this.UserPreference.SmartProjectsOption?.GetSmartProjectByName(name);
      if (smartProjectByName != null)
      {
        smartProjectByName.Timeline = timeline;
      }
      else
      {
        List<SmartProjectOption> smartProjects = this.UserPreference.SmartProjectsOption.SmartProjects;
        SmartProjectOption smartProjectOption = new SmartProjectOption();
        smartProjectOption.Name = name;
        smartProjectOption.Timeline = timeline;
        smartProjectOption.ViewMode = "list";
        SortOption sortOption;
        if (!(name == "inbox"))
        {
          sortOption = new SortOption()
          {
            orderBy = "dueDate",
            groupBy = "dueDate"
          };
        }
        else
        {
          sortOption = new SortOption();
          sortOption.orderBy = "sortOrder";
          sortOption.groupBy = "sortOrder";
        }
        smartProjectOption.SortOption = sortOption;
        smartProjects.Add(smartProjectOption);
      }
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.SmartProjectsOption.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
      SettingsHelper.PushLocalPreference();
    }

    public void SaveFocusKeepInSync(bool keepInSync)
    {
      if (this.UserPreference != null && this.UserPreference?.FocusConfig == null)
        this.UserPreference.FocusConfig = new ticktick_WPF.Models.FocusConfig();
      if (this.UserPreference?.FocusConfig == null)
        return;
      this.UserPreference.FocusConfig.keepInSync = keepInSync;
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.FocusConfig.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
      SettingsHelper.PushLocalPreference();
    }

    public void SaveCustomColor(string color)
    {
      if (string.IsNullOrEmpty(color))
        return;
      if (this.UserPreference != null && this.UserPreference?.RecentlyColors == null)
        this.UserPreference.RecentlyColors = new RecentlyColors();
      if (this.UserPreference?.RecentlyColors == null)
        return;
      List<string> source = this.UserPreference?.RecentlyColors.colors ?? new List<string>();
      for (int index = 0; index < source.Count; ++index)
      {
        string colorString = source[index];
        source[index] = ThemeUtil.GetNoAlphaColorString(colorString);
      }
      color = ThemeUtil.GetNoAlphaColorString(color);
      if (source.Contains(color))
        source.Remove(color);
      source.Insert(0, color);
      if (source.Count > 7)
        source = source.Take<string>(7).ToList<string>();
      this.UserPreference.RecentlyColors.colors = source;
      long timeStampInMills = Utils.GetNowTimeStampInMills();
      this.UserPreference.RecentlyColors.mtime = timeStampInMills;
      this.UserPreference.mtime = timeStampInMills;
      this._isChanged = true;
    }

    private LocalSettings()
    {
    }

    public static void Init()
    {
    }

    public object this[string propertyName]
    {
      get
      {
        PropertyInfo propertyInfo = ((IEnumerable<PropertyInfo>) this.GetType().GetProperties()).FirstOrDefault<PropertyInfo>((Func<PropertyInfo, bool>) (f => string.Equals(f.Name.ToLower(), propertyName.ToLower(), StringComparison.CurrentCultureIgnoreCase)));
        return !(propertyInfo != (PropertyInfo) null) ? (object) null : propertyInfo.GetValue((object) this);
      }
      set
      {
        ((IEnumerable<PropertyInfo>) this.GetType().GetProperties()).FirstOrDefault<PropertyInfo>((Func<PropertyInfo, bool>) (f => string.Equals(f.Name, propertyName, StringComparison.CurrentCultureIgnoreCase)))?.SetValue((object) this, value);
      }
    }

    public CalendarDisplaySettings CalendarDisplaySettings
    {
      get
      {
        if (this._calendarDisplaySettings == null && !string.IsNullOrEmpty(LocalSettings.Settings.SettingsModel.CalendarDisplaySettings))
          this._calendarDisplaySettings = JsonConvert.DeserializeObject<CalendarDisplaySettings>(LocalSettings.Settings.SettingsModel.CalendarDisplaySettings);
        this._calendarDisplaySettings = this._calendarDisplaySettings ?? new CalendarDisplaySettings();
        this._calendarDisplaySettings.CheckNull();
        return this._calendarDisplaySettings;
      }
      set => this._calendarDisplaySettings = value;
    }

    public ProjectListOpenStatus ProjectListOpenStatus
    {
      get
      {
        if (this._projectListOpenStatus == null && !string.IsNullOrEmpty(LocalSettings.Settings.SettingsModel.ProjectListOpenStatus))
          this._projectListOpenStatus = JsonConvert.DeserializeObject<ProjectListOpenStatus>(LocalSettings.Settings.SettingsModel.ProjectListOpenStatus);
        this._projectListOpenStatus = this._projectListOpenStatus ?? new ProjectListOpenStatus();
        return this._projectListOpenStatus;
      }
      set => this._projectListOpenStatus = value;
    }

    public ShortcutModel ShortCutModel
    {
      get
      {
        if (this._shortcutModel != null)
          return this._shortcutModel;
        this._shortcutModel = new ShortcutModel()
        {
          CanNotify = true
        };
        try
        {
          this._shortcutModel.CopyProperties(!string.IsNullOrEmpty(LocalSettings.Settings.SettingsModel.ShortcutModel) ? JsonConvert.DeserializeObject<ShortcutModel>(LocalSettings.Settings.SettingsModel.ShortcutModel) : (ShortcutModel) null);
        }
        catch (Exception ex)
        {
          UtilLog.Warn(ex.Message);
        }
        return this._shortcutModel;
      }
      set => this._shortcutModel = value;
    }

    public PomoLocalSetting PomoLocalSetting
    {
      get
      {
        if (this._pomoLocalSetting == null && !string.IsNullOrEmpty(LocalSettings.Settings.SettingsModel.PomoLocalSetting))
          this._pomoLocalSetting = JsonConvert.DeserializeObject<PomoLocalSetting>(LocalSettings.Settings.SettingsModel.PomoLocalSetting) ?? new PomoLocalSetting();
        this._pomoLocalSetting = this._pomoLocalSetting ?? new PomoLocalSetting();
        return this._pomoLocalSetting;
      }
    }

    public StatisticsModel StatisticsModel
    {
      get
      {
        if (this._statisticsModel == null && !string.IsNullOrEmpty(LocalSettings.Settings.SettingsModel.Statistics))
          this._statisticsModel = JsonConvert.DeserializeObject<StatisticsModel>(LocalSettings.Settings.SettingsModel.Statistics);
        return this._statisticsModel;
      }
      set => this._statisticsModel = value;
    }

    public int InBoxId
    {
      get => this.Common.InBoxId;
      set
      {
        this.Common.InBoxId = value;
        this._isChanged = true;
      }
    }

    public string LoginUserId
    {
      get => this.Common.LoginUserId;
      set
      {
        this.Common.LoginUserId = value;
        GlobalSettings.UserId = value;
        this._isChanged = true;
      }
    }

    public string LoginUserAuth
    {
      get => this.Common.LoginUserAuth;
      set
      {
        this.Common.LoginUserAuth = value;
        this._isChanged = true;
      }
    }

    public string LoginUserName
    {
      get => this.Common.LoginUserName;
      set
      {
        this.Common.LoginUserName = value;
        this._isChanged = true;
      }
    }

    public string LoginAvatarUrl
    {
      get => this.Common.LoginAvatarUrl;
      set
      {
        this.Common.LoginAvatarUrl = value;
        this._isChanged = true;
      }
    }

    public string InServerBoxId
    {
      get => this.Common.InServerBoxId;
      set
      {
        this.Common.InServerBoxId = value;
        this._isChanged = true;
      }
    }

    public string LocalTimeZone
    {
      get => this.Common.LocalTimeZone;
      set
      {
        this.Common.LocalTimeZone = value;
        this._isChanged = true;
      }
    }

    public int GrayVersionDate
    {
      get => this.Common.GrayVersionDate;
      set
      {
        this.Common.GrayVersionDate = value;
        this._isChanged = true;
      }
    }

    public string UserDeviceString
    {
      get => this.Common.UserDeviceString;
      set
      {
        this.Common.UserDeviceString = value;
        this._isChanged = true;
      }
    }

    public string CustomThemeLocation
    {
      get => this.CustomTheme.CustomThemeLocation;
      set => this.CustomTheme.CustomThemeLocation = value;
    }

    public string CustomThemeColor
    {
      get => this.CustomTheme.CustomThemeColor;
      set => this.CustomTheme.CustomThemeColor = value;
    }

    public double ThemeImageOpacity
    {
      get => Math.Max(this.CustomTheme.ThemeImageOpacity, 0.6);
      set
      {
        this.CustomTheme.ThemeImageOpacity = Math.Max(value, 0.6);
        this.OnPropertyChanged(nameof (ThemeImageOpacity));
      }
    }

    public int ThemeImageBlurRadius
    {
      get => Math.Min(this.CustomTheme.ThemeImageBlurRadius, 40);
      set
      {
        this.CustomTheme.ThemeImageBlurRadius = Math.Min(value, 40);
        this.OnPropertyChanged(nameof (ThemeImageBlurRadius));
      }
    }

    public double ShowAreaOpacity
    {
      get => Math.Max(this.CustomTheme.ShowAreaOpacity, 0.8);
      set => this.CustomTheme.ShowAreaOpacity = Math.Max(value, 0.8);
    }

    public double MainWindowHeight
    {
      get => this.SettingsModel.MainWindowHeight;
      set
      {
        this.SettingsModel.MainWindowHeight = value;
        this.OnPropertyChanged(nameof (MainWindowHeight));
      }
    }

    public double MainWindowWidth
    {
      get => this.SettingsModel.MainWindowWidth;
      set
      {
        this.SettingsModel.MainWindowWidth = value;
        this.OnPropertyChanged(nameof (MainWindowWidth));
      }
    }

    public long CheckPoint
    {
      get => this.SettingsModel.CheckPoint;
      set
      {
        this.SettingsModel.CheckPoint = value;
        this.OnPropertyChanged(nameof (CheckPoint));
      }
    }

    public int SmartListAll
    {
      get
      {
        if (this.SettingsModel.SmartListAll < 0)
          this.SettingsModel.SmartListAll = 0;
        return this.SettingsModel.SmartListAll;
      }
      set
      {
        this.SettingsModel.SmartListAll = value;
        this.OnPropertyChanged(nameof (SmartListAll));
      }
    }

    public int SmartListInbox
    {
      get
      {
        if (this.SettingsModel.SmartListInbox < 0)
          this.SettingsModel.SmartListInbox = 0;
        return this.SettingsModel.SmartListInbox;
      }
      set
      {
        this.SettingsModel.SmartListInbox = value;
        this.OnPropertyChanged(nameof (SmartListInbox));
      }
    }

    public int SmartListToday
    {
      get
      {
        if (this.SettingsModel.SmartListToday < 0)
          this.SettingsModel.SmartListToday = 0;
        return this.SettingsModel.SmartListToday;
      }
      set
      {
        this.SettingsModel.SmartListToday = value;
        this.OnPropertyChanged(nameof (SmartListToday));
      }
    }

    public int SmartListTomorrow
    {
      get
      {
        if (this.SettingsModel.SmartListTomorrow < 0)
          this.SettingsModel.SmartListTomorrow = 0;
        return this.SettingsModel.SmartListTomorrow;
      }
      set
      {
        this.SettingsModel.SmartListTomorrow = value;
        this.OnPropertyChanged(nameof (SmartListTomorrow));
      }
    }

    public int SmartList7Day
    {
      get
      {
        if (this.SettingsModel.SmartList7Day < 0)
          this.SettingsModel.SmartList7Day = 0;
        return this.SettingsModel.SmartList7Day;
      }
      set
      {
        this.SettingsModel.SmartList7Day = value;
        this.OnPropertyChanged(nameof (SmartList7Day));
      }
    }

    public int SmartListForMe
    {
      get
      {
        if (this.SettingsModel.SmartListForMe < 0)
          this.SettingsModel.SmartListForMe = 0;
        return this.SettingsModel.SmartListForMe;
      }
      set
      {
        this.SettingsModel.SmartListForMe = value;
        this.OnPropertyChanged(nameof (SmartListForMe));
      }
    }

    public int SmartListComplete
    {
      get
      {
        if (this.SettingsModel.SmartListComplete < 0)
          this.SettingsModel.SmartListComplete = 0;
        return this.SettingsModel.SmartListComplete;
      }
      set
      {
        this.SettingsModel.SmartListComplete = value;
        this.OnPropertyChanged(nameof (SmartListComplete));
      }
    }

    public int SmartListAbandoned
    {
      get
      {
        if (this.SettingsModel.SmartListAbandoned < 0)
          this.SettingsModel.SmartListAbandoned = 0;
        return this.SettingsModel.SmartListAbandoned;
      }
      set
      {
        this.SettingsModel.SmartListAbandoned = value;
        this.OnPropertyChanged(nameof (SmartListAbandoned));
      }
    }

    public int SmartListTrash
    {
      get
      {
        if (this.SettingsModel.SmartListTrash < 0)
          this.SettingsModel.SmartListTrash = 0;
        return this.SettingsModel.SmartListTrash;
      }
      set
      {
        this.SettingsModel.SmartListTrash = value;
        this.OnPropertyChanged(nameof (SmartListTrash));
      }
    }

    public string ShortcutOpenOrClose
    {
      get => this.SettingsModel.ShortcutOpenOrClose;
      set
      {
        this.SettingsModel.ShortcutOpenOrClose = value;
        this.OnPropertyChanged(nameof (ShortcutOpenOrClose));
      }
    }

    public string ShortcutAddTask
    {
      get => this.SettingsModel.ShortcutAddTask;
      set
      {
        this.SettingsModel.ShortcutAddTask = value;
        this.OnPropertyChanged(nameof (ShortcutAddTask));
      }
    }

    public int ShowCompleteLine
    {
      get => this.ExtraSettings.ShowCompleteLine;
      set
      {
        this.ExtraSettings.ShowCompleteLine = value;
        this.OnPropertyChanged(nameof (ShowCompleteLine));
      }
    }

    public bool IsPro
    {
      get => this.SettingsModel.IsPro;
      set
      {
        this.SettingsModel.IsPro = value;
        this.OnPropertyChanged(nameof (IsPro));
      }
    }

    public string SortTypeOfAllProject
    {
      get => this.SettingsModel.SortTypeOfAllProject;
      set
      {
        this.SettingsModel.SortTypeOfAllProject = value;
        this._isChanged = true;
      }
    }

    public string SortTypeOfInbox
    {
      get => this.SettingsModel.SortTypeOfInbox;
      set
      {
        this.SettingsModel.SortTypeOfInbox = value;
        this.OnPropertyChanged(nameof (SortTypeOfInbox));
      }
    }

    public string SortTypeOfAssignMe
    {
      get => this.SettingsModel.SortTypeOfAssignMe;
      set
      {
        this.SettingsModel.SortTypeOfAssignMe = value;
        this.OnPropertyChanged(nameof (SortTypeOfAssignMe));
      }
    }

    public string SortTypeOfToday
    {
      get => this.SettingsModel.SortTypeOfToday;
      set
      {
        this.SettingsModel.SortTypeOfToday = value;
        this.OnPropertyChanged(nameof (SortTypeOfToday));
      }
    }

    public string SortTypeOfTomorrow
    {
      get => this.SettingsModel.SortTypeOfTomorrow;
      set
      {
        this.SettingsModel.SortTypeOfTomorrow = value;
        this.OnPropertyChanged(nameof (SortTypeOfTomorrow));
      }
    }

    public string SortTypeOfWeek
    {
      get => this.SettingsModel.SortTypeOfWeek;
      set
      {
        this.SettingsModel.SortTypeOfWeek = value;
        this.OnPropertyChanged(nameof (SortTypeOfWeek));
      }
    }

    public string ClosedSectionStatus
    {
      get => this.SettingsModel.ClosedSectionStatus;
      set
      {
        this.SettingsModel.ClosedSectionStatus = value;
        this.OnPropertyChanged(nameof (ClosedSectionStatus));
      }
    }

    public bool MainWindowTopmost
    {
      get => this.SettingsModel.MainWindowTopmost;
      set
      {
        this.SettingsModel.MainWindowTopmost = value;
        this.OnPropertyChanged(nameof (MainWindowTopmost));
      }
    }

    public int SmartListTag
    {
      get
      {
        if (this.SettingsModel.SmartListTag < 0)
          this.SettingsModel.SmartListTag = 0;
        return this.SettingsModel.SmartListTag;
      }
      set
      {
        this.SettingsModel.SmartListTag = value;
        this.OnPropertyChanged(nameof (SmartListTag));
      }
    }

    public string UserChooseLanguage
    {
      get
      {
        return string.IsNullOrEmpty(this.Common.UserChooseLanguage) ? Utils.GetSystemLanguage() : this.Common.UserChooseLanguage;
      }
      set
      {
        this.Common.UserChooseLanguage = value;
        this.OnPropertyChanged(nameof (UserChooseLanguage));
      }
    }

    public int ShowCustomSmartList
    {
      get
      {
        if (this.SettingsModel.ShowCustomSmartList < 0)
          this.SettingsModel.ShowCustomSmartList = 0;
        return this.SettingsModel.ShowCustomSmartList;
      }
      set
      {
        this.SettingsModel.ShowCustomSmartList = value;
        this.OnPropertyChanged(nameof (ShowCustomSmartList));
      }
    }

    public long SyncPoint
    {
      get => this.SettingsModel.SyncPoint;
      set
      {
        this.SettingsModel.SyncPoint = value;
        this.OnPropertyChanged(nameof (SyncPoint));
      }
    }

    public string CalendarFilterData
    {
      get => this.SettingsModel.CalendarFilterData;
      set
      {
        this.SettingsModel.CalendarFilterData = value;
        this.OnPropertyChanged(nameof (CalendarFilterData));
      }
    }

    public string ArrangeTaskFilterData
    {
      get => this.SettingsModel.ArrangeTaskFilterData;
      set
      {
        this.SettingsModel.ArrangeTaskFilterData = value;
        this.OnPropertyChanged(nameof (ArrangeTaskFilterData));
      }
    }

    public bool FocusKeepInSync
    {
      get => UserDao.IsPro() && this.UserPreference?.FocusConfig?.keepInSync.GetValueOrDefault();
    }

    public int PomoDuration
    {
      get => this.SettingsModel.PomoDuration;
      set
      {
        if (this.SettingsModel.PomoDuration == value)
          return;
        this.SettingsModel.PomoDuration = value;
        TickFocusManager.OnPomoSettingsChanged();
        this._isChanged = true;
      }
    }

    public string PomoSound
    {
      get => this.PomoLocalSetting.PomoSound;
      set
      {
        this.PomoLocalSetting.PomoSound = value;
        this.OnPropertyChanged(nameof (PomoSound));
      }
    }

    public int ShortBreakDuration
    {
      get => this.SettingsModel.ShortBreakDuration > 0 ? this.SettingsModel.ShortBreakDuration : 5;
      set
      {
        if (this.SettingsModel.ShortBreakDuration == value)
          return;
        this.SettingsModel.ShortBreakDuration = value;
        TickFocusManager.OnPomoSettingsChanged();
        this._isChanged = true;
      }
    }

    public int LongBreakDuration
    {
      get => this.SettingsModel.LongBreakDuration > 0 ? this.SettingsModel.LongBreakDuration : 10;
      set
      {
        if (this.SettingsModel.LongBreakDuration == value)
          return;
        this.SettingsModel.LongBreakDuration = value;
        TickFocusManager.OnPomoSettingsChanged();
        this._isChanged = true;
      }
    }

    public int LongBreakEvery
    {
      get => this.SettingsModel.LongBreakEvery;
      set
      {
        this.SettingsModel.LongBreakEvery = value;
        this.OnPropertyChanged(nameof (LongBreakEvery));
      }
    }

    public bool AutoNextPomo
    {
      get => this.PomoLocalSetting.AutoNextPomo;
      set
      {
        this.PomoLocalSetting.AutoNextPomo = value;
        this.OnPropertyChanged(nameof (AutoNextPomo));
      }
    }

    public bool AutoBreak
    {
      get => this.PomoLocalSetting.AutoBreak;
      set
      {
        this.PomoLocalSetting.AutoBreak = value;
        this.OnPropertyChanged(nameof (AutoBreak));
      }
    }

    public bool ShowDailyPomo
    {
      get => this.SettingsModel.ShowDailyPomo;
      set
      {
        this.SettingsModel.ShowDailyPomo = value;
        this.OnPropertyChanged(nameof (ShowDailyPomo));
        ticktick_WPF.Notifier.GlobalEventManager.NotifyPomoDailyGoalChanged();
      }
    }

    public bool ShowDailyDuration
    {
      get => this.SettingsModel.ShowDailyDuration;
      set
      {
        this.SettingsModel.ShowDailyDuration = value;
        this.OnPropertyChanged(nameof (ShowDailyDuration));
        ticktick_WPF.Notifier.GlobalEventManager.NotifyPomoDailyGoalChanged();
      }
    }

    public string PomoShortcut
    {
      get => this.SettingsModel.PomoShortcut;
      set
      {
        this.SettingsModel.PomoShortcut = value;
        this.OnPropertyChanged(nameof (PomoShortcut));
      }
    }

    public string CreateStickyShortCut
    {
      get => this.SettingsModel.CreateStickyShortCut;
      set
      {
        this.SettingsModel.CreateStickyShortCut = value;
        this.OnPropertyChanged(nameof (CreateStickyShortCut));
      }
    }

    public string ShowHideStickyShortCut
    {
      get => this.ExtraSettings.ShowHideStickyShortCut;
      set
      {
        this.ExtraSettings.ShowHideStickyShortCut = value;
        this.OnPropertyChanged(nameof (ShowHideStickyShortCut));
      }
    }

    public int DailyPomoGoals
    {
      get => this.SettingsModel.DailyPomoGoals;
      set
      {
        this.SettingsModel.DailyPomoGoals = value;
        this.OnPropertyChanged(nameof (DailyPomoGoals));
        ticktick_WPF.Notifier.GlobalEventManager.NotifyPomoDailyGoalChanged();
      }
    }

    public int PomoWindowLeft
    {
      get => this.PomoLocalSetting.PomoWindowLeft;
      set
      {
        this.PomoLocalSetting.PomoWindowLeft = value;
        this.OnPropertyChanged(nameof (PomoWindowLeft));
      }
    }

    public int PomoWindowTop
    {
      get => this.PomoLocalSetting.PomoWindowTop;
      set
      {
        this.PomoLocalSetting.PomoWindowTop = value;
        this.OnPropertyChanged(nameof (PomoWindowTop));
      }
    }

    public bool PomoTopMost
    {
      get => this.PomoLocalSetting.PomoTopMost;
      set
      {
        this.PomoLocalSetting.PomoTopMost = value;
        this.OnPropertyChanged(nameof (PomoTopMost));
      }
    }

    public bool PomoExpand
    {
      get => this.PomoLocalSetting.PomoExpand;
      set
      {
        this.PomoLocalSetting.PomoExpand = value;
        this.OnPropertyChanged(nameof (PomoExpand));
      }
    }

    public double ProjectPanelWidth
    {
      get => this.SettingsModel.ProjectPanelWidth;
      set
      {
        this.SettingsModel.ProjectPanelWidth = value;
        this.OnPropertyChanged(nameof (ProjectPanelWidth));
      }
    }

    public double DetailListDivide
    {
      get => this.SettingsModel.DetailListDivide;
      set
      {
        this.SettingsModel.DetailListDivide = value;
        this.OnPropertyChanged(nameof (DetailListDivide));
      }
    }

    public int SortOrderOfAll
    {
      get => this.SettingsModel.SortOrderOfAll;
      set => this.SettingsModel.SortOrderOfAll = value;
    }

    public int SortOrderOfToday
    {
      get => this.SettingsModel.SortOrderOfToday;
      set => this.SettingsModel.SortOrderOfToday = value;
    }

    public int SortOrderOfTomorrow
    {
      get => this.SettingsModel.SortOrderOfTomorrow;
      set => this.SettingsModel.SortOrderOfTomorrow = value;
    }

    public int SortOrderOfWeek
    {
      get => this.SettingsModel.SortOrderOfWeek;
      set => this.SettingsModel.SortOrderOfWeek = value;
    }

    public int SortOrderOfCalendar
    {
      get => this.SettingsModel.SortOrderOfCalendar;
      set => this.SettingsModel.SortOrderOfCalendar = value;
    }

    public int SortOrderOfTag
    {
      get => this.SettingsModel.SortOrderOfTag;
      set => this.SettingsModel.SortOrderOfTag = value;
    }

    public int SortOrderOfEvent
    {
      get => this.SettingsModel.SortOrderOfEvent;
      set => this.SettingsModel.SortOrderOfEvent = value;
    }

    public int SortOrderOfInbox
    {
      get => this.SettingsModel.SortOrderOfInbox;
      set => this.SettingsModel.SortOrderOfInbox = value;
    }

    public int SortOrderOfSummary
    {
      get => this.ExtraSettings.SortOrderOfSummary;
      set => this.ExtraSettings.SortOrderOfSummary = value;
    }

    public int SortOrderOfAssign
    {
      get => this.SettingsModel.SortOrderOfAssign;
      set
      {
        this.SettingsModel.SortOrderOfAssign = value;
        this.OnPropertyChanged(nameof (SortOrderOfAssign));
      }
    }

    public bool ShowReminder
    {
      get => this.SettingsModel.ShowReminder;
      set
      {
        this.SettingsModel.ShowReminder = value;
        this.OnPropertyChanged(nameof (ShowReminder));
      }
    }

    public bool ShowReminderInClient
    {
      get => this.SettingsModel.ShowReminderInClient;
      set
      {
        this.SettingsModel.ShowReminderInClient = value;
        this.OnPropertyChanged(nameof (ShowReminderInClient));
      }
    }

    public string AutoTagUserIds
    {
      get => this.SettingsModel.AutoTagUserIds;
      set
      {
        this.SettingsModel.AutoTagUserIds = value;
        this.OnPropertyChanged(nameof (AutoTagUserIds));
      }
    }

    public string SelectProjectId
    {
      get => this.SettingsModel.SelectProjectId;
      set
      {
        this.SettingsModel.SelectProjectId = value;
        this.OnPropertyChanged(nameof (SelectProjectId));
      }
    }

    public int ProxyType
    {
      get => this.Common.ProxyType;
      set
      {
        this.Common.ProxyType = value;
        this._isChanged = true;
      }
    }

    public string ProxyAddress
    {
      get => this.Common.ProxyAddress;
      set
      {
        this.Common.ProxyAddress = value;
        this._isChanged = true;
      }
    }

    public string ProxyPort
    {
      get => this.Common.ProxyPort;
      set
      {
        this.Common.ProxyPort = value;
        this._isChanged = true;
      }
    }

    public string ProxyUsername
    {
      get => this.Common.ProxyUsername;
      set
      {
        this.Common.ProxyUsername = value;
        this._isChanged = true;
      }
    }

    public string ProxyPassword
    {
      get => this.Common.ProxyPassword;
      set
      {
        this.Common.ProxyPassword = value;
        this._isChanged = true;
      }
    }

    public string ProxyDomain
    {
      get => this.Common.ProxyDomain;
      set
      {
        this.Common.ProxyDomain = value;
        this._isChanged = true;
      }
    }

    public bool NeedResetPassword
    {
      get => this.SettingsModel.NeedResetPassword;
      set
      {
        this.SettingsModel.NeedResetPassword = value;
        this.OnPropertyChanged(nameof (NeedResetPassword));
      }
    }

    public string LockShortcut
    {
      get => this.SettingsModel.LockShortcut;
      set
      {
        this.SettingsModel.LockShortcut = value;
        this.OnPropertyChanged(nameof (LockShortcut));
      }
    }

    public string AccountType
    {
      get => this.Common.AccountType;
      set => this.Common.AccountType = value;
    }

    public string NeedResetUserId
    {
      get => this.SettingsModel.NeedResetUserId;
      set
      {
        this.SettingsModel.NeedResetUserId = value;
        this.OnPropertyChanged(nameof (NeedResetUserId));
      }
    }

    public bool NeedShowTutorial
    {
      get => this.SettingsModel.NeedShowTutorial;
      set
      {
        this.SettingsModel.NeedShowTutorial = value;
        this.OnPropertyChanged(nameof (NeedShowTutorial));
      }
    }

    public bool Maxmized
    {
      get => this.SettingsModel.Maxmized;
      set
      {
        this.SettingsModel.Maxmized = value;
        this.OnPropertyChanged(nameof (Maxmized));
      }
    }

    public string SkipVersion
    {
      get => this.Common.SkipVersion;
      set
      {
        this.Common.SkipVersion = value;
        this.OnPropertyChanged(nameof (SkipVersion));
      }
    }

    public double CheckRemindDate
    {
      get => this.SettingsModel.CheckRemindDate;
      set
      {
        this.SettingsModel.CheckRemindDate = value;
        this.OnPropertyChanged(nameof (CheckRemindDate));
      }
    }

    public string ShortcutPin
    {
      get => this.SettingsModel.ShortcutPin;
      set
      {
        this.SettingsModel.ShortcutPin = value;
        this.OnPropertyChanged(nameof (ShortcutPin));
      }
    }

    public bool ShowTimelineTooltip
    {
      get => this.SettingsModel.ShowTimelineTooltip;
      set
      {
        this.SettingsModel.ShowTimelineTooltip = value;
        this.OnPropertyChanged(nameof (ShowTimelineTooltip));
      }
    }

    public string LastCheckSyncDate
    {
      get => this.SettingsModel.LastCheckSyncDate;
      set
      {
        this.SettingsModel.LastCheckSyncDate = value;
        this.OnPropertyChanged(nameof (LastCheckSyncDate));
      }
    }

    public string RecentProjects
    {
      get => this.SettingsModel.RecentProjects;
      set
      {
        this.SettingsModel.RecentProjects = value;
        this.OnPropertyChanged(nameof (RecentProjects));
      }
    }

    public int CollapsedStart
    {
      get => this.SettingsModel.CollapsedStart;
      set
      {
        this.SettingsModel.CollapsedStart = value;
        this.OnPropertyChanged(nameof (CollapsedStart));
      }
    }

    public int CollapsedEnd
    {
      get => this.SettingsModel.CollapsedEnd;
      set
      {
        this.SettingsModel.CollapsedEnd = value;
        this.OnPropertyChanged(nameof (CollapsedEnd));
      }
    }

    public bool ShowDetails
    {
      get => this.SettingsModel.ShowDetails || this._inSearch;
      set
      {
        this.SettingsModel.ShowDetails = value;
        this.OnPropertyChanged(nameof (ShowDetails));
      }
    }

    public string AllDayCustomRemind
    {
      get => this.SettingsModel.AllDayCustomRemind;
      set
      {
        this.SettingsModel.AllDayCustomRemind = value;
        this.OnPropertyChanged(nameof (AllDayCustomRemind));
      }
    }

    public string TimeCustomRemind
    {
      get => this.SettingsModel.TimeCustomRemind;
      set
      {
        this.SettingsModel.TimeCustomRemind = value;
        this.OnPropertyChanged(nameof (TimeCustomRemind));
      }
    }

    public bool ExpandPersonalSection
    {
      get => this.SettingsModel.ExpandPersonalSection;
      set
      {
        this.SettingsModel.ExpandPersonalSection = value;
        this.OnPropertyChanged(nameof (ExpandPersonalSection));
      }
    }

    public int SmartListSummary
    {
      get
      {
        if (this.SettingsModel.SmartListSummary < 0)
          this.SettingsModel.SmartListSummary = 0;
        return this.SettingsModel.SmartListSummary;
      }
      set
      {
        this.SettingsModel.SmartListSummary = value;
        this.OnPropertyChanged(nameof (SmartListSummary));
      }
    }

    public string CompletionSound
    {
      get => this.SettingsModel.CompletionSound;
      set
      {
        this.SettingsModel.CompletionSound = value;
        this.OnPropertyChanged(nameof (CompletionSound));
      }
    }

    public bool UpdateHabit
    {
      get => this.SettingsModel.UpdateHabit;
      set
      {
        this.SettingsModel.UpdateHabit = value;
        this.OnPropertyChanged("UpdateCalendar");
      }
    }

    public bool EnableRingtone => this.SettingsModel.EnableRingtone;

    public double WindowTop
    {
      get => this.SettingsModel.WindowTop;
      set
      {
        this.SettingsModel.WindowTop = value;
        this.OnPropertyChanged(nameof (WindowTop));
      }
    }

    public double WindowLeft
    {
      get => this.SettingsModel.WindowLeft;
      set
      {
        this.SettingsModel.WindowLeft = value;
        this.OnPropertyChanged(nameof (WindowLeft));
      }
    }

    public bool IsDarkTheme => this.ThemeId == "Dark";

    public string ThemeId
    {
      get => !(this.SettingsModel.ThemeId == "Black") ? this.SettingsModel.ThemeId : "Gray";
      set
      {
        this.SettingsModel.ThemeId = value;
        this.OnPropertyChanged(nameof (ThemeId));
      }
    }

    public string LastProRemindeTime
    {
      get => this.SettingsModel.LastProRemindeTime;
      set
      {
        this.SettingsModel.LastProRemindeTime = value;
        this.OnPropertyChanged(nameof (LastProRemindeTime));
      }
    }

    public double CalendarMinMinute
    {
      get => this.SettingsModel.CalendarMinMinute;
      set
      {
        this.SettingsModel.CalendarMinMinute = Math.Round(value);
        this.OnPropertyChanged(nameof (CalendarMinMinute));
      }
    }

    public double CalendarHourHeight
    {
      get => LocalSettings.GetValidHeight(this.SettingsModel.CalendarHourHeight);
      set
      {
        this.SettingsModel.CalendarHourHeight = Math.Round(value);
        this.OnPropertyChanged(nameof (CalendarHourHeight));
      }
    }

    public int ArrangeDisplayType
    {
      get => this.SettingsModel.ArrangeDisplayType;
      set
      {
        this.SettingsModel.ArrangeDisplayType = value;
        ticktick_WPF.Notifier.GlobalEventManager.NotifyArrangeDisplayTypeChanged();
      }
    }

    public int ArrangeTaskDateType
    {
      get => this.SettingsModel.ArrangeTaskDateType;
      set
      {
        this.SettingsModel.ArrangeTaskDateType = value;
        this.OnPropertyChanged(nameof (ArrangeTaskDateType));
      }
    }

    public double WeekAllDayHeight
    {
      get => this.SettingsModel.WeekAllDayHeight;
      set
      {
        this.SettingsModel.WeekAllDayHeight = value;
        this.OnPropertyChanged(nameof (WeekAllDayHeight));
      }
    }

    public bool ShowArrangeReminder
    {
      get => this.SettingsModel.ShowArrangeReminder;
      set
      {
        this.SettingsModel.ShowArrangeReminder = value;
        this.OnPropertyChanged(nameof (ShowArrangeReminder));
      }
    }

    public string PomoType
    {
      get
      {
        return !string.IsNullOrEmpty(this.PomoLocalSetting.PomoType) ? this.PomoLocalSetting.PomoType : FocusConstance.Focus;
      }
      set
      {
        this.PomoLocalSetting.PomoType = value ?? FocusConstance.Focus;
        this.OnPropertyChanged(nameof (PomoType));
      }
    }

    public long UpgradeCheckPoint
    {
      get => this.SettingsModel.UpgradeCheckPoint;
      set
      {
        this.SettingsModel.UpgradeCheckPoint = value;
        this._isChanged = true;
      }
    }

    public int DailyFocusDuration
    {
      get => this.SettingsModel.DailyFocusDuration;
      set
      {
        this.SettingsModel.DailyFocusDuration = value;
        ticktick_WPF.Notifier.GlobalEventManager.NotifyPomoDailyGoalChanged();
      }
    }

    public bool HabitInToday
    {
      get => this.SettingsModel.HabitInToday;
      set
      {
        this.SettingsModel.HabitInToday = value;
        DataChangedNotifier.NotifyHabitsChanged();
        this.OnPropertyChanged(nameof (HabitInToday));
      }
    }

    public bool HabitInCal
    {
      get => this.SettingsModel.HabitInCal;
      set
      {
        this.SettingsModel.HabitInCal = value;
        this.OnPropertyChanged(nameof (HabitInCal));
      }
    }

    public long HabitDefaultOrder
    {
      get => this.SettingsModel.HabitDefaultOrder;
      set
      {
        this.SettingsModel.HabitDefaultOrder = value;
        this.OnPropertyChanged(nameof (HabitDefaultOrder));
      }
    }

    public bool ShowHabit => TabBarModel.IsActive("HABIT", this.DesktopConfig?.bars);

    public bool EnableFocus => TabBarModel.IsActive("POMO", this.DesktopConfig?.bars);

    public bool ShowCountDown
    {
      get => this.SettingsModel.ShowCountDown;
      set
      {
        this.SettingsModel.ShowCountDown = value;
        this.OnPropertyChanged(nameof (ShowCountDown));
      }
    }

    public bool DateParsing
    {
      get => this.SettingsModel.DateParsing;
      set
      {
        this.SettingsModel.DateParsing = value;
        this.OnPropertyChanged(nameof (DateParsing));
      }
    }

    public bool RemoveTimeText
    {
      get => this.SettingsModel.RemoveTimeText;
      set
      {
        this.SettingsModel.RemoveTimeText = value;
        this.OnPropertyChanged(nameof (RemoveTimeText));
      }
    }

    public bool KeepTagsInText
    {
      get => this.SettingsModel.KeepTagsInText;
      set
      {
        this.SettingsModel.KeepTagsInText = value;
        this.OnPropertyChanged(nameof (KeepTagsInText));
      }
    }

    public bool HideComplete
    {
      get => this.SettingsModel.HideComplete;
      set
      {
        int num1 = LocalSettings.Settings.HideComplete ? 1 : 0;
        this.SettingsModel.HideComplete = value;
        this.OnPropertyChanged(nameof (HideComplete));
        int num2 = value ? 1 : 0;
        if (num1 == num2)
          return;
        DataChangedNotifier.NotifyHideCompleteChanged();
      }
    }

    public bool ShowRepeatCircles
    {
      get => this.SettingsModel.ShowRepeatCircles;
      set
      {
        this.SettingsModel.ShowRepeatCircles = value;
        this.OnPropertyChanged(nameof (ShowRepeatCircles));
      }
    }

    public bool ShowFocusRecord
    {
      get => this.SettingsModel.ShowFocusRecord;
      set
      {
        this.SettingsModel.ShowFocusRecord = value;
        this.OnPropertyChanged(nameof (ShowFocusRecord));
      }
    }

    public bool ShowSubtasks
    {
      get => this.SettingsModel.ShowSubtasks;
      set
      {
        this.SettingsModel.ShowSubtasks = value;
        this.OnPropertyChanged(nameof (ShowSubtasks));
      }
    }

    public string WeekStartFrom
    {
      get => this.SettingsModel.WeekStartFrom;
      set
      {
        if (!(this.SettingsModel.WeekStartFrom != value))
          return;
        this.SettingsModel.WeekStartFrom = value;
        this.OnPropertyChanged(nameof (WeekStartFrom));
        DataChangedNotifier.NotifyWeekStartFromChanged();
      }
    }

    public string TimeFormat
    {
      get
      {
        return !(this.SettingsModel.TimeFormat == "System") ? this.SettingsModel.TimeFormat : ticktick_WPF.Util.DateUtils.GetSystemTimeFormat();
      }
      set
      {
        this.SettingsModel.TimeFormat = value;
        ticktick_WPF.Notifier.GlobalEventManager.NotifyTimeFormatChanged();
        this.OnPropertyChanged(nameof (TimeFormat));
      }
    }

    public int PosOfOverdue
    {
      get => this.SettingsModel.PosOfOverdue;
      set
      {
        this.SettingsModel.PosOfOverdue = value;
        this.OnPropertyChanged(nameof (PosOfOverdue));
      }
    }

    public string NotificationOptions
    {
      get => this.SettingsModel.NotificationOptions;
      set
      {
        this.SettingsModel.NotificationOptions = value;
        this.OnPropertyChanged(nameof (NotificationOptions));
      }
    }

    public bool EnableHoliday
    {
      get => this.SettingsModel.EnableHoliday;
      set
      {
        this.SettingsModel.EnableHoliday = value;
        this.OnPropertyChanged(nameof (EnableHoliday));
      }
    }

    public bool EnableLunar
    {
      get => this.SettingsModel.EnableLunar;
      set
      {
        this.SettingsModel.EnableLunar = value;
        this.OnPropertyChanged(nameof (EnableLunar));
      }
    }

    public bool MiniCalendarEnabled
    {
      get => this.ExtraSettings.MiniCalendarEnabled;
      set
      {
        this.ExtraSettings.MiniCalendarEnabled = value;
        this.OnPropertyChanged(nameof (MiniCalendarEnabled));
      }
    }

    public bool ShowWeek
    {
      get => this.SettingsModel.ShowWeek;
      set
      {
        this.SettingsModel.ShowWeek = value;
        this.OnPropertyChanged(nameof (ShowWeek));
      }
    }

    public bool ShowCheckListInCal
    {
      get => this.SettingsModel.ShowCheckListInCal;
      set
      {
        this.SettingsModel.ShowCheckListInCal = value;
        this.OnPropertyChanged(nameof (ShowCheckListInCal));
      }
    }

    public bool ShowCompletedInCal
    {
      get => this.SettingsModel.ShowCompletedInCal;
      set
      {
        this.SettingsModel.ShowCompletedInCal = value;
        this.OnPropertyChanged(nameof (ShowCompletedInCal));
      }
    }

    public string CellColorType
    {
      get => this.SettingsModel.CellColorType ?? "list";
      set
      {
        this.SettingsModel.CellColorType = value ?? "list";
        this.OnPropertyChanged(nameof (CellColorType));
      }
    }

    public bool EnableTimeZone
    {
      get => this.SettingsModel.EnableTimeZone;
      set
      {
        this.SettingsModel.EnableTimeZone = value;
        this.OnPropertyChanged(nameof (EnableTimeZone));
      }
    }

    public bool InSearch
    {
      get => this._inSearch;
      set => this._inSearch = value;
    }

    public int SmartListHabit => !LocalSettings.Settings.ShowHabit ? 1 : 0;

    public bool IsNoteEnabled
    {
      get => this.SettingsModel.IsNoteEnabled;
      set
      {
        this.SettingsModel.IsNoteEnabled = value;
        this._isChanged = true;
      }
    }

    public string StartWeekOfYear
    {
      get => this.SettingsModel.StartWeekOfYear;
      set
      {
        this.SettingsModel.StartWeekOfYear = value;
        this.OnPropertyChanged(nameof (StartWeekOfYear));
      }
    }

    public bool ProjectTypeChangeNotified
    {
      get => this.SettingsModel.ProjectTypeChangeNotified;
      set
      {
        this.SettingsModel.ProjectTypeChangeNotified = value;
        this.OnPropertyChanged(nameof (ProjectTypeChangeNotified));
      }
    }

    public bool DontShowProWindow
    {
      get => this.SettingsModel.DontShowProWindow;
      set
      {
        this.SettingsModel.DontShowProWindow = value;
        this._isChanged = true;
      }
    }

    public bool SpellCheckEnable
    {
      get => this.SettingsModel.SpellCheckEnable;
      set
      {
        this.SettingsModel.SpellCheckEnable = value;
        EventHandler spellCheckChanged = LocalSettings.SpellCheckChanged;
        if (spellCheckChanged != null)
          spellCheckChanged((object) this, (EventArgs) null);
        this._isChanged = true;
      }
    }

    public string SpellCheckLanguage
    {
      get => this.SettingsModel.SpellCheckLanguage;
      set
      {
        this.SettingsModel.SpellCheckLanguage = value;
        EventHandler spellCheckChanged = LocalSettings.SpellCheckChanged;
        if (spellCheckChanged != null)
          spellCheckChanged((object) this, (EventArgs) null);
        this._isChanged = true;
      }
    }

    public int AutoPomoTimes
    {
      get => this.PomoLocalSetting.AutoPomoTimes;
      set
      {
        this.PomoLocalSetting.AutoPomoTimes = value;
        this.OnPropertyChanged("PomoWindowTheme");
      }
    }

    public double PomoWindowOpacity
    {
      get => this.PomoLocalSetting.PomoWindowOpacity;
      set
      {
        this.PomoLocalSetting.PomoWindowOpacity = value;
        this.OnPropertyChanged(nameof (PomoWindowOpacity));
      }
    }

    public string PomoWindowTheme
    {
      get => this.PomoLocalSetting.PomoWindowTheme;
      set
      {
        this.PomoLocalSetting.PomoWindowTheme = value;
        ticktick_WPF.Notifier.GlobalEventManager.NotifyPomoThemeChanged(value);
        this._isChanged = true;
      }
    }

    public bool EnableCountDown
    {
      get => this.SettingsModel.EnableCountDown;
      set => this.SettingsModel.EnableCountDown = value;
    }

    public int MainWindowDisplayModule
    {
      get => this.SettingsModel.MainWindowDisplayModule;
      set
      {
        this.SettingsModel.MainWindowDisplayModule = value;
        this.OnPropertyChanged(nameof (MainWindowDisplayModule));
      }
    }

    public bool AllProjectOpened
    {
      get => this.ProjectListOpenStatus.AllProjectOpened;
      set
      {
        this.ProjectListOpenStatus.AllProjectOpened = value;
        this._isChanged = true;
      }
    }

    public bool AllTagOpened
    {
      get => this.ProjectListOpenStatus.AllTagOpened;
      set
      {
        this.ProjectListOpenStatus.AllTagOpened = value;
        this._isChanged = true;
      }
    }

    public bool AllShareTagOpened
    {
      get => this.ProjectListOpenStatus.AllShareTagOpened;
      set
      {
        this.ProjectListOpenStatus.AllShareTagOpened = value;
        this._isChanged = true;
      }
    }

    public bool AllFilterOpened
    {
      get => this.ProjectListOpenStatus.AllFilterOpened;
      set
      {
        this.ProjectListOpenStatus.AllFilterOpened = value;
        this._isChanged = true;
      }
    }

    public bool AllSubscribeOpened
    {
      get => this.ProjectListOpenStatus.AllSubscribeOpened;
      set
      {
        this.ProjectListOpenStatus.AllSubscribeOpened = value;
        this._isChanged = true;
      }
    }

    public SummaryFilterModel SummaryFilter
    {
      get
      {
        if (this._summaryFilter == null)
        {
          if (!string.IsNullOrEmpty(LocalSettings.Settings.SettingsModel.SummaryFilter))
          {
            try
            {
              this._summaryFilter = JsonConvert.DeserializeObject<SummaryFilterModel>(LocalSettings.Settings.SettingsModel.SummaryFilter) ?? new SummaryFilterModel();
            }
            catch (Exception ex)
            {
              this._summaryFilter = new SummaryFilterModel();
            }
          }
        }
        this._summaryFilter = this._summaryFilter ?? new SummaryFilterModel();
        return this._summaryFilter;
      }
    }

    public List<SmartProjectConf> SmartProjects
    {
      get
      {
        return this._smartProjects == null ? (List<SmartProjectConf>) null : this._smartProjects.GroupBy<SmartProjectConf, string>((Func<SmartProjectConf, string>) (proj => proj.name)).Select<IGrouping<string, SmartProjectConf>, SmartProjectConf>((Func<IGrouping<string, SmartProjectConf>, SmartProjectConf>) (g => g.First<SmartProjectConf>())).ToList<SmartProjectConf>();
      }
      set
      {
        this._smartProjects = value;
        ProjectDataProvider.ReSortPtfTypes();
        this.OnPropertyChanged(nameof (SmartProjects));
      }
    }

    public void SetSmartProjects(string smartProjects)
    {
      try
      {
        this._smartProjects = JsonConvert.DeserializeObject<List<SmartProjectConf>>(smartProjects);
        ProjectDataProvider.ReSortPtfTypes();
      }
      catch
      {
      }
    }

    public void AddSmartProjectConf(SmartProjectConf smartProjectConf)
    {
      if (this._smartProjects == null)
      {
        this._smartProjects = new List<SmartProjectConf>()
        {
          smartProjectConf
        };
      }
      else
      {
        SmartProjectConf smartProjectConf1 = this._smartProjects.FirstOrDefault<SmartProjectConf>((Func<SmartProjectConf, bool>) (p => p.name == smartProjectConf.name));
        if (smartProjectConf1 != null)
          smartProjectConf1.order = smartProjectConf.order;
        else
          this._smartProjects.Add(smartProjectConf);
      }
    }

    public bool LightsOn
    {
      get => this.PomoLocalSetting.LightsOn;
      set => this.PomoLocalSetting.LightsOn = value;
    }

    public bool TaskSetFolded
    {
      get => this.ShortCutModel.TaskSetFolded != "0";
      set
      {
        this.ShortCutModel.TaskSetFolded = value ? "1" : "0";
        this.OnPropertyChanged(nameof (TaskSetFolded));
      }
    }

    public bool TaskOperateFolded
    {
      get => this.ShortCutModel.TaskOperateFolded != "0";
      set
      {
        this.ShortCutModel.TaskOperateFolded = value ? "1" : "0";
        this.OnPropertyChanged(nameof (TaskOperateFolded));
      }
    }

    public bool GlobalOperateFolded
    {
      get => this.ShortCutModel.GlobalOperateFolded != "0";
      set
      {
        this.ShortCutModel.GlobalOperateFolded = value ? "1" : "0";
        this.OnPropertyChanged(nameof (GlobalOperateFolded));
      }
    }

    public bool AppGlobalOperateFolded
    {
      get => this.ShortCutModel.AppGlobalOperateFolded != "0";
      set
      {
        this.ShortCutModel.AppGlobalOperateFolded = value ? "1" : "0";
        this.OnPropertyChanged(nameof (AppGlobalOperateFolded));
      }
    }

    public bool SetTimeFolded
    {
      get => this.ShortCutModel.SetTimeFolded != "0";
      set
      {
        this.ShortCutModel.SetTimeFolded = value ? "1" : "0";
        this.OnPropertyChanged(nameof (SetTimeFolded));
      }
    }

    public bool SetPriorityFolded
    {
      get => this.ShortCutModel.SetPriorityFolded != "0";
      set
      {
        this.ShortCutModel.SetPriorityFolded = value ? "1" : "0";
        this.OnPropertyChanged(nameof (SetPriorityFolded));
      }
    }

    public bool QuickJumpFolded
    {
      get => this.ShortCutModel.QuickJumpFolded != "0";
      set
      {
        this.ShortCutModel.QuickJumpFolded = value ? "1" : "0";
        this.OnPropertyChanged(nameof (QuickJumpFolded));
      }
    }

    public string RecentUsedEmojis
    {
      get => this.SettingsModel.RecentUsedEmojis;
      set => this.SettingsModel.RecentUsedEmojis = value;
    }

    public long DomainInvalidTime
    {
      get => this.Common.DomainInvalidTime;
      set => this.Common.DomainInvalidTime = value;
    }

    public double ProjectTextShadowOpacity
    {
      get => this._projectTextShadowOpacity;
      set
      {
        this._projectTextShadowOpacity = value;
        this.OnPropertyChanged(nameof (ProjectTextShadowOpacity));
      }
    }

    public bool ProjectPinFolded
    {
      get => this._projectPinFolded;
      set
      {
        this._projectPinFolded = value;
        this.OnPropertyChanged(nameof (ProjectPinFolded));
      }
    }

    public long FocusPushPoint
    {
      get => this.ExtraSettings.FocusPushPoint;
      set
      {
        this.ExtraSettings.FocusPushPoint = value;
        this.OnPropertyChanged(nameof (FocusPushPoint));
      }
    }

    public int BaseFontSize
    {
      get => this.Common.BaseFontSize;
      set => this.Common.BaseFontSize = value;
    }

    public string CheckedNewFeature
    {
      get => this.SettingsModel.CheckedNewFeature ?? string.Empty;
      set => this.SettingsModel.CheckedNewFeature = value;
    }

    public bool ShowCalWeekend
    {
      get => this.ExtraSettings.ShowCalWeekend;
      set
      {
        this.ExtraSettings.ShowCalWeekend = value;
        this.OnPropertyChanged(nameof (ShowCalWeekend));
      }
    }

    public void SetSummaryFilter(SummaryFilterModel value)
    {
      this._summaryFilter = value;
      this._isChanged = true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static event EventHandler SpellCheckChanged;

    public static double GetValidHeight(double value)
    {
      return (double) Math.Min(192, Math.Max(32, (int) value));
    }

    protected void OnPropertyChanged(string propertyName = null)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged != null)
        propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
      this._isChanged = true;
    }

    public void SetChanged() => this._isChanged = true;

    public async Task Save(bool force = false)
    {
      LocalSettings localSettings = this;
      if (!localSettings._isChanged && !force || localSettings.Common == null || localSettings.SettingsModel == null || App.Connection == null)
        return;
      // ISSUE: reference to a compiler-generated method
      Task task = await Dispatcher.CurrentDispatcher.InvokeAsync<Task>(new Func<Task>(localSettings.\u003CSave\u003Eb__624_0));
    }

    public void OnTagNameChanged(string original, string revised)
    {
      string calendarFilterData = this.CalendarFilterData;
      if ((calendarFilterData != null ? (calendarFilterData.Contains(original) ? 1 : 0) : 0) != 0)
      {
        ProjectExtra data = ProjectExtra.Deserialize(this.CalendarFilterData, false);
        if (data.Tags.Contains(original))
        {
          data.Tags.Remove(original);
          data.Tags.Add(revised.ToLower());
        }
        this.CalendarFilterData = ProjectExtra.Serialize(data);
      }
      string arrangeTaskFilterData = this.ArrangeTaskFilterData;
      if ((arrangeTaskFilterData != null ? (arrangeTaskFilterData.Contains(original) ? 1 : 0) : 0) == 0)
        return;
      ProjectExtra projectExtra = ProjectExtra.Deserialize(this.ArrangeTaskFilterData, false);
      if (projectExtra.Tags.Contains(original))
      {
        projectExtra.Tags.Remove(original);
        projectExtra.Tags.Add(revised.ToLower());
      }
      this.ArrangeTaskFilterData = ProjectExtra.Serialize(projectExtra);
      ArrangeTaskPanel.ResetArrangeFilter(projectExtra);
    }

    public void NotifyPropertyChanged(string propertyName) => this.OnPropertyChanged(propertyName);

    public bool DomainInvalid()
    {
      return Utils.GetTimeStampInSecond(new DateTime?(DateTime.Now)) - this.DomainInvalidTime < 86400L;
    }

    public void ResetCustomTheme()
    {
      this.ThemeImageOpacity = 1.0;
      this.ThemeImageBlurRadius = 0;
      this.ShowAreaOpacity = 0.9;
      this.CustomThemeColor = string.Empty;
      this.CustomThemeLocation = string.Empty;
    }

    public void SaveCustomTheme()
    {
      this.Common.ThemeImageOpacity = this.ThemeImageOpacity;
      this.Common.ThemeImageBlurRadius = this.ThemeImageBlurRadius;
      this.Common.ShowAreaOpacity = this.ShowAreaOpacity;
      this.Common.CustomThemeLocation = this.CustomThemeLocation;
      this.Common.CustomThemeColor = this.CustomThemeColor;
      this._isChanged = true;
      this.Save();
    }

    public void LoadCustomTheme()
    {
      this.ThemeImageOpacity = this.Common.ThemeImageOpacity;
      this.ThemeImageBlurRadius = this.Common.ThemeImageBlurRadius;
      this.ShowAreaOpacity = this.Common.ShowAreaOpacity;
      this.CustomThemeLocation = this.Common.CustomThemeLocation;
      this.CustomThemeColor = this.Common.CustomThemeColor;
    }

    public void NotifyThemeChanged() => this.OnPropertyChanged("ThemeId");

    public static async Task ResetUserSettings(UserModel user, string accountType)
    {
      LocalSettings.ClearModel();
      SettingsModel settingsModel1 = await App.Connection.Table<SettingsModel>().Where((Expression<Func<SettingsModel, bool>>) (m => m.UserId == user.userId)).FirstOrDefaultAsync();
      if (settingsModel1 == null)
        settingsModel1 = new SettingsModel()
        {
          UserId = user.userId,
          WindowLeft = (SystemParameters.PrimaryScreenWidth - 1140.0) / 2.0,
          WindowTop = (SystemParameters.PrimaryScreenHeight - 780.0) / 2.0
        };
      SettingsModel settingsModel2 = settingsModel1;
      LocalSettings.Settings.SettingsModel = settingsModel2;
      LocalSettings.Settings._extraSettings = (ExtraSettings) null;
      LocalSettings.Settings.SetSmartProjects(LocalSettings.Settings.SettingsModel.SmartProjects);
      LocalSettings.Settings.SetPreference();
      LocalSettings.Settings.LoginUserAuth = user.token;
      LocalSettings.Settings.LoginUserName = user.username;
      LocalSettings.Settings.InServerBoxId = user.inboxId;
      LocalSettings.Settings.LoginUserId = user.userId;
      int num = await Utils.CreatInbox();
      LocalSettings.Settings.InBoxId = num;
      if (!string.IsNullOrEmpty(accountType))
        LocalSettings.Settings.AccountType = accountType;
      LocalSettings.Settings.Save();
    }

    public void SetRemoteTaskListDisplay(bool hideComplete, bool showChecklist)
    {
      bool flag = false;
      if (this.HideComplete != hideComplete)
      {
        this.HideComplete = hideComplete;
        flag = true;
      }
      if (this.ShowSubtasks != showChecklist)
      {
        this.ShowSubtasks = showChecklist;
        flag = true;
      }
      if (!flag)
        return;
      App.Window.Reload();
    }

    public static void ClearModel()
    {
      LocalSettings.Settings.SettingsModel = new SettingsModel();
      LocalSettings.Settings.UserPreference = new UserPreferenceModel();
      LocalSettings.Settings.CalendarDisplaySettings = (CalendarDisplaySettings) null;
      LocalSettings.Settings.ProjectListOpenStatus = (ProjectListOpenStatus) null;
      LocalSettings.Settings.ShortCutModel = (ShortcutModel) null;
      LocalSettings.Settings._pomoLocalSetting = (PomoLocalSetting) null;
      LocalSettings.Settings._summaryFilter = (SummaryFilterModel) null;
      LocalSettings.Settings._smartProjects = (List<SmartProjectConf>) null;
      LocalSettings.Settings._extraSettings = (ExtraSettings) null;
      LocalSettings.Settings._statisticsModel = (StatisticsModel) null;
    }

    public static async Task BeforeLogout()
    {
      LocalSettings settings1 = LocalSettings.Settings;
      DateTime dateTime = DateTime.Today.AddDays(-1.0);
      string str = dateTime.ToString("yyyy MM dd");
      settings1.LastProRemindeTime = str;
      LocalSettings settings2 = LocalSettings.Settings;
      dateTime = DateTime.Today;
      long ticks = dateTime.AddDays(-1.0).Ticks;
      settings2.LastPullYearPromoTime = ticks;
      LocalSettings.Settings.SettingsModel.PreferrenceMTime = 0L;
      await LocalSettings.Settings.Save();
    }

    public ExtraSettings ExtraSettings
    {
      get
      {
        if (this._extraSettings == null)
        {
          if (!string.IsNullOrEmpty(LocalSettings.Settings.SettingsModel.ExtraSettings))
          {
            try
            {
              this._extraSettings = JsonConvert.DeserializeObject<ExtraSettings>(LocalSettings.Settings.SettingsModel.ExtraSettings);
            }
            catch (Exception ex)
            {
              this._extraSettings = new ExtraSettings();
            }
          }
        }
        this._extraSettings = this._extraSettings ?? new ExtraSettings();
        return this._extraSettings;
      }
      set => this._extraSettings = value;
    }

    public long LastPullYearPromoTime
    {
      get
      {
        ExtraSettings extraSettings = this.ExtraSettings;
        return extraSettings == null ? 0L : extraSettings.LastPullYearPromoTime;
      }
      set
      {
        if (this.ExtraSettings == null)
          return;
        this.ExtraSettings.LastPullYearPromoTime = value;
      }
    }

    public FontFamily FontFamily
    {
      get => this._fontFamily;
      set
      {
        this._fontFamily = value;
        this.OnPropertyChanged(nameof (FontFamily));
      }
    }
  }
}
