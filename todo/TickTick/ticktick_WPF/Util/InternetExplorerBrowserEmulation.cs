// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.InternetExplorerBrowserEmulation
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Win32;
using System;
using System.IO;
using System.Security;

#nullable disable
namespace ticktick_WPF.Util
{
  public class InternetExplorerBrowserEmulation
  {
    private const string InternetExplorerRootKey = "Software\\Microsoft\\Internet Explorer";
    private const string BrowserEmulationKey = "Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION";
    private static BrowserEmulationVersion _browserVersion;

    private static int GetInternetExplorerMajorVersion()
    {
      int result = 0;
      try
      {
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Internet Explorer");
        if (registryKey != null)
        {
          object obj = registryKey.GetValue("svcVersion", (object) null) ?? registryKey.GetValue("Version", (object) null);
          if (obj != null)
          {
            string str = obj.ToString();
            int length = str.IndexOf('.');
            if (length != -1)
              int.TryParse(str.Substring(0, length), out result);
          }
        }
      }
      catch (SecurityException ex)
      {
        return 10;
      }
      catch (UnauthorizedAccessException ex)
      {
        return 10;
      }
      return result;
    }

    private static bool SetBrowserEmulationVersion(BrowserEmulationVersion browserEmulationVersion)
    {
      bool flag = false;
      try
      {
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
        if (registryKey != null)
        {
          string fileName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
          if (browserEmulationVersion != BrowserEmulationVersion.Default)
            registryKey.SetValue(fileName, (object) (int) browserEmulationVersion, RegistryValueKind.DWord);
          else if (fileName != null)
            registryKey.DeleteValue(fileName, false);
          flag = true;
        }
      }
      catch (SecurityException ex)
      {
      }
      catch (UnauthorizedAccessException ex)
      {
      }
      return flag;
    }

    public static bool SetBrowserEmulationVersion()
    {
      int explorerMajorVersion = InternetExplorerBrowserEmulation.GetInternetExplorerMajorVersion();
      BrowserEmulationVersion browserEmulationVersion;
      if (explorerMajorVersion >= 11)
      {
        browserEmulationVersion = BrowserEmulationVersion.Version11;
      }
      else
      {
        switch (explorerMajorVersion - 8)
        {
          case 0:
            browserEmulationVersion = BrowserEmulationVersion.Version8;
            break;
          case 1:
            browserEmulationVersion = BrowserEmulationVersion.Version9;
            break;
          case 2:
            browserEmulationVersion = BrowserEmulationVersion.Version10;
            break;
          default:
            browserEmulationVersion = BrowserEmulationVersion.Version7;
            break;
        }
      }
      InternetExplorerBrowserEmulation._browserVersion = browserEmulationVersion;
      return InternetExplorerBrowserEmulation.SetBrowserEmulationVersion(browserEmulationVersion);
    }

    private static BrowserEmulationVersion GetBrowserEmulationVersion()
    {
      BrowserEmulationVersion emulationVersion = BrowserEmulationVersion.Default;
      try
      {
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
        if (registryKey != null)
        {
          string fileName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
          object obj = registryKey.GetValue(fileName, (object) null);
          if (obj != null)
            emulationVersion = (BrowserEmulationVersion) Convert.ToInt32(obj);
        }
      }
      catch (SecurityException ex)
      {
      }
      catch (UnauthorizedAccessException ex)
      {
      }
      InternetExplorerBrowserEmulation._browserVersion = emulationVersion;
      return emulationVersion;
    }

    public static bool IsBrowserEmulationSet()
    {
      return InternetExplorerBrowserEmulation.GetBrowserEmulationVersion() >= BrowserEmulationVersion.Version10;
    }

    public static bool IsVersion11()
    {
      return InternetExplorerBrowserEmulation._browserVersion >= BrowserEmulationVersion.Version11;
    }
  }
}
