// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.DisplayItemNode
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.ViewModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Models
{
  public class DisplayItemNode : Node<DisplayItemModel>
  {
    public DisplayItemNode(DisplayItemModel model)
    {
      this.Value = model;
      this.NodeId = model.Id;
      this.ParentId = model.ParentId;
      this.ProjectId = model.ProjectId;
      this.Children = new List<Node<DisplayItemModel>>();
    }

    protected override void GetLevel()
    {
      this.Level = this.GetLevel(0);
      this.Value.Level = this.Level;
      if (this.Level < 5)
        return;
      Node<DisplayItemModel> validParent = this.FindValidParent((Node<DisplayItemModel>) this);
      if (validParent == null || validParent == this.Parent || validParent == this)
        return;
      TaskDao.UpdateParent(this.NodeId, validParent.NodeId).ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    public override int GetLevel(int i)
    {
      if (this.Parent == null)
        return i;
      ++i;
      return this.Parent.GetLevel(i);
    }

    protected override void SetValueChildren(Node<DisplayItemModel> child)
    {
      if (this.Value.Children == null)
        this.Value.Children = new List<DisplayItemModel>()
        {
          child.Value
        };
      else if (!this.Value.Children.Contains(child.Value))
        this.Value.Children.Add(child.Value);
      child.Value.Parent = this.Value;
    }

    protected override void ClearValueChildren() => this.Value.Children?.Clear();
  }
}
