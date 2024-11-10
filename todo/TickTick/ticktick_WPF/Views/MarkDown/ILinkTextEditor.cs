// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ILinkTextEditor
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public interface ILinkTextEditor
  {
    Dictionary<int, LinkInfo> GetLinkNameDict();

    Dictionary<int, LinkInfo> GetLinkUrlDict();

    void NavigateTask(string projectTaskProjectId, string projectTaskTaskId);

    void ShowInsertLink(string name, string url, VisualLine line, bool b);

    void UnRegisterCaretChanged();

    void RegisterCaretChanged();

    TextEditor GetEditBox();

    Brush GetHighLightColor();

    Brush GetBracketColor();
  }
}
