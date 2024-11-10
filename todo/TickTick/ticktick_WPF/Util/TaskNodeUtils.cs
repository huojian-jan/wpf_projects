// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TaskNodeUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class TaskNodeUtils
  {
    public static List<Node<DisplayItemModel>> GetSortedTaskNodeTree(
      IEnumerable<DisplayItemModel> models)
    {
      Dictionary<string, Node<DisplayItemModel>> taskNodes = new Dictionary<string, Node<DisplayItemModel>>();
      if (models == null)
        return (List<Node<DisplayItemModel>>) null;
      Dictionary<string, int> dict = new Dictionary<string, int>();
      List<DisplayItemModel> list1 = models.ToList<DisplayItemModel>();
      List<string> noteList = new List<string>();
      for (int index = 0; index < list1.Count; ++index)
      {
        DisplayItemModel displayItemModel = list1[index];
        if (displayItemModel.Kind == "NOTE")
        {
          if (displayItemModel.Parent != null)
          {
            displayItemModel.Parent = (DisplayItemModel) null;
            TaskDao.UpdateParent(displayItemModel.Id, string.Empty, false).ContinueWith(new Action<Task>(UtilRun.LogTask));
            UtilLog.Info("SetTaskParent empty " + displayItemModel.Id + " is Note");
          }
          displayItemModel.Children = (List<DisplayItemModel>) null;
          displayItemModel.ChildIds = (List<string>) null;
          noteList.Add(list1[index].Id);
        }
        dict[list1[index].Id] = index;
        taskNodes[list1[index].Id] = (Node<DisplayItemModel>) new DisplayItemNode(list1[index]);
      }
      foreach (Node<DisplayItemModel> node in taskNodes.Values.Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (item => noteList.Contains(item.ParentId))).Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (item => !string.IsNullOrEmpty(item.ParentId))))
      {
        node.Parent = (Node<DisplayItemModel>) null;
        node.ParentId = string.Empty;
      }
      TaskNodeUtils.BuildNodeTree<DisplayItemModel>(taskNodes);
      List<Node<DisplayItemModel>> list2 = taskNodes.Values.Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (node => !node.HasParent)).ToList<Node<DisplayItemModel>>();
      TaskNodeUtils.SortNodes<DisplayItemModel>(list2, dict);
      return list2;
    }

    public static List<TaskBaseViewModel> SortTaskDisplayModels(
      IEnumerable<TaskBaseViewModel> models)
    {
      Dictionary<string, Node<TaskBaseViewModel>> taskNodes = new Dictionary<string, Node<TaskBaseViewModel>>();
      if (models == null)
        return (List<TaskBaseViewModel>) null;
      Dictionary<string, int> dict = new Dictionary<string, int>();
      List<TaskBaseViewModel> list1 = models.ToList<TaskBaseViewModel>();
      for (int index = 0; index < list1.Count; ++index)
      {
        dict[list1[index].Id] = index;
        taskNodes[list1[index].Id] = (Node<TaskBaseViewModel>) new TaskNode(list1[index]);
      }
      TaskNodeUtils.BuildNodeTree<TaskBaseViewModel>(taskNodes);
      List<Node<TaskBaseViewModel>> list2 = taskNodes.Values.Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (node => !node.HasParent)).ToList<Node<TaskBaseViewModel>>();
      TaskNodeUtils.SortNodes<TaskBaseViewModel>(list2, dict);
      List<TaskBaseViewModel> taskBaseViewModelList = new List<TaskBaseViewModel>();
      foreach (Node<TaskBaseViewModel> node in list2)
      {
        taskBaseViewModelList.Add(node.Value);
        taskBaseViewModelList.AddRange(node.GetAllChildrenNode().Select<Node<TaskBaseViewModel>, TaskBaseViewModel>((Func<Node<TaskBaseViewModel>, TaskBaseViewModel>) (n => n.Value)));
      }
      return taskBaseViewModelList;
    }

    public static void BuildNodeTree<T>(
      Dictionary<string, Node<T>> taskNodes,
      bool checkParentExist = false)
      where T : INode
    {
      if (taskNodes == null)
        return;
      foreach (Node<T> node in taskNodes.Values)
      {
        if (!string.IsNullOrEmpty(node.ParentId))
        {
          if (taskNodes.ContainsKey(node.ParentId))
          {
            Node<T> taskNode = taskNodes[node.ParentId];
            if (taskNode.ProjectId != node.ProjectId)
            {
              node.Value.RemoveParent();
              UtilLog.Info("SetTaskParent empty " + node.NodeId + " Project Error");
              node.ParentId = "";
            }
            else if (node.NodeId != node.ParentId && !taskNode.ExistParent(node.NodeId))
            {
              taskNode.Children.Add(node);
              node.Parent = taskNodes[node.ParentId];
            }
            else
            {
              node.Value.RemoveParent();
              UtilLog.Info("SetTaskParent empty " + node.NodeId + " loop " + node.ParentId);
              node.ParentId = "";
            }
          }
          else if (checkParentExist)
          {
            TaskBaseViewModel taskById = TaskCache.GetTaskById(node.ParentId);
            if (taskById != null && taskById.ProjectId != node.ProjectId)
            {
              UtilLog.Info("SetTaskParent empty " + node.NodeId + " Project Error");
              node.Value.RemoveParent();
              node.ParentId = "";
            }
          }
        }
      }
    }

    private static void SortNodes<T>(List<Node<T>> nodes, Dictionary<string, int> dict) where T : INode
    {
      if (nodes == null || nodes.Count == 0)
        return;
      nodes.ForEach((Action<Node<T>>) (node => TaskNodeUtils.SortNodes<T>(node.Children, dict)));
      nodes.Sort((Comparison<Node<T>>) ((a, b) =>
      {
        if (dict.ContainsKey(a.NodeId) && dict.ContainsKey(b.NodeId))
          return dict[a.NodeId].CompareTo(dict[b.NodeId]);
        return !dict.ContainsKey(b.NodeId) ? -1 : 1;
      }));
    }

    public static List<T> GetModelsFromNodes<T>(List<Node<T>> nodes) where T : INode
    {
      List<T> modelsFromNodes = new List<T>();
      foreach (Node<T> node in nodes)
      {
        if (!node.HasParent)
        {
          modelsFromNodes.Add(node.Value);
          modelsFromNodes.AddRange((IEnumerable<T>) node.GetAllChildrenValue());
        }
      }
      return modelsFromNodes;
    }

    public static List<Node<TaskBaseViewModel>> GetTaskNodeTree(List<TaskBaseViewModel> tasks)
    {
      Dictionary<string, Node<TaskBaseViewModel>> dictionary = tasks.ToDictionary<TaskBaseViewModel, string, Node<TaskBaseViewModel>>((Func<TaskBaseViewModel, string>) (t => t.Id), (Func<TaskBaseViewModel, Node<TaskBaseViewModel>>) (t => (Node<TaskBaseViewModel>) new TaskNode(t)));
      TaskNodeUtils.BuildNodeTree<TaskBaseViewModel>(dictionary);
      return dictionary.Values.ToList<Node<TaskBaseViewModel>>();
    }
  }
}
