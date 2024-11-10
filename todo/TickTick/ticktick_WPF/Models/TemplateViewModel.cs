// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TemplateViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TemplateViewModel : BaseViewModel
  {
    private string _title;
    private string _content;
    private bool _openOption;
    private string _desc;
    private List<string> _items;

    public string Id { get; set; }

    public List<string> Tags { get; set; }

    public bool IsList { get; set; }

    public bool WithChild
    {
      get
      {
        List<TemplateChildViewModel> children = this.Children;
        return children != null && __nonvirtual (children.Count) > 0;
      }
    }

    public DateTime CreatedTime { get; set; }

    public List<TemplateChildViewModel> Children { get; set; }

    public bool ShowContent => !this.IsList && !string.IsNullOrEmpty(this.Content?.Trim());

    public bool ShowTag
    {
      get
      {
        List<string> tags = this.Tags;
        return tags != null && __nonvirtual (tags.Count) > 0;
      }
    }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public string Desc
    {
      get => this._desc;
      set
      {
        this._desc = value;
        this.OnPropertyChanged(nameof (Desc));
      }
    }

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.OnPropertyChanged(nameof (Content));
      }
    }

    public List<string> Items
    {
      get => this._items;
      set
      {
        this._items = value;
        this.OnPropertyChanged(nameof (Items));
      }
    }

    public bool OpenOption
    {
      get => this._openOption;
      set
      {
        this._openOption = value;
        this.OnPropertyChanged(nameof (OpenOption));
      }
    }

    private TemplateViewModel(TaskTemplateModel model)
    {
      this.Id = model.Id;
      this.Title = model.Title;
      this.Content = model.Content;
      this.Desc = model.Desc;
      List<string> items = model.Items;
      this.Items = items != null ? items.ToList<string>() : (List<string>) null;
      List<string> tags = model.Tags;
      this.Tags = tags != null ? tags.ToList<string>() : (List<string>) null;
      this.IsList = this.Items != null || !string.IsNullOrEmpty(this.Desc);
      this.CreatedTime = model.CreatedTime;
      this.Children = TemplateViewModel.BuildChildren(model.Chidlren);
    }

    private static List<TemplateChildViewModel> BuildChildren(
      List<TaskTemplateModel> templateChildren)
    {
      if (templateChildren == null)
        return (List<TemplateChildViewModel>) null;
      List<TemplateChildViewModel> children = new List<TemplateChildViewModel>();
      templateChildren.ForEach((Action<TaskTemplateModel>) (child => GetChildren(child, children, -1)));
      return children;

      void GetChildren(
        TaskTemplateModel templateChild,
        List<TemplateChildViewModel> childViewModels,
        int parentLevel)
      {
        TemplateChildViewModel templateChildViewModel1 = new TemplateChildViewModel();
        templateChildViewModel1.Id = templateChild.Id;
        templateChildViewModel1.Title = templateChild.Title;
        templateChildViewModel1.Level = parentLevel + 1;
        List<string> items = templateChild.Items;
        // ISSUE: explicit non-virtual call
        templateChildViewModel1.IsTextMode = (items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) == 0;
        TemplateChildViewModel templateChildViewModel2 = templateChildViewModel1;
        childViewModels.Add(templateChildViewModel2);
        templateChild.Chidlren?.ForEach((Action<TaskTemplateModel>) (child => GetChildren(child, children, Math.Min(2, parentLevel + 1))));
      }
    }

    public static List<TemplateViewModel> Build(List<TaskTemplateModel> models)
    {
      return models.Select<TaskTemplateModel, TemplateViewModel>((Func<TaskTemplateModel, TemplateViewModel>) (m => new TemplateViewModel(m))).ToList<TemplateViewModel>();
    }
  }
}
