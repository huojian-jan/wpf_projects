// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.LoginBindThirdViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public class LoginBindThirdViewModel : BaseViewModel
  {
    private string _password;
    private string _passwordComfirm;
    private bool _passwordInvaild;
    private bool _passwordNotSame;
    private bool _networkBroken;
    private bool _notUploading;

    public string Password
    {
      get => this._password;
      set
      {
        this._password = value;
        this.OnPropertyChanged(nameof (Password));
      }
    }

    public string PasswordComfirm
    {
      get => this._passwordComfirm;
      set
      {
        this._passwordComfirm = value;
        this.OnPropertyChanged(nameof (PasswordComfirm));
      }
    }

    public string Email { get; set; }

    public string ComfirmTitle => Utils.GetString("LoginThirdRegisteredTitle");

    public string ComfirmInfo
    {
      get => string.Format(Utils.GetString("LoginThirdRegisteredInfo"), (object) this.Email);
    }

    public bool PasswordInvaild
    {
      get => this._passwordInvaild;
      set
      {
        this._passwordInvaild = value;
        this.OnPropertyChanged(nameof (PasswordInvaild));
      }
    }

    public bool PasswordNotSame
    {
      get => this._passwordNotSame;
      set
      {
        this._passwordNotSame = value;
        this.OnPropertyChanged(nameof (PasswordNotSame));
      }
    }

    public bool NetworkBroken
    {
      get => this._networkBroken;
      set
      {
        this._networkBroken = value;
        this.OnPropertyChanged(nameof (NetworkBroken));
      }
    }

    public bool NotUploading
    {
      get => this._notUploading;
      set
      {
        this._notUploading = value;
        this.OnPropertyChanged(nameof (NotUploading));
      }
    }
  }
}
