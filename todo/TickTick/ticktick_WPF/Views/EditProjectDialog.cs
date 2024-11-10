// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.EditProjectDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Drag;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views
{
  public class EditProjectDialog : System.Windows.Window, IOkCancelWindow, IComponentConnector, IStyleConnector
  {
    private readonly bool _isAddProject;
    private readonly ticktick_WPF.ViewModels.ProjectViewModel _model = new ticktick_WPF.ViewModels.ProjectViewModel();
    private readonly string _originalName;
    private readonly string _originColor = "null";
    internal EditProjectDialog Window;
    internal TextBlock WTitle;
    internal Grid InputGrid;
    internal EmojiEditor AddListNameTextBox;
    internal Grid EmojiSelectGrid;
    internal Grid ProjectPathGrid;
    internal Path ProjectPath;
    internal Path SetEmojiIcon;
    internal EmjTextBlock EmojiText;
    internal EscPopup EmojiSelectPopup;
    internal EmojiSelectControl EmojiSelector;
    internal TextBox HideTextBox;
    internal TextBlock AddListNameRepeatTextBlock;
    internal ticktick_WPF.Views.Misc.ColorSelector.ColorSelector ColorItems;
    internal TextBlock TypeText;
    internal CustomComboBox ProjectTypeComboBox;
    internal CustomComboBox ProjectGroupComboBox;
    internal Grid ProjectBelongTo;
    internal StackPanel TeamNamePanel;
    internal TextBlock SwitchListTypeText;
    internal CustomComboBox SelectTeamComBox;
    internal EscPopup SelectAddProjectPopup;
    internal ItemsControl ProjectTypeItems;
    internal Grid TypeSelectPanel;
    internal Border ListBorder;
    internal Path ListPath;
    internal Border KanbanBorder;
    internal Path KanbanPath;
    internal Border TimelineBorder;
    internal Path TimelinePath;
    internal Image HeadProImage;
    internal CustomComboBox PrivacyComboBox;
    internal CheckBox AddListIsHideCheckBox;
    internal Grid MutedGrid;
    internal CheckBox MutedCheckBox;
    internal Button SaveButton;
    internal Border BackBorder;
    internal Image ViewModeDisplayBorder;
    internal EmjTextBlock DisplayName;
    private bool _contentLoaded;

    public EditProjectDialog(string groupId = "", string teamId = null)
    {
      teamId = teamId == "c1a7e08345e444dea187e21a692f0d7a" ? (string) null : teamId;
      EmojiSearchHelper.Init();
      this.InitializeComponent();
      this._model.InAll = true;
      this._model.TeamId = teamId;
      this._model.groupId = groupId;
      this._model.IsNew = true;
      this.DataContext = (object) this._model;
      this.LoadProjectGroup(teamId, groupId);
      this._isAddProject = true;
      this.ColorItems.SetSelectedColor(string.Empty);
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(this.AddListNameTextBox.Text);
      this.LoadTeamInfo(teamId);
      this.AddListNameTextBox.EnterUp += new EventHandler(this.OnTextKeyUp);
      this.SetSelectedView("list");
      this.SaveButton.Content = (object) Utils.GetString("Add");
      this.SetBackground();
      this.ColorItems.ColorSelect += new EventHandler<string>(this.OnColorChanged);
    }

    public EditProjectDialog(ticktick_WPF.ViewModels.ProjectViewModel model)
    {
      EmojiSearchHelper.Init();
      this.InitializeComponent();
      this.WTitle.Text = Utils.GetString("EditList");
      this.ColorItems.SetSelectedColor(model.Color);
      this.CheckEmojiStart(model);
      this._model = model;
      this.DataContext = (object) this._model;
      this.MutedGrid.Visibility = model.IsShareList() ? Visibility.Visible : Visibility.Collapsed;
      this.SetProjectIcon(this._model.Kind);
      this.LoadProjectGroup(model.TeamId, model.groupId);
      this._isAddProject = false;
      this._originalName = this._model.Name;
      this._originColor = this._model.Color;
      this.LoadTeamInfo(model.TeamId);
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.id));
      if ((projectModel != null ? (projectModel.IsProjectPermit() ? 1 : 0) : 0) == 0)
      {
        this.ProjectTypeComboBox.Visibility = Visibility.Collapsed;
        this.TypeText.Visibility = Visibility.Collapsed;
      }
      this.SetSelectedView(model.originalProject?.viewMode ?? "list");
      this.CheckTimeline(model.Kind);
      this.AddListNameTextBox.EnterUp += new EventHandler(this.OnTextKeyUp);
      this.DisplayName.Text = this.EmojiText.Text.Trim() + this.AddListNameTextBox.Text;
      this.SetBackground();
      this.ColorItems.ColorSelect += new EventHandler<string>(this.OnColorChanged);
      if (!((bool?) model.originalProject?.closed).GetValueOrDefault())
        return;
      this.TypeSelectPanel.Visibility = Visibility.Collapsed;
    }

    private void SetBackground()
    {
      if (string.IsNullOrEmpty(this._model.Color) || this._model.Color.ToLower() == "transparent" || this._model.Color.Length == 9 && this._model.Color.StartsWith("#00"))
        this.BackBorder.SetResourceReference(Control.BackgroundProperty, (object) "PrimaryColor");
      else
        this.BackBorder.Background = (Brush) ThemeUtil.GetColorInString(this._model.Color);
    }

    private void OnColorChanged(object sender, string e)
    {
      this._model.Color = this.ColorItems.GetSelectedColor();
      this.SetBackground();
    }

    private void OnTextKeyUp(object sender, EventArgs e)
    {
      if (!this.SaveButton.IsEnabled || this.EmojiSelectPopup.IsOpen || this.ColorItems.ColorPopup.IsOpen || this.ProjectGroupComboBox.IsOpen)
        return;
      this.SaveProject();
    }

    private void SetProjectIcon(string kind)
    {
      if (kind == Constants.ProjectKind.NOTE.ToString())
        this.ProjectPath.Data = Utils.GetIconData(this._model.IsShareList() ? "IcShareNoteProject" : "IcNoteProject");
      else
        this.ProjectPath.Data = Utils.GetIconData(this._model.IsShareList() ? "IcSharedProject" : "IcNormalProject");
    }

    private void CheckEmojiStart(ticktick_WPF.ViewModels.ProjectViewModel model)
    {
      string emojiIcon1 = EmojiHelper.GetEmojiIcon(model.Name);
      if (string.IsNullOrEmpty(emojiIcon1) || !model.Name.StartsWith(emojiIcon1))
        return;
      string emojiIcon2 = EmojiHelper.GetEmojiIcon(model.Name);
      model.Name = model.Name.Remove(0, emojiIcon2.Length);
      this.SetProjectEmoji(emojiIcon2);
    }

    public event EventHandler<ticktick_WPF.ViewModels.ProjectViewModel> OnProjectSaved;

    private void LoadTeamInfo(string teamId)
    {
      List<TeamModel> teams = CacheManager.GetTeams();
      List<TeamModel> list = teams != null ? teams.Where<TeamModel>((Func<TeamModel, bool>) (team => !team.expired)).ToList<TeamModel>() : (List<TeamModel>) null;
      if (list == null || list.Count == 0)
        return;
      this.ProjectBelongTo.Visibility = list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
      if (string.IsNullOrEmpty(this._model.id))
      {
        this.TeamNamePanel.Visibility = Visibility.Collapsed;
        this.SelectTeamComBox.Visibility = Visibility.Visible;
        this.LoadTeamComBox(list, teamId);
        this.PrivacyComboBox.Visibility = string.IsNullOrEmpty(teamId) ? Visibility.Collapsed : Visibility.Visible;
      }
      else
      {
        this.TeamNamePanel.Visibility = Visibility.Visible;
        this.SelectTeamComBox.Visibility = Visibility.Collapsed;
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.ProjectId == this._model.originalProject.ProjectId));
        if (list.Count != 0)
        {
          if (projectModel == null)
            return;
          bool? closed = projectModel.closed;
          if (!closed.HasValue)
            return;
          closed = projectModel.closed;
          if (!closed.Value)
            return;
        }
        this.SwitchListTypeText.Visibility = Visibility.Collapsed;
      }
    }

    private void LoadTeamComBox(List<TeamModel> teams, string teamId)
    {
      ObservableCollection<ComboBoxViewModel> items1 = new ObservableCollection<ComboBoxViewModel>();
      foreach (TeamModel team in teams)
      {
        ObservableCollection<ComboBoxViewModel> observableCollection = items1;
        ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) team.id, team.name);
        comboBoxViewModel.Selected = team.id == teamId;
        observableCollection.Add(comboBoxViewModel);
      }
      ObservableCollection<ComboBoxViewModel> observableCollection1 = items1;
      ComboBoxViewModel comboBoxViewModel1 = new ComboBoxViewModel((object) "c1a7e08345e444dea187e21a692f0d7a", Utils.GetString("Personal"));
      comboBoxViewModel1.Selected = string.IsNullOrEmpty(teamId);
      observableCollection1.Add(comboBoxViewModel1);
      this.SelectTeamComBox.Init<ComboBoxViewModel>(items1, (ComboBoxViewModel) null);
      ObservableCollection<ComboBoxViewModel> items2 = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel2 = new ComboBoxViewModel((object) "0", Utils.GetString("PrivateToMe"));
      comboBoxViewModel2.Selected = true;
      items2.Add(comboBoxViewModel2);
      items2.Add(new ComboBoxViewModel((object) "1", string.Format(Utils.GetString("PublicToTeam"), (object) teams[0].name)));
      this.PrivacyComboBox.Init<ComboBoxViewModel>(items2, (ComboBoxViewModel) null);
    }

    private void OnTeamSelected(object sender, ComboBoxViewModel e)
    {
      string teamId = e.Value.ToString();
      this.LoadProjectGroup(teamId, this._model.groupId);
      this._model.TeamId = teamId == "c1a7e08345e444dea187e21a692f0d7a" ? (string) null : teamId;
      this.PrivacyComboBox.Visibility = string.IsNullOrEmpty(this._model.TeamId) ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void OnLoad(object sender, RoutedEventArgs e)
    {
      EditProjectDialog element = this;
      if (element._model?.originalProject?.ProjectId != null)
      {
        // ISSUE: reference to a compiler-generated method
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(element.\u003COnLoad\u003Eb__17_0));
        if ((projectModel != null ? (projectModel.IsProjectPermit() ? 1 : 0) : 0) == 0)
        {
          element.AddListNameTextBox.ReadOnly = true;
          element.EmojiSelectGrid.IsEnabled = false;
          FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
          Keyboard.Focus((IInputElement) element);
          // ISSUE: reference to a compiler-generated method
          element.\u003C\u003En__0((EventArgs) e);
          return;
        }
      }
      await Task.Delay(100);
      element.AddListNameTextBox.FocusEnd();
    }

    private async Task LoadProjectGroup(string teamId = "", string groupId = "")
    {
      if (teamId == "c1a7e08345e444dea187e21a692f0d7a")
        teamId = string.Empty;
      List<ProjectGroupModel> projectGroups = CacheManager.GetProjectGroups();
      IOrderedEnumerable<ProjectGroupModel> orderedEnumerable = (!string.IsNullOrEmpty(teamId) ? (IEnumerable<ProjectGroupModel>) projectGroups.Where<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (group => group.teamId == teamId)).ToList<ProjectGroupModel>() : (IEnumerable<ProjectGroupModel>) projectGroups.Where<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (group => string.IsNullOrEmpty(group.teamId))).ToList<ProjectGroupModel>()).OrderBy<ProjectGroupModel, long?>((Func<ProjectGroupModel, long?>) (g => g.sortOrder));
      ObservableCollection<ComboBoxViewModel> observableCollection1 = new ObservableCollection<ComboBoxViewModel>();
      observableCollection1.Add(new ComboBoxViewModel((object) "NONE", Utils.GetString("none")));
      ObservableCollection<ComboBoxViewModel> items1 = observableCollection1;
      foreach (ProjectGroupModel projectGroupModel in (IEnumerable<ProjectGroupModel>) orderedEnumerable)
      {
        ObservableCollection<ComboBoxViewModel> observableCollection2 = items1;
        ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) projectGroupModel.id, projectGroupModel.name);
        comboBoxViewModel.Selected = projectGroupModel.id == groupId;
        observableCollection2.Add(comboBoxViewModel);
      }
      items1.Add(new ComboBoxViewModel((object) "AddProjectGroup", Utils.GetString("AddFolder"))
      {
        Image = Utils.GetIcon("IcAdd"),
        ShowImage = true,
        CanSelect = false
      });
      this.ProjectGroupComboBox.Init<ComboBoxViewModel>(items1, (ComboBoxViewModel) null);
      ObservableCollection<ComboBoxViewModel> items2 = new ObservableCollection<ComboBoxViewModel>();
      items2.Add(new ComboBoxViewModel((object) "TASK", Utils.GetString("TaskList")));
      ComboBoxViewModel comboBoxViewModel1 = new ComboBoxViewModel((object) "NOTE", Utils.GetString("NoteList"));
      comboBoxViewModel1.Selected = this._model?.Kind == Constants.ProjectKind.NOTE.ToString();
      items2.Add(comboBoxViewModel1);
      this.ProjectTypeComboBox.Init<ComboBoxViewModel>(items2, (ComboBoxViewModel) null);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.SaveProject();

    private async void SaveProject()
    {
      EditProjectDialog editProjectDialog = this;
      editProjectDialog.CheckValid();
      if (editProjectDialog.AddListNameRepeatTextBlock.IsVisible)
        return;
      if (editProjectDialog.PrivacyComboBox.IsVisible && editProjectDialog.PrivacyComboBox.SelectedIndex == 1)
      {
        if (!await TeamService.CheckFreeTeamShareCount(editProjectDialog._model.TeamId, (System.Windows.Window) editProjectDialog))
          return;
      }
      Constants.ProjectKind projectKind;
      bool? nullable;
      if (!LocalSettings.Settings.ProjectTypeChangeNotified)
      {
        string kind = editProjectDialog._model.Kind;
        projectKind = Constants.ProjectKind.NOTE;
        string str = projectKind.ToString();
        if (kind != str && editProjectDialog.ProjectTypeComboBox.SelectedIndex == 1)
        {
          LocalSettings.Settings.ProjectTypeChangeNotified = true;
          nullable = new CustomerDialog("", Utils.GetString("ProjectTypeChangeMessage"), Utils.GetString("OK"), Utils.GetString("Cancel")).ShowDialog();
          if (!nullable.GetValueOrDefault())
            return;
        }
      }
      editProjectDialog._model.SetName(editProjectDialog.EmojiText.Text.Trim() + editProjectDialog.AddListNameTextBox.Text);
      ticktick_WPF.ViewModels.ProjectViewModel model1 = editProjectDialog._model;
      bool? isChecked = editProjectDialog.AddListIsHideCheckBox.IsChecked;
      nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      int num = (int) nullable ?? 1;
      model1.InAll = num != 0;
      ticktick_WPF.ViewModels.ProjectViewModel model2 = editProjectDialog._model;
      projectKind = editProjectDialog.ProjectTypeComboBox.SelectedIndex == 1 ? Constants.ProjectKind.NOTE : Constants.ProjectKind.TASK;
      string str1 = projectKind.ToString();
      model2.Kind = str1;
      string kind1 = editProjectDialog._model.Kind;
      projectKind = Constants.ProjectKind.NOTE;
      string str2 = projectKind.ToString();
      Constants.SortType sortType1;
      if (kind1 != str2)
      {
        string sortType2 = editProjectDialog._model.sortType;
        sortType1 = Constants.SortType.createdTime;
        string str3 = sortType1.ToString();
        if (!(sortType2 == str3))
        {
          string sortType3 = editProjectDialog._model.sortType;
          sortType1 = Constants.SortType.modifiedTime;
          string str4 = sortType1.ToString();
          if (!(sortType3 == str4))
            goto label_12;
        }
        ticktick_WPF.ViewModels.ProjectViewModel model3 = editProjectDialog._model;
        sortType1 = Constants.SortType.sortOrder;
        string str5 = sortType1.ToString();
        model3.sortType = str5;
label_12:
        if (editProjectDialog._model.SortOption != null)
        {
          string orderBy1 = editProjectDialog._model.SortOption.orderBy;
          sortType1 = Constants.SortType.createdTime;
          string str6 = sortType1.ToString();
          if (!(orderBy1 == str6))
          {
            string orderBy2 = editProjectDialog._model.SortOption.orderBy;
            sortType1 = Constants.SortType.modifiedTime;
            string str7 = sortType1.ToString();
            if (!(orderBy2 == str7))
              goto label_16;
          }
          SortOption sortOption = editProjectDialog._model.SortOption;
          sortType1 = Constants.SortType.sortOrder;
          string str8 = sortType1.ToString();
          sortOption.orderBy = str8;
        }
      }
label_16:
      string kind2 = editProjectDialog._model.Kind;
      projectKind = Constants.ProjectKind.NOTE;
      string str9 = projectKind.ToString();
      if (kind2 == str9)
      {
        if (!string.IsNullOrEmpty(editProjectDialog._model.sortType))
        {
          string sortType4 = editProjectDialog._model.sortType;
          sortType1 = Constants.SortType.dueDate;
          string str10 = sortType1.ToString();
          if (!(sortType4 == str10))
          {
            string sortType5 = editProjectDialog._model.sortType;
            sortType1 = Constants.SortType.priority;
            string str11 = sortType1.ToString();
            if (!(sortType5 == str11))
              goto label_21;
          }
        }
        ticktick_WPF.ViewModels.ProjectViewModel model4 = editProjectDialog._model;
        sortType1 = Constants.SortType.sortOrder;
        string str12 = sortType1.ToString();
        model4.sortType = str12;
label_21:
        if (editProjectDialog._model.SortOption != null)
        {
          string groupBy1 = editProjectDialog._model.SortOption.groupBy;
          sortType1 = Constants.SortType.dueDate;
          string str13 = sortType1.ToString();
          if (!(groupBy1 == str13))
          {
            string groupBy2 = editProjectDialog._model.SortOption.groupBy;
            sortType1 = Constants.SortType.priority;
            string str14 = sortType1.ToString();
            if (!(groupBy2 == str14) && !string.IsNullOrEmpty(editProjectDialog._model.SortOption.groupBy))
              goto label_25;
          }
          SortOption sortOption1 = editProjectDialog._model.SortOption;
          sortType1 = Constants.SortType.sortOrder;
          string str15 = sortType1.ToString();
          sortOption1.groupBy = str15;
label_25:
          string orderBy3 = editProjectDialog._model.SortOption.orderBy;
          sortType1 = Constants.SortType.dueDate;
          string str16 = sortType1.ToString();
          if (!(orderBy3 == str16))
          {
            string orderBy4 = editProjectDialog._model.SortOption.orderBy;
            sortType1 = Constants.SortType.priority;
            string str17 = sortType1.ToString();
            if (!(orderBy4 == str17) && !string.IsNullOrEmpty(editProjectDialog._model.SortOption.orderBy))
              goto label_28;
          }
          SortOption sortOption2 = editProjectDialog._model.SortOption;
          sortType1 = Constants.SortType.sortOrder;
          string str18 = sortType1.ToString();
          sortOption2.orderBy = str18;
        }
      }
label_28:
      string selectedColor = editProjectDialog.ColorItems.GetSelectedColor();
      editProjectDialog._model.Color = !string.IsNullOrEmpty(selectedColor) ? selectedColor : (string) null;
      if (editProjectDialog._model.Color != null && editProjectDialog._model.Color.Length == 9)
        editProjectDialog._model.Color = editProjectDialog._model.Color.StartsWith("#00") ? "transparent" : "#" + editProjectDialog._model.Color.Substring(3);
      if (editProjectDialog._model.Color != editProjectDialog._originColor)
        ticktick_WPF.Views.Misc.ColorSelector.ColorSelector.TryAddClickEvent(editProjectDialog._model.Color);
      string originGroup = string.IsNullOrEmpty(editProjectDialog._model.groupId) ? "NONE" : editProjectDialog._model.groupId;
      if (editProjectDialog.ProjectGroupComboBox.SelectedItem != null)
        editProjectDialog._model.groupId = editProjectDialog.ProjectGroupComboBox.SelectedItem.Value.ToString() != "" ? editProjectDialog.ProjectGroupComboBox.SelectedItem.Value.ToString() : "NONE";
      string str19 = await editProjectDialog.SaveProject(editProjectDialog._model, originGroup);
      editProjectDialog._model.id = str19;
      UtilLog.Info(string.Format("ProjectMenu.AddOrEditProject : id {0},isAdd {1}", (object) str19, (object) editProjectDialog._model.IsNew));
      EventHandler<ticktick_WPF.ViewModels.ProjectViewModel> onProjectSaved = editProjectDialog.OnProjectSaved;
      if (onProjectSaved != null)
        onProjectSaved((object) editProjectDialog, editProjectDialog._model);
      editProjectDialog.Close();
    }

    private bool IsProjectNameExist(string projectName, string projectId, string teamId)
    {
      return CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.name.Trim() == projectName.Trim() && p.id != projectId && p.teamId == teamId && p.IsBelongGroup(this.ProjectGroupComboBox.SelectedItem.Value.ToString()) && !p.Isinbox)) != null;
    }

    private static bool CheckProjectTitle(string title)
    {
      char[] anyOf = new char[10]
      {
        '\\',
        '/',
        '\'',
        '"',
        ':',
        '*',
        '?',
        '<',
        '>',
        '|'
      };
      return title.IndexOfAny(anyOf) != -1;
    }

    private async Task<string> SaveProject(ticktick_WPF.ViewModels.ProjectViewModel model, string originGroup)
    {
      ProjectModel projectModel1;
      if (model.IsNew)
        projectModel1 = (ProjectModel) null;
      else
        projectModel1 = await ProjectDao.GetProjectById(model.id);
      ProjectModel project = projectModel1;
      ProjectModel projectModel2 = project;
      if (projectModel2 == null)
      {
        ProjectModel projectModel3 = new ProjectModel();
        projectModel3._Id = model._Id;
        projectModel3.id = model.id;
        projectModel3.userCount = model.userCount;
        projectModel3.closed = model.closed;
        projectModel3.sortType = model.sortType;
        projectModel3.permission = model.originalProject?.permission;
        projectModel3.Timeline = model.originalProject?.Timeline;
        projectModel3.teamMemberPermission = model.teamMemberPermission;
        projectModel3.SortOption = model.SortOption;
        projectModel3.userid = LocalSettings.Settings.LoginUserId;
        projectModel2 = projectModel3;
      }
      project = projectModel2;
      string oldGroup = project.groupId;
      project.name = model.Name;
      project.color = model.Color;
      project.inAll = model.InAll;
      project.groupId = model.groupId;
      project.sortOrder = model.sortOrder;
      project.sync_status = model.sync_status;
      project.muted = model.Muted;
      project.teamId = model.TeamId;
      project.kind = model.Kind;
      project.needAudit = new bool?(model.needAudit);
      project.openToTeam = model.openToTeam;
      project.viewMode = model.ViewMode;
      if (model.Kind == Constants.ProjectKind.NOTE.ToString() && !LocalSettings.Settings.IsNoteEnabled)
      {
        LocalSettings.Settings.IsNoteEnabled = true;
        LocalSettings.Settings.Save();
      }
      if (string.IsNullOrEmpty(project.id))
        project.id = Utils.GetGuid();
      if (!this._isAddProject)
      {
        if (project.sync_status == Constants.SyncStatus.SYNC_DONE.ToString())
          project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
        if (!project.IsBelongGroup(originGroup) && project.IsBelongGroup(project.groupId))
          project.sortOrder = project.IsBelongGroup(string.Empty) ? ProjectDragHelper.GetNewSortOrder(true, originGroup, project.teamId) : ProjectDragHelper.GetNewSortOrder(true, project.groupId, false, project.teamId);
        ProjectModel projectModel = project;
        projectModel._Id = await ProjectDao.TryUpdateProject(project);
        projectModel = (ProjectModel) null;
        if (model.originalProject != null && project.muted != model.originalProject.muted)
          ReminderCalculator.AssembleReminders();
      }
      else
      {
        project.sync_status = Constants.SyncStatus.SYNC_NEW.ToString();
        if (!string.IsNullOrEmpty(project.teamId))
        {
          project.openToTeam = new bool?(this.PrivacyComboBox.SelectedIndex == 1);
          TeamModel teamById = CacheManager.GetTeamById(project.teamId);
          if (teamById != null && !teamById.open)
          {
            teamById.open = true;
            await TeamDao.UpdateTeam(teamById);
          }
        }
        else if (!LocalSettings.Settings.ExpandPersonalSection)
          LocalSettings.Settings.ExpandPersonalSection = true;
        if (!string.IsNullOrEmpty(project.groupId))
        {
          ProjectGroupModel group = CacheManager.GetProjectGroups().FirstOrDefault<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => p.id == project.groupId));
          if (group != null && !group.open)
          {
            group.open = true;
            int num = await BaseDao<ProjectGroupModel>.UpdateAsync(group);
            CacheManager.UpdateProjectGroup(group);
          }
          project.sortOrder = group != null ? ProjectDragHelper.GetNewSortOrder(true, group.id, false, project.teamId) : ProjectDao.GetNewProjectOrder(project.teamId);
          group = (ProjectGroupModel) null;
        }
        else
          project.sortOrder = ProjectDao.GetNewProjectOrder(project.teamId);
        await ProjectDao.CreateProject(project);
      }
      if (project.IsShareList() && oldGroup != project.groupId)
        ProjectGroupDao.CheckGroupGroupBy(oldGroup);
      string id = project.id;
      oldGroup = (string) null;
      return id;
    }

    private async void OnProjectGroupEdit(object sender, ProjectGroupModel projectGroup)
    {
      this._model.groupId = projectGroup.id;
      await this.LoadProjectGroup(this._model.TeamId, this._model.groupId);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((System.Windows.Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void CheckValid(object sender, EventArgs e) => this.CheckValid();

    private async void CheckExist()
    {
      if (this.IsProjectNameExist(this.EmojiText.Text + this.AddListNameTextBox.Text, this._model.id, this._model.TeamId))
      {
        this.AddListNameRepeatTextBlock.Text = Utils.GetString("AddOrEditProjectNameRepeat");
        this.AddListNameRepeatTextBlock.Visibility = Visibility.Visible;
      }
      else if (this.AddListNameRepeatTextBlock.Text == Utils.GetString("AddOrEditProjectNameRepeat"))
        this.AddListNameRepeatTextBlock.Visibility = Visibility.Collapsed;
      this.SaveButton.IsEnabled = this.AddListNameRepeatTextBlock.Visibility != 0;
      this.DisplayName.Text = this.EmojiText.Text.Trim() + this.AddListNameTextBox.Text;
    }

    private async void CheckValid()
    {
      string str = this.EmojiText.Text + this.AddListNameTextBox.Text;
      if (string.IsNullOrEmpty(str.Trim()))
      {
        this.AddListNameRepeatTextBlock.Text = Utils.GetString("AddOrEditProjectNameCantNull");
        this.AddListNameRepeatTextBlock.Visibility = Visibility.Visible;
      }
      else if (str[0] == '#' || str[0] == '＃')
      {
        this.AddListNameRepeatTextBlock.Text = Utils.GetString("ListNameBeginError");
        this.AddListNameRepeatTextBlock.Visibility = Visibility.Visible;
      }
      else if (!NameUtils.IsValidNameNoCheckSharp(str, false))
      {
        this.AddListNameRepeatTextBlock.Text = Utils.GetString("ListNameCantContain");
        this.AddListNameRepeatTextBlock.Visibility = Visibility.Visible;
      }
      else if (this.IsProjectNameExist(str, this._model.id, this._model.TeamId))
      {
        this.AddListNameRepeatTextBlock.Text = Utils.GetString("AddOrEditProjectNameRepeat");
        this.AddListNameRepeatTextBlock.Visibility = Visibility.Visible;
      }
      else
        this.AddListNameRepeatTextBlock.Visibility = Visibility.Collapsed;
      this.SaveButton.IsEnabled = this.AddListNameRepeatTextBlock.Visibility != 0;
      this.DisplayName.Text = this.EmojiText.Text.Trim() + this.AddListNameTextBox.Text;
    }

    private async void OnSwitchListClick(object sender, MouseButtonEventArgs e)
    {
      EditProjectDialog editProjectDialog = this;
      if (string.IsNullOrEmpty(editProjectDialog._model.TeamId))
      {
        List<TeamModel> list = CacheManager.GetTeams().Where<TeamModel>((Func<TeamModel, bool>) (team => !team.expired)).ToList<TeamModel>();
        if (list.Count == 1)
        {
          await editProjectDialog.TryUpgradeProject(editProjectDialog._model.originalProject.ProjectId, list[0].id);
        }
        else
        {
          if (list.Count <= 1)
            return;
          List<EntityViewModel> entityViewModelList = new List<EntityViewModel>();
          foreach (TeamModel teamModel in list)
            entityViewModelList.Add(new EntityViewModel()
            {
              Key = teamModel.id,
              Title = string.Format(Utils.GetString("TeamName"), (object) teamModel.name)
            });
          editProjectDialog.ProjectTypeItems.ItemsSource = (IEnumerable) entityViewModelList;
          editProjectDialog.SelectAddProjectPopup.IsOpen = true;
        }
      }
      else
      {
        ErrorModel errorModel = await Communicator.DegradeTeamProject(editProjectDialog._model.originalProject.id);
        if (errorModel == null)
        {
          editProjectDialog._model.TeamId = (string) null;
          Utils.Toast(Utils.GetString("DowngradeSuccessfulMessage"));
          // ISSUE: reference to a compiler-generated method
          ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(editProjectDialog.\u003COnSwitchListClick\u003Eb__30_1));
          if (project != null)
          {
            project.teamId = (string) null;
            project.groupId = (string) null;
            project.sortOrder = TeamDao.GetNewProjectInPersonal();
            project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
            int num = await ProjectDao.TryUpdateProject(project);
            SyncManager.Sync();
          }
          ListViewContainer.ReloadProjectData(false);
        }
        else
        {
          // ISSUE: reference to a compiler-generated method
          TeamModel teamModel = CacheManager.GetTeams().FirstOrDefault<TeamModel>(new Func<TeamModel, bool>(editProjectDialog.\u003COnSwitchListClick\u003Eb__30_2));
          switch (errorModel.errorCode)
          {
            case "no_team_project_permission":
              new CustomerDialog(Utils.GetString("TeamToPersonError"), Utils.GetString("NoProjectExist"), Utils.GetString("GotIt"), "").ShowDialog();
              break;
            case "degrade_team_project_failed":
              new CustomerDialog(Utils.GetString("TeamToPersonError"), Utils.GetString("TeamToPersonNoPermission"), Utils.GetString("GotIt"), "").ShowDialog();
              break;
            case "not_project_owner":
              new CustomerDialog(Utils.GetString("TeamToPersonError"), Utils.GetString("NotOwnerTeamToPerson"), Utils.GetString("GotIt"), "").ShowDialog();
              break;
            case "team_expired":
              if (new CustomerDialog(Utils.GetString("TeamToPersonError"), string.Format(Utils.GetString("TeamExpiredTeamToPerson"), (object) (teamModel?.name ?? "")), Utils.GetString("GotIt"), "").ShowDialog().GetValueOrDefault())
              {
                string signOnToken = await Communicator.GetSignOnToken();
                Utils.TryProcessStartUrl(string.Format(BaseUrl.GetApiDomain() + "/sign/autoSignOn?token={0}&dest={1}", (object) signOnToken, (object) string.Format(Utils.GetString("RenewTeamUrl"), (object) editProjectDialog._model.TeamId)));
                break;
              }
              break;
          }
        }
        editProjectDialog.Close();
      }
    }

    private async Task TryUpgradeProject(string projectId, string teamId)
    {
      EditProjectDialog editProjectDialog = this;
      ErrorModel errorModel = await Communicator.UpgradeTeamProject(projectId, teamId);
      if (errorModel == null)
      {
        ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == this._model.id));
        if (project != null)
        {
          project.teamId = teamId;
          project.groupId = (string) null;
          project.sortOrder = TeamDao.GetNewProjectOrderInTeam(teamId);
          project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          int num = await ProjectDao.TryUpdateProject(project);
          SyncManager.Sync();
        }
        ListViewContainer.ReloadProjectData(false);
      }
      else
      {
        TeamModel teamModel1 = CacheManager.GetTeams().FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (t => t.id == teamId));
        switch (errorModel.errorCode)
        {
          case "no_team_project_permission":
            new CustomerDialog(Utils.GetString("PersonToTeamError"), Utils.GetString("NoProjectExist"), Utils.GetString("GotIt"), "").ShowDialog();
            break;
          case "not_project_owner":
            new CustomerDialog(Utils.GetString("PersonToTeamError"), Utils.GetString("NotOwnerPersonToTeam"), Utils.GetString("GotIt"), "").ShowDialog();
            break;
          case "upgrade_team_project_failed":
            new CustomerDialog(Utils.GetString("PersonToTeamError"), string.Format(Utils.GetString("SbNotTeamMember"), (object) (teamModel1?.name ?? "")), Utils.GetString("GotIt"), "").ShowDialog();
            break;
          case "team_expired":
            TeamModel teamModel2 = CacheManager.GetTeams().FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (t => t.id == teamId));
            if (new CustomerDialog(Utils.GetString("PersonToTeamError"), string.Format(Utils.GetString("TeamExpiredPersonToTeam"), (object) (teamModel2?.name ?? "")), Utils.GetString("GotIt"), "").ShowDialog().GetValueOrDefault())
            {
              string signOnToken = await Communicator.GetSignOnToken();
              Utils.TryProcessStartUrl(string.Format(BaseUrl.GetApiDomain() + "/sign/autoSignOn?token={0}&dest={1}", (object) signOnToken, (object) string.Format(Utils.GetString("RenewTeamUrl"), (object) teamId)));
              break;
            }
            break;
        }
      }
      editProjectDialog.Close();
    }

    private async void OnSelectTypeClick(object sender, MouseButtonEventArgs e)
    {
      this.SelectAddProjectPopup.IsOpen = false;
      if (!(sender is Button button) || !(button.Tag is string tag))
        return;
      this.TryUpgradeProject(tag);
    }

    private async void TryUpgradeProject(string teamId)
    {
      if (string.IsNullOrEmpty(this._model.id))
        return;
      await this.TryUpgradeProject(this._model.originalProject.ProjectId, teamId);
    }

    public void OnCancel()
    {
      if (this.ProjectGroupComboBox.IsOpen || this.ProjectTypeComboBox.IsOpen)
        return;
      this.Close();
    }

    public void Ok()
    {
      if (this.ProjectGroupComboBox.IsOpen || this.ProjectTypeComboBox.IsOpen || !this.SaveButton.IsEnabled)
        return;
      this.SaveProject();
    }

    private void ShowEmojiSelector(object sender, MouseButtonEventArgs e)
    {
      this.EmojiSelector.GetItems();
      this.EmojiSelector.SetCanReset(!string.IsNullOrEmpty(this.EmojiText.Text));
      this.EmojiSelectPopup.IsOpen = true;
    }

    private void SetProjectEmoji(string emoji)
    {
      this.EmojiText.Text = emoji ?? string.Empty;
      this.ProjectPathGrid.Visibility = string.IsNullOrEmpty(emoji) ? Visibility.Visible : Visibility.Collapsed;
      this.AddListNameTextBox.MaxLength = 64 - this.EmojiText.Text.Length;
      this.DisplayName.Text = this.EmojiText.Text.Trim() + this.AddListNameTextBox.Text;
    }

    private void OnEmojiSelected(string emoji, bool closePopup)
    {
      this.SetProjectEmoji(emoji);
      this.CheckValid();
      if (!closePopup)
        return;
      this.EmojiSelectPopup.IsOpen = false;
    }

    private void OnKindChanged(object sender, ComboBoxViewModel e)
    {
      if (!(e.Value is string kind))
        return;
      this._model.Kind = kind;
      this.SetProjectIcon(kind);
      this.CheckTimeline(kind);
    }

    private void CheckTimeline(string kind)
    {
      bool flag = kind == Constants.ProjectKind.NOTE.ToString();
      this.TimelineBorder.Opacity = flag ? 0.6 : 1.0;
      this.TimelineBorder.Cursor = flag ? Cursors.No : Cursors.Hand;
      if (!flag || !(this._model.ViewMode == "timeline"))
        return;
      this.SetSelectedView("list");
    }

    private void OnGroupSelected(object sender, ComboBoxViewModel e)
    {
      if (e.Value.ToString() == "AddProjectGroup")
      {
        AddProjectGroupDialog projectGroupDialog = new AddProjectGroupDialog(this._model.id, this._model.TeamId);
        projectGroupDialog.Owner = System.Windows.Window.GetWindow((DependencyObject) this);
        projectGroupDialog.ProjectGroupEdit += new EventHandler<ProjectGroupModel>(this.OnProjectGroupEdit);
        projectGroupDialog.ShowDialog();
        this.Activate();
      }
      else
        this.CheckExist();
    }

    private void OnPrivacySelected(object sender, ComboBoxViewModel e)
    {
      this._model.openToTeam = new bool?(e.Value.ToString() == "1");
      this.SetProjectIcon(this._model.Kind);
      this.MutedGrid.Visibility = this._model.IsShareList() ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SwitchViewClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || frameworkElement.Cursor == Cursors.No || !(frameworkElement.Tag is string tag))
        return;
      this.SetSelectedView(tag);
    }

    private void SetSelectedView(string mode)
    {
      if (mode == "timeline" && !ProChecker.CheckPro(ProType.TimeLine, (System.Windows.Window) this))
        return;
      this.ListBorder.SetResourceReference(Control.BackgroundProperty, mode == "list" ? (object) "PrimaryColor5" : (object) "BaseColorOpacity4");
      this.KanbanBorder.SetResourceReference(Control.BackgroundProperty, mode == "kanban" ? (object) "PrimaryColor5" : (object) "BaseColorOpacity4");
      this.TimelineBorder.SetResourceReference(Control.BackgroundProperty, mode == "timeline" ? (object) "PrimaryColor5" : (object) "BaseColorOpacity4");
      this.ListBorder.BorderThickness = mode == "list" ? new Thickness(1.0) : new Thickness(0.0);
      this.KanbanBorder.BorderThickness = mode == "kanban" ? new Thickness(1.0) : new Thickness(0.0);
      this.TimelineBorder.BorderThickness = mode == "timeline" ? new Thickness(1.0) : new Thickness(0.0);
      this.ListPath.SetResourceReference(Shape.FillProperty, mode == "list" ? (object) "PrimaryColor" : (object) "BaseColorOpacity40");
      this.KanbanPath.SetResourceReference(Shape.FillProperty, mode == "kanban" ? (object) "PrimaryColor" : (object) "BaseColorOpacity40");
      this.TimelinePath.SetResourceReference(Shape.FillProperty, mode == "timeline" ? (object) "PrimaryColor" : (object) "BaseColorOpacity40");
      this._model.ViewMode = mode;
      switch (mode)
      {
        case "list":
          this.ViewModeDisplayBorder.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/Theme/" + (LocalSettings.Settings.ThemeId == "Dark" ? "dark" : "light") + "/ListPreview.png"));
          break;
        case "kanban":
          this.ViewModeDisplayBorder.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/Theme/" + (LocalSettings.Settings.ThemeId == "Dark" ? "dark" : "light") + "/KanbanPreview.png"));
          break;
        case "timeline":
          this.ViewModeDisplayBorder.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/Theme/" + (LocalSettings.Settings.ThemeId == "Dark" ? "dark" : "light") + "/TimelinePreview.png"));
          break;
      }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.InputGrid.IsMouseOver)
        return;
      FocusManager.SetFocusedElement((DependencyObject) this, (IInputElement) this);
      Keyboard.Focus((IInputElement) this);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/editprojectdialog.xaml", UriKind.Relative));
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
          this.Window = (EditProjectDialog) target;
          this.Window.Loaded += new RoutedEventHandler(this.OnLoad);
          this.Window.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
          break;
        case 2:
          this.WTitle = (TextBlock) target;
          break;
        case 3:
          this.InputGrid = (Grid) target;
          break;
        case 4:
          this.AddListNameTextBox = (EmojiEditor) target;
          break;
        case 5:
          this.EmojiSelectGrid = (Grid) target;
          this.EmojiSelectGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowEmojiSelector);
          break;
        case 6:
          this.ProjectPathGrid = (Grid) target;
          break;
        case 7:
          this.ProjectPath = (Path) target;
          break;
        case 8:
          this.SetEmojiIcon = (Path) target;
          break;
        case 9:
          this.EmojiText = (EmjTextBlock) target;
          break;
        case 10:
          this.EmojiSelectPopup = (EscPopup) target;
          break;
        case 11:
          this.EmojiSelector = (EmojiSelectControl) target;
          break;
        case 12:
          this.HideTextBox = (TextBox) target;
          break;
        case 13:
          this.AddListNameRepeatTextBlock = (TextBlock) target;
          break;
        case 14:
          this.ColorItems = (ticktick_WPF.Views.Misc.ColorSelector.ColorSelector) target;
          break;
        case 15:
          this.TypeText = (TextBlock) target;
          break;
        case 16:
          this.ProjectTypeComboBox = (CustomComboBox) target;
          break;
        case 17:
          this.ProjectGroupComboBox = (CustomComboBox) target;
          break;
        case 18:
          this.ProjectBelongTo = (Grid) target;
          break;
        case 19:
          this.TeamNamePanel = (StackPanel) target;
          break;
        case 20:
          this.SwitchListTypeText = (TextBlock) target;
          this.SwitchListTypeText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSwitchListClick);
          break;
        case 21:
          this.SelectTeamComBox = (CustomComboBox) target;
          break;
        case 22:
          this.SelectAddProjectPopup = (EscPopup) target;
          break;
        case 23:
          this.ProjectTypeItems = (ItemsControl) target;
          break;
        case 25:
          this.TypeSelectPanel = (Grid) target;
          break;
        case 26:
          this.ListBorder = (Border) target;
          this.ListBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchViewClick);
          break;
        case 27:
          this.ListPath = (Path) target;
          break;
        case 28:
          this.KanbanBorder = (Border) target;
          this.KanbanBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchViewClick);
          break;
        case 29:
          this.KanbanPath = (Path) target;
          break;
        case 30:
          this.TimelineBorder = (Border) target;
          this.TimelineBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchViewClick);
          break;
        case 31:
          this.TimelinePath = (Path) target;
          break;
        case 32:
          this.HeadProImage = (Image) target;
          break;
        case 33:
          this.PrivacyComboBox = (CustomComboBox) target;
          break;
        case 34:
          this.AddListIsHideCheckBox = (CheckBox) target;
          break;
        case 35:
          this.MutedGrid = (Grid) target;
          break;
        case 36:
          this.MutedCheckBox = (CheckBox) target;
          break;
        case 37:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 38:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 39:
          this.BackBorder = (Border) target;
          break;
        case 40:
          this.ViewModeDisplayBorder = (Image) target;
          break;
        case 41:
          this.DisplayName = (EmjTextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 24)
        return;
      ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnSelectTypeClick);
    }
  }
}
