// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagDisplayViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagDisplayViewModel : BaseViewModel
  {
    private bool _childSelected;
    private bool _collapsed;
    private bool _isOpen;
    private bool _partSelected;
    private bool _selected;
    private bool _isSubTag;
    private bool _isParent;
    private bool _focused;
    public string Tag;

    private TagDisplayViewModel()
    {
    }

    public TagDisplayViewModel(bool split) => this.IsSplit = split;

    public TagDisplayViewModel(string title, bool selected, bool partSelected = false)
    {
      this.Title = title;
      this._selected = selected;
      this._partSelected = partSelected;
      this.Tag = title.ToLower();
      this.Icon = Utils.GetIcon("IcTagLine");
    }

    public bool IsAddTag { get; private set; }

    public bool IsAllTag { get; set; }

    public Geometry Icon { get; set; }

    public double IconWidth { get; set; } = 15.0;

    public bool Highlighted => this.Selected || this.ShowPartSelected;

    public bool ShowPartSelected
    {
      get
      {
        if (this.Selected)
          return false;
        if (this.PartSelected)
          return true;
        return !this._isOpen && this.IsParent && this._childSelected;
      }
    }

    public string Parent { get; set; }

    public bool IsSplit { get; set; }

    public string Title { get; set; }

    public List<TagDisplayViewModel> Children { get; set; }

    public bool Focused
    {
      get => this._focused;
      set
      {
        this._focused = value;
        this.OnPropertyChanged(nameof (Focused));
      }
    }

    public bool IsSubTag
    {
      get => this._isSubTag;
      set
      {
        this._isSubTag = value;
        this.OnPropertyChanged(nameof (IsSubTag));
      }
    }

    public bool IsParent
    {
      get => this._isParent;
      set
      {
        this._isParent = value;
        this.OnPropertyChanged(nameof (IsParent));
        this.OnPropertyChanged("ShowPartSelected");
        this.OnPropertyChanged("Highlighted");
      }
    }

    public bool PartSelected
    {
      get => this._partSelected;
      set
      {
        this._partSelected = value;
        this.OnPropertyChanged("ShowPartSelected");
        this.OnPropertyChanged("Highlighted");
      }
    }

    public bool ChildSelected
    {
      get => this._childSelected;
      set
      {
        this._childSelected = value;
        this.OnPropertyChanged("ShowPartSelected");
        this.OnPropertyChanged("Highlighted");
      }
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
        this.OnPropertyChanged("Highlighted");
      }
    }

    public bool IsOpen
    {
      get => this._isOpen;
      set
      {
        this._isOpen = value;
        this.OnPropertyChanged(nameof (IsOpen));
        this.OnPropertyChanged("ShowPartSelected");
        this.OnPropertyChanged("Highlighted");
      }
    }

    public bool Collapsed
    {
      get => this._collapsed;
      set
      {
        this._collapsed = value;
        this.OnPropertyChanged(nameof (Collapsed));
      }
    }

    public static TagDisplayViewModel BuildAddTag(string tag, bool selected = false)
    {
      return new TagDisplayViewModel()
      {
        Title = string.Format(Utils.GetString("CreateTag"), (object) tag),
        Selected = false,
        IsAddTag = true,
        Tag = tag,
        _selected = selected,
        Icon = Utils.GetIcon("IcCreateTag"),
        IconWidth = 18.0
      };
    }

    public static TagDisplayViewModel BuildAllTag(bool selected = true)
    {
      return new TagDisplayViewModel()
      {
        Title = Utils.GetString("AllTags"),
        IsAllTag = true,
        Tag = "*tag",
        Selected = selected,
        Icon = Utils.GetIcon("IcAllTag"),
        IconWidth = 18.0
      };
    }

    public static TagDisplayViewModel BuildNoTag(bool selected = false)
    {
      return new TagDisplayViewModel()
      {
        Title = Utils.GetString("NoTags"),
        Tag = "!tag",
        Selected = selected,
        Icon = Utils.GetIcon("IcNoTag")
      };
    }

    public TagDisplayViewModel Clone() => (TagDisplayViewModel) this.MemberwiseClone();
  }
}
