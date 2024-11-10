// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.CustomMenuItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public class CustomMenuItemViewModel : UpDownSelectViewModel
  {
    private string _text;
    private string _rightText;
    private Geometry _icon;

    public object Value { get; set; }

    public List<CustomMenuItemViewModel> SubActions { get; set; }

    public string Text
    {
      get => this._text;
      set
      {
        this._text = value;
        this.OnPropertyChanged(nameof (Text));
      }
    }

    public string RightText
    {
      get => this._rightText;
      set
      {
        this._rightText = value;
        this.OnPropertyChanged(nameof (RightText));
      }
    }

    public double ImageWidth { get; set; } = 16.0;

    public double MinWidth { get; set; }

    public Thickness ImageMargin { get; set; } = new Thickness(12.0, 0.0, 0.0, 0.0);

    public DrawingImage Image { get; set; }

    public Thickness TextMargin { get; set; } = new Thickness(6.0, -1.0, 4.0, 0.0);

    public Geometry Icon
    {
      get => this._icon;
      set
      {
        this._icon = value;
        this.OnPropertyChanged(nameof (Icon));
      }
    }

    public Geometry ExtraIcon { get; set; }

    public SolidColorBrush ExtraIconColor { get; set; }

    public string ExtraIconTips { get; set; }

    public int ExtraIconAngle { get; set; }

    public int ExtraIconSize { get; set; } = 18;

    public bool IsImage { get; set; }

    public bool IsSplit { get; set; }

    public bool IsCenterText { get; set; }

    public override bool HasChildren
    {
      get
      {
        List<CustomMenuItemViewModel> subActions = this.SubActions;
        return (subActions != null ? (__nonvirtual (subActions.Count) > 0 ? 1 : 0) : 0) != 0 || this.SubControl != null;
      }
    }

    public int FontSize { get; set; } = 13;

    public bool IsMessage { get; set; }

    public ITabControl SubControl { get; set; }

    public bool NeedSubContentStyle { get; set; } = true;

    public CustomMenuItemViewModel(object val, string title, DrawingImage image, string rightText = "")
    {
      this.Text = title;
      this.Image = image;
      this.Value = val;
      this.IsImage = true;
      this.ImageWidth = image == null ? 0.0 : 16.0;
      this.RightText = rightText;
      this.MinWidth = this.GetTextWidth(title, rightText) + this.ImageWidth + 30.0;
      if (image != null)
        return;
      this.TextMargin = new Thickness(12.0, -1.0, 4.0, 0.0);
    }

    private double GetTextWidth(string title, string rightText)
    {
      return Utils.MeasureString(title + rightText, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 13.0).Width;
    }

    public CustomMenuItemViewModel(object val, string title, Geometry icon, string rightText = "")
    {
      this.Text = title;
      this._icon = icon;
      this.Value = val;
      this.RightText = rightText;
      this.MinWidth = this.GetTextWidth(title, rightText) + (icon == null ? 0.0 : 30.0) + 30.0;
      if (icon != null)
        return;
      this.TextMargin = new Thickness(12.0, -1.0, 4.0, 0.0);
    }

    public CustomMenuItemViewModel(object val)
    {
      this.IsSplit = true;
      this.IsEnable = false;
    }

    public void SetExtraIconAngle(int angle)
    {
      this.ExtraIconAngle = angle;
      this.OnPropertyChanged("ExtraIconAngle");
    }
  }
}
