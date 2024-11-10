// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.ArrangeTaskItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class ArrangeTaskItem : UserControl, IComponentConnector
  {
    private ArrangeTaskPanel _parent;
    private SolidColorBrush _outDateBrush = ThemeUtil.GetColor("OutDateColor");
    private SolidColorBrush _dateBrush = ThemeUtil.GetColor("ThemeBlue");
    internal TaskBar TaskBar;
    internal StackPanel ArrangeSection;
    internal Path OpenIndicator;
    internal TextBlock CountText;
    private bool _contentLoaded;

    public ArrangeTaskItem()
    {
      this.InitializeComponent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
      this.ArrangeSection.MouseLeftButtonUp -= new MouseButtonEventHandler(this.FoldingClick);
      this.ArrangeSection.MouseLeftButtonUp += new MouseButtonEventHandler(this.FoldingClick);
      this.TaskBar.TimeText.Foreground = (Brush) this._outDateBrush;
      this.TaskBar.TimeText.Opacity = 1.0;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is CalendarDisplayViewModel dataContext))
        return;
      if (dataContext.IsSection)
      {
        this.OpenIndicator.RenderTransform = (Transform) new RotateTransform(dataContext.IsOpened ? 0.0 : -90.0);
        this.CountText.Text = dataContext.Children?.Count.ToString() ?? "";
      }
      else if (dataContext.SourceViewModel.OutDate())
        this.TaskBar.TimeText.Foreground = (Brush) this._outDateBrush;
      else
        this.TaskBar.TimeText.Foreground = (Brush) this._dateBrush;
    }

    private ArrangeTaskPanel GetParent()
    {
      return this._parent = this._parent ?? Utils.FindParent<ArrangeTaskPanel>((DependencyObject) this);
    }

    private async void FoldingClick(object sender, MouseButtonEventArgs e)
    {
      ArrangeTaskItem arrangeTaskItem = this;
      arrangeTaskItem.ArrangeSection.MouseLeftButtonUp -= new MouseButtonEventHandler(arrangeTaskItem.FoldingClick);
      if (arrangeTaskItem.DataContext is CalendarDisplayViewModel dataContext)
      {
        (dataContext.IsOpened ? (Storyboard) arrangeTaskItem.FindResource((object) "CloseAnim") : (Storyboard) arrangeTaskItem.FindResource((object) "OpenAnim")).Begin();
        dataContext.IsOpened = !dataContext.IsOpened;
        if (dataContext.IsOpened)
          ArrangeSectionStatusDao.DeleteSectionStatusModel(new ArrangeSectionStatusModel()
          {
            Name = dataContext.Title,
            Type = LocalSettings.Settings.ArrangeDisplayType
          });
        else
          ArrangeSectionStatusDao.AddSectionStatusModel(new ArrangeSectionStatusModel()
          {
            Name = dataContext.Title,
            Type = LocalSettings.Settings.ArrangeDisplayType,
            UserId = LocalSettings.Settings.LoginUserId
          });
        if (arrangeTaskItem.GetParent() != null)
          await arrangeTaskItem._parent.OnSectionStatusChanged(dataContext.IsOpened, dataContext);
      }
      arrangeTaskItem.ArrangeSection.MouseLeftButtonUp += new MouseButtonEventHandler(arrangeTaskItem.FoldingClick);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/arrangetaskitem.xaml", UriKind.Relative));
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
          this.TaskBar = (TaskBar) target;
          break;
        case 2:
          this.ArrangeSection = (StackPanel) target;
          break;
        case 3:
          this.OpenIndicator = (Path) target;
          break;
        case 4:
          this.CountText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
