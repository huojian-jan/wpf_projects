// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CheckList.ChecklistItemControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views.CheckList
{
  public class ChecklistItemControl : UserControl, IComponentConnector
  {
    private bool _clickFlag;
    private ChecklistControl _parent;
    private bool _raiseSaveTitleEvent = true;
    private CheckItemViewModel _model;
    private bool _inSticky;
    private bool _spliting;
    internal ChecklistItemControl ItemControl;
    internal Grid Container;
    internal Grid DragGrid;
    internal Grid CheckGrid;
    internal TextBlock HintText;
    internal DetailTextBox TitleTextBox;
    internal Grid Time;
    internal Border ReminderButton;
    internal TextBlock TimeText;
    internal Image DeleteButton;
    internal Grid BottomLineGrid;
    internal Line BottomLine;
    internal Line HighlightBottomLine;
    internal Grid BlinkBackground;
    private bool _contentLoaded;

    public ChecklistItemControl()
      : this((ChecklistControl) null, false)
    {
    }

    public ChecklistItemControl(ChecklistControl parent, bool inSticky, bool isDark = false)
    {
      this.InitializeComponent();
      this._parent = parent;
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      this.TitleTextBox.SetWordWrap(true);
      if (!inSticky)
        return;
      this._inSticky = true;
      this.DragGrid.Visibility = Visibility.Hidden;
      this.BottomLineGrid.Visibility = Visibility.Collapsed;
      ThemeUtil.SetTheme(isDark ? "dark" : "light", (FrameworkElement) this);
      this.TitleTextBox.SetBaseColor("StickyContentTextColor", "StickyContentTextColor", "StickyCompletedTextColor", "StickyTextColor20");
      this.TitleTextBox.SetLightTheme();
      this.TitleTextBox.Padding = new Thickness(6.0, 5.0, 0.0, 2.0);
      this.TitleTextBox.LineSpacing = 3.0;
      this.CheckGrid.Margin = new Thickness(3.0, 6.0, 0.0, 0.0);
      this.TitleTextBox.SetResourceReference(Control.FontSizeProperty, (object) "StickyFont13");
      this.TimeText.SetResourceReference(Control.FontSizeProperty, (object) "StickyFont12");
      this.TimeText.Margin = new Thickness(0.0, 7.0, 0.0, 0.0);
      this.CheckGrid.SetResourceReference(FrameworkElement.MarginProperty, (object) "StickyCheckItemIconMargin");
      this.Container.SetResourceReference(FrameworkElement.MinHeightProperty, (object) "StickyHeight30");
      this.ReminderButton.Width = 0.0;
      this.DeleteButton.Width = 0.0;
      this.DeleteButton.Margin = new Thickness(0.0);
      this.Time.Visibility = Visibility.Collapsed;
    }

    public string Id => this.Model?.Id;

    public CheckItemViewModel Model => this.DataContext as CheckItemViewModel;

    public event EventHandler<ItemTextChange> Split;

    public event EventHandler<ItemTextChange> Merge;

    public event EventHandler<string> MoveUp;

    public event EventHandler<string> MoveDown;

    public event EventHandler<string> Check;

    public event EventHandler<string> UnCheck;

    public event EventHandler<ItemTextChange> TitleChanged;

    public event EventHandler<string> TagSelected;

    public event EventHandler<string> ToastString;

    public event EventHandler<QuickSetModel> QuickItemSelected;

    public event EventHandler<double> CaretVerticalOffsetChanged;

    public event ChecklistItemControl.DragDelegate ItemDrag;

    public event EventHandler QuickAddTask;

    public event EventHandler<bool> PopOpened;

    public event EventHandler<bool> PopClosed;

    private ChecklistControl GetParent()
    {
      return this._parent ?? (this._parent = ChecklistItemControl.FindParent((DependencyObject) this));
    }

    private void BindEvents()
    {
      ChecklistControl parent = this.GetParent();
      if (parent == null)
        return;
      parent.Register(this);
      this.TitleTextBox.MergeText += new EventHandler<string>(this.OnMergeText);
      this.TitleTextBox.SplitText += new EventHandler<int>(this.OnSplitItem);
      this.TitleTextBox.MoveUp += (EventHandler) ((obj, arg) =>
      {
        EventHandler<string> moveUp = this.MoveUp;
        if (moveUp == null)
          return;
        moveUp((object) this, this.Id);
      });
      this.TitleTextBox.MoveDown += (EventHandler) ((obj, arg) =>
      {
        EventHandler<string> moveDown = this.MoveDown;
        if (moveDown == null)
          return;
        moveDown((object) this, this.Id);
      });
      this.TitleTextBox.CaretVerticalOffsetChanged += new EventHandler<double>(this.OnItemCaretVOffsetChanged);
      this.InitShortCut();
    }

    private void UnbindEvents()
    {
      ChecklistControl parent = this.GetParent();
      if (parent == null)
        return;
      parent.Unregister(this);
      this.TitleTextBox.MergeText -= new EventHandler<string>(this.OnMergeText);
      this.TitleTextBox.SplitText -= new EventHandler<int>(this.OnSplitItem);
      this.TitleTextBox.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnItemCaretVOffsetChanged);
      if (this._model != null)
      {
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnItemTitleChanged), "Title");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnSortOrderChanged), "SortOrder");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnDateChanged), "DisplayStartDate");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnStatusChanged), "Status");
        this._model = (CheckItemViewModel) null;
      }
      this.RemoveKeyBinding();
    }

    private void InitShortCut()
    {
      if (this.InputBindings.Count < 6)
        return;
      if (this._inSticky)
      {
        KeyBinding inputBinding = this.InputBindings[5] as KeyBinding;
        this.InputBindings.Clear();
        if (inputBinding != null)
          this.InputBindings.Add((InputBinding) inputBinding);
        KeyBindingManager.TryAddKeyBinding("CompleteTask", this.InputBindings[0] as KeyBinding);
      }
      else
      {
        KeyBindingManager.TryAddKeyBinding("ClearDate", this.InputBindings[0] as KeyBinding);
        KeyBindingManager.TryAddKeyBinding("SetToday", this.InputBindings[1] as KeyBinding);
        KeyBindingManager.TryAddKeyBinding("SetTomorrow", this.InputBindings[2] as KeyBinding);
        KeyBindingManager.TryAddKeyBinding("SetNextWeek", this.InputBindings[3] as KeyBinding);
        KeyBindingManager.TryAddKeyBinding("SetDate", this.InputBindings[4] as KeyBinding);
        KeyBindingManager.TryAddKeyBinding("CompleteTask", this.InputBindings[5] as KeyBinding);
      }
    }

    private void RemoveKeyBinding()
    {
      if (this.InputBindings.Count < 6)
        return;
      if (this._inSticky)
      {
        KeyBindingManager.RemoveKeyBinding("CompleteTask", this.InputBindings[0] as KeyBinding);
      }
      else
      {
        KeyBindingManager.RemoveKeyBinding("ClearDate", this.InputBindings[0] as KeyBinding);
        KeyBindingManager.RemoveKeyBinding("SetToday", this.InputBindings[1] as KeyBinding);
        KeyBindingManager.RemoveKeyBinding("SetTomorrow", this.InputBindings[2] as KeyBinding);
        KeyBindingManager.RemoveKeyBinding("SetNextWeek", this.InputBindings[3] as KeyBinding);
        KeyBindingManager.RemoveKeyBinding("SetDate", this.InputBindings[4] as KeyBinding);
        KeyBindingManager.RemoveKeyBinding("CompleteTask", this.InputBindings[5] as KeyBinding);
      }
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      CheckItemViewModel newValue = e.NewValue as CheckItemViewModel;
      if (e.OldValue is CheckItemViewModel oldValue)
      {
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnItemTitleChanged), "Title");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnSortOrderChanged), "SortOrder");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnStatusChanged), "Status");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnDateChanged), "DisplayStartDate");
      }
      ChecklistControl parent = this.GetParent();
      if ((parent != null ? (parent.InMainDetail ? 1 : 0) : 0) != 0)
        this.TitleTextBox.SetupSearchRender(false, true);
      if (newValue != null)
      {
        this.TitleTextBox.SetTextOffset(this.Model.Title, true, true);
        this._model = newValue;
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnItemTitleChanged), "Title");
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnSortOrderChanged), "SortOrder");
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnStatusChanged), "Status");
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnDateChanged), "DisplayStartDate");
        this.SetDateTextColor();
        Border reminderButton = this.ReminderButton;
        DateTime? displayStartDate;
        int num1;
        if (newValue.DisplayStartDate.HasValue || !newValue.Enable || !this.IsMouseOver)
        {
          displayStartDate = newValue.DisplayStartDate;
          num1 = displayStartDate.HasValue ? 2 : 1;
        }
        else
          num1 = 0;
        reminderButton.Visibility = (Visibility) num1;
        Image deleteButton = this.DeleteButton;
        int num2;
        if (!newValue.Enable || !this.IsMouseOver)
        {
          displayStartDate = newValue.DisplayStartDate;
          num2 = displayStartDate.HasValue ? 2 : 1;
        }
        else
          num2 = 0;
        deleteButton.Visibility = (Visibility) num2;
      }
      if ((parent != null ? (parent.InQuickAddWindow ? 1 : 0) : 0) == 0)
        return;
      this.DragGrid.Visibility = Visibility.Collapsed;
      this.BottomLineGrid.Margin = new Thickness(25.0, 0.0, 0.0, 0.0);
    }

    private void OnSortOrderChanged(object sender, PropertyChangedEventArgs e)
    {
      this.GetParent()?.SortChecklistItems();
    }

    private void OnStatusChanged(object sender, PropertyChangedEventArgs e)
    {
      this.GetParent()?.SortChecklistItems();
      this.SetDateTextColor();
    }

    private void SetDateTextColor()
    {
      CheckItemViewModel model = this._model;
      int status = model != null ? model.Status : 0;
      DateTime? displayStartDate = (DateTime?) this._model?.DisplayStartDate;
      if (status != 0)
        this.TimeText.SetResourceReference(Control.ForegroundProperty, (object) "BaseColorOpacity20");
      else if (displayStartDate.HasValue && displayStartDate.Value < DateTime.Today.Date)
        this.TimeText.SetResourceReference(Control.ForegroundProperty, (object) "OutDateColor");
      else
        this.TimeText.SetResourceReference(Control.ForegroundProperty, (object) "PrimaryColor");
    }

    private void OnDateChanged(object sender, PropertyChangedEventArgs e)
    {
      this.SetDateTextColor();
    }

    private void OnItemTitleChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.TitleTextBox.KeyboardFocused || !(this.TitleTextBox.Text != this.Model.Title))
        return;
      this.TitleTextBox.SetTextOffset(this.Model.Title, false, true);
    }

    private void OnItemCaretVOffsetChanged(object sender, double e)
    {
      EventHandler<double> verticalOffsetChanged = this.CaretVerticalOffsetChanged;
      if (verticalOffsetChanged == null)
        return;
      verticalOffsetChanged((object) this, e);
    }

    private void OnPopClosed(object sender, EventArgs e)
    {
      EventHandler<bool> popClosed = this.PopClosed;
      if (popClosed == null)
        return;
      popClosed((object) this, false);
    }

    private void OnPopOpened(object sender, EventArgs e)
    {
      EventHandler<bool> popOpened = this.PopOpened;
      if (popOpened == null)
        return;
      popOpened((object) this, false);
    }

    private void OnLinkPopClosed(object sender, EventArgs e)
    {
      EventHandler<bool> popClosed = this.PopClosed;
      if (popClosed == null)
        return;
      popClosed((object) this, true);
    }

    private void OnLinkPopOpened(object sender, EventArgs e)
    {
      EventHandler<bool> popOpened = this.PopOpened;
      if (popOpened == null)
        return;
      popOpened((object) this, true);
    }

    private async void OnHandleDown(object sender, MouseButtonEventArgs e)
    {
      ChecklistItemControl.DragDelegate itemDrag = this.ItemDrag;
      if (itemDrag == null)
        return;
      itemDrag(this.Model, (MouseEventArgs) e);
    }

    private async void OnSplitItem(object sender, int index)
    {
      ChecklistItemControl sender1 = this;
      if (sender1._spliting)
        return;
      sender1._spliting = true;
      object obj = (object) null;
      int num = 0;
      try
      {
        if (sender1._parent != null && sender1._parent.InQuickAddWindow && Utils.IfCtrlPressed())
        {
          EventHandler quickAddTask = sender1.QuickAddTask;
          if (quickAddTask != null)
            quickAddTask((object) sender1, (EventArgs) null);
          num = 1;
        }
        else if (sender1.Model.Enable)
        {
          sender1.Model.ShowAddHint = false;
          sender1._raiseSaveTitleEvent = false;
          index = Math.Min(index, sender1.TitleTextBox.Text.Length);
          EventHandler<ItemTextChange> split = sender1.Split;
          if (split != null)
            split((object) sender1, new ItemTextChange()
            {
              Id = sender1.Id,
              Text = sender1.TitleTextBox.Text.Substring(0, index),
              Extra = sender1.TitleTextBox.Text.Substring(index)
            });
        }
      }
      catch (object ex)
      {
        obj = ex;
      }
      await Task.Delay(100);
      sender1._spliting = false;
      object obj1 = obj;
      if (obj1 != null)
      {
        if (!(obj1 is Exception source))
          throw obj1;
        ExceptionDispatchInfo.Capture(source).Throw();
      }
      if (num == 1)
        return;
      obj = (object) null;
    }

    private void OnMergeText(object sender, string text)
    {
      this._raiseSaveTitleEvent = false;
      EventHandler<ItemTextChange> merge = this.Merge;
      if (merge == null)
        return;
      merge((object) this, new ItemTextChange()
      {
        Id = this.Id,
        Text = text
      });
    }

    private async void SelectTimeClick(object sender, RoutedEventArgs e)
    {
      ChecklistItemControl checklistItemControl = this;
      if (checklistItemControl.DataContext is CheckItemViewModel dataContext)
      {
        if (!dataContext.IsAgendaOwner && Utils.IsDida())
        {
          EventHandler<string> toastString = checklistItemControl.ToastString;
          if (toastString != null)
            toastString((object) null, Utils.GetString("AttendeeSetDate"));
        }
        if (!dataContext.Enable)
        {
          checklistItemControl.GetParent()?.TryToastUnableText();
          return;
        }
      }
      await Task.Delay(10);
      checklistItemControl.ShowSelectTime();
    }

    private void ShowSelectTime()
    {
      if (!ProChecker.CheckPro(ProType.ReminderForSubTasks) || this.Model == null)
        return;
      ChecklistControl parent = this.GetParent();
      if (parent != null && !parent.IsNewAddTask)
        this.Model.Save(false);
      this.ShowSetTimeDialog(new TimeData()
      {
        IsAllDay = this.Model.IsAllDay,
        StartDate = this.Model.DisplayStartDate,
        TimeZone = new TimeZoneViewModel(this.Model.IsFloating, this.Model.TimeZoneName),
        IsDefault = !this.Model.IsAllDay.HasValue && !this.Model.StartDate.HasValue
      });
    }

    public void ShowSetTimeDialog(TimeData timeData)
    {
      CheckItemViewModel model = this.Model;
      if (model == null)
        return;
      string id = model.Id;
      TimeData timeData1 = timeData;
      SetDateDialog dialog = SetDateDialog.GetDialog();
      dialog.ClearEventHandle();
      dialog.Save += (EventHandler<TimeData>) (async (o, data) =>
      {
        double repeatDiff = model.RepeatDiff;
        if (repeatDiff > 0.0)
        {
          TimeData timeData2 = data;
          DateTime? startDate = data.StartDate;
          ref DateTime? local = ref startDate;
          DateTime? nullable = local.HasValue ? new DateTime?(local.GetValueOrDefault().AddDays(-repeatDiff)) : new DateTime?();
          timeData2.StartDate = nullable;
        }
        model.SourceViewModel.IsAllDay = data.IsAllDay;
        model.SourceViewModel.StartDate = data.StartDate;
        model.SourceViewModel.RemindTime = new DateTime?();
        ChecklistControl parent = this.GetParent();
        if ((parent != null ? (parent.IsNewAddTask ? 1 : 0) : 0) != 0)
          return;
        await TaskService.SetCheckItemDate(model.TaskServerId, model.Id, new TimeDataModel()
        {
          StartDate = model.StartDate,
          IsAllDay = model.IsAllDay
        });
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskServerId ?? "");
        if (thinTaskById != null && (!data.IsAllDay.HasValue || !data.IsAllDay.Value) && !Utils.IsEmptyDate(data.StartDate) && ((int) thinTaskById.isAllDay ?? (Utils.IsEmptyDate(thinTaskById.startDate) ? 1 : 0)) != 0 && thinTaskById.timeZone != TimeZoneData.LocalTimeZoneModel?.TimeZoneName)
        {
          thinTaskById.timeZone = TimeZoneData.LocalTimeZoneModel?.TimeZoneName;
          await TaskService.UpdateTaskOnTimeChanged(thinTaskById);
        }
        this.GetParent()?.NotifyTaskChanged(CheckItemModifyType.SetTime);
      });
      dialog.Clear += (EventHandler) (async (o, args) =>
      {
        model.SourceViewModel.IsAllDay = new bool?();
        model.SourceViewModel.StartDate = new DateTime?();
        model.SourceViewModel.RemindTime = new DateTime?();
        ChecklistControl parent = this.GetParent();
        if ((parent != null ? (parent.IsNewAddTask ? 1 : 0) : 0) != 0)
          return;
        await TaskService.SetCheckItemDate(model.TaskServerId, model.Id, new TimeDataModel()
        {
          StartDate = new DateTime?(),
          IsAllDay = new bool?()
        });
        this.GetParent()?.NotifyTaskChanged(CheckItemModifyType.SetTime);
      });
      dialog.Hided += (EventHandler) ((obj, e) => this.GetParent()?.OnDatePopupOpenChange(false));
      System.Windows.Point point = this.TranslatePoint(new System.Windows.Point(this.ActualWidth - 180.0, 30.0), (UIElement) this.BottomLineGrid);
      dialog.Show(timeData1, new SetDateDialogArgs(itemMode: true, target: (UIElement) this.BottomLineGrid, hOffset: point.X, vOffset: point.Y, showQuickDate: false));
      this.GetParent()?.OnDatePopupOpenChange(true);
    }

    private static ChecklistControl FindParent(DependencyObject child)
    {
      if (child == null)
        return (ChecklistControl) null;
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      if (parent == null)
        return (ChecklistControl) null;
      return parent is ChecklistControl checklistControl ? checklistControl : ChecklistItemControl.FindParent(parent);
    }

    public void Blink()
    {
      object resource = Application.Current?.FindResource((object) "ColorPrimary");
      if (resource == null || !(resource is Color toValue))
        return;
      toValue.A = (byte) 26;
      ColorAnimation colorAnimation = new ColorAnimation(toValue, new Duration(TimeSpan.FromSeconds(0.7)));
      colorAnimation.AutoReverse = true;
      colorAnimation.BeginTime = new TimeSpan?(TimeSpan.FromSeconds(0.1));
      ColorAnimation animation = colorAnimation;
      animation.Completed += (EventHandler) ((sender, obj) => this.BlinkBackground.Visibility = Visibility.Collapsed);
      this.BlinkBackground.Visibility = Visibility.Visible;
      this.BlinkBackground.Background = (Brush) new SolidColorBrush(Colors.Transparent);
      this.BlinkBackground.Background.BeginAnimation(SolidColorBrush.ColorProperty, (AnimationTimeline) animation);
    }

    public void FocusTitle(int caretIndex = -1, bool scroll = false)
    {
      this.TitleTextBox.FocusText(caretIndex);
      if (!scroll)
        return;
      EventHandler<double> verticalOffsetChanged = this.CaretVerticalOffsetChanged;
      if (verticalOffsetChanged == null)
        return;
      verticalOffsetChanged((object) this, 21.0);
    }

    public async void FocusEnd() => this.TitleTextBox.FocusEnd();

    private void DeleteClick(object sender, MouseButtonEventArgs e)
    {
      if (this.Model == null)
        return;
      this.GetParent()?.RemoveItemById(this.Model.Id);
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.HighlightBottomLine.Visibility = Visibility.Visible;
    }

    private void ItemChecked(object sender, RoutedEventArgs e) => this.ChangeItemStatus(true);

    private void ItemUnChecked(object sender, RoutedEventArgs e) => this.ChangeItemStatus(false);

    private void ChangeItemStatus(bool isChecked)
    {
      if (!this._clickFlag)
        return;
      this.ToggleCompleted(isChecked);
      this._clickFlag = false;
    }

    public void ToggleTaskCompleted()
    {
      if (this.Model == null)
        return;
      this.ToggleCompleted(this.Model.Status == 0);
    }

    private void ToggleCompleted(bool isChecked)
    {
      if (isChecked)
      {
        EventHandler<string> check = this.Check;
        if (check == null)
          return;
        check((object) this, this.Id);
      }
      else
      {
        EventHandler<string> unCheck = this.UnCheck;
        if (unCheck == null)
          return;
        unCheck((object) this, this.Id);
      }
    }

    private async void CheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      this._clickFlag = true;
    }

    private async void OnLostFocus(object sender, EventArgs e)
    {
      this.HighlightBottomLine.Visibility = Visibility.Collapsed;
      if (this.Model == null || !(this.Model.Title != this.TitleTextBox.Text))
        return;
      this.Model.SourceViewModel.Title = this.TitleTextBox.Text;
      await this.TrySaveItem(this.Model);
    }

    private async Task TrySaveItem(CheckItemViewModel model)
    {
      await this.SaveItemTitle(model);
      if (this._parent != null && (this._parent.InQuickAddWindow || this._parent.IsNewAddTask) || model == null)
        return;
      await SyncStatusDao.AddSyncStatus(model.TaskServerId, 0);
      SyncManager.TryDelaySync();
    }

    private async Task SaveItemTitle(CheckItemViewModel model)
    {
      if (model == null)
        return;
      await model.Save(false);
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is CheckItemViewModel dataContext))
        return;
      this.Time.Cursor = dataContext.Enable ? Cursors.Hand : Cursors.No;
      this.TimeText.Cursor = dataContext.Enable ? Cursors.Hand : Cursors.No;
      Border reminderButton = this.ReminderButton;
      DateTime? displayStartDate = dataContext.DisplayStartDate;
      int num1;
      if (displayStartDate.HasValue || !dataContext.Enable)
      {
        displayStartDate = dataContext.DisplayStartDate;
        num1 = displayStartDate.HasValue ? 2 : 1;
      }
      else
        num1 = 0;
      reminderButton.Visibility = (Visibility) num1;
      Image deleteButton = this.DeleteButton;
      int num2;
      if (!dataContext.Enable)
      {
        displayStartDate = dataContext.DisplayStartDate;
        num2 = displayStartDate.HasValue ? 2 : 1;
      }
      else
        num2 = 0;
      deleteButton.Visibility = (Visibility) num2;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e) => this.HideOperationButton();

    private void HideOperationButton()
    {
      if (!(this.DataContext is CheckItemViewModel dataContext))
        return;
      Border reminderButton = this.ReminderButton;
      DateTime? displayStartDate = dataContext.DisplayStartDate;
      int num1 = displayStartDate.HasValue ? 2 : 1;
      reminderButton.Visibility = (Visibility) num1;
      Image deleteButton = this.DeleteButton;
      displayStartDate = dataContext.DisplayStartDate;
      int num2 = displayStartDate.HasValue ? 2 : 1;
      deleteButton.Visibility = (Visibility) num2;
    }

    private async void OnTitleChanged(object sender, EventArgs e)
    {
      ChecklistItemControl sender1 = this;
      string text = sender1.TitleTextBox?.Text;
      CheckItemViewModel model = sender1.Model;
      if (model == null)
      {
        text = (string) null;
        model = (CheckItemViewModel) null;
      }
      else
      {
        if (text != null)
          model.SourceViewModel.Title = text;
        if (!model.IsValid)
        {
          text = (string) null;
          model = (CheckItemViewModel) null;
        }
        else
        {
          ChecklistControl parent = sender1.GetParent();
          if ((parent != null ? (parent.IsNewAddTask ? 1 : 0) : 0) != 0)
          {
            text = (string) null;
            model = (CheckItemViewModel) null;
          }
          else
          {
            await sender1.TrySaveItem(model);
            EventHandler<ItemTextChange> titleChanged = sender1.TitleChanged;
            if (titleChanged == null)
            {
              text = (string) null;
              model = (CheckItemViewModel) null;
            }
            else
            {
              titleChanged((object) sender1, new ItemTextChange()
              {
                Id = model.Id,
                Text = text
              });
              text = (string) null;
              model = (CheckItemViewModel) null;
            }
          }
        }
      }
    }

    public void SetDate(string key)
    {
      if (!ProChecker.CheckPro(ProType.ReminderForSubTasks))
        return;
      CheckItemViewModel model = this.Model;
      if (model == null || !model.Enable)
        return;
      double repeatDiff = model.RepeatDiff;
      switch (key)
      {
        case "today":
          model.SourceViewModel.StartDate = new DateTime?(DateTime.Today.AddDays(-repeatDiff));
          model.SourceViewModel.IsAllDay = new bool?(true);
          break;
        case "tomorrow":
          TaskBaseViewModel sourceViewModel1 = model.SourceViewModel;
          DateTime dateTime1 = DateTime.Today;
          dateTime1 = dateTime1.AddDays(1.0);
          DateTime? nullable1 = new DateTime?(dateTime1.AddDays(-repeatDiff));
          sourceViewModel1.StartDate = nullable1;
          model.SourceViewModel.IsAllDay = new bool?(true);
          break;
        case "nextweek":
          TaskBaseViewModel sourceViewModel2 = model.SourceViewModel;
          DateTime dateTime2 = DateTime.Today;
          dateTime2 = dateTime2.AddDays(7.0);
          DateTime? nullable2 = new DateTime?(dateTime2.AddDays(-repeatDiff));
          sourceViewModel2.StartDate = nullable2;
          model.SourceViewModel.IsAllDay = new bool?(true);
          break;
      }
      this.ReminderButton.Visibility = Visibility.Hidden;
      model.SourceViewModel.RemindTime = new DateTime?();
      ChecklistControl parent = this.GetParent();
      if ((parent != null ? (parent.IsNewAddTask ? 1 : 0) : 0) != 0 || !model.StartDate.HasValue)
        return;
      TaskService.SetCheckItemDate(model.TaskServerId, model.Id, new TimeDataModel()
      {
        IsAllDay = new bool?(true),
        StartDate = model.StartDate
      });
      SyncManager.TryDelaySync();
    }

    private void SelectDate(object sender, EventArgs e)
    {
      if (this._inSticky)
        return;
      this.ShowSelectTime();
    }

    public void SelectDate() => this.ShowSelectTime();

    public async void ClearDate()
    {
      this.Model.SourceViewModel.StartDate = new DateTime?();
      this.Model.SourceViewModel.RemindTime = new DateTime?();
      this.ReminderButton.Visibility = this.Model.DisplayStartDate.HasValue || !this.Model.Enable ? Visibility.Hidden : Visibility.Visible;
      await TaskService.SetCheckItemDate(this.Model.TaskServerId, this.Model.Id, new TimeDataModel()
      {
        StartDate = new DateTime?(),
        IsAllDay = new bool?()
      });
      SyncManager.TryDelaySync();
    }

    private void CheckEditable(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is CheckItemViewModel dataContext) || dataContext.IsAgendaOwner || !Utils.IsDida())
        return;
      EventHandler<string> toastString = this.ToastString;
      if (toastString == null)
        return;
      toastString((object) null, Utils.GetString("AttendeeModifyContent"));
    }

    private void CheckChckBoxEditable(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is CheckItemViewModel dataContext))
        return;
      if (!dataContext.IsAgendaOwner && Utils.IsDida())
      {
        EventHandler<string> toastString = this.ToastString;
        if (toastString != null)
          toastString((object) null, Utils.GetString("AttendeeCompleteSubtask"));
      }
      if (dataContext.Enable)
        return;
      this.GetParent()?.TryToastUnableText();
    }

    public void RemoveUndoCache()
    {
    }

    private void OnItemSelected(object sender, QuickSetModel e)
    {
      EventHandler<QuickSetModel> quickItemSelected = this.QuickItemSelected;
      if (quickItemSelected == null)
        return;
      quickItemSelected(sender, e);
    }

    private void EditorOnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    public double GetSearchOffset()
    {
      double firstSearchIndex = this.TitleTextBox.GetFirstSearchIndex();
      if (firstSearchIndex >= 0.0)
      {
        EventHandler<double> verticalOffsetChanged = this.CaretVerticalOffsetChanged;
        if (verticalOffsetChanged != null)
          verticalOffsetChanged((object) this, firstSearchIndex);
      }
      return firstSearchIndex;
    }

    public void SetStickyTheme(bool isDark)
    {
      ThemeUtil.SetTheme(isDark ? "dark" : "light", (FrameworkElement) this);
      this.TitleTextBox.SetLightTheme();
    }

    public void UnbindEvent()
    {
      this.TitleTextBox.MergeText -= new EventHandler<string>(this.OnMergeText);
      this.TitleTextBox.SplitText -= new EventHandler<int>(this.OnSplitItem);
      this.TitleTextBox.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnItemCaretVOffsetChanged);
      if (this._model != null)
      {
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnItemTitleChanged), "Title");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnSortOrderChanged), "SortOrder");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnDateChanged), "DisplayStartDate");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnStatusChanged), "Status");
        this._model = (CheckItemViewModel) null;
      }
      this.RemoveKeyBinding();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/checklist/checklistitemcontrol.xaml", UriKind.Relative));
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
          this.ItemControl = (ChecklistItemControl) target;
          this.ItemControl.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          this.ItemControl.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
          this.ItemControl.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.DragGrid = (Grid) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnHandleDown);
          break;
        case 5:
          this.CheckGrid = (Grid) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.CheckChckBoxEditable);
          break;
        case 7:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.CheckBoxClick);
          ((ToggleButton) target).Checked += new RoutedEventHandler(this.ItemChecked);
          ((ToggleButton) target).Unchecked += new RoutedEventHandler(this.ItemUnChecked);
          break;
        case 8:
          this.HintText = (TextBlock) target;
          break;
        case 9:
          this.TitleTextBox = (DetailTextBox) target;
          break;
        case 10:
          this.Time = (Grid) target;
          this.Time.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.SelectTimeClick);
          break;
        case 11:
          this.ReminderButton = (Border) target;
          this.ReminderButton.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.SelectTimeClick);
          break;
        case 12:
          this.TimeText = (TextBlock) target;
          this.TimeText.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.SelectTimeClick);
          break;
        case 13:
          this.DeleteButton = (Image) target;
          this.DeleteButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.DeleteClick);
          break;
        case 14:
          this.BottomLineGrid = (Grid) target;
          break;
        case 15:
          this.BottomLine = (Line) target;
          break;
        case 16:
          this.HighlightBottomLine = (Line) target;
          break;
        case 17:
          this.BlinkBackground = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void DragDelegate(CheckItemViewModel model, MouseEventArgs arg);

    public delegate void SetTimeDelegate(string id, TimeData timeData);
  }
}
