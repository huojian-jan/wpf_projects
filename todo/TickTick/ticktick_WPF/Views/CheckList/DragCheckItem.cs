// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CheckList.DragCheckItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.Views.CheckList
{
  public class DragCheckItem : UserControl, IComponentConnector
  {
    internal DragCheckItem ItemControl;
    internal DetailTextBox TitleBox;
    private bool _contentLoaded;

    public DragCheckItem()
    {
      this.InitializeComponent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is CheckItemViewModel dataContext))
        return;
      this.TitleBox.SetText(dataContext.Title);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/checklist/dragcheckitem.xaml", UriKind.Relative));
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
          this.TitleBox = (DetailTextBox) target;
        else
          this._contentLoaded = true;
      }
      else
        this.ItemControl = (DragCheckItem) target;
    }
  }
}
