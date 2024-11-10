// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KeyBindingCommand
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows;
using System.Windows.Input;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class KeyBindingCommand
  {
    public static void SyncCommand(object sender = null) => SyncManager.Sync();

    public static void InputCommand(object sender, ProjectIdentity identity)
    {
      if (identity != null)
      {
        ProjectIdentity project1 = identity;
        if (project1 is NormalProjectIdentity normalProjectIdentity)
        {
          ProjectModel project2 = normalProjectIdentity.Project;
          if ((project2 != null ? (project2.IsEnable() ? 1 : 0) : 0) == 0)
            goto label_4;
        }
        if (!(project1 is SubscribeCalendarProjectIdentity) && !(project1 is BindAccountCalendarProjectIdentity))
        {
          identity = project1.Copy(project1);
          goto label_7;
        }
label_4:
        identity = (ProjectIdentity) ProjectIdentity.GetDefaultProject();
      }
      else
        identity = (ProjectIdentity) ProjectIdentity.GetDefaultProject();
label_7:
      AddTaskWindow.ShowWindow(sender as Window, identity);
    }

    private static ITaskOperation GetFocusedTaskItem(object sender)
    {
      if (sender is DependencyObject element)
      {
        switch (FocusManager.GetFocusedElement(element))
        {
          case ITaskOperation focusedTaskItem:
            return focusedTaskItem;
          case TextArea child:
            ITaskOperation parent = Utils.FindParent<ITaskOperation>((DependencyObject) child);
            if (parent != null)
              return parent;
            break;
        }
      }
      return (ITaskOperation) null;
    }

    public static void SetPriorityCommand(object sender, int priority)
    {
      ITaskOperation focusedTaskItem = KeyBindingCommand.GetFocusedTaskItem(sender);
      if (focusedTaskItem != null)
      {
        focusedTaskItem.SetPriority(priority);
      }
      else
      {
        if (!(sender is IKeyBinding keyBinding))
          return;
        keyBinding.BatchSetPriorityCommand(priority);
      }
    }

    public static void SetDateCommand(object sender, string key)
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "set_date");
      ITaskOperation focusedTaskItem = KeyBindingCommand.GetFocusedTaskItem(sender);
      if (focusedTaskItem != null)
      {
        focusedTaskItem.SetDate(key);
      }
      else
      {
        DateTime dateTime = DateTime.Today;
        switch (key)
        {
          case "today":
            dateTime = DateTime.Today;
            break;
          case "tomorrow":
            dateTime = DateTime.Today.AddDays(1.0);
            break;
          case "nextweek":
            dateTime = DateTime.Today.AddDays(7.0);
            break;
        }
        if (!(sender is IKeyBinding keyBinding))
          return;
        keyBinding.BatchSetDateCommand(new DateTime?(dateTime));
      }
    }

    public static void ClearDateCommand(object sender)
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "clear_time");
      ITaskOperation focusedTaskItem = KeyBindingCommand.GetFocusedTaskItem(sender);
      if (focusedTaskItem != null)
      {
        focusedTaskItem.ClearDate();
      }
      else
      {
        if (!(sender is IKeyBinding keyBinding))
          return;
        DateTime? date = new DateTime?();
        keyBinding.BatchSetDateCommand(date);
      }
    }

    public static void PinTaskCommand(object sender)
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "pin_tasks");
      ITaskOperation focusedTaskItem = KeyBindingCommand.GetFocusedTaskItem(sender);
      if (focusedTaskItem != null)
      {
        focusedTaskItem.PinOrUnpinTask();
      }
      else
      {
        if (!(sender is IKeyBinding keyBinding))
          return;
        keyBinding.BatchPinTaskCommand();
      }
    }

    public static void OpenStickyCommand(object sender)
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "open_as_sticky_note");
      ITaskOperation focusedTaskItem = KeyBindingCommand.GetFocusedTaskItem(sender);
      if (focusedTaskItem != null)
      {
        focusedTaskItem.OpenSticky();
      }
      else
      {
        if (!(sender is IKeyBinding keyBinding))
          return;
        keyBinding.BatchOpenStickyCommand();
      }
    }

    public static void DeleteCommand(object sender)
    {
      UserActCollectUtils.AddShortCutEvent("edit_tasks", "delete_tasks");
      ITaskOperation focusedTaskItem = KeyBindingCommand.GetFocusedTaskItem(sender);
      if (focusedTaskItem != null)
      {
        focusedTaskItem.Delete();
      }
      else
      {
        if (!(sender is IKeyBinding keyBinding))
          return;
        keyBinding.BatchDeleteCommand();
      }
    }
  }
}
