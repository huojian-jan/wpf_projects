// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerDetailTitleItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerDetailTitleItem : DockPanel
  {
    private FocusObjIcon _focusIcon;
    private EmjTextBlock _title;
    private TextBlock _typeText;
    private Border _navigateButton;

    public TimerDetailTitleItem()
    {
      FocusObjIcon focusObjIcon = new FocusObjIcon(40.0);
      focusObjIcon.VerticalAlignment = VerticalAlignment.Center;
      focusObjIcon.Margin = new Thickness(0.0, 0.0, 12.0, 0.0);
      this._focusIcon = focusObjIcon;
      EmjTextBlock emjTextBlock = new EmjTextBlock();
      emjTextBlock.FontSize = 18.0;
      emjTextBlock.Height = 25.0;
      emjTextBlock.TextWrapping = TextWrapping.Wrap;
      emjTextBlock.ClipToBounds = true;
      emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      emjTextBlock.Margin = new Thickness(0.0, 10.0, 0.0, 2.0);
      this._title = emjTextBlock;
      this._typeText = new TextBlock() { FontSize = 12.0 };
      Border border = new Border();
      border.Width = 20.0;
      border.Height = 20.0;
      border.VerticalAlignment = VerticalAlignment.Center;
      border.Cursor = Cursors.Hand;
      this._navigateButton = border;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.Margin = new Thickness(20.0, 0.0, 20.0, 0.0);
      this.Height = 62.0;
      this._focusIcon.SetValue(DockPanel.DockProperty, (object) Dock.Left);
      this.Children.Add((UIElement) this._focusIcon);
      this._navigateButton.SetValue(DockPanel.DockProperty, (object) Dock.Right);
      this.Children.Add((UIElement) this._navigateButton);
      this._title.SetValue(DockPanel.DockProperty, (object) Dock.Top);
      this._title.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.Children.Add((UIElement) this._title);
      this._typeText.SetValue(DockPanel.DockProperty, (object) Dock.Right);
      this._typeText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
      this.Children.Add((UIElement) this._typeText);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is TimerDetailTitleViewModel newValue))
        return;
      this._focusIcon.SetIconAndColor(newValue.TModel.Icon, newValue.TModel.Color);
      this._title.Text = newValue.TModel.Name;
      this._typeText.Text = newValue.TModel.Type == "pomodoro" ? string.Format("{0} {1} m", (object) Utils.GetString("PomoTimer2"), (object) newValue.TModel.PomodoroTime) : Utils.GetString("Timing");
    }
  }
}
