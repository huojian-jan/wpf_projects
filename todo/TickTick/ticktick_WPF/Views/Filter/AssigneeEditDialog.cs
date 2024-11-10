// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.AssigneeEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class AssigneeEditDialog : ConditionEditDialogBase<string>
  {
    private List<string> _originAssignees = new List<string>();

    public AssigneeEditDialog(List<string> selectedAssignees, bool showAll)
    {
      this.InitializeComponent();
      this._originAssignees = selectedAssignees;
      this.InitData(selectedAssignees, showAll);
      this.OnSelectedValueChanged += (EventHandler<FilterConditionViewModel>) ((sender, e) =>
      {
        FilterListItemViewModel listItemViewModel1 = e != null ? e.ItemsSource.FirstOrDefault<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (item => item.Value.ToString() == "other")) : (FilterListItemViewModel) null;
        if (listItemViewModel1 != null)
          listItemViewModel1.PartSelected = false;
        if (e != null && e.ItemsSource.All<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (item => item.Selected)))
        {
          foreach (FilterListItemViewModel listItemViewModel2 in (Collection<FilterListItemViewModel>) e.ItemsSource)
            listItemViewModel2.Selected = false;
          e.IsAllSelected = true;
        }
        EventHandler<List<string>> selectedAssigneeChanged = this.OnSelectedAssigneeChanged;
        if (selectedAssigneeChanged == null)
          return;
        selectedAssigneeChanged((object) this, this.GetSelectedValues());
      });
    }

    public AssigneeEditDialog(int version)
      : this(new List<string>(), false)
    {
    }

    public event EventHandler<List<string>> OnSelectedAssigneeChanged;

    private void InitData(List<string> selectedAssignees, bool showAll)
    {
      ObservableCollection<FilterListItemViewModel> observableCollection = new ObservableCollection<FilterListItemViewModel>();
      observableCollection.Add(new FilterListItemViewModel()
      {
        Title = Utils.GetString("NotAssigned"),
        Value = (object) "noassignee",
        Icon = "IcNotAssigned",
        ShowIcon = true,
        Selected = selectedAssignees.Contains("noassignee")
      });
      FilterListItemViewModel listItemViewModel1 = new FilterListItemViewModel()
      {
        Title = Utils.GetString("Me"),
        Value = (object) "me",
        Icon = "IcAssignToMe",
        ShowIcon = true,
        Selected = selectedAssignees.Contains("me"),
        IsAssignee = true
      };
      observableCollection.Add(listItemViewModel1);
      FilterListItemViewModel listItemViewModel2 = new FilterListItemViewModel()
      {
        Title = Utils.GetString("AssignedToOthers"),
        Value = (object) "other",
        Icon = "IcAssignToOther",
        ShowIcon = true,
        Selected = selectedAssignees.Contains("other"),
        IsAssignee = true,
        Unfold = true,
        Children = new List<FilterListItemViewModel>()
      };
      observableCollection.Add(listItemViewModel2);
      Dictionary<string, ShareUserModel> allShareUsers = AvatarHelper.GetAllShareUsers();
      allShareUsers.Remove(LocalSettings.Settings.LoginUserId);
      List<ShareUserModel> list = allShareUsers.Values.Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => user.isAccept)).OrderBy<ShareUserModel, string>((Func<ShareUserModel, string>) (user => !string.IsNullOrEmpty(user.displayName) ? user.displayName : user.username)).ToList<ShareUserModel>();
      if (list.Count > 0)
        observableCollection.Add(new FilterListItemViewModel()
        {
          IsSplit = true,
          Value = (object) "split"
        });
      foreach (ShareUserModel shareUserModel in list)
      {
        FilterListItemViewModel listItemViewModel3 = new FilterListItemViewModel();
        listItemViewModel3.Title = string.IsNullOrEmpty(shareUserModel.displayName) ? shareUserModel.username : shareUserModel.displayName;
        listItemViewModel3.Value = (object) shareUserModel.userId.ToString();
        listItemViewModel3.ImageUrl = string.IsNullOrEmpty(shareUserModel.avatarUrl) ? "pack://application:,,,/Assets/avatar-new.png" : shareUserModel.avatarUrl;
        listItemViewModel3.Selected = selectedAssignees.Contains("other") || selectedAssignees.Contains(shareUserModel.userId.ToString());
        listItemViewModel3.IsAssignee = true;
        int? siteId = shareUserModel.siteId;
        int num = 10;
        listItemViewModel3.ShowFeishu = siteId.GetValueOrDefault() == num & siteId.HasValue && Utils.IsDida();
        listItemViewModel3.GroupId = "other";
        FilterListItemViewModel listItemViewModel4 = listItemViewModel3;
        observableCollection.Add(listItemViewModel4);
        listItemViewModel2.Children.Add(listItemViewModel4);
      }
      this.ViewModel = new FilterConditionViewModel()
      {
        Type = CondType.Assignee,
        ShowAll = showAll,
        ItemsSource = observableCollection,
        IsAllSelected = selectedAssignees.Count == 0,
        SupportedLogic = new List<LogicType>()
        {
          LogicType.Or
        }
      };
    }

    public override void Restore()
    {
      this.RestoreSetSelected();
      EventHandler<List<string>> selectedAssigneeChanged = this.OnSelectedAssigneeChanged;
      if (selectedAssigneeChanged == null)
        return;
      selectedAssigneeChanged((object) this, this._originAssignees);
    }

    private void RestoreSetSelected()
    {
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
        listItemViewModel.Selected = this._originAssignees.Contains(listItemViewModel.Value.ToString());
    }

    protected override void SyncOriginal()
    {
      this._originAssignees = this.GetSelectedValues().ToList<string>();
    }

    protected override bool CanSave() => this.GetSelectedValues().Count > 0 || base.CanSave();
  }
}
