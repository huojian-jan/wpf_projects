// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchTaskItemControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchTaskItemControl : UserControl, IComponentConnector
  {
    internal TaskCheckIcon CheckIcon;
    internal TaskTitleBox Title;
    internal EmjTextBlock ProjectTitle;
    internal TextBlock ContentText;
    internal CommentSearchLabel CommentText;
    private bool _contentLoaded;

    public SearchTaskItemControl()
    {
      this.InitializeComponent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataChanged);
      this.Title.SetupPreSearchRender();
      this.Title.IsHitTestVisible = false;
      this.CommentText.CommentText.FontSize = 12.0;
    }

    private void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is SearchTaskItemViewModel dataContext))
        return;
      this.Title.SetText(dataContext.SourceModel.Title);
      this.CheckIcon.SetIconColor(dataContext.SourceModel.Type, dataContext.SourceModel.Priority, dataContext.SourceModel.Status);
      this.CheckIcon.Cursor = !dataContext.SourceModel.Editable ? Cursors.No : (dataContext.SourceModel.IsEvent || dataContext.SourceModel.IsNote || dataContext.SourceModel.IsCourse ? Cursors.Arrow : Cursors.Hand);
      if (!string.IsNullOrWhiteSpace(dataContext.Content))
      {
        this.ContentText.Visibility = Visibility.Visible;
        this.SetContent(dataContext.Content, this.ContentText);
        this.ContentText.Margin = new Thickness(42.0, 1.0, 130.0, string.IsNullOrWhiteSpace(dataContext.CommentStr) ? 4.0 : 0.0);
      }
      if (!string.IsNullOrEmpty(dataContext.SourceModel.ProjectId))
        this.ProjectTitle.Text = dataContext.SourceModel.ProjectName;
      if (string.IsNullOrWhiteSpace(dataContext.CommentStr))
        return;
      this.CommentText.Visibility = Visibility.Visible;
      this.SetContent(dataContext.CommentStr, this.CommentText.CommentText);
    }

    private void SetContent(string content, TextBlock block)
    {
      block.Inlines.Clear();
      string str = content;
      string text1 = (str.Length > 300 ? str.Substring(0, 299) : str).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("    ", " ");
      MatchCollection matchCollection = SearchHelper.PreSearchRegex.Matches(text1.ToLower());
      if (matchCollection.Count > 0)
      {
        for (int i = 0; i < matchCollection.Count; ++i)
        {
          int startIndex = i > 0 ? matchCollection[i - 1].Index + matchCollection[i - 1].Length : 0;
          string text2 = text1.Substring(startIndex, matchCollection[i].Index - startIndex);
          block.Inlines.Add(text2);
          InlineCollection inlines = block.Inlines;
          Run run = new Run(text1.Substring(matchCollection[i].Index, matchCollection[i].Length));
          run.Background = SearchHelper.GetSearchHighlightColor();
          inlines.Add((Inline) run);
        }
        int startIndex1 = matchCollection[matchCollection.Count - 1].Index + matchCollection[matchCollection.Count - 1].Length;
        string text3 = text1.Substring(startIndex1, text1.Length - startIndex1);
        block.Inlines.Add(text3);
      }
      else
        block.Inlines.Add(text1);
    }

    private async void OnCheckIconClick(object sender, MouseButtonEventArgs e)
    {
      SearchTaskItemControl child = this;
      if (!(child.DataContext is SearchTaskItemViewModel dataContext) || child.CheckIcon.Cursor != Cursors.Hand)
        return;
      e.Handled = true;
      if (dataContext.TogglingStatus)
        return;
      dataContext.TogglingStatus = true;
      dataContext.Status = dataContext.SourceModel.Status == 0 ? 2 : 0;
      dataContext.ResetIcon();
      TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(dataContext.SourceModel.Id, dataContext.Status, true, (IToastShowWindow) App.Window);
      Utils.FindParent<SearchInputControl>((DependencyObject) child)?.OnItemCheckClick();
    }

    private void OnProjectClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is SearchTaskItemViewModel dataContext))
        return;
      e.Handled = true;
      Utils.FindParent<SearchInputControl>((DependencyObject) this)?.OnItemProjectClick(dataContext.SourceModel.ProjectId);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/searchtaskitemcontrol.xaml", UriKind.Relative));
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
          this.CheckIcon = (TaskCheckIcon) target;
          break;
        case 2:
          this.Title = (TaskTitleBox) target;
          break;
        case 3:
          this.ProjectTitle = (EmjTextBlock) target;
          this.ProjectTitle.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnProjectClick);
          break;
        case 4:
          this.ContentText = (TextBlock) target;
          break;
        case 5:
          this.CommentText = (CommentSearchLabel) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
