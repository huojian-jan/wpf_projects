// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.Node`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Dal;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class Node<T> where T : INode
  {
    public string NodeId;
    public string ProjectId;
    public List<Node<T>> Children;
    public Node<T> Parent;
    public int Level;
    public T Value;
    public string ParentId;

    public bool HasParent => this.Parent != null;

    public List<T> GetAllChildrenValue()
    {
      Dictionary<string, T> dict = new Dictionary<string, T>();
      this.ClearValueChildren();
      List<Node<T>> children = this.Children;
      // ISSUE: explicit non-virtual call
      if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        foreach (Node<T> child in this.Children)
        {
          this.SetValueChildren(child);
          this.GetChildrenValue(child, dict);
        }
      }
      return dict.Values.ToList<T>();
    }

    protected virtual void ClearValueChildren()
    {
    }

    public virtual bool ExistParent(string parentId) => false;

    protected Node<T> FindValidParent(Node<T> node)
    {
      return !node.HasParent || node.Parent == null || node.Level < 4 ? node : this.FindValidParent(node.Parent);
    }

    private void GetChildrenValue(Node<T> node, Dictionary<string, T> dict)
    {
      if (dict.ContainsKey(node.NodeId))
      {
        TaskDao.UpdateParent(node.NodeId, "", false);
      }
      else
      {
        dict.Add(node.NodeId, node.Value);
        node.GetLevel();
        node.ClearValueChildren();
        List<Node<T>> children = node.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) == 0)
          return;
        node.ClearValueChildren();
        foreach (Node<T> child in node.Children)
        {
          node.SetValueChildren(child);
          this.GetChildrenValue(child, dict);
        }
      }
    }

    protected virtual void SetValueChildren(Node<T> child)
    {
    }

    public List<Node<T>> GetAllChildrenNode()
    {
      Dictionary<string, Node<T>> dict = new Dictionary<string, Node<T>>();
      List<Node<T>> children = this.Children;
      // ISSUE: explicit non-virtual call
      if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        foreach (Node<T> child in this.Children)
          this.GetChildrenNode(child, dict);
      }
      return dict.Values.ToList<Node<T>>();
    }

    private void GetChildrenNode(Node<T> node, Dictionary<string, Node<T>> dict)
    {
      if (dict.ContainsKey(node.NodeId))
        return;
      dict.Add(node.NodeId, node);
      List<Node<T>> children = node.Children;
      // ISSUE: explicit non-virtual call
      if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (Node<T> child in node.Children)
        this.GetChildrenNode(child, dict);
    }

    protected virtual void GetLevel() => this.Level = this.GetLevel(0);

    public virtual int GetLevel(int i)
    {
      if (i > 4)
        return 4;
      if (this.Parent == null)
        return i;
      ++i;
      return this.Parent.GetLevel(i);
    }

    public virtual Node<DisplayItemModel> ToDisplayItemNode(string parentId)
    {
      return (Node<DisplayItemModel>) null;
    }
  }
}
