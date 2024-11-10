// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.AppearanceControl
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class AppearanceControl : System.Windows.Controls.UserControl, IComponentConnector
  {
    internal TextBlock CustomTitle;
    internal GroupTitle AppearanceTitle;
    internal Grid ThemeSelectGrid;
    internal ScrollViewer Scroller;
    internal StackPanel ThemePanel;
    internal ItemsControl PureColorItems;
    internal ItemsControl SeasonItems;
    internal ItemsControl CityItems;
    internal ItemsControl PhotoItems;
    internal StackPanel CustomPanel;
    internal Border BackImage;
    internal ItemsControl AppIcons;
    internal StackPanel DisplayPanel;
    internal ContentControl FontFamilyCtrl;
    internal ContentControl FontSizeCtrl;
    internal ContentControl ProjectNumSetCtrl;
    internal ContentControl CompleteLineSetCtrl;
    internal Border ToastBorder;
    internal TextBlock ToastText;
    internal StackPanel UseSystemPanel;
    internal System.Windows.Controls.CheckBox UseSystemThemeCheckBox;
    private bool _contentLoaded;

    public AppearanceControl()
    {
      this.InitializeComponent();
      this.InitThemeData();
      this.InitIconData();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Loaded += (RoutedEventHandler) ((o, e) => this.Scroller.ScrollToTop());
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      if (Utils.IsZhCn() && Utils.IsDida())
        this.FontFamilyCtrl.Content = (object) new FontFamilySetPanel(FontFamilyViewModel.GetChineseFontViewModel());
      else if (Utils.IsEn() && !Utils.IsDida())
        this.FontFamilyCtrl.Content = (object) new FontFamilySetPanel(FontFamilyViewModel.GetEnFontViewModel());
      this.FontSizeCtrl.Content = (object) new FontSizeSetPanel();
      this.ProjectNumSetCtrl.Content = (object) new NumDisplaySettingsPanel();
      this.CompleteLineSetCtrl.Content = (object) new CompleteLineDisplaySettingsPanel();
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
    }

    private void InitThemeData()
    {
      List<ThemeBaseModel> items = new List<ThemeBaseModel>();
      List<ColorThemeViewModel> colorThemeViewModelList = new List<ColorThemeViewModel>();
      ColorThemeViewModel colorThemeViewModel1 = new ColorThemeViewModel();
      colorThemeViewModel1.Key = "White";
      colorThemeViewModel1.IsWhite = true;
      colorThemeViewModelList.Add(colorThemeViewModel1);
      ColorThemeViewModel colorThemeViewModel2 = new ColorThemeViewModel();
      colorThemeViewModel2.Key = "Blue";
      colorThemeViewModelList.Add(colorThemeViewModel2);
      ColorThemeViewModel colorThemeViewModel3 = new ColorThemeViewModel();
      colorThemeViewModel3.Key = "Pink";
      colorThemeViewModelList.Add(colorThemeViewModel3);
      ColorThemeViewModel colorThemeViewModel4 = new ColorThemeViewModel();
      colorThemeViewModel4.Key = "Green";
      colorThemeViewModelList.Add(colorThemeViewModel4);
      ColorThemeViewModel colorThemeViewModel5 = new ColorThemeViewModel();
      colorThemeViewModel5.Key = "Gray";
      colorThemeViewModelList.Add(colorThemeViewModel5);
      ColorThemeViewModel colorThemeViewModel6 = new ColorThemeViewModel();
      colorThemeViewModel6.Key = "Yellow";
      colorThemeViewModelList.Add(colorThemeViewModel6);
      ColorThemeViewModel colorThemeViewModel7 = new ColorThemeViewModel();
      colorThemeViewModel7.Key = "Dark";
      colorThemeViewModel7.IsDark = true;
      colorThemeViewModelList.Add(colorThemeViewModel7);
      List<ColorThemeViewModel> collection1 = colorThemeViewModelList;
      List<ProThemeViewModel> proThemeViewModelList1 = new List<ProThemeViewModel>();
      ProThemeViewModel proThemeViewModel1 = new ProThemeViewModel();
      proThemeViewModel1.Key = "Spring";
      proThemeViewModelList1.Add(proThemeViewModel1);
      ProThemeViewModel proThemeViewModel2 = new ProThemeViewModel();
      proThemeViewModel2.Key = "Summer";
      proThemeViewModelList1.Add(proThemeViewModel2);
      ProThemeViewModel proThemeViewModel3 = new ProThemeViewModel();
      proThemeViewModel3.Key = "Autumn";
      proThemeViewModelList1.Add(proThemeViewModel3);
      ProThemeViewModel proThemeViewModel4 = new ProThemeViewModel();
      proThemeViewModel4.Key = "Winter";
      proThemeViewModelList1.Add(proThemeViewModel4);
      List<ProThemeViewModel> collection2 = proThemeViewModelList1;
      List<ProThemeViewModel> proThemeViewModelList2 = new List<ProThemeViewModel>();
      ProThemeViewModel proThemeViewModel5 = new ProThemeViewModel();
      proThemeViewModel5.Key = "London";
      proThemeViewModelList2.Add(proThemeViewModel5);
      ProThemeViewModel proThemeViewModel6 = new ProThemeViewModel();
      proThemeViewModel6.Key = "Moscow";
      proThemeViewModelList2.Add(proThemeViewModel6);
      ProThemeViewModel proThemeViewModel7 = new ProThemeViewModel();
      proThemeViewModel7.Key = "Sanfrancisco";
      proThemeViewModelList2.Add(proThemeViewModel7);
      ProThemeViewModel proThemeViewModel8 = new ProThemeViewModel();
      proThemeViewModel8.Key = "Seoul";
      proThemeViewModelList2.Add(proThemeViewModel8);
      ProThemeViewModel proThemeViewModel9 = new ProThemeViewModel();
      proThemeViewModel9.Key = "Shanghai";
      proThemeViewModelList2.Add(proThemeViewModel9);
      ProThemeViewModel proThemeViewModel10 = new ProThemeViewModel();
      proThemeViewModel10.Key = "Sydney";
      proThemeViewModelList2.Add(proThemeViewModel10);
      ProThemeViewModel proThemeViewModel11 = new ProThemeViewModel();
      proThemeViewModel11.Key = "Tokyo";
      proThemeViewModelList2.Add(proThemeViewModel11);
      ProThemeViewModel proThemeViewModel12 = new ProThemeViewModel();
      proThemeViewModel12.Key = "NewYork";
      proThemeViewModelList2.Add(proThemeViewModel12);
      List<ProThemeViewModel> collection3 = proThemeViewModelList2;
      List<ProThemeViewModel> proThemeViewModelList3 = collection3;
      ProThemeViewModel proThemeViewModel13 = new ProThemeViewModel();
      proThemeViewModel13.Key = "Guangzhou";
      proThemeViewModelList3.Add(proThemeViewModel13);
      List<ProThemeViewModel> proThemeViewModelList4 = collection3;
      ProThemeViewModel proThemeViewModel14 = new ProThemeViewModel();
      proThemeViewModel14.Key = "Shenzhen";
      proThemeViewModelList4.Add(proThemeViewModel14);
      List<ProThemeViewModel> proThemeViewModelList5 = collection3;
      ProThemeViewModel proThemeViewModel15 = new ProThemeViewModel();
      proThemeViewModel15.Key = "Hangzhou";
      proThemeViewModelList5.Insert(0, proThemeViewModel15);
      List<ProThemeViewModel> proThemeViewModelList6 = collection3;
      ProThemeViewModel proThemeViewModel16 = new ProThemeViewModel();
      proThemeViewModel16.Key = "Beijing";
      proThemeViewModelList6.Insert(0, proThemeViewModel16);
      List<ProThemeViewModel> proThemeViewModelList7 = new List<ProThemeViewModel>();
      ProThemeViewModel proThemeViewModel17 = new ProThemeViewModel();
      proThemeViewModel17.Key = "Structure";
      proThemeViewModelList7.Add(proThemeViewModel17);
      ProThemeViewModel proThemeViewModel18 = new ProThemeViewModel();
      proThemeViewModel18.Key = "Blacksea";
      proThemeViewModelList7.Add(proThemeViewModel18);
      ProThemeViewModel proThemeViewModel19 = new ProThemeViewModel();
      proThemeViewModel19.Key = "Desert";
      proThemeViewModelList7.Add(proThemeViewModel19);
      ProThemeViewModel proThemeViewModel20 = new ProThemeViewModel();
      proThemeViewModel20.Key = "Leaves";
      proThemeViewModelList7.Add(proThemeViewModel20);
      ProThemeViewModel proThemeViewModel21 = new ProThemeViewModel();
      proThemeViewModel21.Key = "Birds";
      proThemeViewModelList7.Add(proThemeViewModel21);
      ProThemeViewModel proThemeViewModel22 = new ProThemeViewModel();
      proThemeViewModel22.Key = "Dawn";
      proThemeViewModelList7.Add(proThemeViewModel22);
      ProThemeViewModel proThemeViewModel23 = new ProThemeViewModel();
      proThemeViewModel23.Key = "Blossom";
      proThemeViewModelList7.Add(proThemeViewModel23);
      ProThemeViewModel proThemeViewModel24 = new ProThemeViewModel();
      proThemeViewModel24.Key = "Frozen";
      proThemeViewModelList7.Add(proThemeViewModel24);
      ProThemeViewModel proThemeViewModel25 = new ProThemeViewModel();
      proThemeViewModel25.Key = "Meadow";
      proThemeViewModelList7.Add(proThemeViewModel25);
      ProThemeViewModel proThemeViewModel26 = new ProThemeViewModel();
      proThemeViewModel26.Key = "Silence";
      proThemeViewModelList7.Add(proThemeViewModel26);
      List<ProThemeViewModel> collection4 = proThemeViewModelList7;
      items.AddRange((IEnumerable<ThemeBaseModel>) collection1);
      items.AddRange((IEnumerable<ThemeBaseModel>) collection2);
      items.AddRange((IEnumerable<ThemeBaseModel>) collection3);
      items.AddRange((IEnumerable<ThemeBaseModel>) collection4);
      items.ForEach((Action<ThemeBaseModel>) (item =>
      {
        item.Parent = items;
        item.Selected = item.Key == LocalSettings.Settings.ExtraSettings.AppTheme;
        item.Color = ThemeUtil.GetColor("Theme" + item.Key);
        item.Name = Utils.GetString(item.Key);
      }));
      CustomThemeViewModel customThemeViewModel1 = new CustomThemeViewModel();
      customThemeViewModel1.Parent = items;
      CustomThemeViewModel customThemeViewModel2 = customThemeViewModel1;
      items.Add((ThemeBaseModel) customThemeViewModel2);
      this.CustomPanel.DataContext = (object) customThemeViewModel2;
      this.PureColorItems.ItemsSource = (IEnumerable) collection1;
      this.SeasonItems.ItemsSource = (IEnumerable) collection2;
      this.CityItems.ItemsSource = (IEnumerable) collection3;
      this.PhotoItems.ItemsSource = (IEnumerable) collection4;
      this.UseSystemThemeCheckBox.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.UseSystemTheme);
    }

    private void InitIconData()
    {
      List<AppIconViewModel> icons = new List<AppIconViewModel>();
      List<AppIconViewModel> list = Enum.GetValues(typeof (AppIconKey)).Cast<AppIconKey>().Select<AppIconKey, AppIconViewModel>((Func<AppIconKey, AppIconViewModel>) (e => new AppIconViewModel(e, icons))).ToList<AppIconViewModel>();
      icons.AddRange((IEnumerable<AppIconViewModel>) list);
      this.AppIcons.ItemsSource = (IEnumerable) icons;
    }

    private void OnCustomClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.PremiumThemes) || !(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is CustomThemeViewModel dataContext))
        return;
      List<string> list = ((IEnumerable<string>) Directory.GetFiles(AppPaths.ThemeDir, "custom*.*")).ToList<string>();
      if (list.Count > 0)
      {
        CustomThemeDialog customThemeDialog = new CustomThemeDialog(list[0], LocalSettings.Settings.CustomThemeLocation);
        customThemeDialog.Owner = Window.GetWindow((DependencyObject) this);
        customThemeDialog.ShowDialog();
      }
      else
      {
        OpenFileDialog openFileDialog1 = new OpenFileDialog();
        openFileDialog1.Multiselect = false;
        openFileDialog1.Filter = "IMAGE (*.JPG;*.JPEG;*.PJPEG;*.PNG;*.GIF;*.PJP;*.JFIF;*.BMP;*.TIFF;*.TIF;*.DIB;*.WEBP)|*.JPG;*.JPEG;*.PJPEG;*.PNG;*.GIF;*.PJP;*.JFIF;*.BMP;*.TIFF;*.TIF;*.DIB;*.WEBP";
        openFileDialog1.FilterIndex = 1;
        OpenFileDialog openFileDialog2 = openFileDialog1;
        if (openFileDialog2.ShowDialog() == DialogResult.OK)
        {
          try
          {
            string fileName = openFileDialog2.FileName;
            string str1 = ((IEnumerable<string>) fileName.Split('.')).LastOrDefault<string>();
            if (!string.IsNullOrEmpty(str1))
            {
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
              CustomThemeDialog customThemeDialog = new CustomThemeDialog(str2, LocalSettings.Settings.CustomThemeLocation);
              customThemeDialog.Owner = Window.GetWindow((DependencyObject) this);
              customThemeDialog.ShowDialog();
            }
          }
          catch (Exception ex)
          {
            UtilLog.Error(ex);
          }
        }
      }
      dataContext.Selected = LocalSettings.Settings.ExtraSettings.AppTheme == "Custom";
      dataContext.SetImage();
      if (dataContext.Selected)
        dataContext.SetSelected();
      else
        App.Instance.LoadTheme("Custom", LocalSettings.Settings.ExtraSettings.AppTheme);
    }

    private bool ImageAvailable(string saveImagePath) => Utils.LoadBitmap(saveImagePath) != null;

    public void Toast(string toastText)
    {
      this.ToastBorder.Visibility = Visibility.Visible;
      this.ToastText.Text = toastText;
      this.ToastBorder.BeginStoryboard((Storyboard) this.FindResource((object) "ShowToast"));
    }

    private void OnToasted(object sender, EventArgs e)
    {
      this.ToastBorder.Visibility = Visibility.Collapsed;
    }

    private void OnTabSelected(object sender, GroupTitleViewModel e)
    {
      this.Scroller.ScrollToVerticalOffset(0.0);
      this.ThemePanel.Visibility = e.Index == 0 ? Visibility.Visible : Visibility.Collapsed;
      this.AppIcons.Visibility = e.Index == 1 ? Visibility.Visible : Visibility.Collapsed;
      this.DisplayPanel.Visibility = e.Index == 2 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnUseSystemClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is System.Windows.Controls.CheckBox checkBox1))
        return;
      System.Windows.Controls.CheckBox checkBox2 = checkBox1;
      bool? isChecked = checkBox1.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      checkBox2.IsChecked = nullable;
      LocalSettings.Settings.ExtraSettings.UseSystemTheme = ((int) checkBox1.IsChecked ?? 1) != 0;
      LocalSettings.Settings.Save();
      if (LocalSettings.Settings.ExtraSettings.UseSystemTheme)
        ThemeUtil.TrySetSystemTheme(true);
      else
        App.Instance.LoadTargetTheme(LocalSettings.Settings.ExtraSettings.AppTheme);
    }

    public void ReloadCustomImage()
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/theme/appearancecontrol.xaml", UriKind.Relative));
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
          ((Timeline) target).Completed += new EventHandler(this.OnToasted);
          break;
        case 2:
          this.CustomTitle = (TextBlock) target;
          break;
        case 3:
          this.AppearanceTitle = (GroupTitle) target;
          break;
        case 4:
          this.ThemeSelectGrid = (Grid) target;
          break;
        case 5:
          this.Scroller = (ScrollViewer) target;
          break;
        case 6:
          this.ThemePanel = (StackPanel) target;
          break;
        case 7:
          this.PureColorItems = (ItemsControl) target;
          break;
        case 8:
          this.SeasonItems = (ItemsControl) target;
          break;
        case 9:
          this.CityItems = (ItemsControl) target;
          break;
        case 10:
          this.PhotoItems = (ItemsControl) target;
          break;
        case 11:
          this.CustomPanel = (StackPanel) target;
          this.CustomPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCustomClick);
          break;
        case 12:
          this.BackImage = (Border) target;
          break;
        case 13:
          this.AppIcons = (ItemsControl) target;
          break;
        case 14:
          this.DisplayPanel = (StackPanel) target;
          break;
        case 15:
          this.FontFamilyCtrl = (ContentControl) target;
          break;
        case 16:
          this.FontSizeCtrl = (ContentControl) target;
          break;
        case 17:
          this.ProjectNumSetCtrl = (ContentControl) target;
          break;
        case 18:
          this.CompleteLineSetCtrl = (ContentControl) target;
          break;
        case 19:
          this.ToastBorder = (Border) target;
          break;
        case 20:
          this.ToastText = (TextBlock) target;
          break;
        case 21:
          this.UseSystemPanel = (StackPanel) target;
          break;
        case 22:
          this.UseSystemThemeCheckBox = (System.Windows.Controls.CheckBox) target;
          this.UseSystemThemeCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnUseSystemClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
