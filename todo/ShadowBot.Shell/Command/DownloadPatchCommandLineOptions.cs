﻿// Decompiled with JetBrains decompiler
// Type: ShadowBot.Shell.Command.DownloadPatchCommandLineOptions
// Assembly: ShadowBot.Shell, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 457F80CA-4ECD-4EAF-8CD8-6496B2C0756F
// Assembly location: C:\Program Files\ShadowBot\shadowbot-5.22.49\ShadowBot.Shell.dll

using CommandLine;
using pRX2r9p7wQ0K4WPhEW;
using System.Runtime.CompilerServices;

#nullable disable
namespace ShadowBot.Shell.Command
{
  public class DownloadPatchCommandLineOptions
  {
    [Option("patch-version", Required = true, Default = null)]
    public string PatchVersion
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => (string) null;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("patch-url", Required = true, Default = null)]
    public string PatchUrl
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => (string) null;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [Option("installer-url", Required = true, Default = null)]
    public string InstallerUrl
    {
      [MethodImpl(MethodImplOptions.NoInlining)] get => (string) null;
      [MethodImpl(MethodImplOptions.NoInlining)] set
      {
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public DownloadPatchCommandLineOptions()
    {
    }

    static DownloadPatchCommandLineOptions() => vIRtpD5vF7kQ7qvOEa.WUGVB3or3P();
  }
}