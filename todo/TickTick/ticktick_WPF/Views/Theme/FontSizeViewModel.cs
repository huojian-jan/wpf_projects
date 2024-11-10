// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.FontSizeViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.ObjectModel;
using ticktick_WPF.Framework.Collections;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class FontSizeViewModel : BaseViewModel
  {
    private bool _selected;

    public string Title { get; set; }

    public int Size { get; set; }

    public bool Selected
    {
      get => this._selected;
      set
      {
        if (this._selected == value)
          return;
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    private FontSizeViewModel(int size, string title)
    {
      this.Size = size;
      this.Title = title;
      this.Selected = LocalSettings.Settings.BaseFontSize == size;
    }

    public static ObservableCollection<FontSizeViewModel> BuildModels()
    {
      ExtObservableCollection<FontSizeViewModel> observableCollection = new ExtObservableCollection<FontSizeViewModel>();
      observableCollection.Add(new FontSizeViewModel(14, Utils.GetString("Normal"))
      {
        Selected = LocalSettings.Settings.BaseFontSize == 14 || LocalSettings.Settings.BaseFontSize == 0
      });
      observableCollection.Add(new FontSizeViewModel(16, Utils.GetString("Large")));
      observableCollection.Add(new FontSizeViewModel(18, Utils.GetString("ExtraLarge")));
      return (ObservableCollection<FontSizeViewModel>) observableCollection;
    }
  }
}
