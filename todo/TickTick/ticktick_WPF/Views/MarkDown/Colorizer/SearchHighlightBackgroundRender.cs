// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Colorizer.SearchHighlightBackgroundRender
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.Colorizer
{
  public class SearchHighlightBackgroundRender : IBackgroundRenderer
  {
    private TextEditor _editor;
    private bool _textChanged;
    private Dictionary<int, LinkInfo> _linkDict;
    private bool _isTitle;
    public bool InDetail;
    private bool _inPreSearch;

    public SearchHighlightBackgroundRender(
      TextEditor editor,
      Dictionary<int, LinkInfo> linkUrlDict,
      bool isTitle,
      bool inDetail,
      bool inPreSearch = false)
    {
      this._editor = editor;
      this._linkDict = linkUrlDict;
      this._isTitle = isTitle;
      this.InDetail = inDetail;
      this._inPreSearch = inPreSearch;
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      if (!this._inPreSearch && (!LocalSettings.Settings.InSearch || string.IsNullOrEmpty(SearchHelper.SearchKey) || this.InDetail && SearchHelper.DetailChanged))
        return;
      if (this._textChanged)
      {
        this.Segments.Clear();
        foreach (VisualLine visualLine in (IEnumerable<VisualLine>) textView.VisualLines.AsParallel<VisualLine>())
        {
          int offset = visualLine.FirstDocumentLine.Offset;
          int length = visualLine.LastDocumentLine.EndOffset - offset;
          string lower = this._editor.Document.GetText(offset, length).ToLower();
          MatchCollection matchCollection = (this._inPreSearch ? SearchHelper.PreSearchRegex : SearchHelper.SearchRegex).Matches(lower);
          for (int i = 0; i < matchCollection.Count; ++i)
          {
            int num1 = visualLine.FirstDocumentLine.Offset + matchCollection[i].Index;
            bool flag = false;
            foreach (KeyValuePair<int, LinkInfo> keyValuePair in this._linkDict)
            {
              int num2 = keyValuePair.Key - 1;
              int num3 = num2 + keyValuePair.Value.Url.Length + 1;
              if (num1 >= num2 && num1 <= num3)
              {
                flag = true;
                break;
              }
            }
            if (!flag)
              this.Segments.Add(new TextSegment()
              {
                StartOffset = num1,
                Length = matchCollection[i].Length
              });
          }
        }
      }
      this._textChanged = false;
      foreach (TextSegment segment in this.Segments)
      {
        foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, (ISegment) segment))
        {
          if (rect.Width > 4.0)
            drawingContext.DrawRoundedRectangle(SearchHelper.GetSearchHighlightColor(), (Pen) null, new Rect(rect.Left, rect.Top, rect.Width + 1.0, rect.Height), 0.0, 0.0);
        }
      }
    }

    public KnownLayer Layer { get; }

    public List<TextSegment> Segments { get; set; } = new List<TextSegment>();

    public void SetTextChanged(bool b) => this._textChanged = true;

    public int? GetFirstIndex() => this.Segments.FirstOrDefault<TextSegment>()?.StartOffset;
  }
}
