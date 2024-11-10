// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TrashSyncService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class TrashSyncService
  {
    private const int LoadRemoteStep = 50;
    public static bool PersonalDrainOff;
    public static bool TeamDrainOff;
    private static int _personalPage;
    private static int _teamPage;
    private static long? _personalNext;
    private static long? _teamNext;

    public static bool IsDrainOff(bool isPerson)
    {
      return !(!UserManager.IsTeamUser() | isPerson) ? TrashSyncService.TeamDrainOff : TrashSyncService.PersonalDrainOff;
    }

    public static long? Next(bool isPerson)
    {
      return !(!UserManager.IsTeamUser() | isPerson) ? TrashSyncService._teamNext : TrashSyncService._personalNext;
    }

    public static int TrashPage(bool isPerson)
    {
      return Math.Max(isPerson ? TrashSyncService._personalPage : TrashSyncService._teamPage, 1);
    }

    public async Task<bool> TryLoadTrashTasks(bool isPerson)
    {
      isPerson = !UserManager.IsTeamUser() | isPerson;
      if (TrashSyncService.IsDrainOff(isPerson))
        return false;
      (string s, List<TaskModel> tasks) = await Communicator.BatchCheckTrash(TrashSyncService.Next(isPerson), 50, isPerson ? 1 : 2);
      if (tasks == null)
        return false;
      int num1 = await TaskService.MergeTasks((IEnumerable<TaskModel>) tasks) ? 1 : 0;
      long? nullable1 = TrashSyncService.Next(isPerson);
      long result = nullable1 ?? -1L;
      int num2;
      if (!string.IsNullOrEmpty(s) && long.TryParse(s, out result))
      {
        nullable1 = TrashSyncService.Next(isPerson);
        long num3 = result;
        num2 = nullable1.GetValueOrDefault() == num3 & nullable1.HasValue ? 1 : 0;
      }
      else
        num2 = 1;
      bool flag = num2 != 0;
      if (isPerson)
      {
        long? nullable2;
        if (!flag && result != -1L)
        {
          nullable2 = new long?(result);
        }
        else
        {
          nullable1 = new long?();
          nullable2 = nullable1;
        }
        TrashSyncService._personalNext = nullable2;
        TrashSyncService.PersonalDrainOff = flag;
        ++TrashSyncService._personalPage;
      }
      else
      {
        long? nullable3;
        if (!flag && result != -1L)
        {
          nullable3 = new long?(result);
        }
        else
        {
          nullable1 = new long?();
          nullable3 = nullable1;
        }
        TrashSyncService._teamNext = nullable3;
        TrashSyncService.TeamDrainOff = flag;
        ++TrashSyncService._teamPage;
      }
      return true;
    }

    public static void Reset()
    {
      TrashSyncService.PersonalDrainOff = false;
      TrashSyncService.TeamDrainOff = false;
      TrashSyncService._personalPage = 0;
      TrashSyncService._teamPage = 0;
      TrashSyncService._personalNext = new long?();
      TrashSyncService._teamNext = new long?();
    }
  }
}
