// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchProjectItemTemplate
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
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchProjectItemTemplate : UserControl, IComponentConnector
  {
    internal TaskTitleBox Title;
    private bool _contentLoaded;

    public SearchProjectItemTemplate()
    {
      this.InitializeComponent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataChanged);
      this.Title.SetupPreSearchRender();
    }

    private void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is SearchTagAndProjectModel dataContext))
        return;
      this.Title.SetText(dataContext.Name);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/searchprojectitemtemplate.xaml", UriKind.Relative));
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
        this.Title = (TaskTitleBox) target;
      else
        this._contentLoaded = true;
    }
  }
}
