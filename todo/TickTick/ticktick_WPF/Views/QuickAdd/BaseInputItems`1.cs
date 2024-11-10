// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.BaseInputItems`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public abstract class BaseInputItems<T> : QuickInputItems
  {
    private string _inputText;
    private InputItemViewModel<T> _selectedItem;

    public event EventHandler<InputItemViewModel<T>> OnItemSelected;

    protected void LoadData(bool selectFirst = true)
    {
      ObservableCollection<InputItemViewModel<T>> observableCollection = this.InitData();
      if (observableCollection == null || observableCollection.Count <= 0)
        return;
      if (selectFirst)
        observableCollection[0].Selected = true;
      this.Items.ItemsSource = (IEnumerable) observableCollection;
    }

    private bool IsExactlyMatched()
    {
      InputItemViewModel<T> selectedItem = this.GetSelectedItem();
      if (selectedItem == null)
        return false;
      return string.Equals(selectedItem.Title, this._inputText, StringComparison.CurrentCultureIgnoreCase) || selectedItem.Pinyin == this._inputText;
    }

    public override bool Filter(string filter, List<string> selected = null)
    {
      try
      {
        string key = filter.ToLower();
        this._inputText = key;
        ObservableCollection<InputItemViewModel<T>> source = this.InitData();
        if (string.IsNullOrEmpty(key))
        {
          source[0].Selected = true;
          this.Items.ItemsSource = (IEnumerable) source;
          return true;
        }
        List<InputItemViewModel<T>> list = source.Where<InputItemViewModel<T>>((Func<InputItemViewModel<T>, bool>) (item =>
        {
          if (item == null)
            return false;
          if (!string.IsNullOrEmpty(item.Title) && item.Title.ToLower().Contains(key) || !string.IsNullOrEmpty(item.Pinyin) && item.Pinyin.Contains(key))
            return true;
          return !string.IsNullOrEmpty(item.Inits) && item.Inits.Contains(key);
        })).ToList<InputItemViewModel<T>>();
        if (list.Count > 0)
        {
          ObservableCollection<InputItemViewModel<T>> observableCollection = new ObservableCollection<InputItemViewModel<T>>(list);
          observableCollection[0].Selected = true;
          this.Items.ItemsSource = (IEnumerable) observableCollection;
          return true;
        }
        if (this.IsTag())
        {
          this.Items.ItemsSource = (IEnumerable) new ObservableCollection<InputItemViewModel<T>>();
          return true;
        }
        this.Items.ItemsSource = (IEnumerable) new ObservableCollection<InputItemViewModel<T>>();
        return false;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    protected abstract ObservableCollection<InputItemViewModel<T>> InitData();

    protected override void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Grid grid) || !(grid.DataContext is InputItemViewModel<T> dataContext))
        return;
      EventHandler<InputItemViewModel<T>> onItemSelected = this.OnItemSelected;
      if (onItemSelected != null)
        onItemSelected((object) this, dataContext);
      e.Handled = true;
    }

    public override void TrySelectItem(bool exactly = false)
    {
      InputItemViewModel<T> selectedItem = this.GetSelectedItem();
      if (selectedItem == null || exactly && !this.IsExactlyMatched())
        return;
      EventHandler<InputItemViewModel<T>> onItemSelected = this.OnItemSelected;
      if (onItemSelected == null)
        return;
      onItemSelected((object) this, selectedItem);
    }

    public override bool TrySelectIteWithSpace(string content)
    {
      if (this.Items.HasItems && this.Items.ItemsSource is ObservableCollection<InputItemViewModel<T>> itemsSource && itemsSource.Count == 1)
      {
        InputItemViewModel<T> e = itemsSource.FirstOrDefault<InputItemViewModel<T>>((Func<InputItemViewModel<T>, bool>) (model => model.Selected));
        if (e != null && string.Equals(content, e.Title, StringComparison.CurrentCultureIgnoreCase))
        {
          EventHandler<InputItemViewModel<T>> onItemSelected = this.OnItemSelected;
          if (onItemSelected != null)
            onItemSelected((object) this, e);
          return true;
        }
      }
      return false;
    }

    protected override void OnItemEnter(object sender, MouseEventArgs e)
    {
      if (!(sender is Grid grid) || !(grid.DataContext is InputItemViewModel<T> dataContext))
        return;
      if (this._selectedItem == null)
      {
        this._selectedItem = dataContext;
      }
      else
      {
        this._selectedItem.Selected = false;
        this._selectedItem = dataContext;
      }
      dataContext.Selected = true;
    }

    private InputItemViewModel<T> GetSelectedItem()
    {
      return this.Items.HasItems && this.Items.ItemsSource is ObservableCollection<InputItemViewModel<T>> itemsSource ? itemsSource.FirstOrDefault<InputItemViewModel<T>>((Func<InputItemViewModel<T>, bool>) (model => model.Selected)) : (InputItemViewModel<T>) null;
    }

    public override void Move(bool forward)
    {
      if (!this.Items.HasItems || !(this.Items.ItemsSource is ObservableCollection<InputItemViewModel<T>> itemsSource))
        return;
      InputItemViewModel<T> selectedItem = this.GetSelectedItem();
      if (selectedItem == null)
      {
        itemsSource[0].Selected = true;
      }
      else
      {
        int num = itemsSource.TakeWhile<InputItemViewModel<T>>((Func<InputItemViewModel<T>, bool>) (model => !model.Selected)).Count<InputItemViewModel<T>>();
        int index = forward ? num - 1 : num + 1;
        if (index < 0)
          index = itemsSource.Count - 1;
        else if (index >= itemsSource.Count)
          index = 0;
        selectedItem.Selected = false;
        this._selectedItem = itemsSource[index];
        itemsSource[index].Selected = true;
        this.Items.ScrollIntoView((object) itemsSource[index]);
      }
    }
  }
}
