// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ProjectMoveListViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  [Serializable]
  public class ProjectMoveListViewModel : BaseViewModel
  {
    public static ObservableCollection<ProjectMoveListViewModel> GetProjectUseToOrderList(
      ObservableCollection<ProjectModel> projectMoveSaveList,
      ObservableCollection<ProjectGroupModel> projectGroupMoveSaveList)
    {
      ObservableCollection<ProjectMoveListViewModel> source = new ObservableCollection<ProjectMoveListViewModel>();
      foreach (ProjectGroupModel projectGroupMoveSave in (Collection<ProjectGroupModel>) projectGroupMoveSaveList)
        source.Add(new ProjectMoveListViewModel()
        {
          _Id = projectGroupMoveSave._Id,
          id = projectGroupMoveSave.id,
          Name = projectGroupMoveSave.name,
          type = ProjectMoveListViewModel.projectMoveItemType.group,
          sortOrder = projectGroupMoveSave.sortOrder.GetValueOrDefault()
        });
      foreach (ProjectModel projectMoveSave in (Collection<ProjectModel>) projectMoveSaveList)
      {
        if (string.IsNullOrEmpty(projectMoveSave.groupId) || projectMoveSave.groupId == "NONE")
        {
          bool? closed = projectMoveSave.closed;
          bool flag = true;
          if (!(closed.GetValueOrDefault() == flag & closed.HasValue))
            source.Add(new ProjectMoveListViewModel()
            {
              _Id = projectMoveSave._Id,
              id = projectMoveSave.id,
              Name = projectMoveSave.name,
              type = ProjectMoveListViewModel.projectMoveItemType.project,
              sortOrder = projectMoveSave.sortOrder,
              userCount = projectMoveSave.userCount
            });
        }
      }
      return new ObservableCollection<ProjectMoveListViewModel>(source.OrderBy<ProjectMoveListViewModel, long>((Func<ProjectMoveListViewModel, long>) (p => p.sortOrder)).ToList<ProjectMoveListViewModel>());
    }

    public static int FindUseItem(ItemCollection contextMenuList)
    {
      for (int index = 0; index < contextMenuList.Count; ++index)
      {
        if ((contextMenuList[index] as MenuItem).Foreground == ThemeUtil.GetColor("PrimaryColor"))
          return index;
      }
      return 0;
    }

    public int _Id { get; set; }

    public string id { get; set; }

    public long sortOrder { get; set; }

    public int userCount { get; set; }

    public ProjectMoveListViewModel.projectMoveItemType type { get; set; }

    private string name { get; set; }

    private string color { get; set; }

    public string Name
    {
      get => this.name;
      set
      {
        this.name = value;
        this.OnPropertyChanged(nameof (Name));
      }
    }

    public string Color
    {
      get => this.color == null ? ThemeUtil.GetColor("BaseColorOpacity60").ToString() : this.color;
      set
      {
        this.color = value;
        this.OnPropertyChanged(nameof (Color));
      }
    }

    public enum projectMoveItemType
    {
      group,
      project,
    }
  }
}
