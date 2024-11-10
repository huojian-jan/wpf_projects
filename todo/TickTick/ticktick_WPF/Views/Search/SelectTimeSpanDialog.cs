// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SelectTimeSpanDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SelectTimeSpanDialog : UserControl, IComponentConnector
  {
    private readonly SelectTimeSpanViewModel _viewModel;
    internal Grid TimeSpanGrid;
    internal TextBox StartDate;
    internal TextBox EndDate;
    internal TextBox FocusBox;
    internal Button SaveBtn;
    internal Button CancelBtn;
    internal TickDatePicker StartDateCalendar;
    internal TickDatePicker EndDateCalendar;
    private bool _contentLoaded;

    public event EventHandler<SelectTimeSpanViewModel> OnSpanSelect;

    public event EventHandler Cancel;

    public event EventHandler OnEndEdit;

    public SelectTimeSpanDialog()
      : this(new SelectTimeSpanViewModel())
    {
    }

    private SelectTimeSpanDialog(SelectTimeSpanViewModel viewModel)
    {
      this.InitializeComponent();
      this._viewModel = viewModel;
      this.DataContext = (object) this._viewModel;
      this.StartDateCalendar.SelectedDate = this._viewModel.StartDate;
      this.EndDateCalendar.SelectedDate = this._viewModel.EndDate;
      this.StartDateCalendar.SelectedDateChanged += (EventHandler<DateTime>) ((sender, date) =>
      {
        this._viewModel.StartDate = this.StartDateCalendar.SelectedDate;
        this.StartDateCalendar.Visibility = Visibility.Collapsed;
        this.TimeSpanGrid.Visibility = Visibility.Visible;
        this.CheckValidTimeSpan();
        EventHandler onEndEdit = this.OnEndEdit;
        if (onEndEdit == null)
          return;
        onEndEdit((object) this, (EventArgs) null);
      });
      this.EndDateCalendar.SelectedDateChanged += (EventHandler<DateTime>) ((sender, date) =>
      {
        this._viewModel.EndDate = this.EndDateCalendar.SelectedDate;
        this.EndDateCalendar.Visibility = Visibility.Collapsed;
        this.TimeSpanGrid.Visibility = Visibility.Visible;
        this.CheckValidTimeSpan();
        EventHandler onEndEdit = this.OnEndEdit;
        if (onEndEdit == null)
          return;
        onEndEdit((object) this, (EventArgs) null);
      });
    }

    public void SetStartDate(DateTime? startDate) => this._viewModel.StartDate = startDate;

    private void CheckValidTimeSpan(bool needToast = true)
    {
      if (!Utils.IsEmptyDate(this._viewModel.StartDate) && !Utils.IsEmptyDate(this._viewModel.EndDate))
      {
        DateTime? nullable = this._viewModel.StartDate;
        if (nullable.HasValue)
        {
          nullable = this._viewModel.EndDate;
          if (nullable.HasValue)
          {
            nullable = this._viewModel.StartDate;
            DateTime t1 = nullable.Value;
            nullable = this._viewModel.EndDate;
            DateTime t2 = nullable.Value;
            if (DateTime.Compare(t1, t2) > 0)
            {
              if (needToast)
                Utils.Toast(Utils.GetString("InvalidTimeSpan"));
            }
            else
            {
              this.SaveBtn.IsEnabled = true;
              return;
            }
          }
        }
      }
      this.SaveBtn.IsEnabled = false;
    }

    public void SetEndDate(DateTime? endDate)
    {
      this._viewModel.EndDate = endDate;
      this.CheckValidTimeSpan(false);
    }

    private void SaveBtnClick(object sender, RoutedEventArgs e)
    {
      EventHandler<SelectTimeSpanViewModel> onSpanSelect = this.OnSpanSelect;
      if (onSpanSelect != null)
        onSpanSelect((object) this, this._viewModel);
      e.Handled = true;
    }

    private void CancelBtnClick(object sender, RoutedEventArgs e)
    {
      EventHandler cancel = this.Cancel;
      if (cancel != null)
        cancel((object) this, (EventArgs) null);
      e.Handled = true;
    }

    private void StartDateGotFocus(object sender, RoutedEventArgs e)
    {
      if (this._viewModel.StartDate.HasValue)
        this.StartDateCalendar.SelectedDate = new DateTime?(this._viewModel.StartDate.Value);
      this.StartDateCalendar.Visibility = this.StartDateCalendar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
      this.EndDateCalendar.Visibility = Visibility.Collapsed;
    }

    private void EndDateGotFocus(object sender, RoutedEventArgs e)
    {
      if (this._viewModel.EndDate.HasValue)
        this.EndDateCalendar.SelectedDate = new DateTime?(this._viewModel.EndDate.Value);
      this.EndDateCalendar.Visibility = this.EndDateCalendar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
      this.StartDateCalendar.Visibility = Visibility.Collapsed;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (!this.SaveBtn.IsEnabled)
            break;
          EventHandler<SelectTimeSpanViewModel> onSpanSelect = this.OnSpanSelect;
          if (onSpanSelect == null)
            break;
          onSpanSelect((object) this, this._viewModel);
          break;
        case Key.Escape:
          EventHandler cancel = this.Cancel;
          if (cancel == null)
            break;
          cancel((object) this, (EventArgs) null);
          break;
      }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e) => this.FocusBox.Focus();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/selecttimespandialog.xaml", UriKind.Relative));
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
          ((UIElement) target).PreviewKeyUp += new KeyEventHandler(this.OnKeyUp);
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
          break;
        case 2:
          this.TimeSpanGrid = (Grid) target;
          break;
        case 3:
          this.StartDate = (TextBox) target;
          break;
        case 4:
          this.EndDate = (TextBox) target;
          break;
        case 5:
          this.FocusBox = (TextBox) target;
          this.FocusBox.PreviewKeyUp += new KeyEventHandler(this.OnKeyUp);
          break;
        case 6:
          ((UIElement) target).PreviewMouseDown += new MouseButtonEventHandler(this.StartDateGotFocus);
          break;
        case 7:
          ((UIElement) target).PreviewMouseDown += new MouseButtonEventHandler(this.EndDateGotFocus);
          break;
        case 8:
          this.SaveBtn = (Button) target;
          this.SaveBtn.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.SaveBtnClick);
          break;
        case 9:
          this.CancelBtn = (Button) target;
          this.CancelBtn.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.CancelBtnClick);
          break;
        case 10:
          this.StartDateCalendar = (TickDatePicker) target;
          break;
        case 11:
          this.EndDateCalendar = (TickDatePicker) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
