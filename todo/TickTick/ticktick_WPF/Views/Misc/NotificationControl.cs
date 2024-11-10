// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.NotificationControl
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
using System.Windows.Markup;
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class NotificationControl : UserControl, IComponentConnector
  {
    private readonly ObservableCollection<NotificationViewModel> _notificationShowList = new ObservableCollection<NotificationViewModel>();
    private readonly ObservableCollection<NotificationViewModel> _activitiesShowList = new ObservableCollection<NotificationViewModel>();
    private TempDataModel _notificationData;
    private bool _windowStart = true;
    private MarkType _markType;
    public NotificationUnreadCount CountModel;
    private static readonly List<string> Types = new List<string>()
    {
      "team",
      "task",
      "attend",
      "projectPermission",
      "share",
      "PayReminder",
      "support",
      "TeamPayReminder",
      "comment",
      "upgrade",
      "forumTopic",
      "score",
      "permission",
      "assign"
    };
    internal Grid SwitchGrid;
    internal GroupTitle SwitchTitle;
    internal Border UnreadNumber;
    internal TextBlock NotificationSingle;
    internal Grid NotificationGrid;
    internal Grid NoNotificationsGrid;
    internal ListView NotificationItemsControl;
    internal Grid ActivitiesGrid;
    internal ListView ActivitiesItemsControl;
    internal Grid NoActivitiesGrid;
    internal LoadingIndicator Loading;
    private bool _contentLoaded;

    public NotificationControl()
    {
      this.InitializeComponent();
      this.NotificationItemsControl.ItemsSource = (IEnumerable) this._notificationShowList;
      this.ActivitiesItemsControl.ItemsSource = (IEnumerable) this._activitiesShowList;
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      this._notificationShowList.Clear();
      this._activitiesShowList.Clear();
    }

    private async void InitViews(object sender, RoutedEventArgs routedEventArgs)
    {
      NotificationControl notificationControl = this;
      if (notificationControl._windowStart)
      {
        notificationControl._windowStart = false;
      }
      else
      {
        notificationControl.SwitchTitle.SelectedTitleChanged -= new EventHandler<GroupTitleViewModel>(notificationControl.OnSelectedChanged);
        if (CacheManager.IsShareListUser())
        {
          notificationControl.NotificationSingle.Visibility = Visibility.Collapsed;
          notificationControl.SwitchGrid.Visibility = Visibility.Visible;
          NotificationUnreadCount notificationCount = await Communicator.GetNotificationCount();
          notificationControl.CountModel = notificationCount;
          if (notificationControl.CountModel.Notification > 0)
          {
            notificationControl.UnreadNumber.Visibility = notificationControl.CountModel.Activity > 0 ? Visibility.Visible : Visibility.Hidden;
            notificationControl._markType = MarkType.notifications;
            notificationControl.CountModel.Notification = 0;
            Communicator.MarkReadNotification(notificationControl._markType.ToString());
            if (notificationControl.CountModel.Activity > 0)
            {
              App.Window.LeftTabBar.ShowNotificationIndicator(notificationControl.CountModel);
              notificationControl.SwitchTitle.SetSelectedIndex(0);
              notificationControl.SwitchTitle.SelectedTitleChanged += new EventHandler<GroupTitleViewModel>(notificationControl.OnSelectedChanged);
              return;
            }
          }
          else if (notificationControl.CountModel.Activity > 0)
          {
            notificationControl.ShowAvtivities();
            notificationControl.UnreadNumber.Visibility = Visibility.Hidden;
            notificationControl._markType = MarkType.activities;
            Communicator.MarkReadNotification(notificationControl._markType.ToString());
            notificationControl.CountModel.Activity = 0;
          }
          else
          {
            notificationControl.UnreadNumber.Visibility = Visibility.Hidden;
            notificationControl._markType = MarkType.none;
          }
        }
        else
        {
          notificationControl.CountModel = (NotificationUnreadCount) null;
          notificationControl._markType = MarkType.none;
          Communicator.MarkReadNotification("all");
          App.Window.LeftTabBar.HideNotificationIndicator();
        }
        notificationControl.SwitchTitle.SetSelectedIndex(notificationControl.ActivitiesGrid.Visibility == Visibility.Visible ? 1 : 0);
        notificationControl.SwitchTitle.SelectedTitleChanged += new EventHandler<GroupTitleViewModel>(notificationControl.OnSelectedChanged);
        App.Window.LeftTabBar.HideNotificationIndicator();
      }
    }

    private void ResetView(object sender, RoutedEventArgs e) => this.OnSwitchNotification(true);

    private void SwitchNotification() => this.OnSwitchNotification();

    private void OnSwitchNotification(bool reset = false)
    {
      if (!(this.ActivitiesGrid.IsVisible | reset))
        return;
      DoubleAnimation resource = (DoubleAnimation) this.FindResource((object) "HideAnimation");
      resource.Completed -= new EventHandler(this.ShowActivities);
      resource.Completed -= new EventHandler(this.ShowNotification);
      resource.Completed += new EventHandler(this.ShowNotification);
      this.ActivitiesGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) resource);
    }

    private void SwitchActivities()
    {
      if (!this.NotificationGrid.IsVisible)
        return;
      DoubleAnimation resource = (DoubleAnimation) this.FindResource((object) "HideAnimation");
      resource.Completed -= new EventHandler(this.ShowNotification);
      resource.Completed -= new EventHandler(this.ShowActivities);
      resource.Completed += new EventHandler(this.ShowActivities);
      this.NotificationGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) resource);
      if (this.CountModel != null && this.CountModel.Activity > 0)
      {
        this._markType = this._markType == MarkType.none ? MarkType.all : MarkType.activities;
        Communicator.MarkReadNotification("activities");
        this.CountModel.Activity = 0;
        Communicator.MarkReadNotification(this._markType.ToString());
        App.Window.LeftTabBar.HideNotificationIndicator();
      }
      this.UnreadNumber.Visibility = Visibility.Hidden;
    }

    private void ShowNotification(object sender, EventArgs e)
    {
      DoubleAnimation resource = (DoubleAnimation) this.FindResource((object) "ShowAnimation");
      this.ActivitiesGrid.Visibility = Visibility.Collapsed;
      this.NotificationGrid.Visibility = Visibility.Visible;
      this.NotificationGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) resource);
    }

    private void ShowActivities(object sender, EventArgs e)
    {
      DoubleAnimation resource = (DoubleAnimation) this.FindResource((object) "ShowAnimation");
      this.NotificationGrid.Visibility = Visibility.Collapsed;
      this.ActivitiesGrid.Visibility = Visibility.Visible;
      this.ActivitiesGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) resource);
    }

    private void ShowAvtivities()
    {
      this.NotificationGrid.Visibility = Visibility.Collapsed;
      this.ActivitiesGrid.Visibility = Visibility.Visible;
      this.ActivitiesGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      this.ActivitiesGrid.Opacity = 1.0;
    }

    public async Task GetNotification()
    {
      NotificationControl notificationControl = this;
      ObservableCollection<NotificationModel> notificationModelList = (ObservableCollection<NotificationModel>) null;
      notificationControl.NoNotificationsGrid.Visibility = Visibility.Collapsed;
      NotificationViewModel notificationViewModel = notificationControl._notificationShowList.FirstOrDefault<NotificationViewModel>();
      if (notificationViewModel != null)
        notificationControl.NotificationItemsControl.ScrollIntoView((object) notificationViewModel);
      notificationControl.Loading.Visibility = Visibility.Visible;
      ((Storyboard) notificationControl.FindResource((object) "ShowNotificationLoadingStoryboard")).Begin();
      if (notificationControl._notificationShowList.Count == 0 && notificationControl._activitiesShowList.Count == 0)
      {
        TempDataModel tempDataModel = await TempDataDao.QueryTempDataModelListDbByTypeAndUserIdAsync("NOTIFICIATION", LocalSettings.Settings["LoginUserId"].ToString());
        notificationControl._notificationData = tempDataModel;
        if (notificationControl._notificationData != null && !string.IsNullOrEmpty(notificationControl._notificationData.Data))
          notificationControl._notificationData.ModifyTime = Utils.GetNowTimeStamp();
        else if (notificationControl._notificationData == null)
          notificationControl._notificationData = new TempDataModel()
          {
            User_Id = LocalSettings.Settings["LoginUserId"].ToString(),
            DataType = "NOTIFICIATION",
            ModifyTime = Utils.GetNowTimeStamp()
          };
      }
      else if (notificationControl._notificationData == null)
        notificationControl._notificationData = new TempDataModel()
        {
          User_Id = LocalSettings.Settings["LoginUserId"].ToString(),
          DataType = "NOTIFICIATION",
          ModifyTime = Utils.GetNowTimeStamp()
        };
      string notificationList = await Communicator.GetNotificationList();
      try
      {
        notificationModelList = JsonConvert.DeserializeObject<ObservableCollection<NotificationModel>>(notificationList);
      }
      catch (Exception ex1)
      {
        if (notificationControl._notificationShowList.Count == 0)
        {
          if (notificationControl._activitiesShowList.Count == 0)
          {
            try
            {
              notificationModelList = JsonConvert.DeserializeObject<ObservableCollection<NotificationModel>>(notificationControl._notificationData.Data);
            }
            catch (Exception ex2)
            {
              notificationModelList = (ObservableCollection<NotificationModel>) null;
            }
          }
        }
      }
      if (notificationModelList != null)
      {
        notificationControl._notificationShowList.Clear();
        notificationControl._activitiesShowList.Clear();
        if (notificationModelList.Count > 0)
        {
          notificationControl._notificationData.Data = notificationList;
          int num = await TempDataDao.UpdateOrInsertTempDataModelListDbByTypeAndUserIdAsync(notificationControl._notificationData);
        }
        try
        {
          await notificationControl.GetViewModels(notificationModelList);
        }
        catch (Exception ex)
        {
          UtilLog.Error(ex);
        }
      }
      notificationControl.ShowOrHideView();
      Storyboard resource = (Storyboard) notificationControl.FindResource((object) "HideNotificationLoadingStoryboard");
      resource.Begin();
      resource.Completed -= new EventHandler(notificationControl.HideLoadingImage);
      resource.Completed += new EventHandler(notificationControl.HideLoadingImage);
      notificationModelList = (ObservableCollection<NotificationModel>) null;
    }

    private void HideLoadingImage(object sender, EventArgs e)
    {
      this.Loading.Visibility = Visibility.Collapsed;
    }

    private void ShowOrHideView()
    {
      ListView notificationItemsControl = this.NotificationItemsControl;
      ObservableCollection<NotificationViewModel> notificationShowList1 = this._notificationShowList;
      // ISSUE: explicit non-virtual call
      int num1 = (notificationShowList1 != null ? (__nonvirtual (notificationShowList1.Count) > 0 ? 1 : 0) : 0) != 0 ? 0 : 2;
      notificationItemsControl.Visibility = (Visibility) num1;
      Grid notificationsGrid = this.NoNotificationsGrid;
      ObservableCollection<NotificationViewModel> notificationShowList2 = this._notificationShowList;
      // ISSUE: explicit non-virtual call
      int num2 = (notificationShowList2 != null ? (__nonvirtual (notificationShowList2.Count) > 0 ? 1 : 0) : 0) != 0 ? 2 : 0;
      notificationsGrid.Visibility = (Visibility) num2;
      ListView activitiesItemsControl = this.ActivitiesItemsControl;
      ObservableCollection<NotificationViewModel> activitiesShowList1 = this._activitiesShowList;
      // ISSUE: explicit non-virtual call
      int num3 = (activitiesShowList1 != null ? (__nonvirtual (activitiesShowList1.Count) > 0 ? 1 : 0) : 0) != 0 ? 0 : 2;
      activitiesItemsControl.Visibility = (Visibility) num3;
      Grid noActivitiesGrid = this.NoActivitiesGrid;
      ObservableCollection<NotificationViewModel> activitiesShowList2 = this._activitiesShowList;
      // ISSUE: explicit non-virtual call
      int num4 = (activitiesShowList2 != null ? (__nonvirtual (activitiesShowList2.Count) > 0 ? 1 : 0) : 0) != 0 ? 2 : 0;
      noActivitiesGrid.Visibility = (Visibility) num4;
    }

    private async Task GetViewModels(
      ObservableCollection<NotificationModel> notificationModelList)
    {
      foreach (NotificationModel notificationModel in (Collection<NotificationModel>) notificationModelList)
      {
        ProjectModel projectById = CacheManager.GetProjectById(notificationModel.notificationUserData.projectId);
        if (!NotificationControl.Types.Contains(notificationModel.type))
        {
          this._notificationShowList.Add(new NotificationViewModel(notificationModel)
          {
            Title = notificationModel.title
          });
        }
        else
        {
          switch (notificationModel.type)
          {
            case "team":
              if (notificationModel.actionStatus == 18 || notificationModel.actionStatus == 19)
                goto case "task";
              else
                break;
            case "task":
              if (string.IsNullOrEmpty(projectById?.teamId) || !TeamDao.IsTeamExpired(projectById.teamId))
              {
                this._activitiesShowList.Add(new NotificationViewModel(notificationModel));
                continue;
              }
              this._notificationShowList.Add(new NotificationViewModel(notificationModel));
              continue;
          }
          this._notificationShowList.Add(new NotificationViewModel(notificationModel));
        }
      }
      NotificationViewModel item;
      foreach (NotificationViewModel notificationViewModel1 in this._notificationShowList.ToList<NotificationViewModel>())
      {
        item = notificationViewModel1;
        NotificationViewModel notificationViewModel;
        switch (item.type)
        {
          case "permission":
            if (item.actionStatus == 2 || item.actionStatus == 3)
            {
              notificationViewModel = item;
              notificationViewModel.AvatarUrl = await Utils.GetAvatarUrl(item.notification.notificationUserData.ownerUserCode);
              notificationViewModel = (NotificationViewModel) null;
              break;
            }
            notificationViewModel = item;
            notificationViewModel.AvatarUrl = await Utils.GetAvatarUrl(item.notification.notificationUserData.applyUserCode);
            notificationViewModel = (NotificationViewModel) null;
            break;
          case "projectPermission":
            if (item.actionStatus == 1 || item.actionStatus == 4 || item.actionStatus == 5)
            {
              notificationViewModel = item;
              notificationViewModel.AvatarUrl = await Utils.GetAvatarUrl(item.notification.notificationUserData.applyUserCode);
              notificationViewModel = (NotificationViewModel) null;
              item.UserDisplayName = item.notification.notificationUserData.applyUserDisplayName;
              break;
            }
            notificationViewModel = item;
            notificationViewModel.AvatarUrl = await Utils.GetAvatarUrl(item.notification.notificationUserData.ownerUserCode);
            notificationViewModel = (NotificationViewModel) null;
            item.UserDisplayName = item.notification.notificationUserData.ownerUserDisplayName;
            break;
          default:
            notificationViewModel = item;
            notificationViewModel.AvatarUrl = await Utils.GetAvatarUrl(item.userCode);
            notificationViewModel = (NotificationViewModel) null;
            break;
        }
        item = (NotificationViewModel) null;
      }
      foreach (NotificationViewModel notificationViewModel in this._activitiesShowList.ToList<NotificationViewModel>())
      {
        item = notificationViewModel;
        item.AvatarUrl = await Utils.GetAvatarUrl(notificationViewModel.userCode);
        item = (NotificationViewModel) null;
      }
    }

    private void OnSelectedChanged(object sender, GroupTitleViewModel e)
    {
      if ("Notification".Equals(e.Title))
        this.SwitchNotification();
      else
        this.SwitchActivities();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/notificationcontrol.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.InitViews);
          ((FrameworkElement) target).Unloaded += new RoutedEventHandler(this.ResetView);
          break;
        case 2:
          this.SwitchGrid = (Grid) target;
          break;
        case 3:
          this.SwitchTitle = (GroupTitle) target;
          break;
        case 4:
          this.UnreadNumber = (Border) target;
          break;
        case 5:
          this.NotificationSingle = (TextBlock) target;
          break;
        case 6:
          this.NotificationGrid = (Grid) target;
          break;
        case 7:
          this.NoNotificationsGrid = (Grid) target;
          break;
        case 8:
          this.NotificationItemsControl = (ListView) target;
          break;
        case 9:
          this.ActivitiesGrid = (Grid) target;
          break;
        case 10:
          this.ActivitiesItemsControl = (ListView) target;
          break;
        case 11:
          this.NoActivitiesGrid = (Grid) target;
          break;
        case 12:
          this.Loading = (LoadingIndicator) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
