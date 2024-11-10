// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.LoadingIndicator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections;
using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views
{
  [TemplatePart(Name = "Border", Type = typeof (Border))]
  public class LoadingIndicator : Control
  {
    public static readonly DependencyProperty SpeedRatioProperty = DependencyProperty.Register(nameof (SpeedRatio), typeof (double), typeof (LoadingIndicator), new PropertyMetadata((object) 1.0, (PropertyChangedCallback) ((o, e) =>
    {
      LoadingIndicator loadingIndicator = (LoadingIndicator) o;
      if (loadingIndicator.PartBorder == null || !loadingIndicator.IsActive)
        return;
      foreach (VisualStateGroup visualStateGroup in (IEnumerable) VisualStateManager.GetVisualStateGroups((FrameworkElement) loadingIndicator.PartBorder))
      {
        if (visualStateGroup.Name == "ActiveStates")
        {
          foreach (VisualState state in (IEnumerable) visualStateGroup.States)
          {
            if (state.Name == "Active")
              state.Storyboard.SetSpeedRatio((FrameworkElement) loadingIndicator.PartBorder, (double) e.NewValue);
          }
        }
      }
    })));
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof (IsActive), typeof (bool), typeof (LoadingIndicator), new PropertyMetadata((object) true, (PropertyChangedCallback) ((o, e) =>
    {
      LoadingIndicator loadingIndicator = (LoadingIndicator) o;
      if (loadingIndicator.PartBorder == null)
        return;
      if (!(bool) e.NewValue)
      {
        VisualStateManager.GoToElementState((FrameworkElement) loadingIndicator.PartBorder, "Inactive", false);
        loadingIndicator.PartBorder.Visibility = Visibility.Collapsed;
      }
      else
      {
        VisualStateManager.GoToElementState((FrameworkElement) loadingIndicator.PartBorder, "Active", false);
        loadingIndicator.PartBorder.Visibility = Visibility.Visible;
        foreach (VisualStateGroup visualStateGroup in (IEnumerable) VisualStateManager.GetVisualStateGroups((FrameworkElement) loadingIndicator.PartBorder))
        {
          if (visualStateGroup.Name == "ActiveStates")
          {
            foreach (VisualState state in (IEnumerable) visualStateGroup.States)
            {
              if (state.Name == "Active")
                state.Storyboard.SetSpeedRatio((FrameworkElement) loadingIndicator.PartBorder, loadingIndicator.SpeedRatio);
            }
          }
        }
      }
    })));
    protected Border PartBorder;

    public double SpeedRatio
    {
      get => (double) this.GetValue(LoadingIndicator.SpeedRatioProperty);
      set => this.SetValue(LoadingIndicator.SpeedRatioProperty, (object) value);
    }

    public bool IsActive
    {
      get => (bool) this.GetValue(LoadingIndicator.IsActiveProperty);
      set => this.SetValue(LoadingIndicator.IsActiveProperty, (object) value);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.PartBorder = (Border) this.GetTemplateChild("PART_Border");
      if (this.PartBorder == null)
        return;
      VisualStateManager.GoToElementState((FrameworkElement) this.PartBorder, this.IsActive ? "Active" : "Inactive", false);
      foreach (VisualStateGroup visualStateGroup in (IEnumerable) VisualStateManager.GetVisualStateGroups((FrameworkElement) this.PartBorder))
      {
        if (visualStateGroup.Name == "ActiveStates")
        {
          foreach (VisualState state in (IEnumerable) visualStateGroup.States)
          {
            if (state.Name == "Active")
              state.Storyboard.SetSpeedRatio((FrameworkElement) this.PartBorder, this.SpeedRatio);
          }
        }
      }
      this.PartBorder.Visibility = this.IsActive ? Visibility.Visible : Visibility.Collapsed;
    }
  }
}
