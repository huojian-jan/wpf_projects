// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineNavBar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineNavBar : UserControl, IComponentConnector
  {
    internal Grid Container;
    internal Border FoldGrid;
    internal Image FoldImage;
    internal Grid TitleGrid;
    internal EmojiTitleEditor Title;
    internal HoverIconButton ShareGrid;
    internal HoverIconButton SelectOrderBtn;
    internal HoverIconButton MoreOptBtn;
    internal EscPopup ChooseSortTypePopup;
    internal EscPopup MoreOptPopup;
    private bool _contentLoaded;

    public TimelineNavBar()
    {
      this.InitializeComponent();
      this.Title.SetCheckFunc(new Func<string, string>(this.CheckTitleValid));
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.Title.SetMaxWidth(this.ActualWidth - 150.0);
    }

    private string CheckTitleValid(string text)
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return (string) null;
      return dataContext.ProjectIdentity?.CheckTitleValid(text);
    }

    private void OnTitleTextChanged(object sender, string e)
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.ProjectIdentity?.SaveTitle(e);
    }

    private void SelectOrderBtnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TimelineViewModel dataContext) || !dataContext.ProjectEnable || dataContext.ProjectIdentity == null)
        return;
      List<SortTypeViewModel> timelineSortTypes = dataContext.ProjectIdentity.GetTimelineSortTypes();
      SortTypeSelector sortTypeSelector = new SortTypeSelector(dataContext.ProjectIdentity, timelineSortTypes, dataContext.TimelineSortOption.Copy(), this.ChooseSortTypePopup);
      sortTypeSelector.SortOptionSelect += new EventHandler<SortOption>(this.OnSortTypeSelect);
      sortTypeSelector.Show();
    }

    private void OnSortTypeSelect(object sender, SortOption e)
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      string data = e.orderBy;
      switch (data)
      {
        case "sortOrder":
          data = "default";
          break;
        case "dueDate":
          data = "time";
          break;
        case "project":
          data = "list";
          break;
      }
      UserActCollectUtils.AddClickEvent("timeline", "sort", data);
      dataContext.TimelineSortOption = e;
      if (e.groupBy == Constants.SortType.sortOrder.ToString() && !string.IsNullOrEmpty(dataContext.ProjectIdentity?.GetProjectId()))
        ColumnBatchHandler.MergeWithServer(dataContext.ProjectIdentity.GetProjectId());
      SyncManager.TryDelaySync(1000);
    }

    private void MoreOptBtnMouseUp(object sender, MouseButtonEventArgs e)
    {
      this.MoreOptPopup.IsOpen = true;
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      types.Add(new CustomMenuItemViewModel((object) "showComplete", Utils.GetString(LocalSettings.Settings.HideComplete ? "ShowCompleted" : "HideCompleted"), Utils.GetImageSource(LocalSettings.Settings.HideComplete ? "showCompletedDrawingImage" : "HideCompletedDrawingImage")));
      types.Add(new CustomMenuItemViewModel((object) "DisplaySetting", Utils.GetString("DisplaySetting"), Utils.GetIconData("IcDisplaySetting")));
      types.Add(new CustomMenuItemViewModel((object) "ArrangeTask", Utils.GetString("ArrangeTask"), Utils.GetIconData("IcArrangeTask")));
      if (dataContext.GroupByEnum == Constants.SortType.sortOrder && dataContext.ProjectEnable)
        types.Add(new CustomMenuItemViewModel((object) "addSection", Utils.GetString("AddSection"), Utils.GetImageSource("AddDrawingImage")));
      if (dataContext.ProjectIdentity is NormalProjectIdentity projectIdentity)
      {
        ProjectModel projectById = CacheManager.GetProjectById(projectIdentity.Id);
        if (projectById != null)
        {
          if (!projectById.IsShareList() && !projectById.Isinbox)
            types.Add(new CustomMenuItemViewModel((object) "share", Utils.GetString("Share"), Utils.GetImageSource("cooperationDrawingImage")));
          if (projectById.IsProjectPermit())
            types.Add(new CustomMenuItemViewModel((object) "listActivities", Utils.GetString("ListActivitiesPro"), Utils.GetImageSource("ProjectActivitiesDrawingImage")));
        }
      }
      List<string> switchViewModes = dataContext.ProjectIdentity?.GetSwitchViewModes();
      SwitchListViewControl topTabControl = switchViewModes != null ? new SwitchListViewControl() : (SwitchListViewControl) null;
      if (switchViewModes != null)
      {
        topTabControl.ViewSelected += new EventHandler<string>(this.OnSwitchView);
        topTabControl.SetButtonStatus(new bool?(false), switchViewModes.Contains("kanban") ? new bool?(false) : new bool?(), new bool?(true));
      }
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.MoreOptPopup, (ITabControl) topTabControl);
      customMenuList.Operated += new EventHandler<object>(this.OnMoreItemSelected);
      customMenuList.Show();
    }

    private void OnMoreItemSelected(object sender, object e)
    {
      if (!(e is string str))
        return;
      switch (str)
      {
        case "share":
          this.ShowShareDiaplog();
          break;
        case "showComplete":
          this.ShowOrHideCompleteClick();
          break;
        case "DisplaySetting":
          this.OnSettingsClick();
          break;
        case "ArrangeTask":
          this.OnArrangeClick();
          break;
        case "addSection":
          this.AddSectionClick();
          break;
        case "listActivities":
          this.OnListActivityClick();
          break;
      }
    }

    private void OnArrangeClick()
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.IsArranging = !dataContext.IsArranging;
      if (!dataContext.IsArranging)
        return;
      UserActCollectUtils.AddClickEvent("timeline", "action_bar", "arrangement");
    }

    private void OnSettingsClick()
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.IsSetting = true;
      UserActCollectUtils.AddClickEvent("timeline", "action_bar", "view_options");
      TimelineSettingsWindow timelineSettingsWindow = new TimelineSettingsWindow(dataContext);
      timelineSettingsWindow.Owner = Window.GetWindow((DependencyObject) this);
      timelineSettingsWindow.ShowDialog();
    }

    private void MenuFoldGridMouseUp(object sender, MouseButtonEventArgs e)
    {
      Utils.FindParent<ListViewContainer>((DependencyObject) this)?.ShowProjectMenu();
    }

    private void ShowOrHideCompleteClick()
    {
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      UserActCollectUtils.AddClickEvent("timeline", "om", "show_hide_completed");
      dataContext.HideCompleted = !dataContext.HideCompleted;
    }

    private void AddSectionClick()
    {
      UserActCollectUtils.AddClickEvent("timeline", "om", "add_section");
      if (!(this.DataContext is TimelineViewModel dataContext) || dataContext.GroupByEnum != Constants.SortType.sortOrder)
        return;
      dataContext.AddNewColumn();
    }

    private void ShareGridClick(object sender, MouseButtonEventArgs e) => this.ShowShareDiaplog();

    private async Task ShowShareDiaplog()
    {
      TimelineNavBar timelineNavBar = this;
      if (!(timelineNavBar.DataContext is TimelineViewModel dataContext))
        return;
      ProjectModel projectById = await ProjectDao.GetProjectById(dataContext.ProjectIdentity?.GetProjectId());
      if (projectById == null)
        return;
      ShareProjectDialog.TryShowShareDialog(projectById.id, Window.GetWindow((DependencyObject) timelineNavBar));
    }

    private async void OnListActivityClick()
    {
      TimelineNavBar timelineNavBar = this;
      if (!ProChecker.CheckPro(ProType.ListActivities) || !(timelineNavBar.DataContext is TimelineViewModel dataContext) || string.IsNullOrEmpty(dataContext.ProjectIdentity?.GetProjectId()))
        return;
      ProjectActivityWindow projectActivityWindow = new ProjectActivityWindow(dataContext.ProjectIdentity.GetProjectId());
      projectActivityWindow.Owner = Window.GetWindow((DependencyObject) timelineNavBar);
      ProjectActivityWindow window = projectActivityWindow;
      window.Closed += (EventHandler) ((o, args) => window.Owner?.Activate());
      window.Show();
    }

    private void OnSwitchView(object sender, string e)
    {
      this.MoreOptPopup.IsOpen = false;
      if (!(e == "list") && !(e == "kanban"))
        return;
      UserActCollectUtils.AddClickEvent("list_mode", "switch", e);
      if (!(this.DataContext is TimelineViewModel dataContext))
        return;
      dataContext.ProjectIdentity?.SwitchViewMode(e);
    }

    public void SetTitle(string title) => this.Title.SetText(title);

    public void SetTitleEnable(bool enable) => this.Title.SetEnable(enable);

    private void OnFoldGridMouseEnter(object sender, MouseEventArgs e)
    {
      Utils.FindParent<ListViewContainer>((DependencyObject) this)?.TryShowMenuOnHover((UIElement) this.FoldGrid);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/timeline/timelinenavbar.xaml", UriKind.Relative));
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
          this.Container = (Grid) target;
          break;
        case 2:
          this.FoldGrid = (Border) target;
          this.FoldGrid.MouseEnter += new MouseEventHandler(this.OnFoldGridMouseEnter);
          this.FoldGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.MenuFoldGridMouseUp);
          break;
        case 3:
          this.FoldImage = (Image) target;
          break;
        case 4:
          this.TitleGrid = (Grid) target;
          break;
        case 5:
          this.Title = (EmojiTitleEditor) target;
          break;
        case 6:
          this.ShareGrid = (HoverIconButton) target;
          break;
        case 7:
          this.SelectOrderBtn = (HoverIconButton) target;
          break;
        case 8:
          this.MoreOptBtn = (HoverIconButton) target;
          break;
        case 9:
          this.ChooseSortTypePopup = (EscPopup) target;
          break;
        case 10:
          this.MoreOptPopup = (EscPopup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
