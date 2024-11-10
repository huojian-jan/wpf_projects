// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.RecordLoadMoreItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class RecordLoadMoreItem : Border
  {
    public RecordLoadMoreItem()
    {
      this.HorizontalAlignment = HorizontalAlignment.Left;
      this.Background = (Brush) Brushes.Transparent;
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataChanged);
      this.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
    }

    private void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is RecordLoadMoreItem.FocusRecordLoadMoreViewModel newValue))
        return;
      if (newValue.NeedClick)
      {
        TextBlock textBlock = new TextBlock()
        {
          Text = Utils.GetString("LoadMore"),
          FontSize = 12.0
        };
        textBlock.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
        this.Child = (UIElement) textBlock;
        this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLoadMoreClock);
      }
      else
      {
        LoadingIndicator loadingIndicator = new LoadingIndicator()
        {
          IsActive = true,
          SpeedRatio = 3.0
        };
        loadingIndicator.SetResourceReference(FrameworkElement.StyleProperty, (object) "LoadingIndicatorRingStyle");
        this.Child = (UIElement) loadingIndicator;
      }
    }

    private void OnLoadMoreClock(object sender, MouseButtonEventArgs e)
    {
      LoadingIndicator loadingIndicator = new LoadingIndicator()
      {
        IsActive = true,
        SpeedRatio = 3.0
      };
      loadingIndicator.SetResourceReference(FrameworkElement.StyleProperty, (object) "LoadingIndicatorRingStyle");
      this.Child = (UIElement) loadingIndicator;
      this.LoadMore();
    }

    private async void LoadMore()
    {
      Utils.FindParent<FocusStatisticsPanel>((DependencyObject) this)?.LoadMore();
    }

    public class FocusRecordLoadMoreViewModel : FocusStatisticsPanelItemViewModel
    {
      public bool NeedClick { get; set; }

      public FocusRecordLoadMoreViewModel() => this.IsLoadMore = true;
    }
  }
}
