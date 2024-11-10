// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SystemToastManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util
{
  public class SystemToastManager
  {
    public static void Notify(
      string id,
      string group,
      string img,
      string title,
      string content,
      Dictionary<string, List<ToastCommands>> paras = null,
      List<ToastCommands> commands = null,
      bool silent = false)
    {
      ToastContentBuilder toastContentBuilder = new ToastContentBuilder().AddArgument("action", id).AddArgument("conversationId", 9813);
      string text = title;
      int? nullable = new int?(1);
      AdaptiveTextStyle? hintStyle = new AdaptiveTextStyle?();
      bool? hintWrap = new bool?();
      int? hintMaxLines = nullable;
      int? hintMinLines = new int?();
      AdaptiveTextAlign? hintAlign = new AdaptiveTextAlign?();
      ToastContentBuilder t1 = toastContentBuilder.AddText(text, hintStyle, hintWrap, hintMaxLines, hintMinLines, hintAlign).AddText(content).AddAppLogoOverride(new Uri(img), new ToastGenericAppLogoCrop?(ToastGenericAppLogoCrop.Default));
      SystemToastManager.AddToastInput(t1, paras);
      SystemToastManager.AddToastButtons(t1, commands);
      t1.SetToastScenario(ToastScenario.Reminder);
      if (silent)
        t1.AddAudio((Uri) null, new bool?(false), new bool?(true));
      t1.Show((CustomizeToast) (t =>
      {
        t.put_Tag(id);
        t.put_Group(group);
      }));
    }

    private static void AddToastInput(
      ToastContentBuilder t,
      Dictionary<string, List<ToastCommands>> paras)
    {
      if (paras == null || paras.Count == 0)
        return;
      foreach (KeyValuePair<string, List<ToastCommands>> para in paras)
      {
        if (para.Value != null && para.Value.Count != 0)
        {
          ToastSelectionBox input = new ToastSelectionBox(para.Key)
          {
            DefaultSelectionBoxItemId = para.Value[0].Argument
          };
          foreach (ToastCommands toastCommands in para.Value)
            input.Items.Add(new ToastSelectionBoxItem(toastCommands.Argument, toastCommands.Content));
          t.AddToastInput((IToastInput) input);
        }
      }
    }

    private static void AddToastButtons(ToastContentBuilder t, List<ToastCommands> commands)
    {
      if (commands == null || commands.Count == 0)
        return;
      foreach (ToastCommands command in commands)
        t.AddButton((IToastButton) new ToastButton().SetContent(command.Content).AddArgument("action", command.Argument));
    }
  }
}
