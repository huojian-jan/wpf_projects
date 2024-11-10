// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.AddCommentControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.MarkDown;

#nullable disable
namespace ticktick_WPF.Views
{
  public class AddCommentControl : UserControl, IComponentConnector
  {
    private readonly List<Mention> _selectedMentionList = new List<Mention>();
    private CommentEditViewModel _commentEditViewModel;
    private List<Mention> _mentionList;
    private int _selectedAtLabelIndex = -1;
    internal TextBlock MoreText;
    internal EmojiEditor ContentTxt;
    internal TextBlock ExpandOrCollapseBtn;
    internal TextBox CommentInputTxt;
    internal Popup AtPopup;
    internal ScrollViewer AtPopupScrollViewer;
    internal StackPanel AtPopupStackPanel;
    private bool _contentLoaded;

    public AddCommentControl(CommentEditViewModel commentEditViewModel)
    {
      AddCommentControl addCommentControl = this;
      this.InitializeComponent();
      this.Loaded += (RoutedEventHandler) ((sender, e) =>
      {
        addCommentControl._commentEditViewModel = commentEditViewModel;
        addCommentControl.DataContext = (object) addCommentControl._commentEditViewModel;
        addCommentControl.InitView();
      });
      this.ContentTxt.EditBox.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    public CommentEditViewModel EditModel
    {
      get => this._commentEditViewModel;
      set => this._commentEditViewModel = value;
    }

    public event EventHandler CommentGotFocus;

    public event EventHandler<CommentModel> OnCommentAdded;

    private async Task InitMentionList(string projectId)
    {
      List<ShareUserModel> shareUsers = await AvatarHelper.GetProjectUsersAsync(projectId);
      UserInfoModel userInfo = await UserManager.GetUserInfo();
      this._mentionList = new List<Mention>();
      if (shareUsers == null)
        shareUsers = (List<ShareUserModel>) null;
      else if (shareUsers.Count <= 0)
      {
        shareUsers = (List<ShareUserModel>) null;
      }
      else
      {
        using (List<ShareUserModel>.Enumerator enumerator = shareUsers.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            ShareUserModel current = enumerator.Current;
            if (current.isAccept && current.userCode != userInfo?.userCode)
            {
              string str = string.Empty;
              if (!string.IsNullOrEmpty(current.username))
                str = current.username;
              if (!string.IsNullOrEmpty(current.displayName))
                str = current.displayName;
              long num = current.userId ?? -1L;
              if (num != -1L)
                this._mentionList.Add(new Mention()
                {
                  userId = num,
                  atLabel = str
                });
            }
          }
          shareUsers = (List<ShareUserModel>) null;
        }
      }
    }

    private void InitView()
    {
      this.CommentInputTxt.TextChanged += (TextChangedEventHandler) ((sender, e) =>
      {
        if (sender == null || !(sender is TextBox textBox2) || this._commentEditViewModel == null)
          return;
        string text = textBox2.Text;
        this._commentEditViewModel.Content = text;
        if (text.EndsWith("@"))
          this.TryShowAtDialog();
        else
          this.CloseAtDialog();
      });
      this.CommentInputTxt.PreviewKeyDown += (KeyEventHandler) ((sender, e) =>
      {
        switch (e.Key)
        {
          case Key.Return:
            if (!this.AtPopup.IsOpen)
            {
              if (Utils.IfShiftPressed())
                break;
              if (this.CommentInputTxt.Text.Length <= 1024)
                this.AddComment();
              e.Handled = true;
              break;
            }
            this.SelectAtLabel();
            e.Handled = true;
            break;
          case Key.Escape:
            if (!this.AtPopup.IsOpen)
              break;
            this.AtPopup.IsOpen = false;
            e.Handled = true;
            break;
          case Key.Up:
            this.MoveUp();
            break;
          case Key.Down:
            this.MoveDown();
            break;
        }
      });
      this.MoreText.MouseLeftButtonUp += (MouseButtonEventHandler) ((sender, e) =>
      {
        this._commentEditViewModel.RecentScrollHeight = 0.0;
        Utils.FindParent<TaskDetailView>((DependencyObject) this)?.ExpandComment();
      });
    }

    private void AddComment()
    {
      if (string.IsNullOrEmpty(this.CommentInputTxt.Text.Trim()) || this._commentEditViewModel.LeftInputCount < 0)
        return;
      string str1 = this.CommentInputTxt.Text;
      string str2 = string.Empty;
      if (!string.IsNullOrEmpty(this._commentEditViewModel.ReplyName) && str1.Contains(this._commentEditViewModel.ReplyName))
      {
        str1 = str1.Substring(this._commentEditViewModel.GetReplyContent().Length);
        str2 = this._commentEditViewModel.ReplyCommentId;
      }
      EventHandler<CommentModel> onCommentAdded = this.OnCommentAdded;
      if (onCommentAdded != null)
        onCommentAdded((object) this, new CommentModel()
        {
          title = str1,
          mentions = this._selectedMentionList,
          replyCommentId = str2,
          projectSid = this._commentEditViewModel.ProjectId,
          taskSid = this._commentEditViewModel.TaskId,
          isMySelf = true
        });
      this.CommentInputTxt.Text = string.Empty;
      this._commentEditViewModel.RecentScrollHeight = 0.0;
    }

    private void SelectAtLabel()
    {
      if (this._selectedAtLabelIndex < 0 || this._selectedAtLabelIndex >= this.AtPopupStackPanel.Children.Count || !(this.AtPopupStackPanel.Children[this._selectedAtLabelIndex] is TextBox child))
        return;
      this.SelectAtLabel((Mention) child.Tag);
    }

    private async void CloseAtDialog()
    {
      if (this.AtPopup.IsOpen)
        this.AtPopup.IsOpen = false;
      await Task.Delay(500);
    }

    private async void TryShowAtDialog()
    {
      if (this._mentionList == null || this._mentionList.Count == 0)
        await this.InitMentionList(this.EditModel.ProjectId);
      if (this._mentionList == null || this._mentionList.Count <= 0)
        return;
      this.AtPopupStackPanel.Children.Clear();
      foreach (Mention mention in this._mentionList)
        this.AtPopupStackPanel.Children.Add((UIElement) this.AddAtItem(mention));
      this.AtPopup.PlacementTarget = (UIElement) this.CommentInputTxt;
      Popup atPopup1 = this.AtPopup;
      Rect fromCharacterIndex = this.CommentInputTxt.GetRectFromCharacterIndex(this.CommentInputTxt.CaretIndex);
      double num = fromCharacterIndex.Y + 10.0;
      atPopup1.VerticalOffset = num;
      Popup atPopup2 = this.AtPopup;
      fromCharacterIndex = this.CommentInputTxt.GetRectFromCharacterIndex(this.CommentInputTxt.CaretIndex);
      double x = fromCharacterIndex.X;
      atPopup2.HorizontalOffset = x;
      if (this.AtPopupStackPanel.Children.Count != 0)
      {
        if (this.AtPopupStackPanel.Children[0] is TextBox child)
          child.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
        this._selectedAtLabelIndex = 0;
        this.AtPopupScrollViewer.ScrollToTop();
      }
      this.AtPopup.IsOpen = true;
    }

    public async void FocusInput()
    {
      if (this.CommentInputTxt.IsFocused && !(this.CommentInputTxt.Text != this.EditModel?.Content))
        return;
      this.CommentInputTxt.Text = this.EditModel?.Content ?? string.Empty;
      await Task.Delay(20);
      this.CommentInputTxt.Focus();
      this.CommentInputTxt.CaretIndex = this.CommentInputTxt.Text.Length;
    }

    public void ClearText() => this.CommentInputTxt.Text = string.Empty;

    private void MoveDown()
    {
      if (!this.AtPopup.IsOpen || this.AtPopupStackPanel.Children.Count <= 0 || this._selectedAtLabelIndex >= this.AtPopupStackPanel.Children.Count - 1)
        return;
      AddCommentControl.SetUnSelected(this.AtPopupStackPanel.Children[this._selectedAtLabelIndex] as TextBox);
      AddCommentControl.SetSelected(this.AtPopupStackPanel.Children[this._selectedAtLabelIndex + 1] as TextBox);
      ++this._selectedAtLabelIndex;
    }

    private void MoveUp()
    {
      if (!this.AtPopup.IsOpen || this.AtPopupStackPanel.Children.Count <= 0 || this._selectedAtLabelIndex <= 0)
        return;
      AddCommentControl.SetUnSelected(this.AtPopupStackPanel.Children[this._selectedAtLabelIndex] as TextBox);
      AddCommentControl.SetSelected(this.AtPopupStackPanel.Children[this._selectedAtLabelIndex - 1] as TextBox);
      --this._selectedAtLabelIndex;
    }

    private static void SetUnSelected(TextBox textbox)
    {
      textbox.Background = (Brush) ThemeUtil.GetColor("WindowBackgroundPrimary");
      textbox.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity100_80");
    }

    private static void SetSelected(TextBox textbox)
    {
      textbox.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
    }

    private TextBox AddAtItem(Mention mention)
    {
      TextBox textBox1 = new TextBox();
      textBox1.Style = (Style) Application.Current?.FindResource((object) "TagPopupItemTextBoxStyle");
      textBox1.MouseEnter += (MouseEventHandler) ((sender, e) =>
      {
        if (this.AtPopupStackPanel.Children.Count != 0)
        {
          for (int index = 0; index < this.AtPopupStackPanel.Children.Count; ++index)
          {
            if (this.AtPopupStackPanel.Children[index] is TextBox child2)
            {
              child2.Background = (Brush) ThemeUtil.GetColor("WindowBackgroundPrimary");
              child2.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity100_80");
            }
          }
        }
        if (!(sender is TextBox textBox3))
          return;
        textBox3.Background = (Brush) ThemeUtil.GetColor("BaseColorOpacity5");
        textBox3.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
      });
      textBox1.MouseLeave += (MouseEventHandler) ((sender, e) =>
      {
        if (!(sender is TextBox textBox5))
          return;
        textBox5.Background = (Brush) ThemeUtil.GetColor("WindowBackgroundPrimary");
      });
      textBox1.MouseLeftButtonUp += (MouseButtonEventHandler) ((sender, e) =>
      {
        if (!(sender is TextBox textBox7))
          return;
        this.SelectAtLabel((Mention) textBox7.Tag);
      });
      textBox1.Tag = (object) mention;
      textBox1.Text = mention.atLabel;
      textBox1.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity100_80");
      return textBox1;
    }

    private void SelectAtLabel(Mention selected)
    {
      this._selectedMentionList.Add(new Mention()
      {
        userId = selected.userId,
        atLabel = "@" + selected.atLabel
      });
      this._commentEditViewModel.Content = this._commentEditViewModel.Content + selected.atLabel + " ";
      this.AtPopup.IsOpen = false;
      this.CommentInputTxt.Text = this._commentEditViewModel.Content;
      this.CommentInputTxt.Focus();
      this.CommentInputTxt.CaretIndex = this._commentEditViewModel.Content.Length;
    }

    private void expandOrCollapseBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.ToggleExpandOrCollapse();
    }

    private void ToggleExpandOrCollapse()
    {
      if (this.ExpandOrCollapseBtn.Text == Utils.GetString("expand"))
        this.ExpandOrCollapseBtn.Text = Utils.GetString("Collapse");
      else
        this.ExpandOrCollapseBtn.Text = Utils.GetString("expand");
    }

    private void contentTxt_TextChanged(object sender, TextChangedEventArgs e)
    {
    }

    private void OnCommentGotFocus(object sender, RoutedEventArgs e)
    {
      EventHandler commentGotFocus = this.CommentGotFocus;
      if (commentGotFocus == null)
        return;
      commentGotFocus(sender, (EventArgs) e);
    }

    public bool IsEditing() => this.CommentInputTxt.Text.Length > 0;

    public void SetModel(CommentEditViewModel editCommentModel)
    {
      this._commentEditViewModel = editCommentModel;
      this.DataContext = (object) this._commentEditViewModel;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/addcommentcontrol.xaml", UriKind.Relative));
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
          this.MoreText = (TextBlock) target;
          break;
        case 2:
          this.ContentTxt = (EmojiEditor) target;
          break;
        case 3:
          this.ExpandOrCollapseBtn = (TextBlock) target;
          this.ExpandOrCollapseBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.expandOrCollapseBtn_MouseLeftButtonUp);
          break;
        case 4:
          this.CommentInputTxt = (TextBox) target;
          this.CommentInputTxt.GotFocus += new RoutedEventHandler(this.OnCommentGotFocus);
          break;
        case 5:
          this.AtPopup = (Popup) target;
          break;
        case 6:
          this.AtPopupScrollViewer = (ScrollViewer) target;
          break;
        case 7:
          this.AtPopupStackPanel = (StackPanel) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
