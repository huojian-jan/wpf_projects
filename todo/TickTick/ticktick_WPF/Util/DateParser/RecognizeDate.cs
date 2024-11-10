// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.RecognizeDate
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  internal class RecognizeDate
  {
    private int recognizeStringStartPosition;
    private DateTime cal;
    private string reminder;
    private List<string> recogStrings = new List<string>();
    private TickTickDuration duration;

    public RecognizeDate()
    {
    }

    public RecognizeDate(string recogString, int recognizeStringStartPosition, DateTime cal)
    {
      this.recogStrings.Add(recogString);
      this.recognizeStringStartPosition = recognizeStringStartPosition;
      this.cal = cal;
    }

    public RecognizeDate(
      string recogString,
      int recognizeStringStartPosition,
      TickTickDuration duration)
    {
      this.recogStrings.Add(recogString);
      this.recognizeStringStartPosition = recognizeStringStartPosition;
      this.duration = duration;
    }

    public int getRecognizeStringStartPosition() => this.recognizeStringStartPosition;

    public DateTime getCal() => this.cal;

    public void SetCal(DateTime cal) => this.cal = cal;

    public void addRecogString(string str) => this.recogStrings.Add(str);

    public string getReminder() => this.reminder;

    public void setReminder(string reminder) => this.reminder = reminder;

    public List<string> getRecogString() => this.recogStrings;

    public TickTickDuration getDuration() => this.duration;
  }
}
