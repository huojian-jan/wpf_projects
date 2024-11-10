// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.QuickAddText
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.DateParser;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class QuickAddText : UserControl, IEmojiRender, IDateParseBox, IComponentConnector
  {
    private DateTokenGenerator _dateTokenGenerator;
    private string _filterContent;
    private string _quickDate;
    private TokenGenerator _tokenGenerator;
    private bool _canEnter;
    private QuickSelectionControl _selectionControl;
    public readonly Dictionary<int, string> EmojiDict = new Dictionary<int, string>();
    private readonly DelayActionHandler _dateParserHandler = new DelayActionHandler(150);
    private bool _canMoveDown;
    private DateTime _lastKeyDownTime;
    private string _origin;
    private bool _inputByKeyBoard;
    private int _startIndex;
    private HashSet<string> _projectNames;
    internal QuickAddText Root;
    internal Grid Grid;
    internal TextEditor EditBox;
    internal Popup SelectionPopup;
    private bool _contentLoaded;

    public bool CanParseDate { get; set; } = true;

    public bool Focused => this.EditBox.TextArea.IsKeyboardFocused;

    public QuickAddText()
    {
      this.InitializeComponent();
      this.SetupEvents();
      this.SetupGenerators();
      this.InitShortCut();
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    public event EventHandler<double> CaretHorizontalOffsetChanged;

    public event EventHandler<string> TextChanged;

    public event EventHandler<string> TagSelected;

    public event EventHandler<string> EnterText;

    public event EventHandler<IPaserDueDate> DateParsed;

    public event EventHandler<QuickAddToken> TokenRemoved;

    public event EventHandler<bool> PopupOpenChanged;

    public event EventHandler<int> PrioritySelect;

    public event EventHandler<ProjectModel> ProjectSelect;

    public event EventHandler<DateTime> DateSelect;

    public event EventHandler<AvatarViewModel> AssigneeSelect;

    public event EventHandler<string> CalendarSelect;

    public event EventHandler MoveDown;

    public event EventHandler<KeyEventArgs> TextKeyDown;

    private void SetupGenerators()
    {
      this._tokenGenerator = new TokenGenerator(this);
      this._dateTokenGenerator = new DateTokenGenerator((IDateParseBox) this);
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) this._dateTokenGenerator);
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) this._tokenGenerator);
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new EmojiGenerator((IEmojiRender) this));
      this._tokenGenerator.TokenRemoved -= new EventHandler<QuickAddToken>(this.OnTokenRemoved);
      this._tokenGenerator.TokenRemoved += new EventHandler<QuickAddToken>(this.OnTokenRemoved);
      this.EditBox.TextArea.Caret.PositionChanged -= new EventHandler(this.NotifyCaretChanged);
      this.EditBox.TextArea.Caret.PositionChanged += new EventHandler(this.NotifyCaretChanged);
      this.EditBox.GotFocus += new RoutedEventHandler(this.OnGotFocus);
      this.EditBox.TextArea.TextView.LinkTextForegroundBrush = (Brush) ThemeUtil.GetColor("TextAccentColor", (FrameworkElement) this);
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this.EditBox.Text))
      {
        this.EditBox.GotFocus -= new RoutedEventHandler(this.OnGotFocus);
        AddTaskViewModel model = this.GetQuickAddView()?.Model;
        if (model != null && model.AutoAddTags)
        {
          this.InitTagTokens(model.Tags);
          this.GetQuickAddView()?.TryInitAvatar(model.Assignee);
          model.AutoAddTags = false;
        }
        this.EditBox.GotFocus += new RoutedEventHandler(this.OnGotFocus);
      }
      this._projectNames = new HashSet<string>((IEnumerable<string>) CacheManager.GetProjects().Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.name)).ToList<string>());
    }

    private void LogEmojiPosition()
    {
      this.EmojiDict.Clear();
      foreach (Match match in EmojiData.MatchOne2.Matches(this.EditBox.Text))
        this.EmojiDict[match.Index] = match.Value;
    }

    private void NotifyCaretChanged(object sender, EventArgs e)
    {
      double x = this.EditBox.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.EditBox.TextArea.Caret.Position.Line, Math.Max(1, this.EditBox.TextArea.Caret.Column)), VisualYPosition.LineTop).X;
      EventHandler<double> horizontalOffsetChanged = this.CaretHorizontalOffsetChanged;
      if (horizontalOffsetChanged == null)
        return;
      horizontalOffsetChanged((object) this, x);
    }

    private void OnAppThemeChanged(object sender, EventArgs e)
    {
      this.EditBox.TextArea.TextView.LinkTextForegroundBrush = (Brush) ThemeUtil.GetColor("TextAccentColor", (FrameworkElement) this);
      this.EditBox.TextArea.TextView.Redraw();
    }

    private void OnTokenRemoved(object sender, QuickAddToken token)
    {
      if (token.TokenType == TokenType.QuickDate)
        this._quickDate = string.Empty;
      if (!string.IsNullOrEmpty(this.EditBox.Text))
        this.ForceRender();
      EventHandler<QuickAddToken> tokenRemoved = this.TokenRemoved;
      if (tokenRemoved == null)
        return;
      tokenRemoved((object) this, token);
    }

    private async void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
      QuickAddText quickAddText1 = this;
      int time = 1;
      bool flag = true;
      while (flag)
      {
        try
        {
          if (e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
          {
            QuickAddText quickAddText = quickAddText1;
            string text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (text != null)
            {
              e.CancelCommand();
              if (Utils.ShowBatchAdd(text) && !quickAddText1.IsUsedInCalendar())
              {
                Utils.RunOnUiThread(quickAddText1.Dispatcher, (Action) (() => quickAddText.GetQuickAddView()?.ShowBatchAddWindow(text)));
              }
              else
              {
                ProjectTask extra = TaskUtils.ParseTaskUrl(text) ?? TaskUtils.ParseTaskUrlWithoutTitle(text);
                if (extra != null && !text.StartsWith("[") && !text.EndsWith(")"))
                {
                  string text1 = text.Replace(" " + extra.Title, string.Empty);
                  e.CancelCommand();
                  await quickAddText1.TryPasteLinkTask(extra, text1);
                  break;
                }
                string str = text.Replace("\n", " ").Replace("\r", " ");
                quickAddText1.InitTokens(str);
                int offset = quickAddText1.EditBox.CaretOffset;
                if (quickAddText1.EditBox.SelectionLength > 0)
                {
                  quickAddText1.EditBox.Document.Remove(quickAddText1.EditBox.SelectionStart, quickAddText1.EditBox.SelectionLength);
                  offset = quickAddText1.EditBox.SelectionStart;
                }
                quickAddText1.EditBox.Document.Insert(offset, str);
                quickAddText1.EditBox.CaretOffset = offset + str.Length;
              }
            }
          }
          flag = false;
        }
        catch (Exception ex)
        {
          flag = time++ < 10;
        }
      }
    }

    private async Task TryPasteLinkTask(ProjectTask extra, string text)
    {
      string str = extra.Title;
      if (string.IsNullOrEmpty(str))
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(extra.TaskId);
        str = thinTaskById == null || string.IsNullOrEmpty(thinTaskById.title) ? Utils.GetString("MyTask") : thinTaskById.title;
      }
      this.EditBox.Document.Insert(this.EditBox.CaretOffset, "[" + str + "](" + text + ")");
    }

    private void InitTokens(string content)
    {
      foreach (TagModel tagModel in CacheManager.GetTags().ToList<TagModel>().Where<TagModel>((Func<TagModel, bool>) (tag => content.ToLower().Contains("#" + tag.name))))
        this._tokenGenerator.TryAddToken(QuickAddToken.BuildTag("#" + tagModel.GetDisplayName()));
      foreach (ProjectModel e in CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (project => content.Contains("^" + project.name) || content.Contains("~" + project.name))))
      {
        this._tokenGenerator.TryAddToken(QuickAddToken.BuildProject("^" + e.name));
        this._tokenGenerator.TryAddToken(QuickAddToken.BuildProject("~" + e.name));
        EventHandler<ProjectModel> projectSelect = this.ProjectSelect;
        if (projectSelect != null)
          projectSelect((object) this, e);
      }
      Dictionary<string, int> dictionary = new Dictionary<string, int>()
      {
        {
          Utils.GetString("PriorityHigh"),
          5
        },
        {
          Utils.GetString("PriorityMedium"),
          3
        },
        {
          Utils.GetString("PriorityLow"),
          1
        },
        {
          Utils.GetString("PriorityNull"),
          0
        }
      };
      foreach (string key in dictionary.Keys.Where<string>((Func<string, bool>) (priority => content.Contains("!" + priority) || content.Contains("！" + priority))))
      {
        this._tokenGenerator.TryAddToken(QuickAddToken.BuildPriority("!" + key));
        this._tokenGenerator.TryAddToken(QuickAddToken.BuildPriority("！" + key));
        EventHandler<int> prioritySelect = this.PrioritySelect;
        if (prioritySelect != null)
          prioritySelect((object) this, dictionary[key]);
      }
      QuickAddView quickAddView = this.GetQuickAddView();
      List<AvatarViewModel> list = quickAddView != null ? quickAddView.GetAvatars().ToList<AvatarViewModel>() : (List<AvatarViewModel>) null;
      if (list == null || !list.Any<AvatarViewModel>())
        return;
      foreach (AvatarViewModel e in list.Where<AvatarViewModel>((Func<AvatarViewModel, bool>) (avatar => content.Contains("@" + avatar.Name))))
      {
        this._tokenGenerator.TryAddToken(QuickAddToken.BuildAssignee("@" + e.Name));
        EventHandler<AvatarViewModel> assigneeSelect = this.AssigneeSelect;
        if (assigneeSelect != null)
          assigneeSelect((object) this, e);
      }
    }

    private void SetPriorityTokens(string content)
    {
      if (this.IsUsedInCalendar())
        return;
      string str1 = content;
      if (!str1.Contains("!") && !str1.Contains("！"))
      {
        this._tokenGenerator.RemoveTokenByType(TokenType.Priority);
        this.GetQuickAddView()?.Model.ResetPriority();
      }
      else
      {
        if (this._tokenGenerator.ExistToken(TokenType.Priority) && this.EditBox.CaretOffset > 0 && content[Math.Max(str1.Length, this.EditBox.CaretOffset) - 1] != ' ')
          return;
        List<QuickAddToken> tokensByType = this._tokenGenerator.GetTokensByType(TokenType.Priority);
        if (tokensByType.Any<QuickAddToken>())
        {
          MatchCollection matchCollection = this._tokenGenerator.PriorityRegex.Matches(str1.Substring(0, Math.Max(str1.Length, this.EditBox.CaretOffset)));
          if (matchCollection.Count > 0)
          {
            string pri = matchCollection[matchCollection.Count - 1].ToString().Trim();
            if (!tokensByType.All<QuickAddToken>((Func<QuickAddToken, bool>) (tok => tok.Value.Trim() != pri)))
              return;
            this.OnTokenSelected(QuickAddToken.BuildPriority(pri), handleText: false);
            EventHandler<int> prioritySelect = this.PrioritySelect;
            if (prioritySelect == null)
              return;
            prioritySelect((object) this, TokenGenerator.GetPriorityByText(pri));
          }
          else
          {
            this._tokenGenerator.RemoveTokenByType(TokenType.Priority);
            this.GetQuickAddView()?.Model.ResetPriority();
          }
        }
        else
        {
          MatchCollection matchCollection = this._tokenGenerator.PriorityRegex.Matches(content);
          if (matchCollection.Count <= 0)
            return;
          string str2 = matchCollection[matchCollection.Count - 1].ToString().Trim();
          this.OnTokenSelected(QuickAddToken.BuildPriority(str2), handleText: false);
          EventHandler<int> prioritySelect = this.PrioritySelect;
          if (prioritySelect == null)
            return;
          prioritySelect((object) this, TokenGenerator.GetPriorityByText(str2));
        }
      }
    }

    private void SetTagTokens(string content)
    {
      if (this.IsUsedInCalendar())
        return;
      string str1 = content;
      string[] source = content.Split(' ');
      List<string> stringList = new List<string>();
      List<string> list = ((IEnumerable<string>) source).ToList<string>();
      if (list.Count > 0)
        list.Remove(list[list.Count - 1]);
      for (int index = 0; index < source.Length - 1; ++index)
      {
        string token = source[index];
        if (token.Length != 0 && !this._tokenGenerator.ExistRemoveToken(token))
        {
          string exp = token.Substring(1);
          string str2 = token.Substring(0, 1);
          if ((str2 == "#" || str2 == "＃") && NameUtils.IsValidName(exp, false) && !string.IsNullOrWhiteSpace(token))
            stringList.Add(token);
        }
      }
      this._tokenGenerator.HandleRemainText(list);
      foreach (string tag in stringList)
        this._tokenGenerator.TryAddTagToken(QuickAddToken.BuildTag(tag));
      if (str1 != content)
        this.EditBox.SetTextAndOffset(content, true);
      else
        this.EditBox.TextArea.TextView.Redraw();
    }

    public List<string> GetSelectedTags() => this._tokenGenerator.GetSelectedTags();

    public void InitTagTokens(List<string> tags)
    {
      List<QuickAddToken> quickAddTokenList = new List<QuickAddToken>();
      for (int index = 0; index < tags.Count; ++index)
      {
        string name = tags[index].ToLower();
        string str = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => name == t.name))?.GetDisplayName() ?? tags[index];
        tags[index] = str;
        quickAddTokenList.Add(QuickAddToken.BuildTag("#" + str.Replace("#", string.Empty)));
      }
      foreach (QuickAddToken token in quickAddTokenList)
        this._tokenGenerator.TryAddToken(token);
      this.EditBox.Text = tags.Aggregate<string, string>(string.Empty, (Func<string, string, string>) ((current, tag) => current + "#" + tag + " "));
      this.EditBox.CaretOffset = this.EditBox.Text.Length;
    }

    private void SetupEvents()
    {
      this.EditBox.TextChanged -= new EventHandler(this.EditBoxOnTextChanged);
      this.EditBox.TextChanged += new EventHandler(this.EditBoxOnTextChanged);
      DataObject.AddPastingHandler((DependencyObject) this.EditBox, new DataObjectPastingEventHandler(this.OnPaste));
    }

    private void BindEvents()
    {
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnAppThemeChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnAppThemeChanged);
      this._dateParserHandler.SetAction(new EventHandler(this.TryDateParser));
      KeyBindingManager.ShortCutChanged -= new EventHandler<string>(this.OnShortCutChanged);
      KeyBindingManager.ShortCutChanged += new EventHandler<string>(this.OnShortCutChanged);
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnAppThemeChanged);
      this._dateParserHandler.StopAndClear();
      KeyBindingManager.ShortCutChanged -= new EventHandler<string>(this.OnShortCutChanged);
    }

    private void TryDateParser(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.TryParseDate));
    }

    public void ForceRender()
    {
      this.EditBox.Document.BeginUpdate();
      this.EditBox.Document.Insert(0, " ");
      this.EditBox.Document.Remove(0, 1);
      this.EditBox.Document.EndUpdate();
    }

    public double GetFontSize() => this.EditBox.TextArea.FontSize;

    public string GetColor() => ((Color) this.FindResource((object) "ColorPrimary")).ToString();

    private bool IsUsedInCalendar()
    {
      QuickAddView quickAddView = this.GetQuickAddView();
      return quickAddView != null && quickAddView.Model.IsCalendar;
    }

    private void TryShowQuickPopup()
    {
      string content;
      string mark = this.TryGetMark(out content);
      this._filterContent = content;
      if (string.IsNullOrEmpty(mark))
      {
        this._selectionControl?.ClosePopup(false);
      }
      else
      {
        if (mark == "*")
        {
          List<string> validTokens = this._dateTokenGenerator.GetValidTokens();
          // ISSUE: explicit non-virtual call
          if ((validTokens != null ? (__nonvirtual (validTokens.Count) > 0 ? 1 : 0) : 0) != 0)
            return;
        }
        if (this._selectionControl == null)
        {
          this._selectionControl = new QuickSelectionControl(this.SelectionPopup);
          if (Window.GetWindow((DependencyObject) this) is WidgetWindow window)
            this._selectionControl.Resources = window.Resources;
          this._selectionControl.PopupOpenChanged += (EventHandler<bool>) ((o, e) =>
          {
            EventHandler<bool> popupOpenChanged = this.PopupOpenChanged;
            if (popupOpenChanged == null)
              return;
            popupOpenChanged((object) this, e);
          });
          this._selectionControl.ItemSelected += new EventHandler<QuickSetModel>(this.OnQuickItemSelect);
        }
        bool usedInCal = this.IsUsedInCalendar();
        string accountId = !usedInCal || !(mark == "^") && !(mark == "~") ? "" : this.GetQuickAddView()?.Model.AccountId;
        string projectId = !usedInCal && mark == "@" || mark == "^" || mark == "~" ? this.GetQuickAddView()?.Model.ProjectId : (string) null;
        int? nullable = mark == "!" || mark == "！" ? this.GetQuickAddView()?.Model.Priority : new int?(-1);
        List<string> tags = mark == "#" || mark == "＃" ? this.GetQuickAddView()?.Model.Tags : (List<string>) null;
        string userId = mark == "@" ? this.GetQuickAddView()?.Avatar?.UserId : (string) null;
        if (this._selectionControl.SetSelectionItems(mark, content, usedInCal, accountId, projectId, nullable ?? -1, tags, userId))
        {
          System.Windows.Point popupOffset = this.GetPopupOffset();
          this._selectionControl.TryShowPopup(mark, content, new System.Windows.Point(popupOffset.X - 12.0, -12.0));
          this.DelayFocus();
        }
        else
          this._selectionControl?.ClosePopup();
      }
    }

    private async void DelayFocus()
    {
      await Task.Delay(100);
      this.EditBox.Focus();
      Keyboard.Focus((IInputElement) this.EditBox.TextArea);
    }

    private void OnQuickItemSelect(object sender, QuickSetModel e)
    {
      this._canEnter = false;
      switch (e.Type)
      {
        case QuickSetType.Priority:
          this.OnTokenSelected(QuickAddToken.BuildPriority(this._selectionControl.Tag?.ToString() + e.Title));
          UserActCollectUtils.AddShortCutEvent("quick_add", "set_priority");
          EventHandler<int> prioritySelect = this.PrioritySelect;
          if (prioritySelect == null)
            break;
          prioritySelect((object) this, e.Priority);
          break;
        case QuickSetType.Project:
          this.OnTokenSelected(QuickAddToken.BuildProject(this._selectionControl.Tag?.ToString() + e.Project.name));
          UserActCollectUtils.AddShortCutEvent("quick_add", "add_to_list");
          EventHandler<ProjectModel> projectSelect = this.ProjectSelect;
          if (projectSelect == null)
            break;
          projectSelect((object) this, e.Project);
          break;
        case QuickSetType.Tag:
          this.EditBox.TextChanged -= new EventHandler(this.EditBoxOnTextChanged);
          this.OnTagTokenSelected(QuickAddToken.BuildTag("#" + e.Tag));
          UserActCollectUtils.AddShortCutEvent("quick_add", "set_tag");
          this.EditBox.TextChanged += new EventHandler(this.EditBoxOnTextChanged);
          break;
        case QuickSetType.Date:
          if (e.Date.HasValue)
          {
            this._quickDate = e.Title;
            this.OnTokenSelected(QuickAddToken.BuildQuickDate(this._selectionControl.Tag?.ToString() + e.Title));
            EventHandler<DateTime> dateSelect = this.DateSelect;
            if (dateSelect != null)
              dateSelect((object) this, e.Date.Value);
          }
          UserActCollectUtils.AddShortCutEvent("quick_add", "set_due_date ");
          break;
        case QuickSetType.Assign:
          this.OnTokenSelected(QuickAddToken.BuildAssignee(this._selectionControl.Tag?.ToString() + e.Title));
          EventHandler<AvatarViewModel> assigneeSelect = this.AssigneeSelect;
          if (assigneeSelect != null)
            assigneeSelect((object) this, e.Avatar);
          UserActCollectUtils.AddShortCutEvent("quick_add", "assign_to_member ");
          break;
        case QuickSetType.CalId:
          this.OnTokenSelected(QuickAddToken.BuildProject(this._selectionControl.Tag?.ToString() + e.Title));
          EventHandler<string> calendarSelect = this.CalendarSelect;
          if (calendarSelect == null)
            break;
          calendarSelect((object) this, e.CalId);
          break;
      }
    }

    public async Task FocusText(bool focusEnd = false)
    {
      await Task.Delay(50);
      this.EditBox.Focus();
      this.EditBox.TextArea.Focus();
      if (!focusEnd)
        return;
      this.EditBox.CaretOffset = this.EditBox.Text.Length;
    }

    public void ResetTokens()
    {
      this._dateTokenGenerator?.Reset();
      this._tokenGenerator?.Reset();
      this._quickDate = string.Empty;
    }

    private QuickAddView GetQuickAddView()
    {
      return Utils.FindParent<QuickAddView>((DependencyObject) this);
    }

    private QuickAddWindow GetQuickAddWindow()
    {
      return Utils.FindParent<QuickAddWindow>((DependencyObject) this);
    }

    private void OnTagTokenSelected(QuickAddToken token)
    {
      if (this._tokenGenerator.ExistToken(token))
        return;
      this._tokenGenerator.TryAddToken(token);
      int offset = this.EditBox.CaretOffset;
      if (!string.IsNullOrEmpty(this._filterContent))
      {
        string text = this.EditBox.Text;
        string str1 = this.EditBox.Text;
        int num = this.EditBox.Text.IndexOf("#" + this._filterContent, this._startIndex, StringComparison.Ordinal);
        if (num >= 0)
        {
          string str2 = text.Substring(0, num + 1);
          string str3 = text.Substring(num + 1 + this._filterContent.Length);
          str1 = str2 + str3;
          offset = str2.Length;
        }
        this.EditBox.Text = !string.IsNullOrEmpty(this._filterContent) ? str1 : this.EditBox.Text.Substring(1);
      }
      string text1 = token.Value.Trim().Substring(1) + " ";
      this.EditBox.Document.Insert(offset, text1);
      this.EditBox.Focus();
      this.EditBox.TextArea.Focus();
      this.EditBox.CaretOffset = offset + text1.Length;
      EventHandler<string> tagSelected = this.TagSelected;
      if (tagSelected == null)
        return;
      tagSelected((object) this, token.Value);
    }

    private string TryGetMark(out string content)
    {
      if (!string.IsNullOrEmpty(this.EditBox.Text))
      {
        string input = this.EditBox.Text.Substring(0, this.EditBox.CaretOffset);
        MatchCollection matchCollection = new Regex("(?=[#|@|~|^|!|！|\\*])").Matches(input);
        if (matchCollection.Count > 0)
        {
          Match match = matchCollection[matchCollection.Count - 1];
          if (input.Length > match.Index)
          {
            string mark = input[match.Index].ToString();
            content = input.Substring(match.Index + 1, input.Length - match.Index - 1);
            if (mark == "~" || mark == "^" || mark == "@" || !content.Contains(" "))
            {
              this._startIndex = match.Index;
              return mark;
            }
          }
        }
      }
      content = string.Empty;
      this._startIndex = -1;
      return string.Empty;
    }

    public void OnTokenSelected(QuickAddToken token, bool withMark = false, bool handleText = true)
    {
      this.EditBox.TextChanged -= new EventHandler(this.EditBoxOnTextChanged);
      string str1 = token.Value.Substring(0, 1);
      if (!string.IsNullOrEmpty(this._filterContent))
      {
        if (this._filterContent.StartsWith(str1))
          this._filterContent = this._filterContent.Substring(1);
        string text = this.EditBox.Text;
        string str2 = this.EditBox.Text;
        int num = this.EditBox.Text.IndexOf(str1 + this._filterContent, Math.Max(0, this._startIndex), StringComparison.Ordinal);
        if (num >= 0)
          str2 = text.Substring(0, num + 1) + text.Substring(num + 1 + this._filterContent.Length);
        this.EditBox.Text = !string.IsNullOrEmpty(this._filterContent) ? str2 : this.EditBox.Text.Substring(1);
      }
      QuickAddToken quickAddToken = this._tokenGenerator.TryAddToken(token);
      if (quickAddToken != null && quickAddToken.Value != token.Value)
      {
        int offset = this.EditBox.Text.IndexOf(quickAddToken.Value, StringComparison.Ordinal);
        if (offset >= 0)
        {
          int length = Math.Min(quickAddToken.Value.Length + 1, this.EditBox.Text.Length - offset);
          this.EditBox.Document.Remove(offset, length);
          if (this._startIndex > 0)
            this._startIndex -= length;
        }
      }
      if (handleText)
      {
        this.EditBox.Document.Insert(this.EditBox.Text.IndexOf(str1, Math.Max(0, this._startIndex), StringComparison.Ordinal) + 1, (withMark ? token.Value.Trim() : token.Value.Trim().Substring(1)) + " ");
        if (!this.EditBox.TextArea.IsKeyboardFocused)
        {
          this.EditBox.Focus();
          this.EditBox.TextArea.Focus();
        }
        this.EditBox.CaretOffset = this.EditBox.Text.Length;
      }
      this._selectionControl?.ClosePopup(false);
      this.EditBox.TextChanged += new EventHandler(this.EditBoxOnTextChanged);
      this._startIndex = -1;
    }

    private System.Windows.Point GetPopupOffset()
    {
      return this.EditBox.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.EditBox.Document.GetLocation(this.EditBox.CaretOffset)), VisualYPosition.LineBottom);
    }

    private async void EditBoxOnTextChanged(object sender, EventArgs e)
    {
      int? length = this.EditBox.Text?.Length;
      int? nullable = this._origin?.Length;
      if (length.GetValueOrDefault() > nullable.GetValueOrDefault() & length.HasValue & nullable.HasValue)
        this._inputByKeyBoard = (DateTime.Now - this._lastKeyDownTime).TotalSeconds < 2.0;
      this._dateTokenGenerator.CheckIgnoreToken(this.EditBox.Text);
      EventHandler<string> textChanged = this.TextChanged;
      if (textChanged != null)
        textChanged(sender, this.EditBox.Text);
      DelayActionHandler dateParserHandler = this._dateParserHandler;
      nullable = new int?();
      int? interval = nullable;
      dateParserHandler.TryDoAction(interval);
      this.LogEmojiPosition();
      this.TryShowQuickPopup();
      this.SetTagTokens(this.EditBox.Text);
      this.SetPriorityTokens(this.EditBox.Text);
      if (string.IsNullOrEmpty(this.EditBox.Text))
        this.GetQuickAddView()?.ResetHint();
      this._origin = this.EditBox.Text;
    }

    private void TryParseDate()
    {
      IPaserDueDate date = this.ParseDate();
      if (date == null)
        return;
      EventHandler<IPaserDueDate> dateParsed = this.DateParsed;
      if (dateParsed == null)
        return;
      dateParsed((object) this, date);
    }

    private IPaserDueDate ParseDate()
    {
      QuickSelectionControl selectionControl = this._selectionControl;
      if ((selectionControl != null ? (selectionControl.PopupOpened() ? 1 : 0) : 0) != 0)
        return (IPaserDueDate) null;
      QuickAddView quickAddView = this.GetQuickAddView();
      if (quickAddView == null)
        return (IPaserDueDate) null;
      if (!LocalSettings.Settings.DateParsing || quickAddView.ManualSelectedDate)
        return (IPaserDueDate) null;
      string str1 = this.EditBox.Text;
      if (this._dateTokenGenerator.GetIgnoreTokens().Any<string>())
        str1 = this._dateTokenGenerator.GetIgnoreTokens().Aggregate<string, string>(str1, (Func<string, string, string>) ((current, token) => current.Replace(token, string.Empty)));
      if (this._tokenGenerator.GetTokens().Any<QuickAddToken>())
        str1 = this._tokenGenerator.GetTokens().Aggregate<QuickAddToken, string>(str1, (Func<string, QuickAddToken, string>) ((current, token) => current.Replace(token.Value, string.Empty)));
      DateTime? startDate = quickAddView.Model.OriginalTimeData.StartDate;
      IPaserDueDate date = ticktick_WPF.Util.DateParser.DateParser.Parse(str1, new DateTime?());
      if (date != null && date.GetRecognizeStrings().Any<string>())
      {
        List<string> recognizeStrings = date.GetRecognizeStrings();
        List<string> tokens = (recognizeStrings != null ? recognizeStrings.Select<string, string>((Func<string, string>) (str => str.TrimEnd())).ToList<string>() : (List<string>) null) ?? new List<string>();
        tokens.RemoveAll((Predicate<string>) (t => t.EndsWith("：")));
        this._dateTokenGenerator.AddTokens((IEnumerable<string>) tokens, str1);
      }
      else
        this._dateTokenGenerator.ClearTokens();
      this.EditBox.TextArea.TextView.Redraw();
      return date;
    }

    private void OnEditTextKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Back)
        this._lastKeyDownTime = DateTime.Now;
      switch (e.Key)
      {
        case Key.Back:
          using (List<string>.Enumerator enumerator = this._dateTokenGenerator.GetValidTokens().GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              string current = enumerator.Current;
              int num = this.EditBox.Text.LastIndexOf(current, this.EditBox.CaretOffset, StringComparison.Ordinal);
              if (num >= 0 && num == this.EditBox.CaretOffset - current.Length)
              {
                foreach (QuickAddToken token in this._tokenGenerator.GetTokens())
                {
                  if (token.Value.EndsWith(current) && this.EditBox.Text.LastIndexOf(token.Value, this.EditBox.CaretOffset, StringComparison.Ordinal) == this.EditBox.CaretOffset - token.Value.Length)
                    return;
                }
                this._dateTokenGenerator.AddIgnoreToken(current);
                this.EditBox.TextArea.TextView.Redraw();
                if (this._inputByKeyBoard)
                  e.Handled = true;
              }
            }
            break;
          }
        case Key.Tab:
          e.Handled = true;
          QuickSelectionControl selectionControl1 = this._selectionControl;
          if ((selectionControl1 != null ? (selectionControl1.IsOpen() ? 1 : 0) : 0) != 0)
          {
            this._selectionControl?.TryMovePopupItem(Utils.IfShiftPressed());
            return;
          }
          break;
        case Key.Return:
          e.Handled = true;
          QuickSelectionControl selectionControl2 = this._selectionControl;
          if ((selectionControl2 != null ? (selectionControl2.IsOpen() ? 1 : 0) : 0) != 0)
          {
            this._selectionControl?.TrySelectItem();
            e.Handled = true;
            break;
          }
          break;
        case Key.Escape:
          QuickSelectionControl selectionControl3 = this._selectionControl;
          if (selectionControl3 != null)
          {
            selectionControl3.ClosePopup(false);
            break;
          }
          break;
        case Key.Up:
          QuickSelectionControl selectionControl4 = this._selectionControl;
          if ((selectionControl4 != null ? (selectionControl4.IsOpen() ? 1 : 0) : 0) != 0)
          {
            this._selectionControl?.TryMovePopupItem(true);
            e.Handled = true;
            break;
          }
          break;
        case Key.Down:
          QuickSelectionControl selectionControl5 = this._selectionControl;
          if ((selectionControl5 != null ? (selectionControl5.IsOpen() ? 1 : 0) : 0) != 0)
          {
            this._selectionControl?.TryMovePopupItem(false);
            e.Handled = true;
            break;
          }
          if (e.ImeProcessedKey == Key.None && !this.GetQuickAddView().IsInOperation)
          {
            this._canMoveDown = true;
            break;
          }
          break;
        case Key.D:
          if (Keyboard.Modifiers == ModifierKeys.Control && LocalSettings.Settings.ShortCutModel.SetDate == "Ctrl+D" && e.Key == Key.D)
          {
            e.Handled = true;
            QuickAddView quickAddView = this.GetQuickAddView();
            if (quickAddView != null)
            {
              quickAddView.SelectDate();
              break;
            }
            break;
          }
          break;
        case Key.ImeProcessed:
          this._canEnter = false;
          break;
      }
      EventHandler<KeyEventArgs> textKeyDown = this.TextKeyDown;
      if (textKeyDown == null)
        return;
      textKeyDown((object) this, e);
    }

    private void OnEditTextKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (this._canEnter)
          {
            this.NotifyEnterText();
            e.Handled = true;
            break;
          }
          break;
        case Key.Down:
          if (this._canMoveDown)
          {
            EventHandler moveDown = this.MoveDown;
            if (moveDown != null)
            {
              moveDown((object) this, (EventArgs) null);
              break;
            }
            break;
          }
          break;
      }
      this._canEnter = true;
      this._canMoveDown = false;
    }

    public string GetTitleWithOutToken()
    {
      string seed = this.EditBox.Text.Replace("\r\n", string.Empty);
      if (!string.IsNullOrEmpty(seed))
      {
        List<QuickAddToken> list = this._tokenGenerator.GetTokens().Where<QuickAddToken>((Func<QuickAddToken, bool>) (token => token.TokenType != TokenType.QuickDate)).ToList<QuickAddToken>();
        if (list.Any<QuickAddToken>())
          seed = list.Aggregate<QuickAddToken, string>(seed, (Func<string, QuickAddToken, string>) ((current, token) => current.Replace(token.Value, string.Empty)));
      }
      return seed;
    }

    public string GetTitleContent(bool forceRemove = false)
    {
      string seed = this.EditBox.Text.Replace("\r\n", string.Empty);
      if (!string.IsNullOrEmpty(seed))
      {
        List<QuickAddToken> list1 = this._tokenGenerator.GetTokens().Where<QuickAddToken>((Func<QuickAddToken, bool>) (token => token.TokenType != 0)).ToList<QuickAddToken>();
        if (list1.Any<QuickAddToken>())
          seed = list1.Aggregate<QuickAddToken, string>(seed, (Func<string, QuickAddToken, string>) ((current, token) => current.Replace(token.Value, string.Empty)));
        List<QuickAddToken> list2 = this._tokenGenerator.GetTokens().Where<QuickAddToken>((Func<QuickAddToken, bool>) (token => token.TokenType == TokenType.Tag)).ToList<QuickAddToken>();
        if (list2.Any<QuickAddToken>() && !LocalSettings.Settings.KeepTagsInText | forceRemove)
          seed = list2.Aggregate<QuickAddToken, string>(seed, (Func<string, QuickAddToken, string>) ((current, token) => current.Replace(token.Value, string.Empty)));
        List<string> validTokens = this._dateTokenGenerator.GetValidTokens();
        if (validTokens.Any<string>() && LocalSettings.Settings.RemoveTimeText | forceRemove)
        {
          foreach (string str in validTokens)
          {
            int startIndex = seed.IndexOf(str, StringComparison.Ordinal);
            if (startIndex >= 0 && startIndex + str.Length <= seed.Length)
              seed = seed.Remove(startIndex, str.Length);
          }
        }
      }
      return seed;
    }

    private void NotifyEnterText()
    {
      this.ParseDateImmediately();
      string titleContent = this.GetTitleContent();
      EventHandler<string> enterText = this.EnterText;
      if (enterText == null)
        return;
      enterText((object) this, titleContent);
    }

    private void AddTag(string tag)
    {
      QuickAddToken token = QuickAddToken.BuildTag("#" + TagDataHelper.GetTagDisplayName(tag));
      if (!this._tokenGenerator.TryAddTagToken(token))
        return;
      this.EditBox.Text = token.Value + " " + this.EditBox.Text;
    }

    public void SetPriority(int priority) => this.GetQuickAddView()?.SetPriority(priority);

    public void SetDate(string date) => this.GetQuickAddView()?.SetDate(date);

    public void SelectDate() => this.GetQuickAddView()?.SelectDate();

    public void ClearDate() => this.GetQuickAddView()?.ClearDate();

    public void OnTagsAdded(TagSelectData data)
    {
      string str = this.EditBox.Text;
      foreach (string omniSelectTag in data.OmniSelectTags)
      {
        QuickAddToken token = QuickAddToken.BuildTag("#" + TagDataHelper.GetTagDisplayName(omniSelectTag));
        if (this._tokenGenerator.TryAddTagToken(token))
          str = token.Value + " " + str;
      }
      foreach (QuickAddToken tagToken in this._tokenGenerator.GetTagTokens())
      {
        if (!data.OmniSelectTags.Contains(tagToken.Value.Substring(1).ToLower()))
        {
          this._tokenGenerator.RemoveToken(tagToken, false, false);
          str = str.Replace(tagToken.Value + " ", "");
        }
      }
      this.EditBox.Text = str;
      this.EditBox.CaretOffset = str.Length;
    }

    public void RemoveTokenByType(TokenType type, bool removeText = true)
    {
      List<QuickAddToken> quickAddTokenList = this._tokenGenerator.RemoveTokenByType(type);
      if (removeText)
      {
        string text = this.EditBox.Text.Replace("\r\n", string.Empty);
        foreach (QuickAddToken quickAddToken in quickAddTokenList)
        {
          string oldValue = quickAddToken.Value;
          if (text.Contains(oldValue + " "))
            text = text.Replace(oldValue + " ", "");
          else if (text.Contains(oldValue))
            text = text.Replace(oldValue, "");
        }
        if (this.EditBox.Text != text)
          this.EditBox.SetTextAndOffset(text, true);
      }
      this.ForceRender();
    }

    private void EditorMenuOnContextMenuOpening(object sender, ContextMenuEventArgs context)
    {
      this.AddEditorContextMenu(context);
    }

    public void AddEditorContextMenu(ContextMenuEventArgs context)
    {
      ContextMenu contextMenu = new ContextMenu();
      ItemCollection items1 = contextMenu.Items;
      MenuItem newItem1 = new MenuItem();
      newItem1.Header = (object) Utils.GetString("Cut");
      newItem1.Command = (ICommand) ApplicationCommands.Cut;
      newItem1.InputGestureText = "Ctrl+X";
      items1.Add((object) newItem1);
      ItemCollection items2 = contextMenu.Items;
      MenuItem newItem2 = new MenuItem();
      newItem2.Header = (object) Utils.GetString("Copy");
      newItem2.Command = (ICommand) ApplicationCommands.Copy;
      newItem2.InputGestureText = "Ctrl+C";
      items2.Add((object) newItem2);
      ItemCollection items3 = contextMenu.Items;
      MenuItem newItem3 = new MenuItem();
      newItem3.Header = (object) Utils.GetString("Paste");
      newItem3.Command = (ICommand) ApplicationCommands.Paste;
      newItem3.InputGestureText = "Ctrl+V";
      items3.Add((object) newItem3);
      contextMenu.Items.Add((object) new Separator());
      ItemCollection items4 = contextMenu.Items;
      MenuItem newItem4 = new MenuItem();
      newItem4.Header = (object) Utils.GetString("Undo");
      newItem4.Command = (ICommand) ApplicationCommands.Undo;
      newItem4.InputGestureText = "Ctrl+Z";
      items4.Add((object) newItem4);
      ItemCollection items5 = contextMenu.Items;
      MenuItem newItem5 = new MenuItem();
      newItem5.Header = (object) Utils.GetString("Redo");
      newItem5.Command = (ICommand) ApplicationCommands.Redo;
      newItem5.InputGestureText = "Ctrl+Y";
      items5.Add((object) newItem5);
      ((FrameworkElement) context.Source).ContextMenu = contextMenu;
    }

    private void InitShortCut()
    {
      if (this.EditBox.InputBindings.Count < 9)
        return;
      KeyBindingManager.SetKeyGesture("ClearDate", this.EditBox.InputBindings[0] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetToday", this.EditBox.InputBindings[1] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetTomorrow", this.EditBox.InputBindings[2] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetNextWeek", this.EditBox.InputBindings[3] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetDate", this.EditBox.InputBindings[4] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetNoPriority", this.EditBox.InputBindings[5] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetLowPriority", this.EditBox.InputBindings[6] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetMediumPriority", this.EditBox.InputBindings[7] as KeyBinding);
      KeyBindingManager.SetKeyGesture("SetHighPriority", this.EditBox.InputBindings[8] as KeyBinding);
    }

    private void OnShortCutChanged(object sender, string key)
    {
      if (key == null)
        return;
      switch (key.Length)
      {
        case 7:
          if (!(key == "SetDate"))
            break;
          KeyBindingManager.SetKeyGesture("SetDate", this.EditBox.InputBindings[4] as KeyBinding);
          break;
        case 8:
          if (!(key == "SetToday"))
            break;
          KeyBindingManager.SetKeyGesture("SetToday", this.EditBox.InputBindings[1] as KeyBinding);
          break;
        case 9:
          if (!(key == "ClearDate"))
            break;
          KeyBindingManager.SetKeyGesture("ClearDate", this.EditBox.InputBindings[0] as KeyBinding);
          break;
        case 11:
          switch (key[3])
          {
            case 'N':
              if (!(key == "SetNextWeek"))
                return;
              KeyBindingManager.SetKeyGesture("SetNextWeek", this.EditBox.InputBindings[3] as KeyBinding);
              return;
            case 'T':
              if (!(key == "SetTomorrow"))
                return;
              KeyBindingManager.SetKeyGesture("SetTomorrow", this.EditBox.InputBindings[2] as KeyBinding);
              return;
            default:
              return;
          }
        case 13:
          if (!(key == "SetNoPriority"))
            break;
          KeyBindingManager.SetKeyGesture("SetNoPriority", this.EditBox.InputBindings[5] as KeyBinding);
          break;
        case 14:
          if (!(key == "SetLowPriority"))
            break;
          KeyBindingManager.SetKeyGesture("SetLowPriority", this.EditBox.InputBindings[6] as KeyBinding);
          break;
        case 15:
          if (!(key == "SetHighPriority"))
            break;
          KeyBindingManager.SetKeyGesture("SetHighPriority", this.EditBox.InputBindings[8] as KeyBinding);
          break;
        case 17:
          if (!(key == "SetMediumPriority"))
            break;
          KeyBindingManager.SetKeyGesture("SetMediumPriority", this.EditBox.InputBindings[7] as KeyBinding);
          break;
      }
    }

    private void ParseDateImmediately()
    {
      this._dateParserHandler.CancelAction();
      this.TryParseDate();
    }

    public void RemoveTokenText(ref string text)
    {
      foreach (QuickAddToken token in this._tokenGenerator.GetTokens())
        text = text.Replace(token.Value, "");
    }

    public void ClearHighlightDate()
    {
      this._dateTokenGenerator.Reset();
      this._quickDate = string.Empty;
      this.EditBox.TextArea.TextView.Redraw();
    }

    public Dictionary<int, string> GetEmojiDict() => this.EmojiDict;

    public void RemoveTagTokenText()
    {
      string str = this.EditBox.Text;
      foreach (QuickAddToken token in this._tokenGenerator.GetTokens())
      {
        if (token.TokenType == TokenType.Tag)
          str = str.Replace(token.Value, "");
      }
      this.EditBox.Text = str;
    }

    public bool ExistToken(TokenType type) => this._tokenGenerator.ExistToken(type);

    public int? GetPriorityInTokens()
    {
      List<QuickAddToken> tokensByType = this._tokenGenerator.GetTokensByType(TokenType.Priority);
      return tokensByType.Count > 0 ? new int?(TokenGenerator.GetPriorityByText(tokensByType[0].Value)) : new int?();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/quickadd/quickaddtext.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (QuickAddText) target;
          this.Root.ContextMenuOpening += new ContextMenuEventHandler(this.EditorMenuOnContextMenuOpening);
          break;
        case 2:
          this.Grid = (Grid) target;
          break;
        case 3:
          this.EditBox = (TextEditor) target;
          this.EditBox.PreviewKeyDown += new KeyEventHandler(this.OnEditTextKeyDown);
          this.EditBox.PreviewKeyUp += new KeyEventHandler(this.OnEditTextKeyUp);
          break;
        case 4:
          this.SelectionPopup = (Popup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
