// Decompiled with JetBrains decompiler
// Type: ShadowBot.Shell.LicenseCommandLineOptions
// Assembly: ShadowBot.Shell, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 457F80CA-4ECD-4EAF-8CD8-6496B2C0756F
// Assembly location: C:\Program Files\ShadowBot\shadowbot-5.22.49\ShadowBot.Shell.dll

using CommandLine;
using pRX2r9p7wQ0K4WPhEW;
using ShadowBot.Runtime.Security;
using System.Runtime.CompilerServices;

#nullable disable
namespace ShadowBot.Shell
{
  public class LicenseCommandLineOptions
  {
    [Option("license-kind", Required = false, Default = StandaloneLicenseKind.Robot)]
    public StandaloneLicenseKind LicenseKind
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => (StandaloneLicenseKind) null;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("license-file", Required = false, Default = null)]
    public string LicenseFile
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => (string) null;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("license-code", Required = false, Default = null)]
    public string LicenseCode
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => (string) null;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public LicenseCommandLineOptions()
    {
    }

    static LicenseCommandLineOptions() => vIRtpD5vF7kQ7qvOEa.WUGVB3or3P();
  }
}
