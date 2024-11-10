// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.ProjectActivityWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Activity;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public class ProjectActivityWindow : Window, IComponentConnector
  {
    private readonly string _projectId;
    internal ProjectActivityControl ProjectActivityControl;
    private bool _contentLoaded;

    public ProjectActivityWindow(string projectId)
    {
      this.InitializeComponent();
      this._projectId = projectId;
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnActivityWindowLoaded(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this._projectId))
        return;
      UserInfoModel userInfo = await UserManager.GetUserInfo();
      string userName = LocalSettings.Settings.LoginUserName;
      if (userInfo != null)
        userName = string.IsNullOrEmpty(userInfo.name) ? userInfo.username : userInfo.name;
      string projectActivities = await Communicator.GetProjectActivities(this._projectId);
      try
      {
        List<ProjectModifyModel> items = JsonConvert.DeserializeObject<List<ProjectModifyModel>>(projectActivities).Where<ProjectModifyModel>((Func<ProjectModifyModel, bool>) (item => !item.action.ToUpper().Contains("P_VIEW_MODE"))).ToList<ProjectModifyModel>();
        Dictionary<string, TaskDao.TitleModel> dictionary = (await TaskDao.GetTaskTitleInProject()).ToDictionary<TaskDao.TitleModel, string, TaskDao.TitleModel>((Func<TaskDao.TitleModel, string>) (title => title.TaskId), (Func<TaskDao.TitleModel, TaskDao.TitleModel>) (title => title));
        if (items.Any<ProjectModifyModel>())
        {
          List<ProjectActivityViewModel> source = new List<ProjectActivityViewModel>();
          foreach (ProjectModifyModel model in items)
            source.Add(new ProjectActivityViewModel(model, userName, (IReadOnlyDictionary<string, TaskDao.TitleModel>) dictionary));
          IEnumerable<IGrouping<DateTime, ProjectActivityViewModel>> groupings = source.OrderByDescending<ProjectActivityViewModel, DateTime>((Func<ProjectActivityViewModel, DateTime>) (item => item.Date)).GroupBy<ProjectActivityViewModel, DateTime>((Func<ProjectActivityViewModel, DateTime>) (item => item.Date.Date));
          List<ProjectActivityViewModel> activityViewModelList = new List<ProjectActivityViewModel>();
          foreach (IGrouping<DateTime, ProjectActivityViewModel> collection in groupings)
          {
            activityViewModelList.Add(new ProjectActivityViewModel(DateUtils.FormatTimeDesc(collection.Key, true) + " " + DateUtils.FormatWeekDayName(collection.Key)));
            activityViewModelList.AddRange((IEnumerable<ProjectActivityViewModel>) collection);
          }
          this.ProjectActivityControl.Items.ItemsSource = (IEnumerable) activityViewModelList;
        }
        items = (List<ProjectModifyModel>) null;
      }
      catch (Exception ex)
      {
      }
      userName = (string) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/kanban/projectactivitywindow.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.ProjectActivityControl = (ProjectActivityControl) target;
        else
          this._contentLoaded = true;
      }
      else
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnActivityWindowLoaded);
    }
  }
}
