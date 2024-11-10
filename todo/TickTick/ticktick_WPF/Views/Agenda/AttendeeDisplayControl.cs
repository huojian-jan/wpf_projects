// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Agenda.AttendeeDisplayControl
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
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Agenda
{
  public class AttendeeDisplayControl : UserControl, IComponentConnector
  {
    internal ItemsControl AttendeePanel;
    internal TextBlock ErrorHint;
    private bool _contentLoaded;

    private List<ProfileModel> AttendModels { get; set; } = new List<ProfileModel>();

    public AttendeeDisplayControl() => this.InitializeComponent();

    public async Task<bool> WaitingForAttend(AgendaHelper.IAgenda agenda)
    {
      return AgendaHelper.CanAccessAgenda(agenda) && this.AttendModels.Count == 1;
    }

    public async Task LoadData(double panelWidth, string attendId, string taskId)
    {
      UserInfoModel myself = await UserDao.GetUserByName(LocalSettings.Settings.LoginUserName);
      TaskAttendModel model = await AgendaHelper.GetAgendaAttendModel(attendId, taskId);
      this.LoadData(panelWidth, myself, model);
      TaskAttendModel remoteModel = await AgendaHelper.GetRemoteModel(attendId, taskId);
      if (!(JsonConvert.SerializeObject((object) model) != JsonConvert.SerializeObject((object) remoteModel)))
      {
        myself = (UserInfoModel) null;
        model = (TaskAttendModel) null;
      }
      else
      {
        this.LoadData(panelWidth, myself, remoteModel);
        myself = (UserInfoModel) null;
        model = (TaskAttendModel) null;
      }
    }

    private void LoadData(double panelWidth, UserInfoModel myself, TaskAttendModel model)
    {
      this.AttendeePanel.ItemsSource = (IEnumerable) null;
      this.AttendModels.Clear();
      if (model == null)
        return;
      if (model.organizer != null)
      {
        this.AssembleDisplayModels(model, myself);
        this.NotifyWidthChanged(panelWidth);
      }
      else
        this.DisplayError();
    }

    private void DisplayError()
    {
      this.AttendeePanel.Visibility = Visibility.Collapsed;
      this.ErrorHint.Visibility = Visibility.Visible;
    }

    private void AssembleDisplayModels(TaskAttendModel model, UserInfoModel myself)
    {
      this.AttendeePanel.Visibility = Visibility.Visible;
      this.ErrorHint.Visibility = Visibility.Collapsed;
      if (model == null)
        return;
      if (model.organizer != null)
      {
        if (model.organizer.isMyself)
        {
          this.AttendModels.Add(new ProfileModel()
          {
            name = myself.name,
            email = myself.email,
            avatarUrl = myself.picture,
            isMyself = true,
            isOwner = true
          });
        }
        else
        {
          model.organizer.isOwner = true;
          this.AttendModels.Add(model.organizer);
        }
      }
      if (model.attendees != null && model.attendees.Any<AttendeeModel>())
      {
        foreach (AttendeeModel attendee in model.attendees)
        {
          if (attendee.user.isMyself)
            this.AttendModels.Add(new ProfileModel()
            {
              name = myself.name,
              email = myself.email,
              avatarUrl = myself.picture,
              isOwner = false,
              isMyself = true
            });
          else
            this.AttendModels.Add(attendee.user);
        }
      }
      if (this.AttendModels == null || !this.AttendModels.Any<ProfileModel>())
        return;
      this.AttendModels = this.AttendModels.OrderByDescending<ProfileModel, bool>((Func<ProfileModel, bool>) (m => m.isOwner)).ThenByDescending<ProfileModel, bool>((Func<ProfileModel, bool>) (m => m.isMyself)).ToList<ProfileModel>();
    }

    public void NotifyWidthChanged(double width)
    {
      ObservableCollection<BaseViewModel> observableCollection = new ObservableCollection<BaseViewModel>();
      int displayCount = AttendeeDisplayControl.GetDisplayCount(width);
      if (this.AttendModels == null || !this.AttendModels.Any<ProfileModel>())
        return;
      List<ProfileModel> list = this.AttendModels.Take<ProfileModel>(displayCount - 1).ToList<ProfileModel>();
      foreach (ProfileModel model in list)
        observableCollection.Add((BaseViewModel) new UserProfileViewModel(model));
      if (this.AttendModels.Count > displayCount)
      {
        int num = this.AttendModels.Count - list.Count;
        observableCollection.Add((BaseViewModel) new AttendeeExtraViewModel()
        {
          Title = ("+" + num.ToString())
        });
      }
      else
        observableCollection.Add((BaseViewModel) new AttendeeExtraViewModel()
        {
          Title = string.Format(Utils.GetString("PersonCount"), (object) this.AttendModels.Count)
        });
      this.AttendeePanel.ItemsSource = (IEnumerable) observableCollection;
    }

    private static int GetDisplayCount(double panelWidth) => (int) (panelWidth / 28.0) - 1;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/agenda/attendeedisplaycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.ErrorHint = (TextBlock) target;
        else
          this._contentLoaded = true;
      }
      else
        this.AttendeePanel = (ItemsControl) target;
    }
  }
}
