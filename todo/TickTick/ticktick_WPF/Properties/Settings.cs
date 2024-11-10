// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Properties.Settings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#nullable disable
namespace ticktick_WPF.Properties
{
  [CompilerGenerated]
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.4.0.0")]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
    {
    }

    private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
    {
    }

    public static Settings Default => Settings.defaultInstance;

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("False")]
    public bool NeedRestart
    {
      get => (bool) this[nameof (NeedRestart)];
      set => this[nameof (NeedRestart)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("")]
    public string ApiTest
    {
      get => (string) this[nameof (ApiTest)];
      set => this[nameof (ApiTest)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("0")]
    public long LastPullHolidayTime
    {
      get => (long) this[nameof (LastPullHolidayTime)];
      set => this[nameof (LastPullHolidayTime)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("0")]
    public long LastWindowActiveTime
    {
      get => (long) this[nameof (LastWindowActiveTime)];
      set => this[nameof (LastWindowActiveTime)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("")]
    public string YearPromoClosedUsers
    {
      get => (string) this[nameof (YearPromoClosedUsers)];
      set => this[nameof (YearPromoClosedUsers)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("")]
    public string YearPromo
    {
      get => (string) this[nameof (YearPromo)];
      set => this[nameof (YearPromo)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("0")]
    public long ActiveTime
    {
      get => (long) this[nameof (ActiveTime)];
      set => this[nameof (ActiveTime)] = (object) value;
    }

    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("0")]
    public int DbVersion => (int) this[nameof (DbVersion)];
  }
}
