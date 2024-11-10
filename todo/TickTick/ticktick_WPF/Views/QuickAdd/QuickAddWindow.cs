// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.QuickAddWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CheckList;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Time;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class QuickAddWindow : MyWindow, IComponentConnector, IStyleConnector
  {
    public static readonly DependencyProperty TabSelectedIndexProperty = DependencyProperty.Register(nameof (TabSelectedIndex), typeof (int), typeof (QuickAddWindow), new PropertyMetadata((object) 0, (PropertyChangedCallback) null));
    private AvatarViewModel _avatar;
    private List<AvatarViewModel> _avatars = new List<AvatarViewModel>();
    private bool _isDetail;
    private AddTaskViewModel _model;
    private int _oldOptionNum = -2;
    private volatile bool _popOpened;
    private QuickAddView _quickAddView;
    private List<string> _tags;
    private BatchAddTagWindow _tagSelectWindow;
    private bool _canHide = true;
    private static QuickAddWindow _quickAddWindow;
    private static double _top = -1.0;
    private static double _left = -1.0;
    private bool _imeDown;
    private bool _projectMouseDown;
    private bool _addingTask;
    private TTAsyncLocker _addLocker = new TTAsyncLocker(1, 1);
    internal QuickAddWindow Root;
    internal Border WindowBorder;
    internal Grid Container;
    internal Image TickLogo;
    internal Grid TaskTitleGrid;
    internal Grid DetailGrid;
    internal Grid FocusTitleHighlightLine;
    internal ScrollViewer ContentScrollViewer;
    internal Grid TextContentGrid;
    internal TextBlock ContentHint;
    internal MarkDownEditor ContentText;
    internal Grid CheckItemsGrid;
    internal TextBlock DescHint;
    internal MarkDownEditor DescriptionLinkTextBox;
    internal ChecklistControl Checklist;
    internal Grid TagsContainer;
    internal TagDisplayControl TagDisplayControl;
    internal ScrollViewer FileScroller;
    internal StackPanel OperationPanel;
    internal Grid AssignGrid;
    internal Border AvatarImageRectangle;
    internal ImageBrush AvatarImage;
    internal Image AssignOtherGrid;
    internal Popup AssignPopup;
    internal Grid SetDateGrid;
    internal Path SetDatePath;
    internal Grid SetTagGrid;
    internal Popup SetTagPopup;
    internal Grid SetPriorityGrid;
    internal Popup SetPriorityPopup;
    internal StackPanel ProjectPanel;
    internal TextBlock ProjectName;
    internal Popup SetProjectPopup;
    internal Grid OkCancelGrid;
    internal Button SaveButton;
    internal Button CancelButton;
    private bool _contentLoaded;

    public int TabSelectedIndex
    {
      get => (int) this.GetValue(QuickAddWindow.TabSelectedIndexProperty);
      set
      {
        this.SetValue(QuickAddWindow.TabSelectedIndexProperty, (object) value);
        this.CheckButtonStyle();
      }
    }

    private void CheckButtonStyle()
    {
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, this.TabSelectedIndex == 9);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, this.TabSelectedIndex == 10);
      if (this.TabSelectedIndex <= 2)
        return;
      Keyboard.ClearFocus();
      FocusManager.SetFocusedElement((DependencyObject) this, (IInputElement) this);
      Keyboard.Focus((IInputElement) this);
    }

    public bool IsHide { get; set; }

    public QuickAddWindow()
    {
      this.InitializeComponent();
      this.InitQuickAddView();
      this.Activated += new EventHandler(this.OnActivated);
      this.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
      this.PreviewKeyUp += new KeyEventHandler(this.OnPreviewKeyUp);
      this.KeyUp += new KeyEventHandler(this.OnKeyUp);
      this.InitShortCut();
    }

    private void OnActivated(object sender, EventArgs e)
    {
      if (Math.Abs(QuickAddWindow._left - -1.0) <= 0.1)
        return;
      Matrix? transformFromDevice = PresentationSource.FromVisual((Visual) this)?.CompositionTarget?.TransformFromDevice;
      System.Windows.Point defaultPoint = new System.Windows.Point(SystemParameters.PrimaryScreenWidth / 2.0 - 380.0, SystemParameters.PrimaryScreenHeight / 2.0 - 100.0);
      System.Windows.Point pomoLocationSafely = WidgetLocationHelper.GetPomoLocationSafely(QuickAddWindow._left, QuickAddWindow._top, 760.0, 100.0, transformFromDevice, defaultPoint);
      this.Left = pomoLocationSafely.X;
      this.Top = pomoLocationSafely.Y;
    }

    private async void InitQuickAddView()
    {
      QuickAddWindow quickAddWindow = this;
      NormalProjectIdentity defaultProject = ProjectIdentity.GetDefaultProject();
      quickAddWindow._quickAddView = new QuickAddView((IProjectTaskDefault) defaultProject, QuickAddView.Scenario.QuickAdd)
      {
        ShowDetail = new bool?(false)
      };
      quickAddWindow._quickAddView.Name = "QuickAddTitle";
      quickAddWindow._quickAddView.InputPopupOpened += new EventHandler(quickAddWindow.PopupOpened);
      quickAddWindow._quickAddView.InputPopupClosed += new EventHandler(quickAddWindow.PopupClosed);
      quickAddWindow._quickAddView.BatchTaskAdded += new EventHandler<List<TaskModel>>(quickAddWindow.OnBatchTaskAdded);
      quickAddWindow._quickAddView.GotFocus += new RoutedEventHandler(quickAddWindow.OnInputGotFocus);
      quickAddWindow._quickAddView.TitleText.EditBox.GotFocus -= new RoutedEventHandler(quickAddWindow.OnTitleGotFocus);
      quickAddWindow._quickAddView.TitleText.EditBox.GotFocus += new RoutedEventHandler(quickAddWindow.OnTitleGotFocus);
      quickAddWindow._quickAddView.TitleText.EditBox.LostFocus -= new RoutedEventHandler(quickAddWindow.OnTitleLostFocus);
      quickAddWindow._quickAddView.TitleText.EditBox.LostFocus += new RoutedEventHandler(quickAddWindow.OnTitleLostFocus);
      quickAddWindow._quickAddView.TitleText.EditBox.KeyUp += new KeyEventHandler(quickAddWindow.OnTitleKeyUp);
      quickAddWindow.TaskTitleGrid.Children.Add((UIElement) quickAddWindow._quickAddView);
      quickAddWindow._model = quickAddWindow._quickAddView.Model;
      quickAddWindow._tags = quickAddWindow._quickAddView.GetSelectedTags();
      quickAddWindow.DataContext = (object) quickAddWindow._model;
      // ISSUE: reference to a compiler-generated method
      quickAddWindow._quickAddView.TaskAdded += new EventHandler<TaskModel>(quickAddWindow.\u003CInitQuickAddView\u003Eb__28_0);
      quickAddWindow.InitContentEvents();
    }

    private void OnTitleLostFocus(object sender, RoutedEventArgs e)
    {
      this.FocusTitleHighlightLine.Visibility = Visibility.Collapsed;
    }

    private void OnTitleGotFocus(object sender, RoutedEventArgs e)
    {
      this.FocusTitleHighlightLine.Visibility = Visibility.Visible;
    }

    private void InitContentEvents()
    {
      this.ContentText.TextChanged -= new EventHandler(this.OnContentTextChanged);
      this.ContentText.TextChanged += new EventHandler(this.OnContentTextChanged);
      this.ContentText.RegisterInputHandler();
      this.ContentText.SaveContent -= new EventHandler(this.OnAddTask);
      this.ContentText.SaveContent += new EventHandler(this.OnAddTask);
      this.ContentText.QuickItemSelected -= new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      this.ContentText.QuickItemSelected += new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      this.ContentText.KeyUp -= new EventHandler<KeyEventArgs>(this.OnContentKeyUp);
      this.ContentText.KeyUp += new EventHandler<KeyEventArgs>(this.OnContentKeyUp);
      this.ContentText.MoveUp -= new EventHandler(this.OnContentMoveUp);
      this.ContentText.MoveUp += new EventHandler(this.OnContentMoveUp);
    }

    private void OnContentMoveUp(object sender, EventArgs e) => this._quickAddView.FocusEnd();

    private void OnContentKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      this.HideWindow();
    }

    private void OnAddTask(object sender, EventArgs e) => this.SaveTaskInDetailMode();

    private void OnContentTextChanged(object sender, EventArgs e)
    {
      this.OnTaskContentChanged(this.ContentText.EditBox.Text);
    }

    protected override async void OnContentRendered(EventArgs e)
    {
      QuickAddWindow quickAddWindow = this;
      // ISSUE: reference to a compiler-generated method
      quickAddWindow.\u003C\u003En__0(e);
      quickAddWindow.Activate();
      await Task.Delay(100);
      quickAddWindow._quickAddView?.FocusText();
    }

    private void OnBatchTaskAdded(object sender, List<TaskModel> e) => this.HideWindow();

    private void HandleAfterTaskAdded(string projectId)
    {
      SyncManager.TryDelaySync();
      this.HideWindow();
    }

    public void HideWindow(bool close = true)
    {
      try
      {
        if (!this._popOpened && !this._quickAddView.IsInOperation)
        {
          if (!close)
            return;
          QuickAddWindow._quickAddWindow = (QuickAddWindow) null;
          this.Close();
        }
        else
          this.TryHidePopup();
      }
      catch (Exception ex)
      {
      }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
          this.SwitchViewCommand();
          e.Handled = true;
          break;
        case Key.Up:
        case Key.Down:
          this.HandleUpDown(e.Key == Key.Up);
          break;
        case Key.ImeProcessed:
          this._imeDown = true;
          break;
      }
    }

    private void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        if (this.TabSelectedIndex > 2)
        {
          this.FocusDetail();
          return;
        }
        if (!this._imeDown)
          this.HideWindow();
      }
      this._imeDown = false;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (Utils.IfCtrlPressed())
          {
            this.SaveTaskInDetailMode();
            break;
          }
          this.HandleEnter();
          break;
        case Key.Left:
        case Key.Right:
          if (!this.SetProjectPopup.IsOpen || !(this.SetProjectPopup.Child is ProjectOrGroupPopup child))
            break;
          child.LeftRightSelect(e.Key == Key.Left);
          break;
      }
    }

    private void HandleUpDown(bool isUp)
    {
      switch (this.TabSelectedIndex)
      {
        case 4:
          if (!(this.AssignPopup.Child is SetAssigneeDialog child1))
            break;
          child1.Move(isUp);
          break;
        case 7:
          if (!this.SetPriorityPopup.IsOpen || !(this.SetPriorityPopup.Child is SetPriorityDialog child2))
            break;
          child2.MoveHover(isUp);
          break;
        case 8:
          if (!this.SetProjectPopup.IsOpen || !(this.SetProjectPopup.Child is ProjectOrGroupPopup child3))
            break;
          child3.UpDownSelect(isUp);
          break;
      }
    }

    private async void HandleEnter()
    {
      switch (this.TabSelectedIndex)
      {
        case 3:
          this.SwitchToTextOrList();
          break;
        case 4:
          this.ShowSetAssigneeDialog();
          break;
        case 5:
          this.ShowSetDateDialog(true);
          break;
        case 6:
          this.ShowSetTagDialog();
          break;
        case 7:
          if (this.SetPriorityPopup.IsOpen)
          {
            if (!(this.SetPriorityPopup.Child is SetPriorityDialog child))
              break;
            child.EnterSelect();
            break;
          }
          this.ShowSetPriorityDialog();
          break;
        case 8:
          if (this.SetProjectPopup.IsOpen)
          {
            if (!(this.SetProjectPopup.Child is ProjectOrGroupPopup child))
              break;
            child.EnterSelect();
            break;
          }
          this.ShowSetProjectDialog();
          break;
        case 9:
          await this.TryAddTask();
          break;
        case 10:
          this.HideWindow();
          break;
      }
    }

    public async void SwitchViewCommand()
    {
      QuickAddWindow quickAddWindow = this;
      QuickAddView quickAddView = quickAddWindow._quickAddView;
      if ((quickAddView != null ? (quickAddView.ShowDetail.HasValue ? 1 : 0) : 0) != 0 && !quickAddWindow._quickAddView.ShowDetail.Value)
      {
        quickAddWindow._quickAddView.SwitchView();
        quickAddWindow.SwitchView();
        quickAddWindow.Container.Margin = new Thickness(0.0);
        ((Storyboard) quickAddWindow.FindResource((object) "ShowDetailStory")).Begin();
        quickAddWindow._quickAddView.TagsChanged += new EventHandler(quickAddWindow.OnTitleTagsChanged);
        quickAddWindow.ContentText.AllowTab = false;
      }
      else
        quickAddWindow.TryTabSelect(Utils.IfShiftPressed());
    }

    private void TryTabSelect(bool inShift = false)
    {
      if (this._popOpened || this._quickAddView.IsInOperation)
        return;
      this.TabSelectedIndex += 11 + (inShift ? -1 : 1);
      this.TabSelectedIndex %= 11;
      switch (this.TabSelectedIndex)
      {
        case 0:
          this._quickAddView.FocusText();
          break;
        case 1:
          if (this.ContentText.IsVisible)
          {
            this.ContentText.EditBox.Focus();
            break;
          }
          this.DescriptionLinkTextBox.EditBox.Focus();
          break;
        case 2:
          if (this.Checklist.IsVisible)
          {
            this.Checklist.FocusFirstItem();
            break;
          }
          this.TryTabSelect(inShift);
          break;
        case 4:
          if (this.AssignGrid.IsVisible)
            break;
          this.TryTabSelect(inShift);
          break;
      }
    }

    private void EditorOnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    private void OnTitleTagsChanged(object sender, EventArgs e) => this.SetTagPanel();

    private async void SwitchView()
    {
      QuickAddWindow quickAddWindow = this;
      quickAddWindow._isDetail = true;
      quickAddWindow._tags = quickAddWindow._quickAddView.GetSelectedTags();
      quickAddWindow.TickLogo.Visibility = Visibility.Collapsed;
      quickAddWindow.DetailGrid.Visibility = Visibility.Visible;
      quickAddWindow.SetTagPanel();
      quickAddWindow.InitTagEvent();
      quickAddWindow._avatars = quickAddWindow._quickAddView.GetProjectAvatars();
      quickAddWindow._avatar = quickAddWindow._quickAddView.GetSelectedAvatar();
      quickAddWindow.SetAvatarImage();
      quickAddWindow._quickAddView.ClearTag();
      quickAddWindow._quickAddView.NotifyAvatarsChanged += new EventHandler<List<AvatarViewModel>>(quickAddWindow.OnAvatarsChanged);
      await Task.Delay(10);
      if (quickAddWindow._quickAddView.IsContentEmpty())
        return;
      quickAddWindow.ContentText.EditBox.Focus();
      quickAddWindow.ContentText.EditBox.CaretOffset = 0;
    }

    private void OnAvatarsChanged(object sender, List<AvatarViewModel> avatars)
    {
      this._avatars = avatars;
    }

    private async Task SetAvatarImage()
    {
      if (this._avatar == null)
      {
        this.AvatarImageRectangle.Visibility = Visibility.Collapsed;
        this.AssignOtherGrid.Visibility = Visibility.Visible;
      }
      else
      {
        this.AvatarImageRectangle.Visibility = Visibility.Visible;
        this.AssignOtherGrid.Visibility = Visibility.Collapsed;
        ImageBrush imageBrush = this.AvatarImage;
        imageBrush.ImageSource = (ImageSource) await AvatarHelper.GetAvatarByUrlAsync(this._avatar.AvatarUrl);
        imageBrush = (ImageBrush) null;
      }
    }

    protected override async void OnDeactivated(EventArgs e)
    {
      QuickAddWindow quickAddWindow = this;
      // ISSUE: reference to a compiler-generated method
      quickAddWindow.\u003C\u003En__1(e);
      await Task.Delay(10);
      if (quickAddWindow._popOpened || !quickAddWindow._canHide || quickAddWindow._quickAddView.IsInOperation)
        return;
      quickAddWindow.Hide();
      quickAddWindow.IsHide = true;
    }

    private void OnTitleKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Down || !this._isDetail || this._quickAddView.IsInOperation)
        return;
      this.FocusDetail();
    }

    private void FocusDetail()
    {
      if (this.TextContentGrid.Visibility == Visibility.Visible)
        this.ContentText.EditBox.Focus();
      else
        this.DescriptionLinkTextBox.Focus();
    }

    public async Task AddTask(string text)
    {
      if (this._isDetail)
      {
        if (this._quickAddView.IsInOperation)
          return;
        this.FocusDetail();
      }
      else
      {
        await this._quickAddView.DoAddTask(text);
        this.HideWindow();
      }
    }

    public async Task SaveTaskInDetailMode() => await this.TryAddTask();

    private void OnContentHintClick(object sender, MouseButtonEventArgs e)
    {
      if (!this.TextContentGrid.IsVisible)
        return;
      this.ContentText.Focus();
    }

    private void OnInputGotFocus(object sender, RoutedEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      switch (frameworkElement.Name)
      {
        case "QuickAddTitle":
          this.TabSelectedIndex = 0;
          break;
        case "ContentText":
          this.TabSelectedIndex = 1;
          break;
        case "DescriptionLinkTextBox":
          this.TabSelectedIndex = 1;
          break;
        case "Checklist":
          this.TabSelectedIndex = 2;
          break;
      }
    }

    private void OnTaskContentChanged(string text)
    {
      this.ContentHint.Visibility = string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e) => await this.TryAddTask();

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.HideWindow();

    private void OnDescriptionChanged(object sender, EventArgs eventArgs)
    {
      this.DescHint.Visibility = string.IsNullOrEmpty(this.DescriptionLinkTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnTextOrChecklistClick(object sender, MouseButtonEventArgs e)
    {
      this.SwitchToTextOrList();
    }

    private void SwitchToTextOrList()
    {
      this._model.Kind = this._model.Kind == "TEXT" ? "CHECKLIST" : "TEXT";
      switch (this._model.Kind)
      {
        case "TEXT":
          this.SwitchToText();
          break;
        case "CHECKLIST":
          this.SwitchToList();
          break;
      }
    }

    private void SwitchToText()
    {
      this.ContentText.Text = string.Empty;
      this.TextContentGrid.Visibility = Visibility.Visible;
      this.CheckItemsGrid.Visibility = Visibility.Collapsed;
      string text = this.Checklist.ToText(this.DescriptionLinkTextBox.Text);
      this.ContentText.Text = text;
      this.ContentHint.Visibility = string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
      this.ContentText.EditBox.Focus();
    }

    private async void SwitchToList()
    {
      this.TextContentGrid.Visibility = Visibility.Collapsed;
      this.CheckItemsGrid.Visibility = Visibility.Visible;
      this.DescriptionLinkTextBox.Visibility = Visibility.Visible;
      this.Checklist.SetData(new List<CheckItemViewModel>());
      if (!string.IsNullOrEmpty(this.ContentText.Text))
      {
        ChecklistExtra checklistExtra = ChecklistUtils.Text2Items(this.ContentText.Text);
        this.ContentText.Text = string.Empty;
        this.DescriptionLinkTextBox.SetText(checklistExtra.Description);
        if (!string.IsNullOrEmpty(checklistExtra.Description))
          this.DescHint.Visibility = Visibility.Collapsed;
        if (checklistExtra.ChecklistItems.Count > 0)
          this.InitChecklistView(new List<CheckItemViewModel>((IEnumerable<CheckItemViewModel>) CheckItemViewModel.BuildListFromModels(ChecklistUtils.BuildChecklist(0, (string) null, checklistExtra.ChecklistItems), (TaskDetailViewModel) null)));
      }
      else
        this.InitChecklistView(new List<CheckItemViewModel>((IEnumerable<CheckItemViewModel>) CheckItemViewModel.BuildListFromModels(new List<TaskDetailItemModel>()
        {
          new TaskDetailItemModel()
          {
            id = Utils.GetGuid(),
            TaskId = 0,
            TaskServerId = (string) null,
            title = string.Empty,
            status = 0,
            sortOrder = 0L
          }
        }, (TaskDetailViewModel) null)));
      this.Checklist.FocusFirstItem();
    }

    private void InitChecklistView(List<CheckItemViewModel> source)
    {
      if (source != null && source.Count == 1)
        source[0].ShowAddHint = true;
      this.Checklist.SetData(source);
      this.Checklist.QuickItemSelected -= new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      this.Checklist.QuickItemSelected += new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      this.Checklist.QuickAddTask -= new EventHandler(this.OnQuickAddTask);
      this.Checklist.QuickAddTask += new EventHandler(this.OnQuickAddTask);
      this.Checklist.PopOpened -= new EventHandler<bool>(this.CheckListPopupOpened);
      this.Checklist.PopOpened += new EventHandler<bool>(this.CheckListPopupOpened);
      this.Checklist.PopClosed -= new EventHandler<bool>(this.CheckListPopupClosed);
      this.Checklist.PopClosed += new EventHandler<bool>(this.CheckListPopupClosed);
      this.Checklist.DatePopOpened -= new EventHandler(this.DatePopupOpened);
      this.Checklist.DatePopOpened += new EventHandler(this.DatePopupOpened);
      this.Checklist.DatePopClosed -= new EventHandler(this.DatePopupClosed);
      this.Checklist.DatePopClosed += new EventHandler(this.DatePopupClosed);
    }

    private void OnQuickAddTask(object sender, EventArgs e) => this.SaveTaskInDetailMode();

    private void OnQuickItemSelected(object sender, QuickSetModel e)
    {
      switch (e.Type)
      {
        case QuickSetType.Priority:
          this._model.Priority = e.Priority;
          break;
        case QuickSetType.Project:
          this.OnProjectSelect(e.Project);
          break;
        case QuickSetType.Tag:
          this.OnTagAdded(e.Tag);
          break;
        case QuickSetType.Date:
          TimeData timeData = TimeData.InitDefaultTime();
          timeData.StartDate = e.Date;
          this._model.TimeData = timeData;
          break;
      }
    }

    public void OnTagAdded(string tag)
    {
      if (tag.StartsWith("#") || tag.StartsWith("＃"))
        tag = tag.Substring(1);
      if (!this._tags.Contains(tag))
        this._tags.Add(tag);
      this.SetTagPanel();
    }

    private void SetTagPanel()
    {
      if (this._tags.Count > 0)
      {
        this.TagsContainer.Visibility = Visibility.Visible;
        this.TagDisplayControl.LoadTagData((IReadOnlyCollection<string>) this._tags);
        this.TagDisplayControl.OriginalTags = this._tags;
      }
      else
        this.TagsContainer.Visibility = Visibility.Collapsed;
    }

    private void OnSetAssigneeClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowSetAssigneeDialog();
    }

    private void ShowSetAssigneeDialog()
    {
      if (this.AssignPopup.IsOpen)
      {
        if (!(this.AssignPopup.Child is SetAssigneeDialog child))
          return;
        child.EnterSelect();
      }
      else
      {
        if (!Utils.IsNetworkAvailable())
          return;
        SetAssigneeDialog setAssigneeDialog = new SetAssigneeDialog(this._model.ProjectId, this.AssignPopup, this._quickAddView?.Avatar?.UserId);
        setAssigneeDialog.AssigneeSelect += new EventHandler<AvatarInfo>(this.OnAssigneeSelect);
        setAssigneeDialog.Show();
        setAssigneeDialog.Move(false);
      }
    }

    private void OnAssigneeSelect(object sender, AvatarInfo selected)
    {
      this._avatar = this._avatars.FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (a => a.UserId == selected.UserId));
      this._quickAddView.SetAvatar(this._avatar);
      this.SetAvatarImage();
    }

    private async Task TryAddTask()
    {
      QuickAddWindow quickAddWindow = this;
      // ISSUE: reference to a compiler-generated method
      await quickAddWindow._addLocker.RunAsync(new Func<Task>(quickAddWindow.\u003CTryAddTask\u003Eb__75_0));
    }

    private void PopupOpened(object sender, EventArgs e) => this._popOpened = true;

    private void PopupClosed(object sender, EventArgs e) => this._popOpened = false;

    private void CheckListPopupOpened(object sender, bool isWindow) => this._popOpened = true;

    private void CheckListPopupClosed(object sender, bool isWindow) => this._popOpened = false;

    private void DatePopupOpened(object sender, EventArgs e) => this._popOpened = true;

    private void DatePopupClosed(object sender, EventArgs e) => this._popOpened = false;

    private void TryHidePopup()
    {
      this.SetProjectPopup.IsOpen = false;
      this.SetPriorityPopup.IsOpen = false;
      this.AssignPopup.IsOpen = false;
    }

    private void SetDateClick(object sender, MouseButtonEventArgs e) => this.ShowSetDateDialog();

    private void ShowSetDateDialog(bool withTab = false)
    {
      if (this._model.TimeData == null)
        this._model.TimeData = new TimeData()
        {
          IsDefault = false
        };
      SetDateDialog dialog = SetDateDialog.GetDialog(withTab);
      dialog.ClearEventHandle();
      dialog.Save += (EventHandler<TimeData>) ((obj, data) =>
      {
        TaskService.TryFixRepeatFlag(ref data);
        this._model.TimeData = TimeData.Clone(data);
        this._model.TimeData.IsDefault = false;
        this._quickAddView.ManualSelectedDate = true;
        if (!data.DueDate.HasValue)
          DateUtils.CheckIfTomorrowWronglySet(data);
        if (!this._model.IsCalendar || !this._model.TimeData.StartDate.HasValue)
          return;
        TimeData timeData1 = this._model.TimeData;
        bool? isAllDay;
        int num;
        if (timeData1 == null)
        {
          num = 0;
        }
        else
        {
          isAllDay = timeData1.IsAllDay;
          num = isAllDay.HasValue ? 1 : 0;
        }
        if (num == 0)
          return;
        isAllDay = this._model.TimeData.IsAllDay;
        if (!isAllDay.Value)
          return;
        TimeData timeData2 = this._model.TimeData;
        DateTime? dueDate = this._model.TimeData.DueDate;
        ref DateTime? local = ref dueDate;
        DateTime? nullable = new DateTime?(local.HasValue ? local.GetValueOrDefault().AddDays(1.0) : this._model.TimeData.StartDate.Value.AddDays(1.0));
        timeData2.DueDate = nullable;
      });
      dialog.Clear += (EventHandler) ((obj, arg) =>
      {
        if (!this._model.IsCalendar)
          this._model.TimeData = (TimeData) null;
        this._quickAddView.ManualSelectedDate = true;
      });
      dialog.Hided += new EventHandler(this.PopupClosed);
      dialog.Show(this._model.TimeData, new SetDateDialogArgs(target: (UIElement) this.SetDateGrid, hOffset: -20.0, vOffset: 25.0, canSkip: false));
      this._popOpened = true;
    }

    private void SetTagClick(object sender, MouseButtonEventArgs e) => this.ShowSetTagDialog();

    private void ShowSetTagDialog()
    {
      BatchSetTagControl batchSetTagControl = new BatchSetTagControl();
      batchSetTagControl.Close += (EventHandler) ((s, e) => this.SetTagPopup.IsOpen = false);
      batchSetTagControl.TagsSelect += (EventHandler<TagSelectData>) ((s, e) =>
      {
        List<string> omniSelectTags = e.OmniSelectTags;
        this._tags.Clear();
        this._tags.AddRange((IEnumerable<string>) omniSelectTags);
        this.SetTagPanel();
        this.SetTagPopup.IsOpen = false;
      });
      this.SetTagPopup.Child = (UIElement) batchSetTagControl;
      batchSetTagControl.Init(new TagSelectData()
      {
        OmniSelectTags = new List<string>((IEnumerable<string>) this._tags)
      }, true);
      this.SetTagPopup.IsOpen = true;
    }

    private void SetPriorityClick(object sender, MouseButtonEventArgs e)
    {
      if (this.SetPriorityPopup.IsOpen)
        this.SetPriorityPopup.IsOpen = false;
      else
        this.ShowSetPriorityDialog();
    }

    private void ShowSetPriorityDialog()
    {
      SetPriorityDialog setPriorityDialog = new SetPriorityDialog(this.SetPriorityPopup, this._model.Priority);
      setPriorityDialog.PrioritySelect += new EventHandler<int>(this.PrioritySelect);
      setPriorityDialog.Show();
    }

    private void PrioritySelect(object sender, int priority)
    {
      this._model.Priority = priority;
      this.SetPriorityPopup.IsOpen = false;
    }

    private void SetProjectClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._projectMouseDown)
        return;
      this._projectMouseDown = false;
      this.ShowSetProjectDialog();
    }

    private async void ShowSetProjectDialog()
    {
      QuickAddWindow quickAddWindow = this;
      Popup setProjectPopup = quickAddWindow.SetProjectPopup;
      ProjectExtra data = new ProjectExtra();
      List<string> stringList;
      if (!quickAddWindow._model.SelectProject)
      {
        stringList = new List<string>();
      }
      else
      {
        stringList = new List<string>();
        stringList.Add(quickAddWindow._model.ProjectId);
      }
      data.ProjectIds = stringList;
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup(setProjectPopup, data, new ProjectSelectorExtra()
      {
        showAll = false,
        batchMode = false,
        canSelectGroup = false,
        CanSearch = true,
        onlyShowPermission = true,
        ShowColumn = true,
        ColumnId = quickAddWindow._model.GetProjectColumnId()
      });
      projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(quickAddWindow.OnProjectSelect);
      projectOrGroupPopup.Show();
      await Task.Delay(50);
      HwndHelper.SetFocus(quickAddWindow.SetProjectPopup, false);
    }

    private void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      (string str1, string str2) = e.GetProjectAndColumnId();
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == str1));
      if (project == null)
        return;
      this.OnProjectSelect(project);
      this._model.AddToColumnId = str2;
    }

    private async void OnProjectSelect(ProjectModel project)
    {
      this.SetProjectPopup.IsOpen = false;
      if (project.id != this._model.ProjectId)
      {
        this._model.ProjectId = project.id;
        this._model.ProjectName = project.name ?? Utils.GetString("Inbox");
        this._model.AddToColumnId = (string) null;
      }
      this.LoadAvatars();
    }

    private async Task LoadAvatars()
    {
      QuickAddWindow quickAddWindow = this;
      if (quickAddWindow._model != null)
      {
        // ISSUE: reference to a compiler-generated method
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(quickAddWindow.\u003CLoadAvatars\u003Eb__94_0));
        if (projectModel != null && projectModel.IsShareList())
        {
          List<AvatarViewModel> projectAvatars1 = await AvatarHelper.GetProjectAvatars(quickAddWindow._model.ProjectId);
          quickAddWindow._avatars = projectAvatars1;
          if (!quickAddWindow._avatars.Any<AvatarViewModel>())
          {
            List<AvatarViewModel> projectAvatars2 = await AvatarHelper.GetProjectAvatars(quickAddWindow._model.ProjectId, fetchRemote: true);
            quickAddWindow._avatars = projectAvatars2;
          }
        }
        else
          quickAddWindow._avatars = new List<AvatarViewModel>();
      }
      else
        quickAddWindow._avatars = new List<AvatarViewModel>();
      quickAddWindow._quickAddView.SetAvatars(quickAddWindow._avatars);
    }

    private void InitTagEvent()
    {
      this.TagDisplayControl.TagsChanged -= new EventHandler<List<string>>(this.OnTagControlChanged);
      this.TagDisplayControl.TagsChanged += new EventHandler<List<string>>(this.OnTagControlChanged);
    }

    private void OnTagControlChanged(object sender, List<string> tags) => this.SetTagPanel();

    private void OnMouseScroll(object sender, MouseWheelEventArgs e)
    {
      if (e == null)
        return;
      this.ContentScrollViewer.ScrollToVerticalOffset(this.ContentScrollViewer.VerticalOffset - (double) e.Delta / 2.0);
    }

    public void TryAddTag(string tag)
    {
      if (this._tags.Contains(tag))
        return;
      this._tags.Add(tag);
      this.SetTagPanel();
    }

    private void OnListCaretMoveUp(object sender, EventArgs e)
    {
      this.DescriptionLinkTextBox.FocusEnd();
    }

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    private void OnDescMoveDown(object sender, EventArgs e) => this.Checklist.FocusFirstItem();

    private void OnDescMoveUp(object sender, EventArgs e) => this._quickAddView.FocusEnd();

    private void OnSelectDate(object sender, EventArgs e)
    {
      this.SetDateClick(sender, (MouseButtonEventArgs) null);
    }

    private async void DelayActive()
    {
      QuickAddWindow quickAddWindow = this;
      quickAddWindow._canHide = false;
      await Task.Delay(200);
      quickAddWindow._canHide = true;
      quickAddWindow.Activate();
    }

    private void TryDragWindow(object sender, MouseEventArgs e)
    {
      if (this.TaskTitleGrid.IsMouseOver || this.DetailGrid.IsMouseOver || e.LeftButton != MouseButtonState.Pressed)
        return;
      double top = this.Top;
      double left = this.Left;
      this.DragMove();
      if (Math.Abs(this.Top - top) <= 4.0 && Math.Abs(this.Left - left) <= 4.0)
        return;
      QuickAddWindow._top = this.Top;
      QuickAddWindow._left = this.Left;
    }

    public static void ShowQuickAddWindow()
    {
      if (QuickAddWindow._quickAddWindow != null)
      {
        QuickAddWindow._quickAddWindow.Show();
      }
      else
      {
        QuickAddWindow quickAddWindow = new QuickAddWindow();
        quickAddWindow.Height = 300.0;
        QuickAddWindow._quickAddWindow = quickAddWindow;
        QuickAddWindow._quickAddWindow.Show();
      }
      QuickAddWindow._quickAddWindow.DelayActive();
    }

    public static void ShowOrHideQuickAddWindow()
    {
      if (QuickAddWindow._quickAddWindow != null)
      {
        if (QuickAddWindow._quickAddWindow.IsHide)
        {
          try
          {
            QuickAddWindow._quickAddWindow.Show();
            QuickAddWindow._quickAddWindow.Activate();
            QuickAddWindow._quickAddWindow.IsHide = false;
            QuickAddWindow._quickAddWindow.InitShortCut();
          }
          catch (Exception ex)
          {
            QuickAddWindow quickAddWindow = new QuickAddWindow();
            quickAddWindow.Height = 300.0;
            QuickAddWindow._quickAddWindow = quickAddWindow;
            QuickAddWindow._quickAddWindow.Show();
          }
        }
        else
        {
          QuickAddWindow._quickAddWindow.Hide();
          QuickAddWindow._quickAddWindow.IsHide = true;
        }
      }
      else
      {
        QuickAddWindow quickAddWindow = new QuickAddWindow();
        quickAddWindow.Height = 300.0;
        QuickAddWindow._quickAddWindow = quickAddWindow;
        QuickAddWindow._quickAddWindow.Show();
      }
    }

    private void OnProjectMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._projectMouseDown = true;
    }

    private void InitShortCut()
    {
      if (this.InputBindings.Count < 9)
        return;
      KeyBindingManager.SetKeyGesture("ClearDate", this.InputBindings[0] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetToday", this.InputBindings[1] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetTomorrow", this.InputBindings[2] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetNextWeek", this.InputBindings[3] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetDate", this.InputBindings[4] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetNoPriority", this.InputBindings[5] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetLowPriority", this.InputBindings[6] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetMediumPriority", this.InputBindings[7] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetHighPriority", this.InputBindings[8] as KeyBinding);
    }

    public void ClearDate() => this._quickAddView.ClearDate();

    public void SelectDate() => this.SetDateClick((object) null, (MouseButtonEventArgs) null);

    public void SetDate(string date) => this._quickAddView.SetDate(date);

    public void SetPriority(int pri) => this._quickAddView.SetPriority(pri);

    private void DeleteFile(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is AddTaskAttachmentInfo dataContext))
        return;
      this._model.Remove(dataContext);
    }

    private void OnFileScrollerMouseWheel(object sender, MouseWheelEventArgs e)
    {
      this.FileScroller.ScrollToHorizontalOffset(this.FileScroller.HorizontalOffset - (e.Delta > 0 ? 56.0 : -56.0));
    }

    private void OnFileDrop(object sender, DragEventArgs e)
    {
      this._quickAddView.OnFileDrop(sender, e);
    }

    private void OnDetailKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.V || !Utils.IfCtrlPressed())
        return;
      this._quickAddView.TryPasteAttachment(e);
    }

    private void OnAttachmentClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is AddTaskAttachmentInfo dataContext))
        return;
      dataContext.OnAttachmentClick();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/quickadd/quickaddwindow.xaml", UriKind.Relative));
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
          this.Root = (QuickAddWindow) target;
          break;
        case 2:
          this.WindowBorder = (Border) target;
          break;
        case 3:
          this.Container = (Grid) target;
          this.Container.MouseMove += new MouseEventHandler(this.TryDragWindow);
          break;
        case 4:
          this.TickLogo = (Image) target;
          break;
        case 5:
          this.TaskTitleGrid = (Grid) target;
          break;
        case 6:
          this.DetailGrid = (Grid) target;
          break;
        case 7:
          this.FocusTitleHighlightLine = (Grid) target;
          break;
        case 8:
          this.ContentScrollViewer = (ScrollViewer) target;
          this.ContentScrollViewer.Drop += new DragEventHandler(this.OnFileDrop);
          this.ContentScrollViewer.PreviewKeyDown += new KeyEventHandler(this.OnDetailKeyDown);
          this.ContentScrollViewer.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnContentHintClick);
          this.ContentScrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(this.OnMouseScroll);
          break;
        case 9:
          this.TextContentGrid = (Grid) target;
          break;
        case 10:
          this.ContentHint = (TextBlock) target;
          this.ContentHint.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnContentHintClick);
          break;
        case 11:
          this.ContentText = (MarkDownEditor) target;
          break;
        case 12:
          this.CheckItemsGrid = (Grid) target;
          break;
        case 13:
          this.DescHint = (TextBlock) target;
          break;
        case 14:
          this.DescriptionLinkTextBox = (MarkDownEditor) target;
          break;
        case 15:
          this.Checklist = (ChecklistControl) target;
          break;
        case 16:
          this.TagsContainer = (Grid) target;
          break;
        case 17:
          this.TagDisplayControl = (TagDisplayControl) target;
          break;
        case 18:
          this.FileScroller = (ScrollViewer) target;
          this.FileScroller.PreviewMouseWheel += new MouseWheelEventHandler(this.OnFileScrollerMouseWheel);
          break;
        case 21:
          this.OperationPanel = (StackPanel) target;
          break;
        case 22:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTextOrChecklistClick);
          break;
        case 23:
          this.AssignGrid = (Grid) target;
          break;
        case 24:
          this.AvatarImageRectangle = (Border) target;
          break;
        case 25:
          this.AvatarImage = (ImageBrush) target;
          break;
        case 26:
          this.AssignOtherGrid = (Image) target;
          break;
        case 27:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSetAssigneeClick);
          break;
        case 28:
          this.AssignPopup = (Popup) target;
          this.AssignPopup.Opened += new EventHandler(this.PopupOpened);
          this.AssignPopup.Closed += new EventHandler(this.PopupClosed);
          break;
        case 29:
          this.SetDateGrid = (Grid) target;
          this.SetDateGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetDateClick);
          break;
        case 30:
          this.SetDatePath = (Path) target;
          break;
        case 31:
          this.SetTagGrid = (Grid) target;
          break;
        case 32:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SetTagClick);
          break;
        case 33:
          this.SetTagPopup = (Popup) target;
          this.SetTagPopup.Opened += new EventHandler(this.PopupOpened);
          this.SetTagPopup.Closed += new EventHandler(this.PopupClosed);
          break;
        case 34:
          this.SetPriorityGrid = (Grid) target;
          break;
        case 35:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SetPriorityClick);
          break;
        case 36:
          this.SetPriorityPopup = (Popup) target;
          this.SetPriorityPopup.Opened += new EventHandler(this.PopupOpened);
          this.SetPriorityPopup.Closed += new EventHandler(this.PopupClosed);
          break;
        case 37:
          this.ProjectPanel = (StackPanel) target;
          this.ProjectPanel.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnProjectMouseDown);
          this.ProjectPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetProjectClick);
          break;
        case 38:
          this.ProjectName = (TextBlock) target;
          break;
        case 39:
          this.SetProjectPopup = (Popup) target;
          this.SetProjectPopup.Opened += new EventHandler(this.PopupOpened);
          this.SetProjectPopup.Closed += new EventHandler(this.PopupClosed);
          break;
        case 40:
          this.OkCancelGrid = (Grid) target;
          break;
        case 41:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 42:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 19)
      {
        if (connectionId != 20)
          return;
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.DeleteFile);
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAttachmentClick);
    }
  }
}
