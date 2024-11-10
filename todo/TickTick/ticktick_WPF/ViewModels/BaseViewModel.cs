// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.BaseViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  [Serializable]
  public class BaseViewModel : INotifyPropertyChanged
  {
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

    protected void ChangeAndNotify<T>(ref T value, T newValue, [CallerMemberName] string propertyName = null)
    {
      value = newValue;
      this.OnPropertyChanged(propertyName);
    }
  }
}
