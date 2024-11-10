// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.LinkTextBox
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.TaskList.Item;

#nullable disable
namespace ticktick_WPF.Views
{
  public class LinkTextBox : UserControl, IComponentConnector
  {
    public new static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof (Foreground), typeof (Brush), typeof (LinkTextBox), new PropertyMetadata((object) new SolidColorBrush(Colors.Black), new PropertyChangedCallback(LinkTextBox.OnForegroundChangedCallback)));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof (Text), typeof (string), typeof (LinkTextBox), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(LinkTextBox.OnTextChangedCallback)));
    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof (TextWrapping), typeof (TextWrapping), typeof (LinkTextBox), new PropertyMetadata((object) TextWrapping.NoWrap, new PropertyChangedCallback(LinkTextBox.OnTextWrappingChangedCallback)));
    public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(nameof (AcceptsReturn), typeof (bool), typeof (LinkTextBox), new PropertyMetadata((object) false, new PropertyChangedCallback(LinkTextBox.OnAcceptsReturnChangedCallback)));
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof (IsReadOnly), typeof (bool), typeof (LinkTextBox), new PropertyMetadata((object) false, new PropertyChangedCallback(LinkTextBox.OnIsReadOnlyChangedCallback)));
    private bool _canMerge = true;
    private bool _canSplit;
    private int _currentCaretLine;
    private DateTime _lastTagAddTime = DateTime.Now;
    private DateTime _lastTextChangeTime = DateTime.Now;
    private bool _originalAcceptReturn;
    private bool _rightClick;
    private TagSelectAddWindow _tagPopup;
    private ListItemContent _taskParent;
    private int _tagStartIndex = -1;
    private bool _textKeyPressed;
    private IQuickInput _quickInputItems;
    private int _startIndex = -1;
    internal LinkTextBox Root;
    internal TextBlock TextBlock;
    internal TextBox TextBox;
    internal Popup SelectionPopup;
    internal Grid ItemsContainer;
    private bool _contentLoaded;

    public LinkTextBox()
    {
      this.InitializeComponent();
      this.InitEvents();
    }

    public new bool IsFocused => this.TextBox.IsFocused;

    public Window Context { private get; set; } = (Window) App.Window;

    public double LineHeightScale { get; set; } = 1.5;

    protected double TagPopupVerticalOffset { get; set; } = 4.0;

    public string Text
    {
      get => this.TextBox.Text;
      set
      {
        this.TextBox.TextChanged -= new TextChangedEventHandler(this.NotifyTextChanged);
        this.TextBox.Text = value;
        this.TextBox.TextChanged += new TextChangedEventHandler(this.NotifyTextChanged);
      }
    }

    public TextWrapping TextWrapping
    {
      get => (TextWrapping) this.GetValue(LinkTextBox.TextWrappingProperty);
      set => this.SetValue(LinkTextBox.TextWrappingProperty, (object) value);
    }

    public bool AcceptsReturn
    {
      private get => (bool) this.GetValue(LinkTextBox.AcceptsReturnProperty);
      set => this.SetValue(LinkTextBox.AcceptsReturnProperty, (object) value);
    }

    public bool IsReadOnly
    {
      get => (bool) this.GetValue(LinkTextBox.IsReadOnlyProperty);
      set => this.SetValue(LinkTextBox.IsReadOnlyProperty, (object) value);
    }

    public new Brush Foreground
    {
      get => (Brush) this.GetValue(LinkTextBox.ForegroundProperty);
      set => this.SetValue(LinkTextBox.ForegroundProperty, (object) value);
    }

    public int CaretIndex => this.TextBox.CaretIndex;

    public event EventHandler<string> TextChanged;

    public event EventHandler TextGotFocus;

    public event EventHandler TextLostFocus;

    public event EventHandler<string> MergeText;

    public event EventHandler<SplitData> SplitText;

    public event EventHandler MoveUp;

    public event EventHandler MoveDown;

    public event EventHandler<int> SelectionChanged;

    public event EventHandler<string> TagSelected;

    public event EventHandler TagPopOpened;

    public event EventHandler TagPopClosed;

    public event EventHandler<ProjectTask> Navigate;

    public void SetMaxLength(int maxLength) => this.TextBox.MaxLength = maxLength;

    private static void OnTextWrappingChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is LinkTextBox linkTextBox) || e.NewValue == null)
        return;
      linkTextBox.TextBox.TextWrapping = (TextWrapping) e.NewValue;
      linkTextBox.TextBlock.TextWrapping = (TextWrapping) e.NewValue;
    }

    private static void OnForegroundChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is LinkTextBox linkTextBox) || e.NewValue == null)
        return;
      linkTextBox.TextBox.Foreground = (Brush) e.NewValue;
      linkTextBox.TextBlock.Foreground = (Brush) e.NewValue;
    }

    private static void OnAcceptsReturnChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is LinkTextBox linkTextBox) || e.NewValue == null)
        return;
      linkTextBox.TextBox.AcceptsReturn = (bool) e.NewValue;
    }

    private static void OnIsReadOnlyChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is LinkTextBox linkTextBox) || e.NewValue == null)
        return;
      linkTextBox.TextBox.IsReadOnly = (bool) e.NewValue;
    }

    private static void OnTextChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is LinkTextBox linkTextBox) || e.NewValue == null)
        return;
      linkTextBox.TextBox.Text = e.NewValue.ToString();
    }

    private void InitEvents()
    {
      this.TextBox.SelectionChanged += (RoutedEventHandler) ((sender, args) =>
      {
        EventHandler<int> selectionChanged = this.SelectionChanged;
        if (selectionChanged == null)
          return;
        selectionChanged((object) this, this.TextBox.CaretIndex);
      });
      this.TextBox.TextChanged += new TextChangedEventHandler(this.RenderLinks);
      this.TextBox.TextChanged -= new TextChangedEventHandler(this.NotifyTextChanged);
      this.TextBox.TextChanged += new TextChangedEventHandler(this.NotifyTextChanged);
    }

    public void FocusText(int caretIndex = -1)
    {
      this.TextBlock.Visibility = Visibility.Hidden;
      this.TextBox.Visibility = Visibility.Visible;
      this.TextBox.Focus();
      this.TextBox.CaretIndex = caretIndex == -1 ? this.TextBox.Text.Length : caretIndex;
    }

    private async void OnTextBlockClick(object sender, MouseButtonEventArgs e)
    {
      this.TextBlock.Visibility = Visibility.Hidden;
      this.TextBox.Visibility = Visibility.Visible;
      await Task.Delay(10);
      this.TextBox.Focus();
      try
      {
        if (this.TextBox.GetCharacterIndexFromPoint(Mouse.GetPosition((IInputElement) this.TextBox), true) <= 0)
          return;
        this.TextBox.CaretIndex = this.TextBox.GetCharacterIndexFromPoint(Mouse.GetPosition((IInputElement) this.TextBox), true) + 1;
      }
      catch (Exception ex)
      {
      }
    }

    private void OnLeftClick(object sender, MouseButtonEventArgs e) => this._rightClick = false;

    private void OnRightClick(object sender, MouseButtonEventArgs e) => this._rightClick = true;

    private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
    {
      if (this._rightClick)
        return;
      EventHandler textGotFocus = this.TextGotFocus;
      if (textGotFocus == null)
        return;
      textGotFocus((object) this, (EventArgs) null);
    }

    private async void OnTextBoxSelectionChanged(object sender, RoutedEventArgs e)
    {
      await Task.Delay(150);
      this._currentCaretLine = this.TextBox.GetLineIndexFromCharacterIndex(this.TextBox.SelectionStart);
    }

    private async void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
      LinkTextBox sender1 = this;
      EventHandler textLostFocus = sender1.TextLostFocus;
      if (textLostFocus != null)
        textLostFocus((object) sender1, (EventArgs) null);
      if (sender1.TextBlock.Visibility == Visibility.Visible)
        return;
      sender1.RenderLinks();
    }

    private void NotifyTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!this.TextBox.IsFocused)
      {
        ICollection<TextChange> changes = e.Changes;
        if ((changes != null ? (changes.Count > 0 ? 1 : 0) : 0) == 0)
          return;
      }
      this._lastTextChangeTime = DateTime.Now;
      if (e.Changes != null && e.Changes.Count == 1)
      {
        foreach (TextChange change in (IEnumerable<TextChange>) e.Changes)
        {
          if (change.RemovedLength == 1 && this.TextBox.CaretIndex == 0)
            this._canMerge = false;
        }
      }
      EventHandler<string> textChanged = this.TextChanged;
      if (textChanged == null)
        return;
      textChanged((object) this, this.TextBox.Text);
    }

    private async void RenderLinks(object sender, TextChangedEventArgs textChange)
    {
      this.TextBox.Visibility = Visibility.Visible;
      if (this._quickInputItems != null)
      {
        if (this.TextBox.CaretIndex < this._startIndex || this.MarkChanged())
          this.ClearSelection();
        else
          this.SelectionPopup.IsOpen = this._quickInputItems.Filter(this.TextBox.Text.Substring(this._startIndex, this.TextBox.CaretIndex - this._startIndex));
      }
      if (this.TextBox.IsFocused && Utils.CheckIfAddText(textChange))
      {
        await Task.Delay(20);
        string sentileInput = this.GetSentileInput(textChange);
        if (this.TextBox.Text == "#" || sentileInput == " #" || sentileInput == "\n#" || this.TextBox.Text == "＃" || sentileInput == " ＃" || sentileInput == "\n＃")
        {
          this.ShowTagPopup();
          return;
        }
        if (this.GetParent() != null)
        {
          string mark = sentileInput.Length > 1 ? sentileInput.Substring(sentileInput.Length - 1, 1) : this.TextBox.Text;
          if (this.TextBox.Text == "!" || sentileInput == " !" || sentileInput == "\n!" || this.TextBox.Text == "！" || sentileInput == " ！" || sentileInput == "\n！")
          {
            if (this.GetParent().IsNote())
              return;
            this.TryShowQuickPopup(mark);
          }
          if (this.TextBox.Text == "^" || sentileInput == " ^" || sentileInput == "\n^" || this.TextBox.Text == "~" || sentileInput == " ~" || sentileInput == "\n~")
            this.TryShowQuickPopup(mark);
          if (this.TextBox.Text == "@" || sentileInput == " @" || sentileInput == "\n@")
            this.TryShowQuickPopup(mark);
          if (!(this.TextBox.Text == "*") && !(sentileInput == " *") && !(sentileInput == "\n*"))
            return;
          this.TryShowQuickPopup(mark);
          return;
        }
      }
      if (this.TextBox.IsFocused)
        return;
      this.RenderLinks();
    }

    private ListItemContent GetParent()
    {
      this._taskParent = this._taskParent ?? Utils.FindParent<ListItemContent>((DependencyObject) this);
      return this._taskParent;
    }

    private bool MarkChanged()
    {
      return this._startIndex > this.TextBox.Text.Length || this._startIndex <= 0 || !(this.SelectionPopup.Tag is string tag) || !(this.TextBox.Text.Substring(this._startIndex - 1, 1) == tag);
    }

    private string GetSentileInput(TextChangedEventArgs textChange)
    {
      if (textChange.Changes != null && textChange.Changes.Count == 1)
      {
        int startIndex = textChange.Changes.First<TextChange>().Offset - 1;
        if (startIndex >= 0 && startIndex + 2 <= this.TextBox.Text.Length)
          return this.TextBox.Text.Substring(startIndex, 2);
      }
      return string.Empty;
    }

    private void RenderLinks()
    {
      this.TextBlock.Inlines.Clear();
      string str = this.TextBox.Text.Replace("\r\n", "\n");
      if (string.IsNullOrEmpty(str))
        return;
      int startIndex = 0;
      MatchCollection matchCollection = TaskUtils.UrlRegex.Matches(str.ToLower());
      if (matchCollection.Count <= 0)
        return;
      this.TextBlock.Visibility = Visibility.Visible;
      this.TextBox.Visibility = Visibility.Collapsed;
      foreach (Match match in matchCollection)
      {
        if (match.Index != startIndex)
          this.TextBlock.Inlines.Add((Inline) new Run(str.Substring(startIndex, match.Index - startIndex)));
        Hyperlink hyperlink1 = new Hyperlink((Inline) new Run(str.Substring(match.Index, match.Value.Length)));
        hyperlink1.Tag = (object) str.Substring(match.Index, match.Value.Length);
        Hyperlink hyperlink2 = hyperlink1;
        hyperlink2.Click += new RoutedEventHandler(this.OnUrlClick);
        this.TextBlock.Inlines.Add((Inline) hyperlink2);
        startIndex = match.Index + match.Length;
      }
      if (startIndex >= str.Length)
        return;
      this.TextBlock.Inlines.Add((Inline) new Run(str.Substring(startIndex)));
    }

    private async void ShowTagPopup()
    {
      LinkTextBox linkTextBox = this;
      linkTextBox._tagPopup = new TagSelectAddWindow();
      linkTextBox._tagPopup.TagItems.TagSelected -= new EventHandler<string>(linkTextBox.OnTagSelected);
      linkTextBox._tagPopup.TagItems.TagSelected += new EventHandler<string>(linkTextBox.OnTagSelected);
      linkTextBox._tagPopup.Closed -= new EventHandler(linkTextBox.OnTagPopupClosed);
      linkTextBox._tagPopup.Closed += new EventHandler(linkTextBox.OnTagPopupClosed);
      linkTextBox._tagPopup.TagExit -= new EventHandler<string>(linkTextBox.OnTagInputExit);
      linkTextBox._tagPopup.TagExit += new EventHandler<string>(linkTextBox.OnTagInputExit);
      Rect fromCharacterIndex = linkTextBox.TextBox.GetRectFromCharacterIndex(linkTextBox.TextBox.CaretIndex);
      System.Windows.Point popupOffset = Utils.GetPopupOffset(linkTextBox.Context, (FrameworkElement) linkTextBox, fromCharacterIndex.X, fromCharacterIndex.Y + linkTextBox.TagPopupVerticalOffset);
      if (Math.Abs(popupOffset.X) <= 0.0 || Math.Abs(popupOffset.Y) <= 0.0)
        return;
      linkTextBox._tagPopup.Show(popupOffset.X, popupOffset.Y);
      EventHandler tagPopOpened = linkTextBox.TagPopOpened;
      if (tagPopOpened != null)
        tagPopOpened((object) linkTextBox, (EventArgs) null);
      int caretIndex = linkTextBox.TextBox.CaretIndex;
      linkTextBox._tagStartIndex = caretIndex;
    }

    private async void OnTagInputExit(object sender, string content)
    {
      if (this._tagStartIndex <= 0)
        return;
      string text = this.TextBox.Text;
      this.TextBox.Text = text.Substring(0, Math.Min(this._tagStartIndex - 1, text.Length)) + content + text.Substring(Math.Min(this._tagStartIndex, text.Length));
      int startIndex = this._tagStartIndex;
      this._tagStartIndex = -1;
      await Task.Delay(100);
      this.TextBox.Focus();
      this.TextBox.CaretIndex = Math.Max(startIndex + content.Length - 1, 0);
    }

    private async void OnTagPopupClosed(object sender, EventArgs e)
    {
      LinkTextBox sender1 = this;
      sender1._lastTagAddTime = DateTime.Now;
      EventHandler tagPopClosed = sender1.TagPopClosed;
      if (tagPopClosed != null)
        tagPopClosed((object) sender1, (EventArgs) null);
      await Task.Delay(100);
      sender1.TextBox.Focus();
    }

    private async void OnTagSelected(object sender, string tag)
    {
      LinkTextBox sender1 = this;
      if (sender1._tagStartIndex <= 0)
        return;
      string text = sender1.TextBox.Text;
      string e = text.Substring(0, sender1._tagStartIndex - 1) + text.Substring(Math.Min(text.Length, sender1._tagStartIndex));
      sender1.TextBox.Text = e;
      EventHandler<string> textChanged = sender1.TextChanged;
      if (textChanged != null)
        textChanged((object) sender1, e);
      await Task.Delay(40);
      EventHandler<string> tagSelected = sender1.TagSelected;
      if (tagSelected != null)
        tagSelected((object) sender1, tag.ToLower());
      await Task.Delay(40);
      sender1.TextBox.Focus();
      sender1.TextBox.CaretIndex = sender1._tagStartIndex;
      sender1._tagStartIndex = -1;
    }

    private void OnUrlClick(object sender, RoutedEventArgs e)
    {
      string url = ((FrameworkContentElement) sender).Tag.ToString();
      if (url.EndsWith(")"))
        url = url.Substring(0, url.Length - 1);
      if (url.StartsWith(" #") || url.StartsWith("#") || url.StartsWith("\n#"))
      {
        App.SelectTagProject(url.Replace(" #", "#").Replace("\n#", "#"));
      }
      else
      {
        ProjectTask taskUrlWithoutTitle = TaskUtils.ParseTaskUrlWithoutTitle(url);
        if (taskUrlWithoutTitle != null)
          Utils.FindParent<IListViewParent>((DependencyObject) this)?.NavigateTask(taskUrlWithoutTitle);
        else
          Utils.TryProcessStartUrl(url);
      }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Back:
          this.TryMerge();
          break;
        case Key.Return:
          if (this.SelectionPopup.IsOpen)
          {
            this._quickInputItems.TrySelectItem();
            return;
          }
          if (this._canSplit)
          {
            this.TrySplit();
            break;
          }
          break;
        case Key.Escape:
          this.ClearSelection();
          break;
        case Key.Up:
          this.TryMoveUp();
          break;
        case Key.Down:
          this.TryMoveDown();
          break;
      }
      this._textKeyPressed = false;
    }

    private void TrySplit()
    {
      if ((DateTime.Now - this._lastTextChangeTime).TotalMilliseconds < 100.0 || (DateTime.Now - this._lastTagAddTime).TotalMilliseconds < 500.0)
        return;
      int caretIndex = this.TextBox.CaretIndex;
      if (caretIndex > this.TextBox.Text.Length)
        return;
      try
      {
        string text = this.TextBox.Text.Substring(0, caretIndex);
        string extra = this.TextBox.Text.Substring(caretIndex);
        EventHandler<SplitData> splitText = this.SplitText;
        if (splitText == null)
          return;
        splitText((object) this, new SplitData(text, extra));
      }
      catch (Exception ex)
      {
      }
    }

    private async void TryMoveDown()
    {
      LinkTextBox sender = this;
      if (sender.SelectionPopup.IsOpen)
      {
        sender._quickInputItems.Move(false);
      }
      else
      {
        if (!sender.CanMoveDown())
          return;
        EventHandler moveDown = sender.MoveDown;
        if (moveDown == null)
          return;
        moveDown((object) sender, (EventArgs) null);
      }
    }

    private bool CanMoveDown()
    {
      return this._currentCaretLine == this.TextBox.GetLineIndexFromCharacterIndex(this.TextBox.Text.Length);
    }

    private async void TryMoveUp()
    {
      LinkTextBox sender = this;
      if (sender.SelectionPopup.IsOpen)
      {
        sender._quickInputItems.Move(true);
      }
      else
      {
        if (sender._currentCaretLine != 0)
          return;
        EventHandler moveUp = sender.MoveUp;
        if (moveUp == null)
          return;
        moveUp((object) sender, (EventArgs) null);
      }
    }

    private async void TryMerge()
    {
      LinkTextBox sender = this;
      if (!sender._canMerge || sender.TextBox.CaretIndex != 0)
        return;
      EventHandler<string> mergeText = sender.MergeText;
      if (mergeText == null)
        return;
      mergeText((object) sender, sender.TextBox.Text);
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
      if ((e.Key == Key.Left || e.Key == Key.Right) && this.SelectionPopup.IsOpen)
        e.Handled = true;
      if (e.ImeProcessedKey == Key.Return)
        this._canSplit = false;
      if (e.Key == Key.Return)
        this._canSplit = true;
      if (this._textKeyPressed)
        return;
      this._textKeyPressed = true;
      this._canMerge = e.Key == Key.Back;
    }

    public new bool Focus() => this.TextBox.Focus();

    public async void FocusEnd()
    {
      if (this.TextBox.Visibility != Visibility.Visible)
      {
        this.TextBlock.Visibility = Visibility.Hidden;
        this.TextBox.Visibility = Visibility.Visible;
        await Task.Delay(10);
      }
      this.Focus();
      this.TextBox.Select(this.TextBox.Text.Length, 0);
    }

    public async void FocusFirst()
    {
      if (this.TextBox.Visibility != Visibility.Visible)
      {
        this.TextBlock.Visibility = Visibility.Hidden;
        this.TextBox.Visibility = Visibility.Visible;
        await Task.Delay(10);
      }
      this.Focus();
      this.TextBox.Select(0, 0);
    }

    private async Task TryShowQuickPopup(string mark)
    {
      LinkTextBox linkTextBox = this;
      switch (mark)
      {
        case "!":
        case "！":
          TaskBaseViewModel taskDisplayModel1 = await linkTextBox.GetTaskDisplayModel();
          linkTextBox._quickInputItems = (IQuickInput) new PriorityInputItems(taskDisplayModel1 != null ? taskDisplayModel1.Priority : -1);
          ((BaseInputItems<int>) linkTextBox._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<int>>(linkTextBox.ItemSelected<int>);
          ((BaseInputItems<int>) linkTextBox._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<int>>(linkTextBox.ItemSelected<int>);
          break;
        case "^":
        case "~":
          TaskBaseViewModel taskDisplayModel2 = await linkTextBox.GetTaskDisplayModel();
          linkTextBox._quickInputItems = (IQuickInput) new ProjectInputItems(taskDisplayModel2?.ProjectId);
          ((BaseInputItems<ProjectModel>) linkTextBox._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<ProjectModel>>(linkTextBox.ItemSelected<ProjectModel>);
          ((BaseInputItems<ProjectModel>) linkTextBox._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<ProjectModel>>(linkTextBox.ItemSelected<ProjectModel>);
          break;
        case "*":
          linkTextBox._quickInputItems = (IQuickInput) new DateInputItems();
          ((BaseInputItems<DateTime>) linkTextBox._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<DateTime>>(linkTextBox.ItemSelected<DateTime>);
          ((BaseInputItems<DateTime>) linkTextBox._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<DateTime>>(linkTextBox.ItemSelected<DateTime>);
          break;
        case "@":
          List<AvatarViewModel> avatars = await linkTextBox.GetAvatars();
          linkTextBox._quickInputItems = (IQuickInput) new AssignInputItems(avatars);
          ((BaseInputItems<AvatarViewModel>) linkTextBox._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<AvatarViewModel>>(linkTextBox.ItemSelected<AvatarViewModel>);
          ((BaseInputItems<AvatarViewModel>) linkTextBox._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<AvatarViewModel>>(linkTextBox.ItemSelected<AvatarViewModel>);
          break;
      }
      if (linkTextBox._quickInputItems == null)
        return;
      System.Windows.Point popupOffset = linkTextBox.GetPopupOffset();
      if (linkTextBox._quickInputItems.Filter(""))
      {
        linkTextBox.ItemsContainer.Children.Clear();
        if (linkTextBox._quickInputItems is UIElement quickInputItems)
          linkTextBox.ItemsContainer.Children.Add(quickInputItems);
        linkTextBox.SelectionPopup.HorizontalOffset = popupOffset.X;
        linkTextBox.SelectionPopup.VerticalOffset = 0.0;
        linkTextBox.SelectionPopup.IsOpen = true;
        linkTextBox.SelectionPopup.Tag = (object) mark;
      }
      linkTextBox._startIndex = linkTextBox.TextBox.CaretIndex;
    }

    private void ClearSelection()
    {
      this._quickInputItems = (IQuickInput) null;
      this.ItemsContainer.Children.Clear();
      this.SelectionPopup.IsOpen = false;
      this.SelectionPopup.Tag = (object) "";
      this._startIndex = -1;
    }

    private System.Windows.Point GetPopupOffset()
    {
      Rect fromCharacterIndex = this.TextBox.GetRectFromCharacterIndex(this.TextBox.CaretIndex);
      return new System.Windows.Point(fromCharacterIndex.X, fromCharacterIndex.Y);
    }

    private async Task<TaskBaseViewModel> GetTaskDisplayModel()
    {
      ITaskOperation parent = Utils.FindParent<ITaskOperation>((DependencyObject) this);
      return parent == null || !(parent is Control control) || !(control.DataContext is DisplayItemModel dataContext) ? (TaskBaseViewModel) null : dataContext.SourceViewModel;
    }

    private async Task<List<AvatarViewModel>> GetAvatars()
    {
      ITaskOperation parent = Utils.FindParent<ITaskOperation>((DependencyObject) this);
      if (parent != null && parent is Control control)
      {
        DisplayItemModel model = control.DataContext as DisplayItemModel;
        if (model != null)
        {
          ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.ProjectId));
          if (projectModel != null && projectModel.IsShareList())
            return await AvatarHelper.GetProjectAvatars(model.ProjectId, fetchRemote: true);
        }
      }
      return (List<AvatarViewModel>) null;
    }

    private async void ItemSelected<T>(object sender, InputItemViewModel<T> e)
    {
      int startIndex = this._startIndex;
      int caretIndex = this.TextBox.CaretIndex;
      this.TextBox.Text = this.TextBox.Text.Remove(startIndex - 1, caretIndex - this._startIndex + 1);
      this.TextBox.Focus();
      this.TextBox.CaretIndex = startIndex - 1;
      await Task.Delay(30);
      this.ClearSelection();
    }

    public async void FocusOnMove()
    {
      if (this.TextBox.Visibility != Visibility.Visible)
      {
        this.TextBlock.Visibility = Visibility.Hidden;
        this.TextBox.Visibility = Visibility.Visible;
        await Task.Delay(10);
      }
      Utils.Focus((UIElement) this.TextBox);
      this.TextBox.Select(this.TextBox.Text.Length, 0);
      this.TextBox.ScrollToHorizontalOffset(10000.0);
    }

    public void SetText(string text)
    {
      this.Text = text;
      this._lastTextChangeTime = DateTime.Now;
      EventHandler<string> textChanged = this.TextChanged;
      if (textChanged == null)
        return;
      textChanged((object) this, text);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/linktextbox.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (LinkTextBox) target;
          break;
        case 2:
          this.TextBlock = (TextBlock) target;
          this.TextBlock.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTextBlockClick);
          break;
        case 3:
          this.TextBox = (TextBox) target;
          this.TextBox.GotFocus += new RoutedEventHandler(this.OnTextBoxGotFocus);
          this.TextBox.LostFocus += new RoutedEventHandler(this.OnTextBoxLostFocus);
          this.TextBox.KeyUp += new KeyEventHandler(this.OnKeyUp);
          this.TextBox.SelectionChanged += new RoutedEventHandler(this.OnTextBoxSelectionChanged);
          this.TextBox.PreviewMouseRightButtonDown += new MouseButtonEventHandler(this.OnRightClick);
          this.TextBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnLeftClick);
          this.TextBox.PreviewKeyDown += new KeyEventHandler(this.OnTextKeyDown);
          break;
        case 4:
          this.SelectionPopup = (Popup) target;
          break;
        case 5:
          this.ItemsContainer = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
