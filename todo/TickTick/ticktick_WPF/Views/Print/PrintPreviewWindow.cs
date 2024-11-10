// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.PrintPreviewWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Xps.Packaging;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Calendar.Month;
using ticktick_WPF.Views.Calendar.Week;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class PrintPreviewWindow : MyWindow, IComponentConnector, IStyleConnector
  {
    private FlowDocument _doc;
    private bool _printCal;
    private DocumentPaginator _pt;
    private PrintPreviewViewModel _viewModel;
    private Size _pageSize = new Size(1240.0, 1754.0);
    private WeekPrint _week;
    private int _pagesPerSheet;
    private Duplexing _duplexing = Duplexing.OneSided;
    private int _copyCount = 1;
    private MonthPrint _monthView;
    private DayPrint _day;
    private DocumentPaginator _dp;
    private string _summaryText;
    private TaskDetailPrintView _detailView;
    private TaskDetailPrintViewModel _detailModel;
    internal Grid Container;
    internal Grid Preview;
    internal DocumentViewer DocViewer;
    internal Grid MessageBorder;
    internal TextBlock PrintingText;
    internal TextBlock PrinterStatusText;
    private bool _contentLoaded;

    public PrintPreviewWindow(TaskDetailPrintViewModel model)
    {
      this.InitializeComponent();
      int height = 1112;
      int width = 786;
      this._detailView = new TaskDetailPrintView(model, (double) width, (double) height);
      this.Preview.Children.Add((UIElement) this._detailView);
      this._detailView.UpdateLayout();
      this._detailModel = model;
      this._printCal = false;
      this._doc = this._detailView.Doc;
      this.DocViewer.FitToWidth();
    }

    public PrintPreviewWindow(TaskListPrintViewModel model, bool isDetailList = false)
    {
      this.InitializeComponent();
      this._doc = new TaskListPrintView(model, isDetailList).RichTextBox.Document;
      this.DocViewer.FitToWidth();
    }

    public PrintPreviewWindow(string summaryText)
    {
      int height = 1112;
      int width = 786;
      this.InitializeComponent();
      this._summaryText = summaryText;
      SummaryPrint element = new SummaryPrint(summaryText, (double) width, (double) height);
      this.Preview.Children.Add((UIElement) element);
      this._printCal = false;
      element.UpdateLayout();
      this._doc = element.Doc;
      this.DocViewer.FitToWidth();
    }

    public PrintPreviewWindow(List<List<WeekEventModel>> monthRowData, DateTime startDate)
    {
      this.InitializeComponent();
      this.DocViewer.FitToWidth();
      this._monthView = new MonthPrint(monthRowData, startDate);
      this._doc = this._monthView.Doc;
      this._doc.PageWidth = 1754.0;
      this._doc.PageHeight = 1240.0;
      this._printCal = true;
    }

    public PrintPreviewWindow(
      List<WeekEventModel> allDayModels,
      List<CalendarTimelineDayViewModel> pointModels,
      DateTime startDate,
      int days = 7,
      bool showWeekends = true)
    {
      int pageHeight = 1754;
      int pageWidth = 1240;
      this.InitializeComponent();
      this._week = new WeekPrint(allDayModels, pointModels, (double) pageWidth, (double) pageHeight, startDate, days, showWeekends);
      this._doc = this._week.Doc;
      this._doc.PageWidth = (double) pageWidth;
      this._doc.PageHeight = (double) pageHeight;
      this.Preview.Children.Add((UIElement) this._week);
      this._printCal = true;
      this.DocViewer.FitToWidth();
    }

    public PrintPreviewWindow(
      List<CalendarDisplayViewModel> allDayTasks,
      WeekDayCellViewModel pointModels,
      DateTime date,
      bool isAll)
    {
      int docHeight = 1754;
      int docWidth = 1240;
      this.InitializeComponent();
      this._day = new DayPrint(allDayTasks, pointModels, (double) docWidth, (double) docHeight, date, isAll);
      this._doc = this._day.Doc;
      this._doc.PageWidth = (double) docWidth;
      this._doc.PageHeight = (double) docHeight;
      this.Preview.Children.Add((UIElement) this._day);
      this._printCal = true;
      this.DocViewer.FitToWidth();
    }

    private void GetPaginator()
    {
      this._dp = ((IDocumentPaginatorSource) this._doc).DocumentPaginator;
      this._dp.PageSize = this._pageSize;
      this._pt = (DocumentPaginator) new PaginatorHeaderFooter(this._dp, this._printCal ? 1250.0 : 793.0);
      this._pt.GetPage(100);
      this._viewModel.PageCount = this._pt.PageCount;
      this.Preview.Children.Clear();
    }

    private async void PrintPreviewWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
      PrintPreviewWindow printPreviewWindow1 = this;
      if (!string.IsNullOrEmpty(printPreviewWindow1._summaryText) || printPreviewWindow1._detailView != null)
      {
        await Task.Delay(200);
        while (true)
        {
          TaskDetailPrintView detailView = printPreviewWindow1._detailView;
          if ((detailView != null ? (detailView.IsMdLoaded ? 1 : 0) : 1) == 0)
            await Task.Delay(200);
          else
            break;
        }
      }
      try
      {
        printPreviewWindow1._viewModel = new PrintPreviewViewModel(((IDocumentPaginatorSource) printPreviewWindow1._doc).DocumentPaginator, printPreviewWindow1._monthView != null, !string.IsNullOrEmpty(printPreviewWindow1._summaryText));
        if (printPreviewWindow1.DocViewer.Template.FindName("ToolBar", (FrameworkElement) printPreviewWindow1.DocViewer) is ContentControl name1)
        {
          name1.DataContext = (object) printPreviewWindow1._viewModel;
          if (name1.Template.FindName("Orientation", (FrameworkElement) name1) is ComboBox name)
          {
            name.SelectionChanged -= new SelectionChangedEventHandler(printPreviewWindow1.OnOrientationChanged);
            if (printPreviewWindow1._viewModel.PageOrientation == PageOrientation.Landscape)
              name.SelectedIndex = 1;
            name.SelectionChanged += new SelectionChangedEventHandler(printPreviewWindow1.OnOrientationChanged);
          }
        }
        if (printPreviewWindow1._viewModel.PageSize == null)
        {
          printPreviewWindow1._viewModel.PageSize = new PageMediaSize(PageMediaSizeName.ISOA4, 794.0, 1123.0);
          printPreviewWindow1._viewModel.PageMediaSizes = new List<PageMediaSize>()
          {
            printPreviewWindow1._viewModel.PageSize
          };
        }
        double? nullable = printPreviewWindow1._viewModel.PageSize.Width;
        if (nullable.HasValue)
        {
          nullable = printPreviewWindow1._viewModel.PageSize.Height;
          if (nullable.HasValue)
          {
            PrintPreviewWindow printPreviewWindow2 = printPreviewWindow1;
            nullable = printPreviewWindow1._viewModel.PageSize.Width;
            double width = (double) (int) nullable.Value;
            nullable = printPreviewWindow1._viewModel.PageSize.Height;
            double height = (double) (int) nullable.Value;
            Size size = new Size(width, height);
            printPreviewWindow2._pageSize = size;
          }
        }
        printPreviewWindow1.SetDocumentViewer();
        printPreviewWindow1._viewModel.NeedReload += new EventHandler(printPreviewWindow1.ReloadViewer);
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex?.ToString() + string.Empty);
      }
      printPreviewWindow1.Activate();
    }

    private void SetPrintText()
    {
      if (!(this.DocViewer.Template.FindName("ToolBar", (FrameworkElement) this.DocViewer) is ContentControl name1) || !(name1.Template.FindName("PrintButton", (FrameworkElement) name1) is Button name2))
        return;
      if (this._viewModel.CurrentPrintQueue.QueueStatus == PrintQueueStatus.None)
        name2.Content = this._viewModel.CurrentPrintQueue.Name.ToLower().Contains("fax") ? (object) Utils.GetString("Send") : (this._viewModel.CurrentPrintQueue.Name.ToLower().Contains("pdf") || this._viewModel.CurrentPrintQueue.Name.ToLower().Contains("xps") ? (object) Utils.GetString("PublicSave") : (object) Utils.GetString("Print"));
      else
        name2.Content = (object) Utils.GetString("Print");
    }

    private async void ReloadViewer(object sender, EventArgs e)
    {
      PrintPreviewWindow printPreviewWindow1 = this;
      PrintPreviewViewModel viewModel1 = printPreviewWindow1._viewModel;
      double? nullable;
      int num1;
      if (viewModel1 == null)
      {
        num1 = 1;
      }
      else
      {
        PageMediaSize pageSize = viewModel1.PageSize;
        if (pageSize == null)
        {
          num1 = 1;
        }
        else
        {
          nullable = pageSize.Width;
          num1 = !nullable.HasValue ? 1 : 0;
        }
      }
      if (num1 != 0)
        return;
      PrintPreviewViewModel viewModel2 = printPreviewWindow1._viewModel;
      int num2;
      if (viewModel2 == null)
      {
        num2 = 1;
      }
      else
      {
        PageMediaSize pageSize = viewModel2.PageSize;
        if (pageSize == null)
        {
          num2 = 1;
        }
        else
        {
          nullable = pageSize.Height;
          num2 = !nullable.HasValue ? 1 : 0;
        }
      }
      if (num2 != 0)
        return;
      if (printPreviewWindow1._viewModel.PageOrientation == PageOrientation.Portrait)
      {
        PrintPreviewWindow printPreviewWindow2 = printPreviewWindow1;
        nullable = printPreviewWindow1._viewModel.PageSize.Width;
        double width = (double) (int) nullable.Value;
        nullable = printPreviewWindow1._viewModel.PageSize.Height;
        double height = (double) (int) nullable.Value;
        Size size = new Size(width, height);
        printPreviewWindow2._pageSize = size;
      }
      else if (printPreviewWindow1._viewModel.PageOrientation == PageOrientation.Landscape)
      {
        PrintPreviewWindow printPreviewWindow3 = printPreviewWindow1;
        nullable = printPreviewWindow1._viewModel.PageSize.Height;
        double width = (double) (int) nullable.Value;
        nullable = printPreviewWindow1._viewModel.PageSize.Width;
        double height = (double) (int) nullable.Value;
        Size size = new Size(width, height);
        printPreviewWindow3._pageSize = size;
      }
      if (printPreviewWindow1._monthView != null)
      {
        double num3 = 1250.0 / 793.0;
        printPreviewWindow1._doc.PageWidth = printPreviewWindow1._pageSize.Width * num3;
        printPreviewWindow1._doc.ColumnWidth = printPreviewWindow1._pageSize.Width * num3;
        printPreviewWindow1._doc.PageHeight = printPreviewWindow1._pageSize.Height * num3;
      }
      if (printPreviewWindow1._day != null)
      {
        double num4 = 1250.0 / 793.0;
        printPreviewWindow1.Preview.Children.Clear();
        printPreviewWindow1._day = new DayPrint(printPreviewWindow1._day.AllDayTasks, printPreviewWindow1._day.PointModel, printPreviewWindow1._pageSize.Width * num4, printPreviewWindow1._pageSize.Height * num4, printPreviewWindow1._day.Date, printPreviewWindow1._day.IsAll);
        printPreviewWindow1.Preview.Children.Add((UIElement) printPreviewWindow1._day);
        printPreviewWindow1._doc = printPreviewWindow1._day.Doc;
        await Task.Delay(200);
      }
      if (printPreviewWindow1._week != null)
      {
        double num5 = 1250.0 / 793.0;
        printPreviewWindow1.Preview.Children.Clear();
        printPreviewWindow1._week = new WeekPrint(printPreviewWindow1._week.AllDayModels, printPreviewWindow1._week.PointModels, printPreviewWindow1._pageSize.Width * num5, printPreviewWindow1._pageSize.Height * num5, printPreviewWindow1._week.StartDate, printPreviewWindow1._week.Days, printPreviewWindow1._week.ShowWeekends);
        printPreviewWindow1.Preview.Children.Add((UIElement) printPreviewWindow1._week);
        printPreviewWindow1._doc = printPreviewWindow1._week.Doc;
        await Task.Delay(200);
      }
      if (!string.IsNullOrEmpty(printPreviewWindow1._summaryText))
      {
        printPreviewWindow1.Preview.Children.Clear();
        SummaryPrint summary = new SummaryPrint(printPreviewWindow1._summaryText, printPreviewWindow1._pageSize.Width, printPreviewWindow1._pageSize.Height);
        printPreviewWindow1.Preview.Children.Add((UIElement) summary);
        await Task.Delay(200);
        printPreviewWindow1._doc = summary.Doc;
        await Task.Delay(200);
        summary = (SummaryPrint) null;
      }
      if (printPreviewWindow1._detailView != null)
      {
        double height = printPreviewWindow1.Preview.Height;
        double actualHeight = printPreviewWindow1.Preview.ActualHeight;
        printPreviewWindow1.Preview.Children.Clear();
        printPreviewWindow1._detailView = new TaskDetailPrintView(printPreviewWindow1._detailModel, printPreviewWindow1._pageSize.Width, printPreviewWindow1._pageSize.Height);
        printPreviewWindow1.Preview.Children.Add((UIElement) printPreviewWindow1._detailView);
        while (true)
        {
          TaskDetailPrintView detailView = printPreviewWindow1._detailView;
          if ((detailView != null ? (detailView.IsMdLoaded ? 1 : 0) : 1) == 0)
            await Task.Delay(200);
          else
            break;
        }
        printPreviewWindow1._doc = printPreviewWindow1._detailView.Doc;
        await Task.Delay(200);
      }
      printPreviewWindow1.SetDocumentViewer();
    }

    private async void SetDocumentViewer()
    {
      this.GetPaginator();
      if (this._detailModel != null && this._detailModel.Kind != "CHECKLIST")
        await Task.Delay(300);
      try
      {
        Package package = Package.Open((Stream) new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
        Uri uri = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Tick_Tick\\print.xps");
        PackageStore.RemovePackage(uri);
        PackageStore.AddPackage(uri, package);
        XpsDocument xpsDocument = new XpsDocument(package, CompressionOption.Fast, uri.AbsoluteUri);
        XpsDocument.CreateXpsDocumentWriter(xpsDocument).Write(this._pt);
        this.DocViewer.Document = (IDocumentPaginatorSource) xpsDocument.GetFixedDocumentSequence();
        xpsDocument.Close();
      }
      catch (Exception ex)
      {
      }
    }

    public void PrintCommand()
    {
      try
      {
        PrintDialog printDialog = new PrintDialog();
        printDialog.PrintQueue = this._viewModel.CurrentPrintQueue;
        printDialog.PrintTicket.Duplexing = new Duplexing?(this._duplexing);
        printDialog.PrintTicket.CopyCount = new int?(this._copyCount);
        printDialog.PrintTicket.PagesPerSheet = new int?(this._pagesPerSheet);
        printDialog.PrintTicket.PageOrientation = new PageOrientation?(this._viewModel.PageOrientation);
        printDialog.PrintTicket.PageMediaSize = this._viewModel.PageSize;
        string message = this.CheckPrint(this._viewModel.CurrentPrintQueue);
        if (this._viewModel.CurrentPrintQueue.QueueStatus != PrintQueueStatus.None)
          this.Toast(message);
        PaginatorHeaderFooter paginatorHeaderFooter = new PaginatorHeaderFooter(this._dp, this._printCal ? 1250.0 : 793.0);
        paginatorHeaderFooter.GetPage(100);
        printDialog.PrintDocument((DocumentPaginator) paginatorHeaderFooter, "");
      }
      catch (Exception ex1)
      {
        try
        {
          UtilLog.Info("Print Exception : " + ex1.Message);
          this.Toast("error");
          this.DocViewer.Print();
        }
        catch (Exception ex2)
        {
        }
      }
    }

    private void Toast(string message)
    {
      this.PrintingText.Text = Utils.GetString("AddToPrintList");
      if (!string.IsNullOrEmpty(message))
      {
        this.PrinterStatusText.Text = Utils.GetString("PrinterStatus") + message;
        this.PrinterStatusText.Visibility = Visibility.Visible;
      }
      else
        this.PrinterStatusText.Visibility = Visibility.Collapsed;
      this.MessageBorder.Visibility = Visibility.Visible;
      ((Storyboard) this.FindResource((object) "ShowMessageStory")).Begin();
    }

    private void HideMessageStory(object sender, EventArgs e)
    {
      this.MessageBorder.Visibility = Visibility.Collapsed;
    }

    public void CloseCommand() => this.Close();

    private void CancelRightOption(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void OnOrientationChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this._viewModel == null)
        return;
      this._viewModel.PageOrientation = ((Selector) sender).SelectedIndex == 0 ? PageOrientation.Portrait : PageOrientation.Landscape;
    }

    public string CheckPrint(PrintQueue pq)
    {
      PrintQueueStatus? queueStatus = pq?.QueueStatus;
      string str;
      if (queueStatus.HasValue)
      {
        switch (queueStatus.GetValueOrDefault())
        {
          case PrintQueueStatus.None:
            str = "";
            goto label_6;
          case PrintQueueStatus.Paused:
          case PrintQueueStatus.Error:
          case PrintQueueStatus.PendingDeletion:
          case PrintQueueStatus.PaperJam:
          case PrintQueueStatus.PaperOut:
          case PrintQueueStatus.ManualFeed:
          case PrintQueueStatus.PaperProblem:
          case PrintQueueStatus.Offline:
          case PrintQueueStatus.IOActive:
          case PrintQueueStatus.Busy:
          case PrintQueueStatus.Printing:
          case PrintQueueStatus.OutputBinFull:
          case PrintQueueStatus.NotAvailable:
          case PrintQueueStatus.Processing:
          case PrintQueueStatus.Initializing:
          case PrintQueueStatus.WarmingUp:
          case PrintQueueStatus.TonerLow:
          case PrintQueueStatus.NoToner:
          case PrintQueueStatus.UserIntervention:
          case PrintQueueStatus.OutOfMemory:
          case PrintQueueStatus.DoorOpen:
          case PrintQueueStatus.ServerUnknown:
          case PrintQueueStatus.PowerSave:
            str = Utils.GetString("Printer" + pq.QueueStatus.ToString());
            goto label_6;
          case PrintQueueStatus.Waiting:
            str = "";
            goto label_6;
        }
      }
      str = "";
label_6:
      return str;
    }

    private void OnTextInput(object sender, TextCompositionEventArgs e)
    {
      if (char.IsDigit(e.Text, e.Text.Length - 1))
        return;
      e.Handled = true;
    }

    private void OnTextLostFocus(object sender, RoutedEventArgs e)
    {
      if (!(sender is TextBox textBox))
        return;
      string str = textBox.Text ?? "";
      textBox.Text = str;
    }

    private void OnPagesPerSheetChanged(object sender, SelectionChangedEventArgs e)
    {
      switch (((Selector) sender).SelectedIndex)
      {
        case 1:
          this._pagesPerSheet = 2;
          break;
        case 2:
          this._pagesPerSheet = 4;
          break;
        case 3:
          this._pagesPerSheet = 6;
          break;
        case 4:
          this._pagesPerSheet = 9;
          break;
        case 5:
          this._pagesPerSheet = 16;
          break;
        default:
          this._pagesPerSheet = 1;
          break;
      }
    }

    private void OnDuplexingChanged(object sender, SelectionChangedEventArgs e)
    {
      switch (((Selector) sender).SelectedIndex)
      {
        case 1:
          this._duplexing = Duplexing.TwoSidedLongEdge;
          break;
        default:
          this._duplexing = Duplexing.OneSided;
          break;
      }
    }

    private void OnCopyCountChanged(object sender, TextChangedEventArgs e)
    {
      if (!(sender is TextBox textBox))
        return;
      int result;
      if (int.TryParse(textBox.Text, out result))
      {
        if (result > 0)
        {
          this._copyCount = result;
        }
        else
        {
          this._copyCount = 1;
          textBox.Text = "1";
        }
      }
      else
      {
        this._copyCount = 1;
        textBox.Text = "1";
      }
    }

    private void TryDrag(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      this.DragMove();
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e) => this.Close();

    private void OnQueueChanged(object sender, SelectionChangedEventArgs e) => this.SetPrintText();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/printpreviewwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((Timeline) target).Completed += new EventHandler(this.HideMessageStory);
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 9:
          this.Preview = (Grid) target;
          break;
        case 10:
          this.DocViewer = (DocumentViewer) target;
          this.DocViewer.PreviewMouseRightButtonUp += new MouseButtonEventHandler(this.CancelRightOption);
          break;
        case 12:
          this.MessageBorder = (Grid) target;
          break;
        case 13:
          this.PrintingText = (TextBlock) target;
          break;
        case 14:
          this.PrinterStatusText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 3:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.TryDrag);
          break;
        case 4:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.OnQueueChanged);
          break;
        case 5:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.OnOrientationChanged);
          break;
        case 6:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.OnPagesPerSheetChanged);
          break;
        case 7:
          ((TextBoxBase) target).TextChanged += new TextChangedEventHandler(this.OnCopyCountChanged);
          ((UIElement) target).PreviewTextInput += new TextCompositionEventHandler(this.OnTextInput);
          ((UIElement) target).LostFocus += new RoutedEventHandler(this.OnTextLostFocus);
          break;
        case 8:
          ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.OnDuplexingChanged);
          break;
        case 11:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
          break;
      }
    }
  }
}
