// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.SelectSoundControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class SelectSoundControl : UserControl, IComponentConnector, IStyleConnector
  {
    private string _lastSelected;
    public Popup Popup;
    public List<PomoSoundViewModel> Models = new List<PomoSoundViewModel>()
    {
      new PomoSoundViewModel("none"),
      new PomoSoundViewModel("Clock"),
      new PomoSoundViewModel("Stove"),
      new PomoSoundViewModel("Boiling"),
      new PomoSoundViewModel("TempleBlock"),
      new PomoSoundViewModel("Storm"),
      new PomoSoundViewModel("Rain"),
      new PomoSoundViewModel("Cafe"),
      new PomoSoundViewModel("MusicBox"),
      new PomoSoundViewModel("Morning"),
      new PomoSoundViewModel("Summer"),
      new PomoSoundViewModel("Chirp"),
      new PomoSoundViewModel("Forest"),
      new PomoSoundViewModel("Stream"),
      new PomoSoundViewModel("DeepSea"),
      new PomoSoundViewModel("Wave"),
      new PomoSoundViewModel("Desert"),
      new PomoSoundViewModel("StreetTraffic")
    };
    internal Polygon X;
    internal WrapPanel SoundPanel;
    internal Grid ToastGrid;
    internal TextBlock ToastTextBlock;
    private bool _contentLoaded;

    public event EventHandler ItemSelect;

    public SelectSoundControl()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      foreach (PomoSoundViewModel model in this.Models)
      {
        UIElementCollection children = this.SoundPanel.Children;
        ContentControl element = new ContentControl();
        element.DataContext = (object) model;
        children.Add((UIElement) element);
      }
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => this.CheckSelected();

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is PomoSoundViewModel dataContext))
        return;
      if (dataContext.Key != "none" && dataContext.Key != "Clock" && !UserDao.IsPro())
      {
        ProChecker.ShowUpgradeDialog(ProType.PomoSound);
      }
      else
      {
        this._lastSelected = dataContext.Key;
        if (dataContext.Downloaded)
        {
          LocalSettings.Settings.PomoSound = dataContext.Key;
          PomoSoundPlayer.StartPlaySound(TickFocusManager.Status != 0);
          this.SetModelSelected(dataContext);
          dataContext.Selected = true;
        }
        else if (dataContext.NeedDownload)
        {
          if (Utils.IsNetworkAvailable())
          {
            dataContext.TryDownload();
            dataContext.NotifyDownLoadCompleted += new EventHandler<string>(this.OnSoundDownLoaded);
          }
          else
          {
            this.ToastGrid.Visibility = Visibility.Visible;
            ((Storyboard) this.FindResource((object) "ToastShowAndHide")).Begin();
          }
        }
        EventHandler itemSelect = this.ItemSelect;
        if (itemSelect == null)
          return;
        itemSelect((object) this, (EventArgs) null);
      }
    }

    private void OnStoryCompleted(object sender, EventArgs e)
    {
      this.ToastGrid.Visibility = Visibility.Collapsed;
    }

    private void SetModelSelected(PomoSoundViewModel model)
    {
      model.Selected = true;
      this.Models.ForEach((Action<PomoSoundViewModel>) (m =>
      {
        if (!(m.Key != model.Key))
          return;
        m.Selected = false;
      }));
    }

    private void OnSoundDownLoaded(object sender, string e)
    {
      if (!(e == this._lastSelected))
        return;
      LocalSettings.Settings.PomoSound = e;
      PomoSoundPlayer.StartPlaySound(TickFocusManager.Status != 0);
      if (!(sender is PomoSoundViewModel model))
        return;
      this.SetModelSelected(model);
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e)
    {
      if (this.Popup == null)
        return;
      this.Popup.IsOpen = false;
    }

    public string GetSelectedItem()
    {
      return this.Models.FirstOrDefault<PomoSoundViewModel>((Func<PomoSoundViewModel, bool>) (m => m.Selected))?.Key;
    }

    public void CheckSelected()
    {
      if (!(this.GetSelectedItem() != LocalSettings.Settings.PomoSound))
        return;
      this.Models.ForEach((Action<PomoSoundViewModel>) (m => m.Selected = false));
      PomoSoundViewModel pomoSoundViewModel = this.Models.FirstOrDefault<PomoSoundViewModel>((Func<PomoSoundViewModel, bool>) (m => m.Key == LocalSettings.Settings.PomoSound));
      if (pomoSoundViewModel == null)
        return;
      if (pomoSoundViewModel.Downloaded)
      {
        pomoSoundViewModel.Selected = true;
      }
      else
      {
        this.Models[0].Selected = true;
        LocalSettings.Settings.PomoSound = "none";
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/selectsoundcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((Timeline) target).Completed += new EventHandler(this.OnStoryCompleted);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
          break;
        case 3:
          this.X = (Polygon) target;
          break;
        case 4:
          this.SoundPanel = (WrapPanel) target;
          break;
        case 8:
          this.ToastGrid = (Grid) target;
          break;
        case 9:
          this.ToastTextBlock = (TextBlock) target;
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
      switch (connectionId)
      {
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
      }
    }
  }
}
