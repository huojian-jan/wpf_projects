﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.WindowExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Reflection;
using System.Windows;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public static class WindowExtensions
  {
    public static double GetActualLeft(this Window window)
    {
      return (double) typeof (Window).GetField("_actualLeft", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) window);
    }

    public static double GetActualTop(this Window window)
    {
      return (double) typeof (Window).GetField("_actualTop", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) window);
    }
  }
}