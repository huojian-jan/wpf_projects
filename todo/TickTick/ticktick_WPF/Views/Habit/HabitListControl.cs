// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitListControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Network;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitListControl : UserControl, IComponentConnector, IStyleConnector
  {
    public static readonly DependencyProperty IsArchiveProperty = DependencyProperty.Register(nameof (IsArchive), typeof (bool), typeof (HabitListControl), new PropertyMetadata((object) false));
    private double _startY = -1.0;
    private HabitItemBaseViewModel _dragItem;
    private int _currentDragIndex;
    private bool _isUpdating;
    private bool _currentDragIsOpen;
    internal HabitListControl Root;
    internal EscPopup MorePopup;
    internal Grid ItemsGrid;
    internal ListView HabitItems;
    internal Popup DragPopup;
    internal HabitListItem PopupItem;
    internal HabitListSectionItem PopupSection;
    internal Grid EmptyGrid;
    internal Popup RecordPopup;
    internal StackPanel RecordHintGrid;
    internal TextBlock CheckInAmountText;
    private bool _contentLoaded;

    public HabitListControl()
    {
      this.InitializeComponent();
      this.HabitItems.ItemsSource = (IEnumerable) new ObservableCollection<HabitItemBaseViewModel>();
      this.PopupItem.BottomLine.Visibility = Visibility.Collapsed;
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.HabitsChanged -= new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.HabitSectionChanged -= new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.HabitCheckInChanged -= new EventHandler<HabitCheckInModel>(this.OnHabitCheckInChanged);
      DataChangedNotifier.HabitCyclesChanged -= new EventHandler<string>(this.OnHabitCyclesChanged);
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
    }

    private void BindEvents()
    {
      DataChangedNotifier.HabitsChanged += new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.HabitSectionChanged += new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.HabitCheckInChanged += new EventHandler<HabitCheckInModel>(this.OnHabitCheckInChanged);
      DataChangedNotifier.HabitCyclesChanged += new EventHandler<string>(this.OnHabitCyclesChanged);
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
    }

    private void OnDayChanged(object sender, EventArgs e) => this.GetHabitModels();

    public event EventHandler<string> HabitSelected;

    public event EventHandler<int> ItemsCountChanged;

    public bool IsArchive
    {
      get => (bool) this.GetValue(HabitListControl.IsArchiveProperty);
      set => this.SetValue(HabitListControl.IsArchiveProperty, (object) value);
    }

    public ObservableCollection<HabitItemBaseViewModel> Items
    {
      get
      {
        return this.HabitItems.ItemsSource is ObservableCollection<HabitItemBaseViewModel> itemsSource ? itemsSource : new ObservableCollection<HabitItemBaseViewModel>();
      }
    }

    public bool IsUpdating => this._isUpdating;

    public async Task GetHabitModels()
    {
      HabitListControl habitListControl = this;
      while (habitListControl._isUpdating)
        await Task.Delay(50);
      habitListControl._isUpdating = true;
      try
      {
        List<HabitSectionModel> source;
        if (habitListControl.IsArchive)
          source = new List<HabitSectionModel>();
        else
          source = await HabitService.GetHabitSections();
        List<HabitSectionModel> sortedSections = source.OrderBy<HabitSectionModel, long>((Func<HabitSectionModel, long>) (s => s.SortOrder)).ToList<HabitSectionModel>();
        List<HabitModel> habitsbyStatus = await HabitService.GetHabitsbyStatus(habitListControl.IsArchive ? 1 : 0);
        List<HabitItemBaseViewModel> itemBaseViewModelList = new List<HabitItemBaseViewModel>();
        string selectedHabitId = Utils.FindParent<HabitContainer>((DependencyObject) habitListControl).GetSelectedHabitId();
        foreach (HabitSectionModel habitSectionModel in sortedSections)
        {
          HabitSectionModel section = habitSectionModel;
          List<HabitModel> list = habitsbyStatus.Where<HabitModel>((Func<HabitModel, bool>) (s => s.SectionId == section.Id)).OrderBy<HabitModel, long>((Func<HabitModel, long>) (s => s.SortOrder)).ToList<HabitModel>();
          if (list.Count > 0)
          {
            HabitSectionListViewModel sectionListViewModel1 = new HabitSectionListViewModel(section);
            sectionListViewModel1.Num = 0;
            sectionListViewModel1.Archived = habitListControl.IsArchive;
            HabitSectionListViewModel sectionListViewModel2 = sectionListViewModel1;
            itemBaseViewModelList.Add((HabitItemBaseViewModel) sectionListViewModel2);
            foreach (HabitModel habitModel in list)
            {
              HabitModel s = habitModel;
              ++sectionListViewModel2.Num;
              HabitItemViewModel habitItemViewModel1 = new HabitItemViewModel(s);
              habitItemViewModel1.Selected = s.Id == selectedHabitId;
              HabitItemViewModel habitItemViewModel2 = habitItemViewModel1;
              ObservableCollection<HabitItemBaseViewModel> items = habitListControl.Items;
              if ((items != null ? items.FirstOrDefault<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (it => it.Id == s.Id && !it.IsHide)) : (HabitItemBaseViewModel) null) is HabitItemViewModel habitItemViewModel3)
                habitItemViewModel2.WeekCheckIns = habitItemViewModel3.WeekCheckIns;
              sectionListViewModel2.Children.Add(habitItemViewModel2);
              if (section.IsOpen)
                itemBaseViewModelList.Add((HabitItemBaseViewModel) habitItemViewModel2);
            }
            habitsbyStatus.RemoveAll((Predicate<HabitModel>) (s => s.SectionId == section.Id));
          }
        }
        HabitSectionListViewModel sectionListViewModel3 = new HabitSectionListViewModel(HabitSectionModel.GetDefault());
        sectionListViewModel3.Num = habitsbyStatus.Count;
        sectionListViewModel3.Archived = habitListControl.IsArchive;
        sectionListViewModel3.IsOther = true;
        HabitSectionListViewModel defaultViewModel = sectionListViewModel3;
        List<HabitSectionListViewModel> list1 = itemBaseViewModelList.OfType<HabitSectionListViewModel>().ToList<HabitSectionListViewModel>();
        HabitSectionListViewModel sectionListViewModel4 = list1.LastOrDefault<HabitSectionListViewModel>((Func<HabitSectionListViewModel, bool>) (sectionView => sectionView.SortOrder < defaultViewModel.SortOrder));
        int num = 0;
        if (sectionListViewModel4 != null && sectionListViewModel4.IsOpen)
          num = sectionListViewModel4.Num;
        int val1 = itemBaseViewModelList.IndexOf((HabitItemBaseViewModel) sectionListViewModel4) + 1 + num;
        List<HabitItemBaseViewModel> collection = new List<HabitItemBaseViewModel>();
        int index;
        if (val1 >= 0 && list1.Any<HabitSectionListViewModel>() && habitsbyStatus.Any<HabitModel>())
        {
          index = Math.Max(val1, 0);
          collection.Add((HabitItemBaseViewModel) defaultViewModel);
        }
        else
        {
          index = 0;
          HabitSectionModel.GetDefault().IsOpen = true;
        }
        bool isOpen = HabitSectionModel.GetDefault().IsOpen;
        foreach (HabitModel habitModel in (IEnumerable<HabitModel>) habitsbyStatus.OrderBy<HabitModel, long>((Func<HabitModel, long>) (h => h.SortOrder)))
        {
          HabitModel s = habitModel;
          s.SectionId = HabitSectionModel.GetDefault().Id;
          HabitItemViewModel habitItemViewModel4 = new HabitItemViewModel(s);
          habitItemViewModel4.Selected = s.Id == selectedHabitId;
          HabitItemViewModel habitItemViewModel5 = habitItemViewModel4;
          ObservableCollection<HabitItemBaseViewModel> items = habitListControl.Items;
          if ((items != null ? items.FirstOrDefault<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (it => it.Id == s.Id && !it.IsHide)) : (HabitItemBaseViewModel) null) is HabitItemViewModel habitItemViewModel6)
            habitItemViewModel5.WeekCheckIns = habitItemViewModel6?.WeekCheckIns;
          if (isOpen)
            collection.Add((HabitItemBaseViewModel) habitItemViewModel5);
          defaultViewModel.Children.Add(habitItemViewModel5);
        }
        itemBaseViewModelList.InsertRange(index, (IEnumerable<HabitItemBaseViewModel>) collection);
        habitListControl.SetItems(itemBaseViewModelList);
        habitListControl.EmptyGrid.Visibility = itemBaseViewModelList.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        EventHandler<int> itemsCountChanged = habitListControl.ItemsCountChanged;
        if (itemsCountChanged != null)
          itemsCountChanged((object) habitListControl, itemBaseViewModelList.Count);
        await habitListControl.GetCheckIns(itemBaseViewModelList.Where<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (s => s is HabitItemViewModel)).Select<HabitItemBaseViewModel, string>((Func<HabitItemBaseViewModel, string>) (h => h.Id)).ToList<string>());
        sortedSections = (List<HabitSectionModel>) null;
      }
      catch (Exception ex)
      {
      }
      finally
      {
        habitListControl._isUpdating = false;
      }
    }

    private async Task GetCheckIns(List<string> ids)
    {
      if (this.IsArchive)
        return;
      ObservableCollection<HabitItemBaseViewModel> items = this.Items;
      if ((items != null ? (items.Count<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (h => !h.IsHide)) > 0 ? 1 : 0) : 0) == 0)
        return;
      await Task.Delay(10);
      List<HabitCheckInModel> habitCheckInModelList;
      if (this.IsArchive)
        habitCheckInModelList = (List<HabitCheckInModel>) null;
      else
        habitCheckInModelList = await HabitCheckInDao.GetCheckInsByIdsInSpan(ids, DateTime.Today.AddDays(-6.0), DateTime.Today);
      List<HabitCheckInModel> checkIns = habitCheckInModelList;
      foreach (HabitItemBaseViewModel itemBaseViewModel in (Collection<HabitItemBaseViewModel>) this.Items)
      {
        if (!itemBaseViewModel.IsHide && itemBaseViewModel is HabitItemViewModel habitItemViewModel && ids.Contains(habitItemViewModel.Id))
          habitItemViewModel.SetCheckIns((IEnumerable<HabitCheckInModel>) checkIns);
      }
    }

    public async void CheckInHabit(string habitId)
    {
      while (this._isUpdating)
        await Task.Delay(10);
      this._isUpdating = true;
      try
      {
        ObservableCollection<HabitItemBaseViewModel> items = this.Items;
        HabitItemBaseViewModel itemBaseViewModel = items != null ? items.FirstOrDefault<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (item => item.Id == habitId)) : (HabitItemBaseViewModel) null;
        if (!(itemBaseViewModel is HabitItemViewModel habitItemViewModel1))
          ;
        else
        {
          if (itemBaseViewModel.IsHide)
          {
            for (int index = 0; index < this.Items.Count; ++index)
            {
              switch (this.Items[index])
              {
                case HabitItemViewModel habitItemViewModel:
                  if (habitItemViewModel.Habit?.SectionId == habitItemViewModel1.Habit?.SectionId)
                  {
                    habitItemViewModel.IsHide = false;
                    break;
                  }
                  break;
                case HabitSectionListViewModel sectionListViewModel:
                  if (sectionListViewModel.Id == habitItemViewModel1.Habit?.SectionId)
                  {
                    sectionListViewModel.IsOpen = true;
                    break;
                  }
                  break;
              }
            }
          }
          HabitDayCheckModel dayCheck = habitItemViewModel1.WeekCheckIns.FirstOrDefault<HabitDayCheckModel>((Func<HabitDayCheckModel, bool>) (item => item.Date == DateTime.Today));
          HabitListItem selfControl = habitItemViewModel1.SelfControl;
          if (selfControl == null)
            ;
          else
            selfControl.ManuallyRecordCheckIn(dayCheck);
        }
      }
      catch (Exception ex)
      {
      }
      finally
      {
        this._isUpdating = false;
      }
    }

    public void StartDragSection(HabitSectionListViewModel model)
    {
      model.Dragging = true;
      if (!model.IsOpen)
        return;
      this.SetItems(this.Items.Where<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (h =>
      {
        if (h.IsHide)
          return false;
        return !(h is HabitItemViewModel habitItemViewModel2) || !(habitItemViewModel2.Habit.SectionId == model.Id);
      })).ToList<HabitItemBaseViewModel>());
      this._currentDragIsOpen = model.IsOpen;
      model.IsOpen = false;
    }

    private void SetItems(List<HabitItemBaseViewModel> models)
    {
      ItemsSourceHelper.SetHabitListItemsSource(this.HabitItems, models, (int) this.HabitItems.ActualHeight / 60);
    }

    private void OnHabitsChanged(object sender, EventArgs e)
    {
      if (this.Visibility != Visibility.Visible)
        return;
      this.GetHabitModels();
    }

    private async void OnAddHabitClick(object sender, MouseButtonEventArgs e)
    {
      HabitListControl habitListControl = this;
      if (!await HabitUtils.CheckHabitLimit())
        return;
      AddOrEditHabitDialog orEditHabitDialog = new AddOrEditHabitDialog();
      orEditHabitDialog.Owner = Window.GetWindow((DependencyObject) habitListControl);
      orEditHabitDialog.ShowDialog();
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "setting", Utils.GetString("HabitSetting"), Utils.GetIcon("IcSettings")),
        new CustomMenuItemViewModel((object) "export", Utils.GetString("Export"), Utils.GetIcon("IcExport"))
      }, (Popup) this.MorePopup);
      customMenuList.Operated += new EventHandler<object>(this.OnMoreItemSelected);
      customMenuList.Show();
    }

    public void Toast(string text)
    {
      Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.TryToastString((object) null, text);
    }

    private async void OnMoreItemSelected(object sender, object e)
    {
      HabitListControl habitListControl = this;
      switch (e as string)
      {
        case "setting":
          HabitSettingWindow habitSettingWindow = new HabitSettingWindow();
          habitSettingWindow.Owner = Window.GetWindow((DependencyObject) habitListControl);
          habitSettingWindow.ShowDialog();
          break;
        case "export":
          try
          {
            UserActCollectUtils.AddClickEvent("habit", "export", "habit_export");
            string epT = LocalSettings.Settings.ExtraSettings.EpT;
            List<string> stringList;
            if (epT == null)
              stringList = (List<string>) null;
            else
              stringList = ((IEnumerable<string>) epT.Split(',')).ToList<string>();
            if (stringList == null)
              stringList = new List<string>();
            List<string> exportTimes = stringList;
            string today = DateUtils.GetDateNum(DateTime.Now.ToUniversalTime()).ToString();
            if (exportTimes.Count<string>((Func<string, bool>) (t => t == today)) >= 3)
            {
              habitListControl.Toast(Utils.GetString("ExportTooManyTimes"));
              break;
            }
            await Task.Delay(250);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "ExportHabit";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = Utils.GetString("Habits") + "_" + DateUtils.GetDateNum(DateTime.Today).ToString();
            saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            bool? nullable = saveFileDialog.ShowDialog();
            bool flag = true;
            if (nullable.GetValueOrDefault() == flag & nullable.HasValue)
            {
              string file = await ApiClient.GetFile(BaseUrl.GetApiDomain() + "/api/v2/data/export/habits", saveFileDialog.FileName);
              if (!string.IsNullOrEmpty(file))
              {
                if (file.Contains("export_too_many_times"))
                {
                  habitListControl.Toast(Utils.GetString("ExportTooManyTimes"));
                  LocalSettings.Settings.ExtraSettings.EpT = today + "," + today + "," + today;
                  LocalSettings.Settings.Save(true);
                }
                else
                  habitListControl.Toast(Utils.GetString("ExportFailed"));
              }
              else
              {
                if (exportTimes.Count >= 3)
                  exportTimes.RemoveAt(0);
                exportTimes.Add(today);
                LocalSettings.Settings.ExtraSettings.EpT = exportTimes.Join<string>(",");
                LocalSettings.Settings.Save(true);
                habitListControl.Toast(Utils.GetString("ExportSuccess"));
              }
            }
            exportTimes = (List<string>) null;
            break;
          }
          catch (Exception ex)
          {
            habitListControl.Toast(Utils.GetString("ExportFailed"));
            break;
          }
      }
    }

    private async void SwitchKeepingHabit()
    {
      HabitListControl sender = this;
      if (!sender.IsArchive)
        return;
      sender.EmptyGrid.Visibility = Visibility.Collapsed;
      sender.IsArchive = false;
      EventHandler<string> habitSelected = sender.HabitSelected;
      if (habitSelected != null)
        habitSelected((object) sender, (string) null);
      await sender.GetHabitModels();
    }

    private async void SwitchArchiveHabit()
    {
      HabitListControl sender = this;
      if (sender.IsArchive)
        return;
      sender.EmptyGrid.Visibility = Visibility.Collapsed;
      sender.IsArchive = true;
      EventHandler<string> habitSelected = sender.HabitSelected;
      if (habitSelected != null)
        habitSelected((object) sender, (string) null);
      await sender.GetHabitModels();
    }

    public async void ShowAddStepPop(UIElement sender, string toastText)
    {
      HabitListControl habitListControl = this;
      if (sender == null)
        return;
      Storyboard story = (Storyboard) habitListControl.FindResource((object) "ShowRecordPopupAnim");
      story.SkipToFill();
      await Task.Delay(10);
      habitListControl.RecordPopup.PlacementTarget = sender;
      habitListControl.CheckInAmountText.Text = toastText;
      await Task.Delay(10);
      habitListControl.RecordPopup.IsOpen = true;
      story.Begin();
      story = (Storyboard) null;
    }

    private void OnRecordStoryCompleted(object sender, EventArgs e)
    {
      this.RecordPopup.IsOpen = false;
    }

    public void OnItemSelected(string habitId)
    {
      foreach (HabitItemBaseViewModel itemBaseViewModel in (Collection<HabitItemBaseViewModel>) this.Items)
        itemBaseViewModel.Selected = itemBaseViewModel.Id == habitId;
      EventHandler<string> habitSelected = this.HabitSelected;
      if (habitSelected == null)
        return;
      habitSelected((object) this, habitId);
    }

    public void TryDragItem(object sender, MouseEventArgs e)
    {
      if (this.IsArchive)
        return;
      this._startY = e.GetPosition((IInputElement) this.ItemsGrid).Y;
      this._dragItem = (HabitItemBaseViewModel) null;
      this._currentDragIndex = -1;
      if (sender is HabitListSectionItem habitListSectionItem)
      {
        if (!(habitListSectionItem.DataContext is HabitSectionListViewModel dataContext))
          return;
        this._dragItem = (HabitItemBaseViewModel) dataContext;
        this._currentDragIndex = this.Items.IndexOf((HabitItemBaseViewModel) dataContext);
      }
      else
      {
        if (!(this.GetMousePositionItem(e.GetPosition((IInputElement) this.HabitItems))?.DataContext is HabitItemViewModel dataContext) || dataContext.IsHide)
          return;
        this._dragItem = (HabitItemBaseViewModel) dataContext;
        this._currentDragIndex = this.Items.IndexOf((HabitItemBaseViewModel) dataContext);
      }
    }

    private ListViewItem GetMousePositionItem(System.Windows.Point pos)
    {
      HitTestResult hitTestResult = VisualTreeHelper.HitTest((Visual) this.ItemsGrid, pos);
      return hitTestResult != null ? Utils.FindParent<ListViewItem>(hitTestResult.VisualHit) : (ListViewItem) null;
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
      if (this._dragItem == null)
        return;
      System.Windows.Point position1 = e.GetPosition((IInputElement) this.ItemsGrid);
      if (this.DragPopup.IsOpen)
      {
        if (this._dragItem is HabitSectionListViewModel)
        {
          this.DragPopup.HorizontalOffset = position1.X - 22.0;
          this.DragPopup.VerticalOffset = position1.Y - 22.0;
        }
        else
        {
          this.DragPopup.HorizontalOffset = 10.0;
          this.DragPopup.VerticalOffset = position1.Y - 22.0;
        }
        ListViewItem mousePositionItem = this.GetMousePositionItem(new System.Windows.Point(10.0, e.GetPosition((IInputElement) this.ItemsGrid).Y));
        if (!(mousePositionItem?.DataContext is HabitItemBaseViewModel dataContext))
          return;
        int num = this.Items.IndexOf(dataContext);
        if (this._currentDragIndex < 0)
          return;
        System.Windows.Point position2 = e.GetPosition((IInputElement) mousePositionItem);
        if (num == this._currentDragIndex)
          return;
        int val2 = -1;
        if (this._currentDragIndex < num && position2.Y > mousePositionItem.ActualHeight / 2.0)
        {
          val2 = num;
          HabitItemBaseViewModel itemBaseViewModel = val2 + 1 < this.Items.Count ? this.Items[val2 + 1] : (HabitItemBaseViewModel) null;
          if (this._dragItem.IsSection && itemBaseViewModel != null && !itemBaseViewModel.IsHide && !itemBaseViewModel.IsSection)
            return;
        }
        else if (this._currentDragIndex > num && position2.Y < mousePositionItem.ActualHeight / 2.0)
        {
          val2 = num;
          if (this._dragItem.IsSection && !dataContext.IsSection || !this._dragItem.IsSection && val2 == 0 && dataContext.IsSection)
            return;
        }
        if (val2 == this._currentDragIndex || val2 < 0 || val2 >= this.Items.Count)
          return;
        List<HabitItemBaseViewModel> list = this.Items.Where<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (h => !h.IsHide)).ToList<HabitItemBaseViewModel>();
        list.Remove(this._dragItem);
        list.Insert(Math.Min(list.Count, val2), this._dragItem);
        try
        {
          this.SetItems(list);
        }
        catch (Exception ex)
        {
        }
        this._currentDragIndex = val2;
      }
      else
      {
        if (this._startY <= 0.0 || this._dragItem == null)
          return;
        if (this._dragItem is HabitItemViewModel dragItem1 && Math.Abs(position1.Y - this._startY) > 8.0)
        {
          dragItem1.Dragging = true;
          this.ShowDragPop(dragItem1, position1);
        }
        if (!(this._dragItem is HabitSectionListViewModel dragItem2) || Math.Abs(position1.Y - this._startY) <= 8.0)
          return;
        this.StartDragSection(dragItem2);
        this.ShowDragPop(dragItem2, position1);
      }
    }

    private void ShowDragPop(HabitSectionListViewModel model, System.Windows.Point position)
    {
      this.PopupSection.IsEnabled = false;
      this.PopupSection.DataContext = (object) new HabitSectionListViewModel(model.Section)
      {
        Num = model.Num
      };
      this.PopupItem.Visibility = Visibility.Collapsed;
      this.PopupSection.Visibility = Visibility.Visible;
      this.DragPopup.HorizontalOffset = position.X - 1.0 * (this.ItemsGrid.ActualWidth > 400.0 ? 200.0 : this.ItemsGrid.ActualWidth / 2.0);
      this.DragPopup.VerticalOffset = position.Y - 22.0;
      this.DragPopup.IsOpen = true;
    }

    private void ShowDragPop(HabitItemViewModel model, System.Windows.Point position)
    {
      this.PopupItem.IsEnabled = false;
      this.PopupItem.DataContext = (object) new HabitItemViewModel(model.Habit)
      {
        ShowCheckIns = false
      };
      this.PopupItem.Visibility = Visibility.Visible;
      this.PopupSection.Visibility = Visibility.Collapsed;
      this.DragPopup.HorizontalOffset = position.X - 1.0 * (this.ItemsGrid.ActualWidth > 400.0 ? 200.0 : this.ItemsGrid.ActualWidth / 2.0);
      this.DragPopup.VerticalOffset = position.Y - 22.0;
      this.DragPopup.IsOpen = true;
    }

    private void OnDrop(object sender, MouseButtonEventArgs e)
    {
      if (!this.DragPopup.IsOpen && this._dragItem != null)
      {
        this._dragItem.Dragging = false;
        this._dragItem = (HabitItemBaseViewModel) null;
      }
      this.ItemsGrid.ReleaseMouseCapture();
      if (this._dragItem is HabitItemViewModel dragItem2)
      {
        long sortOrder1 = dragItem2.SortOrder;
        HabitItemBaseViewModel itemBaseViewModel1 = this._currentDragIndex > 0 ? this.Items[this._currentDragIndex - 1] : (HabitItemBaseViewModel) null;
        HabitItemBaseViewModel itemBaseViewModel2 = this._currentDragIndex < this.Items.Count<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (i => !i.IsHide)) - 1 ? this.Items[this._currentDragIndex + 1] : (HabitItemBaseViewModel) null;
        HabitItemViewModel habitItemViewModel1 = itemBaseViewModel1 as HabitItemViewModel;
        HabitSectionListViewModel sectionListViewModel = itemBaseViewModel1 as HabitSectionListViewModel;
        long? sortOrder2 = habitItemViewModel1?.SortOrder;
        long? nullable = itemBaseViewModel2 is HabitItemViewModel habitItemViewModel2 ? new long?(habitItemViewModel2.SortOrder) : new long?();
        if (sortOrder2.HasValue || nullable.HasValue)
          sortOrder1 = sortOrder2.HasValue ? (nullable.HasValue ? sortOrder2.Value + (nullable.Value - sortOrder2.Value) / 2L : sortOrder2.Value + 268435456L) : nullable.Value - 268435456L;
        string sectionId = dragItem2.Habit.SectionId;
        if (habitItemViewModel1 != null)
          sectionId = habitItemViewModel1.Habit.SectionId;
        else if (sectionListViewModel != null)
          sectionId = sectionListViewModel.Id;
        if (sortOrder1 != dragItem2.SortOrder || sectionId != dragItem2.Habit.SectionId)
        {
          dragItem2.Habit.SectionId = sectionId;
          dragItem2.Habit.SortOrder = sortOrder1;
          HabitDao.SaveSortOrderAndSection(dragItem2.Habit.Id, sortOrder1, sectionId);
        }
      }
      else if (this._dragItem is HabitSectionListViewModel dragItem1)
      {
        List<HabitSectionListViewModel> list = this.Items.OfType<HabitSectionListViewModel>().Where<HabitSectionListViewModel>((Func<HabitSectionListViewModel, bool>) (v => !v.IsHide)).ToList<HabitSectionListViewModel>();
        int num = list.IndexOf(dragItem1);
        long order = HabitListControl.GetSortOrderBetween(num > 0 ? new long?(list[num - 1].SortOrder) : new long?(), num < list.Count - 1 ? new long?(list[num + 1].SortOrder) : new long?()) ?? dragItem1.SortOrder;
        dragItem1.IsOpen = this._currentDragIsOpen;
        if (order != dragItem1.SortOrder)
        {
          dragItem1.Section.SortOrder = order;
          dragItem1.SortOrder = order;
          HabitService.UpdateSectionSortOrderById(dragItem1.Id, order);
        }
        else
          this.GetHabitModels();
      }
      if (this._dragItem != null)
      {
        this._dragItem.Dragging = false;
        this._dragItem = (HabitItemBaseViewModel) null;
      }
      this._startY = -1.0;
      this._currentDragIndex = -1;
      this.DragPopup.IsOpen = false;
    }

    private static long? GetSortOrderBetween(long? front, long? next)
    {
      if (!front.HasValue && !next.HasValue)
        return new long?();
      if (!front.HasValue)
        return new long?(next.Value - 268435456L);
      return !next.HasValue ? new long?(front.Value + 268435456L) : new long?(front.Value + (next.Value - front.Value) / 2L);
    }

    private async void OnHabitCheckInChanged(object sender, HabitCheckInModel checkIn)
    {
      HabitListControl habitListControl = this;
      if (habitListControl.Visibility != Visibility.Visible || sender is HabitListItem child && Utils.FindParent<HabitListControl>((DependencyObject) child) == habitListControl)
        return;
      try
      {
        DateTime exact = DateTime.ParseExact(checkIn.CheckinStamp, "yyyyMMdd", (IFormatProvider) App.Ci);
        ObservableCollection<HabitItemBaseViewModel> items = habitListControl.Items;
        if ((items != null ? items.FirstOrDefault<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (habit => !habit.IsHide && habit.Id == checkIn.HabitId)) : (HabitItemBaseViewModel) null) is HabitItemViewModel item)
        {
          if ((DateTime.Today - exact).TotalDays <= 7.0)
            await item.UpdateWeekCheckIns();
          await item.UpdateTotalCheckDaysAndCurrentStreak();
        }
        item = (HabitItemViewModel) null;
      }
      catch (Exception ex)
      {
      }
    }

    public void RemoveItem(string habitId)
    {
      ObservableCollection<HabitItemBaseViewModel> items = this.Items;
      HabitItemBaseViewModel itemBaseViewModel = items != null ? items.FirstOrDefault<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (it => !it.IsHide && it.Id == habitId)) : (HabitItemBaseViewModel) null;
      if (itemBaseViewModel == null)
        return;
      this.Items?.Remove(itemBaseViewModel);
      EventHandler<string> habitSelected = this.HabitSelected;
      if (habitSelected == null)
        return;
      habitSelected((object) this, (string) null);
    }

    private async void OnHabitCyclesChanged(object sender, string e)
    {
      ObservableCollection<HabitItemBaseViewModel> items = this.Items;
      if (!((items != null ? items.FirstOrDefault<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (h => !h.IsHide && h.Id == e)) : (HabitItemBaseViewModel) null) is HabitItemViewModel model))
      {
        model = (HabitItemViewModel) null;
      }
      else
      {
        HabitModel habitById = await HabitDao.GetHabitById(model.Habit.Id);
        if (habitById == null)
        {
          model = (HabitItemViewModel) null;
        }
        else
        {
          model.SetCyclesList(habitById.CompletedCyclesList);
          model = (HabitItemViewModel) null;
        }
      }
    }

    private void OnGroupTitleSelectedTitleChanged(object sender, GroupTitleViewModel e)
    {
      if ("Archived".Equals(e.Title))
        this.SwitchArchiveHabit();
      else
        this.SwitchKeepingHabit();
    }

    public void ClearData() => this.HabitItems.ItemsSource = (IEnumerable) null;

    public void OnSectionOpenChanged(HabitSectionListViewModel section)
    {
      List<HabitItemBaseViewModel> list = this.Items.Where<HabitItemBaseViewModel>((Func<HabitItemBaseViewModel, bool>) (h => !h.IsHide)).ToList<HabitItemBaseViewModel>();
      if (!section.IsOpen)
      {
        list.RemoveAll((Predicate<HabitItemBaseViewModel>) (model => model is HabitItemViewModel habitItemViewModel && habitItemViewModel.Habit.SectionId == section.Id));
      }
      else
      {
        int num = list.IndexOf((HabitItemBaseViewModel) section);
        if (num >= 0)
        {
          list.InsertRange(num + 1, (IEnumerable<HabitItemBaseViewModel>) section.Children);
          this.GetCheckIns(section.Children.Select<HabitItemViewModel, string>((Func<HabitItemViewModel, string>) (item => item.Id)).ToList<string>());
        }
      }
      this.SetItems(list);
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (!(sender is ScrollViewer scrollViewer) || scrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
        return;
      scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta > 0 ? 1.0 : -1.0));
      e.Handled = true;
    }

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      if (Mouse.LeftButton != MouseButtonState.Pressed || this.RecordPopup.IsOpen)
        return;
      e.Handled = true;
    }

    public async Task TrySelectItem(string habitId)
    {
      for (int time = 0; this.Items.Count == 0 && time < 60; ++time)
        await Task.Delay(50);
      foreach (HabitItemBaseViewModel itemBaseViewModel in this.Items.ToList<HabitItemBaseViewModel>())
      {
        if (itemBaseViewModel.Selected && itemBaseViewModel.Id != habitId)
          itemBaseViewModel.Selected = false;
        else if (!itemBaseViewModel.Selected && itemBaseViewModel.Id == habitId)
        {
          itemBaseViewModel.Selected = true;
          this.HabitItems.ScrollIntoView((object) itemBaseViewModel);
        }
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitlistcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (HabitListControl) target;
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.OnRecordStoryCompleted);
          break;
        case 5:
          this.MorePopup = (EscPopup) target;
          break;
        case 6:
          this.ItemsGrid = (Grid) target;
          this.ItemsGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDrop);
          this.ItemsGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.TryDragItem);
          this.ItemsGrid.MouseMove += new MouseEventHandler(this.OnDragMove);
          break;
        case 7:
          this.HabitItems = (ListView) target;
          break;
        case 8:
          this.DragPopup = (Popup) target;
          break;
        case 9:
          this.PopupItem = (HabitListItem) target;
          break;
        case 10:
          this.PopupSection = (HabitListSectionItem) target;
          break;
        case 11:
          this.EmptyGrid = (Grid) target;
          break;
        case 12:
          this.RecordPopup = (Popup) target;
          break;
        case 13:
          this.RecordHintGrid = (StackPanel) target;
          break;
        case 14:
          this.CheckInAmountText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 3)
      {
        if (connectionId != 4)
          return;
        ((FrameworkElement) target).RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
      }
      else
        ((UIElement) target).PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel);
    }
  }
}
