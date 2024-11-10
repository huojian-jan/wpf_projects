// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.OptionCheckBox
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class OptionCheckBox : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty ShowCheckIconProperty = DependencyProperty.Register(nameof (ShowCheckIcon), typeof (bool), typeof (OptionCheckBox), new PropertyMetadata((object) true));
    public static readonly DependencyProperty HoverSelectedProperty = DependencyProperty.Register(nameof (HoverSelected), typeof (bool), typeof (OptionCheckBox), new PropertyMetadata((object) false));
    public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register(nameof (Selected), typeof (bool), typeof (OptionCheckBox), new PropertyMetadata((object) false));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof (Text), typeof (string), typeof (OptionCheckBox), new PropertyMetadata((object) null));
    public static readonly DependencyProperty Text2Property = DependencyProperty.Register(nameof (Text2), typeof (string), typeof (OptionCheckBox), new PropertyMetadata((object) null));
    public static readonly DependencyProperty TextPaddingProperty = DependencyProperty.Register(nameof (TextPadding), typeof (Thickness), typeof (OptionCheckBox), new PropertyMetadata((object) new Thickness(8.0, 0.0, 4.0, 0.0)));
    public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register(nameof (IconMargin), typeof (Thickness), typeof (OptionCheckBox), new PropertyMetadata((object) new Thickness(16.0, 0.0, 0.0, 0.0)));
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof (Icon), typeof (Geometry), typeof (OptionCheckBox), new PropertyMetadata((object) null));
    public static readonly DependencyProperty IconFillProperty = DependencyProperty.Register(nameof (IconFill), typeof (Brush), typeof (OptionCheckBox), new PropertyMetadata((object) null));
    public static readonly DependencyProperty CanMultiSelectedProperty = DependencyProperty.Register(nameof (CanMultiSelected), typeof (bool), typeof (OptionCheckBox), new PropertyMetadata((object) false));
    public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof (Group), typeof (string), typeof (OptionCheckBox), new PropertyMetadata((object) null));
    public static readonly DependencyProperty MemberIdProperty = DependencyProperty.Register(nameof (MemberId), typeof (string), typeof (OptionCheckBox), new PropertyMetadata((object) null));
    internal OptionCheckBox Root;
    internal EmjTextBlock TitleText;
    private bool _contentLoaded;

    public event EventHandler Clicked;

    private static event EventHandler<OptionCheckArgs> SelectedChanged;

    public bool ShowCheckIcon
    {
      get => (bool) this.GetValue(OptionCheckBox.ShowCheckIconProperty);
      set => this.SetValue(OptionCheckBox.ShowCheckIconProperty, (object) value);
    }

    public bool HoverSelected
    {
      get => (bool) this.GetValue(OptionCheckBox.HoverSelectedProperty);
      set => this.SetValue(OptionCheckBox.HoverSelectedProperty, (object) value);
    }

    public bool Selected
    {
      get => (bool) this.GetValue(OptionCheckBox.SelectedProperty);
      set
      {
        this.SetValue(OptionCheckBox.SelectedProperty, (object) value);
        this.OnSelectedChanged(new OptionCheckArgs(this.Group, this.MemberId, this.Selected));
      }
    }

    public string Text
    {
      get => (string) this.GetValue(OptionCheckBox.TextProperty);
      set => this.SetValue(OptionCheckBox.TextProperty, (object) value);
    }

    public string Text2
    {
      get => (string) this.GetValue(OptionCheckBox.Text2Property);
      set => this.SetValue(OptionCheckBox.Text2Property, (object) value);
    }

    public Thickness TextPadding
    {
      get => (Thickness) this.GetValue(OptionCheckBox.TextPaddingProperty);
      set => this.SetValue(OptionCheckBox.TextPaddingProperty, (object) value);
    }

    public Thickness IconMargin
    {
      get => (Thickness) this.GetValue(OptionCheckBox.IconMarginProperty);
      set => this.SetValue(OptionCheckBox.IconMarginProperty, (object) value);
    }

    public Geometry Icon
    {
      get => (Geometry) this.GetValue(OptionCheckBox.IconProperty);
      set => this.SetValue(OptionCheckBox.IconProperty, (object) value);
    }

    public Brush IconFill
    {
      get => (Brush) this.GetValue(OptionCheckBox.IconFillProperty);
      set => this.SetValue(OptionCheckBox.IconFillProperty, (object) value);
    }

    public bool CanMultiSelected
    {
      get => (bool) this.GetValue(OptionCheckBox.CanMultiSelectedProperty);
      set => this.SetValue(OptionCheckBox.CanMultiSelectedProperty, (object) value);
    }

    public string Group
    {
      get => (string) this.GetValue(OptionCheckBox.GroupProperty);
      set => this.SetValue(OptionCheckBox.GroupProperty, (object) value);
    }

    public string MemberId
    {
      get
      {
        if (string.IsNullOrEmpty((string) this.GetValue(OptionCheckBox.MemberIdProperty)))
          this.SetValue(OptionCheckBox.MemberIdProperty, (object) Guid.NewGuid().ToString());
        return (string) this.GetValue(OptionCheckBox.MemberIdProperty);
      }
      set => this.SetValue(OptionCheckBox.MemberIdProperty, (object) value);
    }

    public OptionCheckBox() => this.InitializeComponent();

    private void OnSelectedChanged(OptionCheckArgs e)
    {
      if (this.CanMultiSelected || !(this.Group == e.Group) || !e.Selected)
        return;
      this.SetValue(OptionCheckBox.SelectedProperty, (object) (this.MemberId == e.UId));
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      EventHandler clicked = this.Clicked;
      if (clicked == null)
        return;
      clicked(sender, (EventArgs) e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/optioncheckbox.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.TitleText = (EmjTextBlock) target;
        else
          this._contentLoaded = true;
      }
      else
      {
        this.Root = (OptionCheckBox) target;
        this.Root.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
      }
    }
  }
}
