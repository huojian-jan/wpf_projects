// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.MarkDownEditor
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using CommonMark.Syntax;
using Emoji.Wpf;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.MarkDown.Colorizer;
using ticktick_WPF.Views.MarkDown.SpellCheck;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class MarkDownEditor : 
    System.Windows.Controls.UserControl,
    ILinkTextEditor,
    ISpellChecker,
    IEmojiRender,
    IComponentConnector
  {
    private static readonly Regex LinkRegex = new Regex("\\[(.*)\\]\\((.*)\\)");
    public static readonly Regex AttachmentRegex = new Regex("!\\[(image|file)\\]\\(([^\\)]+)\\)");
    public static readonly DependencyProperty SyntaxTreeProperty = DependencyProperty.Register(nameof (SyntaxTree), typeof (Block), typeof (MarkDownEditor), new PropertyMetadata((object) null));
    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(nameof (ReadOnly), typeof (bool), typeof (MarkDownEditor), new PropertyMetadata((object) false));
    public static readonly DependencyProperty EnableSpellCheckProperty = DependencyProperty.Register(nameof (EnableSpellCheck), typeof (bool), typeof (MarkDownEditor), new PropertyMetadata((object) true, new PropertyChangedCallback(MarkDownEditor.OnSpellCheckChanged)));
    public static readonly DependencyProperty ImageModeProperty = DependencyProperty.Register(nameof (ImageMode), typeof (int), typeof (MarkDownEditor), new PropertyMetadata((object) 0, new PropertyChangedCallback(MarkDownEditor.OnImageModeChanged)));
    public static readonly DependencyProperty IsDarkProperty = DependencyProperty.Register(nameof (IsDark), typeof (bool), typeof (MarkDownEditor), new PropertyMetadata((object) false, new PropertyChangedCallback(MarkDownEditor.OnIsDarkChanged)));
    public static readonly DependencyProperty LineSpacingProperty = DependencyProperty.Register(nameof (LineSpacing), typeof (double), typeof (MarkDownEditor), new PropertyMetadata((object) 6.0));
    private HighlightColorizer _colorizer;
    private readonly DelayActionHandler _textChangeNotifyHandler = new DelayActionHandler(100);
    private readonly DelayActionHandler _redrawNotifyHandler = new DelayActionHandler(200);
    private readonly DelayActionHandler _textChangeHandler = new DelayActionHandler(150);
    public readonly Dictionary<string, List<System.Windows.Point>> CodeDict = new Dictionary<string, List<System.Windows.Point>>();
    public readonly Dictionary<int, LinkInfo> LinkNameDict = new Dictionary<int, LinkInfo>();
    public readonly Dictionary<int, LinkInfo> LinkUrlDict = new Dictionary<int, LinkInfo>();
    public readonly List<Inline> MarkList = new List<Inline>();
    public readonly List<Inline> InlineCodeList = new List<Inline>();
    public readonly Dictionary<int, AttachmentInfo> AttachmentDict = new Dictionary<int, AttachmentInfo>();
    public readonly HashSet<int> AttachmentEndSet = new HashSet<int>();
    public Dictionary<int, string> EmojiDict = new Dictionary<int, string>();
    private BlockBackgroundRenderer _backgroundRenderer;
    private bool _currentFocused;
    private InsertLinkWindow _linkWindow;
    private bool _previewCaretChangedRedrew;
    private string _pasteContent;
    private int _previousLineCount;
    private int _caretBeforeMoveUp;
    private Brush _highLightColor;
    private Brush _bracketColor;
    private Brush _textColor;
    private SearchHighlightBackgroundRender _searchRender;
    private bool _inPrint;
    private string _previewText;
    private string _editingTaskId;
    private bool _inImeProcess;
    private bool? _isDark;
    private string _color100 = "BaseColorOpacity100_80";
    private string _color20 = "BaseColorOpacity20";
    private string _filterContent;
    private QuickSelectionControl _selectionControl;
    private int _markIndex;
    public Popup SelectionPopup;
    internal MarkDownEditor Root;
    internal TextEditor EditBox;
    private bool _contentLoaded;

    private static void OnIsDarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is MarkDownEditor markDownEditor))
        return;
      int num = (e.NewValue as bool?).GetValueOrDefault() ? 1 : 0;
      markDownEditor.SetThemeColor(num != 0);
    }

    private void SetThemeColor(bool isDark)
    {
      bool? isDark1 = this._isDark;
      bool flag = isDark;
      if (isDark1.GetValueOrDefault() == flag & isDark1.HasValue)
        return;
      this._isDark = new bool?(isDark);
      this.OnAppThemeChanged((object) null, (EventArgs) null);
    }

    public double LineSpacing
    {
      get => (double) this.GetValue(MarkDownEditor.LineSpacingProperty);
      set => this.SetValue(MarkDownEditor.LineSpacingProperty, (object) value);
    }

    public bool IsDark
    {
      get => (bool) this.GetValue(MarkDownEditor.IsDarkProperty);
      set => this.SetValue(MarkDownEditor.IsDarkProperty, (object) value);
    }

    private static void OnImageModeChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is MarkDownEditor markDownEditor) || !(e.NewValue is int newValue))
        return;
      markDownEditor.ImageMode = newValue;
    }

    private static void OnSpellCheckChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is MarkDownEditor markDownEditor) || !(e.NewValue is bool newValue))
        return;
      markDownEditor.EnableSpellCheck = newValue;
    }

    public bool EnableSpellCheck
    {
      get => (bool) this.GetValue(MarkDownEditor.EnableSpellCheckProperty);
      set
      {
        if (value)
          return;
        this.SpellCheckProvider?.Disconnect();
      }
    }

    public bool ReadOnly
    {
      get => (bool) this.GetValue(MarkDownEditor.ReadOnlyProperty);
      set
      {
        this.SetValue(MarkDownEditor.ReadOnlyProperty, (object) value);
        this.EditBox.IsReadOnly = value;
        this.EditBox.TextArea.Caret.CaretBrush = value ? (Brush) Brushes.Transparent : this._textColor;
      }
    }

    public int ImageMode
    {
      get => (int) this.GetValue(MarkDownEditor.ImageModeProperty);
      set
      {
        this.SetCurrentValue(MarkDownEditor.ImageModeProperty, (object) value);
        this.EditBox.TextArea.TextView.Redraw();
      }
    }

    public bool AllowTab { get; set; } = true;

    public int MaxLength { get; set; } = -1;

    public SpellCheckProvider SpellCheckProvider { get; set; }

    public bool InPrint => this._inPrint;

    public MarkDownEditor()
    {
      this.InitializeComponent();
      this.Setup();
      this.SetupMargin();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    public MarkDownEditor(bool inPrint)
    {
      this.InitializeComponent();
      this.Setup();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      this._inPrint = inPrint;
      if (string.IsNullOrEmpty(this._previewText))
        return;
      this.Text = this._previewText;
    }

    private void Setup()
    {
      this._highLightColor = (Brush) ThemeUtil.GetColor("TextAccentColor");
      this._bracketColor = (Brush) ThemeUtil.GetColor(this._color20, (FrameworkElement) this);
      this._textColor = (Brush) ThemeUtil.GetColor(this._color100);
      this._colorizer = new HighlightColorizer(this);
      this.SetupSyntaxHighlighting();
      this.SetupSpellCheck();
      this.SetupGenerators();
      this.SetupEvents();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e) => this.BindEvents();

    public bool CurrentFocused
    {
      get => this._currentFocused;
      set => this._currentFocused = value;
    }

    public bool KeyboardFocused => this.EditBox.TextArea.IsKeyboardFocused;

    private Block SyntaxTree
    {
      get => (Block) this.GetValue(MarkDownEditor.SyntaxTreeProperty);
      set => this.SetValue(MarkDownEditor.SyntaxTreeProperty, (object) value);
    }

    public string Text
    {
      get => this.EditBox.Text;
      set
      {
        if (!this._inPrint && !this.IsLoaded)
        {
          this._previewText = value;
        }
        else
        {
          this.UnRegisterInputHandler();
          this.EditBox.TextChanged -= new EventHandler(this.EditBoxOnTextChanged);
          this.OnTextSet(value);
          this.EditBox.Text = value;
          this.AfterTextSet();
          this.EditBox.TextChanged += new EventHandler(this.EditBoxOnTextChanged);
          this.RegisterInputHandler();
        }
      }
    }

    private void OnTextSet(string text)
    {
      text = text ?? string.Empty;
      this.SyntaxTree = AbstractSyntaxTree.GenerateAbstractSyntaxTree(text);
      this._colorizer?.UpdateAbstractSyntaxTree(this.SyntaxTree);
      this._backgroundRenderer?.UpdateAbstractSyntaxTree(this.SyntaxTree);
      this.LogPositions(text);
      this.LogEmojiPosition(text);
      this.LogCodeRects(text);
      this.Redraw();
    }

    private void AfterTextSet()
    {
      this.SpellCheckProvider?.SetTextChanged(true, false);
      this._searchRender?.SetTextChanged(true);
      this.InvalidMargin();
      this._previousLineCount = this.EditBox.LineCount;
    }

    private void LogEmojiPosition(string text)
    {
      this.EmojiDict.Clear();
      MatchCollection matchCollection = EmojiData.MatchOne2.Matches(text);
      List<KeyValuePair<int, int>> source = new List<KeyValuePair<int, int>>();
      foreach (KeyValuePair<int, LinkInfo> keyValuePair in this.LinkNameDict)
        source.Add(new KeyValuePair<int, int>(keyValuePair.Key, keyValuePair.Key + keyValuePair.Value.Link.Length));
      EmojiData.MatchOne2.Matches(text);
      foreach (Match match in matchCollection)
      {
        Match m = match;
        if (!source.Any<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>) (pair => m.Index >= pair.Key && m.Index <= pair.Value)))
          this.EmojiDict[m.Index] = m.Value;
      }
    }

    public Dictionary<int, string> GetEmojiDict() => this.EmojiDict;

    public event EventHandler<ProjectTask> Navigate;

    public event EventHandler<double> CaretVerticalOffsetChanged;

    public event EventHandler QuickPopupOpened;

    public event EventHandler QuickPopupClosed;

    public event EventHandler<System.Windows.Input.KeyEventArgs> KeyUp;

    public event EventHandler<System.Windows.Input.KeyEventArgs> KeyDown;

    public event EventHandler MoveUp;

    public event EventHandler MoveDown;

    public event EventHandler EscKeyUp;

    public event EventHandler LinkPopupOpened;

    public event EventHandler LinkPopupClosed;

    public event EventHandler SaveContent;

    public event EventHandler EnterImmersive;

    public event EventHandler SelectDate;

    public event EventHandler TextChanged;

    public event EventHandler<string> TaskIdChanged;

    public void SetupMargin(double width = 22.0)
    {
      this.EditBox.TextArea.LeftMargins.Clear();
      this.EditBox.TextArea.LeftMargins.Add((UIElement) new MarkDownTagMargin(this.EditBox.TextArea, this, width));
    }

    private void SetupEvents()
    {
      this.EditBox.TextChanged -= new EventHandler(this.EditBoxOnTextChanged);
      this.EditBox.TextChanged += new EventHandler(this.EditBoxOnTextChanged);
      this.EditBox.Options.IndentationSize = 4;
      this.EditBox.Options.EnableHyperlinks = false;
      this.EditBox.Options.ConvertTabsToSpaces = true;
      this.EditBox.Options.AllowScrollBelowDocument = true;
      this.EditBox.Options.EnableEmailHyperlinks = false;
      this.EditBox.Options.InheritWordWrapIndentation = false;
      this.EditBox.PreviewKeyDown -= new System.Windows.Input.KeyEventHandler(this.OnEditorKeyDown);
      this.EditBox.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.OnEditorKeyDown);
      this.EditBox.PreviewKeyUp -= new System.Windows.Input.KeyEventHandler(this.OnEditKeyUp);
      this.EditBox.PreviewKeyUp += new System.Windows.Input.KeyEventHandler(this.OnEditKeyUp);
      this.EditBox.GotFocus -= new RoutedEventHandler(this.OnEditorGotFocus);
      this.EditBox.GotFocus += new RoutedEventHandler(this.OnEditorGotFocus);
      this.EditBox.LostFocus -= new RoutedEventHandler(this.OnEditorLostFocus);
      this.EditBox.LostFocus += new RoutedEventHandler(this.OnEditorLostFocus);
      this.EditBox.TextArea.Caret.PositionChanged -= new EventHandler(this.OnCaretPositionChanged);
      this.EditBox.TextArea.Caret.PositionChanged += new EventHandler(this.OnCaretPositionChanged);
      this.EditBox.TextArea.TextView.LinkTextForegroundBrush = (Brush) ThemeUtil.GetColor("TextAccentColor");
      System.Windows.DataObject.RemovePastingHandler((DependencyObject) this.EditBox, new DataObjectPastingEventHandler(this.OnPaste));
      System.Windows.DataObject.AddPastingHandler((DependencyObject) this.EditBox, new DataObjectPastingEventHandler(this.OnPaste));
    }

    private void BindEvents()
    {
      this._textChangeNotifyHandler.SetAction(new EventHandler(this.NotifyTextChanged));
      this._textChangeHandler.SetAction(new EventHandler(this.HandlerTextChanged));
      this._redrawNotifyHandler.SetAction(new EventHandler(this.NotifyRedraw));
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnAppThemeChanged);
      LocalSettings.SpellCheckChanged += new EventHandler(this.OnSpellCheckChanged);
    }

    private void UnbindEvents()
    {
      this._textChangeNotifyHandler.StopAndClear();
      this._textChangeHandler.StopAndClear();
      this._redrawNotifyHandler.StopAndClear();
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnAppThemeChanged);
      LocalSettings.SpellCheckChanged -= new EventHandler(this.OnSpellCheckChanged);
    }

    private void OnEditorLostFocus(object sender, RoutedEventArgs e)
    {
      this._selectionControl?.TryRemoveTaskSelector();
    }

    private void NotifyRedraw(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() => this.EditBox.TextArea.TextView.Redraw()));
    }

    private void OnSpellCheckChanged(object sender, EventArgs e)
    {
      if (!this.EnableSpellCheck)
        return;
      if (LocalSettings.Settings.SpellCheckEnable)
        this.SpellCheckProvider?.Initialize((ISpellChecker) this);
      else
        this.SpellCheckProvider?.Disconnect();
      this.EditBox.TextChanged -= new EventHandler(this.EditBoxOnTextChanged);
      this.SpellCheckProvider?.SetTextChanged(true, false);
      this.EditBox.Document.Insert(0, " ");
      this.EditBox.Document.UndoStack.Undo();
      this.EditBox.TextChanged += new EventHandler(this.EditBoxOnTextChanged);
    }

    private void OnEditKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (this._inImeProcess)
      {
        this._inImeProcess = false;
      }
      else
      {
        switch (e.Key)
        {
          case Key.Return:
            if (this.SelectionPopup != null && this.SelectionPopup.IsOpen)
            {
              this._selectionControl.TrySelectItem();
              e.Handled = true;
              return;
            }
            break;
          case Key.Up:
            if (this.SelectionPopup != null && this.SelectionPopup.IsOpen)
            {
              this._selectionControl.Move(true);
              e.Handled = true;
              return;
            }
            break;
          case Key.Down:
            if (this.SelectionPopup != null && this.SelectionPopup.IsOpen)
            {
              this._selectionControl.Move(false);
              e.Handled = true;
              return;
            }
            break;
        }
        if (this._caretBeforeMoveUp == this.EditBox.CaretOffset && !Utils.IfShiftPressed())
        {
          if (e.Key == Key.Up)
          {
            EventHandler moveUp = this.MoveUp;
            if (moveUp != null)
              moveUp(sender, (EventArgs) null);
          }
          if (e.Key == Key.Down)
          {
            EventHandler moveDown = this.MoveDown;
            if (moveDown != null)
              moveDown(sender, (EventArgs) null);
          }
        }
        EventHandler<System.Windows.Input.KeyEventArgs> keyUp = this.KeyUp;
        if (keyUp == null)
          return;
        keyUp(sender, e);
      }
    }

    private void NotifyTextChanged(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() =>
      {
        EventHandler textChanged = this.TextChanged;
        if (textChanged == null)
          return;
        textChanged(sender, e);
      }));
    }

    public void UnRegisterInputHandler()
    {
      this.EditBox.Document.PropertyChanged -= new PropertyChangedEventHandler(this.OnEditBoxPropertyChanged);
    }

    public void RegisterInputHandler()
    {
      this.EditBox.Document.PropertyChanged -= new PropertyChangedEventHandler(this.OnEditBoxPropertyChanged);
      this.EditBox.Document.PropertyChanged += new PropertyChangedEventHandler(this.OnEditBoxPropertyChanged);
    }

    private void OnEditorGotFocus(object sender, RoutedEventArgs e)
    {
      this._currentFocused = true;
      this.EditBox.TextArea.TextView.Redraw();
    }

    private void OnAppThemeChanged(object sender, EventArgs e)
    {
      this._colorizer.OnThemeChanged(new Theme((FrameworkElement) this));
      this._highLightColor = (Brush) ThemeUtil.GetColor("TextAccentColor", (FrameworkElement) this);
      this._textColor = (Brush) ThemeUtil.GetColor(this._color100, (FrameworkElement) this);
      this._bracketColor = (Brush) ThemeUtil.GetColor(this._color20, (FrameworkElement) this);
      this.EditBox.TextArea.Caret.CaretBrush = this.EditBox.IsReadOnly ? (Brush) Brushes.Transparent : this._textColor;
      this.EditBox.TextArea.TextView.LinkTextForegroundBrush = this._highLightColor;
      this.EditBox.TextArea.TextView.Redraw();
    }

    public void SetTheme(bool light = true)
    {
      ThemeUtil.SetTheme(light ? nameof (light) : "dark", (FrameworkElement) this);
      this._colorizer.OnThemeChanged(new Theme((FrameworkElement) this));
      this._highLightColor = (Brush) ThemeUtil.GetColor("TextAccentColor", (FrameworkElement) this);
      this._bracketColor = (Brush) ThemeUtil.GetColor(this._color20, (FrameworkElement) this);
      this._textColor = (Brush) ThemeUtil.GetColor(this._color100, (FrameworkElement) this);
    }

    public void ClearTheme() => this.Resources.MergedDictionaries.Clear();

    private void OnCaretPositionChanged(object sender, EventArgs e) => this.InvalidMargin();

    public void UnRegisterCaretChanged()
    {
      this.EditBox.TextArea.Caret.PositionChanged -= new EventHandler(this.NotifyCaretChanged);
    }

    public void RegisterCaretChanged()
    {
      this.EditBox.TextArea.Caret.PositionChanged -= new EventHandler(this.NotifyCaretChanged);
      this.EditBox.TextArea.Caret.PositionChanged += new EventHandler(this.NotifyCaretChanged);
    }

    private void NotifyCaretChanged(object sender, EventArgs e)
    {
      int line = this.EditBox.TextArea.Caret.GetPosition().Line;
      double e1 = this.EditBox.TextArea.TextView.GetVisualPosition(new TextViewPosition(line, Math.Max(1, this.EditBox.TextArea.Caret.Column)), VisualYPosition.LineTop).Y + 21.0;
      if (this.CurrentFocused)
      {
        EventHandler<double> verticalOffsetChanged = this.CaretVerticalOffsetChanged;
        if (verticalOffsetChanged != null)
          verticalOffsetChanged((object) this, e1);
      }
      bool flag = false;
      foreach (KeyValuePair<int, LinkInfo> keyValuePair in this.LinkNameDict)
      {
        if (keyValuePair.Key <= this.EditBox.CaretOffset && this.EditBox.CaretOffset <= keyValuePair.Key + keyValuePair.Value.Link.Length)
        {
          flag = true;
          break;
        }
      }
      DocumentLine lineByNumber = this.EditBox.Document.GetLineByNumber(line);
      string text = this.EditBox.Document.GetText(lineByNumber.Offset, lineByNumber.Length);
      Brush brush = this.EditBox.IsReadOnly ? (Brush) Brushes.Transparent : (flag ? this._highLightColor : this._textColor);
      if (!brush.Equals((object) this.EditBox.TextArea.Caret.CaretBrush))
      {
        this.EditBox.TextArea.Caret.CaretBrush = brush;
        this._redrawNotifyHandler?.TryDoAction();
      }
      else if (!Regex.IsMatch(text, "[~|*|:]"))
      {
        if (this._previewCaretChangedRedrew)
          this._redrawNotifyHandler?.TryDoAction();
        this._previewCaretChangedRedrew = false;
      }
      else
      {
        this._previewCaretChangedRedrew = true;
        this._redrawNotifyHandler?.TryDoAction();
      }
    }

    private void OnEditorKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (this.ReadOnly)
        return;
      this._inImeProcess = false;
      switch (e.Key)
      {
        case Key.Back:
          int caretOffset1 = this.EditBox.CaretOffset;
          if (caretOffset1 == 0)
          {
            ListInputHandler.TryRemoveEmptyLineOnDelete(this.EditBox);
            return;
          }
          if (this.EditBox.SelectionLength > 0)
            this.HandleSelectionBehindAttachment();
          else if (caretOffset1 > 0 && caretOffset1 <= this.Text.Length)
          {
            DocumentLine lineByOffset = this.EditBox.TextArea.Document.GetLineByOffset(caretOffset1);
            if (lineByOffset.Offset == this.EditBox.CaretOffset)
            {
              DocumentLine previousLine = lineByOffset.PreviousLine;
              if (this.AttachmentDict.ContainsKey(this.EditBox.CaretOffset) && previousLine != null && previousLine.Length > 0)
              {
                this.EditBox.CaretOffset = previousLine.EndOffset;
                e.Handled = true;
              }
              else if (previousLine != null && this.AttachmentEndSet.Contains(previousLine.EndOffset) && lineByOffset.Length > 0)
              {
                this.EditBox.CaretOffset = previousLine.EndOffset;
                e.Handled = true;
              }
            }
            else if (lineByOffset.EndOffset == this.EditBox.CaretOffset && this.AttachmentEndSet.Contains(lineByOffset.EndOffset))
            {
              this.EditBox.Select(lineByOffset.Offset, lineByOffset.Length);
              e.Handled = true;
            }
          }
          int removeLinkLength = this.GetRemoveLinkLength(caretOffset1);
          if (removeLinkLength > 0)
          {
            this.EditBox.Document.Remove(caretOffset1 - (removeLinkLength - 1), removeLinkLength - 1);
            break;
          }
          break;
        case Key.Tab:
        case Key.ImeProcessed:
          if (e.Key == Key.Tab && !this.AllowTab)
          {
            e.Handled = true;
            return;
          }
          if (this.AttachmentDict.ContainsKey(this.EditBox.CaretOffset))
          {
            this.EditBox.InsertText(this.EditBox.CaretOffset, "\n");
            --this.EditBox.CaretOffset;
            break;
          }
          if (this.AttachmentEndSet.Contains(this.EditBox.CaretOffset))
          {
            this.EditBox.InsertText(this.EditBox.CaretOffset, "\n");
            break;
          }
          break;
        case Key.Return:
        case Key.Up:
        case Key.Down:
          if (this.SelectionPopup != null && this.SelectionPopup.IsOpen)
          {
            e.Handled = true;
            return;
          }
          break;
        case Key.Escape:
          if (this.SelectionPopup != null && this.SelectionPopup.IsOpen)
          {
            e.Handled = true;
            this._selectionControl?.ClosePopup();
            return;
          }
          break;
        case Key.Delete:
          int caretOffset2 = this.EditBox.CaretOffset;
          if (this.EditBox.SelectionLength > 0)
          {
            this.HandleSelectionBehindAttachment();
            break;
          }
          if (caretOffset2 >= 0 && caretOffset2 < this.Text.Length)
          {
            DocumentLine lineByOffset = this.EditBox.TextArea.Document.GetLineByOffset(caretOffset2);
            if (lineByOffset.EndOffset == this.EditBox.CaretOffset)
            {
              DocumentLine nextLine = lineByOffset.NextLine;
              if (nextLine != null && this.AttachmentDict.ContainsKey(nextLine.Offset) && lineByOffset.Length > 0)
              {
                this.EditBox.CaretOffset = nextLine.Offset;
                e.Handled = true;
                break;
              }
              if (this.AttachmentEndSet.Contains(caretOffset2) && nextLine != null && nextLine.Length > 0)
              {
                this.EditBox.CaretOffset = nextLine.Offset;
                e.Handled = true;
                break;
              }
              break;
            }
            if (lineByOffset.Offset == this.EditBox.CaretOffset && this.AttachmentEndSet.Contains(lineByOffset.EndOffset))
            {
              this.EditBox.Select(lineByOffset.Offset, lineByOffset.Length);
              e.Handled = true;
              break;
            }
            break;
          }
          break;
        case Key.X:
          if (Utils.IfCtrlPressed() && this.EditBox.SelectionLength > 0)
          {
            this.HandleSelectionBehindAttachment();
            break;
          }
          break;
      }
      this._inImeProcess = e.ImeProcessedKey != 0;
      if (e.Key == Key.Tab)
      {
        if (string.IsNullOrEmpty(this.EditBox.SelectedText))
        {
          Regex regex = new Regex("(\\d+)\\. ");
          DocumentLine lineByOffset = this.EditBox.Document.GetLineByOffset(this.EditBox.CaretOffset);
          string text = this.EditBox.Document.GetText(lineByOffset.Offset, lineByOffset.Length);
          if (text.TrimStart().StartsWith("- ") || text.TrimStart().StartsWith("* ") || text.TrimStart().StartsWith("+ ") || regex.IsMatch(text.TrimStart()))
          {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None)
              this.OutIndentLine();
            else
              this.IndentLine();
            e.Handled = true;
          }
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None)
        {
          EditorUtilities.OutIndentList(this.EditBox);
          this.EditBox.AdjustNumberedList();
          e.Handled = true;
        }
        else
        {
          EditorUtilities.IndentList(this.EditBox);
          this.EditBox.AdjustNumberedList();
          e.Handled = true;
        }
      }
      if (Keyboard.Modifiers == ModifierKeys.Control)
      {
        if (e.Key == Key.I)
        {
          this.Italic();
          e.Handled = true;
        }
        if (LocalSettings.Settings.ShortCutModel.SetDate == "Ctrl+D" && e.Key == Key.D)
        {
          e.Handled = true;
          EventHandler selectDate = this.SelectDate;
          if (selectDate != null)
            selectDate((object) this, (EventArgs) null);
        }
      }
      int key = (int) e.Key;
      if (e.Key == Key.Up || e.Key == Key.Down)
        this._caretBeforeMoveUp = this.EditBox.CaretOffset;
      EventHandler<System.Windows.Input.KeyEventArgs> keyDown = this.KeyDown;
      if (keyDown == null)
        return;
      keyDown((object) this, e);
    }

    private void HandleSelectionBehindAttachment()
    {
      if (this.AttachmentEndSet.Contains(this.EditBox.SelectionStart))
      {
        DocumentLine nextLine = this.EditBox.TextArea.Document.GetLineByOffset(this.EditBox.SelectionStart).NextLine;
        if (nextLine == null)
          return;
        if (this.EditBox.CaretOffset == this.EditBox.SelectionStart)
          this.EditBox.CaretOffset = nextLine.Offset;
        this.EditBox.SelectionLength = Math.Max(0, this.EditBox.SelectionLength - (nextLine.Offset - this.EditBox.SelectionStart));
        this.EditBox.SelectionStart = nextLine.Offset;
      }
      else
      {
        if (!this.AttachmentDict.ContainsKey(this.EditBox.SelectionStart + this.EditBox.SelectionLength))
          return;
        int offset = this.EditBox.SelectionStart + this.EditBox.SelectionLength;
        DocumentLine previousLine = this.EditBox.TextArea.Document.GetLineByOffset(offset).PreviousLine;
        if (previousLine == null)
          return;
        if (this.EditBox.CaretOffset == offset)
          this.EditBox.CaretOffset = previousLine.EndOffset;
        this.EditBox.SelectionLength = Math.Max(0, previousLine.EndOffset - this.EditBox.SelectionStart);
      }
    }

    private int GetRemoveLinkLength(int caret)
    {
      if (this.LinkUrlDict.Any<KeyValuePair<int, LinkInfo>>())
      {
        int startIndex = this.EditBox.Text.LastIndexOf("[", caret, StringComparison.Ordinal);
        if (startIndex >= 0)
        {
          string link = this.EditBox.Text.Substring(startIndex, caret - startIndex);
          if (this.LinkNameDict.Select<KeyValuePair<int, LinkInfo>, string>((Func<KeyValuePair<int, LinkInfo>, string>) (info => "[" + info.Value.Link + "](" + info.Value.Url + ")")).Any<string>((Func<string, bool>) (content => link == content)))
            return link.Length;
        }
      }
      return -1;
    }

    private void SetupGenerators()
    {
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new CustomLinkElementGenerator((ILinkTextEditor) this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new CheckElementGenerator(this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new UnCheckElementGenerator(this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new HeadingElementGenerator(this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new ListElementGenerator(this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new NumberedListElementGenerator(this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new QuoteElementGenerator());
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new SplitLineElementGenerator(this.EditBox));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkNameGenerator((ILinkTextEditor) this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkUrlGenerator((ILinkTextEditor) this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new AutoHideSymbolGenerator(this, true));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new AutoHideSymbolGenerator(this, false));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new ImageElementGenerator(this));
      this.EditBox.TextArea.TextView.ElementGenerators.Insert(0, (VisualLineElementGenerator) new EmojiGenerator((IEmojiRender) this));
      this.AddLinkBracketElementGenerator();
    }

    private void AddLinkBracketElementGenerator()
    {
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkBracketGenerator((ILinkTextEditor) this, true, true));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkBracketGenerator((ILinkTextEditor) this, false, true));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkBracketGenerator((ILinkTextEditor) this, true, false));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkBracketGenerator((ILinkTextEditor) this, false, false));
    }

    public void Bold() => this.AddOrRemoveText("**");

    public void Italic() => this.AddOrRemoveText("*");

    public void Code() => this.AddOrRemoveText("`");

    public void UnderLine() => this.AddOrRemoveText("~");

    public void StrokeLine() => this.AddOrRemoveText("~~");

    public new void AddText(string param) => this.EditBox.AddText(param, this._currentFocused);

    public void Highlight() => this.AddOrRemoveText("::");

    public void ToggleCheckItem()
    {
      TextArea textArea = this.EditBox.TextArea;
      TextDocument document = this.EditBox.Document;
      Selection selection = textArea.Selection;
      if (selection != null)
      {
        TextViewPosition textViewPosition = selection.StartPosition;
        if (textViewPosition.Line > 1)
        {
          textViewPosition = selection.StartPosition;
          int line1 = textViewPosition.Line;
          textViewPosition = selection.EndPosition;
          int line2 = textViewPosition.Line;
          int num1 = Math.Min(line1, line2);
          textViewPosition = selection.StartPosition;
          int line3 = textViewPosition.Line;
          textViewPosition = selection.EndPosition;
          int line4 = textViewPosition.Line;
          int num2 = Math.Max(line3, line4);
          for (int currentLine = num1; currentLine <= num2; ++currentLine)
            MarkDownEditor.ToggleLineCheckItem(document, currentLine);
          return;
        }
      }
      MarkDownEditor.ToggleLineCheckItem(document, textArea.Caret.Line);
    }

    public void TrySaveContent()
    {
      EventHandler saveContent = this.SaveContent;
      if (saveContent == null)
        return;
      saveContent((object) this, (EventArgs) null);
    }

    private static void ToggleLineCheckItem(TextDocument document, int currentLine)
    {
      DocumentLine lineByNumber = document.GetLineByNumber(currentLine);
      string text = document.GetText(lineByNumber.Offset, lineByNumber.Length);
      if (new Regex("- \\[x\\]").IsMatch(text))
      {
        int num = text.IndexOf("]", StringComparison.Ordinal);
        document.Replace(lineByNumber.Offset + num - 1, 1, " ");
      }
      if (!new Regex("- \\[ \\]").IsMatch(text))
        return;
      int num1 = text.IndexOf("]", StringComparison.Ordinal);
      document.Replace(lineByNumber.Offset + num1 - 1, 1, "x");
    }

    public void InsertLine() => EditorUtilities.AddHorizontalLine(this.EditBox);

    private async Task AddOrRemoveText(string quote)
    {
      if (this.CaretNearAttachment() || this.ReadOnly)
        return;
      this.EditBox.AddRemoveText(quote, this._currentFocused || this.EditBox.SelectionLength > 0);
      await Task.Delay(100);
      this.FocusEditBox();
    }

    public void Quote()
    {
      if (this.CaretNearAttachment() || this.ReadOnly)
        return;
      EditorUtilities.AddRemoveTextInStart(this.EditBox, "> ");
    }

    public void InsertHeader(int level)
    {
      if (this.CaretNearAttachment() || this.ReadOnly)
        return;
      EditorUtilities.AddOrRemoveHeading(this.EditBox, new string('#', level) + " ");
    }

    public void InsertUnOrderList()
    {
      if (this.CaretNearAttachment() || this.ReadOnly)
        return;
      EditorUtilities.AddRemoveTextInStart(this.EditBox, "* ");
    }

    public void InsertCheckItem()
    {
      if (this.CaretNearAttachment() || this.ReadOnly)
        return;
      EditorUtilities.AddRemoveTextInStart(this.EditBox, "- [ ] ");
    }

    public void InsertNumberedList()
    {
      if (this.CaretNearAttachment())
        return;
      if (this.ReadOnly)
        return;
      try
      {
        this.EditBox.BeginChange();
        if (string.IsNullOrEmpty(this.EditBox.SelectedText))
        {
          DocumentLine lineByOffset = this.EditBox.Document.GetLineByOffset(this.EditBox.CaretOffset);
          if (lineByOffset == null)
            return;
          Regex regex = new Regex("^(\\d+)\\. ");
          string content = this.EditBox.Document.Text.Substring(lineByOffset.Offset, lineByOffset.Length);
          string input = content;
          Match match = regex.Match(input);
          if (match.Success)
          {
            int length = match.Groups[0].Length;
            this.EditBox.Document.Remove(lineByOffset.Offset, length);
            this.FocusEditBox();
          }
          else
          {
            int offset;
            int length;
            if (EditorUtilities.IsListTypeText(content, out offset, out length))
            {
              if (offset < 0)
                offset = 0;
              this.EditBox.Document.Remove(lineByOffset.Offset + offset, length);
            }
            this.EditBox.Document.Insert(lineByOffset.Offset, "1. ");
            this.FocusEditBox();
          }
        }
        else
          EditorUtilities.ConvertSelectionToList(this.EditBox);
      }
      catch (Exception ex)
      {
      }
      finally
      {
        this.EditBox.EndChange();
      }
    }

    public void Indent()
    {
      if (string.IsNullOrEmpty(this.EditBox.SelectedText))
        this.IndentLine();
      else
        EditorUtilities.IndentList(this.EditBox);
    }

    public void DeIndent()
    {
      if (string.IsNullOrEmpty(this.EditBox.SelectedText))
        this.OutIndentLine();
      else
        EditorUtilities.OutIndentList(this.EditBox);
    }

    private void IndentLine()
    {
      DocumentLine lineByOffset = this.EditBox.Document.GetLineByOffset(this.EditBox.CaretOffset);
      if (lineByOffset != null)
        this.EditBox.Document.Insert(lineByOffset.Offset, "    ");
      this.EditBox.AdjustNumberedList();
    }

    private void OutIndentLine()
    {
      DocumentLine lineByOffset = this.EditBox.Document.GetLineByOffset(this.EditBox.CaretOffset);
      if (lineByOffset != null && this.EditBox.Document.GetText((ISegment) lineByOffset).StartsWith("    "))
        this.EditBox.Document.Remove(lineByOffset.Offset, 4);
      this.EditBox.AdjustNumberedList();
    }

    public void ShowInsertLink(string name, string url, VisualLine line = null, bool isNew = true)
    {
      if (this.ReadOnly || this.CaretNearAttachment())
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
      MarkDownEditor markDownEditor1 = this;
      if (!string.IsNullOrEmpty(name) && name.Contains("\n"))
        return;
      EventHandler linkPopupOpened = markDownEditor1.LinkPopupOpened;
      if (linkPopupOpened != null)
        linkPopupOpened((object) markDownEditor1, (EventArgs) null);
      System.Windows.Point startPosition = markDownEditor1.GetStartPosition(line, 320, 210);
      MarkDownEditor markDownEditor2 = markDownEditor1;
      InsertLinkWindow insertLinkWindow = new InsertLinkWindow((ILinkTextEditor) markDownEditor1, name, url, isNew);
      insertLinkWindow.Left = startPosition.X + 10.0;
      insertLinkWindow.Top = startPosition.Y + 10.0;
      markDownEditor2._linkWindow = insertLinkWindow;
      markDownEditor1._linkWindow.Closed -= new EventHandler(markDownEditor1.OnLinkClosed);
      markDownEditor1._linkWindow.Closed += new EventHandler(markDownEditor1.OnLinkClosed);
      markDownEditor1._linkWindow.ShowDialog();
    }

    private void OnLinkClosed(object sender, EventArgs e)
    {
      EventHandler linkPopupClosed = this.LinkPopupClosed;
      if (linkPopupClosed == null)
        return;
      linkPopupClosed(sender, e);
    }

    private void EditBoxOnTextChanged(object sender, EventArgs eventArgs)
    {
      this._searchRender?.SetTextChanged(true);
      this.SpellCheckProvider?.SetTextChanged(true);
      if (!SearchHelper.DetailChanged)
      {
        SearchHighlightBackgroundRender searchRender = this._searchRender;
        if ((searchRender != null ? (searchRender.InDetail ? 1 : 0) : 0) != 0)
          SearchHelper.DetailChanged = true;
      }
      if (this.EditBox.LineCount < 200)
        this.HandleOnTextChanged();
      else
        this._textChangeHandler.TryDoAction();
      this._textChangeNotifyHandler.TryDoAction();
    }

    private void HandlerTextChanged(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.HandleOnTextChanged));
    }

    private void SetupSpellCheck()
    {
      if (!this.EnableSpellCheck)
        return;
      if (SpellingService.CommonSpellingService == null)
        SpellingService.CommonSpellingService = new SpellingService();
      this.SpellCheckProvider = new SpellCheckProvider((ISpellingService) SpellingService.CommonSpellingService);
      if (!LocalSettings.Settings.SpellCheckEnable)
        return;
      this.SpellCheckProvider.Initialize((ISpellChecker) this);
    }

    public void SetupSearchRender(bool isTitle, bool inDetail)
    {
      if (this._searchRender != null)
        return;
      this._searchRender = new SearchHighlightBackgroundRender(this.EditBox, this.LinkUrlDict, isTitle, inDetail);
      this.EditBox.TextArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer) this._searchRender);
      SearchHelper.SearchKeyChanged += new EventHandler(this.OnSearchKeyChanged);
    }

    private void OnSearchKeyChanged(object sender, EventArgs e)
    {
      this._searchRender.SetTextChanged(true);
      this.EditBox.TextArea.TextView.Redraw();
    }

    public double GetFirstSearchIndex()
    {
      int offset = (int?) this._searchRender?.GetFirstIndex() ?? -1;
      return offset >= 0 && offset < this.Text.Length ? this.EditBox.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.EditBox.Document.GetLocation(offset)), VisualYPosition.LineBottom).Y + 21.0 : -1.0;
    }

    private void SetupSyntaxHighlighting()
    {
      this.EditBox.TextChanged -= new EventHandler(this.EditBoxOnTextChanged);
      this.EditBox.TextChanged += new EventHandler(this.EditBoxOnTextChanged);
      this.EditBox.TextArea.TextView.LineTransformers.Add((IVisualLineTransformer) this._colorizer);
      this._backgroundRenderer = new BlockBackgroundRenderer(this);
      this.EditBox.TextArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer) this._backgroundRenderer);
      if (this._searchRender == null)
        return;
      this.EditBox.TextArea.TextView.BackgroundRenderers.Remove((IBackgroundRenderer) this._searchRender);
      this.EditBox.TextArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer) this._searchRender);
    }

    private void HandleOnTextChanged()
    {
      try
      {
        this.OnTextSet(this.Text);
        this.InvalidMargin();
        this.TryShowQuickSetPopup();
      }
      catch (Exception ex)
      {
      }
    }

    private void LogCodeRects(string text)
    {
      this.CodeDict.Clear();
      foreach (Block enumerateSpanningBlock in AbstractSyntaxTree.EnumerateSpanningBlocks(this.SyntaxTree, 0, text.Length))
      {
        if (enumerateSpanningBlock.Tag == BlockTag.FencedCode)
        {
          string info = enumerateSpanningBlock.FencedCodeData.Info;
          System.Windows.Point point = new System.Windows.Point((double) enumerateSpanningBlock.SourcePosition, (double) (enumerateSpanningBlock.SourcePosition + enumerateSpanningBlock.SourceLength));
          if (this.CodeDict.ContainsKey(info))
            this.CodeDict[info].Add(point);
          else
            this.CodeDict.Add(info, new List<System.Windows.Point>()
            {
              point
            });
        }
      }
      if (this.CodeDict == null || !this.CodeDict.Any<KeyValuePair<string, List<System.Windows.Point>>>())
        return;
      foreach (IVisualLineTransformer visualLineTransformer in this.CodeDict.Keys.Select<string, IVisualLineTransformer>(new Func<string, IVisualLineTransformer>(this.TryCreateCodeColorizer)).Where<IVisualLineTransformer>((Func<IVisualLineTransformer, bool>) (colorizer => colorizer != null)))
        this.EditBox.TextArea.TextView.LineTransformers.Add(visualLineTransformer);
    }

    private IVisualLineTransformer TryCreateCodeColorizer(string lang)
    {
      if (lang.ToLower() == "js")
        lang = "javascript";
      if (lang.ToLower() == "c#")
        lang = "csharp";
      if (lang.ToLower() == "c++" || lang.ToLower() == "c")
        lang = "cpp";
      if (lang.ToLower().Contains("sql"))
        lang = "sql";
      string str = ThemeKey.IsDarkTheme(LocalSettings.Settings.ThemeId) ? "dark" : "light";
      IHighlightingDefinition definition;
      using (Stream manifestResourceStream = typeof (MarkDownEditor).Assembly.GetManifestResourceStream("ticktick_WPF.Views.MarkDown.Colorizer.Resources." + lang.ToLower() + "_" + str + ".xshd"))
      {
        if (manifestResourceStream == null)
          return (IVisualLineTransformer) null;
        using (XmlReader reader = (XmlReader) new XmlTextReader(manifestResourceStream))
          definition = HighlightingLoader.Load(reader, (IHighlightingDefinitionReferenceResolver) HighlightingManager.Instance);
      }
      return (IVisualLineTransformer) new CodeColorizer(this, definition);
    }

    private void LogPositions(string content)
    {
      string editingTaskId = this._editingTaskId;
      this.LinkNameDict.Clear();
      this.LinkUrlDict.Clear();
      this.InlineCodeList.Clear();
      this.MarkList.Clear();
      this.AttachmentDict.Clear();
      this.AttachmentEndSet.Clear();
      int length = content.Length;
      foreach (Block enumerateSpanningBlock in AbstractSyntaxTree.EnumerateSpanningBlocks(this.SyntaxTree, 0, length))
      {
        foreach (Inline enumerateInline in AbstractSyntaxTree.EnumerateInlines(enumerateSpanningBlock.InlineContent))
        {
          switch (enumerateInline.Tag)
          {
            case InlineTag.Code:
              this.InlineCodeList.Add(enumerateInline);
              continue;
            case InlineTag.Emphasis:
            case InlineTag.Strong:
            case InlineTag.Strikethrough:
            case InlineTag.HighLight:
            case InlineTag.UnderLine:
              this.MarkList.Add(enumerateInline);
              continue;
            case InlineTag.Link:
              string input = content.Substring(enumerateInline.SourcePosition, enumerateInline.SourceLength);
              if (input.Contains("["))
              {
                int num = input.IndexOf("[", StringComparison.Ordinal);
                input = content.Substring(enumerateInline.SourcePosition + num, Math.Min(length - enumerateInline.SourcePosition - num, enumerateInline.SourceLength));
              }
              Match match = MarkDownEditor.LinkRegex.Match(input);
              if (match.Success)
              {
                string link = match.Groups[1].Value;
                string url = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(url))
                {
                  LinkInfo linkInfo = new LinkInfo(link, url);
                  this.LinkNameDict[enumerateInline.SourcePosition + match.Groups[1].Index] = linkInfo;
                  this.LinkUrlDict[enumerateInline.SourcePosition + match.Groups[2].Index] = linkInfo;
                  continue;
                }
                continue;
              }
              continue;
            case InlineTag.Image:
              if (!string.IsNullOrEmpty(editingTaskId))
              {
                string str1 = content.Substring(enumerateInline.SourcePosition, enumerateInline.SourceLength);
                if (enumerateInline.SourcePosition + enumerateInline.SourceLength <= length)
                {
                  string targetUrl = enumerateInline.TargetUrl;
                  bool flag = Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute);
                  if (!flag && str1.ToLower().StartsWith("![file](") && str1.EndsWith(")"))
                  {
                    if (!string.IsNullOrEmpty(targetUrl))
                    {
                      AttachmentInfo attachmentInfo = new AttachmentInfo(str1, "file", targetUrl, enumerateInline.SourcePosition);
                      this.AttachmentDict.Add(attachmentInfo.Offset, attachmentInfo);
                      this.AttachmentEndSet.Add(attachmentInfo.Offset + str1.Length);
                      continue;
                    }
                    continue;
                  }
                  if (!string.IsNullOrEmpty(targetUrl))
                  {
                    if (flag)
                    {
                      if (AttachmentProvider.GetFileType(Path.GetFileName(targetUrl)) == Constants.AttachmentKind.IMAGE)
                      {
                        int hashCode = targetUrl.GetHashCode();
                        string str2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Tick_Tick\\Image\\" + hashCode.ToString() + ".png";
                        if (!ImageElementGenerator.NotImageUrlSet.Contains(hashCode) && (!System.IO.File.Exists(str2) || new FileInfo(str2).Length == 0L))
                          this.TryDownLoadUrlImage(targetUrl, str2);
                      }
                      else
                        continue;
                    }
                    this.AttachmentDict.Add(enumerateInline.SourcePosition, new AttachmentInfo(str1, "image", targetUrl, enumerateInline.SourcePosition));
                    this.AttachmentEndSet.Add(enumerateInline.SourcePosition + str1.Length);
                    continue;
                  }
                  continue;
                }
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      if (this._inPrint)
        return;
      AttachmentDao.CheckTaskAttachment(editingTaskId, this.AttachmentDict.Values.ToList<AttachmentInfo>());
    }

    private async void TryDownLoadUrlImage(string matchUrl, string filePath)
    {
      MarkDownEditor editor = this;
      new Thread((ThreadStart) (async () =>
      {
        try
        {
          byte[] buffer = new WebClient().DownloadData(matchUrl);
          BitmapImage source = new BitmapImage();
          using (MemoryStream memoryStream = new MemoryStream(buffer))
          {
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.StreamSource = (Stream) memoryStream;
            source.EndInit();
            source.Freeze();
          }
          if (!source.IsDownloading && source.Height > 1.0 && source.Width > 1.0)
          {
            BitmapEncoder bitmapEncoder = (BitmapEncoder) new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource) source));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
              bitmapEncoder.Save((Stream) fileStream);
              fileStream.Close();
            }
            editor.Dispatcher?.Invoke((Action) (() => editor.EditBox.TextArea.TextView.Redraw()));
          }
          else
            ImageElementGenerator.NotImageUrlSet.Add(matchUrl.GetHashCode());
        }
        catch (NotSupportedException ex)
        {
        }
        catch (Exception ex)
        {
        }
      })).Start();
    }

    private void InvalidMargin()
    {
      foreach (UIElement leftMargin in (Collection<UIElement>) this.EditBox.TextArea.LeftMargins)
        leftMargin.InvalidateVisual();
    }

    private async void OnPaste(object sender, DataObjectPastingEventArgs arg)
    {
      try
      {
        arg.CancelCommand();
        if (this.EditBox.SelectionLength > 0)
          this.HandleSelectionBehindAttachment();
        string text = (string) arg.SourceDataObject.GetData(System.Windows.DataFormats.UnicodeText, true);
        if (this.MaxLength >= 0 && text != null && this.Text.Length + text.Length - this.EditBox.SelectionLength > this.MaxLength)
          text = text.Substring(0, Math.Max(0, this.MaxLength - this.Text.Length + this.EditBox.SelectionLength));
        if (string.IsNullOrEmpty(text))
          return;
        if (this.AttachmentDict.ContainsKey(this.EditBox.CaretOffset))
        {
          this.EditBox.InsertText(this.EditBox.CaretOffset, "\n");
          --this.EditBox.CaretOffset;
        }
        else if (this.AttachmentEndSet.Contains(this.EditBox.CaretOffset))
          this.EditBox.InsertText(this.EditBox.CaretOffset, "\n");
        ProjectTask extra = TaskUtils.ParseTaskUrl(text) ?? TaskUtils.ParseTaskUrlWithoutTitle(text);
        if (extra != null && !text.StartsWith("[") && !text.EndsWith(")"))
        {
          string text1 = text.Replace(" " + extra.Title, string.Empty);
          await this.TryPasteLinkTask(extra, text1);
        }
        else
        {
          string str1 = await this.HandlePasteAttachment(text);
          if (str1 == null)
            return;
          if (!string.IsNullOrEmpty(str1))
            text = str1;
          string str2 = text.TrimEnd();
          if (text.Length - str2.Length > 100)
            text = str2;
          int offset = this.EditBox.CaretOffset;
          if (this.EditBox.SelectionLength > 0)
          {
            if (this.EditBox.CaretOffset >= this.EditBox.SelectionStart && this.EditBox.CaretOffset <= this.EditBox.SelectionStart + this.EditBox.SelectionLength)
              offset = this.EditBox.SelectionStart;
            if (this.EditBox.CaretOffset > this.EditBox.SelectionStart + this.EditBox.SelectionLength)
              offset = this.EditBox.SelectionStart + (this.EditBox.CaretOffset - (this.EditBox.SelectionStart + this.EditBox.SelectionLength));
            this.EditBox.Document.Remove(this.EditBox.SelectionStart, this.EditBox.SelectionLength);
          }
          this.EditBox.Document.Insert(offset, text);
          this.EditBox.CaretOffset = offset + text.Length;
          if (str1 == string.Empty)
          {
            await Task.Delay(5);
            this.CheckIfExtraPaste(text);
          }
          await Task.Delay(5);
          this.NotifyOffsetChanged();
        }
        text = (string) null;
      }
      catch (Exception ex)
      {
      }
    }

    private void NotifyOffsetChanged()
    {
      double e = this.EditBox.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.EditBox.TextArea.Caret.GetPosition().Line, Math.Max(1, this.EditBox.TextArea.Caret.Column)), VisualYPosition.LineTop).Y + 21.0;
      if (!this.CurrentFocused)
        return;
      EventHandler<double> verticalOffsetChanged = this.CaretVerticalOffsetChanged;
      if (verticalOffsetChanged == null)
        return;
      verticalOffsetChanged((object) this, e);
    }

    private async Task<string> HandlePasteAttachment(string text)
    {
      MarkDownEditor child = this;
      if (!string.IsNullOrEmpty(child._editingTaskId))
      {
        List<AttachmentInfo> attachmentInfos = child.GetSelectedAttachment();
        text = text.Replace("\r\n", "\n");
        Dictionary<Match, AttachmentModel> pastAttachments = child.GetAttachmentsInText(text);
        if (pastAttachments.Count > 0)
        {
          TaskDetailView parent = Utils.FindParent<TaskDetailView>((DependencyObject) child);
          bool flag = parent != null;
          if (flag)
          {
            TaskDetailView taskDetailView = parent;
            int count1 = pastAttachments.Count;
            List<AttachmentInfo> attachmentInfoList = attachmentInfos;
            // ISSUE: explicit non-virtual call
            int count2 = attachmentInfoList != null ? __nonvirtual (attachmentInfoList.Count) : 0;
            flag = !await taskDetailView.CheckAttachmentLimit(count1, count2);
          }
          if (flag)
            return (string) null;
          DocumentLine line = child.EditBox.TextArea.Document.GetLineByOffset(child.EditBox.CaretOffset);
          StringBuilder newText = new StringBuilder();
          int index = 0;
          List<Match> list1 = pastAttachments.Keys.ToList<Match>();
          list1.Sort((Comparison<Match>) ((a, b) => a.Index.CompareTo(b.Index)));
          foreach (Match match1 in list1)
          {
            Match match = match1;
            if (match.Index == 0 && line.Offset != child.EditBox.CaretOffset)
              newText.Insert(0, "\n");
            AttachmentModel otherAttachment = pastAttachments[match];
            List<AttachmentInfo> source = attachmentInfos;
            List<AttachmentInfo> list2 = source != null ? source.Where<AttachmentInfo>((Func<AttachmentInfo, bool>) (info => string.Equals(info.Value, match.Value, StringComparison.CurrentCultureIgnoreCase))).ToList<AttachmentInfo>() : (List<AttachmentInfo>) null;
            string str = string.Empty;
            // ISSUE: explicit non-virtual call
            if (list2 != null && __nonvirtual (list2.Count) > 0)
              list2.ForEach((Action<AttachmentInfo>) (info => attachmentInfos.Remove(info)));
            else if (otherAttachment.taskId != child._editingTaskId || otherAttachment.status != 1 || otherAttachment.deleted)
              str = Utils.GetGuid();
            newText.Append(text.Substring(index, match.Index - index));
            index = match.Index + match.Length;
            if (!string.IsNullOrEmpty(str))
            {
              newText.Append(match.Value.Replace(otherAttachment.id, str));
              await AttachmentDao.CopyAttachment(otherAttachment, str, child._editingTaskId);
            }
            else
              newText.Append(match.Value);
            if (match.Index + match.Length == text.Length && line.EndOffset != child.EditBox.CaretOffset)
              newText.Append("\n");
          }
          if (index != text.Length)
            newText.Append(text.Substring(index));
          text = newText.ToString();
          return text;
        }
        pastAttachments = (Dictionary<Match, AttachmentModel>) null;
      }
      return string.Empty;
    }

    private Dictionary<Match, AttachmentModel> GetAttachmentsInText(string text)
    {
      Dictionary<Match, AttachmentModel> attachmentsInText = new Dictionary<Match, AttachmentModel>();
      MatchCollection matchCollection = MarkDownEditor.AttachmentRegex.Matches(text.ToLower());
      for (int i = 0; i < matchCollection.Count; ++i)
      {
        Match key = matchCollection[i];
        if (key.Index <= 0 || key.Index + key.Length >= text.Length || text[key.Index - 1] == '\n' && text[key.Index + key.Length] == '\n')
        {
          AttachmentModel attachmentInUrl = AttachmentDao.GetAttachmentInUrl(key.Groups[2].Value);
          if (attachmentInUrl != null)
            attachmentsInText.Add(key, attachmentInUrl);
        }
      }
      return attachmentsInText;
    }

    private List<AttachmentInfo> GetSelectedAttachment()
    {
      return this.EditBox.SelectionLength <= 0 ? (List<AttachmentInfo>) null : this.AttachmentDict.Values.ToList<AttachmentInfo>().Where<AttachmentInfo>((Func<AttachmentInfo, bool>) (info => !Uri.IsWellFormedUriString(info.Url, UriKind.Absolute) && this.EditBox.SelectionStart <= info.Offset && this.EditBox.SelectionStart + this.EditBox.SelectionLength >= info.Offset + info.Value.Length && AttachmentDao.GetAttachmentInUrl(info.Url) != null)).ToList<AttachmentInfo>();
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

    private void CheckIfExtraPaste(string content)
    {
      TextEditor editBox = this.EditBox;
      string checkTextExtra = this.GetCheckTextExtra(content);
      string str = content + checkTextExtra;
      if (!editBox.Text.Contains(str) || string.IsNullOrEmpty(checkTextExtra))
        return;
      int offset = editBox.Document.Text.IndexOf(str, StringComparison.Ordinal);
      editBox.Document.Replace(offset, str.Length, content);
    }

    private string GetCheckTextExtra(string content)
    {
      using (IEnumerator<string> enumerator = new List<string>()
      {
        "- [ ]",
        "- [x]",
        "- [X]",
        "- ",
        "+ ",
        "* ",
        "> "
      }.Where<string>(new Func<string, bool>(content.StartsWith)).GetEnumerator())
      {
        if (enumerator.MoveNext())
          return enumerator.Current;
      }
      Match match = new Regex("(\\d+)\\. ", RegexOptions.Compiled).Match(((IEnumerable<string>) content.Split('\n')).Last<string>());
      return !match.Success ? string.Empty : match.Groups[0].Value;
    }

    private void OnEditBoxPropertyChanged(
      object sender,
      PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (!(propertyChangedEventArgs.PropertyName == "LineCount"))
        return;
      int lineCount = this.EditBox.LineCount;
      if (this._previousLineCount != -1)
        ListInputHandler.AdjustList(this.EditBox, lineCount > this._previousLineCount);
      this._previousLineCount = lineCount;
    }

    private System.Windows.Point GetStartPosition(VisualLine line = null, int width = 245, int height = 140)
    {
      TextView textView = this.EditBox.TextArea.TextView;
      TextViewPosition position = new TextViewPosition(this.EditBox.Document.GetLocation(this.EditBox.CaretOffset));
      if (line != null)
        position = new TextViewPosition(this.EditBox.Document.GetLocation(line.StartOffset));
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

    public void ExitEditor()
    {
      EventHandler escKeyUp = this.EscKeyUp;
      if (escKeyUp == null)
        return;
      escKeyUp((object) this, (EventArgs) null);
    }

    private void EditorMenuOnContextMenuClosing(object sender, ContextMenuEventArgs context)
    {
      EventHandler quickPopupClosed = this.QuickPopupClosed;
      if (quickPopupClosed == null)
        return;
      quickPopupClosed((object) this, (EventArgs) null);
    }

    private void EditorMenuOnContextMenuOpening(object sender, ContextMenuEventArgs context)
    {
      this.AddEditorContextMenu(context);
      EventHandler quickPopupOpened = this.QuickPopupOpened;
      if (quickPopupOpened == null)
        return;
      quickPopupOpened((object) this, (EventArgs) null);
    }

    public void AddEditorContextMenu(ContextMenuEventArgs context)
    {
      this.FocusEditBox();
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
      newItem1.CommandTarget = (IInputElement) this.EditBox.TextArea;
      newItem1.InputGestureText = "Ctrl+X";
      items1.Add((object) newItem1);
      ItemCollection items2 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem2 = new System.Windows.Controls.MenuItem();
      newItem2.Header = (object) Utils.GetString("Copy");
      newItem2.Command = (ICommand) ApplicationCommands.Copy;
      newItem2.CommandTarget = (IInputElement) this.EditBox.TextArea;
      newItem2.InputGestureText = "Ctrl+C";
      items2.Add((object) newItem2);
      ItemCollection items3 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem3 = new System.Windows.Controls.MenuItem();
      newItem3.Header = (object) Utils.GetString("Paste");
      newItem3.Command = (ICommand) ApplicationCommands.Paste;
      newItem3.CommandParameter = (object) System.Windows.Clipboard.GetDataObject();
      newItem3.CommandTarget = (IInputElement) this.EditBox.TextArea;
      newItem3.InputGestureText = "Ctrl+V";
      items3.Add((object) newItem3);
      contextMenu.Items.Add((object) new Separator());
      ItemCollection items4 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem4 = new System.Windows.Controls.MenuItem();
      newItem4.Header = (object) Utils.GetString("Undo");
      newItem4.Command = (ICommand) ApplicationCommands.Undo;
      newItem4.CommandTarget = (IInputElement) this.EditBox.TextArea;
      newItem4.InputGestureText = "Ctrl+Z";
      items4.Add((object) newItem4);
      ItemCollection items5 = contextMenu.Items;
      System.Windows.Controls.MenuItem newItem5 = new System.Windows.Controls.MenuItem();
      newItem5.Header = (object) Utils.GetString("Redo");
      newItem5.Command = (ICommand) ApplicationCommands.Redo;
      newItem5.CommandTarget = (IInputElement) this.EditBox.TextArea;
      newItem5.InputGestureText = "Ctrl+Y";
      items5.Add((object) newItem5);
      FrameworkElement source = (FrameworkElement) context.Source;
      source.ContextMenu = contextMenu;
      source.ContextMenu.IsOpen = true;
    }

    public void EnterImmersiveMode()
    {
      EventHandler enterImmersive = this.EnterImmersive;
      if (enterImmersive == null)
        return;
      enterImmersive((object) this, (EventArgs) null);
    }

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

    public void SetText(string text)
    {
      this.UnRegisterCaretChanged();
      this.Text = text;
      this.EditBoxOnTextChanged((object) null, (EventArgs) null);
      this.RegisterCaretChanged();
    }

    public void SetTextAndOffset(string text, bool restoreOffset)
    {
      this.UnRegisterInputHandler();
      this.EditBox.TextChanged -= new EventHandler(this.EditBoxOnTextChanged);
      this.OnTextSet(text);
      this.EditBox.SetTextAndOffset(text, restoreOffset);
      this.AfterTextSet();
      this.EditBox.TextChanged += new EventHandler(this.EditBoxOnTextChanged);
      this.RegisterInputHandler();
    }

    public string HandleTabs()
    {
      try
      {
        string str1 = "";
        int num1 = -1;
        int num2 = 0;
        foreach (DocumentLine line in (IEnumerable<DocumentLine>) this.EditBox.Document.Lines)
        {
          string str2 = this.Text.Substring(line.Offset, line.Length);
          string str3 = str2.TrimStart();
          int num3 = str2.Length - str3.Length;
          if (!str3.StartsWith("- ") && !str3.StartsWith("* ") && !str3.StartsWith("+ "))
          {
            int length = str3.IndexOf(". ", StringComparison.Ordinal);
            if (length <= 0 || !int.TryParse(str3.Substring(0, length), out int _))
            {
              str1 = str1 + str2 + "\r\n";
              continue;
            }
          }
          if (num1 < 0 || num3 < num2 * 4)
          {
            num2 = num3 / 4;
            num1 = 0;
            str1 = str1 + str3 + "\r\n";
          }
          else if (num3 >= num2 * 4)
          {
            num1 = Math.Min((num3 - num2 * 4) / 4, num1 + 1);
            for (int index = 0; index < num1; ++index)
              str1 += "    ";
            str1 = str1 + str3 + "\r\n";
          }
          else
          {
            num1 = -1;
            num2 = 0;
            str1 = str1 + str3 + "\r\n";
          }
        }
        return str1;
      }
      catch (Exception ex)
      {
        return this.Text;
      }
    }

    public int GetPageLastIndex(double height)
    {
      TextView textView = this.EditBox.TextArea.TextView;
      DocumentLine documentLine = (DocumentLine) null;
      foreach (DocumentLine line in (IEnumerable<DocumentLine>) this.EditBox.Document.Lines)
      {
        TextViewPosition position = new TextViewPosition(this.EditBox.Document.GetLocation(line.EndOffset));
        if (textView.GetVisualPosition(position, VisualYPosition.LineBottom).Y > height)
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
      try
      {
        TextView textView = this.EditBox.TextArea.TextView;
        if (start == end)
          return end;
        for (int pageLastIndex = start; pageLastIndex < end; pageLastIndex += interval)
        {
          TextViewPosition position1 = new TextViewPosition(this.EditBox.Document.GetLocation(pageLastIndex));
          System.Windows.Point visualPosition1 = textView.GetVisualPosition(position1, VisualYPosition.LineBottom);
          TextViewPosition position2 = new TextViewPosition(this.EditBox.Document.GetLocation(pageLastIndex + interval));
          System.Windows.Point visualPosition2 = textView.GetVisualPosition(position2, VisualYPosition.LineBottom);
          if (visualPosition1.Y < height && visualPosition2.Y > height)
            return interval != 1 ? this.GetPageLastIndex(height, pageLastIndex, Math.Min(end, pageLastIndex + interval), interval / 10) : pageLastIndex;
          if (visualPosition1.Y > height)
            return pageLastIndex;
        }
        return end;
      }
      catch (Exception ex)
      {
        return end;
      }
    }

    public List<Inline> GetMarkList() => this.MarkList;

    public Dictionary<int, LinkInfo> GetLinkNameDict() => this.LinkNameDict;

    public Dictionary<int, LinkInfo> GetLinkUrlDict() => this.LinkUrlDict;

    public TextEditor GetEditBox() => this.EditBox;

    public Brush GetBracketColor() => this._bracketColor;

    public Brush GetHighLightColor() => this._highLightColor;

    public Dictionary<int, AttachmentInfo> GetImageDict() => this.AttachmentDict;

    public void CorrectSpellingError(string correct, TextSegment errorSegment)
    {
      this.EditBox.CaretOffset = errorSegment.StartOffset;
      this.EditBox.Document.Replace((ISegment) errorSegment, correct);
    }

    public void AddErrorToDict(string error, TextSegment errorSegment)
    {
      this.SpellCheckProvider?.Add(error);
      this.EditBox.CaretOffset = errorSegment.StartOffset;
      this.EditBox.Document.Insert(0, " ");
      this.EditBox.Document.UndoStack.Undo();
    }

    public void SetImageGeneratorTaskId(string id)
    {
      this._editingTaskId = id;
      EventHandler<string> taskIdChanged = this.TaskIdChanged;
      if (taskIdChanged == null)
        return;
      taskIdChanged((object) this, id);
    }

    public void TryDeleteAttachment(int offset)
    {
      if (!this.AttachmentDict.ContainsKey(offset))
        return;
      AttachmentInfo attachmentInfo = this.AttachmentDict[offset];
      this.EditBox.Document.Replace(offset, attachmentInfo.Value.Length, "");
      this.FocusEditBox();
      Keyboard.Focus((IInputElement) this.EditBox);
      this.EditBox.CaretOffset = offset;
    }

    public string RemoveAttachment()
    {
      IOrderedEnumerable<int> orderedEnumerable = this.AttachmentDict.Keys.OrderByDescending<int, int>((Func<int, int>) (k => k));
      string str = this.Text;
      foreach (int key in (IEnumerable<int>) orderedEnumerable)
      {
        if (this.AttachmentDict.ContainsKey(key))
        {
          AttachmentInfo attachmentInfo = this.AttachmentDict[key];
          if (AttachmentDao.GetAttachmentInUrl(attachmentInfo.Url) != null && str.Length >= attachmentInfo.Offset + attachmentInfo.Value.Length)
          {
            int offset = attachmentInfo.Offset;
            int length = attachmentInfo.Value.Length;
            if (attachmentInfo.Offset != 0)
            {
              if (str[offset - 1] == '\r')
              {
                --offset;
                ++length;
              }
              else if (offset > 1 && str[offset - 2] == '\r' && str[offset - 1] == '\n')
              {
                offset -= 2;
                length += 2;
              }
            }
            str = str.Remove(offset, length);
          }
        }
      }
      return str;
    }

    public int GetAttachmentDefaultInsertIndex() => this.Text.Length;

    private bool CaretNearAttachment()
    {
      int selectionStart = this.EditBox.SelectionStart;
      int num1 = this.EditBox.SelectionStart + this.EditBox.SelectionLength;
      foreach (int key in this.AttachmentDict.Keys.ToList<int>())
      {
        if (this.AttachmentDict.ContainsKey(key))
        {
          AttachmentInfo attachmentInfo = this.AttachmentDict[key];
          int num2 = key + attachmentInfo.Value.Length;
          if (key <= num1 && key >= selectionStart || num2 <= num1 && num2 >= selectionStart)
            return true;
        }
      }
      return false;
    }

    public void Redraw() => this.EditBox.TextArea.TextView.Redraw();

    private void BeforeTextInput(object sender, TextCompositionEventArgs e)
    {
      if (this.MaxLength < 0)
        return;
      string text1 = e.Text;
      if (this.Text.Length >= this.MaxLength)
      {
        e.Handled = true;
        Utils.Toast(Utils.GetString("CharactersNumberLimit"));
      }
      else
      {
        if (this.Text.Length + text1.Length - this.EditBox.SelectionLength <= this.MaxLength)
          return;
        e.Handled = true;
        string text2 = text1.Substring(0, Math.Max(0, this.MaxLength - this.Text.Length + this.EditBox.SelectionLength));
        if (text2.Length == 0)
          Utils.Toast(Utils.GetString("CharactersNumberLimit"));
        else
          this.InsertText(text2);
      }
    }

    private void InsertText(string text)
    {
      int offset = this.EditBox.CaretOffset;
      if (this.EditBox.SelectionLength > 0)
      {
        this.EditBox.Document.Remove(this.EditBox.SelectionStart, this.EditBox.SelectionLength);
        offset = this.EditBox.SelectionStart;
      }
      this.EditBox.Document.Insert(offset, text);
      this.EditBox.CaretOffset = offset + text.Length;
    }

    public void FixCurrentIndex()
    {
      DocumentLine lineByOffset = this.EditBox.TextArea.Document.GetLineByOffset(this.EditBox.CaretOffset);
      if (this.EditBox.CaretOffset == lineByOffset.Offset || !this.AttachmentEndSet.Contains(lineByOffset.EndOffset))
        return;
      this.EditBox.CaretOffset = lineByOffset.EndOffset;
    }

    public bool BetweenCode(int offset)
    {
      return this.CodeDict.Values.Any<List<System.Windows.Point>>((Func<List<System.Windows.Point>, bool>) (points => points != null && points.Any<System.Windows.Point>((Func<System.Windows.Point, bool>) (p => (double) offset >= p.X && (double) offset < p.Y)))) || this.InlineCodeList.Any<Inline>((Func<Inline, bool>) (inline => offset >= inline.SourcePosition && offset < inline.SourcePosition + inline.SourceLength));
    }

    public void FocusEnd()
    {
      this.FocusEditBox();
      this.EditBox.CaretOffset = this.Text.Length;
    }

    public void FocusFirst()
    {
      this.FocusEditBox();
      this.EditBox.CaretOffset = 0;
    }

    public void FocusEditBox() => this.EditBox.TryFocus();

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
      this.SelectionPopup.Opened += new EventHandler(this.OnQuickPopupOpen);
      this.SelectionPopup.Closed += new EventHandler(this.OnQuickPopupClosed);
    }

    private async void TryShowQuickSetPopup()
    {
      MarkDownEditor markDownEditor = this;
      if (markDownEditor.BetweenCode(markDownEditor.EditBox.CaretOffset))
        return;
      string mark = markDownEditor.TryGetMark();
      if (string.IsNullOrEmpty(mark))
      {
        if (markDownEditor.SelectionPopup == null)
          return;
        markDownEditor.SelectionPopup.IsOpen = false;
      }
      else
      {
        markDownEditor.InitPopup();
        if (markDownEditor._selectionControl == null)
        {
          markDownEditor._selectionControl = new QuickSelectionControl(markDownEditor.SelectionPopup);
          markDownEditor.SelectionPopup.Child = (UIElement) markDownEditor._selectionControl;
          markDownEditor._selectionControl.ItemSelected += new EventHandler<QuickSetModel>(markDownEditor.OnQuickItemSelect);
        }
        bool usedInCal = markDownEditor.IsUsedInCalendar();
        string calId = !usedInCal || !(mark == "^") && !(mark == "~") ? string.Empty : markDownEditor.GetAccountId();
        string assignee = mark == "@" ? markDownEditor.GetAssignee() : (string) null;
        int priority = mark == "!" ? markDownEditor.GetPriority() : -1;
        List<string> tags = mark == "#" ? markDownEditor.GetTags() : (List<string>) null;
        if (markDownEditor._selectionControl.SetSelectionItems(mark, markDownEditor._filterContent, usedInCal, calId, (string) null, priority, tags, assignee))
        {
          System.Windows.Point popupOffset = markDownEditor.GetPopupOffset();
          markDownEditor._selectionControl.TryShowPopup(mark, markDownEditor._filterContent, new System.Windows.Point(popupOffset.X + 10.0, popupOffset.Y - 12.0));
        }
        else
          markDownEditor._selectionControl?.ClosePopup();
      }
    }

    private void OnQuickItemSelect(object sender, QuickSetModel e)
    {
      if (e.Type == QuickSetType.TaskLink)
      {
        int offset = Math.Min(this.Text.Length, Math.Max(0, this._markIndex));
        this.EditBox.Document.Remove(offset, Math.Min(this._filterContent.Length + 2, this.Text.Length - offset));
        string copyLink = TaskUtils.GetCopyLink(e.task.ProjectId, e.task.Id, e.task.Title, true);
        this.EditBox.Document.Insert(offset, copyLink);
        this.EditBox.CaretOffset = offset + copyLink.Length;
        this.FocusEditBox();
      }
      else
      {
        this.RemoveQuickSetText();
        EventHandler<QuickSetModel> quickItemSelected = this.QuickItemSelected;
        if (quickItemSelected == null)
          return;
        quickItemSelected(sender, e);
      }
    }

    private System.Windows.Point GetPopupOffset()
    {
      return this.EditBox.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.EditBox.Document.GetLocation(this.EditBox.CaretOffset)), VisualYPosition.LineBottom);
    }

    private TaskBaseViewModel GetTaskBaseModel()
    {
      return this.DataContext is TaskDetailViewModel dataContext ? dataContext.SourceViewModel : (TaskBaseViewModel) null;
    }

    protected string GetAvatarProjectId() => this.GetTaskBaseModel()?.ProjectId;

    protected string GetAssignee() => this.GetTaskBaseModel()?.Assignee;

    protected int GetPriority()
    {
      TaskBaseViewModel taskBaseModel = this.GetTaskBaseModel();
      return taskBaseModel == null ? -1 : taskBaseModel.Priority;
    }

    protected List<string> GetTags()
    {
      TaskBaseViewModel taskBaseModel = this.GetTaskBaseModel();
      if (taskBaseModel == null)
        return (List<string>) null;
      string[] tags = taskBaseModel.Tags;
      return tags == null ? (List<string>) null : ((IEnumerable<string>) tags).ToList<string>();
    }

    protected virtual string GetAccountId() => "";

    protected virtual bool IsUsedInCalendar() => false;

    public void RemoveQuickSetText()
    {
      int offset = Math.Min(this.Text.Length, Math.Max(0, this._markIndex));
      this.EditBox.Document.Remove(offset, Math.Min(this._filterContent.Length + 1, this.Text.Length - offset));
      this.FocusEditBox();
      this.EditBox.CaretOffset = offset;
    }

    private string TryGetMark()
    {
      DocumentLine lineByOffset = this.EditBox.Document.GetLineByOffset(this.EditBox.CaretOffset);
      string input = string.Empty;
      if (lineByOffset != null)
      {
        if (lineByOffset.Offset < this.EditBox.CaretOffset)
          input = this.EditBox.Text.Substring(lineByOffset.Offset, this.EditBox.CaretOffset - lineByOffset.Offset);
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
          if (input.StartsWith("[[") || input.StartsWith("【【"))
          {
            int num = 0;
            for (int index = 0; index < source.Length - 1; ++index)
              num += source[index].Length + 1;
            this._filterContent = input.Substring(2);
            this._markIndex = lineByOffset.Offset + num;
            return "[[";
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

    public void OnAttachmentMouseEnter(object sender)
    {
      if (!(sender is Border bd))
        return;
      Utils.FindParent<TaskDetailView>((DependencyObject) this)?.OnAttachmentMouseEnter(bd);
    }

    private void OnQuickPopupOpen(object sender, EventArgs e)
    {
      EventHandler quickPopupOpened = this.QuickPopupOpened;
      if (quickPopupOpened == null)
        return;
      quickPopupOpened((object) this, (EventArgs) null);
    }

    private void OnQuickPopupClosed(object sender, EventArgs e)
    {
      EventHandler quickPopupClosed = this.QuickPopupClosed;
      if (quickPopupClosed == null)
        return;
      quickPopupClosed((object) this, (EventArgs) null);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/markdown/markdowneditor.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
        {
          this.EditBox = (TextEditor) target;
          this.EditBox.PreviewTextInput += new TextCompositionEventHandler(this.BeforeTextInput);
        }
        else
          this._contentLoaded = true;
      }
      else
      {
        this.Root = (MarkDownEditor) target;
        this.Root.ContextMenuOpening += new ContextMenuEventHandler(this.EditorMenuOnContextMenuOpening);
        this.Root.ContextMenuClosing += new ContextMenuEventHandler(this.EditorMenuOnContextMenuClosing);
      }
    }
  }
}
