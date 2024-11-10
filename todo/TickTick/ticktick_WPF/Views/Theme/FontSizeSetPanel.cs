// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.FontSizeSetPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class FontSizeSetPanel : UserControl, IComponentConnector, IStyleConnector
  {
    internal ItemsControl FontSizeItems;
    private bool _contentLoaded;

    public FontSizeSetPanel()
    {
      this.InitializeComponent();
      this.FontSizeItems.ItemsSource = (IEnumerable) FontSizeViewModel.BuildModels();
    }

    private async void OnFontSizeSelect(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is FontSizeViewModel dataContext))
        return;
      this.TrySetFontSize(dataContext);
    }

    private async Task TrySetFontSize(FontSizeViewModel model)
    {
      if (this.FontSizeItems.ItemsSource is ObservableCollection<FontSizeViewModel> itemsSource)
      {
        foreach (FontSizeViewModel fontSizeViewModel in (Collection<FontSizeViewModel>) itemsSource)
        {
          if (fontSizeViewModel.Selected && fontSizeViewModel.Size != model.Size)
            fontSizeViewModel.Selected = false;
        }
      }
      model.Selected = true;
      LocalSettings.Settings.BaseFontSize = model.Size;
      App.SetFontSize();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/theme/fontsizesetpanel.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.FontSizeItems = (ItemsControl) target;
      else
        this._contentLoaded = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFontSizeSelect);
    }
  }
}
