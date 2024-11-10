// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.ColorPicker
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc.ColorSelector.Media;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector
{
  public class ColorPicker : UserControl, IComponentConnector
  {
    private double H;
    private double S = 1.0;
    private double B = 1.0;
    private SolidColorBrush _selectColor = Brushes.Transparent;
    private int _r = (int) byte.MaxValue;
    private int _g = (int) byte.MaxValue;
    private int _b = (int) byte.MaxValue;
    private Popup _popup;
    internal ColorPicker root;
    internal Border viewSelectColor;
    internal Rectangle viewSelectColor1;
    internal Rectangle viewSelectColor2;
    internal ThumbPro thumbSB;
    internal GradientStop viewLine1;
    internal GradientStop viewLine2;
    internal GradientStop viewLine3;
    internal GradientStop viewLine4;
    internal GradientStop viewLine5;
    internal GradientStop viewLine6;
    internal GradientStop viewLine7;
    internal ThumbPro thumbH;
    internal Ellipse DisplayEll;
    internal TextBox TextR;
    internal TextBox TextG;
    internal TextBox TextB;
    internal TextBox TextHex;
    internal Button SaveButton;
    private bool _contentLoaded;

    public event EventHandler<string> ColorSelected;

    public ColorPicker(Popup popup, string selectedColor)
    {
      this.InitializeComponent();
      this._popup = popup;
      this.Init(selectedColor);
    }

    private async Task Init(string selectedColor)
    {
      ColorPicker colorPicker = this;
      if (ThemeUtil.IsEmptyColor(selectedColor))
        selectedColor = ThemeUtil.IsDark() ? "#000000" : "#FFFFFF";
      RgbaColor rColor = new RgbaColor(selectedColor);
      colorPicker.TextHex.Text = ThemeUtil.GetNoAlphaColorString(rColor.HexString);
      colorPicker.SelectColor = rColor.SolidColorBrush;
      TextBox textR = colorPicker.TextR;
      int num = rColor.R;
      string str1 = num.ToString();
      textR.Text = str1;
      TextBox textG = colorPicker.TextG;
      num = rColor.G;
      string str2 = num.ToString();
      textG.Text = str2;
      TextBox textB = colorPicker.TextB;
      num = rColor.B;
      string str3 = num.ToString();
      textB.Text = str3;
      colorPicker._r = rColor.R;
      colorPicker._g = rColor.G;
      colorPicker._b = rColor.B;
      while (colorPicker.ActualWidth < 10.0)
        await Task.Delay(5);
      colorPicker.RColorChange(rColor);
      rColor = (RgbaColor) null;
    }

    private void ThumbPro_ValueChanged(double xpercent, double ypercent)
    {
      this.H = 360.0 * xpercent;
      HsbaColor hsbaColor = new HsbaColor(this.H, 1.0, 1.0);
      this.viewSelectColor.Background = (Brush) hsbaColor.SolidColorBrush;
      this.thumbH.Background = (Brush) hsbaColor.SolidColorBrush;
      HsbaColor hColor = new HsbaColor(this.H, this.S, this.B);
      this.SelectColor = hColor.SolidColorBrush;
      this.HColorChange(hColor);
    }

    private void ThumbPro_ValueChanged_1(double xpercent, double ypercent)
    {
      this.S = xpercent;
      this.B = 1.0 - ypercent;
      HsbaColor hColor = new HsbaColor(this.H, this.S, this.B);
      this.SelectColor = hColor.SolidColorBrush;
      this.HColorChange(hColor);
    }

    public SolidColorBrush SelectColor
    {
      get => this._selectColor;
      set
      {
        this._selectColor = value;
        this.DisplayEll.Fill = (Brush) this.SelectColor;
        PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if (propertyChanged == null)
          return;
        propertyChanged((object) this, new PropertyChangedEventArgs(nameof (SelectColor)));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void HColorChange(HsbaColor hColor)
    {
      RgbaColor rgbaColor = hColor.RgbaColor;
      this.TextR.Text = rgbaColor.R.ToString();
      this.TextG.Text = rgbaColor.G.ToString();
      this.TextB.Text = rgbaColor.B.ToString();
      this._r = rgbaColor.R;
      this._g = rgbaColor.G;
      this._b = rgbaColor.B;
      this.TextHex.Text = ThemeUtil.GetNoAlphaColorString(rgbaColor.HexString);
    }

    private void RColorChange(RgbaColor rColor)
    {
      HsbaColor hsbaColor = rColor.HsbaColor;
      this.H = hsbaColor.H;
      this.S = hsbaColor.S;
      this.B = hsbaColor.B;
      this.thumbH.SetTopLeftByPercent(this.H / 360.0, 0.0);
      this.thumbSB.SetTopLeftByPercent(this.S, 1.0 - this.B);
      hsbaColor.S = 1.0;
      hsbaColor.B = 1.0;
      this.viewSelectColor.Background = (Brush) hsbaColor.SolidColorBrush;
      this.thumbH.Background = (Brush) hsbaColor.SolidColorBrush;
    }

    private void OnNumPreviewInput(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length < 1 || char.IsDigit(e.Text, e.Text.Length - 1))
        return;
      e.Handled = true;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (sender is TextBox textBox && string.IsNullOrEmpty(textBox.Text))
        return;
      if (this.TextHex.Equals(sender))
      {
        string str1 = this.TextHex.Text;
        if (str1.Length < 6)
          return;
        try
        {
          if (!str1.StartsWith("#"))
            str1 = "#" + str1;
          Color color = (Color) ColorConverter.ConvertFromString(str1);
          RgbaColor rColor = new RgbaColor((int) color.R, (int) color.G, (int) color.B, (int) color.A);
          string alphaColorString = ThemeUtil.GetNoAlphaColorString(rColor.HexString);
          if (this.TextHex.Text != alphaColorString)
          {
            this.TextHex.TextChanged -= new TextChangedEventHandler(this.OnTextChanged);
            this.TextHex.Text = alphaColorString;
            this.TextHex.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          }
          this.SelectColor = rColor.SolidColorBrush;
          TextBox textR = this.TextR;
          int num = rColor.R;
          string str2 = num.ToString();
          textR.Text = str2;
          TextBox textG = this.TextG;
          num = rColor.G;
          string str3 = num.ToString();
          textG.Text = str3;
          TextBox textB = this.TextB;
          num = rColor.B;
          string str4 = num.ToString();
          textB.Text = str4;
          this._r = rColor.R;
          this._g = rColor.G;
          this._b = rColor.B;
          this.RColorChange(rColor);
        }
        catch (Exception ex)
        {
        }
      }
      else
      {
        if (this.TextR.Equals(sender))
        {
          int result;
          if (int.TryParse(this.TextR.Text, out result))
          {
            if (result > (int) byte.MaxValue || result < 0)
            {
              result = result > (int) byte.MaxValue ? (int) byte.MaxValue : 0;
              this.TextR.Text = result.ToString();
            }
            this._r = result;
          }
          else
            this.TextR.Text = this._r.ToString();
        }
        if (this.TextG.Equals(sender))
        {
          int result;
          if (int.TryParse(this.TextG.Text, out result))
          {
            if (result > (int) byte.MaxValue || result < 0)
            {
              result = result > (int) byte.MaxValue ? (int) byte.MaxValue : 0;
              this.TextG.Text = result.ToString();
            }
            this._g = result;
          }
          else
            this.TextG.Text = this._g.ToString();
        }
        if (this.TextB.Equals(sender))
        {
          int result;
          if (int.TryParse(this.TextB.Text, out result))
          {
            if (result > (int) byte.MaxValue || result < 0)
            {
              result = result > (int) byte.MaxValue ? (int) byte.MaxValue : 0;
              this.TextB.Text = result.ToString();
            }
            this._b = result;
          }
          else
            this.TextB.Text = this._b.ToString();
        }
        RgbaColor rColor = new RgbaColor(this._r, this._g, this._b, (int) byte.MaxValue);
        this.SelectColor = rColor.SolidColorBrush;
        this.TextHex.Text = ThemeUtil.GetNoAlphaColorString(rColor.HexString);
        this.RColorChange(rColor);
      }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      string alphaColorString = ThemeUtil.GetNoAlphaColorString(new RgbaColor(this._r, this._g, this._b, (int) byte.MaxValue).SolidColorBrush.Color.ToString());
      LocalSettings.Settings.SaveCustomColor(alphaColorString);
      EventHandler<string> colorSelected = this.ColorSelected;
      if (colorSelected != null)
        colorSelected((object) this, alphaColorString);
      this._popup.IsOpen = false;
      UserActCollectUtils.AddClickEvent("project_list_ui", "list_color", "custom");
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this._popup.IsOpen = false;

    private async void OnGotFocus(object sender, RoutedEventArgs e)
    {
      ColorPicker colorPicker = this;
      if (!(sender is TextBox t))
      {
        t = (TextBox) null;
      }
      else
      {
        t.TextChanged -= new TextChangedEventHandler(colorPicker.OnTextChanged);
        t.TextChanged += new TextChangedEventHandler(colorPicker.OnTextChanged);
        await Task.Delay(200);
        t.SelectAll();
        t = (TextBox) null;
      }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
      if (!(sender is TextBox textBox))
        return;
      textBox.Select(0, 0);
      textBox.TextChanged -= new TextChangedEventHandler(this.OnTextChanged);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/colorselector/colorpicker.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.root = (ColorPicker) target;
          break;
        case 2:
          this.viewSelectColor = (Border) target;
          break;
        case 3:
          this.viewSelectColor1 = (Rectangle) target;
          break;
        case 4:
          this.viewSelectColor2 = (Rectangle) target;
          break;
        case 5:
          this.thumbSB = (ThumbPro) target;
          break;
        case 6:
          this.viewLine1 = (GradientStop) target;
          break;
        case 7:
          this.viewLine2 = (GradientStop) target;
          break;
        case 8:
          this.viewLine3 = (GradientStop) target;
          break;
        case 9:
          this.viewLine4 = (GradientStop) target;
          break;
        case 10:
          this.viewLine5 = (GradientStop) target;
          break;
        case 11:
          this.viewLine6 = (GradientStop) target;
          break;
        case 12:
          this.viewLine7 = (GradientStop) target;
          break;
        case 13:
          this.thumbH = (ThumbPro) target;
          break;
        case 14:
          this.DisplayEll = (Ellipse) target;
          break;
        case 15:
          this.TextR = (TextBox) target;
          this.TextR.PreviewTextInput += new TextCompositionEventHandler(this.OnNumPreviewInput);
          this.TextR.GotFocus += new RoutedEventHandler(this.OnGotFocus);
          this.TextR.LostFocus += new RoutedEventHandler(this.OnLostFocus);
          break;
        case 16:
          this.TextG = (TextBox) target;
          this.TextG.PreviewTextInput += new TextCompositionEventHandler(this.OnNumPreviewInput);
          this.TextG.GotFocus += new RoutedEventHandler(this.OnGotFocus);
          this.TextG.LostFocus += new RoutedEventHandler(this.OnLostFocus);
          break;
        case 17:
          this.TextB = (TextBox) target;
          this.TextB.PreviewTextInput += new TextCompositionEventHandler(this.OnNumPreviewInput);
          this.TextB.GotFocus += new RoutedEventHandler(this.OnGotFocus);
          this.TextB.LostFocus += new RoutedEventHandler(this.OnLostFocus);
          break;
        case 18:
          this.TextHex = (TextBox) target;
          this.TextHex.GotFocus += new RoutedEventHandler(this.OnGotFocus);
          this.TextHex.LostFocus += new RoutedEventHandler(this.OnLostFocus);
          break;
        case 19:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 20:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
