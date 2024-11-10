// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusStatisticsPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.SyncServices;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public sealed class FocusStatisticsPanel : Grid
  {
    private ListView _listView;
    private FocusStatisticsTitleView _floatTitle;
    private FocusRecordEmptyItem _emptyItem;
    private int _limits;
    private List<FocusStatisticsPanelItemViewModel> _itemsSource;
    private bool _loading;

    public FocusStatisticsPanel()
    {
      this._listView = new ListView();
      this.Children.Add((UIElement) this._listView);
      this._listView.ItemTemplateSelector = (DataTemplateSelector) new FocusStatisticsPanelItemTemplateSelector();
      this._listView.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListViewStyle");
      this._limits = 30;
      this.PullRemote(true);
      this.PullStatistics();
      FocusRecordEmptyItem focusRecordEmptyItem = new FocusRecordEmptyItem();
      focusRecordEmptyItem.Margin = new Thickness(0.0, 240.0, 0.0, 0.0);
      focusRecordEmptyItem.VerticalAlignment = VerticalAlignment.Center;
      this._emptyItem = focusRecordEmptyItem;
      this.Children.Add((UIElement) this._emptyItem);
      this.LoadItems();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Loaded += new RoutedEventHandler(this.BindEvent);
      this.Unloaded += new RoutedEventHandler(this.RemoveEvent);
      FocusStatisticsTitleView statisticsTitleView = new FocusStatisticsTitleView(true);
      statisticsTitleView.VerticalAlignment = VerticalAlignment.Top;
      statisticsTitleView.Margin = new Thickness(0.0, 6.0, 0.0, 0.0);
      statisticsTitleView.DataContext = (object) new FocusStatisticsTitleItemViewModel(Utils.GetString("FocusRecord"), "AddDrawingImage", "Add");
      this._floatTitle = statisticsTitleView;
    }

    private void RemoveEvent(object sender, RoutedEventArgs e)
    {
      PomoNotifier.Changed -= new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
      PomoNotifier.ServiceChanged -= new EventHandler(this.OnPomoChanged);
    }

    private void BindEvent(object sender, RoutedEventArgs e)
    {
      PomoNotifier.Changed += new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
      PomoNotifier.ServiceChanged += new EventHandler(this.OnPomoChanged);
    }

    private async Task PullStatistics()
    {
      await PomoSyncService.PullStatistics();
      if (!(this._listView.ItemsSource is ObservableCollection<FocusStatisticsPanelItemViewModel> itemsSource))
        return;
      itemsSource[1] = (FocusStatisticsPanelItemViewModel) new FocusStatisticsOverviewItem(LocalSettings.Settings.StatisticsModel);
    }

    private void LoadItems() => this.Dispatcher.Invoke<Task>(new Func<Task>(this.TryLoadItems));

    private async Task TryLoadItems()
    {
      List<FocusStatisticsPanelItemViewModel> itemsSource = new List<FocusStatisticsPanelItemViewModel>()
      {
        (FocusStatisticsPanelItemViewModel) new FocusStatisticsTitleItemViewModel(Utils.GetString("Overview"), "", "Statistics"),
        (FocusStatisticsPanelItemViewModel) new FocusStatisticsOverviewItem(LocalSettings.Settings.StatisticsModel),
        (FocusStatisticsPanelItemViewModel) new FocusStatisticsTitleItemViewModel(Utils.GetString("FocusRecord"), "AddDrawingImage", "Add")
      };
      List<PomodoroModel> pomos = await PomoDao.GetPomoDescByStart(this._limits);
      Dictionary<DateTime, List<FocusRecordItemViewModel>> datePomoDict = new Dictionary<DateTime, List<FocusRecordItemViewModel>>();
      foreach (PomodoroModel pomo in pomos)
      {
        List<PomoTask> pomoTasksByPomoId = await PomoDao.GetPomoTasksByPomoId(pomo.Id);
        DateTime dateTime = pomo.EndTime;
        DateTime date1 = dateTime.Date;
        FocusRecordItemViewModel recordItemViewModel1 = new FocusRecordItemViewModel(pomo, date1, pomoTasksByPomoId);
        if (datePomoDict.ContainsKey(date1))
          datePomoDict[date1].Add(recordItemViewModel1);
        else
          datePomoDict[date1] = new List<FocusRecordItemViewModel>()
          {
            recordItemViewModel1
          };
        dateTime = pomo.StartTime;
        DateTime date2 = dateTime.Date;
        dateTime = pomo.EndTime;
        DateTime date3 = dateTime.Date;
        if (date2 != date3)
        {
          dateTime = pomo.StartTime;
          DateTime date4 = dateTime.Date;
          FocusRecordItemViewModel recordItemViewModel2 = new FocusRecordItemViewModel(pomo, date4, pomoTasksByPomoId);
          if (datePomoDict.ContainsKey(date4))
            datePomoDict[date4].Add(recordItemViewModel2);
          else
            datePomoDict[date4] = new List<FocusRecordItemViewModel>()
            {
              recordItemViewModel2
            };
        }
      }
      foreach (KeyValuePair<DateTime, List<FocusRecordItemViewModel>> keyValuePair in datePomoDict)
      {
        itemsSource.Add((FocusStatisticsPanelItemViewModel) new FocusRecordItemViewModel(keyValuePair.Key));
        List<FocusRecordItemViewModel> recordItemViewModelList = keyValuePair.Value;
        // ISSUE: explicit non-virtual call
        if ((recordItemViewModelList != null ? (__nonvirtual (recordItemViewModelList.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          keyValuePair.Value.Last<FocusRecordItemViewModel>().IsLastItem = true;
          itemsSource.AddRange((IEnumerable<FocusStatisticsPanelItemViewModel>) keyValuePair.Value);
        }
      }
      if (pomos.Count == this._limits)
        itemsSource.Add((FocusStatisticsPanelItemViewModel) new RecordLoadMoreItem.FocusRecordLoadMoreViewModel()
        {
          NeedClick = (this._limits == 30)
        });
      this._emptyItem.Visibility = pomos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      ItemsSourceHelper.SetItemsSource<FocusStatisticsPanelItemViewModel>((ItemsControl) this._listView, itemsSource);
      this._itemsSource = itemsSource;
      itemsSource = (List<FocusStatisticsPanelItemViewModel>) null;
      pomos = (List<PomodoroModel>) null;
      datePomoDict = (Dictionary<DateTime, List<FocusRecordItemViewModel>>) null;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      if (!(this._listView.Template.FindName("ScrollViewer", (FrameworkElement) this._listView) is ScrollViewer name))
        return;
      name.ScrollChanged += new ScrollChangedEventHandler(this.OnScrollChanged);
    }

    private void OnPomoChanged(object sender, PomoChangeArgs e)
    {
      DelayActionHandlerCenter.TryDoAction("FocusStatisticsPanelLoadItems", (EventHandler) ((sender1, eventArgs) => this.LoadItems()), 500);
    }

    private void OnPomoChanged(object sender, EventArgs e)
    {
      DelayActionHandlerCenter.TryDoAction("FocusStatisticsPanelLoadItems", (EventHandler) ((sender1, eventArgs) => this.LoadItems()), 500);
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      FocusRecordItem mousePointItem = Utils.GetMousePointItem<FocusRecordItem>(new System.Windows.Point(50.0, this._listView.Clip != null ? 50.0 : 10.0), (FrameworkElement) this._listView);
      this._listView.Clip = mousePointItem != null ? (Geometry) new RectangleGeometry(new Rect(0.0, 40.0, this.ActualWidth, this.ActualHeight - 40.0)) : (Geometry) null;
      this.Children.Remove((UIElement) this._floatTitle);
      if (mousePointItem != null)
        this.Children.Add((UIElement) this._floatTitle);
      if (!(sender is ScrollViewer scrollViewer))
        return;
      List<FocusStatisticsPanelItemViewModel> itemsSource = this._itemsSource;
      if (!((itemsSource != null ? itemsSource.LastOrDefault<FocusStatisticsPanelItemViewModel>() : (FocusStatisticsPanelItemViewModel) null) is RecordLoadMoreItem.FocusRecordLoadMoreViewModel loadMoreViewModel) || loadMoreViewModel.NeedClick || e.VerticalOffset < scrollViewer.ScrollableHeight - 10.0)
        return;
      this.LoadMore();
    }

    public async Task LoadMore()
    {
      if (this._loading)
        return;
      this._loading = true;
      this._limits += 30;
      this.LoadItems();
      await this.PullRemote();
      this._loading = false;
    }

    private async Task PullRemote(bool firstPull = false)
    {
      if (!await PomoSyncService.PullFocusTimelines(firstPull))
        return;
      this.LoadItems();
    }

    public void Reload() => this.LoadItems();
  }
}
