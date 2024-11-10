// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.CommentEditViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class CommentEditViewModel : BaseViewModel
  {
    private string _content = string.Empty;
    private CommentViewModel _recentComment;
    private string _replyName = string.Empty;
    private bool _showLeftCount;
    private double _recentScrollHeight;
    private bool _showTopLine = true;
    public string TaskId;
    public string ProjectId;
    public bool Init;
    private double _lastItemHeight;

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.OnPropertyChanged(nameof (Content));
        this.OnPropertyChanged("LeftInputCount");
        this.OnPropertyChanged("ShowLeftCount");
        this.OnPropertyChanged("TextPadding");
      }
    }

    public bool ShowLeftCount => this.LeftInputCount >= 924;

    public int LeftInputCount
    {
      get => this._content.Length;
      set
      {
      }
    }

    public Thickness TextPadding
    {
      get
      {
        return !this.ShowLeftCount ? new Thickness(8.0, 10.0, 8.0, 10.0) : new Thickness(8.0, 10.0, 8.0, 24.0);
      }
    }

    public string ReplyName
    {
      get => this._replyName;
      set
      {
        string replyName = this._replyName;
        this._replyName = value;
        this.Content = this.GetReplyContent(this.Content.Trim(), replyName);
      }
    }

    public string ReplyCommentId { get; set; } = string.Empty;

    public CommentViewModel RecentComment
    {
      get => this._recentComment;
      set
      {
        this._recentComment = value;
        this.OnPropertyChanged(nameof (RecentComment));
        this.OnPropertyChanged("ShowRecentComment");
      }
    }

    public double RecentScrollHeight
    {
      get => this._recentScrollHeight;
      set
      {
        this._recentScrollHeight = value;
        this.OnPropertyChanged("ShowRecentComment");
      }
    }

    public bool ShowRecentComment
    {
      get
      {
        return this._lastItemHeight > 0.0 && this.RecentScrollHeight > this._lastItemHeight - 15.0 && this.RecentComment != null && !this.RecentComment.ListModel.Editing();
      }
    }

    public bool ShowTopLine
    {
      get => this._showTopLine;
      set
      {
        this._showTopLine = value;
        this.OnPropertyChanged(nameof (ShowTopLine));
      }
    }

    public void SetLastHeight(double height)
    {
      this._lastItemHeight = height;
      this.OnPropertyChanged("ShowRecentComment");
    }

    public string GetReplyContent(string oldContent = null, string oldName = null)
    {
      string str = Utils.GetString("reply") + " " + oldName + ":";
      if (!string.IsNullOrEmpty(oldContent) && oldContent.StartsWith(str))
        oldContent = oldContent.Remove(0, str.Length);
      return Utils.GetString("reply") + " " + this._replyName + ": " + oldContent;
    }
  }
}
