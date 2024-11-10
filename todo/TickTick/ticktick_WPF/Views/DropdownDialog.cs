// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.DropdownDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views
{
  public class DropdownDialog : UserControl, IComponentConnector
  {
    private List<ListItemData> _itemsSource;
    internal UpDownSelectListView Listview;
    private bool _contentLoaded;

    public event EventHandler<ListItemData> OnItemSelected;

    public List<ListItemData> ItemsSource
    {
      get => this._itemsSource;
      set
      {
        this._itemsSource = value;
        this.Listview.ItemsSource = (IEnumerable) this._itemsSource;
      }
    }

    public DropdownDialog() => this.InitializeComponent();

    private void OnItemClick(bool onEnter, UpDownSelectViewModel e)
    {
      EventHandler<ListItemData> onItemSelected = this.OnItemSelected;
      if (onItemSelected == null)
        return;
      onItemSelected((object) this, e as ListItemData);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/dropdowndialog.xaml", UriKind.Relative));
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
      if (connectionId == 1)
        this.Listview = (UpDownSelectListView) target;
      else
        this._contentLoaded = true;
    }
  }
}
