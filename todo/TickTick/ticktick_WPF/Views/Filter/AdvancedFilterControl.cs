// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.AdvancedFilterControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class AdvancedFilterControl : UserControl, IComponentConnector
  {
    private AdvancedFilterViewModel _viewModel = new AdvancedFilterViewModel();
    internal WrapPanel ConditionPanel;
    private bool _contentLoaded;

    public bool ExistNote { get; set; }

    public AdvancedFilterControl() => this.InitializeComponent();

    public List<CardViewModel> CardList
    {
      private get => this.ViewModel.CardList;
      set
      {
        this.ViewModel.CardList = value;
        this.NotifyDataSetChanged();
      }
    }

    public AdvancedFilterViewModel ViewModel
    {
      get => this._viewModel;
      set
      {
        this._viewModel = value;
        this.NotifyDataSetChanged();
      }
    }

    private void NotifyDataSetChanged()
    {
      this.ConditionPanel.Children.Clear();
      if (this.ViewModel.CardList == null || this.ViewModel.CardList.Count <= 0)
        return;
      for (int index = 0; index < this.ViewModel.CardList.Count; ++index)
      {
        this.ViewModel.CardList[index].Index = index;
        ConditionCard filterConditionCard = this.GetFilterConditionCard(this.ViewModel.CardList[index]);
        if (filterConditionCard != null)
        {
          filterConditionCard.OnConditionChanged += new EventHandler<ChangeData>(this.OnConditionChanged);
          filterConditionCard.GetDropDownList += new ConditionCard.GetDropdownListHandler(this.CardGetDropDownList);
          this.ConditionPanel.Children.Add((UIElement) filterConditionCard);
        }
      }
    }

    private void NotifyItemChanged(int index, CardViewModel viewModel)
    {
      ConditionCard filterConditionCard = this.GetFilterConditionCard(viewModel);
      filterConditionCard.OnConditionChanged += new EventHandler<ChangeData>(this.OnConditionChanged);
      filterConditionCard.GetDropDownList += new ConditionCard.GetDropdownListHandler(this.CardGetDropDownList);
      this.ConditionPanel.Children.RemoveAt(index);
      this.ConditionPanel.Children.Insert(index, (UIElement) filterConditionCard);
    }

    private List<CondType> CardGetDropDownList()
    {
      List<CondType> dropDownList = new List<CondType>()
      {
        CondType.Lists,
        CondType.Tag,
        CondType.Date,
        CondType.Priority,
        CondType.Assignee,
        CondType.KeyWords,
        CondType.TaskType
      };
      foreach (CardViewModel cardViewModel in this.CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>())
      {
        switch (cardViewModel)
        {
          case ProjectOrGroupCardViewModel _:
            dropDownList.Remove(CondType.Lists);
            continue;
          case DateCardViewModel _:
            dropDownList.Remove(CondType.Date);
            continue;
          case PriorityCardViewModel _:
            dropDownList.Remove(CondType.Priority);
            continue;
          case AssigneeCardViewModel _:
            dropDownList.Remove(CondType.Assignee);
            continue;
          case KeywordsViewModel _:
            dropDownList.Remove(CondType.KeyWords);
            continue;
          case TaskTypeViewModel _:
            dropDownList.Remove(CondType.TaskType);
            continue;
          default:
            continue;
        }
      }
      List<TagModel> tags = TagDataHelper.GetTags();
      if ((tags == null || tags.Count == 0) && dropDownList.Contains(CondType.Tag))
        dropDownList.Remove(CondType.Tag);
      if (CacheManager.GetProjects().Count<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList())) == 0 && dropDownList.Contains(CondType.Assignee))
        dropDownList.Remove(CondType.Assignee);
      if (!this.ExistNote)
        dropDownList.Remove(CondType.TaskType);
      return dropDownList;
    }

    private ConditionCard GetFilterConditionCard(CardViewModel model)
    {
      ConditionCard filterConditionCard = (ConditionCard) null;
      model.Version = this.ViewModel.Version;
      switch (model.Type)
      {
        case CardType.InitFilter:
          filterConditionCard = (ConditionCard) new InitConditionCard(model);
          break;
        case CardType.InitLogic:
        case CardType.LogicAnd:
        case CardType.LogicOr:
          filterConditionCard = (ConditionCard) new LogicConditionCard(model);
          break;
        case CardType.AddMore:
          filterConditionCard = (ConditionCard) new AddMoreCard(model);
          break;
        case CardType.Filter:
          switch (model)
          {
            case ProjectOrGroupCardViewModel viewModel1:
              filterConditionCard = (ConditionCard) new ProjectOrGroupCard((CardViewModel) viewModel1);
              break;
            case TagCardViewModel viewModel2:
              filterConditionCard = (ConditionCard) new TagCard((CardViewModel) viewModel2);
              break;
            case DateCardViewModel viewModel3:
              filterConditionCard = (ConditionCard) new DateCard((CardViewModel) viewModel3);
              break;
            case PriorityCardViewModel viewModel4:
              filterConditionCard = (ConditionCard) new PriorityCard((CardViewModel) viewModel4);
              break;
            case AssigneeCardViewModel viewModel5:
              filterConditionCard = (ConditionCard) new AssigneeCard((CardViewModel) viewModel5);
              break;
            case KeywordsViewModel viewModel6:
              filterConditionCard = (ConditionCard) new KeywordsCard((CardViewModel) viewModel6);
              break;
            case TaskTypeViewModel viewModel7:
              filterConditionCard = (ConditionCard) new TaskTypeCard((CardViewModel) viewModel7);
              break;
          }
          break;
      }
      return filterConditionCard;
    }

    private void OnConditionChanged(object sender, ChangeData delta)
    {
      switch (delta.Type)
      {
        case ChangeType.AddMore:
          this.CardList = FilterConditionProvider.AddMoreCondition(this.ViewModel.CardList);
          break;
        case ChangeType.Filter:
        case ChangeType.Logic:
          delta.To.Index = delta.From.Index;
          this.CardList[delta.From.Index] = delta.To;
          this.NotifyItemChanged(delta.To.Index, delta.To);
          this.AdjustLeftCondition(delta);
          break;
      }
      this.TryRemoveEmptyFilter();
    }

    private void TryRemoveEmptyFilter()
    {
      if (this.CardList.Count != 5 || this.CardList[1].Type != CardType.InitLogic || this.CardList[2].Type != CardType.InitFilter)
        return;
      this.CardList.RemoveAt(1);
      this.CardList.RemoveAt(1);
      this.CardList = FilterConditionProvider.AddMoreCondition(this.ViewModel.CardList);
      this.NotifyDataSetChanged();
    }

    private void AdjustLeftCondition(ChangeData delta)
    {
      if (delta.To.Index - 1 <= 0)
        return;
      switch (delta.To.Type)
      {
        case CardType.InitFilter:
          this.AdjustLeftCard(delta, (Func<bool>) (() => this.CardList[delta.To.Index - 1].Type != 0), CardType.InitLogic);
          break;
        case CardType.Filter:
          this.AdjustLeftCard(delta, (Func<bool>) (() => this.CardList[delta.To.Index - 1].Type != CardType.LogicAnd && this.CardList[delta.To.Index - 1].Type != CardType.LogicOr), CardType.LogicAnd);
          break;
      }
    }

    private void AdjustLeftCard(ChangeData changeData, Func<bool> condition, CardType type)
    {
      if (changeData.To.Index <= 1)
        return;
      int index = changeData.To.Index - 1;
      if (!condition())
        return;
      CardViewModel viewModel = new CardViewModel()
      {
        Type = type,
        Index = index
      };
      this.CardList[index] = viewModel;
      this.NotifyItemChanged(index, viewModel);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/advancedfiltercontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.ConditionPanel = (WrapPanel) target;
      else
        this._contentLoaded = true;
    }
  }
}
