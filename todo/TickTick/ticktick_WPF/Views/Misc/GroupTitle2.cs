// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.GroupTitle2
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class GroupTitle2 : UserControl, IComponentConnector, IStyleConnector
  {
    public static readonly DependencyProperty CountProperty = DependencyProperty.Register("BorderHeight", typeof (int), typeof (GroupTitle2), new PropertyMetadata((object) 2));
    private List<GroupTitleViewModel> _models = new List<GroupTitleViewModel>();
    public string _titles = string.Empty;
    private int _tabIndex = -1;
    internal GroupTitle2 Root;
    internal Border BackBorder;
    internal ItemsControl TitlesListView;
    private bool _contentLoaded;

    public int Count
    {
      get => (int) this.GetValue(GroupTitle2.CountProperty);
      set => this.SetValue(GroupTitle2.CountProperty, (object) value);
    }

    public event EventHandler<GroupTitleViewModel> SelectedTitleChanged;

    public string Titles
    {
      get => this._titles;
      set
      {
        this._titles = value;
        this.UpdateTitles();
      }
    }

    public void SetSelectedIndex(int index)
    {
      if (index >= this._models.Count || index < 0)
        return;
      for (int index1 = 0; index1 < this._models.Count; ++index1)
        this._models[index1].IsSelected = index == index1;
    }

    public int GetSelectedIndex()
    {
      GroupTitleViewModel groupTitleViewModel = this._models.FirstOrDefault<GroupTitleViewModel>((Func<GroupTitleViewModel, bool>) (m => m.IsSelected));
      return groupTitleViewModel == null ? 0 : groupTitleViewModel.Index;
    }

    public GroupTitle2() => this.InitializeComponent();

    public void UpdateTitles()
    {
      string[] source = this._titles.Split('|');
      if (((IEnumerable<string>) source).Any<string>())
      {
        this._models.Clear();
        for (int index = 0; index < source.Length; ++index)
        {
          string key = source[index];
          this._models.Add(new GroupTitleViewModel()
          {
            Content = Utils.GetString(key),
            Title = key,
            Index = index
          });
        }
        if (this._models.Count > 0)
          this._models[0].IsSelected = true;
      }
      this.Count = Math.Max(1, source.Length);
      ItemsSourceHelper.SetItemsSource<GroupTitleViewModel>(this.TitlesListView, this._models);
    }

    public void SetTitleContent(string title, string content)
    {
      foreach (GroupTitleViewModel model in this._models)
      {
        if (model.Title.Equals(title))
          model.Content = content;
      }
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is GroupTitleViewModel dataContext) || dataContext.IsSelected)
        return;
      this._models.ForEach((Action<GroupTitleViewModel>) (m => m.IsSelected = false));
      dataContext.IsSelected = true;
      EventHandler<GroupTitleViewModel> selectedTitleChanged = this.SelectedTitleChanged;
      if (selectedTitleChanged == null)
        return;
      selectedTitleChanged((object) this, dataContext);
    }

    public void TabSelectFirst()
    {
      this._tabIndex = 0;
      this._models.ForEach((Action<GroupTitleViewModel>) (m => m.IsTabSelected = false));
      this._models[this._tabIndex].IsTabSelected = true;
    }

    public void TabSelectLast()
    {
      this._tabIndex = this._models.Count - 1;
      this._models.ForEach((Action<GroupTitleViewModel>) (m => m.IsTabSelected = false));
      this._models[this._tabIndex].IsTabSelected = true;
    }

    public void TabSelect(int index)
    {
      this._models.ForEach((Action<GroupTitleViewModel>) (m => m.IsTabSelected = false));
      if (index < this._models.Count && index >= 0)
      {
        this._tabIndex = index;
        this._models[this._tabIndex].IsTabSelected = true;
      }
      else
        this._tabIndex = -1;
    }

    public bool TabSelectNext(bool next)
    {
      if (this._models.All<GroupTitleViewModel>((Func<GroupTitleViewModel, bool>) (m => !m.IsTabSelected)))
      {
        if (this.InTab())
          this._models[this._tabIndex].IsTabSelected = true;
        return false;
      }
      this._tabIndex += next ? 1 : -1;
      if (Math.Max(0, Math.Min(this._tabIndex, this._models.Count - 1)) != this._tabIndex)
        return false;
      this._models.ForEach((Action<GroupTitleViewModel>) (m => m.IsTabSelected = false));
      this._models[++this._tabIndex].IsTabSelected = true;
      return true;
    }

    public bool InTab() => this._tabIndex >= 0 && this._tabIndex < this._models.Count;

    public void SelectTabItem()
    {
      if (!this.InTab())
        return;
      GroupTitleViewModel model = this._models[this._tabIndex];
      if (model.IsSelected)
        return;
      this._models.ForEach((Action<GroupTitleViewModel>) (m => m.IsSelected = false));
      model.IsSelected = true;
      EventHandler<GroupTitleViewModel> selectedTitleChanged = this.SelectedTitleChanged;
      if (selectedTitleChanged == null)
        return;
      selectedTitleChanged((object) this, model);
    }

    public void ClearTabSelected()
    {
      this._models.ForEach((Action<GroupTitleViewModel>) (m => m.IsTabSelected = false));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/grouptitle2.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (GroupTitle2) target;
          break;
        case 2:
          this.BackBorder = (Border) target;
          break;
        case 3:
          this.TitlesListView = (ItemsControl) target;
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
      if (connectionId != 4)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
    }
  }
}
