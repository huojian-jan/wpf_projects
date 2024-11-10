// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.AppOptionItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class AppOptionItem : MenuItem, IComponentConnector
  {
    public static readonly DependencyProperty IconDependencyProperty = DependencyProperty.Register(nameof (Icon), typeof (ImageSource), typeof (AppOptionItem), new PropertyMetadata((object) null, new PropertyChangedCallback(AppOptionItem.OnDataChangeCallback)));
    public static readonly DependencyProperty TitleDependencyProperty = DependencyProperty.Register(nameof (Title), typeof (string), typeof (AppOptionItem), new PropertyMetadata((object) string.Empty, (PropertyChangedCallback) null));
    public static readonly DependencyProperty ShortcutDependencyProperty = DependencyProperty.Register(nameof (Shortcut), typeof (string), typeof (AppOptionItem), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(AppOptionItem.OnShortCutChanged)));
    internal AppOptionItem Root;
    internal Grid IconGrid;
    internal Image IconImage;
    internal TextBlock ShortcutText;
    private bool _contentLoaded;

    public ImageSource Icon
    {
      get => (ImageSource) this.GetValue(AppOptionItem.IconDependencyProperty);
      set => this.SetValue(AppOptionItem.IconDependencyProperty, (object) value);
    }

    public string Title
    {
      get => (string) this.GetValue(AppOptionItem.TitleDependencyProperty);
      set => this.SetValue(AppOptionItem.TitleDependencyProperty, (object) value);
    }

    public string Shortcut
    {
      get => (string) this.GetValue(AppOptionItem.ShortcutDependencyProperty);
      set => this.SetValue(AppOptionItem.ShortcutDependencyProperty, (object) value);
    }

    private static void OnShortCutChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is AppOptionItem appOptionItem) || e.NewValue == null)
        return;
      appOptionItem.ShortcutText.Visibility = Visibility.Visible;
    }

    public AppOptionItem() => this.InitializeComponent();

    private static void OnDataChangeCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is AppOptionItem appOptionItem) || e.NewValue == null)
        return;
      appOptionItem.IconImage.Source = (ImageSource) e.NewValue;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/appoptionitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (AppOptionItem) target;
          break;
        case 2:
          this.IconGrid = (Grid) target;
          break;
        case 3:
          this.IconImage = (Image) target;
          break;
        case 4:
          this.ShortcutText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
