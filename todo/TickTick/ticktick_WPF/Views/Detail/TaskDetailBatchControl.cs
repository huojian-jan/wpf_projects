// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskDetailBatchControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskDetailBatchControl : Grid, IComponentConnector
  {
    private BatchDetailView _parent;
    internal TextBlock BatchTaskText;
    internal TextBlock CloseButton;
    internal OptionItemWithImageIcon BatchRestoreProjectButton;
    internal EscPopup SetProjectPopup;
    internal OptionItemWithImageIcon BatchDeleteFromTrashButton;
    internal Grid BatchSelectDateButton;
    internal Grid BatchTaskPriorityButton;
    internal TextBlock PriorityText;
    internal EscPopup SetPriorityPopup;
    internal DropIconButton BatchTaskMoveProjectButton;
    internal Grid BatchAssigneeGrid;
    internal Rectangle AvatarImage;
    internal ImageBrush Avatar;
    internal EscPopup BatchAssigneePopup;
    internal Grid BatchSetTagsButton;
    internal TextBlock BatchTagsText;
    internal Border BatchTagsContainer;
    internal Border BatchBlockWidthTarget;
    internal WrapPanel BatchTaskBlocks;
    internal Border BatchCompleteOrUndoneBorder;
    internal Path BatchTaskDoneIcon;
    internal Path BatchTaskUndoneIcon;
    internal Border BatchTaskStarBorder;
    internal Path BatchTaskStartPinIcon;
    internal Path BatchTaskStartUnPinIcon;
    internal Border BatchTaskMergeBorder;
    internal Border BatchTaskCopyBorder;
    internal Border BatchSwitchTaskNoteBorder;
    internal Path SwitchPath;
    internal TextBlock SwitchTaskToNoteText;
    internal TextBlock SwitchNoteToTaskText;
    internal Border BatchOpenStickyBorder;
    internal Border BatchCopyTextBorder;
    internal Border BatchDeleteBorder;
    private bool _contentLoaded;

    public TaskDetailBatchControl(BatchDetailView parent)
    {
      this.InitializeComponent();
      this._parent = parent;
      this.BatchTaskMoveProjectButton.TitleText.Height = 18.0;
    }

    private void RestoreProjectClick(object sender, MouseButtonEventArgs e)
    {
      this.SetProjectPopup.PlacementTarget = (UIElement) this.BatchRestoreProjectButton;
      this._parent?.OnRestoreProject();
    }

    private void DeleteFromTrashClick(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnBatchDeleteTaskForever();
    }

    private void SelectDateClick(object sender, MouseButtonEventArgs e)
    {
      this._parent?.SelectDateClick(e);
    }

    private void SetPriorityClick(object sender, MouseButtonEventArgs e)
    {
      if (this.SetPriorityPopup.IsOpen)
      {
        this.SetPriorityPopup.IsOpen = false;
      }
      else
      {
        SetPriorityDialog setPriorityDialog = new SetPriorityDialog();
        this.SetPriorityPopup.Child = (UIElement) setPriorityDialog;
        this.SetPriorityPopup.IsOpen = true;
        // ISSUE: method pointer
        setPriorityDialog.PrioritySelect += new EventHandler<int>((object) this, __methodptr(\u003CSetPriorityClick\u003Eg__OnPrioritySelected\u007C5_0));
      }
    }

    private void MoveProjectMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnMoveProject(this.SetProjectPopup, true);
    }

    private void OnSetAssigneeMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnSetAssigneeMouseUp(true, this.BatchAssigneePopup);
    }

    private void OnTagMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this._parent?.OnTag(this.BatchSetTagsButton);
    }

    private void OnBatchCompleteMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this._parent?.OnBatchCompleteMouseUp(sender);
    }

    private void OnStarMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnBatchPinClick(sender, this.BatchTaskStartPinIcon.IsVisible);
    }

    private void OnMergeMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnMergeMouseUp(sender);
    }

    private void BatchCopyMouseUp(object sender, MouseButtonEventArgs e) => this._parent?.OnCopy();

    private void SwitchTaskNoteMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.BatchSwitchTaskNoteMouseUp(sender);
    }

    private void OnBatchCopyTextMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnBatchCopyTextMouseUp(sender);
    }

    private void OnDeleteMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnBatchDelete(sender);
    }

    public async void SetBatchAssignVisible(bool show, string avatarUrl)
    {
      this.BatchAssigneeGrid.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
      ImageBrush imageBrush = this.Avatar;
      imageBrush.ImageSource = (ImageSource) await AvatarHelper.GetAvatarByUrlAsync(avatarUrl);
      imageBrush = (ImageBrush) null;
      this.AvatarImage.Visibility = !string.IsNullOrEmpty(avatarUrl) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnBatchOpenStickyMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.BatchOpenSticky();
    }

    public async void SetAvatar(string assignee, string projectId)
    {
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.ProjectId == projectId));
      if ((projectModel == null ? 0 : (projectModel.IsShareList() ? 1 : 0)) != 0)
      {
        string avatarUrl = (string) null;
        if (!string.IsNullOrEmpty(assignee) && assignee != "-1")
          avatarUrl = await AvatarHelper.GetAvatarUrl(assignee, projectId);
        this.SetBatchAssignVisible(true, avatarUrl);
      }
      else
        this.SetBatchAssignVisible(false, (string) null);
    }

    private void OnCancelClick(object sender, MouseButtonEventArgs e) => this._parent?.OnCancel();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskdetailbatchcontrol.xaml", UriKind.Relative));
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
          this.BatchTaskText = (TextBlock) target;
          break;
        case 2:
          this.CloseButton = (TextBlock) target;
          this.CloseButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCancelClick);
          break;
        case 3:
          this.BatchRestoreProjectButton = (OptionItemWithImageIcon) target;
          break;
        case 4:
          this.SetProjectPopup = (EscPopup) target;
          break;
        case 5:
          this.BatchDeleteFromTrashButton = (OptionItemWithImageIcon) target;
          break;
        case 6:
          this.BatchSelectDateButton = (Grid) target;
          this.BatchSelectDateButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectDateClick);
          break;
        case 7:
          this.BatchTaskPriorityButton = (Grid) target;
          this.BatchTaskPriorityButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetPriorityClick);
          break;
        case 8:
          this.PriorityText = (TextBlock) target;
          break;
        case 9:
          this.SetPriorityPopup = (EscPopup) target;
          break;
        case 10:
          this.BatchTaskMoveProjectButton = (DropIconButton) target;
          break;
        case 11:
          this.BatchAssigneeGrid = (Grid) target;
          this.BatchAssigneeGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSetAssigneeMouseUp);
          break;
        case 12:
          this.AvatarImage = (Rectangle) target;
          break;
        case 13:
          this.Avatar = (ImageBrush) target;
          break;
        case 14:
          this.BatchAssigneePopup = (EscPopup) target;
          break;
        case 15:
          this.BatchSetTagsButton = (Grid) target;
          this.BatchSetTagsButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTagMouseUp);
          break;
        case 16:
          this.BatchTagsText = (TextBlock) target;
          break;
        case 17:
          this.BatchTagsContainer = (Border) target;
          break;
        case 18:
          this.BatchBlockWidthTarget = (Border) target;
          break;
        case 19:
          this.BatchTaskBlocks = (WrapPanel) target;
          break;
        case 20:
          this.BatchCompleteOrUndoneBorder = (Border) target;
          this.BatchCompleteOrUndoneBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBatchCompleteMouseUp);
          break;
        case 21:
          this.BatchTaskDoneIcon = (Path) target;
          break;
        case 22:
          this.BatchTaskUndoneIcon = (Path) target;
          break;
        case 23:
          this.BatchTaskStarBorder = (Border) target;
          this.BatchTaskStarBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStarMouseUp);
          break;
        case 24:
          this.BatchTaskStartPinIcon = (Path) target;
          break;
        case 25:
          this.BatchTaskStartUnPinIcon = (Path) target;
          break;
        case 26:
          this.BatchTaskMergeBorder = (Border) target;
          this.BatchTaskMergeBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMergeMouseUp);
          break;
        case 27:
          this.BatchTaskCopyBorder = (Border) target;
          this.BatchTaskCopyBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.BatchCopyMouseUp);
          break;
        case 28:
          this.BatchSwitchTaskNoteBorder = (Border) target;
          this.BatchSwitchTaskNoteBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchTaskNoteMouseUp);
          break;
        case 29:
          this.SwitchPath = (Path) target;
          break;
        case 30:
          this.SwitchTaskToNoteText = (TextBlock) target;
          break;
        case 31:
          this.SwitchNoteToTaskText = (TextBlock) target;
          break;
        case 32:
          this.BatchOpenStickyBorder = (Border) target;
          this.BatchOpenStickyBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBatchOpenStickyMouseUp);
          break;
        case 33:
          this.BatchCopyTextBorder = (Border) target;
          this.BatchCopyTextBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBatchCopyTextMouseUp);
          break;
        case 34:
          this.BatchDeleteBorder = (Border) target;
          this.BatchDeleteBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteMouseUp);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
