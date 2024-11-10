// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchOperationDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchOperationDialog : MyWindow, IOkCancelWindow, IComponentConnector
  {
    internal SearchOperationControl SearchControl;
    private bool _contentLoaded;

    public SearchOperationDialog()
    {
      this.InitializeComponent();
      this.Loaded += (RoutedEventHandler) ((o, e) =>
      {
        this.Showing = true;
        this.SearchControl.FocusInput();
      });
    }

    public bool Showing { get; set; }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Showing = false;
      base.OnClosing(e);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
    }

    public void OnCancel() => this.Close();

    public void Ok()
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/searchoperationdialog.xaml", UriKind.Relative));
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
        this.SearchControl = (SearchOperationControl) target;
      else
        this._contentLoaded = true;
    }
  }
}
