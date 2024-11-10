// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.SortProjectData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class SortProjectData : ProjectData
  {
    public SortOption SortOption
    {
      get => this.ProjectIdentity.SortOption;
      set => this.ProjectIdentity.SortOption = value;
    }

    public bool ShowLoadMore { get; set; }

    public static async Task<bool> IsSortOrderEmpty(SortOption option, string id)
    {
      bool flag;
      return flag;
    }

    protected static void SaveSpecialProjectSortType(string index, SortOption sortOption)
    {
      LocalSettings.Settings[index] = (object) sortOption.ToSmartString();
    }

    public virtual string GetTitle() => string.Empty;

    public virtual void SaveSortOption(SortOption sortOption)
    {
    }

    public virtual DrawingImage GetEmptyImage() => this.EmptyImage;

    public virtual Geometry GetEmptyPath() => this.EmptyPath;

    public virtual Thickness GetEmptyMargin() => new Thickness(0.0);

    public virtual async Task<string> GetEmptyTitle() => this.EmptyTitle;

    public virtual string GetEmptyContent() => this.EmptyContent;

    public virtual bool ShowFilterExpired() => false;

    public virtual bool ShowCalendarExpired() => false;
  }
}
