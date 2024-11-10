// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ItemSelection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

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
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class ItemSelection : UserControl, ITabControl, IComponentConnector
  {
    public string AccountId;
    public ProjectExtra OriginalData = new ProjectExtra();
    public bool ShowAll;
    public bool ShowSmartAll;
    public bool ShowCalendars;
    public bool ShowFilters;
    public bool ShowSmartProjects;
    public bool ShowTags;
    public bool ShowColumns;
    public string SelectedColumn;
    private bool _tagShowed;
    public bool ShowNoteProject = true;
    public bool ShowSharedProject = true;
    private bool _showIndent;
    private List<SelectableItemViewModel> _data;
    private HashSet<string> _foldIds = new HashSet<string>();
    private SelectableItemViewModel _hoverParent;
    private SelectableItemViewModel _hoverSubParent;
    private Popup _popup;
    private Dictionary<string, List<ColumnModel>> _projectColumnDict = new Dictionary<string, List<ColumnModel>>();
    private readonly PopupLocationInfo _popupTracker = new PopupLocationInfo();
    private readonly PopupLocationInfo _subPopupTracker = new PopupLocationInfo();
    internal Grid Container;
    internal UpDownSelectListView SelectableItems;
    internal EscPopup SubPopup;
    internal UpDownSelectListView SubItems;
    internal EscPopup SubSubPopup;
    internal UpDownSelectListView SubSubItems;
    private bool _contentLoaded;

    public bool BatchMode
    {
      get => this.SelectableItems.CanBatchSelected;
      set => this.SelectableItems.CanBatchSelected = value;
    }

    public bool CanSelectGroup { get; set; }

    public bool OnlyShowPermission { get; set; }

    public bool ShowFilterGroup { get; set; }

    public bool IsCalFilter { get; set; }

    public bool ShowFilterCalendar { get; set; }

    public bool ShowHabitCategory { get; set; }

    public bool ShowAllProjectsCategory { get; set; }

    public bool ShowListGroup { get; set; }

    public bool ShowInbox { get; set; } = true;

    public ItemSelection(Popup popup, bool defaultHoverMode = true)
    {
      this._popup = popup;
      if (this._popup != null)
        this._popup.Closed += (EventHandler) ((o, e) => this.SubPopup.IsOpen = false);
      this.InitializeComponent();
      this.SubPopup.NeedFocus = false;
      this.SubSubPopup.NeedFocus = false;
      this.SelectableItems.NeedHandleItemEnter = defaultHoverMode;
    }

    public event EventHandler<SelectableItemViewModel> ItemSelect;

    public ProjectExtra GetSelectedData()
    {
      if (this._data == null)
        return new ProjectExtra();
      ProjectExtra selectedData = new ProjectExtra()
      {
        FilterIds = this._data.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (p => p is FilterItemViewModel && p.Selected)).Select<SelectableItemViewModel, string>((Func<SelectableItemViewModel, string>) (p => p.Id)).ToList<string>()
      };
      IEnumerable<CalendarGroupViewModel> source1 = this._data.OfType<CalendarGroupViewModel>();
      CalendarGroupViewModel calendarGroupViewModel = source1 != null ? source1.FirstOrDefault<CalendarGroupViewModel>() : (CalendarGroupViewModel) null;
      if (calendarGroupViewModel != null && calendarGroupViewModel.Selected)
      {
        selectedData.SubscribeCalendars.Add("#allSubscribe");
      }
      else
      {
        selectedData.BindAccounts = this._data.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (p => p is BindAccountViewModel && p.Selected)).Select<SelectableItemViewModel, string>((Func<SelectableItemViewModel, string>) (p => p.Id)).ToList<string>();
        selectedData.SubscribeCalendars = this._data.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (p => p is SubscribeCalendarViewModel && p.Selected)).Select<SelectableItemViewModel, string>((Func<SelectableItemViewModel, string>) (p => p.Id)).ToList<string>();
      }
      IEnumerable<ListGroupViewModel> source2 = this._data.OfType<ListGroupViewModel>();
      ListGroupViewModel listGroupItem = source2 != null ? source2.FirstOrDefault<ListGroupViewModel>() : (ListGroupViewModel) null;
      if (listGroupItem != null && listGroupItem.Selected)
      {
        selectedData.ProjectIds.Add("#alllists");
      }
      else
      {
        HashSet<SelectableItemViewModel> allListOrGroup = this.GetAllListOrGroup(listGroupItem, false);
        selectedData.ProjectIds = allListOrGroup.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (p =>
        {
          switch (p)
          {
            case ProjectViewModel _:
            case FilterCalendarViewModel _:
            case FilterHabitViewModel _:
            case AllProjectCategoryViewModel _:
              return p.Selected;
            default:
              return false;
          }
        })).Select<SelectableItemViewModel, string>((Func<SelectableItemViewModel, string>) (p => p.Id)).ToList<string>();
        selectedData.GroupIds = allListOrGroup.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (p => p is ProjectGroupViewModel && p.Selected)).Select<SelectableItemViewModel, string>((Func<SelectableItemViewModel, string>) (p => p.Id)).ToList<string>();
      }
      SelectableItemViewModel selectableItemViewModel1 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (m => m is TagGroupViewModel));
      if (selectableItemViewModel1 != null)
      {
        if (selectableItemViewModel1.Selected)
        {
          selectedData.Tags.Add("*withtags");
        }
        else
        {
          List<SelectableItemViewModel> source3 = new List<SelectableItemViewModel>();
          foreach (SelectableItemViewModel child in selectableItemViewModel1.Children)
          {
            source3.Add(child);
            child.Children.ForEach(new Action<SelectableItemViewModel>(source3.Add));
          }
          selectedData.Tags = source3.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (p => p.Selected)).Select<SelectableItemViewModel, string>((Func<SelectableItemViewModel, string>) (p => p.Id)).ToList<string>();
        }
      }
      else
        selectedData.Tags = this._data.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (p => p is TagViewModel && p.Selected)).Select<SelectableItemViewModel, string>((Func<SelectableItemViewModel, string>) (p => p.Id)).ToList<string>();
      if (selectedData.IsAll)
      {
        SelectableItemViewModel selectableItemViewModel2 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is AllProjectViewModel));
        if (selectableItemViewModel2 != null)
          selectableItemViewModel2.Selected = true;
      }
      this.OriginalData = selectedData;
      return selectedData;
    }

    private HashSet<SelectableItemViewModel> GetAllListOrGroup(
      ListGroupViewModel listGroupItem,
      bool withTeam)
    {
      HashSet<SelectableItemViewModel> listAndGroupData = new HashSet<SelectableItemViewModel>();
      foreach (SelectableItemViewModel selectableItemViewModel in listGroupItem != null ? listGroupItem.Children : this._data.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item =>
      {
        if (!(item.GroupParent is ListGroupViewModel))
        {
          switch (item)
          {
            case FilterCalendarViewModel _:
            case AllProjectCategoryViewModel _:
              break;
            default:
              return item is FilterHabitViewModel;
          }
        }
        return true;
      })).ToList<SelectableItemViewModel>())
      {
        if (!listAndGroupData.Contains(selectableItemViewModel))
        {
          if (selectableItemViewModel is TeamSectionViewModel sectionViewModel)
          {
            foreach (SelectableItemViewModel child in sectionViewModel.Children)
            {
              listAndGroupData.Add(child);
              child.Children.ForEach((Action<SelectableItemViewModel>) (model => listAndGroupData.Add(model)));
            }
          }
          else
          {
            listAndGroupData.Add(selectableItemViewModel);
            selectableItemViewModel.Children.ForEach((Action<SelectableItemViewModel>) (model => listAndGroupData.Add(model)));
          }
        }
      }
      return listAndGroupData;
    }

    public void LoadData(string searchText = null)
    {
      List<SelectableItemViewModel> selectableItemViewModelList1 = new List<SelectableItemViewModel>();
      if (!string.IsNullOrEmpty(this.AccountId))
      {
        this.LoadAccountCalendars((ICollection<SelectableItemViewModel>) selectableItemViewModelList1, this.AccountId);
      }
      else
      {
        if (this.ShowAll)
        {
          List<SelectableItemViewModel> selectableItemViewModelList2 = selectableItemViewModelList1;
          AllProjectViewModel projectViewModel = new AllProjectViewModel();
          projectViewModel.Selected = this.OriginalData != null && this.OriginalData.IsAll && this.BatchMode;
          projectViewModel.InCalFilter = this.IsCalFilter;
          selectableItemViewModelList2.Add((SelectableItemViewModel) projectViewModel);
          if (!this.IsCalFilter)
          {
            List<SelectableItemViewModel> selectableItemViewModelList3 = selectableItemViewModelList1;
            SelectableItemViewModel selectableItemViewModel = new SelectableItemViewModel();
            selectableItemViewModel.IsSplit = true;
            selectableItemViewModel.IsEnable = false;
            selectableItemViewModelList3.Add(selectableItemViewModel);
          }
        }
        if (this.ShowAllProjectsCategory)
        {
          List<SelectableItemViewModel> selectableItemViewModelList4 = selectableItemViewModelList1;
          AllProjectCategoryViewModel categoryViewModel = new AllProjectCategoryViewModel();
          categoryViewModel.Selected = this.OriginalData != null && this.OriginalData.ProjectIds.Contains("ProjectAll2e4c103c57ef480997943206");
          selectableItemViewModelList4.Add((SelectableItemViewModel) categoryViewModel);
          List<SelectableItemViewModel> selectableItemViewModelList5 = selectableItemViewModelList1;
          SelectableItemViewModel selectableItemViewModel = new SelectableItemViewModel();
          selectableItemViewModel.IsSplit = true;
          selectableItemViewModel.IsEnable = false;
          selectableItemViewModelList5.Add(selectableItemViewModel);
        }
        if (this.ShowSmartProjects)
        {
          if (this.ShowSmartAll)
          {
            List<SelectableItemViewModel> selectableItemViewModelList6 = selectableItemViewModelList1;
            AllSmartProjectViewModel projectViewModel = new AllSmartProjectViewModel();
            projectViewModel.Selected = this.OriginalData != null && this.OriginalData.SmartIds.Contains("_special_id_all");
            selectableItemViewModelList6.Add((SelectableItemViewModel) projectViewModel);
          }
          List<SelectableItemViewModel> selectableItemViewModelList7 = selectableItemViewModelList1;
          TodaySmartProjectViewModel projectViewModel1 = new TodaySmartProjectViewModel();
          projectViewModel1.Selected = this.OriginalData != null && this.OriginalData.SmartIds.Contains("_special_id_today");
          selectableItemViewModelList7.Add((SelectableItemViewModel) projectViewModel1);
          List<SelectableItemViewModel> selectableItemViewModelList8 = selectableItemViewModelList1;
          TomorrowSmartProjectViewModel projectViewModel2 = new TomorrowSmartProjectViewModel();
          projectViewModel2.Selected = this.OriginalData != null && this.OriginalData.SmartIds.Contains("_special_id_tomorrow");
          selectableItemViewModelList8.Add((SelectableItemViewModel) projectViewModel2);
          List<SelectableItemViewModel> selectableItemViewModelList9 = selectableItemViewModelList1;
          WeekSmartProjectViewModel projectViewModel3 = new WeekSmartProjectViewModel();
          projectViewModel3.Selected = this.OriginalData != null && this.OriginalData.SmartIds.Contains("_special_id_week");
          selectableItemViewModelList9.Add((SelectableItemViewModel) projectViewModel3);
          List<SelectableItemViewModel> selectableItemViewModelList10 = selectableItemViewModelList1;
          AssignToMeSmartProjectViewModel projectViewModel4 = new AssignToMeSmartProjectViewModel();
          projectViewModel4.Selected = this.OriginalData != null && this.OriginalData.SmartIds.Contains("_special_id_assigned");
          selectableItemViewModelList10.Add((SelectableItemViewModel) projectViewModel4);
        }
        foreach (int sortedPtfType in ProjectDataProvider.SortedPtfTypes)
        {
          switch (sortedPtfType)
          {
            case 0:
              this.LoadProjects(selectableItemViewModelList1, searchText);
              if (this.ShowFilterCalendar)
              {
                bool flag = this.OriginalData.ProjectIds.Contains("Calendar5959a2259161d16d23a4f272");
                if (!flag)
                  flag = CacheManager.GetSubscribeCalendars().Any<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Show != "hidden"));
                if (!flag)
                  flag = CacheManager.GetBindCalendars().Any<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Show != "hidden"));
                if (flag)
                {
                  List<SelectableItemViewModel> selectableItemViewModelList11 = selectableItemViewModelList1;
                  SelectableItemViewModel selectableItemViewModel = new SelectableItemViewModel();
                  selectableItemViewModel.IsSplit = true;
                  selectableItemViewModel.IsEnable = false;
                  selectableItemViewModelList11.Add(selectableItemViewModel);
                  List<SelectableItemViewModel> selectableItemViewModelList12 = selectableItemViewModelList1;
                  FilterCalendarViewModel calendarViewModel = new FilterCalendarViewModel();
                  calendarViewModel.Selected = this.OriginalData.ProjectIds.Contains("Calendar5959a2259161d16d23a4f272");
                  selectableItemViewModelList12.Add((SelectableItemViewModel) calendarViewModel);
                }
              }
              if (this.ShowHabitCategory)
              {
                List<SelectableItemViewModel> selectableItemViewModelList13 = selectableItemViewModelList1;
                SelectableItemViewModel selectableItemViewModel = new SelectableItemViewModel();
                selectableItemViewModel.IsSplit = true;
                selectableItemViewModel.IsEnable = false;
                selectableItemViewModelList13.Add(selectableItemViewModel);
                List<SelectableItemViewModel> selectableItemViewModelList14 = selectableItemViewModelList1;
                FilterHabitViewModel filterHabitViewModel = new FilterHabitViewModel();
                ProjectExtra originalData = this.OriginalData;
                filterHabitViewModel.Selected = originalData != null && originalData.ProjectIds.Contains("Habit2e4c103c57ef480997943206");
                selectableItemViewModelList14.Add((SelectableItemViewModel) filterHabitViewModel);
                continue;
              }
              continue;
            case 1:
              if (this.ShowTags)
              {
                this.LoadTags(selectableItemViewModelList1);
                continue;
              }
              continue;
            case 2:
              if (this.ShowFilters)
              {
                this.LoadFilters(selectableItemViewModelList1, this.BatchMode);
                continue;
              }
              continue;
            case 3:
              if (this.ShowCalendars)
              {
                this.LoadCalendars((ICollection<SelectableItemViewModel>) selectableItemViewModelList1);
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      selectableItemViewModelList1.ToList<SelectableItemViewModel>().ForEach((Action<SelectableItemViewModel>) (model => model.BatchMode = this.BatchMode));
      this._data = selectableItemViewModelList1;
      TagGroupViewModel tagGroup = selectableItemViewModelList1.OfType<TagGroupViewModel>().FirstOrDefault<TagGroupViewModel>();
      if (tagGroup != null && this._foldIds.Contains(tagGroup.Id))
      {
        tagGroup.Open = false;
        this.OnTagGroupChanged(tagGroup, false);
      }
      ListGroupViewModel listGroup = selectableItemViewModelList1.OfType<ListGroupViewModel>().FirstOrDefault<ListGroupViewModel>();
      if (listGroup != null && this._foldIds.Contains(listGroup.Id))
      {
        listGroup.Open = false;
        this.OnListGroupChanged(listGroup, false);
      }
      FilterGroupViewModel filterGroup = selectableItemViewModelList1.OfType<FilterGroupViewModel>().FirstOrDefault<FilterGroupViewModel>();
      if (filterGroup != null && this._foldIds.Contains(filterGroup.Id))
      {
        filterGroup.Open = false;
        this.OnFilterGroupChanged(filterGroup, false);
      }
      CalendarGroupViewModel calGroup = selectableItemViewModelList1.OfType<CalendarGroupViewModel>().FirstOrDefault<CalendarGroupViewModel>();
      if (calGroup != null && this._foldIds.Contains(calGroup.Id))
      {
        calGroup.Open = false;
        this.OnCalendarGroupChanged(calGroup, false);
      }
      this.SetModels(this._data);
    }

    internal void OnSectionGroupSelected(SelectableItemViewModel model)
    {
      switch (model)
      {
        case ListGroupViewModel listGroupViewModel:
          this.OriginalData.ProjectIds.Clear();
          this.OriginalData.GroupIds.Clear();
          this.OriginalData.FilterIds.Clear();
          if (listGroupViewModel.Selected)
          {
            this.OriginalData.ProjectIds.Add(listGroupViewModel.Id);
            break;
          }
          break;
        case TagGroupViewModel tagGroupViewModel:
          this.OriginalData.Tags.Clear();
          this.OriginalData.FilterIds.Clear();
          if (tagGroupViewModel.Selected)
          {
            this.OriginalData.Tags.Add("*withtags");
            break;
          }
          break;
        case CalendarGroupViewModel calendarGroupViewModel:
          this.OriginalData.SubscribeCalendars.Clear();
          this.OriginalData.BindAccounts.Clear();
          this.OriginalData.FilterIds.Clear();
          if (calendarGroupViewModel.Selected)
          {
            this.OriginalData.SubscribeCalendars.Add(calendarGroupViewModel.Id);
            break;
          }
          break;
      }
      this.LoadData();
      EventHandler<SelectableItemViewModel> itemSelect = this.ItemSelect;
      if (itemSelect == null)
        return;
      itemSelect((object) this, model);
    }

    private void SetModels(List<SelectableItemViewModel> models)
    {
      ItemsSourceHelper.SetItemsSource<SelectableItemViewModel>((ItemsControl) this.SelectableItems, models);
    }

    private void LoadAccountCalendars(ICollection<SelectableItemViewModel> models, string accountId)
    {
      BindCalendarAccountModel account = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (acc => acc.Id == accountId));
      if (account == null)
        return;
      List<BindCalendarModel> list = CacheManager.GetBindCalendars().Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.AccountId == account.Id && cal.Accessible)).ToList<BindCalendarModel>();
      Geometry icon = SubscribeCalendarHelper.GetCalendarProjectIconById(accountId);
      if (!list.Any<BindCalendarModel>())
        return;
      foreach (SubscribeCalendarViewModel calendarViewModel in list.Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Accessible)).Select<BindCalendarModel, SubscribeCalendarViewModel>((Func<BindCalendarModel, SubscribeCalendarViewModel>) (cal =>
      {
        return new SubscribeCalendarViewModel(cal.Id, cal.Name, account.Id)
        {
          Selected = this.OriginalData.SubscribeCalendars.Contains(cal.Id) || this.OriginalData.BindAccounts.Contains(account.Id),
          ShowIndent = this._showIndent,
          Icon = icon,
          InCalFilter = this.IsCalFilter
        };
      })))
        models.Add((SelectableItemViewModel) calendarViewModel);
    }

    private void LoadCalendars(ICollection<SelectableItemViewModel> models)
    {
      bool allSelect = this.OriginalData.SubscribeCalendars.Contains("#allSubscribe");
      CalendarGroupViewModel calendarGroupViewModel1 = new CalendarGroupViewModel();
      calendarGroupViewModel1.InCalFilter = this.IsCalFilter;
      calendarGroupViewModel1.Selected = allSelect;
      calendarGroupViewModel1.PartSelected = this.OriginalData.BindAccounts.Any<string>() || this.OriginalData.SubscribeCalendars.Any<string>();
      CalendarGroupViewModel calendarGroupViewModel2 = calendarGroupViewModel1;
      List<BindCalendarAccountModel> list1 = CacheManager.GetBindCalendarAccounts().ToList<BindCalendarAccountModel>();
      if (list1.Any<BindCalendarAccountModel>())
      {
        list1.Sort((Comparison<BindCalendarAccountModel>) ((a, b) =>
        {
          int accountType1 = SubscribeCalendarHelper.GetAccountType(a);
          int accountType2 = SubscribeCalendarHelper.GetAccountType(b);
          return accountType1 == accountType2 && a.CreatedTime.HasValue ? a.CreatedTime.Value.CompareTo((object) b.CreatedTime) : accountType1.CompareTo(accountType2);
        }));
        models.Add((SelectableItemViewModel) calendarGroupViewModel2);
        foreach (BindCalendarAccountModel calendarAccountModel in list1)
        {
          BindCalendarAccountModel account = calendarAccountModel;
          List<BindCalendarModel> list2 = CacheManager.GetBindCalendars().Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.AccountId == account.Id && cal.Show != "hidden")).ToList<BindCalendarModel>();
          if (list2.Any<BindCalendarModel>())
          {
            string title = string.IsNullOrEmpty(account.Description) ? account.Account : account.Description;
            if (account.Site == "feishu")
              title = Utils.GetString("FeishuCalendar");
            BindAccountViewModel accountViewModel1 = new BindAccountViewModel(account.Id, title, account.Kind);
            accountViewModel1.Selected = allSelect || this.OriginalData.BindAccounts.Contains(account.Id);
            accountViewModel1.ShowIndent = true;
            accountViewModel1.InCalFilter = this.IsCalFilter;
            accountViewModel1.IsParent = true;
            accountViewModel1.Open = true;
            BindAccountViewModel accountViewModel2 = accountViewModel1;
            calendarGroupViewModel2.Children.Add((SelectableItemViewModel) accountViewModel2);
            models.Add((SelectableItemViewModel) accountViewModel2);
            IEnumerable<SubscribeCalendarViewModel> calendarViewModels = list2.Select<BindCalendarModel, SubscribeCalendarViewModel>((Func<BindCalendarModel, SubscribeCalendarViewModel>) (cal =>
            {
              return new SubscribeCalendarViewModel(cal.Id, cal.Name, account.Id)
              {
                Selected = allSelect || this.OriginalData.SubscribeCalendars.Contains(cal.Id) || this.OriginalData.BindAccounts.Contains(account.Id),
                ShowIndent = true,
                IsSubItem = true,
                InCalFilter = this.IsCalFilter
              };
            }));
            accountViewModel2.Children = new List<SelectableItemViewModel>();
            foreach (SubscribeCalendarViewModel calendarViewModel in calendarViewModels)
            {
              accountViewModel2.Children.Add((SelectableItemViewModel) calendarViewModel);
              models.Add((SelectableItemViewModel) calendarViewModel);
            }
          }
        }
      }
      List<CalendarSubscribeProfileModel> list3 = CacheManager.GetSubscribeCalendars().Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Show != "hidden")).ToList<CalendarSubscribeProfileModel>();
      if (!list3.Any<CalendarSubscribeProfileModel>())
        return;
      if (list1.Count == 0)
        models.Add((SelectableItemViewModel) calendarGroupViewModel2);
      list3.Sort((Comparison<CalendarSubscribeProfileModel>) ((a, b) =>
      {
        DateTime? createdTime = a.CreatedTime;
        ref DateTime? local = ref createdTime;
        return !local.HasValue ? -1 : local.GetValueOrDefault().CompareTo((object) b.CreatedTime);
      }));
      foreach (SubscribeCalendarViewModel calendarViewModel in list3.Select<CalendarSubscribeProfileModel, SubscribeCalendarViewModel>((Func<CalendarSubscribeProfileModel, SubscribeCalendarViewModel>) (subscribe =>
      {
        return new SubscribeCalendarViewModel(subscribe.Id, subscribe.Name)
        {
          Selected = allSelect || this.OriginalData.SubscribeCalendars.Contains(subscribe.Id),
          ShowIndent = this._showIndent,
          InCalFilter = this.IsCalFilter
        };
      })))
      {
        calendarGroupViewModel2.Children.Add((SelectableItemViewModel) calendarViewModel);
        models.Add((SelectableItemViewModel) calendarViewModel);
      }
    }

    private void LoadFilters(List<SelectableItemViewModel> models, bool batchMode)
    {
      List<FilterModel> list = CacheManager.GetFilters().Where<FilterModel>((Func<FilterModel, bool>) (filter => filter.deleted != 1)).OrderBy<FilterModel, long>((Func<FilterModel, long>) (filter => filter.sortOrder)).ToList<FilterModel>();
      if (list.Count <= 0)
        return;
      FilterGroupViewModel filterGroupViewModel1 = new FilterGroupViewModel();
      filterGroupViewModel1.InCalFilter = this.IsCalFilter;
      FilterGroupViewModel filterGroupViewModel2 = filterGroupViewModel1;
      models.Add((SelectableItemViewModel) filterGroupViewModel2);
      foreach (FilterItemViewModel filterItemViewModel in list.Select<FilterModel, FilterItemViewModel>((Func<FilterModel, FilterItemViewModel>) (filter =>
      {
        return new FilterItemViewModel(filter.id, filter.name)
        {
          Selected = this.OriginalData.FilterIds.Contains(filter.id),
          ShowIndent = true,
          InCalFilter = this.IsCalFilter
        };
      })))
      {
        models.Add((SelectableItemViewModel) filterItemViewModel);
        filterGroupViewModel2.Children.Add((SelectableItemViewModel) filterItemViewModel);
      }
    }

    private void LoadTags(List<SelectableItemViewModel> models)
    {
      List<TagModel> list = CacheManager.GetTags().OrderBy<TagModel, long>((Func<TagModel, long>) (tag => tag.sortOrder)).ToList<TagModel>();
      if (list.Count > 0)
      {
        bool allTagSelect = this.OriginalData.Tags.Contains("*withtags");
        TagGroupViewModel tagGroupViewModel = new TagGroupViewModel();
        tagGroupViewModel.InCalFilter = this.IsCalFilter;
        tagGroupViewModel.Selected = allTagSelect;
        tagGroupViewModel.PartSelected = this.BatchMode && this.OriginalData.Tags.Any<string>();
        TagGroupViewModel tagGroup = tagGroupViewModel;
        models.Add((SelectableItemViewModel) tagGroup);
        List<TagModel> tagChildrens = list.Where<TagModel>((Func<TagModel, bool>) (t => !string.IsNullOrEmpty(t.parent))).ToList<TagModel>();
        foreach (TagViewModel tagViewModel in list.Where<TagModel>((Func<TagModel, bool>) (t => string.IsNullOrEmpty(t.parent))).ToList<TagModel>().Select<TagModel, TagViewModel>((Func<TagModel, TagViewModel>) (tag =>
        {
          TagViewModel tagViewModel = new TagViewModel(tag, true, this.IsCalFilter)
          {
            Selected = allTagSelect || this.OriginalData.Tags.Contains(tag.name)
          };
          tagViewModel.GroupParent = (SelectableItemViewModel) tagGroup;
          tagGroup.Children.Add((SelectableItemViewModel) tagViewModel);
          tagViewModel.Children = tagViewModel.IsParent ? tagChildrens.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == tag.name)).Select<TagModel, SelectableItemViewModel>((Func<TagModel, SelectableItemViewModel>) (t =>
          {
            return (SelectableItemViewModel) new TagViewModel(t, true, this.IsCalFilter)
            {
              Selected = (allTagSelect || this.OriginalData.Tags.Contains(t.name))
            };
          })).ToList<SelectableItemViewModel>() : new List<SelectableItemViewModel>();
          tagViewModel.PartSelected = tagViewModel.IsParent && !tagViewModel.Selected && tagViewModel.Children != null && tagViewModel.Children.Any<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (child => child.Selected));
          return tagViewModel;
        })).ToList<TagViewModel>())
        {
          models.Add((SelectableItemViewModel) tagViewModel);
          if (tagViewModel.IsParent && tagViewModel.Open)
            tagViewModel.Children?.ForEach(new Action<SelectableItemViewModel>(models.Add));
        }
        this._tagShowed = true;
      }
      else
        this._tagShowed = false;
    }

    private void LoadProjects(List<SelectableItemViewModel> models, string searchText)
    {
      List<SelectableItemViewModel> selectableItemViewModelList1 = ProjectDataAssembler.AssembleProjects(this.OnlyShowPermission, this.IsCalFilter);
      List<string> projectIds1 = this.OriginalData.ProjectIds;
      // ISSUE: explicit non-virtual call
      bool flag = projectIds1 != null && __nonvirtual (projectIds1.Contains("#alllists"));
      bool showIndent = selectableItemViewModelList1.Count > 3 && this.ShowListGroup;
      ListGroupViewModel listGroupViewModel = new ListGroupViewModel();
      listGroupViewModel.Open = !this._foldIds.Contains("#alllists");
      listGroupViewModel.InCalFilter = this.IsCalFilter;
      listGroupViewModel.Selected = flag;
      int num1;
      if (this.BatchMode && !flag)
      {
        List<string> projectIds2 = this.OriginalData.ProjectIds;
        if ((projectIds2 != null ? (projectIds2.Any<string>() ? 1 : 0) : 0) == 0)
        {
          List<string> groupIds = this.OriginalData.GroupIds;
          num1 = groupIds != null ? (groupIds.Any<string>() ? 1 : 0) : 0;
        }
        else
          num1 = 1;
      }
      else
        num1 = 0;
      listGroupViewModel.PartSelected = num1 != 0;
      ListGroupViewModel listGroup = listGroupViewModel;
      if (showIndent)
        models.Add((SelectableItemViewModel) listGroup);
      else if (this.ShowSmartProjects)
      {
        List<SelectableItemViewModel> selectableItemViewModelList2 = models;
        SelectableItemViewModel selectableItemViewModel = new SelectableItemViewModel();
        selectableItemViewModel.IsSplit = true;
        selectableItemViewModel.IsEnable = false;
        selectableItemViewModelList2.Add(selectableItemViewModel);
      }
      InboxProjectViewModel projectViewModel1 = new InboxProjectViewModel();
      int num2;
      if (!flag)
      {
        List<string> projectIds3 = this.OriginalData.ProjectIds;
        num2 = projectIds3 != null ? (projectIds3.Any<string>((Func<string, bool>) (id => id != null && id.Contains("inbox"))) ? 1 : 0) : 0;
      }
      else
        num2 = 1;
      projectViewModel1.Selected = num2 != 0;
      projectViewModel1.ShowIndent = showIndent;
      projectViewModel1.InCalFilter = this.IsCalFilter;
      InboxProjectViewModel inbox = projectViewModel1;
      inbox.GroupParent = (SelectableItemViewModel) listGroup;
      listGroup.Children.Add((SelectableItemViewModel) inbox);
      if (!string.IsNullOrWhiteSpace(searchText))
      {
        searchText = searchText.ToLower();
        selectableItemViewModelList1.Insert(0, (SelectableItemViewModel) inbox);
        List<string> list = ((IEnumerable<string>) searchText.Split(' ')).ToList<string>();
        list.RemoveAll((Predicate<string>) (key => key == ""));
        foreach (SelectableItemViewModel selectableItemViewModel in selectableItemViewModelList1)
        {
          if ((!selectableItemViewModel.IsNote || this.ShowNoteProject) && (!selectableItemViewModel.IsShare || this.ShowSharedProject) && selectableItemViewModel is ProjectViewModel projectViewModel2)
          {
            string lower1 = projectViewModel2.Title?.ToLower();
            if (PinyinUtils.ToPinyin(lower1).Contains(searchText) || SearchHelper.KeyWordMatch(projectViewModel2.Emoji + lower1, searchText.Trim(), list) != 0)
            {
              models.Add(selectableItemViewModel);
              selectableItemViewModel.IsSubItem = false;
            }
            if (this._projectColumnDict.ContainsKey(projectViewModel2.Id))
            {
              foreach (ColumnModel column in this._projectColumnDict[projectViewModel2.Id])
              {
                string lower2 = column.name?.ToLower();
                if (PinyinUtils.ToPinyin(lower2).Contains(searchText) || SearchHelper.KeyWordMatch(lower2, searchText.Trim(), list) != 0)
                {
                  List<SelectableItemViewModel> selectableItemViewModelList3 = models;
                  ProjectColumnViewModel projectColumnViewModel = new ProjectColumnViewModel(column);
                  projectColumnViewModel.Desc = projectViewModel2.Emoji + projectViewModel2.Title;
                  projectColumnViewModel.IsSubItem = false;
                  selectableItemViewModelList3.Add((SelectableItemViewModel) projectColumnViewModel);
                }
              }
            }
          }
        }
        this.ClosePopup();
      }
      else
      {
        if (this.ShowInbox)
          models.Add((SelectableItemViewModel) inbox);
        if ((inbox.Children == null || inbox.Children.Count == 0) && inbox.Id != null && this._projectColumnDict.ContainsKey(inbox.Id))
        {
          List<ColumnModel> source = this._projectColumnDict[inbox.Id];
          string id = source.FirstOrDefault<ColumnModel>()?.id;
          string selectedColumnId = string.IsNullOrEmpty(this.SelectedColumn) ? id : this.SelectedColumn;
          inbox.Children = source.Select<ColumnModel, SelectableItemViewModel>((Func<ColumnModel, SelectableItemViewModel>) (c =>
          {
            return (SelectableItemViewModel) new ProjectColumnViewModel(c)
            {
              Selected = (inbox.Selected && c.id == selectedColumnId)
            };
          })).ToList<SelectableItemViewModel>();
          List<SelectableItemViewModel> children = inbox.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 1 ? 1 : 0) : 0) != 0)
          {
            inbox.ShowSubOnSide = true;
            inbox.Selectable = false;
          }
        }
        foreach (SelectableItemViewModel selectableItemViewModel1 in selectableItemViewModelList1)
        {
          SelectableItemViewModel project = selectableItemViewModel1;
          project.ShowIndent = showIndent;
          project.GroupParent = (SelectableItemViewModel) listGroup;
          if (!(project is TeamSectionViewModel))
          {
            ProjectGroupViewModel projectGroupViewModel = project as ProjectGroupViewModel;
            if (projectGroupViewModel == null)
            {
              ProjectViewModel projectViewModel = project as ProjectViewModel;
              if (projectViewModel != null && (this.ShowNoteProject || !project.IsNote) && (this.ShowSharedProject || !project.IsShare))
              {
                SelectableItemViewModel selectableItemViewModel2 = project;
                int num3;
                if (!flag)
                {
                  List<string> projectIds4 = this.OriginalData.ProjectIds;
                  // ISSUE: explicit non-virtual call
                  num3 = projectIds4 != null ? (__nonvirtual (projectIds4.Contains(projectViewModel.Id)) ? 1 : 0) : 0;
                }
                else
                  num3 = 1;
                selectableItemViewModel2.Selected = num3 != 0;
                if ((projectViewModel.Children == null || projectViewModel.Children.Count == 0) && projectViewModel.Id != null && this._projectColumnDict.ContainsKey(projectViewModel.Id))
                {
                  List<ColumnModel> source = this._projectColumnDict[closure_3.Id];
                  string id = source.FirstOrDefault<ColumnModel>()?.id;
                  string selectedColumnId = string.IsNullOrEmpty(this.SelectedColumn) ? id : this.SelectedColumn;
                  closure_3.Children = source.Select<ColumnModel, SelectableItemViewModel>((Func<ColumnModel, SelectableItemViewModel>) (c =>
                  {
                    return (SelectableItemViewModel) new ProjectColumnViewModel(c)
                    {
                      Selected = (closure_3.Selected && c.id == selectedColumnId)
                    };
                  })).ToList<SelectableItemViewModel>();
                  List<SelectableItemViewModel> children = closure_3.Children;
                  // ISSUE: explicit non-virtual call
                  if ((children != null ? (__nonvirtual (children.Count) > 1 ? 1 : 0) : 0) != 0)
                  {
                    closure_3.ShowSubOnSide = true;
                    closure_3.Selectable = false;
                  }
                }
                if (!(projectViewModel is SubProjectViewModel))
                {
                  models.Add(project);
                  listGroup.Children.Add(project);
                }
              }
            }
            else
            {
              ProjectGroupViewModel projectGroupViewModel1 = projectGroupViewModel;
              int num4;
              if (!flag)
              {
                List<string> groupIds = this.OriginalData.GroupIds;
                // ISSUE: explicit non-virtual call
                num4 = groupIds != null ? (__nonvirtual (groupIds.Contains(projectGroupViewModel.Id)) ? 1 : 0) : 0;
              }
              else
                num4 = 1;
              projectGroupViewModel1.Selected = num4 != 0;
              projectGroupViewModel.Children.ForEach((Action<SelectableItemViewModel>) (p =>
              {
                SelectableItemViewModel selectableItemViewModel3 = p;
                int num5;
                if (!projectGroupViewModel.Selected)
                {
                  List<string> projectIds5 = this.OriginalData.ProjectIds;
                  // ISSUE: explicit non-virtual call
                  num5 = projectIds5 != null ? (__nonvirtual (projectIds5.Contains(p.Id)) ? 1 : 0) : 0;
                }
                else
                  num5 = 1;
                selectableItemViewModel3.Selected = num5 != 0;
                p.BatchMode = this.BatchMode;
                p.IsSubItem = true;
                p.ShowIndent = showIndent;
              }));
              projectGroupViewModel.Children.RemoveAll((Predicate<SelectableItemViewModel>) (p => p.IsNote && !this.ShowNoteProject));
              projectGroupViewModel.Children.RemoveAll((Predicate<SelectableItemViewModel>) (p => p.IsShare && !this.ShowSharedProject));
              ProjectGroupViewModel projectGroupViewModel2 = projectGroupViewModel;
              int num6;
              if (!projectGroupViewModel.Selected)
              {
                List<SelectableItemViewModel> children = projectGroupViewModel.Children;
                bool? nullable1;
                bool? nullable2;
                if (children == null)
                {
                  nullable1 = new bool?();
                  nullable2 = nullable1;
                }
                else
                  nullable2 = new bool?(children.Any<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (child => child.Selected)));
                nullable1 = nullable2;
                num6 = nullable1.Value ? 1 : 0;
              }
              else
                num6 = 0;
              projectGroupViewModel2.PartSelected = num6 != 0;
              projectGroupViewModel.ShowSubOnSide = !this.CanSelectGroup;
              projectGroupViewModel.Selectable = this.CanSelectGroup;
              projectGroupViewModel.IsParent = this.CanSelectGroup;
              models.Add((SelectableItemViewModel) projectGroupViewModel);
              listGroup.Children.Add(project);
              if (this.CanSelectGroup)
                projectGroupViewModel.Children?.ForEach((Action<SelectableItemViewModel>) (child =>
                {
                  if (!projectGroupViewModel.Open)
                    return;
                  models.Add(child);
                  listGroup.Children.Add(project);
                }));
            }
          }
          else
          {
            listGroup.Children.Add(project);
            models.Add(project);
          }
        }
      }
    }

    public void OnTagGroupChanged(TagGroupViewModel tagGroup, bool setData = true)
    {
      if (this._data != null && tagGroup != null)
      {
        if (tagGroup.Open)
        {
          this._foldIds.Remove(tagGroup.Id);
          int index = this._data.IndexOf((SelectableItemViewModel) tagGroup);
          foreach (SelectableItemViewModel child1 in tagGroup.Children)
          {
            this._data.Insert(++index, child1);
            if (child1.IsParent && child1.Open)
              child1.Children?.ForEach((Action<SelectableItemViewModel>) (child => this._data.Insert(++index, child)));
          }
        }
        else
        {
          this._foldIds.Add(tagGroup.Id);
          this._data.RemoveAll((Predicate<SelectableItemViewModel>) (m => m is TagViewModel));
        }
      }
      if (!setData)
        return;
      ItemsSourceHelper.SetItemsSource<SelectableItemViewModel>((ItemsControl) this.SelectableItems, this._data);
    }

    public void OnListGroupChanged(ListGroupViewModel listGroup, bool setData = true)
    {
      if (this._data != null && listGroup != null)
      {
        List<SelectableItemViewModel> listAndGroupData = new List<SelectableItemViewModel>();
        HashSet<string> dataIds = new HashSet<string>();
        foreach (SelectableItemViewModel child1 in listGroup.Children)
        {
          if (!dataIds.Contains(child1.Id))
          {
            listAndGroupData.Add(child1);
            dataIds.Add(child1.Id);
            if (child1 is TeamSectionViewModel sectionViewModel)
            {
              if (child1.Open)
              {
                foreach (SelectableItemViewModel child2 in sectionViewModel.Children)
                {
                  listAndGroupData.Add(child2);
                  dataIds.Add(child2.Id);
                  if (child2.Open)
                    child2.Children.ForEach((Action<SelectableItemViewModel>) (model =>
                    {
                      listAndGroupData.Add(model);
                      dataIds.Add(model.Id);
                    }));
                }
              }
            }
            else if (child1.Open)
              child1.Children.ForEach((Action<SelectableItemViewModel>) (model =>
              {
                listAndGroupData.Add(model);
                dataIds.Add(model.Id);
              }));
          }
        }
        if (listGroup.Open)
        {
          this._foldIds.Remove(listGroup.Id);
          int num = this._data.IndexOf((SelectableItemViewModel) listGroup);
          foreach (SelectableItemViewModel selectableItemViewModel in listAndGroupData)
            this._data.Insert(++num, selectableItemViewModel);
        }
        else
        {
          this._foldIds.Add(listGroup.Id);
          this._data.RemoveAll((Predicate<SelectableItemViewModel>) (item => dataIds.Contains(item.Id)));
        }
      }
      if (!setData)
        return;
      ItemsSourceHelper.SetItemsSource<SelectableItemViewModel>((ItemsControl) this.SelectableItems, this._data);
    }

    public void OnItemSelectedChanged(SelectableItemViewModel model)
    {
      if (!(model is ProjectGroupViewModel))
      {
        if (!(model is ProjectViewModel))
        {
          if (!(model is AllProjectViewModel model4))
          {
            if (!(model is AllProjectCategoryViewModel model3))
            {
              if (!(model is FilterItemViewModel model2))
              {
                if (!(model is BindAccountViewModel account))
                {
                  if (!(model is SubscribeCalendarViewModel sub))
                  {
                    if (model is TagViewModel model1)
                    {
                      if (this.BatchMode)
                        this.HandleOnTagSelected((SelectableItemViewModel) model1);
                      this.PartSelectAllTag();
                    }
                  }
                  else
                  {
                    this.HandleOnSubscribeSelected(sub);
                    this.PartSelectAllCal();
                  }
                }
                else
                {
                  this.HandleOnAccountSelected((SelectableItemViewModel) account);
                  this.PartSelectAllCal();
                }
              }
              else if (this.BatchMode)
                this.HandleOnAllOrFilterSelected((SelectableItemViewModel) model2);
            }
            else
              this.HandleOnAllProjectOrFilterSelected((SelectableItemViewModel) model3);
          }
          else
            this.HandleOnAllOrFilterSelected((SelectableItemViewModel) model4);
        }
        else
        {
          if (this.ShowColumns)
          {
            List<SelectableItemViewModel> children = model.Children;
            // ISSUE: explicit non-virtual call
            if ((children != null ? (__nonvirtual (children.Count) > 1 ? 1 : 0) : 0) != 0)
            {
              model.Selected = false;
              return;
            }
          }
          this.PartSelectAllList();
          if (model is SubProjectViewModel)
            this.HandleOnSubItemCheckChanged(model);
        }
      }
      else if (this.CanSelectGroup)
      {
        model.Children?.ForEach((Action<SelectableItemViewModel>) (child => child.Selected = model.Selected));
        this.PartSelectAllList();
      }
      else
      {
        model.Selected = false;
        return;
      }
      this.SelectAllOnItemUnSelected(model);
      this.UnSelectAllOnItemSelected(model);
      EventHandler<SelectableItemViewModel> itemSelect = this.ItemSelect;
      if (itemSelect == null)
        return;
      itemSelect((object) this, model);
    }

    private void PartSelectAllList()
    {
      SelectableItemViewModel selectableItemViewModel = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (i => i is ListGroupViewModel));
      if (selectableItemViewModel == null)
        return;
      if (selectableItemViewModel.Selected)
      {
        this.OriginalData.ProjectIds.Clear();
        selectableItemViewModel.Selected = false;
      }
      selectableItemViewModel.PartSelected = this._data.Any<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (m =>
      {
        switch (m)
        {
          case ProjectGroupViewModel projectGroupViewModel2 when projectGroupViewModel2.Highlighted:
            return true;
          case ProjectViewModel projectViewModel2:
            return projectViewModel2.Selected;
          default:
            return false;
        }
      }));
    }

    private void PartSelectAllTag()
    {
      SelectableItemViewModel selectableItemViewModel = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (i => i is TagGroupViewModel));
      if (selectableItemViewModel == null)
        return;
      if (selectableItemViewModel.Selected)
      {
        this.OriginalData.Tags.Clear();
        selectableItemViewModel.Selected = false;
      }
      selectableItemViewModel.PartSelected = this._data.Any<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (m => m is TagViewModel tagViewModel && tagViewModel.Selected));
    }

    private void PartSelectAllCal()
    {
      SelectableItemViewModel selectableItemViewModel = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (i => i is CalendarGroupViewModel));
      if (selectableItemViewModel == null)
        return;
      if (selectableItemViewModel.Selected)
      {
        this.OriginalData.SubscribeCalendars.Clear();
        selectableItemViewModel.Selected = false;
      }
      selectableItemViewModel.PartSelected = this._data.Any<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (m =>
      {
        switch (m)
        {
          case SubscribeCalendarViewModel calendarViewModel2 when calendarViewModel2.Selected:
            return true;
          case BindAccountViewModel accountViewModel2:
            return accountViewModel2.Highlighted;
          default:
            return false;
        }
      }));
    }

    private void HandleOnTagSelected(SelectableItemViewModel model)
    {
      if (!string.IsNullOrEmpty(model.ParentId))
      {
        this.HandleOnSubItemCheckChanged(model);
      }
      else
      {
        if (!model.IsParent)
          return;
        model.Children?.ForEach((Action<SelectableItemViewModel>) (child => child.Selected = model.Selected));
      }
    }

    public void OnGroupItemMouseMove(object sender, MouseEventArgs args)
    {
      if (!(sender is SelectableItem child) || !(child.DataContext is SelectableItemViewModel dataContext))
        return;
      if (dataContext is ProjectColumnViewModel)
      {
        if (dataContext.HoverSelected)
          return;
        Utils.FindParent<UpDownSelectListView>((DependencyObject) child)?.ClearHover();
        dataContext.HoverSelected = true;
      }
      else if (dataContext is SubProjectViewModel)
      {
        if (this.SubSubPopup.IsOpen)
        {
          if (this._hoverSubParent == dataContext)
          {
            this._subPopupTracker.Mark();
            return;
          }
          if (this._subPopupTracker.IsInSafeArea())
            return;
        }
        if (this.SubSubPopup.IsOpen && this._hoverSubParent != null)
        {
          this.SubSubPopup.IsOpen = false;
          this._hoverSubParent.HoverSelected = false;
          this._hoverSubParent.SubOpened = false;
          this._hoverSubParent = (SelectableItemViewModel) null;
        }
        List<SelectableItemViewModel> children = dataContext.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 1 ? 1 : 0) : 0) != 0)
        {
          this._hoverSubParent = dataContext;
          Utils.FindParent<UpDownSelectListView>((DependencyObject) child)?.ClearHover();
          this._hoverSubParent.HoverSelected = true;
          this.SubSubPopup.PlacementTarget = (UIElement) child.SubPopupPlacement;
          this.SubSubPopup.IsOpen = true;
          dataContext.SubOpened = true;
          this.SetSubSubItems(dataContext.Children);
          this._subPopupTracker.Bind((Popup) this.SubSubPopup, true);
        }
        else
        {
          if (dataContext.HoverSelected)
            return;
          Utils.FindParent<UpDownSelectListView>((DependencyObject) child)?.ClearHover();
          dataContext.HoverSelected = true;
        }
      }
      else
      {
        if (this.SubPopup.IsOpen)
        {
          if (this._hoverParent == dataContext)
          {
            this._popupTracker.Mark();
            return;
          }
          if (this._popupTracker.IsInSafeArea())
            return;
        }
        if (this.SubSubPopup.IsOpen && this._hoverSubParent != null)
        {
          this.SubSubPopup.IsOpen = false;
          this._hoverSubParent.HoverSelected = false;
          this._hoverSubParent.SubOpened = false;
          this._hoverSubParent = (SelectableItemViewModel) null;
        }
        if (this.SubPopup.IsOpen && this._hoverParent != null)
        {
          this.SubPopup.IsOpen = false;
          this._hoverParent.HoverSelected = false;
          this._hoverParent.SubOpened = false;
          this._hoverParent = (SelectableItemViewModel) null;
        }
        switch (dataContext)
        {
          case ProjectViewModel projectViewModel:
            List<SelectableItemViewModel> children1 = projectViewModel.Children;
            // ISSUE: explicit non-virtual call
            if ((children1 != null ? (__nonvirtual (children1.Count) > 1 ? 1 : 0) : 0) != 0)
            {
              this._hoverParent = (SelectableItemViewModel) projectViewModel;
              Utils.FindParent<UpDownSelectListView>((DependencyObject) child)?.ClearHover();
              this._hoverParent.HoverSelected = true;
              this.SubPopup.PlacementTarget = (UIElement) child.SubPopupPlacement;
              this.ShowSubPopup();
              projectViewModel.SubOpened = true;
              this.SetSubItems(projectViewModel.Children);
              break;
            }
            if (dataContext.HoverSelected)
              break;
            Utils.FindParent<UpDownSelectListView>((DependencyObject) child)?.ClearHover();
            dataContext.HoverSelected = true;
            break;
          case ProjectGroupViewModel projectGroupViewModel:
            if (!projectGroupViewModel.ShowSubOnSide)
              break;
            List<SelectableItemViewModel> children2 = projectGroupViewModel.Children;
            // ISSUE: explicit non-virtual call
            if ((children2 != null ? (__nonvirtual (children2.Count) > 0 ? 1 : 0) : 0) == 0)
              break;
            this._hoverParent = (SelectableItemViewModel) projectGroupViewModel;
            Utils.FindParent<UpDownSelectListView>((DependencyObject) child)?.ClearHover();
            this._hoverParent.HoverSelected = true;
            this.SubPopup.PlacementTarget = (UIElement) child.SubPopupPlacement;
            this.ShowSubPopup();
            projectGroupViewModel.SubOpened = true;
            this.SetSubItems(projectGroupViewModel.Children);
            break;
        }
      }
    }

    private void ShowSubPopup()
    {
      this.SubPopup.IsOpen = true;
      this._popupTracker.Bind((Popup) this.SubPopup, true);
    }

    private void SetSubItems(List<SelectableItemViewModel> groupChildren)
    {
      groupChildren.ForEach((Action<SelectableItemViewModel>) (m =>
      {
        m.ShowIndent = false;
        m.IsSubItem = false;
      }));
      ObservableCollection<SelectableItemViewModel> source = new ObservableCollection<SelectableItemViewModel>(groupChildren);
      this.SubItems.ItemsSource = (IEnumerable) source;
      SelectableItemViewModel selectableItemViewModel = source.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (m => m.Selected));
      if (selectableItemViewModel == null)
        return;
      this.SubItems.ScrollIntoView((object) selectableItemViewModel);
    }

    private void SetSubSubItems(List<SelectableItemViewModel> groupChildren)
    {
      groupChildren.ForEach((Action<SelectableItemViewModel>) (m =>
      {
        m.ShowIndent = false;
        m.IsSubItem = false;
      }));
      ObservableCollection<SelectableItemViewModel> source = new ObservableCollection<SelectableItemViewModel>(groupChildren);
      this.SubSubItems.ItemsSource = (IEnumerable) source;
      SelectableItemViewModel selectableItemViewModel = source.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (m => m.Selected));
      if (selectableItemViewModel == null)
        return;
      this.SubSubItems.ScrollIntoView((object) selectableItemViewModel);
    }

    private async void OnSubItemClick(bool enter, UpDownSelectViewModel e)
    {
      if (e.Selectable)
        this.SubPopup.IsOpen = false;
      this.OnItemClick(enter, e);
    }

    private async void OnSubSubItemClick(bool enter, UpDownSelectViewModel e)
    {
      if (e.Selectable)
      {
        this.SubSubPopup.IsOpen = false;
        this.SubPopup.IsOpen = false;
      }
      this.OnItemClick(enter, e);
    }

    private void OnItemClick(bool enter, UpDownSelectViewModel e)
    {
      if (!(e is SelectableItemViewModel model))
        return;
      if (model.IsSectionGroup)
      {
        model.Open = !model.Open;
        switch (model)
        {
          case TagGroupViewModel tagGroup:
            this.OnTagGroupChanged(tagGroup);
            break;
          case ListGroupViewModel listGroup:
            this.OnListGroupChanged(listGroup);
            break;
          case FilterGroupViewModel filterGroup:
            this.OnFilterGroupChanged(filterGroup);
            break;
          case CalendarGroupViewModel calGroup:
            this.OnCalendarGroupChanged(calGroup);
            break;
          case TeamSectionViewModel section:
            this.OnSectionGroupChanged(section);
            break;
        }
      }
      else if (model.Selectable)
        this.OnItemSelectedChanged(model);
      if (!model.InCalFilter || !model.Selected)
        return;
      switch (model)
      {
        case AllProjectViewModel _:
          UserActCollectUtils.AddClickEvent("calendar", "filter_select", "all");
          break;
        case ProjectViewModel _:
        case ProjectGroupViewModel _:
          UserActCollectUtils.AddClickEvent("calendar", "filter_select", "list_or_folder");
          break;
        case FilterItemViewModel _:
          UserActCollectUtils.AddClickEvent("calendar", "filter_select", "filter");
          break;
        case TagViewModel _:
          UserActCollectUtils.AddClickEvent("calendar", "filter_select", "tag");
          break;
        case SubscribeCalendarViewModel _:
          UserActCollectUtils.AddClickEvent("calendar", "filter_select", "calendar");
          break;
      }
    }

    private void HandleOnSubscribeSelected(SubscribeCalendarViewModel sub)
    {
      SelectableItemViewModel selectableItemViewModel1 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (model => model.Id == sub.AccountId));
      if (selectableItemViewModel1 == null)
        return;
      int num1 = selectableItemViewModel1.Children.Count<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item.Selected));
      SelectableItemViewModel selectableItemViewModel2 = selectableItemViewModel1;
      int? count;
      int num2;
      if (num1 > 0)
      {
        int num3 = num1;
        count = selectableItemViewModel1.Children?.Count;
        int valueOrDefault = count.GetValueOrDefault();
        num2 = !(num3 == valueOrDefault & count.HasValue) ? 1 : 0;
      }
      else
        num2 = 0;
      selectableItemViewModel2.PartSelected = num2 != 0;
      SelectableItemViewModel selectableItemViewModel3 = selectableItemViewModel1;
      int num4 = num1;
      count = selectableItemViewModel1.Children?.Count;
      int num5 = num4 == count.GetValueOrDefault() & count.HasValue ? 1 : 0;
      selectableItemViewModel3.Selected = num5 != 0;
    }

    private void HandleOnAccountSelected(SelectableItemViewModel account)
    {
      BindAccountViewModel model = account as BindAccountViewModel;
      if (model == null)
        return;
      model.PartSelected = false;
      model.Children.ForEach((Action<SelectableItemViewModel>) (child => child.Selected = model.Selected));
    }

    private void SelectAllOnItemUnSelected(SelectableItemViewModel model)
    {
      if (model.Selected || this._data.Any<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (m =>
      {
        switch (m)
        {
          case ProjectGroupViewModel projectGroupViewModel2 when projectGroupViewModel2.Highlighted:
            return true;
          case ProjectViewModel projectViewModel2:
            return projectViewModel2.Selected;
          default:
            return false;
        }
      })))
        return;
      SelectableItemViewModel selectableItemViewModel1 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is FilterHabitViewModel));
      SelectableItemViewModel selectableItemViewModel2 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item.Id == "Calendar5959a2259161d16d23a4f272"));
      SelectableItemViewModel selectableItemViewModel3 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item.Id == "ProjectAll2e4c103c57ef480997943206"));
      if (selectableItemViewModel1 != null && selectableItemViewModel1.Selected || selectableItemViewModel2 != null && selectableItemViewModel2.Selected || selectableItemViewModel3 != null && selectableItemViewModel3.Selected)
        return;
      SelectableItemViewModel selectableItemViewModel4 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is AllProjectViewModel));
      if (selectableItemViewModel4 != null)
      {
        selectableItemViewModel4.Selected = true;
      }
      else
      {
        if (selectableItemViewModel3 == null)
          return;
        selectableItemViewModel3.Selected = true;
      }
    }

    private void UnSelectAllOnItemSelected(SelectableItemViewModel model)
    {
      if (!(model is AllProjectViewModel) && model.Selected)
      {
        SelectableItemViewModel selectableItemViewModel = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is AllProjectViewModel));
        if (selectableItemViewModel != null)
          selectableItemViewModel.Selected = false;
      }
      if (!(model is AllProjectCategoryViewModel) && model.Selected && model.Id != "Habit2e4c103c57ef480997943206" && model.Id != "Calendar5959a2259161d16d23a4f272")
      {
        SelectableItemViewModel selectableItemViewModel = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is AllProjectCategoryViewModel));
        if (selectableItemViewModel != null)
          selectableItemViewModel.Selected = false;
      }
      if (!model.Selected || model is FilterItemViewModel)
        return;
      this._data.ToList<SelectableItemViewModel>().ForEach((Action<SelectableItemViewModel>) (item =>
      {
        if (!(item is FilterItemViewModel))
          return;
        item.Selected = false;
      }));
    }

    private void HandleOnAllProjectOrFilterSelected(SelectableItemViewModel model)
    {
      if (model.Selected)
      {
        this._data.ToList<SelectableItemViewModel>().ForEach(new Action<SelectableItemViewModel>(CancelSelectChildren));
      }
      else
      {
        SelectableItemViewModel selectableItemViewModel1 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is FilterHabitViewModel));
        SelectableItemViewModel selectableItemViewModel2 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item.Id == "Calendar5959a2259161d16d23a4f272"));
        if (selectableItemViewModel1 == null || selectableItemViewModel1.Selected || selectableItemViewModel2 == null || selectableItemViewModel2.Selected)
          return;
        SelectableItemViewModel selectableItemViewModel3 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is AllProjectViewModel));
        if (selectableItemViewModel3 == null)
          return;
        selectableItemViewModel3.Selected = true;
      }

      void CancelSelectChildren(SelectableItemViewModel item)
      {
        if (!(item.Id != model.Id) || !(item.Id != "Habit2e4c103c57ef480997943206") || !(item.Id != "Calendar5959a2259161d16d23a4f272"))
          return;
        item.Selected = false;
        item.PartSelected = false;
        List<SelectableItemViewModel> children = item.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) == 0)
          return;
        foreach (SelectableItemViewModel child in item.Children)
          CancelSelectChildren(child);
      }
    }

    private void HandleOnAllOrFilterSelected(SelectableItemViewModel model)
    {
      if (model.Selected)
      {
        this._data.ToList<SelectableItemViewModel>().ForEach(new Action<SelectableItemViewModel>(CancelSelectChildren));
      }
      else
      {
        SelectableItemViewModel selectableItemViewModel = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is AllProjectViewModel));
        if (selectableItemViewModel == null)
          return;
        selectableItemViewModel.Selected = true;
      }

      void CancelSelectChildren(SelectableItemViewModel item)
      {
        if (!(item.Id != model.Id))
          return;
        item.Selected = false;
        item.PartSelected = false;
        List<SelectableItemViewModel> children = item.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) == 0)
          return;
        foreach (SelectableItemViewModel child in item.Children)
          CancelSelectChildren(child);
      }
    }

    private void HandleOnSubItemCheckChanged(SelectableItemViewModel model)
    {
      SelectableItemViewModel selectableItemViewModel1 = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item.Id == model.ParentId));
      if (selectableItemViewModel1 == null)
        return;
      if (!model.Selected)
      {
        selectableItemViewModel1.Selected = false;
        SelectableItemViewModel selectableItemViewModel2 = selectableItemViewModel1;
        List<SelectableItemViewModel> children = selectableItemViewModel1.Children;
        int num = children != null ? (children.Any<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (child => child.Selected)) ? 1 : 0) : 0;
        selectableItemViewModel2.PartSelected = num != 0;
      }
      else
      {
        if (selectableItemViewModel1.Selected)
          return;
        List<SelectableItemViewModel> children = selectableItemViewModel1.Children;
        if ((children != null ? (children.Any<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (child => !child.Selected)) ? 1 : 0) : 0) != 0)
          selectableItemViewModel1.PartSelected = true;
        else
          selectableItemViewModel1.Selected = true;
      }
    }

    private async void OnItemsLoaded(object sender, RoutedEventArgs e)
    {
      if (this.ShowColumns)
        await this.GetAllColumns();
      this.LoadData();
      if (this.BatchMode)
        return;
      List<SelectableItemViewModel> data = this._data;
      SelectableItemViewModel selectableItemViewModel = data != null ? data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (d => d.Selected || d.Highlighted)) : (SelectableItemViewModel) null;
      if (selectableItemViewModel == null)
        return;
      this.SelectableItems.ScrollIntoView((object) selectableItemViewModel);
    }

    private async Task GetAllColumns()
    {
      List<ColumnModel> allColumnsAsync = await ColumnDao.GetAllColumnsAsync();
      allColumnsAsync.Sort((Comparison<ColumnModel>) ((a, b) =>
      {
        long? sortOrder = a.sortOrder;
        ref long? local = ref sortOrder;
        return !local.HasValue ? 1 : local.GetValueOrDefault().CompareTo((object) b.sortOrder);
      }));
      this._projectColumnDict.Clear();
      foreach (ColumnModel columnModel in allColumnsAsync)
      {
        if (columnModel.deleted != 1 && !string.IsNullOrEmpty(columnModel.projectId))
        {
          if (this._projectColumnDict.ContainsKey(columnModel.projectId))
            this._projectColumnDict[columnModel.projectId].Add(columnModel);
          else
            this._projectColumnDict[columnModel.projectId] = new List<ColumnModel>()
            {
              columnModel
            };
        }
      }
    }

    public void OnSectionGroupChanged(TeamSectionViewModel section)
    {
      if (section.Open)
      {
        SelectableItemViewModel selectableItemViewModel = this._data.FirstOrDefault<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (model => model is TeamSectionViewModel sectionViewModel && sectionViewModel.Id == section.TeamId));
        if (selectableItemViewModel != null)
        {
          int index = this._data.IndexOf(selectableItemViewModel);
          List<SelectableItemViewModel> models = new List<SelectableItemViewModel>();
          ItemSelection.AddItems(selectableItemViewModel, models);
          models.ForEach((Action<SelectableItemViewModel>) (child =>
          {
            if (child.IsNote && !this.ShowNoteProject || child.IsShare && !this.ShowSharedProject)
              return;
            this._data.Insert(++index, child);
          }));
        }
      }
      else
        this._data.RemoveAll((Predicate<SelectableItemViewModel>) (item => !(item is TeamSectionViewModel) && item.TeamId == section.TeamId));
      ItemsSourceHelper.SetItemsSource<SelectableItemViewModel>((ItemsControl) this.SelectableItems, this._data);
    }

    private static void AddItems(SelectableItemViewModel item, List<SelectableItemViewModel> models)
    {
      if (item.Children == null || !item.Children.Any<SelectableItemViewModel>())
        return;
      foreach (SelectableItemViewModel child in item.Children)
      {
        models.Add(child);
        if (!(child is ProjectViewModel))
          ItemSelection.AddItems(child, models);
      }
    }

    public void SelectItem(SelectableItemViewModel model)
    {
      EventHandler<SelectableItemViewModel> itemSelect = this.ItemSelect;
      if (itemSelect == null)
        return;
      itemSelect((object) this, model);
    }

    public void OnFoldChildren(SelectableItem item, SelectableItemViewModel model)
    {
      if (!model.IsParent || this._data == null)
        return;
      model.Open = !model.Open;
      if (model.Open)
      {
        int index = this._data.IndexOf(model);
        if (index >= 0)
          model.Children?.ForEach((Action<SelectableItemViewModel>) (child => this._data.Insert(++index, child)));
      }
      else
        this._data.RemoveAll((Predicate<SelectableItemViewModel>) (m => m.ParentId == model.Id));
      ItemsSourceHelper.SetItemsSource<SelectableItemViewModel>((ItemsControl) this.SelectableItems, this._data);
    }

    public bool ChildMouseOver() => this.SubPopup.IsMouseOver || this.SubSubPopup.IsMouseOver;

    public void OnFilterGroupChanged(FilterGroupViewModel filterGroup, bool setData = true)
    {
      if (this._data != null && filterGroup != null)
      {
        if (filterGroup.Open)
        {
          this._foldIds.Remove(filterGroup.Id);
          int index = this._data.IndexOf((SelectableItemViewModel) filterGroup);
          foreach (SelectableItemViewModel child1 in filterGroup.Children)
          {
            this._data.Insert(++index, child1);
            if (child1.IsParent && child1.Open)
              child1.Children?.ForEach((Action<SelectableItemViewModel>) (child => this._data.Insert(++index, child)));
          }
        }
        else
        {
          this._foldIds.Add(filterGroup.Id);
          this._data.RemoveAll((Predicate<SelectableItemViewModel>) (m => m is FilterItemViewModel));
        }
      }
      if (!setData)
        return;
      ItemsSourceHelper.SetItemsSource<SelectableItemViewModel>((ItemsControl) this.SelectableItems, this._data);
    }

    public void OnCalendarGroupChanged(CalendarGroupViewModel calGroup, bool setData = true)
    {
      if (this._data != null && calGroup != null)
      {
        if (calGroup.Open)
        {
          this._foldIds.Remove(calGroup.Id);
          int index = this._data.IndexOf((SelectableItemViewModel) calGroup);
          foreach (SelectableItemViewModel child1 in calGroup.Children)
          {
            this._data.Insert(++index, child1);
            if (child1.IsParent && child1.Open)
              child1.Children?.ForEach((Action<SelectableItemViewModel>) (child => this._data.Insert(++index, child)));
          }
        }
        else
        {
          this._foldIds.Add(calGroup.Id);
          this._data.RemoveAll((Predicate<SelectableItemViewModel>) (m => m is SubscribeCalendarViewModel || m is BindAccountViewModel));
        }
      }
      if (!setData)
        return;
      ItemsSourceHelper.SetItemsSource<SelectableItemViewModel>((ItemsControl) this.SelectableItems, this._data);
    }

    public bool UpDownSelect(bool isUp)
    {
      if (this.SubSubPopup.IsOpen)
      {
        this.SubSubItems.UpDownSelect(isUp);
        return true;
      }
      if (!this.SubPopup.IsOpen)
        return this.SelectableItems.UpDownSelect(isUp);
      this.SubItems.UpDownSelect(isUp);
      return true;
    }

    public bool HandleTab(bool shift)
    {
      this.UpDownSelect(shift);
      return true;
    }

    public bool HandleEnter()
    {
      if (this.SubSubPopup.IsOpen)
      {
        this.SubSubPopup.HandleEnter();
        return true;
      }
      if (!this.SubPopup.IsOpen)
        return this.SelectableItems.HandleEnter();
      this.SubPopup.HandleEnter();
      return true;
    }

    public bool HandleEsc() => false;

    public bool LeftRightSelect(bool isLeft)
    {
      if (this.SubSubPopup.IsOpen & isLeft)
      {
        this.SubSubItems.ClearHover();
        this.SubSubPopup.IsOpen = false;
        if (this._hoverSubParent != null)
        {
          this._hoverSubParent.SubOpened = false;
          this._hoverSubParent = (SelectableItemViewModel) null;
        }
        return true;
      }
      if (isLeft && this.SubPopup.IsOpen)
      {
        if (this._hoverSubParent != null)
        {
          this._hoverSubParent.SubOpened = false;
          this._hoverSubParent = (SelectableItemViewModel) null;
        }
        this.SubItems.ClearHover();
        this.SubPopup.IsOpen = false;
        if (this._hoverParent != null)
        {
          this._hoverParent.SubOpened = false;
          this._hoverParent = (SelectableItemViewModel) null;
        }
        return true;
      }
      if (!isLeft && this.SubPopup.IsOpen)
      {
        SelectableItem singleVisualChildren = Utils.FindSingleVisualChildren<SelectableItem>((DependencyObject) this.SubItems.GetHoverSelectedItem());
        if (singleVisualChildren != null && singleVisualChildren.DataContext is SelectableItemViewModel dataContext && dataContext.ShowSubOnSide)
        {
          List<SelectableItemViewModel> children = dataContext.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            this._hoverSubParent = dataContext;
            this._hoverSubParent.HoverSelected = true;
            this.SubSubPopup.PlacementTarget = (UIElement) singleVisualChildren.SubPopupPlacement;
            this.SubSubPopup.IsOpen = true;
            dataContext.SubOpened = true;
            dataContext.Children[0].HoverSelected = true;
            this.SetSubSubItems(dataContext.Children);
          }
        }
        return true;
      }
      if (isLeft || this.SubPopup.IsOpen)
        return false;
      SelectableItem singleVisualChildren1 = Utils.FindSingleVisualChildren<SelectableItem>((DependencyObject) this.SelectableItems.GetHoverSelectedItem());
      if (singleVisualChildren1 != null && singleVisualChildren1.DataContext is SelectableItemViewModel dataContext1 && dataContext1.ShowSubOnSide)
      {
        List<SelectableItemViewModel> children = dataContext1.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          this._hoverParent = dataContext1;
          this._hoverParent.HoverSelected = true;
          this.SubPopup.PlacementTarget = (UIElement) singleVisualChildren1.SubPopupPlacement;
          this.SubPopup.IsOpen = true;
          dataContext1.SubOpened = true;
          dataContext1.Children[0].HoverSelected = true;
          this.SetSubItems(dataContext1.Children);
        }
      }
      return true;
    }

    public void EnterSelect()
    {
      if (this.SubSubPopup.IsOpen)
        this.SubSubPopup.HandleEnter();
      else if (this.SubPopup.IsOpen)
        this.SubItems.HandleEnter();
      else
        this.SelectableItems.HandleEnter();
    }

    public void HoverSelectFirst()
    {
      List<SelectableItemViewModel> data = this._data;
      // ISSUE: explicit non-virtual call
      if ((data != null ? (__nonvirtual (data.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      this._data[0].HoverSelected = true;
    }

    private void OnLeftRightKeyDown(object sender, bool isLeft) => this.LeftRightSelect(isLeft);

    private void OnSubLeftRightKeyDown(object sender, bool isLeft)
    {
      if (!isLeft)
        return;
      this.SubPopup.IsOpen = false;
    }

    private void OnSubClosed(object sender, EventArgs e)
    {
      if (this._hoverParent == null)
        return;
      this._hoverParent.SubOpened = false;
      this._hoverParent = (SelectableItemViewModel) null;
    }

    private void OnSubSubClosed(object sender, EventArgs e)
    {
      if (this._hoverParent == null)
        return;
      this._hoverParent.SubOpened = false;
      this._hoverParent = (SelectableItemViewModel) null;
    }

    public void ClosePopup()
    {
      this.SubSubPopup.IsOpen = false;
      this.SubPopup.IsOpen = false;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/itemselection.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnItemsLoaded);
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.SelectableItems = (UpDownSelectListView) target;
          break;
        case 4:
          this.SubPopup = (EscPopup) target;
          break;
        case 5:
          this.SubItems = (UpDownSelectListView) target;
          break;
        case 6:
          this.SubSubPopup = (EscPopup) target;
          break;
        case 7:
          this.SubSubItems = (UpDownSelectListView) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
