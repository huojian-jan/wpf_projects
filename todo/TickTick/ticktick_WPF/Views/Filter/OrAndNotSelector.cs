// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.OrAndNotSelector
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
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class OrAndNotSelector : StackPanel, IComponentConnector
  {
    internal Grid OrGrid;
    internal Border OrBorder;
    internal Grid AndGrid;
    internal Border AndBorder;
    internal Grid NotGrid;
    private bool _contentLoaded;

    public OrAndNotSelector() => this.InitializeComponent();

    public event EventHandler<LogicType> OnLogicChanged;

    private void SelectOrLogic(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is FilterConditionViewModel dataContext))
        return;
      dataContext.Logic = LogicType.Or;
      EventHandler<LogicType> onLogicChanged = this.OnLogicChanged;
      if (onLogicChanged == null)
        return;
      onLogicChanged((object) this, LogicType.Or);
    }

    private void SelectAndLogic(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is FilterConditionViewModel dataContext))
        return;
      if (dataContext.AllLogicEnabled)
      {
        dataContext.Logic = LogicType.And;
        EventHandler<LogicType> onLogicChanged = this.OnLogicChanged;
        if (onLogicChanged == null)
          return;
        onLogicChanged((object) this, LogicType.And);
      }
      else
        Utils.Toast(Utils.GetString("CannotSelectNoTagAndLogicAndAtTheSameTime"));
    }

    private void SelectNotLogic(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is FilterConditionViewModel dataContext))
        return;
      if (dataContext.WithTagsSelected && dataContext.WithoutTagsSelected)
      {
        Utils.Toast(Utils.GetString("CannotSelectWithTagNoTagAndLogicNotAtTheSameTime"));
      }
      else
      {
        dataContext.Logic = LogicType.Not;
        EventHandler<LogicType> onLogicChanged = this.OnLogicChanged;
        if (onLogicChanged == null)
          return;
        onLogicChanged((object) this, LogicType.Not);
      }
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is FilterConditionViewModel newValue))
        return;
      bool flag1 = newValue.SupportedLogic.Contains(LogicType.And);
      bool flag2 = newValue.SupportedLogic.Contains(LogicType.Not);
      int num1 = flag1 | flag2 ? 0 : 4;
      this.OrBorder.CornerRadius = new CornerRadius(4.0, (double) num1, (double) num1, 4.0);
      if (flag1)
      {
        int num2 = flag2 ? 0 : 4;
        this.AndGrid.Visibility = Visibility.Visible;
        this.AndBorder.CornerRadius = new CornerRadius(0.0, (double) num2, (double) num2, 0.0);
      }
      if (!flag2)
        return;
      this.NotGrid.Visibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/orandnotselector.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          break;
        case 2:
          this.OrGrid = (Grid) target;
          this.OrGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectOrLogic);
          break;
        case 3:
          this.OrBorder = (Border) target;
          break;
        case 4:
          this.AndGrid = (Grid) target;
          this.AndGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectAndLogic);
          break;
        case 5:
          this.AndBorder = (Border) target;
          break;
        case 6:
          this.NotGrid = (Grid) target;
          this.NotGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectNotLogic);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
