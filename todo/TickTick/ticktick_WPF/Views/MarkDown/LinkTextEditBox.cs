// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LinkTextEditBox
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using CommonMark.Syntax;
using Emoji.Wpf;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.DateParser;
using ticktick_WPF.Views.MarkDown.Colorizer;
using ticktick_WPF.Views.MarkDown.SpellCheck;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Search;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class LinkTextEditBox : 
    TextEditor,
    ILinkTextEditor,
    ISpellChecker,
    IEmojiRender,
    IDateParseBox
  {
    private DateTokenGenerator _dateTokenGenerator;
    private static Pen _selectionBorder;
    private static SolidColorBrush _selectionBrush;
    private DeleteLineColorizer _deleteLine = new DeleteLineColorizer();
    private static readonly Regex LinkRegex = new Regex("\\[(.*)\\]\\((.*)\\)");
    public static readonly DependencyProperty LineSpacingProperty = DependencyProperty.Register(nameof (LineSpacing), typeof (double), typeof (LinkTextEditBox), new PropertyMetadata((object) 7.0, new PropertyChangedCallback(LinkTextEditBox.OnLineSpacingChanged)));
    public static readonly DependencyProperty LinkSyntaxTreeProperty = DependencyProperty.Register(nameof (LinkSyntaxTree), typeof (Block), typeof (LinkTextEditBox), new PropertyMetadata((object) null));
    public static readonly DependencyProperty RenderLinkProperty = DependencyProperty.Register(nameof (RenderLink), typeof (bool), typeof (LinkTextEditBox), new PropertyMetadata((object) true));
    public static readonly DependencyProperty AcceptReturnProperty = DependencyProperty.Register(nameof (AcceptReturn), typeof (bool), typeof (LinkTextEditBox), new PropertyMetadata((object) false));
    public static readonly DependencyProperty TextStatusProperty = DependencyProperty.Register(nameof (TextStatus), typeof (int), typeof (LinkTextEditBox), new PropertyMetadata((object) 0, new PropertyChangedCallback(LinkTextEditBox.OnStatusChanged)));
    public static readonly DependencyProperty EnableSpellCheckProperty = DependencyProperty.Register(nameof (EnableSpellCheck), typeof (bool), typeof (LinkTextEditBox), new PropertyMetadata((object) true, new PropertyChangedCallback(LinkTextEditBox.OnSpellCheckChanged)));
    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(nameof (ReadOnly), typeof (bool), typeof (LinkTextEditBox), new PropertyMetadata((object) false, new PropertyChangedCallback(LinkTextEditBox.OnIsReadOnlyChangedCallback)));
    public static readonly DependencyProperty IsDarkProperty = DependencyProperty.Register(nameof (IsDark), typeof (bool), typeof (LinkTextEditBox), new PropertyMetadata((object) false, new PropertyChangedCallback(LinkTextEditBox.OnIsDarkChanged)));
    public readonly Dictionary<int, LinkInfo> LinkNameDict = new Dictionary<int, LinkInfo>();
    public readonly Dictionary<int, LinkInfo> LinkUrlDict = new Dictionary<int, LinkInfo>();
    public readonly Dictionary<int, string> EmojiDict = new Dictionary<int, string>();
    private bool _currentFocused;
    private InsertLinkWindow _linkWindow;
    private string _pasteContent;
    private int _previousLineCount;
    private int _parseLinkRandom;
    private SolidColorBrush _highLightColor;
    private SolidColorBrush _bracketColor;
    private int _caretBeforeKeyDown;
    protected bool ShowUrlNameOnly;
    private SearchHighlightBackgroundRender _searchRender;
    public IPaserDueDate ParsedData;
    private bool _selectionPopupOpened;
    private bool _inImeProcess;
    private SolidColorBrush _caretColor;
    private bool? _isDark;
    private string _color100 = "BaseColorOpacity100_80";
    private string _color80 = "BaseColorOpacity80_60";
    private string _color40 = "BaseColorOpacity40";
    private string _color20 = "BaseColorOpacity20";
    private string _uid;
    private string _filterContent;
    private QuickSelectionControl _selectionControl;
    private bool _canSplit;
    private bool? _canMerge;
    private int _markIndex;
    private bool _isLight;
    public Popup SelectionPopup;

    public event EventHandler<IPaserDueDate> DateParsed;

    public int MaxLength { get; set; } = -1;

    static LinkTextEditBox()
    {
      LinkTextEditBox._selectionBorder = new Pen((Brush) Brushes.Transparent, 0.0);
    }

    public LinkTextEditBox()
    {
      this._uid = Utils.GetGuid();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      if (LinkTextEditBox._selectionBrush == null)
      {
        SolidColorBrush solidColorBrush = new SolidColorBrush();
        solidColorBrush.Color = (Color) this.FindResource((object) SystemColors.HighlightColorKey);
        solidColorBrush.Opacity = 0.3;
        LinkTextEditBox._selectionBrush = solidColorBrush;
      }
      this.Name = "EditBox";
      this.WordWrap = false;
      this.SetValue(ScrollViewer.PanningModeProperty, (object) PanningMode.HorizontalFirst);
      this.TextArea.TextView.LineSpacing = 4.0;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      this.InitEditBox();
    }

    private static void OnLineSpacingChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is LinkTextEditBox linkTextEditBox) || !(e.NewValue is double newValue))
        return;
      linkTextEditBox.LineSpacing = newValue;
      linkTextEditBox.TextArea.TextView.LineSpacing = linkTextEditBox.WordWrap ? newValue : 4.0;
    }

    private static void OnIsDarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is LinkTextEditBox linkTextEditBox))
        return;
      int num = (e.NewValue as bool?).GetValueOrDefault() ? 1 : 0;
      linkTextEditBox.SetThemeColor(num != 0);
    }

    private void SetThemeColor(bool isDark)
    {
      bool? isDark1 = this._isDark;
      bool flag = isDark;
      if (isDark1.GetValueOrDefault() == flag & isDark1.HasValue)
        return;
      this._isDark = new bool?(isDark);
      this.SetTextHighLight();
      this.TextArea.TextView.Redraw();
    }

    private static void OnIsReadOnlyChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is LinkTextEditBox linkTextEditBox) || e.NewValue == null)
        return;
      linkTextEditBox.ReadOnly = (bool) e.NewValue;
    }

    private static void OnSpellCheckChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is LinkTextEditBox linkTextEditBox) || !(e.NewValue is bool newValue))
        return;
      linkTextEditBox.EnableSpellCheck = newValue;
    }

    private static void OnStatusChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is LinkTextEditBox linkTextEditBox))
        return;
      linkTextEditBox.SetTextForeground();
      linkTextEditBox.SetTextHighLight();
      linkTextEditBox.SetDeleteLine();
    }

    public void SetWordWrap(bool wrap)
    {
      if (this.WordWrap == wrap)
        return;
      this.WordWrap = wrap;
      this.SetValue(ScrollViewer.PanningModeProperty, (object) (PanningMode) (wrap ? 0 : 4));
      this.TextArea.TextView.LineSpacing = wrap ? this.LineSpacing : 4.0;
    }

    public double LineSpacing
    {
      get => (double) this.GetValue(LinkTextEditBox.LineSpacingProperty);
      set => this.SetValue(LinkTextEditBox.LineSpacingProperty, (object) value);
    }

    public bool IsDark
    {
      get => (bool) this.GetValue(LinkTextEditBox.IsDarkProperty);
      set => this.SetValue(LinkTextEditBox.IsDarkProperty, (object) value);
    }

    public bool AcceptReturn
    {
      get => (bool) this.GetValue(LinkTextEditBox.AcceptReturnProperty);
      set => this.SetValue(LinkTextEditBox.AcceptReturnProperty, (object) value);
    }

    public bool RenderLink
    {
      get => (bool) this.GetValue(LinkTextEditBox.RenderLinkProperty);
      set => this.SetValue(LinkTextEditBox.RenderLinkProperty, (object) value);
    }

    public int TextStatus
    {
      get => (int) this.GetValue(LinkTextEditBox.TextStatusProperty);
      set => this.SetValue(LinkTextEditBox.TextStatusProperty, (object) value);
    }

    public bool EnableSpellCheck
    {
      get => (bool) this.GetValue(LinkTextEditBox.EnableSpellCheckProperty);
      set
      {
        if (value)
          return;
        this.SpellCheckProvider?.Disconnect();
      }
    }

    public SpellCheckProvider SpellCheckProvider { get; set; }

    public bool AutoGetUrlTitle { get; set; } = true;

    public bool SelectionPopupOpened => this._selectionPopupOpened;

    public bool KeyboardFocused => this.TextArea.IsKeyboardFocused;

    public bool CurrentFocused
    {
      get => this._currentFocused;
      set => this._currentFocused = value;
    }

    private Block LinkSyntaxTree
    {
      get => (Block) this.GetValue(LinkTextEditBox.LinkSyntaxTreeProperty);
      set => this.SetValue(LinkTextEditBox.LinkSyntaxTreeProperty, (object) value);
    }

    public bool ReadOnly
    {
      get => (bool) this.GetValue(LinkTextEditBox.ReadOnlyProperty);
      set
      {
        this.IsReadOnly = value;
        this.TextArea.Caret.CaretBrush = value ? (Brush) Brushes.Transparent : (Brush) ThemeUtil.GetColor(this._color100, (FrameworkElement) this);
        if (!value)
          return;
        this.TextArea.ClearSelection();
      }
    }

    public void SetText(string val)
    {
      this.TextChanged -= new EventHandler(this.OnEditBoxTextChanged);
      this.LinkNameDict.Clear();
      this.LinkUrlDict.Clear();
      this.EmojiDict.Clear();
      this.Text = val;
      this.OnTextSet();
      this.TextChanged += new EventHandler(this.OnEditBoxTextChanged);
    }

    public void SetTextOffset(string text, bool restoreOffset, bool clearUndo = false)
    {
      this.TextChanged -= new EventHandler(this.OnEditBoxTextChanged);
      this.SetTextAndOffset(text, restoreOffset, clearUndo);
      this.OnTextSet();
      this.TextChanged += new EventHandler(this.OnEditBoxTextChanged);
    }

    private void OnTextSet()
    {
      this._parseLinkRandom = -1;
      this.LinkSyntaxTree = AbstractSyntaxTree.GenerateAbstractSyntaxTree(this.Text);
      this.LogLinkPositions();
      this.LogEmojiPosition();
      this.SpellCheckProvider?.SetTextChanged(true, false);
      this._searchRender?.SetTextChanged(true);
      this.TextArea.TextView.Redraw();
    }

    private void LogEmojiPosition()
    {
      this.EmojiDict.Clear();
      foreach (Match match in EmojiData.MatchOne2.Matches(this.Text))
        this.EmojiDict[match.Index] = match.Value;
    }

    public Dictionary<int, string> GetEmojiDict() => this.EmojiDict;

    public event EventHandler<ProjectTask> Navigate;

    public event EventHandler<double> CaretVerticalOffsetChanged;

    public event EventHandler<int> CaretChanged;

    public event EventHandler<System.Windows.Input.KeyEventArgs> KeysUp;

    public event EventHandler<System.Windows.Input.KeyEventArgs> KeysDown;

    public event EventHandler MoveUp;

    public event EventHandler MoveDown;

    public event EventHandler EscKeyUp;

    public event EventHandler<int> SplitText;

    public event EventHandler TextLostFocus;

    public event EventHandler<RoutedEventArgs> TextGotFocus;

    public event EventHandler PopOpened;

    public event EventHandler PopClosed;

    public event EventHandler LinkPopClosed;

    public event EventHandler LinkPopOpened;

    public event EventHandler SaveContent;

    public event EventHandler SelectDate;

    public event EventHandler LinkTextChange;

    public event EventHandler<List<string>> IgnoreTokenChanged;

    private void InitEditBox()
    {
      this.SetupGenerators();
      this._dateTokenGenerator = new DateTokenGenerator((IDateParseBox) this);
      this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) this._dateTokenGenerator);
      this.SetTextHighLight();
      this.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.Background = (Brush) Brushes.Transparent;
      this.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
      this.LostFocus += new RoutedEventHandler(this.OnLostFocus);
      this.VerticalAlignment = VerticalAlignment.Center;
      this.GotFocus += new RoutedEventHandler(this.OnFocused);
      this.MouseWheel += new MouseWheelEventHandler(this.OnMouseWheel);
      this.VerticalContentAlignment = VerticalAlignment.Center;
      this.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
      this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.OnKeyDown);
      this.PreviewKeyUp += new System.Windows.Input.KeyEventHandler(this.OnKeyUp);
      this.SetTextForeground();
      this.Options.IndentationSize = 4;
      this.Options.EnableHyperlinks = false;
      this.Options.ConvertTabsToSpaces = true;
      this.Options.AllowScrollBelowDocument = true;
      this.Options.EnableEmailHyperlinks = false;
      this.Options.InheritWordWrapIndentation = false;
      this.TextArea.FocusVisualStyle = (Style) null;
      this.TextArea.SelectionForeground = (Brush) null;
      this.TextArea.SelectionCornerRadius = 0.0;
      this.TextArea.VerticalAlignment = VerticalAlignment.Center;
      this.TextArea.VerticalContentAlignment = VerticalAlignment.Bottom;
      this.TextArea.SelectionBorder = LinkTextEditBox._selectionBorder;
      this.TextArea.SelectionBrush = (Brush) LinkTextEditBox._selectionBrush;
      this.TextArea.TextView.LineSpacing = this.WordWrap ? this.LineSpacing : 4.0;
      System.Windows.DataObject.RemovePastingHandler((DependencyObject) this, new DataObjectPastingEventHandler(this.OnPaste));
      System.Windows.DataObject.AddPastingHandler((DependencyObject) this, new DataObjectPastingEventHandler(this.OnPaste));
    }

    private void BindEvents()
    {
      this.TextChanged -= new EventHandler(this.OnEditBoxTextChanged);
      this.TextChanged += new EventHandler(this.OnEditBoxTextChanged);
      this.TextArea.Caret.PositionChanged += new EventHandler(this.NotifyCaretChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnAppThemeChanged);
      DataChangedNotifier.ShowCompleteLineChanged += new EventHandler(this.OnShowCompleteLineChanged);
      LocalSettings.SpellCheckChanged += new EventHandler(this.OnSpellCheckChanged);
    }

    private void OnShowCompleteLineChanged(object sender, EventArgs e) => this.SetDeleteLine();

    private void UnbindEvents()
    {
      this.TextArea.Caret.PositionChanged -= new EventHandler(this.NotifyCaretChanged);
      this.TextChanged -= new EventHandler(this.OnEditBoxTextChanged);
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnAppThemeChanged);
      DataChangedNotifier.ShowCompleteLineChanged -= new EventHandler(this.OnShowCompleteLineChanged);
      LocalSettings.SpellCheckChanged -= new EventHandler(this.OnSpellCheckChanged);
      DelayActionHandlerCenter.RemoveAction(this._uid);
    }

    private void TryParseDate()
    {
      QuickSelectionControl selectionControl = this._selectionControl;
      if ((selectionControl != null ? (selectionControl.PopupOpened() ? 1 : 0) : 0) != 0 || !LocalSettings.Settings.DateParsing || this._dateTokenGenerator == null)
        return;
      string str1 = this.Text;
      if (this._dateTokenGenerator.GetIgnoreTokens().Any<string>())
        str1 = this._dateTokenGenerator.GetIgnoreTokens().Aggregate<string, string>(str1, (Func<string, string, string>) ((current, token) => current.Replace(token, string.Empty)));
      this.ParsedData = ticktick_WPF.Util.DateParser.DateParser.Parse(str1, new DateTime?());
      if (this.ParsedData != null && this.ParsedData.GetRecognizeStrings().Any<string>())
      {
        List<string> recognizeStrings = this.ParsedData.GetRecognizeStrings();
        List<string> tokens = (recognizeStrings != null ? recognizeStrings.Select<string, string>((Func<string, string>) (str => str.TrimEnd())).ToList<string>() : (List<string>) null) ?? new List<string>();
        tokens.RemoveAll((Predicate<string>) (t => t.EndsWith("：")));
        this._dateTokenGenerator.AddTokens((IEnumerable<string>) tokens, str1);
      }
      else
      {
        this.ParsedData = (IPaserDueDate) null;
        this._dateTokenGenerator.ClearTokens();
      }
      this.TextArea.TextView.Redraw();
      EventHandler<IPaserDueDate> dateParsed = this.DateParsed;
      if (dateParsed == null)
        return;
      dateParsed((object) this, this.ParsedData);
    }

    private void OnSpellCheckChanged(object sender, EventArgs e)
    {
      if (!this.EnableSpellCheck)
        return;
      if (LocalSettings.Settings.SpellCheckEnable)
        this.SpellCheckProvider?.Initialize((ISpellChecker) this);
      else
        this.SpellCheckProvider?.Disconnect();
      this.TextChanged -= new EventHandler(this.OnEditBoxTextChanged);
      this.SpellCheckProvider?.SetTextChanged(true, false);
      this.Document.Insert(0, " ");
      this.Document.UndoStack.Undo();
      this.TextChanged += new EventHandler(this.OnEditBoxTextChanged);
    }

    private void NotifyTextChanged(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() =>
      {
        EventHandler linkTextChange = this.LinkTextChange;
        if (linkTextChange == null)
          return;
        linkTextChange(sender, e);
      }));
    }

    private void OnAppThemeChanged(object sender, EventArgs e)
    {
      this.SetTextHighLight();
      this.TextArea.TextView.Redraw();
    }

    public void SetLightTheme() => this.SetTextHighLight();

    public void SetBaseColor(string color100 = "BaseColorOpacity100_80", string color80 = "BaseColorOpacity80_60", string color40 = "BaseColorOpacity40", string color20 = "BaseColorOpacity20")
    {
      this._color100 = color100;
      this._color80 = color80;
      this._color40 = color40;
      this._color20 = color20;
    }

    private void SetTextHighLight()
    {
      this._highLightColor = this.TextStatus == 0 || this.TextStatus == 8060 ? ThemeUtil.GetColor("TextAccentColor", (FrameworkElement) this) : ThemeUtil.GetColor(this._color40, (FrameworkElement) this);
      this.TextArea.TextView.LinkTextForegroundBrush = (Brush) this._highLightColor;
      this._bracketColor = ThemeUtil.GetColor(this._color20, (FrameworkElement) this);
      this._caretColor = ThemeUtil.GetColor(this._color100, (FrameworkElement) this);
      this.TextArea.Caret.CaretBrush = this.IsReadOnly ? (Brush) Brushes.Transparent : (Brush) this._caretColor;
    }

    public void SetTextForeground()
    {
      switch (this.TextStatus)
      {
        case 0:
          this.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, (object) this._color100);
          break;
        case 8060:
          this.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, (object) this._color80);
          break;
        default:
          this.SetResourceReference(System.Windows.Controls.Control.ForegroundProperty, (object) this._color40);
          break;
      }
    }

    private void SetDeleteLine()
    {
      if (LocalSettings.Settings.ExtraSettings.ShowCompleteLine == 1 && (this.TextStatus == 2 || this.TextStatus == 1 || this.TextStatus == -1))
      {
        this.TextArea.TextView.LineTransformers.Remove((IVisualLineTransformer) this._deleteLine);
        this.TextArea.TextView.LineTransformers.Add((IVisualLineTransformer) this._deleteLine);
      }
      else
        this.TextArea.TextView.LineTransformers.Remove((IVisualLineTransformer) this._deleteLine);
    }

    public void UnRegisterCaretChanged()
    {
      this.TextArea.Caret.PositionChanged -= new EventHandler(this.NotifyCaretChanged);
    }

    public void RegisterCaretChanged()
    {
      this.TextArea.Caret.PositionChanged -= new EventHandler(this.NotifyCaretChanged);
      this.TextArea.Caret.PositionChanged += new EventHandler(this.NotifyCaretChanged);
    }

    public TextEditor GetEditBox() => (TextEditor) this;

    public Brush GetBracketColor() => (Brush) this._bracketColor;

    private void NotifyCaretChanged(object sender, EventArgs e)
    {
      double e1 = this.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.TextArea.Caret.Position.Line, Math.Max(1, this.TextArea.Caret.Column)), VisualYPosition.LineTop).Y + 21.0;
      if (this.CurrentFocused)
      {
        EventHandler<double> verticalOffsetChanged = this.CaretVerticalOffsetChanged;
        if (verticalOffsetChanged != null)
          verticalOffsetChanged((object) this, e1);
      }
      EventHandler<int> caretChanged = this.CaretChanged;
      if (caretChanged != null)
        caretChanged((object) this, this.CaretOffset);
      bool flag = false;
      foreach (KeyValuePair<int, LinkInfo> keyValuePair in this.LinkNameDict)
      {
        if (keyValuePair.Key <= this.CaretOffset && this.CaretOffset <= keyValuePair.Key + keyValuePair.Value.Link.Length)
        {
          flag = true;
          break;
        }
      }
      this.TextArea.Caret.CaretBrush = this.IsReadOnly ? (Brush) Brushes.Transparent : (flag ? (Brush) this._highLightColor : (Brush) this._caretColor);
    }

    private int GetRemoveLinkLength(int caret, bool onlyUrl = false, bool behindIndex = false)
    {
      int removeLinkLength = -1;
      if (this.LinkUrlDict.Any<KeyValuePair<int, LinkInfo>>())
      {
        int startIndex = behindIndex ? this.Text.IndexOf(")", caret, StringComparison.Ordinal) : this.Text.LastIndexOf("[", caret, StringComparison.Ordinal);
        if (startIndex >= 0)
        {
          string str1 = behindIndex ? this.Text.Substring(caret, startIndex - caret + 1) : this.Text.Substring(startIndex, caret - startIndex);
          foreach (KeyValuePair<int, LinkInfo> keyValuePair in this.LinkNameDict.ToList<KeyValuePair<int, LinkInfo>>())
          {
            string str2 = "[" + keyValuePair.Value.Link + "](" + keyValuePair.Value.Url + ")";
            if (str1 == str2)
            {
              removeLinkLength = onlyUrl ? keyValuePair.Value.Url.Length : str1.Length;
              break;
            }
          }
        }
      }
      return removeLinkLength;
    }

    protected virtual void SetupGenerators()
    {
      this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new EmojiGenerator((IEmojiRender) this));
      this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new CustomLinkElementGenerator((ILinkTextEditor) this, !this.RenderLink));
      if (this.RenderLink)
      {
        this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkNameGenerator((ILinkTextEditor) this));
        this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkUrlGenerator((ILinkTextEditor) this));
        this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkBracketGenerator((ILinkTextEditor) this, true, true));
        this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkBracketGenerator((ILinkTextEditor) this, false, true));
        this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkBracketGenerator((ILinkTextEditor) this, true, false));
        this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkBracketGenerator((ILinkTextEditor) this, false, false));
      }
      if (!this.EnableSpellCheck)
        return;
      if (SpellingService.CommonSpellingService == null)
        SpellingService.CommonSpellingService = new SpellingService();
      this.SpellCheckProvider = new SpellCheckProvider((ISpellingService) SpellingService.CommonSpellingService);
      if (!LocalSettings.Settings.SpellCheckEnable)
        return;
      this.SpellCheckProvider.Initialize((ISpellChecker) this);
    }

    public void SetupSearchRender(bool isTitle, bool inSearchDetail)
    {
      if (this._searchRender != null)
        return;
      this._searchRender = new SearchHighlightBackgroundRender((TextEditor) this, this.LinkUrlDict, isTitle, inSearchDetail);
      this.TextArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer) this._searchRender);
    }

    public void SetupPreSearchRender()
    {
      this._searchRender = new SearchHighlightBackgroundRender((TextEditor) this, this.LinkUrlDict, true, false, true);
      this.TextArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer) this._searchRender);
    }

    public double GetFirstSearchIndex()
    {
      int offset = (int?) this._searchRender?.GetFirstIndex() ?? -1;
      return offset >= 0 && offset < this.Text.Length ? this.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.Document.GetLocation(offset)), VisualYPosition.LineTop).Y + 21.0 : -1.0;
    }

    public void ShowInsertLink(string name, string url, VisualLine line = null, bool isNew = true)
    {
      if (this.ReadOnly)
        return;
      if (string.IsNullOrEmpty(url))
      {
        string text = System.Windows.Clipboard.GetText();
        if (!string.IsNullOrEmpty(text))
        {
          if (new Regex("(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]?\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]\\.[^\\s]{2,})").IsMatch(text))
            url = text;
          ProjectTask taskUrl = TaskUtils.ParseTaskUrl(text);
          if (taskUrl != null)
            url = text.Replace(" " + taskUrl.Title, string.Empty).TrimEnd();
        }
      }
      else
        isNew = false;
      this.InsertLink(name, url, line, isNew);
    }

    private async void InsertLink(string name, string url, VisualLine line = null, bool isNew = true)
    {
      LinkTextEditBox linkTextEditBox1 = this;
      if (!string.IsNullOrEmpty(name) && name.Contains("\n"))
        return;
      EventHandler linkPopOpened = linkTextEditBox1.LinkPopOpened;
      if (linkPopOpened != null)
        linkPopOpened((object) linkTextEditBox1, (EventArgs) null);
      System.Windows.Point startPosition = linkTextEditBox1.GetStartPosition(line, 320, 210);
      LinkTextEditBox linkTextEditBox2 = linkTextEditBox1;
      InsertLinkWindow insertLinkWindow = new InsertLinkWindow((ILinkTextEditor) linkTextEditBox1, name, url, isNew);
      insertLinkWindow.Left = startPosition.X + 10.0;
      insertLinkWindow.Top = startPosition.Y + 10.0;
      linkTextEditBox2._linkWindow = insertLinkWindow;
      linkTextEditBox1._linkWindow.Closed -= new EventHandler(linkTextEditBox1.OnLinkClosed);
      linkTextEditBox1._linkWindow.Closed += new EventHandler(linkTextEditBox1.OnLinkClosed);
      linkTextEditBox1._linkWindow.ShowDialog();
    }

    private void OnLinkClosed(object sender, EventArgs e)
    {
      EventHandler linkPopClosed = this.LinkPopClosed;
      if (linkPopClosed == null)
        return;
      linkPopClosed(sender, e);
    }

    private void OnEditBoxTextChanged(object sender, EventArgs eventArgs)
    {
      this.HandleOnTextChanged();
      DelayActionHandlerCenter.TryDoAction(this._uid, new EventHandler(this.NotifyTextChanged), 50);
    }

    private void HandleOnTextChanged()
    {
      try
      {
        if (!SearchHelper.DetailChanged)
        {
          SearchHighlightBackgroundRender searchRender = this._searchRender;
          if ((searchRender != null ? (searchRender.InDetail ? 1 : 0) : 0) != 0)
            SearchHelper.DetailChanged = true;
        }
        if (this.CanParseDate)
          this.TryParseDate();
        this.LinkSyntaxTree = AbstractSyntaxTree.GenerateAbstractSyntaxTree(this.Text);
        this.LogLinkPositions();
        this.LogEmojiPosition();
        this.SpellCheckProvider?.SetTextChanged(true);
        this._searchRender?.SetTextChanged(true);
        this.TextArea.TextView.Redraw();
        this.TryShowQuickSetPopup();
      }
      catch (Exception ex)
      {
      }
    }

    private void LogLinkPositions()
    {
      this.LinkNameDict.Clear();
      this.LinkUrlDict.Clear();
      foreach (Block enumerateSpanningBlock in AbstractSyntaxTree.EnumerateSpanningBlocks(this.LinkSyntaxTree, 0, this.Document.TextLength))
      {
        foreach (Inline enumerateInline in AbstractSyntaxTree.EnumerateInlines(enumerateSpanningBlock.InlineContent))
        {
          if (enumerateInline.Tag == InlineTag.Link)
          {
            string input = this.Document.Text.Substring(enumerateInline.SourcePosition, enumerateInline.SourceLength);
            if (input.Contains("["))
            {
              int num = input.IndexOf("[", StringComparison.Ordinal);
              input = this.Document.Text.Substring(enumerateInline.SourcePosition + num, enumerateInline.SourceLength);
            }
            Match match = LinkTextEditBox.LinkRegex.Match(input);
            if (match.Success)
            {
              string link = match.Groups[1].Value;
              string url = match.Groups[2].Value;
              if (!string.IsNullOrEmpty(url))
              {
                LinkInfo linkInfo = new LinkInfo(link, url);
                this.LinkNameDict.Add(enumerateInline.SourcePosition + match.Groups[1].Index, linkInfo);
                this.LinkUrlDict.Add(enumerateInline.SourcePosition + match.Groups[2].Index, linkInfo);
              }
            }
          }
        }
      }
    }

    protected virtual async void OnPaste(object sender, DataObjectPastingEventArgs arg)
    {
      LinkTextEditBox linkTextEditBox = this;
      try
      {
        string data = (string) arg.SourceDataObject.GetData(System.Windows.DataFormats.UnicodeText, true);
        ProjectTask extra = TaskUtils.ParseTaskUrl(data) ?? TaskUtils.ParseTaskUrlWithoutTitle(data);
        if (extra != null && data != null && !data.StartsWith("[") && !data.EndsWith(")"))
        {
          string text = (linkTextEditBox.AcceptReturn ? data : data.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ")).Replace(" " + extra.Title, string.Empty);
          arg.CancelCommand();
          await linkTextEditBox.TryPasteLinkTask(extra, text);
        }
        else
        {
          if (data == null)
            return;
          string text1 = linkTextEditBox.AcceptReturn ? data : data.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
          arg.CancelCommand();
          if (linkTextEditBox.MaxLength >= 0 && linkTextEditBox.Text.Length + text1.Length - linkTextEditBox.SelectionLength > linkTextEditBox.MaxLength)
          {
            if (!linkTextEditBox.WordWrap)
              text1 = text1.Replace("\n", " ").Replace("\r", " ");
            string text2 = text1.Substring(0, linkTextEditBox.MaxLength - linkTextEditBox.Text.Length + linkTextEditBox.SelectionLength);
            linkTextEditBox.InsertText(text2);
          }
          else
          {
            if (!linkTextEditBox.WordWrap)
              text1 = text1.Replace("\n", " ").Replace("\r", " ");
            linkTextEditBox.InsertText(text1);
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void InsertText(string text)
    {
      int offset = this.CaretOffset;
      if (this.SelectionLength > 0)
      {
        this.Document.Remove(this.SelectionStart, this.SelectionLength);
        offset = this.SelectionStart;
      }
      this.Document.Insert(offset, text);
      this.CaretOffset = offset + text.Length;
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
      if (this.MaxLength >= 0)
      {
        string text1 = e.Text;
        if (this.Text.Length >= this.MaxLength)
        {
          e.Handled = true;
          Utils.Toast(Utils.GetString("CharactersNumberLimit"));
        }
        else if (this.Text.Length + text1.Length - this.SelectionLength > this.MaxLength)
        {
          e.Handled = true;
          string text2 = text1.Substring(0, Math.Max(0, this.MaxLength - this.Text.Length + this.SelectionLength));
          if (text2.Length == 0)
          {
            Utils.Toast(Utils.GetString("CharactersNumberLimit"));
            return;
          }
          this.InsertText(text2);
        }
      }
      base.OnPreviewTextInput(e);
    }

    private async Task TryPasteLinkTask(ProjectTask extra, string text)
    {
      LinkTextEditBox linkTextEditBox = this;
      string str = extra.Title;
      if (string.IsNullOrEmpty(str))
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(extra.TaskId);
        str = thinTaskById == null || string.IsNullOrEmpty(thinTaskById.title) ? Utils.GetString("MyTask") : thinTaskById.title;
      }
      // ISSUE: explicit non-virtual call
      __nonvirtual (linkTextEditBox.Document).Insert(linkTextEditBox.CaretOffset, "[" + str + "](" + text + ")");
    }

    private System.Windows.Point GetStartPosition(VisualLine line = null, int width = 245, int height = 140)
    {
      TextView textView = this.TextArea.TextView;
      TextViewPosition position = new TextViewPosition(this.Document.GetLocation(this.CaretOffset));
      if (line != null)
        position = new TextViewPosition(this.Document.GetLocation(line.StartOffset));
      System.Windows.Point visualPosition1 = textView.GetVisualPosition(position, VisualYPosition.LineBottom);
      System.Windows.Point visualPosition2 = textView.GetVisualPosition(position, VisualYPosition.LineBottom);
      System.Windows.Point screen1 = textView.PointToScreen(visualPosition1 - textView.ScrollOffset);
      System.Windows.Point screen2 = textView.PointToScreen(visualPosition2 - textView.ScrollOffset);
      Size device = new Size((double) width, (double) height).TransformToDevice((Visual) textView);
      Rect rect = new Rect(screen1, device);
      Rect wpf = Screen.GetWorkingArea(screen1.ToSystemDrawing()).ToWpf();
      if (!wpf.Contains(rect))
      {
        if (rect.Left < wpf.Left)
          rect.X = wpf.Left;
        else if (rect.Right > wpf.Right)
          rect.X = wpf.Right - rect.Width;
        if (rect.Bottom > wpf.Bottom)
          rect.Y = screen2.Y - rect.Height;
        if (rect.Y < wpf.Top)
          rect.Y = wpf.Top;
      }
      rect = rect.TransformFromDevice((Visual) textView);
      return new System.Windows.Point(rect.X - 20.0, rect.Y - 20.0);
    }

    protected void OnPopupOpenChanged(bool open)
    {
      if (open)
      {
        EventHandler popOpened = this.PopOpened;
        if (popOpened == null)
          return;
        popOpened((object) this, (EventArgs) null);
      }
      else
      {
        EventHandler popClosed = this.PopClosed;
        if (popClosed == null)
          return;
        popClosed((object) this, (EventArgs) null);
      }
    }

    protected void EditorMenuOnContextMenuOpening(object sender, ContextMenuEventArgs context)
    {
      if (this.ReadOnly)
        return;
      this.AddEditorContextMenu(context);
      this.OnPopupOpenChanged(true);
    }

    public void AddEditorContextMenu(ContextMenuEventArgs context)
    {
      System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();
      contextMenu.PlacementTarget = (UIElement) this;
      if (this.EnableSpellCheck && !this.ReadOnly)
      {
        SpellCheckSuggestion.SpellCheckSuggestions((ISpellChecker) this, contextMenu);
        SpellCheckSuggestion.AddSpellCheckLanguageMenu(contextMenu);
      }
      ItemCollection items1 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem1 = new System.Windows.Controls.MenuItem();
      newItem1.Header = (object) Utils.GetString("Cut");
      newItem1.Command = (ICommand) ApplicationCommands.Cut;
      newItem1.CommandTarget = (IInputElement) this.TextArea;
      newItem1.InputGestureText = "Ctrl+X";
      items1.Add((object) newItem1);
      ItemCollection items2 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem2 = new System.Windows.Controls.MenuItem();
      newItem2.Header = (object) Utils.GetString("Copy");
      newItem2.Command = (ICommand) ApplicationCommands.Copy;
      newItem2.CommandTarget = (IInputElement) this.TextArea;
      newItem2.InputGestureText = "Ctrl+C";
      items2.Add((object) newItem2);
      ItemCollection items3 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem3 = new System.Windows.Controls.MenuItem();
      newItem3.Header = (object) Utils.GetString("Paste");
      newItem3.Command = (ICommand) ApplicationCommands.Paste;
      newItem3.CommandTarget = (IInputElement) this.TextArea;
      newItem3.InputGestureText = "Ctrl+V";
      items3.Add((object) newItem3);
      contextMenu.Items.Add((object) new Separator());
      ItemCollection items4 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem4 = new System.Windows.Controls.MenuItem();
      newItem4.Header = (object) Utils.GetString("Undo");
      newItem4.Command = (ICommand) ApplicationCommands.Undo;
      newItem4.CommandTarget = (IInputElement) this.TextArea;
      newItem4.InputGestureText = "Ctrl+Z";
      items4.Add((object) newItem4);
      ItemCollection items5 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem5 = new System.Windows.Controls.MenuItem();
      newItem5.Header = (object) Utils.GetString("Redo");
      newItem5.Command = (ICommand) ApplicationCommands.Redo;
      newItem5.CommandTarget = (IInputElement) this.TextArea;
      newItem5.InputGestureText = "Ctrl+Y";
      items5.Add((object) newItem5);
      ((FrameworkElement) context.Source).ContextMenu = contextMenu;
      contextMenu.IsOpen = true;
    }

    public Dictionary<int, LinkInfo> GetLinkNameDict() => this.LinkNameDict;

    public Dictionary<int, LinkInfo> GetLinkUrlDict() => this.LinkUrlDict;

    public void NavigateTask(string projectId, string taskId)
    {
      EventHandler<ProjectTask> navigate = this.Navigate;
      if (navigate == null)
        return;
      navigate((object) this, new ProjectTask()
      {
        ProjectId = projectId,
        TaskId = taskId
      });
    }

    public void ResetText(string text)
    {
      this.UnRegisterCaretChanged();
      this.Text = text;
      this.OnEditBoxTextChanged((object) null, (EventArgs) null);
      this.RegisterCaretChanged();
    }

    public Brush GetHighLightColor() => (Brush) this._highLightColor;

    public int GetPageLastIndex(double height)
    {
      TextView textView = this.TextArea.TextView;
      DocumentLine documentLine = (DocumentLine) null;
      foreach (DocumentLine line in (IEnumerable<DocumentLine>) this.Document.Lines)
      {
        TextViewPosition position1 = new TextViewPosition(this.Document.GetLocation(line.Offset));
        textView.GetVisualPosition(position1, VisualYPosition.LineBottom);
        TextViewPosition position2 = new TextViewPosition(this.Document.GetLocation(line.Offset));
        if (textView.GetVisualPosition(position2, VisualYPosition.LineBottom).Y > height)
        {
          documentLine = line;
          break;
        }
      }
      if (documentLine == null)
        return this.Text.Length;
      int interval = documentLine.Length > 1000 ? 1000 : (documentLine.Length > 100 ? 100 : 10);
      return this.GetPageLastIndex(height, documentLine.Offset, documentLine.EndOffset, interval);
    }

    private int GetPageLastIndex(double height, int start, int end, int interval)
    {
      TextView textView = this.TextArea.TextView;
      if (start == end)
        return end;
      for (int pageLastIndex = start; pageLastIndex <= end; pageLastIndex += interval)
      {
        TextViewPosition position1 = new TextViewPosition(this.Document.GetLocation(pageLastIndex));
        System.Windows.Point visualPosition1 = textView.GetVisualPosition(position1, VisualYPosition.LineBottom);
        TextViewPosition position2 = new TextViewPosition(this.Document.GetLocation(pageLastIndex + interval));
        System.Windows.Point visualPosition2 = textView.GetVisualPosition(position2, VisualYPosition.LineBottom);
        if (visualPosition1.Y < height && visualPosition2.Y > height)
          return interval == 1 ? pageLastIndex : this.GetPageLastIndex(height, pageLastIndex, Math.Min(end, pageLastIndex + interval), interval / 10);
        if (visualPosition1.Y > height)
          return pageLastIndex;
      }
      return end;
    }

    private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (this.ReadOnly)
        return;
      if (this._inImeProcess)
      {
        EventHandler<System.Windows.Input.KeyEventArgs> keysUp = this.KeysUp;
        if (keysUp != null)
          keysUp(sender, e);
        this._canMerge = new bool?();
        this._canSplit = false;
        this._caretBeforeKeyDown = -1;
        this._inImeProcess = false;
      }
      else
      {
        switch (e.Key)
        {
          case Key.Back:
            if (this._canMerge.GetValueOrDefault())
            {
              EventHandler<string> mergeText = this.MergeText;
              if (mergeText != null)
              {
                mergeText((object) this, this.Text);
                break;
              }
              break;
            }
            break;
          case Key.Return:
            if (this.IsQuickPopupOpen())
            {
              this._selectionControl.TrySelectItem();
              e.Handled = true;
              return;
            }
            if (this._canSplit && !Utils.IfCtrlPressed())
            {
              EventHandler<int> splitText = this.SplitText;
              if (splitText != null)
              {
                splitText((object) this, this.CaretOffset);
                break;
              }
              break;
            }
            break;
          case Key.Escape:
            EventHandler escKeyUp = this.EscKeyUp;
            if (escKeyUp != null)
            {
              escKeyUp((object) this, (EventArgs) e);
              break;
            }
            break;
          case Key.Up:
            if (this.IsQuickPopupOpen())
            {
              this._selectionControl.Move(true);
              e.Handled = true;
              return;
            }
            if (!this.WordWrap || this._caretBeforeKeyDown == this.CaretOffset && !Utils.IfShiftPressed())
            {
              EventHandler moveUp = this.MoveUp;
              if (moveUp != null)
              {
                moveUp(sender, (EventArgs) null);
                break;
              }
              break;
            }
            break;
          case Key.Down:
            if (this.IsQuickPopupOpen())
            {
              this._selectionControl.Move(false);
              e.Handled = true;
              return;
            }
            if (!this.WordWrap || this._caretBeforeKeyDown == this.CaretOffset && !Utils.IfShiftPressed())
            {
              EventHandler moveDown = this.MoveDown;
              if (moveDown != null)
              {
                moveDown(sender, (EventArgs) null);
                break;
              }
              break;
            }
            break;
        }
        EventHandler<System.Windows.Input.KeyEventArgs> keysUp = this.KeysUp;
        if (keysUp != null)
          keysUp(sender, e);
        this._canMerge = new bool?();
        this._canSplit = false;
        this._caretBeforeKeyDown = -1;
      }
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (this.ReadOnly)
        return;
      this._inImeProcess = false;
      switch (e.Key)
      {
        case Key.Back:
          if (!this._canMerge.HasValue)
            this._canMerge = new bool?(this.CaretOffset == 0 && this.SelectedText.Length == 0);
          if (!this.ShowUrlNameOnly)
          {
            int caretOffset = this.CaretOffset;
            int removeLinkLength = this.GetRemoveLinkLength(caretOffset);
            if (removeLinkLength > 0)
            {
              this.Document.Remove(caretOffset - removeLinkLength, removeLinkLength);
              EventHandler linkTextChange = this.LinkTextChange;
              if (linkTextChange != null)
                linkTextChange((object) this, (EventArgs) null);
              e.Handled = true;
            }
          }
          else if (this.TryDeleteLinkName(true))
            e.Handled = true;
          List<string> validTokens = this._dateTokenGenerator?.GetValidTokens();
          // ISSUE: explicit non-virtual call
          if (validTokens != null && __nonvirtual (validTokens.Count) > 0)
          {
            using (List<string>.Enumerator enumerator = validTokens.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                string current = enumerator.Current;
                int num = this.Text.LastIndexOf(current, this.CaretOffset, StringComparison.Ordinal);
                if (num >= 0 && num == this.CaretOffset - current.Length)
                {
                  this._dateTokenGenerator.AddIgnoreToken(current);
                  EventHandler<List<string>> ignoreTokenChanged = this.IgnoreTokenChanged;
                  if (ignoreTokenChanged != null)
                    ignoreTokenChanged((object) this, this._dateTokenGenerator.GetIgnoreTokens());
                  this.TryParseDate();
                  e.Handled = true;
                }
              }
              break;
            }
          }
          else
            break;
        case Key.Tab:
          e.Handled = true;
          break;
        case Key.Return:
          if (!this.AcceptReturn || this.IsQuickPopupOpen())
            e.Handled = true;
          this._canSplit = true;
          break;
        case Key.Escape:
          if (this.IsQuickPopupOpen())
          {
            this._selectionControl?.ClosePopup();
            e.Handled = true;
            return;
          }
          break;
        case Key.Left:
          if (this.IsQuickPopupOpen())
            e.Handled = true;
          if (this.ShowUrlNameOnly && this.TryMoveOverLinkBracket(false))
          {
            e.Handled = true;
            break;
          }
          break;
        case Key.Up:
        case Key.Down:
          if (this._caretBeforeKeyDown < 0)
            this._caretBeforeKeyDown = this.CaretOffset;
          if (this.IsQuickPopupOpen())
          {
            e.Handled = true;
            break;
          }
          break;
        case Key.Right:
          if (this.IsQuickPopupOpen())
            e.Handled = true;
          if (this.ShowUrlNameOnly && this.TryMoveOverLinkBracket(true))
          {
            e.Handled = true;
            break;
          }
          break;
        case Key.Delete:
          if (this.ShowUrlNameOnly && this.TryDeleteLinkName(false))
          {
            e.Handled = true;
            break;
          }
          break;
      }
      this._inImeProcess = e.ImeProcessedKey != 0;
      if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && LocalSettings.Settings.ShortCutModel.SetDate == "Ctrl+D")
      {
        e.Handled = true;
        EventHandler selectDate = this.SelectDate;
        if (selectDate != null)
          selectDate((object) this, (EventArgs) null);
      }
      EventHandler<System.Windows.Input.KeyEventArgs> keysDown = this.KeysDown;
      if (keysDown == null)
        return;
      keysDown((object) this, e);
    }

    private bool TryMoveOverLinkBracket(bool isRight)
    {
      int caretOffset = this.CaretOffset;
      if (this.LinkUrlDict == null || !this.LinkUrlDict.Any<KeyValuePair<int, LinkInfo>>())
        return false;
      foreach (KeyValuePair<int, LinkInfo> keyValuePair in this.LinkNameDict.ToList<KeyValuePair<int, LinkInfo>>())
      {
        if (isRight && caretOffset < this.Text.Length - 3)
        {
          if (this.Text[caretOffset] == '[')
          {
            string str = "[" + keyValuePair.Value.Link + "](" + keyValuePair.Value.Url + ")";
            if (caretOffset <= this.Text.Length - str.Length && this.Text.Substring(caretOffset, str.Length) == str)
            {
              this.CaretOffset += 2;
              return true;
            }
          }
          else if (this.Text[caretOffset + 1] == ']' && keyValuePair.Value.Link.Length > 0)
          {
            string str1 = keyValuePair.Value.Link.Substring(keyValuePair.Value.Link.Length - 1, 1) + "](" + keyValuePair.Value.Url + ")";
            string str2 = "[" + keyValuePair.Value.Link.Substring(0, keyValuePair.Value.Link.Length - 1);
            if (caretOffset <= this.Text.Length - str1.Length && caretOffset >= str2.Length)
            {
              string str3 = this.Text.Substring(caretOffset, str1.Length);
              string str4 = this.Text.Substring(caretOffset - str2.Length, str2.Length);
              string str5 = str1;
              if (str3 == str5 && str4 == str2)
              {
                this.CaretOffset += keyValuePair.Value.Url.Length + 4;
                return true;
              }
            }
          }
        }
        if (!isRight && caretOffset > 1)
        {
          if (this.Text[caretOffset - 2] == '[' && keyValuePair.Value.Link.Length > 0)
          {
            string str = keyValuePair.Value.Link.Substring(1, keyValuePair.Value.Link.Length - 1) + "](" + keyValuePair.Value.Url + ")";
            if (caretOffset <= this.Text.Length - str.Length && this.Text.Substring(caretOffset, str.Length) == str)
            {
              this.CaretOffset -= 2;
              return true;
            }
          }
          else if (this.Text[caretOffset - 1] == ')' && keyValuePair.Value.Link.Length > 0)
          {
            string str = "[" + keyValuePair.Value.Link + "](" + keyValuePair.Value.Url + ")";
            if (caretOffset >= str.Length && this.Text.Substring(caretOffset - str.Length, str.Length) == str)
            {
              this.CaretOffset -= (keyValuePair.Value.Link.Substring(keyValuePair.Value.Link.Length - 1, 1) + "](" + keyValuePair.Value.Url + ")").Length;
              return true;
            }
          }
        }
      }
      return false;
    }

    private bool TryDeleteLinkName(bool backDelete)
    {
      int caretOffset = this.CaretOffset;
      if (this.LinkUrlDict == null || !this.LinkUrlDict.Any<KeyValuePair<int, LinkInfo>>())
        return false;
      int startIndex1 = backDelete ? this.Text.LastIndexOf("[", caretOffset, StringComparison.Ordinal) : this.Text.IndexOf(")", caretOffset, StringComparison.Ordinal);
      if (startIndex1 < 0)
        return false;
      int startIndex2 = backDelete ? this.Text.IndexOf(")", startIndex1, StringComparison.Ordinal) : this.Text.LastIndexOf("[", startIndex1, StringComparison.Ordinal);
      string str1 = backDelete ? this.Text.Substring(startIndex1, Math.Max(0, startIndex2 - startIndex1 + 1)) : (startIndex2 >= 0 ? this.Text.Substring(startIndex2, Math.Max(0, startIndex1 - startIndex2 + 1)) : string.Empty);
      foreach (KeyValuePair<int, LinkInfo> keyValuePair in this.LinkNameDict.ToList<KeyValuePair<int, LinkInfo>>())
      {
        string str2 = "[" + keyValuePair.Value.Link + "](" + keyValuePair.Value.Url + ")";
        if (str1 == str2)
        {
          if (keyValuePair.Value.Link.Length <= 1)
          {
            if (this.SelectedText.Length > 0 || startIndex2 + (backDelete ? 1 : 0) != caretOffset)
              return false;
            this.Document.Remove(backDelete ? startIndex1 : startIndex2, Math.Abs(startIndex1 - startIndex2) + 1);
            EventHandler linkTextChange = this.LinkTextChange;
            if (linkTextChange != null)
              linkTextChange((object) this, (EventArgs) null);
            return true;
          }
          if (startIndex2 + (backDelete ? 1 : 0) != caretOffset)
          {
            if (this.SelectedText.Length > 0 && str2.Contains(this.SelectedText) && this.SelectedText.Contains("(" + keyValuePair.Value.Url + ")"))
              this.SelectionLength = this.SelectedText.Replace("(" + keyValuePair.Value.Url + ")", "").Length - 1;
            return false;
          }
          if (this.SelectedText.Length > 0)
            return false;
          this.Document.Remove(backDelete ? caretOffset - (keyValuePair.Value.Url.Length + 4) : caretOffset + 1, 1);
          EventHandler linkTextChange1 = this.LinkTextChange;
          if (linkTextChange1 != null)
            linkTextChange1((object) this, (EventArgs) null);
          return true;
        }
      }
      return false;
    }

    private void OnFocused(object sender, RoutedEventArgs e)
    {
      this._currentFocused = true;
      this._parseLinkRandom = -1;
      EventHandler<RoutedEventArgs> textGotFocus = this.TextGotFocus;
      if (textGotFocus == null)
        return;
      textGotFocus(sender, e);
    }

    private void OnLostFocus(object sender, RoutedEventArgs e) => this.TryPasteLink();

    public bool TryPasteLink()
    {
      string text = this.Text;
      if (this.TextArea.IsKeyboardFocused)
        return false;
      this._currentFocused = false;
      EventHandler textLostFocus = this.TextLostFocus;
      if (textLostFocus != null)
        textLostFocus((object) this, (EventArgs) null);
      if (!this.IsMouseOver)
        this.Select(Math.Min(this.CaretOffset, this.Text.Length), 0);
      if (!this.AutoGetUrlTitle || ((int) LocalSettings.Settings.UserPreference?.GeneralConfig?.urlParseEnabled ?? 1) == 0)
        return false;
      Match urlMatch = TaskUtils.UrlRegex.Match(text);
      if (!urlMatch.Success)
        return false;
      Match match = LinkTextEditBox.LinkRegex.Match(text);
      if (match.Success && match.Index <= urlMatch.Index)
        return false;
      int random = new Random().Next(1, 10000);
      this._parseLinkRandom = random;
      new Thread(new ThreadStart(Function)).Start();
      return true;

      void Function()
      {
        try
        {
          string title = TaskUtils.GetLinkTitle(urlMatch.Value);
          if (string.IsNullOrEmpty(title))
            return;
          this.Dispatcher?.InvokeAsync((Action) (() =>
          {
            if (this.CurrentFocused || this._parseLinkRandom != random)
              return;
            this._parseLinkRandom = -1;
            if (urlMatch.Index < 0 || urlMatch.Index + urlMatch.Length > text.Length)
              return;
            string val = text.Remove(urlMatch.Index, urlMatch.Length).Insert(urlMatch.Index, "[" + title + "](" + urlMatch.Value + ")");
            if (!(this.Text == text))
              return;
            this.SetText(val);
            EventHandler linkTextChange = this.LinkTextChange;
            if (linkTextChange == null)
              return;
            linkTextChange((object) this, (EventArgs) null);
          }));
        }
        catch (Exception ex)
        {
        }
      }
    }

    public void FocusText(int caret = -1)
    {
      this.Focus();
      if (caret < 0 || caret >= this.Text.Length)
        return;
      this.CaretOffset = caret;
    }

    public void FocusEnd()
    {
      this.Focus();
      this.CaretOffset = this.Text.Length;
    }

    public void FocusFirst()
    {
      this.Focus();
      this.CaretOffset = 0;
    }

    public void CorrectSpellingError(string correct, TextSegment errorSegment)
    {
      this.CaretOffset = errorSegment.StartOffset;
      this.Document.Replace((ISegment) errorSegment, correct);
    }

    public void AddErrorToDict(string error, TextSegment errorSegment)
    {
      this.SpellCheckProvider?.Add(error);
      this.CaretOffset = errorSegment.StartOffset;
      this.Document.Insert(0, " ");
      this.Document.UndoStack.Undo();
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e) => e.Handled = true;

    public void SetCanParseDate(bool canParseDate)
    {
      if (this.CanParseDate == canParseDate)
        return;
      this.CanParseDate = canParseDate;
      List<string> validTokens = this._dateTokenGenerator?.GetValidTokens();
      // ISSUE: explicit non-virtual call
      if (validTokens != null && __nonvirtual (validTokens.Count) > 0 && LocalSettings.Settings.RemoveTimeText)
      {
        string text = this.Text;
        this.SetText(validTokens.Aggregate<string, string>(text, (Func<string, string, string>) ((current, token) => current.Replace(token, string.Empty))));
      }
      this._dateTokenGenerator?.Reset();
      this.ParsedData = (IPaserDueDate) null;
      if (canParseDate)
        return;
      this.TextArea.TextView.Redraw();
    }

    public void ClearParseDate()
    {
      if (!this.CanParseDate)
        return;
      this.CanParseDate = false;
      this._dateTokenGenerator?.Reset();
      this.ParsedData = (IPaserDueDate) null;
      this.TextArea.TextView.Redraw();
    }

    public bool CanParseDate { get; set; }

    public bool ParsingDate => this.CanParseDate && this.ParsedData != null;

    public async void ForceRender()
    {
      LinkTextEditBox sender = this;
      EventHandler<List<string>> ignoreTokenChanged = sender.IgnoreTokenChanged;
      if (ignoreTokenChanged != null)
        ignoreTokenChanged((object) sender, sender._dateTokenGenerator.GetIgnoreTokens());
      sender.TryParseDate();
    }

    public double GetFontSize() => this.TextArea.FontSize;

    public string GetColor() => ((Color) this.FindResource((object) "ColorPrimary")).ToString();

    public void RemoveTokenText(ref string text)
    {
    }

    public string GetParsedText()
    {
      string seed = this.Text.Replace("\r\n", string.Empty);
      if (this.CanParseDate && LocalSettings.Settings.RemoveTimeText)
      {
        List<string> validTokens = this._dateTokenGenerator.GetValidTokens();
        if (validTokens.Any<string>() && LocalSettings.Settings.RemoveTimeText)
          seed = validTokens.Aggregate<string, string>(seed, (Func<string, string, string>) ((current, token) => current.Replace(token, string.Empty)));
      }
      return seed;
    }

    private void OnSelectionPopupOpened(object sender, EventArgs e)
    {
      this._selectionPopupOpened = true;
      EventHandler popOpened = this.PopOpened;
      if (popOpened == null)
        return;
      popOpened(sender, (EventArgs) null);
    }

    private void OnSelectionPopupClosed(object sender, EventArgs e)
    {
      this._selectionPopupOpened = false;
      EventHandler popClosed = this.PopClosed;
      if (popClosed == null)
        return;
      popClosed(sender, (EventArgs) null);
    }

    public void SetIgnoreToken(List<string> tokens)
    {
      this._dateTokenGenerator?.SetIgnoreToken(tokens);
      this.TryParseDate();
    }

    public bool AllowQuickSet { get; set; } = true;

    public event EventHandler<string> MergeText;

    public event EventHandler<QuickSetModel> QuickItemSelected;

    private void InitPopup()
    {
      if (this.SelectionPopup != null)
        return;
      this.SelectionPopup = new Popup()
      {
        StaysOpen = false,
        AllowsTransparency = true,
        Placement = PlacementMode.Relative,
        PopupAnimation = PopupAnimation.Fade
      };
      this.SelectionPopup.PlacementTarget = (UIElement) this;
      this.SelectionPopup.Opened += new EventHandler(this.OnSelectionPopupOpened);
      this.SelectionPopup.Closed += new EventHandler(this.OnSelectionPopupClosed);
    }

    private async void TryShowQuickSetPopup()
    {
      LinkTextEditBox linkTextEditBox = this;
      if (!linkTextEditBox.AllowQuickSet)
        return;
      string mark = linkTextEditBox.TryGetMark();
      if (string.IsNullOrEmpty(mark))
      {
        if (linkTextEditBox.SelectionPopup == null)
          return;
        linkTextEditBox.SelectionPopup.IsOpen = false;
      }
      else
      {
        linkTextEditBox.InitPopup();
        if (linkTextEditBox._selectionControl == null)
        {
          linkTextEditBox._selectionControl = new QuickSelectionControl(linkTextEditBox.SelectionPopup);
          linkTextEditBox.SelectionPopup.Child = (UIElement) linkTextEditBox._selectionControl;
          linkTextEditBox._selectionControl.ItemSelected += new EventHandler<QuickSetModel>(linkTextEditBox.OnQuickItemSelect);
        }
        bool usedInCal = linkTextEditBox.IsUsedInCalendar();
        string calId = !usedInCal || !(mark == "^") && !(mark == "~") ? string.Empty : linkTextEditBox.GetAccountId();
        string avatarProjectId = !usedInCal && mark == "@" || mark == "^" || mark == "~" ? linkTextEditBox.GetAvatarProjectId() : (string) null;
        int priority = mark == "!" || mark == "！" ? linkTextEditBox.GetPriority() : -1;
        string assignee = mark == "@" ? linkTextEditBox.GetAssignee() : (string) null;
        List<string> tags = mark == "#" || mark == "＃" ? linkTextEditBox.GetTags() : (List<string>) null;
        if (linkTextEditBox._selectionControl.SetSelectionItems(mark, linkTextEditBox._filterContent, usedInCal, calId, avatarProjectId, priority, tags, assignee))
        {
          System.Windows.Point popupOffset = linkTextEditBox.GetPopupOffset();
          linkTextEditBox._selectionControl.TryShowPopup(mark, linkTextEditBox._filterContent, new System.Windows.Point(popupOffset.X + 10.0, popupOffset.Y - 12.0));
        }
        else
          linkTextEditBox._selectionControl.ClosePopup();
      }
    }

    private async void OnQuickItemSelect(object sender, QuickSetModel e)
    {
      LinkTextEditBox linkTextEditBox = this;
      linkTextEditBox.AllowQuickSet = false;
      linkTextEditBox.RemoveQuickSetText(e);
      linkTextEditBox.UpdateLayout();
      UtilLog.Info("AfterRemoveQuickSetText " + linkTextEditBox.Text);
      EventHandler<QuickSetModel> quickItemSelected = linkTextEditBox.QuickItemSelected;
      if (quickItemSelected != null)
        quickItemSelected(sender, e);
      linkTextEditBox.AllowQuickSet = true;
    }

    private System.Windows.Point GetPopupOffset()
    {
      return this.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.Document.GetLocation(this.CaretOffset)), VisualYPosition.LineBottom);
    }

    protected virtual string GetAvatarProjectId() => (string) null;

    protected virtual string GetAssignee() => (string) null;

    protected virtual int GetPriority() => -1;

    protected virtual List<string> GetTags() => (List<string>) null;

    protected virtual string GetAccountId() => "";

    protected virtual bool IsUsedInCalendar() => false;

    public void RemoveQuickSetText(QuickSetModel e)
    {
      int num = Math.Min(this.Text.Length, Math.Max(0, this._markIndex));
      this.Document.Remove(num, Math.Min(this._filterContent.Length + 1, this.Text.Length - num));
      if (!string.IsNullOrEmpty(e.Tag) && LocalSettings.Settings.KeepTagsInText)
      {
        TagModel tagByName = CacheManager.GetTagByName(e.Tag.ToLower());
        if (tagByName != null)
        {
          this.Document.Insert(num, "#" + tagByName.GetDisplayName());
          num += tagByName.GetDisplayName().Length + 1;
        }
      }
      this.Focus();
      this.TextArea.Focus();
      this.CaretOffset = Math.Min(this.Text.Length, num);
    }

    private string TryGetMark()
    {
      DocumentLine lineByOffset = this.Document.GetLineByOffset(this.CaretOffset);
      string input = string.Empty;
      if (lineByOffset != null)
      {
        if (lineByOffset.Offset < this.CaretOffset)
          input = this.Text.Substring(lineByOffset.Offset, this.CaretOffset - lineByOffset.Offset);
        if (!string.IsNullOrEmpty(input))
        {
          string[] source = input.Split(' ');
          if (source.Length != 0)
            input = ((IEnumerable<string>) source).Last<string>();
          Match match = new Regex(this.MarkRegexText).Match(input);
          if (match.Success && match.Index == 0 && match.Groups[1].Value != "|")
          {
            int num = 0;
            for (int index = 0; index < source.Length - 1; ++index)
              num += source[index].Length + 1;
            string mark = match.Groups[1].Value;
            this._filterContent = match.Groups[2].Value;
            this._markIndex = lineByOffset.Offset + num;
            return mark;
          }
        }
      }
      this._filterContent = string.Empty;
      this._markIndex = -1;
      return string.Empty;
    }

    private string MarkRegexText { get; set; } = "([#|@|~|^|!|！|\\*])(.*)";

    public void SetMarkRegexText(bool unable, bool inDetail = true, bool isNote = false)
    {
      if (unable)
        this.MarkRegexText = "";
      else if (inDetail)
        this.MarkRegexText = isNote ? "([#|@|~|^|\\*])(.*)" : "([#|@|~|^|!|！|\\*])(.*)";
      else
        this.MarkRegexText = "([#|@|~|^|!|！|\\*])(.*)";
    }

    private bool IsQuickPopupOpen() => this.SelectionPopup != null && this.SelectionPopup.IsOpen;
  }
}
