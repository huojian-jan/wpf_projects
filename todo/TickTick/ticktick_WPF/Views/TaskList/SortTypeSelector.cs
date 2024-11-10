// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.SortTypeSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class SortTypeSelector
  {
    private readonly bool _resetDate;
    private readonly bool _resetProject;
    private readonly bool _resetTag;
    private ObservableCollection<CustomMenuItemViewModel> _menuItems;
    private EscPopup _popup;
    private List<SortTypeViewModel> _sortTypes;
    private CustomMenuItemViewModel _groupItem;
    private CustomMenuItemViewModel _orderItem;
    private SortOption _option;
    private SortOption _originOption;
    private ProjectIdentity _catId;
    private bool _inKanban;
    private bool _inTimeline;
    private bool _isGroup;

    public SortTypeSelector(
      ProjectIdentity identity,
      List<SortTypeViewModel> sortTypes,
      SortOption option,
      bool inKanban = false,
      EscPopup popup = null,
      List<CustomMenuItemViewModel> extraItem = null)
    {
      this._sortTypes = sortTypes;
      SortOption sortOption1 = option?.Copy();
      if (sortOption1 == null)
        sortOption1 = new SortOption()
        {
          groupBy = "dueDate",
          orderBy = "dueDate"
        };
      this._originOption = sortOption1;
      SortOption sortOption2 = option?.Copy();
      if (sortOption2 == null)
        sortOption2 = new SortOption()
        {
          groupBy = "dueDate",
          orderBy = "dueDate"
        };
      this._option = sortOption2;
      this._catId = identity;
      this._inKanban = inKanban;
      this._isGroup = identity is GroupProjectIdentity;
      SortTypeViewModel sortTypeViewModel1 = this._sortTypes.FirstOrDefault<SortTypeViewModel>((Func<SortTypeViewModel, bool>) (s => s.Id == this._option.groupBy));
      SortTypeViewModel sortTypeViewModel2 = this._sortTypes.FirstOrDefault<SortTypeViewModel>((Func<SortTypeViewModel, bool>) (s => s.Id == this._option.orderBy)) ?? this._sortTypes[0];
      if (sortTypeViewModel1 == null)
        sortTypeViewModel1 = inKanban ? this._sortTypes[0] : new SortTypeViewModel("none");
      ObservableCollection<CustomMenuItemViewModel> observableCollection = new ObservableCollection<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "group", Utils.GetString("GroupBy"), SortTypeViewModel.GetGroupIconById(sortTypeViewModel1?.Id ?? "none"));
      menuItemViewModel1.RightText = sortTypeViewModel1?.Title ?? Utils.GetString("none");
      menuItemViewModel1.ExtraIcon = Utils.GetIcon("ArrowLine");
      menuItemViewModel1.ExtraIconAngle = -90;
      menuItemViewModel1.ExtraIconSize = 14;
      menuItemViewModel1.ExtraIconColor = ThemeUtil.GetColor("BaseColorOpacity40", (FrameworkElement) popup);
      menuItemViewModel1.ShowSelected = false;
      this._groupItem = menuItemViewModel1;
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "sort", Utils.GetString("SortBy"), SortTypeViewModel.GetSortIconById(sortTypeViewModel2.Id, false));
      menuItemViewModel2.RightText = sortTypeViewModel2.Title;
      menuItemViewModel2.ExtraIcon = Utils.GetIcon("ArrowLine");
      menuItemViewModel2.ExtraIconAngle = -90;
      menuItemViewModel2.ExtraIconSize = 14;
      menuItemViewModel2.ExtraIconColor = ThemeUtil.GetColor("BaseColorOpacity40", (FrameworkElement) popup);
      menuItemViewModel2.ShowSelected = false;
      this._orderItem = menuItemViewModel2;
      extraItem?.ForEach(new Action<CustomMenuItemViewModel>(((Collection<CustomMenuItemViewModel>) observableCollection).Add));
      observableCollection.Add(this._groupItem);
      observableCollection.Add(this._orderItem);
      this._menuItems = observableCollection;
      this._popup = popup;
      if (this._popup == null)
        return;
      this._popup.Closed -= new EventHandler(this.OnClosed);
      this._popup.Closed += new EventHandler(this.OnClosed);
    }

    private void OnClosed(object sender, EventArgs e)
    {
      if (this._popup != null)
        this._popup.Closed -= new EventHandler(this.OnClosed);
      if (!(this._option.groupBy != this._originOption.groupBy) && !(this._option.orderBy != this._originOption.orderBy))
        return;
      UserActCollectUtils.AddSortOptionEvent("list_group_order", "sort_switch", this._option.groupBy, this._option.orderBy);
    }

    public SortTypeSelector(
      ProjectIdentity catId,
      List<SortTypeViewModel> sortTypes,
      SortOption option,
      EscPopup popup = null)
    {
      this._sortTypes = sortTypes;
      SortOption sortOption1 = option?.Copy();
      if (sortOption1 == null)
        sortOption1 = new SortOption()
        {
          groupBy = "none",
          orderBy = "dueDate"
        };
      this._originOption = sortOption1;
      SortOption sortOption2 = option?.Copy();
      if (sortOption2 == null)
        sortOption2 = new SortOption()
        {
          groupBy = "none",
          orderBy = "dueDate"
        };
      this._option = sortOption2;
      this._catId = catId;
      this._inKanban = false;
      this._inTimeline = true;
      SortTypeViewModel sortTypeViewModel = this._sortTypes.FirstOrDefault<SortTypeViewModel>((Func<SortTypeViewModel, bool>) (s => s.Id == this._option.groupBy));
      ObservableCollection<CustomMenuItemViewModel> observableCollection = new ObservableCollection<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "group", Utils.GetString("GroupBy"), SortTypeViewModel.GetGroupIconById(sortTypeViewModel?.Id ?? "none"));
      menuItemViewModel1.RightText = SortTypeViewModel.GetTitleById(sortTypeViewModel?.Id ?? "none", false);
      menuItemViewModel1.ExtraIcon = Utils.GetIcon("ArrowLine");
      menuItemViewModel1.ExtraIconAngle = -90;
      menuItemViewModel1.ExtraIconSize = 14;
      menuItemViewModel1.ExtraIconColor = ThemeUtil.GetColor("BaseColorOpacity40");
      menuItemViewModel1.ShowSelected = false;
      this._groupItem = menuItemViewModel1;
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "sort", Utils.GetString("SortBy"), SortTypeViewModel.GetSortIconById(this._option.orderBy, false));
      menuItemViewModel2.RightText = SortTypeViewModel.GetTitleById(this._option.orderBy, true);
      menuItemViewModel2.ExtraIcon = Utils.GetIcon("ArrowLine");
      menuItemViewModel2.ExtraIconAngle = -90;
      menuItemViewModel2.ExtraIconSize = 14;
      menuItemViewModel2.ExtraIconColor = ThemeUtil.GetColor("BaseColorOpacity40");
      menuItemViewModel2.ShowSelected = false;
      this._orderItem = menuItemViewModel2;
      observableCollection.Add(this._groupItem);
      observableCollection.Add(this._orderItem);
      this._menuItems = observableCollection;
      this._popup = popup;
      if (this._popup == null)
        return;
      this._popup.Closed -= new EventHandler(this.OnClosed);
      this._popup.Closed += new EventHandler(this.OnClosed);
    }

    private async Task SetReset()
    {
      if (this._inTimeline)
        return;
      int num1 = await SyncSortOrderDao.ExistTaskListSortOrder(this._option, this._catId.GetSortProjectId()) ? 1 : 0;
      string empty = string.Empty;
      if (num1 != 0)
      {
        switch (this._option.orderBy)
        {
          case "dueDate":
            empty = Utils.GetString("CenterOrderComboBoxByDefaultDuedate");
            break;
          case "project":
            empty = Utils.GetString("ResetProjectOrder");
            break;
          case "priority":
            empty = Utils.GetString("ResetPriorityOrder");
            break;
          case "tag":
            empty = Utils.GetString("ResetTagOrder");
            break;
        }
      }
      if (!string.IsNullOrEmpty(empty))
      {
        CustomMenuItemViewModel menuItemViewModel = this._menuItems.FirstOrDefault<CustomMenuItemViewModel>((Func<CustomMenuItemViewModel, bool>) (m => m.Value == (object) "reset"));
        if (menuItemViewModel != null)
        {
          menuItemViewModel.Text = empty;
        }
        else
        {
          this._menuItems.Add(new CustomMenuItemViewModel((object) ""));
          this._menuItems.Add(new CustomMenuItemViewModel((object) "reset", empty, (Geometry) null)
          {
            IsCenterText = true
          });
        }
      }
      else
      {
        int num2 = this._menuItems.IndexOf(this._orderItem);
        if (num2 >= this._menuItems.Count - 1)
          return;
        for (int index = this._menuItems.Count - 1; index > num2; --index)
          this._menuItems.RemoveAt(index);
      }
    }

    public async void Show()
    {
      SortTypeSelector sortTypeSelector = this;
      await sortTypeSelector.SetReset();
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) sortTypeSelector._menuItems, (Popup) sortTypeSelector._popup);
      customMenuList.MinWidth = 212.0;
      customMenuList.AutoClose = false;
      customMenuList.Operated += new EventHandler<object>(sortTypeSelector.OnItemSelected);
      customMenuList.Closed += new EventHandler(sortTypeSelector.OnControlUnloaded);
      customMenuList.Show();
    }

    private void OnItemSelected(object sender, object e)
    {
      if (!(e is string str) || string.IsNullOrEmpty(str))
        return;
      switch (str)
      {
        case "reset":
          this._popup.IsOpen = false;
          EventHandler<int> resetSortOrder = this.ResetSortOrder;
          if (resetSortOrder != null)
            resetSortOrder((object) this, 0);
          if (this._option == null)
            break;
          UserActCollectUtils.AddSortOptionEvent("list_group_order", "resume", this._option.groupBy, this._option.orderBy);
          break;
        case "group":
        case "sort":
          this._groupItem.Selected = false;
          this._orderItem.Selected = false;
          this.OnItemClick(sender, str);
          break;
        default:
          EventHandler<string> optionSelect = this.OptionSelect;
          if (optionSelect != null)
            optionSelect((object) this, str);
          this._popup.IsOpen = false;
          break;
      }
    }

    private void OnControlUnloaded(object sender, EventArgs e)
    {
      this.ResetSortOrder = (EventHandler<int>) null;
      this.OptionSelect = (EventHandler<string>) null;
      this.SortOptionSelect = (EventHandler<SortOption>) null;
    }

    public event EventHandler<int> ResetSortOrder;

    public event EventHandler<string> OptionSelect;

    public event EventHandler<SortOption> SortOptionSelect;

    private void OnItemClick(object sender, string type)
    {
      EscPopup escPopup1 = new EscPopup();
      escPopup1.PlacementTarget = (UIElement) (sender as FrameworkElement);
      escPopup1.Placement = PlacementMode.Left;
      escPopup1.HorizontalOffset = 212.0;
      escPopup1.StaysOpen = false;
      EscPopup escPopup2 = escPopup1;
      int num1 = this._menuItems.IndexOf(this._groupItem);
      double num2 = (double?) Application.Current?.FindResource((object) "Height36") ?? 36.0;
      switch (type)
      {
        case "group":
          List<SortTypeViewModel> source = this._inTimeline ? this._sortTypes.Where<SortTypeViewModel>((Func<SortTypeViewModel, bool>) (t => t.Id != "dueDate")).ToList<SortTypeViewModel>() : this._sortTypes.Where<SortTypeViewModel>((Func<SortTypeViewModel, bool>) (t => t.CanGroup())).ToList<SortTypeViewModel>();
          if (!this._inKanban)
            source.Add(new SortTypeViewModel("none"));
          List<CustomMenuItemViewModel> types1 = new List<CustomMenuItemViewModel>();
          types1.AddRange(source.Select<SortTypeViewModel, CustomMenuItemViewModel>((Func<SortTypeViewModel, CustomMenuItemViewModel>) (sortType =>
          {
            return new CustomMenuItemViewModel((object) sortType.Id, SortTypeViewModel.GetTitleById(sortType.Id, false), (Geometry) null)
            {
              Selected = sortType.Id == this._option.groupBy
            };
          })));
          escPopup2.Closed += (EventHandler) ((o, e) => this._groupItem.SetExtraIconAngle(-90));
          escPopup2.VerticalOffset = num1 >= 0 ? (double) (num1 + 1) * num2 + 8.0 : 0.0;
          this._groupItem.SetExtraIconAngle(0);
          CustomMenuList customMenuList1 = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types1, (Popup) escPopup2);
          customMenuList1.MinWidth = 130.0;
          customMenuList1.Operated += new EventHandler<object>(this.OnGroupSelected);
          customMenuList1.Show();
          break;
        case "sort":
          List<CustomMenuItemViewModel> types2 = new List<CustomMenuItemViewModel>();
          if (!this._inTimeline)
          {
            List<SortTypeViewModel> list = this._sortTypes.ToList<SortTypeViewModel>();
            types2.AddRange(list.Where<SortTypeViewModel>((Func<SortTypeViewModel, bool>) (s =>
            {
              if (!(s.Id != "project"))
                return false;
              return !this._isGroup || !(s.Id == "assignee");
            })).Select<SortTypeViewModel, CustomMenuItemViewModel>((Func<SortTypeViewModel, CustomMenuItemViewModel>) (sortType =>
            {
              return new CustomMenuItemViewModel((object) sortType.Id, sortType.Title, (Geometry) null)
              {
                Selected = sortType.Id == this._option.orderBy
              };
            })));
          }
          else
          {
            List<CustomMenuItemViewModel> menuItemViewModelList1 = types2;
            CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "sortOrder", Utils.GetString("Default"), (Geometry) null);
            menuItemViewModel1.Selected = "sortOrder" == this._option.orderBy;
            menuItemViewModelList1.Add(menuItemViewModel1);
            List<CustomMenuItemViewModel> menuItemViewModelList2 = types2;
            CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "dueDate", Utils.GetString("TimeToDate"), (Geometry) null);
            menuItemViewModel2.Selected = "dueDate" == this._option.orderBy;
            menuItemViewModelList2.Add(menuItemViewModel2);
          }
          escPopup2.Closed += (EventHandler) ((o, e) => this._orderItem.SetExtraIconAngle(-90));
          escPopup2.VerticalOffset = num1 >= 0 ? (double) (num1 + 2) * num2 + 8.0 : 0.0;
          this._orderItem.SetExtraIconAngle(0);
          CustomMenuList customMenuList2 = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types2, (Popup) escPopup2);
          customMenuList2.MinWidth = 130.0;
          customMenuList2.Operated += new EventHandler<object>(this.OnSortSelected);
          customMenuList2.Show();
          break;
      }
    }

    private void OnGroupSelected(object sender, object e)
    {
      if (!(e is string id))
        return;
      this._option.groupBy = id;
      this._groupItem.Icon = SortTypeViewModel.GetGroupIconById(id);
      this._groupItem.RightText = SortTypeViewModel.GetTitleById(id, false);
      EventHandler<SortOption> sortOptionSelect = this.SortOptionSelect;
      if (sortOptionSelect != null)
        sortOptionSelect((object) this, this._option.Copy());
      this.SetReset();
    }

    private void OnSortSelected(object sender, object e)
    {
      if (!(e is string id))
        return;
      this._option.orderBy = id;
      this._orderItem.Icon = SortTypeViewModel.GetSortIconById(id, false);
      this._orderItem.RightText = SortTypeViewModel.GetTitleById(id, this._inTimeline);
      EventHandler<SortOption> sortOptionSelect = this.SortOptionSelect;
      if (sortOptionSelect != null)
        sortOptionSelect((object) this, this._option.Copy());
      this.SetReset();
    }
  }
}
