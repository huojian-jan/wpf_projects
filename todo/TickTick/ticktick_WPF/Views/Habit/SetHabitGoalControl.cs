// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.SetHabitGoalControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class SetHabitGoalControl : UserControl, IComponentConnector
  {
    private HabitGoalModel _goal;
    private Timer _resetTimer = new Timer(1500.0);
    internal CheckBox BoolTypeCheckBox;
    internal CheckBox RealTypeCheckBox;
    internal Border GoalBorder;
    internal TextBlock DailyGoalText;
    internal EscPopup SetTimesPopup;
    internal TextBox Times;
    internal TextBox Unit;
    internal CustomComboBox CheckTypeComboBox;
    internal Grid AutoRecordGrid;
    internal TextBlock EachTimeText;
    internal TextBox StepText;
    internal Button SaveButton;
    private bool _contentLoaded;

    public event EventHandler<HabitGoalModel> OnGoalSaved;

    public event EventHandler Closed;

    public SetHabitGoalControl()
    {
      this.InitializeComponent();
      this._resetTimer.Elapsed += new ElapsedEventHandler(this.ResetStepOrTimes);
    }

    public void Init(HabitModel habit)
    {
      this.BoolTypeCheckBox.IsChecked = new bool?(habit.IsBoolHabit());
      this.RealTypeCheckBox.IsChecked = new bool?(!habit.IsBoolHabit());
      this.DailyGoalText.Text = string.Format(Utils.GetString("DailyTimes"), (object) habit.Goal, (object) habit.Unit);
      this.EachTimeText.Text = string.Format(Utils.GetString("RecordAmountEachTime"), (object) habit.Unit);
      this._goal = new HabitGoalModel()
      {
        Type = habit.Type,
        Goal = habit.Goal,
        Step = habit.Step,
        Unit = habit.Unit
      };
      ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
      observableCollection.Add(new ComboBoxViewModel((object) 0, Utils.GetString("AutoRecord"), 28.0));
      observableCollection.Add(new ComboBoxViewModel((object) 1, Utils.GetString("ManualRecord"), 28.0));
      observableCollection.Add(new ComboBoxViewModel((object) 2, Utils.GetString("CompleteAll"), 28.0));
      ObservableCollection<ComboBoxViewModel> items = observableCollection;
      this.CheckTypeComboBox.Init<ComboBoxViewModel>(items, items[0]);
    }

    private async void OnBoolTypeCheckChanged(object sender, MouseButtonEventArgs e)
    {
      await Task.Delay(50);
      CheckBox realTypeCheckBox = this.RealTypeCheckBox;
      bool? isChecked = this.BoolTypeCheckBox.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      realTypeCheckBox.IsChecked = nullable;
      UserActCollectUtils.AddClickEvent("habit", "add_edit_habit", "goal_achieve");
    }

    private async void OnRealTypeCheckChanged(object sender, MouseButtonEventArgs e)
    {
      await Task.Delay(50);
      CheckBox boolTypeCheckBox = this.BoolTypeCheckBox;
      bool? isChecked = this.RealTypeCheckBox.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      boolTypeCheckBox.IsChecked = nullable;
      UserActCollectUtils.AddClickEvent("habit", "add_edit_habit", "goal_amount");
    }

    private void OpenTimesPopup(object sender, MouseButtonEventArgs e)
    {
      this.Times.Text = this._goal.Goal.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      this.Unit.TextChanged -= new TextChangedEventHandler(this.OnUnitChanged);
      this.Unit.TextChanged += new TextChangedEventHandler(this.OnUnitChanged);
      this.Unit.Text = this._goal.Unit;
      this.AutoRecordGrid.Visibility = Visibility.Collapsed;
      if (this._goal.Step > 0.0)
      {
        this.CheckTypeComboBox.SetSelected((object) 0);
        this.AutoRecordGrid.Visibility = Visibility.Visible;
        this.StepText.Text = (this._goal.Step <= 0.0 ? 1.0 : this._goal.Step).ToString((IFormatProvider) CultureInfo.InvariantCulture);
      }
      else if (Math.Abs(this._goal.Step) < 0.0001)
        this.CheckTypeComboBox.SetSelected((object) 2);
      else if (Math.Abs(this._goal.Step + 1.0) < 0.0001)
        this.CheckTypeComboBox.SetSelected((object) 1);
      this.SetTimesPopup.IsOpen = true;
      HwndHelper.SetFocus((Popup) this.SetTimesPopup, false);
    }

    private void OnBoolTypeTextClick(object sender, MouseButtonEventArgs e)
    {
      this.BoolTypeCheckBox.IsChecked = new bool?(true);
      this.RealTypeCheckBox.IsChecked = new bool?(false);
    }

    private void OnRealTypeTextClick(object sender, MouseButtonEventArgs e)
    {
      this.BoolTypeCheckBox.IsChecked = new bool?(false);
      this.RealTypeCheckBox.IsChecked = new bool?(true);
    }

    private void OnSaveTimes(object sender, RoutedEventArgs e)
    {
      if (this.RealTypeCheckBox.IsChecked.GetValueOrDefault())
      {
        if (this.CheckTypeComboBox.SelectedItem.Value is int num)
        {
          switch (num)
          {
            case 0:
              double db1;
              if (MathUtil.TryParseString2Double(this.StepText.Text, out db1))
              {
                this._goal.Step = db1;
                break;
              }
              break;
            case 1:
              this._goal.Step = -1.0;
              break;
            case 2:
              this._goal.Step = 0.0;
              break;
          }
        }
        double db2;
        if (MathUtil.TryParseString2Double(this.Times.Text, out db2))
          this._goal.Goal = db2;
        if (!string.IsNullOrEmpty(this.Unit.Text))
          this._goal.Unit = this.Unit.Text;
        this._goal.Type = HabitType.Real.ToString();
      }
      else
        this._goal.Type = HabitType.Boolean.ToString();
      this.DailyGoalText.Text = string.Format(Utils.GetString("DailyTimes"), (object) this._goal.Goal, (object) this._goal.Unit);
      this.SetTimesPopup.IsOpen = false;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler closed = this.Closed;
      if (closed == null)
        return;
      closed((object) this, (EventArgs) null);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this._goal.Type = this.RealTypeCheckBox.IsChecked.GetValueOrDefault() ? HabitType.Real.ToString() : HabitType.Boolean.ToString();
      EventHandler<HabitGoalModel> onGoalSaved = this.OnGoalSaved;
      if (onGoalSaved == null)
        return;
      onGoalSaved((object) this, this._goal);
    }

    private void OnSetTimesCancel(object sender, RoutedEventArgs e)
    {
      this.SetTimesPopup.IsOpen = false;
    }

    private void CheckInputValid(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length >= 1 && !char.IsDigit(e.Text, e.Text.Length - 1) && e.Text != ".")
        e.Handled = true;
      if (!(sender is TextBox textBox))
        return;
      if (textBox.Text.Contains("."))
      {
        if (e.Text == ".")
          e.Handled = true;
        int length = textBox.Text.IndexOf(".", StringComparison.Ordinal);
        if (textBox.CaretIndex > length)
        {
          if (textBox.Text.Substring(length + 1, textBox.Text.Length - length - 1).Length < 2)
            return;
          e.Handled = true;
        }
        else
        {
          if (textBox.Text.Substring(0, length).Length < 5)
            return;
          e.Handled = true;
        }
      }
      else
      {
        if (textBox.Text.Length >= 5 && e.Text != ".")
          e.Handled = true;
        if (!(e.Text == ".") || textBox.CaretIndex != 0)
          return;
        e.Handled = true;
      }
    }

    private void ResetStepOrTimes(object sender, ElapsedEventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.ResetTextBox));
      this._resetTimer.Stop();
    }

    private void ResetTextBox()
    {
      double db1;
      if (MathUtil.TryParseString2Double(this.Times.Text, out db1))
      {
        if (Math.Abs(db1) < 0.0001)
        {
          this.Times.Text = "1";
          this.Times.SelectAll();
        }
      }
      else
      {
        this.Times.Text = "1";
        this.Times.SelectAll();
      }
      double db2;
      if (MathUtil.TryParseString2Double(this.StepText.Text, out db2))
      {
        if (Math.Abs(db2) >= 0.0001)
          return;
        this.StepText.Text = "1";
        this.StepText.SelectAll();
      }
      else
      {
        this.StepText.Text = "1";
        this.StepText.SelectAll();
      }
    }

    private void OnTimesOrStepTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!(sender is TextBox textBox))
        return;
      double db;
      if (MathUtil.TryParseString2Double(textBox.Text, out db))
      {
        if (Math.Abs(db) < 0.0001)
        {
          this._resetTimer.Stop();
          this._resetTimer.Start();
        }
      }
      else
      {
        this._resetTimer.Stop();
        this._resetTimer.Start();
      }
      textBox.Text = textBox.Text.Replace(" ", "");
    }

    private void OnTextLostFocus(object sender, RoutedEventArgs e)
    {
      double db;
      if (!(sender is TextBox textBox) || !MathUtil.TryParseString2Double(textBox.Text, out db))
        return;
      this._resetTimer.Stop();
      textBox.Text = Math.Abs(db) < 0.0001 ? "1" : db.ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    private void OnUnitTextLostFocus(object sender, RoutedEventArgs e)
    {
      if (!string.IsNullOrEmpty(this.Unit.Text))
        return;
      this.Unit.Text = this._goal.Unit;
    }

    private void OnUnitChanged(object sender, TextChangedEventArgs e)
    {
      this.EachTimeText.Text = string.Format(Utils.GetString("RecordAmountEachTime"), string.IsNullOrEmpty(this.Unit.Text) ? (object) Utils.GetString("Count") : (object) this.Unit.Text);
    }

    private void OnCheckTypeChanged(object sender, ComboBoxViewModel e)
    {
      this.AutoRecordGrid.Visibility = e.Value.Equals((object) 0) ? Visibility.Visible : Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/sethabitgoalcontrol.xaml", UriKind.Relative));
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
          this.BoolTypeCheckBox = (CheckBox) target;
          this.BoolTypeCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnBoolTypeCheckChanged);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBoolTypeTextClick);
          break;
        case 3:
          this.RealTypeCheckBox = (CheckBox) target;
          this.RealTypeCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnRealTypeCheckChanged);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnRealTypeTextClick);
          break;
        case 5:
          this.GoalBorder = (Border) target;
          this.GoalBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OpenTimesPopup);
          break;
        case 6:
          this.DailyGoalText = (TextBlock) target;
          break;
        case 7:
          this.SetTimesPopup = (EscPopup) target;
          break;
        case 8:
          this.Times = (TextBox) target;
          this.Times.PreviewTextInput += new TextCompositionEventHandler(this.CheckInputValid);
          this.Times.TextChanged += new TextChangedEventHandler(this.OnTimesOrStepTextChanged);
          this.Times.LostFocus += new RoutedEventHandler(this.OnTextLostFocus);
          break;
        case 9:
          this.Unit = (TextBox) target;
          this.Unit.LostFocus += new RoutedEventHandler(this.OnUnitTextLostFocus);
          break;
        case 10:
          this.CheckTypeComboBox = (CustomComboBox) target;
          break;
        case 11:
          this.AutoRecordGrid = (Grid) target;
          break;
        case 12:
          this.EachTimeText = (TextBlock) target;
          break;
        case 13:
          this.StepText = (TextBox) target;
          this.StepText.PreviewTextInput += new TextCompositionEventHandler(this.CheckInputValid);
          this.StepText.TextChanged += new TextChangedEventHandler(this.OnTimesOrStepTextChanged);
          this.StepText.LostFocus += new RoutedEventHandler(this.OnTextLostFocus);
          break;
        case 14:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveTimes);
          break;
        case 15:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSetTimesCancel);
          break;
        case 16:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 17:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
