// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.PrintPreviewViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Threading;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class PrintPreviewViewModel : BaseViewModel
  {
    private int _pageCount;
    private DocumentPaginator _paginator;
    private PrintQueue _currentPrintQueue;
    private PageMediaSize _pageSize;
    private int _startIndex;
    private int _endIndex;
    private PrintQueueCollection _printQueues;
    private List<PageMediaSize> _pageMediaSizes;
    private PageOrientation _pageOrientation;
    private Dispatcher _dispatcher;

    public event EventHandler NeedReload;

    public int StartIndex
    {
      get => this._startIndex;
      set
      {
        if (value > this.EndIndex || value < 0)
          return;
        this._startIndex = value;
        this.OnPropertyChanged(nameof (StartIndex));
      }
    }

    public int EndIndex
    {
      get => this._endIndex;
      set
      {
        if (value < this.StartIndex || value > this.PageCount)
          return;
        this._endIndex = value;
        this.OnPropertyChanged(nameof (EndIndex));
      }
    }

    public int PageCount
    {
      get => this._pageCount;
      set
      {
        this._pageCount = value;
        if (this.EndIndex == 0 || this.EndIndex > value)
          this.EndIndex = value;
        if (this.StartIndex == 0 || this.StartIndex > value)
          this.StartIndex = 1;
        this.OnPropertyChanged(nameof (PageCount));
      }
    }

    public PrintQueue CurrentPrintQueue
    {
      get => this._currentPrintQueue;
      set
      {
        if (this._currentPrintQueue == value)
          return;
        this._currentPrintQueue = value;
        this.OnPropertyChanged(nameof (CurrentPrintQueue));
        this.LoadPageMediaSize();
      }
    }

    public PrintQueueCollection PrintQueues
    {
      get => this._printQueues;
      set
      {
        this._printQueues = value;
        this.OnPropertyChanged(nameof (PrintQueues));
      }
    }

    public PageMediaSize PageSize
    {
      get => this._pageSize;
      set
      {
        bool flag = !object.Equals((object) (double?) this._pageSize?.Width, (object) (double?) value?.Width) || !object.Equals((object) (double?) this._pageSize?.Height, (object) (double?) value?.Height);
        if (this._pageSize == value || value == null)
          return;
        this._pageSize = value;
        this.OnPropertyChanged(nameof (PageSize));
        if (!(this._paginator != null & flag))
          return;
        EventHandler needReload = this.NeedReload;
        if (needReload == null)
          return;
        needReload((object) null, (EventArgs) null);
      }
    }

    public List<PageMediaSize> PageMediaSizes
    {
      get => this._pageMediaSizes;
      set
      {
        this._pageMediaSizes = value;
        this.OnPropertyChanged(nameof (PageMediaSizes));
      }
    }

    public PageOrientation PageOrientation
    {
      get => this._pageOrientation;
      set
      {
        this._pageOrientation = value;
        this.OnPropertyChanged(nameof (PageOrientation));
        if (this._paginator == null)
          return;
        EventHandler needReload = this.NeedReload;
        if (needReload == null)
          return;
        needReload((object) null, (EventArgs) null);
      }
    }

    public PrintPreviewViewModel(DocumentPaginator paginator, bool isLandscape, bool pdfDefault)
    {
      this._paginator = paginator;
      this._dispatcher = Dispatcher.CurrentDispatcher;
      this.PageOrientation = isLandscape ? PageOrientation.Landscape : PageOrientation.Portrait;
      this.LoadPrintQueues(pdfDefault);
    }

    private void LoadPrintQueues(bool pdfDefault)
    {
      try
      {
        this.PrintQueues = new LocalPrintServer().GetPrintQueues();
        if (pdfDefault)
          this.CurrentPrintQueue = this.PrintQueues.FirstOrDefault<PrintQueue>((Func<PrintQueue, bool>) (x => x.FullName.ToLower().Contains("pdf"))) ?? this.PrintQueues.FirstOrDefault<PrintQueue>();
        else
          this.CurrentPrintQueue = this.PrintQueues.FirstOrDefault<PrintQueue>((Func<PrintQueue, bool>) (x => x.FullName == LocalPrintServer.GetDefaultPrintQueue().FullName)) ?? this.PrintQueues.FirstOrDefault<PrintQueue>();
      }
      catch (Exception ex)
      {
      }
    }

    private void LoadPageMediaSize()
    {
      this.PageMediaSizes = this.CurrentPrintQueue.GetPrintCapabilities().PageMediaSizeCapability.ToList<PageMediaSize>();
      this.PageSize = this.PageMediaSizes.FirstOrDefault<PageMediaSize>((Func<PageMediaSize, bool>) (x =>
      {
        PageMediaSizeName? pageMediaSizeName1 = x.PageMediaSizeName;
        PageMediaSizeName pageMediaSizeName2 = PageMediaSizeName.ISOA4;
        return pageMediaSizeName1.GetValueOrDefault() == pageMediaSizeName2 & pageMediaSizeName1.HasValue;
      })) ?? this.PageMediaSizes.FirstOrDefault<PageMediaSize>();
    }
  }
}
