// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TempDataModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TempDataModel : BaseModel
  {
    public const string NOTIFICIATION = "NOTIFICIATION";
    public const string SHAREUSERLIST = "SHAREUSERLIST";
    public const string USERLIST = "USERLIST";
    public const string ATTACHMENTLIMIT = "ATTACHMENTLIMIT";

    public string User_Id { get; set; }

    public string Data { get; set; }

    public string DataType { get; set; }

    public long ModifyTime { get; set; }

    public string EntityId { get; set; }
  }
}
