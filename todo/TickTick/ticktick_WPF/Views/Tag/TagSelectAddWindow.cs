// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagSelectAddWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagSelectAddWindow : MyWindow, IComponentConnector
  {
    private double _left;
    private double _top;
    internal Border ShadowBorder;
    internal Border TagBorder;
    internal TextBox TitleText;
    internal Grid TagPanel;
    internal TagSelectionControl TagItems;
    internal Border ErrorBorder;
    internal Popup ErrorPopup;
    private bool _contentLoaded;

    public TagSelectAddWindow(bool canCreateTag = true, string text = null)
    {
      this.InitializeComponent();
      this.TagItems.CanCreateTag = canCreateTag;
      if (text == null)
        return;
      this.TagBorder.Visibility = Visibility.Hidden;
    }

    public event EventHandler<string> TagExit;

    public void Show(double left, double top)
    {
      this._left = left;
      this._top = top;
      this.InitEvents();
      this.Show();
      this.FocusTextEnd();
    }

    private void InitEvents()
    {
      this.TagItems.TagSelected -= new EventHandler<string>(this.OnTagSelected);
      this.TagItems.TagSelected += new EventHandler<string>(this.OnTagSelected);
      this.TagItems.TagsCountChanged -= new EventHandler<int>(this.OnTagsCountChanged);
      this.TagItems.TagsCountChanged += new EventHandler<int>(this.OnTagsCountChanged);
    }

    private void OnTagsCountChanged(object sender, int count)
    {
      this.TagPanel.Visibility = count <= 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnTagSelected(object sender, string tag) => this.Hide();

    private async void FocusTextEnd()
    {
      this.TitleText.Focus();
      await Task.Delay(10);
      this.TitleText.Select(this.TitleText.Text.Length, 0);
    }

    private void OnTitleKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
          if (Utils.IfShiftPressed())
          {
            this.TagItems.MoveUp();
            break;
          }
          this.TagItems.MoveDown();
          break;
        case Key.Return:
          this.TryAddTag();
          e.Handled = true;
          break;
        case Key.Escape:
          this.ExitTagMode();
          break;
        case Key.Up:
          this.TagItems.MoveUp();
          break;
        case Key.Down:
          this.TagItems.MoveDown();
          break;
      }
    }

    private void TryAddTag()
    {
      if (TagSelectAddWindow.CheckIfTextValid(this.TitleText.Text))
        this.TagItems.TrySelectFocusedTag();
      else
        Utils.Toast(Utils.GetString("TagNotValid"));
    }

    private static bool CheckIfTextValid(string text)
    {
      return !string.IsNullOrEmpty(text) && (!text.StartsWith("#") || NameUtils.IsValidName(text.Substring(1)));
    }

    private void ExitTagMode()
    {
      EventHandler<string> tagExit = this.TagExit;
      if (tagExit != null)
        tagExit((object) this, this.TitleText.Text);
      this.Hide();
    }

    private void InitLocation()
    {
      if (double.IsNaN(this._left) || double.IsInfinity(this._left))
        this._left = 0.0;
      if (double.IsNaN(this._top) || double.IsInfinity(this._top))
        this._top = 0.0;
      this.Left = this._left;
      this.Top = this._top;
    }

    private void OnTitleTextChanged(object sender, TextChangedEventArgs e)
    {
      if (this.TitleText.Text == "# ")
      {
        this.ExitTagMode();
      }
      else
      {
        string str = string.IsNullOrEmpty(this.TitleText.Text) ? string.Empty : this.TitleText.Text;
        if (!Utils.IsWindows7() && str != "#" && str.EndsWith("#"))
        {
          this.ExitTagMode();
          this.TitleText.TextChanged -= new TextChangedEventHandler(this.OnTitleTextChanged);
          this.TitleText.Text = string.Empty;
          this.TitleText.TextChanged += new TextChangedEventHandler(this.OnTitleTextChanged);
        }
        if (str.Contains(" "))
          this.ExitTagMode();
        if (str == "#")
        {
          this.TagBorder.Background = (Brush) new SolidColorBrush(Colors.Transparent);
          this.ShadowBorder.Background = (Brush) new SolidColorBrush(Colors.Transparent);
          this.TitleText.Foreground = (Brush) new SolidColorBrush(Colors.Transparent);
          this.TagItems?.Filter(string.Empty, (List<string>) null);
        }
        else
        {
          if (!string.IsNullOrEmpty(str) && str.StartsWith("#"))
            str = str.Substring(1);
          this.ShadowBorder.Background = (Brush) new SolidColorBrush(Colors.White);
          this.TagBorder.Background = (Brush) ThemeUtil.GetPrimaryColor(0.18);
          this.TitleText.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
          this.ShowErrorHint(!NameUtils.IsValidName(str, false));
          if (!string.IsNullOrEmpty(str))
          {
            if (str.EndsWith(" "))
            {
              EventHandler<string> tagExit = this.TagExit;
              if (tagExit != null)
                tagExit((object) this, " ");
              this.Hide();
            }
            else
              this.TagItems?.Filter(str, (List<string>) null);
          }
          else
            this.ExitTagMode();
        }
      }
    }

    private void ShowErrorHint(bool show)
    {
      if (this.ErrorBorder == null)
        return;
      this.ErrorBorder.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
      this.ErrorPopup.IsOpen = show;
    }

    private void OnWindowDeactivated(object sender, EventArgs e) => this.Close();

    private void OnWindowLoaded(object sender, RoutedEventArgs e) => this.InitLocation();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/tagselectaddwindow.xaml", UriKind.Relative));
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
          this.ShadowBorder = (Border) target;
          break;
        case 2:
          this.TagBorder = (Border) target;
          break;
        case 3:
          this.TitleText = (TextBox) target;
          this.TitleText.PreviewKeyUp += new KeyEventHandler(this.OnTitleKeyUp);
          this.TitleText.TextChanged += new TextChangedEventHandler(this.OnTitleTextChanged);
          break;
        case 4:
          this.TagPanel = (Grid) target;
          break;
        case 5:
          this.TagItems = (TagSelectionControl) target;
          break;
        case 6:
          this.ErrorBorder = (Border) target;
          break;
        case 7:
          this.ErrorPopup = (Popup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
