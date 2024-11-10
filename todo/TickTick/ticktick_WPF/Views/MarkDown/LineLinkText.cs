// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LineLinkText
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Input;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class LineLinkText : VisualLineLinkText
  {
    private string _url;
    private ILinkTextEditor _editor;

    public LineLinkText(VisualLine parentVisualLine, string url, ILinkTextEditor editor)
      : base(parentVisualLine, url.Length)
    {
      this._url = url;
      this._editor = editor;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left || e.Handled || string.IsNullOrEmpty(this._url))
        return;
      ProjectTask taskUrlWithoutTitle = TaskUtils.ParseTaskUrlWithoutTitle(this._url);
      if (taskUrlWithoutTitle != null)
        this._editor.NavigateTask(taskUrlWithoutTitle.ProjectId, taskUrlWithoutTitle.TaskId);
      else
        base.OnMouseDown(e);
    }
  }
}
