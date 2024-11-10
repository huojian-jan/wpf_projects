// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.CustomThemeDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class CustomThemeDialog : Window, IComponentConnector, IStyleConnector
  {
    internal StackPanel CustomThemeGrid;
    internal ImageClipper ImageClipper;
    internal Popup CustomThemeSetPopup;
    internal ItemsControl CustomThemes;
    internal Slider ImageOpacitySlider;
    internal Slider ImageBlurSlider;
    internal Slider ShowAreaOpacitySlider;
    private bool _contentLoaded;

    public CustomThemeDialog(string imageUrl, string location = null)
    {
      this.InitializeComponent();
      this.SetCustomBase();
      this.ImageClipper.OnThemeColorsChanged += new EventHandler<List<SelectThemeColorViewModel>>(this.OnCustomThemeColorsChanged);
      this.ImageClipper.SetImage(imageUrl, location);
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnThemeSetClick(object sender, MouseButtonEventArgs e)
    {
      this.CustomThemeSetPopup.IsOpen = true;
    }

    private void OnCustomThemeColorsChanged(object sender, List<SelectThemeColorViewModel> e)
    {
      SelectThemeColorViewModel themeColorViewModel1 = e.FirstOrDefault<SelectThemeColorViewModel>((Func<SelectThemeColorViewModel, bool>) (c => c.Color == LocalSettings.Settings.CustomThemeColor));
      SelectThemeColorViewModel themeColorViewModel2 = themeColorViewModel1 == null ? e[0] : themeColorViewModel1;
      LocalSettings.Settings.CustomThemeColor = themeColorViewModel2.Color;
      themeColorViewModel2.Selected = true;
      this.SetThemeColor(themeColorViewModel2.Color);
      this.CustomThemes.ItemsSource = (IEnumerable) e;
    }

    private void OnThemeColorClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is SelectThemeColorViewModel dataContext))
        return;
      if (this.CustomThemes.ItemsSource is List<SelectThemeColorViewModel> itemsSource)
        itemsSource.ForEach((Action<SelectThemeColorViewModel>) (m => m.Selected = false));
      dataContext.Selected = true;
      this.SetThemeColor(dataContext.Color);
    }

    private void SetThemeColor(string color)
    {
      ResourceDictionary resourceDictionary1 = new ResourceDictionary();
      resourceDictionary1[(object) "ProjectMenuBackGround"] = (object) ThemeUtil.GetColorInString("#1A000000");
      if (ColorConverter.ConvertFromString(color) is Color color1)
      {
        resourceDictionary1[(object) "ColorPrimary"] = (object) color1;
        resourceDictionary1[(object) "MainWindow"] = (object) color1;
        resourceDictionary1[(object) "PrimaryColor"] = (object) new SolidColorBrush(color1);
        resourceDictionary1[(object) "DateColorPrimary"] = (object) new SolidColorBrush(color1);
        ResourceDictionary resourceDictionary2 = resourceDictionary1;
        SolidColorBrush solidColorBrush1 = new SolidColorBrush(color1);
        solidColorBrush1.Opacity = 0.1;
        resourceDictionary2[(object) "KanbanContainerColor"] = (object) solidColorBrush1;
        ResourceDictionary resourceDictionary3 = resourceDictionary1;
        SolidColorBrush solidColorBrush2 = new SolidColorBrush(color1);
        solidColorBrush2.Opacity = 0.1;
        resourceDictionary3[(object) "MatrixContainerColor"] = (object) solidColorBrush2;
      }
      if (ColorConverter.ConvertFromString("#CC" + color.Substring(color.Length - 6, 6)) is Color color2)
      {
        resourceDictionary1[(object) "LeftBarBackColorTop"] = (object) color2;
        resourceDictionary1[(object) "LeftBarBackColorBottom"] = (object) color2;
      }
      if (ColorConverter.ConvertFromString("#28" + color.Substring(color.Length - 6, 6)) is Color color3)
        resourceDictionary1[(object) "ItemSelected"] = (object) color3;
      if (ColorConverter.ConvertFromString("#14" + color.Substring(color.Length - 6, 6)) is Color color4)
        resourceDictionary1[(object) "ItemHover"] = (object) color4;
      App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri("Resource/Themes/LightNecessaryColor.xaml", UriKind.Relative)
      });
      App.Instance.Resources.MergedDictionaries.Add(resourceDictionary1);
      LocalSettings.Settings.CustomThemeColor = color;
      LocalSettings.Settings.NotifyThemeChanged();
      DataChangedNotifier.NotifyThemeModeChanged();
    }

    private void OnImageOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      LocalSettings.Settings.ThemeImageOpacity = 0.6 + e.NewValue * 0.04;
    }

    private void OnImageBlurChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      LocalSettings.Settings.ThemeImageBlurRadius = (int) e.NewValue * 4;
    }

    private void OnShowAreaOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      LocalSettings.Settings.ShowAreaOpacity = 0.8 + e.NewValue * 0.02;
      App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        [(object) "ShowAreaBackground"] = (object) ThemeUtil.GetAlphaColor("#FFFFFF", (int) (LocalSettings.Settings.ShowAreaOpacity * 100.0))
      });
    }

    private void OnResetCustomImageClick(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      openFileDialog1.Multiselect = false;
      openFileDialog1.Filter = "IMAGE (*.JPG;*.JPEG;*.PJPEG;*.PNG;*.GIF;*.PJP;*.JFIF;*.BMP;*.TIFF;*.TIF;*.DIB;*.WEBP)|*.JPG;*.JPEG;*.PJPEG;*.PNG;*.GIF;*.PJP;*.JFIF;*.BMP;*.TIFF;*.TIF;*.DIB;*.WEBP";
      openFileDialog1.FilterIndex = 1;
      OpenFileDialog openFileDialog2 = openFileDialog1;
      if (openFileDialog2.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;
      try
      {
        string fileName = openFileDialog2.FileName;
        string str1 = ((IEnumerable<string>) fileName.Split('.')).LastOrDefault<string>();
        if (string.IsNullOrEmpty(str1))
          return;
        string str2 = AppPaths.ThemeDir + "custom_bak." + str1;
        foreach (string file in Directory.GetFiles(AppPaths.ThemeDir, "custom_bak.*"))
        {
          FileInfo fileInfo = new FileInfo(file);
          fileInfo.Attributes = FileAttributes.Normal;
          fileInfo.Delete();
        }
        File.Copy(fileName, str2);
        if (!this.ImageAvailable(str2))
          return;
        this.ResetCustomTheme();
        this.ImageClipper.SetImage(str2);
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
    }

    private bool ImageAvailable(string saveImagePath) => Utils.LoadBitmap(saveImagePath) != null;

    private void ResetCustomTheme()
    {
      LocalSettings.Settings.ResetCustomTheme();
      this.ImageOpacitySlider.Value = (1.0 - LocalSettings.Settings.ThemeImageOpacity) * 25.0;
      this.ImageBlurSlider.Value = (double) LocalSettings.Settings.ThemeImageBlurRadius / 4.0;
      this.ShowAreaOpacitySlider.Value = (1.0 - LocalSettings.Settings.ShowAreaOpacity) * 50.0;
    }

    private void OnCustomSaveClick(object sender, RoutedEventArgs e)
    {
      try
      {
        string[] files = Directory.GetFiles(AppPaths.ThemeDir, "custom_bak.*");
        if (files.Length != 0)
        {
          foreach (string file in Directory.GetFiles(AppPaths.ThemeDir, "custom*.*"))
          {
            if (!file.Contains("custom_bak"))
            {
              FileInfo fileInfo = new FileInfo(file);
              fileInfo.Attributes = FileAttributes.Normal;
              fileInfo.Delete();
            }
          }
          string sourceFileName = files[0];
          File.Move(sourceFileName, sourceFileName.Replace("custom_bak", "custom" + DateTime.Now.Ticks.ToString()));
        }
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
      Rect currentLocation = this.ImageClipper.GetCurrentLocation();
      LocalSettings settings = LocalSettings.Settings;
      string[] strArray = new string[7];
      double num = currentLocation.Left;
      strArray[0] = num.ToString();
      strArray[1] = ",";
      num = currentLocation.Top;
      strArray[2] = num.ToString();
      strArray[3] = ",";
      num = currentLocation.Width;
      strArray[4] = num.ToString();
      strArray[5] = ",";
      num = currentLocation.Height;
      strArray[6] = num.ToString();
      string str = string.Concat(strArray);
      settings.CustomThemeLocation = str;
      LocalSettings.Settings.SaveCustomTheme();
      if ("Custom" != LocalSettings.Settings.ExtraSettings.AppTheme)
      {
        UserActCollectUtils.AddClickEvent("settings", "theme", "Custom".ToLower());
        LocalSettings.Settings.ExtraSettings.AppTheme = "Custom";
      }
      this.Close();
    }

    private void SetCustomBase()
    {
      LocalSettings.Settings.LoadCustomTheme();
      this.ImageOpacitySlider.Value = (LocalSettings.Settings.ThemeImageOpacity - 0.6) * 25.0;
      this.ImageBlurSlider.Value = (double) LocalSettings.Settings.ThemeImageBlurRadius / 4.0;
      this.ShowAreaOpacitySlider.Value = (LocalSettings.Settings.ShowAreaOpacity - 0.8) * 50.0;
      string themeId = LocalSettings.Settings.ThemeId;
      if (ThemeKey.IsCustomTheme(themeId))
        return;
      if (ThemeKey.IsDarkTheme(themeId))
      {
        App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
        {
          Source = new Uri("Resource/Themes/Light.xaml", UriKind.Relative)
        });
        App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
        {
          Source = new Uri("Resource/icons_light.xaml", UriKind.Relative)
        });
      }
      App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri("Resource/Themes/LightNecessaryColor.xaml", UriKind.Relative)
      });
      App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri("Resource/Themes/Custom.xaml", UriKind.Relative)
      });
      App.Instance.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        [(object) "ShowAreaBackground"] = (object) ThemeUtil.GetAlphaColor("#FFFFFF", (int) (LocalSettings.Settings.ShowAreaOpacity * 100.0))
      });
      App.Instance.SetProjectMenuTextShadowOpacity(true);
      LocalSettings.Settings.ThemeId = "Custom";
      if (!ThemeKey.IsDarkTheme(themeId))
        return;
      DataChangedNotifier.NotifyIsDarkChanged();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/theme/customthemedialog.xaml", UriKind.Relative));
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
          this.CustomThemeGrid = (StackPanel) target;
          break;
        case 2:
          this.ImageClipper = (ImageClipper) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnThemeSetClick);
          break;
        case 4:
          this.CustomThemeSetPopup = (Popup) target;
          break;
        case 5:
          this.CustomThemes = (ItemsControl) target;
          break;
        case 7:
          this.ImageOpacitySlider = (Slider) target;
          this.ImageOpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.OnImageOpacityChanged);
          break;
        case 8:
          this.ImageBlurSlider = (Slider) target;
          this.ImageBlurSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.OnImageBlurChanged);
          break;
        case 9:
          this.ShowAreaOpacitySlider = (Slider) target;
          this.ShowAreaOpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.OnShowAreaOpacityChanged);
          break;
        case 10:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.OnResetCustomImageClick);
          break;
        case 11:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.OnCustomSaveClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 6)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnThemeColorClick);
    }
  }
}
