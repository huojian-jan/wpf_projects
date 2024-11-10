// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TagSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TagSection : Section
  {
    public TagSection(string tag)
      : base()
    {
      this.Name = tag;
    }

    public override string GetTag()
    {
      if (this.Name == Utils.GetString("NoTags"))
        return string.Empty;
      return TagSerializer.ToJsonContent(new List<string>()
      {
        this.Name
      });
    }

    public override bool CanSwitch(DisplayType displayType) => true;

    public override bool CanSort(string sortBy) => true;
  }
}
