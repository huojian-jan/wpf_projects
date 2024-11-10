// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.InsertLinkWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class InsertLinkWindow : Window, IComponentConnector
  {
    private readonly ILinkTextEditor _editor;
    private readonly string _originalTitle;
    private readonly string _originalUrl;
    private bool _isNew;
    internal TextBlock TitleText;
    internal TextBox LinkNameText;
    internal TextBox LinkUrlText;
    internal Button SaveButton;
    internal Button DeleteLink;
    private bool _contentLoaded;

    public InsertLinkWindow(ILinkTextEditor editor, string title, string url, bool isNew)
    {
      this.InitializeComponent();
      this._editor = editor;
      this._originalTitle = title;
      this._originalUrl = url;
      this._isNew = isNew;
      this.LinkNameText.Text = title;
      this.LinkUrlText.Text = url;
      if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(url))
      {
        this.LinkNameText.Focus();
      }
      else
      {
        this.LinkUrlText.CaretIndex = url.Length;
        this.LinkUrlText.Focus();
      }
      if (string.IsNullOrEmpty(url))
      {
        this.TitleText.Text = Utils.GetString("AddLink");
        this.DeleteLink.Visibility = Visibility.Collapsed;
      }
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      double left = this.Left;
      double top = this.Top;
      WindowHelper.MoveTo((Window) this, (int) left, (int) top);
      this.Left = left;
      this.Top = top;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private string GetLinkText()
    {
      string linkText = this.LinkUrlText.Text;
      if (!string.IsNullOrWhiteSpace(linkText) && !string.IsNullOrWhiteSpace(this.LinkNameText.Text))
        linkText = linkText + " \"" + this.LinkNameText.Text + "\"";
      return linkText;
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      InsertLinkWindow insertLinkWindow = this;
      insertLinkWindow._editor.UnRegisterCaretChanged();
      if (string.IsNullOrWhiteSpace(insertLinkWindow.LinkNameText.Text))
      {
        ProjectTask taskUrlWithoutTitle = TaskUtils.ParseTaskUrlWithoutTitle(insertLinkWindow.LinkUrlText.Text);
        if (taskUrlWithoutTitle != null)
        {
          TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskUrlWithoutTitle.TaskId);
          if (thinTaskById != null)
            insertLinkWindow.LinkNameText.Text = thinTaskById.title;
        }
        if (string.IsNullOrEmpty(insertLinkWindow.LinkNameText.Text))
          insertLinkWindow.LinkNameText.Text = Utils.GetString("MyTask");
      }
      if (!insertLinkWindow._isNew)
      {
        string original = "[" + insertLinkWindow._originalTitle + "](" + insertLinkWindow._originalUrl + ")";
        string revised = "[" + insertLinkWindow.LinkNameText.Text + "](" + insertLinkWindow.LinkUrlText.Text + ")";
        if (string.IsNullOrEmpty(insertLinkWindow.LinkUrlText.Text))
          revised = insertLinkWindow.LinkNameText.Text ?? "";
        EditorUtilities.ReplaceLink(insertLinkWindow._editor.GetEditBox(), original, revised);
      }
      else
        EditorUtilities.InsertHyperlink(insertLinkWindow._editor.GetEditBox(), insertLinkWindow.GetLinkText());
      insertLinkWindow._editor.RegisterCaretChanged();
      insertLinkWindow.Close();
    }

    private void OnDeleteLinkClick(object sender, RoutedEventArgs e)
    {
      this._editor.UnRegisterCaretChanged();
      string original = "[" + this._originalTitle + "](" + this._originalUrl + ")";
      string revised = this.LinkNameText.Text ?? "";
      EditorUtilities.ReplaceLink(this._editor.GetEditBox(), original, revised);
      this._editor.RegisterCaretChanged();
      this.Close();
    }

    private void OnLinkNameKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      this.LinkUrlText.Focus();
    }

    private void OnLinkUrlKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      this.OnSaveClick(sender, (RoutedEventArgs) e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/markdown/insertlinkwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.TitleText = (TextBlock) target;
          break;
        case 2:
          this.LinkNameText = (TextBox) target;
          this.LinkNameText.KeyDown += new KeyEventHandler(this.OnLinkNameKeyDown);
          break;
        case 3:
          this.LinkUrlText = (TextBox) target;
          this.LinkUrlText.KeyDown += new KeyEventHandler(this.OnLinkUrlKeyDown);
          break;
        case 4:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 5:
          this.DeleteLink = (Button) target;
          this.DeleteLink.Click += new RoutedEventHandler(this.OnDeleteLinkClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
