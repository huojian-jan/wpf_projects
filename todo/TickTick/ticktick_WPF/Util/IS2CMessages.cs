﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.IS2CMessages
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.ServiceModel;

#nullable disable
namespace ticktick_WPF.Util
{
  [ServiceContract(SessionMode = SessionMode.Allowed)]
  public interface IS2CMessages
  {
    [OperationContract(IsOneWay = true)]
    void CommandInClient(string text);
  }
}