// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TagCardViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Views.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TagCardViewModel : FilterListDisplayModel<string>
  {
    public TagCardViewModel() => this.ConditionName = "tag";

    public List<string> Values
    {
      get => this.ValueList;
      set
      {
        this.ValueList = value;
        this.Content = this.ToCardText();
        this.OnPropertyChanged(nameof (Values));
      }
    }

    public override string ToCardText()
    {
      switch (this.ValueList.Count)
      {
        case 0:
          return Utils.GetString("Tags");
        case 1:
          return this.LogicType == LogicType.Not ? Utils.GetString("Exclude") + " " + TagCardViewModel.GetTagValue(this.ValueList[0]) : TagCardViewModel.GetTagValue(this.ValueList[0]);
        default:
          return this.LogicType == LogicType.Not ? FilterListDisplayModel<string>.GetLogicString(this.LogicType) + " ( " + TagCardViewModel.DisplayTagText((IReadOnlyList<string>) this.ValueList, ", ") + " )" : TagCardViewModel.DisplayTagText((IReadOnlyList<string>) this.ValueList, " " + FilterListDisplayModel<string>.GetLogicString(this.LogicType) + " ");
      }
    }

    public static string ToNormalDisplayText(List<string> tags, int displayNum = 3)
    {
      return TagCardViewModel.ToTagText(tags, ", ", displayNum);
    }

    private static string ToTagText(List<string> tags, string divider, int displayNum = 3)
    {
      if (tags.Count == 0)
        return Utils.GetString("AllTags");
      return tags.Count <= displayNum ? TagCardViewModel.DisplayTagText((IReadOnlyList<string>) tags, divider) : string.Format(Utils.GetString("TagDisplayMessage"), (object) TagCardViewModel.DisplayTagText((IReadOnlyList<string>) tags.Take<string>(displayNum).ToList<string>(), divider), (object) (tags.Count - displayNum));
    }

    private static string DisplayTagText(IReadOnlyList<string> tags, string divider)
    {
      string str = string.Empty;
      if (tags != null && tags.Count > 0)
      {
        for (int index = 0; index < tags.Count; ++index)
          str = index >= tags.Count - 1 ? str + TagCardViewModel.GetTagValue(tags[index]) : str + TagCardViewModel.GetTagValue(tags[index]) + divider;
      }
      return str;
    }

    private static string GetTagValue(string key)
    {
      if (key == "!tag")
        return Utils.GetString("NoTagsTitle");
      if (key == "*withtags")
        return Utils.GetString("WithTags");
      List<TagModel> tags = CacheManager.GetTags();
      return (tags != null ? tags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => string.Equals(t.name, key, StringComparison.CurrentCultureIgnoreCase))) : (TagModel) null)?.GetDisplayName() ?? key;
    }

    public override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new TagEditDialog();
    }

    public override Rule<string> GetParseRule() => (Rule<string>) new TagRule();
  }
}
