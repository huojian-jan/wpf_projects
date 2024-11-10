// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.CommentListViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class CommentListViewModel : BaseViewModel
  {
    private List<CommentViewModel> _commentList;
    private ObservableCollection<CommentViewModel> _displayList;
    private bool _showExpand;
    private ViewMode _viewMode;

    public CommentListViewModel(
      List<CommentViewModel> commentList,
      CommentEditViewModel editComment,
      bool canEdit = true)
    {
      this._viewMode = ViewMode.Recent;
      this._commentList = commentList.OrderBy<CommentViewModel, DateTime>((Func<CommentViewModel, DateTime>) (c => c.CreateDate)).ToList<CommentViewModel>();
      this._displayList = new ObservableCollection<CommentViewModel>();
      if (this._commentList.Count > 0)
      {
        foreach (CommentViewModel comment in this._commentList)
        {
          comment.ListModel = this;
          comment.CanEdit = canEdit;
        }
      }
      this.SetRecentDisplayList();
      this.ShowExpand = commentList.Count > 5;
      this.EditComment = editComment;
    }

    public CommentEditViewModel EditComment { get; }

    public bool ShowExpand
    {
      get => this._showExpand;
      set
      {
        this._showExpand = value;
        this.OnPropertyChanged(nameof (ShowExpand));
      }
    }

    public ViewMode ViewMode
    {
      get => this._viewMode;
      set
      {
        this._viewMode = value;
        this.OnPropertyChanged(nameof (ViewMode));
      }
    }

    public ObservableCollection<CommentViewModel> DisplayList
    {
      get => this._displayList;
      set
      {
        this._displayList = value;
        this.OnPropertyChanged(nameof (DisplayList));
      }
    }

    public List<CommentViewModel> CommentList
    {
      get => this._commentList;
      set
      {
        this._commentList = value;
        this.OnPropertyChanged("Count");
        this.OnPropertyChanged(nameof (CommentList));
        CommentEditViewModel editComment = this.EditComment;
        List<CommentViewModel> commentList = this._commentList;
        CommentViewModel comment = (commentList != null ? (__nonvirtual (commentList.Count) > 0 ? 1 : 0) : 0) != 0 ? this._commentList[this._commentList.Count - 1] : (CommentViewModel) null;
        editComment.RecentComment = comment;
      }
    }

    public int Count => this._commentList.Count;

    private void SetRecentDisplayList()
    {
      ObservableCollection<CommentViewModel> observableCollection = new ObservableCollection<CommentViewModel>();
      for (int index = Math.Max(0, this._commentList.Count - 5); index < this._commentList.Count; ++index)
        observableCollection.Add(this._commentList[index]);
      this.DisplayList = observableCollection;
    }

    private void SetFullDisplayList()
    {
      ObservableCollection<CommentViewModel> observableCollection = new ObservableCollection<CommentViewModel>();
      for (int index = 0; index < this._commentList.Count; ++index)
        observableCollection.Add(this._commentList[index]);
      this.DisplayList = observableCollection;
    }

    public void ExpandOrCollapse()
    {
      if (this.ViewMode == ViewMode.Full)
      {
        this.ViewMode = ViewMode.Recent;
        this.SetRecentDisplayList();
      }
      else
      {
        if (this.ViewMode != ViewMode.Recent)
          return;
        this.ViewMode = ViewMode.Full;
        this.SetFullDisplayList();
      }
    }

    public void Expand()
    {
      if (this.ViewMode == ViewMode.Full)
        return;
      this.ViewMode = ViewMode.Full;
      this.SetFullDisplayList();
    }

    public void AddItem(CommentViewModel newComment)
    {
      this._commentList.Add(newComment);
      if (!this._showExpand && this._commentList.Count > 5 && this.ViewMode == ViewMode.Recent)
        this.ShowExpand = true;
      if (this.DisplayList.Count >= 5 && this.ViewMode == ViewMode.Recent)
        this.DisplayList.RemoveAt(0);
      this.DisplayList.Add(newComment);
      this.OnPropertyChanged("Count");
    }

    public bool CheckComments(List<CommentModel> localComments)
    {
      foreach (CommentModel localComment in localComments)
      {
        CommentModel comment = localComment;
        CommentViewModel commentViewModel = this._commentList.FirstOrDefault<CommentViewModel>((Func<CommentViewModel, bool>) (c => c.Id == comment.id));
        if (commentViewModel == null)
          return false;
        if (commentViewModel.Content != comment.title.Trim())
          commentViewModel.Content = comment.title.Trim();
      }
      return true;
    }

    public void RemoteComment(CommentViewModel model)
    {
      this._commentList.Remove(model);
      this.DisplayList.Remove(model);
      if (this.ViewMode == ViewMode.Recent)
        this.SetRecentDisplayList();
      if (this._showExpand && this._commentList.Count <= 5)
        this.ShowExpand = false;
      this.OnPropertyChanged("Count");
      this.OnPropertyChanged("CommentList");
      CommentEditViewModel editComment = this.EditComment;
      List<CommentViewModel> commentList = this._commentList;
      // ISSUE: explicit non-virtual call
      CommentViewModel comment = (commentList != null ? (__nonvirtual (commentList.Count) > 0 ? 1 : 0) : 0) != 0 ? this._commentList[this._commentList.Count - 1] : (CommentViewModel) null;
      editComment.RecentComment = comment;
    }

    public bool Editing()
    {
      return this._commentList.Any<CommentViewModel>((Func<CommentViewModel, bool>) (m => m.Editing));
    }
  }
}
