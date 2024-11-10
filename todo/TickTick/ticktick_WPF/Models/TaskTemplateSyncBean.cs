// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskTemplateSyncBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskTemplateSyncBean
  {
    [JsonProperty(PropertyName = "add")]
    public List<TaskTemplateModel> Add { get; set; } = new List<TaskTemplateModel>();

    [JsonProperty(PropertyName = "delete")]
    public List<string> Delete { get; set; } = new List<string>();

    [JsonProperty(PropertyName = "update")]
    public List<TaskTemplateModel> Update { get; set; } = new List<TaskTemplateModel>();

    public bool IsEmpty()
    {
      List<TaskTemplateModel> add = this.Add;
      // ISSUE: explicit non-virtual call
      if ((add != null ? (__nonvirtual (add.Count) == 0 ? 1 : 0) : 0) != 0)
      {
        List<string> delete = this.Delete;
        // ISSUE: explicit non-virtual call
        if ((delete != null ? (__nonvirtual (delete.Count) == 0 ? 1 : 0) : 0) != 0)
        {
          List<TaskTemplateModel> update = this.Update;
          // ISSUE: explicit non-virtual call
          return update != null && __nonvirtual (update.Count) == 0;
        }
      }
      return false;
    }
  }
}
