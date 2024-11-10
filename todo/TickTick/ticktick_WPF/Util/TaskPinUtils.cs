// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TaskPinUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class TaskPinUtils
  {
    [Obsolete]
    public static List<DisplayItemModel> GetPinnedInModels(
      List<DisplayItemModel> models,
      HashSet<string> pinnedIds)
    {
      List<DisplayItemModel> pinnedInModels = new List<DisplayItemModel>();
      Dictionary<string, List<DisplayItemModel>> parentChildrenDict = new Dictionary<string, List<DisplayItemModel>>();
      foreach (DisplayItemModel displayItemModel in models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.Status == 0)))
      {
        if (!string.IsNullOrEmpty(displayItemModel.ParentId))
        {
          if (parentChildrenDict.ContainsKey(displayItemModel.ParentId))
            parentChildrenDict[displayItemModel.ParentId].Add(displayItemModel);
          else
            parentChildrenDict[displayItemModel.ParentId] = new List<DisplayItemModel>()
            {
              displayItemModel
            };
        }
        if (displayItemModel.IsPinned)
          pinnedInModels.Add(displayItemModel);
      }
      int count = pinnedInModels.Count;
      for (int index = 0; index < count; ++index)
      {
        DisplayItemModel model = pinnedInModels[index];
        if (!pinnedIds.Contains(model.Id))
        {
          pinnedIds.Add(model.Id);
          List<DisplayItemModel> children = new List<DisplayItemModel>();
          HashSet<string> childIds = new HashSet<string>();
          GetAllChildren(model, children, childIds);
          foreach (DisplayItemModel displayItemModel in children)
          {
            if (!pinnedIds.Contains(displayItemModel.Id))
            {
              pinnedInModels.Add(displayItemModel);
              pinnedIds.Add(displayItemModel.Id);
            }
          }
        }
      }
      return pinnedInModels;

      void GetAllChildren(
        DisplayItemModel model,
        List<DisplayItemModel> children,
        HashSet<string> childIds)
      {
        if (!parentChildrenDict.ContainsKey(model.Id))
          return;
        foreach (DisplayItemModel model1 in parentChildrenDict[model.Id])
        {
          if (!childIds.Contains(model1.Id) && model1.IsPinned)
          {
            childIds.Add(model1.Id);
            children.Add(model1);
            GetAllChildren(model1, children, childIds);
          }
        }
      }
    }
  }
}
