// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Notify
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Media;
using System.Windows;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  internal static class Notify
  {
    public static void Alert(string alert, Window owner = null)
    {
      int num = (int) Notify.Show(alert, MessageBoxButton.OK, MessageBoxImage.Hand, owner);
    }

    public static MessageBoxResult ConfirmYesNo(string question, Window owner = null)
    {
      return Notify.Show(question, MessageBoxButton.YesNo, MessageBoxImage.Question, owner);
    }

    public static MessageBoxResult ConfirmYesNoCancel(string question, Window owner = null)
    {
      return Notify.Show(question, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, owner);
    }

    private static MessageBoxResult Show(
      string message,
      MessageBoxButton button,
      MessageBoxImage image,
      Window owner)
    {
      string text = message ?? "null";
      Window window = owner ?? Application.Current?.MainWindow;
      return Application.Current == null ? MessageBoxResult.None : Application.Current.Dispatcher.Invoke<MessageBoxResult>((Func<MessageBoxResult>) (() => window == null ? MessageBox.Show(text, Utils.GetString("PublicTickTick"), button, image) : MessageBox.Show(window, text, Utils.GetString("PublicTickTick"), button, image)));
    }

    public static void Beep() => SystemSounds.Beep.Play();
  }
}
