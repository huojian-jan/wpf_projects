// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.SyncTaskBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class SyncTaskBean
  {
    [JsonProperty(PropertyName = "update")]
    public List<TaskModel> Update { get; set; } = new List<TaskModel>();

    [JsonProperty(PropertyName = "delete")]
    public List<TaskProject> Delete { get; set; } = new List<TaskProject>();

    [JsonProperty(PropertyName = "add")]
    public List<TaskModel> Add { get; set; } = new List<TaskModel>();

    [JsonProperty(PropertyName = "deletedInTrash")]
    public List<TaskProject> DeletedInTrash { get; set; } = new List<TaskProject>();

    [JsonProperty(PropertyName = "deletedForever")]
    public List<TaskProject> DeletedForever { get; set; } = new List<TaskProject>();

    [JsonIgnore]
    public List<TaskProject> DeletedFromTrash { get; set; } = new List<TaskProject>();

    [JsonProperty(PropertyName = "addAttachments")]
    public List<TaskModel> AddAttachments { get; set; } = new List<TaskModel>();

    [JsonProperty(PropertyName = "updateAttachments")]
    public List<TaskModel> UpdateAttachments { get; set; } = new List<TaskModel>();

    [JsonProperty(PropertyName = "deleteAttachments")]
    public List<TaskModel> DeleteAttachments { get; set; } = new List<TaskModel>();

    public bool Empty
    {
      get
      {
        return this.Add.Count == 0 && this.Update.Count == 0 && this.Delete.Count == 0 && this.DeletedInTrash.Count == 0 && this.DeletedForever.Count == 0;
      }
    }

    public string GetCountMessage()
    {
      string[] strArray = new string[17];
      int? nullable1 = this.Update?.Count;
      strArray[0] = nullable1.ToString();
      strArray[1] = " ";
      List<TaskProject> delete = this.Delete;
      int? nullable2;
      if (delete == null)
      {
        nullable1 = new int?();
        nullable2 = nullable1;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        nullable2 = new int?(__nonvirtual (delete.Count));
      }
      nullable1 = nullable2;
      strArray[2] = nullable1.ToString();
      strArray[3] = " ";
      List<TaskModel> add = this.Add;
      int? nullable3;
      if (add == null)
      {
        nullable1 = new int?();
        nullable3 = nullable1;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        nullable3 = new int?(__nonvirtual (add.Count));
      }
      nullable1 = nullable3;
      strArray[4] = nullable1.ToString();
      strArray[5] = " ";
      List<TaskProject> deletedInTrash = this.DeletedInTrash;
      int? nullable4;
      if (deletedInTrash == null)
      {
        nullable1 = new int?();
        nullable4 = nullable1;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        nullable4 = new int?(__nonvirtual (deletedInTrash.Count));
      }
      nullable1 = nullable4;
      strArray[6] = nullable1.ToString();
      strArray[7] = " ";
      List<TaskProject> deletedForever = this.DeletedForever;
      int? nullable5;
      if (deletedForever == null)
      {
        nullable1 = new int?();
        nullable5 = nullable1;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        nullable5 = new int?(__nonvirtual (deletedForever.Count));
      }
      nullable1 = nullable5;
      strArray[8] = nullable1.ToString();
      strArray[9] = " ";
      List<TaskProject> deletedFromTrash = this.DeletedFromTrash;
      int? nullable6;
      if (deletedFromTrash == null)
      {
        nullable1 = new int?();
        nullable6 = nullable1;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        nullable6 = new int?(__nonvirtual (deletedFromTrash.Count));
      }
      nullable1 = nullable6;
      strArray[10] = nullable1.ToString();
      strArray[11] = "  ";
      List<TaskModel> addAttachments = this.AddAttachments;
      int? nullable7;
      if (addAttachments == null)
      {
        nullable1 = new int?();
        nullable7 = nullable1;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        nullable7 = new int?(__nonvirtual (addAttachments.Count));
      }
      nullable1 = nullable7;
      strArray[12] = nullable1.ToString();
      strArray[13] = "  ";
      List<TaskModel> updateAttachments = this.UpdateAttachments;
      int? nullable8;
      if (updateAttachments == null)
      {
        nullable1 = new int?();
        nullable8 = nullable1;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        nullable8 = new int?(__nonvirtual (updateAttachments.Count));
      }
      nullable1 = nullable8;
      strArray[14] = nullable1.ToString();
      strArray[15] = "  ";
      List<TaskModel> deleteAttachments = this.DeleteAttachments;
      int? nullable9;
      if (deleteAttachments == null)
      {
        nullable1 = new int?();
        nullable9 = nullable1;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        nullable9 = new int?(__nonvirtual (deleteAttachments.Count));
      }
      nullable1 = nullable9;
      strArray[16] = nullable1.ToString();
      return string.Concat(strArray);
    }
  }
}
