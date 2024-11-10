// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.EmojiEditor
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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class EmojiEditor : UserControl, IEmojiRender, ILinkTextEditor, IComponentConnector
  {
    public readonly Dictionary<int, string> EmojiDict = new Dictionary<int, string>();
    public new static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(nameof (FontSize), typeof (double), typeof (EmojiEditor), new PropertyMetadata((object) 14.0));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof (Text), typeof (string), typeof (EmojiEditor), new PropertyMetadata((object) null, new PropertyChangedCallback(EmojiEditor.OnTextChanged)));
    public static readonly DependencyProperty TextPaddingProperty = DependencyProperty.Register(nameof (TextPadding), typeof (Thickness), typeof (EmojiEditor), new PropertyMetadata((object) new Thickness(0.0)));
    public new static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(nameof (BorderBrush), typeof (Brush), typeof (EmojiEditor), new PropertyMetadata((object) Brushes.Transparent));
    public static readonly DependencyProperty BorderBackgroundProperty = DependencyProperty.Register(nameof (BorderBackground), typeof (Brush), typeof (EmojiEditor), new PropertyMetadata((object) Brushes.Transparent));
    public new static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(nameof (BorderThickness), typeof (Thickness), typeof (EmojiEditor), new PropertyMetadata((object) new Thickness(0.0)));
    public static readonly DependencyProperty BorderCornerProperty = DependencyProperty.Register(nameof (BorderCorner), typeof (CornerRadius), typeof (EmojiEditor), new PropertyMetadata((object) new CornerRadius(0.0)));
    public new static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(nameof (Padding), typeof (Thickness), typeof (EmojiEditor), new PropertyMetadata((object) new Thickness(0.0)));
    public new static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof (Foreground), typeof (Brush), typeof (EmojiEditor), new PropertyMetadata((object) Brushes.Black));
    public new static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(nameof (FontWeight), typeof (FontWeight), typeof (EmojiEditor), new PropertyMetadata((object) FontWeights.Normal));
    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(nameof (ReadOnly), typeof (bool), typeof (EmojiEditor), new PropertyMetadata((object) false, new PropertyChangedCallback(EmojiEditor.OnIsReadOnlyChangedCallback)));
    public static readonly DependencyProperty FocusedProperty = DependencyProperty.Register(nameof (Focused), typeof (bool), typeof (EmojiEditor), new PropertyMetadata((object) false));
    public static readonly DependencyProperty AcceptReturnProperty = DependencyProperty.Register(nameof (AcceptReturn), typeof (bool), typeof (EmojiEditor), new PropertyMetadata((object) false));
    public static readonly DependencyProperty WordWrapProperty = DependencyProperty.Register(nameof (WordWrap), typeof (bool), typeof (EmojiEditor), new PropertyMetadata((object) true, new PropertyChangedCallback(EmojiEditor.OnWordWrapChanged)));
    public static readonly DependencyProperty TextVerticalAlignmentProperty = DependencyProperty.Register(nameof (TextVerticalAlignment), typeof (VerticalAlignment), typeof (EmojiEditor), new PropertyMetadata((object) VerticalAlignment.Center, new PropertyChangedCallback(EmojiEditor.OnTextVerticalAlignmentChanged)));
    private bool _enterDown;
    internal EmojiEditor Root;
    internal Border TextBorder;
    internal TextBlock HintText;
    internal TextEditor EditBox;
    private bool _contentLoaded;

    public event EventHandler TextChanged;

    public event EventHandler<KeyEventArgs> KeysUp;

    public event EventHandler<KeyEventArgs> KeysDown;

    public event EventHandler EnterUp;

    public EmojiEditor()
    {
      this.InitializeComponent();
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new EmojiGenerator((IEmojiRender) this));
      this.EditBox.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new CustomLinkElementGenerator((ILinkTextEditor) this, true));
      this.WordWrap = true;
      this.EditBox.TextChanged += new EventHandler(this.OnTextChanged);
      this.EditBox.GotFocus += new RoutedEventHandler(this.OnFocused);
      this.EditBox.LostFocus += new RoutedEventHandler(this.OnLostFocused);
      this.EditBox.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
      this.EditBox.PreviewKeyUp += new KeyEventHandler(this.OnKeyUp);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (this._enterDown && e.Key == Key.Return)
      {
        EventHandler enterUp = this.EnterUp;
        if (enterUp != null)
          enterUp((object) this, (EventArgs) null);
      }
      this._enterDown = false;
      EventHandler<KeyEventArgs> keysUp = this.KeysUp;
      if (keysUp == null)
        return;
      keysUp((object) this, e);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      if (this.Tag is string tag)
        this.HintText.Text = tag;
      this.EditBox.TextArea.TextView.LinkTextForegroundBrush = (Brush) ThemeUtil.GetColor("TextAccentColor", (FrameworkElement) this);
      DataObject.RemovePastingHandler((DependencyObject) this.EditBox, new DataObjectPastingEventHandler(this.OnPaste));
      DataObject.AddPastingHandler((DependencyObject) this.EditBox, new DataObjectPastingEventHandler(this.OnPaste));
      this.EditBox.Options.EnableHyperlinks = false;
      this.EditBox.Options.EnableEmailHyperlinks = false;
      this.EditBox.Options.InheritWordWrapIndentation = false;
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
      int num = 1;
      bool flag = true;
      while (flag)
      {
        try
        {
          if (e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true) && e.SourceDataObject.GetData(DataFormats.UnicodeText) is string str)
          {
            if (this.MaxLength >= 0 && this.EditBox.Text.Length + str.Length - this.EditBox.SelectionLength > this.MaxLength)
            {
              e.CancelCommand();
              if (!this.WordWrap)
                str = str.Replace("\n", " ").Replace("\r", " ");
              this.InsertText(str.Substring(0, this.MaxLength - this.EditBox.Text.Length + this.EditBox.SelectionLength));
              break;
            }
            if (!this.WordWrap)
            {
              e.CancelCommand();
              this.InsertText(str.Replace("\n", " ").Replace("\r", " "));
            }
          }
          flag = false;
        }
        catch (Exception ex)
        {
          flag = num++ < 10;
        }
      }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (this.ReadOnly)
        return;
      if (e.Key == Key.Return)
      {
        if (!this.AcceptReturn || this.MaxLength > 0 && this.Text.Length + Environment.NewLine.Length > this.MaxLength)
          e.Handled = true;
        this._enterDown = true;
      }
      else
        this._enterDown = false;
      EventHandler<KeyEventArgs> keysDown = this.KeysDown;
      if (keysDown == null)
        return;
      keysDown((object) this, e);
    }

    private void OnLostFocused(object sender, RoutedEventArgs e) => this.Focused = false;

    private void OnFocused(object sender, RoutedEventArgs e)
    {
      if (this.ReadOnly)
        return;
      this.Focused = true;
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
      EventHandler textChanged = this.TextChanged;
      if (textChanged != null)
        textChanged((object) this, (EventArgs) null);
      this.LogEmojiPosition();
      this.HintText.Visibility = string.IsNullOrEmpty(this.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    public new double FontSize
    {
      get => this.EditBox.FontSize;
      set => this.SetValue(EmojiEditor.FontSizeProperty, (object) value);
    }

    private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is EmojiEditor emojiEditor) || e.NewValue == null)
        return;
      emojiEditor.Text = (string) e.NewValue;
    }

    public string Text
    {
      get => this.EditBox.Text;
      set
      {
        this.EditBox.Text = value;
        this.SetCurrentValue(EmojiEditor.TextProperty, (object) value);
        this.EditBox.TextArea.TextView.LineTransformers.Clear();
        this.LogEmojiPosition();
        this.EditBox.TextArea.TextView.Redraw();
      }
    }

    public Thickness TextPadding
    {
      get => (Thickness) this.GetValue(EmojiEditor.TextPaddingProperty);
      set => this.SetValue(EmojiEditor.TextPaddingProperty, (object) value);
    }

    public new Brush BorderBrush
    {
      get => this.TextBorder.BorderBrush;
      set => this.SetValue(EmojiEditor.BorderBrushProperty, (object) value);
    }

    public Brush BorderBackground
    {
      get => this.TextBorder.Background;
      set => this.SetValue(EmojiEditor.BorderBackgroundProperty, (object) value);
    }

    public new Thickness BorderThickness
    {
      get => this.TextBorder.BorderThickness;
      set => this.SetValue(EmojiEditor.BorderThicknessProperty, (object) value);
    }

    public CornerRadius BorderCorner
    {
      get => this.TextBorder.CornerRadius;
      set => this.SetValue(EmojiEditor.BorderCornerProperty, (object) value);
    }

    public new Thickness Padding
    {
      get => this.TextBorder.Padding;
      set => this.SetValue(EmojiEditor.PaddingProperty, (object) value);
    }

    public new Brush Foreground
    {
      get => this.EditBox.Foreground;
      set => this.SetValue(EmojiEditor.ForegroundProperty, (object) value);
    }

    public new FontWeight FontWeight
    {
      get => this.EditBox.FontWeight;
      set => this.SetValue(EmojiEditor.FontWeightProperty, (object) value);
    }

    private static void OnIsReadOnlyChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is EmojiEditor emojiEditor) || e.NewValue == null)
        return;
      emojiEditor.ReadOnly = (bool) e.NewValue;
    }

    public bool Focused
    {
      get => (bool) this.GetValue(EmojiEditor.FocusedProperty);
      set => this.SetValue(EmojiEditor.FocusedProperty, (object) value);
    }

    private static void OnWordWrapChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is EmojiEditor emojiEditor) || !(e.NewValue is bool newValue))
        return;
      emojiEditor.WordWrap = newValue;
    }

    public bool WordWrap
    {
      get => this.EditBox.WordWrap;
      set
      {
        this.EditBox.WordWrap = value;
        this.EditBox.SetValue(ScrollViewer.PanningModeProperty, (object) (PanningMode) (value ? 0 : 4));
        this.EditBox.HorizontalScrollBarVisibility = value ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Hidden;
        this.EditBox.VerticalScrollBarVisibility = value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        this.EditBox.TextArea.TextView.LineSpacing = value ? 4.0 : 0.2;
      }
    }

    private static void OnTextVerticalAlignmentChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is EmojiEditor emojiEditor) || !(e.NewValue is VerticalAlignment newValue))
        return;
      emojiEditor.EditBox.VerticalAlignment = newValue;
    }

    public VerticalAlignment TextVerticalAlignment
    {
      get => this.EditBox.VerticalAlignment;
      set
      {
        this.SetValue(EmojiEditor.TextVerticalAlignmentProperty, (object) value);
        this.EditBox.VerticalAlignment = value;
      }
    }

    public bool AcceptReturn
    {
      get => (bool) this.GetValue(EmojiEditor.AcceptReturnProperty);
      set => this.SetValue(EmojiEditor.AcceptReturnProperty, (object) value);
    }

    public bool ReadOnly
    {
      get => (bool) this.GetValue(EmojiEditor.ReadOnlyProperty);
      set
      {
        this.EditBox.IsReadOnly = value;
        this.EditBox.TextArea.Caret.CaretBrush = value ? (Brush) Brushes.Transparent : (Brush) ThemeUtil.GetColor("BaseColorOpacity100");
        if (value)
          this.EditBox.TextArea.ClearSelection();
        if (this.ReadOnly == value)
          return;
        this.SetValue(EmojiEditor.ReadOnlyProperty, (object) value);
      }
    }

    public int MaxLength { get; set; } = -1;

    public void FocusEnd()
    {
      this.EditBox.Focus();
      Keyboard.Focus((IInputElement) this.EditBox);
      this.EditBox.CaretOffset = this.Text.Length;
      this.EditBox.ScrollToEnd();
    }

    public void Focus()
    {
      this.EditBox.Focus();
      Keyboard.Focus((IInputElement) this.EditBox);
    }

    public Dictionary<int, string> GetEmojiDict() => this.EmojiDict;

    private void LogEmojiPosition()
    {
      this.EmojiDict.Clear();
      if (string.IsNullOrEmpty(this.Text))
        return;
      foreach (Match match in (this.Text.Length > 200 ? EmojiData.MatchOne2 : EmojiData.MatchOne).Matches(this.Text))
        this.EmojiDict[match.Index] = match.Value;
    }

    private void OnTextInput(object sender, TextCompositionEventArgs e)
    {
      if (this.MaxLength < 0)
        return;
      string text = e.Text;
      if (this.MaxLength < 0)
        return;
      if (this.EditBox.Text.Length >= this.MaxLength)
      {
        e.Handled = true;
      }
      else
      {
        if (this.EditBox.Text.Length + text.Length - this.EditBox.SelectionLength <= this.MaxLength)
          return;
        e.Handled = true;
        this.InsertText(text.Substring(0, this.MaxLength - this.EditBox.Text.Length + this.EditBox.SelectionLength));
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

    public Dictionary<int, LinkInfo> GetLinkNameDict() => (Dictionary<int, LinkInfo>) null;

    public Dictionary<int, LinkInfo> GetLinkUrlDict() => (Dictionary<int, LinkInfo>) null;

    public void NavigateTask(string projectTaskProjectId, string projectTaskTaskId)
    {
    }

    public void ShowInsertLink(string name, string url, VisualLine line, bool b)
    {
    }

    public void UnRegisterCaretChanged()
    {
    }

    public void RegisterCaretChanged()
    {
    }

    public TextEditor GetEditBox() => this.EditBox;

    public Brush GetHighLightColor() => this.Foreground;

    public Brush GetBracketColor() => this.Foreground;

    private void OnBorderMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (this.EditBox.TextArea.IsKeyboardFocused)
        return;
      this.EditBox.Focus();
      this.EditBox.CaretOffset = this.EditBox.Text.Length;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/markdown/emojieditor.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (EmojiEditor) target;
          break;
        case 2:
          this.TextBorder = (Border) target;
          this.TextBorder.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnBorderMouseUp);
          break;
        case 3:
          this.HintText = (TextBlock) target;
          break;
        case 4:
          this.EditBox = (TextEditor) target;
          this.EditBox.PreviewTextInput += new TextCompositionEventHandler(this.OnTextInput);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
