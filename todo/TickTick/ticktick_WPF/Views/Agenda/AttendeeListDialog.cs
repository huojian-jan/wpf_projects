// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Agenda.AttendeeListDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Agenda
{
  public class AttendeeListDialog : Window, IComponentConnector, IStyleConnector
  {
    private readonly string _projectId;
    private readonly string _attendId;
    private readonly string _taskId;
    private readonly TaskAttendModel _model;
    internal ItemsControl AttendeeItems;
    private bool _contentLoaded;

    public AttendeeListDialog(
      string projectId,
      string attendId,
      string taskId,
      TaskAttendModel model)
    {
      this.InitializeComponent();
      this._attendId = attendId;
      this._projectId = projectId;
      this._taskId = taskId;
      this._model = model;
      this.LoadData();
    }

    private async Task LoadData()
    {
      if (this._model == null)
        return;
      bool canDelete = this._model.organizer.isMyself;
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(this._taskId);
      if (thinTaskById != null)
      {
        ProjectModel projectById = CacheManager.GetProjectById(thinTaskById.projectId);
        if (projectById != null && projectById.IsShareList())
          canDelete = true;
      }
      UserInfoModel userByName = await UserDao.GetUserByName(LocalSettings.Settings.LoginUserName);
      ObservableCollection<UserProfileViewModel> source = new ObservableCollection<UserProfileViewModel>();
      if (this._model.organizer != null)
      {
        ObservableCollection<UserProfileViewModel> observableCollection = source;
        UserProfileViewModel profileViewModel;
        if (!this._model.organizer.isMyself)
        {
          profileViewModel = new UserProfileViewModel(this._model.organizer)
          {
            IsOwner = true
          };
        }
        else
        {
          profileViewModel = new UserProfileViewModel(userByName, true);
          profileViewModel.IsOwner = true;
        }
        observableCollection.Add(profileViewModel);
      }
      if (this._model.attendees != null && this._model.attendees.Any<AttendeeModel>())
      {
        foreach (AttendeeModel attendee in this._model.attendees)
          source.Add(new UserProfileViewModel(attendee)
          {
            ShowDelete = canDelete
          });
        source = new ObservableCollection<UserProfileViewModel>(source.ToList<UserProfileViewModel>().OrderByDescending<UserProfileViewModel, bool>((Func<UserProfileViewModel, bool>) (m => m.IsOwner)).ThenByDescending<UserProfileViewModel, bool>((Func<UserProfileViewModel, bool>) (m => m.IsMySelf)).ToList<UserProfileViewModel>());
      }
      this.AttendeeItems.ItemsSource = (IEnumerable) source;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is TextBlock textBlock))
        ;
      else
      {
        string userCode = textBlock.Tag as string;
        if (userCode == null)
          ;
        else
        {
          await Communicator.RemoveAttendee(this._projectId, this._attendId, userCode);
          ObservableCollection<UserProfileViewModel> itemsSource = (ObservableCollection<UserProfileViewModel>) this.AttendeeItems.ItemsSource;
          UserProfileViewModel profileViewModel = itemsSource.FirstOrDefault<UserProfileViewModel>((Func<UserProfileViewModel, bool>) (m => m.UserCode == userCode));
          if (profileViewModel == null)
            ;
          else
            itemsSource.Remove(profileViewModel);
        }
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/agenda/attendeelistdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.AttendeeItems = (ItemsControl) target;
      else
        this._contentLoaded = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
    }
  }
}
