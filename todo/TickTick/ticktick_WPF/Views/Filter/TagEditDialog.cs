// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.TagEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class TagEditDialog : ConditionEditDialogBase<string>
  {
    private List<string> _originalTags;
    private List<string> _selectedTags;

    public TagEditDialog()
      : this(false)
    {
    }

    private TagEditDialog(bool showAll)
      : this(showAll, new List<string>())
    {
    }

    public TagEditDialog(bool showAll, List<string> selectedTags, LogicType logicType = LogicType.Or)
    {
      this.InitializeComponent();
      this._selectedTags = selectedTags;
      this._originalTags = selectedTags.ToList<string>();
      this.InitData(showAll, (ICollection<string>) selectedTags, logicType);
      this.OnSelectedValueChanged += (EventHandler<FilterConditionViewModel>) ((sender, model) =>
      {
        if (!this.ViewModel.IsAllSelected)
        {
          this._selectedTags = this.GetSelectedValues();
          this.ViewModel.WithoutTagsSelected = this._selectedTags.Contains("!tag");
          if (this._selectedTags.Contains("!tag"))
          {
            this._selectedTags.Remove("!tag");
            this._selectedTags.Insert(0, "!tag");
          }
          this.ViewModel.WithTagsSelected = this._selectedTags.Contains("*withtags");
          if (this._selectedTags.Contains("*withtags"))
          {
            this._selectedTags.Remove("*withtags");
            this._selectedTags.Insert(0, "*withtags");
          }
          this.ViewModel.AllLogicEnabled = !this._selectedTags.Contains("!tag");
          EventHandler<List<string>> selectedTagChanged = this.OnSelectedTagChanged;
          if (selectedTagChanged == null)
            return;
          selectedTagChanged((object) this, this._selectedTags);
        }
        else
        {
          this._selectedTags = new List<string>();
          EventHandler<List<string>> selectedTagChanged = this.OnSelectedTagChanged;
          if (selectedTagChanged == null)
            return;
          selectedTagChanged((object) this, new List<string>());
        }
      });
    }

    public event EventHandler<List<string>> OnSelectedTagChanged;

    protected override bool HandleCheckedAble(FilterListItemViewModel selectedItem)
    {
      if (selectedItem.Value.ToString() == "!tag" && this.ViewModel.Logic == LogicType.And)
      {
        Utils.Toast(Utils.GetString("CannotSelectNoTagAndLogicAndAtTheSameTime"));
        return true;
      }
      if (this.ViewModel.Logic == LogicType.Not)
      {
        switch (selectedItem.Value.ToString())
        {
          case "!tag":
            if (this.ViewModel.WithTagsSelected)
            {
              Utils.Toast(Utils.GetString("CannotSelectWithTagNoTagAndLogicNotAtTheSameTime"));
              return true;
            }
            break;
          case "*withtags":
            if (this.ViewModel.WithoutTagsSelected)
            {
              Utils.Toast(Utils.GetString("CannotSelectWithTagNoTagAndLogicNotAtTheSameTime"));
              return true;
            }
            break;
        }
      }
      return false;
    }

    protected override bool CanSave() => this._selectedTags.Count > 0 || base.CanSave();

    protected override void SyncOriginal()
    {
      this._originalTags = this._selectedTags.ToList<string>();
    }

    private void InitData(bool showAll, ICollection<string> selectedTags, LogicType logicType)
    {
      ObservableCollection<FilterListItemViewModel> observableCollection = new ObservableCollection<FilterListItemViewModel>();
      List<TagModel> tags = TagDataHelper.GetTags();
      // ISSUE: explicit non-virtual call
      if (tags != null && __nonvirtual (tags.Count) > 0)
      {
        observableCollection.Add(new FilterListItemViewModel()
        {
          Title = Utils.GetString("WithTags"),
          Icon = "IcWithTag",
          Value = (object) "*withtags",
          ShowIcon = true,
          Selected = selectedTags.Contains("*withtags")
        });
        observableCollection.Add(new FilterListItemViewModel()
        {
          Title = Utils.GetString("NoTagsTitle"),
          Icon = "IcNoTag",
          Value = (object) "!tag",
          ShowIcon = true,
          Selected = selectedTags.Contains("!tag")
        });
        List<TagModel> list1 = tags.Where<TagModel>((Func<TagModel, bool>) (t => t.IsChild())).ToList<TagModel>();
        foreach (FilterListItemViewModel listItemViewModel1 in tags.Where<TagModel>((Func<TagModel, bool>) (tag => !tag.IsChild())).Select<TagModel, FilterListItemViewModel>((Func<TagModel, FilterListItemViewModel>) (tag => new FilterListItemViewModel(tag, selectedTags))))
        {
          FilterListItemViewModel item = listItemViewModel1;
          if (item.IsTagParent)
          {
            List<TagModel> list2 = list1.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == (string) item.Value)).ToList<TagModel>();
            item.Children = list2.Select<TagModel, FilterListItemViewModel>((Func<TagModel, FilterListItemViewModel>) (t => new FilterListItemViewModel(t, selectedTags))).ToList<FilterListItemViewModel>();
            FilterListItemViewModel listItemViewModel2 = item;
            int num;
            if (!item.Selected)
            {
              List<FilterListItemViewModel> children = item.Children;
              bool? nullable1;
              bool? nullable2;
              if (children == null)
              {
                nullable1 = new bool?();
                nullable2 = nullable1;
              }
              else
                nullable2 = new bool?(children.Any<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (c => c.Selected)));
              nullable1 = nullable2;
              num = nullable1.Value ? 1 : 0;
            }
            else
              num = 0;
            listItemViewModel2.PartSelected = num != 0;
          }
          observableCollection.Add(item);
          if (item.Unfold)
            item.Children?.ForEach(new Action<FilterListItemViewModel>(((Collection<FilterListItemViewModel>) observableCollection).Add));
        }
      }
      this.ViewModel = new FilterConditionViewModel()
      {
        Type = CondType.Tag,
        ShowAll = showAll,
        AllData = Utils.GetIconData("IcAllTag"),
        AllPathWidth = 16,
        CanSelectLogic = true,
        Logic = logicType,
        ItemsSource = observableCollection,
        IsAllSelected = selectedTags.Count == 0,
        WithTagsSelected = selectedTags.Contains("*withtags"),
        WithoutTagsSelected = selectedTags.Contains("!tag"),
        SupportedLogic = new List<LogicType>()
        {
          LogicType.And,
          LogicType.Or,
          LogicType.Not
        }
      };
    }

    public override void Restore()
    {
      this.RestoreSetSelected();
      EventHandler<List<string>> selectedTagChanged = this.OnSelectedTagChanged;
      if (selectedTagChanged == null)
        return;
      selectedTagChanged((object) this, this._originalTags);
    }

    private void RestoreSetSelected()
    {
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
        listItemViewModel.Selected = this._originalTags.Contains(listItemViewModel.Value.ToString());
    }
  }
}
