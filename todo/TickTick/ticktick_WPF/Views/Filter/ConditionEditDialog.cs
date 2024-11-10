// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ConditionEditDialog
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ConditionEditDialog : UserControl, IComponentConnector
  {
    private FilterConditionViewModel _viewModel;
    internal Grid AllItem;
    internal Grid LogicSelector;
    internal ListView ListView;
    internal Button SaveBtn;
    internal Button CancelBtn;
    private bool _contentLoaded;

    public ConditionEditDialog()
    {
      this.InitializeComponent();
      this.CancelBtn.Click += (RoutedEventHandler) ((sender, e) =>
      {
        this.Restore();
        EventHandler onCancel = this.OnCancel;
        if (onCancel == null)
          return;
        onCancel((object) this, (EventArgs) null);
      });
      this.SaveBtn.Click += (RoutedEventHandler) ((sender, e) =>
      {
        this.IsSave = true;
        if (!this.CanSave())
          return;
        this.SyncOriginal();
        EventHandler<FilterConditionViewModel> onSave = this.OnSave;
        if (onSave == null)
          return;
        onSave((object) this, this.ViewModel);
      });
      if (!ConditionEditDialog.IsUsedInSearch)
        return;
      this.SaveBtn.Content = (object) Utils.GetString("OK");
    }

    protected FilterConditionViewModel ViewModel
    {
      get => this._viewModel;
      set
      {
        this._viewModel = value;
        this.DataContext = (object) value;
        if (value.ItemsSource == null)
          return;
        this.ListView.ItemsSource = (IEnumerable) value.ItemsSource;
      }
    }

    public bool IsSave { get; set; }

    private static bool IsUsedInSearch => false;

    public event EventHandler OnCancel;

    public event EventHandler<FilterConditionViewModel> OnSave;

    public event EventHandler<LogicType> OnLogicChanged;

    public event EventHandler<FilterConditionViewModel> OnSelectedValueChanged;

    protected virtual bool CanSave() => this.ViewModel.IsAllSelected && this.ViewModel.ShowAll;

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      FilterListItemViewModel selectedItem = sender is ListView listView ? (FilterListItemViewModel) listView.SelectedItem : (FilterListItemViewModel) (object) null;
      if (selectedItem == null || selectedItem.DisplayType != FilterItemDisplayType.Normal || this.HandleCheckedAble(selectedItem))
        return;
      this.OnItemSelected(selectedItem);
    }

    public void OnItemSelected(FilterListItemViewModel selectedItem)
    {
      selectedItem.Selected = !selectedItem.Selected;
      if (selectedItem.IsProjectGroup || selectedItem.IsAssignee && string.IsNullOrEmpty(selectedItem.GroupId))
      {
        selectedItem.Children?.ForEach((Action<FilterListItemViewModel>) (item => item.Selected = selectedItem.Selected));
        selectedItem.PartSelected = false;
      }
      if ((selectedItem.IsSecondLevel || selectedItem.IsAssignee) && !string.IsNullOrEmpty(selectedItem.GroupId))
      {
        FilterListItemViewModel listItemViewModel1 = this._viewModel.ItemsSource.FirstOrDefault<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (model => model.Value.ToString() == selectedItem.GroupId));
        if (listItemViewModel1 != null)
        {
          if (selectedItem.Selected)
          {
            listItemViewModel1.PartSelected = true;
          }
          else
          {
            listItemViewModel1.Selected = false;
            FilterListItemViewModel listItemViewModel2 = listItemViewModel1;
            List<FilterListItemViewModel> children = listItemViewModel1.Children;
            int num = children != null ? (children.Any<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (item => item.Selected)) ? 1 : 0) : 0;
            listItemViewModel2.PartSelected = num != 0;
          }
        }
      }
      if (selectedItem.Selected)
      {
        if (selectedItem.IsAllItem)
        {
          foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this._viewModel.ItemsSource)
          {
            if (!listItemViewModel.IsAllItem)
              listItemViewModel.Selected = false;
          }
        }
        else
        {
          foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this._viewModel.ItemsSource)
          {
            if (listItemViewModel.IsAllItem)
            {
              listItemViewModel.Selected = false;
              break;
            }
          }
        }
      }
      if (selectedItem.Selected)
        this._viewModel.IsAllSelected = false;
      else
        this.SetAllSelected();
      EventHandler<FilterConditionViewModel> selectedValueChanged = this.OnSelectedValueChanged;
      if (selectedValueChanged != null)
        selectedValueChanged((object) this, this._viewModel);
      this.SaveBtn.IsEnabled = this.CanSave();
    }

    public virtual void Restore()
    {
    }

    protected virtual void SyncOriginal()
    {
    }

    private void SetAllSelected()
    {
      this._viewModel.IsAllSelected = this._viewModel.ItemsSource.FirstOrDefault<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (x => x.Selected)) == null;
    }

    protected virtual bool HandleCheckedAble(FilterListItemViewModel selectedItem) => false;

    private void OnLogicSelected(object sender, LogicType e)
    {
      EventHandler<LogicType> onLogicChanged = this.OnLogicChanged;
      if (onLogicChanged == null)
        return;
      onLogicChanged((object) this, e);
    }

    private void OnAllToggle(object sender, MouseButtonEventArgs e)
    {
      this._viewModel.IsAllSelected = !this._viewModel.IsAllSelected;
      if (this._viewModel.IsAllSelected)
      {
        foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this._viewModel.ItemsSource)
          listItemViewModel.Selected = false;
      }
      EventHandler<FilterConditionViewModel> selectedValueChanged = this.OnSelectedValueChanged;
      if (selectedValueChanged == null)
        return;
      selectedValueChanged((object) this, (FilterConditionViewModel) null);
    }

    public void OnOpenClick(FilterListItemViewModel model)
    {
      model.Unfold = !model.Unfold;
      if (model.Unfold)
      {
        int index = this._viewModel.ItemsSource.IndexOf(model);
        if (index < 0)
          return;
        model.Children?.ForEach((Action<FilterListItemViewModel>) (item => this._viewModel.ItemsSource.Insert(++index, item)));
      }
      else
        model.Children?.ForEach((Action<FilterListItemViewModel>) (item => this._viewModel.ItemsSource.Remove(item)));
    }

    public void ValueChanged()
    {
      EventHandler<FilterConditionViewModel> selectedValueChanged = this.OnSelectedValueChanged;
      if (selectedValueChanged == null)
        return;
      selectedValueChanged((object) this, this._viewModel);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/conditioneditdialog.xaml", UriKind.Relative));
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
          this.AllItem = (Grid) target;
          this.AllItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAllToggle);
          break;
        case 2:
          this.LogicSelector = (Grid) target;
          break;
        case 3:
          this.ListView = (ListView) target;
          this.ListView.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 4:
          this.SaveBtn = (Button) target;
          break;
        case 5:
          this.CancelBtn = (Button) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
