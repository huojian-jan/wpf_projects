// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.SummaryPrint
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MarkDown;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class SummaryPrint : UserControl, IComponentConnector
  {
    private double _height;
    private MarkDownEditor _markDown;
    private double _width;
    internal FlowDocumentPageViewer PageViewer;
    internal FlowDocument Doc;
    internal ScrollViewer MarkDownGrid;
    private bool _contentLoaded;

    public SummaryPrint(string text, double width, double height)
    {
      ThemeUtil.SetTheme("light", (FrameworkElement) this);
      this.InitializeComponent();
      this.Doc.PageHeight = height;
      this.Doc.PageWidth = width;
      this._height = height - 150.0;
      this._width = width - 120.0;
      this.PageViewer.Height = height;
      this.Doc.FontFamily = LocalSettings.Settings.ExtraSettings.AppFontFamily == "SourceHansansSC_CN" ? new FontFamily("Microsoft YaHei UI") : LocalSettings.Settings.FontFamily;
      this._markDown = new MarkDownEditor(true);
      this._markDown.Width = width - 120.0;
      this._markDown.Text = text;
      this.MarkDownGrid.Content = (object) this._markDown;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this._markDown.SetTheme();
      this._markDown.UpdateLayout();
      this.GetBlocks();
    }

    private void GetBlocks()
    {
      if (this._markDown.ActualHeight > this._height)
      {
        int pageLastIndex = this._markDown.GetPageLastIndex(this._height);
        string str = this._markDown.Text.Substring(0, pageLastIndex);
        MarkDownEditor markDownEditor = new MarkDownEditor(true);
        markDownEditor.Text = str;
        markDownEditor.Width = this._width;
        markDownEditor.HorizontalAlignment = HorizontalAlignment.Left;
        markDownEditor.Margin = new Thickness(-20.0, 0.0, 0.0, 0.0);
        markDownEditor.EnableSpellCheck = false;
        this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) markDownEditor));
        this._markDown.Text = this._markDown.Text.Substring(pageLastIndex);
        this._markDown.UpdateLayout();
        this.GetBlocks();
      }
      else
      {
        BlockCollection blocks = this.Doc.Blocks;
        MarkDownEditor markDownEditor = new MarkDownEditor(true);
        markDownEditor.Text = this._markDown.Text;
        markDownEditor.Width = this._width;
        markDownEditor.HorizontalAlignment = HorizontalAlignment.Left;
        markDownEditor.Margin = new Thickness(-20.0, 0.0, 0.0, 0.0);
        markDownEditor.EnableSpellCheck = false;
        BlockUIContainer blockUiContainer = new BlockUIContainer((UIElement) markDownEditor);
        blocks.Add((Block) blockUiContainer);
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/summaryprint.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.PageViewer = (FlowDocumentPageViewer) target;
          break;
        case 2:
          this.Doc = (FlowDocument) target;
          break;
        case 3:
          this.MarkDownGrid = (ScrollViewer) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
