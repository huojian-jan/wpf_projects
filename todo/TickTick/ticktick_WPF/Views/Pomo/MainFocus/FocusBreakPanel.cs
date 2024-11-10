// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MainFocus.FocusBreakPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MainFocus
{
  public class FocusBreakPanel : StackPanel
  {
    private TextBlock _relaxText;

    public FocusBreakPanel()
    {
      this.InitImage();
      this.InitText();
    }

    private void InitImage()
    {
      BitmapImage bitmapImage = new BitmapImage();
      bitmapImage.BeginInit();
      bitmapImage.DecodePixelWidth = 216;
      bitmapImage.UriSource = new Uri("pack://application:,,,/Assets/get_pomo.png");
      bitmapImage.EndInit();
      bitmapImage.Freeze();
      Image element = new Image();
      element.Source = (ImageSource) bitmapImage;
      element.Width = 216.0;
      element.Height = 216.0;
      element.HorizontalAlignment = HorizontalAlignment.Center;
      element.Margin = new Thickness(0.0, 0.0, 0.0, 40.0);
      this.Children.Add((UIElement) element);
    }

    private void InitText()
    {
      TextBlock textBlock1 = new TextBlock();
      textBlock1.FontSize = 28.0;
      textBlock1.Text = Utils.GetString("GotAPomo");
      textBlock1.Margin = new Thickness(0.0, 0.0, 0.0, 8.0);
      textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
      TextBlock element = textBlock1;
      element.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.Children.Add((UIElement) element);
      TextBlock textBlock2 = new TextBlock();
      textBlock2.FontSize = 16.0;
      textBlock2.Text = Utils.GetString("Relax1Min");
      textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
      this._relaxText = textBlock2;
      this._relaxText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
      this.Children.Add((UIElement) this._relaxText);
    }

    public void SetRelaxText()
    {
      long relaxMinutes = TickFocusManager.Config.GetRelaxMinutes();
      this._relaxText.Text = string.Format(Utils.GetString(relaxMinutes > 1L ? "RelaxNMins" : "Relax1Min"), (object) relaxMinutes);
    }
  }
}
