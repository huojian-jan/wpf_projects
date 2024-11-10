// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusTimerListView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo.MainFocus;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusTimerListView : SortableListView<FocusTimerViewModel>
  {
    private string _selectedId;
    private StackPanel _emptyPanel;
    private Path _emptyPath;
    private Image _emptyImage;
    private TextBlock _emptyText1;
    private TextBlock _emptyText2;
    private bool _archive;

    public FocusTimerListView()
    {
      this.Loaded += new RoutedEventHandler(this.Init);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    private void Init(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.Init);
      ContentControl contentControl = new ContentControl();
      contentControl.SetResourceReference(FrameworkElement.StyleProperty, (object) "PopupContentStyle");
      contentControl.Margin = new Thickness(0.0);
      contentControl.Content = (object) new TimerListItem()
      {
        InPopup = true
      };
      this.PopupContent = (FrameworkElement) contentControl;
      this.PopupContentHeight = 54.0;
      this.ItemList.SetResourceReference(ItemsControl.ItemTemplateProperty, (object) "TimerItemTemplate");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
    }

    private void OnDayChanged(object sender, EventArgs e)
    {
      if (this._archive || !(this.ItemList.ItemsSource is ObservableCollection<FocusTimerViewModel> itemsSource))
        return;
      ItemsSourceHelper.SetItemsSource<FocusTimerViewModel>((ItemsControl) this.ItemList, itemsSource.ToList<FocusTimerViewModel>());
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
    }

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      if (!e.TaskTextChangedIds.Any() || !(this.ItemList.ItemsSource is ObservableCollection<FocusTimerViewModel> itemsSource))
        return;
      foreach (FocusTimerViewModel focusTimerViewModel in (Collection<FocusTimerViewModel>) itemsSource)
      {
        if (e.TaskTextChangedIds.Contains(focusTimerViewModel.ObjId))
          focusTimerViewModel.CheckTimerName();
      }
    }

    public void SetItems(List<TimerModel> items, bool archive, string selectedId)
    {
      this._archive = archive;
      if (items.Count == 0)
      {
        ItemsSourceHelper.SetItemsSource<FocusTimerViewModel>((ItemsControl) this.ItemList, new List<FocusTimerViewModel>());
        this.ShowEmptyImage();
      }
      else
      {
        if (this._emptyPanel != null)
          this._emptyPanel.Visibility = Visibility.Collapsed;
        List<FocusTimerViewModel> list = items.Select<TimerModel, FocusTimerViewModel>((Func<TimerModel, FocusTimerViewModel>) (m => new FocusTimerViewModel(m)
        {
          Selected = m.Id == selectedId
        })).ToList<FocusTimerViewModel>();
        if (!archive)
          TimerService.SetTimerTodayDuration(list);
        ItemsSourceHelper.SetItemsSource<FocusTimerViewModel>((ItemsControl) this.ItemList, list);
      }
    }

    private void ShowEmptyImage()
    {
      if (this._emptyPanel == null)
      {
        StackPanel stackPanel = new StackPanel();
        stackPanel.Margin = new Thickness(0.0, -30.0, 0.0, 0.0);
        stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
        stackPanel.VerticalAlignment = VerticalAlignment.Center;
        this._emptyPanel = stackPanel;
        Grid grid = new Grid();
        grid.Width = 200.0;
        grid.Height = 200.0;
        Grid element1 = grid;
        Ellipse ellipse = new Ellipse();
        ellipse.Width = 145.0;
        ellipse.Height = 145.0;
        Ellipse element2 = ellipse;
        element2.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity5");
        element1.Children.Add((UIElement) element2);
        Path path = new Path();
        path.Width = 200.0;
        path.Height = 200.0;
        path.Margin = new Thickness(7.0, 6.0, 7.0, 6.0);
        this._emptyPath = path;
        this._emptyPath.SetResourceReference(Shape.FillProperty, (object) "EmptyPathColor");
        element1.Children.Add((UIElement) this._emptyPath);
        Image image = new Image();
        image.Width = 200.0;
        image.Height = 200.0;
        image.Stretch = Stretch.None;
        this._emptyImage = image;
        element1.Children.Add((UIElement) this._emptyImage);
        this._emptyPanel.Children.Add((UIElement) element1);
        TextBlock textBlock1 = new TextBlock();
        textBlock1.TextAlignment = TextAlignment.Center;
        textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
        textBlock1.Margin = new Thickness(0.0, 0.0, 0.0, 10.0);
        this._emptyText1 = textBlock1;
        this._emptyText1.SetResourceReference(FrameworkElement.StyleProperty, (object) "Title05");
        this._emptyPanel.Children.Add((UIElement) this._emptyText1);
        TextBlock textBlock2 = new TextBlock();
        textBlock2.TextAlignment = TextAlignment.Center;
        textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
        this._emptyText2 = textBlock2;
        this._emptyText2.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body02");
        this._emptyPanel.Children.Add((UIElement) this._emptyText2);
        this.Children.Add((UIElement) this._emptyPanel);
      }
      else
        this._emptyPanel.Visibility = Visibility.Visible;
      this._emptyPath.Margin = this._archive ? new Thickness(120.0, 57.0, 0.0, 0.0) : new Thickness(7.0, 6.0, 7.0, 6.0);
      this._emptyPath.Data = Utils.GetIconData(this._archive ? "IcArchiveHabitEmpty" : "IcHabitListEmpty");
      this._emptyImage.SetResourceReference(Image.SourceProperty, this._archive ? (object) "HabitArchiveEmptyDrawingImage" : (object) "HabitListEmptyDrawingImage");
      this._emptyImage.Margin = this._archive ? new Thickness(-6.0, 1.0, 0.0, 0.0) : new Thickness(0.0, 0.0, 0.0, 0.0);
      this._emptyText1.Text = Utils.GetString(this._archive ? "NoArchiveTimer" : "NoTimer");
      this._emptyText2.Text = Utils.GetString(this._archive ? "NoArchiveTimerMessage" : "NoTimerMessage");
    }

    public void SetItemFocusing()
    {
      if (!(this.ItemList.ItemsSource is ObservableCollection<FocusTimerViewModel> itemsSource))
        return;
      foreach (FocusTimerViewModel focusTimerViewModel in (Collection<FocusTimerViewModel>) itemsSource)
        focusTimerViewModel.SetFocusing();
    }

    public void OnItemClick(FocusTimerViewModel model)
    {
      this.ClearSelect();
      model.Selected = true;
      Utils.FindParent<MainFocusControl>((DependencyObject) this)?.OnTimerSelect(model.Id);
    }

    public void ClearSelect()
    {
      if (!(this.ItemList.ItemsSource is ObservableCollection<FocusTimerViewModel> itemsSource))
        return;
      foreach (FocusTimerViewModel focusTimerViewModel in (Collection<FocusTimerViewModel>) itemsSource)
        focusTimerViewModel.Selected = false;
    }
  }
}
