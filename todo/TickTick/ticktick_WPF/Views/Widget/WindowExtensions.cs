// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WindowExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public static class WindowExtensions
  {
    public static readonly DependencyProperty SinkerProperty = DependencyProperty.RegisterAttached("Sinker", typeof (WindowSinker), typeof (WindowExtensions), (PropertyMetadata) new UIPropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty AlwaysOnBottomProperty = DependencyProperty.RegisterAttached("AlwaysOnBottom", typeof (bool), typeof (WindowExtensions), (PropertyMetadata) new UIPropertyMetadata((object) false, new PropertyChangedCallback(WindowExtensions.OnAlwaysOnBottomChanged)));

    public static WindowSinker GetSinker(DependencyObject obj)
    {
      return (WindowSinker) obj.GetValue(WindowExtensions.SinkerProperty);
    }

    public static void SetSinker(DependencyObject obj, WindowSinker value)
    {
      obj.SetValue(WindowExtensions.SinkerProperty, (object) value);
    }

    public static bool GetAlwaysOnBottom(DependencyObject obj)
    {
      return (bool) obj.GetValue(WindowExtensions.AlwaysOnBottomProperty);
    }

    public static void SetAlwaysOnBottom(DependencyObject obj, bool value)
    {
      obj.SetValue(WindowExtensions.AlwaysOnBottomProperty, (object) value);
    }

    private static void OnAlwaysOnBottomChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is Window Window))
        return;
      if ((bool) e.NewValue)
      {
        WindowSinker windowSinker = new WindowSinker(Window);
        windowSinker.Sink();
        WindowExtensions.SetSinker((DependencyObject) Window, windowSinker);
      }
      else
      {
        WindowExtensions.GetSinker((DependencyObject) Window).Unsink();
        WindowExtensions.SetSinker((DependencyObject) Window, (WindowSinker) null);
      }
    }
  }
}
