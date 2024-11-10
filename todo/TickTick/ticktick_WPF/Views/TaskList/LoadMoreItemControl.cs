// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.LoadMoreItemControl
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
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.TaskList;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class LoadMoreItemControl : UserControl, IComponentConnector
  {
    private TaskListView _parent;
    private bool _loading;
    internal LoadingIndicator LoadingIndicator;
    internal TextBlock LoadMoreText;
    private bool _contentLoaded;

    public event EventHandler<string> LoadMore;

    public LoadMoreItemControl()
    {
      this.InitializeComponent();
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.GetParent()?.UnregisterLoadMore(this));
    }

    private void OnDataBinded(object sender, DependencyPropertyChangedEventArgs e)
    {
      this._loading = false;
      this.GetParent()?.RegisterLoadMore(this);
      this.LoadingIndicator.Visibility = Visibility.Collapsed;
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      if (dataContext.InDetail)
        this.LoadMoreText.Text = string.Format(Utils.GetString(dataContext.MoreCount > 1 ? "MoreCompleteTasks" : "MoreCompleteTask"), (object) dataContext.MoreCount);
      else
        this.LoadMoreText.Text = Utils.GetString("LoadMore");
    }

    private TaskListView GetParent()
    {
      return this._parent ?? (this._parent = LoadMoreItemControl.FindParent((DependencyObject) this));
    }

    private static TaskListView FindParent(DependencyObject child)
    {
      if (child == null)
        return (TaskListView) null;
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      if (parent == null)
        return (TaskListView) null;
      return parent is TaskListView ? parent as TaskListView : LoadMoreItemControl.FindParent(parent);
    }

    private void OnLoadMoreClick(object sender, MouseButtonEventArgs e) => this.OnLoadMore(true);

    public void OnLoadMore(bool force)
    {
      if (this._loading)
        return;
      if (this.DataContext is DisplayItemModel dataContext)
      {
        if (!force)
        {
          Section section = dataContext.Section;
          int num1;
          if (section == null)
          {
            num1 = 0;
          }
          else
          {
            int? count = section.Children?.Count;
            int num2 = 5;
            num1 = count.GetValueOrDefault() <= num2 & count.HasValue ? 1 : 0;
          }
          if (num1 != 0)
            return;
        }
        this._loading = true;
        EventHandler<string> loadMore = this.LoadMore;
        if (loadMore != null)
          loadMore((object) this, dataContext.ParentId);
      }
      this.LoadMoreText.Text = Utils.GetString("Loading");
      this.LoadingIndicator.Visibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tasklist/loadmoreitemcontrol.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBinded);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLoadMoreClick);
          break;
        case 3:
          this.LoadingIndicator = (LoadingIndicator) target;
          break;
        case 4:
          this.LoadMoreText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
