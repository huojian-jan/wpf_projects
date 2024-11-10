// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagLabelControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MainListView;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagLabelControl : UserControl, IComponentConnector
  {
    internal TagLabelControl Root;
    internal Grid Tag;
    internal Border Bd;
    internal Grid DeleteGrid;
    private bool _contentLoaded;

    public TagLabelControl() => this.InitializeComponent();

    private TagDisplayControl FindParent()
    {
      return Utils.FindParent<TagDisplayControl>((DependencyObject) this);
    }

    private void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TagLabelViewModel dataContext))
        return;
      this.FindParent()?.RemoveTag(dataContext.Tag);
    }

    private void OnTagClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TagLabelViewModel dataContext))
        return;
      TagDisplayControl parent1 = this.FindParent();
      if (parent1 == null || !parent1.CanClickTag)
        return;
      ListViewContainer parent2 = Utils.FindParent<ListViewContainer>((DependencyObject) this);
      if (parent2 != null)
        parent2.SelectTagProject(dataContext.Tag);
      else
        App.SelectTagProject(dataContext.Tag);
      parent1.NotifyTagClick(dataContext.Tag);
    }

    private void TagMouseEnter(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is TagLabelViewModel dataContext))
        return;
      this.DeleteGrid.Visibility = dataContext.Editable ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TagMouseLeave(object sender, MouseEventArgs e)
    {
      this.DeleteGrid.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/taglabelcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (TagLabelControl) target;
          break;
        case 2:
          this.Tag = (Grid) target;
          this.Tag.MouseLeave += new MouseEventHandler(this.TagMouseLeave);
          this.Tag.MouseEnter += new MouseEventHandler(this.TagMouseEnter);
          this.Tag.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTagClick);
          break;
        case 3:
          this.Bd = (Border) target;
          break;
        case 4:
          this.DeleteGrid = (Grid) target;
          this.DeleteGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
