// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.InsertSummaryWindow
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
namespace ticktick_WPF.Views.Summary
{
  public class InsertSummaryWindow : MyWindow, IComponentConnector
  {
    internal SummaryControl SummaryControl;
    private bool _contentLoaded;

    public event EventHandler<string> InsertSummary;

    public InsertSummaryWindow()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.SummaryControl.SetInsertMode();
      this.SummaryControl.LoadData();
      this.SummaryControl.InsertSummary += new EventHandler<string>(this.OnInsertSummary);
    }

    private void OnInsertSummary(object sender, string e)
    {
      EventHandler<string> insertSummary = this.InsertSummary;
      if (insertSummary != null)
        insertSummary((object) this, e + "\r\n");
      this.Close();
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e) => this.Close();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/summary/insertsummarywindow.xaml", UriKind.Relative));
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
          this.SummaryControl = (SummaryControl) target;
        else
          this._contentLoaded = true;
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
    }
  }
}
