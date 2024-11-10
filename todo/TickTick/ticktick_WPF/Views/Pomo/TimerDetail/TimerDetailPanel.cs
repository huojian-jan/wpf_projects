// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerDetailPanel
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Framework.Collections;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.SyncServices;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo.FocusStatistics;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerDetailPanel : Grid
  {
    private ListView _listView;
    private int _limits;
    private TimerModel _timer;
    private readonly TTAsyncTaskLocker<TimerModel> _loadAsync = new TTAsyncTaskLocker<TimerModel>(1, 1);
    private bool _loading;
    private TimerTimelineItemViewModel _timelineViewModel;

    private ObservableCollection<FocusStatisticsPanelItemViewModel> ItemsSource
    {
      get
      {
        return this._listView.ItemsSource is ObservableCollection<FocusStatisticsPanelItemViewModel> itemsSource ? itemsSource : (ObservableCollection<FocusStatisticsPanelItemViewModel>) new ExtObservableCollection<FocusStatisticsPanelItemViewModel>();
      }
    }

    public TimerDetailPanel()
    {
      this.InitTopOption();
      ListView listView = new ListView();
      listView.Margin = new Thickness(0.0, 36.0, 0.0, 0.0);
      this._listView = listView;
      this.Children.Add((UIElement) this._listView);
      this._listView.ItemTemplateSelector = (DataTemplateSelector) new TimerDetailItemTemplateSelector();
      this._listView.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListViewStyle");
      this._timelineViewModel = new TimerTimelineItemViewModel(this);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Loaded += (RoutedEventHandler) ((o, e) =>
      {
        PomoNotifier.ServiceChanged += new EventHandler(this.OnPomoChanged);
        PomoNotifier.Changed += new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
        PomoNotifier.LinkChanged += new EventHandler<PomoLinkArgs>(this.OnPomoLinkChanged);
        PomoNotifier.PomoCommit += new EventHandler(this.OnPomoCommit);
        TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
      });
      this.Unloaded += (RoutedEventHandler) ((o, e) =>
      {
        PomoNotifier.ServiceChanged -= new EventHandler(this.OnPomoChanged);
        PomoNotifier.LinkChanged -= new EventHandler<PomoLinkArgs>(this.OnPomoLinkChanged);
        PomoNotifier.Changed -= new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
        PomoNotifier.PomoCommit -= new EventHandler(this.OnPomoCommit);
        TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
      });
    }

    private void InitTopOption()
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Text = Utils.GetString("Close");
      textBlock.FontSize = 14.0;
      textBlock.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock.VerticalAlignment = VerticalAlignment.Top;
      textBlock.Background = (Brush) Brushes.Transparent;
      textBlock.Cursor = Cursors.Hand;
      textBlock.Margin = new Thickness(20.0, 4.0, 0.0, 0.0);
      TextBlock element1 = textBlock;
      element1.SetResourceReference(TextBlock.ForegroundProperty, (object) "PrimaryColor");
      element1.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
      this.Children.Add((UIElement) element1);
      HoverIconButton hoverIconButton = new HoverIconButton();
      hoverIconButton.HorizontalAlignment = HorizontalAlignment.Right;
      hoverIconButton.VerticalAlignment = VerticalAlignment.Top;
      hoverIconButton.Height = 22.0;
      hoverIconButton.Width = 22.0;
      hoverIconButton.Margin = new Thickness(0.0, 4.0, 20.0, 0.0);
      HoverIconButton element2 = hoverIconButton;
      element2.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "MoreDrawingImage");
      element2.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
      this.Children.Add((UIElement) element2);
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      EscPopup escPopup1 = new EscPopup();
      escPopup1.PlacementTarget = sender as UIElement;
      escPopup1.StaysOpen = false;
      escPopup1.Placement = PlacementMode.Left;
      escPopup1.VerticalOffset = 20.0;
      escPopup1.HorizontalOffset = 28.0;
      EscPopup escPopup2 = escPopup1;
      bool flag = this._timer.Status != 0;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "Archive", Utils.GetString(flag ? "Restore" : "Archive"), Utils.GetImageSource(flag ? "CancelArchiveDrawingImage" : "ArchiveDrawingImage")),
        new CustomMenuItemViewModel((object) "Delete", Utils.GetString("Delete"), Utils.GetImageSource("DeleteDrawingLine"))
      };
      if (!flag)
      {
        types.Insert(0, new CustomMenuItemViewModel((object) "Edit", Utils.GetString("Edit"), Utils.GetImageSource("EditDrawingImage")));
        types.Insert(1, new CustomMenuItemViewModel((object) "Add", Utils.GetString("AddRecord"), Utils.GetIcon("IcAdd"))
        {
          ImageMargin = new Thickness(11.0, 0.0, 2.0, 0.0)
        });
      }
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) escPopup2);
      customMenuList.Operated += new EventHandler<object>(this.OnActionSelected);
      customMenuList.Show();
    }

    private async void OnActionSelected(object sender, object e)
    {
      TimerDetailPanel timerDetailPanel = this;
      switch (e.ToString())
      {
        case "Edit":
          TimerModel timerById = await TimerDao.GetTimerById(timerDetailPanel._timer.Id);
          if (timerById == null)
            break;
          AddTimerWindow addTimerWindow = new AddTimerWindow(timerById);
          addTimerWindow.Owner = Window.GetWindow((DependencyObject) timerDetailPanel);
          addTimerWindow.ShowDialog();
          break;
        case "Add":
          TimerModel timer = await TimerDao.GetTimerById(timerDetailPanel._timer.Id);
          if (timer != null)
          {
            string objTitle = await TimerService.GetObjTitle(timer.ObjId, timer.ObjType);
            if (objTitle != null && timer.Name != objTitle)
            {
              timer.Name = objTitle;
              await TimerService.UpdateName(timer.Id, objTitle);
            }
            new AddFocusRecordWindow(timer).ShowDialog();
            UserActCollectUtils.AddClickEvent("timer", "timer_detail", "add_record");
          }
          timer = (TimerModel) null;
          break;
        case "Archive":
          if (timerDetailPanel._timer.Status != 0)
          {
            if (!await ProChecker.CheckTimerLimit(Window.GetWindow((DependencyObject) timerDetailPanel)))
              break;
          }
          else if (!new CustomerDialog("", Utils.GetString("ArchiveTimerMessage"), Utils.GetString("Archive"), Utils.GetString("Cancel"), Window.GetWindow((DependencyObject) timerDetailPanel)).ShowDialog().GetValueOrDefault())
            break;
          TimerService.ChangeArchiveStatus(timerDetailPanel._timer.Id);
          break;
        case "Delete":
          if (!new CustomerDialog("", Utils.GetString("DeleteTimerMessage"), Utils.GetString("Delete"), Utils.GetString("Cancel"), Window.GetWindow((DependencyObject) timerDetailPanel)).ShowDialog().GetValueOrDefault())
            break;
          TimerService.DeleteTimer(timerDetailPanel._timer.Id);
          break;
      }
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e)
    {
      Utils.FindParent<FocusView>((DependencyObject) this)?.ClearTimerSelect();
    }

    public async Task LoadTimer(TimerModel timer)
    {
      await this.TryLoadItems(timer);
      this.PullRemote(timer.Id);
    }

    private async Task TryLoadItems(TimerModel timer)
    {
      await this.Dispatcher.Invoke<Task>((Func<Task>) (() => this.LoadItems(timer)));
    }

    private async Task LoadItems(TimerModel timer)
    {
      TimerDetailPanel timerDetailPanel = this;
      await timerDetailPanel._loadAsync.RunAsync(new Func<TimerModel, Task>(timerDetailPanel.SetItems), timer);
    }

    private async Task SetItems(TimerModel timer)
    {
      TimerDetailPanel detail = this;
      List<FocusStatisticsPanelItemViewModel> itemsSource;
      List<PomodoroModel> pomos;
      Dictionary<DateTime, List<FocusRecordItemViewModel>> datePomoDict;
      if (detail._timer == null && timer == null)
      {
        itemsSource = (List<FocusStatisticsPanelItemViewModel>) null;
        pomos = (List<PomodoroModel>) null;
        datePomoDict = (Dictionary<DateTime, List<FocusRecordItemViewModel>>) null;
      }
      else
      {
        if (timer != null)
        {
          bool first = timer.Id != detail._timer?.Id;
          detail._timer = timer;
          detail._limits = 30;
          detail._timelineViewModel = new TimerTimelineItemViewModel(detail);
          detail._timelineViewModel.SetTimer(timer);
          detail.PullTimeline(detail._timelineViewModel, first: first);
        }
        else
        {
          TimerModel timerById = await TimerDao.GetTimerById(detail._timer.Id);
          detail._timer = timerById;
          detail._timelineViewModel.ReloadData();
        }
        TimerOverviewModel timerOverView = await TimerService.GetTimerOverView(detail._timer);
        itemsSource = new List<FocusStatisticsPanelItemViewModel>()
        {
          (FocusStatisticsPanelItemViewModel) new TimerDetailTitleViewModel(detail._timer),
          (FocusStatisticsPanelItemViewModel) new TimerStatisticsOverviewItem(timerOverView),
          (FocusStatisticsPanelItemViewModel) detail._timelineViewModel
        };
        pomos = await PomoDao.GetPomoDescByStartAndTimerId(detail._limits, detail._timer.Id, detail._timer.ObjId, detail._timer.ObjType == "habit", detail._timelineViewModel.StartDate, detail._timelineViewModel.EndDate.AddDays(1.0));
        datePomoDict = new Dictionary<DateTime, List<FocusRecordItemViewModel>>();
        foreach (PomodoroModel pomo in pomos)
        {
          List<PomoTask> pomoTasksByPomoId = await PomoDao.GetPomoTasksByPomoId(pomo.Id);
          DateTime dateTime = pomo.EndTime;
          DateTime date1 = dateTime.Date;
          FocusRecordItemViewModel recordItemViewModel1 = new FocusRecordItemViewModel(pomo, date1, pomoTasksByPomoId, false);
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
            FocusRecordItemViewModel recordItemViewModel2 = new FocusRecordItemViewModel(pomo, date4, pomoTasksByPomoId, false);
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
        if (pomos.Count == detail._limits)
          itemsSource.Add((FocusStatisticsPanelItemViewModel) new RecordLoadMoreItem.FocusRecordLoadMoreViewModel()
          {
            NeedClick = (detail._limits == 30)
          });
        ItemsSourceHelper.SetItemsSource<FocusStatisticsPanelItemViewModel>((ItemsControl) detail._listView, itemsSource);
        itemsSource = (List<FocusStatisticsPanelItemViewModel>) null;
        pomos = (List<PomodoroModel>) null;
        datePomoDict = (Dictionary<DateTime, List<FocusRecordItemViewModel>>) null;
      }
    }

    private async Task PullRemote(string id)
    {
      TimerDetailPanel timerDetailPanel = this;
      TimerOverviewModel overView = await TimerSyncService.PullTimerOverview(id);
      if (overView == null)
        ;
      else if (!(timerDetailPanel._timer.Id == id))
        ;
      else if (!timerDetailPanel.IsVisible)
        ;
      else
      {
        TimerModel timerById = await TimerDao.GetTimerById(id);
        timerDetailPanel._timer = timerById;
        overView = await TimerService.GetTimerOverView(timerDetailPanel._timer);
        timerDetailPanel.Dispatcher.Invoke((Action) (() => this.ItemsSource[1] = (FocusStatisticsPanelItemViewModel) new TimerStatisticsOverviewItem(overView)));
      }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      if (!(this._listView.Template.FindName("ScrollViewer", (FrameworkElement) this._listView) is ScrollViewer name))
        return;
      name.ScrollChanged += new ScrollChangedEventHandler(this.OnScrollChanged);
    }

    private void OnPomoLinkChanged(object sender, PomoLinkArgs e)
    {
      if (this._timer == null || !(e.NewTimerId == this._timer.Id) && !(e.PreviewTimerId == this._timer.Id))
        return;
      this.TryLoadItems((TimerModel) null);
    }

    private void OnPomoChanged(object sender, PomoChangeArgs e)
    {
      this.TryLoadItems((TimerModel) null);
    }

    private void OnPomoCommit(object sender, EventArgs e)
    {
      if (this._timer != null)
        this.PullRemote(this._timer.Id);
      this.Dispatcher.Invoke((Action) (() => this.PullTimeline(this._timelineViewModel, false)));
    }

    private async void OnDayChanged(object sender, EventArgs e)
    {
      if (this._timer == null)
        return;
      this.TryLoadItems(await TimerDao.GetTimerById(this._timer.Id));
    }

    private void OnPomoChanged(object sender, EventArgs e)
    {
      if (this.ItemsSource.Count <= 3 || !(this.ItemsSource[2] is TimerTimelineItemViewModel timelineItemViewModel))
        return;
      timelineItemViewModel.ReloadData();
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (!(sender is ScrollViewer scrollViewer))
        return;
      ObservableCollection<FocusStatisticsPanelItemViewModel> itemsSource = this.ItemsSource;
      if (!((itemsSource != null ? itemsSource.LastOrDefault<FocusStatisticsPanelItemViewModel>() : (FocusStatisticsPanelItemViewModel) null) is RecordLoadMoreItem.FocusRecordLoadMoreViewModel loadMoreViewModel) || loadMoreViewModel.NeedClick || e.VerticalOffset < scrollViewer.ScrollableHeight - 10.0)
        return;
      this.LoadMore();
    }

    private async void LoadMore()
    {
      if (this._loading)
        return;
      this._loading = true;
      this._limits += 30;
      this.TryLoadItems((TimerModel) null);
      await this.PullRemoteFocus(this._timelineViewModel.StartDate, false, this._timelineViewModel.TModel.Id);
      this._loading = false;
    }

    private async Task PullRemoteFocus(DateTime start, bool firstPull, string id)
    {
      if (!await PomoSyncService.PullTimerTimelines(Utils.GetTimeStamp(new DateTime?(start)), firstPull, id) || !(id == this._timelineViewModel.TModel.Id) || !(start == this._timelineViewModel.StartDate))
        return;
      this.TryLoadItems((TimerModel) null);
    }

    public void PullTimeline(TimerTimelineItemViewModel viewModel, bool getFront = true, bool first = false)
    {
      if (first)
        TimerSyncService.ClearPulledKey();
      string interval = viewModel.Interval;
      DateTime startDate = viewModel.StartDate;
      DateTime endDate = viewModel.EndDate;
      this.PullTimerStatistics(viewModel.TModel.Id, startDate, endDate, false, interval);
      this.PullRemoteFocus(viewModel.StartDate, first, viewModel.TModel.Id);
      if (!getFront)
        return;
      DateTime start = startDate;
      DateTime end = endDate;
      switch (viewModel.Interval)
      {
        case "month":
          start = startDate.AddMonths(-1);
          end = start.AddMonths(1).AddDays(-1.0);
          break;
        case "year":
          start = startDate.AddYears(-1);
          end = start.AddYears(1).AddDays(-1.0);
          break;
        case "week":
          start = startDate.AddDays(-7.0);
          end = start.AddDays(6.0);
          break;
      }
      this.PullTimerStatistics(viewModel.TModel.Id, start, end, true, interval);
    }

    private async Task PullTimerStatistics(
      string id,
      DateTime start,
      DateTime end,
      bool delay,
      string interval)
    {
      if (delay)
        await Task.Delay(1000);
      if (interval != this._timelineViewModel.Interval)
        return;
      bool flag = this._timelineViewModel.TModel.Id == id;
      if (flag)
        flag = await TimerSyncService.TryPullTimerStatistics(id, start, end, interval);
      if (!flag || !(start == this._timelineViewModel.StartDate))
        return;
      this._timelineViewModel.ReloadData();
    }

    public void ReloadRecords()
    {
      this._limits = 30;
      this.TryLoadItems((TimerModel) null);
    }
  }
}
