// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.CommentViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class CommentViewModel : BaseViewModel
  {
    public string Id;
    private readonly CommentModel _model;
    private string _avatarUrl;
    private bool _candelete;
    private bool _editing;
    private string _content;
    private DateTime _createDate;
    private bool _deleted;
    private string _userName;

    public CommentViewModel(CommentModel model)
    {
      this._model = model;
      this._userName = model.userName;
      this._avatarUrl = model.avatarUrl;
      this._candelete = model.candelete;
      this._createDate = model.createdTime ?? DateTime.Now;
      this._content = model.title.Trim();
      this.IsMySelf = model.isMySelf;
      this.Id = model.id;
      this.SetAvatar();
    }

    public CommentListViewModel ListModel { get; set; }

    public CommentModel Model => this._model;

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.OnPropertyChanged(nameof (Content));
      }
    }

    public string UserName
    {
      get => this._userName;
      set
      {
        this._userName = value;
        this.OnPropertyChanged(nameof (UserName));
      }
    }

    public DateTime CreateDate
    {
      get => this._createDate;
      set
      {
        this._createDate = value;
        this.OnPropertyChanged(nameof (CreateDate));
      }
    }

    public bool Deleted
    {
      get => this._deleted;
      set
      {
        this._deleted = value;
        this.OnPropertyChanged(nameof (Deleted));
      }
    }

    public bool Editing
    {
      get => this._editing;
      set
      {
        this._editing = value;
        this.OnPropertyChanged(nameof (Editing));
      }
    }

    public bool CanDelete
    {
      get => this._candelete;
      set
      {
        this._candelete = value;
        this.OnPropertyChanged(nameof (CanDelete));
      }
    }

    public string AvatarUrl
    {
      get => this._avatarUrl;
      set
      {
        this._avatarUrl = value;
        this.OnPropertyChanged(nameof (AvatarUrl));
        this.SetAvatar();
      }
    }

    private async void SetAvatar()
    {
      CommentViewModel commentViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(commentViewModel.AvatarUrl);
      commentViewModel.Avatar = avatarByUrlAsync;
      commentViewModel.OnPropertyChanged("Avatar");
    }

    public BitmapImage Avatar { get; set; }

    public bool IsMySelf { get; }

    public bool CanEdit { get; set; }

    public async Task RemoveComment(CommentViewModel model)
    {
      CommentViewModel commentViewModel = this;
      commentViewModel.ListModel.RemoteComment(model);
      commentViewModel.OnPropertyChanged("Count");
      UtilLog.Info("TaskDetail.DeleteComment taskId " + model._model.taskSid + ", commentId " + model._model.id);
      await CommentService.DeleteComment(model._model);
      await TaskService.SaveCommentCount(model._model.taskSid);
    }

    public bool IsLastOne()
    {
      int? nullable1 = this.ListModel?.CommentList?.IndexOf(this);
      CommentListViewModel listModel = this.ListModel;
      int? nullable2 = listModel != null ? new int?(listModel.Count - 1) : new int?();
      return nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() & nullable1.HasValue == nullable2.HasValue;
    }

    public async void Save()
    {
      this._model.modifiedTime = new DateTime?(DateTime.Now);
      this._model.title = this.Content;
      await CommentService.UpdateComment(this._model);
    }
  }
}
