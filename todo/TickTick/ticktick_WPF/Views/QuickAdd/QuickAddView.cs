// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.QuickAddView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.DateParser;
using ticktick_WPF.Util.Files;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Time;
using TickTickModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class QuickAddView : UserControl, IComponentConnector, IStyleConnector
  {
    private QuickAddView.Scenario _scenario;
    public AvatarViewModel Avatar;
    private List<AvatarViewModel> _avatars;
    private bool _canRender;
    private string _content;
    private string _dateTemplate;
    private bool _isPopupOpen;
    private bool _isInOperation;
    private bool _manualSelectedDate;
    private bool _needClearTag;
    private string _priorityTemplate;
    private string _projectTemplate;
    private QuickInputItems _quickInputItems;
    private DateTime? _quickSelectedDate;
    private DateTime? _tagAddTime;
    private bool _wronglyTimeSetToastShowed;
    public EscPopup OperationPopup;
    public EventHandler<List<AvatarViewModel>> NotifyAvatarsChanged;
    public bool? ShowDetail;
    private bool _adding;
    private IProjectTaskDefault _taskDefault;
    private int _tabIndex;
    private string _uid;
    private bool _mouseDown;
    internal QuickAddView RootView;
    internal Grid AddTaskGrid;
    internal Border InputBackground;
    internal Border InputBorder;
    internal StackPanel HintPanel;
    internal System.Windows.Shapes.Path AddPath;
    internal TextBlock HintTextBlock;
    internal ScrollViewer InputScrollViewer;
    internal QuickAddText TitleText;
    internal StackPanel OperationPanel;
    internal Border SetDateBorder;
    internal Border DateGrid;
    internal StackPanel DetailDateGrid;
    internal System.Windows.Shapes.Path SetDatePath;
    internal TextBlock DateText;
    internal Border SetCalBorder;
    internal EscPopup SetCalendarPopup;
    internal EscPopup SetProjectPopup;
    internal Border MoreGrid;
    internal Border AttachmentBorder;
    internal ScrollViewer FileScroller;
    private bool _contentLoaded;

    public bool IsInOperation
    {
      get => this._isInOperation;
      set
      {
        if (value)
        {
          DelayActionHandlerCenter.RemoveAction(this._uid);
          this._isInOperation = true;
        }
        else
          DelayActionHandlerCenter.TryDoAction(this._uid, (EventHandler) ((o, e) => this.Dispatcher.Invoke((Action) (() => this._isInOperation = false))), 100);
      }
    }

    public bool ManualSelectedDate
    {
      get => this._manualSelectedDate;
      set
      {
        this._manualSelectedDate = value;
        this.TitleText.ClearHighlightDate();
      }
    }

    public QuickAddView(
      IProjectTaskDefault taskDefault,
      QuickAddView.Scenario scenario = QuickAddView.Scenario.TaskList,
      string themeId = "light",
      bool focus = false,
      Section section = null)
      : this(AddTaskViewModel.Build(taskDefault, section), scenario, themeId, focus)
    {
      this._taskDefault = taskDefault;
    }

    public QuickAddView()
      : this((IProjectTaskDefault) ProjectIdentity.GetDefaultProject())
    {
      this.InitializeComponent();
    }

    private QuickAddView(
      AddTaskViewModel model,
      QuickAddView.Scenario scenario = QuickAddView.Scenario.TaskList,
      string themeId = "light",
      bool focus = false)
    {
      EscPopup escPopup = new EscPopup();
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.VerticalOffset = -5.0;
      escPopup.HorizontalOffset = -60.0;
      escPopup.StaysOpen = false;
      this.OperationPopup = escPopup;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      if (scenario == QuickAddView.Scenario.Widget)
        ThemeUtil.SetTheme(themeId, (FrameworkElement) this);
      this._uid = Utils.GetGuid();
      this.InitializeComponent();
      this.Model = model;
      this.Model.Init();
      this._scenario = scenario;
      this._needClearTag = this.Model.Tags == null || this.Model.Tags.Count == 0;
      this.DataContext = (object) this.Model;
      switch (scenario)
      {
        case QuickAddView.Scenario.QuickAdd:
          this.InitAddWindowStyle();
          break;
        case QuickAddView.Scenario.Widget:
          this.InitWidgetStyle();
          break;
        case QuickAddView.Scenario.Kanban:
          this.InitKanbanStyle();
          break;
      }
      this.InitQuickAddText();
      this.InitCalendarSettings();
      if (!focus)
        return;
      this.FocusEnd();
    }

    public void ResetView(IProjectTaskDefault taskDefault, bool focus = false, bool keepText = false)
    {
      this._taskDefault = taskDefault;
      this.Model = AddTaskViewModel.Build(taskDefault);
      this.Model.Init();
      this._needClearTag = this.Model.Tags == null || this.Model.Tags.Count == 0;
      this.DataContext = (object) this.Model;
      this.InitCalendarSettings();
      this._wronglyTimeSetToastShowed = false;
      this.Avatar = (AvatarViewModel) null;
      this._quickSelectedDate = new DateTime?();
      this.ManualSelectedDate = false;
      this.TitleText.ResetTokens();
      if (!keepText)
        this.TitleText.EditBox.Text = string.Empty;
      if (focus)
        this.FocusEnd();
      this.LoadAvatars();
      this.InputScrollViewer.ScrollToHorizontalOffset(0.0);
    }

    public AddTaskViewModel Model { get; private set; }

    public List<AvatarViewModel> GetAvatars() => this._avatars;

    public event EventHandler<TaskModel> TaskAdded;

    public event EventHandler<List<TaskModel>> BatchTaskAdded;

    public event EventHandler HideGuide;

    public event EventHandler InputPopupOpened;

    public event EventHandler InputPopupClosed;

    public event EventHandler TagsChanged;

    public void Reset(AddTaskViewModel model, QuickAddView.Scenario scenario = QuickAddView.Scenario.TaskList, bool focus = false)
    {
      this.Model = model;
      this._taskDefault = model.TaskDefault;
      this.Model.Init();
      this._scenario = scenario;
      this._needClearTag = this.Model.Tags == null || this.Model.Tags.Count == 0;
      this.DataContext = (object) this.Model;
      this._wronglyTimeSetToastShowed = false;
      this.Avatar = (AvatarViewModel) null;
      this._quickSelectedDate = new DateTime?();
      this.TitleText.ResetTokens();
      this.TitleText.EditBox.Text = string.Empty;
      this.ManualSelectedDate = false;
      if (focus)
        this.FocusEnd();
      this.LoadAvatars();
    }

    private void InitCalendarSettings()
    {
      if (!this.Model.IsCalendar || UserDao.IsUserValid())
        return;
      this.TitleText.EditBox.IsReadOnly = true;
    }

    private void OnViewUnloaded(object sender, RoutedEventArgs e)
    {
      this.TaskAdded = (EventHandler<TaskModel>) null;
      this.BatchTaskAdded = (EventHandler<List<TaskModel>>) null;
      this.HideGuide = (EventHandler) null;
      this.InputPopupOpened = (EventHandler) null;
      this.InputPopupClosed = (EventHandler) null;
      this.TagsChanged = (EventHandler) null;
      this.NotifyAvatarsChanged = (EventHandler<List<AvatarViewModel>>) null;
    }

    private async void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (this.ShowDetail.HasValue)
        this.HintTextBlock.Text = Utils.GetString("QuickAddWindowTitleHint");
      await this.LoadAvatars();
    }

    private void InitQuickAddText()
    {
      this.TitleText.TagSelected -= new EventHandler<string>(this.OnNewTagSelected);
      this.TitleText.TagSelected += new EventHandler<string>(this.OnNewTagSelected);
      this.TitleText.EnterText -= new EventHandler<string>(this.OnEnterText);
      this.TitleText.EnterText += new EventHandler<string>(this.OnEnterText);
      this.TitleText.PrioritySelect -= new EventHandler<int>(this.OnQuickPrioritySelect);
      this.TitleText.PrioritySelect += new EventHandler<int>(this.OnQuickPrioritySelect);
      this.TitleText.ProjectSelect -= new EventHandler<ProjectModel>(this.OnQuickProjectSelect);
      this.TitleText.ProjectSelect += new EventHandler<ProjectModel>(this.OnQuickProjectSelect);
      this.TitleText.CalendarSelect -= new EventHandler<string>(this.OnQuickCalendarSelect);
      this.TitleText.CalendarSelect += new EventHandler<string>(this.OnQuickCalendarSelect);
      this.TitleText.DateSelect -= new EventHandler<DateTime>(this.OnQuickDateSelect);
      this.TitleText.DateSelect += new EventHandler<DateTime>(this.OnQuickDateSelect);
      this.TitleText.AssigneeSelect -= new EventHandler<AvatarViewModel>(this.OnQuickAssignSelect);
      this.TitleText.AssigneeSelect += new EventHandler<AvatarViewModel>(this.OnQuickAssignSelect);
      this.TitleText.EditBox.GotFocus -= new RoutedEventHandler(this.TaskTextGotFocus);
      this.TitleText.EditBox.GotFocus += new RoutedEventHandler(this.TaskTextGotFocus);
      this.TitleText.EditBox.LostFocus -= new RoutedEventHandler(this.TaskTextLostFocus);
      this.TitleText.EditBox.LostFocus += new RoutedEventHandler(this.TaskTextLostFocus);
      this.TitleText.TextChanged -= new EventHandler<string>(this.OnTitleTextChanged);
      this.TitleText.TextChanged += new EventHandler<string>(this.OnTitleTextChanged);
      this.TitleText.DateParsed -= new EventHandler<IPaserDueDate>(this.OnDateParsed);
      this.TitleText.DateParsed += new EventHandler<IPaserDueDate>(this.OnDateParsed);
      this.TitleText.TokenRemoved -= new EventHandler<QuickAddToken>(this.OnTokenRemoved);
      this.TitleText.TokenRemoved += new EventHandler<QuickAddToken>(this.OnTokenRemoved);
      this.TitleText.PopupOpenChanged -= new EventHandler<bool>(this.PopupOpenChanged);
      this.TitleText.PopupOpenChanged += new EventHandler<bool>(this.PopupOpenChanged);
      this.KeyUp -= new KeyEventHandler(this.OnKeyUp);
      this.KeyUp += new KeyEventHandler(this.OnKeyUp);
      this.TitleText.TextKeyDown -= new EventHandler<KeyEventArgs>(this.OnTextKeyDown);
      this.TitleText.TextKeyDown += new EventHandler<KeyEventArgs>(this.OnTextKeyDown);
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Tab)
        this.TabSelectItem();
      if (e.Key != Key.V || !Utils.IfCtrlPressed())
        return;
      this.TryPasteAttachment(e);
    }

    public void TryPasteAttachment(KeyEventArgs e)
    {
      try
      {
        BitmapSource pastImage = this.GetPastImage();
        if (pastImage != null)
        {
          e.Handled = true;
          this.TryPasteImage(pastImage);
        }
        else
        {
          List<string> pastFiles = FileUtils.GetPastFiles();
          if (pastFiles == null || !pastFiles.Any<string>())
            return;
          e.Handled = true;
          this.TryPasteFile((IReadOnlyCollection<string>) pastFiles);
        }
      }
      catch (Exception ex)
      {
        Utils.Toast(Utils.GetString("FileNotFound"));
      }
    }

    private async Task<bool> TryPasteFile(IReadOnlyCollection<string> files)
    {
      if (files == null || files.Count <= 0)
        return false;
      if (!await this.CheckAttachmentLimit(files.Count))
        return true;
      if (files.Count <= 0)
        return false;
      foreach (string file in (IEnumerable<string>) files)
      {
        string path = FileUtils.SavePasteFile(file, IOUtils.GetFileName(file));
        try
        {
          if (File.Exists(path))
            this.Model.AddFile(path);
        }
        catch (Exception ex)
        {
          Utils.Toast(Utils.GetString("AddFailed"));
        }
      }
      return true;
    }

    private BitmapSource GetPastImage()
    {
      BitmapSource pastImage = (BitmapSource) null;
      try
      {
        if (string.IsNullOrEmpty(Clipboard.GetText()))
          pastImage = Clipboard.GetImage();
      }
      catch (Exception ex)
      {
      }
      return pastImage;
    }

    private async Task TryPasteImage(BitmapSource image)
    {
      if (image == null)
        return;
      if (!await this.CheckAttachmentLimit(1))
        return;
      string path = ImageUtils.SavePasteImage(image, Utils.GetGuid() + ".png");
      if (!File.Exists(path))
        return;
      this.Model.AddFile(path);
    }

    public async Task<bool> CheckAttachmentLimit(int fileNum)
    {
      QuickAddView quickAddView = this;
      fileNum += quickAddView.Model.Files.Count;
      if (Utils.GetResidueAttachmentCount(fileNum) < (long) fileNum)
      {
        quickAddView._isInOperation = true;
        if (UserDao.IsPro())
        {
          CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("AttachmentsLimit"), MessageBoxButton.OK);
          customerDialog.Owner = Window.GetWindow((DependencyObject) quickAddView);
          customerDialog.ShowDialog();
        }
        else
          ProChecker.ShowUpgradeDialog(ProType.MoreAttachments);
        quickAddView._isInOperation = false;
        return false;
      }
      if ((long) fileNum <= Utils.GetUserLimit(Constants.LimitKind.TaskAttachmentNumber))
        return true;
      quickAddView._isInOperation = true;
      CustomerDialog customerDialog1 = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("TaskAttachmentsLimitContent"), MessageBoxButton.OK);
      customerDialog1.Owner = Window.GetWindow((DependencyObject) quickAddView);
      customerDialog1.ShowDialog();
      quickAddView._isInOperation = false;
      return false;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      this.ClearTabSelect();
    }

    private void OnQuickCalendarSelect(object sender, string calendarId)
    {
      this.Model.CalendarId = calendarId;
    }

    private void OnTextOffsetChanged(object sender, double offset)
    {
      double num = this.InputScrollViewer.ActualWidth - 4.0;
      if (num <= 0.0)
        return;
      if (offset > num + this.InputScrollViewer.HorizontalOffset)
        this.InputScrollViewer.ScrollToHorizontalOffset(offset - num);
      if (this.InputScrollViewer.HorizontalOffset <= offset)
        return;
      this.InputScrollViewer.ScrollToHorizontalOffset(offset);
    }

    private void OnTokenRemoved(object sender, QuickAddToken token)
    {
      switch (token.TokenType)
      {
        case TokenType.Tag:
          this.Model.Tags.Remove("#" + token.Value.Replace("#", string.Empty).TrimEnd());
          break;
        case TokenType.Project:
          if (this.TitleText.ExistToken(TokenType.Project))
            break;
          this.Model.ResetProject();
          break;
        case TokenType.Priority:
          if (this.TitleText.ExistToken(TokenType.Priority))
            break;
          this.Model.ResetPriority();
          break;
        case TokenType.QuickDate:
          this._quickSelectedDate = new DateTime?();
          this.Model.StartDate = new DateTime?();
          this.Model.DetailDayText = string.Empty;
          this.Model.ResetHint();
          break;
      }
    }

    private void OnDateParsed(object sender, IPaserDueDate parseData)
    {
      this.GetValidDateTokens(parseData);
    }

    private void OnTitleTextChanged(object sender, string e) => this.RefreshHint();

    private void OnQuickAssignSelect(object sender, AvatarViewModel model)
    {
      AvatarViewModel avatarViewModel = AvatarHelper.GetCacheProjectAvatars(this.Model.ProjectId).FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (avatar => avatar.UserId == model.UserId));
      if (avatarViewModel == null)
        return;
      this.Avatar = avatarViewModel;
    }

    private void OnQuickDateSelect(object sender, DateTime date)
    {
      ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(new TimeData()
      {
        StartDate = new DateTime?(date)
      });
      TimeData timeData = TimeData.InitDefaultTime();
      this._quickSelectedDate = new DateTime?(date);
      timeData.StartDate = new DateTime?(date);
      this.Model.TimeData = timeData;
    }

    private async void OnQuickProjectSelect(object sender, ProjectModel project)
    {
      if (project.id != this.Model.ProjectId)
      {
        this.Model.ProjectId = project.id;
        this.Model.AddToColumnId = (string) null;
        this.Model.ProjectName = project.name;
        this.Model.IsNote = this._scenario != QuickAddView.Scenario.QuickAdd && project.IsNote;
      }
      await this.LoadAvatars();
    }

    private void OnQuickPrioritySelect(object sender, int priority)
    {
      this.Model.Priority = priority;
    }

    private async void OnEnterText(object sender, string text)
    {
      QuickAddView child = this;
      if (child._adding)
        return;
      child._adding = true;
      QuickAddWindow parent = Utils.FindParent<QuickAddWindow>((DependencyObject) child);
      if (parent != null)
      {
        if (!string.IsNullOrEmpty(child.TitleText.EditBox.Text))
          await parent.AddTask(text);
      }
      else
        await child.OnEnterKeyUp(text);
      child.InputScrollViewer.ScrollToHome();
      child._adding = false;
    }

    private async Task OnEnterKeyUp(string text)
    {
      if (this._tabIndex == 1)
        this.ShowSetDateDialog(true);
      else if (this._tabIndex == 2)
      {
        if (this.MoreGrid.IsVisible)
          this.ShowMorePopup(true);
        if (!this.SetCalBorder.IsVisible)
          return;
        this.IsInOperation = true;
        this.ShowSelectProjectPopup(true);
      }
      else
        await this.DoAddTask(text);
    }

    public virtual async Task DoAddTask(string text)
    {
      if (string.IsNullOrEmpty(this.TitleText.EditBox.Text) && this.Model.Files.Count == 0)
        return;
      if (await ProChecker.CheckTaskLimit(this.Model.ProjectId))
        return;
      List<string> selectedTags = this.TitleText.GetSelectedTags();
      AvatarViewModel selectedAvatar = this.GetSelectedAvatar();
      if (!this.TitleText.GetPriorityInTokens().HasValue)
        this.Model.ResetPriority();
      if (await this.AddTask(text ?? string.Empty, selectedTags, selectedAvatar == null ? string.Empty : selectedAvatar.UserId) == null)
        return;
      this.ResetAfterAddTask();
    }

    private void OnNewTagSelected(object sender, string tag)
    {
      if (this.ShowDetail.GetValueOrDefault() && Window.GetWindow((DependencyObject) this) is QuickAddWindow window)
      {
        window.OnTagAdded(tag);
      }
      else
      {
        if (this.Model.Tags.Contains(tag))
          return;
        this.Model.Tags.Add(tag);
      }
    }

    public AddTaskViewModel GetTaskAddModel() => this.Model;

    public AddTaskExtraInfo GetExtra()
    {
      AddTaskExtraInfo extra = new AddTaskExtraInfo()
      {
        Title = this.TitleText.GetTitleContent(),
        Tags = this.TitleText.GetSelectedTags()
      };
      AvatarViewModel selectedAvatar = this.GetSelectedAvatar();
      if (selectedAvatar != null)
        extra.AssigneeId = selectedAvatar.UserId;
      return extra;
    }

    private async Task LoadAvatars()
    {
      QuickAddView quickAddView = this;
      if (quickAddView.Model != null)
      {
        // ISSUE: reference to a compiler-generated method
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(quickAddView.\u003CLoadAvatars\u003Eb__82_0));
        if (projectModel != null && projectModel.IsShareList())
        {
          List<AvatarViewModel> projectAvatars1 = await AvatarHelper.GetProjectAvatars(quickAddView.Model.ProjectId);
          quickAddView._avatars = projectAvatars1;
          if (!quickAddView._avatars.Any<AvatarViewModel>())
          {
            List<AvatarViewModel> projectAvatars2 = await AvatarHelper.GetProjectAvatars(quickAddView.Model.ProjectId, fetchRemote: true);
            quickAddView._avatars = projectAvatars2;
          }
        }
        else
          quickAddView._avatars = new List<AvatarViewModel>();
      }
      else
        quickAddView._avatars = new List<AvatarViewModel>();
      EventHandler<List<AvatarViewModel>> notifyAvatarsChanged = quickAddView.NotifyAvatarsChanged;
      if (notifyAvatarsChanged == null)
        return;
      notifyAvatarsChanged((object) null, quickAddView._avatars);
    }

    public void SetAvatars(List<AvatarViewModel> avatars) => this._avatars = avatars;

    public List<AvatarViewModel> GetProjectAvatars() => this._avatars;

    public void SetAvatar(AvatarViewModel avatar) => this.Avatar = avatar;

    private void TryInitAddTag(List<string> tags)
    {
      if (tags == null || tags.Count <= 0)
        return;
      tags = tags.Select<string, string>((Func<string, string>) (tag => tag.Replace("#", string.Empty))).ToList<string>();
      this.TitleText.InitTagTokens(tags);
    }

    public void TryInitAvatar(string assignee)
    {
      if (string.IsNullOrEmpty(assignee))
        return;
      List<AvatarViewModel> cacheProjectAvatars = AvatarHelper.GetCacheProjectAvatars(this.Model.ProjectId);
      AvatarViewModel avatarViewModel = cacheProjectAvatars != null ? cacheProjectAvatars.FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (avatar => avatar.UserId == assignee)) : (AvatarViewModel) null;
      if (avatarViewModel == null)
        return;
      this.Avatar = avatarViewModel;
      this.TitleText.OnTokenSelected(QuickAddToken.BuildAssignee("@" + avatarViewModel.Name), true);
      this.RefreshHint();
    }

    private void InitKanbanStyle()
    {
      this.DateGrid.Visibility = Visibility.Visible;
      this.DetailDateGrid.Visibility = Visibility.Collapsed;
      this.AddTaskGrid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
    }

    private void InitWidgetStyle()
    {
      this.TitleText.FontSize = 14.0;
      this.AddTaskGrid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
      this.OperationPanel.Visibility = Visibility.Collapsed;
    }

    public async void ShowBatchAddWindow(string text)
    {
      QuickAddView quickAddView = this;
      bool closeClick = false;
      quickAddView._isInOperation = true;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("BatchAddTasks"), Utils.GetString("BatchAddTasksContent"), Utils.GetString("Add"), Utils.GetString("Cancel"));
      customerDialog.Owner = Window.GetWindow((DependencyObject) quickAddView);
      customerDialog.CloseClick += (EventHandler) ((sender, e) => closeClick = true);
      bool? nullable = customerDialog.ShowDialog();
      quickAddView._isInOperation = false;
      if (closeClick)
        return;
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
      {
        text = text.Replace("\n", " ").Replace("\r", " ");
        int selectionStart = quickAddView.TitleText.EditBox.SelectionStart;
        string str = (quickAddView.TitleText.EditBox.SelectionLength > 0 ? quickAddView.TitleText.EditBox.Text.Remove(selectionStart, quickAddView.TitleText.EditBox.SelectionLength) : quickAddView.TitleText.EditBox.Text).Insert(selectionStart, text);
        quickAddView.TitleText.EditBox.Text = str;
        quickAddView.TitleText.EditBox.CaretOffset = selectionStart + text.Length;
      }
      else
        await quickAddView.BatchAddTask(text);
    }

    private async Task BatchAddTask(string text)
    {
      QuickAddView sender = this;
      string[] titles;
      if (string.IsNullOrEmpty(text))
      {
        titles = (string[]) null;
      }
      else
      {
        titles = text.Trim().Split('\n');
        if (!((IEnumerable<string>) titles).Any<string>())
          titles = (string[]) null;
        else if (await ProChecker.CheckTaskLimit(sender.Model.ProjectId, titles.Length))
        {
          titles = (string[]) null;
        }
        else
        {
          bool? nullable;
          if (sender.Model.TimeData != null)
          {
            TimeData timeData = sender.Model.TimeData;
            List<TaskReminderModel> taskReminderModelList;
            if (sender.Model.TimeData.IsAllDay.HasValue)
            {
              nullable = sender.Model.TimeData.IsAllDay;
              if (nullable.Value)
              {
                taskReminderModelList = TimeData.GetDefaultAllDayReminders();
                goto label_8;
              }
            }
            taskReminderModelList = TimeData.GetDefaultTimeReminders();
label_8:
            timeData.Reminders = taskReminderModelList;
          }
          List<string> list = ((IEnumerable<string>) titles).ToList<string>();
          string projectId = sender.Model.ProjectId;
          TimeData timeData1 = sender.Model.TimeData;
          int priority = sender.Model.IsNote ? 0 : sender.Model.Priority;
          List<string> selectedTags = sender.TitleText.GetSelectedTags();
          string projectColumnId = sender.Model.GetProjectColumnId();
          int num = sender.Model.IsNote ? 1 : 0;
          DateTime? startDate = sender.Model.OriginalTimeData.StartDate;
          nullable = new bool?();
          bool? addTop = nullable;
          long? targetSortOrder = new long?();
          List<TaskModel> taskModelList = await TaskService.BatchAddTasks(list, projectId, timeData1, priority, selectedTags, columnId: projectColumnId, isNote: num != 0, defaultDate: startDate, addTop: addTop, targetSortOrder: targetSortOrder);
          TaskChangeNotifier.NotifyTaskBatchAdded(taskModelList.Select<TaskModel, string>((Func<TaskModel, string>) (model => model.id)).ToList<string>());
          EventHandler<List<TaskModel>> batchTaskAdded = sender.BatchTaskAdded;
          if (batchTaskAdded != null)
            batchTaskAdded((object) sender, taskModelList);
          sender.ResetAfterAddTask();
          titles = (string[]) null;
        }
      }
    }

    private void InitAddWindowStyle()
    {
      this.TitleText.FontSize = 20.0;
      this.TitleText.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
      this.AddPath.Visibility = Visibility.Collapsed;
      this.AddTaskGrid.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
      this.HintTextBlock.Margin = new Thickness(1.0, 0.0, 0.0, 0.0);
      this.InputBorder.Visibility = Visibility.Collapsed;
      this.InputBackground.Visibility = Visibility.Collapsed;
    }

    private void SetDateClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      if (!this._mouseDown)
        return;
      this._mouseDown = false;
      this.ShowSetDateDialog();
    }

    private void ShowSetDateDialog(bool showTab = false)
    {
      if (this.Model.TimeData == null)
        this.Model.TimeData = new TimeData()
        {
          IsDefault = false
        };
      if (this.Model.IsCalendar)
      {
        DateTime? dueDate = this.Model.TimeData.DueDate;
        if (!dueDate.HasValue)
        {
          dueDate = this.Model.TimeData.DueDate;
          if (!dueDate.HasValue)
            this.Model.TimeData.DueDate = this.Model.TimeData.StartDate;
        }
      }
      SetDateDialog dialog = SetDateDialog.GetDialog(showTab);
      dialog.ClearEventHandle();
      dialog.Save += (EventHandler<TimeData>) ((obj, data) =>
      {
        TaskService.TryFixRepeatFlag(ref data);
        this.Model.TimeData = TimeData.Clone(data);
        this.Model.TimeData.IsDefault = false;
        this.ManualSelectedDate = true;
        if (!data.DueDate.HasValue)
          ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(data);
        if (!this.Model.IsCalendar || !this.Model.TimeData.StartDate.HasValue)
          return;
        TimeData timeData = this.Model.TimeData;
        bool? isAllDay;
        int num;
        if (timeData == null)
        {
          num = 0;
        }
        else
        {
          isAllDay = timeData.IsAllDay;
          num = isAllDay.HasValue ? 1 : 0;
        }
        if (num == 0)
          return;
        isAllDay = this.Model.TimeData.IsAllDay;
        if (!isAllDay.Value)
          return;
        this.Model.TimeData.DueDate = new DateTime?(this.Model.TimeData.DueDate ?? this.Model.TimeData.StartDate.Value);
      });
      dialog.Clear += (EventHandler) ((obj, arg) =>
      {
        if (!this.Model.IsCalendar)
          this.Model.TimeData = (TimeData) null;
        this.ManualSelectedDate = true;
        this.TitleText.ClearHighlightDate();
      });
      dialog.Hided += new EventHandler(this.PopupClosed);
      bool showRemind = true;
      if (this.Model.IsCalendar)
        showRemind = CacheManager.GetAccountCalById(this.Model.AccountId)?.Kind != "icloud";
      dialog.Show(this.Model.TimeData, new SetDateDialogArgs(this.Model.IsCalendar, this.Model.IsNote, target: (UIElement) this.SetDateBorder, hOffset: -80.0, vOffset: 20.0, canSkip: false, showRemind: showRemind));
      this.IsInOperation = true;
    }

    private async void PopupOpenChanged(object sender, bool e)
    {
      QuickAddView quickAddView = this;
      if (!e)
      {
        Window window = Window.GetWindow((DependencyObject) quickAddView);
        if (window != null && window.IsActive)
        {
          quickAddView.TitleText.EditBox.Focus();
          quickAddView.TitleText.FocusText();
        }
      }
      else
        quickAddView.ClearTabSelect();
      quickAddView.IsInOperation = e;
    }

    private void PopupOpened(object sender, EventArgs e) => this.IsInOperation = true;

    private async void PopupClosed(object sender, EventArgs e)
    {
      QuickAddView quickAddView = this;
      Window window = Window.GetWindow((DependencyObject) quickAddView);
      if (window != null && window.IsActive)
      {
        quickAddView.TitleText.EditBox.Focus();
        quickAddView.TitleText.FocusText();
      }
      quickAddView.IsInOperation = false;
    }

    private void SetProjectClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.IsInOperation = true;
      this.ShowSelectProjectPopup();
    }

    public virtual void ShowSelectProjectPopup(bool enter = false)
    {
      EscPopup setProjectPopup = this.SetProjectPopup;
      ProjectExtra data = new ProjectExtra();
      List<string> stringList;
      if (!this.Model.SelectProject)
      {
        stringList = new List<string>();
      }
      else
      {
        stringList = new List<string>();
        stringList.Add(this.Model.ProjectId);
      }
      data.ProjectIds = stringList;
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) setProjectPopup, data, new ProjectSelectorExtra()
      {
        showAll = false,
        batchMode = false,
        canSelectGroup = false,
        onlyShowPermission = true,
        ShowColumn = true,
        ColumnId = this.Model.GetProjectColumnId()
      });
      projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnProjectSelect);
      projectOrGroupPopup.Closed += (EventHandler) ((sender, e) => this.IsInOperation = false);
      projectOrGroupPopup.Show();
      if (enter)
        projectOrGroupPopup.HoverSelectFirst();
      this.IsInOperation = true;
    }

    public void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      (string str1, string str2) = e.GetProjectAndColumnId();
      ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == str1));
      if (project == null)
        return;
      this.OnProjectSelect(project);
      this.Model.AddToColumnId = str2;
    }

    private async void OnProjectSelect(ProjectModel project)
    {
      this.TitleText.EditBox.Focus();
      this.SetProjectPopup.IsOpen = false;
      this.Model.ProjectId = project.id;
      this.Model.ProjectName = project.name ?? Utils.GetString("Inbox");
      this.Model.IsNote = this._scenario != QuickAddView.Scenario.QuickAdd && project.IsNote;
      await this.LoadAvatars();
      this.TitleText.OnTokenSelected(QuickAddToken.BuildProject("^" + project.name), true);
    }

    public AvatarViewModel GetSelectedAvatar() => this.Avatar;

    protected void ResetAfterAddTask()
    {
      this.ManualSelectedDate = false;
      this.Model.Reset(this._taskDefault);
      this.Model.ResetHint();
      if (this._needClearTag)
      {
        this.Model.OriginalTags = new List<string>();
        this.Model.Tags = new List<string>();
      }
      this._wronglyTimeSetToastShowed = false;
      this.Avatar = (AvatarViewModel) null;
      this._quickSelectedDate = new DateTime?();
      this.TitleText.ResetTokens();
      this.TitleText.EditBox.Text = string.Empty;
      this.TryInitAddTag(this.Model.Tags);
      this.TryInitAvatar(this.Model.Assignee);
    }

    public void ResetHint() => this.Model.ResetHint();

    private async Task<TaskModel> AddTask(string title, List<string> tags, string assignId = "")
    {
      QuickAddView sender = this;
      title = title.Trim();
      TaskModel task = new TaskModel()
      {
        id = Utils.GetGuid(),
        kind = sender.Model.IsNote ? "NOTE" : "TEXT",
        title = title,
        priority = sender.Model.IsNote ? 0 : sender.Model.Priority,
        projectId = sender.Model.ProjectId,
        columnId = sender.Model.ProjectChanged() ? sender.Model.AddToColumnId : sender.Model.GetProjectColumnId(),
        tag = TagSerializer.ToJsonContent(tags),
        assignee = assignId
      };
      TagService.CheckTagsExist(tags);
      if (sender.Model.TimeData != null)
      {
        task.startDate = sender.Model.TimeData.StartDate;
        task.dueDate = sender.Model.IsNote ? new DateTime?() : sender.Model.TimeData.DueDate;
        task.isAllDay = sender.Model.TimeData.IsAllDay;
        task.repeatFrom = sender.Model.IsNote ? (string) null : sender.Model.TimeData.RepeatFrom;
        task.repeatFlag = sender.Model.IsNote ? (string) null : sender.Model.TimeData.RepeatFlag;
        task.isFloating = new bool?(sender.Model.TimeData.TimeZone.IsFloat);
        task.timeZone = sender.Model.TimeData.TimeZone.TimeZoneName;
        if (sender.Model.TimeData.Reminders != null && sender.Model.TimeData.Reminders.Count > 0)
        {
          task.reminders = sender.Model.TimeData.Reminders.ToArray();
          task.reminderCount = task.reminders?.Length.Value;
          TaskReminderModel[] taskReminderModelArray = task.reminders;
          for (int index = 0; index < taskReminderModelArray.Length; ++index)
          {
            TaskReminderModel taskReminderModel = taskReminderModelArray[index];
            taskReminderModel.id = Utils.GetGuid();
            taskReminderModel.taskserverid = task.id;
            int num = await TaskReminderDao.SaveReminders(taskReminderModel);
          }
          taskReminderModelArray = (TaskReminderModel[]) null;
        }
      }
      AvatarViewModel selectedAvatar = sender.GetSelectedAvatar();
      task.assignee = selectedAvatar == null ? string.Empty : selectedAvatar.UserId;
      if (selectedAvatar == null)
        task.assignee = assignId;
      task.createdTime = new DateTime?(DateTime.Now);
      task.sortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(task.projectId, string.Empty);
      if (sender._taskDefault is ProjectIdentity taskDefault)
        await TaskSortOrderService.NewTaskAdded(task?.id, taskDefault.CatId, taskDefault.SortOption);
      if (sender.Model.IsPin)
        task.pinnedTime = DateTime.Now.ToString(UtcDateTimeConverter.GetConverterValue(DateTime.Now));
      if (sender.Model.IsComplete)
      {
        task.status = 2;
        task.completedTime = new DateTime?(DateTime.Now);
        task.completedUserId = LocalSettings.Settings.LoginUserId;
      }
      List<AttachmentModel> attachments = await AttachmentProvider.AddAttachmentModels(task.id, sender.Model.ClearAndGetFiles());
      if (attachments.Count > 0)
      {
        AttachmentProvider.AppendTaskContent(task, attachments);
        AttachmentCache.SetTodayAttachmentCount(AttachmentCache.TodayAttachmentCount() + attachments.Count);
      }
      TaskModel taskModel1 = await TaskService.AddTask(task, sender: (object) sender);
      EventHandler<TaskModel> taskAdded = sender.TaskAdded;
      if (taskAdded != null)
        taskAdded((object) sender, task);
      TaskUtils.TryGetTaskLinkTitle(task);
      if (sender._scenario != QuickAddView.Scenario.Widget && sender._scenario != QuickAddView.Scenario.QuickAdd && sender.Model.ProjectId != sender.Model.OriginalProjectId)
        Utils.GetToastWindow()?.ToastMoveProjectControl(task.projectId, task.title, MoveToastType.Add);
      TaskModel taskModel2 = task;
      task = (TaskModel) null;
      return taskModel2;
    }

    private void TaskTextGotFocus(object sender, RoutedEventArgs e)
    {
      if (this._scenario == QuickAddView.Scenario.AddWindow || this._scenario == QuickAddView.Scenario.Widget || this._scenario == QuickAddView.Scenario.QuickAdd && this.ShowDetail.GetValueOrDefault())
        return;
      this.OperationPanel.Visibility = Visibility.Visible;
    }

    private async void TaskTextLostFocus(object sender, RoutedEventArgs e)
    {
      QuickAddView quickAddView = this;
      quickAddView.RefreshHint();
      await Task.Delay(30);
      // ISSUE: explicit non-virtual call
      if (!quickAddView.ShowDetail.HasValue && !__nonvirtual (quickAddView.IsMouseOver) && !quickAddView.IsInOperation && !quickAddView.TitleText.Focused)
      {
        quickAddView.OperationPanel.Visibility = Visibility.Collapsed;
        quickAddView.ClearTabSelect();
      }
      if (quickAddView.OperationPanel.IsVisible)
        return;
      List<string> tags1 = quickAddView.Model.Tags;
      // ISSUE: explicit non-virtual call
      if ((tags1 != null ? (__nonvirtual (tags1.Count) > 0 ? 1 : 0) : 0) == 0 && string.IsNullOrEmpty(quickAddView.Model.Assignee) || quickAddView.TitleText.EditBox.TextArea.IsKeyboardFocused)
        return;
      string seed = quickAddView.TitleText.EditBox.Text ?? string.Empty;
      List<string> tags2 = quickAddView.Model.Tags;
      // ISSUE: explicit non-virtual call
      if ((tags2 != null ? (__nonvirtual (tags2.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        List<string> list = quickAddView.Model.Tags.ToList<string>();
        list.Sort((Comparison<string>) ((a, b) => b.Length.CompareTo(a.Length)));
        seed = list.Aggregate<string, string>(seed, (Func<string, string, string>) ((current, tag) => current.Replace("#" + tag + " ", "")));
      }
      if (!string.IsNullOrEmpty(quickAddView.Model.Assignee))
        seed = seed.Replace("@" + quickAddView.Avatar?.Name + " ", "");
      if (!string.IsNullOrEmpty(seed))
        return;
      quickAddView.TitleText.EditBox.Text = "";
      quickAddView.Model.AutoAddTags = true;
    }

    protected void RefreshHint()
    {
      this.HintPanel.Visibility = string.IsNullOrEmpty(this.TitleText.EditBox.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    public bool IsContentEmpty() => string.IsNullOrEmpty(this.TitleText.EditBox.Text);

    protected void CalendarSelect(string title, string id)
    {
      this.Model.CalendarName = title;
      this.Model.CalendarId = id;
      this.Model.ResetHint();
      this.RefreshHint();
    }

    public void OnAssigneeSelect(AvatarInfo model)
    {
      this.TitleText.EditBox.Focus();
      AvatarViewModel avatarViewModel = AvatarHelper.GetCacheProjectAvatars(this.Model.ProjectId).FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (avatar => avatar.UserId == model.UserId));
      if (avatarViewModel != null)
      {
        this.Avatar = avatarViewModel;
        this.TitleText.OnTokenSelected(QuickAddToken.BuildAssignee("@" + avatarViewModel.Name), true);
      }
      else
      {
        this.Avatar = (AvatarViewModel) null;
        this.TitleText.RemoveTokenByType(TokenType.Assignee);
      }
    }

    public async void OnTagsAdded(TagSelectData data)
    {
      QuickAddView quickAddView = this;
      quickAddView.TitleText.OnTagsAdded(data);
      if (!quickAddView.OperationPopup.IsOpen)
      {
        quickAddView.TitleText.FocusText();
      }
      else
      {
        Window.GetWindow((DependencyObject) quickAddView)?.Activate();
        quickAddView.OperationPopup.IsOpen = false;
      }
    }

    public void FocusEnd() => this.TitleText.FocusText(true);

    private void GetValidDateTokens(IPaserDueDate parsedData)
    {
      if (this.ManualSelectedDate)
        return;
      string text = this.TitleText.EditBox.Text;
      if (parsedData?.GetRecognizeStrings() != null && parsedData.IsTimeSeted() && parsedData.GetRecognizeStrings().Count > 0)
      {
        this.Model.TimeData = parsedData.ToTimeData(!string.IsNullOrEmpty(text) && text.Contains(Utils.GetString("reminder")));
        if (this.Model.IsNote)
          this.Model.TimeData.DueDate = new DateTime?();
        if (this._wronglyTimeSetToastShowed)
          return;
        ticktick_WPF.Util.DateUtils.CheckIfTomorrowWronglySet(this.Model.TimeData);
        this._wronglyTimeSetToastShowed = true;
      }
      else if (!Utils.IsEmptyDate(this._quickSelectedDate))
      {
        TimeData timeData = TimeData.InitDefaultTime();
        timeData.StartDate = this._quickSelectedDate;
        this.Model.TimeData = timeData;
      }
      else
        this.Model.ResetTimeData(this._taskDefault);
    }

    public void FocusText() => this.TitleText.EditBox.Focus();

    public void SetPriority(int priority)
    {
      this.Model.Priority = priority;
      this.TitleText.OnTokenSelected(QuickAddToken.BuildPriority("!" + Utils.GetPriorityName(priority)), true);
    }

    public void SetDate(string key)
    {
      DateTime dateTime = DateTime.Today;
      switch (key)
      {
        case "today":
          dateTime = DateTime.Today;
          break;
        case "tomorrow":
          dateTime = DateTime.Today.AddDays(1.0);
          break;
        case "nextweek":
          dateTime = DateTime.Today.AddDays(7.0);
          break;
      }
      this.ManualSelectedDate = true;
      TimeData timeData = TimeData.InitDefaultTime();
      timeData.StartDate = new DateTime?(dateTime);
      this.Model.TimeData = timeData;
    }

    public void ClearDate()
    {
      this.ManualSelectedDate = true;
      this.Model.TimeData = (TimeData) null;
    }

    public void SelectDate() => this.ShowSetDateDialog();

    private void OnMoreClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      if (!this._mouseDown)
        return;
      this._mouseDown = false;
      this.ShowMorePopup(false);
    }

    private void ShowMorePopup(bool isEnter)
    {
      this.OperationPopup.Closed -= new EventHandler(this.PopupClosed);
      this.OperationPopup.Closed += new EventHandler(this.PopupClosed);
      this.OperationPopup.Opened -= new EventHandler(this.PopupOpened);
      this.OperationPopup.Opened += new EventHandler(this.PopupOpened);
      this.OperationPopup.PlacementTarget = (UIElement) this.MoreGrid;
      if (!this.TitleText.GetPriorityInTokens().HasValue)
        this.Model.ResetPriority();
      AddOptionDialog addOptionDialog = new AddOptionDialog();
      addOptionDialog.Init(this.Model, isEnter);
      addOptionDialog.QuickAddView = this;
      this.OperationPopup.Child = (UIElement) addOptionDialog;
      this.OperationPopup.IsOpen = true;
      EventHandler hideGuide = this.HideGuide;
      if (hideGuide == null)
        return;
      hideGuide((object) this, (EventArgs) null);
    }

    public void NotifyAddTask(TaskModel task)
    {
      if (task == null)
        return;
      this.ResetAfterAddTask();
      this.RefreshHint();
      EventHandler<TaskModel> taskAdded = this.TaskAdded;
      if (taskAdded == null)
        return;
      taskAdded((object) this, task);
    }

    public async Task HideOptionDialog(bool stopOperate = false)
    {
      QuickAddView quickAddView = this;
      if (stopOperate)
        quickAddView.IsInOperation = false;
      quickAddView.OperationPopup.Closed -= new EventHandler(quickAddView.PopupClosed);
      quickAddView.OperationPopup.IsOpen = false;
      await Task.Delay(10);
      quickAddView.TitleText.EditBox.Focus();
      quickAddView.TitleText.FocusText();
    }

    public async void SwitchView()
    {
      QuickAddView quickAddView1 = this;
      QuickAddView quickAddView2 = quickAddView1;
      bool? showDetail = quickAddView1.ShowDetail;
      bool? nullable = showDetail.HasValue ? new bool?(!showDetail.GetValueOrDefault()) : new bool?();
      quickAddView2.ShowDetail = nullable;
      quickAddView1.AttachmentBorder.Visibility = Visibility.Collapsed;
      quickAddView1.TitleText.FontSize = 16.0;
      quickAddView1.HintTextBlock.Text = Utils.GetString("RightTitle");
      quickAddView1.MaxHeight = 40.0;
      quickAddView1.OperationPanel.Visibility = Visibility.Collapsed;
      quickAddView1.ClearTabSelect();
      await Task.Delay(10);
      if (!quickAddView1.IsContentEmpty())
        return;
      quickAddView1.TitleText.FocusText();
    }

    public void OnAddTask(TaskModel task)
    {
      EventHandler<TaskModel> taskAdded = this.TaskAdded;
      if (taskAdded == null)
        return;
      taskAdded((object) this, task);
    }

    public List<string> GetSelectedTags() => this.TitleText.GetSelectedTags();

    private void TitleTextOnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    public void ClearTag() => this.TitleText.RemoveTagTokenText();

    private void OnAddViewMouseLeftUp(object sender, MouseButtonEventArgs e)
    {
      this.TitleText.FocusText();
    }

    public bool TabSelectItem()
    {
      if (this.OperationPopup.IsOpen || this.IsInOperation)
        return false;
      if (!this.TitleText.Focused)
      {
        this.TitleText.FocusText(true);
        this.ClearTabSelect();
      }
      else
      {
        this._tabIndex += 3 + (Utils.IfShiftPressed() ? -1 : 1);
        this._tabIndex %= 3;
        switch (this._tabIndex)
        {
          case 0:
            this.ClearTabSelect();
            break;
          case 1:
            this.SetDateBorder.SetResourceReference(Control.BorderBrushProperty, (object) "PrimaryColor");
            this.SetCalBorder.BorderBrush = (Brush) Brushes.Transparent;
            this.MoreGrid.BorderBrush = (Brush) Brushes.Transparent;
            break;
          case 2:
            this.SetDateBorder.BorderBrush = (Brush) Brushes.Transparent;
            this.SetCalBorder.SetResourceReference(Control.BorderBrushProperty, (object) "PrimaryColor");
            this.MoreGrid.SetResourceReference(Control.BorderBrushProperty, (object) "PrimaryColor");
            break;
        }
      }
      return true;
    }

    public void ClearTabSelect()
    {
      this._tabIndex = 0;
      this.SetDateBorder.BorderBrush = (Brush) Brushes.Transparent;
      this.SetCalBorder.BorderBrush = (Brush) Brushes.Transparent;
      this.MoreGrid.BorderBrush = (Brush) Brushes.Transparent;
    }

    public async Task TryAddTask() => await this.DoAddTask(this.TitleText.GetTitleContent());

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this._isInOperation)
        return;
      this._mouseDown = true;
    }

    private void OnPopupClick(object sender, MouseButtonEventArgs e) => e.Handled = true;

    public bool IsLostFocus() => this.OperationPanel.Visibility != 0;

    public async Task TryAddTaskOnLostFocus()
    {
      if (string.IsNullOrWhiteSpace(this.TitleText.GetTitleContent(true)))
        return;
      await this.TryAddTask();
    }

    public void TryFocus()
    {
      this.TitleText.EditBox.Focus();
      this.TitleText.FocusText();
    }

    private void DeleteFile(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is AddTaskAttachmentInfo dataContext))
        return;
      this.Model.Remove(dataContext);
    }

    public void OnFileDrop(object sender, DragEventArgs e)
    {
      e.Handled = true;
      this.TryPasteFile((IReadOnlyCollection<string>) QuickAddView.GetDropFiles(e));
    }

    private static List<string> GetDropFiles(DragEventArgs e)
    {
      Array data = (Array) e.Data.GetData(DataFormats.FileDrop);
      List<string> dropFiles = new List<string>();
      if (data != null)
      {
        foreach (object obj in data)
          dropFiles.Add(obj.ToString());
      }
      return dropFiles;
    }

    private void OnFileScrollerMouseWheel(object sender, MouseWheelEventArgs e)
    {
      this.FileScroller.ScrollToHorizontalOffset(this.FileScroller.HorizontalOffset - (e.Delta > 0 ? 56.0 : -56.0));
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
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/quickadd/quickaddview.xaml", UriKind.Relative));
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
          this.RootView = (QuickAddView) target;
          this.RootView.Loaded += new RoutedEventHandler(this.OnViewLoaded);
          this.RootView.Unloaded += new RoutedEventHandler(this.OnViewUnloaded);
          this.RootView.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddViewMouseLeftUp);
          break;
        case 2:
          this.AddTaskGrid = (Grid) target;
          this.AddTaskGrid.Drop += new DragEventHandler(this.OnFileDrop);
          break;
        case 3:
          this.InputBackground = (Border) target;
          break;
        case 4:
          this.InputBorder = (Border) target;
          break;
        case 5:
          this.HintPanel = (StackPanel) target;
          break;
        case 6:
          this.AddPath = (System.Windows.Shapes.Path) target;
          break;
        case 7:
          this.HintTextBlock = (TextBlock) target;
          break;
        case 8:
          this.InputScrollViewer = (ScrollViewer) target;
          break;
        case 9:
          this.TitleText = (QuickAddText) target;
          break;
        case 10:
          this.OperationPanel = (StackPanel) target;
          break;
        case 11:
          this.SetDateBorder = (Border) target;
          break;
        case 12:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.SetDateClick);
          break;
        case 13:
          this.DateGrid = (Border) target;
          break;
        case 14:
          this.DetailDateGrid = (StackPanel) target;
          break;
        case 15:
          this.SetDatePath = (System.Windows.Shapes.Path) target;
          break;
        case 16:
          this.DateText = (TextBlock) target;
          break;
        case 17:
          this.SetCalBorder = (Border) target;
          break;
        case 18:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SetProjectClick);
          break;
        case 19:
          this.SetCalendarPopup = (EscPopup) target;
          break;
        case 20:
          this.SetProjectPopup = (EscPopup) target;
          break;
        case 21:
          this.MoreGrid = (Border) target;
          break;
        case 22:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
          break;
        case 23:
          this.AttachmentBorder = (Border) target;
          break;
        case 24:
          this.FileScroller = (ScrollViewer) target;
          this.FileScroller.PreviewMouseWheel += new MouseWheelEventHandler(this.OnFileScrollerMouseWheel);
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
      if (connectionId != 25)
      {
        if (connectionId != 26)
          return;
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.DeleteFile);
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAttachmentClick);
    }

    public enum Scenario
    {
      TaskList,
      QuickAdd,
      Widget,
      AddWindow,
      Kanban,
    }
  }
}
