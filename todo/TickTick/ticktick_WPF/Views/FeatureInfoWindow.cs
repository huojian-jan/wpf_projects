// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.FeatureInfoWindow
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
using ticktick_WPF.Util;
using TickTickUtils;
using XamlAnimatedGif;

#nullable disable
namespace ticktick_WPF.Views
{
  public class FeatureInfoWindow : MyWindow, IOkCancelWindow, IComponentConnector
  {
    internal Border DisplayImage;
    internal Image GifImage;
    internal Button CommandButton;
    private bool _contentLoaded;

    public FeatureInfoWindow()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    public void Ok() => this.Close();

    public void OnCancel() => this.Close();

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      this.DragMove();
      e.Handled = false;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

    internal static async Task Show(
      string title,
      string info1,
      string info2,
      string name,
      string cTitle,
      Action act)
    {
      string imagePath = string.Empty;
      name += ".gif";
      imagePath = AppPaths.ResourceDir + name;
      int num = await IOUtils.CheckResourceExist(AppPaths.ResourceDir, name, "https://" + BaseUrl.GetPullDomain() + "/windows/static/" + name) ? 1 : 0;
      FeatureInfoWindowViewModel infoWindowViewModel = new FeatureInfoWindowViewModel(title, info1, info2, imagePath, cTitle, act);
      FeatureInfoWindow featureInfoWindow = new FeatureInfoWindow();
      featureInfoWindow.Owner = (Window) App.Window;
      featureInfoWindow.DataContext = (object) infoWindowViewModel;
      featureInfoWindow.ShowDialog();
      imagePath = (string) null;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      FeatureInfoWindow featureInfoWindow = this;
      if (!(featureInfoWindow.DataContext is FeatureInfoWindowViewModel dataContext))
        return;
      featureInfoWindow.GifImage.SetValue(AnimationBehavior.SourceUriProperty, (object) new Uri(dataContext.GifPath));
    }

    private void OnCommandClick(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is FeatureInfoWindowViewModel dataContext)
        dataContext.DoAction();
      this.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/featureinfowindow.xaml", UriKind.Relative));
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
          this.DisplayImage = (Border) target;
          break;
        case 2:
          this.GifImage = (Image) target;
          break;
        case 3:
          this.CommandButton = (Button) target;
          this.CommandButton.Click += new RoutedEventHandler(this.OnCommandClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CloseButton_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
