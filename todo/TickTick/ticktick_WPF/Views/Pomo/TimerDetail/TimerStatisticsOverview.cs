// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerStatisticsOverview
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerStatisticsOverview : Grid
  {
    private TextBlock _totalDays;
    private TextBlock _todayDuration;
    private TextBlock _totalDuration;

    public TimerStatisticsOverview()
    {
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      this.InitTotalDays();
      this.InitTodayFocusDuration();
      this.InitTotalFocusDuration();
      this.Margin = new Thickness(20.0, 20.0, 20.0, 20.0);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is TimerStatisticsOverviewItem newValue))
        return;
      this._totalDays.Text = newValue.TotalDays.ToString() ?? "";
      UiUtils.SetTimeTextRun(this._todayDuration, newValue.TodayDuration, 14, "BaseColorOpacity80");
      UiUtils.SetTimeTextRun(this._totalDuration, newValue.TotalDuration, 14, "BaseColorOpacity80");
    }

    private void InitTotalDays()
    {
      StackPanel container = this.GetContainer(0);
      TextBlock textBlock = this.GetTextBlock(12.0, "BaseColorOpacity60");
      textBlock.Text = Utils.GetString("FocusedDays");
      textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      container.Children.Add((UIElement) textBlock);
      this._totalDays = this.GetTextBlock(24.0, "BaseColorOpacity90");
      container.Children.Add((UIElement) this._totalDays);
    }

    private void InitTodayFocusDuration()
    {
      StackPanel container = this.GetContainer(1);
      TextBlock textBlock = this.GetTextBlock(12.0, "BaseColorOpacity60");
      textBlock.Text = Utils.GetString("TodayFocus");
      textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      container.Children.Add((UIElement) textBlock);
      this._todayDuration = this.GetTextBlock(24.0, "BaseColorOpacity90");
      container.Children.Add((UIElement) this._todayDuration);
    }

    private void InitTotalFocusDuration()
    {
      StackPanel container = this.GetContainer(2);
      TextBlock textBlock = this.GetTextBlock(12.0, "BaseColorOpacity60");
      textBlock.Text = Utils.GetString("TotalFocus");
      textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      container.Children.Add((UIElement) textBlock);
      this._totalDuration = this.GetTextBlock(24.0, "BaseColorOpacity90");
      container.Children.Add((UIElement) this._totalDuration);
    }

    private TextBlock GetTextBlock(double fontsize, string foregroundKey)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = fontsize;
      textBlock.SetResourceReference(TextBlock.ForegroundProperty, (object) foregroundKey);
      return textBlock;
    }

    private StackPanel GetContainer(int column)
    {
      Border border = new Border();
      border.Height = 72.0;
      border.CornerRadius = new CornerRadius(6.0);
      border.Margin = new Thickness(0.0, 0.0, column != 2 ? 12.0 : 0.0, 0.0);
      Border element = border;
      element.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity3");
      element.SetValue(Grid.ColumnProperty, (object) column);
      StackPanel stackPanel = new StackPanel();
      stackPanel.VerticalAlignment = VerticalAlignment.Center;
      stackPanel.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
      StackPanel container = stackPanel;
      element.Child = (UIElement) container;
      this.Children.Add((UIElement) element);
      return container;
    }
  }
}
