// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.TaskSearchModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class TaskSearchModel
  {
    public string Id => this.SourceModel?.Id;

    public string Title => this.SourceModel?.Title;

    public string Content { get; set; }

    public string Desc => this.SourceModel?.Desc;

    public string Attachment { get; set; }

    public string CheckItems { get; set; } = "";

    public string Comment { get; set; } = "";

    public TaskBaseViewModel SourceModel { get; set; }

    public bool Contains(string searchKey) => this.GetText().Contains(searchKey);

    public string GetText()
    {
      return (string.Empty + this.Title + "\r\n" + this.Content + "\r\n" + this.Desc + "\r\n" + this.Attachment + "\r\n" + this.CheckItems + "\r\n" + this.Comment).ToLowerInvariant();
    }
  }
}
