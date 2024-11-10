// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagAddControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagAddControl : UserControl, IComponentConnector
  {
    private bool _tagSelected;
    private bool _showError;
    private bool _chineseInput;
    private TagSelectionControl TagItems;
    private Popup TagDisplayPopup;
    internal Border TextBorder;
    internal Grid AddTagIcon;
    internal Grid TextGrid;
    internal TextBox AddTagTextBox;
    internal TextBlock AddTagHint;
    internal Border ErrorBorder;
    internal Popup ErrorPopup;
    internal Run ErrorText;
    private bool _contentLoaded;

    public TagAddControl()
    {
      this.InitializeComponent();
      this.Unloaded += (RoutedEventHandler) ((s, e) =>
      {
        DelayActionHandlerCenter.RemoveAction("TagAddControlTextChanged");
        if (this.TagDisplayPopup == null)
          return;
        this.TagDisplayPopup.IsOpen = false;
      });
    }

    private TagDisplayControl FindParent()
    {
      return Utils.FindParent<TagDisplayControl>((DependencyObject) this);
    }

    private void OnTagPanelChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.FindParent()?.NotifyTagPanelVisibleChanged(this.TagItems.Visibility);
    }

    private TagDisplayControl GetParent()
    {
      return Utils.FindParent<TagDisplayControl>((DependencyObject) this);
    }

    private void OnTagSelected(object sender, string tag)
    {
      TagDisplayControl parent = this.FindParent();
      if (!string.IsNullOrEmpty(this.AddTagTextBox.Text))
      {
        this.AddTagTextBox.TextChanged -= new TextChangedEventHandler(this.OnTextChanged);
        this.AddTagTextBox.Text = string.Empty;
        this.AddTagTextBox.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
      }
      this.ResetState();
      parent?.NotifyTagPanelVisibleChanged(Visibility.Collapsed);
      parent?.AddNewTag(tag.ToLower());
    }

    private void OnAddTagClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (this.AddTagIcon.Visibility != Visibility.Visible)
        return;
      this.AddTagIcon.Visibility = Visibility.Collapsed;
      this.TextGrid.Visibility = Visibility.Visible;
      this.AddTagHint.Visibility = Visibility.Visible;
      this.AddTagTextBox.Focus();
      this.DisplaySelectTags(string.Empty);
    }

    private void OnAddTagTextChanged()
    {
      if (this._chineseInput)
        return;
      string text = this.AddTagTextBox.Text;
      this.AddTagHint.Visibility = string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
      if (!NameUtils.IsValidName(text))
        this.ShowTagInValid();
      else if (this.CheckTagExisted(text))
        this.ShowTagExisted();
      else
        this.DisplaySelectTags(text);
    }

    private void InitPopup()
    {
      if (this.TagDisplayPopup != null)
        return;
      this.TagDisplayPopup = new Popup()
      {
        StaysOpen = false,
        AllowsTransparency = true,
        Placement = PlacementMode.Bottom
      };
      this.TagDisplayPopup.PlacementTarget = (UIElement) this;
      this.TagDisplayPopup.Closed += new EventHandler(this.OnTagPopupClosed);
      ContentControl contentControl = new ContentControl();
      contentControl.SetResourceReference(FrameworkElement.StyleProperty, (object) "PopupContentStyle");
      this.TagItems = new TagSelectionControl()
      {
        BatchMode = false
      };
      this.TagItems.TagSelected += new EventHandler<string>(this.OnTagSelected);
      this.TagItems.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.OnTagPanelChanged);
      contentControl.Content = (object) this.TagItems;
      this.TagDisplayPopup.Child = (UIElement) contentControl;
    }

    private void DisplaySelectTags(string prefix)
    {
      this.InitPopup();
      this.ErrorPopup.IsOpen = false;
      this._showError = false;
      this.ErrorBorder.Visibility = Visibility.Collapsed;
      if (this.TagDisplayPopup != null)
        this.TagDisplayPopup.IsOpen = true;
      if (!(this.DataContext is TagLabelViewModel))
        return;
      this.TagItems.Filter(prefix, (List<string>) null);
    }

    private void OnTagPopupClosed(object sender, EventArgs e)
    {
      this.FindParent()?.NotifyTagPanelVisibleChanged(Visibility.Collapsed);
    }

    private bool CheckTagExisted(string prefix)
    {
      TagDisplayControl parent = this.GetParent();
      return parent != null && parent.CheckIfTagExisted(prefix);
    }

    private void ShowTagExisted()
    {
      this.ErrorText.Text = Utils.GetString("TagExisted");
      this.ShowErrorPopup();
    }

    private void ShowTagInValid()
    {
      this.ErrorText.Text = Utils.GetString("TagNotValid");
      this.ShowErrorPopup();
    }

    private void ShowErrorPopup()
    {
      this.ErrorPopup.IsOpen = true;
      this.ErrorBorder.Visibility = Visibility.Visible;
      if (this.TagDisplayPopup != null)
        this.TagDisplayPopup.IsOpen = false;
      this._showError = true;
    }

    private void ResetState()
    {
      this.AddTagHint.Visibility = Visibility.Collapsed;
      this.TextGrid.Visibility = Visibility.Collapsed;
      this.AddTagIcon.Visibility = Visibility.Visible;
      this.AddTagTextBox.Text = string.Empty;
      this._showError = false;
      if (this.TagDisplayPopup == null)
        return;
      this.TagDisplayPopup.IsOpen = false;
    }

    private void OnAddTagTextKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
        case Key.Down:
          e.Handled = true;
          this.MoveTagSelectedItem();
          break;
        case Key.Return:
          this.TrySelectTag();
          break;
        case Key.Escape:
          this.ResetState();
          break;
        case Key.Up:
          this.TagItems.MoveUp();
          break;
      }
      if (e.ImeProcessedKey >= Key.A && e.ImeProcessedKey <= Key.Z || e.ImeProcessedKey == Key.Back || e.ImeProcessedKey == Key.Left || e.ImeProcessedKey == Key.Right)
      {
        this._chineseInput = true;
        this.AddTagHint.Visibility = Visibility.Collapsed;
      }
      else
        this._chineseInput = false;
    }

    private void MoveTagSelectedItem()
    {
      if (!Utils.IfShiftPressed())
        this.TagItems.MoveDown();
      else
        this.TagItems.MoveUp();
    }

    private void TrySelectTag()
    {
      if (!NameUtils.IsValidName(this.AddTagTextBox.Text))
        this.ShowTagInValid();
      else
        this.TagItems.TrySelectFocusedTag();
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      if (this.TextGrid.IsMouseOver || this._showError)
        return;
      this.ResetState();
    }

    private void OnTextBoxClick(object sender, MouseButtonEventArgs e)
    {
      this.AddTagTextBox.Focus();
      this.OnAddTagTextChanged();
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
      if (this.TextGrid.IsMouseOver || this.TagItems.IsMouseOver)
        return;
      this.ResetState();
    }

    private async void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      TagAddControl tagAddControl = this;
      if (tagAddControl.TextGrid.Visibility != Visibility.Visible)
        return;
      DelayActionHandlerCenter.TryDoAction("TagAddControlTextChanged", new EventHandler(tagAddControl.OnTextChange), 20);
    }

    private void OnTextChange(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.OnAddTagTextChanged));
    }

    private void OnAddTagTextKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Up:
        case Key.Down:
          e.Handled = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/tagaddcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddTagClick);
          break;
        case 2:
          this.TextBorder = (Border) target;
          break;
        case 3:
          this.AddTagIcon = (Grid) target;
          break;
        case 4:
          this.TextGrid = (Grid) target;
          this.TextGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTextBoxClick);
          break;
        case 5:
          this.AddTagTextBox = (TextBox) target;
          this.AddTagTextBox.LostFocus += new RoutedEventHandler(this.OnTextBoxLostFocus);
          this.AddTagTextBox.PreviewKeyDown += new KeyEventHandler(this.OnAddTagTextKeyDown);
          this.AddTagTextBox.PreviewKeyUp += new KeyEventHandler(this.OnAddTagTextKeyUp);
          this.AddTagTextBox.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        case 6:
          this.AddTagHint = (TextBlock) target;
          break;
        case 7:
          this.ErrorBorder = (Border) target;
          break;
        case 8:
          this.ErrorPopup = (Popup) target;
          break;
        case 9:
          this.ErrorText = (Run) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
