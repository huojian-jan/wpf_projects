// Decompiled with JetBrains decompiler
// Type: ProjectTaskIds
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Util.Provider;

#nullable disable
public class ProjectTaskIds
{
  public BlockingSet<string> UnCompletedTaskIds { get; set; } = new BlockingSet<string>();

  public BlockingSet<string> UnCompletedNoteIds { get; set; } = new BlockingSet<string>();

  public BlockingSet<string> UnCompletedItemIds { get; set; } = new BlockingSet<string>();

  public ProjectIdentity Project { get; set; }

  public int EventCounts { get; set; }
}
