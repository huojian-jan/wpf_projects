// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.NumDisplaySettingsPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections;
using System.Windows.Controls;
using ticktick_WPF.Cache;
using ticktick_WPF.Framework.Collections;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class NumDisplaySettingsPanel : DisplaySettingsPanel
  {
    public NumDisplaySettingsPanel()
    {
      this.Title.Text = Utils.GetString("SidebarCount");
      ItemsControl setNumItems = this.SetNumItems;
      ExtObservableCollection<DisplaySettingsViewModel> observableCollection = new ExtObservableCollection<DisplaySettingsViewModel>();
      observableCollection.Add(new DisplaySettingsViewModel(0, "ShowNum", Utils.GetString("ShowAll"))
      {
        Selected = LocalSettings.Settings.ExtraSettings.NumDisplayType == 0
      });
      observableCollection.Add(new DisplaySettingsViewModel(1, "HideNoteNum", Utils.GetString("HideNoteNum"))
      {
        Selected = LocalSettings.Settings.ExtraSettings.NumDisplayType == 1
      });
      observableCollection.Add(new DisplaySettingsViewModel(2, "HideNum", Utils.GetString("HideAll"))
      {
        Selected = LocalSettings.Settings.ExtraSettings.NumDisplayType == 2
      });
      setNumItems.ItemsSource = (IEnumerable) observableCollection;
    }

    protected override void OnModelSelected(DisplaySettingsViewModel model)
    {
      LocalSettings.Settings.ExtraSettings.NumDisplayType = model.Type;
      TaskCountCache.SetNeedLoad();
      LocalSettings.Settings.NotifyPropertyChanged("ProjectNum");
    }
  }
}
