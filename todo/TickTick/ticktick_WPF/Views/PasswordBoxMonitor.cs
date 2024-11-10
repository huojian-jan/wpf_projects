// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.PasswordBoxMonitor
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views
{
  public class PasswordBoxMonitor : DependencyObject
  {
    public static readonly DependencyProperty IsMonitoringProperty = DependencyProperty.RegisterAttached("IsMonitoring", typeof (bool), typeof (PasswordBoxMonitor), (PropertyMetadata) new UIPropertyMetadata((object) false, new PropertyChangedCallback(PasswordBoxMonitor.OnIsMonitoringChanged)));
    public static readonly DependencyProperty PasswordLengthProperty = DependencyProperty.RegisterAttached("PasswordLength", typeof (int), typeof (PasswordBoxMonitor), (PropertyMetadata) new UIPropertyMetadata((object) 0));

    public static bool GetIsMonitoring(DependencyObject obj)
    {
      return (bool) obj.GetValue(PasswordBoxMonitor.IsMonitoringProperty);
    }

    public static void SetIsMonitoring(DependencyObject obj, bool value)
    {
      obj.SetValue(PasswordBoxMonitor.IsMonitoringProperty, (object) value);
    }

    public static int GetPasswordLength(DependencyObject obj)
    {
      return (int) obj.GetValue(PasswordBoxMonitor.PasswordLengthProperty);
    }

    public static void SetPasswordLength(DependencyObject obj, int value)
    {
      obj.SetValue(PasswordBoxMonitor.PasswordLengthProperty, (object) value);
    }

    private static void OnIsMonitoringChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is PasswordBox passwordBox))
        return;
      if ((bool) e.NewValue)
        passwordBox.PasswordChanged += new RoutedEventHandler(PasswordBoxMonitor.PasswordChanged);
      else
        passwordBox.PasswordChanged -= new RoutedEventHandler(PasswordBoxMonitor.PasswordChanged);
    }

    private static void PasswordChanged(object sender, RoutedEventArgs e)
    {
      if (!(sender is PasswordBox passwordBox))
        return;
      PasswordBoxMonitor.SetPasswordLength((DependencyObject) passwordBox, passwordBox.Password.Length);
    }
  }
}
