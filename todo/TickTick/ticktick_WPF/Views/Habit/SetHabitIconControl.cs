// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.SetHabitIconControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class SetHabitIconControl : UserControl, IComponentConnector, IStyleConnector
  {
    public static readonly DependencyProperty SelectedIconProperty = DependencyProperty.Register(nameof (SelectedIcon), typeof (string), typeof (SetHabitIconControl), new PropertyMetadata((object) "habit_daily_check_in", (PropertyChangedCallback) null));
    public static readonly DependencyProperty IconColorProperty = DependencyProperty.Register(nameof (IconColor), typeof (string), typeof (SetHabitIconControl), new PropertyMetadata((object) "#70C362", (PropertyChangedCallback) null));
    public static readonly DependencyProperty IconTextProperty = DependencyProperty.Register(nameof (IconText), typeof (string), typeof (SetHabitIconControl), new PropertyMetadata((object) "A", (PropertyChangedCallback) null));
    public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(nameof (TextColor), typeof (string), typeof (SetHabitIconControl), new PropertyMetadata((object) "", new PropertyChangedCallback(SetHabitIconControl.OnTextColorChanged)));
    public static readonly DependencyProperty IsIconProperty = DependencyProperty.Register(nameof (IsIcon), typeof (bool), typeof (SetHabitIconControl), new PropertyMetadata((object) true, (PropertyChangedCallback) null));
    internal SetHabitIconControl Root;
    internal Image SelectedIconImage;
    internal Grid TransparentGrid;
    internal Border TextBackground;
    internal EmjTextBlock EmjTextBox;
    internal ItemsControl IconItems;
    internal TextBox IconTextBox;
    internal ticktick_WPF.Views.Misc.ColorSelector.ColorSelector ColorItems;
    private bool _contentLoaded;

    private static void OnTextColorChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is SetHabitIconControl habitIconControl) || !(e.NewValue is string newValue))
        return;
      habitIconControl.TransparentGrid.Visibility = string.IsNullOrEmpty(newValue) ? Visibility.Visible : Visibility.Collapsed;
      habitIconControl.EmjTextBox.Foreground = string.IsNullOrEmpty(newValue) || newValue.ToLower() == "transparent" ? (Brush) ThemeUtil.GetColor("PrimaryColor") : (Brush) Brushes.White;
    }

    public string SelectedIcon
    {
      get => (string) this.GetValue(SetHabitIconControl.SelectedIconProperty);
      set => this.SetValue(SetHabitIconControl.SelectedIconProperty, (object) value);
    }

    public string IconColor
    {
      get => (string) this.GetValue(SetHabitIconControl.IconColorProperty);
      set => this.SetValue(SetHabitIconControl.IconColorProperty, (object) value);
    }

    public string IconText
    {
      get => (string) this.GetValue(SetHabitIconControl.IconTextProperty);
      set => this.SetValue(SetHabitIconControl.IconTextProperty, (object) value);
    }

    public string TextColor
    {
      get => (string) this.GetValue(SetHabitIconControl.TextColorProperty);
      set => this.SetValue(SetHabitIconControl.TextColorProperty, (object) value);
    }

    public bool IsIcon
    {
      get => (bool) this.GetValue(SetHabitIconControl.IsIconProperty);
      set => this.SetValue(SetHabitIconControl.IsIconProperty, (object) value);
    }

    public event EventHandler OnIconSaved;

    public event EventHandler Closed;

    public SetHabitIconControl()
    {
      this.InitializeComponent();
      this.IconItems.ItemsSource = (IEnumerable) HabitIconViewModel.BuildItems();
      DataObject.AddPastingHandler((DependencyObject) this.IconTextBox, new DataObjectPastingEventHandler(this.OnPaste));
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
      e.CancelCommand();
      if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true) || !(e.SourceDataObject.GetData(DataFormats.UnicodeText) is string data))
        return;
      this.IconTextBox.TextChanged += new TextChangedEventHandler(this.OnIconTextChanged);
      this.IconTextBox.Text = data;
    }

    private List<HabitIconViewModel> Icons
    {
      get => this.IconItems.ItemsSource as List<HabitIconViewModel>;
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      EventHandler onIconSaved = this.OnIconSaved;
      if (onIconSaved == null)
        return;
      onIconSaved((object) this, (EventArgs) null);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler closed = this.Closed;
      if (closed == null)
        return;
      closed((object) this, (EventArgs) null);
    }

    public void Reset(string iconRes, string color)
    {
      if (iconRes.StartsWith("txt"))
      {
        this.TextColor = color;
        this.IconText = ((IEnumerable<string>) iconRes.Split('_')).LastOrDefault<string>() ?? (Utils.IsCn() ? "文" : "A");
        this.SelectedIconImage.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/Habits/habit_daily_check_in.png"));
        this.IconColor = HabitUtils.IconColorDict["habit_daily_check_in"];
        this.SelectedIcon = "habit_daily_check_in";
        this.IsIcon = false;
      }
      else
      {
        this.SelectedIcon = iconRes;
        this.IconColor = color;
        this.IconText = Utils.IsCn() ? "文" : "A";
        this.IsIcon = true;
        this.SelectedIconImage.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/Habits/" + this.SelectedIcon.ToLower() + ".png"));
      }
      this.IconTextBox.Text = this.IconText;
      this.ColorItems.SetSelectedColor(this.TextColor);
      this.TransparentGrid.Visibility = string.IsNullOrEmpty(this.TextColor) ? Visibility.Visible : Visibility.Collapsed;
      this.EmjTextBox.Foreground = string.IsNullOrEmpty(this.TextColor) ? (Brush) ThemeUtil.GetColor("PrimaryColor") : (Brush) Brushes.White;
      this.Icons.ForEach((Action<HabitIconViewModel>) (icon => icon.Selected = this.SelectedIcon == icon.IconName));
    }

    private void OnIconItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Grid grid))
        return;
      HabitIconViewModel iconModel = grid.DataContext as HabitIconViewModel;
      if (iconModel == null)
        return;
      this.Icons.ForEach((Action<HabitIconViewModel>) (icon => icon.Selected = iconModel.IconName == icon.IconName));
      this.SelectedIcon = iconModel.IconName;
      this.SelectedIconImage.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/Habits/" + this.SelectedIcon.ToLower() + ".png"));
      this.IconColor = iconModel.Color;
    }

    private void SwitchIconClick(object sender, MouseButtonEventArgs e)
    {
      if (this.IsIcon)
        return;
      this.IsIcon = true;
    }

    private void SwitchTextClick(object sender, MouseButtonEventArgs e)
    {
      if (!this.IsIcon)
        return;
      this.IsIcon = false;
    }

    private void OnIconTextInput(object sender, TextCompositionEventArgs e)
    {
      string emojiIcon = EmojiHelper.GetEmojiIcon(e.Text);
      this.IconTextBox.MaxLength = string.IsNullOrEmpty(emojiIcon) || !e.Text.StartsWith(emojiIcon) ? 1 : emojiIcon.Length;
      this.IconTextBox.TextChanged += new TextChangedEventHandler(this.OnIconTextChanged);
    }

    private void OnIconTextChanged(object sender, TextChangedEventArgs e)
    {
      string text = this.IconTextBox.Text.Trim();
      if (text.Length >= 1)
      {
        string emojiIcon = EmojiHelper.GetEmojiIcon(text);
        this.IconText = string.IsNullOrEmpty(emojiIcon) || !text.StartsWith(emojiIcon) ? text.Substring(0, 1) : EmojiHelper.GetEmojiIcon(text);
        this.IconTextBox.Text = this.IconText;
      }
      this.IconTextBox.TextChanged -= new TextChangedEventHandler(this.OnIconTextChanged);
    }

    private void OnTextColorSelect(object sender, string e)
    {
      if (e.ToLower() == "transparent")
        e = (string) null;
      this.TextColor = e;
      this.TransparentGrid.Visibility = string.IsNullOrEmpty(this.TextColor) ? Visibility.Visible : Visibility.Collapsed;
      this.EmjTextBox.Foreground = string.IsNullOrEmpty(this.TextColor) ? (Brush) ThemeUtil.GetColor("PrimaryColor") : (Brush) Brushes.White;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/sethabiticoncontrol.xaml", UriKind.Relative));
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
          this.Root = (SetHabitIconControl) target;
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchIconClick);
          break;
        case 3:
          this.SelectedIconImage = (Image) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SwitchTextClick);
          break;
        case 5:
          this.TransparentGrid = (Grid) target;
          break;
        case 6:
          this.TextBackground = (Border) target;
          break;
        case 7:
          this.EmjTextBox = (EmjTextBlock) target;
          break;
        case 8:
          this.IconItems = (ItemsControl) target;
          break;
        case 10:
          this.IconTextBox = (TextBox) target;
          this.IconTextBox.PreviewTextInput += new TextCompositionEventHandler(this.OnIconTextInput);
          break;
        case 11:
          this.ColorItems = (ticktick_WPF.Views.Misc.ColorSelector.ColorSelector) target;
          break;
        case 12:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 13:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
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
      if (connectionId != 9)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnIconItemClick);
    }
  }
}
