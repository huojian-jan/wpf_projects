// Decompiled with JetBrains decompiler
// Type: ObfuscationAttribute
// Assembly: ShadowBot.Shell, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 457F80CA-4ECD-4EAF-8CD8-6496B2C0756F
// Assembly location: C:\Program Files\ShadowBot\shadowbot-5.22.49\ShadowBot.Shell.dll

using pRX2r9p7wQ0K4WPhEW;
using System;
using System.Runtime.CompilerServices;

#nullable disable
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
public sealed class ObfuscationAttribute : Attribute
{
  private bool m_applyToMembers;
  private bool m_exclude;
  private string m_feature;
  private bool m_strip;

  public bool ApplyToMembers
  {
    [MethodImpl(MethodImplOptions.NoInlining)] get => true;
    [MethodImpl(MethodImplOptions.NoInlining)] set
    {
    }
  }

  public bool Exclude
  {
    [MethodImpl(MethodImplOptions.NoInlining)] get => true;
    [MethodImpl(MethodImplOptions.NoInlining)] set
    {
    }
  }

  public string Feature
  {
    [MethodImpl(MethodImplOptions.NoInlining)] get => (string) null;
    [MethodImpl(MethodImplOptions.NoInlining)] set
    {
    }
  }

  public bool StripAfterObfuscation
  {
    [MethodImpl(MethodImplOptions.NoInlining)] get => true;
    [MethodImpl(MethodImplOptions.NoInlining)] set
    {
    }
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  public ObfuscationAttribute()
  {
  }

  static ObfuscationAttribute() => vIRtpD5vF7kQ7qvOEa.WUGVB3or3P();
}
