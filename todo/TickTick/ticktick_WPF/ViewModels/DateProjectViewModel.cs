// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.DateProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Provider;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class DateProjectViewModel : ProjectItemViewModel, IDroppable
  {
    public DateProjectViewModel(string dateStamp)
    {
      this.Id = dateStamp;
      this.Title = DateProjectViewModel.GetTitle(dateStamp);
      this.CanDrag = true;
      this.CanDrop = true;
      this.IsPtfItem = true;
      this.ListType = ProjectListType.Project;
      this.ViewMode = "list";
    }

    private static string GetTitle(string stamp)
    {
      return DateTime.ParseExact(stamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
    }

    public override ProjectIdentity GetIdentity()
    {
      return (ProjectIdentity) new DateProjectIdentity(this.Id);
    }

    public string ProjectId => string.Empty;

    public DateTime? DefaultDate => new DateTime?();

    public bool IsCompleted => false;

    public bool IsAbandoned => false;

    public List<string> Tags => new List<string>();

    public int Priority => 0;

    public bool Multiple => false;

    public bool IsDeleted => false;
  }
}
