// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Coaches.CoachControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using XamlAnimatedGif;

#nullable disable
namespace ticktick_WPF.Views.Coaches
{
  public class CoachControl : UserControl, IComponentConnector
  {
    private int _count;
    private int _index;
    internal Image Image;
    internal TextBlock IndexText;
    private bool _contentLoaded;

    public CoachControl(CoachModel linkedCoach)
    {
      this.InitializeComponent();
      this.DataContext = (object) linkedCoach;
      this._index = 0;
      this._count = 1;
      this.SetImage();
      for (; linkedCoach.Next != null; linkedCoach = linkedCoach.Next)
        ++this._count;
      this.UpdateIndexText();
    }

    private void SetImage()
    {
      if (!(this.DataContext is CoachModel dataContext))
        return;
      this.Image.SetValue(AnimationBehavior.SourceUriProperty, (object) new Uri(dataContext.GifPath));
    }

    private void UpdateIndexText()
    {
      UserActCollectUtils.AddClickEvent("timeline", "user_guide", "tip" + (this._index + 1).ToString());
      this.IndexText.Text = string.Format("{0} / {1}", (object) (this._index + 1), (object) this._count);
    }

    private void OnNextClicked(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is CoachModel dataContext))
        return;
      this.DataContext = (object) dataContext.Next;
      ++this._index;
      this.SetImage();
      this.UpdateIndexText();
    }

    private void OnPreClicked(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is CoachModel dataContext))
        return;
      this.DataContext = (object) dataContext.Pre;
      --this._index;
      this.SetImage();
      this.UpdateIndexText();
    }

    private void OnCloseMouseUp(object sender, MouseButtonEventArgs e) => App.Window.EndCoach();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/coaches/coachcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseMouseUp);
          break;
        case 2:
          this.Image = (Image) target;
          break;
        case 3:
          this.IndexText = (TextBlock) target;
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnPreClicked);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnNextClicked);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
