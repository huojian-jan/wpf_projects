// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.FeatureViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  internal class FeatureViewModel : BaseViewModel
  {
    private string _settingText;

    public ExpandFeature Value { get; set; }

    public string Title { get; set; }

    public string Desc { get; set; }

    public string SettingText
    {
      get => !this.IsOpen ? (string) null : this._settingText;
      set
      {
        this._settingText = value;
        this.OnPropertyChanged(nameof (SettingText));
      }
    }

    public bool IsOpen { get; set; }

    public bool ShowOpen { get; set; } = true;

    public string Image { get; set; } = "../../Assets/ImageSource/Feature.png";

    public FeatureViewModel()
    {
    }

    public FeatureViewModel(ExpandFeature feature, bool isOpen, bool showSettings = false)
    {
      this.Value = feature;
      this.Title = this.GetTitle(feature);
      this.Desc = Utils.GetString(feature.ToString() + nameof (Desc));
      this._settingText = !showSettings ? (string) null : Utils.GetString(feature.ToString() + "Setting");
      this.IsOpen = isOpen;
      this.Image = "../../Assets/ImageSource/" + feature.ToString() + "Feature.png";
    }

    private string GetTitle(ExpandFeature feature)
    {
      if (feature == ExpandFeature.Habit)
        return Utils.GetString("HabitView");
      return feature == ExpandFeature.Focus ? Utils.GetString("PomoFocus") : Utils.GetString(feature.ToString());
    }

    public void NotifySettingTextChanged() => this.OnPropertyChanged("SettingText");
  }
}
