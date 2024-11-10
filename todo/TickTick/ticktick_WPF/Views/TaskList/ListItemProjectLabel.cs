// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.ListItemProjectLabel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class ListItemProjectLabel : StackPanel, IComponentConnector
  {
    private bool _projectNameDown;
    internal ListItemProjectLabel ProjectName;
    internal EmjTextBlock ProjectTitle;
    private bool _contentLoaded;

    public ListItemProjectLabel()
    {
      this.InitializeComponent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.SetValue(ToolTipService.InitialShowDelayProperty, (object) 500);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || !dataContext.ShowProject)
        return;
      this.SetTooltip(dataContext.ProjectName);
    }

    private void OnProjectNameMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._projectNameDown = true;
    }

    private async void OnProjectNameClick(object sender, MouseButtonEventArgs e)
    {
      ListItemProjectLabel itemProjectLabel = this;
      if (itemProjectLabel._projectNameDown && itemProjectLabel.DataContext is DisplayItemModel dataContext && (dataContext.IsTaskOrNote || dataContext.IsItem))
      {
        App.Window.TryShowMainWindow();
        App.Window.NavigateNormalProject(dataContext.ProjectId);
      }
      itemProjectLabel._projectNameDown = false;
    }

    private void OnTitleSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || !dataContext.ShowProject)
        return;
      this.SetTooltip(dataContext.ProjectName);
    }

    private async void SetTooltip(string projectName)
    {
      ListItemProjectLabel itemProjectLabel = this;
      await Task.Delay(100);
      double num = Utils.MeasureStringWidth(projectName, 12.0);
      itemProjectLabel.ToolTip = num - 1.0 > itemProjectLabel.ProjectTitle.ActualWidth ? (object) projectName : (object) (string) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tasklist/listitemprojectlabel.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
        {
          this.ProjectTitle = (EmjTextBlock) target;
          this.ProjectTitle.SizeChanged += new SizeChangedEventHandler(this.OnTitleSizeChanged);
        }
        else
          this._contentLoaded = true;
      }
      else
      {
        this.ProjectName = (ListItemProjectLabel) target;
        this.ProjectName.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnProjectNameClick);
        this.ProjectName.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnProjectNameMouseDown);
      }
    }
  }
}
