// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagSelectData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagSelectData
  {
    public List<string> OmniSelectTags = new List<string>();
    public List<string> PartSelectTags = new List<string>();

    public TagSelectData Clone()
    {
      TagSelectData tagSelectData = new TagSelectData();
      tagSelectData.OmniSelectTags.AddRange((IEnumerable<string>) this.OmniSelectTags);
      tagSelectData.PartSelectTags.AddRange((IEnumerable<string>) this.PartSelectTags);
      return tagSelectData;
    }

    public bool IsEqual(TagSelectData tags)
    {
      return tags.OmniSelectTags.Count == this.OmniSelectTags.Count && tags.PartSelectTags.Count == this.PartSelectTags.Count && !tags.OmniSelectTags.Any<string>((Func<string, bool>) (t => !this.OmniSelectTags.Contains(t))) && !tags.PartSelectTags.Any<string>((Func<string, bool>) (t => !this.PartSelectTags.Contains(t)));
    }
  }
}
