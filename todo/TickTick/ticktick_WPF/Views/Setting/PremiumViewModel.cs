// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.PremiumViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class PremiumViewModel : BaseViewModel
  {
    public string Title { get; set; }

    public string Desc { get; set; }

    public string Image { get; set; }

    public PremiumViewModel(string title, string desc, string imageName)
    {
      this.Title = title;
      this.Desc = desc;
      this.Image = "../../Assets/ImageSource/" + imageName + ".png";
    }

    public static List<PremiumViewModel> BuildViewModels()
    {
      return new List<PremiumViewModel>()
      {
        new PremiumViewModel(Utils.GetString("CalMonthView"), Utils.GetString("CalMonthViewDesc"), "CalMonthView"),
        new PremiumViewModel(Utils.GetString("CalTimelineView"), Utils.GetString("CalTimelineViewDesc"), "CalTimelineView"),
        new PremiumViewModel(Utils.GetString("Duration"), Utils.GetString("DurationDesc"), "Duration"),
        new PremiumViewModel(Utils.GetString("Filter"), Utils.GetString("FilterDesc"), "Filter"),
        new PremiumViewModel(Utils.GetString("CheckItemReminder"), Utils.GetString("CheckItemReminderDesc"), "CheckItemReminder"),
        new PremiumViewModel(Utils.GetString("PremiumThemesPro"), Utils.GetString("PremiumThemesDesc"), "PremiumThemes"),
        new PremiumViewModel(Utils.GetString("MoreListTaskItem"), Utils.GetString("MoreListTaskItemDesc"), "MoreListTaskItem"),
        new PremiumViewModel(Utils.GetString("MoreSharingMembersPro"), Utils.GetString("MoreSharingMembersDesc"), "MoreSharingMembers"),
        new PremiumViewModel(Utils.GetString("MoreAttachmentsPro"), Utils.GetString("MoreAttachmentsDesc"), "MoreAttachments"),
        new PremiumViewModel(Utils.GetString("MoreRemindersPro"), Utils.GetString("MoreRemindersDesc"), "MoreReminders"),
        new PremiumViewModel(Utils.GetString("ListActivitiesPro"), Utils.GetString("ListActivitiesDesc"), "ListActivities"),
        new PremiumViewModel(Utils.GetString("TaskActivities"), Utils.GetString("TaskActivitiesDesc"), "TaskActivities"),
        new PremiumViewModel(Utils.GetString("AdvancedSearchPro"), Utils.GetString("AdvancedSearchDesc"), "AdvancedSearch")
      };
    }
  }
}
