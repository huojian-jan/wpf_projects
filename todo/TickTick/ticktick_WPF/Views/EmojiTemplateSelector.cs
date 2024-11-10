// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.EmojiTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views
{
  public class EmojiTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (!(item is EmojiSelectViewModel emojiSelectViewModel) || !(container is FrameworkElement frameworkElement))
        return (DataTemplate) null;
      if (emojiSelectViewModel.IsEmojiItems)
        return frameworkElement.FindResource((object) "EmojiItem") as DataTemplate;
      return emojiSelectViewModel.IsSection ? frameworkElement.FindResource((object) "SectionItem") as DataTemplate : frameworkElement.FindResource((object) "TextItem") as DataTemplate;
    }
  }
}
