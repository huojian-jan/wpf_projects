// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.TimeFilterDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class TimeFilterDialog : UserControl, IComponentConnector
  {
    private List<ListItemData> _itemsSource;
    internal UpDownSelectListView Listview;
    internal SelectTimeSpanDialog SelectTimeDialog;
    private bool _contentLoaded;

    public TimeFilterDialog() => this.InitializeComponent();

    public List<ListItemData> ItemsSource
    {
      get => this._itemsSource;
      set
      {
        this._itemsSource = value;
        this.Listview.ItemsSource = (IEnumerable) this._itemsSource;
      }
    }

    public event EventHandler<DateFilterData> OnFilterSelect;

    public event EventHandler OnCancel;

    public event EventHandler OnEndEditDate;

    public void SetStartDate(DateTime? startDate) => this.SelectTimeDialog.SetStartDate(startDate);

    public void SetEndDate(DateTime? endDate) => this.SelectTimeDialog.SetEndDate(endDate);

    private async void OnItemClick(bool onEnter, UpDownSelectViewModel model)
    {
      TimeFilterDialog sender = this;
      if (model == null)
        return;
      if (model is ListItemData listItemData && (DateFilter) listItemData.Key != DateFilter.Custom)
      {
        EventHandler<DateFilterData> onFilterSelect = sender.OnFilterSelect;
        if (onFilterSelect == null)
          return;
        onFilterSelect((object) sender, new DateFilterData()
        {
          Type = (DateFilter) listItemData.Key
        });
      }
      else
      {
        sender.Listview.Visibility = Visibility.Collapsed;
        sender.SelectTimeDialog.Visibility = Visibility.Visible;
        // ISSUE: reference to a compiler-generated method
        sender.SelectTimeDialog.OnEndEdit += new EventHandler(sender.\u003COnItemClick\u003Eb__16_0);
        // ISSUE: reference to a compiler-generated method
        sender.SelectTimeDialog.Cancel += new EventHandler(sender.\u003COnItemClick\u003Eb__16_1);
        // ISSUE: reference to a compiler-generated method
        sender.SelectTimeDialog.OnSpanSelect += new EventHandler<SelectTimeSpanViewModel>(sender.\u003COnItemClick\u003Eb__16_2);
        await Task.Delay(10);
        HwndHelper.SetFocus((UIElement) sender.SelectTimeDialog);
        sender.SelectTimeDialog.FocusBox.Focus();
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/timefilterdialog.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.SelectTimeDialog = (SelectTimeSpanDialog) target;
        else
          this._contentLoaded = true;
      }
      else
        this.Listview = (UpDownSelectListView) target;
    }
  }
}
