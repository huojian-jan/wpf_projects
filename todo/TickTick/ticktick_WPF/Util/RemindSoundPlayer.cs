// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.RemindSoundPlayer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using ticktick_WPF.Resource;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public class RemindSoundPlayer
  {
    private static WaveOut WaveOut;
    private static SoundPlayer Player;
    private static Dictionary<string, string> _remindSoundFileName = new Dictionary<string, string>()
    {
      {
        "MusicBox",
        "music_box.mp3"
      },
      {
        "Beep",
        "beep.mp3"
      },
      {
        "Leap",
        "leap.mp3"
      },
      {
        "Harp",
        "harp.mp3"
      },
      {
        "Ladder",
        "ladder.mp3"
      },
      {
        "Crystal",
        "crystal.mp3"
      },
      {
        "Matrix",
        "matrix.mp3"
      },
      {
        "Chimes",
        "chimes.mp3"
      },
      {
        "Drum",
        "africa.mp3"
      },
      {
        "Blocks",
        "blocks.mp3"
      },
      {
        "Lattice",
        "lattice.mp3"
      },
      {
        "Pulse",
        "pulse.mp3"
      }
    };

    public static string GetSoundFileName(string sound)
    {
      return !RemindSoundPlayer._remindSoundFileName.ContainsKey(sound) ? (string) null : RemindSoundPlayer._remindSoundFileName[sound];
    }

    public static void PlayTaskRemindSound(bool isTry)
    {
      string sound = LocalSettings.Settings.ExtraSettings.RemindSound;
      if (string.IsNullOrEmpty(sound))
        sound = LocalSettings.Settings.EnableRingtone ? "Harp" : "None";
      if (!Utils.IsWindows7() && !LocalSettings.Settings.ShowReminderInClient && sound == "Harp")
      {
        if (!isTry)
          return;
        string fileName = Environment.GetEnvironmentVariable("WINDIR") + "\\Media\\Windows Notify System Generic.wav";
        if (!File.Exists(fileName))
          return;
        Application.Current?.Dispatcher?.Invoke((Action) (() => new SoundPlayer(fileName).Play()));
      }
      else
        Application.Current?.Dispatcher?.Invoke((Action) (() => RemindSoundPlayer.PlayRemindSound(sound, isTry)));
    }

    public static void PlayFocusRemindSound(bool isBreak, bool trial = false)
    {
      string sound = isBreak ? LocalSettings.Settings.PomoLocalSetting.BreakEndSound : LocalSettings.Settings.PomoLocalSetting.FocusEndSound;
      if (string.IsNullOrEmpty(sound))
        sound = LocalSettings.Settings.EnableRingtone ? "MusicBox" : "None";
      RemindSoundPlayer.PlayRemindSound(sound, trial);
    }

    private static void PlayRemindSound(string sound, bool trial = false)
    {
      try
      {
        UtilLog.Info(string.Format("PlayRemindSound {0} {1}", (object) sound, (object) trial));
        RemindSoundPlayer.Stop();
        if (sound == "None")
          return;
        string str = AppDomain.CurrentDomain.BaseDirectory + (sound == RemindSound.Spiral.ToString() ? "completion_sound_spiral.wav" : "RemindSound\\" + RemindSoundPlayer.GetSoundFileName(sound));
        if (!File.Exists(str))
          return;
        if (str.EndsWith("wav"))
        {
          RemindSoundPlayer.Player = new SoundPlayer()
          {
            SoundLocation = str
          };
          RemindSoundPlayer.Player.Play();
        }
        else
        {
          Mp3FileReader mp3FileReader = new Mp3FileReader(str);
          RemindSoundPlayer.WaveOut = new WaveOut();
          RemindSoundPlayer.WaveOut.Init((IWaveProvider) mp3FileReader);
          RemindSoundPlayer.WaveOut.Play();
        }
      }
      catch (Exception ex)
      {
        UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
      }
    }

    public static void Stop()
    {
      try
      {
        Application.Current?.Dispatcher?.Invoke((Action) (() =>
        {
          if (RemindSoundPlayer.Player != null)
          {
            RemindSoundPlayer.Player.Stop();
            RemindSoundPlayer.Player.Dispose();
            RemindSoundPlayer.Player = (SoundPlayer) null;
          }
          if (RemindSoundPlayer.WaveOut == null)
            return;
          RemindSoundPlayer.WaveOut.Stop();
          RemindSoundPlayer.WaveOut.Dispose();
          RemindSoundPlayer.WaveOut = (WaveOut) null;
        }));
      }
      catch
      {
      }
    }
  }
}
