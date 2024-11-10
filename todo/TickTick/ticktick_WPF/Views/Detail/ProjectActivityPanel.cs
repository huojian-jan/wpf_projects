// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.ProjectActivityPanel
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Activity;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class ProjectActivityPanel : Grid, IComponentConnector
  {
    internal Grid ProjectBackPathGrid;
    internal ProjectActivityControl ProjectActivityControl;
    private bool _contentLoaded;

    public event EventHandler HideDetail;

    public ProjectActivityPanel(bool showBack)
    {
      this.InitializeComponent();
      this.SetBachPathVisible(showBack);
    }

    private void OnBackClick(object sender, MouseButtonEventArgs e)
    {
      this.ProjectActivityControl.Items.ItemsSource = (IEnumerable) null;
      EventHandler hideDetail = this.HideDetail;
      if (hideDetail == null)
        return;
      hideDetail((object) this, (EventArgs) null);
    }

    public async Task Init(string projectId)
    {
      this.ProjectActivityControl.Items.ItemsSource = (IEnumerable) null;
      UserInfoModel userInfo = await UserManager.GetUserInfo();
      string userName = LocalSettings.Settings.LoginUserName;
      if (userInfo != null)
        userName = string.IsNullOrEmpty(userInfo.name) ? userInfo.username : userInfo.name;
      string projectActivities = await Communicator.GetProjectActivities(projectId);
      try
      {
        List<ProjectModifyModel> source1 = JsonConvert.DeserializeObject<List<ProjectModifyModel>>(projectActivities);
        List<ProjectModifyModel> items = source1 != null ? source1.Where<ProjectModifyModel>((Func<ProjectModifyModel, bool>) (item => !item.action.ToUpper().Contains("P_VIEW_MODE"))).ToList<ProjectModifyModel>() : (List<ProjectModifyModel>) null;
        Dictionary<string, TaskDao.TitleModel> dictionary = (await TaskDao.GetTaskTitleInProject()).ToDictionary<TaskDao.TitleModel, string, TaskDao.TitleModel>((Func<TaskDao.TitleModel, string>) (title => title.TaskId), (Func<TaskDao.TitleModel, TaskDao.TitleModel>) (title => title));
        List<ProjectModifyModel> projectModifyModelList = items;
        // ISSUE: explicit non-virtual call
        if ((projectModifyModelList != null ? (__nonvirtual (projectModifyModelList.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          List<ProjectActivityViewModel> source2 = new List<ProjectActivityViewModel>();
          foreach (ProjectModifyModel model in items)
            source2.Add(new ProjectActivityViewModel(model, userName, (IReadOnlyDictionary<string, TaskDao.TitleModel>) dictionary));
          IEnumerable<IGrouping<DateTime, ProjectActivityViewModel>> groupings = source2.OrderByDescending<ProjectActivityViewModel, DateTime>((Func<ProjectActivityViewModel, DateTime>) (item => item.Date)).GroupBy<ProjectActivityViewModel, DateTime>((Func<ProjectActivityViewModel, DateTime>) (item => item.Date.Date));
          List<ProjectActivityViewModel> activityViewModelList = new List<ProjectActivityViewModel>();
          foreach (IGrouping<DateTime, ProjectActivityViewModel> collection in groupings)
          {
            activityViewModelList.Add(new ProjectActivityViewModel(DateUtils.FormatTimeDesc(collection.Key, true) + " " + DateUtils.FormatWeekDayName(collection.Key)));
            activityViewModelList.AddRange((IEnumerable<ProjectActivityViewModel>) collection);
          }
          this.ProjectActivityControl.Items.ItemsSource = (IEnumerable) activityViewModelList;
        }
        items = (List<ProjectModifyModel>) null;
        userName = (string) null;
      }
      catch (Exception ex)
      {
        userName = (string) null;
      }
    }

    public void SetBachPathVisible(bool show)
    {
      this.ProjectBackPathGrid.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    public void ClearEvent() => this.HideDetail = (EventHandler) null;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/projectactivitypanel.xaml", UriKind.Relative));
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
      {
        this.ProjectBackPathGrid = (Grid) target;
        this.ProjectBackPathGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBackClick);
      }
    }
  }
}
