// Decompiled with JetBrains decompiler
// Type: TabPlanData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
public class TabPlanData
{
  public string code { get; set; }

  public Data data { get; set; }

  public override string ToString() => "code:" + this.code + ",data: " + this.data?.ToString();
}
