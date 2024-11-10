// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.OptionItemWithImageIcon
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
namespace ticktick_WPF.Views.Misc
{
  public class OptionItemWithImageIcon : ContentControl, IComponentConnector
  {
    public static readonly DependencyProperty HoverSelectedProperty = DependencyProperty.Register(nameof (HoverSelected), typeof (bool), typeof (OptionItemWithImageIcon), new PropertyMetadata((object) false));
    public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof (ImageSource), typeof (ImageSource), typeof (OptionItemWithImageIcon), new PropertyMetadata((PropertyChangedCallback) null));
    public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof (Content), typeof (string), typeof (OptionItemWithImageIcon), new PropertyMetadata((object) string.Empty));
    public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(nameof (ImageWidth), typeof (double), typeof (OptionItemWithImageIcon), new PropertyMetadata((object) 20.0));
    public static readonly DependencyProperty TextPaddingProperty = DependencyProperty.Register(nameof (TextPadding), typeof (Thickness), typeof (OptionItemWithImageIcon), new PropertyMetadata((object) new Thickness(6.0, 0.0, 14.0, 0.0)));
    public static readonly DependencyProperty ImageMarginProperty = DependencyProperty.Register(nameof (ImageMargin), typeof (Thickness), typeof (OptionItemWithImageIcon), new PropertyMetadata((object) new Thickness(14.0, 0.0, 0.0, 0.0)));
    public static readonly DependencyProperty ImageOpacityProperty = DependencyProperty.Register(nameof (ImageOpacity), typeof (double), typeof (OptionItemWithImageIcon), new PropertyMetadata((object) 0.6));
    internal OptionItemWithImageIcon Root;
    internal Image Image;
    internal TextBlock ContentText;
    private bool _contentLoaded;

    public bool HoverSelected
    {
      get => (bool) this.GetValue(OptionItemWithImageIcon.HoverSelectedProperty);
      set => this.SetValue(OptionItemWithImageIcon.HoverSelectedProperty, (object) value);
    }

    public ImageSource ImageSource
    {
      get => (ImageSource) this.GetValue(OptionItemWithImageIcon.ImageSourceProperty);
      set => this.SetValue(OptionItemWithImageIcon.ImageSourceProperty, (object) value);
    }

    public double CustomFontSize
    {
      get => this.ContentText.FontSize;
      set => this.ContentText.FontSize = value;
    }

    public string Content
    {
      get => (string) this.GetValue(OptionItemWithImageIcon.ContentProperty);
      set => this.SetValue(OptionItemWithImageIcon.ContentProperty, (object) value);
    }

    public double ImageWidth
    {
      get => (double) this.GetValue(OptionItemWithImageIcon.ImageWidthProperty);
      set => this.SetValue(OptionItemWithImageIcon.ImageWidthProperty, (object) value);
    }

    public Thickness TextPadding
    {
      get => (Thickness) this.GetValue(OptionItemWithImageIcon.TextPaddingProperty);
      set => this.SetValue(OptionItemWithImageIcon.TextPaddingProperty, (object) value);
    }

    public Thickness ImageMargin
    {
      get => (Thickness) this.GetValue(OptionItemWithImageIcon.ImageMarginProperty);
      set => this.SetValue(OptionItemWithImageIcon.ImageMarginProperty, (object) value);
    }

    public double ImageOpacity
    {
      get => (double) this.GetValue(OptionItemWithImageIcon.ImageOpacityProperty);
      set => this.SetValue(OptionItemWithImageIcon.ImageOpacityProperty, (object) value);
    }

    public OptionItemWithImageIcon() => this.InitializeComponent();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/optionitemwithimageicon.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (OptionItemWithImageIcon) target;
          break;
        case 2:
          this.Image = (Image) target;
          break;
        case 3:
          this.ContentText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
