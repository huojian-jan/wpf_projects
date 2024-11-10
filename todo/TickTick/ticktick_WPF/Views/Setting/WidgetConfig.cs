// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.WidgetConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class WidgetConfig : FeatureConfig
  {
    public WidgetConfig() => this.InitializeComponent();

    protected override void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox1) || !(checkBox1.DataContext is WidgetConfig.WidgetViewModel dataContext))
        return;
      CheckBox checkBox2 = checkBox1;
      bool? isChecked = checkBox1.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      checkBox2.IsChecked = nullable;
      dataContext.IsOpen = checkBox1.IsChecked.GetValueOrDefault();
      switch (dataContext.Value)
      {
        case WidgetType.Calendar:
          if (ProChecker.CheckPro(ProType.CalendarWidget))
          {
            if (dataContext.IsOpen)
            {
              CalendarWidgetHelper.AddWidget();
              break;
            }
            CalendarWidgetHelper.CloseWidget();
            break;
          }
          break;
        case WidgetType.Matrix:
          if (ProChecker.CheckPro(ProType.MatrixWidget))
          {
            if (dataContext.IsOpen)
            {
              MatrixWidgetHelper.AddWidget();
              break;
            }
            MatrixWidgetHelper.CloseWidget();
            break;
          }
          break;
      }
      dataContext.NotifySettingTextChanged();
    }

    internal class WidgetViewModel : FeatureViewModel
    {
      public WidgetType Value { get; set; }

      public WidgetViewModel(WidgetType type, bool? isOpen)
      {
        this.Value = type;
        this.Title = this.GetTitle(type);
        this.Desc = Utils.GetString(type.ToString() + "WidgetDesc");
        this.IsOpen = isOpen.GetValueOrDefault();
        this.ShowOpen = isOpen.HasValue;
      }

      private string GetTitle(WidgetType type) => Utils.GetString(type.ToString() + "Widget");
    }
  }
}
