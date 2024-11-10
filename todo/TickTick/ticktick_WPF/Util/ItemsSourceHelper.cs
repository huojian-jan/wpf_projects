// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ItemsSourceHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util
{
  public class ItemsSourceHelper
  {
    public static void SetHidableItemsSource<T>(ItemsControl itemsControl, List<T> items) where T : BaseHidableViewModel
    {
      if (itemsControl.ItemsSource is ObservableCollection<T> itemsSource)
      {
        int num1 = Math.Min(itemsSource.Count, items.Count);
        if (itemsSource.Count != items.Count)
        {
          int num2 = Math.Abs(itemsSource.Count - items.Count);
          for (int index = num1; index < num1 + num2; ++index)
          {
            if (itemsSource.Count > items.Count)
              itemsSource[index].IsHide = true;
            else if (itemsSource.Count < items.Count)
              itemsSource.Add(items[index]);
          }
        }
        for (int index = 0; index < num1; ++index)
          itemsSource[index] = items[index];
      }
      else
        itemsControl.ItemsSource = (IEnumerable) new ObservableCollection<T>(items);
    }

    public static void SetItemsSource<T>(ItemsControl itemsControl, List<T> items) where T : INotifyPropertyChanged
    {
      if (items == null)
        items = new List<T>();
      if (itemsControl.ItemsSource is ObservableCollection<T> itemsSource)
      {
        int num1 = Math.Min(itemsSource.Count, items.Count);
        if (itemsSource.Count != items.Count)
        {
          int num2 = Math.Abs(itemsSource.Count - items.Count);
          ObservableCollection<T> observableCollection = new ObservableCollection<T>();
          for (int index = num1; index < num1 + num2; ++index)
          {
            if (itemsSource.Count > items.Count)
              observableCollection.Add(itemsSource[index]);
            else if (itemsSource.Count < items.Count)
              itemsSource.Add(items[index]);
          }
          foreach (T obj in (Collection<T>) observableCollection)
            itemsSource.Remove(obj);
        }
        for (int index = 0; index < num1; ++index)
          itemsSource[index] = items[index];
      }
      else
        itemsControl.ItemsSource = (IEnumerable) new ObservableCollection<T>(items);
    }

    public static void SetHabitListItemsSource(
      ListView itemsControl,
      List<HabitItemBaseViewModel> items,
      int num)
    {
      if (itemsControl.ItemsSource is ObservableCollection<HabitItemBaseViewModel> itemsSource)
      {
        int num1 = Math.Min(itemsSource.Count, items.Count);
        if (itemsSource.Count != items.Count)
        {
          int num2 = Math.Abs(itemsSource.Count - items.Count);
          ObservableCollection<HabitItemBaseViewModel> observableCollection = new ObservableCollection<HabitItemBaseViewModel>();
          for (int index = num1; index < num1 + num2; ++index)
          {
            if (itemsSource.Count > items.Count)
            {
              if (index > num)
              {
                observableCollection.Add(itemsSource[index]);
              }
              else
              {
                HabitItemBaseViewModel itemBaseViewModel = itemsSource[index].Clone();
                itemBaseViewModel.IsHide = true;
                itemBaseViewModel.Id = string.Empty;
                itemsSource[index] = itemBaseViewModel;
              }
            }
            else if (itemsSource.Count < items.Count)
              itemsSource.Add(items[index]);
          }
          foreach (HabitItemBaseViewModel itemBaseViewModel in (Collection<HabitItemBaseViewModel>) observableCollection)
            itemsSource.Remove(itemBaseViewModel);
        }
        for (int index = 0; index < num1; ++index)
        {
          itemsSource[index] = items[index];
          itemsSource[index].IsHide = items[index].IsHide;
        }
      }
      else
        itemsControl.ItemsSource = (IEnumerable) new ObservableCollection<HabitItemBaseViewModel>(items);
    }

    public static void CopyTo<T>(List<T> source, ObservableCollection<T> target)
    {
      lock (target)
      {
        int num1 = Math.Min(source.Count, target.Count);
        if (source.Count != target.Count)
        {
          int num2 = Math.Abs(source.Count - target.Count);
          List<T> objList = new List<T>();
          for (int index = num1; index < num1 + num2; ++index)
          {
            if (target.Count > source.Count)
              objList.Add(target[index]);
            else if (target.Count < source.Count)
              target.Add(source[index]);
          }
          foreach (T obj in objList)
            target.Remove(obj);
        }
        for (int index = 0; index < num1; ++index)
        {
          T obj1 = target[index];
          T obj2 = source[index];
          if ((object) obj1 == null || (object) obj2 == null || !obj1.Equals((object) obj2))
            target[index] = obj2;
        }
      }
    }
  }
}
