// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.QuickSetModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class QuickSetModel
  {
    public QuickSetType Type;
    public string Title;
    public string Tag;
    public string CalId;
    public ProjectModel Project;
    public DateTime? Date;
    public AvatarViewModel Avatar;
    public int Priority;
    public TaskBaseViewModel task;
  }
}
