// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoSoundViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoSoundViewModel : BaseViewModel
  {
    private bool _selected;
    private bool _downloaded;
    private bool _downloading;

    public string Key { get; set; }

    public Geometry Data { get; set; }

    public string Name { get; set; }

    public double Percent { get; set; }

    public event EventHandler<string> NotifyDownLoadCompleted;

    public bool NeedPro { get; set; }

    public PomoSoundViewModel(string key)
    {
      this.Key = key;
      string soundIndex = FocusConstance.GetSoundIndex(key);
      this.Data = Utils.GetIconData("Ic" + soundIndex);
      this.Name = Utils.GetString(soundIndex);
      this.Downloaded = key == "none" || System.IO.File.Exists(AppPaths.PomoSoundDir + this.Key + ".ogg");
      if (key == "Clock" && !this.Downloaded)
      {
        if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + this.Key + ".ogg"))
        {
          try
          {
            if (!Directory.Exists(AppPaths.PomoSoundDir))
              Directory.CreateDirectory(AppPaths.PomoSoundDir);
            System.IO.File.Copy(AppDomain.CurrentDomain.BaseDirectory + this.Key + ".ogg", AppPaths.PomoSoundDir + this.Key + ".ogg");
            this.Downloaded = true;
          }
          catch (Exception ex)
          {
          }
        }
      }
      this.Selected = this.Downloaded && LocalSettings.Settings.PomoSound?.ToLower() == this.Key.ToLower();
      this.NeedPro = key != "none" && key != "Clock";
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public bool Downloaded
    {
      get => this._downloaded;
      set
      {
        this._downloaded = value;
        this.OnPropertyChanged(nameof (Downloaded));
        this.OnPropertyChanged("NeedDownload");
      }
    }

    public bool Downloading
    {
      get => this._downloading;
      set
      {
        this._downloading = value;
        this.OnPropertyChanged(nameof (Downloading));
        this.OnPropertyChanged("NeedDownload");
      }
    }

    public bool NeedDownload => !this.Downloaded && !this.Downloading;

    public async void TryDownload()
    {
      PomoSoundViewModel pomoSoundViewModel = this;
      WebClient webClient = new WebClient();
      try
      {
        if (!Directory.Exists(AppPaths.PomoSoundDir))
          Directory.CreateDirectory(AppPaths.PomoSoundDir);
        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(pomoSoundViewModel.OnSoundDownLoaded);
        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(pomoSoundViewModel.OnSoundDownLoading);
        await webClient.DownloadFileTaskAsync(PomoSoundViewModel.GetRemotePath() + pomoSoundViewModel.Key + ".ogg", AppPaths.PomoSoundDir + pomoSoundViewModel.Key + ".ogg");
      }
      catch (Exception ex)
      {
      }
    }

    private void OnSoundDownLoading(object sender, DownloadProgressChangedEventArgs e)
    {
      this.Downloading = true;
      this.Percent = (double) (e.ProgressPercentage * 100);
      this.OnPropertyChanged("Angle");
    }

    private static string GetRemotePath()
    {
      return "https://" + BaseUrl.GetPullDomain() + "/common/pomodoro/windows/";
    }

    private void OnSoundDownLoaded(object sender, AsyncCompletedEventArgs e)
    {
      if (!System.IO.File.Exists(AppPaths.PomoSoundDir + this.Key + ".ogg"))
        return;
      FileInfo fileInfo = new FileInfo(AppPaths.PomoSoundDir + this.Key + ".ogg");
      if (fileInfo.Length > 0L)
      {
        this.Downloaded = true;
        EventHandler<string> downLoadCompleted = this.NotifyDownLoadCompleted;
        if (downLoadCompleted != null)
          downLoadCompleted((object) this, this.Key);
      }
      else
      {
        try
        {
          fileInfo.Attributes = FileAttributes.Normal;
          fileInfo.Delete();
        }
        catch (Exception ex)
        {
        }
      }
      this.Downloading = false;
    }
  }
}
