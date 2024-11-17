// Decompiled with JetBrains decompiler
// Type: ShadowBot.Shell.CommandLineOptions
// Assembly: ShadowBot.Shell, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 457F80CA-4ECD-4EAF-8CD8-6496B2C0756F
// Assembly location: C:\Program Files\ShadowBot\shadowbot-5.22.49\ShadowBot.Shell.dll

using CommandLine;
using pRX2r9p7wQ0K4WPhEW;
using System.Runtime.CompilerServices;

#nullable disable
namespace ShadowBot.Shell
{
  internal class CommandLineOptions
  {
    [Option("mode", Required = false, Default = ApplicationMode.Default)]
    public ApplicationMode Mode
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => (ApplicationMode) null;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("valid-license", Required = false, Default = false)]
    public bool ValidLicense
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => true;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("version", Required = false, Default = false)]
    public bool Version
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => true;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("machine-code", Required = false, Default = false)]
    public bool MachineCode
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => true;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("license-expires", Required = false, Default = false)]
    public bool LicenseExpires
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => true;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("release", Required = false, Default = false)]
    public bool Release
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => true;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CommandLineOptions()
    {
    }

    static CommandLineOptions() => vIRtpD5vF7kQ7qvOEa.WUGVB3or3P();
  }
}
