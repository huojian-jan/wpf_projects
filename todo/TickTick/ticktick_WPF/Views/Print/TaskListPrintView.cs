// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.TaskListPrintView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Converter;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class TaskListPrintView : UserControl, IComponentConnector
  {
    private readonly TaskListPrintViewModel _printModel;
    private bool _isDetail;
    private ResourceDictionary _resourceDictionary;
    private SolidColorBrush _primaryPrintColor = ThemeUtil.GetColor("PrintTextColorPrimary");
    private SolidColorBrush _secondaryPrintColor = ThemeUtil.GetColor("PrintTextColorSecondary");
    private ItemIconConverter _iconConvert = new ItemIconConverter();
    private ItemIconColorConverter _iconColorConvert = new ItemIconColorConverter();
    internal RichTextBox RichTextBox;
    internal FlowDocument Doc;
    internal Run ListName;
    private bool _contentLoaded;

    public TaskListPrintView(TaskListPrintViewModel model, bool isDetail)
    {
      this._printModel = model;
      this._isDetail = isDetail;
      this.InitializeComponent();
      Application current = Application.Current;
      List<ResourceDictionary> list = current != null ? current.Resources.MergedDictionaries.ToList<ResourceDictionary>() : (List<ResourceDictionary>) null;
      string requestedCulture = "Resource\\icons_light.xaml";
      this._resourceDictionary = list.FirstOrDefault<ResourceDictionary>((Func<ResourceDictionary, bool>) (d => d.Source.OriginalString.Equals(requestedCulture)));
      this.RenderDocument();
      this.RichTextBox.FontFamily = LocalSettings.Settings.ExtraSettings.AppFontFamily == "SourceHansansSC_CN" ? new FontFamily("Microsoft YaHei UI") : LocalSettings.Settings.FontFamily;
    }

    private void RenderDocument()
    {
      if (this._printModel == null)
        return;
      this.ListName.Text = this._printModel.ListName;
      if (this._printModel.TaskPrintViewModels == null)
        return;
      foreach (TaskPrintViewModel taskPrintViewModel in this._printModel.TaskPrintViewModels)
        this.GetTable(taskPrintViewModel);
    }

    private void GetTable(TaskPrintViewModel model)
    {
      Table table = new Table();
      TableColumn tableColumn1 = new TableColumn()
      {
        Width = new GridLength((double) (25 + model.Level * 16))
      };
      TableColumn tableColumn2 = new TableColumn()
      {
        Width = GridLength.Auto
      };
      TableColumn tableColumn3 = new TableColumn();
      table.Columns.Add(tableColumn1);
      table.Columns.Add(tableColumn2);
      table.Columns.Add(tableColumn3);
      TableRowGroup rowGroup = new TableRowGroup();
      TableRow tableRow = new TableRow();
      TableCell tableCell1;
      TableCell tableCell2;
      TableCell tableCell3;
      switch (model.Type)
      {
        case DisplayType.LoadMore:
          Run run = new Run(model.Title);
          run.Foreground = (Brush) this._secondaryPrintColor;
          tableCell1 = new TableCell((Block) new Paragraph((Inline) run));
          tableColumn1.Width = new GridLength(150.0);
          tableCell2 = new TableCell();
          tableCell3 = new TableCell();
          break;
        case DisplayType.Section:
          Path path = new Path();
          path.Data = ((Path) this.FindResource((object) "ic_svg_path_arrow")).Data;
          path.HorizontalAlignment = HorizontalAlignment.Left;
          path.VerticalAlignment = VerticalAlignment.Top;
          path.Width = 9.0;
          path.Height = 6.0;
          path.Stretch = Stretch.Fill;
          path.Margin = new Thickness(0.0, 5.0, 0.0, 0.0);
          SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Black);
          solidColorBrush.Opacity = 0.36;
          path.Fill = (Brush) solidColorBrush;
          path.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
          path.RenderTransform = (Transform) new RotateTransform(model.IsOpen ? 0.0 : 270.0);
          tableCell1 = new TableCell((Block) new BlockUIContainer((UIElement) path));
          Paragraph blockItem1 = new Paragraph((Inline) new Run(model.Title));
          blockItem1.Foreground = (Brush) this._primaryPrintColor;
          tableCell2 = new TableCell((Block) blockItem1);
          tableColumn1.Width = new GridLength(20.0);
          tableColumn3.Width = new GridLength(60.0);
          TableCell tableCell4;
          if (!model.IsOpen)
          {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = model.Count.ToString();
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            tableCell4 = new TableCell((Block) new BlockUIContainer((UIElement) textBlock));
          }
          else
            tableCell4 = new TableCell();
          tableCell3 = tableCell4;
          table.Padding = new Thickness(0.0, 3.0, 0.0, 4.0);
          break;
        default:
          Path checkboxPath = this.GetCheckboxPath(model);
          checkboxPath.Margin = new Thickness((double) (model.Level * 16), 2.0, 0.0, 0.0);
          tableCell1 = new TableCell((Block) new BlockUIContainer((UIElement) checkboxPath));
          TitleRightColumnView titleRightColumnView = new TitleRightColumnView(new RightColumnViewModel(model, this._printModel.IsNormal));
          Paragraph blockItem2 = new Paragraph((Inline) new Run(model.Title));
          blockItem2.Foreground = model.Status != 0 ? (Brush) this._secondaryPrintColor : (Brush) this._primaryPrintColor;
          blockItem2.FontSize = 13.0;
          blockItem2.LineHeight = 10.0;
          blockItem2.Padding = new Thickness(0.0, 1.0, 0.0, 4.0);
          blockItem2.TextDecorations = model.Status != 0 ? TextDecorations.Strikethrough : (TextDecorationCollection) null;
          tableCell2 = new TableCell((Block) blockItem2);
          tableCell3 = new TableCell((Block) new BlockUIContainer((UIElement) titleRightColumnView));
          tableColumn3.Width = new GridLength(titleRightColumnView.NeedWidth);
          break;
      }
      tableRow.Cells.Add(tableCell1);
      tableRow.Cells.Add(tableCell2);
      tableRow.Cells.Add(tableCell3);
      rowGroup.Rows.Add(tableRow);
      if (this._isDetail)
        TaskListPrintView.GetContentOrDesc(model, rowGroup);
      if (model.Type != DisplayType.Section && model.Type != DisplayType.LoadMore && (!this._isDetail || model.SubtaskPrintViewModels == null || model.SubtaskPrintViewModels.Count == 0))
      {
        TableRow lineRow = this.GetLineRow();
        rowGroup.Rows.Add(lineRow);
      }
      table.RowGroups.Add(rowGroup);
      this.Doc.Blocks.Add((Block) table);
      List<SubtaskPrintViewModel> subtaskPrintViewModels = model.SubtaskPrintViewModels;
      // ISSUE: explicit non-virtual call
      if ((subtaskPrintViewModels != null ? (__nonvirtual (subtaskPrintViewModels.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      this.TryAddSubTaskTable(model);
    }

    private TableRow GetLineRow()
    {
      TableRow lineRow = new TableRow();
      Border border = new Border();
      border.Height = 3.0;
      border.BorderBrush = (Brush) ThemeUtil.GetAlphaColor("#191919", 8);
      border.BorderThickness = new Thickness(0.0, 0.0, 0.0, 1.0);
      TableCell tableCell = new TableCell((Block) new BlockUIContainer((UIElement) border))
      {
        ColumnSpan = 2
      };
      lineRow.Cells.Add(new TableCell());
      lineRow.Cells.Add(tableCell);
      return lineRow;
    }

    private void TryAddSubTaskTable(TaskPrintViewModel model)
    {
      Table table = new Table();
      TableColumn tableColumn1 = new TableColumn()
      {
        Width = new GridLength((double) (16 * model.Level + 50))
      };
      TableColumn tableColumn2 = new TableColumn()
      {
        Width = GridLength.Auto
      };
      TableColumn tableColumn3 = new TableColumn()
      {
        Width = new GridLength(100.0)
      };
      table.Columns.Add(tableColumn1);
      table.Columns.Add(tableColumn2);
      table.Columns.Add(tableColumn3);
      TableRowGroup rowGroup = new TableRowGroup();
      foreach (SubtaskPrintViewModel subtaskPrintViewModel in model.SubtaskPrintViewModels)
        this.GetItemRow(subtaskPrintViewModel, rowGroup, model.Level);
      table.RowGroups.Add(rowGroup);
      table.Padding = new Thickness(0.0, 0.0, 0.0, 3.0);
      this.Doc.Blocks.Add((Block) table);
    }

    private void GetItemRow(SubtaskPrintViewModel item, TableRowGroup rowGroup, int parentLevel)
    {
      TableRow tableRow = new TableRow();
      Image image = new Image();
      image.Source = item.Status == 0 ? (ImageSource) Utils.GetImageSource("checkbox01DrawingImage", this._resourceDictionary) : (ImageSource) Utils.GetImageSource("checkbox05DrawingImage", this._resourceDictionary);
      image.Opacity = item.Status == 0 ? 0.36 : 0.18;
      image.HorizontalAlignment = HorizontalAlignment.Left;
      image.VerticalAlignment = VerticalAlignment.Top;
      image.Width = 14.0;
      image.Height = 14.0;
      image.Margin = new Thickness((double) (16 * parentLevel + 24), 2.0, 0.0, 0.0);
      TableCell tableCell1 = new TableCell((Block) new BlockUIContainer((UIElement) image));
      Paragraph blockItem = new Paragraph((Inline) new Run(item.Title));
      blockItem.Foreground = item.Status != 0 ? (Brush) this._secondaryPrintColor : (Brush) this._primaryPrintColor;
      blockItem.FontSize = 13.0;
      blockItem.LineHeight = 20.0;
      blockItem.Padding = new Thickness(0.0, 2.0, 0.0, 3.0);
      blockItem.TextDecorations = item.Status != 0 ? TextDecorations.Strikethrough : (TextDecorationCollection) null;
      TableCell tableCell2 = new TableCell((Block) blockItem);
      if (!item.StartDate.HasValue)
      {
        tableCell2.ColumnSpan = 2;
        tableRow.Cells.Add(tableCell1);
        tableRow.Cells.Add(tableCell2);
      }
      else
      {
        TextBlock textBlock = new TextBlock();
        textBlock.Text = PrintHelper.GetSubTaskTimeText(item);
        textBlock.HorizontalAlignment = HorizontalAlignment.Right;
        textBlock.VerticalAlignment = VerticalAlignment.Top;
        textBlock.Foreground = this.GetTimeTextColor(item.StartDate);
        TableCell tableCell3 = new TableCell((Block) new BlockUIContainer((UIElement) textBlock));
        tableRow.Cells.Add(tableCell1);
        tableRow.Cells.Add(tableCell2);
        tableRow.Cells.Add(tableCell3);
      }
      rowGroup.Rows.Add(tableRow);
    }

    private Brush GetTimeTextColor(DateTime? startDate)
    {
      return startDate.HasValue && startDate.Value.Date < DateTime.Today ? (Brush) ThemeUtil.GetColor("OutDateColor") : (Brush) this._secondaryPrintColor;
    }

    private static void GetContentOrDesc(TaskPrintViewModel model, TableRowGroup rowGroup)
    {
      if (model.Type != DisplayType.Task && model.Type != DisplayType.Agenda && model.Type != DisplayType.Event && model.Type != DisplayType.Note)
        return;
      string text = string.IsNullOrEmpty(model.Content) ? model.Desc : model.Content;
      if (string.IsNullOrEmpty(text))
        return;
      TableRow tableRow = new TableRow();
      Paragraph blockItem = new Paragraph((Inline) new Run(text));
      blockItem.Foreground = (Brush) ThemeUtil.GetColor("PrintTextColorSecondary");
      blockItem.FontSize = 13.0;
      blockItem.LineHeight = 20.0;
      blockItem.Padding = new Thickness(0.0, 2.0, 0.0, 3.0);
      TableCell tableCell = new TableCell((Block) blockItem)
      {
        ColumnSpan = 2
      };
      tableRow.Cells.Add(new TableCell());
      tableRow.Cells.Add(tableCell);
      rowGroup.Rows.Add(tableRow);
    }

    private Path GetCheckboxPath(TaskPrintViewModel model)
    {
      object[] values1 = new object[4]
      {
        (object) model.Type,
        (object) model.Kind,
        (object) model.Status,
        (object) model.CalendarType
      };
      object[] values2 = new object[5]
      {
        (object) model.Type,
        (object) model.Priority,
        (object) model.Status,
        (object) false,
        (object) this
      };
      Path checkboxPath = new Path();
      checkboxPath.Width = model.Type == DisplayType.Course ? 15.0 : 14.0;
      checkboxPath.Height = 14.0;
      checkboxPath.Stretch = Stretch.Uniform;
      checkboxPath.Data = (Geometry) this._iconConvert.Convert(values1, typeof (Geometry), (object) null, (CultureInfo) null);
      checkboxPath.HorizontalAlignment = HorizontalAlignment.Left;
      checkboxPath.VerticalAlignment = VerticalAlignment.Top;
      checkboxPath.Fill = (Brush) this._iconColorConvert.Convert(values2, typeof (SolidColorBrush), (object) null, (CultureInfo) null);
      return checkboxPath;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/tasklistprintview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.RichTextBox = (RichTextBox) target;
          break;
        case 2:
          this.Doc = (FlowDocument) target;
          break;
        case 3:
          this.ListName = (Run) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
