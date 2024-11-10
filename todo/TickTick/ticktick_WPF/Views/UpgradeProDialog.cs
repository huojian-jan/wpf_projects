// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.UpgradeProDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;
using XamlAnimatedGif;

#nullable disable
namespace ticktick_WPF.Views
{
  public class UpgradeProDialog : MyWindow, IOkCancelWindow, IComponentConnector
  {
    private readonly ProType _type;
    private string _teamId;
    internal Border DisplayImage;
    internal Image GifImage;
    internal PathFigure InsidePath;
    internal TextBlock UpdateTitleTextBlock;
    internal TextBlock UpdateContentTextBlock;
    internal TextBlock UpdateDescExtraTextBlock;
    internal Button UpdateNowButton;
    internal Button KnowMoreText;
    internal TextBlock HintText;
    private bool _contentLoaded;

    public UpgradeProDialog() => this.InitializeComponent();

    public UpgradeProDialog(ProType type, bool imageExist, string name, string teamId = null)
    {
      this.InitializeComponent();
      this._type = type;
      if (type == ProType.CalendarWidget)
        type = ProType.CalendarView;
      if (type == ProType.StickyColor)
        type = ProType.PremiumThemes;
      if (type == ProType.FocusMiniStyle)
        this.DisplayImage.Height = 250.0;
      this.UpdateTitleTextBlock.Text = this.GetProTitle(type);
      string format = this.GetProContent(type);
      if (teamId != null)
      {
        TeamModel teamById = CacheManager.GetTeamById(teamId);
        this._teamId = teamId;
        this.KnowMoreText.Visibility = Visibility.Collapsed;
        format = string.Format(format, (object) teamById?.name);
      }
      this.UpdateContentTextBlock.Text = format;
      string str1 = Utils.GetString(type.ToString() + "ExtraProSummary", string.Empty);
      if (!string.IsNullOrEmpty(str1))
      {
        this.UpdateDescExtraTextBlock.Text = str1;
        this.UpdateDescExtraTextBlock.Visibility = Visibility.Visible;
      }
      string str2 = Utils.GetString(type.ToString() + "ExtraProHint", string.Empty);
      if (!string.IsNullOrEmpty(str2))
        this.HintText.Text = str2;
      if (!imageExist)
        return;
      try
      {
        this.GifImage.SetValue(AnimationBehavior.SourceUriProperty, (object) new Uri(AppPaths.UpgradeProDir + name));
      }
      catch (Exception ex)
      {
      }
    }

    private string GetProContent(ProType type)
    {
      if (type == ProType.MoreTeamGuest)
        return string.Format(Utils.GetString(type.ToString() + "ProSummary"), (object) LimitCache.GetProLimitByKey(Constants.LimitKind.ShareUserNumber), (object) Utils.GetAppName());
      return type == ProType.MoreListsUnlimited ? string.Format(Utils.GetString(type.ToString() + "ProSummary"), (object) LimitCache.GetProLimitByKey(Constants.LimitKind.ProjectNumber), (object) Utils.GetAppName()) : Utils.GetString(type.ToString() + "ProSummary");
    }

    private string GetProTitle(ProType type)
    {
      switch (type)
      {
        case ProType.MoreTeamGuest:
        case ProType.TaskActivities:
        case ProType.NoteActivities:
        case ProType.Matrix:
        case ProType.MatrixWidget:
        case ProType.TimeLine:
        case ProType.EmailReminder:
        case ProType.TeamShareLimit:
        case ProType.FocusAutoSync:
        case ProType.FocusMiniStyle:
          return Utils.GetString(type.ToString() + string.Empty);
        case ProType.MoreListsUnlimited:
          return Utils.GetString("MoreListsPro");
        default:
          return Utils.GetString(type.ToString() + "Pro");
      }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      this.DragMove();
      e.Handled = false;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

    private async void UpdateNowClick(object sender, RoutedEventArgs e)
    {
      UpgradeProDialog upgradeProDialog = this;
      string type = string.Empty;
      switch (upgradeProDialog._type)
      {
        case ProType.CalendarView:
          type = "calendar_view";
          break;
        case ProType.Duration:
          type = "time_duration";
          break;
        case ProType.Filter:
          type = "filter";
          break;
        case ProType.MoreSharingMembers:
          type = "share_count";
          break;
        case ProType.MoreTeamGuest:
          Utils.TryProcessStartUrlWithToken("#tu/" + upgradeProDialog._teamId);
          upgradeProDialog.Close();
          return;
        case ProType.MoreReminders:
          type = "reminder_count";
          break;
        case ProType.MoreLists:
          type = "list_count";
          break;
        case ProType.MoreSubTasks:
          type = "sub_task_count";
          break;
        case ProType.MoreTasks:
          type = "task_count";
          break;
        case ProType.TaskActivities:
        case ProType.NoteActivities:
          type = "task_activity";
          break;
        case ProType.ListActivities:
          type = "project_activity";
          break;
        case ProType.MoreAttachments:
          type = "upload_count";
          break;
        case ProType.ReminderForSubTasks:
          type = "sub_task_reminder";
          break;
        case ProType.PremiumThemes:
          type = "theme";
          break;
        case ProType.SubscribeCalendar:
          type = "subscribe_calendar";
          break;
        case ProType.GoogleEventSync:
          type = "subscribe_calendar";
          break;
        case ProType.PomoSound:
          type = "white_noises";
          break;
        case ProType.MoreHabits:
          type = "habit_count";
          break;
        case ProType.Matrix:
          type = "matrix";
          break;
        case ProType.MatrixWidget:
          type = "matrix_widget";
          break;
        case ProType.AppIcon:
          type = "app_icon";
          break;
        case ProType.TimeLine:
          type = "timeline";
          break;
        case ProType.Font:
          type = "fonts";
          break;
        case ProType.StickyColor:
          type = "sticky_note_color";
          break;
        case ProType.EmailReminder:
          type = "email_notifications";
          break;
        case ProType.MoreTimers:
          type = "timer_count";
          break;
        case ProType.MoreCharts:
          type = "timer_statistics";
          break;
        case ProType.TeamShareLimit:
          Utils.TryProcessStartUrlWithToken("#tu/" + upgradeProDialog._teamId);
          upgradeProDialog.Close();
          return;
        case ProType.CalendarWidget:
          type = "calendar_widget";
          break;
        case ProType.FocusAutoSync:
          type = "focus_sync";
          break;
        case ProType.FocusMiniStyle:
          type = "focus_mini_style";
          break;
        case ProType.SummaryStyle:
          type = "summary_contents";
          break;
        case ProType.SummaryTemplate:
          type = "summary_template";
          break;
        case ProType.MiniCalendar:
          type = "mini_calendar";
          break;
      }
      await Utils.StartUpgrade(type);
      upgradeProDialog.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (this._type == ProType.CalendarView && LocalSettings.Settings.MainWindowDisplayModule == 1)
        App.Window.MainCalendar.ShowProToast();
      base.OnClosing(e);
    }

    public void OnCancel() => this.Close();

    public void Ok() => this.Close();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/updatemessagewindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.DisplayImage = (Border) target;
          break;
        case 2:
          this.GifImage = (Image) target;
          break;
        case 3:
          this.InsidePath = (PathFigure) target;
          break;
        case 4:
          this.UpdateTitleTextBlock = (TextBlock) target;
          break;
        case 5:
          this.UpdateContentTextBlock = (TextBlock) target;
          break;
        case 6:
          this.UpdateDescExtraTextBlock = (TextBlock) target;
          break;
        case 7:
          this.UpdateNowButton = (Button) target;
          this.UpdateNowButton.Click += new RoutedEventHandler(this.UpdateNowClick);
          break;
        case 8:
          this.KnowMoreText = (Button) target;
          this.KnowMoreText.Click += new RoutedEventHandler(this.UpdateNowClick);
          break;
        case 9:
          this.HintText = (TextBlock) target;
          break;
        case 10:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CloseButton_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
