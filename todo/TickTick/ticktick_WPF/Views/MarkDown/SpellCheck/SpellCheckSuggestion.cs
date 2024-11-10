// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.SpellCheck.SpellCheckSuggestion
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.SpellCheck
{
  public static class SpellCheckSuggestion
  {
    private static bool IsAlternateAppsKeyShortcut
    {
      get
      {
        if (!Keyboard.IsKeyDown(Key.F10))
          return false;
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
      }
    }

    public static void SpellCheckSuggestions(ISpellChecker editor, ContextMenu contextMenu)
    {
      if (editor.SpellCheckProvider == null)
        return;
      int offset;
      if (SpellCheckSuggestion.IsAlternateAppsKeyShortcut)
      {
        offset = editor.GetEditBox().SelectionStart;
      }
      else
      {
        TextViewPosition? positionFromPoint = editor.GetEditBox().GetPositionFromPoint(Mouse.GetPosition((IInputElement) editor.GetEditBox()));
        if (!positionFromPoint.HasValue)
          return;
        TextDocument document = editor.GetEditBox().Document;
        TextViewPosition textViewPosition = positionFromPoint.Value;
        int line = textViewPosition.Line;
        textViewPosition = positionFromPoint.Value;
        int column = textViewPosition.Column;
        offset = document.GetOffset(line, column);
      }
      TextSegment textSegment = editor.SpellCheckProvider.GetSpellCheckErrors().FirstOrDefault<TextSegment>((Func<TextSegment, bool>) (segment => segment.StartOffset <= offset && segment.EndOffset >= offset && segment.EndOffset <= editor.GetEditBox().Text.Length));
      if (textSegment == null)
        return;
      DocumentLine lineByOffset = editor.GetEditBox().Document.GetLineByOffset(offset);
      if (offset == lineByOffset.Offset || offset == lineByOffset.EndOffset)
        return;
      string text = editor.GetEditBox().Document.GetText((ISegment) textSegment);
      List<string> list = editor.SpellCheckProvider.GetSpellCheckSuggestions(text).ToList<string>();
      if (list.Count > 0)
      {
        foreach (string header in list)
          contextMenu.Items.Add((object) SpellCheckSuggestion.SpellSuggestMenuItem(editor, header, textSegment));
      }
      else
      {
        ItemCollection items = contextMenu.Items;
        MenuItem newItem = new MenuItem();
        newItem.Header = (object) Utils.GetString("NoSpellSuggest");
        newItem.IsEnabled = false;
        items.Add((object) newItem);
      }
      contextMenu.Items.Add((object) new Separator());
      ItemCollection items1 = contextMenu.Items;
      MenuItem newItem1 = new MenuItem();
      newItem1.Header = (object) Utils.GetString("IgnoreSpelling");
      newItem1.Command = SpellCheckUtils.IgnoreSpellingError;
      newItem1.CommandParameter = (object) new SpellCheckHandler(editor, "", text, textSegment);
      items1.Add((object) newItem1);
    }

    private static MenuItem SpellSuggestMenuItem(
      ISpellChecker editor,
      string header,
      TextSegment segment)
    {
      MenuItem menuItem = new MenuItem();
      menuItem.Header = (object) header;
      menuItem.FontWeight = FontWeights.Bold;
      menuItem.Command = SpellCheckUtils.CorrectSpellingError;
      menuItem.CommandParameter = (object) new SpellCheckHandler(editor, header, (string) null, segment);
      return menuItem;
    }

    public static void AddSpellCheckLanguageMenu(ContextMenu contextMenu)
    {
      bool spellCheckEnable = LocalSettings.Settings.SpellCheckEnable;
      ItemCollection items1 = contextMenu.Items;
      MenuItem newItem1 = new MenuItem();
      newItem1.Header = (object) Utils.GetString("SpellingCheck");
      ItemCollection items2 = newItem1.Items;
      MenuItem newItem2 = new MenuItem();
      newItem2.Header = (object) Utils.GetString(spellCheckEnable ? "CloseSpellCheck" : "OpenSpellCheck");
      newItem2.Command = SpellCheckUtils.OpenOrClose;
      newItem2.CommandParameter = (object) !spellCheckEnable;
      items2.Add((object) newItem2);
      newItem1.Items.Add((object) new Separator());
      ItemCollection items3 = newItem1.Items;
      MenuItem newItem3 = new MenuItem();
      newItem3.Header = (object) "English(US)";
      newItem3.Command = SpellCheckUtils.LanguageSelected;
      newItem3.CommandParameter = (object) "en-US";
      newItem3.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "en-US";
      newItem3.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items3.Add((object) newItem3);
      ItemCollection items4 = newItem1.Items;
      MenuItem newItem4 = new MenuItem();
      newItem4.Header = (object) "English(UK)";
      newItem4.Command = SpellCheckUtils.LanguageSelected;
      newItem4.CommandParameter = (object) "en-GB";
      newItem4.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "en-GB";
      newItem4.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items4.Add((object) newItem4);
      ItemCollection items5 = newItem1.Items;
      MenuItem newItem5 = new MenuItem();
      newItem5.Header = (object) "Fançais";
      newItem5.Command = SpellCheckUtils.LanguageSelected;
      newItem5.CommandParameter = (object) "fr-FR";
      newItem5.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "fr-FR";
      newItem5.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items5.Add((object) newItem5);
      ItemCollection items6 = newItem1.Items;
      MenuItem newItem6 = new MenuItem();
      newItem6.Header = (object) "Pусский";
      newItem6.Command = SpellCheckUtils.LanguageSelected;
      newItem6.CommandParameter = (object) "ru-RU";
      newItem6.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "ru-RU";
      newItem6.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items6.Add((object) newItem6);
      ItemCollection items7 = newItem1.Items;
      MenuItem newItem7 = new MenuItem();
      newItem7.Header = (object) "Deutsche";
      newItem7.Command = SpellCheckUtils.LanguageSelected;
      newItem7.CommandParameter = (object) "de-DE";
      newItem7.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "de-DE";
      newItem7.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items7.Add((object) newItem7);
      ItemCollection items8 = newItem1.Items;
      MenuItem newItem8 = new MenuItem();
      newItem8.Header = (object) "Español";
      newItem8.Command = SpellCheckUtils.LanguageSelected;
      newItem8.CommandParameter = (object) "es-ES";
      newItem8.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "es-ES";
      newItem8.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items8.Add((object) newItem8);
      ItemCollection items9 = newItem1.Items;
      MenuItem newItem9 = new MenuItem();
      newItem9.Header = (object) "Português (Brasil)";
      newItem9.Command = SpellCheckUtils.LanguageSelected;
      newItem9.CommandParameter = (object) "pt-BR";
      newItem9.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "pt-BR";
      newItem9.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items9.Add((object) newItem9);
      ItemCollection items10 = newItem1.Items;
      MenuItem newItem10 = new MenuItem();
      newItem10.Header = (object) "Português (Portugal)";
      newItem10.Command = SpellCheckUtils.LanguageSelected;
      newItem10.CommandParameter = (object) "pt-PT";
      newItem10.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "pt-PT";
      newItem10.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items10.Add((object) newItem10);
      ItemCollection items11 = newItem1.Items;
      MenuItem newItem11 = new MenuItem();
      newItem11.Header = (object) "Italiano";
      newItem11.Command = SpellCheckUtils.LanguageSelected;
      newItem11.CommandParameter = (object) "it-IT";
      newItem11.IsChecked = LocalSettings.Settings.SpellCheckLanguage == "it-IT";
      newItem11.IsEnabled = spellCheckEnable && !SpellCheckUtils.Downloading;
      items11.Add((object) newItem11);
      items1.Add((object) newItem1);
      contextMenu.Items.Add((object) new Separator());
    }
  }
}
