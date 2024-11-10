// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ConditionSpanDateItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
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
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ConditionSpanDateItem : UserControl, IComponentConnector
  {
    private static readonly List<ComboBoxViewModel> FromDaysComboBoxItems = new List<ComboBoxViewModel>();
    private static readonly List<ComboBoxViewModel> ToDaysComboBoxItems = new List<ComboBoxViewModel>();
    internal Grid Container;
    internal CustomComboBox FromDayComboBox;
    internal CustomComboBox ToDayComboBox;
    private bool _contentLoaded;

    static ConditionSpanDateItem()
    {
      for (int nDay = -30; nDay < 31; ++nDay)
      {
        string nthDayString = FilterUtils.GetNthDayString(nDay);
        ConditionSpanDateItem.FromDaysComboBoxItems.Add(new ComboBoxViewModel((object) nDay, nthDayString));
        ConditionSpanDateItem.ToDaysComboBoxItems.Add(new ComboBoxViewModel((object) nDay, nthDayString));
      }
      ConditionSpanDateItem.FromDaysComboBoxItems.Insert(0, new ComboBoxViewModel((object) "-", "-"));
      ConditionSpanDateItem.ToDaysComboBoxItems.Add(new ComboBoxViewModel((object) "-", "-"));
    }

    public ConditionSpanDateItem() => this.InitializeComponent();

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is FilterListItemViewModel dataContext))
        return;
      Utils.FindParent<ConditionEditDialog>((DependencyObject) this)?.OnItemSelected(dataContext);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is FilterListItemViewModel dataContext))
        return;
      ConditionSpanDateItem.SetComboBoxItems(this.FromDayComboBox, ConditionSpanDateItem.FromDaysComboBoxItems, dataContext.DaysFrom, new int?());
      ConditionSpanDateItem.SetComboBoxItems(this.ToDayComboBox, ConditionSpanDateItem.ToDaysComboBoxItems, dataContext.DaysTo, dataContext.DaysFrom);
    }

    private static void SetComboBoxItems(
      CustomComboBox comboBox,
      List<ComboBoxViewModel> data,
      int? selectedVal,
      int? minVal)
    {
      ObservableCollection<ComboBoxViewModel> items = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel selected = data.LastOrDefault<ComboBoxViewModel>();
      foreach (ComboBoxViewModel comboBoxViewModel1 in data)
      {
        if (comboBoxViewModel1.Value is int num)
        {
          ComboBoxViewModel comboBoxViewModel2 = comboBoxViewModel1;
          int? nullable;
          int num1;
          if (minVal.HasValue)
          {
            int num2 = num;
            nullable = minVal;
            int valueOrDefault = nullable.GetValueOrDefault();
            num1 = num2 >= valueOrDefault & nullable.HasValue ? 1 : 0;
          }
          else
            num1 = 1;
          comboBoxViewModel2.IsEnable = num1 != 0;
          ComboBoxViewModel comboBoxViewModel3 = comboBoxViewModel1;
          int num3;
          if (selectedVal.HasValue)
          {
            int num4 = num;
            nullable = selectedVal;
            int valueOrDefault = nullable.GetValueOrDefault();
            num3 = num4 == valueOrDefault & nullable.HasValue ? 1 : 0;
          }
          else
            num3 = 0;
          comboBoxViewModel3.Selected = num3 != 0;
        }
        else
          comboBoxViewModel1.Selected = !selectedVal.HasValue;
        selected = comboBoxViewModel1.Selected ? comboBoxViewModel1 : selected;
        items.Add(comboBoxViewModel1);
      }
      comboBox.Init<ComboBoxViewModel>(items, selected);
    }

    private void OnFromDaySelected(object sender, ComboBoxViewModel e)
    {
      if (!(this.DataContext is FilterListItemViewModel dataContext))
        return;
      dataContext.DaysFrom = e.Value as int?;
      if (dataContext.DaysFrom.HasValue)
      {
        int? daysTo = dataContext.DaysTo;
        if (daysTo.HasValue)
        {
          daysTo = dataContext.DaysTo;
          int? daysFrom = dataContext.DaysFrom;
          if (daysTo.GetValueOrDefault() < daysFrom.GetValueOrDefault() & daysTo.HasValue & daysFrom.HasValue)
            dataContext.DaysTo = dataContext.DaysFrom;
        }
      }
      ConditionSpanDateItem.SetComboBoxItems(this.ToDayComboBox, ConditionSpanDateItem.ToDaysComboBoxItems, dataContext.DaysTo, dataContext.DaysFrom);
      dataContext.Value = FilterUtils.GetSpanDateValue(dataContext.DaysFrom, dataContext.DaysTo);
      dataContext.Selected = false;
      Utils.FindParent<ConditionEditDialog>((DependencyObject) this)?.OnItemSelected(dataContext);
    }

    private void OnToDaySelected(object sender, ComboBoxViewModel e)
    {
      if (!(this.DataContext is FilterListItemViewModel dataContext))
        return;
      dataContext.DaysTo = e.Value as int?;
      dataContext.Value = FilterUtils.GetSpanDateValue(dataContext.DaysFrom, dataContext.DaysTo);
      dataContext.Selected = false;
      Utils.FindParent<ConditionEditDialog>((DependencyObject) this)?.OnItemSelected(dataContext);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/conditionspandateitem.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          break;
        case 2:
          this.Container = (Grid) target;
          this.Container.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 3:
          this.FromDayComboBox = (CustomComboBox) target;
          break;
        case 4:
          this.ToDayComboBox = (CustomComboBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
