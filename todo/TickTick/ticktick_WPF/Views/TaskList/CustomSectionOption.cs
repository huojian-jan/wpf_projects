// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.CustomSectionOption
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class CustomSectionOption : ContentControl, IComponentConnector
  {
    private Action _addSectionUpper;
    private Action _addSectionUnder;
    private Action _rename;
    private Action _delete;
    private Action<string, string, SelectableItemViewModel> _moveColumn;
    private EscPopup _popup;
    private bool _contentLoaded;

    public CustomSectionOption(EscPopup popup)
    {
      this._popup = popup;
      this.InitializeComponent();
    }

    public void Show(string columnId = null, string projectId = null)
    {
      List<CustomMenuItemViewModel> menuItemViewModelList1 = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "rename", Utils.GetString("Rename"), Utils.GetImageSource("EditDrawingImage"));
      menuItemViewModel1.ShowSelected = false;
      menuItemViewModelList1.Add(menuItemViewModel1);
      List<CustomMenuItemViewModel> types = menuItemViewModelList1;
      int num = 1;
      if (this._addSectionUpper != null)
      {
        ++num;
        List<CustomMenuItemViewModel> menuItemViewModelList2 = types;
        CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "addUp", Utils.GetString("AddSectionUpper"), Utils.GetImageSource("AddUpperDrawingImage"));
        menuItemViewModel2.ShowSelected = false;
        menuItemViewModelList2.Add(menuItemViewModel2);
      }
      if (this._addSectionUnder != null)
      {
        ++num;
        List<CustomMenuItemViewModel> menuItemViewModelList3 = types;
        CustomMenuItemViewModel menuItemViewModel3 = new CustomMenuItemViewModel((object) "addUnder", Utils.GetString("AddSectionBelow"), Utils.GetImageSource("AddUnderDrawingImage"));
        menuItemViewModel3.ShowSelected = false;
        menuItemViewModelList3.Add(menuItemViewModel3);
      }
      if (this._delete != null)
      {
        List<CustomMenuItemViewModel> menuItemViewModelList4 = types;
        CustomMenuItemViewModel menuItemViewModel4 = new CustomMenuItemViewModel((object) "delete", Utils.GetString("Delete"), Utils.GetImageSource("DeleteDrawingLine"));
        menuItemViewModel4.ShowSelected = false;
        menuItemViewModelList4.Add(menuItemViewModel4);
      }
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this._popup);
      if (!string.IsNullOrWhiteSpace(columnId))
      {
        ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) customMenuList.SubPopup, new ProjectExtra()
        {
          ProjectIds = new List<string>() { projectId }
        }, new ProjectSelectorExtra()
        {
          showAll = false,
          batchMode = false,
          canSelectGroup = false,
          CanSearch = true,
          onlyShowPermission = true,
          ShowColumn = false
        });
        List<CustomMenuItemViewModel> menuItemViewModelList5 = types;
        int index = num;
        CustomMenuItemViewModel menuItemViewModel5 = new CustomMenuItemViewModel((object) "move", Utils.GetString("MoveTo"), Utils.GetIcon("IcMovetoLine"));
        menuItemViewModel5.SubControl = (ITabControl) projectOrGroupPopup;
        menuItemViewModel5.NeedSubContentStyle = false;
        menuItemViewModel5.Selectable = false;
        menuItemViewModelList5.Insert(index, menuItemViewModel5);
        projectOrGroupPopup.ItemSelect += (EventHandler<SelectableItemViewModel>) (async (o, viewModel) =>
        {
          this._popup.IsOpen = false;
          await Task.Delay(100);
          this._moveColumn(columnId, projectId, viewModel);
        });
      }
      customMenuList.Operated += new EventHandler<object>(this.OnOptionSelected);
      customMenuList.Show();
    }

    private void OnOptionSelected(object sender, object e)
    {
      if (!(e is string str))
        return;
      switch (str)
      {
        case "rename":
          this._rename();
          break;
        case "addUp":
          this._addSectionUpper();
          break;
        case "addUnder":
          this._addSectionUnder();
          break;
        case "delete":
          this._delete();
          break;
      }
    }

    public void SetAction(
      Action addSectionUpper,
      Action addSectionUnder,
      Action rename,
      Action delete,
      Action<string, string, SelectableItemViewModel> moveColumn = null)
    {
      this._addSectionUpper = addSectionUpper;
      this._addSectionUnder = addSectionUnder;
      this._rename = rename;
      this._delete = delete;
      this._moveColumn = moveColumn;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tasklist/customsectionoption.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target) => this._contentLoaded = true;
  }
}
