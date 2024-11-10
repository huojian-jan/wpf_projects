// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetSectionItem
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
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetSectionItem : UserControl, IComponentConnector
  {
    internal Path OpenIndicator;
    internal TextBlock PostPoneText;
    private bool _contentLoaded;

    protected WidgetSectionItem(string themeId)
    {
      ThemeUtil.SetTheme(themeId, (FrameworkElement) this);
      this.InitializeComponent();
    }

    public WidgetSectionItem() => this.InitializeComponent();

    private void OnPostPoneMouseUp(object sender, MouseButtonEventArgs e)
    {
      ProjectWidget parent = Utils.FindParent<ProjectWidget>((DependencyObject) this);
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0 || !(this.DataContext is DisplayItemModel))
        return;
      Utils.FindParent<ProjectWidget>((DependencyObject) this)?.PostPoneAll();
    }

    private void OnSectionClick(object sender, MouseButtonEventArgs e)
    {
      ProjectWidget parent = Utils.FindParent<ProjectWidget>((DependencyObject) this);
      if (parent != null && parent.IsLocked || this.PostPoneText.IsMouseOver || (this.DataContext is DisplayItemModel dataContext ? dataContext.Section?.Children : (List<DisplayItemModel>) null) == null)
        return;
      dataContext.IsOpen = !dataContext.IsOpen;
      if (parent == null)
        return;
      parent.OpenOrCloseSection(new SectionStatus()
      {
        SectionId = dataContext.Section.SectionId,
        IsOpen = dataContext.IsOpen
      });
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/widget/widgetsectionitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnSectionClick);
          break;
        case 2:
          this.OpenIndicator = (Path) target;
          break;
        case 3:
          this.PostPoneText = (TextBlock) target;
          this.PostPoneText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnPostPoneMouseUp);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
