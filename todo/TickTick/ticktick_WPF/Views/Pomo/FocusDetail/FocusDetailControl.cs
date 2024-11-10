// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusDetail.FocusDetailControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Pomo.FocusStatistics;
using ticktick_WPF.Views.Pomo.TimerDetail;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusDetail
{
  public sealed class FocusDetailControl : Border
  {
    private readonly StackPanel _focusContainer;
    private readonly FocusStatisticsPanel _statistics;
    private EmojiEditor _noteEditor;
    private FocusDetailTitle _focusDetailTitle;
    private FocusTimeline _timeline;
    private TextBlock _noteLengthText;
    private TimerDetailPanel _timerDetail;

    public FocusDetailControl()
    {
      StackPanel stackPanel = new StackPanel();
      stackPanel.Margin = new Thickness(20.0, 40.0, 20.0, 20.0);
      this._focusContainer = stackPanel;
      FocusStatisticsPanel focusStatisticsPanel = new FocusStatisticsPanel();
      focusStatisticsPanel.Margin = new Thickness(0.0, 25.0, 4.0, 20.0);
      this._statistics = focusStatisticsPanel;
      TimerDetailPanel timerDetailPanel = new TimerDetailPanel();
      timerDetailPanel.Margin = new Thickness(0.0, 25.0, 4.0, 20.0);
      this._timerDetail = timerDetailPanel;
      this.InitFocusChildren();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
    }

    private void InitFocusChildren()
    {
      this.InitTitle();
      this.InitTimeline();
      this.InitNote();
    }

    private void InitTitle()
    {
      FocusDetailTitle focusDetailTitle = new FocusDetailTitle();
      focusDetailTitle.Height = 50.0;
      focusDetailTitle.Margin = new Thickness(0.0, 0.0, 0.0, 20.0);
      this._focusDetailTitle = focusDetailTitle;
      this._focusContainer.Children.Add((UIElement) this._focusDetailTitle);
    }

    private void InitTimeline()
    {
      FocusTimeline focusTimeline = new FocusTimeline();
      focusTimeline.Margin = new Thickness(-20.0, 0.0, 0.0, 40.0);
      this._timeline = focusTimeline;
      this._focusContainer.Children.Add((UIElement) this._timeline);
    }

    private void InitNote()
    {
      Grid grid = new Grid();
      grid.Margin = new Thickness(0.0, 10.0, 0.0, 10.0);
      Grid element1 = grid;
      EmojiEditor emojiEditor = new EmojiEditor();
      emojiEditor.FontSize = 13.0;
      emojiEditor.MaxLength = 500;
      emojiEditor.BorderThickness = new Thickness(1.0);
      emojiEditor.WordWrap = true;
      emojiEditor.AcceptReturn = true;
      emojiEditor.Height = 120.0;
      emojiEditor.Background = (Brush) Brushes.Transparent;
      emojiEditor.BorderCorner = new CornerRadius(4.0);
      emojiEditor.TextVerticalAlignment = VerticalAlignment.Top;
      emojiEditor.Padding = new Thickness(12.0, 8.0, 12.0, 20.0);
      emojiEditor.Cursor = Cursors.IBeam;
      emojiEditor.Tag = (object) Utils.GetString("FocusNoteHint");
      emojiEditor.VerticalAlignment = VerticalAlignment.Top;
      emojiEditor.Text = TickFocusManager.Config.Note;
      this._noteEditor = emojiEditor;
      this._noteEditor.SetResourceReference(EmojiEditor.ForegroundProperty, (object) "BaseColorOpacity80");
      this._noteEditor.SetResourceReference(EmojiEditor.BorderBackgroundProperty, (object) "BaseColorOpacity5");
      this._noteEditor.SetResourceReference(EmojiEditor.BorderBrushProperty, (object) "BaseColorOpacity5");
      this._noteEditor.TextChanged += new EventHandler(this.OnNoteChanged);
      this._noteEditor.GotFocus += new RoutedEventHandler(this.OnNoteGotFocus);
      TextBlock textBlock1 = new TextBlock();
      textBlock1.Text = Utils.GetString("FocusNote");
      textBlock1.Style = (Style) this.FindResource((object) "Body01");
      textBlock1.FontSize = 16.0;
      TextBlock element2 = textBlock1;
      element2.SetBinding(UIElement.VisibilityProperty, (BindingBase) new Binding("Visibility")
      {
        Source = (object) this._noteEditor
      });
      this._focusContainer.Children.Add((UIElement) element2);
      element1.Children.Add((UIElement) this._noteEditor);
      TextBlock textBlock2 = new TextBlock();
      textBlock2.HorizontalAlignment = HorizontalAlignment.Right;
      textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
      textBlock2.FontSize = 12.0;
      textBlock2.Margin = new Thickness(0.0, 0.0, 20.0, 6.0);
      textBlock2.Foreground = (Brush) Brushes.Red;
      this._noteLengthText = textBlock2;
      this._noteLengthText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
      this._noteLengthText.SetBinding(UIElement.VisibilityProperty, (BindingBase) new Binding("Visibility")
      {
        Source = (object) this._noteEditor
      });
      element1.Children.Add((UIElement) this._noteLengthText);
      this._focusContainer.Children.Add((UIElement) element1);
      this._noteEditor.Visibility = TickFocusManager.Status == PomoStatus.WaitingWork ? Visibility.Hidden : Visibility.Visible;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      TickFocusManager.StatusChanged -= new FocusChange(this.OnStatusChanged);
      TickFocusManager.DurationChanged -= new FocusChange(this.OnDurationChanged);
      PomoNotifier.FocusingNoteChanged -= new EventHandler(this.OnFocusingNoteChanged);
      PomoNotifier.Changed -= new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnTitleChanged), "Title");
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusChanged), "NoTask");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.ShowStatisticsOrClock();
      TickFocusManager.StatusChanged += new FocusChange(this.OnStatusChanged);
      TickFocusManager.DurationChanged += new FocusChange(this.OnDurationChanged);
      PomoNotifier.FocusingNoteChanged += new EventHandler(this.OnFocusingNoteChanged);
      PomoNotifier.Changed += new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnTitleChanged), "Title");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusChanged), "NoTask");
    }

    private void OnFocusingNoteChanged(object sender, EventArgs e)
    {
      if (this._noteEditor == null || this._noteEditor.Equals(sender))
        return;
      this._noteEditor.TextChanged -= new EventHandler(this.OnNoteChanged);
      this._noteEditor.EditBox.SetTextAndOffset(TickFocusManager.Config.Note, false);
      this._noteEditor.TextChanged += new EventHandler(this.OnNoteChanged);
    }

    private void OnFocusChanged(object sender, PropertyChangedEventArgs e)
    {
      this._focusDetailTitle.SetFocusIcon();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (e.NewSize.Height <= 350.0)
        return;
      double val2 = e.NewSize.Height * 0.5;
      double val1 = e.NewSize.Height - 340.0;
      this._timeline?.SetSize(e.NewSize.Width, Math.Min(val1, Math.Max(260.0, val2)));
      this._noteEditor.Height = Math.Max(120.0, val2 * 0.25);
    }

    private void OnTitleChanged(object sender, PropertyChangedEventArgs e)
    {
      this._focusDetailTitle.SetTitle();
    }

    private void OnNoteChanged(object sender, EventArgs e)
    {
      this._noteLengthText.Text = this._noteEditor.Text.Length < 400 ? string.Empty : this._noteEditor.Text.Length.ToString() + "/500";
      TickFocusManager.Config.SetNote(sender, this._noteEditor.Text);
    }

    private void OnNoteGotFocus(object sender, RoutedEventArgs e)
    {
      UserActCollectUtils.AddClickEvent("focus", "add_focus_note", TickFocusManager.GetActCType());
    }

    private void OnDurationChanged() => this._timeline?.DrawCurrentFocus();

    private void OnStatusChanged()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (object.Equals((object) this.Child, (object) this._timerDetail))
          return;
        this._noteEditor.Visibility = TickFocusManager.Status == PomoStatus.WaitingWork ? Visibility.Hidden : Visibility.Visible;
        if (this._noteEditor.Text != TickFocusManager.Config.Note)
          this._noteEditor.Text = TickFocusManager.Config.Note;
        this.ShowStatisticsOrClock();
      }));
    }

    private void ShowStatisticsOrClock()
    {
      FocusView parent1 = Utils.FindParent<FocusView>((DependencyObject) this);
      if ((parent1 != null ? (parent1.GetClockPanelShow() ? 1 : 0) : 1) == 0)
      {
        if (object.Equals((object) this.Child, (object) this._statistics))
          return;
        this.Child = (UIElement) this._statistics;
        this._statistics?.Reload();
      }
      else
      {
        if (TickFocusManager.Status == PomoStatus.WaitingWork)
        {
          FocusView parent2 = Utils.FindParent<FocusView>((DependencyObject) this);
          if ((parent2 != null ? (parent2.ExistTimer() ? 1 : 0) : 1) == 0)
          {
            this.Child = (UIElement) this._statistics;
            this._statistics?.Reload();
            return;
          }
        }
        this.Child = (UIElement) this._focusContainer;
        this._timeline?.DrawCurrentFocus();
        this._focusDetailTitle.SetIcon();
        this._focusDetailTitle.SetFocusIcon();
        this._focusDetailTitle.SetTitle();
      }
    }

    private void OnPomoChanged(object sender, PomoChangeArgs e)
    {
      if (this.Child == this._statistics)
        this._statistics?.Reload();
      if (this.Child != this._focusContainer)
        return;
      this._timeline?.DrawCurrentFocus();
    }

    public void ReloadStatistics()
    {
      if (this.Child != this._statistics)
        return;
      this._statistics?.Reload();
    }

    public async Task OnTimerSelect(string timerId)
    {
      FocusDetailControl focusDetailControl = this;
      TimerModel timerModel;
      if (string.IsNullOrEmpty(timerId))
        timerModel = (TimerModel) null;
      else
        timerModel = await TimerDao.GetTimerById(timerId);
      TimerModel timer = timerModel;
      if (timer != null)
      {
        focusDetailControl.Child = (UIElement) focusDetailControl._timerDetail;
        UserActCollectUtils.AddClickEvent("timer", "timer_detail", "show");
        focusDetailControl._timerDetail.LoadTimer(timer);
      }
      else
        focusDetailControl.ShowStatisticsOrClock();
    }

    public void SetFoldStyle(bool fold)
    {
      if (fold)
      {
        this.HorizontalAlignment = HorizontalAlignment.Right;
        this.Width = 400.0;
        this.SetResourceReference(Border.BackgroundProperty, (object) "FoldAreaBackground");
        this.RenderTransform = (Transform) new TranslateTransform()
        {
          X = 400.0
        };
        this.Effect = (Effect) new DropShadowEffect()
        {
          BlurRadius = 10.0,
          Direction = 180.0,
          Opacity = 0.08
        };
        this.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.HorizontalAlignment = HorizontalAlignment.Stretch;
        this.Width = double.NaN;
        this.Background = (Brush) Brushes.Transparent;
        this.RenderTransform = (Transform) null;
        this.Visibility = Visibility.Visible;
        this.Effect = (Effect) null;
      }
    }
  }
}
