// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.SwitchListViewControl
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
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class SwitchListViewControl : StackPanel, ITabControl, IComponentConnector
  {
    private bool? _selectList = new bool?(false);
    private bool? _selectKanban = new bool?(false);
    private bool? _selectTimeline = new bool?(false);
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof (SelectedIndex), typeof (int), typeof (SwitchListViewControl), new PropertyMetadata((object) -10, (PropertyChangedCallback) null));
    internal SwitchListViewControl Root;
    internal Border ListBorder;
    internal Path ListPath;
    internal Border KanbanBorder;
    internal Path KanbanPath;
    internal Border TimelineBorder;
    internal Path TimelinePath;
    private bool _contentLoaded;

    public event EventHandler<string> ViewSelected;

    public int SelectedIndex
    {
      get => (int) this.GetValue(SwitchListViewControl.SelectedIndexProperty);
      set => this.SetValue(SwitchListViewControl.SelectedIndexProperty, (object) value);
    }

    public SwitchListViewControl() => this.InitializeComponent();

    public void SetButtonStatus(bool? selectList, bool? selectKanban, bool? selectTimeline)
    {
      bool? selectList1 = this._selectList;
      bool? nullable1 = selectList;
      if (!(selectList1.GetValueOrDefault() == nullable1.GetValueOrDefault() & selectList1.HasValue == nullable1.HasValue))
      {
        this._selectList = selectList;
        this.ListBorder.Cursor = !this._selectList.HasValue ? Cursors.No : Cursors.Hand;
        this.ListPath.SetResourceReference(Shape.FillProperty, !this._selectList.HasValue ? (object) "BaseColorOpacity10" : (this._selectList.Value ? (object) "PrimaryColor" : (object) "BaseColorOpacity60"));
      }
      bool? selectKanban1 = this._selectKanban;
      bool? nullable2 = selectKanban;
      if (!(selectKanban1.GetValueOrDefault() == nullable2.GetValueOrDefault() & selectKanban1.HasValue == nullable2.HasValue))
      {
        this._selectKanban = selectKanban;
        this.KanbanBorder.Cursor = !this._selectKanban.HasValue ? Cursors.No : Cursors.Hand;
        this.KanbanPath.SetResourceReference(Shape.FillProperty, !this._selectKanban.HasValue ? (object) "BaseColorOpacity10" : (this._selectKanban.Value ? (object) "PrimaryColor" : (object) "BaseColorOpacity60"));
      }
      bool? selectTimeline1 = this._selectTimeline;
      bool? nullable3 = selectTimeline;
      if (selectTimeline1.GetValueOrDefault() == nullable3.GetValueOrDefault() & selectTimeline1.HasValue == nullable3.HasValue)
        return;
      this._selectTimeline = selectTimeline;
      this.TimelineBorder.Cursor = !this._selectTimeline.HasValue ? Cursors.No : Cursors.Hand;
      this.TimelinePath.SetResourceReference(Shape.FillProperty, !this._selectTimeline.HasValue ? (object) "BaseColorOpacity10" : (this._selectTimeline.Value ? (object) "PrimaryColor" : (object) "BaseColorOpacity60"));
    }

    private void SwitchViewClick(object sender, MouseButtonEventArgs e)
    {
      string tag = (sender is FrameworkElement frameworkElement ? frameworkElement.Tag : (object) null) as string;
      e.Handled = true;
      switch (tag)
      {
        case "list":
          if (!this._selectList.HasValue || this._selectList.Value)
            break;
          EventHandler<string> viewSelected1 = this.ViewSelected;
          if (viewSelected1 == null)
            break;
          viewSelected1((object) this, "list");
          break;
        case "kanban":
          if (!this._selectKanban.HasValue || this._selectKanban.Value)
            break;
          EventHandler<string> viewSelected2 = this.ViewSelected;
          if (viewSelected2 == null)
            break;
          viewSelected2((object) this, "kanban");
          break;
        case "timeline":
          if (!this._selectTimeline.HasValue || this._selectTimeline.Value)
            break;
          EventHandler<string> viewSelected3 = this.ViewSelected;
          if (viewSelected3 == null)
            break;
          viewSelected3((object) this, "timeline");
          break;
      }
    }

    public bool HandleTab(bool shift)
    {
      switch (this.SelectedIndex)
      {
        case 0:
          if (shift)
          {
            this.SelectedIndex = -1;
            return false;
          }
          this.SelectedIndex = !this._selectKanban.HasValue ? 2 : 1;
          break;
        case 1:
          this.SelectedIndex = shift ? 0 : 2;
          break;
        case 2:
          if (!shift)
          {
            this.SelectedIndex = -1;
            return false;
          }
          this.SelectedIndex = this._selectKanban.HasValue ? 1 : 0;
          break;
        default:
          this.SelectedIndex = shift ? 2 : 0;
          break;
      }
      return true;
    }

    public bool HandleEnter()
    {
      if (this.SelectedIndex < 0)
        return false;
      switch (this.SelectedIndex)
      {
        case 0:
          if (this._selectList.HasValue && !this._selectList.Value)
          {
            EventHandler<string> viewSelected = this.ViewSelected;
            if (viewSelected != null)
            {
              viewSelected((object) this, "list");
              break;
            }
            break;
          }
          break;
        case 1:
          if (this._selectKanban.HasValue && !this._selectKanban.Value)
          {
            EventHandler<string> viewSelected = this.ViewSelected;
            if (viewSelected != null)
            {
              viewSelected((object) this, "kanban");
              break;
            }
            break;
          }
          break;
        case 2:
          if (this._selectTimeline.HasValue && !this._selectTimeline.Value)
          {
            EventHandler<string> viewSelected = this.ViewSelected;
            if (viewSelected != null)
            {
              viewSelected((object) this, "timeline");
              break;
            }
            break;
          }
          break;
      }
      return true;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      if (this.SelectedIndex < -1)
      {
        if (isUp)
        {
          this.SelectedIndex = -1;
          return false;
        }
        this.SelectedIndex = 0;
        return true;
      }
      this.SelectedIndex = this.SelectedIndex != -1 ? -1 : 0;
      return false;
    }

    public bool LeftRightSelect(bool isLeft)
    {
      switch (this.SelectedIndex)
      {
        case 0:
          this.SelectedIndex = isLeft || !this._selectKanban.HasValue ? 2 : 1;
          break;
        case 1:
          this.SelectedIndex = isLeft ? 0 : 2;
          break;
        case 2:
          this.SelectedIndex = !isLeft || !this._selectKanban.HasValue ? 0 : 1;
          break;
        default:
          this.SelectedIndex = isLeft ? 2 : 0;
          break;
      }
      return true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/switchlistviewcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (SwitchListViewControl) target;
          break;
        case 2:
          this.ListBorder = (Border) target;
          this.ListBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchViewClick);
          break;
        case 3:
          this.ListPath = (Path) target;
          break;
        case 4:
          this.KanbanBorder = (Border) target;
          this.KanbanBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchViewClick);
          break;
        case 5:
          this.KanbanPath = (Path) target;
          break;
        case 6:
          this.TimelineBorder = (Border) target;
          this.TimelineBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchViewClick);
          break;
        case 7:
          this.TimelinePath = (Path) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
