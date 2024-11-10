// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusRecordItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusRecordItem : Border
  {
    private TextBlock _dateText;
    private Grid _recordGrid;
    private Path _icon;
    private static Geometry _pomoData = Utils.GetIcon("IcPomo");
    private static Geometry _timerData = Utils.GetIcon("IcPomoTimer");
    private TextBlock _timeText;
    private TextBlock _durationText;
    private EmjTextBlock _noteText;
    private DockPanel _lineStack;
    private StackPanel _titleStack;
    private Border _hoverBorder;

    public FocusRecordItem()
    {
      this.Background = (Brush) Brushes.Transparent;
      this.MinHeight = 48.0;
      this.Margin = new Thickness(20.0, 0.0, 20.0, 0.0);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is FocusRecordItemViewModel newValue))
        return;
      this._lineStack?.Children.Clear();
      this._titleStack?.Children.Clear();
      if (!string.IsNullOrEmpty(newValue.DateText))
      {
        if (this._dateText == null)
        {
          TextBlock textBlock = new TextBlock();
          textBlock.FontSize = 14.0;
          textBlock.FontWeight = FontWeights.Bold;
          textBlock.VerticalAlignment = VerticalAlignment.Center;
          this._dateText = textBlock;
          this._dateText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
          this._dateText.SetBinding(TextBlock.TextProperty, "DateText");
        }
        this.Child = (UIElement) this._dateText;
      }
      else
      {
        if (this._recordGrid == null)
        {
          this._recordGrid = new Grid();
          this._recordGrid.RowDefinitions.Add(new RowDefinition()
          {
            Height = new GridLength(24.0)
          });
          this._recordGrid.RowDefinitions.Add(new RowDefinition()
          {
            MinHeight = 24.0
          });
          this._recordGrid.ColumnDefinitions.Add(new ColumnDefinition()
          {
            Width = new GridLength(24.0)
          });
          this._recordGrid.ColumnDefinitions.Add(new ColumnDefinition());
          Border border1 = new Border();
          border1.Width = 24.0;
          border1.Height = 24.0;
          border1.CornerRadius = new CornerRadius(12.0);
          border1.HorizontalAlignment = HorizontalAlignment.Left;
          Border element = border1;
          element.SetResourceReference(Border.BackgroundProperty, (object) "PrimaryColor10");
          Path path = new Path();
          path.Width = 14.0;
          path.Height = 14.0;
          path.Stretch = Stretch.Uniform;
          this._icon = path;
          this._icon.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor");
          element.Child = (UIElement) this._icon;
          Border border2 = new Border();
          border2.CornerRadius = new CornerRadius(4.0);
          border2.Margin = new Thickness(4.0, 0.0, -4.0, 4.0);
          border2.Background = (Brush) Brushes.Transparent;
          this._hoverBorder = border2;
          this._hoverBorder.SetValue(Grid.RowSpanProperty, (object) 2);
          this._hoverBorder.SetValue(Grid.ColumnProperty, (object) 1);
          this.BindBorderEvent(this._hoverBorder);
          TextBlock textBlock1 = new TextBlock();
          textBlock1.FontSize = 12.0;
          textBlock1.Margin = new Thickness(8.0, 0.0, 0.0, 0.0);
          textBlock1.VerticalAlignment = VerticalAlignment.Center;
          textBlock1.HorizontalAlignment = HorizontalAlignment.Left;
          textBlock1.IsHitTestVisible = false;
          this._timeText = textBlock1;
          this._timeText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
          this._timeText.SetValue(Grid.ColumnProperty, (object) 1);
          this._timeText.SetBinding(TextBlock.TextProperty, "TimeText");
          TextBlock textBlock2 = new TextBlock();
          textBlock2.FontSize = 12.0;
          textBlock2.VerticalAlignment = VerticalAlignment.Center;
          textBlock2.HorizontalAlignment = HorizontalAlignment.Right;
          textBlock2.IsHitTestVisible = false;
          this._durationText = textBlock2;
          this._durationText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity40");
          this._durationText.SetValue(Grid.ColumnProperty, (object) 1);
          this._durationText.SetBinding(TextBlock.TextProperty, "DurationText");
          this._recordGrid.Children.Add((UIElement) element);
          this._recordGrid.Children.Add((UIElement) this._hoverBorder);
          this._recordGrid.Children.Add((UIElement) this._timeText);
          this._recordGrid.Children.Add((UIElement) this._durationText);
          EmjTextBlock emjTextBlock = new EmjTextBlock();
          emjTextBlock.TextWrapping = TextWrapping.Wrap;
          emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
          emjTextBlock.Height = 18.0;
          emjTextBlock.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
          emjTextBlock.ClipToBounds = true;
          emjTextBlock.IsHitTestVisible = false;
          this._noteText = emjTextBlock;
          this._noteText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
          DockPanel dockPanel = new DockPanel();
          dockPanel.VerticalAlignment = VerticalAlignment.Stretch;
          this._lineStack = dockPanel;
          this._lineStack.SetValue(Grid.RowProperty, (object) 1);
          StackPanel stackPanel = new StackPanel();
          stackPanel.IsHitTestVisible = false;
          this._titleStack = stackPanel;
          this._titleStack.SetValue(Grid.RowProperty, (object) 1);
          this._titleStack.SetValue(Grid.ColumnProperty, (object) 1);
          this._titleStack.Margin = new Thickness(8.0, 0.0, 0.0, 8.0);
          this._recordGrid.Children.Add((UIElement) this._lineStack);
          this._recordGrid.Children.Add((UIElement) this._titleStack);
        }
        List<string> titles = newValue.Titles;
        // ISSUE: explicit non-virtual call
        bool flag = (titles != null ? (__nonvirtual (titles.Count) > 0 ? 1 : 0) : 0) != 0 || !string.IsNullOrWhiteSpace(newValue.Note);
        this._hoverBorder.Margin = new Thickness(4.0, 0.0, -4.0, !flag || !newValue.IsLastItem ? (flag ? 20.0 : 24.0) : 8.0);
        this.Child = (UIElement) this._recordGrid;
        this._icon.Data = newValue.Type == 0 ? FocusRecordItem._pomoData : FocusRecordItem._timerData;
        this.SetLines(newValue);
        this.SetTitles(newValue);
      }
    }

    private void BindBorderEvent(Border border)
    {
      border.Cursor = Cursors.Hand;
      border.MouseLeave += (MouseEventHandler) ((o, e) => border.Background = (Brush) Brushes.Transparent);
      border.MouseEnter += (MouseEventHandler) ((o, e) => border.SetResourceReference(Border.BackgroundProperty, (object) "BaseColorOpacity5"));
      border.MouseLeftButtonUp += (MouseButtonEventHandler) (async (o, e) =>
      {
        FocusRecordItemViewModel vModel = this.DataContext as FocusRecordItemViewModel;
        if (vModel == null)
          ;
        else
        {
          UserActCollectUtils.AddClickEvent("focus", "focus_tab", "focus_record");
          PomodoroModel pomo = await PomoDao.GetPomoById(vModel.FocusId);
          if (pomo == null)
            ;
          else
          {
            PomodoroModel pomodoroModel = pomo;
            pomodoroModel.Tasks = await PomoTaskDao.GetByPomoId(pomo.Id);
            pomodoroModel = (PomodoroModel) null;
            PomoDetailWindow pomoDetailWindow = new PomoDetailWindow(new PomoDisplayViewModel(pomo, pomo.Tasks), (CalendarDisplayViewModel) null);
            pomoDetailWindow.SetCustomStyle();
            pomoDetailWindow.Closed += (EventHandler) (async (obj, args) =>
            {
              await Task.Delay(200);
              await vModel.ResetTitle();
              this.ResetLineAndTitle(vModel);
            });
            pomoDetailWindow.Show((UIElement) border, border.Width, true);
            pomo = (PomodoroModel) null;
          }
        }
      });
    }

    private void ResetLineAndTitle(FocusRecordItemViewModel vModel)
    {
      if (!(this.DataContext is FocusRecordItemViewModel dataContext) || !(dataContext.FocusId == vModel.FocusId))
        return;
      this._lineStack?.Children.Clear();
      this._titleStack?.Children.Clear();
      this.SetLines(vModel);
      this.SetTitles(vModel);
    }

    private void SetTitles(FocusRecordItemViewModel model)
    {
      List<string> titles1 = model.Titles;
      // ISSUE: explicit non-virtual call
      if ((titles1 != null ? (__nonvirtual (titles1.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        foreach (string title in model.Titles)
        {
          EmjTextBlock emjTextBlock = new EmjTextBlock();
          emjTextBlock.Text = title;
          emjTextBlock.FontSize = 14.0;
          emjTextBlock.TextWrapping = TextWrapping.NoWrap;
          emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
          emjTextBlock.Margin = new Thickness(0.0, 4.0, 0.0, 4.0);
          EmjTextBlock element = emjTextBlock;
          element.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
          this._titleStack.Children.Add((UIElement) element);
        }
      }
      if (!string.IsNullOrWhiteSpace(model.Note))
      {
        this._noteText.Text = model.Note;
        this._titleStack.Children.Add((UIElement) this._noteText);
      }
      List<string> titles2 = model.Titles;
      // ISSUE: explicit non-virtual call
      this._titleStack.Margin = new Thickness(8.0, 0.0, 0.0, (titles2 != null ? (__nonvirtual (titles2.Count) > 0 ? 1 : 0) : 0) == 0 && string.IsNullOrWhiteSpace(model.Note) ? 0.0 : (model.IsLastItem ? 8.0 : 20.0));
    }

    private void SetLines(FocusRecordItemViewModel model)
    {
      if (model.IsLastItem)
      {
        List<string> titles = model.Titles;
        // ISSUE: explicit non-virtual call
        if ((titles != null ? (__nonvirtual (titles.Count) > 0 ? 1 : 0) : 0) == 0)
          return;
      }
      List<string> titles1 = model.Titles;
      // ISSUE: explicit non-virtual call
      if ((titles1 != null ? (__nonvirtual (titles1.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        for (int index = 0; index < model.Titles.Count; ++index)
        {
          Rectangle rect = GetRect();
          if (index == 0)
          {
            rect.Height = 7.0;
            rect.Margin = new Thickness(0.0, 4.0, 0.0, 0.0);
          }
          this._lineStack.Children.Add((UIElement) rect);
          this._lineStack.Children.Add((UIElement) GetEllipse());
        }
      }
      Rectangle rectangle1 = new Rectangle();
      rectangle1.Width = 2.0;
      Rectangle element = rectangle1;
      Rectangle rectangle2 = element;
      List<string> titles2 = model.Titles;
      // ISSUE: explicit non-virtual call
      Thickness thickness = new Thickness(0.0, (titles2 != null ? (__nonvirtual (titles2.Count) > 0 ? 1 : 0) : 0) != 0 ? 0.0 : 4.0, 0.0, 4.0);
      rectangle2.Margin = thickness;
      if (!model.IsLastItem)
        element.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor10");
      this._lineStack.Children.Add((UIElement) element);

      static Rectangle GetRect()
      {
        Rectangle rect = new Rectangle();
        rect.Width = 2.0;
        rect.Height = 18.0;
        rect.SetValue(DockPanel.DockProperty, (object) Dock.Top);
        rect.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor10");
        return rect;
      }

      static Ellipse GetEllipse()
      {
        Ellipse ellipse = new Ellipse();
        ellipse.Width = 8.0;
        ellipse.Height = 8.0;
        ellipse.StrokeThickness = 1.0;
        ellipse.SetValue(DockPanel.DockProperty, (object) Dock.Top);
        ellipse.SetResourceReference(Shape.StrokeProperty, (object) "PrimaryColor");
        return ellipse;
      }
    }
  }
}
