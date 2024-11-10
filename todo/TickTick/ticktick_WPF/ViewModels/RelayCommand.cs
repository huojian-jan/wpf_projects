// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.RelayCommand
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class RelayCommand : ICommand
  {
    private Action<object> _action;

    public RelayCommand(Action<object> action) => this._action = action;

    public virtual bool CanExecute(object parameter) => true;

    public event EventHandler CanExecuteChanged;

    public virtual void Execute(object parameter)
    {
      Action<object> action = this._action;
      if (action == null)
        return;
      action(parameter);
    }
  }
}
