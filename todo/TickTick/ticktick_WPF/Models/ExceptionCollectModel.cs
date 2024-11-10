// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ExceptionCollectModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Globalization;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class ExceptionCollectModel
  {
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime time { get; set; }

    public string user { get; set; }

    public string etype { get; set; }

    public string hResult { get; set; }

    public string stackTrace { get; set; }

    public string targetSite { get; set; }

    public string cultureInfo { get; set; }

    public string message { get; set; }

    public string innerMessage { get; set; }

    public string data { get; set; }

    public string innerException { get; set; }

    public string source { get; set; }

    public string kind { get; set; }

    public ExceptionCollectModel(Exception e, ExceptionType type)
    {
      this.time = DateTime.Now;
      this.user = LocalSettings.Settings.LoginUserId;
      this.cultureInfo = CultureInfo.CurrentCulture?.ToString() ?? "";
      this.etype = e.GetType()?.ToString() ?? "";
      this.hResult = e.HResult.ToString() ?? "";
      this.stackTrace = e.StackTrace;
      this.targetSite = e.TargetSite?.ToString() ?? "";
      this.message = e.Message;
      if (e.InnerException != null)
        this.innerMessage = e.InnerException.Message;
      this.data = e.Data?.ToString() ?? "";
      this.innerException = e.InnerException?.ToString() ?? "";
      this.source = e.Source;
      this.kind = type.ToString() ?? "";
      if (!(this.targetSite == "Void SyncFlush()"))
        return;
      this.message += MemoryHelper.GetCurrentSize();
    }
  }
}
