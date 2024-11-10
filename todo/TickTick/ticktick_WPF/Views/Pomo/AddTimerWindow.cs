// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.AddTimerWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class AddTimerWindow : Window, IOkCancelWindow, IComponentConnector
  {
    private bool _isNew;
    private TimerModel _timer;
    private PomoFilterControl _pomoFilter;
    internal Grid IconGrid;
    internal Image IconImage;
    internal EscPopup SetIconPopup;
    internal SetHabitIconControl SetIconControl;
    internal EmojiEditor TitleText;
    internal HoverIconButton LinkIcon;
    internal Border ClearIcon;
    internal RadioButton PomoRadio;
    internal NumInputTextBox PomoCount;
    internal RadioButton TimingRadio;
    internal Button SaveButton;
    private bool _contentLoaded;

    public AddTimerWindow(TimerModel model = null)
    {
      this.InitializeComponent();
      if (model == null)
      {
        model = AddTimerWindow.GetDefaultTimer();
        this._isNew = true;
      }
      this._timer = model;
      this.Title = Utils.GetString(this._isNew ? "AddTimer" : "EditTimer");
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async Task SetTitleText()
    {
      if (!string.IsNullOrEmpty(this._timer.ObjId))
      {
        if (this._timer.ObjType == "task")
        {
          TaskModel taskById = await TaskDao.GetTaskById(this._timer.ObjId);
          if (taskById != null)
          {
            this.TitleText.Text = taskById.title;
            return;
          }
        }
        else if (this._timer.ObjType == "habit")
        {
          HabitModel habitById = await HabitDao.GetHabitById(this._timer.ObjId);
          if (habitById != null)
          {
            this.TitleText.Text = habitById.Name;
            return;
          }
        }
      }
      this.TitleText.Text = this._timer.Name;
    }

    private void SetIconLocation()
    {
      bool flag = !string.IsNullOrEmpty(this._timer.ObjId);
      this.TitleText.ReadOnly = flag;
      this.TitleText.TextBorder.IsEnabled = this._isNew || !this.TitleText.ReadOnly;
      if (this._isNew)
      {
        this.LinkIcon.Visibility = string.IsNullOrEmpty(this._timer.Name) && string.IsNullOrEmpty(this._timer.ObjId) || !string.IsNullOrEmpty(this._timer.ObjId) ? Visibility.Visible : Visibility.Collapsed;
        this.ClearIcon.Visibility = !string.IsNullOrEmpty(this._timer.Name) && string.IsNullOrEmpty(this._timer.ObjId) || !string.IsNullOrEmpty(this._timer.ObjId) ? Visibility.Visible : Visibility.Collapsed;
        this.LinkIcon.IsHitTestVisible = string.IsNullOrEmpty(this._timer.ObjId);
        this.TitleText.Padding = new Thickness(flag ? 30.0 : 8.0, 0.0, 30.0, 0.0);
        this.LinkIcon.HorizontalAlignment = flag ? HorizontalAlignment.Left : HorizontalAlignment.Right;
      }
      else
      {
        this.LinkIcon.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
        this.ClearIcon.Visibility = string.IsNullOrEmpty(this._timer.Name) || flag ? Visibility.Collapsed : Visibility.Visible;
        this.TitleText.Padding = new Thickness(flag ? 30.0 : 8.0, 0.0, string.IsNullOrEmpty(this._timer.Name) || flag ? 8.0 : 30.0, 0.0);
        this.LinkIcon.HorizontalAlignment = HorizontalAlignment.Left;
        this.LinkIcon.IsHitTestVisible = false;
      }
    }

    private void OnSelectIconClick(object sender, MouseButtonEventArgs e)
    {
      this.SetIconPopup.IsOpen = true;
      this.SetIconControl.Reset(this._timer.Icon, this._timer.Color);
      HwndHelper.SetFocus((Popup) this.SetIconPopup, false);
    }

    private void SetIcon()
    {
      this.IconImage.Source = (ImageSource) HabitService.GetIcon(this._timer.Icon, this._timer.Color);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      this.SetTitleText();
      this.PomoCount.Text = this._timer.PomodoroTime.ToString();
      this.PomoRadio.IsChecked = new bool?(this._timer.Type == "pomodoro");
      this.TimingRadio.IsChecked = new bool?(this._timer.Type != "pomodoro");
      this.PomoCount.IsEnabled = this.PomoRadio.IsChecked.GetValueOrDefault();
      this.PomoCount.InputText.Opacity = this.PomoCount.IsEnabled ? 1.0 : 0.4;
      this.SetIcon();
    }

    private void OnIconSelected(object sender, EventArgs e)
    {
      if (sender is SetHabitIconControl habitIconControl)
      {
        if (habitIconControl.IsIcon)
        {
          this._timer.Icon = habitIconControl.SelectedIcon;
          this._timer.Color = habitIconControl.IconColor;
        }
        else
        {
          this._timer.Icon = "txt_" + habitIconControl.IconText;
          string color = habitIconControl.TextColor ?? "";
          if (this._timer.Color != color)
          {
            this._timer.Color = color;
            ticktick_WPF.Views.Misc.ColorSelector.ColorSelector.TryAddClickEvent(color);
          }
        }
      }
      this.SetIconPopup.IsOpen = false;
      this.SetIcon();
    }

    private void HideIconPopup(object sender, EventArgs e) => this.SetIconPopup.IsOpen = false;

    private void UpdateSaveButton()
    {
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(this.TitleText.Text) || !string.IsNullOrEmpty(this._timer.ObjId);
    }

    public void OnCancel() => this.Close();

    public async void Ok()
    {
      AddTimerWindow addTimerWindow = this;
      if (!addTimerWindow.SaveButton.IsEnabled)
        return;
      addTimerWindow._timer.ModifiedTime = DateTime.Now;
      addTimerWindow._timer.Type = addTimerWindow.TimingRadio.IsChecked.GetValueOrDefault() ? "timing" : "pomodoro";
      int result;
      if (addTimerWindow._timer.Type == "pomodoro" && int.TryParse(addTimerWindow.PomoCount.Text, out result))
        addTimerWindow._timer.PomodoroTime = result;
      addTimerWindow._timer.Name = addTimerWindow.TitleText.Text;
      if (addTimerWindow._isNew)
      {
        TimerModel timerModel = addTimerWindow._timer;
        timerModel.SortOrder = await TimerService.GetNewTimerSortOrder();
        timerModel = (TimerModel) null;
        addTimerWindow._timer.CreatedTime = DateTime.Now;
        if (addTimerWindow._timer.ObjType == "habit")
          await HabitService.SaveHabitIcon(addTimerWindow._timer.ObjId, addTimerWindow._timer.Icon, addTimerWindow._timer.Color);
        int num = await BaseDao<TimerModel>.InsertAsync(addTimerWindow._timer);
        UserActCollectUtils.AddClickEvent("timer", "add_timer", string.IsNullOrEmpty(addTimerWindow._timer.ObjId) ? "enter_timer" : "select_task");
        UserActCollectUtils.AddClickEvent("timer", "add_timer", addTimerWindow._timer.Type == "pomodoro" ? "pomo" : "stopwatch");
      }
      else
      {
        addTimerWindow._timer.ModifiedTime = DateTime.Now;
        await TimerService.UpdateTimer(addTimerWindow._timer);
        if (addTimerWindow._timer.ObjType == "habit")
          await HabitService.SaveHabitIcon(addTimerWindow._timer.ObjId, addTimerWindow._timer.Icon, addTimerWindow._timer.Color);
        TickFocusManager.OnTimerChanged(addTimerWindow._timer);
      }
      PomoNotifier.NotifyTimerChanged();
      SyncManager.Sync();
      addTimerWindow.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.Ok();

    private static TimerModel GetDefaultTimer()
    {
      return new TimerModel()
      {
        Id = Utils.GetGuid(),
        Icon = "habit_daily_check_in",
        Color = HabitUtils.IconColorDict["habit_daily_check_in"],
        UserId = LocalSettings.Settings.LoginUserId,
        PomodoroTime = LocalSettings.Settings.PomoDuration
      };
    }

    private void OnTitleChanged(object sender, EventArgs e)
    {
      this._timer.Name = this.TitleText.Text;
      this.SetIconLocation();
      this.UpdateSaveButton();
    }

    private void OnClearClick(object sender, MouseButtonEventArgs e)
    {
      this._timer.ObjId = (string) null;
      this._timer.ObjType = (string) null;
      this.TitleText.Text = string.Empty;
    }

    private async void OnLinkClick(object sender, MouseButtonEventArgs e)
    {
      AddTimerWindow addTimerWindow1 = this;
      List<string> objIds = await TimerDao.GetObjIds();
      if (addTimerWindow1._pomoFilter == null)
      {
        EscPopup escPopup1 = new EscPopup();
        escPopup1.StaysOpen = false;
        escPopup1.PlacementTarget = (UIElement) addTimerWindow1.TitleText;
        escPopup1.Placement = PlacementMode.Center;
        escPopup1.HorizontalOffset = -3.0;
        escPopup1.VerticalOffset = 208.0;
        EscPopup escPopup2 = escPopup1;
        AddTimerWindow addTimerWindow2 = addTimerWindow1;
        PomoFilterControl pomoFilterControl = new PomoFilterControl((Popup) escPopup2, false, linkedIds: objIds, loadTimers: false);
        pomoFilterControl.ShowComplete = true;
        pomoFilterControl.DataContext = (object) new FocusViewModel();
        addTimerWindow2._pomoFilter = pomoFilterControl;
        addTimerWindow1._pomoFilter.Container.Width = 350.0;
        // ISSUE: reference to a compiler-generated method
        addTimerWindow1._pomoFilter.ItemSelected += new EventHandler<DisplayItemModel>(addTimerWindow1.\u003COnLinkClick\u003Eb__20_0);
      }
      Popup parentPopup = addTimerWindow1._pomoFilter.GetParentPopup();
      parentPopup.PlacementTarget = (UIElement) addTimerWindow1.TitleText;
      parentPopup.IsOpen = true;
    }

    public static async Task ShowAddTimerWindow(Window owner)
    {
      if (!await ProChecker.CheckTimerLimit(owner))
        return;
      AddTimerWindow addTimerWindow = new AddTimerWindow();
      addTimerWindow.Owner = owner;
      addTimerWindow.ShowDialog();
    }

    private void OnPomoCheckChanged(object sender, RoutedEventArgs e)
    {
      this.PomoCount.IsEnabled = this.PomoRadio.IsChecked.GetValueOrDefault();
      this.PomoCount.InputText.Opacity = this.PomoCount.IsEnabled ? 1.0 : 0.4;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/addtimerwindow.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnWindowLoaded);
          break;
        case 2:
          this.IconGrid = (Grid) target;
          this.IconGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSelectIconClick);
          break;
        case 3:
          this.IconImage = (Image) target;
          break;
        case 4:
          this.SetIconPopup = (EscPopup) target;
          break;
        case 5:
          this.SetIconControl = (SetHabitIconControl) target;
          break;
        case 6:
          this.TitleText = (EmojiEditor) target;
          break;
        case 7:
          this.LinkIcon = (HoverIconButton) target;
          break;
        case 8:
          this.ClearIcon = (Border) target;
          this.ClearIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClearClick);
          break;
        case 9:
          this.PomoRadio = (RadioButton) target;
          this.PomoRadio.Checked += new RoutedEventHandler(this.OnPomoCheckChanged);
          this.PomoRadio.Unchecked += new RoutedEventHandler(this.OnPomoCheckChanged);
          break;
        case 10:
          this.PomoCount = (NumInputTextBox) target;
          break;
        case 11:
          this.TimingRadio = (RadioButton) target;
          break;
        case 12:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 13:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
