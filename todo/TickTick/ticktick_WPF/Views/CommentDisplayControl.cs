// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CommentDisplayControl
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
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public class CommentDisplayControl : UserControl, IComponentConnector
  {
    private CommentListViewModel _model;
    internal ListView commentListView;
    private bool _contentLoaded;

    public CommentDisplayControl(CommentListViewModel model)
    {
      this.InitializeComponent();
      this._model = model;
      this.DataContext = (object) model;
    }

    public CommentListViewModel Model
    {
      get => this._model;
      set => this._model = value;
    }

    public event EventHandler<CommentViewModel> ShowAddComment;

    private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this._model.ExpandOrCollapse();
      this.ScrollToBottom();
    }

    public void ScrollToBottom()
    {
      this.commentListView.SelectedIndex = this.commentListView.Items.Count - 1;
      this.commentListView.ScrollIntoView(this.commentListView.SelectedItem);
    }

    private void TryShowAddComment(object sender, CommentViewModel e)
    {
      EventHandler<CommentViewModel> showAddComment = this.ShowAddComment;
      if (showAddComment == null)
        return;
      showAddComment((object) null, e);
    }

    public void AddNewItem(CommentModel comment)
    {
      this._model.AddItem(new CommentViewModel(comment)
      {
        CanEdit = true,
        ListModel = this._model
      });
    }

    public void ExpandComment() => this._model?.Expand();

    public void SetModel(CommentListViewModel model)
    {
      this._model = model;
      this.DataContext = (object) model;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/commentdisplaycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.Label_MouseLeftButtonUp);
        else
          this._contentLoaded = true;
      }
      else
        this.commentListView = (ListView) target;
    }
  }
}
