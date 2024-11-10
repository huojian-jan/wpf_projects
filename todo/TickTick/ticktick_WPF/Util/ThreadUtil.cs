// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ThreadUtil
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public class ThreadUtil
  {
    public static void DetachedRunOnUiThread(Action action)
    {
      if (action == null)
        return;
      new Thread((ThreadStart) (() => Application.Current?.Dispatcher?.Invoke((Action) (() => Utils.LogActionTimes(action, 400, (string) null, action.Target?.ToString() + " " + action.Method?.ToString())), DispatcherPriority.Normal))).Start();
    }

    public static void DetachedRunOnUiBackThread(Action action)
    {
      if (action == null)
        return;
      new Thread((ThreadStart) (() => Application.Current?.Dispatcher?.Invoke((Action) (() => Utils.LogActionTimes(action, 400, (string) null, action.Target?.ToString() + " " + action.Method?.ToString())), DispatcherPriority.Background))).Start();
    }

    public static void WriteLock(ReaderWriterLockSlim locker, Action action)
    {
      if (action == null)
        return;
      locker.EnterWriteLock();
      try
      {
        action();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
      finally
      {
        locker.ExitWriteLock();
      }
    }

    public static T WriteLock<T>(ReaderWriterLockSlim locker, Func<T> action)
    {
      if (action != null)
      {
        locker.EnterWriteLock();
        try
        {
          return action();
        }
        catch (Exception ex)
        {
          UtilLog.Error(ex);
        }
        finally
        {
          locker.ExitWriteLock();
        }
      }
      return default (T);
    }

    public static void ReadLock(ReaderWriterLockSlim locker, Action action)
    {
      if (action == null)
        return;
      locker.EnterReadLock();
      try
      {
        action();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
      finally
      {
        locker.ExitReadLock();
      }
    }

    public static T ReadLock<T>(ReaderWriterLockSlim locker, Func<T> action)
    {
      if (action != null)
      {
        locker.EnterReadLock();
        try
        {
          return action();
        }
        catch (Exception ex)
        {
          UtilLog.Error(ex);
        }
        finally
        {
          locker.ExitReadLock();
        }
      }
      return default (T);
    }
  }
}
