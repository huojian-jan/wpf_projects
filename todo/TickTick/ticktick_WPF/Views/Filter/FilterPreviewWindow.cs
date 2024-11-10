// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.FilterPreviewWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class FilterPreviewWindow : Window, ISectionList, IOkCancelWindow, IComponentConnector
  {
    private readonly FilterModel _filterModel;
    internal ListView ListView;
    internal StackPanel NoTaskGrid;
    internal Image EmptyImage;
    internal Path EmptyPath;
    internal Button SaveButton;
    private bool _contentLoaded;

    public FilterPreviewWindow(FilterModel filterModel)
    {
      this._filterModel = filterModel;
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Load((ProjectIdentity) new FilterPreviewIdentity(this._filterModel));
    }

    private async Task Load(ProjectIdentity projectIdentity)
    {
      List<DisplayItemModel> revisedItems = await TaskListRefreshHelper.GetSortedDisplayItems(projectIdentity, await TaskListRefreshHelper.InitData(projectIdentity)) ?? new List<DisplayItemModel>();
      if (revisedItems.Count > 0)
      {
        List<DisplayItemModel> items = new List<DisplayItemModel>();
        foreach (DisplayItemModel displayItemModel in revisedItems)
        {
          items.Add(displayItemModel);
          displayItemModel.HitVisible = false;
          if (!displayItemModel.IsOpen)
          {
            List<DisplayItemModel> children = displayItemModel.Children;
            // ISSUE: explicit non-virtual call
            if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
              displayItemModel.GetChildrenModels(true).ForEach((Action<DisplayItemModel>) (c =>
              {
                c.HitVisible = false;
                items.Add(c);
              }));
          }
        }
        await TaskItemLoadHelper.LoadIndicator(items, false);
        ItemsSourceHelper.SetItemsSource<DisplayItemModel>((ItemsControl) this.ListView, revisedItems);
        revisedItems = (List<DisplayItemModel>) null;
      }
      else
      {
        this.NoTaskGrid.Visibility = Visibility.Visible;
        revisedItems = (List<DisplayItemModel>) null;
      }
    }

    private void SaveBtnClick(object sender, RoutedEventArgs e)
    {
      this.Close();
      this.StopPreview(true);
    }

    private void CancelBtnClick(object sender, RoutedEventArgs e)
    {
      this.Close();
      this.StopPreview();
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    public async void StopPreview(bool save = false)
    {
      FilterPreviewWindow filterPreviewWindow = this;
      FilterModel filter = filterPreviewWindow._filterModel;
      bool needToast = save;
      if (save)
      {
        if (FilterDao.CheckNameValid(filter.name, filter.id))
        {
          await FilterDao.SaveFilterFromPreview(filter);
          SyncManager.TryDelaySync();
          if (!(filterPreviewWindow.Owner is IListViewParent listViewParent))
            listViewParent = (IListViewParent) App.Window;
          listViewParent?.SaveSelectedProject("filter:" + filter.id);
        }
        else
          save = false;
      }
      if (save)
      {
        filter = (FilterModel) null;
      }
      else
      {
        FilterEditDialog filterEditDialog = new FilterEditDialog(filter, needToast);
        filterEditDialog.Owner = filterPreviewWindow.Owner;
        filterEditDialog.ShowDialog();
        filter = (FilterModel) null;
      }
    }

    public void OnSectionStatusChanged(SectionStatus status)
    {
      if (!(this.ListView.ItemsSource is ObservableCollection<DisplayItemModel> itemsSource))
        return;
      List<DisplayItemModel> models = itemsSource.ToList<DisplayItemModel>();
      DisplayItemModel displayItemModel1 = models.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Type == DisplayType.Section && item.Section.SectionId == status.SectionId));
      int num = models.IndexOf(displayItemModel1);
      if (displayItemModel1 != null && num >= 0)
      {
        if (status.IsOpen)
        {
          if (num >= 0)
          {
            for (int index = 0; index < displayItemModel1.Children.Count; ++index)
              models.Insert(num + 1 + index, displayItemModel1.Children[index]);
          }
        }
        else
        {
          List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
          for (int index = num + 1; index < models.Count; ++index)
          {
            DisplayItemModel displayItemModel2 = models[index];
            if (!displayItemModel2.IsSection)
              displayItemModelList.Add(displayItemModel2);
            else
              break;
          }
          displayItemModel1.Children = displayItemModelList;
          displayItemModelList.ForEach((Action<DisplayItemModel>) (child => models.Remove(child)));
        }
      }
      ItemsSourceHelper.SetItemsSource<DisplayItemModel>((ItemsControl) this.ListView, models);
    }

    public void SelectOrDeselectAll(DisplayItemModel model, bool selectAll)
    {
    }

    public async Task OnTaskOpenClick(DisplayItemModel model)
    {
      if (!(this.ListView.ItemsSource is ObservableCollection<DisplayItemModel> itemsSource))
        return;
      List<DisplayItemModel> models = itemsSource.ToList<DisplayItemModel>();
      if (model.IsOpen)
      {
        model.GetChildrenModels(true).ForEach((Action<DisplayItemModel>) (m => models.Remove(m)));
      }
      else
      {
        List<DisplayItemModel> childrenModels = model.GetChildrenModels(false);
        int num = models.IndexOf(model);
        if (num >= 0)
        {
          for (int index = childrenModels.Count - 1; index >= 0; --index)
            models.Insert(num + 1, childrenModels[index]);
        }
      }
      model.IsOpen = !model.IsOpen;
      ItemsSourceHelper.SetItemsSource<DisplayItemModel>((ItemsControl) this.ListView, models);
    }

    public async Task OnAddTaskInSectionClick(DisplayItemModel model)
    {
    }

    public void OnCancel()
    {
      this.Close();
      this.StopPreview();
    }

    public void Ok()
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/filterpreviewwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ListView = (ListView) target;
          break;
        case 2:
          this.NoTaskGrid = (StackPanel) target;
          break;
        case 3:
          this.EmptyImage = (Image) target;
          break;
        case 4:
          this.EmptyPath = (Path) target;
          break;
        case 5:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.SaveBtnClick);
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CancelBtnClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
