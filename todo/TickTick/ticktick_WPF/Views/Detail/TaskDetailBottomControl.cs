// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskDetailBottomControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Agenda;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskDetailBottomControl : Grid, IComponentConnector
  {
    private readonly TaskDetailView _parent;
    private bool _inNavigate;
    private bool _moveProjectMouseDown;
    private bool _moreGridMouseDown;
    private string _switchTag;
    private bool _switchVisible;
    private bool _isPin;
    internal Grid AttendeeItemsPanel;
    internal AttendeeDisplayControl AttendeePanel;
    internal Grid TaskDetailUnit;
    internal Grid TaskDetailMoveGrid;
    internal EscPopup SetProjectPopup;
    internal Grid EditorIcon;
    internal Border EditIconSelectedBorder;
    internal HoverIconButton EditButton;
    internal HoverIconButton CommentGrid;
    internal HoverIconButton DeleteGrid;
    internal HoverIconButton MoreGrid;
    internal EscPopup MorePopup;
    internal ContentControl MoreContentContainer;
    internal Grid TaskDetailTrashGrid;
    internal Grid TrashRestoreGrid;
    internal HoverIconButton RemoveButton;
    private bool _contentLoaded;

    public TaskDetailBottomControl(TaskDetailView taskDetailControl)
    {
      this.InitializeComponent();
      this._parent = taskDetailControl;
    }

    private void ShowAttendeeDialog(object sender, MouseButtonEventArgs e)
    {
      this._parent?.ShowAttendeeDialog();
    }

    private void MoveProjectMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!this._moveProjectMouseDown)
        return;
      this._moveProjectMouseDown = false;
      this.TaskDetailMoveGrid.MouseLeftButtonUp -= new MouseButtonEventHandler(this.MoveProjectMouseUp);
      this._parent?.OnMoveProject(this.SetProjectPopup);
      this.TaskDetailMoveGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoveProjectMouseUp);
    }

    private void PopupOpened(object sender, EventArgs e) => this._parent?.PopupOpened(sender, e);

    private void PopupClosed(object sender, EventArgs e)
    {
      this._parent?.PopupClosed(sender, e);
      this.MoreContentContainer.Content = (object) null;
    }

    private void OnEditorClick(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnEditorClick();
    }

    private void OnCommentClick(object sender, MouseButtonEventArgs e)
    {
      if (this.MorePopup.IsOpen)
        this.MorePopup.IsOpen = false;
      this._parent?.OnCommentClick();
      e.Handled = true;
    }

    private void OnDeleteMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.MorePopup.IsOpen)
        this.MorePopup.IsOpen = false;
      this._parent?.OnDelete();
    }

    private async void MoreGridClick(object sender, MouseButtonEventArgs e)
    {
      TaskDetailBottomControl detailBottomControl = this;
      if (!detailBottomControl._moreGridMouseDown)
        return;
      detailBottomControl._moreGridMouseDown = false;
      TaskDetailMoreContent detailMoreContent = new TaskDetailMoreContent();
      detailMoreContent.SetParent(detailBottomControl._parent, detailBottomControl.MorePopup);
      detailBottomControl.MoreContentContainer.Content = (object) detailMoreContent;
      detailMoreContent.SwitchTaskNoteItem.Tag = (object) detailBottomControl._switchTag;
      detailMoreContent.SwitchTaskNoteItem.Visibility = detailBottomControl._switchVisible ? Visibility.Visible : Visibility.Collapsed;
      if (detailBottomControl.DataContext is TaskDetailViewModel dataContext)
      {
        UserActCollectUtils.AddClickEvent("task_detail", "action", "three_dots");
        bool flag1 = dataContext.Permission != "read" && dataContext.Permission != "comment";
        bool flag2 = dataContext.Kind == "NOTE";
        detailMoreContent.SelectedIndex = -1;
        detailMoreContent.CopyItem.Visibility = flag1 ? Visibility.Visible : Visibility.Collapsed;
        detailMoreContent.NewUploadItem.Visibility = !flag1 || !dataContext.IsOwner ? Visibility.Collapsed : Visibility.Visible;
        detailMoreContent.Print.Visibility = Visibility.Visible;
        detailMoreContent.CopyLinkItem.Visibility = Visibility.Visible;
        detailMoreContent.AddTemplate.Visibility = Visibility.Visible;
        detailMoreContent.OMTopLine.Visibility = flag1 ? Visibility.Visible : Visibility.Collapsed;
        detailMoreContent.AbandonedGrid.Visibility = !flag2 & flag1 ? Visibility.Visible : Visibility.Collapsed;
        detailMoreContent.ActivityItem.Visibility = flag1 ? Visibility.Visible : Visibility.Collapsed;
        detailMoreContent.ActivityItem.Content = Utils.GetString(flag2 ? "NoteActivities" : "TaskActivities");
        detailMoreContent.PomoItem.Visibility = ((!LocalSettings.Settings.EnableFocus || dataContext.Status != 0 ? 0 : (!flag2 ? 1 : 0)) & (flag1 ? 1 : 0)) != 0 ? Visibility.Visible : Visibility.Collapsed;
        detailMoreContent.NewTagItem.Visibility = flag1 ? Visibility.Visible : Visibility.Collapsed;
        detailMoreContent.NewInsetSummaryItem.Visibility = flag1 & flag2 ? Visibility.Visible : Visibility.Collapsed;
        detailMoreContent.FirstOpenTag = true;
        detailMoreContent.PinButton.SetResourceReference(OptionItemWithImageIcon.ImageSourceProperty, detailBottomControl._isPin ? (object) "UnpinnedDrawingImage" : (object) "PinnedDrawingImage");
        detailMoreContent.PinButton.SetResourceReference(OptionItemWithImageIcon.ContentProperty, detailBottomControl._isPin ? (object) "Unpin" : (object) "Pin");
        if (!flag1)
          detailMoreContent.SwitchTaskNoteItem.Visibility = Visibility.Collapsed;
        detailMoreContent.DeletePanel.Visibility = !flag1 || detailBottomControl._inNavigate ? Visibility.Collapsed : Visibility.Visible;
        detailMoreContent.StickyItem.Visibility = flag1 ? Visibility.Visible : Visibility.Collapsed;
        detailMoreContent.SwitchTaskNoteItem.Content = flag2 ? Utils.GetString("ConvertToTask") : Utils.GetString("ConvertToNote");
        detailMoreContent.SwitchTaskNoteItem.SetResourceReference(OptionItemWithImageIcon.ImageSourceProperty, flag2 ? (object) "SwitchTaskDrawingImage" : (object) "SwitchNoteDrawingImage");
        if (TaskDao.GetTaskLevel(dataContext.TaskId, dataContext.ProjectId) >= 4 | flag2 || !flag1)
          detailMoreContent.NewAddSubTaskItem.Visibility = Visibility.Collapsed;
        else
          detailMoreContent.NewAddSubTaskItem.Visibility = Visibility.Visible;
        detailMoreContent.PinButton.Visibility = !flag1 || dataContext.InCal ? Visibility.Collapsed : Visibility.Visible;
      }
      await Task.Delay(20);
      detailBottomControl.MorePopup.IsOpen = !detailBottomControl.MorePopup.IsOpen;
      Window window = Window.GetWindow((DependencyObject) detailBottomControl);
      if (window == null)
        return;
      FocusManager.SetFocusedElement((DependencyObject) window, (IInputElement) window);
    }

    private void RestoreProjectMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnRestoreProject();
    }

    private void DeleteFromTrashMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._parent?.OnDeleteFromTrash();
    }

    public void SetSwitchTagAndVisible(string tag, bool visible)
    {
      this._switchTag = tag;
      this._switchVisible = visible;
    }

    public void SetItemVisible(string itemName, bool show)
    {
      UIElement uiElement = (UIElement) null;
      switch (itemName)
      {
        case "EditorIcon":
          uiElement = (UIElement) this.EditorIcon;
          break;
        case "CommentGrid":
          uiElement = (UIElement) this.CommentGrid;
          break;
        case "DeleteGrid":
          uiElement = (UIElement) this.DeleteGrid;
          break;
        case "EditIconSelectedBorder":
          uiElement = (UIElement) this.EditIconSelectedBorder;
          break;
      }
      if (uiElement == null)
        return;
      uiElement.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    public async Task LoadAttendees(TaskDetailViewModel task, Constants.DetailMode mode)
    {
      TaskDetailBottomControl detailBottomControl = this;
      detailBottomControl.AttendeeItemsPanel.Visibility = Visibility.Collapsed;
      if (string.IsNullOrEmpty(task.AttendId))
        return;
      await detailBottomControl.AttendeePanel.LoadData(mode == Constants.DetailMode.Page ? detailBottomControl.ActualWidth : 340.0, task.AttendId, task.TaskId);
      if (await detailBottomControl.AttendeePanel.WaitingForAttend((AgendaHelper.IAgenda) task))
        return;
      detailBottomControl.AttendeeItemsPanel.Visibility = Visibility.Visible;
    }

    public void OnDataBind(TaskDetailViewModel task)
    {
      bool flag1 = task.Deleted == 1;
      bool flag2 = task.Permission != "comment" && task.Permission != "read";
      this.TaskDetailTrashGrid.Visibility = flag1 & flag2 ? Visibility.Visible : Visibility.Collapsed;
      this.TaskDetailUnit.Visibility = !flag1 ? Visibility.Visible : Visibility.Collapsed;
      ProjectModel projectById = CacheManager.GetProjectById(task.ProjectId);
      if ((projectById != null ? (projectById.IsValid() ? 1 : 0) : 1) == 0)
      {
        this.Visibility = flag1 ? Visibility.Visible : Visibility.Collapsed;
      }
      else
      {
        this.Visibility = Visibility.Visible;
        if (!flag2)
        {
          this.TaskDetailMoveGrid.Cursor = Cursors.No;
          this.RemoveButton.Cursor = Cursors.No;
          this.CommentGrid.Cursor = task.Permission == "read" ? Cursors.No : Cursors.Hand;
        }
        else
        {
          this.TaskDetailMoveGrid.Cursor = Cursors.Hand;
          this.RemoveButton.Cursor = Cursors.Hand;
          this.CommentGrid.Cursor = Cursors.Hand;
        }
        if (flag1 && task.TeamId == TeamService.GetTeamId())
          this.RemoveButton.Visibility = Visibility.Collapsed;
        else
          this.RemoveButton.Visibility = Visibility.Visible;
      }
      this.DeleteGrid.Visibility = Visibility.Collapsed;
    }

    public void SetNavigate()
    {
      this._inNavigate = true;
      this.DeleteGrid.Visibility = Visibility.Collapsed;
      this.TrashRestoreGrid.Visibility = Visibility.Collapsed;
      this.RemoveButton.Visibility = Visibility.Collapsed;
    }

    public void SetPin(bool isPin) => this._isPin = isPin;

    private void MoveProjectMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._moveProjectMouseDown = true;
    }

    private void MoreGridDown(object sender, MouseButtonEventArgs e)
    {
      this._moreGridMouseDown = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskdetailbottomcontrol.xaml", UriKind.Relative));
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
          this.AttendeeItemsPanel = (Grid) target;
          this.AttendeeItemsPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowAttendeeDialog);
          break;
        case 2:
          this.AttendeePanel = (AttendeeDisplayControl) target;
          break;
        case 3:
          this.TaskDetailUnit = (Grid) target;
          break;
        case 4:
          this.TaskDetailMoveGrid = (Grid) target;
          this.TaskDetailMoveGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoveProjectMouseUp);
          this.TaskDetailMoveGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.MoveProjectMouseDown);
          break;
        case 5:
          this.SetProjectPopup = (EscPopup) target;
          break;
        case 6:
          this.EditorIcon = (Grid) target;
          this.EditorIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnEditorClick);
          break;
        case 7:
          this.EditIconSelectedBorder = (Border) target;
          break;
        case 8:
          this.EditButton = (HoverIconButton) target;
          break;
        case 9:
          this.CommentGrid = (HoverIconButton) target;
          break;
        case 10:
          this.DeleteGrid = (HoverIconButton) target;
          break;
        case 11:
          this.MoreGrid = (HoverIconButton) target;
          break;
        case 12:
          this.MorePopup = (EscPopup) target;
          break;
        case 13:
          this.MoreContentContainer = (ContentControl) target;
          break;
        case 14:
          this.TaskDetailTrashGrid = (Grid) target;
          break;
        case 15:
          this.TrashRestoreGrid = (Grid) target;
          this.TrashRestoreGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.RestoreProjectMouseUp);
          break;
        case 16:
          this.RemoveButton = (HoverIconButton) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
