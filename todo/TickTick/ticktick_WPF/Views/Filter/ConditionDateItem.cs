// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ConditionDateItem
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
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ConditionDateItem : Grid, IComponentConnector
  {
    private bool _mouseDown;
    internal Grid Container;
    internal TextBlock BeforeText;
    internal TextBox InputNum;
    internal TextBlock AfterText;
    private bool _contentLoaded;

    public ConditionDateItem() => this.InitializeComponent();

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._mouseDown)
        return;
      this._mouseDown = false;
      if (!(this.DataContext is FilterListItemViewModel dataContext))
        return;
      Utils.FindParent<ConditionEditDialog>((DependencyObject) this)?.OnItemSelected(dataContext);
      if (!dataContext.Selected)
        return;
      this.InputNum.Focus();
      this.InputNum.SelectAll();
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is FilterListItemViewModel dataContext))
        return;
      List<string> list = ((IEnumerable<string>) Utils.GetString(dataContext.DisplayType.ToString()).Replace("{0}", "|").Split('|')).ToList<string>();
      list.Remove(string.Empty);
      if (list.Count == 2)
      {
        this.BeforeText.Text = list[0];
        this.AfterText.Text = list[1].Replace("{1}", "th");
      }
      else
      {
        if (list.Count <= 0)
          return;
        this.BeforeText.Text = string.Empty;
        this.BeforeText.Margin = new Thickness(6.0, 0.0, 0.0, 0.0);
        this.AfterText.Text = list[0];
      }
    }

    private async void OnIntervalTextChanged(object sender, TextChangedEventArgs e)
    {
      ConditionDateItem child = this;
      int result;
      if (!int.TryParse(child.InputNum.Text, out result))
        return;
      if (result > 30)
      {
        child.InputNum.Text = "30";
        child.InputNum.SelectAll();
        result = 30;
      }
      else if (result <= 0)
      {
        child.InputNum.Text = "1";
        child.InputNum.SelectAll();
        result = 1;
      }
      if (!(child.DataContext is FilterListItemViewModel dataContext))
        return;
      dataContext.NDaysValue = result;
      switch (dataContext.DisplayType)
      {
        case FilterItemDisplayType.NextNDays:
          dataContext.Value = (object) (result.ToString() + "days");
          break;
        case FilterItemDisplayType.NDaysLater:
          dataContext.Value = (object) (result.ToString() + "dayslater");
          break;
        case FilterItemDisplayType.AfterNDays:
          dataContext.Value = (object) (result.ToString() + "daysfromtoday");
          break;
        case FilterItemDisplayType.NDaysAgo:
          dataContext.Value = (object) ("-" + result.ToString() + "daysfromtoday");
          break;
      }
      Utils.FindParent<ConditionEditDialog>((DependencyObject) child)?.ValueChanged();
    }

    private void HandleNumberInput(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length < 1 || char.IsDigit(e.Text, e.Text.Length - 1))
        return;
      e.Handled = true;
    }

    private void OnInputFocused(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is FilterListItemViewModel dataContext) || dataContext.Selected)
        return;
      Utils.FindParent<ConditionEditDialog>((DependencyObject) this)?.OnItemSelected(dataContext);
    }

    private void OnItemDown(object sender, MouseButtonEventArgs e) => this._mouseDown = true;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/conditiondateitem.xaml", UriKind.Relative));
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
          this.Container.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnItemDown);
          break;
        case 3:
          this.BeforeText = (TextBlock) target;
          break;
        case 4:
          this.InputNum = (TextBox) target;
          this.InputNum.PreviewTextInput += new TextCompositionEventHandler(this.HandleNumberInput);
          this.InputNum.TextChanged += new TextChangedEventHandler(this.OnIntervalTextChanged);
          this.InputNum.GotFocus += new RoutedEventHandler(this.OnInputFocused);
          break;
        case 5:
          this.AfterText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
