// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoSoundPlayer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.IO;
using System.Media;
using System.Timers;
using System.Windows;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoSoundPlayer
  {
    private static OggDecodeStream _stream;
    private static string _currentSound = "none";
    private static SoundPlayer _soundPlayer;
    private static readonly Timer _soundTimer = new Timer(5000.0);
    private static bool _trial;

    public static void TryPlaySound(bool trial)
    {
      PomoSoundPlayer._trial = trial;
      string soundPath = AppPaths.PomoSoundDir + LocalSettings.Settings.PomoSound + ".ogg";
      if (!File.Exists(soundPath))
        LocalSettings.Settings.PomoSound = "none";
      else
        Application.Current?.Dispatcher?.Invoke(new Action(PlaySound));

      void PlaySound()
      {
        try
        {
          string path = AppPaths.PomoSoundDir + LocalSettings.Settings.PomoSound + ".wav";
          if (!File.Exists(path) || PomoSoundPlayer._currentSound != LocalSettings.Settings.PomoSound)
          {
            OggWavConverter.Convert((Stream) new FileStream(soundPath, FileMode.Open, FileAccess.Read), LocalSettings.Settings.PomoSound);
            PomoSoundPlayer._currentSound = LocalSettings.Settings.PomoSound;
          }
          PomoSoundPlayer.StopPlaySound();
          if (File.Exists(path))
          {
            PomoSoundPlayer._soundPlayer = new SoundPlayer()
            {
              SoundLocation = path
            };
            PomoSoundPlayer._soundPlayer.PlayLooping();
          }
          PomoSoundHelper.SetPomoSoundPlaying(true);
          if (!PomoSoundPlayer._trial)
            return;
          PomoSoundPlayer.StopInSeconds();
        }
        catch (Exception ex)
        {
          UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
        }
      }
    }

    private static void StopInSeconds()
    {
      PomoSoundPlayer._soundTimer.Stop();
      PomoSoundPlayer._soundTimer.Elapsed -= new ElapsedEventHandler(PomoSoundPlayer.TryStopSound);
      PomoSoundPlayer._soundTimer.Elapsed += new ElapsedEventHandler(PomoSoundPlayer.TryStopSound);
      PomoSoundPlayer._soundTimer.Start();
    }

    public static void StartPlaySound(bool trial)
    {
      if (LocalSettings.Settings.PomoSound != "none")
      {
        try
        {
          PomoSoundPlayer.TryPlaySound(trial);
        }
        catch (Exception ex)
        {
        }
      }
      else
        PomoSoundPlayer.StopPlaySound();
    }

    public static void StopPlaySound()
    {
      SoundPlayer soundPlayer = PomoSoundPlayer._soundPlayer;
      PomoSoundPlayer._soundPlayer = (SoundPlayer) null;
      if (soundPlayer != null)
      {
        soundPlayer.Stop();
        soundPlayer.Dispose();
      }
      PomoSoundHelper.SetPomoSoundPlaying(false);
    }

    private static void TryStopSound(object sender, ElapsedEventArgs e)
    {
      Utils.RunOnUiThread(Application.Current?.Dispatcher, new Action(PomoSoundPlayer.TryStopSound));
    }

    public static void TryStopSound()
    {
      PomoSoundPlayer._soundTimer.Stop();
      if (PomoSoundPlayer._trial)
        PomoSoundPlayer.StopPlaySound();
      PomoSoundPlayer._trial = false;
    }
  }
}
