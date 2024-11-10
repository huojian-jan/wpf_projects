// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.DisplaySettingsPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Notifier;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class DisplaySettingsPanel : UserControl, IComponentConnector, IStyleConnector
  {
    internal TextBlock Title;
    internal ItemsControl SetNumItems;
    private bool _contentLoaded;

    public DisplaySettingsPanel()
    {
      this.InitializeComponent();
      DataChangedNotifier.IsDarkChanged += new EventHandler(this.OnIsDarkChanged);
      this.Unloaded += (RoutedEventHandler) ((o, e) => DataChangedNotifier.IsDarkChanged -= new EventHandler(this.OnIsDarkChanged));
    }

    private void OnIsDarkChanged(object sender, EventArgs e)
    {
      if (!(this.SetNumItems.ItemsSource is ObservableCollection<DisplaySettingsViewModel> itemsSource))
        return;
      foreach (DisplaySettingsViewModel settingsViewModel in (Collection<DisplaySettingsViewModel>) itemsSource)
        settingsViewModel.ChangeImage();
    }

    private async void OnItemSelect(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is DisplaySettingsViewModel dataContext))
        return;
      this.TrySetNumDisplayType(dataContext);
    }

    private async Task TrySetNumDisplayType(DisplaySettingsViewModel model)
    {
      if (this.SetNumItems.ItemsSource is ObservableCollection<DisplaySettingsViewModel> itemsSource)
      {
        foreach (DisplaySettingsViewModel settingsViewModel in (Collection<DisplaySettingsViewModel>) itemsSource)
        {
          if (settingsViewModel.Selected && settingsViewModel.Type != model.Type)
            settingsViewModel.Selected = false;
        }
      }
      model.Selected = true;
      this.OnModelSelected(model);
    }

    protected virtual void OnModelSelected(DisplaySettingsViewModel model)
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/theme/displaysettingspanel.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.SetNumItems = (ItemsControl) target;
        else
          this._contentLoaded = true;
      }
      else
        this.Title = (TextBlock) target;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 3)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemSelect);
    }
  }
}
