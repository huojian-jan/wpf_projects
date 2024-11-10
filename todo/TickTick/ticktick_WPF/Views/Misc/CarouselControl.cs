// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.CarouselControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class CarouselControl : Grid, IComponentConnector, IStyleConnector
  {
    private CarouselItemViewModel _selectedItem;
    private Timer _timer = new Timer(3000.0);
    private List<CarouselItemViewModel> _items;
    internal TextBlock Title;
    internal TextBlock SubTitle;
    internal ColumnDefinition FirstColumn;
    internal Image FrontImage;
    internal Image NextImage;
    internal ItemsControl SelectableItems;
    internal Border SlideButton;
    private bool _contentLoaded;

    public CarouselControl()
    {
      this.InitializeComponent();
      this._timer.Elapsed += new ElapsedEventHandler(this.TryStartScroll);
    }

    private void TryStartScroll(object sender, ElapsedEventArgs e)
    {
      Utils.RunOnBackgroundThread(this.Dispatcher, new Action(this.StartScroll));
    }

    private void StartScroll()
    {
      int num = this._items.IndexOf(this._selectedItem);
      int index = num < this._items.Count - 1 ? num + 1 : 0;
      CarouselItemViewModel next = this._items[index];
      this._selectedItem.Selected = false;
      CarouselItemViewModel selectedItem = this._selectedItem;
      next.Selected = true;
      this._selectedItem = next;
      this.MoveToNext(selectedItem, next, (double) (index * 10));
    }

    public void InitData(List<CarouselItemViewModel> models)
    {
      if (models == null || models.Count == 0)
        return;
      int[] numArray = new int[models.Count + 1];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = index;
      this.SelectableItems.ItemsSource = (IEnumerable) numArray;
      this._items = models;
      CarouselItemViewModel model = models[0];
      model.Selected = true;
      this._selectedItem = model;
      this.FrontImage.Source = (ImageSource) new BitmapImage(new Uri(model.ImageUrl));
      this.Title.Text = model.Title;
      this.SubTitle.Text = model.SubTitle;
      this._timer.Start();
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Grid grid) || !(grid.DataContext is int dataContext))
        return;
      this._selectedItem.Selected = false;
      CarouselItemViewModel selectedItem = this._selectedItem;
      int num = this._items.IndexOf(selectedItem);
      if (dataContext > num)
        --dataContext;
      CarouselItemViewModel next = this._items[dataContext];
      this._selectedItem = next;
      this._timer.Stop();
      if (num > dataContext)
        this.MoveToFront(selectedItem, next, (double) (dataContext * 10));
      else
        this.MoveToNext(selectedItem, next, (double) (dataContext * 10));
      this._timer.Start();
    }

    private void MoveToFront(CarouselItemViewModel current, CarouselItemViewModel next, double x)
    {
      this.FrontImage.Source = (ImageSource) new BitmapImage(new Uri(next.ImageUrl));
      this.NextImage.Source = (ImageSource) new BitmapImage(new Uri(current.ImageUrl));
      Storyboard resource = (Storyboard) this.FindResource((object) "MoveToFrontStory");
      ((DoubleAnimation) resource.Children[3]).To = new double?(x);
      resource.Begin();
      this.Title.Text = next.Title;
      this.SubTitle.Text = next.SubTitle;
    }

    private void MoveToNext(CarouselItemViewModel current, CarouselItemViewModel next, double x)
    {
      try
      {
        this.FrontImage.Source = (ImageSource) new BitmapImage(new Uri(current.ImageUrl));
        this.NextImage.Source = (ImageSource) new BitmapImage(new Uri(next.ImageUrl));
        Storyboard resource = (Storyboard) this.FindResource((object) "MoveToNextStory");
        ((DoubleAnimation) resource.Children[3]).To = new double?(x);
        resource.Begin();
        this.Title.Text = next.Title;
        this.SubTitle.Text = next.SubTitle;
      }
      catch (Exception ex)
      {
        UtilLog.Info("CarouselControl.MoveNext " + current.ImageUrl + "  " + next.ImageUrl);
      }
    }

    private void MoveSlideBar(double x)
    {
      Storyboard resource = (Storyboard) this.FindResource((object) "MoveSlideBarStory");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/carouselcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Title = (TextBlock) target;
          break;
        case 2:
          this.SubTitle = (TextBlock) target;
          break;
        case 3:
          this.FirstColumn = (ColumnDefinition) target;
          break;
        case 4:
          this.FrontImage = (Image) target;
          break;
        case 5:
          this.NextImage = (Image) target;
          break;
        case 6:
          this.SelectableItems = (ItemsControl) target;
          break;
        case 8:
          this.SlideButton = (Border) target;
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
      if (connectionId != 7)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
    }
  }
}
