// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Notifier.TasksChangeEventManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;

#nullable disable
namespace ticktick_WPF.Notifier
{
  public class TasksChangeEventManager : WeakEventManager
  {
    private TasksChangeEventManager()
    {
    }

    public static void AddHandler(
      TaskChangeNotifier source,
      EventHandler<TasksChangeEventArgs> handler)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (handler == null)
        throw new ArgumentNullException(nameof (handler));
      TasksChangeEventManager.CurrentManager.ProtectedAddHandler((object) source, (Delegate) handler);
    }

    public static void RemoveHandler(
      TaskChangeNotifier source,
      EventHandler<TasksChangeEventArgs> handler)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      if (handler == null)
        throw new ArgumentNullException(nameof (handler));
      TasksChangeEventManager.CurrentManager.ProtectedRemoveHandler((object) source, (Delegate) handler);
    }

    private static TasksChangeEventManager CurrentManager
    {
      get
      {
        Type managerType = typeof (TasksChangeEventManager);
        TasksChangeEventManager manager = (TasksChangeEventManager) WeakEventManager.GetCurrentManager(managerType);
        if (manager == null)
        {
          manager = new TasksChangeEventManager();
          WeakEventManager.SetCurrentManager(managerType, (WeakEventManager) manager);
        }
        return manager;
      }
    }

    protected override WeakEventManager.ListenerList NewListenerList()
    {
      return (WeakEventManager.ListenerList) new WeakEventManager.ListenerList<TasksChangeEventArgs>();
    }

    protected override void StartListening(object source)
    {
      ((TaskChangeNotifier) source).TasksChanged += new EventHandler<TasksChangeEventArgs>(this.OnSomeEvent);
    }

    protected override void StopListening(object source)
    {
      ((TaskChangeNotifier) source).TasksChanged -= new EventHandler<TasksChangeEventArgs>(this.OnSomeEvent);
    }

    private void OnSomeEvent(object sender, TasksChangeEventArgs e)
    {
      this.DeliverEvent(sender, (EventArgs) e);
    }
  }
}
