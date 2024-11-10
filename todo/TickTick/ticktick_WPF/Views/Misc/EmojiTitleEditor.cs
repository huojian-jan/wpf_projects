// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.EmojiTitleEditor
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Views.MarkDown;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class EmojiTitleEditor : UserControl, IComponentConnector
  {
    internal EmojiEditor Editor;
    internal EmjTextBlock Display;
    internal Grid ErrorGrid;
    internal TextBlock ErrorText;
    private bool _contentLoaded;

    public event EventHandler<string> TextChanged;

    public Func<string, string> CheckFunc { get; set; }

    public EmojiTitleEditor()
    {
      this.InitializeComponent();
      this.Editor.EditBox.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
      this.Editor.EditBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.Editor.EditBox.FontWeight = FontWeights.Bold;
    }

    public void SetCheckFunc(Func<string, string> func) => this.CheckFunc = func;

    public void SetText(string text)
    {
      this.Editor.TextChanged -= new EventHandler(this.OnTextChanged);
      this.Editor.Text = string.Empty;
      this.Editor.Visibility = Visibility.Collapsed;
      this.ErrorGrid.Visibility = Visibility.Collapsed;
      this.Display.Text = text;
      this.Display.Visibility = Visibility.Visible;
    }

    public void SetMaxWidth(double width)
    {
      if (width <= 0.0)
        return;
      this.MaxWidth = width;
    }

    public void SetEnable(bool projectCanEdit) => this.IsHitTestVisible = projectCanEdit;

    private async void OnClick(object sender, MouseButtonEventArgs e)
    {
      EmojiTitleEditor emojiTitleEditor = this;
      if (!emojiTitleEditor.Display.IsVisible)
        return;
      e.Handled = true;
      emojiTitleEditor.Editor.TextChanged -= new EventHandler(emojiTitleEditor.OnTextChanged);
      emojiTitleEditor.Editor.EditBox.LostFocus -= new RoutedEventHandler(emojiTitleEditor.OnLostFocus);
      emojiTitleEditor.Editor.Text = emojiTitleEditor.Display.Text;
      emojiTitleEditor.Editor.Visibility = Visibility.Visible;
      emojiTitleEditor.Editor.Width = Math.Min(emojiTitleEditor.Display.ActualWidth + 80.0, emojiTitleEditor.MaxWidth);
      emojiTitleEditor.Display.Visibility = Visibility.Collapsed;
      await Task.Delay(100);
      emojiTitleEditor.Editor.FocusEnd();
      emojiTitleEditor.Editor.EditBox.LostFocus += new RoutedEventHandler(emojiTitleEditor.OnLostFocus);
      emojiTitleEditor.Editor.TextChanged += new EventHandler(emojiTitleEditor.OnTextChanged);
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
      string str = this.CheckFunc(this.Editor.Text);
      this.ErrorGrid.Visibility = !string.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Collapsed;
      this.ErrorText.Text = str;
    }

    private async void OnLostFocus(object sender, RoutedEventArgs e)
    {
      EmojiTitleEditor emojiTitleEditor = this;
      if (emojiTitleEditor.ErrorGrid.IsVisible)
        return;
      emojiTitleEditor.Editor.EditBox.LostFocus -= new RoutedEventHandler(emojiTitleEditor.OnLostFocus);
      emojiTitleEditor.TrySaveText();
    }

    private void TrySaveText()
    {
      this.Editor.TextChanged -= new EventHandler(this.OnTextChanged);
      this.Editor.Visibility = Visibility.Collapsed;
      if (!string.IsNullOrEmpty(this.Editor.Text.Trim()) && this.Editor.Text.Trim() != this.Display.Text)
      {
        this.Display.Text = this.Editor.Text.Trim();
        EventHandler<string> textChanged = this.TextChanged;
        if (textChanged != null)
          textChanged((object) this, this.Editor.Text.Trim());
      }
      this.Display.Visibility = Visibility.Visible;
      this.Editor.Text = string.Empty;
    }

    private void OnEnterUp(object sender, EventArgs e)
    {
      if (this.ErrorGrid.IsVisible)
        return;
      this.Editor.EditBox.LostFocus -= new RoutedEventHandler(this.OnLostFocus);
      this.TrySaveText();
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      if (this.ErrorGrid.IsVisible)
        this.ErrorGrid.Visibility = Visibility.Collapsed;
      this.SetText(this.Display.Text);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/emojititleeditor.xaml", UriKind.Relative));
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
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
          break;
        case 2:
          this.Editor = (EmojiEditor) target;
          break;
        case 3:
          this.Display = (EmjTextBlock) target;
          break;
        case 4:
          this.ErrorGrid = (Grid) target;
          break;
        case 5:
          this.ErrorText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
