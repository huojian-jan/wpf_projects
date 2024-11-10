// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.DropIconButton
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class DropIconButton : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty ItemSelectedProperty = DependencyProperty.Register(nameof (ItemSelected), typeof (bool), typeof (DropIconButton), new PropertyMetadata((object) false));
    public static readonly DependencyProperty IndenteProperty = DependencyProperty.Register(nameof (Indent), typeof (double), typeof (DropIconButton), new PropertyMetadata((object) 0.0));
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof (IconSize), typeof (double), typeof (DropIconButton), new PropertyMetadata((object) 16.0));
    public static readonly DependencyProperty IconColorProperty = DependencyProperty.Register(nameof (IconColor), typeof (SolidColorBrush), typeof (DropIconButton), new PropertyMetadata((object) Brushes.Transparent));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof (Title), typeof (string), typeof (DropIconButton), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof (Icon), typeof (Geometry), typeof (DropIconButton), new PropertyMetadata((object) null, new PropertyChangedCallback(DropIconButton.OnDataChangeCallback)));
    internal DropIconButton Root;
    internal Path TitleIcon;
    internal EmjTextBlock TitleText;
    private bool _contentLoaded;

    public DropIconButton() => this.InitializeComponent();

    public bool ItemSelected
    {
      get => (bool) this.GetValue(DropIconButton.ItemSelectedProperty);
      set => this.SetValue(DropIconButton.ItemSelectedProperty, (object) value);
    }

    public double Indent
    {
      get => (double) this.GetValue(DropIconButton.IndenteProperty);
      set => this.SetValue(DropIconButton.IndenteProperty, (object) value);
    }

    public string Title
    {
      get => (string) this.GetValue(DropIconButton.TitleProperty);
      set => this.SetValue(DropIconButton.TitleProperty, (object) value);
    }

    public Geometry Icon
    {
      get => (Geometry) this.GetValue(DropIconButton.IconProperty);
      set => this.SetValue(DropIconButton.IconProperty, (object) value);
    }

    public double IconSize
    {
      get => (double) this.GetValue(DropIconButton.IconSizeProperty);
      set => this.SetValue(DropIconButton.IconSizeProperty, (object) value);
    }

    public SolidColorBrush IconColor
    {
      get => (SolidColorBrush) this.GetValue(DropIconButton.IconColorProperty);
      set => this.SetValue(DropIconButton.IconColorProperty, (object) value);
    }

    private static void OnDataChangeCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is DropIconButton dropIconButton) || e.NewValue == null)
        return;
      dropIconButton.TitleIcon.Data = (Geometry) e.NewValue;
      dropIconButton.TitleIcon.Visibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/dropiconbutton.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (DropIconButton) target;
          break;
        case 2:
          this.TitleIcon = (Path) target;
          break;
        case 3:
          this.TitleText = (EmjTextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
