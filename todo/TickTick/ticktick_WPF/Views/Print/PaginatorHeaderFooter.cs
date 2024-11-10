// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.PaginatorHeaderFooter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class PaginatorHeaderFooter : DocumentPaginator
  {
    private readonly DocumentPaginator _mPaginator;
    private double _designWidth;

    public PaginatorHeaderFooter(DocumentPaginator paginator, double designWidth = 793.0)
    {
      this._designWidth = designWidth;
      this._mPaginator = paginator;
    }

    public override DocumentPage GetPage(int pageNumber)
    {
      DocumentPage page = this._mPaginator.GetPage(pageNumber);
      ContainerVisual containerVisual = new ContainerVisual();
      double num = 793.0 / this._designWidth;
      DrawingVisual drawingVisual1 = new DrawingVisual();
      using (DrawingContext drawingContext = drawingVisual1.RenderOpen())
      {
        BitmapImage image = Utils.GetImage("pack://application:,,,/Assets/printDida.png");
        drawingContext.DrawImage((ImageSource) image, new Rect(page.ContentBox.Left, page.ContentBox.Bottom + 6.5 / num, 122.0 / num, 24.0 / num));
      }
      DrawingVisual drawingVisual2 = new DrawingVisual();
      using (DrawingContext drawingContext1 = drawingVisual2.RenderOpen())
      {
        FormattedText formattedText1 = new FormattedText(string.Format("page {0}/{1}", (object) (pageNumber + 1), (object) this.PageCount), CultureInfo.CurrentCulture, FlowDirection.RightToLeft, new Typeface("Courier New"), 10.0 / num, (Brush) ThemeUtil.GetAlphaColor("#191919", 56));
        DrawingContext drawingContext2 = drawingContext1;
        FormattedText formattedText2 = formattedText1;
        Rect contentBox = page.ContentBox;
        double right = contentBox.Right;
        contentBox = page.ContentBox;
        double y = contentBox.Bottom + 13.0 / num;
        Point origin = new Point(right, y);
        drawingContext2.DrawText(formattedText2, origin);
      }
      containerVisual.Children.Add((Visual) new ContainerVisual()
      {
        Children = {
          page.Visual
        }
      });
      containerVisual.Children.Add((Visual) drawingVisual1);
      containerVisual.Children.Add((Visual) drawingVisual2);
      containerVisual.Transform = (Transform) new ScaleTransform(num, num, 0.0, 0.0);
      return new DocumentPage((Visual) containerVisual, page.Size == Size.Empty ? page.Size : new Size(page.Size.Width * num, page.Size.Height * num), page.BleedBox, page.ContentBox);
    }

    public override bool IsPageCountValid => this._mPaginator.IsPageCountValid;

    public override int PageCount => this._mPaginator.PageCount;

    public override Size PageSize
    {
      get => this._mPaginator.PageSize;
      set => this._mPaginator.PageSize = value;
    }

    public override IDocumentPaginatorSource Source => this._mPaginator.Source;
  }
}
