// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusObjIcon
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusObjIcon : Border
  {
    private Path _icon;
    private EmjTextBlock _iconText;
    private double _width;

    public FocusObjIcon(double size)
    {
      this._width = size;
      this.Height = size;
      this.Width = size;
      this.Background = (Brush) Brushes.Transparent;
      this.InitIcon(size);
    }

    private void InitIcon(double size)
    {
      Path path = new Path();
      path.Height = size - 6.0;
      path.Width = size - 6.0;
      path.Stretch = Stretch.Uniform;
      path.HorizontalAlignment = HorizontalAlignment.Center;
      path.VerticalAlignment = VerticalAlignment.Center;
      this._icon = path;
      EmjTextBlock emjTextBlock = new EmjTextBlock();
      emjTextBlock.FontSize = size - 10.0;
      emjTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
      emjTextBlock.VerticalAlignment = VerticalAlignment.Center;
      emjTextBlock.TextAlignment = TextAlignment.Center;
      this._iconText = emjTextBlock;
      this.Child = (UIElement) this._icon;
      this.SetIcon();
      this.SetFocusIcon((string) null, 0);
    }

    public void SetIcon()
    {
      if (this._icon == null)
        return;
      this._icon.SetResourceReference(Shape.FillProperty, TickFocusManager.Status == PomoStatus.Relaxing ? (object) "PomoGreen" : (object) "PrimaryColor");
      this._icon.Data = Utils.GetIcon(TickFocusManager.IsPomo ? "IcPomo" : "IcPomoTimer");
    }

    public void SetIconAndColor(string icon, string color)
    {
      this.Background = (Brush) new ImageBrush()
      {
        ImageSource = (ImageSource) HabitService.GetIcon(icon, color)
      };
      this.Child = (UIElement) null;
    }

    public async void SetFocusIcon(string id, int type)
    {
      FocusObjIcon focusObjIcon = this;
      if (!string.IsNullOrWhiteSpace(id))
      {
        bool flag = type == 2;
        if (type == 1 | flag)
        {
          string icon = (string) null;
          string color = (string) null;
          if (flag)
          {
            TimerModel timerById = await TimerDao.GetTimerById(id);
            if (timerById != null)
            {
              icon = timerById.Icon;
              color = timerById.Color;
            }
          }
          else
          {
            HabitModel habitById = await HabitDao.GetHabitById(id);
            if (habitById != null)
            {
              icon = habitById.IconRes;
              color = habitById.Color;
            }
          }
          if (!string.IsNullOrEmpty(icon))
          {
            focusObjIcon.SetIconAndColor(icon, color);
            return;
          }
          icon = (string) null;
          color = (string) null;
        }
        else
        {
          ProjectModel projectById = CacheManager.GetProjectById(TaskCache.GetTaskById(id)?.ProjectId);
          if (projectById != null && EmojiHelper.StartWithEmoji(projectById.name))
          {
            focusObjIcon.Child = (UIElement) focusObjIcon._iconText;
            focusObjIcon._iconText.FontSize = focusObjIcon._width - 6.0;
            focusObjIcon._iconText.Margin = new Thickness(-3.0, -2.0, 0.0, 0.0);
            focusObjIcon._iconText.Text = EmojiHelper.GetEmojiIcon(projectById.name);
            return;
          }
        }
      }
      focusObjIcon.Child = (UIElement) focusObjIcon._icon;
      focusObjIcon.Background = (Brush) Brushes.Transparent;
    }
  }
}
