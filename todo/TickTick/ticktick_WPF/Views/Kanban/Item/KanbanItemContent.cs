// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.Item.KanbanItemContent
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
using System.Windows.Shapes;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Kanban.Item
{
  public class KanbanItemContent : Grid, IComponentConnector
  {
    private KanbanItemController _controller;
    internal KanbanItemContent Root;
    internal Border FoldGrid;
    internal Canvas CheckBox;
    internal Border CheckIconBorder;
    internal Path CheckPath;
    internal TaskTitleBox TitleBox;
    private bool _contentLoaded;

    public KanbanItemContent()
    {
      this.InitializeComponent();
      this.TitleBox.SetWordWrap(true);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      if (this.TitleBox.Text != dataContext.Title)
        this.TitleBox.SetText(dataContext.Title);
      this.TitleBox.SetTextForeground();
      dataContext.SetIcon();
      this.CheckIconBorder.Opacity = dataContext.Status != 0 ? 0.0 : 0.15;
    }

    private void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!(this.DataContext is DisplayItemModel dataContext) || dataContext.IsToggling)
        return;
      dataContext.IsToggling = true;
      this._controller?.OnCheckBoxClick(dataContext);
    }

    private void OnOpenPathClick(object sender, MouseButtonEventArgs e)
    {
      this._controller?.OnOpenPathClick(sender, e);
      e.Handled = true;
    }

    public void SetController(KanbanItemController controller) => this._controller = controller;

    public bool CheckBoxMouseOver() => this.FoldGrid.IsMouseOver || this.CheckBox.IsMouseOver;

    private void OnCheckBoxRightMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this._controller?.OnCheckBoxRightMouseUp((UIElement) this.CheckBox);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/kanban/item/kanbanitemcontent.xaml", UriKind.Relative));
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
          this.Root = (KanbanItemContent) target;
          this.Root.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          break;
        case 2:
          this.FoldGrid = (Border) target;
          this.FoldGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenPathClick);
          break;
        case 3:
          this.CheckBox = (Canvas) target;
          this.CheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
          this.CheckBox.PreviewMouseRightButtonUp += new MouseButtonEventHandler(this.OnCheckBoxRightMouseUp);
          break;
        case 4:
          this.CheckIconBorder = (Border) target;
          break;
        case 5:
          this.CheckPath = (Path) target;
          break;
        case 6:
          this.TitleBox = (TaskTitleBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
