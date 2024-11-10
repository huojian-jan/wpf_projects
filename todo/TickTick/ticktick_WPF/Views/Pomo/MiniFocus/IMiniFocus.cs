// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.IMiniFocus
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public interface IMiniFocus
  {
    void Init();

    void SetOpacity(double opacity);

    void SetCountText();

    void OnStatusChanged();

    void OnFocusTypeChanged();

    double GetLeftMargin();

    double GetActualWidth();

    double GetActualHeight();

    void SetMoving(bool b);

    bool CanDragMove();

    bool CanHide();

    void SetHideStyle(bool isHide);

    void OnWindowStartHide();

    double GetExtraWidth();
  }
}
