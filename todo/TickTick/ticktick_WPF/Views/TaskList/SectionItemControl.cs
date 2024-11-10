// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.SectionItemControl
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
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class SectionItemControl : UserControl, IComponentConnector
  {
    private TaskListControl _parent;
    private bool _mouseDown;
    private bool _addMouseDown;
    internal SectionItemControl Root;
    internal DockPanel Container;
    internal Path OpenIndicator;
    internal Path StarPath;
    internal HoverIconButton AddButton;
    internal TextBlock PostponeText;
    private bool _contentLoaded;

    private DisplayItemModel Model => this.DataContext as DisplayItemModel;

    public SectionItemControl() => this.InitializeComponent();

    private void OnSectionClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._mouseDown)
        return;
      this._mouseDown = false;
      this.ChangeOpenStatus();
    }

    private void ChangeOpenStatus()
    {
      DisplayItemModel model = this.Model;
      if (model?.Section?.Children == null)
        return;
      model.Num = model.Section.Children.Count;
      foreach (DisplayItemModel child in model.Section.Children)
        model.Num += child.GetChildrenModels(true).Count;
      model.IsOpen = !model.IsOpen;
      ISectionList parent = Utils.FindParent<ISectionList>((DependencyObject) this);
      if (parent == null)
        return;
      parent.OnSectionStatusChanged(new SectionStatus()
      {
        SectionId = model.Section.SectionId,
        IsOpen = model.IsOpen
      });
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is DisplayItemModel newValue) || !newValue.IsSection)
        return;
      this.StarPath.Visibility = newValue.Section is PinnedSection ? Visibility.Visible : Visibility.Collapsed;
      if (newValue.InKanban)
      {
        this.Container.Margin = new Thickness(15.0, 0.0, 12.0, 0.0);
        this.SetMouseEnterEvent();
      }
      newValue.Num = newValue.Section.Children.Count;
      foreach (DisplayItemModel child in newValue.Section.Children)
        newValue.Num += child.GetChildrenModels(true).Count;
      if (!(newValue.Section is AssigneeSection section) || !string.IsNullOrEmpty(section.Name))
        return;
      TaskItemLoadHelper.LoadAssigneeName(newValue);
    }

    private void SelectAllMouseUp(DisplayItemModel model)
    {
      if (model == null)
        return;
      bool flag = model.SectionRightActionText == Utils.GetString("DeselectAll");
      if (!model.IsOpen)
      {
        model.IsOpen = true;
        ISectionList parent = Utils.FindParent<ISectionList>((DependencyObject) this);
        if (parent != null)
          parent.OnSectionStatusChanged(new SectionStatus()
          {
            SectionId = model.Section.SectionId,
            IsOpen = model.IsOpen
          });
      }
      Utils.FindParent<ISectionList>((DependencyObject) this)?.SelectOrDeselectAll(model, !flag);
    }

    private void RightTextMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      if ((!dataContext.InBatchSelected ? 0 : (dataContext.Section is HabitSection || dataContext.Section is TimetableSection ? 0 : (!(dataContext.Section.SectionId == "8ac3038d93c54b80a67321b6a03df066") ? 1 : 0))) != 0)
      {
        this.SelectAllMouseUp(dataContext);
      }
      else
      {
        if (!(dataContext.Section is OutdatedSection section) || !section.ShowPostpone || !new CustomerDialog(Utils.GetString("PostponeToTodayTitle"), Utils.GetString("PostponeToTodayMsg"), Utils.GetString("Postpone"), Utils.GetString("Cancel"), (Window) App.Window).ShowDialog().GetValueOrDefault())
          return;
        TaskUtils.PostPoneTasks(Utils.FindParent<TaskListView>((DependencyObject) this));
      }
    }

    private void OnItemMouseDown(object sender, MouseButtonEventArgs e) => this._mouseDown = true;

    private void SetMouseEnterEvent()
    {
      this.MouseEnter -= new MouseEventHandler(this.OnMouseEnter);
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave -= new MouseEventHandler(this.OnMouseLeave);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || !dataContext.IsSection)
        return;
      this.AddButton.Visibility = Visibility.Collapsed;
      this.PostponeText.Visibility = Visibility.Collapsed;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      KanbanColumnView parent = Utils.FindParent<KanbanColumnView>((DependencyObject) this);
      if (parent == null)
        return;
      ColumnViewModel model = parent.GetModel();
      if ((model != null ? (model.CanAdd ? 1 : 0) : 0) == 0 || !(this.DataContext is DisplayItemModel dataContext) || !dataContext.IsSection || dataContext.Section is HabitSection || !(dataContext.Section?.SectionId != "8ac3038d93c54b80a67321b6a03df066") || dataContext.Section is TimetableSection)
        return;
      if (dataContext.Enable)
      {
        this.PostponeText.Visibility = !(dataContext.Section is OutdatedSection section) || !section.ShowPostpone ? Visibility.Collapsed : Visibility.Visible;
        if (this.PostponeText.Visibility == Visibility.Visible)
          this.PostponeText.Text = Utils.GetString("Postpone");
      }
      if (!(dataContext.Section is OutdatedSection) && !dataContext.Enable)
        return;
      this.AddButton.Visibility = Visibility.Visible;
    }

    private void OnAddTaskMouseDown(object sender, MouseButtonEventArgs e)
    {
      KanbanContainer parent = Utils.FindParent<KanbanContainer>((DependencyObject) this);
      if (parent == null || parent.AddingModel != null)
        return;
      this._addMouseDown = true;
    }

    private void OnAddTaskMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (this.DataContext is DisplayItemModel dataContext && this._addMouseDown)
      {
        if (!dataContext.IsOpen)
          this.ChangeOpenStatus();
        Utils.FindParent<ISectionList>((DependencyObject) this)?.OnAddTaskInSectionClick(dataContext);
      }
      this._addMouseDown = false;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tasklist/sectionitemcontrol.xaml", UriKind.Relative));
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
          this.Root = (SectionItemControl) target;
          this.Root.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnItemMouseDown);
          this.Root.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSectionClick);
          this.Root.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
          break;
        case 2:
          this.Container = (DockPanel) target;
          break;
        case 3:
          this.OpenIndicator = (Path) target;
          break;
        case 4:
          this.StarPath = (Path) target;
          break;
        case 5:
          this.AddButton = (HoverIconButton) target;
          break;
        case 6:
          this.PostponeText = (TextBlock) target;
          this.PostponeText.MouseLeftButtonUp += new MouseButtonEventHandler(this.RightTextMouseUp);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
