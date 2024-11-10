// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.AddProjectGroupDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
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
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views
{
  public class AddProjectGroupDialog : Window, IOkCancelWindow, IComponentConnector
  {
    private ProjectGroupModel _originalProjectGroupModel;
    private string _teamId;
    private readonly bool _isAdd;
    internal AddProjectGroupDialog Root;
    internal Grid EmojiSelectGrid;
    internal Grid ProjectPathGrid;
    internal Path ProjectPath;
    internal Path SetEmojiIcon;
    internal EmjTextBlock EmojiText;
    internal EscPopup EmojiSelectPopup;
    internal EmojiSelectControl EmojiSelector;
    internal EmojiEditor FolderNameTextBox;
    internal TextBlock FolderNameRepeatTextBlock;
    private bool _contentLoaded;

    private AddProjectGroupDialog() => this.InitializeComponent();

    public AddProjectGroupDialog(string id, string teamId = "")
    {
      this.InitializeComponent();
      this._isAdd = true;
      this._teamId = teamId;
      this._originalProjectGroupModel = new ProjectGroupModel()
      {
        sortType = Constants.SortType.project.ToString(),
        SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.project, false)
      };
      this.CheckEmojiStart(this._originalProjectGroupModel);
      this.Root.DataContext = (object) this._originalProjectGroupModel;
      this.Title = Utils.GetString("AddFolder");
    }

    public AddProjectGroupDialog(ProjectGroupModel model)
    {
      this.InitializeComponent();
      this._isAdd = false;
      if (model == null)
        return;
      this._originalProjectGroupModel = model.Clone();
      this.CheckEmojiStart(this._originalProjectGroupModel);
      this.Root.DataContext = (object) this._originalProjectGroupModel;
      this.Title = Utils.GetString("EditFolder");
    }

    public event EventHandler<ProjectGroupModel> ProjectGroupEdit;

    public event EventHandler Cancel;

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Owner?.Activate();
      base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
      EventHandler cancel = this.Cancel;
      if (cancel != null)
        cancel((object) null, (EventArgs) null);
      base.OnClosed(e);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    protected override void OnContentRendered(EventArgs e)
    {
      base.OnContentRendered(e);
      Keyboard.Focus((IInputElement) this.FolderNameTextBox);
      this.FolderNameTextBox.FocusEnd();
    }

    protected override void OnDeactivated(EventArgs e)
    {
      this.Topmost = false;
      base.OnDeactivated(e);
    }

    protected override async void OnActivated(EventArgs e)
    {
      AddProjectGroupDialog projectGroupDialog = this;
      Keyboard.Focus((IInputElement) projectGroupDialog.FolderNameTextBox);
      projectGroupDialog.FolderNameTextBox.FocusEnd();
      projectGroupDialog.Topmost = true;
      await Task.Delay(1);
      projectGroupDialog.Topmost = false;
      // ISSUE: reference to a compiler-generated method
      projectGroupDialog.\u003C\u003En__0(e);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this._originalProjectGroupModel.name = this.FolderNameTextBox.Text;
      this.SaveProjectGroup();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnFolderNameTextBoxKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      this._originalProjectGroupModel.name = this.FolderNameTextBox.Text;
      this.SaveProjectGroup();
    }

    private async void SaveProjectGroup()
    {
      AddProjectGroupDialog sender = this;
      if (!string.IsNullOrEmpty(sender._originalProjectGroupModel.name))
      {
        sender._originalProjectGroupModel.name = sender.EmojiText.Text.Trim() + sender._originalProjectGroupModel.name;
        sender.FolderNameRepeatTextBlock.Visibility = Visibility.Collapsed;
        if (sender._isAdd)
        {
          sender._originalProjectGroupModel.id = Utils.GetGuid();
          sender._originalProjectGroupModel.userId = Utils.GetCurrentUserIdInt();
          sender._originalProjectGroupModel.sync_status = Constants.SyncStatus.SYNC_NEW.ToString();
          sender._originalProjectGroupModel.teamId = sender._teamId;
        }
        await ProjectGroupDao.TrySaveProjectGroup(sender._originalProjectGroupModel);
        SyncManager.Sync();
        ProjectWidgetsHelper.OnProjectChanged((ProjectIdentity) new GroupProjectIdentity(sender._originalProjectGroupModel, (List<ProjectModel>) null));
        EventHandler<ProjectGroupModel> projectGroupEdit = sender.ProjectGroupEdit;
        if (projectGroupEdit != null)
          projectGroupEdit((object) sender, sender._originalProjectGroupModel);
        sender.Close();
      }
      else
        sender.FolderNameRepeatTextBlock.Visibility = Visibility.Visible;
    }

    public void OnCancel() => this.Close();

    public void Ok()
    {
      this._originalProjectGroupModel.name = this.FolderNameTextBox.Text;
      this.SaveProjectGroup();
    }

    private void CheckEmojiStart(ProjectGroupModel model)
    {
      string emojiIcon1 = EmojiHelper.GetEmojiIcon(model.name);
      if (string.IsNullOrEmpty(emojiIcon1) || !model.name.StartsWith(emojiIcon1))
        return;
      string emojiIcon2 = EmojiHelper.GetEmojiIcon(model.name);
      model.name = model.name.Remove(0, emojiIcon2.Length);
      this.SetProjectEmoji(emojiIcon2);
    }

    private void ShowEmojiSelector(object sender, MouseButtonEventArgs e)
    {
      this.EmojiSelector.GetItems();
      this.EmojiSelector.SetCanReset(!string.IsNullOrEmpty(this.EmojiText.Text));
      this.EmojiSelectPopup.IsOpen = true;
    }

    private void SetProjectEmoji(string emoji)
    {
      this.EmojiText.Text = emoji ?? string.Empty;
      this.ProjectPathGrid.Visibility = string.IsNullOrEmpty(emoji) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnEmojiSelected(string emoji, bool closePopup)
    {
      this.SetProjectEmoji(emoji);
      if (!closePopup)
        return;
      this.EmojiSelectPopup.IsOpen = false;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/addprojectgroupdialog.xaml", UriKind.Relative));
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
          this.Root = (AddProjectGroupDialog) target;
          break;
        case 2:
          this.EmojiSelectGrid = (Grid) target;
          this.EmojiSelectGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowEmojiSelector);
          break;
        case 3:
          this.ProjectPathGrid = (Grid) target;
          break;
        case 4:
          this.ProjectPath = (Path) target;
          break;
        case 5:
          this.SetEmojiIcon = (Path) target;
          break;
        case 6:
          this.EmojiText = (EmjTextBlock) target;
          break;
        case 7:
          this.EmojiSelectPopup = (EscPopup) target;
          break;
        case 8:
          this.EmojiSelector = (EmojiSelectControl) target;
          break;
        case 9:
          this.FolderNameTextBox = (EmojiEditor) target;
          break;
        case 10:
          this.FolderNameRepeatTextBlock = (TextBlock) target;
          break;
        case 11:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 12:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
