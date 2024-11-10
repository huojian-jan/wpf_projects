// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.CompleteLineDisplaySettingsPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections;
using System.Windows.Controls;
using ticktick_WPF.Framework.Collections;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class CompleteLineDisplaySettingsPanel : DisplaySettingsPanel
  {
    public CompleteLineDisplaySettingsPanel()
    {
      this.Title.Text = Utils.GetString("CompletedTaskStyle");
      ItemsControl setNumItems = this.SetNumItems;
      ExtObservableCollection<DisplaySettingsViewModel> observableCollection = new ExtObservableCollection<DisplaySettingsViewModel>();
      observableCollection.Add(new DisplaySettingsViewModel(0, "HideCompleteLine", Utils.GetString("Standard"))
      {
        Selected = LocalSettings.Settings.ExtraSettings.ShowCompleteLine == 0
      });
      observableCollection.Add(new DisplaySettingsViewModel(1, "ShowCompleteLine", Utils.GetString("Strikethrough"))
      {
        Selected = LocalSettings.Settings.ExtraSettings.ShowCompleteLine == 1
      });
      setNumItems.ItemsSource = (IEnumerable) observableCollection;
    }

    protected override void OnModelSelected(DisplaySettingsViewModel model)
    {
      LocalSettings.Settings.ShowCompleteLine = model.Type;
      DataChangedNotifier.NotifyShowCompleteLineChanged();
    }
  }
}
