// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.ComboBoxViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public class ComboBoxViewModel : UpDownSelectViewModel
  {
    private string _title;
    private Geometry _image;
    private bool _showImage;
    private double _itemHeight = 36.0;
    public bool CanSelect = true;
    public object Value;

    public double ItemHeight
    {
      get => this._itemHeight;
      set
      {
        this._itemHeight = value;
        this.OnPropertyChanged(nameof (ItemHeight));
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

    public Geometry Image
    {
      get => this._image;
      set
      {
        this._image = value;
        this.OnPropertyChanged(nameof (Image));
      }
    }

    public bool ShowImage
    {
      get => this._showImage;
      set
      {
        this._showImage = value;
        this.OnPropertyChanged(nameof (ShowImage));
      }
    }

    public ComboBoxViewModel()
    {
    }

    public ComboBoxViewModel(object value, string title, double height = 36.0)
    {
      this.Value = value;
      this.Title = title;
      this.ItemHeight = height;
    }
  }
}
