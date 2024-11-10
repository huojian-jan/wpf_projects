// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitContainer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using ticktick_WPF.Notifier;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitContainer : Grid
  {
    private HabitDetailControl _habitDetail;
    private HabitListControl _habitList;
    private bool _hideDetail;
    private DetailHintPanel _hint;
    private string _selectedHabitId;

    public ColumnDefinition Column1 { get; set; }

    public ColumnDefinition Column2 { get; set; }

    public HabitContainer()
    {
      this.Column1 = new ColumnDefinition()
      {
        MinWidth = 340.0,
        Width = new GridLength(1.0, GridUnitType.Star)
      };
      this.ColumnDefinitions.Add(this.Column1);
      this.Column2 = new ColumnDefinition()
      {
        MinWidth = 400.0,
        Width = new GridLength(1.0, GridUnitType.Star)
      };
      this.ColumnDefinitions.Add(this.Column2);
      this.InitHabitControl();
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      DataChangedNotifier.HabitsChanged -= new EventHandler(this.OnHabitChanged);
      if (this._habitList != null)
      {
        this._habitList.HabitSelected -= new EventHandler<string>(this.OnHabitSelected);
        this._habitList.ItemsCountChanged -= new EventHandler<int>(this.OnHabitItemsCountChanged);
      }
      this.SizeChanged -= new SizeChangedEventHandler(this.OnSizeChanged);
      this.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnMouseUp);
      this.Children.Clear();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size size = e.PreviousSize;
      double width1 = size.Width;
      size = e.NewSize;
      double width2 = size.Width;
      this.SetWindowSize(width1, width2);
    }

    private void SetWindowSize(double preview, double newWidth)
    {
      if (newWidth >= 800.0)
      {
        this._hideDetail = false;
        if (preview < 800.0)
        {
          this.Column2.MinWidth = 400.0;
          this.Column2.Width = new GridLength(1.0, GridUnitType.Star);
          this._habitDetail.SetValue(Grid.ColumnProperty, (object) 1);
          this._habitDetail.SetValue(Panel.ZIndexProperty, (object) 0);
          this._habitDetail.BackGrid.Visibility = Visibility.Collapsed;
          TranslateTransform renderTransform = (TranslateTransform) this._habitDetail.RenderTransform;
          renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
          renderTransform.X = 0.0;
          this._habitDetail.Width = double.NaN;
          this._habitDetail.HorizontalAlignment = HorizontalAlignment.Stretch;
          this._habitDetail.Background = (Brush) Brushes.Transparent;
          this._habitDetail.Effect = (Effect) null;
        }
        double actualWidth1 = this.ActualWidth;
        double actualWidth2 = this._habitDetail.ActualWidth;
        double num1;
        if (actualWidth2 == 0.0)
        {
          num1 = 1.0;
        }
        else
        {
          double num2 = actualWidth2 < 342.0 ? 342.0 : actualWidth2;
          num1 = num2 / (actualWidth1 - num2);
        }
        this.Column1.Width = new GridLength(1.0, GridUnitType.Star);
        this.Column2.Width = new GridLength(num1 > 0.0 ? num1 : 1.0, GridUnitType.Star);
        this._habitDetail.Background = (Brush) Brushes.Transparent;
      }
      else
      {
        this._hideDetail = true;
        if (preview >= 800.0)
        {
          this.Column2.MinWidth = 0.0;
          this.Column2.Width = new GridLength(0.0);
          this._habitDetail.SetValue(Grid.ColumnProperty, (object) 0);
          this._habitDetail.SetValue(Panel.ZIndexProperty, (object) 10);
          this._habitDetail.BackGrid.Visibility = Visibility.Visible;
          this._habitDetail.SetResourceReference(Panel.BackgroundProperty, (object) "FoldAreaBackground");
          this._habitDetail.Effect = (Effect) new DropShadowEffect()
          {
            BlurRadius = 10.0,
            Direction = 180.0,
            Opacity = 0.08
          };
          this._habitDetail.RenderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) this.GetAnimation(0.0, 400.0));
          this._habitDetail.Width = 400.0;
          this._habitDetail.HorizontalAlignment = HorizontalAlignment.Right;
        }
        this.TryHideDetail();
      }
    }

    private DoubleAnimation GetAnimation(double from, double to)
    {
      DoubleAnimation animation = new DoubleAnimation();
      animation.Duration = new Duration(TimeSpan.FromMilliseconds(180.0));
      animation.From = new double?(from);
      animation.To = new double?(to);
      return animation;
    }

    private void InitHabitControl()
    {
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
      DataChangedNotifier.HabitsChanged += new EventHandler(this.OnHabitChanged);
      this._habitList = new HabitListControl();
      this._habitList.HabitSelected += new EventHandler<string>(this.OnHabitSelected);
      this._habitList.ItemsCountChanged += new EventHandler<int>(this.OnHabitItemsCountChanged);
      this.SetResourceReference(Panel.BackgroundProperty, (object) "ShowAreaBackground");
      this.Children.Add((UIElement) this._habitList);
      HabitDetailControl habitDetailControl = new HabitDetailControl();
      habitDetailControl.IsIndependent = true;
      habitDetailControl.Scroller.Margin = new Thickness(0.0, 30.0, 0.0, 0.0);
      this._habitDetail = habitDetailControl;
      this.Children.Add((UIElement) this._habitDetail);
      this._habitDetail.SetValue(Grid.ColumnProperty, (object) 1);
      this._habitDetail.Visibility = Visibility.Collapsed;
      this._habitDetail.RenderTransform = (Transform) new TranslateTransform()
      {
        X = 0.0
      };
      this.Column2.Width = new GridLength(1.0, GridUnitType.Star);
      this.Column2.MinWidth = 340.0;
      Border border = new Border();
      border.Width = 5.0;
      border.BorderThickness = new Thickness(1.0, 0.0, 0.0, 0.0);
      border.HorizontalAlignment = HorizontalAlignment.Left;
      Border element1 = border;
      element1.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity10");
      this.Children.Add((UIElement) element1);
      element1.SetValue(Grid.ColumnProperty, (object) 1);
      GridSplitter gridSplitter = new GridSplitter();
      gridSplitter.Width = 5.0;
      gridSplitter.Background = (Brush) Brushes.Transparent;
      gridSplitter.IsTabStop = false;
      gridSplitter.HorizontalAlignment = HorizontalAlignment.Left;
      gridSplitter.FocusVisualStyle = (Style) null;
      GridSplitter element2 = gridSplitter;
      this.Children.Add((UIElement) element2);
      element2.SetValue(Grid.ColumnProperty, (object) 1);
      this._hint = new DetailHintPanel(false, false);
      this.Children.Add((UIElement) this._hint);
      this._hint.SetValue(Grid.ColumnProperty, (object) 1);
      this.SetWindowSize(810.0, this.Width);
    }

    private void OnHabitItemsCountChanged(object sender, int e)
    {
      this._hint.Visibility = e == 0 || this._habitDetail.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void OnHabitChanged(object sender, EventArgs e)
    {
      HabitContainer habitContainer = this;
      // ISSUE: reference to a compiler-generated method
      habitContainer.Dispatcher.Invoke<Task>(new Func<Task>(habitContainer.\u003COnHabitChanged\u003Eb__20_0));
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this._habitDetail != null && this._habitDetail.IsMouseOver || this._habitList.IsMouseOver)
        return;
      this.TryHideDetail();
    }

    private void OnHabitSelected(object sender, string habitId)
    {
      if (this._hideDetail)
      {
        TranslateTransform renderTransform = (TranslateTransform) this._habitDetail.RenderTransform;
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) this.GetAnimation(renderTransform.X, 0.0));
      }
      this._selectedHabitId = habitId;
      if (string.IsNullOrEmpty(habitId))
      {
        if (this._hideDetail)
        {
          this.TryHideDetail();
        }
        else
        {
          this._habitDetail.Visibility = Visibility.Collapsed;
          this._hint.Visibility = Visibility.Visible;
        }
      }
      else
      {
        this._habitDetail.Visibility = Visibility.Visible;
        this._hint.Visibility = Visibility.Collapsed;
        this._habitDetail.Load(habitId, DateTime.Today, true);
        this._habitDetail.ScrollToTop();
      }
      if (sender != null)
        return;
      this._habitList.TrySelectItem(habitId);
    }

    public void TryHideDetail()
    {
      if (!this._hideDetail)
        return;
      TranslateTransform renderTransform = (TranslateTransform) this._habitDetail.RenderTransform;
      renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) this.GetAnimation(renderTransform.X, 400.0));
    }

    public void NavigateItem(string habitId) => this.OnHabitSelected((object) null, habitId);

    public void CheckInHabit(string habitId) => this._habitList.CheckInHabit(habitId);

    public void HideDetail()
    {
      this._habitDetail.Visibility = Visibility.Collapsed;
      this._hint.Visibility = Visibility.Visible;
    }

    public void LoadHabits() => this._habitList.GetHabitModels();

    public string GetSelectedHabitId() => this._habitDetail.GetHabitId();

    public void OnWindowMouseDown(MouseButtonEventArgs e)
    {
      if (this._habitDetail.IsMouseOver)
        return;
      HabitDetailControl habitDetail = this._habitDetail;
      if ((habitDetail != null ? (habitDetail.MorePopup.IsOpen ? 1 : 0) : 1) != 0 || e.GetPosition((IInputElement) this).X > this.Width - this._habitDetail.ActualWidth || this._habitList.HabitItems.IsMouseOver)
        return;
      this.TryHideDetail();
    }
  }
}
