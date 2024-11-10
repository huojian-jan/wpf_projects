// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.CommandBase
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;

#nullable disable
namespace ticktick_WPF.Util
{
  public class CommandBase : ICommand
  {
    private readonly Action<object> _commandWithParam;
    private readonly Action _command;
    private readonly Func<bool> _canExecute;

    public CommandBase(Action command, Func<bool> canExecute = null)
    {
      this._canExecute = canExecute;
      this._command = command ?? throw new ArgumentNullException();
    }

    public CommandBase(Action<object> commandWidthParam, Func<bool> canExecute = null)
    {
      this._canExecute = canExecute;
      this._commandWithParam = commandWidthParam ?? throw new ArgumentNullException();
    }

    public bool CanExecute(object parameter) => this._canExecute == null || this._canExecute();

    public void Execute(object parameter)
    {
      if (parameter != null)
        this._commandWithParam(parameter);
      else if (this._command != null)
      {
        this._command();
      }
      else
      {
        Action<object> commandWithParam = this._commandWithParam;
        if (commandWithParam == null)
          return;
        commandWithParam((object) null);
      }
    }

    public event EventHandler CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }
  }
}
