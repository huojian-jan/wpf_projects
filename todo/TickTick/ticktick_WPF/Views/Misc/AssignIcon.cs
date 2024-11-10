// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.AssignIcon
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class AssignIcon : Border
  {
    private bool? _empty;

    public AssignIcon()
    {
      this.VerticalAlignment = VerticalAlignment.Center;
      this.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public async void SetAvatar(string avatarUrl)
    {
      AssignIcon assignIcon = this;
      bool empty = string.IsNullOrEmpty(avatarUrl);
      if (assignIcon._empty.HasValue)
      {
        bool? empty1 = assignIcon._empty;
        bool flag = empty;
        if (empty1.GetValueOrDefault() == flag & empty1.HasValue)
          goto label_5;
      }
      if (empty)
      {
        // ISSUE: explicit non-virtual call
        __nonvirtual (assignIcon.Width) = 28.0;
        // ISSUE: explicit non-virtual call
        __nonvirtual (assignIcon.Height) = 28.0;
        assignIcon.BorderBrush = (Brush) Brushes.Transparent;
        HoverIconButton hoverIconButton1 = new HoverIconButton();
        hoverIconButton1.VerticalAlignment = VerticalAlignment.Center;
        HoverIconButton hoverIconButton2 = hoverIconButton1;
        hoverIconButton2.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) "AssigntoDrawingImage");
        assignIcon.Child = (UIElement) hoverIconButton2;
        assignIcon.BorderThickness = new Thickness(0.0);
        assignIcon.Margin = new Thickness(0.0);
        assignIcon.Background = (Brush) Brushes.Transparent;
        assignIcon.CornerRadius = new CornerRadius(0.0);
      }
      else
      {
        // ISSUE: explicit non-virtual call
        __nonvirtual (assignIcon.Width) = 22.0;
        // ISSUE: explicit non-virtual call
        __nonvirtual (assignIcon.Height) = 22.0;
        assignIcon.Margin = new Thickness(2.0);
        assignIcon.CornerRadius = new CornerRadius(10.0);
        assignIcon.BorderThickness = new Thickness(1.0);
        assignIcon.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity5");
        assignIcon.Child = (UIElement) null;
      }
label_5:
      if (!empty)
      {
        BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(avatarUrl);
        assignIcon.Background = (Brush) new ImageBrush()
        {
          ImageSource = (ImageSource) avatarByUrlAsync
        };
      }
      assignIcon._empty = new bool?(empty);
    }
  }
}
