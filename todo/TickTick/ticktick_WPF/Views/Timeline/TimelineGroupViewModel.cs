// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineGroupViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ticktick_WPF.Dal;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineGroupViewModel : TimelineDisplayBase
  {
    private double _height;
    private string _title;
    private bool _isOpen;
    private bool _editing;
    private bool _isArrangeOpen = true;
    private Visibility _showArrow;
    private string _avatarUrl;
    private TimelineViewModel _parent;

    public TimelineGroupViewModel(TimelineViewModel parent)
    {
      this._parent = parent ?? TimelineViewModel.Instance;
    }

    public string CatId { get; set; }

    public string ProjectId { get; set; }

    public string Id { get; set; }

    public int Line { get; set; }

    public long SortOrder { get; set; }

    public string AvatarUrl
    {
      get => this._avatarUrl ?? string.Empty;
      set
      {
        this._avatarUrl = value;
        this.OnPropertyChanged(nameof (AvatarUrl));
        this.SetAvatar();
      }
    }

    public BitmapImage Avatar { get; set; }

    private async void SetAvatar()
    {
      TimelineGroupViewModel timelineGroupViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(timelineGroupViewModel.AvatarUrl);
      timelineGroupViewModel.Avatar = avatarByUrlAsync;
      timelineGroupViewModel.OnPropertyChanged("Avatar");
    }

    public double Height
    {
      get => this._height;
      set
      {
        this._height = value;
        this.OnPropertyChanged(nameof (Height));
      }
    }

    public override string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public bool IsOpen
    {
      get => this._isOpen;
      set
      {
        this._isOpen = value;
        this.Angle = value ? 0.0 : -90.0;
        this.OnPropertyChanged("Angle");
        this.OnPropertyChanged(nameof (IsOpen));
        this.OnPropertyChanged("Data");
      }
    }

    public bool Editing
    {
      get => this._editing;
      set
      {
        if (this._editing != value)
          this._parent?.SetGroupEditing(value);
        this._editing = value;
        this.OnPropertyChanged(nameof (Editing));
      }
    }

    public bool IsArrangeOpen
    {
      get => this._isArrangeOpen;
      set
      {
        this._isArrangeOpen = value;
        this.ArrangeAngle = value ? 0.0 : -90.0;
        this.OnPropertyChanged("ArrangeAngle");
        this.OnPropertyChanged(nameof (IsArrangeOpen));
        this.OnPropertyChanged("Data");
      }
    }

    public Visibility ShowArrow
    {
      get => this._showArrow;
      set
      {
        this._showArrow = value;
        this.OnPropertyChanged(nameof (ShowArrow));
      }
    }

    public double Angle { get; set; }

    public double ArrangeAngle { get; set; }

    public TimelineViewModel Parent { get; set; }

    public bool Editable { get; set; }

    public bool IsAvatar { get; set; }

    public async Task UpdateIsOpen(bool isOpen) => await this.UpdateOpen(isOpen, false);

    public async Task UpdateIsArrangeOpen(bool isOpen) => await this.UpdateOpen(isOpen, true);

    private async Task UpdateOpen(bool isOpen, bool isArrange)
    {
      if (isArrange)
        this.IsArrangeOpen = isOpen;
      else
        this.IsOpen = isOpen;
      if (isOpen)
        await SectionStatusDao.OpenProjectSection((isArrange ? "Arrange_" : string.Empty) + this.CatId, this.Id);
      else
        await SectionStatusDao.CloseProjectSection((isArrange ? "Arrange_" : string.Empty) + this.CatId, this.Id);
      if (isArrange)
        await this.Parent.UpdateSortArrangeAsync(false);
      else
        await this.Parent.UpdateCellLineAsync();
    }

    public async Task ToggleOpen(bool isOpen)
    {
      if (isOpen == this.IsOpen)
        return;
      this.IsOpen = isOpen;
      if (isOpen)
        await SectionStatusDao.OpenProjectSection(this.CatId, this.Id);
      else
        await SectionStatusDao.CloseProjectSection(this.CatId, this.Id);
    }
  }
}
