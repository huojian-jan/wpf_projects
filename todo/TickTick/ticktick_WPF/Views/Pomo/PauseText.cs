// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PauseText
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public sealed class PauseText : Border
  {
    private TextBlock _text;

    public PauseText()
    {
      this.SetResourceReference(Border.BackgroundProperty, (object) "ToolTipTopColor");
      this.Height = 20.0;
      this.CornerRadius = new CornerRadius(2.0);
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 11.0;
      textBlock.Foreground = (Brush) Brushes.White;
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock.Margin = new Thickness(8.0, 0.0, 8.0, 0.0);
      this._text = textBlock;
      this.Child = (UIElement) this._text;
    }

    public string Text => this._text.Text;

    public void SetText(string text) => this._text.Text = text;
  }
}
