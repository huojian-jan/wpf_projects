// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.TaskDetailPrintView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Converter;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MarkDown;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class TaskDetailPrintView : UserControl, IComponentConnector
  {
    private readonly SolidColorBrush _primaryPrintColor = ThemeUtil.GetColor("PrintTextColorPrimary");
    private readonly SolidColorBrush _secondaryPrintColor = ThemeUtil.GetColor("PrintTextColorSecondary");
    private TaskDetailPrintViewModel _model;
    private double _height;
    private double _width;
    private MarkDownEditor _markDown;
    private BlockUIContainer _markDownLastBlockUiContainer;
    private ItemIconColorConverter _iconColorConvert = new ItemIconColorConverter();
    internal FlowDocumentPageViewer PageViewer;
    internal FlowDocument Doc;
    internal BlockUIContainer Head;
    internal StackPanel CheckIcon;
    internal Border AvatarGrid;
    internal Border AvatarImage;
    internal Image AssignImage;
    internal Grid PriorityGrid;
    internal ColumnDefinition FColumn;
    internal ColumnDefinition SColumn;
    internal Paragraph TitleText;
    internal Paragraph DescText;
    internal TableRowGroup ItemRowGroup;
    internal BlockUIContainer TagBlock;
    internal WrapPanel TagContainer;
    internal TableRowGroup SubTaskGroup;
    internal Table CommentTable;
    internal TableRowGroup CommentRowGroup;
    internal ScrollViewer MarkDownViewer;
    private bool _contentLoaded;

    public bool IsMdLoaded { get; private set; }

    public TaskDetailPrintView() => this.InitializeComponent();

    public TaskDetailPrintView(TaskDetailPrintViewModel model, double width, double height)
    {
      this._model = model;
      this.DataContext = (object) model;
      this._height = height;
      this._width = width;
      this.InitializeComponent();
      this.Doc.FontFamily = LocalSettings.Settings.ExtraSettings.AppFontFamily == "SourceHansansSC_CN" ? new FontFamily("Microsoft YaHei UI") : LocalSettings.Settings.FontFamily;
      this._markDown = new MarkDownEditor(true);
      this._markDown.ImageMode = model.ImageMode;
      if (!string.IsNullOrEmpty(this._model.Title))
      {
        this._model.ContentOrDesc = "## " + this._model.Title + "\n" + this._model.ContentOrDesc;
        this._model.Title = string.Empty;
      }
      this._markDown.SetImageGeneratorTaskId(model.TaskId);
      this._markDown.Width = this._width - 70.0;
      this._markDown.Text = this._model.ContentOrDesc;
      this.MarkDownViewer.Content = (object) this._markDown;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.SizeChanged += (SizeChangedEventHandler) ((sender, args) =>
      {
        if (!this.IsMdLoaded)
          return;
        this._markDown.Width = args.NewSize.Width - 70.0;
        this._markDown.Text = this._model.ContentOrDesc;
        Size newSize = args.NewSize;
        this._width = newSize.Height;
        newSize = args.NewSize;
        this._height = newSize.Height;
        this.ReloadContent();
      });
    }

    private void OnInitialized(object sender, EventArgs eventArgs)
    {
      this.FColumn.Width = new GridLength((double) this._model.Progress.GetValueOrDefault(), GridUnitType.Star);
      ColumnDefinition scolumn = this.SColumn;
      int? progress = this._model.Progress;
      GridLength gridLength = new GridLength((double) (progress.HasValue ? new int?(100 - progress.GetValueOrDefault()) : new int?()).GetValueOrDefault(), GridUnitType.Star);
      scolumn.Width = gridLength;
      if (this._model.Avatar != null)
      {
        this.AvatarGrid.Visibility = Visibility.Visible;
        this.AvatarImage.Background = (Brush) new ImageBrush()
        {
          ImageSource = (ImageSource) this._model.Avatar
        };
      }
      else
        this.AssignImage.Visibility = CacheManager.GetProjectById(this._model.ProjectId).IsShareList() ? Visibility.Visible : Visibility.Collapsed;
      if (!(this._model.Kind == "NOTE"))
        return;
      this.PriorityGrid.Visibility = Visibility.Collapsed;
      this.CheckIcon.Visibility = Visibility.Collapsed;
    }

    private void TryAddContent()
    {
      this.Doc.Blocks.Remove((Block) this.TitleText);
      this.GetMarkDown();
      this.Doc.Blocks.Remove((Block) this.DescText);
    }

    private void TryRemoveContent()
    {
      this.IsMdLoaded = false;
      this._markDownLastBlockUiContainer = (BlockUIContainer) null;
      List<Block> blockList = new List<Block>();
      foreach (Block block in (TextElementCollection<Block>) this.Doc.Blocks)
      {
        if ("md".Equals(block.Tag))
          blockList.Add(block);
      }
      blockList.ForEach((Action<Block>) (i => this.Doc.Blocks.Remove(i)));
    }

    private void TryAddItems()
    {
      if (!(this._model.Kind == "CHECKLIST"))
        return;
      List<SubtaskPrintViewModel> subtaskPrintViewModels = this._model.SubtaskPrintViewModels;
      // ISSUE: explicit non-virtual call
      if ((subtaskPrintViewModels != null ? (__nonvirtual (subtaskPrintViewModels.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (SubtaskPrintViewModel subtaskPrintViewModel in this._model.SubtaskPrintViewModels)
      {
        TableRow tableRow = new TableRow();
        Path checkboxPath = this.GetCheckboxPath(subtaskPrintViewModel);
        checkboxPath.Margin = new Thickness(0.0, 2.0, 0.0, 0.0);
        TableCell tableCell1 = new TableCell((Block) new BlockUIContainer((UIElement) checkboxPath));
        Paragraph blockItem = new Paragraph((Inline) new Run(subtaskPrintViewModel.Title));
        blockItem.Foreground = subtaskPrintViewModel.Status != 0 ? (Brush) this._secondaryPrintColor : (Brush) this._primaryPrintColor;
        blockItem.FontSize = 13.0;
        blockItem.Padding = new Thickness(0.0, 0.0, 0.0, 2.0);
        blockItem.TextDecorations = subtaskPrintViewModel.Status != 0 ? TextDecorations.Strikethrough : (TextDecorationCollection) null;
        TableCell tableCell2 = new TableCell((Block) blockItem);
        if (!subtaskPrintViewModel.StartDate.HasValue)
        {
          tableCell2.ColumnSpan = 2;
          tableRow.Cells.Add(tableCell1);
          tableRow.Cells.Add(tableCell2);
        }
        else
        {
          TextBlock textBlock = new TextBlock();
          textBlock.Text = PrintHelper.GetSubTaskTimeText(subtaskPrintViewModel);
          textBlock.HorizontalAlignment = HorizontalAlignment.Right;
          textBlock.VerticalAlignment = VerticalAlignment.Bottom;
          textBlock.FontSize = 13.0;
          textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 2.0);
          textBlock.Foreground = this.GetTimeTextColor(subtaskPrintViewModel.StartDate);
          TableCell tableCell3 = new TableCell((Block) new BlockUIContainer((UIElement) textBlock));
          tableRow.Cells.Add(tableCell1);
          tableRow.Cells.Add(tableCell2);
          tableRow.Cells.Add(tableCell3);
        }
        this.ItemRowGroup.Rows.Add(tableRow);
        this.ItemRowGroup.Rows.Add(this.GetLineRow());
      }
    }

    private void GetMarkDown()
    {
      double height = this._height - 150.0;
      if (this._markDownLastBlockUiContainer == null)
        height = this._height - 190.0;
      if (this._markDown.ActualHeight > height)
      {
        int pageLastIndex = this._markDown.GetPageLastIndex(height);
        string str = this._markDown.Text.Substring(0, pageLastIndex);
        MarkDownEditor markDownEditor = new MarkDownEditor(true);
        markDownEditor.Width = this._width - 70.0;
        markDownEditor.HorizontalAlignment = HorizontalAlignment.Left;
        markDownEditor.EnableSpellCheck = false;
        markDownEditor.ImageMode = this._markDown.ImageMode;
        markDownEditor.Height = height;
        BlockUIContainer newItem = new BlockUIContainer((UIElement) markDownEditor);
        markDownEditor.SetImageGeneratorTaskId(this._model.TaskId);
        markDownEditor.Text = str;
        newItem.Tag = (object) "md";
        if (this._markDownLastBlockUiContainer != null)
        {
          this.Doc.Blocks.InsertAfter((Block) this._markDownLastBlockUiContainer, (Block) newItem);
          this._markDownLastBlockUiContainer = newItem;
        }
        else
        {
          this.Doc.Blocks.InsertBefore((Block) this.DescText, (Block) newItem);
          this._markDownLastBlockUiContainer = newItem;
        }
        markDownEditor.UpdateLayout();
        this._markDown.Text = this._markDown.Text.Substring(pageLastIndex);
        this._markDown.UpdateLayout();
        if (!string.IsNullOrEmpty(str))
          this.GetMarkDown();
        else
          this.IsMdLoaded = true;
      }
      else
      {
        MarkDownEditor markDownEditor = new MarkDownEditor(true);
        markDownEditor.Width = this._width - 70.0;
        markDownEditor.MinHeight = this._markDown.ActualHeight + 10.0;
        markDownEditor.Height = this._markDown.ActualHeight + 10.0;
        markDownEditor.HorizontalAlignment = HorizontalAlignment.Left;
        markDownEditor.EnableSpellCheck = false;
        markDownEditor.ImageMode = this._markDown.ImageMode;
        markDownEditor.SetImageGeneratorTaskId(this._model.TaskId);
        markDownEditor.Text = this._markDown.Text;
        BlockUIContainer newItem = new BlockUIContainer((UIElement) markDownEditor);
        newItem.Tag = (object) "md";
        if (this._markDownLastBlockUiContainer != null)
          this.Doc.Blocks.InsertAfter((Block) this._markDownLastBlockUiContainer, (Block) newItem);
        else
          this.Doc.Blocks.InsertBefore((Block) this.DescText, (Block) newItem);
        markDownEditor.UpdateLayout();
        markDownEditor.Redraw();
        markDownEditor.UpdateLayout();
        this.IsMdLoaded = true;
      }
    }

    private TableRow GetLineRow(Thickness margin = default (Thickness))
    {
      TableRow lineRow = new TableRow();
      Border border = new Border();
      border.Height = 6.0;
      border.BorderBrush = (Brush) ThemeUtil.GetAlphaColor("#191919", 10);
      border.BorderThickness = new Thickness(0.0, 0.0, 0.0, 1.0);
      border.Margin = margin;
      TableCell tableCell = new TableCell((Block) new BlockUIContainer((UIElement) border))
      {
        ColumnSpan = 2
      };
      lineRow.Cells.Add(new TableCell());
      lineRow.Cells.Add(tableCell);
      return lineRow;
    }

    private void TryAddSubTask()
    {
      List<SubtaskPrintViewModel> subtaskPrintViewModels = this._model.RealSubtaskPrintViewModels;
      // ISSUE: explicit non-virtual call
      if ((subtaskPrintViewModels != null ? (__nonvirtual (subtaskPrintViewModels.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      Border border = new Border();
      border.Height = 2.0;
      border.BorderBrush = (Brush) ThemeUtil.GetAlphaColor("#05080F", 8);
      border.BorderThickness = new Thickness(0.0, 1.0, 0.0, 0.0);
      BlockUIContainer newItem = new BlockUIContainer((UIElement) border);
      newItem.Margin = new Thickness(0.0, 15.0, 0.0, 15.0);
      this.Doc.Blocks.InsertAfter((Block) this.TagBlock, (Block) newItem);
      foreach (SubtaskPrintViewModel subtaskPrintViewModel in this._model.RealSubtaskPrintViewModels)
      {
        TableRow tableRow = new TableRow();
        StackPanel stackPanel = new StackPanel();
        stackPanel.Orientation = Orientation.Horizontal;
        stackPanel.Children.Add((UIElement) this.GetCheckboxPath(subtaskPrintViewModel));
        UIElementCollection children = stackPanel.Children;
        TextBlock element = new TextBlock();
        element.Text = subtaskPrintViewModel.Title;
        element.Foreground = subtaskPrintViewModel.Status != 0 ? (Brush) this._secondaryPrintColor : (Brush) this._primaryPrintColor;
        element.FontSize = 13.0;
        element.VerticalAlignment = VerticalAlignment.Bottom;
        element.Padding = new Thickness(12.0, 0.0, 0.0, 0.0);
        element.TextDecorations = subtaskPrintViewModel.Status != 0 ? TextDecorations.Strikethrough : (TextDecorationCollection) null;
        children.Add((UIElement) element);
        TableCell tableCell1 = new TableCell((Block) new BlockUIContainer((UIElement) stackPanel))
        {
          ColumnSpan = 2
        };
        tableRow.Cells.Add(tableCell1);
        if (subtaskPrintViewModel.StartDate.HasValue)
        {
          TextBlock textBlock = new TextBlock();
          textBlock.Text = PrintHelper.GetSubTaskTimeText(subtaskPrintViewModel);
          textBlock.HorizontalAlignment = HorizontalAlignment.Right;
          textBlock.VerticalAlignment = VerticalAlignment.Bottom;
          textBlock.FontSize = 13.0;
          textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
          textBlock.Foreground = this.GetTimeTextColor(subtaskPrintViewModel.StartDate);
          TableCell tableCell2 = new TableCell((Block) new BlockUIContainer((UIElement) textBlock));
          tableRow.Cells.Add(tableCell2);
        }
        this.SubTaskGroup.Rows.Add(tableRow);
        this.SubTaskGroup.Rows.Add(this.GetLineRow(new Thickness((double) ((subtaskPrintViewModel.Level - 1) * 15), 0.0, 0.0, 0.0)));
      }
    }

    private Path GetCheckboxPath(SubtaskPrintViewModel model)
    {
      object[] values = new object[5]
      {
        (object) DisplayType.Task,
        (object) model.Priority,
        (object) model.Status,
        (object) false,
        (object) this
      };
      Geometry taskIconGeometry = ThemeUtil.GetTaskIconGeometry(DisplayType.Task, model.Status, model.Kind, 0);
      Path checkboxPath = new Path();
      checkboxPath.Width = 14.0;
      checkboxPath.Height = 14.0;
      checkboxPath.Stretch = Stretch.UniformToFill;
      checkboxPath.Data = taskIconGeometry;
      checkboxPath.HorizontalAlignment = HorizontalAlignment.Left;
      checkboxPath.VerticalAlignment = VerticalAlignment.Bottom;
      checkboxPath.Fill = (Brush) this._iconColorConvert.Convert(values, typeof (SolidColorBrush), (object) null, (CultureInfo) null);
      checkboxPath.Margin = new Thickness((double) ((model.Level - 1) * 15), 0.0, 0.0, 1.0);
      return checkboxPath;
    }

    private Brush GetTimeTextColor(DateTime? startDate)
    {
      return startDate.HasValue && startDate.Value.Date < DateTime.Today ? (Brush) ThemeUtil.GetColor("OutDateColor") : (Brush) this._secondaryPrintColor;
    }

    private void TryAddComments()
    {
      List<CommentViewModel> comments = this._model.Comments;
      // ISSUE: explicit non-virtual call
      if ((comments != null ? (__nonvirtual (comments.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      BlockUIContainer newItem = new BlockUIContainer();
      StackPanel stackPanel1 = new StackPanel();
      stackPanel1.Orientation = Orientation.Horizontal;
      stackPanel1.Margin = new Thickness(1.0, 30.0, 0.0, 0.0);
      StackPanel stackPanel2 = stackPanel1;
      UIElementCollection children1 = stackPanel2.Children;
      TextBlock element1 = new TextBlock();
      element1.Text = Utils.GetString("Comment");
      element1.FontSize = 14.0;
      element1.Foreground = (Brush) this._primaryPrintColor;
      element1.Margin = new Thickness(0.0, 0.0, 10.0, 0.0);
      children1.Add((UIElement) element1);
      UIElementCollection children2 = stackPanel2.Children;
      TextBlock element2 = new TextBlock();
      element2.Text = this._model.Comments.Count.ToString();
      element2.FontSize = 13.0;
      element2.VerticalAlignment = VerticalAlignment.Center;
      element2.Foreground = (Brush) this._secondaryPrintColor;
      children2.Add((UIElement) element2);
      newItem.Child = (UIElement) stackPanel2;
      this.Doc.Blocks.InsertBefore((Block) this.CommentTable, (Block) newItem);
      foreach (CommentViewModel comment in this._model.Comments)
        this.AddComment(comment);
    }

    private void AddComment(CommentViewModel comment)
    {
      TableRow tableRow1 = new TableRow();
      Ellipse ellipse = new Ellipse();
      ellipse.Height = 30.0;
      ellipse.Width = 30.0;
      ellipse.HorizontalAlignment = HorizontalAlignment.Left;
      ellipse.StrokeThickness = 1.0;
      ellipse.Stroke = (Brush) ThemeUtil.GetAlphaColor("#191919", 8);
      ellipse.Fill = (Brush) new ImageBrush((ImageSource) AvatarHelper.GetAvatarByUrl(comment.AvatarUrl));
      TableCell tableCell1 = new TableCell((Block) new BlockUIContainer((UIElement) ellipse))
      {
        RowSpan = 2
      };
      StackPanel stackPanel = new StackPanel();
      stackPanel.Orientation = Orientation.Horizontal;
      UIElementCollection children = stackPanel.Children;
      TextBlock element = new TextBlock();
      element.Text = comment.UserName;
      element.FontSize = 14.0;
      element.FontWeight = FontWeights.SemiBold;
      element.Foreground = (Brush) this._primaryPrintColor;
      element.Margin = new Thickness(0.0, 0.0, 10.0, 0.0);
      children.Add((UIElement) element);
      stackPanel.Children.Add((UIElement) new TextBlock()
      {
        Text = ticktick_WPF.Util.DateUtils.FormatCommentTime(comment.CreateDate),
        FontSize = 13.0,
        Foreground = (Brush) this._secondaryPrintColor
      });
      TableCell tableCell2 = new TableCell((Block) new BlockUIContainer((UIElement) stackPanel));
      tableRow1.Cells.Add(tableCell1);
      tableRow1.Cells.Add(tableCell2);
      this.CommentRowGroup.Rows.Add(tableRow1);
      TableRow tableRow2 = new TableRow();
      Run run = new Run(comment.Content);
      run.FontSize = 14.0;
      run.Foreground = (Brush) this._primaryPrintColor;
      Paragraph blockItem = new Paragraph((Inline) run);
      blockItem.Padding = new Thickness(0.0, 2.0, 0.0, 15.0);
      TableCell tableCell3 = new TableCell((Block) blockItem);
      tableRow2.Cells.Add(tableCell3);
      this.CommentRowGroup.Rows.Add(tableRow2);
    }

    private void TryAddAttachments()
    {
      try
      {
        List<AttachmentViewModel> attachments = this._model.Attachments;
        // ISSUE: explicit non-virtual call
        if ((attachments != null ? (__nonvirtual (attachments.Count) > 0 ? 1 : 0) : 0) == 0 || !(this._model.Kind == "CHECKLIST"))
          return;
        BlockUIContainer previousSibling = this.TagBlock;
        bool flag = true;
        foreach (AttachmentViewModel attachment in this._model.Attachments)
        {
          int top = flag ? 30 : 6;
          flag = false;
          if (attachment.FileType == "IMAGE" && !string.IsNullOrEmpty(attachment.LocalPath))
          {
            BlockUIContainer newItem = new BlockUIContainer();
            double maxWidth = this._width - 80.0;
            int num = this._model.ImageMode == 1 ? 120 : 800;
            Size imageSize = ThemeUtil.GetImageSize(attachment.LocalPath);
            double maxHeight = (double) num;
            double width = imageSize.Width;
            double height = imageSize.Height;
            Size smallRect = ImageUtils.GetSmallRect(maxWidth, maxHeight, width, height);
            Grid grid1 = new Grid();
            grid1.Margin = new Thickness(10.0, (double) top, 0.0, 6.0);
            grid1.Height = smallRect.Height;
            Grid grid2 = grid1;
            Rectangle rectangle = new Rectangle();
            rectangle.Height = smallRect.Height;
            rectangle.Width = smallRect.Width;
            rectangle.RadiusX = 4.0;
            rectangle.RadiusY = 4.0;
            rectangle.HorizontalAlignment = HorizontalAlignment.Left;
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = (ImageSource) ThemeUtil.GetImage(attachment.LocalPath, (int) (smallRect.Width * 1.5));
            imageBrush.Stretch = Stretch.Uniform;
            rectangle.Fill = (Brush) imageBrush;
            Rectangle element1 = rectangle;
            Border element2 = new Border();
            grid2.Children.Add((UIElement) element1);
            grid2.Children.Add((UIElement) element2);
            newItem.Child = (UIElement) grid2;
            this.Doc.Blocks.InsertAfter((Block) previousSibling, (Block) newItem);
            previousSibling = newItem;
          }
          else
          {
            BlockUIContainer newItem = new BlockUIContainer();
            Border border1 = new Border();
            border1.Height = 50.0;
            border1.Margin = new Thickness(10.0, (double) top, 30.0, 6.0);
            border1.Background = TaskDetailPrintView.GetFileTypeColor(attachment.FileType);
            border1.CornerRadius = new CornerRadius(4.0);
            border1.BorderThickness = new Thickness(1.0);
            border1.BorderBrush = this.GetFileTypeBorderColor(attachment.FileType);
            Border border2 = border1;
            Grid grid = new Grid()
            {
              ColumnDefinitions = {
                new ColumnDefinition() { Width = GridLength.Auto },
                new ColumnDefinition(),
                new ColumnDefinition() { Width = GridLength.Auto }
              }
            };
            Image image = new Image();
            image.Height = 30.0;
            image.Margin = new Thickness(12.0, 0.0, 0.0, 0.0);
            image.Source = (ImageSource) new BitmapImage(new Uri(this.GetFileTypeImage(attachment.FileType)));
            Image element3 = image;
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Text = attachment.FileName;
            textBlock1.FontSize = 14.0;
            textBlock1.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock1.TextTrimming = TextTrimming.CharacterEllipsis;
            textBlock1.Foreground = (Brush) this._primaryPrintColor;
            textBlock1.LineHeight = 14.0;
            textBlock1.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            textBlock1.VerticalAlignment = VerticalAlignment.Center;
            textBlock1.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
            TextBlock element4 = textBlock1;
            element4.SetValue(Grid.ColumnProperty, (object) 1);
            TextBlock textBlock2 = new TextBlock();
            textBlock2.Margin = new Thickness(12.0, 0.0, 12.0, 0.0);
            int num;
            string str;
            if (attachment.Size <= 1024000)
            {
              if (attachment.Size <= 1024)
              {
                num = attachment.Size;
                str = num.ToString() + "B";
              }
              else
              {
                num = attachment.Size / 1024;
                str = num.ToString("f2") + "KB";
              }
            }
            else
            {
              num = attachment.Size / 1024000;
              str = num.ToString("f2") + "MB";
            }
            textBlock2.Text = str;
            textBlock2.Foreground = (Brush) ThemeUtil.GetAlphaColor("#191919", 85);
            textBlock2.FontSize = 12.0;
            textBlock2.LineHeight = 12.0;
            textBlock2.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            textBlock2.VerticalAlignment = VerticalAlignment.Center;
            TextBlock element5 = textBlock2;
            element5.SetValue(Grid.ColumnProperty, (object) 2);
            grid.Children.Add((UIElement) element3);
            grid.Children.Add((UIElement) element4);
            grid.Children.Add((UIElement) element5);
            border2.Child = (UIElement) grid;
            newItem.Child = (UIElement) border2;
            this.Doc.Blocks.InsertAfter((Block) previousSibling, (Block) newItem);
            previousSibling = newItem;
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private string GetFileTypeImage(string fileType)
    {
      if (fileType != null)
      {
        switch (fileType.Length)
        {
          case 3:
            switch (fileType[1])
            {
              case 'D':
                if (fileType == "PDF")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_pdf.png";
                break;
              case 'I':
                if (fileType == "ZIP")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_zip.png";
                break;
              case 'L':
                if (fileType == "XLS")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_xls.png";
                break;
              case 'O':
                if (fileType == "DOC")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_doc.png";
                break;
              case 'P':
                if (fileType == "PPT")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_ppt.png";
                break;
              case 'S':
                if (fileType == "CSV")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_csv.png";
                break;
            }
            break;
          case 4:
            if (fileType == "TEXT")
              return "pack://application:,,,/Assets/Attachment_Icon/Attachment_txt.png";
            break;
          case 5:
            switch (fileType[0])
            {
              case 'A':
                if (fileType == "AUDIO")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_audio.png";
                break;
              case 'O':
                if (fileType == "OTHER")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_unknow.png";
                break;
              case 'V':
                if (fileType == "VIDEO")
                  return "pack://application:,,,/Assets/Attachment_Icon/Attachment_video.png";
                break;
            }
            break;
        }
      }
      return "pack://application:,,,/Assets/Attachment_Icon/Attachment_unknow.png";
    }

    private Brush GetFileTypeBorderColor(string fileType)
    {
      if (fileType != null)
      {
        switch (fileType.Length)
        {
          case 3:
            switch (fileType[1])
            {
              case 'D':
                if (fileType == "PDF")
                  return (Brush) ThemeUtil.GetColor("PdfBorderColor");
                break;
              case 'I':
                if (fileType == "ZIP")
                  return (Brush) ThemeUtil.GetColor("ZipBorderColor");
                break;
              case 'L':
                if (fileType == "XLS")
                  return (Brush) ThemeUtil.GetColor("XlsBorderColor");
                break;
              case 'O':
                if (fileType == "DOC")
                  return (Brush) ThemeUtil.GetColor("DocBorderColor");
                break;
              case 'P':
                if (fileType == "PPT")
                  return (Brush) ThemeUtil.GetColor("PptBorderColor");
                break;
              case 'S':
                if (fileType == "CSV")
                  return (Brush) ThemeUtil.GetColor("CsvBorderColor");
                break;
            }
            break;
          case 4:
            if (fileType == "TEXT")
              return (Brush) ThemeUtil.GetColor("TxtBorderColor");
            break;
          case 5:
            switch (fileType[0])
            {
              case 'A':
                if (fileType == "AUDIO")
                  return (Brush) ThemeUtil.GetColor("AudioBorderColor");
                break;
              case 'O':
                if (fileType == "OTHER")
                  return (Brush) ThemeUtil.GetColor("OtherBorderColor");
                break;
              case 'V':
                if (fileType == "VIDEO")
                  return (Brush) ThemeUtil.GetColor("VideoBorderColor");
                break;
            }
            break;
        }
      }
      return (Brush) ThemeUtil.GetColor("OtherBorderColor");
    }

    private static Brush GetFileTypeColor(string fileType)
    {
      if (fileType != null)
      {
        switch (fileType.Length)
        {
          case 3:
            switch (fileType[1])
            {
              case 'D':
                if (fileType == "PDF")
                  return (Brush) ThemeUtil.GetColor("PdfBackgroundColor");
                break;
              case 'I':
                if (fileType == "ZIP")
                  return (Brush) ThemeUtil.GetColor("ZipBackgroundColor");
                break;
              case 'L':
                if (fileType == "XLS")
                  return (Brush) ThemeUtil.GetColor("XlsBackgroundColor");
                break;
              case 'O':
                if (fileType == "DOC")
                  return (Brush) ThemeUtil.GetColor("DocBackgroundColor");
                break;
              case 'P':
                if (fileType == "PPT")
                  return (Brush) ThemeUtil.GetColor("PptBackgroundColor");
                break;
              case 'S':
                if (fileType == "CSV")
                  return (Brush) ThemeUtil.GetColor("CsvBackgroundColor");
                break;
            }
            break;
          case 4:
            if (fileType == "TEXT")
              return (Brush) ThemeUtil.GetColor("TxtBackgroundColor");
            break;
          case 5:
            switch (fileType[0])
            {
              case 'A':
                if (fileType == "AUDIO")
                  return (Brush) ThemeUtil.GetColor("AudioBackgroundColor");
                break;
              case 'O':
                if (fileType == "OTHER")
                  return (Brush) ThemeUtil.GetColor("OtherBackgroundColor");
                break;
              case 'V':
                if (fileType == "VIDEO")
                  return (Brush) ThemeUtil.GetColor("VideoBackgroundColor");
                break;
            }
            break;
        }
      }
      return (Brush) ThemeUtil.GetColor("OtherBackgroundColor");
    }

    private void TryAddTags()
    {
      List<TagPrintModel> tags = this._model.Tags;
      // ISSUE: explicit non-virtual call
      if ((tags != null ? (__nonvirtual (tags.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (TagPrintModel tag in this._model.Tags)
      {
        Border border = new Border();
        border.Background = (Brush) tag.TagBackColor;
        border.CornerRadius = new CornerRadius(2.0);
        border.Margin = new Thickness(0.0, 0.0, 5.0, 4.0);
        Border element = border;
        TextBlock textBlock = new TextBlock()
        {
          Text = tag.TagTitle,
          Foreground = (Brush) tag.TagTextColor,
          FontSize = 12.0,
          Padding = new Thickness(4.0, 2.0, 4.0, 2.0)
        };
        element.Child = (UIElement) textBlock;
        this.TagContainer.Children.Add((UIElement) element);
      }
    }

    private void ReloadContent()
    {
      this.TryRemoveContent();
      this.TryAddContent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.UpdateLayout();
      this._markDown.SetTheme();
      this._markDown.UpdateLayout();
      this._markDown.Redraw();
      this._markDown.UpdateLayout();
      this.TryAddContent();
      this.TryAddItems();
      this.TryAddTags();
      this.TryAddSubTask();
      this.TryAddAttachments();
      this.TryAddComments();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/taskdetailprintview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Initialized += new EventHandler(this.OnInitialized);
          break;
        case 2:
          this.PageViewer = (FlowDocumentPageViewer) target;
          break;
        case 3:
          this.Doc = (FlowDocument) target;
          break;
        case 4:
          this.Head = (BlockUIContainer) target;
          break;
        case 5:
          this.CheckIcon = (StackPanel) target;
          break;
        case 6:
          this.AvatarGrid = (Border) target;
          break;
        case 7:
          this.AvatarImage = (Border) target;
          break;
        case 8:
          this.AssignImage = (Image) target;
          break;
        case 9:
          this.PriorityGrid = (Grid) target;
          break;
        case 10:
          this.FColumn = (ColumnDefinition) target;
          break;
        case 11:
          this.SColumn = (ColumnDefinition) target;
          break;
        case 12:
          this.TitleText = (Paragraph) target;
          break;
        case 13:
          this.DescText = (Paragraph) target;
          break;
        case 14:
          this.ItemRowGroup = (TableRowGroup) target;
          break;
        case 15:
          this.TagBlock = (BlockUIContainer) target;
          break;
        case 16:
          this.TagContainer = (WrapPanel) target;
          break;
        case 17:
          this.SubTaskGroup = (TableRowGroup) target;
          break;
        case 18:
          this.CommentTable = (Table) target;
          break;
        case 19:
          this.CommentRowGroup = (TableRowGroup) target;
          break;
        case 20:
          this.MarkDownViewer = (ScrollViewer) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
