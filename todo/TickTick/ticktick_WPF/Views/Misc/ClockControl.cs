// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ClockControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Notifier;
using ticktick_WPF.Views.Theme;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class ClockControl : StackPanel
  {
    public bool ShowHour;
    public bool ShowSecond = true;
    private TextBlock _hourText;
    private TextBlock _minuteText;
    private TextBlock _secondText;
    private TextBlock _hourSplit;
    private TextBlock _secondSplit;
    public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(nameof (FontSize), typeof (double), typeof (ClockControl), new PropertyMetadata((object) 16.0, new PropertyChangedCallback(ClockControl.OnFontSizeChanged)));
    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof (Foreground), typeof (Brush), typeof (ClockControl), new PropertyMetadata((object) Brushes.Black, new PropertyChangedCallback(ClockControl.OnForegroundChanged)));

    public ClockControl(double size, string foreground)
    {
    }

    private void SetFontFamily(object sender, EventArgs e)
    {
      FontFamily focusFontFamily = FontFamilyUtils.GetFocusFontFamily();
      this._hourText.FontFamily = focusFontFamily;
      this._hourSplit.FontFamily = focusFontFamily;
      this._minuteText.FontFamily = focusFontFamily;
      this._secondSplit.FontFamily = focusFontFamily;
      this._secondText.FontFamily = focusFontFamily;
    }

    public ClockControl()
    {
      this.Orientation = Orientation.Horizontal;
      this.VerticalAlignment = VerticalAlignment.Center;
      this.LoadChildren();
      this.Loaded += (RoutedEventHandler) ((sender, args) =>
      {
        DataChangedNotifier.FontFamilyChanged -= new EventHandler(this.SetFontFamily);
        DataChangedNotifier.FontFamilyChanged += new EventHandler(this.SetFontFamily);
      });
      this.Unloaded += (RoutedEventHandler) ((sender, args) => DataChangedNotifier.FontFamilyChanged -= new EventHandler(this.SetFontFamily));
    }

    private void LoadChildren()
    {
      this._hourText = this.GetTextBlock();
      this.Children.Add((UIElement) this._hourText);
      this._hourSplit = this.GetTextBlock();
      this._hourSplit.Text = ":";
      this._hourSplit.Margin = new Thickness(0.0, 0.0, 0.0, 2.5);
      this.Children.Add((UIElement) this._hourSplit);
      this._minuteText = this.GetTextBlock();
      this.Children.Add((UIElement) this._minuteText);
      this._secondSplit = this.GetTextBlock();
      this._secondSplit.Text = ":";
      this._secondSplit.Margin = new Thickness(0.0, 0.0, 0.0, 2.5);
      this.Children.Add((UIElement) this._secondSplit);
      this._secondText = this.GetTextBlock();
      this.Children.Add((UIElement) this._secondText);
      this.SetFontFamily((object) null, (EventArgs) null);
    }

    private TextBlock GetTextBlock()
    {
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 16.0;
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100_80");
      return textBlock;
    }

    private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is ClockControl clockControl) || !(e.NewValue is double newValue))
        return;
      clockControl.SetFontSize(newValue);
    }

    private void SetFontSize(double fontsize)
    {
      this._hourText.FontSize = fontsize;
      this._hourSplit.FontSize = fontsize;
      this._hourSplit.Margin = new Thickness(0.0, 0.0, 0.0, fontsize / 6.0);
      this._minuteText.FontSize = fontsize;
      this._secondSplit.FontSize = fontsize;
      this._secondSplit.Margin = new Thickness(0.0, 0.0, 0.0, fontsize / 6.0);
      this._secondText.FontSize = fontsize;
    }

    public double FontSize
    {
      get => (double) this.GetValue(ClockControl.FontSizeProperty);
      set => this.SetValue(ClockControl.FontSizeProperty, (object) value);
    }

    private static void OnForegroundChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is ClockControl clockControl) || !(e.NewValue is Brush newValue))
        return;
      clockControl.SetForeground(newValue);
    }

    private void SetForeground(Brush brush)
    {
      this._hourText.Foreground = brush;
      this._hourSplit.Foreground = brush;
      this._minuteText.Foreground = brush;
      this._secondSplit.Foreground = brush;
      this._secondText.Foreground = brush;
    }

    public Brush Foreground
    {
      get => (Brush) this.GetValue(ClockControl.ForegroundProperty);
      set => this.SetValue(ClockControl.ForegroundProperty, (object) value);
    }

    public int? ShowHourFont { get; set; }

    public void SetTime(int second)
    {
      this._hourText.Visibility = !this.ShowHour || second < 3600 ? Visibility.Collapsed : Visibility.Visible;
      this._hourSplit.Visibility = this._hourText.Visibility;
      if (this.ShowHourFont.HasValue)
      {
        if (this._hourText.Visibility == Visibility.Visible && Math.Abs(this._hourText.FontSize - (double) this.ShowHourFont.Value) > 1.0)
          this.SetFontSize((double) this.ShowHourFont.Value);
        else if (this._hourText.Visibility != Visibility.Visible && Math.Abs(this._hourText.FontSize - this.FontSize) > 1.0)
          this.SetFontSize(this.FontSize);
      }
      if (this.ShowHour)
      {
        int num = second / 60;
        this._hourText.Text = ClockControl.PaddingTimeText(num / 60);
        this._minuteText.Text = ClockControl.PaddingTimeText(num % 60);
        this._secondText.Text = ClockControl.PaddingTimeText(second % 60);
      }
      else
      {
        this._minuteText.Text = ClockControl.PaddingTimeText(second / 60);
        this._secondText.Text = ClockControl.PaddingTimeText(second % 60);
      }
    }

    private static string PaddingTimeText(int count)
    {
      return count >= 0 && count < 10 ? "0" + count.ToString() : count.ToString();
    }
  }
}
