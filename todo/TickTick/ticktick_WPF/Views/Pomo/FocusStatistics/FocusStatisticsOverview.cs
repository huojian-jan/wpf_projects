// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusStatisticsOverview
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusStatisticsOverview : Grid
  {
    private TextBlock _todayPomo;
    private TextBlock _totalPomo;
    private TextBlock _todayDuration;
    private TextBlock _totalDuration;

    public FocusStatisticsOverview()
    {
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star)
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(1.0, GridUnitType.Star)
      });
      this.RowDefinitions.Add(new RowDefinition()
      {
        Height = new GridLength(1.0, GridUnitType.Star)
      });
      this.InitTodayPomos();
      this.InitTotalPomos();
      this.InitTodayFocusDuration();
      this.InitTotalFocusDuration();
      this.Margin = new Thickness(20.0, 20.0, 20.0, 40.0);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is FocusStatisticsOverviewItem newValue))
        return;
      TextBlock todayPomo = this._todayPomo;
      int num1 = newValue.TodayPomos;
      string str1 = num1.ToString() ?? "";
      todayPomo.Text = str1;
      TextBlock totalPomo = this._totalPomo;
      num1 = newValue.TotalPomos;
      string str2 = num1.ToString() ?? "";
      totalPomo.Text = str2;
      long num2 = newValue.TodayDuration / 60L;
      long num3 = newValue.TodayDuration % 60L;
      this._todayDuration.Inlines.Clear();
      if (num2 > 0L)
      {
        this._todayDuration.Inlines.Add((Inline) new Run(num2.ToString() ?? ""));
        InlineCollection inlines = this._todayDuration.Inlines;
        Run run = new Run(" h ");
        run.FontSize = 14.0;
        inlines.Add((Inline) run);
      }
      this._todayDuration.Inlines.Add((Inline) new Run(num3.ToString() ?? ""));
      InlineCollection inlines1 = this._todayDuration.Inlines;
      Run run1 = new Run(" m");
      run1.FontSize = 14.0;
      inlines1.Add((Inline) run1);
      long num4 = newValue.TotalDuration / 60L;
      long num5 = newValue.TotalDuration % 60L;
      this._totalDuration.Inlines.Clear();
      if (num4 > 0L)
      {
        this._totalDuration.Inlines.Add((Inline) new Run(num4.ToString() ?? ""));
        InlineCollection inlines2 = this._totalDuration.Inlines;
        Run run2 = new Run(" h ");
        run2.FontSize = 14.0;
        inlines2.Add((Inline) run2);
      }
      this._totalDuration.Inlines.Add((Inline) new Run(num5.ToString() ?? ""));
      InlineCollection inlines3 = this._totalDuration.Inlines;
      Run run3 = new Run(" m");
      run3.FontSize = 14.0;
      inlines3.Add((Inline) run3);
    }

    private void InitTodayPomos()
    {
      StackPanel container = this.GetContainer(0, 0);
      TextBlock textBlock = this.GetTextBlock(12.0, "BaseColorOpacity60");
      textBlock.Text = Utils.GetString("PomoToday");
      textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      container.Children.Add((UIElement) textBlock);
      this._todayPomo = this.GetTextBlock(24.0, "BaseColorOpacity90");
      container.Children.Add((UIElement) this._todayPomo);
    }

    private void InitTotalPomos()
    {
      StackPanel container = this.GetContainer(0, 1);
      TextBlock textBlock = this.GetTextBlock(12.0, "BaseColorOpacity60");
      textBlock.Text = Utils.GetString("TotalPomo");
      textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      container.Children.Add((UIElement) textBlock);
      this._totalPomo = this.GetTextBlock(24.0, "BaseColorOpacity90");
      container.Children.Add((UIElement) this._totalPomo);
    }

    private void InitTodayFocusDuration()
    {
      StackPanel container = this.GetContainer(1, 0);
      TextBlock textBlock = this.GetTextBlock(12.0, "BaseColorOpacity60");
      textBlock.Text = Utils.GetString("TodayFocusDuration");
      textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 4.0);
      container.Children.Add((UIElement) textBlock);
      this._todayDuration = this.GetTextBlock(24.0, "BaseColorOpacity90");
      container.Children.Add((UIElement) this._todayDuration);
    }

    private void InitTotalFocusDuration()
    {
      StackPanel container = this.GetContainer(1, 1);
      TextBlock textBlock = this.GetTextBlock(12.0, "BaseColorOpacity60");
      textBlock.Text = Utils.GetString("TotalFocusDuration");
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

    private StackPanel GetContainer(int column, int row)
    {
      Border border = new Border();
      border.Height = 72.0;
      border.CornerRadius = new CornerRadius(6.0);
      border.Margin = new Thickness(column == 0 ? 0.0 : 6.0, row == 0 ? 0.0 : 6.0, column == 0 ? 6.0 : 0.0, row == 0 ? 6.0 : 0.0);
      Border element = border;
      element.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity3");
      element.SetValue(Grid.ColumnProperty, (object) column);
      element.SetValue(Grid.RowProperty, (object) row);
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
