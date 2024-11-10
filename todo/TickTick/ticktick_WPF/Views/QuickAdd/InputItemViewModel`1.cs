// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.InputItemViewModel`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class InputItemViewModel<T> : InputItemViewModel
  {
    public T Entity { get; }

    public InputItemViewModel(string title, string value, string url, T entity)
    {
      this.Title = title;
      this.Value = value;
      this.ImageUrl = url;
      this.Entity = entity;
      ticktick_WPF.Util.Pinyin pinyin = PinyinUtils.ToPinyin(title);
      this.Pinyin = pinyin.Text;
      this.Inits = pinyin.Inits;
    }

    public InputItemViewModel(string title, string value, T entity, bool needPy = true)
    {
      this.Title = title;
      this.Value = value;
      this.Entity = entity;
      if (!needPy)
        return;
      ticktick_WPF.Util.Pinyin pinyin = PinyinUtils.ToPinyin(title);
      this.Pinyin = pinyin.Text;
      this.Inits = pinyin.Inits;
    }

    public InputItemViewModel(string title, T entity)
    {
      this.Title = title;
      this.Value = title;
      this.Entity = entity;
      ticktick_WPF.Util.Pinyin pinyin = PinyinUtils.ToPinyin(title);
      this.Pinyin = pinyin.Text;
      this.Inits = pinyin.Inits;
    }
  }
}
