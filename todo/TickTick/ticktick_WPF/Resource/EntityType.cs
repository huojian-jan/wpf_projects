// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Resource.EntityType
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Resource
{
  public static class EntityType
  {
    public const string Task = "task";
    public const string Subtask = "item";
    public const string CalendarEvent = "event";
    public const string Derivative = "derivative";
    public const string Agenda = "agenda";
    public const string Course = "course";
    private static readonly Dictionary<string, int> KeyDict = new Dictionary<string, int>()
    {
      {
        "task",
        1
      },
      {
        "item",
        2
      },
      {
        "event",
        3
      },
      {
        "derivative",
        4
      },
      {
        "agenda",
        1
      },
      {
        "course",
        10
      }
    };

    public static int GetEntityTypeNum(string type)
    {
      int entityTypeNum;
      EntityType.KeyDict.TryGetValue(type, out entityTypeNum);
      return entityTypeNum;
    }

    public static string GetEntityType(int order)
    {
      return EntityType.KeyDict.FirstOrDefault<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>) (x => x.Value == order)).Key;
    }

    public static int GetEntityTypeNum(DisplayType type)
    {
      return EntityType.GetEntityTypeNum(EntityType.GetEntityType(type));
    }

    public static string GetEntityType(DisplayType type)
    {
      switch (type)
      {
        case DisplayType.Task:
          return "task";
        case DisplayType.CheckItem:
          return "item";
        case DisplayType.Agenda:
          return "agenda";
        case DisplayType.Derivative:
          return "derivative";
        case DisplayType.Event:
          return "event";
        case DisplayType.Course:
          return "course";
        default:
          return "task";
      }
    }
  }
}
