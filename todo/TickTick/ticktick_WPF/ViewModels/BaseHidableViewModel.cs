// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.BaseHidableViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.ComponentModel;
using System.Windows;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  [Serializable]
  public class BaseHidableViewModel : INotifyPropertyChanged
  {
    private bool _isHide;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName = null)
    {
      Application.Current?.Dispatcher?.InvokeAsync((Action) (() =>
      {
        PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if (propertyChanged == null)
          return;
        propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
      }));
    }

    public bool IsHide
    {
      get => this._isHide;
      set
      {
        this._isHide = value;
        this.OnPropertyChanged(nameof (IsHide));
      }
    }

    public BaseHidableViewModel Clone() => (BaseHidableViewModel) this.MemberwiseClone();
  }
}
