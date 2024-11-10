// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.AttachmentComparer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  internal class AttachmentComparer : IComparer<AttachmentViewModel>
  {
    public int Compare(AttachmentViewModel left, AttachmentViewModel right)
    {
      if (left == null || right == null)
        return 0;
      if (left.Ordinal > right.Ordinal)
        return 1;
      return left.Ordinal < right.Ordinal ? -1 : AttachmentComparer.CompareCreatedTime(left, right);
    }

    private static int CompareCreatedTime(AttachmentViewModel left, AttachmentViewModel right)
    {
      if (Utils.IsEmptyDate(left.CreatedTime) && !Utils.IsEmptyDate(right.CreatedTime))
        return 1;
      if (!Utils.IsEmptyDate(left.CreatedTime) && Utils.IsEmptyDate(right.CreatedTime))
        return -1;
      if (!Utils.IsEmptyDate(left.CreatedTime) && !Utils.IsEmptyDate(right.CreatedTime))
      {
        DateTime? createdTime = left.CreatedTime;
        if (createdTime.HasValue)
        {
          createdTime = right.CreatedTime;
          if (createdTime.HasValue)
          {
            createdTime = left.CreatedTime;
            DateTime dateTime1 = createdTime.Value;
            ref DateTime local = ref dateTime1;
            createdTime = right.CreatedTime;
            DateTime dateTime2 = createdTime.Value;
            int num = local.CompareTo(dateTime2);
            if (num != 0)
              return -num;
          }
        }
      }
      return 0;
    }
  }
}
