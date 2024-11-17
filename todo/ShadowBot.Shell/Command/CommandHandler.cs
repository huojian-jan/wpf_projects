// Decompiled with JetBrains decompiler
// Type: ShadowBot.Shell.Command.CommandHandler
// Assembly: ShadowBot.Shell, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 457F80CA-4ECD-4EAF-8CD8-6496B2C0756F
// Assembly location: C:\Program Files\ShadowBot\shadowbot-5.22.49\ShadowBot.Shell.dll

using pRX2r9p7wQ0K4WPhEW;
using System.Runtime.CompilerServices;

#nullable disable
namespace ShadowBot.Shell.Command
{
  public abstract class CommandHandler
  {
    public abstract bool Handler(string[] args);

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected bool CheckCommandLineArgsAndSetCulture(string[] args, string keyWords) => true;

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected CommandHandler()
    {
    }

    static CommandHandler() => vIRtpD5vF7kQ7qvOEa.WUGVB3or3P();
  }
}
