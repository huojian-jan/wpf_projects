// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.SubscribeCalendar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class SubscribeCalendar : UserControl, IComponentConnector
  {
    internal TextBlock SubscribeCalendarTitle;
    internal ItemsControl CalendarItems;
    internal Border MuteCalendarGrid;
    internal CheckBox MuteCheckbox;
    internal TextBlock MuteDesc;
    internal TextBlock SubscribeInOtherText;
    internal StackPanel AddFeedCodePanel;
    internal EscPopup SelectProjectPopup;
    internal ItemsControl FeedCodeItems;
    private bool _contentLoaded;

    public SubscribeCalendar()
    {
      this.InitializeComponent();
      this.LoadData();
      this.LoadProjectCodes();
      this.SubscribeInOtherText.Text = string.Format(Utils.GetString("SubscribeInOther"), (object) Utils.GetAppName());
      this.SubscribeCalendarTitle.Text = string.Format(Utils.GetString(nameof (SubscribeCalendarTitle)), (object) Utils.GetAppName());
      this.MuteDesc.Text = string.Format(Utils.GetString("calendar_event_disturb_desc"), (object) Utils.GetAppName());
      this.MuteCheckbox.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.DoNotDisturbInCalendar);
    }

    public async Task LoadData(bool needPull = true)
    {
      await this.LoadLocalCalendars();
      if (!needPull)
        return;
      await this.PullRemoteCalendars();
    }

    private async Task PullRemoteCalendars()
    {
      await SubscribeCalendar.PullSubscribeCalendars();
      await CalendarService.PullAccountCalendarsAndEvents(new DateTime?(DateTime.Today), new DateTime?(DateTime.Today));
      List<SubscribeCalendarViewModel> itemsSource = (List<SubscribeCalendarViewModel>) this.CalendarItems.ItemsSource;
      // ISSUE: explicit non-virtual call
      int count = itemsSource != null ? __nonvirtual (itemsSource.Count) : 0;
      List<SubscribeCalendarViewModel> localCalendars = await SubscribeCalendar.GetLocalCalendars();
      if (localCalendars.Count == count)
        return;
      this.CalendarItems.ItemsSource = (IEnumerable) localCalendars;
      this.MuteCalendarGrid.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task LoadLocalCalendars()
    {
      List<SubscribeCalendarViewModel> localCalendars = await SubscribeCalendar.GetLocalCalendars();
      this.CalendarItems.ItemsSource = (IEnumerable) localCalendars;
      this.MuteCalendarGrid.Visibility = localCalendars.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static async Task<List<SubscribeCalendarViewModel>> GetLocalCalendars()
    {
      List<SubscribeCalendarViewModel> items = new List<SubscribeCalendarViewModel>();
      List<BindCalendarAccountModel> calendarAccounts = await BindCalendarAccountDao.GetBindCalendarAccounts();
      // ISSUE: explicit non-virtual call
      if (calendarAccounts != null && __nonvirtual (calendarAccounts.Count) > 0)
      {
        HashSet<string> accountIds = new HashSet<string>();
        calendarAccounts.Sort((Comparison<BindCalendarAccountModel>) ((a, b) =>
        {
          int accountType1 = SubscribeCalendarHelper.GetAccountType(a);
          int accountType2 = SubscribeCalendarHelper.GetAccountType(b);
          return accountType1 == accountType2 && a.CreatedTime.HasValue ? a.CreatedTime.Value.CompareTo((object) b.CreatedTime) : accountType1.CompareTo(accountType2);
        }));
        calendarAccounts.ForEach((Action<BindCalendarAccountModel>) (account =>
        {
          if (string.IsNullOrEmpty(account.Account) || accountIds.Contains(account.Id))
          {
            App.Connection.DeleteAsync((object) account);
          }
          else
          {
            items.Add(new SubscribeCalendarViewModel(account));
            accountIds.Add(account.Id);
          }
        }));
      }
      List<CalendarSubscribeProfileModel> profiles = await CalendarSubscribeProfileDao.GetProfiles();
      if (profiles != null && profiles.Any<CalendarSubscribeProfileModel>())
      {
        profiles.Sort((Comparison<CalendarSubscribeProfileModel>) ((a, b) =>
        {
          DateTime? createdTime = a.CreatedTime;
          ref DateTime? local = ref createdTime;
          return !local.HasValue ? -1 : local.GetValueOrDefault().CompareTo((object) b.CreatedTime);
        }));
        profiles.ForEach((Action<CalendarSubscribeProfileModel>) (sub => items.Add(new SubscribeCalendarViewModel(sub))));
      }
      items.ForEach((Action<SubscribeCalendarViewModel>) (item => item.Parent = items));
      return items;
    }

    private static async Task PullSubscribeCalendars()
    {
      await CalendarService.PullSubscribeCalendars();
    }

    private void OnAddCalendarClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.SubscribeCalendar))
        return;
      SelectSubscribeTypeWindow subscribeTypeWindow = new SelectSubscribeTypeWindow(this);
      subscribeTypeWindow.Owner = Window.GetWindow((DependencyObject) this);
      subscribeTypeWindow.Show();
    }

    private async Task LoadProjectCodes()
    {
      ObservableCollection<ProjectFeedViewModel> vms = new ObservableCollection<ProjectFeedViewModel>();
      this.FeedCodeItems.ItemsSource = (IEnumerable) vms;
      string projectFeedsCode = await Communicator.GetAllProjectFeedsCode();
      if (string.IsNullOrEmpty(projectFeedsCode))
      {
        vms = (ObservableCollection<ProjectFeedViewModel>) null;
      }
      else
      {
        try
        {
          List<ProjectFeedCode> projectFeedCodeList = JsonConvert.DeserializeObject<List<ProjectFeedCode>>(projectFeedsCode);
          if (projectFeedCodeList == null)
          {
            vms = (ObservableCollection<ProjectFeedViewModel>) null;
          }
          else
          {
            // ISSUE: explicit non-virtual call
            if (__nonvirtual (projectFeedCodeList.Count) <= 0)
            {
              vms = (ObservableCollection<ProjectFeedViewModel>) null;
            }
            else
            {
              projectFeedCodeList.Sort((Comparison<ProjectFeedCode>) ((a, b) => !a.createdTime.HasValue || !b.createdTime.HasValue ? 0 : a.createdTime.Value.CompareTo(b.createdTime.Value)));
              using (List<ProjectFeedCode>.Enumerator enumerator = projectFeedCodeList.GetEnumerator())
              {
                while (enumerator.MoveNext())
                  vms.Add(new ProjectFeedViewModel(enumerator.Current));
                vms = (ObservableCollection<ProjectFeedViewModel>) null;
              }
            }
          }
        }
        catch (Exception ex)
        {
          vms = (ObservableCollection<ProjectFeedViewModel>) null;
        }
      }
    }

    private void OnAddFeedCodeClick(object sender, MouseButtonEventArgs e)
    {
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.SelectProjectPopup, new ProjectExtra(), new ProjectSelectorExtra()
      {
        showAll = false,
        batchMode = false,
        canSelectGroup = false,
        onlyShowPermission = false,
        showNoteProject = false,
        showSharedProject = true,
        CanSearch = true
      });
      projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnProjectSelect);
      projectOrGroupPopup.Show();
    }

    private async void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      SubscribeCalendar subscribeCalendar = this;
      string projectId;
      if (e is ProjectGroupViewModel)
      {
        projectId = (string) null;
      }
      else
      {
        subscribeCalendar.SelectProjectPopup.IsOpen = false;
        if (!new CustomerDialog(Utils.GetString("EnableURL"), Utils.GetString("EnableUrlNotify"), Utils.GetString("GetEnable"), Utils.GetString("Close"), Window.GetWindow((DependencyObject) subscribeCalendar)).ShowDialog().GetValueOrDefault())
        {
          projectId = (string) null;
        }
        else
        {
          projectId = e.Id;
          string projectFeedsCode = await Communicator.GetProjectFeedsCode(projectId);
          if (string.IsNullOrEmpty(projectFeedsCode))
          {
            projectId = (string) null;
          }
          else
          {
            string str = projectFeedsCode.Replace("\"", "");
            if (!(subscribeCalendar.FeedCodeItems.ItemsSource is ObservableCollection<ProjectFeedViewModel> itemsSource))
            {
              projectId = (string) null;
            }
            else
            {
              ProjectFeedViewModel projectFeedViewModel = new ProjectFeedViewModel(new ProjectFeedCode()
              {
                projectId = projectId,
                code = str,
                createdTime = new DateTime?(DateTime.Now)
              });
              itemsSource.Add(projectFeedViewModel);
              projectId = (string) null;
            }
          }
        }
      }
    }

    internal async void RemoveFeedItem(ProjectFeedViewModel vm)
    {
      if (!(this.FeedCodeItems.ItemsSource is ObservableCollection<ProjectFeedViewModel> vms))
        vms = (ObservableCollection<ProjectFeedViewModel>) null;
      else if (!await Communicator.DeleteProjectFeedsCode(vm.ProjectId))
      {
        vms = (ObservableCollection<ProjectFeedViewModel>) null;
      }
      else
      {
        vms.Remove(vm);
        vms = (ObservableCollection<ProjectFeedViewModel>) null;
      }
    }

    private void CalendarNotificationMuteClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      this.MuteCheckbox.IsChecked = new bool?(!this.MuteCheckbox.IsChecked.GetValueOrDefault());
      LocalSettings.Settings.ExtraSettings.DoNotDisturbInCalendar = this.MuteCheckbox.IsChecked.Value;
      LocalSettings.Settings.Save();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/subscribecalendar.xaml", UriKind.Relative));
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
          this.SubscribeCalendarTitle = (TextBlock) target;
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddCalendarClick);
          break;
        case 3:
          this.CalendarItems = (ItemsControl) target;
          break;
        case 4:
          this.MuteCalendarGrid = (Border) target;
          break;
        case 5:
          this.MuteCheckbox = (CheckBox) target;
          this.MuteCheckbox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.CalendarNotificationMuteClick);
          break;
        case 6:
          this.MuteDesc = (TextBlock) target;
          break;
        case 7:
          this.SubscribeInOtherText = (TextBlock) target;
          break;
        case 8:
          this.AddFeedCodePanel = (StackPanel) target;
          this.AddFeedCodePanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddFeedCodeClick);
          break;
        case 9:
          this.SelectProjectPopup = (EscPopup) target;
          break;
        case 10:
          this.FeedCodeItems = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
