// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.LinkUrlTextElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class LinkUrlTextElement : VisualLineText
  {
    private readonly ILinkTextEditor _editor;
    private readonly string _url;
    private readonly bool _showUnderLine;

    public LinkUrlTextElement(
      VisualLine parentVisualLine,
      int length,
      string url,
      ILinkTextEditor editor,
      bool showUnderLine)
      : base(parentVisualLine, length)
    {
      this._url = url;
      this._showUnderLine = showUnderLine;
      this._editor = editor;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      this.TextRunProperties.SetForegroundBrush(this._editor.GetHighLightColor());
      if (this._showUnderLine)
        this.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
      return base.CreateTextRun(startVisualColumn, context);
    }

    protected override void OnQueryCursor(QueryCursorEventArgs e)
    {
      e.Handled = true;
      e.Cursor = Cursors.Hand;
      base.OnQueryCursor(e);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left || e.Handled)
        return;
      e.Handled = true;
      if (string.IsNullOrEmpty(this._url))
        return;
      ProjectTask taskUrlWithoutTitle = TaskUtils.ParseTaskUrlWithoutTitle(this._url);
      if (taskUrlWithoutTitle != null)
        this._editor.NavigateTask(taskUrlWithoutTitle.ProjectId, taskUrlWithoutTitle.TaskId);
      else
        this.OpenUrl();
    }

    private void OpenUrl()
    {
      try
      {
        Process.Start(this._url);
      }
      catch (Exception ex)
      {
      }
    }
  }
}
