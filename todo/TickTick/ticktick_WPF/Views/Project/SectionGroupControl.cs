// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.SectionGroupControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.ProjectList;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class SectionGroupControl : UserControl, IComponentConnector
  {
    internal SectionGroupControl Root;
    internal Grid SectionGroupGrid;
    internal Path OpenIndicator;
    private bool _contentLoaded;

    public SectionGroupControl() => this.InitializeComponent();

    private void OnSectionGroupClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      PtfAllViewModel dataContext = this.DataContext as PtfAllViewModel;
    }

    private ProjectListView GetParent()
    {
      return Utils.FindParent<ProjectListView>((DependencyObject) this);
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/sectiongroupcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (SectionGroupControl) target;
          this.Root.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSectionGroupClick);
          break;
        case 2:
          this.SectionGroupGrid = (Grid) target;
          this.SectionGroupGrid.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
          break;
        case 3:
          this.OpenIndicator = (Path) target;
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnAddClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
