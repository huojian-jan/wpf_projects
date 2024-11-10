// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.TitleRightColumnView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class TitleRightColumnView : UserControl, IComponentConnector
  {
    public double NeedWidth;
    internal TextBlock FirstTag;
    internal TextBlock DateText;
    internal Rectangle AvatarImage;
    private bool _contentLoaded;

    public TitleRightColumnView() => this.InitializeComponent();

    public TitleRightColumnView(RightColumnViewModel model)
    {
      this.DataContext = (object) model;
      this.InitializeComponent();
      if (!string.IsNullOrEmpty(model.AvatarUrl.Trim()))
        this.AvatarImage.Fill = (Brush) new ImageBrush((ImageSource) AvatarHelper.GetAvatarByUrl(model.AvatarUrl));
      if (model.StartDate.HasValue && !Utils.IsEmptyDate(model.StartDate.Value))
        this.DateText.Text = DateUtils.FormatListDateString(model.StartDate.Value, model.DueDate, model.IsAllDay);
      if (!string.IsNullOrEmpty(model.FirstTag))
        this.NeedWidth += Math.Min(this.MeasureString(model.FirstTag).Width, 70.0) + 13.0;
      if (!string.IsNullOrEmpty(model.SecondTag))
        this.NeedWidth += Math.Min(this.MeasureString(model.SecondTag).Width, 70.0) + 13.0;
      if (!string.IsNullOrEmpty(model.MoreTag))
        this.NeedWidth += this.MeasureString(model.MoreTag).Width + 13.0;
      if (!string.IsNullOrEmpty(model.ProjectName))
        this.NeedWidth += Math.Min(this.MeasureString(model.ProjectName).Width, 70.0) + 10.0;
      if (model.ShowAttachment)
        this.NeedWidth += 20.0;
      if (model.ShowComment)
        this.NeedWidth += 20.0;
      if (model.ShowDescription)
        this.NeedWidth += 20.0;
      if (model.ShowReminder)
        this.NeedWidth += 20.0;
      if (model.ShowLocation)
        this.NeedWidth += 20.0;
      if (model.ShowProgress)
        this.NeedWidth += 20.0;
      if (model.ShowRepeat)
        this.NeedWidth += 20.0;
      if (!string.IsNullOrEmpty(this.DateText.Text))
        this.NeedWidth += this.MeasureString(this.DateText.Text).Width + 12.0;
      if (string.IsNullOrEmpty(model.AvatarUrl) || !(model.AvatarUrl != "-1"))
        return;
      this.NeedWidth += 35.0;
    }

    private Size MeasureString(string candidate)
    {
      return Utils.MeasureString(candidate, new Typeface(this.FirstTag.FontFamily, this.FirstTag.FontStyle, this.FirstTag.FontWeight, this.FirstTag.FontStretch), this.FirstTag.FontSize);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/titlerightcolumnview.xaml", UriKind.Relative));
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
          this.FirstTag = (TextBlock) target;
          break;
        case 2:
          this.DateText = (TextBlock) target;
          break;
        case 3:
          this.AvatarImage = (Rectangle) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
