// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.PomoSoundHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.IO;
using ticktick_WPF.Resource;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public class PomoSoundHelper
  {
    private static bool _isPomoSoundPlaying;

    public static void SetPomoSoundPlaying(bool playing)
    {
      PomoSoundHelper._isPomoSoundPlaying = playing;
    }

    public static bool IsPomoSoundPlaying() => PomoSoundHelper._isPomoSoundPlaying;

    public static void InitSetFocusSound()
    {
      if (string.IsNullOrEmpty(LocalSettings.Settings.PomoSound) || !FocusConstance.KeyIndexDict.ContainsKey(LocalSettings.Settings.PomoSound))
      {
        LocalSettings.Settings.PomoSound = string.IsNullOrEmpty(LocalSettings.Settings.PomoSound) ? "none" : "Clock";
        LocalSettings.Settings.Save();
      }
      if (!(LocalSettings.Settings.PomoSound != "none") || File.Exists(AppPaths.PomoSoundDir + LocalSettings.Settings.PomoSound + ".ogg"))
        return;
      LocalSettings.Settings.PomoSound = "none";
      LocalSettings.Settings.Save();
    }
  }
}
