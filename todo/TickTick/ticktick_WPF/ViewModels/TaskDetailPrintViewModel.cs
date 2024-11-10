// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TaskDetailPrintViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TaskDetailPrintViewModel
  {
    public string Title { get; set; }

    public string ContentOrDesc { get; set; }

    public string Kind { get; set; }

    public string AttendId { get; set; }

    public string DayText { get; set; }

    public string RepeatFlag { get; set; }

    public string RepeatFrom { get; set; }

    public BitmapImage Avatar { get; set; }

    public string TaskId { get; set; }

    public string ProjectId { get; set; }

    public int Status { get; set; }

    public int Mode { get; set; }

    public int Priority { get; set; }

    public int? Progress { get; set; }

    public bool ShowSetTime { get; set; }

    public bool IsContent { get; set; }

    public bool? IsAllDay { get; set; }

    public int ImageMode { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public List<SubtaskPrintViewModel> SubtaskPrintViewModels { get; set; }

    public List<SubtaskPrintViewModel> RealSubtaskPrintViewModels { get; set; }

    public List<TagPrintModel> Tags { get; set; }

    public List<AttachmentViewModel> Attachments { get; set; }

    public List<CommentViewModel> Comments { get; set; }

    public TaskDetailPrintViewModel()
    {
    }

    public TaskDetailPrintViewModel(TaskDetailViewModel model)
    {
      this.TaskId = model.TaskId;
      this.Title = model.Title;
      this.Kind = model.Kind;
      this.IsContent = this.Kind != "CHECKLIST";
      this.ContentOrDesc = this.IsContent ? model.TaskContent : model.Desc;
      this.AttendId = model.AttendId;
      this.DayText = model.DayText;
      this.RepeatFlag = model.RepeatFlag;
      this.RepeatFrom = model.RepeatFrom;
      this.ProjectId = model.ProjectId;
      this.Progress = model.Progress;
      this.ShowSetTime = model.ShowSetTime;
      this.IsAllDay = model.IsAllDay;
      this.StartDate = model.StartDate;
      this.DueDate = model.DueDate;
      this.Priority = model.Priority;
      this.ImageMode = model.ImageMode;
      this.Tags = this.LoadTags(model.Tag);
    }

    private List<TagPrintModel> LoadTags(string tagString)
    {
      List<TagPrintModel> tagPrintModelList = new List<TagPrintModel>();
      List<string> tags1 = TagSerializer.ToTags(tagString);
      List<TagModel> tags2 = CacheManager.GetTags();
      foreach (string str in tags1)
      {
        string tagName = str;
        TagModel tagModel = tags2.FirstOrDefault<TagModel>((Func<TagModel, bool>) (m => m.name == tagName));
        if (tagModel != null)
        {
          TagPrintModel tagPrintModel1 = new TagPrintModel()
          {
            TagTitle = tagModel.GetDisplayName(),
            TagBackColor = !string.IsNullOrEmpty(tagModel.color) ? ThemeUtil.GetColorInString(tagModel.color) : ThemeUtil.GetColor("PrimaryColor"),
            TagTextColor = ThemeUtil.GetColorInString("#191919")
          };
          TagPrintModel tagPrintModel2 = tagPrintModel1;
          SolidColorBrush solidColorBrush = new SolidColorBrush();
          solidColorBrush.Color = tagPrintModel1.TagBackColor.Color;
          solidColorBrush.Opacity = 0.56;
          tagPrintModel2.TagBackColor = solidColorBrush;
          tagPrintModelList.Add(tagPrintModel1);
        }
      }
      return tagPrintModelList;
    }

    public async Task LoadAttachment()
    {
      TaskDetailPrintViewModel detailPrintViewModel1 = this;
      AttachmentProvider attachmentProvider = new AttachmentProvider((IEnumerable<AttachmentModel>) await AttachmentDao.GetTaskAttachments(detailPrintViewModel1.TaskId), detailPrintViewModel1.ImageMode);
      TaskDetailPrintViewModel detailPrintViewModel2 = detailPrintViewModel1;
      List<AttachmentViewModel> models = attachmentProvider.GetModels();
      List<AttachmentViewModel> list = models != null ? models.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>) (a => a.AttachmentModel.status == 0 && !a.AttachmentModel.deleted)).ToList<AttachmentViewModel>() : (List<AttachmentViewModel>) null;
      detailPrintViewModel2.Attachments = list;
    }
  }
}
