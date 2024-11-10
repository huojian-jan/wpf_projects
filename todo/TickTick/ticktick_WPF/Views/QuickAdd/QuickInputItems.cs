// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.QuickInputItems
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class QuickInputItems : UserControl, IQuickInput, IComponentConnector, IStyleConnector
  {
    internal ListView Items;
    private bool _contentLoaded;

    public QuickInputItems() => this.InitializeComponent();

    public virtual void Move(bool forward)
    {
    }

    public virtual void TrySelectItem(bool exactly = false)
    {
    }

    public virtual bool Filter(string key, List<string> selected = null) => true;

    public virtual bool IsTag() => false;

    protected virtual void OnItemClick(object sender, MouseButtonEventArgs e)
    {
    }

    protected virtual void OnItemEnter(object sender, MouseEventArgs e)
    {
    }

    public virtual bool TrySelectIteWithSpace(string content) => false;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/quickadd/quickinputitems.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.Items = (ListView) target;
      else
        this._contentLoaded = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemEnter);
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
    }
  }
}
