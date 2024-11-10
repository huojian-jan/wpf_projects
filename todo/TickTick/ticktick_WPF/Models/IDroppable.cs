// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.IDroppable
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public interface IDroppable
  {
    bool CanDrop { get; }

    string ProjectId { get; }

    DateTime? DefaultDate { get; }

    bool IsCompleted { get; }

    bool IsAbandoned { get; }

    List<string> Tags { get; }

    int Priority { get; }

    bool Multiple { get; }

    bool IsDeleted { get; }
  }
}
