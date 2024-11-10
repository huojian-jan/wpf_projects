// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.EditHabitLogWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Dal;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class EditHabitLogWindow : MyWindow, IComponentConnector
  {
    private readonly bool _isEdit;
    private CheckInLogViewModel _source;
    internal Image IconImage;
    internal Border IconTextBorder;
    internal EmjTextBlock IconText;
    internal TextBlock Title;
    internal TextBlock CheckInDateText;
    internal TextBox TitleTextBox;
    internal TextBlock HintText;
    internal TextBlock DoNotShowText;
    internal Button OkButton;
    private bool _contentLoaded;

    public EditHabitLogWindow() => this.InitializeComponent();

    public EditHabitLogWindow(CheckInLogViewModel model, bool isEdit = true, bool isNew = false)
    {
      this.InitializeComponent();
      if (isEdit)
        this.DoNotShowText.Visibility = Visibility.Collapsed;
      if (isNew)
        model.Score = model.UnCompleted ? 10 : 50;
      this._source = model;
      this.DataContext = (object) model.Copy();
      this._isEdit = isEdit;
      this.Title.Text = model.HabitName;
      this.CheckInDateText.Text = this.GetFormatedDateStr(model.Date);
      this.HintText.Text = model.UnCompleted ? Utils.GetString("UnCompleteLogHint") : (model.IsBoolHabit ? Utils.GetString("CheckInLogHint") : Utils.GetString("CheckInLogHintReal"));
      this.SetIcon(model.IconRes, model.Color);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => this.Activate();

    private string GetFormatedDateStr(DateTime fullDate)
    {
      string str1 = DateUtils.FormatShortDate(fullDate);
      string str2 = DateUtils.FormatWeekDayName(fullDate, true);
      return !Utils.IsEn() ? str1 + " " + str2 : str2 + ", " + str1;
    }

    private void SetIcon(string iconRes, string color)
    {
      if (iconRes.StartsWith("txt"))
      {
        this.IconTextBorder.Background = string.IsNullOrEmpty(color) ? (Brush) ThemeUtil.GetPrimaryColor(0.1) : (Brush) ThemeUtil.GetColorInString(color);
        this.IconText.Text = ((IEnumerable<string>) iconRes.Split('_')).LastOrDefault<string>() ?? "";
        this.IconText.Foreground = string.IsNullOrEmpty(color) ? (Brush) ThemeUtil.GetColor("PrimaryColor") : (Brush) Brushes.White;
        this.IconImage.Visibility = Visibility.Collapsed;
      }
      else
      {
        try
        {
          this.IconImage.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/Habits/" + iconRes.ToLower() + ".png"));
        }
        catch (Exception ex)
        {
        }
      }
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnEscKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (!Utils.IfCtrlPressed())
            break;
          this.OnSaveClick((object) this, (RoutedEventArgs) e);
          break;
        case Key.Escape:
          this.Close();
          break;
      }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      EditHabitLogWindow editHabitLogWindow = this;
      CheckInLogViewModel data = editHabitLogWindow.GetData();
      if (data != null)
      {
        if (!editHabitLogWindow._isEdit || string.IsNullOrEmpty(data.Id))
        {
          if (!string.IsNullOrEmpty(editHabitLogWindow.TitleTextBox.Text.Trim()) || data.Score > 0)
          {
            data.Content = editHabitLogWindow.TitleTextBox.Text.Trim();
            data.Score = data.Score;
            await HabitService.AddCheckInRecord(data);
          }
        }
        else if (!string.IsNullOrEmpty(editHabitLogWindow.TitleTextBox.Text.Trim()) || data.Score > 0)
        {
          data.Content = editHabitLogWindow.TitleTextBox.Text.Trim();
          await HabitService.SaveHabitCheckinRecord(data.Id, editHabitLogWindow.TitleTextBox.Text, data.Score);
          editHabitLogWindow._source.Content = data.Content;
          editHabitLogWindow._source.Score = data.Score;
        }
        else
        {
          editHabitLogWindow._source.IsHide = true;
          await HabitService.DeleteCheckInRecord(data.Id);
        }
      }
      editHabitLogWindow.Close();
      data = (CheckInLogViewModel) null;
    }

    private CheckInLogViewModel GetData()
    {
      return this.DataContext != null && this.DataContext is CheckInLogViewModel dataContext ? dataContext : (CheckInLogViewModel) null;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private async void OnLogWindowLoaded(object sender, RoutedEventArgs e)
    {
      CheckInLogViewModel data = this.GetData();
      if (data == null)
        return;
      this.TitleTextBox.Text = data.Content;
      this.TitleTextBox.Focus();
      this.TitleTextBox.CaretIndex = data.Content.Length;
    }

    private async void OnDoNotShowClick(object sender, MouseButtonEventArgs e)
    {
      EditHabitLogWindow editHabitLogWindow = this;
      CheckInLogViewModel data = editHabitLogWindow.GetData();
      if (data == null)
        return;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DoNotShowHabitRecord"), Utils.GetString("DoNotShowHabitRecordHint"), Utils.GetString("GotIt"), string.Empty);
      customerDialog.Owner = Window.GetWindow((DependencyObject) editHabitLogWindow);
      customerDialog.ShowDialog();
      await HabitDao.HabitUnableRecord(data.HabitId);
      editHabitLogWindow.Close();
    }

    private void OnTitleTextChanged(object sender, TextChangedEventArgs e)
    {
      this.OkButton.IsEnabled = this.TitleTextBox.Text.Length <= 1024;
    }

    private void OnScoreClick(object sender, MouseButtonEventArgs e)
    {
      CheckInLogViewModel data = this.GetData();
      int result;
      if (data == null || !(sender is Grid grid) || !int.TryParse(grid.Tag as string, out result))
        return;
      data.Score = data.Score == result ? 0 : result;
    }

    private void TryDargMove(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      this.DragMove();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/edithabitlogwindow.xaml", UriKind.Relative));
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
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.TryDargMove);
          break;
        case 2:
          this.IconImage = (Image) target;
          break;
        case 3:
          this.IconTextBorder = (Border) target;
          break;
        case 4:
          this.IconText = (EmjTextBlock) target;
          break;
        case 5:
          this.Title = (TextBlock) target;
          break;
        case 6:
          this.CheckInDateText = (TextBlock) target;
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnScoreClick);
          break;
        case 8:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnScoreClick);
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnScoreClick);
          break;
        case 10:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnScoreClick);
          break;
        case 11:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnScoreClick);
          break;
        case 12:
          this.TitleTextBox = (TextBox) target;
          this.TitleTextBox.KeyDown += new KeyEventHandler(this.OnEscKeyDown);
          this.TitleTextBox.TextChanged += new TextChangedEventHandler(this.OnTitleTextChanged);
          break;
        case 13:
          this.HintText = (TextBlock) target;
          break;
        case 14:
          this.DoNotShowText = (TextBlock) target;
          this.DoNotShowText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDoNotShowClick);
          break;
        case 15:
          this.OkButton = (Button) target;
          this.OkButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 16:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
