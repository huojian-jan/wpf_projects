// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.SelectableItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class SelectableItemViewModel : UpDownSelectViewModel
  {
    private bool _highlighted;
    private bool _open;
    private bool _partSelected;
    private bool _inCalFilter;
    private Geometry _icon;

    public bool CanMultiSelect { get; protected set; } = true;

    public string Title { get; set; } = "";

    public string Desc { get; set; } = "";

    public string IconText { get; set; }

    public Geometry Icon { get; set; }

    public bool ShowIndent { get; set; }

    public bool IsSubItem { get; set; }

    public bool IsParent { get; set; }

    public string Id { get; protected set; }

    public string ParentId { get; protected set; }

    public long SortOrder { get; set; }

    public string Type { get; set; }

    public bool BatchMode { get; set; }

    public bool IsSplit { get; set; }

    public bool IsSectionGroup { get; set; }

    public bool ShowIcon { get; set; } = true;

    public string TeamId { get; set; }

    public bool IsTeam { get; set; }

    public bool IsNote { get; set; }

    public bool IsBold { get; set; }

    public bool IsShare { get; set; }

    public string Emoji { get; set; } = string.Empty;

    public bool ShowSubOnSide { get; set; }

    public SelectableItemViewModel GroupParent { get; set; }

    public List<SelectableItemViewModel> Children { get; set; } = new List<SelectableItemViewModel>();

    protected virtual void SetOpenIcon()
    {
    }

    public bool Open
    {
      get => this._open;
      set
      {
        this._open = value;
        this.OnPropertyChanged(nameof (Open));
        this.OnPropertyChanged("Highlighted");
        this.SetOpenIcon();
      }
    }

    public bool Highlighted => this._partSelected || this.Selected;

    public bool InCalFilter
    {
      get => this._inCalFilter;
      set
      {
        this._inCalFilter = value;
        this.OnPropertyChanged(nameof (InCalFilter));
      }
    }

    public bool PartSelected
    {
      get => this._partSelected;
      set
      {
        this._partSelected = value;
        this.OnPropertyChanged(nameof (PartSelected));
        this.OnPropertyChanged("Highlighted");
      }
    }

    public override void OnSelectedChanged()
    {
      this.OnPropertyChanged("Highlighted");
      if (!this.Selected || this.IsSectionGroup)
        return;
      this.PartSelected = false;
    }

    public (string, string) GetProjectAndColumnId()
    {
      string str1 = string.Empty;
      string str2;
      switch (this)
      {
        case ProjectViewModel projectViewModel:
          str2 = projectViewModel.Id;
          List<SelectableItemViewModel> children = projectViewModel.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0 && projectViewModel.Children[0] is ProjectColumnViewModel child)
          {
            str1 = child.Id;
            break;
          }
          break;
        case ProjectColumnViewModel projectColumnViewModel:
          str1 = projectColumnViewModel.Id;
          str2 = projectColumnViewModel.ParentId;
          break;
        default:
          str2 = this.Id;
          break;
      }
      return (str2, str1);
    }
  }
}
