// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.DetailView.TaskDetailView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Event;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Attachment;
using ticktick_WPF.Util.DateParser;
using ticktick_WPF.Util.Files;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Activity;
using ticktick_WPF.Views.Agenda;
using ticktick_WPF.Views.CheckList;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.CustomPopup;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.NewUser;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Print;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Summary;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.TaskList.Item;
using ticktick_WPF.Views.Template;
using ticktick_WPF.Views.Time;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MainListView.DetailView
{
  public class TaskDetailView : Grid, IBatchEditable
  {
    private StackPanel _guidePanel;
    private static readonly string _guideString = "![file](https://pull.dida365.com";
    private string _guideLink;
    private string _guideTaskId;
    private ContentControl _videoContainer;
    private ContentControl _actionButton;
    private Stopwatch _stopWatch;
    private string _label;
    private TTMediaPlayer _videoPlayer;
    private Dictionary<string, KeyBinding> _keyBindings;
    private readonly ScrollViewer _detailScrollViewer;
    private string _uid;
    private bool? _isDark;
    private TaskDetailHeadView _detailHead;
    private Grid _scrollContent;
    private Grid _titleUpperContent;
    private TaskActivityControl _activityPanel;
    private RowDefinition _scrollButtonRow;
    private TaskDetailBottomControl _detailBottom;
    private int _quadrantLevel;
    private bool _navigating;
    private TaskActivityPanel _taskActivityPanel;
    private bool _needFocusDetail;
    private bool _canScroll;
    private StackPanel _attachmentPanel;
    private AttachmentProvider _attachmentProvider;
    private readonly TTAsyncLocker _lockAttachment;
    private readonly AttachmentOptionPanel _attachmentOptionPanel;
    private CommentDisplayControl _commentDisplayCtrl;
    private AddCommentControl _addCommentCtrl;
    private readonly TTAsyncLocker<bool, bool, int> _commentAsyncLocker;
    private Grid _contentGrid;
    private MarkDownEditor _contentText;
    private MarkDownEditor _descText;
    private ChecklistControl _checklist;
    private EditorMenu _textEditorMenu;
    private TagDisplayControl _tagsControl;
    private bool _switchTextClick;
    private string _switchListOriginText;
    private StackPanel _hintTextPanel;
    private BatchTaskEditHelper _batchHelper;
    private StackPanel _subtaskGrid;
    private TaskListView _subTaskList;
    private readonly TTAsyncLocker<TaskDetailViewModel, bool, int> _subTaskAsyncLocker;
    private readonly TTAsyncLocker _addSubTaskAsyncLocker;
    private readonly DetailTextBox _titleText;
    private TextBlock _titleHint;
    private HoverIconButton _kindSwitcher;
    private bool _titleForceParse;
    private (string, List<TaskDetailItemModel>) _savedItems;
    private ImmersiveContent _immersiveContent;

    private void TryShowGuide()
    {
      string taskId = this._task.TaskId;
      if (this._guideTaskId == taskId)
        return;
      this._videoPlayer?.PlayVideo((string) null);
      if (!string.IsNullOrEmpty(this._guideTaskId) && !string.IsNullOrEmpty(this._label))
        this.RecordLabelTime();
      this._label = this._task.Label;
      if (!string.IsNullOrEmpty(this._label))
      {
        this._stopWatch = this._stopWatch ?? new Stopwatch();
        this._stopWatch.Restart();
      }
      this._guideTaskId = taskId;
      string path = string.Empty;
      List<GuideProjectTaskResource> guideResources = this._task.GuideResources;
      GuideProjectTaskResource projectTaskResource = guideResources != null ? guideResources.FirstOrDefault<GuideProjectTaskResource>((Func<GuideProjectTaskResource, bool>) (g => g.type == "video")) : (GuideProjectTaskResource) null;
      if (projectTaskResource != null)
        path = projectTaskResource.url;
      else if (this._task.TaskContent != null && this._task.TaskContent.Contains(TaskDetailView._guideString))
      {
        string taskContent = this._task.TaskContent;
        int startIndex = taskContent.LastIndexOf(TaskDetailView._guideString, StringComparison.Ordinal);
        int num = taskContent.IndexOf(")", startIndex, StringComparison.Ordinal);
        if (num > startIndex)
        {
          string str = taskContent.Substring(startIndex, num - startIndex + 1);
          path = str.Substring(8, str.Length - 9);
        }
      }
      string fileName1 = System.IO.Path.GetFileName(path);
      if (fileName1 != null && fileName1.EndsWith(".mp4"))
      {
        this._guideLink = "![file](" + path + ")";
        List<string> list = ((IEnumerable<string>) path.Split('/')).ToList<string>();
        string str = fileName1.Replace(".mp4", ".avi");
        string fileName2 = str.Insert(str.Length - 4, "_" + (list.Count > 1 ? list[list.Count - 2] : ""));
        this.AddGuideVideo(path.Replace(".mp4", ".avi"), fileName2);
      }
      else
      {
        this._guideLink = "";
        if (this._videoContainer != null)
          this._videoContainer.Content = (object) null;
        this._videoPlayer?.PlayVideo((string) null);
        this._videoPlayer = (TTMediaPlayer) null;
      }
      if (this._task != null)
      {
        List<string> guideActions = this._task.GuideActions;
        // ISSUE: explicit non-virtual call
        if ((guideActions != null ? (__nonvirtual (guideActions.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          this.AddAction(this._task.GuideActions[0]);
          return;
        }
      }
      if (this._actionButton == null)
        return;
      this._actionButton.Content = (object) null;
      this._actionButton.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnGuideActionClick);
    }

    public void RecordLabelTime()
    {
      if (string.IsNullOrEmpty(this._label) || this._stopWatch == null || !this._stopWatch.IsRunning)
        return;
      this._stopWatch.Stop();
      UserActCollectUtils.AddTaskLabelEvent("preset_list", "stay_time", this._label, this._stopWatch.ElapsedMilliseconds);
    }

    private string GetActionName(string url)
    {
      switch (url)
      {
        case "ticktick://project?from=select-common":
          return Utils.GetString("ChooseCommonLists");
        case "ticktick://matrix?from=detail":
          return string.Format(Utils.GetString(LocalSettings.Settings.ShowMatrix ? "GoFeature" : "EnableFeature"), (object) Utils.GetString("Matrix"));
        case "ticktick://focus?from=detail":
          return string.Format(Utils.GetString(LocalSettings.Settings.EnableFocus ? "GoFeature" : "EnableFeature"), (object) Utils.GetString("PomoFocus"));
        case "ticktick://habit?from=detail":
          return string.Format(Utils.GetString(LocalSettings.Settings.ShowHabit ? "GoFeature" : "EnableFeature"), (object) Utils.GetString("Habit"));
        default:
          return (string) null;
      }
    }

    private void OnGuideActionClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this._actionButton.Tag is string tag))
        return;
      switch (tag)
      {
        case "ticktick://project?from=select-common":
          UserActCollectUtils.AddClickEvent("preset_list", "preset_list_btn", "select_list");
          if (!UserDao.IsPro() && CacheManager.GetProjectsWithoutInbox().Count<ProjectModel>((Func<ProjectModel, bool>) (p => !p.delete_status)) >= 9)
          {
            ProChecker.CheckPro(ProType.MoreLists);
            break;
          }
          ChooseCommonProjectWindow commonProjectWindow = new ChooseCommonProjectWindow();
          commonProjectWindow.Owner = Window.GetWindow((DependencyObject) this);
          commonProjectWindow.ShowDialog();
          break;
        case "ticktick://matrix?from=detail":
          if (LocalSettings.Settings.ShowMatrix)
          {
            UserActCollectUtils.AddClickEvent("preset_list", "preset_list_btn", "go_matrix");
            App.Window.SwitchModule(DisplayModule.Matrix);
            App.Window.TryShowMainWindow();
            break;
          }
          UserActCollectUtils.AddClickEvent("preset_list", "preset_list_btn", "enable_matrix");
          LocalSettings.Settings.ShowOrHideTabBar("MATRIX", true);
          if (!(this._actionButton.Content is EmjTextBlock content1))
            break;
          content1.Text = "\uD83D\uDC49" + this.GetActionName(tag);
          break;
        case "ticktick://focus?from=detail":
          if (LocalSettings.Settings.EnableFocus)
          {
            UserActCollectUtils.AddClickEvent("preset_list", "preset_list_btn", "go_pomo");
            App.Window.SwitchModule(DisplayModule.Pomo);
            App.Window.TryShowMainWindow();
            break;
          }
          UserActCollectUtils.AddClickEvent("preset_list", "preset_list_btn", "enable_pomo");
          LocalSettings.Settings.ShowOrHideTabBar("POMO", true);
          if (!(this._actionButton.Content is EmjTextBlock content2))
            break;
          content2.Text = "\uD83D\uDC49" + this.GetActionName(tag);
          break;
        case "ticktick://habit?from=detail":
          if (LocalSettings.Settings.ShowHabit)
          {
            UserActCollectUtils.AddClickEvent("preset_list", "preset_list_btn", "go_habit");
            App.Window.SwitchModule(DisplayModule.Habit);
            App.Window.TryShowMainWindow();
            break;
          }
          UserActCollectUtils.AddClickEvent("preset_list", "preset_list_btn", "enable_habit");
          LocalSettings.Settings.ShowOrHideTabBar("HABIT", true);
          if (!(this._actionButton.Content is EmjTextBlock content3))
            break;
          content3.Text = "\uD83D\uDC49" + this.GetActionName(tag);
          break;
      }
    }

    private async void AddAction(string action)
    {
      TaskDetailView taskDetailView = this;
      taskDetailView.InitGuidePanel();
      EmjTextBlock emjTextBlock1 = new EmjTextBlock();
      emjTextBlock1.Background = (Brush) Brushes.Transparent;
      emjTextBlock1.Text = "\uD83D\uDC49 " + taskDetailView.GetActionName(action);
      emjTextBlock1.Margin = new Thickness(0.0, 21.0, 0.0, 4.0);
      EmjTextBlock emjTextBlock2 = emjTextBlock1;
      emjTextBlock2.SetResourceReference(TextBlock.ForegroundProperty, (object) "PrimaryColor");
      emjTextBlock2.SetResourceReference(TextBlock.FontSizeProperty, (object) "Font14");
      taskDetailView._actionButton.Content = (object) emjTextBlock2;
      taskDetailView._actionButton.Tag = (object) action;
      taskDetailView._actionButton.MouseLeftButtonUp -= new MouseButtonEventHandler(taskDetailView.OnGuideActionClick);
      taskDetailView._actionButton.MouseLeftButtonUp += new MouseButtonEventHandler(taskDetailView.OnGuideActionClick);
    }

    private async Task AddGuideVideo(string url, string fileName)
    {
      TaskDetailView taskDetailView1 = this;
      taskDetailView1.InitGuidePanel();
      if (taskDetailView1._videoPlayer == null)
      {
        TaskDetailView taskDetailView2 = taskDetailView1;
        TTMediaPlayer ttMediaPlayer = new TTMediaPlayer();
        ttMediaPlayer.Margin = new Thickness(4.0, 0.0, 4.0, 0.0);
        taskDetailView2._videoPlayer = ttMediaPlayer;
        taskDetailView1._videoContainer.Content = (object) taskDetailView1._videoPlayer;
      }
      if (!File.Exists(AppPaths.ImageDir + fileName))
      {
        int num = await IOUtils.DownloadFile(AppPaths.ImageDir, fileName, url) ? 1 : 0;
      }
      if (File.Exists(AppPaths.ImageDir + fileName))
      {
        try
        {
          taskDetailView1._videoPlayer.PlayVideo(AppPaths.ImageDir + fileName);
        }
        catch (Exception ex)
        {
        }
      }
      else
      {
        if (taskDetailView1._videoContainer != null)
          taskDetailView1._videoContainer.Content = (object) null;
        taskDetailView1._videoPlayer?.PlayVideo((string) null);
        taskDetailView1._videoPlayer = (TTMediaPlayer) null;
      }
    }

    private void InitGuidePanel()
    {
      if (this._guidePanel == null)
      {
        this._guidePanel = new StackPanel();
        this._guidePanel.SetValue(Grid.RowProperty, (object) 3);
        this._contentGrid.Children.Add((UIElement) this._guidePanel);
      }
      if (this._videoContainer == null)
      {
        ContentControl contentControl = new ContentControl();
        contentControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        this._videoContainer = contentControl;
        this._guidePanel.Children.Add((UIElement) this._videoContainer);
      }
      if (this._actionButton != null)
        return;
      ContentControl contentControl1 = new ContentControl();
      contentControl1.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
      contentControl1.Background = (Brush) Brushes.Transparent;
      contentControl1.Cursor = System.Windows.Input.Cursors.Hand;
      contentControl1.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
      this._actionButton = contentControl1;
      this._guidePanel.Children.Add((UIElement) this._actionButton);
    }

    public void RemoveKeyBinding()
    {
      lock (this._keyBindings)
      {
        foreach (KeyValuePair<string, KeyBinding> keyBinding in this._keyBindings)
        {
          KeyBindingManager.RemoveKeyBinding(keyBinding.Key, keyBinding.Value);
          keyBinding.Value.CommandParameter = (object) null;
          keyBinding.Value.CommandTarget = (IInputElement) null;
        }
        this.InputBindings.Clear();
        this._keyBindings.Clear();
      }
      if (this._checklist != null)
        this._checklist.RemoveEvents();
      this.RecordLabelTime();
    }

    private void InitShortcut()
    {
      if (this.Mode != Constants.DetailMode.Sticky)
      {
        AddKeyBinding("SetNoPriority", GetKeyBinding(DetailControlCommands.SetPriorityNoneCommand));
        AddKeyBinding("SetLowPriority", GetKeyBinding(DetailControlCommands.SetPriorityLowCommand));
        AddKeyBinding("SetMediumPriority", GetKeyBinding(DetailControlCommands.SetPriorityMediumCommand));
        AddKeyBinding("SetHighPriority", GetKeyBinding(DetailControlCommands.SetPriorityHighCommand));
        AddKeyBinding("ClearDate", GetKeyBinding(DetailControlCommands.ClearDateCommand));
        AddKeyBinding("SetToday", GetKeyBinding(DetailControlCommands.SetTodayCommand));
        AddKeyBinding("SetTomorrow", GetKeyBinding(DetailControlCommands.SetTomorrowCommand));
        AddKeyBinding("SetNextWeek", GetKeyBinding(DetailControlCommands.SetNextWeekCommand));
        AddKeyBinding("SetDate", GetKeyBinding(DetailControlCommands.SelectDateCommand));
        AddKeyBinding("Print", GetKeyBinding(DetailControlCommands.PrintCommand));
        AddKeyBinding("PinTask", GetKeyBinding(DetailControlCommands.PinCommand));
        AddKeyBinding("OpenSticky", GetKeyBinding(DetailControlCommands.OpenAsSticky));
      }
      AddKeyBinding("CompleteTask", GetKeyBinding(DetailControlCommands.CompleteCommand));
      KeyBinding keyBinding = new KeyBinding(DetailControlCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      this.InputBindings.Add((InputBinding) keyBinding);

      void AddKeyBinding(string key, KeyBinding kb)
      {
        lock (this._keyBindings)
        {
          this._keyBindings[key] = kb;
          KeyBindingManager.TryAddKeyBinding(key, kb);
        }
      }

      KeyBinding GetKeyBinding(ICommand command)
      {
        if (command == null)
          return (KeyBinding) null;
        KeyBinding keyBinding1 = new KeyBinding(command, new KeyGesture(Key.None));
        keyBinding1.CommandParameter = (object) this;
        KeyBinding keyBinding2 = keyBinding1;
        this.InputBindings.Add((InputBinding) keyBinding2);
        return keyBinding2;
      }
    }

    public void SetPriority(int priority)
    {
      if (!this._task.Enable || !(this._task.Kind != "NOTE"))
        return;
      ITaskOperation focusedTaskItem = this.GetFocusedTaskItem();
      if (focusedTaskItem != null)
        focusedTaskItem.SetPriority(priority);
      else
        this.SetTaskPriority(priority);
    }

    public async void SetDate(string key)
    {
      if (!this._task.Enable)
        return;
      ITaskOperation focusedTaskItem = this.GetFocusedTaskItem();
      if (focusedTaskItem != null)
      {
        focusedTaskItem.SetDate(key);
      }
      else
      {
        DateTime time = DateTime.Today;
        switch (key)
        {
          case "today":
            time = DateTime.Today;
            UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_today");
            break;
          case "tomorrow":
            time = DateTime.Today.AddDays(1.0);
            UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_tomorrow");
            break;
          case "nextweek":
            time = DateTime.Today.AddDays(7.0);
            UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_next_week");
            break;
        }
        this.TryClearParseDate();
        await this.OnDateSelected(time);
      }
    }

    public void SelectDate()
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_date");
      if (!this._task.Enable || this.DatePopupShow)
        return;
      ITaskOperation focusedTaskItem = this.GetFocusedTaskItem();
      if (focusedTaskItem != null)
        focusedTaskItem.SelectDate(false);
      else
        this.ShowSelectDateDialog();
    }

    public async void ClearDate()
    {
      if (!this._task.Enable)
        return;
      ITaskOperation focusedTaskItem = this.GetFocusedTaskItem();
      if (focusedTaskItem != null)
        focusedTaskItem.ClearDate();
      else
        await this.ClearTaskDate();
    }

    public async void ToggleTaskCompleted()
    {
      if (!this._task.Enable || !(this._task.Kind != "NOTE"))
        return;
      ITaskOperation focusedTaskItem = this.GetFocusedTaskItem();
      if (focusedTaskItem != null)
        focusedTaskItem.ToggleTaskCompleted();
      else
        await this.CompleteOrUndoneTask();
    }

    public async void OnPrint()
    {
      TaskDetailView sender = this;
      sender._popupShowing = false;
      EventHandler forceHideWindow = sender.ForceHideWindow;
      if (forceHideWindow != null)
        forceHideWindow((object) sender, (EventArgs) null);
      if (!(sender.DataContext is TaskDetailViewModel model))
      {
        model = (TaskDetailViewModel) null;
      }
      else
      {
        string avatarUrl = await AvatarHelper.GetAvatarUrl(model.Assignee, model.ProjectId);
        TaskDetailPrintViewModel printModel = new TaskDetailPrintViewModel(model)
        {
          Avatar = AvatarHelper.GetAvatarByUrl(avatarUrl)
        };
        List<CommentViewModel> commentViewModels = await sender.GetLocalCommentViewModels();
        // ISSUE: reference to a compiler-generated method
        if (commentViewModels.Any<CommentViewModel>(new Func<CommentViewModel, bool>(sender.\u003COnPrint\u003Eb__24_0)))
          printModel.Comments = commentViewModels.Where<CommentViewModel>((Func<CommentViewModel, bool>) (c => c.Model.deleted == 0)).ToList<CommentViewModel>();
        List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(model.TaskId);
        // ISSUE: explicit non-virtual call
        if (checkItemsByTaskId != null && __nonvirtual (checkItemsByTaskId.Count) > 0)
        {
          checkItemsByTaskId.Sort((Comparison<TaskDetailItemModel>) ((a, b) =>
          {
            if (a.status != 0 && b.status == 0)
              return 1;
            if (a.status == 0 && b.status != 0)
              return -1;
            return a.status != 0 && b.status != 0 && a.completedTime.HasValue && b.completedTime.HasValue ? b.completedTime.Value.CompareTo(a.completedTime.Value) : a.sortOrder.CompareTo(b.sortOrder);
          }));
          printModel.SubtaskPrintViewModels = checkItemsByTaskId.Select<TaskDetailItemModel, SubtaskPrintViewModel>((Func<TaskDetailItemModel, SubtaskPrintViewModel>) (i => new SubtaskPrintViewModel(i))).ToList<SubtaskPrintViewModel>();
        }
        Node<DisplayItemModel> taskNode = TaskDao.GetDisplayItemNode(model.TaskId, model.ProjectId, !LocalSettings.Settings.HideComplete);
        await sender.SortChildrenBySortOrder(taskNode);
        List<DisplayItemModel> allChildrenValue = taskNode?.GetAllChildrenValue();
        // ISSUE: explicit non-virtual call
        if (allChildrenValue != null && __nonvirtual (allChildrenValue.Count) > 0)
          printModel.RealSubtaskPrintViewModels = allChildrenValue.Select<DisplayItemModel, SubtaskPrintViewModel>((Func<DisplayItemModel, SubtaskPrintViewModel>) (i => new SubtaskPrintViewModel(i))).ToList<SubtaskPrintViewModel>();
        await printModel.LoadAttachment();
        PrintPreviewWindow printWindow = new PrintPreviewWindow(printModel);
        await Task.Delay(300);
        printWindow.Show();
        printWindow.Activate();
        printModel = (TaskDetailPrintViewModel) null;
        taskNode = (Node<DisplayItemModel>) null;
        printWindow = (PrintPreviewWindow) null;
        model = (TaskDetailViewModel) null;
      }
    }

    public void OnEsc()
    {
      EventHandler escKeyUp = this.EscKeyUp;
      if (escKeyUp == null)
        return;
      escKeyUp((object) this, (EventArgs) null);
    }

    public async void OnPin()
    {
      TaskDetailView child = this;
      if (!child.CheckEnable())
        return;
      ProjectIdentity selectedProject = Utils.FindParent<ListViewContainer>((DependencyObject) child)?.GetSelectedProject();
      await TaskService.TogglesStarred(child._task.TaskId, selectedProject?.CatId);
    }

    private ITaskOperation GetFocusedTaskItem()
    {
      if (this._subtaskGrid != null)
      {
        switch (Keyboard.FocusedElement)
        {
          case ITaskOperation focusedTaskItem:
            return focusedTaskItem;
          case TextArea child:
            ITaskOperation parent = Utils.FindParent<ITaskOperation>((DependencyObject) child);
            if (parent != null)
              return parent;
            break;
        }
      }
      return (ITaskOperation) null;
    }

    public void OpenAsSticky()
    {
      if (this._task.IsNewAdd)
        return;
      EventHandler forceHideWindow = this.ForceHideWindow;
      if (forceHideWindow != null)
        forceHideWindow((object) this, (EventArgs) null);
      TaskStickyWindow.ShowTaskSticky(new List<string>()
      {
        this._task.TaskId
      });
    }

    private TaskDetailViewModel _task => this.DataContext as TaskDetailViewModel;

    public string TaskId => this._task.TaskId;

    public string ProjectId => this._task.ProjectId;

    private bool _popupShowing { get; set; }

    public Constants.DetailMode Mode { get; set; }

    public bool Immerse { get; set; }

    public bool DatePopupShow { get; set; }

    public bool IsBlank { get; set; }

    public ProjectIdentity ParentIdentity { get; set; }

    public bool PopupShowing => this._popupShowing;

    public ScrollViewer ScrollViewer => this._detailScrollViewer;

    public TaskDetailView()
      : this(Constants.DetailMode.Page)
    {
    }

    public TaskDetailView(Constants.DetailMode mode)
    {
      ScrollViewer scrollViewer = new ScrollViewer();
      scrollViewer.Focusable = false;
      scrollViewer.AllowDrop = true;
      scrollViewer.MinHeight = 110.0;
      this._detailScrollViewer = scrollViewer;
      this._uid = Utils.GetGuid();
      this._canScroll = true;
      this._lockAttachment = new TTAsyncLocker(1, 1);
      AttachmentOptionPanel attachmentOptionPanel = new AttachmentOptionPanel();
      attachmentOptionPanel.Width = 0.0;
      attachmentOptionPanel.Height = 0.0;
      attachmentOptionPanel.Cursor = System.Windows.Input.Cursors.Hand;
      attachmentOptionPanel.Opacity = 1.0;
      attachmentOptionPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
      attachmentOptionPanel.VerticalAlignment = VerticalAlignment.Top;
      this._attachmentOptionPanel = attachmentOptionPanel;
      this._commentAsyncLocker = new TTAsyncLocker<bool, bool, int>();
      Grid grid = new Grid();
      grid.MinHeight = 50.0;
      this._contentGrid = grid;
      this._subTaskAsyncLocker = new TTAsyncLocker<TaskDetailViewModel, bool, int>();
      this._addSubTaskAsyncLocker = new TTAsyncLocker(1, 1);
      DetailTextBox detailTextBox = new DetailTextBox();
      detailTextBox.Margin = new Thickness(0.0, 0.0, 24.0, 0.0);
      detailTextBox.MaxLength = 2048;
      this._titleText = detailTextBox;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.Mode = mode;
      this.DataContext = (object) new TaskDetailViewModel(new TaskBaseViewModel());
      this.InitShortcut();
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition());
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.OnDetailKeyDown);
      this.SizeChanged += new SizeChangedEventHandler(this.OnDetailSizeChanged);
      this.InitDetailHead();
      this.InitScrollContent(mode);
      this.InitBottom();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    private void InitDetailHead()
    {
      if (this.Mode == Constants.DetailMode.Sticky || this.Mode == Constants.DetailMode.Editor)
        return;
      TaskDetailHeadView taskDetailHeadView = new TaskDetailHeadView();
      taskDetailHeadView.Margin = new Thickness(21.0, this.Mode == Constants.DetailMode.Page ? 25.0 : 4.0, 20.0, 0.0);
      this._detailHead = taskDetailHeadView;
      this.Children.Add((UIElement) this._detailHead);
    }

    private void InitBottom()
    {
      if (this.Mode == Constants.DetailMode.Sticky || this.Mode == Constants.DetailMode.Editor)
        return;
      this._detailBottom = new TaskDetailBottomControl(this);
      this._detailBottom.SetValue(Grid.RowProperty, (object) 4);
      this.Children.Add((UIElement) this._detailBottom);
    }

    private void InitScrollContent(Constants.DetailMode mode)
    {
      this._detailScrollViewer.SetResourceReference(FrameworkElement.StyleProperty, (object) "for_scrollviewer");
      this._detailScrollViewer.SetValue(Grid.RowProperty, (object) 1);
      this._detailScrollViewer.Drop += new System.Windows.DragEventHandler(this.OnAttachmentDrop);
      this._detailScrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(this.OnMouseScrollOnTaskDetail);
      this._detailScrollViewer.ScrollChanged += new ScrollChangedEventHandler(this.TaskDetailScrollChanged);
      this._detailScrollViewer.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDetailContentClick);
      this.Children.Add((UIElement) this._detailScrollViewer);
      Grid grid1 = new Grid();
      Thickness thickness;
      switch (mode)
      {
        case Constants.DetailMode.Editor:
          thickness = new Thickness(20.0, 0.0, 20.0, 40.0);
          break;
        case Constants.DetailMode.Sticky:
          thickness = new Thickness(10.0, 5.0, 10.0, 5.0);
          break;
        default:
          thickness = new Thickness(20.0, 0.0, 20.0, 0.0);
          break;
      }
      grid1.Margin = thickness;
      this._scrollContent = grid1;
      this._scrollContent.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._scrollContent.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._scrollContent.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._scrollContent.RowDefinitions.Add(new RowDefinition());
      this._scrollContent.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._scrollButtonRow = new RowDefinition()
      {
        Height = new GridLength(0.0)
      };
      this._scrollContent.RowDefinitions.Add(this._scrollButtonRow);
      this._detailScrollViewer.Content = (object) this._scrollContent;
      Grid grid2 = new Grid();
      grid2.Margin = new Thickness(0.0, this.Mode == Constants.DetailMode.Sticky ? 0.0 : 10.0, 0.0, 0.0);
      this._titleUpperContent = grid2;
      this._scrollContent.Children.Add((UIElement) this._titleUpperContent);
      this.InitDetailTitle();
      this.InitContentPanel();
    }

    private void SetUpperContent(TaskDetailViewModel model)
    {
      this.SetParentTitle(model, false);
      this.TryLoadPomoCount(model);
    }

    private void SetParentTitle(TaskDetailViewModel model, bool checkPomo = true)
    {
      int num = this.Mode == Constants.DetailMode.Sticky ? 0 : (this.Mode != Constants.DetailMode.Editor ? 1 : 0);
      TaskBaseViewModel taskById = string.IsNullOrEmpty(model.ParentId) ? (TaskBaseViewModel) null : TaskCache.GetTaskById(model.ParentId);
      if (num != 0 && taskById != null && taskById.ProjectId == model.ProjectId && taskById.Deleted == 0 && taskById.Kind != "NOTE")
      {
        this.SetPrentTitle(taskById);
      }
      else
      {
        Grid singleVisualChildren = Utils.FindSingleVisualChildren<Grid>((DependencyObject) this._titleUpperContent);
        if (singleVisualChildren != null)
          this._titleUpperContent.Children.Remove((UIElement) singleVisualChildren);
      }
      if (!checkPomo)
        return;
      TaskDetailPomoSummaryControl singleVisualChildren1 = Utils.FindSingleVisualChildren<TaskDetailPomoSummaryControl>((DependencyObject) this._titleUpperContent);
      if (singleVisualChildren1 == null)
        return;
      singleVisualChildren1.Margin = new Thickness(0.0, this.GetParentTitle() == null ? -4.0 : 20.0, 0.0, 0.0);
    }

    private async void TryLoadPomoCount(TaskDetailViewModel model)
    {
      TaskDetailView taskDetailView = this;
      int num = taskDetailView.Mode == Constants.DetailMode.Sticky ? 0 : (taskDetailView.Mode != Constants.DetailMode.Editor ? 1 : 0);
      TaskDetailPomoSummaryControl pomoCount = Utils.FindSingleVisualChildren<TaskDetailPomoSummaryControl>((DependencyObject) taskDetailView._titleUpperContent);
      if (num == 0 || !LocalSettings.Settings.EnableFocus || model.Kind == "NOTE")
      {
        if (pomoCount == null)
        {
          pomoCount = (TaskDetailPomoSummaryControl) null;
        }
        else
        {
          taskDetailView._titleUpperContent.Children.Remove((UIElement) pomoCount);
          pomoCount = (TaskDetailPomoSummaryControl) null;
        }
      }
      else
      {
        PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(model.TaskId);
        if (pomoByTaskId != null && taskDetailView.GetDisplayDateDiff() > 0.0)
        {
          pomoByTaskId.PomoDuration = 0L;
          pomoByTaskId.count = 0;
          pomoByTaskId.StopwatchDuration = 0L;
          pomoByTaskId.focuses = (List<object[]>) null;
        }
        if (pomoByTaskId != null && !pomoByTaskId.IsEmpty())
        {
          if (pomoCount == null)
          {
            TaskDetailPomoSummaryControl pomoSummaryControl = new TaskDetailPomoSummaryControl();
            pomoSummaryControl.Background = (Brush) Brushes.Transparent;
            pomoSummaryControl.Cursor = System.Windows.Input.Cursors.Hand;
            pomoSummaryControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            pomoCount = pomoSummaryControl;
            pomoCount.MouseUp += new EventHandler<MouseButtonEventArgs>(taskDetailView.OnTaskPomoClick);
            taskDetailView._titleUpperContent.Children.Add((UIElement) pomoCount);
          }
          pomoCount.Margin = new Thickness(0.0, taskDetailView.GetParentTitle() == null ? -4.0 : 20.0, 0.0, 0.0);
          pomoCount.SetData(pomoByTaskId);
          pomoCount = (TaskDetailPomoSummaryControl) null;
        }
        else
        {
          taskDetailView._titleUpperContent.Children.Remove((UIElement) pomoCount);
          pomoCount = (TaskDetailPomoSummaryControl) null;
        }
      }
    }

    private EmjTextBlock GetParentTitle()
    {
      return Utils.FindSingleVisualChildren<EmjTextBlock>((DependencyObject) this._titleUpperContent);
    }

    private void SetPrentTitle(TaskBaseViewModel parent)
    {
      EmjTextBlock parentTitle = this.GetParentTitle();
      if (parentTitle != null)
      {
        parentTitle.Text = parent.Title;
      }
      else
      {
        Grid grid = new Grid();
        grid.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
        grid.Cursor = System.Windows.Input.Cursors.Hand;
        grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        grid.VerticalAlignment = VerticalAlignment.Top;
        Grid element1 = grid;
        element1.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnParentTitleClick);
        EmjTextBlock emjTextBlock = new EmjTextBlock();
        emjTextBlock.Background = (Brush) Brushes.Transparent;
        emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        emjTextBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        emjTextBlock.Padding = new Thickness(2.0, 2.0, 20.0, 2.0);
        EmjTextBlock element2 = emjTextBlock;
        element2.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body05");
        element2.SetResourceReference(TextBlock.FontSizeProperty, (object) "Font13");
        element2.Text = parent.Title;
        element1.Children.Add((UIElement) element2);
        System.Windows.Shapes.Path arrow = UiUtils.GetArrow(12.0, -90.0, "BaseColorOpacity40");
        arrow.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        element1.Children.Add((UIElement) arrow);
        this._titleUpperContent.Children.Add((UIElement) element1);
      }
    }

    private void OnTaskPomoClick(object sender, MouseButtonEventArgs e)
    {
      if (!this.CheckEnable() || this._task == null || this._task.Status != 0)
        return;
      EscPopup escPopup = new EscPopup();
      escPopup.HorizontalOffset = -5.0;
      escPopup.VerticalOffset = -10.0;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.StaysOpen = false;
      EscPopup popup = escPopup;
      popup.PlacementTarget = sender as UIElement;
      popup.Closed += (EventHandler) ((o, a) =>
      {
        this._popupShowing = false;
        EventHandler<bool> actionPopClosed = this.ActionPopClosed;
        if (actionPopClosed == null)
          return;
        actionPopClosed((object) this, false);
      });
      popup.Opened += (EventHandler) ((o, a) =>
      {
        this._popupShowing = true;
        EventHandler<bool> actionPopClosed = this.ActionPopClosed;
        if (actionPopClosed == null)
          return;
        actionPopClosed((object) this, true);
      });
      TaskPomoSetDialog taskPomoSetDialog = new TaskPomoSetDialog();
      taskPomoSetDialog.Closed += (EventHandler<bool>) ((o, a) => popup.IsOpen = false);
      popup.Child = (UIElement) taskPomoSetDialog;
      taskPomoSetDialog.InitData(this._task.TaskId, true, "task_detail", "task_detail_pomo");
      popup.IsOpen = true;
    }

    private void OnParentTitleClick(object sender, MouseButtonEventArgs e)
    {
      if (string.IsNullOrEmpty(this._task.ParentId))
        return;
      this.Navigate(this._task.ParentId);
      EventHandler<string> taskNavigated = this.TaskNavigated;
      if (taskNavigated != null)
        taskNavigated((object) this, this._task.ParentId);
      e.Handled = true;
    }

    private void TaskDetailScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (this._addCommentCtrl == null || this._commentDisplayCtrl == null || this._addCommentCtrl.EditModel == null)
        return;
      ScrollViewer scrollViewer = (ScrollViewer) sender;
      this._addCommentCtrl.EditModel.RecentScrollHeight = this._taskActivityPanel == null ? scrollViewer.ScrollableHeight - e.VerticalOffset : 0.0;
    }

    public async Task TaskSelect(string taskId, string itemId)
    {
      SearchHelper.DetailChanged = false;
      if (taskId != this._task.TaskId)
        this._detailScrollViewer.ScrollToTop();
      await this.Navigate(taskId, itemId);
      await Task.Delay(10);
      if (!LocalSettings.Settings.InSearch)
        return;
      double firstSearchIndex1 = this._titleText.GetFirstSearchIndex();
      if (firstSearchIndex1 > 0.0)
        this.KeepOffsetInView((object) this._titleText, firstSearchIndex1);
      else if (this._contentText != null)
      {
        double firstSearchIndex2 = this._contentText.GetFirstSearchIndex();
        if (firstSearchIndex2 <= 0.0)
          return;
        this.KeepOffsetInView((object) this._contentText, firstSearchIndex2);
      }
      else
      {
        if (this._descText == null)
          return;
        double firstSearchIndex3 = this._descText.GetFirstSearchIndex();
        if (firstSearchIndex3 > 0.0)
          this.KeepOffsetInView((object) this._descText, firstSearchIndex3);
        else
          this._checklist?.ScrollToSearchOffset();
      }
    }

    public async void TaskAdded(TaskModel task) => await this.Navigate(task.id);

    private void OnMouseScrollOnTaskDetail(object sender, MouseWheelEventArgs e)
    {
      if (this._tagsControl != null && this._tagsControl.AddPopupOpened)
      {
        e.Handled = true;
      }
      else
      {
        if (!this._canScroll || e == null)
          return;
        this._detailScrollViewer.ScrollToVerticalOffset(this._detailScrollViewer.VerticalOffset - (double) e.Delta / 2.0);
      }
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => this.BindEvents();

    public double GetDisplayDateDiff()
    {
      TaskDetailViewModel task1 = this._task;
      if ((task1 != null ? (!task1.StartDate.HasValue ? 1 : 0) : 1) == 0)
      {
        TaskDetailViewModel task2 = this._task;
        if ((task2 != null ? (!task2.DisplayStartDate.HasValue ? 1 : 0) : 1) == 0)
        {
          DateTime? nullable = this._task.DisplayStartDate;
          DateTime dateTime1 = nullable.Value;
          nullable = this._task.StartDate;
          DateTime dateTime2 = nullable.Value;
          return (dateTime1 - dateTime2).TotalDays;
        }
      }
      return 0.0;
    }

    public DateTime? GetDisplayDate()
    {
      if (this._task.Status != 0)
        return new DateTime?();
      TaskDetailViewModel task1 = this._task;
      if ((task1 != null ? (!task1.StartDate.HasValue ? 1 : 0) : 1) == 0)
      {
        TaskDetailViewModel task2 = this._task;
        if ((task2 != null ? (!task2.DisplayStartDate.HasValue ? 1 : 0) : 1) == 0 && !(this._task.StartDate.Value == this._task.DisplayStartDate.Value))
          return this._task.DisplayStartDate;
      }
      return new DateTime?();
    }

    public bool CheckEnable()
    {
      if (this._task.Enable)
        return this._task.Enable;
      if (!TeamDao.IsTeamExpired(this._task.TeamId))
        return !this.IsNoPermission();
      this.TryToast(Utils.GetString("TeamExpiredOperate"));
      return false;
    }

    public void TryToast(string getString)
    {
      Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.TryToastString((object) null, getString);
    }

    private async void ShowSelectDateDialog()
    {
      TaskDetailView sender = this;
      TimeData timeData1 = new TimeData();
      timeData1.StartDate = sender._task.DisplayStartDate;
      timeData1.DueDate = sender._task.DisplayDueDate;
      timeData1.IsAllDay = new bool?(!sender._task.DisplayStartDate.HasValue || ((int) sender._task.IsAllDay ?? 1) != 0);
      TaskReminderModel[] reminders = sender._task.Reminders;
      timeData1.Reminders = reminders != null ? ((IEnumerable<TaskReminderModel>) reminders).ToList<TaskReminderModel>() : (List<TaskReminderModel>) null;
      timeData1.RepeatFrom = sender._task.RepeatFrom;
      timeData1.RepeatFlag = sender._task.RepeatFlag;
      timeData1.ExDates = sender._task.ExDates;
      TimeData timeData = timeData1;
      if (!string.IsNullOrEmpty(sender._task.TimeZoneName))
        timeData.TimeZone = new TimeZoneViewModel(sender._task.IsFloating, sender._task.TimeZoneName);
      if (timeData.IsAllDay.Value && timeData.StartDate.HasValue && timeData.DueDate.HasValue)
      {
        DateTime dateTime = timeData.StartDate.Value;
        DateTime date1 = dateTime.Date;
        dateTime = timeData.DueDate.Value;
        DateTime date2 = dateTime.Date;
        if (date1 == date2)
          timeData.DueDate = new DateTime?();
      }
      if (timeData.StartDate.HasValue)
        timeData.IsDefault = false;
      bool canSkip = !TaskService.IsNonRepeatTask(await TaskDao.GetThinTaskById(sender._task.TaskId));
      bool isNote = sender._task.Kind == "NOTE";
      SetDateDialog dialog = SetDateDialog.GetDialog();
      dialog.ClearEventHandle();
      // ISSUE: reference to a compiler-generated method
      dialog.Clear += new EventHandler(sender.\u003CShowSelectDateDialog\u003Eb__98_0);
      // ISSUE: reference to a compiler-generated method
      dialog.Save += new EventHandler<TimeData>(sender.\u003CShowSelectDateDialog\u003Eb__98_1);
      // ISSUE: reference to a compiler-generated method
      dialog.Hided += new EventHandler(sender.\u003CShowSelectDateDialog\u003Eb__98_2);
      dialog.SkipRecurrence += new EventHandler(sender.OnSkipRecurrence);
      UIElement dateDropTarget = sender._detailHead?.GetDateDropTarget();
      System.Windows.Point point1;
      if (dateDropTarget == null)
      {
        TaskDetailView taskDetailView = sender;
        double x = sender._task.ShowCheckIcon == Visibility.Visible ? 50.0 : 24.0;
        TaskDetailHeadView detailHead = sender._detailHead;
        double y = (detailHead != null ? detailHead.Margin.Top : 0.0) + 32.0;
        System.Windows.Point point2 = new System.Windows.Point(x, y);
        TaskDetailView relativeTo = sender;
        point1 = taskDetailView.TranslatePoint(point2, (UIElement) relativeTo);
      }
      else
        point1 = new System.Windows.Point(-32.0, 22.0);
      System.Windows.Point point3 = point1;
      dialog.Show(timeData, new SetDateDialogArgs(isNote: isNote, target: dateDropTarget ?? (UIElement) sender, hOffset: point3.X, vOffset: point3.Y, canSkip: canSkip));
      UtilLog.Info("DetailShowSetDate");
      sender._popupShowing = true;
      sender.DatePopupShow = true;
      EventHandler actionPopOpened = sender.ActionPopOpened;
      if (actionPopOpened == null)
      {
        timeData = (TimeData) null;
      }
      else
      {
        actionPopOpened((object) sender, (EventArgs) null);
        timeData = (TimeData) null;
      }
    }

    public void PopupOpened(object sender, EventArgs e)
    {
      this._popupShowing = true;
      EventHandler actionPopOpened = this.ActionPopOpened;
      if (actionPopOpened == null)
        return;
      actionPopOpened((object) this, (EventArgs) null);
    }

    public async void PopupClosed(object sender, EventArgs e)
    {
      TaskDetailView sender1 = this;
      sender1._popupShowing = false;
      EventHandler<bool> actionPopClosed = sender1.ActionPopClosed;
      if (actionPopClosed != null)
        actionPopClosed((object) sender1, true);
      await Task.Delay(10);
      if (sender1._popupShowing || !(sender is Popup))
        return;
      if (sender1._needFocusDetail)
        sender1.TryFocusDetail();
      else
        sender1._needFocusDetail = true;
    }

    public void OnMoveProject(EscPopup popup, bool inBatch = false)
    {
      if (this.IsNoPermission())
        return;
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) popup, new ProjectExtra()
      {
        ProjectIds = new List<string>()
        {
          this._task.ProjectId
        }
      }, new ProjectSelectorExtra()
      {
        showAll = false,
        batchMode = false,
        canSelectGroup = false,
        onlyShowPermission = true,
        CanSearch = true,
        ShowColumn = true,
        ColumnId = this._task.ColumnId
      });
      projectOrGroupPopup.ItemSelect += (EventHandler<SelectableItemViewModel>) ((o, e) => this.OnMoveProject(e));
      projectOrGroupPopup.Show();
    }

    private async void OnMoveProject(SelectableItemViewModel e)
    {
      if (e == null)
        return;
      (string str, string columnId) = e.GetProjectAndColumnId();
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == str));
      if (project == null)
        return;
      if (this._task != null && this._task.IsNewAdd)
      {
        this._task.SourceViewModel.ProjectId = project.id;
        this._task.SourceViewModel.ColumnId = columnId;
      }
      else
      {
        this.IsBlank = false;
        UtilLog.Info("TaskDetail.SetProject " + this._task?.TaskId + ", value " + project.id + " from:ProjectSelector");
        this.AddActionEvent("action", "move_project");
        await this.MoveProject(project, columnId);
      }
    }

    private async Task MoveProject(ProjectModel project, string columnId = null)
    {
      await TaskService.TryMoveProject(new MoveProjectArgs(this._task.TaskId, project)
      {
        ColumnId = columnId
      });
      this.TryToastQuadrantChanged(this._task.TaskId);
    }

    private async Task CompleteOrUndoneTask(bool? isComplete = null, bool inBatch = false)
    {
      TaskDetailView child = this;
      child._popupShowing = false;
      int num = child._task.Status != 0 ? 0 : 2;
      isComplete = new bool?(((int) isComplete ?? (num == 2 ? 1 : 0)) != 0);
      if (isComplete.Value && child._task.StartDate.HasValue && child._task.DisplayStartDate.HasValue)
      {
        DateTime dateTime = child._task.StartDate.Value;
        DateTime date1 = dateTime.Date;
        dateTime = child._task.DisplayStartDate.Value;
        DateTime date2 = dateTime.Date;
        if (date1 != date2)
        {
          IToastShowWindow dependentWindow = Utils.FindParent<TaskDetailPopup>((DependencyObject) child)?.DependentWindow;
          if (!await ModifyRepeatHandler.CompleteOrSkipRecurrence(child._task.TaskId, child._task.DisplayStartDate, dependentWindow))
            return;
        }
      }
      await child.ToggleComplete(isComplete.Value);
    }

    private async Task ToggleComplete(bool isComplete)
    {
      int closeStatus = isComplete ? 2 : 0;
      TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(this._task.TaskId, closeStatus, closeStatus == 2, inDetail: true);
      this.SetProgress(this._task.Progress.GetValueOrDefault(), true);
      if (taskCloseExtra?.RepeatTask != null)
      {
        this.Reload(this._task.TaskId);
        this.TryToast(Utils.GetString("CreatedTheNextCircle"));
      }
      else
      {
        if (!isComplete || this.Mode != Constants.DetailMode.Sticky)
          return;
        this.TryToast(Utils.GetString("TaskCompleted"));
      }
    }

    public async void OnSetAssigneeMouseUp(bool inBatch, Popup popup = null)
    {
    }

    private async Task SetAssigneeImage(string assignee, string projectId)
    {
      this._detailHead?.SetAvatarVisible(true, assignee, projectId);
    }

    public async Task SetAssignee(string assignee, bool inBatch)
    {
      this.TryFocusDetail();
      if (this._task.IsNewAdd)
      {
        this._task.SourceViewModel.Assignee = assignee;
      }
      else
      {
        await this.UpdateTaskWithUndo(SetAssign());
        this.SetAssigneeImage(assignee, this._task.ProjectId);
      }

      async Task SetAssign()
      {
        await Task.Yield();
        await TaskService.SetAssignee(this._task.TaskId, assignee);
      }
    }

    public void OnBackClick()
    {
      if (this.Mode == Constants.DetailMode.Page)
      {
        this._detailHead?.SetBackIcon(false);
        Utils.FindParent<TaskView>((DependencyObject) this)?.TryFoldDetail();
      }
      else
      {
        EventHandler navigateBack = this.NavigateBack;
        if (navigateBack == null)
          return;
        navigateBack((object) this, (EventArgs) null);
      }
    }

    public async void OnDelete()
    {
      TaskDetailView taskDetailView = this;
      if (taskDetailView.IsNoPermission())
        return;
      taskDetailView.OnBackClick();
      taskDetailView.AddActionEvent("om", "delete");
      UtilLog.Info("TaskDetail.DeleteTask " + taskDetailView._task.TaskId);
      if (!string.IsNullOrEmpty(taskDetailView._task.AttendId))
      {
        if (!await TaskOperationHelper.CheckIfAllowDeleteTask(taskDetailView._task.TaskId, (DependencyObject) taskDetailView))
          return;
        await taskDetailView.DeleteAgenda();
      }
      else
      {
        TaskDetailViewModel task = taskDetailView._task;
        if ((task != null ? (task.RepeatDiff.HasValue ? 1 : 0) : 0) != 0)
        {
          TaskDetailPopup parent = Utils.FindParent<TaskDetailPopup>((DependencyObject) taskDetailView);
          if (parent != null)
          {
            ModifyRepeatHandler.TryDeleteRecurrence(taskDetailView._task.TaskId, taskDetailView._task.DisplayStartDate, taskDetailView._task.DisplayDueDate, parent.DependentWindow);
            return;
          }
        }
        EventHandler<string> undoOnTaskDeleted = taskDetailView.ShowUndoOnTaskDeleted;
        if (undoOnTaskDeleted == null)
          return;
        undoOnTaskDeleted((object) taskDetailView, taskDetailView._task.TaskId);
      }
    }

    private async Task DeleteAgenda()
    {
      TaskDetailView sender = this;
      await TaskService.DeleteAgenda(sender._task.TaskId, sender._task.ProjectId, sender._task.AttendId);
      EventHandler<string> taskDeleted = sender.TaskDeleted;
      if (taskDeleted != null)
        taskDeleted((object) sender, sender._task.TaskId);
      sender.OnBackClick();
    }

    private async Task DeleteTaskForever(int status = 1)
    {
      TaskDetailView sender = this;
      await TaskService.DeleteTask(sender._task.TaskId, status);
      EventHandler<string> taskDeleted = sender.TaskDeleted;
      if (taskDeleted != null)
        taskDeleted((object) sender, sender._task.TaskId);
      EventHandler notifyCloseWindow = sender.NotifyCloseWindow;
      if (notifyCloseWindow != null)
        notifyCloseWindow((object) sender, (EventArgs) null);
      sender.OnBackClick();
    }

    public async void OnRestoreProject()
    {
      if (this.IsNoPermission())
        return;
      await TaskService.BatchRestoreProject(new List<string>()
      {
        this._task.TaskId
      });
    }

    public void AddActionEvent(string ctype, string label)
    {
      UserActCollectUtils.AddClickEvent("task_detail", ctype, label);
    }

    public void OnStarClick()
    {
      this.AddActionEvent("om", "pin_or_unpin");
      this.OnPin();
    }

    public void SetNeedFocusDetail(bool e) => this._needFocusDetail = e;

    public void SetPopupShowing(bool show) => this._popupShowing = show;

    private void TryDisplayTags()
    {
      this.LoadTags((IReadOnlyCollection<string>) TagSerializer.ToTags(this._task.Tag));
    }

    private async Task UpdateTaskWithUndo(Task t)
    {
      if (this.ParentIdentity is FilterProjectIdentity)
      {
        TaskModel task = await TaskDao.GetTaskById(this._task.TaskId);
        await t;
        this.TryToastTaskChangeUndo(task);
        task = (TaskModel) null;
      }
      else
        await t;
    }

    private void TryToastTaskChangeUndo(TaskModel task)
    {
      if (task == null)
        return;
      if (TaskViewModelHelper.GetMatchedTasks(this.ParentIdentity, new List<string>()
      {
        task.id
      }).Count != 0)
        return;
      Utils.FindParent<IToastShowWindow>((DependencyObject) this).Toast((FrameworkElement) new UndoToast((UndoController) new TaskUndo(task, string.Empty, Utils.GetString("TaskHasBeenFiltered"))));
      UndoHelper.NeedToastFilteredTaskId = string.Empty;
    }

    private async Task TryToastQuadrantChanged(string taskId)
    {
      if (this._quadrantLevel == 0 || this._task.IsNewAdd)
        return;
      (int num, string getString) = await MatrixManager.GetTaskQuadrantChangeString(this._quadrantLevel, taskId);
      this._quadrantLevel = num;
      if (string.IsNullOrEmpty(getString))
        return;
      this.TryToast(getString);
    }

    public void Reload(string taskId = null, bool loadComment = true)
    {
      DelayActionHandlerCenter.TryDoAction(this._uid + nameof (Reload), (EventHandler) ((o, e) => this.Dispatcher.Invoke((Action) (() =>
      {
        if (!string.IsNullOrEmpty(taskId) && !(taskId == this._task.TaskId))
          return;
        this.Navigate(this._task.TaskId, loadComment: loadComment);
      }))), 150);
    }

    internal void SetInQuadrant(int quadrantLevel) => this._quadrantLevel = quadrantLevel;

    public int GetQuadrantLevel() => this._quadrantLevel;

    private async Task TryDelayEditSaveSync()
    {
      await this.SaveTask();
      this.DelaySync();
    }

    private async void DelaySync()
    {
      if (this._popupShowing)
        return;
      SyncManager.TryDelaySync();
    }

    public async void EnterImmersivePage()
    {
      if (!this._task.Enable)
        return;
      EnterImmersiveDelegate enterImmersive = this.EnterImmersive;
      if (enterImmersive == null)
        return;
      string taskId = this._task.TaskId;
      MarkDownEditor contentText = this._contentText;
      int caretOffset = contentText != null ? contentText.EditBox.CaretOffset : 0;
      enterImmersive(taskId, caretOffset);
    }

    public void ShowBackMenu()
    {
      this._detailHead?.SetBackIcon(true);
      this._textEditorMenu?.HideImmersive();
    }

    public void HideBackMenu()
    {
      this._detailHead?.SetBackIcon(false);
      this._textEditorMenu?.ShowImmersive();
    }

    public async Task Navigate(string taskId, string itemId = "", bool forceLoad = true, bool loadComment = true)
    {
      if (!forceLoad && this._task.TaskId != taskId)
        return;
      bool hideEdit = this._task.TaskId != taskId;
      if (string.IsNullOrEmpty(taskId))
        return;
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      TaskModel taskModel = await TaskDao.AssembleFullTask(task);
      if (task != null)
      {
        await this.Navigate(new TaskDetailViewModel(task), itemId, hideEdit, loadComment);
        await Task.Delay(50);
        if (LocalSettings.Settings.InSearch && this.Mode == Constants.DetailMode.Page)
        {
          DetailTextBox titleText = this._titleText;
          double offset = titleText != null ? titleText.GetFirstSearchIndex() : 0.0;
          if (offset > 0.0)
          {
            this.KeepOffsetInView((object) this._titleText, offset);
            return;
          }
          if (this._contentText != null && this._contentText.Visibility == Visibility.Visible)
          {
            double firstSearchIndex = this._contentText.GetFirstSearchIndex();
            if (firstSearchIndex <= 0.0)
              return;
            this.KeepOffsetInView((object) this._contentText, firstSearchIndex);
            return;
          }
          if (this._descText != null && this._descText.Visibility == Visibility.Visible)
          {
            double firstSearchIndex = this._descText.GetFirstSearchIndex();
            if (firstSearchIndex > 0.0)
            {
              this.KeepOffsetInView((object) this._descText, firstSearchIndex);
              return;
            }
            this._checklist?.ScrollToSearchOffset();
          }
        }
      }
      task = (TaskModel) null;
    }

    public async Task Navigate(
      TaskDetailViewModel taskModel,
      string itemId = "",
      bool hideEditor = true,
      bool loadComment = true)
    {
      await this.Dispatcher.Invoke<Task>((Func<Task>) (async () =>
      {
        this._navigating = true;
        UtilLog.Info("NavigateDetail TaskId: " + taskModel.TaskId + " ,commentCount " + taskModel.CommentCount);
        this._switchListOriginText = (string) null;
        this.IsBlank = true;
        taskModel.Element = (FrameworkElement) this;
        taskModel.NavigateItemId = itemId;
        if (taskModel.TaskId != this._task?.TaskId)
          this.ClearSaveItems();
        this.RemoveVmChangeEvent(this._task);
        this.DataContext = (object) taskModel;
        this.TryShowGuide();
        this.SetVmChangeEvent(taskModel);
        bool restoreOffset = this._task?.TaskId == taskModel.TaskId && (this._contentText != null && this._contentText.CurrentFocused || this._titleText.KeyboardFocused || this._descText != null && this._descText.KeyboardFocused);
        await this.InitInputViews(taskModel, restoreOffset);
        this.InitControl();
        this.SetProgress(this._task.Progress.GetValueOrDefault());
        this.LoadAttachmentAsync();
        this.TryDisplayTags();
        this.TryLoadAttendees();
        this.SetMoreOptionMenuOnSingleTask();
        this.TryLoadComments(loadRemote: loadComment);
        this.GetParentAndChildren(taskModel);
        this.SetUpperContent(taskModel);
        this._detailHead?.SetRightIcons(this._task.IsNewAdd, this._task.IsNote);
        this._detailHead?.SetIconColor(this._task.Priority, this._task.Status);
        this.SetAvatar();
        this.CloseActivityPanel();
        this.LoadContentView(taskModel, itemId, restoreOffset);
        if (restoreOffset)
          this._contentText?.FixCurrentIndex();
        if (this._textEditorMenu != null & hideEditor || !this._task.Enable)
          this.SetTextEditorMenu(false);
        this._navigating = false;
      }));
    }

    private void InitControl()
    {
      this._detailBottom?.OnDataBind(this._task);
      this.SetStarGrid();
    }

    public void SetImmerse()
    {
      if (this.Mode == Constants.DetailMode.Editor)
      {
        if (this._detailHead != null)
          this._detailHead.Visibility = Visibility.Collapsed;
        if (this._commentDisplayCtrl != null)
          this._commentDisplayCtrl.Visibility = Visibility.Collapsed;
        if (this._addCommentCtrl != null)
          this._addCommentCtrl.Visibility = Visibility.Collapsed;
        this._detailBottom.Visibility = Visibility.Collapsed;
      }
      else
      {
        if (this._detailHead != null)
          this._detailHead.Visibility = Visibility.Visible;
        this._detailBottom.Visibility = Visibility.Visible;
      }
    }

    public async void TryLoadAttendees()
    {
      if (Utils.IsDida())
      {
        this._task.IsOwner = string.IsNullOrEmpty(this._task.AttendId) || AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) this._task);
        if (this._detailBottom != null)
          await this._detailBottom.LoadAttendees(this._task, this.Mode);
        if (this._contentText != null)
          this._contentText.EditBox.IsEnabled = this._task.IsOwner;
        if (this._descText == null)
          return;
        this._descText.IsEnabled = this._task.IsOwner;
      }
      else
        this._task.IsOwner = true;
    }

    private void SetProgress(int progress, bool withAnim = false)
    {
      if (this._task.Status == 0)
        this._detailHead?.SetProgress(progress > 0 ? progress : 0, withAnim);
      else
        this._detailHead?.SetProgress(0);
    }

    private void SetMoreOptionMenuOnSingleTask()
    {
      string tag = (string) null;
      bool visible = true;
      if (this._task.Status != 0 || !string.IsNullOrEmpty(this._task.AttendId))
        visible = false;
      else if (TaskCache.IsParentTask(this._task.TaskId) || !string.IsNullOrEmpty(this._task.ParentId))
        tag = Utils.GetString("CannotConvertMultiLevelToNote");
      this._detailBottom?.SetSwitchTagAndVisible(tag, visible);
    }

    private async void GetParentAndChildren(TaskDetailViewModel taskModel)
    {
      await Task.Delay(50);
      this.SetSubtasks(taskModel, false);
    }

    private async void SetAvatar(string assignee = null, string projectId = null)
    {
      assignee = assignee ?? this._task.Assignee;
      projectId = projectId ?? this._task.ProjectId;
      if (string.IsNullOrEmpty(projectId))
      {
        this._detailHead?.SetAvatarVisible(false);
      }
      else
      {
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.ProjectId == projectId));
        bool showAssign = false;
        if (projectModel != null && projectModel.IsShareList() && (string.IsNullOrEmpty(this._task.AttendId) || AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) this._task)))
          showAssign = true;
        this._detailHead?.SetAvatarVisible(showAssign, assignee, projectId);
      }
    }

    private void CloseActivityPanel() => this.ToggleTaskActivityPanel(false);

    private async void LoadContentView(
      TaskDetailViewModel taskModel,
      string itemId = "",
      bool restoreOffset = false)
    {
      this.TryShowOptionMenu();
      if (this._task.Kind == "CHECKLIST")
      {
        this.LoadChecklistView(taskModel, itemId);
        this._descText?.RegisterCaretChanged();
      }
      else
      {
        if (this._contentText == null)
          return;
        this._contentText.CurrentFocused = restoreOffset;
        this._contentText.RegisterCaretChanged();
      }
    }

    private void TryShowOptionMenu()
    {
      this._detailBottom?.SetItemVisible("EditorIcon", this._task.IsOwner && this._task.Enable && !this._task.IsNewAdd);
    }

    private async void LoadChecklistView(TaskDetailViewModel model, string itemId)
    {
      List<CheckItemViewModel> checklistItems;
      if (model.Kind != "CHECKLIST")
      {
        checklistItems = (List<CheckItemViewModel>) null;
      }
      else
      {
        checklistItems = new List<CheckItemViewModel>();
        List<TaskDetailItemModel> taskDetailItemModelList;
        if (model.Items != null && ((IEnumerable<TaskDetailItemModel>) model.Items).Any<TaskDetailItemModel>())
        {
          taskDetailItemModelList = ((IEnumerable<TaskDetailItemModel>) model.Items).ToList<TaskDetailItemModel>();
        }
        else
        {
          taskDetailItemModelList = await TaskDetailItemDao.GetCheckItemsByTaskId(model.TaskId);
          if (taskDetailItemModelList == null || taskDetailItemModelList.Count == 0)
          {
            await Task.Delay(1000);
            if (model.Kind != "CHECKLIST")
            {
              checklistItems = (List<CheckItemViewModel>) null;
              return;
            }
            await TaskService.GetRemoteTaskCheckItems(model.ProjectId, model.TaskId);
            taskDetailItemModelList = await TaskDetailItemDao.GetCheckItemsByTaskId(model.TaskId);
          }
          model.Items = taskDetailItemModelList?.ToArray();
        }
        if (taskDetailItemModelList != null && taskDetailItemModelList.Any<TaskDetailItemModel>())
        {
          List<IGrouping<string, TaskDetailItemModel>> list = taskDetailItemModelList.GroupBy<TaskDetailItemModel, string>((Func<TaskDetailItemModel, string>) (i => i.id)).ToList<IGrouping<string, TaskDetailItemModel>>();
          bool flag = false;
          foreach (IGrouping<string, TaskDetailItemModel> source in list)
          {
            if (source.Count<TaskDetailItemModel>() > 1)
            {
              flag = true;
              TaskDetailItemDao.ClearSameItemById(source.Key);
            }
          }
          if (flag)
            taskDetailItemModelList = list.Select<IGrouping<string, TaskDetailItemModel>, TaskDetailItemModel>((Func<IGrouping<string, TaskDetailItemModel>, TaskDetailItemModel>) (g => g.FirstOrDefault<TaskDetailItemModel>())).ToList<TaskDetailItemModel>();
          this._task.SourceViewModel.CheckCheckItems(taskDetailItemModelList);
          foreach (TaskBaseViewModel vm in this._task.SourceViewModel.CheckItems.Value.ToList<TaskBaseViewModel>())
          {
            if (vm.Deleted == 0)
              checklistItems.Add(new CheckItemViewModel(vm, model)
              {
                IsAgendaOwner = this._task.IsOwner,
                Enable = this._task.IsOwner && this._task.Enable,
                IsValid = true
              });
          }
        }
        this.SetCheckItems(checklistItems);
        if (string.IsNullOrEmpty(itemId))
        {
          checklistItems = (List<CheckItemViewModel>) null;
        }
        else
        {
          await Task.Delay(100);
          ChecklistControl checklist = this._checklist;
          if (checklist == null)
          {
            checklistItems = (List<CheckItemViewModel>) null;
          }
          else
          {
            checklist.ScrollToItem(itemId);
            checklistItems = (List<CheckItemViewModel>) null;
          }
        }
      }
    }

    private void SetCheckItems(List<CheckItemViewModel> source)
    {
      if (source != null && source.Count == 1)
        source[0].ShowAddHint = true;
      this._checklist?.SetTaskId(this._task.TaskId);
      this._checklist?.SetData(source);
    }

    private void RemoveVmChangeEvent(TaskDetailViewModel taskModel)
    {
      if (taskModel == null)
        return;
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) taskModel, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), "");
    }

    private void SetVmChangeEvent(TaskDetailViewModel taskModel)
    {
      if (taskModel == null)
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) taskModel, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), "");
    }

    public void ScrollToTop() => this._detailScrollViewer.ScrollToTop();

    internal void SctollToBottom() => this._detailScrollViewer.ScrollToBottom();

    public async void SelectDateClick(MouseButtonEventArgs e)
    {
      if (this.DatePopupShow)
      {
        SetDateDialog dialog = SetDateDialog.GetDialog();
        if ((dialog != null ? (dialog.IsOpen() ? 1 : 0) : 0) == 0)
          return;
      }
      if (e.Handled || !this.CheckEnable())
        return;
      if (this._task.IsOwner)
      {
        this.ShowSelectDateDialog();
        e.Handled = true;
      }
      else
        this.TryToast(Utils.GetString("AttendeeSetDate"));
    }

    private async Task ClearTaskDate()
    {
      TaskDetailView parent = this;
      if (!TaskOperationHelper.CheckIfAgendaAllowClearDate(parent._task.AttendId, (DependencyObject) parent))
        return;
      await parent.DoClearTaskDate();
      parent.TryToastQuadrantChanged(parent._task.TaskId);
    }

    private async Task DoClearTaskDate()
    {
      TaskDetailView sender = this;
      sender._task.Reminders = (TaskReminderModel[]) null;
      sender._task.ExDates = (List<string>) null;
      await sender.UpdateTaskWithUndo(SetTime());
      TaskChangeNotifier.NotifyTaskDateChanged(sender._task.TaskId, (object) sender);
      if (!sender._titleText.TextArea.IsKeyboardFocused)
        return;
      sender.OnTitleGotFocus((object) null, (RoutedEventArgs) null);

      async Task SetTime()
      {
        await Task.Yield();
        if (!string.IsNullOrEmpty(this._task.AttendId))
          await TaskService.ClearAgendaDate(this._task.TaskId);
        else
          await TaskService.ClearDate(this._task.TaskId);
      }
    }

    private async Task SaveTaskDate(TimeData model)
    {
      TaskDetailView sender = this;
      sender.CheckIfReminderPassed(model);
      sender._task.Reminders = model.Reminders?.ToArray() ?? new TaskReminderModel[0];
      sender._task.ExDates = new List<string>();
      if (!sender._task.IsNewAdd)
      {
        await sender.UpdateTaskWithUndo(SetTime());
        TaskChangeNotifier.NotifyTaskDateChanged(sender._task.TaskId, (object) sender);
        sender.TryToastQuadrantChanged(sender._task.TaskId);
      }
      else
        sender._task.SetTimeData(model);

      async Task SetTime()
      {
        await Task.Yield();
        await TaskService.SetDate(this._task.TaskId, model);
      }
    }

    private void CheckIfReminderPassed(TimeData model)
    {
      if (model == null || !model.IsAllDay.HasValue || model.IsAllDay.Value || !model.StartDate.HasValue || !(model.StartDate.Value < DateTime.Now))
        return;
      List<TaskReminderModel> reminders = model.Reminders;
      // ISSUE: explicit non-virtual call
      if ((reminders != null ? (__nonvirtual (reminders.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      this.TryToast(Utils.GetString("InvalidReminder"));
    }

    public async void PrioritySelect(int priority)
    {
      this.IsBlank = false;
      this.AddActionEvent("action", nameof (priority));
      this.SetTaskPriority(priority);
    }

    private async void SetTaskPriority(int priority)
    {
      if (this._task.SourceViewModel.Priority == priority)
        ;
      else if (this._task.IsNewAdd)
      {
        this._task.SourceViewModel.Priority = priority;
      }
      else
      {
        await this.UpdateTaskWithUndo(SetPri());
        this.TryToastQuadrantChanged(this._task.TaskId);
        if (this.Mode != Constants.DetailMode.Sticky)
          ;
        else
        {
          string getString = Utils.GetString("SetAsNonePriority");
          switch (priority)
          {
            case 1:
              getString = Utils.GetString("SetAsLowPriority");
              break;
            case 3:
              getString = Utils.GetString("SetAsMediumPriority");
              break;
            case 5:
              getString = Utils.GetString("SetAsHighPriority");
              break;
          }
          this.TryToast(getString);
        }
      }

      async Task SetPri()
      {
        await Task.Yield();
        await TaskService.SetPriority(this._task.TaskId, priority);
      }
    }

    private async Task SaveTask(CheckMatchedType checkType = CheckMatchedType.None)
    {
      TaskDetailView taskDetailView = this;
      if (taskDetailView._task.IsNewAdd || string.IsNullOrEmpty(taskDetailView._task.TaskId))
        return;
      if (string.IsNullOrEmpty(taskDetailView._task.TaskId))
      {
        UtilLog.Info(string.Format("IsVisible {0},Id {1}", (object) taskDetailView.IsVisible, (object) taskDetailView._task?.TaskId));
      }
      else
      {
        TaskModel taskModel = await TaskDetailViewModel.ToTaskModel(taskDetailView._task);
        if (taskModel == null)
          return;
        taskModel.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnDetailChanged(taskModel, checkType);
        await SyncStatusDao.AddModifySyncStatus(taskDetailView._task.TaskId);
      }
    }

    public bool IsNoPermission(bool isComment = false)
    {
      switch (this._task.Permission)
      {
        case "comment":
          if (!isComment)
          {
            this.TryToast(string.Format(Utils.GetString("NoPermissionToEdit"), (object) Utils.GetString("CanComment")));
            return true;
          }
          break;
        case "read":
          this.TryToast(string.Format(Utils.GetString("NoPermissionToEdit"), (object) Utils.GetString("ReadOnly")));
          return true;
      }
      return false;
    }

    private void OnDetailKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.V)
      {
        if (Utils.IfCtrlPressed())
        {
          try
          {
            if (this.CheckPasteEnable())
            {
              UtilLog.Info(string.Format("StartPastFileCaret {0},{1},{2},{3}", (object) this._contentText?.CurrentFocused, (object) this._contentText?.KeyboardFocused, (object) this._contentText?.EditBox.CaretOffset, (object) this._contentText?.EditBox.Text.Length));
              BitmapSource pastImage = this.GetPastImage();
              if (pastImage != null)
              {
                e.Handled = true;
                this.TryPasteImage(pastImage);
                return;
              }
              List<string> pastFiles = FileUtils.GetPastFiles();
              if (pastFiles != null)
              {
                if (pastFiles.Any<string>())
                {
                  e.Handled = true;
                  this.TryPasteFile((IReadOnlyCollection<string>) pastFiles);
                  return;
                }
              }
            }
          }
          catch (Exception ex)
          {
            this.TryToast(Utils.GetString("FileNotFound"));
          }
        }
      }
      if (e.Key != Key.D0 || !Utils.IfCtrlPressed() || !Utils.IfShiftPressed() || this.Mode != Constants.DetailMode.Page)
        return;
      this.EnterImmersivePage();
    }

    public async Task OnCopy(bool inBatch = false)
    {
      TaskDetailView sender = this;
      sender.AddActionEvent("om", "duplicate");
      if (await ProChecker.CheckTaskLimit(sender._task.ProjectId))
        return;
      TaskModel taskModel = await TaskService.CopyTask(sender._task.TaskId, delayNotify: true);
      EventHandler<string> taskCopied = sender.TaskCopied;
      if (taskCopied != null)
        taskCopied((object) sender, taskModel.id);
      taskModel.projectId = sender._task.ProjectId;
      taskModel.Color = sender._task.Color;
      if (sender.Mode != Constants.DetailMode.Page)
        return;
      await sender.Navigate(taskModel.id);
    }

    private async void OnSkipRecurrence(object sender, EventArgs e)
    {
      TaskDetailView taskDetailView = this;
      if (taskDetailView._task.DisplayStartDate.HasValue)
      {
        DateTime? displayStartDate = taskDetailView._task.DisplayStartDate;
        DateTime? nullable = taskDetailView._task.StartDate;
        if ((displayStartDate.HasValue == nullable.HasValue ? (displayStartDate.HasValue ? (displayStartDate.GetValueOrDefault() != nullable.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0)
        {
          string taskId = taskDetailView._task.TaskId;
          nullable = taskDetailView._task.DisplayStartDate;
          DateTime date = nullable.Value.Date;
          IToastShowWindow dependentWindow = Window.GetWindow((DependencyObject) taskDetailView) is TaskDetailWindow window ? window.DependentWindow : (IToastShowWindow) null;
          await TaskService.SkipRecurrenceByDate(taskId, date, dependentWindow);
          TaskDetailWindow.TryCloseWindow();
          return;
        }
      }
      await taskDetailView.UpdateTaskWithUndo(SetTime());

      async Task SetTime()
      {
        TaskDetailView taskDetailView = this;
        await Task.Yield();
        TaskModel taskModel = await TaskService.SkipCurrentRecurrence(taskDetailView._task.TaskId, toastWindow: (IToastShowWindow) (Window.GetWindow((DependencyObject) taskDetailView) as TaskDetailWindow));
      }
    }

    private async Task OnDateSelected(DateTime time)
    {
      TaskDetailView sender = this;
      if (!string.IsNullOrEmpty(sender._task.AttendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) sender._task))
      {
        sender.TryToast(Utils.GetString("AttendeeSetDate"));
      }
      else
      {
        TaskDetailViewModel task = sender._task;
        if ((task != null ? (task.RepeatDiff.HasValue ? 1 : 0) : 0) != 0)
        {
          TimeData timeData1 = new TimeData();
          timeData1.StartDate = sender._task.DisplayStartDate;
          timeData1.DueDate = sender._task.DisplayDueDate;
          timeData1.IsAllDay = new bool?(!sender._task.DisplayStartDate.HasValue || ((int) sender._task.IsAllDay ?? 1) != 0);
          TaskReminderModel[] reminders = sender._task.Reminders;
          timeData1.Reminders = reminders != null ? ((IEnumerable<TaskReminderModel>) reminders).ToList<TaskReminderModel>() : (List<TaskReminderModel>) null;
          timeData1.RepeatFrom = sender._task.RepeatFrom;
          timeData1.RepeatFlag = sender._task.RepeatFlag;
          timeData1.ExDates = sender._task.ExDates;
          TimeData timeData2 = timeData1;
          ModifyRepeatHandler.TryUpdateDueDateOnlyDate(sender._task.TaskId, sender._task.DisplayStartDate, sender._task.DisplayDueDate, timeData2, time, 1, 1);
        }
        else
        {
          await sender.UpdateTaskWithUndo(SetTime());
          TaskChangeNotifier.NotifyTaskDateChanged(sender._task.TaskId, (object) sender);
          sender.TryToastQuadrantChanged(sender._task.TaskId);
        }
      }

      async Task SetTime()
      {
        await Task.Yield();
        TaskModel taskModel = await TaskService.SetStartDate(this._task.TaskId, time);
      }
    }

    public async void OnTagsSelected(TagSelectData data, bool inBatch)
    {
      UtilLog.Info(string.Format("TaskDetail.SetTag {0} from:TagWindow", (object) this._task?.Id));
      this.NotifyTagsChanged(data.OmniSelectTags);
    }

    private void OnTagAdded(object sender, string tag)
    {
      List<string> tags = TagSerializer.ToTags(this._task.Tag);
      if (tags.Contains(tag))
        return;
      tags.Add(tag);
      this.NotifyTagsChanged(tags);
    }

    private void OnTagsChanged(object sender, List<string> tags)
    {
      this.OnTagPanelVisibleChanged((object) this, Visibility.Collapsed);
      this.NotifyTagsChanged(tags);
    }

    private async void NotifyTagsChanged(List<string> tags)
    {
      this.IsBlank = false;
      if (!this._task.IsNewAdd)
      {
        await this.UpdateTaskWithUndo(SetTag());
        SyncManager.TryDelaySync();
        this.TryToastQuadrantChanged(this._task.TaskId);
      }
      else
        this._task.SourceViewModel.SetTags(tags);

      async Task SetTag()
      {
        await Task.Yield();
        await TaskService.SetTags(this._task.TaskId, tags);
      }
    }

    private async Task ToggleTaskActivityPanel(bool show)
    {
      TaskDetailView taskDetailView = this;
      if (show)
      {
        bool isNote = taskDetailView._task.Kind == "NOTE";
        if (!ProChecker.CheckPro(isNote ? ProType.NoteActivities : ProType.TaskActivities))
          return;
        if (!Utils.IsNetworkAvailable())
        {
          taskDetailView.TryToast(Utils.GetString("NoNetwork"));
        }
        else
        {
          TaskActivityPanel activityPanel = taskDetailView._taskActivityPanel;
          if (activityPanel == null)
          {
            TaskActivityPanel taskActivityPanel = new TaskActivityPanel();
            taskActivityPanel.Margin = new Thickness(20.0, 0.0, 20.0, 0.0);
            activityPanel = taskActivityPanel;
            taskDetailView._taskActivityPanel = activityPanel;
            if (taskDetailView.Mode == Constants.DetailMode.Popup)
              activityPanel.ActivityControl.Items.MaxHeight = 200.0;
            activityPanel.Closed += new EventHandler(taskDetailView.OnCloseActivityClick);
            activityPanel.SetValue(Grid.RowProperty, (object) 2);
            taskDetailView.Children.Add((UIElement) activityPanel);
          }
          UserInfoModel userInfo = await UserManager.GetUserInfo();
          string userName = LocalSettings.Settings.LoginUserName;
          if (userInfo != null)
            userName = string.IsNullOrEmpty(userInfo.name) ? userInfo.username : userInfo.name;
          bool isAgendaCopy = false;
          TaskModel task = await TaskDao.GetThinTaskById(taskDetailView._task.TaskId);
          if (task != null)
            isAgendaCopy = !string.IsNullOrEmpty(task.attendId) && task.attendId != task.id;
          string taskActivities = await Communicator.GetTaskActivities(taskDetailView._task.ProjectId, taskDetailView._task.TaskId);
          int num = 3 * (int) taskDetailView.ActualWidth / 4;
          try
          {
            List<TaskModifyModel> taskModifyModelList = JsonConvert.DeserializeObject<List<TaskModifyModel>>(taskActivities);
            if (taskModifyModelList != null)
            {
              activityPanel.ActivityCountText.Text = taskModifyModelList.Count.ToString();
              List<TaskActivityViewModel> vms = new List<TaskActivityViewModel>();
              DateTime date = new DateTime();
              for (int index = taskModifyModelList.Count - 1; index >= 0; --index)
              {
                if (taskModifyModelList[index].action == "C_CONTENT")
                {
                  DateTime result;
                  DateTime.TryParse(taskModifyModelList[index].when, out result);
                  if (!Utils.IsEmptyDate(result) && (Utils.IsEmptyDate(date) || (date - result).TotalMinutes >= 5.0))
                    date = result;
                  else
                    continue;
                }
                if (!taskModifyModelList[index].action.StartsWith("AT") && !taskDetailView.IsInvalidAgendaAction(taskModifyModelList[index].action, isAgendaCopy))
                  vms.Add(new TaskActivityViewModel(taskModifyModelList[index], userName, isAgendaCopy, task.Floating, task.kind)
                  {
                    MaxDescLength = num
                  });
              }
              activityPanel.ActivityControl.SetItems(vms);
            }
          }
          catch (Exception ex)
          {
          }
          activityPanel.ActivitiesName.Text = Utils.GetString(isNote ? "NoteActivities" : "TaskActivities");
          activityPanel = (TaskActivityPanel) null;
          userName = (string) null;
          task = (TaskModel) null;
        }
      }
      else
      {
        if (taskDetailView._taskActivityPanel == null)
          return;
        taskDetailView.Children.Remove((UIElement) taskDetailView._taskActivityPanel);
        taskDetailView._taskActivityPanel.Closed -= new EventHandler(taskDetailView.OnCloseActivityClick);
        taskDetailView._taskActivityPanel = (TaskActivityPanel) null;
      }
    }

    private bool IsInvalidAgendaAction(string action, bool isAgendaCopy)
    {
      if (!isAgendaCopy)
        return false;
      return action == "T_DONE" || action == "T_UNDONE" || action == "T_REPEAT";
    }

    private void OnCloseActivityClick(object sender, EventArgs e)
    {
      this.ToggleTaskActivityPanel(false);
    }

    private void OnDetailSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this._detailBottom?.AttendeePanel.NotifyWidthChanged(this.ActualWidth);
      if (this._task == null || !(this._task.Kind == "CHECKLIST"))
        return;
      this.LoadAttachmentAsync();
    }

    public void OnPomoCompleted(object sender, List<string> taskIds)
    {
      // ISSUE: explicit non-virtual call
      if (this._task?.TaskId == null || this.Visibility != Visibility.Visible || taskIds == null || !__nonvirtual (taskIds.Contains(this._task.TaskId)))
        return;
      this.Reload();
    }

    public void OnTasksDeleted(List<string> taskIds)
    {
      if (this._subTaskList == null || !this._subTaskList.DisplayModels.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => taskIds.Contains(m.Id))))
        return;
      this.SetSubtasks(this._task);
    }

    public bool TryClosePopup()
    {
      if (this._detailBottom == null || !this._detailBottom.MorePopup.IsOpen)
        return false;
      this._detailBottom.MorePopup.IsOpen = false;
      return true;
    }

    public void SetNavigate() => this._detailBottom?.SetNavigate();

    public void SetEditMenuMode(bool inImmerse)
    {
      if (inImmerse)
      {
        if (this._textEditorMenu == null)
          this.SetTextEditorMenu(true);
        this._textEditorMenu.Opacity = 0.24;
        this._scrollButtonRow.Height = new GridLength(24.0);
        this._textEditorMenu.Margin = new Thickness(0.0, 0.0, 0.0, 15.0);
        this._textEditorMenu.MouseEnter -= new System.Windows.Input.MouseEventHandler(this.OnMenuMouseEnter);
        this._textEditorMenu.MouseEnter += new System.Windows.Input.MouseEventHandler(this.OnMenuMouseEnter);
        this._textEditorMenu.MouseLeave -= new System.Windows.Input.MouseEventHandler(this.OnMenuMouseLeave);
        this._textEditorMenu.MouseLeave += new System.Windows.Input.MouseEventHandler(this.OnMenuMouseLeave);
        this._textEditorMenu.IsImmersiveMode = true;
      }
      else
      {
        if (this._textEditorMenu != null)
        {
          this._textEditorMenu.Margin = new Thickness(0.0);
          this._textEditorMenu.MouseEnter -= new System.Windows.Input.MouseEventHandler(this.OnMenuMouseEnter);
          this._textEditorMenu.MouseLeave -= new System.Windows.Input.MouseEventHandler(this.OnMenuMouseLeave);
          this._textEditorMenu.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
          this._textEditorMenu.Opacity = 1.0;
          this._textEditorMenu.IsImmersiveMode = false;
        }
        this._scrollButtonRow.Height = new GridLength(0.0);
      }
    }

    private void OnMenuMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this._textEditorMenu == null)
        return;
      Storyboard storyboard = new Storyboard();
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.AutoReverse = false;
      doubleAnimation.FillBehavior = FillBehavior.HoldEnd;
      doubleAnimation.From = new double?(0.23999999463558197);
      doubleAnimation.To = new double?(1.0);
      doubleAnimation.Duration = (Duration) TimeSpan.FromMilliseconds(120.0);
      DoubleAnimation element = doubleAnimation;
      storyboard.Children.Add((Timeline) element);
      Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath((object) UIElement.OpacityProperty));
      storyboard.Begin((FrameworkElement) this._textEditorMenu);
    }

    private void OnMenuMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this._textEditorMenu == null)
        return;
      Storyboard storyboard = new Storyboard();
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.AutoReverse = false;
      doubleAnimation.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(500.0));
      doubleAnimation.FillBehavior = FillBehavior.HoldEnd;
      doubleAnimation.From = new double?(1.0);
      doubleAnimation.To = new double?(0.23999999463558197);
      doubleAnimation.Duration = (Duration) TimeSpan.FromMilliseconds(120.0);
      DoubleAnimation element = doubleAnimation;
      storyboard.Children.Add((Timeline) element);
      Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath((object) UIElement.OpacityProperty));
      storyboard.Begin((FrameworkElement) this._textEditorMenu);
    }

    private async Task AbandonedOrReopenTask(bool? isAbandone = null)
    {
      TaskDetailView child = this;
      child._popupShowing = false;
      if (!child.CheckEnable() || child._task == null)
        return;
      isAbandone = new bool?(((int) isAbandone ?? (!child._task.IsAbandoned ? 1 : 0)) != 0);
      int status = isAbandone.Value ? -1 : 0;
      if (child._task.Status == 0 && child._task.StartDate.HasValue && child._task.DisplayStartDate.HasValue)
      {
        DateTime dateTime = child._task.StartDate.Value;
        DateTime date1 = dateTime.Date;
        dateTime = child._task.DisplayStartDate.Value;
        DateTime date2 = dateTime.Date;
        if (date1 != date2)
        {
          IToastShowWindow dependentWindow = Utils.FindParent<TaskDetailPopup>((DependencyObject) child)?.DependentWindow;
          if (!await ModifyRepeatHandler.CompleteOrSkipRecurrence(child._task.TaskId, child._task.DisplayStartDate, dependentWindow))
            return;
        }
      }
      TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(child._task.TaskId, status, true, inDetail: true);
      child.TryToast(status != -1 ? Utils.GetString("TaskReopenedTips") : Utils.GetString("TaskAbandonedTips"));
      if (taskCloseExtra?.RepeatTask == null)
        return;
      child.TryToast(Utils.GetString("CreatedTheNextCircle"));
    }

    public void SetStickyMode() => ThemeUtil.SetTheme("light", (FrameworkElement) this);

    public void TryTabInputBox()
    {
      if (!this._titleText.KeyboardFocused)
        return;
      if (Utils.IfShiftPressed())
      {
        EventHandler focusList = this.FocusList;
        if (focusList == null)
          return;
        focusList((object) this, (EventArgs) null);
      }
      else if (this._contentText != null)
      {
        this._contentText.FocusEditBox();
      }
      else
      {
        if (this._checklist == null)
          return;
        this._checklist.FocusFirstItem();
      }
    }

    public void SetTitleAndContent(TaskBaseViewModel model)
    {
      this.DataContext = (object) new TaskDetailViewModel(model);
      this._titleText.SetTextOffset(model.Title, false);
      this.SetTitleHint(model.Title);
      this._contentText?.SetImageGeneratorTaskId(model.Id);
      this._contentText?.SetTextAndOffset(model.Content, false);
      this._descText?.SetTextAndOffset(model.Desc, false);
    }

    public async Task TryParseUrl()
    {
      if (!this._titleText.CurrentFocused || !this._titleText.TryPasteLink())
        return;
      await Task.Delay(500);
    }

    public void SetTheme(bool isDark)
    {
      if (this._isDark.HasValue && this._isDark.Value == isDark)
        return;
      this._isDark = new bool?(isDark);
      ThemeUtil.SetTheme(isDark ? "dark" : "light", (FrameworkElement) this);
      this.SetSubtasks(this._task);
      this._contentText?.SetTheme(!isDark);
      this._titleText.SetLightTheme();
      this._descText?.SetTheme(!isDark);
      this._checklist?.SetStickyTheme(isDark);
    }

    public void OnShowDialog()
    {
      EventHandler showDialog = this.ShowDialog;
      if (showDialog == null)
        return;
      showDialog((object) this, (EventArgs) null);
    }

    public void OnCloseDialog()
    {
      EventHandler<bool> closeDialog = this.CloseDialog;
      if (closeDialog == null)
        return;
      closeDialog((object) this, false);
    }

    public async void ShowAttendeeDialog()
    {
      TaskDetailView taskDetailView = this;
      if (!taskDetailView.CheckEnable())
        return;
      TaskAttendModel agendaAttendModel = await AgendaHelper.GetAgendaAttendModel(taskDetailView._task.AttendId, taskDetailView._task.TaskId);
      if (agendaAttendModel?.organizer == null)
        return;
      taskDetailView.OnShowDialog();
      AttendeeListDialog attendeeListDialog = new AttendeeListDialog(taskDetailView._task.ProjectId, taskDetailView._task.AttendId, taskDetailView._task.TaskId, agendaAttendModel);
      attendeeListDialog.Owner = Window.GetWindow((DependencyObject) taskDetailView);
      attendeeListDialog.ShowDialog();
      taskDetailView.OnCloseDialog();
      taskDetailView.TryLoadAttendees();
    }

    public void OnEditorClick()
    {
      if (this._task != null && !this._task.Enable)
        return;
      this.SetTextEditorMenu(this._textEditorMenu == null);
    }

    public void OnCommentClick()
    {
      if (this.IsNoPermission(true))
        return;
      this.AddActionEvent("action", "comment");
      this.SetTextEditorMenu(false);
      if (this._addCommentCtrl == null)
      {
        CommentEditViewModel editCommentModel;
        if (this._addCommentCtrl?.EditModel == null || !(this._addCommentCtrl.EditModel.TaskId == this._task.TaskId))
        {
          editCommentModel = new CommentEditViewModel();
          editCommentModel.TaskId = this._task.TaskId;
          editCommentModel.ProjectId = this._task.ProjectId;
        }
        else
          editCommentModel = this._addCommentCtrl.EditModel;
        this.TryInitAddCommentCtrl(editCommentModel, true);
      }
      else
        this.HideAddComment();
    }

    public async Task OnAbandonOrReopenClick()
    {
      this.AddActionEvent("om", "wont_do_reopen");
      UtilLog.Info(string.Format("TaskDetail.AbandonedOrReopenTask {0}, CurrentStatus{1} from:Om", (object) this._task?.TaskId, (object) this._task?.Status));
      await this.AbandonedOrReopenTask();
    }

    public void OnAddAttachmentClick()
    {
      this.AddActionEvent("om", "attachment");
      this.ShowUploadFileDialog();
    }

    public async void TryInsertSummary()
    {
      TaskDetailView sender = this;
      MarkDownEditor content = sender._contentText;
      if (sender._contentText == null)
        return;
      EventHandler showDialog = sender.ShowDialog;
      if (showDialog != null)
        showDialog((object) sender, (EventArgs) null);
      sender._needFocusDetail = false;
      sender._popupShowing = true;
      UtilLog.Info("DetailInsertSummary");
      sender.AddActionEvent("om", "insert_summary");
      InsertSummaryWindow insertSummaryWindow = new InsertSummaryWindow();
      insertSummaryWindow.Owner = Window.GetWindow((DependencyObject) sender);
      insertSummaryWindow.InsertSummary += (EventHandler<string>) ((o, e) =>
      {
        int offset = Math.Min(content.EditBox.CaretOffset, content.Text.Length);
        content.EditBox.Document.Insert(offset, (offset > 0 ? "\r\n" : "") + e);
      });
      insertSummaryWindow.ShowDialog();
      await Task.Delay(200);
      sender.TryFocusDetail();
      Window.GetWindow((DependencyObject) sender)?.Activate();
      sender._popupShowing = false;
      EventHandler<bool> closeDialog = sender.CloseDialog;
      if (closeDialog == null)
        return;
      closeDialog((object) sender, true);
    }

    public void ShowTaskActivities()
    {
      this.AddActionEvent("om", "task_activities");
      this.ToggleTaskActivityPanel(true);
    }

    public async void OnAddTemplateClick()
    {
      TaskDetailView taskDetailView = this;
      if (taskDetailView._task == null)
        return;
      taskDetailView._popupShowing = true;
      EventHandler showDialog = taskDetailView.ShowDialog;
      if (showDialog != null)
        showDialog((object) taskDetailView, (EventArgs) null);
      UtilLog.Info("DetailSaveTemplate");
      await TemplateDao.SaveTemplate(taskDetailView._task.TaskId, taskDetailView);
      taskDetailView.AddActionEvent("om", "save_as_template");
      taskDetailView._popupShowing = false;
      EventHandler<bool> closeDialog = taskDetailView.CloseDialog;
      if (closeDialog != null)
        closeDialog((object) taskDetailView, true);
      taskDetailView.TryFocusDetail();
    }

    public void OnTaskCopy()
    {
      this._titleText.FocusText();
      this.OnCopy();
    }

    public void OnCopyLinkClick()
    {
      this.AddActionEvent("om", "copy_link");
      TaskUtils.CopyTaskLink(this._task.GetTaskId(), this._task.ProjectId, this._task.Title);
    }

    public async Task SwitchTaskNoteClick()
    {
      await TaskService.SwitchTaskOrNote(this._task.TaskId);
      this.AddActionEvent("om", this._task.Kind == "TEXT" ? "convert_to_note" : "convert_to_task");
      SyncManager.TryDelaySync();
    }

    public async Task OnDeleteFromTrash()
    {
      TaskDetailView taskDetailView = this;
      if (taskDetailView.IsNoPermission())
        return;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DeleteForever"), Utils.GetString("DeleteForeverHint"), Utils.GetString("Delete"), Utils.GetString("Cancel"));
      customerDialog.Owner = Window.GetWindow((DependencyObject) taskDetailView);
      customerDialog.Topmost = false;
      if (!customerDialog.ShowDialog().GetValueOrDefault())
        return;
      await taskDetailView.DeleteTaskForever(2);
    }

    public bool MorePopupOpen
    {
      get
      {
        TaskDetailBottomControl detailBottom = this._detailBottom;
        return detailBottom != null && detailBottom.MorePopup.IsOpen;
      }
    }

    public void OnBackClick(object sender)
    {
      if (this.Mode == Constants.DetailMode.Page)
      {
        this.OnBackClick();
      }
      else
      {
        EventHandler navigateBack = this.NavigateBack;
        if (navigateBack == null)
          return;
        navigateBack(sender, (EventArgs) null);
      }
    }

    public async Task OnCheckBoxClick()
    {
      if (!this.CheckEnable())
        return;
      UtilLog.Info(string.Format("TaskDetail.CompleteTask {0}, CurrentStatus {1} from:clickCheckBox", (object) this._task?.TaskId, (object) this._task?.Status));
      await this.CompleteOrUndoneTask();
    }

    public void OnCheckBoxMouseRightUp(UIElement element)
    {
      if (!this.CheckEnable())
        return;
      SetClosedStatusPopup instance = SetClosedStatusPopup.GetInstance(element, this._task.IsCompleted, this._task.IsAbandoned, -20.0, 10.0);
      this.AddActionEvent("action", "checkbox_cm");
      instance.Abandoned += (EventHandler) (async (s, o) =>
      {
        if (this._task == null)
          return;
        UtilLog.Info("TaskDetail.AbandonTask " + this._task?.TaskId + " from:RightClickCheckBox");
        await this.AbandonedOrReopenTask(new bool?(true));
      });
      instance.Completed += (EventHandler) (async (s, o) =>
      {
        if (this._task == null)
          return;
        UtilLog.Info("TaskDetail.CompleteTask " + this._task?.TaskId + " from:RightClickCheckBox");
        await this.CompleteOrUndoneTask(new bool?(true));
      });
      instance.Closed += (EventHandler) ((o, e) => this._popupShowing = false);
      this._popupShowing = true;
      instance.Show();
    }

    public async void ProgressClick(int pointerProgress, int currentProgress)
    {
      TaskDetailView child = this;
      if (!child.CheckEnable() || pointerProgress < 0 || pointerProgress == currentProgress)
        return;
      if (child._task.Status == 0 && child._task.StartDate.HasValue && child._task.DisplayStartDate.HasValue)
      {
        DateTime dateTime = child._task.StartDate.Value;
        DateTime date1 = dateTime.Date;
        dateTime = child._task.DisplayStartDate.Value;
        DateTime date2 = dateTime.Date;
        if (date1 != date2)
        {
          IToastShowWindow dependentWindow = Utils.FindParent<TaskDetailPopup>((DependencyObject) child)?.DependentWindow;
          if (!await ModifyRepeatHandler.CompleteOrSkipRecurrence(child._task.TaskId, child._task.DisplayStartDate, dependentWindow))
            return;
          TaskModel thinTaskById = await TaskDao.GetThinTaskById(child._task.TaskId);
          if (thinTaskById != null)
          {
            TaskService.SyncProgress(thinTaskById, pointerProgress);
            return;
          }
        }
      }
      if (child._detailHead != null)
        await child._detailHead.SetProgress(pointerProgress, true);
      child._task.SourceViewModel.Progress = pointerProgress;
      await child.SaveTask();
    }

    public void SetBackIconEnable(bool enable) => this._detailHead?.SetBackIcon(enable, true);

    public bool Enable() => this._task.Enable;

    public bool CheckMouseOver()
    {
      AddCommentControl addCommentCtrl = this._addCommentCtrl;
      // ISSUE: explicit non-virtual call
      return ((addCommentCtrl != null ? (__nonvirtual (addCommentCtrl.IsMouseOver) ? 1 : 0) : 0) != 0 || this._detailScrollViewer.IsMouseOver && this._contentText != null) && this.Enable();
    }

    public bool TextFocus()
    {
      if (!this._popupShowing)
      {
        DetailTextBox titleText = this._titleText;
        if ((titleText != null ? (titleText.KeyboardFocused ? 1 : 0) : 0) == 0)
        {
          MarkDownEditor contentText = this._contentText;
          if ((contentText != null ? (contentText.KeyboardFocused ? 1 : 0) : 0) == 0)
          {
            MarkDownEditor descText = this._descText;
            if ((descText != null ? (descText.KeyboardFocused ? 1 : 0) : 0) == 0)
            {
              ChecklistControl checklist = this._checklist;
              return checklist != null && checklist.ItemFocus();
            }
          }
        }
      }
      return true;
    }

    public void SetCanScroll(bool b) => this._canScroll = b;

    public void OnCommentEdit(CommentItemControl commentItem)
    {
      System.Windows.Point point = commentItem.TranslatePoint(new System.Windows.Point(0.0, commentItem.ActualHeight), (UIElement) this._scrollContent);
      if (this._detailScrollViewer.VerticalOffset + this._detailScrollViewer.ActualHeight >= point.Y)
        return;
      this._detailScrollViewer.ScrollToVerticalOffset(point.Y - this._detailScrollViewer.ActualHeight + 20.0);
    }

    public void TryForceHideWindow()
    {
      EventHandler forceHideWindow = this.ForceHideWindow;
      if (forceHideWindow == null)
        return;
      forceHideWindow((object) this, (EventArgs) null);
    }

    public UIElement BatchOperaPlacementTarget() => this._detailHead?.GetDateDropTarget();

    public event EventHandler ShowDialog;

    public event EventHandler<bool> CloseDialog;

    private void InitAttachmentOptionPanel()
    {
      this._attachmentOptionPanel.MouseLeave += new System.Windows.Input.MouseEventHandler(this.OnAttachmentMouseLeave);
      this._attachmentOptionPanel.Delete += new EventHandler<AttachmentInfo>(this.OnAttachmentDeleteClick);
      this._attachmentOptionPanel.CheckListDelete += new EventHandler<AttachmentModel>(this.OnCheckListAttachmentDeleteClick);
      this._attachmentOptionPanel.ImageModeChanged += new EventHandler<int>(this.OnImageModeChanged);
      this._attachmentOptionPanel.SetValue(Grid.RowProperty, (object) 2);
      this._scrollContent.Children.Add((UIElement) this._attachmentOptionPanel);
    }

    private void SetAttachments(List<AttachmentViewModel> models)
    {
      bool valueOrDefault = (this.FindResource((object) "IsDarkTheme") as bool?).GetValueOrDefault();
      // ISSUE: explicit non-virtual call
      if (models != null && __nonvirtual (models.Count) > 0)
      {
        if (this._attachmentPanel == null)
        {
          StackPanel stackPanel = new StackPanel();
          stackPanel.Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
          stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
          this._attachmentPanel = stackPanel;
          this._attachmentPanel.SetValue(Grid.RowProperty, (object) 2);
          this._contentGrid.Children.Add((UIElement) this._attachmentPanel);
        }
        this._attachmentPanel.Children.Clear();
        foreach (AttachmentViewModel model in models)
          this._attachmentPanel.Children.Add(model.FileType == "IMAGE" ? (UIElement) new AttachmentImageDisplayControl(model, new AttachmentImageDisplayControl.OnAttachementMouseEnter(this.HandleAttachmentMouseEnter)) : (UIElement) new AttachmentFileDisplayControl(model, new AttachmentImageDisplayControl.OnAttachementMouseEnter(this.HandleAttachmentMouseEnter), valueOrDefault));
      }
      else
      {
        if (this._attachmentPanel == null)
          return;
        this._attachmentPanel.Children.Clear();
        this._contentGrid.Children.Remove((UIElement) this._attachmentPanel);
        this._attachmentPanel = (StackPanel) null;
      }
    }

    private async Task LoadAttachmentAsync()
    {
      TaskDetailView taskDetailView = this;
      await taskDetailView._lockAttachment.RunAsync(new Func<Task>(taskDetailView.LoadAttachmentInternal));
    }

    private async Task LoadAttachmentInternal()
    {
      TaskDetailView taskDetailView = this;
      List<AttachmentModel> taskAttachments = await AttachmentDao.GetTaskAttachments(taskDetailView._task.TaskId);
      List<AttachmentModel> attachmentModelList = taskAttachments != null ? taskAttachments.Where<AttachmentModel>((Func<AttachmentModel, bool>) (a => a.status == 0)).ToList<AttachmentModel>() : (List<AttachmentModel>) null;
      if (attachmentModelList == null || attachmentModelList.Count == 0)
      {
        taskDetailView._attachmentProvider?.Clear();
        taskDetailView.SetAttachments((List<AttachmentViewModel>) null);
        attachmentModelList = (List<AttachmentModel>) null;
      }
      else
      {
        foreach (AttachmentModel attachmentModel in attachmentModelList)
        {
          if ((string.IsNullOrEmpty(attachmentModel.refId) || attachmentModel.refId == attachmentModel.id) && string.IsNullOrEmpty(attachmentModel.localPath) && string.IsNullOrEmpty(attachmentModel.path))
          {
            await TaskService.CheckAttachmentPath(taskDetailView._task.ParentId, taskDetailView._task.TaskId, attachmentModelList);
            break;
          }
        }
        if (taskDetailView._task.Kind == "CHECKLIST")
        {
          List<AttachmentViewModel> models;
          if (taskDetailView._attachmentProvider == null)
          {
            taskDetailView._attachmentProvider = new AttachmentProvider((IEnumerable<AttachmentModel>) attachmentModelList, taskDetailView._task.ImageMode);
            models = taskDetailView._attachmentProvider.GetModels();
          }
          else
          {
            if (taskDetailView._task.TaskId != taskDetailView._attachmentProvider.TaskId)
              taskDetailView._attachmentProvider.Clear();
            taskDetailView._attachmentProvider.UpdateModels(attachmentModelList, taskDetailView._task.ImageMode, out List<string> _, out List<string> _);
            models = taskDetailView._attachmentProvider.GetModels();
          }
          foreach (AttachmentViewModel attachmentViewModel in models)
            attachmentViewModel.Margin = new Thickness(0.0, 5.0, 0.0, 5.0);
          taskDetailView._attachmentProvider.TaskId = taskDetailView._task.TaskId;
          bool isSmall = taskDetailView._task.ImageMode == 1;
          int maxHeight = isSmall ? ImageElementGenerator.SmallSizeHeight : 15000;
          double maxWidth = taskDetailView.ActualWidth - (taskDetailView.Mode == Constants.DetailMode.Sticky ? 30.0 : 44.0);
          if (maxWidth <= 0.0)
          {
            attachmentModelList = (List<AttachmentModel>) null;
          }
          else
          {
            taskDetailView.SetAttachments(models);
            Task.Run((Action) (() =>
            {
              foreach (AttachmentViewModel attachmentViewModel in models)
              {
                if (attachmentViewModel.FileType == "IMAGE")
                {
                  Size imageSize = ThemeUtil.GetImageSize(attachmentViewModel.LocalPath);
                  Size size = isSmall ? ImageUtils.GetSmallRect(maxWidth, (double) maxHeight, imageSize.Width, imageSize.Height) : ImageUtils.GetNormalRect(maxWidth, (double) maxHeight, imageSize.Width, imageSize.Height);
                  attachmentViewModel.Width = size.Width;
                  attachmentViewModel.Height = size.Height;
                  attachmentViewModel.Tag = (object) attachmentViewModel;
                }
                else
                {
                  attachmentViewModel.Width = maxWidth;
                  attachmentViewModel.Tag = (object) attachmentViewModel;
                }
              }
            }));
            attachmentModelList = (List<AttachmentModel>) null;
          }
        }
        else
        {
          AttachmentProvider attachmentProvider = taskDetailView._attachmentProvider;
          if (attachmentProvider == null)
          {
            attachmentModelList = (List<AttachmentModel>) null;
          }
          else
          {
            attachmentProvider.Clear();
            attachmentModelList = (List<AttachmentModel>) null;
          }
        }
      }
    }

    private void HideAttachmentOption()
    {
      this._attachmentOptionPanel.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      this._attachmentOptionPanel.Opacity = 0.0;
      this._attachmentOptionPanel.Visibility = Visibility.Collapsed;
    }

    private async Task ShowUploadFileDialog()
    {
      TaskDetailView sender = this;
      EventHandler showDialog1 = sender.ShowDialog;
      if (showDialog1 != null)
        showDialog1((object) sender, (EventArgs) null);
      UtilLog.Info("DetailShowFileSelector");
      sender._popupShowing = true;
      OpenFileDialog openFileDialog = new OpenFileDialog()
      {
        Multiselect = UserDao.IsPro()
      };
      MarkDownEditor contentText = sender._contentText;
      int caretOffset = contentText != null ? contentText.EditBox.CaretOffset : 0;
      DialogResult showDialog = openFileDialog.ShowDialog();
      sender._popupShowing = false;
      if (sender._contentText != null)
      {
        sender._contentText.EditBox.CaretOffset = caretOffset;
        sender._contentText.FocusEditBox();
        await Task.Delay(100);
      }
      if (showDialog == DialogResult.OK)
      {
        int fileNum = Enumerable.Cast<object>(openFileDialog.FileNames).Count<object>((Func<object, bool>) (item => File.Exists(item.ToString())));
        if (!await sender.CheckAttachmentLimit(fileNum))
        {
          EventHandler<bool> closeDialog = sender.CloseDialog;
          if (closeDialog == null)
          {
            openFileDialog = (OpenFileDialog) null;
          }
          else
          {
            closeDialog((object) sender, true);
            openFileDialog = (OpenFileDialog) null;
          }
        }
        else
        {
          string[] strArray = openFileDialog.FileNames;
          for (int index = 0; index < strArray.Length; ++index)
          {
            string str1 = strArray[index];
            if (File.Exists(str1))
            {
              string fileName = FileUtils.TrimFileName(((IEnumerable<string>) str1.Split('\\')).Last<string>(), 240 - AppPaths.ImageDir.Length);
              string str2 = AppPaths.ImageDir + fileName;
              if (File.Exists(str2))
              {
                int startIndex = fileName.LastIndexOf('.');
                if (startIndex < 0)
                  startIndex = fileName.Length;
                int num = 1;
                do
                {
                  ++num;
                  str2 = AppPaths.ImageDir + fileName.Insert(startIndex, string.Format("({0})", (object) num));
                }
                while (File.Exists(str2));
                fileName = fileName.Insert(startIndex, string.Format("({0})", (object) num));
              }
              File.Copy(str1, str2);
              await sender.UploadAttachment(fileName, str2);
            }
          }
          strArray = (string[]) null;
          sender.LoadAttachmentAsync();
          TaskDetailViewModel taskDetailViewModel = sender._task;
          taskDetailViewModel.Attachments = (await AttachmentDao.GetTaskAttachments(sender._task.TaskId)).ToArray();
          taskDetailViewModel = (TaskDetailViewModel) null;
          EventHandler<bool> closeDialog = sender.CloseDialog;
          if (closeDialog != null)
            closeDialog((object) sender, true);
          TaskChangeNotifier.NotifyAttachmentChanged(sender._task.TaskId, (object) sender);
          openFileDialog = (OpenFileDialog) null;
        }
      }
      else
      {
        EventHandler<bool> closeDialog = sender.CloseDialog;
        if (closeDialog == null)
        {
          openFileDialog = (OpenFileDialog) null;
        }
        else
        {
          closeDialog((object) sender, true);
          openFileDialog = (OpenFileDialog) null;
        }
      }
    }

    private async Task UploadAttachment(string fileName, string filePath)
    {
      TaskDetailView child = this;
      AttachmentModel uploadAttachmentModel;
      IToastShowWindow toastWindow;
      string text;
      string taskId;
      string attachmentText;
      if (FileUtils.FileEmptyOrNotExists(filePath))
      {
        UtilLog.Warn("UploadAttachment empty file " + filePath);
        uploadAttachmentModel = (AttachmentModel) null;
        toastWindow = (IToastShowWindow) null;
        text = (string) null;
        taskId = (string) null;
        attachmentText = (string) null;
      }
      else
      {
        Constants.AttachmentKind fileType = AttachmentProvider.GetFileType(fileName);
        FileInfo fileInfo = new FileInfo(filePath);
        uploadAttachmentModel = new AttachmentModel()
        {
          id = Utils.GetGuid()
        };
        uploadAttachmentModel.refId = uploadAttachmentModel.id;
        uploadAttachmentModel.taskId = child._task.TaskId;
        uploadAttachmentModel.fileName = fileName;
        uploadAttachmentModel.localPath = filePath;
        uploadAttachmentModel.fileType = fileType.ToString();
        uploadAttachmentModel.createdTime = new DateTime?(DateTime.Now);
        uploadAttachmentModel.sync_status = 0.ToString();
        uploadAttachmentModel.size = (int) fileInfo.Length;
        if (uploadAttachmentModel.fileType == Constants.AttachmentKind.IMAGE.ToString())
          AttachmentUploadUtils.CompressImage(uploadAttachmentModel);
        FileUtils.CollectFileSize(uploadAttachmentModel.localPath);
        if (FileUtils.FileOverSize(uploadAttachmentModel.localPath))
        {
          child.TryToast(LocalSettings.Settings.IsPro ? Utils.GetString("AttachmentSizeLimitPro") : Utils.GetString("AttachmentSizeLimit"));
          uploadAttachmentModel = (AttachmentModel) null;
          toastWindow = (IToastShowWindow) null;
          text = (string) null;
          taskId = (string) null;
          attachmentText = (string) null;
        }
        else
        {
          toastWindow = Utils.FindParent<IToastShowWindow>((DependencyObject) child);
          text = child._contentText?.Text ?? string.Empty;
          taskId = child._task.TaskId;
          attachmentText = string.Empty;
          int index = 0;
          int extraIndex = 0;
          UtilLog.Info(string.Format("PastFileCaret {0},{1},{2},{3}", (object) child._contentText?.CurrentFocused, (object) child._contentText?.KeyboardFocused, (object) child._contentText?.EditBox.CaretOffset, (object) child._contentText?.EditBox.Text.Length));
          if (child._task.Kind != "CHECKLIST" && child._contentText != null)
          {
            attachmentText = "![" + (fileType == Constants.AttachmentKind.IMAGE ? "image" : "file") + "](" + uploadAttachmentModel.id + "/" + Utils.UrlEncode(uploadAttachmentModel.fileName) + ")";
            index = child._contentText.CurrentFocused || child._contentText.EditBox.CaretOffset > 0 ? child._contentText.EditBox.CaretOffset : child._contentText.GetAttachmentDefaultInsertIndex();
            DocumentLine lineByOffset = child._contentText.EditBox.TextArea.Document.GetLineByOffset(Math.Min(index, text.Length));
            DocumentLine nextLine = lineByOffset.NextLine;
            if (index != lineByOffset.Offset)
              attachmentText = "\n" + attachmentText;
            if (index != lineByOffset.EndOffset || index == text.Length)
              attachmentText += "\n";
            else
              extraIndex = nextLine == null ? 0 : nextLine.Offset - lineByOffset.EndOffset;
          }
          await AttachmentDao.InsertOrUpdateAttachment(uploadAttachmentModel);
          AttachmentCache.SetTodayAttachmentCount(AttachmentCache.TodayAttachmentCount() + 1);
          if (!child._task.IsNewAdd)
            AttachmentUploadUtils.UpFile(child._task.ProjectId, child._task.TaskId, uploadAttachmentModel, toastWindow, true).ConfigureAwait(false);
          if (child._task.Kind == "CHECKLIST")
          {
            uploadAttachmentModel = (AttachmentModel) null;
            toastWindow = (IToastShowWindow) null;
            text = (string) null;
            taskId = (string) null;
            attachmentText = (string) null;
          }
          else
          {
            text = text.Insert(index, attachmentText);
            if (child._task.TaskId == taskId && child._contentText != null)
            {
              child._contentText.UnRegisterCaretChanged();
              child._contentText.SetTextAndOffset(text, true);
              child._contentText.FocusEditBox();
              child._contentText.RegisterCaretChanged();
              await Task.Delay(50);
              child._contentText.EditBox.CaretOffset = Math.Min(index + attachmentText.Length + extraIndex, child._contentText.Text.Length);
              child.SetContentHint(text);
              child._task.SourceViewModel.Content = text;
            }
            await TaskService.SaveTaskContent(taskId, text);
            uploadAttachmentModel = (AttachmentModel) null;
            toastWindow = (IToastShowWindow) null;
            text = (string) null;
            taskId = (string) null;
            attachmentText = (string) null;
          }
        }
      }
    }

    public async Task<bool> CheckAttachmentLimit(int fileNum, int replaceNum = 0, bool ignoreUpload = false)
    {
      TaskDetailView taskDetailView = this;
      if (!ignoreUpload && Utils.GetResidueAttachmentCount(fileNum) < (long) fileNum)
      {
        if (UserDao.IsPro())
        {
          CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("AttachmentsLimit"), MessageBoxButton.OK);
          customerDialog.Owner = Window.GetWindow((DependencyObject) taskDetailView);
          customerDialog.ShowDialog();
        }
        else
          ProChecker.ShowUpgradeDialog(ProType.MoreAttachments);
        return false;
      }
      int taskAttachmentCount = await AttachmentDao.GetTaskAttachmentCount(taskDetailView._task.TaskId);
      long userLimit = Utils.GetUserLimit(Constants.LimitKind.TaskAttachmentNumber);
      int num = fileNum;
      if ((long) (taskAttachmentCount + num - replaceNum) <= userLimit)
        return true;
      CustomerDialog customerDialog1 = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("TaskAttachmentsLimitContent"), MessageBoxButton.OK);
      customerDialog1.Owner = Window.GetWindow((DependencyObject) taskDetailView);
      customerDialog1.ShowDialog();
      return false;
    }

    private BitmapSource GetPastImage()
    {
      BitmapSource pastImage = (BitmapSource) null;
      try
      {
        if (string.IsNullOrEmpty(System.Windows.Clipboard.GetText()))
          pastImage = System.Windows.Clipboard.GetImage();
      }
      catch (Exception ex)
      {
      }
      return pastImage;
    }

    private bool CheckPasteEnable()
    {
      if (!this.CheckEnable())
        return false;
      if (this._task.IsOwner)
        return true;
      this.TryToast(Utils.GetString("AttendeeModifyContent"));
      return false;
    }

    private async Task<bool> TryPasteFile(IReadOnlyCollection<string> files)
    {
      TaskDetailView sender = this;
      if (files == null || files.Count <= 0)
        return false;
      if (!await sender.CheckAttachmentLimit(files.Count))
        return true;
      if (files.Count <= 0)
        return false;
      foreach (string file in (IEnumerable<string>) files)
      {
        string fileName = IOUtils.GetFileName(file);
        string str = FileUtils.SavePasteFile(file, fileName);
        try
        {
          if (File.Exists(str))
            await sender.UploadAttachment(fileName, str);
        }
        catch (Exception ex)
        {
          sender.TryToast(Utils.GetString("AddFailed"));
        }
      }
      sender.LoadAttachmentAsync();
      TaskDetailViewModel taskDetailViewModel = sender._task;
      taskDetailViewModel.Attachments = (await AttachmentDao.GetTaskAttachments(sender._task.TaskId)).ToArray();
      taskDetailViewModel = (TaskDetailViewModel) null;
      TaskChangeNotifier.NotifyAttachmentChanged(sender._task.TaskId, (object) sender);
      return true;
    }

    private async Task TryPasteImage(BitmapSource image)
    {
      TaskDetailView sender = this;
      if (image == null)
        return;
      if (!await sender.CheckAttachmentLimit(1))
        return;
      string str1 = Utils.GetGuid() + ".png";
      string str2 = ImageUtils.SavePasteImage(image, str1);
      if (!File.Exists(str2))
        return;
      await sender.UploadAttachment(str1, str2);
      sender.LoadAttachmentAsync();
      TaskDetailViewModel taskDetailViewModel = sender._task;
      taskDetailViewModel.Attachments = (await AttachmentDao.GetTaskAttachments(sender._task.TaskId)).ToArray();
      taskDetailViewModel = (TaskDetailViewModel) null;
      TaskChangeNotifier.NotifyAttachmentChanged(sender._task.TaskId, (object) sender);
    }

    private async void OnAttachmentDrop(object sender, System.Windows.DragEventArgs e)
    {
      e.Handled = true;
      this.TryPasteFile((IReadOnlyCollection<string>) TaskDetailView.GetDropFiles(e));
    }

    private static List<string> GetDropFiles(System.Windows.DragEventArgs e)
    {
      try
      {
        Array data = (Array) e.Data.GetData(System.Windows.DataFormats.FileDrop);
        List<string> dropFiles = new List<string>();
        if (data != null)
        {
          foreach (object obj in data)
            dropFiles.Add(obj.ToString());
        }
        return dropFiles;
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex.Message);
        return new List<string>();
      }
    }

    private void AttachmentDeleteGridMouseEnter(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      if (!(sender is Grid grid) || !(grid.DataContext is AttachmentViewModel dataContext))
        return;
      grid.Cursor = dataContext.CanDelete ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.No;
    }

    private async Task TryLoadComments(
      bool showRecent = true,
      bool focus = false,
      bool loadLocal = true,
      bool loadRemote = true)
    {
      if (this.Mode == Constants.DetailMode.Editor || this.Mode == Constants.DetailMode.Sticky)
        return;
      if (this._addCommentCtrl?.EditModel != null)
        this._addCommentCtrl.EditModel.RecentScrollHeight = 0.0;
      int result;
      int.TryParse(this._task.CommentCount, out result);
      if (result > 0)
      {
        if (loadLocal)
        {
          int num = await this.TryLoadLocalComments(showRecent, focus);
        }
        if (loadRemote)
          this.TryLoadRemoteComments();
      }
      else
        this.ClearComment();
      if (this._task.Deleted == 0)
        return;
      this.HideAddComment();
    }

    private void ClearComment()
    {
      AddCommentControl addCommentCtrl = this._addCommentCtrl;
      if ((addCommentCtrl != null ? (addCommentCtrl.IsEditing() ? 1 : 0) : 0) == 0)
        this.HideAddComment();
      this._scrollContent.Children.Remove((UIElement) this._commentDisplayCtrl);
      this._commentDisplayCtrl = (CommentDisplayControl) null;
    }

    private void TryLoadRemoteComments()
    {
      CommentService.TryPullRemoteComments(this._task.ProjectId, this._task.TaskId);
    }

    private async Task<int> LoadComments(bool showRecent = true, bool focus = false)
    {
      TaskDetailView taskDetailView = this;
      return await taskDetailView._commentAsyncLocker.RunAsync(new Func<bool, bool, Task<int>>(taskDetailView.TryLoadLocalComments), showRecent, focus);
    }

    private async Task<int> TryLoadLocalComments(bool showRecent = true, bool focus = false)
    {
      TaskDetailView taskDetailView = this;
      if (taskDetailView._commentDisplayCtrl != null && taskDetailView._commentDisplayCtrl.Tag == (object) taskDetailView.TaskId)
      {
        ObservableCollection<CommentViewModel> displayList = taskDetailView._commentDisplayCtrl.Model.DisplayList;
        if ((displayList != null ? (displayList.Any<CommentViewModel>((Func<CommentViewModel, bool>) (m => m.Editing)) ? 1 : 0) : 0) != 0)
        {
          List<CommentViewModel> commentList = taskDetailView._commentDisplayCtrl.Model.CommentList;
          // ISSUE: explicit non-virtual call
          return commentList != null ? __nonvirtual (commentList.Count) : 0;
        }
      }
      List<CommentViewModel> commentViewModels = await taskDetailView.GetLocalCommentViewModels();
      if (commentViewModels.Count > 0)
      {
        CommentEditViewModel commentEditViewModel1;
        if (taskDetailView._addCommentCtrl?.EditModel == null || !(taskDetailView._addCommentCtrl.EditModel.TaskId == taskDetailView._task.TaskId))
        {
          commentEditViewModel1 = new CommentEditViewModel();
          commentEditViewModel1.TaskId = taskDetailView._task.TaskId;
          commentEditViewModel1.ProjectId = taskDetailView._task.ProjectId;
        }
        else
          commentEditViewModel1 = taskDetailView._addCommentCtrl.EditModel;
        CommentEditViewModel commentEditViewModel2 = commentEditViewModel1;
        commentEditViewModel2.RecentComment = commentViewModels[commentViewModels.Count - 1];
        commentEditViewModel2.RecentScrollHeight = taskDetailView._activityPanel == null ? taskDetailView._detailScrollViewer.ScrollableHeight - taskDetailView._detailScrollViewer.VerticalOffset : 0.0;
        commentEditViewModel2.ShowTopLine = commentViewModels.Count == 0;
        // ISSUE: reference to a compiler-generated method
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(taskDetailView.\u003CTryLoadLocalComments\u003Eb__242_1));
        CommentListViewModel commentListViewModel = new CommentListViewModel(commentViewModels, commentEditViewModel2, taskDetailView._task.Deleted == 0 && projectModel != null && projectModel.IsValid() && taskDetailView._task.Permission != "read");
        if (taskDetailView._task.Deleted == 0 && projectModel != null && projectModel.IsValid())
          taskDetailView.TryInitAddCommentCtrl(commentEditViewModel2, focus);
        taskDetailView.TryInitCommentDisplayCtrl(commentListViewModel);
      }
      else if (taskDetailView._commentDisplayCtrl != null)
      {
        taskDetailView._scrollContent.Children.Remove((UIElement) taskDetailView._commentDisplayCtrl);
        taskDetailView._commentDisplayCtrl = (CommentDisplayControl) null;
      }
      return commentViewModels.Count;
    }

    private void TryInitCommentDisplayCtrl(CommentListViewModel commentListViewModel)
    {
      if (this._commentDisplayCtrl == null)
      {
        this._commentDisplayCtrl = new CommentDisplayControl(commentListViewModel);
        this._commentDisplayCtrl.ShowAddComment += new EventHandler<CommentViewModel>(this.TryShowAddComment);
        this._commentDisplayCtrl.SetValue(Grid.RowProperty, (object) 4);
        this._scrollContent.Children.Add((UIElement) this._commentDisplayCtrl);
      }
      else
        this._commentDisplayCtrl.SetModel(commentListViewModel);
      this._commentDisplayCtrl.Tag = (object) this.TaskId;
    }

    private void TryShowAddComment(object sender, CommentViewModel e)
    {
      if (this._task.Permission == "read" && !this.CheckEnable())
        return;
      CommentEditViewModel commentEditViewModel = this._addCommentCtrl?.EditModel;
      if (commentEditViewModel == null)
        commentEditViewModel = new CommentEditViewModel()
        {
          TaskId = this._task.TaskId,
          ProjectId = this._task.ProjectId
        };
      CommentEditViewModel editCommentModel = commentEditViewModel;
      editCommentModel.ReplyCommentId = e.Model.id;
      editCommentModel.ReplyName = e.Model.userName;
      this.TryInitAddCommentCtrl(editCommentModel, true);
    }

    private void HideAddComment()
    {
      this.Children.Remove((UIElement) this._addCommentCtrl);
      this._addCommentCtrl = (AddCommentControl) null;
    }

    private void TryInitAddCommentCtrl(CommentEditViewModel editCommentModel, bool withFocus = false)
    {
      if (!this._task.Enable && this._task.Permission == "read")
      {
        this.HideAddComment();
      }
      else
      {
        if (this._addCommentCtrl == null)
        {
          this._addCommentCtrl = new AddCommentControl(editCommentModel);
          this._addCommentCtrl.CommentGotFocus += new EventHandler(this.OnCommentGotFocus);
          this._addCommentCtrl.SetValue(Grid.RowProperty, (object) 3);
          this.Children.Add((UIElement) this._addCommentCtrl);
          this._addCommentCtrl.OnCommentAdded += (EventHandler<CommentModel>) (async (sender, content) =>
          {
            content.id = Utils.GetGuid();
            UserInfoModel userInfo = await UserManager.GetUserInfo(true);
            content.userName = string.IsNullOrEmpty(userInfo?.name) ? userInfo?.username : userInfo.name;
            content.avatarUrl = userInfo?.picture;
            content.createdTime = new DateTime?(DateTime.Now);
            content.syncStatus = 0;
            content.deleted = 0;
            content.isMySelf = true;
            content.mentionstring = JsonConvert.SerializeObject((object) content.mentions);
            this._commentDisplayCtrl?.AddNewItem(content);
            this._commentDisplayCtrl?.ScrollToBottom();
            if (this._addCommentCtrl.EditModel == null)
              this._addCommentCtrl.EditModel = new CommentEditViewModel();
            await Task.Delay(1);
            UtilLog.Info("TaskDetail.AddComment taskId " + this._task?.TaskId + ", commentId " + content.id);
            await this.SaveNewAddComment(content);
            await this.TryLoadComments(false, true);
            await this.SyncTaskCommentCount();
          });
        }
        else
          this._addCommentCtrl.SetModel(editCommentModel);
        if (!withFocus)
          return;
        this._addCommentCtrl.FocusInput();
      }
    }

    private void OnCommentGotFocus(object sender, EventArgs e) => this.SetTextEditorMenu(false);

    private async Task<List<CommentViewModel>> GetLocalCommentViewModels()
    {
      List<CommentViewModel> comments = new List<CommentViewModel>();
      List<CommentModel> models = await CommentDao.GetCommentsByTaskId(this._task.TaskId);
      foreach (CommentModel commentModel1 in models)
      {
        CommentModel model = commentModel1;
        model.candelete = this._task.Deleted == 0;
        if (!string.IsNullOrEmpty(model.replyCommentId))
        {
          CommentModel commentModel2 = models.FirstOrDefault<CommentModel>((Func<CommentModel, bool>) (v => v.id == model.replyCommentId));
          if (commentModel2 != null)
            model.title = Utils.GetString("reply") + " " + commentModel2.userName + ": " + model.title;
        }
      }
      if (models.Count > 0 && models.Count > 0)
      {
        List<ShareUserModel> projectUsersAsync = await AvatarHelper.GetProjectUsersAsync(this._task.ProjectId);
        IEnumerable<string> deletedUserIds = (IEnumerable<string>) null;
        if (projectUsersAsync != null)
          deletedUserIds = projectUsersAsync.Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => user.deleted)).Select<ShareUserModel, string>((Func<ShareUserModel, string>) (user => user.userId.ToString()));
        models.ForEach((Action<CommentModel>) (model =>
        {
          if (deletedUserIds != null && deletedUserIds.Contains<string>(model.userId))
            return;
          comments.Add(new CommentViewModel(model));
        }));
      }
      List<CommentViewModel> commentViewModels = comments;
      models = (List<CommentModel>) null;
      return commentViewModels;
    }

    public void ExpandComment()
    {
      this._commentDisplayCtrl?.ExpandComment();
      this.SctollToBottom();
    }

    private async Task SaveNewAddComment(CommentModel comment)
    {
      await CommentService.AddComment(comment);
      await TaskService.SaveCommentCount(comment.taskSid);
    }

    private async Task SyncTaskCommentCount()
    {
      string commentCount = this._commentDisplayCtrl?.Model.Count.ToString() ?? "1";
      TaskModel task = await TaskDao.GetThinTaskById(this._task.TaskId);
      if (task == null)
      {
        commentCount = (string) null;
        task = (TaskModel) null;
      }
      else
      {
        task.commentCount = commentCount;
        await TaskDao.UpdateTask(task);
        await SyncStatusDao.AddModifySyncStatus(task.id);
        commentCount = (string) null;
        task = (TaskModel) null;
      }
    }

    public event EventHandler<string> TagClick;

    private void InitContentPanel()
    {
      this._contentGrid.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._contentGrid.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._contentGrid.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._contentGrid.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._contentGrid.RowDefinitions.Add(new RowDefinition()
      {
        Height = GridLength.Auto
      });
      this._contentGrid.SetValue(Grid.RowProperty, (object) 2);
      this._contentGrid.Margin = new Thickness(1.0, 5.0, 0.0, this.Mode == Constants.DetailMode.Sticky ? 5.0 : 0.0);
      this._contentGrid.SizeChanged += new SizeChangedEventHandler(this.OnContentSizeChanged);
      this._scrollContent.Children.Add((UIElement) this._contentGrid);
      this.InitAttachmentOptionPanel();
    }

    private void OnDetailContentClick(object sender, MouseButtonEventArgs e)
    {
      MarkDownEditor contentText = this._contentText;
      if (contentText == null || contentText.KeyboardFocused || this._titleText.IsMouseOver || this._subTaskList != null || this._commentDisplayCtrl != null && this._commentDisplayCtrl.IsMouseOver)
        return;
      TaskDetailPopup parent = Utils.FindParent<TaskDetailPopup>((DependencyObject) this);
      Window window = Window.GetWindow((DependencyObject) this);
      if ((window != null ? (window.IsActive ? 1 : 0) : 0) == 0 && (parent == null || !parent.IsFocus()))
        return;
      contentText.EditBox.CaretOffset = contentText.EditBox.Text.Length;
      contentText.FocusEditBox();
      e.Handled = true;
    }

    private void LoadContentControl(bool restoreOffset)
    {
      TaskDetailViewModel task = this._task;
      if (task.Kind == "CHECKLIST")
      {
        if (this._contentText != null)
        {
          this.RemoveContentEvent(this._contentText);
          this._contentGrid.Children.Remove((UIElement) this._contentText);
          this._contentText = (MarkDownEditor) null;
        }
        if (this._descText == null)
        {
          MarkDownEditor markDownEditor = new MarkDownEditor();
          markDownEditor.MaxLength = 2048;
          markDownEditor.Margin = new Thickness(-20.0, 0.0, 0.0, 4.0);
          markDownEditor.VerticalAlignment = VerticalAlignment.Top;
          markDownEditor.VerticalContentAlignment = VerticalAlignment.Center;
          this._descText = markDownEditor;
          this._descText.SetResourceReference(MarkDownEditor.IsDarkProperty, (object) "IsDarkTheme");
          this._descText.SetResourceReference(System.Windows.Controls.Control.FontSizeProperty, this.Mode == Constants.DetailMode.Sticky ? (object) "StickyFont13" : (object) "Font14");
          this._descText.SetValue(System.Windows.Controls.Panel.ZIndexProperty, (object) 10);
          this.BindDescEvent(this._descText);
          this._contentGrid.Children.Add((UIElement) this._descText);
          if (this.Mode == Constants.DetailMode.Sticky)
          {
            this._descText.SetTheme(!this._isDark.GetValueOrDefault());
            this._descText.Margin = new Thickness(-16.0, 0.0, 0.0, 2.0);
            this._descText.EditBox.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, (object) "StickyContentTextColor");
            this._descText.SetupMargin(18.0);
            this._descText.LineSpacing = 4.0;
          }
          if (this.Mode == Constants.DetailMode.Page)
            this._descText.SetupSearchRender(false, true);
        }
        this._descText.ReadOnly = !task.Enable;
        this.SetContentHint(task.Desc);
        if (!restoreOffset || this._descText.Text != task.Desc)
        {
          if (!restoreOffset)
          {
            this._descText.EditBox.CaretOffset = 0;
            this._descText.EditBox.SelectionLength = 0;
          }
          this._descText.SetTextAndOffset(task.Desc, restoreOffset);
        }
        if (this._checklist == null)
        {
          ChecklistControl checklistControl = new ChecklistControl();
          checklistControl.Margin = new Thickness(-20.0, 4.0, 0.0, 12.0);
          checklistControl.Background = (Brush) Brushes.Transparent;
          this._checklist = checklistControl;
          this._checklist.SetValue(Grid.RowProperty, (object) 1);
          this.BindChecklistEvent(this._checklist);
          this._contentGrid.Children.Add((UIElement) this._checklist);
          if (this.Mode == Constants.DetailMode.Sticky)
          {
            this._checklist.Margin = new Thickness(-20.0, 0.0, 0.0, 0.0);
            this._checklist?.SetStickyTheme(this._isDark.GetValueOrDefault());
            this._checklist.SetStickyMode();
          }
          if (this.Mode == Constants.DetailMode.Page)
            this._checklist.InMainDetail = true;
        }
        this._checklist.IsNewAddTask = task.IsNewAdd;
      }
      else
      {
        this.SetAttachments((List<AttachmentViewModel>) null);
        if (this._descText != null)
        {
          this.RemoveDescEvent(this._descText);
          this._contentGrid.Children.Remove((UIElement) this._descText);
          this._descText = (MarkDownEditor) null;
        }
        if (this._checklist != null)
        {
          this.RemoveChecklistEvent(this._checklist);
          this._contentGrid.Children.Remove((UIElement) this._checklist);
          this._checklist = (ChecklistControl) null;
        }
        if (this._contentText == null)
        {
          MarkDownEditor markDownEditor = new MarkDownEditor();
          markDownEditor.MaxLength = 160000;
          markDownEditor.Margin = new Thickness(-20.0, 0.0, 0.0, 4.0);
          markDownEditor.VerticalAlignment = VerticalAlignment.Top;
          markDownEditor.VerticalContentAlignment = VerticalAlignment.Center;
          this._contentText = markDownEditor;
          this._contentText.SetBinding(MarkDownEditor.ImageModeProperty, "ImageMode");
          this._contentText.SetResourceReference(MarkDownEditor.IsDarkProperty, (object) "IsDarkTheme");
          this._contentText.SetResourceReference(System.Windows.Controls.Control.FontSizeProperty, this.Mode == Constants.DetailMode.Sticky ? (object) "StickyFont13" : (object) "Font14");
          this._contentText.SetValue(System.Windows.Controls.Panel.ZIndexProperty, (object) 10);
          this.BindContentEvent(this._contentText);
          this._contentGrid.Children.Add((UIElement) this._contentText);
          this._contentText.SetMarkRegexText(false, isNote: this._task.Kind == "NOTE");
          if (this.Mode == Constants.DetailMode.Sticky)
          {
            this._contentText.SetTheme(!this._isDark.GetValueOrDefault());
            this._contentText.Margin = new Thickness(-16.0, 0.0, 0.0, 2.0);
            this._contentText.EditBox.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, (object) "StickyContentTextColor");
            this._contentText.SetupMargin(18.0);
            this._contentText.LineSpacing = 4.0;
          }
          if (this.Mode == Constants.DetailMode.Page)
            this._contentText.SetupSearchRender(false, true);
        }
        this._contentText.ReadOnly = !task.Enable;
        this._contentText.SetImageGeneratorTaskId(task.TaskId);
        this.SetContentHint(task.TaskContent);
        string text = task.TaskContent?.Replace("\t", "    ") ?? string.Empty;
        if (!string.IsNullOrEmpty(this._guideLink) && text.Contains("\n" + this._guideLink))
          text = text.TrimEnd().Replace("\n" + this._guideLink, "");
        if (restoreOffset && !(this._contentText.Text != text))
          return;
        if (!restoreOffset)
        {
          this._contentText.EditBox.CaretOffset = 0;
          this._contentText.EditBox.SelectionLength = 0;
        }
        this._contentText.SetTextAndOffset(text, restoreOffset);
      }
    }

    private void LoadTags(IReadOnlyCollection<string> tags)
    {
      if (tags.Count > 0)
      {
        if (this._tagsControl == null)
        {
          TagDisplayControl tagDisplayControl = new TagDisplayControl(tags, this._task.Enable);
          tagDisplayControl.Margin = new Thickness(0.0, 10.0, 0.0, 8.0);
          this._tagsControl = tagDisplayControl;
          this._tagsControl.TagsChanged += new EventHandler<List<string>>(this.OnTagsChanged);
          this._tagsControl.TagClick += new EventHandler<string>(this.OnTagLabelClick);
          this._tagsControl.TagPanelVisibleChanged += new EventHandler<Visibility>(this.OnTagPanelVisibleChanged);
          this._tagsControl.SetValue(Grid.RowProperty, (object) 4);
          this._contentGrid.Children.Add((UIElement) this._tagsControl);
        }
        else
        {
          if (this._tagsControl != null && !this._contentGrid.Children.Contains((UIElement) this._tagsControl))
            this._contentGrid.Children.Add((UIElement) this._tagsControl);
          this._tagsControl.InitData(tags, this._task.Enable);
        }
      }
      else
      {
        if (this._tagsControl == null)
          return;
        this._contentGrid.Children.Remove((UIElement) this._tagsControl);
      }
    }

    private void OnTagLabelClick(object sender, string tag)
    {
      EventHandler<string> tagClick = this.TagClick;
      if (tagClick == null)
        return;
      tagClick((object) this, tag);
    }

    private void OnTagPanelVisibleChanged(object sender, Visibility visibility)
    {
      this._popupShowing = visibility == Visibility.Visible;
      if (visibility == Visibility.Visible)
      {
        EventHandler actionPopOpened = this.ActionPopOpened;
        if (actionPopOpened == null)
          return;
        actionPopOpened((object) this, (EventArgs) null);
      }
      else
      {
        EventHandler<bool> actionPopClosed = this.ActionPopClosed;
        if (actionPopClosed == null)
          return;
        actionPopClosed((object) this, true);
      }
    }

    private void BindChecklistEvent(ChecklistControl checklist)
    {
      checklist.Changed += new ChecklistControl.ChecklistChangedDelegate(this.ChecklistModified);
      checklist.ItemsClear += new EventHandler(this.OnItemsEmpty);
      checklist.ItemDrop += new EventHandler<string>(this.OnItemDrop);
      checklist.PopOpened += new EventHandler<bool>(this.OnChecklistPopupOpened);
      checklist.PopClosed += new EventHandler<bool>(this.OnChecklistPopupClosed);
      checklist.QuickItemSelected += new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      checklist.ToastUnableText += new EventHandler<string>(this.TryToastUnableText);
      checklist.Navigate += new EventHandler<ProjectTask>(this.OnNavigateTask);
      checklist.DatePopOpened += new EventHandler(this.PopupOpened);
      checklist.DatePopClosed += new EventHandler(this.PopupClosed);
      checklist.CaretVerticalOffsetChanged += new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      checklist.CaretMoveUp += new EventHandler(this.OnListCaretMoveUp);
      checklist.CheckItemsDeleted += new EventHandler<TaskDetailItemModel>(this.OnCheckItemsDeleted);
    }

    private void RemoveChecklistEvent(ChecklistControl checklist)
    {
      checklist.Changed -= new ChecklistControl.ChecklistChangedDelegate(this.ChecklistModified);
      checklist.ItemsClear -= new EventHandler(this.OnItemsEmpty);
      checklist.ItemDrop -= new EventHandler<string>(this.OnItemDrop);
      checklist.PopOpened -= new EventHandler<bool>(this.OnChecklistPopupOpened);
      checklist.PopClosed -= new EventHandler<bool>(this.OnChecklistPopupClosed);
      checklist.QuickItemSelected -= new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      checklist.ToastUnableText -= new EventHandler<string>(this.TryToastUnableText);
      checklist.Navigate -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      checklist.DatePopOpened -= new EventHandler(this.PopupOpened);
      checklist.DatePopClosed -= new EventHandler(this.PopupClosed);
      checklist.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      checklist.CaretMoveUp -= new EventHandler(this.OnListCaretMoveUp);
      checklist.CheckItemsDeleted -= new EventHandler<TaskDetailItemModel>(this.OnCheckItemsDeleted);
    }

    private void BindDescEvent(MarkDownEditor desc)
    {
      desc.RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
      desc.QuickPopupOpened += new EventHandler(this.OnTextPopupOpened);
      desc.QuickItemSelected += new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      desc.LinkPopupOpened += new EventHandler(this.OnLinkPopupOpened);
      desc.LinkPopupClosed += new EventHandler(this.OnLinkPopupClosed);
      desc.CaretVerticalOffsetChanged += new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      desc.GotFocus += new RoutedEventHandler(this.OnDescGotFocus);
      desc.LostFocus += new RoutedEventHandler(this.OnDescLostFocus);
      desc.Navigate += new EventHandler<ProjectTask>(this.OnNavigateTask);
      desc.KeyDown += new EventHandler<System.Windows.Input.KeyEventArgs>(this.OnContentKeyDown);
      desc.MoveUp += new EventHandler(this.OnContentMoveUp);
      desc.MoveDown += new EventHandler(this.OnDescMoveDown);
      desc.EnterImmersive += new EventHandler(this.OnEnterImmersive);
      desc.SelectDate += new EventHandler(this.OnSelectDate);
      desc.TextChanged += new EventHandler(this.OnDescriptionChanged);
      desc.EditBox.PreviewTextInput += new TextCompositionEventHandler(this.OnContentInput);
      desc.SaveContent += new EventHandler(this.TrySaveOnEnter);
    }

    private void RemoveDescEvent(MarkDownEditor desc)
    {
      desc.RequestBringIntoView -= new RequestBringIntoViewEventHandler(this.OnBringIntoView);
      desc.QuickPopupOpened -= new EventHandler(this.OnTextPopupOpened);
      desc.QuickItemSelected -= new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      desc.LinkPopupOpened -= new EventHandler(this.OnLinkPopupOpened);
      desc.LinkPopupClosed -= new EventHandler(this.OnLinkPopupClosed);
      desc.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      desc.GotFocus -= new RoutedEventHandler(this.OnDescGotFocus);
      desc.LostFocus -= new RoutedEventHandler(this.OnDescLostFocus);
      desc.Navigate -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      desc.KeyDown -= new EventHandler<System.Windows.Input.KeyEventArgs>(this.OnContentKeyDown);
      desc.MoveUp -= new EventHandler(this.OnContentMoveUp);
      desc.MoveDown -= new EventHandler(this.OnDescMoveDown);
      desc.EnterImmersive -= new EventHandler(this.OnEnterImmersive);
      desc.SelectDate -= new EventHandler(this.OnSelectDate);
      desc.TextChanged -= new EventHandler(this.OnDescriptionChanged);
      desc.EditBox.PreviewTextInput -= new TextCompositionEventHandler(this.OnContentInput);
      desc.SaveContent -= new EventHandler(this.TrySaveOnEnter);
    }

    private void BindContentEvent(MarkDownEditor contentText)
    {
      contentText.RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
      contentText.QuickPopupOpened += new EventHandler(this.OnTextPopupOpened);
      contentText.QuickPopupClosed += new EventHandler(this.OnTextPopupClosed);
      contentText.QuickItemSelected += new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      contentText.LinkPopupOpened += new EventHandler(this.OnLinkPopupOpened);
      contentText.LinkPopupClosed += new EventHandler(this.OnLinkPopupClosed);
      contentText.CaretVerticalOffsetChanged += new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      contentText.GotFocus += new RoutedEventHandler(this.OnContentGotFocus);
      contentText.LostFocus += new RoutedEventHandler(this.OnContentLostFocus);
      contentText.Navigate += new EventHandler<ProjectTask>(this.OnNavigateTask);
      contentText.KeyDown += new EventHandler<System.Windows.Input.KeyEventArgs>(this.OnContentKeyDown);
      contentText.MoveUp += new EventHandler(this.OnContentMoveUp);
      contentText.SizeChanged += new SizeChangedEventHandler(this.OnContentSizeChanged);
      contentText.EnterImmersive += new EventHandler(this.OnEnterImmersive);
      contentText.SelectDate += new EventHandler(this.OnSelectDate);
      contentText.TextChanged += new EventHandler(this.OnTaskContentChanged);
      contentText.EditBox.PreviewTextInput += new TextCompositionEventHandler(this.OnContentInput);
      contentText.SaveContent += new EventHandler(this.TrySaveOnEnter);
    }

    private void RemoveContentEvent(MarkDownEditor contentText)
    {
      contentText.RequestBringIntoView -= new RequestBringIntoViewEventHandler(this.OnBringIntoView);
      contentText.QuickPopupOpened -= new EventHandler(this.OnTextPopupOpened);
      contentText.QuickItemSelected -= new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      contentText.LinkPopupOpened -= new EventHandler(this.OnLinkPopupOpened);
      contentText.LinkPopupClosed -= new EventHandler(this.OnLinkPopupClosed);
      contentText.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      contentText.GotFocus -= new RoutedEventHandler(this.OnContentGotFocus);
      contentText.LostFocus -= new RoutedEventHandler(this.OnContentLostFocus);
      contentText.Navigate -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      contentText.KeyDown -= new EventHandler<System.Windows.Input.KeyEventArgs>(this.OnContentKeyDown);
      contentText.MoveUp -= new EventHandler(this.OnContentMoveUp);
      contentText.SizeChanged -= new SizeChangedEventHandler(this.OnContentSizeChanged);
      contentText.EnterImmersive -= new EventHandler(this.OnEnterImmersive);
      contentText.SelectDate -= new EventHandler(this.OnSelectDate);
      contentText.TextChanged -= new EventHandler(this.OnTaskContentChanged);
      contentText.EditBox.PreviewTextInput -= new TextCompositionEventHandler(this.OnContentInput);
      contentText.SaveContent -= new EventHandler(this.TrySaveOnEnter);
    }

    private void TrySaveOnEnter(object sender, EventArgs e) => this.OnEsc();

    private void OnContentInput(object sender, TextCompositionEventArgs e)
    {
      this.SetContentHint("text");
    }

    private async void OnTaskContentChanged(object sender, EventArgs e)
    {
      string str = this._contentText?.Text ?? string.Empty;
      if (!string.IsNullOrEmpty(this._guideLink) && !str.Contains("\n" + this._guideLink))
        str = str + "\n" + this._guideLink;
      if (this._navigating || this._contentText == null || !(str != this._task.TaskContent))
        return;
      this.ClearSaveItems();
      this._task.SourceViewModel.Content = str;
      TaskChangeNotifier.NotifyTaskTextChanged(this._task.TaskId);
      await this.TryDelayEditSaveSync();
    }

    private void ClearSaveItems()
    {
      this._savedItems = ((string) null, (List<TaskDetailItemModel>) null);
    }

    private void OnEnterImmersive(object sender, EventArgs e) => this.EnterImmersivePage();

    private void OnContentMoveUp(object sender, EventArgs e) => this._titleText.FocusEnd();

    private void OnContentKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      this.OnEsc();
    }

    private void OnDescLostFocus(object sender, RoutedEventArgs e)
    {
      this._descText?.UnRegisterInputHandler();
    }

    private void OnDescGotFocus(object sender, RoutedEventArgs e)
    {
      this._descText?.RegisterInputHandler();
    }

    private void OnContentLostFocus(object sender, RoutedEventArgs e)
    {
      this._contentText?.UnRegisterInputHandler();
    }

    private void OnContentGotFocus(object sender, RoutedEventArgs e)
    {
      this._contentText?.RegisterInputHandler();
    }

    private void OnContentSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size size = e.NewSize;
      double height1 = size.Height;
      size = e.PreviousSize;
      double height2 = size.Height;
      if (Math.Abs(height1 - height2) <= 10.0 || this._attachmentOptionPanel.Visibility != Visibility.Visible)
        return;
      this.HideAttachmentOption();
    }

    private void OnDescMoveDown(object sender, EventArgs e) => this._checklist?.FocusFirstItem();

    private async void OnDescriptionChanged(object sender, EventArgs e)
    {
      if (this._descText == null || !(this._task.Desc != this._descText.Text))
        return;
      string text = this._descText.Text;
      this._task.SourceViewModel.Desc = text;
      this._switchListOriginText = (string) null;
      TaskChangeNotifier.NotifyTaskTextChanged(this._task.TaskId);
      await this.TryDelayEditSaveSync();
      if (text.Length == 2048)
        this.TryToast(Utils.GetString("DescriptionLengthError"));
      text = (string) null;
    }

    private void SetTextEditorMenu(bool show)
    {
      if (show)
      {
        if (this._textEditorMenu == null)
        {
          EditorMenu editorMenu = new EditorMenu();
          editorMenu.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
          editorMenu.VerticalAlignment = VerticalAlignment.Bottom;
          editorMenu.Background = (Brush) Brushes.Transparent;
          this._textEditorMenu = editorMenu;
          this._textEditorMenu.SetValue(Grid.RowProperty, (object) 1);
          this._textEditorMenu.SetValue(Grid.RowSpanProperty, (object) 2);
          this._textEditorMenu.EditorAction += new EventHandler<string>(this.OnEditorAction);
          this._textEditorMenu.EnterImmersive += new EventHandler(this.OnImmersiveClick);
          this._textEditorMenu.ExitImmersive += new EventHandler(this.OnExitImmersiveClick);
          this.Children.Add((UIElement) this._textEditorMenu);
        }
      }
      else if (this._textEditorMenu != null)
      {
        this._textEditorMenu.EditorAction -= new EventHandler<string>(this.OnEditorAction);
        this._textEditorMenu.EnterImmersive -= new EventHandler(this.OnImmersiveClick);
        this._textEditorMenu.ExitImmersive -= new EventHandler(this.OnExitImmersiveClick);
        this._textEditorMenu.MouseEnter -= new System.Windows.Input.MouseEventHandler(this.OnMenuMouseEnter);
        this._textEditorMenu.MouseLeave -= new System.Windows.Input.MouseEventHandler(this.OnMenuMouseLeave);
        this.Children.Remove((UIElement) this._textEditorMenu);
        this._textEditorMenu = (EditorMenu) null;
      }
      this._scrollContent.Margin = new Thickness(20.0, 0.0, 20.0, this.Mode == Constants.DetailMode.Editor | show ? 40.0 : 0.0);
      this._detailBottom?.SetItemVisible("EditIconSelectedBorder", show);
    }

    private void OnExitImmersiveClick(object sender, EventArgs e)
    {
      EventHandler exitImmersive = this.ExitImmersive;
      if (exitImmersive != null)
        exitImmersive((object) this, (EventArgs) null);
      this.AddActionEvent("md_toolbar", "enter_or_exit_fullscreen");
    }

    private void OnImmersiveClick(object sender, EventArgs e)
    {
      this.EnterImmersivePage();
      this.AddActionEvent("md_toolbar", "enter_or_exit_fullscreen");
    }

    private async void OnEditorAction(object sender, string tag) => this.SetContentStyle(tag);

    public async void SetContentStyle(string tag)
    {
      MarkDownEditor editor = this._task.Kind == "CHECKLIST" ? this._descText : this._contentText;
      if (editor == null)
      {
        editor = (MarkDownEditor) null;
      }
      else
      {
        string[] strArray = tag.Split('_');
        string empty = string.Empty;
        if (strArray.Length > 1)
        {
          tag = strArray[0];
          empty = strArray[1];
        }
        string str = tag;
        if (str != null)
        {
          switch (str.Length)
          {
            case 4:
              switch (str[0])
              {
                case 'B':
                  if (str == "Bold")
                  {
                    editor.Bold();
                    this.AddActionEvent("md_toolbar", "bold");
                    break;
                  }
                  break;
                case 'C':
                  if (str == "Code")
                  {
                    editor.Code();
                    this.AddActionEvent("md_toolbar", "code");
                    break;
                  }
                  break;
                case 'L':
                  if (str == "Link")
                  {
                    editor.ShowInsertLink(editor.EditBox.SelectedText, string.Empty);
                    this.AddActionEvent("md_toolbar", "link");
                    break;
                  }
                  break;
              }
              break;
            case 5:
              if (str == "Quote")
              {
                editor.Quote();
                this.AddActionEvent("md_toolbar", "quote");
                break;
              }
              break;
            case 6:
              if (str == "Italic")
              {
                editor.Italic();
                this.AddActionEvent("md_toolbar", "italic");
                break;
              }
              break;
            case 8:
              switch (str[7])
              {
                case '1':
                  if (str == "Heading1")
                  {
                    editor.InsertHeader(1);
                    this.AddActionEvent("md_toolbar", "header");
                    break;
                  }
                  break;
                case '2':
                  if (str == "Heading2")
                  {
                    editor.InsertHeader(2);
                    this.AddActionEvent("md_toolbar", "header");
                    break;
                  }
                  break;
                case '3':
                  if (str == "Heading3")
                  {
                    editor.InsertHeader(3);
                    this.AddActionEvent("md_toolbar", "header");
                    break;
                  }
                  break;
                case 'e':
                  if (str == "DateTime")
                  {
                    editor.AddText(empty);
                    this.AddActionEvent("md_toolbar", "datetime");
                    break;
                  }
                  break;
              }
              break;
            case 9:
              switch (str[0])
              {
                case 'C':
                  if (str == "CheckItem")
                  {
                    editor.InsertCheckItem();
                    this.AddActionEvent("md_toolbar", "checkbox");
                    break;
                  }
                  break;
                case 'H':
                  if (str == "HighLight")
                  {
                    editor.Highlight();
                    this.AddActionEvent("md_toolbar", "highlight");
                    break;
                  }
                  break;
                case 'S':
                  if (str == "SplitLine")
                  {
                    editor.InsertLine();
                    this.AddActionEvent("md_toolbar", "line");
                    break;
                  }
                  break;
                case 'U':
                  if (str == "UnderLine")
                  {
                    editor.UnderLine();
                    this.AddActionEvent("md_toolbar", "underline");
                    break;
                  }
                  break;
              }
              break;
            case 10:
              switch (str[0])
              {
                case 'A':
                  if (str == "Attachment")
                  {
                    this.ShowUploadFileDialog();
                    break;
                  }
                  break;
                case 'B':
                  if (str == "BulletList")
                  {
                    editor.InsertUnOrderList();
                    this.AddActionEvent("md_toolbar", "unsorted_list");
                    break;
                  }
                  break;
              }
              break;
            case 12:
              if (str == "NumberedList")
              {
                editor.InsertNumberedList();
                this.AddActionEvent("md_toolbar", "sorted_list");
                break;
              }
              break;
            case 13:
              if (str == "StrikeThrough")
              {
                editor.StrokeLine();
                this.AddActionEvent("md_toolbar", "strike");
                break;
              }
              break;
          }
        }
        await Task.Delay(100);
        editor.EditBox.TryFocus();
        editor = (MarkDownEditor) null;
      }
    }

    private void OnCheckItemsDeleted(object sender, TaskDetailItemModel e)
    {
      EventHandler<TaskDetailItemModel> checkItemsDeleted = this.CheckItemsDeleted;
      if (checkItemsDeleted == null)
        return;
      checkItemsDeleted(sender, e);
    }

    private void OnListCaretMoveUp(object sender, EventArgs e) => this._descText?.FocusEnd();

    private void TryToastUnableText(object sender, string e)
    {
      if (string.IsNullOrEmpty(e))
        this.CheckEnable();
      else
        this.TryToast(e);
    }

    private async void OnItemDrop(object sender, string itemId)
    {
      EventHandler<string> checkItemDragDrop = this.CheckItemDragDrop;
      if (checkItemDragDrop != null)
        checkItemDragDrop(sender, itemId);
      TaskListView subtaskList = this.GetSubtaskList();
      if ((subtaskList != null ? (!subtaskList.OnCheckItemDrop(itemId) ? 1 : 0) : 1) == 0)
        return;
      this.Reload(this._task.TaskId);
      await TaskService.OnCheckItemChanged(this._task.TaskId);
    }

    private async void OnItemsEmpty(object sender, EventArgs e)
    {
      if (this._task.IsNewAdd)
      {
        await this.SwitchToText();
      }
      else
      {
        this.Reload(this._task.TaskId);
        await Task.Delay(500);
        this._contentText?.FocusEnd();
      }
    }

    private async void ChecklistModified(string taskid, string itemid, CheckItemModifyType type)
    {
      TaskDetailView taskDetailView1 = this;
      taskDetailView1._switchListOriginText = (string) null;
      if (type != CheckItemModifyType.SetTime)
        await taskDetailView1.SyncTaskProgress();
      int? progress;
      switch (type)
      {
        case CheckItemModifyType.Add:
        case CheckItemModifyType.Uncheck:
          if (taskDetailView1._task.Status != 0)
          {
            TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(taskDetailView1._task.TaskId, 0);
            break;
          }
          break;
        case CheckItemModifyType.Check:
          progress = taskDetailView1._task.Progress;
          int num = 100;
          if (progress.GetValueOrDefault() == num & progress.HasValue && taskDetailView1._task.Status == 0 && TaskCache.CanTaskCompletedByCheckItem(taskDetailView1._task.TaskId))
          {
            if ((await TaskService.SetTaskStatus(taskDetailView1._task.TaskId, 2)).RepeatTask != null)
              await taskDetailView1.Navigate(taskDetailView1._task.TaskId);
            if (taskDetailView1.Mode == Constants.DetailMode.Sticky && taskDetailView1._task.Status != 0)
            {
              taskDetailView1.TryToast(Utils.GetString("TaskCompleted"));
              break;
            }
            break;
          }
          break;
      }
      if (type == CheckItemModifyType.TitleChange)
        return;
      TaskDetailView taskDetailView2 = taskDetailView1;
      progress = taskDetailView1._task.Progress;
      int valueOrDefault = progress.GetValueOrDefault();
      taskDetailView2.SetProgress(valueOrDefault, true);
    }

    private async Task SyncTaskProgress()
    {
      this._task.SourceViewModel.Progress = await TaskService.CalculateProgress(this._task.TaskId);
      await this.SaveTask();
    }

    private void SetContentHint(string text)
    {
      if (this.Mode == Constants.DetailMode.Sticky)
        return;
      if (string.IsNullOrEmpty(text))
      {
        if (this._hintTextPanel == null)
        {
          this._hintTextPanel = new StackPanel()
          {
            Orientation = System.Windows.Controls.Orientation.Horizontal
          };
          TextBlock textBlock = new TextBlock();
          textBlock.IsHitTestVisible = false;
          TextBlock element = textBlock;
          element.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
          element.SetResourceReference(TextBlock.FontSizeProperty, this.Mode == Constants.DetailMode.Sticky ? (object) "StickyFont13" : (object) "Font14");
          this._contentGrid.Children.Add((UIElement) this._hintTextPanel);
          this._hintTextPanel.Children.Add((UIElement) element);
          this._hintTextPanel.SetValue(System.Windows.Controls.Panel.ZIndexProperty, (object) 10);
        }
        TextBlock child = (TextBlock) this._hintTextPanel.Children[0];
        switch (this._task.Kind)
        {
          case "TEXT":
          case "CHECKLIST":
            child.Text = Utils.GetString("Description");
            if (this._hintTextPanel.Children.Count >= 2)
            {
              this._hintTextPanel.Children.RemoveRange(1, this._hintTextPanel.Children.Count - 1);
              break;
            }
            break;
          case "NOTE":
            child.Text = Utils.GetString("NoteDescribe") + " ";
            TextBlock element1;
            if (this._hintTextPanel.Children.Count == 2)
            {
              element1 = (TextBlock) this._hintTextPanel.Children[1];
            }
            else
            {
              element1 = new TextBlock()
              {
                Text = Utils.GetString("AddFromTemplate"),
                TextDecorations = TextDecorations.Underline,
                Background = (Brush) Brushes.Transparent
              };
              element1.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
              element1.SetResourceReference(TextBlock.FontSizeProperty, this.Mode == Constants.DetailMode.Sticky ? (object) "StickyFont13" : (object) "Font14");
              element1.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowNoteTemplate);
              this._hintTextPanel.Children.Add((UIElement) element1);
            }
            element1.Cursor = this._task.Enable ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.No;
            break;
        }
        this._hintTextPanel.Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
      }
      else
      {
        if (this._hintTextPanel == null)
          return;
        this._contentGrid.Children.Remove((UIElement) this._hintTextPanel);
        this._hintTextPanel = (StackPanel) null;
      }
    }

    private void ShowNoteTemplate(object sender, MouseButtonEventArgs e)
    {
      if (!this.CheckEnable())
        return;
      this.OnShowDialog();
      AddTemplateDialog addTemplateDialog = new AddTemplateDialog(TemplateKind.Note, new AddTaskViewModel()
      {
        ProjectId = this._task.ProjectId
      });
      addTemplateDialog.Owner = Window.GetWindow((DependencyObject) this);
      addTemplateDialog.TemplateSelected -= new EventHandler<string>(this.OnTemplateSelected);
      addTemplateDialog.TemplateSelected += new EventHandler<string>(this.OnTemplateSelected);
      addTemplateDialog.ShowDialog();
      addTemplateDialog.Activate();
      addTemplateDialog.Topmost = true;
      this.OnCloseDialog();
    }

    private async void OnTemplateSelected(object sender, string e)
    {
      if (sender is AddTemplateDialog addTemplateDialog)
        addTemplateDialog.Close();
      TaskBaseViewModel note = TaskCache.GetTaskById(this._task?.TaskId);
      TaskTemplateModel templateById = await TemplateDao.GetTemplateById(e);
      if (templateById == null)
        note = (TaskBaseViewModel) null;
      else if (!templateById.IsNote)
        note = (TaskBaseViewModel) null;
      else if (this._task == null)
        note = (TaskBaseViewModel) null;
      else if (!(this._task.Kind == "NOTE"))
      {
        note = (TaskBaseViewModel) null;
      }
      else
      {
        if (string.IsNullOrEmpty(this._titleText.Text.Trim()))
        {
          this._task.SourceViewModel.Title = templateById.Title;
          this._titleText.ResetText(templateById.Title);
        }
        this._contentText?.SetText(templateById.Content?.Replace("\r\n", "\r")?.Replace("\r", "\r\n"));
        if (note == null)
        {
          note = (TaskBaseViewModel) null;
        }
        else
        {
          List<string> tags1 = templateById.Tags;
          // ISSUE: explicit non-virtual call
          if ((tags1 != null ? (__nonvirtual (tags1.Count) > 0 ? 1 : 0) : 0) == 0)
          {
            note = (TaskBaseViewModel) null;
          }
          else
          {
            List<string> tags = TagSerializer.ToTags(note.Tag);
            tags = tags.Union<string>((IEnumerable<string>) templateById.Tags).ToList<string>();
            if (tags.Any<string>())
            {
              List<string> localTags = CacheManager.GetTags().Select<TagModel, string>((Func<TagModel, string>) (tag => tag.name)).ToList<string>();
              List<string> list = tags.Where<string>((Func<string, bool>) (tag => !localTags.Contains(tag.ToLower()))).ToList<string>();
              if (list.Any<string>())
              {
                foreach (string tag1 in list)
                {
                  TagModel tag2 = await TagService.TryCreateTag(tag1);
                }
              }
              this.NotifyTagsChanged(tags);
            }
            await TaskService.SetTags(note.Id, tags);
            tags = (List<string>) null;
            note = (TaskBaseViewModel) null;
          }
        }
      }
    }

    private void HandleAttachmentMouseEnter(object sender)
    {
      if (!(sender is Border bd))
        return;
      this.OnAttachmentMouseEnter(bd);
    }

    public async void OnAttachmentMouseEnter(Border bd)
    {
      int num = this.Mode == Constants.DetailMode.Sticky ? 2 : 5;
      if (bd.Tag is AttachmentMDDisplayModel tag1)
      {
        System.Windows.Point point = bd.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this._contentGrid);
        this._attachmentOptionPanel.Margin = new Thickness(point.X, point.Y + (double) num, 0.0, 0.0);
        this._attachmentOptionPanel.Visibility = Visibility.Visible;
        this._attachmentOptionPanel.SetPanel(tag1, bd.Width, bd.Height, this._task.Enable, this._task.ImageMode);
        this._attachmentOptionPanel.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(), 1.0, 150));
      }
      else
      {
        if (!(bd.Tag is AttachmentViewModel tag))
          return;
        System.Windows.Point point = bd.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this._contentGrid);
        this._attachmentOptionPanel.Margin = new Thickness(point.X, point.Y + (double) num, 0.0, 0.0);
        this._attachmentOptionPanel.Visibility = Visibility.Visible;
        this._attachmentOptionPanel.SetPanel(tag, bd.Width, bd.Height, this._task.Enable, this._task.ImageMode);
        this._attachmentOptionPanel.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(), 1.0, 150));
      }
    }

    private async void OnImageModeChanged(object sender, int mode)
    {
      if (this._task == null || !this.CheckEnable())
        return;
      await TaskService.SetImageMode(this._task.TaskId, mode);
    }

    private async void OnCheckListAttachmentDeleteClick(object sender, AttachmentModel model)
    {
      TaskDetailView sender1 = this;
      if (model == null)
        return;
      if (!sender1._task.IsOwner)
      {
        sender1.TryToast(Utils.GetString("AttendeeModifyContent"));
      }
      else
      {
        UtilLog.Info("TaskDetail.DeleteAttachment taskId " + sender1._task?.TaskId + ", attachment " + model.id + ", from: checkList");
        await AttachmentDao.FakeDeleteAttachment(model.id);
        await AttachmentDao.SetAttachemntActive(sender1._task.ProjectId, sender1._task.TaskId, model.id, 1);
        TaskChangeNotifier.NotifyAttachmentChanged(sender1._task.TaskId, (object) sender1);
        TaskDetailViewModel taskDetailViewModel = sender1._task;
        taskDetailViewModel.Attachments = (await AttachmentDao.GetTaskAttachments(sender1._task.TaskId)).ToArray();
        taskDetailViewModel = (TaskDetailViewModel) null;
        await SyncStatusDao.AddSyncStatus(sender1._task.TaskId, 0);
        sender1.LoadAttachmentAsync();
      }
    }

    private void OnAttachmentDeleteClick(object sender, AttachmentInfo info)
    {
      UtilLog.Info("TaskDetail.DeleteAttachment taskId " + this._task?.TaskId + ", attachment " + info.Url + ", from: OptionPanel");
      this._contentText?.TryDeleteAttachment(info.Offset);
    }

    private void OnAttachmentMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      this.HideAttachmentOption();
    }

    private async Task InitInputViews(TaskDetailViewModel task, bool restoreOffset)
    {
      this.LoadContentControl(restoreOffset);
      this.LoadTaskTitle(restoreOffset);
      string str = await this.CheckTaskAttachmentText(task);
      this._detailBottom?.SetItemVisible("CommentGrid", !this._task.IsNewAdd);
    }

    private async Task<string> CheckTaskAttachmentText(TaskDetailViewModel task)
    {
      if (task.Kind == "CHECKLIST")
        return task.TaskContent;
      TaskModel taskModel = await TaskDao.GetTaskById(task.TaskId);
      if (taskModel != null)
      {
        string content = taskModel.content ?? string.Empty;
        List<AttachmentModel> taskAttachments = await AttachmentDao.GetTaskAttachments(taskModel.id, true);
        if (taskAttachments == null)
          return task.TaskContent;
        foreach (AttachmentModel attachment in taskAttachments)
          AttachmentCache.SetAttachment(attachment);
        List<AttachmentModel> list = taskAttachments.Where<AttachmentModel>((Func<AttachmentModel, bool>) (a => !content.Contains(a.id))).ToList<AttachmentModel>();
        if (list.Count > 0)
        {
          taskModel.content = AttachmentDao.AddAttachmentStrings(content, list);
          await TaskService.UpdateTaskOnContentChanged(taskModel);
          await SyncStatusDao.AddModifySyncStatus(taskModel.id);
          SyncManager.TryDelaySync();
        }
        this._contentText?.Redraw();
      }
      return task.TaskContent;
    }

    public void OnTaskDragging(DragMouseEvent arg)
    {
      if (this._task == null || !this._task.Enable)
        return;
      this._checklist?.HandleTaskMove(arg);
    }

    public void TryFocusDetail(bool focusEnd = true, string taskId = null)
    {
      if (!string.IsNullOrEmpty(taskId) && taskId != this._task.TaskId)
        return;
      if (this._contentText != null)
        this._contentText.FocusEditBox();
      else if (focusEnd)
        this._descText?.FocusEnd();
      else
        this._descText?.FocusFirst();
    }

    public List<CheckItemViewModel> GetChecklistItems()
    {
      ChecklistControl checklist = this._checklist;
      if (checklist == null)
        return (List<CheckItemViewModel>) null;
      List<CheckItemViewModel> checklistItems = checklist.ChecklistItems;
      return checklistItems == null ? (List<CheckItemViewModel>) null : checklistItems.ToList<CheckItemViewModel>();
    }

    public ChecklistControl GetChecklist() => this._checklist;

    public MarkDownEditor GetContentText() => this._contentText;

    public MarkDownEditor GetDescText() => this._descText;

    public event EventHandler<string> TaskNavigated;

    public event EventHandler<DragMouseEvent> SubtaskDragOver;

    public event EventHandler ActionPopOpened;

    public event EventHandler<bool> ActionPopClosed;

    public event EventHandler EscKeyUp;

    public event EventHandler<string> ShowUndoOnTaskDeleted;

    public event EventHandler<string> TaskDeleted;

    public event EventHandler NotifyCloseWindow;

    public event EnterImmersiveDelegate EnterImmersive;

    public event EventHandler ExitImmersive;

    public event EventHandler<string> TaskCopied;

    public event EventHandler FocusList;

    public event EventHandler NavigateBack;

    public event EventHandler ForceHideWindow;

    public event EventHandler SubTaskChanged;

    public event EventHandler<ProjectTask> NavigateTask;

    public event EventHandler<string> CheckItemDragDrop;

    public event EventHandler<TaskDetailItemModel> CheckItemsDeleted;

    private void BindEvents()
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      DataChangedNotifier.IsDarkChanged += new EventHandler(this.ReloadOnThemeChanged);
      DataChangedNotifier.HideCompleteChanged += new EventHandler(this.TryReloadSubTasks);
      DataChangedNotifier.PomoChanged += new EventHandler<string>(this.OnPomoChanged);
      DataChangedNotifier.TagChanged += new EventHandler<TagModel>(this.OnTagModelChanged);
      AttachmentLoadHelper.Downloaded += new EventHandler<string>(this.OnAttachmentLoadChanged);
      AttachmentLoadHelper.DownloadFailed += new EventHandler<string>(this.OnAttachmentLoadChanged);
      AttachmentLoadHelper.Uploaded += new EventHandler<string>(this.OnAttachmentLoadChanged);
      AttachmentLoadHelper.UploadFailed += new EventHandler<string>(this.OnAttachmentLoadChanged);
    }

    private void UnbindEvents()
    {
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      DataChangedNotifier.IsDarkChanged -= new EventHandler(this.ReloadOnThemeChanged);
      DataChangedNotifier.HideCompleteChanged -= new EventHandler(this.TryReloadSubTasks);
      DataChangedNotifier.PomoChanged -= new EventHandler<string>(this.OnPomoChanged);
      DataChangedNotifier.TagChanged -= new EventHandler<TagModel>(this.OnTagModelChanged);
      AttachmentLoadHelper.Downloaded -= new EventHandler<string>(this.OnAttachmentLoadChanged);
      AttachmentLoadHelper.DownloadFailed -= new EventHandler<string>(this.OnAttachmentLoadChanged);
      AttachmentLoadHelper.Uploaded -= new EventHandler<string>(this.OnAttachmentLoadChanged);
      AttachmentLoadHelper.UploadFailed -= new EventHandler<string>(this.OnAttachmentLoadChanged);
    }

    private void OnAttachmentLoadChanged(object sender, string e)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (!(this._task.Kind != "CHECKLIST"))
          return;
        this._contentText?.Redraw();
      }));
    }

    private void OnTagModelChanged(object sender, TagModel e) => this.TryDisplayTags();

    private void OnPomoChanged(object sender, string e)
    {
      if (this.Mode == Constants.DetailMode.Sticky || this.Immerse)
        return;
      this.TryLoadPomoCount(this._task);
    }

    private void TryReloadSubTasks(object sender, EventArgs e) => this.SetSubtasks(this._task);

    private void ReloadOnThemeChanged(object sender, EventArgs e) => this._task?.OnThemeChanged();

    private async void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      TaskDetailView taskDetailView = this;
      if (!taskDetailView.IsVisible)
        return;
      if (e.BatchChangedIds.Any())
      {
        if (taskDetailView._task.TaskId != null && e.BatchChangedIds.Contains(taskDetailView._task.TaskId))
          taskDetailView.Reload(taskDetailView._task.TaskId);
        else
          taskDetailView.OnTaskBatchChanged((IEnumerable<string>) e.BatchChangedIds.Value);
      }
      else
      {
        if (e.DeletedChangedIds.Any())
          taskDetailView.OnTaskBatchDeleted(e.DeletedChangedIds.Value);
        if (e.AttachmentChangedIds.Any() && taskDetailView._task.TaskId != null && e.AttachmentChangedIds.Contains(taskDetailView._task.TaskId))
          taskDetailView.LoadAttachmentAsync();
        if (e.PinChangedIds.Any() && e.PinChangedIds.Contains(taskDetailView._task.TaskId))
          taskDetailView.SetStarGrid();
        if (e.UndoDeletedIds.Any())
          taskDetailView.OnTaskBatchChanged((IEnumerable<string>) e.UndoDeletedIds.Value);
        if (e.ProjectChangedIds.Any())
          taskDetailView.OnTaskBatchChanged((IEnumerable<string>) e.ProjectChangedIds.Value);
        if (e.AddIds.Any())
          taskDetailView.OnTaskBatchChanged((IEnumerable<string>) e.AddIds.Value);
        if (e.StatusChangedIds.Any())
          taskDetailView.OnTaskBatchChanged((IEnumerable<string>) e.StatusChangedIds.Value);
        if (e.DateChangedIds.Any() && e.DateChangedIds.Contains(taskDetailView._task.TaskId))
        {
          List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(taskDetailView._task.TaskId);
          taskDetailView._task.Reminders = remindersByTaskId.ToArray();
          taskDetailView._task.SetPropertyChanged("ShowSnoozeText");
          taskDetailView._task.SetPropertyChanged("RemindTimeText");
        }
        if (e.CheckItemChangedIds.Any() && taskDetailView._task.Kind == "CHECKLIST")
          taskDetailView.Reload(loadComment: false);
        if (e.KindChangedIds.Any() && e.KindChangedIds.Contains(taskDetailView._task.TaskId))
          taskDetailView.Reload(loadComment: false);
        if (!e.SortOrderChangedIds.Any())
          return;
        if (e.SortOrderChangedIds.Contains(taskDetailView._task.TaskId))
          taskDetailView.SetParentTitle(taskDetailView._task);
        // ISSUE: reference to a compiler-generated method
        // ISSUE: reference to a compiler-generated method
        if ((taskDetailView._subTaskList == null || !e.SortOrderChangedIds.ToList().Any<string>(new Func<string, bool>(taskDetailView.\u003COnTasksChanged\u003Eb__380_0))) && !e.SortOrderChangedIds.ToList().Any<string>(new Func<string, bool>(taskDetailView.\u003COnTasksChanged\u003Eb__380_1)))
          return;
        taskDetailView.SetSubtasks(taskDetailView._task);
      }
    }

    private void SetStarGrid(bool? isPin = null)
    {
      if (this._task == null)
        return;
      this._detailBottom?.SetPin(((int) isPin ?? (this._task.Pinned ? 1 : 0)) != 0);
    }

    private void OnTaskBatchDeleted(HashSet<string> ids)
    {
      if (string.IsNullOrEmpty(this._task?.TaskId))
        return;
      TaskListView subTaskList = this._subTaskList;
      List<DisplayItemModel> list = subTaskList != null ? subTaskList.ViewModel.Items.ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      if (list == null || !list.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.Id))))
        return;
      this.SetSubtasks(this._task);
    }

    private void OnTaskBatchChanged(IEnumerable<string> ids)
    {
      if (string.IsNullOrEmpty(this._task?.TaskId))
        return;
      TaskListView subTaskList = this._subTaskList;
      List<DisplayItemModel> list = subTaskList != null ? subTaskList.ViewModel.Items.ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      foreach (string id1 in ids)
      {
        string id = id1;
        TaskBaseViewModel task = TaskCache.GetTaskById(id);
        if (task != null)
        {
          if (id == this._task.TaskId)
            this.SetParentTitle(this._task);
          DisplayItemModel displayItemModel = list != null ? list.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Id == id)) : (DisplayItemModel) null;
          if (displayItemModel != null && (string.IsNullOrEmpty(displayItemModel.ParentId) || displayItemModel.ProjectId != this._task.ProjectId || displayItemModel.ColumnId != this._task.ColumnId))
          {
            this.SetSubtasks(this._task);
            break;
          }
          if (task.TaskRepeatId == this._task.TaskId || (list == null || list.All<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Id != task.Id))) && (task.ParentId == this._task.TaskId || list != null && list.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Id == task.ParentId))))
          {
            this.SetSubtasks(this._task);
            break;
          }
        }
      }
    }

    private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      string propertyName = e.PropertyName;
      if (propertyName == null)
        return;
      switch (propertyName.Length)
      {
        case 3:
          if (!(propertyName == "Tag"))
            return;
          this.OnTaskTagsChanged();
          return;
        case 4:
          if (!(propertyName == "Desc"))
            return;
          this.OnTaskDescChanged();
          return;
        case 5:
          if (!(propertyName == "Title"))
            return;
          this.OnTaskTitleChanged();
          return;
        case 6:
          if (!(propertyName == "Status"))
            return;
          break;
        case 7:
          return;
        case 8:
          switch (propertyName[2])
          {
            case 'i':
              if (!(propertyName == "Priority"))
                return;
              break;
            case 'm':
              if (!(propertyName == "Comments"))
                return;
              this.OnCommentChanged();
              return;
            case 'o':
              if (!(propertyName == "Progress"))
                return;
              this.OnTaskProgressChanged();
              return;
            case 's':
              if (!(propertyName == "Assignee"))
                return;
              this.TrySetAvatar();
              return;
            default:
              return;
          }
          break;
        case 9:
          switch (propertyName[0])
          {
            case 'I':
              if (!(propertyName == "ImageMode"))
                return;
              this.OnTaskImageModeChanged();
              return;
            case 'P':
              if (!(propertyName == "ProjectId"))
                return;
              this.OnProjectIdChanged();
              return;
            default:
              return;
          }
        case 10:
          if (!(propertyName == "CheckItems"))
            return;
          this.OnCheckItemsChanged();
          return;
        case 11:
          if (!(propertyName == "TaskContent"))
            return;
          this.OnTaskContentChanged();
          return;
        default:
          return;
      }
      this._detailHead?.SetIconColor(this._task.Priority, this._task.Status);
    }

    private void OnTaskTitleChanged()
    {
      if (!this._titleText.KeyboardFocused && this._titleText.Text != this._task.Title)
        this._titleText.SetTextOffset(this._task.Title, false);
      this.SetTitleHint(this._task.Title);
    }

    private void OnTaskContentChanged()
    {
      if (!(this._task.Kind != "CHECKLIST"))
        return;
      if (this._contentText != null && !this._contentText.KeyboardFocused && this._contentText.Text != this._task.TaskContent)
      {
        string text = this._task.TaskContent;
        if (this._guideTaskId == this._task.TaskId && !string.IsNullOrEmpty(this._guideLink) && text.Contains("\n" + this._guideLink))
          text = text.TrimEnd().Replace("\n" + this._guideLink, "");
        this._contentText.SetTextAndOffset(text, false);
        this.ClearSaveItems();
      }
      this.SetContentHint(this._task.TaskContent);
    }

    private void OnTaskDescChanged()
    {
      if (!(this._task.Kind == "CHECKLIST"))
        return;
      if (this._descText != null && !this._descText.KeyboardFocused && this._descText.Text != this._task.Desc)
        this._descText.SetTextAndOffset(this._task.Desc, false);
      this.SetContentHint(this._task.Desc);
    }

    private void OnCheckItemsChanged()
    {
      try
      {
        List<TaskBaseViewModel> list = this._task.SourceViewModel?.CheckItems?.ToList();
        if (list != null && list.Count<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (i => i.Deleted == 0)) == this._checklist?.ChecklistItems?.Count.GetValueOrDefault())
          return;
        this._task.Items = (TaskDetailItemModel[]) null;
        this.LoadChecklistView(this._task, (string) null);
      }
      catch (Exception ex)
      {
        Console.WriteLine((object) ex);
        throw;
      }
    }

    private async void OnCommentChanged()
    {
      if (this.Mode == Constants.DetailMode.Sticky || this.Mode == Constants.DetailMode.Editor)
        return;
      if (this._commentDisplayCtrl?.Model?.Count.ToString() != this._task.CommentCount)
      {
        this.TryLoadLocalComments();
      }
      else
      {
        List<CommentModel> commentsByTaskId = await CommentDao.GetCommentsByTaskId(this._task.TaskId);
        // ISSUE: explicit non-virtual call
        if (commentsByTaskId == null || __nonvirtual (commentsByTaskId.Count) <= 0 || ((int) this._commentDisplayCtrl?.Model?.CheckComments(commentsByTaskId) ?? 1) == 0)
          return;
        this.TryLoadLocalComments();
      }
    }

    private void OnTaskTagsChanged() => this.TryDisplayTags();

    private void TrySetAvatar() => this.SetAvatar();

    private void OnTaskImageModeChanged()
    {
      if (!(this._task.Kind == "CHECKLIST"))
        return;
      this.LoadAttachmentAsync();
    }

    private void OnTaskProgressChanged() => this._detailHead?.CheckProgress();

    private void OnProjectIdChanged()
    {
      this.TrySetAvatar();
      this.SetParentTitle(this._task);
    }

    public void ClearEvents()
    {
      this.TaskNavigated = (EventHandler<string>) null;
      this.SubtaskDragOver = (EventHandler<DragMouseEvent>) null;
      this.ActionPopOpened = (EventHandler) null;
      this.ActionPopClosed = (EventHandler<bool>) null;
      this.EscKeyUp = (EventHandler) null;
      this.ShowUndoOnTaskDeleted = (EventHandler<string>) null;
      this.TaskDeleted = (EventHandler<string>) null;
      this.NotifyCloseWindow = (EventHandler) null;
      this.EnterImmersive = (EnterImmersiveDelegate) null;
      this.ExitImmersive = (EventHandler) null;
      this.TaskCopied = (EventHandler<string>) null;
      this.FocusList = (EventHandler) null;
      this.NavigateBack = (EventHandler) null;
      this.ForceHideWindow = (EventHandler) null;
      this.SubTaskChanged = (EventHandler) null;
      this.NavigateTask = (EventHandler<ProjectTask>) null;
      this.CheckItemDragDrop = (EventHandler<string>) null;
      this.CheckItemsDeleted = (EventHandler<TaskDetailItemModel>) null;
      this.RecordLabelTime();
      if (this._tagsControl == null)
        return;
      this._tagsControl.TagsChanged -= new EventHandler<List<string>>(this.OnTagsChanged);
      this._tagsControl.TagClick -= new EventHandler<string>(this.OnTagLabelClick);
      this._tagsControl.TagPanelVisibleChanged -= new EventHandler<Visibility>(this.OnTagPanelVisibleChanged);
      this._contentGrid.Children.Remove((UIElement) this._tagsControl);
      this._tagsControl = (TagDisplayControl) null;
    }

    public void Dispose() => this.ClearEvents();

    private async Task SetSubtasks(TaskDetailViewModel taskModel, bool restoreSelected = true)
    {
      TaskDetailView taskDetailView = this;
      int num = await taskDetailView._subTaskAsyncLocker.RunAsync(new Func<TaskDetailViewModel, bool, Task<int>>(taskDetailView.SetSubtasksView), taskModel, restoreSelected);
    }

    private TaskListView GetSubtaskList()
    {
      StackPanel subtaskGrid = this._subtaskGrid;
      return subtaskGrid != null && subtaskGrid.Children.Count >= 2 && subtaskGrid.Children[1] is TaskListView child ? child : (TaskListView) null;
    }

    private async Task<int> SetSubtasksView(TaskDetailViewModel taskModel, bool restoreSelected = true)
    {
      TaskDetailView editor = this;
      Node<DisplayItemModel> taskNode = (Node<DisplayItemModel>) null;
      List<DisplayItemModel> displayItemModelList = (List<DisplayItemModel>) null;
      if (taskModel != null && taskModel.Deleted == 0 && !taskModel.IsNewAdd && editor.Mode != Constants.DetailMode.Editor && taskModel.Kind != "NOTE")
      {
        taskNode = TaskDao.GetDisplayItemNode(taskModel.TaskId, taskModel.ProjectId, !LocalSettings.Settings.HideComplete);
        await editor.SortChildrenBySortOrder(taskNode);
        displayItemModelList = taskNode?.GetAllChildrenValue();
      }
      bool inSticky = editor.Mode == Constants.DetailMode.Sticky;
      // ISSUE: explicit non-virtual call
      if (displayItemModelList != null && __nonvirtual (displayItemModelList.Count) > 0)
      {
        displayItemModelList.ForEach((Action<DisplayItemModel>) (m =>
        {
          m.InDetail = true;
          m.InSticky = inSticky;
          --m.Level;
          m.ShowDragBar = !inSticky;
        }));
        List<DisplayItemModel> childrenModels = taskNode.Value.GetChildrenModels(false);
        if (editor._subtaskGrid == null)
        {
          TaskDetailView taskDetailView = editor;
          StackPanel stackPanel = new StackPanel();
          stackPanel.Margin = inSticky ? new Thickness(-20.0, 0.0, -18.0, 0.0) : new Thickness(-20.0, 2.0, -18.0, 12.0);
          taskDetailView._subtaskGrid = stackPanel;
          Border border1 = new Border();
          border1.Height = 4.0;
          border1.BorderThickness = new Thickness(0.0, 1.0, 0.0, 0.0);
          border1.Margin = inSticky ? new Thickness(22.0, 0.0, 22.0, 4.0) : new Thickness(22.0, 4.0, 22.0, 4.0);
          Border element1 = border1;
          element1.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity10_20");
          editor._subtaskGrid.Children.Add((UIElement) element1);
          editor._subTaskList = new TaskListView();
          editor._subTaskList.ViewModel.BatchEditor = (IBatchEditable) editor;
          editor._subTaskList.NavigateTask += new EventHandler<string>(editor.OnNavigateSubTask);
          editor._subTaskList.NeedReload += new EventHandler(editor.ReloadSubTasks);
          editor._subTaskList.DragOver += new EventHandler<DragMouseEvent>(editor.OnSubtaskDragOver);
          editor._subTaskList.DragDropped += new EventHandler<string>(editor.OnTaskDrop);
          editor._subtaskGrid.Children.Add((UIElement) editor._subTaskList);
          if (editor.Mode != Constants.DetailMode.Sticky)
          {
            Grid grid = new Grid();
            grid.Margin = new Thickness(18.0, 4.0, 0.0, 0.0);
            grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            grid.VerticalAlignment = VerticalAlignment.Bottom;
            Grid element2 = grid;
            element2.SetResourceReference(FrameworkElement.HeightProperty, (object) "Height30");
            Border border2 = new Border();
            border2.Margin = new Thickness(0.0, 3.0, 0.0, 3.0);
            border2.Cursor = System.Windows.Input.Cursors.Hand;
            Border element3 = border2;
            element3.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle");
            element3.MouseLeftButtonUp += new MouseButtonEventHandler(editor.AddSubTaskClick);
            element2.Children.Add((UIElement) element3);
            System.Windows.Shapes.Path path = UiUtils.CreatePath("IcAdd", "PrimaryColor", "Path01");
            path.SetResourceReference(FrameworkElement.HeightProperty, (object) "Height18");
            path.SetResourceReference(FrameworkElement.WidthProperty, (object) "Height18");
            path.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            path.Margin = new Thickness(4.0, -1.0, 0.0, 0.0);
            path.IsHitTestVisible = false;
            TextBlock textBlock = new TextBlock();
            textBlock.Margin = new Thickness(28.0, 0.0, 4.0, 0.0);
            textBlock.IsHitTestVisible = false;
            textBlock.Text = Utils.GetString("AddSubTask");
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            TextBlock element4 = textBlock;
            element4.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body01");
            element4.SetResourceReference(TextBlock.ForegroundProperty, (object) "PrimaryColor");
            element2.Children.Add((UIElement) path);
            element2.Children.Add((UIElement) element4);
            editor._subtaskGrid.Children.Add((UIElement) element2);
          }
          editor._subtaskGrid.SetValue(Grid.RowProperty, (object) 3);
          editor._scrollContent.Children.Add((UIElement) editor._subtaskGrid);
          editor._batchHelper = new BatchTaskEditHelper((IBatchEditable) editor);
          editor._batchHelper.ShowOrHideOperation += new EventHandler<bool>(editor.OnBatchOperation);
        }
        editor._subTaskList.SetIdentity((ProjectIdentity) new ParentTaskIdentity(taskModel.SourceViewModel), TaskListDisplayType.SubTask);
        int taskLevel = TaskDao.GetTaskLevel(taskModel.TaskId, taskModel.ProjectId);
        editor._subTaskList.SetParentLevel(taskLevel);
        editor._subTaskList.ViewModel.SetItems(childrenModels);
        editor._batchHelper.ClearSelectedTaskIds();
      }
      else if (editor._subTaskList != null)
      {
        editor._subTaskList.NavigateTask -= new EventHandler<string>(editor.OnNavigateSubTask);
        editor._subTaskList.NeedReload -= new EventHandler(editor.ReloadSubTasks);
        editor._subTaskList.DragOver -= new EventHandler<DragMouseEvent>(editor.OnSubtaskDragOver);
        editor._subTaskList.DragDropped -= new EventHandler<string>(editor.OnTaskDrop);
        editor._subTaskList = (TaskListView) null;
        editor._scrollContent.Children.Remove((UIElement) editor._subtaskGrid);
        editor._subtaskGrid = (StackPanel) null;
        editor._batchHelper.ShowOrHideOperation -= new EventHandler<bool>(editor.OnBatchOperation);
        editor._batchHelper = (BatchTaskEditHelper) null;
      }
      int num = 0;
      taskNode = (Node<DisplayItemModel>) null;
      return num;
    }

    public async void OnTaskDrop(object sender, string taskId)
    {
      if (!this.CheckEnable() || this._task == null || !(this._task.Kind == "CHECKLIST") || this._checklist == null)
        return;
      UtilLog.Info("DetailDropTask");
      this._popupShowing = true;
      await this._checklist.HandleTaskDrop(taskId);
      TaskChangeNotifier.NotifyDropToItem(taskId);
      this.Reload(loadComment: false);
      this._popupShowing = false;
    }

    private void OnSubtaskDragOver(object sender, DragMouseEvent e)
    {
      EventHandler<DragMouseEvent> subtaskDragOver = this.SubtaskDragOver;
      if (subtaskDragOver != null)
        subtaskDragOver(sender, e);
      if (!this._task.Enable)
        return;
      this._checklist?.HandleTaskMove(e);
    }

    private void ReloadSubTasks(object sender, EventArgs e) => this.SetSubtasks(this._task);

    private void OnNavigateSubTask(object sender, string taskId)
    {
      this.Navigate(taskId);
      EventHandler<string> taskNavigated = this.TaskNavigated;
      if (taskNavigated == null)
        return;
      taskNavigated((object) this, taskId);
    }

    private async void OnBatchOperation(object sender, bool e)
    {
      TaskDetailView child = this;
      if (!e)
      {
        await Task.Delay(50);
        child.SetPopupShowing(false);
        Utils.FindParent<TaskDetailWindow>((DependencyObject) child)?.Activate();
      }
      else
        child.SetPopupShowing(true);
    }

    public async void AddSubTaskClick(object sender, RoutedEventArgs e)
    {
      TaskDetailView taskDetailView = this;
      taskDetailView.AddActionEvent("om", "add_subtask");
      await taskDetailView._addSubTaskAsyncLocker.RunAsync(new Func<Task>(taskDetailView.AddSubTask));
    }

    public async Task AddSubTask()
    {
      TaskDetailView sender = this;
      TaskBaseViewModel newTask;
      if (await ProChecker.CheckTaskLimit(sender._task.ProjectId))
      {
        newTask = (TaskBaseViewModel) null;
      }
      else
      {
        sender._needFocusDetail = false;
        await Task.Delay(10);
        TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
        newTask = new TaskBaseViewModel()
        {
          Id = Utils.GetGuid(),
          ParentId = sender._task.TaskId,
          ProjectId = sender._task.ProjectId,
          IsAllDay = new bool?(true),
          Kind = "TEXT",
          Title = string.Empty,
          Priority = defaultSafely.Priority,
          Status = sender._task.Status,
          Type = DisplayType.Task,
          ColumnId = sender._task.ColumnId
        };
        if (newTask.Status != 0)
        {
          newTask.CompletedTime = new DateTime?(DateTime.Now);
          newTask.CompletedUser = LocalSettings.Settings.LoginUserId;
        }
        if (sender._task.Pinned)
          newTask.PinnedTime = Utils.GetNowTimeStamp();
        newTask.StartDate = newTask.StartDate ?? defaultSafely.GetDefaultDateTime();
        newTask.SortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(newTask.ProjectId, newTask.ParentId, new bool?(false));
        newTask.CreatedTime = new DateTime?(DateTime.Now);
        if (newTask.StartDate.HasValue)
        {
          foreach (TaskReminderModel defaultAllDayReminder in TimeData.GetDefaultAllDayReminders())
          {
            defaultAllDayReminder.taskserverid = newTask.Id;
            int num = await TaskReminderDao.SaveReminders(defaultAllDayReminder);
          }
        }
        TaskCache.AddToDict(newTask);
        DisplayItemModel displayItemModel = new DisplayItemModel(newTask);
        UtilLog.Info("TaskDetail.AddSubTask " + newTask.Id);
        TaskModel taskModel = await TaskService.AddTask(displayItemModel.GetTaskModel());
        await sender.SetSubtasks(sender._task, false);
        sender.GetSubtaskList()?.TryFocusItemById(newTask.Id);
        EventHandler subTaskChanged = sender.SubTaskChanged;
        if (subTaskChanged == null)
        {
          newTask = (TaskBaseViewModel) null;
        }
        else
        {
          subTaskChanged((object) sender, (EventArgs) null);
          newTask = (TaskBaseViewModel) null;
        }
      }
    }

    private async Task SortChildrenBySortOrder(Node<DisplayItemModel> node, bool showLoadMore = true)
    {
      if (node?.Children == null)
        return;
      node.Children.Sort((Comparison<Node<DisplayItemModel>>) ((a, b) => a.Value.SortOrder.CompareTo(b.Value.SortOrder)));
      bool needLoadMore = true;
      if (!LocalSettings.Settings.HideComplete & showLoadMore && node.Value.ChildIds != null)
      {
        int moreCount = 0;
        foreach (string id in node.Value.ChildIds)
        {
          if (!TaskCache.ExistTask(id))
            ++moreCount;
          else if ((await TaskDao.GetThinTaskById(id)).projectId != this._task.ProjectId)
            await TaskDao.AddOrRemoveTaskChildIds(node.Value.Id, new List<string>()
            {
              id
            }, false);
        }
        if (moreCount > 0)
        {
          DisplayItemModel model = new DisplayItemModel(new TaskBaseViewModel()
          {
            Id = node.NodeId + "more",
            ParentId = node.NodeId,
            Type = DisplayType.LoadMore,
            SortOrder = long.MinValue
          })
          {
            MoreCount = moreCount
          };
          List<Node<DisplayItemModel>> children = node.Children;
          DisplayItemNode displayItemNode = new DisplayItemNode(model);
          displayItemNode.Parent = node;
          children.Add((Node<DisplayItemModel>) displayItemNode);
          needLoadMore = false;
        }
      }
      foreach (Node<DisplayItemModel> child in node.Children)
        await this.SortChildrenBySortOrder(child, needLoadMore);
    }

    public void ShowBatchOperationDialog()
    {
      this._batchHelper.ProjectIdentity = (ProjectIdentity) new ParentTaskIdentity(this._task.SourceViewModel);
      this._batchHelper.ShowOperationDialog();
    }

    public void SetSelectedTaskIds(List<string> taskIds)
    {
      this._batchHelper.SelectedTaskIds = taskIds;
    }

    public void RemoveSelectedId(string id) => this._batchHelper.SelectedTaskIds?.Remove(id);

    public List<string> GetSelectedTaskIds() => this._batchHelper.SelectedTaskIds;

    public void ReloadList() => this.SetSubtasks(this._task);

    public DateTime? DisplayDate { get; set; }

    public bool TitleFocused => this._titleText.TextArea.IsKeyboardFocused;

    private void InitDetailTitle()
    {
      this._titleText.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, (object) "BaseColorOpacity100_80");
      this._titleText.SetResourceReference(System.Windows.Controls.Control.FontSizeProperty, (object) "Font16");
      this._titleText.SetValue(Grid.RowProperty, (object) 1);
      this._titleText.FontWeight = FontWeights.SemiBold;
      this._titleText.SetWordWrap(true);
      this._titleText.Navigate += new EventHandler<ProjectTask>(this.OnNavigateTask);
      this._titleText.PopOpened += new EventHandler(this.OnTextPopupOpened);
      this._titleText.PopClosed += new EventHandler(this.OnTextPopupClosed);
      this._titleText.LinkPopOpened += new EventHandler(this.OnLinkPopupOpened);
      this._titleText.LinkPopClosed += new EventHandler(this.OnLinkPopupClosed);
      this._titleText.KeysDown += new EventHandler<System.Windows.Input.KeyEventArgs>(this.OnTitleKeyDown);
      this._titleText.IgnoreTokenChanged += new EventHandler<List<string>>(this.OnTitleIgnoreTokenChanged);
      this._titleText.QuickItemSelected += new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      this._titleText.PreviewTextInput += new TextCompositionEventHandler(this.OnTitleInput);
      this._titleText.LinkTextChange += new EventHandler(this.OnTitleChanged);
      this._titleText.TextGotFocus += new EventHandler<RoutedEventArgs>(this.OnTitleGotFocus);
      this._titleText.TextLostFocus += new EventHandler(this.OnTitleLostFocus);
      this._titleText.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.CheckIfTaskEditable);
      this._titleText.SplitText += new EventHandler<int>(this.OnTitleSplit);
      this._titleText.MoveDown += new EventHandler(this.OnTitleMoveDown);
      this._titleText.SelectDate += new EventHandler(this.OnSelectDate);
      this._titleText.CaretVerticalOffsetChanged += new EventHandler<double>(this.OnContentVerticalOffsetChanged);
      this._titleText.RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
      this._titleText.DateParsed += new EventHandler<IPaserDueDate>(this.OnDateParsed);
      this._scrollContent.Children.Add((UIElement) this._titleText);
      if (this.Mode == Constants.DetailMode.Sticky)
      {
        this._titleText.SetBaseColor("StickyTitleTextColor", "StickyContentTextColor", "StickyTextColor40", "StickyTextColor20");
        this._titleText.SetLightTheme();
        this._titleText.SetResourceReference(System.Windows.Controls.Control.FontSizeProperty, (object) "StickyFont14");
        this._titleText.LineSpacing = 4.0;
      }
      if (this.Mode != Constants.DetailMode.Page)
        return;
      this._titleText.SetupSearchRender(true, true);
    }

    private void SetTitleHint(string title)
    {
      if (string.IsNullOrEmpty(title))
      {
        if (this._titleHint != null)
          return;
        TextBlock textBlock = new TextBlock();
        textBlock.Padding = new Thickness(2.0, 0.0, 0.0, 0.0);
        textBlock.LineHeight = 20.0;
        textBlock.FontWeight = FontWeights.SemiBold;
        textBlock.FontSize = this._titleText.FontSize;
        textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        textBlock.IsHitTestVisible = false;
        this._titleHint = textBlock;
        this._titleHint.SetResourceReference(System.Windows.Controls.Control.FontSizeProperty, this.Mode == Constants.DetailMode.Sticky ? (object) "StickyFont14" : (object) "Font16");
        this._titleHint.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, (object) "BaseColorOpacity40");
        this._titleHint.SetValue(Grid.RowProperty, (object) 1);
        this._titleHint.Text = Utils.GetString(this._task.Kind == "NOTE" ? "NoteTitle" : "RightTitle");
        this._scrollContent.Children.Add((UIElement) this._titleHint);
      }
      else
      {
        if (this._titleHint == null)
          return;
        this._scrollContent.Children.Remove((UIElement) this._titleHint);
        this._titleHint = (TextBlock) null;
      }
    }

    private void LoadTaskTitle(bool restoreOffset)
    {
      TaskDetailViewModel task = this._task;
      if (!restoreOffset || this._titleText.Text != task.Title)
        this._titleText.SetTextOffset(task.Title, restoreOffset);
      if (task.IsNewAdd)
        this._titleText.Focus();
      this._titleText.SetMarkRegexText(false, isNote: task.IsNote);
      this._titleText.ReadOnly = !task.Enable;
      this.SetTitleHint(task.Title);
      this.SetKindSwitcher(this.Mode != Constants.DetailMode.Sticky && this.Mode != Constants.DetailMode.Editor && !task.IsNote);
    }

    private void SetKindSwitcher(bool show)
    {
      if (show)
      {
        if (this._kindSwitcher == null)
        {
          HoverIconButton hoverIconButton = new HoverIconButton();
          hoverIconButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
          hoverIconButton.VerticalAlignment = VerticalAlignment.Top;
          hoverIconButton.RenderTransform = (Transform) new TranslateTransform(0.0, -4.0);
          hoverIconButton.Height = 26.0;
          hoverIconButton.Width = 26.0;
          hoverIconButton.ImageWidth = 16.0;
          this._kindSwitcher = hoverIconButton;
          this._kindSwitcher.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnKindSwitchClick);
          this._kindSwitcher.SetValue(Grid.RowProperty, (object) 1);
          this._scrollContent.Children.Add((UIElement) this._kindSwitcher);
        }
        this._kindSwitcher.SetResourceReference(HoverIconButton.ImageSourceProperty, this._task.Kind != "CHECKLIST" ? (object) "CheckitemsDrawingImage" : (object) "ContentDrawingImage");
        this._kindSwitcher.SetValue(ToolTipService.InitialShowDelayProperty, (object) 500);
        this._kindSwitcher.SetValue(ToolTipService.BetweenShowDelayProperty, (object) 0);
        this._kindSwitcher.ToolTip = (object) Utils.GetString(this._task.Kind != "CHECKLIST" ? "ChecklistTip" : "DescriptionTip");
      }
      else if (this._kindSwitcher != null)
      {
        this._scrollContent.Children.Remove((UIElement) this._kindSwitcher);
        this._kindSwitcher = (HoverIconButton) null;
      }
      this._titleText.Margin = new Thickness(3.0, 0.0, show ? 26.0 : 0.0, 0.0);
    }

    private void OnKindSwitchClick(object sender, MouseButtonEventArgs e)
    {
      if (Utils.IsDida() && !this._task.IsOwner)
      {
        this.TryToast(Utils.GetString("AttendeeModifyContent"));
      }
      else
      {
        if (!this.CheckEnable())
          return;
        this.ChangeCheckListMode();
      }
    }

    private async void ChangeCheckListMode()
    {
      TaskDetailView child = this;
      List<string> tags = TagSerializer.ToTags(child._task.Tag);
      child.LoadTags((IReadOnlyCollection<string>) tags);
      child.AddActionEvent("action", "switch_checklist_mode");
      switch (child._task.Kind)
      {
        case "CHECKLIST":
          child.SwitchToText();
          break;
        case "TEXT":
          child.SwitchToList();
          break;
      }
      Utils.FindParent<IToastShowWindow>((DependencyObject) child)?.TryHideToast();
    }

    private async Task SwitchToText()
    {
      this._kindSwitcher?.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "ContentDrawingImage");
      string content = string.IsNullOrEmpty(this._switchListOriginText) ? this._checklist.ToText(this._descText.Text) : this._switchListOriginText;
      this._switchListOriginText = (string) null;
      this.LoadContentControl(false);
      string str = await AttachmentDao.AddAttachmentStrings(this._task.TaskId, content);
      this._task.SourceViewModel.Kind = "TEXT";
      this._task.SourceViewModel.Desc = string.Empty;
      this._task.SourceViewModel.Content = str;
      this.LoadContentControl(false);
      this.TryShowOptionMenu();
      if (!this._task.IsNewAdd)
      {
        this._savedItems = (this._task.TaskId, await TaskDetailItemDao.DeleteCheckItemsByTaskId(this._task.TaskId));
        await this.SaveTask(CheckMatchedType.CheckSmart);
        TaskChangeNotifier.NotifyTaskKindChanged(new List<string>()
        {
          this._task.TaskId
        });
      }
      this.LoadAttachmentAsync();
    }

    private async void SwitchToList()
    {
      if (this.IsChecklistOverLimit())
        return;
      MarkDownEditor contentText = this._contentText;
      int num1;
      if (contentText == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = contentText.GetImageDict()?.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 != 0)
      {
        this.OnShowDialog();
        bool? nullable = new CustomerDialog(string.Empty, Utils.GetString("SwitchTextTypeWithAttachmentNotify"), Utils.GetString("Continue"), Utils.GetString("Cancel")).ShowDialog();
        this.OnCloseDialog();
        if (!nullable.GetValueOrDefault())
          return;
      }
      (string str, List<CheckItemViewModel> source) = await this.SwitchTextToList();
      this._kindSwitcher?.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "CheckitemsDrawingImage");
      this._task.SourceViewModel.Desc = str;
      this._task.SourceViewModel.Kind = "CHECKLIST";
      this._task.SourceViewModel.Content = string.Empty;
      this.LoadContentControl(false);
      this.SetCheckItems(source);
      this.SetTextEditorMenu(false);
      if (!this._task.IsNewAdd)
      {
        await this.SaveTask(CheckMatchedType.CheckSmart);
        TaskChangeNotifier.NotifyTaskKindChanged(new List<string>()
        {
          this._task.TaskId
        });
      }
      this.LoadAttachmentAsync();
    }

    private bool IsChecklistOverLimit()
    {
      MarkDownEditor contentText = this._contentText;
      string[] strArray;
      if (contentText == null)
        strArray = (string[]) null;
      else
        strArray = contentText.Text.Replace("\r", "").Split('\n');
      long userLimit = Utils.GetUserLimit(Constants.LimitKind.SubtaskNumber);
      int? length = strArray?.Length;
      long? nullable = length.HasValue ? new long?((long) length.GetValueOrDefault()) : new long?();
      long num = userLimit;
      if (!(nullable.GetValueOrDefault() > num & nullable.HasValue))
        return false;
      if (UserDao.IsPro())
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ChecklistLimit"), MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) this);
        customerDialog.ShowDialog();
      }
      else
        ProChecker.ShowUpgradeDialog(ProType.MoreSubTasks);
      return true;
    }

    private async Task<(string, List<CheckItemViewModel>)> SwitchTextToList()
    {
      TaskDetailView taskDetailView = this;
      string origin = taskDetailView._contentText?.Text;
      string content = taskDetailView._contentText?.RemoveAttachment();
      int? length1 = content?.Length;
      int? length2 = origin?.Length;
      if (!(length1.GetValueOrDefault() == length2.GetValueOrDefault() & length1.HasValue == length2.HasValue))
        origin = (string) null;
      ChecklistExtra extra = ChecklistUtils.Text2Items(content);
      List<CheckItemViewModel> itemModels = (List<CheckItemViewModel>) null;
      if (extra.ChecklistItems.Count > 0)
      {
        List<TaskDetailItemModel> taskDetailItemModelList1;
        if (taskDetailView._savedItems.Item1 == taskDetailView._task.TaskId)
        {
          List<TaskDetailItemModel> taskDetailItemModelList2 = taskDetailView._savedItems.Item2;
          // ISSUE: explicit non-virtual call
          if ((taskDetailItemModelList2 != null ? (__nonvirtual (taskDetailItemModelList2.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            taskDetailItemModelList1 = taskDetailView._savedItems.Item2;
            goto label_7;
          }
        }
        taskDetailItemModelList1 = ChecklistUtils.BuildChecklist(taskDetailView._task.Id, taskDetailView._task.TaskId, extra.ChecklistItems);
label_7:
        List<TaskDetailItemModel> models = taskDetailItemModelList1;
        if (!taskDetailView._task.IsNewAdd)
        {
          List<TaskDetailItemModel> taskDetailItemModelList3 = await TaskDetailItemDao.DeleteCheckItemsByTaskId(taskDetailView._task.TaskId);
          await TaskDetailItemDao.BatchInsertChecklists(models);
          taskDetailView._task.SourceViewModel.CheckCheckItems(models);
        }
        else
        {
          itemModels = CheckItemViewModel.BuildListFromModels(models, taskDetailView._task);
          taskDetailView.SetCheckItems(new List<CheckItemViewModel>());
        }
        models = (List<TaskDetailItemModel>) null;
      }
      taskDetailView._switchListOriginText = origin;
      // ISSUE: reference to a compiler-generated method
      itemModels = itemModels ?? new List<CheckItemViewModel>(taskDetailView._task.SourceViewModel.CheckItems.ToList().Select<TaskBaseViewModel, CheckItemViewModel>(new Func<TaskBaseViewModel, CheckItemViewModel>(taskDetailView.\u003CSwitchTextToList\u003Eb__438_0)));
      (string, List<CheckItemViewModel>) list = (extra.Description, itemModels);
      origin = (string) null;
      extra = (ChecklistExtra) null;
      itemModels = (List<CheckItemViewModel>) null;
      return list;
    }

    private void OnNavigateTask(object sender, ProjectTask task)
    {
      EventHandler<ProjectTask> navigateTask = this.NavigateTask;
      if (navigateTask != null)
        navigateTask((object) this, task);
      switch (this.Mode)
      {
      }
    }

    private void SetTextReadOnly(TaskDetailViewModel model)
    {
      this._titleText.ReadOnly = !model.IsOwner || !model.Enable;
    }

    private void OnSelectDate(object sender, EventArgs e)
    {
      if (this.Mode == Constants.DetailMode.Editor || this.Mode == Constants.DetailMode.Sticky)
        return;
      this.SelectDate();
    }

    private void OnTitleMoveDown(object sender, EventArgs e) => this.TryFocusDetail(false);

    private void OnTitleSplit(object sender, int e)
    {
      if (this._contentText != null)
        this._contentText.FocusEditBox();
      else
        this._checklist?.FocusFirstItem();
    }

    private void CheckIfTaskEditable(object sender, MouseButtonEventArgs e)
    {
      if (!Utils.IsDida() || this._task.IsOwner)
        return;
      this.TryToast(Utils.GetString("AttendeeModifyContent"));
    }

    private async void OnTitleLostFocus(object sender, EventArgs e)
    {
      TaskDetailView child = this;
      if (child._titleText.CanParseDate)
      {
        if (child._titleText.SelectionPopupOpened || child._task == null)
          return;
        if (Utils.FindParent<ListViewContainer>((DependencyObject) child) != null)
        {
          ListItemContent parseDateItem = TaskListItemFocusHelper.ParseDateItem;
          if ((parseDateItem != null ? (parseDateItem.TitleFocused ? 1 : 0) : 0) != 0)
            return;
        }
        TimeData timeData = child._titleText.ParsedData?.ToTimeData(true);
        child._titleText.SetCanParseDate(false);
        string parsedText = child._titleText.GetParsedText();
        child.SetDateAndTitle(parsedText, timeData);
        child._task.ParseData = (TimeData) null;
        TaskListItemFocusHelper.ParseDateId = string.Empty;
        TaskListItemFocusHelper.ParseDateItem = (ListItemContent) null;
      }
      else
      {
        if (!(child._task.Title != child._titleText.Text))
          return;
        child._task.SourceViewModel.Title = child._titleText.Text;
        await child.SaveTask();
      }
    }

    private async Task SetDateAndTitle(string text, TimeData timeData = null)
    {
      TaskDetailViewModel task = this._task;
      if (task == null)
      {
        task = (TaskDetailViewModel) null;
      }
      else
      {
        task.SourceViewModel.Title = text;
        task.SourceViewModel.StartDate = task.DisplayStartDate;
        task.SourceViewModel.DueDate = task.DisplayDueDate;
        task.SourceViewModel.IsAllDay = task.IsAllDay;
        task.SourceViewModel.RepeatFlag = task.RepeatFlag;
        task.SourceViewModel.RepeatFrom = task.RepeatFrom;
        await this.SaveTask();
        if (timeData != null)
        {
          await TaskService.SaveReminders(task.TaskId, timeData.Reminders);
          TaskChangeNotifier.NotifyTaskDateChanged(task.TaskId);
        }
        TaskChangeNotifier.NotifyTaskTextChanged(task.TaskId);
        task = (TaskDetailViewModel) null;
      }
    }

    private void OnTitleGotFocus(object sender, RoutedEventArgs e)
    {
      if (this._titleForceParse)
      {
        this._titleText.SetCanParseDate(true);
      }
      else
      {
        TaskDetailWindow window = Window.GetWindow((DependencyObject) this) as TaskDetailWindow;
        if (this._titleText.CanParseDate || !(this._task.Kind != "NOTE") || window != null && window.ShowInCal)
          return;
        bool canParseDate = false;
        TimeData defaultDate = this.GetDefaultDate();
        if (this._task.DisplayStartDate.HasValue)
        {
          DateTime? nullable1 = this._task.DisplayStartDate;
          DateTime? nullable2 = defaultDate.StartDate;
          if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
          {
            nullable2 = this._task.DisplayDueDate;
            nullable1 = defaultDate.DueDate;
            if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() == nullable1.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
            {
              bool? isAllDay1 = this._task.IsAllDay;
              bool? isAllDay2 = defaultDate.IsAllDay;
              if (!(isAllDay1.GetValueOrDefault() == isAllDay2.GetValueOrDefault() & isAllDay1.HasValue == isAllDay2.HasValue))
                goto label_8;
            }
            else
              goto label_8;
          }
          else
            goto label_8;
        }
        canParseDate = true;
label_8:
        this._titleText.SetCanParseDate(canParseDate);
      }
    }

    private TimeData GetDefaultDate()
    {
      TimeData defaultDate = TimeData.BuildDefaultStartAndEnd();
      ListViewContainer parent = Utils.FindParent<ListViewContainer>((DependencyObject) this);
      if (parent != null)
      {
        TimeData timeData1 = parent.GetSelectedProject()?.GetTimeData();
        if (timeData1 == null)
          return defaultDate;
        if (defaultDate.StartDate.HasValue)
        {
          if (defaultDate.DueDate.HasValue)
          {
            DateTime? nullable1 = defaultDate.DueDate;
            DateTime? startDate = defaultDate.StartDate;
            double totalMinutes = (nullable1.HasValue & startDate.HasValue ? new TimeSpan?(nullable1.GetValueOrDefault() - startDate.GetValueOrDefault()) : new TimeSpan?()).Value.TotalMinutes;
            TimeData timeData2 = defaultDate;
            nullable1 = defaultDate.StartDate;
            DateTime? nullable2 = new DateTime?(nullable1.Value.AddMinutes(totalMinutes));
            timeData2.DueDate = nullable2;
          }
          defaultDate.StartDate = timeData1.StartDate;
        }
        else
        {
          defaultDate.StartDate = timeData1.StartDate;
          defaultDate.IsAllDay = timeData1.IsAllDay;
        }
      }
      return defaultDate;
    }

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    private void OnContentVerticalOffsetChanged(object sender, double offset)
    {
      this.KeepOffsetInView(sender, offset);
    }

    private ImmersiveContent GetImmersiveContent()
    {
      if (this.Mode == Constants.DetailMode.Editor && this._immersiveContent == null)
        this._immersiveContent = Utils.FindParent<ImmersiveContent>((DependencyObject) this);
      return this._immersiveContent;
    }

    private void KeepOffsetInView(object sender, double offset)
    {
      if (this.Mode == Constants.DetailMode.Editor)
      {
        this.GetImmersiveContent()?.KeepOffsetInView(sender, offset);
      }
      else
      {
        double num1 = this._titleUpperContent.ActualHeight + 10.0;
        if (object.Equals(sender, (object) this._contentText) || object.Equals(sender, (object) this._descText))
          num1 += this._titleText.ActualHeight;
        else if (object.Equals(sender, (object) this._checklist))
        {
          num1 += this._titleText.ActualHeight + 18.0;
          if (this._descText != null)
            num1 += this._descText.ActualHeight;
        }
        double num2 = (double) this.FindResource((object) "Font14") - 14.0 + 2.0;
        double num3 = offset + num2 + this._scrollContent.Margin.Bottom;
        double num4 = this._detailScrollViewer.VerticalOffset + this._detailScrollViewer.ActualHeight - num1;
        double num5 = this._detailScrollViewer.VerticalOffset - num1;
        if (num4 >= num3 && offset >= num5)
          return;
        if (num3 > num4)
        {
          this._detailScrollViewer.ScrollToVerticalOffset(num3 - this._detailScrollViewer.ActualHeight + num1);
        }
        else
        {
          if (offset < num5)
            this._detailScrollViewer.ScrollToVerticalOffset(offset + num1 - 10.0);
          if (offset > 24.0 + num2)
            return;
          this._detailScrollViewer.ScrollToVerticalOffset(num1 - 24.0 - num2);
        }
      }
    }

    private void OnTitleChanged(object sender, EventArgs e)
    {
      if (!(this._task.Title != this._titleText.Text))
        return;
      this._task.SourceViewModel.Title = this._titleText.Text;
      TaskChangeNotifier.NotifyTaskTextChanged(this._task.TaskId);
      this.TryDelayEditSaveSync();
    }

    private void OnTitleInput(object sender, TextCompositionEventArgs e)
    {
      this.SetTitleHint("text");
    }

    private void OnQuickItemSelected(object sender, QuickSetModel model)
    {
      switch (model.Type)
      {
        case QuickSetType.Priority:
          UtilLog.Info(string.Format("TaskDetail.SetPriority {0}, value{1} from:quickSelect", (object) this._task?.Id, (object) model.Priority));
          this.SetTaskPriority(model.Priority);
          break;
        case QuickSetType.Project:
          UtilLog.Info(string.Format("TaskDetail.SetProject {0}, value{1} from:quickSelect", (object) this._task?.Id, (object) model.Project.id));
          this.QuickSetProject(model.Project);
          break;
        case QuickSetType.Tag:
          UtilLog.Info(string.Format("TaskDetail.SetTag {0}, value{1} from:quickSelect", (object) this._task?.Id, (object) model.Tag));
          this.OnTagAdded(sender, model.Tag.ToLower());
          break;
        case QuickSetType.Date:
          this.TryClearParseDate();
          if (model.Date.HasValue)
            this.OnDateSelected(model.Date.Value);
          UtilLog.Info(string.Format("TaskDetail.SetDate {0}, value{1} from:quickSelect", (object) this._task?.Id, (object) model.Date));
          break;
        case QuickSetType.Assign:
          UtilLog.Info(string.Format("TaskDetail.SetAssignee {0}, value{1} from:quickSelect", (object) this._task?.Id, (object) model.Avatar.UserId));
          this.SetAssignee(model.Avatar.UserId, false);
          break;
      }
    }

    private async void QuickSetProject(ProjectModel project)
    {
      TaskDetailViewModel task = this._task;
      if (project == null || task == null)
        return;
      this.IsBlank = false;
      if (task.IsNewAdd)
      {
        task.SourceViewModel.ProjectId = project.id;
      }
      else
      {
        await Task.Delay(50);
        await this.MoveProject(project);
      }
    }

    public string GetProjectIds() => this._task.ProjectId;

    private void TryClearParseDate()
    {
      if (!this._titleText.CanParseDate)
        return;
      this._titleText.ClearParseDate();
      if (Utils.FindParent<ListViewContainer>((DependencyObject) this) == null)
        return;
      TaskListItemFocusHelper.ParseDateItem?.TitleTextBox.ClearParseDate();
    }

    private void OnTitleIgnoreTokenChanged(object sender, List<string> e)
    {
      if (Utils.FindParent<ListViewContainer>((DependencyObject) this) == null)
        return;
      TaskListItemFocusHelper.ParseDateItem?.SetIgnoreTokens(e);
    }

    private void OnTitleKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key != Key.Tab)
        return;
      if (this._contentText != null)
        this._contentText.FocusEditBox();
      else
        this._descText?.Focus();
      e.Handled = true;
    }

    private void OnTextPopupClosed(object sender, EventArgs e)
    {
      this._popupShowing = false;
      EventHandler<bool> actionPopClosed = this.ActionPopClosed;
      if (actionPopClosed == null)
        return;
      actionPopClosed((object) this, true);
    }

    private void OnTextPopupOpened(object sender, EventArgs e)
    {
      this._popupShowing = true;
      EventHandler actionPopOpened = this.ActionPopOpened;
      if (actionPopOpened == null)
        return;
      actionPopOpened((object) this, (EventArgs) null);
    }

    private void OnChecklistPopupOpened(object sender, bool isWindow)
    {
      if (!isWindow)
      {
        this._popupShowing = true;
        EventHandler actionPopOpened = this.ActionPopOpened;
        if (actionPopOpened == null)
          return;
        actionPopOpened((object) this, (EventArgs) null);
      }
      else
        this.OnShowDialog();
    }

    private void OnChecklistPopupClosed(object sender, bool isWindow)
    {
      if (!isWindow)
      {
        this._popupShowing = true;
        EventHandler actionPopOpened = this.ActionPopOpened;
        if (actionPopOpened == null)
          return;
        actionPopOpened((object) this, (EventArgs) null);
      }
      else
        this.OnCloseDialog();
    }

    private void OnLinkPopupOpened(object sender, EventArgs e) => this.OnShowDialog();

    private void OnLinkPopupClosed(object sender, EventArgs e) => this.OnCloseDialog();

    private void OnDateParsed(object sender, IPaserDueDate e)
    {
      this._task.SetParseData(e?.ToTimeData(true));
    }

    public async void SetTitleParseEnable(bool canParse, bool delay)
    {
      if (delay)
        await Task.Delay(200);
      this._titleForceParse = canParse;
      this._titleText.SetCanParseDate(canParse);
    }

    public async Task<bool> TrySaveParseDate()
    {
      if (!this._titleText.ParsingDate)
        return false;
      TimeData timeData = this._titleText.ParsedData?.ToTimeData(true);
      this._titleText.SetCanParseDate(false);
      await this.SetDateAndTitle(this._titleText.GetParsedText(), timeData);
      this._task.ParseData = (TimeData) null;
      return true;
    }

    public void TryFocusTitle(bool focusEnd = true, string taskId = null)
    {
      if (!string.IsNullOrEmpty(taskId) && taskId != this._task.TaskId)
        return;
      Keyboard.Focus((IInputElement) this._titleText);
      if (focusEnd)
        this._titleText.FocusEnd();
      else
        this._titleText.Focus();
    }

    public void ClearParseDate() => this._titleText.ClearParseDate();

    public DetailTextBox GetTitleText() => this._titleText;
  }
}
