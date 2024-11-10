// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.WindowToastHelper
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
using ticktick_WPF.Views;
using ticktick_WPF.Views.Undo;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class WindowToastHelper
  {
    public static async Task ToastString(
      Grid grid,
      string text,
      double maxWidth = 600.0,
      double height = 40.0,
      double fontSize = 13.0)
    {
      Application.Current?.Dispatcher?.Invoke<Task>((Func<Task>) (async () =>
      {
        Border uiElement = new Border()
        {
          Background = (Brush) ThemeUtil.GetColor("ToastBackground"),
          CornerRadius = new CornerRadius(4.0),
          Height = height
        };
        uiElement.Child = (UIElement) new TextBlock()
        {
          Text = text,
          TextAlignment = TextAlignment.Center,
          VerticalAlignment = VerticalAlignment.Center,
          HorizontalAlignment = HorizontalAlignment.Center,
          Padding = new Thickness(fontSize, 0.0, fontSize, 0.0),
          Foreground = (Brush) Brushes.White,
          MaxWidth = maxWidth,
          FontSize = fontSize,
          TextTrimming = TextTrimming.CharacterEllipsis
        };
        await WindowToastHelper.ShowAndHideToast(grid, (FrameworkElement) uiElement);
      }));
    }

    public static async Task ShowAndHideToast(Grid grid, FrameworkElement uiElement)
    {
      Application.Current?.Dispatcher?.Invoke<Task>((Func<Task>) (async () =>
      {
        if (grid == null)
          grid = App.Window.ToastGrid;
        try
        {
          uiElement.VerticalAlignment = VerticalAlignment.Bottom;
          if (grid.Children.Count > 0)
          {
            foreach (object child in grid.Children)
            {
              if (child is UndoToast undoToast4)
                undoToast4.OnFinished();
            }
          }
          grid.Children.Clear();
          grid.Children.Add((UIElement) uiElement);
          WindowToastHelper.GetShowStory().Begin(uiElement);
          Storyboard hideStory = new Storyboard();
          DoubleAnimation element = new DoubleAnimation()
          {
            BeginTime = new TimeSpan?(TimeSpan.FromSeconds(3.0)),
            Duration = new Duration(TimeSpan.FromMilliseconds(200.0)),
            From = new double?(1.0),
            To = new double?(0.0)
          };
          hideStory.Children.Add((Timeline) element);
          Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath((object) UIElement.OpacityProperty));
          Storyboard.SetTarget((DependencyObject) element, (DependencyObject) uiElement);
          uiElement.MouseEnter += new MouseEventHandler(MouseEnter);
          // ISSUE: variable of a compiler-generated type
          WindowToastHelper.\u003C\u003Ec__DisplayClass1_1 cDisplayClass11;
          hideStory.Completed += (EventHandler) (async (a, b) =>
          {
            // ISSUE: method pointer
            uiElement.MouseEnter -= new MouseEventHandler((object) cDisplayClass11, __methodptr(\u003CShowAndHideToast\u003Eg__MouseEnter\u007C1));
            // ISSUE: method pointer
            uiElement.MouseLeave -= new MouseEventHandler((object) cDisplayClass11, __methodptr(\u003CShowAndHideToast\u003Eg__MouseLeave\u007C2));
            if (grid.Children.Contains((UIElement) uiElement))
              grid.Children.Remove((UIElement) uiElement);
            await Task.Delay(300);
            if (uiElement is UndoToast undoToast3)
              undoToast3.OnFinished();
            if (!(uiElement is MoveToastControl))
              return;
            TaskDragUndoModel.TryFinishAll();
          });
          hideStory.Begin();

          void MouseEnter(object sender, MouseEventArgs e)
          {
            hideStory.Pause();
            uiElement.MouseEnter -= new MouseEventHandler(MouseEnter);
            uiElement.MouseLeave += new MouseEventHandler(MouseLeave);
          }

          void MouseLeave(object sender, MouseEventArgs e)
          {
            hideStory.Resume();
            uiElement.MouseEnter += new MouseEventHandler(MouseEnter);
            uiElement.MouseLeave -= new MouseEventHandler(MouseLeave);
          }
        }
        catch (Exception ex)
        {
          int num = (int) MessageBox.Show(ExceptionUtils.BuildExceptionMessage(ex));
        }
      }));
    }

    private static Storyboard GetShowStory()
    {
      Storyboard showStory = new Storyboard();
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.BeginTime = new TimeSpan?(new TimeSpan(0L));
      doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200.0));
      doubleAnimation.From = new double?(0.0);
      doubleAnimation.To = new double?(1.0);
      DoubleAnimation element = doubleAnimation;
      showStory.Children.Add((Timeline) element);
      Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath((object) UIElement.OpacityProperty));
      return showStory;
    }
  }
}
