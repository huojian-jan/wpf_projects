// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskNode
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskNode : Node<TaskBaseViewModel>
  {
    public TaskNode(TaskBaseViewModel task)
    {
      this.Value = task;
      this.NodeId = task.Id;
      this.ParentId = task.ParentId;
      this.ProjectId = task.ProjectId;
      this.Children = new List<Node<TaskBaseViewModel>>();
    }

    public override Node<DisplayItemModel> ToDisplayItemNode(string parentId)
    {
      DisplayItemNode node = new DisplayItemNode(new DisplayItemModel(this.Value));
      DisplayItemNode displayItemNode1 = node;
      List<Node<TaskBaseViewModel>> children = this.Children;
      List<Node<DisplayItemModel>> list = children != null ? children.Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (n => n.NodeId != parentId)).Select<Node<TaskBaseViewModel>, Node<DisplayItemModel>>((Func<Node<TaskBaseViewModel>, Node<DisplayItemModel>>) (c =>
      {
        Node<DisplayItemModel> displayItemNode2 = c.ToDisplayItemNode(parentId);
        displayItemNode2.Parent = (Node<DisplayItemModel>) node;
        return displayItemNode2;
      })).ToList<Node<DisplayItemModel>>() : (List<Node<DisplayItemModel>>) null;
      displayItemNode1.Children = list;
      return (Node<DisplayItemModel>) node;
    }

    public override bool ExistParent(string parentId)
    {
      if (!this.HasParent)
        return false;
      return this.ParentId == parentId || this.Parent.ExistParent(parentId);
    }
  }
}
