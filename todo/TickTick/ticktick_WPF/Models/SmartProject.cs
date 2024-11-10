// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SmartProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SmartProject : IDroppable
  {
    public virtual string Id { get; }

    public virtual string UserEventId { get; }

    public virtual string Name { get; }

    public virtual Geometry Icon { get; }

    public virtual string IconText => string.Empty;

    public int Count { get; set; }

    public virtual int SortOrder { get; set; }

    public virtual bool CanDrop => true;

    public virtual string ProjectId => (string) null;

    public virtual DateTime? DefaultDate => new DateTime?();

    public virtual bool IsCompleted => false;

    public virtual bool IsAbandoned => false;

    public virtual bool IsDeleted => false;

    public virtual List<string> Tags => new List<string>();

    public virtual int Priority => 0;

    public virtual bool Multiple => false;
  }
}
