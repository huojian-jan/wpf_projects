// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchFilterDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Filter;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchFilterDialog : Window, IOkCancelWindow, IComponentConnector
  {
    internal NormalFilterControl NormalFilterControl;
    private bool _contentLoaded;

    public SearchFilterDialog()
    {
      this.InitializeComponent();
      this.NormalFilterControl.ViewModel = new NormalFilterViewModel();
      this.NormalFilterControl.SetUseInSearchFilter();
    }

    public SearchFilterDialog(SearchFilterModel searchModel)
    {
      this.InitializeComponent();
      this.NormalFilterControl.ViewModel = new NormalFilterViewModel();
      this.NormalFilterControl.SetUseInSearchFilter();
    }

    public void OnCancel() => this.Close();

    public void Ok()
    {
    }

    private void OnSearchClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/searchfilterdialog.xaml", UriKind.Relative));
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
          this.NormalFilterControl = (NormalFilterControl) target;
          break;
        case 2:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSearchClick);
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
